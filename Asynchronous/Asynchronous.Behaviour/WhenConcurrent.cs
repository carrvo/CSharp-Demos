using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CSharp.Demos.Asynchronous.Behaviour
{
    public sealed class WhenConcurrent
    {
        private TimeSpan Wait { get; } = TimeSpan.FromSeconds(10);
        private TimeSpan WaitForAsync { get; } = TimeSpan.FromSeconds(15);

        private enum EKeyExecutionStep
        {
            BeforeCall,
            AfterCall,
            AfterAwait,
            AfterWaitingForAsync,
            /// <summary>
            /// before SLEEP (blocking)
            /// </summary>
            BeforeBlocking,
            /// <summary>
            /// before DELAY (non-blocking)
            /// </summary>
            BeforeNonBlocking,
            Finish,
        }
        private BehaviourHelper<EKeyExecutionStep> Helper
        {
            get;
        }

        private ITestOutputHelper Output
        {
            get;
        }

        public WhenConcurrent(ITestOutputHelper output)
        {
            Output = output;
            Helper = new BehaviourHelper<EKeyExecutionStep>();
        }

        [Fact]
        public async void OrderOfAwait()
        {
            Helper.NextOrder = EKeyExecutionStep.BeforeCall;
            var t = ExampleAsync();
            Helper.NextOrder = EKeyExecutionStep.AfterCall;
            await t;
            Helper.NextOrder = EKeyExecutionStep.AfterAwait;

            Helper.PrintOutput(Output);
            Helper.AssertNotNextOrder(EKeyExecutionStep.BeforeCall, EKeyExecutionStep.AfterCall);

            Helper.AssertOrder(EKeyExecutionStep.BeforeCall,
                EKeyExecutionStep.BeforeBlocking,
                EKeyExecutionStep.BeforeNonBlocking,
                EKeyExecutionStep.AfterCall,
                EKeyExecutionStep.Finish,
                EKeyExecutionStep.AfterAwait);
        }

        [Fact]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async void OrderWithoutAwait()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Helper.NextOrder = EKeyExecutionStep.BeforeCall;
            var t = ExampleAsync();
            Helper.NextOrder = EKeyExecutionStep.AfterCall;
            //await t;
            Thread.Sleep(WaitForAsync);
            Helper.NextOrder = EKeyExecutionStep.AfterWaitingForAsync;

            Helper.PrintOutput(Output);
            Helper.AssertNotNextOrder(EKeyExecutionStep.BeforeCall, EKeyExecutionStep.AfterCall);

            Helper.AssertOrder(EKeyExecutionStep.BeforeCall,
                EKeyExecutionStep.BeforeBlocking,
                EKeyExecutionStep.BeforeNonBlocking,
                EKeyExecutionStep.AfterCall,
                EKeyExecutionStep.Finish,
                EKeyExecutionStep.AfterWaitingForAsync);
        }

        [Fact]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async void OrderWithoutAwaitOrSleep()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Helper.NextOrder = EKeyExecutionStep.BeforeCall;
            var t = ExampleAsync();
            Helper.NextOrder = EKeyExecutionStep.AfterCall;
            //await t;
            Helper.NextOrder = EKeyExecutionStep.AfterWaitingForAsync;
            Helper.EndTest();
            Thread.Sleep(WaitForAsync);

            Helper.PrintOutput(Output);
            Helper.AssertNotNextOrder(EKeyExecutionStep.BeforeCall, EKeyExecutionStep.AfterCall);
            Helper.AssertDoesNotContain(EKeyExecutionStep.Finish);

            Helper.AssertOrder(EKeyExecutionStep.BeforeCall,
                EKeyExecutionStep.BeforeBlocking,
                EKeyExecutionStep.BeforeNonBlocking,
                EKeyExecutionStep.AfterCall,
                EKeyExecutionStep.AfterWaitingForAsync);
        }

        private async Task ExampleAsync()
        {
            Helper.NextOrder = EKeyExecutionStep.BeforeBlocking;
            Thread.Sleep(Wait);
            Helper.NextOrder = EKeyExecutionStep.BeforeNonBlocking;
            await Task.Delay(Wait);
            Helper.NextOrder = EKeyExecutionStep.Finish;
        }
    }
}
