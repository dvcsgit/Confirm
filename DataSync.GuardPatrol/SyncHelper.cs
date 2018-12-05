using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.MSSQL.GuardPatrol;
using Models.GuardPatrol.DataSync;
using DataAccess.GuardPatrol;
using DbEntity.MSSQL;
using DataAccess;

namespace DataSync.GuardPatrol
{
    public class SyncHelper
    {
        public static RequestResult GetJobList(string UserID, string CheckDate)
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = new List<JobItem>();

                using (GDbEntities db = new GDbEntities())
                {
                    #region Patrol
                    if (Config.Modules.Contains(Define.EnumSystemModule.GuardPatrol))
                    {
                        var checkDate = DateTimeHelper.DateString2DateTime(CheckDate).Value;

                        var jobList = (from x in db.JobUser
                                       join j in db.Job
                                       on x.JobUniqueID equals j.UniqueID
                                       where x.UserID == UserID
                                       select new
                                       {
                                           j.UniqueID,
                                           j.Description,
                                           j.BeginDate,
                                           j.EndDate,
                                           j.BeginTime,
                                           j.EndTime,
                                           j.CycleCount,
                                           j.CycleMode
                                       }).Distinct().ToList();

                        foreach (var job in jobList)
                        {
                            if (JobCycleHelper.IsInCycle(checkDate, job.BeginDate, job.EndDate, job.CycleCount, job.CycleMode))
                            {
                                var beginDateString = string.Empty;
                                var endDateString = string.Empty;

                                DateTime beginDate, endDate;

                                JobCycleHelper.GetDateSpan(checkDate, job.BeginDate, job.EndDate, job.CycleCount, job.CycleMode, out beginDate, out endDate);

                                beginDateString = DateTimeHelper.DateTime2DateString(beginDate);
                                endDateString = DateTimeHelper.DateTime2DateString(endDate);

                                var jobRouteList = (from x in db.JobRoute
                                                    join r in db.Route
                                                    on x.RouteUniqueID equals r.UniqueID
                                                    where x.JobUniqueID == job.UniqueID && !x.IsOptional
                                                    select new
                                                    {
                                                        x.RouteUniqueID
                                                    }).ToList();

                                int checkItemCount = 0;
                                int checkedItemCount = 0;

                                foreach (var jobRoute in jobRouteList)
                                {
                                    var checkResultList = db.CheckResult.Where(x => x.JobUniqueID == job.UniqueID && x.RouteUniqueID == jobRoute.RouteUniqueID && string.Compare(x.CheckDate, beginDateString) >= 0 && string.Compare(x.CheckDate, endDateString) <= 0).Select(x => new
                                    {
                                        x.JobUniqueID,
                                        x.RouteUniqueID,
                                        x.ControlPointUniqueID,
                                        x.CheckItemUniqueID
                                    }).Distinct().ToList();

                                    var controlPointCheckItemList = db.JobControlPointCheckItem.Where(x => x.JobUniqueID == job.UniqueID && x.RouteUniqueID == jobRoute.RouteUniqueID).ToList();

                                    var query = (from checkItem in controlPointCheckItemList
                                                 join checkResult in checkResultList
                                                 on new { checkItem.JobUniqueID, checkItem.RouteUniqueID, checkItem.ControlPointUniqueID, checkItem.CheckItemUniqueID } equals new { checkResult.JobUniqueID, checkResult.RouteUniqueID, checkResult.ControlPointUniqueID, checkResult.CheckItemUniqueID } into tmpCheckResult
                                                 from checkResult in tmpCheckResult.DefaultIfEmpty()
                                                 select new
                                                 {
                                                     CheckItem = checkItem,
                                                     IsChecked = checkResult != null
                                                 }).ToList();

                                    checkItemCount += query.Count;
                                    checkedItemCount += query.Count(x => x.IsChecked);
                                }

                                itemList.Add(new JobItem()
                                {
                                    JobUniqueID = job.UniqueID,
                                    JobDescription = job.Description,
                                    BeginDate = beginDate,
                                    BeginTime = job.BeginTime,
                                    EndDate = endDate,
                                    EndTime = job.EndTime,
                                    CheckItemCount = checkItemCount,
                                    CheckedItemCount = checkedItemCount
                                });
                            }
                        }
                    }
                    #endregion
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

        public static RequestResult GetJobList_v2(string UserID, string CheckDate)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (DbEntities db = new DbEntities())
                {
                    var user = db.User.FirstOrDefault(x => x.ID == UserID);

                    if (user != null)
                    {
                        var organization = db.Organization.First(x => x.UniqueID == user.OrganizationUniqueID);

                        result.ReturnData(GetOrganizationModel(organization, CheckDate));
                    }
                    else
                    {
                        result.ReturnFailedMessage("使用者帳號不存在");
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

        private static OrganizationModel GetOrganizationModel(Organization Organization, string CheckDate)
        {
            var model = new OrganizationModel();

            try
            {
                model = new OrganizationModel()
                {
                    UniqueID = Organization.UniqueID,
                    Description = Organization.Description
                };

                using (DbEntities db = new DbEntities())
                {
                    var organizationList = db.Organization.Where(x => x.ParentUniqueID == Organization.UniqueID).OrderBy(x => x.ID).ToList();

                    foreach (var organization in organizationList)
                    {
                        model.OrganizationList.Add(GetOrganizationModel2(organization, CheckDate));
                    }
                }

                using (GDbEntities db = new GDbEntities())
                {
                    var checkDate = DateTimeHelper.DateString2DateTime(CheckDate).Value;

                    var jobList = db.Job.Where(x => x.OrganizationUniqueID == Organization.UniqueID).ToList();

                    foreach (var job in jobList)
                    {
                        if (JobCycleHelper.IsInCycle(checkDate, job.BeginDate, job.EndDate, job.CycleCount, job.CycleMode))
                        {
                            var beginDateString = string.Empty;
                            var endDateString = string.Empty;

                            DateTime beginDate, endDate;

                            JobCycleHelper.GetDateSpan(checkDate, job.BeginDate, job.EndDate, job.CycleCount, job.CycleMode, out beginDate, out endDate);

                            beginDateString = DateTimeHelper.DateTime2DateString(beginDate);
                            endDateString = DateTimeHelper.DateTime2DateString(endDate);

                            var jobRouteList = (from x in db.JobRoute
                                                join r in db.Route
                                                on x.RouteUniqueID equals r.UniqueID
                                                where x.JobUniqueID == job.UniqueID && !x.IsOptional
                                                select new
                                                {
                                                    x.RouteUniqueID,
                                                    r.ID
                                                }).OrderBy(x => x.ID).ToList();

                            int checkItemCount = 0;
                            int checkedItemCount = 0;

                            foreach (var jobRoute in jobRouteList)
                            {
                                var checkResultList = db.CheckResult.Where(x => x.JobUniqueID == job.UniqueID && x.RouteUniqueID == jobRoute.RouteUniqueID && string.Compare(x.CheckDate, beginDateString) >= 0 && string.Compare(x.CheckDate, endDateString) <= 0).Select(x => new
                                {
                                    x.JobUniqueID,
                                    x.RouteUniqueID,
                                    x.ControlPointUniqueID,
                                    x.CheckItemUniqueID
                                }).Distinct().ToList();

                                var controlPointCheckItemList = db.JobControlPointCheckItem.Where(x => x.JobUniqueID == job.UniqueID && x.RouteUniqueID == jobRoute.RouteUniqueID).ToList();

                                var query = (from checkItem in controlPointCheckItemList
                                             join checkResult in checkResultList
                                             on new { checkItem.JobUniqueID, checkItem.RouteUniqueID, checkItem.ControlPointUniqueID, checkItem.CheckItemUniqueID } equals new { checkResult.JobUniqueID, checkResult.RouteUniqueID, checkResult.ControlPointUniqueID, checkResult.CheckItemUniqueID } into tmpCheckResult
                                             from checkResult in tmpCheckResult.DefaultIfEmpty()
                                             select new
                                             {
                                                 CheckItem = checkItem,
                                                 IsChecked = checkResult != null
                                             }).ToList();

                                checkItemCount += query.Count;
                                checkedItemCount += query.Count(x => x.IsChecked);
                            }

                            model.JobList.Add(new JobItem()
                            {
                                JobUniqueID = job.UniqueID,
                                JobDescription = job.Description,
                                BeginDate = beginDate,
                                BeginTime = job.BeginTime,
                                EndDate = endDate,
                                EndTime = job.EndTime,
                                CheckItemCount = checkItemCount,
                                CheckedItemCount = checkedItemCount
                            });
                        }
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

        private static OrganizationModel GetOrganizationModel2(Organization Organization, string CheckDate)
        {
            var model = new OrganizationModel();

            try
            {
                model = new OrganizationModel()
                {
                    UniqueID = Organization.UniqueID,
                    Description = Organization.Description
                };

                //using (DbEntities db = new DbEntities())
                //{
                //    var organizationList = db.Organization.Where(x => x.ParentUniqueID == Organization.UniqueID).OrderBy(x => x.ID).ToList();

                //    foreach (var organization in organizationList)
                //    {
                //        model.OrganizationList.Add(GetOrganizationModel(organization, CheckDate));
                //    }
                //}

                var downStream = OrganizationDataAccessor.GetDownStreamOrganizationList(Organization.UniqueID, true);

                using (GDbEntities db = new GDbEntities())
                {
                    var checkDate = DateTimeHelper.DateString2DateTime(CheckDate).Value;

                    var jobList = db.Job.Where(x => downStream.Contains(x.OrganizationUniqueID)).OrderBy(x => x.Description).ToList();
                    //var jobList = db.Job.Where(x => x.OrganizationUniqueID == Organization.UniqueID).ToList();

                    foreach (var job in jobList)
                    {
                        if (JobCycleHelper.IsInCycle(checkDate, job.BeginDate, job.EndDate, job.CycleCount, job.CycleMode))
                        {
                            var beginDateString = string.Empty;
                            var endDateString = string.Empty;

                            DateTime beginDate, endDate;

                            JobCycleHelper.GetDateSpan(checkDate, job.BeginDate, job.EndDate, job.CycleCount, job.CycleMode, out beginDate, out endDate);

                            beginDateString = DateTimeHelper.DateTime2DateString(beginDate);
                            endDateString = DateTimeHelper.DateTime2DateString(endDate);

                            var jobRouteList = (from x in db.JobRoute
                                                join r in db.Route
                                                on x.RouteUniqueID equals r.UniqueID
                                                where x.JobUniqueID == job.UniqueID && !x.IsOptional
                                                select new
                                                {
                                                    x.RouteUniqueID,
                                                    r.ID
                                                }).OrderBy(x => x.ID).ToList();

                            int checkItemCount = 0;
                            int checkedItemCount = 0;

                            foreach (var jobRoute in jobRouteList)
                            {
                                var checkResultList = db.CheckResult.Where(x => x.JobUniqueID == job.UniqueID && x.RouteUniqueID == jobRoute.RouteUniqueID && string.Compare(x.CheckDate, beginDateString) >= 0 && string.Compare(x.CheckDate, endDateString) <= 0).Select(x => new
                                {
                                    x.JobUniqueID,
                                    x.RouteUniqueID,
                                    x.ControlPointUniqueID,
                                    x.CheckItemUniqueID
                                }).Distinct().ToList();

                                var controlPointCheckItemList = db.JobControlPointCheckItem.Where(x => x.JobUniqueID == job.UniqueID && x.RouteUniqueID == jobRoute.RouteUniqueID).ToList();

                                var query = (from checkItem in controlPointCheckItemList
                                             join checkResult in checkResultList
                                             on new { checkItem.JobUniqueID, checkItem.RouteUniqueID, checkItem.ControlPointUniqueID, checkItem.CheckItemUniqueID } equals new { checkResult.JobUniqueID, checkResult.RouteUniqueID, checkResult.ControlPointUniqueID, checkResult.CheckItemUniqueID } into tmpCheckResult
                                             from checkResult in tmpCheckResult.DefaultIfEmpty()
                                             select new
                                             {
                                                 CheckItem = checkItem,
                                                 IsChecked = checkResult != null
                                             }).ToList();

                                checkItemCount += query.Count;
                                checkedItemCount += query.Count(x => x.IsChecked);
                            }

                            model.JobList.Add(new JobItem()
                            {
                                JobUniqueID = job.UniqueID,
                                JobDescription = job.Description,
                                BeginDate = beginDate,
                                BeginTime = job.BeginTime,
                                EndDate = endDate,
                                EndTime = job.EndTime,
                                CheckItemCount = checkItemCount,
                                CheckedItemCount = checkedItemCount
                            });
                        }
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
