using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;
using System.Reflection;
using static Xunit.ScenarioReporting.Constants;
namespace Xunit.ScenarioReporting
{
    class OutputController
    {
        private readonly IReportConfiguration _configuration;

        private readonly XmlWriter _xw;
        private readonly FileStream _fileStream;
        private readonly StreamWriter _sw;

        public OutputController(IReportConfiguration configuration)
        {
            _configuration = configuration;
            if (!configuration.WriteOutput)
            {
                Report = new ScenarioReport(configuration.AssemblyName, new NullWriter());
            }
            _fileStream = new FileStream(configuration.XmlReportFile, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);

            _sw = new StreamWriter(_fileStream);

            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Async = true;
            _xw = XmlWriter.Create(_sw, xws);

            var writer = new ReportWriter(_xw);
            Report = new ScenarioReport(configuration.AssemblyName, writer);
        }

        public ScenarioReport Report { get; }

        public async Task Complete()
        {
            await Report.WriteFinalAsync();
            _xw.Close();
            _xw.Dispose();

            await _sw.FlushAsync();
            _sw.Dispose();
            _fileStream.Dispose();

            if (_configuration.WriteHtml)
            {
                await WriteHTML(_configuration.XmlReportFile, _configuration.HtmlReportFile);
            }

            if (_configuration.WriteMarkdown)
            {
                WriteMarkdown(_configuration.XmlReportFile, _configuration.MarkdownReportFile);
            }
        }

        private void WriteMarkdown(string reportBaseFile, string reportFile)
        {
            if (File.Exists(reportBaseFile))
            {
                Assembly assembly = GetType().Assembly;

                //prep needed report components
                Stream sReportContent = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + ReportPath + "." + ReportAssemblyOverviewMarkdownContent);
                XmlReader xrReportContent = XmlReader.Create(sReportContent);
                XslCompiledTransform xctReportContent = new XslCompiledTransform();
                xctReportContent.Load(xrReportContent);

                //generate report
                Stream sReportOutput = new FileStream(reportFile, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);

                xctReportContent.Transform(reportBaseFile, null, sReportOutput);

                sReportOutput.Close();
                sReportOutput.Dispose();

            }

        }

        private async Task WriteHTML(string reportBaseFile, string reportFile)
        {
            if (File.Exists(reportBaseFile))
            {

                Assembly assembly = GetType().Assembly;

                //prep needed report components
                Stream sReportHeader = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + ReportPath + "." + ReportAssemblyOverviewHtmlHeader);
                Stream sReportContent = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + ReportPath + "." + ReportAssemblyOverviewHtmlContent);
                XmlReader xrReportContent = XmlReader.Create(sReportContent);
                XslCompiledTransform xctReportContent = new XslCompiledTransform();
                xctReportContent.Load(xrReportContent);
                Stream sReportFooter = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + ReportPath + "." + ReportAssemblyOverviewHtmlFooter);

                //generate report
                Stream sReportOutput = new FileStream(reportFile, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
                await sReportHeader.CopyToAsync(sReportOutput);
                xctReportContent.Transform(reportBaseFile, null, sReportOutput);
                await sReportFooter.CopyToAsync(sReportOutput);

                sReportOutput.Close();
                sReportOutput.Dispose();

            }

        }

        class NullWriter : IReportWriter
        {
            public Task Write(ReportItem item)
            {
                return Task.CompletedTask;
            }
        }

    }

    interface IReportConfiguration
    {
        bool WriteOutput { get; }
        string XmlReportFile { get; }
        bool WriteHtml { get; }
        bool WriteMarkdown { get; }
        string HtmlReportFile { get; }
        string MarkdownReportFile { get; }
        string AssemblyName { get; }
    }
    class ReportWriter : IReportWriter
    {
        private readonly XmlWriter _output;
        private readonly IReadOnlyDictionary<Type, Func<XmlWriter, ReportItem, Task>> _handlers;

        public ReportWriter(XmlWriter output)
        {
            _handlers = new Dictionary<Type, Func<XmlWriter, ReportItem, Task>>()
            {
                [typeof(StartReport)] = this.StartReport,
                [typeof(EndReport)] = this.EndReport,
                [typeof(StartScenario)] = this.StartScenario,
                [typeof(EndScenario)] = this.EndScenario,
                [typeof(Scenario.ReportEntry.Given)] = this.Given,
                [typeof(Scenario.ReportEntry.When)] = this.When,
                [typeof(Scenario.ReportEntry.Then)] = this.Then,
                [typeof(Scenario.ReportEntry.Assertion)] = this.Then,
            };

            _output = output;
        }


        private async Task StartReport(XmlWriter writer, ReportItem item)
        {
            var start = (StartReport)item;
            await writer.WriteStartDocumentAsync();

            await writer.WriteStartElementAsync(null, XmlTagAssembly, null);

            await writer.WriteElementStringAsync(null, XmlTagName, null, start.ReportName);

            await writer.WriteElementStringAsync(null, XmlTagTime, null, start.ReportTime.ToString());

        }

        private async Task StartScenario(XmlWriter writer, ReportItem item)
        {
            var start = (StartScenario)item;
            //await writer.WriteLineAsync($"{H2} {start.Name}");
            await writer.WriteStartElementAsync(null, XmlTagScenario, null);

            await writer.WriteElementStringAsync(null, XmlTagName, null, start.Name);

        }


        private async Task Given(XmlWriter writer, ReportItem item)
        {
            var given = (Scenario.ReportEntry.Given)item;
            //await writer.WriteLineAsync($"{H4} {given.Title}");
            await writer.WriteStartElementAsync(null, XmlTagGiven, null);
            await WriteDetails(writer, given.Title, given.Details);
            await writer.WriteEndElementAsync();
        }

        private async Task Then(XmlWriter writer, ReportItem item)
        {
            var then = (Scenario.ReportEntry.Then)item;
            //await writer.WriteLineAsync($"{H4} {then.Title}");
            await writer.WriteStartElementAsync(null, XmlTagThen, null);
            await WriteDetails(writer, then.Title, then.Details);
            await writer.WriteEndElementAsync();
        }

        private async Task When(XmlWriter writer, ReportItem item)
        {
            var when = (Scenario.ReportEntry.When)item;
            //await writer.WriteLineAsync($"{H4} When ");
            //await writer.WriteLineAsync($"{H4} {when.Title}");
            await writer.WriteStartElementAsync(null, XmlTagWhen, null);
            await WriteDetails(writer, when.Title, when.Details);
            await writer.WriteEndElementAsync();
        }

        private static async Task WriteDetails(XmlWriter writer, string title, IReadOnlyList<Scenario.ReportEntry.Detail> details)
        {
            await writer.WriteElementStringAsync(null, XmlTagTitle, null, title);

            foreach (var detail in details)
            {
                await writer.WriteStartElementAsync(null, XmlTagDetail, null);

                if (detail is Scenario.ReportEntry.Failure || detail is Scenario.ReportEntry.Mismatch)
                {
                    //await writer.WriteElementStringAsync(null, XMLTagMessage, null, "FAILED " + " " + detail.Name);
                    await writer.WriteElementStringAsync(null, XmlTagFailure, null, detail.Name);

                }
                if (detail.Formatter != null)
                {
                    //await writer.WriteLineAsync($"{Bold}{detail.Name} {Italic}{detail.Formatter(detail.Value)}{Italic}{Bold}");
                    //TODO: This test case not tested
                    //TODO: Might want to output as seperate tagged elements

                    await writer.WriteElementStringAsync(null, XmlTagMessage, null, detail.Name + " " + detail.Formatter(detail.Value));
                    var mismatch = detail as Scenario.ReportEntry.Mismatch;
                    if (mismatch != null)
                    {
                        await writer.WriteElementStringAsync(null, XmlTagMessage, null, mismatch.Name + " " + mismatch.Formatter(mismatch.Actual));
                    }

                }
                else if (detail.Format != null)
                {
                    // var formatString = $"{Bold}{detail.Name} {Italic}{{0:{detail.Format}}}{Italic}{Bold}";
                    //await writer.WriteLineAsync(string.Format(formatString, detail.Value));
                    //TODO: This test case not tested
                    //TODO: {0:{detail.Format}?
                    //TODO: Might want to output as separate tagged elements
                    await writer.WriteElementStringAsync(null, XmlTagMessage, null, detail.Name + " " + string.Format(detail.Format, detail.Value));
                    var mismatch = detail as Scenario.ReportEntry.Mismatch;
                    if (mismatch != null)
                    {
                        await writer.WriteElementStringAsync(null, XmlTagMessage, null, mismatch.Name + " " + string.Format(detail.Format, mismatch.Actual));
                    }
                }
                else
                {
                    //await writer.WriteLineAsync($"{Bold}{detail.Name} {Italic}{detail.Value}{Italic}{Bold}");
                    //TODO: Might want to output as seperate tagged elements
                    await writer.WriteElementStringAsync(null, XmlTagMessage, null, detail.Name + " " + detail.Value);
                    var mismatch = detail as Scenario.ReportEntry.Mismatch;
                    if (mismatch != null)
                    {
                        await writer.WriteElementStringAsync(null, XmlTagMessage, null, mismatch.Name + " " + mismatch.Actual);
                    }
                }

                await writer.WriteEndElementAsync();
            }
            //await writer.WriteEndElementAsync();
        }

        private async Task EndScenario(XmlWriter writer, ReportItem item)
        {
            //await writer.WriteLineAsync();
            //await writer.WriteLineAsync();
            await writer.WriteEndElementAsync();
        }

        private async Task EndReport(XmlWriter writer, ReportItem item)
        {
            await writer.WriteEndElementAsync();

            await writer.WriteEndDocumentAsync();
            await writer.FlushAsync();

        }

        public async Task Write(ReportItem item)
        {
            Func<XmlWriter, ReportItem, Task> handler;
            if (_handlers.TryGetValue(item.GetType(), out handler))
            {
                await handler(_output, item);
            }
            else
            {
                throw new InvalidOperationException($"Unsupported report item of type {item.GetType().FullName}");
            }
        }
    }
}