# Architecture

## Main Components:

The Device Silhouette solution is comprised from the following components:

1. **Communication provider** - this is the messaging broker that is being used to send messages and receive messages to/from the device. In this current project there is a provider implemented for Azure IoT Hub. The provider is implemented as an interface and can be implemented as a provider for additional messaging broker technologies like Kafka, Event Hub, RabbitMQ etc.
2. **Service Fabric Cluster** - this is the service side, runs in the cloud, comprised from a few microservices.
3. **Long term persiatancy storage** - the storage where messages are being persistage for long term and analytics. Implemented as an iterface so it can be extended to suppot any desired storage. Currently implemented in this project for Azure blob storage.

## Service Fabric microservices:

1. Device State Managment Service REST API (Stateless)
2. State Processor Service (Stateless)
2. Device State Repository (stateful Actor based)
4. Feedback Service (Stateless)
5. Device Communication Provider (Stateless)


**Device State Managment Service REST API** is..

The **State Processor Service** main functionality is to receive state get/set requests and process those requests. All operations are Asynchronous. The State Processor Service receives requests from external applications through the Device State Management Services, it also receives requests from devices through the Device Communication Provider endpoints. Then it execute the requests by communicating directly with the State Repository Service. The communication with the device is done through Device Communication Provider.

The **Device State Repository** holds the SIlhouette itself. Its a statful Actor based service that saves the list of the last messages sent to/from the device. 

The **Feddback service** is...

The **Device Communication Provider** is an interface to enable the communication between the Cloud and the Device. It's purpose is to enable Silhouette customers to choose the device communication technology from the variety of existing market messaging technologies, such as IoT Hub, Event Hubs or Kafka. Since each of the technologies supports different communication protocols, it also means the customer is able to choose the communication protocol by choosing a messaging technology that supports the desired protocol.











