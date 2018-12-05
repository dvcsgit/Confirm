using DbEntity.ASE;
using Models.ASE.QA.MSACharacteristicManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace DataAccess.ASE.QA
{
    public class MSACharacteristicDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = (from c in db.QA_MSACHARACTERISTICS
                                 join i in db.QA_MSAICHI
                                 on c.ICHIUNIQUEID equals i.UNIQUEID
                                 join s in db.QA_MSASTATION
                                 on i.STATIONUNIQUEID equals s.UNIQUEID
                                 select new
                                 {
                                     UniqueID = c.UNIQUEID,
                                     Station = s.NAME,
                                     Ichi = i.NAME,
                                     Name = c.NAME
                                 }).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.Name.Contains(Parameters.Keyword));
                    }

                    result.ReturnData(new GridViewModel()
                    {
                        ItemList = query.ToList().Select(x => new GridItem()
                        {
                            UniqueID = x.UniqueID,
                            Station = x.Station,
                            Ichi = x.Ichi,
                            Name = x.Name
                        }).OrderBy(x => x.Station).ThenBy(x => x.Ichi).ThenBy(x => x.Name).ToList()
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
                    var characteristic = (from c in db.QA_MSACHARACTERISTICS
                                          join i in db.QA_MSAICHI
                                          on c.ICHIUNIQUEID equals i.UNIQUEID
                                          join s in db.QA_MSASTATION
                                          on i.STATIONUNIQUEID equals s.UNIQUEID
                                          where c.UNIQUEID == UniqueID
                                          select new
                                          {
                                              UniqueID = c.UNIQUEID,
                                              Station = s.NAME,
                                              Ichi = i.NAME,
                                              Name = c.NAME
                                          }).First();

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = characteristic.UniqueID,
                        Station = characteristic.Station,
                        Ichi = characteristic.Ichi,
                        Name = characteristic.Name,
                        UnitList = db.QA_MSAUNIT.Where(x => x.CHARACTERISTICSUNIQUEID == characteristic.UniqueID).ToList().Select(x => new UnitModel
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

        public static RequestResult GetCreateFormModel()
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var model = new CreateFormModel()
                    {
                        IchiSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        }
                    };

                    model.IchiSelectItemList.AddRange((from i in db.QA_MSAICHI
                                                       join s in db.QA_MSASTATION
                                                       on i.STATIONUNIQUEID equals s.UNIQUEID
                                                       select new
                                                       {
                                                           Station = s.NAME,
                                                           UniqueID = i.UNIQUEID,
                                                           ID = i.ID,
                                                           Name = i.NAME
                                                       }).OrderBy(x => x.Station).ThenBy(x => x.ID).ThenBy(x => x.Name).ToList().Select(x => new SelectListItem
                                                       {
                                                           Value = x.UniqueID,
                                                           Text = string.Format("[{0}]{1}/{2}", x.Station, x.ID, x.Name)
                                                       }).ToList());

                    result.ReturnData(model);
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
                    var exists = db.QA_MSACHARACTERISTICS.FirstOrDefault(x => x.ICHIUNIQUEID == Model.FormInput.IchiUniqueID && x.NAME == Model.FormInput.Name);

                    if (exists == null)
                    {
                        string uniqueID = Guid.NewGuid().ToString();

                        db.QA_MSACHARACTERISTICS.Add(new QA_MSACHARACTERISTICS()
                        {
                            UNIQUEID = uniqueID,
                             ICHIUNIQUEID=Model.FormInput.IchiUniqueID,
                            NAME = Model.FormInput.Name
                        });

                        db.QA_MSAUNIT.AddRange(Model.FormInput.UnitList.Select(x => new QA_MSAUNIT
                        {
                            CHARACTERISTICSUNIQUEID = uniqueID,
                            UNIQUEID = Guid.NewGuid().ToString(),
                            DESCRIPTION = x
                        }).ToList());

                        db.SaveChanges();

                        result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, "量測特性", Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", "量測特性", Resources.Resource.Exists));
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

        public static RequestResult GetEditFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var characteristic = (from c in db.QA_MSACHARACTERISTICS
                                          join i in db.QA_MSAICHI
                                          on c.ICHIUNIQUEID equals i.UNIQUEID
                                          join s in db.QA_MSASTATION
                                          on i.STATIONUNIQUEID equals s.UNIQUEID
                                          where c.UNIQUEID == UniqueID
                                          select new
                                          {
                                              UniqueID = c.UNIQUEID,
                                              Station = s.NAME,
                                              Ichi = i.NAME,
                                              Name = c.NAME
                                          }).First();

                    var model = new EditFormModel()
                    {
                        UniqueID = characteristic.UniqueID,
                        Station = characteristic.Station,
                        Ichi = characteristic.Ichi,             
                        FormInput = new FormInput()
                        {
                            Name = characteristic.Name
                        },
                        CharacteristicUnitList = db.QA_MSAUNIT.Where(x => x.CHARACTERISTICSUNIQUEID == characteristic.UniqueID).Select(x => new UnitModel
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION
                        }).OrderBy(x => x.Description).ToList()
                    };

                    result.ReturnData(model);
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
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                        var characteristic = db.QA_MSACHARACTERISTICS.First(x => x.UNIQUEID == Model.UniqueID);

                        var exists = db.QA_MSACHARACTERISTICS.FirstOrDefault(x => x.UNIQUEID != characteristic.UNIQUEID && x.ICHIUNIQUEID == characteristic.ICHIUNIQUEID && x.NAME == Model.FormInput.Name);

                        if (exists == null)
                        {
                            characteristic.NAME = Model.FormInput.Name;

                            db.SaveChanges();

                            var unitList = db.QA_MSAUNIT.Where(x => x.CHARACTERISTICSUNIQUEID == characteristic.UNIQUEID).ToList();

                            db.QA_MSAUNIT.RemoveRange(db.QA_MSAUNIT.Where(x => x.CHARACTERISTICSUNIQUEID == characteristic.UNIQUEID).ToList());

                            db.SaveChanges();

                            foreach (var unit in Model.FormInput.UnitList)
                            {
                                var u = unitList.FirstOrDefault(x => x.DESCRIPTION == unit);

                                if (u != null)
                                {
                                    db.QA_MSAUNIT.Add(u);
                                }
                                else
                                {
                                    db.QA_MSAUNIT.Add(new QA_MSAUNIT()
                                    {
                                        UNIQUEID = Guid.NewGuid().ToString(),
                                        CHARACTERISTICSUNIQUEID = characteristic.UNIQUEID,
                                        DESCRIPTION = unit
                                    });
                                }
                            }

                            db.SaveChanges();

#if !DEBUG
                            trans.Complete();
#endif

                            result.ReturnData(characteristic.UNIQUEID, string.Format("{0} {1} {2}", Resources.Resource.Edit, "量測特性", Resources.Resource.Success));
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1}", "量測特性", Resources.Resource.Exists));
                        }
#if !DEBUG
                    }
#endif
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
