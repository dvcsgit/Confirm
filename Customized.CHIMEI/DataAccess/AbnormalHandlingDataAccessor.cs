using Customized.CHIMEI.Models.AbnormalHandlingManagement;
using DataAccess;
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.Authenticated;
using Models.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace Customized.CHIMEI.DataAccess
{
    public class AbnormalHandlingDataAccessor
    {
        public static RequestResult GetAIMSLink(EditFormModel Model, Account Account, UrlHelper UrlHelper)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var checkResult = db.CheckResult.First(x => x.EquipmentUniqueID == Model.EquipmentUniqueID && x.CheckDate == Model.CheckDate);
                    
                    var chimeiJob = db.CHIMEI_JOB.First(x => x.JobUniqueID == checkResult.JobUniqueID && x.ControlPointUniqueID == checkResult.ControlPointUniqueID && x.EquipmentUniqueID == checkResult.EquipmentUniqueID && x.PartUniqueID == checkResult.PartUniqueID);

                    var description = string.Empty;

                    foreach (var c in Model.CheckResultList)
                    {
                        description += string.Format("{0}({1})", c.CheckItemDescription, c.Result);

                        description += "/";
                    }

                    if (description.Length > 1)
                    {
                        description = description.Substring(0, description.Length - 1);
                    }

                    if (!string.IsNullOrEmpty(chimeiJob.ACT_KEY))
                    {
                        description += string.Format("({0})", chimeiJob.ACT_KEY);
                    }

                    var link = string.Format("http://10.1.1.127/CHIMEI/Activity/ExternalCallActivityCreate?template_type=MAINT_TEMPLATE&template=NOTIF_WORKFLOE&asset_type=EQUIPMENT&asset={0}&user={1}&activity_description={2}&request_no={3}&external_call=true", checkResult.EquipmentID, Account.ID, description, chimeiJob.ACT_ID);

                    Logger.Log(link);

                    result.ReturnData(link);
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

        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new GridViewModel();

                using (EDbEntities db = new EDbEntities())
                {
                    var query = (from abnormal in db.Abnormal
                                 join x in db.AbnormalCheckResult
                                 on abnormal.UniqueID equals x.AbnormalUniqueID
                                 join c in db.CheckResult
                                 on x.CheckResultUniqueID equals c.UniqueID
                                 join y in db.RouteManager
                                 on c.RouteUniqueID equals y.RouteUniqueID
                                 join a in db.ArriveRecord
                                 on c.ArriveRecordUniqueID equals a.UniqueID
                                 select new
                                 {
                                     a.OrganizationUniqueID,
                                     c.RouteUniqueID,
                                     c.EquipmentUniqueID,
                                     c.EquipmentID,
                                     c.EquipmentName,
                                     c.PartDescription,
                                     c.CheckDate,
                                     c.IsAbnormal,
                                     c.IsAlert,
                                     abnormal.ClosedTime,
                                     abnormal.ClosedUserID,
                                     abnormal.ClosedRemark,
                                     CheckUserID = a.UserID,
                                     ResponsorUserID = y.UserID
                                 }).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Status))
                    {
                        var today = DateTimeHelper.DateTime2DateString(DateTime.Today);

                        if (Parameters.Status == "1" || Parameters.Status == "2" || Parameters.Status == "3" || Parameters.Status == "4")
                        {
                            query = query.Where(x => x.ResponsorUserID == Account.ID);

                            if (Parameters.Status == "1")
                            {
                                query = query.Where(x => x.IsAbnormal && x.CheckDate == today);
                            }

                            if (Parameters.Status == "2")
                            {
                                query = query.Where(x => x.IsAlert && x.CheckDate == today);
                            }

                            if (Parameters.Status == "3")
                            {
                                query = query.Where(x => x.IsAbnormal && !x.ClosedTime.HasValue);
                            }

                            if (Parameters.Status == "4")
                            {
                                query = query.Where(x => x.IsAlert && !x.ClosedTime.HasValue);
                            }
                        }

                        if (Parameters.Status == "5" || Parameters.Status == "6" || Parameters.Status == "7" || Parameters.Status == "8")
                        {
                            query = query.Where(x => Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID));
                            
                            if (Parameters.Status == "5")
                            {
                                query = query.Where(x => x.IsAbnormal && x.CheckDate == today);
                            }

                            if (Parameters.Status == "6")
                            {
                                query = query.Where(x => x.IsAlert && x.CheckDate == today);
                            }

                            if (Parameters.Status == "7")
                            {
                                query = query.Where(x => x.IsAbnormal && !x.ClosedTime.HasValue);
                            }

                            if (Parameters.Status == "8")
                            {
                                query = query.Where(x => x.IsAlert && !x.ClosedTime.HasValue);
                            }
                        }
                    }
                    else
                    {
                        var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                        var organizationList = Account.QueryableOrganizationUniqueIDList.Intersect(downStreamOrganizationList);

                        query = query.Where(x => organizationList.Contains(x.OrganizationUniqueID));
                        
                        if (!string.IsNullOrEmpty(Parameters.BeginDateString))
                        {
                            query = query.Where(x => string.Compare(x.CheckDate, Parameters.BeginDate) >= 0);
                        }

                        if (!string.IsNullOrEmpty(Parameters.EndDateString))
                        {
                            query = query.Where(x => string.Compare(x.CheckDate, Parameters.EndDate) <= 0);
                        }

                        if (!string.IsNullOrEmpty(Parameters.EquipmentUniqueID))
                        {
                            query = query.Where(x => x.EquipmentUniqueID == Parameters.EquipmentUniqueID);
                        }
                    }

                    var queryResult = query.ToList();

                    var tmp = queryResult.Select(x => new
                    {
                        x.RouteUniqueID,
                        x.EquipmentUniqueID,
                        x.EquipmentID,
                        x.EquipmentName,
                        x.PartDescription,
                        x.CheckDate,
                        x.ClosedTime,
                        x.ClosedUserID,
                        x.ClosedRemark
                    }).Distinct().ToList();

                    foreach (var q in tmp)
                    {
                        var item = new GridItem
                        {
                            RouteUniqueID = q.RouteUniqueID,
                            EquipmentUniqueID = q.EquipmentUniqueID,
                            EquipmentID = q.EquipmentID,
                            EquipmentName = q.EquipmentName,
                            PartDescription = q.PartDescription,
                            CheckDate = q.CheckDate,
                            ClosedTime = q.ClosedTime,
                            ClosedUser = UserDataAccessor.GetUser(q.ClosedUserID),
                            ClosedRemark = q.ClosedRemark
                        };

                        var results = queryResult.Where(x => x.EquipmentUniqueID == q.EquipmentUniqueID && x.CheckDate == q.CheckDate).ToList();

                        item.IsAbnormal = results.Any(x => x.IsAbnormal);
                        item.IsAlert = !item.IsAbnormal && results.Any(x => x.IsAlert);

                        var checkUsers = results.Select(x => x.CheckUserID).Distinct().ToList();

                        foreach (var checkUser in checkUsers)
                        {
                            item.CheckUserList.Add(UserDataAccessor.GetUser(checkUser));
                        }

                        var routeManagerList = db.RouteManager.Where(x => x.RouteUniqueID == q.RouteUniqueID).Select(x => x.UserID).OrderBy(x => x).ToList();

                        foreach (var routeManager in routeManagerList)
                        {
                            if (!item.ResponsorList.Any(x => x.ID == routeManager))
                            {
                                item.ResponsorList.Add(UserDataAccessor.GetUser(routeManager));
                            }
                        }

                        model.ItemList.Add(item);
                    }

                    model.ItemList = model.ItemList.OrderBy(x => x.CheckDate).ThenBy(x => x.EquipmentDisplay).ToList();
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

        public static RequestResult GetDetailViewModel(GridItem Item)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new DetailViewModel();

                using (EDbEntities db = new EDbEntities())
                {
                    var query = (from abnormal in db.Abnormal
                                 join x in db.AbnormalCheckResult
                                 on abnormal.UniqueID equals x.AbnormalUniqueID
                                 join c in db.CheckResult
                                 on x.CheckResultUniqueID equals c.UniqueID
                                 join a in db.ArriveRecord
                                 on c.ArriveRecordUniqueID equals a.UniqueID
                                 where c.EquipmentUniqueID == Item.EquipmentUniqueID && c.CheckDate == Item.CheckDate
                                 select new
                                 {
                                     CheckResultUniqueID = c.UniqueID,
                                     AbnormalUniqueID = a.UniqueID,
                                     c.CheckTime,
                                     c.CheckItemUniqueID,
                                     c.CheckItemID,
                                     c.CheckItemDescription,
                                     c.LowerLimit,
                                     c.LowerAlertLimit,
                                     c.UpperAlertLimit,
                                     c.UpperLimit,
                                     c.Unit,
                                     c.Result,
                                     CheckUserID=a.UserID,
                                     c.IsAbnormal,
                                     c.IsAlert
                                 }).ToList();

                    model = new DetailViewModel()
                    {
                        UniqueID = Item.UniqueID,
                        EquipmentUniqueID = Item.EquipmentUniqueID,
                        EquipmentID = Item.EquipmentID,
                        EquipmentName = Item.EquipmentName,
                        PartDescription = Item.PartDescription,
                        CheckDate = Item.CheckDate,
                        ClosedUser = Item.ClosedUser,
                        ClosedTime = Item.ClosedTime,
                        ClosedRemark = Item.ClosedRemark,
                        ResponsorList = Item.ResponsorList
                    };

                    foreach (var q in query)
                    {
                        model.CheckResultList.Add(new CheckResultModel
                        {
                            AbnormalUniqueID = q.AbnormalUniqueID,
                            CheckItemID = q.CheckItemID,
                            CheckItemDescription = q.CheckItemDescription,
                            CheckTime = q.CheckTime,
                            CheckUser = UserDataAccessor.GetUser(q.CheckUserID),
                            LowerLimit = q.LowerLimit,
                            LowerAlertLimit = q.LowerAlertLimit,
                            UpperAlertLimit = q.UpperAlertLimit,
                            UpperLimit = q.UpperLimit,
                            Unit = q.Unit,
                            Result = q.Result,
                            IsAbnormal = q.IsAbnormal,
                            IsAlert = q.IsAlert,
                            AbnormalReasonList = db.CheckResultAbnormalReason.Where(a => a.CheckResultUniqueID == q.CheckResultUniqueID).OrderBy(a => a.AbnormalReasonID).Select(a => new AbnormalReasonModel
                            {
                                Description = a.AbnormalReasonDescription,
                                Remark = a.AbnormalReasonRemark,
                                HandlingMethodList = db.CheckResultHandlingMethod.Where(h => h.CheckResultUniqueID == q.CheckResultUniqueID && h.AbnormalReasonUniqueID == a.AbnormalReasonUniqueID).OrderBy(h => h.HandlingMethodID).Select(h => new HandlingMethodModel
                                {
                                    Description = h.HandlingMethodDescription,
                                    Remark = h.HandlingMethodRemark
                                }).ToList()
                            }).ToList(),
                        });
                    }

                    model.BeforePhotoList = db.AbnormalPhoto.Where(p => p.AbnormalUniqueID == model.FirstAbnormalUniqueID && p.Type == "B").OrderBy(p => p.Seq).ToList().Select(p => p.AbnormalUniqueID + "_" + p.Type + "_" + p.Seq + "." + p.Extension).ToList();
                    model.AfterPhotoList = db.AbnormalPhoto.Where(p => p.AbnormalUniqueID == model.FirstAbnormalUniqueID && p.Type == "A").OrderBy(p => p.Seq).ToList().Select(p => p.AbnormalUniqueID + "_" + p.Type + "_" + p.Seq + "." + p.Extension).ToList();
                    model.FileList = db.AbnormalFile.Where(f => f.AbnormalUniqueID == model.FirstAbnormalUniqueID).ToList().Select(f => new FileModel
                    {
                        AbnormalUniqueID = f.AbnormalUniqueID,
                        Seq = f.Seq,
                        FileName = f.FileName,
                        Extension = f.Extension,
                        Size = f.ContentLength,
                        UploadTime = f.UploadTime,
                        IsSaved = true
                    }).OrderBy(f => f.UploadTime).ToList();

                    var routeManagerList = db.RouteManager.Where(x => x.RouteUniqueID == Item.RouteUniqueID).Select(x => x.UserID).OrderBy(x => x).ToList();

                    foreach (var routeManager in routeManagerList)
                    {
                        if (!model.ResponsorList.Any(x => x.ID == routeManager))
                        {
                            model.ResponsorList.Add(UserDataAccessor.GetUser(routeManager));
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

        public static RequestResult GetEditFormModel(GridItem Item)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new EditFormModel();

                using (EDbEntities db = new EDbEntities())
                {
                    var query = (from abnormal in db.Abnormal
                                 join x in db.AbnormalCheckResult
                                 on abnormal.UniqueID equals x.AbnormalUniqueID
                                 join c in db.CheckResult
                                 on x.CheckResultUniqueID equals c.UniqueID
                                 join a in db.ArriveRecord
                                 on c.ArriveRecordUniqueID equals a.UniqueID
                                 where c.EquipmentUniqueID == Item.EquipmentUniqueID && c.CheckDate == Item.CheckDate
                                 select new
                                 {
                                     CheckResultUniqueID = c.UniqueID,
                                     AbnormalUniqueID = a.UniqueID,
                                     c.CheckTime,
                                     c.CheckItemUniqueID,
                                     c.CheckItemID,
                                     c.CheckItemDescription,
                                     c.LowerLimit,
                                     c.LowerAlertLimit,
                                     c.UpperAlertLimit,
                                     c.UpperLimit,
                                     c.Unit,
                                     c.Result,
                                     CheckUserID = a.UserID,
                                     c.IsAbnormal,
                                     c.IsAlert
                                 }).ToList();

                    model = new EditFormModel()
                    {
                        UniqueID = Item.UniqueID,
                        EquipmentUniqueID = Item.EquipmentUniqueID,
                        EquipmentID = Item.EquipmentID,
                        EquipmentName = Item.EquipmentName,
                        PartDescription = Item.PartDescription,
                        CheckDate = Item.CheckDate,
                        ClosedUser = Item.ClosedUser,
                        ClosedTime = Item.ClosedTime,
                        ClosedRemark = Item.ClosedRemark,
                        ResponsorList = Item.ResponsorList
                    };

                    foreach (var q in query)
                    {
                        model.CheckResultList.Add(new CheckResultModel
                        {
                            AbnormalUniqueID = q.AbnormalUniqueID,
                            CheckItemID = q.CheckItemID,
                            CheckItemDescription = q.CheckItemDescription,
                            CheckTime = q.CheckTime,
                            CheckUser = UserDataAccessor.GetUser(q.CheckUserID),
                            LowerLimit = q.LowerLimit,
                            LowerAlertLimit = q.LowerAlertLimit,
                            UpperAlertLimit = q.UpperAlertLimit,
                            UpperLimit = q.UpperLimit,
                            Unit = q.Unit,
                            Result = q.Result,
                            IsAbnormal = q.IsAbnormal,
                            IsAlert = q.IsAlert,
                            AbnormalReasonList = db.CheckResultAbnormalReason.Where(a => a.CheckResultUniqueID == q.CheckResultUniqueID).OrderBy(a => a.AbnormalReasonID).Select(a => new AbnormalReasonModel
                            {
                                Description = a.AbnormalReasonDescription,
                                Remark = a.AbnormalReasonRemark,
                                HandlingMethodList = db.CheckResultHandlingMethod.Where(h => h.CheckResultUniqueID == q.CheckResultUniqueID && h.AbnormalReasonUniqueID == a.AbnormalReasonUniqueID).OrderBy(h => h.HandlingMethodID).Select(h => new HandlingMethodModel
                                {
                                    Description = h.HandlingMethodDescription,
                                    Remark = h.HandlingMethodRemark
                                }).ToList()
                            }).ToList(),
                        });
                    }

                    var beforePhotoList = db.AbnormalPhoto.Where(x => x.AbnormalUniqueID == model.FirstAbnormalUniqueID && x.Type == "B").OrderBy(x => x.Seq).ToList();

                    foreach (var photo in beforePhotoList)
                    {
                        model.BeforePhotoList.Add(new PhotoModel()
                        {
                            TempUniqueID = Guid.NewGuid().ToString(),
                            AbnormalUniqueID = photo.AbnormalUniqueID,
                            Seq = photo.Seq,
                            Extension = photo.Extension,
                            Type = photo.Type,
                            IsSaved = true
                        });
                    }

                    var afterPhotoList = db.AbnormalPhoto.Where(x => x.AbnormalUniqueID == model.FirstAbnormalUniqueID && x.Type == "A").OrderBy(x => x.Seq).ToList();

                    foreach (var photo in afterPhotoList)
                    {
                        model.AfterPhotoList.Add(new PhotoModel()
                        {
                            TempUniqueID = Guid.NewGuid().ToString(),
                            AbnormalUniqueID = photo.AbnormalUniqueID,
                            Seq = photo.Seq,
                            Extension = photo.Extension,
                            Type = photo.Type,
                            IsSaved = true
                        });
                    }

                    model.FileList = db.AbnormalFile.Where(x => x.AbnormalUniqueID == model.FirstAbnormalUniqueID).ToList().Select(x => new FileModel
                    {
                        AbnormalUniqueID = x.AbnormalUniqueID,
                        Seq = x.Seq,
                        FileName = x.FileName,
                        Extension = x.Extension,
                        Size = x.ContentLength,
                        UploadTime = x.UploadTime,
                        IsSaved = true
                    }).OrderBy(x => x.UploadTime).ToList();

                    var routeManagerList = db.RouteManager.Where(x => x.RouteUniqueID == Item.RouteUniqueID).Select(x => x.UserID).OrderBy(x => x).ToList();

                    foreach (var routeManager in routeManagerList)
                    {
                        if (!model.ResponsorList.Any(x => x.ID == routeManager))
                        {
                            model.ResponsorList.Add(UserDataAccessor.GetUser(routeManager));
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

        public static RequestResult Save(EditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var abnormals = (from a in db.Abnormal
                                     join x in db.AbnormalCheckResult
                                     on a.UniqueID equals x.AbnormalUniqueID
                                     join c in db.CheckResult
                                     on x.CheckResultUniqueID equals c.UniqueID
                                     where c.EquipmentUniqueID == Model.EquipmentUniqueID && c.CheckDate == Model.CheckDate
                                     select a).ToList();

                    foreach (var abnormal in abnormals)
                    { 
                        abnormal.ClosedRemark = Model.ClosedRemark;
                    }

                    var beforePhotoList = db.AbnormalPhoto.Where(x => x.AbnormalUniqueID == Model.FirstAbnormalUniqueID && x.Type == "B").ToList();

                    foreach (var photo in beforePhotoList)
                    {
                        if (!Model.BeforePhotoList.Any(x => x.Seq == photo.Seq))
                        {
                            db.AbnormalPhoto.Remove(photo);

                            try
                            {
                                System.IO.File.Delete(Path.Combine(Config.EquipmentMaintenancePhotoFolderPath, string.Format("{0}_{1}_{2}.{3}", Model.FirstAbnormalUniqueID, photo.Type, photo.Seq, photo.Extension)));
                            }
                            catch { }
                        }
                    }

                    var afterPhotoList = db.AbnormalPhoto.Where(x => x.AbnormalUniqueID == Model.FirstAbnormalUniqueID && x.Type == "A").ToList();

                    foreach (var photo in afterPhotoList)
                    {
                        if (!Model.AfterPhotoList.Any(x => x.Seq == photo.Seq))
                        {
                            db.AbnormalPhoto.Remove(photo);

                            try
                            {
                                System.IO.File.Delete(Path.Combine(Config.EquipmentMaintenancePhotoFolderPath, string.Format("{0}_{1}_{2}.{3}", Model.FirstAbnormalUniqueID, photo.Type, photo.Seq, photo.Extension)));
                            }
                            catch { }
                        }
                    }

                    foreach (var photo in Model.BeforePhotoList)
                    {
                        if (!photo.IsSaved)
                        {
                            db.AbnormalPhoto.Add(new AbnormalPhoto()
                            {
                                AbnormalUniqueID = Model.FirstAbnormalUniqueID,
                                Seq = photo.Seq,
                                Extension = photo.Extension,
                                Type = photo.Type
                            });

                            System.IO.File.Move(photo.TempFullFileName, Path.Combine(Config.EquipmentMaintenancePhotoFolderPath, string.Format("{0}_{1}_{2}.{3}", Model.FirstAbnormalUniqueID, photo.Type, photo.Seq, photo.Extension)));
                        }
                    }

                    foreach (var photo in Model.AfterPhotoList)
                    {
                        if (!photo.IsSaved)
                        {
                            db.AbnormalPhoto.Add(new AbnormalPhoto()
                            {
                                AbnormalUniqueID = Model.FirstAbnormalUniqueID,
                                Seq = photo.Seq,
                                Extension = photo.Extension,
                                Type = photo.Type
                            });

                            System.IO.File.Move(photo.TempFullFileName, Path.Combine(Config.EquipmentMaintenancePhotoFolderPath, string.Format("{0}_{1}_{2}.{3}", Model.FirstAbnormalUniqueID, photo.Type, photo.Seq, photo.Extension)));
                        }
                    }

                    var fileList = db.AbnormalFile.Where(x => x.AbnormalUniqueID == Model.FirstAbnormalUniqueID).ToList();

                    foreach (var file in fileList)
                    {
                        if (!Model.FileList.Any(x => x.Seq == file.Seq))
                        {
                            db.AbnormalFile.Remove(file);

                            try
                            {
                                System.IO.File.Delete(Path.Combine(Config.EquipmentMaintenanceFileFolderPath, string.Format("{0}_{1}.{2}", Model.FirstAbnormalUniqueID, file.Seq, file.Extension)));
                            }
                            catch { }
                        }
                    }

                    foreach (var file in Model.FileList)
                    {
                        if (!file.IsSaved)
                        {
                            db.AbnormalFile.Add(new AbnormalFile()
                            {
                                AbnormalUniqueID = Model.FirstAbnormalUniqueID,
                                Seq = file.Seq,
                                Extension = file.Extension,
                                FileName = file.FileName,
                                UploadTime = file.UploadTime,
                                ContentLength = file.Size
                            });

                            System.IO.File.Move(file.TempFullFileName, Path.Combine(Config.EquipmentMaintenanceFileFolderPath, string.Format("{0}_{1}.{2}", Model.FirstAbnormalUniqueID, file.Seq, file.Extension)));
                        }
                    }

                    db.SaveChanges();

                    result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Save, Resources.Resource.Success));
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

        public static RequestResult Closed(EditFormModel Model, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                result = Save(Model);

                if (result.IsSuccess)
                {
                    if (string.IsNullOrEmpty(Model.ClosedRemark))
                    {
                        result.ReturnFailedMessage(Resources.Resource.CommentRequired);
                    }
                    else
                    {
                        using (EDbEntities db = new EDbEntities())
                        {
                            var abnormals = (from a in db.Abnormal
                                             join x in db.AbnormalCheckResult
                                             on a.UniqueID equals x.AbnormalUniqueID
                                             join c in db.CheckResult
                                             on x.CheckResultUniqueID equals c.UniqueID
                                             where c.EquipmentUniqueID == Model.EquipmentUniqueID && c.CheckDate == Model.CheckDate
                                             select a).ToList();

                            foreach (var abnormal in abnormals)
                            {
                                abnormal.ClosedTime = DateTime.Now;
                                abnormal.ClosedUserID = Account.ID;
                            }

                            db.SaveChanges();
                        }

                        result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Closed, Resources.Resource.Success));
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
    }
}
