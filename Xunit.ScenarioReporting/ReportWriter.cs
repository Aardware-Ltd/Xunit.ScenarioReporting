using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;
using System.Reflection;
using Xunit.ScenarioReporting.Results;
using static Xunit.ScenarioReporting.Constants;
using System.Text.RegularExpressions;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.ScenarioReporting
{
    class OutputController
    {
        private readonly IReportConfiguration _configuration;
        private readonly IMessageSink _diagnosticMessageSink;

        private readonly XmlWriter _xw;
        private readonly FileStream _fileStream;
        private readonly StreamWriter _sw;

        public OutputController(IReportConfiguration configuration, IMessageSink diagnosticMessageSink)
        {
            _configuration = configuration;
            _diagnosticMessageSink = diagnosticMessageSink;

            IReportWriter writer;
            if (!configuration.WriteOutput)
            {
                _diagnosticMessageSink.OnMessage(new DiagnosticMessage("Output is disabled"));
                writer = new NullWriter();
            }
            else
            {
                _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Creating report at {configuration.XmlReportFile}"));
                _fileStream = new FileStream(configuration.XmlReportFile, FileMode.Create, FileAccess.Write,
                    FileShare.None, 4096, true);

                _sw = new StreamWriter(_fileStream);

                XmlWriterSettings xws = new XmlWriterSettings();
                xws.Async = true;
                _xw = XmlWriter.Create(_sw, xws);

                writer = new ReportWriter(_xw);
            }
            Report = new ScenarioReport(configuration.AssemblyName, writer, _diagnosticMessageSink);
        }

        public ScenarioReport Report { get; }

        public async Task Complete()
        {
            _diagnosticMessageSink.OnMessage(new DiagnosticMessage("Completing report"));
            await Report.WriteFinalAsync();
            _xw.Close();
            _xw.Dispose();

            await _sw.FlushAsync();
            _sw.Dispose();
            _fileStream.Dispose();

            if (_configuration.WriteHtml)
            {
                _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Writing html to {_configuration.HtmlReportFile}"));
                await WriteHTML(_configuration.XmlReportFile, _configuration.HtmlReportFile);
            }

            if (_configuration.WriteMarkdown)
            {
                _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"Writing markdown to {_configuration.MarkdownReportFile}"));
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
    
    //TODO: this class would be better off using pattern matching 
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
                [typeof(Given)] = this.Given,
                [typeof(When)] = this.When,
                [typeof(Then)] = this.Then,
                [typeof(Assertion)] = this.Then,
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
            await writer.WriteStartElementAsync(null, XmlTagScenario, null);

            await writer.WriteElementStringAsync(null, XmlTagName, null, start.Name);
            // Create a Wordified Name (currently as NDG) only if there isn't a custom name. A custom name is one that isn't the same as the name in Scope.
            if (String.Equals(start.Name, start.Scope))
            {
                // Current Name is not custom, create a Wordified name
                await writer.WriteElementStringAsync(null, XmlTagNDG, null, Wordify(start.Name));
            }
            else
            {
                // Current Name is custom, don't create a Wordified name
                await writer.WriteElementStringAsync(null, XmlTagNDG, null, "[None created, because Name is a custom name.]");
            }
            await writer.WriteElementStringAsync(null, XmlTagScope, null, start.Scope);


        }


        private async Task Given(XmlWriter writer, ReportItem item)
        {
            var given = (Given)item;
            await writer.WriteStartElementAsync(null, XmlTagGiven, null);
            await WriteDetails(writer, given.Title, given.Details);
            await writer.WriteEndElementAsync();
        }

        private async Task Then(XmlWriter writer, ReportItem item)
        {
            var then = (Then)item;
            await writer.WriteStartElementAsync(null, XmlTagThen, null);
            await writer.WriteElementStringAsync(null, XmlTagScope, null, then.Scope);
            await WriteDetails(writer, then.Title, then.Details);
            await writer.WriteEndElementAsync();
        }

        private async Task When(XmlWriter writer, ReportItem item)
        {
            var when = (When)item;
            await writer.WriteStartElementAsync(null, XmlTagWhen, null);
            await WriteDetails(writer, when.Title, when.Details);
            await writer.WriteEndElementAsync();
        }

        private static async Task WriteDetails(XmlWriter writer, string title, IReadOnlyList<Detail> details)
        {
            await writer.WriteElementStringAsync(null, XmlTagTitle, null, title);

            foreach (var detail in details)
            {
                await writer.WriteStartElementAsync(null, XmlTagDetail, null);
                
                if (detail is Failure)
                {
                    await writer.WriteStartElementAsync(null, XmlTagFailure, null);

                    await writer.WriteStartElementAsync(null, XmlTagException, null);
                    var failure = detail as Failure;
                    await writer.WriteElementStringAsync(null, XmlTagName, null, detail.Name);
                    await writer.WriteElementStringAsync(null, XmlTagType, null, failure.Type.ToString());
                    await writer.WriteElementStringAsync(null, XmlTagValue, null, failure.Value.ToString()); // Value should contain stack trace for Exceptions
                    await writer.WriteEndElementAsync(); // /Exception

                    await writer.WriteEndElementAsync(); // /Failure

                }
                else if (detail.Formatter != null)
                {
                    await writer.WriteElementStringAsync(null, XmlTagName, null, detail.Name);
                    await writer.WriteElementStringAsync(null, XmlTagValue, null, detail.Formatter(detail.Value));

                    var mismatch = detail as Mismatch;
                    if (mismatch != null)
                    {
                        await WriteFailureMismatch(writer, mismatch.Name, mismatch.Formatter(mismatch.Value), mismatch.Formatter(mismatch.Actual));
                    }

                }
                else if (detail.Format != null)
                {
                    await writer.WriteElementStringAsync(null, XmlTagName, null, detail.Name);
                    await writer.WriteElementStringAsync(null, XmlTagValue, null, string.Format(detail.Format, detail.Value));
                    var mismatch = detail as Mismatch;
                    if (mismatch != null)
                    {
                        await WriteFailureMismatch(writer, mismatch.Name, string.Format(detail.Format, mismatch.Value), string.Format(detail.Format, mismatch.Actual));
                    }
                }
                else
                {
                    await writer.WriteElementStringAsync(null, XmlTagName, null, detail.Name);
                    await writer.WriteElementStringAsync(null, XmlTagValue, null, $"{detail.Value}");
                    var mismatch = detail as Mismatch;
                    if (mismatch != null)
                    {
                        await WriteFailureMismatch(writer, mismatch.Name, $"{mismatch.Value}", mismatch.Actual?.ToString());
                    }
                }

                //Recurse any children
                foreach (var childDetail in detail.Children)
                {
                    await writer.WriteStartElementAsync(null, XmlTagChild, null);
                    await WriteDetails(writer, childDetail.Name, childDetail.Children);
                    await writer.WriteEndElementAsync(); // /Child
                } 

                await writer.WriteEndElementAsync(); // /Detail
            }

        }

        private static async Task WriteFailureMismatch(XmlWriter writer, string name, string valueExpected, string valueActual)
        {
            await writer.WriteStartElementAsync(null, XmlTagFailure, null);


            await writer.WriteStartElementAsync(null, XmlTagMismatch, null);


            await writer.WriteElementStringAsync(null, XmlTagName, null, name);

            await writer.WriteStartElementAsync(null, XmlTagExpected, null);
            await writer.WriteElementStringAsync(null, XmlTagValue, null, valueExpected);
            await writer.WriteEndElementAsync(); //  /Expected

            await writer.WriteStartElementAsync(null, XmlTagActual, null);
            await writer.WriteElementStringAsync(null, XmlTagValue, null, valueActual);
            await writer.WriteEndElementAsync(); //  /Actual


            await writer.WriteEndElementAsync(); //  /Mismatch


            await writer.WriteEndElementAsync(); //   /Failure

        }

        private static string ToSentenceCase(string str)
        {
            return Regex.Replace(str, "[a-z][A-Z]", m => $"{m.Value[0]} {char.ToLower(m.Value[1])}");
        }

        private static string Wordify(string str) {
            //TODO: Basic. Revisit.
            str = str.Replace(".", ". ");
            str = str.Replace("_", " ");
            return ToSentenceCase(str);
        }

        private async Task EndScenario(XmlWriter writer, ReportItem item)
        {
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