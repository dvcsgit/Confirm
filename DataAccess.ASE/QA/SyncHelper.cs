using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.ASE;
using Models.ASE.DataSync;

namespace DataAccess.ASE.QA
{
    public class SyncHelper
    {
        public static RequestResult GetJobList(string UserID, string CheckDate)
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = new List<JobItem>();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var checkDate = DateTimeHelper.DateString2DateTime(CheckDate).Value;

                    var query1 = (from f in db.QA_CALIBRATIONFORM
                                  join a in db.QA_CALIBRATIONAPPLY
                                  on f.APPLYUNIQUEID equals a.UNIQUEID
                                  where f.CALRESPONSORID == UserID && f.STATUS == "1"
                                  select new
                                  {
                                      UniqueID = f.UNIQUEID,
                                      CalibrateDate = f.ESTCALDATE,
                                      f.VHNO,
                                      EquipmentUniqueID = a.EQUIPMENTUNIQUEID,
                                      CalibrateType = a.CALTYPE
                                  }).AsQueryable();

                    var query2 = (from f in db.QA_CALIBRATIONFORM
                                  join n in db.QA_CALIBRATIONNOTIFY
                                  on f.NOTIFYUNIQUEID equals n.UNIQUEID
                                  where f.CALRESPONSORID == UserID && f.STATUS == "1"
                                  select new
                                  {
                                      UniqueID = f.UNIQUEID,
                                      CalibrateDate = f.ESTCALDATE,
                                      f.VHNO,
                                      EquipmentUniqueID = n.EQUIPMENTUNIQUEID,
                                      CalibrateType = n.CALTYPE
                                  }).AsQueryable();

                    var formList = query1.Union(query2).ToList();

                    foreach (var form in formList)
                    {
                        var valid = false;

                        if (form.CalibrateType == "IL" || form.CalibrateType == "EL")
                        {
                            var last = db.QA_CALIBRATIONFORMSTEPLOG.Where(x => x.FORMUNIQUEID == form.UniqueID).OrderByDescending(x => x.SEQ).FirstOrDefault();

                            if (last != null && last.STEP == "1")
                            {
                                valid = true;
                            }
                            else
                            {
                                valid = false;
                            }
                        }
                        else
                        {
                            valid = true;
                        }

                        if (valid)
                        {
                            var equipment = EquipmentHelper.Get(null, form.EquipmentUniqueID);

                            var temp = db.QA_CALIBRATIONFORMDETAIL.Where(x => x.FORMUNIQUEID == form.UniqueID).ToList();

                            itemList.Add(new JobItem()
                            {
                                JobUniqueID = form.UniqueID,
                                JobDescription = equipment.CalNo,
                                RouteID = form.VHNO,
                                RouteName = equipment.IchiDisplay,
                                BeginDate = form.CalibrateDate,
                                BeginTime = "",
                                EndDate = form.CalibrateDate,
                                EndTime = "",
                                CheckItemCount = temp.Count,
                                CheckedItemCount = temp.Count(x => x.READINGVALUE.HasValue)
                            });
                        }
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
