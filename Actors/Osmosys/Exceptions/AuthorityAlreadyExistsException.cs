using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Osmosys.DataContracts;

namespace Osmosys.Exceptions
{

    public class AuthorityAlreadyExistsException : Exception
    {
        public AuthorityDto Authority { get; set; }
        public override string Message => $"Authority {Authority.Name} '{Authority.Path}' already exists.";

    }
}
