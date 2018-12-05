using DbEntity.MSSQL;
using DbEntity.MSSQL.AbnormalNotify;
using Models.AbnormalNotify.AbnormalNotify;
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
using Utility;
using Utility.Models;

namespace DataAccess.AbnormalNotify
{
    public class AbnormalNotifyDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, List<Models.Shared.Organization> OrganizationList, List<Models.Shared.UserModel> UserList)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new GridViewModel();

                using (ADbEntities db = new ADbEntities())
                {
                    var query = db.ANForm.AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.BeginDate))
                    {
                        query = query.Where(x => string.Compare(x.OccurDate, Parameters.BeginDate) >= 0);
                    }

                    if (!string.IsNullOrEmpty(Parameters.EndDate))
                    {
                        query = query.Where(x => string.Compare(x.OccurDate, Parameters.EndDate) <= 0);
                    }

                    if (!string.IsNullOrEmpty(Parameters.VHNO))
                    {
                        query = query.Where(x => x.VHNO.Contains(Parameters.VHNO));
                    }

                    var temp = query.OrderByDescending(x => x.VHNO).ToList();

                    foreach (var t in temp)
                    {
                        var createUser = UserList.FirstOrDefault(x => x.ID == t.CreateUserID);

                        model.ItemList.Add(new GridItem()
                        {
                            UniqueID = t.UniqueID,
                            VHNO = t.VHNO,
                            Subject = t.Subject,
                            OccurTime = DateTimeHelper.DateTimeString2DateTime(t.OccurDate, t.OccurTime),
                            CreateUserID = t.CreateUserID,
                            CreateUserName = createUser != null ? createUser.Name : string.Empty,
                            CreateTime = t.CreateTime.Value
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

                using (ADbEntities db = new ADbEntities())
                {
                    var form = db.ANForm.First(x => x.UniqueID == UniqueID);

                    var createUser = UserList.FirstOrDefault(x => x.ID == form.CreateUserID);

                    model = new DetailViewModel()
                    {
                        UniqueID = form.UniqueID,
                        VHNO = form.VHNO,
                        Cost = form.Cost,
                        Location = form.Location,
                        CreateTime = form.CreateTime.Value,
                        CreateUserID = form.CreateUserID,
                        CreateUserName = createUser != null ? createUser.Name : string.Empty,
                        Description = form.Description,
                        EffectArea = form.EffectArea,
                        EffectSystem = form.EffectSystem,
                        FileList = db.ANFormFile.Where(f => f.FormUniqueID == UniqueID).ToList().Select(f => new FileModel
                        {
                            IsSaved = true,
                            Seq = f.Seq,
                            FileName = f.FileName,
                            Extension = f.Extension,
                            Size = f.Length.Value,
                            LastModifyTime = f.LastModifyTime.Value
                        }).OrderBy(f => f.LastModifyTime).ToList(),
                        HandlingDescription = form.HandlingDescription,
                        Mvpn = form.MVPN,
                        OccurTime = DateTimeHelper.DateTimeString2DateTime(form.OccurDate, form.OccurTime),
                        RecoveryDescription = form.RecoveryDescription,
                        RecoveryTime = DateTimeHelper.DateTimeString2DateTime(form.RecoveryDate, form.RecoveryTime),
                        Contact = form.Contact,
                        GroupList = (from x in db.ANFormGroup
                                     join g in db.ANGroup
                                     on x.GroupUniqueID equals g.UniqueID
                                     where x.FormUniqueID == form.UniqueID
                                     select g.Description).OrderBy(x => x).ToList(),
                        Subject = form.Subject,
                        LogList = (from x in db.ANFormLog
                                   where x.FormUniqueID == form.UniqueID
                                   select new
                                   {
                                       x.Action,
                                       x.LogTime,
                                       x.UserID,
                                       x.Seq
                                   }).ToList().Select(x => new LogModel
                                   {
                                       Action = Define.EnumParse<Define.EnumFormAction>(x.Action),
                                       LogTime = x.LogTime.Value,
                                       UserID = x.UserID,
                                       UserName = UserDataAccessor.GetUser(x.UserID).Name,
                                       Seq = x.Seq
                                   }).OrderBy(x => x.Seq).ToList()
                    };

                    var notifyUserList = db.ANFormUser.Where(x => x.FormUniqueID == form.UniqueID).Select(x => x.UserID).ToList();

                    foreach (var notifyUser in notifyUserList)
                    {
                        model.NotifyUserList.Add(UserDataAccessor.GetUser(notifyUser));
                    }

                    var notifyCCUserList = db.ANFormCCUser.Where(x => x.FormUniqueID == form.UniqueID).Select(x => x.UserID).ToList();

                    foreach (var notifyCCUser in notifyCCUserList)
                    {
                        model.NotifyCCUserList.Add(UserDataAccessor.GetUser(notifyCCUser));
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

        public static RequestResult GetCreateFormModel(List<Models.Shared.Organization> OrganizationList, List<Models.Shared.UserModel> AccountList)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ADbEntities db = new ADbEntities())
                {
                    var model = new CreateFormModel()
                    {
                        GroupList = db.ANGroup.OrderBy(x => x.Description).ToDictionary(x => x.UniqueID, x => x.Description),
                        FormInput = new FormInput()
                        {
                            OccurDateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today),
                            OccurTime = DateTime.Now.ToString("HHmm")
                        }
                    };

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
                using (ADbEntities db = new ADbEntities())
                {
                    var occurTime = DateTimeHelper.DateTimeString2DateTime(Model.FormInput.OccurDate, Model.FormInput.OccurTime);

                    if (DateTime.Compare(occurTime.Value, DateTime.Now) <= 0)
                    {
                        var vhnoPrefix = string.Format("A{0}", DateTime.Today.ToString("yyyyMM").Substring(2));

                        var vhnoSeq = 1;

                        var query = db.ANForm.Where(x => x.VHNO.StartsWith(vhnoPrefix)).OrderByDescending(x => x.VHNO).ToList();

                        if (query.Count > 0)
                        {
                            vhnoSeq = int.Parse(query.First().VHNO.Substring(5)) + 1;
                        }

                        var vhno = string.Format("{0}{1}", vhnoPrefix, vhnoSeq.ToString().PadLeft(3, '0'));

                        var uniqueID = Guid.NewGuid().ToString();

                        db.ANForm.Add(new ANForm()
                        {
                            UniqueID = uniqueID,
                            VHNO = vhno,
                            Cost = Model.FormInput.Cost,
                            CreateTime = DateTime.Now,
                            CreateUserID = Account.ID,
                            Description = Model.FormInput.Description,
                            EffectArea = Model.FormInput.EffectArea,
                            EffectSystem = Model.FormInput.EffectSystem,
                            HandlingDescription = Model.FormInput.HandlingDescription,
                            MVPN = Model.FormInput.Mvpn,
                            Location = Model.FormInput.Location,
                            OccurDate = Model.FormInput.OccurDate,
                            OccurTime = Model.FormInput.OccurTime,
                            RecoveryDate = Model.FormInput.RecoveryDate,
                            RecoveryDescription = Model.FormInput.RecoveryDescription,
                            RecoveryTime = Model.FormInput.RecoveryTime,
                            Subject = Model.FormInput.Subject,
                            Contact = Model.FormInput.Contact
                        });

                        db.ANFormGroup.AddRange(Model.FormInput.GroupList.Select(x => new ANFormGroup
                        {
                            FormUniqueID = uniqueID,
                            GroupUniqueID = x
                        }).ToList());

                        db.ANFormLog.Add(new ANFormLog()
                        {
                            FormUniqueID = uniqueID,
                            Seq = 1,
                            Action = Define.EnumFormAction.Create.ToString(),
                            LogTime = DateTime.Now,
                            UserID = Account.ID
                        });

                        db.ANFormUser.AddRange(Model.NotifyUserList.Select(x => new ANFormUser
                        {
                            FormUniqueID = uniqueID,
                            UserID = x.ID
                        }).ToList());

                        db.ANFormCCUser.AddRange(Model.NotifyCCUserList.Select(x => new ANFormCCUser
                        {
                            FormUniqueID = uniqueID,
                            UserID = x.ID
                        }).ToList());

                        foreach (var file in Model.FileList)
                        {
                            db.ANFormFile.Add(new ANFormFile()
                            {
                                FormUniqueID = uniqueID,
                                Seq = file.Seq,
                                Extension = file.Extension,
                                FileName = file.FileName,
                                LastModifyTime = file.LastModifyTime,
                                Length = file.Size
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

        public static RequestResult GetEditFormModel(string UniqueID,List<Models.Shared.UserModel> UserList)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new EditFormModel();

                using (ADbEntities db = new ADbEntities())
                {
                    var form = db.ANForm.First(x => x.UniqueID == UniqueID);

                    var createUser = UserList.FirstOrDefault(x => x.ID == form.CreateUserID);

                    model = new EditFormModel()
                    {
                        GroupList = db.ANGroup.OrderBy(x => x.Description).ToDictionary(x => x.UniqueID, x => x.Description),
                        FormGroupList = db.ANFormGroup.Where(x => x.FormUniqueID == form.UniqueID).Select(x => x.GroupUniqueID).ToList(),
                        UniqueID = form.UniqueID,
                        VHNO = form.VHNO,
                        FormInput = new FormInput()
                        {
                            Cost = form.Cost,
                            Location = form.Location,
                            Description = form.Description,
                            EffectArea = form.EffectArea,
                            EffectSystem = form.EffectSystem,
                            HandlingDescription = form.HandlingDescription,
                            Mvpn = form.MVPN,
                            OccurDateString = DateTimeHelper.DateString2DateStringWithSeparator(form.OccurDate),
                            OccurTime = form.OccurTime,
                            RecoveryDescription = form.RecoveryDescription,
                            RecoveryDateString = DateTimeHelper.DateString2DateStringWithSeparator(form.RecoveryDate),
                            RecoveryTime = form.RecoveryTime,
                            Contact = form.Contact,
                            Subject = form.Subject,
                        },
                        CreateTime = form.CreateTime.Value,
                        CreateUserID = form.CreateUserID,
                        CreateUserName = createUser != null ? createUser.Name : string.Empty,
                        FileList = db.ANFormFile.Where(f => f.FormUniqueID == UniqueID).ToList().Select(f => new FileModel
                        {
                            IsSaved = true,
                            Seq = f.Seq,
                            FileName = f.FileName,
                            Extension = f.Extension,
                            Size = f.Length.Value,
                            LastModifyTime = f.LastModifyTime.Value
                        }).OrderBy(f => f.LastModifyTime).ToList(),
                        LogList = (from x in db.ANFormLog
                                   where x.FormUniqueID == form.UniqueID
                                   select new
                                   {
                                       x.Action,
                                       x.LogTime,
                                       x.UserID,
                                       x.Seq
                                   }).ToList().Select(x => new LogModel
                                   {
                                       Action = Define.EnumParse<Define.EnumFormAction>(x.Action),
                                       LogTime = x.LogTime.Value,
                                       UserID = x.UserID,
                                       UserName = UserDataAccessor.GetUser(x.UserID).Name,
                                       Seq = x.Seq
                                   }).OrderBy(x => x.Seq).ToList()
                    };

                    var notifyUserList = db.ANFormUser.Where(x => x.FormUniqueID == form.UniqueID).Select(x => x.UserID).ToList();

                    foreach (var notifyUser in notifyUserList)
                    {
                        model.NotifyUserList.Add(UserDataAccessor.GetUser(notifyUser));
                    }

                    var notifyCCUserList = db.ANFormCCUser.Where(x => x.FormUniqueID == form.UniqueID).Select(x => x.UserID).ToList();

                    foreach (var notifyCCUser in notifyCCUserList)
                    {
                        model.NotifyCCUserList.Add(UserDataAccessor.GetUser(notifyCCUser));
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
                using (ADbEntities db = new ADbEntities())
                {
                    var occurTime = DateTimeHelper.DateTimeString2DateTime(Model.FormInput.OccurDate, Model.FormInput.OccurTime);

                    if (DateTime.Compare(occurTime.Value, DateTime.Now) <= 0)
                    {
#if !DEBUG
                        using (TransactionScope trans = new TransactionScope())
                        {
#endif
                            var form = db.ANForm.First(x => x.UniqueID == Model.UniqueID);

                            form.Cost = Model.FormInput.Cost;
                            form.Description = Model.FormInput.Description;
                            form.EffectArea = Model.FormInput.EffectArea;
                            form.EffectSystem = Model.FormInput.EffectSystem;
                            form.HandlingDescription = Model.FormInput.HandlingDescription;
                            form.MVPN = Model.FormInput.Mvpn;
                            form.OccurDate = Model.FormInput.OccurDate;
                            form.OccurTime = Model.FormInput.OccurTime;
                            form.RecoveryDate = Model.FormInput.RecoveryDate;
                            form.RecoveryDescription = Model.FormInput.RecoveryDescription;
                            form.RecoveryTime = Model.FormInput.RecoveryTime;
                            form.Contact = Model.FormInput.Contact;
                            form.Location = Model.FormInput.Location;

                            db.SaveChanges();

                            var logSeq = db.ANFormLog.Count(x => x.FormUniqueID == form.UniqueID) + 1;

                            db.ANFormLog.Add(new ANFormLog()
                            {
                                FormUniqueID = form.UniqueID,
                                Seq = logSeq,
                                Action = Define.EnumFormAction.Edit.ToString(),
                                LogTime = DateTime.Now,
                                UserID = Account.ID
                            });

                            db.SaveChanges();

                            db.ANFormGroup.RemoveRange(db.ANFormGroup.Where(x => x.FormUniqueID == form.UniqueID).ToList());

                            db.SaveChanges();

                            db.ANFormGroup.AddRange(Model.FormInput.GroupList.Select(x => new ANFormGroup
                            {
                                FormUniqueID = form.UniqueID,
                                GroupUniqueID = x
                            }).ToList());

                            db.SaveChanges();

                            db.ANFormUser.RemoveRange(db.ANFormUser.Where(x => x.FormUniqueID == form.UniqueID).ToList());

                            db.SaveChanges();

                            db.ANFormUser.AddRange(Model.NotifyUserList.Select(x => new ANFormUser
                            {
                                FormUniqueID = form.UniqueID,
                                UserID = x.ID
                            }).ToList());

                            db.SaveChanges();

                            db.ANFormCCUser.RemoveRange(db.ANFormCCUser.Where(x => x.FormUniqueID == form.UniqueID).ToList());

                            db.SaveChanges();

                            db.ANFormCCUser.AddRange(Model.NotifyCCUserList.Select(x => new ANFormCCUser
                            {
                                FormUniqueID = form.UniqueID,
                                UserID = x.ID
                            }).ToList());

                            db.SaveChanges();

                            var fileList = db.ANFormFile.Where(x => x.FormUniqueID == form.UniqueID).ToList();

                            foreach (var file in fileList)
                            {
                                if (!Model.FileList.Any(x => x.Seq == file.Seq))
                                {
                                    try
                                    {
                                        System.IO.File.Delete(Path.Combine(Config.EquipmentMaintenanceFileFolderPath, string.Format("{0}_{1}.{2}", form.UniqueID, file.Seq, file.Extension)));
                                    }
                                    catch { }
                                }
                            }

                            db.ANFormFile.RemoveRange(fileList);

                            db.SaveChanges();

                            foreach (var file in Model.FileList)
                            {
                                db.ANFormFile.Add(new ANFormFile()
                                {
                                    FormUniqueID = form.UniqueID,
                                    Seq = file.Seq,
                                    Extension = file.Extension,
                                    FileName = file.FileName,
                                    LastModifyTime = file.LastModifyTime,
                                    Length = file.Size
                                });

                                if (!file.IsSaved)
                                {
                                    System.IO.File.Copy(Path.Combine(Config.TempFolder, file.TempFileName), Path.Combine(Config.EquipmentMaintenanceFileFolderPath, string.Format("{0}_{1}.{2}", form.UniqueID, file.Seq, file.Extension)), true);
                                    System.IO.File.Delete(Path.Combine(Config.TempFolder, file.TempFileName));
                                }
                            }

                            db.SaveChanges();

                            if (Config.HaveMailSetting)
                            {
                                SendNotifyMail(form.UniqueID, true);
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

                using (ADbEntities db = new ADbEntities())
                {
                    var form = db.ANForm.First(x => x.UniqueID == UniqueID);

                    var notifyUserList = (from x in db.ANFormUser
                                          where x.FormUniqueID == form.UniqueID
                                          select x.UserID).ToList();

                    var notifyCCUserList = (from x in db.ANFormCCUser
                                            where x.FormUniqueID == form.UniqueID
                                            select x.UserID).ToList();

                    foreach (var notifyUser in notifyUserList)
                    {
                        var user = UserDataAccessor.GetUser(notifyUser);

                        if (!string.IsNullOrEmpty(user.Email))
                        {
                            mailAddressList.Add(new MailAddress(user.Email, user.Name));
                        }
                    }

                    foreach (var notifyCCUser in notifyCCUserList)
                    {
                        var user = UserDataAccessor.GetUser(notifyCCUser);

                        if (!string.IsNullOrEmpty(user.Email))
                        {
                            ccList.Add(new MailAddress(user.Email, user.Name));
                        }
                    }

                    var groupList = db.ANFormGroup.Where(x => x.FormUniqueID == form.UniqueID).Select(x => x.GroupUniqueID).ToList();

                    foreach (var groupUniqueID in groupList)
                    {
                        var groupUserList = (from x in db.ANGroupUser
                                             where x.GroupUniqueID == groupUniqueID
                                             select x.UserID).ToList();

                        foreach (var groupUser in groupUserList)
                        {
                            var user = UserDataAccessor.GetUser(groupUser);

                            if (!string.IsNullOrEmpty(user.Email))
                            {
                                mailAddressList.Add(new MailAddress(user.Email, user.Name));
                            }
                        }

                        var groupCCUserList = (from x in db.ANGroupCCUser
                                               where x.GroupUniqueID == groupUniqueID
                                               select x.UserID).ToList();

                        foreach (var groupCCUser in groupCCUserList)
                        {
                            var user = UserDataAccessor.GetUser(groupCCUser);

                            if (!string.IsNullOrEmpty(user.Email))
                            {
                                ccList.Add(new MailAddress(user.Email, user.Name));
                            }
                        }
                    }

                    if (mailAddressList.Count > 0)
                    {
                        var subject = string.Empty;

                        if (!IsResend)
                        {
                            subject = string.Format("[異常通報][{0}]{1}", form.VHNO, form.Subject);
                        }
                        else
                        {
                            subject = string.Format("【更新】[異常通報][{0}]{1}", form.VHNO, form.Subject);
                        }

                        var th = "<th style=\"width:150px;border:1px solid #333;padding:8px;text-align:right;color:#707070;background: #F4F4F4;\">{0}</th>";
                        var td = "<td style=\"width:400px;border:1px solid #333;padding:8px;color:#707070;\">{0}</td>";

                        var sb = new StringBuilder();

                        sb.Append("<table style=\"1px solid #ddd;font-size:13px;border-collapse:collapse;\">");

                        var occurTime = DateTimeHelper.DateTimeString2DateTime(form.OccurDate, form.OccurTime);
                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "發生時間"));
                        sb.Append(string.Format(td, DateTimeHelper.DateTime2DateTimeStringWithSeperator(occurTime)));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "聯絡人員"));
                        sb.Append(string.Format(td, form.Contact));
                        sb.Append("</tr>");

                        if (!string.IsNullOrEmpty(form.Contact))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "MVPN"));
                            sb.Append(string.Format(td, form.MVPN));
                            sb.Append("</tr>");
                        }

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "異常主旨"));
                        sb.Append(string.Format(td, form.Subject));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "地點"));
                        sb.Append(string.Format(td, form.Location));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "異常原因"));
                        sb.Append(string.Format(td, form.Description));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "緊急對策"));
                        sb.Append(string.Format(td, form.HandlingDescription));
                        sb.Append("</tr>");

                        if (!string.IsNullOrEmpty(form.RecoveryDate))
                        {
                            var recoveryTime = DateTimeHelper.DateTimeString2DateTime(form.RecoveryDate, form.RecoveryTime);

                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "復原時間"));
                            sb.Append(string.Format(td, DateTimeHelper.DateTime2DateTimeStringWithSeperator(recoveryTime)));
                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(form.RecoveryDescription))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "復原說明"));
                            sb.Append(string.Format(td, form.RecoveryDescription));
                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(form.EffectArea))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "影響區域(產線單位)"));
                            sb.Append(string.Format(td, form.EffectArea));
                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(form.EffectSystem))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "影響系統(FAC系統)"));
                            sb.Append(string.Format(td, form.EffectSystem));
                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(form.Cost))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "損失金額"));
                            sb.Append(string.Format(td, form.Cost));
                            sb.Append("</tr>");
                        }

                        sb.Append("</table>");

                        var attachList = new List<Attachment>();

                        var fileList = db.ANFormFile.Where(x => x.FormUniqueID == form.UniqueID);

                        foreach (var file in fileList)
                        {
                            var fileName = string.Format("{0}_{1}.{2}", file.FormUniqueID, file.Seq, file.Extension);

                            var fullFileName = Path.Combine(Config.EquipmentMaintenanceFileFolderPath, fileName);

                            Attachment attach = new Attachment(fullFileName, MediaTypeNames.Application.Octet);

                            ContentDisposition disposition = attach.ContentDisposition;
                            disposition.CreationDate = System.IO.File.GetCreationTime(fullFileName);
                            disposition.ModificationDate = System.IO.File.GetLastWriteTime(fullFileName);
                            disposition.ReadDate = System.IO.File.GetLastAccessTime(fullFileName);
                            disposition.FileName = string.Format("{0}.{1}", file.FileName, file.Extension);
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
                using (ADbEntities db = new ADbEntities())
                {
                    var file = db.ANFormFile.First(x => x.FormUniqueID == FormUniqueID && x.Seq == Seq);

                    model = new FileDownloadModel()
                    {
                        FormUniqueID = file.FormUniqueID,
                        Seq = file.Seq,
                        FileName = file.FileName,
                        Extension = file.Extension
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

        public static RequestResult AddUser(List<UserModel> UserList, List<string> SelectedList)
        {
            RequestResult result = new RequestResult();

            try
            {
                string[] seperator = new string[] { Define.Seperator };

                using (DbEntities db = new DbEntities())
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
                                UserList.Add(UserDataAccessor.GetUser(userID));
                            }
                        }
                        else
                        {
                            var organizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organizationUniqueID, true);

                            var userList = (from u in db.User
                                            where organizationList.Contains(u.OrganizationUniqueID)
                                            select u.ID).ToList();

                            foreach (var user in userList)
                            {
                                if (!UserList.Any(x => x.ID == user))
                                {
                                    UserList.Add(UserDataAccessor.GetUser(user));
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
