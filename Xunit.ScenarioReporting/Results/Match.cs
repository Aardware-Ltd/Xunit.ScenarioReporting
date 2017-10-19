using System;
using System.Collections.Generic;

namespace Xunit.ScenarioReporting.Results
{
    internal class Match : Detail
    {
        public Match(string name, object value, bool displayByDefault, string format = null, Func<object, string> formatter = null) : base(name, value, displayByDefault, format, formatter)
        {
        }

        public Match(IReadOnlyList<Detail> children, bool displayByDefault, string name) : base(children, displayByDefault, name)
        {
        }
    }
}