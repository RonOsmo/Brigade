namespace Brigade.Models
{
	public class EventTypeRole : EntityBase<EventType>
    {
        public string EventRoleId { get; set; }
		public Role Role { get; set; }

        public override void SetId()
        {
            base.SetId(EventRoleId);
        }
    }
}
