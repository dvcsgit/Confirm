using DataAccess;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.EquipmentMaintenance.TrendQuery_CheckItem;
using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace Customized.CHIMEI.DataAccess
{
    public class TrendQuery_CheckItemHelper
    {
        public static RequestResult Query(string CheckItemUniqueID, string Keyword)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new GridViewModel()
                {
                    Parameters = new QueryParameters()
                    {
                        CheckItemUniqueID = CheckItemUniqueID,
                        BeginDateString = DateTimeHelper.DateTime2DateStringWithSeperator(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)),
                        EndDateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today)
                    }
                };

                using (EDbEntities db = new EDbEntities())
                {
                    var checkItem = db.CheckItem.First(x => x.UniqueID == CheckItemUniqueID);

                    model.CheckItemDescription = checkItem.Description;

                    var controlPointQuery = (from x in db.View_ControlPointCheckItem
                                            join p in db.ControlPoint
                                            on x.ControlPointUniqueID equals p.UniqueID
                                            join c in db.CheckItem
                                            on x.CheckItemUniqueID equals c.UniqueID
                                            where x.CheckItemUniqueID == CheckItemUniqueID
                                            select new
                                            {
                                                p.UniqueID,
                                                p.OrganizationUniqueID,
                                                p.ID,
                                                p.Description,
                                                x.LowerLimit,
                                                x.LowerAlertLimit,
                                                x.UpperAlertLimit,
                                                x.UpperLimit
                                            }).AsQueryable();

                    if (!string.IsNullOrEmpty(Keyword))
                    {
                        controlPointQuery = controlPointQuery.Where(x => x.ID.Contains(Keyword) || x.Description.Contains(Keyword));
                    }

                    var controlPointList = controlPointQuery.ToList();

                    foreach (var controlPoint in controlPointList)
                    {
                        model.ItemList.Add(new GridItem()
                        {
                            Type = Define.EnumTreeNodeType.ControlPoint,
                            ControlPointUniqueID = controlPoint.UniqueID,
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(controlPoint.OrganizationUniqueID),
                            ControlPointID = controlPoint.ID,
                            ControlPointDescription = controlPoint.Description,
                            EquipmentUniqueID = string.Empty,
                            PartUniqueID = string.Empty,
                            EquipmentID = string.Empty,
                            EquipmentName = string.Empty,
                            PartDescription = string.Empty,
                            LowerLimit = controlPoint.LowerLimit,
                            LowerAlertLimit = controlPoint.LowerAlertLimit,
                            UpperAlertLimit = controlPoint.UpperAlertLimit,
                            UpperLimit = controlPoint.UpperLimit
                        });
                    }

                    var equipmentQuery = (from x in db.View_EquipmentCheckItem
                                         join e in db.Equipment
                                         on x.EquipmentUniqueID equals e.UniqueID
                                         join p in db.EquipmentPart
                                         on x.PartUniqueID equals p.UniqueID into tmpPart
                                         from p in tmpPart.DefaultIfEmpty()
                                         join c in db.CheckItem
                                         on x.CheckItemUniqueID equals c.UniqueID
                                         where x.CheckItemUniqueID == CheckItemUniqueID
                                         select new
                                         {
                                             e.OrganizationUniqueID,
                                             e.UniqueID,
                                             x.PartUniqueID,
                                             e.ID,
                                             e.Name,
                                             PartDescription = p != null ? p.Description : "",
                                             x.LowerLimit,
                                             x.LowerAlertLimit,
                                             x.UpperAlertLimit,
                                             x.UpperLimit
                                         }).AsQueryable();

                    if (!string.IsNullOrEmpty(Keyword))
                    {
                        equipmentQuery = equipmentQuery.Where(x => x.ID.Contains(Keyword) || x.Name.Contains(Keyword));
                    }

                    var equipmentList = equipmentQuery.ToList();

                    foreach (var equipment in equipmentList)
                    {
                        if (equipment.PartUniqueID == "*")
                        {
                            model.ItemList.Add(new GridItem()
                            {
                                Type = Define.EnumTreeNodeType.Equipment,
                                ControlPointUniqueID = string.Empty,
                                OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(equipment.OrganizationUniqueID),
                                ControlPointID = string.Empty,
                                ControlPointDescription = string.Empty,
                                EquipmentUniqueID = equipment.UniqueID,
                                PartUniqueID = equipment.PartUniqueID,
                                EquipmentID = equipment.ID,
                                EquipmentName = equipment.Name,
                                PartDescription = equipment.PartDescription,
                                LowerLimit = equipment.LowerLimit,
                                LowerAlertLimit = equipment.LowerAlertLimit,
                                UpperAlertLimit = equipment.UpperAlertLimit,
                                UpperLimit = equipment.UpperLimit
                            });
                        }
                        else
                        {
                            model.ItemList.Add(new GridItem()
                            {
                                Type = Define.EnumTreeNodeType.EquipmentPart,
                                ControlPointUniqueID = string.Empty,
                                OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(equipment.OrganizationUniqueID),
                                ControlPointID = string.Empty,
                                ControlPointDescription = string.Empty,
                                EquipmentUniqueID = equipment.UniqueID,
                                PartUniqueID = equipment.PartUniqueID,
                                EquipmentID = equipment.ID,
                                EquipmentName = equipment.Name,
                                PartDescription = equipment.PartDescription,
                                LowerLimit = equipment.LowerLimit,
                                LowerAlertLimit = equipment.LowerAlertLimit,
                                UpperAlertLimit = equipment.UpperAlertLimit,
                                UpperLimit = equipment.UpperLimit
                            });
                        }
                    }
                }

                model.ItemList = model.ItemList.OrderBy(x => x.OrganizationDescription).ThenBy(x => x.ID).ToList();

                result.ReturnData(model);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodInfo.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult Draw(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                var colorList = new List<string>() 
                { 
                    "#a47ae2",
                    "#cd74e6",
                    "#f691b2",
                    "#cca6ac",
                    "#cabdbf",
                    "#c2c2c2",
                    "#b99aff",
                    "#9a9cff",
                    "#4986e7",
                    "#9fc6e7",
                    "#9fe1e7",
                    "#92e1c0",
                    "#fad165",
                    "#fbe983",
                    "#b3dc6c",
                    "#7bd148",
                    "#16a765",
                    "#42d692",
                    "#ffad46",
                    "#ac725e"
                };

                var colorSeq = 0;

                using (EDbEntities db = new EDbEntities())
                {
                    var checkItem = db.CheckItem.First(x => x.UniqueID == Parameters.CheckItemUniqueID);

                    var model = new ChartViewModel()
                    {
                        CheckItemDescription = checkItem.Description
                    };

                    var controlPointList = (from x in db.View_ControlPointCheckItem
                                            join p in db.ControlPoint
                                            on x.ControlPointUniqueID equals p.UniqueID
                                            where x.CheckItemUniqueID == Parameters.CheckItemUniqueID && Parameters.ControlPointList.Contains(p.UniqueID)
                                            select new
                                            {
                                                p.UniqueID,
                                                p.Description,
                                                x.LowerLimit,
                                                x.LowerAlertLimit,
                                                x.UpperAlertLimit,
                                                x.UpperLimit
                                            }).ToList();

                    foreach (var controlPoint in controlPointList)
                    {
                        var checkResultList = db.CheckResult.Where(x => x.ControlPointUniqueID == controlPoint.UniqueID && string.IsNullOrEmpty(x.EquipmentUniqueID) && x.CheckItemUniqueID == Parameters.CheckItemUniqueID && (x.NetValue.HasValue || x.Value.HasValue) && string.Compare(x.CheckDate, Parameters.BeginDate) >= 0 && string.Compare(x.CheckDate, Parameters.EndDate) <= 0).Select(x => new CheckResultModel
                        {
                            CheckDate = x.CheckDate,
                            CheckTime = x.CheckTime,
                            NetValue = x.NetValue,
                            Value = x.Value
                        }).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

                        if (checkResultList.Count > 1)
                        {
                            model.EquipmentList.Add(new EquipmentModel()
                            {
                                ControlPointDescription = controlPoint.Description,
                                EquipmentName = string.Empty,
                                PartDescription = string.Empty,
                                LowerLimit = controlPoint.LowerLimit,
                                LowerAlertLimit = controlPoint.LowerAlertLimit,
                                UpperAlertLimit = controlPoint.UpperAlertLimit,
                                UpperLimit = controlPoint.UpperLimit,
                                CheckResultList = checkResultList,
                                Color = colorList[colorSeq]
                            });

                            colorSeq++;
                        }
                    }

                    var equipmentList = (from x in db.View_EquipmentCheckItem
                                         join e in db.Equipment
                                         on x.EquipmentUniqueID equals e.UniqueID
                                         where x.CheckItemUniqueID == Parameters.CheckItemUniqueID && x.PartUniqueID == "*" && Parameters.EquipmentList.Contains(e.UniqueID)
                                         select new
                                         {
                                             e.UniqueID,
                                             e.Name,
                                             x.LowerLimit,
                                             x.LowerAlertLimit,
                                             x.UpperAlertLimit,
                                             x.UpperLimit
                                         }).ToList();

                    foreach (var equipment in equipmentList)
                    {
                        var checkResultList = db.CheckResult.Where(x => x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == "*" && x.CheckItemUniqueID == Parameters.CheckItemUniqueID && (x.NetValue.HasValue || x.Value.HasValue) && string.Compare(x.CheckDate, Parameters.BeginDate) >= 0 && string.Compare(x.CheckDate, Parameters.EndDate) <= 0).Select(x => new CheckResultModel
                        {
                            CheckDate = x.CheckDate,
                            CheckTime = x.CheckTime,
                            NetValue = x.NetValue,
                            Value = x.Value
                        }).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

                        if (checkResultList.Count > 1)
                        {
                            model.EquipmentList.Add(new EquipmentModel()
                            {
                                ControlPointDescription = string.Empty,
                                EquipmentName = equipment.Name,
                                PartDescription = string.Empty,
                                LowerLimit=equipment.LowerLimit,
                                LowerAlertLimit = equipment.LowerAlertLimit,
                                UpperAlertLimit=equipment.UpperAlertLimit,
                                 UpperLimit=  equipment.UpperLimit,
                                CheckResultList = checkResultList,
                                Color=colorList[colorSeq]
                            });

                            colorSeq++;
                        }
                    }

                    var equipmentPartList = (from x in db.View_EquipmentCheckItem
                                             join e in db.Equipment
                                             on x.EquipmentUniqueID equals e.UniqueID
                                             join p in db.EquipmentPart
                                             on x.PartUniqueID equals p.UniqueID
                                             where x.CheckItemUniqueID == Parameters.CheckItemUniqueID && Parameters.EquipmentPartList.Contains(p.UniqueID)
                                             select new
                                             {
                                                 e.UniqueID,
                                                 EquipmentName=e.Name,
                                                 x.PartUniqueID,
                                                PartDescription= p.Description,
                                                 x.LowerLimit,
                                                 x.LowerAlertLimit,
                                                 x.UpperAlertLimit,
                                                 x.UpperLimit
                                             }).ToList();

                    foreach (var equipmentPart in equipmentPartList)
                    {
                        var checkResultList = db.CheckResult.Where(x => x.EquipmentUniqueID == equipmentPart.UniqueID && x.PartUniqueID == equipmentPart.PartUniqueID && x.CheckItemUniqueID == Parameters.CheckItemUniqueID && (x.NetValue.HasValue || x.Value.HasValue) && string.Compare(x.CheckDate, Parameters.BeginDate) >= 0 && string.Compare(x.CheckDate, Parameters.EndDate) <= 0).Select(x => new CheckResultModel
                        {
                            CheckDate = x.CheckDate,
                            CheckTime = x.CheckTime,
                            NetValue = x.NetValue,
                            Value = x.Value
                        }).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

                        if (checkResultList.Count > 1)
                        {
                            model.EquipmentList.Add(new EquipmentModel()
                            {
                                ControlPointDescription = string.Empty,
                                EquipmentName = equipmentPart.EquipmentName,
                                PartDescription = equipmentPart.PartDescription,
                                LowerLimit = equipmentPart.LowerLimit,
                                LowerAlertLimit = equipmentPart.LowerAlertLimit,
                                UpperAlertLimit = equipmentPart.UpperAlertLimit,
                                UpperLimit = equipmentPart.UpperLimit,
                                CheckResultList = checkResultList,
                                Color = colorList[colorSeq]
                            });

                            colorSeq++;
                        }
                    }

                    if (model.EquipmentList.Count > 0)
                    {
                        model.LowerLimit = model.EquipmentList.First().LowerLimit;
                        model.LowerAlertLimit = model.EquipmentList.First().LowerAlertLimit;
                        model.UpperAlertLimit = model.EquipmentList.First().UpperAlertLimit;
                        model.UpperLimit = model.EquipmentList.First().UpperLimit;

                        result.ReturnData(model);
                    }
                    else
                    {
                        result.ReturnFailedMessage("抄表資料不足，無法繪製趨勢圖");
                    }
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodInfo.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetTreeItem(List<Organization> OrganizationList, string OrganizationUniqueID, string CheckType)
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
                    { Define.EnumTreeAttribute.CheckType, string.Empty },
                    { Define.EnumTreeAttribute.CheckItemUniqueID, string.Empty }
                };

                using (EDbEntities edb = new EDbEntities())
                {
                    if (!string.IsNullOrEmpty(CheckType))
                    {
                        var checkItemList = (from c in edb.CheckItem
                                             join x in edb.EquipmentCheckItem
                                             on c.UniqueID equals x.CheckItemUniqueID into tmpE
                                             from x in tmpE.DefaultIfEmpty()
                                             join y in edb.ControlPointCheckItem
                                             on c.UniqueID equals y.CheckItemUniqueID into tmpC
                                             from y in tmpC.DefaultIfEmpty()
                                             where c.OrganizationUniqueID == OrganizationUniqueID && c.CheckType == CheckType && !c.IsFeelItem
                                             select new
                                             {
                                                 CheckItemUniqueID = c.UniqueID,
                                                 CheckItemID = c.ID,
                                                 CheckItemDescription = c.Description,
                                                 Equipment = x != null ? x.EquipmentUniqueID : "",
                                                 ControlPoint = y != null ? y.ControlPointUniqueID : ""
                                             }).Where(x => !string.IsNullOrEmpty(x.Equipment) || !string.IsNullOrEmpty(x.ControlPoint)).Select(x => new
                                             {
                                                 x.CheckItemUniqueID,
                                                 x.CheckItemID,
                                                 x.CheckItemDescription
                                             }).Distinct().OrderBy(x => x.CheckItemID).ToList();

                        foreach (var checkItem in checkItemList)
                        {
                            var treeItem = new TreeItem() { Title = checkItem.CheckItemDescription };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.CheckItem.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", checkItem.CheckItemID, checkItem.CheckItemDescription);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.CheckType] = CheckType;
                            attributes[Define.EnumTreeAttribute.CheckItemUniqueID] = checkItem.CheckItemUniqueID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else
                    {
                        var checkTypeList = (from c in edb.CheckItem
                                             join x in edb.EquipmentCheckItem
                                             on c.UniqueID equals x.CheckItemUniqueID into tmpE
                                             from x in tmpE.DefaultIfEmpty()
                                             join y in edb.ControlPointCheckItem
                                             on c.UniqueID equals y.CheckItemUniqueID into tmpC
                                             from y in tmpC.DefaultIfEmpty()
                                             where c.OrganizationUniqueID == OrganizationUniqueID && !c.IsFeelItem
                                             select new
                                             {
                                                 CheckType = c.CheckType,
                                                 Equipment = x != null ? x.EquipmentUniqueID : "",
                                                 ControlPoint = y != null ? y.ControlPointUniqueID : ""
                                             }).Where(x => !string.IsNullOrEmpty(x.Equipment) || !string.IsNullOrEmpty(x.ControlPoint)).Select(x => x.CheckType).Distinct().OrderBy(x => x).ToList();

                        foreach (var checkType in checkTypeList)
                        {
                            var treeItem = new TreeItem() { Title = checkType };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.CheckType.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = checkType;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.CheckType] = checkType;
                            attributes[Define.EnumTreeAttribute.CheckItemUniqueID] = string.Empty;
                     
                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItem.State = "closed";

                            treeItemList.Add(treeItem);
                        }

                        var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                        foreach (var organization in organizationList)
                        {
                            var downStream = OrganizationDataAccessor.GetDownStreamOrganizationList(OrganizationList, organization.UniqueID, true);

                            var query = (from c in edb.CheckItem
                                         join x in edb.EquipmentCheckItem
                                         on c.UniqueID equals x.CheckItemUniqueID into tmpE
                                         from x in tmpE.DefaultIfEmpty()
                                         join y in edb.ControlPointCheckItem
                                         on c.UniqueID equals y.CheckItemUniqueID into tmpC
                                         from y in tmpC.DefaultIfEmpty()
                                         where downStream.Contains(c.OrganizationUniqueID) && !c.IsFeelItem
                                         select new
                                         {
                                             c.UniqueID,
                                             Equipment = x != null ? x.EquipmentUniqueID : "",
                                             ControlPoint = y != null ? y.ControlPointUniqueID : ""
                                         }).Where(x => !string.IsNullOrEmpty(x.Equipment) || !string.IsNullOrEmpty(x.ControlPoint)).Select(x => x.UniqueID).Distinct().OrderBy(x => x).ToList();

                            if (query.Count > 0)
                            {
                                var treeItem = new TreeItem() { Title = organization.Description };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                                attributes[Define.EnumTreeAttribute.CheckType] = string.Empty;
                                attributes[Define.EnumTreeAttribute.CheckItemUniqueID] = string.Empty;

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
