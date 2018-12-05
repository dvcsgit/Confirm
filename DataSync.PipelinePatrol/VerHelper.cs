using DbEntity.MSSQL.PipelinePatrol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace DataSync.PipelinePatrol
{
    public class VerHelper
    {
        public static RequestResult GetLastVer(string appName)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    var version = db.Version.Where(x => x.AppName == appName && x.Device == Define.EnumDevice.Android.ToString()).OrderByDescending(x => x.VerCode).First();

                    result.ReturnData(version);
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
