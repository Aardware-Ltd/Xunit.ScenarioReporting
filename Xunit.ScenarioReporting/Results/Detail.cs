using System;

namespace Xunit.ScenarioReporting.Results
{
    internal class Detail
    {
        public string Name { get; }
        public object Value { get; }
        public string Format { get; }
        public Func<object, string> Formatter { get; }

        public Detail(string name, object value, string format = null, Func<object, string> formatter = null)
        {
            Name = name;
            Value = value;
            Format = format;
            Formatter = formatter;
        }
    }
}