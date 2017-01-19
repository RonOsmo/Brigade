using System;
using Brigade.Abstractions;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices.Sync;

namespace Brigade.Models
{
	public class Asset : AnyContainer
    {
        public string Description { get; set; }
        public DateTime? MissingDate { get; set; }
        public string MissingAtLocation { get; set; }
        public DateTime? LastSeenDate { get; set; }
        public string LastSeenAtLocation { get; set; }
        public StockStatus Status { get; set; }
        public bool IsContainer { get; set; }

        #region BrigadeId and Brigade properties

        public string BrigadeId { get; set; }

        private Brigade _brigade;

        [JsonIgnore]
        public Brigade Brigade
        {
            get
            {
                if (_brigade == null && !string.IsNullOrWhiteSpace(BrigadeId))
                {
                    LocalDB.BrigadeTable?.LookupAsync(BrigadeId).ContinueWith(x =>
                    {
                        _brigade = x.Result;
                    });
                }
                return _brigade;
            }
            set
            {
                if ((_brigade != null && value == null) || (_brigade == null && value != null) || (_brigade != null && value != null && !_brigade.Equals(value)))
                {
                    _brigade = value;
                    OnPropertyChanged();
                    if (value == null)
                        BrigadeId = null;
                    else
                        BrigadeId = value.Id;
                }
            }
        }

        #endregion
        #region MissingByUserId and MissingByUser properties

        public string MissingByUserId { get; set; }

		private User _missingByUser;

		[JsonIgnore]
        public User MissingByUser
        {
			get
			{
				if (_missingByUser == null && !string.IsNullOrWhiteSpace(MissingByUserId))
				{
					LocalDB.UserTable?.LookupAsync(MissingByUserId).ContinueWith(x =>
					{
                        _missingByUser = x.Result;
					});
				}
				return _missingByUser;
			}
			set
			{
				if ((_missingByUser != null && value == null) || (_missingByUser == null && value != null) || (_missingByUser != null && value != null && !_missingByUser.Equals(value)))
				{
                    _missingByUser = value;
					OnPropertyChanged();
                    MissingByUserId = value?.Id;
				}
			}
		}

        #endregion
        #region LastSeenByUserId and LastSeenByUser properties

        public string LastSeenByUserId { get; set; }

		User _lastSeenByUser;

		[JsonIgnore]
        public User LastSeenByUser
        {
			get
			{
				if (_lastSeenByUser == null && !string.IsNullOrWhiteSpace(LastSeenByUserId))
				{
                    LocalDB.UserTable?.LookupAsync(LastSeenByUserId).ContinueWith(x =>
                    {
                        _lastSeenByUser = x.Result;
                    });
                }
                return _lastSeenByUser;
			}
			set
			{
				if ((_lastSeenByUser != null && value == null) || (_lastSeenByUser == null && value != null) || (_lastSeenByUser != null && value != null && !_lastSeenByUser.Equals(value)))
				{
                    _lastSeenByUser = value;
					OnPropertyChanged();
                    LastSeenByUserId = value?.Id;
                }
            }
		}

		#endregion
	}
}
