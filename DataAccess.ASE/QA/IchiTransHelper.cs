using DbEntity.ASE;
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
    public class CalibrationFormModel
    {
        //單號
        public string VHNO { get; set; }

        //部門
        public string Department { get; set; }

        //儀校編號
        public string CalNo { get; set; }

        //儀器名稱
        public string IchiName { get; set; }

        public string CalibratorID { get; set; }

        public string CalibratorName { get; set; }

        //校驗人員
        public string Calibrator
        {
            get
            {
                if (!string.IsNullOrEmpty(CalibratorName))
                {
                    return string.Format("{0}/{1}", CalibratorID, CalibratorName);
                }
                else
                {
                    return CalibratorID;
                }
            }
        }

        public string OwnerID { get; set; }

        public string OwnerName { get; set; }

        //設備負責人
        public string Owner
        {
            get
            {
                if (!string.IsNullOrEmpty(OwnerName))
                {
                    return string.Format("{0}/{1}", OwnerID, OwnerName);
                }
                else
                {
                    return OwnerID;
                }
            }
        }

        public DateTime? CalibrateDate { get; set; }

        //校驗日期
        public string CalibrateDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(CalibrateDate);
            }
        }

        //步驟
        public List<int> Steps { get; set; }

        public CalibrationFormModel()
        {
            Steps = new List<int>();
        }
    }

    public class IchiTransHelper
    {
        public static RequestResult GetStep(string CalNo, string QAID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    bool qaValid = true;

                    var account = db.ACCOUNT.FirstOrDefault(x => x.ID == QAID);

                    if (account != null)
                    {
                        if (db.USERAUTHGROUP.Any(x => x.USERID == account.ID && (x.AUTHGROUPID == "QA" || x.AUTHGROUPID == "QA-Verify")))
                        {
                            qaValid = true;
                        }
                        else
                        {
                            qaValid = false;
                        }
                    }
                    else
                    {
                        qaValid = false;
                    }

                    var query = db.QA_EQUIPMENT.FirstOrDefault(x => x.CALNO == CalNo);

                    if (query != null)
                    {
                        var equipment = EquipmentHelper.Get(null, query.UNIQUEID);

                        var form = (from f in db.QA_CALIBRATIONFORM
                                    join apply in db.QA_CALIBRATIONAPPLY
                                    on f.APPLYUNIQUEID equals apply.UNIQUEID into tmpApply
                                    from apply in tmpApply.DefaultIfEmpty()
                                    join notify in db.QA_CALIBRATIONNOTIFY
                                    on f.NOTIFYUNIQUEID equals notify.UNIQUEID into tmpNotify
                                    from notify in tmpNotify.DefaultIfEmpty()
                                    join calibrator in db.ACCOUNT
                                    on f.CALRESPONSORID equals calibrator.ID into tmpCalibrator
                                    from calibrator in tmpCalibrator.DefaultIfEmpty()
                                    //where (apply.EQUIPMENTUNIQUEID == query.UNIQUEID || notify.EQUIPMENTUNIQUEID == query.UNIQUEID) && f.STATUS == "1"
                                    where (apply.EQUIPMENTUNIQUEID == query.UNIQUEID || notify.EQUIPMENTUNIQUEID == query.UNIQUEID) && f.STATUS != "9"
                                    orderby f.ESTCALDATE descending
                                    select new
                                    {
                                        UniqueID = f.UNIQUEID,
                                        f.VHNO,
                                        CalType = !string.IsNullOrEmpty(f.NOTIFYUNIQUEID) ? notify != null ? notify.CALTYPE : "" : !string.IsNullOrEmpty(f.APPLYUNIQUEID) ? apply != null ? apply.CALTYPE : "" : "",
                                        CalibratorID = f.CALRESPONSORID,
                                        CalibratorName = calibrator != null ? calibrator.NAME : "",
                                        CalibrateDate = f.CALDATE
                                    }).FirstOrDefault();

                        var model = new CalibrationFormModel()
                        {
                            VHNO = form != null ? form.VHNO : string.Empty,
                            Department = equipment.OrganizationDescription,
                            CalNo = equipment.CalNo,
                            IchiName = equipment.IchiDisplay,
                            CalibrateDate = form != null ? form.CalibrateDate : default(DateTime?),
                            CalibratorID = form != null ? form.CalibratorID : string.Empty,
                            CalibratorName = form != null ? form.CalibratorName : string.Empty,
                            OwnerID = equipment.OwnerID,
                            OwnerName = equipment.OwnerName
                        };

                        if (form != null && qaValid)
                        {
                            var last = db.QA_CALIBRATIONFORMSTEPLOG.Where(x => x.FORMUNIQUEID == form.UniqueID).OrderByDescending(x => x.SEQ).FirstOrDefault();

                            if (last != null)
                            {
                                if (last.STEP == "1")
                                {
                                    if (form.CalType == "IL")
                                    {
                                        model.Steps = new List<int>() { 4 };
                                    }

                                    if (form.CalType == "EL")
                                    {
                                        model.Steps = new List<int>() { 2, 4 };
                                    }
                                }
                                else if (last.STEP == "2")
                                {
                                    model.Steps = new List<int>() { 3 };
                                }
                                else if (last.STEP == "3")
                                {
                                    model.Steps = new List<int>() { 4 };
                                }
                                else if (last.STEP == "4")
                                {
                                    model.Steps = new List<int>() { 1 };
                                }
                            }
                            else
                            {
                                if (form.CalType == "IL")
                                {
                                    model.Steps = new List<int>() { 1 };
                                }

                                if (form.CalType == "EL")
                                {
                                    model.Steps = new List<int>() { 1 };
                                }
                            }
                        }

                        result.ReturnData(model);
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

        public static RequestResult Trans(string CalNo, int Step, string OwnerID, string QAID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var equipment = db.QA_EQUIPMENT.FirstOrDefault(x => x.CALNO == CalNo);

                    var form = (from f in db.QA_CALIBRATIONFORM
                                join apply in db.QA_CALIBRATIONAPPLY
                                on f.APPLYUNIQUEID equals apply.UNIQUEID into tmpApply
                                from apply in tmpApply.DefaultIfEmpty()

                                join notify in db.QA_CALIBRATIONNOTIFY
                                on f.NOTIFYUNIQUEID equals notify.UNIQUEID into tmpNotify
                                from notify in tmpNotify.DefaultIfEmpty()
                                where apply.EQUIPMENTUNIQUEID == equipment.UNIQUEID || notify.EQUIPMENTUNIQUEID == equipment.UNIQUEID
                                orderby f.ESTCALDATE descending
                                select new { UniqueID = f.UNIQUEID }).FirstOrDefault();

                    if (form != null)
                    {
                        int seq = db.QA_CALIBRATIONFORMSTEPLOG.Count(x => x.FORMUNIQUEID == form.UniqueID) + 1;

                        if (Step == 1)
                        {
                            var account = db.ACCOUNT.FirstOrDefault(x => x.ID == OwnerID);

                            if (account != null)
                            {
                                db.QA_CALIBRATIONFORMSTEPLOG.Add(new QA_CALIBRATIONFORMSTEPLOG()
                                {
                                    FORMUNIQUEID = form.UniqueID,
                                    STEP = "1",
                                    SEQ = seq,
                                    TIME = DateTime.Now,
                                    OWNERID = OwnerID,
                                    QAID = QAID
                                });

                                db.SaveChanges();

                                result.ReturnSuccessMessage("收件成功");
                            }
                            else
                            {
                                result.ReturnFailedMessage("交件人員工號不存在");
                            }
                        }
                        else if (Step == 2)
                        {
                            db.QA_CALIBRATIONFORMSTEPLOG.Add(new QA_CALIBRATIONFORMSTEPLOG()
                            {
                                FORMUNIQUEID = form.UniqueID,
                                STEP = "2",
                                SEQ = seq,
                                TIME = DateTime.Now,
                                OWNERID = "",
                                QAID = QAID
                            });

                            db.SaveChanges();

                            result.ReturnSuccessMessage("送件成功");
                        }
                        else if (Step == 3)
                        {
                            db.QA_CALIBRATIONFORMSTEPLOG.Add(new QA_CALIBRATIONFORMSTEPLOG()
                            {
                                FORMUNIQUEID = form.UniqueID,
                                STEP = "3",
                                SEQ = seq,
                                TIME = DateTime.Now,
                                OWNERID = "",
                                QAID = QAID
                            });

                            db.SaveChanges();

                            result.ReturnSuccessMessage("回件成功");
                        }
                        else if (Step == 4)
                        {
                            var account = db.ACCOUNT.FirstOrDefault(x => x.ID == OwnerID);

                            if (account != null)
                            {
                                db.QA_CALIBRATIONFORMSTEPLOG.Add(new QA_CALIBRATIONFORMSTEPLOG()
                                {
                                    FORMUNIQUEID = form.UniqueID,
                                    STEP = "4",
                                    SEQ = seq,
                                    TIME = DateTime.Now,
                                    OWNERID = OwnerID,
                                    QAID = QAID
                                });

                                db.SaveChanges();

                                result.ReturnSuccessMessage("發件成功");
                            }
                            else
                            {
                                result.ReturnFailedMessage("取件人員工號不存在");
                            }
                        }
                        else
                        {
                            result.ReturnFailedMessage("未知的操作");
                        }
                    }
                    else
                    {
                        result.ReturnFailedMessage("校驗編號不存在");
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
    }
}
