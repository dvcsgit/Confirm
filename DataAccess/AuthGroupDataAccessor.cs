using System;
using System.Linq;
using System.Reflection;
using System.Transactions;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using Models.Shared;
using Models.Authenticated;
using Models.AuthGroupManagement;
using DbEntity.MSSQL;

namespace DataAccess
{
    public class AuthGroupDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (DbEntities db = new DbEntities())
                {
                    var query = db.AuthGroup.AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.ID.Contains(Parameters.Keyword) || x.Name.Contains(Parameters.Keyword));
                    }

                    result.ReturnData(new GridViewModel()
                    {
                        Parameters = Parameters,
                        ItemList = query.Select(x => new GridItem()
                        {
                            AuthGroupID = x.ID,
                            AuthGroupName = x.Name
                        }).OrderBy(x => x.AuthGroupID).ToList()
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

        public static RequestResult GetDetailViewModel(string AuthGroupID)
        {
            RequestResult result = new RequestResult();

            try
            {
                result = GetWebPermissionList();

                if (result.IsSuccess)
                {
                    using (DbEntities db = new DbEntities())
                    {
                        var authGroup = db.AuthGroup.First(x => x.ID == AuthGroupID);

                        result.ReturnData(new DetailViewModel()
                        {
                            AuthGroupID = authGroup.ID,
                            AuthGroupName = authGroup.Name,
                            WebPermissionFunction = new WebPermissionFunctionModel()
                            {
                                WebPermissionList = result.Data as List<WebPermissionModel>,
                                AuthGroupWebPermissionFunctionList = db.AuthGroupWebPermissionFunction.Where(x => x.AuthGroupID == authGroup.ID).Select(x => new Models.AuthGroupManagement.AuthGroupWebPermissionFunction { AuthGroupID = x.AuthGroupID, WebPermissionID = x.WebPermissionID, WebFunctionID = x.WebFunctionID }).ToList()
                            },
                            UserList = (from x in db.UserAuthGroup
                                        join u in db.User
                                        on x.UserID equals u.ID
                                        join o in db.Organization
                                        on u.OrganizationUniqueID equals o.UniqueID
                                        where x.AuthGroupID == AuthGroupID
                                        select new Models.AuthGroupManagement.UserModel
                                        {
                                            OrganizationDescription = o.Description,
                                            ID = u.ID,
                                            Name = u.Name
                                        }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.ID).ToList()
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

        public static RequestResult GetCreateFormModel(Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                result = GetWebPermissionList();

                if (result.IsSuccess)
                {
                    result.ReturnData(new CreateFormModel()
                    {
                        AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(Account.OrganizationUniqueID),
                        WebPermissionFunction = new WebPermissionFunctionModel()
                        {
                            WebPermissionList = result.Data as List<WebPermissionModel>
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

        public static RequestResult GetCopyFormModel(string AuthGroupID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                result = GetWebPermissionList();

                if (result.IsSuccess)
                {
                    using (DbEntities db = new DbEntities())
                    {
                        var authGroup = db.AuthGroup.First(x => x.ID == AuthGroupID);

                        result.ReturnData(new CreateFormModel()
                        {
                            AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(Account.OrganizationUniqueID),
                            WebPermissionFunction = new WebPermissionFunctionModel()
                            {
                                WebPermissionList = result.Data as List<WebPermissionModel>,
                                AuthGroupWebPermissionFunctionList = db.AuthGroupWebPermissionFunction.Where(x => x.AuthGroupID == authGroup.ID).Select(x => new Models.AuthGroupManagement.AuthGroupWebPermissionFunction { AuthGroupID = x.AuthGroupID, WebPermissionID = x.WebPermissionID, WebFunctionID = x.WebFunctionID }).ToList()
                            },
                            UserList = (from x in db.UserAuthGroup
                                        join u in db.User
                                        on x.UserID equals u.ID
                                        join o in db.Organization
                                        on u.OrganizationUniqueID equals o.UniqueID
                                        where x.AuthGroupID == AuthGroupID
                                        select new Models.AuthGroupManagement.UserModel
                                        {
                                            OrganizationDescription = o.Description,
                                            ID = u.ID,
                                            Name = u.Name
                                        }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.ID).ToList()
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

        public static RequestResult Create(CreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (DbEntities db = new DbEntities())
                {
                    var exists = db.AuthGroup.FirstOrDefault(x => x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
                        db.AuthGroup.Add(new AuthGroup()
                        {
                            ID = Model.FormInput.ID,
                            Name = Model.FormInput.Name
                        });

                        db.AuthGroupWebPermissionFunction.AddRange(Model.FormInput.WebPermissionFunctionList.Select(x => new DbEntity.MSSQL.AuthGroupWebPermissionFunction
                        {
                            AuthGroupID = Model.FormInput.ID,
                            WebPermissionID = x.WebPermissionID,
                            WebFunctionID = x.WebFunctionID
                        }).ToList());

                        db.UserAuthGroup.AddRange(Model.UserList.Select(x => new UserAuthGroup
                        {
                            AuthGroupID = Model.FormInput.ID,
                            UserID = x.ID
                        }).ToList());

                        db.SaveChanges();

                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.AuthGroup, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.AuthGroupID, Resources.Resource.Exists));
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

        public static RequestResult GetEditFormModel(string AuthGroupID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                result = GetWebPermissionList();

                if (result.IsSuccess)
                {
                    using (DbEntities db = new DbEntities())
                    {
                        var authGroup = db.AuthGroup.First(x => x.ID == AuthGroupID);

                        result.ReturnData(new EditFormModel()
                        {
                            AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(Account.OrganizationUniqueID),
                            AuthGroupID = authGroup.ID,
                            WebPermissionFunction = new WebPermissionFunctionModel()
                            {
                                WebPermissionList = result.Data as List<WebPermissionModel>,
                                AuthGroupWebPermissionFunctionList = db.AuthGroupWebPermissionFunction.Where(x => x.AuthGroupID == authGroup.ID).Select(x => new Models.AuthGroupManagement.AuthGroupWebPermissionFunction { AuthGroupID = x.AuthGroupID, WebPermissionID = x.WebPermissionID, WebFunctionID = x.WebFunctionID }).ToList()
                            },
                            FormInput = new FormInput()
                            {
                                ID = authGroup.ID,
                                Name = authGroup.Name
                            },
                            UserList = (from x in db.UserAuthGroup
                                        join u in db.User
                                        on x.UserID equals u.ID
                                        join o in db.Organization
                                        on u.OrganizationUniqueID equals o.UniqueID
                                        where x.AuthGroupID == AuthGroupID
                                        select new Models.AuthGroupManagement.UserModel
                                        {
                                            OrganizationDescription = o.Description,
                                            ID = u.ID,
                                            Name = u.Name
                                        }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.ID).ToList()
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
                using (DbEntities db = new DbEntities())
                {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                    #region AuthGroup
                    var authGroup = db.AuthGroup.First(x => x.ID == Model.AuthGroupID);

                    authGroup.Name = Model.FormInput.Name;

                    db.SaveChanges();
                    #endregion

                    #region AuthGroupWebPermissionFunction
                    #region Delete
                    db.AuthGroupWebPermissionFunction.RemoveRange(db.AuthGroupWebPermissionFunction.Where(x => x.AuthGroupID == authGroup.ID).ToList());

                    db.SaveChanges();
                    #endregion

                    #region Insert
                    db.AuthGroupWebPermissionFunction.AddRange(Model.FormInput.WebPermissionFunctionList.Select(x => new DbEntity.MSSQL.AuthGroupWebPermissionFunction
                    {
                        AuthGroupID = authGroup.ID,
                        WebPermissionID = x.WebPermissionID,
                        WebFunctionID = x.WebFunctionID
                    }).ToList());

                    db.SaveChanges();
                    #endregion
                    #endregion

                    #region UserAuthGroup
                    #region Delete
                    db.UserAuthGroup.RemoveRange(db.UserAuthGroup.Where(x => x.AuthGroupID == authGroup.ID).ToList());

                    db.SaveChanges();
                    #endregion

                    #region Insert
                    db.UserAuthGroup.AddRange(Model.UserList.Select(x => new UserAuthGroup
                    {
                        AuthGroupID = authGroup.ID,
                        UserID = x.ID
                    }).ToList());

                    db.SaveChanges();
                    #endregion
                    #endregion
#if !DEBUG
                        trans.Complete();
                    }
#endif

                    result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.AuthGroup, Resources.Resource.Success));
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

        public static RequestResult Delete(string AuthGroupID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (DbEntities db = new DbEntities())
                {
                    //AuthGroup
                    db.AuthGroup.Remove(db.AuthGroup.First(x => x.ID == AuthGroupID));

                    //AuthGroupWebPermissionFunction
                    db.AuthGroupWebPermissionFunction.RemoveRange(db.AuthGroupWebPermissionFunction.Where(x => x.AuthGroupID == AuthGroupID).ToList());

                    //UserAuthGroup
                    db.UserAuthGroup.RemoveRange(db.UserAuthGroup.Where(x => x.AuthGroupID == AuthGroupID).ToList());

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.AuthGroup, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                Error err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        private static RequestResult GetWebPermissionList()
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = new List<WebPermissionModel>();

                using (DbEntities db = new DbEntities())
                {
                    var webPermissionFunctionList = db.WebPermissionFunction.ToList();

                    var ancestorList = db.WebPermission.Where(x => x.ParentID == "*").OrderBy(x => x.Seq).ToList();

                    foreach (var ancestor in ancestorList)
                    {
                        var item = new WebPermissionModel()
                        {
                            ID = ancestor.ID,
                            Description = db.WebPermissionDescription.Where(x => x.WebPermissionID == ancestor.ID).ToDictionary(x => x.Language, x => x.Description),
                            SubItemList = new List<WebPermissionModel>(),
                            WebFunctionList = null
                        };

                        var parentList = db.WebPermission.Where(x => x.ParentID == ancestor.ID).OrderBy(x => x.Seq).ToList();

                        foreach (var parent in parentList)
                        {
                            if (!string.IsNullOrEmpty(parent.Controller))
                            {
                                item.SubItemList.Add(new WebPermissionModel()
                                {
                                    ID = parent.ID,
                                    Description = db.WebPermissionDescription.Where(x => x.WebPermissionID == parent.ID).ToDictionary(x => x.Language, x => x.Description),
                                    SubItemList = null,
                                    WebFunctionList = (from x in webPermissionFunctionList
                                                       join f in db.WebFunction
                                                       on x.WebFunctionID equals f.ID
                                                       where x.WebPermissionID == parent.ID
                                                       select new WebFunctionModel
                                                       {
                                                           ID = f.ID,
                                                           Description = db.WebFunctionDescription.Where(y => y.WebFunctionID == f.ID).ToDictionary(y => y.Language, y => y.Description)
                                                       }).ToList()
                                });
                            }
                            else
                            {
                                var permissionList = db.WebPermission.Where(x => x.ParentID == parent.ID).OrderBy(x => x.Seq).ToList();

                                item.SubItemList.AddRange(permissionList.Select(p => new WebPermissionModel
                                {
                                    ID = p.ID,
                                    Description = db.WebPermissionDescription.Where(d => d.WebPermissionID == p.ID).ToDictionary(d => d.Language, d => d.Description),
                                    SubItemList = null,
                                    WebFunctionList = (from x in webPermissionFunctionList
                                                       join f in db.WebFunction
                                                       on x.WebFunctionID equals f.ID
                                                       where x.WebPermissionID == p.ID
                                                       select new WebFunctionModel
                                                       {
                                                           ID = f.ID,
                                                           Description = db.WebFunctionDescription.Where(d => d.WebFunctionID == f.ID).ToDictionary(d => d.Language, d => d.Description)
                                                       }).ToList()
                                }).ToList());
                            }
                        }

                        if (item.SubItemList != null && item.SubItemList.Count > 0)
                        {
                            itemList.Add(item);
                        }
                    }
                }

                result.ReturnData(itemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult AddUser(List<Models.AuthGroupManagement.UserModel> UserList, List<string> SelectedList)
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
                                              select new Models.AuthGroupManagement.UserModel
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
                                    UserList.Add(new Models.AuthGroupManagement.UserModel()
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
    }
}
