using System;
using System.Runtime.Serialization;

namespace Osmosys.Stocktake.Models
{
    [DataContract(Name = "StockDto", Namespace = "http://osmo.com.au/brigade")]
	public class StockDto : IExtensibleDataObject
	{
		[DataMember]
		public string Id { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
		public string BrigadeId { get; set; }

		[DataMember]
		public string ContainerId { get; set; }

		[DataMember]
		public string AssetId { get; set; }

		[DataMember]
		public string Etag { get; set; }

		[DataMember]
		public DateTime? MissingDate { get; set; }

		[DataMember]
		public string MissingByUserId { get; set; }

		[DataMember]
		public string MissingAtLocation { get; set; }

		[DataMember]
		public DateTime? LastSeenDate { get; set; }

		[DataMember]
		public string LastSeenByUserId { get; set; }

		[DataMember]
		public string LastSeenAtLocation { get; set; }

		[DataMember]
		public StockStatus Status { get; set; }

		[DataMember]
		public bool IsContainer { get; set; }

		// support forward compatible contracts
		public virtual ExtensionDataObject ExtensionData { get; set; }
	}

	[DataContract(Name = "StockStatus", Namespace = "http://osmo.com.au/brigade")]
	public enum StockStatus
	{
		NotSpecified = 0,
		Sighted,
		Missing,
		Disposed,
	}

}
