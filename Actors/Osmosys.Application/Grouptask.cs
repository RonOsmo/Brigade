using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using Osmosys.Abstractions;
using Osmosys.DataContracts;

namespace Osmosys
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class Grouptask : Actor, IGrouptask
    {
        private ActorService _actorService;

        /// <summary>
        /// Initializes a new instance of Grouptask
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public Grouptask(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
            _actorService = actorService;
        }

        public async Task AddTaskAsync(UserTaskDto item, List<UserDto> users)
        {
            if (string.IsNullOrWhiteSpace(item?.ViewModel?.Name) || string.IsNullOrWhiteSpace(item.ViewModel.ApplicationPath)
                || string.IsNullOrWhiteSpace(item.OwningEntityId) || string.IsNullOrWhiteSpace(item.OwningEntityTypeName)
                || string.IsNullOrWhiteSpace(item.TaskTypeName))
                throw new ArgumentNullException(nameof(item));

            if (item.Id == Guid.Empty)
                item.Id = new Guid();

            foreach (var user in users)
            {
                if (user?.Id == null || user.AuthorityPath == null)
                    throw new ArgumentNullException(nameof(user));

                await this.StateManager.AddStateAsync(user.Path, item);

                var userProxy = ActorProxy.Create<IUser>(new ActorId(user.Path));
                await userProxy.AddTaskAsync(item);
            }
        }

        public async Task RemoveTasksAsync()
        {
            var keys = await this.StateManager.GetStateNamesAsync();
            foreach (var key in keys)
            {
                var userTask = await this.StateManager.GetStateAsync<UserTaskDto>(key);
                var userProxy = ActorProxy.Create<IUser>(new ActorId(key));
                await userProxy.RemoveTaskAsync(userTask.ViewModel, userTask.OwningEntityTypeName, userTask.OwningEntityId);
            }
            await this.StateManager.ClearCacheAsync();
        }
    }
}
