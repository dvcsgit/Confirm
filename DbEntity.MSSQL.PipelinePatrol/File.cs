//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DbEntity.MSSQL.PipelinePatrol
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
