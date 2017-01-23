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
    internal class Authority : Actor, IAuthority
    {
        private readonly Uri _roleServiceUri = new Uri("fabric:/Applications/RoleActorService");
        private readonly Uri _authorityServiceUri = new Uri("fabric:/Applications/AuthorityActorService");
        private readonly Uri _applicationAuthorityServiceUri = new Uri("fabric:/Applications/ApplicationAuthorityActorService");
        private ActorService _actorService;
        private ActorId _actorId;


        /// <summary>
        /// Initializes a new instance of Authority
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public Authority(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
            _actorService = actorService;
            _actorId = actorId;
        }

        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, $"Authority {_actorId} with ServiceName: '{_actorService.ActorTypeInformation.ServiceName}' activated.");
            return Task.Delay(1);
            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see https://aka.ms/servicefabricactorsstateserialization

            //await this.StateManager.SetStateAsync("LoggedInCount", 0);
        }

        protected override async Task OnDeactivateAsync()
        {
            await base.OnDeactivateAsync();
        }

        public async Task AddApplicationAsync(ApplicationDto application)
        {
            var exists = await this.StateManager.TryGetStateAsync<ApplicationDto>("Application." + application.Path);
            if (exists.HasValue)
                return;

            await this.StateManager.SetStateAsync("Application." + application.Path, application);
            var authority = await this.StateManager.GetStateAsync<AuthorityDto>("Authority");
            var applicationAuthorityProxy =
                ActorProxy.Create<IApplicationAuthority>(new ActorId(application.Path + "." + authority.Path));
            var applicationAuthority = await applicationAuthorityProxy.CreateAsync(application, authority);
            if (applicationAuthority == null)
                return;

            await applicationAuthorityProxy.CreateUpdateRolesAsync(application);

            var parentPath = await this.StateManager.TryGetStateAsync<string>("ParentPath");
            if (!parentPath.HasValue || string.IsNullOrWhiteSpace(parentPath.Value))
                return;

            // repoint my children's inherited roles to me
            var children = await this.ListChildrenAsync();
            foreach (var child in children)
            {
                var childApplicationAuthorityProxy =
                    ActorProxy.Create<IApplicationAuthority>(new ActorId(application.Path + "." + child.Path));
                await childApplicationAuthorityProxy.RepointInheritedRolesAsync(authority.Path);
            }
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

            // handle insert - a new child between me and one or more of my children
            var children = await this.ListChildrenAsync();
            foreach (var grandChild in children.Where(n => n.Path.EndsWith(childPath)))
            {
                var grandChildProxy = ActorProxy.Create<IAuthority>(new ActorId(grandChild.Path));
                await grandChildProxy.SetParentPathAsync(childPath);
                await this.StateManager.RemoveStateAsync("Child." + childPath);
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

        public async Task<List<ApplicationDto>> ListApplicationsAsync()
        {
            var applications = new List<ApplicationDto>();
            var names = await this.StateManager.GetStateNamesAsync();
            foreach (var key in names.Where(k => k.StartsWith("Application.")))
            {
                var application = await this.StateManager.GetStateAsync<ApplicationDto>(key);
                applications.Add(application);
            }
            return applications;

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

            // check that all users in all applications have been removed
            var childProxy = ActorProxy.Create<IAuthority>(new ActorId(child.Path));
            var childApplications = await childProxy.ListApplicationsAsync();
            foreach (var application in childApplications)
            {
                var childApplicationAuthority =
                    ActorProxy.Create<IApplicationAuthority>(new ActorId(application.Path + "." + child.Path));
                var users = await childApplicationAuthority.ListUsersAsync();
                if (users.Count > 0)
                    throw new ChildHasUsersException {Child = child, UserCount = users.Count};
            }

            // repoint application inheritance of any grandchildren to me
            var me = await this.StateManager.GetStateAsync<AuthorityDto>("Authority");
            var myApps = await this.ListApplicationsAsync();
            var myApplications = myApps.ToDictionary(app => app.Path);
            foreach (var application in childApplications.Where(childApp => myApplications.ContainsKey(childApp.Path)))
            {
                var grandChildren = await this.ListChildrenAsync();
                foreach (var grandChild in grandChildren)
                {
                    var grandChildProxy = ActorProxy.Create<IAuthority>(new ActorId(grandChild.Path));
                    var grandChildApplicationProxy =
                        ActorProxy.Create<IApplicationAuthority>(new ActorId(application.Path + "." + grandChild.Path));
                    await grandChildApplicationProxy.RepointInheritedRolesAsync(me.Path);
                }
            }

            foreach (var application in childApplications)
            {
                var childAppAuthActor = new ActorId(application.Path + "." + child.Path);
                var childApplicationAuthority =
                    ActorProxy.Create<IApplicationAuthority>(childAppAuthActor);

                // remove role actors
                var roles = await childApplicationAuthority.ListRolesAsync();
                foreach (var role in roles)
                {
                    var actorRole = new ActorId(role.Path);
                    var roleServiceProxy = ActorServiceProxy.Create(_roleServiceUri, actorRole);
                    await roleServiceProxy.DeleteActorAsync(actorRole, new CancellationToken());
                }

                // remove child ApplicationAuthority actor 
                var applicationAuthorityServiceProxy = ActorServiceProxy.Create(_applicationAuthorityServiceUri, childAppAuthActor);
                await applicationAuthorityServiceProxy.DeleteActorAsync(childAppAuthActor, new CancellationToken());
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

        public async Task<List<UserCountDto>> CountUsersAsync()
        {
            var userCounts = new List<UserCountDto>();
            var authority = await this.StateManager.GetStateAsync<AuthorityDto>("Authority");
            var applications = await this.ListApplicationsAsync();
            foreach (var application in applications)
            {
                var applicationAuthorityProxy = ActorProxy.Create<IApplicationAuthority>(new ActorId(application.Path + "." + authority.Path));
                var platformCounts = await applicationAuthorityProxy.CountUsersAsync();
                var userCount = new UserCountDto();
                foreach (var platformCount in platformCounts)
                {
                    userCount.LoggedInUserCount += platformCount.LoggedInUserCount;
                    userCount.MaxLoggedInUserCount += platformCount.MaxLoggedInUserCount;
                    userCount.TotalTime += platformCount.TotalTime;
                }
                userCounts.Add(userCount);
            }
            return userCounts;
        }

        public async Task UpgradeApplicationAsync(ApplicationDto application)
        {
            var exists = await this.StateManager.TryGetStateAsync<ApplicationDto>("Application." + application.Path);
            if (!exists.HasValue)
                return;

            var authority = await this.StateManager.GetStateAsync<AuthorityDto>("Authority");
            var applicationAuthorityProxy =
                ActorProxy.Create<IApplicationAuthority>(new ActorId(application.Path + "." + authority.Path));
            await applicationAuthorityProxy.CreateUpdateRolesAsync(application);
        }
    }
}
