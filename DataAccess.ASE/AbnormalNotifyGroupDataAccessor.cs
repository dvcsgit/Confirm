using DbEntity.ASE;
using Models.ASE.AbnormalNotifyGroup;
using Models.ASE.Shared;
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

namespace DataAccess.ASE
{
    public class AbnormalNotifyGroupDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new GridViewModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = db.FAC_ABNORMALNOTIFYGROUP.AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.KeyWord))
                    {
                        query = query.Where(x => x.DESCRIPTION.Contains(Parameters.KeyWord));
                    }

                    model.ItemList = query.ToList().Select(x => new GridItem
                    {
                        UniqueID = x.UNIQUEID,
                        GroupType = x.GROUPTYPE,
                        Description = x.DESCRIPTION,
                        CanDelete = !db.FAC_ABNORMALNOTIFYFORMGROUP.Any(f => f.GROUPUNIQUEID == x.UNIQUEID)
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = db.FAC_ABNORMALNOTIFYGROUP.FirstOrDefault(x => x.DESCRIPTION == Model.FormInput.Description);

                    if (query == null)
                    {
                        var uniqueID = Guid.NewGuid().ToString();

                        db.FAC_ABNORMALNOTIFYGROUP.Add(new FAC_ABNORMALNOTIFYGROUP()
                        {
                            UNIQUEID = uniqueID,
                            GROUPTYPE = Model.FormInput.GroupType,
                            DESCRIPTION = Model.FormInput.Description
                        });

                        db.FAC_ABNORMALNOTIFYGROUPUSER.AddRange(Model.UserList.Select(x => new FAC_ABNORMALNOTIFYGROUPUSER
                        {
                            GROUPUNIQUEID = uniqueID,
                            USERID = x.ID
                        }).ToList());

                        db.FAC_ABNORMALNOTIFYGROUPCCUSER.AddRange(Model.CCUserList.Select(x => new FAC_ABNORMALNOTIFYGROUPCCUSER
                        {
                            GROUPUNIQUEID = uniqueID,
                            USERID = x.ID
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var group = db.FAC_ABNORMALNOTIFYGROUP.First(x => x.UNIQUEID == UniqueID);

                    model = new DetailViewModel()
                    {
                        UniqueID = group.UNIQUEID,
                        GroupType = group.GROUPTYPE,
                        Description = group.DESCRIPTION,
                        CanDelete = !db.FAC_ABNORMALNOTIFYFORMGROUP.Any(x => x.GROUPUNIQUEID == group.UNIQUEID)
                    };

                    var userList = db.FAC_ABNORMALNOTIFYGROUPUSER.Where(x => x.GROUPUNIQUEID == group.UNIQUEID).Select(x => x.USERID).OrderBy(x => x).ToList();

                    foreach (var user in userList)
                    {
                        var account = UserList.FirstOrDefault(x => x.ID == user);

                        model.UserList.Add(new Models.ASE.Shared.ASEUserModel()
                        {
                            ID = user,
                            Email = account != null ? account.Email : string.Empty,
                            Name = account != null ? account.Name : string.Empty,
                            OrganizationDescription = account != null ? account.OrganizationDescription : string.Empty
                        });
                    }

                    var ccUserList = db.FAC_ABNORMALNOTIFYGROUPCCUSER.Where(x => x.GROUPUNIQUEID == group.UNIQUEID).Select(x => x.USERID).OrderBy(x => x).ToList();

                    foreach (var user in ccUserList)
                    {
                        var account = UserList.FirstOrDefault(x => x.ID == user);

                        model.CCUserList.Add(new Models.ASE.Shared.ASEUserModel()
                        {
                            ID = user,
                            Email = account != null ? account.Email : string.Empty,
                            Name = account != null ? account.Name : string.Empty,
                            OrganizationDescription = account != null ? account.OrganizationDescription : string.Empty
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

        public static RequestResult GetEditFormModel(string UniqueID, Models.Authenticated.Account Account, List<Models.Shared.UserModel> UserList)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new EditFormModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var group = db.FAC_ABNORMALNOTIFYGROUP.First(x => x.UNIQUEID == UniqueID);

                    model = new EditFormModel()
                    {
                        UniqueID = group.UNIQUEID,
                        GroupType = group.GROUPTYPE,
                        FormInput = new FormInput()
                        {
                            Description = group.DESCRIPTION
                        }
                    };

                    var userList = db.FAC_ABNORMALNOTIFYGROUPUSER.Where(x => x.GROUPUNIQUEID == group.UNIQUEID).Select(x => x.USERID).OrderBy(x => x).ToList();

                    foreach (var user in userList)
                    {
                        var account = UserList.FirstOrDefault(x => x.ID == user);

                        model.UserList.Add(new Models.ASE.Shared.ASEUserModel()
                        {
                            ID = user,
                            Email = account != null ? account.Email : string.Empty,
                            Name = account != null ? account.Name : string.Empty,
                            OrganizationDescription = account != null ? account.OrganizationDescription : string.Empty
                        });
                    }

                    var ccUserList = db.FAC_ABNORMALNOTIFYGROUPCCUSER.Where(x => x.GROUPUNIQUEID == group.UNIQUEID).Select(x => x.USERID).OrderBy(x => x).ToList();

                    foreach (var user in ccUserList)
                    {
                        var account = UserList.FirstOrDefault(x => x.ID == user);

                        model.CCUserList.Add(new Models.ASE.Shared.ASEUserModel()
                        {
                            ID = user,
                            Email = account != null ? account.Email : string.Empty,
                            Name = account != null ? account.Name : string.Empty,
                            OrganizationDescription = account != null ? account.OrganizationDescription : string.Empty
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

        public static RequestResult Edit(EditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var group = db.FAC_ABNORMALNOTIFYGROUP.First(x => x.UNIQUEID == Model.UniqueID);

                    var query = db.FAC_ABNORMALNOTIFYGROUP.FirstOrDefault(x => x.UNIQUEID != group.UNIQUEID && x.DESCRIPTION == Model.FormInput.Description);

                    if (query == null)
                    {
#if !DEBUG
                        using (TransactionScope trans = new TransactionScope())
                        {
#endif
                            group.DESCRIPTION = Model.FormInput.Description;

                            db.SaveChanges();

                            db.FAC_ABNORMALNOTIFYGROUPUSER.RemoveRange(db.FAC_ABNORMALNOTIFYGROUPUSER.Where(x => x.GROUPUNIQUEID == group.UNIQUEID).ToList());

                            db.SaveChanges();

                            db.FAC_ABNORMALNOTIFYGROUPUSER.AddRange(Model.UserList.Select(x => new FAC_ABNORMALNOTIFYGROUPUSER
                            {
                                GROUPUNIQUEID = group.UNIQUEID,
                                USERID = x.ID
                            }).ToList());

                            db.SaveChanges();

                            db.FAC_ABNORMALNOTIFYGROUPCCUSER.RemoveRange(db.FAC_ABNORMALNOTIFYGROUPCCUSER.Where(x => x.GROUPUNIQUEID == group.UNIQUEID).ToList());

                            db.SaveChanges();

                            db.FAC_ABNORMALNOTIFYGROUPCCUSER.AddRange(Model.CCUserList.Select(x => new FAC_ABNORMALNOTIFYGROUPCCUSER
                            {
                                GROUPUNIQUEID = group.UNIQUEID,
                                USERID = x.ID
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    db.FAC_ABNORMALNOTIFYGROUP.Remove(db.FAC_ABNORMALNOTIFYGROUP.FirstOrDefault(x => x.UNIQUEID == UniqueID));

                    db.FAC_ABNORMALNOTIFYGROUPUSER.RemoveRange(db.FAC_ABNORMALNOTIFYGROUPUSER.Where(x => x.GROUPUNIQUEID == UniqueID).ToList());

                    db.FAC_ABNORMALNOTIFYGROUPCCUSER.RemoveRange(db.FAC_ABNORMALNOTIFYGROUPCCUSER.Where(x => x.GROUPUNIQUEID == UniqueID).ToList());

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

        public static RequestResult AddUser(List<Models.ASE.Shared.ASEUserModel> UserList, List<string> SelectedList)
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
                                              select new Models.ASE.Shared.ASEUserModel
                                              {
                                                  ID = u.ID,
                                                  Name = u.NAME,
                                                  Email = u.EMAIL,
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
                                                Email = u.EMAIL,
                                                OrganizationDescription = o.DESCRIPTION
                                            }).ToList();

                            foreach (var user in userList)
                            {
                                if (!UserList.Any(x => x.ID == user.ID))
                                {
                                    UserList.Add(new Models.ASE.Shared.ASEUserModel()
                                    {
                                        ID = user.ID,
                                        Name = user.Name,
                                        Email = user.Email,
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
    }
}
