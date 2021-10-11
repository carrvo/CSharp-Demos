using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CSharp.Demos.Asynchronous.Behaviour
{
    public sealed class BehaviourHelper<TEnum>
        //where TEnum : Enum
    {
        private Boolean _endTest;

        private IList<(DateTime Timestamp, TEnum Step)> ExecutionOrder
        {
            get;
        }

        public TEnum NextOrder
        {
            set
            {
                if (!_endTest)
                {
                    ExecutionOrder.Add((DateTime.Now, value));
                }
            }
        }

        public BehaviourHelper()
        {
            _endTest = false;
            ExecutionOrder = new List<(DateTime, TEnum)>();
        }

        public void EndTest() => _endTest = true;

        public void PrintOutput(ITestOutputHelper output)
        {
            EndTest();
            foreach (var order in ExecutionOrder)
            {
                output.WriteLine($"[{order.Timestamp}] {order.Step}");
            }
        }

        public void AssertDoesNotContain(TEnum value)
        {
            Assert.DoesNotContain(value, ExecutionOrder.Select(x => x.Step));
        }

        public void AssertOrder(TEnum before, TEnum after)
        {
            var beforeStep = ExecutionOrder.Single(x => x.Step.Equals(before));
            var afterStep = ExecutionOrder.Single(x => x.Step.Equals(after));
            Assert.True(beforeStep.Timestamp < afterStep.Timestamp,
                $"Before: {before} ({beforeStep.Timestamp}){Environment.NewLine}After: {after} ({afterStep.Timestamp})");
        }

        public void AssertNextOrder(TEnum before, TEnum after)
        {
            var afterStep = ExecutionOrder.Single(x => x.Step.Equals(after));
            var nextStep = ExecutionOrder.SkipWhile(x => !x.Step.Equals(before)).Skip(1).First();
            Assert.Equal(afterStep, nextStep);
        }

        public void AssertNotNextOrder(TEnum before, TEnum after)
        {
            var afterStep = ExecutionOrder.Single(x => x.Step.Equals(after));
            var nextStep = ExecutionOrder.SkipWhile(x => !x.Step.Equals(before)).Skip(1).First();
            Assert.NotEqual(afterStep, nextStep);
        }

        public void AssertOrder(params TEnum[] values)
        {
            for (Int32 index = 0; index + 1 < values.Length; index++)
            {
                AssertNextOrder(values[index], values[index + 1]);
            }
        }
    }
}
