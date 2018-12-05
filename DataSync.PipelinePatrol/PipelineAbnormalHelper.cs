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
    public class PipelineAbnormalHelper
    {
        public static RequestResult Create(string UniqueID, PipelineAbnormalFormInput Model, string TempPhotoFolder)
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

                        string vhno = string.Format("{0}{1}", preFix, (db.PipelineAbnormal.Count(x => x.VHNO.StartsWith(preFix)) + 1).ToString().PadLeft(2, '0'));

                        db.PipelineAbnormal.Add(new PipelineAbnormal()
                        {
                            UniqueID = UniqueID,
                            VHNO = vhno,
                            LAT = Model.LAT,
                            LNG = Model.LNG,
                            Description = Model.Description,
                            PipePointUniqueID = Model.PipePointUniqueID,
                            Address = Model.Address,
                            CreateTime = DateTime.Now,
                            CreateUserID = Model.UserID,
                            AbnormalReasonUniqueID = Model.AbnormalReasonUniqueID,
                            AbnormalReasonRemark = Model.AbnormalReasonRemark
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

                        DB.PipelineAbnormalPhoto.Add(new PipelineAbnormalPhoto()
                        {
                            PipelineAbnormalUniqueID = UniqueID,
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
                    var pipelineAbnormal = db.PipelineAbnormal.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new PipelineAbnormalModel()
                    {
                        UniqueID = pipelineAbnormal.UniqueID,
                        VHNO = pipelineAbnormal.VHNO,
                        Address = pipelineAbnormal.Address,
                        CreateUserID = pipelineAbnormal.CreateUserID,
                        CreateTime = pipelineAbnormal.CreateTime,
                        LAT = pipelineAbnormal.LAT,
                        LNG = pipelineAbnormal.LNG,
                        Description = pipelineAbnormal.Description,
                        PipePointUniqueID = pipelineAbnormal.PipePointUniqueID,
                        AbnormalReasonUniqueID = pipelineAbnormal.AbnormalReasonUniqueID,
                        AbnormalReasonRemark = pipelineAbnormal.AbnormalReasonRemark
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
                    var photoList = db.PipelineAbnormalPhoto.Where(x => x.PipelineAbnormalUniqueID == UniqueID).ToList();

                    var zipFileName = Path.Combine(Config.TempFolder, string.Format("{0}{1}.zip", UniqueID, DateTimeHelper.DateTime2DateTimeString(DateTime.Now)));

                    var zipFile = new ZipFile(zipFileName);

                    foreach (var photo in photoList)
                    {
                        zipFile.AddFile(Path.Combine(Config.PipelinePatrolPhotoFolderPath, string.Format("{0}_{1}.{2}", photo.PipelineAbnormalUniqueID, photo.Seq, photo.Extension)), "");
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
                    var fileList = db.PipelineAbnormalFile.Where(x => x.PipelineAbnormalUniqueID == UniqueID).ToList();

                    var zipFileName = Path.Combine(Config.TempFolder, string.Format("{0}{1}.zip", UniqueID, DateTimeHelper.DateTime2DateTimeString(DateTime.Now)));

                    var zipFile = new ZipFile(zipFileName);

                    foreach (var file in fileList)
                    {
                        zipFile.AddFile(Path.Combine(Config.PipelinePatrolFileFolderPath, string.Format("{0}_{1}.{2}", file.PipelineAbnormalUniqueID, file.Seq, file.Extension)), "");
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
                List<PipelineAbnormalModel> viewModel = new List<PipelineAbnormalModel>();
                using (PDbEntities db = new PDbEntities())
                {
                    //var photoMap = new Dictionary<string, List<ConstructionPhotoModel>>();
                    //var fileMap = new Dictionary<string, List<ConstructionFileModel>>();
                    viewModel = db.PipelineAbnormal.Where(x => vhnoList.Contains(x.VHNO)).Select(x => new PipelineAbnormalModel
                    {
                        UniqueID = x.UniqueID,
                        VHNO = x.VHNO,
                        Address = x.Address,
                        CreateUserID = x.CreateUserID,
                        CreateTime = x.CreateTime,
                        LAT = x.LAT,
                        LNG = x.LNG,
                        Description = x.Description,
                        PipePointUniqueID = x.PipePointUniqueID,
                        AbnormalReasonUniqueID = x.AbnormalReasonUniqueID,
                        AbnormalReasonRemark = x.AbnormalReasonRemark


                    }).ToList();


                   

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
