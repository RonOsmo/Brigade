using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Microsoft.ServiceFabric.Actors;
using Osmosys.DataContracts;

namespace Osmosys.Abstractions
{
    public interface IRole : IActor
    {
        Task AddRoleAsync(RoleDto role);
        Task AddUserAsync(UserDto user);
        Task ApproveUserAsync(UserDto user);
        Task ApproveUsersAsync();
        Task CreateAsync(RoleDto role);
        Task<RoleDto> ExistsAsync();
        Task<bool> HasAccessViewModelAsync(ViewModelDto viewModel);
        Task<List<RoleDto>> ListRolesAsync();
        Task<List<UserRoleDto>> ListUsersAsync();
        Task<List<ViewModelDto>> ListViewModelsAsync();
        Task RemoveRoleAsync(RoleDto role);
        Task RemoveUserAsync(UserDto user);
        Task RepointRolesAsync(string fromAuthorityPath, string toAuthorityPath);
        Task<RoleDto> UpdateRoleAsync(RoleDto role);
        Task UpdateUserAsync(UserDto user);
    }

}
