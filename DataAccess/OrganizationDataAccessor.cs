using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using Models.Shared;
using Models.Authenticated;
using Models.OrganizationManagement;
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using DbEntity.MSSQL.GuardPatrol;
using DbEntity.MSSQL.PipelinePatrol;
using DbEntity.MSSQL.TruckPatrol;
#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess
{
    public class OrganizationDataAccessor
    {
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

        public static List<Models.Shared.Organization> GetAllOrganization()
        {
            var organizationList = new List<Models.Shared.Organization>();

            try
            {
                using (DbEntities db = new DbEntities())
                {
                    organizationList = db.Organization.Select(x => new Models.Shared.Organization
                    {
                        UniqueID = x.UniqueID,
                        ParentUniqueID = x.ParentUniqueID,
                        ID = x.ID,
                        Description = x.Description
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
                using (DbEntities db = new DbEntities())
                {
                    var organization = (from o in db.Organization
                                        //join m in db.User
                                        //on o.ManagerUserID equals m.ID into tmpManager
                                        //from m in tmpManager.DefaultIfEmpty()
                                        where o.UniqueID == UniqueID
                                        select new
                                        {
                                            o.UniqueID,
                                            o.ParentUniqueID,
                                            o.ID,
                                            o.Description,
                                            //o.ManagerUserID,
                                            //ManagerUserName = m != null ? m.Name : string.Empty
                                        }).First();

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = organization.UniqueID,
                        Permission = Account.OrganizationPermission(UniqueID),
                        ParentOrganizationFullDescription = GetOrganizationFullDescription(organization.ParentUniqueID),
                        ID = organization.ID,
                        Description = organization.Description,
                        EditableOrganizationList = GetEditableOrganizationList(UniqueID).Union(GetDownStreamOrganizationList(UniqueID, true)).Select(x => GetOrganizationFullDescription(x)).OrderBy(x => x).ToList(),
                        QueryableOrganizationList = GetQueryableOrganizationList(UniqueID).Select(x => GetOrganizationFullDescription(x)).OrderBy(x => x).ToList(),
                        ManagerList = (from x in db.OrganizationManager
                                       join u in db.User
                                       on x.UserID equals u.ID
                                       where x.OrganizationUniqueID == UniqueID
                                       select new UserModel
                                       {
                                           ID = u.ID,
                                           Name = u.Name
                                       }).OrderBy(x => x.ID).ToList()
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

        public static RequestResult Create(CreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (DbEntities db = new DbEntities())
                {
                    var exists = db.Organization.FirstOrDefault(x => x.ParentUniqueID == Model.ParentUniqueID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
                        string uniqueID = Guid.NewGuid().ToString();

                        db.Organization.Add(new DbEntity.MSSQL.Organization()
                        {
                            UniqueID = uniqueID,
                            ParentUniqueID = Model.ParentUniqueID,
                            ID = Model.FormInput.ID,
                            Description = Model.FormInput.Description,
                            //ManagerUserID = Model.FormInput.ManagerUserID
                        });

                        if (!string.IsNullOrEmpty(Model.FormInput.Managers))
                        {
                            db.OrganizationManager.AddRange(Model.FormInput.Managers.Split(',').ToList().Select(x => new OrganizationManager()
                            {
                                OrganizationUniqueID = uniqueID,
                                UserID = x
                            }).ToList());
                        }

                        db.EditableOrganization.AddRange(Model.EditableOrganizationList.Where(x=>x.CanDelete).Select(x => new EditableOrganization
                        {
                            OrganizationUniqueID = uniqueID,
                            EditableOrganizationUniqueID = x.UniqueID
                        }).ToList());

                        db.QueryableOrganization.AddRange(Model.QueryableOrganizationList.Select(x => new QueryableOrganization
                        {
                            OrganizationUniqueID = uniqueID,
                            QueryableOrganizationUniqueID = x.UniqueID
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
                using (DbEntities db = new DbEntities())
                {
                    var organization = (from o in db.Organization
                                        //join m in db.User
                                        //on o.ManagerUserID equals m.ID into tmpManager
                                        //from m in tmpManager.DefaultIfEmpty()
                                        where o.UniqueID == UniqueID
                                        select new
                                        {
                                            o.UniqueID,
                                            o.ParentUniqueID,
                                            o.ID,
                                            o.Description,
                                            //o.ManagerUserID,
                                            //ManagerUserName = m != null ? m.Name : string.Empty
                                        }).First();

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = organization.UniqueID,
                        AncestorOrganizationUniqueID = GetAncestorOrganizationUniqueID(organization.UniqueID),
                        ParentOrganizationFullDescription = GetOrganizationFullDescription(organization.ParentUniqueID),
                        //ManagerName = organization.ManagerUserName,
                        FormInput = new FormInput()
                        {
                            ID = organization.ID,
                            Description = organization.Description,
                            //ManagerUserID = organization.ManagerUserID
                        },
                        //UserList = (from u in db.User
                        //            join o in db.Organization
                        //            on u.OrganizationUniqueID equals o.UniqueID
                        //            select new Models.Shared.UserModel
                        //            {
                        //                ID = u.ID,
                        //                Name = u.Name
                        //            }).ToList(),
                        ManagerList = db.OrganizationManager.Where(x => x.OrganizationUniqueID == UniqueID).Select(x => x.UserID).ToList(),
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

        public static RequestResult Edit(EditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (DbEntities db = new DbEntities())
                {
                    var organization = db.Organization.First(x => x.UniqueID == Model.UniqueID);

                    var exists = db.Organization.FirstOrDefault(x => x.UniqueID != organization.UniqueID && x.ParentUniqueID == organization.ParentUniqueID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
#if !DEBUG
                        using (TransactionScope trans = new TransactionScope())
                        {
#endif
                        organization.ID = Model.FormInput.ID;
                        organization.Description = Model.FormInput.Description;
                        //organization.ManagerUserID = Model.FormInput.ManagerUserID;

                        db.SaveChanges();

                        db.OrganizationManager.RemoveRange(db.OrganizationManager.Where(x => x.OrganizationUniqueID == Model.UniqueID).ToList());
                        db.EditableOrganization.RemoveRange(db.EditableOrganization.Where(x => x.OrganizationUniqueID == Model.UniqueID).ToList());
                        db.QueryableOrganization.RemoveRange(db.QueryableOrganization.Where(x => x.OrganizationUniqueID == Model.UniqueID).ToList());

                        db.SaveChanges();

                        if (!string.IsNullOrEmpty(Model.FormInput.Managers))
                        {
                            db.OrganizationManager.AddRange(Model.FormInput.Managers.Split(',').ToList().Select(x => new OrganizationManager()
                            {
                                OrganizationUniqueID = Model.UniqueID,
                                UserID = x
                            }).ToList());
                        }

                        db.EditableOrganization.AddRange(Model.EditableOrganizationList.Where(x=>x.CanDelete).Select(x => new EditableOrganization
                        {
                            OrganizationUniqueID = Model.UniqueID,
                            EditableOrganizationUniqueID = x.UniqueID
                        }).ToList());

                        db.QueryableOrganization.AddRange(Model.QueryableOrganizationList.Select(x => new QueryableOrganization
                        {
                            OrganizationUniqueID = Model.UniqueID,
                            QueryableOrganizationUniqueID = x.UniqueID
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
                using (DbEntities db = new DbEntities())
                {
                    DeleteHelper.Organization(db, organizationList);

                    db.SaveChanges();

                    //if (Config.Modules.Contains(Define.EnumSystemModule.EquipmentMaintenance) || Config.Modules.Contains(Define.EnumSystemModule.EquipmentPatrol))
                    //{
                    //    using (EDbEntities edb = new EDbEntities())
                    //    {
                    //        DeleteHelper.Organization(edb, organizationList);

                    //        edb.SaveChanges();
                    //    }
                    //}

                    //if (Config.Modules.Contains(Define.EnumSystemModule.GuardPatrol))
                    //{
                    //    using (GDbEntities gdb = new GDbEntities())
                    //    {
                    //        DeleteHelper.Organization(gdb, organizationList);

                    //        gdb.SaveChanges();
                    //    }
                    //}

                    //if (Config.Modules.Contains(Define.EnumSystemModule.PipelinePatrol))
                    //{
                    //    using (PDbEntities pdb = new PDbEntities())
                    //    {
                    //        DeleteHelper.Organization(pdb, organizationList);

                    //        pdb.SaveChanges();
                    //    }
                    //}

                    //if (Config.Modules.Contains(Define.EnumSystemModule.TruckPatrol))
                    //{
                    //    using (TDbEntities tdb = new TDbEntities())
                    //    {
                    //        DeleteHelper.Organization(tdb, organizationList);

                    //        tdb.SaveChanges();
                    //    }
                    //}
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

                using (DbEntities db = new DbEntities())
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
                using (EDbEntities db = new EDbEntities())
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
                using (EDbEntities db = new EDbEntities())
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

        public static List<string> GetUpStreamOrganizationList(string OrganizationUniqueID, bool Include)
        {
            var itemList = new List<string>();

            try
            {
                if (Include)
                {
                    itemList.Add(OrganizationUniqueID);
                }

                using (DbEntities db = new DbEntities())
                {
                    //向上搜尋
                    if (OrganizationUniqueID != "*")
                    {
                        var organization = db.Organization.First(x => x.UniqueID == OrganizationUniqueID);

                        while (organization.ParentUniqueID != "*")
                        {
                            organization = db.Organization.First(x => x.UniqueID == organization.ParentUniqueID);

                            itemList.Add(organization.UniqueID);
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

        public static List<string> GetDownStreamOrganizationList(string OrganizationUniqueID, bool Include)
        {
            var itemList = new List<string>();

            try
            {
                if (Include)
                {
                    itemList.Add(OrganizationUniqueID);
                }

                using (DbEntities db = new DbEntities())
                {
                    //向下搜尋
                    var organizationList = db.Organization.Where(x => x.ParentUniqueID == OrganizationUniqueID).Select(x => x.UniqueID).ToList();

                    while (organizationList.Count > 0)
                    {
                        itemList.AddRange(organizationList);

                        organizationList = db.Organization.Where(x => organizationList.Contains(x.ParentUniqueID)).Select(x => x.UniqueID).ToList();
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
                using (DbEntities db = new DbEntities())
                {
                    itemList = db.EditableOrganization.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).Select(x => x.EditableOrganizationUniqueID).ToList();
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
                using (DbEntities db = new DbEntities())
                {
                    itemList = db.QueryableOrganization.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).Select(x => x.QueryableOrganizationUniqueID).ToList();
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

                using (DbEntities db = new DbEntities())
                {
                    ancestorOrganizationUniqueID = db.Organization.First(x => x.ParentUniqueID == "*" && upStreamOrganizationList.Contains(x.UniqueID)).UniqueID;
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
                using (DbEntities db = new DbEntities())
                {
                    var query = db.Organization.First(x => x.UniqueID == UniqueID);

                    organization = new OrganizationModel()
                    {
                        AncestorOrganizationUniqueID = GetAncestorOrganizationUniqueID(query.UniqueID),
                        UniqueID = query.UniqueID,
                        ID = query.ID,
                        Description = query.Description,
                        FullDescription = GetOrganizationFullDescription(query.UniqueID),
                        ManagerID = query.ManagerUserID
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

        public static string GetOrganizationDescription(string UniqueID)
        {
            string description = string.Empty;

            try
            {
                using (DbEntities db = new DbEntities())
                {
                    var organization = db.Organization.FirstOrDefault(x => x.UniqueID == UniqueID);

                    if (organization != null)
                    {
                        description = organization.Description;
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
                    using (DbEntities db = new DbEntities())
                    {
                        var organization = db.Organization.First(x => x.UniqueID == UniqueID);

                        description = organization.Description;

                        while (organization.ParentUniqueID != "*")
                        {
                            organization = db.Organization.First(x => x.UniqueID == organization.ParentUniqueID);

                            description = organization.Description + " -> " + description;
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
                    using (DbEntities db = new DbEntities())
                    {
                        var organization = db.Organization.First(x => x.UniqueID == UniqueID);

                        description = organization.Description;

                        while (organization.ParentUniqueID != "*")
                        {
                            organization = db.Organization.First(x => x.UniqueID == organization.ParentUniqueID);

                            description = organization.Description + description;
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
