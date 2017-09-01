using System.Collections.Generic;

namespace Xunit.ScenarioReporting.Results
{
    internal class Then : ReportEntry
    {
        public string Scope { get; }

        public Then(string scope, string title, IReadOnlyList<Detail> details) : base(title, details)
        {
            Scope = scope;
        }
    }
}