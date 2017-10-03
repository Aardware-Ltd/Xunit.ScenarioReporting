using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xunit.ScenarioReporting
{
    class ScenarioScope
    {
        readonly static AsyncLocal<string> Current = new AsyncLocal<string>();

        public static IDisposable Push(string value)
        {
            Current.Value = value;
            return new Disposer();
        }

        public static string CurrentValue() => Current.Value;

        class Disposer : IDisposable
        {
            public void Dispose()
            {
                Current.Value = null;
            }
        }
    }
    /// <summary>
    /// Provides a definition extension to classes deriving from <see cref="ReflectionBasedScenarioRunner{TGiven,TWhen,TThen}"/>.
    /// </summary>
    public static class ReflectionBasedScenarioExtensions
    {
        /// <summary>
        /// Provides a builder method to define a scenarioRunner in terms of DTOs for Givens, When and Thens
        /// </summary>
        /// <typeparam name="TGiven">The base type of Given DTOs</typeparam>
        /// <typeparam name="TWhen">The base type of the When DTO</typeparam>
        /// <typeparam name="TThen">The base type of Then DTOs</typeparam>
        /// <param name="scenarioRunner">The scenarioRunner for which to build the definition</param>
        /// <param name="define">A method that takes the builder and calls methods on it to build the scenarioRunner.</param>
        /// <param name="title">An optional title for the scenario. If not specified the name will be taken from the scope of the scenario (Test method, Class Fixture, Collection Fixture)</param>
        /// <returns></returns>
        public static async Task Run<TGiven, TWhen, TThen>(this ReflectionBasedScenarioRunner<TGiven, TWhen, TThen> scenarioRunner, Action<IDefine<TGiven, TWhen, TThen>> define, string title = null)
        {
            BuildScenario(scenarioRunner, define, title);
            if (scenarioRunner.DelayReporting)
                await scenarioRunner.Execute();
            else await scenarioRunner.Complete(ReportContext.CurrentValue());
        }

        private static void BuildScenario<TGiven, TWhen, TThen>(ReflectionBasedScenarioRunner<TGiven, TWhen, TThen> scenarioRunner, Action<IDefine<TGiven, TWhen, TThen>> define,
            string title)
        {
            if (scenarioRunner.Title == null) scenarioRunner.Title = title;
            scenarioRunner.Scope = scenarioRunner.Scope ?? ScenarioScope.CurrentValue();
            var builder = ReflectionBasedScenarioRunner<TGiven, TWhen, TThen>.ScenarioDefinition.Builder;
            define(builder);
            scenarioRunner.Definition = builder.Build();
        }

        /// <summary>
        /// Provides a builder method to define a scenarioRunner in terms of DTOs for Givens, When and Thens
        /// </summary>
        /// <typeparam name="TGiven">The base type of Given DTOs</typeparam>
        /// <typeparam name="TWhen">The base type of the When DTO</typeparam>
        /// <typeparam name="TThen">The base type of Then DTOs</typeparam>
        /// /// <typeparam name="TState">The type of the state that will be returned for further assertions</typeparam>
        /// <param name="scenarioRunner">The scenarioRunner for which to build the definition</param>
        /// <param name="define">A method that takes the builder and calls methods on it to build the scenarioRunner.</param>
        /// <returns></returns>
        public static async Task<TState> Run<TGiven, TWhen, TThen, TState>(this ReflectionBasedScenarioRunner<TGiven, TWhen, TThen, TState> scenarioRunner, Action<IDefine<TGiven, TWhen, TThen>> define, string title = null) where TState : class
        {
            if (!scenarioRunner.DelayReporting)
                throw new InvalidOperationException(
                    "Task runners with state should only be used in class or collection fixtures");
            if (scenarioRunner.State != null)
                return scenarioRunner.State;
            BuildScenario(scenarioRunner, define, title);
            if (scenarioRunner.DelayReporting)
                await scenarioRunner.Execute();
            return scenarioRunner.State;
        }
    }
}