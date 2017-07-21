using Xunit;
using Xunit.Abstractions;

namespace ScenarioReportingTests
{
    public class ClassScenarioFixtureExample : IClassFixture<ExampleScenario>
    {
        private readonly ExampleScenario _scenario;
        private ITestOutputHelper _output;
        
        public ClassScenarioFixtureExample(ExampleScenario scenario, ITestOutputHelper output)
        {
            _scenario = scenario;
            _output = output;
        }

        [Fact]
        public void OtherInvariantShouldBeTrue()
        {
            Assert.True(_scenario.OtherInvariant);
        }
    }
}