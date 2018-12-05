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
using DbEntity.ASE;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.ASE
{
    public class FlowDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = (from f in db.FLOW
                                 join o in db.ORGANIZATION
                                 on f.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                 where downStreamOrganizationList.Contains(f.ORGANIZATIONUNIQUEID) && Account.QueryableOrganizationUniqueIDList.Contains(f.ORGANIZATIONUNIQUEID)
                                 select new
                                 {
                                     UniqueID = f.UNIQUEID,
                                     f.ORGANIZATIONUNIQUEID,
                                     OrganizationDescription = o.DESCRIPTION,
                                     Description = f.DESCRIPTION
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
                            Permission = Account.OrganizationPermission(x.ORGANIZATIONUNIQUEID),
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = db.FLOW.First(x => x.UNIQUEID == UniqueID);

                    var model = new DetailViewModel()
                    {
                        UniqueID = query.UNIQUEID,
                        Permission = Account.OrganizationPermission(query.ORGANIZATIONUNIQUEID),
                        OrganizationUniqueID = query.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(query.ORGANIZATIONUNIQUEID),
                        Description = query.DESCRIPTION,
                        VerifyOrganizationList = (from x in db.FLOWVERIFYORGANIZATION
                                                  join o in db.ORGANIZATION
                                                  on x.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                                  join u in db.ACCOUNT
                                                  on o.MANAGERUSERID equals u.ID into tmpManager
                                                  from manager in tmpManager
                                                  where x.FLOWUNIQUEID == query.UNIQUEID
                                                  select new VerifyOrganizationModel
                                                  {
                                                      UniqueID = x.ORGANIZATIONUNIQUEID,
                                                      Description = o.DESCRIPTION,
                                                      ManagerID = o.MANAGERUSERID,
                                                      ManagerName = manager != null ? manager.NAME : string.Empty,
                                                      Seq = x.SEQ
                                                  }).OrderBy(x => x.Seq).ToList()
                    };

                    var flowFormList = db.FLOWFORM.Where(x => x.FLOWUNIQUEID == UniqueID).ToList();

                    foreach (var flowForm in flowFormList)
                    {
                        switch (Define.EnumParse<Define.EnumForm>(flowForm.FORM))
                        {
                            case Define.EnumForm.EquipmentPatrolResult:
                                model.FormDescriptionList.Add(Resources.Resource.EquipmentPatrolResult);
                                break;
                            case Define.EnumForm.MaintenanceForm:
                                model.FormDescriptionList.Add(Resources.Resource.MaintenanceForm);
                                break;
                            case Define.EnumForm.RepairForm:
                                var formType = db.RFORMTYPE.First(x => x.UNIQUEID == flowForm.RFORMTYPEUNIQUEID);

                                    model.FormDescriptionList.Add(string.Format("{0}({1})", Resources.Resource.RepairForm, formType.DESCRIPTION));
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
                    using (ASEDbEntities db = new ASEDbEntities())
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var formTypeList = db.RFORMTYPE.ToList();

                    foreach (var formType in formTypeList)
                    {
                        formList.Add(new FormModel()
                        {
                            Form = Define.EnumForm.RepairForm,
                            FormDescription = Resources.Resource.RepairForm,
                            RepairFormTypeUniqueID = formType.UNIQUEID,
                            RepairFormTypeDescription = formType.DESCRIPTION
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var exists = db.FLOW.FirstOrDefault(x => x.ORGANIZATIONUNIQUEID == Model.OrganizationUniqueID && x.DESCRIPTION == Model.FormInput.Description);

                    if (exists == null)
                    {
                        var managerNotExistOrganizationList = Model.VerifyOrganizationList.Where(x => !x.IsManagerExist).ToList();

                        if (managerNotExistOrganizationList.Count == 0)
                        {
                            string uniqueID = Guid.NewGuid().ToString();

                            db.FLOW.Add(new FLOW()
                            {
                                UNIQUEID = uniqueID,
                                ORGANIZATIONUNIQUEID = Model.OrganizationUniqueID,
                                DESCRIPTION = Model.FormInput.Description
                            });

                            foreach (var verifyOrganization in Model.VerifyOrganizationList)
                            {
                                db.FLOWVERIFYORGANIZATION.Add(new FLOWVERIFYORGANIZATION()
                                {
                                    FLOWUNIQUEID = uniqueID,
                                    ORGANIZATIONUNIQUEID = verifyOrganization.UniqueID,
                                    SEQ = verifyOrganization.Seq
                                });
                            }

                            foreach (var form in Model.FormInput.FormList)
                            {
                                var temp = (from f in db.FLOW
                                            join x in db.FLOWFORM
                                            on f.UNIQUEID equals x.FLOWUNIQUEID
                                            where f.ORGANIZATIONUNIQUEID == Model.OrganizationUniqueID && x.FORM == form.Form.ToString() && x.RFORMTYPEUNIQUEID == form.RepairFormTypeUniqueID
                                            select x).FirstOrDefault();

                                if (temp != null)
                                {
                                    db.FLOWFORM.Remove(temp);
                                }

                                db.FLOWFORM.Add(new DbEntity.ASE.FLOWFORM()
                                {
                                    FLOWUNIQUEID = uniqueID,
                                    FORM = form.Form.ToString(),
                                    RFORMTYPEUNIQUEID = form.RepairFormTypeUniqueID
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
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var flow = db.FLOW.First(x => x.UNIQUEID == UniqueID);

                        result.ReturnData(new CreateFormModel()
                        {
                            ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(flow.ORGANIZATIONUNIQUEID),
                            OrganizationUniqueID = flow.ORGANIZATIONUNIQUEID,
                            FormList = result.Data as List<FormModel>,
                            FlowFormList = db.FLOWFORM.Where(x => x.FLOWUNIQUEID == UniqueID).Select(x => new Models.FlowManagement.FlowForm { FlowUniqueID = x.FLOWUNIQUEID, Form = x.FORM, RFormTypeUniqueID = x.RFORMTYPEUNIQUEID }).ToList(),
                            VerifyOrganizationList = (from x in db.FLOWVERIFYORGANIZATION
                                                      join o in db.ORGANIZATION
                                                      on x.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                                      join u in db.ACCOUNT
                                                      on o.MANAGERUSERID equals u.ID into tmpManager
                                                      from manager in tmpManager
                                                      where x.FLOWUNIQUEID == flow.UNIQUEID
                                                      select new VerifyOrganizationModel
                                                      {
                                                          UniqueID = x.ORGANIZATIONUNIQUEID,
                                                          Description = o.DESCRIPTION,
                                                          ManagerID = o.MANAGERUSERID,
                                                          ManagerName = manager != null ? manager.NAME : string.Empty,
                                                          Seq = x.SEQ
                                                      }).OrderBy(x => x.Seq).ToList()
                        });
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
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var flow = db.FLOW.First(x => x.UNIQUEID == UniqueID);

                        var organization = OrganizationDataAccessor.GetOrganization(flow.ORGANIZATIONUNIQUEID);

                        result.ReturnData(new EditFormModel()
                        {
                            AncestorOrganizationUniqueID = organization.AncestorOrganizationUniqueID,
                            ParentOrganizationFullDescription = organization.FullDescription,
                            UniqueID = flow.UNIQUEID,
                            FlowFormList = db.FLOWFORM.Where(x => x.FLOWUNIQUEID == UniqueID).Select(x => new Models.FlowManagement.FlowForm { FlowUniqueID = x.FLOWUNIQUEID, Form = x.FORM, RFormTypeUniqueID = x.RFORMTYPEUNIQUEID }).ToList(),
                            VerifyOrganizationList = (from x in db.FLOWVERIFYORGANIZATION
                                                      join o in db.ORGANIZATION
                                                      on x.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                                      join u in db.ACCOUNT
                                                      on o.MANAGERUSERID equals u.ID into tmpManager
                                                      from manager in tmpManager
                                                      where x.FLOWUNIQUEID == flow.UNIQUEID
                                                      select new VerifyOrganizationModel
                                                      {
                                                          UniqueID = x.ORGANIZATIONUNIQUEID,
                                                          Description = o.DESCRIPTION,
                                                          ManagerID = o.MANAGERUSERID,
                                                          ManagerName = manager != null ? manager.NAME : string.Empty,
                                                          Seq = x.SEQ
                                                      }).OrderBy(x => x.Seq).ToList(),
                            FormList = result.Data as List<FormModel>,
                            FormInput = new FormInput()
                            {
                                Description = flow.DESCRIPTION
                            }
                        });
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var flow = db.FLOW.First(x => x.UNIQUEID == Model.UniqueID);

                    var exists = db.FLOW.FirstOrDefault(x => x.UNIQUEID != flow.UNIQUEID && x.ORGANIZATIONUNIQUEID == flow.ORGANIZATIONUNIQUEID && x.DESCRIPTION == Model.FormInput.Description);

                    if (exists == null)
                    {
                        var managerNotExistOrganizationList = Model.VerifyOrganizationList.Where(x => !x.IsManagerExist).ToList();

                        if (managerNotExistOrganizationList.Count == 0)
                        {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                            flow.DESCRIPTION = Model.FormInput.Description;

                            db.SaveChanges();

                            db.FLOWVERIFYORGANIZATION.RemoveRange(db.FLOWVERIFYORGANIZATION.Where(x => x.FLOWUNIQUEID == flow.UNIQUEID).ToList());

                            db.SaveChanges();

                            foreach (var verifyOrganization in Model.VerifyOrganizationList)
                            {
                                db.FLOWVERIFYORGANIZATION.Add(new FLOWVERIFYORGANIZATION()
                                {
                                    FLOWUNIQUEID = flow.UNIQUEID,
                                    ORGANIZATIONUNIQUEID = verifyOrganization.UniqueID,
                                    SEQ = verifyOrganization.Seq
                                });
                            }

                            db.FLOWFORM.RemoveRange(db.FLOWFORM.Where(x => x.FLOWUNIQUEID == flow.UNIQUEID).ToList());

                            db.SaveChanges();

                            foreach (var form in Model.FormInput.FormList)
                            {
                                var temp = (from f in db.FLOW
                                            join x in db.FLOWFORM
                                            on f.UNIQUEID equals x.FLOWUNIQUEID
                                            where f.ORGANIZATIONUNIQUEID == flow.ORGANIZATIONUNIQUEID && x.FORM == form.Form.ToString() && x.RFORMTYPEUNIQUEID == form.RepairFormTypeUniqueID
                                            select x).FirstOrDefault();

                                if (temp != null)
                                {
                                    db.FLOWFORM.Remove(temp);
                                }

                                db.FLOWFORM.Add(new DbEntity.ASE.FLOWFORM()
                                {
                                    FLOWUNIQUEID = flow.UNIQUEID,
                                    FORM = form.Form.ToString(),
                                    RFORMTYPEUNIQUEID = form.RepairFormTypeUniqueID
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    foreach (string pageState in PageStateList)
                    {
                        string[] temp = pageState.Split(Define.Seperators, StringSplitOptions.None);

                        string organizationUniqueID = temp[0];

                        int seq = int.Parse(temp[1]);

                        var organization = db.ORGANIZATION.First(x => x.UNIQUEID == organizationUniqueID);

                        var manager = db.ACCOUNT.FirstOrDefault(x => x.ID == organization.MANAGERUSERID);

                        verifyOrganizationList.Add(new VerifyOrganizationModel()
                        {
                            UniqueID = organizationUniqueID,
                            Description = organization.DESCRIPTION,
                            ManagerID = organization.MANAGERUSERID,
                            ManagerName = manager!=null?manager.NAME:string.Empty,
                            Seq = seq
                        });
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    foreach (var uniqueID in SelectedList)
                    {
                        db.FLOW.Remove(db.FLOW.First(x => x.UNIQUEID == uniqueID));

                        db.FLOWFORM.RemoveRange(db.FLOWFORM.Where(x => x.FLOWUNIQUEID == uniqueID).ToList());

                        db.FLOWVERIFYORGANIZATION.RemoveRange(db.FLOWVERIFYORGANIZATION.Where(x => x.FLOWUNIQUEID == uniqueID).ToList());
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    foreach (var selected in SelectedList)
                    {
                        var organization = db.ORGANIZATION.First(x => x.UNIQUEID == selected);

                        var manager = db.ACCOUNT.FirstOrDefault(x => x.ID == organization.MANAGERUSERID);

                        VerifyOrganizationList.Add(new VerifyOrganizationModel()
                        {
                            UniqueID = organization.UNIQUEID,
                            Description = organization.DESCRIPTION,
                            ManagerID = organization.MANAGERUSERID,
                            ManagerName = manager!=null?manager.NAME:string.Empty,
                            Seq = VerifyOrganizationList.Count + 1
                        });
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

        public static RequestResult GetRootTreeItem(List<Models.Shared.Organization> OrganizationList, string AncestorOrganizationUniqueID, Account Account)
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var organization = OrganizationList.First(x => x.UniqueID == AncestorOrganizationUniqueID);

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

                    var haveFlow = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.FLOW.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID);

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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (Account.QueryableOrganizationUniqueIDList.Any(x => x == OrganizationUniqueID))
                    {
                        var flowList = db.FLOW.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).OrderBy(x => x.DESCRIPTION).ToList();

                        foreach (var flow in flowList)
                        {
                            var treeItem = new TreeItem() { Title = flow.DESCRIPTION };

                            attributes[Utility.Define.EnumTreeAttribute.NodeType] = Utility.Define.EnumTreeNodeType.Flow.ToString();
                            attributes[Utility.Define.EnumTreeAttribute.ToolTip] = flow.DESCRIPTION;
                            attributes[Utility.Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Utility.Define.EnumTreeAttribute.FlowUniqueID] = flow.UNIQUEID;

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

                        var haveFlow = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.FLOW.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID);

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
