using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace Silhouette.EndToEndTests.Steps
{
    public class StepsBase
    {
        private List<string> _timeoutMessages = new List<string>();

        protected void Log(string message)
        {
            Console.WriteLine($"\t{DateTime.Now:yyyy-MM-dd-HH-mm-ss} {message}");
        }


        [AfterScenario]
        public void FlagTimeouts()
        {
            if (_timeoutMessages.Count > 0)
            {
                var timeoutMessageString = string.Join("\r\n", _timeoutMessages);

                Assert.Inconclusive("One or more steps exceeded the target time for responses:\r\n" + timeoutMessageString);
            }
        }


        /// <summary>
        /// Add a timeout to flag at the end of the scenario without halting the test
        /// </summary>
        /// <param name="message"></param>
        /// <param name="memberName"></param>
        protected void AddTimeoutMessage(string message)
        {
            var stepText = ScenarioContext.Current.StepContext.StepInfo.Text;
            _timeoutMessages.Add($"Step '{stepText}': {message}");
        }


        protected async Task<TResult> RetryWithTimeoutAsync<TResult>(
                            int timeoutInSeconds,
                            Func<CancellationToken, Task<TResult>> action,
                            double waitIntervalInSeconds = 0.25
            )
        {
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutInSeconds)).Token;

            TResult result;
            while ((result = await action(cancellationToken)) == null)
            {
                await Task.Delay(TimeSpan.FromSeconds(waitIntervalInSeconds), cancellationToken);
            }

            return result;
        }


        // The slightly odd style of test method (with RunAndBlock) is because SpecFlow currently doesn't support async tests _yet_ :-( 
        // See https://github.com/techtalk/SpecFlow/issues/542
        // Update: in PR https://github.com/techtalk/SpecFlow/pull/647
        public void RunAndBlock(Func<Task> asyncAction)
        {
            try
            {
                asyncAction().Wait();
            }
            catch (AggregateException ae) when (ae.InnerException is AssertFailedException)
            {
                var afe = (AssertFailedException)ae.InnerException;
                throw new AssertFailedException("Wrapped: " + afe.Message, afe); // wrap the exception so that the inner exception preserves the stack trace
            }
        }
        public TResult RunAndBlock<TResult>(Func<Task<TResult>> asyncFunc)
        {
            try
            {
                return asyncFunc().Result;
            }
            catch (AggregateException ae) when (ae.InnerException is AssertFailedException)
            {
                var afe = (AssertFailedException)ae.InnerException;
                throw new AssertFailedException("Wrapped: " + afe.Message, afe); // wrap the exception so that the inner exception preserves the stack trace
            }
        }


        public TResult RunAndBlockWithTargetTime<TResult>(int targetTime,
            string actionDescription,
            Func<Task<TResult>> asyncAction)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                TResult result = asyncAction().Result;
                var elapsedTime = stopwatch.Elapsed;

                var targetTimeSpan = TimeSpan.FromSeconds(targetTime);
                if (elapsedTime > targetTimeSpan)
                {
                    var stepText = ScenarioContext.Current.StepContext.StepInfo.Text;
                    AddTimeoutMessage($"action '{actionDescription}' took {elapsedTime} with a target of {targetTimeSpan}");
                }
                return result;
            }
            catch (AggregateException ae) when (ae.InnerException is AssertFailedException)
            {
                var afe = (AssertFailedException)ae.InnerException;
                throw new AssertFailedException("Wrapped: " + afe.Message, afe); // wrap the exception so that the inner exception preserves the stack trace
            }
        }
    }
}
