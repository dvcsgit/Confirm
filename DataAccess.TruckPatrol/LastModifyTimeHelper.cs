using DbEntity.MSSQL.TruckPatrol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace DataAccess.TruckPatrol
{
    public class LastModifyTimeHelper
    {
        public static string Get(string TruckUniqueID)
        {
            string lastModifyTime = string.Empty;

            try
            {
                var lastModifyTimeList = new List<DateTime>();

                using (TDbEntities db = new TDbEntities())
                {
                    lastModifyTimeList.Add(db.Truck.First(x => x.UniqueID == TruckUniqueID).LastModifyTime);
                }

                var last = lastModifyTimeList.OrderByDescending(x => x).FirstOrDefault();

                if (last != null)
                {
                    lastModifyTime = DateTimeHelper.DateTime2DateTimeString(last);
                }
            }
            catch (Exception ex)
            {
                lastModifyTime = string.Empty;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return lastModifyTime;
        }
    }
}
