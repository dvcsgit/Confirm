using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Data.SQLite;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using Utility;
using Utility.Models;
using DbEntity.MSSQL;
using DbEntity.MSSQL.GuardPatrol;
using Models.GuardPatrol.DataSync;
using DataAccess.GuardPatrol;
using DataAccess;

namespace DataSync.GuardPatrol
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
                var checkDate = DateTimeHelper.DateString2DateTime(Model.CheckDate).Value;

                using (GDbEntities db = new GDbEntities())
                {
                    #region UnPatrolReason
                    DataModel.UnPatrolReasonList = db.UnPatrolReason.ToList();
                    #endregion

                    #region UnRFIDReason
                    DataModel.UnRFIDReasonList = db.UnRFIDReason.ToList();
                    #endregion

                    #region OverTimeReason
                    DataModel.OverTimeReasonList = db.OverTimeReason.ToList();
                    #endregion

                    #region TimeSpanAbnormalReason
                    DataModel.TimeSpanAbnormalReasonList = db.TimeSpanAbnormalReason.ToList();
                    #endregion

                    foreach (var parameter in Model.Parameters)
                    {
                        #region Job
                        var job = db.Job.First(x => x.UniqueID == parameter.JobUniqueID);

                        var lastModifyTime = LastModifyTimeHelper.Get(job.UniqueID);

                        var jobBeginDateString = string.Empty;
                        var jobEndDateString = string.Empty;

                        if (parameter.IsExceptChecked)
                        {
                            DateTime beginDate, endDate;

                            JobCycleHelper.GetDateSpan(checkDate, job.BeginDate, job.EndDate, job.CycleCount, job.CycleMode, out beginDate, out endDate);

                            jobBeginDateString = DateTimeHelper.DateTime2DateString(beginDate);
                            jobEndDateString = DateTimeHelper.DateTime2DateString(endDate);
                        }

                        var jobRouteList = (from x in db.JobRoute
                                            join j in db.Job
                                            on x.JobUniqueID equals j.UniqueID
                                            join r in db.Route
                                            on x.RouteUniqueID equals r.UniqueID
                                            where x.JobUniqueID == parameter.JobUniqueID
                                            select new
                                            {
                                                x.UniqueID,
                                                r.OrganizationUniqueID,
                                                JobUniqueID = j.UniqueID,
                                                RouteUniqueID = x.RouteUniqueID,
                                                RouteID = r.ID,
                                                RouteName = r.Name,
                                                j.Description,
                                                j.BeginDate,
                                                j.EndDate,
                                                j.TimeMode,
                                                j.BeginTime,
                                                j.EndTime,
                                                j.CycleCount,
                                                j.CycleMode,
                                                j.IsCheckBySeq,
                                                j.IsShowPrevRecord,
                                                j.Remark
                                            }).ToList();

                        foreach (var jobRoute in jobRouteList)
                        {
                            var jobModel = new JobModel()
                            {
                                UniqueID = jobRoute.UniqueID,
                                OrganizationUniqueID = jobRoute.OrganizationUniqueID,
                                JobDescription = jobRoute.Description,
                                RouteID = jobRoute.RouteID,
                                RouteName = jobRoute.RouteName,
                                TimeMode = jobRoute.TimeMode,
                                BeginTime = jobRoute.BeginTime,
                                EndTime = jobRoute.EndTime,
                                IsCheckBySeq = jobRoute.IsCheckBySeq,
                                IsShowPrevRecord = jobRoute.IsShowPrevRecord,
                                Remark = jobRoute.Remark,
                                LastModifyTime = lastModifyTime,
                                UserList = (from x in db.JobUser
                                            where x.JobUniqueID == jobRoute.JobUniqueID
                                            select new UserModel
                                            {
                                                ID = x.UserID
                                            }).ToList()
                            };

                            #region ControlPoint
                            var controlPointList = (from x in db.JobControlPoint
                                                    join y in db.RouteControlPoint
                                                    on new { x.RouteUniqueID, x.ControlPointUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID }
                                                    join c in db.ControlPoint
                                                    on x.ControlPointUniqueID equals c.UniqueID
                                                    where x.JobUniqueID == jobRoute.JobUniqueID && x.RouteUniqueID == jobRoute.RouteUniqueID
                                                    select new
                                                    {
                                                        c.UniqueID,
                                                        c.ID,
                                                        c.Description,
                                                        c.IsFeelItemDefaultNormal,
                                                        c.TagID,
                                                        x.MinTimeSpan,
                                                        c.Remark,
                                                        y.Seq
                                                    }).ToList();

                            foreach (var controlPoint in controlPointList)
                            {
                                var controlPointModel = new ControlPointModel()
                                {
                                    UniqueID = controlPoint.UniqueID,
                                    ID = controlPoint.ID,
                                    Description = controlPoint.Description,
                                    IsFeelItemDefaultNormal = controlPoint.IsFeelItemDefaultNormal,
                                    TagID = controlPoint.TagID,
                                    MinTimeSpan = controlPoint.MinTimeSpan,
                                    Remark = controlPoint.Remark,
                                    Seq = controlPoint.Seq
                                };

                                #region ControlPointCheckItem
                                var controlPointCheckItemList = (from x in db.JobControlPointCheckItem
                                                                 join y in db.RouteControlPointCheckItem
                                                                 on new { x.RouteUniqueID, x.ControlPointUniqueID, x.CheckItemUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.CheckItemUniqueID }
                                                                 join c in db.View_ControlPointCheckItem
                                                                 on new { x.ControlPointUniqueID, x.CheckItemUniqueID } equals new { c.ControlPointUniqueID, c.CheckItemUniqueID }
                                                                 where x.JobUniqueID == jobRoute.JobUniqueID && x.RouteUniqueID == jobRoute.RouteUniqueID && x.ControlPointUniqueID == controlPoint.UniqueID
                                                                 select new
                                                                 {
                                                                     UniqueID = c.CheckItemUniqueID,
                                                                     c.ID,
                                                                     c.Description,
                                                                     c.IsFeelItem,
                                                                     c.LowerLimit,
                                                                     c.LowerAlertLimit,
                                                                     c.UpperAlertLimit,
                                                                     c.UpperLimit,
                                                                     c.Remark,
                                                                     c.Unit,
                                                                     y.Seq
                                                                 }).ToList();

                                foreach (var checkItem in controlPointCheckItemList)
                                {
                                    bool except = false;

                                    if (parameter.IsExceptChecked)
                                    {
                                        var prevCheckResult = db.CheckResult.Where(x => x.JobUniqueID == jobRoute.JobUniqueID && x.RouteUniqueID == jobRoute.RouteUniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID && string.Compare(x.CheckDate, jobBeginDateString) >= 0 && string.Compare(x.CheckDate, jobEndDateString) <= 0).FirstOrDefault();

                                        except = prevCheckResult != null;
                                    }

                                    if (!except)
                                    {
                                        var checkItemModel = new CheckItemModel()
                                        {
                                            UniqueID = checkItem.UniqueID,
                                            ID = checkItem.ID,
                                            Description = checkItem.Description,
                                            IsFeelItem = checkItem.IsFeelItem,
                                            LowerLimit = checkItem.LowerLimit,
                                            LowerAlertLimit = checkItem.LowerAlertLimit,
                                            UpperAlertLimit = checkItem.UpperAlertLimit,
                                            UpperLimit = checkItem.UpperLimit,
                                            Remark = checkItem.Remark,
                                            Unit = checkItem.Unit,
                                            Seq = checkItem.Seq,
                                            FeelOptionList = db.CheckItemFeelOption.Where(x => x.CheckItemUniqueID == checkItem.UniqueID).Select(x => new FeelOptionModel
                                            {
                                                UniqueID = x.UniqueID,
                                                Description = x.Description,
                                                IsAbnormal = x.IsAbnormal,
                                                Seq = x.Seq
                                            }).ToList(),
                                            AbnormalReasonList = (from ca in db.CheckItemAbnormalReason
                                                                  join a in db.AbnormalReason
                                                                  on ca.AbnormalReasonUniqueID equals a.UniqueID
                                                                  where ca.CheckItemUniqueID == checkItem.UniqueID
                                                                  select new AbnormalReasonModel
                                                                  {
                                                                      UniqueID = a.UniqueID,
                                                                      ID = a.ID,
                                                                      Description = a.Description,
                                                                      HandlingMethodList = (from ah in db.AbnormalReasonHandlingMethod
                                                                                            join h in db.HandlingMethod
                                                                                                on ah.HandlingMethodUniqueID equals h.UniqueID
                                                                                            where ah.AbnormalReasonUniqueID == a.UniqueID
                                                                                            select new HandlingMethodModel
                                                                                            {
                                                                                                UniqueID = h.UniqueID,
                                                                                                ID = h.ID,
                                                                                                Description = h.Description
                                                                                            }).ToList()
                                                                  }).ToList()
                                        };

                                        if (jobRoute.IsShowPrevRecord)
                                        {
                                            var prevCheckResult = db.CheckResult.Where(x => x.ControlPointUniqueID == controlPoint.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID).OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();

                                            if (prevCheckResult != null && !DataModel.PrevCheckResultList.Any(x => x.ControlPointUniqueID == controlPoint.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID))
                                            {
                                                DataModel.PrevCheckResultList.Add(new PrevCheckResultModel()
                                                {
                                                    ControlPointUniqueID = controlPoint.UniqueID,
                                                    CheckItemUniqueID = checkItem.UniqueID,
                                                    CheckDate = prevCheckResult.CheckDate,
                                                    CheckTime = prevCheckResult.CheckTime,
                                                    Result = prevCheckResult.Result,
                                                    IsAbnormal = prevCheckResult.IsAbnormal,
                                                    LowerLimit = prevCheckResult.LowerLimit,
                                                    LowerAlertLimit = prevCheckResult.LowerAlertLimit,
                                                    UpperAlertLimit = prevCheckResult.UpperAlertLimit,
                                                    UpperLimit = prevCheckResult.UpperLimit,
                                                    Unit = prevCheckResult.Unit,
                                                    AbnormalReasonList = db.CheckResultAbnormalReason.Where(a => a.CheckResultUniqueID == prevCheckResult.UniqueID).OrderBy(a => a.AbnormalReasonID).Select(a => new PrevCheckResultAbnormalReasonModel
                                                    {
                                                        Description = a.AbnormalReasonDescription,
                                                        Remark = a.AbnormalReasonRemark,
                                                        HandlingMethodList = db.CheckResultHandlingMethod.Where(h => h.CheckResultUniqueID == prevCheckResult.UniqueID && h.AbnormalReasonUniqueID == a.AbnormalReasonUniqueID).OrderBy(h => h.HandlingMethodID).Select(h => new PrevCheckResultHandlingMethodModel
                                                        {
                                                            Description = h.HandlingMethodDescription,
                                                            Remark = h.HandlingMethodRemark
                                                        }).ToList()
                                                    }).ToList()
                                                });
                                            }
                                        }

                                        controlPointModel.CheckItemList.Add(checkItemModel);
                                    }
                                }
                                #endregion

                                if (controlPointModel.CheckItemList.Count > 0)
                                {
                                    jobModel.ControlPointList.Add(controlPointModel);
                                }
                            }
                            #endregion

                        #endregion

                            if (jobModel.ControlPointList.Count > 0)
                            {
                                DataModel.JobList.Add(jobModel);
                            }
                        }
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
                            cmd.CommandText = "INSERT INTO AbnormalReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", "OTHER");
                            cmd.Parameters.AddWithValue("ID", "OTHER");
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO AbnormalReasonHandlingMethod (AbnormalReasonUniqueID, HandlingMethodUniqueID) VALUES (@AbnormalReasonUniqueID, @HandlingMethodUniqueID)";

                            cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", "OTHER");
                            cmd.Parameters.AddWithValue("HandlingMethodUniqueID", "OTHER");

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var abnormalReason in DataModel.AbnormalReasonList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO AbnormalReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", abnormalReason.UniqueID);
                                cmd.Parameters.AddWithValue("ID", abnormalReason.ID);
                                cmd.Parameters.AddWithValue("Description", abnormalReason.Description);

                                cmd.ExecuteNonQuery();
                            }

                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO AbnormalReasonHandlingMethod (AbnormalReasonUniqueID, HandlingMethodUniqueID) VALUES (@AbnormalReasonUniqueID, @HandlingMethodUniqueID)";

                                cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", abnormalReason.UniqueID);
                                cmd.Parameters.AddWithValue("HandlingMethodUniqueID", "OTHER");

                                cmd.ExecuteNonQuery();
                            }

                            #region AbnormalReasonHandlingMethod
                            foreach (var handlingMethod in abnormalReason.HandlingMethodList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO AbnormalReasonHandlingMethod (AbnormalReasonUniqueID, HandlingMethodUniqueID) VALUES (@AbnormalReasonUniqueID, @HandlingMethodUniqueID)";

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
                                cmd.CommandText = "INSERT INTO CheckItemAbnormalReason (CheckItemUniqueID, AbnormalReasonUniqueID) VALUES (@CheckItemUniqueID, @AbnormalReasonUniqueID)";

                                cmd.Parameters.AddWithValue("CheckItemUniqueID", checkItem.UniqueID);
                                cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", "OTHER");

                                cmd.ExecuteNonQuery();
                            }

                            #region CheckItemAbnormalReason
                            foreach (var abnormalReason in checkItem.AbnormalReasonList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO CheckItemAbnormalReason (CheckItemUniqueID, AbnormalReasonUniqueID) VALUES (@CheckItemUniqueID, @AbnormalReasonUniqueID)";

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
                                    cmd.CommandText = "INSERT INTO CheckItemFeelOption (UniqueID, CheckItemUniqueID, Description, IsAbnormal, Seq) VALUES (@UniqueID, @CheckItemUniqueID, @Description, @IsAbnormal, @Seq)";

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
                            cmd.CommandText = "INSERT INTO HandlingMethod (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", "OTHER");
                            cmd.Parameters.AddWithValue("ID", "OTHER");
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var handlingMethod in DataModel.HandlingMethodList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO HandlingMethod (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", handlingMethod.UniqueID);
                                cmd.Parameters.AddWithValue("ID", handlingMethod.ID);
                                cmd.Parameters.AddWithValue("Description", handlingMethod.Description);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region Job, ControlPoint, CheckItem
                        foreach (var job in DataModel.JobList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO Job (UniqueID, Description, Remark, TimeMode, BeginTime, EndTime, IsCheckBySeq, IsShowPrevRecord) VALUES (@UniqueID, @Description, @Remark, @TimeMode, @BeginTime, @EndTime, @IsCheckBySeq, @IsShowPrevRecord)";

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
                                    cmd.CommandText = "INSERT INTO ControlPoint (JobUniqueID, ControlPointUniqueID, ID, Description, IsFeelItemDefaultNormal, TagID, MinTimeSpan, Remark, Seq) VALUES (@JobUniqueID, @ControlPointUniqueID, @ID, @Description, @IsFeelItemDefaultNormal, @TagID, @MinTimeSpan, @Remark, @Seq)";

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

                                
                                foreach (var checkItem in controlPoint.CheckItemList)
                                {
                                    using (SQLiteCommand cmd = conn.CreateCommand())
                                    {
                                        cmd.CommandText = "INSERT INTO CheckItem (JobUniqueID, ControlPointUniqueID, EquipmentUniqueID, PartUniqueID, CheckItemUniqueID, ID, Description, IsFeelItem, LowerLimit, LowerAlertLimit, UpperAlertLimit, UpperLimit, Unit, Remark, Seq) VALUES (@JobUniqueID, @ControlPointUniqueID, @EquipmentUniqueID, @PartUniqueID, @CheckItemUniqueID, @ID, @Description, @IsFeelItem, @LowerLimit, @LowerAlertLimit, @UpperAlertLimit, @UpperLimit, @Unit, @Remark, @Seq)";

                                        cmd.Parameters.AddWithValue("JobUniqueID", job.UniqueID);
                                        cmd.Parameters.AddWithValue("ControlPointUniqueID", controlPoint.UniqueID);
                                        cmd.Parameters.AddWithValue("EquipmentUniqueID", "");
                                        cmd.Parameters.AddWithValue("PartUniqueID", "");
                                        cmd.Parameters.AddWithValue("CheckItemUniqueID", checkItem.UniqueID);
                                        cmd.Parameters.AddWithValue("ID", checkItem.ID);
                                        cmd.Parameters.AddWithValue("Description", checkItem.Description);
                                        cmd.Parameters.AddWithValue("IsFeelItem", checkItem.IsFeelItem ? "Y" : "N");
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
                        #endregion

                        #region LastModifyTime
                        foreach (var lasyModifyTime in DataModel.LastModifyTimeList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO LastModifyTime (JobUniqueID, VersionTime) VALUES (@JobUniqueID, @VersionTime)";

                                cmd.Parameters.AddWithValue("JobUniqueID", lasyModifyTime.Key);
                                cmd.Parameters.AddWithValue("VersionTime", lasyModifyTime.Value);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region OverTimeReason
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO OverTimeReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", "OTHER");
                            cmd.Parameters.AddWithValue("ID", "OTHER");
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var overTimeReason in DataModel.OverTimeReasonList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO OverTimeReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

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
                            cmd.CommandText = "INSERT INTO TimeSpanAbnormalReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", "OTHER");
                            cmd.Parameters.AddWithValue("ID", "OTHER");
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var timeSpanAbnormalReason in DataModel.TimeSpanAbnormalReasonList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO TimeSpanAbnormalReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

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
                            cmd.CommandText = "INSERT INTO UnPatrolReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", "OTHER");
                            cmd.Parameters.AddWithValue("ID", "OTHER");
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var unPatrolReason in DataModel.UnPatrolReasonList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO UnPatrolReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

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
                            cmd.CommandText = "INSERT INTO UnRFIDReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", "OTHER");
                            cmd.Parameters.AddWithValue("ID", "OTHER");
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var unRFIDReason in DataModel.UnRFIDReasonList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO UnRFIDReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

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
                                cmd.CommandText = "INSERT INTO [User] (ID, Title, Name, Password, UID) VALUES (@ID, @Title, @Name, @Password, @UID)";

                                cmd.Parameters.AddWithValue("ID", user.ID);
                                cmd.Parameters.AddWithValue("Title", user.Title);
                                cmd.Parameters.AddWithValue("Name", user.Name);
                                cmd.Parameters.AddWithValue("Password", user.Password);
                                cmd.Parameters.AddWithValue("UID", user.UID);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region PrevCheckResult
                        foreach (var prevCheckResult in DataModel.PrevCheckResultList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO PrevCheckResult (ControlPointUniqueID, EquipmentUniqueID, PartUniqueID, CheckItemUniqueID, LowerLimit, LowerAlertLimit, UpperAlertLimit, UpperLimit, Unit, CheckDate, CheckTime, Result, IsAbnormal, AbnormalReason) VALUES (@ControlPointUniqueID, @EquipmentUniqueID, @PartUniqueID, @CheckItemUniqueID, @LowerLimit, @LowerAlertLimit, @UpperAlertLimit, @UpperLimit, @Unit, @CheckDate, @CheckTime, @Result, @IsAbnormal, @AbnormalReason)";

                                cmd.Parameters.AddWithValue("ControlPointUniqueID", prevCheckResult.ControlPointUniqueID);
                                cmd.Parameters.AddWithValue("EquipmentUniqueID", "");
                                cmd.Parameters.AddWithValue("PartUniqueID", "");
                                cmd.Parameters.AddWithValue("CheckItemUniqueID", prevCheckResult.CheckItemUniqueID);
                                cmd.Parameters.AddWithValue("LowerLimit", prevCheckResult.LowerLimit);
                                cmd.Parameters.AddWithValue("LowerAlertLimit", prevCheckResult.LowerAlertLimit);
                                cmd.Parameters.AddWithValue("UpperAlertLimit", prevCheckResult.UpperAlertLimit);
                                cmd.Parameters.AddWithValue("UpperLimit", prevCheckResult.UpperLimit);
                                cmd.Parameters.AddWithValue("Unit", prevCheckResult.Unit);
                                cmd.Parameters.AddWithValue("CheckDate", prevCheckResult.CheckDate);
                                cmd.Parameters.AddWithValue("CheckTime", prevCheckResult.CheckTime);
                                cmd.Parameters.AddWithValue("Result", prevCheckResult.Result);
                                cmd.Parameters.AddWithValue("IsAbnormal", prevCheckResult.IsAbnormal);
                                cmd.Parameters.AddWithValue("AbnormalReason", prevCheckResult.AbnormalReasons);

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
                return Path.Combine(Config.GuardPatrolSQLiteTemplateFolderPath, Define.SQLite_GuardPatrol);
            }
        }

        private string GeneratedFolderPath
        {
            get
            {
                return Path.Combine(Config.GuardPatrolSQLiteGeneratedFolderPath, Guid);
            }
        }

        private string GeneratedDbFilePath
        {
            get
            {
                return Path.Combine(GeneratedFolderPath, Define.SQLite_GuardPatrol);
            }
        }

        private string GeneratedZipFilePath
        {
            get
            {
                return Path.Combine(GeneratedFolderPath, Define.SQLiteZip_GuardPatrol);
            }
        }

        private string SQLiteConnString
        {
            get
            {
                return string.Format("Data Source={0};Version=3;", GeneratedDbFilePath);
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

        ~DownloadHelper()
        {
            Dispose(false);
        }

        #endregion
    }
}
