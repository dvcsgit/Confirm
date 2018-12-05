using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.Authenticated;
using Models.EquipmentMaintenance.Calendar;

namespace DataAccess.EquipmentMaintenance
{
    public class CalendarDataAccessor
    {
        public static RequestResult GetEvents(bool Patrol, bool MaintenanceForm, bool RepairForm, DateTime Begin, DateTime End, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var eventList = new List<Event>();

                    if (Patrol)
                    {
                        var itemList = new List<JobItem>();

                        var jobList = (from jobUser in db.JobUser
                                       join job in db.Job
                                       on jobUser.JobUniqueID equals job.UniqueID
                                       join route in db.Route
                                       on job.RouteUniqueID equals route.UniqueID
                                       where jobUser.UserID == Account.ID
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
                                if (JobCycleHelper.IsInCycle(date, job.Job.BeginDate, job.Job.EndDate, job.Job.CycleCount, job.Job.CycleMode))
                                {
                                    DateTime beginDate, endDate;

                                    JobCycleHelper.GetDateSpan(date, job.Job.BeginDate, job.Job.EndDate, job.Job.CycleCount, job.Job.CycleMode, out beginDate, out endDate);

                                    var beginDateString = DateTimeHelper.DateTime2DateString(beginDate);
                                    var endDateString = DateTimeHelper.DateTime2DateString(endDate);

                                    if (!string.IsNullOrEmpty(job.Job.BeginTime) && !string.IsNullOrEmpty(job.Job.EndTime) && string.Compare(job.Job.BeginTime, job.Job.EndTime) > 0)
                                    {
                                        endDateString = DateTimeHelper.DateTime2DateString(endDate.AddDays(1));
                                    }

                                    var jobResult = db.JobResult.FirstOrDefault(x => x.JobUniqueID == job.Job.UniqueID && x.BeginDate == beginDateString && x.EndDate == endDateString);

                                    if (jobResult == null)
                                    {
                                        JobResultDataAccessor.Refresh(Guid.NewGuid().ToString(), job.Job.UniqueID, beginDateString, endDateString);

                                        jobResult = db.JobResult.FirstOrDefault(x => x.JobUniqueID == job.Job.UniqueID && x.BeginDate == beginDateString && x.EndDate == endDateString);
                                    }

                                    if (!itemList.Any(x => x.JobResultUniqueID == jobResult.UniqueID))
                                    {
                                        itemList.Add(new JobItem()
                                        {
                                            JobResultUniqueID = jobResult.UniqueID,
                                            JobDescription = jobResult.Description,
                                            BeginDate = DateTimeHelper.DateString2DateTime(jobResult.BeginDate).Value,
                                            EndDate = DateTimeHelper.DateString2DateTime(jobResult.EndDate).Value,
                                            CompleteRate = jobResult.CompleteRate,
                                            BeginTime = jobResult.BeginTime,
                                            EndTime = jobResult.EndTime
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

                        var mjobList = (from j in db.MJob
                                        join x in db.MJobUser
                                        on j.UniqueID equals x.MJobUniqueID
                                        where x.UserID == Account.ID
                                        select j).ToList();

                        var date = Begin;

                        while (date <= End)
                        {
                            foreach (var job in mjobList)
                            {
                                if (JobCycleHelper.IsInCycle(date, job.BeginDate, job.EndDate, job.CycleCount, job.CycleMode))
                                {
                                    DateTime beginDate, endDate;

                                    JobCycleHelper.GetDateSpan(date, job.BeginDate, job.EndDate, job.CycleCount, job.CycleMode, out beginDate, out endDate);

                                    if (!mjobItemList.Any(x => x.UniqueID == job.UniqueID && x.CycleBeginDate == beginDate && x.CycleEndDate == endDate))
                                    {
                                        var mformList = db.MForm.Where(x => x.MJobUniqueID == job.UniqueID && x.CycleBeginDate == beginDate && x.CycleEndDate == endDate).ToList();

                                        if (mformList.Count > 0)
                                        {
                                            mjobItemList.Add(new MJobItem()
                                            {
                                                UniqueID = job.UniqueID,
                                                CycleBeginDate = beginDate,
                                                CycleEndDate = endDate,
                                                Subject = job.Description,
                                                FormList = mformList.Select(x => new MFormItem()
                                                {
                                                    VHNO = x.VHNO,
                                                    Status = x.Status,
                                                    EstBeginDate = x.EstBeginDate,
                                                    EstEndDate = x.EstEndDate
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

                            date = date.AddDays(1);
                        }

                        var rFormItemList = new List<RFormItem>();

                        var rFormList = db.RForm.Where(x => x.TakeJobUserID == Account.ID && x.EstBeginDate.HasValue && x.EstEndDate.HasValue).ToList();

                        foreach (var rForm in rFormList)
                        {
                            var estDateList = new List<DateTime>();

                            date = rForm.EstBeginDate.Value;

                            while (date <= rForm.EstEndDate.Value)
                            {
                                estDateList.Add(date);

                                date = date.AddDays(1);
                            }

                            if (queryDateList.Intersect(estDateList).Count() > 0)
                            {
                                rFormItemList.Add(new RFormItem()
                                {
                                    UniqueID = rForm.UniqueID,
                                    VHNO = rForm.VHNO,
                                    EstBeginDate = rForm.EstBeginDate.Value,
                                    EstEndDate = rForm.EstEndDate.Value,
                                    Status = rForm.Status,
                                    Subject = rForm.Subject
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
