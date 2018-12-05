using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using Models.FlowManagement;
using Models.Authenticated;
using System.Text;
using Models.Shared;
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess
{
    public class FlowDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (DbEntities db = new DbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = (from f in db.Flow
                                 join o in db.Organization
                                 on f.OrganizationUniqueID equals o.UniqueID
                                 where downStreamOrganizationList.Contains(f.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(f.OrganizationUniqueID)
                                 select new
                                 {
                                     UniqueID = f.UniqueID,
                                     f.OrganizationUniqueID,
                                     OrganizationDescription = o.Description,
                                     Description = f.Description
                                 }).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.Description.Contains(Parameters.Keyword));
                    }

                    var organization = OrganizationDataAccessor.GetOrganization(Parameters.OrganizationUniqueID);

                    var itemList = query.ToList();

                    result.ReturnData(new GridViewModel()
                    {
                        Permission = Account.OrganizationPermission(Parameters.OrganizationUniqueID),
                        OrganizationUniqueID = Parameters.OrganizationUniqueID,
                        OrganizationDescription = organization.Description,
                        FullOrganizationDescription = organization.FullDescription,
                        ItemList = itemList.Select(x => new GridItem()
                        {
                            UniqueID = x.UniqueID,
                            Permission = Account.OrganizationPermission(x.OrganizationUniqueID),
                            OrganizationDescription = x.OrganizationDescription,
                            Description = x.Description
                        }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.Description).ToList()
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

        public static RequestResult GetDetailViewModel(string UniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (DbEntities db = new DbEntities())
                {
                    var query = db.Flow.First(x => x.UniqueID == UniqueID);

                    var model = new DetailViewModel()
                    {
                        UniqueID = query.UniqueID,
                        Permission = Account.OrganizationPermission(query.OrganizationUniqueID),
                        OrganizationUniqueID = query.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(query.OrganizationUniqueID),
                        Description = query.Description,
                        VerifyOrganizationList = (from x in db.FlowVerifyOrganization
                                                  join o in db.Organization
                                                  on x.OrganizationUniqueID equals o.UniqueID
                                                  //join u in db.User
                                                  //on o.ManagerUserID equals u.ID into tmpManager
                                                  //from manager in tmpManager
                                                  where x.FlowUniqueID == query.UniqueID
                                                  select new VerifyOrganizationModel
                                                  {
                                                      UniqueID = x.OrganizationUniqueID,
                                                      Description = o.Description,
                                                      //ManagerID = o.ManagerUserID,
                                                      //ManagerName = manager != null ? manager.Name : string.Empty,
                                                      Seq = x.Seq
                                                  }).OrderBy(x => x.Seq).ToList()
                    };

                    foreach (var organization in model.VerifyOrganizationList)
                    {
                        var organizationManagers = (from x in db.OrganizationManager
                                                    join u in db.User
                                                    on x.UserID equals u.ID
                                                    where x.OrganizationUniqueID == organization.UniqueID
                                                    select u).ToList();

                        foreach(var manager in organizationManagers)
                        {
                            organization.ManagerList.Add(UserDataAccessor.GetUser(manager.ID));
                        }
                    }

                    var flowFormList = db.FlowForm.Where(x => x.FlowUniqueID == UniqueID).ToList();

                    foreach (var flowForm in flowFormList)
                    {
                        switch (Define.EnumParse<Define.EnumForm>(flowForm.Form))
                        {
                            case Define.EnumForm.EquipmentPatrolResult:
                                model.FormDescriptionList.Add(Resources.Resource.EquipmentPatrolResult);
                                break;
                            case Define.EnumForm.MaintenanceForm:
                                model.FormDescriptionList.Add(Resources.Resource.MaintenanceForm);
                                break;
                            case Define.EnumForm.RepairForm:
                                using (EDbEntities edb = new EDbEntities())
                                {
                                    var formType = edb.RFormType.First(x => x.UniqueID == flowForm.RFormTypeUniqueID);

                                    model.FormDescriptionList.Add(string.Format("{0}({1})", Resources.Resource.RepairForm, formType.Description));
                                }
                                break;
                        }
                    }

                    model.FormDescriptionList = model.FormDescriptionList.OrderBy(x => x).ToList();

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

        public static RequestResult GetCreateFormModel(string OrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                result = GetFormList();

                if (result.IsSuccess)
                {
                    using (DbEntities db = new DbEntities())
                    {
                        if (OrganizationUniqueID == "*")
                        {
                            result.ReturnData(new CreateFormModel()
                            {
                                AncestorOrganizationUniqueID = "*",
                                OrganizationUniqueID = OrganizationUniqueID,
                                ParentOrganizationFullDescription = "*",
                                FormList = result.Data as List<FormModel>
                            });
                        }
                        else
                        {
                            var organization = OrganizationDataAccessor.GetOrganization(OrganizationUniqueID);

                            result.ReturnData(new CreateFormModel()
                            {
                                AncestorOrganizationUniqueID = organization.AncestorOrganizationUniqueID,
                                OrganizationUniqueID = OrganizationUniqueID,
                                ParentOrganizationFullDescription = organization.FullDescription,
                                FormList = result.Data as List<FormModel>
                            });
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

        private static RequestResult GetFormList()
        {
            RequestResult result = new RequestResult();

            try
            {
                var formList = new List<FormModel>();

                formList.Add(new FormModel()
                {
                    Form = Define.EnumForm.EquipmentPatrolResult,
                    FormDescription = Resources.Resource.EquipmentPatrolResult,
                    RepairFormTypeUniqueID = "-"
                });

                formList.Add(new FormModel()
                {
                    Form = Define.EnumForm.MaintenanceForm,
                    FormDescription = Resources.Resource.MaintenanceForm,
                    RepairFormTypeUniqueID = "-"
                });

                using (EDbEntities db = new EDbEntities())
                {
                    var formTypeList = db.RFormType.ToList();

                    foreach (var formType in formTypeList)
                    {
                        formList.Add(new FormModel()
                        {
                            Form = Define.EnumForm.RepairForm,
                            FormDescription = Resources.Resource.RepairForm,
                            RepairFormTypeUniqueID = formType.UniqueID,
                            RepairFormTypeDescription = formType.Description
                        });
                    }
                }

                result.ReturnData(formList.OrderBy(x => x.Description).ToList());
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
                using (DbEntities db = new DbEntities())
                {
                    var exists = db.Flow.FirstOrDefault(x => x.OrganizationUniqueID == Model.OrganizationUniqueID && x.Description == Model.FormInput.Description);

                    if (exists == null)
                    {
                        var managerNotExistOrganizationList = Model.VerifyOrganizationList.Where(x => !x.IsManagerExist).ToList();

                        if (managerNotExistOrganizationList.Count == 0)
                        {
                            string uniqueID = Guid.NewGuid().ToString();

                            db.Flow.Add(new Flow()
                            {
                                UniqueID = uniqueID,
                                OrganizationUniqueID = Model.OrganizationUniqueID,
                                Description = Model.FormInput.Description
                            });

                            foreach (var verifyOrganization in Model.VerifyOrganizationList)
                            {
                                db.FlowVerifyOrganization.Add(new FlowVerifyOrganization()
                                {
                                    FlowUniqueID = uniqueID,
                                    OrganizationUniqueID = verifyOrganization.UniqueID,
                                    Seq = verifyOrganization.Seq
                                });
                            }

                            foreach (var form in Model.FormInput.FormList)
                            {
                                var temp = (from f in db.Flow
                                            join x in db.FlowForm
                                            on f.UniqueID equals x.FlowUniqueID
                                            where f.OrganizationUniqueID == Model.OrganizationUniqueID && x.Form == form.Form.ToString() && x.RFormTypeUniqueID == form.RepairFormTypeUniqueID
                                            select x).FirstOrDefault();

                                if (temp != null)
                                {
                                    db.FlowForm.Remove(temp);
                                }

                                db.FlowForm.Add(new DbEntity.MSSQL.FlowForm()
                                {
                                    FlowUniqueID = uniqueID,
                                    Form = form.Form.ToString(),
                                    RFormTypeUniqueID = form.RepairFormTypeUniqueID
                                });
                            }

                            db.SaveChanges();

                            result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.Flow, Resources.Resource.Success));
                        }
                        else
                        {
                            StringBuilder sb = new StringBuilder();

                            foreach (var organization in managerNotExistOrganizationList)
                            {
                                sb.Append(string.Format("{0} {1} {2}", organization.Description, Resources.Resource.NotSet, Resources.Resource.Manager));
                                sb.Append("、");
                            }

                            sb.Remove(sb.Length - 1, 1);

                            result.ReturnFailedMessage(sb.ToString());
                        }
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.FlowDescription, Resources.Resource.Exists));
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

        public static RequestResult GetCopyFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                result = GetFormList();

                if (result.IsSuccess)
                {
                    using (DbEntities db = new DbEntities())
                    {
                        var flow = db.Flow.First(x => x.UniqueID == UniqueID);

                        var model = new CreateFormModel()
                        {
                            ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(flow.OrganizationUniqueID),
                            OrganizationUniqueID = flow.OrganizationUniqueID,
                            FormList = result.Data as List<FormModel>,
                            FlowFormList = db.FlowForm.Where(x => x.FlowUniqueID == UniqueID).Select(x => new Models.FlowManagement.FlowForm { FlowUniqueID = x.FlowUniqueID, Form = x.Form, RFormTypeUniqueID = x.RFormTypeUniqueID }).ToList(),
                            VerifyOrganizationList = (from x in db.FlowVerifyOrganization
                                                      join o in db.Organization
                                                      on x.OrganizationUniqueID equals o.UniqueID
                                                      //join u in db.User
                                                      //on o.ManagerUserID equals u.ID into tmpManager
                                                      //from manager in tmpManager
                                                      where x.FlowUniqueID == flow.UniqueID
                                                      select new VerifyOrganizationModel
                                                      {
                                                          UniqueID = x.OrganizationUniqueID,
                                                          Description = o.Description,
                                                          //ManagerID = o.ManagerUserID,
                                                          //ManagerName = manager != null ? manager.Name : string.Empty,
                                                          Seq = x.Seq
                                                      }).OrderBy(x => x.Seq).ToList()
                        };

                        foreach (var organization in model.VerifyOrganizationList)
                        {
                            var organizationManagers = (from x in db.OrganizationManager
                                                        join u in db.User
                                                        on x.UserID equals u.ID
                                                        where x.OrganizationUniqueID == organization.UniqueID
                                                        select u).ToList();

                            foreach (var manager in organizationManagers)
                            {
                                organization.ManagerList.Add(UserDataAccessor.GetUser(manager.ID));
                            }
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

        public static RequestResult GetEditFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                result = GetFormList();

                if (result.IsSuccess)
                {
                    using (DbEntities db = new DbEntities())
                    {
                        var flow = db.Flow.First(x => x.UniqueID == UniqueID);

                        var organization = OrganizationDataAccessor.GetOrganization(flow.OrganizationUniqueID);

                        var model = new EditFormModel()
                        {
                            AncestorOrganizationUniqueID = organization.AncestorOrganizationUniqueID,
                            ParentOrganizationFullDescription = organization.FullDescription,
                            UniqueID = flow.UniqueID,
                            FlowFormList = db.FlowForm.Where(x => x.FlowUniqueID == UniqueID).Select(x => new Models.FlowManagement.FlowForm { FlowUniqueID = x.FlowUniqueID, Form = x.Form, RFormTypeUniqueID = x.RFormTypeUniqueID }).ToList(),
                            VerifyOrganizationList = (from x in db.FlowVerifyOrganization
                                                      join o in db.Organization
                                                      on x.OrganizationUniqueID equals o.UniqueID
                                                      //join u in db.User
                                                      //on o.ManagerUserID equals u.ID into tmpManager
                                                      //from manager in tmpManager
                                                      where x.FlowUniqueID == flow.UniqueID
                                                      select new VerifyOrganizationModel
                                                      {
                                                          UniqueID = x.OrganizationUniqueID,
                                                          Description = o.Description,
                                                          //ManagerID = o.ManagerUserID,
                                                          //ManagerName = manager != null ? manager.Name : string.Empty,
                                                          Seq = x.Seq
                                                      }).OrderBy(x => x.Seq).ToList(),
                            FormList = result.Data as List<FormModel>,
                            FormInput = new FormInput()
                            {
                                Description = flow.Description
                            }
                        };

                        foreach (var org in model.VerifyOrganizationList)
                        {
                            var organizationManagers = (from x in db.OrganizationManager
                                                        join u in db.User
                                                        on x.UserID equals u.ID
                                                        where x.OrganizationUniqueID == org.UniqueID
                                                        select u).ToList();

                            foreach (var manager in organizationManagers)
                            {
                                org.ManagerList.Add(UserDataAccessor.GetUser(manager.ID));
                            }
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
                using (DbEntities db = new DbEntities())
                {
                    var flow = db.Flow.First(x => x.UniqueID == Model.UniqueID);

                    var exists = db.Flow.FirstOrDefault(x => x.UniqueID != flow.UniqueID && x.OrganizationUniqueID == flow.OrganizationUniqueID && x.Description == Model.FormInput.Description);

                    if (exists == null)
                    {
                        var managerNotExistOrganizationList = Model.VerifyOrganizationList.Where(x => !x.IsManagerExist).ToList();

                        if (managerNotExistOrganizationList.Count == 0)
                        {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                            flow.Description = Model.FormInput.Description;

                            db.SaveChanges();

                            db.FlowVerifyOrganization.RemoveRange(db.FlowVerifyOrganization.Where(x => x.FlowUniqueID == flow.UniqueID).ToList());

                            db.SaveChanges();

                            foreach (var verifyOrganization in Model.VerifyOrganizationList)
                            {
                                db.FlowVerifyOrganization.Add(new FlowVerifyOrganization()
                                {
                                    FlowUniqueID = flow.UniqueID,
                                    OrganizationUniqueID = verifyOrganization.UniqueID,
                                    Seq = verifyOrganization.Seq
                                });
                            }

                            db.FlowForm.RemoveRange(db.FlowForm.Where(x => x.FlowUniqueID == flow.UniqueID).ToList());

                            db.SaveChanges();

                            foreach (var form in Model.FormInput.FormList)
                            {
                                var temp = (from f in db.Flow
                                            join x in db.FlowForm
                                            on f.UniqueID equals x.FlowUniqueID
                                            where f.OrganizationUniqueID == flow.OrganizationUniqueID && x.Form == form.Form.ToString() && x.RFormTypeUniqueID == form.RepairFormTypeUniqueID
                                            select x).FirstOrDefault();

                                if (temp != null)
                                {
                                    db.FlowForm.Remove(temp);
                                }

                                db.FlowForm.Add(new DbEntity.MSSQL.FlowForm()
                                {
                                    FlowUniqueID = flow.UniqueID,
                                    Form = form.Form.ToString(),
                                    RFormTypeUniqueID = form.RepairFormTypeUniqueID
                                });
                            }

                            db.SaveChanges();

#if !DEBUG
                        trans.Complete();
                    }
#endif
                        }
                        else
                        {
                            StringBuilder sb = new StringBuilder();

                            foreach (var organization in managerNotExistOrganizationList)
                            {
                                sb.Append(string.Format("{0} {1} {2}", organization.Description, Resources.Resource.NotSet, Resources.Resource.Manager));
                                sb.Append("、");
                            }

                            sb.Remove(sb.Length - 1, 1);

                            result.ReturnFailedMessage(sb.ToString());
                        }

                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.Flow, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.FlowDescription, Resources.Resource.Exists));
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

        public static RequestResult SavePageState(List<string> PageStateList)
        {
            RequestResult result = new RequestResult();

            try
            {
                var verifyOrganizationList = new List<VerifyOrganizationModel>();

                using (DbEntities db = new DbEntities())
                {
                    foreach (string pageState in PageStateList)
                    {
                        string[] temp = pageState.Split(Define.Seperators, StringSplitOptions.None);

                        string organizationUniqueID = temp[0];
                        int seq = int.Parse(temp[1]);

                        var query = (from o in db.Organization
                                     //join u in db.User
                                     //on o.ManagerUserID equals u.ID into tmpManager
                                     //from manager in tmpManager.DefaultIfEmpty()
                                     where o.UniqueID == organizationUniqueID
                                     select new
                                     {
                                         Organization = o,
                                         //ManagerName = manager != null ? manager.Name : string.Empty
                                     }).First();

                        var model = new VerifyOrganizationModel()
                        {
                            UniqueID = organizationUniqueID,
                            Description = query.Organization.Description,
                            //ManagerID = query.Organization.ManagerUserID,
                            //ManagerName = query.ManagerName,
                            Seq = seq
                        };

                        var organizationManagers = (from x in db.OrganizationManager
                                                    join u in db.User
                                                    on x.UserID equals u.ID
                                                    where x.OrganizationUniqueID == query.Organization.UniqueID
                                                    select u).ToList();

                        foreach (var manager in organizationManagers)
                        {
                            model.ManagerList.Add(UserDataAccessor.GetUser(manager.ID));
                        }

                        verifyOrganizationList.Add(model);
                    }
                }

                result.ReturnData(verifyOrganizationList.OrderBy(x => x.Seq).ToList());
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult Delete(List<string> SelectedList)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (DbEntities db = new DbEntities())
                {
                    foreach (var uniqueID in SelectedList)
                    {
                        db.Flow.Remove(db.Flow.First(x => x.UniqueID == uniqueID));

                        db.FlowForm.RemoveRange(db.FlowForm.Where(x => x.FlowUniqueID == uniqueID).ToList());

                        db.FlowVerifyOrganization.RemoveRange(db.FlowVerifyOrganization.Where(x => x.FlowUniqueID == uniqueID).ToList());
                    }

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.Flow, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult AddOrganization(List<VerifyOrganizationModel> VerifyOrganizationList, List<string> SelectedList)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (DbEntities db = new DbEntities())
                {
                    foreach (var selected in SelectedList)
                    {
                        var query = (from o in db.Organization
                                     //join u in db.User
                                     //on o.ManagerUserID equals u.ID into tmpManager
                                     //from manager in tmpManager.DefaultIfEmpty()
                                     where o.UniqueID == selected
                                     select new
                                     {
                                         Organization = o,
                                         //ManagerName = manager != null ? manager.Name : string.Empty
                                     }).First();

                        var model = new VerifyOrganizationModel()
                        {
                            UniqueID = query.Organization.UniqueID,
                            Description = query.Organization.Description,
                            //ManagerID = query.Organization.ManagerUserID,
                            //ManagerName = query.ManagerName,
                            Seq = VerifyOrganizationList.Count + 1
                        };

                        var organizationManagers = (from x in db.OrganizationManager
                                                    join u in db.User
                                                    on x.UserID equals u.ID
                                                    where x.OrganizationUniqueID == query.Organization.UniqueID
                                                    select u).ToList();

                        foreach (var manager in organizationManagers)
                        {
                            model.ManagerList.Add(UserDataAccessor.GetUser(manager.ID));
                        }

                        VerifyOrganizationList.Add(model);
                    }
                }

                result.ReturnData(VerifyOrganizationList.OrderBy(x => x.Seq).ToList());
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetRootTreeItem(List<Models.Shared.Organization> OrganizationList, string OrganizationUniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                var attributes = new Dictionary<Utility.Define.EnumTreeAttribute, string>() 
                { 
                    { Utility.Define.EnumTreeAttribute.NodeType, string.Empty },
                    { Utility.Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Utility.Define.EnumTreeAttribute.OrganizationUniqueID, string.Empty },
                    { Utility.Define.EnumTreeAttribute.FlowUniqueID, string.Empty }
                };

                using (DbEntities db = new DbEntities())
                {
                    var organization = db.Organization.First(x => x.UniqueID == OrganizationUniqueID);
                    
                    var treeItem = new TreeItem() { Title = organization.Description };

                    attributes[Utility.Define.EnumTreeAttribute.NodeType] = Utility.Define.EnumTreeNodeType.Organization.ToString();
                    attributes[Utility.Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                    attributes[Utility.Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                    attributes[Utility.Define.EnumTreeAttribute.FlowUniqueID] = string.Empty;

                    foreach (var attribute in attributes)
                    {
                        treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                    }

                    var haveDownStreamOrganization = OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID));

                    var haveFlow = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.Flow.Any(x => x.OrganizationUniqueID == organization.UniqueID);

                    if (haveDownStreamOrganization || haveFlow)
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

        public static RequestResult GetTreeItem(List<Models.Shared.Organization> OrganizationList, string OrganizationUniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                var attributes = new Dictionary<Utility.Define.EnumTreeAttribute, string>() 
                { 
                    { Utility.Define.EnumTreeAttribute.NodeType, string.Empty },
                    { Utility.Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Utility.Define.EnumTreeAttribute.OrganizationUniqueID, string.Empty },
                    { Utility.Define.EnumTreeAttribute.FlowUniqueID, string.Empty }
                };

                using (DbEntities db = new DbEntities())
                {
                    if (Account.QueryableOrganizationUniqueIDList.Any(x => x == OrganizationUniqueID))
                    {
                        var flowList = db.Flow.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.Description).ToList();

                        foreach (var flow in flowList)
                        {
                            var treeItem = new TreeItem() { Title = flow.Description };

                            attributes[Utility.Define.EnumTreeAttribute.NodeType] = Utility.Define.EnumTreeNodeType.Flow.ToString();
                            attributes[Utility.Define.EnumTreeAttribute.ToolTip] = flow.Description;
                            attributes[Utility.Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Utility.Define.EnumTreeAttribute.FlowUniqueID] = flow.UniqueID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }
                    }

                    var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                    foreach (var organization in organizationList)
                    {
                        var treeItem = new TreeItem() { Title = organization.Description };

                        attributes[Utility.Define.EnumTreeAttribute.NodeType] = Utility.Define.EnumTreeNodeType.Organization.ToString();
                        attributes[Utility.Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                        attributes[Utility.Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                        attributes[Utility.Define.EnumTreeAttribute.FlowUniqueID] = string.Empty;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        var haveDownStreamOrganization = OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID));

                        var haveFlow = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.Flow.Any(x => x.OrganizationUniqueID == organization.UniqueID);

                        if (haveDownStreamOrganization || haveFlow)
                        {
                            treeItem.State = "closed";
                        }

                        treeItemList.Add(treeItem);
                    }
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
    }
}
