//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace DbEntity.MSSQL
{
    using System;
    using System.Collections.Generic;
    
    public partial class WebPermission
    {
        public string ID { get; set; }
        public string ParentID { get; set; }
        public string Area { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Icon { get; set; }
        public int Seq { get; set; }
        public bool IsEnabled { get; set; }
    }
}
