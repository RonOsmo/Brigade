using System;
using System.Collections.Generic;
using System.Linq;
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
    internal class ApplicationAuthority : Actor, IApplicationAuthority
    {
        private readonly Uri _roleServiceUri = new Uri("fabric:/Roles");
        private ActorService _actorService;

        /// <summary>
        /// Initializes a new instance of ApplicationAuthority
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public ApplicationAuthority(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
            _actorService = actorService;
        }

        public async Task<RoleDto> AddCreateRoleAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentNullException(nameof(roleName));

            var applicationAuthority = await this.StateManager.GetStateAsync<ApplicationAuthorityDto>("ApplicationAuthority");

            var authorityProxy = ActorProxy.Create<IAuthority>(new ActorId(applicationAuthority.AuthorityPath));
            var authority = await authorityProxy.GetParentAsync();

            var role = new RoleDto
            {
                Name = roleName,
                ApplicationPath = applicationAuthority.ApplicationPath,
                AuthorityPath = applicationAuthority.AuthorityPath,
                Inherits = !string.IsNullOrWhiteSpace(authority.ContainerId),
                RoleType = RoleType.Custom,
            };

            var roleProxy = ActorProxy.Create<IRole>(new ActorId(role.Path));
            await roleProxy.CreateAsync(role);

            await this.StateManager.SetStateAsync("Role." + role.Path, role);

            if (role.Inherits)
                await AddCreateLinkToInheritedRoleAsync(role, roleProxy, authority);

            return role;
        }

        private async Task AddCreateRoleAsync(RoleDto role)
        {
            if (string.IsNullOrWhiteSpace(role?.Name))
                throw new ArgumentNullException(nameof(role));

            var authorityProxy = ActorProxy.Create<IAuthority>(new ActorId(role.AuthorityPath));
            var authority = await authorityProxy.GetParentAsync();

            var roleProxy = ActorProxy.Create<IRole>(new ActorId(role.Path));
            await roleProxy.CreateAsync(role);

            await this.StateManager.SetStateAsync("Role." + role.Path, role);

            if (role.Inherits)
                await AddCreateLinkToInheritedRoleAsync(role, roleProxy, authority);
        }

        private async Task AddCreateLinkToInheritedRoleAsync(RoleDto role, IRole roleProxy, AuthorityDto authority)
        {
            var parentApplicationAuthorityProxy = ActorProxy.Create<IApplicationAuthority>(new ActorId(role.ApplicationPath + "." + authority.Path));
            var parentRole = (role.RoleType == RoleType.Custom)
                ? await parentApplicationAuthorityProxy.GetRoleAsync(role.Name)
                : await parentApplicationAuthorityProxy.GetStandardRoleAsync(role.RoleType);

            if (parentRole != null)
            {
                await roleProxy.AddRoleAsync(parentRole);
            }
        }

        public async Task<UserDto> AddCreateUserAsync(UserDto user)
        {
            var applicationAuthority = await this.StateManager.GetStateAsync<ApplicationAuthorityDto>("ApplicationAuthority");

            var newUser = await this.StateManager.TryGetStateAsync<UserDto>("User." + user.Path);
            if (newUser.HasValue)
                return newUser.Value;

            var userActorProxy = ActorProxy.Create<IUser>(new ActorId(user.Path));
            user = await userActorProxy.CreateAsync(user);

            return user;
        }

        public async Task<List<UserPlatformCountDto>> CountUsersAsync()
        {
            var userCounts = new List<UserPlatformCountDto>();
            var keys = await this.StateManager.GetStateNamesAsync();
            foreach (var key in keys.Where(k => k.StartsWith("UserCount.")))
            {
                var userCount = await this.StateManager.GetStateAsync<UserPlatformCountDto>(key);
                userCounts.Add(userCount);
            }
            return userCounts;
        }

        public async Task<ApplicationAuthorityDto> CreateAsync(ApplicationDto application, AuthorityDto authority)
        {
            if (string.IsNullOrWhiteSpace(application?.Id) || string.IsNullOrWhiteSpace(application.Name) ||
                string.IsNullOrWhiteSpace(application.ThisVersion))
                throw new ArgumentNullException(nameof(application));

            var applicationAuthority = new ApplicationAuthorityDto {ApplicationPath = application.VersionPath, AuthorityPath = authority.Path};
            var prevApplicationAuthority = await this.StateManager.TryGetStateAsync<ApplicationAuthorityDto>("ApplicationAuthority");
            if (prevApplicationAuthority.HasValue)
                throw new ApplicationAuthorityAlreadyExistsException
                {
                    ApplicationAuthority = applicationAuthority,
                    PrevApplicationAuthority = prevApplicationAuthority.Value
                };

            await this.StateManager.SetStateAsync("ApplicationAuthority", applicationAuthority);
            return applicationAuthority;
        }

        public async Task CreateUpdateRolesAsync(ApplicationDto newVersion)
        {
            if (string.IsNullOrWhiteSpace(newVersion?.ThisVersion))
                throw new ArgumentNullException(nameof(newVersion));

            var applicationAuthority = await this.StateManager.GetStateAsync<ApplicationAuthorityDto>("ApplicationAuthority");

            if (newVersion.VersionPath != applicationAuthority.ApplicationPath)
            {
                await this.StateManager.AddOrUpdateStateAsync("ApplicationAuthority", applicationAuthority, (k, v) =>
                {
                    v.ApplicationPath = newVersion.VersionPath;
                    return v;
                });
            }

            var myRoles = await this.ListRolesAsync();
            var myStdRoles = myRoles.Where(role => role.IsApplicationRole && role.RoleType != RoleType.Custom).ToDictionary(role => role.Id);
            var myCustomRoles = myRoles.Where(role => role.IsApplicationRole && role.RoleType == RoleType.Custom).ToDictionary(role => role.Name);

            var applicationProxy = ActorProxy.Create<IApplication>(new ActorId(newVersion.VersionPath));
            var appRoles = await applicationProxy.ListRolesAsync();

            foreach (var role in appRoles.Where(r => r.RoleType != RoleType.Custom && !myStdRoles.ContainsKey(r.Id)))
            {
                myStdRoles.Remove(role.Id);
                role.AuthorityPath = applicationAuthority.AuthorityPath;
                role.IsApplicationRole = true;
                await this.AddCreateRoleAsync(role);
            }

            foreach (var role in appRoles.Where(r => r.RoleType == RoleType.Custom && !myCustomRoles.ContainsKey(r.Name)))
            {
                myCustomRoles.Remove(role.Name);
                role.AuthorityPath = applicationAuthority.AuthorityPath;
                role.IsApplicationRole = true;
                await this.AddCreateRoleAsync(role);
            }

            // cleanup obsolete roles
            foreach (var role in myStdRoles.Values)
            {
                await this.RemoveDeleteRoleAsync(role);
            }
            foreach (var role in myCustomRoles.Values)
            {
                await this.RemoveDeleteRoleAsync(role);
            }
        }

        public async Task<ApplicationAuthorityDto> ExistsAsync()
        {
            var applicationAuthority =
                await this.StateManager.TryGetStateAsync<ApplicationAuthorityDto>("ApplicationAuthority");

            return (applicationAuthority.HasValue) ? applicationAuthority.Value : null;
        }

        public async Task<RoleDto> GetRoleAsync(string roleName)
        {
            var roles = await this.ListRolesAsync();
            return roles.FirstOrDefault(role => role.Name == roleName);
        }

        public async Task<RoleDto> GetStandardRoleAsync(RoleType stdRoleType)
        {
            var roles = await this.ListRolesAsync();
            return roles.FirstOrDefault(role => role.RoleType == stdRoleType);
        }

        public async Task<List<RoleDto>> ListRolesAsync()
        {
            var roles = new List<RoleDto>();
            var names = await this.StateManager.GetStateNamesAsync();
            foreach (var key in names.Where(k => k.StartsWith("Role.")))
            {
                var role = await this.StateManager.GetStateAsync<RoleDto>(key);
                roles.Add(role);
            }
            return roles;
        }

        public async Task<List<UserDto>> ListUsersAsync()
        {
            var users = new List<UserDto>();
            var names = await this.StateManager.GetStateNamesAsync();
            foreach (var key in names.Where(k => k.StartsWith("User.")))
            {
                var user = await this.StateManager.GetStateAsync<UserDto>(key);
                users.Add(user);
            }
            return users;
        }

        public async Task LoginUserAsync(PlatformDto platform, UserDto user)
        {
            if (string.IsNullOrWhiteSpace(platform?.PlatformName) || string.IsNullOrWhiteSpace(platform.PlatformVersion))
                throw new ArgumentNullException(nameof(platform));

            var applicationAuthority =
                await this.StateManager.GetStateAsync<ApplicationAuthorityDto>("ApplicationAuthority");
            
            var existsUser = await this.StateManager.TryGetStateAsync<UserDto>("User." + user.Path);
            if (!existsUser.HasValue)
                throw new UserHasNoApplicationAccessException
                {
                    User = user,
                    ApplicationPath = applicationAuthority.ApplicationPath
                };

            var platformUsage = new PlatformUsageDto
            {
                Platform = platform,
                LastLoginDateTime = DateTime.Now,
                LoginCount = 1,
            };
            await this.StateManager.AddOrUpdateStateAsync("UserLogin." + platform.Path + "." + user.Id, platformUsage,
                (k, v) =>
                {
                    if (v.LoginCount == 0)
                        v.LastLoginDateTime = DateTime.Now;
                    v.LastLogoffDateTime = null;
                    v.LoginCount++;
                    return v;
                });

            var userCount = new UserPlatformCountDto
            {
                PlatformPath = platform.Path,
                LoggedInUserCount = 0,
                MaxLoggedInUserCount = 1,
                TotalTime = TimeSpan.Zero,
            };
            await this.StateManager.AddOrUpdateStateAsync("UserCount." + platform.Path, userCount, (k, v) =>
            {
                if (++v.LoggedInUserCount > v.MaxLoggedInUserCount)
                    v.MaxLoggedInUserCount = v.MaxLoggedInUserCount;
                return v;
            });

            var applicationProxy = ActorProxy.Create<IApplication>(new ActorId(applicationAuthority.ApplicationPath));
            await applicationProxy.LoginUserAsync(platform, user);
        }

        public async Task LogoffUserAsync(PlatformDto platform, UserDto user)
        {
            if (string.IsNullOrWhiteSpace(platform?.PlatformName) || string.IsNullOrWhiteSpace(platform.PlatformVersion))
                throw new ArgumentNullException(nameof(platform));

            var applicationAuthority =
                await this.StateManager.GetStateAsync<ApplicationAuthorityDto>("ApplicationAuthority");

            var existsUser = await this.StateManager.TryGetStateAsync<UserDto>("User." + user.Path);
            if (!existsUser.HasValue)
                throw new UserHasNoApplicationAccessException
                {
                    User = user,
                    ApplicationPath = applicationAuthority.ApplicationPath
                };

            var platformUsage = new PlatformUsageDto
            {
                Platform = platform,
                LastLogoffDateTime = DateTime.Now,
                LoginCount = 0,
            };
            TimeSpan ts = TimeSpan.Zero;
            await this.StateManager.AddOrUpdateStateAsync("UserLogin." + platform.Path + "." + user.Id, platformUsage,
                (k, usage) =>
                {
                    if (--usage.LoginCount == 0)
                    {
                        usage.LastLogoffDateTime = DateTime.Now;
                        if (usage.LastLoginDateTime.HasValue)
                            ts = usage.LastLogoffDateTime.Value - usage.LastLoginDateTime.Value;
                        usage.LoggedInTimeSpan = ts;
                    }
                    return usage;
                });

            var userCount = new UserPlatformCountDto
            {
                PlatformPath = platform.Path,
                LoggedInUserCount = 0,
                MaxLoggedInUserCount = 1,
                TotalTime = TimeSpan.Zero,
            };
            await this.StateManager.AddOrUpdateStateAsync("UserCount." + platform.Path, userCount,
                (k, v) =>
                {
                    if (--v.LoggedInUserCount == 0)
                    {
                        v.TotalTime = v.TotalTime.Add(ts);
                    }
                    return v;
                });

            var applicationProxy = ActorProxy.Create<IApplication>(new ActorId(applicationAuthority.ApplicationPath));
            await applicationProxy.LoginUserAsync(platform, user);
        }

        public async Task RemoveDeleteRoleAsync(RoleDto role)
        {
            var ok = await this.StateManager.TryRemoveStateAsync("Role." + role.Path);
            if (!ok)
                return;

            var actorRoleId = new ActorId(role.Path);
            var roleProxy = ActorProxy.Create<IRole>(actorRoleId);

            // remove users from role
            var userRoles = await roleProxy.ListUsersAsync();
            foreach (var userRole in userRoles)
            {
                await roleProxy.RemoveUserAsync(userRole.User);

                // TODO: fix up tasks awaiting the user which she does not have access anymore
                var userProxy = ActorProxy.Create<IUser>(new ActorId(userRole.User.Path));
            }

            // remove the role actor
            var roleServiceProxy = ActorServiceProxy.Create(_roleServiceUri, actorRoleId);
            await roleServiceProxy.DeleteActorAsync(actorRoleId, new CancellationToken());
        }

        public async Task RemoveDeleteUserAsync(UserDto user)
        {
            var ok = await this.StateManager.TryRemoveStateAsync("User." + user.Path);
            if (!ok)
                return;

            var applicationAuthority =
                await this.StateManager.GetStateAsync<ApplicationAuthorityDto>("ApplicationAuthority");
            var userActorId = new ActorId(applicationAuthority.ApplicationPath + "." + user.Path);
            var userProxy = ActorProxy.Create<IUser>(userActorId);

            // empty task lists for the user
            var viewModelCount = await userProxy.CountTasksAsync();
            foreach (var counter in viewModelCount)
            {
                var viewModel = counter.ViewModel;
                var tasks = await userProxy.ListTasksAsync(viewModel);
                foreach (var task in tasks)
                {
                    await userProxy.RemoveTaskAsync(viewModel, task.OwningEntityTypeName, task.OwningEntityId);
                }
            }

            // remove roles for the user
            var roles = await userProxy.ListRolesAsync();
            foreach (var userRole in roles)
            {
                await userProxy.RemoveRoleAsync(userRole.Role);
            }

            // remove user's actor 
            var userServiceProxy = ActorServiceProxy.Create(new Uri("fabric:/Users"), userActorId);
            await userServiceProxy.DeleteActorAsync(userActorId, new CancellationToken());
        }

        public async Task RepointInheritedRolesAsync(string newAuthorityPath)
        {
            var applicationAuthority =
                await this.StateManager.GetStateAsync<ApplicationAuthorityDto>("ApplicationAuthority");
            var roles = await this.ListRolesAsync();
            foreach (var role in roles.Where(r => r.Inherits))
            {
                var roleProxy = ActorProxy.Create<IRole>(new ActorId(role.Path));
                await roleProxy.RepointRolesAsync(applicationAuthority.AuthorityPath, newAuthorityPath);
                await this.StateManager.AddOrUpdateStateAsync("Role." + role.Path, role,
                    (k, v) =>
                    {
                        v.AuthorityPath = applicationAuthority.AuthorityPath;
                        return v;
                    });
            }
        }

        public async Task UpdateRoleAsync(RoleDto role)
        {
            await this.StateManager.SetStateAsync("Role." + role.Path, role);
        }

        public async Task UpdateUserAsync(UserDto user)
        {
            await this.StateManager.SetStateAsync("User." + user.Path, user);
        }
    }
}
