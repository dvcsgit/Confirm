using Customized.FPTC.Models.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Customized.FPTC.Utility
{
    public class Config
    {
        private const string ConfigFileName = "Customized.FPTC.Config.xml";

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

        public static List<Company> CompanyList
        {
            get
            {
                var companyList = new List<Company>();

                var elements = Root.Element("COMPANYS").Elements("COMPANY").ToList();

                foreach (var element in elements)
                {
                    companyList.Add(new Company()
                    {
                        ID = element.Attribute("ID").Value,
                        Name = element.Attribute("NAME").Value
                    });
                }

                companyList = companyList.OrderBy(x => x.ID).ToList();

                return companyList;
            }
        }

        public static List<Department> DepartmentList
        {
            get
            {
                var departmentList = new List<Department>();

                var elements = Root.Element("DEPARTMENTS").Elements("DEPARTMENT").ToList();

                foreach (var element in elements)
                {
                    departmentList.Add(new Department()
                    {
                        CompanyID = element.Attribute("COMPANYID").Value,
                        ID = element.Attribute("ID").Value,
                        Name = element.Attribute("NAME").Value
                    });
                }

                departmentList = departmentList.OrderBy(x => x.CompanyID).ThenBy(x => x.ID).ToList();

                return departmentList;
            }
        }
    }
}
