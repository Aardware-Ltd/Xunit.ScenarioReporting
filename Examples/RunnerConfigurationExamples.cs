using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.ScenarioReporting;

namespace Examples
{
    public class RunnerConfigurationExamples
    {
        private readonly Runner _runner;

        public RunnerConfigurationExamples()
        {
            _runner = new Runner()
                .Configure(cfg => cfg
                 .IgnoreAll("Id")
                 .IgnoreAll<Guid>()
                 .ForType<CustomType>(t => t
                    .Hide(i => i.HiddenValue)
                    .Ignore(i => i.IgnoredValue))
                 .Format<DateTime>("")
                 .Format<DateTimeOffset>(d => $"{d.Year}")
                 .CustomReader<CustomReadType>((i,d,name) => new ObjectPropertyDefinition(typeof(CustomReadType), name, d, $"Point({i.X}, {i.Y})", null, null, new ObjectPropertyDefinition[]{}))
                 );
        }

        class CustomType
        {
            public string HiddenValue { get; private set; }
            public string IgnoredValue { get; private set; }
        }

        class CustomReadType
        {
            public int X { get; set; }
            public int Y { get; set; }
        }
        [Fact]
        public Task ConfiguredRunner()
        {
            return Task.CompletedTask;
        }
        class Runner:ReflectionBasedScenarioRunner<object, object, object> {
            protected override Task Given(IReadOnlyList<object> givens)
            {
                throw new NotImplementedException();
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
