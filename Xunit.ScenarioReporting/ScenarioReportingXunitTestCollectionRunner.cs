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
    class ScenarioReportingXunitTestCollectionRunner : XunitTestCollectionRunner
    {
        private readonly ScenarioReport _report;
        readonly IMessageSink _diagnosticMessageSink;
        private ScenarioRunner _scenarioRunner;

        public ScenarioReportingXunitTestCollectionRunner(ScenarioReport report,
                                                            ITestCollection testCollection,
                                                            IEnumerable<IXunitTestCase> testCases,
                                                            IMessageSink diagnosticMessageSink,
                                                            IMessageBus messageBus,
                                                            ITestCaseOrderer testCaseOrderer,
                                                            ExceptionAggregator aggregator,
                                                            CancellationTokenSource cancellationTokenSource)
            : base(testCollection, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
        {
            _report = report;
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        protected override async Task AfterTestCollectionStartingAsync()
        {
            await base.AfterTestCollectionStartingAsync();
            Aggregator.Run(() =>
            {
                _scenarioRunner = CollectionFixtureMappings.Values.OfType<ScenarioRunner>().SingleOrDefault();
                if (_scenarioRunner != null)
                {
                    _scenarioRunner.DelayReporting = true;
                    _scenarioRunner.Scope = TestCollection.CollectionDefinition.Name;
                }
            });
        }

        protected override Task<RunSummary> RunTestClassAsync(ITestClass testClass, IReflectionTypeInfo @class, IEnumerable<IXunitTestCase> testCases)
        {
            return new ScenarioReportingXunitTestClassRunner(_report, testClass, @class, testCases, _diagnosticMessageSink, MessageBus, TestCaseOrderer, new ExceptionAggregator(Aggregator), CancellationTokenSource, CollectionFixtureMappings).RunAsync();
        }

        protected override async Task BeforeTestCollectionFinishedAsync()
        {
            
            if (_scenarioRunner != null)
            {
                try
                {
                    await _scenarioRunner.Complete(_report);
                }
                catch (Exception ex)
                {
                    Aggregator.Add(ex);
                }
            }

            await base.BeforeTestCollectionFinishedAsync();
        }
    }
}
