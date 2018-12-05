using Models.ASE.QA.CalibrationApply;
using DataAccess.ASE;
using DbEntity.ASE;
using Models.Authenticated;
using Models.Shared;
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
    public class CalibrationApplyDataAccessor
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

        public static RequestResult Delete(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var apply = db.QA_CALIBRATIONAPPLY.First(x => x.UNIQUEID == UniqueID);

                    db.QA_CALIBRATIONAPPLYDETAIL.RemoveRange(db.QA_CALIBRATIONAPPLYDETAIL.Where(x => x.APPLYUNIQUEID == UniqueID).ToList());
                    db.QA_CALIBRATIONAPPLYFLOWLOG.RemoveRange(db.QA_CALIBRATIONAPPLYFLOWLOG.Where(x => x.APPLYUNIQUEID == UniqueID).ToList());
                    db.QA_CALIBRATIONAPPLYMSADETAIL.RemoveRange(db.QA_CALIBRATIONAPPLYMSADETAIL.Where(x => x.APPLYUNIQUEID == UniqueID).ToList());
                    db.QA_CALIBRATIONAPPLYSUBDETAIL.RemoveRange(db.QA_CALIBRATIONAPPLYSUBDETAIL.Where(x => x.APPLYUNIQUEID == UniqueID).ToList());
                    db.QA_CALIBRATIONAPPLY.Remove(apply);

                    var form = db.QA_CALIBRATIONFORM.FirstOrDefault(x => x.APPLYUNIQUEID == UniqueID);

                    if (form != null)
                    {
                        db.QA_CALIBRATIONFORMDETAIL.RemoveRange(db.QA_CALIBRATIONFORMDETAIL.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList());
                        db.QA_CALIBRATIONFORMFILE.RemoveRange(db.QA_CALIBRATIONFORMFILE.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList());
                        db.QA_CALIBRATIONFORMFLOWLOG.RemoveRange(db.QA_CALIBRATIONFORMFLOWLOG.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList());
                        db.QA_CALIBRATIONFORMSTDUSE.RemoveRange(db.QA_CALIBRATIONFORMSTDUSE.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList());
                        db.QA_CALIBRATIONFORMSTEPLOG.RemoveRange(db.QA_CALIBRATIONFORMSTEPLOG.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList());
                        db.QA_CALIBRATIONFORMTAKEJOBLOG.RemoveRange(db.QA_CALIBRATIONFORMTAKEJOBLOG.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList());
                        db.QA_CALIBRATIONFORM.Remove(form);
                    }

                    if (!string.IsNullOrEmpty(apply.EQUIPMENTUNIQUEID))
                    {
                        db.QA_EQUIPMENT.RemoveRange(db.QA_EQUIPMENT.Where(x => x.UNIQUEID == apply.EQUIPMENTUNIQUEID).ToList());
                        db.QA_EQUIPMENTCALDETAIL.RemoveRange(db.QA_EQUIPMENTCALDETAIL.Where(x => x.EQUIPMENTUNIQUEID == apply.EQUIPMENTUNIQUEID).ToList());
                        db.QA_EQUIPMENTCALSUBDETAIL.RemoveRange(db.QA_EQUIPMENTCALSUBDETAIL.Where(x => x.EQUIPMENTUNIQUEID == apply.EQUIPMENTUNIQUEID).ToList());
                        db.QA_EQUIPMENTMSADETAIL.RemoveRange(db.QA_EQUIPMENTMSADETAIL.Where(x => x.EQUIPMENTUNIQUEID == apply.EQUIPMENTUNIQUEID).ToList());

                        var notifyList = db.QA_CALIBRATIONNOTIFY.Where(x => x.EQUIPMENTUNIQUEID == apply.EQUIPMENTUNIQUEID).ToList();

                        foreach (var notify in notifyList)
                        {
                            db.QA_CALIBRATIONNOTIFYDETAIL.RemoveRange(db.QA_CALIBRATIONNOTIFYDETAIL.Where(x => x.NOTIFYUNIQUEID == notify.UNIQUEID).ToList());
                            db.QA_CALIBRATIONNOTIFYSUBDETAIL.RemoveRange(db.QA_CALIBRATIONNOTIFYSUBDETAIL.Where(x => x.NOTIFYUNIQUEID == notify.UNIQUEID).ToList());
                            db.QA_CALIBRATIONNOTIFYFLOWLOG.RemoveRange(db.QA_CALIBRATIONNOTIFYFLOWLOG.Where(x => x.NOTIFYUNIQUEID == notify.UNIQUEID).ToList());
                            db.QA_CALIBRATIONNOTIFY.Remove(notify);

                            var calibrationForm = db.QA_CALIBRATIONFORM.FirstOrDefault(x => x.NOTIFYUNIQUEID == notify.UNIQUEID);

                            if (calibrationForm != null)
                            {
                                db.QA_CALIBRATIONFORMDETAIL.RemoveRange(db.QA_CALIBRATIONFORMDETAIL.Where(x => x.FORMUNIQUEID == calibrationForm.UNIQUEID).ToList());
                                db.QA_CALIBRATIONFORMFILE.RemoveRange(db.QA_CALIBRATIONFORMFILE.Where(x => x.FORMUNIQUEID == calibrationForm.UNIQUEID).ToList());
                                db.QA_CALIBRATIONFORMFLOWLOG.RemoveRange(db.QA_CALIBRATIONFORMFLOWLOG.Where(x => x.FORMUNIQUEID == calibrationForm.UNIQUEID).ToList());
                                db.QA_CALIBRATIONFORMSTDUSE.RemoveRange(db.QA_CALIBRATIONFORMSTDUSE.Where(x => x.FORMUNIQUEID == calibrationForm.UNIQUEID).ToList());
                                db.QA_CALIBRATIONFORMSTEPLOG.RemoveRange(db.QA_CALIBRATIONFORMSTEPLOG.Where(x => x.FORMUNIQUEID == calibrationForm.UNIQUEID).ToList());
                                db.QA_CALIBRATIONFORMTAKEJOBLOG.RemoveRange(db.QA_CALIBRATIONFORMTAKEJOBLOG.Where(x => x.FORMUNIQUEID == calibrationForm.UNIQUEID).ToList());
                                db.QA_CALIBRATIONFORM.Remove(calibrationForm);
                            }
                        }

                        var msaNotifyList = db.QA_MSANOTIFY.Where(x => x.EQUIPMENTUNIQUEID == apply.EQUIPMENTUNIQUEID).ToList();

                        foreach (var msaNotify in msaNotifyList)
                        {
                            db.QA_MSANOTIFYDETAIL.RemoveRange(db.QA_MSANOTIFYDETAIL.Where(x => x.NOTIFYUNIQUEID == msaNotify.UNIQUEID).ToList());
                            db.QA_MSANOTIFYFLOWLOG.RemoveRange(db.QA_MSANOTIFYFLOWLOG.Where(x => x.NOTIFYUNIQUEID == msaNotify.UNIQUEID).ToList());
                            db.QA_MSANOTIFY.Remove(msaNotify);
                        }

                        var msaFormList = db.QA_MSAFORM.Where(x => x.EQUIPMENTUNIQUEID == apply.EQUIPMENTUNIQUEID).ToList();

                        foreach (var msaForm in msaFormList)
                        {
                            db.QA_MSAFORMBIASVALUE.RemoveRange(db.QA_MSAFORMBIASVALUE.Where(x => x.FORMUNIQUEID == msaForm.UNIQUEID).ToList());
                            db.QA_MSAFORMCOUNT.RemoveRange(db.QA_MSAFORMCOUNT.Where(x => x.FORMUNIQUEID == msaForm.UNIQUEID).ToList());
                            db.QA_MSAFORMCOUNTREFVAL.RemoveRange(db.QA_MSAFORMCOUNTREFVAL.Where(x => x.FORMUNIQUEID == msaForm.UNIQUEID).ToList());
                            db.QA_MSAFORMCOUNTVALUE.RemoveRange(db.QA_MSAFORMCOUNTVALUE.Where(x => x.FORMUNIQUEID == msaForm.UNIQUEID).ToList());
                            db.QA_MSAFORMGRR.RemoveRange(db.QA_MSAFORMGRR.Where(x => x.FORMUNIQUEID == msaForm.UNIQUEID).ToList());
                            db.QA_MSAFORMGRRVALUE.RemoveRange(db.QA_MSAFORMGRRVALUE.Where(x => x.FORMUNIQUEID == msaForm.UNIQUEID).ToList());
                            db.QA_MSAFORMLINEARITY.RemoveRange(db.QA_MSAFORMLINEARITY.Where(x => x.FORMUNIQUEID == msaForm.UNIQUEID).ToList());
                            db.QA_MSAFORMLINEARITYVALUE.RemoveRange(db.QA_MSAFORMLINEARITYVALUE.Where(x => x.FORMUNIQUEID == msaForm.UNIQUEID).ToList());
                            db.QA_MSAFORMSTABILITY.RemoveRange(db.QA_MSAFORMSTABILITY.Where(x => x.FORMUNIQUEID == msaForm.UNIQUEID).ToList());
                            db.QA_MSAFORMSTABILITYVALUE.RemoveRange(db.QA_MSAFORMSTABILITYVALUE.Where(x => x.FORMUNIQUEID == msaForm.UNIQUEID).ToList());
                            db.QA_MSAFORM.Remove(msaForm);
                        }

                        var changeFormList = (from d in db.QA_CHANGEFORMDETAIL
                                              join f in db.QA_CHANGEFORM
                                              on d.FORMUNIQUEID equals f.UNIQUEID
                                              where d.EQUIPMENTUNIQUEID == apply.EQUIPMENTUNIQUEID
                                              select f.UNIQUEID).Distinct().ToList();

                        foreach (var changeForm in changeFormList)
                        {
                            var changeFormDetailList = db.QA_CHANGEFORMDETAIL.Where(x => x.FORMUNIQUEID == changeForm).ToList();

                            if (changeFormDetailList.Count > 1)
                            {
                                db.QA_CHANGEFORMDETAIL.Remove(db.QA_CHANGEFORMDETAIL.First(x => x.FORMUNIQUEID == changeForm && x.EQUIPMENTUNIQUEID == apply.EQUIPMENTUNIQUEID));
                            }
                            else
                            {
                                db.QA_CHANGEFORM.Remove(db.QA_CHANGEFORM.First(x => x.UNIQUEID == changeForm));
                                db.QA_CHANGEFORMDETAIL.RemoveRange(db.QA_CHANGEFORMDETAIL.Where(x => x.FORMUNIQUEID == changeForm).ToList());
                                db.QA_CHANGEFORMFLOWLOG.RemoveRange(db.QA_CHANGEFORMFLOWLOG.Where(x => x.FORMUNIQUEID == changeForm).ToList());
                            }
                        }
                    }

                    db.SaveChanges();

                    result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Delete, Resources.Resource.Success));
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

        public static RequestResult Query(List<Models.Shared.Organization> OrganizationList, QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var isQA = Account.UserAuthGroupList.Contains("QA-Verify") || Account.UserAuthGroupList.Contains("QA") || Account.UserAuthGroupList.Contains("QA-FullQuery");

                var model = new GridViewModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = (from x in db.QA_CALIBRATIONAPPLY
                                 join o in db.ORGANIZATION
                                 on x.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                 join i in db.QA_ICHI on x.ICHIUNIQUEID equals i.UNIQUEID into tmpIchi
                                 from i in tmpIchi.DefaultIfEmpty()
                                 join u in db.ACCOUNT on x.CREATORID equals u.ID into tmpCreator
                                 from u in tmpCreator.DefaultIfEmpty()
                                 where Parameters.StatusList.Contains(x.STATUS)
                                 select new
                                 {
                                     UniqueID = x.UNIQUEID,
                                     VHNO = x.VHNO,
                                     Status = x.STATUS,
                                     OrganizationUniqueID = x.ORGANIZATIONUNIQUEID,
                                     OrganizationDescription = o.DESCRIPTION,
                                     SerialNo = x.SERIALNO,
                                     IchiUniqueID = x.ICHIUNIQUEID,
                                     IchiName = i != null ? i.NAME : "",
                                     IchiRemark = x.ICHIREMARK,
                                     Brand = x.BRAND,
                                     Model = x.MODEL,
                                     CreatorID = x.CREATORID,
                                     CreatorName = u != null ? u.NAME : "",
                                     CreateTime = x.CREATETIME,
                                     OwnerID = x.OWNERID,
                                     OwnerManagerID = x.OWNERMANAGERID,
                                     PEID = x.PEID,
                                     PEManagerID = x.PEMANAGERID,
                                     x.MSA,
                                     x.CAL
                                 }).AsQueryable();

                    if (!isQA)
                    {
                        query = query.Where(x => Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID) || x.CreatorID == Account.ID || x.OwnerID == Account.ID || x.OwnerManagerID == Account.ID || x.PEID == Account.ID || x.PEManagerID == Account.ID).AsQueryable();
                    }

                    if (Parameters.BeginDate.HasValue)
                    {
                        query = query.Where(x => DateTime.Compare(x.CreateTime, Parameters.BeginDate.Value) >= 0);
                    }

                    if (Parameters.EndDate.HasValue)
                    {
                        query = query.Where(x => DateTime.Compare(x.CreateTime, Parameters.EndDate.Value) < 0);
                    }

                    if (!string.IsNullOrEmpty(Parameters.VHNO))
                    {
                        query = query.Where(x => x.VHNO.Contains(Parameters.VHNO));
                    }

                    if (!string.IsNullOrEmpty(Parameters.SerialNo))
                    {
                        query = query.Where(x => x.SerialNo.Contains(Parameters.SerialNo));
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
                        Status = new ApplyStatus(x.Status),
                        Factory = OrganizationDataAccessor.GetFactory(OrganizationList, x.OrganizationUniqueID),
                        Department = x.OrganizationDescription,
                        SerialNo = x.SerialNo,
                        IchiUniqueID = x.IchiUniqueID,
                        IchiName = x.IchiName,
                        IchiRemark = x.IchiRemark,
                        Brand = x.Brand,
                        Model = x.Model,
                        CreatorID = x.CreatorID,
                        CreatorName = x.CreatorName,
                        CreateTime = x.CreateTime,
                        OwnerID = x.OwnerID,
                        OwnerManagerID = x.OwnerManagerID,
                        PEID = x.PEID,
                        PEManagerID = x.PEManagerID,
                        MSA = x.MSA == "Y",
                        CAL = x.CAL == "Y",
                        Account = Account,
                        LogList = db.QA_CALIBRATIONAPPLYFLOWLOG.Where(l => l.APPLYUNIQUEID == x.UniqueID && l.ISCANCELED == "N" && !l.VERIFYTIME.HasValue).Select(l => new LogModel
                        {
                            Seq = l.SEQ,
                            FlowSeq = l.FLOWSEQ,
                            NotifyTime = l.NOTIFYTIME,
                            VerifyResult = l.VERIFYRESULT,
                            UserID = l.USERID,
                            VerifyTime = l.VERIFYTIME,
                            VerifyComment = l.VERIFYCOMMENT,
                        }).ToList()
                    }).OrderBy(x => x.Seq).ThenBy(x => x.StatusSeq).ThenByDescending(x => x.VHNO).ToList();
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

        public static GridItem Query(List<Models.Shared.Organization> OrganizationList, string EquipmentUniqueID)
        {
            var model = new GridItem();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = (from x in db.QA_CALIBRATIONAPPLY
                                 join o in db.ORGANIZATION
                                 on x.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                 join i in db.QA_ICHI on x.ICHIUNIQUEID equals i.UNIQUEID into tmpIchi
                                 from i in tmpIchi.DefaultIfEmpty()
                                 join u in db.ACCOUNT on x.CREATORID equals u.ID into tmpCreator
                                 from u in tmpCreator.DefaultIfEmpty()
                                 where x.EQUIPMENTUNIQUEID == EquipmentUniqueID
                                 select new
                                 {
                                     UniqueID = x.UNIQUEID,
                                     VHNO = x.VHNO,
                                     Status = x.STATUS,
                                     OrganizationUniqueID = x.ORGANIZATIONUNIQUEID,
                                     OrganizationDescription = o.DESCRIPTION,
                                     SerialNo = x.SERIALNO,
                                     IchiUniqueID = x.ICHIUNIQUEID,
                                     IchiName = i != null ? i.NAME : "",
                                     IchiRemark = x.ICHIREMARK,
                                     Brand = x.BRAND,
                                     Model = x.MODEL,
                                     CreatorID = x.CREATORID,
                                     CreatorName = u != null ? u.NAME : "",
                                     CreateTime = x.CREATETIME,
                                     OwnerID = x.OWNERID,
                                     OwnerManagerID = x.OWNERMANAGERID,
                                     PEID = x.PEID,
                                     PEManagerID = x.PEMANAGERID,
                                     x.MSA,
                                     x.CAL
                                 }).First();

                    model = new GridItem
                    {
                        UniqueID = query.UniqueID,
                        VHNO = query.VHNO,
                        Status = new ApplyStatus(query.Status),
                        Factory = OrganizationDataAccessor.GetFactory(OrganizationList, query.OrganizationUniqueID),
                        Department = query.OrganizationDescription,
                        SerialNo = query.SerialNo,
                        IchiUniqueID = query.IchiUniqueID,
                        IchiName = query.IchiName,
                        IchiRemark = query.IchiRemark,
                        Brand = query.Brand,
                        Model = query.Model,
                        CreatorID = query.CreatorID,
                        CreatorName = query.CreatorName,
                        CreateTime = query.CreateTime,
                        OwnerID = query.OwnerID,
                        OwnerManagerID = query.OwnerManagerID,
                        PEID = query.PEID,
                        PEManagerID = query.PEManagerID,
                        MSA = query.MSA == "Y",
                        CAL = query.CAL == "Y",
                        LogList = db.QA_CALIBRATIONAPPLYFLOWLOG.Where(l => l.APPLYUNIQUEID == query.UniqueID && l.ISCANCELED == "N" && !l.VERIFYTIME.HasValue).Select(l => new LogModel
                        {
                            Seq = l.SEQ,
                            FlowSeq = l.FLOWSEQ,
                            NotifyTime = l.NOTIFYTIME,
                            VerifyResult = l.VERIFYRESULT,
                            UserID = l.USERID,
                            VerifyTime = l.VERIFYTIME,
                            VerifyComment = l.VERIFYCOMMENT,
                        }).ToList()
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

        public static RequestResult GetCreateFormModel(Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var owner = db.ACCOUNT.First(x => x.ID == Account.ID);

                    var model = new CreateFormModel()
                    {
                        IchiTypeSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        },
                        IchiSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        },
                        CharacteristicTypeSelectItemList = new List<SelectListItem>()
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        },
                        IchiList = db.QA_ICHI.Select(x => new IchiModel
                        {
                            UniqueID = x.UNIQUEID,
                            Type = x.TYPE,
                            Name = x.NAME
                        }).OrderBy(x => x.Name).ToList(),
                        CharacteristicTypeList = (from x in db.QA_ICHICHARACTERISTIC
                                                  join c in db.QA_CHARACTERISTIC
                                                  on x.CHARACTERISTICUNIQUEID equals c.UNIQUEID
                                                  select new
                                                  {
                                                      x.ICHIUNIQUEID,
                                                      c.TYPE
                                                  }).Distinct().Select(x => new CharacteristicTypeModel
                                                  {
                                                      IchiUniqueID = x.ICHIUNIQUEID,
                                                      Type = x.TYPE
                                                  }).OrderBy(x => x.Type).ToList(),
                        FormInput = new FormInput()
                        {
                            CAL = true,
                            CalCycle = 12,
                            OwnerID = owner.ID,
                            OwnerManagerID = owner.MANAGERID
                        }
                    };

                    model.IchiTypeSelectItemList.AddRange(model.IchiList.Select(x => x.Type).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    model.IchiTypeSelectItemList.Add(new SelectListItem()
                    {
                        Value = "00",
                        Text = Resources.Resource.Other
                    });

                    result.ReturnData(model);
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

        public static RequestResult Create(CreateFormModel Model, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (Model.FormInput.CAL&& (!Model.FormInput.CalCycle.HasValue ||Model.FormInput.CalCycle.Value<=0))
                    {
                        result.ReturnFailedMessage("請輸入正確的校正頻率(月)");
                    }
                    else if (Model.FormInput.MSA && (!Model.FormInput.MSACycle.HasValue || Model.FormInput.MSACycle.Value <= 0))
                    {
                        result.ReturnFailedMessage("請輸入正確的MSA頻率(月)");
                    }
                    else if (!string.IsNullOrEmpty(Model.FormInput.SerialNo) && (db.QA_EQUIPMENT.Any(x => x.SERIALNO == Model.FormInput.SerialNo) || db.QA_CALIBRATIONAPPLY.Any(x => x.SERIALNO == Model.FormInput.SerialNo)))
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.SerialNo, Resources.Resource.Exists));
                    }
                    else if (Model.FormInput.CAL && (Model.ItemList == null || Model.ItemList.Count == 0))
                    {
                        result.ReturnFailedMessage("請建立校驗資訊");
                    }
                    else
                    {
                        #region VHNO
                        var vhnoPreFix = string.Format("NC{0}", DateTime.Today.ToString("yyyyMM").Substring(2));

                        var query = db.QA_CALIBRATIONAPPLY.Where(x => x.VHNO.StartsWith(vhnoPreFix)).OrderByDescending(x => x.VHNO).ToList();

                        int vhnoSeq = 1;

                        if (query.Count > 0)
                        {
                            vhnoSeq = int.Parse(query.First().VHNO.Substring(6)) + 1;
                        }

                        var vhno = string.Format("{0}{1}", vhnoPreFix, vhnoSeq.ToString().PadLeft(4, '0'));
                        #endregion

                        var verifyList = new List<string>();

                        var uniqueID = Guid.NewGuid().ToString();

                        var organizationUniqueID = string.Empty;

                        if (!string.IsNullOrEmpty(Model.FormInput.OwnerID))
                        {
                            organizationUniqueID = db.ACCOUNT.First(x => x.ID == Model.FormInput.OwnerID).ORGANIZATIONUNIQUEID;
                        }
                        else if (!string.IsNullOrEmpty(Model.FormInput.PEID))
                        {
                            organizationUniqueID = db.ACCOUNT.First(x => x.ID == Model.FormInput.PEID).ORGANIZATIONUNIQUEID;
                        }
                        else
                        {
                            organizationUniqueID = Account.OrganizationUniqueID;
                        }

                        var apply = new QA_CALIBRATIONAPPLY()
                        {
                            UNIQUEID = uniqueID,
                            VHNO = vhno,
                            STATUS = "1",
                            ORGANIZATIONUNIQUEID = organizationUniqueID,
                            FACTORYID = Model.FormInput.FactoryID,
                            ICHITYPE = Model.FormInput.IchiType,
                            ICHIUNIQUEID = Model.FormInput.IchiUniqueID,
                            ICHIREMARK = Model.FormInput.IchiRemark,
                            CHARACTERISTICTYPE = Model.FormInput.CharacteristicType,
                            SERIALNO = !string.IsNullOrEmpty(Model.FormInput.SerialNo) ? Model.FormInput.SerialNo : vhno,
                            MACHINENO = !string.IsNullOrEmpty(Model.FormInput.MachineNo) ? Model.FormInput.MachineNo : "NA",
                            BRAND = !string.IsNullOrEmpty(Model.FormInput.Brand) ? Model.FormInput.Brand : "NA",
                            MODEL = !string.IsNullOrEmpty(Model.FormInput.Model) ? Model.FormInput.Model : "NA",
                            SPEC = !string.IsNullOrEmpty(Model.FormInput.Spec) ? Model.FormInput.Spec : "NA",
                            CAL = Model.FormInput.CAL ? "Y" : "N",
                            MSA = Model.FormInput.MSA ? "Y" : "N",
                            CREATORID = Account.ID,
                            CREATETIME = DateTime.Now,
                            OWNERID = Model.FormInput.OwnerID,
                            OWNERMANAGERID = Model.FormInput.OwnerManagerID,
                            PEID = Model.FormInput.PEID,
                            PEMANAGERID = Model.FormInput.PEManagerID,
                            REMARK = !string.IsNullOrEmpty(Model.FormInput.Remark) ? Model.FormInput.Remark : "NA",
                            CALCYCLE = Model.FormInput.CalCycle,
                            MSACYCLE = Model.FormInput.MSACycle,
                            CYCLE = 0
                        };

                        db.QA_CALIBRATIONAPPLY.Add(apply);

                        var itemSeq = 1;

                        foreach (var item in Model.ItemList.OrderBy(x => x.Characteristic))
                        {
                            db.QA_CALIBRATIONAPPLYDETAIL.Add(new QA_CALIBRATIONAPPLYDETAIL()
                            {
                                APPLYUNIQUEID = uniqueID,
                                SEQ = itemSeq,
                                CHARACTERISTICUNIQUEID = item.CharacteristicUniqueID,
                                CHARACTERISTICREMARK = item.CharacteristicRemark,
                                UNITUNIQUEID = item.UnitUniqueID,
                                UNITREMARK = item.UnitRemark,
                                LOWERUSINGRANGE = item.LowerUsingRange,
                                LOWERUSINGRANGEUNITUNIQUEID = item.LowerUsingRangeUnitUniqueID,
                                LOWERUSINGRANGEUNITREMARK = item.LowerUsingRangeUnitRemark,
                                UPPERUSINGRANGE = item.UpperUsingRange,
                                UPPERUSINGRANGEUNITUNIQUEID = item.UpperUsingRangeUnitUniqueID,
                                UPPERUSINGRANGEUNITREMARK = item.UpperUsingRangeUnitRemark,
                                RANGETOLERANCESYMBOL = item.UsingRangeToleranceSymbol,
                                RANGETOLERANCE = item.UsingRangeTolerance,
                                RANGETOLERANCEUNITUNIQUEID = item.UsingRangeToleranceUnitUniqueID,
                                RANGETOLERANCEUNITREMARK = item.UsingRangeToleranceUnitRemark
                            });

                            var subItemSeq = 1;

                            foreach (var subitem in item.ItemList.OrderBy(x => x.CalibrationPoint))
                            {
                                db.QA_CALIBRATIONAPPLYSUBDETAIL.Add(new QA_CALIBRATIONAPPLYSUBDETAIL()
                                {
                                    APPLYUNIQUEID = uniqueID,
                                    DETAILSEQ = itemSeq,
                                    SEQ = subItemSeq,
                                    CALIBRATIONPOINT = Convert.ToDecimal(subitem.CalibrationPoint),
                                    CALIBRATIONPOINTUNITUNIQUEID = subitem.CalibrationPointUnitUniqueID,
                                    TOLERANCESYMBOL = subitem.ToleranceSymbol,
                                    TOLERANCE = Convert.ToDecimal(subitem.Tolerance),
                                    TOLERANCEUNITUNIQUEID = subitem.ToleranceUnitUniqueID
                                });

                                subItemSeq++;
                            }

                            itemSeq++;
                        }

                        db.SaveChanges();

                        #region Flow
                        var time = DateTime.Now;

                        var logSeq = 1;

                        #region Creator
                        db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                        {
                            APPLYUNIQUEID = uniqueID,
                            SEQ = logSeq,
                            FLOWSEQ = 0,
                            USERID = Account.ID,
                            NOTIFYTIME = time,
                            VERIFYTIME = time,
                            VERIFYRESULT = "Y",
                            ISCANCELED = "N"
                        });

                        logSeq++;
                        #endregion

                        //CAL
                        if (Model.FormInput.CAL)
                        {
                            //CAL Creator != Owner
                            if (Account.ID != Model.FormInput.OwnerID)
                            {
                                #region CAL Creator != Owner

                                #region Owner
                                db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                {
                                    APPLYUNIQUEID = uniqueID,
                                    SEQ = logSeq,
                                    FLOWSEQ = 1,
                                    USERID = Model.FormInput.OwnerID,
                                    NOTIFYTIME = time,
                                    ISCANCELED = "N"
                                });

                                logSeq++;

                                verifyList.Add(Model.FormInput.OwnerID);
                                //SendVerifyMail(uniqueID, vhno, Model.FormInput.OwnerID);
                                #endregion

                                #endregion
                            }
                            //CAL Creator == Owner
                            else
                            {
                                #region CAL Creator = Owner

                                #region Owner
                                db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                {
                                    APPLYUNIQUEID = uniqueID,
                                    SEQ = logSeq,
                                    FLOWSEQ = 1,
                                    USERID = Model.FormInput.OwnerID,
                                    NOTIFYTIME = time,
                                    VERIFYTIME = time,
                                    VERIFYRESULT = "Y",
                                    ISCANCELED = "N"
                                });

                                logSeq++;
                                #endregion

                                #region OwnerManager
                                db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                {
                                    APPLYUNIQUEID = uniqueID,
                                    SEQ = logSeq,
                                    FLOWSEQ = 2,
                                    USERID = Model.FormInput.OwnerManagerID,
                                    NOTIFYTIME = time,
                                    ISCANCELED = "N"
                                });

                                logSeq++;

                                verifyList.Add(Model.FormInput.OwnerManagerID);
                                //SendVerifyMail(uniqueID, vhno, Model.FormInput.OwnerManagerID);
                                #endregion

                                #region PE
                                db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                {
                                    APPLYUNIQUEID = uniqueID,
                                    SEQ = logSeq,
                                    FLOWSEQ = 3,
                                    USERID = Model.FormInput.PEID,
                                    NOTIFYTIME = time,
                                    ISCANCELED = "N"
                                });

                                logSeq++;

                                verifyList.Add(Model.FormInput.PEID);
                                //SendVerifyMail(uniqueID, vhno, Model.FormInput.PEID);
                                #endregion

                                #endregion
                            }
                        }
                        //!CAL
                        else
                        {
                            //!CAL && MSA
                            if (Model.FormInput.MSA)
                            {
                                //!CAL && MSA 有設定Owner
                                if (!string.IsNullOrEmpty(Model.FormInput.OwnerID))
                                {
                                    #region !CAL && MSA 有設定 Owner

                                    //Creator != Owner
                                    if (Account.ID != Model.FormInput.OwnerID)
                                    {
                                        #region !CAL && MSA 有設定Owner Creator != Owner

                                        #region Owner
                                        db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                        {
                                            APPLYUNIQUEID = uniqueID,
                                            SEQ = logSeq,
                                            FLOWSEQ = 1,
                                            USERID = Model.FormInput.OwnerID,
                                            NOTIFYTIME = time,
                                            ISCANCELED = "N"
                                        });

                                        logSeq++;

                                        verifyList.Add(Model.FormInput.OwnerID);
                                        //SendVerifyMail(uniqueID, vhno, Model.FormInput.OwnerID);
                                        #endregion

                                        #endregion
                                    }
                                    //Creator == Owner
                                    else
                                    {
                                        #region !CAL && MSA 有設定Owner Creator = Owner

                                        #region Owner
                                        db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                        {
                                            APPLYUNIQUEID = uniqueID,
                                            SEQ = logSeq,
                                            FLOWSEQ = 1,
                                            USERID = Model.FormInput.OwnerID,
                                            NOTIFYTIME = time,
                                            VERIFYTIME = time,
                                            VERIFYRESULT = "Y",
                                            ISCANCELED = "N"
                                        });

                                        logSeq++;
                                        #endregion

                                        #region OwnerManager
                                        db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                        {
                                            APPLYUNIQUEID = uniqueID,
                                            SEQ = logSeq,
                                            FLOWSEQ = 2,
                                            USERID = Model.FormInput.OwnerManagerID,
                                            NOTIFYTIME = time,
                                            ISCANCELED = "N"
                                        });

                                        logSeq++;

                                        verifyList.Add(Model.FormInput.OwnerManagerID);
                                        //SendVerifyMail(uniqueID, vhno, Model.FormInput.OwnerManagerID);
                                        #endregion

                                        #region PE
                                        db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                        {
                                            APPLYUNIQUEID = uniqueID,
                                            SEQ = logSeq,
                                            FLOWSEQ = 3,
                                            USERID = Model.FormInput.PEID,
                                            NOTIFYTIME = time,
                                            ISCANCELED = "N"
                                        });

                                        logSeq++;

                                        verifyList.Add(Model.FormInput.PEID);
                                        //SendVerifyMail(uniqueID, vhno, Model.FormInput.PEID);
                                        #endregion

                                        #endregion
                                    }

                                    #endregion
                                }
                                //!CAL && MSA 沒有設定Owner
                                else
                                {
                                    #region !CAL && MSA 沒有設定Owner

                                    #region PE
                                    db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                    {
                                        APPLYUNIQUEID = uniqueID,
                                        SEQ = logSeq,
                                        FLOWSEQ = 3,
                                        USERID = Model.FormInput.PEID,
                                        NOTIFYTIME = time,
                                        ISCANCELED = "N"
                                    });

                                    logSeq++;

                                    verifyList.Add(Model.FormInput.PEID);
                                    //SendVerifyMail(uniqueID, vhno, Model.FormInput.PEID);
                                    #endregion

                                    #endregion
                                }
                            }
                            //!CAL && !MSA
                            else
                            {
                                //!CAL && !MSA Creator != Owner
                                if (Account.ID != Model.FormInput.OwnerID)
                                {
                                    #region !CAL && !MSA Creator != Owner

                                    #region Owner
                                    db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                    {
                                        APPLYUNIQUEID = uniqueID,
                                        SEQ = logSeq,
                                        FLOWSEQ = 1,
                                        USERID = Model.FormInput.OwnerID,
                                        NOTIFYTIME = time,
                                        ISCANCELED = "N"
                                    });

                                    logSeq++;

                                    verifyList.Add(Model.FormInput.OwnerID);
                                    //SendVerifyMail(uniqueID, vhno, Model.FormInput.OwnerID);
                                    #endregion

                                    #endregion
                                }
                                //!CAL && !MSA Creator == Owner
                                else
                                {
                                    #region !CAL && !MSA Creator == Owner

                                    #region Owner
                                    db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                    {
                                        APPLYUNIQUEID = uniqueID,
                                        SEQ = logSeq,
                                        FLOWSEQ = 1,
                                        USERID = Model.FormInput.OwnerID,
                                        NOTIFYTIME = time,
                                        VERIFYTIME = time,
                                        VERIFYRESULT = "Y",
                                        ISCANCELED = "N"
                                    });

                                    logSeq++;
                                    #endregion

                                    #region OwnerManager
                                    db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                    {
                                        APPLYUNIQUEID = uniqueID,
                                        SEQ = logSeq,
                                        FLOWSEQ = 2,
                                        USERID = Model.FormInput.OwnerManagerID,
                                        NOTIFYTIME = time,
                                        ISCANCELED = "N"
                                    });

                                    logSeq++;

                                    verifyList.Add(Model.FormInput.OwnerManagerID);
                                    //SendVerifyMail(uniqueID, vhno, Model.FormInput.OwnerManagerID);
                                    #endregion

                                    //!CAL && !MSA 有設定PE
                                    if (!string.IsNullOrEmpty(Model.FormInput.PEID))
                                    {
                                        #region !CAL && !MSA 有設定PE

                                        #region PE
                                        db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                        {
                                            APPLYUNIQUEID = uniqueID,
                                            SEQ = logSeq,
                                            FLOWSEQ = 3,
                                            USERID = Model.FormInput.PEID,
                                            NOTIFYTIME = time,
                                            ISCANCELED = "N"
                                        });

                                        logSeq++;

                                        verifyList.Add(Model.FormInput.PEID);
                                        //SendVerifyMail(uniqueID, vhno, Model.FormInput.PEID);
                                        #endregion

                                        #endregion
                                    }

                                    #endregion
                                }
                            }
                        }
                        #endregion

                        db.SaveChanges();

                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.CalibrationApply, Resources.Resource.Success));

                        SendCreatedMail(uniqueID);

                        foreach (var verify in verifyList)
                        {
                            SendVerifyMail(uniqueID, vhno, verify);
                        }
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

        public static RequestResult GetEditFormModel(List<Models.Shared.Organization> OrganizationList, string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var apply = (from x in db.QA_CALIBRATIONAPPLY
                                 join creator in db.ACCOUNT on x.CREATORID equals creator.ID into tmpCreator
                                 from creator in tmpCreator.DefaultIfEmpty()
                                 where x.UNIQUEID == UniqueID
                                 select new
                                 {
                                     x.UNIQUEID,
                                     x.VHNO,
                                     x.STATUS,
                                     x.ORGANIZATIONUNIQUEID,
                                     x.CREATETIME,
                                     x.CREATORID,
                                     CreatorName = creator != null ? creator.NAME : "",
                                     x.FACTORYID,
                                     x.ICHITYPE,
                                     x.ICHIUNIQUEID,
                                     x.ICHIREMARK,
                                     x.CHARACTERISTICTYPE,
                                     x.SERIALNO,
                                     x.MACHINENO,
                                     x.BRAND,
                                     x.MODEL,
                                     x.SPEC,
                                     x.CAL,
                                     x.MSA,
                                     x.OWNERID,
                                     x.OWNERMANAGERID,
                                     x.PEID,
                                     x.PEMANAGERID,
                                     x.CALCYCLE,
                                     x.MSACYCLE,
                                     x.REMARK
                                 }).First();

                    var form = db.QA_CALIBRATIONFORM.FirstOrDefault(x => x.APPLYUNIQUEID == apply.UNIQUEID);

                    var model = new EditFormModel()
                    {
                        UniqueID = apply.UNIQUEID,
                        FormUniqueID = form!=null?form.UNIQUEID:"",
                        FormVHNO=form!=null?form.VHNO:"",
                        VHNO = apply.VHNO,
                        Status = new ApplyStatus(apply.STATUS),
                        Factory = OrganizationDataAccessor.GetFactory(OrganizationList, apply.ORGANIZATIONUNIQUEID),
                        Department = OrganizationDataAccessor.GetOrganizationFullDescription(apply.ORGANIZATIONUNIQUEID),
                        CreateTime = apply.CREATETIME,
                        CreatorID = apply.CreatorName,
                        CreatorName = apply.CreatorName,
                        IchiTypeSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        },
                        IchiSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        },
                        CharacteristicTypeSelectItemList = new List<SelectListItem>()
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        },
                        IchiList = db.QA_ICHI.Select(x => new IchiModel
                        {
                            UniqueID = x.UNIQUEID,
                            Type = x.TYPE,
                            Name = x.NAME
                        }).OrderBy(x => x.Name).ToList(),
                        CharacteristicTypeList = (from x in db.QA_ICHICHARACTERISTIC
                                                  join c in db.QA_CHARACTERISTIC
                                                  on x.CHARACTERISTICUNIQUEID equals c.UNIQUEID
                                                  select new
                                                  {
                                                      x.ICHIUNIQUEID,
                                                      c.TYPE
                                                  }).Distinct().Select(x => new CharacteristicTypeModel
                                                  {
                                                      IchiUniqueID = x.ICHIUNIQUEID,
                                                      Type = x.TYPE
                                                  }).OrderBy(x => x.Type).ToList(),
                        FormInput = new FormInput()
                        {
                            FactoryID = apply.FACTORYID,
                            IchiType = apply.ICHITYPE,
                            SerialNo = apply.SERIALNO,
                            MachineNo = apply.MACHINENO,
                            IchiUniqueID = apply.ICHIUNIQUEID,
                            IchiRemark = apply.ICHIREMARK,
                            CharacteristicType = apply.CHARACTERISTICTYPE,
                            Brand = apply.BRAND,
                            Model = apply.MODEL,
                            Spec = apply.SPEC,
                            CalCycle = apply.CALCYCLE,
                            MSACycle = apply.MSACYCLE,
                            CAL = apply.CAL == "Y",
                            MSA = apply.MSA == "Y",
                            OwnerID = apply.OWNERID,
                            OwnerManagerID = apply.OWNERMANAGERID,
                            PEID = apply.PEID,
                            PEManagerID = apply.PEMANAGERID,
                            Remark = apply.REMARK
                        },
                        LogList = (from log in db.QA_CALIBRATIONAPPLYFLOWLOG
                                   join user in db.ACCOUNT
                                   on log.USERID equals user.ID into tmpUser
                                   from user in tmpUser.DefaultIfEmpty()
                                   where log.APPLYUNIQUEID == apply.UNIQUEID && log.ISCANCELED == "N"
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
                                   }).OrderBy(x => x.VerifyTime).ThenBy(x => x.Seq).ToList(),
                        ItemList = (from detail in db.QA_CALIBRATIONAPPLYDETAIL
                                    join characterstic in db.QA_CHARACTERISTIC
                                    on detail.CHARACTERISTICUNIQUEID equals characterstic.UNIQUEID into tmpCharacteristic
                                    from characterstic in tmpCharacteristic.DefaultIfEmpty()
                                    join unit in db.QA_UNIT
                                    on detail.UNITUNIQUEID equals unit.UNIQUEID into tmpUnit
                                    from unit in tmpUnit.DefaultIfEmpty()
                                    join lowerUsingRangeUnit in db.QA_UNIT
                                    on detail.LOWERUSINGRANGEUNITUNIQUEID equals lowerUsingRangeUnit.UNIQUEID into tmpLowerUsingRangeUnit
                                    from lowerUsingRangeUnit in tmpLowerUsingRangeUnit.DefaultIfEmpty()
                                    join upperUsingRangeUnit in db.QA_UNIT
                                    on detail.UPPERUSINGRANGEUNITUNIQUEID equals upperUsingRangeUnit.UNIQUEID into tmpUpperUsingRangeUnit
                                    from upperUsingRangeUnit in tmpUpperUsingRangeUnit.DefaultIfEmpty()
                                    join rangeToleranceUnit in db.QA_TOLERANCEUNIT
                                    on detail.RANGETOLERANCEUNITUNIQUEID equals rangeToleranceUnit.UNIQUEID into tmpRangeToleranceUnit
                                    from rangeToleranceUnit in tmpRangeToleranceUnit.DefaultIfEmpty()
                                    where detail.APPLYUNIQUEID == apply.UNIQUEID
                                    select new DetailItem
                                    {
                                        Seq = detail.SEQ,
                                        CharacteristicUniqueID = detail.CHARACTERISTICUNIQUEID,
                                        CharacteristicDescription = characterstic != null ? characterstic.DESCRIPTION : "",
                                        CharacteristicRemark = detail.CHARACTERISTICREMARK,
                                        UnitUniqueID = detail.UNITUNIQUEID,
                                        UnitDescription = unit != null ? unit.DESCRIPTION : "",
                                        UnitRemark = detail.UNITREMARK,
                                        LowerUsingRange = detail.LOWERUSINGRANGE,
                                        LowerUsingRangeUnitUniqueID = detail.LOWERUSINGRANGEUNITUNIQUEID,
                                        LowerUsingRangeUnitDescription = lowerUsingRangeUnit != null ? lowerUsingRangeUnit.DESCRIPTION : "",
                                        LowerUsingRangeUnitRemark = detail.LOWERUSINGRANGEUNITREMARK,
                                        UpperUsingRange = detail.UPPERUSINGRANGE,
                                        UpperUsingRangeUnitUniqueID = detail.UPPERUSINGRANGEUNITUNIQUEID,
                                        UpperUsingRangeUnitDescription = upperUsingRangeUnit != null ? upperUsingRangeUnit.DESCRIPTION : "",
                                        UpperUsingRangeUnitRemark = detail.UPPERUSINGRANGEUNITREMARK,
                                        UsingRangeToleranceSymbol = detail.RANGETOLERANCESYMBOL,
                                        UsingRangeTolerance = detail.RANGETOLERANCE,
                                        UsingRangeToleranceUnitUniqueID = detail.RANGETOLERANCEUNITUNIQUEID,
                                        UsingRangeToleranceUnitDescription = rangeToleranceUnit != null ? rangeToleranceUnit.DESCRIPTION : "",
                                        UsingRangeToleranceUnitRemark = detail.RANGETOLERANCEUNITREMARK,
                                        ItemList = (from subDetail in db.QA_CALIBRATIONAPPLYSUBDETAIL
                                                    join calibrationPointUnit in db.QA_UNIT
                                                    on subDetail.CALIBRATIONPOINTUNITUNIQUEID equals calibrationPointUnit.UNIQUEID into tmpCalibrationUnit
                                                    from calibrationPointUnit in tmpCalibrationUnit.DefaultIfEmpty()
                                                    join toleranceUnit in db.QA_TOLERANCEUNIT
                                                    on subDetail.TOLERANCEUNITUNIQUEID equals toleranceUnit.UNIQUEID into tmpToleranceUnit
                                                    from toleranceUnit in tmpToleranceUnit.DefaultIfEmpty()
                                                    where subDetail.APPLYUNIQUEID == apply.UNIQUEID && subDetail.DETAILSEQ == detail.SEQ
                                                    orderby subDetail.SEQ
                                                    select new SubDetailItem
                                                    {
                                                        Seq = subDetail.SEQ,
                                                        CalibrationPoint = subDetail.CALIBRATIONPOINT,
                                                        CalibrationPointUnitUniqueID = subDetail.CALIBRATIONPOINTUNITUNIQUEID,
                                                        CalibrationPointUnitDescription = calibrationPointUnit != null ? calibrationPointUnit.DESCRIPTION : "",
                                                        ToleranceSymbol = subDetail.TOLERANCESYMBOL,
                                                        Tolerance = subDetail.TOLERANCE,
                                                        ToleranceUnitUniqueID = subDetail.TOLERANCEUNITUNIQUEID,
                                                        ToleranceUnitDescription = toleranceUnit != null ? toleranceUnit.DESCRIPTION : ""
                                                    }).ToList()
                                    }).OrderBy(x => x.Seq).ToList()
                    };

                    var ichiTypeList = db.QA_ICHI.Select(x => x.TYPE).Distinct().OrderBy(x => x).ToList();

                    foreach (var ichiType in ichiTypeList)
                    {
                        model.IchiTypeSelectItemList.Add(new SelectListItem()
                        {
                            Value = ichiType,
                            Text = ichiType
                        });

                        if (model.FormInput.IchiType == ichiType)
                        {
                            var ichiList = model.IchiList.Where(x => x.Type == ichiType).OrderBy(x => x.Name).ToList();

                            foreach (var ichi in ichiList)
                            {
                                model.IchiSelectItemList.Add(new SelectListItem()
                                {
                                    Value = ichi.UniqueID,
                                    Text = ichi.Name
                                });

                                if (model.FormInput.IchiUniqueID == ichi.UniqueID)
                                {
                                    var characteristicTypeList = model.CharacteristicTypeList.Where(x => x.IchiUniqueID == ichi.UniqueID).OrderBy(x => x.Type).ToList();

                                    foreach (var characteristicType in characteristicTypeList)
                                    {
                                        model.CharacteristicTypeSelectItemList.Add(new SelectListItem()
                                        {
                                            Value = characteristicType.Type,
                                            Text = characteristicType.Type
                                        });
                                    }
                                }
                            }

                            model.IchiSelectItemList.Add(new SelectListItem()
                            {
                                Value = Define.OTHER,
                                Text = Resources.Resource.Other
                            });

                            if (model.FormInput.IchiUniqueID == Define.OTHER)
                            {
                                var characteristicTypeList = model.CharacteristicTypeList.Where(x => x.IchiUniqueID == Define.OTHER).OrderBy(x => x.Type).ToList();

                                foreach (var characteristicType in characteristicTypeList)
                                {
                                    model.CharacteristicTypeSelectItemList.Add(new SelectListItem()
                                    {
                                        Value = characteristicType.Type,
                                        Text = characteristicType.Type
                                    });
                                }
                            }
                        }
                    }

                    model.IchiTypeSelectItemList.Add(new SelectListItem()
                    {
                        Value = "00",
                        Text = Resources.Resource.Other
                    });

                    result.ReturnData(model);
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

        /// <summary>
        /// Owner
        /// </summary>
        public static RequestResult Edit(EditFormModel Model, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
#if !DEBUG
                using (TransactionScope trans = new TransactionScope())
                {
#endif
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (Model.FormInput.CAL && (!Model.FormInput.CalCycle.HasValue || Model.FormInput.CalCycle.Value <= 0))
                    {
                        result.ReturnFailedMessage("請輸入正確的校正頻率(月)");
                    }
                    else if (Model.FormInput.MSA && (!Model.FormInput.MSACycle.HasValue || Model.FormInput.MSACycle.Value <= 0))
                    {
                        result.ReturnFailedMessage("請輸入正確的MSA頻率(月)");
                    }
                    else if (Model.FormInput.CAL && (Model.ItemList == null || Model.ItemList.Count == 0))
                    {
                        result.ReturnFailedMessage("請建立校驗資訊");
                    }
                    else
                    {
                        var apply = db.QA_CALIBRATIONAPPLY.First(x => x.UNIQUEID == Model.UniqueID);

                        bool isSerialNoOK = true;

                        if (!string.IsNullOrEmpty(Model.FormInput.SerialNo))
                        {
                            if (!string.IsNullOrEmpty(apply.EQUIPMENTUNIQUEID))
                            {
                                if (db.QA_EQUIPMENT.Any(x => x.UNIQUEID != apply.EQUIPMENTUNIQUEID && x.SERIALNO == Model.FormInput.SerialNo))
                                {
                                    isSerialNoOK = false;

                                    result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.SerialNo, Resources.Resource.Exists));
                                }
                            }
                            else
                            {
                                if (db.QA_EQUIPMENT.Any(x => x.SERIALNO == Model.FormInput.SerialNo))
                                {
                                    isSerialNoOK = false;

                                    result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.SerialNo, Resources.Resource.Exists));
                                }
                            }
                        }

                        if (isSerialNoOK)
                        {
                            var organizationUniqueID = string.Empty;

                            if (!string.IsNullOrEmpty(Model.FormInput.OwnerID))
                            {
                                var owner = db.ACCOUNT.First(x => x.ID == Model.FormInput.OwnerID);

                                organizationUniqueID = owner.ORGANIZATIONUNIQUEID;
                            }
                            else if (!string.IsNullOrEmpty(Model.FormInput.PEID))
                            {
                                var pe = db.ACCOUNT.First(x => x.ID == Model.FormInput.PEID);

                                organizationUniqueID = pe.ORGANIZATIONUNIQUEID;
                            }
                            else
                            {
                                organizationUniqueID = Account.OrganizationUniqueID;
                            }

                            apply.STATUS = "1";
                            apply.ORGANIZATIONUNIQUEID = organizationUniqueID;
                            apply.FACTORYID = Model.FormInput.FactoryID;
                            apply.ICHITYPE = Model.FormInput.IchiType;
                            apply.ICHIUNIQUEID = Model.FormInput.IchiUniqueID;
                            apply.ICHIREMARK = Model.FormInput.IchiRemark;
                            apply.CHARACTERISTICTYPE = Model.FormInput.CharacteristicType;
                            apply.SERIALNO = !string.IsNullOrEmpty(Model.FormInput.SerialNo) ? Model.FormInput.SerialNo : apply.VHNO;
                            apply.MACHINENO = !string.IsNullOrEmpty(Model.FormInput.MachineNo) ? Model.FormInput.MachineNo : "NA";
                            apply.BRAND = !string.IsNullOrEmpty(Model.FormInput.Brand) ? Model.FormInput.Brand : "NA";
                            apply.MODEL = !string.IsNullOrEmpty(Model.FormInput.Model) ? Model.FormInput.Model : "NA";
                            apply.SPEC = !string.IsNullOrEmpty(Model.FormInput.Spec) ? Model.FormInput.Spec : "NA";
                            apply.CAL = Model.FormInput.CAL ? "Y" : "N";
                            apply.MSA = Model.FormInput.MSA ? "Y" : "N";
                            apply.OWNERID = Model.FormInput.OwnerID;
                            apply.OWNERMANAGERID = Model.FormInput.OwnerManagerID;
                            apply.PEID = Model.FormInput.PEID;
                            apply.PEMANAGERID = Model.FormInput.PEManagerID;
                            apply.REMARK = !string.IsNullOrEmpty(Model.FormInput.Remark) ? Model.FormInput.Remark : "NA";
                            apply.CALCYCLE = Model.FormInput.CalCycle;
                            apply.MSACYCLE = Model.FormInput.MSACycle;

                            db.SaveChanges();

                            db.QA_CALIBRATIONAPPLYDETAIL.RemoveRange(db.QA_CALIBRATIONAPPLYDETAIL.Where(x => x.APPLYUNIQUEID == apply.UNIQUEID).ToList());
                            db.QA_CALIBRATIONAPPLYSUBDETAIL.RemoveRange(db.QA_CALIBRATIONAPPLYSUBDETAIL.Where(x => x.APPLYUNIQUEID == apply.UNIQUEID).ToList());

                            db.SaveChanges();

                            var itemSeq = 1;

                            foreach (var item in Model.ItemList.OrderBy(x => x.Characteristic))
                            {
                                db.QA_CALIBRATIONAPPLYDETAIL.Add(new QA_CALIBRATIONAPPLYDETAIL()
                                {
                                    APPLYUNIQUEID = apply.UNIQUEID,
                                    SEQ = itemSeq,
                                    CHARACTERISTICUNIQUEID = item.CharacteristicUniqueID,
                                    CHARACTERISTICREMARK = item.CharacteristicRemark,
                                    UNITUNIQUEID = item.UnitUniqueID,
                                    UNITREMARK = item.UnitRemark,
                                    LOWERUSINGRANGE = item.LowerUsingRange,
                                    LOWERUSINGRANGEUNITUNIQUEID = item.LowerUsingRangeUnitUniqueID,
                                    LOWERUSINGRANGEUNITREMARK = item.LowerUsingRangeUnitRemark,
                                    UPPERUSINGRANGE = item.UpperUsingRange,
                                    UPPERUSINGRANGEUNITUNIQUEID = item.UpperUsingRangeUnitUniqueID,
                                    UPPERUSINGRANGEUNITREMARK = item.UpperUsingRangeUnitRemark,
                                    RANGETOLERANCESYMBOL = item.UsingRangeToleranceSymbol,
                                    RANGETOLERANCE = item.UsingRangeTolerance,
                                    RANGETOLERANCEUNITUNIQUEID = item.UsingRangeToleranceUnitUniqueID,
                                    RANGETOLERANCEUNITREMARK = item.UsingRangeToleranceUnitRemark
                                });

                                var subItemSeq = 1;

                                foreach (var subitem in item.ItemList.OrderBy(x => x.CalibrationPoint))
                                {
                                    db.QA_CALIBRATIONAPPLYSUBDETAIL.Add(new QA_CALIBRATIONAPPLYSUBDETAIL()
                                    {
                                        APPLYUNIQUEID = apply.UNIQUEID,
                                        DETAILSEQ = itemSeq,
                                        SEQ = subItemSeq,
                                        CALIBRATIONPOINT = Convert.ToDecimal(subitem.CalibrationPoint),
                                        CALIBRATIONPOINTUNITUNIQUEID = subitem.CalibrationPointUnitUniqueID,
                                        TOLERANCESYMBOL = subitem.ToleranceSymbol,
                                        TOLERANCE = Convert.ToDecimal(subitem.Tolerance),
                                        TOLERANCEUNITUNIQUEID = subitem.ToleranceUnitUniqueID
                                    });

                                    subItemSeq++;
                                }

                                itemSeq++;
                            }

                            #region Flow
                            var time = DateTime.Now;

                            var log = db.QA_CALIBRATIONAPPLYFLOWLOG.FirstOrDefault(x => x.APPLYUNIQUEID == apply.UNIQUEID && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue && x.FLOWSEQ == 0);

                            if (log != null)
                            {
                                log.VERIFYRESULT = "Y";
                                log.USERID = Account.ID;
                                log.VERIFYTIME = time;

                                db.SaveChanges();
                            }

                            log = db.QA_CALIBRATIONAPPLYFLOWLOG.FirstOrDefault(x => x.APPLYUNIQUEID == apply.UNIQUEID && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue && x.FLOWSEQ == 1);

                            if (log != null)
                            {
                                log.VERIFYRESULT = "Y";
                                log.USERID = Account.ID;
                                log.VERIFYTIME = time;

                                db.SaveChanges();
                            }

                            var logSeq = db.QA_CALIBRATIONAPPLYFLOWLOG.Where(x => x.APPLYUNIQUEID == apply.UNIQUEID).Max(x => x.SEQ) + 1;

                            //CAL
                            if (Model.FormInput.CAL)
                            {
                                //CAL Creator != Owner
                                if (Account.ID != Model.FormInput.OwnerID)
                                {
                                    #region CAL Creator != Owner

                                    #region Owner
                                    db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                    {
                                        APPLYUNIQUEID = apply.UNIQUEID,
                                        SEQ = logSeq,
                                        FLOWSEQ = 1,
                                        USERID = Model.FormInput.OwnerID,
                                        NOTIFYTIME = time,
                                        ISCANCELED = "N"
                                    });

                                    logSeq++;

                                    SendVerifyMail(apply.UNIQUEID, apply.VHNO, Model.FormInput.OwnerID);
                                    #endregion

                                    #endregion
                                }
                                //CAL Creator == Owner
                                else
                                {
                                    #region CAL Creator = Owner

                                    #region Owner
                                    db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                    {
                                        APPLYUNIQUEID = apply.UNIQUEID,
                                        SEQ = logSeq,
                                        FLOWSEQ = 1,
                                        USERID = Model.FormInput.OwnerID,
                                        NOTIFYTIME = time,
                                        VERIFYTIME = time,
                                        VERIFYRESULT = "Y",
                                        ISCANCELED = "N"
                                    });

                                    logSeq++;
                                    #endregion

                                    #region OwnerManager
                                    db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                    {
                                        APPLYUNIQUEID = apply.UNIQUEID,
                                        SEQ = logSeq,
                                        FLOWSEQ = 2,
                                        USERID = Model.FormInput.OwnerManagerID,
                                        NOTIFYTIME = time,
                                        ISCANCELED = "N"
                                    });

                                    logSeq++;

                                    SendVerifyMail(apply.UNIQUEID, apply.VHNO, Model.FormInput.OwnerManagerID);
                                    #endregion

                                    #region PE
                                    db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                    {
                                        APPLYUNIQUEID = apply.UNIQUEID,
                                        SEQ = logSeq,
                                        FLOWSEQ = 3,
                                        USERID = Model.FormInput.PEID,
                                        NOTIFYTIME = time,
                                        ISCANCELED = "N"
                                    });

                                    logSeq++;

                                    SendVerifyMail(apply.UNIQUEID, apply.VHNO, Model.FormInput.PEID);
                                    #endregion

                                    #endregion
                                }
                            }
                            //!CAL
                            else
                            {
                                //!CAL && MSA
                                if (Model.FormInput.MSA)
                                {
                                    //!CAL && MSA 有設定Owner
                                    if (!string.IsNullOrEmpty(Model.FormInput.OwnerID))
                                    {
                                        #region !CAL && MSA 有設定 Owner

                                        //Creator != Owner
                                        if (Account.ID != Model.FormInput.OwnerID)
                                        {
                                            #region !CAL && MSA 有設定Owner Creator != Owner

                                            #region Owner
                                            db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                            {
                                                APPLYUNIQUEID = apply.UNIQUEID,
                                                SEQ = logSeq,
                                                FLOWSEQ = 1,
                                                USERID = Model.FormInput.OwnerID,
                                                NOTIFYTIME = time,
                                                ISCANCELED = "N"
                                            });

                                            logSeq++;

                                            SendVerifyMail(apply.UNIQUEID, apply.VHNO, Model.FormInput.OwnerID);
                                            #endregion

                                            #endregion
                                        }
                                        //Creator == Owner
                                        else
                                        {
                                            #region !CAL && MSA 有設定Owner Creator = Owner

                                            #region Owner
                                            db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                            {
                                                APPLYUNIQUEID = apply.UNIQUEID,
                                                SEQ = logSeq,
                                                FLOWSEQ = 1,
                                                USERID = Model.FormInput.OwnerID,
                                                NOTIFYTIME = time,
                                                VERIFYTIME = time,
                                                VERIFYRESULT = "Y",
                                                ISCANCELED = "N"
                                            });

                                            logSeq++;
                                            #endregion

                                            #region OwnerManager
                                            db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                            {
                                                APPLYUNIQUEID = apply.UNIQUEID,
                                                SEQ = logSeq,
                                                FLOWSEQ = 2,
                                                USERID = Model.FormInput.OwnerManagerID,
                                                NOTIFYTIME = time,
                                                ISCANCELED = "N"
                                            });

                                            logSeq++;

                                            SendVerifyMail(apply.UNIQUEID, apply.VHNO, Model.FormInput.OwnerManagerID);
                                            #endregion

                                            #region PE
                                            db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                            {
                                                APPLYUNIQUEID = apply.UNIQUEID,
                                                SEQ = logSeq,
                                                FLOWSEQ = 3,
                                                USERID = Model.FormInput.PEID,
                                                NOTIFYTIME = time,
                                                ISCANCELED = "N"
                                            });

                                            logSeq++;

                                            SendVerifyMail(apply.UNIQUEID, apply.VHNO, Model.FormInput.PEID);
                                            #endregion

                                            #endregion
                                        }

                                        #endregion
                                    }
                                    //!CAL && MSA 沒有設定Owner
                                    else
                                    {
                                        #region !CAL && MSA 沒有設定Owner

                                        #region PE
                                        db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                        {
                                            APPLYUNIQUEID = apply.UNIQUEID,
                                            SEQ = logSeq,
                                            FLOWSEQ = 3,
                                            USERID = Model.FormInput.PEID,
                                            NOTIFYTIME = time,
                                            ISCANCELED = "N"
                                        });

                                        logSeq++;

                                        SendVerifyMail(apply.UNIQUEID, apply.VHNO, Model.FormInput.PEID);
                                        #endregion

                                        #endregion
                                    }
                                }
                                //!CAL && !MSA
                                else
                                {
                                    //!CAL && !MSA Creator != Owner
                                    if (Account.ID != Model.FormInput.OwnerID)
                                    {
                                        #region !CAL && !MSA Creator != Owner

                                        #region Owner
                                        db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                        {
                                            APPLYUNIQUEID = apply.UNIQUEID,
                                            SEQ = logSeq,
                                            FLOWSEQ = 1,
                                            USERID = Model.FormInput.OwnerID,
                                            NOTIFYTIME = time,
                                            ISCANCELED = "N"
                                        });

                                        logSeq++;

                                        SendVerifyMail(apply.UNIQUEID, apply.VHNO, Model.FormInput.OwnerID);
                                        #endregion

                                        #endregion
                                    }
                                    //!CAL && !MSA Creator == Owner
                                    else
                                    {
                                        #region !CAL && !MSA Creator == Owner

                                        #region Owner
                                        db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                        {
                                            APPLYUNIQUEID = apply.UNIQUEID,
                                            SEQ = logSeq,
                                            FLOWSEQ = 1,
                                            USERID = Model.FormInput.OwnerID,
                                            NOTIFYTIME = time,
                                            VERIFYTIME = time,
                                            VERIFYRESULT = "Y",
                                            ISCANCELED = "N"
                                        });

                                        logSeq++;
                                        #endregion

                                        #region OwnerManager
                                        db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                        {
                                            APPLYUNIQUEID = apply.UNIQUEID,
                                            SEQ = logSeq,
                                            FLOWSEQ = 2,
                                            USERID = Model.FormInput.OwnerManagerID,
                                            NOTIFYTIME = time,
                                            ISCANCELED = "N"
                                        });

                                        logSeq++;

                                        SendVerifyMail(apply.UNIQUEID, apply.VHNO, Model.FormInput.OwnerManagerID);
                                        #endregion

                                        //!CAL && !MSA 有設定PE
                                        if (!string.IsNullOrEmpty(Model.FormInput.PEID))
                                        {
                                            #region !CAL && !MSA 有設定PE

                                            #region PE
                                            db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                            {
                                                APPLYUNIQUEID = apply.UNIQUEID,
                                                SEQ = logSeq,
                                                FLOWSEQ = 3,
                                                USERID = Model.FormInput.PEID,
                                                NOTIFYTIME = time,
                                                ISCANCELED = "N"
                                            });

                                            logSeq++;

                                            SendVerifyMail(apply.UNIQUEID, apply.VHNO, Model.FormInput.PEID);
                                            #endregion

                                            #endregion
                                        }

                                        #endregion
                                    }
                                }
                            }
                            #endregion

                            db.SaveChanges();

                            result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.CalibrationApply, Resources.Resource.Success));
                        }
                    }
                }
#if !DEBUG
                    trans.Complete();
                }
#endif
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetCopyFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var apply = (from x in db.QA_CALIBRATIONAPPLY
                                 join creator in db.ACCOUNT on x.CREATORID equals creator.ID into tmpCreator
                                 from creator in tmpCreator.DefaultIfEmpty()
                                 where x.UNIQUEID == UniqueID
                                 select new
                                 {
                                     x.UNIQUEID,
                                     x.VHNO,
                                     x.STATUS,
                                     x.ORGANIZATIONUNIQUEID,
                                     x.CREATETIME,
                                     x.CREATORID,
                                     CreateUserName = creator != null ? creator.NAME : "",
                                     x.FACTORYID,
                                     x.ICHITYPE,
                                     x.ICHIUNIQUEID,
                                     x.ICHIREMARK,
                                     x.CHARACTERISTICTYPE,
                                     x.SERIALNO,
                                     x.MACHINENO,
                                     x.BRAND,
                                     x.MODEL,
                                     x.SPEC,
                                     x.CAL,
                                     x.MSA,
                                     x.OWNERID,
                                     x.OWNERMANAGERID,
                                     x.PEID,
                                     x.PEMANAGERID,
                                     x.CALCYCLE,
                                     x.MSACYCLE,
                                     x.REMARK
                                 }).First();

                    var model = new CreateFormModel()
                    {
                        IchiTypeSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        },
                        IchiSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        },
                        CharacteristicTypeSelectItemList = new List<SelectListItem>()
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        },
                        IchiList = db.QA_ICHI.Select(x => new IchiModel
                        {
                            UniqueID = x.UNIQUEID,
                            Type = x.TYPE,
                            Name = x.NAME
                        }).OrderBy(x => x.Name).ToList(),
                        CharacteristicTypeList = (from x in db.QA_ICHICHARACTERISTIC
                                                  join c in db.QA_CHARACTERISTIC
                                                  on x.CHARACTERISTICUNIQUEID equals c.UNIQUEID
                                                  select new
                                                  {
                                                      x.ICHIUNIQUEID,
                                                      c.TYPE
                                                  }).Distinct().Select(x => new CharacteristicTypeModel
                                                  {
                                                      IchiUniqueID = x.ICHIUNIQUEID,
                                                      Type = x.TYPE
                                                  }).OrderBy(x => x.Type).ToList(),
                        FormInput = new FormInput()
                        {
                            FactoryID = apply.FACTORYID,
                            IchiType = apply.ICHITYPE,
                            IchiUniqueID = apply.ICHIUNIQUEID,
                            IchiRemark = apply.ICHIREMARK,
                            CharacteristicType = apply.CHARACTERISTICTYPE,
                            Brand = apply.BRAND,
                            Model = apply.MODEL,
                            Spec =apply.SPEC,
                            CalCycle = apply.CALCYCLE,
                            MSACycle=apply.MSACYCLE,
                            CAL = apply.CAL == "Y",
                            MSA = apply.MSA == "Y",
                            OwnerID = apply.OWNERID,
                            OwnerManagerID = apply.OWNERMANAGERID,
                            PEID = apply.PEID,
                            PEManagerID = apply.PEMANAGERID,
                            Remark = apply.REMARK
                        },
                        ItemList = (from detail in db.QA_CALIBRATIONAPPLYDETAIL
                                    join characterstic in db.QA_CHARACTERISTIC
                                    on detail.CHARACTERISTICUNIQUEID equals characterstic.UNIQUEID into tmpCharacteristic
                                    from characterstic in tmpCharacteristic.DefaultIfEmpty()
                                    join unit in db.QA_UNIT
                                    on detail.UNITUNIQUEID equals unit.UNIQUEID into tmpUnit
                                    from unit in tmpUnit.DefaultIfEmpty()
                                    join lowerUsingRangeUnit in db.QA_UNIT
                                    on detail.LOWERUSINGRANGEUNITUNIQUEID equals lowerUsingRangeUnit.UNIQUEID into tmpLowerUsingRangeUnit
                                    from lowerUsingRangeUnit in tmpLowerUsingRangeUnit.DefaultIfEmpty()
                                    join upperUsingRangeUnit in db.QA_UNIT
                                    on detail.UPPERUSINGRANGEUNITUNIQUEID equals upperUsingRangeUnit.UNIQUEID into tmpUpperUsingRangeUnit
                                    from upperUsingRangeUnit in tmpUpperUsingRangeUnit.DefaultIfEmpty()
                                    join rangeToleranceUnit in db.QA_TOLERANCEUNIT
                                    on detail.RANGETOLERANCEUNITUNIQUEID equals rangeToleranceUnit.UNIQUEID into tmpRangeToleranceUnit
                                    from rangeToleranceUnit in tmpRangeToleranceUnit.DefaultIfEmpty()
                                    where detail.APPLYUNIQUEID == apply.UNIQUEID
                                    select new DetailItem
                                    {
                                        Seq = detail.SEQ,
                                        CharacteristicUniqueID = detail.CHARACTERISTICUNIQUEID,
                                        CharacteristicDescription = characterstic != null ? characterstic.DESCRIPTION : "",
                                        CharacteristicRemark = detail.CHARACTERISTICREMARK,
                                        UnitUniqueID = detail.UNITUNIQUEID,
                                        UnitDescription = unit != null ? unit.DESCRIPTION : "",
                                        UnitRemark = detail.UNITREMARK,
                                        LowerUsingRange = detail.LOWERUSINGRANGE,
                                        LowerUsingRangeUnitUniqueID = detail.LOWERUSINGRANGEUNITUNIQUEID,
                                        LowerUsingRangeUnitDescription = lowerUsingRangeUnit != null ? lowerUsingRangeUnit.DESCRIPTION : "",
                                        LowerUsingRangeUnitRemark = detail.LOWERUSINGRANGEUNITREMARK,
                                        UpperUsingRange = detail.UPPERUSINGRANGE,
                                        UpperUsingRangeUnitUniqueID = detail.UPPERUSINGRANGEUNITUNIQUEID,
                                        UpperUsingRangeUnitDescription = upperUsingRangeUnit != null ? upperUsingRangeUnit.DESCRIPTION : "",
                                        UpperUsingRangeUnitRemark = detail.UPPERUSINGRANGEUNITREMARK,
                                        UsingRangeToleranceSymbol = detail.RANGETOLERANCESYMBOL,
                                        UsingRangeTolerance = detail.RANGETOLERANCE,
                                        UsingRangeToleranceUnitUniqueID = detail.RANGETOLERANCEUNITUNIQUEID,
                                        UsingRangeToleranceUnitDescription = rangeToleranceUnit != null ? rangeToleranceUnit.DESCRIPTION : "",
                                        UsingRangeToleranceUnitRemark = detail.RANGETOLERANCEUNITREMARK,
                                        ItemList = (from subDetail in db.QA_CALIBRATIONAPPLYSUBDETAIL
                                                    join calibrationPointUnit in db.QA_UNIT
                                                    on subDetail.CALIBRATIONPOINTUNITUNIQUEID equals calibrationPointUnit.UNIQUEID into tmpCalibrationUnit
                                                    from calibrationPointUnit in tmpCalibrationUnit.DefaultIfEmpty()
                                                    join toleranceUnit in db.QA_TOLERANCEUNIT
                                                    on subDetail.TOLERANCEUNITUNIQUEID equals toleranceUnit.UNIQUEID into tmpToleranceUnit
                                                    from toleranceUnit in tmpToleranceUnit.DefaultIfEmpty()
                                                    where subDetail.APPLYUNIQUEID == apply.UNIQUEID && subDetail.DETAILSEQ == detail.SEQ
                                                    orderby subDetail.SEQ
                                                    select new SubDetailItem
                                                    {
                                                        Seq = subDetail.SEQ,
                                                        CalibrationPoint = subDetail.CALIBRATIONPOINT,
                                                        CalibrationPointUnitUniqueID = subDetail.CALIBRATIONPOINTUNITUNIQUEID,
                                                        CalibrationPointUnitDescription = calibrationPointUnit != null ? calibrationPointUnit.DESCRIPTION : "",
                                                        ToleranceSymbol = subDetail.TOLERANCESYMBOL,
                                                        Tolerance = subDetail.TOLERANCE,
                                                        ToleranceUnitUniqueID = subDetail.TOLERANCEUNITUNIQUEID,
                                                        ToleranceUnitDescription = toleranceUnit != null ? toleranceUnit.DESCRIPTION : ""
                                                    }).ToList()
                                    }).OrderBy(x => x.Seq).ToList()
                    };

                    var ichiTypeList = db.QA_ICHI.Select(x => x.TYPE).Distinct().OrderBy(x => x).ToList();

                    foreach (var ichiType in ichiTypeList)
                    {
                        model.IchiTypeSelectItemList.Add(new SelectListItem()
                        {
                            Value = ichiType,
                            Text = ichiType
                        });

                        if (model.FormInput.IchiType == ichiType)
                        {
                            var ichiList = model.IchiList.Where(x => x.Type == ichiType).OrderBy(x => x.Name).ToList();

                            foreach (var ichi in ichiList)
                            {
                                model.IchiSelectItemList.Add(new SelectListItem()
                                {
                                    Value = ichi.UniqueID,
                                    Text = ichi.Name
                                });

                                if (model.FormInput.IchiUniqueID == ichi.UniqueID)
                                {
                                    var characteristicTypeList = model.CharacteristicTypeList.Where(x => x.IchiUniqueID == ichi.UniqueID).OrderBy(x => x.Type).ToList();

                                    foreach (var characteristicType in characteristicTypeList)
                                    {
                                        model.CharacteristicTypeSelectItemList.Add(new SelectListItem()
                                        {
                                            Value = characteristicType.Type,
                                            Text = characteristicType.Type
                                        });
                                    }
                                }
                            }

                            model.IchiSelectItemList.Add(new SelectListItem()
                            {
                                Value = Define.OTHER,
                                Text = Resources.Resource.Other
                            });
                        }
                    }

                    model.IchiTypeSelectItemList.Add(new SelectListItem()
                    {
                        Value = Define.OTHER,
                        Text = Resources.Resource.Other
                    });

                    result.ReturnData(model);
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

        public static RequestResult GetOwnerFormModel(List<Models.Shared.Organization> OrganizationList, string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    result.ReturnData(new OwnerFormModel()
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
                            MSAType = "1",
                            MSASubType = "1"
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
                    model = new QAFormModel()
                    {
                        UniqueID = UniqueID,
                        FormViewModel = GetFormViewModel(OrganizationList, UniqueID),
                        CalibratorSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        },
                        VerifyCommentList = db.QA_VERIFYCOMMENT.OrderBy(x => x.DESCRIPTION).ToList().Select(x => new VerifyCommentModel()
                        {
                            Description = x.DESCRIPTION,
                            UniqueID = x.UNIQUEID
                        }).ToList(),
                        FormInput = new QAFormInput()
                        {
                            EstCalibrateDateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today.AddDays(7)),
                            CalibrateType = "IL",
                            CalibrateUnit = "L",
                            CaseType = "G",
                            EstMSADateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today.AddMonths(3))
                        }
                    };

                    var calibratorList = (from x in db.USERAUTHGROUP
                                          join u in db.ACCOUNT
                                          on x.USERID equals u.ID
                                          where x.AUTHGROUPID == "QA"
                                          select u).ToList();

                    foreach (var c in calibratorList)
                    {
                        var ename = string.Empty;

                        if (!string.IsNullOrEmpty(c.EMAIL))
                        {
                            try
                            {
                                ename = c.EMAIL.Substring(0, c.EMAIL.IndexOf('@'));
                            }
                            catch
                            {
                                ename = string.Empty;
                            }
                        }

                        var display = string.Empty;

                        if (!string.IsNullOrEmpty(ename))
                        {
                            display = string.Format("{0}/{1}/{2}", c.ID, c.NAME, ename);
                        }
                        else
                        {
                            display = string.Format("{0}/{1}", c.ID, c.NAME);
                        }

                        model.CalibratorSelectItemList.Add(new SelectListItem()
                        {
                            Value = c.ID,
                            Text = display
                        });
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

        /// <summary>
        /// Owner/OwnerManager/PEManager
        /// </summary>
        public static RequestResult Approve(string UniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var apply = db.QA_CALIBRATIONAPPLY.First(x => x.UNIQUEID == UniqueID);

                    var log = db.QA_CALIBRATIONAPPLYFLOWLOG.Where(x => x.APPLYUNIQUEID == apply.UNIQUEID && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue && x.USERID == Account.ID).OrderBy(x => x.SEQ).FirstOrDefault();

                    if (log != null)
                    {
                        var verifyList = new List<string>();

                        log.VERIFYTIME = DateTime.Now;
                        log.VERIFYRESULT = "Y";

                        var logSeq = db.QA_CALIBRATIONAPPLYFLOWLOG.Where(x => x.APPLYUNIQUEID == apply.UNIQUEID).Max(x => x.SEQ) + 1;

                        //Owner
                        if (log.FLOWSEQ == 1)
                        {
                            db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                            {
                                APPLYUNIQUEID = apply.UNIQUEID,
                                SEQ = logSeq,
                                FLOWSEQ = 2,
                                USERID = apply.OWNERMANAGERID,
                                NOTIFYTIME = DateTime.Now,
                                ISCANCELED = "N"
                            });

                            logSeq++;

                            verifyList.Add(apply.OWNERMANAGERID);

                            if (!string.IsNullOrEmpty(apply.PEID))
                            {
                                db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                {
                                    APPLYUNIQUEID = apply.UNIQUEID,
                                    SEQ = logSeq,
                                    FLOWSEQ = 3,
                                    USERID = apply.PEID,
                                    NOTIFYTIME = DateTime.Now,
                                    ISCANCELED = "N"
                                });

                                logSeq++;

                                verifyList.Add(apply.PEID);
                            }
                        }
                        //OwnerManager
                        else if (log.FLOWSEQ == 2)
                        {
                            if (!db.QA_CALIBRATIONAPPLYFLOWLOG.Any(x => x.APPLYUNIQUEID == apply.UNIQUEID && x.SEQ != log.SEQ && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue))
                            {
                                db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                {
                                    APPLYUNIQUEID = apply.UNIQUEID,
                                    SEQ = logSeq,
                                    FLOWSEQ = 5,
                                    NOTIFYTIME = DateTime.Now,
                                    ISCANCELED = "N"
                                });

                                logSeq++;

                                verifyList.Add("QA");
                            }
                        }
                        //PEManager
                        else if (log.FLOWSEQ == 4)
                        {
                            if (!db.QA_CALIBRATIONAPPLYFLOWLOG.Any(x => x.APPLYUNIQUEID == apply.UNIQUEID && x.SEQ != log.SEQ && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue))
                            {
                                db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                {
                                    APPLYUNIQUEID = apply.UNIQUEID,
                                    SEQ = logSeq,
                                    FLOWSEQ = 5,
                                    NOTIFYTIME = DateTime.Now,
                                    ISCANCELED = "N"
                                });

                                logSeq++;

                                verifyList.Add("QA");
                            }
                        }

                        db.SaveChanges();

                        foreach (var verify in verifyList)
                        {
                            if (verify == "QA")
                            {
                                SendVerifyMail(apply.UNIQUEID, apply.VHNO);
                            }
                            else
                            {
                                SendVerifyMail(apply.UNIQUEID, apply.VHNO, verify);
                            }
                        }
                    }
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

        /// <summary>
        /// Owner/OwnerManager/PE/PEManager
        /// </summary>
        public static RequestResult Reject(string UniqueID, string Comment, Account Account)
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
                        var apply = db.QA_CALIBRATIONAPPLY.First(x => x.UNIQUEID == UniqueID);                        

                        var log = db.QA_CALIBRATIONAPPLYFLOWLOG.Where(x => x.APPLYUNIQUEID == apply.UNIQUEID && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue && x.USERID == Account.ID).OrderBy(x => x.SEQ).FirstOrDefault();

                        if (log != null)
                        {
                            log.VERIFYTIME = DateTime.Now;
                            log.VERIFYRESULT = "N";
                            log.VERIFYCOMMENT = Comment;

                            db.SaveChanges();

                            var logSeq = db.QA_CALIBRATIONAPPLYFLOWLOG.Where(x => x.APPLYUNIQUEID == apply.UNIQUEID).Max(x => x.SEQ) + 1;

                            //Owner
                            if (log.FLOWSEQ == 1)
                            {
                                apply.STATUS = "2";

                                var logList = db.QA_CALIBRATIONAPPLYFLOWLOG.Where(x => x.APPLYUNIQUEID == UniqueID && x.SEQ != log.SEQ && !x.VERIFYTIME.HasValue).ToList();

                                foreach (var l in logList)
                                {
                                    l.ISCANCELED = "Y";
                                }

                                db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                {
                                    APPLYUNIQUEID = apply.UNIQUEID,
                                    SEQ = logSeq,
                                    FLOWSEQ = 0,
                                    NOTIFYTIME = DateTime.Now,
                                    USERID = apply.CREATORID,
                                    ISCANCELED = "N"
                                });

                                db.SaveChanges();
                            }
                            //Owner Manager
                            else if (log.FLOWSEQ == 2)
                            {
                                apply.STATUS = "2";

                                var logList = db.QA_CALIBRATIONAPPLYFLOWLOG.Where(x => x.APPLYUNIQUEID == UniqueID && x.SEQ != log.SEQ && !x.VERIFYTIME.HasValue).ToList();

                                foreach (var l in logList)
                                {
                                    l.ISCANCELED = "Y";
                                }

                                db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                {
                                    APPLYUNIQUEID = apply.UNIQUEID,
                                    SEQ = logSeq,
                                    FLOWSEQ = 1,
                                    NOTIFYTIME = DateTime.Now,
                                    USERID = apply.OWNERID,
                                    ISCANCELED = "N"
                                });

                                db.SaveChanges();
                            }
                            //PE
                            else if (log.FLOWSEQ == 3)
                            {
                                apply.STATUS = "2";

                                var logList = db.QA_CALIBRATIONAPPLYFLOWLOG.Where(x => x.APPLYUNIQUEID == UniqueID && x.SEQ != log.SEQ && !x.VERIFYTIME.HasValue).ToList();

                                foreach (var l in logList)
                                {
                                    l.ISCANCELED = "Y";
                                }

                                db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                {
                                    APPLYUNIQUEID = apply.UNIQUEID,
                                    SEQ = logSeq,
                                    FLOWSEQ = 1,
                                    NOTIFYTIME = DateTime.Now,
                                    USERID = apply.OWNERID,
                                    ISCANCELED = "N"
                                });

                                db.SaveChanges();
                            }
                            //PE Mananger
                            else if (log.FLOWSEQ == 4)
                            {
                                db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                {
                                    APPLYUNIQUEID = apply.UNIQUEID,
                                    SEQ = logSeq,
                                    FLOWSEQ = 3,
                                    NOTIFYTIME = DateTime.Now,
                                    USERID = apply.PEID,
                                    ISCANCELED = "N"
                                });

                                db.SaveChanges();
                            }
                        }
                        
                    #if !DEBUG
                        trans.Complete();
#endif

                        SendRejectMail(apply.UNIQUEID, apply.VHNO,new List<string>() { apply.CREATORID, apply.OWNERID });
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

        public static RequestResult ChangePE(PEFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var apply = db.QA_CALIBRATIONAPPLY.First(x => x.UNIQUEID == Model.UniqueID);

                    var log = db.QA_CALIBRATIONAPPLYFLOWLOG.First(x => x.APPLYUNIQUEID == apply.UNIQUEID && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue && x.FLOWSEQ == 3);

                    log.VERIFYTIME = DateTime.Now;
                    log.VERIFYRESULT = "Y";
                    log.VERIFYCOMMENT = Model.FormInput.Comment;

                    var logSeq = db.QA_CALIBRATIONAPPLYFLOWLOG.Where(x => x.APPLYUNIQUEID == apply.UNIQUEID).Max(x => x.SEQ) + 1;

                    apply.PEID = Model.FormInput.PEID;
                    apply.PEMANAGERID = Model.FormInput.PEManagerID;

                    db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                    {
                        APPLYUNIQUEID = apply.UNIQUEID,
                        SEQ = logSeq,
                        FLOWSEQ = 3,
                        USERID = Model.FormInput.PEID,
                        NOTIFYTIME = DateTime.Now,
                        ISCANCELED = "N"
                    });

                    SendVerifyMail(apply.UNIQUEID, apply.VHNO,Model.FormInput.PEID);

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

        /// <summary>
        /// PE
        /// </summary>
        public static RequestResult PEApprove(PEFormModel Model)
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


                        var apply = db.QA_CALIBRATIONAPPLY.First(x => x.UNIQUEID == Model.UniqueID);

                        apply.MSATYPE = Model.FormInput.MSAType;

                        if (Model.FormInput.MSAType == "1")
                        {
                            apply.MSASUBTYPE = Model.FormInput.MSASubType;
                        }
                        else
                        {
                            apply.MSASUBTYPE = null;
                        }

                        apply.MSASTATIONUNIQUEID = Model.FormInput.MSAStationUniqueID;
                        apply.MSASTATIONREMARK = Model.FormInput.MSAStationRemark;
                        apply.MSAICHIUNIQUEID = Model.FormInput.MSAIchiUniqueID;
                        apply.MSAICHIREMARK = Model.FormInput.MSAIchiRemark;

                        db.SaveChanges();

                        db.QA_CALIBRATIONAPPLYMSADETAIL.RemoveRange(db.QA_CALIBRATIONAPPLYMSADETAIL.Where(x => x.APPLYUNIQUEID == apply.UNIQUEID).ToList());

                        db.SaveChanges();

                        int msaSeq = 1;

                        foreach (var s in Model.FormInput.MSACharacteristicList)
                        {
                            string[] temp = s.Split(Define.Seperators, StringSplitOptions.None);

                            string characteristicUniqueID = temp[0];
                            string characteristicRemark = temp[1];
                            string unitUniqueID = temp[2];
                            string unitRemark = temp[3];
                            string lowerRange = temp[4];
                            string upperRange = temp[5];

                            db.QA_CALIBRATIONAPPLYMSADETAIL.Add(new QA_CALIBRATIONAPPLYMSADETAIL()
                            {
                                APPLYUNIQUEID = apply.UNIQUEID,
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

                        var log = db.QA_CALIBRATIONAPPLYFLOWLOG.FirstOrDefault(x => x.APPLYUNIQUEID == apply.UNIQUEID && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue && x.FLOWSEQ == 3);

                        if (log != null)
                        {
                            log.VERIFYTIME = DateTime.Now;
                            log.VERIFYRESULT = "Y";
                        }

                        db.SaveChanges();

                        var logSeq = db.QA_CALIBRATIONAPPLYFLOWLOG.Where(x => x.APPLYUNIQUEID == apply.UNIQUEID).Max(x => x.SEQ) + 1;

                        db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                        {
                            APPLYUNIQUEID = apply.UNIQUEID,
                            SEQ = logSeq,
                            FLOWSEQ = 4,
                            USERID = apply.PEMANAGERID,
                            NOTIFYTIME = DateTime.Now,
                            ISCANCELED = "N"
                        });

                        db.SaveChanges();

                        SendVerifyMail(apply.UNIQUEID, apply.VHNO,apply.PEMANAGERID);
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

        /// <summary>
        /// QA
        /// </summary>
        public static RequestResult QAApprove(List<Models.Shared.Organization> OrganizationList, QAFormModel Model, Account Account)
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
                    var apply = db.QA_CALIBRATIONAPPLY.First(x => x.UNIQUEID == Model.UniqueID);

                    apply.STATUS = "3";

                    if (apply.CAL == "Y")
                    {
                        apply.CALTYPE = Model.FormInput.CalibrateType;

                        if (Model.FormInput.CalibrateType != "IL")
                        {
                            apply.CALUNIT = Model.FormInput.CalibrateUnit;
                        }
                        else
                        {
                            apply.CALUNIT = "L";
                        }

                        apply.CASETYPE = Model.FormInput.CaseType;
                        apply.CALRESPONSORID = Model.FormInput.CalibratorID;
                        apply.ESTCALDATE = Model.FormInput.EstCalibrateDate;
                    }
                    else
                    {
                        apply.CALTYPE = null;
                        apply.CALUNIT = null;
                        apply.CASETYPE = null;
                        apply.CALRESPONSORID = null;
                        apply.ESTCALDATE = null;

                        if (apply.MSA == "Y")
                        {
                            apply.MSACALNO = Model.FormInput.MSACalNo;
                            apply.ESTMSADATE = Model.FormInput.EstMSADate;
                        }
                    }

                    var equipmentUniqueID = apply.EQUIPMENTUNIQUEID;

                    var calNo = string.Empty;

                    if (apply.CAL == "Y")
                    {
                        if (apply.ICHITYPE != Define.OTHER)
                        {
                            var calNoPreFix = string.Format("{0}{1}-", apply.FACTORYID, apply.ICHITYPE);

                            var calNoseq = 1;

                            calNo = string.Format("{0}{1}", calNoPreFix, calNoseq.ToString().PadLeft(4, '0'));

                            var calNoQuery = db.QA_EQUIPMENT.Where(x => !string.IsNullOrEmpty(x.CALNO) && x.CALNO.StartsWith(calNoPreFix)).Select(x => x.CALNO).ToList();

                            if (calNoQuery.Count > 0)
                            {
                                while (calNoseq < 10000)
                                {
                                    var q1 = calNoQuery.FirstOrDefault(x => x == calNo);

                                    if (q1 != null)
                                    {
                                        calNoseq++;

                                        calNo = string.Format("{0}{1}", calNoPreFix, calNoseq.ToString().PadLeft(4, '0'));
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }

                            if (apply.MSA == "Y")
                            {
                                apply.MSACALNO = string.Format("MSA({0})", calNo);
                            }
                        }
                    }

                    //新申請
                    if (string.IsNullOrEmpty(equipmentUniqueID))
                    {
                        equipmentUniqueID = Guid.NewGuid().ToString();

                        apply.EQUIPMENTUNIQUEID = equipmentUniqueID;

                        var equipment = new QA_EQUIPMENT()
                        {
                            UNIQUEID = equipmentUniqueID,
                            ORGANIZATIONUNIQUEID = apply.ORGANIZATIONUNIQUEID,
                            STATUS = "1",
                            MACHINENO = apply.MACHINENO,
                            SERIALNO = apply.SERIALNO,
                            ICHIUNIQUEID = apply.ICHIUNIQUEID,
                            ICHIREMARK = apply.ICHIREMARK,
                            FACTORYID = apply.FACTORYID,
                            ICHITYPE = apply.ICHITYPE,
                            REMARK = apply.REMARK,
                            CHARACTERISTICTYPE = apply.CHARACTERISTICTYPE,
                            SPEC = apply.SPEC,
                            BRAND = apply.BRAND,
                            MODEL = apply.MODEL,
                            CALNO = apply.CAL == "Y" ? calNo : string.Empty,
                            CAL = apply.CAL,
                            MSA = apply.MSA,
                            MSACYCLE = apply.MSACYCLE,
                            CALCYCLE = apply.CALCYCLE,
                            CYCLE = 0,
                            OWNERID = apply.OWNERID,
                            OWNERMANAGERID = apply.OWNERMANAGERID,
                            PEID = apply.PEID,
                            PEMANAGERID = apply.PEMANAGERID,
                            CALTYPE = apply.CALTYPE,
                            CALUNIT = apply.CALUNIT,
                            CASETYPE = apply.CASETYPE,
                            MSACALNO = apply.MSA == "Y" ? apply.MSACALNO : string.Empty,
                            MSAICHIREMARK = apply.MSAICHIREMARK,
                            MSAICHIUNIQUEID = apply.MSAICHIUNIQUEID,
                            MSASTATIONREMARK = apply.MSASTATIONREMARK,
                            MSASTATIONUNIQUEID = apply.MSASTATIONUNIQUEID,
                            MSATYPE = apply.MSATYPE,
                            MSASUBTYPE = apply.MSASUBTYPE
                        };

                        if (apply.CAL == "Y")
                        {
                            equipment.NEXTCALDATE = Model.FormInput.EstCalibrateDate;
                        }
                        else
                        {
                            equipment.NEXTCALDATE = null;

                            if (apply.MSA == "Y")
                            {
                                equipment.NEXTMSADATE = Model.FormInput.EstMSADate;
                            }
                            else
                            {
                                equipment.NEXTMSADATE = null;
                            }
                        }

                        db.QA_EQUIPMENT.Add(equipment);

                        db.SaveChanges();
                    }
                    //文審退回重送
                    else
                    {
                        var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == equipmentUniqueID);

                        equipment.MACHINENO = apply.MACHINENO;
                        equipment.SERIALNO = apply.SERIALNO;
                        equipment.ICHIUNIQUEID = apply.ICHIUNIQUEID;
                        equipment.ICHIREMARK = apply.ICHIREMARK;
                        equipment.FACTORYID = apply.FACTORYID;
                        equipment.ICHITYPE = apply.ICHITYPE;
                        equipment.REMARK = apply.REMARK;
                        equipment.CHARACTERISTICTYPE = apply.CHARACTERISTICTYPE;
                        equipment.SPEC = apply.SPEC;
                        equipment.BRAND = apply.BRAND;
                        equipment.MODEL = apply.MODEL;

                        if (apply.CAL == "Y")
                        {
                            if (string.IsNullOrEmpty(equipment.CALNO))
                            {
                                equipment.CALNO = calNo;
                            }
                        }
                        else
                        {
                            equipment.CALNO = string.Empty;
                        }

                        equipment.CAL = apply.CAL;
                        equipment.MSA = apply.MSA;
                        equipment.MSACYCLE = apply.MSACYCLE;
                        equipment.CALCYCLE = apply.CALCYCLE;
                        equipment.OWNERID = apply.OWNERID;
                        equipment.OWNERMANAGERID = apply.OWNERMANAGERID;
                        equipment.PEID = apply.PEID;
                        equipment.PEMANAGERID = apply.PEMANAGERID;
                        equipment.CALTYPE = apply.CALTYPE;
                        equipment.CALUNIT = apply.CALUNIT;
                        equipment.CASETYPE = apply.CASETYPE;

                        if (equipment.MSA == "Y")
                        {
                            if (string.IsNullOrEmpty(equipment.MSACALNO))
                            {
                                equipment.MSACALNO = apply.MSACALNO;
                            }
                        }
                        else
                        {
                            equipment.MSACALNO = string.Empty;
                        }

                        equipment.MSAICHIREMARK = apply.MSAICHIREMARK;
                        equipment.MSAICHIUNIQUEID = apply.MSAICHIUNIQUEID;
                        equipment.MSASTATIONREMARK = apply.MSASTATIONREMARK;
                        equipment.MSASTATIONUNIQUEID = apply.MSASTATIONUNIQUEID;
                        equipment.MSATYPE = apply.MSATYPE;
                        equipment.MSASUBTYPE = apply.MSASUBTYPE;

                        if (apply.CAL == "Y")
                        {
                            equipment.NEXTCALDATE = Model.FormInput.EstCalibrateDate;
                        }
                        else
                        {
                            equipment.NEXTCALDATE = null;

                            if (apply.MSA == "Y")
                            {
                                equipment.NEXTMSADATE = Model.FormInput.EstMSADate;
                            }
                            else
                            {
                                equipment.NEXTMSADATE = null;
                            }
                        }

                        db.QA_EQUIPMENTCALDETAIL.RemoveRange(db.QA_EQUIPMENTCALDETAIL.Where(x => x.EQUIPMENTUNIQUEID == equipmentUniqueID).ToList());
                        db.QA_EQUIPMENTCALSUBDETAIL.RemoveRange(db.QA_EQUIPMENTCALSUBDETAIL.Where(x => x.EQUIPMENTUNIQUEID == equipmentUniqueID).ToList());
                        db.QA_EQUIPMENTMSADETAIL.RemoveRange(db.QA_EQUIPMENTMSADETAIL.Where(x => x.EQUIPMENTUNIQUEID == equipmentUniqueID).ToList());

                        db.SaveChanges();
                    }

                    var log = db.QA_CALIBRATIONAPPLYFLOWLOG.FirstOrDefault(x => x.APPLYUNIQUEID == apply.UNIQUEID && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue && x.FLOWSEQ == 5);

                    if (log != null)
                    {
                        log.USERID = Account.ID;
                        log.VERIFYTIME = DateTime.Now;
                        log.VERIFYRESULT = "Y";

                        db.SaveChanges();
                    }

                    db.QA_EQUIPMENTMSADETAIL.AddRange(db.QA_CALIBRATIONAPPLYMSADETAIL.Where(x => x.APPLYUNIQUEID == apply.UNIQUEID).ToList().Select(x => new QA_EQUIPMENTMSADETAIL
                    {
                        EQUIPMENTUNIQUEID = equipmentUniqueID,
                        LOWERRANGE = x.LOWERRANGE,
                        MSACHARACTERISITICREMARK = x.MSACHARACTERISITICREMARK,
                        MSACHARACTERISITICUNIQUEID = x.MSACHARACTERISITICUNIQUEID,
                        MSAUNITREMARK = x.MSAUNITREMARK,
                        MSAUNITUNIQUEID = x.MSAUNITUNIQUEID,
                        SEQ = x.SEQ,
                        UPPERRANGE = x.UPPERRANGE
                    }).ToList());

                    db.SaveChanges();

                    if (apply.CAL == "Y")
                    {
                        var form = db.QA_CALIBRATIONFORM.FirstOrDefault(x => x.APPLYUNIQUEID == apply.UNIQUEID);

                        var formUniqueID = form != null ? form.UNIQUEID : Guid.NewGuid().ToString();

                        if (form == null)
                        {
                            var vhnoPrefix = string.Format("C{0}", DateTime.Today.ToString("yyyyMM").Substring(2));

                            var vhnoSeq = 1;

                            var vhnoQuery = db.QA_CALIBRATIONFORM.Where(x => x.VHNO.StartsWith(vhnoPrefix)).OrderByDescending(x => x.VHNO).ToList();

                            if (vhnoQuery.Count > 0)
                            {
                                vhnoSeq = int.Parse(vhnoQuery.First().VHNO.Substring(5)) + 1;
                            }

                            var vhno = string.Format("{0}{1}", vhnoPrefix, vhnoSeq.ToString().PadLeft(4, '0'));

                            if (apply.CALUNIT == "L")
                            {
                                db.QA_CALIBRATIONFORM.Add(new QA_CALIBRATIONFORM()
                                {
                                    UNIQUEID = formUniqueID,
                                    TRACEABLENO = "92_31_0000_0007",
                                    VHNO = vhno,
                                    APPLYUNIQUEID = apply.UNIQUEID,
                                    STATUS = "0",
                                    JOBCALRESPONSERID = Model.FormInput.CalibratorID,
                                    HAVEABNORMAL = "N",
                                    NOTIFYDATE = DateTime.Now,
                                    ESTCALDATE = Model.FormInput.EstCalibrateDate.Value
                                });
                            }
                            else
                            {
                                db.QA_CALIBRATIONFORM.Add(new QA_CALIBRATIONFORM()
                                {
                                    UNIQUEID = formUniqueID,
                                    TRACEABLENO = "92_31_0000_0007",
                                    VHNO = vhno,
                                    APPLYUNIQUEID = apply.UNIQUEID,
                                    STATUS = "1",
                                    JOBCALRESPONSERID = Model.FormInput.CalibratorID,
                                    CALRESPONSORID = Model.FormInput.CalibratorID,
                                    TAKEJOBDATE = DateTime.Now,
                                    HAVEABNORMAL = "N",
                                    NOTIFYDATE = DateTime.Now,
                                    ESTCALDATE = Model.FormInput.EstCalibrateDate.Value
                                });
                            }
                        }
                        else
                        {
                            if (apply.CALUNIT == "L")
                            {
                                form.STATUS = "0";
                                form.JOBCALRESPONSERID = Model.FormInput.CalibratorID;
                                form.CALRESPONSORID = null;
                                form.TAKEJOBDATE = null;
                                form.NOTIFYDATE = DateTime.Now;
                                form.ESTCALDATE = Model.FormInput.EstCalibrateDate.Value;
                            }
                            else
                            {
                                form.STATUS = "1";
                                form.JOBCALRESPONSERID = Model.FormInput.CalibratorID;
                                form.CALRESPONSORID = Model.FormInput.CalibratorID;
                                form.TAKEJOBDATE = DateTime.Now;
                                form.NOTIFYDATE = DateTime.Now;
                                form.ESTCALDATE = Model.FormInput.EstCalibrateDate.Value;
                            }

                            db.QA_CALIBRATIONFORMDETAIL.RemoveRange(db.QA_CALIBRATIONFORMDETAIL.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList());
                        }

                        db.SaveChanges();

                        var detailList = (from detail in db.QA_CALIBRATIONAPPLYDETAIL
                                          join characterstic in db.QA_CHARACTERISTIC on detail.CHARACTERISTICUNIQUEID equals characterstic.UNIQUEID into tmpCharacteristic
                                          from characterstic in tmpCharacteristic.DefaultIfEmpty()
                                          join unit in db.QA_UNIT on detail.UNITUNIQUEID equals unit.UNIQUEID into tmpUnit
                                          from unit in tmpUnit.DefaultIfEmpty()
                                          join lowerUsingRangeUnit in db.QA_UNIT on detail.LOWERUSINGRANGEUNITUNIQUEID equals lowerUsingRangeUnit.UNIQUEID into tmpLowerUsingRangeUnit
                                          from lowerUsingRangeUnit in tmpLowerUsingRangeUnit.DefaultIfEmpty()
                                          join upperUsingRangeUnit in db.QA_UNIT on detail.UPPERUSINGRANGEUNITUNIQUEID equals upperUsingRangeUnit.UNIQUEID into tmpUpperUsingRangeUnit
                                          from upperUsingRangeUnit in tmpUpperUsingRangeUnit.DefaultIfEmpty()
                                          join rangeToleranceUnit in db.QA_TOLERANCEUNIT on detail.RANGETOLERANCEUNITUNIQUEID equals rangeToleranceUnit.UNIQUEID into tmpRangeToleranceUnit
                                          from rangeToleranceUnit in tmpRangeToleranceUnit.DefaultIfEmpty()
                                          where detail.APPLYUNIQUEID == apply.UNIQUEID
                                          select new DetailItem
                                          {
                                              Seq = detail.SEQ,
                                              CharacteristicUniqueID = detail.CHARACTERISTICUNIQUEID,
                                              CharacteristicDescription = characterstic != null ? characterstic.DESCRIPTION : "",
                                              CharacteristicRemark = detail.CHARACTERISTICREMARK,
                                              UnitUniqueID = detail.UNITUNIQUEID,
                                              UnitDescription = unit != null ? unit.DESCRIPTION : "",
                                              UnitRemark = detail.UNITREMARK,
                                              LowerUsingRange = detail.LOWERUSINGRANGE,
                                              LowerUsingRangeUnitUniqueID = detail.LOWERUSINGRANGEUNITUNIQUEID,
                                              LowerUsingRangeUnitDescription = lowerUsingRangeUnit != null ? lowerUsingRangeUnit.DESCRIPTION : "",
                                              LowerUsingRangeUnitRemark = detail.LOWERUSINGRANGEUNITREMARK,
                                              UpperUsingRange = detail.UPPERUSINGRANGE,
                                              UpperUsingRangeUnitUniqueID = detail.UPPERUSINGRANGEUNITUNIQUEID,
                                              UpperUsingRangeUnitDescription = upperUsingRangeUnit != null ? upperUsingRangeUnit.DESCRIPTION : "",
                                              UpperUsingRangeUnitRemark = detail.UPPERUSINGRANGEUNITREMARK,
                                              UsingRangeToleranceSymbol = detail.RANGETOLERANCESYMBOL,
                                              UsingRangeTolerance = detail.RANGETOLERANCE,
                                              UsingRangeToleranceUnitUniqueID = detail.RANGETOLERANCEUNITUNIQUEID,
                                              UsingRangeToleranceUnitDescription = rangeToleranceUnit != null ? rangeToleranceUnit.DESCRIPTION : "",
                                              UsingRangeToleranceUnitRemark = detail.RANGETOLERANCEUNITREMARK,
                                              ItemList = (from subDetail in db.QA_CALIBRATIONAPPLYSUBDETAIL
                                                          join calibrationPointUnit in db.QA_UNIT on subDetail.CALIBRATIONPOINTUNITUNIQUEID equals calibrationPointUnit.UNIQUEID into tmpCalibrationUnit
                                                          from calibrationPointUnit in tmpCalibrationUnit.DefaultIfEmpty()
                                                          join toleranceUnit in db.QA_TOLERANCEUNIT on subDetail.TOLERANCEUNITUNIQUEID equals toleranceUnit.UNIQUEID into tmpToleranceUnit
                                                          from toleranceUnit in tmpToleranceUnit.DefaultIfEmpty()
                                                          where subDetail.APPLYUNIQUEID == apply.UNIQUEID && subDetail.DETAILSEQ == detail.SEQ
                                                          orderby subDetail.SEQ
                                                          select new SubDetailItem
                                                          {
                                                              Seq = subDetail.SEQ,
                                                              CalibrationPoint = subDetail.CALIBRATIONPOINT,
                                                              CalibrationPointUnitUniqueID = subDetail.CALIBRATIONPOINTUNITUNIQUEID,
                                                              CalibrationPointUnitDescription = calibrationPointUnit != null ? calibrationPointUnit.DESCRIPTION : "",
                                                              ToleranceSymbol = subDetail.TOLERANCESYMBOL,
                                                              Tolerance = subDetail.TOLERANCE,
                                                              ToleranceUnitUniqueID = subDetail.TOLERANCEUNITUNIQUEID,
                                                              ToleranceUnitDescription = toleranceUnit != null ? toleranceUnit.DESCRIPTION : ""
                                                          }).ToList()
                                          }).OrderBy(x => x.Seq).ToList();

                        db.QA_EQUIPMENTCALDETAIL.AddRange(detailList.Select(x => new QA_EQUIPMENTCALDETAIL
                        {
                            EQUIPMENTUNIQUEID = equipmentUniqueID,
                            CHARACTERISTICREMARK = x.CharacteristicRemark,
                            CHARACTERISTICUNIQUEID = x.CharacteristicUniqueID,
                            LOWERUSINGRANGE = x.LowerUsingRange,
                            LOWERUSINGRANGEUNITREMARK = x.LowerUsingRangeUnitRemark,
                            LOWERUSINGRANGEUNITUNIQUEID = x.LowerUsingRangeUnitUniqueID,
                            RANGETOLERANCE = x.UsingRangeTolerance,
                            RANGETOLERANCESYMBOL = x.UsingRangeToleranceSymbol,
                            RANGETOLERANCEUNITREMARK = x.UsingRangeToleranceUnitRemark,
                            RANGETOLERANCEUNITUNIQUEID = x.UsingRangeToleranceUnitUniqueID,
                            SEQ = x.Seq,
                            UNITREMARK = x.UnitRemark,
                            UNITUNIQUEID = x.UnitUniqueID,
                            UPPERUSINGRANGE = x.UpperUsingRange,
                            UPPERUSINGRANGEUNITREMARK = x.UpperUsingRangeUnitRemark,
                            UPPERUSINGRANGEUNITUNIQUEID = x.UpperUsingRangeUnitUniqueID
                        }).ToList());

                        var seq = 1;

                        foreach (var detail in detailList)
                        {
                            foreach (var subDetail in detail.ItemList)
                            {
                                var toleranceUnit = db.QA_TOLERANCEUNIT.FirstOrDefault(x => x.UNIQUEID == subDetail.ToleranceUnitUniqueID);

                                db.QA_CALIBRATIONFORMDETAIL.Add(new QA_CALIBRATIONFORMDETAIL
                                {
                                    FORMUNIQUEID = formUniqueID,
                                    SEQ = seq,
                                    CHARACTERISTIC = detail.Characteristic,
                                    USINGRANGE = detail.UsingRange,
                                    RANGETOLERANCE = detail.UsingRangeToleranceDisplay,
                                    CALIBRATIONPOINT = subDetail.CalibrationPointDisplay,
                                    TOLERANCESYMBOL = subDetail.ToleranceSymbol,
                                    TOLERANCE = Convert.ToDecimal( subDetail.Tolerance),
                                    TOLERANCEUNIT = subDetail.ToleranceUnitDisplay,
                                    UNIT = subDetail.CalibrationPointUnitDescription,
                                    TOLERANCEUNITRATE = subDetail.ToleranceUnitUniqueID == "%" ? 0 : toleranceUnit != null ? toleranceUnit.RATE.Value : 1
                                });

                                db.QA_EQUIPMENTCALSUBDETAIL.Add(new QA_EQUIPMENTCALSUBDETAIL
                                {
                                    EQUIPMENTUNIQUEID = equipmentUniqueID,
                                    CALIBRATIONPOINT =Convert.ToDecimal( subDetail.CalibrationPoint),
                                    CALIBRATIONPOINTUNITUNIQUEID = subDetail.CalibrationPointUnitUniqueID,
                                    DETAILSEQ = detail.Seq,
                                    SEQ = subDetail.Seq,
                                    TOLERANCE = Convert.ToDecimal( subDetail.Tolerance),
                                    TOLERANCESYMBOL = subDetail.ToleranceSymbol,
                                    TOLERANCEUNITUNIQUEID = subDetail.ToleranceUnitUniqueID
                                });

                                seq++;
                            }
                        }

                        db.SaveChanges();

                        CalibrationFormDataAccessor.SendCreatedMail(OrganizationList, formUniqueID);
                    }
                    else
                    {
                        if (apply.MSA == "Y")
                        {
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
                                        EQUIPMENTUNIQUEID = equipmentUniqueID,
                                        STATUS = "1",
                                        LOWERRANGE = detail.LowerRange,
                                        UPPERRANGE = detail.UpperRange,
                                        CREATETIME = createTime,
                                        CHARACTERISITIC = detail.Charateristic,
                                        ESTMSADATE = Model.FormInput.EstMSADate,
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
                                    EQUIPMENTUNIQUEID = equipmentUniqueID,
                                    STATUS = "1",
                                    LOWERRANGE = detail.LowerRange,
                                    UPPERRANGE = detail.UpperRange,
                                    CREATETIME = createTime,
                                    CHARACTERISITIC = detail.Charateristic,
                                    ESTMSADATE = Model.FormInput.EstMSADate,
                                    MSAICHI = Model.FormViewModel.MSAIchiDisplay,
                                    MSARESPONSORID = Model.FormViewModel.PEID,
                                    STATION = Model.FormViewModel.MSAStationDisplay,
                                    TYPE = Model.FormViewModel.MSAType,
                                    UNIT = detail.Unit,
                                    SUBTYPE = Model.FormViewModel.MSASubType
                                });
                            }

                            db.SaveChanges();
                        }
                    }

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

        /// <summary>
        /// QA
        /// </summary>
        public static RequestResult QAReject(string UniqueID, string RejectTo, string Comment, Account Account)
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
                        var apply = db.QA_CALIBRATIONAPPLY.First(x => x.UNIQUEID == UniqueID);

                        var log = db.QA_CALIBRATIONAPPLYFLOWLOG.FirstOrDefault(x => x.APPLYUNIQUEID == apply.UNIQUEID && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue && x.FLOWSEQ == 5);

                        if (log != null)
                        {
                            log.USERID = Account.ID;
                            log.VERIFYTIME = DateTime.Now;
                            log.VERIFYRESULT = "N";
                            log.VERIFYCOMMENT = Comment;

                            db.SaveChanges();
                        }

                        var logList = db.QA_CALIBRATIONAPPLYFLOWLOG.Where(x => x.APPLYUNIQUEID == apply.UNIQUEID && x.SEQ != log.SEQ && !x.VERIFYTIME.HasValue).ToList();

                        foreach (var l in logList)
                        {
                            l.ISCANCELED = "Y";
                        }

                        db.SaveChanges();

                        var logSeq = db.QA_CALIBRATIONAPPLYFLOWLOG.Where(x => x.APPLYUNIQUEID == apply.UNIQUEID).Max(x => x.SEQ) + 1;

                        if (RejectTo == "ME")
                        {
                            apply.STATUS = "2";

                            db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                            {
                                APPLYUNIQUEID = apply.UNIQUEID,
                                SEQ = logSeq,
                                FLOWSEQ = 1,
                                NOTIFYTIME = DateTime.Now,
                                USERID = apply.OWNERID,
                                ISCANCELED = "N"
                            });

                            SendRejectMail(apply.UNIQUEID, apply.VHNO, new List<string>() { apply.CREATORID, apply.OWNERID });
                        }
                        else if (RejectTo == "PE")
                        {
                            db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                            {
                                APPLYUNIQUEID = apply.UNIQUEID,
                                SEQ = logSeq,
                                FLOWSEQ = 3,
                                NOTIFYTIME = DateTime.Now,
                                USERID = apply.PEID,
                                ISCANCELED = "N"
                            });

                            SendRejectMail(apply.UNIQUEID, apply.VHNO, new List<string>() { apply.PEID });
                        }

                        db.SaveChanges();

                    #if !DEBUG
                        trans.Complete();
#endif

                        
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

        public static RequestResult GetDetailItemCreateFormModel(string IchiUniqueID, string CharacteristicType)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var model = new DetailItemCreateFormModel()
                    {
                        CharacteristicSelectItemList = new List<SelectListItem>() 
                        { 
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        },
                        UnitSelectItemList = new List<SelectListItem>() 
                        { 
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        },
                        LowerUsingRangeUnitSelectItemList = new List<SelectListItem>() 
                        { 
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        },
                        UpperUsingRangeUnitSelectItemList = new List<SelectListItem>() 
                        { 
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        },
                        UsingRangeToleranceUnitSelectItemList = new List<SelectListItem>() 
                        { 
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        },
                        UnitList = (from x in db.QA_CHARACTERISTICUNIT
                                    join u in db.QA_UNIT
                                    on x.UNITUNIQUEID equals u.UNIQUEID
                                    select new CharacteristicUnitModel
                                    {
                                        CharacteristicUniqueID = x.CHARACTERISTICUNIQUEID,
                                        UniqueID = u.UNIQUEID,
                                        Description = u.DESCRIPTION
                                    }).OrderBy(x => x.Description).ToList(),
                        ToleranceUnitList = db.QA_TOLERANCEUNIT.OrderBy(x => x.DESCRIPTION).Select(x => new ToleranceUnitModel
                        {
                            UnitUniqueID = x.UNITUNIQUEID,
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION
                        }).ToList()
                    };

                    model.CharacteristicSelectItemList.AddRange((from x in db.QA_ICHICHARACTERISTIC
                                                                 join c in db.QA_CHARACTERISTIC
                                                                 on x.CHARACTERISTICUNIQUEID equals c.UNIQUEID
                                                                 where x.ICHIUNIQUEID == IchiUniqueID && c.TYPE == CharacteristicType
                                                                 select c).OrderBy(x => x.DESCRIPTION).Select(x => new SelectListItem
                                                                 {
                                                                     Value = x.UNIQUEID,
                                                                     Text = x.DESCRIPTION
                                                                 }).ToList());

                    model.CharacteristicSelectItemList.Add(new SelectListItem()
                    {
                        Value = Define.OTHER,
                        Text = Resources.Resource.Other
                    });

                    result.ReturnData(model);
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

        public static RequestResult CreateDetailItem(List<DetailItem> ItemList, DetailItemCreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                int seq = 1;

                if (ItemList.Count > 0)
                {
                    seq = ItemList.Max(x => x.Seq) + 1;
                }

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var characteristic = db.QA_CHARACTERISTIC.FirstOrDefault(x => x.UNIQUEID == Model.FormInput.CharacteristicUniqueID);
                    var unit = db.QA_UNIT.FirstOrDefault(x => x.UNIQUEID == Model.FormInput.UnitUniqueID);
                    var lowerUsingRangeUnit = db.QA_UNIT.FirstOrDefault(x => x.UNIQUEID == Model.FormInput.LowerUsingRangeUnitUniqueID);
                    var upperUsingRangeUnit = db.QA_UNIT.FirstOrDefault(x => x.UNIQUEID == Model.FormInput.UpperUsingRangeUnitUniqueID);
                    var usingRangeToleranceUnit = db.QA_TOLERANCEUNIT.FirstOrDefault(x => x.UNIQUEID == Model.FormInput.UsingRangeToleranceUnitUniqueID);

                    var detailItem = new DetailItem()
                    {
                        Seq = seq,
                        CharacteristicUniqueID = Model.FormInput.CharacteristicUniqueID,
                        CharacteristicDescription = characteristic != null ? characteristic.DESCRIPTION : string.Empty,
                        CharacteristicRemark = Model.FormInput.CharacteristicRemark,
                        UnitUniqueID = Model.FormInput.UnitUniqueID,
                        UnitDescription = unit != null ? unit.DESCRIPTION : string.Empty,
                        UnitRemark = Model.FormInput.UnitRemark,
                        LowerUsingRange = Model.FormInput.LowerUsingRange,
                        LowerUsingRangeUnitUniqueID = Model.FormInput.LowerUsingRangeUnitUniqueID,
                        LowerUsingRangeUnitDescription = lowerUsingRangeUnit != null ? lowerUsingRangeUnit.DESCRIPTION : string.Empty,
                        LowerUsingRangeUnitRemark = Model.FormInput.LowerUsingRangeUnitRemark,
                        UpperUsingRange = Model.FormInput.UpperUsingRange,
                        UpperUsingRangeUnitUniqueID = Model.FormInput.UpperUsingRangeUnitUniqueID,
                        UpperUsingRangeUnitDescription = upperUsingRangeUnit != null ? upperUsingRangeUnit.DESCRIPTION : string.Empty,
                        UpperUsingRangeUnitRemark = Model.FormInput.UpperUsingRangeUnitRemark,
                        UsingRangeToleranceSymbol = Model.FormInput.UsingRangeToleranceSymbol,
                        UsingRangeTolerance = Model.FormInput.UsingRangeTolerance,
                        UsingRangeToleranceUnitUniqueID = Model.FormInput.UsingRangeToleranceUnitUniqueID,
                        UsingRangeToleranceUnitDescription = usingRangeToleranceUnit != null ? usingRangeToleranceUnit.DESCRIPTION : string.Empty,
                        UsingRangeToleranceUnitRemark = Model.FormInput.UsingRangeToleranceUnitRemark
                    };

                    var itemSeq = 1;

                    foreach (string subitem in Model.FormInput.SubItemList)
                    {
                        string[] temp = subitem.Split(Define.Seperators, StringSplitOptions.None);

                        var calibrationPoint = temp[0];
                        var calibrationPointUnitUniqueID = temp[1];
                        var toleranceMark = temp[2];
                        var tolerance = temp[3];
                        var toleranceUnitUniqueID = temp[4];

                        var calibrationPointUnit = db.QA_UNIT.FirstOrDefault(x => x.UNIQUEID == calibrationPointUnitUniqueID);
                        var toleranceUnit = db.QA_TOLERANCEUNIT.FirstOrDefault(x => x.UNIQUEID == toleranceUnitUniqueID);

                        detailItem.ItemList.Add(new SubDetailItem()
                        {
                            Seq = itemSeq,
                            CalibrationPoint = decimal.Parse(calibrationPoint),
                            CalibrationPointUnitUniqueID = calibrationPointUnitUniqueID,
                            CalibrationPointUnitDescription = calibrationPointUnit != null ? calibrationPointUnit.DESCRIPTION : string.Empty,
                            ToleranceSymbol = toleranceMark,
                            Tolerance = decimal.Parse(tolerance),
                            ToleranceUnitUniqueID = toleranceUnitUniqueID,
                            ToleranceUnitDescription = toleranceUnit != null ? toleranceUnit.DESCRIPTION : string.Empty
                        });

                        itemSeq++;
                    }

                    ItemList.Add(detailItem);
                }

                result.ReturnData(ItemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetDetailItemEditFormModel(string IchiUniqueID, string CharacteristicType, DetailItem Item)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var model = new DetailItemEditFormModel
                    {
                        Seq = Item.Seq,
                        CharacteristicSelectItemList = new List<SelectListItem>() 
                        { 
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        },
                        UnitSelectItemList = new List<SelectListItem>() 
                        { 
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        },
                        LowerUsingRangeUnitSelectItemList = new List<SelectListItem>() 
                        { 
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        },
                        UpperUsingRangeUnitSelectItemList = new List<SelectListItem>() 
                        { 
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        },
                        UsingRangeToleranceUnitSelectItemList = new List<SelectListItem>() 
                        { 
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        },
                        UnitList = (from x in db.QA_CHARACTERISTICUNIT
                                    join u in db.QA_UNIT
                                    on x.UNITUNIQUEID equals u.UNIQUEID
                                    select new CharacteristicUnitModel
                                    {
                                        CharacteristicUniqueID = x.CHARACTERISTICUNIQUEID,
                                        UniqueID = u.UNIQUEID,
                                        Description = u.DESCRIPTION
                                    }).OrderBy(x => x.Description).ToList(),
                        ToleranceUnitList = db.QA_TOLERANCEUNIT.OrderBy(x => x.DESCRIPTION).Select(x => new ToleranceUnitModel
                        {
                            UnitUniqueID = x.UNITUNIQUEID,
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION
                        }).ToList(),
                        FormInput = new DetailItemFormInput
                        {
                            CharacteristicUniqueID = Item.CharacteristicUniqueID,
                            CharacteristicRemark = Item.CharacteristicRemark,
                            UnitUniqueID = Item.UnitUniqueID,
                            UnitRemark = Item.UnitRemark,
                            LowerUsingRange = Item.LowerUsingRange,
                            LowerUsingRangeUnitUniqueID = Item.LowerUsingRangeUnitUniqueID,
                            LowerUsingRangeUnitRemark = Item.LowerUsingRangeUnitRemark,
                            UpperUsingRange = Item.UpperUsingRange,
                            UpperUsingRangeUnitUniqueID = Item.UpperUsingRangeUnitUniqueID,
                            UpperUsingRangeUnitRemark = Item.UpperUsingRangeUnitRemark,
                            UsingRangeToleranceSymbol = Item.UsingRangeToleranceSymbol,
                            UsingRangeTolerance = Item.UsingRangeTolerance,
                            UsingRangeToleranceUnitUniqueID = Item.UsingRangeToleranceUnitUniqueID,
                            UsingRangeToleranceUnitRemark = Item.UsingRangeToleranceUnitRemark
                        },
                        ItemList = Item.ItemList
                    };

                    var characteristicList = (from x in db.QA_ICHICHARACTERISTIC
                                              join c in db.QA_CHARACTERISTIC
                                              on x.CHARACTERISTICUNIQUEID equals c.UNIQUEID
                                              where x.ICHIUNIQUEID == IchiUniqueID && c.TYPE == CharacteristicType
                                              select c).OrderBy(x => x.DESCRIPTION).ToList();

                    foreach (var characteristic in characteristicList)
                    {
                        model.CharacteristicSelectItemList.Add(new SelectListItem()
                        {
                            Value = characteristic.UNIQUEID,
                            Text = characteristic.DESCRIPTION
                        });

                        if (model.FormInput.CharacteristicUniqueID == characteristic.UNIQUEID)
                        {
                            var unitList = model.UnitList.Where(x => x.CharacteristicUniqueID == characteristic.UNIQUEID).OrderBy(x => x.Description).ToList();

                            foreach (var unit in unitList)
                            {
                                model.UnitSelectItemList.Add(new SelectListItem()
                                {
                                    Text = unit.Description,
                                    Value = unit.UniqueID
                                });

                                model.LowerUsingRangeUnitSelectItemList.Add(new SelectListItem()
                                {
                                    Text = unit.Description,
                                    Value = unit.UniqueID
                                });

                                model.UpperUsingRangeUnitSelectItemList.Add(new SelectListItem()
                                {
                                    Text = unit.Description,
                                    Value = unit.UniqueID
                                });

                                if (model.FormInput.UnitUniqueID == unit.UniqueID)
                                {
                                    var toleranceUnitList = model.ToleranceUnitList.Where(x => x.UnitUniqueID == unit.UniqueID).OrderBy(x => x.Description).ToList();

                                    foreach (var toleranceUnit in toleranceUnitList)
                                    {
                                        model.UsingRangeToleranceUnitSelectItemList.Add(new SelectListItem()
                                        {
                                            Text = toleranceUnit.Description,
                                            Value = toleranceUnit.UniqueID
                                        });
                                    }

                                    model.UsingRangeToleranceUnitSelectItemList.Add(new SelectListItem()
                                    {
                                        Value = "%",
                                        Text = "%"
                                    });

                                    model.UsingRangeToleranceUnitSelectItemList.Add(new SelectListItem()
                                    {
                                        Value = Define.OTHER,
                                        Text = Resources.Resource.Other
                                    });
                                }
                            }

                            model.UnitSelectItemList.Add(new SelectListItem()
                            {
                                Value = Define.OTHER,
                                Text = Resources.Resource.Other
                            });

                            model.LowerUsingRangeUnitSelectItemList.Add(new SelectListItem()
                            {
                                Value = Define.OTHER,
                                Text = Resources.Resource.Other
                            });

                            model.UpperUsingRangeUnitSelectItemList.Add(new SelectListItem()
                            {
                                Value = Define.OTHER,
                                Text = Resources.Resource.Other
                            });
                        }
                    }

                    model.CharacteristicSelectItemList.Add(new SelectListItem()
                    {
                        Value = Define.OTHER,
                        Text = Resources.Resource.Other
                    });

                    result.ReturnData(model);
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

        public static RequestResult EditDetailItem(List<DetailItem> ItemList, DetailItemEditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                var item = ItemList.First(x => x.Seq == Model.Seq);

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var characteristic = db.QA_CHARACTERISTIC.FirstOrDefault(x => x.UNIQUEID == Model.FormInput.CharacteristicUniqueID);
                    var unit = db.QA_UNIT.FirstOrDefault(x => x.UNIQUEID == Model.FormInput.UnitUniqueID);
                    var lowerUsingRangeUnit = db.QA_UNIT.FirstOrDefault(x => x.UNIQUEID == Model.FormInput.LowerUsingRangeUnitUniqueID);
                    var upperUsingRangeUnit = db.QA_UNIT.FirstOrDefault(x => x.UNIQUEID == Model.FormInput.UpperUsingRangeUnitUniqueID);
                    var usingRangeToleranceUnit = db.QA_TOLERANCEUNIT.FirstOrDefault(x => x.UNIQUEID == Model.FormInput.UsingRangeToleranceUnitUniqueID);

                    item.CharacteristicUniqueID = Model.FormInput.CharacteristicUniqueID;
                    item.CharacteristicDescription = characteristic != null ? characteristic.DESCRIPTION : string.Empty;
                    item.CharacteristicRemark = Model.FormInput.CharacteristicRemark;
                    item.UnitUniqueID = Model.FormInput.UnitUniqueID;
                    item.UnitDescription = unit != null ? unit.DESCRIPTION : string.Empty;
                    item.UnitRemark = Model.FormInput.UnitRemark;
                    item.LowerUsingRange = Model.FormInput.LowerUsingRange;
                    item.LowerUsingRangeUnitUniqueID = Model.FormInput.LowerUsingRangeUnitUniqueID;
                    item.LowerUsingRangeUnitDescription = lowerUsingRangeUnit != null ? lowerUsingRangeUnit.DESCRIPTION : string.Empty;
                    item.LowerUsingRangeUnitRemark = Model.FormInput.LowerUsingRangeUnitRemark;
                    item.UpperUsingRange = Model.FormInput.UpperUsingRange;
                    item.UpperUsingRangeUnitUniqueID = Model.FormInput.UpperUsingRangeUnitUniqueID;
                    item.UpperUsingRangeUnitDescription = upperUsingRangeUnit != null ? upperUsingRangeUnit.DESCRIPTION : string.Empty;
                    item.UpperUsingRangeUnitRemark = Model.FormInput.UpperUsingRangeUnitRemark;
                    item.UsingRangeToleranceSymbol = Model.FormInput.UsingRangeToleranceSymbol;
                    item.UsingRangeTolerance = Model.FormInput.UsingRangeTolerance;
                    item.UsingRangeToleranceUnitUniqueID = Model.FormInput.UsingRangeToleranceUnitUniqueID;
                    item.UsingRangeToleranceUnitDescription = usingRangeToleranceUnit != null ? usingRangeToleranceUnit.DESCRIPTION : string.Empty;
                    item.UsingRangeToleranceUnitRemark = Model.FormInput.UsingRangeToleranceUnitRemark;

                    item.ItemList = new List<SubDetailItem>();

                    var itemSeq = 1;

                    foreach (string subitem in Model.FormInput.SubItemList)
                    {
                        string[] temp = subitem.Split(Define.Seperators, StringSplitOptions.None);

                        var calibrationPoint = temp[0];
                        var calibrationPointUnitUniqueID = temp[1];
                        var toleranceMark = temp[2];
                        var tolerance = temp[3];
                        var toleranceUnitUniqueID = temp[4];

                        var calibrationPointUnit = db.QA_UNIT.FirstOrDefault(x => x.UNIQUEID == calibrationPointUnitUniqueID);
                        var toleranceUnit = db.QA_TOLERANCEUNIT.FirstOrDefault(x => x.UNIQUEID == toleranceUnitUniqueID);

                        item.ItemList.Add(new SubDetailItem()
                        {
                             Seq = itemSeq,
                            CalibrationPoint = decimal.Parse(calibrationPoint),
                            CalibrationPointUnitUniqueID = calibrationPointUnitUniqueID,
                            CalibrationPointUnitDescription = calibrationPointUnit != null ? calibrationPointUnit.DESCRIPTION : string.Empty,
                            ToleranceSymbol = toleranceMark,
                             Tolerance = decimal.Parse(tolerance),
                            ToleranceUnitUniqueID = toleranceUnitUniqueID,
                            ToleranceUnitDescription = toleranceUnit != null ? toleranceUnit.DESCRIPTION : string.Empty
                        });

                        itemSeq++;
                    }
                }

                result.ReturnData(ItemList);
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
                var apply = (from a in db.QA_CALIBRATIONAPPLY
                             join ichi in db.QA_ICHI on a.ICHIUNIQUEID equals ichi.UNIQUEID into tmpIchi
                             from ichi in tmpIchi.DefaultIfEmpty()
                             join msaStation in db.QA_MSASTATION on a.MSASTATIONUNIQUEID equals msaStation.UNIQUEID into tmpMSAStation
                             from msaStation in tmpMSAStation.DefaultIfEmpty()
                             join msaIchi in db.QA_MSAICHI on a.MSAICHIUNIQUEID equals msaIchi.UNIQUEID into tmpMSAIchi
                             from msaIchi in tmpMSAIchi.DefaultIfEmpty()
                             join creator in db.ACCOUNT on a.CREATORID equals creator.ID into tmpCreator
                             from creator in tmpCreator.DefaultIfEmpty()
                             join owner in db.ACCOUNT on a.OWNERID equals owner.ID into tmpOwner
                             from owner in tmpOwner.DefaultIfEmpty()
                             join ownerManager in db.ACCOUNT on a.OWNERMANAGERID equals ownerManager.ID into tmpOwnerManager
                             from ownerManager in tmpOwnerManager.DefaultIfEmpty()
                             join pe in db.ACCOUNT on a.PEID equals pe.ID into tmpPE
                             from pe in tmpPE.DefaultIfEmpty()
                             join peManager in db.ACCOUNT on a.PEMANAGERID equals peManager.ID into tmpPEManager
                             from peManager in tmpPEManager.DefaultIfEmpty()
                             join calibrator in db.ACCOUNT on a.CALRESPONSORID equals calibrator.ID into tmpCalibrator
                             from calibrator in tmpCalibrator.DefaultIfEmpty()
                             where a.UNIQUEID == UniqueID
                             select new
                             {
                                 a.UNIQUEID,
                                 a.VHNO,
                                 a.STATUS,
                                 a.CREATORID,
                                 CreatorName = creator != null ? creator.NAME : "",
                                 a.CREATETIME,
                                 a.ORGANIZATIONUNIQUEID,
                                 a.FACTORYID,
                                 a.ICHITYPE,
                                 a.ICHIUNIQUEID,
                                 IchiName = ichi != null ? ichi.NAME : "",
                                 a.ICHIREMARK,
                                 a.CHARACTERISTICTYPE,
                                 a.MACHINENO,
                                 a.SERIALNO,
                                 a.BRAND,
                                 a.MODEL,
                                 a.SPEC,
                                 a.CAL,
                                 a.MSA,
                                 a.OWNERID,
                                 OwnerName = owner != null ? owner.NAME : "",
                                 a.OWNERMANAGERID,
                                 OwnerManagerName = ownerManager != null ? ownerManager.NAME : "",
                                 a.PEID,
                                 PEName = pe != null ? pe.NAME : "",
                                 a.PEMANAGERID,
                                 PEManagerName = peManager != null ? peManager.NAME : "",
                                 a.CALCYCLE,
                                 a.MSACYCLE,
                                 a.REMARK,
                                 a.CALTYPE,
                                 a.CALUNIT,
                                 a.CASETYPE,
                                 a.CALRESPONSORID,
                                 CalibratorName = calibrator != null ? calibrator.NAME : "",
                                 a.ESTCALDATE,
                                 a.MSATYPE,
                                 a.MSASUBTYPE,
                                 MSAResponserName = pe != null ? pe.NAME : "",
                                 a.ESTMSADATE,
                                 a.MSASTATIONUNIQUEID,
                                 MSAStationName = msaStation != null ? msaStation.NAME : "",
                                 a.MSASTATIONREMARK,
                                 a.MSAICHIUNIQUEID,
                                 MSAIchiName = msaIchi != null ? msaIchi.NAME : "",
                                 a.MSAICHIREMARK
                             }).First();

                var form = db.QA_CALIBRATIONFORM.FirstOrDefault(x => x.APPLYUNIQUEID == apply.UNIQUEID);

                model = new FormViewModel()
                {
                    VHNO = apply.VHNO,
                    FormUniqueID = form!=null?form.UNIQUEID:"",
                    FormVHNO = form!=null?form.VHNO:"",
                    Status = new ApplyStatus(apply.STATUS),
                    CreatorID = apply.CREATORID,
                    CreatorName = apply.CreatorName,
                    CreateTime = apply.CREATETIME,
                    Factory = OrganizationDataAccessor.GetFactory(OrganizationList, apply.ORGANIZATIONUNIQUEID),
                    Department = OrganizationDataAccessor.GetOrganizationFullDescription(apply.ORGANIZATIONUNIQUEID),
                    FactoryID = apply.FACTORYID,
                    IchiType = apply.ICHITYPE,
                    IchiUniqueID = apply.ICHIUNIQUEID,
                    IchiName = apply.IchiName,
                    IchiRemark = apply.ICHIREMARK,
                    CharacteristicType = apply.CHARACTERISTICTYPE,
                    SerialNo = apply.SERIALNO,
                    MachineNo = apply.MACHINENO,
                    Brand = apply.BRAND,
                    Model = apply.MODEL,
                    Spec = apply.SPEC,
                    CAL = apply.CAL == "Y",
                    MSA = apply.MSA == "Y",
                    OwnerID = apply.OWNERID,
                    OwnerName = apply.OwnerName,
                    OwnerManagerID = apply.OWNERMANAGERID,
                    OwnerManagerName = apply.OwnerManagerName,
                    PEID = apply.PEID,
                    PEName = apply.PEName,
                    PEManagerID = apply.PEMANAGERID,
                    PEManagerName = apply.PEManagerName,
                    CalCycle = apply.CALCYCLE,
                    MSACycle = apply.MSACYCLE,
                    Remark = apply.REMARK,
                    CalibrateType = apply.CALTYPE,
                    CalibrateUnit = apply.CALUNIT,
                    CaseType = apply.CASETYPE,
                    CalibratorID = apply.CALRESPONSORID,
                    CalibratorName = apply.CalibratorName,
                    EstCalibrateDate = apply.ESTCALDATE,
                    MSAResponsorID = apply.PEID,
                    MSAResponsorName = apply.MSAResponserName,
                    EstMSADate = apply.ESTMSADATE,
                    MSAType = apply.MSATYPE,
                    MSASubType = apply.MSASUBTYPE,
                    MSAStationUniqueID = apply.MSASTATIONUNIQUEID,
                    MSAStationName = apply.MSAStationName,
                    MSAStationRemark = apply.MSASTATIONREMARK,
                    MSAIchiUniqueID = apply.MSAICHIUNIQUEID,
                    MSAIchiName = apply.MSAIchiName,
                    MSAIchiRemark = apply.MSAICHIREMARK,
                    MSACharacteristicList = (from msaDetail in db.QA_CALIBRATIONAPPLYMSADETAIL
                                             join msaCharacteristic in db.QA_MSACHARACTERISTICS on msaDetail.MSACHARACTERISITICUNIQUEID equals msaCharacteristic.UNIQUEID into tmpCharacteristic
                                             from msaCharacteristic in tmpCharacteristic.DefaultIfEmpty()
                                             join msaUnit in db.QA_MSAUNIT on msaDetail.MSAUNITUNIQUEID equals msaUnit.UNIQUEID into tmpUnit
                                             from msaUnit in tmpUnit.DefaultIfEmpty()
                                             where msaDetail.APPLYUNIQUEID == apply.UNIQUEID
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
                    LogList = (from log in db.QA_CALIBRATIONAPPLYFLOWLOG
                               join user in db.ACCOUNT on log.USERID equals user.ID into tmpUser
                               from user in tmpUser.DefaultIfEmpty()
                               where log.APPLYUNIQUEID == apply.UNIQUEID && log.ISCANCELED == "N"
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
                               }).OrderBy(x => x.VerifyTime).ThenBy(x => x.Seq).ToList(),
                    ItemList = (from detail in db.QA_CALIBRATIONAPPLYDETAIL
                                join characterstic in db.QA_CHARACTERISTIC on detail.CHARACTERISTICUNIQUEID equals characterstic.UNIQUEID into tmpCharacteristic
                                from characterstic in tmpCharacteristic.DefaultIfEmpty()
                                join unit in db.QA_UNIT on detail.UNITUNIQUEID equals unit.UNIQUEID into tmpUnit
                                from unit in tmpUnit.DefaultIfEmpty()
                                join lowerUsingRangeUnit in db.QA_UNIT on detail.LOWERUSINGRANGEUNITUNIQUEID equals lowerUsingRangeUnit.UNIQUEID into tmpLowerUsingRangeUnit
                                from lowerUsingRangeUnit in tmpLowerUsingRangeUnit.DefaultIfEmpty()
                                join upperUsingRangeUnit in db.QA_UNIT on detail.UPPERUSINGRANGEUNITUNIQUEID equals upperUsingRangeUnit.UNIQUEID into tmpUpperUsingRangeUnit
                                from upperUsingRangeUnit in tmpUpperUsingRangeUnit.DefaultIfEmpty()
                                join rangeToleranceUnit in db.QA_TOLERANCEUNIT on detail.RANGETOLERANCEUNITUNIQUEID equals rangeToleranceUnit.UNIQUEID into tmpRangeToleranceUnit
                                from rangeToleranceUnit in tmpRangeToleranceUnit.DefaultIfEmpty()
                                where detail.APPLYUNIQUEID == apply.UNIQUEID
                                select new DetailItem
                                {
                                    Seq = detail.SEQ,
                                    CharacteristicUniqueID = detail.CHARACTERISTICUNIQUEID,
                                    CharacteristicDescription = characterstic != null ? characterstic.DESCRIPTION : "",
                                    CharacteristicRemark = detail.CHARACTERISTICREMARK,
                                    UnitUniqueID = detail.UNITUNIQUEID,
                                    UnitDescription = unit != null ? unit.DESCRIPTION : "",
                                    UnitRemark = detail.UNITREMARK,
                                    LowerUsingRange = detail.LOWERUSINGRANGE,
                                    LowerUsingRangeUnitUniqueID = detail.LOWERUSINGRANGEUNITUNIQUEID,
                                    LowerUsingRangeUnitDescription = lowerUsingRangeUnit != null ? lowerUsingRangeUnit.DESCRIPTION : "",
                                    LowerUsingRangeUnitRemark = detail.LOWERUSINGRANGEUNITREMARK,
                                    UpperUsingRange = detail.UPPERUSINGRANGE,
                                    UpperUsingRangeUnitUniqueID = detail.UPPERUSINGRANGEUNITUNIQUEID,
                                    UpperUsingRangeUnitDescription = upperUsingRangeUnit != null ? upperUsingRangeUnit.DESCRIPTION : "",
                                    UpperUsingRangeUnitRemark = detail.UPPERUSINGRANGEUNITREMARK,
                                    UsingRangeToleranceSymbol = detail.RANGETOLERANCESYMBOL,
                                    UsingRangeTolerance = detail.RANGETOLERANCE,
                                    UsingRangeToleranceUnitUniqueID = detail.RANGETOLERANCEUNITUNIQUEID,
                                    UsingRangeToleranceUnitDescription = rangeToleranceUnit != null ? rangeToleranceUnit.DESCRIPTION : "",
                                    UsingRangeToleranceUnitRemark = detail.RANGETOLERANCEUNITREMARK,
                                    ItemList = (from subDetail in db.QA_CALIBRATIONAPPLYSUBDETAIL
                                                join calibrationPointUnit in db.QA_UNIT on subDetail.CALIBRATIONPOINTUNITUNIQUEID equals calibrationPointUnit.UNIQUEID into tmpCalibrationUnit
                                                from calibrationPointUnit in tmpCalibrationUnit.DefaultIfEmpty()
                                                join toleranceUnit in db.QA_TOLERANCEUNIT on subDetail.TOLERANCEUNITUNIQUEID equals toleranceUnit.UNIQUEID into tmpToleranceUnit
                                                from toleranceUnit in tmpToleranceUnit.DefaultIfEmpty()
                                                where subDetail.APPLYUNIQUEID == apply.UNIQUEID && subDetail.DETAILSEQ == detail.SEQ
                                                orderby subDetail.SEQ
                                                select new SubDetailItem
                                                {
                                                    Seq = subDetail.SEQ,
                                                    CalibrationPoint = subDetail.CALIBRATIONPOINT,
                                                    CalibrationPointUnitUniqueID = subDetail.CALIBRATIONPOINTUNITUNIQUEID,
                                                    CalibrationPointUnitDescription = calibrationPointUnit != null ? calibrationPointUnit.DESCRIPTION : "",
                                                    ToleranceSymbol = subDetail.TOLERANCESYMBOL,
                                                    Tolerance = subDetail.TOLERANCE,
                                                    ToleranceUnitUniqueID = subDetail.TOLERANCEUNITUNIQUEID,
                                                    ToleranceUnitDescription = toleranceUnit != null ? toleranceUnit.DESCRIPTION : ""
                                                }).ToList()
                                }).OrderBy(x => x.Seq).ToList()
                };
            }

            return model;
        }

        public static void SendRemindMail(string UniqueID, string VHNO, int Days, string UserID)
        {
            SendVerifyMail(UniqueID, string.Format("[逾期][簽核通知][{0}天][{1}]儀器校驗/免校驗申請單", Days, VHNO), new List<string>() { UserID });
        }

        public static void SendVerifyMail(string UniqueID, string VHNO, string UserID)
        {
            SendVerifyMail(UniqueID, string.Format("[簽核通知][{0}]儀器校驗/免校驗申請單", VHNO), new List<string>() { UserID });
        }

        public static void SendRemindMail(string UniqueID, string VHNO, int Days)
        {
            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var userList = db.USERAUTHGROUP.Where(x => x.AUTHGROUPID == "QA-Verify").Select(x => x.USERID).ToList();

                    if (userList != null && userList.Count > 0)
                    {
                        SendVerifyMail(UniqueID, string.Format("[逾期][簽核通知][{0}天][{1}]儀器校驗/免校驗申請單", Days, VHNO), userList);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public static void SendVerifyMail(string UniqueID, string VHNO)
        {
            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var userList = db.USERAUTHGROUP.Where(x => x.AUTHGROUPID == "QA-Verify").Select(x => x.USERID).ToList();

                    if (userList != null && userList.Count > 0)
                    {
                        SendVerifyMail(UniqueID, string.Format("[簽核通知][{0}]儀器校驗/免校驗申請單", VHNO), userList);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private static void SendVerifyMail(string UniqueID, string Subject, List<string> UserList)
        {
            try
            {
                var mailAddressList = new List<MailAddress>();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var apply = db.QA_CALIBRATIONAPPLY.First(x => x.UNIQUEID == UniqueID);

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

                        var th = "<th style=\"width:150px;border:1px solid #333;padding:8px;text-align:right;color:#707070;background: #F4F4F4;\">{0}</th>";
                        var td = "<td style=\"width:400px;border:1px solid #333;padding:8px;color:#707070;\">{0}</td>";

                        var sb = new StringBuilder();

                        sb.Append("<table style=\"1px solid #ddd;font-size:13px;border-collapse:collapse;\">");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "單號"));
                        sb.Append(string.Format(td, apply.VHNO));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "部門"));
                        sb.Append(string.Format(td, OrganizationDataAccessor.GetOrganizationDescription(apply.ORGANIZATIONUNIQUEID)));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "序號"));
                        sb.Append(string.Format(td, apply.SERIALNO));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "儀器名稱"));
                        if (apply.ICHIUNIQUEID == Define.OTHER)
                        {
                            sb.Append(string.Format(td, apply.ICHIREMARK));
                        }
                        else
                        {
                            var ichi = db.QA_ICHI.FirstOrDefault(x => x.UNIQUEID == apply.ICHIUNIQUEID);

                            if (ichi != null)
                            {
                                sb.Append(string.Format(td, ichi.NAME));
                            }
                        }
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "廠牌"));
                        sb.Append(string.Format(td, apply.BRAND));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "型號"));
                        sb.Append(string.Format(td, apply.MODEL));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "校驗類別"));
                        if (apply.CAL == "Y" && apply.MSA == "Y")
                        {
                            sb.Append(string.Format(td, "校驗 + MSA"));
                        }
                        else if (!(apply.CAL == "Y") && apply.MSA == "Y")
                        {
                            sb.Append(string.Format(td, "MSA"));

                            sb.Append("MSA");
                        }
                        else if (apply.CAL == "Y" && !(apply.MSA == "Y"))
                        {
                            sb.Append(string.Format(td, "校驗"));
                        }
                        else
                        {
                            sb.Append(string.Format(td, "免校驗"));
                        }
                        sb.Append("</tr>");

                        if (!string.IsNullOrEmpty(apply.OWNERID))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "設備負責人"));

                            var owner = db.ACCOUNT.FirstOrDefault(x => x.ID == apply.OWNERID);
                            if (owner != null)
                            {
                                sb.Append(string.Format(td, apply.OWNERID, owner.NAME));
                            }
                            else
                            {
                                sb.Append(string.Format(td, apply.OWNERID));
                            }

                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(apply.OWNERMANAGERID))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "設備負責人主管"));

                            var ownerManager = db.ACCOUNT.FirstOrDefault(x => x.ID == apply.OWNERMANAGERID);
                            if (ownerManager != null)
                            {
                                sb.Append(string.Format(td, apply.OWNERMANAGERID, ownerManager.NAME));
                            }
                            else
                            {
                                sb.Append(string.Format(td, apply.OWNERMANAGERID));
                            }

                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(apply.PEID))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "製程負責人"));

                            var pe = db.ACCOUNT.FirstOrDefault(x => x.ID == apply.PEID);
                            if (pe != null)
                            {
                                sb.Append(string.Format(td, apply.PEID, pe.NAME));
                            }
                            else
                            {
                                sb.Append(string.Format(td, apply.PEID));
                            }

                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(apply.PEMANAGERID))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "製程負責人主管"));

                            var peManager = db.ACCOUNT.FirstOrDefault(x => x.ID == apply.PEMANAGERID);
                            if (peManager != null)
                            {
                                sb.Append(string.Format(td, apply.PEMANAGERID, peManager.NAME));
                            }
                            else
                            {
                                sb.Append(string.Format(td, apply.PEMANAGERID));
                            }

                            sb.Append("</tr>");
                        }

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "申請時間"));
                        sb.Append(string.Format(td, DateTimeHelper.DateTime2DateTimeStringWithSeperator(apply.CREATETIME)));
                        sb.Append("</tr>");


                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "申請人員"));
                        var createUser = db.ACCOUNT.FirstOrDefault(x => x.ID == apply.CREATORID);
                        if (createUser != null)
                        {
                            sb.Append(string.Format(td, apply.CREATORID, createUser.NAME));
                        }
                        else
                        {
                            sb.Append(string.Format(td, apply.CREATORID));
                        }
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "連結"));
                        sb.Append(string.Format(td, string.Format("<a href=\"http://ASECL-eINSPRD01/FEM.Portal/Home/Index?ReturnUrl=http://ASECL-eINSPRD01/FEM/zh-tw/Customized_ASE_QA/CalibrationApply/Index?VHNO={0}\">連結</a>", apply.VHNO)));
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

        public static void SendRejectMail(string UniqueID, string VHNO, List<string> UserList)
        {
            SendVerifyMail(UniqueID, string.Format("[退回修正通知][{0}]儀器校驗/免校驗申請單", VHNO), UserList);
        }

        private static void SendCreatedMail(string UniqueID)
        {
            try
            {
                var mailAddressList = new List<MailAddress>();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var apply = db.QA_CALIBRATIONAPPLY.First(x => x.UNIQUEID == UniqueID);

                    var createUser = db.ACCOUNT.FirstOrDefault(x => x.ID == apply.CREATORID);

                    if (createUser != null && !string.IsNullOrEmpty(createUser.EMAIL))
                    {
                        mailAddressList.Add(new MailAddress(createUser.EMAIL, createUser.NAME));
                    }
                    
                    if (mailAddressList.Count > 0)
                    {

                        var th = "<th style=\"width:150px;border:1px solid #333;padding:8px;text-align:right;color:#707070;background: #F4F4F4;\">{0}</th>";
                        var td = "<td style=\"width:400px;border:1px solid #333;padding:8px;color:#707070;\">{0}</td>";

                        var sb = new StringBuilder();

                        sb.Append("<table style=\"1px solid #ddd;font-size:13px;border-collapse:collapse;\">");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "單號"));
                        sb.Append(string.Format(td, apply.VHNO));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "部門"));
                        sb.Append(string.Format(td, OrganizationDataAccessor.GetOrganizationDescription(apply.ORGANIZATIONUNIQUEID)));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "序號"));
                        sb.Append(string.Format(td, apply.SERIALNO));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "儀器名稱"));
                        if (apply.ICHIUNIQUEID == Define.OTHER)
                        {
                            sb.Append(string.Format(td, apply.ICHIREMARK));
                        }
                        else
                        {
                            var ichi = db.QA_ICHI.FirstOrDefault(x => x.UNIQUEID == apply.ICHIUNIQUEID);

                            if (ichi != null)
                            {
                                sb.Append(string.Format(td, ichi.NAME));
                            }
                        }
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "廠牌"));
                        sb.Append(string.Format(td, apply.BRAND));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "型號"));
                        sb.Append(string.Format(td, apply.MODEL));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "校驗類別"));
                        if (apply.CAL == "Y" && apply.MSA == "Y")
                        {
                            sb.Append(string.Format(td, "校驗 + MSA"));
                        }
                        else if (!(apply.CAL == "Y") && apply.MSA == "Y")
                        {
                            sb.Append(string.Format(td, "MSA"));

                            sb.Append("MSA");
                        }
                        else if (apply.CAL == "Y" && !(apply.MSA == "Y"))
                        {
                            sb.Append(string.Format(td, "校驗"));
                        }
                        else
                        {
                            sb.Append(string.Format(td, "免校驗"));
                        }
                        sb.Append("</tr>");

                        if (!string.IsNullOrEmpty(apply.OWNERID))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "設備負責人"));

                            var owner = db.ACCOUNT.FirstOrDefault(x => x.ID == apply.OWNERID);
                            if (owner != null)
                            {
                                sb.Append(string.Format(td, apply.OWNERID, owner.NAME));
                            }
                            else
                            {
                                sb.Append(string.Format(td, apply.OWNERID));
                            }

                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(apply.OWNERMANAGERID))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "設備負責人主管"));

                            var ownerManager = db.ACCOUNT.FirstOrDefault(x => x.ID == apply.OWNERMANAGERID);
                            if (ownerManager != null)
                            {
                                sb.Append(string.Format(td, apply.OWNERMANAGERID, ownerManager.NAME));
                            }
                            else
                            {
                                sb.Append(string.Format(td, apply.OWNERMANAGERID));
                            }

                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(apply.PEID))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "製程負責人"));

                            var pe = db.ACCOUNT.FirstOrDefault(x => x.ID == apply.PEID);
                            if (pe != null)
                            {
                                sb.Append(string.Format(td, apply.PEID, pe.NAME));
                            }
                            else
                            {
                                sb.Append(string.Format(td, apply.PEID));
                            }

                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(apply.PEMANAGERID))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "製程負責人主管"));

                            var peManager = db.ACCOUNT.FirstOrDefault(x => x.ID == apply.PEMANAGERID);
                            if (peManager != null)
                            {
                                sb.Append(string.Format(td, apply.PEMANAGERID, peManager.NAME));
                            }
                            else
                            {
                                sb.Append(string.Format(td, apply.PEMANAGERID));
                            }

                            sb.Append("</tr>");
                        }

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "申請時間"));
                        sb.Append(string.Format(td, DateTimeHelper.DateTime2DateTimeStringWithSeperator(apply.CREATETIME)));
                        sb.Append("</tr>");


                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "申請人員"));
                        if (createUser != null)
                        {
                            sb.Append(string.Format(td, apply.CREATORID, createUser.NAME));
                        }
                        else
                        {
                            sb.Append(string.Format(td, apply.CREATORID));
                        }
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "連結"));
                        sb.Append(string.Format(td, string.Format("<a href=\"http://ASECL-eINSPRD01/FEM.Portal/Home/Index?ReturnUrl=http://ASECL-eINSPRD01/FEM/zh-tw/Customized_ASE_QA/CalibrationApply/Index?VHNO={0}\">連結</a>", apply.VHNO)));
                        sb.Append("</tr>");

                        MailHelper.SendMail(mailAddressList, string.Format("[立案通知][{0}]儀器校驗/免校驗申請單", apply.VHNO), sb.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }
    }
}
