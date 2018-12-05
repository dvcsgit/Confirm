using DbEntity.ASE;
using Models.ASE.QS.FactoryManagement;
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
    public class FactoryDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = db.QS_FACTORY.AsQueryable();

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
                    var factory = db.QS_FACTORY.First(x => x.UNIQUEID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = factory.UNIQUEID,
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
                    var exists = db.QS_FACTORY.FirstOrDefault(x => x.DESCRIPTION == Model.FormInput.Description);

                    if (exists == null)
                    {
                        string uniqueID = Guid.NewGuid().ToString();

                        db.QS_FACTORY.Add(new QS_FACTORY()
                        {
                            UNIQUEID = uniqueID,
                            DESCRIPTION = Model.FormInput.Description
                        });

                        db.SaveChanges();

                        result.ReturnData(uniqueID, "新增受稽廠別成功");
                    }
                    else
                    {
                        result.ReturnFailedMessage("受稽廠別描述已存在");
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
                    var factory = db.QS_FACTORY.First(x => x.UNIQUEID == UniqueID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = factory.UNIQUEID,
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
                    var factory = db.QS_FACTORY.First(x => x.UNIQUEID == Model.UniqueID);

                    var exists = db.QS_FACTORY.FirstOrDefault(x => x.UNIQUEID != factory.UNIQUEID && x.DESCRIPTION == Model.FormInput.Description);

                    if (exists == null)
                    {
                        factory.DESCRIPTION = Model.FormInput.Description;

                        db.SaveChanges();

                        result.ReturnSuccessMessage("編輯受稽廠別成功");
                    }
                    else
                    {
                        result.ReturnFailedMessage("受稽廠別描述已存在");
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
                    db.QS_FACTORY.Remove(db.QS_FACTORY.First(x => x.UNIQUEID == UniqueID));

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage("刪除受稽廠別成功");
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
