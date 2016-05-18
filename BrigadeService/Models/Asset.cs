using System.ComponentModel.DataAnnotations;
using System;

namespace DataTableStorage.Models
{
    [System.ComponentModel.DataAnnotations.Schema.Table("Asset")]
    public class Asset : BrigadeBaseEntity
    {

        [Required]
        [StringLength(maximumLength: 20, MinimumLength = 1)]
        public string AssetCode { get; set; }

        [MaxLength(length: 500)]
        public string Description { get; set; }

        [MaxLength(length: 128)]
        public string LockedByUserId { get; set; }

        [MaxLength(length: 128)]
        public string LockedOnDevice { get; set; }

        [Required]
        public bool Sighted { get; set; }

        public DateTime? MissingDate { get; set; }

        public override void SetId(string authority, string brigade)
        {
            base.SetId(authority, brigade);

            RowKey = AssetCode;
        }
    }
}
