using System;
using System.Linq;
using System.Reflection;
using Utility;
#if ORACLE
using DbEntity.ORACLE.EquipmentMaintenance;
#else
using DbEntity.MSSQL.EquipmentMaintenance;
#endif

namespace DataSync.EquipmentMaintenance
{
    public class VersionHelper
    {
        public static string Get(string AppName, Define.EnumDevice Device)
        {
            string version = string.Empty;

            try
            {
                using (EDbEntities db = new EDbEntities())
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
