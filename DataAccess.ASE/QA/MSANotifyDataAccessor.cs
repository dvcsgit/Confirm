using DbEntity.ASE;
using Models.ASE.QA.MSANotify;
using Models.Authenticated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;
using Utility.Models;
using System.Transactions;
using System.Net.Mail;

namespace DataAccess.ASE.QA
{
    public class MSANotifyDataAccessor
    {
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

        public static List<GridItem> Query(List<Models.Shared.Organization> OrganizationList, string EquipmentUniqueID)
        {
            var model = new List<GridItem>();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = (from x in db.QA_MSANOTIFY
                                 join e in db.QA_EQUIPMENT
                                 on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                 join o in db.ORGANIZATION
                                 on e.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                 join i in db.QA_ICHI on e.ICHIUNIQUEID equals i.UNIQUEID into tmpIchi
                                 from i in tmpIchi.DefaultIfEmpty()
                                 where x.EQUIPMENTUNIQUEID==EquipmentUniqueID
                                 select new
                                 {
                                     UniqueID = x.UNIQUEID,
                                     VHNO = x.VHNO,
                                     Status = x.STATUS,
                                     CalNo = e.MSACALNO,
                                     OrganizationUniqueID = e.ORGANIZATIONUNIQUEID,
                                     OrganizationDescription = o.DESCRIPTION,
                                     x.ESTMSADATE,
                                     SerialNo = e.SERIALNO,
                                     IchiUniqueID = e.ICHIUNIQUEID,
                                     IchiName = i != null ? i.NAME : "",
                                     IchiRemark = e.ICHIREMARK,
                                     Brand = e.BRAND,
                                     Model = e.MODEL,
                                     CreateTime = x.CREATETIME,
                                     OwnerID = e.OWNERID,
                                     OwnerManagerID = e.OWNERMANAGERID,
                                     PEID = e.PEID,
                                     PEManagerID = e.PEMANAGERID,
                                 }).AsQueryable();

                    model = query.ToList().Select(x => new GridItem
                    {
                        UniqueID = x.UniqueID,
                        VHNO = x.VHNO,
                        Status = new NotifyStatus(x.Status),
                        CalNo = x.CalNo,
                        EstMSADate = x.ESTMSADATE,
                        Factory = OrganizationDataAccessor.GetFactory(OrganizationList, x.OrganizationUniqueID),
                        Department = x.OrganizationDescription,
                        SerialNo = x.SerialNo,
                        IchiUniqueID = x.IchiUniqueID,
                        IchiName = x.IchiName,
                        IchiRemark = x.IchiRemark,
                        Brand = x.Brand,
                        Model = x.Model,
                        CreateTime = x.CreateTime,
                        OwnerID = x.OwnerID,
                        OwnerManagerID = x.OwnerManagerID,
                        PEID = x.PEID,
                        PEManagerID = x.PEManagerID,
                        LogList = db.QA_MSANOTIFYFLOWLOG.Where(l => l.NOTIFYUNIQUEID == x.UniqueID && l.ISCANCELED == "N" && !l.VERIFYTIME.HasValue).Select(l => new LogModel
                        {
                            Seq = l.SEQ,
                            FlowSeq = l.FLOWSEQ,
                            NotifyTime = l.NOTIFYTIME,
                            VerifyResult = l.VERIFYRESULT,
                            UserID = l.USERID,
                            VerifyTime = l.VERIFYTIME,
                            VerifyComment = l.VERIFYCOMMENT,
                        }).ToList()
                    }).OrderBy(x => x.EstMSADateString).ToList();
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
                var isQA = Account.UserAuthGroupList.Contains("QA-Verify") || Account.UserAuthGroupList.Contains("QA") || Account.UserAuthGroupList.Contains("QA-FullQuery");

                var model = new GridViewModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = (from x in db.QA_MSANOTIFY
                                 join e in db.QA_EQUIPMENT
                                 on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                 join o in db.ORGANIZATION
                                 on e.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                 join i in db.QA_ICHI on e.ICHIUNIQUEID equals i.UNIQUEID into tmpIchi
                                 from i in tmpIchi.DefaultIfEmpty()
                                 where Parameters.StatusList.Contains(x.STATUS)
                                 select new
                                 {
                                     UniqueID = x.UNIQUEID,
                                     VHNO = x.VHNO,
                                     Status = x.STATUS,
                                     CalNo = e.MSACALNO,
                                     OrganizationUniqueID = e.ORGANIZATIONUNIQUEID,
                                     OrganizationDescription = o.DESCRIPTION,
                                     x.ESTMSADATE,
                                     SerialNo = e.SERIALNO,
                                     IchiUniqueID = e.ICHIUNIQUEID,
                                     IchiName = i != null ? i.NAME : "",
                                     IchiRemark = e.ICHIREMARK,
                                     Brand = e.BRAND,
                                     Model = e.MODEL,
                                     CreateTime = x.CREATETIME,
                                     OwnerID = e.OWNERID,
                                     OwnerManagerID = e.OWNERMANAGERID,
                                     PEID = e.PEID,
                                     PEManagerID = e.PEMANAGERID,
                                 }).AsQueryable();

                    if (!isQA)
                    {
                        query = query.Where(x => Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID) || x.OwnerID == Account.ID || x.OwnerManagerID == Account.ID || x.PEID == Account.ID || x.PEManagerID == Account.ID).AsQueryable();
                    }

                    if (Parameters.BeginDate.HasValue)
                    {
                        query = query.Where(x => DateTime.Compare(x.ESTMSADATE.Value, Parameters.BeginDate.Value) >= 0);
                    }

                    if (Parameters.EndDate.HasValue)
                    {
                        query = query.Where(x => DateTime.Compare(x.ESTMSADATE.Value, Parameters.EndDate.Value) < 0);
                    }

                    if (!string.IsNullOrEmpty(Parameters.VHNO))
                    {
                        query = query.Where(x => x.VHNO.Contains(Parameters.VHNO));
                    }

                    if (!string.IsNullOrEmpty(Parameters.SerialNo))
                    {
                        query = query.Where(x => x.SerialNo.Contains(Parameters.SerialNo));
                    }

                    if (!string.IsNullOrEmpty(Parameters.CalNo))
                    {
                        query = query.Where(x => x.CalNo.Contains(Parameters.CalNo));
                    }

                    if (!string.IsNullOrEmpty(Parameters.OwnerID))
                    {
                        query = query.Where(x => x.OwnerID == Parameters.OwnerID);
                    }

                    if (!string.IsNullOrEmpty(Parameters.OwnerManagerID))
                    {
                        query = query.Where(x => x.OwnerManagerID == Parameters.OwnerManagerID);
                    }

                    if (!string.IsNullOrEmpty(Parameters.PEID))
                    {
                        query = query.Where(x => x.PEID == Parameters.PEID);
                    }

                    if (!string.IsNullOrEmpty(Parameters.PEManagerID))
                    {
                        query = query.Where(x => x.PEManagerID == Parameters.PEManagerID);
                    }

                    if (!string.IsNullOrEmpty(Parameters.FactoryUniqueID))
                    {
                        var downStream = OrganizationDataAccessor.GetDownStreamOrganizationList(OrganizationList, Parameters.FactoryUniqueID, true);

                        query = query.Where(x => downStream.Contains(x.OrganizationUniqueID));
                    }

                    if (!string.IsNullOrEmpty(Parameters.IchiName))
                    {
                        query = query.Where(x => x.IchiName.Contains(Parameters.IchiName) || x.IchiRemark.Contains(Parameters.IchiName));
                    }

                    if (!string.IsNullOrEmpty(Parameters.Brand))
                    {
                        query = query.Where(x => x.Brand.Contains(Parameters.Brand));
                    }

                    if (!string.IsNullOrEmpty(Parameters.Model))
                    {
                        query = query.Where(x => x.Model.Contains(Parameters.Model));
                    }

                    model.ItemList = query.ToList().Select(x => new GridItem
                    {
                        UniqueID = x.UniqueID,
                        VHNO = x.VHNO,
                        Status = new NotifyStatus(x.Status),
                        CalNo = x.CalNo,
                        EstMSADate = x.ESTMSADATE,
                        Factory = OrganizationDataAccessor.GetFactory(OrganizationList, x.OrganizationUniqueID),
                        Department = x.OrganizationDescription,
                        SerialNo = x.SerialNo,
                        IchiUniqueID = x.IchiUniqueID,
                        IchiName = x.IchiName,
                        IchiRemark = x.IchiRemark,
                        Brand = x.Brand,
                        Model = x.Model,
                        CreateTime = x.CreateTime,
                        OwnerID = x.OwnerID,
                        OwnerManagerID = x.OwnerManagerID,
                        PEID = x.PEID,
                        PEManagerID = x.PEManagerID,
                        Account = Account,
                        LogList = db.QA_MSANOTIFYFLOWLOG.Where(l => l.NOTIFYUNIQUEID == x.UniqueID && l.ISCANCELED == "N" && !l.VERIFYTIME.HasValue).Select(l => new LogModel
                        {
                            Seq = l.SEQ,
                            FlowSeq = l.FLOWSEQ,
                            NotifyTime = l.NOTIFYTIME,
                            VerifyResult = l.VERIFYRESULT,
                            UserID = l.USERID,
                            VerifyTime = l.VERIFYTIME,
                            VerifyComment = l.VERIFYCOMMENT,
                        }).ToList()
                    }).OrderBy(x => x.Seq).ThenBy(x => x.StatusSeq).ThenBy(x => x.EstMSADateString).ToList();
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

        public static RequestResult GetDetailViewModel(List<Models.Shared.Organization> OrganizationList, string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                result.ReturnData(new DetailViewModel()
                {
                    UniqueID = UniqueID,
                    FormViewModel = GetFormViewModel(OrganizationList, UniqueID)
                });
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetManagerFormModel(List<Models.Shared.Organization> OrganizationList, string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    result.ReturnData(new ManagerFormModel()
                    {
                        UniqueID = UniqueID,
                        FormViewModel = GetFormViewModel(OrganizationList, UniqueID),
                        VerifyCommentList = db.QA_VERIFYCOMMENT.OrderBy(x => x.DESCRIPTION).ToList().Select(x => new VerifyCommentModel()
                        {
                            Description = x.DESCRIPTION,
                            UniqueID = x.UNIQUEID
                        }).ToList(),
                    });
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

        public static RequestResult GetPEFormModel(List<Models.Shared.Organization> OrganizationList, string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new PEFormModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var notify = db.QA_MSANOTIFY.First(x => x.UNIQUEID == UniqueID);
                    var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == notify.EQUIPMENTUNIQUEID);

                    model = new PEFormModel()
                    {
                        UniqueID = UniqueID,
                        FormViewModel = GetFormViewModel(OrganizationList, UniqueID),
                        VerifyCommentList = db.QA_VERIFYCOMMENT.OrderBy(x => x.DESCRIPTION).ToList().Select(x => new VerifyCommentModel()
                        {
                            Description = x.DESCRIPTION,
                            UniqueID = x.UNIQUEID
                        }).ToList(),
                        MSAStationSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        },
                        MSAIchiSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        },
                        MSAIchiList = db.QA_MSAICHI.OrderBy(x => x.ID).Select(x => new MSAIchiModel
                        {
                            StationUniqueID = x.STATIONUNIQUEID,
                            UniqueID = x.UNIQUEID,
                            ID = x.ID,
                            Name = x.NAME
                        }).ToList(),
                        FormInput = new PEFormInput()
                        {
                            MSAType = !string.IsNullOrEmpty(notify.MSATYPE) ? notify.MSATYPE : equipment.MSATYPE,
                            MSASubType = !string.IsNullOrEmpty(notify.MSASUBTYPE) ? notify.MSASUBTYPE : equipment.MSASUBTYPE,
                            MSAIchiRemark = !string.IsNullOrEmpty(notify.MSAICHIREMARK) ? notify.MSAICHIREMARK : equipment.MSAICHIREMARK,
                            MSAIchiUniqueID = !string.IsNullOrEmpty(notify.MSAICHIUNIQUEID) ? notify.MSAICHIUNIQUEID : equipment.MSAICHIUNIQUEID,
                            MSAStationUniqueID = !string.IsNullOrEmpty(notify.MSASTATIONUNIQUEID) ? notify.MSASTATIONUNIQUEID : equipment.MSASTATIONUNIQUEID,
                            MSAStationRemark = !string.IsNullOrEmpty(notify.MSASTATIONREMARK) ? notify.MSASTATIONREMARK : equipment.MSASTATIONREMARK
                        }
                    };

                    var stationList = db.QA_MSASTATION.OrderBy(x => x.NAME).ToList();

                    foreach (var s in stationList)
                    {
                        model.MSAStationSelectItemList.Add(new SelectListItem()
                        {
                            Value = s.UNIQUEID,
                            Text = s.NAME
                        });

                        if (s.UNIQUEID == model.FormInput.MSAStationUniqueID)
                        {
                            var ichiList = model.MSAIchiList.Where(x => x.StationUniqueID == s.UNIQUEID).ToList();

                            foreach (var ichi in ichiList)
                            {
                                model.MSAIchiSelectItemList.Add(new SelectListItem()
                                {
                                    Value = ichi.UniqueID,
                                    Text = string.Format("{0}/{1}", ichi.ID, ichi.Name)
                                });
                            }

                            model.MSAIchiSelectItemList.Add(new SelectListItem()
                            {
                                Value = Define.OTHER,
                                Text = Resources.Resource.Other
                            });
                        }
                    }

                    model.MSAStationSelectItemList.Add(new SelectListItem()
                    {
                        Value = Define.OTHER,
                        Text = Resources.Resource.Other
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

        public static RequestResult GetQAFormModel(List<Models.Shared.Organization> OrganizationList, string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new QAFormModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var formViewModel = GetFormViewModel(OrganizationList, UniqueID);

                    model = new QAFormModel()
                    {
                        UniqueID = UniqueID,
                        FormViewModel = formViewModel,
                        VerifyCommentList = db.QA_VERIFYCOMMENT.OrderBy(x => x.DESCRIPTION).ToList().Select(x => new VerifyCommentModel()
                        {
                            Description = x.DESCRIPTION,
                            UniqueID = x.UNIQUEID
                        }).ToList(),
                        FormInput = new QAFormInput()
                        {
                            EstMSADateString = formViewModel.EstMSADateString
                        }
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

        public static RequestResult Approve(List<Models.Shared.Organization> OrganizationList, string UniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var notify = db.QA_MSANOTIFY.First(x => x.UNIQUEID == UniqueID);
                    var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == notify.EQUIPMENTUNIQUEID);

                    var log = db.QA_MSANOTIFYFLOWLOG.Where(x => x.NOTIFYUNIQUEID == notify.UNIQUEID && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue && x.USERID == Account.ID).OrderBy(x => x.SEQ).First();

                    log.VERIFYTIME = DateTime.Now;
                    log.VERIFYRESULT = "Y";

                    var logSeq = db.QA_MSANOTIFYFLOWLOG.Where(x => x.NOTIFYUNIQUEID == notify.UNIQUEID).Max(x => x.SEQ) + 1;

                    if (log.FLOWSEQ == 3)
                    {
                        db.QA_MSANOTIFYFLOWLOG.Add(new QA_MSANOTIFYFLOWLOG()
                        {
                            NOTIFYUNIQUEID = notify.UNIQUEID,
                            SEQ = logSeq,
                            FLOWSEQ = 4,
                            USERID = equipment.PEMANAGERID,
                            NOTIFYTIME = DateTime.Now,
                            ISCANCELED = "N"
                        });

                        logSeq++;

                        SendVerifyMail(OrganizationList, equipment.PEMANAGERID, notify);
                    }
                    //PEManager
                    else if (log.FLOWSEQ == 4)
                    {
                        db.QA_MSANOTIFYFLOWLOG.Add(new QA_MSANOTIFYFLOWLOG()
                        {
                            NOTIFYUNIQUEID = notify.UNIQUEID,
                            SEQ = logSeq,
                            FLOWSEQ = 5,
                            NOTIFYTIME = DateTime.Now,
                            ISCANCELED = "N"
                        });

                        logSeq++;

                        SendVerifyMail(OrganizationList, notify);
                    }

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Verify, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult Reject(List<Models.Shared.Organization> OrganizationList, string UniqueID, string Comment, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                    var notify = db.QA_MSANOTIFY.First(x => x.UNIQUEID == UniqueID);
                    var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == notify.EQUIPMENTUNIQUEID);

                    notify.STATUS = "2";

                    db.SaveChanges();

                    var log = db.QA_MSANOTIFYFLOWLOG.Where(x => x.NOTIFYUNIQUEID == notify.UNIQUEID && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue && x.USERID == Account.ID).OrderBy(x => x.SEQ).First();

                    log.VERIFYTIME = DateTime.Now;
                    log.VERIFYRESULT = "N";
                    log.VERIFYCOMMENT = Comment;

                    db.SaveChanges();

                    var logList = db.QA_MSANOTIFYFLOWLOG.Where(x => x.NOTIFYUNIQUEID == UniqueID && x.SEQ != log.SEQ && !x.VERIFYTIME.HasValue).ToList();

                    foreach (var l in logList)
                    {
                        l.ISCANCELED = "Y";
                    }

                    db.SaveChanges();

                    var logSeq = db.QA_MSANOTIFYFLOWLOG.Where(x => x.NOTIFYUNIQUEID == notify.UNIQUEID).Max(x => x.SEQ) + 1;

                    db.QA_MSANOTIFYFLOWLOG.Add(new QA_MSANOTIFYFLOWLOG()
                    {
                        NOTIFYUNIQUEID = notify.UNIQUEID,
                        SEQ = logSeq,
                        FLOWSEQ = 3,
                        NOTIFYTIME = DateTime.Now,
                        USERID = equipment.PEID,
                        ISCANCELED = "N"
                    });

                    db.SaveChanges();

#if !DEBUG
                        trans.Complete();
#endif

                    SendRejectMail(OrganizationList, new List<string>() { equipment.PEID }, notify);
#if !DEBUG
                    }
#endif
                }

                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Verify, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult ChangePE(List<Models.Shared.Organization> OrganizationList, PEFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var notify = db.QA_MSANOTIFY.First(x => x.UNIQUEID == Model.UniqueID);
                    var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == notify.EQUIPMENTUNIQUEID);

                    var log = db.QA_MSANOTIFYFLOWLOG.First(x => x.NOTIFYUNIQUEID == notify.UNIQUEID && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue && x.FLOWSEQ == 3);

                    log.VERIFYTIME = DateTime.Now;
                    log.VERIFYRESULT = "Y";
                    log.VERIFYCOMMENT = Model.FormInput.Comment;

                    var logSeq = db.QA_MSANOTIFYFLOWLOG.Where(x => x.NOTIFYUNIQUEID == notify.UNIQUEID).Max(x => x.SEQ) + 1;

                    equipment.PEID = Model.FormInput.PEID;
                    equipment.PEMANAGERID = Model.FormInput.PEManagerID;

                    db.QA_MSANOTIFYFLOWLOG.Add(new QA_MSANOTIFYFLOWLOG()
                    {
                        NOTIFYUNIQUEID = notify.UNIQUEID,
                        SEQ = logSeq,
                        FLOWSEQ = 3,
                        USERID = Model.FormInput.PEID,
                        NOTIFYTIME = DateTime.Now,
                        ISCANCELED = "N"
                    });

                    SendVerifyMail(OrganizationList, Model.FormInput.PEID, notify);

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Submit, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult PEApprove(List<Models.Shared.Organization> OrganizationList, PEFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                    var log = db.QA_MSANOTIFYFLOWLOG.FirstOrDefault(x => x.NOTIFYUNIQUEID == Model.UniqueID && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue && x.FLOWSEQ == 3);

                        if (log != null)
                        {
                            log.VERIFYTIME = DateTime.Now;
                            log.VERIFYRESULT = "Y";

                            db.SaveChanges();

                            var notify = db.QA_MSANOTIFY.First(x => x.UNIQUEID == Model.UniqueID);

                            if (notify.STATUS == "2")
                            {
                                notify.STATUS = "1";
                            }

                            var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == notify.EQUIPMENTUNIQUEID);

                            notify.MSATYPE = Model.FormInput.MSAType;

                            if (Model.FormInput.MSAType == "1")
                            {
                                notify.MSASUBTYPE = Model.FormInput.MSASubType;
                            }
                            else
                            {
                                notify.MSASUBTYPE = null;
                            }

                            notify.MSASTATIONUNIQUEID = Model.FormInput.MSAStationUniqueID;
                            notify.MSASTATIONREMARK = Model.FormInput.MSAStationRemark;
                            notify.MSAICHIUNIQUEID = Model.FormInput.MSAIchiUniqueID;
                            notify.MSAICHIREMARK = Model.FormInput.MSAIchiRemark;

                            db.SaveChanges();

                            int msaSeq = 1;

                            db.QA_MSANOTIFYDETAIL.RemoveRange(db.QA_MSANOTIFYDETAIL.Where(x => x.NOTIFYUNIQUEID == notify.UNIQUEID).ToList());

                            db.SaveChanges();

                            foreach (var s in Model.FormInput.MSACharacteristicList)
                            {
                                string[] temp = s.Split(Define.Seperators, StringSplitOptions.None);

                                string characteristicUniqueID = temp[0];
                                string characteristicRemark = temp[1];
                                string unitUniqueID = temp[2];
                                string unitRemark = temp[3];
                                string lowerRange = temp[4];
                                string upperRange = temp[5];

                                db.QA_MSANOTIFYDETAIL.Add(new QA_MSANOTIFYDETAIL()
                                {
                                    NOTIFYUNIQUEID = notify.UNIQUEID,
                                    SEQ = msaSeq,
                                    MSACHARACTERISITICUNIQUEID = characteristicUniqueID,
                                    MSACHARACTERISITICREMARK = characteristicRemark,
                                    MSAUNITUNIQUEID = unitUniqueID,
                                    MSAUNITREMARK = unitRemark,
                                    LOWERRANGE = lowerRange,
                                    UPPERRANGE = upperRange
                                });

                                msaSeq++;
                            }

                            db.SaveChanges();

                            var logSeq = db.QA_MSANOTIFYFLOWLOG.Where(x => x.NOTIFYUNIQUEID == notify.UNIQUEID).Max(x => x.SEQ) + 1;

                            db.QA_MSANOTIFYFLOWLOG.Add(new QA_MSANOTIFYFLOWLOG()
                            {
                                NOTIFYUNIQUEID = notify.UNIQUEID,
                                SEQ = logSeq,
                                FLOWSEQ = 4,
                                USERID = equipment.PEMANAGERID,
                                NOTIFYTIME = DateTime.Now,
                                ISCANCELED = "N"
                            });

                            db.SaveChanges();

                            SendVerifyMail(OrganizationList, equipment.PEMANAGERID, notify);
                        }
#if !DEBUG
                        trans.Complete();
                    }
#endif
                }

                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Submit, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult QAApprove(QAFormModel Model, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                    var notify = db.QA_MSANOTIFY.First(x => x.UNIQUEID == Model.UniqueID);

                    notify.STATUS = "3";

                    //notify.ESTMSADATE = Model.FormInput.EstMSADate;

                    //var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == notify.EQUIPMENTUNIQUEID);

                    //equipment.NEXTMSADATE = Model.FormInput.EstMSADate;

                    db.QA_EQUIPMENTMSADETAIL.RemoveRange(db.QA_EQUIPMENTMSADETAIL.Where(x => x.EQUIPMENTUNIQUEID == notify.EQUIPMENTUNIQUEID).ToList());

                    db.SaveChanges();

                    var log = db.QA_MSANOTIFYFLOWLOG.FirstOrDefault(x => x.NOTIFYUNIQUEID == notify.UNIQUEID && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue && x.FLOWSEQ == 5);

                    if (log != null)
                    {
                        log.USERID = Account.ID;
                        log.VERIFYTIME = DateTime.Now;
                        log.VERIFYRESULT = "Y";

                        db.SaveChanges();
                    }

                    db.QA_EQUIPMENTMSADETAIL.AddRange(db.QA_MSANOTIFYDETAIL.Where(x => x.NOTIFYUNIQUEID == notify.UNIQUEID).ToList().Select(x => new QA_EQUIPMENTMSADETAIL
                    {
                        EQUIPMENTUNIQUEID = notify.EQUIPMENTUNIQUEID,
                        LOWERRANGE = x.LOWERRANGE,
                        MSACHARACTERISITICREMARK = x.MSACHARACTERISITICREMARK,
                        MSACHARACTERISITICUNIQUEID = x.MSACHARACTERISITICUNIQUEID,
                        MSAUNITREMARK = x.MSAUNITREMARK,
                        MSAUNITUNIQUEID = x.MSAUNITUNIQUEID,
                        SEQ = x.SEQ,
                        UPPERRANGE = x.UPPERRANGE
                    }).ToList());

                    db.SaveChanges();

                    var vhnoPrefix = string.Format("MSA{0}", DateTime.Today.ToString("yyyyMM").Substring(2));

                    var vhnoSeq = 1;

                    var query = db.QA_MSAFORM.Where(x => x.VHNO.StartsWith(vhnoPrefix)).OrderByDescending(x => x.VHNO).ToList();

                    if (query.Count > 0)
                    {
                        vhnoSeq = int.Parse(query.First().VHNO.Substring(7, 4)) + 1;
                    }

                    var vhno = string.Format("{0}{1}", vhnoPrefix, vhnoSeq.ToString().PadLeft(4, '0'));

                    var createTime = DateTime.Now;

                    if (Model.FormViewModel.MSACharacteristicList.Count > 1)
                    {
                        var subSeq = 1;

                        foreach (var detail in Model.FormViewModel.MSACharacteristicList)
                        {
                            var subVhno = string.Format("{0}-{1}", vhno, subSeq);

                            var formUniqueID = Guid.NewGuid().ToString();

                            db.QA_MSAFORM.Add(new QA_MSAFORM()
                            {
                                UNIQUEID = formUniqueID,
                                VHNO = subVhno,
                                EQUIPMENTUNIQUEID = notify.EQUIPMENTUNIQUEID,
                                STATUS = "1",
                                LOWERRANGE = detail.LowerRange,
                                UPPERRANGE = detail.UpperRange,
                                CREATETIME = createTime,
                                CHARACTERISITIC = detail.Charateristic,
                                ESTMSADATE = notify.ESTMSADATE,
                                //ESTMSADATE = Model.FormInput.EstMSADate,
                                MSAICHI = Model.FormViewModel.MSAIchiDisplay,
                                MSARESPONSORID = Model.FormViewModel.PEID,
                                STATION = Model.FormViewModel.MSAStationDisplay,
                                TYPE = Model.FormViewModel.MSAType,
                                SUBTYPE = Model.FormViewModel.MSASubType,
                                UNIT = detail.Unit
                            });

                            subSeq++;
                        }
                    }
                    else
                    {
                        var formUniqueID = Guid.NewGuid().ToString();

                        var detail = Model.FormViewModel.MSACharacteristicList.First();

                        db.QA_MSAFORM.Add(new QA_MSAFORM()
                        {
                            UNIQUEID = formUniqueID,
                            VHNO = vhno,
                            EQUIPMENTUNIQUEID = notify.EQUIPMENTUNIQUEID,
                            STATUS = "1",
                            LOWERRANGE = detail.LowerRange,
                            UPPERRANGE = detail.UpperRange,
                            CREATETIME = createTime,
                            CHARACTERISITIC = detail.Charateristic,
                            //ESTMSADATE = Model.FormInput.EstMSADate,
                            ESTMSADATE = notify.ESTMSADATE,
                            MSAICHI = Model.FormViewModel.MSAIchiDisplay,
                            MSARESPONSORID = Model.FormViewModel.PEID,
                            STATION = Model.FormViewModel.MSAStationDisplay,
                            TYPE = Model.FormViewModel.MSAType,
                            UNIT = detail.Unit,
                            SUBTYPE = Model.FormViewModel.MSASubType
                        });
                    }

                    db.SaveChanges();

#if !DEBUG
                        trans.Complete();
                    }
#endif
                }

                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Verify, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult QAReject(List<Models.Shared.Organization> OrganizationList, string UniqueID, string Comment, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                    var notify = db.QA_MSANOTIFY.First(x => x.UNIQUEID == UniqueID);
                    var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == notify.EQUIPMENTUNIQUEID);

                    notify.STATUS = "2";

                    db.SaveChanges();

                    var log = db.QA_MSANOTIFYFLOWLOG.First(x => x.NOTIFYUNIQUEID == notify.UNIQUEID && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue && x.FLOWSEQ == 5);

                    log.USERID = Account.ID;
                    log.VERIFYTIME = DateTime.Now;
                    log.VERIFYRESULT = "N";
                    log.VERIFYCOMMENT = Comment;

                    db.SaveChanges();

                    var logList = db.QA_MSANOTIFYFLOWLOG.Where(x => x.NOTIFYUNIQUEID == notify.UNIQUEID && x.SEQ != log.SEQ && !x.VERIFYTIME.HasValue).ToList();

                    foreach (var l in logList)
                    {
                        l.ISCANCELED = "Y";
                    }

                    db.SaveChanges();

                    var seq = db.QA_MSANOTIFYFLOWLOG.Where(x => x.NOTIFYUNIQUEID == notify.UNIQUEID).Max(x => x.SEQ) + 1;

                    db.QA_MSANOTIFYFLOWLOG.Add(new QA_MSANOTIFYFLOWLOG()
                    {
                        NOTIFYUNIQUEID = notify.UNIQUEID,
                        SEQ = seq,
                        FLOWSEQ = 3,
                        NOTIFYTIME = DateTime.Now,
                        USERID = equipment.PEID,
                        ISCANCELED = "N"
                    });

                    db.SaveChanges();

#if !DEBUG
                        trans.Complete();
#endif

                    SendRejectMail(OrganizationList, new List<string>() { equipment.PEID }, notify);
#if !DEBUG
                    }
#endif
                }

                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Verify, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetMSACharacteristicList(string IchiUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = new List<MSACharacteristicFormModel>();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var characteristicsList = db.QA_MSACHARACTERISTICS.Where(x => x.ICHIUNIQUEID == IchiUniqueID).OrderBy(x => x.NAME).ToList();

                    foreach (var characteristics in characteristicsList)
                    {
                        var item = new MSACharacteristicFormModel()
                        {
                            UniqueID = characteristics.UNIQUEID,
                            Name = characteristics.NAME,
                            UnitSelectItemList = new List<SelectListItem>() 
                            {
                                Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                            }
                        };

                        var unitList = db.QA_MSAUNIT.Where(x => x.CHARACTERISTICSUNIQUEID == characteristics.UNIQUEID).OrderBy(x => x.DESCRIPTION).ToList();

                        foreach (var unit in unitList)
                        {
                            item.UnitSelectItemList.Add(new SelectListItem()
                            {
                                Value = unit.UNIQUEID,
                                Text = unit.DESCRIPTION
                            });
                        }

                        item.UnitSelectItemList.Add(new SelectListItem()
                        {
                            Value = Define.OTHER,
                            Text = Resources.Resource.Other
                        });

                        itemList.Add(item);
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

        private static FormViewModel GetFormViewModel(List<Models.Shared.Organization> OrganizationList, string UniqueID)
        {
            var model = new FormViewModel();

            using (ASEDbEntities db = new ASEDbEntities())
            {
                var notify = (from a in db.QA_MSANOTIFY
                              join e in db.QA_EQUIPMENT
                              on a.EQUIPMENTUNIQUEID equals e.UNIQUEID
                             join ichi in db.QA_ICHI on e.ICHIUNIQUEID equals ichi.UNIQUEID into tmpIchi
                             from ichi in tmpIchi.DefaultIfEmpty()
                             join msaStation in db.QA_MSASTATION on a.MSASTATIONUNIQUEID equals msaStation.UNIQUEID into tmpMSAStation
                             from msaStation in tmpMSAStation.DefaultIfEmpty()
                             join msaIchi in db.QA_MSAICHI on a.MSAICHIUNIQUEID equals msaIchi.UNIQUEID into tmpMSAIchi
                             from msaIchi in tmpMSAIchi.DefaultIfEmpty()
                             join owner in db.ACCOUNT on e.OWNERID equals owner.ID into tmpOwner
                             from owner in tmpOwner.DefaultIfEmpty()
                             join ownerManager in db.ACCOUNT on e.OWNERMANAGERID equals ownerManager.ID into tmpOwnerManager
                             from ownerManager in tmpOwnerManager.DefaultIfEmpty()
                             join pe in db.ACCOUNT on e.PEID equals pe.ID into tmpPE
                             from pe in tmpPE.DefaultIfEmpty()
                             join peManager in db.ACCOUNT on e.PEMANAGERID equals peManager.ID into tmpPEManager
                             from peManager in tmpPEManager.DefaultIfEmpty()
                             where a.UNIQUEID == UniqueID
                             select new
                             {
                                 a.UNIQUEID,
                                 a.VHNO,
                                 a.EQUIPMENTUNIQUEID,
                                 a.STATUS,
                                 a.CREATETIME,
                                 e.ORGANIZATIONUNIQUEID,
                                 e.FACTORYID,
                                 e.ICHITYPE,
                                 e.ICHIUNIQUEID,
                                 IchiName = ichi != null ? ichi.NAME : "",
                                 e.ICHIREMARK,
                                 e.CHARACTERISTICTYPE,
                                 e.MACHINENO,
                                 e.SERIALNO,
                                 e.BRAND,
                                 e.MODEL,
                                 e.SPEC,
                                 e.OWNERID,
                                 OwnerName = owner != null ? owner.NAME : "",
                                 e.OWNERMANAGERID,
                                 OwnerManagerName = ownerManager != null ? ownerManager.NAME : "",
                                 e.PEID,
                                 PEName = pe != null ? pe.NAME : "",
                                 e.PEMANAGERID,
                                 PEManagerName = peManager != null ? peManager.NAME : "",
                                 e.MSACYCLE,
                                 e.REMARK,
                                 a.ESTMSADATE,
                                 a.MSATYPE,
                                 a.MSASUBTYPE,
                                 MSAResponserName = pe != null ? pe.NAME : "",
                                 a.MSASTATIONUNIQUEID,
                                 MSAStationName = msaStation != null ? msaStation.NAME : "",
                                 a.MSASTATIONREMARK,
                                 a.MSAICHIUNIQUEID,
                                 MSAIchiName = msaIchi != null ? msaIchi.NAME : "",
                                 a.MSAICHIREMARK
                             }).First();

                model = new FormViewModel()
                {
                    VHNO = notify.VHNO,
                    Status = new NotifyStatus(notify.STATUS),
                    Equipment = EquipmentHelper.Get(OrganizationList, notify.EQUIPMENTUNIQUEID),
                    CreateTime = notify.CREATETIME,
                    Factory = OrganizationDataAccessor.GetFactory(OrganizationList, notify.ORGANIZATIONUNIQUEID),
                    Department = OrganizationDataAccessor.GetOrganizationFullDescription(notify.ORGANIZATIONUNIQUEID),
                    FactoryID = notify.FACTORYID,
                    IchiType = notify.ICHITYPE,
                    IchiUniqueID = notify.ICHIUNIQUEID,
                    IchiName = notify.IchiName,
                    IchiRemark = notify.ICHIREMARK,
                    CharacteristicType = notify.CHARACTERISTICTYPE,
                    SerialNo = notify.SERIALNO,
                    MachineNo = notify.MACHINENO,
                    Brand = notify.BRAND,
                    Model = notify.MODEL,
                    Spec = notify.SPEC,
                    OwnerID = notify.OWNERID,
                    OwnerName = notify.OwnerName,
                    OwnerManagerID = notify.OWNERMANAGERID,
                    OwnerManagerName = notify.OwnerManagerName,
                    PEID = notify.PEID,
                    PEName = notify.PEName,
                    PEManagerID = notify.PEMANAGERID,
                    PEManagerName = notify.PEManagerName,
                    Cycle = notify.MSACYCLE.Value,
                    Remark = notify.REMARK,
                    MSAResponsorID = notify.PEID,
                    MSAResponsorName = notify.MSAResponserName,
                    EstMSADate = notify.ESTMSADATE,
                    MSAType = notify.MSATYPE,
                    MSASubType = notify.MSASUBTYPE,
                    MSAStationUniqueID = notify.MSASTATIONUNIQUEID,
                    MSAStationName = notify.MSAStationName,
                    MSAStationRemark = notify.MSASTATIONREMARK,
                    MSAIchiUniqueID = notify.MSAICHIUNIQUEID,
                    MSAIchiName = notify.MSAIchiName,
                    MSAIchiRemark = notify.MSAICHIREMARK,
                    MSACharacteristicList = (from msaDetail in db.QA_MSANOTIFYDETAIL
                                             join msaCharacteristic in db.QA_MSACHARACTERISTICS on msaDetail.MSACHARACTERISITICUNIQUEID equals msaCharacteristic.UNIQUEID into tmpCharacteristic
                                             from msaCharacteristic in tmpCharacteristic.DefaultIfEmpty()
                                             join msaUnit in db.QA_MSAUNIT on msaDetail.MSAUNITUNIQUEID equals msaUnit.UNIQUEID into tmpUnit
                                             from msaUnit in tmpUnit.DefaultIfEmpty()
                                             where msaDetail.NOTIFYUNIQUEID == notify.UNIQUEID
                                             select new MSACharacteristicModel
                                             {
                                                 Seq = msaDetail.SEQ,
                                                 CharateristicUniqueID = msaDetail.MSACHARACTERISITICUNIQUEID,
                                                 CharateristicName = msaCharacteristic != null ? msaCharacteristic.NAME : "",
                                                 CharateristicRemark = msaDetail.MSACHARACTERISITICREMARK,
                                                 UnitUniqueID = msaDetail.MSAUNITUNIQUEID,
                                                 UnitDescription = msaUnit != null ? msaUnit.DESCRIPTION : "",
                                                 UnitRemark = msaDetail.MSAUNITREMARK,
                                                 LowerRange = msaDetail.LOWERRANGE,
                                                 UpperRange = msaDetail.UPPERRANGE
                                             }).OrderBy(x => x.Seq).ToList(),
                    LogList = (from log in db.QA_MSANOTIFYFLOWLOG
                               join user in db.ACCOUNT on log.USERID equals user.ID into tmpUser
                               from user in tmpUser.DefaultIfEmpty()
                               where log.NOTIFYUNIQUEID == notify.UNIQUEID && log.ISCANCELED == "N"
                               select new LogModel
                               {
                                   Seq = log.SEQ,
                                   FlowSeq = log.FLOWSEQ,
                                   NotifyTime = log.NOTIFYTIME,
                                   VerifyResult = log.VERIFYRESULT,
                                   UserID = log.USERID,
                                   UserName = user != null ? user.NAME : "",
                                   VerifyTime = log.VERIFYTIME,
                                   VerifyComment = log.VERIFYCOMMENT,
                               }).OrderBy(x => x.VerifyTime).ThenBy(x => x.Seq).ToList()
                };
            }

            return model;
        }

        public static void SendRemindMail(string UniqueID, int Days,string UserID, bool IsDelay)
        {
            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var notify = db.QA_MSANOTIFY.First(x => x.UNIQUEID == UniqueID);

                    var equipment = EquipmentHelper.Get(null, notify.EQUIPMENTUNIQUEID);

                    if (IsDelay)
                    {
                        SendVerifyMail(null, new List<string>() { UserID }, string.Format("[逾期][簽核通知][{0}天][{1}][{2}]MSA通知單", Days, notify.VHNO, equipment.MSACalNo), notify);
                    }
                    else
                    {
                        SendVerifyMail(null, new List<string>() { UserID }, string.Format("[跟催][簽核通知][{0}天][{1}][{2}]MSA通知單", Days, notify.VHNO, equipment.MSACalNo), notify);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public static void SendVerifyMail(string UniqueID, string UserID)
        {
            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var notify = db.QA_MSANOTIFY.First(x => x.UNIQUEID == UniqueID);

                    var equipment = EquipmentHelper.Get(null, notify.EQUIPMENTUNIQUEID);

                    SendVerifyMail(null, new List<string>() { UserID }, string.Format("[簽核通知][{0}][{1}]MSA通知單", notify.VHNO, equipment.MSACalNo), notify);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public static void SendRemindMail(string UniqueID, int Days, bool IsDelay)
        {
            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var notify = db.QA_MSANOTIFY.First(x => x.UNIQUEID == UniqueID);

                    var equipment = EquipmentHelper.Get(null, notify.EQUIPMENTUNIQUEID);

                    var userList = db.USERAUTHGROUP.Where(x => x.AUTHGROUPID == "QA-Verify").Select(x => x.USERID).ToList();

                    if (userList != null && userList.Count > 0)
                    {
                        if (IsDelay)
                        {
                            SendVerifyMail(null, userList, string.Format("[逾期][簽核通知][{0}天][{1}][{2}]MSA通知單", Days, notify.VHNO, equipment.MSACalNo), notify);
                        }
                        else
                        {
                            SendVerifyMail(null, userList, string.Format("[跟催][簽核通知][{0}天][{1}][{2}]MSA通知單", Days, notify.VHNO, equipment.MSACalNo), notify);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public static void SendVerifyMail(string UniqueID)
        {
            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var notify = db.QA_MSANOTIFY.First(x => x.UNIQUEID == UniqueID);

                    var equipment = EquipmentHelper.Get(null, notify.EQUIPMENTUNIQUEID);

                    var userList = db.USERAUTHGROUP.Where(x => x.AUTHGROUPID == "QA-Verify").Select(x => x.USERID).ToList();

                    if (userList != null && userList.Count > 0)
                    {
                        SendVerifyMail(null, userList, string.Format("[簽核通知][{0}][{1}]MSA通知單", notify.VHNO, equipment.MSACalNo), notify);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public static void SendVerifyMail(List<Models.Shared.Organization> OrganizationList, string UserID, QA_MSANOTIFY Notify)
        {
            var equipment = EquipmentHelper.Get(OrganizationList, Notify.EQUIPMENTUNIQUEID);

            SendVerifyMail(OrganizationList, new List<string>() { UserID }, string.Format("[簽核通知][{0}][{1}]MSA通知單", Notify.VHNO, equipment.MSACalNo), Notify);
        }

        private static void SendVerifyMail(List<Models.Shared.Organization> OrganizationList, QA_MSANOTIFY Notify)
        {
            try
            {
                var equipment = EquipmentHelper.Get(OrganizationList, Notify.EQUIPMENTUNIQUEID);

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var userList = db.USERAUTHGROUP.Where(x => x.AUTHGROUPID == "QA-Verify").Select(x => x.USERID).ToList();

                    if (userList != null && userList.Count > 0)
                    {
                        SendVerifyMail(OrganizationList, userList, string.Format("[簽核通知][{0}][{1}]MSA通知單", Notify.VHNO, equipment.MSACalNo), Notify);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private static void SendVerifyMail(List<Models.Shared.Organization> OrganizationList, List<string> UserList, string Subject, QA_MSANOTIFY Notify)
        {
            try
            {
                var mailAddressList = new List<MailAddress>();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    foreach (var u in UserList)
                    {
                        var user = db.ACCOUNT.FirstOrDefault(x => x.ID == u);

                        if (user != null && !string.IsNullOrEmpty(user.EMAIL))
                        {
                            mailAddressList.Add(new MailAddress(user.EMAIL, user.NAME));
                        }
                    }

                    if (mailAddressList.Count > 0)
                    {
                        var equipment = EquipmentHelper.Get(OrganizationList, Notify.EQUIPMENTUNIQUEID);

                        var th = "<th style=\"width:150px;border:1px solid #333;padding:8px;text-align:right;color:#707070;background: #F4F4F4;\">{0}</th>";
                        var td = "<td style=\"width:400px;border:1px solid #333;padding:8px;color:#707070;\">{0}</td>";

                        var sb = new StringBuilder();

                        sb.Append("<table style=\"1px solid #ddd;font-size:13px;border-collapse:collapse;\">");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "單號"));
                        sb.Append(string.Format(td, Notify.VHNO));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "校驗編號"));
                        sb.Append(string.Format(td, equipment.MSACalNo));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "預計MSA日期"));
                        sb.Append(string.Format(td, DateTimeHelper.DateTime2DateStringWithSeperator(Notify.ESTMSADATE)));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "部門"));
                        sb.Append(string.Format(td, equipment.OrganizationDescription));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "序號"));
                        sb.Append(string.Format(td, equipment.SerialNo));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "儀器名稱"));
                        sb.Append(string.Format(td, equipment.IchiDisplay));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "廠牌"));
                        sb.Append(string.Format(td, equipment.Brand));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "型號"));
                        sb.Append(string.Format(td, equipment.Model));
                        sb.Append("</tr>");

                        if (!string.IsNullOrEmpty(equipment.OwnerID))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "設備負責人"));
                            sb.Append(string.Format(td, equipment.Owner));
                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(equipment.OwnerManagerID))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "設備負責人主管"));

                            sb.Append(string.Format(td, equipment.OwnerManager));

                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(equipment.PEID))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "製程負責人"));

                            sb.Append(string.Format(td, equipment.PE));

                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(equipment.PEManagerID))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "製程負責人主管"));

                            sb.Append(string.Format(td, equipment.PEManager));

                            sb.Append("</tr>");
                        }

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "立案時間"));
                        sb.Append(string.Format(td, DateTimeHelper.DateTime2DateTimeStringWithSeperator(Notify.CREATETIME)));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "連結"));
                        sb.Append(string.Format(td, string.Format("<a href=\"http://ASECL-eINSPRD01/FEM.Portal/Home/Index?ReturnUrl=http://ASECL-eINSPRD01/FEM/zh-tw/Customized_ASE_QA/MSANotify/Index?VHNO={0}\">連結</a>", Notify.VHNO)));
                        sb.Append("</tr>");

                        MailHelper.SendMail(mailAddressList, Subject, sb.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public static void SendRejectMail(List<Models.Shared.Organization> OrganizationList, List<string> UserList, QA_MSANOTIFY Notify)
        {
            var equipment = EquipmentHelper.Get(OrganizationList, Notify.EQUIPMENTUNIQUEID);

            SendVerifyMail(OrganizationList, UserList, string.Format("[退回修正通知][{0}][{1}]MSA通知單", Notify.VHNO, equipment.MSACalNo), Notify);
        }
    }
}
