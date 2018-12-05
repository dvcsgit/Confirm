using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.DataSync
{
    public class InspectionCloseFormInput
    {
        public string UniqueID { get; set; }
        
        
        // 結案人員
        public string UserID { get; set; }
        
        // 結案TRUE
        public bool IsClosed { get; set; }
        

        // 結案備註
        public string Remark { get; set; }
    }
}
