using Brigade.Abstractions;

namespace Brigade.Models
{
	public class Authority : RootEntity
    {
		// uses short Id eg: "cfa.vic.gov.au"
		public string CaptainRole { get; set; }
		public string MemberRole { get; set; }
		public string AssociateRole { get; set; }
		public string PublicRole { get; set; }
		public string SecurityAssignRole { get; set; }
		public string SecurityApproverRole { get; set; }
		public string TrainerRole { get; set; }
	}
}
