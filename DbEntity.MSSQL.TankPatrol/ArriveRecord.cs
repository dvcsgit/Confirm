//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace DbEntity.MSSQL.TankPatrol
{
    using System;
    using System.Collections.Generic;
    
    public partial class ArriveRecord
    {
        public string UniqueID { get; set; }
        public string OrganizationUniqueID { get; set; }
        public string OrganizationDescription { get; set; }
        public string StationUniqueID { get; set; }
        public string StationID { get; set; }
        public string StationDescription { get; set; }
        public string IslandUniqueID { get; set; }
        public string IslandID { get; set; }
        public string IslandDescription { get; set; }
        public string PortUniqueID { get; set; }
        public string PortID { get; set; }
        public string PortDescription { get; set; }
        public string CheckType { get; set; }
        public string TankNo { get; set; }
        public string Driver { get; set; }
        public string Owner { get; set; }
        public string LastTimeMaterial { get; set; }
        public string ThisTimeMaterial { get; set; }
        public string ArriveDate { get; set; }
        public string ArriveTime { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string UnRFIDReasonUniqueID { get; set; }
        public string UnRFIDReasonID { get; set; }
        public string UnRFIDReasonDescription { get; set; }
        public string UnRFIDReasonRemark { get; set; }
        public string SignExtension { get; set; }
    }
}