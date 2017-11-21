using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Query;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Generator;
using Microsoft.ServiceFabric.Actors.Query;

namespace IoTDemoConsole.Helpers
{

    /// <summary>
    /// Class ActorHelper.
    /// </summary>
    public static class ActorHelper
    {
        /// <summary>
        /// Enumerates the actors int64 partition.
        /// </summary>
        /// <typeparam name="TActorInterface">The type of the t actor interface.</typeparam>
        /// <param name="serviceUri">The service URI.</param>
        /// <param name="onlyAlive">if set to <c>true</c> execute only for alive actors.</param>
        /// <param name="state">The state.</param>
        /// <param name="doSomething">Code to execute for every actor.</param>
        /// <param name="enterIntoPartition">Code to execute after change partition.</param>
        /// <param name="exitFromPartition">Code to execute before change partition.</param>
        /// <returns>Task.</returns>
        public static async Task EnumerateActorsInt64Partition<TActorInterface>(Uri serviceUri, bool onlyAlive,
                object state, Func<ActorId, Partition, object, Task> doSomething,
                Func<Partition, int, object, Task> enterIntoPartition,
                Func<Partition, int, object, Task> exitFromPartition)
        {
            var actorUri = ActorNameFormat.GetFabricServiceUri(typeof(TActorInterface), serviceUri);
            int counter = 0;
            int totalItem = await GetNumberOfActors<TActorInterface>(serviceUri, onlyAlive);
            using (var client = new FabricClient())
            {
                var partitions = await client.QueryManager.GetPartitionListAsync(actorUri);

                foreach (var partition in partitions)
                {
                    if (enterIntoPartition != null) await enterIntoPartition(partition, totalItem, state);
                    ContinuationToken continuationToken = null;
                    var partitionInformation = (Int64RangePartitionInformation)partition.PartitionInformation;

                    var actorService = ActorServiceProxy.Create(actorUri, partitionInformation.LowKey);

                    PagedResult<ActorInformation> actorList = null;
                    do
                    {
                        actorList = await actorService.GetActorsAsync(continuationToken, default(CancellationToken));
                        for (int i = 0; i < actorList.Items.Count(); i++, counter++)
                        {
                            var actorInfo = actorList.Items.ElementAt(i);
                            if ((onlyAlive && actorInfo.IsActive) || !onlyAlive)
                            {
                                await doSomething(actorInfo.ActorId, partition, state);
                            }
                        }
                        continuationToken = actorList.ContinuationToken;
                    } while (continuationToken != null);
                    if (exitFromPartition != null) await exitFromPartition(partition, totalItem, state);
                }
            }
        }


        /// <summary>
        /// Gets the number of actors.
        /// </summary>
        /// <typeparam name="TActorInterface">The type of the t actor interface.</typeparam>
        /// <param name="serviceUri">The service URI.</param>
        /// <param name="onlyAlive">if set to <c>true</c> [only alive].</param>
        /// <returns>Task&lt;System.Int32&gt;.</returns>
        private static async Task<int> GetNumberOfActors<TActorInterface>(Uri serviceUri, bool onlyAlive)
        {
            int counter = 0;
            using (var client = new FabricClient())
            {
                var partitions = await client.QueryManager.GetPartitionListAsync(serviceUri);

                foreach (var partition in partitions)
                {
                    ContinuationToken continuationToken = null;
                    var partitionInformation = (Int64RangePartitionInformation)partition.PartitionInformation;
                    var actorService = ActorServiceProxy.Create(serviceUri, partitionInformation.LowKey);

                    var queryResults = await actorService.GetActorsAsync(continuationToken, default(CancellationToken));
                    if (onlyAlive)
                        counter += queryResults.Items.Count(a => a.IsActive);
                    else
                        counter += queryResults.Items.Count();
                }
            }
            return counter;
        }

        /// <summary>
        /// Gets the partions.
        /// </summary>
        /// <typeparam name="TActorInterface">The type of the t actor interface.</typeparam>
        /// <param name="serviceUri">The service URI.</param>
        /// <returns>Task&lt;IEnumerable&lt;ServicePartitionInformation&gt;&gt;.</returns>
        private static async Task<IEnumerable<ServicePartitionInformation>> GetPartions<TActorInterface>(Uri serviceUri)
        {
            var result = new List<ServicePartitionInformation>();
            using (var client = new FabricClient())
            {
                var partitions = await client.QueryManager.GetPartitionListAsync(serviceUri);

                foreach (var partition in partitions)
                {
                    partitions.Add(partition);
                }
            }
            return result;
        }

        
    }
}
