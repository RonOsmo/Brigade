using System;

namespace Osmosys.Stocktake.Models
{
    public class AssetDto
	{
		public string Id { get; set; }
		public string BrigadeId { get; set; }
		public DateTime DatePurchased { get; set; }
		public decimal PurchaseCost { get; set; }
		public decimal InstallationCost { get; set; }
		public string SerialNumber { get; set; }
		public string Manufacturer { get; set; }
		public string Make { get; set; }
		public string Model { get; set; }
		public DateTime? DateDisposed { get; set; }
		public string MethodOfDisposal { get; set; }
		public string PriceReceived { get; set; }
	}

}
