﻿//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace DbEntity.MSSQL.AbnormalNotify
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class ADbEntities : DbContext
    {
        public ADbEntities()
            : base("name=ADbEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<ANForm> ANForm { get; set; }
        public virtual DbSet<ANFormCCUser> ANFormCCUser { get; set; }
        public virtual DbSet<ANFormFile> ANFormFile { get; set; }
        public virtual DbSet<ANFormGroup> ANFormGroup { get; set; }
        public virtual DbSet<ANFormLog> ANFormLog { get; set; }
        public virtual DbSet<ANFormUser> ANFormUser { get; set; }
        public virtual DbSet<ANGroup> ANGroup { get; set; }
        public virtual DbSet<ANGroupCCUser> ANGroupCCUser { get; set; }
        public virtual DbSet<ANGroupUser> ANGroupUser { get; set; }
    }
}