using System;
using System.Configuration;
using System.IO;

namespace Xunit.ScenarioReporting
{
    class ReportConfiguration
    {
        private readonly bool _defaultGenerateHtmlReport;
        private readonly bool _defaultGenerateMarkdownReport;
        private readonly string _defaultTargetHtmlReportFile;
        private readonly string _defaultTargetMarkdownReportFile;
        private readonly string _defaultTargetXmlReportFile;
        private readonly string _configFullPathName;

        private const string ReportFileNamePrefix = "Report";
        private const string FileExtensionHtml = "html";
        private const string FileExtensionMarkdown = "md";
        private const string FileExtensionXml = "xml";

        public ReportConfiguration(string assemblyFullPathName, string assemblyConfigFullPathName) {
            _defaultGenerateHtmlReport = false;
            _defaultGenerateMarkdownReport = false;
            string assemblyPath = Path.GetDirectoryName(assemblyFullPathName);
            string assemblyFile = Path.GetFileName(assemblyFullPathName);
            _defaultTargetHtmlReportFile = Path.Combine(assemblyPath, ReportFileNamePrefix + Path.ChangeExtension(assemblyFile, FileExtensionHtml));
            _defaultTargetMarkdownReportFile = Path.Combine(assemblyPath, ReportFileNamePrefix + Path.ChangeExtension(assemblyFile, FileExtensionMarkdown));
            _defaultTargetXmlReportFile = Path.Combine(assemblyPath, ReportFileNamePrefix + Path.ChangeExtension(assemblyFile, FileExtensionXml));
            _configFullPathName = assemblyConfigFullPathName;
        }

        public bool GetGenerateHtmlReport()
        {
            bool dataParsed = _defaultGenerateHtmlReport;

            string dataIn = ReadAppSetting("GenerateHtmlReport");

            if (dataIn != String.Empty)
            {
                if (Boolean.TryParse(dataIn, out dataParsed))
                {
                    //dataParsed now has the parsed value, do nothing
                }
                else
                { 
                    throw new ArgumentException("Configuration file error. True or False expected, '" + dataIn + "'  in appSettings Key GenerateHtmlReport.");
                }
            }

            return dataParsed;

        }

        public bool GetGenerateMarkdownReport()
        {
            bool dataParsed = _defaultGenerateMarkdownReport;

            string dataIn = ReadAppSetting("GenerateMarkdownReport");

            if (dataIn != String.Empty)
            {
                if (Boolean.TryParse(dataIn, out dataParsed))
                {
                    //dataParsed now has the parsed value, do nothing
                }
                else
                {
                    throw new ArgumentException("Configuration file error. True or False expected, '" + dataIn + "' in appSettings Key GenerateMarkdownReport.");
                }
            }

            return dataParsed;

        }

        public string GetTargetHtmlReportFile()
        {
            string dataParsed = _defaultTargetHtmlReportFile;

            string dataIn = ReadAppSetting("TargetHtmlReportFile").Trim();

            if (dataIn != String.Empty)
            {
                if (FileLooksValid(dataIn))
                {
                    dataParsed = dataIn;
                }
                else
                {
                    throw new ArgumentException("Configuration file error. Invalid file, '" + dataIn + "' in appSettings Key TargetHtmlReportFile.");
                }
            }

            return dataParsed;

        }

        public string GetTargetMarkdownReportFile()
        {
            string dataParsed = _defaultTargetMarkdownReportFile;

            string dataIn = ReadAppSetting("TargetMarkdownReportFile").Trim();

            if (dataIn != String.Empty)
            {
                if (FileLooksValid(dataIn))
                {
                    dataParsed = dataIn;
                }
                else
                {
                    throw new ArgumentException("Configuration file error. Invalid file, '" + dataIn + "' in appSettings Key TargetMarkdownReportFile.");
                }
            }

            return dataParsed;

        }

        public string GetTargetXmlReportFile()
        {
            string dataParsed = _defaultTargetXmlReportFile;

            string dataIn = ReadAppSetting("TargetXmlReportFile").Trim();

            if (dataIn != String.Empty)
            {
                if (FileLooksValid(dataIn))
                {
                    dataParsed = dataIn;
                }
                else
                {
                    throw new ArgumentException("Configuration file error. Invalid file, '" + dataIn + "' in appSettings Key TargetXmlReportFile.");
                }
            }

            return dataParsed;

        }

        private string ReadAppSetting(string appSettingKey)
        {
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
            configFileMap.ExeConfigFilename = _configFullPathName;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            if (config.AppSettings.Settings[appSettingKey] != null)
                return config.AppSettings.Settings[appSettingKey].Value;
            else
                return String.Empty;
            
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
