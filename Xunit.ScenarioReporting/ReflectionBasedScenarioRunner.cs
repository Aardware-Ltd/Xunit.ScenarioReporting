using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Xunit.ScenarioReporting
{
    /// <summary>
    /// Generates reports using reflection to pull the details from the definition. Applies the Given and When from
    /// the definition to the subject under test and then verifies the results using reflection.
    /// </summary>
    /// <typeparam name="TGiven">The base class for the Given types</typeparam>
    /// <typeparam name="TWhen">The base class for the When type</typeparam>
    /// <typeparam name="TThen">The base class for the Then types</typeparam>
    /// <remarks>This scenario type should cover most requirements for scenarios. Inherit from this class to define how
    /// to use the Given/When/Then
    /// Generally you can provide a single inherited version of this class to run multiple scenarioRunner definitions</remarks>
    /// <inheritdoc />
    public abstract class ReflectionBasedScenarioRunner<TGiven, TWhen, TThen> : ScenarioRunner
    {
        /// <summary>
        /// Provides access to the scenarioRunner definition. The preferred way of builidng this is to use the <see cref="ReflectionBasedScenarioExtensions.Run{TGiven,TWhen,TThen}"/>
        /// method which provids a fluent builder for defining the current scenarioRunner.
        /// </summary>
        protected internal ScenarioDefinition Definition { protected get; set; }
        
        /// <summary>
        /// Provides access to the exception that was thrown for additional verification.
        /// </summary>
        public Exception Thrown { get; private set; }
        private bool _run;
        private IReadOnlyList<TThen> _actuals;

        /// <summary>
        /// Provides a definition for the <see cref="ScenarioRunner"/> to run.
        /// </summary>
        protected internal class ScenarioDefinition
        {
            private ScenarioDefinition(IReadOnlyList<TGiven> given, TWhen when, IReadOnlyList<TThen> then)
            {
                Given = given;
                When = when;
                Then = then;
            }

            private ScenarioDefinition(IReadOnlyList<TGiven> given, TWhen when, Exception expectedException, bool verifyMessage)
            {
                Given = given;
                When = when;
                Then = new TThen[]{};
                ExpectedException = expectedException;
                VerifyExceptionMessage = verifyMessage;
            }

            public bool VerifyExceptionMessage { get; set; }

            public Exception ExpectedException { get; }

            /// <summary>
            /// The givens for the current scenarioRunner.
            /// </summary>
            public IReadOnlyList<TGiven> Given { get; }

            /// <summary>
            /// The when for the current scenarioRunner
            /// </summary>
            public TWhen When { get; }

            /// <summary>
            /// The expected results from the current scenarioRunner.
            /// </summary>
            public IReadOnlyList<TThen> Then { get; }

            internal static ScenarioDefinitionBuilder Builder => new ScenarioDefinitionBuilder();

            internal class ScenarioDefinitionBuilder : IDefine<TGiven, TWhen, TThen>, IGiven<TWhen, TThen>, IWhen<TThen>
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

                void IWhen<TThen>.Throws<T>(T exception, bool verifyMessage)
                {
                    _scenarioDefinition = new ScenarioDefinition(_givens, _when, exception, true);
                }

                internal ScenarioDefinition Build()
                {
                    return _scenarioDefinition;
                }
            }

        }

        /// <inheritdoc />
        protected override async Task Run()
        {
            if (Definition == null)
                throw new InvalidOperationException(Constants.Errors.ScenarioNotDefined);

            if (_run) return;
            _run = true;
            RecordSetup();
            await Given(Definition.Given);
            try
            {
                await When(Definition.When);
            }
            catch (Exception ex) when (Definition.VerifyExceptionMessage)
            {
                Thrown = ex;
            }
            _actuals = await ActualResults();
            Verify(Definition.ExpectedException, Thrown, Definition.VerifyExceptionMessage);
            Verify(Definition.Then, _actuals);
        }

        private void Verify(Exception expected, Exception actual, bool verifyExceptionMessage)
        {
            if (expected == null) return;
            RecordThen("Exception", detail =>
            {
                if (actual == null)
                {
                    detail.Mismatch(expected.GetType().FullName, expected, actual);
                    return;
                }
                if (actual.GetType() != expected.GetType())
                    detail.Mismatch("Type", expected.GetType(), actual.GetType(), formatter: Formatters.FromClassName);
                if(verifyExceptionMessage)
                    if (expected.Message == actual.Message)
                        detail.Match("Message", expected.Message);
                    else
                        detail.Mismatch("Message", expected.Message, actual.Message);
            });
        }

        private void RecordSetup()
        {
            foreach (var given in Definition.Given)
            {
                Report(given, base.RecordGiven);
            }
            if (Definition.When != null)
                Report(Definition.When, RecordWhen);
        }

        /// <summary>
        /// Applies the given DTOs to the subject under test.
        /// </summary>
        /// <param name="givens">The givens to apply to the scenarioRunner</param>
        /// <returns>A Task that should be complete only after all the givens have been applied to the 
        /// subject under test.</returns>
        protected abstract Task Given(IReadOnlyList<TGiven> givens);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="when"></param>
        /// <returns></returns>
        protected abstract Task When(TWhen when);

        /// <summary>
        /// Retrieves the actual results for verification.
        /// </summary>
        /// <returns>A <see cref="Task{TThen}"/> that should complete when the results are available</returns>
        protected abstract Task<IReadOnlyList<TThen>> ActualResults();

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
        /// A custom set of comparers used when verifying the scenarioRunner. Can be used to provide non default comparers.
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
                            split = ignored.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                        else
                            split = new[] { "*", ignored };
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
        internal void Verify(IReadOnlyList<TThen> expected, IReadOnlyList<TThen> actual)
        {
            var maxIterations = Math.Min(expected.Count, actual.Count);

            //TODO: consider walking type tree until primitives
            for (int i = 0; i < maxIterations; i++)
            {

                var e = expected[i];
                var a = actual[i];
                RecordThen(e.GetType().Name, details =>
                {
                    if (a.GetType() != e.GetType())
                    {

                        details.Mismatch("Type", e, a, formatter: Formatters.FromClassName);

                        return;
                    }
                    HashSet<string> ignored = IgnoredByType(e.GetType());
                    
                    foreach (var p in e.GetType().GetProperties())
                    {
                        if (ignored.Contains(p.Name)) continue;
                        var ev = p.GetValue(e);
                        var av = p.GetValue(a);
                        if (!Comparers.TryGetValue(p.PropertyType, out var comparer))
                        {
                            comparer = (IEqualityComparer)typeof(EqualityComparer<>).MakeGenericType(p.PropertyType)
                                .GetProperty("Default", BindingFlags.Static | BindingFlags.Public).GetValue(null);
                        }
                        if (!comparer.Equals(ev, av))
                        {
                            //fail and continue
                            details.Mismatch(p.Name, ev, av);
                        }
                        else
                        {
                            details.Match(p.Name, ev);
                        }
                    }
                });
            }
            if (expected.Count > actual.Count)
            {
                for (int i = Math.Max(0, actual.Count - 1); i < expected.Count; i++)
                {
                    RecordThen("Missing expected results", details => details.Mismatch("Type", expected[i], null, formatter: Formatters.FromClassName));
                }
            }
            if (actual.Count > expected.Count)
            {
                for (int i = Math.Max(0, expected.Count - 1); i < actual.Count; i++)
                {
                    RecordThen("More results than expected", details => details.Mismatch("Type", null, actual[i], formatter: Formatters.FromClassName));
                }
            }
        }

        void DetailFromProperties(IAddDetail details, object instance, Type type)
        {
            var ignored = IgnoredByType(type);
            if (type.IsPrimitive)
                details.Add("Value", instance);
            var properties = type.GetProperties().Where(p => !ignored.Contains(p.Name));
            foreach (var property in properties)
                details.Add(property.Name, property.GetValue(instance));//TODO: Add looking up formatters and format strings
        }

        private void Report(object instance, Action<string, Action<IAddDetail>> create)
        {
            var type = instance.GetType();
            create(type.Name, details => DetailFromProperties(details, instance, type));
        }
    }

    public abstract class ReflectionBasedScenarioRunner<TGiven, TWhen, TThen, TState> : ReflectionBasedScenarioRunner<TGiven, TWhen, TThen> where TState : class
    {
        protected async override Task Run()
        {
            await base.Run();
            if(State == null)
                State = await AcquireState();
        }

        protected abstract Task<TState> AcquireState();
        public TState State { get; private set; }
    }

    /// <summary>
    /// Fluent interface for defining a <see cref="ScenarioRunner"/>
    /// </summary>
    /// <typeparam name="TG">The type of the Givens</typeparam>
    /// <typeparam name="TW">The type of the When</typeparam>
    /// <typeparam name="TT">The type of the Thens</typeparam>
    public interface IDefine<in TG, in TW, in TT>
    {
        /// <summary>
        /// Specify the givens for the scenarioRunner
        /// </summary>
        /// <param name="givens"></param>
        /// <returns>An interface to specify the when</returns>
        IGiven<TW, TT> Given(params TG[] givens);
    }
    /// <summary>
    /// Fluent interface for defining a <see cref="ScenarioRunner"/>
    /// </summary>
    /// <typeparam name="TW">The type of the When</typeparam>
    /// <typeparam name="TT">The type of the Thens</typeparam>
    public interface IGiven<in TW, in TT>
    {
        /// <summary>
        /// Specify the when of the scenarioRunner
        /// </summary>
        /// <param name="when">The when of the scenarioRunner</param>
        /// <returns>An interface for defining the Thens</returns>
        IWhen<TT> When(TW when);
    }
    /// <summary>
    /// Fluent interface for defining a <see cref="ScenarioRunner"/>
    /// </summary>
    /// <typeparam name="TT">The type of the Thens</typeparam>
    public interface IWhen<in TT>
    {
        /// <summary>
        /// Specify the thens of the scenarioRunner
        /// </summary>
        /// <param name="then">The thens of the scenarioRunner</param>
        void Then(params TT[] then);

        /// <summary>
        /// Specify that the When should throw an exception
        /// </summary>
        /// <typeparam name="T">The type of expected exception</typeparam>
        /// <param name="exception"></param>
        /// <param name="verifyMessage">If true then then the message of the exception as well as the type will be checked. If false, only the type of exception thrown will be checked.</param>
        void Throws<T>(T exception, bool verifyMessage = true) where T : Exception;
    }
}