using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.ScenarioReporting
{
    class ScenarioReportingXunitTestFrameworkDiscoverer : XunitTestFrameworkDiscoverer {
        
        public ScenarioReportingXunitTestFrameworkDiscoverer(IAssemblyInfo assemblyInfo,
            ISourceInformationProvider sourceProvider,
            IMessageSink diagnosticMessageSink,
            IXunitTestCollectionFactory collectionFactory = null)
            : base(assemblyInfo, sourceProvider, diagnosticMessageSink)
        {
            DiscovererTypeCache[typeof(FactAttribute)] = typeof(ScenarioFactAttributeDiscoverer);
            DiscovererTypeCache[typeof(TheoryAttribute)] = typeof(ScenarioTheoryAttributeDiscoverer);
        }
        
    }
}