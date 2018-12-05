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

namespace DataSync.PipelinePatrol
{
    public class ConstructionHelper
    {
        public static RequestResult Create(string UniqueID, ConstructionFormInput Model, string TempPhotoFolder)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    result = SavePhoto(db, UniqueID, TempPhotoFolder, false);

                    if (result.IsSuccess)
                    {
                        //CyyMMdd
                        string preFix = string.Format("C{0}", DateTimeHelper.DateTime2DateString(DateTime.Today).Substring(2));

                        var query = db.Construction.Where(x => x.VHNO.StartsWith(preFix)).OrderByDescending(x => x.VHNO).ToList();

                        int seq = 1;

                        if (query.Count > 0)
                        {
                            seq = int.Parse(query.First().VHNO.Substring(7)) + 1;
                        }

                        //CyyMMddxx
                        string vhno = string.Format("{0}{1}", preFix, seq.ToString().PadLeft(2, '0'));

                        db.Construction.Add(new Construction()
                        {
                            UniqueID = UniqueID,
                            VHNO = vhno,
                            LAT = Model.LAT,
                            LNG = Model.LNG,
                            Description = Model.Description,
                            BeginDate = Model.BeginDate,
                            EndDate = Model.EndDate,
                            InspectionUniqueID = Model.InspectionUniqueID,
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

        public static RequestResult Closed(string UniqueID, ConstructionFormInput Model, string TempPhotoFolder)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    result = SavePhoto(db, UniqueID, TempPhotoFolder, true);

                    if (result.IsSuccess)
                    {
                        var construction = db.Construction.First(x => x.UniqueID == UniqueID);
                        var currentDate = DateTime.Now;
                        construction.ClosedTime = currentDate ;
                        construction.ClosedUserID = Model.UserID;
                        construction.ClosedRemark = Model.Remark;
                        
                        // 如果這個施工單有 會勘單,會勘單會被結案掉
                        if (!string.IsNullOrEmpty(construction.InspectionUniqueID))
                        {
                            var inspection = db.Inspection.Where(x => x.UniqueID == construction.InspectionUniqueID).FirstOrDefault();
                            if (inspection != null)
                            {
                                inspection.ClosedTime = currentDate;
                                inspection.ClosedUserID = Model.UserID;
                                inspection.ClosedRemark = string.Format("因施工單:{0} 結案,此案一併結案.",construction.VHNO);
                            }
                        }


                        db.SaveChanges();

                        result.Success();
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

        private static RequestResult SavePhoto(PDbEntities DB, string UniqueID, string TempPhotoFolder, bool IsClosed)
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

                    if (IsClosed)
                    {
                        var photo = DB.ConstructionPhoto.Where(x => x.ConstructionUniqueID == UniqueID).OrderByDescending(x => x.Seq).FirstOrDefault();

                        if (photo != null)
                        {
                            seq = photo.Seq + 1;
                        }
                    }

                    foreach (var file in files)
                    {
                        var extension = new FileInfo(file).Extension.Replace(".", "");

                        DB.ConstructionPhoto.Add(new ConstructionPhoto()
                        {
                            ConstructionUniqueID = UniqueID,
                            Seq = seq,
                            Extension = extension,
                            IsClosed = IsClosed
                        });

                        System.IO.File.Move(file, Path.Combine(Config.PipelinePatrolPhotoFolderPath, string.Format("{0}_{1}.{2}", UniqueID, seq, extension)));

                        using (ZipFile zip = new ZipFile())
                        {
                            zip.AddFile(Path.Combine(Config.PipelinePatrolPhotoFolderPath, string.Format("{0}_{1}.{2}", UniqueID, seq, extension)), "");

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
                    var construction = db.Construction.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new ConstructionModel()
                    {
                        UniqueID = construction.UniqueID,
                        VHNO = construction.VHNO,
                        ConstructionFirmUniqueID = construction.ConstructionFirmUniqueID,
                        ConstructionFirmRemark = construction.ConstructionFirmRemark,
                        ConstructionTypeUniqueID = construction.ConstructionTypeUniqueID,
                        ConstructionTypeRemark = construction.ConstructionTypeRemark,
                        BeginDate = construction.BeginDate,
                        EndDate = construction.EndDate,
                        Address = construction.Address,
                        CreateUserID = construction.CreateUserID,
                        CreateTime = construction.CreateTime,
                        LAT = construction.LAT,
                        LNG = construction.LNG,
                        Description = construction.Description,
                        PipePointUniqueID = construction.PipePointUniqueID,
                        InspectionUniqueID = construction.InspectionUniqueID,
                        Photos = db.ConstructionPhoto.Where(x => x.ConstructionUniqueID == UniqueID).AsEnumerable().Select(x => new ConstructionPhotoModel
                        {

                            ConstructionUniqueID = x.ConstructionUniqueID,
                            Seq = x.Seq,
                            FileName = string.Format("{0}_{1}.{2}", x.ConstructionUniqueID, x.Seq, x.Extension),
                            FilePath = ""
                        }).OrderBy(x => x.Seq).ToList(),
                        Files = db.ConstructionFile.Where(x => x.ConstructionUniqueID == UniqueID).AsEnumerable().Select(x => new ConstructionFileModel
                        {

                            ConstructionUniqueID = x.ConstructionUniqueID,
                            Seq = x.Seq,
                            FileName = string.Format("{0}_{1}.{2}", x.ConstructionUniqueID, x.Seq, x.Extension),
                            FilePath = ""
                        }).OrderBy(x => x.Seq).ToList()
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

        public static RequestResult GetPhoto(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    var photoList = db.ConstructionPhoto.Where(x => x.ConstructionUniqueID == UniqueID).ToList();

                    var zipFileName = Path.Combine(Config.TempFolder, string.Format("{0}{1}.zip", UniqueID, DateTimeHelper.DateTime2DateTimeString(DateTime.Now)));

                    var zipFile = new ZipFile(zipFileName);

                    foreach (var photo in photoList)
                    {
                        zipFile.AddFile(Path.Combine(Config.PipelinePatrolPhotoFolderPath, string.Format("{0}_{1}.{2}", photo.ConstructionUniqueID, photo.Seq, photo.Extension)), "");
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

        public static RequestResult GetFile(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    var fileList = db.ConstructionFile.Where(x => x.ConstructionUniqueID == UniqueID).ToList();

                    var zipFileName = Path.Combine(Config.TempFolder, string.Format("{0}{1}.zip", UniqueID, DateTimeHelper.DateTime2DateTimeString(DateTime.Now)));

                    var zipFile = new ZipFile(zipFileName);

                    foreach (var file in fileList)
                    {
                        zipFile.AddFile(Path.Combine(Config.PipelinePatrolFileFolderPath, string.Format("{0}_{1}.{2}", file.ConstructionUniqueID, file.Seq, file.Extension)), "");
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
                List<ConstructionModel> viewModel = new List<ConstructionModel>();
                using (PDbEntities db = new PDbEntities())
                {
                    var photoMap = new Dictionary<string, List<ConstructionPhotoModel>>();
                    var fileMap = new Dictionary<string, List<ConstructionFileModel>>();
                    viewModel = db.Construction.Where(x => vhnoList.Contains(x.VHNO)).Select(x => new ConstructionModel
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
                        PipePointUniqueID = x.PipePointUniqueID,
                        InspectionUniqueID = x.InspectionUniqueID
                    
                    
                    }).ToList();


                    var uniqueIdList = viewModel.Select(x => x.UniqueID).ToList();

                    //photo
                    photoMap = db.ConstructionPhoto.Where(x => uniqueIdList.Contains(x.ConstructionUniqueID)).AsEnumerable()
                        .GroupBy(x => x.ConstructionUniqueID)
                        .ToDictionary(x => x.Key, x => x.Select(y=>  new ConstructionPhotoModel {

                            ConstructionUniqueID = y.ConstructionUniqueID,
                            Seq = y.Seq,
                            FileName = string.Format("{0}_{1}.{2}", y.ConstructionUniqueID, y.Seq, y.Extension),
                            FilePath = ""
                        
                        }).ToList());

                    //file
                    fileMap = db.ConstructionFile.Where(x => uniqueIdList.Contains(x.ConstructionUniqueID)).AsEnumerable()
                        .GroupBy(x => x.ConstructionUniqueID)
                        .ToDictionary(x => x.Key, x => x.Select(y => new ConstructionFileModel {
                            ConstructionUniqueID = y.ConstructionUniqueID,
                            Seq = y.Seq,
                            FileName = string.Format("{0}_{1}.{2}", y.ConstructionUniqueID, y.Seq, y.Extension),
                            FilePath = ""
                        
                        }).ToList());
                    
          

                    //loop

                    foreach (var item in viewModel)
                    {
                        
                        if(photoMap.ContainsKey(item.UniqueID))
                        {
                            item.Photos = photoMap[item.UniqueID];
                        }

                        if (fileMap.ContainsKey(item.UniqueID))
                        {
                            item.Files = fileMap[item.UniqueID];
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
    }
}
