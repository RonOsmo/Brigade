using System;
using System.Collections.Generic;
using System.Text;

namespace Brigade.Models
{
    public class UserRole : EntityBase<User>
    {
		public Role Role { get; set; }
		public bool AssignmentApproved { get; set; }
    }
}
