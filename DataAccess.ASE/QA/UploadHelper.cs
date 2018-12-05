using System;
using System.Reflection;
using Utility;
using Utility.Models;
using DbEntity.ASE;

namespace DataAccess.ASE.QA
{
    public class UploadHelper
    {
        public static RequestResult Upload(string Guid, string UserID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    db.QA_UPLOADLOG.Add(new QA_UPLOADLOG()
                    {
                        UNIQUEID = Guid,
                        UPLOADUSERID = UserID,
                        UPLOADTIME = DateTime.Now
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
