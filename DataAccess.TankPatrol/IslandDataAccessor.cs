using DbEntity.MSSQL.TankPatrol;
using Models.Authenticated;
using Models.Shared;
using Models.TankPatrol.IslandManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace DataAccess.TankPatrol
{
    public class IslandDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                var organizationList = Account.QueryableOrganizationUniqueIDList.Intersect(downStreamOrganizationList);

                using (TankDbEntities db = new TankDbEntities())
                {
                    var query = (from i in db.Island
                                 join s in db.Station
                                 on i.StationUniqueID equals s.UniqueID
                                 where organizationList.Contains(s.OrganizationUniqueID)
                                 select new
                                 {
                                     s.OrganizationUniqueID,
                                     StationUniqueID = s.UniqueID,
                                     StationID = s.ID,
                                     StationDescription = s.Description,
                                     IslandUniqueID = i.UniqueID,
                                     IslandID = i.ID,
                                     IslandDescription = i.Description
                                 }).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.StationUniqueID))
                    {
                        query = query.Where(x => x.StationUniqueID == Parameters.StationUniqueID);
                    }

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.IslandID.Contains(Parameters.Keyword) || x.IslandDescription.Contains(Parameters.Keyword));
                    }

                    var organization = OrganizationDataAccessor.GetOrganization(Parameters.OrganizationUniqueID);

                    var station = db.Station.FirstOrDefault(x => x.UniqueID == Parameters.StationUniqueID);

                    result.ReturnData(new GridViewModel()
                    {
                        Permission = Account.OrganizationPermission(Parameters.OrganizationUniqueID),
                        FullOrganizationDescription = organization.FullDescription,
                        OrganizationUniqueID = Parameters.OrganizationUniqueID,
                        OrganizationDescription = organization.Description,
                        StationUniqueID = station != null ? station.UniqueID : string.Empty,
                        StationDescription = station != null ? station.Description : string.Empty,
                        ItemList = query.ToList().Select(x => new GridItem
                                   {
                                       UniqueID = x.IslandUniqueID,
                                       Permission = Account.OrganizationPermission(x.OrganizationUniqueID),
                                       OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.OrganizationUniqueID),
                                       StationDescription = x.StationDescription,
                                       ID = x.IslandID,
                                       Description = x.IslandDescription
                                   }).OrderBy(x => x.StationDescription).ThenBy(x => x.ID).ToList()
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
                using (TankDbEntities db = new TankDbEntities())
                {
                    var island = (from i in db.Island
                                  join s in db.Station
                                  on i.StationUniqueID equals s.UniqueID
                                  where i.UniqueID == UniqueID
                                  select new
                                  {
                                      i.UniqueID,
                                      s.OrganizationUniqueID,
                                      StationID = s.ID,
                                      StationDescription = s.Description,
                                      i.ID,
                                      i.Description
                                  }).First();

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = island.UniqueID,
                        Permission = Account.OrganizationPermission(island.OrganizationUniqueID),
                        OrganizationUniqueID = island.OrganizationUniqueID,
                        StationID = island.StationID,
                        StationDescription = island.StationDescription,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(island.OrganizationUniqueID),
                        IslandID = island.ID,
                        IslandDescription = island.Description
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

        public static RequestResult GetCreateFormModel(string OrganizationUniqueID, string StationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (TankDbEntities db = new TankDbEntities())
                {
                    var model = new CreateFormModel()
                    {
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(OrganizationUniqueID),
                        StationSelectItemList = new List<System.Web.Mvc.SelectListItem>() { Define.DefaultSelectListItem(Resources.Resource.SelectOne) },
                        FormInput = new FormInput()
                        {
                            StationUniqueID = StationUniqueID
                        }
                    };

                    var stationList = db.Station.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                    model.StationSelectItemList.AddRange(stationList.Select(x => new SelectListItem
                    {
                        Value = x.UniqueID,
                        Text = string.Format("{0}/{1}", x.ID, x.Description)
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

        public static RequestResult Create(CreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (TankDbEntities db = new TankDbEntities())
                {
                    var query = db.Island.FirstOrDefault(x => x.StationUniqueID == Model.FormInput.StationUniqueID && x.ID == Model.FormInput.ID);

                    if (query == null)
                    {
                        db.Island.Add(new Island()
                        {
                            UniqueID = Guid.NewGuid().ToString(),
                            StationUniqueID = Model.FormInput.StationUniqueID,
                            ID = Model.FormInput.ID,
                            Description = Model.FormInput.Description
                        });

                        db.SaveChanges();

                        result.ReturnSuccessMessage("新增灌島成功");
                    }
                    else
                    {
                        result.ReturnFailedMessage("灌島代號已存在");
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
                using (TankDbEntities db = new TankDbEntities())
                {
                    var island = (from i in db.Island
                                  join s in db.Station
                                  on i.StationUniqueID equals s.UniqueID
                                  where i.UniqueID == UniqueID
                                  select new
                                  {
                                      i.UniqueID,
                                      s.OrganizationUniqueID,
                                      StationDescription = s.Description,
                                      i.ID,
                                      i.Description
                                  }).First();

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = island.UniqueID,
                        StationDescription = island.StationDescription,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(island.OrganizationUniqueID),
                        FormInput = new FormInput
                        {
                            ID = island.ID,
                            Description = island.Description
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
                using (TankDbEntities db = new TankDbEntities())
                {
                    var island = db.Island.First(x => x.UniqueID == Model.UniqueID);

                    var query = db.Island.FirstOrDefault(x => x.UniqueID != Model.UniqueID && x.StationUniqueID == island.StationUniqueID && x.ID == Model.FormInput.ID);

                    if (query == null)
                    {
                        island.ID = Model.FormInput.ID;
                        island.Description = Model.FormInput.Description;

                        db.SaveChanges();

                        result.ReturnSuccessMessage("編輯灌島成功");
                    }
                    else
                    {
                        result.ReturnFailedMessage("灌島代號已存在");
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
                using (TankDbEntities db = new TankDbEntities())
                {
                    db.PortCheckItem.RemoveRange((from x in db.PortCheckItem
                                                  join p in db.Port
                                                  on x.PortUniqueID equals p.UniqueID
                                                  join i in db.Island
                                                  on p.IslandUniqueID equals i.UniqueID
                                                  where i.UniqueID == UniqueID
                                                  select x).ToList());

                    db.Port.RemoveRange((from p in db.Port
                                         join i in db.Island
                                         on p.IslandUniqueID equals i.UniqueID
                                         where i.UniqueID == UniqueID
                                         select p).ToList());

                    db.Island.Remove(db.Island.First(x => x.UniqueID == UniqueID));

                    db.SaveChanges();

                    result.ReturnSuccessMessage("刪除灌島成功");
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

        public static RequestResult GetTreeItem(List<Models.Shared.Organization> OrganizationList, string OrganizationUniqueID, string StationUniqueID, Account Account)
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
                    { Define.EnumTreeAttribute.StationUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.IslandUniqueID, string.Empty }
                };

                using (TankDbEntities tankDb = new TankDbEntities())
                {
                    if (!string.IsNullOrEmpty(StationUniqueID))
                    {
                        var islandList = tankDb.Island.Where(x => x.StationUniqueID == StationUniqueID).OrderBy(x => x.ID).ToList();

                        foreach (var island in islandList)
                        {
                            var treeItem = new TreeItem() { Title = island.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Island.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", island.ID, island.Description);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.StationUniqueID] = StationUniqueID;
                            attributes[Define.EnumTreeAttribute.IslandUniqueID] = island.UniqueID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else
                    {
                        if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                        {
                            var stationList = tankDb.Station.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                            foreach (var station in stationList)
                            {
                                var treeItem = new TreeItem() { Title = string.Format("{0}", station.Description) };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Station.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", station.ID, station.Description);
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                                attributes[Define.EnumTreeAttribute.StationUniqueID] = station.UniqueID;
                                attributes[Define.EnumTreeAttribute.IslandUniqueID] = string.Empty;

                                foreach (var attribute in attributes)
                                {
                                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                }

                                if (tankDb.Island.Any(x => x.StationUniqueID == station.UniqueID))
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
                            attributes[Define.EnumTreeAttribute.StationUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.IslandUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                        ||
                        (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && tankDb.Station.Any(x => x.OrganizationUniqueID == organization.UniqueID)))
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
                    { Define.EnumTreeAttribute.StationUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.IslandUniqueID, string.Empty }
                };

                using (TankDbEntities tankDb = new TankDbEntities())
                {
                    var organization = OrganizationList.First(x => x.UniqueID == OrganizationUniqueID);

                    var treeItem = new TreeItem() { Title = organization.Description };

                    attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                    attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                    attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                    attributes[Define.EnumTreeAttribute.StationUniqueID] = string.Empty;
                    attributes[Define.EnumTreeAttribute.IslandUniqueID] = string.Empty;

                    foreach (var attribute in attributes)
                    {
                        treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                    }

                    if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                        ||
                        (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && tankDb.Station.Any(x => x.OrganizationUniqueID == organization.UniqueID)))
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
    }
}
