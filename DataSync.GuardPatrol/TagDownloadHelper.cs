using DataAccess;
using DbEntity.MSSQL;
using DbEntity.MSSQL.GuardPatrol;
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
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;

namespace DataSync.GuardPatrol
{
    public class TagDownloadHelper : IDisposable
    {
        private string Guid;

        public RequestResult Generate(string UserID, string Password)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (DbEntities db = new DbEntities())
                {
                    var user = db.User.FirstOrDefault(x => x.ID == UserID);

                    if (user != null)
                    {
                        if (string.Compare(Password, user.Password, false) == 0)
                        {
                            result = Init();

                            if (result.IsSuccess)
                            {
                                result = GenerateSQLite(db, user.OrganizationUniqueID);

                                if (result.IsSuccess)
                                {
                                    result = GenerateZip();
                                }
                            }
                        }
                        else
                        {
                            result.ReturnFailedMessage(Resources.Resource.WrongPassword);
                        }
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

        private RequestResult GenerateSQLite(DbEntities DB, string OrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var organizationList = new List<Organization>();
                var controlPointList = new List<ControlPoint>();

                var organizationPermissionList = OrganizationDataAccessor.GetUserOrganizationPermissionList(OrganizationUniqueID);

                using (GDbEntities edb = new GDbEntities())
                {
                    foreach (var organizationPermission in organizationPermissionList)
                    {
                        var organization = DB.Organization.FirstOrDefault(x => x.UniqueID == organizationPermission.UniqueID);

                        if (organization != null)
                        {
                            organizationList.Add(organization);

                            if (organizationPermission.Permission == Define.EnumOrganizationPermission.Editable)
                            {
                                controlPointList.AddRange(edb.ControlPoint.Where(x => x.OrganizationUniqueID == organization.UniqueID).ToList());
                            }
                        }
                    }
                }

                using (SQLiteConnection conn = new SQLiteConnection(this.SQLiteConnString))
                {
                    conn.Open();

                    using (SQLiteTransaction trans = conn.BeginTransaction())
                    {
                        foreach (var organization in organizationList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO Organization (UniqueID, ParentUniqueID, ID, Description) VALUES (@UniqueID, @ParentUniqueID, @ID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", organization.UniqueID);
                                cmd.Parameters.AddWithValue("ParentUniqueID", organization.ParentUniqueID);
                                cmd.Parameters.AddWithValue("ID", organization.ID);
                                cmd.Parameters.AddWithValue("Description", organization.Description);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        foreach (var controlPoint in controlPointList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO ControlPoint (UniqueID, OrganizationUniqueID, ID, Description, TagID) VALUES (@UniqueID, @OrganizationUniqueID, @ID, @Description, @TagID)";

                                cmd.Parameters.AddWithValue("UniqueID", controlPoint.UniqueID);
                                cmd.Parameters.AddWithValue("OrganizationUniqueID", controlPoint.OrganizationUniqueID);
                                cmd.Parameters.AddWithValue("ID", controlPoint.ID);
                                cmd.Parameters.AddWithValue("Description", controlPoint.Description);
                                cmd.Parameters.AddWithValue("TagID", controlPoint.TagID);

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
                return Path.Combine(Config.GuardPatrolTagSQLiteTemplateFolderPath, Define.SQLite_GuardPatrolTag);
            }
        }

        private string GeneratedFolderPath
        {
            get
            {
                return Path.Combine(Config.GuardPatrolTagSQLiteGeneratedFolderPath, Guid);
            }
        }

        private string GeneratedDbFilePath
        {
            get
            {
                return Path.Combine(GeneratedFolderPath, Define.SQLite_GuardPatrolTag);
            }
        }

        private string GeneratedZipFilePath
        {
            get
            {
                return Path.Combine(GeneratedFolderPath, Define.SQLiteZip_GuardPatrolTag);
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

        ~TagDownloadHelper()
        {
            Dispose(false);
        }

        #endregion
    }
}
