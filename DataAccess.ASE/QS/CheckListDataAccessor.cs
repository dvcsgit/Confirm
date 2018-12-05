using DbEntity.ASE;
using Models.ASE.QS.CheckListManagement;
using Models.Authenticated;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace DataAccess.ASE.QS
{
    public class CheckListDataAccessor
    {
        public static RequestResult GetQueryFormModel()
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    result.ReturnData(new QueryFormModel()
                    {
                        FactoryList = db.QS_FACTORY.Select(x => new { x.UNIQUEID, x.DESCRIPTION }).Union((from x in db.QS_FACTORY_FORM
                                                                                                          join f in db.QS_FORM
                                                                                                              on x.FORMUNIQUEID equals f.FACTORYUNIQUEID
                                                                                                          select new { x.UNIQUEID, x.DESCRIPTION }).Distinct()).Select(x => new FactoryModel
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION
                        }).OrderBy(x => x.Description).ToList(),
                        ShiftList = db.QS_SHIFT.Select(x => new { x.UNIQUEID, x.DESCRIPTION }).Union(db.QS_SHIFT.Select(x => new { x.UNIQUEID, x.DESCRIPTION })).Distinct().Select(x => new ShiftModel
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION
                        }).OrderBy(x => x.Description).ToList()
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

        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new GridViewModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    model.FactoryList = db.QS_FACTORY.Select(x => new FactoryModel { 
                     UniqueID=x.UNIQUEID,
                     Description=x.DESCRIPTION
                    }).OrderBy(x => x.Description).ToList();

                    var query = (from f in db.QS_FORM
                                 join factory in db.QS_FACTORY_FORM
                                 on new { FormUniqueID = f.UNIQUEID, FactoryUniqueID = f.FACTORYUNIQUEID } equals new { FormUniqueID = factory.FORMUNIQUEID, FactoryUniqueID = factory.UNIQUEID } into tmpFactory
                                 from factory in tmpFactory.DefaultIfEmpty()
                                 join shift in db.QS_SHIFT_FORM
                                 on new { FormUniqueID = f.UNIQUEID, ShiftUniqueID = f.SHIFTUNIQUEID } equals new { FormUniqueID = shift.FORMUNIQUEID, ShiftUniqueID = shift.UNIQUEID } into tmpShift
                                 from shift in tmpShift.DefaultIfEmpty()
                                 join r in db.QS_FORM_CHECKRESULT
                                 on f.UNIQUEID equals r.FORMUNIQUEID into tmpR
                                 from r in tmpR.DefaultIfEmpty()
                                 select new
                                 {
                                     f.UNIQUEID,
                                     f.AUDITDATE,
                                     f.VHNO,
                                     f.FACTORYUNIQUEID,
                                     f.FACTORYREMARK,
                                     FactoryDescription = factory != null ? factory.DESCRIPTION : "",
                                     f.SHIFTUNIQUEID,
                                     f.SHIFTREMARK,
                                     ShiftDescription = shift != null ? shift.DESCRIPTION : "",
                                     f.AUDITORID,
                                     CarNo = r!=null?r.CARNO:""
                                 }).AsQueryable();

                    if (Parameters.BeginDate.HasValue)
                    {
                        query = query.Where(x => DateTime.Compare(x.AUDITDATE, Parameters.BeginDate.Value) >= 0);
                    }

                    if (Parameters.EndDate.HasValue)
                    {
                        query = query.Where(x => DateTime.Compare(x.AUDITDATE, Parameters.EndDate.Value) <= 0);
                    }

                    if (!string.IsNullOrEmpty(Parameters.VHNO))
                    {
                        query = query.Where(x => x.VHNO.Contains(Parameters.VHNO));
                    }

                    if (Parameters.FactoryList != null && Parameters.FactoryList.Count > 0)
                    {
                        query = query.Where(x => Parameters.FactoryList.Contains(x.FACTORYUNIQUEID));
                    }

                    if (!string.IsNullOrEmpty(Parameters.FactoryDescription))
                    {
                        query = query.Where(x => x.FACTORYREMARK.Contains(Parameters.FactoryDescription) || x.FactoryDescription.Contains(Parameters.FactoryDescription));
                    }

                    if (!string.IsNullOrEmpty(Parameters.CarNo))
                    {
                        query = query.Where(x => !string.IsNullOrEmpty(x.CarNo) && x.CarNo.Contains(Parameters.CarNo));
                    }

                    if (Parameters.ShiftList != null && Parameters.ShiftList.Count > 0)
                    {
                        query = query.Where(x => Parameters.ShiftList.Contains(x.SHIFTUNIQUEID));
                    }

                    var temp = query.Select(x => new {
                        x.UNIQUEID,
                        x.AUDITDATE,
                        x.VHNO,
                        x.FACTORYUNIQUEID,
                        x.FactoryDescription,
                        x.SHIFTUNIQUEID,
                        x.ShiftDescription,
                        x.AUDITORID,
                    }).Distinct().OrderByDescending(x => x.VHNO).ToList();

                    foreach (var t in temp)
                    {
                        var auditor = db.ACCOUNT.FirstOrDefault(x => x.ID == t.AUDITORID);

                        var item = new GridItem()
                        {
                            UniqueID = t.UNIQUEID,
                            VHNO = t.VHNO,
                            Factory = t.FactoryDescription,
                            Shift = t.ShiftDescription,
                            AuditDate = t.AUDITDATE,
                            AuditorID = t.AUDITORID,
                            AuditorName = auditor != null ? auditor.NAME : string.Empty,
                            StationList = (from x in db.QS_FORM_STATION
                                           join s in db.QS_STATION_FORM
                                           on new { FormUniqueID = x.FORMUNIQUEID, StationUniqueID = x.STATIONUNIQUEID } equals new { FormUniqueID = s.FORMUNIQUEID, StationUniqueID = s.UNIQUEID } into tmpStation
                                           from s in tmpStation.DefaultIfEmpty()
                                           where x.FORMUNIQUEID == t.UNIQUEID
                                           select new
                                           {
                                               x.STATIONUNIQUEID,
                                               StationDescription = s != null ? s.DESCRIPTION : ""
                                           }).ToList().Select(x => x.StationDescription).OrderBy(x => x).ToList()
                        };

                        model.ItemList.Add(item);
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

        public static RequestResult GetDetailViewModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = (from form in db.QS_FORM
                                 join auditor in db.ACCOUNT
                                 on form.AUDITORID equals auditor.ID into tmpAuditor
                                 from auditor in tmpAuditor.DefaultIfEmpty()
                                 join auditorManager in db.ACCOUNT
                                 on form.AUDITORMANAGERID equals auditorManager.ID into tmpAuditorManager
                                 from auditorManager in tmpAuditorManager.DefaultIfEmpty()
                                 join factory in db.QS_FACTORY_FORM
                                 on new { FormUniqueID = form.UNIQUEID, FactoryUniqueID = form.FACTORYUNIQUEID } equals new { FormUniqueID = factory.FORMUNIQUEID, FactoryUniqueID = factory.UNIQUEID } into tmpFactory
                                 from factory in tmpFactory.DefaultIfEmpty()
                                 join shift in db.QS_SHIFT_FORM
                                 on new { FormUniqueID = form.UNIQUEID, ShiftUniqueID = form.SHIFTUNIQUEID } equals new { FormUniqueID = shift.FORMUNIQUEID, ShiftUniqueID = shift.UNIQUEID } into tmpShift
                                 from shift in tmpShift.DefaultIfEmpty()
                                 where form.UNIQUEID == UniqueID
                                 select new
                                 {
                                     Form = form,
                                     AuditorName = auditor != null ? auditor.NAME : "",
                                     AuditorManagerName = auditorManager != null ? auditorManager.NAME : "",
                                     FactoryDescription = factory != null ? factory.DESCRIPTION : "",
                                     ShiftDescription = shift != null ? shift.DESCRIPTION : ""
                                 }).First();

                    var model = new DetailViewModel()
                    {
                        UniqueID = query.Form.UNIQUEID,
                        VHNO = query.Form.VHNO,
                        AuditDate = query.Form.AUDITDATE,
                        AuditorID = query.Form.AUDITORID,
                        AuditorName = query.AuditorName,
                        AuditorManagerID = query.Form.AUDITORMANAGERID,
                        AuditorManagerName = query.AuditorManagerName,
                        FactoryUniqueID = query.Form.FACTORYUNIQUEID,
                        FactoryDescription = query.FactoryDescription,
                        ShiftUniqueID = query.Form.SHIFTUNIQUEID,
                        ShiftDescription = query.ShiftDescription,
                        StationList = (from x in db.QS_FORM_STATION
                                       join s in db.QS_STATION_FORM
                                       on new { FormUniqueID = x.FORMUNIQUEID, StationUniqueID = x.STATIONUNIQUEID } equals new { FormUniqueID = s.FORMUNIQUEID, StationUniqueID = s.UNIQUEID } into tmpStation
                                       from s in tmpStation.DefaultIfEmpty()
                                       where x.FORMUNIQUEID == query.Form.UNIQUEID
                                       select new
                                       {
                                           x.STATIONUNIQUEID,
                                           StationDescription = s != null ? s.DESCRIPTION : ""
                                       }).ToList().Select(x => x.StationDescription).OrderBy(x => x).ToList()
                    };

                    var checkItemList = db.QS_CHECKITEM_FORM.Where(x => x.FORMUNIQUEID == query.Form.UNIQUEID).ToList();

                    var checkResultList = (from r in db.QS_FORM_CHECKRESULT
                                           join station in db.QS_STATION_FORM
                                           on new { FormUniqueID = r.FORMUNIQUEID, StationUniqueID = r.STATIONUNIQUEID } equals new { FormUniqueID = station.FORMUNIQUEID, StationUniqueID = station.UNIQUEID } into tmpStation
                                           from station in tmpStation.DefaultIfEmpty()
                                           join auditType in db.QS_AUDITTYPE_FORM
                                           on new { FormUniqueID = r.FORMUNIQUEID, AuditTypeUniqueID = r.AUDITTYPEUNIQUEID } equals new { FormUniqueID = auditType.FORMUNIQUEID, AuditTypeUniqueID = auditType.UNIQUEID } into tmpAuditType
                                           from auditType in tmpAuditType.DefaultIfEmpty()
                                           join belongShift in db.QS_SHIFT_FORM
                                           on new { FormUniqueID = r.FORMUNIQUEID, BelongShiftUniqueID = r.BELONGSHIFTUNIQUEID } equals new { FormUniqueID = belongShift.FORMUNIQUEID, BelongShiftUniqueID = belongShift.UNIQUEID } into tmpBelongShift
                                           from belongShift in tmpBelongShift.DefaultIfEmpty()

                                           join auditorShift in db.QS_SHIFT_FORM
                                          on new { FormUniqueID = r.FORMUNIQUEID, AuditorShiftUniqueID = r.AUDITORSHIFTUNIQUEID } equals new { FormUniqueID = auditorShift.FORMUNIQUEID, AuditorShiftUniqueID = auditorShift.UNIQUEID } into tmpAuditorShift
                                           from auditorShift in tmpAuditorShift.DefaultIfEmpty()

                                           join errorUser in db.ACCOUNT
                                         on r.ERRORUSERID equals errorUser.ID into tmpErrorUser
                                           from errorUser in tmpErrorUser.DefaultIfEmpty()

                                           join carOwner in db.ACCOUNT
                                           on r.CAROWNERID equals carOwner.ID into tmpCarOwner
                                           from carOwner in tmpCarOwner.DefaultIfEmpty()
                                           join carOwnerManager in db.ACCOUNT
                                           on r.CAROWNERMANAGERID equals carOwnerManager.ID into tmpCarOwnerManager
                                           from carOwnerManager in tmpCarOwnerManager.DefaultIfEmpty()
                                           join departmentManager in db.ACCOUNT
                                           on r.DEPARTMENTMANAGERID equals departmentManager.ID into tmpDepartmentManager
                                           from departmentManager in tmpDepartmentManager.DefaultIfEmpty()
                                           join risk in db.QS_RISK_FORM
                                           on new { FormUniqueID = r.FORMUNIQUEID, RiskUniqueID = r.RISKUNIQUEID } equals new { FormUniqueID = risk.FORMUNIQUEID, RiskUniqueID = risk.UNIQUEID } into tmpRisk
                                           from risk in tmpRisk.DefaultIfEmpty()
                                           join grade in db.QS_GRADE_FORM
                                           on new { FormUniqueID = r.FORMUNIQUEID, GradeUniqueID = r.GRADEUNIQUEID } equals new { FormUniqueID = grade.FORMUNIQUEID, GradeUniqueID = grade.UNIQUEID } into tmpGrade
                                           from grade in tmpGrade.DefaultIfEmpty()
                                           where r.FORMUNIQUEID == query.Form.UNIQUEID
                                           select new
                                           {
                                               Result = r,
                                               Station = station != null ? station.DESCRIPTION : "",
                                               AuditType = auditType != null ? auditType.DESCRIPTION : "",
                                               BelongShift = belongShift != null ? belongShift.DESCRIPTION : "",
                                               AuditorShift = auditorShift!=null?auditorShift.DESCRIPTION:"",
                                               CarOwnerName = carOwner != null ? carOwner.NAME : "",
                                               CarOwnerManagerName = carOwnerManager != null ? carOwnerManager.NAME : "",
                                               DepartmentManagerName = departmentManager != null ? departmentManager.NAME : "",
                                               ErrorUserName = errorUser!=null?errorUser.NAME:"",
                                               Risk = risk != null ? risk.DESCRIPTION : "",
                                               Grade = grade != null ? grade.DESCRIPTION : ""
                                           }).ToList();

                    var checkTypeList = checkItemList.Select(x => new { x.TYPEID, x.TYPEEDESCRIPTION, x.TYPECDESCRIPTION }).Distinct().OrderBy(x => x.TYPEID).ToList();

                    foreach (var checkType in checkTypeList)
                    {
                        model.CheckTypeList.Add(new CheckTypeModel()
                        {
                            ID = checkType.TYPEID,
                            EDescription = checkType.TYPEEDESCRIPTION,
                            CDescription = checkType.TYPECDESCRIPTION,
                            CheckItemList = checkItemList.Where(c => c.TYPEID == checkType.TYPEID).OrderBy(c => c.ID).ToList().Select(c => new CheckItemModel
                            {
                                UniqueID = c.UNIQUEID,
                                ID = string.Format("{0}.{1}", checkType.TYPEID, c.ID),
                                EDescription = c.EDESCRIPTION,
                                CDescription = c.CDESCRIPTION,
                                CheckTimes = c.CHECKTIMES,
                                Unit = c.UNIT,
                                CheckResultList = checkResultList.Where(x => x.Result.CHECKITEMUNIQUEID == c.UNIQUEID).Select(x => new CheckResultModel
                                {
                                    Seq = x.Result.SEQ,
                                    StationUniqueID = x.Result.STATIONUNIQUEID,
                                    Station = x.Station,
                                    AuditObject = x.Result.AUDITOBJECT,
                                    ResDepartments = x.Result.RESDEPARTMENTS,
                                    Result = x.Result.RESULT,
                                    CarNo = x.Result.CARNO,
                                    Remark = x.Result.REMARK,
                                    CPNO = x.Result.CPNO,
                                    Weekly = x.Result.WEEKLY,
                                    AuditTypeUniqueID = x.Result.AUDITTYPEUNIQUEID,
                                    AuditType = x.AuditType,
                                    AuditorShiftUniqueID=x.Result.AUDITORSHIFTUNIQUEID,
                                    AuditorShift=x.AuditorShift,
                                    BelongShiftUniqueID = x.Result.BELONGSHIFTUNIQUEID,
                                    BelongShift = x.BelongShift,
                                    CarOwnerID = x.Result.CAROWNERID,
                                    CarOwnerName = x.CarOwnerName,
                                    CarOwnerManagerID = x.Result.CAROWNERMANAGERID,
                                    CarOwnerManagerName = x.CarOwnerManagerName,
                                    DepartmentManagerID = x.Result.DEPARTMENTMANAGERID,
                                    DepartmentManagerName = x.DepartmentManagerName,
                                    RiskUniqueID = x.Result.RISKUNIQUEID,
                                    Risk = x.Risk,
                                    GradeUniqueID = x.Result.GRADEUNIQUEID,
                                    Grade = x.Grade,
                                    IsBelongMO = x.Result.ISBELONGMO,
                                    ErrorUserID = x.Result.ERRORUSERID,
                                    ErrorUserName = x.ErrorUserName,
                                    ErrorMachineNo = x.Result.ERRORMACHINENO,
                                    ErrorArea = x.Result.ERRORAREA,
                                    PhotoList = db.QS_FORM_PHOTO.Where(p => p.FORMUNIQUEID == query.Form.UNIQUEID && p.CHECKITEMUNIQUEID == c.UNIQUEID && p.CHECKITEMSEQ == x.Result.SEQ).ToList().Select(p => new PhotoModel
                                    {
                                        FormUniqueID = query.Form.UNIQUEID,
                                        CheckItemUniqueID = c.UNIQUEID,
                                        CheckItemSeq = Convert.ToInt32(x.Result.SEQ),
                                        Seq = Convert.ToInt32(p.SEQ),
                                        Extension = p.EXTENSION,
                                        IsSaved = true
                                    }).ToList()
                                }).ToList()
                            }).ToList()
                        });
                    }

                    foreach (var checkType in model.CheckTypeList)
                    {
                        foreach (var checkItem in checkType.CheckItemList)
                        {
                            foreach (var checkResult in checkItem.CheckResultList)
                            {
                                foreach (var resDepartment in checkResult.ResDepartmentList)
                                {
                                    if (!string.IsNullOrEmpty(resDepartment))
                                    {
                                        var dept = db.QS_RESDEPARTMENT_FORM.FirstOrDefault(x => x.FORMUNIQUEID == query.Form.UNIQUEID && x.UNIQUEID == resDepartment);

                                        if (dept != null)
                                        {
                                            checkResult.ResDepartmentDescriptionList.Add(dept.DESCRIPTION);
                                        }
                                    }
                                }
                            }
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

        public static RequestResult GetCreateFormModel(string FactoryUniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var auditor = db.ACCOUNT.First(x => x.ID == Account.ID);

                    var factory = db.QS_FACTORY.First(x => x.UNIQUEID == FactoryUniqueID);

                    var model = new CreateFormModel()
                    {
                        FactoryUniqueID = FactoryUniqueID,
                        Factory =factory.DESCRIPTION,
                        ShiftList = db.QS_SHIFT.Select(x => new ShiftModel
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION
                        }).OrderBy(x => x.Description).ToList(),
                        StationList = (from x in db.QS_FACTORY_STATION
                                      join s in db.QS_STATION
                                      on x.STATIONUNIQUEID equals s.UNIQUEID
                                      where x.FACTORYUNIQUEID==FactoryUniqueID
                                       select s).Select(x => new StationModel
                                       {
                                           UniqueID = x.UNIQUEID,
                                           Type = x.TYPE,
                                           Description = x.DESCRIPTION
                                       }).OrderBy(x => x.Description).ToList(),
                        ResDepartmentList = db.QS_RESDEPARTMENT.Select(x => new ResDepartmentModel
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION
                        }).OrderBy(x => x.Description).ToList(),
                        AuditTypeList = db.QS_AUDITTYPE.Select(x => new AuditTypeModel
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION
                        }).ToList(),
                        RiskList = db.QS_RISK.Select(x => new RiskModel
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION
                        }).ToList(),
                        GradeList = db.QS_GRADE.Select(x => new GradeModel
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION
                        }).ToList(),
                        FormInput = new FormInput()
                        {
                            AuditDateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today),
                            AuditorID = Account.ID,
                            AuditorManagerID = auditor.MANAGERID
                        }
                    };

                    var checkItemList = (from x in db.QS_FACTORY_CHECKITEM
                                         join c in db.QS_CHECKITEM
                                         on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                         where x.FACTORYUNIQUEID == FactoryUniqueID
                                         select c).ToList();
                    //var checkItemList = db.QS_CHECKITEM.ToList();

                    var checkTypeList = checkItemList.Select(x => new { x.TYPEID, x.TYPEEDESCRIPTION, x.TYPECDESCRIPTION }).Distinct().OrderBy(x => x.TYPEID).ToList();

                    foreach (var checkType in checkTypeList)
                    {
                        model.CheckTypeList.Add(new CheckTypeModel()
                        {
                            ID = checkType.TYPEID,
                            EDescription = checkType.TYPEEDESCRIPTION,
                            CDescription = checkType.TYPECDESCRIPTION,
                            CheckItemList = checkItemList.Where(c => c.TYPEID == checkType.TYPEID).OrderBy(c => c.ID).ToList().Select(c => new CheckItemModel
                            {
                                UniqueID = c.UNIQUEID,
                                ID = string.Format("{0}.{1}", checkType.TYPEID, c.ID),
                                EDescription = c.EDESCRIPTION,
                                CDescription = c.CDESCRIPTION,
                                CheckTimes = c.CHECKTIMES,
                                Unit = c.UNIT,
                                RemarkList = (from x in db.QS_CHECKITEMREMARK
                                              join r in db.QS_REMARK
                                              on x.REMARKUNIQUEID equals r.UNIQUEID
                                              where x.CHECKITEMUNIQUEID == c.UNIQUEID
                                              select new RemarkModel
                                              {
                                                  UniqueID = r.UNIQUEID,
                                                  Description = r.DESCRIPTION
                                              }).OrderBy(x => x.Description).ToList()
                            }).ToList()
                        });
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

        public static RequestResult Create(CreateFormModel Model, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var vhnoPrex = string.Format("Q{0}", Model.FormInput.AuditDate.ToString("yyyyMMdd").Substring(2, 6));

                    var query = db.QS_FORM.Where(x => x.VHNO.StartsWith(vhnoPrex)).OrderByDescending(x => x.VHNO).ToList();

                    int vhnoSeq = 1;

                    if (query.Count > 0)
                    {
                        vhnoSeq = int.Parse(query.First().VHNO.Substring(7)) + 1;
                    }

                    var vhno = string.Format("{0}{1}", vhnoPrex, vhnoSeq.ToString().PadLeft(2, '0'));

                    var uniqueID = Guid.NewGuid().ToString();

                    db.QS_FORM.Add(new QS_FORM()
                    {
                        UNIQUEID = uniqueID,
                        VHNO = vhno,
                        AUDITDATE = Model.FormInput.AuditDate,
                        AUDITORID = Model.FormInput.AuditorID,
                        AUDITORMANAGERID = Model.FormInput.AuditorManagerID,
                        FACTORYUNIQUEID = Model.FactoryUniqueID,
                        SHIFTUNIQUEID = Model.FormInput.ShiftUniqueID
                    });

                    db.QS_FORM_LOG.Add(new QS_FORM_LOG()
                    {
                        FORMUNIQUEID = uniqueID,
                        SEQ = 1,
                        ACTION = Define.EnumFormAction.Create.ToString(),
                        ACTIONTIME = DateTime.Now,
                        USERID = Account.ID
                    });

                    foreach (var station in Model.FormInput.StationList)
                    {
                        if (station == Define.OTHER)
                        {
                            db.QS_FORM_STATION.Add(new QS_FORM_STATION()
                            {
                                FORMUNIQUEID = uniqueID,
                                STATIONUNIQUEID = station
                            });
                        }
                        else
                        {
                            db.QS_FORM_STATION.Add(new QS_FORM_STATION()
                            {
                                FORMUNIQUEID = uniqueID,
                                STATIONUNIQUEID = station
                            });
                        }
                    }

                    foreach (var checkResultString in Model.FormInput.CheckResultStringList)
                    {
                        string[] t = checkResultString.Split(Define.Seperators, StringSplitOptions.None);

                        var checkItemUniqueID = t[0];
                        var seq = int.Parse(t[1]);
                        var stationUniqueID = t[2];
                        var auditObject = t[3];
                        var resDepartments = t[4].Replace(",", Define.Seperator);
                        var r = t[5];
                        var carNo = t[6];
                        var remark = t[7];
                        var cpNo = t[8];
                        var weekly = !string.IsNullOrEmpty(t[9]) ? decimal.Parse(t[9]) : default(decimal?);
                        var auditTypeUniqueID = t[10];
                        var auditorShiftUniqueID = t[11];
                        var belongShiftUniqueID = t[12];
                        var carOwnerID = t[13];
                        var carOwnerManagerID = t[14];
                        var departmentManagerID = t[15];
                        var riskUniqueID = t[16];
                        var gradeUniqueID = t[17];
                        var isBelongMO = t[18];
                        var errorUserID = t[19];
                        var errorMachineNo = t[20];
                        var errorArea = t[21];

                        var checkResult = new QS_FORM_CHECKRESULT()
                        {
                            FORMUNIQUEID = uniqueID,
                            CHECKITEMUNIQUEID = checkItemUniqueID,
                            SEQ = seq,
                            STATIONUNIQUEID = stationUniqueID,
                            AUDITOBJECT = auditObject,
                            RESDEPARTMENTS = resDepartments,
                            RESULT = r,
                            CARNO = carNo,
                            REMARK = remark
                        };

                        if (r == "N")
                        {
                            checkResult.CPNO = cpNo;
                            checkResult.WEEKLY = weekly;   
                            checkResult.AUDITTYPEUNIQUEID = auditTypeUniqueID;
                            checkResult.AUDITORSHIFTUNIQUEID = auditorShiftUniqueID;
                            checkResult.BELONGSHIFTUNIQUEID = belongShiftUniqueID;
                            checkResult.CAROWNERID = carOwnerID;
                            checkResult.CAROWNERMANAGERID = carOwnerManagerID;
                            checkResult.DEPARTMENTMANAGERID = departmentManagerID;
                            checkResult.RISKUNIQUEID = riskUniqueID;
                            checkResult.GRADEUNIQUEID = gradeUniqueID;
                            checkResult.ISBELONGMO = isBelongMO;
                            checkResult.ERRORUSERID = errorUserID;
                            checkResult.ERRORMACHINENO = errorMachineNo;
                            checkResult.ERRORAREA = errorArea;
                        }

                        db.QS_FORM_CHECKRESULT.Add(checkResult);
                    }

                    #region
                    db.QS_AUDITTYPE_FORM.AddRange(db.QS_AUDITTYPE.ToList().Select(x => new QS_AUDITTYPE_FORM
                    {
                        FORMUNIQUEID = uniqueID,
                        UNIQUEID = x.UNIQUEID,
                        DESCRIPTION = x.DESCRIPTION
                    }).ToList());

                    db.QS_CHECKITEM_FORM.AddRange((from x in db.QS_FACTORY_CHECKITEM
                                                   join c in db.QS_CHECKITEM
                                                       on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                                   where x.FACTORYUNIQUEID == Model.FactoryUniqueID
                                                   select c).ToList().Select(x => new QS_CHECKITEM_FORM
                    {
                        FORMUNIQUEID = uniqueID,
                        UNIQUEID = x.UNIQUEID,
                        TYPEID = x.TYPEID,
                        TYPEEDESCRIPTION = x.TYPEEDESCRIPTION,
                        TYPECDESCRIPTION = x.TYPECDESCRIPTION,
                        ID = x.ID,
                        EDESCRIPTION = x.EDESCRIPTION,
                        CDESCRIPTION = x.CDESCRIPTION,
                        CHECKTIMES = x.CHECKTIMES,
                        UNIT = x.UNIT
                    }).ToList());

                    db.QS_CHECKITEMREMARK_FORM.AddRange(db.QS_CHECKITEMREMARK.ToList().Select(x => new QS_CHECKITEMREMARK_FORM
                    {
                        FORMUNIQUEID = uniqueID,
                        CHECKITEMUNIQUEID = x.CHECKITEMUNIQUEID,
                        REMARKUNIQUEID = x.REMARKUNIQUEID
                    }).ToList());

                    db.QS_FACTORY_FORM.AddRange(db.QS_FACTORY.ToList().Select(x => new QS_FACTORY_FORM
                    {
                        FORMUNIQUEID = uniqueID,
                        UNIQUEID = x.UNIQUEID,
                        DESCRIPTION = x.DESCRIPTION
                    }).ToList());

                    db.QS_GRADE_FORM.AddRange(db.QS_GRADE.ToList().Select(x => new QS_GRADE_FORM
                    {
                        FORMUNIQUEID = uniqueID,
                        UNIQUEID = x.UNIQUEID,
                        DESCRIPTION = x.DESCRIPTION
                    }).ToList());

                    db.QS_REMARK_FORM.AddRange(db.QS_REMARK.ToList().Select(x => new QS_REMARK_FORM
                    {
                        FORMUNIQUEID = uniqueID,
                        UNIQUEID = x.UNIQUEID,
                        DESCRIPTION = x.DESCRIPTION
                    }).ToList());

                    db.QS_RESDEPARTMENT_FORM.AddRange(db.QS_RESDEPARTMENT.ToList().Select(x => new QS_RESDEPARTMENT_FORM
                    {
                        FORMUNIQUEID = uniqueID,
                        UNIQUEID = x.UNIQUEID,
                        DESCRIPTION = x.DESCRIPTION
                    }).ToList());

                    db.QS_RISK_FORM.AddRange(db.QS_RISK.ToList().Select(x => new QS_RISK_FORM
                    {
                        FORMUNIQUEID = uniqueID,
                        UNIQUEID = x.UNIQUEID,
                        DESCRIPTION = x.DESCRIPTION
                    }).ToList());

                    db.QS_SHIFT_FORM.AddRange(db.QS_SHIFT.ToList().Select(x => new QS_SHIFT_FORM
                    {
                        FORMUNIQUEID = uniqueID,
                        UNIQUEID = x.UNIQUEID,
                        DESCRIPTION = x.DESCRIPTION
                    }).ToList());

                    var factoryStationList = (from x in db.QS_FACTORY_STATION
                                                 join s in db.QS_STATION
                                                 on x.STATIONUNIQUEID equals s.UNIQUEID
                                                 where x.FACTORYUNIQUEID == Model.FactoryUniqueID
                                                 select s).ToList();

                    foreach (var factoryStation in factoryStationList)
                    {
                        db.QS_STATION_FORM.Add(new QS_STATION_FORM
                        {
                            FORMUNIQUEID = uniqueID,
                            TYPE = factoryStation.TYPE,
                            UNIQUEID = factoryStation.UNIQUEID,
                            DESCRIPTION = factoryStation.DESCRIPTION
                        });
                    }

                    #endregion

                    foreach (var photo in Model.PhotoList)
                    {
                        if (!photo.IsSaved)
                        {
                            db.QS_FORM_PHOTO.Add(new QS_FORM_PHOTO()
                            {
                                FORMUNIQUEID = uniqueID,
                                CHECKITEMUNIQUEID = photo.CheckItemUniqueID,
                                CHECKITEMSEQ = photo.CheckItemSeq,
                                SEQ = photo.Seq,
                                EXTENSION = photo.Extension
                            });

                            System.IO.File.Move(photo.TempFullFileName, Path.Combine(Config.QSFileFolderPath, string.Format("{0}_{1}_{2}_{3}.{4}", uniqueID, photo.CheckItemUniqueID, photo.CheckItemSeq, photo.Seq, photo.Extension)));
                        }
                    }

                    db.SaveChanges();

                    result.ReturnSuccessMessage("新增成功");
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

        public static RequestResult GetEditFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = db.QS_FORM.First(x => x.UNIQUEID == UniqueID);

                    var factory = db.QS_FACTORY.FirstOrDefault(x => x.UNIQUEID == form.FACTORYUNIQUEID);

                    var model = new EditFormModel()
                    {
                        UniqueID = form.UNIQUEID,
                        VHNO =form.VHNO,
                        Factory = factory!=null?factory.DESCRIPTION:form.FACTORYREMARK,
                        ShiftList = db.QS_SHIFT_FORM.Where(x => x.FORMUNIQUEID == form.UNIQUEID).Select(x => new ShiftModel
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION
                        }).OrderBy(x => x.Description).ToList(),
                        StationList = db.QS_STATION_FORM.Where(x => x.FORMUNIQUEID == form.UNIQUEID).Select(x => new StationModel
                        {
                            UniqueID = x.UNIQUEID,
                            Type = x.TYPE,
                            Description = x.DESCRIPTION
                        }).OrderBy(x => x.Description).ToList(),
                        ResDepartmentList = db.QS_RESDEPARTMENT_FORM.Where(x => x.FORMUNIQUEID == form.UNIQUEID).Select(x => new ResDepartmentModel
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION
                        }).OrderBy(x => x.Description).ToList(),
                        AuditTypeList = db.QS_AUDITTYPE_FORM.Where(x => x.FORMUNIQUEID == form.UNIQUEID).Select(x => new AuditTypeModel
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION
                        }).ToList(),
                        RiskList = db.QS_RISK_FORM.Where(x => x.FORMUNIQUEID == form.UNIQUEID).Select(x => new RiskModel
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION
                        }).ToList(),
                        GradeList = db.QS_GRADE_FORM.Where(x => x.FORMUNIQUEID == form.UNIQUEID).Select(x => new GradeModel
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION
                        }).ToList(),
                        FormInput = new FormInput()
                        {
                            AuditDateString = DateTimeHelper.DateTime2DateStringWithSeperator(form.AUDITDATE),
                            AuditorID = form.AUDITORID,
                            AuditorManagerID = form.AUDITORMANAGERID,
                            ShiftUniqueID = form.SHIFTUNIQUEID
                        }
                    };

                    var stationList = db.QS_FORM_STATION.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList();

                    foreach (var station in stationList)
                    {
                        model.FormStationList.Add(station.STATIONUNIQUEID);
                    }

                    var checkItemList = db.QS_CHECKITEM_FORM.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList();
                    var checkResultList = db.QS_FORM_CHECKRESULT.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList();

                    var checkTypeList = checkItemList.Select(x => new { x.TYPEID, x.TYPEEDESCRIPTION, x.TYPECDESCRIPTION }).Distinct().OrderBy(x => x.TYPEID).ToList();

                    foreach (var checkType in checkTypeList)
                    {
                        model.CheckTypeList.Add(new CheckTypeModel()
                        {
                            ID = checkType.TYPEID,
                            EDescription = checkType.TYPEEDESCRIPTION,
                            CDescription = checkType.TYPECDESCRIPTION,
                            CheckItemList = checkItemList.Where(c => c.TYPEID == checkType.TYPEID).OrderBy(c => c.ID).ToList().Select(c => new CheckItemModel
                            {
                                UniqueID = c.UNIQUEID,
                                ID = string.Format("{0}.{1}", checkType.TYPEID, c.ID),
                                EDescription = c.EDESCRIPTION,
                                CDescription = c.CDESCRIPTION,
                                CheckTimes = c.CHECKTIMES,
                                Unit = c.UNIT,
                                CheckResultList = checkResultList.Where(x => x.CHECKITEMUNIQUEID == c.UNIQUEID).Select(x => new CheckResultModel
                                {
                                    Seq = x.SEQ,
                                    StationUniqueID = x.STATIONUNIQUEID,
                                    AuditObject = x.AUDITOBJECT,
                                    ResDepartments = x.RESDEPARTMENTS,
                                    Result = x.RESULT,
                                      AuditorShiftUniqueID=x.AUDITORSHIFTUNIQUEID,
                                          CPNO=x.CPNO,
                                           ErrorArea=x.ERRORAREA,
                                            ErrorMachineNo=x.ERRORMACHINENO,
                                             ErrorUserID=x.ERRORUSERID,
                                    Remark = x.REMARK,
                                    CarNo = x.CARNO,
                                    Weekly = x.WEEKLY,
                                    AuditTypeUniqueID = x.AUDITTYPEUNIQUEID,
                                    BelongShiftUniqueID = x.BELONGSHIFTUNIQUEID,
                                    CarOwnerID = x.CAROWNERID,
                                    CarOwnerManagerID = x.CAROWNERMANAGERID,
                                    DepartmentManagerID = x.DEPARTMENTMANAGERID,
                                    RiskUniqueID = x.RISKUNIQUEID,
                                    GradeUniqueID = x.GRADEUNIQUEID,
                                    IsBelongMO = x.ISBELONGMO,
                                    PhotoList = db.QS_FORM_PHOTO.Where(p => p.FORMUNIQUEID == form.UNIQUEID && p.CHECKITEMUNIQUEID == c.UNIQUEID && p.CHECKITEMSEQ == x.SEQ).ToList().Select(p => new PhotoModel
                                    {
                                        FormUniqueID = form.UNIQUEID,
                                        CheckItemUniqueID = c.UNIQUEID,
                                        CheckItemSeq = Convert.ToInt32(x.SEQ),
                                        Seq = Convert.ToInt32(p.SEQ),
                                        Extension = p.EXTENSION,
                                        IsSaved = true
                                    }).ToList(),
                                }).ToList(),
                                RemarkList = (from x in db.QS_CHECKITEMREMARK
                                              join r in db.QS_REMARK
                                              on x.REMARKUNIQUEID equals r.UNIQUEID
                                              where x.CHECKITEMUNIQUEID == c.UNIQUEID
                                              select new RemarkModel
                                              {
                                                  UniqueID = r.UNIQUEID,
                                                  Description = r.DESCRIPTION
                                              }).OrderBy(x => x.Description).ToList()
                            }).ToList()
                        });
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

        public static RequestResult GetCopyFormModel(string UniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = db.QS_FORM.First(x => x.UNIQUEID == UniqueID);

                    var factory = db.QS_FACTORY.FirstOrDefault(x => x.UNIQUEID == form.FACTORYUNIQUEID);

                    var auditor = db.ACCOUNT.First(x => x.ID == Account.ID);

                    var model = new CreateFormModel()
                    {
                        FactoryUniqueID = form.FACTORYUNIQUEID,
                        Factory = factory!=null?factory.DESCRIPTION:"",
                        ShiftList = db.QS_SHIFT_FORM.Where(x => x.FORMUNIQUEID == form.UNIQUEID).Select(x => new ShiftModel
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION
                        }).OrderBy(x => x.Description).ToList(),
                        StationList = db.QS_STATION_FORM.Where(x => x.FORMUNIQUEID == form.UNIQUEID).Select(x => new StationModel
                        {
                            UniqueID = x.UNIQUEID,
                            Type = x.TYPE,
                            Description = x.DESCRIPTION
                        }).OrderBy(x => x.Description).ToList(),
                        ResDepartmentList = db.QS_RESDEPARTMENT_FORM.Where(x => x.FORMUNIQUEID == form.UNIQUEID).Select(x => new ResDepartmentModel
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION
                        }).OrderBy(x => x.Description).ToList(),
                        AuditTypeList = db.QS_AUDITTYPE_FORM.Where(x => x.FORMUNIQUEID == form.UNIQUEID).Select(x => new AuditTypeModel
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION
                        }).ToList(),
                        RiskList = db.QS_RISK_FORM.Where(x => x.FORMUNIQUEID == form.UNIQUEID).Select(x => new RiskModel
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION
                        }).ToList(),
                        GradeList = db.QS_GRADE_FORM.Where(x => x.FORMUNIQUEID == form.UNIQUEID).Select(x => new GradeModel
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION
                        }).ToList(),
                        FormInput = new FormInput()
                        {
                            AuditDateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today),
                            AuditorID = Account.ID,
                            AuditorManagerID = auditor.MANAGERID,
                            ShiftUniqueID = form.SHIFTUNIQUEID
                        }
                    };

                    var stationList = db.QS_FORM_STATION.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList();

                    foreach (var station in stationList)
                    {
                        model.FormStationList.Add(station.STATIONUNIQUEID);
                    }

                    var checkItemList = db.QS_CHECKITEM_FORM.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList();
                    var checkResultList = db.QS_FORM_CHECKRESULT.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList();

                    var checkTypeList = checkItemList.Select(x => new { x.TYPEID, x.TYPEEDESCRIPTION, x.TYPECDESCRIPTION }).Distinct().OrderBy(x => x.TYPEID).ToList();

                    foreach (var checkType in checkTypeList)
                    {
                        model.CheckTypeList.Add(new CheckTypeModel()
                        {
                            ID = checkType.TYPEID,
                            EDescription = checkType.TYPEEDESCRIPTION,
                            CDescription = checkType.TYPECDESCRIPTION,
                            CheckItemList = checkItemList.Where(c => c.TYPEID == checkType.TYPEID).OrderBy(c => c.ID).ToList().Select(c => new CheckItemModel
                            {
                                UniqueID = c.UNIQUEID,
                                ID = string.Format("{0}.{1}", checkType.TYPEID, c.ID),
                                EDescription = c.EDESCRIPTION,
                                CDescription = c.CDESCRIPTION,
                                CheckTimes = c.CHECKTIMES,
                                Unit = c.UNIT,
                                CheckResultList = checkResultList.Where(x => x.CHECKITEMUNIQUEID == c.UNIQUEID).Select(x => new CheckResultModel
                                {
                                    Seq = x.SEQ,
                                    StationUniqueID = x.STATIONUNIQUEID,
                                    AuditObject = x.AUDITOBJECT,
                                    ResDepartments = x.RESDEPARTMENTS,
                                    Result = x.RESULT,
                                     AuditorShiftUniqueID = x.AUDITORSHIFTUNIQUEID,
                                      ErrorArea=x.ERRORAREA,
                                       ErrorUserID=x.ERRORUSERID,
                                        ErrorMachineNo=x.ERRORMACHINENO,
                                         CPNO=  x.CPNO,
                                    Remark = x.REMARK,
                                    CarNo = x.CARNO,
                                    Weekly = x.WEEKLY,
                                    AuditTypeUniqueID = x.AUDITTYPEUNIQUEID,
                                    BelongShiftUniqueID = x.BELONGSHIFTUNIQUEID,
                                    CarOwnerID = x.CAROWNERID,
                                    CarOwnerManagerID = x.CAROWNERMANAGERID,
                                    DepartmentManagerID = x.DEPARTMENTMANAGERID,
                                    RiskUniqueID = x.RISKUNIQUEID,
                                    GradeUniqueID = x.GRADEUNIQUEID,
                                    IsBelongMO = x.ISBELONGMO,
                                }).ToList(),
                                RemarkList = (from x in db.QS_CHECKITEMREMARK
                                              join r in db.QS_REMARK
                                              on x.REMARKUNIQUEID equals r.UNIQUEID
                                              where x.CHECKITEMUNIQUEID == c.UNIQUEID
                                              select new RemarkModel
                                              {
                                                  UniqueID = r.UNIQUEID,
                                                  Description = r.DESCRIPTION
                                              }).OrderBy(x => x.Description).ToList()
                            }).ToList()
                        });
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

        public static RequestResult Edit(EditFormModel Model, Account Account)
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
                    var form = db.QS_FORM.First(x => x.UNIQUEID == Model.UniqueID);

                    form.AUDITDATE = Model.FormInput.AuditDate;
                    form.AUDITORID = Model.FormInput.AuditorID;
                    form.AUDITORMANAGERID = Model.FormInput.AuditorManagerID;
                    form.SHIFTUNIQUEID = Model.FormInput.ShiftUniqueID;

                    db.SaveChanges();

                    db.QS_FORM_LOG.Add(new QS_FORM_LOG()
                    {
                        FORMUNIQUEID = form.UNIQUEID,
                        SEQ = db.QS_FORM_LOG.Count(x => x.FORMUNIQUEID == form.UNIQUEID) + 1,
                        ACTION = Define.EnumFormAction.Edit.ToString(),
                        ACTIONTIME = DateTime.Now,
                        USERID = Account.ID
                    });

                    db.SaveChanges();

                    db.QS_FORM_STATION.RemoveRange(db.QS_FORM_STATION.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList());

                    db.SaveChanges();

                    foreach (var station in Model.FormInput.StationList)
                    {
                        db.QS_FORM_STATION.Add(new QS_FORM_STATION()
                        {
                            FORMUNIQUEID = form.UNIQUEID,
                            STATIONUNIQUEID = station
                        });
                    }

                    db.SaveChanges();

                    db.QS_FORM_CHECKRESULT.RemoveRange(db.QS_FORM_CHECKRESULT.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList());

                    db.SaveChanges();

                    foreach (var checkResultString in Model.FormInput.CheckResultStringList)
                    {
                        string[] t = checkResultString.Split(Define.Seperators, StringSplitOptions.None);

                        var checkItemUniqueID = t[0];
                        var seq = int.Parse(t[1]);
                        var stationUniqueID = t[2];
                        var auditObject = t[3];
                        var resDepartments = t[4].Replace(",", Define.Seperator);
                        var r = t[5];
                        var carNo = t[6];
                        var remark = t[7];
                        var cpNo = t[8];
                        var weekly = !string.IsNullOrEmpty(t[9]) ? decimal.Parse(t[9]) : default(decimal?);
                        var auditTypeUniqueID = t[10];
                        var auditorShiftUniqueID = t[11];
                        var belongShiftUniqueID = t[12];
                        var carOwnerID = t[13];
                        var carOwnerManagerID = t[14];
                        var departmentManagerID = t[15];
                        var riskUniqueID = t[16];
                        var gradeUniqueID = t[17];
                        var isBelongMO = t[18];
                        var errorUserID = t[19];
                        var errorMachineNo = t[20];
                        var errorArea = t[21];

                        var checkResult = new QS_FORM_CHECKRESULT()
                        {
                            FORMUNIQUEID = form.UNIQUEID,
                            CHECKITEMUNIQUEID = checkItemUniqueID,
                            SEQ = seq,
                            STATIONUNIQUEID = stationUniqueID,
                            AUDITOBJECT = auditObject,
                            RESDEPARTMENTS = resDepartments,
                            RESULT = r,
                            REMARK = remark,
                            CARNO = carNo
                        };

                        if (r == "N")
                        {
                            checkResult.CPNO = cpNo;
                            checkResult.WEEKLY = weekly;
                            checkResult.AUDITTYPEUNIQUEID = auditTypeUniqueID;
                            checkResult.AUDITORSHIFTUNIQUEID = auditorShiftUniqueID;
                            checkResult.BELONGSHIFTUNIQUEID = belongShiftUniqueID;
                            checkResult.CAROWNERID = carOwnerID;
                            checkResult.CAROWNERMANAGERID = carOwnerManagerID;
                            checkResult.DEPARTMENTMANAGERID = departmentManagerID;
                            checkResult.RISKUNIQUEID = riskUniqueID;
                            checkResult.GRADEUNIQUEID = gradeUniqueID;
                            checkResult.ISBELONGMO = isBelongMO;
                            checkResult.ERRORUSERID = errorUserID;
                            checkResult.ERRORMACHINENO = errorMachineNo;
                            checkResult.ERRORAREA = errorArea;
                        }

                        db.QS_FORM_CHECKRESULT.Add(checkResult);
                    }

                    db.SaveChanges();

                    var photoList = db.QS_FORM_PHOTO.Where(x => x.FORMUNIQUEID == Model.UniqueID).ToList();

                    foreach (var photo in photoList)
                    {
                        if (!Model.PhotoList.Any(x => x.CheckItemUniqueID == photo.CHECKITEMUNIQUEID && x.CheckItemSeq == photo.CHECKITEMSEQ && x.Seq == photo.SEQ))
                        {
                            db.QS_FORM_PHOTO.Remove(photo);

                            try
                            {
                                System.IO.File.Delete(Path.Combine(Config.QSFileFolderPath, string.Format("{0}_{1}_{2}_{3}.{4}", Model.UniqueID, photo.CHECKITEMUNIQUEID, photo.CHECKITEMSEQ, photo.SEQ, photo.EXTENSION)));
                            }
                            catch { }
                        }
                    }

                    db.SaveChanges();

                    foreach (var photo in Model.PhotoList)
                    {
                        if (!photo.IsSaved)
                        {
                            var photoModel = db.QS_FORM_PHOTO.FirstOrDefault(x => x.FORMUNIQUEID == Model.UniqueID && x.CHECKITEMUNIQUEID == photo.CheckItemUniqueID && x.CHECKITEMSEQ == photo.CheckItemSeq && x.SEQ == photo.Seq);

                            if (photoModel != null)
                            {
                                photoModel.EXTENSION = photo.Extension;
                            }
                            else
                            {
                                db.QS_FORM_PHOTO.Add(new QS_FORM_PHOTO()
                                {
                                    FORMUNIQUEID = Model.UniqueID,
                                    CHECKITEMUNIQUEID = photo.CheckItemUniqueID,
                                    CHECKITEMSEQ = photo.CheckItemSeq,
                                    SEQ = photo.Seq,
                                    EXTENSION = photo.Extension
                                });
                            }

                            var fullFileName = Path.Combine(Config.QSFileFolderPath, string.Format("{0}_{1}_{2}_{3}.{4}", Model.UniqueID, photo.CheckItemUniqueID, photo.CheckItemSeq, photo.Seq, photo.Extension));

                            if (System.IO.File.Exists(fullFileName))
                            {
                                System.IO.File.Delete(fullFileName);
                            }

                            System.IO.File.Move(photo.TempFullFileName, fullFileName);
                        }
                    }

                    db.SaveChanges();
#if !DEBUG
                        trans.Complete();
                    }
#endif

                    result.ReturnSuccessMessage("編輯成功");
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

        public static RequestResult Delete(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    db.QS_AUDITTYPE_FORM.RemoveRange(db.QS_AUDITTYPE_FORM.Where(x => x.FORMUNIQUEID == UniqueID).ToList());
                    db.QS_CHECKITEM_FORM.RemoveRange(db.QS_CHECKITEM_FORM.Where(x => x.FORMUNIQUEID == UniqueID).ToList());
                    db.QS_CHECKITEMREMARK_FORM.RemoveRange(db.QS_CHECKITEMREMARK_FORM.Where(x => x.FORMUNIQUEID == UniqueID).ToList());
                    db.QS_FACTORY_FORM.RemoveRange(db.QS_FACTORY_FORM.Where(x => x.FORMUNIQUEID == UniqueID).ToList());
                    db.QS_FORM.Remove(db.QS_FORM.First(x => x.UNIQUEID == UniqueID));
                    db.QS_FORM_CHECKRESULT.RemoveRange(db.QS_FORM_CHECKRESULT.Where(x => x.FORMUNIQUEID == UniqueID).ToList());
                    db.QS_FORM_LOG.RemoveRange(db.QS_FORM_LOG.Where(x => x.FORMUNIQUEID == UniqueID).ToList());
                    db.QS_FORM_STATION.RemoveRange(db.QS_FORM_STATION.Where(x => x.FORMUNIQUEID == UniqueID).ToList());
                    db.QS_GRADE_FORM.RemoveRange(db.QS_GRADE_FORM.Where(x => x.FORMUNIQUEID == UniqueID).ToList());
                    db.QS_REMARK_FORM.RemoveRange(db.QS_REMARK_FORM.Where(x => x.FORMUNIQUEID == UniqueID).ToList());
                    db.QS_RESDEPARTMENT_FORM.RemoveRange(db.QS_RESDEPARTMENT_FORM.Where(x => x.FORMUNIQUEID == UniqueID).ToList());
                    db.QS_RISK_FORM.RemoveRange(db.QS_RISK_FORM.Where(x => x.FORMUNIQUEID == UniqueID).ToList());
                    db.QS_SHIFT_FORM.RemoveRange(db.QS_SHIFT_FORM.Where(x => x.FORMUNIQUEID == UniqueID).ToList());
                    db.QS_STATION_FORM.RemoveRange(db.QS_STATION_FORM.Where(x => x.FORMUNIQUEID == UniqueID).ToList());

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage("刪除成功");
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

        public static RequestResult Report(List<string> SelectedList)
        {
            RequestResult result = new RequestResult();

            try
            {
                var workBook = new XSSFWorkbook();

                IFont headerFont;
                IFont cellFont;

                ICellStyle headerStyle;
                ICellStyle cellStyle;

                #region Header Style
                headerFont = workBook.CreateFont();
                headerFont.Color = HSSFColor.Black.Index;
                headerFont.Boldweight = (short)FontBoldWeight.Bold;
                headerFont.FontHeightInPoints = 20;

                headerStyle = workBook.CreateCellStyle();
                headerStyle.BorderTop = BorderStyle.Thin;
                headerStyle.BorderBottom = BorderStyle.Thin;
                headerStyle.BorderLeft = BorderStyle.Thin;
                headerStyle.BorderRight = BorderStyle.Thin;
                headerStyle.WrapText = true;
                headerStyle.SetFont(headerFont);
                #endregion

                #region Cell Style
                cellFont = workBook.CreateFont();
                cellFont.Color = HSSFColor.Black.Index;
                cellFont.Boldweight = (short)FontBoldWeight.Normal;
                cellFont.FontHeightInPoints = 12;

                cellStyle = workBook.CreateCellStyle();
                cellStyle.BorderTop = BorderStyle.Thin;
                cellStyle.BorderBottom = BorderStyle.Thin;
                cellStyle.BorderLeft = BorderStyle.Thin;
                cellStyle.BorderRight = BorderStyle.Thin;
                cellStyle.WrapText = true;
                cellStyle.SetFont(cellFont);
                #endregion

                foreach (var uniqueID in SelectedList)
                {
                    result = GetDetailViewModel(uniqueID);

                    if (result.IsSuccess)
                    {
                        workBook = CreateSheet(workBook, headerStyle, cellStyle, result.Data as DetailViewModel);
                    }
                    else
                    {
                        break;
                    }
                }

                var output = new ExcelExportModel(string.Format("內稽查檢表_{0}",DateTimeHelper.DateTime2DateTimeString(DateTime.Now)), Define.EnumExcelVersion._2007);

                using (FileStream fs = System.IO.File.OpenWrite(output.FullFileName))
                {
                    workBook.Write(fs);
                }

                byte[] buff = null;

                using (var fs = System.IO.File.OpenRead(output.FullFileName))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        long numBytes = new FileInfo(output.FullFileName).Length;

                        buff = br.ReadBytes((int)numBytes);

                        br.Close();
                    }

                    fs.Close();
                }

                output.Data = buff;

                result.ReturnData(output);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult MonthlyReport(List<string> SelectedList)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = (from r in db.QS_FORM_CHECKRESULT
                                 join c in db.QS_CHECKITEM_FORM
                                 on new { FormUniqueID = r.FORMUNIQUEID, CheckItemUniqueID = r.CHECKITEMUNIQUEID } equals new { FormUniqueID = c.FORMUNIQUEID, CheckItemUniqueID = c.UNIQUEID }
                                 join form in db.QS_FORM
                                 on r.FORMUNIQUEID equals form.UNIQUEID
                                 join factory in db.QS_FACTORY_FORM
                                 on new { FormUniqueID = form.UNIQUEID, FactoryUniqueID = form.FACTORYUNIQUEID } equals new { FormUniqueID = factory.FORMUNIQUEID, FactoryUniqueID = factory.UNIQUEID } into tmpFactory
                                 from factory in tmpFactory.DefaultIfEmpty()
                                 //join shift in db.QS_SHIFT_FORM
                                 //on new { FormUniqueID = form.UNIQUEID, ShiftUniqueID = form.SHIFTUNIQUEID } equals new { FormUniqueID = shift.FORMUNIQUEID, ShiftUniqueID = shift.UNIQUEID } into tmpShift
                                 //from shift in tmpShift.DefaultIfEmpty()
                                 join auditStation in db.QS_STATION_FORM
                                 on new { FormUniqueID = r.FORMUNIQUEID, StationUniqueID = r.STATIONUNIQUEID } equals new { FormUniqueID = auditStation.FORMUNIQUEID, StationUniqueID = auditStation.UNIQUEID } into tmpAuditStation
                                 from auditStation in tmpAuditStation.DefaultIfEmpty()
                                 join auditType in db.QS_AUDITTYPE_FORM
                                 on new { FormUniqueID = r.FORMUNIQUEID, AuditTypeUniqueID = r.AUDITTYPEUNIQUEID } equals new { FormUniqueID = auditType.FORMUNIQUEID, AuditTypeUniqueID = auditType.UNIQUEID } into tmpAuditType
                                 from auditType in tmpAuditType.DefaultIfEmpty()
                                 join belongShift in db.QS_SHIFT_FORM
                                 on new { FormUniqueID = r.FORMUNIQUEID, BelongShiftUniqueID = r.BELONGSHIFTUNIQUEID } equals new { FormUniqueID = belongShift.FORMUNIQUEID, BelongShiftUniqueID = belongShift.UNIQUEID } into tmpBelongShift
                                 from belongShift in tmpBelongShift.DefaultIfEmpty()


                                 join auditorShift in db.QS_SHIFT_FORM
                                on new { FormUniqueID = r.FORMUNIQUEID, auditorShiftUniqueID = r.AUDITORSHIFTUNIQUEID } equals new { FormUniqueID = auditorShift.FORMUNIQUEID, auditorShiftUniqueID = auditorShift.UNIQUEID } into tmpAuditorShift
                                 from auditorShift in tmpAuditorShift.DefaultIfEmpty()

                                 join carOwner in db.ACCOUNT
                                 on r.CAROWNERID equals carOwner.ID into tmpCarOwner
                                 from carOwner in tmpCarOwner.DefaultIfEmpty()
                                 join carOwnerManager in db.ACCOUNT
                                 on r.CAROWNERMANAGERID equals carOwnerManager.ID into tmpCarOwnerManager
                                 from carOwnerManager in tmpCarOwnerManager.DefaultIfEmpty()
                                 join departmentManager in db.ACCOUNT
                                 on r.DEPARTMENTMANAGERID equals departmentManager.ID into tmpDepartmentManager
                                 from departmentManager in tmpDepartmentManager.DefaultIfEmpty()
                                 join errorUser in db.ACCOUNT
                                 on r.ERRORUSERID equals errorUser.ID into tmpErrorUser
                                 from errorUser in tmpErrorUser.DefaultIfEmpty()
                                 join risk in db.QS_RISK_FORM
                                 on new { FormUniqueID = r.FORMUNIQUEID, RiskUniqueID = r.RISKUNIQUEID } equals new { FormUniqueID = risk.FORMUNIQUEID, RiskUniqueID = risk.UNIQUEID } into tmpRisk
                                 from risk in tmpRisk.DefaultIfEmpty()
                                 join grade in db.QS_GRADE_FORM
                                 on new { FormUniqueID = r.FORMUNIQUEID, GradeUniqueID = r.GRADEUNIQUEID } equals new { FormUniqueID = grade.FORMUNIQUEID, GradeUniqueID = grade.UNIQUEID } into tmpGrade
                                 from grade in tmpGrade.DefaultIfEmpty()
                                 where SelectedList.Contains(r.FORMUNIQUEID) && r.RESULT == "N"
                                 select new
                                 {
                                     Form = form,
                                     FactoryDescription = factory != null ? factory.DESCRIPTION : "",
                                     AuditorShiftDescription = auditorShift != null ? auditorShift.DESCRIPTION : "",
                                     Result = r,
                                     CheckItemUniqueID = c.UNIQUEID,
                                     CheckTypeID = c.TYPEID,
                                     CheckItemID = c.ID,
                                     AuditStation = auditStation != null ? auditStation.DESCRIPTION : "",
                                     AuditType = auditType != null ? auditType.DESCRIPTION : "",
                                     BelongShift = belongShift != null ? belongShift.DESCRIPTION : "",
                                     CarOwnerName = carOwner != null ? carOwner.NAME : "",
                                     CarOwnerManagerName = carOwnerManager != null ? carOwnerManager.NAME : "",
                                     DepartmentManagerName = departmentManager != null ? departmentManager.NAME : "",
                                     ErrorUserName = errorUser!=null?errorUser.NAME:"",
                                     Risk = risk != null ? risk.DESCRIPTION : "",
                                     Grade = grade != null ? grade.DESCRIPTION : ""
                                 }).ToList();

                    var itemList = query.Select(x => new MonthlyReportItem
                    {
                        CarNo = x.Result.CARNO,
                         CPNO=x.Result.CPNO,
                          ErrorArea=x.Result.ERRORAREA,
                           ErrorMachineNo=x.Result.ERRORMACHINENO,
                            ErrorUserID=x.Result.ERRORUSERID,
                            ErrorUserName = x.ErrorUserName,
                              VHNO=x.Form.VHNO,
                        Weekly = x.Result.WEEKLY,
                        AuditType = x.AuditType,
                        AuditDate = x.Form.AUDITDATE,
                        ShiftUniqueID = x.Form.SHIFTUNIQUEID,
                        ShiftDescription = x.AuditorShiftDescription,
                        ShiftRemark = x.Form.SHIFTREMARK,
                        FactoryUniqueID = x.Form.FACTORYUNIQUEID,
                        FactoryDescription = x.FactoryDescription,
                        FactoryRemark = x.Form.FACTORYREMARK,
                        StationList = (from y in db.QS_FORM_STATION
                                       join s in db.QS_STATION_FORM
                                       on new { FormUniqueID = y.FORMUNIQUEID, StationUniqueID = y.STATIONUNIQUEID } equals new { FormUniqueID = s.FORMUNIQUEID, StationUniqueID = s.UNIQUEID } into tmpStation
                                       from s in tmpStation.DefaultIfEmpty()
                                       where y.FORMUNIQUEID == x.Form.UNIQUEID
                                       select new
                                       {
                                           y.STATIONUNIQUEID,
                                           StationDescription = s != null ? s.DESCRIPTION : "",
                                           y.STATIONREMARK
                                       }).ToList().Select(y => y.STATIONUNIQUEID == Define.OTHER ? y.STATIONREMARK : y.StationDescription).OrderBy(y => y).ToList(),
                        CheckTypeID = x.CheckTypeID,
                        CheckItemID = x.CheckItemID,
                        AuditStation = x.AuditStation,
                        ResDepartments = x.Result.RESDEPARTMENTS,
                        ResDepartmentDescriptionList = new List<string>() { },
                        BelongShift = x.BelongShift,
                        CarOwnerID = x.Result.CAROWNERID,
                        CarOwnerName = x.CarOwnerName,
                        CarOwnerManagerID = x.Result.CAROWNERMANAGERID,
                        CarOwnerManagerName = x.CarOwnerManagerName,
                        DepartmentManagerID = x.Result.DEPARTMENTMANAGERID,
                        DepartmentManagerName = x.DepartmentManagerName,
                        Risk = x.Risk,
                        Grade = x.Grade,
                        IsBelongMO = x.Result.ISBELONGMO,
                        Remark = x.Result.REMARK,
                        PhotoList = db.QS_FORM_PHOTO.Where(p => p.FORMUNIQUEID == x.Form.UNIQUEID && p.CHECKITEMUNIQUEID == x.CheckItemUniqueID && p.CHECKITEMSEQ == x.Result.SEQ).ToList().Select(p => new PhotoModel
                        {
                            FormUniqueID = x.Form.UNIQUEID,
                            CheckItemUniqueID = x.CheckItemUniqueID,
                            CheckItemSeq = Convert.ToInt32(x.Result.SEQ),
                            Seq = Convert.ToInt32(p.SEQ),
                            Extension = p.EXTENSION,
                            IsSaved = true
                        }).ToList()
                    }).ToList();


                    IWorkbook workbook = new XSSFWorkbook();

                    #region WorkBook Style
                    IFont headerFont;
                    IFont cellFont;

                    ICellStyle headerStyle;
                    ICellStyle cellStyle;
                    #endregion

                    #region Header Style
                    headerFont = workbook.CreateFont();
                    headerFont.Color = HSSFColor.Black.Index;
                    headerFont.Boldweight = (short)FontBoldWeight.Bold;
                    headerFont.FontHeightInPoints = 12;

                    headerStyle = workbook.CreateCellStyle();
                    headerStyle.FillForegroundColor = HSSFColor.Grey25Percent.Index;
                    headerStyle.FillPattern = FillPattern.SolidForeground;
                    headerStyle.BorderTop = BorderStyle.Thin;
                    headerStyle.BorderBottom = BorderStyle.Thin;
                    headerStyle.BorderLeft = BorderStyle.Thin;
                    headerStyle.BorderRight = BorderStyle.Thin;
                    headerStyle.SetFont(headerFont);
                    #endregion

                    #region Cell Style
                    cellFont = workbook.CreateFont();
                    cellFont.Color = HSSFColor.Black.Index;
                    cellFont.Boldweight = (short)FontBoldWeight.Normal;
                    cellFont.FontHeightInPoints = 12;

                    cellStyle = workbook.CreateCellStyle();
                    cellStyle.VerticalAlignment = VerticalAlignment.Center;
                    cellStyle.BorderTop = BorderStyle.Thin;
                    cellStyle.BorderBottom = BorderStyle.Thin;
                    cellStyle.BorderLeft = BorderStyle.Thin;
                    cellStyle.BorderRight = BorderStyle.Thin;
                    cellStyle.SetFont(cellFont);
                    #endregion

                    IRow row;

                    ICell cell;

                    var rowIndex = 0;

                    #region Sheet 2
                    var worksheet = workbook.CreateSheet(Resources.Resource.CheckResult);

                    row = worksheet.CreateRow(0);

                    #region Header
                    cell = row.CreateCell(0);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("單號");

                    cell = row.CreateCell(1);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("C.P No.");

                    cell = row.CreateCell(2);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("CAR No.");

                    cell = row.CreateCell(3);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Weekly週別");

                    cell = row.CreateCell(4);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("稽核性質");

                    cell = row.CreateCell(5);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Audit Date日期");

                    cell = row.CreateCell(6);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Shift班別");

                    cell = row.CreateCell(7);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Plant廠別");

                    cell = row.CreateCell(8);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Station");

                    cell = row.CreateCell(9);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Item稽核項目");

                    cell = row.CreateCell(10);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Detail Item稽核細項");

                    cell = row.CreateCell(11);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Process製程站別");

                    cell = row.CreateCell(12);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Dept.缺失部門(負責部門)");

                    cell = row.CreateCell(13);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("問題發生歸屬班別");

                    cell = row.CreateCell(14);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("CAR Owner");

                    cell = row.CreateCell(15);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("CAR Owner' Boss");

                    cell = row.CreateCell(16);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Dept. Magr");

                    cell = row.CreateCell(17);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Grade/Risk風險等級");

                    cell = row.CreateCell(18);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Grade");

                    cell = row.CreateCell(19);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Owner確認(Y/N)是否歸屬MO");

                    cell = row.CreateCell(20);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Describe稽核缺失內容");

                    cell = row.CreateCell(21);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Badge缺失人員工號");

                    cell = row.CreateCell(22);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("M/C No.機台編號");

                    cell = row.CreateCell(23);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Location區域");

                    int cellIndex = 24;
                    var maxCount = itemList.Where(x => x.PhotoList != null && x.PhotoList.Count > 0).Max(x => x.PhotoList.Count);
                    var maxCellIndex = cellIndex + maxCount;

                    for (cellIndex = 24; cellIndex < maxCellIndex; cellIndex++)
                    {
                        cell = row.CreateCell(cellIndex);
                        cell.CellStyle = headerStyle;
                        cell.SetCellValue(string.Format("Picture缺失照片{0}", cellIndex - 23));
                        worksheet.SetColumnWidth(cellIndex, 20 * 256);
                    }

                    #endregion

                    rowIndex = 1;

                    foreach (var item in itemList)
                    {
                        row = worksheet.CreateRow(rowIndex);

                        if (item.PhotoList.Count > 0)
                        {
                            row.HeightInPoints = 120;
                        }

                        cell = row.CreateCell(0);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.VHNO);

                        cell = row.CreateCell(1);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.CPNO);

                        cell = row.CreateCell(2);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.CarNo);

                        cell = row.CreateCell(3);
                        cell.CellStyle = cellStyle;
                        if (item.Weekly.HasValue)
                        {
                            cell.SetCellValue(item.Weekly.Value.ToString()); 
                        }

                        cell = row.CreateCell(4);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.AuditType);

                        cell = row.CreateCell(5);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.AuditDateString);

                        cell = row.CreateCell(6);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.Shift);

                        cell = row.CreateCell(7);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.Factory);

                        cell = row.CreateCell(8);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.Stations);

                        cell = row.CreateCell(9);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.CheckTypeID.ToString());

                        cell = row.CreateCell(10);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.CheckItemID.ToString());

                        cell = row.CreateCell(11);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.AuditStation);

                        cell = row.CreateCell(12);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.ResDepartmentDescription);

                        cell = row.CreateCell(13);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.BelongShift);

                        cell = row.CreateCell(14);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.CarOwner);

                        cell = row.CreateCell(15);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.CarOwnerManager);

                        cell = row.CreateCell(16);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.DepartmentManager);

                        cell = row.CreateCell(17);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.Risk);

                        cell = row.CreateCell(18);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.Grade);

                        cell = row.CreateCell(19);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.IsBelongMO);

                        cell = row.CreateCell(20);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.Remark);

                        cell = row.CreateCell(21);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.ErrorUser);

                        cell = row.CreateCell(22);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.ErrorMachineNo);

                        cell = row.CreateCell(23);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.ErrorArea);

                        for (cellIndex = 24; cellIndex < maxCellIndex; cellIndex++)
                        {
                            cell = row.CreateCell(cellIndex);
                            cell.CellStyle = cellStyle;

                            if (item.PhotoList.Count > cellIndex - 24)
                            {
                                var photo = item.PhotoList[cellIndex - 24];

                                try
                                {
                                    byte[] bytes = File.ReadAllBytes(photo.FullFileName);

                                    int pictureIndex = workbook.AddPicture(bytes, PictureType.JPEG);

                                    var patriarch = worksheet.CreateDrawingPatriarch();

                                    var anchor = new XSSFClientAnchor(0, 0, 0, 0, cellIndex, rowIndex, cellIndex+1, rowIndex + 1);

                                    patriarch.CreatePicture(anchor, pictureIndex);
                                }
                                catch (Exception ex)
                                {
                                    Logger.Log(MethodBase.GetCurrentMethod(), ex);
                                }
                            }
                        }

                        rowIndex++;
                    }
                    #endregion

                    var model = new ExcelExportModel(string.Format("Internal audit weekly trend_{0}", DateTimeHelper.DateTime2DateTimeString(DateTime.Now)), Define.EnumExcelVersion._2007);

                    using (var fs = File.OpenWrite(model.FullFileName))
                    {
                        workbook.Write(fs);

                        fs.Close();
                    }

                    byte[] buff = null;

                    using (var fs = File.OpenRead(model.FullFileName))
                    {
                        using (BinaryReader br = new BinaryReader(fs))
                        {
                            long numBytes = new FileInfo(model.FullFileName).Length;

                            buff = br.ReadBytes((int)numBytes);

                            br.Close();
                        }

                        fs.Close();
                    }

                    model.Data = buff;

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

        private static XSSFWorkbook CreateSheet(XSSFWorkbook WorkBook, ICellStyle HeaderStyle, ICellStyle CellStyle, DetailViewModel Model)
        {
            var sheet = WorkBook.CreateSheet(Model.VHNO);

            IRow row;

            ICell cell;

            var sb = new StringBuilder();

            #region Row 0
            row = sheet.CreateRow(0);

            cell = row.CreateCell(0);
            cell.CellStyle = HeaderStyle;
            cell.SetCellValue("PROCESS AUDIT CHECKLIST");

            for (int i = 1; i <= 12; i++)
            {
                cell = row.CreateCell(i);
                cell.CellStyle = HeaderStyle;
            }

            sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 12));
            #endregion

            #region Row 1
            row = sheet.CreateRow(1);

            cell = row.CreateCell(0);
            cell.CellStyle = HeaderStyle;
            cell.SetCellValue("內 稽 查 檢 表");

            for (int i = 1; i <= 12; i++)
            {
                cell = row.CreateCell(i);
                cell.CellStyle = HeaderStyle;
            }

            sheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 12));
            #endregion

            #region Row 2
            row = sheet.CreateRow(2);

            cell = row.CreateCell(0);
            cell.CellStyle = HeaderStyle;
            cell.SetCellValue(string.Format("受稽廠別：{0}", Model.Factory));

            for (int i = 1; i <= 12; i++)
            {
                cell = row.CreateCell(i);
                cell.CellStyle = HeaderStyle;
            }

            sheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 12));
            #endregion

            #region Row 3
            row = sheet.CreateRow(3);

            cell = row.CreateCell(0);
            cell.CellStyle = HeaderStyle;
            cell.SetCellValue(string.Format("Shift 受稽班別：{0}", Model.Shift));

            for (int i = 1; i <= 8; i++)
            {
                cell = row.CreateCell(i);
                cell.CellStyle = HeaderStyle;
            }

            sheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 9));

            cell = row.CreateCell(9);
            cell.CellStyle = CellStyle;

            cell = row.CreateCell(10);
            cell.CellStyle = CellStyle;
            cell.SetCellValue("Audit Leader 稽核主管：");
            
            cell = row.CreateCell(11);
            cell.CellStyle = CellStyle;
            cell.SetCellValue(Model.AuditorManager);

            cell = row.CreateCell(12);
            cell.CellStyle = CellStyle;

            sheet.AddMergedRegion(new CellRangeAddress(3, 3, 11, 12));
            #endregion

            #region Row 4
            row = sheet.CreateRow(4);

            cell = row.CreateCell(0);
            cell.CellStyle = HeaderStyle;
            cell.SetCellValue(string.Format("Station 受稽站別：{0}", Model.Stations));

            for (int i = 1; i <= 8; i++)
            {
                cell = row.CreateCell(i);
                cell.CellStyle = HeaderStyle;
            }

            sheet.AddMergedRegion(new CellRangeAddress(4, 5, 0, 9));

            cell = row.CreateCell(9);
            cell.CellStyle = CellStyle;
            

            cell = row.CreateCell(10);
            cell.CellStyle = CellStyle;
            cell.SetCellValue("Auditor 稽核人員：");
            
            cell = row.CreateCell(11);
            cell.CellStyle = CellStyle;
            cell.SetCellValue(Model.Auditor);

            cell = row.CreateCell(12);
            cell.CellStyle = CellStyle;

            sheet.AddMergedRegion(new CellRangeAddress(4, 4, 11, 12));
            #endregion

            #region Row 5
            row = sheet.CreateRow(5);

            for (int i = 0; i <= 8; i++)
            {
                cell = row.CreateCell(i);
                cell.CellStyle = HeaderStyle;
            }

            cell = row.CreateCell(9);
            cell.CellStyle = CellStyle;
            

            cell = row.CreateCell(10);
            cell.CellStyle = CellStyle;
            cell.SetCellValue("Audit Date 稽核日期：");
            

            cell = row.CreateCell(11);
            cell.CellStyle = CellStyle;
            cell.SetCellValue(Model.AuditDateString);

            cell = row.CreateCell(12);
            cell.CellStyle = CellStyle;

            sheet.AddMergedRegion(new CellRangeAddress(5, 5, 11, 12));
            #endregion

            #region Row 6
            row = sheet.CreateRow(6);

            cell = row.CreateCell(0);
            cell.CellStyle = HeaderStyle;
            sb = new StringBuilder();
            sb.AppendLine("Check Items");
            sb.AppendLine("稽核項目");
            cell.SetCellValue(sb.ToString());

            cell = row.CreateCell(1);
            cell.CellStyle = HeaderStyle;

            sheet.AddMergedRegion(new CellRangeAddress(6, 8, 0, 1));

            cell = row.CreateCell(2);
            cell.CellStyle = CellStyle;
            sb = new StringBuilder();
            sb.AppendLine("Audit Sampling");
            sb.AppendLine("稽核抽樣");

            cell.SetCellValue(sb.ToString());

            sheet.AddMergedRegion(new CellRangeAddress(6, 8, 2, 2));

            cell = row.CreateCell(3);
            cell.CellStyle = CellStyle;
            sb = new StringBuilder();
            sb.AppendLine("Process Station");
            sb.AppendLine("製程站別");
            cell.SetCellValue(sb.ToString());

            sheet.AddMergedRegion(new CellRangeAddress(6, 8, 3, 3));

            cell = row.CreateCell(4);
            cell.CellStyle = CellStyle;
            cell.SetCellValue("Audit record 稽核記錄");

            cell = row.CreateCell(5);
            cell.CellStyle = CellStyle;

            cell = row.CreateCell(6);
            cell.CellStyle = CellStyle;

            cell = row.CreateCell(7);
            cell.CellStyle = CellStyle;

            sheet.AddMergedRegion(new CellRangeAddress(6, 6, 4, 7));

            cell = row.CreateCell(8);
            cell.CellStyle = CellStyle;
            sb = new StringBuilder();
            sb.AppendLine("e-CAR No.(non-conform only)");
            sb.AppendLine("改正追蹤系統單號(不符合事項發現時)");
            cell.SetCellValue(sb.ToString());
            sheet.AddMergedRegion(new CellRangeAddress(6, 8, 8, 8));

            cell = row.CreateCell(9);
            cell.CellStyle = CellStyle;

            sb = new StringBuilder();
            sb.AppendLine("Remark 備註");
            sb.AppendLine("(稽核所見事實/有爭議的內容 / 缺點描述)");
            cell.SetCellValue(sb.ToString());

            cell = row.CreateCell(10);
            cell.CellStyle = CellStyle;
            
            cell = row.CreateCell(11);
            cell.CellStyle = CellStyle;

            cell = row.CreateCell(12);
            cell.CellStyle = CellStyle;

            sheet.AddMergedRegion(new CellRangeAddress(6, 8, 9, 12));
            #endregion

            #region Row 7
            row = sheet.CreateRow(7);

            cell = row.CreateCell(0);
            cell.CellStyle = CellStyle;

            cell = row.CreateCell(1);
            cell.CellStyle = CellStyle;

            cell = row.CreateCell(2);
            cell.CellStyle = CellStyle;

            cell = row.CreateCell(3);
            cell.CellStyle = CellStyle;

            cell = row.CreateCell(4);
            cell.CellStyle = CellStyle;

            sb = new StringBuilder();
            sb.AppendLine("Auditee/Machine/Document/Area");
            sb.AppendLine("受稽OP/機台/文件/區域");
            cell.SetCellValue(sb.ToString());

            sheet.AddMergedRegion(new CellRangeAddress(7, 8, 4, 4));

            cell = row.CreateCell(5);
            cell.CellStyle = CellStyle;

            sb = new StringBuilder();
            sb.AppendLine("(QA/MFG/PE/ME/Warehouse/Shipping/Others)");
            sb.AppendLine("負責部門");

            cell.SetCellValue(sb.ToString());

            sheet.AddMergedRegion(new CellRangeAddress(7, 8, 5, 5));

            cell = row.CreateCell(6);
            cell.CellStyle = CellStyle;

            sb = new StringBuilder();
            sb.AppendLine("Audit Result");
            sb.AppendLine("稽核結果");
            cell.SetCellValue(sb.ToString());

            cell = row.CreateCell(7);
            cell.CellStyle = CellStyle;

            sheet.AddMergedRegion(new CellRangeAddress(7, 7, 6, 7));

            cell = row.CreateCell(8);
            cell.CellStyle = CellStyle;
            cell = row.CreateCell(9);
            cell.CellStyle = CellStyle;
            cell = row.CreateCell(10);
            cell.CellStyle = CellStyle;
            cell = row.CreateCell(11);
            cell.CellStyle = CellStyle;
            cell = row.CreateCell(12);
            cell.CellStyle = CellStyle;
            #endregion

            #region Row 8
            row = sheet.CreateRow(8);

            cell = row.CreateCell(0);
            cell.CellStyle = CellStyle;
            cell = row.CreateCell(1);
            cell.CellStyle = CellStyle;
            cell = row.CreateCell(2);
            cell.CellStyle = CellStyle;
            cell = row.CreateCell(3);
            cell.CellStyle = CellStyle;
            cell = row.CreateCell(4);
            cell.CellStyle = CellStyle;
            cell = row.CreateCell(5);
            cell.CellStyle = CellStyle;

            cell = row.CreateCell(6);
            cell.CellStyle = CellStyle;

            sb = new StringBuilder();
            sb.AppendLine("Conform");
            sb.AppendLine("符合");

            cell.SetCellValue(sb.ToString());

            cell = row.CreateCell(7);
            cell.CellStyle = CellStyle;

            sb = new StringBuilder();
            sb.AppendLine("Non-conform");
            sb.AppendLine("不符合");

            cell.SetCellValue(sb.ToString());

            cell = row.CreateCell(8);
            cell.CellStyle = CellStyle;
            cell = row.CreateCell(9);
            cell.CellStyle = CellStyle;
            cell = row.CreateCell(10);
            cell.CellStyle = CellStyle;
            cell = row.CreateCell(11);
            cell.CellStyle = CellStyle;
            cell = row.CreateCell(12);
            cell.CellStyle = CellStyle;
            #endregion

            #region CheckResult
            var rowIndex = 9;

            foreach (var checkType in Model.CheckTypeList)
            {
                row = sheet.CreateRow(rowIndex);

                cell = row.CreateCell(0);
                cell.CellStyle = CellStyle;
                cell.SetCellValue(checkType.ID.ToString());

                cell = row.CreateCell(1);
                cell.CellStyle = CellStyle;
                cell.SetCellValue(string.Format("{0} {1}", checkType.EDescription, checkType.CDescription));

                for (int i = 2; i <= 12; i++)
                {
                    cell = row.CreateCell(i);
                    cell.CellStyle = CellStyle;
                }

                sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 1, 12));

                rowIndex++;

                foreach (var checkItem in checkType.CheckItemList)
                {
                    row = sheet.CreateRow(rowIndex);

                    cell = row.CreateCell(0);
                    cell.CellStyle = CellStyle;
                    cell.SetCellValue(checkItem.ID.ToString());

                    cell = row.CreateCell(1);
                    cell.CellStyle = CellStyle;
                    cell.SetCellValue(string.Format("{0} {1}", checkItem.EDescription, checkItem.CDescription));

                    cell = row.CreateCell(2);
                    cell.CellStyle = CellStyle;
                    cell.SetCellValue(string.Format("{0}{1}/次", checkItem.CheckTimes, checkItem.Unit));

                    var checkTimes = Convert.ToInt32(checkItem.CheckTimes);

                    sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex + checkTimes - 1, 0, 0));
                    sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex + checkTimes - 1, 1, 1));
                    sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex + checkTimes - 1, 2, 2));

                    var checkResult = checkItem.CheckResultList.OrderBy(x => x.Seq).FirstOrDefault();

                    cell = row.CreateCell(3);
                    cell.CellStyle = CellStyle;
                    if (checkResult != null)
                    {
                        cell.SetCellValue(checkResult.Station);
                    }

                    cell = row.CreateCell(4);
                    cell.CellStyle = CellStyle;
                    if (checkResult != null)
                    {
                        cell.SetCellValue(checkResult.AuditObject);
                    }

                    cell = row.CreateCell(5);
                    cell.CellStyle = CellStyle;
                    if (checkResult != null)
                    {
                        cell.SetCellValue(checkResult.ResDepartmentDescription);
                    }

                    cell = row.CreateCell(6);
                    cell.CellStyle = CellStyle;
                    if (checkResult != null && checkResult.Result == "Y")
                    {
                        cell.SetCellValue("V");
                    }

                    if (checkResult != null && checkResult.Result == "0")
                    {
                        cell.SetCellValue("無此項目");
                    }

                    cell = row.CreateCell(7);
                    cell.CellStyle = CellStyle;
                    if (checkResult != null && checkResult.Result == "N")
                    {
                        cell.SetCellValue("V");
                    }

                    if (checkResult != null && checkResult.Result == "0")
                    {
                        sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 6, 7));
                    }

                    cell = row.CreateCell(8);
                    cell.CellStyle = CellStyle;
                    if (checkResult != null)
                    {
                        cell.SetCellValue(checkResult.CarNo);
                    }

                    cell = row.CreateCell(9);
                    cell.CellStyle = CellStyle;
                    if (checkResult != null)
                    {
                        cell.SetCellValue(checkResult.Remark);
                    }

                    cell = row.CreateCell(10);
                    cell.CellStyle = CellStyle;

                    cell = row.CreateCell(11);
                    cell.CellStyle = CellStyle;

                    cell = row.CreateCell(12);
                    cell.CellStyle = CellStyle;

                    sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 9, 12));

                    

                    rowIndex++;

                    for (int i = 1; i < checkTimes; i++)
                    {
                        row = sheet.CreateRow(rowIndex);

                        cell = row.CreateCell(0);
                        cell.CellStyle = CellStyle;
                        cell = row.CreateCell(1);
                        cell.CellStyle = CellStyle;
                        cell = row.CreateCell(2);
                        cell.CellStyle = CellStyle;

                        checkResult = checkItem.CheckResultList.FirstOrDefault(x => x.Seq == i + 1);

                        cell = row.CreateCell(3);
                        cell.CellStyle = CellStyle;
                        if (checkResult != null)
                        {
                            cell.SetCellValue(checkResult.Station);
                        }

                        cell = row.CreateCell(4);
                        cell.CellStyle = CellStyle;
                        if (checkResult != null)
                        {
                            cell.SetCellValue(checkResult.AuditObject);
                        }

                        cell = row.CreateCell(5);
                        cell.CellStyle = CellStyle;
                        if (checkResult != null)
                        {
                            cell.SetCellValue(checkResult.ResDepartmentDescription);
                        }

                        cell = row.CreateCell(6);
                        cell.CellStyle = CellStyle;
                        if (checkResult != null && checkResult.Result == "Y")
                        {
                            cell.SetCellValue("V");
                        }

                        if (checkResult != null && checkResult.Result == "0")
                        {
                            cell.SetCellValue("無此項目");
                        }

                        cell = row.CreateCell(7);
                        cell.CellStyle = CellStyle;
                        if (checkResult != null && checkResult.Result == "N")
                        {
                            cell.SetCellValue("V");
                        }

                        if (checkResult != null && checkResult.Result == "0")
                        {
                            sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 6, 7));
                        }

                        cell = row.CreateCell(8);
                        cell.CellStyle = CellStyle;
                        if (checkResult != null)
                        {
                            cell.SetCellValue(checkResult.CarNo);
                        }

                        cell = row.CreateCell(9);
                        cell.CellStyle = CellStyle;
                        if (checkResult != null)
                        {
                            cell.SetCellValue(checkResult.Remark);
                        }

                        cell = row.CreateCell(10);
                        cell.CellStyle = CellStyle;

                        cell = row.CreateCell(11);
                        cell.CellStyle = CellStyle;

                        cell = row.CreateCell(12);
                        cell.CellStyle = CellStyle;

                        sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 9, 12));

                       

                        rowIndex++;
                    }
                }
            }

            #endregion

            row = sheet.CreateRow(rowIndex);

            cell = row.CreateCell(0);
            cell.CellStyle = CellStyle;

            sb = new StringBuilder();

            sb.AppendLine("Ckecklist Fill Out Notice表單填寫注意事項");
            sb.AppendLine("1. If audit result is compliant with audit item,  please mark the check in \"conform\" column. If not, please mark the check in \"Nonconform\" column.");
            sb.AppendLine("    若稽核結果與稽核項目一致則勾選「符合」；反之，勾選「不符合」。");
            sb.AppendLine("2. If \"Nonconform\" is marked, the auditee needs to sign in \"signature\" that  represents the nonconformance is confirmed by auditee.");
            sb.AppendLine("   若勾選「不符合」，需請受稽者在「確認者簽名」處簽名，表示此缺失成立。");
            sb.AppendLine("3. If \"Nonconform\" is marked and there is a disputation between auditor and auditee, please mark in \"Wait confirm.\" It represents that  this finding has to wait advanced clarification.");
            sb.AppendLine("   若勾選「不符合」，而受稽者對此缺失存有爭議，請勾選「待確認」，表示此缺失待進一步釐清。");
            sb.AppendLine("4. If any audit item is not applicable, please fill in N/A in \"Remark.\"");
            sb.AppendLine("   若稽核項目不適用，請在「備註」填寫N/A。");

            cell.SetCellValue(sb.ToString());

            for (int i = 1; i <= 12; i++)
            {
                cell = row.CreateCell(i);
                cell.CellStyle = CellStyle;
            }

            sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 0, 12));

            sheet.SetColumnWidth(0, 7 * 256);
            sheet.SetColumnWidth(1, 90 * 256);
            sheet.SetColumnWidth(2, 19 * 256);
            sheet.SetColumnWidth(3, 12 * 256);
            sheet.SetColumnWidth(4, 21 * 256);
            sheet.SetColumnWidth(5, 10 * 256);
            sheet.SetColumnWidth(6, 13 * 256);
            sheet.SetColumnWidth(7, 14 * 256);
            sheet.SetColumnWidth(8, 14 * 256);
            sheet.SetColumnWidth(9, 42 * 256);
            sheet.SetColumnWidth(10, 37 * 256);
            sheet.SetColumnWidth(11, 12 * 256);
            sheet.SetColumnWidth(12, 9 * 256);

            return WorkBook;
        }
    }
}
