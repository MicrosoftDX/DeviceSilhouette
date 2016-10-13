// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetClient
{
    
    // TODO move this into a class library :-)
    // TODO - handle receiving messages
    // TODO - online/offline
    public class DeviceSimulator
    {
        private readonly string _connectionString;

        private DeviceClient _deviceClient = null;
        private CancellationTokenSource _receiveLoopCancellationTokenSource = null;

        public string DeviceId { get; private set; }

        public event EventHandler<ReceiveMessageEventArgs> ReceivedMessage;

        public DeviceSimulator(string connectionString, string deviceId)
        {
            _connectionString = connectionString;
            DeviceId = deviceId;
        }
        

        /// <summary>
        /// Initialize. Ensure registered in IoT Hub
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            var registryManager = RegistryManager.CreateFromConnectionString(_connectionString);
            var device = await registryManager.GetDeviceAsync(DeviceId);
            if (device == null)
            {
                Console.WriteLine($"Add device: {DeviceId}");
                device = await registryManager.AddDeviceAsync(new Device(DeviceId));
            }

            string hostname = GetHostNameFromConnectionString(_connectionString);
            _deviceClient = DeviceClient.Create(
                hostname, 
                new DeviceAuthenticationWithRegistrySymmetricKey(DeviceId, device.Authentication.SymmetricKey.PrimaryKey)
                );
        }


        public async Task SendStateMessageAsync(object state)
        {
            var stateString = JsonConvert.SerializeObject(state);
            await SendStateMessageAsync(stateString);
        }
        public async Task SendStateMessageAsync(string state)
        {
            var stateBuf = Encoding.UTF8.GetBytes(state);

            var message = new Microsoft.Azure.Devices.Client.Message(stateBuf)
            {
                Properties =
                {
                    { "MessageType", "Report" },
                    { "MessageSubType", "State" },
                },
                //CorrelationId = "asdas" // TODO - use this when responding to messages!
            };
            await _deviceClient.SendEventAsync(message);
        }

        public async Task RequestStateMessageAsync()
        {
            var message = new Microsoft.Azure.Devices.Client.Message()
            {
                Properties =
                {
                    { "MessageType", "Inquiry" },
                    { "MessageSubType", "GetState" },
                }               
            };
            await _deviceClient.SendEventAsync(message);
        }

        public void StartReceiveMessageLoop()
        {
            // TODO - check if already got cancellation token source etc

            _receiveLoopCancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => ReceiveMessages(_receiveLoopCancellationTokenSource.Token));
        }
        // TODO - StopReceiveMessageLoopAsync

        public async Task CompleteReceivedMessageAsync(DeviceMessage message)
        {
            await _deviceClient.CompleteAsync(message.Message);
        }        

        public async Task RejectReceivedMessageAsync(DeviceMessage message)
        {
            await _deviceClient.RejectAsync(message.Message);
        }
        public async Task AbandonReceivedMessageAsync(DeviceMessage message)
        {
            await _deviceClient.AbandonAsync(message.Message);
        }


        private async Task ReceiveMessages(CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                var message = await _deviceClient.ReceiveAsync();
                if (message != null)
                {
                    var deviceMessage = new DeviceMessage(message);
                    var args = new ReceiveMessageEventArgs(deviceMessage);
                    ReceivedMessage?.Invoke(this, args);
                    // complete, abandon, reject: https://msdn.microsoft.com/en-us/library/azure/mt590786.aspx
                    switch (args.Action)
                    {
                        case ReceiveMessageAction.None:
                            // no action :-)
                            break;
                        case ReceiveMessageAction.Complete:
                            await _deviceClient.CompleteAsync(message);
                            break;
                        case ReceiveMessageAction.Abandon:
                            await _deviceClient.AbandonAsync(message);
                            break;
                        case ReceiveMessageAction.Reject:
                            await _deviceClient.RejectAsync(message);
                            break;
                        default:
                            throw new NotImplementedException($"Unhandled ReceiveMessageAction: '{args.Action}'");
                    }
                }
            }            
        }

        private static string GetHostNameFromConnectionString(string connectionString)
        {
            var builder = Microsoft.Azure.Devices.IotHubConnectionStringBuilder.Create(connectionString);
            return builder.HostName;
        }
    }
}

