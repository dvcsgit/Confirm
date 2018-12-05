using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSI.AbnormalSummaryMail.Models
{
    public class CheckResultPhotoModel
    {
        public string UniqueID { get; set; }

        public int Seq { get; set; }

        public string Extension { get; set; }

        public string FileName
        {
            get
            {
                return string.Format("{0}_{1}.{2}", UniqueID, Seq, Extension);
            }
        }

        public string FullFileName
        {
            get
            {
                //return string.Format("http://http://203.72.132.14/FEM/EquipmentPatrolPhoto/{0}", FileName);
                return string.Format("http://172.28.128.165/FEM/EquipmentPatrolPhoto/{0}", FileName);
            }
        }
    }
}
