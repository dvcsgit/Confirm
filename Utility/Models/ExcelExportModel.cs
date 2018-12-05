using System;
using System.IO;

namespace Utility.Models
{
    public class ExcelExportModel
    {
        public byte[] Data { get; set; }

        private string FilePath = string.Empty;

        private string DataName { get; set; }

        private Define.EnumExcelVersion ExcelVersion;

        public string FileName
        {
            get
            {
                return string.Format("{0}.{1}", DataName, Extension);
            }
        }

        public string ContentType
        {
            get
            {
                if (ExcelVersion == Define.EnumExcelVersion._2003)
                {
                    return Define.ExcelContentType_2003;
                }
                else if (ExcelVersion == Define.EnumExcelVersion._2007)
                {
                    return Define.ExcelContentType_2007;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string FullFileName
        {
            get
            {
                return Path.Combine(FilePath, FileName);
            }
        }

        private string Extension
        {
            get
            {
                if (ExcelVersion == Define.EnumExcelVersion._2003)
                {
                    return Define.ExcelExtension_2003;
                }
                else if (ExcelVersion == Define.EnumExcelVersion._2007)
                {
                    return Define.ExcelExtension_2007;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public ExcelExportModel(string DataName, Define.EnumExcelVersion ExcelVersion)
        {
            FilePath = Path.Combine(Config.TempFolder, Guid.NewGuid().ToString());

            this.DataName = DataName;

            this.ExcelVersion = ExcelVersion;

            if (!Directory.Exists(FilePath))
            {
                Directory.CreateDirectory(FilePath);
            }
        }
    }
}
