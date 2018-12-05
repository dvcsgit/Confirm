using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.ASE;
using Models.Authenticated;
using Models.EquipmentMaintenance.Calendar;

namespace DataAccess.ASE
{
    public class CalendarDataAccessor
    {
        public static RequestResult GetEvents(bool Patrol, bool MaintenanceForm, bool RepairForm, DateTime Begin, DateTime End, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var eventList = new List<Event>();

                    if (Patrol)
                    {
                        var itemList = new List<JobItem>();

                        var jobList = (from jobUser in db.JOBUSER
                                       join job in db.JOB
                                       on jobUser.JOBUNIQUEID equals job.UNIQUEID
                                       join route in db.ROUTE
                                       on job.ROUTEUNIQUEID equals route.UNIQUEID
                                       where jobUser.USERID == Account.ID
                                       select new
                                       {
                                           Route = route,
                                           Job = job
                                       }).ToList();

                        var date = Begin;

                        while (date <= End)
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

                                    if (!itemList.Any(x => x.JobResultUniqueID == jobResult.UNIQUEID))
                                    {
                                        itemList.Add(new JobItem()
                                        {
                                            JobResultUniqueID = jobResult.UNIQUEID,
                                            JobDescription = jobResult.DESCRIPTION,
                                            BeginDate = DateTimeHelper.DateString2DateTime(jobResult.BEGINDATE).Value,
                                            EndDate = DateTimeHelper.DateString2DateTime(jobResult.ENDDATE).Value,
                                            CompleteRate = jobResult.COMPLETERATE,
                                            BeginTime = jobResult.BEGINTIME,
                                            EndTime = jobResult.ENDTIME
                                        });
                                    }
                                }
                            }

                            date = date.AddDays(1);
                        }

                        eventList.AddRange((from x in itemList
                                            select new Event
                                            {
                                                id = Guid.NewGuid().ToString(),
                                                title = x.Display,
                                                start = x.Begin,
                                                end = x.End,
                                                allDay = x.IsAllDay,
                                                color = x.Color,
                                                IsMaintenanceForm = false,
                                                IsRepairForm = false,
                                                VHNO = string.Empty
                                            }).ToList());
                    }

                    if (MaintenanceForm)
                    {
                        var mjobItemList = new List<MJobItem>();

                        var mjobList = (from j in db.MJOB
                                        join x in db.MJOBUSER
                                        on j.UNIQUEID equals x.MJOBUNIQUEID
                                        where x.USERID == Account.ID
                                        select j).ToList();

                         var date = Begin;

                         while (date <= End)
                         {
                             foreach (var job in mjobList)
                             {
                                 if (JobCycleHelper.IsInCycle(date, job.BEGINDATE.Value, job.ENDDATE, job.CYCLECOUNT.Value, job.CYCLEMODE))
                                 {
                                     DateTime beginDate, endDate;

                                     JobCycleHelper.GetDateSpan(date, job.BEGINDATE.Value, job.ENDDATE, job.CYCLECOUNT.Value, job.CYCLEMODE, out beginDate, out endDate);

                                     if (!mjobItemList.Any(x => x.UniqueID == job.UNIQUEID && x.CycleBeginDate == beginDate && x.CycleEndDate == endDate))
                                     {
                                         var mformList = db.MFORM.Where(x => x.MJOBUNIQUEID == job.UNIQUEID && x.CYCLEBEGINDATE == beginDate && x.CYCLEENDDATE == endDate).ToList();

                                         if (mformList.Count > 0)
                                         {
                                             mjobItemList.Add(new MJobItem()
                                             {
                                                 UniqueID = job.UNIQUEID,
                                                 CycleBeginDate = beginDate,
                                                 CycleEndDate = endDate,
                                                 Subject = job.DESCRIPTION,
                                                 FormList = mformList.Select(x => new MFormItem()
                                                 {
                                                     VHNO = x.VHNO,
                                                     Status = x.STATUS,
                                                     EstBeginDate = x.ESTBEGINDATE,
                                                     EstEndDate = x.ESTENDDATE
                                                 }).ToList()
                                             });
                                         }
                                     }
                                 }
                             }

                             date = date.AddDays(1);
                         }

                         eventList.AddRange((from x in mjobItemList
                                            select new Event
                                            {
                                                id = Guid.NewGuid().ToString(),
                                                title = x.Display,
                                                start = x.Begin,
                                                end = x.End,
                                                allDay = true,
                                                color = x.Color,
                                                IsMaintenanceForm = true,
                                                IsRepairForm = false,
                                                VHNO = x.VHNO
                                            }).ToList());
                    }

                    if (RepairForm)
                    {
                        var queryDateList = new List<DateTime>();

                        var date = Begin;

                        while (date <= End)
                        {
                            queryDateList.Add(date);

                           date= date.AddDays(1);
                        }

                        var rFormItemList = new List<RFormItem>();

                        var rFormList = db.RFORM.Where(x => x.TAKEJOBUSERID == Account.ID && x.ESTBEGINDATE.HasValue && x.ESTENDDATE.HasValue).ToList();

                        foreach (var rForm in rFormList)
                        {
                            var estDateList = new List<DateTime>();

                            date = rForm.ESTBEGINDATE.Value;

                            while (date <= rForm.ESTENDDATE.Value)
                            {
                                estDateList.Add(date);

                                date = date.AddDays(1);
                            }

                            if (queryDateList.Intersect(estDateList).Count() > 0)
                            {
                                rFormItemList.Add(new RFormItem()
                                {
                                    UniqueID = rForm.UNIQUEID,
                                    VHNO = rForm.VHNO,
                                    EstBeginDate = rForm.ESTBEGINDATE.Value,
                                    EstEndDate = rForm.ESTENDDATE.Value,
                                    Status = rForm.STATUS,
                                    Subject = rForm.SUBJECT
                                });
                            }
                        }

                        eventList.AddRange((from x in rFormItemList
                                            select new Event
                                            {
                                                id = Guid.NewGuid().ToString(),
                                                title = x.Display,
                                                start = x.Begin,
                                                end = x.End,
                                                allDay = true,
                                                color = x.Color,
                                                IsMaintenanceForm = false,
                                                IsRepairForm = true,
                                                VHNO = x.VHNO
                                            }).ToList());
                    }


                    result.ReturnData(eventList.ToArray());
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
