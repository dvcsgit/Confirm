using DbEntity.ASE;
using Ionic.Zip;
using Models.ASE.QA.DataSync;
using Models.ASE.QA.EquipmentManagement;
using Models.ASE.QA.Shared;
using Models.Authenticated;
using NPOI.HSSF.UserModel;
using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;
using PagedList;
using System.Web.Mvc;

namespace DataAccess.ASE.QA
{
    public class EquipmentHelper
    {
        public static RequestResult UploadPhoto(string UniqueID, string Extension)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == UniqueID);

                    equipment.PHOTOEXTENSION = Extension;

                    db.SaveChanges();

                    result.Success();
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

        public static RequestResult CAL(CALFormModel Model, List<Models.Shared.Organization> OrganizationList, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                if (!Model.FormInput.EstCalDate.HasValue)
                {
                    result.ReturnFailedMessage("請選擇預計校驗日期");
                }
                else
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == Model.UniqueID);

                        if (DateTime.Compare(Model.FormInput.EstCalDate.Value, equipment.NEXTCALDATE.Value) > 0)
                        {
                            result.ReturnFailedMessage("預計校驗日期不可大於儀器下次校驗日期");
                        }
                        else
                        {
                            #region VHNO
                            var vhnoPreFix = string.Format("R{0}", Model.FormInput.EstCalDate.Value.ToString("yyyyMM").Substring(2));

                            var tmp = db.QA_CALIBRATIONNOTIFY.Where(x => x.VHNO.StartsWith(vhnoPreFix)).OrderByDescending(x => x.VHNO).ToList();

                            int vhnoSeq = 1;

                            if (tmp.Count > 0)
                            {
                                vhnoSeq = int.Parse(tmp.First().VHNO.Substring(5)) + 1;
                            }

                            var vhno = string.Format("{0}{1}", vhnoPreFix, vhnoSeq.ToString().PadLeft(4, '0'));
                            #endregion

                            var notifyUniqueID = Guid.NewGuid().ToString();

                            var time = DateTime.Now;

                            var notify = new QA_CALIBRATIONNOTIFY()
                            {
                                UNIQUEID = notifyUniqueID,
                                EQUIPMENTUNIQUEID = equipment.UNIQUEID,
                                CREATETIME = time,
                                VHNO = vhno,
                                STATUS = "1",
                                CALTYPE = equipment.CALTYPE,
                                CALUNIT = equipment.CALUNIT,
                                CASETYPE = equipment.CASETYPE,
                                ESTCALDATE = Model.FormInput.EstCalDate,
                            };

                            db.QA_CALIBRATIONNOTIFY.Add(notify);

                            db.QA_CALIBRATIONNOTIFYFLOWLOG.Add(new QA_CALIBRATIONNOTIFYFLOWLOG()
                            {
                                NOTIFYUNIQUEID = notifyUniqueID,
                                SEQ = 1,
                                FLOWSEQ = 0,
                                USERID = Account.ID,
                                NOTIFYTIME = time,
                                VERIFYTIME = time,
                                VERIFYRESULT = "Y",
                                ISCANCELED = "N"
                            });

                            db.QA_CALIBRATIONNOTIFYFLOWLOG.Add(new QA_CALIBRATIONNOTIFYFLOWLOG()
                            {
                                NOTIFYUNIQUEID = notifyUniqueID,
                                SEQ = 2,
                                FLOWSEQ = 1,
                                USERID = equipment.OWNERID,
                                NOTIFYTIME = time,
                                ISCANCELED = "N"
                            });

                            var detailList = db.QA_EQUIPMENTCALDETAIL.Where(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID).ToList();
                            var subDetailList = db.QA_EQUIPMENTCALSUBDETAIL.Where(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID).ToList();

                            db.QA_CALIBRATIONNOTIFYDETAIL.AddRange(detailList.Select(x => new QA_CALIBRATIONNOTIFYDETAIL
                            {
                                NOTIFYUNIQUEID = notifyUniqueID,
                                SEQ = x.SEQ,
                                UPPERUSINGRANGEUNITUNIQUEID = x.UPPERUSINGRANGEUNITUNIQUEID,
                                UPPERUSINGRANGEUNITREMARK = x.UPPERUSINGRANGEUNITREMARK,
                                UPPERUSINGRANGE = x.UPPERUSINGRANGE,
                                UNITUNIQUEID = x.UNITUNIQUEID,
                                UNITREMARK = x.UNITREMARK,
                                RANGETOLERANCEUNITUNIQUEID = x.RANGETOLERANCEUNITUNIQUEID,
                                RANGETOLERANCEUNITREMARK = x.RANGETOLERANCEUNITREMARK,
                                RANGETOLERANCESYMBOL = x.RANGETOLERANCESYMBOL,
                                RANGETOLERANCE = x.RANGETOLERANCE,
                                LOWERUSINGRANGEUNITUNIQUEID = x.LOWERUSINGRANGEUNITUNIQUEID,
                                CHARACTERISTICREMARK = x.CHARACTERISTICREMARK,
                                CHARACTERISTICUNIQUEID = x.CHARACTERISTICUNIQUEID,
                                LOWERUSINGRANGE = x.LOWERUSINGRANGE,
                                LOWERUSINGRANGEUNITREMARK = x.LOWERUSINGRANGEUNITREMARK
                            }).ToList());

                            db.QA_CALIBRATIONNOTIFYSUBDETAIL.AddRange(subDetailList.Select(x => new QA_CALIBRATIONNOTIFYSUBDETAIL
                            {
                                NOTIFYUNIQUEID = notifyUniqueID,
                                DETAILSEQ = x.DETAILSEQ,
                                CALIBRATIONPOINT = x.CALIBRATIONPOINT,
                                CALIBRATIONPOINTUNITUNIQUEID = x.CALIBRATIONPOINTUNITUNIQUEID,
                                SEQ = x.SEQ,
                                TOLERANCE = x.TOLERANCE,
                                TOLERANCESYMBOL = x.TOLERANCESYMBOL,
                                TOLERANCEUNITUNIQUEID = x.TOLERANCEUNITUNIQUEID
                            }).ToList());

                            equipment.NEXTCALDATE = Model.FormInput.EstCalDate;

                            db.SaveChanges();

                            CalibrationNotifyDataAccessor.SendVerifyMail(OrganizationList, equipment.OWNERID, notify);

                            result.ReturnSuccessMessage("發送校驗通知單成功");
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

        public static RequestResult MSA(MSAFormModel Model, List<Models.Shared.Organization> OrganizationList, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                if (!Model.FormInput.EstMSADate.HasValue)
                {
                    result.ReturnFailedMessage("請選擇預計MSA日期");
                }
                else
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == Model.UniqueID);

                        if (DateTime.Compare(Model.FormInput.EstMSADate.Value, equipment.NEXTMSADATE.Value) > 0)
                        {
                            result.ReturnFailedMessage("預計MSA日期不可大於儀器下次MSA日期");
                        }
                        else
                        {
                            #region VHNO
                            var vhnoPreFix = string.Format("R{0}", Model.FormInput.EstMSADate.Value.ToString("yyyyMM").Substring(2));

                            var tmp = db.QA_MSANOTIFY.Where(x => x.VHNO.StartsWith(vhnoPreFix)).OrderByDescending(x => x.VHNO).ToList();

                            int vhnoSeq = 1;

                            if (tmp.Count > 0)
                            {
                                vhnoSeq = int.Parse(tmp.First().VHNO.Substring(5)) + 1;
                            }

                            var vhno = string.Format("{0}{1}", vhnoPreFix, vhnoSeq.ToString().PadLeft(4, '0'));
                            #endregion

                            var notifyUniqueID = Guid.NewGuid().ToString();

                            var time = DateTime.Now;

                            var notify = new QA_MSANOTIFY()
                            {
                                UNIQUEID = notifyUniqueID,
                                EQUIPMENTUNIQUEID = equipment.UNIQUEID,
                                CREATETIME = time,
                                VHNO = vhno,
                                STATUS = "1",
                                ESTMSADATE = Model.FormInput.EstMSADate,
                                MSAICHIREMARK = equipment.MSAICHIREMARK,
                                MSAICHIUNIQUEID = equipment.MSAICHIUNIQUEID,
                                MSASTATIONREMARK = equipment.MSASTATIONREMARK,
                                MSASTATIONUNIQUEID = equipment.MSASTATIONUNIQUEID,
                                MSATYPE = equipment.MSATYPE,
                                MSASUBTYPE = equipment.MSASUBTYPE
                            };

                            db.QA_MSANOTIFY.Add(notify);

                            db.QA_MSANOTIFYFLOWLOG.Add(new QA_MSANOTIFYFLOWLOG()
                            {
                                NOTIFYUNIQUEID = notifyUniqueID,
                                SEQ = 1,
                                FLOWSEQ = 0,
                                USERID = Account.ID,
                                NOTIFYTIME = time,
                                VERIFYTIME = time,
                                VERIFYRESULT = "Y",
                                ISCANCELED = "N"
                            });

                            db.QA_MSANOTIFYFLOWLOG.Add(new QA_MSANOTIFYFLOWLOG()
                            {
                                NOTIFYUNIQUEID = notifyUniqueID,
                                SEQ = 2,
                                FLOWSEQ = 3,
                                USERID = equipment.PEID,
                                NOTIFYTIME = time,
                                ISCANCELED = "N"
                            });

                            var detailList = db.QA_EQUIPMENTMSADETAIL.Where(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID).ToList();

                            db.QA_MSANOTIFYDETAIL.AddRange(detailList.Select(x => new QA_MSANOTIFYDETAIL
                            {
                                NOTIFYUNIQUEID = notifyUniqueID,
                                SEQ = x.SEQ,
                                LOWERRANGE = x.LOWERRANGE,
                                MSACHARACTERISITICREMARK = x.MSACHARACTERISITICREMARK,
                                MSACHARACTERISITICUNIQUEID = x.MSACHARACTERISITICUNIQUEID,
                                MSAUNITREMARK = x.MSAUNITREMARK,
                                MSAUNITUNIQUEID = x.MSAUNITUNIQUEID,
                                UPPERRANGE = x.UPPERRANGE
                            }).ToList());

                            equipment.NEXTMSADATE = Model.FormInput.EstMSADate;

                            db.SaveChanges();

                            MSANotifyDataAccessor.SendVerifyMail(OrganizationList, equipment.PEID, notify);

                            result.ReturnSuccessMessage("發送MSA通知單成功");
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

        public static RequestResult GetQueryFormModel(List<Models.Shared.Organization> OrganizationList)
        {
            RequestResult result = new RequestResult();

            try
            {
                var factoryList = OrganizationDataAccessor.GetFactoryList(OrganizationList);

                var model = new QueryFormModel()
                {
                    FactorySelectItemList = new List<SelectListItem>() {
                     Define.DefaultSelectListItem(Resources.Resource.SelectAll)
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

        public static RequestResult Edit(EditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (!string.IsNullOrEmpty(Model.FormInput.SerialNo) && (db.QA_EQUIPMENT.Any(x =>x.UNIQUEID!=Model.UniqueID&& x.SERIALNO == Model.FormInput.SerialNo) || db.QA_CALIBRATIONAPPLY.Any(x =>x.EQUIPMENTUNIQUEID!=Model.UniqueID&& x.SERIALNO == Model.FormInput.SerialNo)))
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.SerialNo, Resources.Resource.Exists));
                    }
                    else
                    {
                        var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == Model.UniqueID);

                        equipment.BRAND = Model.FormInput.Brand;
                        equipment.MODEL = Model.FormInput.Model;
                        equipment.MACHINENO = !string.IsNullOrEmpty(Model.FormInput.MachineNo) ? Model.FormInput.MachineNo : "NA";
                        equipment.SERIALNO = !string.IsNullOrEmpty(Model.FormInput.SerialNo) ? Model.FormInput.SerialNo : "NA";
                        equipment.REMARK = !string.IsNullOrEmpty(Model.FormInput.Remark) ? Model.FormInput.Remark : "NA";

                        db.SaveChanges();

                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.Equipment, Resources.Resource.Success));
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

        public static RequestResult GetEditFormModel(List<Models.Shared.Organization> OrganizationList,string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var equipment = Get(OrganizationList, UniqueID);

                result.ReturnData(new EditFormModel()
                {
                    UniqueID = equipment.UniqueID,
                    Equipment = equipment,
                    FormInput = new FormInput()
                    {
                        Brand = equipment.Brand,
                        Model = equipment.Model,
                        Remark = equipment.Remark,
                        MachineNo = equipment.MachineNo,
                        SerialNo = equipment.SerialNo
                    },
                    CalibrationApply = CalibrationApplyDataAccessor.Query(OrganizationList, UniqueID),
                    CalibrationNotifyList = CalibrationNotifyDataAccessor.Query(OrganizationList, UniqueID),
                    CalibrationFormList = CalibrationFormDataAccessor.Query(OrganizationList, UniqueID),
                    ChangeFormList = ChangeFormDataAccessor.Query(OrganizationList, UniqueID),
                    MSANotifyList = MSANotifyDataAccessor.Query(OrganizationList, UniqueID),
                    MSAFormList = MSAForm_v2DataAccessor.Query(OrganizationList, UniqueID),
                    AbnormalFormList = AbnormalFormDataAccessor.Query(OrganizationList, UniqueID)
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

        public static RequestResult Query(List<Models.Shared.Organization> OrganizationList, QueryParameters Parameters, int PageIndex, int PageSize, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var isQA = Account.UserAuthGroupList.Contains("QA-Verify") || Account.UserAuthGroupList.Contains("QA") || Account.UserAuthGroupList.Contains("QA-FullQuery");

                var model = new GridViewModel()
                {
                    Parameters = Parameters
                };

                model.Parameters.PageIndex = PageIndex;
                model.Parameters.PageSize = PageSize;

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = (from e in db.QA_EQUIPMENT
                                 join o in db.ORGANIZATION
                                 on e.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                 join i in db.QA_ICHI
                                 on e.ICHIUNIQUEID equals i.UNIQUEID into tmpIchi
                                 from i in tmpIchi.DefaultIfEmpty()
                                 join owner in db.ACCOUNT
                                 on e.OWNERID equals owner.ID into tmpOwner
                                 from owner in tmpOwner.DefaultIfEmpty()
                                 join ownerMgr in db.ACCOUNT
                                 on e.OWNERMANAGERID equals ownerMgr.ID into tmpOwnerMgr
                                 from ownerMgr in tmpOwnerMgr.DefaultIfEmpty()
                                 join pe in db.ACCOUNT
                                 on e.PEID equals pe.ID into tmpPE
                                 from pe in tmpPE.DefaultIfEmpty()
                                 join peMgr in db.ACCOUNT
                                 on e.PEMANAGERID equals peMgr.ID into tmpPEMgr
                                 from peMgr in tmpPEMgr.DefaultIfEmpty()
                                 where Parameters.StatusList.Contains(e.STATUS)
                                 select new
                                 {
                                     UniqueID = e.UNIQUEID,
                                     OrganizationUniqueID = e.ORGANIZATIONUNIQUEID,
                                     OrganizationDescription = o.DESCRIPTION,
                                     CalNo = e.CALNO,
                                     MSACalNo = e.MSACALNO,
                                     Status = e.STATUS,
                                     IchiUniqueID = e.ICHIUNIQUEID,
                                     IchiName = i != null ? i.NAME : "",
                                     IchiRemark = e.ICHIREMARK,
                                     SerialNo = e.SERIALNO,
                                     Brand = e.BRAND,
                                     Model = e.MODEL,
                                     CAL = e.CAL,
                                     MSA = e.MSA,
                                     LastCalDate = e.LASTCALDATE,
                                     NextCalDate = e.NEXTCALDATE,
                                     LastMSADate = e.LASTMSADATE,
                                     NextMSADate = e.NEXTMSADATE,
                                     OwnerID = e.OWNERID,
                                     OwnerName = owner != null ? owner.NAME : "",
                                     OwnerManagerID = e.OWNERMANAGERID,
                                     OwnerManagerName = ownerMgr != null ? ownerMgr.NAME : "",
                                     PEID = e.PEID,
                                     PEName = pe != null ? pe.NAME : "",
                                     PEManagerID = e.PEMANAGERID,
                                     PEManagerName = peMgr != null ? peMgr.NAME : ""
                                 }).AsQueryable();

                    if (!isQA)
                    {
                        query = query.Where(x => Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID) || x.OwnerID == Account.ID || x.OwnerManagerID == Account.ID || x.PEID == Account.ID || x.PEManagerID == Account.ID).AsQueryable();
                    }

                    if (Parameters.NextCalBeginDate.HasValue)
                    {
                        query = query.Where(x =>x.NextCalDate.HasValue&& DateTime.Compare(x.NextCalDate.Value, Parameters.NextCalBeginDate.Value) >= 0);
                    }

                    if (Parameters.NextCalEndDate.HasValue)
                    {
                        query = query.Where(x =>x.NextCalDate.HasValue&& DateTime.Compare(x.NextCalDate.Value, Parameters.NextCalEndDate.Value) < 0);
                    }

                    if (Parameters.NextMSABeginDate.HasValue)
                    {
                        query = query.Where(x =>x.NextMSADate.HasValue&& DateTime.Compare(x.NextMSADate.Value, Parameters.NextMSABeginDate.Value) >= 0);
                    }

                    if (Parameters.NextMSAEndDate.HasValue)
                    {
                        query = query.Where(x =>x.NextMSADate.HasValue&& DateTime.Compare(x.NextMSADate.Value, Parameters.NextMSAEndDate.Value) < 0);
                    }

                    if (!string.IsNullOrEmpty(Parameters.CalNo))
                    {
                        query = query.Where(x => x.CalNo.Contains(Parameters.CalNo) || x.MSACalNo.Contains(Parameters.CalNo));
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

                    if (!string.IsNullOrEmpty(Parameters.Brand))
                    {
                        query = query.Where(x => x.Brand.Contains(Parameters.Brand));
                    }

                    if (!string.IsNullOrEmpty(Parameters.Model))
                    {
                        query = query.Where(x => x.Model.Contains(Parameters.Model));
                    }

                    model.ItemList = query.ToList().Select(equipment => new GridItem
                   {
                       UniqueID = equipment.UniqueID,
                       Factory = OrganizationDataAccessor.GetFactory(OrganizationList, equipment.OrganizationUniqueID),
                       OrganizationDescription = equipment.OrganizationDescription,
                       Status = equipment.Status,
                       CalNo = equipment.CalNo,
                       MSACalNo = equipment.MSACalNo,
                       IchiUniqueID = equipment.IchiUniqueID,
                       IchiName = equipment.IchiName,
                       IchiRemark = equipment.IchiRemark,
                       SerialNo = equipment.SerialNo,
                       Brand = equipment.Brand,
                       Model = equipment.Model,
                       CAL = equipment.CAL == "Y",
                       MSA = equipment.MSA == "Y",
                       LastCalDate = equipment.LastCalDate,
                       NextCalDate = equipment.NextCalDate,
                       LastMSADate = equipment.LastMSADate,
                       NextMSADate = equipment.NextMSADate,
                       OwnerID = equipment.OwnerID,
                       OwnerName = equipment.OwnerName,
                       OwnerManagerID = equipment.OwnerManagerID,
                       OwnerManagerName = equipment.OwnerManagerName,
                       PEID = equipment.PEID,
                       PEName = equipment.PEName,
                       PEManagerID = equipment.PEManagerID,
                       PEManagerName = equipment.PEManagerName,
                       IsDelay = false,
                   }).OrderBy(x => x.CalNo).ToPagedList(PageIndex, PageSize);
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

        public static RequestResult Export(List<Models.Shared.Organization> OrganizationList, QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var isQA = Account.UserAuthGroupList.Contains("QA-Verify") || Account.UserAuthGroupList.Contains("QA") || Account.UserAuthGroupList.Contains("QA-FullQuery");

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = (from e in db.QA_EQUIPMENT
                                 join o in db.ORGANIZATION
                                 on e.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                 join i in db.QA_ICHI
                                 on e.ICHIUNIQUEID equals i.UNIQUEID into tmpIchi
                                 from i in tmpIchi.DefaultIfEmpty()
                                 join owner in db.ACCOUNT
                                 on e.OWNERID equals owner.ID into tmpOwner
                                 from owner in tmpOwner.DefaultIfEmpty()
                                 join ownerMgr in db.ACCOUNT
                                 on e.OWNERMANAGERID equals ownerMgr.ID into tmpOwnerMgr
                                 from ownerMgr in tmpOwnerMgr.DefaultIfEmpty()
                                 join pe in db.ACCOUNT
                                 on e.PEID equals pe.ID into tmpPE
                                 from pe in tmpPE.DefaultIfEmpty()
                                 join peMgr in db.ACCOUNT
                                 on e.PEMANAGERID equals peMgr.ID into tmpPEMgr
                                 from peMgr in tmpPEMgr.DefaultIfEmpty()
                                 where Parameters.StatusList.Contains(e.STATUS)
                                 select new
                                 {
                                     UniqueID = e.UNIQUEID,
                                     OrganizationUniqueID = e.ORGANIZATIONUNIQUEID,
                                     OrganizationDescription = o.DESCRIPTION,
                                     CalNo = e.CALNO,
                                     MSACalNo = e.MSACALNO,
                                     Status = e.STATUS,
                                     IchiUniqueID = e.ICHIUNIQUEID,
                                     IchiName = i != null ? i.NAME : "",
                                     IchiRemark = e.ICHIREMARK,
                                     SerialNo = e.SERIALNO,
                                     Brand = e.BRAND,
                                     Model = e.MODEL,
                                     CAL = e.CAL,
                                     MSA = e.MSA,
                                     LastCalDate = e.LASTCALDATE,
                                     NextCalDate = e.NEXTCALDATE,
                                     LastMSADate = e.LASTMSADATE,
                                     NextMSADate = e.NEXTMSADATE,
                                     OwnerID = e.OWNERID,
                                     OwnerName = owner != null ? owner.NAME : "",
                                     OwnerManagerID = e.OWNERMANAGERID,
                                     OwnerManagerName = ownerMgr != null ? ownerMgr.NAME : "",
                                     PEID = e.PEID,
                                     PEName = pe != null ? pe.NAME : "",
                                     PEManagerID = e.PEMANAGERID,
                                     PEManagerName = peMgr != null ? peMgr.NAME : ""
                                 }).AsQueryable();

                    if (!isQA)
                    {
                        query = query.Where(x => Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID) || x.OwnerID == Account.ID || x.OwnerManagerID == Account.ID || x.PEID == Account.ID || x.PEManagerID == Account.ID).AsQueryable();
                    }

                    if (Parameters.NextCalBeginDate.HasValue)
                    {
                        query = query.Where(x => x.NextCalDate.HasValue && DateTime.Compare(x.NextCalDate.Value, Parameters.NextCalBeginDate.Value) >= 0);
                    }

                    if (Parameters.NextCalEndDate.HasValue)
                    {
                        query = query.Where(x => x.NextCalDate.HasValue && DateTime.Compare(x.NextCalDate.Value, Parameters.NextCalEndDate.Value) < 0);
                    }

                    if (Parameters.NextMSABeginDate.HasValue)
                    {
                        query = query.Where(x => x.NextMSADate.HasValue && DateTime.Compare(x.NextMSADate.Value, Parameters.NextMSABeginDate.Value) >= 0);
                    }

                    if (Parameters.NextMSAEndDate.HasValue)
                    {
                        query = query.Where(x => x.NextMSADate.HasValue && DateTime.Compare(x.NextMSADate.Value, Parameters.NextMSAEndDate.Value) < 0);
                    }

                    if (!string.IsNullOrEmpty(Parameters.CalNo))
                    {
                        query = query.Where(x => x.CalNo.Contains(Parameters.CalNo) || x.MSACalNo.Contains(Parameters.CalNo));
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

                    if (!string.IsNullOrEmpty(Parameters.Brand))
                    {
                        query = query.Where(x => x.Brand.Contains(Parameters.Brand));
                    }

                    if (!string.IsNullOrEmpty(Parameters.Model))
                    {
                        query = query.Where(x => x.Model.Contains(Parameters.Model));
                    }

                    var itemList = query.ToList().Select(equipment => new GridItem
                    {
                        UniqueID = equipment.UniqueID,
                        Factory = OrganizationDataAccessor.GetFactory(OrganizationList, equipment.OrganizationUniqueID),
                        OrganizationDescription = equipment.OrganizationDescription,
                        Status = equipment.Status,
                        CalNo = equipment.CalNo,
                        MSACalNo = equipment.MSACalNo,
                        IchiUniqueID = equipment.IchiUniqueID,
                        IchiName = equipment.IchiName,
                        IchiRemark = equipment.IchiRemark,
                        SerialNo = equipment.SerialNo,
                        Brand = equipment.Brand,
                        Model = equipment.Model,
                        CAL = equipment.CAL == "Y",
                        MSA = equipment.MSA == "Y",
                        LastCalDate = equipment.LastCalDate,
                        NextCalDate = equipment.NextCalDate,
                        LastMSADate = equipment.LastMSADate,
                        NextMSADate = equipment.NextMSADate,
                        OwnerID = equipment.OwnerID,
                        OwnerName = equipment.OwnerName,
                        OwnerManagerID = equipment.OwnerManagerID,
                        OwnerManagerName = equipment.OwnerManagerName,
                        PEID = equipment.PEID,
                        PEName = equipment.PEName,
                        PEManagerID = equipment.PEManagerID,
                        PEManagerName = equipment.PEManagerName,
                        IsDelay = false,
                    }).OrderBy(x => x.CalNo).ToList();

                    using (ExcelHelper helper = new ExcelHelper(string.Format("設備資料匯出_{0}", DateTimeHelper.DateTime2DateTimeString(DateTime.Now)), Define.EnumExcelVersion._2007))
                    {
                        helper.CreateSheet<GridItem>(itemList);

                        result.ReturnData(helper.Export());
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

        public static EquipmentModel Get(List<Models.Shared.Organization> OrganizationList, string UniqueID)
        {
            var model = new EquipmentModel();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var equipment = (from e in db.QA_EQUIPMENT
                                     join i in db.QA_ICHI
                                     on e.ICHIUNIQUEID equals i.UNIQUEID into tmpIchi
                                     from i in tmpIchi.DefaultIfEmpty()
                                     join msaStation in db.QA_MSASTATION
                                     on e.MSASTATIONUNIQUEID equals msaStation.UNIQUEID into tmpMSAStation
                                     from msaStation in tmpMSAStation.DefaultIfEmpty()
                                     join msaIchi in db.QA_MSAICHI
                                     on e.MSAICHIUNIQUEID equals msaIchi.UNIQUEID into tmpMSAIchi
                                     from msaIchi in tmpMSAIchi.DefaultIfEmpty()
                                     join owner in db.ACCOUNT
                                     on e.OWNERID equals owner.ID into tmpOwner
                                     from owner in tmpOwner.DefaultIfEmpty()
                                     join ownerMgr in db.ACCOUNT
                                     on e.OWNERMANAGERID equals ownerMgr.ID into tmpOwnerMgr
                                     from ownerMgr in tmpOwnerMgr.DefaultIfEmpty()
                                     join pe in db.ACCOUNT
                                     on e.PEID equals pe.ID into tmpPE
                                     from pe in tmpPE.DefaultIfEmpty()
                                     join peMgr in db.ACCOUNT
                                     on e.PEMANAGERID equals peMgr.ID into tmpPEMgr
                                     from peMgr in tmpPEMgr.DefaultIfEmpty()
                                     where e.UNIQUEID == UniqueID
                                     select new
                                     {
                                         UniqueID = e.UNIQUEID,
                                         OrganizationUniqueID = e.ORGANIZATIONUNIQUEID,
                                         CalNo = e.CALNO,
                                         MSACalNo = e.MSACALNO,
                                         Status = e.STATUS,
                                         FactoryID = e.FACTORYID,
                                         IchiType = e.ICHITYPE,
                                         IchiUniqueID = e.ICHIUNIQUEID,
                                         IchiName = i != null ? i.NAME : "",
                                         IchiRemark = e.ICHIREMARK,
                                         CharacteristicType = e.CHARACTERISTICTYPE,
                                         SerialNo = e.SERIALNO,
                                         MachineNo = e.MACHINENO,
                                         Spec = e.SPEC,
                                         Brand = e.BRAND,
                                         Model = e.MODEL,
                                         CAL = e.CAL,
                                         MSA = e.MSA,
                                         CalCycle = e.CALCYCLE,
                                         MSACycle = e.MSACYCLE,
                                         //Cycle = e.CYCLE,
                                         LastCalDate = e.LASTCALDATE,
                                         NextCalDate = e.NEXTCALDATE,
                                         LastMSADate = e.LASTMSADATE,
                                         NextMSADate = e.NEXTMSADATE,
                                         Extension = e.PHOTOEXTENSION,
                                         Remark = e.REMARK,
                                         OwnerID = e.OWNERID,
                                         OwnerName = owner != null ? owner.NAME : "",
                                         OwnerManagerID = e.OWNERMANAGERID,
                                         OwnerManagerName = ownerMgr != null ? ownerMgr.NAME : "",
                                         PEID = e.PEID,
                                         PEName = pe != null ? pe.NAME : "",
                                         PEManagerID = e.PEMANAGERID,
                                         PEManagerName = peMgr != null ? peMgr.NAME : "",
                                         MSAType = e.MSATYPE,
                                         MSASubType = e.MSASUBTYPE,
                                         MSAStationUniqueID = e.MSASTATIONUNIQUEID,
                                         MSAStationName = msaStation != null ? msaStation.NAME : "",
                                         MSAStationRemark = e.MSASTATIONREMARK,
                                         MSAIchiUniqueID = e.MSAICHIUNIQUEID,
                                         MSAIchiName = msaIchi != null ? msaIchi.NAME : "",
                                         MSAIchiRemark = e.MSAICHIREMARK
                                     }).FirstOrDefault();

                    if (equipment != null)
                    {
                        var organization = OrganizationDataAccessor.GetOrganization(equipment.OrganizationUniqueID);

                        model = new EquipmentModel()
                        {
                            UniqueID = equipment.UniqueID,
                            Factory = OrganizationDataAccessor.GetFactory(OrganizationList, equipment.OrganizationUniqueID),
                            OrganizationUniqueID = equipment.OrganizationUniqueID,
                            OrganizationDescription = organization.Description,
                            OrganizationFullDescription = organization.FullDescription,
                            Status = equipment.Status,
                            CalNo = equipment.CalNo,
                            MSACalNo = equipment.MSACalNo,
                            FactoryID = equipment.FactoryID,
                            IchiType = equipment.IchiType,
                            CharacteristicType = equipment.CharacteristicType,
                            IchiUniqueID = equipment.IchiUniqueID,
                            IchiName = equipment.IchiName,
                            IchiRemark = equipment.IchiRemark,
                            SerialNo = equipment.SerialNo,
                            MachineNo = equipment.MachineNo,
                            Spec = equipment.Spec,
                            Brand = equipment.Brand,
                            Model = equipment.Model,
                            CalCycle = equipment.CalCycle,
                            MSACycle = equipment.MSACycle,
                            CAL = equipment.CAL == "Y",
                            MSA = equipment.MSA == "Y",
                            LastCalDate = equipment.LastCalDate,
                            NextCalDate = equipment.NextCalDate,
                            LastMSADate = equipment.LastMSADate,
                            NextMSADate = equipment.NextMSADate,
                            Remark = equipment.Remark,
                            Extension = equipment.Extension,
                            OwnerID = equipment.OwnerID,
                            OwnerName = equipment.OwnerName,
                            OwnerManagerID = equipment.OwnerManagerID,
                            OwnerManagerName = equipment.OwnerManagerName,
                            PEID = equipment.PEID,
                            PEName = equipment.PEName,
                            PEManagerID = equipment.PEManagerID,
                            PEManagerName = equipment.PEManagerName,
                            MSAType = equipment.MSAType,
                            MSASubType = equipment.MSASubType,
                            MSAStationUniqueID = equipment.MSAStationUniqueID,
                            MSAStationName = equipment.MSAStationName,
                            MSAStationRemark = equipment.MSAStationRemark,
                            MSAIchiUniqueID = equipment.MSAIchiUniqueID,
                            MSAIchiName = equipment.MSAIchiName,
                            MSAIchiRemark = equipment.MSAIchiRemark
                        };

                        var query = (from f in db.QA_CALIBRATIONFORM
                                     join a in db.QA_CALIBRATIONAPPLY
                                     on f.APPLYUNIQUEID equals a.UNIQUEID
                                     where a.EQUIPMENTUNIQUEID == equipment.UniqueID && DateTime.Compare(f.ESTCALDATE, DateTime.Today) < 0 && (f.STATUS == "0" || f.STATUS == "1" || f.STATUS == "4" || f.STATUS == "6" || f.STATUS == "8")
                                     select f).ToList();

                        if (query.Count > 0)
                        {
                            if (query.Any(x => x.STATUS == "0" || x.STATUS == "1" || x.STATUS == "4" || x.STATUS == "6"))
                            {
                                model.IsDelay = true;
                            }
                            else
                            {
                                //Implement(Abnormal).....
                            }
                        }
                        else
                        {
                            model.IsDelay = false;
                        }
                    }
                    else
                    {
                        model = null;
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

        public static RequestResult GetPhoto(string CALNO)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var equipment = db.QA_EQUIPMENT.FirstOrDefault(x => x.CALNO == CALNO);

                    if (equipment != null)
                    {
                        if (!string.IsNullOrEmpty(equipment.PHOTOEXTENSION))
                        {
                            result.ReturnData(new PhotoModel()
                            {
                                UniqueID = equipment.UNIQUEID,
                                Extension = equipment.PHOTOEXTENSION
                            });
                        }
                        else
                        {
                            result.Failed();
                        }
                    }
                    else
                    {
                        result.Failed();
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

        public static RequestResult Create(string Folder, string CALNO)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var equipment = db.QA_EQUIPMENT.FirstOrDefault(x => x.CALNO == CALNO);

                    if (equipment != null)
                    {
                        var extractPath = Path.Combine(Folder, "Extract");

                        Directory.CreateDirectory(extractPath);

                        using (var zip = ZipFile.Read(Path.Combine(Folder, "EquipmentPhoto.zip")))
                        {
                            foreach (var entry in zip)
                            {
                                entry.Extract(extractPath);
                            }
                        }

                        var file = Directory.GetFiles(extractPath)[0];

                        var extension = new FileInfo(file).Extension.Substring(1);

                        var photoName = string.Format("{0}.{1}", equipment.UNIQUEID, extension);

                        File.Copy(file, Path.Combine(Config.QAFileFolderPath, photoName), true);

                        equipment.PHOTOEXTENSION = extension;

                        db.SaveChanges();

                        result.Success();
                    }
                    else
                    {
                        result.Failed();
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

        public static RequestResult ExportQRCode(List<Models.Shared.Organization> OrganizationList, List<Models.Shared.UserModel> UserList, List<string> UniqueIDList, Account Account, Define.EnumExcelVersion ExcelVersion, string fileName)
        {
            RequestResult result = new RequestResult();

            try
            {
                IWorkbook wk = null;


                if (ExcelVersion == Define.EnumExcelVersion._2003)
                {
                    wk = new HSSFWorkbook();
                }
                else
                {
                    wk = new XSSFWorkbook();
                }

                ISheet sheet = wk.CreateSheet("QRCODE");
                ISheet sheet2 = wk.CreateSheet("QRCODE_NG");

                List<Models.ASE.QA.CalibrationForm.QRCodeItem> dataSource = new List<Models.ASE.QA.CalibrationForm.QRCodeItem>();
                #region fetching data
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = (from e in db.QA_EQUIPMENT
                                 join calibrator in db.ACCOUNT
                                 on e.LASTCALUSERID equals calibrator.ID into tmpCalibrator
                                 from calibrator in tmpCalibrator.DefaultIfEmpty()
                                 where UniqueIDList.Contains(e.UNIQUEID)
                                 select new
                                 {
                                     e.CALNO,
                                     e.STATUS,
                                     e.LASTCALDATE,
                                     e.NEXTCALDATE,
                                     CalibratorName = calibrator != null ? calibrator.NAME : ""
                                 }).OrderBy(x => x.CALNO).ToList();
                    //var query = db.QA_EQUIPMENT.Where(x => UniqueIDList.Contains(x.UNIQUEID)).ToList();
                   
                    foreach (var q in query)
                    {
                        //var sign = q.LASTCALUSERID;

                        //var a = UserList.FirstOrDefault(x => x.ID == q.LASTCALUSERID);

                        //if (a != null)
                        //{
                        //    var user = new Models.ASE.Shared.ASEUserModel()
                        //    {
                        //        ID = a.ID,
                        //        Name = a.Name,
                        //        Email = a.Email,
                        //        OrganizationDescription = a.OrganizationDescription
                        //    };

                        //    sign = !string.IsNullOrEmpty(user.EName) ? user.EName.Replace("_", " ") : user.Name.Replace("_", " ");
                        //}

                        dataSource.Add(new Models.ASE.QA.CalibrationForm.QRCodeItem()
                        {
                            SN = q.CALNO,
                            CALDate = DateTimeHelper.DateTime2DateStringWithSeperator(q.LASTCALDATE),
                            DueDate = DateTimeHelper.DateTime2DateStringWithSeperator(q.NEXTCALDATE),
                            //Sign = sign,
                            Sign = q.CalibratorName,
                            IsFailed = q.STATUS == "1" ? false : true
                        });
                    }
                }
                #endregion

                CalibrationFormDataAccessor.processQRCodeSheet1(wk, sheet, dataSource.Where(x => x.IsFailed == false).ToList());
                CalibrationFormDataAccessor.processQRCodeSheet2(OrganizationList, wk, sheet2, dataSource.Where(x => x.IsFailed == true).ToList());

                // Setting print margin
                sheet.SetMargin(MarginType.TopMargin, 0);
                sheet.SetMargin(MarginType.RightMargin, 0);
                sheet.SetMargin(MarginType.LeftMargin, 0);
                sheet.SetMargin(MarginType.BottomMargin, 0);
                sheet.SetMargin(MarginType.HeaderMargin, 0);
                sheet.SetMargin(MarginType.HeaderMargin, 0);

                sheet2.SetMargin(MarginType.TopMargin, 0);
                sheet2.SetMargin(MarginType.RightMargin, 0);
                sheet2.SetMargin(MarginType.LeftMargin, 0);
                sheet2.SetMargin(MarginType.BottomMargin, 0);
                sheet2.SetMargin(MarginType.HeaderMargin, 0);
                sheet2.SetMargin(MarginType.HeaderMargin, 0);


                // Use reflection go call internal method GetCTWorksheet()
                MethodInfo methodInfo = sheet.GetType().GetMethod("GetCTWorksheet", BindingFlags.NonPublic | BindingFlags.Instance);
                var ct = (CT_Worksheet)methodInfo.Invoke(sheet, new object[] { });

                CT_SheetView view = ct.sheetViews.GetSheetViewArray(0);
                view.view = ST_SheetViewType.pageBreakPreview;

                var ct2 = (CT_Worksheet)methodInfo.Invoke(sheet2, new object[] { });
                CT_SheetView view2 = ct2.sheetViews.GetSheetViewArray(0);
                view2.view = ST_SheetViewType.pageBreakPreview;

                //Save file
                var savePath = Path.Combine(Config.TempFolder, fileName);
                using (FileStream file = new FileStream(savePath, FileMode.Create))
                {
                    wk.Write(file);
                    file.Close();
                }

                result.Data = fileName;
                result.IsSuccess = true;
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
