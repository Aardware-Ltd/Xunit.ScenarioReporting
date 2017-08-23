using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xunit.ScenarioReporting
{
    class ScenarioVerificationException : Exception
    {
        internal ScenarioVerificationException(Scenario.ReportEntry.Then[] failures): base(FormatFailures(failures))
        {
            
        }

        static string FormatFailures(IReadOnlyList<Scenario.ReportEntry.Then> failures)
        {
            var sb = new StringBuilder();
            foreach (var failure in failures)
            {
                sb.AppendLine(failure.Title);
                foreach (var detail in failure.Details.OfType<Scenario.ReportEntry.Mismatch>())
                {
                    sb.AppendLine($"  Expected : {detail.Value}");
                    sb.AppendLine($"  Actual   : {detail.Actual}");
                }
            }
            return sb.ToString();
        }
    }
}