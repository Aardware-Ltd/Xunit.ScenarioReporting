namespace Xunit.ScenarioReporting
{
    /// <summary>
    /// Defines constants used in the assembly
    /// </summary>
    public static class Constants
    {
        internal const string XmlTagAssembly = "Assembly";
        internal const string XmlTagName = "Name";
        internal const string XmlTagNDG = "NDG";
        internal const string XmlTagScope = "Scope";
        internal const string XmlTagValue = "Value";
        internal const string XmlTagExpected = "Expected";
        internal const string XmlTagActual = "Actual";
        internal const string XmlTagTime = "Time";
        internal const string XmlTagScenario = "Definition";
        internal const string XmlTagGiven = "Given";
        internal const string XmlTagThen = "Then";
        internal const string XmlTagWhen = "When";
        internal const string XmlTagChild = "Child";
        internal const string XmlTagDetails = "Details";
        internal const string XmlTagTitle = "Title";
        internal const string XmlTagDetail = "Detail";
        internal const string XmlTagMessage = "Message";

        //Error reporting
        internal const string XmlTagFailure = "Failure"; 
        internal const string XmlTagMismatch = "Mismatch";
        internal const string XmlTagException = "Exception";

        internal const string ReportAssemblyOverviewHtmlHeader = "ReportAssemblyOverviewHTMLHeader.html";
        internal const string ReportAssemblyOverviewHtmlContent = "ReportAssemblyOverviewHTMLContent.xslt";
        internal const string ReportAssemblyOverviewHtmlFooter = "ReportAssemblyOverviewHTMLFooter.html";
        internal const string ReportAssemblyOverviewHtml = "ReportAssemblyOverview.html";
        internal const string ReportAssemblyOverviewMarkdownContent = "ReportAssemblyOverviewMarkdownContent.xslt";
        internal const string ReportAssemblyOverviewMarkdown = "ReportAssemblyOverview.md";
        internal const string ReportPath = "Reports";
        
        /// <summary>
        /// The name of the assembly to use in the <see cref="TestFrameworkAttribute"/>
        /// </summary>
        public const string AssemblyName = "Xunit.ScenarioReporting";
        /// <summary>
        /// The name of the framework to use in the <see cref="TestFrameworkAttribute"/>
        /// </summary>
        public const string Framework = AssemblyName + "." + nameof(ScenarioReportingXunitTestFramework);

        internal static class Errors
        {
            public static string DontReturnScenarioResults =
                    "Returning Scenario run results where in a class that takes as ScenarioRunner as a constructor parameter in not allowed";

            public static string ScenarioNotDefined = "Scenario is not defined";    
        }
    }
}