namespace Xunit.ScenarioReporting
{
    class StartScenario : ReportItem
    {
        public string Name { get; }

        public StartScenario(string name)
        {
            Name = name;
        }
    }
}