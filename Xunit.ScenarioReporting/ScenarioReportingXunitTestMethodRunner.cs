using System.Collections.Generic;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.ScenarioReporting
{
    class ScenarioReportingXunitTestMethodRunner : TestMethodRunner<ScenarioReportingXunitTestCase>
    {
        private readonly ScenarioReport _report;
        private readonly IMessageSink _diagnosticMessageSink;
        private readonly object[] _constructorArguments;

        protected override Task<RunSummary> RunTestCaseAsync(ScenarioReportingXunitTestCase testCase)
        {
            return testCase.RunAsync(_report, _diagnosticMessageSink, MessageBus, _constructorArguments,
                new ExceptionAggregator(Aggregator), CancellationTokenSource);
        }

        public ScenarioReportingXunitTestMethodRunner(ScenarioReport report, ITestMethod testMethod, IReflectionTypeInfo @class, IReflectionMethodInfo method, IEnumerable<ScenarioReportingXunitTestCase> testCases, IMessageSink diagnosticMessageSink, IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, object[] constructorArguments) : base(testMethod, @class, method, testCases, messageBus, aggregator, cancellationTokenSource)
        {
            _report = report;
            _diagnosticMessageSink = diagnosticMessageSink;
            _constructorArguments = constructorArguments;
        }
    }
}