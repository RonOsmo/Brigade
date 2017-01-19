using System;
using Brigade.Abstractions;
using Newtonsoft.Json;

namespace Brigade.Models
{
	public class UserTask : EntityBase<User>
    {
		public DateTime? ActualDate { get; set; }
		public string Description { get; set; }
		public bool DoNotExecuteBefore { get; set; }
		public TimeSpan? RemindBefore { get; set; }
		public TimeSpan? RemindersAfter { get; set; }
		#region OwnerId and Owner properties

		private string _ownerId;
		private IEntityId _owner;

		public string OwnerId
		{
			get
			{
				return _ownerId;
			}
			set
			{
				if (value != _ownerId)
				{
					_ownerId = value;
					_owner = null;
					OnPropertyChanged();
				}
			}
		}

		[JsonIgnore]
		public IEntityId Owner
		{
			get
			{
				if (_owner == null && !string.IsNullOrWhiteSpace(_ownerId))
				{
					LocalDB.RoleTable.LookupAsync(_ownerId).ContinueWith(x =>
					{
						_owner = x.Result;
					});
				}
				return _owner;
			}
			set
			{
				if ((_owner != null && value == null) || (_owner == null && value != null) || (_owner != null && value != null && !_owner.Equals(value)))
				{
					_owner = value;
					if (value == null)
						_ownerId = null;
					else
						_ownerId = value.Id;
					OnPropertyChanged();
				}
			}
		}

		#endregion
		#region SentByUserId and SentByUser properties

		private string _sentByUserId;
		private User _sentByUser;

		public string SentByUserId
		{
			get
			{
				return _sentByUserId;
			}
			set
			{
				if (value != _sentByUserId)
				{
					_sentByUserId = value;
					_sentByUser = null;
					OnPropertyChanged();
				}
			}
		}

		[JsonIgnore]
		public User SentByUser
		{
			get
			{
				if (_sentByUser == null && !string.IsNullOrWhiteSpace(_sentByUserId))
				{
					LocalDB.UserTable.LookupAsync(_sentByUserId).ContinueWith(x =>
					{
						_sentByUser = x.Result;
					});
				}
				return _sentByUser;
			}
			set
			{
				if ((_sentByUser != null && value == null) || (_sentByUser == null && value != null) || (_sentByUser != null && value != null && !_sentByUser.Equals(value)))
				{
					_sentByUser = value;
					if (value == null)
						_sentByUserId = null;
					else
						_sentByUserId = value.Id;
					OnPropertyChanged();
				}
			}
		}

		#endregion
	}
}
