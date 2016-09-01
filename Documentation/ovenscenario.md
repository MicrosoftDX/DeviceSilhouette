# Oven Maintenance Scenario

This scenario deals with determining (in the cloud) errors in the functioning of an cloud-connected oven based on reported metrics.


##### Sequence of events
1. Oven sends state
 - On or Of
 - Set temperature
 - Actual temperature
 - Heeting element temperature
 - Set cooking program
 - Set oven function (grill, hot air, micro wave)
 - Set fan switch
 - Actual fan On or Off
 - Set rotation switch
 - Actual rotation motion (= speed)

2. Silhouette saves state
3. App engine verifies oven health based on reported state and thresholds
 - When an error is detected verify actual status with Oven
 - If actual state equals reported state send Alert

4. App engine analysis long term history for error patterns
 - When an error is detected send Alert

5. Engineer queries the last know state of oven or the oven functions over time, like
 - Show me the times the microwave was On and the speed of the rotation
 - Show me the set temperature and the actual temperature over time
 - Show me the last diagnostics log (or the logs from last month)

6. Engineer takes appropriate action

##### Requirements
- Cloud maintains a year of history
- App has uderstanding of proper functioning of the oven
  - If Microwave is On, plate should rotate
  - If Hot Air is On, fan should be spinning
  - Set temperature should be reached within a certain time
  - If a program is chosen the functions should be set accordingly
