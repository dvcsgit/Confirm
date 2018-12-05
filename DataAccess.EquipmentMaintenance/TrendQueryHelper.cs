using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.Shared;
using Models.Authenticated;
using Models.EquipmentMaintenance.TrendQuery;
using DbEntity.MSSQL;

namespace DataAccess.EquipmentMaintenance
{
    public class TrendQueryHelper
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = new List<ChartViewModel>();

                using (EDbEntities db = new EDbEntities())
                {
                    if (!string.IsNullOrEmpty(Parameters.EquipmentUniqueID))
                    {
                        var query1 = (from x in db.View_EquipmentCheckItem
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
                                         CheckItemUniqueID = c.UniqueID,
                                         CheckItemID = c.ID,
                                         CheckItemDescription = x.Description,
                                         LowerLimit = x.LowerLimit,
                                         LowerAlertLimit = x.LowerAlertLimit,
                                         UpperAlertLimit = x.UpperAlertLimit,
                                         UpperLimit = x.UpperLimit
                                     }).OrderBy(x => x.CheckItemID).ToList();

                        var query2 = (from x in db.EquipmentStandard
                                      join e in db.Equipment
                                      on x.EquipmentUniqueID equals e.UniqueID
                                      join p in db.EquipmentPart
                                      on x.PartUniqueID equals p.UniqueID into tmpPart
                                      from p in tmpPart.DefaultIfEmpty()
                                      join s in db.Standard
                                      on x.StandardUniqueID equals s.UniqueID
                                      where x.EquipmentUniqueID == Parameters.EquipmentUniqueID && x.PartUniqueID == Parameters.PartUniqueID
                                      select new
                                      {
                                          EquipmentName = e.Name,
                                          PartDescription = p != null ? p.Description : "",
                                          StandardUniqueID = s.UniqueID,
                                          StandardID = s.ID,
                                          StandardDescription = s.Description,
                                          LowerLimit =x.IsInherit?s.LowerLimit: x.LowerLimit,
                                          LowerAlertLimit =x.IsInherit?s.LowerAlertLimit: x.LowerAlertLimit,
                                          UpperAlertLimit =x.IsInherit?s.UpperAlertLimit: x.UpperAlertLimit,
                                          UpperLimit =x.IsInherit?s.UpperLimit: x.UpperLimit
                                      }).OrderBy(x => x.StandardID).ToList();

                        foreach (var q in query1)
                        {
                            var checkResultList = db.CheckResult.Where(x => x.EquipmentUniqueID == Parameters.EquipmentUniqueID && x.PartUniqueID == Parameters.PartUniqueID && x.CheckItemUniqueID == q.CheckItemUniqueID && (x.NetValue.HasValue || x.Value.HasValue) && string.Compare(x.CheckDate, Parameters.BeginDate) >= 0 && string.Compare(x.CheckDate, Parameters.EndDate) <= 0).Select(x => new CheckResultModel
                            {
                                CheckDate = x.CheckDate,
                                CheckTime = x.CheckTime,
                                NetValue = x.NetValue,
                                Value = x.Value
                            }).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

                            if (checkResultList.Count > 1)
                            {
                                itemList.Add(new ChartViewModel()
                                {
                                    Type="P",
                                    ControlPointDescription = string.Empty,
                                    EquipmentName = q.EquipmentName,
                                    PartDescription = q.PartDescription,
                                    CheckItemUniqueID = q.CheckItemUniqueID,
                                    CheckItemDescription = q.CheckItemDescription,
                                    LowerLimit = q.LowerLimit,
                                    LowerAlertLimit = q.LowerAlertLimit,
                                    UpperAlertLimit = q.UpperAlertLimit,
                                    UpperLimit = q.UpperLimit,
                                    CheckResultList = checkResultList
                                });
                            }
                        }

                        foreach (var q in query2)
                        {
                            var standardResultList = (from x in db.MFormResult
                                                      join r in db.MFormStandardResult
                                                      on x.UniqueID equals r.ResultUniqueID
                                                      where r.EquipmentUniqueID == Parameters.EquipmentUniqueID && r.PartUniqueID == Parameters.PartUniqueID && r.StandardUniqueID == q.StandardUniqueID && (r.NetValue.HasValue || r.Value.HasValue) && string.Compare(x.PMDate, Parameters.BeginDate) >= 0 && string.Compare(x.PMDate, Parameters.EndDate) <= 0
                                                      select new CheckResultModel {
                                                      CheckDate = x.PMDate,
                                                      CheckTime=x.PMTime,
                                                      NetValue = r.NetValue,
                                                      Value=r.Value
                                                      }).ToList();

                            if (standardResultList.Count > 1)
                            {
                                itemList.Add(new ChartViewModel()
                                {
                                    Type="M",
                                    ControlPointDescription = string.Empty,
                                    EquipmentName = q.EquipmentName,
                                    PartDescription = q.PartDescription,
                                    CheckItemUniqueID = q.StandardUniqueID,
                                    CheckItemDescription = q.StandardDescription,
                                    LowerLimit = q.LowerLimit,
                                    LowerAlertLimit = q.LowerAlertLimit,
                                    UpperAlertLimit = q.UpperAlertLimit,
                                    UpperLimit = q.UpperLimit,
                                    CheckResultList = standardResultList
                                });
                            }
                        }
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
                                         CheckItemUniqueID = c.UniqueID,
                                         CheckItemID = c.ID,
                                         CheckItemDescription = c.Description,
                                         LowerLimit = x.LowerLimit,
                                         LowerAlertLimit = x.LowerAlertLimit,
                                         UpperAlertLimit = x.UpperAlertLimit,
                                         UpperLimit = x.UpperLimit
                                     }).ToList();

                        foreach (var q in query)
                        {
                            var checkResultList = db.CheckResult.Where(x => x.ControlPointUniqueID == Parameters.ControlPointUniqueID && string.IsNullOrEmpty(x.EquipmentUniqueID) && x.CheckItemUniqueID == q.CheckItemUniqueID && (x.NetValue.HasValue || x.Value.HasValue) && string.Compare(x.CheckDate, Parameters.BeginDate) >= 0 && string.Compare(x.CheckDate, Parameters.EndDate) <= 0).Select(x => new CheckResultModel
                            {
                                CheckDate = x.CheckDate,
                                CheckTime = x.CheckTime,
                                NetValue = x.NetValue,
                                Value = x.Value
                            }).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

                            if (checkResultList.Count > 0)
                            {
                                itemList.Add(new ChartViewModel()
                                {
                                    Type="P",
                                    ControlPointDescription = q.ControlPointDescription,
                                    EquipmentName = string.Empty,
                                    PartDescription = string.Empty,
                                    CheckItemUniqueID = q.CheckItemUniqueID,
                                    CheckItemDescription = q.CheckItemDescription,
                                    LowerLimit = q.LowerLimit,
                                    LowerAlertLimit = q.LowerAlertLimit,
                                    UpperAlertLimit = q.UpperAlertLimit,
                                    UpperLimit = q.UpperLimit,
                                    CheckResultList = checkResultList
                                });
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
                    { Define.EnumTreeAttribute.ControlPointUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.EquipmentUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.PartUniqueID, string.Empty }
                };

                using (EDbEntities edb = new EDbEntities())
                {
                    var organization = OrganizationList.First(x => x.UniqueID == OrganizationUniqueID);

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

                    var downStream = OrganizationDataAccessor.GetDownStreamOrganizationList(OrganizationList, organization.UniqueID, true);

                    var query1 = (from x in edb.ControlPointCheckItem
                                  join p in edb.ControlPoint
                                  on x.ControlPointUniqueID equals p.UniqueID
                                  join c in edb.CheckItem
                                  on x.CheckItemUniqueID equals c.UniqueID
                                  where !c.IsFeelItem && c.TextValueType.HasValue&&c.TextValueType.Value==1 && downStream.Contains(p.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(p.OrganizationUniqueID)
                                  select p).ToList();

                    var query2 = (from x in edb.EquipmentCheckItem
                                  join e in edb.Equipment
                                  on x.EquipmentUniqueID equals e.UniqueID
                                  join p in edb.EquipmentPart
                                  on e.UniqueID equals p.EquipmentUniqueID into tmpPart
                                  from p in tmpPart.DefaultIfEmpty()
                                  join c in edb.CheckItem
                                  on x.CheckItemUniqueID equals c.UniqueID
                                  where !c.IsFeelItem && c.TextValueType.HasValue && c.TextValueType.Value == 1 && downStream.Contains(e.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(e.OrganizationUniqueID)
                                  select e).ToList();

                    var query3 = (from x in edb.EquipmentStandard
                                  join e in edb.Equipment
                                  on x.EquipmentUniqueID equals e.UniqueID
                                  join p in edb.EquipmentPart
                                  on e.UniqueID equals p.EquipmentUniqueID into tmpPart
                                  from p in tmpPart.DefaultIfEmpty()
                                  join s in edb.Standard
                                  on x.StandardUniqueID equals s.UniqueID
                                  where !s.IsFeelItem && downStream.Contains(e.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(e.OrganizationUniqueID)
                                  select e).ToList();

                    if (query1.Count > 0 || query2.Count > 0 || query3.Count > 0)
                    {
                        treeItem.State = "closed";
                    }

                    treeItemList.Add(treeItem);
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
                    { Define.EnumTreeAttribute.ControlPointUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.EquipmentUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.PartUniqueID, string.Empty }
                };

                using (EDbEntities edb = new EDbEntities())
                {
                    if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                    {
                        var controlPointList = (from x in edb.ControlPointCheckItem
                                                join p in edb.ControlPoint
                                                on x.ControlPointUniqueID equals p.UniqueID
                                                join c in edb.CheckItem
                                                on x.CheckItemUniqueID equals c.UniqueID
                                                where !c.IsFeelItem && c.TextValueType.HasValue && c.TextValueType.Value == 1 && p.OrganizationUniqueID == OrganizationUniqueID
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

                        var equipmentList1 = (from x in edb.EquipmentCheckItem
                                             join e in edb.Equipment
                                             on x.EquipmentUniqueID equals e.UniqueID
                                             join p in edb.EquipmentPart
                                             on e.UniqueID equals p.EquipmentUniqueID into tmpPart
                                             from p in tmpPart.DefaultIfEmpty()
                                             join c in edb.CheckItem
                                             on x.CheckItemUniqueID equals c.UniqueID
                                              where !c.IsFeelItem && c.TextValueType.HasValue && c.TextValueType.Value == 1 && e.OrganizationUniqueID == OrganizationUniqueID
                                             select new
                                             {
                                                 EquipmentUniqueID = x.EquipmentUniqueID,
                                                 PartUniqueID = x.PartUniqueID,
                                                 EquipmentID = e.ID,
                                                 EquipmentName = e.Name,
                                                 PartDescription = p != null ? p.Description : ""
                                             }).Distinct().OrderBy(x => x.EquipmentID).ThenBy(x => x.PartDescription).ToList();

                        var equipmentList2 = (from x in edb.EquipmentStandard
                                              join e in edb.Equipment
                                              on x.EquipmentUniqueID equals e.UniqueID
                                              join p in edb.EquipmentPart
                                              on e.UniqueID equals p.EquipmentUniqueID into tmpPart
                                              from p in tmpPart.DefaultIfEmpty()
                                              join s in edb.Standard
                                              on x.StandardUniqueID equals s.UniqueID
                                              where !s.IsFeelItem && e.OrganizationUniqueID == OrganizationUniqueID
                                              select new
                                              {
                                                  EquipmentUniqueID = x.EquipmentUniqueID,
                                                  PartUniqueID = x.PartUniqueID,
                                                  EquipmentID = e.ID,
                                                  EquipmentName = e.Name,
                                                  PartDescription = p != null ? p.Description : ""
                                              }).Distinct().OrderBy(x => x.EquipmentID).ThenBy(x => x.PartDescription).ToList();

                        var equipmentList = equipmentList1.Union(equipmentList2).Select(x => new {
                            EquipmentUniqueID = x.EquipmentUniqueID,
                            PartUniqueID = x.PartUniqueID,
                            EquipmentID = x.EquipmentID,
                            EquipmentName = x.EquipmentName,
                            PartDescription = x.PartDescription
                        }).Distinct().ToList();

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
                    }
                    
                    var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                    foreach (var organization in organizationList)
                    {
                        var downStream = OrganizationDataAccessor.GetDownStreamOrganizationList(OrganizationList, organization.UniqueID, true);

                        var query1 = (from x in edb.ControlPointCheckItem
                                      join p in edb.ControlPoint
                                      on x.ControlPointUniqueID equals p.UniqueID
                                      join c in edb.CheckItem
                                      on x.CheckItemUniqueID equals c.UniqueID
                                      where !c.IsFeelItem && c.TextValueType.HasValue && c.TextValueType.Value == 1 && downStream.Contains(p.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(p.OrganizationUniqueID)
                                      select p).ToList();

                        var query2 = (from x in edb.EquipmentCheckItem
                                      join e in edb.Equipment
                                      on x.EquipmentUniqueID equals e.UniqueID
                                      join p in edb.EquipmentPart
                                      on e.UniqueID equals p.EquipmentUniqueID into tmpPart
                                      from p in tmpPart.DefaultIfEmpty()
                                      join c in edb.CheckItem
                                      on x.CheckItemUniqueID equals c.UniqueID
                                      where !c.IsFeelItem && c.TextValueType.HasValue && c.TextValueType.Value == 1 && downStream.Contains(e.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(e.OrganizationUniqueID)
                                      select e).ToList();

                        var query3 = (from x in edb.EquipmentStandard
                                      join e in edb.Equipment
                                      on x.EquipmentUniqueID equals e.UniqueID
                                      join p in edb.EquipmentPart
                                      on e.UniqueID equals p.EquipmentUniqueID into tmpPart
                                      from p in tmpPart.DefaultIfEmpty()
                                      join s in edb.Standard
                                      on x.StandardUniqueID equals s.UniqueID
                                      where !s.IsFeelItem && downStream.Contains(e.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(e.OrganizationUniqueID)
                                      select e).ToList();

                        if (query1.Count > 0 || query2.Count > 0 || query3.Count > 0)
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
