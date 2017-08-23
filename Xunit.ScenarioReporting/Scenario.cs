using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xunit.ScenarioReporting
{
    /// <summary>
    /// Used to describe a scenario using given when then 
    /// </summary>
    public abstract class Scenario : IAsyncLifetime
    {
        private bool _verified;
        private bool _initialized;
        private bool _errored;
        readonly List<ReportEntry.Then> _results = new List<ReportEntry.Then>();

        internal void AddResult(string name, Exception e = null)
        {
            if(e != null)
                _results.Add(new ReportEntry.Assertion(name, e));
            else _results.Add(new ReportEntry.Assertion(name));
        }

        internal IReadOnlyList<ReportEntry.Given> GetGivens() => ReportGivens();
        internal ReportEntry.When GetWhen() => ReportWhen();
        internal IReadOnlyList<ReportEntry.Then> GetThens() => ReportThens().Concat(_results).ToList();

        internal abstract IReadOnlyList<ReportEntry.Given> ReportGivens();
        internal abstract ReportEntry.When ReportWhen();
        internal abstract IReadOnlyList<ReportEntry.Then> ReportThens();


        internal class ReportEntry : ReportItem
        {
            public ReportEntry(string title, IReadOnlyList<Detail> details)
            {
                Title = title;
                Details = details;
            }

            public string Title { get; }
            public IReadOnlyList<Detail> Details { get; }

            public class Given : ReportEntry
            {
                public Given(string title, IReadOnlyList<Detail> details) : base(title, details)
                {
                }
            }
            public class When : ReportEntry
            {
                public When(string title, IReadOnlyList<Detail> details) : base(title, details)
                {
                }
            }

            public class Then : ReportEntry
            {
                public Then(string title, IReadOnlyList<Detail> details) : base(title, details)
                {

                }
            }

            public class Assertion : Then
            {
                public Assertion(string title) : base(title, new Detail[] { })
                {
                }

                public Assertion(string title, Exception ex) : base(title, new[] { ExceptionToDetail(ex) }) { }

                static Detail ExceptionToDetail(Exception ex)
                {
                    return new Failure(ex.Message, ex.StackTrace);
                }
            }

            public class Failure : Detail
            {
                public Failure(string name, string stacktrace) : base(name, stacktrace)
                {

                }
            }
            public class Match : Detail
            {
                public Match(string name, object value, string format = null, Func<object, string> formatter = null) : base(name, value, format, formatter)
                {
                }
            }

            public class Mismatch : Detail
            {
                public object Actual { get; }

                public Mismatch(string name, object expected, object actual, string format = null, Func<object, string> formatter = null) : base(name, expected, format, formatter)
                {
                    Actual = actual;
                }
            }

            public class Detail
            {
                public string Name { get; }
                public object Value { get; }
                public string Format { get; }
                public Func<object, string> Formatter { get; }

                public Detail(string name, object value, string format = null, Func<object, string> formatter = null)
                {
                    Name = name;
                    Value = value;
                    Format = format;
                    Formatter = formatter;
                }
            }
        }
        

        async Task IAsyncLifetime.InitializeAsync()
        {
            if (_initialized) return;
            _initialized = true;
            try
            {
                await Initialize();
            }
            catch
            {
                _errored = true;
                throw;
            }
        }

        internal async Task SafeVerify(string name)
        {
            if (_verified || _errored) return;
            try
            {
                await ((IAsyncLifetime)this).InitializeAsync();

                _verified = true;
                await Verify();

            }
            catch (Exception e) when(!(e is ScenarioVerificationException))
            {
                _errored = true;
                AddResult(name, e);
                throw;
            }

            var failures = ReportThens().Where(x => x.Details.OfType<ReportEntry.Mismatch>().Any()).Select(x => new ReportEntry.Then(x.Title, x.Details.OfType<ReportEntry.Mismatch>().ToArray())).ToArray();
            if (failures.Any())
                throw new ScenarioVerificationException(failures.ToArray());
        }

        internal Exception VerificationException(ReportEntry.Then[] failures)
        {
            return new ScenarioVerificationException(failures);
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            await SafeVerify("DisposeAsync");
        }

        protected abstract Task Verify();
        protected abstract Task Initialize();

        public string Title { get; set; }
    }
}