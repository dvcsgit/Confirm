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
    
    public partial class File
    {
        public string UniqueID { get; set; }
        public string OrganizationUniqueID { get; set; }
        public string PipelineUniqueID { get; set; }
        public string PipePointUniqueID { get; set; }
        public string FolderUniqueID { get; set; }
        public string FileName { get; set; }
        public int Size { get; set; }
        public string Extension { get; set; }
        public bool IsDownload2Mobile { get; set; }
        public string UserID { get; set; }
        public System.DateTime LastModifyTime { get; set; }
    }
}
