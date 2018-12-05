using System;
using System.Text;
using System.Linq;
using System.Web.Mvc;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.ASE;
using Models.Authenticated;
using Models.EquipmentMaintenance.RepairFormManagement;
using System.Transactions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;
using System.IO;
using Models.Shared;
using System.Net.Mail;

namespace DataAccess.ASE
{
    public class RepairFormDataAccessor
    {
        public static RequestResult GetUserOptions(List<Models.Shared.UserModel> AccountList, string Term, bool IsInit)
        {
            RequestResult result = new RequestResult();

            try
            {
                var query = AccountList.Select(x => new Models.ASE.Shared.ASEUserModel
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

        public static RequestResult GetQueryFormModel(string RepairFormUniqueID, string CheckResultUniqueID, string Status, string VHNO, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
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
                            Status = Status,
                            VHNO = VHNO
                        }
                    };

                    var ancestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(Account.OrganizationUniqueID);

                    var formTypeList = db.RFORMTYPE.Where(x => x.ANCESTORORGANIZATIONUNIQUEID == ancestorOrganizationUniqueID).OrderBy(x => x.DESCRIPTION).ToList();

                    foreach (var formType in formTypeList)
                    {
                        model.RepairFormTypeSelectItemList.Add(new SelectListItem()
                        {
                            Value = formType.UNIQUEID,
                            Text = formType.DESCRIPTION
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = (from f in db.RFORM
                                 join o in db.ORGANIZATION
                                 on f.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                 join pm in db.ORGANIZATION
                                 on f.PMORGANIZATIONUNIQUEID equals pm.UNIQUEID
                                 join t in db.RFORMTYPE
                                 on f.RFORMTYPEUNIQUEID equals t.UNIQUEID
                                 where organizationList.Contains(f.ORGANIZATIONUNIQUEID) || organizationList.Contains(f.PMORGANIZATIONUNIQUEID)
                                 select new
                                 {
                                     OrganizationDescription = o.DESCRIPTION,
                                     MaintenanceOrganizationDescription = pm.DESCRIPTION,
                                     f.UNIQUEID,
                                     f.VHNO,
                                     f.STATUS,
                                     f.RFORMTYPEUNIQUEID,
                                     RepairFormType = t.DESCRIPTION,
                                     EstBeginDate = f.ESTBEGINDATE,
                                     EstEndDate = f.ESTENDDATE,
                                     Subject = f.SUBJECT,
                                     f.EQUIPMENTID,
                                     f.EQUIPMENTNAME,
                                     f.PARTDESCRIPTION,
                                     f.TAKEJOBUSERID
                                 }).Distinct().AsQueryable();

                    if (Parameters.StatusList.Count > 0)
                    {
                        if (Parameters.StatusList.Contains("5"))
                        {
                            Parameters.StatusList.Add("4");
                        }

                        query = query.Where(x => Parameters.StatusList.Contains(x.STATUS));
                    }

                    //if (!string.IsNullOrEmpty(Parameters.Status))
                    //{
                    //    if (Parameters.Status == "5")
                    //    {
                    //        query = query.Where(x => x.STATUS == "4");
                    //    }
                    //    else
                    //    {
                    //        query = query.Where(x => x.STATUS == Parameters.Status);
                    //    }
                    //}

                    if (!string.IsNullOrEmpty(Parameters.VHNO))
                    {
                        query = query.Where(x => x.VHNO.Contains(Parameters.VHNO));
                    }

                    if (!string.IsNullOrEmpty(Parameters.RepairFormTypeUniqueID))
                    {
                        query = query.Where(x => x.RFORMTYPEUNIQUEID == Parameters.RepairFormTypeUniqueID);
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

                    var organization = OrganizationDataAccessor.GetOrganization(Parameters.OrganizationUniqueID);

                    var model = new GridViewModel()
                    {
                        Permission = Account.OrganizationPermission(Parameters.OrganizationUniqueID),
                        OrganizationUniqueID = Parameters.OrganizationUniqueID,
                        OrganizationDescription = organization != null ? organization.Description : "*",
                        FullOrganizationDescription = organization != null ? organization.FullDescription : "*",
                        ItemList = query.Select(x=>new GridItem{
                        UniqueID = x.UNIQUEID,
                            OrganizationDescription = x.OrganizationDescription,
                            MaintenanceOrganizationDescription = x.MaintenanceOrganizationDescription,
                            VHNO = x.VHNO,
                            Subject = x.Subject,
                            EstBeginDate = x.EstBeginDate,
                            EstEndDate = x.EstEndDate,
                            EquipmentID = x.EQUIPMENTID,
                            EquipmentName = x.EQUIPMENTNAME,
                            PartDescription = x.PARTDESCRIPTION,
                            RepairFormType = x.RepairFormType,
                            Status = x.STATUS,
                            TakeJobUserID = x.TAKEJOBUSERID
                        }).ToList()
                    };

                    if (Parameters.StatusList.Count > 0)
                    {
                        model.ItemList = model.ItemList.Where(x => Parameters.StatusList.Contains(x.StatusCode)).ToList();
                    }

                    //if (!string.IsNullOrEmpty(Parameters.Status))
                    //{
                    //    model.ItemList = model.ItemList.Where(x => x.StatusCode == Parameters.Status).ToList();
                    //}

                    model.ItemList = model.ItemList.OrderBy(x => x.VHNO).ThenBy(x => x.OrganizationDescription).ThenBy(x => x.MaintenanceOrganizationDescription).ToList();

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

        public static RequestResult GetDetailViewModel(string UniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var repairForm = (from f in db.RFORM
                                      join e in db.EQUIPMENT
                                      on f.EQUIPMENTUNIQUEID equals e.UNIQUEID into tmpEquipment
                                      from e in tmpEquipment.DefaultIfEmpty()
                                      join p in db.EQUIPMENTPART
                                      on f.PARTUNIQUEID equals p.UNIQUEID into tmpPart
                                      from p in tmpPart.DefaultIfEmpty()
                                      join t in db.RFORMTYPE
                                      on f.RFORMTYPEUNIQUEID equals t.UNIQUEID
                                      where f.UNIQUEID == UniqueID
                                      select new
                                      {
                                          Form = f,
                                          FormType = t.DESCRIPTION,
                                          EquipmentID = e != null ? e.ID : "",
                                          EquipmentName = e != null ? e.NAME : "",
                                          PartDescription = p != null ? p.DESCRIPTION : ""
                                      }).First();

                    var organization = OrganizationDataAccessor.GetOrganization(repairForm.Form.ORGANIZATIONUNIQUEID);

                    var pmOrganization = db.ORGANIZATION.First(x => x.UNIQUEID == repairForm.Form.PMORGANIZATIONUNIQUEID);

                    var model = new DetailViewModel()
                    {
                        UniqueID = repairForm.Form.UNIQUEID,
                        AncestorOrganizationUniqueID = organization.AncestorOrganizationUniqueID,
                        ParentOrganizationFullDescription = organization.FullDescription,
                        MaintenanceOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(repairForm.Form.PMORGANIZATIONUNIQUEID),
                        VHNO = repairForm.Form.VHNO,
                        Subject = repairForm.Form.SUBJECT,
                        Status = repairForm.Form.STATUS,
                        Description = repairForm.Form.DESCRIPTION,
                        EstBeginDate = repairForm.Form.ESTBEGINDATE,
                        EstEndDate = repairForm.Form.ESTENDDATE,
                        CreateUser = UserDataAccessor.GetUser(repairForm.Form.CREATEUSERID),
                        CreateTime = repairForm.Form.CREATETIME,
                        //JobManager = UserDataAccessor.GetUser(pmOrganization.MANAGERUSERID),
                        RefuseReason = repairForm.Form.MANAGERREFUSEREASON,
                        JobTime = repairForm.Form.MANAGERJOBTIME,
                        TakeJobUser = UserDataAccessor.GetUser(repairForm.Form.TAKEJOBUSERID),
                        JobRefuseReason = repairForm.Form.JOBREFUSEREASON,
                        TakeJobTime = repairForm.Form.TAKEJOBTIME,
                        RepairFormType = repairForm.FormType,
                        EquipmentID = repairForm.EquipmentID,
                        EquipmentName = repairForm.EquipmentName,
                        PartDescription = repairForm.PartDescription,
                        MaterialList = (from x in db.RFORMMATERIAL
                                        join m in db.MATERIAL
                                        on x.MATERIALUNIQUEID equals m.UNIQUEID
                                        where x.RFORMUNIQUEID == UniqueID
                                        select new MaterialModel
                                        {
                                            Seq = x.SEQ,
                                            UniqueID = x.MATERIALUNIQUEID,
                                            ID = m.ID,
                                            Name = m.NAME,
                                            Quantity = x.QUANTITY
                                        }).OrderBy(x => x.Seq).ToList(),
                        FileList = db.RFORMFILE.Where(f => f.RFORMUNIQUEID == repairForm.Form.UNIQUEID).ToList().Select(f => new FileModel
                        {
                            Seq = f.SEQ,
                            FileName = f.FILENAME,
                            Extension = f.EXTENSION,
                            Size = f.CONTENTLENGTH,
                            UploadTime = f.UPLOADTIME,
                            IsSaved = true
                        }).OrderBy(f => f.UploadTime).ToList()
                    };

                    var workingHourList = db.RFORMWORKINGHOUR.Where(x => x.RFORMUNIQUEID == UniqueID).OrderBy(x => x.SEQ).ToList();

                    foreach (var workingHour in workingHourList)
                    {
                        model.WorkingHourList.Add(new WorkingHourModel()
                        {
                            Seq = workingHour.SEQ,
                            User = UserDataAccessor.GetUser(workingHour.USERID),
                            BeginDate = workingHour.BEGINDATE,
                            EndDate = workingHour.ENDDATE,
                            WorkingHour = double.Parse(workingHour.WORKINGHOUR.Value.ToString())
                        });
                    }

                    var columnList = (from x in db.RFORMTYPECOLUMN
                                      join c in db.RFORMCOLUMN
                                      on x.COLUMNUNIQUEID equals c.UNIQUEID
                                      where x.RFORMTYPEUNIQUEID == repairForm.Form.RFORMTYPEUNIQUEID
                                      orderby x.SEQ
                                      select c).ToList();

                    foreach (var column in columnList)
                    {
                        var value = db.RFORMCOLUMNVALUE.FirstOrDefault(x => x.RFORMUNIQUEID == repairForm.Form.UNIQUEID && x.COLUMNUNIQUEID == column.UNIQUEID);

                        model.ColumnList.Add(new ColumnModel()
                        {
                            UniqueID = column.UNIQUEID,
                            Description = column.DESCRIPTION,
                            OptionUniqueID = value != null ? value.COLUMNOPTIONUNIQUEID : "",
                            Value = value != null ? value.VALUE : "",
                            OptionList = db.RFORMCOLUMNOPTION.Where(x => x.COLUMNUNIQUEID == column.UNIQUEID).Select(x => new Models.EquipmentMaintenance.RepairFormManagement.RFormColumnOption
                            {
                                ColumnUniqueID = x.COLUMNUNIQUEID,
                                UniqueID = x.UNIQUEID,
                                Description = x.DESCRIPTION,
                                Seq = x.SEQ.Value
                            }).OrderBy(x => x.Description).ToList()
                        });
                    }

                    var rformUserList = db.RFORMJOBUSER.Where(x => x.RFORMUNIQUEID == repairForm.Form.UNIQUEID).Select(x => x.USERID).ToList();

                    foreach (var rformUser in rformUserList)
                    {
                        model.JobUserList.Add(UserDataAccessor.GetUser(rformUser));
                    }

                    var flow = db.RFORMFLOW.FirstOrDefault(x => x.RFORMUNIQUEID == model.UniqueID);

                    if (flow != null)
                    {
                        model.IsClosed = flow.ISCLOSED == "Y";
                    }
                    else
                    {
                        model.IsClosed = false;
                    }

                    var logs = db.RFORMFLOWLOG.Where(x => x.RFORMUNIQUEID == model.UniqueID).OrderBy(x => x.SEQ).ToList();

                    foreach (var log in logs)
                    {
                        model.FlowLogList.Add(new FlowLogModel()
                        {
                            Seq = log.SEQ,
                            IsReject = !string.IsNullOrEmpty(log.ISREJECT) ? log.ISREJECT == "Y" : default(bool?),
                            User = UserDataAccessor.GetUser(log.USERID),
                            NotifyTime = log.NOTIFYTIME.Value,
                            VerifyTime = log.VERIFYTIME,
                            Remark = log.REMARK
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var ancestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(OrganizationUniqueID);

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
                                                                                      select new Models.EquipmentMaintenance.RepairFormManagement.RFormSubject
                                                                                      {
                                                                                          UniqueID = s.UNIQUEID,
                                                                                          ID = s.ID,
                                                                                          Description = s.DESCRIPTION,
                                                                                          AncestorOrganizationUniqueID = s.ANCESTORORGANIZATIONUNIQUEID
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

                        var equipmentList = db.EQUIPMENT.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

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
                    else
                    {
                        var checkResult = db.CHECKRESULT.First(x => x.UNIQUEID == CheckResultUniqueID);

                        model.OrganizationUniqueID = checkResult.ORGANIZATIONUNIQUEID;
                        model.AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(checkResult.ORGANIZATIONUNIQUEID);
                        model.FullOrganizationDescription = OrganizationDataAccessor.GetOrganizationFullDescription(checkResult.ORGANIZATIONUNIQUEID);

                        model.EquipmentSelectItemList = new List<SelectListItem>() 
                        { 
                            Define.DefaultSelectListItem(Resources.Resource.None)
                        };

                        var equipmentList = db.EQUIPMENT.Where(x => x.ORGANIZATIONUNIQUEID == checkResult.ORGANIZATIONUNIQUEID).OrderBy(x => x.ID).ToList();

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

                        if (!string.IsNullOrEmpty(checkResult.EQUIPMENTUNIQUEID))
                        {
                            var equipment = db.EQUIPMENT.FirstOrDefault(x => x.UNIQUEID == checkResult.EQUIPMENTUNIQUEID);

                            if (equipment != null && !string.IsNullOrEmpty(equipment.PMORGANIZATIONUNIQUEID))
                            {
                                var maintenanceOrganization = OrganizationDataAccessor.GetOrganization(equipment.PMORGANIZATIONUNIQUEID);

                                model.MaintenanceOrganization = string.Format("{0}/{1}", maintenanceOrganization.ID, maintenanceOrganization.Description);

                                model.FormInput.MaintenanceOrganizationUniqueID = equipment.PMORGANIZATIONUNIQUEID;
                            }
                        }

                        model.CheckResultUniqueID = checkResult.UNIQUEID;
                        model.EquipmentID = checkResult.EQUIPMENTID;
                        model.EquipmentName = checkResult.EQUIPMENTNAME;
                        model.PartDescription = !string.IsNullOrEmpty(checkResult.PARTUNIQUEID) ? checkResult.PARTDESCRIPTION : "";

                        string reason = string.Empty;

                        var abnormalReasonList = db.CHECKRESULTABNORMALREASON.Where(x => x.CHECKRESULTUNIQUEID == checkResult.UNIQUEID).OrderBy(x => x.ABNORMALREASONID).ToList();

                        if (abnormalReasonList.Count > 0)
                        {
                            var sb = new StringBuilder();

                            foreach (var abnormalReason in abnormalReasonList)
                            {
                                if (!string.IsNullOrEmpty(abnormalReason.ABNORMALREASONDESCRIPTION))
                                {
                                    sb.Append(abnormalReason.ABNORMALREASONDESCRIPTION);
                                }
                                else
                                {
                                    sb.Append(abnormalReason.ABNORMALREASONREMARK);
                                }

                                sb.Append("、");
                            }

                            sb.Remove(sb.Length - 1, 1);

                            reason = sb.ToString();
                        }

                        model.FormInput.EquipmentUniqueID = string.Format("{0}{1}{2}", checkResult.EQUIPMENTUNIQUEID, Define.Seperator, checkResult.PARTUNIQUEID);
                        model.FormInput.Subject = checkResult.CHECKITEMDESCRIPTION;
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

                        var temp = db.RFORM.Where(x => x.VHNO.StartsWith(vhnoPrefix)).OrderByDescending(x => x.VHNO).FirstOrDefault();

                        if (temp != null)
                        {
                            seq = int.Parse(temp.VHNO.Substring(5)) + 1;
                        }

                        var vhno = string.Format("{0}{1}", vhnoPrefix, seq.ToString().PadLeft(4, '0'));

                        var uniqueID = Guid.NewGuid().ToString();

                        var createTime = DateTime.Now;

                        var equipment = db.EQUIPMENT.FirstOrDefault(x => x.UNIQUEID == equipmentUniqueID);
                        var part = db.EQUIPMENTPART.FirstOrDefault(x => x.UNIQUEID == partUniqueID);

                        var form = new RFORM()
                        {
                            UNIQUEID = uniqueID,
                            STATUS = "0",
                            VHNO = vhno,
                            ORGANIZATIONUNIQUEID = Model.OrganizationUniqueID,
                            PMORGANIZATIONUNIQUEID = maintenanceOrganizationUniqueID,
                            EQUIPMENTUNIQUEID = equipmentUniqueID,
                            PARTUNIQUEID = partUniqueID,
                            RFORMTYPEUNIQUEID = Model.FormInput.RepairFormTypeUniqueID,
                            SUBJECT = Model.FormInput.Subject,
                            DESCRIPTION = Model.FormInput.Description,
                            CREATEUSERID = UserID,
                            CREATETIME = createTime,
                            EQUIPMENTID = equipment != null ? equipment.ID : string.Empty,
                            EQUIPMENTNAME = equipment != null ? equipment.NAME : string.Empty,
                            PARTDESCRIPTION = part != null ? part.DESCRIPTION : string.Empty
                        };

                        if (Model.FormInput.IsRepairBySelf)
                        {
                            form.STATUS = "4";
                            form.MANAGERJOBTIME = createTime;
                            form.JOBREFUSEREASON = string.Empty;
                            form.TAKEJOBTIME = createTime;
                            form.TAKEJOBUSERID = UserID;
                            form.ESTBEGINDATE = Model.FormInput.EstBeginDate;
                            form.ESTENDDATE = Model.FormInput.EstEndDate;

                            db.RFORMJOBUSER.Add(new RFORMJOBUSER()
                            {
                                RFORMUNIQUEID = uniqueID,
                                USERID = UserID
                            });
                        }

                        db.RFORM.Add(form);

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

        public static RequestResult GetEditFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var repairForm = (from f in db.RFORM
                                      join e in db.EQUIPMENT
                                      on f.EQUIPMENTUNIQUEID equals e.UNIQUEID into tmpEquipment
                                      from e in tmpEquipment.DefaultIfEmpty()
                                      join p in db.EQUIPMENTPART
                                      on f.PARTUNIQUEID equals p.UNIQUEID into tmpPart
                                      from p in tmpPart.DefaultIfEmpty()
                                      join t in db.RFORMTYPE
                                      on f.RFORMTYPEUNIQUEID equals t.UNIQUEID
                                      where f.UNIQUEID == UniqueID
                                      select new
                                      {
                                          Form = f,
                                          FormType = t.DESCRIPTION,
                                          EquipmentID = e != null ? e.ID : "",
                                          EquipmentName = e != null ? e.NAME : "",
                                          PartDescription = p != null ? p.DESCRIPTION : ""
                                      }).First();

                    var maintenanceOrganization = OrganizationDataAccessor.GetOrganization(repairForm.Form.PMORGANIZATIONUNIQUEID);

                    var model = new EditFormModel()
                    {
                        UniqueID = repairForm.Form.UNIQUEID,
                        AncestorOrganizationUniqueID = maintenanceOrganization.AncestorOrganizationUniqueID,
                        OrganizationUniqueID = repairForm.Form.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(repairForm.Form.ORGANIZATIONUNIQUEID),
                        MaintenanceOrganizationFullDescription = maintenanceOrganization.FullDescription,
                        VHNO = repairForm.Form.VHNO,
                        RefuseReason = repairForm.Form.MANAGERREFUSEREASON,
                        JobRefuseReason = repairForm.Form.JOBREFUSEREASON,
                         RepairFormType = repairForm.FormType,
                        EquipmentID = repairForm.EquipmentID,
                        EquipmentName = repairForm.EquipmentName,
                        PartDescription = repairForm.PartDescription,
                         EquipmentUniqueID=repairForm.Form.EQUIPMENTUNIQUEID,
                          PartUniqueID=repairForm.Form.PARTUNIQUEID,
                          Status=repairForm.Form.STATUS,
                        Subject = repairForm.Form.SUBJECT,
                        Description = repairForm.Form.DESCRIPTION,
                        EstBeginDate = repairForm.Form.ESTBEGINDATE,
                        EstEndDate = repairForm.Form.ESTENDDATE,
                        CreateUser = UserDataAccessor.GetUser(repairForm.Form.CREATEUSERID),
                        CreateTime = repairForm.Form.CREATETIME,
                        //JobManager = UserDataAccessor.GetUser(maintenanceOrganization.ManagerID),
                        JobTime = repairForm.Form.MANAGERJOBTIME,
                        TakeJobUser = UserDataAccessor.GetUser(repairForm.Form.TAKEJOBUSERID),
                        TakeJobTime = repairForm.Form.TAKEJOBTIME,
                        MaterialList = (from x in db.RFORMMATERIAL
                                        join m in db.MATERIAL
                                        on x.MATERIALUNIQUEID equals m.UNIQUEID
                                        where x.RFORMUNIQUEID == UniqueID
                                        select new MaterialModel
                                        {
                                            Seq = x.SEQ,
                                            UniqueID = x.MATERIALUNIQUEID,
                                            ID = m.ID,
                                            Name = m.NAME,
                                            Quantity = x.QUANTITY
                                        }).OrderBy(x => x.Seq).ToList(),
                        FileList = db.RFORMFILE.Where(f => f.RFORMUNIQUEID == repairForm.Form.UNIQUEID).ToList().Select(f => new FileModel
                        {
                            Seq = f.SEQ,
                            FileName = f.FILENAME,
                            Extension = f.EXTENSION,
                            Size = f.CONTENTLENGTH,
                            UploadTime = f.UPLOADTIME,
                            IsSaved = true
                        }).OrderBy(f => f.UploadTime).ToList()
                    };

                    var workingHourList = db.RFORMWORKINGHOUR.Where(x => x.RFORMUNIQUEID == UniqueID).OrderBy(x => x.SEQ).ToList();

                    foreach (var workingHour in workingHourList)
                    {
                        model.WorkingHourList.Add(new WorkingHourModel()
                        {
                            Seq = workingHour.SEQ,
                            User = UserDataAccessor.GetUser(workingHour.USERID),
                            BeginDate = workingHour.BEGINDATE,
                            EndDate = workingHour.ENDDATE,
                            WorkingHour = double.Parse(workingHour.WORKINGHOUR.Value.ToString())
                        });
                    }

                    var columnList = (from x in db.RFORMTYPECOLUMN
                                      join c in db.RFORMCOLUMN
                                      on x.COLUMNUNIQUEID equals c.UNIQUEID
                                      where x.RFORMTYPEUNIQUEID == repairForm.Form.RFORMTYPEUNIQUEID
                                      orderby x.SEQ
                                      select c).ToList();

                    foreach (var column in columnList)
                    {
                        var value = db.RFORMCOLUMNVALUE.FirstOrDefault(x => x.RFORMUNIQUEID == repairForm.Form.UNIQUEID && x.COLUMNUNIQUEID == column.UNIQUEID);

                        model.ColumnList.Add(new ColumnModel()
                        {
                            UniqueID = column.UNIQUEID,
                            Description = column.DESCRIPTION,
                            OptionUniqueID = value != null ? value.COLUMNOPTIONUNIQUEID : "",
                            Value = value != null ? value.VALUE : "",
                            OptionList = db.RFORMCOLUMNOPTION.Where(x => x.COLUMNUNIQUEID == column.UNIQUEID).Select(x => new Models.EquipmentMaintenance.RepairFormManagement.RFormColumnOption
                            {
                                ColumnUniqueID = x.COLUMNUNIQUEID,
                                UniqueID = x.UNIQUEID,
                                Description = x.DESCRIPTION,
                                Seq = x.SEQ.Value
                            }).OrderBy(x => x.Description).ToList()
                        });
                    }

                    var rformUserList = db.RFORMJOBUSER.Where(x => x.RFORMUNIQUEID == repairForm.Form.UNIQUEID).Select(x => x.USERID).ToList();

                    foreach (var rformUser in rformUserList)
                    {
                        model.JobUserList.Add(UserDataAccessor.GetUser(rformUser));
                    }

                    var flow = db.RFORMFLOW.FirstOrDefault(x => x.RFORMUNIQUEID == model.UniqueID);

                    if (flow != null)
                    {
                        model.IsClosed = flow.ISCLOSED == "Y";
                    }
                    else
                    {
                        model.IsClosed = false;
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

        public static RequestResult Edit(EditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        db.RFORMCOLUMNVALUE.RemoveRange(db.RFORMCOLUMNVALUE.Where(x => x.RFORMUNIQUEID == Model.UniqueID).ToList());
                        db.RFORMMATERIAL.RemoveRange(db.RFORMMATERIAL.Where(x => x.RFORMUNIQUEID == Model.UniqueID).ToList());
                        db.RFORMWORKINGHOUR.RemoveRange(db.RFORMWORKINGHOUR.Where(x => x.RFORMUNIQUEID == Model.UniqueID).ToList());

                        db.SaveChanges();

                        db.RFORMCOLUMNVALUE.AddRange(Model.ColumnList.Select(x => new RFORMCOLUMNVALUE
                        {
                            RFORMUNIQUEID = Model.UniqueID,
                            COLUMNUNIQUEID = x.UniqueID,
                            COLUMNOPTIONUNIQUEID = x.OptionUniqueID,
                            VALUE = x.Value
                        }).ToList());

                        db.RFORMMATERIAL.AddRange(Model.MaterialList.Select(x => new RFORMMATERIAL
                        {
                            RFORMUNIQUEID = Model.UniqueID,
                            SEQ = x.Seq,
                            MATERIALUNIQUEID = x.UniqueID,
                            QUANTITY = x.Quantity
                        }).ToList());

                        foreach (var workingHour in Model.WorkingHourList.OrderBy(x => x.Seq))
                        {
                            db.RFORMWORKINGHOUR.Add(new RFORMWORKINGHOUR()
                            {
                                RFORMUNIQUEID = Model.UniqueID,
                                SEQ = workingHour.Seq,
                                BEGINDATE = workingHour.BeginDate,
                                ENDDATE = workingHour.EndDate,
                                WORKINGHOUR = decimal.Parse(workingHour.WorkingHour.ToString()),
                                USERID = workingHour.User.ID
                            });
                        }

                        db.SaveChanges();

                        var fileList = db.RFORMFILE.Where(x => x.RFORMUNIQUEID == Model.UniqueID).ToList();

                        foreach (var file in fileList)
                        {
                            if (!Model.FileList.Any(x => x.Seq == file.SEQ))
                            {
                                try
                                {
                                    System.IO.File.Delete(Path.Combine(Config.EquipmentMaintenanceFileFolderPath, string.Format("{0}_{1}.{2}", Model.UniqueID, file.SEQ, file.EXTENSION)));
                                }
                                catch { }
                            }
                        }

                        db.RFORMFILE.RemoveRange(fileList);

                        db.SaveChanges();

                        foreach (var file in Model.FileList)
                        {
                            db.RFORMFILE.Add(new RFORMFILE()
                            {
                                RFORMUNIQUEID = Model.UniqueID,
                                SEQ = file.Seq,
                                EXTENSION = file.Extension,
                                FILENAME = file.FileName,
                                UPLOADTIME = file.UploadTime,
                                CONTENTLENGTH = file.Size
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var repairForm = db.RFORM.First(x => x.UNIQUEID == UniqueID);

                    var nextVerifyOrganization = (from f in db.FLOW
                                                  join x in db.FLOWFORM
                                                  on f.UNIQUEID equals x.FLOWUNIQUEID
                                                  join v in db.FLOWVERIFYORGANIZATION
                                                  on f.UNIQUEID equals v.FLOWUNIQUEID
                                                  join o in db.ORGANIZATION
                                                  on v.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                                  where f.ORGANIZATIONUNIQUEID == repairForm.PMORGANIZATIONUNIQUEID && x.FORM == Define.EnumForm.RepairForm.ToString() && x.RFORMTYPEUNIQUEID == repairForm.RFORMTYPEUNIQUEID
                                                  select new
                                                  {
                                                      o.UNIQUEID,
                                                      o.DESCRIPTION,
                                                      o.MANAGERUSERID,
                                                      v.SEQ
                                                  }).OrderBy(x => x.SEQ).FirstOrDefault();

                    //有設定簽核流程
                    if (nextVerifyOrganization != null)
                    {
                        if (!string.IsNullOrEmpty(nextVerifyOrganization.MANAGERUSERID))
                        {
                            repairForm.STATUS = "6";

                            var flow = db.RFORMFLOW.FirstOrDefault(x => x.RFORMUNIQUEID == UniqueID);

                            int currentSeq = 1;

                            if (flow == null)
                            {
                                db.RFORMFLOW.Add(new RFORMFLOW()
                                {
                                    RFORMUNIQUEID = UniqueID,
                                    CURRENTSEQ = currentSeq,
                                    ISCLOSED = "N"
                                });
                            }
                            else
                            {
                                currentSeq = flow.CURRENTSEQ.Value + 1;

                                flow.CURRENTSEQ = currentSeq;
                            }

                            db.RFORMFLOWLOG.Add(new RFORMFLOWLOG()
                            {
                                RFORMUNIQUEID = UniqueID,
                                SEQ = currentSeq,
                                FLOWSEQ = 1,
                                USERID = nextVerifyOrganization.MANAGERUSERID,
                                NOTIFYTIME = DateTime.Now
                            });

                            db.SaveChanges();

                            result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Submit, Resources.Resource.Complete));

                            SendRFormApproveNotifyMail(UniqueID);
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1} {2} {3}", Resources.Resource.Organization, nextVerifyOrganization.DESCRIPTION, Resources.Resource.NotSet, Resources.Resource.Manager));
                        }
                    }
                    else
                    {
                        var organization = db.ORGANIZATION.First(x => x.UNIQUEID == repairForm.PMORGANIZATIONUNIQUEID);

                        if (!string.IsNullOrEmpty(organization.MANAGERUSERID))
                        {
                            repairForm.STATUS = "6";

                            var flow = db.RFORMFLOW.FirstOrDefault(x => x.RFORMUNIQUEID == UniqueID);

                            int currentSeq = 1;

                            if (flow == null)
                            {
                                db.RFORMFLOW.Add(new RFORMFLOW()
                                {
                                    RFORMUNIQUEID = UniqueID,
                                    CURRENTSEQ = currentSeq,
                                    ISCLOSED = "N"
                                });
                            }
                            else
                            {
                                currentSeq = flow.CURRENTSEQ.Value + 1;

                                flow.CURRENTSEQ = currentSeq;
                            }

                            db.RFORMFLOWLOG.Add(new RFORMFLOWLOG()
                            {
                                RFORMUNIQUEID = UniqueID,
                                SEQ = currentSeq,
                                FLOWSEQ = 0,
                                USERID = organization.MANAGERUSERID,
                                NOTIFYTIME = DateTime.Now
                            });

                            db.SaveChanges();

                            result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Submit, Resources.Resource.Complete));

                            SendRFormApproveNotifyMail(UniqueID);
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1} {2} {3}", Resources.Resource.Organization, organization.DESCRIPTION, Resources.Resource.NotSet, Resources.Resource.Manager));
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
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var repairForm = db.RFORM.First(x => x.UNIQUEID == Model.UniqueID);

                        repairForm.STATUS = "2";
                        repairForm.MANAGERJOBTIME = DateTime.Now;
                        repairForm.JOBREFUSEREASON = string.Empty;

                        db.RFORMJOBUSER.RemoveRange(db.RFORMJOBUSER.Where(x => x.RFORMUNIQUEID == repairForm.UNIQUEID).ToList());

                        db.SaveChanges();

                        db.RFORMJOBUSER.AddRange(Model.JobUserList.Select(x => new RFORMJOBUSER
                        {
                            RFORMUNIQUEID = repairForm.UNIQUEID,
                            USERID = x.ID
                        }).ToList());

                        db.SaveChanges();

                        result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.Complete));

                        SendRFormTakeJobNotifyMail(repairForm.UNIQUEID);
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var repairForm = db.RFORM.First(x => x.UNIQUEID == UniqueID);

                    repairForm.STATUS = "1";
                    repairForm.MANAGERREFUSEREASON = RefuseReason;

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

        public static RequestResult TakeJob(string UniqueID,string EstBeginDateString, string EstEndDateString,string RealTakeJobDateString,string RealTakeJobHour,string RealTakeJobMin,string RealTakeJobUserID,Account Account)
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
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var repairForm = db.RFORM.First(x => x.UNIQUEID == UniqueID);

                        repairForm.STATUS = "4";
                        repairForm.ESTBEGINDATE = DateTimeHelper.DateStringWithSeperator2DateTime(EstBeginDateString);
                        repairForm.ESTENDDATE = DateTimeHelper.DateStringWithSeperator2DateTime(EstEndDateString);
                        repairForm.TAKEJOBUSERID = Account.ID;
                        repairForm.TAKEJOBTIME = DateTime.Now;

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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var repairForm = db.RFORM.First(x => x.UNIQUEID == UniqueID);

                    repairForm.STATUS = "3";
                    repairForm.JOBREFUSEUSERID = Account.ID;
                    repairForm.JOBREFUSETIME = null;
                    repairForm.JOBREFUSEREASON = JobRefuseReason;

                    //db.RFORMJOBUSER.RemoveRange(db.RFORMJOBUSER.Where(x => x.RFORMUNIQUEID == repairForm.UNIQUEID).ToList());

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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var repairForm = db.RFORM.First(x => x.UNIQUEID == UniqueID);
                    var flow = db.RFORMFLOW.First(x => x.RFORMUNIQUEID == UniqueID);
                    var currentFlowLog = db.RFORMFLOWLOG.Where(x => x.RFORMUNIQUEID == UniqueID).OrderByDescending(x => x.SEQ).First();

                    if (currentFlowLog.FLOWSEQ.Value == 0)
                    {
                        repairForm.STATUS = "8";

                        flow.ISCLOSED = "Y";

                        currentFlowLog.ISREJECT = "N";
                        currentFlowLog.VERIFYTIME = DateTime.Now;
                        currentFlowLog.REMARK = Remark;

                        db.SaveChanges();

                        //QFormDataAccessor.Closed(UniqueID);

                        result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Approve, Resources.Resource.Complete));
                    }
                    else
                    {
                        var nextVerifyOrganization = (from f in db.FLOW
                                                      join x in db.FLOWFORM
                                                      on f.UNIQUEID equals x.FLOWUNIQUEID
                                                      join v in db.FLOWVERIFYORGANIZATION
                                                      on f.UNIQUEID equals v.FLOWUNIQUEID
                                                      join o in db.ORGANIZATION
                                                      on v.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                                      where f.ORGANIZATIONUNIQUEID == repairForm.PMORGANIZATIONUNIQUEID && x.FORM == Define.EnumForm.RepairForm.ToString() && x.RFORMTYPEUNIQUEID == repairForm.RFORMTYPEUNIQUEID
                                                      select new
                                                      {
                                                          o.UNIQUEID,
                                                          o.DESCRIPTION,
                                                          o.MANAGERUSERID,
                                                          v.SEQ
                                                      }).Where(x => x.SEQ > currentFlowLog.FLOWSEQ).OrderBy(x => x.SEQ).FirstOrDefault();

                        if (nextVerifyOrganization != null)
                        {
                            if (!string.IsNullOrEmpty(nextVerifyOrganization.MANAGERUSERID))
                            {
                                flow.CURRENTSEQ = flow.CURRENTSEQ.Value + 1;

                                currentFlowLog.ISREJECT = "N";
                                currentFlowLog.VERIFYTIME = DateTime.Now;
                                currentFlowLog.REMARK = Remark;

                                db.RFORMFLOWLOG.Add(new RFORMFLOWLOG()
                                {
                                    RFORMUNIQUEID = UniqueID,
                                    SEQ = flow.CURRENTSEQ.Value,
                                    FLOWSEQ = nextVerifyOrganization.SEQ,
                                    USERID = nextVerifyOrganization.MANAGERUSERID,
                                    NOTIFYTIME = DateTime.Now
                                });

                                db.SaveChanges();

                                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Approve, Resources.Resource.Complete));

                                SendRFormApproveNotifyMail(UniqueID);
                            }
                            else
                            {
                                result.ReturnFailedMessage(string.Format("{0} {1} {2} {3}", Resources.Resource.Organization, nextVerifyOrganization.DESCRIPTION, Resources.Resource.NotSet, Resources.Resource.Manager));
                            }
                        }
                        else
                        {
                            repairForm.STATUS = "8";

                            flow.ISCLOSED = "Y";

                            currentFlowLog.ISREJECT = "N";
                            currentFlowLog.VERIFYTIME = DateTime.Now;
                            currentFlowLog.REMARK = Remark;

                            db.SaveChanges();

                            result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Approve, Resources.Resource.Complete));

                            //QFormDataAccessor.Closed(UniqueID);
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var repairForm = db.RFORM.First(x => x.UNIQUEID == UniqueID);

                    repairForm.STATUS = "7";

                    var currentFlowLog = db.RFORMFLOWLOG.Where(x => x.RFORMUNIQUEID == UniqueID).OrderByDescending(x => x.SEQ).First();

                    currentFlowLog.ISREJECT = "Y";
                    currentFlowLog.VERIFYTIME = DateTime.Now;
                    currentFlowLog.REMARK = Remark;

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

                using (ASEDbEntities db = new ASEDbEntities())
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

                                var material = db.MATERIAL.First(x => x.UNIQUEID == materialUniqueID);

                                tempList.Add(new MaterialModel()
                                {
                                    Seq = seq,
                                    UniqueID = material.UNIQUEID,
                                    ID = material.ID,
                                    Name = material.NAME,
                                    Quantity = 0
                                });
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(equipmentType))
                            {
                                var materialList = db.MATERIAL.Where(x => x.ORGANIZATIONUNIQUEID == organizationUniqueID && x.EQUIPMENTTYPE == equipmentType).ToList();

                                foreach (var material in materialList)
                                {
                                    if (!tempList.Any(x => x.UniqueID == material.UNIQUEID))
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
                                            UniqueID = material.UNIQUEID,
                                            ID = material.ID,
                                            Name = material.NAME,
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
                                        var materialList = db.MATERIAL.Where(x => x.ORGANIZATIONUNIQUEID == organization).ToList();

                                        foreach (var material in materialList)
                                        {
                                            if (!tempList.Any(x => x.UniqueID == material.UNIQUEID))
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
                                                    UniqueID = material.UNIQUEID,
                                                    ID = material.ID,
                                                    Name = material.NAME,
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
                worksheet.AddMergedRegion(new CellRangeAddress(6, 6, 3, 4));
                worksheet.AddMergedRegion(new CellRangeAddress(7, 7, 0, 1));
                worksheet.AddMergedRegion(new CellRangeAddress(7, 7, 3, 4));
                worksheet.AddMergedRegion(new CellRangeAddress(8, 11, 0, 0));
                worksheet.AddMergedRegion(new CellRangeAddress(8, 8, 2, 5));
                worksheet.AddMergedRegion(new CellRangeAddress(9, 9, 2, 5));
                worksheet.AddMergedRegion(new CellRangeAddress(10, 10, 2, 5));
                worksheet.AddMergedRegion(new CellRangeAddress(11, 11, 1, 5));
                #endregion

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
                cell.SetCellValue(Model.MaintenanceOrganizationFullDescription);
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

                #region Row 6
                row = worksheet.CreateRow(6);
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

                #region Row 7
                row = worksheet.CreateRow(7);
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

                #region Row 8
                row = worksheet.CreateRow(8);
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

                #region Row 9
                row = worksheet.CreateRow(9);
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

                #region Row 10
                row = worksheet.CreateRow(10);
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

                #region Row 11
                row = worksheet.CreateRow(11);
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

                var currentRowIndex = 12;

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
                cell.SetCellValue(Resources.Resource.FrontWorkingHour);

                cell = row.CreateCell(3);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.WorkingHour);

                cell = row.CreateCell(4);
                cell.CellStyle = cellStyle;

                cell = row.CreateCell(5);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.TotalWorkingHour);

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

                currentRowIndex = workingHourLastRow+1;
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
                                              select new UserModel
                                              {
                                                  ID = u.ID,
                                                  Name = u.NAME,
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
                                                OrganizationDescription = o.DESCRIPTION
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
                //var mailAddressList = new List<MailAddress>();

                //using (ASEDbEntities db = new ASEDbEntities())
                //{
                //    var form = db.RFORM.First(x => x.UNIQUEID == RFormUniqueID);

                //    var user = db.ACCOUNT.FirstOrDefault(x => x.ID == form.JOBMANAGERID);

                //    if (user != null && !string.IsNullOrEmpty(user.EMAIL))
                //    {
                //        mailAddressList.Add(new MailAddress(user.EMAIL, user.NAME));
                //    }

                //    if (mailAddressList.Count > 0)
                //    {
                //        var repairFormType = string.Empty;

                //        if (form.RFORMTYPEUNIQUEID == "M")
                //        {
                //            repairFormType = Resources.Resource.MaintenanceRoute;
                //        }
                //        else
                //        {
                //            var formType = db.RFORMTYPE.FirstOrDefault(x => x.UNIQUEID == form.RFORMTYPEUNIQUEID);

                //            if (formType != null)
                //            {
                //                repairFormType = formType.DESCRIPTION;
                //            }
                //            else
                //            {
                //                repairFormType = "-";
                //            }
                //        }

                //        var equipmentID = string.Empty;
                //        var equipmentName = string.Empty;
                //        var partDescription = string.Empty;

                //        if (!string.IsNullOrEmpty(form.EQUIPMENTUNIQUEID))
                //        {
                //            var equipment = db.EQUIPMENT.FirstOrDefault(x => x.UNIQUEID == form.EQUIPMENTUNIQUEID);

                //            if (equipment != null)
                //            {
                //                equipmentID = equipment.ID;
                //                equipmentName = equipment.NAME;

                //                if (!string.IsNullOrEmpty(form.PARTUNIQUEID))
                //                {
                //                    var part = db.EQUIPMENTPART.FirstOrDefault(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.UNIQUEID == form.PARTUNIQUEID);

                //                    if (part != null)
                //                    {
                //                        partDescription = part.DESCRIPTION;
                //                    }
                //                }
                //            }
                //        }

                //        var subject = string.Format("[{0}][{1}][{2}]", Resources.Resource.RFormJobNotify, repairFormType, form.VHNO);

                //        StringBuilder sb = new StringBuilder();

                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.VHNO, form.VHNO));
                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.RepairFormType, repairFormType));
                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Subject, form.SUBJECT));
                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Description, form.DESCRIPTION));

                //        if (!string.IsNullOrEmpty(equipmentID))
                //        {
                //            if (!string.IsNullOrEmpty(partDescription))
                //            {
                //                sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Equipment, string.Format("{0}/{1}-{2}", equipmentID, equipmentName, partDescription)));
                //            }
                //            else
                //            {
                //                sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Equipment, string.Format("{0}/{1}", equipmentID, equipmentName)));
                //            }
                //        }

                //        var createUser = UserDataAccessor.GetUser(form.CREATEUSERID);
                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.CreateUser, createUser.User));

                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.CreateTime, DateTimeHelper.DateTime2DateTimeStringWithSeperator(form.CREATETIME)));

                //        MailHelper.SendMail(mailAddressList, subject, sb.ToString());
                //    }
                //}
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
                //var mailAddressList = new List<MailAddress>();

                //using (ASEDbEntities db = new ASEDbEntities())
                //{
                //    var form = db.RFORM.First(x => x.UNIQUEID == RFormUniqueID);

                //    var user = db.ACCOUNT.FirstOrDefault(x => x.ID == form.CREATEUSERID);

                //    if (user != null && !string.IsNullOrEmpty(user.EMAIL))
                //    {
                //        mailAddressList.Add(new MailAddress(user.EMAIL, user.NAME));
                //    }

                //    if (mailAddressList.Count > 0)
                //    {
                //        var repairFormType = string.Empty;

                //        if (form.RFORMTYPEUNIQUEID == "M")
                //        {
                //            repairFormType = Resources.Resource.MaintenanceRoute;
                //        }
                //        else
                //        {
                //            var formType = db.RFORMTYPE.FirstOrDefault(x => x.UNIQUEID == form.RFORMTYPEUNIQUEID);

                //            if (formType != null)
                //            {
                //                repairFormType = formType.DESCRIPTION;
                //            }
                //            else
                //            {
                //                repairFormType = "-";
                //            }
                //        }

                //        var equipmentID = string.Empty;
                //        var equipmentName = string.Empty;
                //        var partDescription = string.Empty;

                //        if (!string.IsNullOrEmpty(form.EQUIPMENTUNIQUEID))
                //        {
                //            var equipment = db.EQUIPMENT.FirstOrDefault(x => x.UNIQUEID == form.EQUIPMENTUNIQUEID);

                //            if (equipment != null)
                //            {
                //                equipmentID = equipment.ID;
                //                equipmentName = equipment.NAME;

                //                if (!string.IsNullOrEmpty(form.PARTUNIQUEID))
                //                {
                //                    var part = db.EQUIPMENTPART.FirstOrDefault(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.UNIQUEID == form.PARTUNIQUEID);

                //                    if (part != null)
                //                    {
                //                        partDescription = part.DESCRIPTION;
                //                    }
                //                }
                //            }
                //        }

                //        var subject = string.Format("[{0}][{1}][{2}]", Resources.Resource.RFormRefuseMotify, repairFormType, form.VHNO);

                //        StringBuilder sb = new StringBuilder();

                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.VHNO, form.VHNO));
                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.RepairFormType, repairFormType));
                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Subject, form.SUBJECT));
                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Description, form.DESCRIPTION));

                //        if (!string.IsNullOrEmpty(equipmentID))
                //        {
                //            if (!string.IsNullOrEmpty(partDescription))
                //            {
                //                sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Equipment, string.Format("{0}/{1}-{2}", equipmentID, equipmentName, partDescription)));
                //            }
                //            else
                //            {
                //                sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Equipment, string.Format("{0}/{1}", equipmentID, equipmentName)));
                //            }
                //        }

                //        var jobManager = UserDataAccessor.GetUser(form.JOBMANAGERID);
                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.JobManager, jobManager.User));

                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.RefuseReason, form.REFUSEREASON));

                //        MailHelper.SendMail(mailAddressList, subject, sb.ToString());
                //    }
                //}
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
                //var mailAddressList = new List<MailAddress>();

                //using (ASEDbEntities db = new ASEDbEntities())
                //{
                //    var form = db.RFORM.First(x => x.UNIQUEID == RFormUniqueID);

                //    var jobUserList = db.RFORMUSER.Where(x => x.RFORMUNIQUEID == RFormUniqueID).Select(x => x.USERID).ToList();

                //    foreach (var jobUser in jobUserList)
                //    {
                //        var user = db.ACCOUNT.FirstOrDefault(x => x.ID == jobUser);

                //        if (user != null && !string.IsNullOrEmpty(user.EMAIL))
                //        {
                //            mailAddressList.Add(new MailAddress(user.EMAIL, user.NAME));
                //        }
                //    }

                //    if (mailAddressList.Count > 0)
                //    {
                //        var repairFormType = string.Empty;

                //        if (form.RFORMTYPEUNIQUEID == "M")
                //        {
                //            repairFormType = Resources.Resource.MaintenanceRoute;
                //        }
                //        else
                //        {
                //            var formType = db.RFORMTYPE.FirstOrDefault(x => x.UNIQUEID == form.RFORMTYPEUNIQUEID);

                //            if (formType != null)
                //            {
                //                repairFormType = formType.DESCRIPTION;
                //            }
                //            else
                //            {
                //                repairFormType = "-";
                //            }
                //        }

                //        var equipmentID = string.Empty;
                //        var equipmentName = string.Empty;
                //        var partDescription = string.Empty;

                //        if (!string.IsNullOrEmpty(form.EQUIPMENTUNIQUEID))
                //        {
                //            var equipment = db.EQUIPMENT.FirstOrDefault(x => x.UNIQUEID == form.EQUIPMENTUNIQUEID);

                //            if (equipment != null)
                //            {
                //                equipmentID = equipment.ID;
                //                equipmentName = equipment.NAME;

                //                if (!string.IsNullOrEmpty(form.PARTUNIQUEID))
                //                {
                //                    var part = db.EQUIPMENTPART.FirstOrDefault(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.UNIQUEID == form.PARTUNIQUEID);

                //                    if (part != null)
                //                    {
                //                        partDescription = part.DESCRIPTION;
                //                    }
                //                }
                //            }
                //        }

                //        var subject = string.Format("[{0}][{1}][{2}]", Resources.Resource.RFormTakeJobMotify, repairFormType, form.VHNO);

                //        StringBuilder sb = new StringBuilder();

                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.VHNO, form.VHNO));
                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.RepairFormType, repairFormType));
                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Subject, form.SUBJECT));
                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Description, form.DESCRIPTION));

                //        if (!string.IsNullOrEmpty(equipmentID))
                //        {
                //            if (!string.IsNullOrEmpty(partDescription))
                //            {
                //                sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Equipment, string.Format("{0}/{1}-{2}", equipmentID, equipmentName, partDescription)));
                //            }
                //            else
                //            {
                //                sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Equipment, string.Format("{0}/{1}", equipmentID, equipmentName)));
                //            }
                //        }

                //        var createUser = UserDataAccessor.GetUser(form.CREATEUSERID);
                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.CreateUser, createUser.User));

                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.CreateTime, DateTimeHelper.DateTime2DateTimeStringWithSeperator(form.CREATETIME)));

                //        var jobManager = UserDataAccessor.GetUser(form.JOBMANAGERID);
                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.JobManager, jobManager.User));

                //        MailHelper.SendMail(mailAddressList, subject, sb.ToString());
                //    }
                //}
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
                //var mailAddressList = new List<MailAddress>();

                //using (ASEDbEntities db = new ASEDbEntities())
                //{
                //    var form = db.RFORM.First(x => x.UNIQUEID == RFormUniqueID);

                //    var user = db.ACCOUNT.FirstOrDefault(x => x.ID == form.JOBMANAGERID);

                //    if (user != null && !string.IsNullOrEmpty(user.EMAIL))
                //    {
                //        mailAddressList.Add(new MailAddress(user.EMAIL, user.NAME));
                //    }

                //    if (mailAddressList.Count > 0)
                //    {
                //        var repairFormType = string.Empty;

                //        if (form.RFORMTYPEUNIQUEID == "M")
                //        {
                //            repairFormType = Resources.Resource.MaintenanceRoute;
                //        }
                //        else
                //        {
                //            var formType = db.RFORMTYPE.FirstOrDefault(x => x.UNIQUEID == form.RFORMTYPEUNIQUEID);

                //            if (formType != null)
                //            {
                //                repairFormType = formType.DESCRIPTION;
                //            }
                //            else
                //            {
                //                repairFormType = "-";
                //            }
                //        }

                //        var equipmentID = string.Empty;
                //        var equipmentName = string.Empty;
                //        var partDescription = string.Empty;

                //        if (!string.IsNullOrEmpty(form.EQUIPMENTUNIQUEID))
                //        {
                //            var equipment = db.EQUIPMENT.FirstOrDefault(x => x.UNIQUEID == form.EQUIPMENTUNIQUEID);

                //            if (equipment != null)
                //            {
                //                equipmentID = equipment.ID;
                //                equipmentName = equipment.NAME;

                //                if (!string.IsNullOrEmpty(form.PARTUNIQUEID))
                //                {
                //                    var part = db.EQUIPMENTPART.FirstOrDefault(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.UNIQUEID == form.PARTUNIQUEID);

                //                    if (part != null)
                //                    {
                //                        partDescription = part.DESCRIPTION;
                //                    }
                //                }
                //            }
                //        }

                //        var subject = string.Format("[{0}][{1}][{2}]", Resources.Resource.RFormJobRefuseNotify, repairFormType, form.VHNO);

                //        StringBuilder sb = new StringBuilder();

                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.VHNO, form.VHNO));
                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.RepairFormType, repairFormType));
                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Subject, form.SUBJECT));
                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Description, form.DESCRIPTION));

                //        if (!string.IsNullOrEmpty(equipmentID))
                //        {
                //            if (!string.IsNullOrEmpty(partDescription))
                //            {
                //                sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Equipment, string.Format("{0}/{1}-{2}", equipmentID, equipmentName, partDescription)));
                //            }
                //            else
                //            {
                //                sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Equipment, string.Format("{0}/{1}", equipmentID, equipmentName)));
                //            }
                //        }

                //        var createUser = UserDataAccessor.GetUser(form.CREATEUSERID);
                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.CreateUser, createUser.User));

                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.CreateTime, DateTimeHelper.DateTime2DateTimeStringWithSeperator(form.CREATETIME)));
                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.JobRefuseReason, form.JOBREFUSEREASON));

                //        MailHelper.SendMail(mailAddressList, subject, sb.ToString());
                //    }
                //}
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
                //var mailAddressList = new List<MailAddress>();

                //using (ASEDbEntities db = new ASEDbEntities())
                //{
                //    var form = db.RFORM.First(x => x.UNIQUEID == RFormUniqueID);

                //    var flowLog = (from f in db.RFORMFLOW
                //                   join l in db.RFORMFLOWLOG
                //                   on new { f.RFORMUNIQUEID, Seq = f.CURRENTSEQ.Value } equals new { l.RFORMUNIQUEID, Seq = l.SEQ }
                //                   where f.RFORMUNIQUEID == RFormUniqueID
                //                   select l).First();

                //    var user = db.ACCOUNT.FirstOrDefault(x => x.ID == flowLog.USERID);

                //    if (user != null && !string.IsNullOrEmpty(user.EMAIL))
                //    {
                //        mailAddressList.Add(new MailAddress(user.EMAIL, user.NAME));
                //    }

                //    if (mailAddressList.Count > 0)
                //    {
                //        var repairFormType = string.Empty;

                //        if (form.RFORMTYPEUNIQUEID == "M")
                //        {
                //            repairFormType = Resources.Resource.MaintenanceRoute;
                //        }
                //        else
                //        {
                //            var formType = db.RFORMTYPE.FirstOrDefault(x => x.UNIQUEID == form.RFORMTYPEUNIQUEID);

                //            if (formType != null)
                //            {
                //                repairFormType = formType.DESCRIPTION;
                //            }
                //            else
                //            {
                //                repairFormType = "-";
                //            }
                //        }

                //        var equipmentID = string.Empty;
                //        var equipmentName = string.Empty;
                //        var partDescription = string.Empty;

                //        if (!string.IsNullOrEmpty(form.EQUIPMENTUNIQUEID))
                //        {
                //            var equipment = db.EQUIPMENT.FirstOrDefault(x => x.UNIQUEID == form.EQUIPMENTUNIQUEID);

                //            if (equipment != null)
                //            {
                //                equipmentID = equipment.ID;
                //                equipmentName = equipment.NAME;

                //                if (!string.IsNullOrEmpty(form.PARTUNIQUEID))
                //                {
                //                    var part = db.EQUIPMENTPART.FirstOrDefault(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.UNIQUEID == form.PARTUNIQUEID);

                //                    if (part != null)
                //                    {
                //                        partDescription = part.DESCRIPTION;
                //                    }
                //                }
                //            }
                //        }

                //        var subject = string.Format("[{0}][{1}][{2}]", Resources.Resource.RFormApproveNotify, repairFormType, form.VHNO);

                //        StringBuilder sb = new StringBuilder();

                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.VHNO, form.VHNO));
                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.RepairFormType, repairFormType));
                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Subject, form.SUBJECT));
                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Description, form.DESCRIPTION));

                //        if (!string.IsNullOrEmpty(equipmentID))
                //        {
                //            if (!string.IsNullOrEmpty(partDescription))
                //            {
                //                sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Equipment, string.Format("{0}/{1}-{2}", equipmentID, equipmentName, partDescription)));
                //            }
                //            else
                //            {
                //                sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Equipment, string.Format("{0}/{1}", equipmentID, equipmentName)));
                //            }
                //        }

                //        MailHelper.SendMail(mailAddressList, subject, sb.ToString());
                //    }
                //}
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
                //var mailAddressList = new List<MailAddress>();

                //using (ASEDbEntities db = new ASEDbEntities())
                //{
                //    var form = db.RFORM.First(x => x.UNIQUEID == RFormUniqueID);

                //    var currentFlowLog = db.RFORMFLOWLOG.Where(x => x.RFORMUNIQUEID == RFormUniqueID).OrderByDescending(x => x.SEQ).First();

                //    var flowUserList = db.RFORMFLOWLOG.Where(x => x.RFORMUNIQUEID == RFormUniqueID && x.SEQ != currentFlowLog.SEQ).Select(x => x.USERID).Distinct().ToList();

                //    var user = db.ACCOUNT.FirstOrDefault(x => x.ID == form.TAKEJOBUSERID);

                //    if (user != null && !string.IsNullOrEmpty(user.EMAIL))
                //    {
                //        mailAddressList.Add(new MailAddress(user.EMAIL, user.NAME));
                //    }

                //    foreach (var flowUser in flowUserList)
                //    {
                //        user = db.ACCOUNT.FirstOrDefault(x => x.ID == flowUser);

                //        if (user != null && !string.IsNullOrEmpty(user.EMAIL))
                //        {
                //            mailAddressList.Add(new MailAddress(user.EMAIL, user.NAME));
                //        }
                //    }

                //    if (mailAddressList.Count > 0)
                //    {
                //        var repairFormType = string.Empty;

                //        if (form.RFORMTYPEUNIQUEID == "M")
                //        {
                //            repairFormType = Resources.Resource.MaintenanceRoute;
                //        }
                //        else
                //        {
                //            var formType = db.RFORMTYPE.FirstOrDefault(x => x.UNIQUEID == form.RFORMTYPEUNIQUEID);

                //            if (formType != null)
                //            {
                //                repairFormType = formType.DESCRIPTION;
                //            }
                //            else
                //            {
                //                repairFormType = "-";
                //            }
                //        }

                //        var equipmentID = string.Empty;
                //        var equipmentName = string.Empty;
                //        var partDescription = string.Empty;

                //        if (!string.IsNullOrEmpty(form.EQUIPMENTUNIQUEID))
                //        {
                //            var equipment = db.EQUIPMENT.FirstOrDefault(x => x.UNIQUEID == form.EQUIPMENTUNIQUEID);

                //            if (equipment != null)
                //            {
                //                equipmentID = equipment.ID;
                //                equipmentName = equipment.NAME;

                //                if (!string.IsNullOrEmpty(form.PARTUNIQUEID))
                //                {
                //                    var part = db.EQUIPMENTPART.FirstOrDefault(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.UNIQUEID == form.PARTUNIQUEID);

                //                    if (part != null)
                //                    {
                //                        partDescription = part.DESCRIPTION;
                //                    }
                //                }
                //            }
                //        }

                //        var subject = string.Format("[{0}][{1}][{2}]", Resources.Resource.RFormRejectNotify, repairFormType, form.VHNO);

                //        StringBuilder sb = new StringBuilder();

                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.VHNO, form.VHNO));
                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.RepairFormType, repairFormType));
                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Subject, form.SUBJECT));
                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Description, form.DESCRIPTION));

                //        if (!string.IsNullOrEmpty(equipmentID))
                //        {
                //            if (!string.IsNullOrEmpty(partDescription))
                //            {
                //                sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Equipment, string.Format("{0}/{1}-{2}", equipmentID, equipmentName, partDescription)));
                //            }
                //            else
                //            {
                //                sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.Equipment, string.Format("{0}/{1}", equipmentID, equipmentName)));
                //            }
                //        }

                //        sb.Append(string.Format("<p>{0}：{1}</p>", Resources.Resource.VerifyComment, currentFlowLog.REMARK));

                //        MailHelper.SendMail(mailAddressList, subject, sb.ToString());
                //    }
                //}
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var file = db.RFORMFILE.First(x => x.RFORMUNIQUEID == RFormUniqueID && x.SEQ == Seq);

                    model = new FileDownloadModel()
                    {
                        FormUniqueID = file.RFORMUNIQUEID,
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
    }
}
