using DbEntity.ASE;
using Models.ASE.QS.PhotoManagement;
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
    public class PhotoDataAccessor
    {
        public static RequestResult Query()
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    result.ReturnData(new GridViewModel()
                    {
                        ItemList = (from p in db.QS_PHOTO
                                    join u in db.ACCOUNT
                                    on p.USERID equals u.ID into tmpUser
                                    from u in tmpUser.DefaultIfEmpty()
                                    select new GridItem
                                    {
                                        UniqueID = p.UNIQUEID,
                                        UserID = p.USERID,
                                        UserName = u != null ? u.NAME : "",
                                        PhotoTime = p.PHOTOTIME,
                                        Extension = p.EXTENSION
                                    }).OrderByDescending(x => x.PhotoTime).ToList()
                    });
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

        public static RequestResult Delete(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    db.QS_PHOTO.Remove(db.QS_PHOTO.First(x => x.UNIQUEID == UniqueID));

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage("刪除成功");
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
