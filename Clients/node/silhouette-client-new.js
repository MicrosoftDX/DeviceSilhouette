/*
** Create a Silhouette client using the specified transport
** mqtt
** iothub
** ...
*/

function create(transport, config)
{
  switch (transport) {
    case 'mqtt':
      return require('./silhouette-client-mqtt').create(config);
    case 'iothub':
      return require('./silhouette-client-iothub-new').create(config);
    default:
      // Raise some kind of error
  }
}

module.exports.create = create;
