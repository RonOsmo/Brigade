using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Osmosys.DataContracts
{
    [DataContract(Name = "PlatformDto", Namespace = "http://osmo.com.au/brigade")]
    public class PlatformDto : IExtensibleDataObject
    {
        [DataMember] public string PlatformName { get; set; }
        [DataMember] public string PlatformVersion { get; set; }
        [IgnoreDataMember] public string Path => PlatformName + "." + PlatformVersion.Replace('.', '-') + ".";

        [DataMember] public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Name = "UserCountDto", Namespace = "http://osmo.com.au/brigade")]
    public class UserCountDto : IExtensibleDataObject
    {
        [DataMember] public int LoggedInUserCount { get; set; }
        [DataMember] public int MaxLoggedInUserCount { get; set; }
        [DataMember] public TimeSpan TotalTime { get; set; }
        [DataMember] public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Name = "UserPlatformCountDto", Namespace = "http://osmo.com.au/brigade")]
    public class UserPlatformCountDto : IExtensibleDataObject
    {
        [DataMember]
        public string PlatformPath { get; set; }
        [DataMember]
        public int LoggedInUserCount { get; set; }
        [DataMember]
        public int MaxLoggedInUserCount { get; set; }
        [DataMember]
        public TimeSpan TotalTime { get; set; }
        [DataMember]
        public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Name = "PlatformUsageDto", Namespace = "http://osmo.com.au/brigade")]
    public class PlatformUsageDto : IExtensibleDataObject
    {
        [DataMember] public PlatformDto Platform { get; set; }
        [DataMember] public TimeSpan? LoggedInTimeSpan { get; set; }
        [DataMember] public int LoginCount { get; set; }
        [DataMember] public DateTime? LastLoginDateTime { get; set; }
        [DataMember] public DateTime? LastLogoffDateTime { get; set; }
        [DataMember] public ExtensionDataObject ExtensionData { get; set; }
    }

    [DataContract(Name = "ClientUsageDto", Namespace = "http://osmo.com.au/brigade")]
    public class ClientUsageDto : IExtensibleDataObject
    {
        [DataMember]
        public PlatformDto Platform { get; set; }
        [DataMember]
        public ApplicationDto Application { get; set; }
        [DataMember]
        public TimeSpan? LoggedInTimeSpan { get; set; }
        [DataMember]
        public int LoginCount { get; set; }
        [DataMember]
        public DateTime? LastLoginDateTime { get; set; }
        [DataMember]
        public DateTime? LastLogoffDateTime { get; set; }

        [IgnoreDataMember]
        public string Path => Platform.Path + "." + Application.VersionPath;

        [DataMember]
        public ExtensionDataObject ExtensionData { get; set; }
    }
}
