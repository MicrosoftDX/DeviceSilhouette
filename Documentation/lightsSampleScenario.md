# Lighting scenarios

## Scenario 1
In this scenario the light, switch and light sensor all communication with IOT Hub.

```

    +-----------------+             +------------------+
    |                 |             |                  |
    |   Phone App     +------------->   Custom Service |
    |                 |             |                  |
    +-----------------+             +------------------+
                                             |
                                             |
                                    +--------v---------+
                                    |                  |
                                    |   Silhouette     |
                                    |                  |
                                    +------------------+
                                             |
                                             |
                                    +--------v---------+
                                    |                  |
                                    |    IoT Hub       |
                                    |                  |
                                    +------------------+
                                             ^
                                             |
                          +-----------------------------------+
                          |                  |                |
                          |                  |                |
                      +--------+        +----+---+       +----+----+
                      | Switch +        | Light  |       | Sensor  |
                      +--------+        +--------+       +---------+

```

### Components
* Light bulb - the physical light bulb, can be On or Off.
* Switch - to turn the light bulb On/Off.
* Light sensor - detects the light levels. Used to turn lights off during the day to save electricity. 
* Phone app - Get the light bulb status: On/Off, When was turned On/Off, by who was the light turned On/Off (Phone app, light sensor, switch). Send command to turn On/Off the light bulb.

### Event sequence

0. Starting state: Light off, Sensor "Dark", Silhouette state matches devices
0. Get up, turn on switch. Message processed by Service and command issued to turn light on
0. Light receives command to turn on. Current state: Light on, Sensor "Dark"
0. Later, light sensor reports "Light" 


### Outcome
Naive outcome: Service sees that it is light and turns the light off

Desired outcome: light is only turned off if the user hasn't just turned it on.



## Scenario 2
In this scenario the switch communicates directly with the light. The light and light sensor communicate with IOT Hub.

```

+-----------------+             +------------------+
|                 |             |                  |
|   Phone App     +------------->   Custom Ser^ice |
|                 |             |                  |
+-----------------+             +--------+---------+
                                         |
                                         |
                                +--------v---------+
                                |                  |
                                |   Silhouette     |
                                |                  |
                                +--------+---------+
                                         |
                                         |
                                +--------v---------+
                                |                  |
                                |    IoT Hub       |
                                |                  |
                                +--------+---------+
                                         ^
                                         |
                                         +----------------+
                                         |                |
                                         |                |
                  +--------+        +----+---+       +----+----+
                  | Switch +--------> Light  |       | Sensor  |
                  +--------+        +--------+       +---------+

```


### Components
* Light bulb - the physical light bulb, can be On or Off.
* Switch - to turn the light bulb On/Off, directly connected to the Light to support offline operation.
* Light sensor - detects the light levels. Used to turn lights off during the day to save electricity. 
* Phone app - Get the light bulb status: On/Off, When was turned On/Off, by who was the light turned On/Off (Phone app, light sensor, switch). Send command to turn On/Off the light bulb.


### Event sequence

0. Starting state: Light off, Sensor "Dark", Silhouette state matches devices
0. Network connection goes down (i.e. devices cannot communicate to IoT Hub)
0. Get up, turn on switch. Light on, Sensor "Dark" (Silhouette state shows light off)
0. Sensor reports "Light"<br/>
    At this point local state is Light: on, Sensor "Light"<br/>
    Silhouette state is Light: off, Sensor "Dark"<br/>
0. Network comes back up.


### Outcome
The naive outcome in this case will likely depend on the order in which the messages are received by IoT Hub. 
If the light on message is first then the service will see the light come on and then the sensor reporting that it is light, so will likely turn the light off to save electricity.
On the other hand, if the service sees that it is light and then the light is turned on it will likely determine that you opted to turn the light on even though it is light and leave the light on.


The desired outcome is likely specific to the actual application but we should support the decision making.

Desired outcome (1): When making the decision about whether to turn the light off, the application will likely want some history of the states. I.e. not just what the light state is, but when it was reported. For example, the light may be expected to report state every 5 minutes. If the last report is over that interval then the application may want to wait until a newer reported state is available in the API before making a decision.
