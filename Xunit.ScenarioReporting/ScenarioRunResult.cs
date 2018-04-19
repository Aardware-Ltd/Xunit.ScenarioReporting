using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using Xunit.ScenarioReporting.Results;

namespace Xunit.ScenarioReporting
{
    /// <summary>
    /// The result of running a Scenario. Used internally for reporting.
    /// </summary>
    public class ScenarioRunResult
    {
        internal readonly ExceptionDispatchInfo ErrorInfo;

        internal ScenarioRunResult(string title, IReadOnlyList<ReportEntry> entries, ExceptionDispatchInfo errorInfo)
        {
            ErrorInfo = errorInfo;
            Title = title;
            Entries = entries;
        }
        /// <summary>
        /// Gets or sets the scope of the scenario. Generally the scope will be set automatically
        /// by the test framework.
        /// </summary>
        public string Scope { get; set; }

        internal string Grouping { get; set; }

        internal string Title { get; set; }
       

        internal void ThrowIfErrored()
        {
            ErrorInfo?.Throw();
            var failures = Failures().Concat(Mismatches()).ToArray();
            if (failures.Any())
                throw new ScenarioVerificationException(failures.ToArray());
        }
        internal IReadOnlyList<ReportEntry> Entries { get; }
        private IEnumerable<ReportEntry> Mismatches()
        {
            return Entries.Traverse(ChildAccessor)
                .Where(x => x.Details.OfType<Mismatch>().Any())
                .Select(x => new ReportEntry(x.Title, x.Details.OfType<Mismatch>().ToArray()));
        }

        private static IEnumerable<ReportEntry> ChildAccessor(ReportEntry r)
        {
            if (r is ReportEntryGroup group)
                return @group;
            return Empty<ReportEntry>.ReadOnlyList;
        }

        private IEnumerable<Assertion> Failures()
        {
            return Entries.Traverse(ChildAccessor).OfType<Assertion>().Where(x => x.Details.OfType<Failure>().Any());
        }

        internal Exception VerificationException(ReportEntry[] failures)
        {
            return new ScenarioVerificationException(failures);
        }
    }
}