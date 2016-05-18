using System;

namespace Brigade.Models
{
    public class Event : EntityBase<EventType>
    {
        public DateTime EventDate { get; set; }
        public TimeSpan EventLength { get; set; }
        
        public string EventDescription { get; set; }
        public string EventLocation { get; set; }

        public override void SetId()
        {
            base.SetId(EventDate.ToString("yyyyMMddHHMM") + "." + (EventDate + EventLength).ToString("yyyyMMddHHMM"));
        }
    }
}
