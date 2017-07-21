using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ScenarioReportingTests.ExampleDomain;
using Xunit;
using Xunit.ScenarioReporting;

[assembly: TestFramework("Xunit.ScenarioReporting.ScenarioReportingXunitTestFramework", "Xunit.ScenarioReporting")]

namespace ScenarioReportingTests
{
    public class ExampleScenario : ReflectionBasedScenario<object, object, object>
    {
        public ExampleScenario()
        {
            _aggregate = new CalculatorAggregate();
        }

        private readonly CalculatorAggregate _aggregate;
        private ComputedResult _actual;

        protected override Task Given(IReadOnlyList<object> givens)
        {
            foreach (Number given in givens)
            {
                _aggregate.Enter(given);
            }
            return Task.CompletedTask;
        }

        protected override Task When(object when)
        {
            _actual = _aggregate.Compute((Operation)when);
            return Task.CompletedTask;
        }

        protected override Task<IReadOnlyList<object>> ActualResults()
        {
            return Task.FromResult((IReadOnlyList<object>)new []{_actual});
        }

        protected override Task<Definition> Define()
        {
            return Task.FromResult(Definition.Define(new Number(3), new Number(5)).When(new Operation(OperationType.Add)).Then(new ComputedResult(8)));
        }

        public bool OtherInvariant => true;


    }
}
