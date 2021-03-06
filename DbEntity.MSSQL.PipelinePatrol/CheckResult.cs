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
    
    public partial class CheckResult
    {
        public string UniqueID { get; set; }
        public string ArriveRecordUniqueID { get; set; }
        public string OrganizationUniqueID { get; set; }
        public string JobUniqueID { get; set; }
        public string JobID { get; set; }
        public string JobDescription { get; set; }
        public string RouteUniqueID { get; set; }
        public string RouteID { get; set; }
        public string RouteName { get; set; }
        public string PipePointUniqueID { get; set; }
        public string PipePointID { get; set; }
        public string PipePointDescription { get; set; }
        public string CheckItemUniqueID { get; set; }
        public string CheckItemID { get; set; }
        public string CheckItemDescription { get; set; }
        public Nullable<double> LowerLimit { get; set; }
        public Nullable<double> LowerAlertLimit { get; set; }
        public Nullable<double> UpperAlertLimit { get; set; }
        public Nullable<double> UpperLimit { get; set; }
        public string Unit { get; set; }
        public string CheckDate { get; set; }
        public string CheckTime { get; set; }
        public string FeelOptionUniqueID { get; set; }
        public string FeelOptionDescription { get; set; }
        public Nullable<double> Value { get; set; }
        public Nullable<double> NetValue { get; set; }
        public bool IsAbnormal { get; set; }
        public bool IsAlert { get; set; }
        public string Result { get; set; }
        public string Remark { get; set; }
    }
}
