using DataAccess;
using DataAccess.TruckPatrol;
using DbEntity.MSSQL;
using DbEntity.MSSQL.TruckPatrol;
using ICSharpCode.SharpZipLib.Zip;
using Models.TruckPatrol.DataSync;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace DataSync.TruckPatrol
{
    public class DownloadHelper : IDisposable
    {
        private string Guid;

        private DownloadDataModel DataModel = new DownloadDataModel();

        public RequestResult Generate(string UserID)
        {
            RequestResult result = new RequestResult();

            try
            {
                result = Init();

                if (result.IsSuccess)
                {
                    result = Query(UserID);

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

        private RequestResult Query(string UserID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var ancestorOrganizationUniqueID = string.Empty;

                using (DbEntities db = new DbEntities())
                {
                    ancestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(db.User.First(x => x.ID == UserID).OrganizationUniqueID);
                }

                var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(ancestorOrganizationUniqueID, true);

                using (TDbEntities db = new TDbEntities())
                {
                    #region UnRFIDReason
                    DataModel.UnRFIDReasonList = db.UnRFIDReason.Select(x => new Models.TruckPatrol.DataSync.UnRFIDReason
                    {
                        UniqueID = x.UniqueID,
                        ID = x.ID,
                        Description = x.Description,
                        LastModifyTime = x.LastModifyTime
                    }).ToList();
                    #endregion

                    #region Truck
                    var truckList = db.Truck.Where(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID)).ToList();

                    foreach (var truck in truckList)
                    {
                        var truckModel = new TruckModel()
                        {
                            UniqueID = truck.UniqueID,
                            OrganizationUniqueID = truck.OrganizationUniqueID,
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(truck.OrganizationUniqueID),
                            TruckNo = truck.TruckNo,
                            BindingType = truck.BindingType,
                            IsCheckBySeq = truck.IsCheckBySeq,
                            IsShowPrevRecord = truck.IsShowPrevRecord,
                            Remark = truck.Remark,
                            LastModifyTime = LastModifyTimeHelper.Get(truck.UniqueID)
                        };

                        #region ControlPoint
                        var truckControlPointList = db.ControlPoint.Where(x => x.TruckUniqueID == truck.UniqueID).ToList();

                        foreach (var controlPoint in truckControlPointList)
                        {
                            var controlPointModel = new ControlPointModel()
                            {
                                UniqueID = controlPoint.UniqueID,
                                ID = controlPoint.ID,
                                Description = controlPoint.Description,
                                IsFeelItemDefaultNormal = controlPoint.IsFeelItemDefaultNormal,
                                TagID = controlPoint.TagID,
                                Remark = controlPoint.Remark,
                                Seq = controlPoint.Seq
                            };

                            #region ControlPointCheckItem
                            var controlPointCheckItemList = db.View_ControlPointCheckItem.Where(x => x.ControlPointUniqueID == controlPoint.UniqueID).ToList();

                            foreach (var checkItem in controlPointCheckItemList)
                            {
                                var checkItemModel = new CheckItemModel()
                                {
                                    UniqueID = checkItem.CheckItemUniqueID,
                                    ID = checkItem.ID,
                                    Description = checkItem.Description,
                                    IsFeelItem = checkItem.IsFeelItem,
                                    LowerLimit = checkItem.LowerLimit,
                                    LowerAlertLimit = checkItem.LowerAlertLimit,
                                    UpperAlertLimit = checkItem.UpperAlertLimit,
                                    UpperLimit = checkItem.UpperLimit,
                                    Remark = checkItem.Remark,
                                    Unit = checkItem.Unit,
                                    Seq = checkItem.Seq,
                                    FeelOptionList = db.CheckItemFeelOption.Where(x => x.CheckItemUniqueID == checkItem.CheckItemUniqueID).Select(x => new FeelOptionModel
                                    {
                                        UniqueID = x.UniqueID,
                                        Description = x.Description,
                                        IsAbnormal = x.IsAbnormal,
                                        Seq = x.Seq
                                    }).ToList(),
                                    AbnormalReasonList = (from ca in db.CheckItemAbnormalReason
                                                          join a in db.AbnormalReason
                                                          on ca.AbnormalReasonUniqueID equals a.UniqueID
                                                          where ca.CheckItemUniqueID == checkItem.CheckItemUniqueID
                                                          select new AbnormalReasonModel
                                                          {
                                                              UniqueID = a.UniqueID,
                                                              ID = a.ID,
                                                              Description = a.Description,
                                                              HandlingMethodList = (from ah in db.AbnormalReasonHandlingMethod
                                                                                    join h in db.HandlingMethod
                                                                                        on ah.HandlingMethodUniqueID equals h.UniqueID
                                                                                    where ah.AbnormalReasonUniqueID == a.UniqueID
                                                                                    select new HandlingMethodModel
                                                                                    {
                                                                                        UniqueID = h.UniqueID,
                                                                                        ID = h.ID,
                                                                                        Description = h.Description
                                                                                    }).ToList()
                                                          }).ToList()
                                };

                                if (truck.IsShowPrevRecord)
                                {
                                    var prevCheckResult = (from c in db.CheckResult
                                                           join a in db.ArriveRecord
                                                           on c.ArriveRecordUniqueID equals a.UniqueID
                                                           where a.ControlPointUniqueID == controlPoint.UniqueID && c.CheckItemUniqueID == checkItem.CheckItemUniqueID
                                                           select c).OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();

                                    if (prevCheckResult != null && !DataModel.PrevCheckResultList.Any(x => x.ControlPointUniqueID == controlPoint.UniqueID && x.CheckItemUniqueID == checkItem.CheckItemUniqueID))
                                    {
                                        DataModel.PrevCheckResultList.Add(new PrevCheckResultModel()
                                        {
                                            ControlPointUniqueID = controlPoint.UniqueID,
                                            CheckItemUniqueID = checkItem.CheckItemUniqueID,
                                            CheckDate = prevCheckResult.CheckDate,
                                            CheckTime = prevCheckResult.CheckTime,
                                            Result = prevCheckResult.Result,
                                            IsAbnormal = prevCheckResult.IsAbnormal,
                                            LowerLimit = prevCheckResult.LowerLimit,
                                            LowerAlertLimit = prevCheckResult.LowerAlertLimit,
                                            UpperAlertLimit = prevCheckResult.UpperAlertLimit,
                                            UpperLimit = prevCheckResult.UpperLimit,
                                            Unit = prevCheckResult.Unit,
                                            AbnormalReasonList = db.CheckResultAbnormalReason.Where(a => a.CheckResultUniqueID == prevCheckResult.UniqueID).OrderBy(a => a.AbnormalReasonID).Select(a => new PrevCheckResultAbnormalReasonModel
                                            {
                                                Description = a.AbnormalReasonDescription,
                                                Remark = a.AbnormalReasonRemark,
                                                HandlingMethodList = db.CheckResultHandlingMethod.Where(h => h.CheckResultUniqueID == prevCheckResult.UniqueID && h.AbnormalReasonUniqueID == a.AbnormalReasonUniqueID).OrderBy(h => h.HandlingMethodID).Select(h => new PrevCheckResultHandlingMethodModel
                                                {
                                                    Description = h.HandlingMethodDescription,
                                                    Remark = h.HandlingMethodRemark
                                                }).ToList()
                                            }).ToList()
                                        });
                                    }
                                }

                                controlPointModel.CheckItemList.Add(checkItemModel);
                            }
                            #endregion

                            if (controlPointModel.CheckItemList.Count > 0)
                            {
                                truckModel.ControlPointList.Add(controlPointModel);
                            }
                        }
                        #endregion

                        DataModel.TruckList.Add(truckModel);

                    #endregion
                    }
                }

                using (DbEntities db = new DbEntities())
                {
                    DataModel.UserList = (from u in db.User
                                          where downStreamOrganizationList.Contains(u.OrganizationUniqueID)
                                          select new UserModel
                                          {
                                              ID = u.ID,
                                              Name = u.Name,
                                              Password = u.Password,
                                              Title = u.Title,
                                              UID = u.UID
                                          }).ToList();
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
                        #region AbnormalReason, AbnormalReasonHandlingMethod
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO AbnormalReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", "OTHER");
                            cmd.Parameters.AddWithValue("ID", "OTHER");
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO AbnormalReasonHandlingMethod (AbnormalReasonUniqueID, HandlingMethodUniqueID) VALUES (@AbnormalReasonUniqueID, @HandlingMethodUniqueID)";

                            cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", "OTHER");
                            cmd.Parameters.AddWithValue("HandlingMethodUniqueID", "OTHER");

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var abnormalReason in DataModel.AbnormalReasonList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO AbnormalReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", abnormalReason.UniqueID);
                                cmd.Parameters.AddWithValue("ID", abnormalReason.ID);
                                cmd.Parameters.AddWithValue("Description", abnormalReason.Description);

                                cmd.ExecuteNonQuery();
                            }

                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO AbnormalReasonHandlingMethod (AbnormalReasonUniqueID, HandlingMethodUniqueID) VALUES (@AbnormalReasonUniqueID, @HandlingMethodUniqueID)";

                                cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", abnormalReason.UniqueID);
                                cmd.Parameters.AddWithValue("HandlingMethodUniqueID", "OTHER");

                                cmd.ExecuteNonQuery();
                            }

                            #region AbnormalReasonHandlingMethod
                            foreach (var handlingMethod in abnormalReason.HandlingMethodList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO AbnormalReasonHandlingMethod (AbnormalReasonUniqueID, HandlingMethodUniqueID) VALUES (@AbnormalReasonUniqueID, @HandlingMethodUniqueID)";

                                    cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", abnormalReason.UniqueID);
                                    cmd.Parameters.AddWithValue("HandlingMethodUniqueID", handlingMethod.UniqueID);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                            #endregion
                        }
                        #endregion

                        #region CheckItemAbnormalReason, CheckItemFeelOption
                        foreach (var checkItem in DataModel.CheckItemList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO CheckItemAbnormalReason (CheckItemUniqueID, AbnormalReasonUniqueID) VALUES (@CheckItemUniqueID, @AbnormalReasonUniqueID)";

                                cmd.Parameters.AddWithValue("CheckItemUniqueID", checkItem.UniqueID);
                                cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", "OTHER");

                                cmd.ExecuteNonQuery();
                            }

                            #region CheckItemAbnormalReason
                            foreach (var abnormalReason in checkItem.AbnormalReasonList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO CheckItemAbnormalReason (CheckItemUniqueID, AbnormalReasonUniqueID) VALUES (@CheckItemUniqueID, @AbnormalReasonUniqueID)";

                                    cmd.Parameters.AddWithValue("CheckItemUniqueID", checkItem.UniqueID);
                                    cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", abnormalReason.UniqueID);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                            #endregion

                            #region CheckItemFeelOption
                            foreach (var feelOption in checkItem.FeelOptionList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO CheckItemFeelOption (UniqueID, CheckItemUniqueID, Description, IsAbnormal, Seq) VALUES (@UniqueID, @CheckItemUniqueID, @Description, @IsAbnormal, @Seq)";

                                    cmd.Parameters.AddWithValue("UniqueID", feelOption.UniqueID);
                                    cmd.Parameters.AddWithValue("CheckItemUniqueID", checkItem.UniqueID);
                                    cmd.Parameters.AddWithValue("Description", feelOption.Description);
                                    cmd.Parameters.AddWithValue("IsAbnormal", feelOption.IsAbnormal ? "Y" : "N");
                                    cmd.Parameters.AddWithValue("Seq", feelOption.Seq);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                            #endregion
                        }
                        #endregion

                        #region HandlingMethod
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO HandlingMethod (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", "OTHER");
                            cmd.Parameters.AddWithValue("ID", "OTHER");
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var handlingMethod in DataModel.HandlingMethodList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO HandlingMethod (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", handlingMethod.UniqueID);
                                cmd.Parameters.AddWithValue("ID", handlingMethod.ID);
                                cmd.Parameters.AddWithValue("Description", handlingMethod.Description);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region Truck, ControlPoint, CheckItem
                        foreach (var truck in DataModel.TruckList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO Truck (UniqueID, Organization, TruckNo, BindingType, IsCheckBySeq, IsShowPrevRecord, Remark) VALUES (@UniqueID, @Organization, @TruckNo, @BindingType, @IsCheckBySeq, @IsShowPrevRecord, @Remark)";

                                cmd.Parameters.AddWithValue("UniqueID", truck.UniqueID);
                                cmd.Parameters.AddWithValue("Organization", truck.OrganizationDescription);
                                cmd.Parameters.AddWithValue("TruckNo", truck.TruckNo);
                                cmd.Parameters.AddWithValue("BindingType", truck.BindingType);
                                cmd.Parameters.AddWithValue("IsCheckBySeq", truck.IsCheckBySeq ? "Y" : "N");
                                cmd.Parameters.AddWithValue("IsShowPrevRecord", truck.IsShowPrevRecord ? "Y" : "N");
                                cmd.Parameters.AddWithValue("Remark", truck.Remark);

                                cmd.ExecuteNonQuery();
                            }

                            foreach (var controlPoint in truck.ControlPointList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO ControlPoint (TruckUniqueID, ControlPointUniqueID, ID, Description, IsFeelItemDefaultNormal, TagID, Remark, Seq) VALUES (@TruckUniqueID, @ControlPointUniqueID, @ID, @Description, @IsFeelItemDefaultNormal, @TagID, @Remark, @Seq)";

                                    cmd.Parameters.AddWithValue("TruckUniqueID", truck.UniqueID);
                                    cmd.Parameters.AddWithValue("ControlPointUniqueID", controlPoint.UniqueID);
                                    cmd.Parameters.AddWithValue("ID", controlPoint.ID);
                                    cmd.Parameters.AddWithValue("Description", controlPoint.Description);
                                    cmd.Parameters.AddWithValue("IsFeelItemDefaultNormal", controlPoint.IsFeelItemDefaultNormal ? "Y" : "N");
                                    cmd.Parameters.AddWithValue("TagID", controlPoint.TagID);
                                    cmd.Parameters.AddWithValue("Remark", controlPoint.Remark);
                                    cmd.Parameters.AddWithValue("Seq", controlPoint.Seq);

                                    cmd.ExecuteNonQuery();
                                }

                                foreach (var checkItem in controlPoint.CheckItemList)
                                {
                                    using (SQLiteCommand cmd = conn.CreateCommand())
                                    {
                                        cmd.CommandText = "INSERT INTO CheckItem (TruckUniqueID, ControlPointUniqueID, CheckItemUniqueID, ID, Description, IsFeelItem, LowerLimit, LowerAlertLimit, UpperAlertLimit, UpperLimit, Unit, Remark, Seq) VALUES (@TruckUniqueID, @ControlPointUniqueID, @CheckItemUniqueID, @ID, @Description, @IsFeelItem, @LowerLimit, @LowerAlertLimit, @UpperAlertLimit, @UpperLimit, @Unit, @Remark, @Seq)";

                                        cmd.Parameters.AddWithValue("TruckUniqueID", truck.UniqueID);
                                        cmd.Parameters.AddWithValue("ControlPointUniqueID", controlPoint.UniqueID);
                                        cmd.Parameters.AddWithValue("CheckItemUniqueID", checkItem.UniqueID);
                                        cmd.Parameters.AddWithValue("ID", checkItem.ID);
                                        cmd.Parameters.AddWithValue("Description", checkItem.Description);
                                        cmd.Parameters.AddWithValue("IsFeelItem", checkItem.IsFeelItem ? "Y" : "N");
                                        cmd.Parameters.AddWithValue("LowerLimit", checkItem.LowerLimit);
                                        cmd.Parameters.AddWithValue("LowerAlertLimit", checkItem.LowerAlertLimit);
                                        cmd.Parameters.AddWithValue("UpperAlertLimit", checkItem.UpperAlertLimit);
                                        cmd.Parameters.AddWithValue("UpperLimit", checkItem.UpperLimit);
                                        cmd.Parameters.AddWithValue("Unit", checkItem.Unit);
                                        cmd.Parameters.AddWithValue("Remark", checkItem.Remark);
                                        cmd.Parameters.AddWithValue("Seq", checkItem.Seq);

                                        cmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                        #endregion

                        #region LastModifyTime
                        foreach (var lasyModifyTime in DataModel.LastModifyTimeList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO LastModifyTime (TruckUniqueID, VersionTime) VALUES (@TruckUniqueID, @VersionTime)";

                                cmd.Parameters.AddWithValue("TruckUniqueID", lasyModifyTime.Key);
                                cmd.Parameters.AddWithValue("VersionTime", lasyModifyTime.Value);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion


                        #region UnRFIDReason
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO UnRFIDReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", "OTHER");
                            cmd.Parameters.AddWithValue("ID", "OTHER");
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var unRFIDReason in DataModel.UnRFIDReasonList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO UnRFIDReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", unRFIDReason.UniqueID);
                                cmd.Parameters.AddWithValue("ID", unRFIDReason.ID);
                                cmd.Parameters.AddWithValue("Description", unRFIDReason.Description);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region User
                        foreach (var user in DataModel.UserList)
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
                        #endregion

                        #region PrevCheckResult
                        foreach (var prevCheckResult in DataModel.PrevCheckResultList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO PrevCheckResult (ControlPointUniqueID, CheckItemUniqueID, LowerLimit, LowerAlertLimit, UpperAlertLimit, UpperLimit, Unit, CheckDate, CheckTime, Result, IsAbnormal, AbnormalReason) VALUES (@ControlPointUniqueID, @CheckItemUniqueID, @LowerLimit, @LowerAlertLimit, @UpperAlertLimit, @UpperLimit, @Unit, @CheckDate, @CheckTime, @Result, @IsAbnormal, @AbnormalReason)";

                                cmd.Parameters.AddWithValue("ControlPointUniqueID", prevCheckResult.ControlPointUniqueID);
                                cmd.Parameters.AddWithValue("CheckItemUniqueID", prevCheckResult.CheckItemUniqueID);
                                cmd.Parameters.AddWithValue("LowerLimit", prevCheckResult.LowerLimit);
                                cmd.Parameters.AddWithValue("LowerAlertLimit", prevCheckResult.LowerAlertLimit);
                                cmd.Parameters.AddWithValue("UpperAlertLimit", prevCheckResult.UpperAlertLimit);
                                cmd.Parameters.AddWithValue("UpperLimit", prevCheckResult.UpperLimit);
                                cmd.Parameters.AddWithValue("Unit", prevCheckResult.Unit);
                                cmd.Parameters.AddWithValue("CheckDate", prevCheckResult.CheckDate);
                                cmd.Parameters.AddWithValue("CheckTime", prevCheckResult.CheckTime);
                                cmd.Parameters.AddWithValue("Result", prevCheckResult.Result);
                                cmd.Parameters.AddWithValue("IsAbnormal", prevCheckResult.IsAbnormal);
                                cmd.Parameters.AddWithValue("AbnormalReason", prevCheckResult.AbnormalReasons);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

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
                return Path.Combine(Config.TruckPatrolSQLiteTemplateFolderPath, Define.SQLite_TruckPatrol);
            }
        }

        private string GeneratedFolderPath
        {
            get
            {
                return Path.Combine(Config.TruckPatrolSQLiteGeneratedFolderPath, Guid);
            }
        }

        private string GeneratedDbFilePath
        {
            get
            {
                return Path.Combine(GeneratedFolderPath, Define.SQLite_TruckPatrol);
            }
        }

        private string GeneratedZipFilePath
        {
            get
            {
                return Path.Combine(GeneratedFolderPath, Define.SQLiteZip_TruckPatrol);
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
