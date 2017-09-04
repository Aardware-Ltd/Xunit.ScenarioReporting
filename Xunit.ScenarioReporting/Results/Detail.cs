using System;
using System.Collections.Generic;

namespace Xunit.ScenarioReporting.Results
{
    internal class Detail
    {
        public string Name { get; }
        public object Value { get; }
        public string Format { get; }
        public Func<object, string> Formatter { get; }
        public IReadOnlyList<Detail> Children { get; }
        private static readonly IReadOnlyList<Detail> EmptyChildren = new Detail[] { };
        public Detail(string name, object value, string format = null, Func<object, string> formatter = null)
        {
            Name = name;
            Value = value;
            Format = format;
            Formatter = formatter;
            Children = EmptyChildren;
        }

        public Detail(IReadOnlyList<Detail> children, string name)
        {
            Name = name;
            Children = children;
        }
    }
}