using DbEntity.MSSQL.TankPatrol;
using Models.TankPatrol.MobileRelease;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace DataAccess.TankPatrol
{
    public class MobileReleaseDataAccessor
    {
        public static RequestResult GetGridViewModel()
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new GridViewModel();

                using (TankDbEntities db = new TankDbEntities())
                {
                    var deviceList = db.Version.Select(x => x.Device).Distinct().OrderBy(x => x).ToList();

                    foreach (var device in deviceList)
                    {
                        var ver = db.Version.Where(x => x.Device == device).OrderByDescending(x => x.VerCode).First();

                        model.ItemList.Add(new GridItem()
                        {
                            ID = ver.Id,
                            Device = ver.Device,
                            VerName = ver.VerName,
                            ReleaseDate = DateTimeHelper.DateTime2DateStringWithSeperator(ver.DateReleased)
                        });
                    }
                }

                result.ReturnData(model);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static FileModel Get(int ID)
        {
            var result = new FileModel();

            try
            {
                using (TankDbEntities db = new TankDbEntities())
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
