using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using ServiceFabricServiceModel;

namespace Osmosys.DataContracts
{
    [DataContract(Name = "AuthorityDto", Namespace = "http://osmo.com.au/brigade")]
    public class AuthorityDto : IExtensibleDataObject
    {
        [IgnoreDataMember]
        private string _id;

        [IgnoreDataMember]
        private string _containerId;

        [DataMember]
        public string Id    // eg: maccy
        {
            get
            {
                return _id ?? string.Empty;
            }
            set
            {
                _id = value;
            }
        } 

        [DataMember]
        public string ContainerId   // eg: cfa.vic.gov.au
        {
            get
            {
                return _containerId ?? string.Empty;
            }
            set
            {
                _containerId = value;
            }
        }

        [DataMember]
        public string Name { get; set; }

        [IgnoreDataMember]
        public string Path => Id + "." + ContainerId;

        [DataMember]
        public virtual ExtensionDataObject ExtensionData { get; set; }
    }

}
