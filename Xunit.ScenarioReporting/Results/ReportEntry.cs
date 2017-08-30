using System.Collections.Generic;

namespace Xunit.ScenarioReporting.Results
{
    internal class ReportEntry : ReportItem
    {
        public ReportEntry(string title, IReadOnlyList<Detail> details)
        {
            Title = title;
            Details = details;
        }

        public string Title { get; }
        public IReadOnlyList<Detail> Details { get; }


    }
}
