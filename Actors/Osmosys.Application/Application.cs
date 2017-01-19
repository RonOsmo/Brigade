using System;
using System.CodeDom;
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

namespace Osmosys.Application
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
    internal class Application : Actor, IApplication
    {
        /// <summary>
        /// Initializes a new instance of Application
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public Application(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        public Task AddApplicationRoleToUserAsync(RoleDto role, UserDto user)
        {
            throw new NotImplementedException();
        }

        public async Task<ApplicationDto> AddCreateVersionAsync(string currentVersion, string inheritsVersion, params string[] upgradeVersions)
        {
            if (string.IsNullOrWhiteSpace(currentVersion))
                throw new ArgumentNullException(nameof(currentVersion));

            var application = await this.StateManager.GetStateAsync<ApplicationDto>("Application");

            var upgrades = upgradeVersions.ToDictionary(s => s);

            var inheritsVersionFound = false;
            var versions = await this.ListApplicationVersionsAsync();
            foreach (var version in versions)
            {
                if (version.ThisVersion == inheritsVersion)
                    inheritsVersionFound = true;
                if (version.ThisVersion == currentVersion)
                    throw new ApplicationCurrentVersionException {Application = version};

                if (upgrades.ContainsKey(version.ThisVersion))
                {
                    version.UpgradeToVersion = currentVersion;
                    await this.StateManager.SetStateAsync("Version." + currentVersion, version);

                    var versionProxy = ActorProxy.Create<IApplication>(new ActorId(version.VersionPath));
                    await versionProxy.UpdateApplication(version);
                }
            }

            if (!string.IsNullOrWhiteSpace(inheritsVersion) && !inheritsVersionFound)
                throw new ApplicationMissingInheritVersionException
                {
                    Application = application,
                    CurrentVersion = currentVersion,
                    InheritsVersion = inheritsVersion
                };

            var childVersion = new ApplicationDto
            {
                Id = application.Id,
                Name = application.Name,
                CurrentVersion = currentVersion,
                InheritsVersion = inheritsVersion,
            };
            await this.StateManager.SetStateAsync("Version." + currentVersion, childVersion);

            var childVersionProxy = ActorProxy.Create<IApplication>(new ActorId(childVersion.VersionPath));
            childVersion = await childVersionProxy.UpdateApplication(childVersion);

            return childVersion;
        }

        public async Task AddRoleViewModelAsync(RoleDto role, ViewModelDto viewModel)
        {
            var application = await this.StateManager.GetStateAsync<ApplicationDto>("Application");
            viewModel.ApplicationPath = application.VersionPath;
            role.AuthorityPath = application.VersionPath;
            var roleViewModel = new RoleViewModelDto {Role = role, ViewModel = viewModel};
            await this.StateManager.SetStateAsync("RoleViewModel." + roleViewModel.Path, roleViewModel);
        }

        public async Task<RoleDto> AddStandardRoleAsync(RoleType roleType, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            var application = await this.StateManager.GetStateAsync<ApplicationDto>("Application");
            var role = new RoleDto
            {
                ApplicationPath = application.VersionPath,
                Inherits = (roleType != RoleType.Custom),
                IsApplicationRole = true,
                Name = name,
                RoleType = roleType,
                Version = 1,
            };

            await this.StateManager.SetStateAsync("Role." + role.Id, role);
            return role;
        }

        public Task<UserCountDto> CountUsersAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<ApplicationDto> CreateAsync(string identifier, string name, string currentVersion)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentNullException(nameof(identifier));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(currentVersion))
                throw new ArgumentNullException(nameof(currentVersion));

            var application = new ApplicationDto
            {
                Id = identifier,
                Name = name,
                ThisVersion = currentVersion,
                CurrentVersion = currentVersion,
            };

            var exists = await this.StateManager.TryGetStateAsync<ApplicationDto>("Application");
            if (exists.HasValue)
            {
                if (exists.Value.Id != identifier || exists.Value.Name != name ||
                    exists.Value.CurrentVersion != currentVersion ||
                    !string.IsNullOrWhiteSpace(exists.Value.UpgradeToVersion))
                    throw new ApplicationBadCreateException
                    {
                        Identifier = identifier,
                        Name = name,
                        CurrentVersion = currentVersion
                    };

                return exists.Value;
            }

            await this.StateManager.SetStateAsync("Application", application);
            await this.StateManager.SetStateAsync("Version." + currentVersion, application);

            return application;
        }

        public async Task<ApplicationDto> ExistsAsync()
        {
            var exists = await this.StateManager.TryGetStateAsync<ApplicationDto>("Application");
            return exists.HasValue ? exists.Value : null;
        }

        public async Task<RoleDto> GetStandardRoleTypeAsync(RoleType stdRoleType)
        {
            var roles = await this.ListRolesAsync();
            foreach (var role in roles.Where(r => r.RoleType == stdRoleType))
            {
                return role;
            }
            return null;
        }

        public async Task<List<ApplicationDto>> ListApplicationVersionsAsync()
        {
            var versions = new List<ApplicationDto>();
            var keys = await this.StateManager.GetStateNamesAsync();
            foreach (var key in keys.Where(k => k.StartsWith("Version.")))
            {
                var version = await this.StateManager.GetStateAsync<ApplicationDto>(key);
                versions.Add(version);
            }
            return versions;
        }

        public async Task<List<RoleDto>> ListRolesAsync()
        {
            var roles = new Dictionary<string, RoleDto>();

            var application = await this.ExistsAsync();
            if (!string.IsNullOrWhiteSpace(application?.InheritsVersion))
            {
                var parentVersion = ActorProxy.Create<IApplication>(new ActorId(application.InheritsPath));
                var inheritedRoles = await parentVersion.ListRolesAsync();
                foreach (var role in inheritedRoles)
                {
                    roles.Add(role.Name, role);
                }
            }

            var keys = await this.StateManager.GetStateNamesAsync();
            foreach (var key in keys.Where(k => k.StartsWith("Role.")))
            {
                var role = await this.StateManager.GetStateAsync<RoleDto>(key);
                roles[role.Name] = role;
            }

            return roles.Values.ToList();
        }

        public async Task<List<ViewModelDto>> ListViewModelsAsync(RoleDto role)
        {
            var viewModels = new Dictionary<string, ViewModelDto>();

            var application = await this.ExistsAsync();
            if (!string.IsNullOrWhiteSpace(application?.InheritsVersion))
            {
                var parentVersion = ActorProxy.Create<IApplication>(new ActorId(application.InheritsPath));
                var inheritedViewModels = await parentVersion.ListViewModelsAsync(role);
                foreach (var viewModel in inheritedViewModels)
                {
                    viewModels.Add(viewModel.Name, viewModel);
                }
            }

            var keys = await this.StateManager.GetStateNamesAsync();
            foreach (var key in keys.Where(k => k.StartsWith("RoleViewModel." + role.Id)))
            {
                var roleViewModel = await this.StateManager.GetStateAsync<RoleViewModelDto>(key);
                viewModels[roleViewModel.ViewModel.Name] = roleViewModel.ViewModel;
            }

            return viewModels.Values.ToList();
        }

        public async Task LoginUserAsync(PlatformDto platform, UserDto user)
        {
            if (string.IsNullOrWhiteSpace(platform?.PlatformName) || string.IsNullOrWhiteSpace(platform.PlatformVersion))
                throw new ArgumentNullException(nameof(platform));

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

            var userCount = new UserCountDto
            {
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

            var application = await this.StateManager.GetStateAsync<ApplicationDto>("Application");
            if (string.IsNullOrWhiteSpace(application.ThisVersion))
                return;
            var applicationProxy = ActorProxy.Create<IApplication>(new ActorId(application.Path));
            await applicationProxy.LoginUserAsync(platform, user);
        }

        public async Task LogoffUserAsync(PlatformDto platform, UserDto user)
        {
            if (string.IsNullOrWhiteSpace(platform?.PlatformName) || string.IsNullOrWhiteSpace(platform.PlatformVersion))
                throw new ArgumentNullException(nameof(platform));

            var platformUsage = new PlatformUsageDto
            {
                Platform = platform,
                LastLogoffDateTime = DateTime.Now,
                LoginCount = 0,
            };
            await this.StateManager.AddOrUpdateStateAsync("UserLogin." + platform.Path + "." + user.Id, platformUsage,
                (k, v) =>
                {
                    if (--v.LoginCount == 0)
                        v.LastLogoffDateTime = DateTime.Now;
                    platformUsage = v;
                    return v;
                });

            if (platformUsage.LoginCount == 0 && platformUsage.LastLoginDateTime.HasValue && platformUsage.LastLogoffDateTime.HasValue)
            {
                var userCount = new UserCountDto
                {
                    LoggedInUserCount = 0,
                    MaxLoggedInUserCount = 1,
                    TotalTime = platformUsage.LastLogoffDateTime.Value - platformUsage.LastLoginDateTime.Value,
                };
                await this.StateManager.AddOrUpdateStateAsync("UserCount", userCount, (k, v) =>
                {
                    --v.LoggedInUserCount;
                    v.TotalTime += userCount.TotalTime;
                    return v;
                });
            }

            var application = await this.StateManager.GetStateAsync<ApplicationDto>("Application");
            if (string.IsNullOrWhiteSpace(application.ThisVersion))
                return;
            var applicationProxy = ActorProxy.Create<IApplication>(new ActorId(application.Path));
            await applicationProxy.LoginUserAsync(platform, user);
        }

        public async Task RemoveRoleAsync(RoleDto role)
        {
            await this.StateManager.RemoveStateAsync("Role." + role.Id);
            var viewModels = await this.ListViewModelsAsync(role);
            foreach (var viewModel in viewModels)
            {
                var roleViewModel = new RoleViewModelDto {Role = role, ViewModel = viewModel};
                await this.StateManager.RemoveStateAsync("RoleViewModel." + roleViewModel.Path);
            }

            var application = await this.ExistsAsync();
            if (!string.IsNullOrWhiteSpace(application?.InheritsVersion))
            {
                var parentVersion = ActorProxy.Create<IApplication>(new ActorId(application.InheritsPath));
                await parentVersion.RemoveRoleAsync(role);
            }
        }

        public async Task RemoveRoleViewModelAsync(RoleDto role, ViewModelDto viewModel)
        {
            var roleViewModel = new RoleViewModelDto {Role = role, ViewModel = viewModel};
            await this.StateManager.RemoveStateAsync("RoleViewModel." + roleViewModel.Path);

            var application = await this.ExistsAsync();
            if (!string.IsNullOrWhiteSpace(application?.InheritsVersion))
            {
                var parentVersion = ActorProxy.Create<IApplication>(new ActorId(application.InheritsPath));
                await parentVersion.RemoveRoleViewModelAsync(role, viewModel);
            }
        }

        public async Task<ApplicationDto> UpdateApplication(ApplicationDto application)
        {
            var exists = await this.StateManager.TryGetStateAsync<ApplicationDto>("Application");
            if (!exists.HasValue)
                throw new Exception();

            if (exists.Value.Id != application.Id)
                throw new Exception();

            await this.StateManager.AddOrUpdateStateAsync("Application", application, (k, v) =>
            {
                v.Name = application.Name;
                v.CurrentVersion = application.CurrentVersion;
                v.InheritsVersion = application.InheritsVersion;
                v.UpgradeToVersion = application.UpgradeToVersion;
                return v;
            });

            return application;
        }
    }
}
