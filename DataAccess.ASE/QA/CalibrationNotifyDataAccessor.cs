using Models.ASE.QA.CalibrationNotify;
using DataAccess;
using Models.Authenticated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;
using DbEntity.ASE;
using System.Web.Mvc;
using System.Transactions;
using System.Net.Mail;

namespace DataAccess.ASE.QA
{
    public class CalibrationNotifyDataAccessor
    {
        public static RequestResult Export(GridViewModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ExcelHelper helper = new ExcelHelper("校驗通知單", Define.EnumExcelVersion._2007))
                {
                    helper.CreateSheet<GridItem>(Model.ItemList);

                    result.ReturnData(helper.Export());
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
                    var query = (from x in db.QA_CALIBRATIONNOTIFY
                                 join e in db.QA_EQUIPMENT
                                 on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                 join o in db.ORGANIZATION
                                 on e.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                 join i in db.QA_ICHI on e.ICHIUNIQUEID equals i.UNIQUEID into tmpIchi
                                 from i in tmpIchi.DefaultIfEmpty()
                                 where x.EQUIPMENTUNIQUEID == EquipmentUniqueID
                                 select new
                                 {
                                     UniqueID = x.UNIQUEID,
                                     CalNo = e.CALNO,
                                     VHNO = x.VHNO,
                                     Status = x.STATUS,
                                     OrganizationUniqueID = e.ORGANIZATIONUNIQUEID,
                                     OrganizationDescription = o.DESCRIPTION,
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
                                     x.ESTCALDATE
                                 }).AsQueryable();

                    model = query.ToList().Select(x => new GridItem
                    {
                        UniqueID = x.UniqueID,
                        VHNO = x.VHNO,
                        Status = new NotifyStatus(x.Status),
                        CalNo = x.CalNo,
                        EstCalibrateDate = x.ESTCALDATE,
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
                        LogList = db.QA_CALIBRATIONNOTIFYFLOWLOG.Where(l => l.NOTIFYUNIQUEID == x.UniqueID && l.ISCANCELED == "N" && !l.VERIFYTIME.HasValue).Select(l => new LogModel
                        {
                            Seq = l.SEQ,
                            FlowSeq = l.FLOWSEQ,
                            NotifyTime = l.NOTIFYTIME,
                            VerifyResult = l.VERIFYRESULT,
                            UserID = l.USERID,
                            VerifyTime = l.VERIFYTIME,
                            VerifyComment = l.VERIFYCOMMENT,
                        }).ToList()
                    }).OrderBy(x => x.EstCalibrateDate).ThenBy(x => x.CreateTime).ToList();
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
                    var query = (from x in db.QA_CALIBRATIONNOTIFY
                                 join e in db.QA_EQUIPMENT
                                 on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                 join owner in db.ACCOUNT
                                 on e.OWNERID equals owner.ID into tmpOwner
                                 from owner in tmpOwner
                                 join o in db.ORGANIZATION
                                 on e.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                 join i in db.QA_ICHI on e.ICHIUNIQUEID equals i.UNIQUEID into tmpIchi
                                 from i in tmpIchi.DefaultIfEmpty()
                                 where Parameters.StatusList.Contains(x.STATUS)
                                 select new
                                 {
                                     UniqueID = x.UNIQUEID,
                                     CalNo = e.CALNO,
                                     VHNO = x.VHNO,
                                     Status = x.STATUS,
                                     OrganizationUniqueID = e.ORGANIZATIONUNIQUEID,
                                     OrganizationDescription = o.DESCRIPTION,
                                     SerialNo = e.SERIALNO,
                                     IchiUniqueID = e.ICHIUNIQUEID,
                                     IchiName = i != null ? i.NAME : "",
                                     IchiRemark = e.ICHIREMARK,
                                     Brand = e.BRAND,
                                     Model = e.MODEL,
                                     CreateTime = x.CREATETIME,
                                     OwnerID = e.OWNERID,
                                     OwnerName = owner!=null?owner.NAME:"",
                                     OwnerManagerID = e.OWNERMANAGERID,
                                     PEID = e.PEID,
                                     PEManagerID = e.PEMANAGERID,
                                     x.ESTCALDATE
                                 }).AsQueryable();

                    if (!isQA)
                    {
                        query = query.Where(x => Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID) || x.OwnerID == Account.ID || x.OwnerManagerID == Account.ID || x.PEID == Account.ID || x.PEManagerID == Account.ID).AsQueryable();
                    }

                    if (Parameters.BeginDate.HasValue)
                    {
                        query = query.Where(x => DateTime.Compare(x.ESTCALDATE.Value, Parameters.BeginDate.Value) >= 0);
                    }

                    if (Parameters.EndDate.HasValue)
                    {
                        query = query.Where(x => DateTime.Compare(x.ESTCALDATE.Value, Parameters.EndDate.Value) < 0);
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
                        EstCalibrateDate = x.ESTCALDATE,
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
                        OwnerName = x.OwnerName,
                        OwnerManagerID = x.OwnerManagerID,
                        PEID = x.PEID,
                        PEManagerID = x.PEManagerID,
                        Account = Account,
                        LogList = db.QA_CALIBRATIONNOTIFYFLOWLOG.Where(l => l.NOTIFYUNIQUEID == x.UniqueID && l.ISCANCELED == "N" && !l.VERIFYTIME.HasValue).Select(l => new LogModel
                        {
                            Seq = l.SEQ,
                            FlowSeq = l.FLOWSEQ,
                            NotifyTime = l.NOTIFYTIME,
                            VerifyResult = l.VERIFYRESULT,
                            UserID = l.USERID,
                            VerifyTime = l.VERIFYTIME,
                            VerifyComment = l.VERIFYCOMMENT,
                        }).ToList()
                    }).OrderBy(x => x.Seq).ThenBy(x => x.StatusSeq).ThenBy(x => x.EstCalibrateDateString).ToList();
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

        public static RequestResult GetEditFormModel(List<Models.Shared.Organization> OrganizationList, string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var notify = (from x in db.QA_CALIBRATIONNOTIFY
                                  join e in db.QA_EQUIPMENT
                                  on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
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
                                  where x.UNIQUEID == UniqueID
                                  select new
                                  {
                                      x.UNIQUEID,
                                      x.VHNO,
                                      x.STATUS,
                                      e.ORGANIZATIONUNIQUEID,
                                      EquipmentUniqueID = e.UNIQUEID,
                                      x.CREATETIME,
                                      e.FACTORYID,
                                      e.CALNO,
                                      e.ICHITYPE,
                                      e.ICHIUNIQUEID,
                                      IchiName = i != null ? i.NAME : "",
                                      e.ICHIREMARK,
                                      e.CHARACTERISTICTYPE,
                                      e.SERIALNO,
                                      e.MACHINENO,
                                      e.BRAND,
                                      e.MODEL,
                                      e.SPEC,
                                      OwnerID = e.OWNERID,
                                      OwnerName = owner != null ? owner.NAME : "",
                                      OwnerManagerID = e.OWNERMANAGERID,
                                      OwnerManagerName = ownerMgr != null ? ownerMgr.NAME : "",
                                      PEID = e.PEID,
                                      PEName = pe != null ? pe.NAME : "",
                                      PEManagerID = e.PEMANAGERID,
                                      PEManagerName = peMgr != null ? peMgr.NAME : "",
                                      e.CALCYCLE,
                                      e.REMARK
                                  }).First();

                    var form = db.QA_CALIBRATIONFORM.FirstOrDefault(x => x.NOTIFYUNIQUEID == notify.UNIQUEID);

                    var model = new EditFormModel()
                    {
                        UniqueID = notify.UNIQUEID,
                        FormUniqueID = form!=null?form.UNIQUEID:"",
                        FormVHNO = form!=null?form.VHNO:"",
                        VHNO = notify.VHNO,
                        Status = notify.STATUS,
                        Equipment = EquipmentHelper.Get(OrganizationList, notify.EquipmentUniqueID),
                        CalNo=notify.CALNO,
                        Factory = OrganizationDataAccessor.GetFactory(OrganizationList, notify.ORGANIZATIONUNIQUEID),
                        Department = OrganizationDataAccessor.GetOrganizationFullDescription(notify.ORGANIZATIONUNIQUEID),
                        CreateTime = notify.CREATETIME,
                        FactoryID = notify.FACTORYID,
                        IchiType = notify.ICHITYPE,
                        SerialNo = notify.SERIALNO,
                        MachineNo = notify.MACHINENO,
                        Brand = notify.BRAND,
                        Model = notify.MODEL,
                        Spec = notify.SPEC,
                        Cycle = notify.CALCYCLE.Value,
                        OwnerID = notify.OwnerID,
                        OwnerName = notify.OwnerName,
                        OwnerManagerID = notify.OwnerManagerID,
                        OwnerManagerName = notify.OwnerManagerName,
                        PEID = notify.PEID,
                        PEName = notify.PEName,
                        PEManagerID = notify.PEManagerID,
                        PEManagerName = notify.PEManagerName,
                        Remark = notify.REMARK,
                        IchiSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        },
                        CharacteristicTypeSelectItemList = new List<SelectListItem>()
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        },
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
                            IchiUniqueID = notify.ICHIUNIQUEID,
                            IchiRemark = notify.ICHIREMARK,
                            CharacteristicType = notify.CHARACTERISTICTYPE,
                            Spec = notify.SPEC
                        },
                        LogList = (from log in db.QA_CALIBRATIONNOTIFYFLOWLOG
                                   join user in db.ACCOUNT
                                   on log.USERID equals user.ID into tmpUser
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
                                   }).OrderBy(x => x.VerifyTime).ThenBy(x => x.Seq).ToList(),
                        ItemList = (from detail in db.QA_CALIBRATIONNOTIFYDETAIL
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
                                    where detail.NOTIFYUNIQUEID == notify.UNIQUEID
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
                                        ItemList = (from subDetail in db.QA_CALIBRATIONNOTIFYSUBDETAIL
                                                    join calibrationPointUnit in db.QA_UNIT
                                                    on subDetail.CALIBRATIONPOINTUNITUNIQUEID equals calibrationPointUnit.UNIQUEID into tmpCalibrationUnit
                                                    from calibrationPointUnit in tmpCalibrationUnit.DefaultIfEmpty()
                                                    join toleranceUnit in db.QA_TOLERANCEUNIT
                                                    on subDetail.TOLERANCEUNITUNIQUEID equals toleranceUnit.UNIQUEID into tmpToleranceUnit
                                                    from toleranceUnit in tmpToleranceUnit.DefaultIfEmpty()
                                                    where subDetail.NOTIFYUNIQUEID == notify.UNIQUEID && subDetail.DETAILSEQ == detail.SEQ
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

                    var ichiList = db.QA_ICHI.Where(x => x.TYPE == model.IchiType).OrderBy(x => x.NAME).ToList();

                    foreach (var ichi in ichiList)
                    {
                        model.IchiSelectItemList.Add(new SelectListItem()
                        {
                            Value = ichi.UNIQUEID,
                            Text = ichi.NAME
                        });

                        if (model.FormInput.IchiUniqueID == ichi.UNIQUEID)
                        {
                            var characteristicTypeList = model.CharacteristicTypeList.Where(x => x.IchiUniqueID == ichi.UNIQUEID).OrderBy(x => x.Type).ToList();

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

        public static RequestResult Edit(List<Models.Shared.Organization> OrganizationList, EditFormModel Model, Account Account)
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
                    if ((Model.ItemList == null || Model.ItemList.Count == 0))
                    {
                        result.ReturnFailedMessage("請建立校驗資訊");
                    }
                    else
                    {
                        var notify = db.QA_CALIBRATIONNOTIFY.First(x => x.UNIQUEID == Model.UniqueID);

                        notify.STATUS = "1";

                        db.SaveChanges();

                        db.QA_CALIBRATIONNOTIFYDETAIL.RemoveRange(db.QA_CALIBRATIONNOTIFYDETAIL.Where(x => x.NOTIFYUNIQUEID == notify.UNIQUEID).ToList());
                        db.QA_CALIBRATIONNOTIFYSUBDETAIL.RemoveRange(db.QA_CALIBRATIONNOTIFYSUBDETAIL.Where(x => x.NOTIFYUNIQUEID == notify.UNIQUEID).ToList());

                        db.SaveChanges();

                        var itemSeq = 1;

                        foreach (var item in Model.ItemList.OrderBy(x => x.Characteristic))
                        {
                            db.QA_CALIBRATIONNOTIFYDETAIL.Add(new QA_CALIBRATIONNOTIFYDETAIL()
                            {
                                NOTIFYUNIQUEID = notify.UNIQUEID,
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
                                db.QA_CALIBRATIONNOTIFYSUBDETAIL.Add(new QA_CALIBRATIONNOTIFYSUBDETAIL()
                                {
                                    NOTIFYUNIQUEID = notify.UNIQUEID,
                                    DETAILSEQ = itemSeq,
                                    SEQ = subItemSeq,
                                    CALIBRATIONPOINT = subitem.CalibrationPoint,
                                    CALIBRATIONPOINTUNITUNIQUEID = subitem.CalibrationPointUnitUniqueID,
                                    TOLERANCESYMBOL = subitem.ToleranceSymbol,
                                    TOLERANCE = subitem.Tolerance,
                                    TOLERANCEUNITUNIQUEID = subitem.ToleranceUnitUniqueID
                                });

                                subItemSeq++;
                            }

                            itemSeq++;
                        }

                        var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == notify.EQUIPMENTUNIQUEID);

                        equipment.ICHIUNIQUEID = Model.FormInput.IchiUniqueID;
                        equipment.ICHIREMARK = Model.FormInput.IchiRemark;
                        equipment.CHARACTERISTICTYPE = Model.FormInput.CharacteristicType;
                        equipment.SPEC = Model.FormInput.Spec;

                        db.SaveChanges();

                        var time = DateTime.Now;

                        var log = db.QA_CALIBRATIONNOTIFYFLOWLOG.FirstOrDefault(x => x.NOTIFYUNIQUEID == notify.UNIQUEID && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue && x.FLOWSEQ == 0);

                        if (log != null)
                        {
                            log.VERIFYRESULT = "Y";
                            log.USERID = Account.ID;
                            log.VERIFYTIME = time;

                            db.SaveChanges();
                        }

                        log = db.QA_CALIBRATIONNOTIFYFLOWLOG.FirstOrDefault(x => x.NOTIFYUNIQUEID == notify.UNIQUEID && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue && x.FLOWSEQ == 1);

                        if (log != null)
                        {
                            log.VERIFYRESULT = "Y";
                            log.USERID = Account.ID;
                            log.VERIFYTIME = time;

                            db.SaveChanges();
                        }

                        var logSeq = db.QA_CALIBRATIONNOTIFYFLOWLOG.Where(x => x.NOTIFYUNIQUEID == notify.UNIQUEID).Max(x => x.SEQ) + 1;

                        //db.QA_CALIBRATIONNOTIFYFLOWLOG.Add(new QA_CALIBRATIONNOTIFYFLOWLOG()
                        //{
                        //    NOTIFYUNIQUEID = notify.UNIQUEID,
                        //    SEQ = logSeq,
                        //    FLOWSEQ = 1,
                        //    USERID = equipment.OWNERID,
                        //    NOTIFYTIME = time,
                        //    VERIFYTIME = time,
                        //    VERIFYRESULT = "Y",
                        //    ISCANCELED = "N"
                        //});

                        //logSeq++;

                        db.QA_CALIBRATIONNOTIFYFLOWLOG.Add(new QA_CALIBRATIONNOTIFYFLOWLOG()
                        {
                            NOTIFYUNIQUEID = notify.UNIQUEID,
                            SEQ = logSeq,
                            FLOWSEQ = 2,
                            USERID = equipment.OWNERMANAGERID,
                            NOTIFYTIME = time,
                            ISCANCELED = "N"
                        });

                        logSeq++;

                        SendVerifyMail(OrganizationList, equipment.OWNERMANAGERID, notify);

                        if (!string.IsNullOrEmpty(equipment.PEID))
                        {
                            db.QA_CALIBRATIONNOTIFYFLOWLOG.Add(new QA_CALIBRATIONNOTIFYFLOWLOG()
                            {
                                NOTIFYUNIQUEID = notify.UNIQUEID,
                                SEQ = logSeq,
                                FLOWSEQ = 3,
                                USERID = equipment.PEID,
                                NOTIFYTIME = time,
                                ISCANCELED = "N"
                            });

                            logSeq++;

                            SendVerifyMail(OrganizationList, equipment.PEID, notify);
                        }


                        db.SaveChanges();

                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, "儀器校驗通知單", Resources.Resource.Success));
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

        public static RequestResult GetQAFormModel(List<Models.Shared.Organization> OrganizationList, string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new QAFormModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var notify = db.QA_CALIBRATIONNOTIFY.First(x=>x.UNIQUEID==UniqueID);

                    var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == notify.EQUIPMENTUNIQUEID);

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
                            EstCalibrateDateString = DateTimeHelper.DateTime2DateStringWithSeperator(notify.ESTCALDATE),
                            CalibrateType = equipment.CALTYPE,
                            CalibrateUnit = equipment.CALUNIT,
                            CaseType = equipment.CASETYPE,
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

        public static RequestResult Approve(List<Models.Shared.Organization> OrganizationList, string UniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var notify = db.QA_CALIBRATIONNOTIFY.First(x => x.UNIQUEID == UniqueID);
                    var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == notify.EQUIPMENTUNIQUEID);

                    var log = db.QA_CALIBRATIONNOTIFYFLOWLOG.Where(x => x.NOTIFYUNIQUEID == notify.UNIQUEID && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue && x.USERID == Account.ID).OrderBy(x => x.SEQ).First();

                    log.VERIFYTIME = DateTime.Now;
                    log.VERIFYRESULT = "Y";

                    var logSeq = db.QA_CALIBRATIONNOTIFYFLOWLOG.Where(x => x.NOTIFYUNIQUEID == notify.UNIQUEID).Max(x => x.SEQ) + 1;

                    //OwnerManager
                    if (log.FLOWSEQ == 2)
                    {
                        if (!db.QA_CALIBRATIONNOTIFYFLOWLOG.Any(x => x.NOTIFYUNIQUEID == notify.UNIQUEID && x.SEQ != log.SEQ && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue))
                        {
                            db.QA_CALIBRATIONNOTIFYFLOWLOG.Add(new QA_CALIBRATIONNOTIFYFLOWLOG()
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
                    }
                    //PE
                    else if (log.FLOWSEQ == 3)
                    {
                        db.QA_CALIBRATIONNOTIFYFLOWLOG.Add(new QA_CALIBRATIONNOTIFYFLOWLOG()
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
                    //PE Manager
                    else if (log.FLOWSEQ == 4)
                    {
                        if (!db.QA_CALIBRATIONNOTIFYFLOWLOG.Any(x => x.NOTIFYUNIQUEID == notify.UNIQUEID && x.SEQ != log.SEQ && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue))
                        {
                            db.QA_CALIBRATIONNOTIFYFLOWLOG.Add(new QA_CALIBRATIONNOTIFYFLOWLOG()
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
                    var notify = db.QA_CALIBRATIONNOTIFY.First(x => x.UNIQUEID == UniqueID);
                    var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == notify.EQUIPMENTUNIQUEID);

                    notify.STATUS = "2";

                    db.SaveChanges();

                    var log = db.QA_CALIBRATIONNOTIFYFLOWLOG.Where(x => x.NOTIFYUNIQUEID == notify.UNIQUEID && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue && x.USERID == Account.ID).OrderBy(x => x.SEQ).FirstOrDefault();

                    if (log != null)
                    {
                        log.VERIFYTIME = DateTime.Now;
                        log.VERIFYRESULT = "N";
                        log.VERIFYCOMMENT = Comment;

                        db.SaveChanges();
                    }

                    var logList = db.QA_CALIBRATIONNOTIFYFLOWLOG.Where(x => x.NOTIFYUNIQUEID == UniqueID && x.SEQ != log.SEQ && !x.VERIFYTIME.HasValue).ToList();

                    foreach (var l in logList)
                    {
                        l.ISCANCELED = "Y";
                    }

                    db.SaveChanges();

                    var logSeq = db.QA_CALIBRATIONNOTIFYFLOWLOG.Where(x => x.NOTIFYUNIQUEID == notify.UNIQUEID).Max(x => x.SEQ) + 1;

                    db.QA_CALIBRATIONNOTIFYFLOWLOG.Add(new QA_CALIBRATIONNOTIFYFLOWLOG()
                    {
                        NOTIFYUNIQUEID = notify.UNIQUEID,
                        SEQ = logSeq,
                        FLOWSEQ = 1,
                        NOTIFYTIME = DateTime.Now,
                        USERID = equipment.OWNERID,
                        ISCANCELED = "N"
                    });

                    db.SaveChanges();

#if !DEBUG
                        trans.Complete();
#endif

                    SendRejectMail(OrganizationList, new List<string>() { equipment.OWNERID }, notify);
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

                    var notify = db.QA_CALIBRATIONNOTIFY.First(x => x.UNIQUEID == Model.UniqueID);
                    var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == notify.EQUIPMENTUNIQUEID);

                    notify.STATUS = "3";

                    notify.CALTYPE = Model.FormInput.CalibrateType;
                    equipment.CALTYPE = Model.FormInput.CalibrateType;

                    if (Model.FormInput.CalibrateType != "IL")
                    {
                        notify.CALUNIT = Model.FormInput.CalibrateUnit;
                        equipment.CALUNIT = Model.FormInput.CalibrateUnit;
                    }
                    else
                    {
                        notify.CALUNIT = "L";
                        equipment.CALUNIT = "L";
                    }
                    
                    notify.CASETYPE = Model.FormInput.CaseType;
                    equipment.CASETYPE = Model.FormInput.CaseType;

                    notify.CALRESPONSORID = Model.FormInput.CalibratorID;
                    //notify.ESTCALDATE = Model.FormInput.EstCalibrateDate;

                    //equipment.NEXTCALDATE = Model.FormInput.EstCalibrateDate;

                    db.SaveChanges();

                    db.QA_EQUIPMENTCALDETAIL.RemoveRange(db.QA_EQUIPMENTCALDETAIL.Where(x => x.EQUIPMENTUNIQUEID == notify.EQUIPMENTUNIQUEID).ToList());
                    db.QA_EQUIPMENTCALSUBDETAIL.RemoveRange(db.QA_EQUIPMENTCALSUBDETAIL.Where(x => x.EQUIPMENTUNIQUEID == notify.EQUIPMENTUNIQUEID).ToList());

                    db.SaveChanges();

                    var log = db.QA_CALIBRATIONNOTIFYFLOWLOG.FirstOrDefault(x => x.NOTIFYUNIQUEID == notify.UNIQUEID && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue && x.FLOWSEQ == 5);

                    if (log != null)
                    {
                        log.USERID = Account.ID;
                        log.VERIFYTIME = DateTime.Now;
                        log.VERIFYRESULT = "Y";

                        db.SaveChanges();
                    }

                    db.SaveChanges();

                    var form = db.QA_CALIBRATIONFORM.FirstOrDefault(x => x.NOTIFYUNIQUEID == notify.UNIQUEID);

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

                        db.QA_CALIBRATIONFORM.Add(new QA_CALIBRATIONFORM()
                        {
                            UNIQUEID = formUniqueID,
                            TRACEABLENO = "92_31_0000_0007",
                            VHNO = vhno,
                            NOTIFYUNIQUEID = notify.UNIQUEID,
                            STATUS = "0",
                            JOBCALRESPONSERID = Model.FormInput.CalibratorID,
                            HAVEABNORMAL = "N",
                            NOTIFYDATE = DateTime.Now,
                            ESTCALDATE = notify.ESTCALDATE.Value
                        });
                    }
                    else
                    {
                        form.STATUS = "0";
                        form.ESTCALDATE = notify.ESTCALDATE.Value;
                        form.JOBCALRESPONSERID = Model.FormInput.CalibratorID;
                        //form.CALDATE = Model.FormInput.EstCalibrateDate.Value;

                        db.QA_CALIBRATIONFORMDETAIL.RemoveRange(db.QA_CALIBRATIONFORMDETAIL.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList());
                    }

                    db.SaveChanges();

                    var detailList = (from detail in db.QA_CALIBRATIONNOTIFYDETAIL
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
                                      where detail.NOTIFYUNIQUEID == notify.UNIQUEID
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
                                          ItemList = (from subDetail in db.QA_CALIBRATIONNOTIFYSUBDETAIL
                                                      join calibrationPointUnit in db.QA_UNIT on subDetail.CALIBRATIONPOINTUNITUNIQUEID equals calibrationPointUnit.UNIQUEID into tmpCalibrationUnit
                                                      from calibrationPointUnit in tmpCalibrationUnit.DefaultIfEmpty()
                                                      join toleranceUnit in db.QA_TOLERANCEUNIT on subDetail.TOLERANCEUNITUNIQUEID equals toleranceUnit.UNIQUEID into tmpToleranceUnit
                                                      from toleranceUnit in tmpToleranceUnit.DefaultIfEmpty()
                                                      where subDetail.NOTIFYUNIQUEID == notify.UNIQUEID && subDetail.DETAILSEQ == detail.SEQ
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
                        EQUIPMENTUNIQUEID = notify.EQUIPMENTUNIQUEID,
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
                                TOLERANCE = subDetail.Tolerance,
                                TOLERANCEUNIT = subDetail.ToleranceUnitDisplay,
                                UNIT = subDetail.CalibrationPointUnitDescription,
                                TOLERANCEUNITRATE = subDetail.ToleranceUnitUniqueID == "%" ? 0 : toleranceUnit != null ? toleranceUnit.RATE.Value : 1
                            });

                            db.QA_EQUIPMENTCALSUBDETAIL.Add(new QA_EQUIPMENTCALSUBDETAIL
                            {
                                EQUIPMENTUNIQUEID = notify.EQUIPMENTUNIQUEID,
                                CALIBRATIONPOINT = subDetail.CalibrationPoint,
                                CALIBRATIONPOINTUNITUNIQUEID = subDetail.CalibrationPointUnitUniqueID,
                                DETAILSEQ = detail.Seq,
                                SEQ = subDetail.Seq,
                                TOLERANCE = subDetail.Tolerance,
                                TOLERANCESYMBOL = subDetail.ToleranceSymbol,
                                TOLERANCEUNITUNIQUEID = subDetail.ToleranceUnitUniqueID
                            });

                            seq++;
                        }
                    }

                    db.SaveChanges();

                    CalibrationFormDataAccessor.SendCreatedMail(OrganizationList, formUniqueID);

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

                    var log = db.QA_CALIBRATIONNOTIFYFLOWLOG.FirstOrDefault(x => x.NOTIFYUNIQUEID == UniqueID && x.ISCANCELED == "N" && !x.VERIFYTIME.HasValue && x.FLOWSEQ == 5);

                    if (log != null)
                    {

#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                        var notify = db.QA_CALIBRATIONNOTIFY.First(x => x.UNIQUEID == UniqueID);
                        var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == notify.EQUIPMENTUNIQUEID);

                        notify.STATUS = "2";

                        db.SaveChanges();

                        log.USERID = Account.ID;
                        log.VERIFYTIME = DateTime.Now;
                        log.VERIFYRESULT = "N";
                        log.VERIFYCOMMENT = Comment;

                        db.SaveChanges();

                        var logList = db.QA_CALIBRATIONNOTIFYFLOWLOG.Where(x => x.NOTIFYUNIQUEID == notify.UNIQUEID && x.SEQ != log.SEQ && !x.VERIFYTIME.HasValue).ToList();

                        foreach (var l in logList)
                        {
                            l.ISCANCELED = "Y";
                        }

                        db.SaveChanges();

                        var seq = db.QA_CALIBRATIONNOTIFYFLOWLOG.Where(x => x.NOTIFYUNIQUEID == notify.UNIQUEID).Max(x => x.SEQ) + 1;

                        db.QA_CALIBRATIONNOTIFYFLOWLOG.Add(new QA_CALIBRATIONNOTIFYFLOWLOG()
                        {
                            NOTIFYUNIQUEID = notify.UNIQUEID,
                            SEQ = seq,
                            FLOWSEQ = 1,
                            NOTIFYTIME = DateTime.Now,
                            USERID = equipment.OWNERID,
                            ISCANCELED = "N"
                        });

                        db.SaveChanges();

                        SendRejectMail(OrganizationList, new List<string>() { equipment.OWNERID }, notify);

#if !DEBUG
                        trans.Complete();
                    }
#endif
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
                var notify = (from a in db.QA_CALIBRATIONNOTIFY
                              join e in db.QA_EQUIPMENT
                              on a.EQUIPMENTUNIQUEID equals e.UNIQUEID
                             join ichi in db.QA_ICHI on e.ICHIUNIQUEID equals ichi.UNIQUEID into tmpIchi
                             from ichi in tmpIchi.DefaultIfEmpty()
                             join owner in db.ACCOUNT on e.OWNERID equals owner.ID into tmpOwner
                             from owner in tmpOwner.DefaultIfEmpty()
                             join ownerManager in db.ACCOUNT on e.OWNERMANAGERID equals ownerManager.ID into tmpOwnerManager
                             from ownerManager in tmpOwnerManager.DefaultIfEmpty()
                             join pe in db.ACCOUNT on e.PEID equals pe.ID into tmpPE
                             from pe in tmpPE.DefaultIfEmpty()
                             join peManager in db.ACCOUNT on e.PEMANAGERID equals peManager.ID into tmpPEManager
                             from peManager in tmpPEManager.DefaultIfEmpty()
                             join calibrator in db.ACCOUNT on a.CALRESPONSORID equals calibrator.ID into tmpCalibrator
                             from calibrator in tmpCalibrator.DefaultIfEmpty()
                             where a.UNIQUEID == UniqueID
                             select new
                             {
                                 a.UNIQUEID,
                                 a.VHNO,
                                 a.STATUS,
                                 a.CREATETIME,
                                 e.ORGANIZATIONUNIQUEID,
                                 EquipmentUniqueID=e.UNIQUEID,
                                 e.CALNO,
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
                                 e.CALCYCLE,
                                 e.REMARK,
                                 a.CALTYPE,
                                 a.CALUNIT,
                                 a.CASETYPE,
                                 a.CALRESPONSORID,
                                 CalibratorName = calibrator != null ? calibrator.NAME : "",
                                 a.ESTCALDATE
                             }).First();

                var form = db.QA_CALIBRATIONFORM.FirstOrDefault(x => x.NOTIFYUNIQUEID == notify.UNIQUEID);

                model = new FormViewModel()
                {
                    VHNO = notify.VHNO,
                    FormUniqueID = form != null ? form.UNIQUEID : "",
                    FormVHNO = form!=null?form.VHNO:"",
                    Status = new NotifyStatus(notify.STATUS),
                    CreateTime = notify.CREATETIME,
                     CalNo = notify.CALNO,
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
                    Cycle = notify.CALCYCLE.Value,
                    Remark = notify.REMARK,
                    CalibrateType = notify.CALTYPE,
                    CalibrateUnit = notify.CALUNIT,
                    CaseType = notify.CASETYPE,
                    Equipment = EquipmentHelper.Get(OrganizationList, notify.EquipmentUniqueID),
                    CalibratorID = notify.CALRESPONSORID,
                    CalibratorName = notify.CalibratorName,
                    EstCalibrateDate = notify.ESTCALDATE,
                    LogList = (from log in db.QA_CALIBRATIONNOTIFYFLOWLOG
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
                               }).OrderBy(x => x.VerifyTime).ThenBy(x => x.Seq).ToList(),
                    ItemList = (from detail in db.QA_CALIBRATIONNOTIFYDETAIL
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
                                where detail.NOTIFYUNIQUEID == notify.UNIQUEID
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
                                    ItemList = (from subDetail in db.QA_CALIBRATIONNOTIFYSUBDETAIL
                                                join calibrationPointUnit in db.QA_UNIT on subDetail.CALIBRATIONPOINTUNITUNIQUEID equals calibrationPointUnit.UNIQUEID into tmpCalibrationUnit
                                                from calibrationPointUnit in tmpCalibrationUnit.DefaultIfEmpty()
                                                join toleranceUnit in db.QA_TOLERANCEUNIT on subDetail.TOLERANCEUNITUNIQUEID equals toleranceUnit.UNIQUEID into tmpToleranceUnit
                                                from toleranceUnit in tmpToleranceUnit.DefaultIfEmpty()
                                                where subDetail.NOTIFYUNIQUEID == notify.UNIQUEID && subDetail.DETAILSEQ == detail.SEQ
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

        private static void SendVerifyMail(List<Models.Shared.Organization> OrganizationList, QA_CALIBRATIONNOTIFY Notify)
        {
            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var userList = db.USERAUTHGROUP.Where(x => x.AUTHGROUPID == "QA-Verify").Select(x => x.USERID).ToList();

                    var equipment = EquipmentHelper.Get(OrganizationList, Notify.EQUIPMENTUNIQUEID);

                    if (userList != null && userList.Count > 0)
                    {
                        SendVerifyMail(OrganizationList, userList, string.Format("[簽核通知][{0}][{1}]儀器校驗通知單", Notify.VHNO, equipment.CalNo), Notify);
                    }
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
                    var userList = db.USERAUTHGROUP.Where(x => x.AUTHGROUPID == "QA-Verify").Select(x => x.USERID).ToList();

                    var notify = db.QA_CALIBRATIONNOTIFY.First(x => x.UNIQUEID == UniqueID);

                    var equipment = EquipmentHelper.Get(null, notify.EQUIPMENTUNIQUEID);

                    if (userList != null && userList.Count > 0)
                    {
                        if (IsDelay)
                        {
                            SendVerifyMail(null, userList, string.Format("[逾期][簽核通知][{0}天][{1}][{2}]儀器校驗通知單", Days, notify.VHNO, equipment.CalNo), notify);
                        }
                        else
                        {
                            SendVerifyMail(null, userList, string.Format("[跟催][簽核通知][{0}天][{1}][{2}]儀器校驗通知單", Days, notify.VHNO, equipment.CalNo), notify);
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
                    var userList = db.USERAUTHGROUP.Where(x => x.AUTHGROUPID == "QA-Verify").Select(x => x.USERID).ToList();

                    var notify = db.QA_CALIBRATIONNOTIFY.First(x => x.UNIQUEID == UniqueID);

                    var equipment = EquipmentHelper.Get(null, notify.EQUIPMENTUNIQUEID);

                    if (userList != null && userList.Count > 0)
                    {
                        SendVerifyMail(null, userList, string.Format("[簽核通知][{0}][{1}]儀器校驗通知單", notify.VHNO, equipment.CalNo), notify);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public static void SendVerifyMail(List<Models.Shared.Organization> OrganizationList, string UserID, QA_CALIBRATIONNOTIFY Notify)
        {
            try
            {
                var equipment = EquipmentHelper.Get(OrganizationList, Notify.EQUIPMENTUNIQUEID);

                SendVerifyMail(OrganizationList, new List<string> { UserID }, string.Format("[簽核通知][{0}][{1}]儀器校驗通知單", Notify.VHNO, equipment.CalNo), Notify);
            }
            catch(Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public static void SendRemindMail(string UniqueID, int Days, string UserID, bool IsDelay)
        {
            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var notify = db.QA_CALIBRATIONNOTIFY.First(x => x.UNIQUEID == UniqueID);

                    var equipment = EquipmentHelper.Get(null, notify.EQUIPMENTUNIQUEID);

                    if (IsDelay)
                    {
                        SendVerifyMail(null, new List<string> { UserID }, string.Format("[逾期][簽核通知][{0}天][{1}][{2}]儀器校驗通知單", Days, notify.VHNO, equipment.CalNo), notify);
                    }
                    else
                    {
                        SendVerifyMail(null, new List<string> { UserID }, string.Format("[跟催][簽核通知][{0}天][{1}][{2}]儀器校驗通知單", Days, notify.VHNO, equipment.CalNo), notify);
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
                    var notify = db.QA_CALIBRATIONNOTIFY.First(x => x.UNIQUEID == UniqueID);

                    var equipment = EquipmentHelper.Get(null, notify.EQUIPMENTUNIQUEID);

                    SendVerifyMail(null, new List<string> { UserID }, string.Format("[簽核通知][{0}][{1}]儀器校驗通知單", notify.VHNO, equipment.CalNo), notify);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private static void SendVerifyMail(List<Models.Shared.Organization> OrganizationList, List<string> UserList, string Subject, QA_CALIBRATIONNOTIFY Notify)
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
                        sb.Append(string.Format(td, equipment.CalNo));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "預計校驗日期"));
                        sb.Append(string.Format(td, DateTimeHelper.DateTime2DateStringWithSeperator(Notify.ESTCALDATE)));
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
                        sb.Append(string.Format(td, string.Format("<a href=\"http://ASECL-eINSPRD01/FEM.Portal/Home/Index?ReturnUrl=http://ASECL-eINSPRD01/FEM/zh-tw/Customized_ASE_QA/CalibrationNotify/Index?VHNO={0}\">連結</a>", Notify.VHNO)));
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

        public static void SendRejectMail(List<Models.Shared.Organization> OrganizationList, List<string> UserList, QA_CALIBRATIONNOTIFY Notify)
        {
            try
            {
                var equipment = EquipmentHelper.Get(OrganizationList, Notify.EQUIPMENTUNIQUEID);

                SendVerifyMail(OrganizationList, UserList, string.Format("[退回修正通知][{0}][{1}]儀器校驗通知單", Notify.VHNO, equipment.CalNo), Notify);
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }  
        }
    }
}
