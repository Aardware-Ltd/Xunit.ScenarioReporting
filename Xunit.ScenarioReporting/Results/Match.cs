﻿using System;
using System.Collections.Generic;

namespace Xunit.ScenarioReporting.Results
{
    internal class Match : Detail
    {
        public Match(string name, object value, string format = null, Func<object, string> formatter = null) : base(name, value, format, formatter)
        {
        }

        public Match(IReadOnlyList<Detail> children, string name) : base(children, name)
        {
        }
    }
}