using System.ComponentModel.DataAnnotations;
using System;

namespace DataTableStorage.Models
{
    [System.ComponentModel.DataAnnotations.Schema.Table("Event")]
    public class Event : BrigadeBaseEntity
    {

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public TimeSpan Length { get; set; }

        [Required]
        public string EventType { get; set; }

        public string EventDescription { get; set; }

        public string EventLocation { get; set; }

        public override void SetId(string authority, string brigade)
        {
            base.SetId(authority, brigade);
            RowKey = FromDate.ToString("yyyyMMddHHMM") +
                BrigadeBaseEntity.SEPARATOR + (FromDate + Length).ToString("yyyyMMddHHMM") +
                BrigadeBaseEntity.SEPARATOR + EventType;
        }
    }

    [System.ComponentModel.DataAnnotations.Schema.Table("Event")]
    public class EventRole : BrigadeBaseEntity
    {

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public TimeSpan Length { get; set; }

        [Required]
        public string EventType { get; set; }

        [Required]
        public string RoleNeeded { get; set; }

        public string User { get; set; }

        public override void SetId(string authority, string brigade)
        {
            ContainerId = FromDate.ToString("yyyyMMddHHMM") +
                BrigadeBaseEntity.SEPARATOR + (FromDate + Length).ToString("yyyyMMddHHMM");

            base.SetId(authority, brigade);
            RowKey = BrigadeBaseEntity.SEPARATOR + RoleNeeded +
                BrigadeBaseEntity.SEPARATOR + User;
        }

        public override string TypeName
        {
            get
            {
                return EventType;
            }
        }
    }

    [System.ComponentModel.DataAnnotations.Schema.Table("Event")]
    public class Availability : BrigadeBaseEntity
    {

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        [Required]
        public string EventType { get; set; }

        [Required]
        [StringLength(maximumLength: 20, MinimumLength = 1)]
        public string UserName { get; set; }

        public bool Automatic { get; set; }

        public override void SetId(string authority, string brigade)
        {
            ContainerId = EventType;
            base.SetId(authority, brigade);
            RowKey = (FromDate.HasValue ? FromDate.Value.ToString("yyyyMMdd") : "00000000") +
                BrigadeBaseEntity.SEPARATOR + (ToDate.HasValue ? ToDate.Value.ToString("yyyyMMdd") : "99999999") +
                BrigadeBaseEntity.SEPARATOR + UserName;
        }
    }
}
