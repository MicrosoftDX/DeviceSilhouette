# Architecture

The Device Silhouette solution is comprised from the following components:

1. Communication provider - this is the messaging broker that is being used to send messages and receive messages to/from the device. In this current project there is a provider implemented for Azure IoT Hub. The provider is implemented as an interface and can be implemented as a provider for additional messaging broker technologies like Kafka, Event Hub, RabbitMQ etc.
2. Service Fabric Cluster - this is the service side, runs in the cloud, comprised from a few microservices.
3. Long term persiatancy storage - the storage where messages are being persistage for long term and analytics. Implemented as an iterface so it can be extended to suppot any desired storage. Currently implemented in this project for Azure blob storage.

Service Fabric microservices:

1. Device State Managment Service REST API
2. State Processor Service
2. Device State Repository
4. Feedback Service







