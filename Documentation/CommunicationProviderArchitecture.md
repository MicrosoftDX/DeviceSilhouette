
                                         +-----------------------+
                                         |                       |
                            +------------+ Device(s)             <---------------------+
                            |  +--------->                       +-----------------+   |
                            |  |         +-----------------------+                 |   |
                            |  |                                                   |   |
                            |  |                                                   |   |
                            |  |                                                   |   |
                            |  |                                                   |   |
                            |  |                                                   |   |
                            |  |                                                   |   |
                            |  |                                                   |   |
        +-------------------v-------------------+                 +----------------v------------------------------------------------------------------+
        |                                       |                 |                                                                                   |
        |                                       |                 | DeviceEndpointForKafka                                                            |
        |  IoTHub                               |                 |                                                                                   |
        |                                       |                 |                  APIApp                                                           |
        |                                       |                 |                                                                                   |
        |                                       |                 +--------------------------------------^--------------------------------------------+
        |                                       |                           |                            |                                 |
        |                                       |                           |                            |                                 |
        |                                       |                 +---------v------------+               |                                 |
        |                                       |                 |                      |               |                                 |
        |                                       |                 | Kafka Server(s)      |           +-----------------------+   +---------v----------+       +--------------------+
        |                                       |                 |                      |           |                       |   |                    |       |                    |
        |                                       |                 |                      |           | Storage for Cloud to  |   | DeviceIDRepository |       | DeviceRegistration |
        |                                       |                 |                      |           | Device Messages       |   |                    <-------+ Tool               |
        |                                       |                 |                      |           |                       |   |                    |       |     ConsoleApp     |
        |                                       |                 |                      |           |                       |   |    Redis           |       +--------------------+
        |                                       |                 |                      |           |          TableStorage |   |    TableStorage    |
        |                                       |                 |                      |           |                       |   |                    |
        |                                       |                 |          VMs         |           +------^----------------+   +--------------------+
        |                                       |                 |                      |                  |
        +----------------------^----------------+                 +----------------------+                  |
                            |  |                                             |                              |
                            |  |                                             |                              |
                            |  |                                             |                              |
        +-------------------v-------------------+                 +----------v-----------------------------------------------+
        | CommunicationProviderForIoTHub        |                 | CommunicationProviderForKafka                            |
        |                                       |                 |                                                          |
        +------------------^--------------------+                 +-----------------------^----------------------------------+
                           |                                                              |
                           |                                                              |
                           |                                                              |
        +--------------------------------------------------------------------------------------------------------------------+
        |                                                                                                                    |
        |  Silhouette State Management Services                                                                              |
        |                                                                                                                    |
        |                                                                                                                    |
        |                                                                                                                    |
        |                                                                                                                    |
        +--------------------------------------------------------------------------------------------------------------------+


