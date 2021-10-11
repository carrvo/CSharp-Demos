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
    public sealed class WhenSerial
    {
        private TimeSpan Wait { get; } = TimeSpan.FromSeconds(1);

        private enum EKeyExecutionStep
        {
            BeforeAll,
            BeforeTopBlocking,
            AfterTopBlocking,
            Finish,
            BeforeOuterFirst,
            BeforeOuterFirstBlocking,
            AfterOuterFirstBlocking,
            AfterOuterFirst,
            BeforeOuterSecond,
            AfterOuterSecond,
            BeforeInnerFirst,
            BeforeInnerFirstBlocking,
            AfterInnerFirstBlocking,
            AfterInnerFirst,
            BeforeInnerSecond,
            AfterInnerSecond,
            BeforeBottom,
            AfterBottom,
        }
        private BehaviourHelper<EKeyExecutionStep> Helper
        {
            get;
        }

        private ITestOutputHelper Output
        {
            get;
        }

        public WhenSerial(ITestOutputHelper output)
        {
            Output = output;
            Helper = new BehaviourHelper<EKeyExecutionStep>();
        }

        /// <summary>
        /// Serialized despite using await.
        /// Has to do with awaiting in a different place than the call.
        /// </summary>
        [Fact]
        public async void Serialized()
        {
            Helper.NextOrder = EKeyExecutionStep.BeforeAll;
            var first = OuterFirst();
            var second = OuterSecond();
            Helper.NextOrder = EKeyExecutionStep.BeforeTopBlocking;
            Thread.Sleep(Wait);
            Helper.NextOrder = EKeyExecutionStep.AfterTopBlocking;
            await first;
            await second;
            Helper.NextOrder = EKeyExecutionStep.Finish;

            Helper.PrintOutput(Output);
            Helper.AssertOrder(EKeyExecutionStep.BeforeAll,
                EKeyExecutionStep.BeforeOuterFirst,
                EKeyExecutionStep.BeforeInnerFirst,
                EKeyExecutionStep.BeforeBottom,
                EKeyExecutionStep.AfterBottom,
                EKeyExecutionStep.BeforeInnerFirstBlocking,
                EKeyExecutionStep.AfterInnerFirstBlocking,
                EKeyExecutionStep.AfterInnerFirst,
                EKeyExecutionStep.BeforeInnerSecond,
                EKeyExecutionStep.AfterInnerSecond,
                EKeyExecutionStep.BeforeOuterFirstBlocking,
                EKeyExecutionStep.AfterOuterFirstBlocking,
                EKeyExecutionStep.AfterOuterFirst,
                EKeyExecutionStep.BeforeOuterSecond,
                EKeyExecutionStep.AfterOuterSecond,
                EKeyExecutionStep.BeforeTopBlocking,
                EKeyExecutionStep.AfterTopBlocking,
                EKeyExecutionStep.Finish);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

        private async Task OuterFirst()
        {
            Helper.NextOrder = EKeyExecutionStep.BeforeOuterFirst;
            var first = InnerFirst();
            var second = InnerSecond();
            Helper.NextOrder = EKeyExecutionStep.BeforeOuterFirstBlocking;
            Thread.Sleep(Wait);
            Helper.NextOrder = EKeyExecutionStep.AfterOuterFirstBlocking;
            await first;
            await second;
            Helper.NextOrder = EKeyExecutionStep.AfterOuterFirst;
        }

        private async Task OuterSecond()
        {
            Helper.NextOrder = EKeyExecutionStep.BeforeOuterSecond;
            Thread.Sleep(Wait);
            Helper.NextOrder = EKeyExecutionStep.AfterOuterSecond;
        }

        private async Task InnerFirst()
        {
            Helper.NextOrder = EKeyExecutionStep.BeforeInnerFirst;
            var first = RecursionLayer();
            Helper.NextOrder = EKeyExecutionStep.BeforeInnerFirstBlocking;
            Thread.Sleep(Wait);
            Helper.NextOrder = EKeyExecutionStep.AfterInnerFirstBlocking;
            await first;
            Helper.NextOrder = EKeyExecutionStep.AfterInnerFirst;
        }

        private async Task InnerSecond()
        {
            Helper.NextOrder = EKeyExecutionStep.BeforeInnerSecond;
            Thread.Sleep(Wait);
            Helper.NextOrder = EKeyExecutionStep.AfterInnerSecond;
        }

        private async Task RecursionLayer()
        {
            Helper.NextOrder = EKeyExecutionStep.BeforeBottom;
            Thread.Sleep(Wait);
            Helper.NextOrder = EKeyExecutionStep.AfterBottom;
        }

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}
