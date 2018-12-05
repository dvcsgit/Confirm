using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Data.SQLite;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using Utility;
using Utility.Models;
using DbEntity.ASE;
using DataAccess;
using Models.ASE.QA.DataSync;
using System.Collections.Generic;

namespace DataAccess.ASE.QA
{
    public class DownloadHelper : IDisposable
    {
        private string Guid;

        private DownloadDataModel DataModel = new DownloadDataModel();

        public RequestResult Generate()
        {
            RequestResult result = new RequestResult();

            try
            {
                result = Init();

                if (result.IsSuccess)
                {
                    result = Query();

                    if (result.IsSuccess)
                    {
                        result = GenerateSQLite();

                        if (result.IsSuccess)
                        {
                            result = GenerateZip();
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

        private RequestResult Init()
        {
            RequestResult result = new RequestResult();

            try
            {
                Guid = System.Guid.NewGuid().ToString();

                Directory.CreateDirectory(GeneratedFolderPath);

                System.IO.File.Copy(TemplateDbFilePath, GeneratedDbFilePath);

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

        private RequestResult Query()
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query1 = (from form in db.QA_CALIBRATIONFORM
                                  join apply in db.QA_CALIBRATIONAPPLY
                                  on form.APPLYUNIQUEID equals apply.UNIQUEID 
                                  where form.STATUS == "1"
                                  select new
                                  {
                                      UniqueID = form.UNIQUEID,
                                      form.VHNO,
                                      EquipmentUniqueID = apply.EQUIPMENTUNIQUEID,
                                      CalibratorID = form.CALRESPONSORID,
                                      CalType = apply.CALTYPE,
                                      CalUnit = apply.CALUNIT
                                  }).Where(x => !string.IsNullOrEmpty(x.EquipmentUniqueID) && x.CalUnit == "L" && (x.CalType == "IL" || x.CalType == "IF")).ToList();

                    var query2 = (from form in db.QA_CALIBRATIONFORM
                                  join notify in db.QA_CALIBRATIONNOTIFY
                                  on form.NOTIFYUNIQUEID equals notify.UNIQUEID
                                  where form.STATUS == "1"
                                  select new
                                  {
                                      UniqueID = form.UNIQUEID,
                                      form.VHNO,
                                      EquipmentUniqueID = notify.EQUIPMENTUNIQUEID,
                                      CalibratorID = form.CALRESPONSORID,
                                      CalType = notify.CALTYPE,
                                      CalUnit = notify.CALUNIT
                                  }).Where(x => !string.IsNullOrEmpty(x.EquipmentUniqueID) && x.CalUnit == "L" && (x.CalType == "IL" || x.CalType == "IF")).ToList();

                    var formList = query1.Union(query2).ToList();

                    foreach (var form in formList)
                    {
                        bool canEdit = false;

                        if (form.CalType == "IL")
                        {
                            var query = db.QA_CALIBRATIONFORMSTEPLOG.Where(x => x.FORMUNIQUEID == form.UniqueID).OrderByDescending(x => x.SEQ).FirstOrDefault();

                            if (query != null && query.STEP == "1")
                            {
                                canEdit = true;
                            }
                        }
                        else
                        {
                            canEdit = true;
                        }

                        if (canEdit)
                        {
                            var equipment = EquipmentHelper.Get(null, form.EquipmentUniqueID);

                            var jobModel = new JobModel()
                            {
                                UniqueID = form.UniqueID,
                                OrganizationUniqueID = equipment.OrganizationUniqueID,
                                JobDescription = equipment.CalNo,
                                RouteID = form.VHNO,
                                RouteName = equipment.IchiDisplay,
                                TimeMode = 1,
                                BeginTime = string.Empty,
                                EndTime = string.Empty,
                                IsCheckBySeq = false,
                                IsShowPrevRecord = false,
                                Remark = string.Empty,
                                LastModifyTime = LastModifyTimeHelper.Get(form.UniqueID),
                                User = new UserModel() { ID = form.CalibratorID },
                                STDUSEList = (from x in db.QA_CALIBRATIONFORMSTDUSE
                                              join e in db.QA_EQUIPMENT
                                              on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                              where x.FORMUNIQUEID == form.UniqueID
                                              select e.CALNO).ToList()
                            };

                            var controlPointModel = new ControlPointModel()
                            {
                                UniqueID = form.UniqueID,
                                ID = equipment.CalNo,
                                Description = equipment.IchiDisplay,
                                IsFeelItemDefaultNormal = false,
                                TagID = string.Empty,
                                MinTimeSpan = null,
                                Remark = string.Empty,
                                Seq = 1
                            };

                            var itemList = db.QA_CALIBRATIONFORMDETAIL.Where(x => x.FORMUNIQUEID == form.UniqueID).OrderBy(x => x.SEQ).ToList();

                            //var seq = 1;

                            foreach (var item in itemList)
                            {
                                var checkItemModel = new CheckItemModel()
                                {
                                    UniqueID = string.Format("{0}_{1}", item.FORMUNIQUEID, item.SEQ),
                                    ID = item.CHARACTERISTIC,
                                    Description = item.CALIBRATIONPOINT.ToString(),
                                    Tolerance = item.TOLERANCE,
                                    ToleranceSymbol = item.TOLERANCESYMBOL,
                                    ToleranceRate = item.TOLERANCEUNITRATE,
                                    IsFeelItem = false,
                                    LowerLimit = null,
                                    LowerAlertLimit = null,
                                    UpperAlertLimit = null,
                                    UpperLimit = null,
                                    Remark = string.Empty,
                                    Unit = item.TOLERANCEUNIT,
                                    Standard = item.STANDARD,
                                    ReadingValue = item.READINGVALUE,
                                    Seq = item.SEQ
                                };

                                //seq++;

                                controlPointModel.CheckItemList.Add(checkItemModel);
                            }
                            
                            if (controlPointModel.CheckItemList.Count > 0)
                            {
                                jobModel.ControlPointList.Add(controlPointModel);
                            }

                            if (jobModel.ControlPointList.Count > 0)
                            {
                                DataModel.JobList.Add(jobModel);
                            }
                        }
                    }

                    DataModel.UserList = (from x in db.USERAUTHGROUP
                                          join u in db.ACCOUNT
                                          on x.USERID equals u.ID
                                          where x.AUTHGROUPID == "QA" || x.AUTHGROUPID == "QA-Verify"
                                          select u).Distinct().Select(x => new UserModel
                                          {
                                              ID = x.ID,
                                              Name = x.NAME,
                                              Title = x.TITLE,
                                              Password = x.PASSWORD,
                                              UID = x.TAGID
                                          }).ToList();

                    foreach (var job in DataModel.JobList)
                    {
                        var acocunt = db.ACCOUNT.First(x => x.ID == job.User.ID);

                        job.User = new UserModel()
                        {
                            ID = acocunt.ID,
                            Name = acocunt.NAME,
                            Password = acocunt.PASSWORD,
                            Title = acocunt.TITLE,
                            UID = acocunt.TAGID
                        };
                    }
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

        private RequestResult GenerateSQLite()
        {
            RequestResult result = new RequestResult();

            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(this.SQLiteConnString))
                {
                    conn.Open();

                    using (SQLiteTransaction trans = conn.BeginTransaction())
                    {
                        foreach (var job in DataModel.JobList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO Job (UniqueID, Description, Remark, TimeMode, BeginTime, EndTime, IsCheckBySeq, IsShowPrevRecord, UserID, CALNO) VALUES (@UniqueID, @Description, @Remark, @TimeMode, @BeginTime, @EndTime, @IsCheckBySeq, @IsShowPrevRecord, @UserID, @CALNO)";

                                cmd.Parameters.AddWithValue("UniqueID", job.UniqueID);
                                cmd.Parameters.AddWithValue("Description", job.Description);
                                cmd.Parameters.AddWithValue("Remark", job.Remark);
                                cmd.Parameters.AddWithValue("TimeMode", job.TimeMode);
                                cmd.Parameters.AddWithValue("BeginTime", job.BeginTime);
                                cmd.Parameters.AddWithValue("EndTime", job.EndTime);
                                cmd.Parameters.AddWithValue("IsCheckBySeq", job.IsCheckBySeq ? "Y" : "N");
                                cmd.Parameters.AddWithValue("IsShowPrevRecord", job.IsShowPrevRecord ? "Y" : "N");
                                cmd.Parameters.AddWithValue("UserID", job.User.ID);
                                cmd.Parameters.AddWithValue("CALNO", job.JobDescription);

                                cmd.ExecuteNonQuery();
                            }

                            foreach (var stduse in job.STDUSEList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO STDUSE (CFormUniqueID, CALNO) VALUES (@CFormUniqueID, @CALNO)";

                                    cmd.Parameters.AddWithValue("CFormUniqueID", job.UniqueID);
                                    cmd.Parameters.AddWithValue("CALNO", stduse);

                                    cmd.ExecuteNonQuery();
                                }
                            }

                            foreach (var controlPoint in job.ControlPointList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO ControlPoint (JobUniqueID, ControlPointUniqueID, ID, Description, IsFeelItemDefaultNormal, TagID, MinTimeSpan, Remark, Seq) VALUES (@JobUniqueID, @ControlPointUniqueID, @ID, @Description, @IsFeelItemDefaultNormal, @TagID, @MinTimeSpan, @Remark, @Seq)";

                                    cmd.Parameters.AddWithValue("JobUniqueID", job.UniqueID);
                                    cmd.Parameters.AddWithValue("ControlPointUniqueID", controlPoint.UniqueID);
                                    cmd.Parameters.AddWithValue("ID", controlPoint.ID);
                                    cmd.Parameters.AddWithValue("Description", controlPoint.Description);
                                    cmd.Parameters.AddWithValue("IsFeelItemDefaultNormal", controlPoint.IsFeelItemDefaultNormal ? "Y" : "N");
                                    cmd.Parameters.AddWithValue("TagID", controlPoint.TagID);
                                    cmd.Parameters.AddWithValue("MinTimeSpan", controlPoint.MinTimeSpan);
                                    cmd.Parameters.AddWithValue("Remark", controlPoint.Remark);
                                    cmd.Parameters.AddWithValue("Seq", controlPoint.Seq);

                                    cmd.ExecuteNonQuery();
                                }

                                foreach (var checkItem in controlPoint.CheckItemList)
                                {
                                    using (SQLiteCommand cmd = conn.CreateCommand())
                                    {
                                        cmd.CommandText = "INSERT INTO CheckItem (JobUniqueID, ControlPointUniqueID, EquipmentUniqueID, PartUniqueID, CheckItemUniqueID, ID, Description, IsFeelItem, LowerLimit, LowerAlertLimit, UpperAlertLimit, UpperLimit, Tolerance, ToleranceSymbol, ToleranceRate, Unit, Remark, Seq, Standard, ReadingValue) VALUES (@JobUniqueID, @ControlPointUniqueID, @EquipmentUniqueID, @PartUniqueID, @CheckItemUniqueID, @ID, @Description, @IsFeelItem, @LowerLimit, @LowerAlertLimit, @UpperAlertLimit, @UpperLimit, @Tolerance, @ToleranceSymbol, @ToleranceRate, @Unit, @Remark, @Seq, @Standard, @ReadingValue)";

                                        cmd.Parameters.AddWithValue("JobUniqueID", job.UniqueID);
                                        cmd.Parameters.AddWithValue("ControlPointUniqueID", controlPoint.UniqueID);
                                        cmd.Parameters.AddWithValue("EquipmentUniqueID", "");
                                        cmd.Parameters.AddWithValue("PartUniqueID", "");
                                        cmd.Parameters.AddWithValue("CheckItemUniqueID", checkItem.UniqueID);
                                        cmd.Parameters.AddWithValue("ID", checkItem.ID);
                                        cmd.Parameters.AddWithValue("Description", checkItem.Description);
                                        cmd.Parameters.AddWithValue("IsFeelItem", checkItem.IsFeelItem ? "Y" : "N");
                                        cmd.Parameters.AddWithValue("LowerLimit", checkItem.LowerLimit);
                                        cmd.Parameters.AddWithValue("LowerAlertLimit", checkItem.LowerAlertLimit);
                                        cmd.Parameters.AddWithValue("UpperAlertLimit", checkItem.UpperAlertLimit);
                                        cmd.Parameters.AddWithValue("UpperLimit", checkItem.UpperLimit);
                                        cmd.Parameters.AddWithValue("Tolerance", checkItem.Tolerance);
                                        cmd.Parameters.AddWithValue("ToleranceSymbol", checkItem.ToleranceSymbol);
                                        cmd.Parameters.AddWithValue("ToleranceRate", checkItem.ToleranceRate);
                                        cmd.Parameters.AddWithValue("Unit", checkItem.Unit);
                                        cmd.Parameters.AddWithValue("Remark", checkItem.Remark);
                                        cmd.Parameters.AddWithValue("Seq", checkItem.Seq);
                                        cmd.Parameters.AddWithValue("Standard", checkItem.Standard);
                                        cmd.Parameters.AddWithValue("ReadingValue", checkItem.ReadingValue);

                                        cmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                        
                        foreach (var lasyModifyTime in DataModel.LastModifyTimeList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO LastModifyTime (JobUniqueID, VersionTime) VALUES (@JobUniqueID, @VersionTime)";

                                cmd.Parameters.AddWithValue("JobUniqueID", lasyModifyTime.Key);
                                cmd.Parameters.AddWithValue("VersionTime", lasyModifyTime.Value);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        
                        foreach (var user in DataModel.AllUserList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO [User] (ID, Title, Name, Password, UID) VALUES (@ID, @Title, @Name, @Password, @UID)";

                                cmd.Parameters.AddWithValue("ID", user.ID);
                                cmd.Parameters.AddWithValue("Title", user.Title);
                                cmd.Parameters.AddWithValue("Name", user.Name);
                                cmd.Parameters.AddWithValue("Password", user.Password);
                                cmd.Parameters.AddWithValue("UID", user.UID);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        trans.Commit();
                    }

                    conn.Close();
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

        private RequestResult GenerateZip()
        {
            RequestResult result = new RequestResult();

            try
            {
                using (FileStream fs = System.IO.File.Create(GeneratedZipFilePath))
                {
                    using (ZipOutputStream zipStream = new ZipOutputStream(fs))
                    {
                        zipStream.SetLevel(9); //0-9, 9 being the highest level of compression

                        ZipHelper.CompressFolder(GeneratedDbFilePath, zipStream);

                        zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream

                        zipStream.Close();
                    }
                }

                result.ReturnData(GeneratedZipFilePath);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        private string TemplateDbFilePath
        {
            get
            {
                return Path.Combine(Config.QASQLiteTemplateFolderPath, Define.SQLite_EquipmentMaintenance);
            }
        }

        private string GeneratedFolderPath
        {
            get
            {
                return Path.Combine(Config.QASQLiteGeneratedFolderPath, Guid);
            }
        }

        private string GeneratedDbFilePath
        {
            get
            {
                return Path.Combine(GeneratedFolderPath, Define.SQLite_EquipmentMaintenance);
            }
        }

        private string GeneratedZipFilePath
        {
            get
            {
                return Path.Combine(GeneratedFolderPath, Define.SQLiteZip_EquipmentMaintenance);
            }
        }

        private string SQLiteConnString
        {
            get
            {
                return string.Format("Data Source={0};Version=3;", GeneratedDbFilePath);
            }
        }

        #region IDisposable

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {

                }
            }

            _disposed = true;
        }

        ~DownloadHelper()
        {
            Dispose(false);
        }
        #endregion
    }
}
