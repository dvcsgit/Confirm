using DataAccess.EquipmentMaintenance;
using DbEntity.MSSQL.EquipmentMaintenance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace JobResult.CrossDayRecover
{
    class Program
    {
        static void Main(string[] args)
        {
            using (EDbEntities db = new EDbEntities())
            {
                var jobList = db.Job.ToList();

                var min = jobList.Min(x => x.BeginDate);

                while (min <= DateTime.Today)
                {
                    foreach (var job in jobList)
                    {
                        if (JobCycleHelper.IsInCycle(min, job.BeginDate, job.EndDate, job.CycleCount, job.CycleMode))
                        {
                            DateTime beginDate, endDate;

                            JobCycleHelper.GetDateSpan(min, job.BeginDate, job.EndDate, job.CycleCount, job.CycleMode, out beginDate, out endDate);

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

                            var errors = db.JobResult.Where(x => x.JobUniqueID == job.UniqueID && x.BeginDate == beginDateString && x.EndDate != endDateString).ToList();

                            if (errors.Count > 0)
                            {
                                foreach (var error in errors)
                                {
                                    db.Database.ExecuteSqlCommand(string.Format("UPDATE ArriveRecord SET JobResultUniqueID = '{0}' WHERE JobResultUniqueID = '{1}'", jobResult.UniqueID, error.UniqueID));

                                    db.JobResult.Remove(error);
                                }

                                db.SaveChanges();

                                JobResultDataAccessor.Refresh(jobResult.UniqueID, job.UniqueID, beginDateString, endDateString);
                            }
                        }
                    }

                    min = min.AddDays(1);
                }
            }
        }

        //static void Main(string[] args)
        //{
        //    using (EDbEntities db = new EDbEntities())
        //    {
        //        var jobList = db.Job.ToList();

        //        foreach (var job in jobList)
        //        {
        //            Console.WriteLine(string.Format("Job({0}/{1})", jobList.IndexOf(job) + 1, jobList.Count));

        //            var jobResultList = db.JobResult.Where(x => x.JobUniqueID == job.UniqueID).ToList();

        //            foreach (var jobResult in jobResultList)
        //            {
        //                Console.WriteLine(string.Format("JobResult({0}/{1})", jobResultList.IndexOf(jobResult) + 1, jobResultList.Count));

        //                var jobBeginDate = DateTimeHelper.DateString2DateTime(jobResult.BeginDate).Value;
        //                var jobEndDate = DateTimeHelper.DateString2DateTime(jobResult.EndDate).Value;

        //                if (!string.IsNullOrEmpty(job.BeginTime) && !string.IsNullOrEmpty(job.EndTime) && string.Compare(job.BeginTime, job.EndTime) > 0)
        //                {
        //                    jobEndDate = jobBeginDate.AddDays(1);
        //                }

        //                var jobBeginDateString = DateTimeHelper.DateTime2DateString(jobBeginDate);
        //                var jobEndDateString = DateTimeHelper.DateTime2DateString(jobEndDate);

        //                jobResult.EndDate = jobEndDateString;

        //                var jobBeginTime = new DateTime();
        //                var jobEndTime = new DateTime();

        //                if (!string.IsNullOrEmpty(job.BeginTime))
        //                { 
        //                    jobBeginTime = DateTimeHelper.DateTimeString2DateTime(jobBeginDateString, job.BeginTime).Value;
        //                }
        //                else
        //                {
        //                    jobBeginTime = jobBeginDate;
        //                }

        //                if (!string.IsNullOrEmpty(job.EndTime))
        //                {
        //                    jobEndTime = DateTimeHelper.DateTimeString2DateTime(jobEndDateString, job.EndTime).Value;
        //                }
        //                else
        //                {
        //                    jobEndTime = jobEndDate;
        //                }

        //                var arriveRecords = db.ArriveRecord.Where(x => x.JobUniqueID == job.UniqueID && (x.ArriveDate == jobBeginDateString || x.ArriveDate == jobEndDateString)).ToList();

        //                foreach (var arriveRecord in arriveRecords)
        //                {
        //                    var arriveTime = DateTimeHelper.DateTimeString2DateTime(arriveRecord.ArriveDate, arriveRecord.ArriveTime).Value;

        //                    if (DateTime.Compare(arriveTime, jobBeginTime) >= 0 && DateTime.Compare(arriveTime, jobEndTime) <= 0)
        //                    {
        //                        arriveRecord.JobResultUniqueID = jobResult.UniqueID;
        //                    }
        //                }

        //                db.SaveChanges();

        //                JobResultDataAccessor.Refresh(jobResult.UniqueID, jobResult.JobUniqueID, jobBeginDateString, jobEndDateString);
        //            }
        //        }
        //    }
        //}
    }
}
