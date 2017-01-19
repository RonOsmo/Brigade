using System.Collections.Generic;
using Brigade.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace Brigade.Models
{
	public class Role : AnyContainer
    {
		public string Name { get; set; }
		public string Description { get; set; }

		public Task<IEnumerable<UserRole>> GetUserRolesAsync()
		{
			var query = 
				LocalDB.UserRoleTable.CreateQuery()
				.Where(ur => ur.Role.Id == this.Id)
				.ToEnumerableAsync();
			return query;
		}
	}
}
