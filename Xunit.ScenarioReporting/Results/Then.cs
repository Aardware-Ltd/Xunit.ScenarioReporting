using System.Collections.Generic;

namespace Xunit.ScenarioReporting.Results
{
    internal class Then : ReportEntry
    {
        public Then(string title, IReadOnlyList<Detail> details) : base(title, details)
        {

        }
    }
}