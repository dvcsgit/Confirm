using DbEntity.ASE;
using Models.ASE.QA.AbnormalForm;
using Models.Authenticated;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace DataAccess.ASE.QA
{
    public class AbnormalFormDataAccessor
    {
        public static RequestResult GetUserOptions(List<Models.Shared.UserModel> UserList, string Term, bool IsInit)
        {
            RequestResult result = new RequestResult();

            try
            {
                var query = UserList.Select(x => new Models.ASE.Shared.ASEUserModel
                {
                    ID = x.ID,
                    Name = x.Name,
                    Email = x.Email,
                    OrganizationDescription = x.OrganizationDescription
                }).ToList();

                if (!string.IsNullOrEmpty(Term))
                {
                    if (IsInit)
                    {
                        query = query.Where(x => x.ID == Term).ToList();
                    }
                    else
                    {
                        var term = Term.ToLower();

                        query = query.Where(x => x.Term.Contains(term)).ToList();
                    }
                }

                result.ReturnData(query.Select(x => new SelectListItem { Value = x.ID, Text = x.Display }).Distinct().ToList());
            }
            catch (Exception ex)
            {
                Error err = new Error(MethodInfo.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetQueryFormModel(List<Models.Shared.Organization> OrganizationList, string VHNO)
        {
            RequestResult result = new RequestResult();

            try
            {
                var factoryList = OrganizationDataAccessor.GetFactoryList(OrganizationList);

                var model = new QueryFormModel()
                {
                    FactorySelectItemList = new List<SelectListItem>() {
                     Define.DefaultSelectListItem(Resources.Resource.SelectAll)
                    },
                    Parameters = new QueryParameters()
                    {
                        VHNO = VHNO
                    }
                };

                foreach (var factory in factoryList)
                {
                    model.FactorySelectItemList.Add(new SelectListItem()
                    {
                        Text = factory.Description,
                        Value = factory.UniqueID
                    });
                }

                result.ReturnData(model);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult Submit(string UniqueID, string FlowVHNO, string FlowClosedDate, string FlowFileExtension, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var abnormal = db.QA_ABNORMALFORM.First(x => x.UNIQUEID == UniqueID);

                    abnormal.STATUS = "5";

                    abnormal.FLOWVHNO = FlowVHNO;
                    abnormal.FLOWCLOSEDDATE = FlowClosedDate;
                    abnormal.FLOWFILEEXTENSION = FlowFileExtension;

                    var time = DateTime.Now;

                    var log = db.QA_ABNORMALFORMFLOWLOG.First(x => x.FORMUNIQUEID == abnormal.UNIQUEID && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue && x.FLOWSEQ == 2 && x.USERID == Account.ID);

                    log.USERID = Account.ID;
                    log.VERIFYTIME = time;
                    log.VERIFYRESULT = "Y";

                    var logSeq = 1;

                    var logList = db.QA_ABNORMALFORMFLOWLOG.Where(x => x.FORMUNIQUEID == abnormal.UNIQUEID).ToList();

                    if (logList.Count > 0)
                    {
                        logSeq = logList.Max(x => x.SEQ) + 1;
                    }

                    var peManagerID =(from x in db.QA_ABNORMALFORM
                                           join f in db.QA_CALIBRATIONFORM
                                           on x.CALFORMUNIQUEID equals f.UNIQUEID
                                           join a in db.QA_CALIBRATIONAPPLY
                                           on f.APPLYUNIQUEID equals a.UNIQUEID
                                           join e in db.QA_EQUIPMENT
                                           on a.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                           where x.UNIQUEID == abnormal.UNIQUEID
                                           select e.PEMANAGERID).FirstOrDefault();

                    if (string.IsNullOrEmpty(peManagerID))
                    {
                        peManagerID = (from x in db.QA_ABNORMALFORM
                                       join f in db.QA_CALIBRATIONFORM
                                       on x.CALFORMUNIQUEID equals f.UNIQUEID
                                       join n in db.QA_CALIBRATIONNOTIFY
                                       on f.NOTIFYUNIQUEID equals n.UNIQUEID
                                       join e in db.QA_EQUIPMENT
                                       on n.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                       where x.UNIQUEID == abnormal.UNIQUEID
                                       select e.PEMANAGERID).First();
                    }

                    db.QA_ABNORMALFORMFLOWLOG.Add(new QA_ABNORMALFORMFLOWLOG()
                    {
                        FORMUNIQUEID = abnormal.UNIQUEID,
                        SEQ = logSeq,
                        FLOWSEQ = 3,
                        ISCANCELED = "N",
                        NOTIFYTIME = time,
                        USERID = peManagerID
                    });

                    db.SaveChanges();

                    SendVerifyMail();

                    result.ReturnSuccessMessage("呈核成功");
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

        public static RequestResult ApproveAdjust(string UniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var abnormal = db.QA_ABNORMALFORM.First(x => x.UNIQUEID == UniqueID);

                    abnormal.STATUS = "4";

                    var form = db.QA_CALIBRATIONFORM.First(x => x.UNIQUEID == abnormal.CALFORMUNIQUEID);

                    var calDate = DateTime.Today.AddDays(3);

                    if (DateTime.Compare(calDate, form.ESTCALDATE) >= 0)
                    {
                        form.ESTCALDATE = calDate;
                    }

                    form.STATUS = "8";

                    var log = db.QA_ABNORMALFORMFLOWLOG.First(x => x.FORMUNIQUEID == abnormal.UNIQUEID && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue && x.FLOWSEQ == 4);

                    log.USERID = Account.ID;
                    log.VERIFYTIME = DateTime.Now;
                    log.VERIFYRESULT = "Y";

                    db.SaveChanges();

                    result.ReturnSuccessMessage("核簽成功");
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

        public static RequestResult RejectAdjust(string UniqueID, string VerifyComment, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var abnormal = db.QA_ABNORMALFORM.First(x => x.UNIQUEID == UniqueID);

                    abnormal.STATUS = "1";

                    var time = DateTime.Now;

                    var log = db.QA_ABNORMALFORMFLOWLOG.First(x => x.FORMUNIQUEID == abnormal.UNIQUEID && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue && x.FLOWSEQ == 4);

                    log.USERID = Account.ID;
                    log.VERIFYTIME = time;
                    log.VERIFYRESULT = "N";
                    log.VERIFYCOMMENT = VerifyComment;

                    var ownerID = (from x in db.QA_ABNORMALFORM
                                join f in db.QA_CALIBRATIONFORM
                                on x.CALFORMUNIQUEID equals f.UNIQUEID
                                join a in db.QA_CALIBRATIONAPPLY
                                on f.APPLYUNIQUEID equals a.UNIQUEID
                                join e in db.QA_EQUIPMENT
                                on a.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                where x.UNIQUEID == UniqueID
                                select e.OWNERID).FirstOrDefault();

                    if (string.IsNullOrEmpty(ownerID))
                    {
                        ownerID = (from x in db.QA_ABNORMALFORM
                                join f in db.QA_CALIBRATIONFORM
                                on x.CALFORMUNIQUEID equals f.UNIQUEID
                                join n in db.QA_CALIBRATIONNOTIFY
                                on f.NOTIFYUNIQUEID equals n.UNIQUEID
                                join e in db.QA_EQUIPMENT
                                on n.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                where x.UNIQUEID == UniqueID
                                select e.OWNERID).First();
                    }

                    var logSeq = 1;

                    var logList = db.QA_ABNORMALFORMFLOWLOG.Where(x => x.FORMUNIQUEID == UniqueID).ToList();

                    if (logList.Count > 0)
                    {
                        logSeq = logList.Max(x => x.SEQ) + 1;
                    }

                    db.QA_ABNORMALFORMFLOWLOG.Add(new QA_ABNORMALFORMFLOWLOG()
                    {
                        FORMUNIQUEID = UniqueID,
                        SEQ = logSeq,
                        FLOWSEQ = 1,
                        ISCANCELED = "N",
                        NOTIFYTIME = time,
                        USERID = ownerID
                    });

                    db.SaveChanges();

                    SendRejectMail();

                    result.ReturnSuccessMessage("退回成功");
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

        public static RequestResult Approve(string UniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var time = DateTime.Now;

                    var log = db.QA_ABNORMALFORMFLOWLOG.First(x => x.FORMUNIQUEID == UniqueID && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue && x.FLOWSEQ == 3);

                    log.USERID = Account.ID;
                    log.VERIFYTIME = time;
                    log.VERIFYRESULT = "Y";

                    var logSeq = 1;

                    var logList = db.QA_ABNORMALFORMFLOWLOG.Where(x => x.FORMUNIQUEID == UniqueID).ToList();

                    if (logList.Count > 0)
                    {
                        logSeq = logList.Max(x => x.SEQ) + 1;
                    }

                    db.QA_ABNORMALFORMFLOWLOG.Add(new QA_ABNORMALFORMFLOWLOG()
                    {
                        FORMUNIQUEID = UniqueID,
                        SEQ = logSeq,
                        FLOWSEQ = 4,
                        ISCANCELED = "N",
                        NOTIFYTIME = time
                    });

                    db.SaveChanges();

                    SendVerifyMail();

                    result.ReturnSuccessMessage("簽核成功");
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

        public static RequestResult Reject(string UniqueID, string VerifyComment, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var abnormal = db.QA_ABNORMALFORM.First(x => x.UNIQUEID == UniqueID);

                    abnormal.STATUS = "3";

                    var time = DateTime.Now;

                    var log = db.QA_ABNORMALFORMFLOWLOG.First(x => x.FORMUNIQUEID == abnormal.UNIQUEID && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue && x.FLOWSEQ == 3);

                    log.USERID = Account.ID;
                    log.VERIFYTIME = time;
                    log.VERIFYRESULT = "N";
                    log.VERIFYCOMMENT = VerifyComment;

                    var peID = (from x in db.QA_ABNORMALFORM
                                join f in db.QA_CALIBRATIONFORM
                                on x.CALFORMUNIQUEID equals f.UNIQUEID
                                join a in db.QA_CALIBRATIONAPPLY
                                on f.APPLYUNIQUEID equals a.UNIQUEID
                                join e in db.QA_EQUIPMENT
                                on a.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                where x.UNIQUEID == UniqueID
                                select e.PEID).FirstOrDefault();

                    if (string.IsNullOrEmpty(peID))
                    {
                        peID = (from x in db.QA_ABNORMALFORM
                                join f in db.QA_CALIBRATIONFORM
                                on x.CALFORMUNIQUEID equals f.UNIQUEID
                                join n in db.QA_CALIBRATIONNOTIFY
                                on f.NOTIFYUNIQUEID equals n.UNIQUEID
                                join e in db.QA_EQUIPMENT
                                on n.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                where x.UNIQUEID == UniqueID
                                select e.PEID).First();
                    }

                    var logSeq = 1;

                    var logList = db.QA_ABNORMALFORMFLOWLOG.Where(x => x.FORMUNIQUEID == UniqueID).ToList();

                    if (logList.Count > 0)
                    {
                        logSeq = logList.Max(x => x.SEQ) + 1;
                    }

                    db.QA_ABNORMALFORMFLOWLOG.Add(new QA_ABNORMALFORMFLOWLOG()
                    {
                        FORMUNIQUEID = UniqueID,
                        SEQ = logSeq,
                        FLOWSEQ = 2,
                        ISCANCELED = "N",
                        NOTIFYTIME = time,
                        USERID = peID
                    });

                    db.SaveChanges();

                    SendRejectMail();

                    result.ReturnSuccessMessage("退回成功");
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

        public static RequestResult QAApprove(string UniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var abnormal = db.QA_ABNORMALFORM.First(x => x.UNIQUEID == UniqueID);

                    abnormal.STATUS = "6";

                    var changeForm = db.QA_CHANGEFORM.First(x => x.ABNORMALFORMUNIQUEID == UniqueID);

                    if (changeForm.STATUS == "3")
                    {
                        abnormal.STATUS = "4";
                    }

                    var time = DateTime.Now;

                    var log = db.QA_ABNORMALFORMFLOWLOG.First(x => x.FORMUNIQUEID == UniqueID && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue && x.FLOWSEQ == 4);

                    log.USERID = Account.ID;
                    log.VERIFYTIME = time;
                    log.VERIFYRESULT = "Y";

                    db.SaveChanges();

                    result.ReturnSuccessMessage("簽核成功");
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

        public static RequestResult QAReject(string UniqueID, string VerifyComment, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var abnormal = db.QA_ABNORMALFORM.First(x => x.UNIQUEID == UniqueID);

                    abnormal.STATUS = "3";

                    var time = DateTime.Now;

                    var log = db.QA_ABNORMALFORMFLOWLOG.First(x => x.FORMUNIQUEID == abnormal.UNIQUEID && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue && x.FLOWSEQ == 4);

                    log.USERID = Account.ID;
                    log.VERIFYTIME = time;
                    log.VERIFYRESULT = "N";
                    log.VERIFYCOMMENT = VerifyComment;

                    var logSeq = 1;

                    var logList = db.QA_ABNORMALFORMFLOWLOG.Where(x => x.FORMUNIQUEID == UniqueID).ToList();

                    if (logList.Count > 0)
                    {
                        logSeq = logList.Max(x => x.SEQ) + 1;
                    }

                    var peID = (from x in db.QA_ABNORMALFORM
                                join f in db.QA_CALIBRATIONFORM
                                on x.CALFORMUNIQUEID equals f.UNIQUEID
                                join a in db.QA_CALIBRATIONAPPLY
                                on f.APPLYUNIQUEID equals a.UNIQUEID
                                join e in db.QA_EQUIPMENT
                                on a.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                where x.UNIQUEID == UniqueID
                                select e.PEID).FirstOrDefault();

                    if (string.IsNullOrEmpty(peID))
                    {
                        peID = (from x in db.QA_ABNORMALFORM
                                join f in db.QA_CALIBRATIONFORM
                                on x.CALFORMUNIQUEID equals f.UNIQUEID
                                join n in db.QA_CALIBRATIONNOTIFY
                                on f.NOTIFYUNIQUEID equals n.UNIQUEID
                                join e in db.QA_EQUIPMENT
                                on n.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                where x.UNIQUEID == UniqueID
                                select e.PEID).First();
                    }

                    db.QA_ABNORMALFORMFLOWLOG.Add(new QA_ABNORMALFORMFLOWLOG()
                    {
                        FORMUNIQUEID = UniqueID,
                        SEQ = logSeq,
                        FLOWSEQ = 2,
                        ISCANCELED = "N",
                        NOTIFYTIME = time,
                        USERID = peID
                    });

                    db.SaveChanges();

                    SendRejectMail();

                    result.ReturnSuccessMessage("退回成功");
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

        public static RequestResult Adjust(string UniqueID, string Remark, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = db.QA_ABNORMALFORM.First(x => x.UNIQUEID == UniqueID);

                    form.STATUS = "2";
                    form.HANDLINGREMARK = Remark;

                    var logSeq = 1;

                    var logList = db.QA_ABNORMALFORMFLOWLOG.Where(x => x.FORMUNIQUEID == UniqueID).ToList();

                    if (logList.Count > 0)
                    {
                        logSeq = logList.Max(x => x.SEQ) + 1;
                    }

                    var time = DateTime.Now;

                    db.QA_ABNORMALFORMFLOWLOG.Add(new QA_ABNORMALFORMFLOWLOG()
                    {
                        FORMUNIQUEID = form.UNIQUEID,
                        FLOWSEQ = 1,
                        ISCANCELED = "N",
                        NOTIFYTIME = time,
                        SEQ = logSeq,
                        USERID = Account.ID,
                        VERIFYRESULT = "Y",
                        VERIFYTIME = time
                    });

                    logSeq++;

                    db.QA_ABNORMALFORMFLOWLOG.Add(new QA_ABNORMALFORMFLOWLOG()
                    {
                        FORMUNIQUEID = form.UNIQUEID,
                        FLOWSEQ = 4,
                        ISCANCELED = "N",
                        NOTIFYTIME = time,
                        SEQ = logSeq
                    });

                    db.SaveChanges();

                    SendVerifyMail();

                    result.ReturnSuccessMessage("呈核成功");
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

        public static List<GridItem> Query(List<Models.Shared.Organization> OrganizationList, string EquipmentUniqueID)
        {
            var model = new List<GridItem>();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query1 = (from x in db.QA_ABNORMALFORM
                                  join f in db.QA_CALIBRATIONFORM
                                  on x.CALFORMUNIQUEID equals f.UNIQUEID
                                  join a in db.QA_CALIBRATIONAPPLY
                                  on f.APPLYUNIQUEID equals a.UNIQUEID
                                  join e in db.QA_EQUIPMENT
                                  on a.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                  join i in db.QA_ICHI
                                  on e.ICHIUNIQUEID equals i.UNIQUEID into tmpIchi
                                  from i in tmpIchi.DefaultIfEmpty()
                                  join calibrator in db.ACCOUNT
                                  on f.CALRESPONSORID equals calibrator.ID into tmpCalibrator
                                  from calibrator in tmpCalibrator.DefaultIfEmpty()
                                  join owner in db.ACCOUNT
                                  on e.OWNERID equals owner.ID into tmpOwner
                                  from owner in tmpOwner
                                  where a.EQUIPMENTUNIQUEID==EquipmentUniqueID
                                  select new
                                  {
                                      UniqueID = x.UNIQUEID,
                                      VHNO = x.VHNO,
                                      Status = x.STATUS,
                                      OrganizationUniqueID = e.ORGANIZATIONUNIQUEID,
                                      SerialNo = e.SERIALNO,
                                      MachineNo = e.MACHINENO,
                                      IchiUniqueID = e.ICHIUNIQUEID,
                                      IchiName = i != null ? i.NAME : "",
                                      IchiRemark = e.ICHIREMARK,
                                      Brand = e.BRAND,
                                      Model = e.MODEL,
                                      CalibrateDate = f.ESTCALDATE,
                                      CalibratorID = f.CALRESPONSORID,
                                      CalibratorName = calibrator != null ? calibrator.NAME : "",
                                      OwnerID = e.OWNERID,
                                      OwnerName = owner != null ? owner.NAME : "",
                                      OwnerManagerID = e.OWNERMANAGERID,
                                      PEID = e.PEID,
                                      PEManagerID = e.PEMANAGERID,
                                      CalNo = e.CALNO,
                                      CreateTime = x.CREATETIME
                                  }).AsQueryable();

                    var query2 = (from x in db.QA_ABNORMALFORM
                                  join f in db.QA_CALIBRATIONFORM
                                  on x.CALFORMUNIQUEID equals f.UNIQUEID
                                  join n in db.QA_CALIBRATIONNOTIFY
                                  on f.NOTIFYUNIQUEID equals n.UNIQUEID
                                  join e in db.QA_EQUIPMENT
                                  on n.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                  join i in db.QA_ICHI
                                  on e.ICHIUNIQUEID equals i.UNIQUEID into tmpIchi
                                  from i in tmpIchi.DefaultIfEmpty()
                                  join calibrator in db.ACCOUNT
                                  on f.CALRESPONSORID equals calibrator.ID into tmpCalibrator
                                  from calibrator in tmpCalibrator.DefaultIfEmpty()
                                  join owner in db.ACCOUNT
                                  on e.OWNERID equals owner.ID into tmpOwner
                                  from owner in tmpOwner
                                  where n.EQUIPMENTUNIQUEID==EquipmentUniqueID
                                  select new
                                  {
                                      UniqueID = x.UNIQUEID,
                                      VHNO = x.VHNO,
                                      Status = x.STATUS,
                                      OrganizationUniqueID = e.ORGANIZATIONUNIQUEID,
                                      SerialNo = e.SERIALNO,
                                      MachineNo = e.MACHINENO,
                                      IchiUniqueID = e.ICHIUNIQUEID,
                                      IchiName = i != null ? i.NAME : "",
                                      IchiRemark = e.ICHIREMARK,
                                      Brand = e.BRAND,
                                      Model = e.MODEL,
                                      CalibrateDate = f.ESTCALDATE,
                                      CalibratorID = f.CALRESPONSORID,
                                      CalibratorName = calibrator != null ? calibrator.NAME : "",
                                      OwnerID = e.OWNERID,
                                      OwnerName = owner != null ? owner.NAME : "",
                                      OwnerManagerID = e.OWNERMANAGERID,
                                      PEID = e.PEID,
                                      PEManagerID = e.PEMANAGERID,
                                      CalNo = e.CALNO,
                                      CreateTime = x.CREATETIME
                                  }).AsQueryable();

                    model = query1.ToList().Union(query2.ToList()).Select(x => new GridItem
                    {
                        UniqueID = x.UniqueID,
                        VHNO = x.VHNO,
                        Status = x.Status,
                        Factory = OrganizationDataAccessor.GetFactory(OrganizationList, x.OrganizationUniqueID),
                        Brand = x.Brand,
                        Model = x.Model,
                        SerialNo = x.SerialNo,
                        CalibrateDate = x.CalibrateDate,
                        CalibratorID = x.CalibratorID,
                        CalibratorName = x.CalibratorName,
                        CalNo = x.CalNo,
                        CreateTime = x.CreateTime.Value,
                        IchiName = x.IchiName,
                        IchiRemark = x.IchiRemark,
                        IchiUniqueID = x.IchiUniqueID,
                        OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.OrganizationUniqueID),
                        OwnerID = x.OwnerID,
                        OwnerName = x.OwnerName,
                        OwnerManagerID = x.OwnerManagerID,
                        PEID = x.PEID,
                        PEManagerID = x.PEManagerID
                    }).OrderBy(x => x.CreateTimeTimeString).ToList();
                }
            }
            catch (Exception ex)
            {
                model = null;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return model;
        }

        public static RequestResult Query(List<Models.Shared.Organization> OrganizationList, QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new GridViewModel();

                var qa = Account.UserAuthGroupList.Contains("QA-Verify") || Account.UserAuthGroupList.Contains("QA") || Account.UserAuthGroupList.Contains("QA-FullQuery");

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query1 = (from x in db.QA_ABNORMALFORM
                                 join f in db.QA_CALIBRATIONFORM
                                 on x.CALFORMUNIQUEID equals f.UNIQUEID
                                 join a in db.QA_CALIBRATIONAPPLY
                                 on f.APPLYUNIQUEID equals a.UNIQUEID
                                 join e in db.QA_EQUIPMENT
                                 on a.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                 join i in db.QA_ICHI
                                 on e.ICHIUNIQUEID equals i.UNIQUEID into tmpIchi
                                 from i in tmpIchi.DefaultIfEmpty()
                                 join calibrator in db.ACCOUNT
                                 on f.CALRESPONSORID equals calibrator.ID into tmpCalibrator
                                 from calibrator in tmpCalibrator.DefaultIfEmpty()
                                 join owner in db.ACCOUNT
                                 on e.OWNERID equals owner.ID into tmpOwner
                                 from owner in tmpOwner
                                 select new
                                 {
                                     UniqueID = x.UNIQUEID,
                                     VHNO = x.VHNO,
                                     Status = x.STATUS,
                                     OrganizationUniqueID = e.ORGANIZATIONUNIQUEID,
                                     SerialNo = e.SERIALNO,
                                     MachineNo = e.MACHINENO,
                                     IchiUniqueID = e.ICHIUNIQUEID,
                                     IchiName = i != null ? i.NAME : "",
                                     IchiRemark = e.ICHIREMARK,
                                     Brand = e.BRAND,
                                     Model = e.MODEL,
                                     CalibrateDate = f.ESTCALDATE,
                                     CalibratorID = f.CALRESPONSORID,
                                     CalibratorName = calibrator != null ? calibrator.NAME : "",
                                     OwnerID = e.OWNERID,
                                     OwnerName = owner!=null?owner.NAME:"",
                                     OwnerManagerID = e.OWNERMANAGERID,
                                     PEID = e.PEID,
                                     PEManagerID = e.PEMANAGERID,
                                     CalNo = e.CALNO,
                                    CreateTime = x.CREATETIME
                                 }).AsQueryable();

                    var query2 = (from x in db.QA_ABNORMALFORM
                                 join f in db.QA_CALIBRATIONFORM
                                 on x.CALFORMUNIQUEID equals f.UNIQUEID
                                 join n in db.QA_CALIBRATIONNOTIFY
                                 on f.NOTIFYUNIQUEID equals n.UNIQUEID
                                 join e in db.QA_EQUIPMENT
                                 on n.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                 join i in db.QA_ICHI
                                 on e.ICHIUNIQUEID equals i.UNIQUEID into tmpIchi
                                 from i in tmpIchi.DefaultIfEmpty()
                                 join calibrator in db.ACCOUNT
                                 on f.CALRESPONSORID equals calibrator.ID into tmpCalibrator
                                 from calibrator in tmpCalibrator.DefaultIfEmpty()
                                 join owner in db.ACCOUNT
                                 on e.OWNERID equals owner.ID into tmpOwner
                                 from owner in tmpOwner
                                 select new
                                 {
                                     UniqueID = x.UNIQUEID,
                                     VHNO = x.VHNO,
                                     Status = x.STATUS,
                                     OrganizationUniqueID = e.ORGANIZATIONUNIQUEID,
                                     SerialNo = e.SERIALNO,
                                     MachineNo = e.MACHINENO,
                                     IchiUniqueID = e.ICHIUNIQUEID,
                                     IchiName = i != null ? i.NAME : "",
                                     IchiRemark = e.ICHIREMARK,
                                     Brand = e.BRAND,
                                     Model = e.MODEL,
                                     CalibrateDate = f.ESTCALDATE,
                                     CalibratorID = f.CALRESPONSORID,
                                     CalibratorName = calibrator != null ? calibrator.NAME : "",
                                     OwnerID = e.OWNERID,
                                     OwnerName = owner != null ? owner.NAME : "",
                                     OwnerManagerID = e.OWNERMANAGERID,
                                     PEID = e.PEID,
                                     PEManagerID = e.PEMANAGERID,
                                     CalNo = e.CALNO,
                                     CreateTime = x.CREATETIME
                                 }).AsQueryable();

                    if (!qa)
                    {
                        query1 = query1.Where(x => Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID) || x.CalibratorID == Account.ID || x.OwnerID == Account.ID || x.OwnerManagerID == Account.ID || x.PEID == Account.ID || x.PEManagerID == Account.ID).AsQueryable();
                        query2 = query2.Where(x => Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID) || x.CalibratorID == Account.ID || x.OwnerID == Account.ID || x.OwnerManagerID == Account.ID || x.PEID == Account.ID || x.PEManagerID == Account.ID).AsQueryable();
                    }

                    if (Parameters.BeginDate.HasValue)
                    {
                        query1 = query1.Where(x => DateTime.Compare(x.CalibrateDate, Parameters.BeginDate.Value) >= 0);
                        query2 = query2.Where(x => DateTime.Compare(x.CalibrateDate, Parameters.BeginDate.Value) >= 0);
                    }

                    if (Parameters.EndDate.HasValue)
                    {
                        query1 = query1.Where(x => DateTime.Compare(x.CalibrateDate, Parameters.EndDate.Value) < 0);
                        query2 = query2.Where(x => DateTime.Compare(x.CalibrateDate, Parameters.EndDate.Value) < 0);
                    }

                    if (!string.IsNullOrEmpty(Parameters.VHNO))
                    {
                        query1 = query1.Where(x => x.VHNO.Contains(Parameters.VHNO));
                        query2 = query2.Where(x => x.VHNO.Contains(Parameters.VHNO));
                    }

                    if (!string.IsNullOrEmpty(Parameters.Status))
                    {
                        var statusList = Parameters.StatusList;

                        if (statusList.Contains("3"))
                        {
                            statusList.Add("5");
                            statusList.Add("6");
                        }

                        query1 = query1.Where(x => statusList.Contains(x.Status));
                        query2 = query2.Where(x => statusList.Contains(x.Status));
                    }

                    if (!string.IsNullOrEmpty(Parameters.OwnerID))
                    {
                        query1 = query1.Where(x => x.OwnerID == Parameters.OwnerID);
                        query2 = query2.Where(x => x.OwnerID == Parameters.OwnerID);
                    }

                    if (!string.IsNullOrEmpty(Parameters.OwnerManagerID))
                    {
                        query1 = query1.Where(x => x.OwnerManagerID == Parameters.OwnerManagerID);
                        query2 = query2.Where(x => x.OwnerManagerID == Parameters.OwnerManagerID);
                    }

                    if (!string.IsNullOrEmpty(Parameters.PEID))
                    {
                        query1 = query1.Where(x => x.PEID == Parameters.PEID);
                        query2 = query2.Where(x => x.PEID == Parameters.PEID);
                    }

                    if (!string.IsNullOrEmpty(Parameters.PEManagerID))
                    {
                        query1 = query1.Where(x => x.PEManagerID == Parameters.PEManagerID);
                        query2 = query2.Where(x => x.PEManagerID == Parameters.PEManagerID);
                    }

                    if (!string.IsNullOrEmpty(Parameters.FactoryUniqueID))
                    {
                        var downStream = OrganizationDataAccessor.GetDownStreamOrganizationList(OrganizationList, Parameters.FactoryUniqueID, true);

                        query1 = query1.Where(x => downStream.Contains(x.OrganizationUniqueID));
                        query2 = query2.Where(x => downStream.Contains(x.OrganizationUniqueID));
                    }

                    if (!string.IsNullOrEmpty(Parameters.IchiName))
                    {
                        query1 = query1.Where(x => x.IchiName.Contains(Parameters.IchiName) || x.IchiRemark.Contains(Parameters.IchiName));
                        query2 = query2.Where(x => x.IchiName.Contains(Parameters.IchiName) || x.IchiRemark.Contains(Parameters.IchiName));
                    }

                    if (!string.IsNullOrEmpty(Parameters.Brand))
                    {
                        query1 = query1.Where(x => x.Brand.Contains(Parameters.Brand));
                        query2 = query2.Where(x => x.Brand.Contains(Parameters.Brand));
                    }

                    if (!string.IsNullOrEmpty(Parameters.Model))
                    {
                        query1 = query1.Where(x => x.Model.Contains(Parameters.Model));
                        query2 = query2.Where(x => x.Model.Contains(Parameters.Model));
                    }

                    model.ItemList = query1.ToList().Union(query2.ToList()).Select(x => new GridItem
                    {
                        UniqueID = x.UniqueID,
                        VHNO = x.VHNO,
                        Status = x.Status,
                        Factory = OrganizationDataAccessor.GetFactory(OrganizationList, x.OrganizationUniqueID),
                        Brand = x.Brand,
                        Model = x.Model,
                        SerialNo = x.SerialNo,
                        CalibrateDate = x.CalibrateDate,
                        CalibratorID = x.CalibratorID,
                        CalibratorName = x.CalibratorName,
                        CalNo = x.CalNo,
                        CreateTime = x.CreateTime.Value,
                        IchiName = x.IchiName,
                        IchiRemark = x.IchiRemark,
                        IchiUniqueID = x.IchiUniqueID,
                        OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.OrganizationUniqueID),
                        OwnerID = x.OwnerID,
                        OwnerName = x.OwnerName,
                        OwnerManagerID = x.OwnerManagerID,
                        PEID = x.PEID,
                        PEManagerID = x.PEManagerID
                    }).ToList();
                }

                result.ReturnData(model);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetCreateFormModel(List<Models.Shared.Organization> OrganizationList, string CalibrationFormUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new CreateFormModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = (from f in db.QA_CALIBRATIONFORM
                                 join a in db.QA_CALIBRATIONAPPLY
                                 on f.APPLYUNIQUEID equals a.UNIQUEID
                                 join calibrator in db.ACCOUNT
                                 on f.CALRESPONSORID equals calibrator.ID into tmpCalibrator
                                 from calibrator in tmpCalibrator.DefaultIfEmpty()
                                 where f.UNIQUEID == CalibrationFormUniqueID
                                 select new
                                 {
                                     EquipmentUniqueID = a.EQUIPMENTUNIQUEID,
                                     CalibrateDate = f.CALDATE,
                                     EstCalibrateDate = f.ESTCALDATE,
                                     CalibrationFormUniqueID = f.UNIQUEID,
                                     CalibratorID = f.CALRESPONSORID,
                                     CalibratorName = calibrator != null ? calibrator.NAME : ""
                                 }).FirstOrDefault();

                    if (query == null)
                    {
                        query = (from f in db.QA_CALIBRATIONFORM
                                 join n in db.QA_CALIBRATIONNOTIFY
                                 on f.NOTIFYUNIQUEID equals n.UNIQUEID
                                 join calibrator in db.ACCOUNT
                                 on f.CALRESPONSORID equals calibrator.ID into tmpCalibrator
                                 from calibrator in tmpCalibrator.DefaultIfEmpty()
                                 where f.UNIQUEID == CalibrationFormUniqueID
                                 select new
                                 {
                                     EquipmentUniqueID = n.EQUIPMENTUNIQUEID,
                                     CalibrateDate = f.CALDATE,
                                     EstCalibrateDate = f.ESTCALDATE,
                                     CalibrationFormUniqueID = f.UNIQUEID,
                                     CalibratorID = f.CALRESPONSORID,
                                     CalibratorName = calibrator != null ? calibrator.NAME : ""
                                 }).First();
                    }

                    model = new CreateFormModel()
                    {
                        Equipment = EquipmentHelper.Get(OrganizationList, query.EquipmentUniqueID),
                        CalibrationFormUniqueID = query.CalibrationFormUniqueID,
                        CalibrateDate = query.CalibrateDate.HasValue?query.CalibrateDate.Value:query.EstCalibrateDate,
                        CalibratorID = query.CalibratorID,
                        CalibratorName = query.CalibratorName,
                        ItemList = db.QA_CALIBRATIONFORMDETAIL.Where(x => x.FORMUNIQUEID == query.CalibrationFormUniqueID).OrderBy(x => x.SEQ).ToList().Select(x => new Models.ASE.QA.CalibrationForm.DetailItem
                        {
                            Seq = x.SEQ,
                            Characteristic = x.CHARACTERISTIC,
                            CalibrationPoint = x.CALIBRATIONPOINT,
                            UsingRange = x.USINGRANGE,
                            Standard = x.STANDARD,
                            CalibrateDate = x.CALDATE,
                            Tolerance = x.TOLERANCE,
                            ToleranceMark = x.TOLERANCESYMBOL,
                            ToleranceUnitRate = x.TOLERANCEUNITRATE,
                            ToleranceUnit = x.TOLERANCEUNIT,
                            Unit = x.UNIT,
                            ReadingValue = x.READINGVALUE
                        }).ToList().Where(x => x.IsFailed.HasValue && x.IsFailed.Value).ToList(),
                        STDUSEList = (from x in db.QA_CALIBRATIONFORMSTDUSE
                                      join e in db.QA_EQUIPMENT
                                      on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                      join calibrator in db.ACCOUNT
                                      on e.LASTCALUSERID equals calibrator.ID into tmpCalibrator
                                      from calibrator in tmpCalibrator.DefaultIfEmpty()
                                      join i in db.QA_ICHI
                                      on e.ICHIUNIQUEID equals i.UNIQUEID into tmpIchi
                                      from i in tmpIchi.DefaultIfEmpty()
                                      where x.FORMUNIQUEID == query.CalibrationFormUniqueID
                                      select new Models.ASE.QA.CalibrationForm.STDUSEModel
                                      {
                                          UniqueID = e.UNIQUEID,
                                          CalNo = e.CALNO,
                                          LastCalibrateDate = e.LASTCALDATE,
                                          NextCalibrateDate = e.NEXTCALDATE,
                                          CalibratorID = e.LASTCALUSERID,
                                          CalibratorName = calibrator != null ? calibrator.NAME : "",
                                           IchiUniqueID=e.ICHIUNIQUEID,
                                           IchiName=i!=null?i.NAME:"",
                                            IchiRemark=e.ICHIREMARK
                                      }).ToList()
                    };
                }

                result.ReturnData(model);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult Create(CreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var vhnoPrefix = string.Format("A{0}", DateTime.Today.ToString("yyyyMM").Substring(2));

                    var vhnoSeq = 1;

                    var query = db.QA_ABNORMALFORM.Where(x => x.VHNO.StartsWith(vhnoPrefix)).OrderByDescending(x => x.VHNO).ToList();

                    if (query.Count > 0)
                    {
                        vhnoSeq = int.Parse(query.First().VHNO.Substring(7)) + 1;
                    }

                    var vhno = string.Format("{0}{1}", vhnoPrefix, vhnoSeq.ToString().PadLeft(3, '0'));

                    var uniqueID = Guid.NewGuid().ToString();

                    db.QA_CALIBRATIONFORM.First(x => x.UNIQUEID == Model.CalibrationFormUniqueID).STATUS = "7";

                    db.QA_ABNORMALFORM.Add(new QA_ABNORMALFORM()
                    {
                        UNIQUEID = uniqueID,
                        VHNO = vhno,
                        CALFORMUNIQUEID = Model.CalibrationFormUniqueID,
                        CREATETIME = DateTime.Now,
                        STATUS = "1",
                        REMARK = Model.FormInput.OtherInformation
                    });

                    db.QA_ABNORMALFORMDETAIL.AddRange(Model.ItemList.Select(x => new QA_ABNORMALFORMDETAIL
                    {
                        FORMUNIQUEID = uniqueID,
                        SEQ = x.Seq,
                        CHARACTERISTIC = x.Characteristic,
                        USINGRANGE = x.UsingRange,
                        CALIBRATIONPOINT = x.CalibrationPoint,
                        TOLERANCE = x.ToleranceDisplay,
                         STANDARD = x.StandardDisplay,
                         CALDATE=x.CalibrateDate,
                        READINGVALUE = x.ReadingValue.HasValue ? decimal.Parse(x.ReadingValue.Value.ToString()) : default(decimal?)
                    }).ToList());

                    db.QA_ABNORMALFORMSTDUSE.AddRange(Model.STDUSEList.Select(x => new QA_ABNORMALFORMSTDUSE
                    {
                        FORMUNIQUEID = uniqueID,
                        EQUIPMENTUNIQUEID = x.UniqueID
                    }).ToList());

                    foreach (var file in Model.FileList)
                    {
                        db.QA_ABNORMALFORMFILE.Add(new QA_ABNORMALFORMFILE()
                        {
                            FORMUNIQUEID = uniqueID,
                            SEQ = file.Seq,
                            EXTENSION = file.Extension,
                            FILENAME = file.FileName,
                            UPLOADTIME = file.LastModifyTime,
                            CONTENTLENGTH = file.Size
                        });

                        if (!file.IsSaved)
                        {
                            System.IO.File.Copy(Path.Combine(Config.TempFolder, file.TempFileName), Path.Combine(Config.EquipmentMaintenanceFileFolderPath, string.Format("{0}_{1}.{2}", uniqueID, file.Seq, file.Extension)), true);
                            System.IO.File.Delete(Path.Combine(Config.TempFolder, file.TempFileName));
                        }
                    }

                    db.SaveChanges();

                    result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.CalibrationAbnormalForm, Resources.Resource.Success));
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

        public static RequestResult GetDetailViewModel(List<Models.Shared.Organization> OrganizationList, string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new DetailViewModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = (from x in db.QA_ABNORMALFORM
                                     join f in db.QA_CALIBRATIONFORM
                                     on x.CALFORMUNIQUEID equals f.UNIQUEID
                                     join a in db.QA_CALIBRATIONAPPLY
                                     on f.APPLYUNIQUEID equals a.UNIQUEID
                                     join calibrator in db.ACCOUNT
                                     on f.CALRESPONSORID equals calibrator.ID into tmpCalibrator
                                     from calibrator in tmpCalibrator.DefaultIfEmpty()
                                     where x.UNIQUEID == UniqueID
                                     select new
                                     {
                                         UniqueID = x.UNIQUEID,
                                         x.VHNO,
                                         Status = x.STATUS,
                                         EquipmentUniqueID=a.EQUIPMENTUNIQUEID,
                                         CreateTime = x.CREATETIME,
                                         CalibrateDate = f.ESTCALDATE,
                                         CalibrationFormUniqueID = f.UNIQUEID,
                                         HandlingRemark=x.HANDLINGREMARK,
                                         OtherInformation = x.REMARK,
                                         CalibratorID = f.CALRESPONSORID,
                                         CalibratorName = calibrator != null ? calibrator.NAME : "",
                                         FlowVHNO=x.FLOWVHNO,
                                         FlowClosedDate = x.FLOWCLOSEDDATE,
                                         FlowFileExtension=x.FLOWFILEEXTENSION
                                     }).FirstOrDefault();

                    if (form == null)
                    {
                        form = (from x in db.QA_ABNORMALFORM
                                join f in db.QA_CALIBRATIONFORM
                                on x.CALFORMUNIQUEID equals f.UNIQUEID
                                join n in db.QA_CALIBRATIONNOTIFY
                                on f.NOTIFYUNIQUEID equals n.UNIQUEID
                                join calibrator in db.ACCOUNT
                                on f.CALRESPONSORID equals calibrator.ID into tmpCalibrator
                                from calibrator in tmpCalibrator.DefaultIfEmpty()
                                where x.UNIQUEID == UniqueID
                                select new
                                {
                                    UniqueID = x.UNIQUEID,
                                    x.VHNO,
                                    Status = x.STATUS,
                                    EquipmentUniqueID = n.EQUIPMENTUNIQUEID,
                                    CreateTime = x.CREATETIME,
                                    CalibrateDate = f.ESTCALDATE,
                                    CalibrationFormUniqueID = f.UNIQUEID,
                                    HandlingRemark = x.HANDLINGREMARK,
                                    OtherInformation = x.REMARK,
                                    CalibratorID = f.CALRESPONSORID,
                                    CalibratorName = calibrator != null ? calibrator.NAME : "",
                                    FlowVHNO = x.FLOWVHNO,
                                    FlowClosedDate = x.FLOWCLOSEDDATE,
                                    FlowFileExtension = x.FLOWFILEEXTENSION
                                }).FirstOrDefault();
                    }

                    model = new DetailViewModel()
                    {
                        UniqueID = form.UniqueID,
                        VHNO = form.VHNO,
                        Status = form.Status,
                        Equipment = EquipmentHelper.Get(OrganizationList, form.EquipmentUniqueID),
                        CalibrateDate = form.CalibrateDate,
                        CalibratorID = form.CalibratorID,
                        CalibratorName = form.CalibratorName,
                        CreateTime = form.CreateTime.Value,
                        OtherInformation = form.OtherInformation,
                        HandlingRemark = form.HandlingRemark,
                         FlowVHNO=form.FlowVHNO,
                          FlowClosedDate = form.FlowClosedDate,
                            FlowFileExtension=form.FlowFileExtension,
                        ItemList = db.QA_ABNORMALFORMDETAIL.Where(a => a.FORMUNIQUEID == form.UniqueID).OrderBy(x => x.SEQ).ToList().Select(a => new DetailItem
                        {
                            Seq = a.SEQ,
                            Standard = a.STANDARD,
                            Characteristic = a.CHARACTERISTIC,
                            UsingRange = a.USINGRANGE,
                            CalibrationPoint = a.CALIBRATIONPOINT,
                            Tolerance = a.TOLERANCE,
                            CalibrateDate = a.CALDATE,
                            ReadingValue = a.READINGVALUE.HasValue ? double.Parse(a.READINGVALUE.Value.ToString()) : default(double?)
                        }).ToList(),
                        FileList = db.QA_ABNORMALFORMFILE.Where(f => f.FORMUNIQUEID == form.UniqueID).ToList().Select(f => new FileModel
                        {
                            Seq = f.SEQ,
                            FileName = f.FILENAME,
                            Extension = f.EXTENSION,
                            Size = f.CONTENTLENGTH,
                            LastModifyTime = f.UPLOADTIME,
                            IsSaved = true
                        }).OrderBy(f => f.LastModifyTime).ToList(),
                        STDUSEList = (from x in db.QA_ABNORMALFORMSTDUSE
                                      join e in db.QA_EQUIPMENT
                                      on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                      join calibrator in db.ACCOUNT
                                      on e.LASTCALUSERID equals calibrator.ID into tmpCalibrator
                                      from calibrator in tmpCalibrator.DefaultIfEmpty()
                                      where x.FORMUNIQUEID == form.UniqueID
                                      select new STDUSEModel
                                      {
                                          UniqueID = e.UNIQUEID,
                                          CalNo = e.CALNO,
                                          LastCalibrateDate = e.LASTCALDATE,
                                          NextCalibrateDate = e.NEXTCALDATE,
                                          CalibratorID = e.LASTCALUSERID,
                                          CalibratorName = calibrator != null ? calibrator.NAME : ""
                                      }).ToList(),
                        LogList = (from l in db.QA_ABNORMALFORMFLOWLOG
                                   join u in db.ACCOUNT
                                   on l.USERID equals u.ID into tmpUser
                                   from u in tmpUser.DefaultIfEmpty()
                                   where l.FORMUNIQUEID == form.UniqueID
                                   select new LogModel
                                   {
                                       Seq = l.SEQ,
                                       FlowSeq = l.FLOWSEQ,
                                       NotifyTime = l.NOTIFYTIME,
                                       VerifyResult = l.VERIFYRESULT,
                                       UserID = l.USERID,
                                       UserName = u != null ? u.NAME : "",
                                       VerifyTime = l.VERIFYTIME,
                                       VerifyComment = l.VERIFYCOMMENT
                                   }).OrderBy(x => x.VerifyTime).ThenBy(x => x.Seq).ToList()
                    };

                    var changeForm = (from f in db.QA_CHANGEFORM
                                     join owner in db.ACCOUNT
                                     on f.NEWOWNERID equals owner.ID into tmpOwner
                                     from owner in tmpOwner.DefaultIfEmpty()
                                     join ownerMgr in db.ACCOUNT
                                     on f.NEWOWNERMANAGERID equals ownerMgr.ID into tmpOwnerMgr
                                     from ownerMgr in tmpOwnerMgr.DefaultIfEmpty()
                                     join pe in db.ACCOUNT
                                     on f.NEWPEID equals pe.ID into tmpPE
                                     from pe in tmpPE.DefaultIfEmpty()
                                     join peMgr in db.ACCOUNT
                                     on f.NEWPEMANAGERID equals peMgr.ID into tmpPEMgr
                                     from peMgr in tmpPEMgr.DefaultIfEmpty()
                                     where f.ABNORMALFORMUNIQUEID==form.UniqueID
                                     select new{
                                    UniqueID= f.UNIQUEID,
                                    VHNO=f.VHNO,
                                    Status=f.STATUS,
                                    ChangeType=f.CHANGETYPE,
                                    ChangeReason=f.CHANGEREASON,
                                   FixFinishedDate= f.FIXFINISHEDDATE,
                                   CreateTime=f.CREATETIME,
                                     OwnerID=f.NEWOWNERID,
                                     OwnerName=owner!=null?owner.NAME:"",
                                     OwnerManagerID=f.NEWOWNERMANAGERID,
                                     OwnerManagerName=ownerMgr!=null?ownerMgr.NAME:"",
                                     PEID=f.NEWPEID,
                                     PEName=pe!=null?pe.NAME:"",
                                     PEManagerID=f.NEWPEMANAGERID,
                                     PEManagerName=peMgr!=null?peMgr.NAME:"",
                                     }).FirstOrDefault();

                    if (changeForm != null)
                    {
                        model.ChangeForm = new ChangeFormModel()
                        {
                            UniqueID = changeForm.UniqueID,
                            ChangeReason = changeForm.ChangeReason,
                            ChangeType = changeForm.ChangeType,
                            CreateTime = changeForm.CreateTime,
                            FixFinishedDate = changeForm.FixFinishedDate,
                            OwnerID = changeForm.OwnerID,
                            OwnerName = changeForm.OwnerName,
                            OwnerManagerID = changeForm.OwnerManagerID,
                            OwnerManagerName = changeForm.OwnerManagerName,
                            PEID = changeForm.PEID,
                            PEName=changeForm.PEName,
                            PEManagerID=changeForm.PEManagerID,
                            PEManagerName=changeForm.PEManagerName,
                            Status = changeForm.Status, 
                            VHNO = changeForm.VHNO
                        };
                    }
                    else
                    {
                        model.ChangeForm = null;
                    }
                }

                result.ReturnData(model);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        private static void SendVerifyMail()
        { 
        
        }

        private static void SendRejectMail()
        {

        }

        public static FileDownloadModel GetFile(string FormUniqueID, int Seq)
        {
            var model = new FileDownloadModel();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var file = db.QA_ABNORMALFORMFILE.First(x => x.FORMUNIQUEID == FormUniqueID && x.SEQ == Seq);

                    model = new FileDownloadModel()
                    {
                        FormUniqueID = file.FORMUNIQUEID,
                        Seq = file.SEQ,
                        FileName = file.FILENAME,
                        Extension = file.EXTENSION
                    };
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
