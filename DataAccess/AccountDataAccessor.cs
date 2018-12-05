using System;
using System.Linq;
using System.Reflection;
using Utility;
using Utility.Models;
using Models.Authenticated;
using System.IO;
using DbEntity.MSSQL;
using System.Collections.Generic;

namespace DataAccess
{
    public class AccountDataAccessor
    {
        public static List<Models.Shared.UserModel> GetAllUser()
        {
            var accountList = new List<Models.Shared.UserModel>();

            try
            {
                using (DbEntities db = new DbEntities())
                {
                    accountList = (from u in db.User
                                   join o in db.Organization
                                   on u.OrganizationUniqueID equals o.UniqueID
                                   select new Models.Shared.UserModel
                                   {
                                       ID = u.ID,
                                       Name = u.Name,
                                       OrganizationUniqueID = u.OrganizationUniqueID,
                                       Email = u.Email,
                                       OrganizationDescription = o.Description
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
                using (DbEntities db = new DbEntities())
                {
                    result = db.User.Any(x => x.ID == UserID && x.LoginID == LoginID);
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
                using (DbEntities db = new DbEntities())
                {
                    var user = db.User.FirstOrDefault(x => x.ID == Model.UserID);

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
                using (DbEntities db = new DbEntities())
                {
                    var user = db.User.FirstOrDefault(x => x.ID == UserID);

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

        public static RequestResult GetAccount(List<Models.Shared.Organization> OrganizationList, User User)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (DbEntities db = new DbEntities())
                {
                    var account = new Account()
                    {
                        ID = User.ID,
                        Name = User.Name,
                        OrganizationUniqueID = User.OrganizationUniqueID,
                        WebPermissionFunctionList = (from a in db.UserAuthGroup
                                                     join x in db.AuthGroupWebPermissionFunction
                                                     on a.AuthGroupID equals x.AuthGroupID
                                                     join p in db.WebPermission
                                                     on x.WebPermissionID equals p.ID
                                                     where a.UserID == User.ID
                                                     select new UserWebPermissionFunction
                                                     {
                                                         WebPermissionID = p.ID,
                                                         WebFunctionID = x.WebFunctionID,
                                                         Area = p.Area,
                                                         Controller = p.Controller,
                                                         Action = p.Action
                                                     }).Distinct().ToList(),
                        UserOrganizationPermissionList = OrganizationDataAccessor.GetUserOrganizationPermissionList(User.OrganizationUniqueID)
                    };

                    account.UserRootOrganizationUniqueID = OrganizationDataAccessor.GetUserRootOrganizationUniqueID(OrganizationList, account);

                    var userPhoto = db.UserPhoto.FirstOrDefault(x => x.UserID == User.ID);

                    if (userPhoto != null)
                    {
                        account.Photo = string.Format("{0}.{1}", userPhoto.FileUniqueID, userPhoto.Extension);
                    }

                    var userPermissionList = (from a in db.UserAuthGroup
                                              join x in db.AuthGroupWebPermissionFunction
                                              on a.AuthGroupID equals x.AuthGroupID
                                              join p in db.WebPermission
                                              on x.WebPermissionID equals p.ID
                                              where a.UserID == User.ID
                                              select new
                                              {
                                                  p.ID,
                                                  p.ParentID,
                                              }).Distinct().ToList();

                    foreach (var userPermission in userPermissionList)
                    {
                        var parent = db.WebPermission.First(x => x.ID == userPermission.ParentID);
                        var permission = db.WebPermission.First(x => x.ID == userPermission.ID);

                        if (parent.ParentID == "*")
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
                                    Description = db.WebPermissionDescription.Where(x => x.WebPermissionID == parent.ID).ToDictionary(x => x.Language, x => x.Description),
                                    Icon = parent.Icon,
                                    Seq = parent.Seq
                                };

                                account.MenuItemList.Add(ancestorMenuItem);
                            }

                            ancestorMenuItem.SubItemList.Add(new MenuItem()
                            {
                                ID = permission.ID,
                                Area = permission.Area,
                                Controller = permission.Controller,
                                Action = permission.Action,
                                Description = db.WebPermissionDescription.Where(x => x.WebPermissionID == permission.ID).ToDictionary(x => x.Language, x => x.Description),
                                Icon = permission.Icon,
                                Seq = permission.Seq
                            });
                        }
                        else
                        {
                            var ancestor = db.WebPermission.First(x => x.ID == parent.ParentID);

                            var ancestorMenuItem = account.MenuItemList.FirstOrDefault(x => x.ID == ancestor.ID);

                            if (ancestorMenuItem == null)
                            {
                                ancestorMenuItem = new MenuItem()
                                {
                                    ID = ancestor.ID,
                                    Area = string.Empty,
                                    Controller = string.Empty,
                                    Action = string.Empty,
                                    Description = db.WebPermissionDescription.Where(x => x.WebPermissionID == ancestor.ID).ToDictionary(x => x.Language, x => x.Description),
                                    Icon = ancestor.Icon,
                                    Seq = ancestor.Seq
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
                                    Description = db.WebPermissionDescription.Where(x => x.WebPermissionID == parent.ID).ToDictionary(x => x.Language, x => x.Description),
                                    Icon = parent.Icon,
                                    Seq = parent.Seq
                                };

                                ancestorMenuItem.SubItemList.Add(parentMenuItem);
                            }

                            parentMenuItem.SubItemList.Add(new MenuItem()
                            {
                                ID = permission.ID,
                                Area = permission.Area,
                                Controller = permission.Controller,
                                Action = permission.Action,
                                Description = db.WebPermissionDescription.Where(x => x.WebPermissionID == permission.ID).ToDictionary(x => x.Language, x => x.Description),
                                Icon = permission.Icon,
                                Seq = permission.Seq
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
                using (DbEntities db = new DbEntities())
                {
                    var user = db.User.First(x => x.ID == Account.ID);

                    if (string.Compare(user.Password, Model.Opassword, false) == 0)
                    {
                        user.Password = Model.Npassword;
                        user.LastModifyTime = DateTime.Now;

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

                    using (DbEntities db = new DbEntities())
                    {
                        var userPhoto = db.UserPhoto.FirstOrDefault(x => x.UserID == Account.ID);

                        if (userPhoto != null)
                        {
                            File.Delete(Path.Combine(Config.UserPhotoFolderPath, string.Format("{0}.{1}", userPhoto.FileUniqueID, userPhoto.Extension)));

                            userPhoto.FileUniqueID = uniqueID;
                            userPhoto.Extension = extension;
                        }
                        else
                        {
                            db.UserPhoto.Add(new UserPhoto()
                            {
                                UserID = Account.ID,
                                FileUniqueID = uniqueID,
                                Extension = extension
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
