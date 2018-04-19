using System;
using System.Collections.Generic;
using System.Text;

namespace Xunit.ScenarioReporting
{
    static class Empty<T>
    {
        internal static readonly IReadOnlyList<T> ReadOnlyList = new T[]{};
    }
}
