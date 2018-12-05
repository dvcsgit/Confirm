using System;
using System.Linq;
using System.Reflection;
using Utility;
using Utility.Models;
using DbEntity.ASE;
using Models.EquipmentMaintenance.OverTimeReasonManagement;

namespace DataAccess.ASE
{
    public class OverTimeReasonDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = db.OVERTIMEREASON.AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.KeyWord))
                    {
                        query = query.Where(x => x.ID.Contains(Parameters.KeyWord) || x.DESCRIPTION.Contains(Parameters.KeyWord));
                    }

                    result.ReturnData(new GridViewModel()
                    {
                        ItemList = query.Select(x => new GridItem()
                        {
                            UniqueID = x.UNIQUEID,
                            ID = x.ID,
                            Description = x.DESCRIPTION
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var reason = db.OVERTIMEREASON.First(x => x.UNIQUEID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = reason.UNIQUEID,
                        ID = reason.ID,
                        Description = reason.DESCRIPTION
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
                    var exists = db.OVERTIMEREASON.FirstOrDefault(x => x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
                        string uniqueID = Guid.NewGuid().ToString();

                        db.OVERTIMEREASON.Add(new OVERTIMEREASON()
                        {
                            UNIQUEID = uniqueID,
                            ID = Model.FormInput.ID,
                            DESCRIPTION = Model.FormInput.Description,
                            LASTMODIFYTIME = DateTime.Now
                        });

                        db.SaveChanges();

                        result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.OverTimeReason, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.AbnormalReasonID, Resources.Resource.Exists));
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
                    var reason = db.OVERTIMEREASON.First(x => x.UNIQUEID == UniqueID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = reason.UNIQUEID,
                        FormInput = new FormInput()
                        {
                            ID = reason.ID,
                            Description = reason.DESCRIPTION
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
                    var reason = db.OVERTIMEREASON.First(x => x.UNIQUEID == Model.UniqueID);

                    var exists = db.OVERTIMEREASON.FirstOrDefault(x => x.UNIQUEID != reason.UNIQUEID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
                        reason.DESCRIPTION = Model.FormInput.Description;
                        reason.LASTMODIFYTIME = DateTime.Now;

                        db.SaveChanges();

                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.OverTimeReason, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.AbnormalReasonID, Resources.Resource.Exists));
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
                    db.OVERTIMEREASON.Remove(db.OVERTIMEREASON.First(x => x.UNIQUEID == UniqueID));

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.OverTimeReason, Resources.Resource.Success));
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
