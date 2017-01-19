using Brigade.Abstractions;
using Newtonsoft.Json;

namespace Brigade.Models
{
	public class EventRole : EntityBase<EventType>
    {
        public string Name { get; set; }
		public string Description { get; set; }
		#region RoleID and Role properties - to restrict who can be in this role

		private string _roleId;
		private Role _role;

		public string RoleId
		{
			get
			{
				return _roleId;
			}
			set
			{
				if (value != _roleId)
				{
					_roleId = value;
					_role = null;
					OnPropertyChanged();
				}
			}
		}

		[JsonIgnore]
		public Role Role
		{
			get
			{
				if (_role == null && !string.IsNullOrWhiteSpace(_roleId))
				{
					LocalDB.RoleTable.LookupAsync(_roleId).ContinueWith(x =>
					{
						_role = x.Result;
					});
				}
				return _role;
			}
			set
			{
				if ((_role != null && value == null) || (_role == null && value != null) || (_role != null && value != null && !_role.Equals(value)))
				{
					_role = value;
					if (value == null)
						_roleId = null;
					else
						_roleId = value.Id;
					OnPropertyChanged();
				}
			}
		}

		#endregion
		#region CertificateOutcomeId and CertificateOutcome properties

		private string _certificateOutcomeId;
		private Certificate _certificateOutcome;

		public string CertificateOutcomeId
		{
			get
			{
				return _certificateOutcomeId;
			}
			set
			{
				if (value != _certificateOutcomeId)
				{
					_certificateOutcomeId = value;
					_certificateOutcome = null;
					OnPropertyChanged();
				}
			}
		}

		[JsonIgnore]
		public Certificate CertificateOutcome
		{
			get
			{
				if (_certificateOutcome == null && !string.IsNullOrWhiteSpace(_certificateOutcomeId))
				{
					LocalDB.CertificateTable.LookupAsync(_certificateOutcomeId).ContinueWith(x =>
					{
						_certificateOutcome = x.Result;
					});
				}
				return _certificateOutcome;
			}
			set
			{
				if ((_certificateOutcome != null && value == null) || (_certificateOutcome == null && value != null) || (_certificateOutcome != null && value != null && !_certificateOutcome.Equals(value)))
				{
					_certificateOutcome = value;
					if (value == null)
						_certificateOutcomeId = null;
					else
						_certificateOutcomeId = value.Id;
					OnPropertyChanged();
				}
			}
		}

		#endregion
	}
}
