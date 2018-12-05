using Accord.Statistics.Testing;
using DbEntity.ASE;
using Models.ASE.QA.MSAForm_v2;
using Models.Authenticated;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace DataAccess.ASE.QA
{
    public class MSAForm_v2DataAccessor
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

        public static RequestResult Return(List<Models.Shared.Organization> OrganizationList, string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = db.QA_MSAFORM.First(x => x.UNIQUEID == UniqueID);

                    var vhnoPreFix = form.VHNO.Substring(0, 11);

                    var formList = db.QA_MSAFORM.Where(x => x.VHNO.StartsWith(vhnoPreFix)).ToList();

                    foreach (var f in formList)
                    {
                        f.STATUS = "6";
                    }

                    var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == form.EQUIPMENTUNIQUEID);

                    var nextMSADate = equipment.NEXTMSADATE;

                    if (!equipment.NEXTMSADATE.HasValue)
                    {
                        nextMSADate = form.ESTMSADATE;
                    }

                    vhnoPreFix = string.Format("R{0}", nextMSADate.Value.ToString("yyyyMM").Substring(2));

                    var tmp = db.QA_MSANOTIFY.Where(x => x.VHNO.StartsWith(vhnoPreFix)).OrderByDescending(x => x.VHNO).ToList();

                    int vhnoSeq = 1;

                    if (tmp.Count > 0)
                    {
                        vhnoSeq = int.Parse(tmp.First().VHNO.Substring(5)) + 1;
                    }

                    var vhno = string.Format("{0}{1}", vhnoPreFix, vhnoSeq.ToString().PadLeft(4, '0'));

                    var notifyUniqueID = Guid.NewGuid().ToString();

                    var time = DateTime.Now;

                    var notify = new QA_MSANOTIFY()
                    {
                        UNIQUEID = notifyUniqueID,
                        EQUIPMENTUNIQUEID = equipment.UNIQUEID,
                        CREATETIME = time,
                        VHNO = vhno,
                        STATUS = "1",
                        ESTMSADATE = nextMSADate,
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
                        USERID = "SYS",
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

                    db.SaveChanges();

                    result.ReturnSuccessMessage(string.Format("取消立案完成，已重新轉立MSA通知單[{0}]", vhno));

                    MSANotifyDataAccessor.SendVerifyMail(OrganizationList, equipment.PEID, notify);
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
                    var query = (from x in db.QA_MSAFORM
                                 join e in db.QA_EQUIPMENT
                                 on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                 where x.EQUIPMENTUNIQUEID == EquipmentUniqueID
                                 select new
                                 {
                                     UniqueID = x.UNIQUEID,
                                     x.VHNO,
                                     x.TYPE,
                                     x.SUBTYPE,
                                     Status = x.STATUS,
                                     CalNo = e.MSACALNO,
                                     OrganizationUniqueID = e.ORGANIZATIONUNIQUEID,
                                     MSAResponsorID = x.MSARESPONSORID,
                                     OwnerID = e.OWNERID,
                                     OwnerManagerID = e.OWNERMANAGERID,
                                     PEID = e.PEID,
                                     PEManagerID = e.PEMANAGERID,
                                     EstMSADate = x.ESTMSADATE,
                                     MSADate = x.MSADATE,
                                     CreateTime = x.CREATETIME,
                                     Station = x.STATION,
                                     MSAIchi = x.MSAICHI,
                                     Characteristic = x.CHARACTERISITIC,
                                     LowerRange = x.LOWERRANGE,
                                     UpperRange = x.UPPERRANGE
                                 }).AsQueryable();

                    var temp = query.OrderByDescending(x => x.VHNO).ToList();

                    foreach (var t in temp)
                    {
                        var msaReponsor = db.ACCOUNT.FirstOrDefault(x => x.ID == t.MSAResponsorID);

                        model.Add(new GridItem()
                        {
                            UniqueID = t.UniqueID,
                            VHNO = t.VHNO,
                            Type = t.TYPE,
                            SubType = t.SUBTYPE,
                            Status = new FormStatus(t.Status, t.EstMSADate.Value),
                            Factory = OrganizationDataAccessor.GetFactory(OrganizationList, t.OrganizationUniqueID),
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(t.OrganizationUniqueID),
                            CalNo = t.CalNo,
                            MSAIchi = t.MSAIchi,
                            MSACharacteristic = t.Characteristic,
                            MSALowerRange = t.LowerRange,
                            MSAUpperRange = t.UpperRange,
                            Station = t.Station,
                            EstMSADate = t.EstMSADate.Value,
                            MSADate = t.MSADate,
                            MSAResponsorID = t.MSAResponsorID,
                            MSAResponsorName = msaReponsor != null ? msaReponsor.NAME : string.Empty,
                            CreateTime = t.CreateTime.Value
                        });
                    }

                    model = model.OrderBy(x => x.EstMSADateString).ToList();
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
                    var query = (from x in db.QA_MSAFORM
                                 join e in db.QA_EQUIPMENT
                                 on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                 join i in db.QA_ICHI on e.ICHIUNIQUEID equals i.UNIQUEID into tmpIchi
                                 from i in tmpIchi.DefaultIfEmpty()
                                 select new
                                 {
                                     UniqueID = x.UNIQUEID,
                                     x.VHNO,
                                     x.TYPE,
                                     x.SUBTYPE,
                                     Status = x.STATUS,
                                     CalNo = e.MSACALNO,
                                     OrganizationUniqueID = e.ORGANIZATIONUNIQUEID,
                                     MSAResponsorID = x.MSARESPONSORID,
                                     OwnerID = e.OWNERID,
                                     OwnerManagerID = e.OWNERMANAGERID,
                                     PEID = e.PEID,
                                     PEManagerID = e.PEMANAGERID,
                                     EstMSADate = x.ESTMSADATE,
                                     MSADate = x.MSADATE,
                                     CreateTime = x.CREATETIME,
                                     Station = x.STATION,
                                     MSAIchi = x.MSAICHI,
                                     Characteristic = x.CHARACTERISITIC,
                                     LowerRange = x.LOWERRANGE,
                                     UpperRange = x.UPPERRANGE,
                                     Model = e.MODEL,
                                     Brand = e.BRAND,
                                     IchiRemark = e.ICHIREMARK,
                                     IchiName = i != null ? i.NAME : ""
                                 }).AsQueryable();

                    if (!qa)
                    {
                        query = query.Where(x => Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID) || x.MSAResponsorID == Account.ID || x.OwnerID == Account.ID || x.OwnerManagerID == Account.ID || x.PEID == Account.ID || x.PEManagerID == Account.ID).AsQueryable();
                    }

                    if (Parameters.EstBeginDate.HasValue)
                    {
                        query = query.Where(x => DateTime.Compare(x.EstMSADate.Value, Parameters.EstBeginDate.Value) >= 0);
                    }

                    if (Parameters.EstEndDate.HasValue)
                    {
                        query = query.Where(x => DateTime.Compare(x.EstMSADate.Value, Parameters.EstEndDate.Value) < 0);
                    }

                    if (Parameters.ActBeginDate.HasValue)
                    {
                        query = query.Where(x =>x.MSADate.HasValue&& DateTime.Compare(x.MSADate.Value, Parameters.ActBeginDate.Value) >= 0);
                    }

                    if (Parameters.ActEndDate.HasValue)
                    {
                        query = query.Where(x => x.MSADate.HasValue && DateTime.Compare(x.MSADate.Value, Parameters.ActEndDate.Value) < 0);
                    }

                    if (!string.IsNullOrEmpty(Parameters.VHNO))
                    {
                        query = query.Where(x => x.VHNO.Contains(Parameters.VHNO));
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

                    var temp = query.OrderByDescending(x => x.VHNO).ToList();

                    foreach (var t in temp)
                    {
                        var msaReponsor = db.ACCOUNT.FirstOrDefault(x => x.ID == t.MSAResponsorID);

                        model.ItemList.Add(new GridItem()
                        {
                            UniqueID = t.UniqueID,
                            VHNO = t.VHNO,
                            Type = t.TYPE,
                            SubType = t.SUBTYPE,
                            Status = new FormStatus(t.Status, t.EstMSADate.Value),
                            Factory = OrganizationDataAccessor.GetFactory(OrganizationList, t.OrganizationUniqueID),
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(t.OrganizationUniqueID),
                            CalNo = t.CalNo,
                            MSAIchi = t.MSAIchi,
                            MSACharacteristic = t.Characteristic,
                            MSALowerRange = t.LowerRange,
                            MSAUpperRange = t.UpperRange,
                            Station = t.Station,
                            EstMSADate = t.EstMSADate.Value,
                            MSADate = t.MSADate,
                            MSAResponsorID = t.MSAResponsorID,
                            MSAResponsorName = msaReponsor != null ? msaReponsor.NAME : string.Empty,
                            CreateTime = t.CreateTime.Value,
                            Account = Account
                        });
                    }

                    if (!string.IsNullOrEmpty(Parameters.Status))
                    {
                        model.ItemList = model.ItemList.Where(x => x.Status.StatusCode == Parameters.Status).ToList();
                    }

                    model.ItemList = model.ItemList.OrderBy(x => x.Seq).ThenBy(x => x.StatusSeq).ThenBy(x => x.EstMSADate).ToList();
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
                var model = new DetailViewModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = db.QA_MSAFORM.First(x => x.UNIQUEID == UniqueID);

                    var msaReponsor = db.ACCOUNT.FirstOrDefault(x => x.ID == query.MSARESPONSORID);

                    model = new DetailViewModel()
                    {
                        UniqueID = query.UNIQUEID,
                        VHNO = query.VHNO,
                        Status = new FormStatus(query.STATUS, query.ESTMSADATE.Value),
                        Type = query.TYPE,
                        SubType = query.SUBTYPE,
                        Unit = query.UNIT,
                        MSAIchi = query.MSAICHI,
                        MSACharacteristic = query.CHARACTERISITIC,
                        MSALowerRange = query.LOWERRANGE,
                        MSAUpperRange = query.UPPERRANGE,
                        Station = query.STATION,
                        EstMSADate = query.ESTMSADATE.Value,
                        MSADate = query.MSADATE,
                        MSAResponsorID = query.MSARESPONSORID,
                        MSAResponsorName = msaReponsor != null ? msaReponsor.NAME : string.Empty,
                        CreateTime = query.CREATETIME.Value,
                        Equipment = EquipmentHelper.Get(OrganizationList, query.EQUIPMENTUNIQUEID),
                        StabilityLowerRange = query.STABILITYLOWERRANGE,
                        StabilityUpperRange = query.STABILITYUPPERRANGE,
                        StabilityPrepareDate = query.STABILITYPREPAREDATE,
                        BLPackageType = query.BLPACKAGETYPE,
                        BLDate = query.BLDATE,
                        BiasReferenceValue = query.BIASREFVAL,
                        GRRHPackageType = query.GRRHPACKAGETYPE,
                        GRRHUSL = query.GRRHUSL,
                        GRRHCL = query.GRRHCL,
                        GRRHLSL = query.GRRHLSL,
                        GRRHTvUsed = query.GRRHTVUSED,
                        GRRMPackageType = query.GRRMPACKAGETYPE,
                        GRRMUSL = query.GRRMUSL,
                        GRRMCL = query.GRRMCL,
                        GRRMLSL = query.GRRMLSL,
                        GRRMTvUsed = query.GRRMTVUSED,
                        GRRLPackageType = query.GRRLPACKAGETYPE,
                        GRRLUSL = query.GRRLUSL,
                        GRRLCL = query.GRRLCL,
                        GRRLLSL = query.GRRLLSL,
                        GRRLTvUsed = query.GRRLTVUSED,
                        CountPackageType = query.COUNTPACKAGETYPE,
                        LogList = (from l in db.QA_CALIBRATIONFORMFLOWLOG
                                   join u in db.ACCOUNT
                                   on l.USERID equals u.ID
                                   where l.FORMUNIQUEID == query.UNIQUEID
                                   select new LogModel
                                   {
                                       Seq = l.SEQ,
                                       FlowSeq = l.FLOWSEQ,
                                       NotifyTime = l.NOTIFYTIME,
                                       VerifyResult = l.VERIFYRESULT,
                                       UserID = l.USERID,
                                       UserName = u.NAME,
                                       VerifyTime = l.VERIFYTIME,
                                       VerifyComment = l.VERIFYCOMMENT
                                   }).OrderBy(x => x.VerifyTime).ThenBy(x => x.Seq).ToList()
                    };

                    if (query.TYPE == "1")
                    {
                        var stabilityList = db.QA_MSAFORMSTABILITY.Where(x => x.FORMUNIQUEID == query.UNIQUEID).ToList();
                        var stabilityValueList = db.QA_MSAFORMSTABILITYVALUE.Where(x => x.FORMUNIQUEID == query.UNIQUEID).ToList();

                        for (int point = 1; point <= 30; point++)
                        {
                            var stabilityItem = new StabilityItem()
                            {
                                Point = point
                            };

                            var stability = stabilityList.FirstOrDefault(x => x.POINT == point);

                            if (stability != null)
                            {
                                stabilityItem.Date = stability.DATE;
                            }

                            for (int data = 1; data <= 5; data++)
                            {
                                var value = stabilityValueList.FirstOrDefault(x => x.POINT == point && x.DATA == data);

                                stabilityItem.ValueList.Add(new StabilityValue()
                                {
                                    Data = data,
                                    Value = value != null ? value.VALUE : default(decimal?)
                                });
                            }

                            model.StabilityList.Add(stabilityItem);
                        }

                        if (!string.IsNullOrEmpty(query.STABILITYRESULT))
                        {
                            model.StabilityResult = new StabilityResult()
                            {
                                Result = query.STABILITYRESULT,
                                Stability = query.STABILITYVAL,
                                Stdev = query.STABILITYSTDEV
                            };
                        }
                        else
                        {
                            model.StabilityResult = null;
                        }

                        var biasValueList = db.QA_MSAFORMBIASVALUE.Where(x => x.FORMUNIQUEID == query.UNIQUEID).ToList();

                        for (int trials = 1; trials <= 15; trials++)
                        {
                            var biasItem = new BiasItem()
                            {
                                Trials = trials
                            };

                            var biasValue = biasValueList.FirstOrDefault(x => x.TRIALS == trials);

                            if (biasValue != null)
                            {
                                biasItem.Value = biasValue.VALUE;
                            }

                            model.BiasList.Add(biasItem);
                        }

                        if (!string.IsNullOrEmpty(query.BIASRESULT))
                        {
                            model.BiasResult = new BiasResult()
                            {
                                SignificantT = Convert.ToDecimal(2.14479),
                                TStatistic = query.BIASTSTATISTIC,
                                Result = query.BIASRESULT
                            };
                        }
                        else
                        {
                            model.BiasResult = null;
                        }

                        var linearityList = db.QA_MSAFORMLINEARITY.Where(x => x.FORMUNIQUEID == query.UNIQUEID).ToList();
                        var linearityValueList = db.QA_MSAFORMLINEARITYVALUE.Where(x => x.FORMUNIQUEID == query.UNIQUEID).ToList();

                        for (int trials = 1; trials <= 5; trials++)
                        {
                            var linearityItem = new LinearityItem()
                            {
                                Trials = trials
                            };

                            var linearity = linearityList.FirstOrDefault(x => x.TRIALS == trials);

                            if (linearity != null)
                            {
                                linearityItem.ReferenceValue = linearity.REFVAL;
                            }

                            for (int data = 1; data <= 12; data++)
                            {
                                var value = linearityValueList.FirstOrDefault(x => x.TRIALS == trials && x.DATA == data);

                                linearityItem.ValueList.Add(new LinearityValue()
                                {
                                    Data = data,
                                    Value = value != null ? value.VALUE : default(decimal?)
                                });
                            }

                            model.LinearityList.Add(linearityItem);
                        }

                        if (!string.IsNullOrEmpty(query.LINEARITYRESULT))
                        {
                            model.LinearityResult = new LinearityResult()
                            {
                                ta = query.LINEARITYTA,
                                tb = query.LINEARITYTB,
                                t58 = Convert.ToDecimal(2.00172),
                                Result = query.LINEARITYRESULT
                            };
                        }
                        else
                        {
                            model.LinearityResult = null;
                        }

                        var levelList = new List<string> { "H", "M", "L" };
                        var appraiserList = new List<string> { "A", "B", "C" };

                        var grrList = db.QA_MSAFORMGRR.Where(x => x.FORMUNIQUEID == query.UNIQUEID).ToList();
                        var grrValueList = db.QA_MSAFORMGRRVALUE.Where(x => x.FORMUNIQUEID == query.UNIQUEID).ToList();

                        foreach (var level in levelList)
                        {
                            var grr = grrList.FirstOrDefault(x => x.LEVEL == level);

                            var grrItem = new GRRItem()
                            {
                                Level = level,
                                Date = grr != null ? grr.DATE : default(DateTime?)
                            };

                            foreach (var appraiser in appraiserList)
                            {
                                var appraiserModel = new GRRAppraiser()
                                {
                                    Appraiser = appraiser,
                                    UserID = grr != null ? (appraiser == "A" ? grr.APPRAISERA : appraiser == "B" ? grr.APPRAISERB : grr.APPRAISERC) : ""
                                };

                                for (int part = 1; part <= 10; part++)
                                {
                                    for (int trials = 1; trials <= 3; trials++)
                                    {
                                        var value = grrValueList.FirstOrDefault(x => x.LEVEL == level && x.APPRAISER == appraiser && x.PART == part && x.TRIALS == trials);

                                        appraiserModel.ValueList.Add(new GRRValue()
                                        {
                                            Part = part,
                                            Trials = trials,
                                            Value = value != null ? value.VALUE : default(decimal?)
                                        });
                                    }
                                }

                                grrItem.AppraiserList.Add(appraiserModel);
                            }

                            model.GRRList.Add(grrItem);
                        }

                        if (!string.IsNullOrEmpty(query.GRRHRESULT))
                        {
                            model.GRRHResult = new GRRResult()
                            {
                                GRR = query.GRRH,
                                ndc = query.GRRHNDC,
                                Result = query.GRRHRESULT
                            };
                        }
                        else
                        {
                            model.GRRHResult = null;
                        }

                        if (!string.IsNullOrEmpty(query.GRRMRESULT))
                        {
                            model.GRRMResult = new GRRResult()
                            {
                                GRR = query.GRRM,
                                ndc = query.GRRMNDC,
                                Result = query.GRRMRESULT
                            };
                        }
                        else
                        {
                            model.GRRMResult = null;
                        }

                        if (!string.IsNullOrEmpty(query.GRRLRESULT))
                        {
                            model.GRRLResult = new GRRResult()
                            {
                                GRR = query.GRRL,
                                ndc = query.GRRLNDC,
                                Result = query.GRRLRESULT
                            };
                        }
                        else
                        {
                            model.GRRLResult = null;
                        }


                        if (!string.IsNullOrEmpty(query.ANOVAHRESULT))
                        {
                            model.AnovaHResult = new AnovaResult()
                            {
                                TV = query.ANOVAHTV,
                                PT = query.ANOVAHPT,
                                NDC = query.ANOVAHNDC,
                                Result = query.ANOVAHRESULT
                            };
                        }
                        else
                        {
                            model.AnovaHResult = null;
                        }

                        if (!string.IsNullOrEmpty(query.ANOVAMRESULT))
                        {
                            model.AnovaMResult = new AnovaResult()
                            {
                                TV = query.ANOVAMTV,
                                PT = query.ANOVAMPT,
                                NDC = query.ANOVAMNDC,
                                Result = query.ANOVAMRESULT
                            };
                        }
                        else
                        {
                            model.AnovaMResult = null;
                        }

                        if (!string.IsNullOrEmpty(query.ANOVALRESULT))
                        {
                            model.AnovaLResult = new AnovaResult()
                            {
                                TV = query.ANOVALTV,
                                PT = query.ANOVALPT,
                                NDC = query.ANOVALNDC,
                                Result = query.ANOVALRESULT
                            };
                        }
                        else
                        {
                            model.AnovaLResult = null;
                        }
                    }

                    if (query.TYPE == "2")
                    {
                        if (!string.IsNullOrEmpty(query.COUNTRESULT))
                        {
                            model.CountResult = new CountResult()
                            {
                                CountAAlarm = query.COUNTAALARM,
                                CountAEffective = query.COUNTAEFFECTIVE,
                                CountAError = query.COUNTAERROR,
                                CountBAlarm = query.COUNTBALARM,
                                CountBEffective = query.COUNTBEFFECTIVE,
                                CountBError = query.COUNTBERROR,
                                CountCAlarm = query.COUNTCALARM,
                                CountCEffective = query.COUNTCEFFECTIVE,
                                CountCError = query.COUNTCERROR,
                                KappaA = query.KAPPAA,
                                KappaB = query.KAPPAB,
                                KappaC = query.KAPPAC,
                                KappaResult = query.KAPPARESULT,
                                Result = query.COUNTRESULT
                            };
                        }
                        else
                        {
                            model.CountResult = null;
                        }

                        var appraiserList = new List<string> { "A", "B", "C" };

                        var countList = db.QA_MSAFORMCOUNT.Where(x => x.FORMUNIQUEID == query.UNIQUEID).ToList();

                        foreach (var appraiser in appraiserList)
                        {
                            var count = countList.FirstOrDefault(x => x.APPRAISER == appraiser);

                            var countItem = new CountItem()
                            {
                                Appraiser = appraiser,
                                UserID = count != null ? count.USERID : ""
                            };

                            var countValueList = db.QA_MSAFORMCOUNTVALUE.Where(x => x.FORMUNIQUEID == query.UNIQUEID && x.APPRAISER == appraiser).ToList();

                            for (int sample = 1; sample <= 50; sample++)
                            {
                                for (int trials = 1; trials <= 3; trials++)
                                {
                                    var countValue = countValueList.FirstOrDefault(x => x.SAMPLE == sample && x.TRIALS == trials);

                                    countItem.ValueList.Add(new CountValue()
                                    {
                                        Sample = sample,
                                        Trials = trials,
                                        Value = countValue != null ? countValue.VALUE : default(int?)
                                    });
                                }
                            }

                            model.CountList.Add(countItem);
                        }

                        var countReferenceList = db.QA_MSAFORMCOUNTREFVAL.Where(x => x.FORMUNIQUEID == query.UNIQUEID).ToList();

                        for (int sample = 1; sample <= 50; sample++)
                        {
                            var countReference = countReferenceList.FirstOrDefault(x => x.SAMPLE == sample);

                            model.CountReferenceValueList.Add(new CountReferenceValue()
                            {
                                Sample = sample,
                                Reference = countReference != null ? countReference.REF : default(int?),
                                ReferenceValue = countReference != null ? countReference.REFVAL : default(int?)
                            });
                        }
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

        public static RequestResult GetEditFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new EditFormModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = db.QA_MSAFORM.First(x => x.UNIQUEID == UniqueID);

                    var msaReponsor = db.ACCOUNT.FirstOrDefault(x => x.ID == query.MSARESPONSORID);

                    model = new EditFormModel()
                    {
                        UniqueID = query.UNIQUEID,
                        //VHNO = query.VHNO,
                        //Status = new FormStatus(query.STATUS, query.ESTMSADATE.Value),
                        Type = query.TYPE,
                        //SubType = query.SUBTYPE,
                        //Unit = query.UNIT,
                        //MSAIchi = query.MSAICHI,
                        //MSACharacteristic = query.CHARACTERISITIC,
                        //MSALowerRange = query.LOWERRANGE,
                        //MSAUpperRange = query.UPPERRANGE,
                        //Station = query.STATION,
                        //EstMSADate = query.ESTMSADATE.Value,
                        //MSAResponsorID = query.MSARESPONSORID,
                        //MSAResponsorName = msaReponsor != null ? msaReponsor.NAME : string.Empty,
                        //CreateTime = query.CREATETIME.Value,
                        //Equipment = EquipmentHelper.Get(query.EQUIPMENTUNIQUEID),
                        FormInput = new FormInput()
                        {
                            StabilityLowerRange = query.STABILITYLOWERRANGE,
                            StabilityUpperRange = query.STABILITYUPPERRANGE,
                            BLPackageType = query.BLPACKAGETYPE,
                            BiasReferenceValue = query.BIASREFVAL,
                            GRRHPackageType = query.GRRHPACKAGETYPE,
                            GRRHUSL = query.GRRHUSL,
                            GRRHCL = query.GRRHCL,
                            GRRHLSL = query.GRRHLSL,
                            GRRHTvUsed = query.GRRHTVUSED,
                            GRRMPackageType = query.GRRMPACKAGETYPE,
                            GRRMUSL = query.GRRMUSL,
                            GRRMCL = query.GRRMCL,
                            GRRMLSL = query.GRRMLSL,
                            GRRMTvUsed = query.GRRMTVUSED,
                            GRRLPackageType = query.GRRLPACKAGETYPE,
                            GRRLUSL = query.GRRLUSL,
                            GRRLCL = query.GRRLCL,
                            GRRLLSL = query.GRRLLSL,
                            GRRLTvUsed = query.GRRLTVUSED,
                            CountPackageType = query.COUNTPACKAGETYPE
                        },
                        //LogList = (from l in db.QA_CALIBRATIONFORMFLOWLOG
                        //           join u in db.ACCOUNT
                        //           on l.USERID equals u.ID
                        //           where l.FORMUNIQUEID == query.UNIQUEID
                        //           select new LogModel
                        //           {
                        //               Seq = l.SEQ,
                        //               FlowSeq = l.FLOWSEQ,
                        //               NotifyTime = l.NOTIFYTIME,
                        //               VerifyResult = l.VERIFYRESULT,
                        //               UserID = l.USERID,
                        //               UserName = u.NAME,
                        //               VerifyTime = l.VERIFYTIME,
                        //               VerifyComment = l.VERIFYCOMMENT
                        //           }).OrderBy(x => x.VerifyTime).ThenBy(x => x.Seq).ToList()
                    };

                    if (query.TYPE == "1")
                    {
                        //var stabilityList = db.QA_MSAFORMSTABILITY.Where(x => x.FORMUNIQUEID == query.UNIQUEID).ToList();
                        //var stabilityValueList = db.QA_MSAFORMSTABILITYVALUE.Where(x => x.FORMUNIQUEID == query.UNIQUEID).ToList();

                        //for (int point = 1; point <= 30; point++)
                        //{
                        //    var stabilityItem = new StabilityItem()
                        //    {
                        //        Point = point
                        //    };

                        //    var stability = stabilityList.FirstOrDefault(x => x.POINT == point);

                        //    if (stability != null)
                        //    {
                        //        stabilityItem.Date = stability.DATE;
                        //    }

                        //    for (int data = 1; data <= 5; data++)
                        //    {
                        //        var value = stabilityValueList.FirstOrDefault(x => x.POINT == point && x.DATA == data);

                        //        stabilityItem.ValueList.Add(new StabilityValue()
                        //        {
                        //            Data = data,
                        //            Value = value != null ? value.VALUE : default(decimal?)
                        //        });
                        //    }

                        //    model.StabilityList.Add(stabilityItem);
                        //}

                        //if (!string.IsNullOrEmpty(query.STABILITYRESULT))
                        //{
                        //    model.StabilityResult = new StabilityResult()
                        //    {
                        //        Result = query.STABILITYRESULT,
                        //        Stability = query.STABILITYVAL,
                        //        Stdev = query.STABILITYSTDEV
                        //    };
                        //}
                        //else
                        //{
                        //    model.StabilityResult = null;
                        //}

                        //var biasValueList = db.QA_MSAFORMBIASVALUE.Where(x => x.FORMUNIQUEID == query.UNIQUEID).ToList();

                        //for (int trials = 1; trials <= 15; trials++)
                        //{
                        //    var biasItem = new BiasItem()
                        //    {
                        //        Trials = trials
                        //    };

                        //    var biasValue = biasValueList.FirstOrDefault(x => x.TRIALS == trials);

                        //    if (biasValue != null)
                        //    {
                        //        biasItem.Value = biasValue.VALUE;
                        //    }

                        //    model.BiasList.Add(biasItem);
                        //}

                        //if (!string.IsNullOrEmpty(query.BIASRESULT))
                        //{
                        //    model.BiasResult = new BiasResult()
                        //    {
                        //        SignificantT = Convert.ToDecimal(2.14479),
                        //        TStatistic = query.BIASTSTATISTIC,
                        //        Result = query.BIASRESULT
                        //    };
                        //}
                        //else
                        //{
                        //    model.BiasResult = null;
                        //}

                        //var linearityList = db.QA_MSAFORMLINEARITY.Where(x => x.FORMUNIQUEID == query.UNIQUEID).ToList();
                        //var linearityValueList = db.QA_MSAFORMLINEARITYVALUE.Where(x => x.FORMUNIQUEID == query.UNIQUEID).ToList();

                        //for (int trials = 1; trials <= 5; trials++)
                        //{
                        //    var linearityItem = new LinearityItem()
                        //    {
                        //        Trials = trials
                        //    };

                        //    var linearity = linearityList.FirstOrDefault(x => x.TRIALS == trials);

                        //    if (linearity != null)
                        //    {
                        //        linearityItem.ReferenceValue = linearity.REFVAL;
                        //    }

                        //    for (int data = 1; data <= 12; data++)
                        //    {
                        //        var value = linearityValueList.FirstOrDefault(x => x.TRIALS == trials && x.DATA == data);

                        //        linearityItem.ValueList.Add(new LinearityValue()
                        //        {
                        //            Data = data,
                        //            Value = value != null ? value.VALUE : default(decimal?)
                        //        });
                        //    }

                        //    model.LinearityList.Add(linearityItem);
                        //}

                        //if (!string.IsNullOrEmpty(query.LINEARITYRESULT))
                        //{
                        //    model.LinearityResult = new LinearityResult()
                        //    {
                        //        ta = query.LINEARITYTA,
                        //        tb = query.LINEARITYTB,
                        //        t58 = Convert.ToDecimal(2.00172),
                        //        Result = query.LINEARITYRESULT
                        //    };
                        //}
                        //else
                        //{
                        //    model.LinearityResult = null;
                        //}

                        //var levelList = new List<string> { "H", "M", "L" };
                        //var appraiserList = new List<string> { "A", "B", "C" };

                        //var grrList = db.QA_MSAFORMGRR.Where(x => x.FORMUNIQUEID == query.UNIQUEID).ToList();
                        //var grrValueList = db.QA_MSAFORMGRRVALUE.Where(x => x.FORMUNIQUEID == query.UNIQUEID).ToList();

                        //foreach (var level in levelList)
                        //{
                        //    var grr = grrList.FirstOrDefault(x => x.LEVEL == level);

                        //    var grrItem = new GRRItem()
                        //    {
                        //        Level = level,
                        //        Date = grr != null ? grr.DATE : default(DateTime?)
                        //    };

                        //    foreach (var appraiser in appraiserList)
                        //    {
                        //        var appraiserModel = new GRRAppraiser()
                        //        {
                        //            Appraiser = appraiser,
                        //            UserID = grr != null ? (appraiser == "A" ? grr.APPRAISERA : appraiser == "B" ? grr.APPRAISERB : grr.APPRAISERC) : ""
                        //        };

                        //        for (int part = 1; part <= 10; part++)
                        //        {
                        //            for (int trials = 1; trials <= 3; trials++)
                        //            {
                        //                var value = grrValueList.FirstOrDefault(x => x.LEVEL == level && x.APPRAISER == appraiser && x.PART == part && x.TRIALS == trials);

                        //                appraiserModel.ValueList.Add(new GRRValue()
                        //                {
                        //                    Part = part,
                        //                    Trials = trials,
                        //                    Value = value != null ? value.VALUE : default(decimal?)
                        //                });
                        //            }
                        //        }

                        //        grrItem.AppraiserList.Add(appraiserModel);
                        //    }

                        //    model.GRRList.Add(grrItem);
                        //}

                        //if (!string.IsNullOrEmpty(query.GRRHRESULT))
                        //{
                        //    model.GRRHResult = new GRRResult()
                        //    {
                        //        GRR = query.GRRH,
                        //        ndc = query.GRRHNDC,
                        //        Result = query.GRRHRESULT
                        //    };
                        //}
                        //else
                        //{
                        //    model.GRRHResult = null;
                        //}

                        //if (!string.IsNullOrEmpty(query.GRRMRESULT))
                        //{
                        //    model.GRRMResult = new GRRResult()
                        //    {
                        //        GRR = query.GRRM,
                        //        ndc = query.GRRMNDC,
                        //        Result = query.GRRMRESULT
                        //    };
                        //}
                        //else
                        //{
                        //    model.GRRMResult = null;
                        //}

                        //if (!string.IsNullOrEmpty(query.GRRLRESULT))
                        //{
                        //    model.GRRLResult = new GRRResult()
                        //    {
                        //        GRR = query.GRRL,
                        //        ndc = query.GRRLNDC,
                        //        Result = query.GRRLRESULT
                        //    };
                        //}
                        //else
                        //{
                        //    model.GRRLResult = null;
                        //}


                        //if (!string.IsNullOrEmpty(query.ANOVAHRESULT))
                        //{
                        //    model.AnovaHResult = new AnovaResult()
                        //    {
                        //        TV = query.ANOVAHTV,
                        //        PT = query.ANOVAHPT,
                        //        NDC = query.ANOVAHNDC,
                        //        Result = query.ANOVAHRESULT
                        //    };
                        //}
                        //else
                        //{
                        //    model.AnovaHResult = null;
                        //}

                        //if (!string.IsNullOrEmpty(query.ANOVAMRESULT))
                        //{
                        //    model.AnovaMResult = new AnovaResult()
                        //    {
                        //        TV = query.ANOVAMTV,
                        //        PT = query.ANOVAMPT,
                        //        NDC = query.ANOVAMNDC,
                        //        Result = query.ANOVAMRESULT
                        //    };
                        //}
                        //else
                        //{
                        //    model.AnovaMResult = null;
                        //}

                        //if (!string.IsNullOrEmpty(query.ANOVALRESULT))
                        //{
                        //    model.AnovaLResult = new AnovaResult()
                        //    {
                        //        TV = query.ANOVALTV,
                        //        PT = query.ANOVALPT,
                        //        NDC = query.ANOVALNDC,
                        //        Result = query.ANOVALRESULT
                        //    };
                        //}
                        //else
                        //{
                        //    model.AnovaLResult = null;
                        //}
                    }

                    if (query.TYPE == "2")
                    {
                        var appraiserList = new List<string> { "A", "B", "C" };

                        //var countList = db.QA_MSAFORMCOUNT.Where(x => x.FORMUNIQUEID == query.UNIQUEID).ToList();

                        //foreach (var appraiser in appraiserList)
                        //{
                        //    var count = countList.FirstOrDefault(x => x.APPRAISER == appraiser);

                        //    var countItem = new CountItem()
                        //    {
                        //        Appraiser = appraiser,
                        //        UserID = count != null ? count.USERID : ""
                        //    };

                        //    var countValueList = db.QA_MSAFORMCOUNTVALUE.Where(x => x.FORMUNIQUEID == query.UNIQUEID && x.APPRAISER == appraiser).ToList();

                        //    for (int sample = 1; sample <= 50; sample++)
                        //    {
                        //        for (int trials = 1; trials <= 3; trials++)
                        //        {
                        //            var countValue = countValueList.FirstOrDefault(x => x.SAMPLE == sample && x.TRIALS == trials);

                        //            countItem.ValueList.Add(new CountValue()
                        //            {
                        //                Sample = sample,
                        //                Trials = trials,
                        //                Value = countValue != null ? countValue.VALUE : default(int?)
                        //            });
                        //        }
                        //    }

                        //    model.CountList.Add(countItem);
                        //}

                        //var countReferenceList = db.QA_MSAFORMCOUNTREFVAL.Where(x => x.FORMUNIQUEID == query.UNIQUEID).ToList();

                        //for (int sample = 1; sample <= 50; sample++)
                        //{
                        //    var countReference = countReferenceList.FirstOrDefault(x => x.SAMPLE == sample);

                        //    model.CountReferenceValueList.Add(new CountReferenceValue()
                        //    {
                        //        Sample = sample,
                        //        Reference = countReference != null ? countReference.REF : default(int?),
                        //        ReferenceValue = countReference != null ? countReference.REFVAL : default(int?)
                        //    });
                        //}

                        //if (!string.IsNullOrEmpty(query.KAPPARESULT) && !string.IsNullOrEmpty(query.COUNTRESULT))
                        //{
                        //    model.CountResult = new CountResult()
                        //    {
                        //        KappaA = query.KAPPAA,
                        //        KappaB = query.KAPPAB,
                        //        KappaC = query.KAPPAC,
                        //        KappaResult = query.KAPPARESULT,
                        //        CountAEffective = query.COUNTAEFFECTIVE,
                        //        CountAError = query.COUNTAERROR,
                        //        CountAAlarm = query.COUNTAALARM,
                        //        CountBEffective = query.COUNTBEFFECTIVE,
                        //        CountBError = query.COUNTBERROR,
                        //        CountBAlarm = query.COUNTBALARM,
                        //        CountCEffective = query.COUNTCEFFECTIVE,
                        //        CountCError = query.COUNTCERROR,
                        //        CountCAlarm = query.COUNTCALARM,
                        //        Result = query.COUNTRESULT
                        //    };
                        //}
                        //else
                        //{
                        //    model.CountResult = null;
                        //}

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

        public static RequestResult Import(List<Models.Shared.Organization> OrganizationList, string UniqueID, string TempFileName)
        {
            RequestResult result = new RequestResult();

            try
            {
                IWorkbook workBook;

                using (FileStream file = new FileStream(TempFileName, FileMode.Open, FileAccess.Read))
                {
                    workBook = new HSSFWorkbook(file);
                    file.Close();
                }

                var type = string.Empty;
                var subType = string.Empty;

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var msaForm = db.QA_MSAFORM.First(x => x.UNIQUEID == UniqueID);

                    type = msaForm.TYPE;
                    subType = msaForm.SUBTYPE;
                }

                if (type == "1")
                {
                    //全距平均
                    if (subType == "1")
                    {
                        var uniqueID = string.Empty;

                        try
                        {
                            var sheet = workBook.GetSheetAt(9);

                            var row = sheet.GetRow(0);
                            var cell = row.GetCell(0);

                            uniqueID = ExcelHelper.GetCellValue(cell);
                        }
                        catch
                        {
                            uniqueID = string.Empty;
                        }

                        if (uniqueID != UniqueID)
                        {
                            result.ReturnFailedMessage("請使用系統下載之指定檔案進行資料輸入及上傳");
                        }
                        else
                        {
                            result = GetEditFormModel(UniqueID);

                            if (result.IsSuccess)
                            {
                                var model = result.Data as EditFormModel;

                                #region 封面
                                var sheet = workBook.GetSheetAt(0);

                                var row = sheet.GetRow(12);
                                var cellValue = ExcelHelper.GetCellValue(row.GetCell(5));

                                model.FormInput.MSADate = !string.IsNullOrEmpty(cellValue) ? DateTime.Parse(cellValue) : default(DateTime?);
                                #endregion

                                #region Stability
                                sheet = workBook.GetSheetAt(1);

                                row = sheet.GetRow(6);
                                model.FormInput.StabilityLowerRange = ExcelHelper.GetCellValue(row.GetCell(17));
                                model.FormInput.StabilityUpperRange = ExcelHelper.GetCellValue(row.GetCell(20));
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(23));
                                model.FormInput.StabilityPrepareDate = !string.IsNullOrEmpty(cellValue) ? DateTime.Parse(cellValue) : default(DateTime?);

                                int point = 1;
                                row = sheet.GetRow(29);
                                for (int cellIndex = 2; cellIndex <= 31; cellIndex++)
                                {
                                    cellValue = ExcelHelper.GetCellValue(row.GetCell(cellIndex));

                                    if (!string.IsNullOrEmpty(cellValue))
                                    {
                                        cellValue = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Parse(cellValue));
                                    }

                                    model.FormInput.StabilityDateList.Add(string.Format("{0}{1}{2}", point, Define.Seperator, cellValue));

                                    point++;
                                }

                                int data = 1;
                                for (int rowIndex = 22; rowIndex <= 26; rowIndex++)
                                {
                                    row = sheet.GetRow(rowIndex);
                                    point = 1;
                                    for (int cellIndex = 2; cellIndex <= 31; cellIndex++)
                                    {
                                        model.FormInput.StabilityList.Add(string.Format("{0}{1}{2}{3}{4}", point, Define.Seperator, data, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(cellIndex))));

                                        point++;
                                    }

                                    data++;
                                }
                                #endregion

                                #region BL
                                sheet = workBook.GetSheetAt(2);
                                row = sheet.GetRow(3);
                                model.FormInput.BLPackageType = ExcelHelper.GetCellValue(row.GetCell(12));
                                row = sheet.GetRow(0);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(18));
                                model.FormInput.BLDate = !string.IsNullOrEmpty(cellValue) ? DateTime.Parse(cellValue) : default(DateTime?);
                                row = sheet.GetRow(7);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(3));
                                model.FormInput.BiasReferenceValue = !string.IsNullOrEmpty(cellValue) ? decimal.Parse(cellValue) : default(decimal?);

                                int trials = 1;
                                for (int rowIndex = 9; rowIndex <= 23; rowIndex++)
                                {
                                    row = sheet.GetRow(rowIndex);
                                    model.FormInput.BiasList.Add(string.Format("{0}{1}{2}", trials, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(2))));
                                    trials++;
                                }

                                row = sheet.GetRow(7);
                                trials = 1;
                                for (int cellIndex = 10; cellIndex <= 14; cellIndex++)
                                {
                                    model.FormInput.LinearityReferenceValueList.Add(string.Format("{0}{1}{2}", trials, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(cellIndex))));

                                    trials++;
                                }

                                data = 1;
                                for (int rowIndex = 9; rowIndex <= 20; rowIndex++)
                                {
                                    row = sheet.GetRow(rowIndex);
                                    trials = 1;

                                    for (int cellIndex = 10; cellIndex <= 14; cellIndex++)
                                    {
                                        model.FormInput.LinearityList.Add(string.Format("{0}{1}{2}{3}{4}", trials, Define.Seperator, data, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(cellIndex))));

                                        trials++;
                                    }

                                    data++;
                                }
                                #endregion

                                #region GRRH
                                sheet = workBook.GetSheetAt(3);
                                row = sheet.GetRow(2);
                                model.FormInput.GRRHPackageType = ExcelHelper.GetCellValue(row.GetCell(3));
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(10));
                                model.FormInput.GRRHUSL = !string.IsNullOrEmpty(cellValue) ? decimal.Parse(cellValue) : default(decimal?);
                                row = sheet.GetRow(3);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(10));
                                model.FormInput.GRRHCL = !string.IsNullOrEmpty(cellValue) ? decimal.Parse(cellValue) : default(decimal?);
                                row = sheet.GetRow(4);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(10));
                                model.FormInput.GRRHLSL = !string.IsNullOrEmpty(cellValue) ? decimal.Parse(cellValue) : default(decimal?);
                                row = sheet.GetRow(1);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(11));
                                if (cellValue == "v" || cellValue == "V")
                                {
                                    model.FormInput.GRRHTvUsed = "1";
                                }
                                row = sheet.GetRow(2);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(11));
                                if (cellValue == "v" || cellValue == "V")
                                {
                                    model.FormInput.GRRHTvUsed = "2";
                                }
                                row = sheet.GetRow(3);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(11));
                                if (cellValue == "v" || cellValue == "V")
                                {
                                    model.FormInput.GRRHTvUsed = "3";
                                }
                                row = sheet.GetRow(4);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(11));
                                if (cellValue == "v" || cellValue == "V")
                                {
                                    model.FormInput.GRRHTvUsed = "4";
                                }

                                row = sheet.GetRow(0);
                                var date = ExcelHelper.GetCellValue(row.GetCell(10));
                                if (!string.IsNullOrEmpty(date))
                                {
                                    date = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Parse(date));
                                }
                                row = sheet.GetRow(9);
                                model.FormInput.GRRAppraiserList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}", "H", Define.Seperator, "A", Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(0)), Define.Seperator, date));

                                row = sheet.GetRow(14);
                                model.FormInput.GRRAppraiserList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}", "H", Define.Seperator, "B", Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(0)), Define.Seperator, date));

                                row = sheet.GetRow(19);
                                model.FormInput.GRRAppraiserList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}", "H", Define.Seperator, "C", Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(0)), Define.Seperator, date));

                                trials = 1;
                                for (int rowIndex = 8; rowIndex <= 10; rowIndex++)
                                {
                                    row = sheet.GetRow(rowIndex);
                                    int part = 1;
                                    for (int cellIndex = 1; cellIndex <= 10; cellIndex++)
                                    {
                                        model.FormInput.GRRList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "H", Define.Seperator, "A", Define.Seperator, part, Define.Seperator, trials, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(cellIndex))));

                                        part++;
                                    }
                                    trials++;
                                }

                                trials = 1;
                                for (int rowIndex = 13; rowIndex <= 15; rowIndex++)
                                {
                                    row = sheet.GetRow(rowIndex);
                                    int part = 1;
                                    for (int cellIndex = 1; cellIndex <= 10; cellIndex++)
                                    {
                                        model.FormInput.GRRList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "H", Define.Seperator, "B", Define.Seperator, part, Define.Seperator, trials, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(cellIndex))));

                                        part++;
                                    }
                                    trials++;
                                }

                                trials = 1;
                                for (int rowIndex = 18; rowIndex <= 20; rowIndex++)
                                {
                                    row = sheet.GetRow(rowIndex);
                                    int part = 1;
                                    for (int cellIndex = 1; cellIndex <= 10; cellIndex++)
                                    {
                                        model.FormInput.GRRList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "H", Define.Seperator, "C", Define.Seperator, part, Define.Seperator, trials, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(cellIndex))));

                                        part++;
                                    }
                                    trials++;
                                }
                                #endregion

                                #region GRRM
                                sheet = workBook.GetSheetAt(5);
                                row = sheet.GetRow(2);
                                model.FormInput.GRRMPackageType = ExcelHelper.GetCellValue(row.GetCell(3));
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(10));
                                model.FormInput.GRRMUSL = !string.IsNullOrEmpty(cellValue) ? decimal.Parse(cellValue) : default(decimal?);
                                row = sheet.GetRow(3);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(10));
                                model.FormInput.GRRMCL = !string.IsNullOrEmpty(cellValue) ? decimal.Parse(cellValue) : default(decimal?);
                                row = sheet.GetRow(4);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(10));
                                model.FormInput.GRRMLSL = !string.IsNullOrEmpty(cellValue) ? decimal.Parse(cellValue) : default(decimal?);
                                row = sheet.GetRow(1);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(11));
                                if (cellValue == "v" || cellValue == "V")
                                {
                                    model.FormInput.GRRMTvUsed = "1";
                                }
                                row = sheet.GetRow(2);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(11));
                                if (cellValue == "v" || cellValue == "V")
                                {
                                    model.FormInput.GRRMTvUsed = "2";
                                }
                                row = sheet.GetRow(3);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(11));
                                if (cellValue == "v" || cellValue == "V")
                                {
                                    model.FormInput.GRRMTvUsed = "3";
                                }
                                row = sheet.GetRow(4);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(11));
                                if (cellValue == "v" || cellValue == "V")
                                {
                                    model.FormInput.GRRMTvUsed = "4";
                                }

                                row = sheet.GetRow(0);
                                date = ExcelHelper.GetCellValue(row.GetCell(10));
                                if (!string.IsNullOrEmpty(date))
                                {
                                    date = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Parse(date));
                                }
                                row = sheet.GetRow(9);
                                model.FormInput.GRRAppraiserList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}", "M", Define.Seperator, "A", Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(0)), Define.Seperator, date));

                                row = sheet.GetRow(14);
                                model.FormInput.GRRAppraiserList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}", "M", Define.Seperator, "B", Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(0)), Define.Seperator, date));

                                row = sheet.GetRow(19);
                                model.FormInput.GRRAppraiserList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}", "M", Define.Seperator, "C", Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(0)), Define.Seperator, date));

                                trials = 1;
                                for (int rowIndex = 8; rowIndex <= 10; rowIndex++)
                                {
                                    row = sheet.GetRow(rowIndex);
                                    int part = 1;
                                    for (int cellIndex = 1; cellIndex <= 10; cellIndex++)
                                    {
                                        model.FormInput.GRRList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "M", Define.Seperator, "A", Define.Seperator, part, Define.Seperator, trials, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(cellIndex))));

                                        part++;
                                    }
                                    trials++;
                                }

                                trials = 1;
                                for (int rowIndex = 13; rowIndex <= 15; rowIndex++)
                                {
                                    row = sheet.GetRow(rowIndex);
                                    int part = 1;
                                    for (int cellIndex = 1; cellIndex <= 10; cellIndex++)
                                    {
                                        model.FormInput.GRRList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "M", Define.Seperator, "B", Define.Seperator, part, Define.Seperator, trials, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(cellIndex))));

                                        part++;
                                    }
                                    trials++;
                                }

                                trials = 1;
                                for (int rowIndex = 18; rowIndex <= 20; rowIndex++)
                                {
                                    row = sheet.GetRow(rowIndex);
                                    int part = 1;
                                    for (int cellIndex = 1; cellIndex <= 10; cellIndex++)
                                    {
                                        model.FormInput.GRRList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "M", Define.Seperator, "C", Define.Seperator, part, Define.Seperator, trials, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(cellIndex))));

                                        part++;
                                    }
                                    trials++;
                                }
                                #endregion

                                #region GRRL
                                sheet = workBook.GetSheetAt(7);
                                row = sheet.GetRow(2);
                                model.FormInput.GRRLPackageType = ExcelHelper.GetCellValue(row.GetCell(3));
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(10));
                                model.FormInput.GRRLUSL = !string.IsNullOrEmpty(cellValue) ? decimal.Parse(cellValue) : default(decimal?);
                                row = sheet.GetRow(3);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(10));
                                model.FormInput.GRRLCL = !string.IsNullOrEmpty(cellValue) ? decimal.Parse(cellValue) : default(decimal?);
                                row = sheet.GetRow(4);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(10));
                                model.FormInput.GRRLLSL = !string.IsNullOrEmpty(cellValue) ? decimal.Parse(cellValue) : default(decimal?);
                                row = sheet.GetRow(1);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(11));
                                if (cellValue == "v" || cellValue == "V")
                                {
                                    model.FormInput.GRRLTvUsed = "1";
                                }
                                row = sheet.GetRow(2);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(11));
                                if (cellValue == "v" || cellValue == "V")
                                {
                                    model.FormInput.GRRLTvUsed = "2";
                                }
                                row = sheet.GetRow(3);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(11));
                                if (cellValue == "v" || cellValue == "V")
                                {
                                    model.FormInput.GRRLTvUsed = "3";
                                }
                                row = sheet.GetRow(4);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(11));
                                if (cellValue == "v" || cellValue == "V")
                                {
                                    model.FormInput.GRRLTvUsed = "4";
                                }

                                row = sheet.GetRow(0);
                                date = ExcelHelper.GetCellValue(row.GetCell(10));
                                if (!string.IsNullOrEmpty(date))
                                {
                                    date = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Parse(date));
                                }
                                row = sheet.GetRow(9);
                                model.FormInput.GRRAppraiserList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}", "L", Define.Seperator, "A", Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(0)), Define.Seperator, date));

                                row = sheet.GetRow(14);
                                model.FormInput.GRRAppraiserList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}", "L", Define.Seperator, "B", Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(0)), Define.Seperator, date));

                                row = sheet.GetRow(19);
                                model.FormInput.GRRAppraiserList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}", "L", Define.Seperator, "C", Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(0)), Define.Seperator, date));

                                trials = 1;
                                for (int rowIndex = 8; rowIndex <= 10; rowIndex++)
                                {
                                    row = sheet.GetRow(rowIndex);
                                    int part = 1;
                                    for (int cellIndex = 1; cellIndex <= 10; cellIndex++)
                                    {
                                        model.FormInput.GRRList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "L", Define.Seperator, "A", Define.Seperator, part, Define.Seperator, trials, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(cellIndex))));

                                        part++;
                                    }
                                    trials++;
                                }

                                trials = 1;
                                for (int rowIndex = 13; rowIndex <= 15; rowIndex++)
                                {
                                    row = sheet.GetRow(rowIndex);
                                    int part = 1;
                                    for (int cellIndex = 1; cellIndex <= 10; cellIndex++)
                                    {
                                        model.FormInput.GRRList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "L", Define.Seperator, "B", Define.Seperator, part, Define.Seperator, trials, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(cellIndex))));

                                        part++;
                                    }
                                    trials++;
                                }

                                trials = 1;
                                for (int rowIndex = 18; rowIndex <= 20; rowIndex++)
                                {
                                    row = sheet.GetRow(rowIndex);
                                    int part = 1;
                                    for (int cellIndex = 1; cellIndex <= 10; cellIndex++)
                                    {
                                        model.FormInput.GRRList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "L", Define.Seperator, "C", Define.Seperator, part, Define.Seperator, trials, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(cellIndex))));

                                        part++;
                                    }
                                    trials++;
                                }
                                #endregion

                                result = Edit(OrganizationList, model);
                            }
                        }
                    }
                    //Anova
                    else if (subType == "2")
                    {
                        var uniqueID = string.Empty;

                        try
                        {
                            var sheet = workBook.GetSheetAt(12);

                            var row = sheet.GetRow(0);
                            var cell = row.GetCell(0);

                            uniqueID = ExcelHelper.GetCellValue(cell);
                        }
                        catch
                        {
                            uniqueID = string.Empty;
                        }

                        if (uniqueID != UniqueID)
                        {
                            result.ReturnFailedMessage("請使用系統下載之指定檔案進行資料輸入及上傳");
                        }
                        else
                        {
                            result = GetEditFormModel(UniqueID);

                            if (result.IsSuccess)
                            {
                                var model = result.Data as EditFormModel;

                                #region 封面
                                var sheet = workBook.GetSheetAt(0);

                                var row = sheet.GetRow(12);
                                var cellValue = ExcelHelper.GetCellValue(row.GetCell(5));

                                model.FormInput.MSADate = !string.IsNullOrEmpty(cellValue) ? DateTime.Parse(cellValue) : default(DateTime?);
                                #endregion

                                #region Stability
                                sheet = workBook.GetSheetAt(1);

                                row = sheet.GetRow(6);
                                model.FormInput.StabilityLowerRange = ExcelHelper.GetCellValue(row.GetCell(17));
                                model.FormInput.StabilityUpperRange = ExcelHelper.GetCellValue(row.GetCell(20));
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(23));
                                model.FormInput.StabilityPrepareDate = !string.IsNullOrEmpty(cellValue) ? DateTime.Parse(cellValue) : default(DateTime?);

                                int point = 1;
                                row = sheet.GetRow(29);
                                for (int cellIndex = 2; cellIndex <= 31; cellIndex++)
                                {
                                    cellValue = ExcelHelper.GetCellValue(row.GetCell(cellIndex));

                                    if (!string.IsNullOrEmpty(cellValue))
                                    {
                                        cellValue = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Parse(cellValue));
                                    }

                                    model.FormInput.StabilityDateList.Add(string.Format("{0}{1}{2}", point, Define.Seperator, cellValue));

                                    point++;
                                }

                                int data = 1;
                                for (int rowIndex = 22; rowIndex <= 26; rowIndex++)
                                {
                                    row = sheet.GetRow(rowIndex);
                                    point = 1;
                                    for (int cellIndex = 2; cellIndex <= 31; cellIndex++)
                                    {
                                        model.FormInput.StabilityList.Add(string.Format("{0}{1}{2}{3}{4}", point, Define.Seperator, data, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(cellIndex))));

                                        point++;
                                    }

                                    data++;
                                }
                                #endregion

                                #region BL
                                sheet = workBook.GetSheetAt(2);
                                row = sheet.GetRow(3);
                                model.FormInput.BLPackageType = ExcelHelper.GetCellValue(row.GetCell(12));
                                row = sheet.GetRow(0);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(18));
                                model.FormInput.BLDate = !string.IsNullOrEmpty(cellValue) ? DateTime.Parse(cellValue) : default(DateTime?);
                                row = sheet.GetRow(7);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(3));
                                model.FormInput.BiasReferenceValue = !string.IsNullOrEmpty(cellValue) ? decimal.Parse(cellValue) : default(decimal?);

                                int trials = 1;
                                for (int rowIndex = 9; rowIndex <= 23; rowIndex++)
                                {
                                    row = sheet.GetRow(rowIndex);
                                    model.FormInput.BiasList.Add(string.Format("{0}{1}{2}", trials, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(2))));
                                    trials++;
                                }

                                row = sheet.GetRow(7);
                                trials = 1;
                                for (int cellIndex = 10; cellIndex <= 14; cellIndex++)
                                {
                                    model.FormInput.LinearityReferenceValueList.Add(string.Format("{0}{1}{2}", trials, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(cellIndex))));

                                    trials++;
                                }

                                data = 1;
                                for (int rowIndex = 9; rowIndex <= 20; rowIndex++)
                                {
                                    row = sheet.GetRow(rowIndex);
                                    trials = 1;

                                    for (int cellIndex = 10; cellIndex <= 14; cellIndex++)
                                    {
                                        model.FormInput.LinearityList.Add(string.Format("{0}{1}{2}{3}{4}", trials, Define.Seperator, data, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(cellIndex))));

                                        trials++;
                                    }

                                    data++;
                                }
                                #endregion

                                #region GRRH
                                sheet = workBook.GetSheetAt(3);
                                row = sheet.GetRow(2);
                                model.FormInput.GRRHPackageType = ExcelHelper.GetCellValue(row.GetCell(3));
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(10));
                                model.FormInput.GRRHUSL = !string.IsNullOrEmpty(cellValue) ? decimal.Parse(cellValue) : default(decimal?);
                                row = sheet.GetRow(3);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(10));
                                model.FormInput.GRRHCL = !string.IsNullOrEmpty(cellValue) ? decimal.Parse(cellValue) : default(decimal?);
                                row = sheet.GetRow(4);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(10));
                                model.FormInput.GRRHLSL = !string.IsNullOrEmpty(cellValue) ? decimal.Parse(cellValue) : default(decimal?);
                                row = sheet.GetRow(1);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(11));
                                if (cellValue == "v" || cellValue == "V")
                                {
                                    model.FormInput.GRRHTvUsed = "1";
                                }
                                row = sheet.GetRow(2);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(11));
                                if (cellValue == "v" || cellValue == "V")
                                {
                                    model.FormInput.GRRHTvUsed = "2";
                                }
                                row = sheet.GetRow(3);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(11));
                                if (cellValue == "v" || cellValue == "V")
                                {
                                    model.FormInput.GRRHTvUsed = "3";
                                }
                                row = sheet.GetRow(4);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(11));
                                if (cellValue == "v" || cellValue == "V")
                                {
                                    model.FormInput.GRRHTvUsed = "4";
                                }

                                row = sheet.GetRow(0);
                                var date = ExcelHelper.GetCellValue(row.GetCell(10));
                                if (!string.IsNullOrEmpty(date))
                                {
                                    date = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Parse(date));
                                }
                                row = sheet.GetRow(9);
                                model.FormInput.GRRAppraiserList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}", "H", Define.Seperator, "A", Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(0)), Define.Seperator, date));

                                row = sheet.GetRow(14);
                                model.FormInput.GRRAppraiserList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}", "H", Define.Seperator, "B", Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(0)), Define.Seperator, date));

                                row = sheet.GetRow(19);
                                model.FormInput.GRRAppraiserList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}", "H", Define.Seperator, "C", Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(0)), Define.Seperator, date));

                                trials = 1;
                                for (int rowIndex = 8; rowIndex <= 10; rowIndex++)
                                {
                                    row = sheet.GetRow(rowIndex);
                                    int part = 1;
                                    for (int cellIndex = 1; cellIndex <= 10; cellIndex++)
                                    {
                                        model.FormInput.GRRList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "H", Define.Seperator, "A", Define.Seperator, part, Define.Seperator, trials, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(cellIndex))));

                                        part++;
                                    }
                                    trials++;
                                }

                                trials = 1;
                                for (int rowIndex = 13; rowIndex <= 15; rowIndex++)
                                {
                                    row = sheet.GetRow(rowIndex);
                                    int part = 1;
                                    for (int cellIndex = 1; cellIndex <= 10; cellIndex++)
                                    {
                                        model.FormInput.GRRList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "H", Define.Seperator, "B", Define.Seperator, part, Define.Seperator, trials, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(cellIndex))));

                                        part++;
                                    }
                                    trials++;
                                }

                                trials = 1;
                                for (int rowIndex = 18; rowIndex <= 20; rowIndex++)
                                {
                                    row = sheet.GetRow(rowIndex);
                                    int part = 1;
                                    for (int cellIndex = 1; cellIndex <= 10; cellIndex++)
                                    {
                                        model.FormInput.GRRList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "H", Define.Seperator, "C", Define.Seperator, part, Define.Seperator, trials, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(cellIndex))));

                                        part++;
                                    }
                                    trials++;
                                }
                                #endregion

                                #region GRRM
                                sheet = workBook.GetSheetAt(6);
                                row = sheet.GetRow(2);
                                model.FormInput.GRRMPackageType = ExcelHelper.GetCellValue(row.GetCell(3));
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(10));
                                model.FormInput.GRRMUSL = !string.IsNullOrEmpty(cellValue) ? decimal.Parse(cellValue) : default(decimal?);
                                row = sheet.GetRow(3);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(10));
                                model.FormInput.GRRMCL = !string.IsNullOrEmpty(cellValue) ? decimal.Parse(cellValue) : default(decimal?);
                                row = sheet.GetRow(4);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(10));
                                model.FormInput.GRRMLSL = !string.IsNullOrEmpty(cellValue) ? decimal.Parse(cellValue) : default(decimal?);
                                row = sheet.GetRow(1);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(11));
                                if (cellValue == "v" || cellValue == "V")
                                {
                                    model.FormInput.GRRMTvUsed = "1";
                                }
                                row = sheet.GetRow(2);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(11));
                                if (cellValue == "v" || cellValue == "V")
                                {
                                    model.FormInput.GRRMTvUsed = "2";
                                }
                                row = sheet.GetRow(3);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(11));
                                if (cellValue == "v" || cellValue == "V")
                                {
                                    model.FormInput.GRRMTvUsed = "3";
                                }
                                row = sheet.GetRow(4);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(11));
                                if (cellValue == "v" || cellValue == "V")
                                {
                                    model.FormInput.GRRMTvUsed = "4";
                                }

                                row = sheet.GetRow(0);
                                date = ExcelHelper.GetCellValue(row.GetCell(10));
                                if (!string.IsNullOrEmpty(date))
                                {
                                    date = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Parse(date));
                                }
                                row = sheet.GetRow(9);
                                model.FormInput.GRRAppraiserList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}", "M", Define.Seperator, "A", Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(0)), Define.Seperator, date));

                                row = sheet.GetRow(14);
                                model.FormInput.GRRAppraiserList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}", "M", Define.Seperator, "B", Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(0)), Define.Seperator, date));

                                row = sheet.GetRow(19);
                                model.FormInput.GRRAppraiserList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}", "M", Define.Seperator, "C", Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(0)), Define.Seperator, date));

                                trials = 1;
                                for (int rowIndex = 8; rowIndex <= 10; rowIndex++)
                                {
                                    row = sheet.GetRow(rowIndex);
                                    int part = 1;
                                    for (int cellIndex = 1; cellIndex <= 10; cellIndex++)
                                    {
                                        model.FormInput.GRRList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "M", Define.Seperator, "A", Define.Seperator, part, Define.Seperator, trials, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(cellIndex))));

                                        part++;
                                    }
                                    trials++;
                                }

                                trials = 1;
                                for (int rowIndex = 13; rowIndex <= 15; rowIndex++)
                                {
                                    row = sheet.GetRow(rowIndex);
                                    int part = 1;
                                    for (int cellIndex = 1; cellIndex <= 10; cellIndex++)
                                    {
                                        model.FormInput.GRRList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "M", Define.Seperator, "B", Define.Seperator, part, Define.Seperator, trials, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(cellIndex))));

                                        part++;
                                    }
                                    trials++;
                                }

                                trials = 1;
                                for (int rowIndex = 18; rowIndex <= 20; rowIndex++)
                                {
                                    row = sheet.GetRow(rowIndex);
                                    int part = 1;
                                    for (int cellIndex = 1; cellIndex <= 10; cellIndex++)
                                    {
                                        model.FormInput.GRRList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "M", Define.Seperator, "C", Define.Seperator, part, Define.Seperator, trials, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(cellIndex))));

                                        part++;
                                    }
                                    trials++;
                                }
                                #endregion

                                #region GRRL
                                sheet = workBook.GetSheetAt(9);
                                row = sheet.GetRow(2);
                                model.FormInput.GRRLPackageType = ExcelHelper.GetCellValue(row.GetCell(3));
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(10));
                                model.FormInput.GRRLUSL = !string.IsNullOrEmpty(cellValue) ? decimal.Parse(cellValue) : default(decimal?);
                                row = sheet.GetRow(3);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(10));
                                model.FormInput.GRRLCL = !string.IsNullOrEmpty(cellValue) ? decimal.Parse(cellValue) : default(decimal?);
                                row = sheet.GetRow(4);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(10));
                                model.FormInput.GRRLLSL = !string.IsNullOrEmpty(cellValue) ? decimal.Parse(cellValue) : default(decimal?);
                                row = sheet.GetRow(1);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(11));
                                if (cellValue == "v" || cellValue == "V")
                                {
                                    model.FormInput.GRRLTvUsed = "1";
                                }
                                row = sheet.GetRow(2);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(11));
                                if (cellValue == "v" || cellValue == "V")
                                {
                                    model.FormInput.GRRLTvUsed = "2";
                                }
                                row = sheet.GetRow(3);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(11));
                                if (cellValue == "v" || cellValue == "V")
                                {
                                    model.FormInput.GRRLTvUsed = "3";
                                }
                                row = sheet.GetRow(4);
                                cellValue = ExcelHelper.GetCellValue(row.GetCell(11));
                                if (cellValue == "v" || cellValue == "V")
                                {
                                    model.FormInput.GRRLTvUsed = "4";
                                }

                                row = sheet.GetRow(0);
                                date = ExcelHelper.GetCellValue(row.GetCell(10));
                                if (!string.IsNullOrEmpty(date))
                                {
                                    date = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Parse(date));
                                }
                                row = sheet.GetRow(9);
                                model.FormInput.GRRAppraiserList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}", "L", Define.Seperator, "A", Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(0)), Define.Seperator, date));

                                row = sheet.GetRow(14);
                                model.FormInput.GRRAppraiserList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}", "L", Define.Seperator, "B", Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(0)), Define.Seperator, date));

                                row = sheet.GetRow(19);
                                model.FormInput.GRRAppraiserList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}", "L", Define.Seperator, "C", Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(0)), Define.Seperator, date));

                                trials = 1;
                                for (int rowIndex = 8; rowIndex <= 10; rowIndex++)
                                {
                                    row = sheet.GetRow(rowIndex);
                                    int part = 1;
                                    for (int cellIndex = 1; cellIndex <= 10; cellIndex++)
                                    {
                                        model.FormInput.GRRList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "L", Define.Seperator, "A", Define.Seperator, part, Define.Seperator, trials, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(cellIndex))));

                                        part++;
                                    }
                                    trials++;
                                }

                                trials = 1;
                                for (int rowIndex = 13; rowIndex <= 15; rowIndex++)
                                {
                                    row = sheet.GetRow(rowIndex);
                                    int part = 1;
                                    for (int cellIndex = 1; cellIndex <= 10; cellIndex++)
                                    {
                                        model.FormInput.GRRList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "L", Define.Seperator, "B", Define.Seperator, part, Define.Seperator, trials, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(cellIndex))));

                                        part++;
                                    }
                                    trials++;
                                }

                                trials = 1;
                                for (int rowIndex = 18; rowIndex <= 20; rowIndex++)
                                {
                                    row = sheet.GetRow(rowIndex);
                                    int part = 1;
                                    for (int cellIndex = 1; cellIndex <= 10; cellIndex++)
                                    {
                                        model.FormInput.GRRList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "L", Define.Seperator, "C", Define.Seperator, part, Define.Seperator, trials, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(cellIndex))));

                                        part++;
                                    }
                                    trials++;
                                }
                                #endregion

                                result = Edit(OrganizationList, model);
                            }
                        }
                    }
                    else
                    {
                        result.ReturnFailedMessage("上傳失敗");
                    }
                }
                //計數
                else if (type == "2")
                {
                    var uniqueID = string.Empty;

                    try
                    {
                        var sheet = workBook.GetSheetAt(3);

                        var row = sheet.GetRow(0);
                        var cell = row.GetCell(0);

                        uniqueID = ExcelHelper.GetCellValue(cell);
                    }
                    catch
                    {
                        uniqueID = string.Empty;
                    }

                    if (uniqueID != UniqueID)
                    {
                        result.ReturnFailedMessage("請使用系統下載之指定檔案進行資料輸入及上傳");
                    }
                    else
                    {
                        result = GetEditFormModel(UniqueID);

                        if (result.IsSuccess)
                        {
                            var model = result.Data as EditFormModel;

                            var sheet = workBook.GetSheetAt(0);

                            var row = sheet.GetRow(3);

                            var tmp = ExcelHelper.GetCellValue(row.GetCell(2));

                            if (tmp.Contains("/"))
                            {
                                var t = tmp.Split('/');

                                model.FormInput.CountPackageType = t[0];
                            }
                            else
                            {
                                model.FormInput.CountPackageType = tmp;
                            }

                            var cellValue = ExcelHelper.GetCellValue(row.GetCell(11));

                            model.FormInput.MSADate = !string.IsNullOrEmpty(cellValue) ? DateTime.Parse(cellValue) : default(DateTime?);

                            int sample = 1;
                            for (int rowIndex = 9; rowIndex <= 58; rowIndex++)
                            {
                                row = sheet.GetRow(rowIndex);
                                model.FormInput.CountReferenceList.Add(string.Format("{0}{1}{2}{3}{4}", sample, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(10)), Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(11))));
                                sample++;
                            }

                            row = sheet.GetRow(6);
                            model.FormInput.CountAppraiserList.Add(string.Format("{0}{1}{2}", "A", Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(1))));
                            model.FormInput.CountAppraiserList.Add(string.Format("{0}{1}{2}", "B", Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(4))));
                            model.FormInput.CountAppraiserList.Add(string.Format("{0}{1}{2}", "C", Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(7))));

                            sample = 1;
                            for (int rowIndex = 9; rowIndex <= 58; rowIndex++)
                            {
                                row = sheet.GetRow(rowIndex);
                                int trials = 1;

                                for (int cellIndex = 1; cellIndex <= 3; cellIndex++)
                                {
                                    model.FormInput.CountValueList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}", "A", Define.Seperator, sample, Define.Seperator, trials, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(cellIndex))));

                                    trials++;
                                }

                                sample++;
                            }

                            sample = 1;
                            for (int rowIndex = 9; rowIndex <= 58; rowIndex++)
                            {
                                row = sheet.GetRow(rowIndex);
                                int trials = 1;

                                for (int cellIndex = 4; cellIndex <= 6; cellIndex++)
                                {
                                    model.FormInput.CountValueList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}", "B", Define.Seperator, sample, Define.Seperator, trials, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(cellIndex))));

                                    trials++;
                                }

                                sample++;
                            }

                            sample = 1;
                            for (int rowIndex = 9; rowIndex <= 58; rowIndex++)
                            {
                                row = sheet.GetRow(rowIndex);
                                int trials = 1;

                                for (int cellIndex = 7; cellIndex <= 9; cellIndex++)
                                {
                                    model.FormInput.CountValueList.Add(string.Format("{0}{1}{2}{3}{4}{5}{6}", "C", Define.Seperator, sample, Define.Seperator, trials, Define.Seperator, ExcelHelper.GetCellValue(row.GetCell(cellIndex))));

                                    trials++;
                                }

                                sample++;
                            }


                            result = Edit(OrganizationList, model);
                        }
                    }
                }
                else
                {
                    result.ReturnFailedMessage("上傳失敗");
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

        public static RequestResult Edit(List<Models.Shared.Organization> OrganizationList, EditFormModel Model)
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
                    if (Model.Type == "1")
                    {
                        #region Form
                        var form = db.QA_MSAFORM.First(x => x.UNIQUEID == Model.UniqueID);

                        //Stability
                        form.STABILITYLOWERRANGE = Model.FormInput.StabilityLowerRange;
                        form.STABILITYUPPERRANGE = Model.FormInput.StabilityUpperRange;
                        form.STABILITYPREPAREDATE = Model.FormInput.StabilityPrepareDate;

                        //Bias & Linearity
                        form.BLPACKAGETYPE = Model.FormInput.BLPackageType;
                        form.BLDATE = Model.FormInput.BLDate;

                        //Bias
                        form.BIASREFVAL = Model.FormInput.BiasReferenceValue;

                        //GRRH
                        form.GRRHPACKAGETYPE = Model.FormInput.GRRHPackageType;
                        form.GRRHUSL = Model.FormInput.GRRHUSL;
                        form.GRRHCL = Model.FormInput.GRRHCL;
                        form.GRRHLSL = Model.FormInput.GRRHLSL;
                        form.GRRHTVUSED = Model.FormInput.GRRHTvUsed;

                        //GRRM
                        form.GRRMPACKAGETYPE = Model.FormInput.GRRMPackageType;
                        form.GRRMUSL = Model.FormInput.GRRMUSL;
                        form.GRRMCL = Model.FormInput.GRRMCL;
                        form.GRRMLSL = Model.FormInput.GRRMLSL;
                        form.GRRMTVUSED = Model.FormInput.GRRMTvUsed;

                        //GRRL
                        form.GRRLPACKAGETYPE = Model.FormInput.GRRLPackageType;
                        form.GRRLUSL = Model.FormInput.GRRLUSL;
                        form.GRRLCL = Model.FormInput.GRRLCL;
                        form.GRRLLSL = Model.FormInput.GRRLLSL;
                        form.GRRLTVUSED = Model.FormInput.GRRLTvUsed;

                        db.SaveChanges();
                        #endregion

                        #region Stability
                        db.QA_MSAFORMSTABILITY.RemoveRange(db.QA_MSAFORMSTABILITY.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList());

                        db.SaveChanges();

                        foreach (var stabilityDate in Model.FormInput.StabilityDateList)
                        {
                            string[] tmp = stabilityDate.Split(Define.Seperators, StringSplitOptions.None);

                            var point = tmp[0];
                            var date = tmp[1];

                            db.QA_MSAFORMSTABILITY.Add(new QA_MSAFORMSTABILITY()
                            {
                                FORMUNIQUEID = form.UNIQUEID,
                                POINT = decimal.Parse(point),
                                DATE = DateTimeHelper.DateStringWithSeperator2DateTime(date)
                            });
                        }

                        db.SaveChanges();
                        #endregion

                        #region StabilityValue
                        db.QA_MSAFORMSTABILITYVALUE.RemoveRange(db.QA_MSAFORMSTABILITYVALUE.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList());

                        db.SaveChanges();

                        foreach (var stability in Model.FormInput.StabilityList)
                        {
                            string[] tmp = stability.Split(Define.Seperators, StringSplitOptions.None);

                            var point = tmp[0];
                            var data = tmp[1];
                            var value = tmp[2];

                            db.QA_MSAFORMSTABILITYVALUE.Add(new QA_MSAFORMSTABILITYVALUE()
                            {
                                FORMUNIQUEID = form.UNIQUEID,
                                POINT = decimal.Parse(point),
                                DATA = decimal.Parse(data),
                                VALUE = !string.IsNullOrEmpty(value) ? decimal.Parse(value) : default(decimal?)
                            });
                        }

                        db.SaveChanges();
                        #endregion

                        #region Bias
                        db.QA_MSAFORMBIASVALUE.RemoveRange(db.QA_MSAFORMBIASVALUE.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList());

                        db.SaveChanges();

                        foreach (var bias in Model.FormInput.BiasList)
                        {
                            string[] tmp = bias.Split(Define.Seperators, StringSplitOptions.None);

                            var trials = tmp[0];
                            var data = tmp[1];

                            db.QA_MSAFORMBIASVALUE.Add(new QA_MSAFORMBIASVALUE()
                            {
                                FORMUNIQUEID = form.UNIQUEID,
                                TRIALS = decimal.Parse(trials),
                                VALUE = !string.IsNullOrEmpty(data) ? decimal.Parse(data) : default(decimal?)
                            });
                        }

                        db.SaveChanges();
                        #endregion

                        #region Linearity
                        db.QA_MSAFORMLINEARITY.RemoveRange(db.QA_MSAFORMLINEARITY.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList());

                        db.SaveChanges();

                        foreach (var LinearityReference in Model.FormInput.LinearityReferenceValueList)
                        {
                            string[] tmp = LinearityReference.Split(Define.Seperators, StringSplitOptions.None);

                            var trials = tmp[0];
                            var value = tmp[1];

                            db.QA_MSAFORMLINEARITY.Add(new QA_MSAFORMLINEARITY()
                            {
                                FORMUNIQUEID = form.UNIQUEID,
                                TRIALS = decimal.Parse(trials),
                                REFVAL = !string.IsNullOrEmpty(value) ? decimal.Parse(value) : default(decimal?)
                            });
                        }

                        db.SaveChanges();
                        #endregion

                        #region LinearityValue
                        db.QA_MSAFORMLINEARITYVALUE.RemoveRange(db.QA_MSAFORMLINEARITYVALUE.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList());

                        db.SaveChanges();

                        foreach (var linearity in Model.FormInput.LinearityList)
                        {
                            string[] tmp = linearity.Split(Define.Seperators, StringSplitOptions.None);

                            var trials = tmp[0];
                            var data = tmp[1];
                            var value = tmp[2];

                            db.QA_MSAFORMLINEARITYVALUE.Add(new QA_MSAFORMLINEARITYVALUE()
                            {
                                FORMUNIQUEID = form.UNIQUEID,
                                TRIALS = decimal.Parse(trials),
                                DATA = decimal.Parse(data),
                                VALUE = !string.IsNullOrEmpty(value) ? decimal.Parse(value) : default(decimal?)
                            });
                        }

                        db.SaveChanges();
                        #endregion

                        #region GRR
                        db.QA_MSAFORMGRR.RemoveRange(db.QA_MSAFORMGRR.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList());

                        db.SaveChanges();

                        var grrList = new List<QA_MSAFORMGRR>();

                        foreach (var grrAppraiserString in Model.FormInput.GRRAppraiserList)
                        {
                            string[] tmp = grrAppraiserString.Split(Define.Seperators, StringSplitOptions.None);

                            var level = tmp[0];
                            var appraiser = tmp[1];
                            var userID = tmp[2];
                            var date = tmp[3];

                            var grr = grrList.FirstOrDefault(x => x.FORMUNIQUEID == form.UNIQUEID && x.LEVEL == level);

                            if (grr == null)
                            {
                                grr = new QA_MSAFORMGRR()
                                {
                                    FORMUNIQUEID = form.UNIQUEID,
                                    LEVEL = level,
                                    DATE = DateTimeHelper.DateStringWithSeperator2DateTime(date)
                                };

                                if (appraiser == "A")
                                {
                                    grr.APPRAISERA = userID;
                                }
                                else if (appraiser == "B")
                                {
                                    grr.APPRAISERB = userID;
                                }
                                else if (appraiser == "C")
                                {
                                    grr.APPRAISERC = userID;
                                }

                                grrList.Add(grr);
                            }
                            else
                            {
                                if (appraiser == "A")
                                {
                                    grr.APPRAISERA = userID;
                                }
                                else if (appraiser == "B")
                                {
                                    grr.APPRAISERB = userID;
                                }
                                else if (appraiser == "C")
                                {
                                    grr.APPRAISERC = userID;
                                }
                            }
                        }

                        db.QA_MSAFORMGRR.AddRange(grrList);

                        db.SaveChanges();
                        #endregion

                        #region Form.MSADate

                        form = db.QA_MSAFORM.First(x => x.UNIQUEID == Model.UniqueID);

                        var msaDate = Model.FormInput.MSADate;

                        var q = grrList.Where(x => x.DATE.HasValue).OrderByDescending(x => x.DATE.Value).FirstOrDefault();

                        if (q != null)
                        {
                            if (msaDate.HasValue)
                            {
                                if (DateTime.Compare(q.DATE.Value, msaDate.Value) > 0)
                                {
                                    msaDate = q.DATE.Value;
                                }
                            }
                            else
                            {
                                msaDate = q.DATE.Value;
                            }
                        }

                        form.MSADATE = msaDate;

                        db.SaveChanges();
                        #endregion

                        #region GRRValue
                        db.QA_MSAFORMGRRVALUE.RemoveRange(db.QA_MSAFORMGRRVALUE.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList());

                        db.SaveChanges();

                        foreach (var grrString in Model.FormInput.GRRList)
                        {
                            string[] tmp = grrString.Split(Define.Seperators, StringSplitOptions.None);

                            var level = tmp[0];
                            var appraiser = tmp[1];
                            var part = tmp[2];
                            var trials = tmp[3];
                            var value = tmp[4];

                            db.QA_MSAFORMGRRVALUE.Add(new QA_MSAFORMGRRVALUE()
                            {
                                FORMUNIQUEID = form.UNIQUEID,
                                LEVEL = level,
                                APPRAISER = appraiser,
                                PART = decimal.Parse(part),
                                TRIALS = decimal.Parse(trials),
                                VALUE = !string.IsNullOrEmpty(value) ? decimal.Parse(value) : default(decimal?),
                            });
                        }

                        db.SaveChanges();
                        #endregion
                    }

                    if (Model.Type == "2")
                    {
                        var form = db.QA_MSAFORM.First(x => x.UNIQUEID == Model.UniqueID);

                        form.MSADATE = Model.FormInput.MSADate;
                        form.COUNTPACKAGETYPE = Model.FormInput.CountPackageType;

                        db.SaveChanges();

                        db.QA_MSAFORMCOUNTREFVAL.RemoveRange(db.QA_MSAFORMCOUNTREFVAL.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList());

                        db.SaveChanges();

                        foreach (var countReference in Model.FormInput.CountReferenceList)
                        {
                            string[] tmp = countReference.Split(Define.Seperators, StringSplitOptions.None);

                            var sample = tmp[0];
                            var reference = tmp[1];
                            var referenceValue = tmp[2];

                            db.QA_MSAFORMCOUNTREFVAL.Add(new QA_MSAFORMCOUNTREFVAL()
                            {
                                FORMUNIQUEID = form.UNIQUEID,
                                SAMPLE = int.Parse(sample),
                                REF = !string.IsNullOrEmpty(reference) ? int.Parse(reference) : default(int?),
                                REFVAL = !string.IsNullOrEmpty(referenceValue) ? int.Parse(referenceValue) : default(int?),
                            });
                        }

                        db.SaveChanges();

                        db.QA_MSAFORMCOUNT.RemoveRange(db.QA_MSAFORMCOUNT.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList());

                        db.SaveChanges();

                        foreach (var countAppraiser in Model.FormInput.CountAppraiserList)
                        {
                            string[] tmp = countAppraiser.Split(Define.Seperators, StringSplitOptions.None);

                            var appraiser = tmp[0];
                            var userID = tmp[1];

                            db.QA_MSAFORMCOUNT.Add(new QA_MSAFORMCOUNT()
                            {
                                FORMUNIQUEID = form.UNIQUEID,
                                APPRAISER = appraiser,
                                USERID = userID
                            });
                        }

                        db.SaveChanges();

                        db.QA_MSAFORMCOUNTVALUE.RemoveRange(db.QA_MSAFORMCOUNTVALUE.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList());

                        db.SaveChanges();

                        foreach (var countValue in Model.FormInput.CountValueList)
                        {
                            string[] tmp = countValue.Split(Define.Seperators, StringSplitOptions.None);

                            var appraiser = tmp[0];
                            var sample = tmp[1];
                            var trials = tmp[2];
                            var value = tmp[3];

                            db.QA_MSAFORMCOUNTVALUE.Add(new QA_MSAFORMCOUNTVALUE()
                            {
                                FORMUNIQUEID = form.UNIQUEID,
                                APPRAISER = appraiser,
                                SAMPLE = int.Parse(sample),
                                TRIALS = int.Parse(trials),
                                VALUE = !string.IsNullOrEmpty(value) ? int.Parse(value) : default(int?)
                            });
                        }

                        db.SaveChanges();
                    }
                }

#if !DEBUG
                    trans.Complete();
                }
#endif

                result.ReturnSuccessMessage("匯入成功");

                GenExcel(OrganizationList, Model.UniqueID);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static void GenExcel(List<Models.Shared.Organization> OrganizationList, string UniqueID)
        {
            try
            {
                var model = GetDetailViewModel(OrganizationList, UniqueID).Data as DetailViewModel;

                var fileName = Path.Combine(Config.QAFile_v2FolderPath, string.Format("{0}.xls", model.UniqueID));

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                IWorkbook workBook;

                #region 計量
                if (model.Type == "1")
                {
                    if (model.SubType == "1")
                    {
                        File.Copy(Config.ReportTemplate_MSA, fileName);

                        using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                        {
                            workBook = new HSSFWorkbook(file);
                            file.Close();
                        }

                        #region 封面
                        var sheet = workBook.GetSheetAt(0);

                        var row = sheet.GetRow(3);
                        var cell = row.GetCell(5);

                        cell.SetCellValue(model.MSAIchi);

                        row = sheet.GetRow(4);
                        cell = row.GetCell(5);

                        cell.SetCellValue(model.Equipment.MSACalNo);

                        row = sheet.GetRow(5);
                        cell = row.GetCell(5);

                        cell.SetCellValue(model.Equipment.Brand);

                        row = sheet.GetRow(6);
                        cell = row.GetCell(5);

                        cell.SetCellValue(model.Equipment.Model);

                        row = sheet.GetRow(7);
                        cell = row.GetCell(5);

                        cell.SetCellValue(model.Station);

                        row = sheet.GetRow(8);
                        cell = row.GetCell(5);

                        cell.SetCellValue(model.MSACharacteristic);

                        row = sheet.GetRow(9);
                        cell = row.GetCell(5);

                        cell.SetCellValue(model.MSARange);

                        row = sheet.GetRow(10);
                        cell = row.GetCell(5);

                        cell.SetCellValue(model.Unit);

                        row = sheet.GetRow(11);
                        cell = row.GetCell(5);

                        cell.SetCellValue(model.MSAResponsor);

                        row = sheet.GetRow(12);
                        cell = row.GetCell(5);

                        cell.SetCellValue(model.MSADateString);

                        sheet.ForceFormulaRecalculation = true;
                        #endregion

                        #region Stability
                        sheet = workBook.GetSheetAt(1);

                        row = sheet.GetRow(6);

                        cell = row.GetCell(0);
                        cell.SetCellValue(model.Equipment.MSACalNo);

                        cell = row.GetCell(4);
                        cell.SetCellValue(model.MSAIchi);

                        cell = row.GetCell(7);
                        cell.SetCellValue(model.Equipment.Model);

                        cell = row.GetCell(10);
                        cell.SetCellValue(model.Station);

                        cell = row.GetCell(12);
                        cell.SetCellValue("5次/DAY");

                        cell = row.GetCell(15);
                        cell.SetCellValue(model.Unit);

                        cell = row.GetCell(17);
                        cell.SetCellValue(model.StabilityLowerRange);

                        cell = row.GetCell(20);
                        cell.SetCellValue(model.StabilityUpperRange);

                        cell = row.GetCell(23);
                        cell.SetCellValue(model.StabilityPrepareDateString);

                        cell = row.GetCell(26);

                        var min = model.StabilityList.Where(x => x.Date.HasValue).OrderBy(x => x.Date).FirstOrDefault();
                        var max = model.StabilityList.Where(x => x.Date.HasValue).OrderByDescending(x => x.Date).FirstOrDefault();

                        var date = string.Empty;

                        if (min != null && max != null)
                        {
                            date = string.Format("{0}~{1}", DateTimeHelper.DateTime2DateString(min.Date), DateTimeHelper.DateTime2DateString(max.Date));
                        }
                        else if (min != null && max == null)
                        {
                            date = DateTimeHelper.DateTime2DateString(min.Date);
                        }
                        else if (min == null && max != null)
                        {
                            date = DateTimeHelper.DateTime2DateString(max.Date);
                        }

                        cell.SetCellValue(date);

                        cell = row.GetCell(29);
                        cell.SetCellValue(model.MSAResponsor);

                        var cellIndex = 0;

                        foreach (var stability in model.StabilityList.OrderBy(x => x.Point))
                        {
                            cellIndex = Convert.ToInt32(stability.Point) + 1;

                            row = sheet.GetRow(22);
                            cell = row.GetCell(cellIndex);

                            var stablityValue = stability.ValueList.FirstOrDefault(x => x.Data == 1);

                            if (stablityValue != null && stablityValue.Value.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(stablityValue.Value));
                            }

                            row = sheet.GetRow(23);
                            cell = row.GetCell(cellIndex);

                            stablityValue = stability.ValueList.FirstOrDefault(x => x.Data == 2);

                            if (stablityValue != null && stablityValue.Value.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(stablityValue.Value));
                            }

                            row = sheet.GetRow(24);
                            cell = row.GetCell(cellIndex);

                            stablityValue = stability.ValueList.FirstOrDefault(x => x.Data == 3);

                            if (stablityValue != null && stablityValue.Value.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(stablityValue.Value));
                            }

                            row = sheet.GetRow(25);
                            cell = row.GetCell(cellIndex);

                            stablityValue = stability.ValueList.FirstOrDefault(x => x.Data == 4);

                            if (stablityValue != null && stablityValue.Value.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(stablityValue.Value));
                            }

                            row = sheet.GetRow(26);
                            cell = row.GetCell(cellIndex);

                            stablityValue = stability.ValueList.FirstOrDefault(x => x.Data == 5);

                            if (stablityValue != null && stablityValue.Value.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(stablityValue.Value));
                            }

                            row = sheet.GetRow(29);
                            cell = row.GetCell(cellIndex);

                            if (stability.Date.HasValue)
                            {
                                cell.SetCellValue(stability.Date.Value);
                            }
                        }

                        sheet.ForceFormulaRecalculation = true;
                        #endregion

                        #region BL
                        sheet = workBook.GetSheetAt(2);

                        row = sheet.GetRow(0);

                        cell = row.GetCell(12);

                        cell.SetCellValue(model.MSAResponsor);

                        cell = row.GetCell(18);

                        if (model.BLDate.HasValue)
                        {
                            cell.SetCellValue(model.BLDate.Value);
                        }

                        row = sheet.GetRow(3);

                        cell = row.GetCell(0);

                        cell.SetCellValue(model.MSAIchi);

                        cell = row.GetCell(6);

                        cell.SetCellValue(model.Equipment.MSACalNo);

                        cell = row.GetCell(12);

                        cell.SetCellValue(model.BLPackageType);

                        cell = row.GetCell(17);

                        cell.SetCellValue(model.MSACharacteristic);

                        if (model.BiasReferenceValue.HasValue)
                        {
                            row = sheet.GetRow(7);

                            cell = row.GetCell(3);

                            cell.SetCellValue(Convert.ToDouble(model.BiasReferenceValue.Value));
                        }

                        var rowIndex = 0;

                        foreach (var bias in model.BiasList.OrderBy(x => x.Trials))
                        {
                            rowIndex = 9 + Convert.ToInt32(bias.Trials) - 1;

                            row = sheet.GetRow(rowIndex);

                            cell = row.GetCell(2);

                            if (bias.Value.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(bias.Value.Value));
                            }
                        }

                        sheet.ForceFormulaRecalculation = true;

                        cellIndex = 0;

                        foreach (var linearity in model.LinearityList.OrderBy(x => x.Trials))
                        {
                            cellIndex = 10 + Convert.ToInt32(linearity.Trials) - 1;

                            if (linearity.ReferenceValue.HasValue)
                            {
                                row = sheet.GetRow(7);
                                cell = row.GetCell(cellIndex);
                                cell.SetCellValue(Convert.ToDouble(linearity.ReferenceValue.Value));
                            }

                            rowIndex = 9;
                            for (int data = 1; data <= 12; data++)
                            {
                                row = sheet.GetRow(rowIndex);
                                cell = row.GetCell(cellIndex);

                                var linearityValue = linearity.ValueList.FirstOrDefault(x => x.Data == data);

                                if (linearityValue != null && linearityValue.Value.HasValue)
                                {
                                    cell.SetCellValue(Convert.ToDouble(linearityValue.Value));
                                }

                                rowIndex++;
                            }
                        }

                        sheet.ForceFormulaRecalculation = true;
                        #endregion

                        #region GRRH
                        var grrH = model.GRRList.First(x => x.Level == "H");

                        if (grrH != null)
                        {
                            sheet = workBook.GetSheetAt(3);

                            row = sheet.GetRow(0);

                            cell = row.GetCell(7);
                            cell.SetCellValue(model.MSAResponsor);

                            cell = row.GetCell(10);
                            cell.SetCellValue(grrH.DateString);

                            row = sheet.GetRow(1);

                            if (model.GRRHTvUsed == "1")
                            {
                                cell = row.GetCell(11);
                                cell.SetCellValue("V");
                            }

                            row = sheet.GetRow(2);

                            cell = row.GetCell(0);
                            cell.SetCellValue(model.Equipment.MSACalNo);

                            cell = row.GetCell(3);
                            cell.SetCellValue(model.GRRHPackageType);

                            cell = row.GetCell(6);
                            cell.SetCellValue(model.MSACharacteristic);

                            cell = row.GetCell(10);

                            if (model.GRRHUSL.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(model.GRRHUSL));
                            }

                            if (model.GRRHTvUsed == "2")
                            {
                                cell = row.GetCell(11);
                                cell.SetCellValue("V");
                            }

                            row = sheet.GetRow(3);

                            cell = row.GetCell(10);
                            if (model.GRRHCL.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(model.GRRHCL));
                            }

                            if (model.GRRHTvUsed == "3")
                            {
                                cell = row.GetCell(11);
                                cell.SetCellValue("V");
                            }

                            row = sheet.GetRow(4);

                            cell = row.GetCell(10);
                            if (model.GRRHLSL.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(model.GRRHLSL));
                            }

                            if (model.GRRHTvUsed == "4")
                            {
                                cell = row.GetCell(11);
                                cell.SetCellValue("V");
                            }

                            row = sheet.GetRow(8);

                            for (int part = 1; part <= 10; part++)
                            {
                                var h = grrH.AppraiserList.First(x => x.Appraiser == "A").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 1);

                                if (h != null)
                                {
                                    cell = row.GetCell(part);
                                    if (h.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(h.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(9);

                            var appraiser = grrH.AppraiserList.First(x => x.Appraiser == "A");

                            if (appraiser != null)
                            {
                                cell = row.GetCell(0);
                                cell.SetCellValue(appraiser.UserID);

                                for (int part = 1; part <= 10; part++)
                                {
                                    var h = grrH.AppraiserList.First(x => x.Appraiser == "A").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 2);

                                    if (h != null)
                                    {
                                        cell = row.GetCell(part);
                                        if (h.Value.HasValue)
                                        {
                                            cell.SetCellValue(Convert.ToDouble(h.Value));
                                        }
                                    }
                                }
                            }

                            row = sheet.GetRow(10);

                            for (int part = 1; part <= 10; part++)
                            {
                                var h = grrH.AppraiserList.First(x => x.Appraiser == "A").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 3);

                                if (h != null)
                                {
                                    cell = row.GetCell(part);
                                    if (h.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(h.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(13);

                            for (int part = 1; part <= 10; part++)
                            {
                                var h = grrH.AppraiserList.First(x => x.Appraiser == "B").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 1);

                                if (h != null)
                                {
                                    cell = row.GetCell(part);
                                    if (h.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(h.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(14);

                            appraiser = grrH.AppraiserList.FirstOrDefault(x => x.Appraiser == "B");

                            if (appraiser != null)
                            {
                                cell = row.GetCell(0);
                                cell.SetCellValue(appraiser.UserID);

                                for (int part = 1; part <= 10; part++)
                                {
                                    var h = grrH.AppraiserList.First(x => x.Appraiser == "B").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 2);

                                    if (h != null)
                                    {
                                        cell = row.GetCell(part);
                                        if (h.Value.HasValue)
                                        {
                                            cell.SetCellValue(Convert.ToDouble(h.Value));
                                        }
                                    }
                                }
                            }

                            row = sheet.GetRow(15);

                            for (int part = 1; part <= 10; part++)
                            {
                                var h = grrH.AppraiserList.First(x => x.Appraiser == "B").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 3);

                                if (h != null)
                                {
                                    cell = row.GetCell(part);
                                    if (h.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(h.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(18);

                            for (int part = 1; part <= 10; part++)
                            {
                                var h = grrH.AppraiserList.First(x => x.Appraiser == "C").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 1);

                                if (h != null)
                                {
                                    cell = row.GetCell(part);
                                    if (h.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(h.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(19);

                            appraiser = grrH.AppraiserList.FirstOrDefault(x => x.Appraiser == "C");

                            if (appraiser != null)
                            {
                                cell = row.GetCell(0);
                                cell.SetCellValue(appraiser.UserID);

                                for (int part = 1; part <= 10; part++)
                                {
                                    var h = grrH.AppraiserList.First(x => x.Appraiser == "C").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 2);

                                    if (h != null)
                                    {
                                        cell = row.GetCell(part);
                                        if (h.Value.HasValue)
                                        {
                                            cell.SetCellValue(Convert.ToDouble(h.Value));
                                        }
                                    }
                                }
                            }

                            row = sheet.GetRow(20);

                            for (int part = 1; part <= 10; part++)
                            {
                                var h = grrH.AppraiserList.First(x => x.Appraiser == "C").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 3);

                                if (h != null)
                                {
                                    cell = row.GetCell(part);
                                    if (h.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(h.Value));
                                    }
                                }
                            }

                            sheet.ForceFormulaRecalculation = true;

                            sheet = workBook.GetSheetAt(4);

                            row = sheet.GetRow(30);
                            cell = row.GetCell(0);
                            appraiser = grrH.AppraiserList.First(x => x.Appraiser == "A");
                            cell.SetCellValue(appraiser.UserID);

                            row = sheet.GetRow(33);
                            cell = row.GetCell(0);
                            appraiser = grrH.AppraiserList.First(x => x.Appraiser == "B");
                            cell.SetCellValue(appraiser.UserID);

                            row = sheet.GetRow(36);
                            cell = row.GetCell(0);
                            appraiser = grrH.AppraiserList.First(x => x.Appraiser == "C");
                            cell.SetCellValue(appraiser.UserID);

                            sheet.ForceFormulaRecalculation = true;
                        }
                        #endregion

                        #region GRRM
                        var grrM = model.GRRList.First(x => x.Level == "M");

                        if (grrM != null)
                        {
                            sheet = workBook.GetSheetAt(5);

                            row = sheet.GetRow(0);

                            cell = row.GetCell(7);
                            cell.SetCellValue(model.MSAResponsor);

                            cell = row.GetCell(10);
                            cell.SetCellValue(grrM.DateString);

                            row = sheet.GetRow(1);

                            if (model.GRRMTvUsed == "1")
                            {
                                cell = row.GetCell(11);
                                cell.SetCellValue("V");
                            }

                            row = sheet.GetRow(2);

                            cell = row.GetCell(0);
                            cell.SetCellValue(model.Equipment.MSACalNo);

                            cell = row.GetCell(3);
                            cell.SetCellValue(model.GRRMPackageType);

                            cell = row.GetCell(6);
                            cell.SetCellValue(model.MSACharacteristic);

                            cell = row.GetCell(10);
                            if (model.GRRMUSL.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(model.GRRMUSL));
                            }

                            if (model.GRRMTvUsed == "2")
                            {
                                cell = row.GetCell(11);
                                cell.SetCellValue("V");
                            }

                            row = sheet.GetRow(3);

                            cell = row.GetCell(10);
                            if (model.GRRMCL.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(model.GRRMCL));
                            }

                            if (model.GRRMTvUsed == "3")
                            {
                                cell = row.GetCell(11);
                                cell.SetCellValue("V");
                            }

                            row = sheet.GetRow(4);

                            cell = row.GetCell(10);
                            if (model.GRRMLSL.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(model.GRRMLSL));
                            }

                            if (model.GRRMTvUsed == "4")
                            {
                                cell = row.GetCell(11);
                                cell.SetCellValue("V");
                            }

                            row = sheet.GetRow(8);

                            for (int part = 1; part <= 10; part++)
                            {
                                var m = grrM.AppraiserList.First(x => x.Appraiser == "A").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 1);

                                if (m != null)
                                {
                                    cell = row.GetCell(part);
                                    if (m.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(m.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(9);

                            var appraiser = grrM.AppraiserList.First(x => x.Appraiser == "A");

                            if (appraiser != null)
                            {
                                cell = row.GetCell(0);
                                cell.SetCellValue(appraiser.UserID);

                                for (int part = 1; part <= 10; part++)
                                {
                                    var m = grrM.AppraiserList.First(x => x.Appraiser == "A").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 2);

                                    if (m != null)
                                    {
                                        cell = row.GetCell(part);
                                        if (m.Value.HasValue)
                                        {
                                            cell.SetCellValue(Convert.ToDouble(m.Value));
                                        }
                                    }
                                }
                            }

                            row = sheet.GetRow(10);

                            for (int part = 1; part <= 10; part++)
                            {
                                var m = grrM.AppraiserList.First(x => x.Appraiser == "A").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 3);

                                if (m != null)
                                {
                                    cell = row.GetCell(part);
                                    if (m.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(m.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(13);

                            for (int part = 1; part <= 10; part++)
                            {
                                var m = grrM.AppraiserList.First(x => x.Appraiser == "B").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 1);

                                if (m != null)
                                {
                                    cell = row.GetCell(part);
                                    if (m.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(m.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(14);

                            appraiser = grrM.AppraiserList.First(x => x.Appraiser == "B");

                            if (appraiser != null)
                            {
                                cell = row.GetCell(0);
                                cell.SetCellValue(appraiser.UserID);

                                for (int part = 1; part <= 10; part++)
                                {
                                    var m = grrM.AppraiserList.First(x => x.Appraiser == "B").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 2);

                                    if (m != null)
                                    {
                                        cell = row.GetCell(part);
                                        if (m.Value.HasValue)
                                        {
                                            cell.SetCellValue(Convert.ToDouble(m.Value));
                                        }
                                    }
                                }
                            }

                            row = sheet.GetRow(15);

                            for (int part = 1; part <= 10; part++)
                            {
                                var m = grrM.AppraiserList.First(x => x.Appraiser == "B").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 3);

                                if (m != null)
                                {
                                    cell = row.GetCell(part);

                                    if (m.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(m.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(18);

                            for (int part = 1; part <= 10; part++)
                            {
                                var m = grrM.AppraiserList.First(x => x.Appraiser == "C").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 1);

                                if (m != null)
                                {
                                    cell = row.GetCell(part);
                                    if (m.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(m.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(19);

                            appraiser = grrM.AppraiserList.First(x => x.Appraiser == "C");

                            if (appraiser != null)
                            {
                                cell = row.GetCell(0);
                                cell.SetCellValue(appraiser.UserID);

                                for (int part = 1; part <= 10; part++)
                                {
                                    var m = grrM.AppraiserList.First(x => x.Appraiser == "C").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 2);

                                    if (m != null)
                                    {
                                        cell = row.GetCell(part);
                                        if (m.Value.HasValue)
                                        {
                                            cell.SetCellValue(Convert.ToDouble(m.Value));
                                        }
                                    }
                                }
                            }

                            row = sheet.GetRow(20);

                            for (int part = 1; part <= 10; part++)
                            {
                                var m = grrM.AppraiserList.First(x => x.Appraiser == "C").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 3);

                                if (m != null)
                                {
                                    cell = row.GetCell(part);
                                    if (m.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(m.Value));
                                    }
                                }
                            }

                            sheet.ForceFormulaRecalculation = true;

                            sheet = workBook.GetSheetAt(6);

                            row = sheet.GetRow(30);
                            cell = row.GetCell(0);
                            appraiser = grrM.AppraiserList.First(x => x.Appraiser == "A");
                            cell.SetCellValue(appraiser.UserID);

                            row = sheet.GetRow(33);
                            cell = row.GetCell(0);
                            appraiser = grrM.AppraiserList.First(x => x.Appraiser == "B");
                            cell.SetCellValue(appraiser.UserID);

                            row = sheet.GetRow(36);
                            cell = row.GetCell(0);
                            appraiser = grrM.AppraiserList.First(x => x.Appraiser == "C");
                            cell.SetCellValue(appraiser.UserID);

                            sheet.ForceFormulaRecalculation = true;
                        }
                        #endregion

                        #region GRRL
                        var grrL = model.GRRList.First(x => x.Level == "L");

                        if (grrL != null)
                        {
                            sheet = workBook.GetSheetAt(7);

                            row = sheet.GetRow(0);

                            cell = row.GetCell(7);
                            cell.SetCellValue(model.MSAResponsor);

                            cell = row.GetCell(10);
                            cell.SetCellValue(grrL.DateString);

                            row = sheet.GetRow(1);

                            if (model.GRRLTvUsed == "1")
                            {
                                cell = row.GetCell(11);
                                cell.SetCellValue("V");
                            }

                            row = sheet.GetRow(2);

                            cell = row.GetCell(0);
                            cell.SetCellValue(model.Equipment.MSACalNo);

                            cell = row.GetCell(3);
                            cell.SetCellValue(model.GRRLPackageType);

                            cell = row.GetCell(6);
                            cell.SetCellValue(model.MSACharacteristic);

                            cell = row.GetCell(10);
                            if (model.GRRLUSL.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(model.GRRLUSL));
                            }

                            if (model.GRRLTvUsed == "2")
                            {
                                cell = row.GetCell(11);
                                cell.SetCellValue("V");
                            }

                            row = sheet.GetRow(3);

                            cell = row.GetCell(10);
                            if (model.GRRLCL.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(model.GRRLCL));
                            }

                            if (model.GRRLTvUsed == "3")
                            {
                                cell = row.GetCell(11);
                                cell.SetCellValue("V");
                            }

                            row = sheet.GetRow(4);

                            cell = row.GetCell(10);
                            if (model.GRRLLSL.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(model.GRRLLSL));
                            }

                            if (model.GRRLTvUsed == "4")
                            {
                                cell = row.GetCell(11);
                                cell.SetCellValue("V");
                            }

                            row = sheet.GetRow(8);

                            for (int part = 1; part <= 10; part++)
                            {
                                var l = grrL.AppraiserList.First(x => x.Appraiser == "A").ValueList.First(x => x.Part == part && x.Trials == 1);

                                if (l != null)
                                {
                                    cell = row.GetCell(part);
                                    if (l.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(l.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(9);

                            var appraiser = grrL.AppraiserList.First(x => x.Appraiser == "A");

                            if (appraiser != null)
                            {
                                cell = row.GetCell(0);
                                cell.SetCellValue(appraiser.UserID);

                                for (int part = 1; part <= 10; part++)
                                {
                                    var l = grrL.AppraiserList.First(x => x.Appraiser == "A").ValueList.First(x => x.Part == part && x.Trials == 2);

                                    if (l != null)
                                    {
                                        cell = row.GetCell(part);
                                        if (l.Value.HasValue)
                                        {
                                            cell.SetCellValue(Convert.ToDouble(l.Value));
                                        }
                                    }
                                }
                            }

                            row = sheet.GetRow(10);

                            for (int part = 1; part <= 10; part++)
                            {
                                var l = grrL.AppraiserList.First(x => x.Appraiser == "A").ValueList.First(x => x.Part == part && x.Trials == 3);

                                if (l != null)
                                {
                                    cell = row.GetCell(part);
                                    if (l.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(l.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(13);

                            for (int part = 1; part <= 10; part++)
                            {
                                var l = grrL.AppraiserList.First(x => x.Appraiser == "B").ValueList.First(x => x.Part == part && x.Trials == 1);

                                if (l != null)
                                {
                                    cell = row.GetCell(part);
                                    if (l.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(l.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(14);

                            appraiser = grrL.AppraiserList.First(x => x.Appraiser == "B");

                            if (appraiser != null)
                            {
                                cell = row.GetCell(0);
                                cell.SetCellValue(appraiser.UserID);

                                for (int part = 1; part <= 10; part++)
                                {
                                    var l = grrL.AppraiserList.First(x => x.Appraiser == "B").ValueList.First(x => x.Part == part && x.Trials == 2);

                                    if (l != null)
                                    {
                                        cell = row.GetCell(part);
                                        if (l.Value.HasValue)
                                        {
                                            cell.SetCellValue(Convert.ToDouble(l.Value));
                                        }
                                    }
                                }
                            }

                            row = sheet.GetRow(15);

                            for (int part = 1; part <= 10; part++)
                            {
                                var l = grrL.AppraiserList.First(x => x.Appraiser == "B").ValueList.First(x => x.Part == part && x.Trials == 3);

                                if (l != null)
                                {
                                    cell = row.GetCell(part);
                                    if (l.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(l.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(18);

                            for (int part = 1; part <= 10; part++)
                            {
                                var l = grrL.AppraiserList.First(x => x.Appraiser == "C").ValueList.First(x => x.Part == part && x.Trials == 1);

                                if (l != null)
                                {
                                    cell = row.GetCell(part);
                                    if (l.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(l.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(19);

                            appraiser = grrL.AppraiserList.First(x => x.Appraiser == "C");

                            if (appraiser != null)
                            {
                                cell = row.GetCell(0);
                                cell.SetCellValue(appraiser.UserID);

                                for (int part = 1; part <= 10; part++)
                                {
                                    var l = grrL.AppraiserList.First(x => x.Appraiser == "C").ValueList.First(x => x.Part == part && x.Trials == 2);

                                    if (l != null)
                                    {
                                        cell = row.GetCell(part);
                                        if (l.Value.HasValue)
                                        {
                                            cell.SetCellValue(Convert.ToDouble(l.Value));
                                        }
                                    }
                                }
                            }

                            row = sheet.GetRow(20);

                            for (int part = 1; part <= 10; part++)
                            {
                                var l = grrL.AppraiserList.First(x => x.Appraiser == "C").ValueList.First(x => x.Part == part && x.Trials == 3);

                                if (l != null)
                                {
                                    cell = row.GetCell(part);
                                    if (l.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(l.Value));
                                    }
                                }
                            }

                            sheet.ForceFormulaRecalculation = true;

                            sheet = workBook.GetSheetAt(8);

                            row = sheet.GetRow(30);
                            cell = row.GetCell(0);
                            appraiser = grrL.AppraiserList.First(x => x.Appraiser == "A");
                            cell.SetCellValue(appraiser.UserID);

                            row = sheet.GetRow(33);
                            cell = row.GetCell(0);
                            appraiser = grrL.AppraiserList.First(x => x.Appraiser == "B");
                            cell.SetCellValue(appraiser.UserID);

                            row = sheet.GetRow(36);
                            cell = row.GetCell(0);
                            appraiser = grrL.AppraiserList.First(x => x.Appraiser == "C");
                            cell.SetCellValue(appraiser.UserID);

                            sheet.ForceFormulaRecalculation = true;
                        }
                        #endregion

                        sheet = workBook.GetSheetAt(9);
                        row = sheet.CreateRow(0);
                        cell = row.CreateCell(0);
                        cell.SetCellValue(model.UniqueID);

                        File.Delete(fileName);

                        using (FileStream file = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
                        {
                            workBook.Write(file);
                            file.Close();
                        }

                        using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                        {
                            workBook = new HSSFWorkbook(file);
                            file.Close();
                        }

                        using (ASEDbEntities db = new ASEDbEntities())
                        {
                            var form = db.QA_MSAFORM.First(x => x.UNIQUEID == model.UniqueID);

                            var evaluator = workBook.GetCreationHelper().CreateFormulaEvaluator();

                            #region Stability
                            sheet = workBook.GetSheetAt(1);

                            var tmpRow = sheet.GetRow(1);

                            var tmpCell = tmpRow.GetCell(29);

                            var cellValue = evaluator.Evaluate(tmpCell);

                            var stabilityValue = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(stabilityValue))
                            {
                                decimal r;

                                if (decimal.TryParse(stabilityValue, out r))
                                {
                                    form.STABILITYVAL = r;
                                }
                                else
                                {
                                    form.STABILITYVAL = null;
                                }
                            }
                            else
                            {
                                form.STABILITYVAL = null;
                            }

                            tmpRow = sheet.GetRow(2);
                            tmpCell = tmpRow.GetCell(29);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var stdev = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(stdev))
                            {
                                decimal r;

                                if (decimal.TryParse(stdev, out r))
                                {
                                    form.STABILITYSTDEV = r;
                                }
                                else
                                {
                                    form.STABILITYSTDEV = null;
                                }
                            }
                            else
                            {
                                form.STABILITYSTDEV = null;
                            }

                            tmpRow = sheet.GetRow(3);
                            tmpCell = tmpRow.GetCell(29);

                            cellValue = evaluator.Evaluate(tmpCell);

                            if (form.STABILITYVAL.HasValue && form.STABILITYSTDEV.HasValue)
                            {
                                var stability = ExcelHelper.GetCellValue(cellValue);

                                if (stability == "PASS")
                                {
                                    form.STABILITYRESULT = "P";
                                }
                                else
                                {
                                    form.STABILITYRESULT = "F";
                                }
                            }
                            else
                            {
                                form.STABILITYRESULT = string.Empty;
                            }
                            #endregion

                            #region BL
                            sheet = workBook.GetSheetAt(2);

                            tmpRow = sheet.GetRow(9);
                            tmpCell = tmpRow.GetCell(7);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var tstatistic = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(tstatistic))
                            {
                                decimal r;

                                if (decimal.TryParse(tstatistic, out r))
                                {
                                    form.BIASTSTATISTIC = r;
                                }
                                else
                                {
                                    form.BIASTSTATISTIC = null;
                                }
                            }
                            else
                            {
                                form.BIASTSTATISTIC = null;
                            }

                            tmpRow = sheet.GetRow(15);
                            tmpCell = tmpRow.GetCell(7);

                            cellValue = evaluator.Evaluate(tmpCell);

                            if (form.BIASTSTATISTIC.HasValue)
                            {
                                var bias = ExcelHelper.GetCellValue(cellValue);

                                if (bias == "PASS")
                                {
                                    form.BIASRESULT = "P";
                                }
                                else
                                {
                                    form.BIASRESULT = "F";
                                }
                            }
                            else
                            {
                                form.BIASRESULT = string.Empty;
                            }

                            tmpRow = sheet.GetRow(26);
                            tmpCell = tmpRow.GetCell(16);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var ta = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(ta))
                            {
                                decimal r;

                                if (decimal.TryParse(ta, out r))
                                {
                                    form.LINEARITYTA = r;
                                }
                                else
                                {
                                    form.LINEARITYTA = null;
                                }
                            }
                            else
                            {
                                form.LINEARITYTA = null;
                            }

                            tmpCell = tmpRow.GetCell(18);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var tb = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(tb))
                            {
                                decimal r;

                                if (decimal.TryParse(tb, out r))
                                {
                                    form.LINEARITYTB = r;
                                }
                                else
                                {
                                    form.LINEARITYTB = null;
                                }
                            }
                            else
                            {
                                form.LINEARITYTB = null;
                            }

                            tmpRow = sheet.GetRow(28);
                            tmpCell = tmpRow.GetCell(16);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var linearityA = ExcelHelper.GetCellValue(cellValue);

                            tmpCell = tmpRow.GetCell(18);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var linearityB = ExcelHelper.GetCellValue(cellValue);

                            if (form.LINEARITYTA.HasValue && form.LINEARITYTB.HasValue)
                            {
                                if (linearityA == "PASS" && linearityB == "PASS")
                                {
                                    form.LINEARITYRESULT = "P";
                                }
                                else
                                {
                                    form.LINEARITYRESULT = "F";
                                }
                            }
                            else
                            {
                                form.LINEARITYRESULT = string.Empty;
                            }
                            #endregion

                            #region GRR H
                            sheet = workBook.GetSheetAt(3);

                            tmpRow = sheet.GetRow(40);
                            tmpCell = tmpRow.GetCell(10);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrh = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(grrh))
                            {
                                decimal r;

                                if (decimal.TryParse(grrh, out r))
                                {
                                    form.GRRH = r;
                                }
                                else
                                {
                                    form.GRRH = null;
                                }
                            }
                            else
                            {
                                form.GRRH = null;
                            }

                            tmpRow = sheet.GetRow(48);
                            tmpCell = tmpRow.GetCell(10);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrhndc = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(grrhndc))
                            {
                                decimal r;

                                if (decimal.TryParse(grrhndc, out r))
                                {
                                    form.GRRHNDC = r;
                                }
                                else
                                {
                                    form.GRRHNDC = null;
                                }
                            }
                            else
                            {
                                form.GRRHNDC = null;
                            }

                            tmpRow = sheet.GetRow(47);
                            tmpCell = tmpRow.GetCell(12);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrhPass = ExcelHelper.GetCellValue(cellValue);

                            tmpRow = sheet.GetRow(48);
                            tmpCell = tmpRow.GetCell(12);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrhImprove = ExcelHelper.GetCellValue(cellValue);

                            tmpRow = sheet.GetRow(49);
                            tmpCell = tmpRow.GetCell(12);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrhReject = ExcelHelper.GetCellValue(cellValue);

                            if (grrhPass == "V")
                            {
                                form.GRRHRESULT = "P";
                            }
                            else if (grrhImprove == "V")
                            {
                                form.GRRHRESULT = "I";
                            }
                            else if (grrhReject == "V")
                            {
                                form.GRRHRESULT = "R";
                            }
                            else
                            {
                                form.GRRHRESULT = "";
                            }
                            #endregion

                            #region GRR M
                            sheet = workBook.GetSheetAt(5);

                            tmpRow = sheet.GetRow(40);
                            tmpCell = tmpRow.GetCell(10);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrm = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(grrm))
                            {
                                decimal r;

                                if (decimal.TryParse(grrm, out r))
                                {
                                    form.GRRM = r;
                                }
                                else
                                {
                                    form.GRRM = null;
                                }
                            }
                            else
                            {
                                form.GRRM = null;
                            }

                            tmpRow = sheet.GetRow(48);
                            tmpCell = tmpRow.GetCell(10);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrmndc = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(grrmndc))
                            {
                                decimal r;

                                if (decimal.TryParse(grrmndc, out r))
                                {
                                    form.GRRMNDC = r;
                                }
                                else
                                {
                                    form.GRRMNDC = null;
                                }
                            }
                            else
                            {
                                form.GRRMNDC = null;
                            }

                            tmpRow = sheet.GetRow(47);
                            tmpCell = tmpRow.GetCell(12);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrmPass = ExcelHelper.GetCellValue(cellValue);

                            tmpRow = sheet.GetRow(48);
                            tmpCell = tmpRow.GetCell(12);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrmImprove = ExcelHelper.GetCellValue(cellValue);

                            tmpRow = sheet.GetRow(49);
                            tmpCell = tmpRow.GetCell(12);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrmReject = ExcelHelper.GetCellValue(cellValue);

                            if (grrmPass == "V")
                            {
                                form.GRRMRESULT = "P";
                            }
                            else if (grrmImprove == "V")
                            {
                                form.GRRMRESULT = "I";
                            }
                            else if (grrmReject == "V")
                            {
                                form.GRRMRESULT = "R";
                            }
                            else
                            {
                                form.GRRMRESULT = "";
                            }
                            #endregion

                            #region GRR L
                            sheet = workBook.GetSheetAt(7);

                            tmpRow = sheet.GetRow(40);
                            tmpCell = tmpRow.GetCell(10);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrl = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(grrl))
                            {
                                decimal r;

                                if (decimal.TryParse(grrl, out r))
                                {
                                    form.GRRL = r;
                                }
                                else
                                {
                                    form.GRRL = null;
                                }
                            }
                            else
                            {
                                form.GRRL = null;
                            }

                            tmpRow = sheet.GetRow(48);
                            tmpCell = tmpRow.GetCell(10);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrlndc = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(grrlndc))
                            {
                                decimal r;

                                if (decimal.TryParse(grrlndc, out r))
                                {
                                    form.GRRLNDC = r;
                                }
                                else
                                {
                                    form.GRRLNDC = null;
                                }
                            }
                            else
                            {
                                form.GRRLNDC = null;
                            }

                            tmpRow = sheet.GetRow(47);
                            tmpCell = tmpRow.GetCell(12);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrlPass = ExcelHelper.GetCellValue(cellValue);

                            tmpRow = sheet.GetRow(48);
                            tmpCell = tmpRow.GetCell(12);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrlImprove = ExcelHelper.GetCellValue(cellValue);

                            tmpRow = sheet.GetRow(49);
                            tmpCell = tmpRow.GetCell(12);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrlReject = ExcelHelper.GetCellValue(cellValue);

                            if (grrlPass == "V")
                            {
                                form.GRRLRESULT = "P";
                            }
                            else if (grrlImprove == "V")
                            {
                                form.GRRLRESULT = "I";
                            }
                            else if (grrlReject == "V")
                            {
                                form.GRRLRESULT = "R";
                            }
                            else
                            {
                                form.GRRLRESULT = "";
                            }
                            #endregion

                            db.SaveChanges();
                        }
                    }

                    if (model.SubType == "2")
                    {
                        File.Copy(Config.ReportTemplate_MSA_Anova, fileName);

                        using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                        {
                            workBook = new HSSFWorkbook(file);
                            file.Close();
                        }

                        #region 封面
                        var sheet = workBook.GetSheetAt(0);

                        var row = sheet.GetRow(3);
                        var cell = row.GetCell(5);

                        cell.SetCellValue(model.MSAIchi);

                        row = sheet.GetRow(4);
                        cell = row.GetCell(5);

                        cell.SetCellValue(model.Equipment.MSACalNo);

                        row = sheet.GetRow(5);
                        cell = row.GetCell(5);

                        cell.SetCellValue(model.Equipment.Brand);

                        row = sheet.GetRow(6);
                        cell = row.GetCell(5);

                        cell.SetCellValue(model.Equipment.Model);

                        row = sheet.GetRow(7);
                        cell = row.GetCell(5);

                        cell.SetCellValue(model.Station);

                        row = sheet.GetRow(8);
                        cell = row.GetCell(5);

                        cell.SetCellValue(model.MSACharacteristic);

                        row = sheet.GetRow(9);
                        cell = row.GetCell(5);

                        cell.SetCellValue(model.MSARange);

                        row = sheet.GetRow(10);
                        cell = row.GetCell(5);

                        cell.SetCellValue(model.Unit);

                        row = sheet.GetRow(11);
                        cell = row.GetCell(5);

                        cell.SetCellValue(model.MSAResponsor);

                        row = sheet.GetRow(12);
                        cell = row.GetCell(5);

                        cell.SetCellValue(model.MSADateString);

                        sheet.ForceFormulaRecalculation = true;
                        #endregion

                        #region Stability
                        sheet = workBook.GetSheetAt(1);

                        row = sheet.GetRow(6);

                        cell = row.GetCell(0);
                        cell.SetCellValue(model.Equipment.MSACalNo);

                        cell = row.GetCell(4);
                        cell.SetCellValue(model.MSAIchi);

                        cell = row.GetCell(7);
                        cell.SetCellValue(model.Equipment.Model);

                        cell = row.GetCell(10);
                        cell.SetCellValue(model.Station);

                        cell = row.GetCell(12);
                        cell.SetCellValue("5次/DAY");

                        cell = row.GetCell(15);
                        cell.SetCellValue(model.Unit);

                        cell = row.GetCell(17);
                        cell.SetCellValue(model.StabilityLowerRange);

                        cell = row.GetCell(20);
                        cell.SetCellValue(model.StabilityUpperRange);

                        cell = row.GetCell(23);
                        cell.SetCellValue(model.StabilityPrepareDateString);

                        cell = row.GetCell(26);

                        var min = model.StabilityList.Where(x => x.Date.HasValue).OrderBy(x => x.Date).FirstOrDefault();
                        var max = model.StabilityList.Where(x => x.Date.HasValue).OrderByDescending(x => x.Date).FirstOrDefault();

                        var date = string.Empty;

                        if (min != null && max != null)
                        {
                            date = string.Format("{0}~{1}", DateTimeHelper.DateTime2DateString(min.Date), DateTimeHelper.DateTime2DateString(max.Date));
                        }
                        else if (min != null && max == null)
                        {
                            date = DateTimeHelper.DateTime2DateString(min.Date);
                        }
                        else if (min == null && max != null)
                        {
                            date = DateTimeHelper.DateTime2DateString(max.Date);
                        }

                        cell.SetCellValue(date);

                        cell = row.GetCell(29);
                        cell.SetCellValue(model.MSAResponsor);

                        var cellIndex = 0;

                        foreach (var stability in model.StabilityList.OrderBy(x => x.Point))
                        {
                            cellIndex = Convert.ToInt32(stability.Point) + 1;

                            row = sheet.GetRow(22);
                            cell = row.GetCell(cellIndex);

                            var stablityValue = stability.ValueList.FirstOrDefault(x => x.Data == 1);

                            if (stablityValue != null && stablityValue.Value.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(stablityValue.Value));
                            }

                            row = sheet.GetRow(23);
                            cell = row.GetCell(cellIndex);

                            stablityValue = stability.ValueList.FirstOrDefault(x => x.Data == 2);

                            if (stablityValue != null && stablityValue.Value.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(stablityValue.Value));
                            }

                            row = sheet.GetRow(24);
                            cell = row.GetCell(cellIndex);

                            stablityValue = stability.ValueList.FirstOrDefault(x => x.Data == 3);

                            if (stablityValue != null && stablityValue.Value.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(stablityValue.Value));
                            }

                            row = sheet.GetRow(25);
                            cell = row.GetCell(cellIndex);

                            stablityValue = stability.ValueList.FirstOrDefault(x => x.Data == 4);

                            if (stablityValue != null && stablityValue.Value.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(stablityValue.Value));
                            }

                            row = sheet.GetRow(26);
                            cell = row.GetCell(cellIndex);

                            stablityValue = stability.ValueList.FirstOrDefault(x => x.Data == 5);

                            if (stablityValue != null && stablityValue.Value.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(stablityValue.Value));
                            }

                            row = sheet.GetRow(29);
                            cell = row.GetCell(cellIndex);

                            if (stability.Date.HasValue)
                            {
                                cell.SetCellValue(stability.Date.Value);
                            }
                        }

                        sheet.ForceFormulaRecalculation = true;
                        #endregion

                        #region BL
                        sheet = workBook.GetSheetAt(2);

                        row = sheet.GetRow(0);

                        cell = row.GetCell(12);

                        cell.SetCellValue(model.MSAResponsor);

                        cell = row.GetCell(18);

                        if (model.BLDate.HasValue)
                        {
                            cell.SetCellValue(model.BLDate.Value);
                        }

                        row = sheet.GetRow(3);

                        cell = row.GetCell(0);

                        cell.SetCellValue(model.MSAIchi);

                        cell = row.GetCell(6);

                        cell.SetCellValue(model.Equipment.MSACalNo);

                        cell = row.GetCell(12);

                        cell.SetCellValue(model.BLPackageType);

                        cell = row.GetCell(17);

                        cell.SetCellValue(model.MSACharacteristic);

                        if (model.BiasReferenceValue.HasValue)
                        {
                            row = sheet.GetRow(7);

                            cell = row.GetCell(3);

                            cell.SetCellValue(Convert.ToDouble(model.BiasReferenceValue.Value));
                        }

                        var rowIndex = 0;

                        foreach (var bias in model.BiasList.OrderBy(x => x.Trials))
                        {
                            rowIndex = 9 + Convert.ToInt32(bias.Trials) - 1;

                            row = sheet.GetRow(rowIndex);

                            cell = row.GetCell(2);

                            if (bias.Value.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(bias.Value.Value));
                            }
                        }

                        sheet.ForceFormulaRecalculation = true;

                        cellIndex = 0;

                        foreach (var linearity in model.LinearityList.OrderBy(x => x.Trials))
                        {
                            cellIndex = 10 + Convert.ToInt32(linearity.Trials) - 1;

                            if (linearity.ReferenceValue.HasValue)
                            {
                                row = sheet.GetRow(7);
                                cell = row.GetCell(cellIndex);
                                cell.SetCellValue(Convert.ToDouble(linearity.ReferenceValue.Value));
                            }

                            row = sheet.GetRow(9);
                            cell = row.GetCell(cellIndex);

                            var linearityValue = linearity.ValueList.FirstOrDefault(x => x.Data == 1);

                            if (linearityValue != null && linearityValue.Value.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(linearityValue.Value));
                            }

                            row = sheet.GetRow(10);
                            cell = row.GetCell(cellIndex);

                            linearityValue = linearity.ValueList.FirstOrDefault(x => x.Data == 2);

                            if (linearityValue != null && linearityValue.Value.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(linearityValue.Value));
                            }

                            row = sheet.GetRow(11);
                            cell = row.GetCell(cellIndex);

                            linearityValue = linearity.ValueList.FirstOrDefault(x => x.Data == 3);

                            if (linearityValue != null && linearityValue.Value.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(linearityValue.Value));
                            }

                            row = sheet.GetRow(12);
                            cell = row.GetCell(cellIndex);

                            linearityValue = linearity.ValueList.FirstOrDefault(x => x.Data == 4);

                            if (linearityValue != null && linearityValue.Value.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(linearityValue.Value));
                            }

                            row = sheet.GetRow(13);
                            cell = row.GetCell(cellIndex);

                            linearityValue = linearity.ValueList.FirstOrDefault(x => x.Data == 5);

                            if (linearityValue != null && linearityValue.Value.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(linearityValue.Value));
                            }

                            row = sheet.GetRow(14);
                            cell = row.GetCell(cellIndex);

                            linearityValue = linearity.ValueList.FirstOrDefault(x => x.Data == 6);

                            if (linearityValue != null && linearityValue.Value.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(linearityValue.Value));
                            }

                            row = sheet.GetRow(15);
                            cell = row.GetCell(cellIndex);

                            linearityValue = linearity.ValueList.FirstOrDefault(x => x.Data == 7);

                            if (linearityValue != null && linearityValue.Value.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(linearityValue.Value));
                            }

                            row = sheet.GetRow(16);
                            cell = row.GetCell(cellIndex);

                            linearityValue = linearity.ValueList.FirstOrDefault(x => x.Data == 8);

                            if (linearityValue != null && linearityValue.Value.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(linearityValue.Value));
                            }

                            row = sheet.GetRow(17);
                            cell = row.GetCell(cellIndex);

                            linearityValue = linearity.ValueList.FirstOrDefault(x => x.Data == 9);

                            if (linearityValue != null && linearityValue.Value.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(linearityValue.Value));
                            }

                            row = sheet.GetRow(18);
                            cell = row.GetCell(cellIndex);

                            linearityValue = linearity.ValueList.FirstOrDefault(x => x.Data == 10);

                            if (linearityValue != null && linearityValue.Value.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(linearityValue.Value));
                            }

                            row = sheet.GetRow(19);
                            cell = row.GetCell(cellIndex);

                            linearityValue = linearity.ValueList.FirstOrDefault(x => x.Data == 11);

                            if (linearityValue != null && linearityValue.Value.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(linearityValue.Value));
                            }

                            row = sheet.GetRow(20);
                            cell = row.GetCell(cellIndex);

                            linearityValue = linearity.ValueList.FirstOrDefault(x => x.Data == 12);

                            if (linearityValue != null && linearityValue.Value.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(linearityValue.Value));
                            }
                        }

                        sheet.ForceFormulaRecalculation = true;
                        #endregion

                        #region GRRH
                        var grrH = model.GRRList.First(x => x.Level == "H");

                        if (grrH != null)
                        {
                            sheet = workBook.GetSheetAt(3);

                            row = sheet.GetRow(0);

                            cell = row.GetCell(7);
                            cell.SetCellValue(model.MSAResponsor);

                            cell = row.GetCell(10);
                            cell.SetCellValue(grrH.DateString);

                            row = sheet.GetRow(1);

                            if (model.GRRHTvUsed == "1")
                            {
                                cell = row.GetCell(11);
                                cell.SetCellValue("V");
                            }

                            row = sheet.GetRow(2);

                            cell = row.GetCell(0);
                            cell.SetCellValue(model.Equipment.MSACalNo);

                            cell = row.GetCell(3);
                            cell.SetCellValue(model.GRRHPackageType);

                            cell = row.GetCell(6);
                            cell.SetCellValue(model.MSACharacteristic);

                            cell = row.GetCell(10);

                            if (model.GRRHUSL.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(model.GRRHUSL));
                            }

                            if (model.GRRHTvUsed == "2")
                            {
                                cell = row.GetCell(11);
                                cell.SetCellValue("V");
                            }

                            row = sheet.GetRow(3);

                            cell = row.GetCell(10);
                            if (model.GRRHCL.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(model.GRRHCL));
                            }

                            if (model.GRRHTvUsed == "3")
                            {
                                cell = row.GetCell(11);
                                cell.SetCellValue("V");
                            }

                            row = sheet.GetRow(4);

                            cell = row.GetCell(10);
                            if (model.GRRHLSL.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(model.GRRHLSL));
                            }

                            if (model.GRRHTvUsed == "4")
                            {
                                cell = row.GetCell(11);
                                cell.SetCellValue("V");
                            }

                            row = sheet.GetRow(8);

                            for (int part = 1; part <= 10; part++)
                            {
                                var h = grrH.AppraiserList.First(x => x.Appraiser == "A").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 1);

                                if (h != null)
                                {
                                    cell = row.GetCell(part);
                                    if (h.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(h.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(9);

                            var appraiser = grrH.AppraiserList.First(x => x.Appraiser == "A");

                            if (appraiser != null)
                            {
                                cell = row.GetCell(0);
                                cell.SetCellValue(appraiser.UserID);

                                for (int part = 1; part <= 10; part++)
                                {
                                    var h = grrH.AppraiserList.First(x => x.Appraiser == "A").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 2);

                                    if (h != null)
                                    {
                                        cell = row.GetCell(part);
                                        if (h.Value.HasValue)
                                        {
                                            cell.SetCellValue(Convert.ToDouble(h.Value));
                                        }
                                    }
                                }
                            }

                            row = sheet.GetRow(10);

                            for (int part = 1; part <= 10; part++)
                            {
                                var h = grrH.AppraiserList.First(x => x.Appraiser == "A").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 3);

                                if (h != null)
                                {
                                    cell = row.GetCell(part);
                                    if (h.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(h.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(13);

                            for (int part = 1; part <= 10; part++)
                            {
                                var h = grrH.AppraiserList.First(x => x.Appraiser == "B").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 1);

                                if (h != null)
                                {
                                    cell = row.GetCell(part);
                                    if (h.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(h.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(14);

                            appraiser = grrH.AppraiserList.FirstOrDefault(x => x.Appraiser == "B");

                            if (appraiser != null)
                            {
                                cell = row.GetCell(0);
                                cell.SetCellValue(appraiser.UserID);

                                for (int part = 1; part <= 10; part++)
                                {
                                    var h = grrH.AppraiserList.First(x => x.Appraiser == "B").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 2);

                                    if (h != null)
                                    {
                                        cell = row.GetCell(part);
                                        if (h.Value.HasValue)
                                        {
                                            cell.SetCellValue(Convert.ToDouble(h.Value));
                                        }
                                    }
                                }
                            }

                            row = sheet.GetRow(15);

                            for (int part = 1; part <= 10; part++)
                            {
                                var h = grrH.AppraiserList.First(x => x.Appraiser == "B").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 3);

                                if (h != null)
                                {
                                    cell = row.GetCell(part);
                                    if (h.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(h.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(18);

                            for (int part = 1; part <= 10; part++)
                            {
                                var h = grrH.AppraiserList.First(x => x.Appraiser == "C").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 1);

                                if (h != null)
                                {
                                    cell = row.GetCell(part);
                                    if (h.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(h.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(19);

                            appraiser = grrH.AppraiserList.FirstOrDefault(x => x.Appraiser == "C");

                            if (appraiser != null)
                            {
                                cell = row.GetCell(0);
                                cell.SetCellValue(appraiser.UserID);

                                for (int part = 1; part <= 10; part++)
                                {
                                    var h = grrH.AppraiserList.First(x => x.Appraiser == "C").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 2);

                                    if (h != null)
                                    {
                                        cell = row.GetCell(part);
                                        if (h.Value.HasValue)
                                        {
                                            cell.SetCellValue(Convert.ToDouble(h.Value));
                                        }
                                    }
                                }
                            }

                            row = sheet.GetRow(20);

                            for (int part = 1; part <= 10; part++)
                            {
                                var h = grrH.AppraiserList.First(x => x.Appraiser == "C").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 3);

                                if (h != null)
                                {
                                    cell = row.GetCell(part);
                                    if (h.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(h.Value));
                                    }
                                }
                            }

                            sheet.ForceFormulaRecalculation = true;

                            sheet = workBook.GetSheetAt(4);

                            row = sheet.GetRow(30);
                            cell = row.GetCell(0);
                            appraiser = grrH.AppraiserList.First(x => x.Appraiser == "A");
                            cell.SetCellValue(appraiser.UserID);

                            row = sheet.GetRow(33);
                            cell = row.GetCell(0);
                            appraiser = grrH.AppraiserList.First(x => x.Appraiser == "B");
                            cell.SetCellValue(appraiser.UserID);

                            row = sheet.GetRow(36);
                            cell = row.GetCell(0);
                            appraiser = grrH.AppraiserList.First(x => x.Appraiser == "C");
                            cell.SetCellValue(appraiser.UserID);

                            sheet.ForceFormulaRecalculation = true;
                        }
                        #endregion

                        #region AnovaH
                        sheet = workBook.GetSheetAt(5);

                        double[][][] anovaHData = new double[3][][];

                        int a = 0;

                        foreach (var appraiser in grrH.AppraiserList)
                        {
                            anovaHData[a] = new double[10][];

                            for (int part = 1; part <= 10; part++)
                            {
                                var trials1 = appraiser.ValueList.First(x => x.Part == part && x.Trials == 1).Value;
                                var trials2 = appraiser.ValueList.First(x => x.Part == part && x.Trials == 2).Value;
                                var trials3 = appraiser.ValueList.First(x => x.Part == part && x.Trials == 3).Value;

                                anovaHData[a][part - 1] = new double[] 
                            { 
                                trials1.HasValue?Convert.ToDouble(trials1.Value):0,
                                trials2.HasValue?Convert.ToDouble(trials2.Value):0,
                                trials3.HasValue?Convert.ToDouble(trials3.Value):0
                            };
                            }

                            a++;
                        }

                        var anovaH = new TwoWayAnova(anovaHData, TwoWayAnovaModel.Mixed);

                        var factorA = anovaH.Table.FirstOrDefault(x => x.Source == "Factor A");
                        var factorB = anovaH.Table.FirstOrDefault(x => x.Source == "Factor B");
                        var interaction = anovaH.Table.FirstOrDefault(x => x.Source == "Interaction AxB");
                        var group = anovaH.Table.FirstOrDefault(x => x.Source == "Within-cells (error)");
                        var total = anovaH.Table.FirstOrDefault(x => x.Source == "Total");

                        row = sheet.GetRow(2);

                        cell = row.GetCell(1);
                        cell.SetCellValue(factorA.SumOfSquares);
                        cell = row.GetCell(2);
                        cell.SetCellValue(factorA.DegreesOfFreedom);
                        cell = row.GetCell(3);
                        cell.SetCellValue(factorA.MeanSquares);
                        cell = row.GetCell(4);
                        if (factorA.Statistic.HasValue)
                        {
                            cell.SetCellValue(factorA.Statistic.Value);
                        }
                        cell = row.GetCell(5);
                        cell.SetCellValue(factorA.Significance.PValue);

                        //cell = row.GetCell(9);
                        //if (model.FormInput.GRRHUSL.HasValue && model.FormInput.GRRHLSL.HasValue)
                        //{
                        //    cell.SetCellValue(Convert.ToDouble(model.FormInput.GRRHUSL.Value - model.FormInput.GRRHLSL.Value));
                        //}

                        row = sheet.GetRow(3);

                        cell = row.GetCell(1);
                        cell.SetCellValue(factorB.SumOfSquares);
                        cell = row.GetCell(2);
                        cell.SetCellValue(factorB.DegreesOfFreedom);
                        cell = row.GetCell(3);
                        cell.SetCellValue(factorB.MeanSquares);
                        cell = row.GetCell(4);
                        if (factorB.Statistic.HasValue)
                        {
                            cell.SetCellValue(factorB.Statistic.Value);
                        }
                        cell = row.GetCell(5);
                        cell.SetCellValue(factorB.Significance.PValue);

                        row = sheet.GetRow(4);

                        cell = row.GetCell(1);
                        cell.SetCellValue(interaction.SumOfSquares);
                        cell = row.GetCell(2);
                        cell.SetCellValue(interaction.DegreesOfFreedom);
                        cell = row.GetCell(3);
                        cell.SetCellValue(interaction.MeanSquares);
                        cell = row.GetCell(4);
                        if (interaction.Statistic.HasValue)
                        {
                            cell.SetCellValue(interaction.Statistic.Value);
                        }
                        cell = row.GetCell(5);
                        cell.SetCellValue(interaction.Significance.PValue);

                        row = sheet.GetRow(5);

                        cell = row.GetCell(1);
                        cell.SetCellValue(group.SumOfSquares);
                        cell = row.GetCell(2);
                        cell.SetCellValue(group.DegreesOfFreedom);
                        cell = row.GetCell(3);
                        cell.SetCellValue(group.MeanSquares);

                        row = sheet.GetRow(6);

                        cell = row.GetCell(1);
                        cell.SetCellValue(total.SumOfSquares);
                        cell = row.GetCell(2);
                        cell.SetCellValue(total.DegreesOfFreedom);

                        row = sheet.GetRow(12);
                        cell = row.GetCell(2);
                        if (model.GRRHUSL.HasValue)
                        {
                            cell.SetCellValue(Convert.ToDouble(model.GRRHUSL.Value));
                        }

                        row = sheet.GetRow(13);
                        cell = row.GetCell(2);
                        if (model.GRRHCL.HasValue)
                        {
                            cell.SetCellValue(Convert.ToDouble(model.GRRHCL.Value));
                        }

                        row = sheet.GetRow(14);
                        cell = row.GetCell(2);
                        if (model.GRRHLSL.HasValue)
                        {
                            cell.SetCellValue(Convert.ToDouble(model.GRRHLSL.Value));
                        }

                        row = sheet.GetRow(15);
                        cell = row.GetCell(2);
                        if (grrH.Average.HasValue)
                        {
                            cell.SetCellValue(Convert.ToDouble(grrH.Average.Value));
                        }

                        sheet.ForceFormulaRecalculation = true;
                        #endregion

                        #region GRRM
                        var grrM = model.GRRList.First(x => x.Level == "M");

                        if (grrM != null)
                        {
                            sheet = workBook.GetSheetAt(6);

                            row = sheet.GetRow(0);

                            cell = row.GetCell(7);
                            cell.SetCellValue(model.MSAResponsor);

                            cell = row.GetCell(10);
                            cell.SetCellValue(grrM.DateString);

                            row = sheet.GetRow(1);

                            if (model.GRRMTvUsed == "1")
                            {
                                cell = row.GetCell(11);
                                cell.SetCellValue("V");
                            }

                            row = sheet.GetRow(2);

                            cell = row.GetCell(0);
                            cell.SetCellValue(model.Equipment.MSACalNo);

                            cell = row.GetCell(3);
                            cell.SetCellValue(model.GRRMPackageType);

                            cell = row.GetCell(6);
                            cell.SetCellValue(model.MSACharacteristic);

                            cell = row.GetCell(10);
                            if (model.GRRMUSL.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(model.GRRMUSL));
                            }

                            if (model.GRRMTvUsed == "2")
                            {
                                cell = row.GetCell(11);
                                cell.SetCellValue("V");
                            }

                            row = sheet.GetRow(3);

                            cell = row.GetCell(10);
                            if (model.GRRMCL.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(model.GRRMCL));
                            }

                            if (model.GRRMTvUsed == "3")
                            {
                                cell = row.GetCell(11);
                                cell.SetCellValue("V");
                            }

                            row = sheet.GetRow(4);

                            cell = row.GetCell(10);
                            if (model.GRRMLSL.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(model.GRRMLSL));
                            }

                            if (model.GRRMTvUsed == "4")
                            {
                                cell = row.GetCell(11);
                                cell.SetCellValue("V");
                            }

                            row = sheet.GetRow(8);

                            for (int part = 1; part <= 10; part++)
                            {
                                var m = grrM.AppraiserList.First(x => x.Appraiser == "A").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 1);

                                if (m != null)
                                {
                                    cell = row.GetCell(part);
                                    if (m.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(m.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(9);

                            var appraiser = grrM.AppraiserList.First(x => x.Appraiser == "A");

                            if (appraiser != null)
                            {
                                cell = row.GetCell(0);
                                cell.SetCellValue(appraiser.UserID);

                                for (int part = 1; part <= 10; part++)
                                {
                                    var m = grrM.AppraiserList.First(x => x.Appraiser == "A").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 2);

                                    if (m != null)
                                    {
                                        cell = row.GetCell(part);
                                        if (m.Value.HasValue)
                                        {
                                            cell.SetCellValue(Convert.ToDouble(m.Value));
                                        }
                                    }
                                }
                            }

                            row = sheet.GetRow(10);

                            for (int part = 1; part <= 10; part++)
                            {
                                var m = grrM.AppraiserList.First(x => x.Appraiser == "A").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 3);

                                if (m != null)
                                {
                                    cell = row.GetCell(part);
                                    if (m.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(m.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(13);

                            for (int part = 1; part <= 10; part++)
                            {
                                var m = grrM.AppraiserList.First(x => x.Appraiser == "B").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 1);

                                if (m != null)
                                {
                                    cell = row.GetCell(part);
                                    if (m.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(m.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(14);

                            appraiser = grrM.AppraiserList.First(x => x.Appraiser == "B");

                            if (appraiser != null)
                            {
                                cell = row.GetCell(0);
                                cell.SetCellValue(appraiser.UserID);

                                for (int part = 1; part <= 10; part++)
                                {
                                    var m = grrM.AppraiserList.First(x => x.Appraiser == "B").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 2);

                                    if (m != null)
                                    {
                                        cell = row.GetCell(part);
                                        if (m.Value.HasValue)
                                        {
                                            cell.SetCellValue(Convert.ToDouble(m.Value));
                                        }
                                    }
                                }
                            }

                            row = sheet.GetRow(15);

                            for (int part = 1; part <= 10; part++)
                            {
                                var m = grrM.AppraiserList.First(x => x.Appraiser == "B").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 3);

                                if (m != null)
                                {
                                    cell = row.GetCell(part);

                                    if (m.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(m.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(18);

                            for (int part = 1; part <= 10; part++)
                            {
                                var m = grrM.AppraiserList.First(x => x.Appraiser == "C").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 1);

                                if (m != null)
                                {
                                    cell = row.GetCell(part);
                                    if (m.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(m.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(19);

                            appraiser = grrM.AppraiserList.First(x => x.Appraiser == "C");

                            if (appraiser != null)
                            {
                                cell = row.GetCell(0);
                                cell.SetCellValue(appraiser.UserID);

                                for (int part = 1; part <= 10; part++)
                                {
                                    var m = grrM.AppraiserList.First(x => x.Appraiser == "C").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 2);

                                    if (m != null)
                                    {
                                        cell = row.GetCell(part);
                                        if (m.Value.HasValue)
                                        {
                                            cell.SetCellValue(Convert.ToDouble(m.Value));
                                        }
                                    }
                                }
                            }

                            row = sheet.GetRow(20);

                            for (int part = 1; part <= 10; part++)
                            {
                                var m = grrM.AppraiserList.First(x => x.Appraiser == "C").ValueList.FirstOrDefault(x => x.Part == part && x.Trials == 3);

                                if (m != null)
                                {
                                    cell = row.GetCell(part);
                                    if (m.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(m.Value));
                                    }
                                }
                            }

                            sheet.ForceFormulaRecalculation = true;

                            sheet = workBook.GetSheetAt(7);

                            row = sheet.GetRow(30);
                            cell = row.GetCell(0);
                            appraiser = grrM.AppraiserList.First(x => x.Appraiser == "A");
                            cell.SetCellValue(appraiser.UserID);

                            row = sheet.GetRow(33);
                            cell = row.GetCell(0);
                            appraiser = grrM.AppraiserList.First(x => x.Appraiser == "B");
                            cell.SetCellValue(appraiser.UserID);

                            row = sheet.GetRow(36);
                            cell = row.GetCell(0);
                            appraiser = grrM.AppraiserList.First(x => x.Appraiser == "C");
                            cell.SetCellValue(appraiser.UserID);

                            sheet.ForceFormulaRecalculation = true;
                        }
                        #endregion

                        #region Anova M
                        sheet = workBook.GetSheetAt(8);

                        double[][][] anovaMData = new double[3][][];

                        a = 0;

                        foreach (var appraiser in grrM.AppraiserList)
                        {
                            anovaMData[a] = new double[10][];

                            for (int part = 1; part <= 10; part++)
                            {
                                var trials1 = appraiser.ValueList.First(x => x.Part == part && x.Trials == 1).Value;
                                var trials2 = appraiser.ValueList.First(x => x.Part == part && x.Trials == 2).Value;
                                var trials3 = appraiser.ValueList.First(x => x.Part == part && x.Trials == 3).Value;

                                anovaMData[a][part - 1] = new double[] 
                            { 
                                trials1.HasValue?Convert.ToDouble(trials1.Value):0,
                                trials2.HasValue?Convert.ToDouble(trials2.Value):0,
                                trials3.HasValue?Convert.ToDouble(trials3.Value):0
                            };
                            }

                            a++;
                        }

                        var anovaM = new TwoWayAnova(anovaMData, TwoWayAnovaModel.Mixed);

                        factorA = anovaM.Table.FirstOrDefault(x => x.Source == "Factor A");
                        factorB = anovaM.Table.FirstOrDefault(x => x.Source == "Factor B");
                        interaction = anovaM.Table.FirstOrDefault(x => x.Source == "Interaction AxB");
                        group = anovaM.Table.FirstOrDefault(x => x.Source == "Within-cells (error)");
                        total = anovaM.Table.FirstOrDefault(x => x.Source == "Total");

                        row = sheet.GetRow(2);

                        cell = row.GetCell(1);
                        cell.SetCellValue(factorA.SumOfSquares);
                        cell = row.GetCell(2);
                        cell.SetCellValue(factorA.DegreesOfFreedom);
                        cell = row.GetCell(3);
                        cell.SetCellValue(factorA.MeanSquares);
                        cell = row.GetCell(4);
                        if (factorA.Statistic.HasValue)
                        {
                            cell.SetCellValue(factorA.Statistic.Value);
                        }
                        cell = row.GetCell(5);
                        cell.SetCellValue(factorA.Significance.PValue);

                        cell = row.GetCell(9);
                        if (model.GRRMUSL.HasValue && model.GRRMLSL.HasValue)
                        {
                            cell.SetCellValue(Convert.ToDouble(model.GRRMUSL.Value - model.GRRMLSL.Value));
                        }

                        row = sheet.GetRow(3);

                        cell = row.GetCell(1);
                        cell.SetCellValue(factorB.SumOfSquares);
                        cell = row.GetCell(2);
                        cell.SetCellValue(factorB.DegreesOfFreedom);
                        cell = row.GetCell(3);
                        cell.SetCellValue(factorB.MeanSquares);
                        cell = row.GetCell(4);
                        if (factorB.Statistic.HasValue)
                        {
                            cell.SetCellValue(factorB.Statistic.Value);
                        }
                        cell = row.GetCell(5);
                        cell.SetCellValue(factorB.Significance.PValue);

                        row = sheet.GetRow(4);

                        cell = row.GetCell(1);
                        cell.SetCellValue(interaction.SumOfSquares);
                        cell = row.GetCell(2);
                        cell.SetCellValue(interaction.DegreesOfFreedom);
                        cell = row.GetCell(3);
                        cell.SetCellValue(interaction.MeanSquares);
                        cell = row.GetCell(4);
                        if (interaction.Statistic.HasValue)
                        {
                            cell.SetCellValue(interaction.Statistic.Value);
                        }
                        cell = row.GetCell(5);
                        cell.SetCellValue(interaction.Significance.PValue);

                        row = sheet.GetRow(5);

                        cell = row.GetCell(1);
                        cell.SetCellValue(group.SumOfSquares);
                        cell = row.GetCell(2);
                        cell.SetCellValue(group.DegreesOfFreedom);
                        cell = row.GetCell(3);
                        cell.SetCellValue(group.MeanSquares);

                        row = sheet.GetRow(6);

                        cell = row.GetCell(1);
                        cell.SetCellValue(total.SumOfSquares);
                        cell = row.GetCell(2);
                        cell.SetCellValue(total.DegreesOfFreedom);

                        row = sheet.GetRow(12);
                        cell = row.GetCell(2);
                        if (model.GRRMUSL.HasValue)
                        {
                            cell.SetCellValue(Convert.ToDouble(model.GRRMUSL.Value));
                        }

                        row = sheet.GetRow(13);
                        cell = row.GetCell(2);
                        if (model.GRRMCL.HasValue)
                        {
                            cell.SetCellValue(Convert.ToDouble(model.GRRMCL.Value));
                        }

                        row = sheet.GetRow(14);
                        cell = row.GetCell(2);
                        if (model.GRRMLSL.HasValue)
                        {
                            cell.SetCellValue(Convert.ToDouble(model.GRRMLSL.Value));
                        }

                        row = sheet.GetRow(15);
                        cell = row.GetCell(2);
                        if (grrM.Average.HasValue)
                        {
                            cell.SetCellValue(Convert.ToDouble(grrM.Average.Value));
                        }

                        sheet.ForceFormulaRecalculation = true;
                        #endregion

                        #region GRRL
                        var grrL = model.GRRList.First(x => x.Level == "L");

                        if (grrL != null)
                        {
                            sheet = workBook.GetSheetAt(9);

                            row = sheet.GetRow(0);

                            cell = row.GetCell(7);
                            cell.SetCellValue(model.MSAResponsor);

                            cell = row.GetCell(10);
                            cell.SetCellValue(grrL.DateString);

                            row = sheet.GetRow(1);

                            if (model.GRRLTvUsed == "1")
                            {
                                cell = row.GetCell(11);
                                cell.SetCellValue("V");
                            }

                            row = sheet.GetRow(2);

                            cell = row.GetCell(0);
                            cell.SetCellValue(model.Equipment.MSACalNo);

                            cell = row.GetCell(3);
                            cell.SetCellValue(model.GRRLPackageType);

                            cell = row.GetCell(6);
                            cell.SetCellValue(model.MSACharacteristic);

                            cell = row.GetCell(10);
                            if (model.GRRLUSL.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(model.GRRLUSL));
                            }

                            if (model.GRRLTvUsed == "2")
                            {
                                cell = row.GetCell(11);
                                cell.SetCellValue("V");
                            }

                            row = sheet.GetRow(3);

                            cell = row.GetCell(10);
                            if (model.GRRLCL.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(model.GRRLCL));
                            }

                            if (model.GRRLTvUsed == "3")
                            {
                                cell = row.GetCell(11);
                                cell.SetCellValue("V");
                            }

                            row = sheet.GetRow(4);

                            cell = row.GetCell(10);
                            if (model.GRRLLSL.HasValue)
                            {
                                cell.SetCellValue(Convert.ToDouble(model.GRRLLSL));
                            }

                            if (model.GRRLTvUsed == "4")
                            {
                                cell = row.GetCell(11);
                                cell.SetCellValue("V");
                            }

                            row = sheet.GetRow(8);

                            for (int part = 1; part <= 10; part++)
                            {
                                var l = grrL.AppraiserList.First(x => x.Appraiser == "A").ValueList.First(x => x.Part == part && x.Trials == 1);

                                if (l != null)
                                {
                                    cell = row.GetCell(part);
                                    if (l.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(l.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(9);

                            var appraiser = grrL.AppraiserList.First(x => x.Appraiser == "A");

                            if (appraiser != null)
                            {
                                cell = row.GetCell(0);
                                cell.SetCellValue(appraiser.UserID);

                                for (int part = 1; part <= 10; part++)
                                {
                                    var l = grrL.AppraiserList.First(x => x.Appraiser == "A").ValueList.First(x => x.Part == part && x.Trials == 2);

                                    if (l != null)
                                    {
                                        cell = row.GetCell(part);
                                        if (l.Value.HasValue)
                                        {
                                            cell.SetCellValue(Convert.ToDouble(l.Value));
                                        }
                                    }
                                }
                            }

                            row = sheet.GetRow(10);

                            for (int part = 1; part <= 10; part++)
                            {
                                var l = grrL.AppraiserList.First(x => x.Appraiser == "A").ValueList.First(x => x.Part == part && x.Trials == 3);

                                if (l != null)
                                {
                                    cell = row.GetCell(part);
                                    if (l.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(l.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(13);

                            for (int part = 1; part <= 10; part++)
                            {
                                var l = grrL.AppraiserList.First(x => x.Appraiser == "B").ValueList.First(x => x.Part == part && x.Trials == 1);

                                if (l != null)
                                {
                                    cell = row.GetCell(part);
                                    if (l.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(l.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(14);

                            appraiser = grrL.AppraiserList.First(x => x.Appraiser == "B");

                            if (appraiser != null)
                            {
                                cell = row.GetCell(0);
                                cell.SetCellValue(appraiser.UserID);

                                for (int part = 1; part <= 10; part++)
                                {
                                    var l = grrL.AppraiserList.First(x => x.Appraiser == "B").ValueList.First(x => x.Part == part && x.Trials == 2);

                                    if (l != null)
                                    {
                                        cell = row.GetCell(part);
                                        if (l.Value.HasValue)
                                        {
                                            cell.SetCellValue(Convert.ToDouble(l.Value));
                                        }
                                    }
                                }
                            }

                            row = sheet.GetRow(15);

                            for (int part = 1; part <= 10; part++)
                            {
                                var l = grrL.AppraiserList.First(x => x.Appraiser == "B").ValueList.First(x => x.Part == part && x.Trials == 3);

                                if (l != null)
                                {
                                    cell = row.GetCell(part);
                                    if (l.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(l.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(18);

                            for (int part = 1; part <= 10; part++)
                            {
                                var l = grrL.AppraiserList.First(x => x.Appraiser == "C").ValueList.First(x => x.Part == part && x.Trials == 1);

                                if (l != null)
                                {
                                    cell = row.GetCell(part);
                                    if (l.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(l.Value));
                                    }
                                }
                            }

                            row = sheet.GetRow(19);

                            appraiser = grrL.AppraiserList.First(x => x.Appraiser == "C");

                            if (appraiser != null)
                            {
                                cell = row.GetCell(0);
                                cell.SetCellValue(appraiser.UserID);

                                for (int part = 1; part <= 10; part++)
                                {
                                    var l = grrL.AppraiserList.First(x => x.Appraiser == "C").ValueList.First(x => x.Part == part && x.Trials == 2);

                                    if (l != null)
                                    {
                                        cell = row.GetCell(part);
                                        if (l.Value.HasValue)
                                        {
                                            cell.SetCellValue(Convert.ToDouble(l.Value));
                                        }
                                    }
                                }
                            }

                            row = sheet.GetRow(20);

                            for (int part = 1; part <= 10; part++)
                            {
                                var l = grrL.AppraiserList.First(x => x.Appraiser == "C").ValueList.First(x => x.Part == part && x.Trials == 3);

                                if (l != null)
                                {
                                    cell = row.GetCell(part);
                                    if (l.Value.HasValue)
                                    {
                                        cell.SetCellValue(Convert.ToDouble(l.Value));
                                    }
                                }
                            }

                            sheet.ForceFormulaRecalculation = true;

                            sheet = workBook.GetSheetAt(10);

                            row = sheet.GetRow(30);
                            cell = row.GetCell(0);
                            appraiser = grrL.AppraiserList.First(x => x.Appraiser == "A");
                            cell.SetCellValue(appraiser.UserID);

                            row = sheet.GetRow(33);
                            cell = row.GetCell(0);
                            appraiser = grrL.AppraiserList.First(x => x.Appraiser == "B");
                            cell.SetCellValue(appraiser.UserID);

                            row = sheet.GetRow(36);
                            cell = row.GetCell(0);
                            appraiser = grrL.AppraiserList.First(x => x.Appraiser == "C");
                            cell.SetCellValue(appraiser.UserID);

                            sheet.ForceFormulaRecalculation = true;
                        }
                        #endregion

                        #region Anova L
                        sheet = workBook.GetSheetAt(11);

                        double[][][] anovaLData = new double[3][][];

                        a = 0;

                        foreach (var appraiser in grrL.AppraiserList)
                        {
                            anovaLData[a] = new double[10][];

                            for (int part = 1; part <= 10; part++)
                            {
                                var trials1 = appraiser.ValueList.First(x => x.Part == part && x.Trials == 1).Value;
                                var trials2 = appraiser.ValueList.First(x => x.Part == part && x.Trials == 2).Value;
                                var trials3 = appraiser.ValueList.First(x => x.Part == part && x.Trials == 3).Value;

                                anovaLData[a][part - 1] = new double[] 
                            { 
                                trials1.HasValue?Convert.ToDouble(trials1.Value):0,
                                trials2.HasValue?Convert.ToDouble(trials2.Value):0,
                                trials3.HasValue?Convert.ToDouble(trials3.Value):0
                            };
                            }

                            a++;
                        }

                        var anovaL = new TwoWayAnova(anovaLData, TwoWayAnovaModel.Mixed);

                        factorA = anovaL.Table.FirstOrDefault(x => x.Source == "Factor A");
                        factorB = anovaL.Table.FirstOrDefault(x => x.Source == "Factor B");
                        interaction = anovaL.Table.FirstOrDefault(x => x.Source == "Interaction AxB");
                        group = anovaL.Table.FirstOrDefault(x => x.Source == "Within-cells (error)");
                        total = anovaL.Table.FirstOrDefault(x => x.Source == "Total");

                        row = sheet.GetRow(2);

                        cell = row.GetCell(1);
                        cell.SetCellValue(factorA.SumOfSquares);
                        cell = row.GetCell(2);
                        cell.SetCellValue(factorA.DegreesOfFreedom);
                        cell = row.GetCell(3);
                        cell.SetCellValue(factorA.MeanSquares);
                        cell = row.GetCell(4);
                        if (factorA.Statistic.HasValue)
                        {
                            cell.SetCellValue(factorA.Statistic.Value);
                        }
                        cell = row.GetCell(5);
                        cell.SetCellValue(factorA.Significance.PValue);

                        cell = row.GetCell(9);
                        if (model.GRRLUSL.HasValue && model.GRRLLSL.HasValue)
                        {
                            cell.SetCellValue(Convert.ToDouble(model.GRRLUSL.Value - model.GRRLLSL.Value));
                        }

                        row = sheet.GetRow(3);

                        cell = row.GetCell(1);
                        cell.SetCellValue(factorB.SumOfSquares);
                        cell = row.GetCell(2);
                        cell.SetCellValue(factorB.DegreesOfFreedom);
                        cell = row.GetCell(3);
                        cell.SetCellValue(factorB.MeanSquares);
                        cell = row.GetCell(4);
                        if (factorB.Statistic.HasValue)
                        {
                            cell.SetCellValue(factorB.Statistic.Value);
                        }
                        cell = row.GetCell(5);
                        cell.SetCellValue(factorB.Significance.PValue);

                        row = sheet.GetRow(4);

                        cell = row.GetCell(1);
                        cell.SetCellValue(interaction.SumOfSquares);
                        cell = row.GetCell(2);
                        cell.SetCellValue(interaction.DegreesOfFreedom);
                        cell = row.GetCell(3);
                        cell.SetCellValue(interaction.MeanSquares);
                        cell = row.GetCell(4);
                        if (interaction.Statistic.HasValue)
                        {
                            cell.SetCellValue(interaction.Statistic.Value);
                        }
                        cell = row.GetCell(5);
                        cell.SetCellValue(interaction.Significance.PValue);

                        row = sheet.GetRow(5);

                        cell = row.GetCell(1);
                        cell.SetCellValue(group.SumOfSquares);
                        cell = row.GetCell(2);
                        cell.SetCellValue(group.DegreesOfFreedom);
                        cell = row.GetCell(3);
                        cell.SetCellValue(group.MeanSquares);

                        row = sheet.GetRow(6);

                        cell = row.GetCell(1);
                        cell.SetCellValue(total.SumOfSquares);
                        cell = row.GetCell(2);
                        cell.SetCellValue(total.DegreesOfFreedom);

                        row = sheet.GetRow(12);
                        cell = row.GetCell(2);
                        if (model.GRRLUSL.HasValue)
                        {
                            cell.SetCellValue(Convert.ToDouble(model.GRRLUSL.Value));
                        }

                        row = sheet.GetRow(13);
                        cell = row.GetCell(2);
                        if (model.GRRLCL.HasValue)
                        {
                            cell.SetCellValue(Convert.ToDouble(model.GRRLCL.Value));
                        }

                        row = sheet.GetRow(14);
                        cell = row.GetCell(2);
                        if (model.GRRLLSL.HasValue)
                        {
                            cell.SetCellValue(Convert.ToDouble(model.GRRLLSL.Value));
                        }

                        row = sheet.GetRow(15);
                        cell = row.GetCell(2);
                        if (grrL.Average.HasValue)
                        {
                            cell.SetCellValue(Convert.ToDouble(grrL.Average.Value));
                        }

                        sheet.ForceFormulaRecalculation = true;
                        #endregion

                        sheet = workBook.GetSheetAt(12);
                        row = sheet.CreateRow(0);
                        cell = row.CreateCell(0);
                        cell.SetCellValue(model.UniqueID);

                        File.Delete(fileName);

                        using (FileStream file = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
                        {
                            workBook.Write(file);
                            file.Close();
                        }

                        using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                        {
                            workBook = new HSSFWorkbook(file);
                            file.Close();
                        }

                        using (ASEDbEntities db = new ASEDbEntities())
                        {
                            var form = db.QA_MSAFORM.First(x => x.UNIQUEID == model.UniqueID);

                            var evaluator = workBook.GetCreationHelper().CreateFormulaEvaluator();

                            #region Stability
                            sheet = workBook.GetSheetAt(1);

                            var tmpRow = sheet.GetRow(1);

                            var tmpCell = tmpRow.GetCell(29);

                            var cellValue = evaluator.Evaluate(tmpCell);

                            var stabilityValue = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(stabilityValue))
                            {
                                decimal r;

                                if (decimal.TryParse(stabilityValue, out r))
                                {
                                    form.STABILITYVAL = r;
                                }
                                else
                                {
                                    form.STABILITYVAL = null;
                                }
                            }
                            else
                            {
                                form.STABILITYVAL = null;
                            }

                            tmpRow = sheet.GetRow(2);
                            tmpCell = tmpRow.GetCell(29);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var stdev = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(stdev))
                            {
                                decimal r;

                                if (decimal.TryParse(stdev, out r))
                                {
                                    form.STABILITYSTDEV = r;
                                }
                                else
                                {
                                    form.STABILITYSTDEV = null;
                                }
                            }
                            else
                            {
                                form.STABILITYSTDEV = null;
                            }

                            tmpRow = sheet.GetRow(3);
                            tmpCell = tmpRow.GetCell(29);

                            cellValue = evaluator.Evaluate(tmpCell);

                            if (form.STABILITYVAL.HasValue && form.STABILITYSTDEV.HasValue)
                            {
                                var stability = ExcelHelper.GetCellValue(cellValue);

                                if (stability == "PASS")
                                {
                                    form.STABILITYRESULT = "P";
                                }
                                else
                                {
                                    form.STABILITYRESULT = "F";
                                }
                            }
                            else
                            {
                                form.STABILITYRESULT = string.Empty;
                            }
                            #endregion

                            #region BL
                            sheet = workBook.GetSheetAt(2);

                            tmpRow = sheet.GetRow(9);
                            tmpCell = tmpRow.GetCell(7);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var tstatistic = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(tstatistic))
                            {
                                decimal r;

                                if (decimal.TryParse(tstatistic, out r))
                                {
                                    form.BIASTSTATISTIC = r;
                                }
                                else
                                {
                                    form.BIASTSTATISTIC = null;
                                }
                            }
                            else
                            {
                                form.BIASTSTATISTIC = null;
                            }

                            tmpRow = sheet.GetRow(15);
                            tmpCell = tmpRow.GetCell(7);

                            cellValue = evaluator.Evaluate(tmpCell);

                            if (form.BIASTSTATISTIC.HasValue)
                            {
                                var bias = ExcelHelper.GetCellValue(cellValue);

                                if (bias == "PASS")
                                {
                                    form.BIASRESULT = "P";
                                }
                                else
                                {
                                    form.BIASRESULT = "F";
                                }
                            }
                            else
                            {
                                form.BIASRESULT = string.Empty;
                            }

                            tmpRow = sheet.GetRow(26);
                            tmpCell = tmpRow.GetCell(16);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var ta = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(ta))
                            {
                                decimal r;

                                if (decimal.TryParse(ta, out r))
                                {
                                    form.LINEARITYTA = r;
                                }
                                else
                                {
                                    form.LINEARITYTA = null;
                                }
                            }
                            else
                            {
                                form.LINEARITYTA = null;
                            }

                            tmpCell = tmpRow.GetCell(18);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var tb = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(tb))
                            {
                                decimal r;

                                if (decimal.TryParse(tb, out r))
                                {
                                    form.LINEARITYTB = r;
                                }
                                else
                                {
                                    form.LINEARITYTB = null;
                                }
                            }
                            else
                            {
                                form.LINEARITYTB = null;
                            }

                            tmpRow = sheet.GetRow(28);
                            tmpCell = tmpRow.GetCell(16);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var linearityA = ExcelHelper.GetCellValue(cellValue);

                            tmpCell = tmpRow.GetCell(18);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var linearityB = ExcelHelper.GetCellValue(cellValue);

                            if (form.LINEARITYTA.HasValue && form.LINEARITYTB.HasValue)
                            {
                                if (linearityA == "PASS" && linearityB == "PASS")
                                {
                                    form.LINEARITYRESULT = "P";
                                }
                                else
                                {
                                    form.LINEARITYRESULT = "F";
                                }
                            }
                            else
                            {
                                form.LINEARITYRESULT = string.Empty;
                            }
                            #endregion

                            #region GRR H
                            sheet = workBook.GetSheetAt(3);

                            tmpRow = sheet.GetRow(40);
                            tmpCell = tmpRow.GetCell(10);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrh = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(grrh))
                            {
                                decimal r;

                                if (decimal.TryParse(grrh, out r))
                                {
                                    form.GRRH = r;
                                }
                                else
                                {
                                    form.GRRH = null;
                                }
                            }
                            else
                            {
                                form.GRRH = null;
                            }

                            tmpRow = sheet.GetRow(48);
                            tmpCell = tmpRow.GetCell(10);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrhndc = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(grrhndc))
                            {
                                decimal r;

                                if (decimal.TryParse(grrhndc, out r))
                                {
                                    form.GRRHNDC = r;
                                }
                                else
                                {
                                    form.GRRHNDC = null;
                                }
                            }
                            else
                            {
                                form.GRRHNDC = null;
                            }

                            tmpRow = sheet.GetRow(47);
                            tmpCell = tmpRow.GetCell(12);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrhPass = ExcelHelper.GetCellValue(cellValue);

                            tmpRow = sheet.GetRow(48);
                            tmpCell = tmpRow.GetCell(12);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrhImprove = ExcelHelper.GetCellValue(cellValue);

                            tmpRow = sheet.GetRow(49);
                            tmpCell = tmpRow.GetCell(12);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrhReject = ExcelHelper.GetCellValue(cellValue);

                            if (grrhPass == "V")
                            {
                                form.GRRHRESULT = "P";
                            }
                            else if (grrhImprove == "V")
                            {
                                form.GRRHRESULT = "I";
                            }
                            else if (grrhReject == "V")
                            {
                                form.GRRHRESULT = "R";
                            }
                            else
                            {
                                form.GRRHRESULT = "";
                            }
                            #endregion

                            #region Anova H
                            sheet = workBook.GetSheetAt(5);

                            tmpRow = sheet.GetRow(7);
                            tmpCell = tmpRow.GetCell(14);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var anovaHTV = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(anovaHTV))
                            {
                                decimal r;

                                if (decimal.TryParse(anovaHTV, out r))
                                {
                                    form.ANOVAHTV = r;
                                }
                                else
                                {
                                    form.ANOVAHTV = null;
                                }
                            }
                            else
                            {
                                form.ANOVAHTV = null;
                            }

                            tmpRow = sheet.GetRow(8);
                            tmpCell = tmpRow.GetCell(14);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var anovaHPT = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(anovaHPT))
                            {
                                decimal r;

                                if (decimal.TryParse(anovaHPT, out r))
                                {
                                    form.ANOVAHPT = r;
                                }
                                else
                                {
                                    form.ANOVAHPT = null;
                                }
                            }
                            else
                            {
                                form.ANOVAHPT = null;
                            }

                            tmpRow = sheet.GetRow(9);
                            tmpCell = tmpRow.GetCell(12);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var anovaHNDC = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(anovaHNDC))
                            {
                                decimal r;

                                if (decimal.TryParse(anovaHNDC, out r))
                                {
                                    form.ANOVAHNDC = r;
                                }
                                else
                                {
                                    form.ANOVAHNDC = null;
                                }
                            }
                            else
                            {
                                form.ANOVAHNDC = null;
                            }

                            tmpRow = sheet.GetRow(7);
                            tmpCell = tmpRow.GetCell(15);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var avovaHTVPass = ExcelHelper.GetCellValue(cellValue);

                            tmpRow = sheet.GetRow(8);
                            tmpCell = tmpRow.GetCell(15);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var avovaHPTPass = ExcelHelper.GetCellValue(cellValue);

                            tmpRow = sheet.GetRow(9);
                            tmpCell = tmpRow.GetCell(15);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var avovaHNDCPass = ExcelHelper.GetCellValue(cellValue);

                            if (avovaHTVPass == "Pass" && avovaHPTPass == "Pass" && avovaHNDCPass == "Pass")
                            {
                                form.ANOVAHRESULT = "P";
                            }
                            else if (avovaHTVPass == " Fail " || avovaHPTPass == " Fail " || avovaHNDCPass == " Fail ")
                            {
                                form.ANOVAHRESULT = "F";
                            }
                            else
                            {
                                form.ANOVAHRESULT = null;
                            }
                            #endregion

                            #region GRR M
                            sheet = workBook.GetSheetAt(6);

                            tmpRow = sheet.GetRow(40);
                            tmpCell = tmpRow.GetCell(10);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrm = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(grrm))
                            {
                                decimal r;

                                if (decimal.TryParse(grrm, out r))
                                {
                                    form.GRRM = r;
                                }
                                else
                                {
                                    form.GRRM = null;
                                }
                            }
                            else
                            {
                                form.GRRM = null;
                            }

                            tmpRow = sheet.GetRow(48);
                            tmpCell = tmpRow.GetCell(10);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrmndc = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(grrmndc))
                            {
                                decimal r;

                                if (decimal.TryParse(grrmndc, out r))
                                {
                                    form.GRRMNDC = r;
                                }
                                else
                                {
                                    form.GRRMNDC = null;
                                }
                            }
                            else
                            {
                                form.GRRMNDC = null;
                            }

                            tmpRow = sheet.GetRow(47);
                            tmpCell = tmpRow.GetCell(12);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrmPass = ExcelHelper.GetCellValue(cellValue);

                            tmpRow = sheet.GetRow(48);
                            tmpCell = tmpRow.GetCell(12);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrmImprove = ExcelHelper.GetCellValue(cellValue);

                            tmpRow = sheet.GetRow(49);
                            tmpCell = tmpRow.GetCell(12);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrmReject = ExcelHelper.GetCellValue(cellValue);

                            if (grrmPass == "V")
                            {
                                form.GRRMRESULT = "P";
                            }
                            else if (grrmImprove == "V")
                            {
                                form.GRRMRESULT = "I";
                            }
                            else if (grrmReject == "V")
                            {
                                form.GRRMRESULT = "R";
                            }
                            else
                            {
                                form.GRRMRESULT = "";
                            }
                            #endregion

                            #region Anova M
                            sheet = workBook.GetSheetAt(8);

                            tmpRow = sheet.GetRow(7);
                            tmpCell = tmpRow.GetCell(14);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var anovaMTV = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(anovaMTV))
                            {
                                decimal r;

                                if (decimal.TryParse(anovaMTV, out r))
                                {
                                    form.ANOVAMTV = r;
                                }
                                else
                                {
                                    form.ANOVAMTV = null;
                                }
                            }
                            else
                            {
                                form.ANOVAMTV = null;
                            }

                            tmpRow = sheet.GetRow(8);
                            tmpCell = tmpRow.GetCell(14);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var anovaMPT = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(anovaMPT))
                            {
                                decimal r;

                                if (decimal.TryParse(anovaMPT, out r))
                                {
                                    form.ANOVAMPT = r;
                                }
                                else
                                {
                                    form.ANOVAMPT = null;
                                }
                            }
                            else
                            {
                                form.ANOVAMPT = null;
                            }

                            tmpRow = sheet.GetRow(9);
                            tmpCell = tmpRow.GetCell(12);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var anovaMNDC = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(anovaMNDC))
                            {
                                decimal r;

                                if (decimal.TryParse(anovaMNDC, out r))
                                {
                                    form.ANOVAMNDC = r;
                                }
                                else
                                {
                                    form.ANOVAMNDC = null;
                                }
                            }
                            else
                            {
                                form.ANOVAMNDC = null;
                            }

                            tmpRow = sheet.GetRow(7);
                            tmpCell = tmpRow.GetCell(15);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var avovaMTVPass = ExcelHelper.GetCellValue(cellValue);

                            tmpRow = sheet.GetRow(8);
                            tmpCell = tmpRow.GetCell(15);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var avovaMPTPass = ExcelHelper.GetCellValue(cellValue);

                            tmpRow = sheet.GetRow(9);
                            tmpCell = tmpRow.GetCell(15);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var avovaMNDCPass = ExcelHelper.GetCellValue(cellValue);

                            if (avovaMTVPass == "Pass" && avovaMPTPass == "Pass" && avovaMNDCPass == "Pass")
                            {
                                form.ANOVAMRESULT = "P";
                            }
                            else if (avovaMTVPass == " Fail " || avovaMPTPass == " Fail " || avovaMNDCPass == " Fail ")
                            {
                                form.ANOVAMRESULT = "F";
                            }
                            else
                            {
                                form.ANOVAMRESULT = null;
                            }
                            #endregion

                            #region GRR L
                            sheet = workBook.GetSheetAt(9);

                            tmpRow = sheet.GetRow(40);
                            tmpCell = tmpRow.GetCell(10);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrl = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(grrl))
                            {
                                decimal r;

                                if (decimal.TryParse(grrl, out r))
                                {
                                    form.GRRL = r;
                                }
                                else
                                {
                                    form.GRRL = null;
                                }
                            }
                            else
                            {
                                form.GRRL = null;
                            }

                            tmpRow = sheet.GetRow(48);
                            tmpCell = tmpRow.GetCell(10);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrlndc = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(grrlndc))
                            {
                                decimal r;

                                if (decimal.TryParse(grrlndc, out r))
                                {
                                    form.GRRLNDC = r;
                                }
                                else
                                {
                                    form.GRRLNDC = null;
                                }
                            }
                            else
                            {
                                form.GRRLNDC = null;
                            }

                            tmpRow = sheet.GetRow(47);
                            tmpCell = tmpRow.GetCell(12);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrlPass = ExcelHelper.GetCellValue(cellValue);

                            tmpRow = sheet.GetRow(48);
                            tmpCell = tmpRow.GetCell(12);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrlImprove = ExcelHelper.GetCellValue(cellValue);

                            tmpRow = sheet.GetRow(49);
                            tmpCell = tmpRow.GetCell(12);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var grrlReject = ExcelHelper.GetCellValue(cellValue);

                            if (grrlPass == "V")
                            {
                                form.GRRLRESULT = "P";
                            }
                            else if (grrlImprove == "V")
                            {
                                form.GRRLRESULT = "I";
                            }
                            else if (grrlReject == "V")
                            {
                                form.GRRLRESULT = "R";
                            }
                            else
                            {
                                form.GRRLRESULT = "";
                            }
                            #endregion

                            #region Anova L
                            sheet = workBook.GetSheetAt(11);

                            tmpRow = sheet.GetRow(7);
                            tmpCell = tmpRow.GetCell(14);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var anovaLTV = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(anovaLTV))
                            {
                                decimal r;

                                if (decimal.TryParse(anovaLTV, out r))
                                {
                                    form.ANOVALTV = r;
                                }
                                else
                                {
                                    form.ANOVALTV = null;
                                }
                            }
                            else
                            {
                                form.ANOVALTV = null;
                            }

                            tmpRow = sheet.GetRow(8);
                            tmpCell = tmpRow.GetCell(14);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var anovaLPT = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(anovaLPT))
                            {
                                decimal r;

                                if (decimal.TryParse(anovaLPT, out r))
                                {
                                    form.ANOVALPT = r;
                                }
                                else
                                {
                                    form.ANOVALPT = null;
                                }
                            }
                            else
                            {
                                form.ANOVALPT = null;
                            }

                            tmpRow = sheet.GetRow(9);
                            tmpCell = tmpRow.GetCell(12);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var anovaLNDC = ExcelHelper.GetCellValue(cellValue);

                            if (!string.IsNullOrEmpty(anovaLNDC))
                            {
                                decimal r;

                                if (decimal.TryParse(anovaLNDC, out r))
                                {
                                    form.ANOVALNDC = r;
                                }
                                else
                                {
                                    form.ANOVALNDC = null;
                                }
                            }
                            else
                            {
                                form.ANOVALNDC = null;
                            }

                            tmpRow = sheet.GetRow(7);
                            tmpCell = tmpRow.GetCell(15);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var avovaLTVPass = ExcelHelper.GetCellValue(cellValue);

                            tmpRow = sheet.GetRow(8);
                            tmpCell = tmpRow.GetCell(15);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var avovaLPTPass = ExcelHelper.GetCellValue(cellValue);

                            tmpRow = sheet.GetRow(9);
                            tmpCell = tmpRow.GetCell(15);

                            cellValue = evaluator.Evaluate(tmpCell);

                            var avovaLNDCPass = ExcelHelper.GetCellValue(cellValue);

                            if (avovaLTVPass == "Pass" && avovaLPTPass == "Pass" && avovaLNDCPass == "Pass")
                            {
                                form.ANOVALRESULT = "P";
                            }
                            else if (avovaLTVPass == " Fail " || avovaLPTPass == " Fail " || avovaLNDCPass == " Fail ")
                            {
                                form.ANOVALRESULT = "F";
                            }
                            else
                            {
                                form.ANOVALRESULT = null;
                            }
                            #endregion

                            db.SaveChanges();
                        }
                    }
                }
                #endregion

                #region 計數
                if (model.Type == "2")
                {
                    File.Copy(Config.ReportTemplate_MSA_Count, fileName);

                    using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                    {
                        workBook = new HSSFWorkbook(file);
                        file.Close();
                    }

                    var sheet = workBook.GetSheetAt(0);

                    var row = sheet.GetRow(3);
                    var cell = row.GetCell(2);

                    cell.SetCellValue(string.Format("{0}/{1}", model.CountPackageType, model.MSACharacteristic));

                    cell = row.GetCell(11);
                    if (model.MSADate.HasValue)
                    {
                        cell.SetCellValue(model.MSADate.Value);
                    }

                    row = sheet.GetRow(4);
                    cell = row.GetCell(2);

                    cell.SetCellValue(string.Format("{0}/{1}", model.MSAIchi, model.Equipment.MSACalNo));

                    cell = row.GetCell(11);
                    cell.SetCellValue(model.MSAResponsor);

                    row = sheet.GetRow(6);

                    cell = row.GetCell(1);

                    cell.SetCellValue(model.CountList.First(x => x.Appraiser == "A").UserID);

                    cell = row.GetCell(4);

                    cell.SetCellValue(model.CountList.First(x => x.Appraiser == "B").UserID);

                    cell = row.GetCell(7);

                    cell.SetCellValue(model.CountList.First(x => x.Appraiser == "C").UserID);

                    var rowIndex = 9;

                    var appraiserList = new List<string> { "A", "B", "C" };

                    for (int sample = 1; sample <= 50; sample++)
                    {
                        rowIndex = 8 + sample;

                        row = sheet.GetRow(rowIndex);

                        var cellIndex = 1;

                        foreach (var appraiser in appraiserList)
                        {
                            var count = model.CountList.First(x => x.Appraiser == appraiser);

                            for (int trials = 1; trials <= 3; trials++)
                            {
                                var countValue = count.ValueList.First(x => x.Sample == sample && x.Trials == trials);

                                cell = row.GetCell(cellIndex);

                                if (countValue.Value.HasValue)
                                {
                                    cell.SetCellValue(countValue.Value.Value);
                                }

                                cellIndex++;
                            }
                        }

                        var reference = model.CountReferenceValueList.First(x => x.Sample == sample);

                        if (reference.Reference.HasValue)
                        {
                            cell = row.GetCell(cellIndex);
                            cell.SetCellValue(reference.Reference.Value);
                        }

                        cellIndex++;

                        if (reference.ReferenceValue.HasValue)
                        {
                            cell = row.GetCell(cellIndex);
                            cell.SetCellValue(reference.ReferenceValue.Value);
                        }
                    }

                    sheet.ForceFormulaRecalculation = true;

                    sheet = workBook.GetSheetAt(1);
                    sheet.ForceFormulaRecalculation = true;

                    sheet = workBook.GetSheetAt(2);
                    sheet.ForceFormulaRecalculation = true;

                    sheet = workBook.GetSheetAt(3);
                    row = sheet.CreateRow(0);
                    cell = row.CreateCell(0);
                    cell.SetCellValue(model.UniqueID);

                    File.Delete(fileName);

                    using (FileStream file = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
                    {
                        workBook.Write(file);
                        file.Close();
                    }

                    using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                    {
                        workBook = new HSSFWorkbook(file);
                        file.Close();
                    }

                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var form = db.QA_MSAFORM.First(x => x.UNIQUEID == model.UniqueID);

                        var evaluator = workBook.GetCreationHelper().CreateFormulaEvaluator();

                        #region Kappa
                        sheet = workBook.GetSheetAt(1);

                        var tmpRow = sheet.GetRow(46);

                        var tmpCell = tmpRow.GetCell(10);

                        var cellValue = evaluator.Evaluate(tmpCell);

                        var kappaA = ExcelHelper.GetCellValue(cellValue);

                        if (!string.IsNullOrEmpty(kappaA))
                        {
                            decimal r;

                            if (decimal.TryParse(kappaA, out r))
                            {
                                form.KAPPAA = r;
                            }
                            else
                            {
                                form.KAPPAA = null;
                            }
                        }
                        else
                        {
                            form.KAPPAA = null;
                        }

                        tmpCell = tmpRow.GetCell(11);

                        cellValue = evaluator.Evaluate(tmpCell);

                        var kappaB = ExcelHelper.GetCellValue(cellValue);

                        if (!string.IsNullOrEmpty(kappaB))
                        {
                            decimal r;

                            if (decimal.TryParse(kappaB, out r))
                            {
                                form.KAPPAB = r;
                            }
                            else
                            {
                                form.KAPPAB = null;
                            }
                        }
                        else
                        {
                            form.KAPPAB = null;
                        }

                        tmpCell = tmpRow.GetCell(12);

                        cellValue = evaluator.Evaluate(tmpCell);

                        var kappaC = ExcelHelper.GetCellValue(cellValue);

                        if (!string.IsNullOrEmpty(kappaC))
                        {
                            decimal r;

                            if (decimal.TryParse(kappaC, out r))
                            {
                                form.KAPPAC = r;
                            }
                            else
                            {
                                form.KAPPAC = null;
                            }
                        }
                        else
                        {
                            form.KAPPAC = null;
                        }

                        tmpRow = sheet.GetRow(54);
                        tmpCell = tmpRow.GetCell(3);

                        cellValue = evaluator.Evaluate(tmpCell);

                        if (form.KAPPAA.HasValue && form.KAPPAB.HasValue && form.KAPPAC.HasValue)
                        {
                            var kappaResult = ExcelHelper.GetCellValue(cellValue);

                            if (kappaResult == "Pass")
                            {
                                form.KAPPARESULT = "P";
                            }
                            else
                            {
                                form.KAPPARESULT = "F";
                            }
                        }
                        else
                        {
                            form.KAPPARESULT = string.Empty;
                        }
                        #endregion

                        #region Count
                        sheet = workBook.GetSheetAt(2);

                        tmpRow = sheet.GetRow(20);
                        tmpCell = tmpRow.GetCell(2);

                        cellValue = evaluator.Evaluate(tmpCell);

                        var countAEffective = ExcelHelper.GetCellValue(cellValue);

                        if (!string.IsNullOrEmpty(countAEffective))
                        {
                            decimal r;

                            if (decimal.TryParse(countAEffective, out r))
                            {
                                form.COUNTAEFFECTIVE = r;
                            }
                            else
                            {
                                form.COUNTAEFFECTIVE = null;
                            }
                        }
                        else
                        {
                            form.COUNTAEFFECTIVE = null;
                        }

                        tmpCell = tmpRow.GetCell(3);

                        cellValue = evaluator.Evaluate(tmpCell);

                        var countAError = ExcelHelper.GetCellValue(cellValue);

                        if (!string.IsNullOrEmpty(countAError))
                        {
                            decimal r;

                            if (decimal.TryParse(countAError, out r))
                            {
                                form.COUNTAERROR = r;
                            }
                            else
                            {
                                form.COUNTAERROR = null;
                            }
                        }
                        else
                        {
                            form.COUNTAERROR = null;
                        }

                        tmpCell = tmpRow.GetCell(4);

                        cellValue = evaluator.Evaluate(tmpCell);

                        var countAAlarm = ExcelHelper.GetCellValue(cellValue);

                        if (!string.IsNullOrEmpty(countAAlarm))
                        {
                            decimal r;

                            if (decimal.TryParse(countAAlarm, out r))
                            {
                                form.COUNTAALARM = r;
                            }
                            else
                            {
                                form.COUNTAALARM = null;
                            }
                        }
                        else
                        {
                            form.COUNTAALARM = null;
                        }

                        tmpRow = sheet.GetRow(21);
                        tmpCell = tmpRow.GetCell(2);

                        cellValue = evaluator.Evaluate(tmpCell);

                        var countBEffective = ExcelHelper.GetCellValue(cellValue);

                        if (!string.IsNullOrEmpty(countBEffective))
                        {
                            decimal r;

                            if (decimal.TryParse(countBEffective, out r))
                            {
                                form.COUNTBEFFECTIVE = r;
                            }
                            else
                            {
                                form.COUNTBEFFECTIVE = null;
                            }
                        }
                        else
                        {
                            form.COUNTBEFFECTIVE = null;
                        }

                        tmpCell = tmpRow.GetCell(3);

                        cellValue = evaluator.Evaluate(tmpCell);

                        var countBError = ExcelHelper.GetCellValue(cellValue);

                        if (!string.IsNullOrEmpty(countBError))
                        {
                            decimal r;

                            if (decimal.TryParse(countBError, out r))
                            {
                                form.COUNTBERROR = r;
                            }
                            else
                            {
                                form.COUNTBERROR = null;
                            }
                        }
                        else
                        {
                            form.COUNTBERROR = null;
                        }

                        tmpCell = tmpRow.GetCell(4);

                        cellValue = evaluator.Evaluate(tmpCell);

                        var countBAlarm = ExcelHelper.GetCellValue(cellValue);

                        if (!string.IsNullOrEmpty(countBAlarm))
                        {
                            decimal r;

                            if (decimal.TryParse(countBAlarm, out r))
                            {
                                form.COUNTBALARM = r;
                            }
                            else
                            {
                                form.COUNTBALARM = null;
                            }
                        }
                        else
                        {
                            form.COUNTBALARM = null;
                        }

                        tmpRow = sheet.GetRow(22);
                        tmpCell = tmpRow.GetCell(2);

                        cellValue = evaluator.Evaluate(tmpCell);

                        var countCEffective = ExcelHelper.GetCellValue(cellValue);

                        if (!string.IsNullOrEmpty(countCEffective))
                        {
                            decimal r;

                            if (decimal.TryParse(countCEffective, out r))
                            {
                                form.COUNTCEFFECTIVE = r;
                            }
                            else
                            {
                                form.COUNTCEFFECTIVE = null;
                            }
                        }
                        else
                        {
                            form.COUNTCEFFECTIVE = null;
                        }

                        tmpCell = tmpRow.GetCell(3);

                        cellValue = evaluator.Evaluate(tmpCell);

                        var countCError = ExcelHelper.GetCellValue(cellValue);

                        if (!string.IsNullOrEmpty(countCError))
                        {
                            decimal r;

                            if (decimal.TryParse(countCError, out r))
                            {
                                form.COUNTCERROR = r;
                            }
                            else
                            {
                                form.COUNTCERROR = null;
                            }
                        }
                        else
                        {
                            form.COUNTCERROR = null;
                        }

                        tmpCell = tmpRow.GetCell(4);

                        cellValue = evaluator.Evaluate(tmpCell);

                        var countCAlarm = ExcelHelper.GetCellValue(cellValue);

                        if (!string.IsNullOrEmpty(countCAlarm))
                        {
                            decimal r;

                            if (decimal.TryParse(countCAlarm, out r))
                            {
                                form.COUNTCALARM = r;
                            }
                            else
                            {
                                form.COUNTCALARM = null;
                            }
                        }
                        else
                        {
                            form.COUNTCALARM = null;
                        }

                        tmpRow = sheet.GetRow(45);
                        tmpCell = tmpRow.GetCell(2);

                        cellValue = evaluator.Evaluate(tmpCell);

                        if (form.COUNTAEFFECTIVE.HasValue && form.COUNTAERROR.HasValue && form.COUNTAALARM.HasValue && form.COUNTBEFFECTIVE.HasValue && form.COUNTBERROR.HasValue && form.COUNTBALARM.HasValue && form.COUNTCEFFECTIVE.HasValue && form.COUNTCERROR.HasValue && form.COUNTCALARM.HasValue)
                        {
                            var countResult = ExcelHelper.GetCellValue(cellValue);

                            if (countResult == "Pass")
                            {
                                form.COUNTRESULT = "P";
                            }
                            else
                            {
                                form.COUNTRESULT = "F";
                            }
                        }
                        else
                        {
                            form.COUNTRESULT = string.Empty;
                        }

                        #endregion

                        db.SaveChanges();
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public static RequestResult Submit(List<Models.Shared.Organization> OrganizationList, string UniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = db.QA_MSAFORM.First(x => x.UNIQUEID == UniqueID);

                    form.STATUS = "3";

                    var time = DateTime.Now;

                    var seq = 1;

                    if (db.QA_CALIBRATIONFORMFLOWLOG.Any(x => x.FORMUNIQUEID == UniqueID))
                    {
                        seq = db.QA_CALIBRATIONFORMFLOWLOG.Max(x => x.SEQ) + 1;
                    }

                    var log = db.QA_CALIBRATIONFORMFLOWLOG.FirstOrDefault(x => x.FORMUNIQUEID == UniqueID && x.FLOWSEQ == 0 && !x.VERIFYTIME.HasValue);

                    if (log != null)
                    {
                        log.VERIFYTIME = time;
                        log.VERIFYRESULT = "Y";
                    }
                    else
                    {
                        db.QA_CALIBRATIONFORMFLOWLOG.Add(new QA_CALIBRATIONFORMFLOWLOG()
                        {
                            FORMUNIQUEID = UniqueID,
                            SEQ = seq,
                            FLOWSEQ = 0,
                            USERID = Account.ID,
                            NOTIFYTIME = time,
                            VERIFYTIME = time,
                            VERIFYRESULT = "Y"
                        });

                        seq++;
                    }

                    db.QA_CALIBRATIONFORMFLOWLOG.Add(new QA_CALIBRATIONFORMFLOWLOG()
                    {
                        FORMUNIQUEID = UniqueID,
                        SEQ = seq,
                        FLOWSEQ = 1,
                        NOTIFYTIME = time
                    });

                    db.SaveChanges();

                    SendVerifyMail(OrganizationList, UniqueID, form.VHNO);

                    result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.SendApprove, Resources.Resource.Success));
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

        public static RequestResult GetVerifyFormModel(List<Models.Shared.Organization> OrganizationList, string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                result = GetDetailViewModel(OrganizationList, UniqueID);

                if (result.IsSuccess)
                {
                    var model = result.Data as DetailViewModel;

                    result.ReturnData(new VerifyFormModel()
                    {
                        UniqueID = model.UniqueID,
                        FormViewModel = model
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

        public static RequestResult Approve(List<Models.Shared.Organization> OrganizationList, VerifyFormModel Model, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var log = db.QA_CALIBRATIONFORMFLOWLOG.FirstOrDefault(x => x.FORMUNIQUEID == Model.UniqueID && x.FLOWSEQ == 1 && !x.VERIFYTIME.HasValue);

                    if (log != null)
                    {
                        log.USERID = Account.ID;
                        log.VERIFYTIME = DateTime.Now;
                        log.VERIFYRESULT = "Y";
                        log.VERIFYCOMMENT = Model.FormInput.Comment;

                        var form = db.QA_MSAFORM.First(x => x.UNIQUEID == Model.UniqueID);

                        form.STATUS = "5";

                        var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == form.EQUIPMENTUNIQUEID);

                        var nextMSADate = form.ESTMSADATE.Value.AddMonths(Convert.ToInt32(equipment.MSACYCLE)).AddDays(-1);

                        equipment.LASTMSADATE = form.ESTMSADATE;
                        equipment.NEXTMSADATE = nextMSADate;

                        db.SaveChanges();

                        SendFinishedMail(OrganizationList, form.UNIQUEID, form.VHNO);
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

        public static RequestResult Reject(List<Models.Shared.Organization> OrganizationList, VerifyFormModel Model, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var log = db.QA_CALIBRATIONFORMFLOWLOG.FirstOrDefault(x => x.FORMUNIQUEID == Model.UniqueID && x.FLOWSEQ == 1 && !x.VERIFYTIME.HasValue);

                    if (log != null)
                    {
                        log.USERID = Account.ID;
                        log.VERIFYTIME = DateTime.Now;
                        log.VERIFYRESULT = "N";
                        log.VERIFYCOMMENT = Model.FormInput.Comment;

                        var form = db.QA_MSAFORM.First(x => x.UNIQUEID == Model.UniqueID);

                        form.STATUS = "4";

                        var seq = db.QA_CALIBRATIONFORMFLOWLOG.Where(x => x.FORMUNIQUEID == form.UNIQUEID).Max(x => x.SEQ) + 1;

                        db.QA_CALIBRATIONFORMFLOWLOG.Add(new QA_CALIBRATIONFORMFLOWLOG()
                        {
                            FORMUNIQUEID = form.UNIQUEID,
                            SEQ = seq,
                            FLOWSEQ = 0,
                            NOTIFYTIME = DateTime.Now,
                            USERID = form.MSARESPONSORID
                        });

                        db.SaveChanges();

                        SendRejectMail(OrganizationList, form.UNIQUEID, form.VHNO, new List<string> { form.MSARESPONSORID });
                    }
                }

                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Reject, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static void SendFinishedMail(List<Models.Shared.Organization> OrganizationList, string UniqueID, string VHNO)
        {
            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = db.QA_MSAFORM.First(x => x.UNIQUEID == UniqueID);

                    var userList = new List<string>();

                    userList.Add(form.MSARESPONSORID);

                    var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == form.EQUIPMENTUNIQUEID);

                    userList.Add(equipment.OWNERID);
                    userList.Add(equipment.OWNERMANAGERID);
                    userList.Add(equipment.PEID);
                    userList.Add(equipment.PEMANAGERID);

                    if (userList != null && userList.Count > 0)
                    {
                        SendVerifyMail(OrganizationList, UniqueID, string.Format("[完成通知][{0}]MSA執行單", VHNO), userList);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public static void SendRemindMail(List<Models.Shared.Organization> OrganizationList, string UniqueID, string VHNO, int Days, bool IsDelay)
        {
            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var userList = db.USERAUTHGROUP.Where(x => x.AUTHGROUPID == "QA-Verify").Select(x => x.USERID).ToList();

                    if (userList != null && userList.Count > 0)
                    {
                        if (IsDelay)
                        {
                            SendVerifyMail(OrganizationList, UniqueID, string.Format("[逾期][簽核通知][{0}天][{1}]MSA執行單", Days, VHNO), userList);
                        }
                        else
                        {
                            SendVerifyMail(OrganizationList, UniqueID, string.Format("[跟催][簽核通知][{0}天][{1}]MSA執行單", Days, VHNO), userList);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public static void SendVerifyMail(List<Models.Shared.Organization> OrganizationList, string UniqueID, string VHNO)
        {
            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var userList = db.USERAUTHGROUP.Where(x => x.AUTHGROUPID == "QA-Verify").Select(x => x.USERID).ToList();

                    if (userList != null && userList.Count > 0)
                    {
                        SendVerifyMail(OrganizationList, UniqueID, string.Format("[簽核通知][{0}]MSA執行單", VHNO), userList);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private static void SendVerifyMail(List<Models.Shared.Organization> OrganizationList, string UniqueID, string Subject, List<string> UserList)
        {
            try
            {
                var mailAddressList = new List<MailAddress>();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = db.QA_MSAFORM.First(x => x.UNIQUEID == UniqueID);

                    var equipment = EquipmentHelper.Get(OrganizationList, form.EQUIPMENTUNIQUEID);

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
                        sb.Append(string.Format(td, form.VHNO));
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
                        sb.Append(string.Format(th, "連結"));
                        sb.Append(string.Format(td, string.Format("<a href=\"http://ASECL-eINSPRD01/FEM.Portal/Home/Index?ReturnUrl=http://ASECL-eINSPRD01/FEM/zh-tw/Customized_ASE_QA/MSAForm/Index?VHNO={0}\">連結</a>", form.VHNO)));
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

        public static void SendRejectMail(List<Models.Shared.Organization> OrganizationList, string UniqueID, string VHNO, List<string> UserList)
        {
            try
            {
                SendVerifyMail(OrganizationList, UniqueID, string.Format("[退回修正通知][{0}]MSA執行單", VHNO), UserList);
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public static FormStatus GetFormStatus(string UniqueID)
        {
            FormStatus status = null;

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = db.QA_MSAFORM.First(x => x.UNIQUEID == UniqueID);

                    status = new FormStatus(query.STATUS, query.ESTMSADATE.Value);
                }
            }
            catch (Exception ex)
            {
                status = null;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return status;
        }
    }
}
