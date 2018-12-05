﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class PDbEntities : DbContext
    {
        public PDbEntities()
            : base("name=PDbEntities")
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
        public virtual DbSet<CheckItem> CheckItem { get; set; }
        public virtual DbSet<CheckItemAbnormalReason> CheckItemAbnormalReason { get; set; }
        public virtual DbSet<CheckItemFeelOption> CheckItemFeelOption { get; set; }
        public virtual DbSet<CheckResult> CheckResult { get; set; }
        public virtual DbSet<CheckResultAbnormalReason> CheckResultAbnormalReason { get; set; }
        public virtual DbSet<CheckResultHandlingMethod> CheckResultHandlingMethod { get; set; }
        public virtual DbSet<CheckResultPhoto> CheckResultPhoto { get; set; }
        public virtual DbSet<Construction> Construction { get; set; }
        public virtual DbSet<ConstructionFile> ConstructionFile { get; set; }
        public virtual DbSet<ConstructionFirm> ConstructionFirm { get; set; }
        public virtual DbSet<ConstructionPhoto> ConstructionPhoto { get; set; }
        public virtual DbSet<ConstructionType> ConstructionType { get; set; }
        public virtual DbSet<Dialog> Dialog { get; set; }
        public virtual DbSet<File> File { get; set; }
        public virtual DbSet<Folder> Folder { get; set; }
        public virtual DbSet<HandlingMethod> HandlingMethod { get; set; }
        public virtual DbSet<Inspection> Inspection { get; set; }
        public virtual DbSet<InspectionPhoto> InspectionPhoto { get; set; }
        public virtual DbSet<InspectionUser> InspectionUser { get; set; }
        public virtual DbSet<Job> Job { get; set; }
        public virtual DbSet<JobRoute> JobRoute { get; set; }
        public virtual DbSet<JobUserLocus> JobUserLocus { get; set; }
        public virtual DbSet<Message> Message { get; set; }
        public virtual DbSet<OnlineUserLocus> OnlineUserLocus { get; set; }
        public virtual DbSet<Pipeline> Pipeline { get; set; }
        public virtual DbSet<PipelineAbnormal> PipelineAbnormal { get; set; }
        public virtual DbSet<PipelineAbnormalFile> PipelineAbnormalFile { get; set; }
        public virtual DbSet<PipelineAbnormalPhoto> PipelineAbnormalPhoto { get; set; }
        public virtual DbSet<PipelineLocus> PipelineLocus { get; set; }
        public virtual DbSet<PipelineSpec> PipelineSpec { get; set; }
        public virtual DbSet<PipelineSpecOption> PipelineSpecOption { get; set; }
        public virtual DbSet<PipelineSpecValue> PipelineSpecValue { get; set; }
        public virtual DbSet<PipePoint> PipePoint { get; set; }
        public virtual DbSet<PipePointCheckItem> PipePointCheckItem { get; set; }
        public virtual DbSet<Route> Route { get; set; }
        public virtual DbSet<RouteCheckPoint> RouteCheckPoint { get; set; }
        public virtual DbSet<RouteCheckPointCheckItem> RouteCheckPointCheckItem { get; set; }
        public virtual DbSet<RoutePipeline> RoutePipeline { get; set; }
        public virtual DbSet<TimeSpanAbnormalReason> TimeSpanAbnormalReason { get; set; }
        public virtual DbSet<UploadLog> UploadLog { get; set; }
        public virtual DbSet<UserExtra> UserExtra { get; set; }
        public virtual DbSet<Version> Version { get; set; }
        public virtual DbSet<View_PipePointCheckItem> View_PipePointCheckItem { get; set; }
    }
}
