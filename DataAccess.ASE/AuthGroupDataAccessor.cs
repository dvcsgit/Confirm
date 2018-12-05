using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using Models.Shared;
using Models.Authenticated;
using Models.AuthGroupManagement;
using DbEntity.ASE;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.ASE
{
    public class AuthGroupDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = db.AUTHGROUP.AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.ID.Contains(Parameters.Keyword) || x.NAME.Contains(Parameters.Keyword));
                    }

                    result.ReturnData(new GridViewModel()
                    {
                        Parameters = Parameters,
                        ItemList = query.Select(x => new GridItem()
                        {
                            AuthGroupID = x.ID,
                            AuthGroupName = x.NAME
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
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var authGroup = db.AUTHGROUP.First(x => x.ID == AuthGroupID);

                        result.ReturnData(new DetailViewModel()
                        {
                            AuthGroupID = authGroup.ID,
                            AuthGroupName = authGroup.NAME,
                            WebPermissionFunction = new WebPermissionFunctionModel()
                            {
                                WebPermissionList = result.Data as List<WebPermissionModel>,
                                AuthGroupWebPermissionFunctionList = db.AUTHGROUPWEBPERMISSIONFUNCTION.Where(x => x.AUTHGROUPID == authGroup.ID).Select(x => new Models.AuthGroupManagement.AuthGroupWebPermissionFunction { AuthGroupID = x.AUTHGROUPID, WebPermissionID = x.WEBPERMISSIONID, WebFunctionID = x.WEBFUNCTIONID }).ToList()
                            },
                            UserList = (from x in db.USERAUTHGROUP
                                        join u in db.ACCOUNT
                                        on x.USERID equals u.ID
                                        join o in db.ORGANIZATION
                                        on u.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                        where x.AUTHGROUPID == AuthGroupID
                                        select new Models.AuthGroupManagement.UserModel
                                        {
                                            OrganizationDescription = o.DESCRIPTION,
                                            ID = u.ID,
                                            Name = u.NAME
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
                        AncestorOrganizationUniqueID=OrganizationDataAccessor.GetAncestorOrganizationUniqueID(Account.OrganizationUniqueID),
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
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var authGroup = db.AUTHGROUP.First(x => x.ID == AuthGroupID);

                        result.ReturnData(new CreateFormModel()
                        {
                            AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(Account.OrganizationUniqueID),
                            WebPermissionFunction = new WebPermissionFunctionModel()
                            {
                                WebPermissionList = result.Data as List<WebPermissionModel>,
                                AuthGroupWebPermissionFunctionList = db.AUTHGROUPWEBPERMISSIONFUNCTION.Where(x => x.AUTHGROUPID == authGroup.ID).Select(x => new Models.AuthGroupManagement.AuthGroupWebPermissionFunction { AuthGroupID = x.AUTHGROUPID, WebPermissionID = x.WEBPERMISSIONID, WebFunctionID = x.WEBFUNCTIONID }).ToList()
                            },
                            UserList = (from x in db.USERAUTHGROUP
                                        join u in db.ACCOUNT
                                        on x.USERID equals u.ID
                                        join o in db.ORGANIZATION
                                        on u.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                        where x.AUTHGROUPID == AuthGroupID
                                        select new Models.AuthGroupManagement.UserModel
                                        {
                                            OrganizationDescription = o.DESCRIPTION,
                                            ID = u.ID,
                                            Name = u.NAME
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var exists = db.AUTHGROUP.FirstOrDefault(x => x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
                        db.AUTHGROUP.Add(new AUTHGROUP()
                        {
                            ID = Model.FormInput.ID,
                            NAME = Model.FormInput.Name
                        });

                        db.AUTHGROUPWEBPERMISSIONFUNCTION.AddRange(Model.FormInput.WebPermissionFunctionList.Select(x => new DbEntity.ASE.AUTHGROUPWEBPERMISSIONFUNCTION
                        {
                            AUTHGROUPID = Model.FormInput.ID,
                            WEBPERMISSIONID = x.WebPermissionID,
                            WEBFUNCTIONID = x.WebFunctionID
                        }).ToList());

                        db.USERAUTHGROUP.AddRange(Model.UserList.Select(x => new USERAUTHGROUP
                        {
                            AUTHGROUPID = Model.FormInput.ID,
                            USERID = x.ID
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
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var authGroup = db.AUTHGROUP.First(x => x.ID == AuthGroupID);

                        result.ReturnData(new EditFormModel()
                        {
                            AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(Account.OrganizationUniqueID),
                            AuthGroupID = authGroup.ID,
                            WebPermissionFunction = new WebPermissionFunctionModel()
                            {
                                WebPermissionList = result.Data as List<WebPermissionModel>,
                                AuthGroupWebPermissionFunctionList = db.AUTHGROUPWEBPERMISSIONFUNCTION.Where(x => x.AUTHGROUPID == authGroup.ID).Select(x => new Models.AuthGroupManagement.AuthGroupWebPermissionFunction { AuthGroupID = x.AUTHGROUPID, WebPermissionID = x.WEBPERMISSIONID, WebFunctionID = x.WEBFUNCTIONID }).ToList()
                            },
                            FormInput = new FormInput()
                            {
                                ID = authGroup.ID,
                                Name = authGroup.NAME
                            },
                            UserList = (from x in db.USERAUTHGROUP
                                        join u in db.ACCOUNT
                                        on x.USERID equals u.ID
                                        join o in db.ORGANIZATION
                                        on u.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                        where x.AUTHGROUPID == AuthGroupID
                                        select new Models.AuthGroupManagement.UserModel
                                        {
                                            OrganizationDescription = o.DESCRIPTION,
                                            ID = u.ID,
                                            Name = u.NAME
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                    #region AuthGroup
                    var authGroup = db.AUTHGROUP.First(x => x.ID == Model.AuthGroupID);

                    authGroup.NAME = Model.FormInput.Name;

                    db.SaveChanges();
                    #endregion

                    #region AuthGroupWebPermissionFunction
                    #region Delete
                    db.AUTHGROUPWEBPERMISSIONFUNCTION.RemoveRange(db.AUTHGROUPWEBPERMISSIONFUNCTION.Where(x => x.AUTHGROUPID == authGroup.ID).ToList());

                    db.SaveChanges();
                    #endregion

                    #region Insert
                    db.AUTHGROUPWEBPERMISSIONFUNCTION.AddRange(Model.FormInput.WebPermissionFunctionList.Select(x => new DbEntity.ASE.AUTHGROUPWEBPERMISSIONFUNCTION
                    {
                        AUTHGROUPID = authGroup.ID,
                        WEBPERMISSIONID = x.WebPermissionID,
                        WEBFUNCTIONID = x.WebFunctionID
                    }).ToList());

                    db.SaveChanges();
                    #endregion
                    #endregion

                    #region UserAuthGroup
                    #region Delete
                    db.USERAUTHGROUP.RemoveRange(db.USERAUTHGROUP.Where(x => x.AUTHGROUPID == authGroup.ID).ToList());

                    db.SaveChanges();
                    #endregion

                    #region Insert
                    db.USERAUTHGROUP.AddRange(Model.UserList.Select(x => new USERAUTHGROUP
                    {
                        AUTHGROUPID = authGroup.ID,
                        USERID = x.ID
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    //AuthGroup
                    db.AUTHGROUP.Remove(db.AUTHGROUP.First(x => x.ID == AuthGroupID));

                    //AuthGroupWebPermissionFunction
                    db.AUTHGROUPWEBPERMISSIONFUNCTION.RemoveRange(db.AUTHGROUPWEBPERMISSIONFUNCTION.Where(x => x.AUTHGROUPID == AuthGroupID).ToList());

                    //UserAuthGroup
                    db.USERAUTHGROUP.RemoveRange(db.USERAUTHGROUP.Where(x => x.AUTHGROUPID == AuthGroupID).ToList());

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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var webPermissionFunctionList = db.WEBPERMISSIONFUNCTION.ToList();

                    var ancestorList = db.WEBPERMISSION.Where(x => x.PARENTID == "*").OrderBy(x => x.SEQ).ToList();

                    foreach (var ancestor in ancestorList)
                    {
                        var item = new WebPermissionModel()
                        {
                            ID = ancestor.ID,
                            Description = db.WEBPERMISSIONDESCRIPTION.Where(x => x.WEBPERMISSIONID == ancestor.ID).ToDictionary(x => x.LANGUAGE, x => x.DESCRIPTION),
                            SubItemList = new List<WebPermissionModel>(),
                            WebFunctionList = null
                        };

                        var parentList = db.WEBPERMISSION.Where(x => x.PARENTID == ancestor.ID).OrderBy(x => x.SEQ).ToList();

                        foreach (var parent in parentList)
                        {
                            if (!string.IsNullOrEmpty(parent.CONTROLLER))
                            {
                                item.SubItemList.Add(new WebPermissionModel()
                                {
                                    ID = parent.ID,
                                    Description = db.WEBPERMISSIONDESCRIPTION.Where(x => x.WEBPERMISSIONID == parent.ID).ToDictionary(x => x.LANGUAGE, x => x.DESCRIPTION),
                                    SubItemList = null,
                                    WebFunctionList = (from x in webPermissionFunctionList
                                                       join f in db.WEBFUNCTION
                                                       on x.WEBFUNCTIONID equals f.ID
                                                       where x.WEBPERMISSIONID == parent.ID
                                                       select new WebFunctionModel
                                                       {
                                                           ID = f.ID,
                                                           Description = db.WEBFUNCTIONDESCRIPTION.Where(y => y.WEBFUNCTIONID == f.ID).ToDictionary(y => y.LANGUAGE, y => y.DESCRIPTION)
                                                       }).ToList()
                                });
                            }
                            else
                            {
                                var permissionList = db.WEBPERMISSION.Where(x => x.PARENTID == parent.ID).OrderBy(x => x.SEQ).ToList();

                                item.SubItemList.AddRange(permissionList.Select(p => new WebPermissionModel
                                {
                                    ID = p.ID,
                                    Description = db.WEBPERMISSIONDESCRIPTION.Where(d => d.WEBPERMISSIONID == p.ID).ToDictionary(d => d.LANGUAGE, d => d.DESCRIPTION),
                                    SubItemList = null,
                                    WebFunctionList = (from x in webPermissionFunctionList
                                                       join f in db.WEBFUNCTION
                                                       on x.WEBFUNCTIONID equals f.ID
                                                       where x.WEBPERMISSIONID == p.ID
                                                       select new WebFunctionModel
                                                       {
                                                           ID = f.ID,
                                                           Description = db.WEBFUNCTIONDESCRIPTION.Where(d => d.WEBFUNCTIONID == f.ID).ToDictionary(d => d.LANGUAGE, d => d.DESCRIPTION)
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
                                              select new Models.AuthGroupManagement.UserModel
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
