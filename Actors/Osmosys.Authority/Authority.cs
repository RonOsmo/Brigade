using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Data;
using Osmosys.Abstractions;
using Osmosys.DataContracts;
using Osmosys.Exceptions;

namespace Osmosys.Authority
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
    internal class Authority : Actor, IAuthority
    {
        private readonly Uri _roleServiceUri = new Uri("fabric:/Roles");
        private readonly Uri _authorityServiceUri = new Uri("fabric:/Authorities");

        /// <summary>
        /// Initializes a new instance of Authority
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public Authority(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        public async Task<AuthorityDto> AddCreateChildAsync(AuthorityDto child)
        {
            if (string.IsNullOrWhiteSpace(child?.Id) || string.IsNullOrWhiteSpace(child.ContainerId))
                throw new ArgumentNullException(nameof(child));

            var childPath = child.Path;
            var parent = await this.StateManager.GetStateAsync<AuthorityDto>("Authority");
            var parentPath = parent.Path;
            if (!childPath.EndsWith(parentPath))
                throw new IsNotAChildException {Child = child, Parent = parent};

            var childAuthorityProxy = ActorProxy.Create<IAuthority>(new ActorId(childPath));
            child = await childAuthorityProxy.CreateAsync(child);
            await childAuthorityProxy.SetParentPathAsync(parentPath);
            //await childAuthorityProxy.CreateUpdateApplicationRolesAsync();

            // handle insert - a new child between me and one or more of my children
            var names = await this.StateManager.GetStateNamesAsync();
            foreach (var name in names.Where(n => n.StartsWith("Child.") && n.Length - 6 > childPath.Length && n.EndsWith(childPath)))
            {
                var grandChild = await this.StateManager.GetStateAsync<AuthorityDto>(name);

                // move grandChildren roles that point to the parent now should go to this child
                var grandChildProxy = ActorProxy.Create<IAuthority>(new ActorId(grandChild.Path));
                await grandChildProxy.SetParentPathAsync(childPath);
                var grandChildRoles = await grandChildProxy.ListRolesAsync();
                foreach (var grandChildRole in grandChildRoles)
                {
                    var grandChildRoleProxy = ActorProxy.Create<IRole>(new ActorId(grandChildRole.Path));
                    await grandChildRoleProxy.RepointRolesAsync(parent.ContainerId, child.ContainerId);
                }
            }

            await this.StateManager.SetStateAsync("Child." + childPath, child);
            return child;
        }


        public async Task<AuthorityDto> CreateAsync(AuthorityDto authority)
        {
            if (authority.Id == null)
                authority.Id = string.Empty;
            if (authority.ContainerId == null)
                authority.ContainerId = string.Empty;

            var prevAuthority = await this.StateManager.TryGetStateAsync<AuthorityDto>("Authority");
            if (prevAuthority.HasValue)
                throw new AuthorityAlreadyExistsException {Authority = prevAuthority.Value};

            await this.StateManager.SetStateAsync("Authority", authority);
            return authority;
        }

        public async Task<AuthorityDto> ExistsAsync()
        {
            var authorityExisting = await this.StateManager.TryGetStateAsync<AuthorityDto>("Authority");
            if (authorityExisting.HasValue)
                return authorityExisting.Value;
            return null;
        }

        public async Task<AuthorityDto> GetParentAsync()
        {
            var parentPath = await this.StateManager.GetStateAsync<string>("ParentPath");
            var parentAuthorityProxy = ActorProxy.Create<IAuthority>(new ActorId(parentPath));
            var parentAuthority = await parentAuthorityProxy.ExistsAsync();
            return parentAuthority;
        }

        public async Task<RoleDto> GetStandardRoleAsync(RoleType stdRoleType)
        {
            var authority = await this.StateManager.GetStateAsync<AuthorityDto>("Authority");

            var roles = await this.ListRolesAsync();
            return roles.FirstOrDefault(r => r.RoleType == stdRoleType);
        }

        public async Task<RoleDto> GetApplicationRoleAsync(ApplicationDto application, string roleName)
        {
            var roles = await this.ListRolesAsync();
            foreach (var role in roles.Where(r => r.Name == roleName))
            {
                return role;
            }
            return null;
        }

        public async Task<List<AuthorityDto>> ListChildrenAsync()
        {
            var authorities = new List<AuthorityDto>();
            var names = await this.StateManager.GetStateNamesAsync();
            foreach (var key in names.Where(k => k.StartsWith("Child.")))
            {
                var authority = await this.StateManager.GetStateAsync<AuthorityDto>(key);
                authorities.Add(authority);
            }
            return authorities;
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

        public async Task RemoveDeleteChildAsync(AuthorityDto child)
        {
            var ok = await this.StateManager.TryRemoveStateAsync("Child." + child.Path);
            if (!ok)
                return;

            //remove the child actor
            var childProxy = ActorProxy.Create<IAuthority>(new ActorId(child.Path));
            var users = await childProxy.ListUsersAsync();
            if (users.Count > 0)
                throw new ChildHasUsersException {Child = child, UserCount = users.Count};

            var authority = await this.StateManager.GetStateAsync<AuthorityDto>("Authority");

            // move any grandChildren to this authority
            var grandChildren = await childProxy.ListChildrenAsync();
            foreach (var grandChild in grandChildren)
            {
                var grandChildProxy = ActorProxy.Create<IAuthority>(new ActorId(grandChild.Path));
                await grandChildProxy.SetParentPathAsync(authority.Path);
                var grandChildRoles = await grandChildProxy.ListRolesAsync();

                // move grandChild's roles that point to the child Container to the authority's Container.
                foreach (var grandChildRole in grandChildRoles)
                {
                    var grandChildRoleProxy = ActorProxy.Create<IRole>(new ActorId(grandChildRole.Path));
                    await grandChildRoleProxy.RepointRolesAsync(child.ContainerId, authority.ContainerId);
                }
            }

            // remove all role actors
            var roles = await childProxy.ListRolesAsync();
            foreach (var role in roles)
            {
                var actorRole = new ActorId(role.Path);
                var roleServiceProxy = ActorServiceProxy.Create(_roleServiceUri, actorRole);
                await roleServiceProxy.DeleteActorAsync(actorRole, new CancellationToken());
            }

            // remove child actor
            var actorChild = new ActorId(child.Path);
            var actorServiceProxy = ActorServiceProxy.Create(_authorityServiceUri, actorChild);
            await actorServiceProxy.DeleteActorAsync(actorChild, new CancellationToken());
        }

        public async Task SetParentPathAsync(string parentPath)
        {
            await this.StateManager.SetStateAsync("ParentPath", parentPath);
        }

        public async Task UpdateRoleAsync(RoleDto role)
        {
            await this.StateManager.SetStateAsync("Role." + role.Path, role);
        }

        public async Task UpdateUserAsync(UserDto user)
        {
            await this.StateManager.SetStateAsync("User." + user.Path, user);
        }

        public Task AddCreateRoleAsync(ApplicationDto application, string roleName)
        {
            throw new NotImplementedException();
        }

        public Task<UserDto> AddCreateUserAsync(UserApplicationDto user)
        {
            throw new NotImplementedException();
        }

        public Task<UserCountDto> CountUsersAsync()
        {
            throw new NotImplementedException();
        }

        public Task LoginUserAsync(PlatformDto platform, UserApplicationDto userApplication)
        {
            throw new NotImplementedException();
        }

        public Task LogoffUserAsync(PlatformDto platform, UserApplicationDto userApplication)
        {
            throw new NotImplementedException();
        }
    }
}
