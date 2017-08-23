using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xunit.ScenarioReporting.Tests
{
    class TestScenario : Scenario
    {
        public TestScenario()
        {
            Givens = new List<Scenario.ReportEntry.Given>();
            Thens = new List<Scenario.ReportEntry.Then>();
        }
        public List<Scenario.ReportEntry.Given> Givens { get; }
        public Scenario.ReportEntry.When When { get; set; }
        public List<Scenario.ReportEntry.Then> Thens { get; }
        internal override IReadOnlyList<Scenario.ReportEntry.Given> ReportGivens()
        {
            return Givens;
        }

        internal override Scenario.ReportEntry.When ReportWhen()
        {
            return When;
        }

        internal override IReadOnlyList<Scenario.ReportEntry.Then> ReportThens()
        {
            return Thens;
        }

        protected override Task Verify()
        {
            return Task.CompletedTask;
        }

        protected override Task Initialize()
        {
            return Task.CompletedTask;
        }
    }
}