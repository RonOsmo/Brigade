using Osmosys.DataContracts;
using System;

namespace Osmosys.Exceptions
{
    public class ChildHasUsersException : Exception
    {
        public AuthorityDto Child { get; set; }
        public int UserCount { get; set; }
        public override string Message => $"Child Container '{Child.Path}' still has {UserCount} Users. Cannot delete.";
    }

}
