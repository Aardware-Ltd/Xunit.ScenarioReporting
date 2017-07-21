using System.Threading.Tasks;

namespace Xunit.ScenarioReporting
{
    interface IReportWriter
    {
        Task Write(ReportItem item);
    }
}