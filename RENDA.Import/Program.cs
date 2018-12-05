using DbEntity.MSSQL.PipelinePatrol;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace RENDA.Import
{
    class Program
    {
        static void Main(string[] args)
        {
            var files = System.IO.Directory.GetFiles(@"C:\Users\Administrator.WIN-14846625429\Desktop\PIC");

            using (PDbEntities db = new PDbEntities())
            {
                foreach (var file in files)
                {
                    var fileInfo = new System.IO.FileInfo(file);

                    var fileName = fileInfo.Name.Substring(0, fileInfo.Name.LastIndexOf('.'));
                    var extension = fileInfo.Name.Substring(fileInfo.Name.LastIndexOf('.') + 1);

                    var pipePoint = db.PipePoint.FirstOrDefault(x => x.ID == fileName);

                    if (pipePoint != null)
                    {
                        var uniqueID = Guid.NewGuid().ToString();

                        var fileModel = new File()
                        {
                            UniqueID = uniqueID,
                            OrganizationUniqueID = "d95e536e-a37b-451c-a234-8327e74115dd",
                            PipelineUniqueID = null,
                            PipePointUniqueID = pipePoint.UniqueID,
                            FolderUniqueID = null,
                            FileName = fileName,
                            Extension = extension,
                            IsDownload2Mobile = true,
                            Size = Convert.ToInt32(fileInfo.Length),
                            UserID = "SYS",
                            LastModifyTime = DateTime.Now
                        };

                        db.File.Add(fileModel);

                        db.SaveChanges();

                        System.IO.File.Copy(file, System.IO.Path.Combine(Config.PipelinePatrolFileFolderPath, uniqueID + "." + extension));

                        using (System.IO.FileStream fs = System.IO.File.Create(System.IO.Path.Combine(Config.PipelinePatrolFileFolderPath, uniqueID + ".zip")))
                        {
                            using (ZipOutputStream zipStream = new ZipOutputStream(fs))
                            {
                                zipStream.SetLevel(9); //0-9, 9 being the highest level of compression

                                ZipHelper.CompressFolder(System.IO.Path.Combine(Config.PipelinePatrolFileFolderPath, uniqueID + "." + extension), zipStream);

                                zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream

                                zipStream.Close();
                            }
                        }
                    }
                }
            }
        }
    }
}
