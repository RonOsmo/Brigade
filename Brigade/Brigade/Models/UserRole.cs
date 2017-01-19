using System;
using System.Collections.Generic;
using Brigade.Abstractions;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace Brigade.Models
{
    public class UserRole : EntityBase<User>
    {

		#region RoleID and Role properties

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
		public bool AssignmentApproved { get; set; }
    }
}
