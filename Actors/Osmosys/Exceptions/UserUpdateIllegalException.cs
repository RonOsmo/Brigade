using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Osmosys.DataContracts;

namespace Osmosys.Exceptions
{
    public class UserUpdateIllegalException : Exception
    {
        public AuthorityDto Authority { get; set; }
        public UserDto User { get; set; }
        public override string Message
            =>
            $"Can't update user {User.UserName} of container: '{User.AuthorityPath}', because it does not exist in this Authority: '{Authority.Path}'."
            ;
    }

}
