using DbEntity.ASE;
using Models.Authenticated;
using Models.EquipmentMaintenance.TrendQuery_CheckItem;
using Models.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace DataAccess.ASE
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var checkItem = db.CHECKITEM.First(x => x.UNIQUEID == CheckItemUniqueID);

                    model.CheckItemDescription = checkItem.DESCRIPTION;

                    var controlPointQuery = (from x in db.CONTROLPOINTCHECKITEM
                                             join p in db.CONTROLPOINT
                                             on x.CONTROLPOINTUNIQUEID equals p.UNIQUEID
                                             join c in db.CHECKITEM
                                             on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                             where x.CHECKITEMUNIQUEID == CheckItemUniqueID
                                             select new
                                             {
                                                 UniqueID = p.UNIQUEID,
                                                 OrganizationUniqueID = p.ORGANIZATIONUNIQUEID,
                                                 p.ID,
                                                 Description = p.DESCRIPTION,
                                                 LowerLimit = x.ISINHERIT == "Y" ? c.LOWERLIMIT : x.LOWERLIMIT,
                                                 LowerAlertLimit = x.ISINHERIT == "Y" ? c.LOWERALERTLIMIT : x.LOWERALERTLIMIT,
                                                 UpperAlertLimit = x.ISINHERIT == "Y" ? c.UPPERALERTLIMIT : x.UPPERALERTLIMIT,
                                                 UpperLimit = x.ISINHERIT == "Y" ? c.UPPERLIMIT : x.UPPERLIMIT
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
                            LowerLimit = controlPoint.LowerLimit.HasValue ? Convert.ToDouble(controlPoint.LowerLimit) : default(double?),
                            LowerAlertLimit = controlPoint.LowerAlertLimit.HasValue ? Convert.ToDouble(controlPoint.LowerAlertLimit) : default(double?),
                            UpperAlertLimit = controlPoint.UpperAlertLimit.HasValue ? Convert.ToDouble(controlPoint.UpperAlertLimit) : default(double?),
                            UpperLimit = controlPoint.UpperLimit.HasValue ? Convert.ToDouble(controlPoint.UpperLimit) : default(double?)
                        });
                    }

                    var equipmentQuery = (from x in db.EQUIPMENTCHECKITEM
                                          join e in db.EQUIPMENT
                                          on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                          join p in db.EQUIPMENTPART
                                          on x.PARTUNIQUEID equals p.UNIQUEID into tmpPart
                                          from p in tmpPart.DefaultIfEmpty()
                                          join c in db.CHECKITEM
                                          on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                          where x.CHECKITEMUNIQUEID == CheckItemUniqueID
                                          select new
                                          {
                                              OrganizationUniqueID = e.ORGANIZATIONUNIQUEID,
                                              UniqueID = e.UNIQUEID,
                                              PartUniqueID = x.PARTUNIQUEID,
                                              e.ID,
                                              Name = e.NAME,
                                              PartDescription = p != null ? p.DESCRIPTION : "",
                                              LowerLimit = x.ISINHERIT == "Y" ? c.LOWERLIMIT : x.LOWERLIMIT,
                                              LowerAlertLimit = x.ISINHERIT == "Y" ? c.LOWERALERTLIMIT : x.LOWERALERTLIMIT,
                                              UpperAlertLimit = x.ISINHERIT == "Y" ? c.UPPERALERTLIMIT : x.UPPERALERTLIMIT,
                                              UpperLimit = x.ISINHERIT == "Y" ? c.UPPERLIMIT : x.UPPERLIMIT
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
                                LowerLimit = equipment.LowerLimit.HasValue ? Convert.ToDouble(equipment.LowerLimit) : default(double?),
                                LowerAlertLimit = equipment.LowerAlertLimit.HasValue ? Convert.ToDouble(equipment.LowerAlertLimit) : default(double?),
                                UpperAlertLimit = equipment.UpperAlertLimit.HasValue ? Convert.ToDouble(equipment.UpperAlertLimit) : default(double?),
                                UpperLimit = equipment.UpperLimit.HasValue ? Convert.ToDouble(equipment.UpperLimit) : default(double?)
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
                                LowerLimit = equipment.LowerLimit.HasValue ? Convert.ToDouble(equipment.LowerLimit) : default(double?),
                                LowerAlertLimit = equipment.LowerAlertLimit.HasValue ? Convert.ToDouble(equipment.LowerAlertLimit) : default(double?),
                                UpperAlertLimit = equipment.UpperAlertLimit.HasValue ? Convert.ToDouble(equipment.UpperAlertLimit) : default(double?),
                                UpperLimit = equipment.UpperLimit.HasValue ? Convert.ToDouble(equipment.UpperLimit) : default(double?)
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var checkItem = db.CHECKITEM.First(x => x.UNIQUEID == Parameters.CheckItemUniqueID);

                    var model = new ChartViewModel()
                    {
                        CheckItemDescription = checkItem.DESCRIPTION
                    };

                    var controlPointList = (from x in db.CONTROLPOINTCHECKITEM
                                            join p in db.CONTROLPOINT
                                            on x.CONTROLPOINTUNIQUEID equals p.UNIQUEID
                                            join c in db.CHECKITEM
                                            on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                            where x.CHECKITEMUNIQUEID == Parameters.CheckItemUniqueID && Parameters.ControlPointList.Contains(p.UNIQUEID)
                                            select new
                                            {
                                               UniqueID= p.UNIQUEID,
                                              Description=  p.DESCRIPTION,
                                               LowerLimit = x.ISINHERIT == "Y" ? c.LOWERLIMIT : x.LOWERLIMIT,
                                               LowerAlertLimit = x.ISINHERIT == "Y" ? c.LOWERALERTLIMIT : x.LOWERALERTLIMIT,
                                               UpperAlertLimit = x.ISINHERIT == "Y" ? c.UPPERALERTLIMIT : x.UPPERALERTLIMIT,
                                               UpperLimit = x.ISINHERIT == "Y" ? c.UPPERLIMIT : x.UPPERLIMIT
                                            }).ToList();

                    foreach (var controlPoint in controlPointList)
                    {
                        var checkResultList = db.CHECKRESULT.Where(x => x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID && string.IsNullOrEmpty(x.EQUIPMENTUNIQUEID) && x.CHECKITEMUNIQUEID == Parameters.CheckItemUniqueID && (x.NETVALUE.HasValue || x.VALUE.HasValue) && string.Compare(x.CHECKDATE, Parameters.BeginDate) >= 0 && string.Compare(x.CHECKDATE, Parameters.EndDate) <= 0).ToList().Select(x => new CheckResultModel
                        {
                            CheckDate = x.CHECKDATE,
                            CheckTime = x.CHECKTIME,
                            NetValue = x.NETVALUE.HasValue ? Convert.ToDouble(x.NETVALUE) : default(double?),
                            Value = x.VALUE.HasValue ? Convert.ToDouble(x.VALUE) : default(double?)
                        }).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

                        if (checkResultList.Count > 1)
                        {
                            model.EquipmentList.Add(new EquipmentModel()
                            {
                                ControlPointDescription = controlPoint.Description,
                                EquipmentName = string.Empty,
                                PartDescription = string.Empty,
                                LowerLimit = controlPoint.LowerLimit.HasValue ? Convert.ToDouble(controlPoint.LowerLimit) : default(double?),
                                LowerAlertLimit = controlPoint.LowerAlertLimit.HasValue ? Convert.ToDouble(controlPoint.LowerAlertLimit) : default(double?),
                                UpperAlertLimit = controlPoint.UpperAlertLimit.HasValue ? Convert.ToDouble(controlPoint.UpperAlertLimit) : default(double?),
                                UpperLimit = controlPoint.UpperLimit.HasValue ? Convert.ToDouble(controlPoint.UpperLimit) : default(double?),
                                CheckResultList = checkResultList,
                                Color = colorList[colorSeq]
                            });

                            colorSeq++;
                        }
                    }

                    var equipmentList = (from x in db.EQUIPMENTCHECKITEM
                                         join e in db.EQUIPMENT
                                         on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                         join c in db.CHECKITEM
                                         on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                         where x.CHECKITEMUNIQUEID == Parameters.CheckItemUniqueID && x.PARTUNIQUEID == "*" && Parameters.EquipmentList.Contains(e.UNIQUEID)
                                         select new
                                         {
                                             UniqueID = e.UNIQUEID,
                                             Name = e.NAME,
                                             LowerLimit = x.ISINHERIT == "Y" ? c.LOWERLIMIT : x.LOWERLIMIT,
                                             LowerAlertLimit = x.ISINHERIT == "Y" ? c.LOWERALERTLIMIT : x.LOWERALERTLIMIT,
                                             UpperAlertLimit = x.ISINHERIT == "Y" ? c.UPPERALERTLIMIT : x.UPPERALERTLIMIT,
                                             UpperLimit = x.ISINHERIT == "Y" ? c.UPPERLIMIT : x.UPPERLIMIT
                                         }).ToList();

                    foreach (var equipment in equipmentList)
                    {
                        var checkResultList = db.CHECKRESULT.Where(x => x.EQUIPMENTUNIQUEID == equipment.UniqueID && x.PARTUNIQUEID == "*" && x.CHECKITEMUNIQUEID == Parameters.CheckItemUniqueID && (x.NETVALUE.HasValue || x.VALUE.HasValue) && string.Compare(x.CHECKDATE, Parameters.BeginDate) >= 0 && string.Compare(x.CHECKDATE, Parameters.EndDate) <= 0).ToList().Select(x => new CheckResultModel
                        {
                            CheckDate = x.CHECKDATE,
                            CheckTime = x.CHECKTIME,
                            NetValue = x.NETVALUE.HasValue ? Convert.ToDouble(x.NETVALUE) : default(double?),
                            Value = x.VALUE.HasValue ? Convert.ToDouble(x.VALUE) : default(double?)
                        }).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

                        if (checkResultList.Count > 1)
                        {
                            model.EquipmentList.Add(new EquipmentModel()
                            {
                                ControlPointDescription = string.Empty,
                                EquipmentName = equipment.Name,
                                PartDescription = string.Empty,
                                 LowerLimit = equipment.LowerLimit.HasValue ? Convert.ToDouble(equipment.LowerLimit) : default(double?),
                                LowerAlertLimit = equipment.LowerAlertLimit.HasValue ? Convert.ToDouble(equipment.LowerAlertLimit) : default(double?),
                                UpperAlertLimit = equipment.UpperAlertLimit.HasValue ? Convert.ToDouble(equipment.UpperAlertLimit) : default(double?),
                                UpperLimit = equipment.UpperLimit.HasValue ? Convert.ToDouble(equipment.UpperLimit) : default(double?),
                                CheckResultList = checkResultList,
                                Color = colorList[colorSeq]
                            });

                            colorSeq++;
                        }
                    }

                    var equipmentPartList = (from x in db.EQUIPMENTCHECKITEM
                                             join e in db.EQUIPMENT
                                             on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                             join p in db.EQUIPMENTPART
                                             on x.PARTUNIQUEID equals p.UNIQUEID
                                             join c in db.CHECKITEM
                                             on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                             where x.CHECKITEMUNIQUEID == Parameters.CheckItemUniqueID && Parameters.EquipmentPartList.Contains(p.UNIQUEID)
                                             select new
                                             {
                                                 UniqueID = e.UNIQUEID,
                                                 EquipmentName = e.NAME,
                                                 PartUniqueID = x.PARTUNIQUEID,
                                                 PartDescription = p.DESCRIPTION,
                                                 LowerLimit = x.ISINHERIT == "Y" ? c.LOWERLIMIT : x.LOWERLIMIT,
                                                 LowerAlertLimit = x.ISINHERIT == "Y" ? c.LOWERALERTLIMIT : x.LOWERALERTLIMIT,
                                                 UpperAlertLimit = x.ISINHERIT == "Y" ? c.UPPERALERTLIMIT : x.UPPERALERTLIMIT,
                                                 UpperLimit = x.ISINHERIT == "Y" ? c.UPPERLIMIT : x.UPPERLIMIT
                                             }).ToList();

                    foreach (var equipmentPart in equipmentPartList)
                    {
                        var checkResultList = db.CHECKRESULT.Where(x => x.EQUIPMENTUNIQUEID == equipmentPart.UniqueID && x.PARTUNIQUEID == equipmentPart.PartUniqueID && x.CHECKITEMUNIQUEID == Parameters.CheckItemUniqueID && (x.NETVALUE.HasValue || x.VALUE.HasValue) && string.Compare(x.CHECKDATE, Parameters.BeginDate) >= 0 && string.Compare(x.CHECKDATE, Parameters.EndDate) <= 0).ToList().Select(x => new CheckResultModel
                        {
                            CheckDate = x.CHECKDATE,
                            CheckTime = x.CHECKTIME,
                            NetValue = x.NETVALUE.HasValue ? Convert.ToDouble(x.NETVALUE) : default(double?),
                            Value = x.VALUE.HasValue ? Convert.ToDouble(x.VALUE) : default(double?)
                        }).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

                        if (checkResultList.Count > 1)
                        {
                            model.EquipmentList.Add(new EquipmentModel()
                            {
                                ControlPointDescription = string.Empty,
                                EquipmentName = equipmentPart.EquipmentName,
                                PartDescription = equipmentPart.PartDescription,
                                LowerLimit = equipmentPart.LowerLimit.HasValue ? Convert.ToDouble(equipmentPart.LowerLimit) : default(double?),
                                LowerAlertLimit = equipmentPart.LowerAlertLimit.HasValue ? Convert.ToDouble(equipmentPart.LowerAlertLimit) : default(double?),
                                UpperAlertLimit = equipmentPart.UpperAlertLimit.HasValue ? Convert.ToDouble(equipmentPart.UpperAlertLimit) : default(double?),
                                UpperLimit = equipmentPart.UpperLimit.HasValue ? Convert.ToDouble(equipmentPart.UpperLimit) : default(double?),
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

        public static RequestResult GetTreeItem(List<Organization> OrganizationList, string OrganizationUniqueID, string CheckType, Account Account)
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (!string.IsNullOrEmpty(CheckType))
                    {
                        var checkItemList = (from c in db.CHECKITEM
                                             join x in db.EQUIPMENTCHECKITEM
                                             on c.UNIQUEID equals x.CHECKITEMUNIQUEID into tmpE
                                             from x in tmpE.DefaultIfEmpty()
                                             join y in db.CONTROLPOINTCHECKITEM
                                             on c.UNIQUEID equals y.CHECKITEMUNIQUEID into tmpC
                                             from y in tmpC.DefaultIfEmpty()
                                             where c.ORGANIZATIONUNIQUEID == OrganizationUniqueID && c.CHECKTYPE == CheckType && c.ISFEELITEM=="N"
                                             select new
                                             {
                                                 CheckItemUniqueID = c.UNIQUEID,
                                                 CheckItemID = c.ID,
                                                 CheckItemDescription = c.DESCRIPTION,
                                                 Equipment = x != null ? x.EQUIPMENTUNIQUEID : "",
                                                 ControlPoint = y != null ? y.CONTROLPOINTUNIQUEID : ""
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
                        if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                        {
                            var checkTypeList = (from c in db.CHECKITEM
                                                 join x in db.EQUIPMENTCHECKITEM
                                                 on c.UNIQUEID equals x.CHECKITEMUNIQUEID into tmpE
                                                 from x in tmpE.DefaultIfEmpty()
                                                 join y in db.CONTROLPOINTCHECKITEM
                                                 on c.UNIQUEID equals y.CHECKITEMUNIQUEID into tmpC
                                                 from y in tmpC.DefaultIfEmpty()
                                                 where c.ORGANIZATIONUNIQUEID == OrganizationUniqueID && c.ISFEELITEM=="N"
                                                 select new
                                                 {
                                                     CheckType = c.CHECKTYPE,
                                                     Equipment = x != null ? x.EQUIPMENTUNIQUEID : "",
                                                     ControlPoint = y != null ? y.CONTROLPOINTUNIQUEID : ""
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
                        }

                        var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                        foreach (var organization in organizationList)
                        {
                            var downStream = OrganizationDataAccessor.GetDownStreamOrganizationList(OrganizationList, organization.UniqueID, true);

                            var query = (from c in db.CHECKITEM
                                         join x in db.EQUIPMENTCHECKITEM
                                         on c.UNIQUEID equals x.CHECKITEMUNIQUEID into tmpE
                                         from x in tmpE.DefaultIfEmpty()
                                         join y in db.CONTROLPOINTCHECKITEM
                                         on c.UNIQUEID equals y.CHECKITEMUNIQUEID into tmpC
                                         from y in tmpC.DefaultIfEmpty()
                                         where downStream.Contains(c.ORGANIZATIONUNIQUEID) && c.ISFEELITEM == "N" && Account.QueryableOrganizationUniqueIDList.Contains(c.ORGANIZATIONUNIQUEID)
                                         select new
                                         {
                                           UniqueID=  c.UNIQUEID,
                                             Equipment = x != null ? x.EQUIPMENTUNIQUEID : "",
                                             ControlPoint = y != null ? y.CONTROLPOINTUNIQUEID : ""
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
                    { Define.EnumTreeAttribute.CheckType, string.Empty },
                    { Define.EnumTreeAttribute.CheckItemUniqueID, string.Empty }
                };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var organization = OrganizationList.First(x => x.UniqueID == OrganizationUniqueID);

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

                    var downStream = OrganizationDataAccessor.GetDownStreamOrganizationList(OrganizationList, organization.UniqueID, true);

                    var query = (from c in db.CHECKITEM
                                 join x in db.EQUIPMENTCHECKITEM
                                 on c.UNIQUEID equals x.CHECKITEMUNIQUEID into tmpE
                                 from x in tmpE.DefaultIfEmpty()
                                 join y in db.CONTROLPOINTCHECKITEM
                                 on c.UNIQUEID equals y.CHECKITEMUNIQUEID into tmpC
                                 from y in tmpC.DefaultIfEmpty()
                                 where downStream.Contains(c.ORGANIZATIONUNIQUEID) && c.ISFEELITEM == "N" && Account.QueryableOrganizationUniqueIDList.Contains(c.ORGANIZATIONUNIQUEID)
                                 select new
                                 {
                                     UniqueID = c.UNIQUEID,
                                     Equipment = x != null ? x.EQUIPMENTUNIQUEID : "",
                                     ControlPoint = y != null ? y.CONTROLPOINTUNIQUEID : ""
                                 }).Where(x => !string.IsNullOrEmpty(x.Equipment) || !string.IsNullOrEmpty(x.ControlPoint)).Select(x => x.UniqueID).Distinct().OrderBy(x => x).ToList();

                    if (query.Count > 0)
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
    }
}
