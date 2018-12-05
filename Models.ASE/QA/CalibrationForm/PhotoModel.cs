using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.CalibrationForm
{
    public class PhotoModel
    {
        public string FileName { get; set; }

        public string Name
        {
            get
            {
                return FileName.Substring(0, FileName.IndexOf("."));
            }
        }

        public int CheckItemSeq
        {
            get
            {
                return int.Parse(Name.Split('_')[1]);
            }
        }

        public int Seq
        {
            get
            {
                return int.Parse(Name.Split('_')[2]);
            }
        }
    }
}
