using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Xunit.ScenarioReporting
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TGiven"></typeparam>
    /// <typeparam name="TWhen"></typeparam>
    /// <typeparam name="TThen"></typeparam>
    public abstract class ReflectionBasedScenario<TGiven, TWhen, TThen> : Scenario
    {
        /// <summary>
        /// Provides access to the scenario definition. The preferred way of builidng this is to use the <see cref="ReflectionBasedScenarioExtensions.Define{TGiven,TWhen,TThen}"/>
        /// method which provids a fluent builder for defining the current scenario.
        /// </summary>
        protected internal ScenarioDefinition Definition { protected get; set; }
        private IReadOnlyList<TThen> _actuals;
        private List<ReportEntry.Then> _thens;
        /// <summary>
        /// Defines the current scenario.
        /// </summary>
        protected internal class ScenarioDefinition
        {
            private ScenarioDefinition(IReadOnlyList<TGiven> given, TWhen when, IReadOnlyList<TThen> then)
            {
                Given = given;
                When = when;
                Then = then;
            }
            /// <summary>
            /// The givens for the current scenario.
            /// </summary>
            public IReadOnlyList<TGiven> Given { get; }

            /// <summary>
            /// The when for the current scenario
            /// </summary>
            public TWhen When { get; }

            /// <summary>
            /// The expected results from the current scenario.
            /// </summary>
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

        /// <summary>
        /// Initializes the scenario by executing the steps but does not perform verification
        /// </summary>
        /// <returns>A <see cref="Task"/> that completes when the Givens and the whene have been applied.</returns>
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

        /// <summary>
        /// Performs the verification that the actual values collected match the expected Then values
        /// </summary>
        /// <returns>A task that completes after all expected values have been matched to actual values with none remaining</returns>
        /// <exception cref="ScenarioVerificationException">Thrown after verification completes if the scenario verification failed.</exception>
        protected sealed override async Task Verify()
        {
            await Verify(Definition.Then, _actuals);
        }
        /// <summary>
        /// Allows the ignoring of properties for reporting and comparison
        /// </summary>
        /// <remarks>
        /// A string of "Id" will ignore the property Id on all object instances.
        /// A string of "SomeClassName.Id" will ignore the property Id on instances of the SomeClassName type.
        /// The ignored properties will be a union of ignored on the type and ignored on all. There is no concept of inheritance
        /// in the ignored properties so specifying an abstract or base class name in the string will not have any effect.
        /// </remarks>
        protected virtual IReadOnlyList<string> IgnoredProperties => new string[] { };

        /// <summary>
        /// A custom set of comparers used when verifying the scenario. Can be used to provide non default comparers.
        /// </summary>
        protected virtual IReadOnlyDictionary<Type, IEqualityComparer> Comparers => new Dictionary<Type, IEqualityComparer>();
        private Dictionary<string, HashSet<string>> _ignoredProperties;

        private HashSet<string> IgnoredByType(Type type)
        {
            void EnsureInitialized()
            {
                if (_ignoredProperties == null)
                {
                    var ignoredByType = new Dictionary<string, HashSet<string>>()
                    {
                        ["*"] = new HashSet<string>()
                    };

                    foreach (var ignored in IgnoredProperties)
                    {

                        //TODO: ignore hierarchies or support them? maybe better to use json.net and paths to remove entries we don't like
                        string[] split;
                        if (ignored.Contains("."))
                            split = ignored.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
                        else
                            split = new[] {"*", ignored};
                        HashSet<string> properties;
                        if (!ignoredByType.TryGetValue(split[0], out properties))
                        {
                            properties = new HashSet<string>();
                            ignoredByType[split[0]] = properties;
                        }
                        properties.Add(split[1]);
                    }
                    _ignoredProperties = ignoredByType;
                }
            }

            HashSet<string> Filter()
            {
                HashSet<string> ignored;
                if (!_ignoredProperties.TryGetValue(type.Name, out ignored)) ignored = new HashSet<string>();
                ignored.UnionWith(_ignoredProperties["*"]);
                return ignored;
            }

            EnsureInitialized();
            return Filter();
        }
        internal virtual Task<IReadOnlyList<ReportEntry.Then>> Verify(IReadOnlyList<TThen> expected, IReadOnlyList<TThen> actual)
        {
            var maxIterations = Math.Min(expected.Count, actual.Count);
            
            
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
                HashSet<string> ignored = IgnoredByType(e.GetType());
                
                var detail = new List<ReportEntry.Detail>();
                foreach (var p in e.GetType().GetProperties())
                {
                    if(ignored.Contains(p.Name)) continue;
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

        private IReadOnlyList<ReportEntry.Detail> DetailFromProperties(object instance, Type type)
        {
            var ignored = IgnoredByType(type);
            return type.GetProperties().Where(p => !ignored.Contains(p.Name)).Select(p => new ReportEntry.Detail(p.Name, p.GetValue(instance))).ToArray();
        }

        private T Report<T>(object instance, Func<string, IReadOnlyList<ReportEntry.Detail>, T> create)
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

    /// <summary>
    /// Fluent interface for defining a <see cref="Scenario"/>
    /// </summary>
    /// <typeparam name="TG">The type of the Givens</typeparam>
    /// <typeparam name="TW">The type of the When</typeparam>
    /// <typeparam name="TT">The type of the Thens</typeparam>
    public interface IDefine<in TG, in TW, in TT>
    {
        /// <summary>
        /// Specify the givens for the scenario
        /// </summary>
        /// <param name="givens"></param>
        /// <returns>An interface to specify the when</returns>
        IGiven<TW, TT> Given(params TG[] givens);
    }
    /// <summary>
    /// Fluent interface for defining a <see cref="Scenario"/>
    /// </summary>
    /// <typeparam name="TW">The type of the When</typeparam>
    /// <typeparam name="TT">The type of the Thens</typeparam>
    public interface IGiven<in TW, in TT>
    {
        /// <summary>
        /// Specify the when of the scenario
        /// </summary>
        /// <param name="when">The when of the scenario</param>
        /// <returns>An interface for defining the Thens</returns>
        IWhen<TT> When(TW when);
    }
    /// <summary>
    /// Fluent interface for defining a <see cref="Scenario"/>
    /// </summary>
    /// <typeparam name="TT">The type of the Thens</typeparam>
    public interface IWhen<in TT>
    {
        /// <summary>
        /// Specify the thens of the scenario
        /// </summary>
        /// <param name="then">The thens of the scenario</param>
        void Then(params TT[] then);
    }
}