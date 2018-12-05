using DbEntity.MSSQL;
using DbEntity.MSSQL.PipelinePatrol;
using Ionic.Zip;
using Models.PipelinePatrol.DataSync;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;
using Models.PipelinePatrol.Shared;
using Newtonsoft.Json;

namespace DataSync.PipelinePatrol
{
    public class InspectionHelper
    {
        public static RequestResult Create(string UniqueID, InspectionFormInput Model, string TempPhotoFolder)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    result = SavePhoto(db, UniqueID, TempPhotoFolder);

                    if (result.IsSuccess)
                    {
                        string preFix = DateTimeHelper.DateTime2DateString(DateTime.Today);

                        string vhno = string.Format("{0}{1}", preFix, (db.Inspection.Count(x => x.VHNO.StartsWith(preFix)) + 1).ToString().PadLeft(2, '0'));

                        db.Inspection.Add(new Inspection()
                        {
                            UniqueID = UniqueID,
                            VHNO = vhno,
                            LAT = Model.LAT,
                            LNG = Model.LNG,
                            Description = Model.Description,
                            BeginDate = Model.BeginDate,
                            EndDate = Model.EndDate,
                            PipePointUniqueID = Model.PipePointUniqueID,
                            Address = Model.Address,
                            ConstructionFirmUniqueID = Model.ConstructionFirmUniqueID,
                            ConstructionFirmRemark = Model.ConstructionFirmRemark,
                            ConstructionTypeUniqueID = Model.ConstructionTypeUniqueID,
                            ConstructionTypeRemark = Model.ConstructionTypeRemark,
                            CreateTime = DateTime.Now,
                            CreateUserID = Model.UserID
                        });

                        db.SaveChanges();

                        result.ReturnData(UniqueID);
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

        private static RequestResult SavePhoto(PDbEntities DB, string UniqueID, string TempPhotoFolder)
        {
            RequestResult result = new RequestResult();

            try
            {
                if (!string.IsNullOrEmpty(TempPhotoFolder))
                {
                    var extractFolder = Path.Combine(TempPhotoFolder, "Extract");

                    Directory.CreateDirectory(extractFolder);

                    var zipFile = Path.Combine(TempPhotoFolder, UniqueID + ".zip");

                    using (ZipFile zip = ZipFile.Read(zipFile))
                    {
                        foreach (ZipEntry entry in zip)
                        {
                            entry.Extract(extractFolder);
                        }
                    }

                    string[] files = Directory.GetFiles(extractFolder);

                    int seq = 1;

                    foreach (var file in files)
                    {
                        var extension = new FileInfo(file).Extension.Replace(".", "");

                        DB.InspectionPhoto.Add(new InspectionPhoto()
                        {
                            InspectionUniqueID = UniqueID,
                            Seq = seq,
                            Extension = extension
                        });

                        System.IO.File.Move(file, Path.Combine(Config.PipelinePatrolPhotoFolderPath, string.Format("{0}_{1}.{2}", UniqueID, seq, extension)));

                        using (ZipFile zip = new ZipFile())
                        {
                            zip.AddFile(Path.Combine(Config.PipelinePatrolPhotoFolderPath, string.Format("{0}_{1}.{2}", UniqueID, seq, extension)));

                            zip.Save(Path.Combine(Config.PipelinePatrolPhotoFolderPath, string.Format("{0}_{1}.zip", UniqueID, seq)));
                        }

                        seq++;
                    }

                    Directory.Delete(TempPhotoFolder, true);

                    result.Success();
                }
                else
                {
                    result.Success();
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

        public static RequestResult Get(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    var inspection = db.Inspection.First(x => x.UniqueID == UniqueID);

                    var model = new InspectionModel()
                    {
                        UniqueID = inspection.UniqueID,
                        VHNO = inspection.VHNO,
                        ConstructionFirmUniqueID = inspection.ConstructionFirmUniqueID,
                        ConstructionFirmRemark = inspection.ConstructionFirmRemark,
                        ConstructionTypeUniqueID = inspection.ConstructionTypeUniqueID,
                        ConstructionTypeRemark = inspection.ConstructionTypeRemark,
                        BeginDate = inspection.BeginDate,
                        EndDate = inspection.EndDate,
                        Address = inspection.Address,
                        CreateUserID = inspection.CreateUserID,
                        CreateTime = inspection.CreateTime,
                        LAT = inspection.LAT,
                        LNG = inspection.LNG,
                        Description = inspection.Description,
                        PipePointUniqueID = inspection.PipePointUniqueID
                    };

                    var photo = db.InspectionPhoto.Where(x => x.InspectionUniqueID == UniqueID).AsEnumerable().Select(x => new InspectionPhotoModel
                    {
                        InspectionUniqueID = x.InspectionUniqueID,
                        Seq = x.Seq,
                        FileName = string.Format("{0}_{1}.{2}", x.InspectionUniqueID, x.Seq, x.Extension),
                        FilePath = ""
                    }).OrderBy(x => x.Seq).ToList();

                    model.Photos = photo;

                    var userList = db.InspectionUser.Where(x => x.InspectionUniqueID == UniqueID).ToList();

                    foreach (var user in userList)
                    {
                        model.UserList.Add(new InspectionUserModel
                        {
                            InspectionUniqueID = user.InspectionUniqueID,
                            UserID = user.UserID,
                            Remark = user.Remark,
                            InspectTime = user.InspectTime,
                            //User = UserHelper.Get(user.UserID)
                        });
                    }

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

        /// <summary>
        /// 新增 InspectionUser
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        public static RequestResult Inspect(InspectionUserFormInput Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    db.InspectionUser.Add(new InspectionUser()
                    {
                        InspectionUniqueID = Model.InspectionUniqueID,
                        InspectTime = DateTime.Now,
                        UserID = Model.UserID,
                        Remark = Model.Remark
                    });

                    db.SaveChanges();
                }

                // 因前端要取得 存進去的資訊 
                var inspectionResult = GetInspectionUser(Model.InspectionUniqueID, Model.UserID);
                if (inspectionResult.IsSuccess)
                {
                    result.Success();
                    result.ReturnData(inspectionResult.Data);

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

        public static RequestResult GetPhoto(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    var photoList = db.InspectionPhoto.Where(x => x.InspectionUniqueID == UniqueID).ToList();

                    var zipFileName = Path.Combine(Config.TempFolder, string.Format("{0}{1}.zip", UniqueID, DateTimeHelper.DateTime2DateTimeString(DateTime.Now)));

                    var zipFile = new ZipFile(zipFileName);

                    foreach (var photo in photoList)
                    {
                        zipFile.AddFile(Path.Combine(Config.PipelinePatrolPhotoFolderPath, string.Format("{0}_{1}.{2}", photo.InspectionUniqueID, photo.Seq, photo.Extension)), "");
                    }

                    zipFile.Save();

                    result.ReturnData(zipFileName);
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

        public static RequestResult GetInspectionUser(string UniqueID, string UserID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    var inspectionUser = db.InspectionUser.First(x => x.InspectionUniqueID == UniqueID && x.UserID == UserID);



                    var inspection = db.Inspection.First(x => x.UniqueID == UniqueID);

                    var model = new InspectionUserModel()
                    {
                        InspectionUniqueID = inspectionUser.InspectionUniqueID,
                        UserID = inspectionUser.UserID,
                        Remark = inspectionUser.Remark,
                        InspectTime = inspectionUser.InspectTime
                    };

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

        /// <summary>
        /// 檢查會堪單 是否已被 同一個人員會堪過
        /// </summary>
        /// <param name="UniqueID"></param>
        /// <param name="UserID"></param>
        /// <returns>True => Exist ; False => Not Exist </returns>
        public static bool CheckIfInspectionUser(string UniqueID, string UserID)
        {
            var result = false;

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    result = db.InspectionUser.Where(x => x.InspectionUniqueID == UniqueID && x.UserID == UserID).Any();

                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);
                Logger.Log(err);

            }

            return result;
        }

        /// <summary>
        /// 檢查同步一次取回
        /// </summary>
        /// <param name="vhnoList"></param>
        /// <returns></returns>
        public static RequestResult Get(List<string> vhnoList)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<InspectionModel> viewModel = new List<InspectionModel>();
                using (PDbEntities db = new PDbEntities())
                {
                    var photoMap = new Dictionary<string, List<InspectionPhotoModel>>();
                    var userMap = new Dictionary<string, List<InspectionUserModel>>();
                    viewModel = db.Inspection.Where(x => vhnoList.Contains(x.VHNO)).Select(x => new InspectionModel
                    {
                        UniqueID = x.UniqueID,
                        VHNO = x.VHNO,
                        ConstructionFirmUniqueID = x.ConstructionFirmUniqueID,
                        ConstructionFirmRemark = x.ConstructionFirmRemark,
                        ConstructionTypeUniqueID = x.ConstructionTypeUniqueID,
                        ConstructionTypeRemark = x.ConstructionTypeRemark,
                        BeginDate = x.BeginDate,
                        EndDate = x.EndDate,
                        Address = x.Address,
                        CreateUserID = x.CreateUserID,
                        CreateTime = x.CreateTime,
                        LAT = x.LAT,
                        LNG = x.LNG,
                        Description = x.Description,
                        PipePointUniqueID = x.PipePointUniqueID


                    }).ToList();


                    var uniqueIdList = viewModel.Select(x => x.UniqueID).ToList();

                    //photo
                    photoMap = db.InspectionPhoto.Where(x => uniqueIdList.Contains(x.InspectionUniqueID)).AsEnumerable()
                        .GroupBy(x => x.InspectionUniqueID)
                        .ToDictionary(x => x.Key, x => x.Select(y => new InspectionPhotoModel
                        {
                            InspectionUniqueID = y.InspectionUniqueID,
                            Seq = y.Seq,
                            FileName = string.Format("{0}_{1}.{2}", y.InspectionUniqueID, y.Seq, y.Extension),
                            FilePath = ""

                        }).ToList());

                    //users
                    userMap = db.InspectionUser.Where(x => uniqueIdList.Contains(x.InspectionUniqueID)).AsEnumerable()
                        .GroupBy(x => x.InspectionUniqueID)
                        .ToDictionary(x => x.Key, x => x.Select(y => new InspectionUserModel
                        {
                            InspectionUniqueID = y.InspectionUniqueID,
                            UserID = y.UserID,
                            Remark = y.Remark,
                            InspectTime = y.InspectTime,

                        }).ToList());



                    //loop

                    foreach (var item in viewModel)
                    {

                        if (photoMap.ContainsKey(item.UniqueID))
                        {
                            item.Photos = photoMap[item.UniqueID];
                        }

                        if (userMap.ContainsKey(item.UniqueID))
                        {
                            item.UserList = userMap[item.UniqueID];
                        }

                    }

                }

                result.ReturnData(viewModel);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult Closed(InspectionCloseFormInput Model, string TempPhotoFolder)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (DbEntities mdb = new DbEntities())
                using (PDbEntities db = new PDbEntities())
                {
                    var inspection = db.Inspection.FirstOrDefault(x => x.UniqueID == Model.UniqueID);

                    if (inspection != null)
                    {
                        var currentDate = DateTime.Now;

                        // valid
                        var errors = new List<string>();

                        if (inspection.CreateUserID != Model.UserID)
                        {
                            var owner = mdb.User.Where(x => x.ID == inspection.CreateUserID).FirstOrDefault();
                            // 確認是不是本人
                            errors.Add(string.Format("會勘單開單人為:{0}/{1},您非本人.",owner.ID,owner.Name));
                        }

                        var relate_constuction = db.Construction.Where(x => x.InspectionUniqueID == Model.UniqueID).FirstOrDefault();
                        if (relate_constuction != null)
                        {
                            // 確認有沒有綁施工單
                            errors.Add(string.Format("因關聯施工單:{0},請直接對施工單進行結案", relate_constuction.VHNO));
                        }

                        if (inspection.ClosedTime.HasValue)
                        {
                            errors.Add(string.Format("此會勘單已於:{0}結案", inspection.ClosedTime.Value.ToString(DateFormateConsts.UI_S_yyyyMMddhhmmss)));
                        }

                        if (errors.Count > 0)
                        {
                            //result.Data = errors;
                            result.Message = JsonConvert.SerializeObject(errors);
                            result.IsSuccess = false;
                        }
                        else
                        {
                            inspection.ClosedTime = currentDate;
                            inspection.ClosedUserID = Model.UserID;
                            inspection.ClosedRemark = Model.Remark;
                            db.SaveChanges();
                            result.Success();
                        }



                    }
                    else
                    {
                        result.ReturnFailedMessage("會勘單不存在");
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
