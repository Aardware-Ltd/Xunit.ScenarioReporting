using Xunit;

namespace ScenarioReportingTests
{
    [Collection("ExampleScenario")]
    public class CollectionFixtureScenarioExample
    {
        private readonly ExampleScenarioCollection.InstanceCountingExampleScenario _scenario;
        
        public CollectionFixtureScenarioExample(ExampleScenarioCollection.InstanceCountingExampleScenario scenario)
        {
            _scenario = scenario;

        }

        [Fact]
        public void ShouldBe1()
        {
            Assert.Equal(1, _scenario.InstanceCount);
        }

        [Fact]
        public void ShouldStillBe1()
        {
            Assert.Equal(1, _scenario.InstanceCount);
        }

    }
}