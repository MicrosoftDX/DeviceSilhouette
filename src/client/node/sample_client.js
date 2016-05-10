var silhouetteClient = require('./silhouette-client');

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
  console.log("in C2D_updateState");
  my_state = state;
}

/*
** This function gets called when the cloud service wants to know the latest state.
** The device should return its latest state.
*/

function C2D_getState()
{
  console.log("in C2D_getState");
  silhouette.updateState(my_state);
}

/*
** Create the Silhouette client
*/

var silhouette = silhouetteClient.create('iothub', {
  connectionString: 'HostName=ciscohackhub.azure-devices.net;DeviceId=silhouette1;SharedAccessKey=XDtfQq9EB6uqVUfSWOwgktQe3L9O3DppNpOgv7s0OW4='
});

silhouette.on('C2D_updateState', C2D_updateState);
silhouette.on('C2D_getState', C2D_getState);

/*
** This could work in whatever way you want. We will just set a timer.
*/

setInterval(doWork, 10*1000);

function doWork()
{
  // console.dir(silhouette);
  // Send our new state to the cloud service
  silhouette.updateState(my_state);
  // TODO: also get the state from the cloud service?
  // check_state = silhouette.getState();
  // TODO: we can use the native client to do other stuff
  // silhouette.client.on('someEvent', doSomething)
}
