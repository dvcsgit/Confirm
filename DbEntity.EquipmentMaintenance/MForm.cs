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
    
    public partial class MForm
    {
        public string UniqueID { get; set; }
        public string VHNO { get; set; }
        public string MJobUniqueID { get; set; }
        public System.DateTime CycleBeginDate { get; set; }
        public System.DateTime CycleEndDate { get; set; }
        public Nullable<System.DateTime> BeginDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public string UserID { get; set; }
        public string Status { get; set; }
    }
}
