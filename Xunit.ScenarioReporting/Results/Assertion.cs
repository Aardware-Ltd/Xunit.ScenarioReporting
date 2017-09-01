using System;

namespace Xunit.ScenarioReporting.Results
{
    internal class Assertion : Then
    {
        public Assertion(string scope, string title) : base(scope, title, new Detail[] { })
        {
        }

        public Assertion(string scope, string title, Exception ex) : base(scope, title, new[] { ExceptionToDetail(ex) }) { }

        static Detail ExceptionToDetail(Exception ex)
        {
            return new Failure(ex.Message, ex.StackTrace);
        }
    }
}