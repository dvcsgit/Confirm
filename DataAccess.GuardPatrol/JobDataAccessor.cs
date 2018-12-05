using DbEntity.MSSQL;
using DbEntity.MSSQL.GuardPatrol;
using Models.Authenticated;
using Models.GuardPatrol.JobManagement;
using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;
using System.Transactions;

namespace DataAccess.GuardPatrol
{
    public class JobDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (GDbEntities db = new GDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.Job.Where(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.Description.Contains(Parameters.Keyword));
                    }

                    var organization = OrganizationDataAccessor.GetOrganization(Parameters.OrganizationUniqueID);

                    result.ReturnData(new GridViewModel()
                    {
                        OrganizationUniqueID = Parameters.OrganizationUniqueID,
                        Permission = Account.OrganizationPermission(Parameters.OrganizationUniqueID),
                        OrganizationDescription = organization.Description,
                        FullOrganizationDescription = organization.FullDescription,
                        ItemList = query.ToList().Select(x => new GridItem()
                        {
                            UniqueID = x.UniqueID,
                            Permission = Account.OrganizationPermission(x.OrganizationUniqueID),
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.OrganizationUniqueID),
                            Description = x.Description,
                            BeginDate = x.BeginDate,
                            EndDate = x.EndDate,
                            BeginTime = x.BeginTime,
                            EndTime = x.EndTime,
                            CycleCount = x.CycleCount,
                            CycleMode = x.CycleMode
                        }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.Description).ToList()
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
                    { Define.EnumTreeAttribute.JobUniqueID, string.Empty }
                };

                using (GDbEntities gdb = new GDbEntities())
                {
                    if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                    {
                        var jobList = gdb.Job.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.Description).ToList();

                        foreach (var job in jobList)
                        {
                            var treeItem = new TreeItem() { Title = job.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Job.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = job.Description;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.JobUniqueID] = job.UniqueID;

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
                            attributes[Define.EnumTreeAttribute.JobUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (db.Organization.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                                ||
                                (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && gdb.Job.Any(x => x.OrganizationUniqueID == organization.UniqueID)))
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

        public static RequestResult GetDetailViewModel(string UniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new DetailViewModel();

                using (GDbEntities db = new GDbEntities())
                {
                    var job = db.Job.First(x => x.UniqueID == UniqueID);

                    model = new DetailViewModel()
                    {
                        UniqueID = job.UniqueID,
                        Permission = Account.OrganizationPermission(job.OrganizationUniqueID),
                        OrganizationUniqueID = job.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(job.OrganizationUniqueID),
                        Description = job.Description,
                        IsCheckBySeq = job.IsCheckBySeq,
                        IsShowPrevRecord = job.IsShowPrevRecord,
                        IsNeedVerify = job.IsNeedVerify,
                        BeginDate = job.BeginDate,
                        EndDate = job.EndDate,
                        TimeMode = job.TimeMode == 0 ? Resources.Resource.TimeMode0 : Resources.Resource.TimeMode1,
                        BeginTime = job.BeginTime,
                        EndTime = job.EndTime,
                        CycleCount = job.CycleCount,
                        CycleMode = job.CycleMode,
                        Remark = job.Remark,
                        UserList = db.JobUser.Where(x => x.JobUniqueID == UniqueID).Select(x => new Models.GuardPatrol.JobManagement.UserModel
                        {
                            ID = x.UserID
                        }).ToList(),
                        RouteList = (from x in db.JobRoute
                                     join r in db.Route
                                     on x.RouteUniqueID equals r.UniqueID
                                     where x.JobUniqueID == UniqueID
                                     select new RouteModel
                                     {
                                         UniqueID = r.UniqueID,
                                         ID = r.ID,
                                         Name = r.Name,
                                         IsOptional = x.IsOptional
                                     }).OrderBy(x => x.ID).ToList()
                    };
                }

                using (DbEntities db = new DbEntities())
                {
                    model.UserList = (from user in model.UserList
                                      join u in db.User
                                      on user.ID equals u.ID
                                      join o in db.Organization
                                      on u.OrganizationUniqueID equals o.UniqueID
                                      select new Models.GuardPatrol.JobManagement.UserModel
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

        public static RequestResult GetDetailTreeItem(string JobUniqueID, string RouteUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new DetailTreeModel()
                {
                    RouteUniqueID = RouteUniqueID
                };

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>() 
                { 
                    { Define.EnumTreeAttribute.NodeType, string.Empty },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.JobUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.RouteUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.ControlPointUniqueID, string.Empty }
                };

                using (GDbEntities db = new GDbEntities())
                {
                    var controlPointList = (from x in db.JobControlPoint
                                            join y in db.RouteControlPoint
                                            on new { x.RouteUniqueID, x.ControlPointUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID }
                                            join c in db.ControlPoint
                                            on x.ControlPointUniqueID equals c.UniqueID
                                            where x.JobUniqueID == JobUniqueID && x.RouteUniqueID == RouteUniqueID
                                            select new
                                            {
                                                c.UniqueID,
                                                c.ID,
                                                c.Description,
                                                y.Seq
                                            }).OrderBy(x => x.Seq).ToList();

                    foreach (var controlPoint in controlPointList)
                    {
                        var treeItem = new TreeItem() { Title = controlPoint.Description };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.ControlPoint.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", controlPoint.ID, controlPoint.Description);
                        attributes[Define.EnumTreeAttribute.JobUniqueID] = JobUniqueID;
                        attributes[Define.EnumTreeAttribute.RouteUniqueID] = RouteUniqueID;
                        attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = controlPoint.UniqueID;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        if (db.JobControlPointCheckItem.Any(x => x.JobUniqueID == JobUniqueID && x.RouteUniqueID == RouteUniqueID && x.ControlPointUniqueID == controlPoint.UniqueID))
                        {
                            treeItem.State = "closed";
                        }

                        model.TreeItemList.Add(treeItem);
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

        public static RequestResult GetDetailTreeItem(string JobUniqueID, string RouteUniqueID, string ControlPointUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>() 
                { 
                    { Define.EnumTreeAttribute.NodeType, string.Empty },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.JobUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.RouteUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.ControlPointUniqueID, string.Empty }
                };

                using (GDbEntities db = new GDbEntities())
                {
                    var checkItemList = (from x in db.JobControlPointCheckItem
                                         join y in db.RouteControlPointCheckItem
                                         on new { x.RouteUniqueID, x.ControlPointUniqueID, x.CheckItemUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.CheckItemUniqueID }
                                         join c in db.CheckItem
                                         on x.CheckItemUniqueID equals c.UniqueID
                                         where x.JobUniqueID == JobUniqueID && x.RouteUniqueID == RouteUniqueID && x.ControlPointUniqueID == ControlPointUniqueID
                                         select new
                                         {
                                             c.ID,
                                             c.Description,
                                             y.Seq
                                         }).OrderBy(x => x.Seq).ToList();

                    foreach (var checkItem in checkItemList)
                    {
                        var treeItem = new TreeItem() { Title = checkItem.Description };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.CheckItem.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", checkItem.ID, checkItem.Description);
                        attributes[Define.EnumTreeAttribute.JobUniqueID] = JobUniqueID;
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

        public static RequestResult Create(CreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (GDbEntities db = new GDbEntities())
                {
                    var exists = db.Job.FirstOrDefault(x => x.OrganizationUniqueID == Model.OrganizationUniqueID && x.Description == Model.FormInput.Description);

                    if (exists == null)
                    {
                        string uniqueID = Guid.NewGuid().ToString();

                        db.Job.Add(new Job()
                        {
                            UniqueID = uniqueID,
                            OrganizationUniqueID = Model.OrganizationUniqueID,
                            Description = Model.FormInput.Description,
                            TimeMode = Model.FormInput.TimeMode,
                            BeginTime = Model.FormInput.BeginTime,
                            EndTime = Model.FormInput.EndTime,
                            IsCheckBySeq = Model.FormInput.IsCheckBySeq,
                            IsShowPrevRecord = Model.FormInput.IsShowPrevRecord,
                            IsNeedVerify = Model.FormInput.IsNeedVerify,
                            CycleCount = Model.FormInput.CycleCount,
                            CycleMode = Model.FormInput.CycleMode,
                            BeginDate = Model.FormInput.BeginDate,
                            EndDate = Model.FormInput.EndDate,
                            Remark = Model.FormInput.Remark,
                            LastModifyTime = DateTime.Now
                        });

                        db.JobUser.AddRange(Model.UserList.Select(x => new JobUser
                        {
                            JobUniqueID = uniqueID,
                            UserID = x.ID
                        })).ToList();

                        foreach (var route in Model.RouteList)
                        {
                            if (route.ControlPointList.Any(x => x.IsChecked))
                            {
                                db.JobRoute.Add(new JobRoute()
                                {
                                    JobUniqueID = uniqueID,
                                    RouteUniqueID = route.UniqueID,
                                    IsOptional = route.IsOptional,
                                    UniqueID = Guid.NewGuid().ToString()
                                });

                                foreach (var controlPoint in route.ControlPointList)
                                {
                                    if (controlPoint.CheckItemList.Count(x => x.IsChecked) > 0)
                                    {
                                        db.JobControlPoint.Add(new JobControlPoint()
                                        {
                                            JobUniqueID = uniqueID,
                                            RouteUniqueID = route.UniqueID,
                                            ControlPointUniqueID = controlPoint.UniqueID,
                                            MinTimeSpan = controlPoint.MinTimeSpan
                                        });

                                        foreach (var checkItem in controlPoint.CheckItemList)
                                        {
                                            if (checkItem.IsChecked)
                                            {
                                                db.JobControlPointCheckItem.Add(new JobControlPointCheckItem()
                                                {
                                                    JobUniqueID = uniqueID,
                                                    RouteUniqueID = route.UniqueID,
                                                    ControlPointUniqueID = controlPoint.UniqueID,
                                                    CheckItemUniqueID = checkItem.UniqueID
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        db.SaveChanges();

                        result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.Job, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.JobDescription, Resources.Resource.Exists));
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

        public static RequestResult GetCopyFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new CreateFormModel();

                using (GDbEntities db = new GDbEntities())
                {
                    var job = db.Job.First(x => x.UniqueID == UniqueID);

                    model = new CreateFormModel()
                    {
                        AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(job.OrganizationUniqueID),
                        OrganizationUniqueID = job.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(job.OrganizationUniqueID),
                        FormInput = new FormInput()
                        {
                            IsCheckBySeq = job.IsCheckBySeq,
                            IsNeedVerify = job.IsNeedVerify,
                            IsShowPrevRecord = job.IsShowPrevRecord,
                            BeginDateString = DateTimeHelper.DateTime2DateStringWithSeperator(job.BeginDate),
                            EndDateString = DateTimeHelper.DateTime2DateStringWithSeperator(job.EndDate),
                            TimeMode = job.TimeMode,
                            BeginTime = job.BeginTime,
                            EndTime = job.EndTime,
                            CycleCount = job.CycleCount,
                            CycleMode = job.CycleMode,
                            Remark = job.Remark
                        },
                        UserList = db.JobUser.Where(x => x.JobUniqueID == UniqueID).Select(x => new Models.GuardPatrol.JobManagement.UserModel
                        {
                            ID = x.UserID
                        }).ToList()
                    };

                    var jobRouteList = (from x in db.JobRoute
                                        join r in db.Route
                                        on x.RouteUniqueID equals r.UniqueID
                                        where x.JobUniqueID == job.UniqueID
                                        select new
                                        {
                                            r.UniqueID,
                                            x.IsOptional,
                                            r.ID,
                                            r.Name
                                        }).ToList();

                    foreach (var jobRoute in jobRouteList)
                    {
                        var routeModel = new RouteModel()
                        {
                            UniqueID = jobRoute.UniqueID,
                            ID = jobRoute.ID,
                            Name = jobRoute.Name,
                            IsOptional = jobRoute.IsOptional
                        };

                        var jobControlPointList = db.JobControlPoint.Where(x => x.JobUniqueID == job.UniqueID && x.RouteUniqueID == jobRoute.UniqueID).ToList();
                        var jobControlPointCheckItemList = db.JobControlPointCheckItem.Where(x => x.JobUniqueID == UniqueID && x.RouteUniqueID == jobRoute.UniqueID).ToList();

                        var routeControlPointList = (from x in db.RouteControlPoint
                                                     join c in db.ControlPoint
                                                     on x.ControlPointUniqueID equals c.UniqueID
                                                     where x.RouteUniqueID == jobRoute.UniqueID
                                                     select new
                                                     {
                                                         c.UniqueID,
                                                         c.ID,
                                                         c.Description,
                                                         x.Seq
                                                     }).ToList();

                        foreach (var routeControlPoint in routeControlPointList)
                        {
                            var jobControlPoint = jobControlPointList.FirstOrDefault(x => x.ControlPointUniqueID == routeControlPoint.UniqueID);

                            var controlPointModel = new ControlPointModel()
                            {
                                UniqueID = routeControlPoint.UniqueID,
                                ID = routeControlPoint.ID,
                                Description = routeControlPoint.Description,
                                MinTimeSpan = jobControlPoint != null ? jobControlPoint.MinTimeSpan : default(int?),
                                Seq = routeControlPoint.Seq
                            };

                            var routeControlPointCheckItemList = (from x in db.RouteControlPointCheckItem
                                                                  join c in db.CheckItem
                                                                  on x.CheckItemUniqueID equals c.UniqueID
                                                                  where x.RouteUniqueID == jobRoute.UniqueID && x.ControlPointUniqueID == routeControlPoint.UniqueID
                                                                  select new
                                                                  {
                                                                      c.UniqueID,
                                                                      c.CheckType,
                                                                      c.ID,
                                                                      c.Description,
                                                                      x.Seq
                                                                  }).ToList();

                            foreach (var routeControlPointCheckItem in routeControlPointCheckItemList)
                            {
                                var jobControlPointCheckItem = jobControlPointCheckItemList.FirstOrDefault(x => x.ControlPointUniqueID == routeControlPoint.UniqueID && x.CheckItemUniqueID == routeControlPointCheckItem.UniqueID);

                                controlPointModel.CheckItemList.Add(new CheckItemModel()
                                {
                                    IsChecked = jobControlPointCheckItem != null,
                                    UniqueID = routeControlPointCheckItem.UniqueID,
                                    CheckType = routeControlPointCheckItem.CheckType,
                                    CheckItemID = routeControlPointCheckItem.ID,
                                    CheckItemDescription = routeControlPointCheckItem.Description,
                                    Seq = routeControlPointCheckItem.Seq
                                });
                            }

                            controlPointModel.CheckItemList = controlPointModel.CheckItemList.OrderBy(x => x.Seq).ToList();

                            routeModel.ControlPointList.Add(controlPointModel);
                        }

                        routeModel.ControlPointList = routeModel.ControlPointList.OrderBy(x => x.Seq).ToList();

                        model.RouteList.Add(routeModel);
                    }
                }

                using (DbEntities db = new DbEntities())
                {
                    model.UserList = (from user in model.UserList
                                      join u in db.User
                                      on user.ID equals u.ID
                                      join o in db.Organization
                                      on u.OrganizationUniqueID equals o.UniqueID
                                      select new Models.GuardPatrol.JobManagement.UserModel
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

        public static RequestResult GetEditFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new EditFormModel();

                using (GDbEntities db = new GDbEntities())
                {
                    var job = db.Job.First(x => x.UniqueID == UniqueID);

                    model = new EditFormModel()
                    {
                        UniqueID = job.UniqueID,
                         AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(job.OrganizationUniqueID),
                        OrganizationUniqueID = job.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(job.OrganizationUniqueID),
                        FormInput = new FormInput()
                        {
                            Description = job.Description,
                            IsCheckBySeq = job.IsCheckBySeq,
                            IsNeedVerify = job.IsNeedVerify,
                            IsShowPrevRecord = job.IsShowPrevRecord,
                            BeginDateString = DateTimeHelper.DateTime2DateStringWithSeperator(job.BeginDate),
                            EndDateString = DateTimeHelper.DateTime2DateStringWithSeperator(job.EndDate),
                            TimeMode = job.TimeMode,
                            BeginTime = job.BeginTime,
                            EndTime = job.EndTime,
                            CycleCount = job.CycleCount,
                            CycleMode = job.CycleMode,
                            Remark = job.Remark
                        },
                        UserList = db.JobUser.Where(x => x.JobUniqueID == UniqueID).Select(x => new Models.GuardPatrol.JobManagement.UserModel
                        {
                            ID = x.UserID
                        }).ToList()
                    };

                    var jobRouteList = (from x in db.JobRoute
                                        join r in db.Route
                                        on x.RouteUniqueID equals r.UniqueID
                                        where x.JobUniqueID == job.UniqueID
                                        select new
                                        {
                                            r.UniqueID,
                                            x.IsOptional,
                                            r.ID,
                                            r.Name
                                        }).ToList();

                    foreach (var jobRoute in jobRouteList)
                    {
                        var routeModel = new RouteModel()
                        {
                            UniqueID = jobRoute.UniqueID,
                            ID = jobRoute.ID,
                            Name = jobRoute.Name,
                            IsOptional = jobRoute.IsOptional
                        };

                        var jobControlPointList = db.JobControlPoint.Where(x => x.JobUniqueID == job.UniqueID && x.RouteUniqueID == jobRoute.UniqueID).ToList();
                        var jobControlPointCheckItemList = db.JobControlPointCheckItem.Where(x => x.JobUniqueID == UniqueID && x.RouteUniqueID == jobRoute.UniqueID).ToList();

                        var routeControlPointList = (from x in db.RouteControlPoint
                                                     join c in db.ControlPoint
                                                     on x.ControlPointUniqueID equals c.UniqueID
                                                     where x.RouteUniqueID == jobRoute.UniqueID
                                                     select new
                                                     {
                                                         c.UniqueID,
                                                         c.ID,
                                                         c.Description,
                                                         x.Seq
                                                     }).ToList();

                        foreach (var routeControlPoint in routeControlPointList)
                        {
                            var jobControlPoint = jobControlPointList.FirstOrDefault(x => x.ControlPointUniqueID == routeControlPoint.UniqueID);

                            var controlPointModel = new ControlPointModel()
                            {
                                UniqueID = routeControlPoint.UniqueID,
                                ID = routeControlPoint.ID,
                                Description = routeControlPoint.Description,
                                MinTimeSpan = jobControlPoint != null ? jobControlPoint.MinTimeSpan : default(int?),
                                Seq = routeControlPoint.Seq
                            };

                            var routeControlPointCheckItemList = (from x in db.RouteControlPointCheckItem
                                                                  join c in db.CheckItem
                                                                  on x.CheckItemUniqueID equals c.UniqueID
                                                                  where x.RouteUniqueID == jobRoute.UniqueID && x.ControlPointUniqueID == routeControlPoint.UniqueID
                                                                  select new
                                                                  {
                                                                      c.UniqueID,
                                                                      c.CheckType,
                                                                      c.ID,
                                                                      c.Description,
                                                                      x.Seq
                                                                  }).ToList();

                            foreach (var routeControlPointCheckItem in routeControlPointCheckItemList)
                            {
                                var jobControlPointCheckItem = jobControlPointCheckItemList.FirstOrDefault(x => x.ControlPointUniqueID == routeControlPoint.UniqueID && x.CheckItemUniqueID == routeControlPointCheckItem.UniqueID);

                                controlPointModel.CheckItemList.Add(new CheckItemModel()
                                {
                                    IsChecked = jobControlPointCheckItem != null,
                                    UniqueID = routeControlPointCheckItem.UniqueID,
                                    CheckType = routeControlPointCheckItem.CheckType,
                                    CheckItemID = routeControlPointCheckItem.ID,
                                    CheckItemDescription = routeControlPointCheckItem.Description,
                                    Seq = routeControlPointCheckItem.Seq
                                });
                            }

                            controlPointModel.CheckItemList = controlPointModel.CheckItemList.OrderBy(x => x.Seq).ToList();

                            routeModel.ControlPointList.Add(controlPointModel);
                        }

                        routeModel.ControlPointList = routeModel.ControlPointList.OrderBy(x => x.Seq).ToList();

                        model.RouteList.Add(routeModel);
                    }
                }

                using (DbEntities db = new DbEntities())
                {
                    model.UserList = (from user in model.UserList
                                      join u in db.User
                                      on user.ID equals u.ID
                                      join o in db.Organization
                                      on u.OrganizationUniqueID equals o.UniqueID
                                      select new Models.GuardPatrol.JobManagement.UserModel
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

        public static RequestResult Edit(EditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (GDbEntities db = new GDbEntities())
                {
                    var job = db.Job.First(x => x.UniqueID == Model.UniqueID);

                    var exists = db.Job.FirstOrDefault(x => x.UniqueID != job.UniqueID && x.OrganizationUniqueID == job.OrganizationUniqueID && x.Description == Model.FormInput.Description);

                    if (exists == null)
                    {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                        #region Job
                        job.Description = Model.FormInput.Description;
                        job.IsCheckBySeq = Model.FormInput.IsCheckBySeq;
                        job.IsNeedVerify = Model.FormInput.IsNeedVerify;
                        job.IsShowPrevRecord = Model.FormInput.IsShowPrevRecord;
                        job.BeginDate = Model.FormInput.BeginDate;
                        job.EndDate = Model.FormInput.EndDate;
                        job.TimeMode = Model.FormInput.TimeMode;
                        job.BeginTime = Model.FormInput.BeginTime;
                        job.EndTime = Model.FormInput.EndTime;
                        job.CycleCount = Model.FormInput.CycleCount;
                        job.CycleMode = Model.FormInput.CycleMode;
                        job.LastModifyTime = DateTime.Now;

                        db.SaveChanges();
                        #endregion

                        #region JobUser
                        #region Delete
                        db.JobUser.RemoveRange(db.JobUser.Where(x => x.JobUniqueID == Model.UniqueID).ToList());

                        db.SaveChanges();
                        #endregion

                        #region Insert
                        db.JobUser.AddRange(Model.UserList.Select(x => new JobUser
                        {
                            JobUniqueID = job.UniqueID,
                            UserID = x.ID
                        })).ToList();
                        #endregion
                        #endregion

                        #region JobRoute
                        var jobRouteList = db.JobRoute.Where(x => x.JobUniqueID == Model.UniqueID).ToList();

                        foreach (var jobRoute in jobRouteList)
                        {
                            var route = Model.RouteList.FirstOrDefault(x => x.UniqueID == jobRoute.RouteUniqueID);

                            if (route == null || !route.ControlPointList.Any(x => x.IsChecked))
                            {
                                db.JobRoute.Remove(jobRoute);
                            }
                        }
                        #endregion

                        #region JobControlPoint, JobControlPointCheckItem
                        #region Delete
                        db.JobControlPoint.RemoveRange(db.JobControlPoint.Where(x => x.JobUniqueID == job.UniqueID).ToList());
                        db.JobControlPointCheckItem.RemoveRange(db.JobControlPointCheckItem.Where(x => x.JobUniqueID == job.UniqueID).ToList());

                        db.SaveChanges();
                        #endregion

                        #region Insert
                        foreach (var route in Model.RouteList)
                        {
                            if (route.ControlPointList.Any(x => x.IsChecked))
                            {
                                var jobRoute = db.JobRoute.FirstOrDefault(x => x.JobUniqueID == job.UniqueID && x.RouteUniqueID == route.UniqueID);

                                if (jobRoute == null)
                                {
                                    db.JobRoute.Add(new JobRoute()
                                    {
                                        JobUniqueID = Model.UniqueID,
                                        RouteUniqueID = route.UniqueID,
                                        IsOptional = route.IsOptional,
                                        UniqueID = Guid.NewGuid().ToString()
                                    });
                                }

                                foreach (var controlPoint in route.ControlPointList)
                                {
                                    if (controlPoint.CheckItemList.Count(x => x.IsChecked) > 0)
                                    {
                                        db.JobControlPoint.Add(new JobControlPoint()
                                        {
                                            JobUniqueID = Model.UniqueID,
                                            RouteUniqueID = route.UniqueID,
                                            ControlPointUniqueID = controlPoint.UniqueID,
                                            MinTimeSpan = controlPoint.MinTimeSpan
                                        });

                                        foreach (var checkItem in controlPoint.CheckItemList)
                                        {
                                            if (checkItem.IsChecked)
                                            {
                                                db.JobControlPointCheckItem.Add(new JobControlPointCheckItem()
                                                {
                                                    JobUniqueID = Model.UniqueID,
                                                    RouteUniqueID = route.UniqueID,
                                                    ControlPointUniqueID = controlPoint.UniqueID,
                                                    CheckItemUniqueID = checkItem.UniqueID
                                                });
                                            }
                                        }
                                    }
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
                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.Job, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.JobDescription, Resources.Resource.Exists));
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

        public static RequestResult Delete(List<string> SelectedList)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (GDbEntities db = new GDbEntities())
                {
                    foreach (var uniqueID in SelectedList)
                    {
                        //Job
                        db.Job.Remove(db.Job.First(x => x.UniqueID == uniqueID));

                        //JobControlPoint
                        db.JobControlPoint.RemoveRange(db.JobControlPoint.Where(x => x.JobUniqueID == uniqueID).ToList());

                        //JobControlPointCheckItem
                        db.JobControlPointCheckItem.RemoveRange(db.JobControlPointCheckItem.Where(x => x.JobUniqueID == uniqueID).ToList());

                        //JobRoute
                        db.JobRoute.RemoveRange(db.JobRoute.Where(x => x.JobUniqueID == uniqueID).ToList());

                        //JobUser
                        db.JobUser.RemoveRange(db.JobUser.Where(x => x.JobUniqueID == uniqueID).ToList());
                    }

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.Job, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult SavePageState(List<RouteModel> RouteList, List<string> RoutePageStateList)
        {
            RequestResult result = new RequestResult();

            try
            {
                foreach (var routePageState in RoutePageStateList)
                {
                    string[] temp = routePageState.Split(Define.Seperators, StringSplitOptions.None);

                    string uniqueID = temp[0];
                    string isOptional = temp[1];

                    RouteList.First(x => x.UniqueID == uniqueID).IsOptional = isOptional == "Y";
                }

                result.ReturnData(RouteList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult SavePageState(List<RouteModel> RouteList, string RouteUniqueID, List<string> ControlPointPageStateList, List<string> ControlPointCheckItemPageStateList)
        {
            RequestResult result = new RequestResult();

            try
            {
                foreach (string controlPointPageState in ControlPointPageStateList)
                {
                    string[] temp = controlPointPageState.Split(Define.Seperators, StringSplitOptions.None);

                    string uniqueID = temp[0];
                    int? minTimeSpan = !string.IsNullOrEmpty(temp[1]) ? int.Parse(temp[1]) : default(int?);

                    RouteList.First(x => x.UniqueID == RouteUniqueID).ControlPointList.First(x => x.UniqueID == uniqueID).MinTimeSpan = minTimeSpan;
                }

                foreach (var controlPointCheckItemPageState in ControlPointCheckItemPageStateList)
                {
                    string[] temp = controlPointCheckItemPageState.Split(Define.Seperators, StringSplitOptions.None);

                    string controlPointUniqueID = temp[0];
                    string checkItemUniqueID = temp[1];
                    string isChecked = temp[2];

                    RouteList.First(x => x.UniqueID == RouteUniqueID).ControlPointList.First(x => x.UniqueID == controlPointUniqueID).CheckItemList.First(x => x.UniqueID == checkItemUniqueID).IsChecked = isChecked == "Y";
                }

                result.ReturnData(RouteList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult AddUser(List<Models.GuardPatrol.JobManagement.UserModel> UserList, List<string> SelectedList,Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                string[] seperator = new string[] { Define.Seperator };

                using (DbEntities db = new DbEntities())
                {
                    foreach (string selected in SelectedList)
                    {
                        string[] temp = selected.Split(seperator, StringSplitOptions.None);

                        var organizationUniqueID = temp[0];
                        var userID = temp[1];

                        if (!string.IsNullOrEmpty(userID))
                        {
                            if (!UserList.Any(x => x.ID == userID))
                            {
                                UserList.Add((from u in db.User
                                              join o in db.Organization
                                              on u.OrganizationUniqueID equals o.UniqueID
                                              where u.ID == userID
                                              select new Models.GuardPatrol.JobManagement.UserModel
                                              {
                                                  ID = u.ID,
                                                  Name = u.Name,
                                                  OrganizationDescription = o.Description
                                              }).First());
                            }
                        }
                        else
                        {
                            var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organizationUniqueID, true);

                            var organizationList = Account.QueryableOrganizationUniqueIDList.Intersect(downStreamOrganizationList);

                            var userList = (from u in db.User
                                            join o in db.Organization
                                           on u.OrganizationUniqueID equals o.UniqueID
                                            where organizationList.Contains(u.OrganizationUniqueID)
                                            select new
                                            {
                                                ID = u.ID,
                                                Name = u.Name,
                                                OrganizationDescription = o.Description
                                            }).ToList();

                            foreach (var user in userList)
                            {
                                if (!UserList.Any(x => x.ID == user.ID))
                                {
                                    UserList.Add(new Models.GuardPatrol.JobManagement.UserModel()
                                    {
                                        ID = user.ID,
                                        Name = user.Name,
                                        OrganizationDescription = user.OrganizationDescription
                                    });
                                }
                            }
                        }
                    }
                }

                result.ReturnData(UserList.OrderBy(x => x.OrganizationDescription).ThenBy(x => x.ID).ToList());
            }
            catch (Exception ex)
            {
                var err = new Error(MethodInfo.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult AddRoute(List<RouteModel> RouteList, List<string> SelectedList, string RefOrganizationUniqueID)
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
                        var routeUniqueID = temp[1];

                        if (!string.IsNullOrEmpty(routeUniqueID))
                        {
                            if (!RouteList.Any(x => x.UniqueID == routeUniqueID))
                            {
                                var route = db.Route.First(x => x.UniqueID == routeUniqueID);

                                var routeModel = new RouteModel()
                                {
                                    UniqueID = route.UniqueID,
                                    ID = route.ID,
                                    Name = route.Name,
                                    IsOptional = false
                                };

                                var controlPointList = (from x in db.RouteControlPoint
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
                                                        }).OrderBy(x => x.Seq).ToList();

                                foreach (var controlPoint in controlPointList)
                                {
                                    controlPoint.CheckItemList = (from x in db.RouteControlPointCheckItem
                                                                  join c in db.CheckItem
                                                                  on x.CheckItemUniqueID equals c.UniqueID
                                                                  where x.RouteUniqueID == route.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID
                                                                  select new CheckItemModel
                                                                  {
                                                                      UniqueID = c.UniqueID,
                                                                      CheckItemID = c.ID,
                                                                      CheckItemDescription = c.Description,
                                                                      CheckType = c.CheckType,
                                                                      Seq = x.Seq,
                                                                      IsChecked = true
                                                                  }).OrderBy(x => x.Seq).ToList();

                                    routeModel.ControlPointList.Add(controlPoint);
                                }

                                RouteList.Add(routeModel);
                            }
                        }
                        else
                        {
                            var availableOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(RefOrganizationUniqueID, true);

                            var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organizationUniqueID, true);

                            foreach (var downStreamOrganization in downStreamOrganizationList)
                            {
                                if (availableOrganizationList.Any(x => x == downStreamOrganization))
                                {
                                    var routeList = db.Route.Where(x => x.OrganizationUniqueID == downStreamOrganization).ToList();

                                    foreach (var route in routeList)
                                    {
                                        if (!RouteList.Any(x => x.UniqueID == route.UniqueID))
                                        {
                                            var routeModel = new RouteModel()
                                            {
                                                UniqueID = route.UniqueID,
                                                ID = route.ID,
                                                Name = route.Name,
                                                IsOptional = false
                                            };

                                            var controlPointList = (from x in db.RouteControlPoint
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
                                                                    }).OrderBy(x => x.Seq).ToList();

                                            foreach (var controlPoint in controlPointList)
                                            {
                                                controlPoint.CheckItemList = (from x in db.RouteControlPointCheckItem
                                                                              join c in db.CheckItem
                                                                              on x.CheckItemUniqueID equals c.UniqueID
                                                                              where x.RouteUniqueID == route.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID
                                                                              select new CheckItemModel
                                                                              {
                                                                                  UniqueID = c.UniqueID,
                                                                                  CheckItemID = c.ID,
                                                                                  CheckItemDescription = c.Description,
                                                                                  CheckType = c.CheckType,
                                                                                  Seq = x.Seq,
                                                                                  IsChecked = true
                                                                              }).OrderBy(x => x.Seq).ToList();

                                                routeModel.ControlPointList.Add(controlPoint);
                                            }

                                            RouteList.Add(routeModel);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                result.ReturnData(RouteList.OrderBy(x => x.ID).ToList());
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
