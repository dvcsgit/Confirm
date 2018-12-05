using DbEntity.MSSQL.TruckPatrol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace DataSync.TruckPatrol
{
    public class VersionHelper
    {
        public static string Get(string AppName, Define.EnumDevice Device)
        {
            string version = string.Empty;

            try
            {
                using (TDbEntities db = new TDbEntities())
                {
                    var query = db.Version.Where(x => x.AppName == AppName && x.Device == Device.ToString()).OrderByDescending(x => x.VerCode).FirstOrDefault();

                    if (query != null)
                    {
                        version = query.VerName;
                    }
                }
            }
            catch (Exception ex)
            {
                version = string.Empty;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return version;
        }
    }
}
