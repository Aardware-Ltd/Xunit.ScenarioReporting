using System;
using System.Collections.Generic;

namespace Xunit.ScenarioReporting.Results
{
    internal class Detail
    {
        public string Name { get; }
        public object Value { get; }
        public bool DisplayByDefault { get; }
        public string Format { get; }
        public Func<object, string> Formatter { get; }
        public IReadOnlyList<Detail> Children { get; }
        private static readonly IReadOnlyList<Detail> EmptyChildren = new Detail[] { };
        public Detail(string name, object value, bool displayByDefault, string format = null, Func<object, string> formatter = null)
        {
            Name = name;
            Value = value;
            DisplayByDefault = displayByDefault;
            Format = format;
            Formatter = formatter;
            Children = EmptyChildren;
        }

        public Detail(IReadOnlyList<Detail> children, bool displayByDefault, string name)
        {
            Name = name;
            DisplayByDefault = displayByDefault;
            Children = children ?? EmptyChildren;
        }
    }
}