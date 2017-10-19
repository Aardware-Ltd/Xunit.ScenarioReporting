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
        public async Task RunnerCustomFormatStrings()
        {
            await new Runner()
                .Configure(cfg => cfg
                    .Format<decimal>("{0:C2}")
                    .Format<Guid>("{0:N}")
                    .Format<DateTime>("{0:g}")
                    .Format<double>("{0:0,0}"))
                .Run(
                def => def.Given().When(new object()).Then(Thens));
        }

        [Fact]
        public async Task RunnerCustomFormatters()
        {
            await new Runner()
                .Configure(cfg => cfg
                    .Format<decimal>(d => $"{d:C4}")
                    .Format<Guid>(g =>$"{g:D}")
                    .Format<DateTime>(d=>$"{d:F}")
                    .Format<double>(d =>$"{d:0,0}"))
                .Run(
                    def => def.Given().When(new object()).Then(Thens));
        }

        class Runner : ReflectionBasedScenarioRunner<object, object, object>
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
}
