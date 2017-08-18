﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;
using System.Reflection;

namespace Xunit.ScenarioReporting
{
    class ReportWriter : IReportWriter
    {
        private readonly string _path;
        private readonly string _name;
        private readonly FileStream _fileStream;
        private readonly StreamWriter _sw;
        private readonly XmlWriter _xw;
        private readonly IReadOnlyDictionary<Type, Func<XmlWriter, ReportItem, Task>> _handlers;


        public ReportWriter(string path, string name)
        {
            _path = path;
            _name = name;
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

            _fileStream = new FileStream(Path.Combine(_path, Path.ChangeExtension(_name, ".xml")), FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
            _sw = new StreamWriter(_fileStream);

            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Async = true;
            _xw = XmlWriter.Create(_sw, xws);
        }

        private async Task WriteMarkdown()
        {
            string reportBaseFile = Path.Combine(_path, Path.ChangeExtension(_name, ".xml"));
            if (File.Exists(reportBaseFile))
            {
                Assembly assembly = Assembly.GetExecutingAssembly();

                //prep needed report components
                Stream sReportContent = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + ReportPath + "." + ReportAssemblyOverviewMarkdownContent);
                XmlReader xrReportContent = XmlReader.Create(sReportContent);
                XslCompiledTransform xctReportContent = new XslCompiledTransform();
                xctReportContent.Load(xrReportContent);

                //generate report
                Stream sReportOutput = new FileStream(Path.Combine(_path, ReportAssemblyOverviewMarkdown), FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);

                xctReportContent.Transform(reportBaseFile, null, sReportOutput);

                sReportOutput.Close();
                sReportOutput.Dispose();

            }

        }

        private async Task WriteHTML()
        {
            string reportBaseFile = Path.Combine(_path, Path.ChangeExtension(_name, ".xml"));
            if (File.Exists(reportBaseFile)) {

                Assembly assembly = Assembly.GetExecutingAssembly();

                //prep needed report components
                Stream sReportHeader = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + ReportPath + "." + ReportAssemblyOverviewHTMLHeader);
                Stream sReportContent = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + ReportPath + "." + ReportAssemblyOverviewHTMLContent);
                XmlReader xrReportContent = XmlReader.Create(sReportContent);
                XslCompiledTransform xctReportContent = new XslCompiledTransform();
                xctReportContent.Load(xrReportContent);
                Stream sReportFooter = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + ReportPath + "." + ReportAssemblyOverviewHTMLFooter);

                //generate report
                Stream sReportOutput = new FileStream(Path.Combine(_path, ReportAssemblyOverviewHTML), FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
                await sReportHeader.CopyToAsync(sReportOutput);
                xctReportContent.Transform(reportBaseFile, null, sReportOutput);
                await sReportFooter.CopyToAsync(sReportOutput);

                sReportOutput.Close();
                sReportOutput.Dispose();

            }

        }

        private async Task StartReport(XmlWriter writer, ReportItem item)
        {
            var start = (StartReport)item;
            //await writer.WriteLineAsync($"{H1} {start.ReportName}");
            //await writer.WriteLineAsync($"Run at {Bold} {start.ReportTime:R} {Bold}");
            //await writer.WriteLineAsync();
            //await writer.WriteLineAsync();
            await writer.WriteStartDocumentAsync();

            await writer.WriteStartElementAsync(null, XMLTagAssembly, null);

            await writer.WriteElementStringAsync(null, XMLTagName, null, start.ReportName);

            await writer.WriteElementStringAsync(null, XMLTagTime, null, start.ReportTime.ToString());

        }

        private async Task StartScenario(XmlWriter writer, ReportItem item)
        {
            var start = (StartScenario)item;
            //await writer.WriteLineAsync($"{H2} {start.Name}");
            await writer.WriteStartElementAsync(null, XMLTagScenario, null);

            await writer.WriteElementStringAsync(null, XMLTagName, null, start.Name);

        }
        

        private async Task Given(XmlWriter writer, ReportItem item)
        {
            var given = (Scenario.ReportEntry.Given)item;
            //await writer.WriteLineAsync($"{H4} {given.Title}");
            await writer.WriteStartElementAsync(null, XMLTagGiven, null);
            await WriteDetails(writer, given.Title, given.Details);
            await writer.WriteEndElementAsync();
        }

        private async Task Then(XmlWriter writer, ReportItem item)
        {
            var then = (Scenario.ReportEntry.Then)item;
            //await writer.WriteLineAsync($"{H4} {then.Title}");
            await writer.WriteStartElementAsync(null, XMLTagThen, null);
            await WriteDetails(writer, then.Title, then.Details);
            await writer.WriteEndElementAsync();
        }

        private async Task When(XmlWriter writer, ReportItem item)
        {
            var when = (Scenario.ReportEntry.When)item;
            //await writer.WriteLineAsync($"{H4} When ");
            //await writer.WriteLineAsync($"{H4} {when.Title}");
            await writer.WriteStartElementAsync(null, XMLTagWhen, null);
            await WriteDetails(writer, when.Title, when.Details);
            await writer.WriteEndElementAsync();
        }
        
        private static async Task WriteDetails(XmlWriter writer, string title, IReadOnlyList<Scenario.ReportEntry.Detail> details)
        {
            await writer.WriteElementStringAsync(null, XMLTagTitle, null, title);

            bool isFirst = true;
            foreach (var detail in details)
            {
                await writer.WriteStartElementAsync(null, XMLTagDetail, null);

                if (detail is Scenario.ReportEntry.Failure)
                {
                    //TODO: This could be an an xml error tag => update xsl and css 
                    //await writer.WriteLineAsync($"{Bold}FAILED {detail.Name}{Bold}");
                    await writer.WriteElementStringAsync(null, XMLTagMessage, null, "FAILED " + " " + detail.Name);

                } else { 
                            
                    if (!isFirst)
                    {
                        //TODO: Not data. Exclude from xml. Should be added to report generators
                        //await writer.WriteAsync($"and ");
                        //await writer.WriteElementStringAsync(null, "Message", null, "and");
                    }
                    else
                    {
                        //TODO: Not data. Exclude from xml. Should be added to report generators
                        isFirst = false;
                        //await writer.WriteAsync($"with ");
                        await writer.WriteElementStringAsync(null, XMLTagMessage, null, "with");

                    }
                    if (detail.Formatter != null)
                    {
                        //await writer.WriteLineAsync($"{Bold}{detail.Name} {Italic}{detail.Formatter(detail.Value)}{Italic}{Bold}");
                        //TODO: This test case not tested
                        //TODO: Might want to output as seperate tagged elements
                        await writer.WriteElementStringAsync(null, XMLTagMessage, null, detail.Name + " " + detail.Formatter(detail.Value));
                    }
                    else if (detail.Format != null)
                    {
                        // var formatString = $"{Bold}{detail.Name} {Italic}{{0:{detail.Format}}}{Italic}{Bold}";
                        //await writer.WriteLineAsync(string.Format(formatString, detail.Value));
                        //TODO: This test case not tested
                        //TODO: {0:{detail.Format}?
                        //TODO: Might want to output as separate tagged elements
                        await writer.WriteElementStringAsync(null, XMLTagMessage, null, detail.Name + " " + string.Format(detail.Format, detail.Value));
                    }
                    else
                    {
                        //await writer.WriteLineAsync($"{Bold}{detail.Name} {Italic}{detail.Value}{Italic}{Bold}");
                        //TODO: Might want to output as seperate tagged elements
                        await writer.WriteElementStringAsync(null, XMLTagMessage, null, detail.Name + " " + detail.Value);

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
            
            writer.Close();
            writer.Dispose();

            await _sw.FlushAsync();
            _sw.Dispose();
            _fileStream.Dispose();

            await WriteHTML();
            await WriteMarkdown();
        }
        

        public async Task Write(ReportItem item)
        {
            Func<XmlWriter, ReportItem, Task> handler;
            if (_handlers.TryGetValue(item.GetType(), out handler))
            {
               // await handler(_sw, item);
                await handler(_xw, item);
            }
            else { throw new InvalidOperationException($"Unsupported report item of type {item.GetType().FullName}"); }
        }

        public const string XMLTagAssembly = "Assembly";
        public const string XMLTagName = "Name";
        public const string XMLTagTime = "Time";
        public const string XMLTagScenario = "Definition";
        public const string XMLTagGiven = "Given";
        public const string XMLTagThen = "Then";
        public const string XMLTagWhen = "When";
        public const string XMLTagDetails = "Details";
        public const string XMLTagTitle = "Title";
        public const string XMLTagDetail = "Detail";
        public const string XMLTagMessage = "Message";
        public const string ReportAssemblyOverviewHTMLHeader = "ReportAssemblyOverviewHTMLHeader.html";
        public const string ReportAssemblyOverviewHTMLContent = "ReportAssemblyOverviewHTMLContent.xslt";
        public const string ReportAssemblyOverviewHTMLFooter = "ReportAssemblyOverviewHTMLFooter.html";
        public const string ReportAssemblyOverviewHTML = "ReportAssemblyOverview.html";
        public const string ReportAssemblyOverviewMarkdownContent = "ReportAssemblyOverviewMarkdownContent.xslt";
        public const string ReportAssemblyOverviewMarkdown = "ReportAssemblyOverview.md";
        public const string ReportPath = "Reports";





    }
}