using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Osmosys.DataContracts;

namespace Osmosys.Abstractions
{
    /// <summary>
    /// This interface defines the methods exposed by an actor.
    /// Clients use this interface to interact with the actor that implements it.
    /// </summary>
    public interface IUser : IActor
    {
        Task AddRoleAsync(RoleDto role);
        Task AddTaskAsync(UserTaskDto item);
        Task ApproveRoleAsync(RoleDto role);
        Task ApproveRolesAsync();
        Task<List<UserViewModelCount>> CountTasksAsync();
        Task<UserDto> CreateAsync(UserDto user);
        Task<bool> HasAccessViewModelAsync(ViewModelDto viewModel);
        Task<bool> IsLoggedInAsync(UserDto user);
        Task<List<UserRoleDto>> ListRolesAsync();
        Task<List<ClientUsageDto>> ListClientUsageAsync();
        Task<List<UserTaskDto>> ListTasksAsync(ViewModelDto viewModel);
        Task<List<ViewModelDto>> ListViewModelsAsync();
        Task LoginClientAsync(PlatformDto platform, ApplicationDto application);
        Task LogoffClientAsync(PlatformDto platform, ApplicationDto application);
        Task LogoffUserAsync();
        Task RemoveTaskAsync(ViewModelDto viewModel, string owningEntityTypeName, string owningEntityId);
        Task RemoveRoleAsync(RoleDto role);
        Task UpdateRoleAsync(RoleDto role);
        Task<UserDto> UpdateUserAsync(UserDto user);
    }

    [DataContract(Name = "UserViewModelCount", Namespace = "http://osmo.com.au/brigade")]
    public class UserViewModelCount : IExtensibleDataObject
    {
        [DataMember] public ViewModelDto ViewModel { get; set; }
        [DataMember] public int Count { get; set; }

        // support forward compatible contracts
        [DataMember] public virtual ExtensionDataObject ExtensionData { get; set; }
    }


}
