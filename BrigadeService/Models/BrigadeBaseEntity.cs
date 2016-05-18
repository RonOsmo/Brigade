using System.ComponentModel.DataAnnotations;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace DataTableStorage.Models
{

    public class BrigadeBaseEntity : TableEntity
    {
        public const string SEPARATOR = ".";
        public string ContainerId { get; set; }

        public virtual string TypeName
        {
            get
            {
                return this.GetType().Name;
            }
        }

        public virtual void SetId(string authority, string brigade)
        {
            if (string.IsNullOrWhiteSpace(authority))
            {
                throw new ArgumentNullException(nameof(authority));
            }
            if (string.IsNullOrWhiteSpace(brigade))
            {
                throw new ArgumentNullException(nameof(brigade));
            }

            if (!string.IsNullOrWhiteSpace(ContainerId))
            {
                PartitionKey = authority + BrigadeBaseEntity.SEPARATOR + brigade + BrigadeBaseEntity.SEPARATOR + TypeName + BrigadeBaseEntity.SEPARATOR + ContainerId.Trim();
            }
            else
            {
                PartitionKey = authority + BrigadeBaseEntity.SEPARATOR + brigade + BrigadeBaseEntity.SEPARATOR + TypeName;
            }
        }
    }
}
