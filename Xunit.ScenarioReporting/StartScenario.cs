namespace Xunit.ScenarioReporting
{
    class StartScenario : ReportItem
    {
        public string Name { get; }
        public string Scope { get; }

        public StartScenario(string name, string scope)
        {
            Name = name;
            Scope = scope;
        }
    }
}