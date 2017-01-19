using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Osmosys.Stocktake.Models;

namespace Osmosys.Stocktake.Abstractions
{

    public interface IStocktake
	{
		/// <summary>
		/// initiate a stocktake of all of a brigade's stock
		/// </summary>
		/// <param name="userDto">the user</param>
		/// <param name="brigadeDto">the brigade (the one for which we are executing the stocktake)</param>
		/// <param name="authority">the authority (for which we are executing the stocktake)</param>
		/// <param name="location">the user's location</param>
		/// <returns>stocktake identifier</returns>
		Task<string> InitiateTopLevelContainerAsync(Models.UserDto userDto, string userContainer, string location);

		/// <summary>
		/// initiate a stocktake of all of a brigade's stock at some particular container and below
		/// </summary>
		/// <param name="container">container to count</param>
		/// <param name="userDto">user responsible for the stocktake</param>
		/// <param name="brigadeDto">the brigade (the one for which we are executing the stocktake)</param>
		/// <param name="authority">the authority (for which we are executing the stocktake)</param>
		/// <param name="location">the user's location</param>
		/// <returns>stocktake identifier</returns>
		Task<string> InitiateContainerAsync(StockDto container, Models.UserDto userDto, Models.BrigadeDto brigadeDto, Models.AuthorityDto authority, string location);

		/// <summary>
		/// take over responsibilty for a portion of the stocktake from someone else
		/// </summary>
		/// <param name="stocktakeId">identifier of stocktake we are doing OR nothing if the stocktake is a new one</param>
		/// <param name="container">container to count</param>
		/// <param name="user">user responsible for the stocktake</param>
		/// <param name="location">the user's location</param>
		/// <returns>stocktake identifier</returns>
		Task<bool> TakeoverContainerAsync(string stocktakeId, StockDto container, UserDto user, string location);

		/// <summary>
		/// set a user as a helper for a container initiated by someone else
		/// </summary>
		/// <param name="stocktakeId">identifier of stocktake</param>
		/// <param name="container">container to help count</param>
		/// <param name="user">user helping with stocktake</param>
		/// <returns>void</returns>
		Task SetHelperAsync(string stocktakeId, StockDto container, UserDto user);

		/// <summary>
		/// set user as available for stocktake
		/// </summary>
		/// <param name="stocktakeId">identifier of stocktake</param>
		/// <param name="user">user available for stocktake</param>
		/// <returns>void</returns>
		Task SetAvailableAsync(string stocktakeId, UserDto user);

		/// <summary>
		/// set user as unavailable for further stocktake
		/// </summary>
		/// <param name="stocktakeId">identifier of stocktake</param>
		/// <param name="user">user no longer available stocktake</param>
		/// <returns>void</returns>
		Task SetUnavailableAsync(string stocktakeId, UserDto user);

		/// <summary>
		/// update various stock as seen
		/// </summary>
		/// <param name="stocktakeId">identifier of stocktake</param>
		/// <param name="container">container to help count</param>
		/// <param name="stocks">list of stock id's</param>
		/// <returns>void</returns>
		Task UpdateSeenAsync(string stocktakeId, StockDto container, IEnumerable<string> stocks);

		/// <summary>
		/// update various stock as missing
		/// </summary>
		/// <param name="stocktakeId">identifier of stocktake</param>
		/// <param name="container">container to help count</param>
		/// <param name="stocks">list of stock id's</param>
		/// <returns>void</returns>
		Task UpdateMissingAsync(string stocktakeId, StockDto container, IEnumerable<string> stocks);
	}

	[DataContract(Name = "Stocktake", Namespace = "http://osmo.com.au/brigade")]
	public class Stocktake : IExtensibleDataObject
	{
		[DataMember]
		public string StocktakeId { get; set; }

		[DataMember]
		public UserDto Initiator { get; set; }

		[DataMember]
		public Models.BrigadeDto Brigade { get; set; }

        [DataMember]
        public Models.AuthorityDto Authority { get; set; }

        [DataMember]
		public DateTime InitiatedDateTime { get; set; }

		[DataMember]
		public DateTime? CompletedDateTime { get; set; }

        // support forward compatible contracts
        [DataMember]
        public virtual ExtensionDataObject ExtensionData { get; set; }
	}

	[DataContract(Name = "StocktakeWip", Namespace = "http://osmo.com.au/brigade")]
	public class StocktakeWip : IExtensibleDataObject
	{
		[DataMember]
		public string ContainerId { get; set; }

		[DataMember]
		public List<UserDto> Helpers { get; set; }

		[DataMember]
		public UserDto AcceptedUserDto { get; set; }

		[DataMember]
		public DateTime? AcceptedDate { get; set; }

		[DataMember]
		public string AcceptedLocation { get; set; }

		[DataMember]
		public List<StockDto> Children { get; set; }

		[DataMember]
		public List<string> StocksSeen { get; set; }

		[DataMember]
		public List<string> StocksMissing { get; set; }

		[DataMember]
		public bool UpdatedStocks { get; set; }

        // support forward compatible contracts
        [DataMember]
        public virtual ExtensionDataObject ExtensionData { get; set; }
	}


}
