﻿using DbEntity.MSSQL.TruckPatrol;
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
    public class UploadHelper
    {
        public static RequestResult Upload(string Guid, string UserID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (TDbEntities db = new TDbEntities())
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
