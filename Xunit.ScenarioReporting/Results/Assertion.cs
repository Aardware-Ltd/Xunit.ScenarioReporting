using System;
using System.Collections.Generic;

namespace Xunit.ScenarioReporting.Results
{
    internal class Assertion : Then
    {
        public Assertion(string title) : base(title, (IReadOnlyList<Detail>) new Detail[] { })
        {
        }

        public Assertion(string title, Exception ex) : base(title, (IReadOnlyList<Detail>) new[] { ExceptionToDetail(ex) }) { }

        static Detail ExceptionToDetail(Exception ex)
        {
            return new Failure(ex.Message, ex.StackTrace);
        }
    }
}