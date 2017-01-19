using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Osmosys.DataContracts;

namespace Osmosys.Exceptions
{
    public class RoleVersionMismatchException : Exception
    {
        public RoleDto Role { get; set; }
        public RoleDto ExistingRole { get; set; }

        public override string Message
            =>
            $"Can't update user {Role.Name} Version={Role.Version}, because it is not the latest Version={ExistingRole.Version}."
            ;
    }
}
