﻿using System;
using System.Linq;
using System.Reflection;
using Utility;
using Utility.Models;
using DbEntity.MSSQL.GuardPatrol;
using Models.GuardPatrol.UnPatrolReasonManagement;

namespace DataAccess.GuardPatrol
{
    public class UnPatrolReasonDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (GDbEntities db = new GDbEntities())
                {
                    var query = db.UnPatrolReason.AsQueryable();

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
                using (GDbEntities db = new GDbEntities())
                {
                    var reason = db.UnPatrolReason.First(x => x.UniqueID == UniqueID);

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
                using (GDbEntities db = new GDbEntities())
                {
                    var exists = db.UnPatrolReason.FirstOrDefault(x => x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
                        string uniqueID = Guid.NewGuid().ToString();

                        db.UnPatrolReason.Add(new UnPatrolReason()
                        {
                            UniqueID = uniqueID,
                            ID = Model.FormInput.ID,
                            Description = Model.FormInput.Description,
                            LastModifyTime = DateTime.Now
                        });

                        db.SaveChanges();

                        result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.UnPatrolReason, Resources.Resource.Success));
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
                using (GDbEntities db = new GDbEntities())
                {
                    var reason = db.UnPatrolReason.First(x => x.UniqueID == UniqueID);

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
                using (GDbEntities db = new GDbEntities())
                {
                    var reason = db.UnPatrolReason.First(x => x.UniqueID == Model.UniqueID);

                    var exists = db.UnPatrolReason.FirstOrDefault(x => x.UniqueID != reason.UniqueID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
                        reason.Description = Model.FormInput.Description;
                        reason.LastModifyTime = DateTime.Now;

                        db.SaveChanges();

                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.UnPatrolReason, Resources.Resource.Success));
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
                using (GDbEntities db = new GDbEntities())
                {
                    db.UnPatrolReason.Remove(db.UnPatrolReason.First(x => x.UniqueID == UniqueID));

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.UnPatrolReason, Resources.Resource.Success));
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
