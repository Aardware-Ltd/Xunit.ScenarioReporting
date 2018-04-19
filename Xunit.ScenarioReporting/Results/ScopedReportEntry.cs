using System.Collections.Generic;

namespace Xunit.ScenarioReporting.Results
{
    internal class ScopedReportEntry : ReportEntry
    {
        public string Scope { get; }

        public ScopedReportEntry(string scope, string title, IReadOnlyList<Detail> details) : base(title, details)
        {
            Scope = scope;
        }
    }
}