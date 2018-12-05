using DbEntity.MSSQL;
using DbEntity.MSSQL.AbnormalNotify;
using Models.AbnormalNotify.AbnormalNotifyGroup;
using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Utility;
using Utility.Models;

namespace DataAccess.AbnormalNotify
{
    public class AbnormalNotifyGroupDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new GridViewModel();

                using (ADbEntities db = new ADbEntities())
                {
                    var query = db.ANGroup.AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.KeyWord))
                    {
                        query = query.Where(x => x.Description.Contains(Parameters.KeyWord));
                    }

                    model.ItemList = query.ToList().Select(x => new GridItem
                    {
                        UniqueID = x.UniqueID,
                        Description = x.Description,
                        CanDelete = !db.ANFormGroup.Any(f => f.GroupUniqueID == x.UniqueID)
                    }).ToList();
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

        public static RequestResult Create(CreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ADbEntities db = new ADbEntities())
                {
                    var query = db.ANGroup.FirstOrDefault(x => x.Description == Model.FormInput.Description);

                    if (query == null)
                    {
                        var uniqueID = Guid.NewGuid().ToString();

                        db.ANGroup.Add(new ANGroup()
                        {
                            UniqueID = uniqueID,
                            Description = Model.FormInput.Description
                        });

                        db.ANGroupUser.AddRange(Model.UserList.Select(x => new ANGroupUser
                        {
                            GroupUniqueID = uniqueID,
                            UserID = x.ID
                        }).ToList());

                        db.ANGroupCCUser.AddRange(Model.CCUserList.Select(x => new ANGroupCCUser
                        {
                            GroupUniqueID = uniqueID,
                            UserID = x.ID
                        }).ToList());

                        db.SaveChanges();

                        result.ReturnSuccessMessage("新增通知群組成功");
                    }
                    else
                    {
                        result.ReturnFailedMessage("通知群組名稱已存在");
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

        public static RequestResult GetDetailViewModel(string UniqueID, List<Models.Shared.UserModel> UserList)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new DetailViewModel();

                using (ADbEntities db = new ADbEntities())
                {
                    var group = db.ANGroup.First(x => x.UniqueID == UniqueID);

                    model = new DetailViewModel()
                    {
                        UniqueID = group.UniqueID,
                        Description = group.Description,
                        CanDelete = !db.ANFormGroup.Any(x => x.GroupUniqueID == group.UniqueID)
                    };

                    var userList = db.ANGroupUser.Where(x => x.GroupUniqueID == group.UniqueID).Select(x => x.UserID).OrderBy(x => x).ToList();

                    foreach (var user in userList)
                    {
                        var account = UserList.FirstOrDefault(x => x.ID == user);

                        if (account != null)
                        {
                            model.UserList.Add(account);
                        }
                    }

                    var ccUserList = db.ANGroupCCUser.Where(x => x.GroupUniqueID == group.UniqueID).Select(x => x.UserID).OrderBy(x => x).ToList();

                    foreach (var user in ccUserList)
                    {
                        var account = UserList.FirstOrDefault(x => x.ID == user);

                        if (account != null)
                        {
                            model.CCUserList.Add(account);
                        }
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

        public static RequestResult GetEditFormModel(string UniqueID, Models.Authenticated.Account Account, List<Models.Shared.UserModel> UserList)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new EditFormModel();

                using (ADbEntities db = new ADbEntities())
                {
                    var group = db.ANGroup.First(x => x.UniqueID == UniqueID);

                    model = new EditFormModel()
                    {
                        UniqueID = group.UniqueID,
                        FormInput = new FormInput()
                        {
                            Description = group.Description
                        }
                    };

                    var userList = db.ANGroupUser.Where(x => x.GroupUniqueID == group.UniqueID).Select(x => x.UserID).OrderBy(x => x).ToList();

                    foreach (var user in userList)
                    {
                        var account = UserList.FirstOrDefault(x => x.ID == user);

                        if (account != null)
                        {
                            model.UserList.Add(account);
                        }
                    }

                    var ccUserList = db.ANGroupCCUser.Where(x => x.GroupUniqueID == group.UniqueID).Select(x => x.UserID).OrderBy(x => x).ToList();

                    foreach (var user in ccUserList)
                    {
                        var account = UserList.FirstOrDefault(x => x.ID == user);

                        if (account != null)
                        {
                            model.CCUserList.Add(account);
                        }
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

        public static RequestResult Edit(EditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {

                using (ADbEntities db = new ADbEntities())
                {
                    var group = db.ANGroup.First(x => x.UniqueID == Model.UniqueID);

                    var query = db.ANGroup.FirstOrDefault(x => x.UniqueID != group.UniqueID && x.Description == Model.FormInput.Description);

                    if (query == null)
                    {
#if !DEBUG
                        using (TransactionScope trans = new TransactionScope())
                        {
#endif
                            group.Description = Model.FormInput.Description;

                            db.SaveChanges();

                            db.ANGroupUser.RemoveRange(db.ANGroupUser.Where(x => x.GroupUniqueID == group.UniqueID).ToList());

                            db.SaveChanges();

                            db.ANGroupUser.AddRange(Model.UserList.Select(x => new ANGroupUser
                            {
                                GroupUniqueID = group.UniqueID,
                                UserID = x.ID
                            }).ToList());

                            db.SaveChanges();

                            db.ANGroupCCUser.RemoveRange(db.ANGroupCCUser.Where(x => x.GroupUniqueID == group.UniqueID).ToList());

                            db.SaveChanges();

                            db.ANGroupCCUser.AddRange(Model.CCUserList.Select(x => new ANGroupCCUser
                            {
                                GroupUniqueID = group.UniqueID,
                                UserID = x.ID
                            }).ToList());

                            db.SaveChanges();

#if !DEBUG
                            trans.Complete();
                        }
#endif

                        result.ReturnSuccessMessage("編輯通知群組成功");
                    }
                    else
                    {
                        result.ReturnFailedMessage("通知群組名稱已存在");
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
                using (ADbEntities db = new ADbEntities())
                {
                    db.ANGroup.Remove(db.ANGroup.FirstOrDefault(x => x.UniqueID == UniqueID));

                    db.ANGroupUser.RemoveRange(db.ANGroupUser.Where(x => x.GroupUniqueID == UniqueID).ToList());

                    db.ANGroupCCUser.RemoveRange(db.ANGroupCCUser.Where(x => x.GroupUniqueID == UniqueID).ToList());

                    db.SaveChanges();

                    result.ReturnSuccessMessage("刪除通知群組成功");
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
