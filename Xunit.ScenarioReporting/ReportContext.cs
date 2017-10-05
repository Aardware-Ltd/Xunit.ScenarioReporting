using System;
using System.Threading;

namespace Xunit.ScenarioReporting
{
    static class ReportContext
    {
        static readonly AsyncLocal<ScenarioReport> Current = new AsyncLocal<ScenarioReport>();

        public static IDisposable Set(ScenarioReport report)
        {
            Current.Value = report;
            return new Disposer();
        }

        class Disposer : IDisposable {
            public void Dispose()
            {
                Current.Value = null;
            }
        }
        public static ScenarioReport CurrentValue() => Current.Value;
    }
}