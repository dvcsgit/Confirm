using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLite.Recover
{
    class Program
    {
        static void Main(string[] args)
        {
            var folder = Utility.Config.EquipmentMaintenanceSQLiteErrorFolderPath;
            var files = Directory.GetFiles(folder);

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);

                var uniqueID = fileInfo.Name.Substring(0, fileInfo.Name.LastIndexOf('.'));

                var extractFolder = Path.Combine(folder, uniqueID);

                Directory.CreateDirectory(extractFolder);

                using (ZipFile zip = new ZipFile(file))
                {
                    zip.ExtractAll(extractFolder);
                }

                var sqlite = Path.Combine(extractFolder, "EquipmentMaintenance.db");

                var connString = string.Format("Data Source={0};Version=3;", sqlite);

                using (SQLiteConnection conn = new SQLiteConnection(connString))
                {
                    conn.Open();

                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        var columnList = new List<string>();

                        cmd.CommandText = "PRAGMA table_info(ArriveRecord)";

                        using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                adapter.Fill(dt);

                                columnList = dt.AsEnumerable().Select(x => x["name"].ToString()).ToList();
                            }
                        }

                        if (!columnList.Any(x => x == "MinTimeSpan"))
                        {
                            cmd.CommandText = "ALTER TABLE ArriveRecord ADD Column MinTimeSpan DOUBLE";

                            cmd.ExecuteNonQuery();
                        }

                        if (!columnList.Any(x => x == "TotalTimeSpan"))
                        {
                            cmd.CommandText = "ALTER TABLE ArriveRecord ADD Column TotalTimeSpan DOUBLE";

                            cmd.ExecuteNonQuery();
                        }

                        if (!columnList.Any(x => x == "TimeSpanAbnormalReasonUniqueID"))
                        {
                            cmd.CommandText = "ALTER TABLE ArriveRecord ADD Column TimeSpanAbnormalReasonUniqueID VARCHAR(40)";

                            cmd.ExecuteNonQuery();
                        }

                        if (!columnList.Any(x => x == "TimeSpanAbnormalReasonRemark"))
                        {
                            cmd.CommandText = "ALTER TABLE ArriveRecord ADD Column TimeSpanAbnormalReasonRemark NVARCHAR(128)";

                            cmd.ExecuteNonQuery();
                        }
                    }

                    conn.Close();
                }

                var resultFolder = Path.Combine(Utility.Config.EquipmentMaintenanceSQLiteUploadFolderPath, uniqueID);

                Directory.CreateDirectory(resultFolder);

                var temp = Directory.GetFiles(extractFolder);

                using (ZipFile zip = new ZipFile(Path.Combine(resultFolder, "EquipmentMaintenance.Upload.zip")))
                {
                    foreach (var t in temp)
                    {
                        zip.AddFile(t, "");
                    }

                    zip.Save();
                }

                Directory.Delete(extractFolder, true);

                File.Delete(file);
            }
        }
    }
}
