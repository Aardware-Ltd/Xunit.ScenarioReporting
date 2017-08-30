using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Examples.ExampleDomain;
using Xunit;
using Xunit.ScenarioReporting;

namespace Examples
{
    public class ClassScenarioFixtureExample : IClassFixture<ClassScenarioFixtureExample.ExampleScenarioRunnerWithState>, IAsyncLifetime
    {
        private readonly ExampleScenarioRunnerWithState _scenarioRunner;
        private CalculatorWithState _calculator;

        public ClassScenarioFixtureExample(ExampleScenarioRunnerWithState scenarioRunner)
        {
            _scenarioRunner = scenarioRunner;
        }


        public async Task InitializeAsync()
        {
            _calculator = await _scenarioRunner.Run(def => def.Given(10, 32).When(new Operation(OperationType.Add)).Then());
        }

        [Fact]
        public void NotZero()
        {
            Assert.NotEqual(0, _calculator.Result);
        }

        [Fact]
        public void NotMoreThan50()
        {
            Assert.True(_calculator.Result < 50);
        }

        [Fact]
        public void IsFortyTwo()
        {
            Assert.Equal(42, _calculator.Result);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public class ExampleScenarioRunnerWithState : ReflectionBasedScenarioRunner<int, object, object,
            CalculatorWithState>
        {
            private readonly CalculatorWithState _calculator;

            public ExampleScenarioRunnerWithState()
            {
                 _calculator = new CalculatorWithState();
            }
            protected override Task Given(IReadOnlyList<int> givens)
            {
                foreach (int given in givens)
                {
                    _calculator.Enter(given);
                }
                return Task.CompletedTask;
            }

            protected override Task When(object when)
            {
                _calculator.Add();
                return Task.CompletedTask;
            }

            protected override Task<IReadOnlyList<object>> ActualResults()
            {
                return Task.FromResult((IReadOnlyList<object>)new object[] { });
            }

            protected override Task<CalculatorWithState> AcquireState()
            {
                return Task.FromResult(_calculator);
            }
        }

        public class CalculatorWithState
        {
            List<int> _numbers = new List<int>();

            public void Enter(int number)
            {
                _numbers.Add(number);
            }

            public void Add()
            {
                Result = _numbers.Sum();
            }

            public int Result { get; private set; }
        }
    }
}