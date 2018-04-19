using System;

namespace Xunit.ScenarioReporting.Results
{
    class StartReport : ReportItem
    {
        public string ReportName { get; }
        public DateTimeOffset ReportTime { get; }

        public StartReport(string reportName, DateTimeOffset reportTime)
        {
            ReportName = reportName;
            ReportTime = reportTime;
        }
    }
}