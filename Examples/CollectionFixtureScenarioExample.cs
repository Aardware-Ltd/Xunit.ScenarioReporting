using System.Threading.Tasks;
using Examples.ExampleDomain;
using Xunit;
using Xunit.ScenarioReporting;

namespace Examples
{
    [Collection("ExampleScenarioRunner")]
    public class CollectionFixtureScenarioExample : IAsyncLifetime
    {
        private readonly ExampleScenarioCollection.InstanceCountingExampleScenarioRunner _scenarioRunner;
        
        public CollectionFixtureScenarioExample(ExampleScenarioCollection.InstanceCountingExampleScenarioRunner scenarioRunner)
        {
            _scenarioRunner = scenarioRunner;
        }

        [Fact]
        public void ShouldBe1()
        {
            Assert.Equal(1, _scenarioRunner.InstanceCount);
        }

        [Fact]
        public void ShouldStillBe1()
        {
            Assert.Equal(1, _scenarioRunner.InstanceCount);
        }

        public Task InitializeAsync()
        {
            return _scenarioRunner.Run(def =>
                def.Given(new Number(10), new Number(30), new Number(2)).When(new Operation(OperationType.Add))
                    .Then(new ComputedResult(42)));
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}