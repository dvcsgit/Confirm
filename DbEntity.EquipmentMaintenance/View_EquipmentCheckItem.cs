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
    
    public partial class View_EquipmentCheckItem
    {
        public string EquipmentUniqueID { get; set; }
        public string PartUniqueID { get; set; }
        public string CheckItemUniqueID { get; set; }
        public string ID { get; set; }
        public string Description { get; set; }
        public bool IsFeelItem { get; set; }
        public Nullable<double> UpperLimit { get; set; }
        public Nullable<double> UpperAlertLimit { get; set; }
        public Nullable<double> LowerAlertLimit { get; set; }
        public Nullable<double> LowerLimit { get; set; }
        public string Unit { get; set; }
        public string Remark { get; set; }
    }
}
