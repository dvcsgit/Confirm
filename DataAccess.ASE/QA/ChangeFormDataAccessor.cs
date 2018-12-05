using DbEntity.ASE;
using Models.ASE.QA.ChangeForm;
using Models.Authenticated;
using NPOI.HSSF.UserModel;
using NPOI.OpenXmlFormats.Spreadsheet;
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
using Utility;
using Utility.Models;

namespace DataAccess.ASE.QA
{
    public class ChangeFormDataAccessor
    {
        public static List<GridItem> Query(List<Models.Shared.Organization> OrganizationList, string EquipmentUniqueID)
        {
            var model = new List<GridItem>();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = (from x in db.QA_CHANGEFORM
                                 join d in db.QA_CHANGEFORMDETAIL
                                 on x.UNIQUEID equals d.FORMUNIQUEID
                                 join e in db.QA_EQUIPMENT
                                 on d.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                 where d.EQUIPMENTUNIQUEID==EquipmentUniqueID
                                 select new
                                 {
                                     x.VHNO,
                                     UniqueID = x.UNIQUEID,
                                     Status = x.STATUS,
                                     OrganizationUniqueID = e.ORGANIZATIONUNIQUEID,
                                     ChangeType = x.CHANGETYPE,
                                     OwnerID = e.OWNERID,
                                     OwnerManagerID = e.OWNERMANAGERID,
                                     PEID = e.PEID,
                                     PEManagerID = e.PEMANAGERID,
                                     CreateTime = x.CREATETIME,
                                     NewOwnerID = x.NEWOWNERID,
                                     NewOwnerManagerID = x.NEWOWNERMANAGERID,
                                     NewPEID = x.NEWPEID,
                                     NewPEManagerID = x.NEWPEMANAGERID,
                                     IsQRCoded = x.ISQRCODE == "Y",
                                     e.CALNO
                                 }).AsQueryable();

                    var temp = query.Select(x => new
                    {
                        x.VHNO,
                        x.UniqueID,
                        x.Status,
                        x.OrganizationUniqueID,
                        x.ChangeType,
                        x.OwnerID,
                        x.OwnerManagerID,
                        x.PEID,
                        x.PEManagerID,
                        x.CreateTime,
                        x.NewOwnerID,
                        x.NewOwnerManagerID,
                        x.NewPEID,
                        x.NewPEManagerID,
                        x.IsQRCoded,
                    }).Distinct().ToList();

                    foreach (var t in temp)
                    {
                        var owner = db.ACCOUNT.FirstOrDefault(x => x.ID == t.OwnerID);
                        var ownerManager = db.ACCOUNT.FirstOrDefault(x => x.ID == t.OwnerManagerID);

                        model.Add(new GridItem()
                        {
                            UniqueID = t.UniqueID,
                            VHNO = t.VHNO,
                            Status = t.Status,
                            ChangeType = t.ChangeType,
                            Factory = OrganizationDataAccessor.GetFactory(OrganizationList, t.OrganizationUniqueID),
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(t.OrganizationUniqueID),
                            OwnerID = t.OwnerID,
                            OwnerName = owner != null ? owner.NAME : string.Empty,
                            OwnerManagerID = t.OwnerManagerID,
                            OwnerManagerName = ownerManager != null ? ownerManager.NAME : string.Empty,
                            CreateTime = t.CreateTime,
                            IsQRCoded = t.IsQRCoded,
                            LogList = db.QA_CHANGEFORMFLOWLOG.Where(l => l.FORMUNIQUEID == t.UniqueID && l.ISCANCELED == "N" && !l.VERIFYTIME.HasValue).Select(l => new LogModel
                            {
                                Seq = l.SEQ,
                                FlowSeq = l.FLOWSEQ.Value,
                                NotifyTime = l.NOTIFYTIME.Value,
                                VerifyResult = l.VERIFYRESULT,
                                UserID = l.USERID,
                                VerifyTime = l.VERIFYTIME,
                                VerifyComment = l.VERIFYCOMMENT,
                            }).ToList()
                        });
                    }

                    model = model.OrderBy(x => x.CreateTimeString).ToList();
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
                    var query = (from x in db.QA_CHANGEFORM
                                 join d in db.QA_CHANGEFORMDETAIL
                                 on x.UNIQUEID equals d.FORMUNIQUEID
                                 join e in db.QA_EQUIPMENT
                                 on d.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                 select new
                                 {
                                     x.VHNO,
                                     UniqueID = x.UNIQUEID,
                                     Status = x.STATUS,
                                     OrganizationUniqueID = e.ORGANIZATIONUNIQUEID,
                                     ChangeType = x.CHANGETYPE,
                                     OwnerID = e.OWNERID,
                                     OwnerManagerID = e.OWNERMANAGERID,
                                     PEID = e.PEID,
                                     PEManagerID = e.PEMANAGERID,
                                     CreateTime = x.CREATETIME,
                                     NewOwnerID = x.NEWOWNERID,
                                     NewOwnerManagerID = x.NEWOWNERMANAGERID,
                                     NewPEID = x.NEWPEID,
                                     NewPEManagerID = x.NEWPEMANAGERID,
                                     IsQRCoded = x.ISQRCODE == "Y",
                                     e.CALNO
                                 }).AsQueryable();

                    if (!qa)
                    {
                        query = query.Where(x => Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID) || x.OwnerID == Account.ID || x.OwnerManagerID == Account.ID || x.PEID == Account.ID || x.PEManagerID == Account.ID || x.NewOwnerID == Account.ID || x.NewOwnerManagerID == Account.ID || x.NewPEID == Account.ID || x.NewPEManagerID == Account.ID).AsQueryable();
                    }

                    if (Parameters.BeginDate.HasValue)
                    {
                        query = query.Where(x => DateTime.Compare(x.CreateTime, Parameters.BeginDate.Value) >= 0);
                    }

                    if (Parameters.EndDate.HasValue)
                    {
                        query = query.Where(x => DateTime.Compare(x.CreateTime, Parameters.EndDate.Value) < 0);
                    }

                    if (!string.IsNullOrEmpty(Parameters.Status))
                    {
                        query = query.Where(x => x.Status == Parameters.Status);
                    }

                    if (!string.IsNullOrEmpty(Parameters.ChangeType))
                    {
                        query = query.Where(x => x.ChangeType == Parameters.ChangeType);
                    }

                    if (!string.IsNullOrEmpty(Parameters.VHNO))
                    {
                        query = query.Where(x => x.VHNO.Contains(Parameters.VHNO));
                    }

                    if (!string.IsNullOrEmpty(Parameters.OwnerID))
                    {
                        query = query.Where(x => x.OwnerID == Parameters.OwnerID);
                    }

                    if (!string.IsNullOrEmpty(Parameters.CALNO))
                    {
                        query = query.Where(x => x.CALNO.Contains(Parameters.CALNO));
                    }

                    var temp = query.Select(x => new
                    {
                        x.VHNO,
                        x.UniqueID,
                        x.Status,
                        x.OrganizationUniqueID,
                        x.ChangeType,
                        x.OwnerID,
                        x.OwnerManagerID,
                        x.PEID,
                        x.PEManagerID,
                        x.CreateTime,
                        x.NewOwnerID,
                        x.NewOwnerManagerID,
                        x.NewPEID,
                        x.NewPEManagerID,
                        x.IsQRCoded,
                    }).Distinct().ToList();

                    foreach (var t in temp)
                    {
                        var owner = db.ACCOUNT.FirstOrDefault(x => x.ID == t.OwnerID);
                        var ownerManager = db.ACCOUNT.FirstOrDefault(x => x.ID == t.OwnerManagerID);

                        model.ItemList.Add(new GridItem()
                        {
                            UniqueID = t.UniqueID,
                            VHNO = t.VHNO,
                            Status = t.Status,
                            ChangeType = t.ChangeType,
                            Factory = OrganizationDataAccessor.GetFactory(OrganizationList, t.OrganizationUniqueID),
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(t.OrganizationUniqueID),
                            OwnerID = t.OwnerID,
                            OwnerName = owner != null ? owner.NAME : string.Empty,
                            OwnerManagerID = t.OwnerManagerID,
                            OwnerManagerName = ownerManager != null ? ownerManager.NAME : string.Empty,
                            CreateTime = t.CreateTime,
                            Account = Account,
                            IsQRCoded  =t.IsQRCoded,
                            LogList = db.QA_CHANGEFORMFLOWLOG.Where(l => l.FORMUNIQUEID == t.UniqueID && l.ISCANCELED == "N" && !l.VERIFYTIME.HasValue).Select(l => new LogModel
                            {
                                Seq = l.SEQ,
                                FlowSeq = l.FLOWSEQ.Value,
                                NotifyTime = l.NOTIFYTIME.Value,
                                VerifyResult = l.VERIFYRESULT,
                                UserID = l.USERID,
                                VerifyTime = l.VERIFYTIME,
                                VerifyComment = l.VERIFYCOMMENT,
                            }).ToList()
                        });
                    }

                    model.ItemList = model.ItemList.OrderBy(x => x.Seq).ThenBy(x => x.StatusSeq).ThenByDescending(x => x.VHNO).ToList();
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

        public static RequestResult GetCreateFormModel(List<Models.Shared.Organization> OrganizationList, string ChangeType, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new CreateFormModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    model = new CreateFormModel()
                    {
                        ChangeType = ChangeType
                    };

                    var equipmentQuery = db.QA_EQUIPMENT.Where(x => x.OWNERID == Account.ID || x.OWNERMANAGERID == Account.ID || (string.IsNullOrEmpty(x.OWNERID) && (x.PEID == Account.ID || x.PEMANAGERID == Account.ID))).AsQueryable();

                    //異常未結案
                    var query1 = (from x in db.QA_ABNORMALFORM
                                  join f in db.QA_CALIBRATIONFORM
                                  on x.CALFORMUNIQUEID equals f.UNIQUEID
                                  join a in db.QA_CALIBRATIONAPPLY
                                  on f.APPLYUNIQUEID equals a.UNIQUEID into tmpApply
                                  from a in tmpApply.DefaultIfEmpty()
                                  join n in db.QA_CALIBRATIONNOTIFY
                                  on f.NOTIFYUNIQUEID equals n.UNIQUEID into tmpNotify
                                  from n in tmpNotify.DefaultIfEmpty()
                                  where x.STATUS != "4"
                                  select a != null ? a.EQUIPMENTUNIQUEID : n.EQUIPMENTUNIQUEID).ToList();

                    equipmentQuery = equipmentQuery.Where(x => !query1.Contains(x.UNIQUEID));

                    var query2 = (from x in db.QA_CHANGEFORM
                                  join d in db.QA_CHANGEFORMDETAIL
                                  on x.UNIQUEID equals d.FORMUNIQUEID
                                  where x.STATUS == "1"
                                  select d.EQUIPMENTUNIQUEID).ToList();

                    equipmentQuery = equipmentQuery.Where(x => !query2.Contains(x.UNIQUEID));

                    var query3 = (from x in db.QA_CALIBRATIONFORM
                                  join n in db.QA_CALIBRATIONNOTIFY
                                  on x.NOTIFYUNIQUEID equals n.UNIQUEID
                                  where !(x.STATUS == "5" || x.STATUS == "6" || x.STATUS == "9")
                                  select n.EQUIPMENTUNIQUEID).ToList();

                    equipmentQuery = equipmentQuery.Where(x => !query3.Contains(x.UNIQUEID));

                    var query4 = (from x in db.QA_CALIBRATIONFORM
                                  join a in db.QA_CALIBRATIONAPPLY
                                  on x.NOTIFYUNIQUEID equals a.UNIQUEID
                                  where !(x.STATUS == "5" || x.STATUS == "6" || x.STATUS == "9")
                                  select a.EQUIPMENTUNIQUEID).ToList();

                    equipmentQuery = equipmentQuery.Where(x => !query4.Contains(x.UNIQUEID));

                    //復用
                    if (ChangeType == "7")
                    {
                        equipmentQuery = equipmentQuery.Where(x => (x.STATUS == "3" || x.STATUS == "4" || x.STATUS == "5" || x.STATUS == "6" || x.STATUS == "7"));
                    }
                    //庫存
                    else if (ChangeType == "6")
                    {
                        equipmentQuery = equipmentQuery.Where(x => (x.STATUS == "1" || x.STATUS == "2" || x.STATUS == "3" || x.STATUS == "6"));
                    }
                    //報廢
                    else if (ChangeType == "3")
                    {
                        equipmentQuery = equipmentQuery.Where(x => (x.STATUS == "1" || x.STATUS == "2" || x.STATUS == "3" || x.STATUS == "6"));
                    }
                    //維修
                    else if (ChangeType == "4")
                    {
                        equipmentQuery = equipmentQuery.Where(x => (x.STATUS == "1" || x.STATUS == "2" || x.STATUS == "3" || x.STATUS == "5"));
                    }
                    //遺失
                    else if (ChangeType == "2")
                    {
                        equipmentQuery = equipmentQuery.Where(x => (x.STATUS == "1" || x.STATUS == "2" || x.STATUS == "3" || x.STATUS == "7"));
                    }
                    //免校
                    else if (ChangeType == "1")
                    {
                        equipmentQuery = equipmentQuery.Where(x => (x.STATUS == "1" || x.STATUS == "2" || x.STATUS == "6"));
                    }
                    //移轉
                    else if (ChangeType == "5")
                    {
                        equipmentQuery = equipmentQuery.Where(x => (x.STATUS == "1" || x.STATUS == "2" || x.STATUS == "3" || x.STATUS == "7"));
                    }
                    //免MSA
                    else if (ChangeType == "8")
                    {
                        equipmentQuery = equipmentQuery.Where(x => x.MSA == "Y");
                    }
                    //變更校正週期
                    else if (ChangeType == "9")
                    {
                        equipmentQuery = equipmentQuery.Where(x => x.CAL == "Y");

                        var q1 = (from f in db.QA_CALIBRATIONFORM
                                      join a in db.QA_CALIBRATIONAPPLY
                                      on f.APPLYUNIQUEID equals a.UNIQUEID
                                      where f.STATUS != "5" && f.STATUS != "9"
                                      select a.EQUIPMENTUNIQUEID).Distinct().ToList();

                        var q2 = (from f in db.QA_CALIBRATIONFORM
                                      join n in db.QA_CALIBRATIONNOTIFY
                                      on f.NOTIFYUNIQUEID equals n.UNIQUEID
                                      where f.STATUS != "5" && f.STATUS != "9"
                                      select n.EQUIPMENTUNIQUEID).Distinct().ToList();

                        var q = q1.Union(q2).Distinct().ToList();

                        equipmentQuery = equipmentQuery.Where(x => !q.Contains(x.UNIQUEID));
                    }
                    //變更MSA週期
                    else if (ChangeType == "A")
                    {
                        equipmentQuery = equipmentQuery.Where(x => x.MSA == "Y");

                        var q = db.QA_MSAFORM.Where(x => x.STATUS != "5" && x.STATUS != "6").Select(x => x.EQUIPMENTUNIQUEID).Distinct().ToList();

                        equipmentQuery = equipmentQuery.Where(x => !q.Contains(x.UNIQUEID));
                    }
                    else if (ChangeType == "B")
                    {
                        equipmentQuery = equipmentQuery.Where(x => x.CAL == "N");

                        equipmentQuery = equipmentQuery.Where(x => (x.STATUS == "3" || x.STATUS == "4" || x.STATUS == "5" || x.STATUS == "6" || x.STATUS == "7"));
                    }
                    else if (ChangeType == "C")
                    {
                        equipmentQuery = equipmentQuery.Where(x => x.MSA == "N");

                        equipmentQuery = equipmentQuery.Where(x => (x.STATUS == "3" || x.STATUS == "4" || x.STATUS == "5" || x.STATUS == "6" || x.STATUS == "7"));
                    }

                    var equipmentList = equipmentQuery.ToList();

                    foreach (var equipment in equipmentList)
                    {
                        model.EquipmentList.Add(EquipmentHelper.Get(OrganizationList, equipment.UNIQUEID));
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

        public static RequestResult GetCreateFormModel(List<Models.Shared.Organization> OrganizationList, string AbnormalFormUniqueID, string ChangeType)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new CreateFormModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var equipmentUniqueID = (from x in db.QA_ABNORMALFORM
                                             join f in db.QA_CALIBRATIONFORM
                                             on x.CALFORMUNIQUEID equals f.UNIQUEID
                                             join a in db.QA_CALIBRATIONAPPLY
                                             on f.APPLYUNIQUEID equals a.UNIQUEID into tmpApply
                                             from a in tmpApply.DefaultIfEmpty()
                                             join n in db.QA_CALIBRATIONNOTIFY
                                             on f.NOTIFYUNIQUEID equals n.UNIQUEID into tmpNotify
                                             from n in tmpNotify.DefaultIfEmpty()
                                             where x.UNIQUEID == AbnormalFormUniqueID
                                             select a != null ? a.EQUIPMENTUNIQUEID : n.EQUIPMENTUNIQUEID).First();

                    model = new CreateFormModel()
                    {
                        AbnormalFormUniqueID = AbnormalFormUniqueID,
                        ChangeType = ChangeType,
                        EquipmentList = new List<Models.ASE.QA.Shared.EquipmentModel>() 
                        { 
                            EquipmentHelper.Get(OrganizationList, equipmentUniqueID)
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

        public static RequestResult Create(List<Models.Shared.Organization> OrganizationList, CreateFormModel Model, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                if (Model.FormInput.EquipmentList.Count == 0)
                {
                    result.ReturnFailedMessage(string.Format("{0} {1} {2}", Resources.Resource.EquipmentCount, Resources.Resource.MustMoreThan, "0"));
                }
                else
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        result.Success();

                        if (Model.ChangeType == "B" && Model.EquipmentList.Any(x => string.IsNullOrEmpty(x.OwnerID)))
                        {
                            result.ReturnFailedMessage("設備未設定設備負責人，請先進行設備移轉申請");
                        }
                        
                        if (Model.ChangeType == "CS"&&Model.EquipmentList.Any(x=>string.IsNullOrEmpty(x.PEID)))
                        {
                            result.ReturnFailedMessage("設備未設定製程負責人，請先進行設備移轉申請");
                        }

                        if (result.IsSuccess)
                        {
                            var uniqueid = Guid.NewGuid().ToString();

                            var vhnoPrefix = string.Format("C{0}", DateTime.Today.ToString("yyyyMM").Substring(2));

                            var vhnoSeq = 1;

                            var query = db.QA_CHANGEFORM.Where(x => x.VHNO.StartsWith(vhnoPrefix)).OrderByDescending(x => x.VHNO).ToList();

                            if (query.Count > 0)
                            {
                                vhnoSeq = int.Parse(query.First().VHNO.Substring(5)) + 1;
                            }

                            var vhno = string.Format("{0}{1}", vhnoPrefix, vhnoSeq.ToString().PadLeft(3, '0'));

                            var form = new QA_CHANGEFORM()
                            {
                                UNIQUEID = uniqueid,
                                VHNO = vhno,
                                ABNORMALFORMUNIQUEID = Model.AbnormalFormUniqueID,
                                CREATETIME = DateTime.Now,
                                STATUS = "1",
                                CHANGETYPE = Model.ChangeType,
                                CHANGEREASON = Model.FormInput.Reason,
                                NEWOWNERID = Model.FormInput.OwnerID,
                                NEWOWNERMANAGERID = Model.FormInput.OwnerManagerID,
                                NEWPEID = Model.FormInput.PEID,
                                NEWPEMANAGERID = Model.FormInput.PEManagerID,
                                FIXFINISHEDDATE = Model.FormInput.FixFinishedDate
                            };

                            if (Model.ChangeType == "9")
                            {
                                form.NEWCYCLE = Model.FormInput.NewCALCycle;
                            }

                            if (Model.ChangeType == "A")
                            {
                                form.NEWCYCLE = Model.FormInput.NewMSACycle;
                            }

                            if (Model.ChangeType == "B")
                            {
                                form.NEWCYCLE = Model.FormInput.CALCycle;
                                form.FIXFINISHEDDATE = Model.FormInput.NextCALDate;
                            }

                            if (Model.ChangeType == "C")
                            {
                                form.NEWCYCLE = Model.FormInput.MSACycle;
                                form.FIXFINISHEDDATE = Model.FormInput.NextMSADate;
                            }

                            db.QA_CHANGEFORM.Add(form);

                            if (!string.IsNullOrEmpty(Model.AbnormalFormUniqueID))
                            {
                                var aForm = db.QA_ABNORMALFORM.First(x => x.UNIQUEID == Model.AbnormalFormUniqueID);

                                aForm.STATUS = "3";

                                var logSeq = 1;

                                var logList = db.QA_ABNORMALFORMFLOWLOG.Where(x => x.FORMUNIQUEID == aForm.UNIQUEID).ToList();

                                if (logList.Count > 0)
                                {
                                    logSeq = logList.Max(x => x.SEQ) + 1;
                                }

                                var peID = string.Empty;

                                var formQuery = (from x in db.QA_ABNORMALFORM
                                                 join f in db.QA_CALIBRATIONFORM
                                                 on x.CALFORMUNIQUEID equals f.UNIQUEID
                                                 where x.UNIQUEID == aForm.UNIQUEID
                                                 select new { f.APPLYUNIQUEID, f.NOTIFYUNIQUEID }).FirstOrDefault();

                                if (formQuery != null)
                                {
                                    if (!string.IsNullOrEmpty(formQuery.APPLYUNIQUEID))
                                    {
                                        peID = (from a in db.QA_CALIBRATIONAPPLY
                                                join e in db.QA_EQUIPMENT
                                                on a.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                                where a.UNIQUEID == formQuery.APPLYUNIQUEID
                                                select e.PEID).FirstOrDefault();
                                    }

                                    if (!string.IsNullOrEmpty(formQuery.NOTIFYUNIQUEID))
                                    {
                                        peID = (from n in db.QA_CALIBRATIONNOTIFY
                                                join e in db.QA_EQUIPMENT
                                                on n.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                                where n.UNIQUEID == formQuery.NOTIFYUNIQUEID
                                                select e.PEID).FirstOrDefault();
                                    }
                                }

                                if (!string.IsNullOrEmpty(peID))
                                {
                                    db.QA_ABNORMALFORMFLOWLOG.Add(new QA_ABNORMALFORMFLOWLOG()
                                    {
                                        FORMUNIQUEID = aForm.UNIQUEID,
                                        SEQ = logSeq,
                                        FLOWSEQ = 2,
                                        ISCANCELED = "N",
                                        NOTIFYTIME = DateTime.Now,
                                        USERID = peID
                                    });
                                }
                                else
                                {
                                    result.ReturnFailedMessage("設備無製程責人，無法啟動後續異常通知單流程");
                                }
                            }

                            if (result.IsSuccess)
                            {
                                db.QA_CHANGEFORMDETAIL.AddRange(Model.FormInput.EquipmentList.Distinct().Select(x => new QA_CHANGEFORMDETAIL
                                {
                                    FORMUNIQUEID = uniqueid,
                                    EQUIPMENTUNIQUEID = x
                                }).ToList());

                                var time = DateTime.Now;

                                var equipment = EquipmentHelper.Get(OrganizationList, Model.FormInput.EquipmentList.First());

                                var verifyList = new List<string>();

                                var ownerManagerID = equipment.OwnerManagerID;

                                if (!string.IsNullOrEmpty(ownerManagerID))
                                {
                                    var ownerManager = db.ACCOUNT.FirstOrDefault(x => x.ID == ownerManagerID);

                                    if (ownerManager == null)
                                    {
                                        var owner = db.ACCOUNT.FirstOrDefault(x => x.ID == equipment.OwnerID);

                                        if (owner != null)
                                        {
                                            ownerManagerID = owner.MANAGERID;
                                        }
                                        else
                                        {
                                            result.ReturnFailedMessage(string.Format("設備負責人[{0}]不存在", equipment.OwnerID));
                                        }
                                    }
                                }

                                var peManagerID = equipment.PEManagerID;

                                if (!string.IsNullOrEmpty(peManagerID))
                                {
                                    var peManager = db.ACCOUNT.FirstOrDefault(x => x.ID == peManagerID);

                                    if (peManager == null)
                                    {
                                        var pe = db.ACCOUNT.FirstOrDefault(x => x.ID == equipment.PEID);

                                        if (pe != null)
                                        {
                                            peManagerID = pe.MANAGERID;
                                        }
                                        else
                                        {
                                            result.ReturnFailedMessage(string.Format("製程負責人[{0}]不存在", equipment.PEID));
                                        }
                                    }
                                }

                                if (result.IsSuccess)
                                {
                                    //免校/遺失/報廢/維修/庫存/免MSA/變更週期(MSA/校正)
                                    if (Model.ChangeType == "1" || Model.ChangeType == "2" || Model.ChangeType == "3" || Model.ChangeType == "4" || Model.ChangeType == "6" || Model.ChangeType == "8" || Model.ChangeType == "9" || Model.ChangeType == "A")
                                    {
                                        if (Account.ID == equipment.OwnerID)
                                        {
                                            db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                            {
                                                FORMUNIQUEID = uniqueid,
                                                SEQ = 1,
                                                FLOWSEQ = 0,
                                                ISCANCELED = "N",
                                                NOTIFYTIME = time,
                                                USERID = equipment.OwnerID,
                                                VERIFYRESULT = "Y",
                                                VERIFYTIME = time
                                            });

                                            db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                            {
                                                FORMUNIQUEID = uniqueid,
                                                SEQ = 2,
                                                FLOWSEQ = 1,
                                                ISCANCELED = "N",
                                                NOTIFYTIME = time,
                                                USERID = ownerManagerID
                                            });

                                            verifyList.Add(ownerManagerID);
                                        }
                                        else if (Account.ID == ownerManagerID)
                                        {
                                            db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                            {
                                                FORMUNIQUEID = uniqueid,
                                                SEQ = 1,
                                                FLOWSEQ = 1,
                                                ISCANCELED = "N",
                                                NOTIFYTIME = time,
                                                USERID = ownerManagerID,
                                                VERIFYRESULT = "Y",
                                                VERIFYTIME = time
                                            });

                                            db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                            {
                                                FORMUNIQUEID = uniqueid,
                                                SEQ = 2,
                                                FLOWSEQ = 8,
                                                ISCANCELED = "N",
                                                NOTIFYTIME = time
                                            });

                                            verifyList.Add("QA");
                                        }
                                        else if (Account.ID == equipment.PEID)
                                        {
                                            db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                            {
                                                FORMUNIQUEID = uniqueid,
                                                SEQ = 1,
                                                FLOWSEQ = 2,
                                                ISCANCELED = "N",
                                                NOTIFYTIME = time,
                                                USERID = equipment.PEID,
                                                VERIFYRESULT = "Y",
                                                VERIFYTIME = time
                                            });

                                            db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                            {
                                                FORMUNIQUEID = uniqueid,
                                                SEQ = 2,
                                                FLOWSEQ = 3,
                                                ISCANCELED = "N",
                                                NOTIFYTIME = time,
                                                USERID = peManagerID
                                            });

                                            verifyList.Add(peManagerID);
                                        }
                                        else if (Account.ID == peManagerID)
                                        {
                                            db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                            {
                                                FORMUNIQUEID = uniqueid,
                                                SEQ = 1,
                                                FLOWSEQ = 3,
                                                ISCANCELED = "N",
                                                NOTIFYTIME = time,
                                                USERID = peManagerID,
                                                VERIFYRESULT = "Y",
                                                VERIFYTIME = time
                                            });

                                            db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                            {
                                                FORMUNIQUEID = uniqueid,
                                                SEQ = 2,
                                                FLOWSEQ = 8,
                                                ISCANCELED = "N",
                                                NOTIFYTIME = time
                                            });

                                            verifyList.Add("QA");
                                        }
                                    }
                                    //移轉
                                    else if (Model.ChangeType == "5")
                                    {
                                        if (Account.ID == equipment.OwnerID)
                                        {
                                            db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                            {
                                                FORMUNIQUEID = uniqueid,
                                                SEQ = 1,
                                                FLOWSEQ = 0,
                                                ISCANCELED = "N",
                                                NOTIFYTIME = time,
                                                USERID = equipment.OwnerID,
                                                VERIFYRESULT = "Y",
                                                VERIFYTIME = time
                                            });

                                            db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                            {
                                                FORMUNIQUEID = uniqueid,
                                                SEQ = 2,
                                                FLOWSEQ = 1,
                                                ISCANCELED = "N",
                                                NOTIFYTIME = time,
                                                USERID = ownerManagerID
                                            });

                                            verifyList.Add(ownerManagerID);
                                        }
                                        else if (Account.ID == ownerManagerID)
                                        {
                                            db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                            {
                                                FORMUNIQUEID = uniqueid,
                                                SEQ = 1,
                                                FLOWSEQ = 1,
                                                ISCANCELED = "N",
                                                NOTIFYTIME = time,
                                                USERID = ownerManagerID,
                                                VERIFYRESULT = "Y",
                                                VERIFYTIME = time
                                            });

                                            if (!string.IsNullOrEmpty(Model.FormInput.OwnerID))
                                            {
                                                db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                                {
                                                    FORMUNIQUEID = uniqueid,
                                                    SEQ = 2,
                                                    FLOWSEQ = 4,
                                                    ISCANCELED = "N",
                                                    NOTIFYTIME = time,
                                                    USERID = Model.FormInput.OwnerID
                                                });

                                                verifyList.Add(Model.FormInput.OwnerID);

                                                if (!string.IsNullOrEmpty(Model.FormInput.PEID))
                                                {
                                                    db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                                    {
                                                        FORMUNIQUEID = uniqueid,
                                                        SEQ = 3,
                                                        FLOWSEQ = 6,
                                                        ISCANCELED = "N",
                                                        NOTIFYTIME = time,
                                                        USERID = Model.FormInput.PEID
                                                    });

                                                    verifyList.Add(Model.FormInput.PEID);
                                                }
                                            }
                                            else if (!string.IsNullOrEmpty(Model.FormInput.PEID))
                                            {
                                                db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                                {
                                                    FORMUNIQUEID = uniqueid,
                                                    SEQ = 2,
                                                    FLOWSEQ = 6,
                                                    ISCANCELED = "N",
                                                    NOTIFYTIME = time,
                                                    USERID = Model.FormInput.PEID
                                                });

                                                verifyList.Add(Model.FormInput.PEID);
                                            }
                                        }
                                        else if (Account.ID == equipment.PEID)
                                        {
                                            db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                            {
                                                FORMUNIQUEID = uniqueid,
                                                SEQ = 1,
                                                FLOWSEQ = 2,
                                                ISCANCELED = "N",
                                                NOTIFYTIME = time,
                                                USERID = equipment.PEID,
                                                VERIFYRESULT = "Y",
                                                VERIFYTIME = time
                                            });

                                            db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                            {
                                                FORMUNIQUEID = uniqueid,
                                                SEQ = 2,
                                                FLOWSEQ = 3,
                                                ISCANCELED = "N",
                                                NOTIFYTIME = time,
                                                USERID = peManagerID
                                            });

                                            verifyList.Add(peManagerID);
                                        }
                                        else if (Account.ID == peManagerID)
                                        {
                                            db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                            {
                                                FORMUNIQUEID = uniqueid,
                                                SEQ = 1,
                                                FLOWSEQ = 3,
                                                ISCANCELED = "N",
                                                NOTIFYTIME = time,
                                                USERID = peManagerID,
                                                VERIFYRESULT = "Y",
                                                VERIFYTIME = time
                                            });

                                            if (!string.IsNullOrEmpty(Model.FormInput.OwnerID))
                                            {
                                                db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                                {
                                                    FORMUNIQUEID = uniqueid,
                                                    SEQ = 2,
                                                    FLOWSEQ = 4,
                                                    ISCANCELED = "N",
                                                    NOTIFYTIME = time,
                                                    USERID = Model.FormInput.OwnerID
                                                });

                                                verifyList.Add(Model.FormInput.OwnerID);

                                                if (!string.IsNullOrEmpty(Model.FormInput.PEID))
                                                {
                                                    db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                                    {
                                                        FORMUNIQUEID = uniqueid,
                                                        SEQ = 3,
                                                        FLOWSEQ = 6,
                                                        ISCANCELED = "N",
                                                        NOTIFYTIME = time,
                                                        USERID = Model.FormInput.PEID
                                                    });

                                                    verifyList.Add(Model.FormInput.PEID);
                                                }
                                            }
                                            else if (!string.IsNullOrEmpty(Model.FormInput.PEID))
                                            {
                                                db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                                {
                                                    FORMUNIQUEID = uniqueid,
                                                    SEQ = 2,
                                                    FLOWSEQ = 6,
                                                    ISCANCELED = "N",
                                                    NOTIFYTIME = time,
                                                    USERID = Model.FormInput.PEID
                                                });

                                                verifyList.Add(Model.FormInput.PEID);
                                            }
                                        }
                                    }
                                    //復用
                                    else if (Model.ChangeType == "7" || Model.ChangeType == "B" || Model.ChangeType == "C")
                                    {
                                        if (Account.ID == equipment.OwnerID)
                                        {
                                            db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                            {
                                                FORMUNIQUEID = uniqueid,
                                                SEQ = 1,
                                                FLOWSEQ = 0,
                                                ISCANCELED = "N",
                                                NOTIFYTIME = time,
                                                USERID = equipment.OwnerID,
                                                VERIFYRESULT = "Y",
                                                VERIFYTIME = time
                                            });

                                            db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                            {
                                                FORMUNIQUEID = uniqueid,
                                                SEQ = 2,
                                                FLOWSEQ = 1,
                                                ISCANCELED = "N",
                                                NOTIFYTIME = time,
                                                USERID = ownerManagerID
                                            });

                                            verifyList.Add(ownerManagerID);

                                            if (!string.IsNullOrEmpty(equipment.PEID))
                                            {
                                                db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                                {
                                                    FORMUNIQUEID = uniqueid,
                                                    SEQ = 3,
                                                    FLOWSEQ = 2,
                                                    ISCANCELED = "N",
                                                    NOTIFYTIME = time,
                                                    USERID = equipment.PEID
                                                });

                                                verifyList.Add(equipment.PEID);
                                            }
                                        }
                                        else if (Account.ID == ownerManagerID)
                                        {
                                            db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                            {
                                                FORMUNIQUEID = uniqueid,
                                                SEQ = 1,
                                                FLOWSEQ = 1,
                                                ISCANCELED = "N",
                                                NOTIFYTIME = time,
                                                USERID = ownerManagerID,
                                                VERIFYRESULT = "Y",
                                                VERIFYTIME = time
                                            });

                                            if (!string.IsNullOrEmpty(equipment.PEID))
                                            {
                                                db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                                {
                                                    FORMUNIQUEID = uniqueid,
                                                    SEQ = 3,
                                                    FLOWSEQ = 2,
                                                    ISCANCELED = "N",
                                                    NOTIFYTIME = time,
                                                    USERID = equipment.PEID
                                                });

                                                verifyList.Add(equipment.PEID);
                                            }
                                            else
                                            {
                                                db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                                {
                                                    FORMUNIQUEID = uniqueid,
                                                    SEQ = 2,
                                                    FLOWSEQ = 8,
                                                    ISCANCELED = "N",
                                                    NOTIFYTIME = time
                                                });

                                                verifyList.Add("QA");
                                            }
                                        }
                                        else if (Account.ID == equipment.PEID)
                                        {
                                            db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                            {
                                                FORMUNIQUEID = uniqueid,
                                                SEQ = 1,
                                                FLOWSEQ = 2,
                                                ISCANCELED = "N",
                                                NOTIFYTIME = time,
                                                USERID = equipment.PEID,
                                                VERIFYRESULT = "Y",
                                                VERIFYTIME = time
                                            });

                                            db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                            {
                                                FORMUNIQUEID = uniqueid,
                                                SEQ = 2,
                                                FLOWSEQ = 3,
                                                ISCANCELED = "N",
                                                NOTIFYTIME = time,
                                                USERID = peManagerID
                                            });

                                            verifyList.Add(peManagerID);
                                        }
                                        else if (Account.ID == peManagerID)
                                        {
                                            db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                            {
                                                FORMUNIQUEID = uniqueid,
                                                SEQ = 1,
                                                FLOWSEQ = 3,
                                                ISCANCELED = "N",
                                                NOTIFYTIME = time,
                                                USERID = peManagerID,
                                                VERIFYRESULT = "Y",
                                                VERIFYTIME = time
                                            });

                                            db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                            {
                                                FORMUNIQUEID = uniqueid,
                                                SEQ = 2,
                                                FLOWSEQ = 8,
                                                ISCANCELED = "N",
                                                NOTIFYTIME = time
                                            });

                                            verifyList.Add("QA");
                                        }
                                    }

                                    db.SaveChanges();

                                    foreach (var verify in verifyList)
                                    {
                                        if (verify == "QA")
                                        {
                                            SendVerifyMail(uniqueid, vhno);
                                        }
                                        else
                                        {
                                            SendVerifyMail(uniqueid, vhno, verify);
                                        }
                                    }

                                    result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Apply, Resources.Resource.CalibrationChangeForm, Resources.Resource.Success));
                                }
                            }
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

        public static RequestResult GetDetailViewModel(string UniqueID)
        {
            return GetDetailViewModel(null, UniqueID);
        }

        public static RequestResult GetDetailViewModel(List<Models.Shared.Organization> OrganizationList, string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new DetailViewModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = (from x in db.QA_CHANGEFORM
                                join d in db.QA_CHANGEFORMDETAIL
                                on x.UNIQUEID equals d.FORMUNIQUEID
                                join e in db.QA_EQUIPMENT
                                on d.EQUIPMENTUNIQUEID equals e.UNIQUEID
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
                                join newOwner in db.ACCOUNT
                                on x.NEWOWNERID equals newOwner.ID into tmpNewOwner
                                from newOwner in tmpNewOwner.DefaultIfEmpty()
                                join newOwnerMgr in db.ACCOUNT
                                on x.NEWOWNERMANAGERID equals newOwnerMgr.ID into tmpNewOwnerMgr
                                from newOwnerMgr in tmpNewOwnerMgr.DefaultIfEmpty()
                                join newPE in db.ACCOUNT
                                on x.NEWPEID equals newPE.ID into tmpNewPE
                                from newPE in tmpNewPE.DefaultIfEmpty()
                                join newPEMgr in db.ACCOUNT
                                on x.NEWPEMANAGERID equals newPEMgr.ID into tmpNewPEMgr
                                from newPEMgr in tmpNewPEMgr.DefaultIfEmpty()
                                where x.UNIQUEID == UniqueID
                                select new
                                {
                                    x.VHNO,
                                    UniqueID = x.UNIQUEID,
                                    Status = x.STATUS,
                                   FixFinishedDate= x.FIXFINISHEDDATE,
                                  NewCycle= x.NEWCYCLE,
                                    OrganizationUniqueID = e.ORGANIZATIONUNIQUEID,
                                    ChangeType = x.CHANGETYPE,
                                    OwnerID = e.OWNERID,
                                    OwnerName = owner != null ? owner.NAME : "",
                                    OwnerManagerID = e.OWNERMANAGERID,
                                    OwnerManagerName = ownerMgr != null ? ownerMgr.NAME : "",
                                    PEID = e.PEID,
                                    PEName = pe != null ? pe.NAME : "",
                                    PEManagerID = e.PEMANAGERID,
                                    PEManagerName = peMgr != null ? peMgr.NAME : "",
                                    NewOwnerID = x.NEWOWNERID,
                                    NewOwnerName = newOwner != null ? newOwner.NAME : "",
                                    NewOwnerManagerID = x.NEWOWNERMANAGERID,
                                    NewOwnerManagerName = newOwnerMgr != null ? newOwnerMgr.NAME : "",
                                    NewPEID = x.NEWPEID,
                                    NewPEName = newPE != null ? newPE.NAME : "",
                                    NewPEManagerID = x.NEWPEMANAGERID,
                                    NewPEManagerName = newPEMgr != null ? newPEMgr.NAME : "",
                                    CreateTime = x.CREATETIME,
                                    ChangeReason = x.CHANGEREASON
                                }).First();
                    
                    model = new DetailViewModel()
                    {
                        UniqueID = form.UniqueID,
                        VHNO = form.VHNO,
                        Status = form.Status,
                        ChangeType = form.ChangeType,
                        Factory = OrganizationDataAccessor.GetFactory(OrganizationList, form.OrganizationUniqueID),
                        OrganizationDescription = OrganizationDataAccessor.GetOrganizationFullDescription(form.OrganizationUniqueID),
                         FixFinishedDate = form.FixFinishedDate,
                         NewCALCycle = form.ChangeType=="9"?form.NewCycle:default(int?),
                         NewMSACycle = form.ChangeType=="A"?form.NewCycle:default(int?),
                        OwnerID = form.OwnerID,
                        OwnerName = form.OwnerName,
                        OwnerManagerID = form.OwnerManagerID,
                        OwnerManagerName = form.OwnerManagerName,
                        PEID = form.PEID,
                        PEName = form.PEName,
                        PEManagerID = form.PEManagerID,
                        PEManagerName = form.PEManagerName,
                        NewOwnerID = form.NewOwnerID,
                        NewOwnerName = form.NewOwnerName,
                        NewOwnerManagerID = form.NewOwnerManagerID,
                        NewOwnerManagerName = form.NewOwnerManagerName,
                        NewPEID = form.NewPEID,
                        NewPEName = form.NewPEName,
                        NewPEManagerID = form.NewPEManagerID,
                        NewPEManagerName = form.NewPEManagerName,
                        CreateTime = form.CreateTime,
                        ChangeReason = form.ChangeReason,
                        LogList = db.QA_CHANGEFORMFLOWLOG.Where(x => x.FORMUNIQUEID == UniqueID && x.ISCANCELED == "N").Select(x => new LogModel
                        {
                            Seq = x.SEQ,
                            FlowSeq = x.FLOWSEQ.Value,
                            NotifyTime = x.NOTIFYTIME.Value,
                            VerifyResult = x.VERIFYRESULT,
                            UserID = x.USERID,
                            VerifyTime = x.VERIFYTIME,
                            VerifyComment = x.VERIFYCOMMENT,
                        }).OrderBy(x => x.VerifyTime).ThenBy(x => x.Seq).ToList(),
                    };

                    var itemList = db.QA_CHANGEFORMDETAIL.Where(x => x.FORMUNIQUEID == UniqueID).ToList();

                    foreach (var item in itemList)
                    {
                        model.ItemList.Add(EquipmentHelper.Get(OrganizationList, item.EQUIPMENTUNIQUEID));
                    }

                    foreach (var item in model.LogList)
                    {
                        var user = db.ACCOUNT.FirstOrDefault(x => x.ID == item.UserID);

                        if (user != null)
                        {
                            item.UserName = user.NAME;
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

        public static RequestResult Approve(string UniqueID, int Seq, string Comment, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = db.QA_CHANGEFORM.First(x => x.UNIQUEID == UniqueID);

                    var item = db.QA_CHANGEFORMDETAIL.First(x => x.FORMUNIQUEID == form.UNIQUEID);

                    var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == item.EQUIPMENTUNIQUEID);

                    var log = db.QA_CHANGEFORMFLOWLOG.First(x => x.FORMUNIQUEID == form.UNIQUEID && x.SEQ == Seq);

                    log.USERID = Account.ID;
                    log.VERIFYTIME = DateTime.Now;
                    log.VERIFYRESULT = "Y";
                    log.VERIFYCOMMENT = Comment;

                    var seq = db.QA_CHANGEFORMFLOWLOG.Where(x => x.FORMUNIQUEID == form.UNIQUEID).Max(x => x.SEQ) + 1;

                    var verifyList = new List<string>();

                    result.Success();

                    var ownerManagerID = equipment.OWNERMANAGERID;

                    if (!string.IsNullOrEmpty(ownerManagerID))
                    {
                        var ownerManager = db.ACCOUNT.FirstOrDefault(x => x.ID == ownerManagerID);

                        if (ownerManager == null)
                        {
                            var owner = db.ACCOUNT.FirstOrDefault(x => x.ID == equipment.OWNERID);

                            if (owner != null)
                            {
                                ownerManagerID = owner.MANAGERID;
                            }
                            else
                            {
                                result.ReturnFailedMessage(string.Format("設備負責人[{0}]不存在", equipment.OWNERID));
                            }
                        }
                    }

                    var peManagerID = equipment.PEMANAGERID;

                    if (!string.IsNullOrEmpty(peManagerID))
                    {
                        var peManager = db.ACCOUNT.FirstOrDefault(x => x.ID == peManagerID);

                        if (peManager == null)
                        {
                            var pe = db.ACCOUNT.FirstOrDefault(x => x.ID == equipment.PEID);

                            if (pe != null)
                            {
                                peManagerID = pe.MANAGERID;
                            }
                            else
                            {
                                result.ReturnFailedMessage(string.Format("製程負責人[{0}]不存在", equipment.PEID));
                            }
                        }
                    }

                    if (result.IsSuccess)
                    {
                        //免校/遺失/報廢/維修/庫存
                        if (form.CHANGETYPE == "1" || form.CHANGETYPE == "2" || form.CHANGETYPE == "3" || form.CHANGETYPE == "4" || form.CHANGETYPE == "6")
                        {
                            //OwnerManager
                            if (log.FLOWSEQ == 1 || log.FLOWSEQ == 3)
                            {
                                db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                {
                                    FORMUNIQUEID = form.UNIQUEID,
                                    SEQ = seq,
                                    FLOWSEQ = 8,
                                    NOTIFYTIME = DateTime.Now,
                                    ISCANCELED = "N"
                                });

                                verifyList.Add("QA");
                            }

                            //QA
                            if (log.FLOWSEQ == 8)
                            {
                                form.STATUS = "3";

                                var abnormalForm = db.QA_ABNORMALFORM.FirstOrDefault(x => x.UNIQUEID == form.ABNORMALFORMUNIQUEID);

                                if (abnormalForm != null)
                                {
                                    if (abnormalForm.STATUS == "6")
                                    {
                                        abnormalForm.STATUS = "4";

                                        var calForm = db.QA_CALIBRATIONFORM.FirstOrDefault(x => x.UNIQUEID == abnormalForm.CALFORMUNIQUEID);

                                        if (calForm != null)
                                        {
                                            calForm.STATUS = "5";
                                        }
                                    }
                                }

                                var equipmentStatus = "";

                                if (form.CHANGETYPE == "1") { equipmentStatus = "3"; }
                                else if (form.CHANGETYPE == "2") { equipmentStatus = "4"; }
                                else if (form.CHANGETYPE == "3") { equipmentStatus = "5"; }
                                else if (form.CHANGETYPE == "4") { equipmentStatus = "6"; }
                                else if (form.CHANGETYPE == "6") { equipmentStatus = "7"; }

                                var equipmentList = (from x in db.QA_CHANGEFORMDETAIL
                                                     join e in db.QA_EQUIPMENT
                                                     on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                                     where x.FORMUNIQUEID == form.UNIQUEID
                                                     select e).ToList();

                                foreach (var e in equipmentList)
                                {
                                    e.STATUS = equipmentStatus;

                                    var calibrationFormList = (from f in db.QA_CALIBRATIONFORM
                                                               join n in db.QA_CALIBRATIONNOTIFY
                                                               on f.NOTIFYUNIQUEID equals n.UNIQUEID
                                                               where n.EQUIPMENTUNIQUEID == e.UNIQUEID && f.STATUS != "5"
                                                               select f).ToList();

                                    if (calibrationFormList != null && calibrationFormList.Count > 0)
                                    {
                                        foreach (var calibrationForm in calibrationFormList)
                                        {
                                            calibrationForm.STATUS = "9";

                                            var calibrationNotify = db.QA_CALIBRATIONNOTIFY.FirstOrDefault(x => x.UNIQUEID == calibrationForm.NOTIFYUNIQUEID);

                                            if (calibrationNotify != null)
                                            {
                                                calibrationNotify.STATUS = "4";
                                            }
                                        }
                                    }

                                    calibrationFormList = (from f in db.QA_CALIBRATIONFORM
                                                           join a in db.QA_CALIBRATIONAPPLY
                                                           on f.APPLYUNIQUEID equals a.UNIQUEID
                                                           where a.EQUIPMENTUNIQUEID == e.UNIQUEID && f.STATUS != "5"
                                                           select f).ToList();

                                    foreach (var calibrationForm in calibrationFormList)
                                    {
                                        calibrationForm.STATUS = "9";
                                    }

                                    var calibrationNotifyList = db.QA_CALIBRATIONNOTIFY.Where(x => x.EQUIPMENTUNIQUEID == e.UNIQUEID && x.STATUS != "3" && x.STATUS != "4").ToList();

                                    foreach (var calibrationNotify in calibrationNotifyList)
                                    {
                                        calibrationNotify.STATUS = "4";
                                    }

                                    var msaFormList = db.QA_MSAFORM.Where(x => x.STATUS != "5" && x.STATUS != "6" && x.EQUIPMENTUNIQUEID == e.UNIQUEID).ToList();

                                    if (msaFormList != null && msaFormList.Count > 0)
                                    {
                                        foreach (var msaForm in msaFormList)
                                        {
                                            msaForm.STATUS = "6";

                                            var msaNotify = db.QA_MSANOTIFY.Where(x => x.EQUIPMENTUNIQUEID == msaForm.EQUIPMENTUNIQUEID && x.ESTMSADATE == msaForm.ESTMSADATE).OrderByDescending(x => x.CREATETIME).FirstOrDefault();

                                            if (msaNotify != null)
                                            {
                                                msaNotify.STATUS = "4";
                                            }
                                        }
                                    }

                                    var msaNotifyList = db.QA_MSANOTIFY.Where(x => x.EQUIPMENTUNIQUEID == e.UNIQUEID && x.STATUS != "3" && x.STATUS != "4").ToList();

                                    foreach (var msaNotify in msaNotifyList)
                                    {
                                        msaNotify.STATUS = "4";
                                    }
                                }
                            }
                        }
                        //移轉
                        else if (form.CHANGETYPE == "5")
                        {
                            //OwnerManager/PEManager
                            if (log.FLOWSEQ == 1 || log.FLOWSEQ == 3)
                            {
                                if (!string.IsNullOrEmpty(form.NEWOWNERID))
                                {
                                    db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                    {
                                        FORMUNIQUEID = form.UNIQUEID,
                                        SEQ = seq,
                                        FLOWSEQ = 4,
                                        USERID = form.NEWOWNERID,
                                        NOTIFYTIME = DateTime.Now,
                                        ISCANCELED = "N"
                                    });

                                    verifyList.Add(form.NEWOWNERID);

                                    if (!string.IsNullOrEmpty(form.NEWPEID))
                                    {
                                        db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                        {
                                            FORMUNIQUEID = form.UNIQUEID,
                                            SEQ = seq + 1,
                                            FLOWSEQ = 6,
                                            USERID = form.NEWPEID,
                                            NOTIFYTIME = DateTime.Now,
                                            ISCANCELED = "N"
                                        });

                                        verifyList.Add(form.NEWPEID);
                                    }
                                }
                                else if (!string.IsNullOrEmpty(form.NEWPEID))
                                {
                                    db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                    {
                                        FORMUNIQUEID = form.UNIQUEID,
                                        SEQ = seq,
                                        FLOWSEQ = 6,
                                        USERID = form.NEWPEID,
                                        NOTIFYTIME = DateTime.Now,
                                        ISCANCELED = "N"
                                    });

                                    verifyList.Add(form.NEWPEID);
                                }
                            }
                            //NewOwner
                            else if (log.FLOWSEQ == 4)
                            {
                                db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                {
                                    FORMUNIQUEID = form.UNIQUEID,
                                    SEQ = seq,
                                    FLOWSEQ = 5,
                                    USERID = form.NEWOWNERMANAGERID,
                                    NOTIFYTIME = DateTime.Now,
                                    ISCANCELED = "N"
                                });

                                verifyList.Add(form.NEWOWNERMANAGERID);
                            }
                            //NewOwnerManager
                            else if (log.FLOWSEQ == 5)
                            {
                                if (!string.IsNullOrEmpty(form.NEWPEMANAGERID))
                                {
                                    if (db.QA_CHANGEFORMFLOWLOG.Any(x => x.FORMUNIQUEID == form.UNIQUEID && x.FLOWSEQ == 7 && x.ISCANCELED == "N" && x.VERIFYTIME.HasValue && x.VERIFYRESULT == "Y"))
                                    {
                                        db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                        {
                                            FORMUNIQUEID = form.UNIQUEID,
                                            SEQ = seq,
                                            FLOWSEQ = 8,
                                            NOTIFYTIME = DateTime.Now,
                                            ISCANCELED = "N"
                                        });

                                        verifyList.Add("QA");
                                    }
                                }
                                else
                                {
                                    db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                    {
                                        FORMUNIQUEID = form.UNIQUEID,
                                        SEQ = seq,
                                        FLOWSEQ = 8,
                                        NOTIFYTIME = DateTime.Now,
                                        ISCANCELED = "N"
                                    });

                                    verifyList.Add("QA");
                                }
                            }
                            //NewPE
                            else if (log.FLOWSEQ == 6)
                            {
                                db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                {
                                    FORMUNIQUEID = form.UNIQUEID,
                                    SEQ = seq,
                                    FLOWSEQ = 7,
                                    USERID = form.NEWPEMANAGERID,
                                    NOTIFYTIME = DateTime.Now,
                                    ISCANCELED = "N"
                                });

                                verifyList.Add(form.NEWPEMANAGERID);
                            }
                            //NewPEManager
                            else if (log.FLOWSEQ == 7)
                            {
                                if (!string.IsNullOrEmpty(form.NEWOWNERMANAGERID))
                                {
                                    if (db.QA_CHANGEFORMFLOWLOG.Any(x => x.FORMUNIQUEID == form.UNIQUEID && x.FLOWSEQ == 5 && x.ISCANCELED == "N" && x.VERIFYTIME.HasValue && x.VERIFYRESULT == "Y"))
                                    {
                                        db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                        {
                                            FORMUNIQUEID = form.UNIQUEID,
                                            SEQ = seq,
                                            FLOWSEQ = 8,
                                            NOTIFYTIME = DateTime.Now,
                                            ISCANCELED = "N"
                                        });

                                        verifyList.Add("QA");
                                    }
                                }
                                else
                                {
                                    db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                    {
                                        FORMUNIQUEID = form.UNIQUEID,
                                        SEQ = seq,
                                        FLOWSEQ = 8,
                                        NOTIFYTIME = DateTime.Now,
                                        ISCANCELED = "N"
                                    });

                                    verifyList.Add("QA");
                                }
                            }
                            //QA
                            else if (log.FLOWSEQ == 8)
                            {
                                form.STATUS = "3";

                                var abnormalForm = db.QA_ABNORMALFORM.FirstOrDefault(x => x.UNIQUEID == form.ABNORMALFORMUNIQUEID);

                                if (abnormalForm != null)
                                {
                                    if (abnormalForm.STATUS == "6")
                                    {
                                        abnormalForm.STATUS = "4";

                                        var calForm = db.QA_CALIBRATIONFORM.FirstOrDefault(x => x.UNIQUEID == abnormalForm.CALFORMUNIQUEID);

                                        if (calForm != null)
                                        {
                                            calForm.STATUS = "5";
                                        }
                                    }
                                }

                                var equipmentList = (from x in db.QA_CHANGEFORMDETAIL
                                                     join e in db.QA_EQUIPMENT
                                                     on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                                     where x.FORMUNIQUEID == form.UNIQUEID
                                                     select e).ToList();

                                foreach (var e in equipmentList)
                                {
                                    e.ORGANIZATIONUNIQUEID = "";

                                    if (!string.IsNullOrEmpty(form.NEWOWNERID))
                                    {
                                        var owner = db.ACCOUNT.FirstOrDefault(x => x.ID == form.NEWOWNERID);

                                        if (owner != null)
                                        {
                                            e.ORGANIZATIONUNIQUEID = owner.ORGANIZATIONUNIQUEID;
                                        }
                                    }

                                    if (string.IsNullOrEmpty(e.ORGANIZATIONUNIQUEID))
                                    {
                                        if (!string.IsNullOrEmpty(form.NEWPEID))
                                        {
                                            var pe = db.ACCOUNT.FirstOrDefault(x => x.ID == form.NEWPEID);

                                            if (pe != null)
                                            {
                                                e.ORGANIZATIONUNIQUEID = pe.ORGANIZATIONUNIQUEID;
                                            }
                                        }
                                    }

                                    e.OWNERID = form.NEWOWNERID;
                                    e.OWNERMANAGERID = form.NEWOWNERMANAGERID;
                                    e.PEID = form.NEWPEID;
                                    e.PEMANAGERID = form.NEWPEMANAGERID;

                                    var calibrationFormList = (from f in db.QA_CALIBRATIONFORM
                                                               join n in db.QA_CALIBRATIONNOTIFY
                                                               on f.NOTIFYUNIQUEID equals n.UNIQUEID
                                                               where n.EQUIPMENTUNIQUEID == e.UNIQUEID && f.STATUS != "5"
                                                               select f).ToList();

                                    if (calibrationFormList != null && calibrationFormList.Count > 0)
                                    {
                                        foreach (var calibrationForm in calibrationFormList)
                                        {
                                            calibrationForm.STATUS = "9";

                                            var calibrationNotify = db.QA_CALIBRATIONNOTIFY.FirstOrDefault(x => x.UNIQUEID == calibrationForm.NOTIFYUNIQUEID);

                                            if (calibrationNotify != null)
                                            {
                                                calibrationNotify.STATUS = "4";
                                            }
                                        }
                                    }

                                    calibrationFormList = (from f in db.QA_CALIBRATIONFORM
                                                           join a in db.QA_CALIBRATIONAPPLY
                                                           on f.APPLYUNIQUEID equals a.UNIQUEID
                                                           where a.EQUIPMENTUNIQUEID == e.UNIQUEID && f.STATUS != "5"
                                                           select f).ToList();

                                    foreach (var calibrationForm in calibrationFormList)
                                    {
                                        calibrationForm.STATUS = "9";
                                    }

                                    var calibrationNotifyList = db.QA_CALIBRATIONNOTIFY.Where(x => x.EQUIPMENTUNIQUEID == e.UNIQUEID && x.STATUS != "3" && x.STATUS != "4").ToList();

                                    foreach (var calibrationNotify in calibrationNotifyList)
                                    {
                                        calibrationNotify.STATUS = "4";
                                    }

                                    var msaFormList = db.QA_MSAFORM.Where(x => x.STATUS != "5" && x.STATUS != "6" && x.EQUIPMENTUNIQUEID == e.UNIQUEID).ToList();

                                    if (msaFormList != null && msaFormList.Count > 0)
                                    {
                                        foreach (var msaForm in msaFormList)
                                        {
                                            msaForm.STATUS = "6";

                                            var msaNotify = db.QA_MSANOTIFY.Where(x => x.EQUIPMENTUNIQUEID == msaForm.EQUIPMENTUNIQUEID && x.ESTMSADATE == msaForm.ESTMSADATE).OrderByDescending(x => x.CREATETIME).FirstOrDefault();

                                            if (msaNotify != null)
                                            {
                                                msaNotify.STATUS = "4";
                                            }
                                        }
                                    }

                                    var msaNotifyList = db.QA_MSANOTIFY.Where(x => x.EQUIPMENTUNIQUEID == e.UNIQUEID && x.STATUS != "3" && x.STATUS != "4").ToList();

                                    foreach (var msaNotify in msaNotifyList)
                                    {
                                        msaNotify.STATUS = "4";
                                    }
                                }
                            }
                        }
                        //復用
                        else if (form.CHANGETYPE == "7" || form.CHANGETYPE == "B" || form.CHANGETYPE == "C")
                        {
                            //OwnerManager
                            if (log.FLOWSEQ == 1)
                            {
                                if (!string.IsNullOrEmpty(peManagerID))
                                {
                                    if (db.QA_CHANGEFORMFLOWLOG.Any(x => x.FORMUNIQUEID == form.UNIQUEID && x.FLOWSEQ == 3 && x.ISCANCELED == "N" && x.VERIFYTIME.HasValue && x.VERIFYRESULT == "Y"))
                                    {
                                        db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                        {
                                            FORMUNIQUEID = form.UNIQUEID,
                                            SEQ = seq,
                                            FLOWSEQ = 8,
                                            NOTIFYTIME = DateTime.Now,
                                            ISCANCELED = "N"
                                        });

                                        verifyList.Add("QA");
                                    }
                                }
                                else
                                {
                                    db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                    {
                                        FORMUNIQUEID = form.UNIQUEID,
                                        SEQ = seq,
                                        FLOWSEQ = 8,
                                        NOTIFYTIME = DateTime.Now,
                                        ISCANCELED = "N"
                                    });

                                    verifyList.Add("QA");
                                }
                            }
                            //PE
                            else if (log.FLOWSEQ == 2)
                            {
                                db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                {
                                    FORMUNIQUEID = form.UNIQUEID,
                                    SEQ = seq,
                                    FLOWSEQ = 3,
                                    USERID = peManagerID,
                                    NOTIFYTIME = DateTime.Now,
                                    ISCANCELED = "N"
                                });

                                verifyList.Add(peManagerID);
                            }
                            //PEManager
                            else if (log.FLOWSEQ == 3)
                            {
                                if (!string.IsNullOrEmpty(ownerManagerID))
                                {
                                    if (db.QA_CHANGEFORMFLOWLOG.Any(x => x.FORMUNIQUEID == form.UNIQUEID && x.FLOWSEQ == 1 && x.ISCANCELED == "N" && x.VERIFYTIME.HasValue && x.VERIFYRESULT == "Y"))
                                    {
                                        db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                        {
                                            FORMUNIQUEID = form.UNIQUEID,
                                            SEQ = seq,
                                            FLOWSEQ = 8,
                                            NOTIFYTIME = DateTime.Now,
                                            ISCANCELED = "N"
                                        });

                                        verifyList.Add("QA");
                                    }
                                }
                                else
                                {
                                    db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                    {
                                        FORMUNIQUEID = form.UNIQUEID,
                                        SEQ = seq,
                                        FLOWSEQ = 8,
                                        NOTIFYTIME = DateTime.Now,
                                        ISCANCELED = "N"
                                    });

                                    verifyList.Add("QA");
                                }
                            }
                            //QA
                            else if (log.FLOWSEQ == 8)
                            {
                                form.STATUS = "3";

                                var abnormalForm = db.QA_ABNORMALFORM.FirstOrDefault(x => x.UNIQUEID == form.ABNORMALFORMUNIQUEID);

                                if (abnormalForm != null)
                                {
                                    if (abnormalForm.STATUS == "6")
                                    {
                                        abnormalForm.STATUS = "4";

                                        var calForm = db.QA_CALIBRATIONFORM.FirstOrDefault(x => x.UNIQUEID == abnormalForm.CALFORMUNIQUEID);

                                        if (calForm != null)
                                        {
                                            calForm.STATUS = "5";
                                        }
                                    }
                                }

                                var equipmentList = (from x in db.QA_CHANGEFORMDETAIL
                                                     join e in db.QA_EQUIPMENT
                                                     on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                                     where x.FORMUNIQUEID == form.UNIQUEID
                                                     select e).ToList();

                                foreach (var e in equipmentList)
                                {
                                    e.STATUS = "1";

                                    if (form.CHANGETYPE == "7")
                                    {
                                        e.CAL = "Y";
                                        e.NEXTCALDATE = DateTime.Today.AddDays(7);

                                        if (e.MSA == "Y")
                                        {
                                            e.NEXTMSADATE = DateTime.Today.AddMonths(1);
                                        }
                                    }

                                    if (form.CHANGETYPE == "B")
                                    {
                                        e.CAL = "Y";
                                        e.NEXTCALDATE = form.FIXFINISHEDDATE;
                                        e.CALCYCLE = form.NEWCYCLE;
                                    }

                                    if (form.CHANGETYPE == "C")
                                    {
                                        e.MSA = "Y";
                                        e.NEXTMSADATE = form.FIXFINISHEDDATE;
                                        e.MSACYCLE = form.NEWCYCLE;
                                    }
                                }
                            }
                        }
                        //免MSA
                        else if (form.CHANGETYPE == "8")
                        {
                            //OwnerManager
                            if (log.FLOWSEQ == 1)
                            {
                                db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                {
                                    FORMUNIQUEID = form.UNIQUEID,
                                    SEQ = seq,
                                    FLOWSEQ = 8,
                                    NOTIFYTIME = DateTime.Now,
                                    ISCANCELED = "N"
                                });

                                verifyList.Add("QA");

                                //if (!string.IsNullOrEmpty(peManagerID))
                                //{
                                //    if (db.QA_CHANGEFORMFLOWLOG.Any(x => x.FORMUNIQUEID == form.UNIQUEID && x.FLOWSEQ == 3 && x.ISCANCELED == "N" && x.VERIFYTIME.HasValue && x.VERIFYRESULT == "Y"))
                                //    {
                                //        db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                //        {
                                //            FORMUNIQUEID = form.UNIQUEID,
                                //            SEQ = seq,
                                //            FLOWSEQ = 8,
                                //            NOTIFYTIME = DateTime.Now,
                                //            ISCANCELED = "N"
                                //        });

                                //        verifyList.Add("QA");
                                //    }
                                //}
                                //else
                                //{
                                    
                                //}
                            }
                            //PE
                            else if (log.FLOWSEQ == 2)
                            {
                                db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                {
                                    FORMUNIQUEID = form.UNIQUEID,
                                    SEQ = seq,
                                    FLOWSEQ = 3,
                                    USERID = peManagerID,
                                    NOTIFYTIME = DateTime.Now,
                                    ISCANCELED = "N"
                                });

                                verifyList.Add(peManagerID);
                            }
                            //PEManager
                            else if (log.FLOWSEQ == 3)
                            {
                                db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                {
                                    FORMUNIQUEID = form.UNIQUEID,
                                    SEQ = seq,
                                    FLOWSEQ = 8,
                                    NOTIFYTIME = DateTime.Now,
                                    ISCANCELED = "N"
                                });

                                verifyList.Add("QA");

                                //if (!string.IsNullOrEmpty(ownerManagerID))
                                //{
                                //    if (db.QA_CHANGEFORMFLOWLOG.Any(x => x.FORMUNIQUEID == form.UNIQUEID && x.FLOWSEQ == 1 && x.ISCANCELED == "N" && x.VERIFYTIME.HasValue && x.VERIFYRESULT == "Y"))
                                //    {
                                //        db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                //        {
                                //            FORMUNIQUEID = form.UNIQUEID,
                                //            SEQ = seq,
                                //            FLOWSEQ = 8,
                                //            NOTIFYTIME = DateTime.Now,
                                //            ISCANCELED = "N"
                                //        });

                                //        verifyList.Add("QA");
                                //    }
                                //}
                                //else
                                //{
                                    
                                //}
                            }
                            //QA
                            else if (log.FLOWSEQ == 8)
                            {
                                form.STATUS = "3";

                                var equipmentList = (from x in db.QA_CHANGEFORMDETAIL
                                                     join e in db.QA_EQUIPMENT
                                                     on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                                     where x.FORMUNIQUEID == form.UNIQUEID
                                                     select e).ToList();

                                foreach (var e in equipmentList)
                                {
                                    e.MSA = "N";

                                    var formList = db.QA_MSAFORM.Where(x => x.EQUIPMENTUNIQUEID == e.UNIQUEID && x.STATUS != "5" && x.STATUS != "6").ToList();

                                    foreach (var f in formList)
                                    {
                                        f.STATUS = "6";
                                    }

                                    var notifyList = db.QA_MSANOTIFY.Where(x => x.EQUIPMENTUNIQUEID == e.UNIQUEID && x.STATUS != "3" && x.STATUS != "4").ToList();

                                    foreach (var n in notifyList)
                                    {
                                        n.STATUS = "4";
                                    }
                                }
                            }
                        }
                        //變更校正週期
                        else if (form.CHANGETYPE == "9")
                        {
                            //OwnerManager
                            if (log.FLOWSEQ == 1)
                            {
                                db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                {
                                    FORMUNIQUEID = form.UNIQUEID,
                                    SEQ = seq,
                                    FLOWSEQ = 8,
                                    NOTIFYTIME = DateTime.Now,
                                    ISCANCELED = "N"
                                });

                                verifyList.Add("QA");

                                //if (!string.IsNullOrEmpty(peManagerID))
                                //{
                                //    if (db.QA_CHANGEFORMFLOWLOG.Any(x => x.FORMUNIQUEID == form.UNIQUEID && x.FLOWSEQ == 3 && x.ISCANCELED == "N" && x.VERIFYTIME.HasValue && x.VERIFYRESULT == "Y"))
                                //    {
                                //        db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                //        {
                                //            FORMUNIQUEID = form.UNIQUEID,
                                //            SEQ = seq,
                                //            FLOWSEQ = 8,
                                //            NOTIFYTIME = DateTime.Now,
                                //            ISCANCELED = "N"
                                //        });

                                //        verifyList.Add("QA");
                                //    }
                                //}
                                //else
                                //{
                                    
                                //}
                            }
                            //PE
                            else if (log.FLOWSEQ == 2)
                            {
                                db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                {
                                    FORMUNIQUEID = form.UNIQUEID,
                                    SEQ = seq,
                                    FLOWSEQ = 3,
                                    USERID = peManagerID,
                                    NOTIFYTIME = DateTime.Now,
                                    ISCANCELED = "N"
                                });

                                verifyList.Add(peManagerID);
                            }
                            //PEManager
                            else if (log.FLOWSEQ == 3)
                            {
                                db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                {
                                    FORMUNIQUEID = form.UNIQUEID,
                                    SEQ = seq,
                                    FLOWSEQ = 8,
                                    NOTIFYTIME = DateTime.Now,
                                    ISCANCELED = "N"
                                });

                                verifyList.Add("QA");

                                //if (!string.IsNullOrEmpty(ownerManagerID))
                                //{
                                //    if (db.QA_CHANGEFORMFLOWLOG.Any(x => x.FORMUNIQUEID == form.UNIQUEID && x.FLOWSEQ == 1 && x.ISCANCELED == "N" && x.VERIFYTIME.HasValue && x.VERIFYRESULT == "Y"))
                                //    {
                                //        db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                //        {
                                //            FORMUNIQUEID = form.UNIQUEID,
                                //            SEQ = seq,
                                //            FLOWSEQ = 8,
                                //            NOTIFYTIME = DateTime.Now,
                                //            ISCANCELED = "N"
                                //        });

                                //        verifyList.Add("QA");
                                //    }
                                //}
                                //else
                                //{
                                    
                                //}
                            }
                            //QA
                            else if (log.FLOWSEQ == 8)
                            {
                                form.STATUS = "3";

                                var equipmentList = (from x in db.QA_CHANGEFORMDETAIL
                                                     join e in db.QA_EQUIPMENT
                                                     on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                                     where x.FORMUNIQUEID == form.UNIQUEID
                                                     select e).ToList();

                                foreach (var e in equipmentList)
                                {
                                    e.CALCYCLE = form.NEWCYCLE;

                                    if (e.LASTCALDATE.HasValue)
                                    {
                                        e.NEXTCALDATE = e.LASTCALDATE.Value.AddMonths(form.NEWCYCLE.Value).AddDays(-1);
                                    }

                                    var calibrationFormList = (from f in db.QA_CALIBRATIONFORM
                                                               join n in db.QA_CALIBRATIONNOTIFY
                                                               on f.NOTIFYUNIQUEID equals n.UNIQUEID
                                                               where n.EQUIPMENTUNIQUEID == equipment.UNIQUEID && f.STATUS != "5"
                                                               select f).ToList();

                                    if (calibrationFormList != null && calibrationFormList.Count > 0)
                                    {
                                        foreach (var calibrationForm in calibrationFormList)
                                        {
                                            calibrationForm.STATUS = "9";

                                            var calibrationNotify = db.QA_CALIBRATIONNOTIFY.FirstOrDefault(x => x.UNIQUEID == calibrationForm.NOTIFYUNIQUEID);

                                            if (calibrationNotify != null)
                                            {
                                                calibrationNotify.STATUS = "4";
                                            }
                                        }
                                    }

                                    calibrationFormList = (from f in db.QA_CALIBRATIONFORM
                                                           join a in db.QA_CALIBRATIONAPPLY
                                                           on f.APPLYUNIQUEID equals a.UNIQUEID
                                                           where a.EQUIPMENTUNIQUEID == equipment.UNIQUEID && f.STATUS != "5"
                                                           select f).ToList();

                                    foreach (var calibrationForm in calibrationFormList)
                                    {
                                        calibrationForm.STATUS = "9";
                                    }

                                    var calibrationNotifyList = db.QA_CALIBRATIONNOTIFY.Where(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.STATUS != "3" && x.STATUS != "4").ToList();

                                    foreach (var calibrationNotify in calibrationNotifyList)
                                    {
                                        calibrationNotify.STATUS = "4";
                                    }
                                }
                            }
                        }
                        //變更MSA週期
                        else if (form.CHANGETYPE == "A")
                        {
                            //OwnerManager
                            if (log.FLOWSEQ == 1)
                            {
                                //if (!string.IsNullOrEmpty(peManagerID))
                                //{
                                //    if (db.QA_CHANGEFORMFLOWLOG.Any(x => x.FORMUNIQUEID == form.UNIQUEID && x.FLOWSEQ == 3 && x.ISCANCELED == "N" && x.VERIFYTIME.HasValue && x.VERIFYRESULT == "Y"))
                                //    {
                                //        db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                //        {
                                //            FORMUNIQUEID = form.UNIQUEID,
                                //            SEQ = seq,
                                //            FLOWSEQ = 8,
                                //            NOTIFYTIME = DateTime.Now,
                                //            ISCANCELED = "N"
                                //        });

                                //        verifyList.Add("QA");
                                //    }
                                //}
                                //else
                                //{
                                    
                                //}

                                db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                {
                                    FORMUNIQUEID = form.UNIQUEID,
                                    SEQ = seq,
                                    FLOWSEQ = 8,
                                    NOTIFYTIME = DateTime.Now,
                                    ISCANCELED = "N"
                                });

                                verifyList.Add("QA");
                            }
                            //PE
                            else if (log.FLOWSEQ == 2)
                            {
                                db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                {
                                    FORMUNIQUEID = form.UNIQUEID,
                                    SEQ = seq,
                                    FLOWSEQ = 3,
                                    USERID = peManagerID,
                                    NOTIFYTIME = DateTime.Now,
                                    ISCANCELED = "N"
                                });

                                verifyList.Add(peManagerID);
                            }
                            //PEManager
                            else if (log.FLOWSEQ == 3)
                            {
                                //if (!string.IsNullOrEmpty(ownerManagerID))
                                //{
                                //    if (db.QA_CHANGEFORMFLOWLOG.Any(x => x.FORMUNIQUEID == form.UNIQUEID && x.FLOWSEQ == 1 && x.ISCANCELED == "N" && x.VERIFYTIME.HasValue && x.VERIFYRESULT == "Y"))
                                //    {
                                //        db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                //        {
                                //            FORMUNIQUEID = form.UNIQUEID,
                                //            SEQ = seq,
                                //            FLOWSEQ = 8,
                                //            NOTIFYTIME = DateTime.Now,
                                //            ISCANCELED = "N"
                                //        });

                                //        verifyList.Add("QA");
                                //    }
                                //}
                                //else
                                //{
                                    
                                //}

                                db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                                {
                                    FORMUNIQUEID = form.UNIQUEID,
                                    SEQ = seq,
                                    FLOWSEQ = 8,
                                    NOTIFYTIME = DateTime.Now,
                                    ISCANCELED = "N"
                                });

                                verifyList.Add("QA");
                            }
                            //QA
                            else if (log.FLOWSEQ == 8)
                            {
                                form.STATUS = "3";

                                var equipmentList = (from x in db.QA_CHANGEFORMDETAIL
                                                     join e in db.QA_EQUIPMENT
                                                     on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                                     where x.FORMUNIQUEID == form.UNIQUEID
                                                     select e).ToList();

                                foreach (var e in equipmentList)
                                {
                                    e.MSACYCLE = form.NEWCYCLE;

                                    if (e.LASTMSADATE.HasValue)
                                    {
                                        e.NEXTMSADATE = e.LASTMSADATE.Value.AddMonths(form.NEWCYCLE.Value).AddDays(-1);
                                    }

                                    var formList = db.QA_MSAFORM.Where(x => x.EQUIPMENTUNIQUEID == e.UNIQUEID && x.STATUS != "5" && x.STATUS != "6").ToList();

                                    foreach (var f in formList)
                                    {
                                        f.STATUS = "6";
                                    }

                                    var notifyList = db.QA_MSANOTIFY.Where(x => x.EQUIPMENTUNIQUEID == e.UNIQUEID && x.STATUS != "3" && x.STATUS != "4").ToList();

                                    foreach (var n in notifyList)
                                    {
                                        n.STATUS = "4";
                                    }
                                }
                            }
                        }

                        db.SaveChanges();

                        foreach (var verify in verifyList)
                        {
                            if (verify == "QA")
                            {
                                SendVerifyMail(form.UNIQUEID, form.VHNO);
                            }
                            else
                            {
                                SendVerifyMail(form.UNIQUEID, form.VHNO, verify);
                            }
                        }

                        result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Verify, Resources.Resource.Success));
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

        public static RequestResult Reject(string UniqueID, int Seq, string Comment, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = db.QA_CHANGEFORM.First(x => x.UNIQUEID == UniqueID);

                    form.STATUS = "2";

                    var detail = db.QA_CHANGEFORMDETAIL.Where(x => x.FORMUNIQUEID == form.UNIQUEID).First();

                    var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == detail.EQUIPMENTUNIQUEID);

                    var log = db.QA_CHANGEFORMFLOWLOG.First(x => x.FORMUNIQUEID == UniqueID && x.SEQ == Seq);

                    log.USERID = Account.ID;
                    log.VERIFYTIME = DateTime.Now;
                    log.VERIFYRESULT = "N";
                    log.VERIFYCOMMENT = Comment;

                    var logList = db.QA_CHANGEFORMFLOWLOG.Where(x => x.FORMUNIQUEID == UniqueID && x.SEQ != Seq && !x.VERIFYTIME.HasValue).ToList();

                    foreach (var l in logList)
                    {
                        l.ISCANCELED = "Y";
                    }

                    //var seq = db.QA_CHANGEFORMFLOWLOG.Where(x => x.FORMUNIQUEID == form.UNIQUEID).Max(x => x.SEQ) + 1;

                    //db.QA_CHANGEFORMFLOWLOG.Add(new QA_CHANGEFORMFLOWLOG()
                    //{
                    //    FORMUNIQUEID = form.UNIQUEID,
                    //    SEQ = seq,
                    //    FLOWSEQ = 0,
                    //    NOTIFYTIME = DateTime.Now,
                    //    USERID = equipment.OWNERID,
                    //    ISCANCELED = "N"
                    //});

                    SendRejectMail(form.UNIQUEID, form.VHNO, new List<string>() { equipment.OWNERID });

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

        public static void SendRemindMail(string UniqueID, string VHNO,int Days, string UserID)
        {
            SendVerifyMail(UniqueID, string.Format("[逾期][簽核通知][{0}天][{1}]儀器狀態異動申請單",Days, VHNO), new List<string>() { UserID });
        }

        public static void SendVerifyMail(string UniqueID, string VHNO, string UserID)
        {
            SendVerifyMail(UniqueID, string.Format("[簽核通知][{0}]儀器狀態異動申請單", VHNO), new List<string>() { UserID });
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
                        SendVerifyMail(UniqueID, string.Format("[逾期][簽核通知][{0}][{1}]儀器狀態異動申請單",Days, VHNO), userList);
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
                        SendVerifyMail(UniqueID, string.Format("[簽核通知][{0}]儀器狀態異動申請單", VHNO), userList);
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
                    var query = GetDetailViewModel(UniqueID);

                    var model = query.Data as DetailViewModel;

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
                        sb.Append(string.Format(td, model.VHNO));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "部門"));
                        sb.Append(string.Format(td, model.OrganizationDescription));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "異動類別"));
                        sb.Append(string.Format(td, model.ChangeTypeDescription));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "申請時間"));
                        sb.Append(string.Format(td, model.CreateTimeString));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "連結"));
                        sb.Append(string.Format(td, string.Format("<a href=\"http://ASECL-eINSPRD01/FEM.Portal/Home/Index?ReturnUrl=http://ASECL-eINSPRD01/FEM/zh-tw/Customized_ASE_QA/ChangeForm/Index?VHNO={0}\">連結</a>", model.VHNO)));
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
            SendVerifyMail(UniqueID, string.Format("[退回修正通知][{0}]儀器狀態異動申請單", VHNO), UserList);
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
                    var query = (from f in db.QA_CHANGEFORM
                                 join d in db.QA_CHANGEFORMDETAIL
                                 on f.UNIQUEID equals d.FORMUNIQUEID
                                 join e in db.QA_EQUIPMENT
                                 on d.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                 join calibrator in db.ACCOUNT
                                 on e.LASTCALUSERID equals calibrator.ID into tmpCalibrator
                                 from calibrator in tmpCalibrator.DefaultIfEmpty()
                                 where UniqueIDList.Contains(f.UNIQUEID) && (f.CHANGETYPE == "3" || f.CHANGETYPE == "4" || f.CHANGETYPE == "6" || f.CHANGETYPE == "7")
                                 select new
                                 {
                                     UniqueID = f.UNIQUEID,
                                     EquipmentUniqueID = e.UNIQUEID,
                                     ChangeType = f.CHANGETYPE,
                                     LastCalibrateDate = e.LASTCALDATE,
                                     NextCalibrateDate = e.NEXTCALDATE,
                                     CalNo = e.CALNO,
                                     CalibratorID = e.LASTCALUSERID,
                                     CalibratorName = calibrator != null ? calibrator.NAME : ""
                                 }).ToList();

                    foreach (var q in query)
                    {
                        //var sign = q.CalibratorID;

                        //var a = UserList.FirstOrDefault(x => x.ID == q.CalibratorID);

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
                            SN = q.CalNo,
                            CALDate = DateTimeHelper.DateTime2DateStringWithSeperator(q.LastCalibrateDate),
                            DueDate = DateTimeHelper.DateTime2DateStringWithSeperator(q.NextCalibrateDate),
                            Sign = q.CalibratorName,
                            //Sign = sign,
                            IsFailed = q.ChangeType == "7" ? false : true
                        });
                    }

                    var formUniqueIDList = query.Select(x => x.UniqueID).Distinct().ToList();

                    var formList = db.QA_CHANGEFORM.Where(x => formUniqueIDList.Contains(x.UNIQUEID)).ToList();

                    foreach (var form in formList)
                    {
                        form.ISQRCODE = "Y";
                    }

                    db.SaveChanges();
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
