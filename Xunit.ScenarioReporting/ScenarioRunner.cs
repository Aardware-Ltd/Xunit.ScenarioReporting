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

        readonly List<ReportEntry> _entries;
        private List<ReportEntry> _currentGroup;

        /// <summary>
        /// Initializes a new instance of the scenario runner
        /// </summary>
        protected ScenarioRunner()
        {
            _entries = new List<ReportEntry>();
        }

        internal void AddResult(string scope, string name, Exception e = null)
        {
            Add(e != null ? new Assertion(scope, name, e) : new Assertion(scope, name));
        }

        internal void Add(ReportEntry entry)
        {
            _currentGroup.Add(entry);
        }

    internal void StartGroup(string name)
        {
            _entries.Add(new ReportEntryGroup(name, _currentGroup = new List<ReportEntry>()));
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
                _result = new ScenarioRunResult(Title, _entries, _error) {Scope = Scope, Grouping = Group};
            }
        }

        internal async Task Complete(ScenarioReport report)
        {
            await ExecuteWithoutThrowing();
            report.Report(_result);
            _result.ThrowIfErrored();
        }
        
        static readonly ReportEntry NullWhen = new ReportEntry("None", new Detail[]{new MissingStep("When"), });
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