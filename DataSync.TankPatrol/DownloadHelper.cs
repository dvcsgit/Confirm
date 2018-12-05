using DataAccess;
using DbEntity.MSSQL;
using DbEntity.MSSQL.TankPatrol;
using ICSharpCode.SharpZipLib.Zip;
using Models.TankPatrol.DataSync;
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

namespace DataSync.TankPatrol
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
                var organizationUniqueID = string.Empty;

                var organizationList = new List<string>();

                using (DbEntities db = new DbEntities())
                {
                    organizationUniqueID = db.User.First(x => x.ID == UserID).OrganizationUniqueID;

                    organizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organizationUniqueID, true);

                    DataModel.UserList = db.User.Where(x => organizationList.Contains(x.OrganizationUniqueID)).Select(x => new UserModel
                    {
                        ID = x.ID,
                        Name = x.Name,
                        Password = x.Password,
                        Title = x.Title,
                        UID = x.UID
                    }).ToList();
                }

                using (TankDbEntities db = new TankDbEntities())
                {
                    DataModel.UnRFIDReasonList = db.UnRFIDReason.Select(x => new Models.TankPatrol.DataSync.UnRFIDReason
                    {
                        UniqueID = x.UniqueID,
                        ID = x.ID,
                        Description = x.Description,
                        LastModifyTime = x.LastModifyTime
                    }).ToList();

                    DataModel.OptionList = db.Option.Where(x => organizationList.Contains(x.OrganizationUniqueID)).Select(x => new OptionModel
                    {
                        UniqueID = x.UniqueID,
                        Type = x.Type,
                        Description = x.Description
                    }).ToList();

                    var stationList = db.Station.Where(x => organizationList.Contains(x.OrganizationUniqueID)).ToList();

                    foreach (var station in stationList)
                    {
                        var stationModel = new StationModel()
                        {
                            UniqueID = station.UniqueID,
                            ID = station.ID,
                            Description = station.Description
                        };

                        var islandList = db.Island.Where(x => x.StationUniqueID == station.UniqueID).ToList();

                        foreach (var island in islandList)
                        {
                            var islandModel = new IslandModel()
                            {
                                UniqueID = island.UniqueID,
                                ID = island.ID,
                                Description = island.Description
                            };

                            var portList = db.Port.Where(x => x.IslandUniqueID == island.UniqueID).ToList();

                            foreach (var port in portList)
                            {
                                var portModel = new PortModel()
                                {
                                    UniqueID = port.UniqueID,
                                    ID = port.ID,
                                    Description = port.Description,
                                    TagID = port.TagID,
                                    LB2LPTimeSpan = port.LB2LPTimeSpan,
                                    LP2LATimeSpan = port.LP2LATimeSpan,
                                    LA2LDTimeSpan = port.LA2LDTimeSpan,
                                    UB2UPTimeSpan = port.UB2UPTimeSpan,
                                    UP2UATimeSpan = port.UP2UATimeSpan,
                                    UA2UDTimeSpan = port.UA2UDTimeSpan
                                };

                                var checkItemList = (from x in db.PortCheckItem
                                                     join c in db.CheckItem
                                                     on x.CheckItemUniqueID equals c.UniqueID
                                                     where x.PortUniqueID == port.UniqueID
                                                     select new
                                                     {
                                                         UniqueID = c.UniqueID,
                                                         x.CheckType,
                                                         x.Procedure,
                                                         c.ID,
                                                         c.Description,
                                                         x.TagID,
                                                         c.IsFeelItem,
                                                         c.TextValueType,
                                                         x.IsInherit,
                                                         x.LowerLimit,
                                                         x.LowerAlertLimit,
                                                         x.UpperAlertLimit,
                                                         x.UpperLimit,
                                                         x.Unit,
                                                         OriLowerLimit = c.LowerLimit,
                                                         OriLowerAlertLimit = c.LowerAlertLimit,
                                                         OriUpperAlertLimit = c.UpperAlertLimit,
                                                         OriUpperLimit = c.UpperLimit,
                                                         OriUnit = c.Unit
                                                     }).ToList();

                                foreach(var checkItem in checkItemList)
                                {
                                    portModel.CheckItemList.Add(new CheckItemModel()
                                    {
                                        UniqueID = checkItem.UniqueID,
                                        CheckType = checkItem.CheckType,
                                        Procedure = checkItem.Procedure,
                                        ID = checkItem.ID,
                                        Description = checkItem.Description,
                                        IsFeelItem = checkItem.IsFeelItem,
                                        TextValueType = checkItem.TextValueType,
                                        LowerLimit = checkItem.IsInherit ? checkItem.OriLowerLimit : checkItem.LowerLimit,
                                        LowerAlertLimit = checkItem.IsInherit ? checkItem.OriLowerAlertLimit : checkItem.LowerAlertLimit,
                                        UpperAlertLimit = checkItem.IsInherit ? checkItem.OriUpperAlertLimit : checkItem.UpperAlertLimit,
                                        UpperLimit = checkItem.IsInherit ? checkItem.OriUpperLimit : checkItem.UpperLimit,
                                        Unit = checkItem.IsInherit ? checkItem.OriUnit : checkItem.Unit,
                                        TagID = checkItem.TagID,
                                        FeelOptionList = db.CheckItemFeelOption.Where(x => x.CheckItemUniqueID == checkItem.UniqueID).Select(x => new FeelOptionModel
                                        {
                                            UniqueID = x.UniqueID,
                                            Description = x.Description,
                                            IsAbnormal = x.IsAbnormal,
                                            Seq = x.Seq
                                        }).ToList(),
                                        AbnormalReasonList = (from ca in db.CheckItemAbnormalReason
                                                              join a in db.AbnormalReason
                                                              on ca.AbnormalReasonUniqueID equals a.UniqueID
                                                              where ca.CheckItemUniqueID == checkItem.UniqueID
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
                                    });
                                }

                                islandModel.PortList.Add(portModel);
                            }

                            stationModel.IslandList.Add(islandModel);
                        }

                        DataModel.StationList.Add(stationModel);
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

                        #region Station, Island, Port, CheckItem
                        foreach (var station in DataModel.StationList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO Station (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", station.UniqueID);
                                cmd.Parameters.AddWithValue("ID", station.ID);
                                cmd.Parameters.AddWithValue("Description", station.Description);

                                cmd.ExecuteNonQuery();
                            }

                            foreach (var island in station.IslandList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO Island (UniqueID, StationUniqueID, ID, Description) VALUES (@UniqueID, @StationUniqueID, @ID, @Description)";

                                    cmd.Parameters.AddWithValue("UniqueID", island.UniqueID);
                                    cmd.Parameters.AddWithValue("StationUniqueID", station.UniqueID);
                                    cmd.Parameters.AddWithValue("ID", island.ID);
                                    cmd.Parameters.AddWithValue("Description", island.Description);

                                    cmd.ExecuteNonQuery();
                                }

                                foreach (var port in island.PortList)
                                {
                                    using (SQLiteCommand cmd = conn.CreateCommand())
                                    {
                                        cmd.CommandText = "INSERT INTO Port (UniqueID, IslandUniqueID, ID, Description, TagID, LB2LPTimeSpan, LP2LATimeSpan, LA2LDTimeSpan, UB2UPTimeSpan, UP2UATimeSpan, UA2UDTimeSpan) VALUES (@UniqueID, @IslandUniqueID, @ID, @Description, @TagID, @LB2LPTimeSpan, @LP2LATimeSpan, @LA2LDTimeSpan, @UB2UPTimeSpan, @UP2UATimeSpan, @UA2UDTimeSpan)";

                                        cmd.Parameters.AddWithValue("UniqueID", port.UniqueID);
                                        cmd.Parameters.AddWithValue("IslandUniqueID", island.UniqueID);
                                        cmd.Parameters.AddWithValue("ID", port.ID);
                                        cmd.Parameters.AddWithValue("Description", port.Description);
                                        cmd.Parameters.AddWithValue("TagID", port.TagID);
                                        cmd.Parameters.AddWithValue("LB2LPTimeSpan", port.LB2LPTimeSpan);
                                        cmd.Parameters.AddWithValue("LP2LATimeSpan", port.LP2LATimeSpan);
                                        cmd.Parameters.AddWithValue("LA2LDTimeSpan", port.LA2LDTimeSpan);
                                        cmd.Parameters.AddWithValue("UB2UPTimeSpan", port.UB2UPTimeSpan);
                                        cmd.Parameters.AddWithValue("UP2UATimeSpan", port.UP2UATimeSpan);
                                        cmd.Parameters.AddWithValue("UA2UDTimeSpan", port.UA2UDTimeSpan);

                                        cmd.ExecuteNonQuery();
                                    }

                                    int seq = 1;

                                    foreach (var checkItem in port.CheckItemList.OrderBy(x => x.ID))
                                    {
                                        using (SQLiteCommand cmd = conn.CreateCommand())
                                        {
                                            cmd.CommandText = "INSERT INTO CheckItem (PortUniqueID, CheckItemUniqueID, CheckType, Procedure, ID, Description, IsFeelItem, TextValueType, LowerLimit, LowerAlertLimit, UpperAlertLimit, UpperLimit, Unit, Seq) VALUES (@PortUniqueID, @CheckItemUniqueID, @CheckType, @Procedure, @ID, @Description, @IsFeelItem, @TextValueType, @LowerLimit, @LowerAlertLimit, @UpperAlertLimit, @UpperLimit, @Unit, @Seq)";

                                            cmd.Parameters.AddWithValue("PortUniqueID", port.UniqueID);
                                            cmd.Parameters.AddWithValue("CheckItemUniqueID", checkItem.UniqueID);
                                            cmd.Parameters.AddWithValue("CheckType", checkItem.CheckType);
                                            cmd.Parameters.AddWithValue("Procedure", checkItem.Procedure);
                                            cmd.Parameters.AddWithValue("ID", checkItem.ID);
                                            cmd.Parameters.AddWithValue("Description", checkItem.Description);
                                            cmd.Parameters.AddWithValue("IsFeelItem", checkItem.IsFeelItem ? "Y" : "N");
                                            cmd.Parameters.AddWithValue("TextValueType", checkItem.TextValueType);
                                            cmd.Parameters.AddWithValue("LowerLimit", checkItem.LowerLimit);
                                            cmd.Parameters.AddWithValue("LowerAlertLimit", checkItem.LowerAlertLimit);
                                            cmd.Parameters.AddWithValue("UpperAlertLimit", checkItem.UpperAlertLimit);
                                            cmd.Parameters.AddWithValue("UpperLimit", checkItem.UpperLimit);
                                            cmd.Parameters.AddWithValue("Unit", checkItem.Unit);
                                            cmd.Parameters.AddWithValue("Seq", seq);

                                            cmd.ExecuteNonQuery();
                                        }

                                        seq++;
                                    }
                                }
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

                        foreach (var driver in DataModel.DriverList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO Driver (UniqueID, Name) VALUES (@UniqueID, @Name)";

                                cmd.Parameters.AddWithValue("UniqueID", driver.UniqueID);
                                cmd.Parameters.AddWithValue("Name", driver.Description);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        foreach (var owner in DataModel.OwnerList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO Owner (UniqueID, Description) VALUES (@UniqueID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", owner.UniqueID);
                                cmd.Parameters.AddWithValue("Description", owner.Description);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        foreach (var material in DataModel.MaterialList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO Material (UniqueID, Description) VALUES (@UniqueID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", material.UniqueID);
                                cmd.Parameters.AddWithValue("Description", material.Description);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        foreach (var tank in DataModel.TankList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO Tank (UniqueID, Description) VALUES (@UniqueID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", tank.UniqueID);
                                cmd.Parameters.AddWithValue("Description", tank.Description);

                                cmd.ExecuteNonQuery();
                            }
                        }

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
                return Path.Combine(Config.TankPatrolSQLiteTemplateFolderPath, Define.SQLite_TankPatrol);
            }
        }

        private string GeneratedFolderPath
        {
            get
            {
                return Path.Combine(Config.TankPatrolSQLiteGeneratedFolderPath, Guid);
            }
        }

        private string GeneratedDbFilePath
        {
            get
            {
                return Path.Combine(GeneratedFolderPath, Define.SQLite_TankPatrol);
            }
        }

        private string GeneratedZipFilePath
        {
            get
            {
                return Path.Combine(GeneratedFolderPath, Define.SQLiteZip_TankPatrol);
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
