using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Xunit.Abstractions;
using Xunit.ScenarioReporting.Results;

namespace Xunit.ScenarioReporting.Tests
{

    public class EmptyScenario
    {
        [Fact]
        public async Task ShouldWriteUndefinedReport()
        {
            var sb = new StringBuilder();
            var writer = XmlWriter.Create(sb, new XmlWriterSettings(){Async = true});
            var reportWriter = new ReportWriter(writer);
            var scenarioReport = new ScenarioReport("Test", reportWriter, new TestMessageSink());
            var scenario = new ScenarioRunResult("Undefined", new Given[]{}, new When("when", new Detail[]{}),new Then[]{}, null);
            scenarioReport.Report(scenario);
            await scenarioReport.WriteFinalAsync();
            var xml = XElement.Parse(sb.ToString());
            Assert.Single(xml.Descendants(Constants.XmlTagScenario));
        }
    }

    class TestMessageSink : IMessageSink
    {
        private readonly ConcurrentQueue<IMessageSinkMessage> _messages;

        public TestMessageSink()
        {
            _messages = new ConcurrentQueue<IMessageSinkMessage>();
        }
        public bool OnMessage(IMessageSinkMessage message)
        {
            _messages.Enqueue(message);
            return true;
        }

        public IReadOnlyList<IMessageSinkMessage> Messages => _messages.ToArray();
    }
}
