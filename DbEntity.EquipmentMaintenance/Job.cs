//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace DbEntity.EquipmentMaintenance
{
    using System;
    using System.Collections.Generic;
    
    public partial class Job
    {
        public string UniqueID { get; set; }
        public string RouteUniqueID { get; set; }
        public string Description { get; set; }
        public bool IsCheckBySeq { get; set; }
        public bool IsShowPrevRecord { get; set; }
        public bool IsNeedVerify { get; set; }
        public string CycleMode { get; set; }
        public int CycleCount { get; set; }
        public System.DateTime BeginDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public string BeginTime { get; set; }
        public string EndTime { get; set; }
        public string Remark { get; set; }
        public System.DateTime LastModifyTime { get; set; }
    }
}
