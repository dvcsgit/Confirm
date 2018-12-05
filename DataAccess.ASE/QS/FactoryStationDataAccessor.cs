using DbEntity.ASE;
using Models.ASE.QS.FactoryStationManagement;
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
    public class FactoryStationDataAccessor
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
                        StationList = (from x in db.QS_FACTORY_STATION
                                       join s in db.QS_STATION
                                       on x.STATIONUNIQUEID equals s.UNIQUEID
                                       where x.FACTORYUNIQUEID == UniqueID
                                       select new StationModel
                                       {
                                           UniqueID = s.UNIQUEID,
                                           Type = s.TYPE,
                                           Description = s.DESCRIPTION
                                       }).OrderBy(x => x.Type).ThenBy(x => x.Description).ToList()
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
                        StationList = db.QS_STATION.Select(x => new StationModel
                        {
                            UniqueID = x.UNIQUEID,
                            Type = x.TYPE,
                            Description = x.DESCRIPTION
                        }).OrderBy(x=>x.Type).ThenBy(x => x.Description).ToList(),
                        FactoryStationList = (from x in db.QS_FACTORY_STATION
                                              join s in db.QS_STATION
                                              on x.STATIONUNIQUEID equals s.UNIQUEID
                                              where x.FACTORYUNIQUEID == UniqueID
                                              select s.UNIQUEID).ToList()
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
                    db.QS_FACTORY_STATION.RemoveRange(db.QS_FACTORY_STATION.Where(x => x.FACTORYUNIQUEID == Model.UniqueID).ToList());

                    db.QS_FACTORY_STATION.AddRange(Model.FormInput.StationList.Select(x => new QS_FACTORY_STATION
                    {
                        FACTORYUNIQUEID = Model.UniqueID,
                        STATIONUNIQUEID = x
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
