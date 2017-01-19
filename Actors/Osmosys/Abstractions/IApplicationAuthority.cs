using Osmosys.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace Osmosys.Abstractions
{
    public interface IApplicationAuthority : IActor
    {
        Task AddCreateRoleAsync(string roleName);
        Task<UserDto> AddCreateUserAsync(UserDto user);
        Task<ApplicationAuthorityDto> CreateAsync(ApplicationDto application, AuthorityDto authority);
        Task CreateUpdateRolesAsync(ApplicationDto newVersion);
        Task<List<UserPlatformCountDto>> CountUsersAsync();
        Task<ApplicationAuthorityDto> ExistsAsync();
        Task<RoleDto> GetRoleAsync(string roleName);
        Task<RoleDto> GetStandardRoleAsync(RoleType stdRoleType);
        Task<List<RoleDto>> ListRolesAsync();
        Task<List<UserDto>> ListUsersAsync();
        Task LoginUserAsync(PlatformDto platform, UserDto user);
        Task LogoffUserAsync(PlatformDto platform, UserDto user);
        Task RemoveDeleteRoleAsync(RoleDto role);
        Task RemoveDeleteUserAsync(UserDto user);
        Task UpdateUserAsync(UserDto user);
        Task UpdateRoleAsync(RoleDto user);
    }
}
