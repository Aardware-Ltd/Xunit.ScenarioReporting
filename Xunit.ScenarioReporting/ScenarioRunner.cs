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
        protected internal interface IAddDetail
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
        protected internal interface IAddThenDetail
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

        internal void Add(Given given)
        {
            _givens.Add(given);
        }

        internal void Add(When when)
        {
            _when = when;
        }

        internal void Add(Then then)
        {
            _thens.Add(then);
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
                var subDetails = new DetailAccumulator();
                Details.Add(new Detail(name, value, format, formatter));
                
                return subDetails;
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
        /// <summary>
        /// The scope of the scenario. This is determined by where the scenario runner is created, Test method, Class Fixture or Collection Fixture.
        /// </summary>
        public string Scope { get; internal set; }
    }
}