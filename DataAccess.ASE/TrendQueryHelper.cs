using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.ASE;
using Models.Shared;
using Models.Authenticated;
using Models.EquipmentMaintenance.TrendQuery;

namespace DataAccess.ASE
{
    public class TrendQueryHelper
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = new List<ChartViewModel>();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (!string.IsNullOrEmpty(Parameters.EquipmentUniqueID))
                    {
                        var query = (from x in db.EQUIPMENTCHECKITEM
                                     join e in db.EQUIPMENT
                                     on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                     join p in db.EQUIPMENTPART
                                     on x.PARTUNIQUEID equals p.UNIQUEID into tmpPart
                                     from p in tmpPart.DefaultIfEmpty()
                                     join c in db.CHECKITEM
                                     on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                     where x.EQUIPMENTUNIQUEID == Parameters.EquipmentUniqueID && x.PARTUNIQUEID == Parameters.PartUniqueID
                                     select new
                                     {
                                         EquipmentName = e.NAME,
                                         PartDescription = p != null ? p.DESCRIPTION : "",
                                         CheckItemUNIQUEID = c.UNIQUEID,
                                         CheckItemID = c.ID,
                                         CheckItemDescription = c.DESCRIPTION,
                                         LowerLimit = x.ISINHERIT == "Y" ? c.LOWERLIMIT : x.LOWERLIMIT,
                                         LowerAlertLimit = x.ISINHERIT == "Y" ? c.LOWERALERTLIMIT : x.LOWERALERTLIMIT,
                                         UpperAlertLimit = x.ISINHERIT == "Y" ? c.UPPERALERTLIMIT : x.UPPERALERTLIMIT,
                                         UpperLimit = x.ISINHERIT == "Y" ? c.UPPERLIMIT : x.UPPERLIMIT
                                     }).OrderBy(x => x.CheckItemID).ToList();

                        foreach (var q in query)
                        {
                            var tmp = db.CHECKRESULT.Where(x => x.EQUIPMENTUNIQUEID == Parameters.EquipmentUniqueID && x.PARTUNIQUEID == Parameters.PartUniqueID && x.CHECKITEMUNIQUEID == q.CheckItemUNIQUEID && (x.NETVALUE.HasValue || x.VALUE.HasValue) && !string.IsNullOrEmpty(x.CHECKDATE) && string.Compare(x.CHECKDATE, Parameters.BeginDate) >= 0 && string.Compare(x.CHECKDATE, Parameters.EndDate) <= 0).ToList();

                            var checkResultList = tmp.Select(x => new CheckResultModel
                            {
                                CheckDate = x.CHECKDATE,
                                CheckTime = x.CHECKTIME,
                                NetValue= x.NETVALUE.HasValue?Convert.ToDouble(x.NETVALUE):default(double?),
                                Value = x.VALUE.HasValue ? Convert.ToDouble(x.VALUE) : default(double?)
                            }).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

                            if (checkResultList.Count > 1)
                            {
                                itemList.Add(new ChartViewModel()
                                {
                                    ControlPointDescription = string.Empty,
                                    EquipmentName = q.EquipmentName,
                                    PartDescription = q.PartDescription,
                                    CheckItemUniqueID = q.CheckItemUNIQUEID,
                                    CheckItemDescription = q.CheckItemDescription,
                                    LowerLimit = q.LowerLimit.HasValue?Convert.ToDouble(q.LowerLimit.Value):default(double?),
                                    LowerAlertLimit = q.LowerAlertLimit.HasValue ? Convert.ToDouble(q.LowerAlertLimit.Value) : default(double?),
                                    UpperAlertLimit = q.UpperAlertLimit.HasValue ? Convert.ToDouble(q.UpperAlertLimit.Value) : default(double?),
                                    UpperLimit = q.UpperLimit.HasValue ? Convert.ToDouble(q.UpperLimit.Value) : default(double?),
                                    CheckResultList = checkResultList
                                });
                            }
                        }
                    }
                    else
                    {
                        var query = (from x in db.CONTROLPOINTCHECKITEM
                                     join p in db.CONTROLPOINT
                                     on x.CONTROLPOINTUNIQUEID equals p.UNIQUEID
                                     join c in db.CHECKITEM
                                     on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                     where x.CONTROLPOINTUNIQUEID == Parameters.ControlPointUniqueID
                                     select new
                                     {
                                         ControlPointDescription = p.DESCRIPTION,
                                         CheckItemUNIQUEID = c.UNIQUEID,
                                         CheckItemID = c.ID,
                                         CheckItemDescription = c.DESCRIPTION,
                                         LowerLimit = x.ISINHERIT == "Y" ? c.LOWERLIMIT : x.LOWERLIMIT,
                                         LowerAlertLimit = x.ISINHERIT == "Y" ? c.LOWERALERTLIMIT : x.LOWERALERTLIMIT,
                                         UpperAlertLimit = x.ISINHERIT == "Y" ? c.UPPERALERTLIMIT : x.UPPERALERTLIMIT,
                                         UpperLimit = x.ISINHERIT == "Y" ? c.UPPERLIMIT : x.UPPERLIMIT
                                     }).ToList();

                        foreach (var q in query)
                        {
                            var tmp = db.CHECKRESULT.Where(x => x.CONTROLPOINTUNIQUEID == Parameters.ControlPointUniqueID && string.IsNullOrEmpty(x.EQUIPMENTUNIQUEID) && x.CHECKITEMUNIQUEID == q.CheckItemUNIQUEID && (x.NETVALUE.HasValue || x.VALUE.HasValue) && !string.IsNullOrEmpty(x.CHECKDATE) && string.Compare(x.CHECKDATE, Parameters.BeginDate) >= 0 && string.Compare(x.CHECKDATE, Parameters.EndDate) <= 0).ToList();

                            var checkResultList = tmp.Select(x => new CheckResultModel
                            {
                                CheckDate = x.CHECKDATE,
                                CheckTime = x.CHECKTIME,
                                NetValue = x.NETVALUE.HasValue ? Convert.ToDouble(x.NETVALUE) : default(double?),
                                Value = x.VALUE.HasValue ? Convert.ToDouble(x.VALUE) : default(double?)
                            }).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

                            if (checkResultList.Count > 0)
                            {
                                itemList.Add(new ChartViewModel()
                                {
                                    ControlPointDescription = q.ControlPointDescription,
                                    EquipmentName = string.Empty,
                                    PartDescription = string.Empty,
                                    CheckItemUniqueID = q.CheckItemUNIQUEID,
                                    CheckItemDescription = q.CheckItemDescription,
                                    LowerLimit = q.LowerLimit.HasValue ? Convert.ToDouble(q.LowerLimit.Value) : default(double?),
                                    LowerAlertLimit = q.LowerAlertLimit.HasValue ? Convert.ToDouble(q.LowerAlertLimit.Value) : default(double?),
                                    UpperAlertLimit = q.UpperAlertLimit.HasValue ? Convert.ToDouble(q.UpperAlertLimit.Value) : default(double?),
                                    UpperLimit = q.UpperLimit.HasValue ? Convert.ToDouble(q.UpperLimit.Value) : default(double?),
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

                using (ASEDbEntities edb = new ASEDbEntities())
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

                    var query1 = (from x in edb.CONTROLPOINTCHECKITEM
                                  join p in edb.CONTROLPOINT
                                  on x.CONTROLPOINTUNIQUEID equals p.UNIQUEID
                                  join c in edb.CHECKITEM
                                  on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                  where c.ISFEELITEM == "N" && downStream.Contains(p.ORGANIZATIONUNIQUEID) && Account.QueryableOrganizationUniqueIDList.Contains(p.ORGANIZATIONUNIQUEID)
                                  select p).ToList();

                    var query2 = (from x in edb.EQUIPMENTCHECKITEM
                                  join e in edb.EQUIPMENT
                                  on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                  join p in edb.EQUIPMENTPART
                                  on e.UNIQUEID equals p.EQUIPMENTUNIQUEID into tmpPart
                                  from p in tmpPart.DefaultIfEmpty()
                                  join c in edb.CHECKITEM
                                  on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                  where c.ISFEELITEM == "N" && downStream.Contains(e.ORGANIZATIONUNIQUEID) && Account.QueryableOrganizationUniqueIDList.Contains(e.ORGANIZATIONUNIQUEID)
                                  select e).ToList();

                    if (query1.Count > 0 || query2.Count > 0)
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

                using (ASEDbEntities edb = new ASEDbEntities())
                {
                    if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                    {
                        var controlPointList = (from x in edb.CONTROLPOINTCHECKITEM
                                                join p in edb.CONTROLPOINT
                                                on x.CONTROLPOINTUNIQUEID equals p.UNIQUEID
                                                join c in edb.CHECKITEM
                                                on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                                where c.ISFEELITEM=="N" && p.ORGANIZATIONUNIQUEID == OrganizationUniqueID
                                                select p).Distinct().OrderBy(x => x.ID).ToList();

                        foreach (var controlPoint in controlPointList)
                        {
                            var treeItem = new TreeItem() { Title = controlPoint.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.ControlPoint.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", controlPoint.ID, controlPoint.DESCRIPTION);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = controlPoint.UNIQUEID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }

                        var equipmentList = (from x in edb.EQUIPMENTCHECKITEM
                                             join e in edb.EQUIPMENT
                                             on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                             join p in edb.EQUIPMENTPART
                                             on e.UNIQUEID equals p.EQUIPMENTUNIQUEID into tmpPart
                                             from p in tmpPart.DefaultIfEmpty()
                                             join c in edb.CHECKITEM
                                             on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                             where c.ISFEELITEM=="N" && e.ORGANIZATIONUNIQUEID == OrganizationUniqueID
                                             select new
                                             {
                                                 EquipmentUniqueID = x.EQUIPMENTUNIQUEID,
                                                 PartUniqueID = x.PARTUNIQUEID,
                                                 EquipmentID = e.ID,
                                                 EquipmentName = e.NAME,
                                                 PartDescription = p != null ? p.DESCRIPTION : ""
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
                    }

                    var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                    foreach (var organization in organizationList)
                    {
                        var downStream = OrganizationDataAccessor.GetDownStreamOrganizationList(OrganizationList, organization.UniqueID, true);

                        var query1 = (from x in edb.CONTROLPOINTCHECKITEM
                                      join p in edb.CONTROLPOINT
                                      on x.CONTROLPOINTUNIQUEID equals p.UNIQUEID
                                      join c in edb.CHECKITEM
                                      on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                      where c.ISFEELITEM == "N" && downStream.Contains(p.ORGANIZATIONUNIQUEID) && Account.QueryableOrganizationUniqueIDList.Contains(p.ORGANIZATIONUNIQUEID)
                                      select p).ToList();

                        var query2 = (from x in edb.EQUIPMENTCHECKITEM
                                      join e in edb.EQUIPMENT
                                      on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                      join p in edb.EQUIPMENTPART
                                      on e.UNIQUEID equals p.EQUIPMENTUNIQUEID into tmpPart
                                      from p in tmpPart.DefaultIfEmpty()
                                      join c in edb.CHECKITEM
                                      on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                      where c.ISFEELITEM == "N" && downStream.Contains(e.ORGANIZATIONUNIQUEID) && Account.QueryableOrganizationUniqueIDList.Contains(e.ORGANIZATIONUNIQUEID)
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
