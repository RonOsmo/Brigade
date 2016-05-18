namespace Brigade.Models
{
	public class EventAttendee : EntityBase<Event>
    {
        public EventTypeRole AttendeeRole { get; set; }
        public User Attendee { get; set; }

        public override void SetId()
        {
            base.SetId(AttendeeRole.EventRoleId + "." + Attendee.UserId);
        }
    }
}
