using DbEntity.MSSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Utility;
using Utility.Models;

namespace WebApi.CHIMEI.E.DataAccess
{
    public class UserDataAccessor
    {
        public static RequestResult UpdateUID(string UserID, string UID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (DbEntities db = new DbEntities())
                {
                    var user = db.User.First(x => x.ID == UserID);

                    user.UID = UID;

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