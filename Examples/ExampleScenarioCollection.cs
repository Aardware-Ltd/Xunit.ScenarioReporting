using System.Threading;
using Xunit;

namespace Examples
{
    [CollectionDefinition("ExampleScenario")]
    public class ExampleScenarioCollection : ICollectionFixture<
        ExampleScenarioCollection.InstanceCountingExampleScenario>
    {
        public class InstanceCountingExampleScenario : ExampleScenario
        {
            public InstanceCountingExampleScenario()
            {
                Interlocked.Increment(ref _instanceCount);
            }
            public int InstanceCount => _instanceCount;
            static int _instanceCount = 0;
        }
    }
}