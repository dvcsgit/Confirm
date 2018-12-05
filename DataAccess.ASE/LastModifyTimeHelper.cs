using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using DbEntity.ASE;
using Utility.Models;

namespace DataAccess.ASE
{
    public class LastModifyTimeHelper
    {
        public static RequestResult Get(List<string> JobList)
        {
            RequestResult result = new RequestResult();

            try
            {
                var lastModifyTimeList = new Dictionary<string, string>();

                foreach (var jobUniqueID in JobList)
                {
                    lastModifyTimeList.Add(jobUniqueID, Get(jobUniqueID));
                }

                result.ReturnData(lastModifyTimeList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static string Get(string JobUniqueID)
        {
            string lastModifyTime = string.Empty;

            try
            {
                var lastModifyTimeList = new List<DateTime?>();

                var userList = new List<string>();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    userList = db.JOBUSER.Where(x => x.JOBUNIQUEID == JobUniqueID).Select(x => x.USERID).ToList();

                    lastModifyTimeList.Add(db.JOB.First(x => x.UNIQUEID == JobUniqueID).LASTMODIFYTIME);

                    lastModifyTimeList.AddRange(db.OVERTIMEREASON.Select(x => x.LASTMODIFYTIME).ToList());

                    lastModifyTimeList.AddRange(db.UNRFIDREASON.Select(x => x.LASTMODIFYTIME).ToList());

                    lastModifyTimeList.AddRange((from jobControlPoint in db.JOBCONTROLPOINT
                                                 join controlPoint in db.CONTROLPOINT
                                                 on jobControlPoint.CONTROLPOINTUNIQUEID equals controlPoint.UNIQUEID
                                                 where jobControlPoint.JOBUNIQUEID == JobUniqueID
                                                 select controlPoint.LASTMODIFYTIME).ToList());

                    lastModifyTimeList.AddRange((from jobEquipment in db.JOBEQUIPMENT
                                                 join equipment in db.EQUIPMENT
                                                 on jobEquipment.EQUIPMENTUNIQUEID equals equipment.UNIQUEID
                                                 where jobEquipment.JOBUNIQUEID == JobUniqueID
                                                 select equipment.LASTMODIFYTIME).ToList());

                    var jobControlPointCheckItemList = (from jobControlPointCheckItem in db.JOBCONTROLPOINTCHECKITEM
                                                        join checkItem in db.CHECKITEM
                                                        on jobControlPointCheckItem.CHECKITEMUNIQUEID equals checkItem.UNIQUEID
                                                        where jobControlPointCheckItem.JOBUNIQUEID == JobUniqueID
                                                        select checkItem).ToList();

                    foreach (var checkItem in jobControlPointCheckItemList)
                    {
                        lastModifyTimeList.Add(checkItem.LASTMODIFYTIME);

                        var abnormalReasonList = (from x in db.CHECKITEMABNORMALREASON
                                                  join a in db.ABNORMALREASON
                                                  on x.ABNORMALREASONUNIQUEID equals a.UNIQUEID
                                                  where x.CHECKITEMUNIQUEID == checkItem.UNIQUEID
                                                  select a).ToList();

                        foreach (var abnormalReason in abnormalReasonList)
                        {
                            lastModifyTimeList.Add(abnormalReason.LASTMODIFYTIME);

                            lastModifyTimeList.AddRange((from x in db.ABNORMALREASONHANDLINGMETHOD
                                                         join h in db.HANDLINGMETHOD
                                                         on x.HANDLINGMETHODUNIQUEID equals h.UNIQUEID
                                                         where x.ABNORMALREASONUNIQUEID == abnormalReason.UNIQUEID
                                                         select h.LASTMODIFYTIME).ToList());
                        }
                    }

                    var jobEquipmentCheckItemList = (from jobEquipmentCheckItem in db.JOBEQUIPMENTCHECKITEM
                                                     join checkItem in db.CHECKITEM
                                                     on jobEquipmentCheckItem.CHECKITEMUNIQUEID equals checkItem.UNIQUEID
                                                     where jobEquipmentCheckItem.JOBUNIQUEID == JobUniqueID
                                                     select checkItem).ToList();

                    foreach (var checkItem in jobEquipmentCheckItemList)
                    {
                        lastModifyTimeList.Add(checkItem.LASTMODIFYTIME);

                        var abnormalReasonList = (from x in db.CHECKITEMABNORMALREASON
                                                  join a in db.ABNORMALREASON
                                                  on x.ABNORMALREASONUNIQUEID equals a.UNIQUEID
                                                  where x.CHECKITEMUNIQUEID == checkItem.UNIQUEID
                                                  select a).ToList();

                        foreach (var abnormalReason in abnormalReasonList)
                        {
                            lastModifyTimeList.Add(abnormalReason.LASTMODIFYTIME);

                            lastModifyTimeList.AddRange((from x in db.ABNORMALREASONHANDLINGMETHOD
                                                         join h in db.HANDLINGMETHOD
                                                         on x.HANDLINGMETHODUNIQUEID equals h.UNIQUEID
                                                         where x.ABNORMALREASONUNIQUEID == abnormalReason.UNIQUEID
                                                         select h.LASTMODIFYTIME).ToList());
                        }
                    }
                }

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    foreach (var user in userList)
                    {
                        lastModifyTimeList.Add(db.ACCOUNT.First(x => x.ID == user).LASTMODIFYTIME);
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
