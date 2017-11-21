using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;

namespace IoTDemoConsole.Helpers
{

    /// <summary>
    /// Class ReliableServiceHelper.
    /// </summary>
    public static class ReliableServiceHelper
    {

        /// <summary>
        /// Enumerates the service int64 partition.
        /// </summary>
        /// <param name="serviceUri">The service URI.</param>
        /// <param name="doSomething">Code to execute for every partition.</param>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        public static async Task<bool> EnumerateServiceInt64Partition(Uri serviceUri,
            Func<Int64RangePartitionInformation, Task<bool>> doSomething)
        {
            bool result = true;
            using (var client = new FabricClient())
            {
                var partitions = (await client.QueryManager.GetPartitionListAsync(serviceUri)).ToList();

                foreach (var partition in partitions)
                {
                    var partitionInformation = (Int64RangePartitionInformation)partition.PartitionInformation;
                    result &= await doSomething(partitionInformation);
                }
            }
            return result;
        }


        /// <summary>
        /// Enumerates the service int64 partition parallel.
        /// </summary>
        /// <param name="serviceUri">The service URI.</param>
        /// <param name="doSomething">Code to execute for every partition.</param>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        public static async Task<bool> EnumerateServiceInt64PartitionParallel(Uri serviceUri,
            Func<Int64RangePartitionInformation, Task<bool>> doSomething)
        {
            bool result = true;
            var tasks = new List<Task<bool>>();
            using (var client = new FabricClient())
            {
                var partitions = (await client.QueryManager.GetPartitionListAsync(serviceUri)).ToList();
                foreach (var partition in partitions)
                {
                    var partitionInformation = (Int64RangePartitionInformation)partition.PartitionInformation;
                    tasks.Add(doSomething(partitionInformation));
                }
            }

            await Task.WhenAll(tasks);
            result = tasks.Select(t => t.Result).Aggregate((a, b) => a & b);
            return result;
        }

    }
}
