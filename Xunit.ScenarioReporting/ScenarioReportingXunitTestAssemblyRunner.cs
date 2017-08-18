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
    public class ScenarioReportingXunitTestAssemblyRunner : XunitTestAssemblyRunner
    {
        private ScenarioReport _scenarioReport;
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
                if (TestAssembly.Assembly.AssemblyPath == null && TestAssembly.ConfigFileName == null)
                {
                    _scenarioReport = new ScenarioReport(name,new NullWriter());
                    return;
                }

                // var path = GetOutputPath(TestAssembly.Assembly.AssemblyPath, TestAssembly.ConfigFileName);                
                _scenarioReport = new ScenarioReport(name, new ReportWriter(TestAssembly.Assembly.AssemblyPath, TestAssembly.ConfigFileName));
            });
        }
        class NullWriter : IReportWriter {
            public Task Write(ReportItem item)
            {
                return Task.CompletedTask;
            }
        }
        static string GetOutputPath(string assemblyPath, string configFilePath)
        {
            if (configFilePath != null && ! string.Equals(Path.GetFileName(configFilePath), "xunit.console.exe.Config", StringComparison.InvariantCultureIgnoreCase))
            {
                //TODO: try load config to see if it has an output path
                return Path.GetDirectoryName(configFilePath);
            }
            return Path.GetDirectoryName(assemblyPath);
        }

        protected override async Task BeforeTestAssemblyFinishedAsync()
        {
            await _scenarioReport.WriteFinalAsync();
            await base.BeforeTestAssemblyFinishedAsync();
        }

        protected override Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus,
                                                                   ITestCollection testCollection,
                                                                   IEnumerable<IXunitTestCase> testCases,
                                                                   CancellationTokenSource cancellationTokenSource)
            => new ScenarioReportingXunitTestCollectionRunner(_scenarioReport, testCollection, testCases, DiagnosticMessageSink, messageBus, TestCaseOrderer, new ExceptionAggregator(Aggregator), cancellationTokenSource).RunAsync();
    }
}
