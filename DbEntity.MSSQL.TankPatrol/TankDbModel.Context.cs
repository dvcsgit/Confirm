﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class TankDbEntities : DbContext
    {
        public TankDbEntities()
            : base("name=TankDbEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AbnormalReason> AbnormalReason { get; set; }
        public virtual DbSet<AbnormalReasonHandlingMethod> AbnormalReasonHandlingMethod { get; set; }
        public virtual DbSet<ArriveRecord> ArriveRecord { get; set; }
        public virtual DbSet<ArriveRecordPhoto> ArriveRecordPhoto { get; set; }
        public virtual DbSet<CheckItemAbnormalReason> CheckItemAbnormalReason { get; set; }
        public virtual DbSet<CheckItemFeelOption> CheckItemFeelOption { get; set; }
        public virtual DbSet<CheckResult> CheckResult { get; set; }
        public virtual DbSet<CheckResultAbnormalReason> CheckResultAbnormalReason { get; set; }
        public virtual DbSet<CheckResultHandlingMethod> CheckResultHandlingMethod { get; set; }
        public virtual DbSet<CheckResultPhoto> CheckResultPhoto { get; set; }
        public virtual DbSet<HandlingMethod> HandlingMethod { get; set; }
        public virtual DbSet<Island> Island { get; set; }
        public virtual DbSet<Port> Port { get; set; }
        public virtual DbSet<PortCheckItem> PortCheckItem { get; set; }
        public virtual DbSet<Station> Station { get; set; }
        public virtual DbSet<UnRFIDReason> UnRFIDReason { get; set; }
        public virtual DbSet<UploadLog> UploadLog { get; set; }
        public virtual DbSet<Version> Version { get; set; }
        public virtual DbSet<Option> Option { get; set; }
        public virtual DbSet<CheckItem> CheckItem { get; set; }
    }
}
