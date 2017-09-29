using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.ScenarioReporting
{
    internal class ScenarioReportingXunitTestAssemblyRunner : XunitTestAssemblyRunner
    {
        private ScenarioReport _scenarioReport;
        private OutputController _controller;

        public ScenarioReportingXunitTestAssemblyRunner(ITestAssembly testAssembly,
                                                          IEnumerable<IXunitTestCase> testCases,
                                                          IMessageSink diagnosticMessageSink,
                                                          IMessageSink executionMessageSink,
                                                          ITestFrameworkExecutionOptions executionOptions)
            : base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
        { }

        protected override async Task AfterTestAssemblyStartingAsync()
        {
            // Let everything initialize
            await base.AfterTestAssemblyStartingAsync();
            
            // Go find all the AssemblyFixtureAttributes adorned on the test assembly
            Aggregator.Run(() =>
            {
                var name = TestAssembly.Assembly.Name;
                if (name.Contains(','))
                {
                    var assemblyName = new AssemblyName(name);
                    name = assemblyName.Name;
                }
         
                var configuration = new ReportConfiguration(name, Directory.GetCurrentDirectory(), TestAssembly.Assembly.AssemblyPath, TestAssembly.ConfigFileName);
                _controller = new OutputController(configuration, DiagnosticMessageSink);
                _scenarioReport = _controller.Report;
            });
        }

        protected override async Task BeforeTestAssemblyFinishedAsync()
        {
            if(_controller != null)
                await _controller.Complete();
            await base.BeforeTestAssemblyFinishedAsync();
        }

        protected override Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus,
                                                                   ITestCollection testCollection,
                                                                   IEnumerable<IXunitTestCase> testCases,
                                                                   CancellationTokenSource cancellationTokenSource)
            => new ScenarioReportingXunitTestCollectionRunner(_scenarioReport, testCollection, testCases, DiagnosticMessageSink, messageBus, TestCaseOrderer, new ExceptionAggregator(Aggregator), cancellationTokenSource).RunAsync();
    }
}
