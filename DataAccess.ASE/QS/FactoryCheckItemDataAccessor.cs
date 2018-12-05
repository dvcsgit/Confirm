using DbEntity.ASE;
using Models.ASE.QS.FactoryCheckItemManagement;
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
    public class FactoryCheckItemDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = db.QS_FACTORY.AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.KeyWord))
                    {
                        query = query.Where(x => x.DESCRIPTION.Contains(Parameters.KeyWord));
                    }

                    result.ReturnData(new GridViewModel()
                    {
                        ItemList = query.Select(x => new GridItem()
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION
                        }).OrderBy(x => x.Description).ToList()
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

        public static RequestResult GetDetailViewModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var factory = db.QS_FACTORY.First(x => x.UNIQUEID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = factory.UNIQUEID,
                        Description = factory.DESCRIPTION,
                        CheckItemList = (from x in db.QS_FACTORY_CHECKITEM
                                         join c in db.QS_CHECKITEM
                                         on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                         where x.FACTORYUNIQUEID == UniqueID
                                         select new CheckItemModel
                                         {
                                             TypeID=c.TYPEID,
                                             TypeCDescription=c.TYPECDESCRIPTION,
                                             TypeEDescription = c.TYPEEDESCRIPTION,
                                             ID = c.ID,
                                             EDescription = c.EDESCRIPTION,
                                             CDescription = c.CDESCRIPTION,
                                             CheckTimes = c.CHECKTIMES,
                                             UniqueID = c.UNIQUEID,
                                             Unit = c.UNIT
                                         }).OrderBy(x => x.TypeID).ThenBy(x=>x.ID).ToList()
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

        public static RequestResult GetEditFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var factory = db.QS_FACTORY.First(x => x.UNIQUEID == UniqueID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = factory.UNIQUEID,
                        Description = factory.DESCRIPTION,
                        CheckItemList = db.QS_CHECKITEM.Select(x => new CheckItemModel
                                         {
                                             TypeID = x.TYPEID,
                                             TypeCDescription = x.TYPECDESCRIPTION,
                                             TypeEDescription = x.TYPEEDESCRIPTION,
                                             ID = x.ID,
                                             EDescription = x.EDESCRIPTION,
                                             CDescription = x.CDESCRIPTION,
                                             CheckTimes = x.CHECKTIMES,
                                             UniqueID = x.UNIQUEID,
                                             Unit = x.UNIT
                                         }).OrderBy(x => x.TypeID).ThenBy(x => x.ID).ToList(),
                        FactoryCheckItemList = (from x in db.QS_FACTORY_CHECKITEM
                                                join c in db.QS_CHECKITEM
                                                on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                                where x.FACTORYUNIQUEID == UniqueID
                                                select c.UNIQUEID).ToList()
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

        public static RequestResult Edit(EditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    db.QS_FACTORY_CHECKITEM.RemoveRange(db.QS_FACTORY_CHECKITEM.Where(x => x.FACTORYUNIQUEID == Model.UniqueID).ToList());

                    db.QS_FACTORY_CHECKITEM.AddRange(Model.FormInput.CheckItemList.Select(x => new QS_FACTORY_CHECKITEM
                    {
                        FACTORYUNIQUEID = Model.UniqueID,
                        CHECKITEMUNIQUEID = x
                    }).ToList());

                    db.SaveChanges();

                    result.ReturnSuccessMessage("編輯成功");
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
