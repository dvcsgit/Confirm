using System.IO;
namespace Models.EquipmentMaintenance.FileManagement
{
    public class FileModel
    {
        public string UniqueID { get; set; }

        public string FileName { get; set; }

        public string Extension { get; set; }

        public string Display
        {
            get
            {
                return this.FileName + "." + this.Extension;
            }
        }

        public string FilePath { get; set; }

        public string FullFileName
        {
            get
            {
                return Path.Combine(FilePath, UniqueID + "." + this.Extension);
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
