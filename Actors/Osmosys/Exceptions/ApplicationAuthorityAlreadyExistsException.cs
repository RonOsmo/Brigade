using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Osmosys.DataContracts;

namespace Osmosys.Exceptions
{
    public class ApplicationAuthorityAlreadyExistsException : Exception
    {
        public ApplicationAuthorityDto ApplicationAuthority { get; set; }
        public ApplicationAuthorityDto PrevApplicationAuthority { get; set; }

        public override string Message
            =>
            $"{PrevApplicationAuthority.Path} for Version:'{ApplicationAuthority.ApplicationPath}' already exists with Version '{PrevApplicationAuthority.ApplicationPath}'."
            + ((ApplicationAuthority.ApplicationPath == PrevApplicationAuthority.ApplicationPath)
                ? ""
                : " Consider Upgrading instead.")
            ;
    }
}
