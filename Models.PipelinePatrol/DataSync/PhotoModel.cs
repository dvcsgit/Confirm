using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.PipelinePatrol.DataSync
{
    public class PhotoModel
    {
        public string UserID { get; set; }

        public string FileName
        {
            get
            {
                return string.Format("{0}.{1}", UserID, Extension);
            }
        }

        public string FilePath
        {
            get
            {
                return Path.Combine(Config.UserPhotoFolderPath, string.Format("{0}.{1}", FileUniqueID, Extension));
            }
        }

        public string FileUniqueID { get; set; }

        public string Extension { get; set; }

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
