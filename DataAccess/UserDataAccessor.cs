using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using Models.Shared;
using Models.Authenticated;
using Models.UserManagement;
using DbEntity.MSSQL;
using System.Web.Mvc;
#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess
{
    public class UserDataAccessor
    {
        public static RequestResult GetUserOptions(List<Models.Shared.UserModel> UserList, string Term, bool IsInit)
        {
            RequestResult result = new RequestResult();

            try
            {
                var query = UserList.Select(x => new Models.Shared.UserModel
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
                        var itemList = Term.Split(',').ToList();

                        query = query.Where(x => itemList.Contains(x.ID)).ToList();
                    }
                    else
                    {
                        //var term = Term.ToLower();

                        query = query.Where(x => x.User.Contains(Term)).ToList();
                    }
                }

                result.ReturnData(query.Select(x => new SelectListItem { Value = x.ID, Text = x.User }).Distinct().ToList());
            }
            catch (Exception ex)
            {
                Error err = new Error(MethodInfo.GetCurrentMethod(), ex);

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
                using (DbEntities db = new DbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = (from u in db.User
                                 join o in db.Organization
                                 on u.OrganizationUniqueID equals o.UniqueID
                                 where downStreamOrganizationList.Contains(u.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(u.OrganizationUniqueID)
                                 select new
                                 {
                                     User = u,
                                     OrganizationDescription = o.Description
                                 }).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.User.Title.Contains(Parameters.Keyword) || x.User.ID.Contains(Parameters.Keyword) || x.User.Name.Contains(Parameters.Keyword));
                    }

                    var organization = OrganizationDataAccessor.GetOrganization(Parameters.OrganizationUniqueID);

                    var itemList = query.ToList();

                    var model = new GridViewModel()
                    {
                        Permission = Account.OrganizationPermission(Parameters.OrganizationUniqueID),
                        OrganizationUniqueID = Parameters.OrganizationUniqueID,
                        OrganizationDescription = organization.Description,
                        FullOrganizationDescription = organization.FullDescription,
                        ItemList = itemList.Select(x => new GridItem()
                        {
                            Permission = Account.OrganizationPermission(x.User.OrganizationUniqueID),
                            OrganizationDescription = x.OrganizationDescription,
                            Title = x.User.Title,
                            ID = x.User.ID,
                            Name = x.User.Name,
                            Email = x.User.Email,
                            IsMobileUser = x.User.IsMobileUser,
                            AuthGroupList = (from y in db.UserAuthGroup
                                             join a in db.AuthGroup
                                             on y.AuthGroupID equals a.ID
                                             where y.UserID == x.User.ID
                                             orderby a.ID
                                             select a.Name).ToList()
                        }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.Title).ThenBy(x => x.ID).ToList()
                    };

                    var ancestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(Parameters.OrganizationUniqueID);

                    var downStreamList = OrganizationDataAccessor.GetDownStreamOrganizationList(ancestorOrganizationUniqueID, true);

                    foreach (var downStream in downStreamList)
                    {
                        if (Account.EditableOrganizationUniqueIDList.Any(x => x == downStream))
                        {
                            model.MoveToTargetList.Add(new MoveToTarget()
                            {
                                UniqueID = downStream,
                                Description = OrganizationDataAccessor.GetOrganizationFullDescription(downStream),
                                Direction = Define.EnumMoveDirection.Down
                            });
                        }
                    }

                    model.MoveToTargetList = model.MoveToTargetList.OrderBy(x => x.Description).ToList();

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

        public static RequestResult GetDetailViewModel(string UserID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (DbEntities db = new DbEntities())
                {
                    var user = db.User.First(x => x.ID == UserID);

                    result.ReturnData(new DetailViewModel()
                    {
                        Permission = Account.OrganizationPermission(user.OrganizationUniqueID),
                        OrganizationUniqueID = user.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(user.OrganizationUniqueID),
                        ID = user.ID,
                        Name = user.Name,
                        UID = user.UID,
                        EMail = user.Email,
                        Title = user.Title,
                        IsMobileUser = user.IsMobileUser,
                        AuthGroupNameList = (from x in db.UserAuthGroup
                                             join a in db.AuthGroup
                                             on x.AuthGroupID equals a.ID
                                             where x.UserID == user.ID
                                             select a.Name).ToList()
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
                using (DbEntities db = new DbEntities())
                {
                    result.ReturnData(new CreateFormModel()
                    {
                        OrganizationUniqueID = OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(OrganizationUniqueID),
                        AuthGroupList = db.AuthGroup.Select(x => new Models.UserManagement.AuthGroup { ID = x.ID, Name = x.Name }).OrderBy(x => x.ID).ToList()
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
                using (DbEntities db = new DbEntities())
                {
                    var user = db.User.First(x => x.ID == UserID);

                    result.ReturnData(new CreateFormModel()
                    {
                        OrganizationUniqueID = user.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(user.OrganizationUniqueID),
                        AuthGroupList = db.AuthGroup.Select(x => new Models.UserManagement.AuthGroup { ID = x.ID, Name = x.Name }).OrderBy(x => x.ID).ToList(),
                        UserAuthGroupList = db.UserAuthGroup.Where(x => x.UserID == UserID).Select(x => x.AuthGroupID).ToList(),
                        FormInput = new FormInput()
                        {
                            Title = user.Title,
                            IsMobileUser = user.IsMobileUser
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
                using (DbEntities db = new DbEntities())
                {
                    var exists = db.User.FirstOrDefault(x => x.ID == Model.FormInput.ID);

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

                            var users = db.User.Where(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID)).ToList();

                            if (users.Count + 1 > userLimit.Users)
                            {
                                exceedsLimit = true;

                                result.ReturnFailedMessage(string.Format(Resources.Resource.ExceedsUserLimit, organization.Description, userLimit.Users));
                            }
                            else
                            {
                                if (Model.FormInput.IsMobileUser && users.Count(x => x.IsMobileUser) + 1 > userLimit.MobileUsers)
                                {
                                    exceedsLimit = true;

                                    result.ReturnFailedMessage(string.Format(Resources.Resource.ExceedsMobileUserLimit, organization.Description, userLimit.MobileUsers));
                                }
                            }
                        }

                        if (!exceedsLimit)
                        {
                            db.User.Add(new User()
                            {
                                OrganizationUniqueID = Model.OrganizationUniqueID,
                                ID = Model.FormInput.ID,
                                Name = Model.FormInput.Name,
                                Password = Model.FormInput.ID,
                                Title = Model.FormInput.Title,
                                Email = Model.FormInput.EMail,
                                UID = Model.FormInput.UID,
                                IsMobileUser = Model.FormInput.IsMobileUser,
                                LastModifyTime = DateTime.Now
                            });

                            db.UserAuthGroup.AddRange(Model.FormInput.AuthGroupList.Select(x => new UserAuthGroup
                            {
                                UserID = Model.FormInput.ID,
                                AuthGroupID = x
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
                using (DbEntities db = new DbEntities())
                {
                    var user = db.User.First(x => x.ID == UserID);

                    result.ReturnData(new EditFormModel()
                    {
                        UserID = user.ID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(user.OrganizationUniqueID),
                        AuthGroupList = db.AuthGroup.Select(x => new Models.UserManagement.AuthGroup { ID = x.ID, Name = x.Name }).OrderBy(x => x.ID).ToList(),
                        UserAuthGroupList = db.UserAuthGroup.Where(x => x.UserID == UserID).Select(x => x.AuthGroupID).ToList(),
                        FormInput = new FormInput()
                        {
                            ID = user.ID,
                            Name = user.Name,
                            Title = user.Title,
                            EMail = user.Email,
                            UID = user.UID,
                            IsMobileUser = user.IsMobileUser
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
                using (DbEntities db = new DbEntities())
                {
                    var user = db.User.First(x => x.ID == Model.UserID);

                    bool exceedsLimit = false;

                    if (Model.FormInput.IsMobileUser)
                    {
                        var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(user.OrganizationUniqueID, true);

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

                            var mobileUsers = db.User.Where(x => x.ID != user.ID && x.IsMobileUser && downStreamOrganizationList.Contains(x.OrganizationUniqueID)).ToList();

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
                        user.Name = Model.FormInput.Name;
                        user.Title = Model.FormInput.Title;
                        user.Email = Model.FormInput.EMail;
                        user.UID = Model.FormInput.UID;
                        user.IsMobileUser = Model.FormInput.IsMobileUser;
                        user.LastModifyTime = DateTime.Now;

                        db.SaveChanges();
                        #endregion

                        #region UserAuthGroup
                        #region Delete
                        db.UserAuthGroup.RemoveRange(db.UserAuthGroup.Where(x => x.UserID == Model.UserID).ToList());

                        db.SaveChanges();
                        #endregion

                        #region Insert
                        db.UserAuthGroup.AddRange(Model.FormInput.AuthGroupList.Select(x => new UserAuthGroup
                        {
                            UserID = Model.UserID,
                            AuthGroupID = x
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
                using (DbEntities db = new DbEntities())
                {
                    var user = db.User.First(x => x.ID == UserID);

                    user.Password = user.ID;

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
                using (DbEntities db = new DbEntities())
                {
                    var user = db.User.First(x => x.ID == UserID);

                    user.UID = UID;

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
                using (DbEntities db = new DbEntities())
                {
                    foreach (var userID in SelectedList)
                    {
                        db.User.Remove(db.User.First(x => x.ID == userID));
                        db.UserAuthGroup.RemoveRange(db.UserAuthGroup.Where(x => x.UserID == userID).ToList());

                        var engContactList = db.EmgContact.Where(x => x.UserID == userID).Select(x => x.UniqueID).ToList();

                        foreach (var emgContact in engContactList)
                        {
                            db.EmgContact.Remove(db.EmgContact.First(x => x.UniqueID == emgContact));

                            db.EmgContactTel.RemoveRange(db.EmgContactTel.Where(x => x.EmgContactUniqueID == emgContact).ToList());
                        }
                    }

                    db.SaveChanges();
                }

#if EquipmentPatrol || EquipmentMaintenance
                using (DbEntity.MSSQL.EquipmentMaintenance.EDbEntities db = new DbEntity.MSSQL.EquipmentMaintenance.EDbEntities())
                    {
                        foreach (var userID in SelectedList)
                        {
                            db.JobUser.RemoveRange(db.JobUser.Where(x => x.UserID == userID).ToList());

                            db.MJobUser.RemoveRange(db.MJobUser.Where(x => x.UserID == userID).ToList());
                        }

                        db.SaveChanges();
                    }
#endif

#if GuardPatrol
                using (DbEntity.MSSQL.GuardPatrol.GDbEntities db = new DbEntity.MSSQL.GuardPatrol.GDbEntities())
                    {
                        foreach (var userID in SelectedList)
                        {
                            db.JobUser.RemoveRange(db.JobUser.Where(x => x.UserID == userID).ToList());
                        }

                        db.SaveChanges();
                    }
#endif
                   
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

        public static RequestResult Move(string OrganizationUniqueID, List<string> SelectedList)
        {
            RequestResult result = new RequestResult();

            try
            {
#if !DEBUG
                using (TransactionScope trans = new TransactionScope())
                {
#endif
                using (DbEntities db = new DbEntities())
                {
                    foreach (var userID in SelectedList)
                    {
                        db.User.First(x => x.ID == userID).OrganizationUniqueID = OrganizationUniqueID;
                    }

                    db.SaveChanges();
                }

#if EquipmentPatrol || EquipmentMaintenance
                using (DbEntity.MSSQL.EquipmentMaintenance.EDbEntities db = new DbEntity.MSSQL.EquipmentMaintenance.EDbEntities())
                    {
                        foreach (var userID in SelectedList)
                        {
                            db.JobUser.RemoveRange(db.JobUser.Where(x => x.UserID == userID).ToList());
                        }

                        db.SaveChanges();
                    }
#endif

#if EquipmentMaintenance
                using (DbEntity.MSSQL.EquipmentMaintenance.EDbEntities db = new DbEntity.MSSQL.EquipmentMaintenance.EDbEntities())
                    {
                        foreach (var userID in SelectedList)
                        {
                            db.MJobUser.RemoveRange(db.MJobUser.Where(x => x.UserID == userID).ToList());
                        }

                        db.SaveChanges();
                    }
#endif

#if GuardPatrol
                using (DbEntity.MSSQL.GuardPatrol.GDbEntities db = new DbEntity.MSSQL.GuardPatrol.GDbEntities())
                    {
                        foreach (var userID in SelectedList)
                        {
                            db.JobUser.RemoveRange(db.JobUser.Where(x => x.UserID == userID).ToList());
                        }

                        db.SaveChanges();
                    }
#endif

#if !DEBUG
                    trans.Complete();
                }
#endif

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Move, Resources.Resource.User, Resources.Resource.Success));
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

                using (DbEntities db = new DbEntities())
                {
                    var organization = OrganizationList.First(x => x.UniqueID == Account.OrganizationUniqueID);
                    //var organization = OrganizationList.First(x => x.UniqueID == AncestorOrganizationUniqueID);

                    var treeItem = new TreeItem() { Title = organization.Description };

                    attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                    attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                    attributes[Define.EnumTreeAttribute.UserID] = string.Empty;

                    foreach (var attribute in attributes)
                    {
                        treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                    }

                    var haveDownStreamOrganization = OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID));

                    bool haveUser = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.User.Any(x => x.OrganizationUniqueID == organization.UniqueID);

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

                using (DbEntities db = new DbEntities())
                {
                    if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                    {
                        var userList = db.User.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

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

                        bool haveUser = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.User.Any(x => x.OrganizationUniqueID == organization.UniqueID);

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

                using (DbEntities db = new DbEntities())
                {
                    var userList = db.User.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

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

                    var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && Account.QueryableOrganizationUniqueIDList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                    foreach (var organization in organizationList)
                    {
                        var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organization.UniqueID, true);

                        if (db.User.Any(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID)))
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
                using (DbEntities db = new DbEntities())
                {
                    userModel = (from u in db.User
                                 join o in db.Organization
                                 on u.OrganizationUniqueID equals o.UniqueID
                                 where u.ID == UserID
                                 select new UserModel
                                 {
                                     OrganizationDescription = o.Description,
                                     ID = u.ID,
                                     Name = u.Name,
                                     Email  =u.Email
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
