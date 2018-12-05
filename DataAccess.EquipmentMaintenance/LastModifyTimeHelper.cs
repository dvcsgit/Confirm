using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;

namespace DataAccess.EquipmentMaintenance
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

                using (EDbEntities db = new EDbEntities())
                {
                    userList = db.JobUser.Where(x => x.JobUniqueID == JobUniqueID).Select(x => x.UserID).ToList();

                    lastModifyTimeList.Add(db.Job.First(x => x.UniqueID == JobUniqueID).LastModifyTime);

                    lastModifyTimeList.AddRange(db.OverTimeReason.Select(x => x.LastModifyTime).ToList());

                    lastModifyTimeList.AddRange(db.UnRFIDReason.Select(x => x.LastModifyTime).ToList());

                    lastModifyTimeList.AddRange((from jobControlPoint in db.JobControlPoint
                                                 join controlPoint in db.ControlPoint
                                                 on jobControlPoint.ControlPointUniqueID equals controlPoint.UniqueID
                                                 where jobControlPoint.JobUniqueID == JobUniqueID
                                                 select controlPoint.LastModifyTime).ToList());

                    lastModifyTimeList.AddRange((from jobEquipment in db.JobEquipment
                                                 join equipment in db.Equipment
                                                 on jobEquipment.EquipmentUniqueID equals equipment.UniqueID
                                                 where jobEquipment.JobUniqueID == JobUniqueID
                                                 select equipment.LastModifyTime).ToList());

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

                    var jobEquipmentCheckItemList = (from jobEquipmentCheckItem in db.JobEquipmentCheckItem
                                                     join checkItem in db.CheckItem
                                                     on jobEquipmentCheckItem.CheckItemUniqueID equals checkItem.UniqueID
                                                     where jobEquipmentCheckItem.JobUniqueID == JobUniqueID
                                                     select checkItem).ToList();

                    foreach (var checkItem in jobEquipmentCheckItemList)
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
