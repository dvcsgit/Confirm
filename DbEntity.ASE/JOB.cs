//------------------------------------------------------------------------------
// <auto-generated>
//    這個程式碼是由範本產生。
//
//    對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//    如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace DbEntity.ASE
{
    using System;
    using System.Collections.Generic;
    
    public partial class JOB
    {
        public string UNIQUEID { get; set; }
        public string ROUTEUNIQUEID { get; set; }
        public string DESCRIPTION { get; set; }
        public string ISCHECKBYSEQ { get; set; }
        public string ISSHOWPREVRECORD { get; set; }
        public string ISNEEDVERIFY { get; set; }
        public string CYCLEMODE { get; set; }
        public Nullable<int> CYCLECOUNT { get; set; }
        public Nullable<System.DateTime> BEGINDATE { get; set; }
        public Nullable<System.DateTime> ENDDATE { get; set; }
        public Nullable<int> TIMEMODE { get; set; }
        public string BEGINTIME { get; set; }
        public string ENDTIME { get; set; }
        public string REMARK { get; set; }
        public Nullable<System.DateTime> LASTMODIFYTIME { get; set; }
        public string ISONLYWORKDAY { get; set; }
        public string MON { get; set; }
        public string TUE { get; set; }
        public string WED { get; set; }
        public string THU { get; set; }
        public string FRI { get; set; }
        public string SAT { get; set; }
        public string SUN { get; set; }
        public string ISINTERVAL { get; set; }
    }
}
