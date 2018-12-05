using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Models;
using DbEntity.MSSQL.EquipmentMaintenance;
using System.Reflection;
using Utility;
using Models.Shared;
using DataAccess;
using Customized.CHIMEI.Models.TrendQuery;

namespace Customized.CHIMEI.DataAccess
{
    public class TrendQueryHelper
    {
        public static RequestResult Query(QueryParameters Parameters)
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

                var itemList = new List<ChartViewModel>();

                using (EDbEntities db = new EDbEntities())
                {
                    if (!string.IsNullOrEmpty(Parameters.EquipmentUniqueID))
                    {
                        var query = (from x in db.View_EquipmentCheckItem
                                     join e in db.Equipment
                                     on x.EquipmentUniqueID equals e.UniqueID
                                     join p in db.EquipmentPart
                                     on x.PartUniqueID equals p.UniqueID into tmpPart
                                     from p in tmpPart.DefaultIfEmpty()
                                     join c in db.CheckItem
                                     on x.CheckItemUniqueID equals c.UniqueID
                                     where x.EquipmentUniqueID == Parameters.EquipmentUniqueID && x.PartUniqueID == Parameters.PartUniqueID
                                     select new
                                     {
                                         EquipmentName = e.Name,
                                         PartDescription = p != null ? p.Description : "",
                                         c.CheckType,
                                         CheckItemUniqueID = c.UniqueID,
                                         CheckItemID = c.ID,
                                         CheckItemDescription = x.Description,
                                         LowerLimit = x.LowerLimit,
                                         LowerAlertLimit = x.LowerAlertLimit,
                                         UpperAlertLimit = x.UpperAlertLimit,
                                         UpperLimit = x.UpperLimit
                                     }).OrderBy(x => x.CheckItemID).ToList();

                        var checkTypeList = query.Where(x => x.CheckType == "馬達-振動" || x.CheckType == "馬達-溫度").Select(x => x.CheckType).Distinct().OrderBy(x => x).ToList();

                        foreach (var checkType in checkTypeList)
                        {
                            var checkItemList = query.Where(x => x.CheckType == checkType).ToList();

                            if (checkItemList.Count > 0)
                            {
                                var c = checkItemList.First();

                                var chartModel = new ChartViewModel()
                                {
                                    CheckType = checkType,
                                    ControlPointDescription = string.Empty,
                                    EquipmentName = c.EquipmentName,
                                    PartDescription = c.PartDescription,
                                    LowerLimit = checkItemList.Where(x => x.LowerLimit.HasValue).Select(x => x.LowerLimit.Value).Distinct().ToList(),
                                    LowerAlertLimit = checkItemList.Where(x => x.LowerAlertLimit.HasValue).Select(x => x.LowerAlertLimit.Value).Distinct().ToList(),
                                    UpperAlertLimit = checkItemList.Where(x => x.UpperAlertLimit.HasValue).Select(x => x.UpperAlertLimit.Value).Distinct().ToList(),
                                    UpperLimit = checkItemList.Where(x => x.UpperLimit.HasValue).Select(x => x.UpperLimit.Value).Distinct().ToList()
                                };

                                foreach (var checkItem in checkItemList)
                                {
                                    var checkResultList = db.CheckResult.Where(x => x.EquipmentUniqueID == Parameters.EquipmentUniqueID && x.PartUniqueID == Parameters.PartUniqueID && x.CheckItemUniqueID == checkItem.CheckItemUniqueID && (x.NetValue.HasValue || x.Value.HasValue) && string.Compare(x.CheckDate, Parameters.BeginDate) >= 0 && string.Compare(x.CheckDate, Parameters.EndDate) <= 0).Select(x => new CheckResultModel
                                    {
                                        CheckDate = x.CheckDate,
                                        CheckTime = x.CheckTime,
                                        NetValue = x.NetValue,
                                        Value = x.Value
                                    }).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

                                    if (checkResultList.Count > 1)
                                    {
                                        chartModel.CheckItemList.Add(new CheckItemModel()
                                        {
                                            CheckItemDescription = checkItem.CheckItemDescription,
                                            LowerLimit = checkItem.LowerLimit,
                                            LowerAlertLimit = checkItem.LowerAlertLimit,
                                            UpperAlertLimit = checkItem.UpperAlertLimit,
                                            UpperLimit = checkItem.UpperLimit,
                                            CheckResultList = checkResultList,
                                            Color = colorList[colorSeq]
                                        });

                                        colorSeq++;
                                    }
                                }

                                if (chartModel.CheckItemList.Count > 0)
                                {
                                    itemList.Add(chartModel);
                                }
                            }
                        }

                        //var checkItemList = query.Where(x => x.CheckType == "馬達-振動" || x.CheckType == "馬達-溫度").ToList();

                        //if (checkItemList.Count > 0)
                        //{
                        //    var c = checkItemList.First();

                        //    var chartModel = new ChartViewModel()
                        //    {
                        //        CheckType = "",
                        //        ControlPointDescription = string.Empty,
                        //        EquipmentName = c.EquipmentName,
                        //        PartDescription = c.PartDescription,
                        //        LowerLimit = checkItemList.Where(x => x.LowerLimit.HasValue).Select(x => x.LowerLimit.Value).Distinct().ToList(),
                        //        LowerAlertLimit = checkItemList.Where(x => x.LowerAlertLimit.HasValue).Select(x => x.LowerAlertLimit.Value).Distinct().ToList(),
                        //        UpperAlertLimit = checkItemList.Where(x => x.UpperAlertLimit.HasValue).Select(x => x.UpperAlertLimit.Value).Distinct().ToList(),
                        //        UpperLimit = checkItemList.Where(x => x.UpperLimit.HasValue).Select(x => x.UpperLimit.Value).Distinct().ToList()
                        //    };

                        //    foreach (var checkItem in checkItemList)
                        //    {
                        //        var checkResultList = db.CheckResult.Where(x => x.EquipmentUniqueID == Parameters.EquipmentUniqueID && x.PartUniqueID == Parameters.PartUniqueID && x.CheckItemUniqueID == checkItem.CheckItemUniqueID && (x.NetValue.HasValue || x.Value.HasValue) && string.Compare(x.CheckDate, Parameters.BeginDate) >= 0 && string.Compare(x.CheckDate, Parameters.EndDate) <= 0).Select(x => new CheckResultModel
                        //        {
                        //            CheckDate = x.CheckDate,
                        //            CheckTime = x.CheckTime,
                        //            NetValue = x.NetValue,
                        //            Value = x.Value
                        //        }).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

                        //        if (checkResultList.Count > 1)
                        //        {
                        //            chartModel.CheckItemList.Add(new CheckItemModel()
                        //            {
                        //                CheckItemDescription = checkItem.CheckItemDescription,
                        //                LowerLimit = checkItem.LowerLimit,
                        //                LowerAlertLimit = checkItem.LowerAlertLimit,
                        //                UpperAlertLimit = checkItem.UpperAlertLimit,
                        //                UpperLimit = checkItem.UpperLimit,
                        //                CheckResultList = checkResultList,
                        //                Color = colorList[colorSeq]
                        //            });

                        //            colorSeq++;
                        //        }
                        //    }

                        //    if (chartModel.CheckItemList.Count > 0)
                        //    {
                        //        itemList.Add(chartModel);
                        //    }
                        //}
                    }
                    else
                    {
                        var query = (from x in db.View_ControlPointCheckItem
                                      join p in db.ControlPoint
                                      on x.ControlPointUniqueID equals p.UniqueID
                                      join c in db.CheckItem
                                      on x.CheckItemUniqueID equals c.UniqueID
                                      where x.ControlPointUniqueID == Parameters.ControlPointUniqueID
                                      select new
                                      {
                                          ControlPointDescription = p.Description,
                                          c.CheckType,
                                          CheckItemUniqueID = c.UniqueID,
                                          CheckItemID = c.ID,
                                          CheckItemDescription = c.Description,
                                          LowerLimit = x.LowerLimit,
                                          LowerAlertLimit = x.LowerAlertLimit,
                                          UpperAlertLimit = x.UpperAlertLimit,
                                          UpperLimit = x.UpperLimit
                                      }).ToList();

                        var checkTypeList = query.Where(x => x.CheckType == "馬達-振動" || x.CheckType == "馬達-溫度").Select(x => x.CheckType).Distinct().OrderBy(x => x).ToList();

                        foreach (var checkType in checkTypeList)
                        {
                            var checkItemList = query.Where(x => x.CheckType == checkType).ToList();

                            if (checkItemList.Count > 0)
                            {
                                var c = checkItemList.First();

                                var chartModel = new ChartViewModel()
                                {
                                    CheckType = checkType,
                                    ControlPointDescription = c.ControlPointDescription,
                                    EquipmentName = string.Empty,
                                    PartDescription = string.Empty,
                                    LowerLimit = checkItemList.Where(x => x.LowerLimit.HasValue).Select(x => x.LowerLimit.Value).Distinct().ToList(),
                                    LowerAlertLimit = checkItemList.Where(x => x.LowerAlertLimit.HasValue).Select(x => x.LowerAlertLimit.Value).Distinct().ToList(),
                                    UpperAlertLimit = checkItemList.Where(x => x.UpperAlertLimit.HasValue).Select(x => x.UpperAlertLimit.Value).Distinct().ToList(),
                                    UpperLimit = checkItemList.Where(x => x.UpperLimit.HasValue).Select(x => x.UpperLimit.Value).Distinct().ToList()
                                };

                                foreach (var checkItem in checkItemList)
                                {
                                    var checkResultList = db.CheckResult.Where(x => x.ControlPointUniqueID == Parameters.ControlPointUniqueID && string.IsNullOrEmpty(x.EquipmentUniqueID) && x.CheckItemUniqueID == checkItem.CheckItemUniqueID && (x.NetValue.HasValue || x.Value.HasValue) && string.Compare(x.CheckDate, Parameters.BeginDate) >= 0 && string.Compare(x.CheckDate, Parameters.EndDate) <= 0).Select(x => new CheckResultModel
                                    {
                                        CheckDate = x.CheckDate,
                                        CheckTime = x.CheckTime,
                                        NetValue = x.NetValue,
                                        Value = x.Value
                                    }).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

                                    if (checkResultList.Count > 1)
                                    {
                                        chartModel.CheckItemList.Add(new CheckItemModel()
                                        {
                                            CheckItemDescription = checkItem.CheckItemDescription,
                                            LowerLimit = checkItem.LowerLimit,
                                            LowerAlertLimit = checkItem.LowerAlertLimit,
                                            UpperAlertLimit = checkItem.UpperAlertLimit,
                                            UpperLimit = checkItem.UpperLimit,
                                            CheckResultList = checkResultList,
                                            Color = colorList[colorSeq]
                                        });

                                        colorSeq++;
                                    }
                                }

                                if (chartModel.CheckItemList.Count > 0)
                                {
                                    itemList.Add(chartModel);
                                }
                            }
                        }
                    }
                }

                if (itemList.Count > 0)
                {
                    result.ReturnData(itemList);
                }
                else
                {
                    result.ReturnFailedMessage("抄表資料不足，無法繪製趨勢圖");
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

        public static RequestResult GetTreeItem(List<Organization> OrganizationList, string OrganizationUniqueID)
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
                    { Define.EnumTreeAttribute.ControlPointUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.EquipmentUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.PartUniqueID, string.Empty }
                };

                using (EDbEntities edb = new EDbEntities())
                {
                    var controlPointList = (from x in edb.ControlPointCheckItem
                                            join p in edb.ControlPoint
                                            on x.ControlPointUniqueID equals p.UniqueID
                                            join c in edb.CheckItem
                                            on x.CheckItemUniqueID equals c.UniqueID
                                            where !c.IsFeelItem && p.OrganizationUniqueID == OrganizationUniqueID
                                            select p).Distinct().OrderBy(x => x.ID).ToList();

                    foreach (var controlPoint in controlPointList)
                    {
                        var treeItem = new TreeItem() { Title = controlPoint.Description };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.ControlPoint.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", controlPoint.ID, controlPoint.Description);
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                        attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = controlPoint.UniqueID;
                        attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = string.Empty;
                        attributes[Define.EnumTreeAttribute.PartUniqueID] = string.Empty;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        treeItemList.Add(treeItem);
                    }

                    var equipmentList = (from x in edb.EquipmentCheckItem
                                         join e in edb.Equipment
                                         on x.EquipmentUniqueID equals e.UniqueID
                                         join p in edb.EquipmentPart
                                         on e.UniqueID equals p.EquipmentUniqueID into tmpPart
                                         from p in tmpPart.DefaultIfEmpty()
                                         join c in edb.CheckItem
                                         on x.CheckItemUniqueID equals c.UniqueID
                                         where !c.IsFeelItem && e.OrganizationUniqueID == OrganizationUniqueID
                                         select new
                                         {
                                             EquipmentUniqueID = x.EquipmentUniqueID,
                                             PartUniqueID = x.PartUniqueID,
                                             EquipmentID = e.ID,
                                             EquipmentName = e.Name,
                                             PartDescription = p != null ? p.Description : ""
                                         }).Distinct().OrderBy(x => x.EquipmentID).ThenBy(x => x.PartDescription).ToList();

                    foreach (var equipment in equipmentList)
                    {
                        if (equipment.PartUniqueID == "*")
                        {
                            var treeItem = new TreeItem() { Title = equipment.EquipmentName };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Equipment.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", equipment.EquipmentID, equipment.EquipmentName);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = equipment.EquipmentUniqueID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = equipment.PartUniqueID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }
                        else
                        {
                            var treeItem = new TreeItem() { Title = string.Format("{0}-{1}", equipment.EquipmentName, equipment.PartDescription) };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.EquipmentPart.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", equipment.EquipmentID, string.Format("{0}-{1}", equipment.EquipmentName, equipment.PartDescription));
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = equipment.EquipmentUniqueID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = equipment.PartUniqueID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }
                    }

                    var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                    foreach (var organization in organizationList)
                    {
                        var downStream = OrganizationDataAccessor.GetDownStreamOrganizationList(OrganizationList, organization.UniqueID, true);

                        var query1 = (from x in edb.ControlPointCheckItem
                                      join p in edb.ControlPoint
                                      on x.ControlPointUniqueID equals p.UniqueID
                                      join c in edb.CheckItem
                                      on x.CheckItemUniqueID equals c.UniqueID
                                      where !c.IsFeelItem && downStream.Contains(p.OrganizationUniqueID)
                                      select p).ToList();

                        var query2 = (from x in edb.EquipmentCheckItem
                                      join e in edb.Equipment
                                      on x.EquipmentUniqueID equals e.UniqueID
                                      join p in edb.EquipmentPart
                                      on e.UniqueID equals p.EquipmentUniqueID into tmpPart
                                      from p in tmpPart.DefaultIfEmpty()
                                      join c in edb.CheckItem
                                      on x.CheckItemUniqueID equals c.UniqueID
                                      where !c.IsFeelItem && downStream.Contains(e.OrganizationUniqueID)
                                      select e).ToList();

                        if (query1.Count > 0 || query2.Count > 0)
                        {
                            var treeItem = new TreeItem() { Title = organization.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                            attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItem.State = "closed";

                            treeItemList.Add(treeItem);
                        }
                    }
                }

                result.ReturnData(treeItemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodInfo.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }
    }
}
