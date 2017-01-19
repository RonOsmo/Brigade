using System;
using Brigade.Abstractions;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices.Sync;

namespace Brigade.Models
{
    public class Event : EntityBase<EventType>
    {
		[JsonIgnore]
		public string Name
		{
			get { return Container.Name; }
		}
		public DateTime EventDate { get; set; }
        public TimeSpan EventLength { get; set; }
        
        public string EventDescription { get; set; }
        public string EventLocation { get; set; }
    }
}
