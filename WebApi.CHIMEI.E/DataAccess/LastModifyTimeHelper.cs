using DbEntity.MSSQL.EquipmentMaintenance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Utility;
using Utility.Models;

namespace WebApi.CHIMEI.E.DataAccess
{
    public class LastModifyTimeHelper
    {
        public static string Get(string JobUniqueID)
        {
            return string.Empty;
        }

        public static RequestResult Get(List<string> JobList)
        {
            RequestResult result = new RequestResult();

            try
            {
                var lastModifyTimeList = new Dictionary<string, string>();

                foreach (var jobUniqueID in JobList)
                {
                    lastModifyTimeList.Add(jobUniqueID, Get(jobUniqueID));
                }

                result.ReturnData(lastModifyTimeList);
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