using System;

namespace Brigade.Models
{
    public class Availability : EntityBase<EventType>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public User User { get; set; }
        public bool Automatic { get; set; }

        //public override void SetId(string authority, string brigade)
        //{
        //    ContainerId = EventType;
        //    base.SetId(authority, brigade);
        //    RowKey = (FromDate.HasValue ? FromDate.Value.ToString("yyyyMMdd") : "00000000") +
        //        BrigadeBaseEntity.SEPARATOR + (ToDate.HasValue ? ToDate.Value.ToString("yyyyMMdd") : "99999999") +
        //        BrigadeBaseEntity.SEPARATOR + UserName;
        //}
    }
}
