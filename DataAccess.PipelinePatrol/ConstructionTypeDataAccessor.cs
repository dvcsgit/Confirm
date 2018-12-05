using System;
using System.Linq;
using System.Reflection;
using Utility;
using Utility.Models;
using DbEntity.MSSQL.PipelinePatrol;
using Models.PipelinePatrol.ConstructionTypeManagement;

namespace DataAccess.PipelinePatrol
{
    public class ConstructionTypeDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    var query = db.ConstructionType.AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.KeyWord))
                    {
                        query = query.Where(x => x.ID.Contains(Parameters.KeyWord) || x.Description.Contains(Parameters.KeyWord));
                    }

                    result.ReturnData(new GridViewModel()
                    {
                        ItemList = query.Select(x => new GridItem()
                        {
                            UniqueID = x.UniqueID,
                            ID = x.ID,
                            Description = x.Description
                        }).OrderBy(x => x.ID).ToList()
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
                using (PDbEntities db = new PDbEntities())
                {
                    var type = db.ConstructionType.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = type.UniqueID,
                        ID = type.ID,
                        Description = type.Description
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
                using (PDbEntities db = new PDbEntities())
                {
                    var exists = db.ConstructionType.FirstOrDefault(x => x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
                        string uniqueID = Guid.NewGuid().ToString();

                        db.ConstructionType.Add(new ConstructionType()
                        {
                            UniqueID = uniqueID,
                            ID = Model.FormInput.ID,
                            Description = Model.FormInput.Description
                        });

                        db.SaveChanges();

                        result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.ConstructionType, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.ConstructionTypeID, Resources.Resource.Exists));
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
                using (PDbEntities db = new PDbEntities())
                {
                    var type = db.ConstructionType.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = type.UniqueID,
                        FormInput = new FormInput()
                        {
                            ID = type.ID,
                            Description = type.Description
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
                using (PDbEntities db = new PDbEntities())
                {
                    var type = db.ConstructionType.First(x => x.UniqueID == Model.UniqueID);

                    var exists = db.ConstructionType.FirstOrDefault(x => x.UniqueID != type.UniqueID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
                        type.Description = Model.FormInput.Description;

                        db.SaveChanges();

                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.ConstructionType, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.ConstructionTypeID, Resources.Resource.Exists));
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
                using (PDbEntities db = new PDbEntities())
                {
                    db.ConstructionType.Remove(db.ConstructionType.First(x => x.UniqueID == UniqueID));

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.ConstructionType, Resources.Resource.Success));
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
