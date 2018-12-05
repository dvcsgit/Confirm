using DbEntity.ASE;
using Models.ASE.QA.LabManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace DataAccess.ASE.QA
{
    public class LabDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = db.QA_LAB.AsQueryable();

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
                    var lab = db.QA_LAB.First(x => x.UNIQUEID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = lab.UNIQUEID,
                        Description = lab.DESCRIPTION
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
                    var exists = db.QA_LAB.FirstOrDefault(x => x.DESCRIPTION == Model.FormInput.Description);

                    if (exists == null)
                    {
                        var seq = 1;

                        var query = db.QA_LAB.OrderByDescending(x => x.UNIQUEID).ToList();

                        if (query.Count > 0)
                        {
                            seq = int.Parse(query.First().UNIQUEID) + 1;
                        }

                        string uniqueID = seq.ToString().PadLeft(32, '0');

                        db.QA_LAB.Add(new QA_LAB()
                        {
                            UNIQUEID = uniqueID,
                            DESCRIPTION = Model.FormInput.Description
                        });

                        db.SaveChanges();

                        result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, "外校實驗室", Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", "實驗室名稱", Resources.Resource.Exists));
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
                    var lab = db.QA_LAB.First(x => x.UNIQUEID == UniqueID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = lab.UNIQUEID,
                        FormInput = new FormInput()
                        {
                            Description = lab.DESCRIPTION
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
                    var lab = db.QA_LAB.First(x => x.UNIQUEID == Model.UniqueID);

                    var exists = db.QA_LAB.FirstOrDefault(x => x.UNIQUEID != lab.UNIQUEID && x.DESCRIPTION == Model.FormInput.Description);

                    if (exists == null)
                    {
                        lab.DESCRIPTION = Model.FormInput.Description;

                        db.SaveChanges();

                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, "外校實驗室", Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", "實驗室名稱", Resources.Resource.Exists));
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
