using DbEntity.ASE;
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

namespace DataAccess.ASE
{
    //public class QFormDataAccessor
    //{
    //    public static RequestResult Query()
    //    {
    //        RequestResult result = new RequestResult();

    //        try
    //        {
    //            result = Query(new QueryParameters());

    //            if (result.IsSuccess)
    //            {
    //                var itemList = result.Data as List<GridItem>;

    //                itemList = itemList.Where(x => x.Status == 1 || x.Status == 2).ToList();

    //                result.ReturnData(itemList);
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            var err = new Error(MethodBase.GetCurrentMethod(), ex);

    //            Logger.Log(err);

    //            result.ReturnError(err);
    //        }

    //        return result;
    //    }

    //    public static RequestResult Query(QueryParameters Parameters)
    //    {
    //        RequestResult result = new RequestResult();

    //        try
    //        {
    //            using (ASEDbEntities db = new ASEDbEntities())
    //            {
    //                var query = db.QForm.AsQueryable();

    //                if (Parameters.BeginDate.HasValue)
    //                {
    //                    query = query.Where(x => DateTime.Compare(Parameters.BeginDate.Value, x.CreateTime) <= 0);
    //                }

    //                if (Parameters.EndDate.HasValue)
    //                {
    //                    query = query.Where(x => DateTime.Compare(Parameters.EndDate.Value, x.CreateTime) >= 0);
    //                }

    //                if (!string.IsNullOrEmpty(Parameters.VHNO))
    //                {
    //                    query = query.Where(x => x.VHNO.Contains(Parameters.VHNO));
    //                }

    //                var itemList = query.Select(x => new GridItem()
    //                {
    //                    UniqueID = x.UNIQUEID,
    //                    VHNO = x.VHNO,
    //                    Subject = x.Subject,
    //                    ContactName = x.ContactName,
    //                    CreateTime = x.CreateTime,
    //                    JobUserID = x.JobUserID,
    //                    TakeJobTime = x.TakeJobTime,
    //                    IsClosed = x.IsClosed,
    //                    ClosedTime = x.ClosedTime
    //                }).ToList();

    //                if (!string.IsNullOrEmpty(Parameters.Status))
    //                {
    //                    var status = int.Parse(Parameters.Status);

    //                    itemList = itemList.Where(x => x.Status == status).ToList();
    //                }

    //                foreach (var item in itemList)
    //                {
    //                    if (!string.IsNullOrEmpty(item.JobUserID))
    //                    {
    //                        var user = db.ACCOUNT.FirstOrDefault(x => x.ID == item.JobUserID);

    //                        if (user != null)
    //                        {
    //                            item.JobUserName = user.Name;
    //                        }
    //                    }
    //                }

    //                result.ReturnData(itemList.OrderBy(x => x.VHNO).ToList());
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            var err = new Error(MethodBase.GetCurrentMethod(), ex);

    //            Logger.Log(err);

    //            result.ReturnError(err);
    //        }

    //        return result;
    //    }

    //    public static RequestResult Create(CreateFormModel Model)
    //    {
    //        RequestResult result = new RequestResult();

    //        try
    //        {
    //            using (ASEDbEntities db = new ASEDbEntities())
    //            {
    //                var uniqueID = Guid.NewGuid().ToString();

    //                var vhno = DateTime.Today.ToString("yyyyMM");

    //                var seq = 1;

    //                var temp = db.QForm.Where(x => x.VHNO.StartsWith(vhno)).OrderByDescending(x => x.VHNO).FirstOrDefault();

    //                if (temp != null)
    //                {
    //                    seq = int.Parse(temp.VHNO.Substring(6)) + 1;
    //                }

    //                vhno = string.Format("{0}{1}", DateTime.Today.ToString("yyyyMM"), seq.ToString().PadLeft(4, '0'));

    //                db.QForm.Add(new QForm()
    //                {
    //                    UniqueID = uniqueID,
    //                    VHNO = vhno,
    //                    Subject = Model.FormInput.Subject,
    //                    Description = Model.FormInput.DESCRIPTION,
    //                    ContactName = Model.FormInput.ContactName,
    //                    ContactEmail = Model.FormInput.ContactEmail,
    //                    ContactTel = Model.FormInput.ContactTel,
    //                    CreateTime = DateTime.Now,
    //                    IsClosed = false
    //                });

    //                db.SaveChanges();

    //                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Create, Resources.Resource.Success));

    //                if (Config.HaveMailSetting)
    //                {
    //                    SendQFormNotifyMail(uniqueID);
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            var err = new Error(MethodBase.GetCurrentMethod(), ex);

    //            Logger.Log(err);

    //            result.ReturnError(err);
    //        }

    //        return result;
    //    }

    //    public static RequestResult GetDetailViewModel(string UniqueID)
    //    {
    //        RequestResult result = new RequestResult();

    //        try
    //        {
    //            var model = new DetailViewModel();

    //            using (ASEDbEntities db = new ASEDbEntities())
    //            {
    //                var query = db.QForm.First(x => x.UNIQUEID == UniqueID);

    //                model = new DetailViewModel()
    //                {
    //                    UniqueID = query.UNIQUEID,
    //                    VHNO = query.VHNO,
    //                    Subject = query.Subject,
    //                    Description = query.DESCRIPTION,
    //                    ContactName = query.ContactName,
    //                    ContactTel = query.ContactTel,
    //                    ContactEmail = query.ContactEmail,
    //                    CreateTime = query.CreateTime,
    //                    JobUserID = query.JobUserID,
    //                    TakeJobTime = query.TakeJobTime,
    //                    Comment = query.Comment,
    //                    IsClosed = query.IsClosed,
    //                    ClosedTime = query.ClosedTime
    //                };

    //                var repairFormList = (from x in db.QFormRForm
    //                                      join r in db.RFORM
    //                                      on x.RFORMUNIQUEID equals r.UNIQUEID
    //                                      where x.QFormUniqueID == UniqueID
    //                                      select r).ToList();

    //                foreach (var form in repairFormList)
    //                {
    //                    var item = new RepairFormModel()
    //                    {
    //                        UniqueID = form.UNIQUEID,
    //                        OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(form.ORGANIZATIONUNIQUEID),
    //                        MaintenanceOrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(form.PMORGANIZATIONUNIQUEID),
    //                        VHNO = form.VHNO,
    //                        IsSubmit = form.IsSubmit,
    //                        Subject = form.Subject,
    //                        EstBeginDate = form.EstBeginDate,
    //                        EstEndDate = form.EstEndDate,
    //                        RefuseReason = form.RefuseReason,
    //                        JobRefuseReason = form.JobRefuseReason,
    //                        JobTime = form.JobTime,
    //                        JobUserID = form.TakeJobUserID,
    //                        TakeJobTime = form.TakeJobTime
    //                    };

    //                    var flow = db.RFORMFLOW.FirstOrDefault(x => x.RFORMUNIQUEID == item.UNIQUEID);

    //                    if (flow != null)
    //                    {
    //                        item.IsClosed = flow.IsClosed;
    //                    }
    //                    else
    //                    {
    //                        item.IsClosed = false;
    //                    }

    //                    if (!string.IsNullOrEmpty(form.MFormUniqueID))
    //                    {
    //                        item.RepairFormType = Resources.Resource.MaintenanceRoute;
    //                    }
    //                    else
    //                    {
    //                        var repairFormType = db.RFORMTYPE.FirstOrDefault(x => x.UNIQUEID == form.RFORMTYPEUNIQUEID);

    //                        if (repairFormType != null)
    //                        {
    //                            item.RepairFormType = repairFormType.DESCRIPTION;
    //                        }
    //                        else
    //                        {
    //                            item.RepairFormType = "-";
    //                        }
    //                    }

    //                    if (!string.IsNullOrEmpty(form.EQUIPMENTUNIQUEID))
    //                    {
    //                        var equipment = db.EQUIPMENT.FirstOrDefault(x => x.UNIQUEID == form.EQUIPMENTUNIQUEID);

    //                        if (equipment != null)
    //                        {
    //                            item.EQUIPMENTID = equipment.ID;
    //                            item.EQUIPMENTNAME = equipment.Name;

    //                            if (!string.IsNullOrEmpty(form.PARTUNIQUEID))
    //                            {
    //                                var part = db.EQUIPMENTPART.FirstOrDefault(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.UNIQUEID == form.PARTUNIQUEID);

    //                                if (part != null)
    //                                {
    //                                    item.PARTDESCRIPTION = part.DESCRIPTION;
    //                                }
    //                            }
    //                        }
    //                    }

    //                    model.RepairFormList.Add(item);
    //                }
    //            }

    //            if (!string.IsNullOrEmpty(model.JobUserID))
    //            {
    //                using (ASEDbEntities db = new ASEDbEntities())
    //                {
    //                    var user = db.ACCOUNT.FirstOrDefault(x => x.ID == model.JobUserID);

    //                    if (user != null)
    //                    {
    //                        model.JobUserName = user.Name;
    //                    }
    //                }
    //            }

    //            result.ReturnData(model);
    //        }
    //        catch (Exception ex)
    //        {
    //            var err = new Error(MethodBase.GetCurrentMethod(), ex);

    //            Logger.Log(err);

    //            result.ReturnError(err);
    //        }

    //        return result;
    //    }

    //    public static RequestResult Edit(string UniqueID, string Comment)
    //    {
    //        RequestResult result = new RequestResult();

    //        try
    //        {
    //            using (ASEDbEntities db = new ASEDbEntities())
    //            {
    //                var form = db.QForm.First(x => x.UNIQUEID == UniqueID);

    //                form.Comment = Comment;

    //                db.SaveChanges();

    //                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Edit, Resources.Resource.Success));
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            var err = new Error(MethodBase.GetCurrentMethod(), ex);

    //            Logger.Log(err);

    //            result.ReturnError(err);
    //        }

    //        return result;
    //    }

    //    public static RequestResult TakeJob(string UniqueID, Account Account)
    //    {
    //        RequestResult result = new RequestResult();

    //        try
    //        {
    //            using (ASEDbEntities db = new ASEDbEntities())
    //            {
    //                var form = db.QForm.First(x => x.UNIQUEID == UniqueID);

    //                form.JobUserID = Account.ID;
    //                form.TakeJobTime = DateTime.Now;

    //                db.SaveChanges();

    //                if (Config.HaveMailSetting)
    //                {
    //                    SendQFormTakeJobNotifyMail(UniqueID);
    //                }
    //            }

    //            result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.TakeJob, Resources.Resource.Complete));
    //        }
    //        catch (Exception ex)
    //        {
    //            var err = new Error(MethodBase.GetCurrentMethod(), ex);

    //            Logger.Log(err);

    //            result.ReturnError(err);
    //        }

    //        return result;
    //    }

    //    public static RequestResult Closed(string UniqueID, string Comment)
    //    {
    //        RequestResult result = new RequestResult();

    //        try
    //        {
    //            result = Edit(UniqueID, Comment);

    //            if (result.IsSuccess)
    //            {
    //                using (ASEDbEntities db = new ASEDbEntities())
    //                {
    //                    var form = db.QForm.First(x => x.UNIQUEID == UniqueID);

    //                    if (string.IsNullOrEmpty(form.Comment))
    //                    {
    //                        result.ReturnFailedMessage(Resources.Resource.ClosedQFormRequired);
    //                    }
    //                    else
    //                    {
    //                        form.IsClosed = true;
    //                        form.ClosedTime = DateTime.Now;

    //                        db.SaveChanges();

    //                        result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Closed, Resources.Resource.Success));

    //                        if (Config.HaveMailSetting)
    //                        {
    //                            SendQFormClosedNotifyMail(UniqueID);
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            var err = new Error(MethodBase.GetCurrentMethod(), ex);

    //            Logger.Log(err);

    //            result.ReturnError(err);
    //        }

    //        return result;
    //    }

    //    public static RequestResult GetCreateRepairFormModel(string QFormUniqueID, string OrganizationUniqueID, string Subject, string Description)
    //    {
    //        RequestResult result = new RequestResult();

    //        try
    //        {
    //            var model = new CreateRepairFormModel()
    //            {
    //                QFormUniqueID = QFormUniqueID,
    //                FormInput = new CreateRepairFormInput()
    //                {
    //                    Subject = Subject,
    //                    Description = Description
    //                },
    //                RepairFormTypeSelectItemList = new List<SelectListItem>() 
    //                { 
    //                    Define.DefaultSelectListItem(Resources.Resource.SelectOne)
    //                },
    //                SubjectSelectItemList = new List<SelectListItem>() 
    //                { 
    //                    Define.DefaultSelectListItem(Resources.Resource.SelectOne)
    //                }
    //            };

    //            using (ASEDbEntities db = new ASEDbEntities())
    //            {
    //                var ancestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(OrganizationUniqueID);

    //                var repairFormTypeList = db.RFORMTYPE.Where(x => x.ANCESTORORGANIZATIONUNIQUEID == ancestorOrganizationUniqueID).OrderBy(x => x.DESCRIPTION).ToList();

    //                foreach (var repairFormType in repairFormTypeList)
    //                {
    //                    model.RepairFormTypeSelectItemList.Add(new SelectListItem()
    //                    {
    //                        Value = repairFormType.UNIQUEID,
    //                        Text = repairFormType.DESCRIPTION
    //                    });

    //                    model.RepairFormTypeSubjectList.Add(repairFormType.UNIQUEID, (from x in db.RFORMTYPESUBJECT
    //                                                                                  join s in db.RFORMSUBJECT
    //                                                                                  on x.SubjectUniqueID equals s.UNIQUEID
    //                                                                                  where x.RFORMTYPEUNIQUEID == repairFormType.UNIQUEID
    //                                                                                  orderby x.SEQ
    //                                                                                  select new Models.EquipmentMaintenance.QFormManagement.RFormSubject
    //                                                                                  {
    //                                                                                      UniqueID = s.UNIQUEID,
    //                                                                                      ID = s.ID,
    //                                                                                      Description = s.DESCRIPTION,
    //                                                                                      AncestorOrganizationUniqueID = s.ANCESTORORGANIZATIONUNIQUEID
    //                                                                                  }).ToList());
    //                }

    //                var organization = OrganizationDataAccessor.GetOrganization(OrganizationUniqueID);

    //                model.ORGANIZATIONUNIQUEID = OrganizationUniqueID;
    //                model.ANCESTORORGANIZATIONUNIQUEID = organization.ANCESTORORGANIZATIONUNIQUEID;
    //                model.FullOrganizationDescription = organization.FullDescription;

    //                model.EquipmentSelectItemList = new List<SelectListItem>() 
    //                { 
    //                    Define.DefaultSelectListItem(Resources.Resource.None)
    //                };

    //                var equipmentList = db.EQUIPMENT.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

    //                foreach (var equipment in equipmentList)
    //                {
    //                    model.EquipmentSelectItemList.Add(new SelectListItem()
    //                    {
    //                        Value = string.Format("{0}{1}{2}", equipment.UNIQUEID, Define.Seperator, "*"),
    //                        Text = string.Format("{0}/{1}", equipment.ID, equipment.Name)
    //                    });

    //                    var partList = db.EQUIPMENTPART.Where(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID).OrderBy(x => x.DESCRIPTION).ToList();

    //                    foreach (var part in partList)
    //                    {
    //                        model.EquipmentSelectItemList.Add(new SelectListItem()
    //                        {
    //                            Value = string.Format("{0}{1}{2}", equipment.UNIQUEID, Define.Seperator, part.UNIQUEID),
    //                            Text = string.Format("{0}/{1}-{2}", equipment.ID, equipment.Name, part.DESCRIPTION)
    //                        });
    //                    }

    //                    if (!string.IsNullOrEmpty(equipment.PMORGANIZATIONUNIQUEID))
    //                    {
    //                        model.EquipmentMaintenanceOrganizationList.Add(equipment.UNIQUEID, OrganizationDataAccessor.GetOrganization(equipment.PMORGANIZATIONUNIQUEID));
    //                    }
    //                }
    //            }

    //            result.ReturnData(model);
    //        }
    //        catch (Exception ex)
    //        {
    //            var err = new Error(MethodBase.GetCurrentMethod(), ex);

    //            Logger.Log(err);

    //            result.ReturnError(err);
    //        }

    //        return result;
    //    }

    //    public static RequestResult CreateRepairForm(CreateRepairFormModel Model, string UserID)
    //    {
    //        RequestResult result = new RequestResult();

    //        try
    //        {
    //            string jobManagerID = string.Empty;

    //            using (ASEDbEntities db = new ASEDbEntities())
    //            {
    //                var organization = db.ORGANIZATION.First(x => x.UNIQUEID == Model.FormInput.PMORGANIZATIONUNIQUEID);

    //                if (!string.IsNullOrEmpty(organization.MANAGERUSERID))
    //                {
    //                    jobManagerID = organization.MANAGERUSERID;
    //                }
    //                else
    //                {
    //                    result.ReturnFailedMessage(string.Format("{0} {1} {2} {3}", Resources.Resource.Organization, organization.DESCRIPTION, Resources.Resource.NotSet, Resources.Resource.Manager));
    //                }
    //            }

    //            if (!string.IsNullOrEmpty(jobManagerID))
    //            {
    //                using (ASEDbEntities db = new ASEDbEntities())
    //                {
    //                    var equipmentUniqueID = string.Empty;
    //                    var partUniqueID = string.Empty;

    //                    if (!string.IsNullOrEmpty(Model.FormInput.EQUIPMENTUNIQUEID))
    //                    {
    //                        string[] t = Model.FormInput.EQUIPMENTUNIQUEID.Split(Define.Seperators, StringSplitOptions.None);

    //                        equipmentUniqueID = t[0];
    //                        partUniqueID = t[1];
    //                    }

    //                    var vhno = DateTime.Today.ToString("yyyyMM");

    //                    var seq = 1;

    //                    var temp = db.RFORM.Where(x => x.VHNO.StartsWith(vhno)).OrderByDescending(x => x.VHNO).FirstOrDefault();

    //                    if (temp != null)
    //                    {
    //                        seq = int.Parse(temp.VHNO.Substring(6)) + 1;
    //                    }

    //                    vhno = string.Format("{0}{1}", DateTime.Today.ToString("yyyyMM"), seq.ToString().PadLeft(4, '0'));

    //                    var uniqueID = Guid.NewGuid().ToString();

    //                    db.RFORM.Add(new RForm()
    //                    {
    //                        UniqueID = uniqueID,
    //                        VHNO = vhno,
    //                        OrganizationUniqueID = Model.ORGANIZATIONUNIQUEID,
    //                        MaintenanceOrganizationUniqueID = Model.FormInput.PMORGANIZATIONUNIQUEID,
    //                        EquipmentUniqueID = equipmentUniqueID,
    //                        PartUniqueID = partUniqueID,
    //                        CheckResultUniqueID = string.Empty,
    //                        RFormTypeUniqueID = Model.FormInput.RepairFormTypeUniqueID,
    //                        Subject = Model.FormInput.Subject,
    //                        Description = Model.FormInput.DESCRIPTION,
    //                        CreateUserID = UserID,
    //                        CreateTime = DateTime.Now,
    //                        JobManagerID = jobManagerID,
    //                        IsSubmit = false
    //                    });

    //                    db.QFormRForm.Add(new QFormRForm()
    //                    {
    //                        RFormUniqueID = uniqueID,
    //                        QFormUniqueID = Model.QFormUniqueID
    //                    });

    //                    db.SaveChanges();

    //                    result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.RepairForm, Resources.Resource.Success));
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            var err = new Error(MethodBase.GetCurrentMethod(), ex);

    //            Logger.Log(err);

    //            result.ReturnError(err);
    //        }

    //        return result;
    //    }

    //    public static void Closed(string RFormUniqueID)
    //    {
    //        try
    //        {
    //            using (ASEDbEntities db = new ASEDbEntities())
    //            {
    //                var qFormUniqueID = db.QFormRForm.Where(x => x.RFORMUNIQUEID == RFormUniqueID).Select(x => x.QFormUniqueID).FirstOrDefault();

    //                if (qFormUniqueID != null)
    //                {
    //                    var isQFormClosed = true;

    //                    var rFormList = (from x in db.QFormRForm
    //                                     join r in db.RFORM
    //                                     on x.RFORMUNIQUEID equals r.UNIQUEID
    //                                     where x.QFormUniqueID == qFormUniqueID
    //                                     select r).ToList();

    //                    foreach (var rForm in rFormList)
    //                    {
    //                        var isRFormClosed = !string.IsNullOrEmpty(rForm.RefuseReason);

    //                        if (!isRFormClosed)
    //                        {
    //                            var temp = db.RFORMFLOW.FirstOrDefault(x => x.RFORMUNIQUEID == rForm.UNIQUEID);

    //                            isRFormClosed = temp != null && temp.IsClosed;
    //                        }

    //                        isQFormClosed = isQFormClosed && isRFormClosed;
    //                    }

    //                    if (isQFormClosed)
    //                    {
    //                        var qForm = db.QForm.First(x => x.UNIQUEID == qFormUniqueID);

    //                        qForm.IsClosed = true;
    //                        qForm.ClosedTime = DateTime.Now;

    //                        db.SaveChanges();

    //                        if (Config.HaveMailSetting)
    //                        {
    //                            SendQFormClosedNotifyMail(qFormUniqueID);
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Logger.Log(MethodBase.GetCurrentMethod(), ex);
    //        }
    //    }

    //    private static void SendQFormNotifyMail(string QFormUniqueID)
    //    {
    //        try
    //        {
    //            using (ASEDbEntities db = new ASEDbEntities())
    //            {
    //                var form = db.QForm.First(x => x.UNIQUEID == QFormUniqueID);

    //                var mailAddressList = new List<MailAddress>();

    //                var managerList = new List<string>();

    //                managerList = db.QFormManager.Select(x => x.UserID).ToList();

    //                foreach (var manager in managerList)
    //                {
    //                    var user = db.ACCOUNT.FirstOrDefault(x => x.ID == manager);

    //                    if (user != null && !string.IsNullOrEmpty(user.Email))
    //                    {
    //                        mailAddressList.Add(new MailAddress(user.Email, user.Name));
    //                    }
    //                }

    //                if (mailAddressList.Count > 0)
    //                {
    //                    var subject = string.Format("[{0}][{1}]{2}", Resources.Resource.QFormNotify, form.VHNO, form.Subject);

    //                    StringBuilder sb = new StringBuilder();

    //                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.VHNO, form.VHNO));
    //                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Subject, form.Subject));
    //                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.DESCRIPTION, form.DESCRIPTION));
    //                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Contact, form.ContactName));
    //                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.ContactTel, form.ContactTel));
    //                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.ContactEmail, form.ContactEmail));
    //                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.CreateTime, DateTimeHelper.DateTime2DateTimeStringWithSeperator(form.CreateTime)));

    //                    MailHelper.SendMail(mailAddressList, subject, sb.ToString());
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Logger.Log(MethodBase.GetCurrentMethod(), ex);
    //        }
    //    }

    //    private static void SendQFormTakeJobNotifyMail(string QFormUniqueID)
    //    {
    //        try
    //        {
    //            using (ASEDbEntities db = new ASEDbEntities())
    //            {
    //                var form = db.QForm.First(x => x.UNIQUEID == QFormUniqueID);

    //                var mailAddressList = new List<MailAddress>();

    //                var managerList = new List<string>();

    //                managerList = db.QFormManager.Select(x => x.UserID).ToList();

    //                foreach (var manager in managerList)
    //                {
    //                    var user = db.ACCOUNT.FirstOrDefault(x => x.ID == manager);

    //                    if (user != null && !string.IsNullOrEmpty(user.Email))
    //                    {
    //                        mailAddressList.Add(new MailAddress(user.Email, user.Name));
    //                    }
    //                }

    //                if (!string.IsNullOrEmpty(form.ContactEmail))
    //                {
    //                    mailAddressList.Add(new MailAddress(form.ContactEmail, form.ContactName));
    //                }

    //                if (mailAddressList.Count > 0)
    //                {
    //                    var subject = string.Format("[{0}][{1}]{2}", Resources.Resource.QFormTakeJobNotify, form.VHNO, form.Subject);

    //                    StringBuilder sb = new StringBuilder();

    //                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.VHNO, form.VHNO));
    //                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Subject, form.Subject));
    //                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.DESCRIPTION, form.DESCRIPTION));
    //                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Contact, form.ContactName));
    //                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.ContactTel, form.ContactTel));
    //                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.ContactEmail, form.ContactEmail));
    //                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.CreateTime, DateTimeHelper.DateTime2DateTimeStringWithSeperator(form.CreateTime)));

    //                    var takeJobUser = UserDataAccessor.GetUser(form.JobUserID);

    //                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.TakeJobUser, takeJobUser.User));

    //                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.TakeJobTime, DateTimeHelper.DateTime2DateTimeStringWithSeperator(form.TakeJobTime)));

    //                    MailHelper.SendMail(mailAddressList, subject, sb.ToString());
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Logger.Log(MethodBase.GetCurrentMethod(), ex);
    //        }
    //    }

    //    private static void SendQFormClosedNotifyMail(string QFormUniqueID)
    //    {
    //        try
    //        {
    //            using (ASEDbEntities db = new ASEDbEntities())
    //            {
    //                var mailAddressList = new List<MailAddress>();

    //                var managerList = new List<string>();

    //                managerList = db.QFormManager.Select(x => x.UserID).ToList();

    //                foreach (var manager in managerList)
    //                {
    //                    var user = db.ACCOUNT.FirstOrDefault(x => x.ID == manager);

    //                    if (user != null && !string.IsNullOrEmpty(user.Email))
    //                    {
    //                        mailAddressList.Add(new MailAddress(user.Email, user.Name));
    //                    }
    //                }

    //                var form = db.QForm.First(x => x.UNIQUEID == QFormUniqueID);

    //                if (!string.IsNullOrEmpty(form.ContactEmail))
    //                {
    //                    mailAddressList.Add(new MailAddress(form.ContactEmail, form.ContactName));
    //                }

    //                if (mailAddressList.Count > 0)
    //                {
    //                    var subject = string.Format("[{0}][{1}]{2}", Resources.Resource.QFormClosedNotify, form.VHNO, form.Subject);

    //                    StringBuilder sb = new StringBuilder();

    //                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.VHNO, form.VHNO));
    //                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Subject, form.Subject));
    //                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.DESCRIPTION, form.DESCRIPTION));
    //                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Contact, form.ContactName));
    //                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.ContactTel, form.ContactTel));
    //                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.ContactEmail, form.ContactEmail));
    //                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.CreateTime, DateTimeHelper.DateTime2DateTimeStringWithSeperator(form.CreateTime)));

    //                    var jobUser = UserDataAccessor.GetUser(form.JobUserID);

    //                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.TakeJobUser, jobUser.User));
    //                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.TakeJobTime, DateTimeHelper.DateTime2DateTimeStringWithSeperator(form.TakeJobTime)));

    //                    var comment = string.Empty;

    //                    if (db.QFormRForm.Any(x => x.QFormUniqueID == form.UNIQUEID))
    //                    {
    //                        comment = Resources.Resource.TransRepairForm;

    //                        if (!string.IsNullOrEmpty(form.Comment))
    //                        {
    //                            comment = Resources.Resource.TransRepairForm + "、" + form.Comment;
    //                        }
    //                    }
    //                    else
    //                    {
    //                        comment = form.Comment;
    //                    }

    //                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Comment, comment));
    //                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.ClosedTime, DateTimeHelper.DateTime2DateTimeStringWithSeperator(form.ClosedTime)));

    //                    MailHelper.SendMail(mailAddressList, subject, sb.ToString());
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Logger.Log(MethodBase.GetCurrentMethod(), ex);
    //        }
    //    }
    //}
}
