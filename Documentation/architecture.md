# Architecture



# TODO: ADD DIAGRAM HERE

## Main Components:

The Device Silhouette solution is comprised from the following components:

1. **Messaging broker** - being used to send messages and receive messages to/from the device. This project can support the usage of any messaging broker by implementing a Device Communication provider. The provider is implemented as an interface and can be implemented as a provider for additional messaging broker technologies like Kafka, Event Hub, RabbitMQ etc. In this current repro there is a provider implemented for Azure IoT Hub. 
2. **Service Fabric Cluster** - this is the service side, runs in the cloud, comprised from a few microservices, listed below.
3. **Long term persiatancy storage** - the storage where messages are being persistage for long term and analytics. Implemented as an iterface so it can be extended to suppot any desired storage. Currently implemented in this repro for Azure blob storage.

## Service Fabric microservices:

1. **Device State Managment Service REST API (Stateless)**

  Enables external applications to communicate with the Device State Repository. Using the Device State Management Service an external application can get the latest state or send command to a device to change its states.
  
2. **State Processor Service (Stateless)**

  Receive state get/set requests and process those requests. All operations are Asynchronous. The State Processor Service receives requests from external applications through the Device State Management Service REST API, it also receives requests from devices through the Device Communication Provider endpoints. Then it execute the requests by communicating directly with the State Repository Service. The communication with the device is done through Device Communication Provider.
  
2. **Device State Repository (stateful Actor based)**

  Holds the Silhouette itself. Its a statful Actor based service that saves the list of the last messages sent to/from the device. 

4. **Feedback Service (Stateless)**

  Responsible for check messages delivery status from IoT Hub, meaning for messages sent to the device it will look for aknoledgment/non-aknoledgment and update the Device State Repository with the latest message delivery status.
  
5. **Device Communication Provider (Stateless)**
  
  interface to enable the communication between the Cloud and the Device. It's purpose is to enable Silhouette customers to choose the device communication technology from the variety of existing market messaging technologies, such as IoT Hub, Event Hubs or Kafka. Since each of the technologies supports different communication protocols, it also means the customer is able to choose the communication protocol by choosing a messaging technology that supports the desired protocol.

6. **Storage Provider Service (Stateless)**

  Persists all the messages to along term persistancy storage. Implemented as an interface that can be extended to any desired storage. In this current repro there is a provider implemented for Azure Blob Storage. 











