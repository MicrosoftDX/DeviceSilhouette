var silhouette = require('./silhouette-client');

var my_state = {
  'temperature': 42, // this is a read-only value
  'heaterOn': false // this is a read/write value
};

/*
** This function gets called when the cloud service sends an update to the state.
** The device should update its state accordingly.
*/

function C2D_updateState(state)
{
  // TODO: should I check what is inside the state? 
  // - What if heaterOn has changed? How should I detect to take action?
  // - What if the cloud service wants to set the temperature? It makes no sense e.g. for a sensor.
  // - Do we get all the properties on update or just the ones changed? 
  my_state = state;
}

/*
** This function gets called when the cloud service wants to know the latest state.
** The device should return its latest state.
*/

function C2D_getState()
{
  client.updateState(my_state);
}

/*
** Create the Silhouette client
*/

var client = silhouette.create('iothub', {
  connectionString: 'HostName=ciscohackhub.azure-devices.net;DeviceId=silhouette1;SharedAccessKey=XDtfQq9EB6uqVUfSWOwgktQe3L9O3DppNpOgv7s0OW4='
});

client.on('C2D_updateState', C2D_updateState);
client.on('C2D_getState', C2D_getState);

/*
** This could work in whatever way you want. We will just set a timer.
*/

setInterval(doWork, 10*1000);

function doWork()
{
  console.dir(client);
  // Send our new state to the cloud service
  client.updateState(my_state);
  // TODO: also get the state from the cloud service?
  // check_state = client.getState();
}
