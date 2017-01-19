using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Osmosys.DataContracts;

namespace Osmosys.Abstractions
{
    public interface IUserApplication : IActor
    {
        Task AddRoleAsync(RoleDto role);
        Task AddTaskAsync(UserTaskDto item);
        Task ApproveRoleAsync(RoleDto role);
        Task ApproveRolesAsync();
        Task<List<UserViewModelCount>> CountTasksAsync();
        Task<UserDto> CreateAsync(UserApplicationDto userApplication);
        Task<bool> HasAccessViewModelAsync(ViewModelDto viewModel);
        Task<bool> IsLoggedInAsync(UserDto user);
        Task<List<UserRoleDto>> ListRolesAsync();
        Task<List<PlatformUsageDto>> ListClientUsageAsync();
        Task<List<UserTaskDto>> ListTasksAsync(ViewModelDto viewModel);
        Task<List<ViewModelDto>> ListViewModelsAsync();
        Task LoginUserAsync(PlatformDto platform);
        Task LogoffClientAsync(PlatformDto platform);
        Task LogoffUserAsync();
        Task RemoveTaskAsync(ViewModelDto viewModel, string owningEntityTypeName, string owningEntityId);
        Task RemoveRoleAsync(RoleDto role);
        Task UpdateRoleAsync(RoleDto role);
        Task<UserDto> UpdateUserAsync(UserDto user);
    }
}
