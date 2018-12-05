using DbEntity.ASE;
using Models.ASE.QS.MobileRelease;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace DataAccess.ASE.QS
{
    public class MobileReleaseDataAccessor
    {
        public static FileModel Get(int ID)
        {
            var result = new FileModel();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var file = db.QA_VERSION.First(x => x.ID == ID);

                    result = new FileModel()
                    {
                        ApkName = file.APKNAME,
                        Device = Define.EnumParse<Define.EnumDevice>(file.DEVICE)
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
