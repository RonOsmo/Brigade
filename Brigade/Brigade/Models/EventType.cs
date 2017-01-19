using System;
using Brigade.Abstractions;

namespace Brigade.Models
{
	public class EventType : EntityBase<Brigade>
    {
        public string Name { get; set; }
		public string Description { get; set; }
        public TimeSpan DefaultReminder { get; set; }
    }
}
