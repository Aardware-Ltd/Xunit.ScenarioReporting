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

    internal class MissingResult : Mismatch {
        public MissingResult(string name, object expected, string format = null, Func<object, string> formatter = null) : base(name, expected, null, format, formatter)
        {
        }
    }
    internal class ExtraResult : Mismatch {
        public ExtraResult(string name, object actual, string format = null, Func<object, string> formatter = null) : base(name, null, actual, format, formatter)
        {
        }
    }
}