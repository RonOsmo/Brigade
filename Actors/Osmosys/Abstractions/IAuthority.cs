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
        Task AddApplicationAsync(ApplicationDto application);
        Task<AuthorityDto> CreateAsync(AuthorityDto authority);
        Task<List<UserCountDto>> CountUsersAsync();
        Task<AuthorityDto> ExistsAsync();
        Task<AuthorityDto> GetParentAsync();
        Task<List<ApplicationDto>> ListApplicationsAsync();
        Task<List<AuthorityDto>> ListChildrenAsync();
        Task RemoveDeleteChildAsync(AuthorityDto authority);
        Task SetParentPathAsync(string parentPath);
        Task UpgradeApplicationAsync(ApplicationDto application);

    }
}
