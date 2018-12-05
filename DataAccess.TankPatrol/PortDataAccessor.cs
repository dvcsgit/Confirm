using DbEntity.MSSQL.TankPatrol;
using Models.Authenticated;
using Models.Shared;
using Models.TankPatrol.PortManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace DataAccess.TankPatrol
{
    public class PortDataAccessor
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
                    var query = (from p in db.Port
                                join i in db.Island
                                on p.IslandUniqueID equals i.UniqueID
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
                                     IslandDescription = i.Description,
                                     PortUniqueID = p.UniqueID,
                                     PortID=p.ID,
                                     PortDescription = p.Description
                                 }).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.StationUniqueID))
                    {
                        query = query.Where(x => x.StationUniqueID == Parameters.StationUniqueID);
                    }

                    if (!string.IsNullOrEmpty(Parameters.IslandUniqueID))
                    {
                        query = query.Where(x => x.IslandUniqueID == Parameters.IslandUniqueID);
                    }

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.IslandID.Contains(Parameters.Keyword) || x.IslandDescription.Contains(Parameters.Keyword));
                    }

                    var organization = OrganizationDataAccessor.GetOrganization(Parameters.OrganizationUniqueID);

                    var station = db.Station.FirstOrDefault(x => x.UniqueID == Parameters.StationUniqueID);
                    var island = db.Island.FirstOrDefault(x => x.UniqueID == Parameters.IslandUniqueID);

                    result.ReturnData(new GridViewModel()
                    {
                        Permission = Account.OrganizationPermission(Parameters.OrganizationUniqueID),
                        OrganizationDescription = organization.Description,
                        OrganizationUniqueID = organization.UniqueID,
                        StationUniqueID = station != null ? station.UniqueID : string.Empty,
                        FullOrganizationDescription = OrganizationDataAccessor.GetOrganizationFullDescription(Parameters.OrganizationUniqueID),
                        StationDescription = station != null ? station.Description : string.Empty,
                        IslandUniqueID = island != null ? island.UniqueID : string.Empty,
                        IslandDescription = island != null ? island.Description : string.Empty,
                        ItemList = query.ToList().Select(x => new GridItem
                                    {
                                        UniqueID = x.PortUniqueID,
                                        ID = x.PortID,
                                        Description = x.PortDescription,
                                        Permission = Account.OrganizationPermission(x.OrganizationUniqueID),
                                        OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.OrganizationUniqueID),
                                        StationDescription = x.StationDescription,
                                        IslandDescription = x.IslandDescription
                                    }).ToList()
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
                    var query = (from p in db.Port
                                 join i in db.Island
                                 on p.IslandUniqueID equals i.UniqueID
                                 join s in db.Station
                                 on i.StationUniqueID equals s.UniqueID
                                 where p.UniqueID == UniqueID
                                 select new
                                 {
                                     s.OrganizationUniqueID,
                                     StationDescription = s.Description,
                                     IslandDescription = i.Description,
                                     p.UniqueID,
                                     p.ID,
                                     p.Description,
                                     p.TagID,
                                     p.LB2LPTimeSpan,
                                     p.LP2LATimeSpan,
                                     p.LA2LDTimeSpan,
                                     p.UB2UPTimeSpan,
                                     p.UP2UATimeSpan,
                                     p.UA2UDTimeSpan
                                 }).First();

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = query.UniqueID,
                        Permission = Account.OrganizationPermission(query.OrganizationUniqueID),
                        ID = query.ID,
                        Description = query.Description,
                        TagID = query.TagID,
                        LB2LPTimeSpan = query.LB2LPTimeSpan,
                        LP2LATimeSpan = query.LP2LATimeSpan,
                        LA2LDTimeSpan = query.LA2LDTimeSpan,
                        UB2UPTimeSpan = query.UB2UPTimeSpan,
                        UP2UATimeSpan = query.UP2UATimeSpan,
                        UA2UDTimeSpan = query.UA2UDTimeSpan,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(query.OrganizationUniqueID),
                        StationDescription = query.StationDescription,
                        IslandDescription = query.IslandDescription,
                        LBCheckItemList = (from x in db.PortCheckItem
                                           join c in db.CheckItem
                                           on x.CheckItemUniqueID equals c.UniqueID
                                           where x.PortUniqueID == query.UniqueID && x.CheckType == "L" && x.Procedure == "B"
                                           select new CheckItemModel
                                           {
                                               UniqueID = c.UniqueID,
                                               CheckType = c.CheckType,
                                               ID = c.ID,
                                               Description = c.Description,
                                               IsFeelItem = c.IsFeelItem,
                                               IsAccumulation = c.IsAccumulation,
                                               IsInherit = x.IsInherit,
                                               OriLowerLimit = c.LowerLimit,
                                               OriLowerAlertLimit = c.LowerAlertLimit,
                                               OriUpperAlertLimit = c.UpperAlertLimit,
                                               OriUpperLimit = c.UpperLimit,
                                               OriAccumulationBase = c.AccumulationBase,
                                               OriUnit = c.Unit,
                                               OriRemark = c.Remark,
                                               LowerLimit = x.LowerLimit,
                                               LowerAlertLimit = x.LowerAlertLimit,
                                               UpperAlertLimit = x.UpperAlertLimit,
                                               UpperLimit = x.UpperLimit,
                                               AccumulationBase = x.AccumulationBase,
                                               Unit = x.Unit,
                                               TagID = x.TagID
                                           }).ToList(),
                        LPCheckItemList = (from x in db.PortCheckItem
                                           join c in db.CheckItem
                                           on x.CheckItemUniqueID equals c.UniqueID
                                           where x.PortUniqueID == query.UniqueID && x.CheckType == "L" && x.Procedure == "P"
                                           select new CheckItemModel
                                           {
                                               UniqueID = c.UniqueID,
                                               CheckType = c.CheckType,
                                               ID = c.ID,
                                               Description = c.Description,
                                               IsFeelItem = c.IsFeelItem,
                                               IsAccumulation = c.IsAccumulation,
                                               IsInherit = x.IsInherit,
                                               OriLowerLimit = c.LowerLimit,
                                               OriLowerAlertLimit = c.LowerAlertLimit,
                                               OriUpperAlertLimit = c.UpperAlertLimit,
                                               OriUpperLimit = c.UpperLimit,
                                               OriAccumulationBase = c.AccumulationBase,
                                               OriUnit = c.Unit,
                                               OriRemark = c.Remark,
                                               LowerLimit = x.LowerLimit,
                                               LowerAlertLimit = x.LowerAlertLimit,
                                               UpperAlertLimit = x.UpperAlertLimit,
                                               UpperLimit = x.UpperLimit,
                                               AccumulationBase = x.AccumulationBase,
                                               Unit = x.Unit,
                                               TagID = x.TagID
                                           }).ToList(),
                        LACheckItemList = (from x in db.PortCheckItem
                                           join c in db.CheckItem
                                           on x.CheckItemUniqueID equals c.UniqueID
                                           where x.PortUniqueID == query.UniqueID && x.CheckType == "L" && x.Procedure == "A"
                                           select new CheckItemModel
                                           {
                                               UniqueID = c.UniqueID,
                                               CheckType = c.CheckType,
                                               ID = c.ID,
                                               Description = c.Description,
                                               IsFeelItem = c.IsFeelItem,
                                               IsAccumulation = c.IsAccumulation,
                                               IsInherit = x.IsInherit,
                                               OriLowerLimit = c.LowerLimit,
                                               OriLowerAlertLimit = c.LowerAlertLimit,
                                               OriUpperAlertLimit = c.UpperAlertLimit,
                                               OriUpperLimit = c.UpperLimit,
                                               OriAccumulationBase = c.AccumulationBase,
                                               OriUnit = c.Unit,
                                               OriRemark = c.Remark,
                                               LowerLimit = x.LowerLimit,
                                               LowerAlertLimit = x.LowerAlertLimit,
                                               UpperAlertLimit = x.UpperAlertLimit,
                                               UpperLimit = x.UpperLimit,
                                               AccumulationBase = x.AccumulationBase,
                                               Unit = x.Unit,
                                               TagID = x.TagID
                                           }).ToList(),
                        LDCheckItemList = (from x in db.PortCheckItem
                                           join c in db.CheckItem
                                           on x.CheckItemUniqueID equals c.UniqueID
                                           where x.PortUniqueID == query.UniqueID && x.CheckType == "L" && x.Procedure == "D"
                                           select new CheckItemModel
                                           {
                                               UniqueID = c.UniqueID,
                                               CheckType = c.CheckType,
                                               ID = c.ID,
                                               Description = c.Description,
                                               IsFeelItem = c.IsFeelItem,
                                               IsAccumulation = c.IsAccumulation,
                                               IsInherit = x.IsInherit,
                                               OriLowerLimit = c.LowerLimit,
                                               OriLowerAlertLimit = c.LowerAlertLimit,
                                               OriUpperAlertLimit = c.UpperAlertLimit,
                                               OriUpperLimit = c.UpperLimit,
                                               OriAccumulationBase = c.AccumulationBase,
                                               OriUnit = c.Unit,
                                               OriRemark = c.Remark,
                                               LowerLimit = x.LowerLimit,
                                               LowerAlertLimit = x.LowerAlertLimit,
                                               UpperAlertLimit = x.UpperAlertLimit,
                                               UpperLimit = x.UpperLimit,
                                               AccumulationBase = x.AccumulationBase,
                                               Unit = x.Unit,
                                               TagID = x.TagID
                                           }).ToList(),
                        UBCheckItemList = (from x in db.PortCheckItem
                                           join c in db.CheckItem
                                           on x.CheckItemUniqueID equals c.UniqueID
                                           where x.PortUniqueID == query.UniqueID && x.CheckType == "U" && x.Procedure == "B"
                                           select new CheckItemModel
                                           {
                                               UniqueID = c.UniqueID,
                                               CheckType = c.CheckType,
                                               ID = c.ID,
                                               Description = c.Description,
                                               IsFeelItem = c.IsFeelItem,
                                               IsAccumulation = c.IsAccumulation,
                                               IsInherit = x.IsInherit,
                                               OriLowerLimit = c.LowerLimit,
                                               OriLowerAlertLimit = c.LowerAlertLimit,
                                               OriUpperAlertLimit = c.UpperAlertLimit,
                                               OriUpperLimit = c.UpperLimit,
                                               OriAccumulationBase = c.AccumulationBase,
                                               OriUnit = c.Unit,
                                               OriRemark = c.Remark,
                                               LowerLimit = x.LowerLimit,
                                               LowerAlertLimit = x.LowerAlertLimit,
                                               UpperAlertLimit = x.UpperAlertLimit,
                                               UpperLimit = x.UpperLimit,
                                               AccumulationBase = x.AccumulationBase,
                                               Unit = x.Unit,
                                               TagID = x.TagID
                                           }).ToList(),
                        UPCheckItemList = (from x in db.PortCheckItem
                                           join c in db.CheckItem
                                           on x.CheckItemUniqueID equals c.UniqueID
                                           where x.PortUniqueID == query.UniqueID && x.CheckType == "U" && x.Procedure == "P"
                                           select new CheckItemModel
                                           {
                                               UniqueID = c.UniqueID,
                                               CheckType = c.CheckType,
                                               ID = c.ID,
                                               Description = c.Description,
                                               IsFeelItem = c.IsFeelItem,
                                               IsAccumulation = c.IsAccumulation,
                                               IsInherit = x.IsInherit,
                                               OriLowerLimit = c.LowerLimit,
                                               OriLowerAlertLimit = c.LowerAlertLimit,
                                               OriUpperAlertLimit = c.UpperAlertLimit,
                                               OriUpperLimit = c.UpperLimit,
                                               OriAccumulationBase = c.AccumulationBase,
                                               OriUnit = c.Unit,
                                               OriRemark = c.Remark,
                                               LowerLimit = x.LowerLimit,
                                               LowerAlertLimit = x.LowerAlertLimit,
                                               UpperAlertLimit = x.UpperAlertLimit,
                                               UpperLimit = x.UpperLimit,
                                               AccumulationBase = x.AccumulationBase,
                                               Unit = x.Unit,
                                               TagID = x.TagID
                                           }).ToList(),
                        UACheckItemList = (from x in db.PortCheckItem
                                           join c in db.CheckItem
                                           on x.CheckItemUniqueID equals c.UniqueID
                                           where x.PortUniqueID == query.UniqueID && x.CheckType == "U" && x.Procedure == "A"
                                           select new CheckItemModel
                                           {
                                               UniqueID = c.UniqueID,
                                               CheckType = c.CheckType,
                                               ID = c.ID,
                                               Description = c.Description,
                                               IsFeelItem = c.IsFeelItem,
                                               IsAccumulation = c.IsAccumulation,
                                               IsInherit = x.IsInherit,
                                               OriLowerLimit = c.LowerLimit,
                                               OriLowerAlertLimit = c.LowerAlertLimit,
                                               OriUpperAlertLimit = c.UpperAlertLimit,
                                               OriUpperLimit = c.UpperLimit,
                                               OriAccumulationBase = c.AccumulationBase,
                                               OriUnit = c.Unit,
                                               OriRemark = c.Remark,
                                               LowerLimit = x.LowerLimit,
                                               LowerAlertLimit = x.LowerAlertLimit,
                                               UpperAlertLimit = x.UpperAlertLimit,
                                               UpperLimit = x.UpperLimit,
                                               AccumulationBase = x.AccumulationBase,
                                               Unit = x.Unit,
                                               TagID = x.TagID
                                           }).ToList(),
                        UDCheckItemList = (from x in db.PortCheckItem
                                           join c in db.CheckItem
                                           on x.CheckItemUniqueID equals c.UniqueID
                                           where x.PortUniqueID == query.UniqueID && x.CheckType == "U" && x.Procedure == "D"
                                           select new CheckItemModel
                                           {
                                               UniqueID = c.UniqueID,
                                               CheckType = c.CheckType,
                                               ID = c.ID,
                                               Description = c.Description,
                                               IsFeelItem = c.IsFeelItem,
                                               IsAccumulation = c.IsAccumulation,
                                               IsInherit = x.IsInherit,
                                               OriLowerLimit = c.LowerLimit,
                                               OriLowerAlertLimit = c.LowerAlertLimit,
                                               OriUpperAlertLimit = c.UpperAlertLimit,
                                               OriUpperLimit = c.UpperLimit,
                                               OriAccumulationBase = c.AccumulationBase,
                                               OriUnit = c.Unit,
                                               OriRemark = c.Remark,
                                               LowerLimit = x.LowerLimit,
                                               LowerAlertLimit = x.LowerAlertLimit,
                                               UpperAlertLimit = x.UpperAlertLimit,
                                               UpperLimit = x.UpperLimit,
                                               AccumulationBase = x.AccumulationBase,
                                               Unit = x.Unit,
                                               TagID = x.TagID
                                           }).ToList()
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

        public static RequestResult GetCreateFormModel(string OrganizationUniqueID, string StationUniqueID, string IslandUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (TankDbEntities db = new TankDbEntities())
                {
                    var model = new CreateFormModel()
                    {
                        OrganizationUniqueID = OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(OrganizationUniqueID),
                        StationSelectItemList = new List<SelectListItem>() { Define.DefaultSelectListItem(Resources.Resource.SelectOne) },
                        IslandSelectItemList = new List<SelectListItem>() { Define.DefaultSelectListItem(Resources.Resource.SelectOne) },
                        FormInput = new FormInput()
                        {
                            StationUniqueID = StationUniqueID,
                            IslandUniqueID = IslandUniqueID
                        }
                    };

                    var stationList = db.Station.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                    foreach (var station in stationList)
                    {
                        model.StationSelectItemList.Add(new SelectListItem()
                        {
                            Text = string.Format("{0}/{1}", station.ID, station.Description),
                            Value = station.UniqueID
                        });

                        var islandList = db.Island.Where(x => x.StationUniqueID == station.UniqueID).OrderBy(x => x.ID).ToList();

                        model.IslandList.AddRange(islandList.Select(x => new IslandModel
                        {
                            StationUniqueID = station.UniqueID,
                            UniqueID = x.UniqueID,
                            ID = x.ID,
                            Description = x.Description
                        }).ToList());

                        if (station.UniqueID == StationUniqueID)
                        {
                            model.IslandSelectItemList.AddRange(islandList.Select(x => new SelectListItem
                            {
                                Text = string.Format("{0}/{1}", x.ID, x.Description),
                                Value = x.UniqueID
                            }).ToList());
                        }
                    }

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
                    var query = db.Port.FirstOrDefault(x => x.IslandUniqueID == Model.FormInput.IslandUniqueID && x.ID == Model.FormInput.ID);

                    if (query == null)
                    {
                        var uniqueID = Guid.NewGuid().ToString();

                        db.Port.Add(new Port()
                        {
                            UniqueID = uniqueID,
                            ID = Model.FormInput.ID,
                            Description = Model.FormInput.Description,
                            TagID = Model.FormInput.TagID,
                            IslandUniqueID = Model.FormInput.IslandUniqueID,
                            LB2LPTimeSpan = Model.FormInput.LB2LPTimeSpan,
                            LP2LATimeSpan = Model.FormInput.LP2LATimeSpan,
                            LA2LDTimeSpan = Model.FormInput.LA2LDTimeSpan,
                            UB2UPTimeSpan = Model.FormInput.UB2UPTimeSpan,
                            UP2UATimeSpan = Model.FormInput.UP2UATimeSpan,
                            UA2UDTimeSpan = Model.FormInput.UA2UDTimeSpan
                        });

                        db.PortCheckItem.AddRange(Model.LBCheckItemList.Select(x => new PortCheckItem
                        {
                            PortUniqueID = uniqueID,
                            CheckItemUniqueID = x.UniqueID,
                            CheckType = "L",
                            Procedure="B",
                            TagID = x.TagID,
                            IsInherit = x.IsInherit,
                            LowerLimit = x.LowerLimit,
                            LowerAlertLimit = x.LowerAlertLimit,
                            UpperAlertLimit = x.UpperAlertLimit,
                            UpperLimit = x.UpperLimit,
                            AccumulationBase = x.AccumulationBase,
                            Unit = x.Unit
                        }).ToList());

                        db.PortCheckItem.AddRange(Model.LPCheckItemList.Select(x => new PortCheckItem
                        {
                            PortUniqueID = uniqueID,
                            CheckItemUniqueID = x.UniqueID,
                            CheckType = "L",
                            Procedure = "P",
                            TagID = x.TagID,
                            IsInherit = x.IsInherit,
                            LowerLimit = x.LowerLimit,
                            LowerAlertLimit = x.LowerAlertLimit,
                            UpperAlertLimit = x.UpperAlertLimit,
                            UpperLimit = x.UpperLimit,
                            AccumulationBase = x.AccumulationBase,
                            Unit = x.Unit
                        }).ToList());

                        db.PortCheckItem.AddRange(Model.LACheckItemList.Select(x => new PortCheckItem
                        {
                            PortUniqueID = uniqueID,
                            CheckItemUniqueID = x.UniqueID,
                            CheckType = "L",
                            Procedure = "A",
                            TagID = x.TagID,
                            IsInherit = x.IsInherit,
                            LowerLimit = x.LowerLimit,
                            LowerAlertLimit = x.LowerAlertLimit,
                            UpperAlertLimit = x.UpperAlertLimit,
                            UpperLimit = x.UpperLimit,
                            AccumulationBase = x.AccumulationBase,
                            Unit = x.Unit
                        }).ToList());

                        db.PortCheckItem.AddRange(Model.LDCheckItemList.Select(x => new PortCheckItem
                        {
                            PortUniqueID = uniqueID,
                            CheckItemUniqueID = x.UniqueID,
                            CheckType = "L",
                            Procedure = "D",
                            TagID = x.TagID,
                            IsInherit = x.IsInherit,
                            LowerLimit = x.LowerLimit,
                            LowerAlertLimit = x.LowerAlertLimit,
                            UpperAlertLimit = x.UpperAlertLimit,
                            UpperLimit = x.UpperLimit,
                            AccumulationBase = x.AccumulationBase,
                            Unit = x.Unit
                        }).ToList());

                        db.PortCheckItem.AddRange(Model.UBCheckItemList.Select(x => new PortCheckItem
                        {
                            PortUniqueID = uniqueID,
                            CheckItemUniqueID = x.UniqueID,
                            CheckType = "U",
                            Procedure = "B",
                            TagID = x.TagID,
                            IsInherit = x.IsInherit,
                            LowerLimit = x.LowerLimit,
                            LowerAlertLimit = x.LowerAlertLimit,
                            UpperAlertLimit = x.UpperAlertLimit,
                            UpperLimit = x.UpperLimit,
                            AccumulationBase = x.AccumulationBase,
                            Unit = x.Unit
                        }).ToList());

                        db.PortCheckItem.AddRange(Model.UPCheckItemList.Select(x => new PortCheckItem
                        {
                            PortUniqueID = uniqueID,
                            CheckItemUniqueID = x.UniqueID,
                            CheckType = "U",
                            Procedure = "P",
                            TagID = x.TagID,
                            IsInherit = x.IsInherit,
                            LowerLimit = x.LowerLimit,
                            LowerAlertLimit = x.LowerAlertLimit,
                            UpperAlertLimit = x.UpperAlertLimit,
                            UpperLimit = x.UpperLimit,
                            AccumulationBase = x.AccumulationBase,
                            Unit = x.Unit
                        }).ToList());

                        db.PortCheckItem.AddRange(Model.UACheckItemList.Select(x => new PortCheckItem
                        {
                            PortUniqueID = uniqueID,
                            CheckItemUniqueID = x.UniqueID,
                            CheckType = "U",
                            Procedure = "A",
                            TagID = x.TagID,
                            IsInherit = x.IsInherit,
                            LowerLimit = x.LowerLimit,
                            LowerAlertLimit = x.LowerAlertLimit,
                            UpperAlertLimit = x.UpperAlertLimit,
                            UpperLimit = x.UpperLimit,
                            AccumulationBase = x.AccumulationBase,
                            Unit = x.Unit
                        }).ToList());

                        db.PortCheckItem.AddRange(Model.UDCheckItemList.Select(x => new PortCheckItem
                        {
                            PortUniqueID = uniqueID,
                            CheckItemUniqueID = x.UniqueID,
                            CheckType = "U",
                            Procedure = "D",
                            TagID = x.TagID,
                            IsInherit = x.IsInherit,
                            LowerLimit = x.LowerLimit,
                            LowerAlertLimit = x.LowerAlertLimit,
                            UpperAlertLimit = x.UpperAlertLimit,
                            UpperLimit = x.UpperLimit,
                            AccumulationBase = x.AccumulationBase,
                            Unit = x.Unit
                        }).ToList());

                        db.SaveChanges();

                        result.ReturnSuccessMessage("新增灌口成功");
                    }
                    else
                    {
                        result.ReturnFailedMessage("灌口代號已存在");
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
                    var query = (from p in db.Port
                                 join i in db.Island
                                 on p.IslandUniqueID equals i.UniqueID
                                 join s in db.Station
                                 on i.StationUniqueID equals s.UniqueID
                                 where p.UniqueID == UniqueID
                                 select new
                                 {
                                     s.OrganizationUniqueID,
                                     StationDescription = s.Description,
                                     IslandDescription = i.Description,
                                     p.UniqueID,
                                     p.ID,
                                     p.Description,
                                     p.TagID,
                                     p.LB2LPTimeSpan,
                                     p.LP2LATimeSpan,
                                     p.LA2LDTimeSpan,
                                     p.UB2UPTimeSpan,
                                     p.UP2UATimeSpan,
                                     p.UA2UDTimeSpan
                                 }).First();

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = query.UniqueID,
                        OrganizationUniqueID = query.OrganizationUniqueID,
                        FormInput = new FormInput()
                        {
                            ID = query.ID,
                            Description = query.Description,
                            TagID = query.TagID,
                            LB2LPTimeSpan=query.LB2LPTimeSpan,
                            LP2LATimeSpan=query.LP2LATimeSpan,
                            LA2LDTimeSpan=query.LA2LDTimeSpan,
                            UB2UPTimeSpan = query.UB2UPTimeSpan,
                            UP2UATimeSpan = query.UP2UATimeSpan,
                            UA2UDTimeSpan = query.UA2UDTimeSpan
                        },
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(query.OrganizationUniqueID),
                        StationDescription = query.StationDescription,
                        IslandDescription = query.IslandDescription,
                        LBCheckItemList = (from x in db.PortCheckItem
                                           join c in db.CheckItem
                                           on x.CheckItemUniqueID equals c.UniqueID
                                           where x.PortUniqueID == query.UniqueID && x.CheckType == "L" && x.Procedure == "B"
                                           select new CheckItemModel
                                           {
                                               UniqueID = c.UniqueID,
                                               CheckType = c.CheckType,
                                               ID = c.ID,
                                               Description = c.Description,
                                               IsFeelItem = c.IsFeelItem,
                                               IsAccumulation = c.IsAccumulation,
                                               IsInherit = x.IsInherit,
                                               OriLowerLimit = c.LowerLimit,
                                               OriLowerAlertLimit = c.LowerAlertLimit,
                                               OriUpperAlertLimit = c.UpperAlertLimit,
                                               OriUpperLimit = c.UpperLimit,
                                               OriAccumulationBase = c.AccumulationBase,
                                               OriUnit = c.Unit,
                                               OriRemark = c.Remark,
                                               LowerLimit = x.LowerLimit,
                                               LowerAlertLimit = x.LowerAlertLimit,
                                               UpperAlertLimit = x.UpperAlertLimit,
                                               UpperLimit = x.UpperLimit,
                                               AccumulationBase = x.AccumulationBase,
                                               Unit = x.Unit,
                                               TagID = x.TagID
                                           }).ToList(),
                        LPCheckItemList = (from x in db.PortCheckItem
                                           join c in db.CheckItem
                                           on x.CheckItemUniqueID equals c.UniqueID
                                           where x.PortUniqueID == query.UniqueID && x.CheckType == "L" && x.Procedure == "P"
                                           select new CheckItemModel
                                           {
                                               UniqueID = c.UniqueID,
                                               CheckType = c.CheckType,
                                               ID = c.ID,
                                               Description = c.Description,
                                               IsFeelItem = c.IsFeelItem,
                                               IsAccumulation = c.IsAccumulation,
                                               IsInherit = x.IsInherit,
                                               OriLowerLimit = c.LowerLimit,
                                               OriLowerAlertLimit = c.LowerAlertLimit,
                                               OriUpperAlertLimit = c.UpperAlertLimit,
                                               OriUpperLimit = c.UpperLimit,
                                               OriAccumulationBase = c.AccumulationBase,
                                               OriUnit = c.Unit,
                                               OriRemark = c.Remark,
                                               LowerLimit = x.LowerLimit,
                                               LowerAlertLimit = x.LowerAlertLimit,
                                               UpperAlertLimit = x.UpperAlertLimit,
                                               UpperLimit = x.UpperLimit,
                                               AccumulationBase = x.AccumulationBase,
                                               Unit = x.Unit,
                                               TagID = x.TagID
                                           }).ToList(),
                        LACheckItemList = (from x in db.PortCheckItem
                                           join c in db.CheckItem
                                           on x.CheckItemUniqueID equals c.UniqueID
                                           where x.PortUniqueID == query.UniqueID && x.CheckType == "L" && x.Procedure == "A"
                                           select new CheckItemModel
                                           {
                                               UniqueID = c.UniqueID,
                                               CheckType = c.CheckType,
                                               ID = c.ID,
                                               Description = c.Description,
                                               IsFeelItem = c.IsFeelItem,
                                               IsAccumulation = c.IsAccumulation,
                                               IsInherit = x.IsInherit,
                                               OriLowerLimit = c.LowerLimit,
                                               OriLowerAlertLimit = c.LowerAlertLimit,
                                               OriUpperAlertLimit = c.UpperAlertLimit,
                                               OriUpperLimit = c.UpperLimit,
                                               OriAccumulationBase = c.AccumulationBase,
                                               OriUnit = c.Unit,
                                               OriRemark = c.Remark,
                                               LowerLimit = x.LowerLimit,
                                               LowerAlertLimit = x.LowerAlertLimit,
                                               UpperAlertLimit = x.UpperAlertLimit,
                                               UpperLimit = x.UpperLimit,
                                               AccumulationBase = x.AccumulationBase,
                                               Unit = x.Unit,
                                               TagID = x.TagID
                                           }).ToList(),
                        LDCheckItemList = (from x in db.PortCheckItem
                                           join c in db.CheckItem
                                           on x.CheckItemUniqueID equals c.UniqueID
                                           where x.PortUniqueID == query.UniqueID && x.CheckType == "L" && x.Procedure == "D"
                                           select new CheckItemModel
                                           {
                                               UniqueID = c.UniqueID,
                                               CheckType = c.CheckType,
                                               ID = c.ID,
                                               Description = c.Description,
                                               IsFeelItem = c.IsFeelItem,
                                               IsAccumulation = c.IsAccumulation,
                                               IsInherit = x.IsInherit,
                                               OriLowerLimit = c.LowerLimit,
                                               OriLowerAlertLimit = c.LowerAlertLimit,
                                               OriUpperAlertLimit = c.UpperAlertLimit,
                                               OriUpperLimit = c.UpperLimit,
                                               OriAccumulationBase = c.AccumulationBase,
                                               OriUnit = c.Unit,
                                               OriRemark = c.Remark,
                                               LowerLimit = x.LowerLimit,
                                               LowerAlertLimit = x.LowerAlertLimit,
                                               UpperAlertLimit = x.UpperAlertLimit,
                                               UpperLimit = x.UpperLimit,
                                               AccumulationBase = x.AccumulationBase,
                                               Unit = x.Unit,
                                               TagID = x.TagID
                                           }).ToList(),
                        UBCheckItemList = (from x in db.PortCheckItem
                                           join c in db.CheckItem
                                           on x.CheckItemUniqueID equals c.UniqueID
                                           where x.PortUniqueID == query.UniqueID && x.CheckType == "U" && x.Procedure == "B"
                                           select new CheckItemModel
                                           {
                                               UniqueID = c.UniqueID,
                                               CheckType = c.CheckType,
                                               ID = c.ID,
                                               Description = c.Description,
                                               IsFeelItem = c.IsFeelItem,
                                               IsAccumulation = c.IsAccumulation,
                                               IsInherit = x.IsInherit,
                                               OriLowerLimit = c.LowerLimit,
                                               OriLowerAlertLimit = c.LowerAlertLimit,
                                               OriUpperAlertLimit = c.UpperAlertLimit,
                                               OriUpperLimit = c.UpperLimit,
                                               OriAccumulationBase = c.AccumulationBase,
                                               OriUnit = c.Unit,
                                               OriRemark = c.Remark,
                                               LowerLimit = x.LowerLimit,
                                               LowerAlertLimit = x.LowerAlertLimit,
                                               UpperAlertLimit = x.UpperAlertLimit,
                                               UpperLimit = x.UpperLimit,
                                               AccumulationBase = x.AccumulationBase,
                                               Unit = x.Unit,
                                               TagID = x.TagID
                                           }).ToList(),
                        UPCheckItemList = (from x in db.PortCheckItem
                                           join c in db.CheckItem
                                           on x.CheckItemUniqueID equals c.UniqueID
                                           where x.PortUniqueID == query.UniqueID && x.CheckType == "U" && x.Procedure == "P"
                                           select new CheckItemModel
                                           {
                                               UniqueID = c.UniqueID,
                                               CheckType = c.CheckType,
                                               ID = c.ID,
                                               Description = c.Description,
                                               IsFeelItem = c.IsFeelItem,
                                               IsAccumulation = c.IsAccumulation,
                                               IsInherit = x.IsInherit,
                                               OriLowerLimit = c.LowerLimit,
                                               OriLowerAlertLimit = c.LowerAlertLimit,
                                               OriUpperAlertLimit = c.UpperAlertLimit,
                                               OriUpperLimit = c.UpperLimit,
                                               OriAccumulationBase = c.AccumulationBase,
                                               OriUnit = c.Unit,
                                               OriRemark = c.Remark,
                                               LowerLimit = x.LowerLimit,
                                               LowerAlertLimit = x.LowerAlertLimit,
                                               UpperAlertLimit = x.UpperAlertLimit,
                                               UpperLimit = x.UpperLimit,
                                               AccumulationBase = x.AccumulationBase,
                                               Unit = x.Unit,
                                               TagID = x.TagID
                                           }).ToList(),
                        UACheckItemList = (from x in db.PortCheckItem
                                           join c in db.CheckItem
                                           on x.CheckItemUniqueID equals c.UniqueID
                                           where x.PortUniqueID == query.UniqueID && x.CheckType == "U" && x.Procedure == "A"
                                           select new CheckItemModel
                                           {
                                               UniqueID = c.UniqueID,
                                               CheckType = c.CheckType,
                                               ID = c.ID,
                                               Description = c.Description,
                                               IsFeelItem = c.IsFeelItem,
                                               IsAccumulation = c.IsAccumulation,
                                               IsInherit = x.IsInherit,
                                               OriLowerLimit = c.LowerLimit,
                                               OriLowerAlertLimit = c.LowerAlertLimit,
                                               OriUpperAlertLimit = c.UpperAlertLimit,
                                               OriUpperLimit = c.UpperLimit,
                                               OriAccumulationBase = c.AccumulationBase,
                                               OriUnit = c.Unit,
                                               OriRemark = c.Remark,
                                               LowerLimit = x.LowerLimit,
                                               LowerAlertLimit = x.LowerAlertLimit,
                                               UpperAlertLimit = x.UpperAlertLimit,
                                               UpperLimit = x.UpperLimit,
                                               AccumulationBase = x.AccumulationBase,
                                               Unit = x.Unit,
                                               TagID = x.TagID
                                           }).ToList(),
                        UDCheckItemList = (from x in db.PortCheckItem
                                           join c in db.CheckItem
                                           on x.CheckItemUniqueID equals c.UniqueID
                                           where x.PortUniqueID == query.UniqueID && x.CheckType == "U" && x.Procedure == "D"
                                           select new CheckItemModel
                                           {
                                               UniqueID = c.UniqueID,
                                               CheckType = c.CheckType,
                                               ID = c.ID,
                                               Description = c.Description,
                                               IsFeelItem = c.IsFeelItem,
                                               IsAccumulation = c.IsAccumulation,
                                               IsInherit = x.IsInherit,
                                               OriLowerLimit = c.LowerLimit,
                                               OriLowerAlertLimit = c.LowerAlertLimit,
                                               OriUpperAlertLimit = c.UpperAlertLimit,
                                               OriUpperLimit = c.UpperLimit,
                                               OriAccumulationBase = c.AccumulationBase,
                                               OriUnit = c.Unit,
                                               OriRemark = c.Remark,
                                               LowerLimit = x.LowerLimit,
                                               LowerAlertLimit = x.LowerAlertLimit,
                                               UpperAlertLimit = x.UpperAlertLimit,
                                               UpperLimit = x.UpperLimit,
                                               AccumulationBase = x.AccumulationBase,
                                               Unit = x.Unit,
                                               TagID = x.TagID
                                           }).ToList()
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
                    
                    var port = db.Port.First(x => x.UniqueID == Model.UniqueID);

                    var query = db.Port.FirstOrDefault(x => x.UniqueID != port.UniqueID && x.IslandUniqueID == Model.FormInput.IslandUniqueID && x.ID == Model.FormInput.ID);

                    if (query == null)
                    {
#if !DEBUG
                        using (TransactionScope trans = new TransactionScope())
                        {
#endif
                            port.ID = Model.FormInput.ID;
                            port.Description = Model.FormInput.Description;
                            port.TagID = Model.FormInput.TagID;
                            port.LB2LPTimeSpan = Model.FormInput.LB2LPTimeSpan;
                            port.LP2LATimeSpan = Model.FormInput.LP2LATimeSpan;
                            port.LA2LDTimeSpan = Model.FormInput.LA2LDTimeSpan;
                            port.UB2UPTimeSpan = Model.FormInput.UB2UPTimeSpan;
                            port.UP2UATimeSpan = Model.FormInput.UP2UATimeSpan;
                            port.UA2UDTimeSpan = Model.FormInput.UA2UDTimeSpan;

                            db.SaveChanges();

                            db.PortCheckItem.RemoveRange(db.PortCheckItem.Where(x => x.PortUniqueID == port.UniqueID).ToList());

                            db.SaveChanges();

                            db.PortCheckItem.AddRange(Model.LBCheckItemList.Select(x => new PortCheckItem
                            {
                                PortUniqueID = port.UniqueID,
                                CheckItemUniqueID = x.UniqueID,
                                CheckType = "L",
                                Procedure = "B",
                                TagID = x.TagID,
                                IsInherit = x.IsInherit,
                                LowerLimit = x.LowerLimit,
                                LowerAlertLimit = x.LowerAlertLimit,
                                UpperAlertLimit = x.UpperAlertLimit,
                                UpperLimit = x.UpperLimit,
                                AccumulationBase = x.AccumulationBase,
                                Unit = x.Unit
                            }).ToList());

                            db.PortCheckItem.AddRange(Model.LPCheckItemList.Select(x => new PortCheckItem
                            {
                                PortUniqueID = port.UniqueID,
                                CheckItemUniqueID = x.UniqueID,
                                CheckType = "L",
                                Procedure = "P",
                                TagID = x.TagID,
                                IsInherit = x.IsInherit,
                                LowerLimit = x.LowerLimit,
                                LowerAlertLimit = x.LowerAlertLimit,
                                UpperAlertLimit = x.UpperAlertLimit,
                                UpperLimit = x.UpperLimit,
                                AccumulationBase = x.AccumulationBase,
                                Unit = x.Unit
                            }).ToList());

                            db.PortCheckItem.AddRange(Model.LACheckItemList.Select(x => new PortCheckItem
                            {
                                PortUniqueID = port.UniqueID,
                                CheckItemUniqueID = x.UniqueID,
                                CheckType = "L",
                                Procedure = "A",
                                TagID = x.TagID,
                                IsInherit = x.IsInherit,
                                LowerLimit = x.LowerLimit,
                                LowerAlertLimit = x.LowerAlertLimit,
                                UpperAlertLimit = x.UpperAlertLimit,
                                UpperLimit = x.UpperLimit,
                                AccumulationBase = x.AccumulationBase,
                                Unit = x.Unit
                            }).ToList());

                            db.PortCheckItem.AddRange(Model.LDCheckItemList.Select(x => new PortCheckItem
                            {
                                PortUniqueID = port.UniqueID,
                                CheckItemUniqueID = x.UniqueID,
                                CheckType = "L",
                                Procedure = "D",
                                TagID = x.TagID,
                                IsInherit = x.IsInherit,
                                LowerLimit = x.LowerLimit,
                                LowerAlertLimit = x.LowerAlertLimit,
                                UpperAlertLimit = x.UpperAlertLimit,
                                UpperLimit = x.UpperLimit,
                                AccumulationBase = x.AccumulationBase,
                                Unit = x.Unit
                            }).ToList());

                            db.PortCheckItem.AddRange(Model.UBCheckItemList.Select(x => new PortCheckItem
                            {
                                PortUniqueID = port.UniqueID,
                                CheckItemUniqueID = x.UniqueID,
                                CheckType = "U",
                                Procedure = "B",
                                TagID = x.TagID,
                                IsInherit = x.IsInherit,
                                LowerLimit = x.LowerLimit,
                                LowerAlertLimit = x.LowerAlertLimit,
                                UpperAlertLimit = x.UpperAlertLimit,
                                UpperLimit = x.UpperLimit,
                                AccumulationBase = x.AccumulationBase,
                                Unit = x.Unit
                            }).ToList());

                            db.PortCheckItem.AddRange(Model.UPCheckItemList.Select(x => new PortCheckItem
                            {
                                PortUniqueID = port.UniqueID,
                                CheckItemUniqueID = x.UniqueID,
                                CheckType = "U",
                                Procedure = "P",
                                TagID = x.TagID,
                                IsInherit = x.IsInherit,
                                LowerLimit = x.LowerLimit,
                                LowerAlertLimit = x.LowerAlertLimit,
                                UpperAlertLimit = x.UpperAlertLimit,
                                UpperLimit = x.UpperLimit,
                                AccumulationBase = x.AccumulationBase,
                                Unit = x.Unit
                            }).ToList());

                            db.PortCheckItem.AddRange(Model.UACheckItemList.Select(x => new PortCheckItem
                            {
                                PortUniqueID = port.UniqueID,
                                CheckItemUniqueID = x.UniqueID,
                                CheckType = "U",
                                Procedure = "A",
                                TagID = x.TagID,
                                IsInherit = x.IsInherit,
                                LowerLimit = x.LowerLimit,
                                LowerAlertLimit = x.LowerAlertLimit,
                                UpperAlertLimit = x.UpperAlertLimit,
                                UpperLimit = x.UpperLimit,
                                AccumulationBase = x.AccumulationBase,
                                Unit = x.Unit
                            }).ToList());

                            db.PortCheckItem.AddRange(Model.UDCheckItemList.Select(x => new PortCheckItem
                            {
                                PortUniqueID = port.UniqueID,
                                CheckItemUniqueID = x.UniqueID,
                                CheckType = "U",
                                Procedure = "D",
                                TagID = x.TagID,
                                IsInherit = x.IsInherit,
                                LowerLimit = x.LowerLimit,
                                LowerAlertLimit = x.LowerAlertLimit,
                                UpperAlertLimit = x.UpperAlertLimit,
                                UpperLimit = x.UpperLimit,
                                AccumulationBase = x.AccumulationBase,
                                Unit = x.Unit
                            }).ToList());

                            db.SaveChanges();

#if !DEBUG
                            trans.Complete();
                        }
#endif

                        result.ReturnSuccessMessage("編輯灌口成功");
                    }
                    else
                    {
                        result.ReturnFailedMessage("灌口代號已存在");
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
                    db.PortCheckItem.RemoveRange(db.PortCheckItem.Where(x=>x.PortUniqueID==UniqueID).ToList());

                    db.Port.Remove(db.Port.First(x => x.UniqueID == UniqueID));

                    db.SaveChanges();

                    result.ReturnSuccessMessage("刪除灌口成功");
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

        public static RequestResult SavePageState(List<CheckItemModel> CheckItemList, List<string> PageStateList)
        {
            RequestResult result = new RequestResult();

            try
            {
                foreach (string pageState in PageStateList)
                {
                    string[] temp = pageState.Split(Define.Seperators, StringSplitOptions.None);

                    string isInherit = temp[0];
                    string checkItemUniqueID = temp[1];
                    string tagID = temp[2];
                    string lowerLimit = temp[3];
                    string lowerAlertLimit = temp[4];
                    string upperAlertLimit = temp[5];
                    string upperLimit = temp[6];
                    string accumulationBase = temp[7];
                    string unit = temp[8];
                    string remark = temp[9];

                    var checkItem = CheckItemList.First(x => x.UniqueID == checkItemUniqueID);

                    checkItem.IsInherit = isInherit == "Y";
                    checkItem.TagID = tagID;

                    if (!checkItem.IsInherit)
                    {
                        checkItem.LowerLimit = !string.IsNullOrEmpty(lowerLimit) ? double.Parse(lowerLimit) : default(double?);
                        checkItem.LowerAlertLimit = !string.IsNullOrEmpty(lowerAlertLimit) ? double.Parse(lowerAlertLimit) : default(double?);
                        checkItem.UpperAlertLimit = !string.IsNullOrEmpty(upperAlertLimit) ? double.Parse(upperAlertLimit) : default(double?);
                        checkItem.UpperLimit = !string.IsNullOrEmpty(upperLimit) ? double.Parse(upperLimit) : default(double?);
                        checkItem.AccumulationBase = !string.IsNullOrEmpty(accumulationBase) ? double.Parse(accumulationBase) : default(double?);
                        checkItem.Unit = unit;
                        checkItem.Remark = remark;
                    }
                    else
                    {
                        checkItem.LowerLimit = null;
                        checkItem.LowerAlertLimit = null;
                        checkItem.UpperAlertLimit = null;
                        checkItem.UpperLimit = null;
                        checkItem.AccumulationBase = null;
                        checkItem.Unit = string.Empty;
                        checkItem.Remark = string.Empty;
                    }
                }

                result.ReturnData(CheckItemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult AddCheckItem(List<CheckItemModel> CheckItemList, List<string> SelectedList, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (TankDbEntities db = new TankDbEntities())
                {
                    foreach (string selected in SelectedList)
                    {
                        string[] temp = selected.Split(Define.Seperators, StringSplitOptions.None);

                        var organizationUniqueID = temp[0];
                        var checkType = temp[1];
                        var checkItemUniqueID = temp[2];

                        if (!string.IsNullOrEmpty(checkItemUniqueID))
                        {
                            if (!CheckItemList.Any(x => x.UniqueID == checkItemUniqueID))
                            {
                                var checkItem = db.CheckItem.First(x => x.UniqueID == checkItemUniqueID);

                                CheckItemList.Add(new CheckItemModel()
                                {
                                    UniqueID = checkItem.UniqueID,
                                    CheckType = checkItem.CheckType,
                                    ID = checkItem.ID,
                                    Description = checkItem.Description,
                                    IsFeelItem = checkItem.IsFeelItem,
                                    IsAccumulation = checkItem.IsAccumulation,
                                    IsInherit = true,
                                    OriLowerLimit = checkItem.LowerLimit,
                                    OriLowerAlertLimit = checkItem.LowerAlertLimit,
                                    OriUpperAlertLimit = checkItem.UpperAlertLimit,
                                    OriUpperLimit = checkItem.UpperLimit,
                                    OriAccumulationBase = checkItem.AccumulationBase,
                                    OriUnit = checkItem.Unit,
                                    OriRemark = checkItem.Remark
                                });
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(checkType))
                            {
                                var checkItemList = db.CheckItem.Where(x => x.OrganizationUniqueID == organizationUniqueID && x.CheckType == checkType).ToList();

                                foreach (var checkItem in checkItemList)
                                {
                                    if (!CheckItemList.Any(x => x.UniqueID == checkItem.UniqueID))
                                    {
                                        CheckItemList.Add(new CheckItemModel()
                                        {
                                            UniqueID = checkItem.UniqueID,
                                            CheckType = checkItem.CheckType,
                                            ID = checkItem.ID,
                                            Description = checkItem.Description,
                                            IsFeelItem = checkItem.IsFeelItem,
                                            IsAccumulation = checkItem.IsAccumulation,
                                            IsInherit = true,
                                            OriLowerLimit = checkItem.LowerLimit,
                                            OriLowerAlertLimit = checkItem.LowerAlertLimit,
                                            OriUpperAlertLimit = checkItem.UpperAlertLimit,
                                            OriUpperLimit = checkItem.UpperLimit,
                                            OriAccumulationBase = checkItem.AccumulationBase,
                                            OriUnit = checkItem.Unit,
                                            OriRemark = checkItem.Remark
                                        });
                                    }
                                }
                            }
                            else
                            {
                                var availableOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(RefOrganizationUniqueID, true);

                                var organizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organizationUniqueID, true);

                                foreach (var organization in organizationList)
                                {
                                    if (availableOrganizationList.Any(x => x == organization))
                                    {
                                        var checkItemList = db.CheckItem.Where(x => x.OrganizationUniqueID == organization).ToList();

                                        foreach (var checkItem in checkItemList)
                                        {
                                            if (!CheckItemList.Any(x => x.UniqueID == checkItem.UniqueID))
                                            {
                                                CheckItemList.Add(new CheckItemModel()
                                                {
                                                    UniqueID = checkItem.UniqueID,
                                                    CheckType = checkItem.CheckType,
                                                    ID = checkItem.ID,
                                                    Description = checkItem.Description,
                                                    IsFeelItem = checkItem.IsFeelItem,
                                                    IsAccumulation = checkItem.IsAccumulation,
                                                    IsInherit = true,
                                                    OriLowerLimit = checkItem.LowerLimit,
                                                    OriLowerAlertLimit = checkItem.LowerAlertLimit,
                                                    OriUpperAlertLimit = checkItem.UpperAlertLimit,
                                                    OriUpperLimit = checkItem.UpperLimit,
                                                    OriAccumulationBase = checkItem.AccumulationBase,
                                                    OriUnit = checkItem.Unit,
                                                    OriRemark = checkItem.Remark
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                CheckItemList = CheckItemList.OrderBy(x => x.CheckType).ThenBy(x => x.ID).ToList();

                result.ReturnData(CheckItemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetTreeItem(List<Models.Shared.Organization> OrganizationList, string OrganizationUniqueID, string StationUniqueID, string IslandUniqueID, Account Account)
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
                    { Define.EnumTreeAttribute.IslandUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.PortUniqueID, string.Empty }
                };

                using (TankDbEntities tankDb = new TankDbEntities())
                {
                    if (!string.IsNullOrEmpty(IslandUniqueID))
                    {
                        var portList = tankDb.Port.Where(x => x.IslandUniqueID == IslandUniqueID).OrderBy(x => x.ID).ToList();

                        foreach (var port in portList)
                        {
                            var treeItem = new TreeItem() { Title = port.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Port.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", port.ID, port.Description);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.StationUniqueID] = StationUniqueID;
                            attributes[Define.EnumTreeAttribute.IslandUniqueID] = IslandUniqueID;
                            attributes[Define.EnumTreeAttribute.PortUniqueID] = port.UniqueID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else if (!string.IsNullOrEmpty(StationUniqueID))
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
                            attributes[Define.EnumTreeAttribute.PortUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (tankDb.Port.Any(x => x.IslandUniqueID == island.UniqueID))
                            {
                                treeItem.State = "closed";
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
                                var treeItem = new TreeItem() { Title = station.Description };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Station.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", station.ID, station.Description);
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                                attributes[Define.EnumTreeAttribute.StationUniqueID] = station.UniqueID;
                                attributes[Define.EnumTreeAttribute.IslandUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.PortUniqueID] = string.Empty;

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
                            attributes[Define.EnumTreeAttribute.PortUniqueID] = string.Empty;

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
                    { Define.EnumTreeAttribute.IslandUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.PortUniqueID, string.Empty }
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
                    attributes[Define.EnumTreeAttribute.PortUniqueID] = string.Empty;

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
