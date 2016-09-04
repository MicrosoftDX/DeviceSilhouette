# Sample App

This repository contains a sample app to demonstarte the [Home Ligning Scenario](lightsSampleScenario.md).
The sample app is implementted as a Windows 10 UWP.

The code for the sample app is under SampleApp/SampleApp.sln

The sample includes one device named *DemoAppLightBulb*.
The state of the device can be one of the two posible states values:

```

"values": {
        "status": "off"
      }
      
"values": {
        "status": "on"
      },

```



The sample app comprised from two UWP apps:

1. HomeLightsMobile - Simulate a mobile device to control home light remotly. It enables to get the light state and swith the light state.
2. LightDeviceApp - Simulate the home





