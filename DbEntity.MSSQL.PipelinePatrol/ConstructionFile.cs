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
    
    public partial class ConstructionFile
    {
        public string ConstructionUniqueID { get; set; }
        public int Seq { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
        public bool IsClosed { get; set; }
    }
}