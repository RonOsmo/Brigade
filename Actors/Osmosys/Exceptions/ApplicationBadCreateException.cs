using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Osmosys.DataContracts;

namespace Osmosys.Exceptions
{
    public class ApplicationBadCreateException : Exception
    {
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string CurrentVersion { get; set; }
        public ApplicationDto Existing { get; set; }

        public override string Message
            =>
            $"Can't create Application {Identifier} {Name} with Version: '{CurrentVersion}'. Application Application: {Existing.Id} {Existing.Name} with Version: '{Existing.CurrentVersion}'."
            ;
    }
}
