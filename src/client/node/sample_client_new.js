var silhouetteClient = require('./silhouette-client-new');

var my_state = {
  Xaxis: 0,
  Yaxis: 0,
  Zaxis: 0
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
  console.log("in C2D_updateState; message:");
  console.dir(state)
  my_state = state.data.State;
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
  connectionString: 'HostName=SilhouetteHub.azure-devices.net;DeviceId=silhouette1;SharedAccessKey=rkGFp9PKEr7UjeKn/MFG2dpDpNajopSg0h6FhP0jFHo='
  //connectionString: 'HostName=SilhouetteHub.azure-devices.net;DeviceId=silhouette1;SharedAccessKeyName=device;SharedAccessKey=5l0nsPi3d8ggCdEeYTQi5YkWWuYKsUxSEPEpJMBslqA='
});

silhouette.on('C2D_updateState', C2D_updateState);
silhouette.on('C2D_getState', C2D_getState);
//silhouette.on('error', error_handle);

/*
** This could work in whatever way you want. We will just set a timer.
*/

setInterval(doWork, 10*1000);

function doWork()
{
  // console.dir(silhouette);
  // Send our new state to the cloud service
  console.log('sending updated state:');
  console.dir(my_state);
  silhouette.updateState(my_state);
  ++my_state.Xaxis;
  // TODO: also get the state from the cloud service?
  // check_state = silhouette.getState();
  // TODO: we can use the native client to do other stuff
  // silhouette.client.on('someEvent', doSomething)
}
