using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using Models.Shared;
using Models.Authenticated;
using Models.UserManagement;
using DbEntity.ASE;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.ASE
{
    public class UserDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = (from u in db.ACCOUNT
                                 join o in db.ORGANIZATION
                                 on u.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                 where downStreamOrganizationList.Contains(u.ORGANIZATIONUNIQUEID) && Account.QueryableOrganizationUniqueIDList.Contains(u.ORGANIZATIONUNIQUEID)
                                 select new
                                 {
                                     User = u,
                                     OrganizationDescription = o.DESCRIPTION
                                 }).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.User.TITLE.Contains(Parameters.Keyword) || x.User.ID.Contains(Parameters.Keyword) || x.User.NAME.Contains(Parameters.Keyword));
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
                            Permission = Account.OrganizationPermission(x.User.ORGANIZATIONUNIQUEID),
                            OrganizationDescription = x.OrganizationDescription,
                            Title = x.User.TITLE,
                            ID = x.User.ID,
                            Name = x.User.NAME,
                            Email = x.User.EMAIL,
                            IsMobileUser = x.User.ISMOBILEUSER == "Y",
                            AuthGroupList = (from y in db.USERAUTHGROUP
                                             join a in db.AUTHGROUP
                                             on y.AUTHGROUPID equals a.ID
                                             where y.USERID == x.User.ID
                                             orderby a.ID
                                             select a.NAME).ToList()
                        }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.Title).ThenBy(x => x.ID).ToList()
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

        public static RequestResult GetDetailViewModel(string UserID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var user = db.ACCOUNT.First(x => x.ID == UserID);

                    result.ReturnData(new DetailViewModel()
                    {
                        Permission = Account.OrganizationPermission(user.ORGANIZATIONUNIQUEID),
                        OrganizationUniqueID = user.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(user.ORGANIZATIONUNIQUEID),
                        ID = user.ID,
                        Name = user.NAME,
                        UID = user.TAGID,
                        EMail = user.EMAIL,
                        Title = user.TITLE,
                        IsMobileUser = user.ISMOBILEUSER == "Y",
                        AuthGroupNameList = (from x in db.USERAUTHGROUP
                                             join a in db.AUTHGROUP
                                             on x.AUTHGROUPID equals a.ID
                                             where x.USERID == user.ID
                                             select a.NAME).ToList()
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

        public static RequestResult GetCreateFormModel(string OrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    result.ReturnData(new CreateFormModel()
                    {
                        OrganizationUniqueID = OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(OrganizationUniqueID),
                        AuthGroupList = db.AUTHGROUP.Select(x => new Models.UserManagement.AuthGroup { ID = x.ID, Name = x.NAME }).OrderBy(x => x.ID).ToList()
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

        public static RequestResult GetCopyFormModel(string UserID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var user = db.ACCOUNT.First(x => x.ID == UserID);

                    result.ReturnData(new CreateFormModel()
                    {
                        OrganizationUniqueID = user.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(user.ORGANIZATIONUNIQUEID),
                        AuthGroupList = db.AUTHGROUP.Select(x => new Models.UserManagement.AuthGroup { ID = x.ID, Name = x.NAME }).OrderBy(x => x.ID).ToList(),
                        UserAuthGroupList = db.USERAUTHGROUP.Where(x => x.USERID == UserID).Select(x => x.AUTHGROUPID).ToList(),
                        FormInput = new FormInput()
                        {
                            Title = user.TITLE,
                            IsMobileUser = user.ISMOBILEUSER == "Y"
                        }
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var exists = db.ACCOUNT.FirstOrDefault(x => x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
                        bool exceedsLimit = false;

                        var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(Model.OrganizationUniqueID, true);

                        UserLimit userLimit = null;

                        foreach (var item in Config.UserLimits)
                        {
                            if (upStreamOrganizationList.Contains(item.OrganizationUniqueID))
                            {
                                userLimit = item;

                                break;
                            }
                        }

                        if (userLimit != null)
                        {
                            var organization = OrganizationDataAccessor.GetOrganization(userLimit.OrganizationUniqueID);

                            var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(userLimit.OrganizationUniqueID, true);

                            var users = db.ACCOUNT.Where(x => downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)).ToList();

                            if (users.Count + 1 > userLimit.Users)
                            {
                                exceedsLimit = true;

                                result.ReturnFailedMessage(string.Format(Resources.Resource.ExceedsUserLimit, organization.Description, userLimit.Users));
                            }
                            else
                            {
                                if (Model.FormInput.IsMobileUser && users.Count(x => x.ISMOBILEUSER == "Y") + 1 > userLimit.MobileUsers)
                                {
                                    exceedsLimit = true;

                                    result.ReturnFailedMessage(string.Format(Resources.Resource.ExceedsMobileUserLimit, organization.Description, userLimit.MobileUsers));
                                }
                            }
                        }

                        if (!exceedsLimit)
                        {
                            db.ACCOUNT.Add(new ACCOUNT()
                            {
                                ORGANIZATIONUNIQUEID = Model.OrganizationUniqueID,
                                ID = Model.FormInput.ID,
                                NAME = Model.FormInput.Name,
                                PASSWORD = Model.FormInput.ID,
                                TITLE = Model.FormInput.Title,
                                EMAIL = Model.FormInput.EMail,
                                TAGID = Model.FormInput.UID,
                                ISMOBILEUSER = Model.FormInput.IsMobileUser ? "Y" : "N",
                                LASTMODIFYTIME = DateTime.Now
                            });

                            db.USERAUTHGROUP.AddRange(Model.FormInput.AuthGroupList.Select(x => new USERAUTHGROUP
                            {
                                USERID = Model.FormInput.ID,
                                AUTHGROUPID = x
                            }).ToList());

                            db.SaveChanges();

                            result.ReturnData(Model.FormInput.ID, string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.User, Resources.Resource.Success));
                        }
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.UserID, Resources.Resource.Exists));
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

        public static RequestResult GetEditFormModel(string UserID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var user = db.ACCOUNT.First(x => x.ID == UserID);

                    result.ReturnData(new EditFormModel()
                    {
                        UserID = user.ID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(user.ORGANIZATIONUNIQUEID),
                        AuthGroupList = db.AUTHGROUP.Select(x => new Models.UserManagement.AuthGroup { ID = x.ID, Name = x.NAME }).OrderBy(x => x.ID).ToList(),
                        UserAuthGroupList = db.USERAUTHGROUP.Where(x => x.USERID == UserID).Select(x => x.AUTHGROUPID).ToList(),
                        FormInput = new FormInput()
                        {
                            ID = user.ID,
                            Name = user.NAME,
                            Title = user.TITLE,
                            EMail = user.EMAIL,
                            UID = user.TAGID,
                            IsMobileUser = user.ISMOBILEUSER == "Y"
                        }
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var user = db.ACCOUNT.First(x => x.ID == Model.UserID);

                    bool exceedsLimit = false;

                    if (Model.FormInput.IsMobileUser)
                    {
                        var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(user.ORGANIZATIONUNIQUEID, true);

                        UserLimit userLimit = null;

                        foreach (var item in Config.UserLimits)
                        {
                            if (upStreamOrganizationList.Contains(item.OrganizationUniqueID))
                            {
                                userLimit = item;

                                break;
                            }
                        }

                        if (userLimit != null)
                        {
                            var organization = OrganizationDataAccessor.GetOrganization(userLimit.OrganizationUniqueID);

                            var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(userLimit.OrganizationUniqueID, true);

                            var mobileUsers = db.ACCOUNT.Where(x => x.ID != user.ID && x.ISMOBILEUSER == "Y" && downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)).ToList();

                            if (mobileUsers.Count + 1 > userLimit.MobileUsers)
                            {
                                exceedsLimit = true;

                                result.ReturnFailedMessage(string.Format(Resources.Resource.ExceedsMobileUserLimit, organization.Description, userLimit.MobileUsers));
                            }
                        }
                    }

                    if (!exceedsLimit)
                    {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                        #region User
                        user.NAME = Model.FormInput.Name;
                        user.TITLE = Model.FormInput.Title;
                        user.EMAIL = Model.FormInput.EMail;
                        user.TAGID = Model.FormInput.UID;
                        user.ISMOBILEUSER = Model.FormInput.IsMobileUser ? "Y" : "N";
                        user.LASTMODIFYTIME = DateTime.Now;

                        db.SaveChanges();
                        #endregion

                        #region UserAuthGroup
                        #region Delete
                        db.USERAUTHGROUP.RemoveRange(db.USERAUTHGROUP.Where(x => x.USERID == Model.UserID).ToList());

                        db.SaveChanges();
                        #endregion

                        #region Insert
                        db.USERAUTHGROUP.AddRange(Model.FormInput.AuthGroupList.Select(x => new USERAUTHGROUP
                        {
                            USERID = Model.UserID,
                            AUTHGROUPID = x
                        }).ToList());

                        db.SaveChanges();
                        #endregion
                        #endregion
#if !DEBUG
                        trans.Complete();
                    }
#endif
                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.User, Resources.Resource.Success));
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

        public static RequestResult ResetPassword(string UserID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var user = db.ACCOUNT.First(x => x.ID == UserID);

                    user.PASSWORD = user.ID;

                    db.SaveChanges();

                    result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.ResetPassword, Resources.Resource.Success));
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

        public static RequestResult UpdateUID(string UserID, string UID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var user = db.ACCOUNT.First(x => x.ID == UserID);

                    user.TAGID = UID;

                    db.SaveChanges();
                }

                result.Success();
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        /// <summary>
        /// Implement...
        /// </summary>
        public static RequestResult Delete(List<string> SelectedList)
        {
            RequestResult result = new RequestResult();

            try
            {
#if !DEBUG
                using (TransactionScope trans = new TransactionScope())
                {
#endif
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    DeleteHelper.User(db, SelectedList);

                    db.SaveChanges();
                }
#if !DEBUG
                    trans.Complete();
                }
#endif

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.User, Resources.Resource.Success));
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
                    { Define.EnumTreeAttribute.OrganizationUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.UserID, string.Empty }
                };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var organization = OrganizationList.First(x => x.UniqueID == AncestorOrganizationUniqueID);

                    var treeItem = new TreeItem() { Title = organization.Description };

                    attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                    attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                    attributes[Define.EnumTreeAttribute.UserID] = string.Empty;

                    foreach (var attribute in attributes)
                    {
                        treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                    }

                    var haveDownStreamOrganization = OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID));

                    bool haveUser = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.ACCOUNT.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID);

                    if (haveDownStreamOrganization || haveUser)
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
                    { Define.EnumTreeAttribute.NodeType, string.Empty },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.OrganizationUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.UserID, string.Empty }
                };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                    {
                        var userList = db.ACCOUNT.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                        foreach (var user in userList)
                        {
                            var treeItem = new TreeItem() { Title = user.NAME };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.User.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", user.ID, user.NAME);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.UserID] = user.ID;

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

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                        attributes[Define.EnumTreeAttribute.UserID] = string.Empty;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        bool haveDownStreamOrganizaiton = OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID));

                        bool haveUser = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.ACCOUNT.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID);

                        if (haveDownStreamOrganizaiton || haveUser)
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

        public static RequestResult GetSelectTreeItem(List<Models.Shared.Organization> OrganizationList, string OrganizationUniqueID, Account Account)
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var userList = db.ACCOUNT.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                    foreach (var user in userList)
                    {
                        var treeItem = new TreeItem() { Title = user.NAME };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.User.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", user.ID, user.NAME);
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                        attributes[Define.EnumTreeAttribute.UserID] = user.ID;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        treeItemList.Add(treeItem);
                    }

                    var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && Account.QueryableOrganizationUniqueIDList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                    foreach (var organization in organizationList)
                    {
                        var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organization.UniqueID, true);

                        if (db.ACCOUNT.Any(x => downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID) && Account.QueryableOrganizationUniqueIDList.Contains(x.ORGANIZATIONUNIQUEID)))
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

                            treeItem.State = "closed";

                            treeItemList.Add(treeItem);
                        }
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

        public static UserModel GetUser(string UserID)
        {
            var userModel = new UserModel();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    userModel = (from u in db.ACCOUNT
                                 join o in db.ORGANIZATION
                                 on u.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                 where u.ID == UserID
                                 select new UserModel
                                 {
                                     OrganizationDescription = o.DESCRIPTION,
                                     ID = u.ID,
                                     Name = u.NAME
                                 }).FirstOrDefault();

                    if (userModel == null)
                    {
                        userModel = new UserModel() { ID = UserID };
                    }
                }
            }
            catch (Exception ex)
            {
                userModel = new UserModel() { ID = UserID };

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return userModel;
        }
    }
}
