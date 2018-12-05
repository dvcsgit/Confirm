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
using DbEntity.MSSQL.GuardPatrol;
using Models.Shared;
using Models.Authenticated;
using Models.GuardPatrol.RouteManagement;

namespace DataAccess.GuardPatrol
{
    public class RouteDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (GDbEntities db = new GDbEntities())
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

                using (GDbEntities db = new GDbEntities())
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
                        ManagerList = db.RouteManager.Where(x => x.RouteUniqueID == UniqueID).Select(x => new ManagerModel()
                        {
                            ID = x.UserID
                        }).ToList()
                    };
                }

                using (DbEntities db = new DbEntities())
                {
                    model.ManagerList = (from m in model.ManagerList
                                         join u in db.User
                                         on m.ID equals u.ID
                                         join o in db.Organization
                                         on u.OrganizationUniqueID equals o.UniqueID
                                         select new ManagerModel
                                         {
                                             OrganizationDescription = o.Description,
                                             ID = u.ID,
                                             Name = u.Name
                                         }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.ID).ToList();
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

                using (GDbEntities db = new GDbEntities())
                {
                    var route = db.Route.First(x => x.UniqueID == UniqueID);

                    model = new CreateFormModel()
                    {
                        OrganizationUniqueID = route.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(route.OrganizationUniqueID),
                        ControlPointList = (from x in db.RouteControlPoint
                                            join c in db.ControlPoint
                                            on x.ControlPointUniqueID equals c.UniqueID
                                            where x.RouteUniqueID == route.UniqueID
                                            select new ControlPointModel
                                            {
                                                UniqueID = c.UniqueID,
                                                ID = c.ID,
                                                Description = c.Description,
                                                MinTimeSpan = x.MinTimeSpan,
                                                Seq = x.Seq,
                                            }).OrderBy(x => x.Seq).ToList()
                    };

                    foreach (var controlPoint in model.ControlPointList)
                    {
                        var controlPointCheckItemList = (from x in db.ControlPointCheckItem
                                                         join c in db.CheckItem
                                                         on x.CheckItemUniqueID equals c.UniqueID
                                                         where x.ControlPointUniqueID == controlPoint.UniqueID
                                                         select c).ToList();

                        foreach (var checkItem in controlPointCheckItemList)
                        {
                            var routeControlPointCheckItem = db.RouteControlPointCheckItem.FirstOrDefault(x => x.RouteUniqueID == route.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID);

                            controlPoint.CheckItemList.Add(new CheckItemModel()
                            {
                                IsChecked = routeControlPointCheckItem != null,
                                UniqueID = checkItem.UniqueID,
                                CheckType = checkItem.CheckType,
                                CheckItemID = checkItem.ID,
                                CheckItemDescription = checkItem.Description,
                                Seq = routeControlPointCheckItem != null ? routeControlPointCheckItem.Seq : int.MaxValue
                            });
                        }

                        controlPoint.CheckItemList = controlPoint.CheckItemList.OrderBy(x => x.Seq).ToList();
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
                using (GDbEntities db = new GDbEntities())
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
                            LastModifyTime = DateTime.Now
                        });

                        int controlPointSeq = 1;

                        foreach (var controlPoint in Model.ControlPointList)
                        {
                            db.RouteControlPoint.Add(new RouteControlPoint()
                            {
                                RouteUniqueID = uniqueID,
                                ControlPointUniqueID = controlPoint.UniqueID,
                                MinTimeSpan = controlPoint.MinTimeSpan,
                                Seq = controlPointSeq
                            });

                            controlPointSeq++;

                            int controlPointCheckItemSeq = 1;

                            foreach (var checkItem in controlPoint.CheckItemList)
                            {
                                if (checkItem.IsChecked)
                                {
                                    db.RouteControlPointCheckItem.Add(new RouteControlPointCheckItem
                                    {
                                        RouteUniqueID = uniqueID,
                                        ControlPointUniqueID = controlPoint.UniqueID,
                                        CheckItemUniqueID = checkItem.UniqueID,
                                        Seq = controlPointCheckItemSeq
                                    });

                                    controlPointCheckItemSeq++;
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

                using (GDbEntities db = new GDbEntities())
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
                            Name = route.Name
                        },
                        ControlPointList = (from x in db.RouteControlPoint
                                          join c in db.ControlPoint
                                          on x.ControlPointUniqueID equals c.UniqueID
                                          where x.RouteUniqueID == route.UniqueID
                                          select new ControlPointModel
                                          {
                                              UniqueID = c.UniqueID,
                                              ID = c.ID,
                                              Description = c.Description,
                                              MinTimeSpan = x.MinTimeSpan,
                                              Seq = x.Seq
                                          }).OrderBy(x => x.Seq).ToList()
                    };

                    foreach (var controlPoint in model.ControlPointList)
                    {
                        var controlPointCheckItemList = (from x in db.ControlPointCheckItem
                                                         join c in db.CheckItem
                                                         on x.CheckItemUniqueID equals c.UniqueID
                                                         where x.ControlPointUniqueID == controlPoint.UniqueID
                                                         select c).ToList();

                        foreach (var checkItem in controlPointCheckItemList)
                        {
                            var routeControlPointCheckItem = db.RouteControlPointCheckItem.FirstOrDefault(x => x.RouteUniqueID == route.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID);

                            controlPoint.CheckItemList.Add(new CheckItemModel()
                            {
                                IsChecked = routeControlPointCheckItem != null,
                                UniqueID = checkItem.UniqueID,
                                CheckType = checkItem.CheckType,
                                CheckItemID = checkItem.ID,
                                CheckItemDescription = checkItem.Description,
                                Seq = routeControlPointCheckItem != null ? routeControlPointCheckItem.Seq : int.MaxValue
                            });
                        }

                        controlPoint.CheckItemList = controlPoint.CheckItemList.OrderBy(x => x.Seq).ToList();
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
                using (GDbEntities db = new GDbEntities())
                {
                    var route = db.Route.First(x => x.UniqueID == Model.UniqueID);

                    var exists = db.Route.FirstOrDefault(x => x.UniqueID != route.UniqueID && x.OrganizationUniqueID == route.OrganizationUniqueID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                        #region Route
                        route.ID = Model.FormInput.ID;
                        route.Name = Model.FormInput.Name;
                        route.LastModifyTime = DateTime.Now;

                        db.SaveChanges();
                        #endregion

                        #region RouteControlPoint, RouteControlPointCheckItem, RouteEquipment, RouteEquipmentCheckItem
                        #region Delete
                        db.RouteControlPoint.RemoveRange(db.RouteControlPoint.Where(x => x.RouteUniqueID == route.UniqueID).ToList());
                        db.RouteControlPointCheckItem.RemoveRange(db.RouteControlPointCheckItem.Where(x => x.RouteUniqueID == route.UniqueID).ToList());

                        db.SaveChanges();
                        #endregion


                        #region Insert
                        int controlPointSeq = 1;

                        foreach (var controlPoint in Model.ControlPointList)
                        {
                            db.RouteControlPoint.Add(new RouteControlPoint()
                            {
                                RouteUniqueID = route.UniqueID,
                                ControlPointUniqueID = controlPoint.UniqueID,
                                MinTimeSpan = controlPoint.MinTimeSpan,
                                Seq = controlPointSeq
                            });

                            controlPointSeq++;

                            int controlPointCheckItemSeq = 1;

                            foreach (var checkItem in controlPoint.CheckItemList)
                            {
                                if (checkItem.IsChecked)
                                {
                                    db.RouteControlPointCheckItem.Add(new RouteControlPointCheckItem
                                    {
                                        RouteUniqueID = route.UniqueID,
                                        ControlPointUniqueID = controlPoint.UniqueID,
                                        CheckItemUniqueID = checkItem.UniqueID,
                                        Seq = controlPointCheckItemSeq
                                    });

                                    controlPointCheckItemSeq++;
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

        public static RequestResult SavePageState(List<ControlPointModel> ControlPointList, List<string> ControlPointPageStateList, List<string> ControlPointCheckItemPageStateList)
        {
            RequestResult result = new RequestResult();

            try
            {
                foreach (string controlPointPageState in ControlPointPageStateList)
                {
                    string[] temp = controlPointPageState.Split(Define.Seperators, StringSplitOptions.None);

                    string uniqueID = temp[0];
                    int? minTimeSpan = !string.IsNullOrEmpty(temp[1]) ? int.Parse(temp[1]) : default(int?);
                    int seq = int.Parse(temp[2]) + 1;

                    var controlPoint = ControlPointList.First(x => x.UniqueID == uniqueID);

                    controlPoint.MinTimeSpan = minTimeSpan;
                    controlPoint.Seq = seq;
                }

                foreach (var controlPointCheckItemPageState in ControlPointCheckItemPageStateList)
                {
                    string[] temp = controlPointCheckItemPageState.Split(Define.Seperators, StringSplitOptions.None);

                    string controlPointUniqueID = temp[0];
                    string checkItemUniqueID = temp[1];
                    int seq = int.Parse(temp[2]) + 1;
                    string isChecked = temp[3];

                    var checkItem = ControlPointList.First(x => x.UniqueID == controlPointUniqueID).CheckItemList.First(x => x.UniqueID == checkItemUniqueID);

                    checkItem.IsChecked = isChecked == "Y";
                    checkItem.Seq = seq;
                }

                foreach (var controlPoint in ControlPointList)
                {
                    controlPoint.CheckItemList = controlPoint.CheckItemList.OrderBy(x => x.Seq).ToList();
                }

                result.ReturnData(ControlPointList.OrderBy(x => x.Seq).ToList());
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult AddControlPoint(List<ControlPointModel> ControlPointList, List<string> SelectedList, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (GDbEntities db = new GDbEntities())
                {
                    foreach (string selected in SelectedList)
                    {
                        string[] temp = selected.Split(Define.Seperators, StringSplitOptions.None);

                        var organizationUniqueID = temp[0];
                        var controlPointUniqueID = temp[1];

                        if (!string.IsNullOrEmpty(controlPointUniqueID))
                        {
                            if (!ControlPointList.Any(x => x.UniqueID == controlPointUniqueID))
                            {
                                var controlPoint = db.ControlPoint.First(x => x.UniqueID == controlPointUniqueID);

                                var controlPointModel = new ControlPointModel()
                                {
                                    UniqueID = controlPoint.UniqueID,
                                    ID = controlPoint.ID,
                                    Description = controlPoint.Description,
                                    Seq = ControlPointList.Count + 1
                                };

                                var checkItemList = (from x in db.ControlPointCheckItem
                                                     join c in db.CheckItem
                                                     on x.CheckItemUniqueID equals c.UniqueID
                                                     where x.ControlPointUniqueID == controlPointUniqueID
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

                                ControlPointList.Add(controlPointModel);
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
                                    var controlPointList = db.ControlPoint.Where(x => x.OrganizationUniqueID == organization).OrderBy(x => x.ID).ToList();

                                    foreach (var controlPoint in controlPointList)
                                    {
                                        if (!ControlPointList.Any(x => x.UniqueID == controlPoint.UniqueID))
                                        {
                                            var controlPointModel = new ControlPointModel()
                                            {
                                                UniqueID = controlPoint.UniqueID,
                                                ID = controlPoint.ID,
                                                Description = controlPoint.Description,
                                                Seq = ControlPointList.Count + 1
                                            };

                                            var checkItemList = (from x in db.ControlPointCheckItem
                                                                 join c in db.CheckItem
                                                                 on x.CheckItemUniqueID equals c.UniqueID
                                                                 where x.ControlPointUniqueID == controlPoint.UniqueID
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

                                            ControlPointList.Add(controlPointModel);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (var controlPoint in ControlPointList)
                {
                    controlPoint.CheckItemList = controlPoint.CheckItemList.OrderBy(x => x.Seq).ToList();
                }

                result.ReturnData(ControlPointList.OrderBy(x => x.Seq).ToList());
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

                using (GDbEntities gdb = new GDbEntities())
                {
                    if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                    {
                        var routeList = gdb.Route.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

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
                                (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && gdb.Route.Any(x => x.OrganizationUniqueID == organization.UniqueID)))
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
                    { Define.EnumTreeAttribute.ControlPointUniqueID, string.Empty }
                };

                using (GDbEntities db = new GDbEntities())
                {
                    var controlPointList = (from x in db.RouteControlPoint
                                            join c in db.ControlPoint
                                            on x.ControlPointUniqueID equals c.UniqueID
                                            where x.RouteUniqueID == RouteUniqueID
                                            select new
                                            {
                                                c.UniqueID,
                                                c.ID,
                                                c.Description,
                                                x.Seq
                                            }).OrderBy(x => x.Seq).ToList();

                    foreach (var controlPoint in controlPointList)
                    {
                        var treeItem = new TreeItem() { Title = controlPoint.Description };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.ControlPoint.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", controlPoint.ID, controlPoint.Description);
                        attributes[Define.EnumTreeAttribute.RouteUniqueID] = RouteUniqueID;
                        attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = controlPoint.UniqueID;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        if (db.RouteControlPointCheckItem.Any(x => x.RouteUniqueID == RouteUniqueID && x.ControlPointUniqueID == controlPoint.UniqueID))
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

        public static RequestResult GetDetailTreeItem(string RouteUniqueID, string ControlPointUniqueID)
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
                    { Define.EnumTreeAttribute.ControlPointUniqueID, string.Empty },
                };

                using (GDbEntities db = new GDbEntities())
                {
                    var checkItemList = (from x in db.RouteControlPointCheckItem
                                         join c in db.CheckItem
                                         on x.CheckItemUniqueID equals c.UniqueID
                                         where x.RouteUniqueID == RouteUniqueID && x.ControlPointUniqueID == ControlPointUniqueID
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
                        attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = ControlPointUniqueID;

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

                using (GDbEntities gdb = new GDbEntities())
                {
                    var routeList = gdb.Route.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

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

                            if (gdb.Route.Any(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && availableOrganizationList.Contains(x.OrganizationUniqueID)))
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

        public static RequestResult Delete(List<string> SelectedList)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (GDbEntities db = new GDbEntities())
                {
                    foreach (var uniqueID in SelectedList)
                    {
                        db.Route.Remove(db.Route.First(x => x.UniqueID == uniqueID));

                        db.RouteControlPoint.RemoveRange(db.RouteControlPoint.Where(x => x.RouteUniqueID == uniqueID).ToList());

                        db.RouteControlPointCheckItem.RemoveRange(db.RouteControlPointCheckItem.Where(x => x.RouteUniqueID == uniqueID).ToList());

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
    }
}
