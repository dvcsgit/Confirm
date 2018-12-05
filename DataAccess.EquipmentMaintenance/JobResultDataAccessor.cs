using DbEntity.MSSQL.EquipmentMaintenance;
using System;
using System.Reflection;
using Utility;
using System.Linq;
using Models.EquipmentMaintenance.JobResultManagement;
using Utility.Models;
using Models.Authenticated;
using System.Collections.Generic;
using Models.Home;

namespace DataAccess.EquipmentMaintenance
{
    public class JobResultDataAccessor
    {
        public static RequestResult Query(Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new SummaryViewModel();

                var personalJobResultList = new List<JobResult>();

                var queryableOrganizationJobResultList = new List<JobResult>();

                using (EDbEntities db = new EDbEntities())
                {
                    var personalJobList = (from j in db.Job
                                           join r in db.Route
                                           on j.RouteUniqueID equals r.UniqueID
                                           join x in db.JobUser
                                           on j.UniqueID equals x.JobUniqueID
                                           where x.UserID == Account.ID
                                           select j).Distinct().ToList();

                    var queryableOrganizationJobList = (from j in db.Job
                                                        join r in db.Route
                                                        on j.RouteUniqueID equals r.UniqueID
                                                        where Account.QueryableOrganizationUniqueIDList.Contains(r.OrganizationUniqueID)
                                                        select j).ToList();

                    foreach (var job in personalJobList)
                    {
                        if (JobCycleHelper.IsInCycle(DateTime.Today, job.BeginDate, job.EndDate, job.CycleCount, job.CycleMode))
                        {
                            DateTime beginDate, endDate;

                            JobCycleHelper.GetDateSpan(DateTime.Today, job.BeginDate, job.EndDate, job.CycleCount, job.CycleMode, out beginDate, out endDate);

                            var beginDateString = DateTimeHelper.DateTime2DateString(beginDate);
                            var endDateString = DateTimeHelper.DateTime2DateString(endDate);

                            if (!string.IsNullOrEmpty(job.BeginTime) && !string.IsNullOrEmpty(job.EndTime) && string.Compare(job.BeginTime, job.EndTime) > 0)
                            {
                                endDateString = DateTimeHelper.DateTime2DateString(endDate.AddDays(1));
                            }

                            var jobResult = db.JobResult.FirstOrDefault(x => x.JobUniqueID == job.UniqueID && x.BeginDate == beginDateString && x.EndDate == endDateString);

                            if (jobResult == null)
                            {
                                JobResultDataAccessor.Refresh(Guid.NewGuid().ToString(), job.UniqueID, beginDateString, endDateString);

                                jobResult = db.JobResult.FirstOrDefault(x => x.JobUniqueID == job.UniqueID && x.BeginDate == beginDateString && x.EndDate == endDateString);
                            }

                            if (jobResult != null && !personalJobResultList.Any(x => x.UniqueID == jobResult.UniqueID))
                            {
                                personalJobResultList.Add(jobResult);
                            }
                        }
                    }

                    foreach (var job in queryableOrganizationJobList)
                    {
                        if (JobCycleHelper.IsInCycle(DateTime.Today, job.BeginDate, job.EndDate, job.CycleCount, job.CycleMode))
                        {
                            DateTime beginDate, endDate;

                            JobCycleHelper.GetDateSpan(DateTime.Today, job.BeginDate, job.EndDate, job.CycleCount, job.CycleMode, out beginDate, out endDate);

                            var beginDateString = DateTimeHelper.DateTime2DateString(beginDate);
                            var endDateString = DateTimeHelper.DateTime2DateString(endDate);

                            if (!string.IsNullOrEmpty(job.BeginTime) && !string.IsNullOrEmpty(job.EndTime) && string.Compare(job.BeginTime, job.EndTime) > 0)
                            {
                                endDateString = DateTimeHelper.DateTime2DateString(endDate.AddDays(1));
                            }

                            var jobResult = db.JobResult.FirstOrDefault(x => x.JobUniqueID == job.UniqueID && x.BeginDate == beginDateString && x.EndDate == endDateString);

                            if (jobResult == null)
                            {
                                JobResultDataAccessor.Refresh(Guid.NewGuid().ToString(), job.UniqueID, beginDateString, endDateString);

                                jobResult = db.JobResult.FirstOrDefault(x => x.JobUniqueID == job.UniqueID && x.BeginDate == beginDateString && x.EndDate == endDateString);
                            }

                            if (jobResult != null && !queryableOrganizationJobResultList.Any(x => x.UniqueID == jobResult.UniqueID))
                            {
                                queryableOrganizationJobResultList.Add(jobResult);
                            }
                        }
                    }
                }

                model.PersonalItemList.Add(new SummaryItem()
                {
                    BoxColor = "infobox-red",
                    Icon = "fa-exclamation-circle",
                    Count = personalJobResultList.Count(x => x.CompleteRate == string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.UnPatrol)),
                    Text = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.UnPatrol)
                });

                model.PersonalItemList.Add(new SummaryItem()
                {
                    BoxColor = "infobox-red",
                    Icon = "fa-check",
                    Count = personalJobResultList.Count(x => x.CompleteRate.StartsWith(Resources.Resource.Incomplete)),
                    Text = Resources.Resource.Incomplete
                });

                model.PersonalItemList.Add(new SummaryItem()
                {
                    BoxColor = "infobox-blue",
                    Icon = "fa-check",
                    Count = personalJobResultList.Count(x => x.CompleteRate.StartsWith(Resources.Resource.Processing)),
                    Text = Resources.Resource.Processing
                });

                model.PersonalItemList.Add(new SummaryItem()
                {
                    BoxColor = "infobox-green",
                    Icon = "fa-check",
                    Count = personalJobResultList.Count(x => x.CompleteRate == Resources.Resource.Completed),
                    Text = Resources.Resource.Completed
                });

                model.QueryableOrganizationItemList.Add(new SummaryItem()
                {
                    BoxColor = "infobox-red",
                    Icon = "fa-exclamation-circle",
                    Count = queryableOrganizationJobResultList.Count(x => x.CompleteRate == string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.UnPatrol)),
                    Text = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.UnPatrol)
                });

                model.QueryableOrganizationItemList.Add(new SummaryItem()
                {
                    BoxColor = "infobox-red",
                    Icon = "fa-check",
                    Count = queryableOrganizationJobResultList.Count(x => x.CompleteRate.StartsWith(Resources.Resource.Incomplete)),
                    Text = Resources.Resource.Incomplete
                });

                model.QueryableOrganizationItemList.Add(new SummaryItem()
                {
                    BoxColor = "infobox-blue",
                    Icon = "fa-check",
                    Count = queryableOrganizationJobResultList.Count(x => x.CompleteRate.StartsWith(Resources.Resource.Processing)),
                    Text = Resources.Resource.Processing
                });

                model.QueryableOrganizationItemList.Add(new SummaryItem()
                {
                    BoxColor = "infobox-green",
                    Icon = "fa-check",
                    Count = queryableOrganizationJobResultList.Count(x => x.CompleteRate == Resources.Resource.Completed),
                    Text = Resources.Resource.Completed
                });

                result.ReturnData(model);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static void Refresh(string UniqueID, string JobUniqueID, string BeginDateString, string EndDateString)
        {
            Refresh(UniqueID, JobUniqueID, BeginDateString, EndDateString, string.Empty, string.Empty, string.Empty, string.Empty);
        }

        public static void Refresh(string UniqueID, string JobUniqueID, string BeginDateString, string EndDateString, string OverTimeReason, string OverTimeUser, string UnPatrolReason, string UnPatrolUser)
        {
            try
            {
                var model = Query(UniqueID, JobUniqueID, BeginDateString, EndDateString);

                if (model != null)
                {
                    using (EDbEntities db = new EDbEntities())
                    {
                        var jobResult = db.JobResult.FirstOrDefault(x => x.UniqueID == UniqueID);

                        if (jobResult != null)
                        {
                            jobResult.IsCompleted = model.IsCompleted;
                            jobResult.IsNeedVerify = model.IsNeedVerify;
                            jobResult.Description = model.Description;
                            jobResult.ArriveStatus = model.ArriveStatus;
                            jobResult.ArriveStatusLabelClass = model.ArriveStatusLabelClass;
                            jobResult.BeginTime = model.BeginTime;
                            jobResult.CompleteRate = model.CompleteRate;
                            jobResult.CompleteRateLabelClass = model.CompleteRateLabelClass;
                            jobResult.EndTime = model.EndTime;
                            jobResult.HaveAbnormal = model.HaveAbnormal;
                            jobResult.HaveAlert = model.HaveAlert;
                            jobResult.TimeSpan = model.TimeSpan;
                            jobResult.OverTimeReason = OverTimeReason;
                            jobResult.OverTimeUser = OverTimeUser;
                            jobResult.UnPatrolReason = UnPatrolReason;
                            jobResult.UnPatrolUser = UnPatrolUser;
                            jobResult.JobUsers = model.JobUsers;
                            jobResult.CheckUsers = model.CheckUsers;
                            jobResult.JobEndTime = model.JobEndTime;
                        }
                        else
                        {
                            db.JobResult.Add(new JobResult()
                            {
                                UniqueID = UniqueID,
                                OrganizationDescription = model.OrganizationDescription,
                                JobUniqueID = model.JobUniqueID,
                                BeginDate = model.BeginDate,
                                EndDate = model.EndDate,
                                IsCompleted = model.IsCompleted,
                                IsNeedVerify = model.IsNeedVerify,
                                Description = model.Description,
                                ArriveStatus = model.ArriveStatus,
                                ArriveStatusLabelClass = model.ArriveStatusLabelClass,
                                BeginTime = model.BeginTime,
                                CompleteRate = model.CompleteRate,
                                CompleteRateLabelClass = model.CompleteRateLabelClass,
                                EndTime = model.EndTime,
                                HaveAbnormal = model.HaveAbnormal,
                                HaveAlert = model.HaveAlert,
                                TimeSpan = model.TimeSpan,
                                OverTimeReason = OverTimeReason,
                                OverTimeUser = OverTimeUser,
                                UnPatrolReason = UnPatrolReason,
                                UnPatrolUser = UnPatrolUser,
                                JobUsers = model.JobUsers,
                                CheckUsers = model.CheckUsers,
                                JobEndTime = model.JobEndTime
                            });
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

        private static JobResultModel Query(string JobResultUniqueID, string JobUniqueID, string JobBeginDate, string JobEndDate)
        {
            var model = new JobResultModel();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var job = (from j in db.Job
                               join r in db.Route
                               on j.RouteUniqueID equals r.UniqueID
                               where j.UniqueID == JobUniqueID
                               select new
                               {
                                   Job = j,
                                   Route = r
                               }).First();

                    var organization = OrganizationDataAccessor.GetOrganization(job.Route.OrganizationUniqueID);

                    model = new JobResultModel()
                    {
                        UniqueID = JobResultUniqueID,
                        OrganizationDescription = organization != null ? organization.Description : string.Empty,
                        JobUniqueID = JobUniqueID,
                        BeginDate = JobBeginDate,
                        EndDate = JobEndDate,
                        IsNeedVerify = job.Job.IsNeedVerify,
                        RouteID = job.Route.ID,
                        RouteName = job.Route.Name,
                        JobDescription = job.Job.Description,
                        TimeMode = job.Job.TimeMode,
                        BeginTime = job.Job.BeginTime,
                        EndTime = job.Job.EndTime
                    };

                    var jobUserList = db.JobUser.Where(x => x.JobUniqueID == job.Job.UniqueID).Select(x => x.UserID).OrderBy(x => x).ToList();

                    foreach (var jobUser in jobUserList)
                    {
                        var user = UserDataAccessor.GetUser(jobUser);

                        if (user != null)
                        {
                            model.JobUserList.Add(user);
                        }
                    }

                    var allArriveRecordList = db.ArriveRecord.Where(x => x.JobResultUniqueID == JobResultUniqueID).ToList();

                    var checkUserList = allArriveRecordList.Select(x => x.UserID).Distinct().OrderBy(x => x).ToList();

                    foreach (var checkUser in checkUserList)
                    {
                        var user = UserDataAccessor.GetUser(checkUser);

                        if (user != null)
                        {
                            model.CheckUserList.Add(user);
                        }
                    }

                    var allCheckResultList = (from c in db.CheckResult
                                              join a in db.ArriveRecord
                                              on c.ArriveRecordUniqueID equals a.UniqueID
                                              where a.JobResultUniqueID == JobResultUniqueID
                                              select c).ToList();

                    var controlPointList = (from x in db.JobControlPoint
                                            join j in db.Job
                                            on x.JobUniqueID equals j.UniqueID
                                            join y in db.RouteControlPoint
                                            on new { j.RouteUniqueID, x.ControlPointUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID}
                                            join c in db.ControlPoint
                                            on x.ControlPointUniqueID equals c.UniqueID
                                            where x.JobUniqueID == job.Job.UniqueID
                                            select new
                                            {
                                                UniqueID = c.UniqueID,
                                                ID = c.ID,
                                                Description = c.Description,
                                                y.Seq
                                            }).OrderBy(x => x.Seq).ToList();

                    foreach (var controlPoint in controlPointList)
                    {
                        var controlPointModel = new ControlPointModel()
                        {
                            UniqueID = controlPoint.UniqueID,
                            ArriveRecordList = allArriveRecordList.Where(x => x.ControlPointUniqueID == controlPoint.UniqueID).OrderBy(x => x.ArriveDate).ThenBy(x => x.ArriveTime).Select(x => new ArriveRecordModel
                            {
                                UniqueID = x.UniqueID,
                                ArriveDate = x.ArriveDate,
                                ArriveTime = x.ArriveTime
                            }).ToList()
                        };

                        var controlPointCheckItemList = (from x in db.JobControlPointCheckItem
                                                         join j in db.Job
                                                         on x.JobUniqueID equals j.UniqueID
                                                         join y in db.RouteControlPointCheckItem
                                                         on new { j.RouteUniqueID, x.ControlPointUniqueID, x.CheckItemUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.CheckItemUniqueID}
                                                         join c in db.View_ControlPointCheckItem
                                                         on new { x.ControlPointUniqueID, x.CheckItemUniqueID } equals new { c.ControlPointUniqueID, c.CheckItemUniqueID }
                                                         where x.JobUniqueID == job.Job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID
                                                         select new
                                                         {
                                                             UniqueID = c.CheckItemUniqueID,
                                                             c.ID,
                                                             c.Description,
                                                             y.Seq
                                                         }).OrderBy(x => x.Seq).ToList();

                        foreach (var checkItem in controlPointCheckItemList)
                        {
                            var checkItemModel = new CheckItemModel()
                            {
                                EquipmentUniqueID = "",
                                PartUniqueID = "",
                                CheckItemUniqueID = checkItem.UniqueID
                            };

                            var checkResultList = allCheckResultList.Where(x => x.ControlPointUniqueID == controlPoint.UniqueID && string.IsNullOrEmpty(x.EquipmentUniqueID) && string.IsNullOrEmpty(x.PartUniqueID) && x.CheckItemUniqueID == checkItem.UniqueID).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

                            foreach (var checkResult in checkResultList)
                            {
                                checkItemModel.CheckResultList.Add(new CheckResultModel()
                                {
                                    ArriveRecordUniqueID = checkResult.ArriveRecordUniqueID,
                                    UniqueID = checkResult.UniqueID,
                                    CheckDate = checkResult.CheckDate,
                                    CheckTime = checkResult.CheckTime,
                                    IsAbnormal = checkResult.IsAbnormal,
                                    IsAlert = checkResult.IsAlert
                                });
                            }

                            controlPointModel.CheckItemList.Add(checkItemModel);
                        }

                        var equipmentList = (from x in db.JobEquipment
                                             join j in db.Job
                                             on x.JobUniqueID equals j.UniqueID
                                             join y in db.RouteEquipment
                                             on new { j.RouteUniqueID, x.ControlPointUniqueID, x.EquipmentUniqueID, x.PartUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.EquipmentUniqueID, y.PartUniqueID}
                                             where x.JobUniqueID == job.Job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID
                                             select new { x.EquipmentUniqueID, x.PartUniqueID, y.Seq }).OrderBy(x => x.Seq).ToList();

                        foreach (var equipment in equipmentList)
                        {
                            var equipmentCheckItemList = (from x in db.JobEquipmentCheckItem
                                                          join j in db.Job
                                                          on x.JobUniqueID equals j.UniqueID
                                                          join y in db.RouteEquipmentCheckItem
                                                          on new { j.RouteUniqueID, x.ControlPointUniqueID, x.EquipmentUniqueID, x.PartUniqueID, x.CheckItemUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.EquipmentUniqueID, y.PartUniqueID, y.CheckItemUniqueID}
                                                          join e in db.Equipment
                                                          on x.EquipmentUniqueID equals e.UniqueID
                                                          join p in db.EquipmentPart
                                                          on new { x.EquipmentUniqueID, x.PartUniqueID } equals new { p.EquipmentUniqueID, PartUniqueID = p.UniqueID } into tmpPart
                                                          from p in tmpPart.DefaultIfEmpty()
                                                          join c in db.View_EquipmentCheckItem
                                                          on new { x.EquipmentUniqueID, x.PartUniqueID, x.CheckItemUniqueID } equals new { c.EquipmentUniqueID, c.PartUniqueID, c.CheckItemUniqueID }
                                                          where x.JobUniqueID == job.Job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == equipment.EquipmentUniqueID && x.PartUniqueID == equipment.PartUniqueID
                                                          select new
                                                          {
                                                              x.EquipmentUniqueID,
                                                              x.PartUniqueID,
                                                              x.CheckItemUniqueID,
                                                              y.Seq
                                                          }).OrderBy(x => x.Seq).ToList();

                            foreach (var checkItem in equipmentCheckItemList)
                            {
                                var checkItemModel = new CheckItemModel()
                                {
                                    EquipmentUniqueID = checkItem.EquipmentUniqueID,
                                    PartUniqueID = checkItem.PartUniqueID,
                                    CheckItemUniqueID = checkItem.CheckItemUniqueID
                                };

                                var checkResultList = allCheckResultList.Where(x => x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == checkItem.EquipmentUniqueID && x.PartUniqueID == checkItem.PartUniqueID && x.CheckItemUniqueID == checkItem.CheckItemUniqueID).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

                                foreach (var checkResult in checkResultList)
                                {
                                    checkItemModel.CheckResultList.Add(new CheckResultModel()
                                    {
                                        ArriveRecordUniqueID = checkResult.ArriveRecordUniqueID,
                                        UniqueID = checkResult.UniqueID,
                                        CheckDate = checkResult.CheckDate,
                                        CheckTime = checkResult.CheckTime,
                                        IsAbnormal = checkResult.IsAbnormal,
                                        IsAlert = checkResult.IsAlert
                                    });
                                }

                                controlPointModel.CheckItemList.Add(checkItemModel);
                            }
                        }

                        model.ControlPointList.Add(controlPointModel);
                    }
                }
            }
            catch (Exception ex)
            {
                model = null;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return model;
        }
    }
}
