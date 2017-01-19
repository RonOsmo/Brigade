using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Data;
using Osmosys.DataContracts;

namespace Osmosys.Abstractions
{
    public interface IAuthority : IActor
    {
        Task<AuthorityDto> AddCreateChildAsync(AuthorityDto authority);
        Task<AuthorityDto> CreateAsync(AuthorityDto authority);
        Task<UserCountDto> CountUsersAsync();
        Task<AuthorityDto> ExistsAsync();
        Task<AuthorityDto> GetParentAsync();
        Task<List<AuthorityDto>> ListChildrenAsync();
        Task<List<RoleDto>> ListRolesAsync();
        Task<List<UserDto>> ListUsersAsync();
        Task LoginUserAsync(PlatformDto platform, UserApplicationDto userApplication);
        Task LogoffUserAsync(PlatformDto platform, UserApplicationDto userApplication);
        Task RemoveDeleteChildAsync(AuthorityDto authority);
        Task SetParentPathAsync(string parentPath);
        Task UpdateUserAsync(UserDto user);
    }
}
