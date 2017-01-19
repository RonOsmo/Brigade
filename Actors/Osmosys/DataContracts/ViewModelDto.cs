using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Osmosys.DataContracts
{
    [DataContract(Name = "ViewModel", Namespace = "http://osmo.com.au/brigade")]
    public class ViewModelDto : IExtensibleDataObject
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string ApplicationPath { get; set; }

        [DataMember]
        public bool Inherits { get; set; }

        [IgnoreDataMember]
        public string Path => Name + "." + ApplicationPath;

        // support forward compatible contracts
        [DataMember]
        public virtual ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Name = "RoleViewModel", Namespace = "http://osmo.com.au/brigade")]
    public class RoleViewModelDto : IExtensibleDataObject
    {
        [DataMember] public RoleDto Role { get; set; }
        [DataMember] public ViewModelDto ViewModel { get; set; }
        [IgnoreDataMember] public string Path => Role.Id + "." + ViewModel.Name;

        // support forward compatible contracts
        [DataMember]
        public virtual ExtensionDataObject ExtensionData { get; set; }
    }
}
