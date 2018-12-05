using System;
using System.Linq;
using System.Reflection;
using Utility;
using Utility.Models;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.EquipmentMaintenance.TimeSpanAbnormalReasonManagement;

namespace DataAccess.EquipmentMaintenance
{
    public class TimeSpanAbnormalReasonDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var query = db.TimeSpanAbnormalReason.AsQueryable();

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
                using (EDbEntities db = new EDbEntities())
                {
                    var reason = db.TimeSpanAbnormalReason.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = reason.UniqueID,
                        ID = reason.ID,
                        Description = reason.Description
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
                using (EDbEntities db = new EDbEntities())
                {
                    var exists = db.TimeSpanAbnormalReason.FirstOrDefault(x => x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
                        string uniqueID = Guid.NewGuid().ToString();

                        db.TimeSpanAbnormalReason.Add(new TimeSpanAbnormalReason()
                        {
                            UniqueID = uniqueID,
                            ID = Model.FormInput.ID,
                            Description = Model.FormInput.Description,
                            LastModifyTime = DateTime.Now
                        });

                        db.SaveChanges();

                        result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.TimeSpanAbnormalReason, Resources.Resource.Success));
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
                using (EDbEntities db = new EDbEntities())
                {
                    var reason = db.TimeSpanAbnormalReason.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = reason.UniqueID,
                        FormInput = new FormInput()
                        {
                            ID = reason.ID,
                            Description = reason.Description
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
                using (EDbEntities db = new EDbEntities())
                {
                    var reason = db.TimeSpanAbnormalReason.First(x => x.UniqueID == Model.UniqueID);

                    var exists = db.TimeSpanAbnormalReason.FirstOrDefault(x => x.UniqueID != reason.UniqueID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
                        reason.Description = Model.FormInput.Description;
                        reason.LastModifyTime = DateTime.Now;

                        db.SaveChanges();

                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.TimeSpanAbnormalReason, Resources.Resource.Success));
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
                using (EDbEntities db = new EDbEntities())
                {
                    db.TimeSpanAbnormalReason.Remove(db.TimeSpanAbnormalReason.First(x => x.UniqueID == UniqueID));

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.TimeSpanAbnormalReason, Resources.Resource.Success));
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
