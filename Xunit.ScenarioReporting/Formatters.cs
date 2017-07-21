namespace Xunit.ScenarioReporting
{
    public static class Formatters
    {
        public static string FromClassName(object instance)
        {
            var type = instance?.GetType();
            return type?.FullName;
        }
    }
}