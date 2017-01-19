using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Osmosys.DataContracts
{
    [DataContract(Name = "ApplicationDto", Namespace = "http://osmo.com.au/brigade")]
    public class ApplicationDto : IExtensibleDataObject
    {
        [IgnoreDataMember] private string _id;
        [IgnoreDataMember] public string VersionPath => ThisVersion.Replace(".", "-") + "." + Id;
        [IgnoreDataMember] public string InheritsPath => InheritsVersion.Replace(".", "-") + "." + Id;
        [IgnoreDataMember] public string Path => Id;

        [DataMember]
        public string Id // friendly name like "Brigade"
        {
            get
            {
                _id = _id ?? string.Empty;
                return _id;
            }
            set { _id = value; }
        }

        [DataMember] public string Name { get; set; }
        [DataMember] public string ThisVersion { get; set; }
        [DataMember] public string CurrentVersion { get; set; }
        [DataMember] public string UpgradeToVersion { get; set; }
        [DataMember] public string InheritsVersion { get; set; } // optional backward links

        [DataMember] public virtual ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Name = "ApplicationAuthorityDto", Namespace = "http://osmo.com.au/brigade")]
    public class ApplicationAuthorityDto : IExtensibleDataObject
    {
        [DataMember] public string ApplicationPath { get; set; }
        [DataMember] public string AuthorityPath { get; set; }
        [IgnoreDataMember] public string Path => ApplicationPath + "." + AuthorityPath;
        [DataMember] public virtual ExtensionDataObject ExtensionData { get; set; }
    }
}
