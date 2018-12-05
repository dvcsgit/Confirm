using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.DataSync
{
    public class PipePointFileModel
    {
        public string UniqueID { get; set; }

        public string Name { get; set; }

        public string Extension { get; set; }

        public string FileName
        {
            get
            {
                return string.Format("{0}.{1}", Name, Extension);
            }
        }
    }
}
