using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

namespace DataTableStorage.Models
{

    [System.ComponentModel.DataAnnotations.Schema.Table("User")]
    public class User : BrigadeBaseEntity
    {
        protected List<BrigadeBaseEntity> Roles = new List<BrigadeBaseEntity>();

        [Required]
        [StringLength(maximumLength: 20, MinimumLength = 1)]
        public string UserName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public IList<BrigadeBaseEntity> AddRole(string roleName)
        {
            if (!Roles.Any(ur => ur.RowKey == roleName || ur.ContainerId == roleName))
            {
                Roles.Add(new UserRole { ContainerId = UserName, RoleName = roleName });
                Roles.Add(new RoleUser { ContainerId = roleName, UserName = UserName });
            }
            return Roles;
        }

        public override void SetId(string authority, string brigade)
        {
            base.SetId(authority, brigade);
            RowKey = UserName;

            foreach (var role in Roles)
            {
                role.SetId(authority, brigade);
            }
        }
    }

    [System.ComponentModel.DataAnnotations.Schema.Table("User")]
    public class UserRole : BrigadeBaseEntity
    {
        [Required]
        [StringLength(maximumLength: 50, MinimumLength = 1)]
        public string RoleName { get; set; }

        public override void SetId(string authority, string brigade)
        {
            base.SetId(authority, brigade);

            RowKey = RoleName;
        }

        public override string TypeName
        {
            get
            {
                return "User";
            }
        }
    }

    [System.ComponentModel.DataAnnotations.Schema.Table("User")]
    public class RoleUser : BrigadeBaseEntity
    {
        [Required]
        [StringLength(maximumLength: 20, MinimumLength = 1)]
        public string UserName { get; set; }

        public override void SetId(string authority, string brigade)
        {
            base.SetId(authority, brigade);

            RowKey = UserName;
        }

        public override string TypeName
        {
            get
            {
                return "Role";
            }
        }
    }
}
