using DbEntity.ASE;
using Models.ASE.QS.StationManagement;
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
    public class StationDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = db.QS_STATION.AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.KeyWord))
                    {
                        query = query.Where(x => x.DESCRIPTION.Contains(Parameters.KeyWord));
                    }

                    result.ReturnData(new GridViewModel()
                    {
                        ItemList = query.Select(x => new GridItem()
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
                    var factory = db.QS_STATION.First(x => x.UNIQUEID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = factory.UNIQUEID,
                        Type = factory.TYPE,
                        Description = factory.DESCRIPTION
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
                    var exists = db.QS_STATION.FirstOrDefault(x =>x.TYPE==Model.FormInput.Type&& x.DESCRIPTION == Model.FormInput.Description);

                    if (exists == null)
                    {
                        string uniqueID = Guid.NewGuid().ToString();

                        db.QS_STATION.Add(new QS_STATION()
                        {
                            UNIQUEID = uniqueID,
                            TYPE = Model.FormInput.Type,
                            DESCRIPTION = Model.FormInput.Description
                        });

                        db.SaveChanges();

                        result.ReturnData(uniqueID, "新增受稽站別成功");
                    }
                    else
                    {
                        result.ReturnFailedMessage("受稽站別描述已存在");
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
                    var factory = db.QS_STATION.First(x => x.UNIQUEID == UniqueID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = factory.UNIQUEID,
                        Type = factory.TYPE,
                        FormInput = new FormInput()
                        {
                            Description = factory.DESCRIPTION
                        }
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
                    var factory = db.QS_STATION.First(x => x.UNIQUEID == Model.UniqueID);

                    var exists = db.QS_STATION.FirstOrDefault(x => x.UNIQUEID != factory.UNIQUEID && x.TYPE == factory.TYPE && x.DESCRIPTION == Model.FormInput.Description);

                    if (exists == null)
                    {
                        factory.DESCRIPTION = Model.FormInput.Description;

                        db.SaveChanges();

                        result.ReturnSuccessMessage("編輯受稽站別成功");
                    }
                    else
                    {
                        result.ReturnFailedMessage("受稽站別描述已存在");
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

        public static RequestResult Delete(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    db.QS_STATION.Remove(db.QS_STATION.First(x => x.UNIQUEID == UniqueID));

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage("刪除受稽站別成功");
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
