using System;

namespace Xunit.ScenarioReporting
{
    class ScenarioVerificationException : Exception
    {
        public Exception Error { get; }

        public ScenarioVerificationException(Exception error)
        {
            Error = error;
        }

        public override string ToString()
        {
            return Error.ToString();
        }
    }
}