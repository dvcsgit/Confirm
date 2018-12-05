using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.Authenticated;
using Models.EquipmentMaintenance.QFormManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace DataAccess.EquipmentMaintenance
{
    public class QFormDataAccessor
    {
        public static RequestResult Query()
        {
            RequestResult result = new RequestResult();

            try
            {
                result = Query(new QueryParameters());

                if (result.IsSuccess)
                {
                    var itemList = result.Data as List<GridItem>;

                    itemList = itemList.Where(x => x.Status == 1 || x.Status == 2).ToList();

                    result.ReturnData(itemList);
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
                using (EDbEntities edb = new EDbEntities())
                {
                    var query = edb.QForm.AsQueryable();

                    if (Parameters.BeginDate.HasValue)
                    {
                        query = query.Where(x => DateTime.Compare(Parameters.BeginDate.Value, x.CreateTime) <= 0);
                    }

                    if (Parameters.EndDate.HasValue)
                    {
                        query = query.Where(x => DateTime.Compare(Parameters.EndDate.Value, x.CreateTime) >= 0);
                    }

                    if (!string.IsNullOrEmpty(Parameters.VHNO))
                    {
                        query = query.Where(x => x.VHNO.Contains(Parameters.VHNO));
                    }

                    var itemList = query.Select(x => new GridItem()
                    {
                        UniqueID = x.UniqueID,
                        VHNO = x.VHNO,
                        Subject = x.Subject,
                        ContactName = x.ContactName,
                        CreateTime = x.CreateTime,
                        JobUserID = x.JobUserID,
                        TakeJobTime = x.TakeJobTime,
                        IsClosed = x.IsClosed,
                        ClosedTime = x.ClosedTime
                    }).ToList();

                    if (!string.IsNullOrEmpty(Parameters.Status))
                    {
                        var status = int.Parse(Parameters.Status);

                        itemList = itemList.Where(x => x.Status == status).ToList();
                    }

                    using (DbEntities db = new DbEntities())
                    {
                        foreach (var item in itemList)
                        {
                            if (!string.IsNullOrEmpty(item.JobUserID))
                            {
                                var user = db.User.FirstOrDefault(x => x.ID == item.JobUserID);

                                if (user != null)
                                {
                                    item.JobUserName = user.Name;
                                }
                            }
                        }
                    }

                    result.ReturnData(itemList.OrderByDescending(x => x.VHNO).ToList());
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

        public static RequestResult Create(CreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities edb = new EDbEntities())
                {
                    var uniqueID = Guid.NewGuid().ToString();

                    var vhno = DateTime.Today.ToString("yyyyMM");

                    var seq = 1;

                    var temp = edb.QForm.Where(x => x.VHNO.StartsWith(vhno)).OrderByDescending(x => x.VHNO).FirstOrDefault();

                    if (temp != null)
                    {
                        seq = int.Parse(temp.VHNO.Substring(6)) + 1;
                    }

                    vhno = string.Format("{0}{1}", DateTime.Today.ToString("yyyyMM"), seq.ToString().PadLeft(4, '0'));

                    edb.QForm.Add(new QForm()
                    {
                        UniqueID = uniqueID,
                        VHNO = vhno,
                        Subject = Model.FormInput.Subject,
                        Description = Model.FormInput.Description,
                        ContactName = Model.FormInput.ContactName,
                        ContactEmail = Model.FormInput.ContactEmail,
                        ContactTel = Model.FormInput.ContactTel,
                        CreateTime = DateTime.Now,
                        IsClosed = false
                    });

                    edb.SaveChanges();

                    result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Create, Resources.Resource.Success));

                    if (Config.HaveMailSetting)
                    {
                        SendQFormNotifyMail(uniqueID);
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
            RequestResult result = new RequestResult();

            try
            {
                var model = new DetailViewModel();

                using (EDbEntities db = new EDbEntities())
                {
                    var query = db.QForm.First(x => x.UniqueID == UniqueID);

                    model = new DetailViewModel()
                    {
                        UniqueID = query.UniqueID,
                        VHNO = query.VHNO,
                        Subject = query.Subject,
                        Description = query.Description,
                        ContactName = query.ContactName,
                        ContactTel = query.ContactTel,
                        ContactEmail = query.ContactEmail,
                        CreateTime = query.CreateTime,
                        JobUserID = query.JobUserID,
                        TakeJobTime = query.TakeJobTime,
                        Comment = query.Comment,
                        IsClosed = query.IsClosed,
                        ClosedTime = query.ClosedTime
                    };

                    var repairFormList = (from x in db.QFormRForm
                                          join r in db.RForm
                                          on x.RFormUniqueID equals r.UniqueID
                                          where x.QFormUniqueID == UniqueID
                                          select r).ToList();

                    foreach (var form in repairFormList)
                    {
                        var item = new RepairFormModel()
                        {
                            UniqueID = form.UniqueID,
                            Status = form.Status,
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(form.OrganizationUniqueID),
                            MaintenanceOrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(form.MaintenanceOrganizationUniqueID),
                            VHNO = form.VHNO,
                            Subject = form.Subject,
                             JobTime=form.ManagerJobTime,
                              RefuseReason=form.JobRefuseReason,
                            EstBeginDate = form.EstBeginDate,
                            EstEndDate = form.EstEndDate,
                            JobRefuseReason = form.JobRefuseReason,
                            JobUserID = form.TakeJobUserID,
                            TakeJobTime = form.TakeJobTime
                        };

                        var repairFormType = db.RFormType.FirstOrDefault(x => x.UniqueID == form.RFormTypeUniqueID);

                        if (repairFormType != null)
                        {
                            item.RepairFormType = repairFormType.Description;
                        }
                        else
                        {
                            item.RepairFormType = "-";
                        }

                        if (!string.IsNullOrEmpty(form.EquipmentUniqueID))
                        {
                            var equipment = db.Equipment.FirstOrDefault(x => x.UniqueID == form.EquipmentUniqueID);

                            if (equipment != null)
                            {
                                item.EquipmentID = equipment.ID;
                                item.EquipmentName = equipment.Name;

                                if (!string.IsNullOrEmpty(form.PartUniqueID))
                                {
                                    var part = db.EquipmentPart.FirstOrDefault(x => x.EquipmentUniqueID == equipment.UniqueID && x.UniqueID == form.PartUniqueID);

                                    if (part != null)
                                    {
                                        item.PartDescription = part.Description;
                                    }
                                }
                            }
                        }

                        model.RepairFormList.Add(item);
                    }
                }

                if (!string.IsNullOrEmpty(model.JobUserID))
                {
                    using (DbEntities db = new DbEntities())
                    {
                        var user = db.User.FirstOrDefault(x => x.ID == model.JobUserID);

                        if (user != null)
                        {
                            model.JobUserName = user.Name;
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

        public static RequestResult Edit(string UniqueID, string Comment)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var form = db.QForm.First(x => x.UniqueID == UniqueID);

                    form.Comment = Comment;

                    db.SaveChanges();

                    result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Edit, Resources.Resource.Success));
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
                using (EDbEntities edb = new EDbEntities())
                {
                    var form = edb.QForm.First(x => x.UniqueID == UniqueID);

                    form.JobUserID = Account.ID;
                    form.TakeJobTime = DateTime.Now;

                    edb.SaveChanges();

                    if (Config.HaveMailSetting)
                    {
                        SendQFormTakeJobNotifyMail(UniqueID);
                    }
                }

                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.TakeJob, Resources.Resource.Complete));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult Closed(string UniqueID, string Comment)
        {
            RequestResult result = new RequestResult();

            try
            {
                result = Edit(UniqueID, Comment);

                if (result.IsSuccess)
                {
                    using (EDbEntities edb = new EDbEntities())
                    {
                        var form = edb.QForm.First(x => x.UniqueID == UniqueID);

                        if (string.IsNullOrEmpty(form.Comment))
                        {
                            result.ReturnFailedMessage(Resources.Resource.ClosedQFormRequired);
                        }
                        else
                        {
                            form.IsClosed = true;
                            form.ClosedTime = DateTime.Now;

                            edb.SaveChanges();

                            result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Closed, Resources.Resource.Success));

                            if (Config.HaveMailSetting)
                            {
                                SendQFormClosedNotifyMail(UniqueID);
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

        public static RequestResult GetCreateRepairFormModel(string QFormUniqueID, string OrganizationUniqueID, string Subject, string Description)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new CreateRepairFormModel()
                {
                    QFormUniqueID = QFormUniqueID,
                    FormInput = new CreateRepairFormInput()
                    {
                        Subject = Subject,
                        Description = Description
                    },
                    RepairFormTypeSelectItemList = new List<SelectListItem>() 
                    { 
                        Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                    },
                    SubjectSelectItemList = new List<SelectListItem>() 
                    { 
                        Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                    }
                };

                using (EDbEntities db = new EDbEntities())
                {
                    var ancestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(OrganizationUniqueID);

                    var repairFormTypeList = db.RFormType.Where(x => x.AncestorOrganizationUniqueID == ancestorOrganizationUniqueID).OrderBy(x => x.Description).ToList();

                    foreach (var repairFormType in repairFormTypeList)
                    {
                        model.RepairFormTypeSelectItemList.Add(new SelectListItem()
                        {
                            Value = repairFormType.UniqueID,
                            Text = repairFormType.Description
                        });

                        model.RepairFormTypeSubjectList.Add(repairFormType.UniqueID, (from x in db.RFormTypeSubject
                                                                                      join s in db.RFormSubject
                                                                                      on x.SubjectUniqueID equals s.UniqueID
                                                                                      where x.RFormTypeUniqueID == repairFormType.UniqueID
                                                                                      orderby x.Seq
                                                                                      select new Models.EquipmentMaintenance.QFormManagement.RFormSubject
                                                                                      {
                                                                                          UniqueID = s.UniqueID,
                                                                                          ID = s.ID,
                                                                                          Description = s.Description,
                                                                                          AncestorOrganizationUniqueID = s.AncestorOrganizationUniqueID
                                                                                      }).ToList());
                    }

                    var organization = OrganizationDataAccessor.GetOrganization(OrganizationUniqueID);

                    model.OrganizationUniqueID = OrganizationUniqueID;
                    model.AncestorOrganizationUniqueID = organization.AncestorOrganizationUniqueID;
                    model.FullOrganizationDescription = organization.FullDescription;

                    model.EquipmentSelectItemList = new List<SelectListItem>() 
                    { 
                        Define.DefaultSelectListItem(Resources.Resource.None)
                    };

                    var equipmentList = db.Equipment.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                    foreach (var equipment in equipmentList)
                    {
                        model.EquipmentSelectItemList.Add(new SelectListItem()
                        {
                            Value = string.Format("{0}{1}{2}", equipment.UniqueID, Define.Seperator, "*"),
                            Text = string.Format("{0}/{1}", equipment.ID, equipment.Name)
                        });

                        var partList = db.EquipmentPart.Where(x => x.EquipmentUniqueID == equipment.UniqueID).OrderBy(x => x.Description).ToList();

                        foreach (var part in partList)
                        {
                            model.EquipmentSelectItemList.Add(new SelectListItem()
                            {
                                Value = string.Format("{0}{1}{2}", equipment.UniqueID, Define.Seperator, part.UniqueID),
                                Text = string.Format("{0}/{1}-{2}", equipment.ID, equipment.Name, part.Description)
                            });
                        }

                        if (!string.IsNullOrEmpty(equipment.MaintenanceOrganizationUniqueID))
                        {
                            model.EquipmentMaintenanceOrganizationList.Add(equipment.UniqueID, OrganizationDataAccessor.GetOrganization(equipment.MaintenanceOrganizationUniqueID));
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

        public static RequestResult CreateRepairForm(CreateRepairFormModel Model, string UserID)
        {
            RequestResult result = new RequestResult();

            try
            {
                string jobManagerID = string.Empty;
                string maintenanceOrganizationUniqueID = string.Empty;

                using (DbEntities db = new DbEntities())
                {
                    maintenanceOrganizationUniqueID = Model.FormInput.MaintenanceOrganizationUniqueID;

                    var organization = db.Organization.First(x => x.UniqueID == Model.FormInput.MaintenanceOrganizationUniqueID);

                    if (!string.IsNullOrEmpty(organization.ManagerUserID))
                    {
                        jobManagerID = organization.ManagerUserID;
                    }
                    else
                    {
                        jobManagerID = string.Empty;

                        result.ReturnFailedMessage(string.Format("{0} {1} {2} {3}", Resources.Resource.Organization, organization.Description, Resources.Resource.NotSet, Resources.Resource.Manager));
                    }
                }

                using (EDbEntities db = new EDbEntities())
                {


                    if (!string.IsNullOrEmpty(jobManagerID))
                    {
                        var equipmentUniqueID = string.Empty;
                        var partUniqueID = string.Empty;

                        if (!string.IsNullOrEmpty(Model.FormInput.EquipmentUniqueID))
                        {
                            string[] t = Model.FormInput.EquipmentUniqueID.Split(Define.Seperators, StringSplitOptions.None);

                            equipmentUniqueID = t[0];
                            partUniqueID = t[1];
                        }

                        var vhnoPrefix = string.Format("R{0}", DateTime.Today.ToString("yyyyMM").Substring(2));

                        var seq = 1;

                        var temp = db.RForm.Where(x => x.VHNO.StartsWith(vhnoPrefix)).OrderByDescending(x => x.VHNO).FirstOrDefault();

                        if (temp != null)
                        {
                            seq = int.Parse(temp.VHNO.Substring(5)) + 1;
                        }

                        var vhno = string.Format("{0}{1}", vhnoPrefix, seq.ToString().PadLeft(4, '0'));

                        var uniqueID = Guid.NewGuid().ToString();

                        var createTime = DateTime.Now;

                        var equipment = db.Equipment.FirstOrDefault(x => x.UniqueID == equipmentUniqueID);
                        var part = db.EquipmentPart.FirstOrDefault(x => x.UniqueID == partUniqueID);

                        var form = new RForm()
                        {
                            UniqueID = uniqueID,
                            Status = "0",
                            VHNO = vhno,
                            OrganizationUniqueID = Model.OrganizationUniqueID,
                            MaintenanceOrganizationUniqueID = maintenanceOrganizationUniqueID,
                            EquipmentUniqueID = equipmentUniqueID,
                            PartUniqueID = partUniqueID,
                            RFormTypeUniqueID = Model.FormInput.RepairFormTypeUniqueID,
                            Subject = Model.FormInput.Subject,
                            Description = Model.FormInput.Description,
                            CreateUserID = UserID,
                            CreateTime = createTime,
                            EquipmentID = equipment != null ? equipment.ID : string.Empty,
                            EquipmentName = equipment != null ? equipment.Name : string.Empty,
                            PartDescription = part != null ? part.Description : string.Empty
                        };

                        db.RForm.Add(form);

                        db.QFormRForm.Add(new QFormRForm()
                        {
                            RFormUniqueID = uniqueID,
                            QFormUniqueID = Model.QFormUniqueID
                        });

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

        public static void Closed(string RFormUniqueID)
        {
            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var qFormUniqueID = db.QFormRForm.Where(x => x.RFormUniqueID == RFormUniqueID).Select(x => x.QFormUniqueID).FirstOrDefault();

                    if (qFormUniqueID != null)
                    {
                        var isQFormClosed = true;

                        var rFormList = (from x in db.QFormRForm
                                         join r in db.RForm
                                         on x.RFormUniqueID equals r.UniqueID
                                         where x.QFormUniqueID == qFormUniqueID
                                         select r).ToList();

                        foreach (var rForm in rFormList)
                        {
                            var isRFormClosed = rForm.Status == "8";

                            if (!isRFormClosed)
                            {
                                var temp = db.RFormFlow.FirstOrDefault(x => x.RFormUniqueID == rForm.UniqueID);

                                isRFormClosed = temp != null && temp.IsClosed;
                            }

                            isQFormClosed = isQFormClosed && isRFormClosed;
                        }

                        if (isQFormClosed)
                        {
                            var qForm = db.QForm.First(x => x.UniqueID == qFormUniqueID);

                            qForm.IsClosed = true;
                            qForm.ClosedTime = DateTime.Now;

                            db.SaveChanges();

                            if (Config.HaveMailSetting)
                            {
                                SendQFormClosedNotifyMail(qFormUniqueID);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private static void SendQFormNotifyMail(string QFormUniqueID)
        {
            try
            {
                using (EDbEntities edb = new EDbEntities())
                {
                    var form = edb.QForm.First(x => x.UniqueID == QFormUniqueID);

                    var mailAddressList = new List<MailAddress>();

                    var managerList = new List<string>();

                    managerList = edb.QFormManager.Select(x => x.UserID).ToList();

                    using (DbEntities db = new DbEntities())
                    {
                        foreach (var manager in managerList)
                        {
                            var user = db.User.FirstOrDefault(x => x.ID == manager);

                            if (user != null && !string.IsNullOrEmpty(user.Email))
                            {
                                mailAddressList.Add(new MailAddress(user.Email, user.Name));
                            }
                        }
                    }

                    if (mailAddressList.Count > 0)
                    {
                        var subject = string.Format("[{0}][{1}]{2}", Resources.Resource.QFormNotify, form.VHNO, form.Subject);

                        StringBuilder sb = new StringBuilder();

                        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.VHNO, form.VHNO));
                        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Subject, form.Subject));
                        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Description, form.Description));
                        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Contact, form.ContactName));
                        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.ContactTel, form.ContactTel));
                        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.ContactEmail, form.ContactEmail));
                        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.CreateTime, DateTimeHelper.DateTime2DateTimeStringWithSeperator(form.CreateTime)));

                        MailHelper.SendMail(mailAddressList, subject, sb.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private static void SendQFormTakeJobNotifyMail(string QFormUniqueID)
        {
            try
            {
                using (EDbEntities edb = new EDbEntities())
                {
                    var form = edb.QForm.First(x => x.UniqueID == QFormUniqueID);

                    var mailAddressList = new List<MailAddress>();

                    var managerList = new List<string>();

                    managerList = edb.QFormManager.Select(x => x.UserID).ToList();

                    using (DbEntities db = new DbEntities())
                    {
                        foreach (var manager in managerList)
                        {
                            var user = db.User.FirstOrDefault(x => x.ID == manager);

                            if (user != null && !string.IsNullOrEmpty(user.Email))
                            {
                                mailAddressList.Add(new MailAddress(user.Email, user.Name));
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(form.ContactEmail))
                    {
                        mailAddressList.Add(new MailAddress(form.ContactEmail, form.ContactName));
                    }

                    if (mailAddressList.Count > 0)
                    {
                        var subject = string.Format("[{0}][{1}]{2}", Resources.Resource.QFormTakeJobNotify, form.VHNO, form.Subject);

                        StringBuilder sb = new StringBuilder();

                        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.VHNO, form.VHNO));
                        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Subject, form.Subject));
                        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Description, form.Description));
                        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Contact, form.ContactName));
                        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.ContactTel, form.ContactTel));
                        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.ContactEmail, form.ContactEmail));
                        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.CreateTime, DateTimeHelper.DateTime2DateTimeStringWithSeperator(form.CreateTime)));

                        var takeJobUser = UserDataAccessor.GetUser(form.JobUserID);

                        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.TakeJobUser, takeJobUser.User));

                        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.TakeJobTime, DateTimeHelper.DateTime2DateTimeStringWithSeperator(form.TakeJobTime)));

                        MailHelper.SendMail(mailAddressList, subject, sb.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private static void SendQFormClosedNotifyMail(string QFormUniqueID)
        {
            try
            {
                using (EDbEntities edb = new EDbEntities())
                {
                    var mailAddressList = new List<MailAddress>();

                    var managerList = new List<string>();

                    managerList = edb.QFormManager.Select(x => x.UserID).ToList();

                    using (DbEntities db = new DbEntities())
                    {
                        foreach (var manager in managerList)
                        {
                            var user = db.User.FirstOrDefault(x => x.ID == manager);

                            if (user != null && !string.IsNullOrEmpty(user.Email))
                            {
                                mailAddressList.Add(new MailAddress(user.Email, user.Name));
                            }
                        }
                    }

                    var form = edb.QForm.First(x => x.UniqueID == QFormUniqueID);

                    if (!string.IsNullOrEmpty(form.ContactEmail))
                    {
                        mailAddressList.Add(new MailAddress(form.ContactEmail, form.ContactName));
                    }

                    if (mailAddressList.Count > 0)
                    {
                        var subject = string.Format("[{0}][{1}]{2}", Resources.Resource.QFormClosedNotify, form.VHNO, form.Subject);

                        StringBuilder sb = new StringBuilder();

                        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.VHNO, form.VHNO));
                        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Subject, form.Subject));
                        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Description, form.Description));
                        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Contact, form.ContactName));
                        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.ContactTel, form.ContactTel));
                        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.ContactEmail, form.ContactEmail));
                        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.CreateTime, DateTimeHelper.DateTime2DateTimeStringWithSeperator(form.CreateTime)));

                        var jobUser = UserDataAccessor.GetUser(form.JobUserID);

                        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.TakeJobUser, jobUser.User));
                        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.TakeJobTime, DateTimeHelper.DateTime2DateTimeStringWithSeperator(form.TakeJobTime)));

                        var comment = string.Empty;

                        if (edb.QFormRForm.Any(x => x.QFormUniqueID == form.UniqueID))
                        {
                            comment = Resources.Resource.TransRepairForm;

                            if (!string.IsNullOrEmpty(form.Comment))
                            {
                                comment = Resources.Resource.TransRepairForm + "、" + form.Comment;
                            }
                        }
                        else
                        {
                            comment = form.Comment;
                        }

                        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Comment, comment));
                        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.ClosedTime, DateTimeHelper.DateTime2DateTimeStringWithSeperator(form.ClosedTime)));

                        MailHelper.SendMail(mailAddressList, subject, sb.ToString());
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
