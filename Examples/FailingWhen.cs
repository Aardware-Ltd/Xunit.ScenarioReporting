using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.ScenarioReporting;

namespace Examples
{
    public class FailingWhen
    {
        [Fact]
        public Task<ScenarioRunResult> ShouldFail()
        {
            return new TestRunner().Run(def => def.Given().When(new object()).Then());
        }

        class TestRunner : ReflectionBasedScenarioRunner<object, object, object>
        {
            protected override Task Given(IReadOnlyList<object> givens)
            {
                return Task.CompletedTask;
            }

            protected override Task When(object when)
            {
                throw new NotImplementedException();
            }

            protected override Task<IReadOnlyList<object>> ActualResults()
            {
                throw new NotImplementedException();
            }
        }
    }
}
