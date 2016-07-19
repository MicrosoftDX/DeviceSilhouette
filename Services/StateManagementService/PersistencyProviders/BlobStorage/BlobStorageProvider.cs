using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersistencyProviders.BlobStorage
{
    class BlobStorageProvider : IHistoryStorage
    {
        private string _storageConnection;
        private const string _storageContainer = "silhouette-messages";

        public BlobStorageProvider(string storageConnection)
        {
            _storageConnection = storageConnection;
        }

        /// <summary>
        /// Stores a single state message to blob storage
        /// </summary>
        /// <param name="stateMessage">the device state message to store</param>
        /// <returns></returns>
        public Task StoreStateMessage(DeviceState stateMessage)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Stores a collection of state messages to blob storage
        /// </summary>
        /// <param name="stateMessages">device state messages to store</param>
        /// <returns></returns>
        public Task StoreStateMessages(DeviceState[] stateMessages)
        {
            throw new NotImplementedException();
        }
    }
}
