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
    
    public partial class PipelineAbnormal
    {
        public string UniqueID { get; set; }
        public string VHNO { get; set; }
        public string PipePointUniqueID { get; set; }
        public string AbnormalReasonUniqueID { get; set; }
        public string AbnormalReasonRemark { get; set; }
        public string Description { get; set; }
        public double LNG { get; set; }
        public double LAT { get; set; }
        public string Address { get; set; }
        public string CreateUserID { get; set; }
        public System.DateTime CreateTime { get; set; }
        public string ClosedUserID { get; set; }
        public Nullable<System.DateTime> ClosedTime { get; set; }
        public string ClosedRemark { get; set; }
    }
}
