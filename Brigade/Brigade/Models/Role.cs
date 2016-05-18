using System.Collections.Generic;

namespace Brigade.Models
{
	public class Role : AnyContainer
    {
		public string RoleId { get; set; }
		public string RoleDescription { get; set; }
		public IList<UserRole> Users { get; set; }
	}
}
