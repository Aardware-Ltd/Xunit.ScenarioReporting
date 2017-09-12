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
        public IReadOnlyList<Detail> Children => _children;

        private List<Detail> _children; 

        public Detail(string name, object value, string format = null, Func<object, string> formatter = null)
        {
            Name = name;
            Value = value;
            Format = format;
            Formatter = formatter;
            _children = new List<Detail>();
        }

        public Detail(Detail parent, string name, object value, string format = null, Func<object, string> formatter = null) : this(name, value, format, formatter)
        {
            parent._children.Add(this);
        }
    }
}