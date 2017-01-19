using System;
using System.Runtime.Serialization;

namespace Osmosys.DataContracts
{

    [DataContract(Name = "RoleDto", Namespace = "http://osmo.com.au/brigade")]
    public class RoleDto : IExtensibleDataObject
    {
        [IgnoreDataMember] private Guid _id;
        [IgnoreDataMember] private string _authorityPath;
        [IgnoreDataMember] private string _applicationPath;
        [IgnoreDataMember] public string Path => Id + "." + ApplicationPath + "." + AuthorityPath;

        [DataMember]
        public Guid Id
        {
            get
            {
                if (_id == Guid.Empty)
                    _id = new Guid();
                return _id;
            }
            set { _id = value; }
        }

        [DataMember]
        public string AuthorityPath
        {
            get
            {
                _authorityPath = _authorityPath ?? string.Empty;
                return _authorityPath;
            }
            set { _authorityPath = value; }
        }

        [DataMember]
        public string ApplicationPath
        {
            get
            {
                _applicationPath = _applicationPath ?? string.Empty;
                return _applicationPath;
            }
            set { _applicationPath = value; }
        }

        [DataMember] public string Name { get; set; }
        [DataMember] public bool Inherits { get; set; }
        [DataMember] public bool IsApplicationRole { get; set; }
        [DataMember] public RoleType RoleType { get; set; }
        [DataMember] public int Version { get; set; }
        [DataMember] public virtual ExtensionDataObject ExtensionData { get; set; }
    }

    public enum RoleType
    {
        Custom = 0,
        Admin = 1,
        AdminApprover = 2,
        Spare1,
        Spare2,
        Spare3,
        Spare4,
        Spare5,
        Spare6,
        Spare7,
        Spare8,
        Spare9,
        Spare10,
        Spare11,
        Spare12,
        Spare13,
        Spare14,
        Spare15,
        Spare16,
        Spare17,
        Spare18,
        Spare19,
    }

    [DataContract(Name = "UserRoleDto", Namespace = "http://osmo.com.au/brigade")]
    public class UserRoleDto : IExtensibleDataObject
    {
        [DataMember] public UserDto User { get; set; }
        [DataMember] public RoleDto Role { get; set; }
        [DataMember] public bool Approved { get; set; }

        public virtual ExtensionDataObject ExtensionData { get; set; }
    }
}
