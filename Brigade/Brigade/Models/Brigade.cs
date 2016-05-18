
namespace Brigade.Models
{
    public class Brigade : EntityBase<Authority>
    {
        public string BrigadeId { get; set; }   // short eg: "maccy"
        public string Name { get; set; }
		public string CaptainRole { get; set; }
		public string MemberRole { get; set; }
		public string AssociateRole { get; set; }
		public string PublicRole { get; set; }
		public string SecurityAssignRole { get; set; }
		public string SecurityApproverRole { get; set; }
		public string FacebookUrl { get; set; }

        public override void SetId()
        {
            base.SetId(BrigadeId);
        }
    }
}
