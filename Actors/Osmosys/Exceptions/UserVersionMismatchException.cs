using Osmosys.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osmosys.Exceptions
{

    public class UserVersionMismatchException : Exception
    {
        public UserDto User { get; set; }
        public UserDto ExistingUser { get; set; }

        public override string Message
            =>
            $"Can't update user {User.UserName} Version={User.Version}, because it is not the latest Version={ExistingUser.Version}."
            ;
    }
}
