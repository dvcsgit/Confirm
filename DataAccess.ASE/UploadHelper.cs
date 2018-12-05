using DbEntity.ASE;
using Models.ASE.DataSync;
using System;
using System.Linq;
using System.Reflection;
using Utility;
using Utility.Models;

namespace DataAccess.ASE
{
    public class UploadHelper
    {
        public static RequestResult Upload(string Guid, ApiFormInput FormInput)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    db.UPLOADLOG.Add(new UPLOADLOG()
                    {
                        UNIQUEID = Guid,
                        UPLOADUSERID = FormInput.UserID,
                        UPLOADTIME = DateTime.Now
                    });

                    db.SaveChanges();

                    result.Success();

                    try
                    {
                        if (!string.IsNullOrEmpty(FormInput.MacAddress))
                        {
                            var status = db.DEVICESTATUS.FirstOrDefault(x => x.MACADDRESS == FormInput.MacAddress);

                            if (status != null)
                            {
                                status.APPVERSION = FormInput.AppVersion;
                                status.IMEI = FormInput.IMEI;
                                status.LASTUPDATETIME = DateTime.Now;
                            }
                            else
                            {
                                status = new DEVICESTATUS()
                                {
                                    UNIQUEID = System.Guid.NewGuid().ToString(),
                                    APPVERSION = FormInput.AppVersion,
                                    IMEI = FormInput.IMEI,
                                    MACADDRESS = FormInput.MacAddress,
                                    LASTUPDATETIME = DateTime.Now
                                };

                                db.DEVICESTATUS.Add(status);
                            }
                        }
                        else if (!string.IsNullOrEmpty(FormInput.IMEI))
                        {
                            var status = db.DEVICESTATUS.FirstOrDefault(x => x.IMEI == FormInput.IMEI);

                            if (status != null)
                            {
                                status.APPVERSION = FormInput.AppVersion;
                                status.MACADDRESS = FormInput.MacAddress;
                                status.LASTUPDATETIME = DateTime.Now;
                            }
                            else
                            {
                                status = new DEVICESTATUS()
                                {
                                    UNIQUEID = System.Guid.NewGuid().ToString(),
                                    APPVERSION = FormInput.AppVersion,
                                    IMEI = FormInput.IMEI,
                                    MACADDRESS = FormInput.MacAddress,
                                    LASTUPDATETIME = DateTime.Now
                                };

                                db.DEVICESTATUS.Add(status);
                            }
                        }
                    }
                    catch
                    {
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
