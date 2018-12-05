using DataAccess;
using DbEntity.MSSQL;
using DbEntity.MSSQL.PipelinePatrol;
using Models.PipelinePatrol.DataSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace DataSync.PipelinePatrol
{
    public class UserHelper
    {
        public static RequestResult Signin(SigninFormInput Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities pdb = new PDbEntities())
                using (DbEntities db = new DbEntities())
                {
                    if (!string.IsNullOrEmpty(Model.UID))
                    {
                        var user = db.User.FirstOrDefault(x => x.UID == Model.UID);

                        if (user != null)
                        {
                            if (!user.IsMobileUser)
                            {
                                result.ReturnFailedMessage(Resources.Resource.NotMobileUser);
                            }
                            else
                            {
                                result.ReturnData(Get(user.ID));
                            }
                        }
                        else
                        {
                            result.ReturnFailedMessage(Resources.Resource.UserIDNotExist);
                        }
                    }
                    else
                    {
                        var user = db.User.FirstOrDefault(x => x.ID == Model.UserID);

                        if (user != null)
                        {
                            if (!user.IsMobileUser)
                            {
                                result.ReturnFailedMessage(Resources.Resource.NotMobileUser);
                            }
                            else
                            {
                                if (string.Compare(user.Password, Model.Password, false) == 0)
                                {
                                    result.ReturnData(Get(user.ID));
                                }
                                else
                                {
                                    result.ReturnFailedMessage(Resources.Resource.WrongPassword);
                                }
                            }
                        }
                        else
                        {
                            result.ReturnFailedMessage(Resources.Resource.UserIDNotExist);
                        }
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

        /// <summary>
        /// 更新 UserExtra 資料
        /// 用來判斷 通知 或 同一個帳號只能在一台設備
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        public static RequestResult UpdateUserExtra(UserExtraFormInput Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (DbEntities db = new DbEntities())
                using (PDbEntities pdb = new PDbEntities())
                {
                    var user = db.User.FirstOrDefault(x => x.ID == Model.UserID);

                    if (user != null)
                    {
                        var userExtra = pdb.UserExtra.FirstOrDefault(x => x.UserID == Model.UserID);
                        var currentDate = DateTime.Now;
                        if(userExtra == null)
                        {
                            //create
                            pdb.UserExtra.Add(new UserExtra
                            {
                                UserID = Model.UserID,
                                FCMID = Model.FCMID,
                                DeviceID = Model.DeviceID,
                                IMEI = Model.IMEI,
                                CreateTime = currentDate,
                                LastModifyTime = currentDate
                            });


                        }
                        else
                        {
                            //update
                            userExtra.FCMID = Model.FCMID;
                            userExtra.DeviceID = Model.DeviceID;
                            userExtra.IMEI = Model.IMEI;
                            userExtra.LastModifyTime = currentDate;
                        }
                        
                        
                        pdb.SaveChanges();
                        result.Success();
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

        public static UserModel Get(string UserID)
        {
            UserModel result = null;

            try
            {
                using (DbEntities db = new DbEntities())
                {
                    var user = db.User.First(x => x.ID == UserID);

                    result = new UserModel()
                    {
                        ID = user.ID,
                        Name = user.Name,
                        OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(user.OrganizationUniqueID),
                        Title = user.Title
                    };
                }
            }
            catch (Exception ex)
            {
                result = null;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return result;
        }

        public static RequestResult GetPhoto(string UserID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (DbEntities db = new DbEntities())
                {
                    var photo = db.UserPhoto.FirstOrDefault(x => x.UserID == UserID);

                    if (photo != null)
                    {
                        result.ReturnData(new PhotoModel()
                        {
                            UserID = photo.UserID,
                            FileUniqueID = photo.FileUniqueID,
                            Extension = photo.Extension
                        });
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

        public static RequestResult UpdateLocation(LocationModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    db.OnlineUserLocus.Add(new OnlineUserLocus()
                    {
                        UserID = Model.UserID,
                        UpdateTime = DateTime.Now,
                        LNG = Model.LNG,
                        LAT = Model.LAT,
                        JobUniqueID = Model.JobUniqueID,
                        RouteUniqueID = Model.RouteUniqueID
                    });

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

        public static RequestResult Disconnect(string UserID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    db.OnlineUserLocus.RemoveRange(db.OnlineUserLocus.Where(x => x.UserID == UserID).ToList());

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

        public static RequestResult Signout(string UserID)
        {
            RequestResult result = new RequestResult();

            try
            {
                
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
