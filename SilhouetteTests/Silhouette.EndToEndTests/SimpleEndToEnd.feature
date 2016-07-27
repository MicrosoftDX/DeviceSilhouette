Feature: SimpleEndToEnd
	Ensure the basic cloud-to-device and device-to-cloud functionality is working

@mytag
Scenario: Device to cloud messages
	Given a registered and connected device with id e2eDevice1
	When the device reports its state
	Then the reported state API should contain the reported state 
	And the messages API should contain the reported state message
