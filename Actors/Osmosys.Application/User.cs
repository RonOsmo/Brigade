using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using Osmosys.Abstractions;
using Osmosys.DataContracts;
using Osmosys.Exceptions;

namespace Osmosys
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class User : Actor, IUser
    {
        private ActorService _actorService;
        private ActorId _actorId;

        /// <summary>
        /// Initializes a new instance of User
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public User(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
            _actorService = actorService;
            _actorId = actorId;
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "User activated.");

            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see https://aka.ms/servicefabricactorsstateserialization

            await this.StateManager.SetStateAsync("LoggedInCount", 0);
        }

        protected override async Task OnDeactivateAsync()
        {
            var loggedInCount = await this.StateManager.GetStateAsync<int>("LoggedInCount");
            if (loggedInCount > 0)
            {
                await this.LogoffUserAsync();
            }
            await base.OnDeactivateAsync();
        }

        public async Task AddRoleAsync(RoleDto role)
        {
            if (string.IsNullOrWhiteSpace(role?.Name) || string.IsNullOrWhiteSpace(role.ApplicationPath))
                throw new ArgumentNullException(nameof(role));

            if (role.AuthorityPath == null) // allow authority to be root
                role.AuthorityPath = string.Empty;

            var user = await this.StateManager.TryGetStateAsync<UserDto>("User");
            if (user.HasValue)
            {
                var ur = await this.StateManager.TryGetStateAsync<UserRoleDto>("Role." + role.Path);
                if (!ur.HasValue)
                {
                    var userRole = new UserRoleDto
                    {
                        User = user.Value,
                        Role = role,
                        Approved = false,
                    };
                    await this.StateManager.SetStateAsync("Role." + role.Path, userRole);
                    var roleProxy = ActorProxy.Create<IRole>(new ActorId(role.Path));
                    await roleProxy.AddUserAsync(user.Value);
                }
            }
        }

        public async Task AddTaskAsync(UserTaskDto item)
        {
            if (string.IsNullOrWhiteSpace(item?.ViewModel?.Name) || string.IsNullOrWhiteSpace(item.ViewModel.ApplicationPath)
                || string.IsNullOrWhiteSpace(item.OwningEntityId) || string.IsNullOrWhiteSpace(item.OwningEntityTypeName) 
                || string.IsNullOrWhiteSpace(item.TaskTypeName))
                throw new ArgumentNullException(nameof(item));

            var viewModel = item.ViewModel;
            var user = await this.StateManager.TryGetStateAsync<UserDto>("User");
            if (user.HasValue)
            {
                var hasAccess = true;
                var viewModelCount = await this.StateManager.TryGetStateAsync<UserViewModelCount>("ViewModel." + viewModel.Path);
                if (!viewModelCount.HasValue)
                {
                    hasAccess = await this.HasAccessViewModelAsync(viewModel);
                }

                if (hasAccess)
                {
                    var userViewModelCount = new UserViewModelCount
                    {
                        ViewModel = viewModel,
                        Count = 1
                    };

                    await
                        this.StateManager.AddOrUpdateStateAsync<UserViewModelCount>("ViewModel." + viewModel.Path, userViewModelCount,
                            (key, usvmc) =>
                            {
                                usvmc.Count++;
                                return usvmc;
                            });

                    var client = ActorProxy.Create<IUsertask>(new ActorId(viewModel.Path + "." + user.Value.Path));
                    await client.AddTaskAsync(item);
                }
            }
        }

        public async Task ApproveRoleAsync(RoleDto role)
        {
            if (string.IsNullOrWhiteSpace(role?.Name) || string.IsNullOrWhiteSpace(role.ApplicationPath))
                throw new ArgumentNullException(nameof(role));

            if (role.AuthorityPath == null) // allow authority to be root
                role.AuthorityPath = string.Empty;

            var user = await this.StateManager.TryGetStateAsync<UserDto>("User");
            if (user.HasValue)
            {
                var userRole = await this.StateManager.TryGetStateAsync<UserRoleDto>("Role." + role.Path);
                if (userRole.HasValue && !userRole.Value.Approved)
                {
                    userRole.Value.Approved = true;
                    await this.StateManager.SetStateAsync("Role." + "." + role.Path, userRole);
                    var roleProxy = ActorProxy.Create<IRole>(new ActorId(role.Path));
                    await roleProxy.ApproveUserAsync(user.Value);
                }
            }
        }

        public async Task ApproveRolesAsync()
        {
            var userRoles = await this.ListRolesAsync();
            foreach (var userRole in userRoles.Where(ur => !ur.Approved))
            {
                await this.ApproveRoleAsync(userRole.Role);
            }
        }

        public async Task<List<UserViewModelCount>> CountTasksAsync()
        {
            var viewModels = new List<UserViewModelCount>();
            var loggedInCount = await this.StateManager.GetOrAddStateAsync("LoggedInCount", 0);
            if (loggedInCount > 0)
            {
                var names = await this.StateManager.GetStateNamesAsync();
                foreach (var name in names.Where(name => name.StartsWith("ViewModel.")))
                {
                    var vm = await this.StateManager.GetStateAsync<UserViewModelCount>(name);
                    if (vm.Count > 0)
                    {
                        viewModels.Add(vm);
                    }
                }
            }
            return viewModels;
        }

        public async Task<UserDto> CreateAsync(UserDto user)
        {
            if (user?.Id == null || user.AuthorityPath == null)
                throw new ArgumentNullException(nameof(user));

            await this.StateManager.SetStateAsync<UserDto>("User", user);
            return user;
        }

        public async Task<bool> HasAccessViewModelAsync(ViewModelDto viewModel)
        {
            if (string.IsNullOrWhiteSpace(viewModel?.Name) || string.IsNullOrWhiteSpace(viewModel.ApplicationPath))
                throw new ArgumentNullException(nameof(viewModel));

            var names = await this.StateManager.GetStateNamesAsync();
            foreach (var name in names.Where(name => name.StartsWith("Role.")))
            {
                var userRole = await this.StateManager.GetStateAsync<UserRoleDto>(name);
                if (!userRole.Approved)
                    continue;
                var roleProxy = ActorProxy.Create<IRole>(new ActorId(userRole.Role.Path));
                var hasAccess = await roleProxy.HasAccessViewModelAsync(viewModel);
                if (hasAccess)
                    return true;
            }

            return false;
        }

        public async Task<bool> IsLoggedInAsync(UserDto user)
        {
            if (user?.Id == null || user.AuthorityPath == null)
                throw new ArgumentNullException(nameof(user));

            await this.StateManager.SetStateAsync<UserDto>("User", user);
            var loggedInCount = await this.StateManager.GetStateAsync<int>("LoggedInCount");
            return loggedInCount > 0;
        }

        public async Task<List<ClientUsageDto>> ListClientUsageAsync()
        {
            var clientUsages = new List<ClientUsageDto>();
            var enumerator = await this.StateManager.GetStateNamesAsync();
            foreach (var clientId in enumerator.Where(name => name.StartsWith("Client.")))
            {
                var clientUsage = await this.StateManager.GetStateAsync<ClientUsageDto>(clientId);
                clientUsages.Add(clientUsage);
            }
            return clientUsages;
        }

        public async Task<List<UserRoleDto>> ListRolesAsync()
        {
            var result = new List<UserRoleDto>();
            var names = await this.StateManager.GetStateNamesAsync();
            foreach (var name in names.Where(name => name.StartsWith("Role.")))
            {
                result.Add(await this.StateManager.GetStateAsync<UserRoleDto>(name));
            }
            return result;
        }

        public async Task<List<UserTaskDto>> ListTasksAsync(ViewModelDto viewModel)
        {
            if (string.IsNullOrWhiteSpace(viewModel?.Name) || string.IsNullOrWhiteSpace(viewModel.ApplicationPath))
                throw new ArgumentNullException(nameof(viewModel));

            var loggedInCount = await this.StateManager.GetOrAddStateAsync("LoggedInCount", 0);
            if (loggedInCount > 0)
            {
                var user = await this.StateManager.GetStateAsync<UserDto>("User");
                var usersViewModel = await this.StateManager.TryGetStateAsync<UserViewModelCount>("ViewModel." + viewModel.Path);
                if (usersViewModel.HasValue && usersViewModel.Value.Count > 0)
                {
                    var client = ActorProxy.Create<IUsertask>(new ActorId(viewModel.Path + "." + user.Path));
                    return await client.ListTasksAsync();
                }
            }
            return new List<UserTaskDto>();
        }

        public async Task<List<ViewModelDto>> ListViewModelsAsync()
        {
            var viewModels = new List<ViewModelDto>();
            var userRoles = await this.ListRolesAsync();
            foreach (var userRole in userRoles)
            {
                var roleProxy = ActorProxy.Create<IRole>(new ActorId(userRole.Role.Path));
                var vms = await roleProxy.ListViewModelsAsync();
                viewModels.AddRange(vms);
            }
            return viewModels.Distinct().ToList();
        }

        public async Task LoginClientAsync(PlatformDto platform, ApplicationDto application)
        {
            if (string.IsNullOrWhiteSpace(platform?.PlatformName) || string.IsNullOrWhiteSpace(platform.PlatformVersion))
                throw new ArgumentNullException(nameof(platform));
            if (string.IsNullOrWhiteSpace(application?.Id) || string.IsNullOrWhiteSpace(application.Name) ||
                string.IsNullOrWhiteSpace(application.ThisVersion))
                throw new ArgumentNullException(nameof(application));

            var user = await this.StateManager.GetStateAsync<UserDto>("User");
            var clientUsage = new ClientUsageDto
            {
                Platform = platform,
                Application = application,
                LastLoginDateTime = DateTime.Now,
                LoginCount = 1,
            };

            await this.StateManager.AddOrUpdateStateAsync("Client." + clientUsage.Path, clientUsage,
                (k, v) =>
                {
                    v.LastLoginDateTime = DateTime.Now;
                    v.LoginCount++;
                    return v;
                });

            await this.StateManager.AddOrUpdateStateAsync("LoggedInCount", 0,
                (k, v) =>
                {
                    v++;
                    return v;
                });

            var clientProxy = ActorProxy.Create<IApplicationAuthority>(new ActorId(application.Path + "." + user.AuthorityPath));
            await clientProxy.LoginUserAsync(platform, user);
        }

        public async Task LogoffClientAsync(PlatformDto platform, ApplicationDto application)
        {
            if (string.IsNullOrWhiteSpace(platform?.PlatformName) || string.IsNullOrWhiteSpace(platform.PlatformVersion))
                throw new ArgumentNullException(nameof(platform));
            if (string.IsNullOrWhiteSpace(application?.Id) || string.IsNullOrWhiteSpace(application.Name) ||
                string.IsNullOrWhiteSpace(application.ThisVersion))
                throw new ArgumentNullException(nameof(application));

            var clientCount = 0;
            await this.StateManager.AddOrUpdateStateAsync("LoggedInCount", 0,
                (k, v) =>
                {
                    if (v > 0)
                        --v;
                    return clientCount = v;
                });

            if (clientCount > 0)
            {
                var user = await this.StateManager.GetStateAsync<UserDto>("User");

                var clientUsage = new ClientUsageDto
                {
                    Platform = platform,
                    Application = application,
                    LastLogoffDateTime = DateTime.Now,
                    LoginCount = 0,
                };

                await this.StateManager.AddOrUpdateStateAsync("Client." + clientUsage.Path, clientUsage,
                    (k, v) =>
                    {
                        v.LastLogoffDateTime = DateTime.Now;
                        v.LoggedInTimeSpan += v.LastLogoffDateTime - v.LastLoginDateTime;
                        return v;
                    });

                var clientProxy = ActorProxy.Create<IApplicationAuthority>(new ActorId(application.Path + "." + user.AuthorityPath));
                await clientProxy.LoginUserAsync(platform, user);
            }
        }

        public async Task LogoffUserAsync()
        {
            var loggedInCount = 0;
            await this.StateManager.AddOrUpdateStateAsync("LoggedInCount", loggedInCount,
                (k, v) =>
                {
                    if (v > 0)
                    {
                        loggedInCount = v;
                        v = 0;
                    }
                    return v;
                });

            if (loggedInCount > 0)
            {
                var user = await this.StateManager.GetStateAsync<UserDto>("User");
                var clients = await this.StateManager.GetStateNamesAsync();

                foreach (var key in clients.Where(id => id.StartsWith("Client.")))
                {
                    var clientUsage = await this.StateManager.GetStateAsync<ClientUsageDto>(key);
                    if (clientUsage.LastLoginDateTime > clientUsage.LastLogoffDateTime)
                    {
                        await this.StateManager.AddOrUpdateStateAsync(key, clientUsage, (k, v) =>
                        {
                            v.LastLogoffDateTime = DateTime.Now;
                            v.LoggedInTimeSpan += v.LastLogoffDateTime - v.LastLoginDateTime;
                            return v;
                        });

                        var clientProxy = ActorProxy.Create<IApplicationAuthority>(new ActorId(clientUsage.Application.Path + "." + user.AuthorityPath));
                        await clientProxy.LogoffUserAsync(clientUsage.Platform, user);
                    }
                }
            }
        }

        public async Task RemoveTaskAsync(ViewModelDto viewModel, string owningEntityTypeName, string owningEntityId)
        {
            if (string.IsNullOrWhiteSpace(viewModel?.Name) || string.IsNullOrWhiteSpace(viewModel.ApplicationPath))
                throw new ArgumentNullException(nameof(viewModel));
            if (string.IsNullOrWhiteSpace(owningEntityTypeName))
                throw new ArgumentNullException(nameof(owningEntityTypeName));
            if (string.IsNullOrWhiteSpace(owningEntityId))
                throw new ArgumentNullException(nameof(owningEntityId));

            var user = await this.StateManager.TryGetStateAsync<UserDto>("User");
            if (user.HasValue)
            {
                var viewModelPath = viewModel.Path;
                var clientProxy = ActorProxy.Create<IUsertask>(new ActorId(viewModelPath + "." + user.Value.Path));
                var userViewModelCount = new UserViewModelCount
                {
                    ViewModel = viewModel,
                    Count = await clientProxy.RemoveTaskAsync(owningEntityTypeName, owningEntityId),
                };

                await this.StateManager.SetStateAsync<UserViewModelCount>("ViewModel." + viewModelPath, userViewModelCount);
            }

        }

        public async Task RemoveRoleAsync(RoleDto role)
        {
            if (string.IsNullOrWhiteSpace(role?.Name) || string.IsNullOrWhiteSpace(role.ApplicationPath))
                throw new ArgumentNullException(nameof(role));

            if (role.AuthorityPath == null) // allow authority to be root
                role.AuthorityPath = string.Empty;

            var ok = await this.StateManager.TryRemoveStateAsync("Role." + role.Path);
            if (ok)
            {
                var user = await this.StateManager.GetStateAsync<UserDto>("User");
                var clientProxy = ActorProxy.Create<IRole>(new ActorId(role.Path));
                await clientProxy.RemoveUserAsync(user);

                // re-evaluate viewModel access and delete task lists for ones no longer available
                var taskCounts = await this.CountTasksAsync();
                foreach (var counter in taskCounts)
                {
                    var access = await this.HasAccessViewModelAsync(counter.ViewModel);
                    if (access)
                        continue;

                    var taskList = await this.ListTasksAsync(counter.ViewModel);
                    foreach (var task in taskList)
                    {
                        await this.RemoveTaskAsync(task.ViewModel, task.OwningEntityTypeName, task.OwningEntityId);
                    }
                }
            }
        }

        public async Task UpdateRoleAsync(RoleDto role)
        {
            await this.StateManager.SetStateAsync("Role." + role.Path, role);
        }

        public async Task<UserDto> UpdateUserAsync(UserDto user)
        {
            if (user?.Id == null || string.IsNullOrWhiteSpace(user.AuthorityPath))
                throw new ArgumentNullException(nameof(user));

            var authorityProxy = ActorProxy.Create<IAuthority>(new ActorId(user.AuthorityPath));
            var authority = await authorityProxy.ExistsAsync();
            var userToUpdate = await this.StateManager.TryGetStateAsync<UserDto>("User");

            if (authority?.Path != user.AuthorityPath || !userToUpdate.HasValue)
                throw new UserUpdateIllegalException { Authority = authority, User = user };

            if (userToUpdate.Value.Version != user.Version)
                throw new UserVersionMismatchException { User = user, ExistingUser = userToUpdate.Value };

            user.Version++;
            await this.StateManager.SetStateAsync("User", user);

            // update user in userRole records
            var applications = new Dictionary<string,string>();
            var userRoles = await this.ListRolesAsync();
            foreach (var userRole in userRoles)
            {
                var applicationPath = userRole.Role.ApplicationPath;
                if (!applications.ContainsKey(applicationPath))
                {
                    // update applicationAuthority
                    applications[applicationPath] = applicationPath;
                    var applicationAuthorityProxy =
                        ActorProxy.Create<IApplicationAuthority>(new ActorId(applicationPath + "." + user.AuthorityPath));
                    await applicationAuthorityProxy.UpdateUserAsync(user);
                }

                userRole.User = user;
                var rolePath = userRole.Role.Path;
                await this.StateManager.SetStateAsync("Role." + rolePath, userRole);
                var roleProxy = ActorProxy.Create<IRole>(new ActorId(rolePath));
                await roleProxy.UpdateUserAsync(user);
            }

            return user;
        }
    }
}
