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
    
    public partial class RouteCheckPoint
    {
        public string RouteUniqueID { get; set; }
        public string PipePointUniqueID { get; set; }
        public Nullable<int> MinTimeSpan { get; set; }
        public int Seq { get; set; }
    }
}