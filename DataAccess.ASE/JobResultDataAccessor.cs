using DbEntity.ASE;
using System;
using System.Reflection;
using Utility;
using System.Linq;
using Models.EquipmentMaintenance.JobResultManagement;
using Utility.Models;
using System.Collections.Generic;
using Models.Authenticated;
using Models.Home;

namespace DataAccess.ASE
{
    public class JobResultDataAccessor
    {
        public static RequestResult Query(Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new SummaryViewModel();

                var personalJobResultList = new List<JOBRESULT>();

                var queryableOrganizationJobResultList = new List<JOBRESULT>();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var personalJobList = (from j in db.JOB
                                           join r in db.ROUTE
                                           on j.ROUTEUNIQUEID equals r.UNIQUEID
                                           join x in db.JOBUSER
                                           on j.UNIQUEID equals x.JOBUNIQUEID
                                           where x.USERID == Account.ID
                                           select j).Distinct().ToList();

                    var queryableOrganizationJobList = (from j in db.JOB
                                                        join r in db.ROUTE
                                                        on j.ROUTEUNIQUEID equals r.UNIQUEID
                                                        where Account.QueryableOrganizationUniqueIDList.Contains(r.ORGANIZATIONUNIQUEID)
                                                        select j).ToList();

                    foreach (var job in personalJobList)
                    {
                        if (JobCycleHelper.IsInCycle(DateTime.Today, job.BEGINDATE.Value, job.ENDDATE, job.CYCLECOUNT.Value, job.CYCLEMODE))
                        {
                            DateTime beginDate, endDate;

                            JobCycleHelper.GetDateSpan(DateTime.Today, job.BEGINDATE.Value, job.ENDDATE, job.CYCLECOUNT.Value, job.CYCLEMODE, out beginDate, out endDate);

                            var beginDateString = DateTimeHelper.DateTime2DateString(beginDate);
                            var endDateString = DateTimeHelper.DateTime2DateString(endDate);

                            var jobResult = db.JOBRESULT.FirstOrDefault(x => x.JOBUNIQUEID == job.UNIQUEID && x.BEGINDATE == beginDateString && x.ENDDATE == endDateString);

                            if (jobResult == null)
                            {
                                JobResultDataAccessor.Refresh(Guid.NewGuid().ToString(), job.UNIQUEID, beginDateString, endDateString);

                                jobResult = db.JOBRESULT.FirstOrDefault(x => x.JOBUNIQUEID == job.UNIQUEID && x.BEGINDATE == beginDateString && x.ENDDATE == endDateString);
                            }

                            if (jobResult != null && !personalJobResultList.Any(x => x.UNIQUEID == jobResult.UNIQUEID))
                            {
                                personalJobResultList.Add(jobResult);
                            }
                        }
                    }

                    foreach (var job in queryableOrganizationJobList)
                    {
                        if (JobCycleHelper.IsInCycle(DateTime.Today, job.BEGINDATE.Value, job.ENDDATE, job.CYCLECOUNT.Value, job.CYCLEMODE))
                        {
                            DateTime beginDate, endDate;

                            JobCycleHelper.GetDateSpan(DateTime.Today, job.BEGINDATE.Value, job.ENDDATE, job.CYCLECOUNT.Value, job.CYCLEMODE, out beginDate, out endDate);

                            var beginDateString = DateTimeHelper.DateTime2DateString(beginDate);
                            var endDateString = DateTimeHelper.DateTime2DateString(endDate);

                            var jobResult = db.JOBRESULT.FirstOrDefault(x => x.JOBUNIQUEID == job.UNIQUEID && x.BEGINDATE == beginDateString && x.ENDDATE == endDateString);

                            if (jobResult == null)
                            {
                                JobResultDataAccessor.Refresh(Guid.NewGuid().ToString(), job.UNIQUEID, beginDateString, endDateString);

                                jobResult = db.JOBRESULT.FirstOrDefault(x => x.JOBUNIQUEID == job.UNIQUEID && x.BEGINDATE == beginDateString && x.ENDDATE == endDateString);
                            }

                            if (jobResult != null && !queryableOrganizationJobResultList.Any(x => x.UNIQUEID == jobResult.UNIQUEID))
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
                    Count = personalJobResultList.Count(x => x.COMPLETERATE == string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.UnPatrol)),
                    Text = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.UnPatrol)
                });

                model.PersonalItemList.Add(new SummaryItem()
                {
                    BoxColor = "infobox-red",
                    Icon = "fa-check",
                    Count = personalJobResultList.Count(x => x.COMPLETERATE.StartsWith(Resources.Resource.Incomplete)),
                    Text = Resources.Resource.Incomplete
                });

                model.PersonalItemList.Add(new SummaryItem()
                {
                    BoxColor = "infobox-blue",
                    Icon = "fa-check",
                    Count = personalJobResultList.Count(x => x.COMPLETERATE.StartsWith(Resources.Resource.Processing)),
                    Text = Resources.Resource.Processing
                });

                model.PersonalItemList.Add(new SummaryItem()
                {
                    BoxColor = "infobox-green",
                    Icon = "fa-check",
                    Count = personalJobResultList.Count(x => x.COMPLETERATE == Resources.Resource.Completed),
                    Text = Resources.Resource.Completed
                });

                model.QueryableOrganizationItemList.Add(new SummaryItem()
                {
                    BoxColor = "infobox-red",
                    Icon = "fa-exclamation-circle",
                    Count = queryableOrganizationJobResultList.Count(x => x.COMPLETERATE == string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.UnPatrol)),
                    Text = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.UnPatrol)
                });

                model.QueryableOrganizationItemList.Add(new SummaryItem()
                {
                    BoxColor = "infobox-red",
                    Icon = "fa-check",
                    Count = queryableOrganizationJobResultList.Count(x => x.COMPLETERATE.StartsWith(Resources.Resource.Incomplete)),
                    Text = Resources.Resource.Incomplete
                });

                model.QueryableOrganizationItemList.Add(new SummaryItem()
                {
                    BoxColor = "infobox-blue",
                    Icon = "fa-check",
                    Count = queryableOrganizationJobResultList.Count(x => x.COMPLETERATE.StartsWith(Resources.Resource.Processing)),
                    Text = Resources.Resource.Processing
                });

                model.QueryableOrganizationItemList.Add(new SummaryItem()
                {
                    BoxColor = "infobox-green",
                    Icon = "fa-check",
                    Count = queryableOrganizationJobResultList.Count(x => x.COMPLETERATE == Resources.Resource.Completed),
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
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var jobResult = db.JOBRESULT.FirstOrDefault(x => x.UNIQUEID == UniqueID);

                        if (jobResult != null)
                        {
                            jobResult.ISCOMPLETED = model.IsCompleted?"Y":"N";
                            jobResult.ISNEEDVERIFY = model.IsNeedVerify ? "Y" : "N";
                            jobResult.DESCRIPTION = model.Description;
                            jobResult.ARRIVESTATUS = model.ArriveStatus;
                            jobResult.ARRIVESTATUSLABELCLASS = model.ArriveStatusLabelClass;
                            jobResult.BEGINTIME = model.BeginTime;
                            jobResult.COMPLETERATE = model.CompleteRate;
                            jobResult.COMPLETERATELABELCLASS = model.CompleteRateLabelClass;
                            jobResult.ENDTIME = model.EndTime;
                            jobResult.HAVEABNORMAL = model.HaveAbnormal ? "Y" : "N";
                            jobResult.HAVEALERT = model.HaveAlert ? "Y" : "N";
                            jobResult.TIMESPAN = model.TimeSpan;
                            jobResult.OVERTIMEREASON = OverTimeReason;
                            jobResult.OVERTIMEUSER = OverTimeUser;
                            jobResult.UNPATROLREASON = UnPatrolReason;
                            jobResult.UNPATROLUSER = UnPatrolUser;
                            jobResult.JOBUSERS = model.JobUsers;
                            jobResult.CHECKUSERS = model.CheckUsers;
                            jobResult.JOBENDTIME = model.JobEndTime;
                        }
                        else
                        {
                            db.JOBRESULT.Add(new JOBRESULT()
                            {
                                UNIQUEID = UniqueID,
                                ORGANIZATIONDESCRIPTION = model.OrganizationDescription,
                                JOBUNIQUEID = model.JobUniqueID,
                                BEGINDATE = model.BeginDate,
                                ENDDATE = model.EndDate,
                                ISCOMPLETED = model.IsCompleted ? "Y" : "N",
                                ISNEEDVERIFY = model.IsNeedVerify ? "Y" : "N",
                                DESCRIPTION = model.Description,
                                ARRIVESTATUS = model.ArriveStatus,
                                ARRIVESTATUSLABELCLASS = model.ArriveStatusLabelClass,
                                BEGINTIME = model.BeginTime,
                                COMPLETERATE = model.CompleteRate,
                                COMPLETERATELABELCLASS = model.CompleteRateLabelClass,
                                ENDTIME = model.EndTime,
                                HAVEABNORMAL = model.HaveAbnormal ? "Y" : "N",
                                HAVEALERT = model.HaveAlert ? "Y" : "N",
                                TIMESPAN = model.TimeSpan,
                                OVERTIMEREASON = OverTimeReason,
                                OVERTIMEUSER = OverTimeUser,
                                UNPATROLREASON = UnPatrolReason,
                                UNPATROLUSER = UnPatrolUser,
                                JOBUSERS = model.JobUsers,
                                CHECKUSERS = model.CheckUsers,
                                JOBENDTIME = model.JobEndTime
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var job = (from j in db.JOB
                               join r in db.ROUTE
                               on j.ROUTEUNIQUEID equals r.UNIQUEID
                               where j.UNIQUEID == JobUniqueID
                               select new
                               {
                                   Job = j,
                                   Route = r
                               }).First();

                    var organization = OrganizationDataAccessor.GetOrganization(job.Route.ORGANIZATIONUNIQUEID);

                    model = new JobResultModel()
                    {
                        UniqueID = JobResultUniqueID,
                        OrganizationDescription = organization != null ? organization.Description : string.Empty,
                        JobUniqueID = JobUniqueID,
                        BeginDate = JobBeginDate,
                        EndDate = JobEndDate,
                        IsNeedVerify = job.Job.ISNEEDVERIFY=="Y",
                        RouteID = job.Route.ID,
                        RouteName = job.Route.NAME,
                        JobDescription = job.Job.DESCRIPTION,
                        TimeMode = job.Job.TIMEMODE.Value,
                        BeginTime = job.Job.BEGINTIME,
                        EndTime = job.Job.ENDTIME
                    };

                    var jobUserList = db.JOBUSER.Where(x => x.JOBUNIQUEID == job.Job.UNIQUEID).Select(x => x.USERID).OrderBy(x => x).ToList();

                    foreach (var jobUser in jobUserList)
                    {
                        var user = UserDataAccessor.GetUser(jobUser);

                        if (user != null)
                        {
                            model.JobUserList.Add(user);
                        }
                    }

                    var allArriveRecordList = db.ARRIVERECORD.Where(x => x.JOBRESULTUNIQUEID == JobResultUniqueID).ToList();

                    var checkUserList = allArriveRecordList.Select(x => x.USERID).Distinct().OrderBy(x => x).ToList();

                    foreach (var checkUser in checkUserList)
                    {
                        var user = UserDataAccessor.GetUser(checkUser);

                        if (user != null)
                        {
                            model.CheckUserList.Add(user);
                        }
                    }

                    var allCheckResultList = (from c in db.CHECKRESULT
                                              join a in db.ARRIVERECORD
                                              on c.ARRIVERECORDUNIQUEID equals a.UNIQUEID
                                              where a.JOBRESULTUNIQUEID == JobResultUniqueID
                                              select c).ToList();

                    var controlPointList = (from x in db.JOBCONTROLPOINT
                                            join j in db.JOB
                                            on x.JOBUNIQUEID equals j.UNIQUEID
                                            join y in db.ROUTECONTROLPOINT
                                            on new { j.ROUTEUNIQUEID, x.CONTROLPOINTUNIQUEID } equals new { y.ROUTEUNIQUEID, y.CONTROLPOINTUNIQUEID }
                                            join c in db.CONTROLPOINT
                                            on x.CONTROLPOINTUNIQUEID equals c.UNIQUEID
                                            where x.JOBUNIQUEID == job.Job.UNIQUEID
                                            select new
                                            {
                                                UniqueID = c.UNIQUEID,
                                                ID = c.ID,
                                                Description = c.DESCRIPTION,
                                                y.SEQ
                                            }).OrderBy(x => x.SEQ).ToList();

                    foreach (var controlPoint in controlPointList)
                    {
                        var controlPointModel = new ControlPointModel()
                        {
                            UniqueID = controlPoint.UniqueID,
                            ArriveRecordList = allArriveRecordList.Where(x => x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID).OrderBy(x => x.ARRIVEDATE).ThenBy(x => x.ARRIVETIME).Select(x => new ArriveRecordModel
                            {
                                UniqueID = x.UNIQUEID,
                                ArriveDate = x.ARRIVEDATE,
                                ArriveTime = x.ARRIVETIME
                            }).ToList()
                        };

                        var controlPointCheckItemList = (from x in db.JOBCONTROLPOINTCHECKITEM
                                                         join j in db.JOB
                                                         on x.JOBUNIQUEID equals j.UNIQUEID
                                                         join y in db.ROUTECONTROLPOINTCHECKITEM
                                                         on new { j.ROUTEUNIQUEID, x.CONTROLPOINTUNIQUEID, x.CHECKITEMUNIQUEID } equals new { y.ROUTEUNIQUEID, y.CONTROLPOINTUNIQUEID, y.CHECKITEMUNIQUEID }
                                                         join c in db.CONTROLPOINTCHECKITEM
                                                         on new { x.CONTROLPOINTUNIQUEID, x.CHECKITEMUNIQUEID } equals new { c.CONTROLPOINTUNIQUEID, c.CHECKITEMUNIQUEID }
                                                         join i in db.CHECKITEM
                                                         on x.CHECKITEMUNIQUEID equals i.UNIQUEID
                                                         where x.JOBUNIQUEID == job.Job.UNIQUEID && x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID
                                                         select new
                                                         {
                                                             UniqueID = c.CHECKITEMUNIQUEID,
                                                             i.ID,
                                                             i.DESCRIPTION,
                                                             y.SEQ
                                                         }).OrderBy(x => x.SEQ).ToList();

                        foreach (var checkItem in controlPointCheckItemList)
                        {
                            var checkItemModel = new CheckItemModel()
                            {
                                EquipmentUniqueID = "",
                                PartUniqueID = "",
                                CheckItemUniqueID = checkItem.UniqueID
                            };

                            var checkResultList = allCheckResultList.Where(x => x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID && string.IsNullOrEmpty(x.EQUIPMENTUNIQUEID) && string.IsNullOrEmpty(x.PARTUNIQUEID) && x.CHECKITEMUNIQUEID == checkItem.UniqueID).OrderBy(x => x.CHECKDATE).ThenBy(x => x.CHECKTIME).ToList();

                            foreach (var checkResult in checkResultList)
                            {
                                checkItemModel.CheckResultList.Add(new CheckResultModel()
                                {
                                    ArriveRecordUniqueID = checkResult.ARRIVERECORDUNIQUEID,
                                    UniqueID = checkResult.UNIQUEID,
                                    CheckDate = checkResult.CHECKDATE,
                                    CheckTime = checkResult.CHECKTIME,
                                    IsAbnormal = checkResult.ISABNORMAL == "Y",
                                    IsAlert = checkResult.ISALERT == "Y"
                                });
                            }

                            controlPointModel.CheckItemList.Add(checkItemModel);
                        }

                        var equipmentList = (from x in db.JOBEQUIPMENT
                                             join j in db.JOB
                                             on x.JOBUNIQUEID equals j.UNIQUEID
                                             join y in db.ROUTEEQUIPMENT
                                             on new { j.ROUTEUNIQUEID, x.CONTROLPOINTUNIQUEID, x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID } equals new { y.ROUTEUNIQUEID, y.CONTROLPOINTUNIQUEID, y.EQUIPMENTUNIQUEID, y.PARTUNIQUEID }
                                             where x.JOBUNIQUEID == job.Job.UNIQUEID && x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID
                                             select new { x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID, y.SEQ }).OrderBy(x => x.SEQ).ToList();

                        foreach (var equipment in equipmentList)
                        {
                            var equipmentCheckItemList = (from x in db.JOBEQUIPMENTCHECKITEM
                                                          join j in db.JOB
                                                          on x.JOBUNIQUEID equals j.UNIQUEID
                                                          join y in db.ROUTEEQUIPMENTCHECKITEM
                                                          on new { j.ROUTEUNIQUEID, x.CONTROLPOINTUNIQUEID, x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID, x.CHECKITEMUNIQUEID } equals new { y.ROUTEUNIQUEID, y.CONTROLPOINTUNIQUEID, y.EQUIPMENTUNIQUEID, y.PARTUNIQUEID, y.CHECKITEMUNIQUEID }
                                                          join e in db.EQUIPMENT
                                                          on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                                          join c in db.EQUIPMENTCHECKITEM
                                                          on new { x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID, x.CHECKITEMUNIQUEID } equals new { c.EQUIPMENTUNIQUEID, c.PARTUNIQUEID, c.CHECKITEMUNIQUEID }
                                                          join i in db.CHECKITEM
                                                          on x.CHECKITEMUNIQUEID equals i.UNIQUEID
                                                          where x.JOBUNIQUEID == job.Job.UNIQUEID && x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID && x.EQUIPMENTUNIQUEID == equipment.EQUIPMENTUNIQUEID && x.PARTUNIQUEID == equipment.PARTUNIQUEID
                                                          select new
                                                          {
                                                              x.EQUIPMENTUNIQUEID,
                                                              x.PARTUNIQUEID,
                                                              x.CHECKITEMUNIQUEID,
                                                              y.SEQ
                                                          }).OrderBy(x => x.SEQ).ToList();

                            foreach (var checkItem in equipmentCheckItemList)
                            {
                                var checkItemModel = new CheckItemModel()
                                {
                                    EquipmentUniqueID = checkItem.EQUIPMENTUNIQUEID,
                                    PartUniqueID = checkItem.PARTUNIQUEID,
                                    CheckItemUniqueID = checkItem.CHECKITEMUNIQUEID
                                };

                                var checkResultList = allCheckResultList.Where(x => x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID && x.EQUIPMENTUNIQUEID == checkItem.EQUIPMENTUNIQUEID && x.PARTUNIQUEID == checkItem.PARTUNIQUEID && x.CHECKITEMUNIQUEID == checkItem.CHECKITEMUNIQUEID).OrderBy(x => x.CHECKDATE).ThenBy(x => x.CHECKTIME).ToList();

                                foreach (var checkResult in checkResultList)
                                {
                                    checkItemModel.CheckResultList.Add(new CheckResultModel()
                                    {
                                        ArriveRecordUniqueID = checkResult.ARRIVERECORDUNIQUEID,
                                        UniqueID = checkResult.UNIQUEID,
                                        CheckDate = checkResult.CHECKDATE,
                                        CheckTime = checkResult.CHECKTIME,
                                        IsAbnormal = checkResult.ISABNORMAL == "Y",
                                        IsAlert = checkResult.ISALERT == "Y"
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
