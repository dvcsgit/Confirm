using DbEntity.MSSQL.TankPatrol;
using Models.Authenticated;
using Models.Shared;
using Models.TankPatrol.StationManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace DataAccess.TankPatrol
{
    public class StationDataAccessor
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
                    var query = db.Station.Where(x => organizationList.Contains(x.OrganizationUniqueID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.ID.Contains(Parameters.Keyword) || x.Description.Contains(Parameters.Keyword));
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
                            ID = x.ID,
                            Description = x.Description,
                            Permission = Account.OrganizationPermission(x.OrganizationUniqueID),
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.OrganizationUniqueID),
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
                using (TankDbEntities db = new TankDbEntities())
                {
                    var station = db.Station.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = station.UniqueID,
                        OrganizationUniqueID = station.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(station.OrganizationUniqueID),
                        ID = station.ID,
                        Description = station.Description,
                        Permission = Account.OrganizationPermission(station.OrganizationUniqueID)
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
                using (TankDbEntities db = new TankDbEntities())
                {
                    var query = db.Station.FirstOrDefault(x => x.OrganizationUniqueID == Model.OrganizationUniqueID && x.ID == Model.FormInput.ID);

                    if (query == null)
                    {
                        db.Station.Add(new Station()
                        {
                            UniqueID = Guid.NewGuid().ToString(),
                            OrganizationUniqueID = Model.OrganizationUniqueID,
                            ID = Model.FormInput.ID,
                            Description = Model.FormInput.Description
                        });

                        db.SaveChanges();

                        result.ReturnSuccessMessage("新增裝/卸料站成功");
                    }
                    else
                    {
                        result.ReturnFailedMessage("裝/卸料站代號已存在");
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
                    var station = db.Station.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = station.UniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(station.OrganizationUniqueID),
                        FormInput = new FormInput
                        {
                            ID = station.ID,
                            Description = station.Description
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
                    var station = db.Station.First(x => x.UniqueID == Model.UniqueID);

                    var query = db.Station.FirstOrDefault(x => x.UniqueID != Model.UniqueID && x.OrganizationUniqueID == station.OrganizationUniqueID && x.ID == Model.FormInput.ID);

                    if (query == null)
                    {
                        station.ID = Model.FormInput.ID;
                        station.Description = Model.FormInput.Description;
                        
                        db.SaveChanges();

                        result.ReturnSuccessMessage("編輯裝/卸料站成功");
                    }
                    else
                    {
                        result.ReturnFailedMessage("裝/卸料站代號已存在");
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
                                                  join s in db.Station
                                                  on i.StationUniqueID equals s.UniqueID
                                                  where s.UniqueID == UniqueID
                                                  select x).ToList());

                    db.Port.RemoveRange((from p in db.Port
                                         join i in db.Island
                                         on p.IslandUniqueID equals i.UniqueID
                                         join s in db.Station
                                         on i.StationUniqueID equals s.UniqueID
                                         where s.UniqueID == UniqueID
                                         select p).ToList());

                    db.Island.RemoveRange(db.Island.Where(x => x.StationUniqueID == UniqueID).ToList());

                    db.Station.Remove(db.Station.First(x => x.UniqueID == UniqueID));

                    db.SaveChanges();

                    result.ReturnSuccessMessage("刪除裝/卸料站成功");
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

        public static RequestResult GetTreeItem(List<Models.Shared.Organization> OrganizationList, string OrganizationUniqueID, Account Account)
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
                    { Define.EnumTreeAttribute.StationUniqueID, string.Empty }
                };

                using (TankDbEntities tankDb = new TankDbEntities())
                {
                    if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                    {
                        var stationList = tankDb.Station.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                        foreach (var station in stationList)
                        {
                            var treeItem = new TreeItem() { Title = station.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Station.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", station.ID, station.Description);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.StationUniqueID] = station.UniqueID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
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
                    { Define.EnumTreeAttribute.StationUniqueID, string.Empty }
                };

                using (TankDbEntities tankDb = new TankDbEntities())
                {
                    var organization = OrganizationList.First(x => x.UniqueID == OrganizationUniqueID);

                    var treeItem = new TreeItem() { Title = organization.Description };

                    attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                    attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                    attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                    attributes[Define.EnumTreeAttribute.StationUniqueID] = string.Empty;

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
