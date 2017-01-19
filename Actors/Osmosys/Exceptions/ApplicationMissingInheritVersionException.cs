using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Osmosys.DataContracts;

namespace Osmosys.Exceptions
{
    public class ApplicationMissingInheritVersionException : Exception
    {
        public ApplicationDto Application { get; set; }
        public string InheritsVersion { get; set; }
        public string CurrentVersion { get; set; }

        public override string Message
            =>
            $"Application: {Application.Id} {Application.Name} does not contain Inherited Version '{InheritsVersion}' for new Version '{CurrentVersion}'."
            ;
    }

}
