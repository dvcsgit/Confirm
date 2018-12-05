using System;
using System.Linq;
using System.Reflection;
using Utility;
using Utility.Models;
using DbEntity.MSSQL.GuardPatrol;

namespace DataSync.GuardPatrol
{
    public class VerHelper
    {
        public static RequestResult GetLastVer(string appName)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (GDbEntities db = new GDbEntities())
                {
                    var version = db.Version.Where(x => x.AppName == appName && x.Device == Define.EnumDevice.Android.ToString()).OrderByDescending(x => x.VerCode).FirstOrDefault();

                    if (version != null)
                    {
                        result.ReturnData(version);
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
