using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonUtils;

namespace CommunicationProviders.IoTHub
{
    public class IoTHubFeedbackProcessor : IFeedbackReceiver
    {
        private Func<FeedbackRecord, Task> _feedbackHandler;
        private readonly FeedbackReceiver<FeedbackBatch> _feedbackReceiver;        

        public IoTHubFeedbackProcessor(string iotHubConnectionString, Func<FeedbackRecord, Task> feedbackHandler)
        {
            try
            {
                _feedbackReceiver = ServiceClient.CreateFromConnectionString(iotHubConnectionString).GetFeedbackReceiver();
                _feedbackHandler = feedbackHandler;
            }
            // Write the exception to ETW but still through the exception to prevent deployment of an unhealthy service
            catch (Exception ex)
            {
                SilhouetteEventSource.Current.LogException(ex.ToString());
                throw ex;

            }
        }
       

        public async Task ReceviceFeedbackAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                FeedbackBatch feedbackBatch = await _feedbackReceiver.ReceiveAsync();
                if (feedbackBatch != null)
                {                    
                    var processingTasks = feedbackBatch.Records.Select(_feedbackHandler);
                    await Task.WhenAll(processingTasks);
                    await _feedbackReceiver.CompleteAsync(feedbackBatch);
                }                
            }          
        }
    }
}
