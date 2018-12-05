using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EquipmentMaintenance.RouteManagement
{
    public class Job
    {
        public string UniqueID { get; set; }
        public string RouteUniqueID { get; set; }
        public string Description { get; set; }
        public bool IsCheckBySeq { get; set; }
        public bool IsShowPrevRecord { get; set; }
        public bool IsNeedVerify { get; set; }
        public string CycleMode { get; set; }
        public int CycleCount { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TimeMode { get; set; }
        public string BeginTime { get; set; }
        public string EndTime { get; set; }
        public string Remark { get; set; }
        public DateTime LastModifyTime { get; set; }
    }
}
