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
    
    public partial class ArriveRecord
    {
        public string UniqueID { get; set; }
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
        public string ArriveDate { get; set; }
        public string ArriveTime { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string Remark { get; set; }
        public Nullable<double> LNG { get; set; }
        public Nullable<double> LAT { get; set; }
        public Nullable<double> MinTimeSpan { get; set; }
        public Nullable<double> TotalTimeSpan { get; set; }
        public string TimeSpanAbnormalReasonUniqueID { get; set; }
        public string TimeSpanAbnormalReasonID { get; set; }
        public string TimeSpanAbnormalReasonDescription { get; set; }
        public string TimeSpanAbnormalReasonRemark { get; set; }
    }
}
