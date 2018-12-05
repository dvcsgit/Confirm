using DbEntity.MSSQL.EquipmentMaintenance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Utility;
using WebApi.CHIMEI.E.Models;

namespace WebApi.CHIMEI.E.DataAccess
{
    public class MobileReleaseDataAccessor
    {
        public static FileModel Get(int ID)
        {
            var result = new FileModel();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var file = db.Version.First(x => x.Id == ID);

                    result = new FileModel()
                    {
                        ApkName = file.ApkName,
                        Device = Define.EnumParse<Define.EnumDevice>(file.Device)
                    };
                }
            }
            catch (Exception ex)
            {
                result = null;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return result;
        }
    }
}