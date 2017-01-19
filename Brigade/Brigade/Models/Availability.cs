using System;
using Brigade.Abstractions;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices.Sync;


namespace Brigade.Models
{
    public class Availability : EntityBase<EventType>
    {
		[JsonIgnore]
		public string Name { get { return Container.Name; } }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
		#region UserId and User properties

		string _userId;
		User _user;

		[JsonIgnore]
		public User User
		{
			get
			{
				if (_user == null && !string.IsNullOrWhiteSpace(_userId))
				{
					LocalDB.UserTable.LookupAsync(_userId).ContinueWith(x =>
					{
						_user = x.Result;
					});
				}
				return _user;
			}
			set
			{
				if ((_user != null && value == null) || (_user == null && value != null) || (_user != null && value != null && !_user.Equals(value)))
				{
					_user = value;
					if (value == null)
						_userId = null;
					else
						_userId = value.Id;
					OnPropertyChanged();
				}
			}
		}

		public string UserId
		{
			get { return _userId; }
			set
			{
				if (value != _userId)
				{
					_userId = value;
					_user = null;
					OnPropertyChanged();
				}
			}
		}

		#endregion
	}
}
