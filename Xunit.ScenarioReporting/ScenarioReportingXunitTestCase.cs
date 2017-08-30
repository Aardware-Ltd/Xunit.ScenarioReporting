using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.ScenarioReporting
{
    class ScenarioReportingXunitTestCase : XunitTestCase
    {
        [Obsolete("Called by deserializer")]
        public ScenarioReportingXunitTestCase()
        {

        }

        public ScenarioReportingXunitTestCase(
            IMessageSink diagnosticMessageSink,
            TestMethodDisplay defaultMethodDisplay,
            ITestMethod testMethod,
            object[] testMethodArguments = null)
            : base(diagnosticMessageSink, defaultMethodDisplay, testMethod, testMethodArguments)
        {

        }

        public override Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus,
            object[] constructorArguments,
            ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
        {

            throw new NotSupportedException("Must use overload with report");
        }
        public Task<RunSummary> RunAsync(ScenarioReport report, IMessageSink diagnosticMessageSink, IMessageBus messageBus,
            object[] constructorArguments,
            ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
        {

            var scenario = constructorArguments.OfType<ScenarioRunner>().SingleOrDefault();
            return new ScenarioReportingXunitTestCaseRunner(scenario, report, this, DisplayName, SkipReason,
                constructorArguments, TestMethodArguments, messageBus, new ExceptionAggregator(aggregator),
                cancellationTokenSource).RunAsync();
        }
    }
}