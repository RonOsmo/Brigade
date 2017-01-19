using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Osmosys.DataContracts;

namespace Osmosys.Exceptions
{
    public class UserHasNoApplicationAccessException : Exception
    {
        public UserDto User { get; set; }
        public string ApplicationPath { get; set; }
        public override string Message => $"User: {User.UserName} has been given no access to Application Path: '{ApplicationPath}'.";
    }
}
