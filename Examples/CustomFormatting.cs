using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.ScenarioReporting;

namespace Examples
{
    public class CustomFormatting
    {
        private static readonly object[] Thens = new object[]
        {
            new ClassWithDecimal(4.1457894m),
            new ClassWithGuid(Guid.NewGuid()),
            new ClassWithCustomStruct(DateTime.UtcNow, 79874641.78678f)
        };
        [Fact]
        public Task<ScenarioRunResult> ScenarioCustomFormatStrings()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task RunnerCustomFormatStrings()
        {
            await new ScenarioWithCustomFormatStrings().Run(
                def => def.Given().When(new object()).Then(Thens));
        }

        [Fact]
        public Task<ScenarioRunResult> ScenarioCustomFormatters()
        {
            throw new NotImplementedException();
        }


        [Fact]
        public Task<ScenarioRunResult> RunnerCustomFormatters()
        {
            throw new NotImplementedException();
        }

        class BaseScenario : ReflectionBasedScenarioRunner<object, object, object>
        {
            protected override Task Given(IReadOnlyList<object> givens)
            {
                return Task.CompletedTask;
            }

            protected override Task When(object when)
            {
                return Task.CompletedTask;
            }

            protected override Task<IReadOnlyList<object>> ActualResults()
            {
                return Task.FromResult(Definition.Then);
            }
        }
        class ScenarioWithCustomFormatStrings : BaseScenario
        {
            public ScenarioWithCustomFormatStrings()
            {
                AddFormatString<decimal>("{0:C2}");
                AddFormatString<Guid>("{0:N}");
                AddFormatString<DateTime>("{0:g}");
                AddFormatString<double>("{0:0,0}");
            }
        }

        class ClassWithDecimal
        {
            public decimal Value { get; }

            public ClassWithDecimal(decimal value)
            {
                Value = value;
            }
        }

        class ClassWithGuid
        {
            public ClassWithGuid(Guid value)
            {
                Value = value;
            }

            public Guid Value { get; }
        }

        class ClassWithCustomStruct
        {
            public CustomStruct Value { get; }

            public ClassWithCustomStruct(DateTime timestamp, double value)
            {
                Value = new CustomStruct(timestamp, value);
            }
        }

        struct CustomStruct : IEquatable<CustomStruct>
        {
            public readonly DateTime Timestamp;
            public readonly double Value;
            public CustomStruct(DateTime timestamp, double value)
            {
                Timestamp = timestamp;
                Value = value;
            }

            public bool Equals(CustomStruct other)
            {
                return true;
            }            
        }
}

    //public class CustomEqualityChecks
    //{
    //    public Task<ScenarioRunResult> DoubleCheckWithEpsilon()
    //    {
            
    //    }

    //    public Task<ScenarioRunResult> CustomEqualityComparer()
    //    {
            
    //    }
    //}

}
