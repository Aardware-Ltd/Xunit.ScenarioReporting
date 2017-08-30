using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.ScenarioReporting
{
    class ScenarioReportingXunitTestCaseRunner : XunitTestCaseRunner
    {
        private readonly ScenarioRunner _scenarioRunner;
        private readonly ScenarioReport _report;

        public ScenarioReportingXunitTestCaseRunner(ScenarioRunner scenarioRunner, ScenarioReport report, IXunitTestCase testCase, string displayName,
            string skipReason, object[] constructorArguments, object[] testMethodArguments, IMessageBus messageBus,
            ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource) : base(testCase,
            displayName, skipReason, constructorArguments, testMethodArguments, messageBus, aggregator,
            cancellationTokenSource)
        {
            _scenarioRunner = scenarioRunner;
            _report = report;
        }

        protected override Task<RunSummary> RunTestAsync()
        {
            return new ScenarioReportingXunitTestRunner(_scenarioRunner, _report, new XunitTest(TestCase, DisplayName), MessageBus,
                TestClass, ConstructorArguments, TestMethod, TestMethodArguments, SkipReason, BeforeAfterAttributes,
                new ExceptionAggregator(Aggregator), CancellationTokenSource).RunAsync();
        }
    }
}