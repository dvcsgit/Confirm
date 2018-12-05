using System;
using System.Linq;
using System.Reflection;
using Utility;
using Utility.Models;
using DbEntity.ASE;

namespace DataAccess.ASE.QA
{
    public class VerHelper
    {
        public static RequestResult GetLastVer(string appName)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var version = db.QA_VERSION.Where(x => x.APPNAME == appName && x.DEVICE == Define.EnumDevice.Android.ToString()).OrderByDescending(x => x.VERCODE).FirstOrDefault();

                    if (version != null)
                    {
                        result.ReturnData(new Models.EquipmentMaintenance.DataSync.Version()
                        {
                            ApkName = version.APKNAME,
                            AppName = version.APPNAME,
                            DateCreated = version.DATECREATED.Value,
                            DateReleased = version.DATERELEASED.Value,
                            Device = version.DEVICE,
                            Id = version.ID,
                            VerName = version.VERNAME,
                            IsForceUpdate = version.ISFORCEUPDATE == "Y",
                            ReleaseNote = version.RELEASENOTE,
                            VerCode = version.VERCODE.Value
                        });
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
    }
}
