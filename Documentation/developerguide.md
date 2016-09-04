# Developer Guide


## Set the development environment

Follow the instructions [here](devenvironment.md)

## Providers

This repository uses the "Providers" concept in order to enable developers to use the Device Silhouette with different existing technologies. For example, for messaging broker, there are a few posible technologies like IoTHub, Kafka etc.
Provider is an interface that a developer can implement for his desired technology.

### Existing providers and implementations in this repro:

#### Communication Providers

The communication providers is a group of interfaces that enable the usage of a specific messaging broker.
It includes the following interfaces:

```
    public interface IMessageReceiver
    {
        // read messages from the communication provider endpoint
        Task RunAsync(CancellationToken cancellationToken);
    }
    public interface IMessageSender
    {
        /* send messages to the communication provider endpoint
         * DeviceId - device it to set message to
         * MessageType - State:Set or State:Get
         * Meesage - message json string         
        */
        Task SendCloudToDeviceAsync(DeviceMessage silhouetteMessage);
    }

    public interface IFeedbackReceiver
    {
        // process feedbacks from C2D messages
        Task ReceviceFeedbackAsync(CancellationToken cancellationToken);
    }
```

This repository includes implementation for IoTHub.

#### Persistancy Providers




## Service
## Clients
## Test 
## Sample app

