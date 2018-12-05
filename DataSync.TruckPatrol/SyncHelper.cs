using DataAccess;
using DataAccess.TruckPatrol;
using DbEntity.MSSQL;
using DbEntity.MSSQL.TruckPatrol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace DataSync.TruckPatrol
{
    public class SyncHelper
    {
        public static RequestResult GetLastModifyTime(string UserID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var ancestorOrganizationUniqueID = string.Empty;

                using (DbEntities db = new DbEntities())
                {
                    var user = db.User.First(x => x.ID == UserID);

                    ancestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(user.OrganizationUniqueID);
                }

                var organizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(ancestorOrganizationUniqueID, true);

                using (TDbEntities db = new TDbEntities())
                {
                    result.ReturnData(db.Truck.Where(x => organizationList.Contains(x.OrganizationUniqueID)).ToList().ToDictionary(x => x.UniqueID, x => DateTimeHelper.DateTime2DateTimeString(x.LastModifyTime)));
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
