using DbEntity.MSSQL.TankPatrol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace DataSync.TankPatrol
{
    public class VerHelper
    {
        public static RequestResult GetLastVer(string appName)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (TankDbEntities db = new TankDbEntities())
                {
                    var device = Define.EnumDevice.Android.ToString();

                    var version = db.Version.Where(x => x.AppName == appName && x.Device == device).OrderByDescending(x => x.VerCode).FirstOrDefault();

                    if (version != null)
                    {
                        result.ReturnData(new Models.TankPatrol.DataSync.Version()
                        {
                            ApkName = version.ApkName,
                            VerCode = version.VerCode,
                            ReleaseNote = version.ReleaseNote,
                            IsForceUpdate = version.IsForceUpdate,
                            Id = version.Id,
                            VerName = version.VerName,
                            Device = version.Device,
                            DateReleased = version.DateReleased,
                            DateCreated = version.DateCreated,
                            AppName = version.AppName
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
