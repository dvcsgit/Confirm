using System;
using System.Text;
using System.Linq;
using System.Web.Mvc;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.Authenticated;
using Models.EquipmentMaintenance.RepairFormManagement;
using System.Transactions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;
using System.IO;
using Models.Shared;
using System.Net.Mail;

namespace DataAccess.EquipmentMaintenance
{
    public class RepairFormDataAccessor
    {
        public static RequestResult GetUserOptions(List<Models.Shared.UserModel> AccountList, string Term, bool IsInit)
        {
            RequestResult result = new RequestResult();

            try
            {
                var query = AccountList.Select(x => new
                {
                    ID = x.ID,
                    Name = x.Name,
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

                        query = query.Where(x => x.ID.Contains(term) || x.Name.Contains(term)).ToList();
                    }
                }

                result.ReturnData(query.Select(x => new SelectListItem { Value = x.ID, Text = string.Format("{0}/{1}/{2}", x.OrganizationDescription, x.ID, x.Name) }).Distinct().ToList());
            }
            catch (Exception ex)
            {
                Error err = new Error(MethodInfo.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetQueryFormModel(string RepairFormUniqueID, string CheckResultUniqueID, string Status, string VHNO, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var model = new QueryFormModel()
                    {
                        InitParameters = new InitParameters()
                        {
                            RepairFormUniqueID = RepairFormUniqueID,
                            CheckResultUniqueID = CheckResultUniqueID
                        },
                        RepairFormTypeSelectItemList = new List<SelectListItem>() 
                        { 
                            Define.DefaultSelectListItem(Resources.Resource.SelectAll)
                        },
                        Parameters = new QueryParameters()
                        {
                            VHNO = VHNO
                        }
                    };

                    if (!string.IsNullOrEmpty(Status))
                    {
                        foreach (var item in model.StatusSelectItemList)
                        {
                            if (item.Value == Status)
                            {
                                item.Selected = true;
                            }
                            else
                            {
                                item.Selected = false;
                            }
                        }
                    }

                    var ancestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(Account.OrganizationUniqueID);

                    var formTypeList = db.RFormType.Where(x => x.AncestorOrganizationUniqueID == ancestorOrganizationUniqueID).OrderBy(x => x.Description).ToList();

                    foreach (var formType in formTypeList)
                    {
                        model.RepairFormTypeSelectItemList.Add(new SelectListItem()
                        {
                            Value = formType.UniqueID,
                            Text = formType.Description
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

        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                var organizationList = Account.QueryableOrganizationUniqueIDList.Intersect(downStreamOrganizationList);

                using (EDbEntities db = new EDbEntities())
                {
                    var query = (from f in db.RForm
                                 join t in db.RFormType
                                 on f.RFormTypeUniqueID equals t.UniqueID
                                 where organizationList.Contains(f.OrganizationUniqueID) || organizationList.Contains(f.MaintenanceOrganizationUniqueID)
                                 select new
                                 {
                                     f.OrganizationUniqueID,
                                     f.MaintenanceOrganizationUniqueID,
                                     f.UniqueID,
                                     f.VHNO,
                                     f.Status,
                                     f.RFormTypeUniqueID,
                                     RepairFormType = t.Description,
                                     EstBeginDate = f.EstBeginDate,
                                     EstEndDate = f.EstEndDate,
                                     Subject = f.Subject,
                                     f.EquipmentID,
                                     f.EquipmentName,
                                     f.PartDescription,
                                     f.TakeJobUserID,
                                     f.CreateTime
                                 }).Distinct().AsQueryable();

                    if (Parameters.StatusList.Count > 0)
                    {
                        if (Parameters.StatusList.Contains("5"))
                        {
                            Parameters.StatusList.Add("4");
                        }

                        query = query.Where(x => Parameters.StatusList.Contains(x.Status));
                    }
                   
                    if (!string.IsNullOrEmpty(Parameters.VHNO))
                    {
                        query = query.Where(x => x.VHNO.Contains(Parameters.VHNO));
                    }

                    if (!string.IsNullOrEmpty(Parameters.RepairFormTypeUniqueID))
                    {
                        query = query.Where(x => x.RFormTypeUniqueID == Parameters.RepairFormTypeUniqueID);
                    }

                    if (Parameters.EstBeginDate.HasValue)
                    {
                        query = query.Where(x => x.EstBeginDate.HasValue);

                        query = query.Where(x => x.EstBeginDate.Value >= Parameters.EstBeginDate.Value);
                    }

                    if (Parameters.EstEndDate.HasValue)
                    {
                        query = query.Where(x => x.EstEndDate.HasValue);

                        query = query.Where(x => x.EstEndDate.Value <= Parameters.EstEndDate.Value);
                    }

                    if (Parameters.CreateTimeBeginDate.HasValue)
                    {
                        query = query.Where(x => x.CreateTime >= Parameters.CreateTimeBeginDate.Value);
                    }

                    if (Parameters.CreateTimeEndDate.HasValue)
                    {
                        query = query.Where(x => x.CreateTime <= Parameters.CreateTimeEndDate.Value);
                    }

                    var organization = OrganizationDataAccessor.GetOrganization(Parameters.OrganizationUniqueID);

                    var model = new GridViewModel()
                    {
                        Permission = Account.OrganizationPermission(Parameters.OrganizationUniqueID),
                        OrganizationUniqueID = Parameters.OrganizationUniqueID,
                        OrganizationDescription = organization != null ? organization.Description : "*",
                        FullOrganizationDescription = organization != null ? organization.FullDescription : "*",
                        ItemList = query.ToList().Select(x => new GridItem
                        {
                            UniqueID = x.UniqueID,
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.OrganizationUniqueID),
                            MaintenanceOrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.MaintenanceOrganizationUniqueID),
                            VHNO = x.VHNO,
                            Subject = x.Subject,
                            EstBeginDate = x.EstBeginDate,
                            EstEndDate = x.EstEndDate,
                            EquipmentID = x.EquipmentID,
                            EquipmentName = x.EquipmentName,
                            PartDescription = x.PartDescription,
                            RepairFormType = x.RepairFormType,
                            Status = x.Status,
                            TakeJobUserID = x.TakeJobUserID
                        }).ToList()
                    };

                    if (Parameters.StatusList.Count>0)
                    {
                        model.ItemList = model.ItemList.Where(x => Parameters.StatusList.Contains(x.StatusCode)).ToList();
                    }

                    model.ItemList = model.ItemList.OrderByDescending(x => x.VHNO).ThenBy(x => x.OrganizationDescription).ThenBy(x => x.MaintenanceOrganizationDescription).ToList();

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

        public static RequestResult GetDetailViewModel(string UniqueID)
        {
            return GetDetailViewModel(UniqueID, null);
        }

        public static RequestResult GetDetailViewModel(string UniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities edb = new EDbEntities())
                {
                    using (DbEntities db = new DbEntities()) {
                        var repairForm = (from f in edb.RForm
                                          join e in edb.Equipment
                                          on f.EquipmentUniqueID equals e.UniqueID into tmpEquipment
                                          from e in tmpEquipment.DefaultIfEmpty()
                                          join p in edb.EquipmentPart
                                          on f.PartUniqueID equals p.UniqueID into tmpPart
                                          from p in tmpPart.DefaultIfEmpty()
                                          join t in edb.RFormType
                                          on f.RFormTypeUniqueID equals t.UniqueID
                                          where f.UniqueID == UniqueID
                                          select new
                                          {
                                              Form = f,
                                              FormType = t.Description,
                                              EquipmentID = e != null ? e.ID : "",
                                              EquipmentName = e != null ? e.Name : "",
                                              PartDescription = p != null ? p.Description : ""
                                          }).First();

                        var organization = OrganizationDataAccessor.GetOrganization(repairForm.Form.OrganizationUniqueID);

                        var pmOrganization = OrganizationDataAccessor.GetOrganization(repairForm.Form.MaintenanceOrganizationUniqueID);

                        var model = new DetailViewModel()
                        {
                            UniqueID = repairForm.Form.UniqueID,
                            AncestorOrganizationUniqueID = organization.AncestorOrganizationUniqueID,
                            OrganizationDescription = organization.Description,
                            ParentOrganizationFullDescription = organization.FullDescription,
                            MaintenanceOrganizationFullDescription = pmOrganization.FullDescription,
                            MaintenanceOrganizationDescription = pmOrganization.Description,
                            VHNO = repairForm.Form.VHNO,
                            Subject = repairForm.Form.Subject,
                            Status = repairForm.Form.Status,
                            Description = repairForm.Form.Description,
                            EstBeginDate = repairForm.Form.EstBeginDate,
                            EstEndDate = repairForm.Form.EstEndDate,
                            CreateUser = UserDataAccessor.GetUser(repairForm.Form.CreateUserID),
                            RealTakeJobTime=repairForm.Form.RealTakeJobTime,
                            RealTakeJobDateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Now),
                              ClosedTime=repairForm.Form.ClosedTime,
                               RealTakeJobUser=UserDataAccessor.GetUser(repairForm.Form.RealTakeJobUserID),
                            RealTakeJobUserID = Account!=null?Account.ID:"",
                                 RealTakeJobHour=DateTime.Now.Hour.ToString().PadLeft(2,'0'),
                            RealTakeJobMin = DateTime.Now.Minute.ToString().PadLeft(2, '0'),
                            CreateTime = repairForm.Form.CreateTime,
                            //JobManager = UserDataAccessor.GetUser(pmOrganization.ManagerID),
                            RefuseReason = repairForm.Form.ManagerRefuseReason,
                            JobTime = repairForm.Form.ManagerJobTime,
                            TakeJobUser = UserDataAccessor.GetUser(repairForm.Form.TakeJobUserID),
                            JobRefuseReason = repairForm.Form.JobRefuseReason,
                            TakeJobTime = repairForm.Form.TakeJobTime,
                            RepairFormType = repairForm.FormType,
                            EquipmentID = repairForm.EquipmentID,
                            EquipmentName = repairForm.EquipmentName,
                            PartDescription = repairForm.PartDescription,
                            MaterialList = (from x in edb.RFormMaterial
                                            join m in edb.Material
                                            on x.MaterialUniqueID equals m.UniqueID
                                            where x.RFormUniqueID == UniqueID
                                            select new MaterialModel
                                            {
                                                Seq = x.Seq,
                                                UniqueID = x.MaterialUniqueID,
                                                ID = m.ID,
                                                Name = m.Name,
                                                Quantity = x.Quantity
                                            }).OrderBy(x => x.Seq).ToList(),
                            FileList = edb.RFormFile.Where(f => f.RFormUniqueID == repairForm.Form.UniqueID).ToList().Select(f => new FileModel
                            {
                                Seq = f.Seq,
                                FileName = f.FileName,
                                Extension = f.Extension,
                                Size = f.ContentLength,
                                UploadTime = f.UploadTime,
                                IsSaved = true
                            }).OrderBy(f => f.UploadTime).ToList()
                        };

                        var organizationManagers = (from x in db.OrganizationManager
                                                    join u in db.User
                                                    on x.UserID equals u.ID
                                                    where x.OrganizationUniqueID == pmOrganization.UniqueID
                                                    select u).ToList();

                        foreach (var user in organizationManagers)
                        {
                            model.JobManagerList.Add(UserDataAccessor.GetUser(user.ID));
                        }

                        var workingHourList = edb.RFormWorkingHour.Where(x => x.RFormUniqueID == UniqueID).OrderBy(x => x.Seq).ToList();

                        foreach (var workingHour in workingHourList)
                        {
                            model.WorkingHourList.Add(new WorkingHourModel()
                            {
                                Seq = workingHour.Seq,
                                User = UserDataAccessor.GetUser(workingHour.UserID),
                                BeginDate = workingHour.BeginDate,
                                EndDate = workingHour.EndDate,
                                WorkingHour = double.Parse(workingHour.WorkingHour.ToString())
                            });
                        }

                        var columnList = (from x in edb.RFormTypeColumn
                                          join c in edb.RFormColumn
                                          on x.ColumnUniqueID equals c.UniqueID
                                          where x.RFormTypeUniqueID == repairForm.Form.RFormTypeUniqueID
                                          orderby x.Seq
                                          select c).ToList();

                        foreach (var column in columnList)
                        {
                            var value = edb.RFormColumnValue.FirstOrDefault(x => x.RFormUniqueID == repairForm.Form.UniqueID && x.ColumnUniqueID == column.UniqueID);

                            model.ColumnList.Add(new ColumnModel()
                            {
                                UniqueID = column.UniqueID,
                                Description = column.Description,
                                OptionUniqueID = value != null ? value.ColumnOptionUniqueID : "",
                                Value = value != null ? value.Value : "",
                                OptionList = edb.RFormColumnOption.Where(x => x.ColumnUniqueID == column.UniqueID).Select(x => new Models.EquipmentMaintenance.RepairFormManagement.RFormColumnOption
                                {
                                    ColumnUniqueID = x.ColumnUniqueID,
                                    UniqueID = x.UniqueID,
                                    Description = x.Description,
                                    Seq = x.Seq
                                }).OrderBy(x => x.Description).ToList()
                            });
                        }

                        var rformUserList = edb.RFormJobUser.Where(x => x.RFormUniqueID == repairForm.Form.UniqueID).Select(x => x.UserID).ToList();

                        foreach (var rformUser in rformUserList)
                        {
                            var user = UserDataAccessor.GetUser(rformUser);

                            model.JobUserList.Add(user);

                            model.RealTakeJobUserSelectItemList.Add(new SelectListItem()
                            {
                                Text = string.Format("{0}/{1}", user.ID, user.Name),
                                Value = user.ID
                            });
                        }

                        var flow = edb.RFormFlow.FirstOrDefault(x => x.RFormUniqueID == model.UniqueID);

                        if (flow != null)
                        {
                            model.IsClosed = flow.IsClosed;

                            var flowLog = edb.RFormFlowLog.FirstOrDefault(x => x.RFormUniqueID == model.UniqueID && x.Seq == flow.CurrentSeq);

                            if (flowLog != null)
                            {
                                var oManagers = (from x in db.OrganizationManager
                                                            join u in db.User
                                                            on x.UserID equals u.ID
                                                            where x.OrganizationUniqueID == flowLog.OrganizationUniqueID
                                                            select u).ToList();

                                foreach (var manager in oManagers)
                                {
                                    model.CurrentVerifyUserList.Add(UserDataAccessor.GetUser(manager.ID));
                                }
                            }
                        }
                        else
                        {
                            model.IsClosed = false;
                        }

                        var logs = edb.RFormFlowLog.Where(x => x.RFormUniqueID == model.UniqueID).OrderBy(x => x.Seq).ToList();

                        foreach (var log in logs)
                        {
                            model.FlowLogList.Add(new FlowLogModel()
                            {
                                Seq = log.Seq,
                                IsReject = log.IsReject,
                                User = UserDataAccessor.GetUser(log.UserID),
                                NotifyTime = log.NotifyTime,
                                VerifyTime = log.VerifyTime,
                                Remark = log.Remark
                            });
                        }

                        result.ReturnData(model);
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

        public static RequestResult GetCreateFormModel(string OrganizationUniqueID, string CheckResultUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new CreateFormModel()
                {
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

                    var repaiRFormTypeList = db.RFormType.Where(x => x.AncestorOrganizationUniqueID == ancestorOrganizationUniqueID).OrderBy(x => x.Description).ToList();

                    foreach (var repaiRFormType in repaiRFormTypeList)
                    {
                        model.RepairFormTypeSelectItemList.Add(new SelectListItem()
                        {
                            Value = repaiRFormType.UniqueID,
                            Text = repaiRFormType.Description
                        });

                        model.RepairFormTypeSubjectList.Add(repaiRFormType.UniqueID, (from x in db.RFormTypeSubject
                                                                                      join s in db.RFormSubject
                                                                                      on x.SubjectUniqueID equals s.UniqueID
                                                                                      where x.RFormTypeUniqueID == repaiRFormType.UniqueID
                                                                                      orderby x.Seq
                                                                                      select new Models.EquipmentMaintenance.RepairFormManagement.RFormSubject
                                                                                      {
                                                                                          UniqueID = s.UniqueID,
                                                                                          ID = s.ID,
                                                                                          Description = s.Description,
                                                                                          AncestorOrganizationUniqueID = s.AncestorOrganizationUniqueID
                                                                                      }).ToList());
                    }

                    if (string.IsNullOrEmpty(CheckResultUniqueID))
                    {
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
                    else
                    {
                        var checkResult = db.CheckResult.First(x => x.UniqueID == CheckResultUniqueID);

                        model.OrganizationUniqueID = checkResult.OrganizationUniqueID;
                        model.AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(checkResult.OrganizationUniqueID);
                        model.FullOrganizationDescription = OrganizationDataAccessor.GetOrganizationFullDescription(checkResult.OrganizationUniqueID);

                        model.EquipmentSelectItemList = new List<SelectListItem>() 
                        { 
                            Define.DefaultSelectListItem(Resources.Resource.None)
                        };

                        var equipmentList = db.Equipment.Where(x => x.OrganizationUniqueID == checkResult.OrganizationUniqueID).OrderBy(x => x.ID).ToList();

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

                        if (!string.IsNullOrEmpty(checkResult.EquipmentUniqueID))
                        {
                            var equipment = db.Equipment.FirstOrDefault(x => x.UniqueID == checkResult.EquipmentUniqueID);

                            if (equipment != null && !string.IsNullOrEmpty(equipment.MaintenanceOrganizationUniqueID))
                            {
                                var maintenanceOrganization = OrganizationDataAccessor.GetOrganization(equipment.MaintenanceOrganizationUniqueID);

                                model.MaintenanceOrganization = string.Format("{0}/{1}", maintenanceOrganization.ID, maintenanceOrganization.Description);

                                model.FormInput.MaintenanceOrganizationUniqueID = equipment.MaintenanceOrganizationUniqueID;
                            }
                        }

                        model.CheckResultUniqueID = checkResult.UniqueID;
                        model.EquipmentID = checkResult.EquipmentID;
                        model.EquipmentName = checkResult.EquipmentName;
                        model.PartDescription = !string.IsNullOrEmpty(checkResult.PartUniqueID) ? checkResult.PartDescription : "";

                        string reason = string.Empty;

                        var abnormalReasonList = db.CheckResultAbnormalReason.Where(x => x.CheckResultUniqueID == checkResult.UniqueID).OrderBy(x => x.AbnormalReasonID).ToList();

                        if (abnormalReasonList.Count > 0)
                        {
                            var sb = new StringBuilder();

                            foreach (var abnormalReason in abnormalReasonList)
                            {
                                if (!string.IsNullOrEmpty(abnormalReason.AbnormalReasonDescription))
                                {
                                    sb.Append(abnormalReason.AbnormalReasonDescription);
                                }
                                else
                                {
                                    sb.Append(abnormalReason.AbnormalReasonRemark);
                                }

                                sb.Append("、");
                            }

                            sb.Remove(sb.Length - 1, 1);

                            reason = sb.ToString();
                        }

                        model.FormInput.EquipmentUniqueID = string.Format("{0}{1}{2}", checkResult.EquipmentUniqueID, Define.Seperator, checkResult.PartUniqueID);
                        model.FormInput.Subject = checkResult.CheckItemDescription;
                        model.FormInput.Description = reason;
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

        public static RequestResult Create(CreateFormModel Model, string UserID)
        {
            RequestResult result = new RequestResult();

            try
            {
                //string jobManagerID = string.Empty;
                string maintenanceOrganizationUniqueID = string.Empty;

                using (DbEntities db = new DbEntities())
                {
                    if (Model.FormInput.IsRepairBySelf)
                    {
                        maintenanceOrganizationUniqueID = db.User.First(x => x.ID == UserID).OrganizationUniqueID;
                        //jobManagerID = UserID;
                    }
                    else
                    {
                        maintenanceOrganizationUniqueID = Model.FormInput.MaintenanceOrganizationUniqueID;

                        var organizationManagers = (from x in db.OrganizationManager
                                                    join u in db.User
                                                    on x.UserID equals u.ID
                                                    where x.OrganizationUniqueID == Model.FormInput.MaintenanceOrganizationUniqueID
                                                    select u).ToList();

                        var organization = db.Organization.First(x => x.UniqueID == Model.FormInput.MaintenanceOrganizationUniqueID);

                        if (organizationManagers.Count>0)
                            //if (!string.IsNullOrEmpty(organization.ManagerUserID))
                        {
                            //jobManagerID = organization.ManagerUserID;
                        }
                        else
                        {
                            //jobManagerID = string.Empty;
                            maintenanceOrganizationUniqueID = string.Empty;

                            result.ReturnFailedMessage(string.Format("{0} {1} {2} {3}", Resources.Resource.Organization, organization.Description, Resources.Resource.NotSet, Resources.Resource.Manager));
                        }
                    }
                }

                using (EDbEntities db = new EDbEntities())
                {


                    if (!string.IsNullOrEmpty(maintenanceOrganizationUniqueID))
                        //if (!string.IsNullOrEmpty(jobManagerID))
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

                        if (Model.FormInput.IsRepairBySelf)
                        {
                            form.Status = "4";
                            form.ManagerJobTime = createTime;
                            form.JobRefuseReason = string.Empty;
                            form.TakeJobTime = createTime;
                            form.TakeJobUserID = UserID;
                            form.EstBeginDate = Model.FormInput.EstBeginDate;
                            form.EstEndDate = Model.FormInput.EstEndDate;

                            db.RFormJobUser.Add(new RFormJobUser()
                            {
                                RFormUniqueID = uniqueID,
                                UserID = UserID
                            });
                        }

                        db.RForm.Add(form);

                        foreach (var file in Model.FileList)
                        {
                            db.RFormFile.Add(new RFormFile()
                            {
                                RFormUniqueID = uniqueID,
                                Seq = file.Seq,
                                Extension = file.Extension,
                                FileName = file.FileName,
                                UploadTime = file.UploadTime,
                                ContentLength = file.Size
                            });

                            if (!file.IsSaved)
                            {
                                System.IO.File.Copy(Path.Combine(Config.TempFolder, file.TempFileName), Path.Combine(Config.EquipmentMaintenanceFileFolderPath, string.Format("{0}_{1}.{2}", uniqueID, file.Seq, file.Extension)), true);
                                System.IO.File.Delete(Path.Combine(Config.TempFolder, file.TempFileName));
                            }
                        }

                        db.SaveChanges();

                        SendRFormJobNotifyMail(uniqueID);

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

        public static RequestResult GetEditFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities edb = new EDbEntities())
                {
                    using (DbEntities db = new DbEntities())
                    {
                        var repairForm = (from f in edb.RForm
                                          join e in edb.Equipment
                                          on f.EquipmentUniqueID equals e.UniqueID into tmpEquipment
                                          from e in tmpEquipment.DefaultIfEmpty()
                                          join p in edb.EquipmentPart
                                          on f.PartUniqueID equals p.UniqueID into tmpPart
                                          from p in tmpPart.DefaultIfEmpty()
                                          join t in edb.RFormType
                                          on f.RFormTypeUniqueID equals t.UniqueID
                                          where f.UniqueID == UniqueID
                                          select new
                                          {
                                              Form = f,
                                              FormType = t.Description,
                                              EquipmentID = e != null ? e.ID : "",
                                              EquipmentName = e != null ? e.Name : "",
                                              PartDescription = p != null ? p.Description : ""
                                          }).First();

                        var maintenanceOrganization = OrganizationDataAccessor.GetOrganization(repairForm.Form.MaintenanceOrganizationUniqueID);

                        var model = new EditFormModel()
                        {
                            UniqueID = repairForm.Form.UniqueID,
                            AncestorOrganizationUniqueID = maintenanceOrganization.AncestorOrganizationUniqueID,
                            OrganizationUniqueID = repairForm.Form.OrganizationUniqueID,
                            ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(repairForm.Form.OrganizationUniqueID),
                            MaintenanceOrganizationFullDescription = maintenanceOrganization.FullDescription,
                            VHNO = repairForm.Form.VHNO,
                            RefuseReason = repairForm.Form.ManagerRefuseReason,
                            JobRefuseReason = repairForm.Form.JobRefuseReason,
                            RepairFormType = repairForm.FormType,
                            EquipmentID = repairForm.EquipmentID,
                            EquipmentName = repairForm.EquipmentName,
                            PartDescription = repairForm.PartDescription,
                            EquipmentUniqueID = repairForm.Form.EquipmentUniqueID,
                            PartUniqueID = repairForm.Form.PartUniqueID,
                            Status = repairForm.Form.Status,
                            Subject = repairForm.Form.Subject,
                            Description = repairForm.Form.Description,
                            EstBeginDate = repairForm.Form.EstBeginDate,
                            EstEndDate = repairForm.Form.EstEndDate,
                            CreateUser = UserDataAccessor.GetUser(repairForm.Form.CreateUserID),
                            CreateTime = repairForm.Form.CreateTime,
                            //JobManager = UserDataAccessor.GetUser(maintenanceOrganization.ManagerID),
                            JobTime = repairForm.Form.ManagerJobTime,
                            TakeJobUser = UserDataAccessor.GetUser(repairForm.Form.TakeJobUserID),
                            TakeJobTime = repairForm.Form.TakeJobTime,
                            MaterialList = (from x in edb.RFormMaterial
                                            join m in edb.Material
                                            on x.MaterialUniqueID equals m.UniqueID
                                            where x.RFormUniqueID == UniqueID
                                            select new MaterialModel
                                            {
                                                Seq = x.Seq,
                                                UniqueID = x.MaterialUniqueID,
                                                ID = m.ID,
                                                Name = m.Name,
                                                Quantity = x.Quantity
                                            }).OrderBy(x => x.Seq).ToList(),
                            FileList = edb.RFormFile.Where(f => f.RFormUniqueID == repairForm.Form.UniqueID).ToList().Select(f => new FileModel
                            {
                                Seq = f.Seq,
                                FileName = f.FileName,
                                Extension = f.Extension,
                                Size = f.ContentLength,
                                UploadTime = f.UploadTime,
                                IsSaved = true
                            }).OrderBy(f => f.UploadTime).ToList()
                        };

                        var organizationManagers = (from x in db.OrganizationManager
                                                    join u in db.User
                                                    on x.UserID equals u.ID
                                                    where x.OrganizationUniqueID == maintenanceOrganization.UniqueID
                                                    select u).ToList();

                        foreach (var user in organizationManagers)
                        {
                            model.JobManagerList.Add(UserDataAccessor.GetUser(user.ID));
                        }

                        var workingHourList = edb.RFormWorkingHour.Where(x => x.RFormUniqueID == UniqueID).OrderBy(x => x.Seq).ToList();

                        foreach (var workingHour in workingHourList)
                        {
                            model.WorkingHourList.Add(new WorkingHourModel()
                            {
                                Seq = workingHour.Seq,
                                User = UserDataAccessor.GetUser(workingHour.UserID),
                                BeginDate = workingHour.BeginDate,
                                EndDate = workingHour.EndDate,
                                WorkingHour = double.Parse(workingHour.WorkingHour.ToString())
                            });
                        }

                        var columnList = (from x in edb.RFormTypeColumn
                                          join c in edb.RFormColumn
                                          on x.ColumnUniqueID equals c.UniqueID
                                          where x.RFormTypeUniqueID == repairForm.Form.RFormTypeUniqueID
                                          orderby x.Seq
                                          select c).ToList();

                        foreach (var column in columnList)
                        {
                            var value = edb.RFormColumnValue.FirstOrDefault(x => x.RFormUniqueID == repairForm.Form.UniqueID && x.ColumnUniqueID == column.UniqueID);

                            model.ColumnList.Add(new ColumnModel()
                            {
                                UniqueID = column.UniqueID,
                                Description = column.Description,
                                OptionUniqueID = value != null ? value.ColumnOptionUniqueID : "",
                                Value = value != null ? value.Value : "",
                                OptionList = edb.RFormColumnOption.Where(x => x.ColumnUniqueID == column.UniqueID).Select(x => new Models.EquipmentMaintenance.RepairFormManagement.RFormColumnOption
                                {
                                    ColumnUniqueID = x.ColumnUniqueID,
                                    UniqueID = x.UniqueID,
                                    Description = x.Description,
                                    Seq = x.Seq
                                }).OrderBy(x => x.Description).ToList()
                            });
                        }

                        var rformUserList = edb.RFormJobUser.Where(x => x.RFormUniqueID == repairForm.Form.UniqueID).Select(x => x.UserID).ToList();

                        foreach (var rformUser in rformUserList)
                        {
                            model.JobUserList.Add(UserDataAccessor.GetUser(rformUser));
                        }

                        var flow = edb.RFormFlow.FirstOrDefault(x => x.RFormUniqueID == model.UniqueID);

                        if (flow != null)
                        {
                            model.IsClosed = flow.IsClosed;
                        }
                        else
                        {
                            model.IsClosed = false;
                        }

                        result.ReturnData(model);
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

        public static RequestResult Edit(EditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    using (EDbEntities db = new EDbEntities())
                    {
                        var form = db.RForm.First(x => x.UniqueID == Model.UniqueID);

                        var closedDate = DateTimeHelper.DateStringWithSeperator2DateTime(Model.ClosedDateString);

                        if (closedDate.HasValue && !string.IsNullOrEmpty(Model.ClosedHour) && !string.IsNullOrEmpty(Model.ClosedMin))
                        {
                            var closedTime = new DateTime(closedDate.Value.Year, closedDate.Value.Month, closedDate.Value.Day, int.Parse(Model.ClosedHour), int.Parse(Model.ClosedMin), 0);

                            form.ClosedTime = closedTime;
                        }
                        
                        db.RFormColumnValue.RemoveRange(db.RFormColumnValue.Where(x => x.RFormUniqueID == Model.UniqueID).ToList());
                        db.RFormMaterial.RemoveRange(db.RFormMaterial.Where(x => x.RFormUniqueID == Model.UniqueID).ToList());
                        db.RFormWorkingHour.RemoveRange(db.RFormWorkingHour.Where(x => x.RFormUniqueID == Model.UniqueID).ToList());

                        db.SaveChanges();

                        db.RFormColumnValue.AddRange(Model.ColumnList.Select(x => new RFormColumnValue
                        {
                            RFormUniqueID = Model.UniqueID,
                            ColumnUniqueID = x.UniqueID,
                            ColumnOptionUniqueID = x.OptionUniqueID,
                            Value = x.Value
                        }).ToList());

                        db.RFormMaterial.AddRange(Model.MaterialList.Select(x => new RFormMaterial
                        {
                            RFormUniqueID = Model.UniqueID,
                            Seq = x.Seq,
                            MaterialUniqueID = x.UniqueID,
                            Quantity = x.Quantity
                        }).ToList());

                        foreach (var workingHour in Model.WorkingHourList.OrderBy(x => x.Seq))
                        {
                            db.RFormWorkingHour.Add(new RFormWorkingHour()
                            {
                                RFormUniqueID = Model.UniqueID,
                                Seq = workingHour.Seq,
                                BeginDate = workingHour.BeginDate,
                                EndDate = workingHour.EndDate,
                                WorkingHour = double.Parse(workingHour.WorkingHour.ToString()),
                                UserID = workingHour.User.ID
                            });
                        }

                        db.SaveChanges();

                        var fileList = db.RFormFile.Where(x => x.RFormUniqueID == Model.UniqueID).ToList();

                        foreach (var file in fileList)
                        {
                            if (!Model.FileList.Any(x => x.Seq == file.Seq))
                            {
                                try
                                {
                                    System.IO.File.Delete(Path.Combine(Config.EquipmentMaintenanceFileFolderPath, string.Format("{0}_{1}.{2}", Model.UniqueID, file.Seq, file.Extension)));
                                }
                                catch { }
                            }
                        }

                        db.RFormFile.RemoveRange(fileList);

                        db.SaveChanges();

                        foreach (var file in Model.FileList)
                        {
                            db.RFormFile.Add(new RFormFile()
                            {
                                RFormUniqueID = Model.UniqueID,
                                Seq = file.Seq,
                                Extension = file.Extension,
                                FileName = file.FileName,
                                UploadTime = file.UploadTime,
                                ContentLength = file.Size
                            });

                            if (!file.IsSaved)
                            {
                                System.IO.File.Copy(Path.Combine(Config.TempFolder, file.TempFileName), Path.Combine(Config.EquipmentMaintenanceFileFolderPath, string.Format("{0}_{1}.{2}", Model.UniqueID, file.Seq, file.Extension)), true);
                                System.IO.File.Delete(Path.Combine(Config.TempFolder, file.TempFileName));
                            }
                        }

                        db.SaveChanges();
                    }

                    trans.Complete();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Save, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult Submit(string UniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities edb = new EDbEntities())
                {
                    var repairForm = edb.RForm.First(x => x.UniqueID == UniqueID);

                    using (DbEntities db = new DbEntities())
                    {
                        var nextVerifyOrganization = (from f in db.Flow
                                                      join x in db.FlowForm
                                                      on f.UniqueID equals x.FlowUniqueID
                                                      join v in db.FlowVerifyOrganization
                                                      on f.UniqueID equals v.FlowUniqueID
                                                      join o in db.Organization
                                                      on v.OrganizationUniqueID equals o.UniqueID
                                                      where f.OrganizationUniqueID == repairForm.MaintenanceOrganizationUniqueID && x.Form == Define.EnumForm.RepairForm.ToString() && x.RFormTypeUniqueID == repairForm.RFormTypeUniqueID
                                                      select new
                                                      {
                                                          o.UniqueID,
                                                          o.Description,
                                                          o.ManagerUserID,
                                                          v.Seq
                                                      }).OrderBy(x => x.Seq).FirstOrDefault();

                        //有設定簽核流程
                        if (nextVerifyOrganization != null)
                        {
                            var organizationManagers = (from x in db.OrganizationManager
                                                        join u in db.User
                                                        on x.UserID equals u.ID
                                                        where x.OrganizationUniqueID == nextVerifyOrganization.UniqueID
                                                        select u).ToList();

                            //if (!string.IsNullOrEmpty(nextVerifyOrganization.ManagerUserID))
                            if (organizationManagers.Count>0)
                            {
                                repairForm.Status = "6";

                                var flow = edb.RFormFlow.FirstOrDefault(x => x.RFormUniqueID == UniqueID);

                                int currentSeq = 1;

                                if (flow == null)
                                {
                                    edb.RFormFlow.Add(new RFormFlow()
                                    {
                                        RFormUniqueID = UniqueID,
                                        CurrentSeq = currentSeq,
                                        IsClosed = false
                                    });
                                }
                                else
                                {
                                    currentSeq = flow.CurrentSeq + 1;

                                    flow.CurrentSeq = currentSeq;
                                }

                                edb.RFormFlowLog.Add(new RFormFlowLog()
                                {
                                    RFormUniqueID = UniqueID,
                                    Seq = currentSeq,
                                    FlowSeq = 1,
                                    //UserID = nextVerifyOrganization.ManagerUserID,
                                    OrganizationUniqueID = nextVerifyOrganization.UniqueID,
                                    NotifyTime = DateTime.Now
                                });

                                edb.SaveChanges();

                                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Submit, Resources.Resource.Complete));

                                SendRFormApproveNotifyMail(UniqueID);
                            }
                            else
                            {
                                result.ReturnFailedMessage(string.Format("{0} {1} {2} {3}", Resources.Resource.Organization, nextVerifyOrganization.Description, Resources.Resource.NotSet, Resources.Resource.Manager));
                            }
                        }
                        else
                        {
                            var organization = db.Organization.First(x => x.UniqueID == repairForm.MaintenanceOrganizationUniqueID);

                            var organizationManagers = (from x in db.OrganizationManager
                                                        join u in db.User
                                                        on x.UserID equals u.ID
                                                        where x.OrganizationUniqueID == organization.UniqueID
                                                        select u).ToList();


                            if (organizationManagers.Count>0)
                                //if (!string.IsNullOrEmpty(organization.ManagerUserID))
                            {
                                repairForm.Status = "6";

                                var flow = edb.RFormFlow.FirstOrDefault(x => x.RFormUniqueID == UniqueID);

                                int currentSeq = 1;

                                if (flow == null)
                                {
                                    edb.RFormFlow.Add(new RFormFlow()
                                    {
                                        RFormUniqueID = UniqueID,
                                        CurrentSeq = currentSeq,
                                        IsClosed = false
                                    });
                                }
                                else
                                {
                                    currentSeq = flow.CurrentSeq + 1;

                                    flow.CurrentSeq = currentSeq;
                                }

                                edb.RFormFlowLog.Add(new RFormFlowLog()
                                {
                                    RFormUniqueID = UniqueID,
                                    Seq = currentSeq,
                                    FlowSeq = 0,
                                    //UserID = organization.ManagerUserID,
                                    OrganizationUniqueID = organization.UniqueID,
                                    NotifyTime = DateTime.Now
                                });

                                edb.SaveChanges();

                                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Submit, Resources.Resource.Complete));

                                SendRFormApproveNotifyMail(UniqueID);
                            }
                            else
                            {
                                result.ReturnFailedMessage(string.Format("{0} {1} {2} {3}", Resources.Resource.Organization, organization.Description, Resources.Resource.NotSet, Resources.Resource.Manager));
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

        public static RequestResult Job(DetailViewModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                if (Model.JobUserList.Count > 0)
                {
                    using (EDbEntities db = new EDbEntities())
                    {
                        var repairForm = db.RForm.First(x => x.UniqueID == Model.UniqueID);

                        repairForm.Status = "2";
                        repairForm.ManagerJobTime = DateTime.Now;
                        repairForm.JobRefuseReason = string.Empty;

                        db.RFormJobUser.RemoveRange(db.RFormJobUser.Where(x => x.RFormUniqueID == repairForm.UniqueID).ToList());

                        db.SaveChanges();

                        db.RFormJobUser.AddRange(Model.JobUserList.Select(x => new RFormJobUser
                        {
                            RFormUniqueID = repairForm.UniqueID,
                            UserID = x.ID
                        }).ToList());

                        db.SaveChanges();

                        result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.Complete));

                        SendRFormTakeJobNotifyMail(repairForm.UniqueID);
                    }
                }
                else
                {
                    result.ReturnFailedMessage(Resources.Resource.JobUserRequired);
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

        public static RequestResult Refuse(string UniqueID, string RefuseReason)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var repairForm = db.RForm.First(x => x.UniqueID == UniqueID);

                    repairForm.Status = "1";
                    repairForm.ManagerRefuseReason = RefuseReason;

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Refuse, Resources.Resource.Complete));

                SendRFormRefuseNotifyMail(UniqueID);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult TakeJob(string UniqueID, string EstBeginDateString, string EstEndDateString, string RealTakeJobDateString,string RealTakeJobHour, string RealTakeJobMin, string RealTakeJobUserID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var begin = DateTimeHelper.DateStringWithSeperator2DateTime(EstBeginDateString).Value;
                var end = DateTimeHelper.DateStringWithSeperator2DateTime(EstEndDateString).Value;

                if (DateTime.Compare(begin, end) > 0)
                {
                    result.ReturnFailedMessage(Resources.Resource.BeginDateGreaterThanEndDate);
                }
                else
                {
                    using (EDbEntities db = new EDbEntities())
                    {
                        var repairForm = db.RForm.First(x => x.UniqueID == UniqueID);

                        repairForm.Status = "4";
                        repairForm.EstBeginDate = DateTimeHelper.DateStringWithSeperator2DateTime(EstBeginDateString);
                        repairForm.EstEndDate = DateTimeHelper.DateStringWithSeperator2DateTime(EstEndDateString);
                        repairForm.TakeJobUserID = Account.ID;
                        repairForm.TakeJobTime = DateTime.Now;

                        var realTakeJobDate = DateTimeHelper.DateStringWithSeperator2DateTime(RealTakeJobDateString).Value;
                        var realTakeJobTime = new DateTime(realTakeJobDate.Year, realTakeJobDate.Month, realTakeJobDate.Day, int.Parse(RealTakeJobHour), int.Parse(RealTakeJobMin), 0);

                        repairForm.RealTakeJobUserID = RealTakeJobUserID;
                        repairForm.RealTakeJobTime = realTakeJobTime;
                        
                        db.SaveChanges();
                    }

                    result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.TakeJob, Resources.Resource.Complete));
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

        public static RequestResult RefuseJob(string UniqueID, string JobRefuseReason, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var repairForm = db.RForm.First(x => x.UniqueID == UniqueID);

                    repairForm.Status = "3";
                    repairForm.JobRefuseUserID = Account.ID;
                    repairForm.JobRefuseTime = null;
                    repairForm.JobRefuseReason = JobRefuseReason;

                    //db.RFormJobUser.RemoveRange(db.RFormJobUser.Where(x => x.RFormUniqueID == repairForm.UniqueID).ToList());

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Refuse, Resources.Resource.Complete));

                SendRFormJobRefuseNotifyMail(UniqueID);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult Approve(string UniqueID, string Remark, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities edb = new EDbEntities())
                {
                    using (DbEntities db = new DbEntities())
                    {
                        var repairForm = edb.RForm.First(x => x.UniqueID == UniqueID);
                        var flow = edb.RFormFlow.First(x => x.RFormUniqueID == UniqueID);
                        var currentFlowLog = edb.RFormFlowLog.Where(x => x.RFormUniqueID == UniqueID).OrderByDescending(x => x.Seq).First();

                        if (currentFlowLog.FlowSeq == 0)
                        {
                            repairForm.Status = "8";

                            flow.IsClosed = true;

                            currentFlowLog.UserID = Account.ID;
                            currentFlowLog.UserName = Account.Name;
                            currentFlowLog.IsReject = false;
                            currentFlowLog.VerifyTime = DateTime.Now;
                            currentFlowLog.Remark = Remark;

                            edb.SaveChanges();

#if CIIC
                                QFormDataAccessor.Closed(UniqueID);
#endif

                            result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Approve, Resources.Resource.Complete));
                        }
                        else
                        {
                            var nextVerifyOrganization = (from f in db.Flow
                                                          join x in db.FlowForm
                                                          on f.UniqueID equals x.FlowUniqueID
                                                          join v in db.FlowVerifyOrganization
                                                          on f.UniqueID equals v.FlowUniqueID
                                                          join o in db.Organization
                                                          on v.OrganizationUniqueID equals o.UniqueID
                                                          where f.OrganizationUniqueID == repairForm.MaintenanceOrganizationUniqueID && x.Form == Define.EnumForm.RepairForm.ToString() && x.RFormTypeUniqueID == repairForm.RFormTypeUniqueID
                                                          select new
                                                          {
                                                              o.UniqueID,
                                                              o.Description,
                                                              o.ManagerUserID,
                                                              v.Seq
                                                          }).Where(x => x.Seq > currentFlowLog.FlowSeq).OrderBy(x => x.Seq).FirstOrDefault();

                            if (nextVerifyOrganization != null)
                            {
                                var organizationManagers = (from x in db.OrganizationManager
                                                            join u in db.User
                                                            on x.UserID equals u.ID
                                                            where x.OrganizationUniqueID == nextVerifyOrganization.UniqueID
                                                            select u).ToList();

                                if (organizationManagers.Count>0)
                                    //if (!string.IsNullOrEmpty(nextVerifyOrganization.ManagerUserID))
                                {
                                    flow.CurrentSeq = flow.CurrentSeq + 1;

                                    currentFlowLog.UserID = Account.ID;
                                    currentFlowLog.UserName = Account.Name;
                                    currentFlowLog.IsReject = false;
                                    currentFlowLog.VerifyTime = DateTime.Now;
                                    currentFlowLog.Remark = Remark;

                                    edb.RFormFlowLog.Add(new RFormFlowLog()
                                    {
                                        RFormUniqueID = UniqueID,
                                        Seq = flow.CurrentSeq,
                                        FlowSeq = nextVerifyOrganization.Seq,
                                        OrganizationUniqueID = nextVerifyOrganization.UniqueID,
                                        //UserID = nextVerifyOrganization.ManagerUserID,
                                        NotifyTime = DateTime.Now
                                    });

                                    edb.SaveChanges();

                                    result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Approve, Resources.Resource.Complete));

                                    SendRFormApproveNotifyMail(UniqueID);
                                }
                                else
                                {
                                    result.ReturnFailedMessage(string.Format("{0} {1} {2} {3}", Resources.Resource.Organization, nextVerifyOrganization.Description, Resources.Resource.NotSet, Resources.Resource.Manager));
                                }
                            }
                            else
                            {
                                repairForm.Status = "8";

                                flow.IsClosed = true;

                                currentFlowLog.UserID = Account.ID;
                                currentFlowLog.UserName = Account.Name;
                                currentFlowLog.IsReject = false;
                                currentFlowLog.VerifyTime = DateTime.Now;
                                currentFlowLog.Remark = Remark;

                                edb.SaveChanges();

                                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Approve, Resources.Resource.Complete));

#if CIIC
                                QFormDataAccessor.Closed(UniqueID);
#endif
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

        public static RequestResult Reject(string UniqueID, string Remark, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var repairForm = db.RForm.First(x => x.UniqueID == UniqueID);

                    repairForm.Status = "7";

                    var currentFlowLog = db.RFormFlowLog.Where(x => x.RFormUniqueID == UniqueID).OrderByDescending(x => x.Seq).First();

                    currentFlowLog.UserID = Account.ID;
                    currentFlowLog.UserName = Account.Name;
                    currentFlowLog.IsReject = true;
                    currentFlowLog.VerifyTime = DateTime.Now;
                    currentFlowLog.Remark = Remark;

                    db.SaveChanges();

                    result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Reject, Resources.Resource.Complete));

                    SendRFormRejectNotifyMail(UniqueID);
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

        public static RequestResult SavePageState(List<ColumnModel> ColumnList, List<string> PageStateList)
        {
            RequestResult result = new RequestResult();

            try
            {
                foreach (string pageState in PageStateList)
                {
                    string[] temp = pageState.Split(Define.Seperators, StringSplitOptions.None);

                    string columnUniqueID = temp[0];
                    string optionUniqueID = temp[1];
                    string value = temp[2];

                    var column = ColumnList.First(x => x.UniqueID == columnUniqueID);

                    column.OptionUniqueID = optionUniqueID;
                    column.Value = value;
                }

                result.ReturnData(ColumnList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult SavePageState(List<MaterialModel> MaterialList, List<string> PageStateList)
        {
            RequestResult result = new RequestResult();

            try
            {
                foreach (string pageState in PageStateList)
                {
                    string[] temp = pageState.Split(Define.Seperators, StringSplitOptions.None);

                    int seq = int.Parse(temp[0]);
                    string qty = temp[1];

                    var material = MaterialList.First(x => x.Seq == seq);

                    if (!string.IsNullOrEmpty(qty))
                    {
                        int q;

                        if (int.TryParse(qty, out q))
                        {
                            material.Quantity = q;
                        }
                        else
                        {
                            material.Quantity = 0;
                        }
                    }
                    else
                    {
                        material.Quantity = 0;
                    }
                }

                result.ReturnData(MaterialList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult CreateWorkingHour(List<WorkingHourModel> WorkingHourList, CreateWorkingHourFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                if (string.Compare(Model.FormInput.BeginDate, Model.FormInput.EndDate) > 0)
                {
                    result.ReturnFailedMessage(Resources.Resource.BeginDateGreaterThanEndDate);
                }
                else
                {
                    int seq = 1;

                    if (WorkingHourList.Count > 0)
                    {
                        seq = WorkingHourList.Max(x => x.Seq) + 1;
                    }

                    WorkingHourList.Add(new WorkingHourModel()
                    {
                        Seq = seq,
                        BeginDate = Model.FormInput.BeginDate,
                        EndDate = Model.FormInput.EndDate,
                        WorkingHour = Model.FormInput.WorkingHour,
                        User = UserDataAccessor.GetUser(Model.FormInput.UserID)
                    });

                    WorkingHourList = WorkingHourList.OrderBy(x => x.Seq).ToList();

                    result.ReturnData(WorkingHourList);
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

        public static RequestResult AddMaterial(List<MaterialModel> MaterialList, List<string> SelectedList, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var tempList = new List<MaterialModel>();

                using (EDbEntities db = new EDbEntities())
                {
                    foreach (string selected in SelectedList)
                    {
                        string[] temp = selected.Split(Define.Seperators, StringSplitOptions.None);

                        var organizationUniqueID = temp[0];
                        var equipmentType = temp[1];
                        var materialUniqueID = temp[2];

                        if (!string.IsNullOrEmpty(materialUniqueID))
                        {
                            if (!tempList.Any(x => x.UniqueID == materialUniqueID))
                            {
                                int seq = 1;

                                if (tempList.Count > 0)
                                {
                                    seq = tempList.Max(x => x.Seq) + 1;
                                }
                                else if (MaterialList.Count > 0)
                                {
                                    seq = MaterialList.Max(x => x.Seq) + 1;
                                }

                                var material = db.Material.First(x => x.UniqueID == materialUniqueID);

                                tempList.Add(new MaterialModel()
                                {
                                    Seq = seq,
                                    UniqueID = material.UniqueID,
                                    ID = material.ID,
                                    Name = material.Name,
                                    Quantity = 0
                                });
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(equipmentType))
                            {
                                var materialList = db.Material.Where(x => x.OrganizationUniqueID == organizationUniqueID && x.EquipmentType == equipmentType).ToList();

                                foreach (var material in materialList)
                                {
                                    if (!tempList.Any(x => x.UniqueID == material.UniqueID))
                                    {
                                        int seq = 1;

                                        if (tempList.Count > 0)
                                        {
                                            seq = tempList.Max(x => x.Seq) + 1;
                                        }
                                        else if (MaterialList.Count > 0)
                                        {
                                            seq = MaterialList.Max(x => x.Seq) + 1;
                                        }

                                        tempList.Add(new MaterialModel()
                                        {
                                            Seq = seq,
                                            UniqueID = material.UniqueID,
                                            ID = material.ID,
                                            Name = material.Name,
                                            Quantity = 0
                                        });
                                    }
                                }
                            }
                            else
                            {
                                var availableOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(RefOrganizationUniqueID, true);

                                var organizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organizationUniqueID, true);

                                foreach (var organization in organizationList)
                                {
                                    if (availableOrganizationList.Any(x => x == organization))
                                    {
                                        var materialList = db.Material.Where(x => x.OrganizationUniqueID == organization).ToList();

                                        foreach (var material in materialList)
                                        {
                                            if (!tempList.Any(x => x.UniqueID == material.UniqueID))
                                            {
                                                int seq = 1;

                                                if (tempList.Count > 0)
                                                {
                                                    seq = tempList.Max(x => x.Seq) + 1;
                                                }
                                                else if (MaterialList.Count > 0)
                                                {
                                                    seq = MaterialList.Max(x => x.Seq) + 1;
                                                }

                                                tempList.Add(new MaterialModel()
                                                {
                                                    Seq = seq,
                                                    UniqueID = material.UniqueID,
                                                    ID = material.ID,
                                                    Name = material.Name,
                                                    Quantity = 0
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (tempList.Count > 0)
                {
                    MaterialList.AddRange(tempList);
                }

                result.ReturnData(MaterialList.OrderBy(x => x.ID).ToList());
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static ExcelExportModel Export(DetailViewModel Model)
        {
            try
            {
                var workBook = new XSSFWorkbook();

                #region 標楷體 12px
                var font = workBook.CreateFont();
                font.FontName = "標楷體";
                font.FontHeightInPoints = 12;
                #endregion

                #region 標楷體 16px, UnderLine
                var headerFont = workBook.CreateFont();
                headerFont.FontName = "標楷體";
                headerFont.FontHeightInPoints = 16;
                headerFont.Underline = FontUnderlineType.Single;
                #endregion

                #region Header Style
                var headerStyle = workBook.CreateCellStyle();
                headerStyle.SetFont(headerFont);
                headerStyle.Alignment = HorizontalAlignment.Center;
                headerStyle.VerticalAlignment = VerticalAlignment.Top - 1;
                headerStyle.BorderTop = BorderStyle.None;
                headerStyle.BorderBottom = BorderStyle.None;
                headerStyle.BorderLeft = BorderStyle.None;
                headerStyle.BorderRight = BorderStyle.None;
                headerStyle.WrapText = true;
                #endregion

                #region Cell Style
                var cellStyle0 = workBook.CreateCellStyle();
                cellStyle0.SetFont(font);
                cellStyle0.Alignment = HorizontalAlignment.Left;
                cellStyle0.VerticalAlignment = VerticalAlignment.Center - 1;
                cellStyle0.BorderTop = BorderStyle.None;
                cellStyle0.BorderBottom = BorderStyle.None;
                cellStyle0.BorderLeft = BorderStyle.None;
                cellStyle0.BorderRight = BorderStyle.None;
                cellStyle0.WrapText = true;

                var cellStyle = workBook.CreateCellStyle();
                cellStyle.SetFont(font);
                cellStyle.Alignment = HorizontalAlignment.Left;
                cellStyle.VerticalAlignment = VerticalAlignment.Center - 1;
                cellStyle.BorderTop = BorderStyle.Thin;
                cellStyle.BorderBottom = BorderStyle.Thin;
                cellStyle.BorderLeft = BorderStyle.Thin;
                cellStyle.BorderRight = BorderStyle.Thin;
                cellStyle.WrapText = true;
                #endregion

                var worksheet = workBook.CreateSheet(Model.VHNO);

                #region 設定各欄寬度
                worksheet.SetColumnWidth(0, 5 * 256);
                worksheet.SetColumnWidth(1, 11 * 256);
                worksheet.SetColumnWidth(2, 24 * 256);
                worksheet.SetColumnWidth(3, 5 * 256);
                worksheet.SetColumnWidth(4, 16 * 256);
                worksheet.SetColumnWidth(5, 24 * 256);
                #endregion

                #region 設定合併儲存格
                worksheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 5));
                worksheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 1));
                worksheet.AddMergedRegion(new CellRangeAddress(2, 2, 3, 4));
                worksheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 1));
                worksheet.AddMergedRegion(new CellRangeAddress(3, 3, 3, 4));
                worksheet.AddMergedRegion(new CellRangeAddress(5, 5, 0, 1));
                worksheet.AddMergedRegion(new CellRangeAddress(5, 5, 2, 5));

                worksheet.AddMergedRegion(new CellRangeAddress(6, 6, 0, 1));
                worksheet.AddMergedRegion(new CellRangeAddress(6, 6, 2, 5));

                worksheet.AddMergedRegion(new CellRangeAddress(7, 7, 0, 1));
                worksheet.AddMergedRegion(new CellRangeAddress(7, 7, 3, 4));

                worksheet.AddMergedRegion(new CellRangeAddress(8, 8, 0, 1));
                worksheet.AddMergedRegion(new CellRangeAddress(8, 8, 3, 4));

                worksheet.AddMergedRegion(new CellRangeAddress(9, 9, 0, 1));
                worksheet.AddMergedRegion(new CellRangeAddress(9, 9, 2, 5));
                worksheet.AddMergedRegion(new CellRangeAddress(10, 10, 0, 1));
                worksheet.AddMergedRegion(new CellRangeAddress(10, 10, 2, 5));
                worksheet.AddMergedRegion(new CellRangeAddress(11, 11, 0, 1));
                worksheet.AddMergedRegion(new CellRangeAddress(11, 11, 2, 5));

                
                worksheet.AddMergedRegion(new CellRangeAddress(12, 15, 0, 0));
                worksheet.AddMergedRegion(new CellRangeAddress(12, 12, 2, 5));
                worksheet.AddMergedRegion(new CellRangeAddress(13, 13, 2, 5));
                worksheet.AddMergedRegion(new CellRangeAddress(14, 14, 2, 5));
                worksheet.AddMergedRegion(new CellRangeAddress(15, 15, 1, 5));

                #endregion

                worksheet.PrintSetup.PaperSize = 9;

                IRow row;

                ICell cell;

                #region Row 0
                row = worksheet.CreateRow(0);
                row.HeightInPoints = 25;

                cell = row.CreateCell(0);
                cell.CellStyle = headerStyle;
                cell.SetCellValue(Resources.Resource.RepairForm);
                #endregion

                #region Row 2
                row = worksheet.CreateRow(2);
                row.HeightInPoints = 17;

                cell = row.CreateCell(0);
                cell.CellStyle = cellStyle0;
                cell.SetCellValue(Resources.Resource.VHNO);

                cell = row.CreateCell(2);
                cell.CellStyle = cellStyle0;
                cell.SetCellValue(Model.VHNO);

                cell = row.CreateCell(3);
                cell.CellStyle = cellStyle0;
                cell.SetCellValue(Resources.Resource.MaintenanceOrganization);

                cell = row.CreateCell(5);
                cell.CellStyle = cellStyle0;
                cell.SetCellValue(Model.MaintenanceOrganizationDescription);
                #endregion

                #region Row 3
                row = worksheet.CreateRow(3);
                row.HeightInPoints = 17;

                cell = row.CreateCell(0);
                cell.CellStyle = cellStyle0;
                cell.SetCellValue(Resources.Resource.RepairFormType);

                cell = row.CreateCell(2);
                cell.CellStyle = cellStyle0;
                cell.SetCellValue(Model.RepairFormType);

                cell = row.CreateCell(3);
                cell.CellStyle = cellStyle0;
                cell.SetCellValue("印表時間");

                cell = row.CreateCell(5);
                cell.CellStyle = cellStyle0;
                cell.SetCellValue(DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTime.Now));
                #endregion

                #region Row 5
                row = worksheet.CreateRow(5);
                row.HeightInPoints = 17;

                cell = row.CreateCell(0);
                cell.CellStyle = cellStyle;
                cell.SetCellValue("設備部門");

                cell = row.CreateCell(1);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(2);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Model.OrganizationDescription);

                cell = row.CreateCell(3);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(4);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(5);
                cell.CellStyle = cellStyle;
                #endregion

                #region Row 6
                row = worksheet.CreateRow(6);
                row.HeightInPoints = 17;

                cell = row.CreateCell(0);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.EquipmentID);

                cell = row.CreateCell(1);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(2);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Model.EquipmentID);

                cell = row.CreateCell(3);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(4);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(5);
                cell.CellStyle = cellStyle;
                #endregion

                #region Row 7
                row = worksheet.CreateRow(7);
                row.HeightInPoints = 17;

                cell = row.CreateCell(0);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.EquipmentName);

                cell = row.CreateCell(1);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(2);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Model.EquipmentName);

                cell = row.CreateCell(3);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.PartDescription);

                cell = row.CreateCell(4);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Model.PartDescription);

                cell = row.CreateCell(5);
                cell.CellStyle = cellStyle;
                #endregion

                #region Row 8
                row = worksheet.CreateRow(8);
                row.HeightInPoints = 17;

                cell = row.CreateCell(0);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.MaintenanceBeginDate);

                cell = row.CreateCell(1);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(2);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Model.EstBeginDateString);

                cell = row.CreateCell(3);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.MaintenanceEndDate);

                cell = row.CreateCell(4);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Model.EstEndDateString);

                cell = row.CreateCell(5);
                cell.CellStyle = cellStyle;
                #endregion

                #region Row 9
                row = worksheet.CreateRow(9);
                row.HeightInPoints = 17;

                cell = row.CreateCell(0);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.TakeJobTime);

                cell = row.CreateCell(1);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(2);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Model.TakeJobTimeString);

                cell = row.CreateCell(3);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(4);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(5);
                cell.CellStyle = cellStyle;
                #endregion

                #region Row 10
                row = worksheet.CreateRow(10);
                row.HeightInPoints = 17;

                cell = row.CreateCell(0);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.TakeJobUser);

                cell = row.CreateCell(1);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(2);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Model.TakeJobUser.User);

                cell = row.CreateCell(3);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(4);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(5);
                cell.CellStyle = cellStyle;
                #endregion

                #region Row 11
                row = worksheet.CreateRow(11);
                row.HeightInPoints = 17;

                cell = row.CreateCell(0);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.ClosedTime);

                cell = row.CreateCell(1);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(2);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Model.ClosedTimeString);

                cell = row.CreateCell(3);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(4);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(5);
                cell.CellStyle = cellStyle;
                #endregion

                #region Row 12
                row = worksheet.CreateRow(12);
                row.HeightInPoints = 17;

                cell = row.CreateCell(0);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.AbnormalStatus);

                cell = row.CreateCell(1);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.CreateUser);

                cell = row.CreateCell(2);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Model.CreateUser.User);

                cell = row.CreateCell(3);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(4);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(5);
                cell.CellStyle = cellStyle;
                #endregion

                #region Row 13
                row = worksheet.CreateRow(13);
                row.HeightInPoints = 17;

                cell = row.CreateCell(0);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(1);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.CreateTime);

                cell = row.CreateCell(2);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Model.CreateTimeString);

                cell = row.CreateCell(3);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(4);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(5);
                cell.CellStyle = cellStyle;
                #endregion

                #region Row 14
                row = worksheet.CreateRow(14);
                row.HeightInPoints = 17;

                cell = row.CreateCell(0);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(1);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.Subject);

                cell = row.CreateCell(2);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Model.Subject);

                cell = row.CreateCell(3);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(4);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(5);
                cell.CellStyle = cellStyle;
                #endregion

                #region Row 15
                row = worksheet.CreateRow(15);
                row.HeightInPoints = 100;

                cell = row.CreateCell(0);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(1);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(string.Format("{0}：{1}", Resources.Resource.Description, Model.Description));

                cell = row.CreateCell(2);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(3);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(4);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(5);
                cell.CellStyle = cellStyle;
                #endregion

                var currentRowIndex = 16;

                #region RepairForm Column
                foreach (var column in Model.ColumnList)
                {
                    row = worksheet.CreateRow(currentRowIndex);
                    row.HeightInPoints = 100;

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(column.Description);

                    cell = row.CreateCell(1);
                    cell.CellStyle = cellStyle;
                    if (column.OptionList.Count > 0)
                    {
                        cell.SetCellValue(column.OptionValue);
                    }
                    else
                    {
                        cell.SetCellValue(column.Value);
                    }

                    cell = row.CreateCell(2);
                    cell.CellStyle = cellStyle;

                    cell = row.CreateCell(3);
                    cell.CellStyle = cellStyle;

                    cell = row.CreateCell(4);
                    cell.CellStyle = cellStyle;

                    cell = row.CreateCell(5);
                    cell.CellStyle = cellStyle;

                    worksheet.AddMergedRegion(new CellRangeAddress(currentRowIndex, currentRowIndex, 1, 5));

                    currentRowIndex++;
                }
                #endregion

                var workingHourBeginRow = currentRowIndex;

                #region 工時紀錄
                row = worksheet.CreateRow(currentRowIndex);
                row.HeightInPoints = 17;

                cell = row.CreateCell(0);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.WorkingHourRecord);

                cell = row.CreateCell(1);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.MaintenanceUser);

                cell = row.CreateCell(2);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.BeginDate);

                cell = row.CreateCell(3);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.EndDate);

                cell = row.CreateCell(4);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(5);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.WorkingHour);

                worksheet.AddMergedRegion(new CellRangeAddress(currentRowIndex, currentRowIndex, 3, 4));

                var workingHourLastRow = currentRowIndex + 5;

                if (Model.WorkingHourList.Count > 4)
                {
                    workingHourLastRow = currentRowIndex + Model.WorkingHourList.Count + 1;
                }

                worksheet.AddMergedRegion(new CellRangeAddress(currentRowIndex, workingHourLastRow, 0, 0));

                currentRowIndex++;

                for (int i = currentRowIndex; i < workingHourLastRow; i++)
                {
                    row = worksheet.CreateRow(i);
                    row.HeightInPoints = 17;

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;

                    cell = row.CreateCell(1);
                    cell.CellStyle = cellStyle;

                    cell = row.CreateCell(2);
                    cell.CellStyle = cellStyle;

                    cell = row.CreateCell(3);
                    cell.CellStyle = cellStyle;

                    cell = row.CreateCell(4);
                    cell.CellStyle = cellStyle;

                    cell = row.CreateCell(5);
                    cell.CellStyle = cellStyle;

                    worksheet.AddMergedRegion(new CellRangeAddress(i, i, 3, 4));
                }

                row = worksheet.CreateRow(workingHourLastRow);
                row.HeightInPoints = 17;

                cell = row.CreateCell(0);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(1);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(2);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(3);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(4);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(5);
                cell.CellStyle = cellStyle;

                worksheet.AddMergedRegion(new CellRangeAddress(workingHourLastRow, workingHourLastRow, 1, 4));

                currentRowIndex = workingHourBeginRow + 1;

                foreach (var item in Model.WorkingHourList)
                {
                    row = worksheet.GetRow(currentRowIndex);
                    row.HeightInPoints = 17;

                    cell = row.GetCell(1);
                    cell.SetCellValue(item.User.User);

                    cell = row.GetCell(2);
                    //cell.SetCellValue(item.FrontWorkingHour.HasValue ? item.FrontWorkingHour.ToString() : "");

                    cell = row.GetCell(3);
                    cell.SetCellValue(item.WorkingHour);

                    cell = row.GetCell(5);
                    cell.SetCellValue(item.WorkingHour);

                    currentRowIndex++;
                }

                row = worksheet.GetRow(workingHourLastRow);

                cell = row.GetCell(1);
                cell.SetCellValue(Resources.Resource.Total);

                cell = row.GetCell(5);
                cell.SetCellValue(Model.WorkingHourList.Sum(x => x.WorkingHour));

                currentRowIndex = workingHourLastRow + 1;
                #endregion


                #region Row
                row = worksheet.CreateRow(currentRowIndex);
                row.HeightInPoints = 17;

                cell = row.CreateCell(0);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.ChangeMaterialList);

                cell = row.CreateCell(1);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.Item);

                cell = row.CreateCell(2);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.MaterialID);

                cell = row.CreateCell(3);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.MaterialName);

                cell = row.CreateCell(4);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(5);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.QTY);

                worksheet.AddMergedRegion(new CellRangeAddress(currentRowIndex, currentRowIndex, 3, 4));
                #endregion

                var materialLastRow = currentRowIndex + 4;

                if (Model.MaterialList.Count > 4)
                {
                    materialLastRow = currentRowIndex + Model.MaterialList.Count;
                }

                worksheet.AddMergedRegion(new CellRangeAddress(currentRowIndex, materialLastRow, 0, 0));

                currentRowIndex++;

                for (int i = currentRowIndex; i <= materialLastRow; i++)
                {
                    row = worksheet.CreateRow(i);
                    row.HeightInPoints = 17;

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;

                    cell = row.CreateCell(1);
                    cell.CellStyle = cellStyle;

                    cell = row.CreateCell(2);
                    cell.CellStyle = cellStyle;

                    cell = row.CreateCell(3);
                    cell.CellStyle = cellStyle;

                    cell = row.CreateCell(4);
                    cell.CellStyle = cellStyle;

                    cell = row.CreateCell(5);
                    cell.CellStyle = cellStyle;

                    worksheet.AddMergedRegion(new CellRangeAddress(i, i, 3, 4));
                }

                var index = 1;

                foreach (var item in Model.MaterialList)
                {
                    row = worksheet.GetRow(currentRowIndex);

                    cell = row.GetCell(1);
                    cell.SetCellValue(index);

                    cell = row.GetCell(2);
                    cell.SetCellValue(item.ID);

                    cell = row.GetCell(3);
                    cell.SetCellValue(item.Name);

                    cell = row.GetCell(5);
                    cell.SetCellValue(item.Quantity);

                    index++;
                    currentRowIndex++;
                }

                currentRowIndex = materialLastRow + 2;

                row = worksheet.CreateRow(currentRowIndex);

                cell = row.CreateCell(0);
                cell.CellStyle = cellStyle0;
                cell.SetCellValue("現場人員確認：");

                cell = row.CreateCell(1);
                cell.CellStyle = cellStyle0;

                cell = row.CreateCell(2);
                cell.CellStyle = cellStyle0;

                worksheet.AddMergedRegion(new CellRangeAddress(currentRowIndex, currentRowIndex, 0, 2));

                var output = new ExcelExportModel(Model.VHNO, Define.EnumExcelVersion._2007);

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

                return output;
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);

                return null;
            }
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
                                UserList.Add((from u in db.User
                                              join o in db.Organization
                                                  on u.OrganizationUniqueID equals o.UniqueID
                                              where u.ID == userID
                                              select new UserModel
                                              {
                                                  ID = u.ID,
                                                  Name = u.Name,
                                                  OrganizationDescription = o.Description
                                              }).First());
                            }
                        }
                        else
                        {
                            var organizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organizationUniqueID, true);

                            var userList = (from u in db.User
                                            join o in db.Organization
                                           on u.OrganizationUniqueID equals o.UniqueID
                                            where organizationList.Contains(u.OrganizationUniqueID)
                                            select new
                                            {
                                                ID = u.ID,
                                                Name = u.Name,
                                                OrganizationDescription = o.Description
                                            }).ToList();

                            foreach (var user in userList)
                            {
                                if (!UserList.Any(x => x.ID == user.ID))
                                {
                                    UserList.Add(new UserModel()
                                    {
                                        ID = user.ID,
                                        Name = user.Name,
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

        private static void SendRFormJobNotifyMail(string RFormUniqueID)
        {
            try
            {
                var mailAddressList = new List<MailAddress>();

                using (DbEntities db = new DbEntities())
                {
                    using (EDbEntities edb = new EDbEntities())
                    {
                        var form = edb.RForm.First(x => x.UniqueID == RFormUniqueID);

                        var organizationManagers = (from x in db.OrganizationManager
                                                    join u in db.User
                                                    on x.UserID equals u.ID
                                                    where x.OrganizationUniqueID == form.MaintenanceOrganizationUniqueID
                                                    select u).ToList();

                        foreach (var organizationManager in organizationManagers)
                        {
                            if (!string.IsNullOrEmpty(organizationManager.Email))
                            {
                                mailAddressList.Add(new MailAddress(organizationManager.Email, organizationManager.Name));
                            }
                        }
                        
                        if (mailAddressList.Count > 0)
                        {
                            var repaiRFormType = string.Empty;

                            var formType = edb.RFormType.FirstOrDefault(x => x.UniqueID == form.RFormTypeUniqueID);

                            if (formType != null)
                            {
                                repaiRFormType = formType.Description;
                            }
                            else
                            {
                                repaiRFormType = "-";
                            }

                            var equipmentID = string.Empty;
                            var equipmentName = string.Empty;
                            var partDescription = string.Empty;

                            if (!string.IsNullOrEmpty(form.EquipmentUniqueID))
                            {
                                var equipment = edb.Equipment.FirstOrDefault(x => x.UniqueID == form.EquipmentUniqueID);

                                if (equipment != null)
                                {
                                    equipmentID = equipment.ID;
                                    equipmentName = equipment.Name;

                                    if (!string.IsNullOrEmpty(form.PartUniqueID))
                                    {
                                        var part = edb.EquipmentPart.FirstOrDefault(x => x.EquipmentUniqueID == equipment.UniqueID && x.UniqueID == form.PartUniqueID);

                                        if (part != null)
                                        {
                                            partDescription = part.Description;
                                        }
                                    }
                                }
                            }

                            var subject = string.Format("[{0}][{1}][{2}]", Resources.Resource.RFormJobNotify, repaiRFormType, form.VHNO);

                            StringBuilder sb = new StringBuilder();

                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.VHNO, form.VHNO));
                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.RepairFormType, repaiRFormType));
                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Subject, form.Subject));
                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Description, form.Description));

                            if (!string.IsNullOrEmpty(equipmentID))
                            {
                                if (!string.IsNullOrEmpty(partDescription))
                                {
                                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Equipment, string.Format("{0}/{1}-{2}", equipmentID, equipmentName, partDescription)));
                                }
                                else
                                {
                                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Equipment, string.Format("{0}/{1}", equipmentID, equipmentName)));
                                }
                            }

                            var createUser = UserDataAccessor.GetUser(form.CreateUserID);
                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.CreateUser, createUser.User));

                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.CreateTime, DateTimeHelper.DateTime2DateTimeStringWithSeperator(form.CreateTime)));

                            MailHelper.SendMail(mailAddressList, subject, sb.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private static void SendRFormRefuseNotifyMail(string RFormUniqueID)
        {
            try
            {
                var mailAddressList = new List<MailAddress>();

                using (DbEntities db = new DbEntities())
                {
                    using (EDbEntities edb = new EDbEntities())
                    {
                        var form = edb.RForm.First(x => x.UniqueID == RFormUniqueID);

                        var user = db.User.FirstOrDefault(x => x.ID == form.CreateUserID);

                        if (user != null && !string.IsNullOrEmpty(user.Email))
                        {
                            mailAddressList.Add(new MailAddress(user.Email, user.Name));
                        }

                        if (mailAddressList.Count > 0)
                        {
                            var repaiRFormType = string.Empty;

                            if (form.RFormTypeUniqueID == "M")
                            {
                                repaiRFormType = Resources.Resource.MaintenanceRoute;
                            }
                            else
                            {
                                var formType = edb.RFormType.FirstOrDefault(x => x.UniqueID == form.RFormTypeUniqueID);

                                if (formType != null)
                                {
                                    repaiRFormType = formType.Description;
                                }
                                else
                                {
                                    repaiRFormType = "-";
                                }
                            }

                            var equipmentID = string.Empty;
                            var equipmentName = string.Empty;
                            var partDescription = string.Empty;

                            if (!string.IsNullOrEmpty(form.EquipmentUniqueID))
                            {
                                var equipment = edb.Equipment.FirstOrDefault(x => x.UniqueID == form.EquipmentUniqueID);

                                if (equipment != null)
                                {
                                    equipmentID = equipment.ID;
                                    equipmentName = equipment.Name;

                                    if (!string.IsNullOrEmpty(form.PartUniqueID))
                                    {
                                        var part = edb.EquipmentPart.FirstOrDefault(x => x.EquipmentUniqueID == equipment.UniqueID && x.UniqueID == form.PartUniqueID);

                                        if (part != null)
                                        {
                                            partDescription = part.Description;
                                        }
                                    }
                                }
                            }

                            var subject = string.Format("[{0}][{1}][{2}]", Resources.Resource.RFormRefuseMotify, repaiRFormType, form.VHNO);

                            StringBuilder sb = new StringBuilder();

                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.VHNO, form.VHNO));
                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.RepairFormType, repaiRFormType));
                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Subject, form.Subject));
                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Description, form.Description));

                            if (!string.IsNullOrEmpty(equipmentID))
                            {
                                if (!string.IsNullOrEmpty(partDescription))
                                {
                                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Equipment, string.Format("{0}/{1}-{2}", equipmentID, equipmentName, partDescription)));
                                }
                                else
                                {
                                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Equipment, string.Format("{0}/{1}", equipmentID, equipmentName)));
                                }
                            }

                            var jobManager = UserDataAccessor.GetUser(form.JobRefuseUserID);

                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.JobManager, jobManager.User));

                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.RefuseReason, form.JobRefuseReason));

                            MailHelper.SendMail(mailAddressList, subject, sb.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private static void SendRFormTakeJobNotifyMail(string RFormUniqueID)
        {
            try
            {
                var mailAddressList = new List<MailAddress>();

                using (DbEntities db = new DbEntities())
                {
                    using (EDbEntities edb = new EDbEntities())
                    {
                        var form = edb.RForm.First(x => x.UniqueID == RFormUniqueID);

                        var jobUserList = edb.RFormJobUser.Where(x => x.RFormUniqueID == RFormUniqueID).Select(x => x.UserID).ToList();

                        foreach (var jobUser in jobUserList)
                        {
                            var user = db.User.FirstOrDefault(x => x.ID == jobUser);

                            if (user != null && !string.IsNullOrEmpty(user.Email))
                            {
                                mailAddressList.Add(new MailAddress(user.Email, user.Name));
                            }
                        }

                        if (mailAddressList.Count > 0)
                        {
                            var repaiRFormType = string.Empty;

                            var formType = edb.RFormType.FirstOrDefault(x => x.UniqueID == form.RFormTypeUniqueID);

                            if (formType != null)
                            {
                                repaiRFormType = formType.Description;
                            }
                            else
                            {
                                repaiRFormType = "-";
                            }

                            var equipmentID = string.Empty;
                            var equipmentName = string.Empty;
                            var partDescription = string.Empty;

                            if (!string.IsNullOrEmpty(form.EquipmentUniqueID))
                            {
                                var equipment = edb.Equipment.FirstOrDefault(x => x.UniqueID == form.EquipmentUniqueID);

                                if (equipment != null)
                                {
                                    equipmentID = equipment.ID;
                                    equipmentName = equipment.Name;

                                    if (!string.IsNullOrEmpty(form.PartUniqueID))
                                    {
                                        var part = edb.EquipmentPart.FirstOrDefault(x => x.EquipmentUniqueID == equipment.UniqueID && x.UniqueID == form.PartUniqueID);

                                        if (part != null)
                                        {
                                            partDescription = part.Description;
                                        }
                                    }
                                }
                            }

                            var subject = string.Format("[{0}][{1}][{2}]", Resources.Resource.RFormTakeJobMotify, repaiRFormType, form.VHNO);

                            StringBuilder sb = new StringBuilder();

                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.VHNO, form.VHNO));
                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.RepairFormType, repaiRFormType));
                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Subject, form.Subject));
                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Description, form.Description));

                            if (!string.IsNullOrEmpty(equipmentID))
                            {
                                if (!string.IsNullOrEmpty(partDescription))
                                {
                                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Equipment, string.Format("{0}/{1}-{2}", equipmentID, equipmentName, partDescription)));
                                }
                                else
                                {
                                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Equipment, string.Format("{0}/{1}", equipmentID, equipmentName)));
                                }
                            }

                            var createUser = UserDataAccessor.GetUser(form.CreateUserID);

                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.CreateUser, createUser.User));

                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.CreateTime, DateTimeHelper.DateTime2DateTimeStringWithSeperator(form.CreateTime)));

                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.JobTime, DateTimeHelper.DateTime2DateTimeStringWithSeperator(form.ManagerJobTime)));

                            MailHelper.SendMail(mailAddressList, subject, sb.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private static void SendRFormJobRefuseNotifyMail(string RFormUniqueID)
        {
            try
            {
                var mailAddressList = new List<MailAddress>();

                using (DbEntities db = new DbEntities())
                {
                    using (EDbEntities edb = new EDbEntities())
                    {
                        var form = edb.RForm.First(x => x.UniqueID == RFormUniqueID);

                        var organizationManagers = (from x in db.OrganizationManager
                                                    join u in db.User
                                                    on x.UserID equals u.ID
                                                    where x.OrganizationUniqueID == form.MaintenanceOrganizationUniqueID
                                                    select u).ToList();

                        foreach (var organizationManager in organizationManagers)
                        {
                            if (!string.IsNullOrEmpty(organizationManager.Email))
                            {
                                mailAddressList.Add(new MailAddress(organizationManager.Email, organizationManager.Name));
                            }
                        }

                        if (mailAddressList.Count > 0)
                        {
                            var repaiRFormType = string.Empty;

                            if (form.RFormTypeUniqueID == "M")
                            {
                                repaiRFormType = Resources.Resource.MaintenanceRoute;
                            }
                            else
                            {
                                var formType = edb.RFormType.FirstOrDefault(x => x.UniqueID == form.RFormTypeUniqueID);

                                if (formType != null)
                                {
                                    repaiRFormType = formType.Description;
                                }
                                else
                                {
                                    repaiRFormType = "-";
                                }
                            }

                            var equipmentID = string.Empty;
                            var equipmentName = string.Empty;
                            var partDescription = string.Empty;

                            if (!string.IsNullOrEmpty(form.EquipmentUniqueID))
                            {
                                var equipment = edb.Equipment.FirstOrDefault(x => x.UniqueID == form.EquipmentUniqueID);

                                if (equipment != null)
                                {
                                    equipmentID = equipment.ID;
                                    equipmentName = equipment.Name;

                                    if (!string.IsNullOrEmpty(form.PartUniqueID))
                                    {
                                        var part = edb.EquipmentPart.FirstOrDefault(x => x.EquipmentUniqueID == equipment.UniqueID && x.UniqueID == form.PartUniqueID);

                                        if (part != null)
                                        {
                                            partDescription = part.Description;
                                        }
                                    }
                                }
                            }

                            var subject = string.Format("[{0}][{1}][{2}]", Resources.Resource.RFormJobRefuseNotify, repaiRFormType, form.VHNO);

                            StringBuilder sb = new StringBuilder();

                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.VHNO, form.VHNO));
                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.RepairFormType, repaiRFormType));
                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Subject, form.Subject));
                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Description, form.Description));

                            if (!string.IsNullOrEmpty(equipmentID))
                            {
                                if (!string.IsNullOrEmpty(partDescription))
                                {
                                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Equipment, string.Format("{0}/{1}-{2}", equipmentID, equipmentName, partDescription)));
                                }
                                else
                                {
                                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Equipment, string.Format("{0}/{1}", equipmentID, equipmentName)));
                                }
                            }

                            var createUser = UserDataAccessor.GetUser(form.CreateUserID);
                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.CreateUser, createUser.User));

                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.CreateTime, DateTimeHelper.DateTime2DateTimeStringWithSeperator(form.CreateTime)));
                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.JobRefuseReason, form.JobRefuseReason));

                            MailHelper.SendMail(mailAddressList, subject, sb.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private static void SendRFormApproveNotifyMail(string RFormUniqueID)
        {
            try
            {
                var mailAddressList = new List<MailAddress>();

                using (DbEntities db = new DbEntities())
                {
                    using (EDbEntities edb = new EDbEntities())
                    {
                        var form = edb.RForm.First(x => x.UniqueID == RFormUniqueID);

                        var flowLog = (from f in edb.RFormFlow
                                       join l in edb.RFormFlowLog
                                       on new { f.RFormUniqueID, Seq = f.CurrentSeq } equals new { l.RFormUniqueID, Seq = l.Seq }
                                       where f.RFormUniqueID == RFormUniqueID
                                       select l).First();

                        var organizationManagers = (from x in db.OrganizationManager
                                                    join u in db.User
                                                    on x.UserID equals u.ID
                                                    where x.OrganizationUniqueID == flowLog.OrganizationUniqueID
                                                    select u).ToList();

                        foreach (var organizationManager in organizationManagers)
                        {
                            if (!string.IsNullOrEmpty(organizationManager.Email))
                            {
                                mailAddressList.Add(new MailAddress(organizationManager.Email, organizationManager.Name));
                            }
                        }

                        if (mailAddressList.Count > 0)
                        {
                            var formType = edb.RFormType.FirstOrDefault(x => x.UniqueID == form.RFormTypeUniqueID);
                            var repairFormType = string.Empty;

                            if (formType != null)
                            {
                                repairFormType = formType.Description;
                            }
                            else
                            {
                                repairFormType = "-";
                            }

                            var equipmentID = string.Empty;
                            var equipmentName = string.Empty;
                            var partDescription = string.Empty;

                            if (!string.IsNullOrEmpty(form.EquipmentUniqueID))
                            {
                                var equipment = edb.Equipment.FirstOrDefault(x => x.UniqueID == form.EquipmentUniqueID);

                                if (equipment != null)
                                {
                                    equipmentID = equipment.ID;
                                    equipmentName = equipment.Name;

                                    if (!string.IsNullOrEmpty(form.PartUniqueID))
                                    {
                                        var part = edb.EquipmentPart.FirstOrDefault(x => x.EquipmentUniqueID == equipment.UniqueID && x.UniqueID == form.PartUniqueID);

                                        if (part != null)
                                        {
                                            partDescription = part.Description;
                                        }
                                    }
                                }
                            }

                            var subject = string.Format("[{0}][{1}][{2}]", Resources.Resource.RFormApproveNotify, repairFormType, form.VHNO);

                            StringBuilder sb = new StringBuilder();

                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.VHNO, form.VHNO));
                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.RepairFormType, repairFormType));
                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Subject, form.Subject));
                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Description, form.Description));

                            if (!string.IsNullOrEmpty(equipmentID))
                            {
                                if (!string.IsNullOrEmpty(partDescription))
                                {
                                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Equipment, string.Format("{0}/{1}-{2}", equipmentID, equipmentName, partDescription)));
                                }
                                else
                                {
                                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Equipment, string.Format("{0}/{1}", equipmentID, equipmentName)));
                                }
                            }

                            MailHelper.SendMail(mailAddressList, subject, sb.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private static void SendRFormRejectNotifyMail(string RFormUniqueID)
        {
            try
            {
                var mailAddressList = new List<MailAddress>();

                using (DbEntities db = new DbEntities())
                {
                    using (EDbEntities edb = new EDbEntities())
                    {
                        var form = edb.RForm.First(x => x.UniqueID == RFormUniqueID);

                        var currentFlowLog = edb.RFormFlowLog.Where(x => x.RFormUniqueID == RFormUniqueID).OrderByDescending(x => x.Seq).First();

                        var flowUserList = edb.RFormFlowLog.Where(x => x.RFormUniqueID == RFormUniqueID && x.Seq != currentFlowLog.Seq).Select(x => x.UserID).Distinct().ToList();

                        var user = db.User.FirstOrDefault(x => x.ID == form.TakeJobUserID);

                        if (user != null && !string.IsNullOrEmpty(user.Email))
                        {
                            mailAddressList.Add(new MailAddress(user.Email, user.Name));
                        }

                        foreach (var flowUser in flowUserList)
                        {
                            user = db.User.FirstOrDefault(x => x.ID == flowUser);

                            if (user != null && !string.IsNullOrEmpty(user.Email))
                            {
                                mailAddressList.Add(new MailAddress(user.Email, user.Name));
                            }
                        }

                        if (mailAddressList.Count > 0)
                        {
                            var repaiRFormType = string.Empty;

                            var formType = edb.RFormType.FirstOrDefault(x => x.UniqueID == form.RFormTypeUniqueID);

                            if (formType != null)
                            {
                                repaiRFormType = formType.Description;
                            }
                            else
                            {
                                repaiRFormType = "-";
                            }

                            var equipmentID = string.Empty;
                            var equipmentName = string.Empty;
                            var partDescription = string.Empty;

                            if (!string.IsNullOrEmpty(form.EquipmentUniqueID))
                            {
                                var equipment = edb.Equipment.FirstOrDefault(x => x.UniqueID == form.EquipmentUniqueID);

                                if (equipment != null)
                                {
                                    equipmentID = equipment.ID;
                                    equipmentName = equipment.Name;

                                    if (!string.IsNullOrEmpty(form.PartUniqueID))
                                    {
                                        var part = edb.EquipmentPart.FirstOrDefault(x => x.EquipmentUniqueID == equipment.UniqueID && x.UniqueID == form.PartUniqueID);

                                        if (part != null)
                                        {
                                            partDescription = part.Description;
                                        }
                                    }
                                }
                            }

                            var subject = string.Format("[{0}][{1}][{2}]", Resources.Resource.RFormRejectNotify, repaiRFormType, form.VHNO);

                            StringBuilder sb = new StringBuilder();

                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.VHNO, form.VHNO));
                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.RepairFormType, repaiRFormType));
                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Subject, form.Subject));
                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Description, form.Description));

                            if (!string.IsNullOrEmpty(equipmentID))
                            {
                                if (!string.IsNullOrEmpty(partDescription))
                                {
                                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Equipment, string.Format("{0}/{1}-{2}", equipmentID, equipmentName, partDescription)));
                                }
                                else
                                {
                                    sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Equipment, string.Format("{0}/{1}", equipmentID, equipmentName)));
                                }
                            }

                            sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.VerifyComment, currentFlowLog.Remark));

                            MailHelper.SendMail(mailAddressList, subject, sb.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public static FileDownloadModel GetFile(string RFormUniqueID, int Seq)
        {
            var model = new FileDownloadModel();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var file = db.RFormFile.First(x => x.RFormUniqueID == RFormUniqueID && x.Seq == Seq);

                    model = new FileDownloadModel()
                    {
                        FormUniqueID = file.RFormUniqueID,
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
    }
}
