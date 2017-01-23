﻿using System;
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
    internal class Usertask : Actor, IUsertask
    {
        private readonly ActorService _actorService;
        private readonly ActorId _actorId;

        /// <summary>
        /// Initializes a new instance of UserTask
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public Usertask(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
            _actorService = actorService;
            _actorId = actorId;
        }

        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, $"Usertask {_actorId} with ServiceName: '{_actorService.ActorTypeInformation.ServiceName}' activated.");
            return Task.Delay(1);
            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see https://aka.ms/servicefabricactorsstateserialization

            //await this.StateManager.SetStateAsync("LoggedInCount", 0);
        }

        public async Task AddTaskAsync(UserTaskDto item)
        {
            await this.StateManager.AddStateAsync(item.Path, item);
            await this.StateManager.AddOrUpdateStateAsync("TaskCount", 0, (k, v) => ++v);
        }

        public async Task<List<UserTaskDto>> ListTasksAsync()
        {
            var items = new List<UserTaskDto>();
            var names = await this.StateManager.GetStateNamesAsync();
            foreach (string name in names)
            {
                var item = await this.StateManager.GetStateAsync<UserTaskDto>(name);
                items.Add(item);
            }
            return items;
        }

        public async Task<int> RemoveTaskAsync(string owningEntityTypeName, string owningEntityId)
        {
            string itemKey = owningEntityTypeName + "." + owningEntityId;
            var item = await this.StateManager.TryGetStateAsync<UserTaskDto>(itemKey);
            if (item.HasValue)
            {
                await this.StateManager.RemoveStateAsync(itemKey);
                await this.StateManager.AddOrUpdateStateAsync("TaskCount", 0, (k, v) => --v);

                if (item.Value.Id != Guid.Empty)
                {
                    var grouptaskProxy = ActorProxy.Create<IGrouptask>(new ActorId(item.Value.Id));
                    await grouptaskProxy.RemoveTasksAsync();
                }
            }

            var taskCount = await this.StateManager.GetOrAddStateAsync("TaskCount", 0);
            if (taskCount == 0)
                await this.StateManager.ClearCacheAsync();
            return taskCount;
        }

    }
}
