Feature: End-to-end message flows
	Ensure the basic cloud-to-device and device-to-cloud functionality is working

# TODO - Some of the tests below have a wait to allow time for messages to be processes
#        we should look at other ways to handle this rather than just waiting for a fixed time!	

# D2C:SetState
Scenario: State reports from device are received in API
	Given a registered and connected device with id e2eDevice1
	
	When the device reports its state
	Then the messages API contains the reported state message for device e2eDevice1 within 2 seconds but wait up to 60 seconds to verify
	And the reported state API contains the reported state for device e2eDevice1

# Before running this test, make sure Silhouette holds state for the device (execute the previuse test)
# D2C:GetState
Scenario: Device requests for a state update
	Given a registered and connected device with id e2eDevice1

	When the device requests its state
	And we set up a trigger for the device receiving messages
	And  the device receieves the state update within 5 seconds but wait up to 60 seconds to verify
	Then the commands API contains the command for the latest state for device e2eDevice1
	# TODO: Add handling feedback messages
	
#C2D:SetState
Scenario: State requests via the API are receieved by a connected device and the message status is accessible in the API
	Given a registered and connected device with id e2eDevice2
	
	When we set up a trigger for the device receiving messages
	And a state request is sent through the Api for device e2eDevice2 with timeoutMs 10000
	Then the API status code is created
	And the API response includes a Location header with the command Url
	Then the device receieves the state request within 5 seconds but wait up to 60 seconds to verify
	# next step stores the correlationId
	Then the messages API contains the command request message for the state for device e2eDevice2 within 5 seconds but wait up to 60 seconds to verify
	And the device message matches the messages API correlationId
	Then the command API contains the command for the state request for device e2eDevice2
	And the command received from the API has no response
	And the commands API contains the command for the state request for device e2eDevice2 
	
	When the device accepts the state request
	Then the messages API contains the command response ACK for the state request for device e2eDevice2 within 5 seconds but wait up to 60 seconds to verify
	And the command API contains the command for the state request for device e2eDevice2
	And the command received from the API has an ACK response

#C2D:GetState
Scenario: Get State command sent via the API and received by a connected device
	Given a registered and connected device with id e2eDevice2

	When we set up a trigger for the device receiving messages
	And a get state command is sent thorugh the Api for device e2eDevice2 with timeoutMs 10000
	Then the API status code is created
	And the API response includes a Location header with the command Url
	Then the device receieves the get state command within 5 seconds but wait up to 60 seconds to verify
	# TODO: Add handling feedback messages

	When the device reports its state
	Then the messages API contains the reported state message for device e2eDevice2 within 2 seconds but wait up to 60 seconds to verify
	And the reported state API contains the reported state for device e2eDevice2