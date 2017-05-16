using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using CalculatorService;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;

namespace CalculatorClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Uri ServiceName = new Uri("fabric:/CalculatorApplication/CalculatorService");
            ServicePartitionResolver serviceResolver = new ServicePartitionResolver(() =>
                new FabricClient());
            Client calcClient = new Client(
                new WcfCommunicationClientFactory<ICalculatorService>(servicePartitionResolver: serviceResolver,
                    clientBinding: WcfUtility.CreateTcpClientBinding()), ServiceName);
            Console.WriteLine(calcClient.Add(3, 5).Result);
            Console.ReadKey();
        }
    }
}
