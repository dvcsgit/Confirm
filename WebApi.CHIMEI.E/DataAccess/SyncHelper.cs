using DbEntity.MSSQL.EquipmentMaintenance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Utility;
using Utility.Models;
using WebApi.CHIMEI.E.Models;

namespace WebApi.CHIMEI.E.DataAccess
{
    public class SyncHelper
    {
        public static RequestResult GetJobList(string UserID, string CheckDate)
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = new List<JobItem>();

                using (EDbEntities db = new EDbEntities())
                {
                    var jobList = (from x in db.JobUser
                                   join j in db.Job
                                   on x.JobUniqueID equals j.UniqueID
                                   join r in db.Route
                                   on j.RouteUniqueID equals r.UniqueID
                                   join y in db.JobResult
                                   on j.UniqueID equals y.JobUniqueID
                                   where x.UserID == UserID && !y.IsCompleted
                                   select new { Job = j, Route = r, JobResult = y }).OrderByDescending(x => x.JobResult.BeginDate).ThenBy(x => x.JobResult.Description).ToList();

                    foreach (var job in jobList)
                    {
                        itemList.Add(new JobItem()
                        {
                            JobUniqueID = job.Job.UniqueID,
                            JobDescription = job.Job.Description,
                            RouteID = job.Route.ID,
                            RouteName = job.Route.Name,
                            BeginDate = job.Job.BeginDate,
                            BeginTime = job.Job.BeginTime,
                            EndDate = job.Job.EndDate,
                            EndTime = job.Job.EndTime,
                            CompleteRate = job.JobResult.CompleteRate
                        });
                    }
                }

                result.ReturnData(itemList);
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