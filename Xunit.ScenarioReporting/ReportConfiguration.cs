using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;

namespace Xunit.ScenarioReporting
{
    class ReportConfiguration : IReportConfiguration
    {
        public string XmlReportFile { get; }
        public bool WriteHtml { get; }
        public bool WriteMarkdown { get; }
        public string HtmlReportFile { get; }
        public string MarkdownReportFile { get; }
        public string AssemblyName { get; }

        public bool WriteOutput { get; }
        private readonly IConfigurationSection _config;

        private const string ReportFileNamePrefix = "Report";
        private const string FileExtensionHtml = "html";
        private const string FileExtensionMarkdown = "md";
        private const string FileExtensionXml = "xml";

        public ReportConfiguration(string assemblyName, string currentDirectory, string assemblyFullPath, string assemblyConfigFullPath)
        {
            AssemblyName = assemblyName;

            if (assemblyFullPath == null && assemblyConfigFullPath == null)
            {
                WriteOutput = false;
                return;
            }
            if (!Path.IsPathRooted(assemblyFullPath))
                assemblyFullPath = Path.Combine(currentDirectory, assemblyFullPath);
            string assemblyPath = Path.GetDirectoryName(assemblyFullPath);
            string assemblyFile = Path.GetFileName(assemblyFullPath);

            var configbuilder = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(assemblyPath, "appsettings.json"), true)
                .AddEnvironmentVariables();

            var configurationRoot = configbuilder.Build();
            _config = configurationRoot.GetSection("appSettings");

            if (!RunUnderNcrunch(false) && Environment.GetEnvironmentVariable("NCrunch") == "1")
            {
                WriteOutput = false;
                return;
            }
            WriteOutput = true;

            XmlReportFile = GetTargetXmlReportFile(@default: DefaultFilePath(assemblyPath, assemblyFile, FileExtensionXml));
            
            WriteHtml = GetGenerateHtmlReport(@default: IsHtmlPathDefined());
            HtmlReportFile = GetTargetHtmlReportFile(@default: DefaultFilePath(assemblyPath, assemblyFile, FileExtensionHtml));
            WriteMarkdown = GetGenerateMarkdownReport(@default: IsMarkdownPathDefined());
            MarkdownReportFile = GetTargetMarkdownReportFile(@default: DefaultFilePath(assemblyPath, assemblyFile, FileExtensionMarkdown));
        }

        private static string DefaultFilePath(string assemblyPath, string assemblyFile, string extension)
        {
            return Path.Combine(assemblyPath,
                ReportFileNamePrefix + Path.ChangeExtension(assemblyFile, extension));
        }

        bool IsHtmlPathDefined()
        {
            return _config.GetSection("TargetHtmlReportFile").Exists();
        }

        bool IsMarkdownPathDefined()
        {
            return _config.GetSection("TargetHtmlReportFile").Exists();
        }

        public bool GetGenerateHtmlReport(bool @default)
        {
            return ReadBool(@default, "GenerateHtmlReport");
        }

        public bool GetGenerateMarkdownReport(bool @default)
        {
            return ReadBool(@default, "GenerateMarkdownReport");
        }

        public bool RunUnderNcrunch(bool @default)
        {
            return ReadBool(@default, nameof(RunUnderNcrunch));
        }

        private bool ReadBool(bool @default, string name)
        {
            bool dataParsed = @default;

            string dataIn = _config[name];

            if (!string.IsNullOrWhiteSpace(dataIn))
            {
                if (!Boolean.TryParse(dataIn, out dataParsed))
                {
                    throw new ArgumentException($"Configuration error. Key: {name}. True or False expected, '{dataIn}'");
                }
            }

            return dataParsed;
        }

        public string GetTargetHtmlReportFile(string @default)
        {
            return ReadFileSetting("TargetHtmlReportFile", @default);
        }
        
        public string GetTargetMarkdownReportFile(string @default)
        {
            return ReadFileSetting("TargetMarkdownReportFile", @default);
        }

        public string GetTargetXmlReportFile(string @default)
        {
            return ReadFileSetting("TargetXmlReportFile", @default);
        }

        private string ReadFileSetting(string key, string @default)
        {
            string dataParsed = @default;

            string dataIn = _config[key];

            if (!string.IsNullOrWhiteSpace(dataIn))
            {
                dataIn = dataIn.Trim();
                if (FileLooksValid(dataIn))
                {
                    dataParsed = dataIn;
                }
                else
                {
                    throw new ArgumentException($"Configuration Error. Key {key} Invalid file, '{dataIn}'");
                }
            }

            return dataParsed;
        }

        private bool FileLooksValid(String fileName)
        {
            System.IO.FileInfo fi = null; //File does not need to exist to create a FileInfo instance
            try
            {
                fi = new System.IO.FileInfo(fileName);
            }
            catch (ArgumentException) { }
            catch (System.IO.PathTooLongException) { }
            catch (NotSupportedException) { }

            if (ReferenceEquals(fi, null))
            {
                //File name isn't valid
                return false;
            }
            else
            {
                //File name is valid. May check for existence by calling fi.Exists.
                return true;
            }
        }

    }
}
