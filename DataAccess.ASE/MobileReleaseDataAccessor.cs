using System;
using System.Linq;
using System.Reflection;
using Utility;
using Utility.Models;
using DbEntity.ASE;
using Models.EquipmentMaintenance.MobileRelease;

namespace DataAccess.ASE
{
    public class MobileReleaseDataAccessor
    {
        public static RequestResult Upload(UploadFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var verCode = 1;

                    var ver = db.VERSION.Where(x => x.APPNAME == Model.FormInput.AppName && x.DEVICE == Model.FormInput.Device.ToString()).OrderByDescending(x => x.VERCODE).FirstOrDefault();

                    if (ver != null)
                    {
                        verCode = ver.VERCODE.Value + 1;
                    }

                    db.VERSION.Add(new DbEntity.ASE.VERSION()
                    {
                        APPNAME = Model.FormInput.AppName,
                        APKNAME = Model.FormInput.ApkName,
                        VERNAME = Model.FormInput.VerName,
                        VERCODE = verCode,
                        RELEASENOTE = Model.FormInput.ReleaseNote,
                        ISFORCEUPDATE = Model.FormInput.IsForceUpdate?"Y":"N",
                        DATERELEASED = Model.FormInput.ReleaseDate,
                        DATECREATED = DateTime.Now,
                        DEVICE = Model.FormInput.Device.ToString()
                    });

                    db.SaveChanges();

                    System.IO.File.Copy(Model.TempFile, System.IO.Path.Combine(Config.EquipmentMaintenanceMobileReleaseFolderPath, Model.FormInput.ApkName + "." + Model.Extension));

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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var deviceList = db.VERSION.Select(x => x.DEVICE).Distinct().OrderBy(x => x).ToList();

                    foreach (var device in deviceList)
                    {
                        var ver = db.VERSION.Where(x => x.DEVICE == device).OrderByDescending(x => x.VERCODE).First();

                        model.ItemList.Add(new GridItem()
                        {
                            ID = ver.ID,
                            Device = ver.DEVICE,
                            VerName = ver.VERNAME,
                            ReleaseDate = DateTimeHelper.DateTime2DateStringWithSeperator(ver.DATERELEASED)
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var file = db.VERSION.First(x => x.ID == ID);

                    result = new FileModel()
                    {
                        ApkName = file.APKNAME,
                        Device = Define.EnumParse<Define.EnumDevice>(file.DEVICE)
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
