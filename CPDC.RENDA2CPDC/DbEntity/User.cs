//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace CPDC.RENDA2CPDC.DbEntity
{
    using System;
    using System.Collections.Generic;
    
    public partial class User
    {
        public string ID { get; set; }
        public string OrganizationUniqueID { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public string UID { get; set; }
        public bool IsMobileUser { get; set; }
        public System.DateTime LastModifyTime { get; set; }
        public string LoginID { get; set; }
    }
}
