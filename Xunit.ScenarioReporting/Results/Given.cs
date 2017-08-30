using System.Collections.Generic;

namespace Xunit.ScenarioReporting.Results
{
    internal class Given : ReportEntry
    {
        public Given(string title, IReadOnlyList<Detail> details) : base(title, details)
        {
        }
    }
}