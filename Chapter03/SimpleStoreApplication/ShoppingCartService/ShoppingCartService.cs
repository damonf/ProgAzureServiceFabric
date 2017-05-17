﻿using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace ShoppingCartService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class ShoppingCartService : StatefulService, IShoppingCartService
    {
        public ShoppingCartService(StatefulServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
            {
                new ServiceReplicaListener(context =>
                    new WcfCommunicationListener<IShoppingCartService>(
                        wcfServiceObject:this,
                        serviceContext:context,
                        endpointResourceName: "ServiceEndpoint",
                        listenerBinding: WcfUtility.CreateTcpListenerBinding()
                    )
                )};
        }

        public async Task AddItem(ShoppingCartItem item)
        {
            var cart = await StateManager.GetOrAddAsync<IReliableDictionary<string,
                ShoppingCartItem>>("myCart");
            using (var tx = StateManager.CreateTransaction())
            {
                await cart.AddOrUpdateAsync(tx, item.ProductName, item, (k, v) => item);
                await tx.CommitAsync();
            }
        }

        public async Task DeleteItem(string productName)
        {
            var cart = await StateManager.GetOrAddAsync<IReliableDictionary<string,
                ShoppingCartItem>>("myCart");
            using (var tx = StateManager.CreateTransaction())
            {
                var existing = await cart.TryGetValueAsync(tx, productName);
                if (existing.HasValue)
                    await cart.TryRemoveAsync(tx, productName);
                await tx.CommitAsync();
            }
        }

        public async Task<List<ShoppingCartItem>> GetItems()
        {
            var cart = await StateManager.GetOrAddAsync<IReliableDictionary<string, ShoppingCartItem>>("myCart");

            using (var tx = StateManager.CreateTransaction())
            {
                // CreateEnumerableAsync returns an IAsyncEnumerable, which as of yet does not support the full set of LINQ extension methods.
                // See the discussion here ...
                // http://stackoverflow.com/questions/36877614/what-would-be-the-best-way-to-search-an-ireliabledictionary
                var items = (await cart.CreateEnumerableAsync(tx)).ToEnumerable().Select(x => x.Value);
                return items.ToList();
            }
        }
    }
}
