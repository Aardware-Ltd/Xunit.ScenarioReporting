namespace Xunit.ScenarioReporting
{
    /// <summary>
    /// A scenario run result that exposes some additional state for additional verifications beyond verifying the expected
    /// Thens
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    public class ScenarioRunResult<TState> : ScenarioRunResult
    {
        /// <summary>
        /// Creates a new instance of a <see cref="ScenarioRunResult{TState}"/>
        /// </summary>
        /// <param name="inner">The scenario run result that this instance will wrap.</param>
        /// <param name="state">The additional state to expose for further verification</param>
        public ScenarioRunResult(ScenarioRunResult inner, TState state) : base(inner.Title, inner.Given, inner.When, inner.Then, inner.ErrorInfo)
        {
            State = state;
        }

        /// <summary>
        /// Gets the additional state to use for verification
        /// </summary>
        public TState State { get; }
    }
}