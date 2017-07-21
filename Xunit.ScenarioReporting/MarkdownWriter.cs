using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Xunit.ScenarioReporting
{
    class MarkdownWriter : IReportWriter {
        private readonly string _path;
        private readonly string _name;
        private readonly FileStream _fileStream;
        private readonly StreamWriter _sw;
        private readonly IReadOnlyDictionary<Type, Func<TextWriter, ReportItem, Task>> _handlers;
        private bool _disposed;

        public MarkdownWriter(string path, string name)
        {
            _path = path;
            _name = name;
            _handlers = new Dictionary<Type, Func<TextWriter, ReportItem, Task>>()
            {
                [typeof(StartReport)] = this.StartReport,
                [typeof(EndReport)] = this.EndReport,
                [typeof(StartScenario)] = this.StartScenario,
                [typeof(EndScenario)] = this.EndScenario,
                [typeof(StartGivens)] = this.StartGivens,
                [typeof(AdditionalGiven)] = this.Additional,
                [typeof(Scenario.Given)] = this.Given,
                [typeof(Scenario.When)] = this.When,
                [typeof(StartThens)] = this.StartThens,
                [typeof(AdditionalThen)] = this.Additional,
                [typeof(Scenario.Then)] = this.Then,
                [typeof(Scenario.Assertion)] = this.Then,
            };
            _fileStream = new FileStream(Path.Combine(_path, Path.ChangeExtension(_name, ".md")), FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
            _sw = new StreamWriter(_fileStream);
        }

        private async Task StartReport(TextWriter writer, ReportItem item)
        {
            var start = (StartReport) item;
            await writer.WriteLineAsync($"{H1} {start.ReportName}");
            await writer.WriteLineAsync($"Run at {Bold} {start.ReportTime:R} {Bold}");
            await writer.WriteLineAsync();
            await writer.WriteLineAsync();
        }

        private async Task StartScenario(TextWriter writer, ReportItem item)
        {
            var start = (StartScenario)item;
            await writer.WriteLineAsync($"{H2} {start.Name}");
        }

        private async Task StartGivens(TextWriter writer, ReportItem item)
        {
            await writer.WriteLineAsync($"{H3} Given ");
        }

        private async Task Given(TextWriter writer, ReportItem item)
        {
            var given = (Scenario.Given) item;
            await writer.WriteLineAsync($"{H4} {given.Title}");
            await WriteDetails(writer, given.Details);
        }

        private async Task Then(TextWriter writer, ReportItem item)
        {
            var then = (Scenario.Then)item;
            await writer.WriteLineAsync($"{H4} {then.Title}");
            await WriteDetails(writer, then.Details);
        }
        private async Task When(TextWriter writer, ReportItem item)
        {
            var when = (Scenario.When) item;
            await writer.WriteLineAsync($"{H4} When ");
            await writer.WriteLineAsync($"{H4} {when.Title}");
            await WriteDetails(writer, when.Details);

        }
        private async Task StartThens(TextWriter writer, ReportItem item)
        {
            await writer.WriteLineAsync($"{H4} Then ");
        }

        private static async Task WriteDetails(TextWriter writer, IReadOnlyList<Scenario.Detail> details)
        {
            bool isFirst = true;
            foreach (var detail in details)
            {
                if (detail is Scenario.Failure)
                {
                    await writer.WriteLineAsync($"{Bold}FAILED {detail.Name}{Bold}");
                    continue;
                }
                if (!isFirst)
                {
                    await writer.WriteAsync($"and ");
                }
                else
                {
                    isFirst = false;
                    await writer.WriteAsync($"with ");
                }
                if (detail.Formatter != null)
                {
                    await writer.WriteLineAsync($"{Bold}{detail.Name} {Italic}{detail.Formatter(detail.Value)}{Italic}{Bold}");
                }
                else if (detail.Format != null)
                {
                    var formatString = $"{Bold}{detail.Name} {Italic}{{0:{detail.Format}}}{Italic}{Bold}";
                    await writer.WriteLineAsync(string.Format(formatString, detail.Value));
                }
                else
                    await writer.WriteLineAsync($"{Bold}{detail.Name} {Italic}{detail.Value}{Italic}{Bold}");
            }
        }

        private async Task EndScenario(TextWriter writer, ReportItem item)
        {
            await writer.WriteLineAsync();
            await writer.WriteLineAsync();
        }
        private async Task EndReport(TextWriter writer, ReportItem item)
        {
            await _sw.FlushAsync();
            _sw.Dispose();
            _fileStream.Dispose();
        }

        private async Task Additional(TextWriter writer, ReportItem item)
        {
            await writer.WriteLineAsync("and");
        }
        public async Task Write(ReportItem item)
        {
            Func<TextWriter, ReportItem, Task> handler;
            if (_handlers.TryGetValue(item.GetType(), out handler))
            {
                await handler(_sw, item);
            }
            else { throw new InvalidOperationException($"Unsupported report item of type {item.GetType().FullName}");}
        }

        public const string H1 = "#";
        public const string H2 = H1 + "#";
        public const string H3 = H2 + "#";
        public const string H4 = H3 + "#";
        public const string H5 = H4 + "#";
        public const string H6 = H5 + "#";

        public const string Bold = "**";
        public const string Italic = "_";
    }
}