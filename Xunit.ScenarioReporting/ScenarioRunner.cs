using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Xunit.ScenarioReporting.Results;

namespace Xunit.ScenarioReporting
{
    /// <summary>
    /// Used to specify how to run a scenario definition
    /// </summary>
    public abstract class ScenarioRunner
    {
        private bool _runCompleted;
        
        readonly List<Given> _givens;
        private When _when;
        readonly List<Then> _thens;

        /// <summary>
        /// Initializes a new instance of the scenario runner
        /// </summary>
        protected ScenarioRunner()
        {
            _givens = new List<Given>();
            _thens = new List<Then>();
        }

        internal void AddResult(string scope, string name, Exception e = null)
        {
            _thens.Add(e != null ? new Assertion(scope, name, e) : new Assertion(scope, name));
        }
        
        /// <summary>
        /// Provides an interface for adding details to the Given, When and then items
        /// </summary>
        protected interface IAddDetail
        {
            /// <summary>
            /// Adds a detail to a report item
            /// </summary>
            /// <param name="name">The name of the detail</param>
            /// <param name="value">The value of the detail</param>
            /// <param name="format">The format of the detail, or null if no format is required</param>
            /// <param name="formatter"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentException">Thrown if both <paramref name="format"/> and <paramref name="formatter"/> are supplied</exception>
            IAddDetail Add(string name, object value, string format = null, Func<object, string> formatter = null);
        }

        /// <summary>
        /// Provides an interface for adding details to the Given, When and then items
        /// </summary>
        protected interface IAddThenDetail
        {
            /// <summary>
            /// Adds a mismatch to a report item
            /// </summary>
            /// <param name="name">The name of the detail</param>
            /// <param name="expected">The expected value </param>
            /// <param name="actual">The actual value</param>
            /// <param name="format">The format of the detail, or null if no format is required</param>
            /// <param name="formatter"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentException">Thrown if both <paramref name="format"/> and <paramref name="formatter"/> are supplied</exception>
            IAddThenDetail Mismatch(string name, object expected, object actual, string format = null, Func<object, string> formatter = null);
            /// <summary>
            /// Adds a match to a report item
            /// </summary>
            /// <param name="name">The name of the detail</param>
            /// <param name="expected">The expected value </param>
            /// <param name="format">The format of the detail, or null if no format is required</param>
            /// <param name="formatter"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentException">Thrown if both <paramref name="format"/> and <paramref name="formatter"/> are supplied</exception>
            IAddThenDetail Match(string name, object expected, string format = null, Func<object, string> formatter = null);

        }

        /// <summary>
        /// Records a specified given for writing to the report.
        /// </summary>
        /// <param name="title">The title of the given.</param>
        /// <param name="detailBuilder">A builder method to add details to the Given</param>
        protected void RecordGiven(string title, Action<IAddDetail> detailBuilder)
        {
            var details = BuildDetails(detailBuilder);

            _givens.Add(new Given(title, details));
        }

        /// <summary>
        /// Records the when for writing to the report.
        /// </summary>
        /// <param name="title">The title of the when</param>
        /// <param name="detailBuilder">A builder method to add details to the When</param>
        /// <remarks>If this method is not called, then the scenario run is considered a failure.</remarks>
        protected void RecordWhen(string title, Action<IAddDetail> detailBuilder)
        {
            var details = BuildDetails(detailBuilder);
            _when = new When(title, details);
        }

        /// <summary>
        /// Records a specified then for writing to the report.
        /// </summary>
        /// <param name="title">The titile of the Then</param>
        /// <param name="detailBuilder">A builder method to add details to the Then</param>
        protected void RecordThen(string title, Action<IAddThenDetail> detailBuilder)
        {
            var details = BuildDetails(detailBuilder);
            _thens.Add(new Then(null, title, details));
        }

        private static List<Detail> BuildDetails(Action<IAddDetail> detailBuilder)
        {
            var builder = new DetailAccumulator();
            detailBuilder(builder);
            return builder.Details;
        }

        private static List<Detail> BuildDetails(Action<IAddThenDetail> detailBuilder)
        {
            var builder = new DetailAccumulator();
            detailBuilder(builder);
            return builder.Details;
        }

        class DetailAccumulator : IAddDetail, IAddThenDetail
        {
            public List<Detail> Details { get; }

            public DetailAccumulator()
            {
                Details = new List<Detail>();
            }

            public IAddDetail Add(string name, object value, string format, Func<object, string> formatter)
            {
                if(format != null && formatter != null) throw new ArgumentException("Cannot specify both format and formatter");
                Details.Add(new Detail(name, value, format, formatter));
                return this;
            }

            public IAddThenDetail Mismatch(string name, object expected, object actual, string format = null, Func<object, string> formatter = null)
            {
                if (format != null && formatter != null) throw new ArgumentException("Cannot specify both format and formatter");
                Details.Add(new Mismatch(name,expected, actual, format, formatter));
                return this;
            }

            public IAddThenDetail Match(string name, object expected, string format = null, Func<object, string> formatter = null)
            {
                if (format != null && formatter != null) throw new ArgumentException("Cannot specify both format and formatter");
                Details.Add(new Match(name, expected, format, formatter));
                return this;
            }
        }
        
        internal async Task<ScenarioRunResult> Result()
        {
            if (!_runCompleted)
            {
                _runCompleted = true;
                try
                {
                    await Run();
                }
                catch (Exception ex)
                {
                    _error = ExceptionDispatchInfo.Capture(ex);
                }
                if (_when == null)
                    AddResult(null, "Incomplete scenario", new Exception("No when provided"));
            }
            return new ScenarioRunResult(Title, _givens, _when ?? NullWhen, _thens, _error){Scope = Scope};
        }
        static readonly When NullWhen = new When("No when provided", new Detail[]{});
        private ExceptionDispatchInfo _error;
        
        /// <summary>
        /// Runs the scenario and performs any verification
        /// </summary>
        /// <returns>A task that completes after the scenario has been run and verified</returns>
        protected abstract Task Run();

        /// <summary>
        /// The title of the scenario to use in the report. If no title is specified, the name is taken from the scope of the scenario,
        /// so if the scenario is used in a collectionfixture, the name of the collection fixture, if used in a class fixture, then
        /// the name of the class fixture. if returned from a test, then the name of the test will be used.
        /// </summary>
        public string Title { get; set; }
        public string Scope { get; internal set; }
    }
}