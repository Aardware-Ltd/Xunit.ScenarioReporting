using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.ScenarioReporting
{
    class ScenarioReportingXunitTestCaseRunner : XunitTestCaseRunner
    {
        private readonly Scenario _scenario;
        private readonly ScenarioReport _report;

        public ScenarioReportingXunitTestCaseRunner(Scenario scenario, ScenarioReport report, IXunitTestCase testCase, string displayName,
            string skipReason, object[] constructorArguments, object[] testMethodArguments, IMessageBus messageBus,
            ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource) : base(testCase,
            displayName, skipReason, constructorArguments, testMethodArguments, messageBus, aggregator,
            cancellationTokenSource)
        {
            _scenario = scenario;
            _report = report;
        }

        protected override Task<RunSummary> RunTestAsync()
        {
            return new ScenarioReportingXunitTestRunner(_scenario, _report, new XunitTest(TestCase, DisplayName), MessageBus,
                TestClass, ConstructorArguments, TestMethod, TestMethodArguments, SkipReason, BeforeAfterAttributes,
                new ExceptionAggregator(Aggregator), CancellationTokenSource).RunAsync();
        }
    }
}