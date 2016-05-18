namespace Brigade.Models
{
	public class Authority : RootEntity
    {
        public string AuthorityId { get; set; }		// short eg: "cfa"
        public string Name { get; set; }
		public string CaptainRole { get; set; }
		public string MemberRole { get; set; }
		public string AssociateRole { get; set; }
		public string PublicRole { get; set; }
		public string SecurityAssignRole { get; set; }
		public string SecurityApproverRole { get; set; }

		public override void SetId()
		{
			base.SetId(AuthorityId);
		}
	}
}
