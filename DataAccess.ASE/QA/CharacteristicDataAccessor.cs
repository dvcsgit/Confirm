using DbEntity.ASE;
using Models.ASE.QA.CharacteristicManagement;
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
    public class CharacteristicDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = db.QA_CHARACTERISTIC.AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.TYPE.Contains(Parameters.Keyword) || x.DESCRIPTION.Contains(Parameters.Keyword));
                    }

                    result.ReturnData(new GridViewModel()
                    {
                        ItemList = query.ToList().Select(x => new GridItem()
                        {
                            UniqueID = x.UNIQUEID,
                            Type = x.TYPE,
                            Description = x.DESCRIPTION
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

        public static RequestResult GetDetailViewModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var characteristic = db.QA_CHARACTERISTIC.First(x => x.UNIQUEID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = characteristic.UNIQUEID,
                        Type = characteristic.TYPE,
                        Description = characteristic.DESCRIPTION,
                        UnitList = (from x in db.QA_CHARACTERISTICUNIT
                                    join u in db.QA_UNIT
                                    on x.UNITUNIQUEID equals u.UNIQUEID
                                    where x.CHARACTERISTICUNIQUEID == characteristic.UNIQUEID
                                    select u.DESCRIPTION).OrderBy(x => x).ToList()
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
                        TypeSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne),
                            new SelectListItem()
                            {
                                Text = Resources.Resource.Create + "...",
                                Value = Define.NEW
                            }
                        },
                        UnitList = db.QA_UNIT.ToList().Select(x => new UnitModel
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION
                        }).OrderBy(x => x.Description).ToList()
                    };

                    model.TypeSelectItemList.AddRange(db.QA_CHARACTERISTIC.Select(x => x.TYPE).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
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
                if (Model.FormInput.Type == Define.OTHER || Model.FormInput.Type == Define.NEW)
                {
                    result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.Unsupported, "量性"));
                }
                else
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var exists = db.QA_CHARACTERISTIC.FirstOrDefault(x => x.TYPE == Model.FormInput.Type && x.DESCRIPTION == Model.FormInput.Description);

                        if (exists == null)
                        {
                            string uniqueID = Guid.NewGuid().ToString();

                            db.QA_CHARACTERISTIC.Add(new QA_CHARACTERISTIC()
                            {
                                UNIQUEID = uniqueID,
                               TYPE = Model.FormInput.Type,
                                DESCRIPTION = Model.FormInput.Description
                            });

                            db.QA_CHARACTERISTICUNIT.AddRange(Model.FormInput.UnitList.Select(x => new QA_CHARACTERISTICUNIT
                            {
                                 CHARACTERISTICUNIQUEID = uniqueID,
                                 UNITUNIQUEID = x
                            }).ToList());

                            db.SaveChanges();

                            result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, "量測特性", Resources.Resource.Success));
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1}", "量測特性名稱", Resources.Resource.Exists));
                        }
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
                    var characteristic = db.QA_CHARACTERISTIC.First(x => x.UNIQUEID == UniqueID);

                    var model = new EditFormModel()
                    {
                        UniqueID = characteristic.UNIQUEID,
                        TypeSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne),
                            new SelectListItem()
                            {
                                Text = Resources.Resource.Create + "...",
                                Value = Define.NEW
                            }
                        },
                        CharacteristicUnitList = db.QA_CHARACTERISTICUNIT.Where(x => x.CHARACTERISTICUNIQUEID == characteristic.UNIQUEID).Select(x => x.UNITUNIQUEID).ToList(),
                        UnitList = db.QA_UNIT.ToList().Select(x => new UnitModel
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION
                        }).OrderBy(x => x.Description).ToList(),
                        FormInput = new FormInput()
                        {
                            Type = characteristic.TYPE,
                            Description = characteristic.DESCRIPTION
                        }
                    };

                    model.TypeSelectItemList.AddRange(db.QA_CHARACTERISTIC.Select(x => x.TYPE).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    var type = model.TypeSelectItemList.FirstOrDefault(x => x.Value == characteristic.TYPE);

                    if (type != null)
                    {
                        type.Selected = true;
                    }

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
                if (Model.FormInput.Type == Define.OTHER || Model.FormInput.Type == Define.NEW)
                {
                    result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.Unsupported, "量性"));
                }
                else
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var characteristic = db.QA_CHARACTERISTIC.First(x => x.UNIQUEID == Model.UniqueID);

                        var exists = db.QA_CHARACTERISTIC.FirstOrDefault(x => x.UNIQUEID != characteristic.UNIQUEID && x.TYPE == Model.FormInput.Type && x.DESCRIPTION == Model.FormInput.Description);

                        if (exists == null)
                        {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                            #region 
                            characteristic.TYPE = Model.FormInput.Type;
                            characteristic.DESCRIPTION = Model.FormInput.Description;

                            db.SaveChanges();
                            #endregion

                            #region 
                            #region Delete
                            db.QA_CHARACTERISTICUNIT.RemoveRange(db.QA_CHARACTERISTICUNIT.Where(x => x.CHARACTERISTICUNIQUEID == Model.UniqueID).ToList());

                            db.SaveChanges();
                            #endregion

                            #region Insert
                            db.QA_CHARACTERISTICUNIT.AddRange(Model.FormInput.UnitList.Select(x => new QA_CHARACTERISTICUNIT
                            {
                                CHARACTERISTICUNIQUEID = characteristic.UNIQUEID,
                                UNITUNIQUEID = x
                            }).ToList());

                            db.SaveChanges();
                            #endregion
                            #endregion
#if !DEBUG
                        trans.Complete();
                    }
#endif
                            result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, "量測特性", Resources.Resource.Success));
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1}", "量測特性名稱", Resources.Resource.Exists));
                        }
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
