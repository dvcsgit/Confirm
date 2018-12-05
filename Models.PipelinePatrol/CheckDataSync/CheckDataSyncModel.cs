using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.CheckDataSync
{
    public class CheckDataSyncModel
    {
        public string Test { get; set; }
        public List<CheckDataSyncItem> CheckDataSync { get; set; }
        public CheckDataSyncModel()
        {
            this.CheckDataSync = new List<CheckDataSyncItem>();
        }
    }

    public class CheckDataSyncItem
    {
        public string MarkerFormConsts { get; set; }
        public Dictionary<string,string> VHNOList { get; set; }
        
    }
}
