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
        
        internal async Task Execute()
        {
            await ExecuteWithoutThrowing();
            _result.ThrowIfErrored();
        }

        private async Task ExecuteWithoutThrowing()
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
                _result = new ScenarioRunResult(Title, _givens, _when ?? NullWhen, _thens, _error) {Scope = Scope, Grouping = Group};
            }
        }

        internal async Task Complete(ScenarioReport report)
        {
            await ExecuteWithoutThrowing();
	        if (report == null)
	        {
		        Trace.WriteLine($"no report found, check that the test assembly has been set in the assembly: \r\n\t[assembly: TestFramework(Xunit.ScenarioReporting.Constants.Framework, Xunit.ScenarioReporting.Constants.AssemblyName)] ");
	        }
	        report?.Report(_result);
            _result.ThrowIfErrored();
        }
        static readonly When NullWhen = new When("No when provided", new Detail[]{});
        private ExceptionDispatchInfo _error;
        private ScenarioRunResult _result;

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
        protected internal string Title { get; set; }
        /// <summary>
        /// The scope of the scenario. This is determined by where the scenario runner is created, Test method, Class Fixture or Collection Fixture.
        /// </summary>
        protected internal string Scope { get; internal set; }
        protected internal string Group { get; internal set; }
        internal bool DelayReporting { get; set; }
    }
}