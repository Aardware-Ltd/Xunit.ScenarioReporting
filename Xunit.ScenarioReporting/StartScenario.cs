namespace Xunit.ScenarioReporting
{
    class StartScenario : ReportItem
    {
        public string Name { get; }
        public string Scope { get; }
        public string Grouping { get; }

        public StartScenario(string name, string scope, string grouping)
        {
            Name = name;
            Scope = scope;
            Grouping = grouping;
        }
    }
}