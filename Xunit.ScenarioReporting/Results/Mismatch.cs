using System;

namespace Xunit.ScenarioReporting.Results
{
    internal class Mismatch : Detail
    {
        public object Actual { get; }

        public Mismatch(string name, object expected, object actual, string format = null, Func<object, string> formatter = null) : base(name, expected, format, formatter)
        {
            Actual = actual;
        }
    }
}