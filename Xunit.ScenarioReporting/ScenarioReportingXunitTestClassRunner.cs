using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.ScenarioReporting
{
    class ScenarioReportingXunitTestClassRunner : XunitTestClassRunner
    {
        private readonly ScenarioReport _report;
        private ScenarioRunner _scenarioRunner;

        public ScenarioReportingXunitTestClassRunner(
                                          ScenarioReport report,
                                          ITestClass testClass,
                                          IReflectionTypeInfo @class,
                                          IEnumerable<IXunitTestCase> testCases,
                                          IMessageSink diagnosticMessageSink,
                                          IMessageBus messageBus,
                                          ITestCaseOrderer testCaseOrderer,
                                          ExceptionAggregator aggregator,
                                          CancellationTokenSource cancellationTokenSource,
                                          IDictionary<Type, object> collectionFixtureMappings)
            : base(testClass, @class, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource, collectionFixtureMappings)
        {
            _report = report;
        }

        protected override Task<RunSummary> RunTestMethodAsync(ITestMethod testMethod, IReflectionMethodInfo method, IEnumerable<IXunitTestCase> testCases,
            object[] constructorArguments)
        {
            return new ScenarioReportingXunitTestMethodRunner(_report, testMethod, Class, method, testCases.OfType<ScenarioReportingXunitTestCase>(), DiagnosticMessageSink, MessageBus, new ExceptionAggregator(Aggregator), CancellationTokenSource, constructorArguments).RunAsync();
        }

        protected override async Task AfterTestClassStartingAsync()
        {
            await base.AfterTestClassStartingAsync();
            Aggregator.Run(() =>
            {
                _scenarioRunner = ClassFixtureMappings.Values.OfType<ScenarioRunner>().SingleOrDefault();
                if (_scenarioRunner != null)
                {
                    _scenarioRunner.DelayReporting = true;
                    _scenarioRunner.Scope = Class.Name;
                }
            });
        }

        protected override async Task BeforeTestClassFinishedAsync()
        {
            if (_scenarioRunner != null)
            {
                await Aggregator.RunAsync(() => _scenarioRunner.Complete(_report));
            }
            await base.BeforeTestClassFinishedAsync();
        }
    }
}
