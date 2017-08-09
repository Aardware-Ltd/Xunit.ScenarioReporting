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
        readonly List<Then> _results = new List<Then>();

        internal void AddResult(string name, Exception e = null)
        {
            if(e != null)
                _results.Add(new Assertion(name, e));
            else _results.Add(new Assertion(name));
        }

        internal IReadOnlyList<Given> GetGivens() => ReportGivens();
        internal When GetWhen() => ReportWhen();
        internal IReadOnlyList<Then> GetThens() => ReportThens().Concat(_results).ToList();

        protected abstract IReadOnlyList<Given> ReportGivens();
        protected abstract When ReportWhen();
        protected abstract IReadOnlyList<Then> ReportThens();


        protected internal class ReportEntry : ReportItem
        {
            public ReportEntry(string title, IReadOnlyList<Detail> details)
            {
                Title = title;
                Details = details;
            }

            public string Title { get; }
            public IReadOnlyList<Detail> Details { get; }
        }

        protected internal class Given : ReportEntry
        {
            public Given(string title, IReadOnlyList<Detail> details) : base(title, details)
            {
            }
        }

        protected internal class When : ReportEntry
        {
            public When(string title, IReadOnlyList<Detail> details) : base(title, details)
            {
            }
        }

        protected internal class Then : ReportEntry
        {
            public Then(string title, IReadOnlyList<Detail> details) : base(title, details)
            {

            }
        }

        protected internal class Assertion : Then
        {
            public Assertion(string title) : base(title, new Detail[] { })
            {
            }

            public Assertion(string title, Exception ex) : base(title, new[] { ExceptionToDetail(ex) }) { }

            static Detail ExceptionToDetail(Exception ex)
            {
                return new Failure(ex.Message);
            }
        }

        protected internal class Failure : Detail
        {
            public Failure(string name) : base(name, null)
            {

            }
        }
        protected internal class Match : Detail
        {
            public Match(string name, object value, string format = null, Func<object, string> formatter = null) : base(name, value, format, formatter)
            {
            }
        }

        protected internal class Mismatch : Detail
        {
            public object Actual { get; }

            public Mismatch(string name, object expected, object actual, string format = null, Func<object, string> formatter = null) : base(name, expected, format, formatter)
            {
                Actual = actual;
            }
        }

        protected internal class Detail
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

        internal async Task SafeVerify()
        {
            if (_verified || _errored) return;
            await Initialize();
            _verified = true;
            await Verify();
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            await SafeVerify();
        }

        protected abstract Task Verify();
        protected abstract Task Initialize();

        public string Title { get; set; }
    }
}