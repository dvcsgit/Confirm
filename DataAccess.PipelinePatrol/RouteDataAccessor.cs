using System;
using System.Linq;
using System.Reflection;
#if !DEBUG
using System.Transactions;
#endif
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.MSSQL;
using DbEntity.MSSQL.PipelinePatrol;
using Models.Shared;
using Models.Authenticated;
using Models.PipelinePatrol.RouteManagement;

namespace DataAccess.PipelinePatrol
{
    public class RouteDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.Route.Where(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.ID.Contains(Parameters.Keyword) || x.Name.Contains(Parameters.Keyword));
                    }

                    var organization = OrganizationDataAccessor.GetOrganization(Parameters.OrganizationUniqueID);

                    result.ReturnData(new GridViewModel()
                    {
                        Permission = Account.OrganizationPermission(Parameters.OrganizationUniqueID),
                        OrganizationUniqueID = Parameters.OrganizationUniqueID,
                        OrganizationDescription = organization.Description,
                        FullOrganizationDescription = organization.FullDescription,
                        ItemList = query.ToList().Select(x => new GridItem
                        {
                            UniqueID = x.UniqueID,
                            Permission = Account.OrganizationPermission(x.OrganizationUniqueID),
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.OrganizationUniqueID),
                            ID = x.ID,
                            Name = x.Name
                        }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.ID).ToList()
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

        public static RequestResult GetDetailViewModel(string UniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new DetailViewModel();
                
                using (PDbEntities db = new PDbEntities())
                {
                    var route = db.Route.First(x => x.UniqueID == UniqueID);

                    model = new DetailViewModel()
                    {
                        UniqueID = route.UniqueID,
                        Permission = Account.OrganizationPermission(route.OrganizationUniqueID),
                        OrganizationUniqueID = route.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(route.OrganizationUniqueID),
                        ID = route.ID,
                        Name = route.Name,
                        Remark = route.Remark,
                        PipelineList = (from x in db.RoutePipeline
                                        join p in db.Pipeline
                                        on x.PipelineUniqueID equals p.UniqueID
                                        where x.RouteUniqueID == route.UniqueID
                                        select new
                                        {
                                            p.OrganizationUniqueID,
                                            p.UniqueID,
                                            p.ID
                                        }).ToList().Select(x => new PipelineModel
                                        {
                                            UniqueID = x.UniqueID,
                                            ID = x.ID,
                                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.OrganizationUniqueID)
                                        }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.ID).ToList()
                    };   
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

        public static RequestResult GetCopyFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new CreateFormModel();

                using (PDbEntities db = new PDbEntities())
                {
                    var route = db.Route.First(x => x.UniqueID == UniqueID);

                    model = new CreateFormModel()
                    {
                        OrganizationUniqueID = route.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(route.OrganizationUniqueID),
                        CheckPointList = (from rc in db.RouteCheckPoint
                                          join pipePoint in db.PipePoint
                                          on rc.PipePointUniqueID equals pipePoint.UniqueID
                                          where rc.RouteUniqueID == route.UniqueID
                                          select new CheckPointModel
                                          {
                                              UniqueID = pipePoint.UniqueID,
                                              PipePointType = pipePoint.PointType,
                                              ID = pipePoint.ID,
                                              Name = pipePoint.Name,
                                              MinTimeSpan = rc.MinTimeSpan,
                                              Seq = rc.Seq,
                                          }).OrderBy(x => x.Seq).ToList(),
                        PipelineList = (from x in db.RoutePipeline
                                        join p in db.Pipeline
                                        on x.PipelineUniqueID equals p.UniqueID
                                        where x.RouteUniqueID == route.UniqueID
                                        select new
                                        {
                                            p.OrganizationUniqueID,
                                            p.UniqueID,
                                            p.ID
                                        }).ToList().Select(x => new PipelineModel
                                        {
                                            UniqueID = x.UniqueID,
                                            ID = x.ID,
                                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.OrganizationUniqueID)
                                        }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.ID).ToList()
                    };

                    foreach (var checkPoint in model.CheckPointList)
                    {
                        var checkPointCheckItemList = (from x in db.PipePointCheckItem
                                                         join c in db.CheckItem
                                                         on x.CheckItemUniqueID equals c.UniqueID
                                                         where x.PipePointUniqueID == checkPoint.UniqueID
                                                         select c).ToList();

                        foreach (var checkItem in checkPointCheckItemList)
                        {
                            var routeCheckPointCheckItem = db.RouteCheckPointCheckItem.FirstOrDefault(x => x.RouteUniqueID == route.UniqueID && x.PipePointUniqueID == checkPoint.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID);

                            checkPoint.CheckItemList.Add(new CheckItemModel()
                            {
                                IsChecked = routeCheckPointCheckItem != null,
                                UniqueID = checkItem.UniqueID,
                                CheckType = checkItem.CheckType,
                                CheckItemID = checkItem.ID,
                                CheckItemDescription = checkItem.Description,
                                Seq = routeCheckPointCheckItem != null ? routeCheckPointCheckItem.Seq : int.MaxValue
                            });
                        }

                        checkPoint.CheckItemList = checkPoint.CheckItemList.OrderBy(x => x.Seq).ToList();
                    }
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
                using (PDbEntities db = new PDbEntities())
                {
                    var exists = db.Route.FirstOrDefault(x => x.OrganizationUniqueID == Model.OrganizationUniqueID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
                        string uniqueID = Guid.NewGuid().ToString();

                        db.Route.Add(new Route()
                        {
                            UniqueID = uniqueID,
                            OrganizationUniqueID = Model.OrganizationUniqueID,
                            ID = Model.FormInput.ID,
                            Name = Model.FormInput.Name,
                             Remark = Model.FormInput.Remark,
                            LastModifyTime = DateTime.Now
                        });

                        db.RoutePipeline.AddRange(Model.PipelineList.Select(x => new RoutePipeline
                        {
                            RouteUniqueID = uniqueID,
                            PipelineUniqueID = x.UniqueID
                        }).ToList());

                        int checkPointSeq = 1;

                        foreach (var checkPoint in Model.CheckPointList)
                        {
                            db.RouteCheckPoint.Add(new RouteCheckPoint()
                            {
                                RouteUniqueID = uniqueID,
                                 PipePointUniqueID = checkPoint.UniqueID,
                                MinTimeSpan = checkPoint.MinTimeSpan,
                                Seq = checkPointSeq
                            });

                            checkPointSeq++;

                            int checkPointCheckItemSeq = 1;

                            foreach (var checkItem in checkPoint.CheckItemList)
                            {
                                if (checkItem.IsChecked)
                                {
                                    db.RouteCheckPointCheckItem.Add(new RouteCheckPointCheckItem
                                    {
                                        RouteUniqueID = uniqueID,
                                        PipePointUniqueID = checkPoint.UniqueID,
                                        CheckItemUniqueID = checkItem.UniqueID,
                                        Seq = checkPointCheckItemSeq
                                    });

                                    checkPointCheckItemSeq++;
                                }
                            }
                        }

                        db.SaveChanges();

                        result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.Route, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.RouteID, Resources.Resource.Exists));
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
                var model = new EditFormModel();

                using (PDbEntities db = new PDbEntities())
                {
                    var route = db.Route.First(x => x.UniqueID == UniqueID);

                    model = new EditFormModel()
                    {
                        UniqueID = route.UniqueID,
                        OrganizationUniqueID = route.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(route.OrganizationUniqueID),
                        FormInput = new FormInput()
                        {
                            ID = route.ID,
                            Name = route.Name,
                             Remark = route.Remark
                        },
                        CheckPointList = (from rc in db.RouteCheckPoint
                                            join pipePoint in db.PipePoint
                                            on rc.PipePointUniqueID equals pipePoint.UniqueID
                                            where rc.RouteUniqueID == route.UniqueID
                                            select new CheckPointModel
                                            {
                                                UniqueID = pipePoint.UniqueID,
                                                PipePointType = pipePoint.PointType,
                                                ID = pipePoint.ID,
                                                Name = pipePoint.Name,
                                                MinTimeSpan = rc.MinTimeSpan,
                                                Seq = rc.Seq
                                            }).OrderBy(x => x.Seq).ToList(),
                        PipelineList = (from x in db.RoutePipeline
                                        join p in db.Pipeline
                                        on x.PipelineUniqueID equals p.UniqueID
                                        where x.RouteUniqueID == route.UniqueID
                                        select new
                                        {
                                            p.OrganizationUniqueID,
                                            p.UniqueID,
                                            p.ID
                                        }).ToList().Select(x => new PipelineModel
                                        {
                                            UniqueID = x.UniqueID,
                                            ID = x.ID,
                                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.OrganizationUniqueID)
                                        }).OrderBy(x => x.OrganizationDescription).ThenBy(x=>x.ID).ToList()
                    };

                    foreach (var checkPoint in model.CheckPointList)
                    {
                        var checkPointCheckItemList = (from x in db.PipePointCheckItem
                                                         join c in db.CheckItem
                                                         on x.CheckItemUniqueID equals c.UniqueID
                                                         where x.PipePointUniqueID == checkPoint.UniqueID
                                                         select c).ToList();

                        foreach (var checkItem in checkPointCheckItemList)
                        {
                            var routeControlPointCheckItem = db.RouteCheckPointCheckItem.FirstOrDefault(x => x.RouteUniqueID == route.UniqueID && x.PipePointUniqueID == checkPoint.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID);

                            checkPoint.CheckItemList.Add(new CheckItemModel()
                            {
                                IsChecked = routeControlPointCheckItem != null,
                                UniqueID = checkItem.UniqueID,
                                CheckType = checkItem.CheckType,
                                CheckItemID = checkItem.ID,
                                CheckItemDescription = checkItem.Description,
                                Seq = routeControlPointCheckItem != null ? routeControlPointCheckItem.Seq : int.MaxValue
                            });
                        }

                        checkPoint.CheckItemList = checkPoint.CheckItemList.OrderBy(x => x.Seq).ToList();
                    }
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

        public static RequestResult Edit(EditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    var route = db.Route.First(x => x.UniqueID == Model.UniqueID);

                    var exists = db.Route.FirstOrDefault(x => x.UniqueID != route.UniqueID && x.OrganizationUniqueID == route.OrganizationUniqueID && x.ID == Model.FormInput.ID);

                    if (exists ==null)
                    {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                        #region Route
                        route.ID = Model.FormInput.ID;
                        route.Name = Model.FormInput.Name;
                        route.Remark = Model.FormInput.Remark;
                        route.LastModifyTime = DateTime.Now;

                        db.SaveChanges();
                        #endregion

                        #region RouteControlPoint, RouteControlPointCheckItem, RouteEquipment, RouteEquipmentCheckItem
                        #region Delete
                        db.RouteCheckPoint.RemoveRange(db.RouteCheckPoint.Where(x => x.RouteUniqueID == route.UniqueID).ToList());
                        db.RouteCheckPointCheckItem.RemoveRange(db.RouteCheckPointCheckItem.Where(x => x.RouteUniqueID == route.UniqueID).ToList());
                        db.RoutePipeline.RemoveRange(db.RoutePipeline.Where(x => x.RouteUniqueID == route.UniqueID).ToList());

                        db.SaveChanges();
                        #endregion


                        #region Insert
                        db.RoutePipeline.AddRange(Model.PipelineList.Select(x => new RoutePipeline
                        {
                            RouteUniqueID = route.UniqueID,
                            PipelineUniqueID = x.UniqueID
                        }).ToList());


                        int checkPointSeq = 1;

                        foreach (var checkPoint in Model.CheckPointList)
                        {
                            db.RouteCheckPoint.Add(new RouteCheckPoint()
                            {
                                RouteUniqueID = route.UniqueID,
                                PipePointUniqueID = checkPoint.UniqueID,
                                MinTimeSpan = checkPoint.MinTimeSpan,
                                Seq = checkPointSeq
                            });

                            checkPointSeq++;

                            int checkPointCheckItemSeq = 1;

                            foreach (var checkItem in checkPoint.CheckItemList)
                            {
                                if (checkItem.IsChecked)
                                {
                                    db.RouteCheckPointCheckItem.Add(new RouteCheckPointCheckItem
                                    {
                                        RouteUniqueID = route.UniqueID,
                                        PipePointUniqueID = checkPoint.UniqueID,
                                        CheckItemUniqueID = checkItem.UniqueID,
                                        Seq = checkPointCheckItemSeq
                                    });

                                    checkPointCheckItemSeq++;
                                }
                            }
                        }

                        db.SaveChanges();
                        #endregion
                        #endregion
#if !DEBUG
                        trans.Complete();
                    }
#endif
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.RouteID, Resources.Resource.Exists));
                    }

                    result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.Route, Resources.Resource.Success));
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

        public static RequestResult SavePageState(List<CheckPointModel> CheckPointList, List<string> CheckPointPageStateList, List<string> CheckPointCheckItemPageStateList)
        {
            RequestResult result = new RequestResult();

            try
            {
                foreach (string checkPointPageState in CheckPointPageStateList)
                {
                    string[] temp = checkPointPageState.Split(Define.Seperators, StringSplitOptions.None);

                    string uniqueID = temp[0];
                    int? minTimeSpan = !string.IsNullOrEmpty(temp[1]) ? int.Parse(temp[1]) : default(int?);
                    int seq = int.Parse(temp[2]) + 1;

                    var checkPoint = CheckPointList.First(x => x.UniqueID == uniqueID);

                    checkPoint.MinTimeSpan = minTimeSpan;
                    checkPoint.Seq = seq;
                }

                foreach (var checkPointCheckItemPageState in CheckPointCheckItemPageStateList)
                {
                    string[] temp = checkPointCheckItemPageState.Split(Define.Seperators, StringSplitOptions.None);

                    string checkPointUniqueID = temp[0];
                    string checkItemUniqueID = temp[1];
                    int seq = int.Parse(temp[2]) + 1;
                    string isChecked = temp[3];

                    var checkItem = CheckPointList.First(x => x.UniqueID == checkPointUniqueID).CheckItemList.First(x => x.UniqueID == checkItemUniqueID);

                    checkItem.IsChecked = isChecked == "Y";
                    checkItem.Seq = seq;
                }

                foreach (var checkPoint in CheckPointList)
                {
                    checkPoint.CheckItemList = checkPoint.CheckItemList.OrderBy(x => x.Seq).ToList();
                }

                result.ReturnData(CheckPointList.OrderBy(x => x.Seq).ToList());
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult Delete(List<string> SelectedList)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    foreach (var uniqueID in SelectedList)
                    {
                        db.Route.Remove(db.Route.First(x => x.UniqueID == uniqueID));

                        db.RouteCheckPoint.RemoveRange(db.RouteCheckPoint.Where(x => x.RouteUniqueID == uniqueID).ToList());

                        db.RouteCheckPointCheckItem.RemoveRange(db.RouteCheckPointCheckItem.Where(x => x.RouteUniqueID == uniqueID).ToList());

                        db.RoutePipeline.RemoveRange(db.RoutePipeline.Where(x => x.RouteUniqueID == uniqueID).ToList());

                        db.JobRoute.RemoveRange(db.JobRoute.Where(x => x.RouteUniqueID == uniqueID).ToList());
                    }

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.Route, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult AddCheckPoint(List<CheckPointModel> CheckPointList, List<string> SelectedList, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    foreach (string selected in SelectedList)
                    {
                        string[] temp = selected.Split(Define.Seperators, StringSplitOptions.None);

                        var organizationUniqueID = temp[0];
                        var pipePointType = temp[1];
                        var checkPointUniqueID = temp[2];

                        if (!string.IsNullOrEmpty(checkPointUniqueID))
                        {
                            if (!CheckPointList.Any(x => x.UniqueID == checkPointUniqueID))
                            {
                                var checkPoint = db.PipePoint.First(x => x.UniqueID == checkPointUniqueID);

                                var controlPointModel = new CheckPointModel()
                                {
                                    UniqueID = checkPoint.UniqueID,
                                     PipePointType=checkPoint.PointType,
                                    ID = checkPoint.ID,
                                    Name = checkPoint.Name,
                                    Seq = CheckPointList.Count + 1
                                };

                                var checkItemList = (from x in db.PipePointCheckItem
                                                     join c in db.CheckItem
                                                     on x.CheckItemUniqueID equals c.UniqueID
                                                     where x.PipePointUniqueID == checkPointUniqueID
                                                     select c).OrderBy(x => x.CheckType).ThenBy(x => x.ID).ToList();

                                int seq = 1;

                                foreach (var checkItem in checkItemList)
                                {
                                    controlPointModel.CheckItemList.Add(new CheckItemModel()
                                    {
                                        IsChecked = true,
                                        UniqueID = checkItem.UniqueID,
                                        CheckType = checkItem.CheckType,
                                        CheckItemID = checkItem.ID,
                                        CheckItemDescription = checkItem.Description,
                                        Seq = seq
                                    });

                                    seq++;
                                }

                                CheckPointList.Add(controlPointModel);
                            }
                        }
                        else if (!string.IsNullOrEmpty(pipePointType))
                        {
                            var checkPointList = db.PipePoint.Where(x => x.OrganizationUniqueID == organizationUniqueID && x.PointType == pipePointType).OrderBy(x => x.ID).ToList();

                            foreach (var checkPoint in checkPointList)
                            {
                                if (!CheckPointList.Any(x => x.UniqueID == checkPoint.UniqueID))
                                {
                                    var controlPointModel = new CheckPointModel()
                                    {
                                        UniqueID = checkPoint.UniqueID,
                                        PipePointType = checkPoint.PointType,
                                        ID = checkPoint.ID,
                                        Name = checkPoint.Name,
                                        Seq = CheckPointList.Count + 1
                                    };

                                    var checkItemList = (from x in db.PipePointCheckItem
                                                         join c in db.CheckItem
                                                         on x.CheckItemUniqueID equals c.UniqueID
                                                         where x.PipePointUniqueID == checkPointUniqueID
                                                         select c).OrderBy(x => x.CheckType).ThenBy(x => x.ID).ToList();

                                    int seq = 1;

                                    foreach (var checkItem in checkItemList)
                                    {
                                        controlPointModel.CheckItemList.Add(new CheckItemModel()
                                        {
                                            IsChecked = true,
                                            UniqueID = checkItem.UniqueID,
                                            CheckType = checkItem.CheckType,
                                            CheckItemID = checkItem.ID,
                                            CheckItemDescription = checkItem.Description,
                                            Seq = seq
                                        });

                                        seq++;
                                    }

                                    CheckPointList.Add(controlPointModel);
                                }
                            }
                        }
                        else
                        {
                            var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(RefOrganizationUniqueID, true);
                            var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(RefOrganizationUniqueID, false);

                            var organizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organizationUniqueID, true);

                            foreach (var organization in organizationList)
                            {
                                if (upStreamOrganizationList.Any(x => x == organization) || downStreamOrganizationList.Any(x => x == organization))
                                {
                                    var checkPointList = db.PipePoint.Where(x => x.OrganizationUniqueID == organization).OrderBy(x => x.ID).ToList();

                                    foreach (var checkPoint in checkPointList)
                                    {
                                        if (!CheckPointList.Any(x => x.UniqueID == checkPoint.UniqueID))
                                        {
                                            var checkPointModel = new CheckPointModel()
                                            {
                                                UniqueID = checkPoint.UniqueID,
                                                PipePointType = checkPoint.PointType,
                                                ID = checkPoint.ID,
                                                Name = checkPoint.Name,
                                                Seq = CheckPointList.Count + 1
                                            };

                                            var checkItemList = (from x in db.PipePointCheckItem
                                                                 join c in db.CheckItem
                                                                 on x.CheckItemUniqueID equals c.UniqueID
                                                                 where x.PipePointUniqueID == checkPoint.UniqueID
                                                                 select c).OrderBy(x => x.CheckType).ThenBy(x => x.ID).ToList();

                                            int seq = 1;

                                            foreach (var checkItem in checkItemList)
                                            {
                                                checkPointModel.CheckItemList.Add(new CheckItemModel()
                                                {
                                                    IsChecked = true,
                                                    UniqueID = checkItem.UniqueID,
                                                    CheckType = checkItem.CheckType,
                                                    CheckItemID = checkItem.ID,
                                                    CheckItemDescription = checkItem.Description,
                                                    Seq = seq
                                                });

                                                seq++;
                                            }

                                            CheckPointList.Add(checkPointModel);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (var checkPoint in CheckPointList)
                {
                    checkPoint.CheckItemList = checkPoint.CheckItemList.OrderBy(x => x.Seq).ToList();
                }

                result.ReturnData(CheckPointList.OrderBy(x => x.Seq).ToList());
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult AddPipeline(List<PipelineModel> PipelineList, List<string> SelectedList, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    foreach (string selected in SelectedList)
                    {
                        string[] temp = selected.Split(Define.Seperators, StringSplitOptions.None);

                        var organizationUniqueID = temp[0];
                        var pipelineUniqueID = temp[1];

                        if (!string.IsNullOrEmpty(pipelineUniqueID))
                        {
                            if (!PipelineList.Any(x => x.UniqueID == pipelineUniqueID))
                            {
                                var pipeline = db.Pipeline.First(x => x.UniqueID == pipelineUniqueID);

                                PipelineList.Add(new PipelineModel()
                                {
                                    OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(pipeline.OrganizationUniqueID),
                                    UniqueID = pipeline.UniqueID,
                                    ID = pipeline.ID
                                });
                            }
                        }
                        else
                        {
                            var organizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organizationUniqueID, true);

                            foreach (var organization in organizationList)
                            {
                                if (Account.QueryableOrganizationUniqueIDList.Contains(organization))
                                {
                                    var pipelineList = db.Pipeline.Where(x => x.OrganizationUniqueID == organization).OrderBy(x => x.ID).ToList();

                                    foreach (var pipeline in pipelineList)
                                    {
                                        if (!PipelineList.Any(x => x.UniqueID == pipeline.UniqueID))
                                        {
                                            PipelineList.Add(new PipelineModel()
                                            {
                                                OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(pipeline.OrganizationUniqueID),
                                                UniqueID = pipeline.UniqueID,
                                                ID = pipeline.ID
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                result.ReturnData(PipelineList.OrderBy(x => x.OrganizationDescription).ThenBy(x => x.ID).ToList());
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetTreeItem(string OrganizationUniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>() 
                { 
                    { Define.EnumTreeAttribute.NodeType, string.Empty },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.OrganizationUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.RouteUniqueID, string.Empty }
                };

                using (PDbEntities edb = new PDbEntities())
                {
                    if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                    {
                        var routeList = edb.Route.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                        foreach (var route in routeList)
                        {
                            var treeItem = new TreeItem() { Title = route.Name };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Route.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", route.ID, route.Name);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.RouteUniqueID] = route.UniqueID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }
                    }

                    using (DbEntities db = new DbEntities())
                    {
                        var organizationList = db.Organization.Where(x => x.ParentUniqueID == OrganizationUniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                        foreach (var organization in organizationList)
                        {
                            var treeItem = new TreeItem() { Title = organization.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                            attributes[Define.EnumTreeAttribute.RouteUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (db.Organization.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                                ||
                                (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && edb.Route.Any(x => x.OrganizationUniqueID == organization.UniqueID)))
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                }

                result.ReturnData(treeItemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetDetailTreeItem(string RouteUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>() 
                { 
                    { Define.EnumTreeAttribute.NodeType, string.Empty },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.RouteUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.PipePointUniqueID, string.Empty }
                };

                using (PDbEntities db = new PDbEntities())
                {
                    var checkPointList = (from x in db.RouteCheckPoint
                                            join p in db.PipePoint
                                            on x.PipePointUniqueID equals p.UniqueID
                                            where x.RouteUniqueID == RouteUniqueID
                                            select new
                                            {
                                                p.UniqueID,
                                                p.ID,
                                                p.Name,
                                                x.Seq
                                            }).OrderBy(x => x.Seq).ToList();

                    foreach (var checkPoint in checkPointList)
                    {
                        var treeItem = new TreeItem() { Title = checkPoint.Name };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.PipePoint.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", checkPoint.ID, checkPoint.Name);
                        attributes[Define.EnumTreeAttribute.RouteUniqueID] = RouteUniqueID;
                        attributes[Define.EnumTreeAttribute.PipePointUniqueID] = checkPoint.UniqueID;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        if (db.RouteCheckPointCheckItem.Any(x => x.RouteUniqueID == RouteUniqueID && x.PipePointUniqueID == checkPoint.UniqueID))
                        {
                            treeItem.State = "closed";
                        }

                        treeItemList.Add(treeItem);
                    }
                }

                result.ReturnData(treeItemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetDetailTreeItem(string RouteUniqueID, string CheckPointUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>() 
                { 
                    { Define.EnumTreeAttribute.NodeType, string.Empty },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.RouteUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.PipePointUniqueID, string.Empty },
                };

                using (PDbEntities db = new PDbEntities())
                {
                    var checkItemList = (from x in db.RouteCheckPointCheckItem
                                         join c in db.CheckItem
                                         on x.CheckItemUniqueID equals c.UniqueID
                                         where x.RouteUniqueID == RouteUniqueID && x.PipePointUniqueID == CheckPointUniqueID
                                         select new
                                         {
                                             c.ID,
                                             c.Description,
                                             x.Seq
                                         }).OrderBy(x => x.Seq).ToList();


                    foreach (var checkItem in checkItemList)
                    {
                        var treeItem = new TreeItem() { Title = checkItem.Description };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.CheckItem.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", checkItem.ID, checkItem.Description);
                        attributes[Define.EnumTreeAttribute.RouteUniqueID] = RouteUniqueID;
                        attributes[Define.EnumTreeAttribute.PipePointUniqueID] = CheckPointUniqueID;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        treeItemList.Add(treeItem);
                    }
                }

                result.ReturnData(treeItemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>() 
                { 
                    { Define.EnumTreeAttribute.NodeType, string.Empty },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.OrganizationUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.RouteUniqueID, string.Empty }
                };

                using (PDbEntities edb = new PDbEntities())
                {
                    var routeList = edb.Route.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                    foreach (var route in routeList)
                    {
                        var treeItem = new TreeItem() { Title = route.Name };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Route.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", route.ID, route.Name);
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                        attributes[Define.EnumTreeAttribute.RouteUniqueID] = route.UniqueID;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        treeItemList.Add(treeItem);
                    }

                    var availableOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(RefOrganizationUniqueID, true);

                    using (DbEntities db = new DbEntities())
                    {
                        var organizationList = db.Organization.Where(x => x.ParentUniqueID == OrganizationUniqueID && availableOrganizationList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                        foreach (var organization in organizationList)
                        {
                            var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organization.UniqueID, true);

                            if (edb.Route.Any(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && availableOrganizationList.Contains(x.OrganizationUniqueID)))
                            {
                                var treeItem = new TreeItem() { Title = organization.Description };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                                attributes[Define.EnumTreeAttribute.RouteUniqueID] = string.Empty;

                                foreach (var attribute in attributes)
                                {
                                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                }

                                treeItem.State = "closed";

                                treeItemList.Add(treeItem);
                            }
                        }
                    }
                }

                result.ReturnData(treeItemList);
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
