using System;
using Microsoft.WindowsAzure.Mobile.Service;
using System.ComponentModel.DataAnnotations;

namespace BrigadeMobileService.DataObjects
{
    public class Asset : StorageData
    {
        public const string SEPARATOR = ".";

        [Required]
        [StringLength(maximumLength: 20, MinimumLength = 1)]
        public string AssetCode { get; set; }
        public Asset Container { get; set; }
        [MaxLength(length: 500)]
        public string Description { get; set; }
        [MaxLength(length: 128)]
        public string LockedByUserId { get; set; }
        [MaxLength(length: 128)]
        public string LockedOnDevice { get; set; }
        [Required]
        public bool Sighted { get; set; }
        public DateTime? MissingDate { get; set; }

        public void SetId()
        {
            if (Container != null)
            {
                Id = Container.Id + Asset.SEPARATOR + AssetCode;
            }
            else
            {
                Id = AssetCode;
            }

        }
    }
}