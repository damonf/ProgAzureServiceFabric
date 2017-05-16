using System;
using System.Threading;
using CalculatorService;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace CalculatorClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var calculatorClient = ServiceProxy.Create<ICalculatorService>
                (new Uri("fabric:/CalculatorApplication/CalculatorService"));

            while (true)
            {
                var result = calculatorClient.Add(1, 2).Result;
                Console.WriteLine(result);
                Thread.Sleep(3000);
            }
        }
    }
}
