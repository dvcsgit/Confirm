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
    
    public partial class QA_CALIBRATIONFORMDETAIL
    {
        public string FORMUNIQUEID { get; set; }
        public int SEQ { get; set; }
        public string CHARACTERISTIC { get; set; }
        public string USINGRANGE { get; set; }
        public string RANGETOLERANCE { get; set; }
        public string CALIBRATIONPOINT { get; set; }
        public string UNIT { get; set; }
        public Nullable<decimal> STANDARD { get; set; }
        public string TOLERANCESYMBOL { get; set; }
        public decimal TOLERANCE { get; set; }
        public string TOLERANCEUNIT { get; set; }
        public Nullable<System.DateTime> CALDATE { get; set; }
        public Nullable<decimal> READINGVALUE { get; set; }
        public decimal TOLERANCEUNITRATE { get; set; }
    }
}
