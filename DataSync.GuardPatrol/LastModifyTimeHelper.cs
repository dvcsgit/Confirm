using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Utility;
#if ORACLE
using DbEntity.ORACLE;
using DbEntity.ORACLE.GuardPatrol;
#else
using DbEntity.MSSQL;
using DbEntity.MSSQL.GuardPatrol;
#endif

namespace DataSync.GuardPatrol
{
    public class LastModifyTimeHelper
    {
        public static string Get(string JobUniqueID)
        {
            string lastModifyTime = string.Empty;

            try
            {
                var lastModifyTimeList = new List<DateTime>();

                var userList = new List<string>();

                using (GDbEntities db = new GDbEntities())
                {
                    userList = db.JobUser.Where(x => x.JobUniqueID == JobUniqueID).Select(x => x.UserID).ToList();

                    lastModifyTimeList.Add(db.Job.First(x => x.UniqueID == JobUniqueID).LastModifyTime);

                    lastModifyTimeList.AddRange(db.TimeSpanAbnormalReason.Select(x => x.LastModifyTime).ToList());

                    lastModifyTimeList.AddRange(db.UnPatrolReason.Select(x => x.LastModifyTime).ToList());

                    lastModifyTimeList.AddRange(db.OverTimeReason.Select(x => x.LastModifyTime).ToList());

                    lastModifyTimeList.AddRange(db.UnRFIDReason.Select(x => x.LastModifyTime).ToList());

                    lastModifyTimeList.AddRange((from jobRoute in db.JobRoute
                                                 join route in db.Route
                                                 on jobRoute.RouteUniqueID equals route.UniqueID
                                                 where jobRoute.JobUniqueID == JobUniqueID
                                                 select route.LastModifyTime).ToList());

                    lastModifyTimeList.AddRange((from jobControlPoint in db.JobControlPoint
                                                 join controlPoint in db.ControlPoint
                                                 on jobControlPoint.ControlPointUniqueID equals controlPoint.UniqueID
                                                 where jobControlPoint.JobUniqueID == JobUniqueID
                                                 select controlPoint.LastModifyTime).ToList());

                    var jobControlPointCheckItemList = (from jobControlPointCheckItem in db.JobControlPointCheckItem
                                                        join checkItem in db.CheckItem
                                                        on jobControlPointCheckItem.CheckItemUniqueID equals checkItem.UniqueID
                                                        where jobControlPointCheckItem.JobUniqueID == JobUniqueID
                                                        select checkItem).ToList();

                    foreach (var checkItem in jobControlPointCheckItemList)
                    {
                        lastModifyTimeList.Add(checkItem.LastModifyTime);

                        var abnormalReasonList = (from x in db.CheckItemAbnormalReason
                                                  join a in db.AbnormalReason
                                                  on x.AbnormalReasonUniqueID equals a.UniqueID
                                                  where x.CheckItemUniqueID == checkItem.UniqueID
                                                  select a).ToList();

                        foreach (var abnormalReason in abnormalReasonList)
                        {
                            lastModifyTimeList.Add(abnormalReason.LastModifyTime);

                            lastModifyTimeList.AddRange((from x in db.AbnormalReasonHandlingMethod
                                                         join h in db.HandlingMethod
                                                         on x.HandlingMethodUniqueID equals h.UniqueID
                                                         where x.AbnormalReasonUniqueID == abnormalReason.UniqueID
                                                         select h.LastModifyTime).ToList());
                        }
                    }
                }

                using (DbEntities db = new DbEntities())
                {
                    foreach (var user in userList)
                    {
                        lastModifyTimeList.Add(db.User.First(x => x.ID == user).LastModifyTime);
                    }
                }

                var last = lastModifyTimeList.OrderByDescending(x => x).FirstOrDefault();

                if (last != null)
                {
                    lastModifyTime = DateTimeHelper.DateTime2DateTimeString(last);
                }
            }
            catch (Exception ex)
            {
                lastModifyTime = string.Empty;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return lastModifyTime;
        }
    }
}
