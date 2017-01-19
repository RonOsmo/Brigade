using System;
using System.Runtime.Serialization;

namespace Osmosys.DataContracts
{

    [DataContract(Name = "UserDto", Namespace = "http://osmo.com.au/brigade")]
	public class UserDto : IExtensibleDataObject
	{
        [IgnoreDataMember] private Guid _id;
        [IgnoreDataMember] private string _authorityPath;
        [IgnoreDataMember] public string Path => Id + "." + AuthorityPath;

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

        [DataMember] public string Name { get; set; }
		[DataMember] public string UserName { get; set; }
		[DataMember] public string Email { get; set; }
		[DataMember] public string MobileNumber { get; set; }
		[DataMember] public string LocaleId { get; set; }
		[DataMember] public UserStatus Status { get; set; }
        [DataMember] public int Version { get; set; }
        [DataMember] public virtual ExtensionDataObject ExtensionData { get; set; }
	}

	[DataContract(Name = "UserStatus", Namespace = "http://osmo.com.au/brigade")]
	public enum UserStatus 
	{
		NotSpecified = 0,
		New,
		Verifying,
		Ok,
		VerificationUnsuccessful,
	}

    [DataContract(Name = "UserApplicationDto", Namespace = "http://osmo.com.au/brigade")]
    public class UserApplicationDto : IExtensibleDataObject
    {
        [DataMember] public UserDto User { get; set; }
        [DataMember] public ApplicationDto Application { get; set; }
        [IgnoreDataMember] public string Path => User.Id + "." + Application.VersionPath + "." + User.AuthorityPath;
        [DataMember] public virtual ExtensionDataObject ExtensionData { get; set; }
    }
}
