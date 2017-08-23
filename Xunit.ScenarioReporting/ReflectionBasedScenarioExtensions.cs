using System;

namespace Xunit.ScenarioReporting
{
    /// <summary>
    /// Provides a definition extension to classes deriving from <see cref="ReflectionBasedScenario{TGiven,TWhen,TThen}"/>.
    /// </summary>
    public static class ReflectionBasedScenarioExtensions
    {
        /// <summary>
        /// Provides a builder method to define a scenario in terms of DTOs for Givens, When and Thens
        /// </summary>
        /// <typeparam name="TGiven">The base type of Given DTOs</typeparam>
        /// <typeparam name="TWhen">The base type of the When DTO</typeparam>
        /// <typeparam name="TThen">The base type of Then DTOs</typeparam>
        /// <param name="scenario">The scenario for which to build the definition</param>
        /// <param name="define">A method that takes the builder and calls methods on it to build the scenario.</param>
        /// <returns></returns>
        public static Scenario Define<TGiven, TWhen, TThen>(this ReflectionBasedScenario<TGiven, TWhen, TThen> scenario, Action<IDefine<TGiven, TWhen, TThen>> define) 
        {
            var builder = ReflectionBasedScenario<TGiven, TWhen, TThen>.ScenarioDefinition.Builder;
            define(builder);
            scenario.Definition = builder.Build();
            return scenario;
        }
    }
}