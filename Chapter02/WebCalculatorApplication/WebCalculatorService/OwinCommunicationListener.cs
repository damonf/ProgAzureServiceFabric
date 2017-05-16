using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace WebCalculatorService
{
    public class OwinCommunicationListener : ICommunicationListener
    {
        private readonly IOwinAppBuilder _startup;
        private readonly ServiceContext _serviceContext;
        private IDisposable _serverHandle;
        private string _listeningAddress;
        private string _appRoot;

        public OwinCommunicationListener(IOwinAppBuilder startup, ServiceContext serviceContext, string appRoot)
        {
            _startup = startup;
            _serviceContext = serviceContext;
            _appRoot = appRoot;
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            var serviceEndpoint =
                _serviceContext.CodePackageActivationContext.GetEndpoint("ServiceEndpoint");

            var port = serviceEndpoint.Port;
            _listeningAddress = $"http://+:{port}/{_appRoot}/";

            _serverHandle = WebApp.Start(_listeningAddress, appBuilder => _startup.Configuration(appBuilder));

            var resultAddress = _listeningAddress.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);

            ServiceEventSource.Current.Message("Listening on {0}", resultAddress);
            return Task.FromResult(resultAddress);
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            StopWebServer();
            return Task.FromResult(true);
        }

        public void Abort()
        {
            StopWebServer();
        }
        private void StopWebServer()
        {
            if (_serverHandle != null)
            {
                try
                {
                    _serverHandle.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // no-op
                }
            }
        }
    }
}
