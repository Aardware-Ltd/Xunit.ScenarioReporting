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
        /// <summary>
        /// Gets the display name of the xUnit.net v2 test framework.
        /// </summary>
        public static readonly string DisplayName = string.Format(CultureInfo.InvariantCulture, "xUnit.net {0}", new object[] { typeof(XunitTestFrameworkDiscoverer).GetTypeInfo().Assembly.GetName().Version });

        readonly Dictionary<Type, IXunitTestCaseDiscoverer> discovererCache = new Dictionary<Type, IXunitTestCaseDiscoverer>();

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestFrameworkDiscoverer"/> class.
        /// </summary>
        /// <param name="assemblyInfo">The test assembly.</param>
        /// <param name="sourceProvider">The source information provider.</param>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="collectionFactory">The test collection factory used to look up test collections.</param>
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