using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using Models.Shared;
using Models.Authenticated;
using Models.OrganizationManagement;
using DbEntity.ASE;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.ASE
{
    public class OrganizationDataAccessor
    {
        public static List<Models.Shared.Organization> GetAllOrganization()
        {
            var organizationList = new List<Models.Shared.Organization>();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    organizationList = db.ORGANIZATION.Select(x => new Models.Shared.Organization
                    {
                        UniqueID = x.UNIQUEID,
                        ParentUniqueID = x.PARENTUNIQUEID,
                        ID = x.ID,
                        Description = x.DESCRIPTION
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                organizationList = new List<Models.Shared.Organization>();

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return organizationList;
        }

        public static RequestResult GetDetailViewModel(string UniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var organization = (from o in db.ORGANIZATION
                                        where o.UNIQUEID == UniqueID
                                        select new
                                        {
                                            o.UNIQUEID,
                                            o.PARENTUNIQUEID,
                                            o.ID,
                                            o.DESCRIPTION,
                                            o.MANAGERUSERID
                                        }).First();

                    var manager = db.ACCOUNT.FirstOrDefault(x => x.ID == organization.MANAGERUSERID);

                    var model = new DetailViewModel()
                    {
                        UniqueID = organization.UNIQUEID,
                        Permission = Account.OrganizationPermission(UniqueID),
                        ParentOrganizationFullDescription = GetOrganizationFullDescription(organization.PARENTUNIQUEID),
                        ID = organization.ID,
                        Description = organization.DESCRIPTION,
                        //ManagerUserID = organization.MANAGERUSERID,
                        //ManagerName = manager != null ? manager.NAME : string.Empty,
                        EditableOrganizationList = GetEditableOrganizationList(UniqueID).Union(GetDownStreamOrganizationList(UniqueID, true)).Select(x => GetOrganizationFullDescription(x)).OrderBy(x => x).ToList(),
                        QueryableOrganizationList = GetQueryableOrganizationList(UniqueID).Select(x => GetOrganizationFullDescription(x)).OrderBy(x => x).ToList(),
                        //ManagerList = (from x in db.ORGANIZATIONMANAGER
                        //               join a in db.ACCOUNT
                        //               on x.USERID equals a.ID
                        //               where x.ORGANIZATIONUNIQUEID == UniqueID
                        //               select new UserModel
                        //               {
                        //                   ID = a.ID,
                        //                   Name = a.Name
                        //               }).OrderBy(x => x.ID).ToList()
                    };

                    model.ManagerList.Add(new UserModel()
                    {
                        ID = organization.MANAGERUSERID,
                        Name = manager != null ? manager.NAME : string.Empty
                    });

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

        public static RequestResult Create(CreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var exists = db.ORGANIZATION.FirstOrDefault(x => x.PARENTUNIQUEID == Model.ParentUniqueID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
                        string uniqueID = Guid.NewGuid().ToString();

                        var organization = new ORGANIZATION()
                        {
                            UNIQUEID = uniqueID,
                            PARENTUNIQUEID = Model.ParentUniqueID,
                            ID = Model.FormInput.ID,
                            DESCRIPTION = Model.FormInput.Description,
                            HR = "N"
                            //MANAGERUSERID = Model.FormInput.ManagerUserID
                        };

                        if (!string.IsNullOrEmpty(Model.FormInput.Managers))
                        {
                            organization.MANAGERUSERID = Model.FormInput.Managers.Split(',').ToList()[0];

                            //db.ORGANIZATIONMANAGER.AddRange(Model.FormInput.Managers.Split(',').ToList().Select(x => new ORGANIZATIONMANAGER()
                            //{
                            //    ORGANIZATIONUNIQUEID = uniqueID,
                            //    USERID = x
                            //}).ToList());
                        }

                        db.ORGANIZATION.Add(organization);

                        db.EDITABLEORGANIZATION.AddRange(Model.EditableOrganizationList.Where(x => x.CanDelete).Select(x => new EDITABLEORGANIZATION
                        {
                            ORGANIZATIONUNIQUEID = uniqueID,
                            EDITABLEORGANIZATIONUNIQUEID = x.UniqueID
                        }).ToList());

                        db.QUERYABLEORGANIZATION.AddRange(Model.QueryableOrganizationList.Select(x => new QUERYABLEORGANIZATION
                        {
                            ORGANIZATIONUNIQUEID = uniqueID,
                            QUERYABLEORGANIZATIONUNIQUEID = x.UniqueID
                        }).ToList());

                        db.SaveChanges();

                        result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.Organization, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.OrganizationID, Resources.Resource.Exists));
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
                    var organization = (from o in db.ORGANIZATION
                                        where o.UNIQUEID == UniqueID
                                        select new
                                        {
                                            o.UNIQUEID,
                                            o.PARENTUNIQUEID,
                                            o.ID,
                                            o.DESCRIPTION,
                                            o.MANAGERUSERID
                                        }).First();

                   //var manager = db.ACCOUNT.FirstOrDefault(x => x.ID == organization.MANAGERUSERID);

                   var model = new EditFormModel()
                   {
                       UniqueID = organization.UNIQUEID,
                       AncestorOrganizationUniqueID = GetAncestorOrganizationUniqueID(organization.UNIQUEID),
                       ParentOrganizationFullDescription = GetOrganizationFullDescription(organization.PARENTUNIQUEID),
                       //ManagerName = manager != null ? manager.NAME : string.Empty,
                       FormInput = new FormInput()
                       {
                           ID = organization.ID,
                           Description = organization.DESCRIPTION,
                           //ManagerUserID = organization.MANAGERUSERID
                       },
                       //ManagerList = db.ORGANIZATIONMANAGER.Where(x => x.ORGANIZATIONUNIQUEID == UniqueID).Select(x => x.USERID).ToList(),
                       EditableOrganizationList = GetEditableOrganizationList(UniqueID).Select(x => new EditableOrganizationModel
                       {
                           CanDelete = true,
                           UniqueID = x,
                           FullDescription = GetOrganizationFullDescription(x)
                       }).Union(GetDownStreamOrganizationList(UniqueID, true).Select(x => new EditableOrganizationModel
                       {
                           CanDelete = false,
                           UniqueID = x,
                           FullDescription = GetOrganizationFullDescription(x)
                       })).OrderBy(x => x.FullDescription).ToList(),
                       QueryableOrganizationList = GetQueryableOrganizationList(UniqueID).Select(x => new QueryableOrganizationModel
                       {
                           UniqueID = x,
                           FullDescription = GetOrganizationFullDescription(x)
                       }).OrderBy(x => x.FullDescription).ToList()
                   };

                   if (!string.IsNullOrEmpty(organization.MANAGERUSERID))
                   {
                       model.ManagerList.Add(organization.MANAGERUSERID);
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var organization = db.ORGANIZATION.First(x => x.UNIQUEID == Model.UniqueID);

                    var exists = db.ORGANIZATION.FirstOrDefault(x => x.UNIQUEID != organization.UNIQUEID && x.PARENTUNIQUEID == organization.PARENTUNIQUEID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
#if !DEBUG
                        using (TransactionScope trans = new TransactionScope())
                        {
#endif
                        organization.ID = Model.FormInput.ID;
                        organization.DESCRIPTION = Model.FormInput.Description;

                        if (!string.IsNullOrEmpty(Model.FormInput.Managers))
                        {
                            organization.MANAGERUSERID = Model.FormInput.Managers.Split(',').ToList()[0];

                            //db.ORGANIZATIONMANAGER.AddRange(Model.FormInput.Managers.Split(',').ToList().Select(x => new ORGANIZATIONMANAGER()
                            //{
                            //    ORGANIZATIONUNIQUEID = Model.UniqueID,
                            //    USERID = x
                            //}).ToList());
                        }

                        db.SaveChanges();

                        //db.ORGANIZATIONMANAGER.RemoveRange(db.ORGANIZATIONMANAGER.Where(x => x.ORGANIZATIONUNIQUEID == Model.UniqueID).ToList());
                        db.EDITABLEORGANIZATION.RemoveRange(db.EDITABLEORGANIZATION.Where(x => x.ORGANIZATIONUNIQUEID == Model.UniqueID).ToList());
                        db.QUERYABLEORGANIZATION.RemoveRange(db.QUERYABLEORGANIZATION.Where(x => x.ORGANIZATIONUNIQUEID == Model.UniqueID).ToList());

                        db.SaveChanges();

                       
                        db.EDITABLEORGANIZATION.AddRange(Model.EditableOrganizationList.Where(x=>x.CanDelete).Select(x => new EDITABLEORGANIZATION
                        {
                            ORGANIZATIONUNIQUEID = Model.UniqueID,
                            EDITABLEORGANIZATIONUNIQUEID = x.UniqueID
                        }).ToList());

                        db.QUERYABLEORGANIZATION.AddRange(Model.QueryableOrganizationList.Select(x => new QUERYABLEORGANIZATION
                        {
                            ORGANIZATIONUNIQUEID = Model.UniqueID,
                            QUERYABLEORGANIZATIONUNIQUEID = x.UniqueID
                        }).ToList());

                        db.SaveChanges();
#if !DEBUG
                            trans.Complete();
                        }
#endif

                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.Organization, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.OrganizationID, Resources.Resource.Exists));
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

        public static RequestResult Delete(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var organizationList = GetDownStreamOrganizationList(UniqueID, true);

#if !DEBUG
                using (TransactionScope trans = new TransactionScope())
                {
#endif
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    DeleteHelper.Organization(db, organizationList);

                    db.SaveChanges();
                }
#if !DEBUG
                    trans.Complete();
                }
#endif
                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.Organization, Resources.Resource.Success));
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

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>() 
                { 
                    { Define.EnumTreeAttribute.NodeType, Define.EnumTreeNodeType.Organization.ToString() },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.OrganizationPermission, string.Empty },
                    { Define.EnumTreeAttribute.OrganizationUniqueID, string.Empty }
                };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var organization = OrganizationList.First(x => x.UniqueID == AncestorOrganizationUniqueID);

                    var treeItem = new TreeItem() { Title = organization.Description };

                    attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                    attributes[Define.EnumTreeAttribute.OrganizationPermission] = Account.OrganizationPermission(organization.UniqueID).ToString();
                    attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;

                    foreach (var attribute in attributes)
                    {
                        treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                    }

                    if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID)))
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

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>() 
                { 
                    { Define.EnumTreeAttribute.NodeType, Define.EnumTreeNodeType.Organization.ToString() },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.OrganizationPermission, string.Empty },
                    { Define.EnumTreeAttribute.OrganizationUniqueID, string.Empty }
                };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                    foreach (var organization in organizationList)
                    {
                        var treeItem = new TreeItem() { Title = organization.Description };

                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                        attributes[Define.EnumTreeAttribute.OrganizationPermission] = Account.OrganizationPermission(organization.UniqueID).ToString();
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID)))
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

        public static RequestResult GetEditableOrganizationRootTreeItem(List<Models.Shared.Organization> OrganizationList, string EditableAncestorOrganizationUniqueID, string AncestorOrganizationUniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                if (string.IsNullOrEmpty(EditableAncestorOrganizationUniqueID) || (!string.IsNullOrEmpty(EditableAncestorOrganizationUniqueID) && EditableAncestorOrganizationUniqueID != AncestorOrganizationUniqueID))
                {
                    var attributes = new Dictionary<Define.EnumTreeAttribute, string>()
                    { 
                        { Define.EnumTreeAttribute.NodeType, Define.EnumTreeNodeType.Organization.ToString() },
                        { Define.EnumTreeAttribute.ToolTip, string.Empty },
                        { Define.EnumTreeAttribute.OrganizationUniqueID, string.Empty }
                    };

                    var editableOrganizationList = new List<string>();

                    if (!string.IsNullOrEmpty(EditableAncestorOrganizationUniqueID))
                    {
                        editableOrganizationList = GetDownStreamOrganizationList(EditableAncestorOrganizationUniqueID, true);
                    }

                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var organization = OrganizationList.First(x => x.UniqueID == AncestorOrganizationUniqueID);

                        var treeItem = new TreeItem() { Title = organization.Description };

                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = Account.OrganizationPermission(organization.UniqueID).ToString();
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID) && !editableOrganizationList.Contains(x.UniqueID)))
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

        public static RequestResult GetEditableOrganizationTreeItem(List<Models.Shared.Organization> OrganizationList, string EditableAncestorOrganizationUniqueID, string OrganizationUniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>() 
                { 
                    { Define.EnumTreeAttribute.NodeType, Define.EnumTreeNodeType.Organization.ToString() },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.OrganizationUniqueID, string.Empty }
                };

                var editableOrganizationList = new List<string>();

                if (!string.IsNullOrEmpty(EditableAncestorOrganizationUniqueID))
                {
                    editableOrganizationList = GetDownStreamOrganizationList(EditableAncestorOrganizationUniqueID, true);
                }

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID) && !editableOrganizationList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                    foreach (var organization in organizationList)
                    {
                        var treeItem = new TreeItem() { Title = organization.Description };

                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID) && !editableOrganizationList.Contains(x.UniqueID)))
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

        public static RequestResult AddEditableOrganization(List<EditableOrganizationModel> EditableOrganizationList, List<string> SelectedList, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                foreach (string selected in SelectedList)
                {
                    var downStreamOrganizationList = GetDownStreamOrganizationList(selected, true);

                    foreach (var downStreamOrganization in downStreamOrganizationList)
                    {
                        if (!EditableOrganizationList.Any(x => x.UniqueID == downStreamOrganization))
                        {
                            EditableOrganizationList.Add(new EditableOrganizationModel()
                            {
                                CanDelete = true,
                                UniqueID = downStreamOrganization,
                                FullDescription = GetOrganizationFullDescription(downStreamOrganization)
                            });
                        }
                    }
                }

                result.ReturnData(EditableOrganizationList.OrderBy(x => x.FullDescription).ToList());
            }
            catch (Exception ex)
            {
                var err = new Error(MethodInfo.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetQueryableOrganizationRootTreeItem(List<Models.Shared.Organization> OrganizationList, string EditableAncestorOrganizationUniqueID, string AncestorOrganizationUniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                if (string.IsNullOrEmpty(EditableAncestorOrganizationUniqueID) || (!string.IsNullOrEmpty(EditableAncestorOrganizationUniqueID) && EditableAncestorOrganizationUniqueID != AncestorOrganizationUniqueID))
                {
                    var attributes = new Dictionary<Define.EnumTreeAttribute, string>()
                    { 
                        { Define.EnumTreeAttribute.NodeType, Define.EnumTreeNodeType.Organization.ToString() },
                        { Define.EnumTreeAttribute.ToolTip, string.Empty },
                        { Define.EnumTreeAttribute.OrganizationUniqueID, string.Empty }
                    };

                    var editableOrganizationList = new List<string>();

                    if (!string.IsNullOrEmpty(EditableAncestorOrganizationUniqueID))
                    {
                        editableOrganizationList = GetDownStreamOrganizationList(EditableAncestorOrganizationUniqueID, true);
                    }

                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var organization = OrganizationList.First(x => x.UniqueID == AncestorOrganizationUniqueID);

                        var treeItem = new TreeItem() { Title = organization.Description };

                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = Account.OrganizationPermission(organization.UniqueID).ToString();
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID) && !editableOrganizationList.Contains(x.UniqueID)))
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

        public static RequestResult GetQueryableOrganizationTreeItem(List<Models.Shared.Organization> OrganizationList, string EditableAncestorOrganizationUniqueID, string OrganizationUniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>() 
                { 
                    { Define.EnumTreeAttribute.NodeType, Define.EnumTreeNodeType.Organization.ToString() },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.OrganizationUniqueID, string.Empty }
                };

                var editableOrganizationList = new List<string>();

                if (!string.IsNullOrEmpty(EditableAncestorOrganizationUniqueID))
                {
                    editableOrganizationList = GetDownStreamOrganizationList(EditableAncestorOrganizationUniqueID, true);
                }

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID) && !editableOrganizationList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                    foreach (var organization in organizationList)
                    {
                        var treeItem = new TreeItem() { Title = organization.Description };

                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID) && !editableOrganizationList.Contains(x.UniqueID)))
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

        public static RequestResult AddQueryableOrganization(List<QueryableOrganizationModel> QueryableOrganizationList, List<string> SelectedList, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                foreach (string selected in SelectedList)
                {
                    var downStreamOrganizationList = GetDownStreamOrganizationList(selected, true);

                    foreach (var downStreamOrganization in downStreamOrganizationList)
                    {
                        if (!QueryableOrganizationList.Any(x => x.UniqueID == downStreamOrganization))
                        {
                            QueryableOrganizationList.Add(new QueryableOrganizationModel()
                            {
                                UniqueID = downStreamOrganization,
                                FullDescription = GetOrganizationFullDescription(downStreamOrganization)
                            });
                        }
                    }
                }

                result.ReturnData(QueryableOrganizationList.OrderBy(x => x.FullDescription).ToList());
            }
            catch (Exception ex)
            {
                var err = new Error(MethodInfo.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static List<UserOrganizationPermission> GetUserOrganizationPermissionList(string OrganizationUniqueID)
        {
            var itemList = new List<UserOrganizationPermission>();

            try
            {
                var upStreamOrganizationList = GetUpStreamOrganizationList(OrganizationUniqueID, false);

                itemList.AddRange(upStreamOrganizationList.Select(x => new UserOrganizationPermission
                {
                    UniqueID = x,
                    Permission = Define.EnumOrganizationPermission.Visible
                }).ToList());

                var downStreamOrganizationList = GetDownStreamOrganizationList(OrganizationUniqueID, true);

                itemList.AddRange(downStreamOrganizationList.Select(x => new UserOrganizationPermission
                {
                    UniqueID = x,
                    Permission = Define.EnumOrganizationPermission.Editable
                }).ToList());

                var editableOrganizationList = GetEditableOrganizationList(OrganizationUniqueID);

                foreach (var o in editableOrganizationList)
                {
                    var item = itemList.FirstOrDefault(x => x.UniqueID == o);

                    if (item == null)
                    {
                        itemList.Add(new UserOrganizationPermission()
                        {
                            UniqueID = o,
                            Permission = Define.EnumOrganizationPermission.Editable
                        });
                    }
                    else
                    {
                        item.Permission = Define.EnumOrganizationPermission.Editable;
                    }
                }

                var queryableOrganizationList = GetQueryableOrganizationList(OrganizationUniqueID);

                foreach (var o in queryableOrganizationList)
                {
                    var item = itemList.FirstOrDefault(x => x.UniqueID == o);

                    if (item == null)
                    {
                        itemList.Add(new UserOrganizationPermission()
                        {
                            UniqueID = o,
                            Permission = Define.EnumOrganizationPermission.Queryable
                        });
                    }
                    else
                    {
                        if (item.Permission == Define.EnumOrganizationPermission.Visible)
                        {
                            item.Permission = Define.EnumOrganizationPermission.Queryable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                itemList = null;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return itemList;
        }

        public static string GetUserRootOrganizationUniqueID(List<Models.Shared.Organization> AllOrganizationList, Account Account)
        {
            var rootOrganizationUniqueID = string.Empty;

            try
            {
                if (Account.OrganizationUniqueID == "*")
                {
                    rootOrganizationUniqueID = "*";
                }
                else
                {
                    var ancensorOrganizationUniqueID = GetAncestorOrganizationUniqueID(Account.OrganizationUniqueID);

                    if (Account.QueryableOrganizationUniqueIDList.Contains(ancensorOrganizationUniqueID))
                    {
                        rootOrganizationUniqueID = ancensorOrganizationUniqueID;
                    }
                    else
                    {
                        var organizationList = AllOrganizationList.Where(x => x.ParentUniqueID == ancensorOrganizationUniqueID).ToList();

                        rootOrganizationUniqueID = GetUserRootOrganizationUniqueID(AllOrganizationList, Account, organizationList, ancensorOrganizationUniqueID);
                    }
                }
            }
            catch (Exception ex)
            {
                rootOrganizationUniqueID = string.Empty;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return rootOrganizationUniqueID;
        }

        public static string GetUserRootOrganizationUniqueID(List<Models.Shared.Organization> AllOrganizationList, Account Account, List<Models.Shared.Organization> OrganizationList, string ParentOrganizationUniqueID)
        {
            var intersectResult = Account.QueryableOrganizationUniqueIDList.Intersect(OrganizationList.Select(x => x.UniqueID));

            if (intersectResult.Count() == 0)
            {
                var queryableOrganizationList = new List<string>();

                foreach (var organization in OrganizationList)
                {
                    var downStreamOrganizationList = GetDownStreamOrganizationList(AllOrganizationList, organization.UniqueID, false);

                    var i = Account.QueryableOrganizationUniqueIDList.Intersect(downStreamOrganizationList).ToList();

                    if (i.Count > 0)
                    {
                        queryableOrganizationList.Add(organization.UniqueID);
                    }
                }

                if (queryableOrganizationList.Count > 1)
                {
                    return ParentOrganizationUniqueID;
                }
                else
                {
                    var organizationUniqueID = queryableOrganizationList.First();

                    var organizationList = AllOrganizationList.Where(x => x.ParentUniqueID == organizationUniqueID).ToList();

                    return GetUserRootOrganizationUniqueID(AllOrganizationList, Account, organizationList, organizationUniqueID);
                }
            }
            else if (intersectResult.Count() == 1)
            {
                return intersectResult.First();
            }
            else
            {
                return ParentOrganizationUniqueID;
            }
        }

        public static List<string> GetUpStreamOrganizationList(string OrganizationUniqueID, bool Include)
        {
            var itemList = new List<string>();

            try
            {
                if (Include)
                {
                    itemList.Add(OrganizationUniqueID);
                }

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    //向上搜尋
                    if (OrganizationUniqueID != "*")
                    {
                        var organization = db.ORGANIZATION.First(x => x.UNIQUEID == OrganizationUniqueID);

                        while (organization.PARENTUNIQUEID != "*")
                        {
                            organization = db.ORGANIZATION.First(x => x.UNIQUEID == organization.PARENTUNIQUEID);

                            itemList.Add(organization.UNIQUEID);
                        }

                        itemList.Add("*");
                    }
                }
            }
            catch (Exception ex)
            {
                itemList = null;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return itemList;
        }

        public static List<string> GetUpStreamOrganizationList(List<Models.Shared.Organization> AllOrganizationList, string OrganizationUniqueID, bool Include)
        {
            var itemList = new List<string>();

            try
            {
                if (Include)
                {
                    itemList.Add(OrganizationUniqueID);
                }

                //向上搜尋
                if (OrganizationUniqueID != "*")
                {
                    var organization = AllOrganizationList.First(x => x.UniqueID == OrganizationUniqueID);

                    while (organization.ParentUniqueID != "*")
                    {
                        organization = AllOrganizationList.First(x => x.UniqueID == organization.ParentUniqueID);

                        itemList.Add(organization.UniqueID);
                    }

                    itemList.Add("*");
                }
            }
            catch (Exception ex)
            {
                itemList = null;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return itemList;
        }

        public static List<string> GetDownStreamOrganizationList(string OrganizationUniqueID, bool Include)
        {
            var itemList = new List<string>();

            try
            {
                if (Include)
                {
                    itemList.Add(OrganizationUniqueID);
                }

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    //向下搜尋
                    var organizationList = db.ORGANIZATION.Where(x => x.PARENTUNIQUEID == OrganizationUniqueID).Select(x => x.UNIQUEID).ToList();

                    while (organizationList.Count > 0)
                    {
                        itemList.AddRange(organizationList);

                        organizationList = db.ORGANIZATION.Where(x => organizationList.Contains(x.PARENTUNIQUEID)).Select(x => x.UNIQUEID).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                itemList = null;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return itemList;
        }

        public static List<string> GetDownStreamOrganizationList(List<Models.Shared.Organization> OrganizationList, string OrganizationUniqueID, bool Include)
        {
            var itemList = new List<string>();

            try
            {
                if (Include)
                {
                    itemList.Add(OrganizationUniqueID);
                }

                //向下搜尋
                var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID).Select(x => x.UniqueID).ToList();

                while (organizationList.Count > 0)
                {
                    itemList.AddRange(organizationList);

                    organizationList = OrganizationList.Where(x => organizationList.Contains(x.ParentUniqueID)).Select(x => x.UniqueID).ToList();
                }
            }
            catch (Exception ex)
            {
                itemList = null;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return itemList;
        }

        public static List<string> GetEditableOrganizationList(string OrganizationUniqueID)
        {
            var itemList = new List<string>();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    itemList = db.EDITABLEORGANIZATION.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).Select(x => x.EDITABLEORGANIZATIONUNIQUEID).ToList();
                }
            }
            catch (Exception ex)
            {
                itemList = null;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return itemList;
        }

        public static List<string> GetQueryableOrganizationList(string OrganizationUniqueID)
        {
            var itemList = new List<string>();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    itemList = db.QUERYABLEORGANIZATION.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).Select(x => x.QUERYABLEORGANIZATIONUNIQUEID).ToList();
                }
            }
            catch (Exception ex)
            {
                itemList = null;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return itemList;
        }

        public static string GetAncestorOrganizationUniqueID(string OrganizationUniqueID)
        {
            string ancestorOrganizationUniqueID = string.Empty;

            try
            {
                var upStreamOrganizationList = GetUpStreamOrganizationList(OrganizationUniqueID, true);

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    ancestorOrganizationUniqueID = db.ORGANIZATION.First(x => x.PARENTUNIQUEID == "*" && upStreamOrganizationList.Contains(x.UNIQUEID)).UNIQUEID;
                }
            }
            catch (Exception ex)
            {
                ancestorOrganizationUniqueID = string.Empty;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return ancestorOrganizationUniqueID;
        }

        public static OrganizationModel GetOrganization(string UniqueID)
        {
            OrganizationModel organization = null;

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = db.ORGANIZATION.First(x => x.UNIQUEID == UniqueID);

                    organization = new OrganizationModel()
                    {
                        AncestorOrganizationUniqueID = GetAncestorOrganizationUniqueID(query.UNIQUEID),
                        UniqueID = query.UNIQUEID,
                        ID = query.ID,
                        Description = query.DESCRIPTION,
                        FullDescription = GetOrganizationFullDescription(query.UNIQUEID),
                        ManagerID = query.MANAGERUSERID
                    }; 
                }
            }
            catch (Exception ex)
            {
                organization = null;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return organization;
        }

        public static List<Models.Shared.Organization> GetFactoryList(List<Models.Shared.Organization> OrganizationList)
        {
            return OrganizationList.Where(x => x.ParentUniqueID == "2a54f076-f14c-44fd-9f42-b202ac9206e0").OrderBy(x=>x.Description).ToList();
        }

        public static string GetFactory(List<Models.Shared.Organization> OrganizationList, string UniqueID)
        {
            string factory = string.Empty;

            try
            {
                if (OrganizationList != null)
                {
                    var organization = OrganizationList.First(x => x.UniqueID == UniqueID);

                    while (organization.ParentUniqueID != "2a54f076-f14c-44fd-9f42-b202ac9206e0" && organization.ParentUniqueID != "*")
                    {
                        organization = OrganizationList.First(x => x.UniqueID == organization.ParentUniqueID);

                        if (organization.ParentUniqueID == "2a54f076-f14c-44fd-9f42-b202ac9206e0")
                        {
                            factory = organization.Description;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                factory = string.Empty;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return factory;
        }

        public static string GetOrganizationDescription(string UniqueID)
        {
            string description = string.Empty;

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var organization = db.ORGANIZATION.FirstOrDefault(x => x.UNIQUEID == UniqueID);

                    if (organization != null)
                    {
                        description = organization.DESCRIPTION;
                    }
                }
            }
            catch (Exception ex)
            {
                description = string.Empty;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return description;
        }

        public static string GetOrganizationFullDescription(string UniqueID)
        {
            string description = string.Empty;

            try
            {
                if (UniqueID == "*")
                {
                    description = "*";
                }
                else
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var organization = db.ORGANIZATION.First(x => x.UNIQUEID == UniqueID);

                        description = organization.DESCRIPTION;

                        while (organization.PARENTUNIQUEID != "*")
                        {
                            organization = db.ORGANIZATION.First(x => x.UNIQUEID == organization.PARENTUNIQUEID);

                            description = organization.DESCRIPTION + " -> " + description;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                description = string.Empty;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return description;
        }

        public static string GetOrganizationFullDescriptionWithOutArrow(string UniqueID)
        {
            string description = string.Empty;

            try
            {
                if (UniqueID == "*")
                {
                    description = "*";
                }
                else
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var organization = db.ORGANIZATION.First(x => x.UNIQUEID == UniqueID);

                        description = organization.DESCRIPTION;

                        while (organization.PARENTUNIQUEID != "*")
                        {
                            organization = db.ORGANIZATION.First(x => x.UNIQUEID == organization.PARENTUNIQUEID);

                            description = organization.DESCRIPTION + description;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                description = string.Empty;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return description;
        }
    }
}
