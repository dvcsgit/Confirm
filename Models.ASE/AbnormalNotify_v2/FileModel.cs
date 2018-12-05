using Models.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.AbnormalNotify_v2
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

        public bool IsSaved { get; set; }

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

        public DateTime LastModifyTime { get; set; }

        public string LastModifyTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(LastModifyTime);
            }
        }
    }
}
