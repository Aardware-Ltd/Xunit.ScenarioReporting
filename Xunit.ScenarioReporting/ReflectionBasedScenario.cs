using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Xunit.ScenarioReporting
{
    public abstract class ReflectionBasedScenario<TGiven, TWhen, TThen> : Scenario
    {
        private Definition _definition;
        private IReadOnlyList<TThen> _actuals;
        private IReadOnlyList<Then> _thens;
        
        protected class Definition
        {
            private Definition(IReadOnlyList<TGiven> given, TWhen when, IReadOnlyList<TThen> then)
            {
                Given = given;
                When = when;
                Then = then;
            }
            public IReadOnlyList<TGiven> Given { get; }

            public TWhen When { get; }

            public IReadOnlyList<TThen> Then { get; }

            public static IGiven Define(params TGiven[] givens)
            {
                return new DefinitionBuilder(givens);
            }

            class DefinitionBuilder : IGiven, IWhen
            {
                private readonly IReadOnlyList<TGiven> _givens;
                private TWhen _when;
                public DefinitionBuilder(IReadOnlyList<TGiven> givens)
                {
                    _givens = givens;
                }
                IWhen IGiven.When(TWhen when)
                {
                    _when = when;
                    return this;
                }

                Definition IWhen.Then(params TThen[] thens)
                {
                    return new Definition(_givens, _when, thens);
                }
            }

            public interface IGiven
            {
                IWhen When(TWhen when);
            }

            public interface IWhen
            {
                Definition Then(params TThen[] thens);
            }
        }

        protected sealed override async Task Initialize()
        {
            _definition = await Define();
            await Given(_definition.Given);
            await When(_definition.When);
            _actuals = await ActualResults();
        }

        protected abstract Task Given(IReadOnlyList<TGiven> givens);
        protected abstract Task When(TWhen when);
        protected abstract Task<IReadOnlyList<TThen>> ActualResults();
        protected abstract Task<Definition> Define();
        protected sealed override async Task Verify()
        {
            _thens = await Verify(_definition.Then, _actuals);
        }

        protected virtual IReadOnlyList<string> IgnoredProperties => new string[] { };
        protected virtual IReadOnlyDictionary<Type, IEqualityComparer> Comparers => new Dictionary<Type, IEqualityComparer>();

        protected virtual Task<IReadOnlyList<Then>> Verify(IReadOnlyList<TThen> expected, IReadOnlyList<TThen> actual)
        {
            var maxIterations = Math.Min(expected.Count, actual.Count);
            var ignoredByType = new Dictionary<string, HashSet<string>>();
            
            foreach (var ignored in IgnoredProperties)
            {
                //TODO: ignore hierarchies or support them? maybe better to use json.net and paths to remove entries we don't like
                var split = ignored.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);

                HashSet<string> properties;
                if (!ignoredByType.TryGetValue(split[0], out properties))
                {
                    properties = new HashSet<string>();
                    ignoredByType[split[0]] = properties;
                }
                properties.Add(split[1]);
            }
            var thens = new List<Then>();
            for (int i = 0; i < maxIterations; i++)
            {
                var e = expected[i];
                var a = actual[i];
                if (a.GetType() != e.GetType())
                {
                    //fail and skip
                    thens.Add(new Then(e.GetType().Name, new Detail[]{new Mismatch("Type", e, a, formatter: Formatters.FromClassName), }));
                    continue;
                }
                HashSet<string> ignored;
                if(!ignoredByType.TryGetValue(e.GetType().Name, out ignored)) ignored = new HashSet<string>();
                var detail = new List<Detail>();
                foreach (var p in e.GetType().GetProperties())
                {
                    var ev = p.GetValue(e);
                    var av = p.GetValue(a);
                    IEqualityComparer comparer;
                    if (!Comparers.TryGetValue(p.PropertyType, out comparer))
                    {
                        comparer = (IEqualityComparer) typeof(EqualityComparer<>).MakeGenericType(p.PropertyType)
                            .GetProperty("Default", BindingFlags.Static|BindingFlags.Public).GetValue(null);
                    }
                    if (!comparer.Equals(ev, av))
                    {
                        //fail and continue
                        detail.Add(new Mismatch(p.Name, ev, av));
                    }
                    else
                    {
                        detail.Add(new Match(p.Name, ev));
                    }
                }
                thens.Add(new Then(e.GetType().Name, detail));
            }
            if (expected.Count > actual.Count)
            {
                for (int i = actual.Count - 1; i < expected.Count; i++)
                {
                    thens.Add(new Then("Missing expected results", new Detail[] { new Mismatch("Type", expected[i], null, formatter: Formatters.FromClassName), }));
                }
            }
            if (actual.Count > expected.Count)
            {
                for (int i = expected.Count - 1; i < actual.Count; i++)
                {
                    thens.Add(new Then("More results than expected", new Detail[] { new Mismatch("Type", null, actual[i], formatter: Formatters.FromClassName), }));
                }
            }
            return Task.FromResult((IReadOnlyList<Then>)thens);
        }

        protected override IReadOnlyList<Given> ReportGivens()
        {
            return _definition.Given.Select(x => Report(x, (n, d) => new Given(n, d))).ToArray();
        }

        private static IReadOnlyList<Detail> DetailFromProperties(object instance, Type type)
        {
            return type.GetProperties().Select(p => new Detail(p.Name, p.GetValue(instance))).ToArray();
        }

        private static T Report<T>(object instance, Func<string, IReadOnlyList<Detail>, T> create)
        {
            var type = instance.GetType();
            return create(type.Name, DetailFromProperties(instance, type));
        }

        protected override When ReportWhen()
        {
            return Report(_definition.When, (n, d) => new When(n, d));
        }

        protected override IReadOnlyList<Then> ReportThens()
        {
            return _thens;
        }
    }
}