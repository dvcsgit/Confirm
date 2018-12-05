using System;
using System.Reflection;
using Utility;
using Utility.Models;
using DbEntity.MSSQL.GuardPatrol;

namespace DataSync.GuardPatrol
{
    public class UploadHelper
    {
        public static RequestResult Upload(string Guid, string UserID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (GDbEntities db = new GDbEntities())
                {
                    db.UploadLog.Add(new UploadLog()
                    {
                        UniqueID = Guid,
                        UploadUserID = UserID,
                        UploadTime = DateTime.Now
                    });

                    db.SaveChanges();
                }

                result.Success();
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
