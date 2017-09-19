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
            var expected = new NestedThen("Party at", DateTimeOffset.Now.AddDays(2), new AdditionalThenDetails("Bring food", new EvenMoreNestedThen(long.MaxValue, SomeStatus.ClosedForHolidays)));
            return new Runner(expected).Run(def =>
                def.Given(new NestedGiven("Test Given", 42, new AdditionalGivenDetails("Test nested given", new MoreGivenNesting("Really arbitrary name", SomeStatus.Open))))
                .When(new NestedWhen("Test When", DateTime.Now, new AdditionalWhenDetails("Additional", new EvenMoreNestedWhen(TimeSpan.FromDays(8), SomeStatus.Understaffed))))
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
            public MoreGivenNesting EvenMoreAdditions { get; }

            public AdditionalGivenDetails(string text, MoreGivenNesting evenMoreAdditions)
            {
                Text = text;
                EvenMoreAdditions = evenMoreAdditions;
            }
        }

        enum SomeStatus
        {
            Unknown,
            Open,
            Closed,
            Understaffed,
            ClosedForHolidays
        }

        class MoreGivenNesting
        {
            public MoreGivenNesting(string someNestedName, SomeStatus status)
            {
                SomeNestedName = someNestedName;
                Status = status;
            }
            public string SomeNestedName { get; }
            public SomeStatus Status { get; }
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
            public EvenMoreNestedWhen MoreThings { get; }

            public AdditionalWhenDetails(string fluff, EvenMoreNestedWhen moreThings)
            {
                Fluff = fluff;
                MoreThings = moreThings;
            }
        }

        class EvenMoreNestedWhen
        {
            public EvenMoreNestedWhen(TimeSpan period, SomeStatus status)
            {
                Period = period;
                Status = status;
            }

            public TimeSpan Period { get; }
            public SomeStatus Status { get; }
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
            public EvenMoreNestedThen AdditionalDetails { get; }

            public AdditionalThenDetails(string text, EvenMoreNestedThen additionalDetails)
            {
                Text = text;
                AdditionalDetails = additionalDetails;
            }
        }

        class EvenMoreNestedThen
        {
            public EvenMoreNestedThen(long biggerNumber, SomeStatus status)
            {
                BiggerNumber = biggerNumber;
                Status = status;
            }

            public long BiggerNumber { get; }
            public SomeStatus Status { get; }
        }
    }
}
