﻿using System;
using System.Linq;
using System.Reflection;
using Utility;
using Utility.Models;
#if ORACLE
using DbEntity.ORACLE.EquipmentMaintenance;
#else
using DbEntity.MSSQL.EquipmentMaintenance;
#endif

namespace DataSync.EquipmentMaintenance
{
    public class VerHelper
    {
        public static RequestResult GetLastVer(string appName)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var device = Define.EnumDevice.Android.ToString();

                    if (appName == "AppSync")
                    {
                        device = Define.EnumDevice.WPF.ToString();
                    }
                    
                    var version = db.Version.Where(x => x.AppName == appName && x.Device == device).OrderByDescending(x => x.VerCode).FirstOrDefault();

                    if (version != null)
                    {
                        result.ReturnData(new Models.EquipmentMaintenance.DataSync.Version()
                        {
                            ApkName = version.ApkName,
                            VerCode = version.VerCode,
                            ReleaseNote = version.ReleaseNote,
                            IsForceUpdate = version.IsForceUpdate,
                            Id = version.Id,
                            VerName = version.VerName,
                            Device = version.Device,
                            DateReleased = version.DateReleased,
                            DateCreated = version.DateCreated,
                            AppName = version.AppName
                        });
                    }
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
