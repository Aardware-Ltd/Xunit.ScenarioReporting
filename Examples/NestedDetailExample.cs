using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.ScenarioReporting;

namespace Examples
{
    public class NestedDetailExample
    {
        [Fact]
        public Task<ScenarioRunResult> Nested()
        {
            var expected = new NestedThen("Party at", DateTimeOffset.Now.AddDays(2), new AdditionalThenDetails("Bring food"));
            return new Runner(expected).Run(def =>
                def.Given(new NestedGiven("Test Given", 42, new AdditionalGivenDetails("Test nested given")))
                .When(new NestedWhen("Test When", DateTime.Now, new AdditionalWhenDetails("Additional")))
                .Then(expected));
        }

        class Runner : ReflectionBasedScenarioRunner<object, object, object> {
            private readonly IReadOnlyList<object> _actual;

            public Runner(params object[] actual)
            {
                _actual = actual;
            }
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
                return Task.FromResult(_actual);
            }
        }

        class NestedGiven
        {
            public string Text { get; }
            public int Number { get; }
            public AdditionalGivenDetails Nested { get; }

            public NestedGiven(string text, int number, AdditionalGivenDetails nested)
            {
                Text = text;
                Number = number;
                Nested = nested;
            }
        }

        class AdditionalGivenDetails
        {
            public string Text { get; }

            public AdditionalGivenDetails(string text)
            {
                Text = text;
            }
        }

        class NestedWhen
        {
            public string Text { get; }
            public DateTime Date { get; }
            public AdditionalWhenDetails Additonal { get; }

            public NestedWhen(string text, DateTime date, AdditionalWhenDetails additonal)
            {
                Text = text;
                Date = date;
                Additonal = additonal;
            }
        }

        class AdditionalWhenDetails
        {
            public string Fluff { get; }

            public AdditionalWhenDetails(string fluff)
            {
                Fluff = fluff;
            }
        }

        class NestedThen
        {
            public string Text { get; }
            public DateTimeOffset When { get; }
            public AdditionalThenDetails Request { get; }

            public NestedThen(string text, DateTimeOffset when, AdditionalThenDetails request)
            {
                Text = text;
                When = when;
                Request = request;
            }
        }

        class AdditionalThenDetails
        {
            public string Text { get; }

            public AdditionalThenDetails(string text)
            {
                Text = text;
            }
        }
    }
}
