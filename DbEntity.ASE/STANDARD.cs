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
    
    public partial class STANDARD
    {
        public string UNIQUEID { get; set; }
        public string ORGANIZATIONUNIQUEID { get; set; }
        public string ID { get; set; }
        public string DESCRIPTION { get; set; }
        public string ISFEELITEM { get; set; }
        public string ISACCUMULATION { get; set; }
        public Nullable<decimal> LOWERLIMIT { get; set; }
        public Nullable<decimal> LOWERALERTLIMIT { get; set; }
        public Nullable<decimal> UPPERALERTLIMIT { get; set; }
        public Nullable<decimal> UPPERLIMIT { get; set; }
        public Nullable<decimal> ACCUMULATIONBASE { get; set; }
        public string UNIT { get; set; }
        public string REMARK { get; set; }
        public Nullable<System.DateTime> LASTMODIFYTIME { get; set; }
        public string MAINTENANCETYPE { get; set; }
        public Nullable<decimal> ACCUMULCATION_D { get; set; }
    }
}
