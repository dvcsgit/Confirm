using DbEntity.ASE;
using Models.ASE.AbnormalNotify;
using Models.Authenticated;
using Models.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace DataAccess.ASE
{
    public class AbnormalNotifyDataAccessor
    {
        public static RequestResult Query(Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new SummaryViewModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = db.FAC_ABNORMALNOTIFYFORM.Where(x => x.STATUS == "1" || x.STATUS == "2").ToList();

                    model.ItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-exclamation-circle",
                        Count = query.Count(x => x.STATUS == "1"),
                        Text = Resources.Resource.AbnormalNotifyStatus_1,
                        Status = 1
                    });

                    model.ItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-blue",
                        Icon = "fa-wrench",
                        Count = query.Count(x => x.STATUS == "2"),
                        Text = Resources.Resource.AbnormalNotifyStatus_2,
                        Status = 2
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

        public static RequestResult Query(QueryParameters Parameters,List<Models.Shared.Organization> OrganizationList, List<Models.Shared.UserModel> UserList)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new GridViewModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = db.FAC_ABNORMALNOTIFYFORM.AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.BeginDate))
                    {
                        query = query.Where(x => string.Compare(x.OCCURDATE, Parameters.BeginDate) >= 0);
                    }

                    if (!string.IsNullOrEmpty(Parameters.EndDate))
                    {
                        query = query.Where(x => string.Compare(x.OCCURDATE, Parameters.EndDate) <= 0);
                    }

                    if (!string.IsNullOrEmpty(Parameters.VHNO))
                    {
                        query = query.Where(x => x.VHNO.Contains(Parameters.VHNO));
                    }

                    if (!string.IsNullOrEmpty(Parameters.Status))
                    {
                        query = query.Where(x => x.STATUS == Parameters.Status);
                    }

                    var temp = query.OrderByDescending(x => x.VHNO).ToList();

                    foreach (var t in temp)
                    {
                        var createUser = UserList.FirstOrDefault(x => x.ID == t.CREATEUSERID);
                        var takeJobUser = UserList.FirstOrDefault(x => x.ID == t.TAKEJOBUSERID);

                        model.ItemList.Add(new GridItem()
                        {
                            UniqueID = t.UNIQUEID,
                            VHNO = t.VHNO,
                            Status = t.STATUS,
                            Subject = t.SUBJECT,
                            OccurTime = DateTimeHelper.DateTimeString2DateTime(t.OCCURDATE, t.OCCURTIME),
                            TakeJobTime = t.TAKEJOBTIME,
                            TakeJobUserID = t.TAKEJOBUSERID,
                            TakeJobUserName = takeJobUser != null ? takeJobUser.Name : string.Empty,
                            ResponsibleOrganization = OrganizationDataAccessor.GetOrganizationDescription(t.RESORGANIZATIONUNIQUEID),
                            CreateUserID = t.CREATEUSERID,
                            CreateUserName = createUser != null ? createUser.Name : string.Empty,
                            CreateTime = t.CREATETIME,
                            ClosedTime = t.CLOSEDTIME,
                            ResponsibleOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(OrganizationList, t.RESORGANIZATIONUNIQUEID, true)
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

        public static RequestResult GetDetailViewModel(string UniqueID, List<Models.Shared.Organization> OrganizationList, List<Models.Shared.UserModel> UserList)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new DetailViewModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = db.FAC_ABNORMALNOTIFYFORM.First(x => x.UNIQUEID == UniqueID);

                    var createUser = UserList.FirstOrDefault(x => x.ID == form.CREATEUSERID);
                    var takeJobUser = UserList.FirstOrDefault(x => x.ID == form.TAKEJOBUSERID);
                    var resOrganization = OrganizationList.FirstOrDefault(x => x.UniqueID == form.RESORGANIZATIONUNIQUEID);

                    model = new DetailViewModel()
                    {
                        UniqueID = form.UNIQUEID,
                        VHNO = form.VHNO,
                        ResponsibleOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(OrganizationList, form.RESORGANIZATIONUNIQUEID, true),
                        Cost = form.COST,
                        Location = form.LOCATION,
                        CreateTime = form.CREATETIME,
                        CreateUserID = form.CREATEUSERID,
                        CreateUserName = createUser != null ? createUser.Name : string.Empty,
                        ClosedRemark = form.CLOSEDREMARK,
                        TakeJobTime = form.TAKEJOBTIME,
                        TakeJobUserID = form.TAKEJOBUSERID,
                        TakeJobUserName = takeJobUser != null ? takeJobUser.Name : string.Empty,
                        Description = form.DESCRIPTION,
                        EffectArea = form.EFFECTAREA,
                        EffectSystem = form.EFFECTSYSTEM,
                        FileList = db.FAC_ABNORMALNOTIFYFORMFILE.Where(f => f.FORMUNIQUEID == UniqueID).ToList().Select(f => new FileModel
                        {
                            IsSaved = true,
                            Seq = f.SEQ,
                            FileName = f.FILENAME,
                            Extension = f.EXTENSION,
                            Size = f.S.Value,
                            LastModifyTime = f.LASTMODIFYTIME.Value
                        }).OrderBy(f => f.LastModifyTime).ToList(),
                        HandlingDescription = form.HANDLINGDESCRIPTION,
                        Mvpn = form.MVPN,
                        OccurTime = DateTimeHelper.DateTimeString2DateTime(form.OCCURDATE, form.OCCURTIME),
                        RecoveryDescription = form.RECOVERYDESCRIPTION,
                        RecoveryTime = DateTimeHelper.DateTimeString2DateTime(form.RECOVERYDATE, form.RECOVERYTIME),
                        ResponsibleOrganization = resOrganization != null ? resOrganization.Description : string.Empty,
                        Contact = form.CONTACT,
                        GroupList = (from x in db.FAC_ABNORMALNOTIFYFORMGROUP
                                     join g in db.FAC_ABNORMALNOTIFYGROUP
                                     on x.GROUPUNIQUEID equals g.UNIQUEID
                                     where x.FORMUNIQUEID == form.UNIQUEID
                                     select g.DESCRIPTION).OrderBy(x => x).ToList(),
                        Status = form.STATUS,
                        Subject = form.SUBJECT,
                        LogList = (from x in db.FAC_ABNORMALNOTIFYFORMLOG
                                   join u in db.ACCOUNT
                                   on x.USERID equals u.ID
                                   where x.FORMUNIQUEID == form.UNIQUEID
                                   select new
                                   {
                                       x.ACTION,
                                       x.LOGTIME,
                                       x.USERID,
                                       USERNAME = u.NAME,
                                       x.SEQ
                                   }).ToList().Select(x => new LogModel
                                   {
                                       Action = Define.EnumParse<Define.EnumFormAction>(x.ACTION),
                                       LogTime = x.LOGTIME,
                                       UserID = x.USERID,
                                       UserName = x.USERNAME,
                                       Seq = x.SEQ
                                   }).OrderBy(x => x.Seq).ToList(),
                        NotifyUserList = (from x in db.FAC_ABNORMALNOTIFYFORMUSER
                                          join u in db.ACCOUNT
                                          on x.USERID equals u.ID
                                          join o in db.ORGANIZATION
                                          on u.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                          where x.FORMUNIQUEID == form.UNIQUEID
                                          select new Models.ASE.Shared.ASEUserModel
                                          {
                                              OrganizationDescription = o.DESCRIPTION,
                                              ID = u.ID,
                                              Name = u.NAME,
                                              Email = u.EMAIL
                                          }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.ID).ToList(),
                        NotifyCCUserList = (from x in db.FAC_ABNORMALNOTIFYFORMCCUSER
                                          join u in db.ACCOUNT
                                          on x.USERID equals u.ID
                                          join o in db.ORGANIZATION
                                          on u.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                          where x.FORMUNIQUEID == form.UNIQUEID
                                          select new Models.ASE.Shared.ASEUserModel
                                          {
                                              OrganizationDescription = o.DESCRIPTION,
                                              ID = u.ID,
                                              Name = u.NAME,
                                              Email = u.EMAIL
                                          }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.ID).ToList()
                    };

                    var repairForm = (from x in db.FAC_ABNORMALNOTIFYFORM
                                      join f in db.RFORM
                                      on x.RFORMUNIQUEID equals f.UNIQUEID
                                      join t in db.RFORMTYPE
                                      on f.RFORMTYPEUNIQUEID equals t.UNIQUEID into tmpType
                                      from t in tmpType.DefaultIfEmpty()
                                      join e in db.EQUIPMENT
                                      on f.EQUIPMENTUNIQUEID equals e.UNIQUEID into tmpEquipment
                                      from e in tmpEquipment.DefaultIfEmpty()
                                      join p in db.EQUIPMENTPART
                                      on f.PARTUNIQUEID equals p.UNIQUEID into tmpPart
                                      from p in tmpPart.DefaultIfEmpty()
                                      where x.UNIQUEID == model.UniqueID
                                      select new
                                      {
                                          MaintenanceOrganizationUniqueID = f.PMORGANIZATIONUNIQUEID,
                                          f.ORGANIZATIONUNIQUEID,
                                          UniqueID = f.UNIQUEID,
                                          f.VHNO,
                                          f.STATUS,
                                          RepairFormType = t != null ? t.DESCRIPTION : "",
                                          EstBeginDate = f.ESTBEGINDATE,
                                          EstEndDate = f.ESTENDDATE,
                                          Subject = f.SUBJECT,
                                          Descriptipn = f.DESCRIPTION,
                                          EquipmentUniqueID = f.EQUIPMENTUNIQUEID,
                                          EquipmentID = e != null ? e.ID : "",
                                          EquipmentName = e != null ? e.NAME : "",
                                          PartUniqueID = f.PARTUNIQUEID,
                                          PartDescription = p != null ? p.DESCRIPTION : ""
                                      }).FirstOrDefault();

                    if (repairForm != null)
                    {
                        model.RepairForm = new RepairFormModel()
                        {
                            UniqueID = repairForm.UniqueID,
                            ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(repairForm.ORGANIZATIONUNIQUEID),
                            MaintenanceOrganizationDescription = OrganizationDataAccessor.GetOrganizationFullDescription(repairForm.MaintenanceOrganizationUniqueID),
                            VHNO = repairForm.VHNO,
                            Subject = repairForm.Subject,
                            Description = repairForm.Descriptipn,
                            EstBeginDate = repairForm.EstBeginDate,
                            EstEndDate = repairForm.EstEndDate,
                            EquipmentID = repairForm.EquipmentID,
                            EquipmentName = repairForm.EquipmentName,
                            PartDescription = repairForm.PartDescription,
                            RepairFormType = repairForm.RepairFormType,
                            Status = repairForm.STATUS
                        };
                    }
                    else
                    {
                        model.RepairForm = null;
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

        public static RequestResult GetCreateFormModel(List<Organization> ResponsibleOrganizationList, List<Organization> OrganizationList, List<Models.Shared.UserModel> AccountList)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var model = new CreateFormModel()
                    {
                        ResponsibleOrganizationSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        },
                        GroupList = db.FAC_ABNORMALNOTIFYGROUP.OrderBy(x=>x.DESCRIPTION).ToDictionary(x => x.UNIQUEID, x => x.DESCRIPTION),
                        FormInput = new FormInput()
                        {
                            OccurDateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today),
                            OccurTime = DateTime.Now.ToString("HHmm")
                        }
                    };

                    foreach (var responsibleOrganization in ResponsibleOrganizationList)
                    {
                        var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(OrganizationList, responsibleOrganization.UniqueID, true);

                        if (AccountList.Any(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID)))
                        {
                            model.ResponsibleOrganizationSelectItemList.Add(new SelectListItem()
                            {
                                Value = responsibleOrganization.UniqueID,
                                Text = string.Format("{0}/{1}", responsibleOrganization.ID, responsibleOrganization.Description)
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

        public static RequestResult Create(CreateFormModel Model, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var occurTime = DateTimeHelper.DateTimeString2DateTime(Model.FormInput.OccurDate, Model.FormInput.OccurTime);

                    if (DateTime.Compare(occurTime.Value, DateTime.Now) <= 0)
                    {
                        var vhnoPrefix = string.Format("A{0}", DateTime.Today.ToString("yyyyMM").Substring(2));

                        var vhnoSeq = 1;

                        var query = db.FAC_ABNORMALNOTIFYFORM.Where(x => x.VHNO.StartsWith(vhnoPrefix)).OrderByDescending(x => x.VHNO).ToList();

                        if (query.Count > 0)
                        {
                            vhnoSeq = int.Parse(query.First().VHNO.Substring(5)) + 1;
                        }

                        var vhno = string.Format("{0}{1}", vhnoPrefix, vhnoSeq.ToString().PadLeft(3, '0'));

                        var uniqueID = Guid.NewGuid().ToString();

                        db.FAC_ABNORMALNOTIFYFORM.Add(new FAC_ABNORMALNOTIFYFORM()
                        {
                            UNIQUEID = uniqueID,
                            VHNO = vhno,
                            COST = Model.FormInput.Cost,
                            CREATETIME = DateTime.Now,
                            CREATEUSERID = Account.ID,
                            DESCRIPTION = Model.FormInput.Description,
                            EFFECTAREA = Model.FormInput.EffectArea,
                            EFFECTSYSTEM = Model.FormInput.EffectSystem,
                            HANDLINGDESCRIPTION = Model.FormInput.HandlingDescription,
                            MVPN = Model.FormInput.Mvpn,
                            LOCATION = Model.FormInput.Location,
                            OCCURDATE = Model.FormInput.OccurDate,
                            OCCURTIME = Model.FormInput.OccurTime,
                            RECOVERYDATE = Model.FormInput.RecoveryDate,
                            RECOVERYDESCRIPTION = Model.FormInput.RecoveryDescription,
                            RECOVERYTIME = Model.FormInput.RecoveryTime,
                            RESORGANIZATIONUNIQUEID = Model.FormInput.ResponsibleOrganizationUniqueID,
                            STATUS = !string.IsNullOrEmpty(Model.FormInput.ResponsibleOrganizationUniqueID) ? "1" : "0",
                            SUBJECT = Model.FormInput.Subject,
                            CONTACT = Model.FormInput.Contact
                        });

                        db.FAC_ABNORMALNOTIFYFORMGROUP.AddRange(Model.FormInput.GroupList.Select(x => new FAC_ABNORMALNOTIFYFORMGROUP
                        {
                            FORMUNIQUEID = uniqueID,
                            GROUPUNIQUEID = x
                        }).ToList());

                        db.FAC_ABNORMALNOTIFYFORMLOG.Add(new FAC_ABNORMALNOTIFYFORMLOG()
                        {
                            FORMUNIQUEID = uniqueID,
                            SEQ = 1,
                            ACTION = Define.EnumFormAction.Create.ToString(),
                            LOGTIME = DateTime.Now,
                            USERID = Account.ID
                        });

                        db.FAC_ABNORMALNOTIFYFORMUSER.AddRange(Model.NotifyUserList.Select(x => new FAC_ABNORMALNOTIFYFORMUSER
                        {
                            FORMUNIQUEID = uniqueID,
                            USERID = x.ID
                        }).ToList());

                        db.FAC_ABNORMALNOTIFYFORMCCUSER.AddRange(Model.NotifyCCUserList.Select(x => new FAC_ABNORMALNOTIFYFORMCCUSER
                        {
                            FORMUNIQUEID = uniqueID,
                            USERID = x.ID
                        }).ToList());

                        foreach (var file in Model.FileList)
                        {
                            db.FAC_ABNORMALNOTIFYFORMFILE.Add(new FAC_ABNORMALNOTIFYFORMFILE()
                            {
                                FORMUNIQUEID = uniqueID,
                                SEQ = file.Seq,
                                EXTENSION = file.Extension,
                                FILENAME = file.FileName,
                                LASTMODIFYTIME = file.LastModifyTime,
                                S = file.Size
                            });

                            System.IO.File.Move(Path.Combine(Config.TempFolder, file.TempFileName), Path.Combine(Config.EquipmentMaintenanceFileFolderPath, string.Format("{0}_{1}.{2}", uniqueID, file.Seq, file.Extension)));
                        }

                        db.SaveChanges();

                        if (Config.HaveMailSetting)
                        {
                            SendNotifyMail(uniqueID, false);
                        }

                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.AbnormalNotifyForm, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage("發生時間不可大於現在");
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

        public static RequestResult GetEditFormModel(string UniqueID, List<Models.Shared.Organization> ResponsibleOrganizationList, List<Models.Shared.UserModel> UserList)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new EditFormModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = db.FAC_ABNORMALNOTIFYFORM.First(x => x.UNIQUEID == UniqueID);

                    var createUser = UserList.FirstOrDefault(x => x.ID == form.CREATEUSERID);
                    var takeJobUser = UserList.FirstOrDefault(x => x.ID == form.TAKEJOBUSERID);
                    var resOrganization = ResponsibleOrganizationList.FirstOrDefault(x => x.UniqueID == form.RESORGANIZATIONUNIQUEID);

                    model = new EditFormModel()
                    {
                        GroupList = db.FAC_ABNORMALNOTIFYGROUP.OrderBy(x => x.DESCRIPTION).ToDictionary(x => x.UNIQUEID, x => x.DESCRIPTION),
                        FormGroupList = db.FAC_ABNORMALNOTIFYFORMGROUP.Where(x=>x.FORMUNIQUEID==form.UNIQUEID).Select(x=>x.GROUPUNIQUEID).ToList(),
                        UniqueID = form.UNIQUEID,
                        VHNO = form.VHNO,
                        FormInput = new FormInput(){
                            Cost = form.COST,
                            Location = form.LOCATION,
                            Description = form.DESCRIPTION,
                            EffectArea = form.EFFECTAREA,
                            EffectSystem = form.EFFECTSYSTEM,
                            HandlingDescription = form.HANDLINGDESCRIPTION,
                            Mvpn = form.MVPN,
                            OccurDateString=DateTimeHelper.DateString2DateStringWithSeparator(form.OCCURDATE),
                            OccurTime = form.OCCURTIME,
                            RecoveryDescription = form.RECOVERYDESCRIPTION,
                            RecoveryDateString=DateTimeHelper.DateString2DateStringWithSeparator(form.RECOVERYDATE),
                            RecoveryTime = form.RECOVERYTIME,
                             ResponsibleOrganizationUniqueID=form.RESORGANIZATIONUNIQUEID,
                            Contact = form.CONTACT,
                            Subject = form.SUBJECT,
                        },
                        ResponsibleOrganization = resOrganization != null ? resOrganization.Description : string.Empty,
                        CreateTime = form.CREATETIME,
                        CreateUserID = form.CREATEUSERID,
                        CreateUserName = createUser != null ? createUser.Name : string.Empty,
                        ClosedRemark = form.CLOSEDREMARK,
                        TakeJobTime = form.TAKEJOBTIME,
                        TakeJobUserID = form.TAKEJOBUSERID,
                        TakeJobUserName = takeJobUser != null ? takeJobUser.Name : string.Empty,
                        FileList = db.FAC_ABNORMALNOTIFYFORMFILE.Where(f => f.FORMUNIQUEID == UniqueID).ToList().Select(f => new FileModel
                        {
                            IsSaved = true,
                            Seq = f.SEQ,
                            FileName = f.FILENAME,
                            Extension = f.EXTENSION,
                            Size = f.S.Value,
                            LastModifyTime = f.LASTMODIFYTIME.Value
                        }).OrderBy(f => f.LastModifyTime).ToList(),
                        Status = form.STATUS,
                        LogList = (from x in db.FAC_ABNORMALNOTIFYFORMLOG
                                   join u in db.ACCOUNT
                                   on x.USERID equals u.ID
                                   where x.FORMUNIQUEID == form.UNIQUEID
                                   select new
                                   {
                                       x.ACTION,
                                       x.LOGTIME,
                                       x.USERID,
                                       USERNAME = u.NAME,
                                       x.SEQ
                                   }).ToList().Select(x => new LogModel
                                   {
                                       Action = Define.EnumParse<Define.EnumFormAction>(x.ACTION),
                                       LogTime = x.LOGTIME,
                                       UserID = x.USERID,
                                       UserName = x.USERNAME,
                                       Seq = x.SEQ
                                   }).OrderBy(x => x.Seq).ToList(),
                        NotifyUserList = (from x in db.FAC_ABNORMALNOTIFYFORMUSER
                                          join u in db.ACCOUNT
                                          on x.USERID equals u.ID
                                          join o in db.ORGANIZATION
                                          on u.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                          where x.FORMUNIQUEID == form.UNIQUEID
                                          select new Models.ASE.Shared.ASEUserModel
                                          {
                                              OrganizationDescription = o.DESCRIPTION,
                                              ID = u.ID,
                                              Name = u.NAME,
                                              Email = u.EMAIL
                                          }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.ID).ToList(),
                        NotifyCCUserList = (from x in db.FAC_ABNORMALNOTIFYFORMCCUSER
                                          join u in db.ACCOUNT
                                          on x.USERID equals u.ID
                                          join o in db.ORGANIZATION
                                          on u.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                          where x.FORMUNIQUEID == form.UNIQUEID
                                          select new Models.ASE.Shared.ASEUserModel
                                          {
                                              OrganizationDescription = o.DESCRIPTION,
                                              ID = u.ID,
                                              Name = u.NAME,
                                              Email = u.EMAIL
                                          }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.ID).ToList()
                    };

                    var repairForm = (from x in db.FAC_ABNORMALNOTIFYFORM
                                      join f in db.RFORM
                                      on x.RFORMUNIQUEID equals f.UNIQUEID
                                      join t in db.RFORMTYPE
                                      on f.RFORMTYPEUNIQUEID equals t.UNIQUEID into tmpType
                                      from t in tmpType.DefaultIfEmpty()
                                      join e in db.EQUIPMENT
                                      on f.EQUIPMENTUNIQUEID equals e.UNIQUEID into tmpEquipment
                                      from e in tmpEquipment.DefaultIfEmpty()
                                      join p in db.EQUIPMENTPART
                                      on f.PARTUNIQUEID equals p.UNIQUEID into tmpPart
                                      from p in tmpPart.DefaultIfEmpty()
                                      where x.UNIQUEID == model.UniqueID
                                      select new
                                      {
                                          MaintenanceOrganizationUniqueID = f.PMORGANIZATIONUNIQUEID,
                                          f.ORGANIZATIONUNIQUEID,
                                          UniqueID = f.UNIQUEID,
                                          f.VHNO,
                                          f.STATUS,
                                          RepairFormType = t != null ? t.DESCRIPTION : "",
                                          EstBeginDate = f.ESTBEGINDATE,
                                          EstEndDate = f.ESTENDDATE,
                                          Subject = f.SUBJECT,
                                          Descriptipn = f.DESCRIPTION,
                                          EquipmentUniqueID = f.EQUIPMENTUNIQUEID,
                                          EquipmentID = e != null ? e.ID : "",
                                          EquipmentName = e != null ? e.NAME : "",
                                          PartUniqueID = f.PARTUNIQUEID,
                                          PartDescription = p != null ? p.DESCRIPTION : ""
                                      }).FirstOrDefault();

                    if (repairForm != null)
                    {
                        model.RepairForm = new RepairFormModel()
                        {
                            UniqueID = repairForm.UniqueID,
                            ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(repairForm.ORGANIZATIONUNIQUEID),
                            MaintenanceOrganizationDescription = OrganizationDataAccessor.GetOrganizationFullDescription(repairForm.MaintenanceOrganizationUniqueID),
                            VHNO = repairForm.VHNO,
                            Subject = repairForm.Subject,
                            Description = repairForm.Descriptipn,
                            EstBeginDate = repairForm.EstBeginDate,
                            EstEndDate = repairForm.EstEndDate,
                            EquipmentID = repairForm.EquipmentID,
                            EquipmentName = repairForm.EquipmentName,
                            PartDescription = repairForm.PartDescription,
                            RepairFormType = repairForm.RepairFormType,
                            Status = repairForm.STATUS
                        };
                    }
                    else
                    {
                        model.RepairForm = null;
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

        public static RequestResult Edit(EditFormModel Model, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var occurTime = DateTimeHelper.DateTimeString2DateTime(Model.FormInput.OccurDate, Model.FormInput.OccurTime);

                    if (DateTime.Compare(occurTime.Value, DateTime.Now) <= 0)
                    {
#if !DEBUG
                        using (TransactionScope trans = new TransactionScope())
                        {
#endif
                            var form = db.FAC_ABNORMALNOTIFYFORM.First(x => x.UNIQUEID == Model.UniqueID);

                            form.COST = Model.FormInput.Cost;
                            form.DESCRIPTION = Model.FormInput.Description;
                            form.EFFECTAREA = Model.FormInput.EffectArea;
                            form.EFFECTSYSTEM = Model.FormInput.EffectSystem;
                            form.HANDLINGDESCRIPTION = Model.FormInput.HandlingDescription;
                            form.MVPN = Model.FormInput.Mvpn;
                            form.OCCURDATE = Model.FormInput.OccurDate;
                            form.OCCURTIME = Model.FormInput.OccurTime;
                            form.RECOVERYDATE = Model.FormInput.RecoveryDate;
                            form.RECOVERYDESCRIPTION = Model.FormInput.RecoveryDescription;
                            form.RECOVERYTIME = Model.FormInput.RecoveryTime;
                            form.CONTACT = Model.FormInput.Contact;
                            form.LOCATION = Model.FormInput.Location;

                            db.SaveChanges();

                            var logSeq = db.FAC_ABNORMALNOTIFYFORMLOG.Count(x => x.FORMUNIQUEID == form.UNIQUEID) + 1;

                            db.FAC_ABNORMALNOTIFYFORMLOG.Add(new FAC_ABNORMALNOTIFYFORMLOG()
                            {
                                FORMUNIQUEID = form.UNIQUEID,
                                SEQ = logSeq,
                                ACTION = Define.EnumFormAction.Edit.ToString(),
                                LOGTIME = DateTime.Now,
                                USERID = Account.ID
                            });

                            db.SaveChanges();

                            db.FAC_ABNORMALNOTIFYFORMGROUP.RemoveRange(db.FAC_ABNORMALNOTIFYFORMGROUP.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList());

                            db.SaveChanges();

                            db.FAC_ABNORMALNOTIFYFORMGROUP.AddRange(Model.FormInput.GroupList.Select(x => new FAC_ABNORMALNOTIFYFORMGROUP
                            {
                                FORMUNIQUEID = form.UNIQUEID,
                                GROUPUNIQUEID = x
                            }).ToList());

                            db.SaveChanges();

                            db.FAC_ABNORMALNOTIFYFORMUSER.RemoveRange(db.FAC_ABNORMALNOTIFYFORMUSER.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList());

                            db.SaveChanges();

                            db.FAC_ABNORMALNOTIFYFORMUSER.AddRange(Model.NotifyUserList.Select(x => new FAC_ABNORMALNOTIFYFORMUSER
                            {
                                FORMUNIQUEID = form.UNIQUEID,
                                USERID = x.ID
                            }).ToList());

                            db.SaveChanges();

                            db.FAC_ABNORMALNOTIFYFORMCCUSER.RemoveRange(db.FAC_ABNORMALNOTIFYFORMCCUSER.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList());

                            db.SaveChanges();

                            db.FAC_ABNORMALNOTIFYFORMCCUSER.AddRange(Model.NotifyCCUserList.Select(x => new FAC_ABNORMALNOTIFYFORMCCUSER
                            {
                                FORMUNIQUEID = form.UNIQUEID,
                                USERID = x.ID
                            }).ToList());

                            db.SaveChanges();

                            var fileList = db.FAC_ABNORMALNOTIFYFORMFILE.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList();

                            foreach (var file in fileList)
                            {
                                if (!Model.FileList.Any(x => x.Seq == file.SEQ))
                                {
                                    try
                                    {
                                        System.IO.File.Delete(Path.Combine(Config.EquipmentMaintenanceFileFolderPath, string.Format("{0}_{1}.{2}", form.UNIQUEID, file.SEQ, file.EXTENSION)));
                                    }
                                    catch { }
                                }
                            }

                            db.FAC_ABNORMALNOTIFYFORMFILE.RemoveRange(fileList);

                            db.SaveChanges();

                            foreach (var file in Model.FileList)
                            {
                                db.FAC_ABNORMALNOTIFYFORMFILE.Add(new FAC_ABNORMALNOTIFYFORMFILE()
                                {
                                    FORMUNIQUEID = form.UNIQUEID,
                                    SEQ = file.Seq,
                                    EXTENSION = file.Extension,
                                    FILENAME = file.FileName,
                                    LASTMODIFYTIME = file.LastModifyTime,
                                    S = file.Size
                                });

                                if (!file.IsSaved)
                                {
                                    System.IO.File.Copy(Path.Combine(Config.TempFolder, file.TempFileName), Path.Combine(Config.EquipmentMaintenanceFileFolderPath, string.Format("{0}_{1}.{2}", form.UNIQUEID, file.Seq, file.Extension)), true);
                                    System.IO.File.Delete(Path.Combine(Config.TempFolder, file.TempFileName));
                                }
                            }

                            db.SaveChanges();

                            if (Config.HaveMailSetting)
                            {
                                SendNotifyMail(form.UNIQUEID, true);
                            }

                            result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.AbnormalNotifyForm, Resources.Resource.Success));

#if !DEBUG
                            trans.Complete();
                        }
#endif
                    }
                    else
                    {
                        result.ReturnFailedMessage("發生時間不可大於現在");
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

        public static void SendNotifyMail(string UniqueID, bool IsResend)
        {
            try
            {
                var mailAddressList = new List<MailAddress>();

                var ccList = new List<MailAddress>()
                {
                    
                };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = db.FAC_ABNORMALNOTIFYFORM.First(x => x.UNIQUEID == UniqueID);

                    var notifyUserList = (from x in db.FAC_ABNORMALNOTIFYFORMUSER
                                          join u in db.ACCOUNT
                                          on x.USERID equals u.ID
                                          where x.FORMUNIQUEID == form.UNIQUEID && !string.IsNullOrEmpty(u.EMAIL)
                                          select u).ToList();

                    var notifyCCUserList = (from x in db.FAC_ABNORMALNOTIFYFORMCCUSER
                                          join u in db.ACCOUNT
                                          on x.USERID equals u.ID
                                          where x.FORMUNIQUEID == form.UNIQUEID && !string.IsNullOrEmpty(u.EMAIL)
                                          select u).ToList();

                    foreach (var notifyUser in notifyUserList)
                    {
                        mailAddressList.Add(new MailAddress(notifyUser.EMAIL, notifyUser.NAME));
                    }

                    foreach (var notifyCCUser in notifyCCUserList)
                    {
                        ccList.Add(new MailAddress(notifyCCUser.EMAIL, notifyCCUser.NAME));
                    }

                    var groupList = db.FAC_ABNORMALNOTIFYFORMGROUP.Where(x => x.FORMUNIQUEID == form.UNIQUEID).Select(x => x.GROUPUNIQUEID).ToList();

                    foreach (var groupUniqueID in groupList)
                    {
                        var groupUserList = (from x in db.FAC_ABNORMALNOTIFYGROUPUSER
                                             join u in db.ACCOUNT
                                             on x.USERID equals u.ID
                                             where x.GROUPUNIQUEID == groupUniqueID && !string.IsNullOrEmpty(u.EMAIL)
                                             select u).ToList();

                        foreach (var groupUser in groupUserList)
                        {
                            mailAddressList.Add(new MailAddress(groupUser.EMAIL, groupUser.NAME));
                        }

                        var groupCCUserList = (from x in db.FAC_ABNORMALNOTIFYGROUPCCUSER
                                             join u in db.ACCOUNT
                                             on x.USERID equals u.ID
                                             where x.GROUPUNIQUEID == groupUniqueID && !string.IsNullOrEmpty(u.EMAIL)
                                             select u).ToList();

                        foreach (var groupCCUser in groupCCUserList)
                        {
                            ccList.Add(new MailAddress(groupCCUser.EMAIL, groupCCUser.NAME));
                        }
                    }

                    if (mailAddressList.Count > 0)
                    {
                        var subject = string.Empty;

                        if (!IsResend)
                        {
                            subject = string.Format("[廠務處異常通報][{0}]{1}",form.VHNO, form.SUBJECT);
                        }
                        else
                        {
                            subject = string.Format("【更新】[廠務處異常通報][{0}]{1}", form.VHNO, form.SUBJECT);
                        }

                        var th = "<th style=\"width:150px;border:1px solid #333;padding:8px;text-align:right;color:#707070;background: #F4F4F4;\">{0}</th>";
                        var td = "<td style=\"width:400px;border:1px solid #333;padding:8px;color:#707070;\">{0}</td>";

                        var sb = new StringBuilder();

                        sb.Append("<table style=\"1px solid #ddd;font-size:13px;border-collapse:collapse;\">");

                        var occurTime = DateTimeHelper.DateTimeString2DateTime(form.OCCURDATE, form.OCCURTIME);
                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "發生時間"));
                        sb.Append(string.Format(td, DateTimeHelper.DateTime2DateTimeStringWithSeperator(occurTime)));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "聯絡人員"));
                        sb.Append(string.Format(td, form.CONTACT));
                        sb.Append("</tr>");

                        if(!string.IsNullOrEmpty(form.CONTACT))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "MVPN"));
                            sb.Append(string.Format(td, form.MVPN));
                            sb.Append("</tr>");
                        }

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "異常主旨"));
                        sb.Append(string.Format(td, form.SUBJECT));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "地點"));
                        sb.Append(string.Format(td, form.LOCATION));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "異常原因"));
                        sb.Append(string.Format(td, form.DESCRIPTION));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "緊急對策"));
                        sb.Append(string.Format(td, form.HANDLINGDESCRIPTION));
                        sb.Append("</tr>");

                        if (!string.IsNullOrEmpty(form.RECOVERYDATE))
                        {
                            var recoveryTime = DateTimeHelper.DateTimeString2DateTime(form.RECOVERYDATE, form.RECOVERYTIME);

                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "復原時間"));
                            sb.Append(string.Format(td, DateTimeHelper.DateTime2DateTimeStringWithSeperator(recoveryTime)));
                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(form.RECOVERYDESCRIPTION))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "復原說明"));
                            sb.Append(string.Format(td, form.RECOVERYDESCRIPTION));
                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(form.EFFECTAREA))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "影響區域(產線單位)"));
                            sb.Append(string.Format(td, form.EFFECTAREA));
                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(form.EFFECTSYSTEM))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "影響系統(FAC系統)"));
                            sb.Append(string.Format(td, form.EFFECTSYSTEM));
                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(form.COST))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "損失金額"));
                            sb.Append(string.Format(td, form.COST));
                            sb.Append("</tr>");
                        }

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "詳細資料"));
                        sb.Append(string.Format(td, string.Format("<a href=\"http://ASECL-eINSPRD01/FEM.Portal/Home/Index?ReturnUrl=http://ASECL-eINSPRD01/FEM/zh-tw/Customized_ASE/AbnormalNotify/Portal?UniqueID=" + form.UNIQUEID + "\">連結</a>")));
                        //sb.Append(string.Format(td, string.Format("<a href=\"http://ASECL-eINSQAS/FEM.Portal/Home/Index?ReturnUrl=http://ASECL-eINSQAS/FEM/zh-tw/Customized_ASE/AbnormalNotify/Portal?UniqueID=" + form.UNIQUEID + "\">連結</a>")));
                        sb.Append("</tr>");

                        sb.Append("</table>");

                        var attachList = new List<Attachment>();

                        var fileList = db.FAC_ABNORMALNOTIFYFORMFILE.Where(x => x.FORMUNIQUEID == form.UNIQUEID);

                        foreach (var file in fileList)
                        {
                            var fileName = string.Format("{0}_{1}.{2}", file.FORMUNIQUEID, file.SEQ, file.EXTENSION);

                            var fullFileName = Path.Combine(Config.EquipmentMaintenanceFileFolderPath, fileName);

                            Attachment attach = new Attachment(fullFileName, MediaTypeNames.Application.Octet);
                            
                            ContentDisposition disposition = attach.ContentDisposition;
                            disposition.CreationDate = System.IO.File.GetCreationTime(fullFileName);
                            disposition.ModificationDate = System.IO.File.GetLastWriteTime(fullFileName);
                            disposition.ReadDate = System.IO.File.GetLastAccessTime(fullFileName);
                            disposition.FileName = string.Format("{0}.{1}", file.FILENAME, file.EXTENSION);
                            disposition.Size = new FileInfo(fullFileName).Length;
                            disposition.DispositionType = DispositionTypeNames.Attachment;

                            attachList.Add(attach);
                        }

#if !DEBUG
                        MailHelper.SendMail(mailAddressList, null, ccList, subject, sb.ToString(), attachList);
#endif
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public static FileDownloadModel GetFile(string FormUniqueID, int Seq)
        {
            var model = new FileDownloadModel();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var file = db.FAC_ABNORMALNOTIFYFORMFILE.First(x => x.FORMUNIQUEID == FormUniqueID && x.SEQ == Seq);

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

        public static RequestResult GetCreateRepairFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new RepairFormCreateFormModel()
                {
                    FormUniqueID = UniqueID,
                    RepairFormTypeSelectItemList = new List<SelectListItem>() 
                    { 
                        Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                    },
                    SubjectSelectItemList = new List<SelectListItem>() 
                    { 
                        Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                    }
                };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = db.FAC_ABNORMALNOTIFYFORM.First(x => x.UNIQUEID == UniqueID);

                    var ancestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(form.RESORGANIZATIONUNIQUEID);

                    var repairFormTypeList = db.RFORMTYPE.Where(x => x.ANCESTORORGANIZATIONUNIQUEID == ancestorOrganizationUniqueID).OrderBy(x => x.DESCRIPTION).ToList();

                    foreach (var repairFormType in repairFormTypeList)
                    {
                        model.RepairFormTypeSelectItemList.Add(new SelectListItem()
                        {
                            Value = repairFormType.UNIQUEID,
                            Text = repairFormType.DESCRIPTION
                        });

                        model.RepairFormTypeSubjectList.Add(repairFormType.UNIQUEID, (from x in db.RFORMTYPESUBJECT
                                                                                      join s in db.RFORMSUBJECT
                                                                                      on x.SUBJECTUNIQUEID equals s.UNIQUEID
                                                                                      where x.RFORMTYPEUNIQUEID == repairFormType.UNIQUEID
                                                                                      orderby x.SEQ
                                                                                      select new Models.ASE.AbnormalNotify.RFormSubject
                                                                                      {
                                                                                          UniqueID = s.UNIQUEID,
                                                                                          ID = s.ID,
                                                                                          Description = s.DESCRIPTION,
                                                                                          AncestorOrganizationUniqueID = s.ANCESTORORGANIZATIONUNIQUEID
                                                                                      }).ToList());
                    }

                    var organization = OrganizationDataAccessor.GetOrganization(form.RESORGANIZATIONUNIQUEID);

                    model.OrganizationUniqueID = form.RESORGANIZATIONUNIQUEID;
                    model.AncestorOrganizationUniqueID = organization.AncestorOrganizationUniqueID;
                    model.FullOrganizationDescription = organization.FullDescription;

                    model.EquipmentSelectItemList = new List<SelectListItem>() 
                        { 
                            Define.DefaultSelectListItem(Resources.Resource.None)
                        };

                    var equipmentList = db.EQUIPMENT.Where(x => x.ORGANIZATIONUNIQUEID == form.RESORGANIZATIONUNIQUEID).OrderBy(x => x.ID).ToList();

                    foreach (var equipment in equipmentList)
                    {
                        model.EquipmentSelectItemList.Add(new SelectListItem()
                        {
                            Value = string.Format("{0}{1}{2}", equipment.UNIQUEID, Define.Seperator, "*"),
                            Text = string.Format("{0}/{1}", equipment.ID, equipment.NAME)
                        });

                        var partList = db.EQUIPMENTPART.Where(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID).OrderBy(x => x.DESCRIPTION).ToList();

                        foreach (var part in partList)
                        {
                            model.EquipmentSelectItemList.Add(new SelectListItem()
                            {
                                Value = string.Format("{0}{1}{2}", equipment.UNIQUEID, Define.Seperator, part.UNIQUEID),
                                Text = string.Format("{0}/{1}-{2}", equipment.ID, equipment.NAME, part.DESCRIPTION)
                            });
                        }

                        if (!string.IsNullOrEmpty(equipment.PMORGANIZATIONUNIQUEID))
                        {
                            model.EquipmentMaintenanceOrganizationList.Add(equipment.UNIQUEID, OrganizationDataAccessor.GetOrganization(equipment.PMORGANIZATIONUNIQUEID));
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

        public static RequestResult CreateRepairForm(RepairFormCreateFormModel Model, string UserID)
        {
            RequestResult result = new RequestResult();

            try
            {
                string jobManagerID = string.Empty;
                string maintenanceOrganizationUniqueID = string.Empty;

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (Model.FormInput.IsRepairBySelf)
                    {
                        maintenanceOrganizationUniqueID = db.ACCOUNT.First(x => x.ID == UserID).ORGANIZATIONUNIQUEID;
                        jobManagerID = UserID;
                    }
                    else
                    {
                        maintenanceOrganizationUniqueID = Model.FormInput.MaintenanceOrganizationUniqueID;

                        var organization = db.ORGANIZATION.First(x => x.UNIQUEID == Model.FormInput.MaintenanceOrganizationUniqueID);

                        if (!string.IsNullOrEmpty(organization.MANAGERUSERID))
                        {
                            jobManagerID = organization.MANAGERUSERID;
                        }
                        else
                        {
                            jobManagerID = string.Empty;
                            result.ReturnFailedMessage(string.Format("{0} {1} {2} {3}", Resources.Resource.Organization, organization.DESCRIPTION, Resources.Resource.NotSet, Resources.Resource.Manager));
                        }
                    }
                }

                if (!string.IsNullOrEmpty(jobManagerID))
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var equipmentUniqueID = string.Empty;
                        var partUniqueID = string.Empty;

                        if (!string.IsNullOrEmpty(Model.FormInput.EquipmentUniqueID))
                        {
                            string[] t = Model.FormInput.EquipmentUniqueID.Split(Define.Seperators, StringSplitOptions.None);

                            equipmentUniqueID = t[0];
                            partUniqueID = t[1];
                        }

                        var equipment = db.EQUIPMENT.FirstOrDefault(x => x.UNIQUEID == equipmentUniqueID);
                        var part = db.EQUIPMENTPART.FirstOrDefault(x => x.UNIQUEID == partUniqueID);

                        var vhno = DateTime.Today.ToString("yyyyMM");

                        var seq = 1;

                        var temp = db.RFORM.Where(x => x.VHNO.StartsWith(vhno)).OrderByDescending(x => x.VHNO).FirstOrDefault();

                        if (temp != null)
                        {
                            seq = int.Parse(temp.VHNO.Substring(6)) + 1;
                        }

                        vhno = string.Format("{0}{1}", DateTime.Today.ToString("yyyyMM"), seq.ToString().PadLeft(4, '0'));

                        var organizationUniqueID = Model.OrganizationUniqueID;

                        var uniqueID = Guid.NewGuid().ToString();

                        var createTime = DateTime.Now;

                        var form = new RFORM()
                        {
                            UNIQUEID = uniqueID,
                            VHNO = vhno,
                            ORGANIZATIONUNIQUEID = organizationUniqueID,
                            PMORGANIZATIONUNIQUEID = maintenanceOrganizationUniqueID,
                            EQUIPMENTUNIQUEID = equipmentUniqueID,
                            EQUIPMENTID = equipment!=null?equipment.ID:"",
                            EQUIPMENTNAME = equipment!=null?equipment.NAME:"",
                            PARTUNIQUEID = partUniqueID,
                            PARTDESCRIPTION = part!=null?part.DESCRIPTION:"",
                            RFORMTYPEUNIQUEID = Model.FormInput.RepairFormTypeUniqueID,
                            SUBJECT = Model.FormInput.Subject,
                            DESCRIPTION = Model.FormInput.Description,
                            CREATEUSERID = UserID,
                            CREATETIME = createTime,
                        };

                        if (Model.FormInput.IsRepairBySelf)
                        {
                            form.STATUS = "4";

                            form.ESTBEGINDATE = Model.FormInput.EstBeginDate;
                            form.ESTENDDATE = Model.FormInput.EstEndDate;
                            form.MANAGERJOBTIME = createTime;
                            form.JOBREFUSEREASON = string.Empty;
                            form.TAKEJOBTIME = createTime;
                            form.TAKEJOBUSERID = UserID;
                            
                            db.RFORMJOBUSER.Add(new RFORMJOBUSER()
                            {
                                RFORMUNIQUEID = uniqueID,
                                USERID = UserID
                            });
                        }
                        else
                        {
                            form.STATUS = "0";
                        }

                        db.RFORM.Add(form);

                        var abnormalForm = db.FAC_ABNORMALNOTIFYFORM.First(x => x.UNIQUEID == Model.FormUniqueID);

                        abnormalForm.RFORMUNIQUEID = uniqueID;

                        db.SaveChanges();

                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.RepairForm, Resources.Resource.Success));
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

        public static RequestResult TakeJob(string UniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = db.FAC_ABNORMALNOTIFYFORM.First(x => x.UNIQUEID == UniqueID);

                    form.TAKEJOBTIME = DateTime.Now;
                    form.TAKEJOBUSERID = Account.ID;
                    form.STATUS = "2";
                    
                    db.SaveChanges();

                    result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.TakeJob, Resources.Resource.Success));
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

        public static RequestResult Closed(ClosedFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = db.FAC_ABNORMALNOTIFYFORM.First(x => x.UNIQUEID == Model.UniqueID);

                    form.CLOSEDREMARK = Model.FormInput.Remark;
                    form.STATUS = "3";
                    form.CLOSEDTIME = DateTime.Now;

                    db.SaveChanges();

                    result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Closed, Resources.Resource.Success));
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

        public static RequestResult GetClosedFormModel(string UniqueID, List<Models.Shared.Organization> OrganizationList, List<Models.Shared.UserModel> UserList)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new ClosedFormModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = db.FAC_ABNORMALNOTIFYFORM.First(x => x.UNIQUEID == UniqueID);

                    var createUser = UserList.FirstOrDefault(x => x.ID == form.CREATEUSERID);
                    var takeJobUser = UserList.FirstOrDefault(x => x.ID == form.TAKEJOBUSERID);
                    var resOrganization = OrganizationList.FirstOrDefault(x => x.UniqueID == form.RESORGANIZATIONUNIQUEID);

                    model = new ClosedFormModel()
                    {
                        UniqueID = form.UNIQUEID,
                        VHNO = form.VHNO,
                        ResponsibleOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(OrganizationList, form.RESORGANIZATIONUNIQUEID, true),
                        Cost = form.COST,
                        CreateTime = form.CREATETIME,
                        CreateUserID = form.CREATEUSERID,
                        CreateUserName = createUser != null ? createUser.Name : string.Empty,
                        FormInput=new ClosedFormInput(){
                         Remark = form.CLOSEDREMARK
                        },
                        TakeJobTime = form.TAKEJOBTIME,
                        Location =form.LOCATION,
                        TakeJobUserID = form.TAKEJOBUSERID,
                        TakeJobUserName = takeJobUser != null ? takeJobUser.Name : string.Empty,
                        Description = form.DESCRIPTION,
                        EffectArea = form.EFFECTAREA,
                        EffectSystem = form.EFFECTSYSTEM,
                        FileList = db.FAC_ABNORMALNOTIFYFORMFILE.Where(f => f.FORMUNIQUEID == UniqueID).ToList().Select(f => new FileModel
                        {
                            IsSaved = true,
                            Seq = f.SEQ,
                            FileName = f.FILENAME,
                            Extension = f.EXTENSION,
                            Size = f.S.Value,
                            LastModifyTime = f.LASTMODIFYTIME.Value
                        }).OrderBy(f => f.LastModifyTime).ToList(),
                        HandlingDescription = form.HANDLINGDESCRIPTION,
                        Mvpn = form.MVPN,
                        OccurTime = DateTimeHelper.DateTimeString2DateTime(form.OCCURDATE, form.OCCURTIME),
                        RecoveryDescription = form.RECOVERYDESCRIPTION,
                        RecoveryTime = DateTimeHelper.DateTimeString2DateTime(form.RECOVERYDATE, form.RECOVERYTIME),
                        ResponsibleOrganization = resOrganization != null ? resOrganization.Description : string.Empty,
                        Contact = form.CONTACT,
                        GroupList = (from x in db.FAC_ABNORMALNOTIFYFORMGROUP
                                    join g in db.FAC_ABNORMALNOTIFYGROUP
                                    on x.GROUPUNIQUEID equals g.UNIQUEID
                                    where x.FORMUNIQUEID==form.UNIQUEID
                                    select g.DESCRIPTION).OrderBy(x=>x).ToList(),
                        Status = form.STATUS,
                        Subject = form.SUBJECT,
                        LogList = (from x in db.FAC_ABNORMALNOTIFYFORMLOG
                                   join u in db.ACCOUNT
                                   on x.USERID equals u.ID
                                   where x.FORMUNIQUEID == form.UNIQUEID
                                   select new
                                   {
                                       x.ACTION,
                                       x.LOGTIME,
                                       x.USERID,
                                       USERNAME = u.NAME,
                                       x.SEQ
                                   }).ToList().Select(x => new LogModel
                                   {
                                       Action = Define.EnumParse<Define.EnumFormAction>(x.ACTION),
                                       LogTime = x.LOGTIME,
                                       UserID = x.USERID,
                                       UserName = x.USERNAME,
                                       Seq = x.SEQ
                                   }).OrderBy(x => x.Seq).ToList(),
                        NotifyUserList = (from x in db.FAC_ABNORMALNOTIFYFORMUSER
                                          join u in db.ACCOUNT
                                          on x.USERID equals u.ID
                                          join o in db.ORGANIZATION
                                          on u.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                          where x.FORMUNIQUEID == form.UNIQUEID
                                          select new Models.ASE.Shared.ASEUserModel
                                          {
                                              OrganizationDescription = o.DESCRIPTION,
                                              ID = u.ID,
                                              Name = u.NAME,
                                              Email = u.EMAIL
                                          }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.ID).ToList()
                    };

                    var repairForm = (from x in db.FAC_ABNORMALNOTIFYFORM
                                      join f in db.RFORM
                                      on x.RFORMUNIQUEID equals f.UNIQUEID
                                      join t in db.RFORMTYPE
                                      on f.RFORMTYPEUNIQUEID equals t.UNIQUEID into tmpType
                                      from t in tmpType.DefaultIfEmpty()
                                      join e in db.EQUIPMENT
                                      on f.EQUIPMENTUNIQUEID equals e.UNIQUEID into tmpEquipment
                                      from e in tmpEquipment.DefaultIfEmpty()
                                      join p in db.EQUIPMENTPART
                                      on f.PARTUNIQUEID equals p.UNIQUEID into tmpPart
                                      from p in tmpPart.DefaultIfEmpty()
                                      where x.UNIQUEID == model.UniqueID
                                      select new
                                      {
                                          MaintenanceOrganizationUniqueID = f.PMORGANIZATIONUNIQUEID,
                                          f.ORGANIZATIONUNIQUEID,
                                          UniqueID = f.UNIQUEID,
                                          f.VHNO,
                                          f.STATUS,
                                          RepairFormType = t != null ? t.DESCRIPTION : "",
                                          EstBeginDate = f.ESTBEGINDATE,
                                          EstEndDate = f.ESTENDDATE,
                                          Subject = f.SUBJECT,
                                          Descriptipn = f.DESCRIPTION,
                                          EquipmentUniqueID = f.EQUIPMENTUNIQUEID,
                                          EquipmentID = e != null ? e.ID : "",
                                          EquipmentName = e != null ? e.NAME : "",
                                          PartUniqueID = f.PARTUNIQUEID,
                                          PartDescription = p != null ? p.DESCRIPTION : ""
                                      }).FirstOrDefault();

                    if (repairForm != null)
                    {
                        model.RepairForm = new RepairFormModel()
                        {
                            UniqueID = repairForm.UniqueID,
                            ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(repairForm.ORGANIZATIONUNIQUEID),
                            MaintenanceOrganizationDescription = OrganizationDataAccessor.GetOrganizationFullDescription(repairForm.MaintenanceOrganizationUniqueID),
                            VHNO = repairForm.VHNO,
                            Subject = repairForm.Subject,
                            Description = repairForm.Descriptipn,
                            EstBeginDate = repairForm.EstBeginDate,
                            EstEndDate = repairForm.EstEndDate,
                            EquipmentID = repairForm.EquipmentID,
                            EquipmentName = repairForm.EquipmentName,
                            PartDescription = repairForm.PartDescription,
                            RepairFormType = repairForm.RepairFormType,
                            Status = repairForm.STATUS
                        };
                    }
                    else
                    {
                        model.RepairForm = null;
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

        public static RequestResult GetUserTreeItem(List<Models.Shared.Organization> OrganizationList, List<Models.Shared.UserModel> UserList, string OrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>() 
                { 
                    { Define.EnumTreeAttribute.NodeType, string.Empty },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.OrganizationUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.UserID, string.Empty }
                };

                var userList = UserList.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                foreach (var user in userList)
                {
                    var treeItem = new TreeItem() { Title = user.Name };

                    attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.User.ToString();
                    attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", user.ID, user.Name);
                    attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                    attributes[Define.EnumTreeAttribute.UserID] = user.ID;

                    foreach (var attribute in attributes)
                    {
                        treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                    }

                    treeItemList.Add(treeItem);
                }

                var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                foreach (var organization in organizationList)
                {
                    var treeItem = new TreeItem() { Title = organization.Description };

                    attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                    attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                    attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                    attributes[Define.EnumTreeAttribute.UserID] = string.Empty;

                    foreach (var attribute in attributes)
                    {
                        treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                    }

                    bool haveDownStreamOrganizaiton = OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID);

                    bool haveUser = UserList.Any(x => x.OrganizationUniqueID == organization.UniqueID);

                    if (haveDownStreamOrganizaiton || haveUser)
                    {
                        treeItem.State = "closed";
                    }

                    treeItemList.Add(treeItem);
                }

                result.ReturnData(treeItemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult AddUser(List<Models.ASE.Shared.ASEUserModel> UserList, List<string> SelectedList)
        {
            RequestResult result = new RequestResult();

            try
            {
                string[] seperator = new string[] { Define.Seperator };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    foreach (string selected in SelectedList)
                    {
                        string[] temp = selected.Split(seperator, StringSplitOptions.None);

                        var organizationUniqueID = temp[0];
                        var userID = temp[1];

                        if (!string.IsNullOrEmpty(userID))
                        {
                            if (!UserList.Any(x => x.ID == userID))
                            {
                                UserList.Add((from u in db.ACCOUNT
                                              join o in db.ORGANIZATION
                                                  on u.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                              where u.ID == userID
                                              select new Models.ASE.Shared.ASEUserModel
                                              {
                                                  ID = u.ID,
                                                  Name = u.NAME,
                                                   Email=u.EMAIL,
                                                  OrganizationDescription = o.DESCRIPTION
                                              }).First());
                            }
                        }
                        else
                        {
                            var organizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organizationUniqueID, true);

                            var userList = (from u in db.ACCOUNT
                                            join o in db.ORGANIZATION
                                           on u.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                            where organizationList.Contains(u.ORGANIZATIONUNIQUEID)
                                            select new
                                            {
                                                ID = u.ID,
                                                Name = u.NAME,
                                                Email = u.EMAIL,
                                                OrganizationDescription = o.DESCRIPTION
                                            }).ToList();

                            foreach (var user in userList)
                            {
                                if (!UserList.Any(x => x.ID == user.ID))
                                {
                                    UserList.Add(new Models.ASE.Shared.ASEUserModel()
                                    {
                                        ID = user.ID,
                                        Name = user.Name,
                                        Email = user.Email,
                                        OrganizationDescription = user.OrganizationDescription
                                    });
                                }
                            }
                        }
                    }
                }

                result.ReturnData(UserList.OrderBy(x => x.OrganizationDescription).ThenBy(x => x.ID).ToList());
            }
            catch (Exception ex)
            {
                var err = new Error(MethodInfo.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }
    }
}
