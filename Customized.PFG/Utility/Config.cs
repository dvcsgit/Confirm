using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Customized.PFG.Utility
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

        public static List<EReportConfig> EReportConfig
        {
            get
            {
                var configs = new List<EReportConfig>();

                var elements = Root.Elements("EReport").ToList();

                foreach (var element in elements)
                {
                    var config = new EReportConfig()
                    {
                        RouteUniqueID = element.Attribute("RouteUniqueID").Value,
                        _2003 = element.Attribute("_2003").Value,
                        _2007 = element.Attribute("_2007").Value
                    };

                    var sheets = element.Elements("Sheet").ToList();

                    foreach (var sheet in sheets)
                    {
                        var sheetConfig = new EReportSheetConfig()
                        {
                            SheetIndex = int.Parse(sheet.Attribute("Index").Value),
                            CheckUserRowIndex = int.Parse(sheet.Attribute("CheckUserRowIndex").Value),
                            CheckUserCellIndex = int.Parse(sheet.Attribute("CheckUserCellIndex").Value)
                        };

                        var rows = sheet.Elements("Row").ToList();

                        foreach (var row in rows)
                        {
                            var rowConfig = new EReportRowConfig()
                            {
                                RowIndex = int.Parse(row.Attribute("Index").Value)
                            };

                            var cells = row.Elements("Cell").ToList();

                            foreach (var cell in cells)
                            {
                                rowConfig.CellList.Add(new EReportCellConfig()
                                {
                                    CellIndex = int.Parse(cell.Attribute("Index").Value),
                                    ControlPoint = cell.Attribute("ControlPoint").Value,
                                    Equipment = cell.Attribute("Equipment").Value,
                                    CheckItem = cell.Attribute("CheckItem").Value
                                });
                            }

                            sheetConfig.RowList.Add(rowConfig);
                        }

                        config.SheetList.Add(sheetConfig);
                    }

                    configs.Add(config);
                }

                return configs;
            }
        }

        public static List<DailyReportConfig> DailyReportConfig
        {
            get
            {
                var configs = new List<DailyReportConfig>();

                var elements = Root.Elements("DailyReport").ToList();

                foreach (var element in elements)
                {
                    var config = new DailyReportConfig()
                    {
                        RouteUniqueID = element.Attribute("RouteUniqueID").Value,
                        _2003 = element.Attribute("_2003").Value,
                        _2007 = element.Attribute("_2007").Value
                    };

                    var excelDefines = element.Elements("ExcelDefine").ToList();

                    foreach (var excelDefine in excelDefines)
                    {
                        config.ExcelDefineList.Add(new DailyReportExcelDefine()
                        {
                            CheckItemBeginRowIndex = int.Parse(excelDefine.Attribute("CheckItemBeginRowIndex").Value),
                            CheckItemEndRowIndex = int.Parse(excelDefine.Attribute("CheckItemEndRowIndex").Value),
                            ControlPointCellIndex = int.Parse(excelDefine.Attribute("ControlPointCellIndex").Value),
                            ControlPointRowIndex = int.Parse(excelDefine.Attribute("ControlPointRowIndex").Value)
                        });
                    }

                    var decimalDefines = element.Elements("DecimalDefine").ToList();

                    foreach (var decimalDefine in decimalDefines)
                    {
                        config.DecimalDefineList.Add(new DailyReportDecimalDefine()
                        {
                            CheckItem = decimalDefine.Attribute("CheckItem").Value,
                            Decimals = decimalDefine.Attribute("Decimals").Value
                        });
                    }

                    configs.Add(config);
                }

                return configs;
            }
        }
    }

    public class EReportConfig
    {
        public string RouteUniqueID { get; set; }

        public string _2003 { get; set; }

        public string _2007 { get; set; }

        public List<EReportSheetConfig> SheetList { get; set; }

        public EReportConfig()
        {
            SheetList = new List<EReportSheetConfig>();
        }
    }

    public class EReportSheetConfig
    {
        public int SheetIndex { get; set; }

        public int CheckUserRowIndex { get; set; }

        public int CheckUserCellIndex { get; set; }

        public List<EReportRowConfig> RowList { get; set; }

        public EReportSheetConfig()
        {
            RowList = new List<EReportRowConfig>();
        }
    }

    public class EReportRowConfig
    {
        public int RowIndex { get; set; }

        public List<EReportCellConfig> CellList { get; set; }

        public EReportRowConfig()
        {
            CellList = new List<EReportCellConfig>();
        }
    }

    public class EReportCellConfig
    {
        public int CellIndex { get; set; }

        public string ControlPoint { get; set; }

        public string Equipment { get; set; }

        public string CheckItem { get; set; }
    }

    public class DailyReportConfig
    {
        public string RouteUniqueID { get; set; }

        public string _2003 { get; set; }

        public string _2007 { get; set; }

        public List<DailyReportExcelDefine> ExcelDefineList { get; set; }

        public List<DailyReportDecimalDefine> DecimalDefineList { get; set; }

        public DailyReportConfig()
        {
            ExcelDefineList = new List<DailyReportExcelDefine>();
            DecimalDefineList = new List<DailyReportDecimalDefine>();
        }
    }

    public class DailyReportDecimalDefine
    {
        public string CheckItem { get; set; }

        public string Decimals { get; set; }
    }

    public class DailyReportExcelDefine
    {
        public int ControlPointRowIndex { get; set; }

        public int ControlPointCellIndex { get; set; }

        public int JobPrefixRowIndex
        {
            get
            {
                return 2;
            }
        }

        public int JobDetailRowIndex
        {
            get
            {
                return 3;
            }
        }

        public int JobDetailBeginCellIndex
        {
            get
            {
                return ControlPointCellIndex + 3;
            }
        }

        public int JobDetailEndCellIndex
        {
            get
            {
                return JobDetailBeginCellIndex + 5;
            }
        }

        public int CheckItemCellIndex
        {
            get
            {
                return ControlPointCellIndex + 1;
            }
        }

        public int CheckItemBeginRowIndex { get; set; }

        public int CheckItemEndRowIndex { get; set; }
    }
}
