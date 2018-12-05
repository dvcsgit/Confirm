using CPDC.RENDA2CPDC.DbEntity;
using DataAccess;
using DbEntity.MSSQL;
using DbEntity.MSSQL.PipelinePatrol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace CPDC.RENDA2CPDC
{
    public class TransHelper : IDisposable
    {
        public void Trans()
        {
            try
            {
                var organizationList = OrganizationDataAccessor.GetDownStreamOrganizationList("d95e536e-a37b-451c-a234-8327e74115dd", true);

                using (DbEntities db = new DbEntities())
                {
                    db.Organization.RemoveRange(db.Organization.Where(x => organizationList.Contains(x.UniqueID)).ToList());

                    db.SaveChanges();

                    db.User.RemoveRange(db.User.Where(x => organizationList.Contains(x.OrganizationUniqueID)).ToList());

                    db.SaveChanges();

                    using (RDbEntities rendaDb = new RDbEntities())
                    {
                        db.Organization.AddRange(rendaDb.Organization.ToList().Select(x => new DbEntity.MSSQL.Organization
                        {
                            UniqueID = x.UniqueID,
                            ID = x.ID,
                            Description = x.Description,
                            ManagerUserID = x.ManagerUserID,
                            ParentUniqueID = x.ParentUniqueID
                        }).ToList());

                        db.SaveChanges();

                        db.User.AddRange(rendaDb.User.Where(x=>x.OrganizationUniqueID!="*").ToList().Select(x => new DbEntity.MSSQL.User
                        {
                            ID = x.ID,
                            OrganizationUniqueID = x.OrganizationUniqueID,
                            Email = x.Email,
                            IsMobileUser = x.IsMobileUser,
                            LastModifyTime = x.LastModifyTime,
                            LoginID = x.LoginID,
                            Name = x.Name,
                            Password = "XXXX",
                            Title = x.Title,
                            UID = x.UID
                        }).ToList());

                        db.SaveChanges();
                    }
                }

                using (PDbEntities db = new PDbEntities())
                {
                    var keyList = new List<string>();

                    #region AbnormalReason, AbnormalReasonHandlingMethod
                    var abnormalReasonList = db.AbnormalReason.Where(x => organizationList.Contains(x.OrganizationUniqueID)).ToList();

                    keyList = abnormalReasonList.Select(x => x.UniqueID).ToList();

                    db.AbnormalReasonHandlingMethod.RemoveRange(db.AbnormalReasonHandlingMethod.Where(x => keyList.Contains(x.AbnormalReasonUniqueID)).ToList());

                    db.AbnormalReason.RemoveRange(abnormalReasonList);

                    db.SaveChanges();
                    #endregion

                    #region CheckItem, CheckItemAbnormalReason, CheckItemFeelOption
                    var checkItemList = db.CheckItem.Where(x => organizationList.Contains(x.OrganizationUniqueID)).ToList();

                    keyList = checkItemList.Select(x => x.UniqueID).ToList();

                    db.CheckItemAbnormalReason.RemoveRange(db.CheckItemAbnormalReason.Where(x => keyList.Contains(x.CheckItemUniqueID)).ToList());

                    db.CheckItemFeelOption.RemoveRange(db.CheckItemFeelOption.Where(x => keyList.Contains(x.CheckItemUniqueID)).ToList());

                    db.CheckItem.RemoveRange(checkItemList);

                    db.SaveChanges();
                    #endregion

                    #region HandlingMethod
                    db.HandlingMethod.RemoveRange(db.HandlingMethod.Where(x => organizationList.Contains(x.UniqueID)).ToList());
                    #endregion

                    #region Job, JobRoute
                    var jobList = db.Job.Where(x => organizationList.Contains(x.OrganizationUniqueID)).ToList();

                    keyList = jobList.Select(x => x.UniqueID).ToList();

                    db.JobRoute.RemoveRange(db.JobRoute.Where(x => keyList.Contains(x.JobUniqueID)).ToList());

                    db.Job.RemoveRange(jobList);

                    db.SaveChanges();
                    #endregion

                    #region Pipeline, PipelineLocus, PipelineSpecValue
                    var pipelineList = db.Pipeline.Where(x => organizationList.Contains(x.OrganizationUniqueID)).ToList();

                    keyList = pipelineList.Select(x => x.UniqueID).ToList();

                    db.PipelineLocus.RemoveRange(db.PipelineLocus.Where(x => keyList.Contains(x.PipelineUniqueID)).ToList());

                    db.PipelineSpecValue.RemoveRange(db.PipelineSpecValue.Where(x => keyList.Contains(x.PipelineUniqueID)).ToList());

                    db.Pipeline.RemoveRange(pipelineList);

                    db.SaveChanges();
                    #endregion

                    #region PipelineSpec, PipelineSpecOption
                    var specList = db.PipelineSpec.Where(x => organizationList.Contains(x.OrganizationUniqueID)).ToList();

                    keyList = specList.Select(x => x.UniqueID).ToList();

                    db.PipelineSpecOption.RemoveRange(db.PipelineSpecOption.Where(x => keyList.Contains(x.SpecUniqueID)).ToList());

                    db.PipelineSpec.RemoveRange(specList);

                    db.SaveChanges();
                    #endregion

                    #region PipePoint, PipePointCheckItem
                    var pipePointList = db.PipePoint.Where(x => organizationList.Contains(x.OrganizationUniqueID)).ToList();

                    keyList = pipePointList.Select(x => x.UniqueID).ToList();

                    db.PipePointCheckItem.RemoveRange(db.PipePointCheckItem.Where(x => keyList.Contains(x.PipePointUniqueID)).ToList());

                    db.PipePoint.RemoveRange(pipePointList);

                    db.SaveChanges();
                    #endregion

                    #region Route, RouteCheckPoint, RouteCheckPointCheckItem, RoutePipeline
                    var routeList = db.Route.Where(x => organizationList.Contains(x.OrganizationUniqueID)).ToList();

                    keyList = routeList.Select(x => x.UniqueID).ToList();

                    db.RouteCheckPoint.RemoveRange(db.RouteCheckPoint.Where(x => keyList.Contains(x.RouteUniqueID)).ToList());

                    db.RouteCheckPointCheckItem.RemoveRange(db.RouteCheckPointCheckItem.Where(x => keyList.Contains(x.RouteUniqueID)).ToList());

                    db.RoutePipeline.RemoveRange(db.RoutePipeline.Where(x => keyList.Contains(x.RouteUniqueID)).ToList());

                    db.Route.RemoveRange(routeList);

                    db.SaveChanges();
                    #endregion

                    using (RPDbEntities rendaDb = new RPDbEntities())
                    {
                        db.AbnormalReason.AddRange(rendaDb.AbnormalReason.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.AbnormalReason
                        {
                            UniqueID = x.UniqueID,
                            ID = x.ID,
                            OrganizationUniqueID = x.OrganizationUniqueID,
                            AbnormalType = x.AbnormalType,
                            Description = x.Description,
                            LastModifyTime = x.LastModifyTime
                        }).ToList());

                        db.SaveChanges();

                        db.AbnormalReasonHandlingMethod.AddRange(rendaDb.AbnormalReasonHandlingMethod.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.AbnormalReasonHandlingMethod
                        {
                            AbnormalReasonUniqueID = x.AbnormalReasonUniqueID,
                            HandlingMethodUniqueID = x.HandlingMethodUniqueID
                        }).ToList());

                        db.SaveChanges();

                        //須加判斷
                        db.ArriveRecord.AddRange(rendaDb.ArriveRecord.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.ArriveRecord
                        {
                            ArriveDate = x.ArriveDate,
                            ArriveTime = x.ArriveTime,
                            JobDescription = x.JobDescription,
                            JobID = x.JobID,
                            JobUniqueID = x.JobUniqueID,
                            LAT = x.LAT,
                            LNG = x.LNG,
                            MinTimeSpan = x.MinTimeSpan,
                            OrganizationUniqueID = x.OrganizationUniqueID,
                            PipePointDescription = x.PipePointDescription,
                            PipePointID = x.PipePointID,
                            PipePointUniqueID = x.PipePointUniqueID,
                            Remark = x.Remark,
                            RouteID = x.RouteID,
                            RouteName = x.RouteName,
                            RouteUniqueID = x.RouteUniqueID,
                            TimeSpanAbnormalReasonDescription = x.TimeSpanAbnormalReasonDescription,
                            TimeSpanAbnormalReasonID = x.TimeSpanAbnormalReasonID,
                            TimeSpanAbnormalReasonRemark = x.TimeSpanAbnormalReasonRemark,
                            TimeSpanAbnormalReasonUniqueID = x.TimeSpanAbnormalReasonUniqueID,
                            TotalTimeSpan = x.TotalTimeSpan,
                            UniqueID = x.UniqueID,
                            UserID = x.UserID,
                            UserName = x.UserName
                        }).ToList());

                        db.SaveChanges();

                        //須加判斷
                        db.ArriveRecordPhoto.AddRange(rendaDb.ArriveRecordPhoto.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.ArriveRecordPhoto
                        {
                            ArriveRecordUniqueID = x.ArriveRecordUniqueID,
                            Extension = x.Extension,
                            Seq = x.Seq
                        }).ToList());

                        db.SaveChanges();

                        db.CheckItem.AddRange(rendaDb.CheckItem.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.CheckItem
                        {
                            AccumulationBase = x.AccumulationBase,
                            CheckType = x.CheckType,
                            Description = x.Description,
                            ID = x.ID,
                            IsAccumulation = x.IsAccumulation,
                            IsFeelItem = x.IsFeelItem,
                            LastModifyTime = x.LastModifyTime,
                            LowerAlertLimit = x.LowerAlertLimit,
                            LowerLimit = x.LowerLimit,
                            OrganizationUniqueID = x.OrganizationUniqueID,
                            Remark = x.Remark,
                            UniqueID = x.UniqueID,
                            Unit = x.Unit,
                            UpperAlertLimit = x.UpperAlertLimit,
                            UpperLimit = x.UpperLimit
                        }).ToList());

                        db.SaveChanges();

                        db.CheckItemAbnormalReason.AddRange(rendaDb.CheckItemAbnormalReason.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.CheckItemAbnormalReason
                        {
                            CheckItemUniqueID = x.CheckItemUniqueID,
                            AbnormalReasonUniqueID = x.AbnormalReasonUniqueID
                        }).ToList());

                        db.SaveChanges();

                        db.CheckItemFeelOption.AddRange(rendaDb.CheckItemFeelOption.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.CheckItemFeelOption
                        {
                            CheckItemUniqueID = x.CheckItemUniqueID,
                            Description = x.Description,
                            IsAbnormal = x.IsAbnormal,
                            Seq = x.Seq,
                            UniqueID = x.UniqueID
                        }).ToList());

                        db.SaveChanges();

                        //須加判斷
                        db.CheckResult.AddRange(rendaDb.CheckResult.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.CheckResult
                        {
                            ArriveRecordUniqueID = x.ArriveRecordUniqueID,
                            CheckDate = x.CheckDate,
                            CheckItemDescription = x.CheckItemDescription,
                            CheckItemID = x.CheckItemID,
                            CheckItemUniqueID = x.CheckItemUniqueID,
                            CheckTime = x.CheckTime,
                            FeelOptionDescription = x.FeelOptionDescription,
                            FeelOptionUniqueID = x.FeelOptionUniqueID,
                            IsAbnormal = x.IsAbnormal,
                            IsAlert = x.IsAlert,
                            JobDescription = x.JobDescription,
                            JobID = x.JobID,
                            JobUniqueID = x.JobUniqueID,
                            LowerAlertLimit = x.LowerAlertLimit,
                            LowerLimit = x.LowerLimit,
                            NetValue = x.NetValue,
                            OrganizationUniqueID = x.OrganizationUniqueID,
                            PipePointDescription = x.PipePointDescription,
                            PipePointID = x.PipePointID,
                            PipePointUniqueID = x.PipePointUniqueID,
                            Remark = x.Remark,
                            Result = x.Result,
                            RouteID = x.RouteID,
                            RouteName = x.RouteName,
                            RouteUniqueID = x.RouteUniqueID,
                            UniqueID = x.UniqueID,
                            Unit = x.Unit,
                            UpperAlertLimit = x.UpperAlertLimit,
                            UpperLimit = x.UpperLimit,
                            Value = x.Value
                        }).ToList());

                        db.SaveChanges();

                        //須加判斷
                        db.CheckResultAbnormalReason.AddRange(rendaDb.CheckResultAbnormalReason.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.CheckResultAbnormalReason
                        {
                            AbnormalReasonDescription = x.AbnormalReasonDescription,
                            AbnormalReasonID = x.AbnormalReasonID,
                            AbnormalReasonRemark = x.AbnormalReasonRemark,
                            AbnormalReasonUniqueID = x.AbnormalReasonUniqueID,
                            CheckResultUniqueID = x.CheckResultUniqueID
                        }).ToList());

                        db.SaveChanges();

                        //須加判斷
                        db.CheckResultHandlingMethod.AddRange(rendaDb.CheckResultHandlingMethod.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.CheckResultHandlingMethod
                        {
                            AbnormalReasonUniqueID = x.AbnormalReasonUniqueID,
                            CheckResultUniqueID = x.CheckResultUniqueID,
                            HandlingMethodDescription = x.HandlingMethodDescription,
                            HandlingMethodID = x.HandlingMethodID,
                            HandlingMethodRemark = x.HandlingMethodRemark,
                            HandlingMethodUniqueID = x.HandlingMethodUniqueID
                        }).ToList());

                        db.SaveChanges();

                        //須加判斷
                        db.CheckResultPhoto.AddRange(rendaDb.CheckResultPhoto.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.CheckResultPhoto
                        {
                            CheckResultUniqueID = x.CheckResultUniqueID,
                            Extension = x.Extension,
                            Seq = x.Seq
                        }).ToList());

                        db.SaveChanges();

                        //須加判斷
                        db.Construction.AddRange(rendaDb.Construction.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.Construction
                        {
                            Address = x.Address,
                            BeginDate = x.BeginDate,
                            ClosedRemark = x.ClosedRemark,
                            ClosedTime = x.ClosedTime,
                            ClosedUserID = x.ClosedUserID,
                            ConstructionFirmRemark = x.ConstructionFirmRemark,
                            ConstructionFirmUniqueID = x.ConstructionFirmUniqueID,
                            ConstructionTypeRemark = x.ConstructionTypeRemark,
                            ConstructionTypeUniqueID = x.ConstructionTypeUniqueID,
                            CreateTime = x.CreateTime,
                            CreateUserID = x.CreateUserID,
                            Description = x.Description,
                            EndDate = x.EndDate,
                            InspectionUniqueID = x.InspectionUniqueID,
                            LAT = x.LAT,
                            LNG = x.LNG,
                            PipePointUniqueID = x.PipePointUniqueID,
                            UniqueID = x.UniqueID,
                            VHNO = x.VHNO
                        }).ToList());

                        db.SaveChanges();

                        //須加判斷
                        db.ConstructionFile.AddRange(rendaDb.ConstructionFile.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.ConstructionFile
                        {
                            ConstructionUniqueID = x.ConstructionUniqueID,
                            Extension = x.Extension,
                            FileName = x.FileName,
                            IsClosed = x.IsClosed,
                            Seq = x.Seq
                        }).ToList());

                        db.SaveChanges();

                        //須加判斷
                        db.ConstructionPhoto.AddRange(rendaDb.ConstructionPhoto.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.ConstructionPhoto
                        {
                            ConstructionUniqueID = x.ConstructionUniqueID,
                            Extension = x.Extension,
                            IsClosed = x.IsClosed,
                            Seq = x.Seq
                        }).ToList());

                        db.SaveChanges();

                        var constructionFirmList = rendaDb.ConstructionFirm.ToList();

                        foreach (var constructionFirm in constructionFirmList)
                        {
                            var query = db.ConstructionFirm.FirstOrDefault(x => x.UniqueID == constructionFirm.UniqueID);

                            if (query == null)
                            {
                                db.ConstructionFirm.Add(new DbEntity.MSSQL.PipelinePatrol.ConstructionFirm()
                                {
                                    UniqueID = constructionFirm.UniqueID,
                                    ID = constructionFirm.ID,
                                    Name = constructionFirm.Name
                                });
                            }
                            else
                            {
                                query.ID = constructionFirm.ID;
                                query.Name = constructionFirm.Name;
                            }
                        }

                        db.SaveChanges();

                        var constructionTypeList = rendaDb.ConstructionType.ToList();

                        foreach (var constructionType in constructionTypeList)
                        {
                            var query = db.ConstructionType.FirstOrDefault(x => x.UniqueID == constructionType.UniqueID);

                            if (query == null)
                            {
                                db.ConstructionType.Add(new DbEntity.MSSQL.PipelinePatrol.ConstructionType()
                                {
                                    UniqueID = constructionType.UniqueID,
                                    ID = constructionType.ID,
                                    Description = constructionType.Description
                                });
                            }
                            else
                            {
                                query.ID = constructionType.ID;
                                query.Description = constructionType.Description;
                            }
                        }

                        db.SaveChanges();

                        //須加判斷
                        db.Dialog.AddRange(rendaDb.Dialog.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.Dialog
                        {
                            ConstructionUniqueID = x.ConstructionUniqueID,
                            Description = x.Description,
                            Extension = x.Extension,
                            InspectionUniqueID = x.InspectionUniqueID,
                            PipelineAbnormalUniqueID = x.PipelineAbnormalUniqueID,
                            Subject = x.Subject,
                            UniqueID = x.UniqueID
                        }).ToList());

                        db.SaveChanges();

                        db.HandlingMethod.AddRange(rendaDb.HandlingMethod.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.HandlingMethod
                        {
                            Description = x.Description,
                            HandlingMethodType = x.HandlingMethodType,
                            ID = x.ID,
                            LastModifyTime = x.LastModifyTime,
                            OrganizationUniqueID = x.OrganizationUniqueID,
                            UniqueID = x.UniqueID
                        }).ToList());

                        db.SaveChanges();

                        //須加判斷
                        db.Inspection.AddRange(rendaDb.Inspection.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.Inspection
                        {
                            Address = x.Address,
                            BeginDate = x.BeginDate,
                            ConstructionFirmRemark = x.ConstructionFirmRemark,
                            ConstructionFirmUniqueID = x.ConstructionFirmUniqueID,
                            ConstructionTypeRemark = x.ConstructionTypeRemark,
                            ConstructionTypeUniqueID = x.ConstructionTypeUniqueID,
                            CreateTime = x.CreateTime,
                            CreateUserID = x.CreateUserID,
                            Description = x.Description,
                            EndDate = x.EndDate,
                            LAT = x.LAT,
                            LNG = x.LNG,
                            PipePointUniqueID = x.PipePointUniqueID,
                            UniqueID = x.UniqueID,
                            VHNO = x.VHNO
                        }).ToList());

                        db.SaveChanges();

                        //須加判斷
                        db.InspectionPhoto.AddRange(rendaDb.InspectionPhoto.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.InspectionPhoto
                        {
                            Extension = x.Extension,
                            InspectionUniqueID = x.InspectionUniqueID,
                            Seq = x.Seq
                        }).ToList());

                        db.SaveChanges();

                        //須加判斷
                        db.InspectionUser.AddRange(rendaDb.InspectionUser.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.InspectionUser
                        {
                            InspectionUniqueID = x.InspectionUniqueID,
                            InspectTime = x.InspectTime,
                            Remark = x.Remark,
                            UserID = x.UserID
                        }).ToList());

                        db.SaveChanges();

                        db.Job.AddRange(rendaDb.Job.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.Job
                        {
                            BeginDate = x.BeginDate,
                            BeginTime = x.BeginTime,
                            CycleCount = x.CycleCount,
                            CycleMode = x.CycleMode,
                            Description = x.Description,
                            EndDate = x.EndDate,
                            EndTime = x.EndTime,
                            ID = x.ID,
                            IsCheckBySeq = x.IsCheckBySeq,
                            IsNeedVerify = x.IsNeedVerify,
                            IsShowPrevRecord = x.IsShowPrevRecord,
                            LastModifyTime = x.LastModifyTime,
                            OrganizationUniqueID = x.OrganizationUniqueID,
                            TimeMode = x.TimeMode,
                            UniqueID = x.UniqueID
                        }).ToList());

                        db.SaveChanges();

                        db.JobRoute.AddRange(rendaDb.JobRoute.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.JobRoute
                        {
                            JobUniqueID = x.JobUniqueID,
                            RouteUniqueID = x.RouteUniqueID
                        }).ToList());

                        db.SaveChanges();

                        //須加判斷
                        db.JobUserLocus.AddRange(rendaDb.JobUserLocus.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.JobUserLocus
                        {
                            CheckDate = x.CheckDate,
                            CheckTime = x.CheckTime,
                            JobUniqueID = x.JobUniqueID,
                            LAT = x.LAT,
                            LNG = x.LNG,
                            RouteUniqueID = x.RouteUniqueID,
                            UserID = x.UserID
                        }).ToList());

                        db.SaveChanges();

                        //須加判斷
                        db.Message.AddRange(rendaDb.Message.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.Message
                        {
                            DialogUniqueID = x.DialogUniqueID,
                            Extension = x.Extension,
                            Message1 = x.Message1,
                            MessageTime = x.MessageTime,
                            Seq = x.Seq,
                            UserID = x.UserID
                        }).ToList());

                        db.SaveChanges();

                        //須加判斷
                        db.OnlineUserLocus.AddRange(rendaDb.OnlineUserLocus.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.OnlineUserLocus
                        {
                            JobUniqueID = x.JobUniqueID,
                            LAT = x.LAT,
                            LNG = x.LNG,
                            RouteUniqueID = x.RouteUniqueID,
                            UpdateTime = x.UpdateTime,
                            UserID = x.UserID
                        }).ToList());

                        db.SaveChanges();

                        db.Pipeline.AddRange(rendaDb.Pipeline.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.Pipeline
                        {
                            Color = x.Color,
                            ID = x.ID,
                            LastModifyTime = x.LastModifyTime,
                            OrganizationUniqueID = x.OrganizationUniqueID,
                            UniqueID = x.UniqueID
                        }).ToList());

                        db.SaveChanges();

                        db.PipelineLocus.AddRange(rendaDb.PipelineLocus.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.PipelineLocus
                        {
                            PipelineUniqueID = x.PipelineUniqueID,
                            LAT = x.LAT,
                            LNG = x.LNG,
                            Seq = x.Seq
                        }).ToList());

                        db.SaveChanges();

                        db.PipelineSpecValue.AddRange(rendaDb.PipelineSpecValue.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.PipelineSpecValue
                        {
                            PipelineUniqueID = x.PipelineUniqueID,
                            Seq = x.Seq,
                            SpecOptionUniqueID = x.SpecOptionUniqueID,
                            SpecUniqueID = x.SpecUniqueID,
                            Value = x.Value
                        }).ToList());

                        db.SaveChanges();

                        //須加判斷
                        db.PipelineAbnormal.AddRange(rendaDb.PipelineAbnormal.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.PipelineAbnormal
                        {
                            AbnormalReasonRemark = x.AbnormalReasonRemark,
                            AbnormalReasonUniqueID = x.AbnormalReasonUniqueID,
                            Address = x.Address,
                            ClosedRemark = x.ClosedRemark,
                            ClosedTime = x.ClosedTime,
                            ClosedUserID = x.ClosedUserID,
                            CreateTime = x.CreateTime,
                            CreateUserID = x.CreateUserID,
                            Description = x.Description,
                            LAT = x.LAT,
                            LNG = x.LNG,
                            PipePointUniqueID = x.PipePointUniqueID,
                            UniqueID = x.UniqueID,
                            VHNO = x.VHNO
                        }).ToList());

                        db.SaveChanges();

                        //須加判斷
                        db.PipelineAbnormalFile.AddRange(rendaDb.PipelineAbnormalFile.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.PipelineAbnormalFile
                        {
                            Extension = x.Extension,
                            FileName = x.FileName,
                            PipelineAbnormalUniqueID = x.PipelineAbnormalUniqueID,
                            Seq = x.Seq
                        }).ToList());

                        db.SaveChanges();

                        //須加判斷
                        db.PipelineAbnormalPhoto.AddRange(rendaDb.PipelineAbnormalPhoto.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.PipelineAbnormalPhoto
                        {
                            Extension = x.Extension,
                            PipelineAbnormalUniqueID = x.PipelineAbnormalUniqueID,
                            Seq = x.Seq
                        }).ToList());

                        db.SaveChanges();

                        db.PipelineSpec.AddRange(rendaDb.PipelineSpec.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.PipelineSpec
                        {
                            Description = x.Description,
                            OrganizationUniqueID = x.OrganizationUniqueID,
                            Type = x.Type,
                            UniqueID = x.UniqueID
                        }).ToList());

                        db.SaveChanges();

                        db.PipelineSpecOption.AddRange(rendaDb.PipelineSpecOption.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.PipelineSpecOption
                        {
                            Description = x.Description,
                            SpecUniqueID = x.SpecUniqueID,
                            Seq = x.Seq,
                            UniqueID = x.UniqueID
                        }).ToList());

                        db.SaveChanges();

                        db.PipePoint.AddRange(rendaDb.PipePoint.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.PipePoint
                        {
                            ID = x.ID,
                            IsFeelItemDefaultNormal = x.IsFeelItemDefaultNormal,
                            LastModifyTime = x.LastModifyTime,
                            LAT = x.LAT,
                            LNG = x.LNG,
                            Name = x.Name,
                            OrganizationUniqueID = x.OrganizationUniqueID,
                            PointType = x.PointType,
                            Remark = x.Remark,
                            UniqueID = x.UniqueID
                        }).ToList());

                        db.SaveChanges();

                        db.PipePointCheckItem.AddRange(rendaDb.PipePointCheckItem.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.PipePointCheckItem
                        {
                            AccumulationBase = x.AccumulationBase,
                            PipePointUniqueID = x.PipePointUniqueID,
                            CheckItemUniqueID = x.CheckItemUniqueID,
                            IsInherit = x.IsInherit,
                            LowerAlertLimit = x.LowerAlertLimit,
                            LowerLimit = x.LowerLimit,
                            Remark = x.Remark,
                            Unit = x.Unit,
                            UpperAlertLimit = x.UpperAlertLimit,
                            UpperLimit = x.UpperLimit
                        }).ToList());

                        db.SaveChanges();

                        db.Route.AddRange(rendaDb.Route.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.Route
                        {
                            ID = x.ID,
                            LastModifyTime = x.LastModifyTime,
                            Name = x.Name,
                            OrganizationUniqueID = x.OrganizationUniqueID,
                            Remark = x.Remark,
                            UniqueID = x.UniqueID
                        }).ToList());

                        db.SaveChanges();

                        db.RouteCheckPoint.AddRange(rendaDb.RouteCheckPoint.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.RouteCheckPoint
                        {
                            RouteUniqueID = x.RouteUniqueID,
                            MinTimeSpan = x.MinTimeSpan,
                            PipePointUniqueID = x.PipePointUniqueID,
                            Seq = x.Seq
                        }).ToList());

                        db.SaveChanges();

                        db.RouteCheckPointCheckItem.AddRange(rendaDb.RouteCheckPointCheckItem.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.RouteCheckPointCheckItem
                        {
                            CheckItemUniqueID = x.CheckItemUniqueID,
                            PipePointUniqueID = x.PipePointUniqueID,
                            RouteUniqueID = x.RouteUniqueID,
                            Seq = x.Seq
                        }).ToList());

                        db.SaveChanges();

                        db.RoutePipeline.AddRange(rendaDb.RoutePipeline.ToList().Select(x => new DbEntity.MSSQL.PipelinePatrol.RoutePipeline
                        {
                            PipelineUniqueID = x.PipelineUniqueID,
                            RouteUniqueID = x.RouteUniqueID
                        }).ToList());

                        db.SaveChanges();

                        var timeSpanAbnormalReasonList = rendaDb.TimeSpanAbnormalReason.ToList();

                        foreach (var timeSpanAbnormalReason in timeSpanAbnormalReasonList)
                        {
                            var query = db.TimeSpanAbnormalReason.FirstOrDefault(x => x.UniqueID == timeSpanAbnormalReason.UniqueID);

                            if (query == null)
                            {
                                db.TimeSpanAbnormalReason.Add(new DbEntity.MSSQL.PipelinePatrol.TimeSpanAbnormalReason()
                                {
                                    UniqueID = timeSpanAbnormalReason.UniqueID,
                                    ID = timeSpanAbnormalReason.ID,
                                    Description = timeSpanAbnormalReason.Description,
                                    LastModifyTime = timeSpanAbnormalReason.LastModifyTime
                                });
                            }
                            else
                            {
                                query.ID = timeSpanAbnormalReason.ID;
                                query.Description = timeSpanAbnormalReason.Description;
                                query.LastModifyTime = timeSpanAbnormalReason.LastModifyTime;
                            }
                        }

                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        #region IDisposable

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {

                }
            }

            _disposed = true;
        }

        ~TransHelper()
        {
            Dispose(false);
        }

        #endregion
    }
}
