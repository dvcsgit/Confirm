using DbEntity.ASE;
using Models.ASE.QA.IchiManagement;
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
    public class IchiDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = db.QA_ICHI.AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.TYPE.Contains(Parameters.Keyword) || x.NAME.Contains(Parameters.Keyword));
                    }

                    result.ReturnData(new GridViewModel()
                    {
                        ItemList = query.ToList().Select(x => new GridItem()
                        {
                            UniqueID = x.UNIQUEID,
                            Type = x.TYPE,
                            Name = x.NAME
                        }).OrderBy(x => x.Type).ThenBy(x => x.Name).ToList()
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
                    var ichi = db.QA_ICHI.First(x => x.UNIQUEID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = ichi.UNIQUEID,
                        Type = ichi.TYPE,
                        Name = ichi.NAME,
                        CharacteristicList = (from x in db.QA_ICHICHARACTERISTIC
                                              join c in db.QA_CHARACTERISTIC
                                              on x.CHARACTERISTICUNIQUEID equals c.UNIQUEID
                                              where x.ICHIUNIQUEID == ichi.UNIQUEID
                                              select c).ToList().Select(x => new CharacteristicModel
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
                        CharacteristicList = db.QA_CHARACTERISTIC.ToList().Select(x => new CharacteristicModel    
                        {
                            UniqueID = x.UNIQUEID,
                            Type = x.TYPE,
                            Description = x.DESCRIPTION
                        }).OrderBy(x => x.Type).ThenBy(x=>x.Description).ToList()
                    };

                    model.TypeSelectItemList.AddRange(db.QA_ICHI.Select(x => x.TYPE).Distinct().OrderBy(x => x).Select(x => new SelectListItem
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
                    result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.Unsupported, "類別"));
                }
                else
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var exists = db.QA_ICHI.FirstOrDefault(x => x.TYPE == Model.FormInput.Type && x.NAME == Model.FormInput.Name);

                        if (exists == null)
                        {
                            string uniqueID = Guid.NewGuid().ToString();

                            db.QA_ICHI.Add(new QA_ICHI()
                            {
                                UNIQUEID = uniqueID,
                                TYPE = Model.FormInput.Type,
                                NAME = Model.FormInput.Name
                            });

                            db.QA_ICHICHARACTERISTIC.AddRange(Model.FormInput.CharacteristicList.Select(x => new QA_ICHICHARACTERISTIC
                            {
                                ICHIUNIQUEID = uniqueID,
                                CHARACTERISTICUNIQUEID = x
                            }).ToList());

                            db.SaveChanges();

                            result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, "儀器", Resources.Resource.Success));
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1}", "儀器名稱", Resources.Resource.Exists));
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
                    var ichi = db.QA_ICHI.First(x => x.UNIQUEID == UniqueID);

                    var model = new EditFormModel()
                    {
                        UniqueID = ichi.UNIQUEID,
                        TypeSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne),
                            new SelectListItem()
                            {
                                Text = Resources.Resource.Create + "...",
                                Value = Define.NEW
                            }
                        },
                        IchiCharacteristicList = db.QA_ICHICHARACTERISTIC.Where(x => x.ICHIUNIQUEID == ichi.UNIQUEID).Select(x => x.CHARACTERISTICUNIQUEID).ToList(),
                        CharacteristicList = db.QA_CHARACTERISTIC.ToList().Select(x => new CharacteristicModel
                       {
                           UniqueID = x.UNIQUEID,
                           Type = x.TYPE,
                           Description = x.DESCRIPTION
                       }).OrderBy(x => x.Type).ThenBy(x=>x.Description).ToList(),
                        FormInput = new FormInput()
                        {
                            Type = ichi.TYPE,
                            Name = ichi.NAME
                        }
                    };

                    model.TypeSelectItemList.AddRange(db.QA_ICHI.Select(x => x.TYPE).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    var type = model.TypeSelectItemList.FirstOrDefault(x => x.Value == ichi.TYPE);

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
                    result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.Unsupported, "類別"));
                }
                else
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var ichi = db.QA_ICHI.First(x => x.UNIQUEID == Model.UniqueID);

                        var exists = db.QA_ICHI.FirstOrDefault(x => x.UNIQUEID != ichi.UNIQUEID && x.TYPE == Model.FormInput.Type && x.NAME == Model.FormInput.Name);

                        if (exists == null)
                        {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                            #region
                            ichi.TYPE = Model.FormInput.Type;
                            ichi.NAME = Model.FormInput.Name;

                            db.SaveChanges();
                            #endregion

                            #region
                            #region Delete
                            db.QA_ICHICHARACTERISTIC.RemoveRange(db.QA_ICHICHARACTERISTIC.Where(x => x.ICHIUNIQUEID == Model.UniqueID).ToList());

                            db.SaveChanges();
                            #endregion

                            #region Insert
                            db.QA_ICHICHARACTERISTIC.AddRange(Model.FormInput.CharacteristicList.Select(x => new QA_ICHICHARACTERISTIC
                            {
                                ICHIUNIQUEID = ichi.UNIQUEID,
                                CHARACTERISTICUNIQUEID = x
                            }).ToList());

                            db.SaveChanges();
                            #endregion
                            #endregion
#if !DEBUG
                        trans.Complete();
                    }
#endif
                            result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, "儀器", Resources.Resource.Success));
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1}", "儀器名稱", Resources.Resource.Exists));
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
