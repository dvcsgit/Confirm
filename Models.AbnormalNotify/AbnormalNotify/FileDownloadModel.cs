using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.AbnormalNotify.AbnormalNotify
{
    public class FileDownloadModel
    {
        public string FormUniqueID { get; set; }

        public int Seq { get; set; }

        public string FileName { get; set; }

        public string Extension { get; set; }

        public string Display
        {
            get
            {
                return string.Format("{0}.{1}", FileName, Extension);
            }
        }

        public string FilePath
        {
            get
            {
                return Config.EquipmentMaintenanceFileFolderPath;
            }
        }

        public string FullFileName
        {
            get
            {
                return Path.Combine(FilePath, string.Format("{0}_{1}.{2}", FormUniqueID, Seq, Extension));
            }
        }

        public string ContentType
        {
            get
            {
                switch (Extension)
                {
                    case "png":
                        return "image/png";
                    case "jpg":
                    case "jpeg":
                        return "image/jpeg";
                    case "gif":
                        return "image/gif";
                    case "pdf":
                        return "application/pdf";
                    case "xls":
                        return "application/vnd.ms-excel";
                    case "xlsx":
                        return "vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    case "doc":
                        return "application/msword";
                    case "docx":
                        return "vnd.openxmlformats-officedocument.wordprocessingml.document";
                    case "tif":
                        return "image/tiff";
                    case "avi":
                        return "video/avi";
                    case "txt":
                        return "text/plain";
                    case "xml":
                        return "text/xml";
                    default:
                        return "application/octet-stream";
                }
            }
        }
    }
}
