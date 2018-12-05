using DbEntity.ASE;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace SQLite2DB.ASE.QS
{
    public class TransHelper : IDisposable
    {
        public void Trans()
        {
            try
            {
                var folders = Directory.GetDirectories(Config.QSSQLiteUploadFolderPath);

                Logger.Log(folders.Length + " Folder in " + Config.QSSQLiteUploadFolderPath);

                //從Upload資料夾搬移到Processing資料夾
                foreach (var folder in folders)
                {
                    System.IO.File.Move(Path.Combine(folder, "QS.Upload.zip"), Path.Combine(Config.QSSQLiteProcessingFolderPath, folder.Substring(folder.LastIndexOf("\\") + 1) + ".zip"));

                    Directory.Delete(folder);
                }

                var zips = Directory.GetFiles(Config.QSSQLiteProcessingFolderPath);

                Logger.Log(zips.Length + " Files To Trans");

                foreach (var zip in zips)
                {
                    FileInfo fileInfo = new FileInfo(zip);

                    var uploadLogUniqueID = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf(fileInfo.Extension));

                    RequestResult result = Trans(uploadLogUniqueID, fileInfo);

                    //轉檔成功->搬移到Backup資料夾 & Update Upload Log
                    if (result.IsSuccess)
                    {
                        using (ASEDbEntities db = new ASEDbEntities())
                        {
                            var uploadLog = db.QA_UPLOADLOG.FirstOrDefault(x => x.UNIQUEID == uploadLogUniqueID);

                            if (uploadLog != null)
                            {
                                uploadLog.TRANSTIME = DateTime.Now;

                                db.SaveChanges();
                            }
                        }

                        System.IO.File.Copy(zip, Path.Combine(Config.QSSQLiteBackupFolderPath, fileInfo.Name), true);

                        System.IO.File.Delete(zip);

                        Directory.Delete(Path.Combine(Config.QSSQLiteProcessingFolderPath, uploadLogUniqueID), true);

                        Logger.Log(zip + " Trans Success");
                    }
                    //轉檔失敗->搬移到Error資料夾
                    else
                    {
                        System.IO.File.Copy(zip, Path.Combine(Config.QSSQLiteErrorFolderPath, fileInfo.Name), true);

                        System.IO.File.Delete(zip);

                        Directory.Delete(Path.Combine(Config.QSSQLiteProcessingFolderPath, uploadLogUniqueID), true);

                        Logger.Log(zip + " Trans Failed");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(new Error(MethodBase.GetCurrentMethod(), ex));
            }
        }

        private RequestResult Trans(string UploadLogUniqueID, FileInfo ZipFileInfo)
        {
            RequestResult result = new RequestResult();

            try
            {
                var extractPath = Path.Combine(Config.QSSQLiteProcessingFolderPath, UploadLogUniqueID);

                using (var zip = ZipFile.Read(ZipFileInfo.FullName))
                {
                    foreach (var entry in zip)
                    {
                        entry.Extract(extractPath);
                    }
                }

                var connString = string.Format("Data Source={0};Version=3;", Path.Combine(extractPath, Define.SQLite_QS));

                using (SQLiteConnection conn = new SQLiteConnection(connString))
                {
                    conn.Open();

                    var uploadList = new List<string>();

                    #region UploadDefine
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM UploadDefine";

                        using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                adapter.Fill(dt);

                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    uploadList = dt.AsEnumerable().Select(x => x["FormUniqueID"].ToString()).ToList();
                                }
                            }
                        }
                    }
                    #endregion

                    foreach (var formUniqueID in uploadList)
                    {
                        var factoryUniqueID = string.Empty;

                        using (ASEDbEntities db = new ASEDbEntities())
                        {
                            #region Form
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = string.Format("SELECT * FROM Form WHERE UniqueID = '{0}'", formUniqueID);

                                using (SQLiteDataAdapter adapterForm = new SQLiteDataAdapter(cmd))
                                {
                                    using (DataTable dt = new DataTable())
                                    {
                                        adapterForm.Fill(dt);

                                        if (dt != null && dt.Rows.Count > 0)
                                        {
                                            for (int i = 0; i < dt.Rows.Count; i++)
                                            {
                                                var auditDate = dt.Rows[i]["AuditDate"].ToString();
                                                var auditorID = dt.Rows[i]["auditorID"].ToString();
                                                
                                                var vhnoPrex = string.Format("Q{0}", auditDate.Substring(2, 6));

                                                var query = db.QS_FORM.Where(x => x.VHNO.StartsWith(vhnoPrex)).OrderByDescending(x => x.VHNO).ToList();

                                                int vhnoSeq = 1;

                                                if (query.Count > 0)
                                                {
                                                    vhnoSeq = int.Parse(query.First().VHNO.Substring(7)) + 1;
                                                }

                                                var vhno = string.Format("{0}{1}", vhnoPrex, vhnoSeq.ToString().PadLeft(2, '0'));

                                                var auditor = db.ACCOUNT.FirstOrDefault(x => x.ID == auditorID);

                                                factoryUniqueID = dt.Rows[i]["FactoryUniqueID"].ToString();

                                                db.QS_FORM.Add(new QS_FORM()
                                                {
                                                    UNIQUEID = formUniqueID,
                                                    VHNO = vhno,
                                                    AUDITDATE = DateTimeHelper.DateString2DateTime(auditDate).Value,
                                                    AUDITORID = auditorID,
                                                    AUDITORMANAGERID = auditor != null ? auditor.MANAGERID : "",
                                                    FACTORYUNIQUEID = dt.Rows[i]["FactoryUniqueID"].ToString(),
                                                    FACTORYREMARK = dt.Rows[i]["FactoryRemark"].ToString(),
                                                    SHIFTUNIQUEID = dt.Rows[i]["ShiftUniqueID"].ToString(),
                                                    SHIFTREMARK = dt.Rows[i]["ShiftRemark"].ToString()
                                                });

                                                db.QS_FORM_LOG.Add(new QS_FORM_LOG()
                                                {
                                                    FORMUNIQUEID = formUniqueID,
                                                    SEQ = 1,
                                                    ACTION = Define.EnumFormAction.Create.ToString(),
                                                    ACTIONTIME = DateTime.Now,
                                                    USERID = auditorID
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region FormStation
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = string.Format("SELECT * FROM FormStation WHERE FormUniqueID = '{0}'", formUniqueID);

                                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                                {
                                    using (DataTable dt = new DataTable())
                                    {
                                        adapter.Fill(dt);

                                        if (dt != null && dt.Rows.Count > 0)
                                        {
                                            for (int i = 0; i < dt.Rows.Count; i++)
                                            {
                                                var stationUniqueID = dt.Rows[i]["StationUniqueID"].ToString();
                                                var stationRemark = dt.Rows[i]["StationRemark"].ToString();

                                                if (stationUniqueID == Define.OTHER)
                                                {
                                                    db.QS_FORM_STATION.Add(new QS_FORM_STATION()
                                                    {
                                                        FORMUNIQUEID = formUniqueID,
                                                        STATIONUNIQUEID = stationUniqueID,
                                                        STATIONREMARK = stationRemark
                                                    });
                                                }
                                                else
                                                {
                                                    db.QS_FORM_STATION.Add(new QS_FORM_STATION()
                                                    {
                                                        FORMUNIQUEID = formUniqueID,
                                                        STATIONUNIQUEID = stationUniqueID
                                                    });
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region FormCheckResult
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = string.Format("SELECT * FROM FormCheckResult WHERE FormUniqueID = '{0}'", formUniqueID);

                                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                                {
                                    using (DataTable dt = new DataTable())
                                    {
                                        adapter.Fill(dt);

                                        if (dt != null && dt.Rows.Count > 0)
                                        {
                                            for (int i = 0; i < dt.Rows.Count; i++)
                                            {
                                                var checkItemUniqueID = dt.Rows[i]["CheckItemUniqueID"].ToString();
                                                var seq = int.Parse(dt.Rows[i]["Seq"].ToString());
                                                var stationUniqueID = dt.Rows[i]["StationUniqueID"].ToString();
                                                var auditObject = dt.Rows[i]["AuditObject"].ToString();
                                                var resDepartments = string.Empty;
                                                var r = dt.Rows[i]["Result"].ToString();
                                                var remark = dt.Rows[i]["Remark"].ToString();

                                                using (SQLiteCommand cmdResDept = conn.CreateCommand())
                                                {
                                                    cmdResDept.CommandText = string.Format("SELECT * FROM FormResDepartment WHERE FormUniqueID = '{0}' AND CheckItemUniqueID = '{1}' AND Seq = '{2}'", formUniqueID, checkItemUniqueID, seq);

                                                    using (SQLiteDataAdapter adapterResDept = new SQLiteDataAdapter(cmdResDept))
                                                    {
                                                        using (DataTable dtResDept = new DataTable())
                                                        {
                                                            adapterResDept.Fill(dtResDept);

                                                            if (dtResDept != null && dtResDept.Rows.Count > 0)
                                                            {
                                                                for (int res = 0; res < dtResDept.Rows.Count; res++)
                                                                {
                                                                    if (res == dtResDept.Rows.Count - 1)
                                                                    {
                                                                        resDepartments = resDepartments + dtResDept.Rows[res]["ResDepartmentUniqueID"].ToString();
                                                                    }
                                                                    else
                                                                    {
                                                                        resDepartments = resDepartments + dtResDept.Rows[res]["ResDepartmentUniqueID"].ToString() + Define.Seperator;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                var checkResult = new QS_FORM_CHECKRESULT()
                                                {
                                                    FORMUNIQUEID = formUniqueID,
                                                    CHECKITEMUNIQUEID = checkItemUniqueID,
                                                    SEQ = seq,
                                                    STATIONUNIQUEID = stationUniqueID,
                                                    AUDITOBJECT = auditObject,
                                                    RESDEPARTMENTS = resDepartments,
                                                    RESULT = r,
                                                    REMARK = remark
                                                };

                                                db.QS_FORM_CHECKRESULT.Add(checkResult);
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region FormCheckResultPhoto
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = string.Format("SELECT * FROM FormCheckResultPhoto WHERE FormUniqueID = '{0}'", formUniqueID);

                                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                                {
                                    using (DataTable dt = new DataTable())
                                    {
                                        adapter.Fill(dt);

                                        if (dt != null && dt.Rows.Count > 0)
                                        {
                                            for (int i = 0; i < dt.Rows.Count; i++)
                                            {
                                                var checkItemUniqueID = dt.Rows[i]["CheckItemUniqueID"].ToString();
                                                var checkItemSeq = int.Parse(dt.Rows[i]["CheckItemSeq"].ToString());
                                                var seq = int.Parse(dt.Rows[i]["Seq"].ToString());

                                                var photo = Path.Combine(extractPath, dt.Rows[i]["FileName"].ToString());

                                                if (System.IO.File.Exists(photo))
                                                {
                                                    var extension = new FileInfo(photo).Extension.Substring(1);

                                                    db.QS_FORM_PHOTO.Add(new QS_FORM_PHOTO()
                                                    {
                                                        FORMUNIQUEID = formUniqueID,
                                                        CHECKITEMUNIQUEID = checkItemUniqueID,
                                                        CHECKITEMSEQ = checkItemSeq,
                                                        SEQ = seq,
                                                        EXTENSION = extension
                                                    });

                                                    System.IO.File.Copy(photo, Path.Combine(Config.QSFileFolderPath, string.Format("{0}_{1}_{2}_{3}.{4}", formUniqueID, checkItemUniqueID, checkItemSeq, seq, extension)), true);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region X_Form
                            db.QS_AUDITTYPE_FORM.AddRange(db.QS_AUDITTYPE.ToList().Select(x => new QS_AUDITTYPE_FORM
                            {
                                FORMUNIQUEID = formUniqueID,
                                UNIQUEID = x.UNIQUEID,
                                DESCRIPTION = x.DESCRIPTION
                            }).ToList());

                            if (db.QS_FACTORY.Any(x => x.UNIQUEID == factoryUniqueID))
                            {
                                var checkItemList = (from x in db.QS_FACTORY_CHECKITEM
                                                     join c in db.QS_CHECKITEM
                                                     on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                                     where x.FACTORYUNIQUEID == factoryUniqueID
                                                     select c).ToList();

                                db.QS_CHECKITEM_FORM.AddRange(checkItemList.Select(x => new QS_CHECKITEM_FORM
                                {
                                    FORMUNIQUEID = formUniqueID,
                                    UNIQUEID = x.UNIQUEID,
                                    TYPEID = x.TYPEID,
                                    TYPEEDESCRIPTION = x.TYPEEDESCRIPTION,
                                    TYPECDESCRIPTION = x.TYPECDESCRIPTION,
                                    ID = x.ID,
                                    EDESCRIPTION = x.EDESCRIPTION,
                                    CDESCRIPTION = x.CDESCRIPTION,
                                    CHECKTIMES = x.CHECKTIMES,
                                    UNIT = x.UNIT
                                }).ToList());
                            }
                            else
                            {
                                var checkItemList = db.QS_CHECKITEM.ToList();

                                db.QS_CHECKITEM_FORM.AddRange(checkItemList.Select(x => new QS_CHECKITEM_FORM
                                {
                                    FORMUNIQUEID = formUniqueID,
                                    UNIQUEID = x.UNIQUEID,
                                    TYPEID = x.TYPEID,
                                    TYPEEDESCRIPTION = x.TYPEEDESCRIPTION,
                                    TYPECDESCRIPTION = x.TYPECDESCRIPTION,
                                    ID = x.ID,
                                    EDESCRIPTION = x.EDESCRIPTION,
                                    CDESCRIPTION = x.CDESCRIPTION,
                                    CHECKTIMES = x.CHECKTIMES,
                                    UNIT = x.UNIT
                                }).ToList());
                            }

                            db.QS_CHECKITEMREMARK_FORM.AddRange(db.QS_CHECKITEMREMARK.ToList().Select(x => new QS_CHECKITEMREMARK_FORM
                            {
                                FORMUNIQUEID = formUniqueID,
                                CHECKITEMUNIQUEID = x.CHECKITEMUNIQUEID,
                                REMARKUNIQUEID = x.REMARKUNIQUEID
                            }).ToList());

                            db.QS_FACTORY_FORM.AddRange(db.QS_FACTORY.ToList().Select(x => new QS_FACTORY_FORM
                            {
                                FORMUNIQUEID = formUniqueID,
                                UNIQUEID = x.UNIQUEID,
                                DESCRIPTION = x.DESCRIPTION
                            }).ToList());

                            db.QS_GRADE_FORM.AddRange(db.QS_GRADE.ToList().Select(x => new QS_GRADE_FORM
                            {
                                FORMUNIQUEID = formUniqueID,
                                UNIQUEID = x.UNIQUEID,
                                DESCRIPTION = x.DESCRIPTION
                            }).ToList());

                            db.QS_REMARK_FORM.AddRange(db.QS_REMARK.ToList().Select(x => new QS_REMARK_FORM
                            {
                                FORMUNIQUEID = formUniqueID,
                                UNIQUEID = x.UNIQUEID,
                                DESCRIPTION = x.DESCRIPTION
                            }).ToList());

                            db.QS_RESDEPARTMENT_FORM.AddRange(db.QS_RESDEPARTMENT.ToList().Select(x => new QS_RESDEPARTMENT_FORM
                            {
                                FORMUNIQUEID = formUniqueID,
                                UNIQUEID = x.UNIQUEID,
                                DESCRIPTION = x.DESCRIPTION
                            }).ToList());

                            db.QS_RISK_FORM.AddRange(db.QS_RISK.ToList().Select(x => new QS_RISK_FORM
                            {
                                FORMUNIQUEID = formUniqueID,
                                UNIQUEID = x.UNIQUEID,
                                DESCRIPTION = x.DESCRIPTION
                            }).ToList());

                            db.QS_SHIFT_FORM.AddRange(db.QS_SHIFT.ToList().Select(x => new QS_SHIFT_FORM
                            {
                                FORMUNIQUEID = formUniqueID,
                                UNIQUEID = x.UNIQUEID,
                                DESCRIPTION = x.DESCRIPTION
                            }).ToList());

                            db.QS_STATION_FORM.AddRange(db.QS_STATION.ToList().Select(x => new QS_STATION_FORM
                            {
                                FORMUNIQUEID = formUniqueID,
                                UNIQUEID = x.UNIQUEID,
                                DESCRIPTION = x.DESCRIPTION
                            }).ToList());
                            #endregion

                            db.SaveChanges();
                        }
                    }

                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var photos = Directory.GetFiles(extractPath);

                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "SELECT * FROM Photo";

                            using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                            {
                                using (DataTable dt = new DataTable())
                                {
                                    adapter.Fill(dt);

                                    if (dt != null && dt.Rows.Count > 0)
                                    {
                                        for (int i = 0; i < dt.Rows.Count; i++)
                                        {
                                            //var photo = Path.Combine(extractPath, dt.Rows[i]["FileName"].ToString());

                                            var fileName = dt.Rows[i]["FileName"].ToString();

                                            var photo = photos.FirstOrDefault(x => x.Contains(fileName));

                                            if (System.IO.File.Exists(photo))
                                            {
                                                var extension = new FileInfo(photo).Extension.Substring(1);

                                                var uniqueID = Guid.NewGuid().ToString();

                                                db.QS_PHOTO.Add(new QS_PHOTO()
                                                {
                                                    UNIQUEID = uniqueID,
                                                    PHOTOTIME = DateTime.Parse(dt.Rows[i]["PhotoTime"].ToString()),
                                                    USERID = dt.Rows[i]["UserID"].ToString(),
                                                    EXTENSION = extension
                                                });

                                                System.IO.File.Copy(photo, Path.Combine(Config.QSFileFolderPath, string.Format("{0}.{1}", uniqueID, extension)), true);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        db.SaveChanges();
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

        ~TransHelper()
        {
            Dispose(false);
        }

        #endregion
    }
}
