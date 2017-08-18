using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Xunit.ScenarioReporting
{
    public static class ReflectionBasedScenarioExtensions
    {
        public static Scenario Define<TGiven, TWhen, TThen>(this ReflectionBasedScenario<TGiven, TWhen, TThen> scenario, Action<IDefine<TGiven, TWhen, TThen>> define) 
        {
            var builder = ReflectionBasedScenario<TGiven, TWhen, TThen>.ScenarioDefinition.Builder;
            define(builder);
            scenario.Definition = builder.Build();
            return scenario;
        }
    }
    public abstract class ReflectionBasedScenario<TGiven, TWhen, TThen> : Scenario
    {
        protected internal ScenarioDefinition Definition { protected get; set; }
        private IReadOnlyList<TThen> _actuals;
        private List<ReportEntry.Then> _thens;
        
        protected internal class ScenarioDefinition
        {
            private ScenarioDefinition(IReadOnlyList<TGiven> given, TWhen when, IReadOnlyList<TThen> then)
            {
                Given = given;
                When = when;
                Then = then;
            }
            public IReadOnlyList<TGiven> Given { get; }

            public TWhen When { get; }

            public IReadOnlyList<TThen> Then { get; }

            internal static DefinitionBuilder Builder = new DefinitionBuilder();

            internal class DefinitionBuilder : IDefine<TGiven, TWhen, TThen>, IGiven<TWhen, TThen>, IWhen<TThen>
            {
                private ScenarioDefinition _scenarioDefinition;
                private IReadOnlyList<TGiven> _givens;
                private TWhen _when;
                
                public IGiven<TWhen, TThen> Given(params TGiven[] givens)
                {
                    _givens = givens;
                    return this;
                }

                IWhen<TThen> IGiven<TWhen, TThen>.When(TWhen when)
                {
                    _when = when;
                    return this;
                }

                void IWhen<TThen>.Then(params TThen[] then)
                {
                    _scenarioDefinition = new ScenarioDefinition(_givens, _when, then);
                }

                internal ScenarioDefinition Build()
                {
                    return _scenarioDefinition;
                }
            }
            
        }

        protected sealed override async Task Initialize()
        {
            _thens = new List<ReportEntry.Then>();
            Definition = await Define();
            if(Definition == null) 
                throw new InvalidOperationException("Definition is undefined");
            await Given(Definition.Given);
            await When(Definition.When);
            _actuals = await ActualResults();
        }

        protected abstract Task Given(IReadOnlyList<TGiven> givens);
        protected abstract Task When(TWhen when);
        protected abstract Task<IReadOnlyList<TThen>> ActualResults();

        protected virtual Task<ScenarioDefinition> Define()
        {
            return Task.FromResult(Definition);
        }

        protected sealed override async Task Verify()
        {
            await Verify(Definition.Then, _actuals);
        }

        protected virtual IReadOnlyList<string> IgnoredProperties => new string[] { };
        protected virtual IReadOnlyDictionary<Type, IEqualityComparer> Comparers => new Dictionary<Type, IEqualityComparer>();

        internal virtual Task<IReadOnlyList<ReportEntry.Then>> Verify(IReadOnlyList<TThen> expected, IReadOnlyList<TThen> actual)
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
            
            for (int i = 0; i < maxIterations; i++)
            {
                var e = expected[i];
                var a = actual[i];
                if (a.GetType() != e.GetType())
                {
                    //fail and skip
                    _thens.Add(new ReportEntry.Then(e.GetType().Name, new ReportEntry.Detail[]{new ReportEntry.Mismatch("Type", e, a, formatter: Formatters.FromClassName), }));
                    continue;
                }
                HashSet<string> ignored;
                if(!ignoredByType.TryGetValue(e.GetType().Name, out ignored)) ignored = new HashSet<string>();
                var detail = new List<ReportEntry.Detail>();
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
                        detail.Add(new ReportEntry.Mismatch(p.Name, ev, av));
                    }
                    else
                    {
                        detail.Add(new ReportEntry.Match(p.Name, ev));
                    }
                }
                _thens.Add(new ReportEntry.Then(e.GetType().Name, detail));
            }
            if (expected.Count > actual.Count)
            {
                for (int i = Math.Max(0, actual.Count - 1); i < expected.Count; i++)
                {
                    _thens.Add(new ReportEntry.Then("Missing expected results", new ReportEntry.Detail[] { new ReportEntry.Mismatch("Type", expected[i], null, formatter: Formatters.FromClassName), }));
                }
            }
            if (actual.Count > expected.Count)
            {
                for (int i = Math.Max(0, expected.Count - 1); i < actual.Count; i++)
                {
                    _thens.Add(new ReportEntry.Then("More results than expected", new ReportEntry.Detail[] { new ReportEntry.Mismatch("Type", null, actual[i], formatter: Formatters.FromClassName), }));
                }
            }

            return Task.FromResult((IReadOnlyList<ReportEntry.Then>)_thens);
        }

        internal override IReadOnlyList<ReportEntry.Given> ReportGivens()
        {
            return Definition.Given.Select(x => Report(x, (n, d) => new ReportEntry.Given(n, d))).ToArray();
        }

        private static IReadOnlyList<ReportEntry.Detail> DetailFromProperties(object instance, Type type)
        {
            return type.GetProperties().Select(p => new ReportEntry.Detail(p.Name, p.GetValue(instance))).ToArray();
        }

        private static T Report<T>(object instance, Func<string, IReadOnlyList<ReportEntry.Detail>, T> create)
        {
            var type = instance.GetType();
            return create(type.Name, DetailFromProperties(instance, type));
        }

        internal override ReportEntry.When ReportWhen()
        {
            return Report(Definition.When, (n, d) => new ReportEntry.When(n, d));
        }

        internal override IReadOnlyList<ReportEntry.Then> ReportThens()
        {
            return _thens;
        }
    }

    public interface IDefine<TG, TW, TT>
    {
        IGiven<TW, TT> Given(params TG[] givens);
    }

    public interface IGiven<TW, TT>
    {
        IWhen<TT> When(TW when);
    }

    public interface IWhen<TT>
    {
        void Then(params TT[] then);
    }
}