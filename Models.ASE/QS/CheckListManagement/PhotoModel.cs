using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QS.CheckListManagement
{
    public class PhotoModel
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

        public string FormUniqueID { get; set; }

        public string CheckItemUniqueID { get; set; }

        public int CheckItemSeq { get; set; }

        public int Seq { get; set; }

        public string Extension { get; set; }

        public string FileName
        {
            get
            {
                return string.Format("{0}_{1}_{2}_{3}.{4}", FormUniqueID, CheckItemUniqueID, CheckItemSeq, Seq, Extension);
            }
        }

        public string FullFileName
        {
            get
            {
                return Path.Combine(Config.QSFileFolderPath, FileName);
            }
        }

        public bool IsSaved { get; set; }
    }
}
