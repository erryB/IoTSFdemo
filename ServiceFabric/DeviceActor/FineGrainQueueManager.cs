using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace DeviceActor
{
    /// <summary>
    /// Class FineGrainQueueManager.
    /// </summary>
    public static class FineGrainQueueManager
    {
        #region Private Methods
        /// <summary>
        /// Gets the name of the queue head identity.
        /// </summary>
        /// <param name="queueName">Name of the queue.</param>
        /// <returns>System.String.</returns>
        private static string GetQueueHeadIdentityName(string queueName)
        {
            return $"{queueName }_HeadIdentity";
        }

        /// <summary>
        /// Gets the name of the queue identity.
        /// </summary>
        /// <param name="queueName">Name of the queue.</param>
        /// <returns>System.String.</returns>
        private static string GetQueueIdentityName(string queueName)
        {
            return $"{queueName }_Identity";
        }

        /// <summary>
        /// Gets the queue item key.
        /// </summary>
        /// <param name="queueName">Name of the queue.</param>
        /// <param name="itemIdentity">The item identity.</param>
        /// <returns>System.String.</returns>
        private static string GetQueueItemKey(string queueName, long itemIdentity)
        {
            return $"{queueName}_{itemIdentity}";
        }

        /// <summary>
        /// Sets the head identity.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="queueName">Name of the queue.</param>
        /// <param name="counter">The counter.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        private static Task SetHeadIdentity(this IActorStateManager state, string queueName,
            long counter, CancellationToken cancellationToken = default(CancellationToken))
        {
            return state.SetStateAsync<long>(GetQueueHeadIdentityName(queueName), counter, cancellationToken);
        }

        /// <summary>
        /// Gets the head identity.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="queueName">Name of the queue.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;System.Int64&gt;.</returns>
        private static async Task<long> GetHeadIdentity(this IActorStateManager state, string queueName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var counter = await state.TryGetStateAsync<long>(GetQueueHeadIdentityName(queueName), cancellationToken);
            return counter.HasValue ? counter.Value : 0;
        }

        /// <summary>
        /// Gets the identity.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="queueName">Name of the queue.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;System.Int64&gt;.</returns>
        private static async Task<long> GetIdentity(this IActorStateManager state, string queueName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var identity = await state.TryGetStateAsync<long>(GetQueueIdentityName(queueName), cancellationToken);
            return identity.HasValue ? identity.Value : -1;
        }

        /// <summary>
        /// Sets the identity.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="queueName">Name of the queue.</param>
        /// <param name="identity">The identity.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        private static Task SetIdentity(this IActorStateManager state, string queueName,
            long identity, CancellationToken cancellationToken = default(CancellationToken))
        {
            return state.SetStateAsync<long>(GetQueueIdentityName(queueName), identity, cancellationToken);
        }


        #endregion

        #region EnqueueAsync
        /// <summary>
        /// enqueue as an asynchronous operation.
        /// </summary>
        /// <typeparam name="TElement">The type of the t element.</typeparam>
        /// <param name="state">The state.</param>
        /// <param name="queueName">Name of the queue.</param>
        /// <param name="element">The element.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;System.Int64&gt;.</returns>
        /// <exception cref="System.NullReferenceException">state</exception>
        /// <exception cref="System.ArgumentException">queueName</exception>
        /// <exception cref="System.InsufficientMemoryException">Memory overflow in the queue!!!</exception>
        public static async Task<long> EnqueueAsync<TElement>(this IActorStateManager state, string queueName,
            TElement element, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (state == null)
                throw new NullReferenceException(nameof(state));
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException(nameof(queueName));

            var currentIdentity = await state.GetIdentity(queueName, cancellationToken);
            if (!cancellationToken.IsCancellationRequested)
            {
                if (currentIdentity == long.MaxValue)
                {
                    var currentHead = await state.GetHeadIdentity(queueName, cancellationToken);
                    if (currentHead == 0)
                        throw new InsufficientMemoryException("Memory overflow in the queue!!!");
                    currentIdentity = 0;
                }
                else
                {
                    currentIdentity++;
                }
                var itemKey = GetQueueItemKey(queueName, currentIdentity);
                await state.AddOrUpdateStateAsync(itemKey, element, (k, v) => element, cancellationToken);
                await state.SetIdentity(queueName, currentIdentity, cancellationToken);
            }

            return await state.GetQueueLengthAsync(queueName, cancellationToken);
        }
        #endregion

        #region DequeueAsync
        /// <summary>
        /// dequeue as an asynchronous operation.
        /// </summary>
        /// <typeparam name="TElement">The type of the t element.</typeparam>
        /// <param name="state">The state.</param>
        /// <param name="queueName">Name of the queue.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;TElement&gt;.</returns>
        /// <exception cref="System.NullReferenceException">state</exception>
        /// <exception cref="System.ArgumentException">queueName</exception>
        public static async Task<TElement> DequeueAsync<TElement>(this IActorStateManager state, string queueName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (state == null)
                throw new NullReferenceException(nameof(state));
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException(nameof(queueName));
            TElement result = default(TElement);

            var currentHeadIdentity = await state.GetHeadIdentity(queueName, cancellationToken);
            var currentIdentity = await state.GetIdentity(queueName, cancellationToken);
            var headGTIdentity = currentHeadIdentity > currentIdentity; // utile per capire se l'head supera il massimo valore long durante il metodo e se serve di resettare gli indici

            var itemKey = GetQueueItemKey(queueName, currentHeadIdentity);
            var element = await state.TryGetStateAsync<TElement>(itemKey, cancellationToken);

            if (element.HasValue)
            {
                // elemento in coda esistente e lo rimuovo
                await state.TryRemoveStateAsync(itemKey, cancellationToken);
                result = element.Value;

                // Check su overflow (se l'head arriva al valore massimo di long, debbo resettarlo a zero)
                if (currentHeadIdentity == long.MaxValue)
                {
                    currentHeadIdentity = 0;
                }
                else
                {
                    currentHeadIdentity++;
                }

                // resetto gli indici se necessario
                if (currentHeadIdentity > currentIdentity && !headGTIdentity) //in questo caso siamo nella situazione in cui l'head era precedentemente minore dell'identity e lo ha superato, quindi non ci sono elementi in coda 
                {
                    // in questo caso non ci sono elementi in coda, quindi possiamo resettare l'indice di head e quello dell'identity
                    await state.SetIdentity(queueName, -1, cancellationToken);
                    await state.SetHeadIdentity(queueName, 0, cancellationToken);
                }
                else
                {
                    await state.SetHeadIdentity(queueName, currentHeadIdentity, cancellationToken);
                }
            }

            return result;
        }

        #endregion 

        #region PurgeQueue
        /// <summary>
        /// Purges the queue.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="queueName">Name of the queue.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;System.Boolean&gt;.</returns>
        /// <exception cref="System.NullReferenceException">state</exception>
        /// <exception cref="System.ArgumentException">queueName</exception>
        public static async Task<bool> PurgeQueue(this IActorStateManager state, string queueName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (state == null)
                throw new NullReferenceException(nameof(state));
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException(nameof(queueName));

            bool result = true;

            var queueItemKeys = (await state.GetStateNamesAsync(cancellationToken))
                .Where(k => k.StartsWith($"{queueName}_")).ToList();
            foreach (var queueItemKey in queueItemKeys)
            {
                result = result & await state.TryRemoveStateAsync(queueItemKey, cancellationToken);
            }
            await state.SetIdentity(queueName, -1, cancellationToken);
            await state.SetHeadIdentity(queueName, 0, cancellationToken);
            return result;
        }

        #endregion

        #region GetQueueLengthAsync
        /// <summary>
        /// get queue length as an asynchronous operation.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="queueName">Name of the queue.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;System.Int64&gt;.</returns>
        /// <exception cref="System.NullReferenceException">state</exception>
        /// <exception cref="System.ArgumentException">queueName</exception>
        public static async Task<long> GetQueueLengthAsync(this IActorStateManager state, string queueName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (state == null)
                throw new NullReferenceException(nameof(state));
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException(nameof(queueName));

            long counter = 0;
            var currentIdentity = await state.GetIdentity(queueName, cancellationToken);
            var currentHead = await state.GetHeadIdentity(queueName, cancellationToken);
            if ((currentIdentity == -1 && currentHead == 0) || currentHead <= currentIdentity)
            {
                counter += (currentIdentity - currentHead + 1);
            }
            else
            {
                counter += ((long.MaxValue - currentHead + 1) + (currentIdentity + 1));
            }
            return counter;
        }

        #endregion

        #region PeekQueueAsync
        /// <summary>
        /// peek queue as an asynchronous operation.
        /// </summary>
        /// <typeparam name="TElement">The type of the t element.</typeparam>
        /// <param name="state">The state.</param>
        /// <param name="queueName">Name of the queue.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;TElement&gt;.</returns>
        /// <exception cref="System.NullReferenceException">state</exception>
        /// <exception cref="System.ArgumentException">queueName</exception>
        public static async Task<TElement> PeekQueueAsync<TElement>(this IActorStateManager state, string queueName,
                CancellationToken cancellationToken = default(CancellationToken))
        {
            if (state == null)
                throw new NullReferenceException(nameof(state));
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException(nameof(queueName));

            TElement result = default(TElement);

            if (await state.GetQueueLengthAsync(queueName, cancellationToken) > 0)
            {
                var currentHeadIdentity = await state.GetHeadIdentity(queueName, cancellationToken);
                var itemKey = GetQueueItemKey(queueName, currentHeadIdentity);
                var element = await state.TryGetStateAsync<TElement>(itemKey, cancellationToken);

                if (element.HasValue)
                {
                    result = element.Value;
                }
            }
            return result;
        }
        #endregion
    }
}
