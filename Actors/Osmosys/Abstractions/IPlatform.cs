using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Osmosys.DataContracts;

namespace Osmosys.Abstractions
{
    public interface IPlatform : IActor
    {
        Task<UserCountDto> CountUsersAsync();
        Task<List<UserDto>> ListUsersAsync();
        Task LoginUserAsync(PlatformDto platform, UserApplicationDto userApplication);
        Task LogoffUserAsync(PlatformDto platform, UserApplicationDto userApplication);
    }
}
