using DbEntity.ASE;
using ICSharpCode.SharpZipLib.Zip;
using Models.ASE.QS.DataSync;
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

namespace DataAccess.ASE.QS
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
                    DataModel.UserList = (from x in db.USERAUTHGROUP
                                          join u in db.ACCOUNT
                                          on x.USERID equals u.ID
                                          where x.AUTHGROUPID == "QS"
                                          select u).Distinct().Select(x => new UserModel
                                          {    
                                              ID = x.ID,
                                              Name = x.NAME,
                                              Title = x.TITLE,
                                              Password = x.PASSWORD,
                                              UID = x.TAGID
                                          }).ToList();

                    DataModel.FactoryList = db.QS_FACTORY.Select(x => new FactoryModel
                    {
                        UniqueID = x.UNIQUEID,
                        Description = x.DESCRIPTION,
                        CheckItemList = db.QS_FACTORY_CHECKITEM.Where(y => y.FACTORYUNIQUEID == x.UNIQUEID).Select(y => y.CHECKITEMUNIQUEID).ToList(),
                        StationList = db.QS_FACTORY_STATION.Where(y => y.FACTORYUNIQUEID == x.UNIQUEID).Select(y => y.STATIONUNIQUEID).ToList()
                    }).ToList();

                    DataModel.ShiftList = db.QS_SHIFT.Select(x => new ShiftModel
                    {
                        UniqueID = x.UNIQUEID,
                        Description = x.DESCRIPTION
                    }).ToList();

                    DataModel.StationList = db.QS_STATION.Select(x => new StationModel
                    {
                        UniqueID = x.UNIQUEID,
                        Type  = x.TYPE,
                        Description = x.DESCRIPTION
                    }).ToList();

                    DataModel.ResDepartmentList = db.QS_RESDEPARTMENT.Select(x => new ResDepartmentModel
                    {
                        UniqueID = x.UNIQUEID,
                        Description = x.DESCRIPTION
                    }).ToList();

                    var checkTypeList = db.QS_CHECKITEM.Select(x => new { x.TYPEID, x.TYPEEDESCRIPTION, x.TYPECDESCRIPTION }).Distinct().ToList();

                    foreach (var checkType in checkTypeList)
                    {
                        DataModel.CheckTypeList.Add(new CheckTypeModel()
                        {
                            ID = checkType.TYPEID,
                            EDescription = checkType.TYPEEDESCRIPTION,
                            CDescription = checkType.TYPECDESCRIPTION,
                            CheckItemList = db.QS_CHECKITEM.Where(x => x.TYPEID == checkType.TYPEID).ToList().Select(checkItem => new CheckItemModel
                            {
                                UniqueID = checkItem.UNIQUEID,
                                ID = checkItem.ID,
                                EDescription = checkItem.EDESCRIPTION,
                                CDescription = checkItem.CDESCRIPTION,
                                CheckTimes = checkItem.CHECKTIMES,
                                Unit = checkItem.UNIT,
                                RemarkList = (from x in db.QS_CHECKITEMREMARK
                                              join r in db.QS_REMARK
                                              on x.REMARKUNIQUEID equals r.UNIQUEID
                                              where x.CHECKITEMUNIQUEID == checkItem.UNIQUEID
                                              select new RemarkModel
                                              {
                                                  UniqueID = r.UNIQUEID,
                                                  Description = r.DESCRIPTION
                                              }).ToList()
                            }).ToList()
                        });
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

                        foreach (var factory in DataModel.FactoryList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO Factory (UniqueID, Description) VALUES (@UniqueID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", factory.UniqueID);
                                cmd.Parameters.AddWithValue("Description", factory.Description);

                                cmd.ExecuteNonQuery();
                            }

                            foreach (var checkItem in factory.CheckItemList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO FactoryCheckItem (FactoryUniqueID, CheckItemUniqueID) VALUES (@FactoryUniqueID, @CheckItemUniqueID)";

                                    cmd.Parameters.AddWithValue("FactoryUniqueID", factory.UniqueID);
                                    cmd.Parameters.AddWithValue("CheckItemUniqueID", checkItem);

                                    cmd.ExecuteNonQuery();
                                }
                            }

                            foreach (var station in factory.StationList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO FactoryStation (FactoryUniqueID, StationUniqueID) VALUES (@FactoryUniqueID, @StationUniqueID)";

                                    cmd.Parameters.AddWithValue("FactoryUniqueID", factory.UniqueID);
                                    cmd.Parameters.AddWithValue("StationUniqueID", station);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        foreach (var shift in DataModel.ShiftList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO Shift (UniqueID, Description) VALUES (@UniqueID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", shift.UniqueID);
                                cmd.Parameters.AddWithValue("Description", shift.Description);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        foreach (var station in DataModel.StationList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO Station (UniqueID, Description, Type) VALUES (@UniqueID, @Description, @Type)";

                                cmd.Parameters.AddWithValue("UniqueID", station.UniqueID);
                                cmd.Parameters.AddWithValue("Description", station.Description);
                                cmd.Parameters.AddWithValue("Type", station.Type);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        foreach (var resDepartment in DataModel.ResDepartmentList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO ResDepartment (UniqueID, Description) VALUES (@UniqueID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", resDepartment.UniqueID);
                                cmd.Parameters.AddWithValue("Description", resDepartment.Description);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        foreach (var checkType in DataModel.CheckTypeList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO CheckType (ID, EDescription, CDescription) VALUES (@ID, @EDescription, @CDescription)";

                                cmd.Parameters.AddWithValue("ID", checkType.ID);
                                cmd.Parameters.AddWithValue("EDescription", checkType.EDescription);
                                cmd.Parameters.AddWithValue("CDescription", checkType.CDescription);

                                cmd.ExecuteNonQuery();
                            }

                            foreach (var checkItem in checkType.CheckItemList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO CheckItem (UniqueID, CheckTypeID, ID, EDescription, CDescription, CheckTimes, Unit) VALUES (@UniqueID, @CheckTypeID, @ID, @EDescription, @CDescription, @CheckTimes, @Unit)";

                                    cmd.Parameters.AddWithValue("UniqueID", checkItem.UniqueID);
                                    cmd.Parameters.AddWithValue("CheckTypeID", checkType.ID);
                                    cmd.Parameters.AddWithValue("ID", checkItem.ID);
                                    cmd.Parameters.AddWithValue("EDescription", checkItem.EDescription);
                                    cmd.Parameters.AddWithValue("CDescription", checkItem.CDescription);
                                    cmd.Parameters.AddWithValue("CheckTimes", checkItem.CheckTimes);
                                    cmd.Parameters.AddWithValue("Unit", checkItem.Unit);

                                    cmd.ExecuteNonQuery();
                                }

                                foreach (var remark in checkItem.RemarkList)
                                {
                                    using (SQLiteCommand cmd = conn.CreateCommand())
                                    {
                                        cmd.CommandText = "INSERT INTO CheckItemRemark (CheckItemUniqueID, RemarkUniqueID) VALUES (@CheckItemUniqueID, @RemarkUniqueID)";

                                        cmd.Parameters.AddWithValue("CheckItemUniqueID", checkItem.UniqueID);
                                        cmd.Parameters.AddWithValue("RemarkUniqueID", remark.UniqueID);

                                        cmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }

                        foreach (var remark in DataModel.RemarkList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO Remark (UniqueID, Description) VALUES (@UniqueID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", remark.UniqueID);
                                cmd.Parameters.AddWithValue("Description", remark.Description);

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
                return Path.Combine(Config.QSSQLiteTemplateFolderPath, Define.SQLite_QS);
            }
        }

        private string GeneratedFolderPath
        {
            get
            {
                return Path.Combine(Config.QSSQLiteGeneratedFolderPath, Guid);
            }
        }

        private string GeneratedDbFilePath
        {
            get
            {
                return Path.Combine(GeneratedFolderPath, Define.SQLite_QS);
            }
        }

        private string GeneratedZipFilePath
        {
            get
            {
                return Path.Combine(GeneratedFolderPath, Define.SQLiteZip_QS);
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
