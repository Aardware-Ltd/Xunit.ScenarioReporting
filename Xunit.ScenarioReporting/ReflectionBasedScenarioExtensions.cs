using System;
using System.Threading.Tasks;

namespace Xunit.ScenarioReporting
{
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
        /// <returns></returns>
        public static Task<ScenarioRunResult> Run<TGiven, TWhen, TThen>(this ReflectionBasedScenarioRunner<TGiven, TWhen, TThen> scenarioRunner, Action<IDefine<TGiven, TWhen, TThen>> define, string title = null)
        {
            if (scenarioRunner.Title == null) scenarioRunner.Title = title;
            var builder = ReflectionBasedScenarioRunner<TGiven, TWhen, TThen>.ScenarioDefinition.Builder;
            define(builder);
            scenarioRunner.Definition = builder.Build();
            return scenarioRunner.Result();
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
        public static async Task<TState> Run<TGiven, TWhen, TThen, TState>(this ReflectionBasedScenarioRunner<TGiven, TWhen, TThen, TState> scenarioRunner, Action<IDefine<TGiven, TWhen, TThen>> define) where TState : class
        {
            if (scenarioRunner.State != null)
                return scenarioRunner.State;
            var builder = ReflectionBasedScenarioRunner<TGiven, TWhen, TThen>.ScenarioDefinition.Builder;
            define(builder);
            scenarioRunner.Definition = builder.Build();
            await scenarioRunner.Result();
            return scenarioRunner.State;
        }
    }
}