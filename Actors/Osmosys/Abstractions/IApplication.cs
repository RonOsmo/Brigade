using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Osmosys.DataContracts;

namespace Osmosys.Abstractions
{
    public interface IApplication : IActor
    {
        Task AddApplicationRoleToUserAsync(RoleDto role, UserDto user);
        Task<ApplicationDto> AddCreateVersionAsync(string currentVersion, string inheritsVersion, string[] upgradeVersions);
        Task<RoleDto> AddStandardRoleAsync(RoleType roleType, string name);
        Task AddRoleViewModelAsync(RoleDto role, ViewModelDto viewModel);
        Task<UserCountDto> CountUsersAsync();
        Task<ApplicationDto> CreateAsync(string identifier, string name, string currentVersion);
        Task<ApplicationDto> ExistsAsync();
        Task<RoleDto> GetStandardRoleTypeAsync(RoleType stdRoleType);
        Task<List<ApplicationDto>> ListApplicationVersionsAsync();
        Task<List<RoleDto>> ListRolesAsync();
        Task<List<ViewModelDto>> ListViewModelsAsync(RoleDto role);
        Task LoginUserAsync(PlatformDto platform, UserDto user);
        Task LogoffUserAsync(PlatformDto platform, UserDto user);
        Task RemoveRoleAsync(RoleDto role);
        Task RemoveRoleViewModelAsync(RoleDto role, ViewModelDto viewModel);
        Task<ApplicationDto> UpdateApplication(ApplicationDto application);
    }
}