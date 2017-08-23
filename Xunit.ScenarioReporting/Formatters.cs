namespace Xunit.ScenarioReporting
{
    /// <summary>
    /// Provides common formatting for objects
    /// </summary>
    public static class Formatters
    {
        /// <summary>
        /// Formats an object as it's type name
        /// </summary>
        /// <param name="instance">The instance to format</param>
        /// <returns>The formatted string for the instance</returns>
        public static string FromClassName(object instance)
        {
            var type = instance?.GetType();
            return type?.FullName;
        }
    }
}