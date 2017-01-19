using Osmosys.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osmosys.Exceptions
{

    public class IsNotAChildException : Exception
    {
        public AuthorityDto Child { get; set; }
        public AuthorityDto Parent { get; set; }
        public override string Message
            => $"Child Container '{Child.Path}' must be a superset of its parent's Container '{Parent.Path}'.";
    }
}
