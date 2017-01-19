using System;
using System.Collections.Generic;
using Brigade.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace Brigade.Models
{
    public class User : EntityBase<Brigade>
    {
		public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public TimeSpan ExtraReminder { get; set; }

		public Task<IEnumerable<UserRole>> GetUserRolesAsync()
		{
			var query =
				LocalDB.UserRoleTable.CreateQuery()
				.Where(ur => ur.ContainerId == this.Id)
				.OrderBy(ur => ur.Role.Name)
				.ToEnumerableAsync();
			return query;
		}

		public Task<IEnumerable<UserCertificate>> GetUserCertificatesAsync()
		{
			var query =
				LocalDB.UserCertificateTable.CreateQuery()
				.Where(uc => uc.ContainerId == this.Id)
				.OrderBy(uc => uc.Certificate.Name)
				.ToEnumerableAsync();
			return query;
		}
	}
}
