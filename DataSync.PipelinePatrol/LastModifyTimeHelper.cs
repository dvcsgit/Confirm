using DataAccess;
using DbEntity.MSSQL;
using DbEntity.MSSQL.PipelinePatrol;
using Models.Authenticated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace DataSync.PipelinePatrol
{
    public class LastModifyTimeHelper
    {
        public static string Get(string UserID)
        {
            string lastModifyTime = string.Empty;

            try
            {
                var organizationList = new List<UserOrganizationPermission>();

                using (DbEntities db = new DbEntities())
                {
                    var user = db.User.First(x => x.ID == UserID);

                    organizationList = OrganizationDataAccessor.GetUserOrganizationPermissionList(user.OrganizationUniqueID);
                }

                var queryableOrganizationList = organizationList.Where(x => x.Permission == Define.EnumOrganizationPermission.Queryable).Select(x=>x.UniqueID).ToList();

                var lastModifyTimeList = new List<DateTime>();

                using (PDbEntities db = new PDbEntities())
                {
                    // 只要這些有資料異動 會找最後一筆的更新的資料時間 當最後更新日
                    lastModifyTimeList.AddRange(db.Pipeline.Where(x => queryableOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.LastModifyTime).ToList());
                    lastModifyTimeList.AddRange(db.Route.Where(x => queryableOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.LastModifyTime).ToList());
                    lastModifyTimeList.AddRange(db.AbnormalReason.Where(x => queryableOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.LastModifyTime).ToList());
                    lastModifyTimeList.AddRange(db.HandlingMethod.Where(x => queryableOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.LastModifyTime).ToList());
                    lastModifyTimeList.AddRange(db.Job.Where(x => queryableOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.LastModifyTime).ToList());
                    lastModifyTimeList.AddRange(db.CheckItem.Where(x => queryableOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.LastModifyTime).ToList());
                    //AbnormalReason
                    //HandlingMethod
                    //Job
                    //CheckItem
                }
                
                var last = lastModifyTimeList.OrderByDescending(x => x).FirstOrDefault();

                if (last != null)
                {
                    lastModifyTime = DateTimeHelper.DateTime2DateTimeString(last);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return lastModifyTime;
        }
    }
}
