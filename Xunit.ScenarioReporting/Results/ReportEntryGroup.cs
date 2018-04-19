using System.Collections;
using System.Collections.Generic;

namespace Xunit.ScenarioReporting.Results
{
    internal class ReportEntryGroup : ReportEntry, IReadOnlyList<ReportEntry>
    {
        private readonly IReadOnlyList<ReportEntry> _entries;

        public ReportEntryGroup(string name, IReadOnlyList<ReportEntry> entries):base(name, Empty<Detail>.ReadOnlyList)
        {
            _entries = entries;
        }

        public IEnumerator<ReportEntry> GetEnumerator()
        {
            return _entries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _entries).GetEnumerator();
        }

        public int Count => _entries.Count;

        public ReportEntry this[int index] => _entries[index];
    }
}