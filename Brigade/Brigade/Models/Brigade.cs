using Brigade.Abstractions;

namespace Brigade.Models
{
    public class Brigade : EntityBase<Authority>
    {
		public override bool UseContainerFullPath
		{
			get
			{
				return true;	// make partition key containers like maccy + . + authority.Id
			}
		}

		// uses short Id eg: "maccy"
		public string CaptainRole { get; set; }
		public string MemberRole { get; set; }
		public string AssociateRole { get; set; }
		public string PublicRole { get; set; }
		public string SecurityAssignRole { get; set; }
		public string SecurityApproverRole { get; set; }
		public string TrainerRole { get; set; }
		public string FacebookUrl { get; set; }
    }
}
