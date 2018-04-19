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
        internal ScenarioVerificationException(ReportEntry[] failures): base(FormatFailures(failures))
        {
            
        }

        static string FormatFailures(IReadOnlyList<ReportEntry> failures)
        {
            var sb = new StringBuilder();
            bool first = true;
            foreach (var failure in failures.Where(x=> !(x.Details.OfType<MissingResult>().Any() || x.Details.OfType<ExtraResult>().Any())))
            {
                if (first)
                {
                    sb.AppendLine("These types had incorrect values: ");
                    first = false;
                }
                sb.AppendLine($"Type : {failure.Title}");
                foreach (var detail in failure.Details.OfType<Mismatch>())
                {
                    var indent = "";
                    if (detail.Name != failure.Title)
                    {
                        indent = "  ";
                        sb.AppendLine($"{indent}Property: {detail.Name}");
                    }
                    sb.AppendLine($"{indent}  Expected : {detail.Value}");
                    sb.AppendLine($"{indent}  Actual   : {detail.Actual}");
                }
            }
            var missing = failures.Where(x => x.Details.OfType<MissingResult>().Any()).ToArray();
            var extra = failures.Where(x => x.Details.OfType<ExtraResult>().Any()).ToArray();
            first = true;
            foreach (var m in missing)
            {
                if (first)
                {
                    first = false;
                    sb.AppendLine();
                    if (missing.Length == 1)
                        sb.AppendLine("The following Then was expected but not present:");
                    else
                        sb.AppendLine($"The following {missing.Length} Thens were expected but were not present:");
                }
                foreach (var detail in m.Details.OfType<MissingResult>())
                {
                    sb.AppendLine(detail.Formatter(detail.Value));
                }
            }
            foreach (var e in extra)
            {
                if (first)
                {
                    first = false;
                    sb.AppendLine();
                    if (extra.Length == 1)
                        sb.AppendLine("The following Then was present but not expected:");
                    else
                        sb.AppendLine($"The following {extra.Length} Thens were present but were not expected:");
                }
                foreach (var detail in e.Details.OfType<ExtraResult>())
                {
                    sb.AppendLine(detail.Formatter(detail.Actual));
                }
            }
            return sb.ToString();
        }
    }
}