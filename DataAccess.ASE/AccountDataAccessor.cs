using System;
using System.Linq;
using System.Reflection;
using Utility;
using Utility.Models;
using Models.Authenticated;
using System.IO;
using DbEntity.ASE;
using System.Collections.Generic;
using System.DirectoryServices;

namespace DataAccess.ASE
{
    public class AccountDataAccessor
    {
        public static List<Models.Shared.UserModel> GetAllUser()
        {
            var accountList = new List<Models.Shared.UserModel>();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    accountList = (from a in db.ACCOUNT
                                   join o in db.ORGANIZATION
                                   on a.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                   select new Models.Shared.UserModel
                                   {
                                       ID = a.ID,
                                       Name = a.NAME,
                                       ManagerID = a.MANAGERID,
                                       OrganizationUniqueID = a.ORGANIZATIONUNIQUEID,
                                       Email = a.EMAIL,
                                       OrganizationDescription = o.DESCRIPTION
                                   }).ToList();
                }
            }
            catch (Exception ex)
            {
                accountList = new List<Models.Shared.UserModel>();

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return accountList;
        }

        public static bool IsAuthenticated(string UserID, string LoginID)
        {
            bool result = false;

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    result = db.ACCOUNT.Any(x => x.ID.ToUpper() == UserID.ToUpper() && x.LOGINID == LoginID);
                }
            }
            catch (Exception ex)
            {
                result = false;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return result;
        }

        public static RequestResult GetAccount(LoginFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var user = db.ACCOUNT.FirstOrDefault(x => x.ID.ToUpper() == Model.UserID.ToUpper());

                    if (user != null)
                    {
                        result.ReturnData(user);
                    }
                    else
                    {
                        result.ReturnFailedMessage(Resources.Resource.UserIDNotExist);
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

        public static RequestResult GetAccount(List<Models.Shared.Organization> OrganizationList, string UserID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var user = db.ACCOUNT.FirstOrDefault(x => x.ID.ToUpper() == UserID.ToUpper());

                    if (user != null)
                    {
                        result = GetAccount(OrganizationList, user);
                    }
                    else
                    {
                        result.Failed();
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

        public static RequestResult GetAccount(List<Models.Shared.Organization> OrganizationList, ACCOUNT User)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var account = new Account()
                    {
                        ID = User.ID,
                        Name = User.NAME,
                        OrganizationUniqueID = User.ORGANIZATIONUNIQUEID,
                        UserAuthGroupList = db.USERAUTHGROUP.Where(x=>x.USERID==User.ID).Select(x=>x.AUTHGROUPID).Distinct().ToList(),
                        WebPermissionFunctionList = (from a in db.USERAUTHGROUP
                                                     join x in db.AUTHGROUPWEBPERMISSIONFUNCTION
                                                     on a.AUTHGROUPID equals x.AUTHGROUPID
                                                     join p in db.WEBPERMISSION
                                                     on x.WEBPERMISSIONID equals p.ID
                                                     where a.USERID == User.ID
                                                     select new UserWebPermissionFunction
                                                     {
                                                         WebPermissionID = p.ID,
                                                         WebFunctionID = x.WEBFUNCTIONID,
                                                         Area = p.AREA,
                                                         Controller = p.CONTROLLER,
                                                         Action = p.ACTION
                                                     }).Distinct().ToList(),
                        UserOrganizationPermissionList = OrganizationDataAccessor.GetUserOrganizationPermissionList(User.ORGANIZATIONUNIQUEID)
                    };

                    account.UserRootOrganizationUniqueID = OrganizationDataAccessor.GetUserRootOrganizationUniqueID(OrganizationList, account);

                    var userPhoto = db.USERPHOTO.FirstOrDefault(x => x.USERID == User.ID);

                    if (userPhoto != null)
                    {
                        account.Photo = string.Format("{0}.{1}", userPhoto.FILEUNIQUEID, userPhoto.EXTENSION);
                    }

                    var userPermissionList = (from a in db.USERAUTHGROUP
                                              join x in db.AUTHGROUPWEBPERMISSIONFUNCTION
                                              on a.AUTHGROUPID equals x.AUTHGROUPID
                                              join p in db.WEBPERMISSION
                                              on x.WEBPERMISSIONID equals p.ID
                                              where a.USERID == User.ID
                                              select new
                                              {
                                                  p.ID,
                                                  p.PARENTID,
                                              }).Distinct().ToList();

                    foreach (var userPermission in userPermissionList)
                    {
                        var parent = db.WEBPERMISSION.First(x => x.ID == userPermission.PARENTID);
                        var permission = db.WEBPERMISSION.First(x => x.ID == userPermission.ID);

                        if (parent.PARENTID == "*")
                        {
                            var ancestorMenuItem = account.MenuItemList.FirstOrDefault(x => x.ID == parent.ID);

                            if (ancestorMenuItem == null)
                            {
                                ancestorMenuItem = new MenuItem()
                                {
                                    ID = parent.ID,
                                    Area = string.Empty,
                                    Controller = string.Empty,
                                    Action = string.Empty,
                                    Description = db.WEBPERMISSIONDESCRIPTION.Where(x => x.WEBPERMISSIONID == parent.ID).ToDictionary(x => x.LANGUAGE, x => x.DESCRIPTION),
                                    Icon = parent.ICON,
                                    Seq = parent.SEQ.Value
                                };

                                account.MenuItemList.Add(ancestorMenuItem);
                            }

                            ancestorMenuItem.SubItemList.Add(new MenuItem()
                            {
                                ID = permission.ID,
                                Area = permission.AREA,
                                Controller = permission.CONTROLLER,
                                Action = permission.ACTION,
                                Description = db.WEBPERMISSIONDESCRIPTION.Where(x => x.WEBPERMISSIONID == permission.ID).ToDictionary(x => x.LANGUAGE, x => x.DESCRIPTION),
                                Icon = permission.ICON,
                                Seq = permission.SEQ.Value
                            });
                        }
                        else
                        {
                            var ancestor = db.WEBPERMISSION.First(x => x.ID == parent.PARENTID);

                            var ancestorMenuItem = account.MenuItemList.FirstOrDefault(x => x.ID == ancestor.ID);

                            if (ancestorMenuItem == null)
                            {
                                ancestorMenuItem = new MenuItem()
                                {
                                    ID = ancestor.ID,
                                    Area = string.Empty,
                                    Controller = string.Empty,
                                    Action = string.Empty,
                                    Description = db.WEBPERMISSIONDESCRIPTION.Where(x => x.WEBPERMISSIONID == ancestor.ID).ToDictionary(x => x.LANGUAGE, x => x.DESCRIPTION),
                                    Icon = ancestor.ICON,
                                    Seq = ancestor.SEQ.Value
                                };

                                account.MenuItemList.Add(ancestorMenuItem);
                            }

                            var parentMenuItem = ancestorMenuItem.SubItemList.FirstOrDefault(x => x.ID == parent.ID);

                            if (parentMenuItem == null)
                            {
                                parentMenuItem = new MenuItem()
                                {
                                    ID = parent.ID,
                                    Area = string.Empty,
                                    Controller = string.Empty,
                                    Action = string.Empty,
                                    Description = db.WEBPERMISSIONDESCRIPTION.Where(x => x.WEBPERMISSIONID == parent.ID).ToDictionary(x => x.LANGUAGE, x => x.DESCRIPTION),
                                    Icon = parent.ICON,
                                    Seq = parent.SEQ.Value
                                };

                                ancestorMenuItem.SubItemList.Add(parentMenuItem);
                            }

                            parentMenuItem.SubItemList.Add(new MenuItem()
                            {
                                ID = permission.ID,
                                Area = permission.AREA,
                                Controller = permission.CONTROLLER,
                                Action = permission.ACTION,
                                Description = db.WEBPERMISSIONDESCRIPTION.Where(x => x.WEBPERMISSIONID == permission.ID).ToDictionary(x => x.LANGUAGE, x => x.DESCRIPTION),
                                Icon = permission.ICON,
                                Seq = permission.SEQ.Value
                            });
                        }
                    }

                    foreach (var menu in account.MenuItemList)
                    {
                        foreach (var item in menu.SubItemList)
                        {
                            if (item.SubItemList != null)
                            {
                                item.SubItemList = item.SubItemList.OrderBy(x => x.Seq).ToList();
                            }
                        }

                        menu.SubItemList = menu.SubItemList.OrderBy(x => x.Seq).ToList();
                    }

                    account.MenuItemList = account.MenuItemList.OrderBy(x => x.Seq).ToList();

                    result.ReturnData(account);
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

        public static RequestResult ChangePassword(PasswordFormModel Model, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var user = db.ACCOUNT.First(x => x.ID == Account.ID);

                    if (string.Compare(user.PASSWORD, Model.Opassword, false) == 0)
                    {
                        user.PASSWORD = Model.Npassword;
                        user.LASTMODIFYTIME = DateTime.Now;

                        db.SaveChanges();

                        result.ReturnSuccessMessage(Resources.Resource.ChangePassword + " " + Resources.Resource.Success);
                    }
                    else
                    {
                        result.ReturnFailedMessage(Resources.Resource.WrongPassword);
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

        public static RequestResult ChangeUserPhoto(UserPhotoFormModel Model, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                if (Model.Photo != null && Model.Photo.ContentLength > 0)
                {
                    var uniqueID = Guid.NewGuid().ToString();

                    string extension = Model.Photo.FileName.Substring(Model.Photo.FileName.LastIndexOf('.') + 1);

                    Model.Photo.SaveAs(Path.Combine(Config.UserPhotoFolderPath, string.Format("{0}.{1}", uniqueID, extension)));

                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var userPhoto = db.USERPHOTO.FirstOrDefault(x => x.USERID == Account.ID);

                        if (userPhoto != null)
                        {
                            File.Delete(Path.Combine(Config.UserPhotoFolderPath, string.Format("{0}.{1}", userPhoto.FILEUNIQUEID, userPhoto.EXTENSION)));

                            userPhoto.FILEUNIQUEID = uniqueID;
                            userPhoto.EXTENSION = extension;
                        }
                        else
                        {
                            db.USERPHOTO.Add(new USERPHOTO()
                            {
                                USERID = Account.ID,
                                FILEUNIQUEID = uniqueID,
                                EXTENSION = extension
                            });
                        }

                        db.SaveChanges();
                    }

                    result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.ChangeUserPhoto, Resources.Resource.Success));
                }
                else
                {
                    result.ReturnFailedMessage(Resources.Resource.UploadFileRequired);
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
    }
}
