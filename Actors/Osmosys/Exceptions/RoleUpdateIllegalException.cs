using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Osmosys.DataContracts;

namespace Osmosys.Exceptions
{
    public class RoleUpdateIllegalException : Exception
    {

        public string AuthorityPath { get; set; }
        public RoleDto Role { get; set; }

        public override string Message
            =>
            $"Can't update user {Role.Name} of container: '{Role.AuthorityPath}', because it does not exist in this Authority: '{((AuthorityPath == null) ? "" : string.Empty)}'."
            ;
    }
}
