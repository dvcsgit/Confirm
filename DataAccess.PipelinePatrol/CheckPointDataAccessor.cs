using DbEntity.MSSQL;
using DbEntity.MSSQL.PipelinePatrol;
using Models.Authenticated;
using Models.PipelinePatrol.CheckPointManagement;
using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using System.Transactions;
using Utility.Models;

namespace DataAccess.PipelinePatrol
{
    public class CheckPointDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.PipePoint.Where(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.PipePointType))
                    {
                        query = query.Where(x => x.OrganizationUniqueID == Parameters.OrganizationUniqueID && x.PointType == Parameters.PipePointType);
                    }

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.ID.Contains(Parameters.Keyword) || x.Name.Contains(Parameters.Keyword));
                    }

                    var organization = OrganizationDataAccessor.GetOrganization(Parameters.OrganizationUniqueID);

                    result.ReturnData(new GridViewModel()
                    {
                        FullOrganizationDescription = organization.FullDescription,
                        ItemList = query.ToList().Select(x => new GridItem()
                        {
                            UniqueID = x.UniqueID,
                            Permission = Account.OrganizationPermission(x.OrganizationUniqueID),
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.OrganizationUniqueID),
                            PipePointType = x.PointType,
                            ID = x.ID,
                            Name = x.Name
                        }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.PipePointType).ThenBy(x => x.ID).ToList()
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
                using (PDbEntities db = new PDbEntities())
                {
                    var pipePoint = db.PipePoint.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = pipePoint.UniqueID,
                        Permission = Account.OrganizationPermission(pipePoint.OrganizationUniqueID),
                        OrganizationUniqueID = pipePoint.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(pipePoint.OrganizationUniqueID),
                        PipePointType = pipePoint.PointType,
                        ID = pipePoint.ID,
                        Name = pipePoint.Name,
                        IsFeelItemDefaultNormal = pipePoint.IsFeelItemDefaultNormal,
                        CheckItemList = (from x in db.PipePointCheckItem
                                         join c in db.CheckItem
                                         on x.CheckItemUniqueID equals c.UniqueID
                                         where x.PipePointUniqueID == pipePoint.UniqueID
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
                                             Remark = x.Remark
                                         }).OrderBy(x => x.CheckType).ThenBy(x => x.ID).ToList()
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

        public static RequestResult GetEditFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    var pipePoint = db.PipePoint.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = pipePoint.UniqueID,
                        OrganizationUniqueID = pipePoint.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(pipePoint.OrganizationUniqueID),
                         PipePointType= pipePoint.PointType,
                         ID=pipePoint.ID,
                         Name=pipePoint.Name,
                        CheckItemList = (from x in db.PipePointCheckItem
                                         join c in db.CheckItem
                                         on x.CheckItemUniqueID equals c.UniqueID
                                         where x.PipePointUniqueID == pipePoint.UniqueID
                                         orderby c.CheckType, c.ID
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
                                             Remark = x.Remark
                                         }).ToList(),
                        FormInput = new FormInput()
                        {
                            IsFeelItemDefaultNormal = pipePoint.IsFeelItemDefaultNormal,
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
                using (PDbEntities db = new PDbEntities())
                {
                    var pipePoint = db.PipePoint.First(x => x.UniqueID == Model.UniqueID);

#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                    #region ControlPoint
                    pipePoint.IsFeelItemDefaultNormal = Model.FormInput.IsFeelItemDefaultNormal;
                    pipePoint.LastModifyTime = DateTime.Now;

                    db.SaveChanges();
                    #endregion

                    #region PipePointCheckItem
                    #region Delete
                    db.PipePointCheckItem.RemoveRange(db.PipePointCheckItem.Where(x => x.PipePointUniqueID == Model.UniqueID).ToList());

                    db.SaveChanges();
                    #endregion

                    #region Insert
                    db.PipePointCheckItem.AddRange(Model.CheckItemList.Select(x => new PipePointCheckItem
                    {
                        PipePointUniqueID = pipePoint.UniqueID,
                        CheckItemUniqueID = x.UniqueID,
                        IsInherit = x.IsInherit,
                        LowerLimit = x.LowerLimit,
                        LowerAlertLimit = x.LowerAlertLimit,
                        UpperAlertLimit = x.UpperAlertLimit,
                        UpperLimit = x.UpperLimit,
                        AccumulationBase = x.AccumulationBase,
                        Unit = x.Unit,
                        Remark = x.Remark
                    }).ToList());

                    db.SaveChanges();
                    #endregion
                    #endregion
#if !DEBUG
                        trans.Complete();
                    }
#endif
                    result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.CheckPoint, Resources.Resource.Success));
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
                    string lowerLimit = temp[2];
                    string lowerAlertLimit = temp[3];
                    string upperAlertLimit = temp[4];
                    string upperLimit = temp[5];
                    string accumulationBase = temp[6];
                    string unit = temp[7];
                    string remark = temp[8];

                    var checkItem = CheckItemList.First(x => x.UniqueID == checkItemUniqueID);

                    checkItem.IsInherit = isInherit == "Y";

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
                using (PDbEntities db = new PDbEntities())
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
        public static RequestResult GetTreeItem(string OrganizationUniqueID, string PipePointType, Account Account)
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
                    { Define.EnumTreeAttribute.PipePointType, string.Empty },
                    { Define.EnumTreeAttribute.PipePointUniqueID, string.Empty }
                };

                using (PDbEntities edb = new PDbEntities())
                {
                    if (string.IsNullOrEmpty(PipePointType))
                    {
                        if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                        {
                            var pipePointTypeList = edb.PipePoint.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).Select(x => x.PointType).Distinct().OrderBy(x => x).ToList();

                            foreach (var pipePointType in pipePointTypeList)
                            {
                                var treeItem = new TreeItem() { Title = pipePointType };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.PipePointType.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = pipePointType;
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                                attributes[Define.EnumTreeAttribute.PipePointType] = pipePointType;
                                attributes[Define.EnumTreeAttribute.PipePointUniqueID] = string.Empty;

                                foreach (var attribute in attributes)
                                {
                                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                }

                                treeItem.State = "closed";

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
                                attributes[Define.EnumTreeAttribute.PipePointType] = string.Empty;
                                attributes[Define.EnumTreeAttribute.PipePointUniqueID] = string.Empty;

                                foreach (var attribute in attributes)
                                {
                                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                }

                                if (db.Organization.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                                    ||
                                    (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && edb.PipePoint.Any(x => x.OrganizationUniqueID == organization.UniqueID)))
                                {
                                    treeItem.State = "closed";
                                }

                                treeItemList.Add(treeItem);
                            }
                        }
                    }
                    else
                    {
                        var pipePointList = edb.PipePoint.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && x.PointType == PipePointType).OrderBy(x => x.ID).ToList();

                        foreach (var pipePoint in pipePointList)
                        {
                            var treeItem = new TreeItem() { Title = pipePoint.Name };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.PipePoint.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", pipePoint.ID, pipePoint.Name);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.PipePointType] = pipePoint.PointType;
                            attributes[Define.EnumTreeAttribute.PipePointUniqueID] = pipePoint.UniqueID;

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

        public static RequestResult GetTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID, string PipePointType)
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
                    { Define.EnumTreeAttribute.PipePointType, string.Empty },
                    { Define.EnumTreeAttribute.PipePointUniqueID, string.Empty }
                };

                using (PDbEntities edb = new PDbEntities())
                {
                    if (string.IsNullOrEmpty(PipePointType))
                    {
                        var pipePointTypeList = edb.PipePoint.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).Select(x => x.PointType).Distinct().OrderBy(x => x).ToList();

                        foreach (var pipePointType in pipePointTypeList)
                        {
                            var treeItem = new TreeItem() { Title = pipePointType };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.PipePointType.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = pipePointType;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.PipePointType] = pipePointType;
                            attributes[Define.EnumTreeAttribute.PipePointUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItem.State = "closed";

                            treeItemList.Add(treeItem);
                        }

                        var availableOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(RefOrganizationUniqueID, true);

                        using (DbEntities db = new DbEntities())
                        {
                            var organizationList = db.Organization.Where(x => x.ParentUniqueID == OrganizationUniqueID && availableOrganizationList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                            foreach (var organization in organizationList)
                            {
                                var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organization.UniqueID, true);

                                if (edb.PipePoint.Any(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && availableOrganizationList.Contains(x.OrganizationUniqueID)))
                                {
                                    var treeItem = new TreeItem() { Title = organization.Description };

                                    attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                                    attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                                    attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                                    attributes[Define.EnumTreeAttribute.PipePointType] = string.Empty;
                                    attributes[Define.EnumTreeAttribute.PipePointUniqueID] = string.Empty;

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
                    else
                    {
                        var pipePointList = edb.PipePoint.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && x.PointType == PipePointType).OrderBy(x => x.ID).ToList();

                        foreach (var pipePoint in pipePointList)
                        {
                            var treeItem = new TreeItem() { Title = pipePoint.Name };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.PipePoint.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", pipePoint.ID, pipePoint.Name);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = pipePoint.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.PipePointType] = pipePoint.PointType;
                            attributes[Define.EnumTreeAttribute.PipePointUniqueID] = pipePoint.UniqueID;

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
    }
}
