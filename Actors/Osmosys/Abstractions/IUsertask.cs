using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Osmosys.DataContracts;


namespace Osmosys.Abstractions
{

    public interface IUsertask : IActor
	{
        Task AddTaskAsync(UserTaskDto item);
        Task<int> RemoveTaskAsync(string owningEntityTypeName, string owningEntityId);
        Task<List<UserTaskDto>> ListTasksAsync();
	}

    public interface IGrouptask : IActor
    {
        Task AddTaskAsync(UserTaskDto item, List<UserDto> users);
        Task RemoveTasksAsync();
    }

    //[DataContract(Name = "UserViewModel", Namespace = "http://osmo.com.au/brigade")]
    //public class UserViewModel : IExtensibleDataObject
    //{ 
    //    [DataMember]
    //    public Dictionary<string, UserViewModelItem> Items { get; set; }

    //    // support forward compatible contracts
    //    [DataMember]
    //    public virtual ExtensionDataObject ExtensionData { get; set; }
    //}


    //[DataContract(Name = "UserViewModelItem", Namespace = "http://osmo.com.au/brigade")]
    //public class UserViewModelItem : IExtensibleDataObject
    //{
    //    [DataMember] public string OwningEntityTypeName { get; set; }
    //    [DataMember] public string OwningEntityId { get; set; }
    //    [DataMember] public UserTaskDto UserTaskType { get; set; }

    //    // support forward compatible contracts
    //    [DataMember] public virtual ExtensionDataObject ExtensionData { get; set; }
    //}

}
