using System.Collections.Generic;
using System.Threading.Tasks;
using Examples.ExampleDomain;
using Xunit.ScenarioReporting;

namespace Examples
{
    public class ExampleScenario : ReflectionBasedScenario<object, object, object>
    {
        public ExampleScenario()
        {
            _aggregate = new CalculatorAggregate();
            this.Define(def => def.Given(new Number(3), new Number(5)).When(new Operation(OperationType.Add)).Then(new ComputedResult(8)));
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
        
        public bool OtherInvariant => true;


    }
}
