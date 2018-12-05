using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.DataSync
{
    public class ConstructionFileModel
    {
        public string ConstructionUniqueID { get; set; }

        public int Seq { get; set; }

        public string FileName { get; set; }

        //不需要,下次更版時刪除
        public string FilePath { get; set; }
    }
}
