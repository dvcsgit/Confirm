using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.Shared;
using Models.Authenticated;
using Models.EquipmentMaintenance.RouteManagement;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.EquipmentMaintenance
{
    public class RouteDataAccessor
    {
        public static RequestResult AddUser(List<ManagerModel> ManagerList, List<string> SelectedList)
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
                            if (!ManagerList.Any(x => x.ID == userID))
                            {
                                ManagerList.Add((from u in db.User
                                              join o in db.Organization
                                                  on u.OrganizationUniqueID equals o.UniqueID
                                              where u.ID == userID
                                              select new ManagerModel
                                              {
                                                  ID = u.ID,
                                                  Name = u.Name,
                                                  OrganizationDescription = o.Description
                                              }).First());
                            }
                        }
                        else
                        {
                            var organizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organizationUniqueID, true);

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
                                if (!ManagerList.Any(x => x.ID == user.ID))
                                {
                                    ManagerList.Add(new ManagerModel()
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

                result.ReturnData(ManagerList.OrderBy(x => x.OrganizationDescription).ThenBy(x => x.ID).ToList());
            }
            catch (Exception ex)
            {
                var err = new Error(MethodInfo.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
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
                
                using (EDbEntities db = new EDbEntities())
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
                        JobList = db.Job.Where(x => x.RouteUniqueID == UniqueID).Select(x => new Models.EquipmentMaintenance.RouteManagement.Job
                        {
                            BeginDate = x.BeginDate,
                            BeginTime = x.BeginTime,
                            CycleCount = x.CycleCount,
                            CycleMode = x.CycleMode,
                            Description = x.Description,
                            EndDate = x.EndDate,
                            EndTime = x.EndTime,
                            IsCheckBySeq = x.IsCheckBySeq,
                            IsNeedVerify = x.IsNeedVerify,
                            IsShowPrevRecord = x.IsShowPrevRecord,
                            LastModifyTime = x.LastModifyTime,
                            Remark = x.Remark,
                            RouteUniqueID = x.RouteUniqueID,
                            TimeMode = x.TimeMode,
                            UniqueID = x.UniqueID
                        }).OrderBy(x => x.Description).ToList(),
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

                using (EDbEntities db = new EDbEntities())
                {
                    var route = db.Route.First(x => x.UniqueID == UniqueID);

                    model = new CreateFormModel()
                    {
                        AncestorOrganizationUniqueID =OrganizationDataAccessor.GetAncestorOrganizationUniqueID(route.OrganizationUniqueID),
                        OrganizationUniqueID = route.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(route.OrganizationUniqueID),
                        ControlPointList = (from rc in db.RouteControlPoint
                                            join controlPoint in db.ControlPoint
                                            on rc.ControlPointUniqueID equals controlPoint.UniqueID
                                            where rc.RouteUniqueID == route.UniqueID
                                            select new ControlPointModel
                                            {
                                                UniqueID = controlPoint.UniqueID,
                                                ID = controlPoint.ID,
                                                Description = controlPoint.Description,
                                                MinTimeSpan = rc.MinTimeSpan,
                                                Seq = rc.Seq,
                                                EquipmentList = (from re in db.RouteEquipment
                                                                 join equipment in db.Equipment
                                                                 on re.EquipmentUniqueID equals equipment.UniqueID
                                                                 join part in db.EquipmentPart
                                                                 on re.PartUniqueID equals part.UniqueID into tmpPart
                                                                 from part in tmpPart.DefaultIfEmpty()
                                                                 where re.RouteUniqueID == route.UniqueID && re.ControlPointUniqueID == controlPoint.UniqueID
                                                                 select new EquipmentModel
                                                                 {
                                                                     EquipmentUniqueID = equipment.UniqueID,
                                                                     PartUniqueID = part != null ? part.UniqueID : "*",
                                                                     EquipmentID = equipment.ID,
                                                                     EquipmentName = equipment.Name,
                                                                     PartDescription = part != null ? part.Description : "",
                                                                     Seq = re.Seq
                                                                 }).OrderBy(x => x.Seq).ToList()
                                            }).OrderBy(x => x.Seq).ToList(),
                        ManagerList = db.RouteManager.Where(x => x.RouteUniqueID == UniqueID).Select(x => new ManagerModel()
                        {
                            ID = x.UserID
                        }).ToList()
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

                        foreach (var equipment in controlPoint.EquipmentList)
                        {
                            var equipmentCheckItemList = (from x in db.EquipmentCheckItem
                                                          join c in db.CheckItem
                                                          on x.CheckItemUniqueID equals c.UniqueID
                                                          where x.EquipmentUniqueID == equipment.EquipmentUniqueID && x.PartUniqueID == equipment.PartUniqueID
                                                          select c).ToList();

                            foreach (var checkItem in equipmentCheckItemList)
                            {
                                var routeEquipmentCheckItem = db.RouteEquipmentCheckItem.FirstOrDefault(x => x.RouteUniqueID == route.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == equipment.EquipmentUniqueID && x.PartUniqueID == equipment.PartUniqueID && x.CheckItemUniqueID == checkItem.UniqueID);

                                equipment.CheckItemList.Add(new CheckItemModel()
                                {
                                    IsChecked = routeEquipmentCheckItem != null,
                                    UniqueID = checkItem.UniqueID,
                                    CheckType = checkItem.CheckType,
                                    CheckItemID = checkItem.ID,
                                    CheckItemDescription = checkItem.Description,
                                    Seq = routeEquipmentCheckItem != null ? routeEquipmentCheckItem.Seq : int.MaxValue
                                });
                            }

                            equipment.CheckItemList = equipment.CheckItemList.OrderBy(x => x.Seq).ToList();
                        }
                    }
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

        public static RequestResult Create(CreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
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

                        db.RouteManager.AddRange(Model.ManagerList.Select(x => new RouteManager()
                        {
                            RouteUniqueID = uniqueID,
                            UserID = x.ID
                        }).ToList());

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

                            int equipmentSeq = 1;

                            foreach (var equipment in controlPoint.EquipmentList)
                            {
                                db.RouteEquipment.Add(new RouteEquipment()
                                {
                                    RouteUniqueID = uniqueID,
                                    ControlPointUniqueID = controlPoint.UniqueID,
                                    EquipmentUniqueID = equipment.EquipmentUniqueID,
                                    PartUniqueID = equipment.PartUniqueID,
                                    Seq = equipmentSeq
                                });

                                equipmentSeq++;

                                int equipmentCheckItemSeq = 1;

                                foreach (var checkItem in equipment.CheckItemList)
                                {
                                    if (checkItem.IsChecked)
                                    {
                                        db.RouteEquipmentCheckItem.Add(new RouteEquipmentCheckItem
                                        {
                                            RouteUniqueID = uniqueID,
                                            ControlPointUniqueID = controlPoint.UniqueID,
                                            EquipmentUniqueID = equipment.EquipmentUniqueID,
                                            PartUniqueID = equipment.PartUniqueID,
                                            CheckItemUniqueID = checkItem.UniqueID,
                                            Seq = equipmentCheckItemSeq
                                        });

                                        equipmentCheckItemSeq++;
                                    }
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

                using (EDbEntities db = new EDbEntities())
                {
                    var route = db.Route.First(x => x.UniqueID == UniqueID);

                    model = new EditFormModel()
                    {
                        AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(route.OrganizationUniqueID),
                        UniqueID = route.UniqueID,
                        OrganizationUniqueID = route.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(route.OrganizationUniqueID),
                        FormInput = new FormInput()
                        {
                            ID = route.ID,
                            Name = route.Name
                        },
                        ControlPointList = (from rc in db.RouteControlPoint
                                            join controlPoint in db.ControlPoint
                                            on rc.ControlPointUniqueID equals controlPoint.UniqueID
                                            where rc.RouteUniqueID == route.UniqueID
                                            select new ControlPointModel
                                            {
                                                UniqueID = controlPoint.UniqueID,
                                                ID = controlPoint.ID,
                                                Description = controlPoint.Description,
                                                MinTimeSpan = rc.MinTimeSpan,
                                                Seq = rc.Seq,
                                                EquipmentList = (from re in db.RouteEquipment
                                                                 join equipment in db.Equipment
                                                                 on re.EquipmentUniqueID equals equipment.UniqueID
                                                                 join part in db.EquipmentPart
                                                                 on re.PartUniqueID equals part.UniqueID into tmpPart
                                                                 from part in tmpPart.DefaultIfEmpty()
                                                                 where re.RouteUniqueID == route.UniqueID && re.ControlPointUniqueID == controlPoint.UniqueID
                                                                 select new EquipmentModel
                                                                 {
                                                                     EquipmentUniqueID = equipment.UniqueID,
                                                                     PartUniqueID = part != null ? part.UniqueID : "*",
                                                                     EquipmentID = equipment.ID,
                                                                     EquipmentName = equipment.Name,
                                                                     PartDescription = part != null ? part.Description : "",
                                                                     Seq = re.Seq
                                                                 }).OrderBy(x => x.Seq).ToList()
                                            }).OrderBy(x => x.Seq).ToList(),
                        ManagerList = db.RouteManager.Where(x => x.RouteUniqueID == UniqueID).Select(x => new ManagerModel()
                        {
                            ID = x.UserID
                        }).ToList()
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

                        foreach (var equipment in controlPoint.EquipmentList)
                        {
                            var equipmentCheckItemList = (from x in db.EquipmentCheckItem
                                                          join c in db.CheckItem
                                                          on x.CheckItemUniqueID equals c.UniqueID
                                                          where x.EquipmentUniqueID == equipment.EquipmentUniqueID && x.PartUniqueID == equipment.PartUniqueID
                                                          select c).ToList();

                            foreach (var checkItem in equipmentCheckItemList)
                            {
                                var routeEquipmentCheckItem = db.RouteEquipmentCheckItem.FirstOrDefault(x => x.RouteUniqueID == route.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == equipment.EquipmentUniqueID && x.PartUniqueID == equipment.PartUniqueID && x.CheckItemUniqueID == checkItem.UniqueID);

                                equipment.CheckItemList.Add(new CheckItemModel()
                                {
                                    IsChecked = routeEquipmentCheckItem != null,
                                    UniqueID = checkItem.UniqueID,
                                    CheckType = checkItem.CheckType,
                                    CheckItemID = checkItem.ID,
                                    CheckItemDescription = checkItem.Description,
                                    Seq = routeEquipmentCheckItem != null ? routeEquipmentCheckItem.Seq : int.MaxValue
                                });
                            }

                            equipment.CheckItemList = equipment.CheckItemList.OrderBy(x => x.Seq).ToList();
                        }
                    }
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

        public static RequestResult Edit(EditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
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
                        route.LastModifyTime = DateTime.Now;

                        db.SaveChanges();
                        #endregion

                        #region RouteControlPoint, RouteControlPointCheckItem, RouteEquipment, RouteEquipmentCheckItem
                        #region Delete
                        db.RouteManager.RemoveRange(db.RouteManager.Where(x => x.RouteUniqueID == route.UniqueID).ToList());
                        db.RouteControlPoint.RemoveRange(db.RouteControlPoint.Where(x => x.RouteUniqueID == route.UniqueID).ToList());
                        db.RouteControlPointCheckItem.RemoveRange(db.RouteControlPointCheckItem.Where(x => x.RouteUniqueID == route.UniqueID).ToList());
                        db.RouteEquipment.RemoveRange(db.RouteEquipment.Where(x => x.RouteUniqueID == route.UniqueID).ToList());
                        db.RouteEquipmentCheckItem.RemoveRange(db.RouteEquipmentCheckItem.Where(x => x.RouteUniqueID == route.UniqueID).ToList());

                        db.SaveChanges();
                        #endregion

                        db.RouteManager.AddRange(Model.ManagerList.Select(x => new RouteManager()
                        {
                            RouteUniqueID = route.UniqueID,
                            UserID = x.ID
                        }).ToList());

                        db.SaveChanges();

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

                            int equipmentSeq = 1;

                            foreach (var equipment in controlPoint.EquipmentList)
                            {
                                db.RouteEquipment.Add(new RouteEquipment()
                                {
                                    RouteUniqueID = route.UniqueID,
                                    ControlPointUniqueID = controlPoint.UniqueID,
                                    EquipmentUniqueID = equipment.EquipmentUniqueID,
                                    PartUniqueID = equipment.PartUniqueID,
                                    Seq = equipmentSeq
                                });

                                equipmentSeq++;

                                int equipmentCheckItemSeq = 1;

                                foreach (var checkItem in equipment.CheckItemList)
                                {
                                    if (checkItem.IsChecked)
                                    {
                                        db.RouteEquipmentCheckItem.Add(new RouteEquipmentCheckItem
                                        {
                                            RouteUniqueID = route.UniqueID,
                                            ControlPointUniqueID = controlPoint.UniqueID,
                                            EquipmentUniqueID = equipment.EquipmentUniqueID,
                                            PartUniqueID = equipment.PartUniqueID,
                                            CheckItemUniqueID = checkItem.UniqueID,
                                            Seq = equipmentCheckItemSeq
                                        });

                                        equipmentCheckItemSeq++;
                                    }
                                }
                            }
                        }

                        db.SaveChanges();
                        #endregion
                        #endregion

                        #region JonControlPoint, JobControlPointCheckItem, JobEquipment, JobEquipmentCheckItem
                        var jobList = db.Job.Where(x => x.RouteUniqueID == route.UniqueID).ToList();

                        foreach (var job in jobList)
                        {
                            var jobControlPointList = db.JobControlPoint.Where(x => x.JobUniqueID == job.UniqueID).ToList();

                            foreach (var jobControlPoint in jobControlPointList)
                            {
                                var routeControlPoint = Model.ControlPointList.FirstOrDefault(x => x.UniqueID == jobControlPoint.ControlPointUniqueID);

                                var jobControlPointCheckItemList = db.JobControlPointCheckItem.Where(x => x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == jobControlPoint.ControlPointUniqueID).ToList();

                                var jobEquipmentList = db.JobEquipment.Where(x => x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == jobControlPoint.ControlPointUniqueID).ToList();

                                if (routeControlPoint != null)
                                {
                                    foreach (var jobControlPointCheckItem in jobControlPointCheckItemList)
                                    {
                                        if (!routeControlPoint.CheckItemList.Where(x => x.IsChecked).Any(x => x.UniqueID == jobControlPointCheckItem.CheckItemUniqueID))
                                        {
                                            db.JobControlPointCheckItem.Remove(jobControlPointCheckItem);
                                        }
                                    }

                                    foreach (var jobEquipment in jobEquipmentList)
                                    {
                                        var routeEquipment = routeControlPoint.EquipmentList.FirstOrDefault(x => x.EquipmentUniqueID == jobEquipment.EquipmentUniqueID && x.PartUniqueID == jobEquipment.PartUniqueID);

                                        if (routeEquipment != null)
                                        {
                                            var jobEquipmentCheckItemList = db.JobEquipmentCheckItem.Where(x => x.JobUniqueID == jobEquipment.JobUniqueID && x.ControlPointUniqueID == jobEquipment.ControlPointUniqueID && x.EquipmentUniqueID == jobEquipment.EquipmentUniqueID && x.PartUniqueID == jobEquipment.PartUniqueID).ToList();

                                            foreach (var jobEquipmentCheckItem in jobEquipmentCheckItemList)
                                            {
                                                if (!routeEquipment.CheckItemList.Where(x => x.IsChecked).Any(x => x.UniqueID == jobEquipmentCheckItem.CheckItemUniqueID))
                                                {
                                                    db.JobEquipmentCheckItem.Remove(jobEquipmentCheckItem);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            db.JobEquipment.Remove(jobEquipment);
                                            db.JobEquipmentCheckItem.RemoveRange(db.JobEquipmentCheckItem.Where(x => x.JobUniqueID == jobEquipment.JobUniqueID && x.ControlPointUniqueID == jobEquipment.ControlPointUniqueID && x.EquipmentUniqueID == jobEquipment.EquipmentUniqueID && x.PartUniqueID == jobEquipment.PartUniqueID).ToList());
                                        }
                                    }
                                }
                                else
                                {
                                    db.JobControlPoint.Remove(jobControlPoint);

                                    db.JobControlPointCheckItem.RemoveRange(jobControlPointCheckItemList);

                                    foreach (var jobEquipment in jobEquipmentList)
                                    {
                                        db.JobEquipment.Remove(jobEquipment);

                                        db.JobEquipmentCheckItem.RemoveRange(db.JobEquipmentCheckItem.Where(x => x.JobUniqueID == jobEquipment.JobUniqueID && x.ControlPointUniqueID == jobEquipment.ControlPointUniqueID && x.EquipmentUniqueID == jobEquipment.EquipmentUniqueID && x.PartUniqueID == jobEquipment.PartUniqueID).ToList());
                                    }
                                }
                            }
                        }

                        db.SaveChanges();

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

        public static RequestResult SavePageState(List<ControlPointModel> ControlPointList, List<string> ControlPointPageStateList, List<string> ControlPointCheckItemPageStateList, List<string> EquipmentPageStateList, List<string> EquipmentCheckItemPageStateList)
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

                foreach (var equipmentPageState in EquipmentPageStateList)
                {
                    string[] temp = equipmentPageState.Split(Define.Seperators, StringSplitOptions.None);

                    string controlPointUniqueID = temp[0];
                    string equipmentUniqueID = temp[1];
                    string partUniqueID = temp[2];
                    int seq = int.Parse(temp[3]) + 1;

                    ControlPointList.First(x => x.UniqueID == controlPointUniqueID).EquipmentList.First(x => x.EquipmentUniqueID == equipmentUniqueID && x.PartUniqueID == partUniqueID).Seq = seq;
                }

                foreach (var equipmentCheckItemPageState in EquipmentCheckItemPageStateList)
                {
                    string[] temp = equipmentCheckItemPageState.Split(Define.Seperators, StringSplitOptions.None);

                    string controlPointUniqueID = temp[0];
                    string equipmentUniqueID = temp[1];
                    string partUniqueID = temp[2];
                    string checkItemUniqueID = temp[3];
                    int seq = int.Parse(temp[4]) + 1;
                    string isChecked = temp[5];

                    var checkItem = ControlPointList.First(x => x.UniqueID == controlPointUniqueID).EquipmentList.First(x => x.EquipmentUniqueID == equipmentUniqueID && x.PartUniqueID == partUniqueID).CheckItemList.First(x => x.UniqueID == checkItemUniqueID);

                    checkItem.IsChecked = isChecked == "Y";
                    checkItem.Seq = seq;
                }

                foreach (var controlPoint in ControlPointList)
                {
                    foreach (var equipment in controlPoint.EquipmentList)
                    {
                        equipment.CheckItemList = equipment.CheckItemList.OrderBy(x => x.Seq).ToList();
                    }

                    controlPoint.EquipmentList = controlPoint.EquipmentList.OrderBy(x => x.Seq).ToList();
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

        public static RequestResult Delete(List<string> SelectedList)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    DeleteHelper.Route(db, SelectedList);

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

        public static RequestResult AddControlPoint(List<ControlPointModel> ControlPointList, List<string> SelectedList, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
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
                                                                 select c).OrderBy(x=>x.CheckType).ThenBy(x => x.ID).ToList();

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
                    foreach (var equipment in controlPoint.EquipmentList)
                    {
                        equipment.CheckItemList = equipment.CheckItemList.OrderBy(x => x.Seq).ToList();
                    }

                    controlPoint.EquipmentList = controlPoint.EquipmentList.OrderBy(x => x.Seq).ToList();
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

        public static RequestResult AddEquipment(List<ControlPointModel> ControlPointList, string ControlPointUniqueID, List<string> SelectedList, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var controlPointModel = ControlPointList.First(x => x.UniqueID == ControlPointUniqueID);

                using (EDbEntities db = new EDbEntities())
                {
                    foreach (string selected in SelectedList)
                    {
                        string[] temp = selected.Split(Define.Seperators, StringSplitOptions.None);

                        var organizationUniqueID = temp[0];
                        var equipmentUniqueID = temp[1];
                        var partUniqueID = temp[2];

                        if (!string.IsNullOrEmpty(partUniqueID) && partUniqueID != "*")
                        {
                            if (!ControlPointList.Any(x => x.EquipmentList.Any(e => e.EquipmentUniqueID == equipmentUniqueID && e.PartUniqueID == partUniqueID)))
                            {
                                var equipment = (from p in db.EquipmentPart
                                                 join e in db.Equipment
                                                 on p.EquipmentUniqueID equals e.UniqueID
                                                 where p.UniqueID == partUniqueID
                                                 select new
                                                 {
                                                     EquipmentUniqueID = e.UniqueID,
                                                     PartUniqueID = p.UniqueID,
                                                     EquipmentID = e.ID,
                                                     EquipmentName = e.Name,
                                                     PartDescription = p.Description
                                                 }).First();

                                var equipmentModel = new EquipmentModel()
                                {
                                    EquipmentUniqueID = equipment.EquipmentUniqueID,
                                    PartUniqueID = equipment.PartUniqueID,
                                    EquipmentID = equipment.EquipmentID,
                                    EquipmentName = equipment.EquipmentName,
                                    PartDescription = equipment.PartDescription,
                                    Seq = controlPointModel.EquipmentList.Count + 1
                                };

                                var checkItemList = (from x in db.EquipmentCheckItem
                                                     join c in db.CheckItem
                                                     on x.CheckItemUniqueID equals c.UniqueID
                                                     where x.EquipmentUniqueID == equipmentUniqueID && x.PartUniqueID == partUniqueID
                                                     select c).OrderBy(x => x.CheckType).ThenBy(x => x.ID).ToList();

                                int seq = 1;

                                foreach (var checkItem in checkItemList)
                                {
                                    equipmentModel.CheckItemList.Add(new CheckItemModel()
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

                                controlPointModel.EquipmentList.Add(equipmentModel);
                            }
                        }
                        else if (!string.IsNullOrEmpty(equipmentUniqueID))
                        {
                            if (!ControlPointList.Any(x => x.EquipmentList.Any(e => e.EquipmentUniqueID == equipmentUniqueID && e.PartUniqueID == "*")))
                            {
                                var equipment = db.Equipment.First(x => x.UniqueID == equipmentUniqueID);

                                var equipmentModel = new EquipmentModel()
                                {
                                    EquipmentUniqueID = equipment.UniqueID,
                                    PartUniqueID = "*",
                                    EquipmentID = equipment.ID,
                                    EquipmentName = equipment.Name,
                                    PartDescription = "",
                                    Seq = controlPointModel.EquipmentList.Count + 1
                                };

                                var checkItemList = (from x in db.EquipmentCheckItem
                                                     join c in db.CheckItem
                                                     on x.CheckItemUniqueID equals c.UniqueID
                                                     where x.EquipmentUniqueID == equipmentUniqueID && x.PartUniqueID == "*"
                                                     select c).OrderBy(x => x.CheckType).ThenBy(x => x.ID).ToList();

                                int seq = 1;

                                foreach (var checkItem in checkItemList)
                                {
                                    equipmentModel.CheckItemList.Add(new CheckItemModel()
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

                                controlPointModel.EquipmentList.Add(equipmentModel);
                            }

                            //var partList = (from p in db.EquipmentPart
                            //                join e in db.Equipment
                            //                on p.EquipmentUniqueID equals e.UniqueID
                            //                where p.EquipmentUniqueID == equipmentUniqueID
                            //                select new
                            //                {
                            //                    EquipmentUniqueID = e.UniqueID,
                            //                    PartUniqueID = p.UniqueID,
                            //                    EquipmentID = e.ID,
                            //                    EquipmentName = e.Name,
                            //                    PartDescription = p.Description
                            //                }).OrderBy(x => x.PartDescription).ToList();

                            //foreach (var part in partList)
                            //{
                            //    if (!ControlPointList.Any(x => x.EquipmentList.Any(e => e.EquipmentUniqueID == part.EquipmentUniqueID && e.PartUniqueID == part.PartUniqueID)))
                            //    {
                            //        var equipmentModel = new EquipmentModel()
                            //        {
                            //            EquipmentUniqueID = part.EquipmentUniqueID,
                            //            PartUniqueID = part.PartUniqueID,
                            //            EquipmentID = part.EquipmentID,
                            //            EquipmentName = part.EquipmentName,
                            //            PartDescription = part.PartDescription,
                            //            Seq = controlPointModel.EquipmentList.Count + 1
                            //        };

                            //        var checkItemList = (from x in db.EquipmentCheckItem
                            //                             join c in db.CheckItem
                            //                             on x.CheckItemUniqueID equals c.UniqueID
                            //                             where x.EquipmentUniqueID == part.EquipmentUniqueID && x.PartUniqueID==part.PartUniqueID
                            //                             select c).OrderBy(x => x.CheckType).ThenBy(x => x.ID).ToList();

                            //        int seq = 1;

                            //        foreach (var checkItem in checkItemList)
                            //        {
                            //            equipmentModel.CheckItemList.Add(new CheckItemModel()
                            //            {
                            //                IsChecked = true,
                            //                UniqueID = checkItem.UniqueID,
                            //                CheckType = checkItem.CheckType,
                            //                CheckItemID = checkItem.ID,
                            //                CheckItemDescription = checkItem.Description,
                            //                Seq = seq
                            //            });

                            //            seq++;
                            //        }

                            //        controlPointModel.EquipmentList.Add(equipmentModel);
                            //    }
                            //}
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
                                    var equipmentList = db.Equipment.Where(x => x.OrganizationUniqueID == organization).OrderBy(x => x.ID).ToList();

                                    foreach (var equipment in equipmentList)
                                    {
                                        if (!ControlPointList.Any(x => x.EquipmentList.Any(e => e.EquipmentUniqueID == equipment.UniqueID && e.PartUniqueID == "*")))
                                        {
                                            var equipmentModel = new EquipmentModel()
                                            {
                                                EquipmentUniqueID = equipment.UniqueID,
                                                PartUniqueID = "*",
                                                EquipmentID = equipment.ID,
                                                EquipmentName = equipment.Name,
                                                PartDescription = "",
                                                Seq = controlPointModel.EquipmentList.Count + 1
                                            };

                                            var checkItemList = (from x in db.EquipmentCheckItem
                                                                 join c in db.CheckItem
                                                                 on x.CheckItemUniqueID equals c.UniqueID
                                                                 where x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == "*"
                                                                 select c).OrderBy(x => x.CheckType).ThenBy(x => x.ID).ToList();

                                            int seq = 1;

                                            foreach (var checkItem in checkItemList)
                                            {
                                                equipmentModel.CheckItemList.Add(new CheckItemModel()
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

                                            controlPointModel.EquipmentList.Add(equipmentModel);
                                        }

                                        var partList = (from p in db.EquipmentPart
                                                        join e in db.Equipment
                                                        on p.EquipmentUniqueID equals e.UniqueID
                                                        where p.EquipmentUniqueID == equipment.UniqueID
                                                        select new
                                                        {
                                                            EquipmentUniqueID = e.UniqueID,
                                                            PartUniqueID = p.UniqueID,
                                                            EquipmentID = e.ID,
                                                            EquipmentName = e.Name,
                                                            PartDescription = p.Description
                                                        }).OrderBy(x => x.PartDescription).ToList();

                                        foreach (var part in partList)
                                        {
                                            if (!ControlPointList.Any(x => x.EquipmentList.Any(e => e.EquipmentUniqueID == part.EquipmentUniqueID && e.PartUniqueID == part.PartUniqueID)))
                                            {
                                                var equipmentModel = new EquipmentModel()
                                                {
                                                    EquipmentUniqueID = part.EquipmentUniqueID,
                                                    PartUniqueID = part.PartUniqueID,
                                                    EquipmentID = part.EquipmentID,
                                                    EquipmentName = part.EquipmentName,
                                                    PartDescription = part.PartDescription,
                                                    Seq = controlPointModel.EquipmentList.Count + 1
                                                };

                                                var checkItemList = (from x in db.EquipmentCheckItem
                                                                     join c in db.CheckItem
                                                                     on x.CheckItemUniqueID equals c.UniqueID
                                                                     where x.EquipmentUniqueID == part.EquipmentUniqueID && x.PartUniqueID == part.PartUniqueID
                                                                     select c).OrderBy(x => x.CheckType).ThenBy(x => x.ID).ToList();

                                                int seq = 1;

                                                foreach (var checkItem in checkItemList)
                                                {
                                                    equipmentModel.CheckItemList.Add(new CheckItemModel()
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

                                                controlPointModel.EquipmentList.Add(equipmentModel);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (var controlPoint in ControlPointList)
                {
                    foreach (var equipment in controlPoint.EquipmentList)
                    {
                        equipment.CheckItemList = equipment.CheckItemList.OrderBy(x => x.Seq).ToList();
                    }

                    controlPoint.EquipmentList = controlPoint.EquipmentList.OrderBy(x => x.Seq).ToList();
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

        public static RequestResult GetRootTreeItem(List<Models.Shared.Organization> OrganizationList, string OrganizationUniqueID, Account Account)
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
                    { Define.EnumTreeAttribute.RouteUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.JobUniqueID, string.Empty },
                };

                using (EDbEntities edb = new EDbEntities())
                {
                    var organization = OrganizationList.First(x => x.UniqueID == OrganizationUniqueID);

                    var treeItem = new TreeItem() { Title = organization.Description };

                    attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                    attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                    attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                    attributes[Define.EnumTreeAttribute.RouteUniqueID] = string.Empty;
                    attributes[Define.EnumTreeAttribute.JobUniqueID] = string.Empty;

                    foreach (var attribute in attributes)
                    {
                        treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                    }

                    if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                        ||
                        (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && edb.Route.Any(x => x.OrganizationUniqueID == organization.UniqueID)))
                    {
                        treeItem.State = "closed";
                    }

                    treeItemList.Add(treeItem);
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

        public static RequestResult GetTreeItem(List<Models.Shared.Organization> OrganizationList, string OrganizationUniqueID, string RouteUniqueID, Account Account)
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
                    { Define.EnumTreeAttribute.RouteUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.JobUniqueID, string.Empty },
                };

                using (EDbEntities edb = new EDbEntities())
                {
                    if (string.IsNullOrEmpty(RouteUniqueID))
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
                                attributes[Define.EnumTreeAttribute.JobUniqueID] = string.Empty;

                                foreach (var attribute in attributes)
                                {
                                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                }

                                if (edb.Job.Any(x => x.RouteUniqueID == route.UniqueID))
                                {
                                    treeItem.State = "closed";
                                }

                                treeItemList.Add(treeItem);
                            }
                        }

                        var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                        foreach (var organization in organizationList)
                        {
                            var treeItem = new TreeItem() { Title = organization.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                            attributes[Define.EnumTreeAttribute.RouteUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.JobUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                                ||
                                (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && edb.Route.Any(x => x.OrganizationUniqueID == organization.UniqueID)))
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else
                    {
                        var jobList = edb.Job.Where(x => x.RouteUniqueID == RouteUniqueID).OrderBy(x => x.Description).ToList();

                        foreach (var job in jobList)
                        {
                            var treeItem = new TreeItem() { Title = job.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Job.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = job.Description;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.RouteUniqueID] = RouteUniqueID;
                            attributes[Define.EnumTreeAttribute.JobUniqueID] = job.UniqueID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
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
                    { Define.EnumTreeAttribute.ControlPointUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.EquipmentUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.PartUniqueID, string.Empty }
                };

                using (EDbEntities db = new EDbEntities())
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
                        attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = string.Empty;
                        attributes[Define.EnumTreeAttribute.PartUniqueID] = string.Empty;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        if (db.RouteEquipment.Any(x => x.RouteUniqueID == RouteUniqueID && x.ControlPointUniqueID == controlPoint.UniqueID) 
                            || 
                            db.RouteControlPointCheckItem.Any(x => x.RouteUniqueID == RouteUniqueID && x.ControlPointUniqueID == controlPoint.UniqueID))
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

        public static RequestResult GetDetailTreeItem(string RouteUniqueID, string ControlPointUniqueID, string EquipmentUniqueID, string PartUniqueID)
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
                    { Define.EnumTreeAttribute.EquipmentUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.PartUniqueID, string.Empty }
                };

                using (EDbEntities db = new EDbEntities())
                {
                    if (!string.IsNullOrEmpty(PartUniqueID) && PartUniqueID!="*")
                    {
                        var checkItemList = (from x in db.RouteEquipmentCheckItem
                                             join c in db.CheckItem
                                             on x.CheckItemUniqueID equals c.UniqueID
                                             where x.RouteUniqueID == RouteUniqueID && x.ControlPointUniqueID == ControlPointUniqueID && x.EquipmentUniqueID == EquipmentUniqueID && x.PartUniqueID == PartUniqueID
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
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = EquipmentUniqueID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = PartUniqueID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else if (!string.IsNullOrEmpty(EquipmentUniqueID))
                    {
                        var checkItemList = (from x in db.RouteEquipmentCheckItem
                                             join c in db.CheckItem
                                             on x.CheckItemUniqueID equals c.UniqueID
                                             where x.RouteUniqueID == RouteUniqueID && x.ControlPointUniqueID == ControlPointUniqueID && x.EquipmentUniqueID == EquipmentUniqueID && x.PartUniqueID=="*"
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
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = EquipmentUniqueID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else
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
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }

                        var equipmentList = (from x in db.RouteEquipment
                                             join e in db.Equipment
                                             on x.EquipmentUniqueID equals e.UniqueID
                                             join p in db.EquipmentPart
                                             on x.PartUniqueID equals p.UniqueID into tmpPart
                                             from p in tmpPart.DefaultIfEmpty()
                                             where x.RouteUniqueID==RouteUniqueID&&x.ControlPointUniqueID==ControlPointUniqueID
                                             select new
                                             {
                                                 EquipmentUniqueID = e.UniqueID,
                                                 PartUniqueID = p != null ? p.UniqueID : "*",
                                                 EquipmentID = e.ID,
                                                 EquipmentName = e.Name,
                                                 PartDescription = p != null ? p.Description : "",
                                                 x.Seq
                                             }).OrderBy(x => x.Seq).ToList();

                        foreach (var equipment in equipmentList)
                        {
                            var treeItem = new TreeItem();

                            if (equipment.PartUniqueID=="*")
                            {
                                treeItem.Title = equipment.EquipmentName;
                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Equipment.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", equipment.EquipmentID, equipment.EquipmentName);
                            }
                            else
                            {
                                treeItem.Title = string.Format("{0}-{1}", equipment.EquipmentName, equipment.PartDescription);
                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.EquipmentPart.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}-{2}", equipment.EquipmentID, equipment.EquipmentName, equipment.PartDescription);
                            }

                            attributes[Define.EnumTreeAttribute.RouteUniqueID] = RouteUniqueID;
                            attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = ControlPointUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = equipment.EquipmentUniqueID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = equipment.PartUniqueID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (db.RouteEquipmentCheckItem.Any(x => x.RouteUniqueID == RouteUniqueID && x.ControlPointUniqueID == ControlPointUniqueID && x.EquipmentUniqueID == equipment.EquipmentUniqueID && x.PartUniqueID == equipment.PartUniqueID))
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
    }
}
