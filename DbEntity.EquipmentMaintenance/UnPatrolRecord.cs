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
    
    public partial class UnPatrolRecord
    {
        public string JobUniqueID { get; set; }
        public string BeginDate { get; set; }
        public string EndDate { get; set; }
        public string UnPatrolReasonUniqueID { get; set; }
        public string UnPatrolReasonID { get; set; }
        public string UnPatrolReasonDescription { get; set; }
        public string UnPatrolReasonRemark { get; set; }
        public string UserID { get; set; }
        public Nullable<System.DateTime> LastModifyTime { get; set; }
    }
}
