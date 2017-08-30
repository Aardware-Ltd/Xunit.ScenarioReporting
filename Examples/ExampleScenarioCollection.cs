using System.Threading;
using Xunit;

namespace Examples
{
    [CollectionDefinition("ExampleScenarioRunner")]
    public class ExampleScenarioCollection : ICollectionFixture<
        ExampleScenarioCollection.InstanceCountingExampleScenarioRunner>
    {
        public class InstanceCountingExampleScenarioRunner : ExampleScenarioRunner
        {
            public InstanceCountingExampleScenarioRunner()
            {
                Interlocked.Increment(ref _instanceCount);
            }
            public int InstanceCount => _instanceCount;
            static int _instanceCount = 0;
        }
    }
}