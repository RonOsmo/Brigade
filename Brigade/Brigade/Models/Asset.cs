using System;

namespace Brigade.Models
{
	public class Asset : AnyContainer
    {
		
		public string AssetId { get; set; }
        public string Description { get; set; }
        public User LockedByUser { get; set; }
        public Device LockedOnDevice { get; set; }
        public bool Sighted { get; set; }
        public DateTime? MissingDate { get; set; }
		public bool GotChildren { get; set; }

		public override void SetId()
		{
			base.SetId(AssetId);
		}
	}
}
