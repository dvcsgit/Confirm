using DbEntity.ASE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace DataAccess.ASE.QS
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
