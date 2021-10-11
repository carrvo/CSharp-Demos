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
    public sealed class WhenConcurrent_NonAsyncMethod
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
            BeforeInside,
            AfterInside,
        }
        private BehaviourHelper<EKeyExecutionStep> Helper
        {
            get;
        }

        private ITestOutputHelper Output
        {
            get;
        }

        public WhenConcurrent_NonAsyncMethod(ITestOutputHelper output)
        {
            Output = output;
            Helper = new BehaviourHelper<EKeyExecutionStep>();
        }

        [Fact]
        public async void OrderOfAwait()
        {
            Helper.NextOrder = EKeyExecutionStep.BeforeCall;
            var t = ExampleAsyncWithRun();
            Helper.NextOrder = EKeyExecutionStep.AfterCall;
            await t;
            Helper.NextOrder = EKeyExecutionStep.AfterAwait;

            Helper.PrintOutput(Output);
            Helper.AssertNotNextOrder(EKeyExecutionStep.BeforeCall, EKeyExecutionStep.AfterCall);

            Helper.AssertOrder(EKeyExecutionStep.BeforeCall,
                EKeyExecutionStep.BeforeBlocking,
                EKeyExecutionStep.BeforeNonBlocking,
                EKeyExecutionStep.BeforeInside,
                EKeyExecutionStep.AfterCall,
                EKeyExecutionStep.AfterInside,
                EKeyExecutionStep.Finish,
                EKeyExecutionStep.AfterAwait);
        }

        [Fact]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async void OrderWithoutAwait()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Helper.NextOrder = EKeyExecutionStep.BeforeCall;
            var t = ExampleAsyncWithRun();
            Helper.NextOrder = EKeyExecutionStep.AfterCall;
            //await t;
            Thread.Sleep(WaitForAsync);
            Helper.NextOrder = EKeyExecutionStep.AfterWaitingForAsync;

            Helper.PrintOutput(Output);
            Helper.AssertNotNextOrder(EKeyExecutionStep.BeforeCall, EKeyExecutionStep.AfterCall);

            Helper.AssertOrder(EKeyExecutionStep.BeforeCall,
                EKeyExecutionStep.BeforeBlocking,
                EKeyExecutionStep.BeforeNonBlocking,
                EKeyExecutionStep.BeforeInside,
                EKeyExecutionStep.AfterCall,
                EKeyExecutionStep.AfterInside,
                EKeyExecutionStep.Finish,
                EKeyExecutionStep.AfterWaitingForAsync);
        }

        [Fact]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async void OrderWithoutAwaitOrSleep()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Helper.NextOrder = EKeyExecutionStep.BeforeCall;
            var t = ExampleAsyncWithRun();
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
                EKeyExecutionStep.BeforeInside,
                EKeyExecutionStep.AfterCall,
                EKeyExecutionStep.AfterWaitingForAsync);
        }

        private async Task ExampleAsyncWithRun()
        {
            Helper.NextOrder = EKeyExecutionStep.BeforeBlocking;
            Thread.Sleep(Wait);
            Helper.NextOrder = EKeyExecutionStep.BeforeNonBlocking;
            await Task.Run((Action)NonAsyncMethod);
            Helper.NextOrder = EKeyExecutionStep.Finish;
        }

        private void NonAsyncMethod()
        {
            Helper.NextOrder = EKeyExecutionStep.BeforeInside; // wins the race condition
            Thread.Sleep(Wait);
            Helper.NextOrder = EKeyExecutionStep.AfterInside;
        }
    }
}
