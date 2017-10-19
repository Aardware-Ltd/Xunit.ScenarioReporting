using System;

namespace Xunit.ScenarioReporting.Results
{
    internal class Failure : Detail
    {
        public Type Type { get; }

        public Failure(string name, Type type, string stacktrace) : base(name, stacktrace, true)
        {
            Type = type;
        }
    }
}