using System;
using System.Collections.Generic;
using System.Fabric.Query;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using Osmosys.Abstractions;
using Osmosys.DataContracts;
using Osmosys.Exceptions;

namespace Osmosys.Role
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
    internal class Role : Actor, IRole
    {
        /// <summary>
        /// Initializes a new instance of Role
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public Role(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        public async Task AddRoleAsync(RoleDto role)
        {
            if (string.IsNullOrWhiteSpace(role?.Name) || string.IsNullOrWhiteSpace(role.ApplicationPath))
                throw new ArgumentNullException(nameof(role));

            if (role.AuthorityPath == null) // allow authority to be root
                role.AuthorityPath = string.Empty;

            await this.StateManager.SetStateAsync("Role." + "." + role.Path, role);
        }

        public async Task AddUserAsync(UserDto user)
        {
            if (user?.Id == null || user.AuthorityPath == null)
                throw new ArgumentNullException(nameof(user));

            var role = await this.StateManager.GetStateAsync<RoleDto>("Role");
            var userRole = new UserRoleDto
            {
                User = user,
                Role = role,
                Approved = false,
            };

            var ur = await this.StateManager.TryGetStateAsync<UserRoleDto>("User." + user.Path);
            if (!ur.HasValue)
            {
                await this.StateManager.SetStateAsync("User." + user.Path, userRole);

                var userProxy = ActorProxy.Create<IUser>(new ActorId(user.Path));
                await userProxy.AddRoleAsync(role);
            }
        }

        public async Task AddViewModelAsync(ViewModelDto viewModel)
        {
            if (string.IsNullOrWhiteSpace(viewModel?.Name))
                throw new ArgumentNullException(nameof(viewModel));

            if (viewModel.ApplicationPath == null)
                viewModel.ApplicationPath = string.Empty;

            await this.StateManager.SetStateAsync("ViewModel." + viewModel.Path, viewModel);
        }

        public async Task ApproveUserAsync(UserDto user)
        {
            if (user?.Id == null || user.AuthorityPath == null)
                throw new ArgumentNullException(nameof(user));

            var role = await this.StateManager.TryGetStateAsync<RoleDto>("Role");
            if (role.HasValue)
            {
                var userPath = user.Path;
                var userRole = await this.StateManager.TryGetStateAsync<UserRoleDto>("User." + userPath);
                if (userRole.HasValue && !userRole.Value.Approved)
                {
                    userRole.Value.Approved = true;
                    await this.StateManager.SetStateAsync("User." + userPath, userRole);
                    var roleProxy = ActorProxy.Create<IRole>(new ActorId(userPath));
                    await roleProxy.ApproveUserAsync(user);
                }
            }
        }

        public async Task ApproveUsersAsync()
        {
            var role = await this.StateManager.TryGetStateAsync<RoleDto>("Role");
            if (role.HasValue)
            {
                var userRoles = await this.ListUsersAsync();
                foreach (var userRole in userRoles.Where(ur => !ur.Approved))
                {
                    var userPath = userRole.User.Path;
                    userRole.Approved = true;
                    await this.StateManager.SetStateAsync("User." + userPath, userRole);
                    var roleProxy = ActorProxy.Create<IRole>(new ActorId(userPath));
                    await roleProxy.ApproveUserAsync(userRole.User);
                }
            }
        }

        /// <summary>
        /// Should only be used from Authority Actor methods
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task CreateAsync(RoleDto role)
        {
            if (string.IsNullOrWhiteSpace(role?.Name) || string.IsNullOrWhiteSpace(role.ApplicationPath))
                throw new ArgumentNullException(nameof(role));

            if (role.AuthorityPath == null) // allow authority to be root
                role.AuthorityPath = string.Empty;

            await this.StateManager.SetStateAsync("Role", role);
        }

        public async Task<RoleDto> ExistsAsync()
        {
            var roleExisting = await this.StateManager.TryGetStateAsync<RoleDto>("Role");
            return roleExisting.HasValue ? roleExisting.Value : null;
        }

        public async Task<bool> HasAccessViewModelAsync(ViewModelDto viewModel)
        {
            if (string.IsNullOrWhiteSpace(viewModel?.Name) || string.IsNullOrWhiteSpace(viewModel.ApplicationPath))
                throw new ArgumentNullException(nameof(viewModel));

            var exists = await this.StateManager.TryGetStateAsync<ViewModelDto>("ViewModel." + viewModel.Path);
            if (exists.HasValue)
                return true;

            var names = await this.StateManager.GetStateNamesAsync();
            foreach (var key in names.Where(k => k.StartsWith("Role.")))
            {
                var role = await this.StateManager.GetStateAsync<RoleDto>(key);
                var roleProxy = ActorProxy.Create<IRole>(new ActorId(role.Name + "." + role.Path));
                var ok = await roleProxy.HasAccessViewModelAsync(viewModel);
                if (ok)
                    return true;
            }
            return false;
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

        public async Task<List<UserRoleDto>> ListUsersAsync()
        {
            var userRoles = new List<UserRoleDto>();
            var names = await this.StateManager.GetStateNamesAsync();
            foreach (var key in names)
            {
                if (key.StartsWith("User."))
                {
                    var userRole = await this.StateManager.GetStateAsync<UserRoleDto>(key);
                    userRoles.Add(userRole);
                }
                else if (key.StartsWith("Role."))
                {
                    var role = await this.StateManager.GetStateAsync<RoleDto>(key);
                    var roleProxy = ActorProxy.Create<IRole>(new ActorId(role.Name + "." + role.Path));
                    userRoles.AddRange(await roleProxy.ListUsersAsync());
                }
            }
            return userRoles.Distinct().ToList();
        }

        public async Task<List<ViewModelDto>> ListViewModelsAsync()
        {
            var viewModels = new List<ViewModelDto>();
            var names = await this.StateManager.GetStateNamesAsync();
            foreach (var key in names)
            {
                if (key.StartsWith("ViewModel."))
                {
                    var viewModel = await this.StateManager.GetStateAsync<ViewModelDto>(key);
                    viewModels.Add(viewModel);
                }
                else if (key.StartsWith("Role."))
                {
                    var role = await this.StateManager.GetStateAsync<RoleDto>(key);
                    var roleProxy = ActorProxy.Create<IRole>(new ActorId(role.Name + "." + role.Path));
                    viewModels.AddRange(await roleProxy.ListViewModelsAsync());
                }
            }
            return viewModels.Distinct().ToList();
        }

        public async Task RemoveRoleAsync(RoleDto role)
        {
            if (string.IsNullOrWhiteSpace(role?.Name) || string.IsNullOrWhiteSpace(role.ApplicationPath))
                throw new ArgumentNullException(nameof(role));

            if (role.AuthorityPath == null) // allow authority to be root
                role.AuthorityPath = string.Empty;

            await this.StateManager.TryRemoveStateAsync("Role." + "." + role.Path);
        }

        public async Task RemoveUserAsync(UserDto user)
        {
            if (user?.Id == null || user.AuthorityPath == null)
                throw new ArgumentNullException(nameof(user));

            var role = await this.StateManager.GetStateAsync<RoleDto>("Role");
            var userPath = user.Path;
            var ur = await this.StateManager.TryGetStateAsync<UserRoleDto>("User." + userPath);
            if (ur.HasValue)
            {
                await this.StateManager.RemoveStateAsync("User." + userPath);

                var userProxy = ActorProxy.Create<IUser>(new ActorId(userPath));
                await userProxy.RemoveRoleAsync(role);
            }
        }

        public async Task RemoveViewModelAsync(ViewModelDto viewModel)
        {
            if (string.IsNullOrWhiteSpace(viewModel?.Name))
                throw new ArgumentNullException(nameof(viewModel));

            if (viewModel.ApplicationPath == null)
                viewModel.ApplicationPath = string.Empty;

            await this.StateManager.TryRemoveStateAsync("ViewModel." + "." + viewModel.Path);
        }

        /// <summary>
        /// Should only be used from Authority Actor methods
        /// </summary>
        /// <param name="fromAuthorityPath"></param>
        /// <param name="toAuthorityPath"></param>
        /// <returns></returns>
        public async Task RepointRolesAsync(string fromAuthorityPath, string toAuthorityPath)
        {
            if (string.IsNullOrWhiteSpace(fromAuthorityPath))
                throw new ArgumentNullException(nameof(fromAuthorityPath));
            if (string.IsNullOrWhiteSpace(toAuthorityPath))
                throw new ArgumentNullException(nameof(toAuthorityPath));
            if (fromAuthorityPath.Length <= toAuthorityPath.Length || !fromAuthorityPath.EndsWith(toAuthorityPath))
                throw new Exception($"Unexpected parameters in Actor method {nameof(RepointRolesAsync)}('{fromAuthorityPath}', '{toAuthorityPath}').");

            var roles = await this.ListRolesAsync();
            foreach (var role in roles.Where(r => r.AuthorityPath == fromAuthorityPath))
            {
                await this.StateManager.RemoveStateAsync("Role." + role.Path);
                role.AuthorityPath = toAuthorityPath;
                await this.StateManager.SetStateAsync("Role." + role.Path, role);
                if (role.RoleType == RoleType.Custom)
                {
                    var applicationAuthorityProxy = ActorProxy.Create<IApplicationAuthority>(new ActorId(toAuthorityPath));
                    await applicationAuthorityProxy.AddCreateRoleAsync(role.Name);
                }
            }
        }

        public async Task<RoleDto> UpdateRoleAsync(RoleDto role)
        {
            if (string.IsNullOrWhiteSpace(role?.Name) || string.IsNullOrWhiteSpace(role.ApplicationPath))
                throw new ArgumentNullException(nameof(role));

            if (role.AuthorityPath == null) // allow authority to be root
                role.AuthorityPath = string.Empty;

            var applicationAuthorityProxy = ActorProxy.Create<IApplicationAuthority>(new ActorId(role.AuthorityPath));
            var applicationAuthority = await applicationAuthorityProxy.ExistsAsync();

            var roleToUpdate = await this.StateManager.TryGetStateAsync<RoleDto>("Role");

            if (applicationAuthority?.AuthorityPath != role.AuthorityPath || !roleToUpdate.HasValue)
                throw new RoleUpdateIllegalException { AuthorityPath = applicationAuthority?.AuthorityPath, Role = role };

            if (roleToUpdate.Value.Version != role.Version)
                throw new RoleVersionMismatchException {Role = role, ExistingRole = roleToUpdate.Value};

            role.Version++;
            await this.StateManager.SetStateAsync("Role", role);

            // update authority
            await applicationAuthorityProxy.UpdateRoleAsync(role);

            // update role in userRole records
            var userRoles = await this.ListUsersAsync();
            foreach (var ur in userRoles)
            {
                ur.Role = role;
                var userPath = ur.User.Path;
                await this.StateManager.SetStateAsync("User." + userPath, ur);
                var userProxy = ActorProxy.Create<IUser>(new ActorId(userPath));
                await userProxy.UpdateRoleAsync(role);
            }

            return role;
        }

        /// <summary>
        /// should only be used from User actor
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task UpdateUserAsync(UserDto user)
        {
            await this.StateManager.SetStateAsync("User." + user.Path, user);
        }

    }
}
