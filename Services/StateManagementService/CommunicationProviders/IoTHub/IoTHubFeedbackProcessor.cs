using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommunicationProviders.IoTHub
{
    public class IoTHubFeedbackProcessor : IFeedbackReceiver
    {
        private Func<FeedbackRecord, Task> _feedbackHandler;
        private readonly FeedbackReceiver<FeedbackBatch> _feedbackReceiver;        

        public IoTHubFeedbackProcessor(string iotHubConnectionString, Func<FeedbackRecord, Task> feedbackHandler)
        {
            _feedbackReceiver = ServiceClient.CreateFromConnectionString(iotHubConnectionString).GetFeedbackReceiver();
            _feedbackHandler = feedbackHandler;
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
