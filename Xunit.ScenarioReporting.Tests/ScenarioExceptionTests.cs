using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xunit.ScenarioReporting.Tests
{
    public class ScenarioExceptionTests
    {
        [Fact]
        public async Task ScenarioWithoutDefinitionShouldThrowOnVerify()
        {
            var runner = new TestScenarioRunnerWithState();
            var result = await runner.Result();
            var ex = Record.Exception(() => result.ThrowIfErrored());
            var ioe = Assert.IsType<InvalidOperationException>(ex);
            Assert.Equal(Constants.Errors.ScenarioNotDefined, ioe.Message);
        }

        [Fact]
        public async Task ScenarioThatThrowsExpectedExceptionShouldSucceed()
        {
            var runner = new TestScenarioRunnerWithState();
            await runner.Run(def => def.Given().When(new InvalidTimeZoneException()).Throws(new InvalidTimeZoneException()));
            var result = await runner.Result();
            var ex = Record.Exception(() => result.ThrowIfErrored());
            Assert.Null(ex);
        }

        [Fact]
        public async Task ScenarioThatThrowsUnexpectedExceptionShouldFail()
        {
            var runner = new TestScenarioRunnerWithState();
            await runner.Run(def => def.Given().When(new InvalidTimeZoneException()).Then());
            var result = await runner.Result();
            var ex = Record.Exception(() => result.ThrowIfErrored());
            Assert.IsType<InvalidTimeZoneException>(ex);
        }

        [Fact]
        public async Task WhenVerificationFailsResultShouldFail()
        {
            var runner = new TestScenarioRunnerWithState();
            await runner.Run(def => def.Given().When(new object()).Then(new object()));
            var result = await runner.Result();
            var ex = Record.Exception(() => result.ThrowIfErrored());
            Assert.IsType<ScenarioVerificationException>(ex);
        }

        class TestScenarioRunnerWithState : ReflectionBasedScenarioRunner<object, object, object, object> {
            private readonly object _state;

            public TestScenarioRunnerWithState()
            {
                _state = null;
            }
            protected override Task Given(IReadOnlyList<object> givens)
            {
                return Task.CompletedTask;
            }

            protected override Task When(object when)
            {
                if (when is Exception ex) throw ex;
                return Task.CompletedTask;
            }

            protected override Task<IReadOnlyList<object>> ActualResults()
            {
                return Task.FromResult((IReadOnlyList<object>)new object[]{});
            }

            protected override Task<object> AcquireState()
            {
                return Task.FromResult(_state);
            }
        }
    }
}
