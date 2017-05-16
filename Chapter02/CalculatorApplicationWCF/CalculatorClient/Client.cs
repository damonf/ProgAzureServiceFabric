﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalculatorService;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;

namespace CalculatorClient
{
    public class Client : ServicePartitionClient<WcfCommunicationClient<ICalculatorService>>,
        ICalculatorService
    {
        public Client(WcfCommunicationClientFactory<ICalculatorService> clientFactory,
            Uri serviceName)
            : base(clientFactory, serviceName)
        {
        }
        public Task<string> Add(int a, int b)
        {
            return this.InvokeWithRetryAsync(client => client.Channel.Add(a, b));
        }
        public Task<string> Subtract(int a, int b)
        {
            return this.InvokeWithRetryAsync(client => client.Channel.Subtract(a, b));
        }
    }
}
