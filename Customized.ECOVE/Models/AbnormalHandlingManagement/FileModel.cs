using Models.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Customized.ECOVE.Models.AbnormalHandlingManagement
{
    public class FileModel
    {
        public string TempUniqueID { get; set; }

        public string TempFileName
        {
            get
            {
                return string.Format("{0}.{1}", TempUniqueID, Extension);
            }
        }

        public string TempFullFileName
        {
            get
            {
                return Path.Combine(Config.TempFolder, TempFileName);
            }
        }

        public string FullFileName
        {
            get
            {
                return Path.Combine(Config.EquipmentMaintenanceFileFolderPath, string.Format("{0}_{1}.{2}", AbnormalUniqueID, Seq, Extension));
            }
        }

        public string AbnormalUniqueID { get; set; }

        public int Seq { get; set; }

        public string Extension { get; set; }

        public string FileName { get; set; }

        public string Display
        {
            get
            {
                return string.Format("{0}.{1}", FileName, Extension);
            }
        }

        public int Size { get; set; }

        public string FileSize
        {
            get
            {
                var index = 0;

                var size = Size;

                while (size > 1024)
                {
                    size = size / 1024;
                    index++;
                }

                return string.Format("{0} {1}", size, Define.FileSizeDescription[index]);
            }
        }

        public DateTime UploadTime { get; set; }

        public string UploadTimeDisplay
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(UploadTime);
            }
        }

        public bool IsSaved { get; set; }
    }
}
