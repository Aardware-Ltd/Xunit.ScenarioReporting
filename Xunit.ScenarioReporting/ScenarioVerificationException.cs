using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit.ScenarioReporting.Results;

namespace Xunit.ScenarioReporting
{
    /// <summary>
    /// Thrown if the scenario result has any failures
    /// </summary>
    public class ScenarioVerificationException : Exception
    {
        internal ScenarioVerificationException(Then[] failures): base(FormatFailures(failures))
        {
            
        }

        static string FormatFailures(IReadOnlyList<Then> failures)
        {
            var sb = new StringBuilder();
            foreach (var failure in failures)
            {
                sb.AppendLine(failure.Title);
                foreach (var detail in failure.Details.OfType<Mismatch>())
                {
                    sb.AppendLine($"  Expected : {detail.Value}");
                    sb.AppendLine($"  Actual   : {detail.Actual}");
                }
            }
            return sb.ToString();
        }
    }
}