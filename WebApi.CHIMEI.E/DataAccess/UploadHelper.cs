using DbEntity.MSSQL.EquipmentMaintenance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Utility;
using Utility.Models;
using WebApi.CHIMEI.E.Models;

namespace WebApi.CHIMEI.E.DataAccess
{
    public class UploadHelper
    {
        public static RequestResult Upload(string Guid, ApiFormInput FormInput)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    db.UploadLog.Add(new UploadLog()
                    {
                        UniqueID = Guid,
                        UploadUserID = FormInput != null ? FormInput.UserID : "",
                        UploadTime = DateTime.Now
                    });

                    db.SaveChanges();

                    result.Success();

                    try
                    {
                        if (FormInput != null)
                        {
                            if (!string.IsNullOrEmpty(FormInput.MacAddress))
                            {
                                var status = db.DeviceStatus.FirstOrDefault(x => x.MacAddress == FormInput.MacAddress);

                                if (status != null)
                                {
                                    status.AppVersion = FormInput.AppVersion;
                                    status.IMEI = FormInput.IMEI;
                                    status.LastUpdateTime = DateTime.Now;
                                }
                                else
                                {
                                    status = new DeviceStatus()
                                    {
                                        UniqueID = System.Guid.NewGuid().ToString(),
                                        AppVersion = FormInput.AppVersion,
                                        IMEI = FormInput.IMEI,
                                        MacAddress = FormInput.MacAddress,
                                        LastUpdateTime = DateTime.Now
                                    };

                                    db.DeviceStatus.Add(status);
                                }
                            }
                            else if (!string.IsNullOrEmpty(FormInput.IMEI))
                            {
                                var status = db.DeviceStatus.FirstOrDefault(x => x.IMEI == FormInput.IMEI);

                                if (status != null)
                                {
                                    status.AppVersion = FormInput.AppVersion;
                                    status.MacAddress = FormInput.MacAddress;
                                    status.LastUpdateTime = DateTime.Now;
                                }
                                else
                                {
                                    status = new DeviceStatus()
                                    {
                                        UniqueID = System.Guid.NewGuid().ToString(),
                                        AppVersion = FormInput.AppVersion,
                                        IMEI = FormInput.IMEI,
                                        MacAddress = FormInput.MacAddress,
                                        LastUpdateTime = DateTime.Now
                                    };

                                    db.DeviceStatus.Add(status);
                                }
                            }

                            db.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(MethodBase.GetCurrentMethod(), ex);
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