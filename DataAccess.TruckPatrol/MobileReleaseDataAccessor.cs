using System;
using System.Linq;
using System.Reflection;
using Utility;
using Utility.Models;
using DbEntity.MSSQL.TruckPatrol;
using Models.TruckPatrol.MobileRelease;

namespace DataAccess.TruckPatrol
{
    public class MobileReleaseDataAccessor
    {
        public static RequestResult Upload(UploadFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (TDbEntities db = new TDbEntities())
                {
                    var verCode = 1;

                    var ver = db.Version.Where(x => x.AppName == Model.FormInput.AppName && x.Device == Model.FormInput.Device.ToString()).OrderByDescending(x => x.VerCode).FirstOrDefault();

                    if (ver != null)
                    {
                        verCode = ver.VerCode + 1;
                    }

                    db.Version.Add(new DbEntity.MSSQL.TruckPatrol.Version()
                    {
                        AppName = Model.FormInput.AppName,
                        ApkName = Model.FormInput.ApkName,
                        VerName = Model.FormInput.VerName,
                        VerCode = verCode,
                        ReleaseNote = Model.FormInput.ReleaseNote,
                        IsForceUpdate = Model.FormInput.IsForceUpdate,
                        DateReleased = Model.FormInput.ReleaseDate,
                        DateCreated = DateTime.Now,
                        Device = Model.FormInput.Device.ToString()
                    });

                    db.SaveChanges();

                    System.IO.File.Copy(Model.TempFile, System.IO.Path.Combine(Config.TruckPatrolMobileReleaseFolderPath, Model.FormInput.ApkName + "." + Model.Extension));

                    System.IO.File.Delete(Model.TempFile);
                }

                result.ReturnSuccessMessage("Release Success");
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetGridViewModel()
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new GridViewModel();

                using (TDbEntities db = new TDbEntities())
                {
                    var deviceList = db.Version.Select(x => x.Device).Distinct().OrderBy(x => x).ToList();

                    foreach (var device in deviceList)
                    {
                        var ver = db.Version.Where(x => x.Device == device).OrderByDescending(x => x.VerCode).First();

                        model.ItemList.Add(new GridItem()
                        {
                            ID = ver.Id,
                            Device = ver.Device,
                            VerName = ver.VerName,
                            ReleaseDate = DateTimeHelper.DateTime2DateStringWithSeperator(ver.DateReleased)
                        });
                    }
                }

                result.ReturnData(model);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static FileModel Get(int ID)
        {
            var result = new FileModel();

            try
            {
                using (TDbEntities db = new TDbEntities())
                {
                    var file = db.Version.First(x => x.Id == ID);

                    result = new FileModel()
                    {
                        ApkName = file.ApkName,
                        Device = Define.EnumParse<Define.EnumDevice>(file.Device)
                    };
                }
            }
            catch (Exception ex)
            {
                result = null;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return result;
        }
    }
}
