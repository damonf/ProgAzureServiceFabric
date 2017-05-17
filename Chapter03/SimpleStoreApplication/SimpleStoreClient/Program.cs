using System;
using System.Fabric;
using Common;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;

namespace SimpleStoreClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceName = new Uri("fabric:/SimpleStoreApplication/ShoppingCartService");
            var serviceResolver = new ServicePartitionResolver(() => new FabricClient());

            for (var i = 0; i < 10; i++)
            {
                var shoppingClient = new Client(
                    new WcfCommunicationClientFactory<IShoppingCartService>(servicePartitionResolver: serviceResolver,
                        clientBinding: WcfUtility.CreateTcpClientBinding()), serviceName, i);

                shoppingClient.AddItem(new ShoppingCartItem
                {
                    ProductName = $"XBOX ONE ({i})",
                    UnitPrice = 329.0,
                    Amount = 2
                }).Wait();

                shoppingClient.AddItem(new ShoppingCartItem
                {
                    ProductName = $"Halo 5 ({i})",
                    UnitPrice = 59.99,
                    Amount = 1
                }).Wait();

                PrintPartition(shoppingClient);

                var list = shoppingClient.GetItems().Result;

                foreach (var item in list)
                {
                    Console.WriteLine($"{item.ProductName}: {item.UnitPrice:C2} X {item.Amount} = {item.LineTotal:C2}");
                }
            }

            Console.ReadKey();
        }

        private static void PrintPartition(Client client)
        {
            if (client.TryGetLastResolvedServicePartition(out ResolvedServicePartition partition))
            {
                Console.WriteLine("Partition ID: " + partition.Info.Id);
            }
        }
    }
}
