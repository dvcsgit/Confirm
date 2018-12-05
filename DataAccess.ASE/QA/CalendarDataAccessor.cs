using DbEntity.ASE;
using Models.ASE.QA.Calendar;
using Models.Authenticated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace DataAccess.ASE.QA
{
    public class CalendarDataAccessor
    {
        public static RequestResult GetEvents(DateTime Begin, DateTime End, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = new List<Item>();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    #region CalibrationApply
                    var calibrationApplyList = db.QA_CALIBRATIONAPPLY.Where(x => x.STATUS == "3" && x.ESTCALDATE.HasValue && !string.IsNullOrEmpty(x.EQUIPMENTUNIQUEID) && DateTime.Compare(x.ESTCALDATE.Value, Begin) >= 0 && DateTime.Compare(x.ESTCALDATE.Value, End) <= 0).ToList();

                    foreach (var apply in calibrationApplyList)
                    {
                        var equipment = (from e in db.QA_EQUIPMENT
                                         join i in db.QA_ICHI
                                         on e.ICHIUNIQUEID equals i.UNIQUEID into tmpIchi
                                         from i in tmpIchi.DefaultIfEmpty()
                                         where e.UNIQUEID == apply.EQUIPMENTUNIQUEID
                                         select new
                                         {
                                             e.CALNO,
                                             e.ICHIUNIQUEID,
                                             IchiName = i != null ? i.NAME : "",
                                             e.ICHIREMARK
                                         }).FirstOrDefault();

                        if (equipment != null)
                        {
                            var form = db.QA_CALIBRATIONFORM.FirstOrDefault(x => x.APPLYUNIQUEID == apply.UNIQUEID);

                            if (form != null)
                            {
                                var status = CalibrationFormDataAccessor.GetFormStatus(form.UNIQUEID);

                                var color = Define.Color_Gray;

                                if (status != null)
                                {
                                    switch (status._Status)
                                    {
                                        case "0":
                                        case "7":
                                        case "8":
                                            color = Define.Color_Orange;
                                            break;
                                        case "1":
                                            color = Define.Color_Blue;
                                            break;
                                        case "2":
                                        case "4":
                                        case "6":
                                            color = Define.Color_Red;
                                            break;
                                        case "3":
                                            color = Define.Color_Purple;
                                            break;
                                        case "5":
                                            color = Define.Color_Green;
                                            break;
                                    }
                                }

                                itemList.Add(new Item()
                                {
                                    CalNo = equipment.CALNO,
                                    IchiUniqueID = equipment.ICHIUNIQUEID,
                                    IchiName = equipment.IchiName,
                                    IchiRemark = equipment.ICHIREMARK,
                                    StatusDescription = status != null ? status.Display : "",
                                    Color = color,
                                    EstDate = apply.ESTCALDATE
                                });
                            }
                        }
                    }
                    #endregion

                    #region Calibration Notify
                    var calNotifyList = db.QA_CALIBRATIONNOTIFY.Where(x =>x.STATUS!="4"&& x.ESTCALDATE.HasValue && DateTime.Compare(x.ESTCALDATE.Value, Begin) >= 0 && DateTime.Compare(x.ESTCALDATE.Value, End) <= 0).ToList();

                    foreach (var notify in calNotifyList)
                    {
                        var equipment = (from e in db.QA_EQUIPMENT
                                         join i in db.QA_ICHI
                                         on e.ICHIUNIQUEID equals i.UNIQUEID into tmpIchi
                                         from i in tmpIchi.DefaultIfEmpty()
                                         where e.UNIQUEID == notify.EQUIPMENTUNIQUEID
                                         select new
                                         {
                                             e.CALNO,
                                             e.ICHIUNIQUEID,
                                             IchiName = i != null ? i.NAME : "",
                                             e.ICHIREMARK
                                         }).FirstOrDefault();

                        if (equipment != null)
                        {
                            var form = db.QA_CALIBRATIONFORM.FirstOrDefault(x => x.NOTIFYUNIQUEID == notify.UNIQUEID);

                            if (form != null)
                            {
                                var status = CalibrationFormDataAccessor.GetFormStatus(form.UNIQUEID);

                                var color = Define.Color_Gray;

                                if (status != null)
                                {
                                    switch (status._Status)
                                    {
                                        case "0":
                                        case "7":
                                        case "8":
                                            color = Define.Color_Orange;
                                            break;
                                        case "1":
                                            color = Define.Color_Blue;
                                            break;
                                        case "2":
                                        case "4":
                                        case "6":
                                            color = Define.Color_Red;
                                            break;
                                        case "3":
                                            color = Define.Color_Purple;
                                            break;
                                        case "5":
                                            color = Define.Color_Green;
                                            break;
                                    }
                                }

                                itemList.Add(new Item()
                                {
                                    CalNo = equipment.CALNO,
                                    IchiUniqueID = equipment.ICHIUNIQUEID,
                                    IchiName = equipment.IchiName,
                                    IchiRemark = equipment.ICHIREMARK,
                                    StatusDescription = status != null ? status.Display : "",
                                    Color = color,
                                    EstDate = notify.ESTCALDATE
                                });
                            }
                            else
                            {
                                itemList.Add(new Item()
                                {
                                    CalNo = equipment.CALNO,
                                    IchiUniqueID = equipment.ICHIUNIQUEID,
                                    IchiName = equipment.IchiName,
                                    IchiRemark = equipment.ICHIREMARK,
                                    StatusDescription = "文審中",
                                    Color = Define.Color_Gray,
                                    EstDate = notify.ESTCALDATE
                                });
                            }
                        }
                    }
                    #endregion

                    #region MSAApply
                    var msaApplyList = db.QA_CALIBRATIONAPPLY.Where(x => x.STATUS == "3" && x.ESTMSADATE.HasValue && !string.IsNullOrEmpty(x.EQUIPMENTUNIQUEID) && DateTime.Compare(x.ESTMSADATE.Value, Begin) >= 0 && DateTime.Compare(x.ESTMSADATE.Value, End) <= 0).ToList();

                    foreach (var apply in msaApplyList)
                    {
                        var equipment = (from e in db.QA_EQUIPMENT
                                         join i in db.QA_ICHI
                                         on e.ICHIUNIQUEID equals i.UNIQUEID into tmpIchi
                                         from i in tmpIchi.DefaultIfEmpty()
                                         where e.UNIQUEID == apply.EQUIPMENTUNIQUEID
                                         select new
                                         {
                                             e.MSACALNO,
                                             e.ICHIUNIQUEID,
                                             IchiName = i != null ? i.NAME : "",
                                             e.ICHIREMARK
                                         }).FirstOrDefault();

                        if (equipment != null)
                        {
                            var form = db.QA_MSAFORM.FirstOrDefault(x => x.EQUIPMENTUNIQUEID == apply.EQUIPMENTUNIQUEID && x.ESTMSADATE == apply.ESTMSADATE);

                            if (form != null)
                            {
                                var status = MSAForm_v2DataAccessor.GetFormStatus(form.UNIQUEID);

                                var color = Define.Color_Gray;

                                if (status != null)
                                {
                                    switch (status.StatusCode)
                                    {
                                        case "3":
                                            color = Define.Color_Purple;
                                            break;
                                        case "1":
                                            color = Define.Color_Blue;
                                            break;
                                        case "2":
                                        case "4":
                                        case "6":
                                            color = Define.Color_Red;
                                            break;
                                        case "5":
                                            color = Define.Color_Green;
                                            break;
                                    }
                                }

                                itemList.Add(new Item()
                                {
                                    CalNo = equipment.MSACALNO,
                                    IchiUniqueID = equipment.ICHIUNIQUEID,
                                    IchiName = equipment.IchiName,
                                    IchiRemark = equipment.ICHIREMARK,
                                    StatusDescription = status != null ? status.Display : "",
                                    Color = color,
                                    EstDate = apply.ESTMSADATE
                                });
                            }
                        }
                    }
                    #endregion

                    #region MSA Notify
                    var msaNotifyList = db.QA_MSANOTIFY.Where(x => x.STATUS != "4" && x.ESTMSADATE.HasValue && DateTime.Compare(x.ESTMSADATE.Value, Begin) >= 0 && DateTime.Compare(x.ESTMSADATE.Value, End) <= 0).ToList();

                    foreach (var notify in msaNotifyList)
                    {
                        var equipment = (from e in db.QA_EQUIPMENT
                                         join i in db.QA_ICHI
                                         on e.ICHIUNIQUEID equals i.UNIQUEID into tmpIchi
                                         from i in tmpIchi.DefaultIfEmpty()
                                         where e.UNIQUEID == notify.EQUIPMENTUNIQUEID
                                         select new
                                         {
                                             e.MSACALNO,
                                             e.ICHIUNIQUEID,
                                             IchiName = i != null ? i.NAME : "",
                                             e.ICHIREMARK
                                         }).FirstOrDefault();

                        if (equipment != null)
                        {
                            var form = db.QA_MSAFORM.FirstOrDefault(x => x.EQUIPMENTUNIQUEID == notify.EQUIPMENTUNIQUEID && x.ESTMSADATE == notify.ESTMSADATE);

                            if (form != null)
                            {
                                var status = MSAForm_v2DataAccessor.GetFormStatus(form.UNIQUEID);

                                var color = Define.Color_Gray;

                                if (status != null)
                                {
                                    switch (status.StatusCode)
                                    {
                                        case "3":
                                            color = Define.Color_Purple;
                                            break;
                                        case "1":
                                            color = Define.Color_Blue;
                                            break;
                                        case "2":
                                        case "4":
                                        case "6":
                                            color = Define.Color_Red;
                                            break;
                                        case "5":
                                            color = Define.Color_Green;
                                            break;
                                    }
                                }

                                itemList.Add(new Item()
                                {
                                    CalNo = equipment.MSACALNO,
                                    IchiUniqueID = equipment.ICHIUNIQUEID,
                                    IchiName = equipment.IchiName,
                                    IchiRemark = equipment.ICHIREMARK,
                                    StatusDescription = status != null ? status.Display : "",
                                    Color = color,
                                    EstDate = notify.ESTMSADATE
                                });
                            }
                            else
                            {
                                itemList.Add(new Item()
                                {
                                    CalNo = equipment.MSACALNO,
                                    IchiUniqueID = equipment.ICHIUNIQUEID,
                                    IchiName = equipment.IchiName,
                                    IchiRemark = equipment.ICHIREMARK,
                                    StatusDescription = "文審中",
                                    Color = Define.Color_Gray,
                                    EstDate = notify.ESTMSADATE
                                });
                            }
                        }
                    }
                    #endregion
                }

                var eventList = from x in itemList
                                select new
                                {
                                    id = Guid.NewGuid().ToString(),
                                    title = x.Display,
                                    start = x.EstDateString,
                                    end = x.EstDateString,
                                    allDay = true,
                                    color = x.Color
                                };

                result.ReturnData(eventList.ToArray());
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
