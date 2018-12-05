using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PFG.DailyReport
{
    public class Config
    {
        private const string ConfigFileName = "Customized.PFG.Config.xml";

        private static string ConfigFile
        {
            get
            {
                string exePath = System.AppDomain.CurrentDomain.BaseDirectory;

                string filePath = Path.Combine(exePath, ConfigFileName);

                if (File.Exists(filePath))
                {
                    return filePath;
                }
                else
                {
                    filePath = Path.Combine(exePath, "bin", ConfigFileName);

                    if (File.Exists(filePath))
                    {
                        return filePath;
                    }
                    else
                    {
                        return ConfigFileName;
                    }
                }
            }
        }

        private static XElement Root
        {
            get
            {
                return XDocument.Load(ConfigFile).Root;
            }
        }

        public static string ReportFolder
        {
            get
            {
                return Root.Element("DailyReportFolder").Value;
            }
        }
    }
}
