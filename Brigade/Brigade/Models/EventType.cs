using System;

namespace Brigade.Models
{
	public class EventType : EntityBase<Brigade>
    {
        public string EventTypeId { get; set; }
        public TimeSpan DefaultReminder { get; set; }

        public override void SetId()
        {
            base.SetId(EventTypeId);
        }
    }
}
