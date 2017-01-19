using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Microsoft.ServiceFabric.Actors;
using Osmosys.DataContracts;

namespace Osmosys.Abstractions
{
    public interface IViewModel : IActor
    {
        Task AddTaskAsync(TaskTypeDto task);
        Task CreateAsync(ViewModelDto viewModel);
        Task<bool> HasTaskTypeAsync(string taskTypeName);
        Task RemoveTaskAsync(TaskTypeDto task);
    }

}
