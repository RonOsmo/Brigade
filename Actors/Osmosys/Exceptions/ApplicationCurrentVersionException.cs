using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Osmosys.DataContracts;

namespace Osmosys.Exceptions
{
    public class ApplicationCurrentVersionException : Exception
    {
        public ApplicationDto Application { get; set; }

        public override string Message
            =>
            $"Application: {Application.Id} {Application.Name} already has CurrentVersion: '{Application.CurrentVersion}'. Can't add Version: '{Application.CurrentVersion}'."
            ;
    }
}
