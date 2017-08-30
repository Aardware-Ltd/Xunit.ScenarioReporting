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

        internal ScenarioRunResult(string title, IReadOnlyList<Given> given, When when, IReadOnlyList<Then> then, ExceptionDispatchInfo errorInfo)
        {
            ErrorInfo = errorInfo;
            Title = title;
            Given = given;
            When = when;
            Then = then;
        }

        internal string Title { get; set; }
        internal IReadOnlyList<Given> Given { get; }
        internal When When { get; }

        internal IReadOnlyList<Then> Then { get; }

        internal void ThrowIfErrored()
        {
            ErrorInfo?.Throw();
            var failures = Failures().Concat(Mismatches()).ToArray();
            if (failures.Any())
                throw new ScenarioVerificationException(failures.ToArray());
        }

        private IEnumerable<Then> Mismatches()
        {
            return Then
                .Where(x => x.Details.OfType<Mismatch>().Any())
                .Select(x => new Then(x.Title, x.Details.OfType<Mismatch>().ToArray()));
        }

        private IEnumerable<Assertion> Failures()
        {
            return Then.OfType<Assertion>().Where(x => x.Details.OfType<Failure>().Any());
        }

        internal Exception VerificationException(Then[] failures)
        {
            return new ScenarioVerificationException(failures);
        }
    }
}