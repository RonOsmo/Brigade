using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Osmosys.Abstractions;

namespace Osmosys.DataContracts
{
    [DataContract(Name = "UserTaskDto", Namespace = "http://osmo.com.au/brigade")]
	public class UserTaskDto : IExtensibleDataObject
	{
		[DataMember] public string OwningEntityId { get; set; }
		[DataMember] public string OwningEntityTypeName { get; set; }
		[DataMember] public ViewModelDto ViewModel { get; set; }
		[DataMember] public string TaskTypeName { get; set; }
        [DataMember] public Guid Id { get; set; } // identifier is only used to group tasks together
	    [IgnoreDataMember] public string Path => OwningEntityId + "." + OwningEntityTypeName;
        [DataMember] public ExtensionDataObject ExtensionData { get; set; }
	}

	[DataContract(Name = "TaskTypeDto", Namespace = "http://osmo.com.au/brigade")]
	public class TaskTypeDto
	{
		[DataMember] public string Name { get; set; }
		[DataMember] public string ContainerId { get; set; } // could be overridden by the Brigade's specialized version
        [DataMember] public string ActorType { get; set; }
        [DataMember] public string ActorId { get; set; }
        [DataMember] public string ActorUrl { get; set; }
		[DataMember] public List<TaskMethodDto> Methods { get; set; }
	}

	[DataContract(Name = "TaskMethodDto", Namespace = "http://osmo.com.au/brigade")]
	public class TaskMethodDto
	{
		[DataMember] public int Sequence { get; set; }
		[DataMember] public string Method { get; set; }
		[DataMember] public string ReturnBindingPath { get; set; }
		[DataMember] public List<TaskMethodArgDto> Args { get; set; }
		[DataMember] public List<TaskMethodValidationDto> Validations { get; set; }
        [DataMember] public List<RoleDto> Roles { get; set; }
	}

	[DataContract(Name = "TaskMethodArgDto", Namespace = "http://osmo.com.au/brigade")]
	public class TaskMethodArgDto
	{
		[DataMember] public int ArgSequence { get; set; }
		[DataMember] public string BindingPath { get; set; } // from ViewModel
		[DataMember] public bool IsCollection { get; set; }
		[DataMember] public string ItemTypeName { get; set; }
	}

	[DataContract(Name = "TaskMethodValidationDto", Namespace = "http://osmo.com.au/brigade")]
	public class TaskMethodValidationDto
	{
		[DataMember] public int Sequence { get; set; }
		[DataMember] public string BindingPath { get; set; } // from ViewModel
		[DataMember] public string TypeName { get; set; }
		[DataMember] public bool ValidateNonNull { get; set; }
		[DataMember] public string Condition { get; set; }
		[DataMember] public string ComparisonValue { get; set; }
	}
}
