using System;
using System.Linq;
using System.Web.Mvc;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.ASE;
using Models.Authenticated;
using Models.EquipmentMaintenance.ProgressQuery;

namespace DataAccess.ASE
{
    public class ProgressQueryHelper
    {
        public static RequestResult Query(Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new SummaryViewModel();

                var jobResultList = new List<JOBRESULT>();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var jobList = new List<JOB>();

                    if (db.ORGANIZATION.Any(x => x.MANAGERUSERID == Account.ID))
                    {
                        jobList = (from j in db.JOB
                                   join r in db.ROUTE
                                   on j.ROUTEUNIQUEID equals r.UNIQUEID
                                   where Account.QueryableOrganizationUniqueIDList.Contains(r.ORGANIZATIONUNIQUEID)
                                   select j).ToList();
                    }
                    else
                    {
                        jobList = (from x in db.JOBUSER
                                   join j in db.JOB
                                   on x.JOBUNIQUEID equals j.UNIQUEID
                                   join r in db.ROUTE
                                   on j.ROUTEUNIQUEID equals r.UNIQUEID
                                   where x.USERID == Account.ID
                                   select j).ToList();
                    }

                    foreach (var job in jobList)
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

                            if (jobResult != null && !jobResultList.Any(x => x.UNIQUEID == jobResult.UNIQUEID))
                            {
                                jobResultList.Add(jobResult);
                            }
                        }
                    }
                }

                model.ItemList.Add(new SummaryItem()
                {
                    BoxColor = "infobox-red",
                    Icon = "fa-exclamation-circle",
                    Count = jobResultList.Count(x => x.COMPLETERATE == string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.UnPatrol)),
                    Text = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.UnPatrol)
                });

                model.ItemList.Add(new SummaryItem()
                {
                    BoxColor = "infobox-red",
                    Icon = "fa-check",
                    Count = jobResultList.Count(x => x.COMPLETERATE.StartsWith(Resources.Resource.Incomplete)),
                    Text = Resources.Resource.Incomplete
                });

                model.ItemList.Add(new SummaryItem()
                {
                    BoxColor = "infobox-blue",
                    Icon = "fa-check",
                    Count = jobResultList.Count(x => x.COMPLETERATE.StartsWith(Resources.Resource.Processing)),
                    Text = Resources.Resource.Processing
                });

                model.ItemList.Add(new SummaryItem()
                {
                    BoxColor = "infobox-green",
                    Icon = "fa-check",
                    Count = jobResultList.Count(x => x.COMPLETERATE == Resources.Resource.Completed),
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

        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = new List<JobResultModel>();

                var jobResultList = new List<JOBRESULT>();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (!string.IsNullOrEmpty(Parameters.JobResultUniqueID))
                    {
                        jobResultList.Add(db.JOBRESULT.First(x => x.UNIQUEID == Parameters.JobResultUniqueID));
                    }
                    else
                    {
                        var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                        var query = (from j in db.JOB
                                     join r in db.ROUTE
                                     on j.ROUTEUNIQUEID equals r.UNIQUEID
                                     where downStreamOrganizationList.Contains(r.ORGANIZATIONUNIQUEID) && Account.QueryableOrganizationUniqueIDList.Contains(r.ORGANIZATIONUNIQUEID)
                                     select new
                                     {
                                         Job = j,
                                         Route = r
                                     }).AsQueryable();

                        if (!string.IsNullOrEmpty(Parameters.RouteUniqueID))
                        {
                            query = query.Where(x => x.Job.ROUTEUNIQUEID == Parameters.RouteUniqueID);
                        }

                        if (!string.IsNullOrEmpty(Parameters.JobUniqueID))
                        {
                            query = query.Where(x => x.Job.UNIQUEID == Parameters.JobUniqueID);
                        }

                        var jobList = query.ToList();

                        var date = Parameters.BeginDate;
                        var end = Parameters.EndDate;

                        while (date <= end)
                        {
                            foreach (var job in jobList)
                            {
                                if (JobCycleHelper.IsInCycle(date, job.Job.BEGINDATE.Value, job.Job.ENDDATE, job.Job.CYCLECOUNT.Value, job.Job.CYCLEMODE))
                                {
                                    DateTime beginDate, endDate;

                                    JobCycleHelper.GetDateSpan(date, job.Job.BEGINDATE.Value, job.Job.ENDDATE, job.Job.CYCLECOUNT.Value, job.Job.CYCLEMODE, out beginDate, out endDate);

                                    var beginDateString = DateTimeHelper.DateTime2DateString(beginDate);
                                    var endDateString = DateTimeHelper.DateTime2DateString(endDate);

                                    var jobResult = db.JOBRESULT.FirstOrDefault(x => x.JOBUNIQUEID == job.Job.UNIQUEID && x.BEGINDATE == beginDateString && x.ENDDATE == endDateString);

                                    if (jobResult == null)
                                    {
                                        JobResultDataAccessor.Refresh(Guid.NewGuid().ToString(), job.Job.UNIQUEID, beginDateString, endDateString);

                                        jobResult = db.JOBRESULT.FirstOrDefault(x => x.JOBUNIQUEID == job.Job.UNIQUEID && x.BEGINDATE == beginDateString && x.ENDDATE == endDateString);
                                    }

                                    if (jobResult != null && !jobResultList.Any(x => x.UNIQUEID == jobResult.UNIQUEID))
                                    {
                                        jobResultList.Add(jobResult);
                                    }
                                }
                            }

                            date = date.AddDays(1);
                        }
                    }

                    foreach (var jobResult in jobResultList)
                    {
                        var allArriveRecordList = db.ARRIVERECORD.Where(x => x.JOBRESULTUNIQUEID == jobResult.UNIQUEID).ToList();

                        var allCheckResultList = (from c in db.CHECKRESULT
                                                  join a in db.ARRIVERECORD
                                                  on c.ARRIVERECORDUNIQUEID equals a.UNIQUEID
                                                  where a.JOBRESULTUNIQUEID == jobResult.UNIQUEID
                                                  select c).ToList();

                        var item = new JobResultModel()
                        {
                            UniqueID = jobResult.UNIQUEID,
                            OrganizationDescription = jobResult.ORGANIZATIONDESCRIPTION,
                            JobUniqueID = jobResult.JOBUNIQUEID,
                            BeginDate = jobResult.BEGINDATE,
                            EndDate = jobResult.ENDDATE,
                            BeginTime = jobResult.BEGINTIME,
                            EndTime = jobResult.ENDTIME,
                            ArriveStatus = jobResult.ARRIVESTATUS,
                            ArriveStatusLabelClass = jobResult.ARRIVESTATUSLABELCLASS,
                            CheckUsers = jobResult.CHECKUSERS,
                            CompleteRate = jobResult.COMPLETERATE,
                            CompleteRateLabelClass = jobResult.COMPLETERATELABELCLASS,
                            Description = jobResult.DESCRIPTION,
                            HaveAbnormal = jobResult.HAVEABNORMAL == "Y",
                            HaveAlert = jobResult.HAVEALERT == "Y",
                            JobUsers = jobResult.JOBUSERS,
                            OverTimeReason = jobResult.OVERTIMEREASON,
                            TimeSpan = jobResult.TIMESPAN,
                            UnPatrolReason = jobResult.UNPATROLREASON
                        };

                        var controlPointList = (from x in db.JOBCONTROLPOINT
                                                join j in db.JOB
                                                on x.JOBUNIQUEID equals j.UNIQUEID
                                                join y in db.ROUTECONTROLPOINT
                                                on new { j.ROUTEUNIQUEID, x.CONTROLPOINTUNIQUEID } equals new { y.ROUTEUNIQUEID, y.CONTROLPOINTUNIQUEID }
                                                join c in db.CONTROLPOINT
                                                on x.CONTROLPOINTUNIQUEID equals c.UNIQUEID
                                                where x.JOBUNIQUEID == jobResult.JOBUNIQUEID
                                                select new
                                                {
                                                    UniqueID = c.UNIQUEID,
                                                    ID = c.ID,
                                                    Description = c.DESCRIPTION,
                                                    x.MINTIMESPAN,
                                                    y.SEQ
                                                }).OrderBy(x => x.SEQ).ToList();

                        foreach (var controlPoint in controlPointList)
                        {
                            var controlPointModel = new ControlPointModel()
                            {
                                UniqueID = controlPoint.UniqueID,
                                JobResultUniqueID = jobResult.UNIQUEID,
                                BeginDate = jobResult.BEGINDATE,
                                EndDate = jobResult.ENDDATE,
                                ID = controlPoint.ID,
                                Description = controlPoint.Description,
                                MinTimeSpan = controlPoint.MINTIMESPAN,
                                ArriveRecordList = allArriveRecordList.Where(x => x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID).OrderBy(x => x.ARRIVEDATE).ThenBy(x => x.ARRIVETIME).Select(x => new ArriveRecordModel
                                {
                                    UniqueID = x.UNIQUEID,
                                    ArriveDate = x.ARRIVEDATE,
                                    ArriveTime = x.ARRIVETIME,
                                    User = UserDataAccessor.GetUser(x.USERNAME),
                                    UnRFIDReasonID = x.UNRFIDREASONID,
                                    UnRFIDReasonDescription = x.UNRFIDREASONDESCRIPTION,
                                    UnRFIDReasonRemark = x.UNRFIDREASONREMARK,
                                    TimeSpanAbnormalReasonID=x.TIMESPANABNORMALREASONID,
                                    TimeSpanAbnormalReasonDescription = x.TIMESPANABNORMALREASONDESC,
                                    TimeSpanAbnormalReasonRemark = x.TIMESPANABNORMALREASONREMARK,
                                    PhotoList = db.ARRIVERECORDPHOTO.Where(p => p.ARRIVERECORDUNIQUEID == x.UNIQUEID).OrderBy(p => p.SEQ).ToList().Select(p => p.ARRIVERECORDUNIQUEID + "_" + p.SEQ + "." + p.EXTENSION).OrderBy(p => p).ToList()
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
                                                             where x.JOBUNIQUEID == jobResult.JOBUNIQUEID && x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID
                                                             select new
                                                             {
                                                                 UniqueID = c.CHECKITEMUNIQUEID,
                                                                 i.ID,
                                                                 i.DESCRIPTION,
                                                                 LowerLimit = c.ISINHERIT == "Y" ? i.LOWERLIMIT : c.LOWERLIMIT,
                                                                 LowerAlertLimit = c.ISINHERIT == "Y" ? i.LOWERALERTLIMIT : c.LOWERALERTLIMIT,
                                                                 UpperAlertLimit = c.ISINHERIT == "Y" ? i.UPPERALERTLIMIT : c.UPPERALERTLIMIT,
                                                                 UpperLimit = c.ISINHERIT == "Y" ? i.UPPERLIMIT : c.UPPERLIMIT,
                                                                 Unit = c.ISINHERIT == "Y" ? i.UNIT : c.UNIT,
                                                                 Seq = y.SEQ.Value
                                                             }).OrderBy(x => x.Seq).ToList();

                            foreach (var checkItem in controlPointCheckItemList)
                            {
                                var checkItemModel = new CheckItemModel()
                                {
                                    EquipmentID = "",
                                    EquipmentName = "",
                                    PartDescription = "",
                                    CheckItemID = checkItem.ID,
                                    CheckItemDescription = checkItem.DESCRIPTION,
                                    LowerLimit = checkItem.LowerLimit.HasValue ? double.Parse(checkItem.LowerLimit.Value.ToString()) : default(double?),
                                    LowerAlertLimit = checkItem.LowerAlertLimit.HasValue ? double.Parse(checkItem.LowerAlertLimit.Value.ToString()) : default(double?),
                                    UpperAlertLimit = checkItem.UpperAlertLimit.HasValue ? double.Parse(checkItem.UpperAlertLimit.Value.ToString()) : default(double?),
                                    UpperLimit = checkItem.UpperLimit.HasValue ? double.Parse(checkItem.UpperLimit.Value.ToString()) : default(double?),
                                    Unit = checkItem.Unit
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
                                        IsAlert = checkResult.ISALERT == "Y",
                                        Result = checkResult.RESULT,
                                        LowerLimit = checkResult.LOWERLIMIT.HasValue ? double.Parse(checkResult.LOWERLIMIT.Value.ToString()) : default(double?),
                                        LowerAlertLimit = checkResult.LOWERALERTLIMIT.HasValue ? double.Parse(checkResult.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                                        UpperAlertLimit = checkResult.UPPERALERTLIMIT.HasValue ? double.Parse(checkResult.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                                        UpperLimit = checkResult.UPPERLIMIT.HasValue ? double.Parse(checkResult.UPPERLIMIT.Value.ToString()) : default(double?),
                                        Unit = checkResult.UNIT,
                                        PhotoList = db.CHECKRESULTPHOTO.Where(p => p.CHECKRESULTUNIQUEID == checkResult.UNIQUEID).OrderBy(p => p.SEQ).ToList().Select(p => p.CHECKRESULTUNIQUEID + "_" + p.SEQ + "." + p.EXTENSION).ToList(),
                                        AbnormalReasonList = db.CHECKRESULTABNORMALREASON.Where(a => a.CHECKRESULTUNIQUEID == checkResult.UNIQUEID).OrderBy(a => a.ABNORMALREASONID).Select(a => new AbnormalReasonModel
                                        {
                                            Description = a.ABNORMALREASONDESCRIPTION,
                                            Remark = a.ABNORMALREASONREMARK,
                                            HandlingMethodList = db.CHECKRESULTHANDLINGMETHOD.Where(h => h.CHECKRESULTUNIQUEID == checkResult.UNIQUEID && h.ABNORMALREASONUNIQUEID == a.ABNORMALREASONUNIQUEID).OrderBy(h => h.HANDLINGMETHODID).Select(h => new HandlingMethodModel
                                            {
                                                Description = h.HANDLINGMETHODDESCRIPTION,
                                                Remark = h.HANDLINGMETHODREMARK
                                            }).ToList()
                                        }).ToList()
                                    });
                                }

                                controlPointModel.CheckItemList.Add(checkItemModel);
                            }

                            var equipmentList = (from x in db.JOBEQUIPMENT
                                                 join j in db.JOB
                                                 on x.JOBUNIQUEID equals j.UNIQUEID
                                                 join y in db.ROUTEEQUIPMENT
                                                 on new { j.ROUTEUNIQUEID, x.CONTROLPOINTUNIQUEID, x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID } equals new { y.ROUTEUNIQUEID, y.CONTROLPOINTUNIQUEID, y.EQUIPMENTUNIQUEID, y.PARTUNIQUEID }
                                                 where x.JOBUNIQUEID == jobResult.JOBUNIQUEID && x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID
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
                                                              where x.JOBUNIQUEID == jobResult.JOBUNIQUEID && x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID && x.EQUIPMENTUNIQUEID == equipment.EQUIPMENTUNIQUEID && x.PARTUNIQUEID == equipment.PARTUNIQUEID
                                                              select new
                                                              {
                                                                  x.EQUIPMENTUNIQUEID,
                                                                  EquipmentID = e.ID,
                                                                  EquipmentName = e.NAME,
                                                                  x.PARTUNIQUEID,
                                                                  CheckItemUniqueID = x.CHECKITEMUNIQUEID,
                                                                  CheckItemID = i.ID,
                                                                  CheckItemDescription = i.DESCRIPTION,
                                                                  LowerLimit = c.ISINHERIT == "Y" ? i.LOWERLIMIT : c.LOWERLIMIT,
                                                                  LowerAlertLimit = c.ISINHERIT == "Y" ? i.LOWERALERTLIMIT : c.LOWERALERTLIMIT,
                                                                  UpperAlertLimit = c.ISINHERIT == "Y" ? i.UPPERALERTLIMIT : c.UPPERALERTLIMIT,
                                                                  UpperLimit = c.ISINHERIT == "Y" ? i.UPPERLIMIT : c.UPPERLIMIT,
                                                                  Unit = c.ISINHERIT == "Y" ? i.UNIT : c.UNIT,
                                                                  Seq = y.SEQ.Value
                                                              }).OrderBy(x => x.Seq).ToList();

                                foreach (var checkItem in equipmentCheckItemList)
                                {
                                    var part = db.EQUIPMENTPART.FirstOrDefault(x => x.UNIQUEID == checkItem.PARTUNIQUEID);

                                    var checkItemModel = new CheckItemModel()
                                    {
                                        EquipmentID = checkItem.EquipmentID,
                                        EquipmentName = checkItem.EquipmentName,
                                        PartDescription = part != null ? part.DESCRIPTION : string.Empty,
                                        CheckItemID = checkItem.CheckItemID,
                                        CheckItemDescription = checkItem.CheckItemDescription,
                                        LowerLimit = checkItem.LowerLimit.HasValue ? double.Parse(checkItem.LowerLimit.Value.ToString()) : default(double?),
                                        LowerAlertLimit = checkItem.LowerAlertLimit.HasValue ? double.Parse(checkItem.LowerAlertLimit.Value.ToString()) : default(double?),
                                        UpperAlertLimit = checkItem.UpperAlertLimit.HasValue ? double.Parse(checkItem.UpperAlertLimit.Value.ToString()) : default(double?),
                                        UpperLimit = checkItem.UpperLimit.HasValue ? double.Parse(checkItem.UpperLimit.Value.ToString()) : default(double?),
                                        Unit = checkItem.Unit
                                    };

                                    var checkResultList = allCheckResultList.Where(x => x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID && x.EQUIPMENTUNIQUEID == checkItem.EQUIPMENTUNIQUEID && x.PARTUNIQUEID == checkItem.PARTUNIQUEID && x.CHECKITEMUNIQUEID == checkItem.CheckItemUniqueID).OrderBy(x => x.CHECKDATE).ThenBy(x => x.CHECKTIME).ToList();

                                    foreach (var checkResult in checkResultList)
                                    {
                                        checkItemModel.CheckResultList.Add(new CheckResultModel()
                                        {
                                            ArriveRecordUniqueID = checkResult.ARRIVERECORDUNIQUEID,
                                            UniqueID = checkResult.UNIQUEID,
                                            CheckDate = checkResult.CHECKDATE,
                                            CheckTime = checkResult.CHECKTIME,
                                            IsAbnormal = checkResult.ISABNORMAL == "Y",
                                            IsAlert = checkResult.ISALERT == "Y",
                                            Result = checkResult.RESULT,
                                            LowerLimit = checkResult.LOWERLIMIT.HasValue ? double.Parse(checkResult.LOWERLIMIT.Value.ToString()) : default(double?),
                                            LowerAlertLimit = checkResult.LOWERALERTLIMIT.HasValue ? double.Parse(checkResult.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                                            UpperAlertLimit = checkResult.UPPERALERTLIMIT.HasValue ? double.Parse(checkResult.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                                            UpperLimit = checkResult.UPPERLIMIT.HasValue ? double.Parse(checkResult.UPPERLIMIT.Value.ToString()) : default(double?),
                                            Unit = checkResult.UNIT,
                                            PhotoList = db.CHECKRESULTPHOTO.Where(p => p.CHECKRESULTUNIQUEID == checkResult.UNIQUEID).OrderBy(p => p.SEQ).ToList().Select(p => p.CHECKRESULTUNIQUEID + "_" + p.SEQ + "." + p.EXTENSION).ToList(),
                                            AbnormalReasonList = db.CHECKRESULTABNORMALREASON.Where(a => a.CHECKRESULTUNIQUEID == checkResult.UNIQUEID).OrderBy(a => a.ABNORMALREASONID).Select(a => new AbnormalReasonModel
                                            {
                                                Description = a.ABNORMALREASONDESCRIPTION,
                                                Remark = a.ABNORMALREASONREMARK,
                                                HandlingMethodList = db.CHECKRESULTHANDLINGMETHOD.Where(h => h.CHECKRESULTUNIQUEID == checkResult.UNIQUEID && h.ABNORMALREASONUNIQUEID == a.ABNORMALREASONUNIQUEID).OrderBy(h => h.HANDLINGMETHODID).Select(h => new HandlingMethodModel
                                                {
                                                    Description = h.HANDLINGMETHODDESCRIPTION,
                                                    Remark = h.HANDLINGMETHODREMARK
                                                }).ToList()
                                            }).ToList()
                                        });
                                    }

                                    controlPointModel.CheckItemList.Add(checkItemModel);
                                }
                            }

                            item.ControlPointList.Add(controlPointModel);
                        }

                        itemList.Add(item);
                    }
                }

                result.ReturnData(itemList.OrderBy(x => x.BeginDate).ThenBy(x => x.BeginTime).ThenBy(x => x.Description).ToList());
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult Export(List<JobResultModel> Model, Define.EnumExcelVersion ExcelVersion)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ExcelHelper helper = new ExcelHelper(string.Format("{0}_{1}({2})", Resources.Resource.EquipmentPatrolResult, Resources.Resource.ExportTime, DateTimeHelper.DateTime2DateTimeString(DateTime.Now)), ExcelVersion))
                {
                    var jobExcelItemList = new List<JobExcelItem>();

                    var controlPointExcelItemList = new List<ControlPointExcelItem>();

                    var checkItemExcelItemList = new List<CheckItemExcelItem>();

                    foreach (var job in Model)
                    {
                        jobExcelItemList.Add(new JobExcelItem()
                        {
                            IsAbnormal = job.HaveAbnormal ? Resources.Resource.Abnormal : (job.HaveAlert ? Resources.Resource.Warning : ""),
                            JobDescription = job.Description,
                            BeginDate = job.BeginDate,
                            EndDate = job.EndDate,
                            BeginTime = job.BeginTime,
                            EndTime = job.EndTime,
                            CompleteRate = job.CompleteRate,
                            ExecBeginDateTimeString = job.ExecBeginDateTimeString,
                            ExecEndDateTimeString = job.ExecEndDateTimeString,
                            ExecTimeSpan = job.ExecTimeSpan,
                            TimeSpan = job.TimeSpan,
                            User = job.JobUsers,
                            UnPatrolReason = job.UnPatrolReason,
                            ArriveStatus = job.ArriveStatus,
                            OverTimeReason = job.OverTimeReason
                        });

                        foreach (var controlPoint in job.ControlPointList)
                        {
                            if (controlPoint.ArriveRecordList.Count > 0)
                            {
                                foreach (var arriveRecord in controlPoint.ArriveRecordList)
                                {
                                    controlPointExcelItemList.Add(new ControlPointExcelItem()
                                    {
                                        JobDescription = job.Description,
                                        BeginDate = job.BeginDate,
                                        EndDate = job.EndDate,
                                        BeginTime = job.BeginTime,
                                        EndTime = job.EndTime,
                                        IsAbnormal = controlPoint.HaveAbnormal ? Resources.Resource.Abnormal : (controlPoint.HaveAlert ? Resources.Resource.Warning : ""),
                                        ControlPoint = controlPoint.ControlPoint,
                                        CompleteRate = controlPoint.CompleteRate,
                                        TimeSpan = controlPoint.TimeSpan,
                                        ArriveDate = arriveRecord.ArriveDate,
                                        ArriveTime = arriveRecord.ArriveTime,
                                        User = arriveRecord.User.User,
                                        UnRFIDReason = arriveRecord.UnRFIDReason
                                    });
                                }
                            }
                            else
                            {
                                controlPointExcelItemList.Add(new ControlPointExcelItem()
                                {
                                    JobDescription = job.Description,
                                    BeginDate = job.BeginDate,
                                    EndDate = job.EndDate,
                                    BeginTime = job.BeginTime,
                                    EndTime = job.EndTime,
                                    IsAbnormal = controlPoint.HaveAbnormal ? Resources.Resource.Abnormal : (controlPoint.HaveAlert ? Resources.Resource.Warning : ""),
                                    ControlPoint = controlPoint.ControlPoint,
                                    CompleteRate = controlPoint.CompleteRate,
                                    TimeSpan = controlPoint.TimeSpan,
                                    ArriveDate = string.Empty,
                                    ArriveTime = string.Empty,
                                    User = string.Empty,
                                    UnRFIDReason = string.Empty
                                });
                            }

                            foreach (var checkItem in controlPoint.CheckItemList)
                            {
                                if (checkItem.CheckResultList.Count > 0)
                                {
                                    foreach (var checkResult in checkItem.CheckResultList)
                                    {
                                        var arriveRecord = controlPoint.ArriveRecordList.FirstOrDefault(x => x.UniqueID == checkResult.ArriveRecordUniqueID);

                                        checkItemExcelItemList.Add(new CheckItemExcelItem()
                                        {
                                            JobDescription = job.Description,
                                            BeginDate = job.BeginDate,
                                            EndDate = job.EndDate,
                                            BeginTime = job.BeginTime,
                                            EndTime = job.EndTime,
                                            ControlPoint = controlPoint.ControlPoint,
                                            IsAbnormal = checkResult.IsAbnormal ? Resources.Resource.Abnormal : (checkResult.IsAlert ? Resources.Resource.Warning : ""),
                                            Equipment = checkItem.Equipment,
                                            CheckItem = checkItem.CheckItem,
                                            CheckDate = checkResult.CheckDate,
                                            CheckTime = checkResult.CheckTime,
                                            Result = checkResult.Result,
                                            LowerLimit = checkResult.LowerLimit.HasValue ? checkResult.LowerLimit.Value.ToString() : "",
                                            LowerAlertLimit = checkItem.LowerAlertLimit.HasValue ? checkItem.LowerAlertLimit.Value.ToString() : "",
                                            UpperAlertLimit = checkItem.UpperAlertLimit.HasValue ? checkItem.UpperAlertLimit.Value.ToString() : "",
                                            UpperLimit = checkResult.UpperLimit.HasValue ? checkResult.UpperLimit.Value.ToString() : "",
                                            Unit = checkResult.Unit,
                                            User = arriveRecord != null ? arriveRecord.User.User : "",
                                            AbnormalReasons = checkResult.AbnormalReasons
                                        });
                                    }
                                }
                                else
                                {
                                    checkItemExcelItemList.Add(new CheckItemExcelItem()
                                    {
                                        JobDescription = job.Description,
                                        BeginDate = job.BeginDate,
                                        EndDate = job.EndDate,
                                        BeginTime = job.BeginTime,
                                        EndTime = job.EndTime,
                                        ControlPoint = controlPoint.ControlPoint,
                                        IsAbnormal = "",
                                        Equipment = checkItem.Equipment,
                                        CheckItem = checkItem.CheckItem,
                                        CheckDate = string.Empty,
                                        CheckTime = string.Empty,
                                        Result = string.Empty,
                                        LowerLimit = checkItem.LowerLimit.HasValue ? checkItem.LowerLimit.Value.ToString() : "",
                                        LowerAlertLimit = checkItem.LowerAlertLimit.HasValue ? checkItem.LowerAlertLimit.Value.ToString() : "",
                                        UpperAlertLimit = checkItem.UpperAlertLimit.HasValue ? checkItem.UpperAlertLimit.Value.ToString() : "",
                                        UpperLimit = checkItem.UpperLimit.HasValue ? checkItem.UpperLimit.Value.ToString() : "",
                                        Unit = checkItem.Unit,
                                        User = string.Empty,
                                        AbnormalReasons = string.Empty
                                    });
                                }
                            }
                        }
                    }

                    helper.CreateSheet(Resources.Resource.PatrolStatus, new Dictionary<string, Utility.ExcelHelper.ExcelDisplayItem>() 
                    {
                        { "IsAbnormal", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.Abnormal, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "JobDescription", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Route, Resources.Resource.Job), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "BeginDate", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.BeginDate), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "EndDate", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.EndDate), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "BeginTime", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.BeginTime), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "EndTime", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.EndTime), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "CompleteRate", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.CompleteRate, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "ExecBeginDateTimeString", new Utility.ExcelHelper.ExcelDisplayItem() { Name = "執行開始時間", CellType = NPOI.SS.UserModel.CellType.String }},
                        { "ExecEndDateTimeString", new Utility.ExcelHelper.ExcelDisplayItem() { Name = "執行結束時間", CellType = NPOI.SS.UserModel.CellType.String }},
                        { "ExecTimeSpan", new Utility.ExcelHelper.ExcelDisplayItem() { Name = "執行時間區間", CellType = NPOI.SS.UserModel.CellType.String }},
                        { "TimeSpan", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.TimeSpan, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "User", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.JobUser, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "UnPatrolReason", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.UnPatrolReason, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "ArriveStatus", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.ArriveStatus, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "OverTimeReason", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.OverTimeReason, CellType = NPOI.SS.UserModel.CellType.String }}
                    }, jobExcelItemList);

                    helper.CreateSheet(Resources.Resource.ArriveRecord, new Dictionary<string, Utility.ExcelHelper.ExcelDisplayItem>() 
                    {
                        { "JobDescription", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Route, Resources.Resource.Job), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "BeginDate", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.BeginDate), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "EndDate", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.EndDate), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "BeginTime", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.BeginTime), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "EndTime", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.EndTime), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "IsAbnormal", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.Abnormal, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "ControlPoint", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.ControlPoint, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "CompleteRate", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.CompleteRate, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "TimeSpan", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.TimeSpan, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "ArriveDate", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.ArriveDate, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "ArriveTime", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.ArriveTime, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "User", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.ArriveUser, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "UnRFIDReason", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.UnRFIDReason, CellType = NPOI.SS.UserModel.CellType.String }}
                    }, controlPointExcelItemList);

                    helper.CreateSheet(Resources.Resource.CheckResult, new Dictionary<string, Utility.ExcelHelper.ExcelDisplayItem>() 
                    {
                        { "JobDescription", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Route, Resources.Resource.Job), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "BeginDate", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.BeginDate), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "EndDate", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.EndDate), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "BeginTime", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.BeginTime), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "EndTime", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.EndTime), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "ControlPoint", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.ControlPoint, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "IsAbnormal", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.Abnormal, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "Equipment", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.Equipment, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "CheckItem", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.CheckItem, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "CheckDate", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.CheckDate, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "CheckTime", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.CheckTime, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "Result", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.CheckResult, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "LowerLimit", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.LowerLimit, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "LowerAlertLimit", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.LowerAlertLimit, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "UpperAlertLimit", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.UpperAlertLimit, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "UpperLimit", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.UpperLimit, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "Unit", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.Unit, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "User", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.CheckUser, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "AbnormalReasons", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1} {2}", Resources.Resource.AbnormalReason, Resources.Resource.And, Resources.Resource.HandlingMethod), CellType = NPOI.SS.UserModel.CellType.String }}
                    }, checkItemExcelItemList);

                    result.ReturnData(helper.Export());
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
    }
}
