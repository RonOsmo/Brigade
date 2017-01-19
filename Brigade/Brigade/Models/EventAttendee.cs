using Brigade.Abstractions;
using Newtonsoft.Json;

namespace Brigade.Models
{
	public class EventAttendee : EntityBase<Event>
    {
		#region EventRoleID and EventRole properties

		private string _eventRoleId;
		private EventRole _eventRole;

		public string RoleId
		{
			get
			{
				return _eventRoleId;
			}
			set
			{
				if (value != _eventRoleId)
				{
					_eventRoleId = value;
					_eventRole = null;
					OnPropertyChanged();
				}
			}
		}

		[JsonIgnore]
		public EventRole EventRole
		{
			get
			{
				if (_eventRole == null && !string.IsNullOrWhiteSpace(_eventRoleId))
				{
					LocalDB.EventRoleTable.LookupAsync(_eventRoleId).ContinueWith(x =>
					{
						_eventRole = x.Result;
					});
				}
				return _eventRole;
			}
			set
			{
				if ((_eventRole != null && value == null) || (_eventRole == null && value != null) || (_eventRole != null && value != null && !_eventRole.Equals(value)))
				{
					_eventRole = value;
					if (value == null)
						_eventRoleId = null;
					else
						_eventRoleId = value.Id;
					OnPropertyChanged();
				}
			}
		}

		#endregion
		#region AttendeeID and Attendee properties

		private string _attendeeId;
		private User _attendee;

		public string AttendeeId
		{
			get
			{
				return _attendeeId;
			}
			set
			{
				if (value != _attendeeId)
				{
					_attendeeId = value;
					_attendee = null;
					OnPropertyChanged();
				}
			}
		}

		[JsonIgnore]
		public User Attendee
		{
			get
			{
				if (_attendee == null && !string.IsNullOrWhiteSpace(_attendeeId))
				{
					LocalDB.UserTable.LookupAsync(_attendeeId).ContinueWith(x =>
					{
						_attendee = x.Result;
					});
				}
				return _attendee;
			}
			set
			{
				if ((_attendee != null && value == null) || (_attendee == null && value != null) || (_attendee != null && value != null && !_attendee.Equals(value)))
				{
					_attendee = value;
					if (value == null)
						_attendeeId = null;
					else
						_attendeeId = value.Id;
					OnPropertyChanged();
				}
			}
		}

		#endregion
	}
}
