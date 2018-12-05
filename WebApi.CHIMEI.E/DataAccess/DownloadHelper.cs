using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Utility;
using Utility.Models;
using WebApi.CHIMEI.E.Models;

namespace WebApi.CHIMEI.E.DataAccess
{
    public class DownloadHelper : IDisposable
    {
        private string Guid;

        private DownloadDataModel DataModel = new DownloadDataModel();

        public RequestResult Generate(DownloadFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                result = Init();

                if (result.IsSuccess)
                {
                    result = Query(Model);

                    if (result.IsSuccess)
                    {
                        result = GenerateSQLite();

                        if (result.IsSuccess)
                        {
                            result = GenerateZip();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        private RequestResult Init()
        {
            RequestResult result = new RequestResult();

            try
            {
                Guid = System.Guid.NewGuid().ToString();

                Directory.CreateDirectory(GeneratedFolderPath);

                System.IO.File.Copy(TemplateDbFilePath, GeneratedDbFilePath);

                result.Success();
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        private RequestResult Query(DownloadFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                var allCheckItemFeelOptionList = new List<CheckItemFeelOption>();
                var allCheckItemAbnormalReasonList = new List<CheckItemAbnormalReason>();
                var allAbnormalReasonList = new List<AbnormalReason>();
                var allAbnormalReasonHandlingMethodList = new List<AbnormalReasonHandlingMethod>();
                var allHandlingMethodList = new List<HandlingMethod>();
                var allJobEquipmentCheckItemList = new List<JobEquipmentCheckItem>();
                var allJobEquipmentList = new List<JobEquipment>();
                var allJobControlPointList = new List<JobControlPoint>();
                var allJobUserList = new List<JobUser>();
                var allJobList = new List<Job>();
                var allCheckResultList = new List<CheckResult>();
                var routeUniqueIDList = new List<string>();
                var allRouteEquipmentCheckItemList = new List<RouteEquipmentCheckItem>();
                var allRouteEquipmentList = new List<RouteEquipment>();
                var allRouteControlPointList = new List<RouteControlPoint>();
                var allRouteList = new List<Route>();
                var allControlPointList = new List<ControlPoint>();
                var allEquipmentList = new List<Equipment>();
                var allCheckItemList = new List<CheckItem>();
                var allEquipmentCheckItemList = new List<EquipmentCheckItem>();

                using (EDbEntities db = new EDbEntities())
                {
                    db.Database.CommandTimeout = 3600;

                    #region UnPatrolReason
                    DataModel.UnPatrolReasonList = db.UnPatrolReason.Select(x => new WebApi.CHIMEI.E.Models.UnPatrolReasonModel
                    {
                        UniqueID = x.UniqueID,
                        ID = x.ID,
                        Description = x.Description,
                        LastModifyTime = x.LastModifyTime
                    }).ToList();
                    #endregion

                    #region UnRFIDReason
                    DataModel.UnRFIDReasonList = db.UnRFIDReason.Select(x => new WebApi.CHIMEI.E.Models.UnRFIDReasonModel
                    {
                        UniqueID = x.UniqueID,
                        ID = x.ID,
                        Description = x.Description,
                        LastModifyTime = x.LastModifyTime
                    }).ToList();
                    #endregion

                    #region OverTimeReason
                    DataModel.OverTimeReasonList = db.OverTimeReason.Select(x => new WebApi.CHIMEI.E.Models.OverTimeReasonModel
                    {
                        UniqueID = x.UniqueID,
                        ID = x.ID,
                        Description = x.Description,
                        LastModifyTime = x.LastModifyTime
                    }).ToList();
                    #endregion

                    #region TimeSpanAbnormalReason
                    DataModel.TimeSpanAbnormalReasonList = db.TimeSpanAbnormalReason.Select(x => new WebApi.CHIMEI.E.Models.TimeSpanAbnormalReasonModel
                    {
                        UniqueID = x.UniqueID,
                        ID = x.ID,
                        Description = x.Description,
                        LastModifyTime = x.LastModifyTime
                    }).ToList();
                    #endregion

                    allCheckItemFeelOptionList = db.CheckItemFeelOption.ToList();
                    allCheckItemAbnormalReasonList = db.CheckItemAbnormalReason.ToList();
                    allAbnormalReasonList = db.AbnormalReason.ToList();
                    allAbnormalReasonHandlingMethodList = db.AbnormalReasonHandlingMethod.ToList();
                    allHandlingMethodList = db.HandlingMethod.ToList();
                    allJobEquipmentCheckItemList = db.JobEquipmentCheckItem.Where(x => Model.JobUniqueIDList.Contains(x.JobUniqueID)).ToList();
                    allJobEquipmentList = db.JobEquipment.Where(x => Model.JobUniqueIDList.Contains(x.JobUniqueID)).ToList();
                    allJobControlPointList = db.JobControlPoint.Where(x => Model.JobUniqueIDList.Contains(x.JobUniqueID)).ToList();
                    allJobUserList = db.JobUser.Where(x => Model.JobUniqueIDList.Contains(x.JobUniqueID)).ToList();
                    allJobList = db.Job.Where(x => Model.JobUniqueIDList.Contains(x.UniqueID)).ToList();
                    allCheckResultList = db.CheckResult.Where(x => Model.JobUniqueIDList.Contains(x.JobUniqueID)).ToList();

                    routeUniqueIDList = allJobList.Select(x => x.RouteUniqueID).Distinct().ToList();
                    allRouteEquipmentCheckItemList = db.RouteEquipmentCheckItem.Where(x => routeUniqueIDList.Contains(x.RouteUniqueID)).ToList();
                    allRouteEquipmentList = db.RouteEquipment.Where(x => routeUniqueIDList.Contains(x.RouteUniqueID)).ToList();
                    allRouteControlPointList = db.RouteControlPoint.Where(x => routeUniqueIDList.Contains(x.RouteUniqueID)).ToList();
                    allRouteList = db.Route.Where(x => routeUniqueIDList.Contains(x.UniqueID)).ToList();
                    allControlPointList = db.ControlPoint.ToList();
                    allEquipmentList = db.Equipment.ToList();
                    allCheckItemList = db.CheckItem.ToList();
                    allEquipmentCheckItemList = db.EquipmentCheckItem.ToList();
                }

                var allQuery = (from jec in allJobEquipmentCheckItemList
                                join je in allJobEquipmentList
                                on new { jec.JobUniqueID, jec.ControlPointUniqueID, jec.EquipmentUniqueID, jec.PartUniqueID } equals new { je.JobUniqueID, je.ControlPointUniqueID, je.EquipmentUniqueID, je.PartUniqueID }
                                join jp in allJobControlPointList
                                on new { jec.JobUniqueID, jec.ControlPointUniqueID } equals new { jp.JobUniqueID, jp.ControlPointUniqueID }
                                join j in allJobList
                                on jec.JobUniqueID equals j.UniqueID
                                join rec in allRouteEquipmentCheckItemList
                                on new { j.RouteUniqueID, jec.ControlPointUniqueID, jec.EquipmentUniqueID, jec.PartUniqueID, jec.CheckItemUniqueID } equals new { rec.RouteUniqueID, rec.ControlPointUniqueID, rec.EquipmentUniqueID, rec.PartUniqueID, rec.CheckItemUniqueID }
                                join re in allRouteEquipmentList
                                on new { j.RouteUniqueID, jec.ControlPointUniqueID, jec.EquipmentUniqueID, jec.PartUniqueID } equals new { re.RouteUniqueID, re.ControlPointUniqueID, re.EquipmentUniqueID, re.PartUniqueID }
                                join rp in allRouteControlPointList
                                on new { j.RouteUniqueID, jec.ControlPointUniqueID } equals new { rp.RouteUniqueID, rp.ControlPointUniqueID }
                                join r in allRouteList
                                on j.RouteUniqueID equals r.UniqueID
                                join p in allControlPointList
                                on jec.ControlPointUniqueID equals p.UniqueID
                                join e in allEquipmentList
                                on jec.EquipmentUniqueID equals e.UniqueID
                                join c in allCheckItemList
                                on jec.CheckItemUniqueID equals c.UniqueID
                                join ec in allEquipmentCheckItemList
                                on new { jec.EquipmentUniqueID, jec.PartUniqueID, jec.CheckItemUniqueID } equals new { ec.EquipmentUniqueID, ec.PartUniqueID, ec.CheckItemUniqueID }
                                join cr in allCheckResultList
                                on new { jec.JobUniqueID, jec.ControlPointUniqueID, jec.EquipmentUniqueID, jec.PartUniqueID, jec.CheckItemUniqueID } equals new { cr.JobUniqueID, cr.ControlPointUniqueID, cr.EquipmentUniqueID, cr.PartUniqueID, cr.CheckItemUniqueID } into tmpCheckResult
                                from cr in tmpCheckResult.DefaultIfEmpty()
                                select new JobContent
                                {
                                    JobUniqueID = jec.JobUniqueID,
                                    JobDescription = j.Description,
                                    RouteID = r.ID,
                                    RouteName = r.Name,
                                    TimeMode = j.TimeMode,
                                    BeginTime = j.BeginTime,
                                    EndTime = j.EndTime,
                                    JobRemark = j.Remark,
                                    ControlPointUniqueID = jec.ControlPointUniqueID,
                                    ControlPointID = p.ID,
                                    ControlPointDescription = p.Description,
                                    TagID = p.TagID,
                                    ControlPointRemark = p.Remark,
                                    ControlPointSeq = rp.Seq,
                                    EquipmentUniqueID = jec.EquipmentUniqueID,
                                    EquipmentID = e.ID,
                                    EquipmentName = e.Name,
                                    EquipmentSeq = re.Seq,
                                    CheckItemUniqueID = jec.CheckItemUniqueID,
                                    CheckItemID = c.ID,
                                    CheckItemDescription = c.Description,
                                    CheckItemRemark = ec.IsInherit ? c.Remark : ec.Remark,
                                    CheckItemUnit = ec.IsInherit ? c.Unit : ec.Unit,
                                    CheckItemSeq = rec.Seq,
                                    IsFeelItem = c.IsFeelItem,
                                    IsFeelItemDefaultNormal = e.IsFeelItemDefaultNormal,
                                    LowerLimit = ec.IsInherit ? c.LowerLimit : ec.LowerLimit,
                                    LowerAlertLimit = ec.IsInherit ? c.LowerAlertLimit : ec.LowerAlertLimit,
                                    UpperAlertLimit = ec.IsInherit ? c.UpperAlertLimit : ec.UpperAlertLimit,
                                    UpperLimit = ec.IsInherit ? c.UpperLimit : ec.UpperLimit,
                                    CheckResultUniqueID = cr != null ? cr.UniqueID : string.Empty
                                }).Where(x => string.IsNullOrEmpty(x.CheckResultUniqueID)).ToList();

                foreach (var uniqueID in Model.JobUniqueIDList)
                {
                    var query = allQuery.Where(x => x.JobUniqueID == uniqueID).ToList();

                    if (query.Count > 0)
                    {
                        var job = query.Select(x => new
                        {
                            x.JobUniqueID,
                            x.JobDescription,
                            x.RouteID,
                            x.RouteName,
                            x.TimeMode,
                            x.BeginTime,
                            x.EndTime,
                            x.JobRemark
                        }).Distinct().First();

                        var jobModel = new JobModel()
                        {
                            UniqueID = job.JobUniqueID,
                            JobDescription = job.JobDescription,
                            RouteID = job.RouteID,
                            RouteName = job.RouteName,
                            TimeMode = job.TimeMode,
                            BeginTime = job.BeginTime,
                            EndTime = job.EndTime,
                            IsCheckBySeq = false,
                            IsShowPrevRecord = false,
                            Remark = job.JobDescription,
                            UserList = (from x in allJobUserList
                                        where x.JobUniqueID == job.JobUniqueID
                                        select new UserModel
                                        {
                                            ID = x.UserID
                                        }).ToList()
                        };

                        #region ControlPoint
                        var controlPointList = query.Select(x => new
                        {
                            x.ControlPointUniqueID,
                            x.ControlPointID,
                            x.ControlPointDescription,
                            x.TagID,
                            x.ControlPointRemark,
                            x.ControlPointSeq
                        }).Distinct().ToList();

                        foreach (var controlPoint in controlPointList)
                        {
                            var controlPointModel = new ControlPointModel()
                            {
                                UniqueID = controlPoint.ControlPointUniqueID,
                                ID = controlPoint.ControlPointID,
                                Description = controlPoint.ControlPointDescription,
                                IsFeelItemDefaultNormal = true,
                                TagID = controlPoint.TagID,
                                Remark = controlPoint.ControlPointRemark,
                                Seq = controlPoint.ControlPointSeq
                            };

                            #region Equipment
                            var equipmentList = query.Where(x => x.ControlPointUniqueID == controlPoint.ControlPointUniqueID).Select(x => new
                            {
                                x.EquipmentUniqueID,
                                x.EquipmentID,
                                x.EquipmentName,
                                x.EquipmentSeq,
                                x.IsFeelItemDefaultNormal
                            }).Distinct().ToList();

                            foreach (var equipment in equipmentList)
                            {
                                var equipmentModel = new EquipmentModel()
                                {
                                    UniqueID = equipment.EquipmentUniqueID,
                                    ID = equipment.EquipmentID,
                                    Name = equipment.EquipmentName,
                                    IsFeelItemDefaultNormal = equipment.IsFeelItemDefaultNormal,
                                    Seq = equipment.EquipmentSeq
                                };

                                #region CheckItem
                                var checkItemList = query.Where(x => x.ControlPointUniqueID == controlPoint.ControlPointUniqueID && x.EquipmentUniqueID == equipment.EquipmentUniqueID).Select(x => new
                                {
                                    x.CheckItemUniqueID,
                                    x.CheckItemID,
                                    x.CheckItemDescription,
                                    x.IsFeelItem,
                                    x.LowerLimit,
                                    x.LowerAlertLimit,
                                    x.UpperAlertLimit,
                                    x.UpperLimit,
                                    x.CheckItemRemark,
                                    x.CheckItemUnit,
                                    x.CheckItemSeq
                                }).Distinct().ToList();

                                foreach (var checkItem in checkItemList)
                                {
                                    equipmentModel.CheckItemList.Add(new CheckItemModel()
                                    {
                                        UniqueID = checkItem.CheckItemUniqueID,
                                        ID = checkItem.CheckItemID,
                                        Description = checkItem.CheckItemDescription,
                                        IsFeelItem = checkItem.IsFeelItem,
                                        LowerLimit = checkItem.LowerLimit,
                                        LowerAlertLimit = checkItem.LowerAlertLimit,
                                        UpperAlertLimit = checkItem.UpperAlertLimit,
                                        UpperLimit = checkItem.UpperLimit,
                                        Remark = checkItem.CheckItemRemark,
                                        Unit = checkItem.CheckItemUnit,
                                        Seq = checkItem.CheckItemSeq,
                                        FeelOptionList = allCheckItemFeelOptionList.Where(x => x.CheckItemUniqueID == checkItem.CheckItemUniqueID).Select(x => new FeelOptionModel
                                        {
                                            UniqueID = x.UniqueID,
                                            Description = x.Description,
                                            IsAbnormal = x.IsAbnormal,
                                            Seq = x.Seq
                                        }).ToList(),
                                        AbnormalReasonList = (from ca in allCheckItemAbnormalReasonList
                                                              join a in allAbnormalReasonList
                                                              on ca.AbnormalReasonUniqueID equals a.UniqueID
                                                              where ca.CheckItemUniqueID == checkItem.CheckItemUniqueID
                                                              select new AbnormalReasonModel
                                                              {
                                                                  UniqueID = a.UniqueID,
                                                                  ID = a.ID,
                                                                  Description = a.Description,
                                                                  HandlingMethodList = (from ah in allAbnormalReasonHandlingMethodList
                                                                                        join h in allHandlingMethodList
                                                                                        on ah.HandlingMethodUniqueID equals h.UniqueID
                                                                                        where ah.AbnormalReasonUniqueID == a.UniqueID
                                                                                        select new HandlingMethodModel
                                                                                        {
                                                                                            UniqueID = h.UniqueID,
                                                                                            ID = h.ID,
                                                                                            Description = h.Description
                                                                                        }).ToList()
                                                              }).ToList()
                                    });
                                }
                                #endregion

                                controlPointModel.EquipmentList.Add(equipmentModel);
                            }
                            #endregion

                            jobModel.ControlPointList.Add(controlPointModel);
                        }
                        #endregion

                        DataModel.JobList.Add(jobModel);
                    }
                }

                using (DbEntities db = new DbEntities())
                {
                    foreach (var job in DataModel.JobList)
                    {
                        job.UserList = (from x in job.UserList
                                        join u in db.User
                                        on x.ID equals u.ID
                                        select new UserModel
                                        {
                                            ID = u.ID,
                                            Name = u.Name,
                                            Title = u.Title,
                                            Password = u.Password,
                                            UID = u.UID
                                        }).ToList();
                    }
                }

                result.Success();
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        private RequestResult GenerateSQLite()
        {
            RequestResult result = new RequestResult();

            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(this.SQLiteConnString))
                {
                    conn.Open();

                    using (SQLiteTransaction trans = conn.BeginTransaction())
                    {
                        #region AbnormalReason, AbnormalReasonHandlingMethod
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO AbnormalReason (UniqueID, ID, Description) ValueS (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", Define.OTHER);
                            cmd.Parameters.AddWithValue("ID", Define.OTHER);
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO AbnormalReasonHandlingMethod (AbnormalReasonUniqueID, HandlingMethodUniqueID) ValueS (@AbnormalReasonUniqueID, @HandlingMethodUniqueID)";

                            cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", Define.OTHER);
                            cmd.Parameters.AddWithValue("HandlingMethodUniqueID", Define.OTHER);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var abnormalReason in DataModel.AbnormalReasonList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO AbnormalReason (UniqueID, ID, Description) ValueS (@UniqueID, @ID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", abnormalReason.UniqueID);
                                cmd.Parameters.AddWithValue("ID", abnormalReason.ID);
                                cmd.Parameters.AddWithValue("Description", abnormalReason.Description);

                                cmd.ExecuteNonQuery();
                            }

                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO AbnormalReasonHandlingMethod (AbnormalReasonUniqueID, HandlingMethodUniqueID) ValueS (@AbnormalReasonUniqueID, @HandlingMethodUniqueID)";

                                cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", abnormalReason.UniqueID);
                                cmd.Parameters.AddWithValue("HandlingMethodUniqueID", Define.OTHER);

                                cmd.ExecuteNonQuery();
                            }

                            #region AbnormalReasonHandlingMethod
                            foreach (var handlingMethod in abnormalReason.HandlingMethodList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO AbnormalReasonHandlingMethod (AbnormalReasonUniqueID, HandlingMethodUniqueID) ValueS (@AbnormalReasonUniqueID, @HandlingMethodUniqueID)";

                                    cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", abnormalReason.UniqueID);
                                    cmd.Parameters.AddWithValue("HandlingMethodUniqueID", handlingMethod.UniqueID);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                            #endregion
                        }
                        #endregion

                        #region CheckItemAbnormalReason, CheckItemFeelOption
                        foreach (var checkItem in DataModel.CheckItemList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO CheckItemAbnormalReason (CheckItemUniqueID, AbnormalReasonUniqueID) ValueS (@CheckItemUniqueID, @AbnormalReasonUniqueID)";

                                cmd.Parameters.AddWithValue("CheckItemUniqueID", checkItem.UniqueID);
                                cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", Define.OTHER);

                                cmd.ExecuteNonQuery();
                            }

                            #region CheckItemAbnormalReason
                            foreach (var abnormalReason in checkItem.AbnormalReasonList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO CheckItemAbnormalReason (CheckItemUniqueID, AbnormalReasonUniqueID) ValueS (@CheckItemUniqueID, @AbnormalReasonUniqueID)";

                                    cmd.Parameters.AddWithValue("CheckItemUniqueID", checkItem.UniqueID);
                                    cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", abnormalReason.UniqueID);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                            #endregion

                            #region CheckItemFeelOption
                            foreach (var feelOption in checkItem.FeelOptionList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO CheckItemFeelOption (UniqueID, CheckItemUniqueID, Description, IsAbnormal, Seq) ValueS (@UniqueID, @CheckItemUniqueID, @Description, @IsAbnormal, @Seq)";

                                    cmd.Parameters.AddWithValue("UniqueID", feelOption.UniqueID);
                                    cmd.Parameters.AddWithValue("CheckItemUniqueID", checkItem.UniqueID);
                                    cmd.Parameters.AddWithValue("Description", feelOption.Description);
                                    cmd.Parameters.AddWithValue("IsAbnormal", feelOption.IsAbnormal ? "Y" : "N");
                                    cmd.Parameters.AddWithValue("Seq", feelOption.Seq);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                            #endregion
                        }
                        #endregion

                        #region HandlingMethod
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO HandlingMethod (UniqueID, ID, Description) ValueS (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", Define.OTHER);
                            cmd.Parameters.AddWithValue("ID", Define.OTHER);
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var handlingMethod in DataModel.HandlingMethodList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO HandlingMethod (UniqueID, ID, Description) ValueS (@UniqueID, @ID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", handlingMethod.UniqueID);
                                cmd.Parameters.AddWithValue("ID", handlingMethod.ID);
                                cmd.Parameters.AddWithValue("Description", handlingMethod.Description);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region Job, ControlPoint, Equipment, CheckItem
                        foreach (var job in DataModel.JobList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO Job (UniqueID, Description, Remark, TimeMode, BeginTime, EndTime, IsCheckBySeq, IsShowPrevRecord) ValueS (@UniqueID, @Description, @Remark, @TimeMode, @BeginTime, @EndTime, @IsCheckBySeq, @IsShowPrevRecord)";

                                cmd.Parameters.AddWithValue("UniqueID", job.UniqueID);
                                cmd.Parameters.AddWithValue("Description", job.Description);
                                cmd.Parameters.AddWithValue("Remark", job.Remark);
                                cmd.Parameters.AddWithValue("TimeMode", job.TimeMode);
                                cmd.Parameters.AddWithValue("BeginTime", job.BeginTime);
                                cmd.Parameters.AddWithValue("EndTime", job.EndTime);
                                cmd.Parameters.AddWithValue("IsCheckBySeq", job.IsCheckBySeq ? "Y" : "N");
                                cmd.Parameters.AddWithValue("IsShowPrevRecord", job.IsShowPrevRecord ? "Y" : "N");

                                cmd.ExecuteNonQuery();
                            }

                            foreach (var controlPoint in job.ControlPointList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO ControlPoint (JobUniqueID, ControlPointUniqueID, ID, Description, IsFeelItemDefaultNormal, TagID, MinTimeSpan, Remark, Seq) ValueS (@JobUniqueID, @ControlPointUniqueID, @ID, @Description, @IsFeelItemDefaultNormal, @TagID, @MinTimeSpan, @Remark, @Seq)";

                                    cmd.Parameters.AddWithValue("JobUniqueID", job.UniqueID);
                                    cmd.Parameters.AddWithValue("ControlPointUniqueID", controlPoint.UniqueID);
                                    cmd.Parameters.AddWithValue("ID", controlPoint.ID);
                                    cmd.Parameters.AddWithValue("Description", controlPoint.Description);
                                    cmd.Parameters.AddWithValue("IsFeelItemDefaultNormal", controlPoint.IsFeelItemDefaultNormal ? "Y" : "N");
                                    cmd.Parameters.AddWithValue("TagID", controlPoint.TagID);
                                    cmd.Parameters.AddWithValue("MinTimeSpan", controlPoint.MinTimeSpan);
                                    cmd.Parameters.AddWithValue("Remark", controlPoint.Remark);
                                    cmd.Parameters.AddWithValue("Seq", controlPoint.Seq);

                                    cmd.ExecuteNonQuery();
                                }

                                foreach (var equipment in controlPoint.EquipmentList)
                                {
                                    using (SQLiteCommand cmd = conn.CreateCommand())
                                    {
                                        cmd.CommandText = "INSERT INTO Equipment (JobUniqueID, ControlPointUniqueID, EquipmentUniqueID, PartUniqueID, ID, Name, PartDescription, IsFeelItemDefaultNormal, Seq) ValueS (@JobUniqueID, @ControlPointUniqueID, @EquipmentUniqueID, @PartUniqueID, @ID, @Name, @PartDescription, @IsFeelItemDefaultNormal, @Seq)";

                                        cmd.Parameters.AddWithValue("JobUniqueID", job.UniqueID);
                                        cmd.Parameters.AddWithValue("ControlPointUniqueID", controlPoint.UniqueID);
                                        cmd.Parameters.AddWithValue("EquipmentUniqueID", equipment.UniqueID);
                                        cmd.Parameters.AddWithValue("PartUniqueID", "*");
                                        cmd.Parameters.AddWithValue("ID", equipment.ID);
                                        cmd.Parameters.AddWithValue("Name", equipment.Name);
                                        cmd.Parameters.AddWithValue("PartDescription", "");
                                        cmd.Parameters.AddWithValue("IsFeelItemDefaultNormal", equipment.IsFeelItemDefaultNormal ? "Y" : "N");
                                        cmd.Parameters.AddWithValue("Seq", equipment.Seq);

                                        cmd.ExecuteNonQuery();
                                    }

                                    foreach (var checkItem in equipment.CheckItemList)
                                    {
                                        using (SQLiteCommand cmd = conn.CreateCommand())
                                        {
                                            cmd.CommandText = "INSERT INTO CheckItem (JobUniqueID, ControlPointUniqueID, EquipmentUniqueID, PartUniqueID, CheckItemUniqueID, ID, Description, IsFeelItem, TextValueType, LowerLimit, LowerAlertLimit, UpperAlertLimit, UpperLimit, Unit, Remark, Seq) ValueS (@JobUniqueID, @ControlPointUniqueID, @EquipmentUniqueID, @PartUniqueID, @CheckItemUniqueID, @ID, @Description, @IsFeelItem, @TextValueType, @LowerLimit, @LowerAlertLimit, @UpperAlertLimit, @UpperLimit, @Unit, @Remark, @Seq)";

                                            cmd.Parameters.AddWithValue("JobUniqueID", job.UniqueID);
                                            cmd.Parameters.AddWithValue("ControlPointUniqueID", controlPoint.UniqueID);
                                            cmd.Parameters.AddWithValue("EquipmentUniqueID", equipment.UniqueID);
                                            cmd.Parameters.AddWithValue("PartUniqueID", "*");
                                            cmd.Parameters.AddWithValue("CheckItemUniqueID", checkItem.UniqueID);
                                            cmd.Parameters.AddWithValue("ID", checkItem.ID);
                                            cmd.Parameters.AddWithValue("Description", checkItem.Description);
                                            cmd.Parameters.AddWithValue("IsFeelItem", checkItem.IsFeelItem ? "Y" : "N");
                                            cmd.Parameters.AddWithValue("TextValueType", checkItem.TextValueType);
                                            cmd.Parameters.AddWithValue("LowerLimit", checkItem.LowerLimit);
                                            cmd.Parameters.AddWithValue("LowerAlertLimit", checkItem.LowerAlertLimit);
                                            cmd.Parameters.AddWithValue("UpperAlertLimit", checkItem.UpperAlertLimit);
                                            cmd.Parameters.AddWithValue("UpperLimit", checkItem.UpperLimit);
                                            cmd.Parameters.AddWithValue("Unit", checkItem.Unit);
                                            cmd.Parameters.AddWithValue("Remark", checkItem.Remark);
                                            cmd.Parameters.AddWithValue("Seq", checkItem.Seq);

                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        #region LastModifyTime
                        foreach (var lasyModifyTime in DataModel.LastModifyTimeList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO LastModifyTime (JobUniqueID, VersionTime) ValueS (@JobUniqueID, @VersionTime)";

                                cmd.Parameters.AddWithValue("JobUniqueID", lasyModifyTime.Key);
                                cmd.Parameters.AddWithValue("VersionTime", lasyModifyTime.Value);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region OverTimeReason
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO OverTimeReason (UniqueID, ID, Description) ValueS (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", Define.OTHER);
                            cmd.Parameters.AddWithValue("ID", Define.OTHER);
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var overTimeReason in DataModel.OverTimeReasonList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO OverTimeReason (UniqueID, ID, Description) ValueS (@UniqueID, @ID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", overTimeReason.UniqueID);
                                cmd.Parameters.AddWithValue("ID", overTimeReason.ID);
                                cmd.Parameters.AddWithValue("Description", overTimeReason.Description);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region TimeSpanAbnormalReason
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO TimeSpanAbnormalReason (UniqueID, ID, Description) ValueS (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", Define.OTHER);
                            cmd.Parameters.AddWithValue("ID", Define.OTHER);
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var timeSpanAbnormalReason in DataModel.TimeSpanAbnormalReasonList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO TimeSpanAbnormalReason (UniqueID, ID, Description) ValueS (@UniqueID, @ID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", timeSpanAbnormalReason.UniqueID);
                                cmd.Parameters.AddWithValue("ID", timeSpanAbnormalReason.ID);
                                cmd.Parameters.AddWithValue("Description", timeSpanAbnormalReason.Description);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region UnPatrolReason
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO UnPatrolReason (UniqueID, ID, Description) ValueS (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", Define.OTHER);
                            cmd.Parameters.AddWithValue("ID", Define.OTHER);
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var unPatrolReason in DataModel.UnPatrolReasonList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO UnPatrolReason (UniqueID, ID, Description) ValueS (@UniqueID, @ID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", unPatrolReason.UniqueID);
                                cmd.Parameters.AddWithValue("ID", unPatrolReason.ID);
                                cmd.Parameters.AddWithValue("Description", unPatrolReason.Description);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region UnRFIDReason
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO UnRFIDReason (UniqueID, ID, Description) ValueS (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", Define.OTHER);
                            cmd.Parameters.AddWithValue("ID", Define.OTHER);
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var unRFIDReason in DataModel.UnRFIDReasonList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO UnRFIDReason (UniqueID, ID, Description) ValueS (@UniqueID, @ID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", unRFIDReason.UniqueID);
                                cmd.Parameters.AddWithValue("ID", unRFIDReason.ID);
                                cmd.Parameters.AddWithValue("Description", unRFIDReason.Description);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region User
                        foreach (var user in DataModel.UserList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO [User] (ID, Title, Name, Password, UID) ValueS (@ID, @Title, @Name, @Password, @UID)";

                                cmd.Parameters.AddWithValue("ID", user.ID);
                                cmd.Parameters.AddWithValue("Title", user.Title);
                                cmd.Parameters.AddWithValue("Name", user.Name);
                                cmd.Parameters.AddWithValue("Password", user.Password);
                                cmd.Parameters.AddWithValue("UID", user.UID);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        trans.Commit();
                    }

                    conn.Close();
                }

                result.Success();
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        private RequestResult GenerateZip()
        {
            RequestResult result = new RequestResult();

            try
            {
                using (FileStream fs = System.IO.File.Create(GeneratedZipFilePath))
                {
                    using (ZipOutputStream zipStream = new ZipOutputStream(fs))
                    {
                        zipStream.SetLevel(9); //0-9, 9 being the highest level of compression

                        ZipHelper.CompressFolder(GeneratedDbFilePath, zipStream);

                        zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream

                        zipStream.Close();
                    }
                }

                result.ReturnData(GeneratedZipFilePath);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        private string TemplateDbFilePath
        {
            get
            {
                return Path.Combine(Config.EquipmentMaintenanceSQLiteTemplateFolderPath, Define.SQLite_EquipmentMaintenance);
            }
        }

        private string GeneratedFolderPath
        {
            get
            {
                return Path.Combine(Config.EquipmentMaintenanceSQLiteGeneratedFolderPath, Guid);
            }
        }

        private string GeneratedDbFilePath
        {
            get
            {
                return Path.Combine(GeneratedFolderPath, Define.SQLite_EquipmentMaintenance);
            }
        }

        private string GeneratedZipFilePath
        {
            get
            {
                return Path.Combine(GeneratedFolderPath, Define.SQLiteZip_EquipmentMaintenance);
            }
        }

        private string SQLiteConnString
        {
            get
            {
                return string.Format("Data Source={0};Version=3;", GeneratedDbFilePath);
            }
        }

        private string SQLQuery = 
            "SELECT" +
            "	j.UniqueID AS JobUniqueID, " +
            "	j.Description AS JobDescription, " +
            "	r.ID AS RouteID, " +
            "	r.Name AS RouteName, " +
            "   CAST(j.TimeMode AS varchar(32)) AS TimeMode, " +
            "	j.BeginTime, " +
            "	j.EndTime, " +
            "	j.Remark AS JobRemark, " +
            "	p.UniqueID AS ControlPointUniqueID, " +
            "	p.ID AS ControlPointID, " +
            "	p.Description AS ControlPointDescription, " +
            "	p.TagID, " +
            "	p.Remark AS ControlPointRemark, " +
            "   CAST(rp.Seq AS varchar(32)) AS ControlPointSeq, " + 
            "	e.UniqueID AS EquipmentUniqueID, " +
            "	e.ID AS EquipmentID, " +
            "	e.Name AS EquipmentName, " +
            "   CAST(e.IsFeelItemDefaultNormal AS varchar(32)) AS IsFeelItemDefaultNormal, " +
            "   CAST(re.Seq AS varchar(32)) AS EquipmentSeq, " +
            "	c.UniqueID AS CheckItemUniqueID, " +
            "	c.ID AS CheckItemID, " +
            "	c.Description AS CheckItemDescription, " +
            "   CAST(c.IsFeelItem AS varchar(32)) AS IsFeelItem, " +
            "   CASE ec.IsInherit WHEN 1 THEN CAST(c.LowerLimit AS varchar(32)) ELSE CAST(ec.LowerLimit AS varchar(32)) END AS LowerLimit, " +
            "   CASE ec.IsInherit WHEN 1 THEN CAST(c.LowerAlertLimit AS varchar(32)) ELSE CAST(ec.LowerAlertLimit AS varchar(32)) END AS LowerAlertLimit, " +
            "   CASE ec.IsInherit WHEN 1 THEN CAST(c.UpperAlertLimit AS varchar(32)) ELSE CAST(ec.UpperAlertLimit AS varchar(32)) END AS UpperAlertLimit, " +
            "   CASE ec.IsInherit WHEN 1 THEN CAST(c.UpperLimit AS varchar(32)) ELSE CAST(ec.UpperLimit AS varchar(32)) END AS UpperLimit, " +
            "	CASE ec.IsInherit WHEN 1 THEN c.Remark ELSE ec.Remark END AS CheckItemRemark, " +
            "	CASE ec.IsInherit WHEN 1 THEN c.Unit ELSE ec.Unit END AS CheckItemUnit, " +
            "   CAST(rec.Seq AS varchar(32)) AS CheckItemSeq " +
            "FROM " +
            "	Job AS j " +
            "	INNER JOIN JobControlPoint AS jp ON j.UniqueID = jp.JobUniqueID " +
            "	INNER JOIN JobEquipment AS je ON jp.JobUniqueID = je.JobUniqueID AND jp.ControlPointUniqueID = je.ControlPointUniqueID " +
            "	INNER JOIN JobEquipmentCheckItem AS jec ON je.JobUniqueID = jec.JobUniqueID AND je.ControlPointUniqueID = jec.ControlPointUniqueID AND je.EquipmentUniqueID = jec.EquipmentUniqueID AND je.PartUniqueID = jec.PartUniqueID " +
            "	INNER JOIN Route AS r ON j.RouteUniqueID = r.UniqueID " +
            "	INNER JOIN RouteControlPoint AS rp ON r.UniqueID = rp.RouteUniqueID " +
            "	INNER JOIN RouteEquipment AS re ON rp.RouteUniqueID = re.RouteUniqueID AND rp.ControlPointUniqueID = re.ControlPointUniqueID " +
            "	INNER JOIN RouteEquipmentCheckItem AS rec ON re.RouteUniqueID = rec.RouteUniqueID AND re.ControlPointUniqueID = rec.ControlPointUniqueID AND re.EquipmentUniqueID = rec.EquipmentUniqueID AND re.PartUniqueID = rec.PartUniqueID " +
            "	INNER JOIN ControlPoint AS p ON jp.ControlPointUniqueID = p.UniqueID " +
            "	INNER JOIN Equipment AS e ON je.EquipmentUniqueID = e.UniqueID " +
            "	INNER JOIN CheckItem AS c ON jec.CheckItemUniqueID = c.UniqueID " +
            "	INNER JOIN EquipmentCheckItem AS ec ON jec.EquipmentUniqueID = ec.EquipmentUniqueID AND jec.PartUniqueID = ec.PartUniqueID AND jec.CheckItemUniqueID = ec.CheckItemUniqueID " +
            "	LEFT OUTER JOIN CheckResult AS cr ON jec.JobUniqueID = cr.JobUniqueID AND jec.ControlPointUniqueID = cr.ControlPointUniqueID AND jec.EquipmentUniqueID = cr.EquipmentUniqueID AND jec.PartUniqueID = cr.PartUniqueID AND jec.CheckItemUniqueID = cr.CheckItemUniqueID " +
            "WHERE cr.UniqueID IS NULL AND j.UniqueID IN {0}";

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

        ~DownloadHelper()
        {
            Dispose(false);
        }

        #endregion
    }
}