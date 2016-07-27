Feature: End-to-end message flows
	Ensure the basic cloud-to-device and device-to-cloud functionality is working

# TODO - Some of the tests below have a wait to allow time for messages to be processes
#        we should look at other ways to handle this rather than just waiting for a fixed time!	

Scenario: State reports from device are received in API
	Given a registered and connected device with id e2eDevice1
	When the device reports its state
	And we wait for 2 seconds
	Then the reported state Api should contain the reported state for device e2eDevice1
	And the messages API should contain the reported state message for device e2eDevice1

Scenario: State requests via the API are receieved by a connected device and the message status is accessible in the API
	Given a registered and connected device with id e2eDevice2
	When a state request is sent through the Api for device e2eDevice2
	Then the Api status code is created
	And the Api response includes a Location header with the command Url
	When we wait for 2 seconds
	Then the device receieves and accepts the state request
	And the messages Api contains the command request message for the state
	And the command Api contains the command for the state with no response
	And the messages Api contains the command response Ack for the state
	And the command Api contains the command for the state with an Ack response
	