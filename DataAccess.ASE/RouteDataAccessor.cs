using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.ASE;
using Models.Shared;
using Models.Authenticated;
using Models.EquipmentMaintenance.RouteManagement;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.ASE
{
    public class RouteDataAccessor
    {
        public static RequestResult AddUser(List<ManagerModel> ManagerList, List<string> SelectedList)
        {
            RequestResult result = new RequestResult();

            try
            {
                string[] seperator = new string[] { Define.Seperator };

                using (ASEDbEntities db = new ASEDbEntities())
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
                                ManagerList.Add((from u in db.ACCOUNT
                                              join o in db.ORGANIZATION
                                                  on u.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                              where u.ID == userID
                                              select new ManagerModel
                                              {
                                                  ID = u.ID,
                                                  Name = u.NAME,
                                                  OrganizationDescription = o.DESCRIPTION
                                              }).First());
                            }
                        }
                        else
                        {
                            var organizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organizationUniqueID, true);

                            var userList = (from u in db.ACCOUNT
                                            join o in db.ORGANIZATION
                                           on u.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                            where organizationList.Contains(u.ORGANIZATIONUNIQUEID)
                                            select new
                                            {
                                                ID = u.ID,
                                                Name = u.NAME,
                                                OrganizationDescription = o.DESCRIPTION
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.ROUTE.Where(x => downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID) && Account.QueryableOrganizationUniqueIDList.Contains(x.ORGANIZATIONUNIQUEID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.ID.Contains(Parameters.Keyword) || x.NAME.Contains(Parameters.Keyword));
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
                            UniqueID = x.UNIQUEID,
                            Permission = Account.OrganizationPermission(x.ORGANIZATIONUNIQUEID),
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.ORGANIZATIONUNIQUEID),
                            ID = x.ID,
                            Name = x.NAME
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
                
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var route = db.ROUTE.First(x => x.UNIQUEID == UniqueID);

                    model = new DetailViewModel()
                    {
                        UniqueID = route.UNIQUEID,
                        Permission = Account.OrganizationPermission(route.ORGANIZATIONUNIQUEID),
                        OrganizationUniqueID = route.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(route.ORGANIZATIONUNIQUEID),
                        ID = route.ID,
                        Name = route.NAME,
                        JobList = db.JOB.Where(x => x.ROUTEUNIQUEID == UniqueID).Select(x => new Models.EquipmentMaintenance.RouteManagement.Job
                        {
                            BeginDate = x.BEGINDATE.Value,
                            BeginTime = x.BEGINTIME,
                            CycleCount = x.CYCLECOUNT.Value,
                            CycleMode = x.CYCLEMODE,
                            Description = x.DESCRIPTION,
                            EndDate = x.ENDDATE,
                            EndTime = x.ENDTIME,
                            IsCheckBySeq = x.ISCHECKBYSEQ == "Y",
                            IsNeedVerify = x.ISNEEDVERIFY == "Y",
                            IsShowPrevRecord = x.ISSHOWPREVRECORD == "Y",
                            LastModifyTime = x.LASTMODIFYTIME.Value,
                            Remark = x.REMARK,
                            RouteUniqueID = x.ROUTEUNIQUEID,
                            TimeMode = x.TIMEMODE.Value,
                            UniqueID = x.UNIQUEID
                        }).OrderBy(x => x.Description).ToList(),
                        ManagerList = db.ROUTEMANAGER.Where(x => x.ROUTEUNIQUEID == UniqueID).Select(x => new ManagerModel()
                        {
                            ID = x.USERID
                        }).ToList()
                    };   
                }

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    model.ManagerList = (from m in model.ManagerList
                                         join u in db.ACCOUNT
                                         on m.ID equals u.ID
                                         join o in db.ORGANIZATION
                                         on u.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                         select new ManagerModel
                                         {
                                             OrganizationDescription = o.DESCRIPTION,
                                             ID = u.ID,
                                             Name = u.NAME
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var route = db.ROUTE.First(x => x.UNIQUEID == UniqueID);

                    model = new CreateFormModel()
                    {
                        AncestorOrganizationUniqueID =OrganizationDataAccessor.GetAncestorOrganizationUniqueID(route.ORGANIZATIONUNIQUEID),
                        OrganizationUniqueID = route.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(route.ORGANIZATIONUNIQUEID),
                        ControlPointList = (from rc in db.ROUTECONTROLPOINT
                                            join controlPoint in db.CONTROLPOINT
                                            on rc.CONTROLPOINTUNIQUEID equals controlPoint.UNIQUEID
                                            where rc.ROUTEUNIQUEID == route.UNIQUEID
                                            select new ControlPointModel
                                            {
                                                UniqueID = controlPoint.UNIQUEID,
                                                ID = controlPoint.ID,
                                                Description = controlPoint.DESCRIPTION,
                                                MinTimeSpan = rc.MINTIMESPAN,
                                                Seq = rc.SEQ.Value,
                                                EquipmentList = (from re in db.ROUTEEQUIPMENT
                                                                 join equipment in db.EQUIPMENT
                                                                 on re.EQUIPMENTUNIQUEID equals equipment.UNIQUEID
                                                                 join part in db.EQUIPMENTPART
                                                                 on re.PARTUNIQUEID equals part.UNIQUEID into tmpPart
                                                                 from part in tmpPart.DefaultIfEmpty()
                                                                 where re.ROUTEUNIQUEID == route.UNIQUEID && re.CONTROLPOINTUNIQUEID == controlPoint.UNIQUEID
                                                                 select new EquipmentModel
                                                                 {
                                                                     EquipmentUniqueID = equipment.UNIQUEID,
                                                                     PartUniqueID = part != null ? part.UNIQUEID : "*",
                                                                     EquipmentID = equipment.ID,
                                                                     EquipmentName = equipment.NAME,
                                                                     PartDescription = part != null ? part.DESCRIPTION : "",
                                                                     Seq = re.SEQ.Value
                                                                 }).OrderBy(x => x.Seq).ToList()
                                            }).OrderBy(x => x.Seq).ToList(),
                        ManagerList = db.ROUTEMANAGER.Where(x => x.ROUTEUNIQUEID == UniqueID).Select(x => new ManagerModel()
                        {
                            ID = x.USERID
                        }).ToList()
                    };

                    foreach (var controlPoint in model.ControlPointList)
                    {
                        var controlPointCheckItemList = (from x in db.CONTROLPOINTCHECKITEM
                                                         join c in db.CHECKITEM
                                                         on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                                         where x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID
                                                         select c).ToList();

                        foreach (var checkItem in controlPointCheckItemList)
                        {
                            var routeControlPointCheckItem = db.ROUTECONTROLPOINTCHECKITEM.FirstOrDefault(x => x.ROUTEUNIQUEID == route.UNIQUEID && x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID && x.CHECKITEMUNIQUEID == checkItem.UNIQUEID);

                            controlPoint.CheckItemList.Add(new CheckItemModel()
                            {
                                IsChecked = routeControlPointCheckItem != null,
                                UniqueID = checkItem.UNIQUEID,
                                CheckType = checkItem.CHECKTYPE,
                                CheckItemID = checkItem.ID,
                                CheckItemDescription = checkItem.DESCRIPTION,
                                Seq = routeControlPointCheckItem != null ? routeControlPointCheckItem.SEQ.Value : int.MaxValue
                            });
                        }

                        controlPoint.CheckItemList = controlPoint.CheckItemList.OrderBy(x => x.Seq).ToList();

                        foreach (var equipment in controlPoint.EquipmentList)
                        {
                            var equipmentCheckItemList = (from x in db.EQUIPMENTCHECKITEM
                                                          join c in db.CHECKITEM
                                                          on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                                          where x.EQUIPMENTUNIQUEID == equipment.EquipmentUniqueID && x.PARTUNIQUEID == equipment.PartUniqueID
                                                          select c).ToList();

                            foreach (var checkItem in equipmentCheckItemList)
                            {
                                var routeEquipmentCheckItem = db.ROUTEEQUIPMENTCHECKITEM.FirstOrDefault(x => x.ROUTEUNIQUEID == route.UNIQUEID && x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID && x.EQUIPMENTUNIQUEID == equipment.EquipmentUniqueID && x.PARTUNIQUEID == equipment.PartUniqueID && x.CHECKITEMUNIQUEID == checkItem.UNIQUEID);

                                equipment.CheckItemList.Add(new CheckItemModel()
                                {
                                    IsChecked = routeEquipmentCheckItem != null,
                                    UniqueID = checkItem.UNIQUEID,
                                    CheckType = checkItem.CHECKTYPE,
                                    CheckItemID = checkItem.ID,
                                    CheckItemDescription = checkItem.DESCRIPTION,
                                    Seq = routeEquipmentCheckItem != null ? routeEquipmentCheckItem.SEQ.Value : int.MaxValue
                                });
                            }

                            equipment.CheckItemList = equipment.CheckItemList.OrderBy(x => x.Seq).ToList();
                        }
                    }
                }

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    model.ManagerList = (from m in model.ManagerList
                                         join u in db.ACCOUNT
                                         on m.ID equals u.ID
                                         join o in db.ORGANIZATION
                                         on u.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                         select new ManagerModel
                                         {
                                             OrganizationDescription = o.DESCRIPTION,
                                             ID = u.ID,
                                             Name = u.NAME
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var exists = db.ROUTE.FirstOrDefault(x => x.ORGANIZATIONUNIQUEID == Model.OrganizationUniqueID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
                        string uniqueID = Guid.NewGuid().ToString();

                        db.ROUTE.Add(new ROUTE()
                        {
                            UNIQUEID = uniqueID,
                            ORGANIZATIONUNIQUEID = Model.OrganizationUniqueID,
                            ID = Model.FormInput.ID,
                            NAME = Model.FormInput.Name,
                            LASTMODIFYTIME = DateTime.Now
                        });

                        db.ROUTEMANAGER.AddRange(Model.ManagerList.Select(x => new ROUTEMANAGER()
                        {
                            ROUTEUNIQUEID = uniqueID,
                            USERID = x.ID
                        }).ToList());

                        int controlPointSeq = 1;

                        foreach (var controlPoint in Model.ControlPointList)
                        {
                            db.ROUTECONTROLPOINT.Add(new ROUTECONTROLPOINT()
                            {
                                ROUTEUNIQUEID = uniqueID,
                                CONTROLPOINTUNIQUEID = controlPoint.UniqueID,
                                MINTIMESPAN = controlPoint.MinTimeSpan,
                                SEQ = controlPointSeq
                            });

                            controlPointSeq++;

                            int controlPointCheckItemSeq = 1;

                            foreach (var checkItem in controlPoint.CheckItemList)
                            {
                                if (checkItem.IsChecked)
                                {
                                    db.ROUTECONTROLPOINTCHECKITEM.Add(new ROUTECONTROLPOINTCHECKITEM
                                    {
                                        ROUTEUNIQUEID = uniqueID,
                                        CONTROLPOINTUNIQUEID = controlPoint.UniqueID,
                                        CHECKITEMUNIQUEID = checkItem.UniqueID,
                                        SEQ = controlPointCheckItemSeq
                                    });

                                    controlPointCheckItemSeq++;
                                }
                            }

                            int equipmentSeq = 1;

                            foreach (var equipment in controlPoint.EquipmentList)
                            {
                                db.ROUTEEQUIPMENT.Add(new ROUTEEQUIPMENT()
                                {
                                    ROUTEUNIQUEID = uniqueID,
                                    CONTROLPOINTUNIQUEID = controlPoint.UniqueID,
                                    EQUIPMENTUNIQUEID = equipment.EquipmentUniqueID,
                                    PARTUNIQUEID = equipment.PartUniqueID,
                                    SEQ = equipmentSeq
                                });

                                equipmentSeq++;

                                int equipmentCheckItemSeq = 1;

                                foreach (var checkItem in equipment.CheckItemList)
                                {
                                    if (checkItem.IsChecked)
                                    {
                                        db.ROUTEEQUIPMENTCHECKITEM.Add(new ROUTEEQUIPMENTCHECKITEM
                                        {
                                            ROUTEUNIQUEID = uniqueID,
                                            CONTROLPOINTUNIQUEID = controlPoint.UniqueID,
                                            EQUIPMENTUNIQUEID = equipment.EquipmentUniqueID,
                                            PARTUNIQUEID = equipment.PartUniqueID,
                                            CHECKITEMUNIQUEID = checkItem.UniqueID,
                                            SEQ = equipmentCheckItemSeq
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var route = db.ROUTE.First(x => x.UNIQUEID == UniqueID);

                    model = new EditFormModel()
                    {
                        AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(route.ORGANIZATIONUNIQUEID),
                        UniqueID = route.UNIQUEID,
                        OrganizationUniqueID = route.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(route.ORGANIZATIONUNIQUEID),
                        FormInput = new FormInput()
                        {
                            ID = route.ID,
                            Name = route.NAME
                        },
                        ControlPointList = (from rc in db.ROUTECONTROLPOINT
                                            join controlPoint in db.CONTROLPOINT
                                            on rc.CONTROLPOINTUNIQUEID equals controlPoint.UNIQUEID
                                            where rc.ROUTEUNIQUEID == route.UNIQUEID
                                            select new ControlPointModel
                                            {
                                                UniqueID = controlPoint.UNIQUEID,
                                                ID = controlPoint.ID,
                                                Description = controlPoint.DESCRIPTION,
                                                MinTimeSpan = rc.MINTIMESPAN,
                                                Seq = rc.SEQ.Value,
                                                EquipmentList = (from re in db.ROUTEEQUIPMENT
                                                                 join equipment in db.EQUIPMENT
                                                                 on re.EQUIPMENTUNIQUEID equals equipment.UNIQUEID
                                                                 join part in db.EQUIPMENTPART
                                                                 on re.PARTUNIQUEID equals part.UNIQUEID into tmpPart
                                                                 from part in tmpPart.DefaultIfEmpty()
                                                                 where re.ROUTEUNIQUEID == route.UNIQUEID && re.CONTROLPOINTUNIQUEID == controlPoint.UNIQUEID
                                                                 select new EquipmentModel
                                                                 {
                                                                     EquipmentUniqueID = equipment.UNIQUEID,
                                                                     PartUniqueID = part != null ? part.UNIQUEID : "*",
                                                                     EquipmentID = equipment.ID,
                                                                     EquipmentName = equipment.NAME,
                                                                     PartDescription = part != null ? part.DESCRIPTION : "",
                                                                     Seq = re.SEQ.Value
                                                                 }).OrderBy(x => x.Seq).ToList()
                                            }).OrderBy(x => x.Seq).ToList(),
                        ManagerList = db.ROUTEMANAGER.Where(x => x.ROUTEUNIQUEID == UniqueID).Select(x => new ManagerModel()
                        {
                            ID = x.USERID
                        }).ToList()
                    };

                    foreach (var controlPoint in model.ControlPointList)
                    {
                        var controlPointCheckItemList = (from x in db.CONTROLPOINTCHECKITEM
                                                         join c in db.CHECKITEM
                                                         on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                                         where x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID
                                                         select c).ToList();

                        foreach (var checkItem in controlPointCheckItemList)
                        {
                            var routeControlPointCheckItem = db.ROUTECONTROLPOINTCHECKITEM.FirstOrDefault(x => x.ROUTEUNIQUEID == route.UNIQUEID && x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID && x.CHECKITEMUNIQUEID == checkItem.UNIQUEID);

                            controlPoint.CheckItemList.Add(new CheckItemModel()
                            {
                                IsChecked = routeControlPointCheckItem != null,
                                UniqueID = checkItem.UNIQUEID,
                                CheckType = checkItem.CHECKTYPE,
                                CheckItemID = checkItem.ID,
                                CheckItemDescription = checkItem.DESCRIPTION,
                                Seq = routeControlPointCheckItem != null ? routeControlPointCheckItem.SEQ.Value : int.MaxValue
                            });
                        }

                        controlPoint.CheckItemList = controlPoint.CheckItemList.OrderBy(x => x.Seq).ToList();

                        foreach (var equipment in controlPoint.EquipmentList)
                        {
                            var equipmentCheckItemList = (from x in db.EQUIPMENTCHECKITEM
                                                          join c in db.CHECKITEM
                                                          on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                                          where x.EQUIPMENTUNIQUEID == equipment.EquipmentUniqueID && x.PARTUNIQUEID == equipment.PartUniqueID
                                                          select c).ToList();

                            foreach (var checkItem in equipmentCheckItemList)
                            {
                                var routeEquipmentCheckItem = db.ROUTEEQUIPMENTCHECKITEM.FirstOrDefault(x => x.ROUTEUNIQUEID == route.UNIQUEID && x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID && x.EQUIPMENTUNIQUEID == equipment.EquipmentUniqueID && x.PARTUNIQUEID == equipment.PartUniqueID && x.CHECKITEMUNIQUEID == checkItem.UNIQUEID);

                                equipment.CheckItemList.Add(new CheckItemModel()
                                {
                                    IsChecked = routeEquipmentCheckItem != null,
                                    UniqueID = checkItem.UNIQUEID,
                                    CheckType = checkItem.CHECKTYPE,
                                    CheckItemID = checkItem.ID,
                                    CheckItemDescription = checkItem.DESCRIPTION,
                                    Seq = routeEquipmentCheckItem != null ? routeEquipmentCheckItem.SEQ.Value : int.MaxValue
                                });
                            }

                            equipment.CheckItemList = equipment.CheckItemList.OrderBy(x => x.Seq).ToList();
                        }
                    }
                }

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    model.ManagerList = (from m in model.ManagerList
                                         join u in db.ACCOUNT
                                         on m.ID equals u.ID
                                         join o in db.ORGANIZATION
                                         on u.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                         select new ManagerModel
                                         {
                                             OrganizationDescription = o.DESCRIPTION,
                                             ID = u.ID,
                                             Name = u.NAME
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var route = db.ROUTE.First(x => x.UNIQUEID == Model.UniqueID);

                    var exists = db.ROUTE.FirstOrDefault(x => x.UNIQUEID != route.UNIQUEID && x.ORGANIZATIONUNIQUEID == route.ORGANIZATIONUNIQUEID && x.ID == Model.FormInput.ID);

                    if (exists ==null)
                    {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                        #region Route
                        route.ID = Model.FormInput.ID;
                        route.NAME = Model.FormInput.Name;
                        route.LASTMODIFYTIME = DateTime.Now;

                        db.SaveChanges();
                        #endregion

                        #region RouteControlPoint, RouteControlPointCheckItem, RouteEquipment, RouteEquipmentCheckItem
                        #region Delete
                        db.ROUTEMANAGER.RemoveRange(db.ROUTEMANAGER.Where(x => x.ROUTEUNIQUEID == route.UNIQUEID).ToList());
                        db.ROUTECONTROLPOINT.RemoveRange(db.ROUTECONTROLPOINT.Where(x => x.ROUTEUNIQUEID == route.UNIQUEID).ToList());
                        db.ROUTECONTROLPOINTCHECKITEM.RemoveRange(db.ROUTECONTROLPOINTCHECKITEM.Where(x => x.ROUTEUNIQUEID == route.UNIQUEID).ToList());
                        db.ROUTEEQUIPMENT.RemoveRange(db.ROUTEEQUIPMENT.Where(x => x.ROUTEUNIQUEID == route.UNIQUEID).ToList());
                        db.ROUTEEQUIPMENTCHECKITEM.RemoveRange(db.ROUTEEQUIPMENTCHECKITEM.Where(x => x.ROUTEUNIQUEID == route.UNIQUEID).ToList());

                        db.SaveChanges();
                        #endregion

                        db.ROUTEMANAGER.AddRange(Model.ManagerList.Select(x => new ROUTEMANAGER()
                        {
                            ROUTEUNIQUEID = route.UNIQUEID,
                            USERID = x.ID
                        }).ToList());

                        db.SaveChanges();

                        #region Insert
                        int controlPointSeq = 1;

                        foreach (var controlPoint in Model.ControlPointList)
                        {
                            db.ROUTECONTROLPOINT.Add(new ROUTECONTROLPOINT()
                            {
                                ROUTEUNIQUEID = route.UNIQUEID,
                                CONTROLPOINTUNIQUEID = controlPoint.UniqueID,
                                MINTIMESPAN = controlPoint.MinTimeSpan,
                                SEQ = controlPointSeq
                            });

                            controlPointSeq++;

                            int controlPointCheckItemSeq = 1;

                            foreach (var checkItem in controlPoint.CheckItemList)
                            {
                                if (checkItem.IsChecked)
                                {
                                    db.ROUTECONTROLPOINTCHECKITEM.Add(new ROUTECONTROLPOINTCHECKITEM
                                    {
                                        ROUTEUNIQUEID = route.UNIQUEID,
                                        CONTROLPOINTUNIQUEID = controlPoint.UniqueID,
                                        CHECKITEMUNIQUEID = checkItem.UniqueID,
SEQ= controlPointCheckItemSeq
                                    });

                                    controlPointCheckItemSeq++;
                                }
                            }

                            int equipmentSeq = 1;

                            foreach (var equipment in controlPoint.EquipmentList)
                            {
                                db.ROUTEEQUIPMENT.Add(new ROUTEEQUIPMENT()
                                {
                                    ROUTEUNIQUEID = route.UNIQUEID,
                                    CONTROLPOINTUNIQUEID = controlPoint.UniqueID,
                                    EQUIPMENTUNIQUEID = equipment.EquipmentUniqueID,
                                    PARTUNIQUEID = equipment.PartUniqueID,
                                    SEQ = equipmentSeq
                                });

                                equipmentSeq++;

                                int equipmentCheckItemSeq = 1;

                                foreach (var checkItem in equipment.CheckItemList)
                                {
                                    if (checkItem.IsChecked)
                                    {
                                        db.ROUTEEQUIPMENTCHECKITEM.Add(new ROUTEEQUIPMENTCHECKITEM
                                        {
                                            ROUTEUNIQUEID = route.UNIQUEID,
                                            CONTROLPOINTUNIQUEID = controlPoint.UniqueID,
                                            EQUIPMENTUNIQUEID = equipment.EquipmentUniqueID,
                                            PARTUNIQUEID = equipment.PartUniqueID,
                                            CHECKITEMUNIQUEID = checkItem.UniqueID,
                                            SEQ = equipmentCheckItemSeq
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
                        var jobList = db.JOB.Where(x => x.ROUTEUNIQUEID == route.UNIQUEID).ToList();

                        foreach (var job in jobList)
                        {
                            var jobControlPointList = db.JOBCONTROLPOINT.Where(x => x.JOBUNIQUEID == job.UNIQUEID).ToList();

                            foreach (var jobControlPoint in jobControlPointList)
                            {
                                var routeControlPoint = Model.ControlPointList.FirstOrDefault(x => x.UniqueID == jobControlPoint.CONTROLPOINTUNIQUEID);

                                var jobControlPointCheckItemList = db.JOBCONTROLPOINTCHECKITEM.Where(x => x.JOBUNIQUEID == job.UNIQUEID && x.CONTROLPOINTUNIQUEID == jobControlPoint.CONTROLPOINTUNIQUEID).ToList();

                                var jobEquipmentList = db.JOBEQUIPMENT.Where(x => x.JOBUNIQUEID == job.UNIQUEID && x.CONTROLPOINTUNIQUEID == jobControlPoint.CONTROLPOINTUNIQUEID).ToList();

                                if (routeControlPoint != null)
                                {
                                    foreach (var jobControlPointCheckItem in jobControlPointCheckItemList)
                                    {
                                        if (!routeControlPoint.CheckItemList.Where(x => x.IsChecked).Any(x => x.UniqueID == jobControlPointCheckItem.CHECKITEMUNIQUEID))
                                        {
                                            db.JOBCONTROLPOINTCHECKITEM.Remove(jobControlPointCheckItem);
                                        }
                                    }

                                    foreach (var jobEquipment in jobEquipmentList)
                                    {
                                        var routeEquipment = routeControlPoint.EquipmentList.FirstOrDefault(x => x.EquipmentUniqueID == jobEquipment.EQUIPMENTUNIQUEID && x.PartUniqueID == jobEquipment.PARTUNIQUEID);

                                        if (routeEquipment != null)
                                        {
                                            var jobEquipmentCheckItemList = db.JOBEQUIPMENTCHECKITEM.Where(x => x.JOBUNIQUEID == jobEquipment.JOBUNIQUEID && x.CONTROLPOINTUNIQUEID == jobEquipment.CONTROLPOINTUNIQUEID && x.EQUIPMENTUNIQUEID == jobEquipment.EQUIPMENTUNIQUEID && x.PARTUNIQUEID == jobEquipment.PARTUNIQUEID).ToList();

                                            foreach (var jobEquipmentCheckItem in jobEquipmentCheckItemList)
                                            {
                                                if (!routeEquipment.CheckItemList.Where(x => x.IsChecked).Any(x => x.UniqueID == jobEquipmentCheckItem.CHECKITEMUNIQUEID))
                                                {
                                                    db.JOBEQUIPMENTCHECKITEM.Remove(jobEquipmentCheckItem);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            db.JOBEQUIPMENT.Remove(jobEquipment);
                                            db.JOBEQUIPMENTCHECKITEM.RemoveRange(db.JOBEQUIPMENTCHECKITEM.Where(x => x.JOBUNIQUEID == jobEquipment.JOBUNIQUEID && x.CONTROLPOINTUNIQUEID == jobEquipment.CONTROLPOINTUNIQUEID && x.EQUIPMENTUNIQUEID == jobEquipment.EQUIPMENTUNIQUEID && x.PARTUNIQUEID == jobEquipment.PARTUNIQUEID).ToList());
                                        }
                                    }
                                }
                                else
                                {
                                    db.JOBCONTROLPOINT.Remove(jobControlPoint);

                                    db.JOBCONTROLPOINTCHECKITEM.RemoveRange(jobControlPointCheckItemList);

                                    foreach (var jobEquipment in jobEquipmentList)
                                    {
                                        db.JOBEQUIPMENT.Remove(jobEquipment);

                                        db.JOBEQUIPMENTCHECKITEM.RemoveRange(db.JOBEQUIPMENTCHECKITEM.Where(x => x.JOBUNIQUEID == jobEquipment.JOBUNIQUEID && x.CONTROLPOINTUNIQUEID == jobEquipment.CONTROLPOINTUNIQUEID && x.EQUIPMENTUNIQUEID == jobEquipment.EQUIPMENTUNIQUEID && x.PARTUNIQUEID == jobEquipment.PARTUNIQUEID).ToList());
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
                using (ASEDbEntities db = new ASEDbEntities())
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
                using (ASEDbEntities db = new ASEDbEntities())
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
                                var controlPoint = db.CONTROLPOINT.First(x => x.UNIQUEID == controlPointUniqueID);

                                var controlPointModel = new ControlPointModel()
                                {
                                    UniqueID = controlPoint.UNIQUEID,
                                    ID = controlPoint.ID,
                                    Description = controlPoint.DESCRIPTION,
                                    Seq = ControlPointList.Count + 1
                                };

                                var checkItemList = (from x in db.CONTROLPOINTCHECKITEM
                                                     join c in db.CHECKITEM
                                                     on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                                     where x.CONTROLPOINTUNIQUEID == controlPointUniqueID
                                                     select c).OrderBy(x => x.CHECKTYPE).ThenBy(x => x.ID).ToList();

                                int seq = 1;

                                foreach (var checkItem in checkItemList)
                                {
                                    controlPointModel.CheckItemList.Add(new CheckItemModel()
                                    {
                                        IsChecked = true,
                                        UniqueID = checkItem.UNIQUEID,
                                        CheckType = checkItem.CHECKTYPE,
                                        CheckItemID = checkItem.ID,
                                        CheckItemDescription = checkItem.DESCRIPTION,
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
                                    var controlPointList = db.CONTROLPOINT.Where(x => x.ORGANIZATIONUNIQUEID == organization).OrderBy(x => x.ID).ToList();

                                    foreach (var controlPoint in controlPointList)
                                    {
                                        if (!ControlPointList.Any(x => x.UniqueID == controlPoint.UNIQUEID))
                                        {
                                            var controlPointModel = new ControlPointModel()
                                            {
                                                UniqueID = controlPoint.UNIQUEID,
                                                ID = controlPoint.ID,
                                                Description = controlPoint.DESCRIPTION,
                                                Seq = ControlPointList.Count + 1
                                            };

                                            var checkItemList = (from x in db.CONTROLPOINTCHECKITEM
                                                                 join c in db.CHECKITEM
                                                                 on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                                                 where x.CONTROLPOINTUNIQUEID == controlPoint.UNIQUEID
                                                                 select c).OrderBy(x=>x.CHECKTYPE).ThenBy(x => x.ID).ToList();

                                            int seq = 1;

                                            foreach (var checkItem in checkItemList)
                                            {
                                                controlPointModel.CheckItemList.Add(new CheckItemModel()
                                                {
                                                    IsChecked = true,
                                                    UniqueID = checkItem.UNIQUEID,
                                                    CheckType = checkItem.CHECKTYPE,
                                                    CheckItemID = checkItem.ID,
                                                    CheckItemDescription = checkItem.DESCRIPTION,
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

                using (ASEDbEntities db = new ASEDbEntities())
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
                                var equipment = (from p in db.EQUIPMENTPART
                                                 join e in db.EQUIPMENT
                                                 on p.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                                 where p.UNIQUEID == partUniqueID
                                                 select new
                                                 {
                                                     EquipmentUniqueID = e.UNIQUEID,
                                                     PartUniqueID = p.UNIQUEID,
                                                     EquipmentID = e.ID,
                                                     EquipmentName = e.NAME,
                                                     PartDescription = p.DESCRIPTION
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

                                var checkItemList = (from x in db.EQUIPMENTCHECKITEM
                                                     join c in db.CHECKITEM
                                                     on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                                     where x.EQUIPMENTUNIQUEID == equipmentUniqueID && x.PARTUNIQUEID == partUniqueID
                                                     select c).OrderBy(x => x.CHECKTYPE).ThenBy(x => x.ID).ToList();

                                int seq = 1;

                                foreach (var checkItem in checkItemList)
                                {
                                    equipmentModel.CheckItemList.Add(new CheckItemModel()
                                    {
                                        IsChecked = true,
                                        UniqueID = checkItem.UNIQUEID,
                                        CheckType = checkItem.CHECKTYPE,
                                        CheckItemID = checkItem.ID,
                                        CheckItemDescription = checkItem.DESCRIPTION,
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
                                var equipment = db.EQUIPMENT.First(x => x.UNIQUEID == equipmentUniqueID);

                                var equipmentModel = new EquipmentModel()
                                {
                                    EquipmentUniqueID = equipment.UNIQUEID,
                                    PartUniqueID = "*",
                                    EquipmentID = equipment.ID,
                                    EquipmentName = equipment.NAME,
                                    PartDescription = "",
                                    Seq = controlPointModel.EquipmentList.Count + 1
                                };

                                var checkItemList = (from x in db.EQUIPMENTCHECKITEM
                                                     join c in db.CHECKITEM
                                                     on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                                     where x.EQUIPMENTUNIQUEID == equipmentUniqueID && x.PARTUNIQUEID == "*"
                                                     select c).OrderBy(x => x.CHECKTYPE).ThenBy(x => x.ID).ToList();

                                int seq = 1;

                                foreach (var checkItem in checkItemList)
                                {
                                    equipmentModel.CheckItemList.Add(new CheckItemModel()
                                    {
                                        IsChecked = true,
                                        UniqueID = checkItem.UNIQUEID,
                                        CheckType = checkItem.CHECKTYPE,
                                        CheckItemID = checkItem.ID,
                                        CheckItemDescription = checkItem.DESCRIPTION,
                                        Seq = seq
                                    });

                                    seq++;
                                }

                                controlPointModel.EquipmentList.Add(equipmentModel);
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
                                    var equipmentList = db.EQUIPMENT.Where(x => x.ORGANIZATIONUNIQUEID == organization).OrderBy(x => x.ID).ToList();

                                    foreach (var equipment in equipmentList)
                                    {
                                        if (!ControlPointList.Any(x => x.EquipmentList.Any(e => e.EquipmentUniqueID == equipment.UNIQUEID && e.PartUniqueID == "*")))
                                        {
                                            var equipmentModel = new EquipmentModel()
                                            {
                                                EquipmentUniqueID = equipment.UNIQUEID,
                                                PartUniqueID = "*",
                                                EquipmentID = equipment.ID,
                                                EquipmentName = equipment.NAME,
                                                PartDescription = "",
                                                Seq = controlPointModel.EquipmentList.Count + 1
                                            };

                                            var checkItemList = (from x in db.EQUIPMENTCHECKITEM
                                                                 join c in db.CHECKITEM
                                                                 on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                                                 where x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.PARTUNIQUEID == "*"
                                                                 select c).OrderBy(x => x.CHECKTYPE).ThenBy(x => x.ID).ToList();

                                            int seq = 1;

                                            foreach (var checkItem in checkItemList)
                                            {
                                                equipmentModel.CheckItemList.Add(new CheckItemModel()
                                                {
                                                    IsChecked = true,
                                                    UniqueID = checkItem.UNIQUEID,
                                                    CheckType = checkItem.CHECKTYPE,
                                                    CheckItemID = checkItem.ID,
                                                    CheckItemDescription = checkItem.DESCRIPTION,
                                                    Seq = seq
                                                });

                                                seq++;
                                            }

                                            controlPointModel.EquipmentList.Add(equipmentModel);
                                        }

                                        var partList = (from p in db.EQUIPMENTPART
                                                        join e in db.EQUIPMENT
                                                        on p.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                                        where p.EQUIPMENTUNIQUEID == equipment.UNIQUEID
                                                        select new
                                                        {
                                                            EquipmentUniqueID = e.UNIQUEID,
                                                            PartUniqueID = p.UNIQUEID,
                                                            EquipmentID = e.ID,
                                                            EquipmentName = e.NAME,
                                                            PartDescription = p.DESCRIPTION
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

                                                var checkItemList = (from x in db.EQUIPMENTCHECKITEM
                                                                     join c in db.CHECKITEM
                                                                     on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                                                     where x.EQUIPMENTUNIQUEID == part.EquipmentUniqueID && x.PARTUNIQUEID == part.PartUniqueID
                                                                     select c).OrderBy(x => x.CHECKTYPE).ThenBy(x => x.ID).ToList();

                                                int seq = 1;

                                                foreach (var checkItem in checkItemList)
                                                {
                                                    equipmentModel.CheckItemList.Add(new CheckItemModel()
                                                    {
                                                        IsChecked = true,
                                                        UniqueID = checkItem.UNIQUEID,
                                                        CheckType = checkItem.CHECKTYPE,
                                                        CheckItemID = checkItem.ID,
                                                        CheckItemDescription = checkItem.DESCRIPTION,
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

        public static RequestResult GetRootTreeItem(List<Models.Shared.Organization> OrganizationList, string AncestorOrganizationUniqueID, Account Account)
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var organization = OrganizationList.First(x => x.UniqueID == AncestorOrganizationUniqueID);

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
                        (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.ROUTE.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID)))
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (string.IsNullOrEmpty(RouteUniqueID))
                    {
                        if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                        {
                            var routeList = db.ROUTE.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                            foreach (var route in routeList)
                            {
                                var treeItem = new TreeItem() { Title = route.NAME };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Route.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", route.ID, route.NAME);
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                                attributes[Define.EnumTreeAttribute.RouteUniqueID] = route.UNIQUEID;
                                attributes[Define.EnumTreeAttribute.JobUniqueID] = string.Empty;

                                foreach (var attribute in attributes)
                                {
                                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                }

                                if (db.JOB.Any(x => x.ROUTEUNIQUEID == route.UNIQUEID))
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
                                (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.ROUTE.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID)))
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else
                    {
                        var jobList = db.JOB.Where(x => x.ROUTEUNIQUEID == RouteUniqueID).OrderBy(x => x.DESCRIPTION).ToList();

                        foreach (var job in jobList)
                        {
                            var treeItem = new TreeItem() { Title = job.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Job.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = job.DESCRIPTION;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.RouteUniqueID] = RouteUniqueID;
                            attributes[Define.EnumTreeAttribute.JobUniqueID] = job.UNIQUEID;

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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var controlPointList = (from x in db.ROUTECONTROLPOINT
                                            join c in db.CONTROLPOINT
                                            on x.CONTROLPOINTUNIQUEID equals c.UNIQUEID
                                            where x.ROUTEUNIQUEID == RouteUniqueID
                                            select new
                                            {
                                                c.UNIQUEID,
                                                c.ID,
                                                c.DESCRIPTION,
                                                x.SEQ
                                            }).OrderBy(x => x.SEQ).ToList();

                    foreach (var controlPoint in controlPointList)
                    {
                        var treeItem = new TreeItem() { Title = controlPoint.DESCRIPTION };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.ControlPoint.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", controlPoint.ID, controlPoint.DESCRIPTION);
                        attributes[Define.EnumTreeAttribute.RouteUniqueID] = RouteUniqueID;
                        attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = controlPoint.UNIQUEID;
                        attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = string.Empty;
                        attributes[Define.EnumTreeAttribute.PartUniqueID] = string.Empty;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        if (db.ROUTEEQUIPMENT.Any(x => x.ROUTEUNIQUEID == RouteUniqueID && x.CONTROLPOINTUNIQUEID == controlPoint.UNIQUEID) 
                            ||
                            db.ROUTECONTROLPOINTCHECKITEM.Any(x => x.ROUTEUNIQUEID == RouteUniqueID && x.CONTROLPOINTUNIQUEID == controlPoint.UNIQUEID))
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (!string.IsNullOrEmpty(PartUniqueID) && PartUniqueID!="*")
                    {
                        var checkItemList = (from x in db.ROUTEEQUIPMENTCHECKITEM
                                             join c in db.CHECKITEM
                                             on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                             where x.ROUTEUNIQUEID == RouteUniqueID && x.CONTROLPOINTUNIQUEID == ControlPointUniqueID && x.EQUIPMENTUNIQUEID == EquipmentUniqueID && x.PARTUNIQUEID == PartUniqueID
                                             select new
                                             {
                                                 c.ID,
                                                 c.DESCRIPTION,
                                                 x.SEQ
                                             }).OrderBy(x => x.SEQ).ToList();

                        foreach (var checkItem in checkItemList)
                        {
                            var treeItem = new TreeItem() { Title = checkItem.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.CheckItem.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", checkItem.ID, checkItem.DESCRIPTION);
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
                        var checkItemList = (from x in db.ROUTEEQUIPMENTCHECKITEM
                                             join c in db.CHECKITEM
                                             on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                             where x.ROUTEUNIQUEID == RouteUniqueID && x.CONTROLPOINTUNIQUEID == ControlPointUniqueID && x.EQUIPMENTUNIQUEID == EquipmentUniqueID && x.PARTUNIQUEID=="*"
                                             select new
                                             {
                                                 c.ID,
                                                 c.DESCRIPTION,
                                                 x.SEQ
                                             }).OrderBy(x => x.SEQ).ToList();

                        foreach (var checkItem in checkItemList)
                        {
                            var treeItem = new TreeItem() { Title = checkItem.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.CheckItem.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", checkItem.ID, checkItem.DESCRIPTION);
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
                        var checkItemList = (from x in db.ROUTECONTROLPOINTCHECKITEM
                                             join c in db.CHECKITEM
                                             on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                             where x.ROUTEUNIQUEID == RouteUniqueID && x.CONTROLPOINTUNIQUEID == ControlPointUniqueID
                                             select new
                                             {
                                                 c.ID,
                                                 c.DESCRIPTION,
                                                 x.SEQ
                                             }).OrderBy(x => x.SEQ).ToList();


                        foreach (var checkItem in checkItemList)
                        {
                            var treeItem = new TreeItem() { Title = checkItem.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.CheckItem.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", checkItem.ID, checkItem.DESCRIPTION);
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

                        var equipmentList = (from x in db.ROUTEEQUIPMENT
                                             join e in db.EQUIPMENT
                                             on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                             join p in db.EQUIPMENTPART
                                             on x.PARTUNIQUEID equals p.UNIQUEID into tmpPart
                                             from p in tmpPart.DefaultIfEmpty()
                                             where x.ROUTEUNIQUEID==RouteUniqueID&&x.CONTROLPOINTUNIQUEID==ControlPointUniqueID
                                             select new
                                             {
                                                 EquipmentUniqueID = e.UNIQUEID,
                                                 PartUniqueID = p != null ? p.UNIQUEID : "*",
                                                 EquipmentID = e.ID,
                                                 EquipmentName = e.NAME,
                                                 PartDescription = p != null ? p.DESCRIPTION : "",
                                                 x.SEQ
                                             }).OrderBy(x => x.SEQ).ToList();

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

                            if (db.ROUTEEQUIPMENTCHECKITEM.Any(x => x.ROUTEUNIQUEID == RouteUniqueID && x.CONTROLPOINTUNIQUEID == ControlPointUniqueID && x.EQUIPMENTUNIQUEID == equipment.EquipmentUniqueID && x.PARTUNIQUEID == equipment.PartUniqueID))
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
