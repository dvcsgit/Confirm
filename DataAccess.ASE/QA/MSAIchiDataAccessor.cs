using DbEntity.ASE;
using Models.ASE.QA.MSAIchiManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace DataAccess.ASE.QA
{
    public class MSAIchiDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = (from i in db.QA_MSAICHI
                                 join s in db.QA_MSASTATION
                                 on i.STATIONUNIQUEID equals s.UNIQUEID
                                 select new
                                 {
                                     i.UNIQUEID,
                                     i.ID,
                                     i.NAME,
                                     StationName = s.NAME
                                 }).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.KeyWord))
                    {
                        query = query.Where(x =>x.ID.Contains(Parameters.KeyWord)|| x.NAME.Contains(Parameters.KeyWord));
                    }

                    result.ReturnData(new GridViewModel()
                    {
                        ItemList = query.Select(x => new GridItem()
                        {
                            UniqueID = x.UNIQUEID,
                            Station=x.StationName,
                            ID=x.ID,
                            Name = x.NAME
                        }).OrderBy(x => x.Station).ThenBy(x => x.ID).ThenBy(x => x.Name).ToList()
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
                    var ichi = (from i in db.QA_MSAICHI
                                join s in db.QA_MSASTATION
                                on i.STATIONUNIQUEID equals s.UNIQUEID
                                where i.UNIQUEID == UniqueID
                                select new
                                {
                                    UniqueID=i.UNIQUEID,
                                    Station = s.NAME,
                                    ID = i.ID,
                                    Name = i.NAME
                                }).First();

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = ichi.UniqueID,
                        Station = ichi.Station,
                        ID=ichi.ID,
                        Name = ichi.Name
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

            try {
                var model = new CreateFormModel()
                {
                    StationSelectItemList = new List<SelectListItem>() { 
                    Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                    }
                };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    model.StationSelectItemList.AddRange(db.QA_MSASTATION.OrderBy(x => x.NAME).ToList().Select(x => new SelectListItem { 
                     Value=x.UNIQUEID,
                     Text=x.NAME
                    }).ToList());
                }

                result.ReturnData(model);
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
                    var exists = db.QA_MSAICHI.FirstOrDefault(x => x.STATIONUNIQUEID == Model.FormInput.StationUniqueID && x.ID == Model.FormInput.ID && x.NAME == Model.FormInput.Name);

                    if (exists == null)
                    {
                        string uniqueID = Guid.NewGuid().ToString();

                        db.QA_MSAICHI.Add(new QA_MSAICHI()
                        {
                            UNIQUEID = uniqueID,
                            STATIONUNIQUEID = Model.FormInput.StationUniqueID,
                            ID = Model.FormInput.ID,
                            NAME = Model.FormInput.Name
                        });

                        db.SaveChanges();

                        result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, "MSA儀器", Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", "儀器代號或名稱", Resources.Resource.Exists));
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
                    var ichi = db.QA_MSAICHI.First(x => x.UNIQUEID == UniqueID);

                    var model = new EditFormModel()
                    {
                        UniqueID = ichi.UNIQUEID,
                        StationSelectItemList = new List<SelectListItem>(){
                          Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                         },
                        FormInput = new FormInput()
                        {
                            StationUniqueID = ichi.STATIONUNIQUEID,
                            ID = ichi.ID,
                            Name = ichi.NAME
                        }
                    };

                    model.StationSelectItemList.AddRange(db.QA_MSASTATION.OrderBy(x => x.NAME).ToList().Select(x => new SelectListItem
                    {
                        Value = x.UNIQUEID,
                        Text = x.NAME
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

        public static RequestResult Edit(EditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var ichi = db.QA_MSAICHI.First(x => x.UNIQUEID == Model.UniqueID);

                    var exists = db.QA_MSAICHI.FirstOrDefault(x => x.UNIQUEID != ichi.UNIQUEID && x.STATIONUNIQUEID == Model.FormInput.StationUniqueID && x.ID == Model.FormInput.ID && x.NAME == Model.FormInput.Name);

                    if (exists == null)
                    {
                        ichi.STATIONUNIQUEID = Model.FormInput.StationUniqueID;
                        ichi.ID = Model.FormInput.ID;
                        ichi.NAME = Model.FormInput.Name;

                        db.SaveChanges();

                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, "MSA儀器", Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", "儀器代號或名稱", Resources.Resource.Exists));
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
