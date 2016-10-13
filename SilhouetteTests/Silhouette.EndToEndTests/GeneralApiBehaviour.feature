# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
Feature: GeneralApiBehaviour
	Verify general behaviour of the API, such as status codes on failure etc

Scenario: For a device with no messages
	Then the messages API contains no messages for device invalidDeviceId
	And the message API returns NotFound for device invalidDeviceId for message version 123
	Then the commands API contains no commands for device invalidDeviceId
	And the command API returns NotFound for device invalidDeviceId for command id invalidId
	
