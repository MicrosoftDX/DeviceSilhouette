using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting;
using DeviceRichState;
using PersistencyProviders.BlobStorage;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;

namespace StorageProviderService
{
    public interface IStorageProviderRemoting : IService
    {
        Task StoreStateMessageAsync(DeviceState stateMessage);

        Task StoreStateMessagesAsync(List<DeviceState> stateMessages);
    }

    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class StorageProviderService : StatelessService, IStorageProviderRemoting
    {
        BlobStorageProvider _storageProvider;

        public StorageProviderService(StatelessServiceContext context, string storageConnectionString)
            : base(context)
        {
            _storageProvider = new BlobStorageProvider(storageConnectionString);
        }

        public async Task StoreStateMessageAsync(DeviceState stateMessage)
        {
            await _storageProvider.StoreStateMessageAsync(stateMessage);
        }

        public async Task StoreStateMessagesAsync(List<DeviceState> stateMessages)
        {
            await _storageProvider.StoreStateMessagesAsync(stateMessages);
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[] { new ServiceInstanceListener(context =>
            this.CreateServiceRemotingListener(context)) };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {            
            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ServiceEventSource.Current.ServiceMessage(this, "Working-{0}", ++iterations);
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
