using System;
using System.Reflection;

namespace Xunit.ScenarioReporting
{
    static class ExceptionExtensions
    {
        public static Exception Unwrap(this Exception ex)
        {
            while (true)
            {
                var tiex = ex as TargetInvocationException;
                if (tiex == null)
                    return ex;

                ex = tiex.InnerException;
            }
        }
    }
}