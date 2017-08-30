using System.Collections.Generic;

namespace Xunit.ScenarioReporting.Results
{
    internal class When : ReportEntry
    {
        public When(string title, IReadOnlyList<Detail> details) : base(title, details)
        {
        }
    }
}