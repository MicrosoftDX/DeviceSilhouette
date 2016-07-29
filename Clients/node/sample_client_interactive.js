var silhouetteClient = require('./silhouette-client');
var blessed = require('blessed');

var my_state = {
  lightOn: false,
  color: 'white'
};

var metadata = { 
	origin : "sensor"
};

var colors = [ 'white', 'red', 'green', 'blue', 'yellow', 'purple', 'azure', 'mauve', 'beige'];

// Silhouette callbacks

function C2D_updateState(state)
{
  box_log.log("received CommandRequest:SetState");
  my_state = state;
  // Update UI
  box_state.content = JSON.stringify(my_state);
  screen.render();
}
function C2D_latestState(state)
{
  box_log.log("received CommandRequest:LatestState");
  my_state = state;
  // Update UI
  box_state.content = JSON.stringify(my_state);
  screen.render();
}

function C2D_getState()
{
  box_log.log("received CommandRequest:ReportState\nResponding with updated state");
  silhouette.updateState(my_state);
}

// Initialize Silhouette

var deviceID = "device42";
var connectionString = process.env.Silhouette_DeviceIotHubConnectionString

var silhouette = silhouetteClient.create('iothub', {
  connectionString
});

silhouette.on('C2D_updateState', C2D_updateState);
silhouette.on('C2D_latestState', C2D_latestState);
silhouette.on('C2D_getState', C2D_getState);
silhouette.on('error', function() {
  box_log.log('error');
});

// Build interface

var screen = blessed.screen({
  smartCSR: true
});

var box_state = blessed.box({
  top: 0,
  left: 0,
  width: '50%',
  height: '50%',
  content: JSON.stringify(my_state),
  tags: true,
  border: {
    type: 'line'
  }
});

var box_usage = blessed.box({
  top: 0,
  left: '50%',
  width: '50%',
  height: '50%',
  content: 's: switch light on/off\nc: cycle color\n\ng: get state\nu: update state\n\nt: start/stop tasks\n\nq: quit',
  tags: true,
  border: {
    type: 'line'
  }
});

var box_log = blessed.log({
  top: '50%',
  left: 0,
  width: '100%',
  height: '50%',
  tags: true,
  border: {
    type: 'line'
  }
});

screen.key(['escape', 'q', 'C-c'], function(ch, key) {
  return process.exit(0);
});

screen.key('s', function(ch, key) {
  my_state.lightOn = my_state.lightOn===true ? false : true;
  // Update UI
  box_state.content = JSON.stringify(my_state);
  screen.render();
});

screen.key('c', function(ch, key) {
  var i = colors.indexOf(my_state.color) + 1;
  my_state.color = i > colors.length ? colors[0] : colors[i];
  // Update UI
  box_state.content = JSON.stringify(my_state);
  screen.render();
});

screen.key('u', function(ch, key) {
  silhouette.updateState(metadata, my_state, deviceID);
  // Update UI
  box_log.log('sending Report:State');
  box_log.log(my_state);
});

screen.key('g', function(ch, key) {
  box_log.log('sending InquiryRequest:GetState');
  silhouette.getState();
});

screen.append(box_state);
screen.append(box_usage);
screen.append(box_log);
screen.render();

// Run tasks

//setInterval(doWork, 10*1000);

function doWork()
{
  silhouette.updateState(metadata, my_state, deviceID);
  // Update UI
  box_log.log('sending updated state:');
  box_log.log(my_state);
}

// TODO: we can use the native client to do other stuff
// silhouette.client.on('someEvent', doSomething)
