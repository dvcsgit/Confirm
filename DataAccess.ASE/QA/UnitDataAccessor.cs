using DbEntity.ASE;
using Models.ASE.QA.UnitManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Utility;
using Utility.Models;

namespace DataAccess.ASE.QA
{
    public class UnitDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try 
            {
                var model = new GridViewModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = db.QA_UNIT.AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.KeyWord))
                    {
                        query = query.Where(x => x.DESCRIPTION.Contains(Parameters.KeyWord));
                    }

                    var queryResults = query.OrderBy(x=>x.DESCRIPTION).ToList();

                    foreach (var queryResult in queryResults)
                    {
                        model.ItemList.Add(new GridItem()
                        {
                            UniqueID = queryResult.UNIQUEID,
                            Description = queryResult.DESCRIPTION,
                            ToleranceUnitList = db.QA_TOLERANCEUNIT.Where(x => x.UNITUNIQUEID == queryResult.UNIQUEID).OrderBy(x=>x.RATE).Select(x => x.DESCRIPTION).ToList()
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

        public static RequestResult GetDetailViewModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var unit = db.QA_UNIT.First(x => x.UNIQUEID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = unit.UNIQUEID,
                        Description = unit.DESCRIPTION,
                        ToleranceUnitList = db.QA_TOLERANCEUNIT.Where(x => x.UNITUNIQUEID == unit.UNIQUEID).Select(x => new ToleranceUnitModel
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION,
                            Rate = x.RATE.Value
                        }).OrderBy(x => x.Rate).ToList()
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

        public static RequestResult Create(CreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var unitUniqueID = Guid.NewGuid().ToString();

                    db.QA_UNIT.Add(new QA_UNIT()
                    {
                        UNIQUEID = unitUniqueID,
                        DESCRIPTION = Model.FormInput.Description
                    });

                    var toleranceUnitList = new List<string>();

                    foreach (var toleranceUnitString in Model.FormInput.ToleranceUnitStringList)
                    {
                        string[] temp = toleranceUnitString.Split(Define.Seperators, StringSplitOptions.None);

                        var toleranceUnitUniqueID = temp[0];
                        var description = temp[1];
                        var rate = decimal.Parse(temp[2]);

                        if (string.IsNullOrEmpty(toleranceUnitUniqueID))
                        {
                            toleranceUnitUniqueID = Guid.NewGuid().ToString();
                        }

                        db.QA_TOLERANCEUNIT.Add(new QA_TOLERANCEUNIT()
                        {
                            UNIQUEID = toleranceUnitUniqueID,
                            UNITUNIQUEID = unitUniqueID,
                            DESCRIPTION = description,
                            RATE = rate
                        });

                        toleranceUnitList.Add(description);
                    }

                    if (!toleranceUnitList.Any(x => x == Model.FormInput.Description))
                    {
                        db.QA_TOLERANCEUNIT.Add(new QA_TOLERANCEUNIT()
                        {
                            UNIQUEID = Guid.NewGuid().ToString(),
                            UNITUNIQUEID = unitUniqueID,
                            DESCRIPTION = Model.FormInput.Description,
                            RATE = 1
                        });
                    }

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.Unit, Resources.Resource.Success));
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
                    var unit = db.QA_UNIT.First(x => x.UNIQUEID == UniqueID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = unit.UNIQUEID,
                        FormInput = new FormInput()
                        {
                            Description = unit.DESCRIPTION
                        },
                        ToleranceUnitList = db.QA_TOLERANCEUNIT.Where(x => x.UNITUNIQUEID == unit.UNIQUEID).Select(x => new ToleranceUnitModel
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION,
                            Rate = x.RATE.Value
                        }).OrderBy(x => x.Rate).ToList()
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
#if !DEBUG
                using (TransactionScope trans = new TransactionScope())
                {
#endif
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var unit = db.QA_UNIT.First(x => x.UNIQUEID == Model.UniqueID);

                        unit.DESCRIPTION = Model.FormInput.Description;

                        db.SaveChanges();

                        db.QA_TOLERANCEUNIT.RemoveRange(db.QA_TOLERANCEUNIT.Where(x => x.UNITUNIQUEID == Model.UniqueID).ToList());

                        db.SaveChanges();

                        foreach (var toleranceUnitString in Model.FormInput.ToleranceUnitStringList)
                        {
                            string[] temp = toleranceUnitString.Split(Define.Seperators, StringSplitOptions.None);

                            var uniqueID = temp[0];
                            var description = temp[1];
                            var rate = decimal.Parse(temp[2]);

                            if (string.IsNullOrEmpty(uniqueID))
                            {
                                uniqueID = Guid.NewGuid().ToString();
                            }

                            db.QA_TOLERANCEUNIT.Add(new QA_TOLERANCEUNIT()
                            {
                                UNIQUEID = uniqueID,
                                UNITUNIQUEID = unit.UNIQUEID,
                                DESCRIPTION = description,
                                RATE = rate
                            });
                        }

                        db.SaveChanges();
                    }
#if !DEBUG
                    trans.Complete();
                }
#endif

                    result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.Unit, Resources.Resource.Success));
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
