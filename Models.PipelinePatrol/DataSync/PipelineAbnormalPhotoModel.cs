using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.DataSync
{
    public class PipelineAbnormalPhotoModel
    {
        public string PipelineAbnormalUniqueID { get; set; }

        public int Seq { get; set; }
        
        public string FileName { get; set; }

        public string FilePath { get; set; }
    }
}
