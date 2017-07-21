using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.ScenarioReporting
{
    class ScenarioTheoryAttributeDiscoverer : TheoryDiscoverer {
        public ScenarioTheoryAttributeDiscoverer(IMessageSink diagnosticMessageSink) : base(diagnosticMessageSink)
        {
        }

        protected override IEnumerable<IXunitTestCase> CreateTestCasesForDataRow(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute, object[] dataRow)
            => new[] { new ScenarioReportingXunitTestCase(DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod, dataRow) };
    }
}