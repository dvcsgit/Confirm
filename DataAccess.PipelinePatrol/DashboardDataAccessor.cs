using DbEntity.MSSQL;
using DbEntity.MSSQL.PipelinePatrol;
using Models.Authenticated;
using Models.PipelinePatrol.Dashboard;
using Models.PipelinePatrol.Shared;
using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace DataAccess.PipelinePatrol
{
    public class DashboardDataAccessor
    {
        public static RequestResult Query(string Zoom, string MapCenterLAT, string MapCenterLNG, List<string> SelectedList, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new DashboardViewModel();

                if (!string.IsNullOrEmpty(Zoom))
                {
                    model.DefaultZoom = int.Parse(Zoom);
                }
                else
                {
                    model.DefaultZoom = null;
                }

                if (!string.IsNullOrEmpty(MapCenterLAT) && !string.IsNullOrEmpty(MapCenterLNG))
                {
                    model.DefaultMapCenter = new Location()
                    {
                        LAT = double.Parse(MapCenterLAT),
                        LNG = double.Parse(MapCenterLNG)
                    };
                }
                else
                {
                    model.DefaultZoom = null;
                }

                using (DbEntities db = new DbEntities())
                {
                    using (PDbEntities pdb = new PDbEntities())
                    {
                        var onlineUserTime = DateTime.Now.AddDays(-1);

                        foreach (var selected in SelectedList)
                        {
                            string[] temp = selected.Split(Define.Seperators, StringSplitOptions.None);

                            var nodeType = Define.EnumParse<Define.EnumTreeNodeType>(temp[0]);

                            string inspectionUniqueID = temp[1];
                            string constructionUniqueID = temp[2];
                            string pipelineAbnormalUniqueID = temp[3];
                            string organizationUniqueID = temp[4];
                            string pipelineUniqueID = temp[5];
                            string pipePointType = temp[6];
                            string pipePointUniqueID = temp[7];
                            string userID = temp[8];

                            if (nodeType == Define.EnumTreeNodeType.InspectionRoot)
                            {
                                var inspectionList = pdb.Inspection.ToList();

                                foreach (var inspection in inspectionList)
                                {
                                    if (!model.InspectionList.Any(x => x.UniqueID == inspection.UniqueID))
                                    {
                                        var constructionFirm = pdb.ConstructionFirm.FirstOrDefault(x => x.UniqueID == inspection.ConstructionFirmUniqueID);
                                        var constructionType = pdb.ConstructionType.FirstOrDefault(x => x.UniqueID == inspection.ConstructionTypeUniqueID);

                                        model.InspectionList.Add(new InspectionViewModel()
                                        {
                                            UniqueID = inspection.UniqueID,
                                            VHNO = inspection.VHNO,
                                            Description = inspection.Description,
                                            ConstructionFirmUniqueID = inspection.ConstructionFirmUniqueID,
                                            ConstructionFirmName = constructionFirm != null ? constructionFirm.Name : string.Empty,
                                            ConstructionFirmRemark = inspection.ConstructionFirmRemark,
                                            ConstructionTypeUniqueID = inspection.ConstructionTypeUniqueID,
                                            ConstructionTypeDescription = constructionType != null ? constructionType.Description : string.Empty,
                                            ConstructionTypeRemark = inspection.ConstructionTypeRemark,
                                            LAT = inspection.LAT,
                                            LNG = inspection.LNG,
                                            IsInspected = pdb.InspectionUser.Any(x => x.InspectionUniqueID == inspection.UniqueID && x.UserID == Account.ID)
                                        });
                                    }
                                }
                            }
                            else if (nodeType == Define.EnumTreeNodeType.Inspection)
                            {
                                if (!model.InspectionList.Any(x => x.UniqueID == inspectionUniqueID))
                                {
                                    var inspection = pdb.Inspection.First(x => x.UniqueID == inspectionUniqueID);
                                    var constructionFirm = pdb.ConstructionFirm.FirstOrDefault(x => x.UniqueID == inspection.ConstructionFirmUniqueID);
                                    var constructionType = pdb.ConstructionType.FirstOrDefault(x => x.UniqueID == inspection.ConstructionTypeUniqueID);

                                    model.InspectionList.Add(new InspectionViewModel()
                                    {
                                        UniqueID = inspection.UniqueID,
                                        VHNO = inspection.VHNO,
                                        Description = inspection.Description,
                                        ConstructionFirmUniqueID = inspection.ConstructionFirmUniqueID,
                                        ConstructionFirmName = constructionFirm != null ? constructionFirm.Name : string.Empty,
                                        ConstructionFirmRemark = inspection.ConstructionFirmRemark,
                                        ConstructionTypeUniqueID = inspection.ConstructionTypeUniqueID,
                                        ConstructionTypeDescription = constructionType != null ? constructionType.Description : string.Empty,
                                        ConstructionTypeRemark = inspection.ConstructionTypeRemark,
                                        LAT = inspection.LAT,
                                        LNG = inspection.LNG,
                                        IsInspected = pdb.InspectionUser.Any(x => x.InspectionUniqueID == inspection.UniqueID && x.UserID == Account.ID)
                                    });
                                }
                            }
                            else if (nodeType == Define.EnumTreeNodeType.ConstructionRoot)
                            {
                                var constructionList = pdb.Construction.Where(x => !x.ClosedTime.HasValue).ToList();

                                foreach (var construction in constructionList)
                                {
                                    if (!model.ConstructionList.Any(x => x.UniqueID == construction.UniqueID))
                                    {
                                        var constructionFirm = pdb.ConstructionFirm.FirstOrDefault(x => x.UniqueID == construction.ConstructionFirmUniqueID);
                                        var constructionType = pdb.ConstructionType.FirstOrDefault(x => x.UniqueID == construction.ConstructionTypeUniqueID);

                                        model.ConstructionList.Add(new ConstructionViewModel()
                                        {
                                            UniqueID = construction.UniqueID,
                                            VHNO = construction.VHNO,
                                            Description = construction.Description,
                                            ConstructionFirmUniqueID = construction.ConstructionFirmUniqueID,
                                            ConstructionFirmName = constructionFirm != null ? constructionFirm.Name : string.Empty,
                                            ConstructionFirmRemark = construction.ConstructionFirmRemark,
                                            ConstructionTypeUniqueID = construction.ConstructionTypeUniqueID,
                                            ConstructionTypeDescription = constructionType != null ? constructionType.Description : string.Empty,
                                            ConstructionTypeRemark = construction.ConstructionTypeRemark,
                                            LAT = construction.LAT,
                                            LNG = construction.LNG
                                        });
                                    }
                                }
                            }
                            else if (nodeType == Define.EnumTreeNodeType.Construction)
                            {
                                if (!model.ConstructionList.Any(x => x.UniqueID == constructionUniqueID))
                                {
                                    var construction = pdb.Inspection.First(x => x.UniqueID == constructionUniqueID);
                                    var constructionFirm = pdb.ConstructionFirm.FirstOrDefault(x => x.UniqueID == construction.ConstructionFirmUniqueID);
                                    var constructionType = pdb.ConstructionType.FirstOrDefault(x => x.UniqueID == construction.ConstructionTypeUniqueID);

                                    model.ConstructionList.Add(new ConstructionViewModel()
                                    {
                                        UniqueID = construction.UniqueID,
                                        VHNO = construction.VHNO,
                                        Description = construction.Description,
                                        ConstructionFirmUniqueID = construction.ConstructionFirmUniqueID,
                                        ConstructionFirmName = constructionFirm != null ? constructionFirm.Name : string.Empty,
                                        ConstructionFirmRemark = construction.ConstructionFirmRemark,
                                        ConstructionTypeUniqueID = construction.ConstructionTypeUniqueID,
                                        ConstructionTypeDescription = constructionType != null ? constructionType.Description : string.Empty,
                                        ConstructionTypeRemark = construction.ConstructionTypeRemark,
                                        LAT = construction.LAT,
                                        LNG = construction.LNG
                                    });
                                }
                            }
                            else if (nodeType == Define.EnumTreeNodeType.PipelineAbnormalRoot)
                            {
                                var pipelineAbnormalList = pdb.PipelineAbnormal.Where(x => !x.ClosedTime.HasValue).ToList();

                                foreach (var pipelineAbnormal in pipelineAbnormalList)
                                {
                                    if (!model.PipelineAbnormalList.Any(x => x.UniqueID == pipelineAbnormal.UniqueID))
                                    {
                                        var abnormalReason = pdb.AbnormalReason.FirstOrDefault(x => x.UniqueID == pipelineAbnormal.AbnormalReasonUniqueID);

                                        model.PipelineAbnormalList.Add(new PipelineAbnormalViewModel()
                                        {
                                            UniqueID = pipelineAbnormal.UniqueID,
                                            VHNO = pipelineAbnormal.VHNO,
                                            Description = pipelineAbnormal.Description,
                                            AbnormalReasonUniqueID = pipelineAbnormal.AbnormalReasonUniqueID,
                                            AbnormalReasonDescription = abnormalReason != null ? abnormalReason.Description : string.Empty,
                                            AbnormalReasonRemark = pipelineAbnormal.AbnormalReasonRemark,
                                            LAT = pipelineAbnormal.LAT,
                                            LNG = pipelineAbnormal.LNG
                                        });
                                    }
                                }
                            }
                            else if (nodeType == Define.EnumTreeNodeType.PipelineAbnormal)
                            {
                                if (!model.PipelineAbnormalList.Any(x => x.UniqueID == pipelineAbnormalUniqueID))
                                {
                                    var pipelineAbnormal = pdb.PipelineAbnormal.First(x => x.UniqueID == pipelineAbnormalUniqueID);
                                    var abnormalReason = pdb.AbnormalReason.FirstOrDefault(x => x.UniqueID == pipelineAbnormal.AbnormalReasonUniqueID);

                                    model.PipelineAbnormalList.Add(new PipelineAbnormalViewModel()
                                    {
                                        UniqueID = pipelineAbnormal.UniqueID,
                                        VHNO = pipelineAbnormal.VHNO,
                                        Description = pipelineAbnormal.Description,
                                        AbnormalReasonUniqueID = pipelineAbnormal.AbnormalReasonUniqueID,
                                        AbnormalReasonDescription = abnormalReason != null ? abnormalReason.Description : string.Empty,
                                        AbnormalReasonRemark = pipelineAbnormal.AbnormalReasonRemark,
                                        LAT = pipelineAbnormal.LAT,
                                        LNG = pipelineAbnormal.LNG
                                    });
                                }
                            }
                            else if (nodeType == Define.EnumTreeNodeType.Pipeline)
                            {
                                if (!model.PipelineList.Any(x => x.UniqueID == pipelineUniqueID))
                                {
                                    var pipeline = pdb.Pipeline.First(x => x.UniqueID == pipelineUniqueID);

                                    model.PipelineList.Add(new PipelineViewModel()
                                    {
                                        UniqueID = pipeline.UniqueID,
                                        OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(pipeline.OrganizationUniqueID),
                                        ID = pipeline.ID,
                                        Color = pipeline.Color,
                                        Locus = pdb.PipelineLocus.Where(x => x.PipelineUniqueID == pipeline.UniqueID).OrderBy(x => x.Seq).Select(x => new Location()
                                        {
                                            LAT = x.LAT,
                                            LNG = x.LNG
                                        }).ToList()
                                    });
                                }
                            }
                            else if (nodeType == Define.EnumTreeNodeType.PipePointRoot)
                            {
                                var pipePointList = pdb.PipePoint.Where(x => x.OrganizationUniqueID == organizationUniqueID).ToList();

                                foreach (var pipePoint in pipePointList)
                                {
                                    if (!model.PipePointList.Any(x => x.UniqueID == pipePoint.UniqueID))
                                    {
                                        model.PipePointList.Add(new PipePointViewModel()
                                        {
                                            UniqueID = pipePoint.UniqueID,
                                            ID = pipePoint.ID,
                                            Name = pipePoint.Name,
                                            PointType = pipePoint.PointType,
                                            Location = new Location { LNG = pipePoint.LNG, LAT = pipePoint.LAT }
                                        });
                                    }
                                }
                            }
                            else if (nodeType == Define.EnumTreeNodeType.PipePointType)
                            {
                                var pipePointList = pdb.PipePoint.Where(x => x.OrganizationUniqueID == organizationUniqueID && x.PointType == pipePointType).ToList();

                                foreach (var pipePoint in pipePointList)
                                {
                                    if (!model.PipePointList.Any(x => x.UniqueID == pipePoint.UniqueID))
                                    {
                                        model.PipePointList.Add(new PipePointViewModel()
                                        {
                                            UniqueID = pipePoint.UniqueID,
                                            ID = pipePoint.ID,
                                            Name = pipePoint.Name,
                                            PointType = pipePoint.PointType,
                                            Location = new Location { LNG = pipePoint.LNG, LAT = pipePoint.LAT }
                                        });
                                    }
                                }
                            }
                            else if (nodeType == Define.EnumTreeNodeType.PipePoint)
                            {
                                if (!model.PipePointList.Any(x => x.UniqueID == pipePointUniqueID))
                                {
                                    var pipePoint = pdb.PipePoint.First(x => x.UniqueID == pipePointUniqueID);

                                    model.PipePointList.Add(new PipePointViewModel()
                                    {
                                        UniqueID = pipePoint.UniqueID,
                                        ID = pipePoint.ID,
                                        Name = pipePoint.Name,
                                        PointType = pipePoint.PointType,
                                        Location = new Location { LNG = pipePoint.LNG, LAT = pipePoint.LAT }
                                    });
                                }
                            }
                            else if (nodeType == Define.EnumTreeNodeType.User)
                            {
                                if (!model.OnlineUserList.Any(x => x.ID == userID))
                                {
                                    var onlineUser = pdb.OnlineUserLocus.Where(x => x.UserID == userID && DateTime.Compare(x.UpdateTime, onlineUserTime) >= 0).OrderByDescending(x => x.UpdateTime).FirstOrDefault();

                                    if (onlineUser != null)
                                    {
                                        var user = UserDataAccessor.GetUser(onlineUser.UserID);

                                        model.OnlineUserList.Add(new OnlineUserViewModel()
                                        {
                                            OrganizationDescription = user.OrganizationDescription,
                                            ID = user.ID,
                                            Name = user.Name,
                                            LNG = onlineUser.LNG,
                                            LAT = onlineUser.LAT,
                                            UpdateTime = onlineUser.UpdateTime
                                        });
                                    }
                                }
                            }
                            else
                            {
                                var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organizationUniqueID, true);

                                #region Pipeline
                                var pipelineList = pdb.Pipeline.Where(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID)).ToList();

                                foreach (var pipeline in pipelineList)
                                {
                                    if (!model.PipelineList.Any(x => x.UniqueID == pipelineUniqueID))
                                    {
                                        model.PipelineList.Add(new PipelineViewModel()
                                        {
                                            UniqueID = pipeline.UniqueID,
                                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(pipeline.OrganizationUniqueID),
                                            ID = pipeline.ID,
                                            Color = pipeline.Color,
                                            Locus = pdb.PipelineLocus.Where(x => x.PipelineUniqueID == pipeline.UniqueID).OrderBy(x => x.Seq).Select(x => new Location()
                                            {
                                                LAT = x.LAT,
                                                LNG = x.LNG
                                            }).ToList()
                                        });
                                    }
                                }
                                #endregion

                                #region PipePoint
                                var pipePointList = pdb.PipePoint.Where(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID)).ToList();

                                foreach (var pipePoint in pipePointList)
                                {
                                    if (!model.PipePointList.Any(x => x.UniqueID == pipePointUniqueID))
                                    {
                                        model.PipePointList.Add(new PipePointViewModel()
                                        {
                                            UniqueID = pipePoint.UniqueID,
                                            ID = pipePoint.ID,
                                            Name = pipePoint.Name,
                                            PointType = pipePoint.PointType,
                                            Location = new Location { LNG = pipePoint.LNG, LAT = pipePoint.LAT }
                                        });
                                    }
                                }
                                #endregion

                                #region OnlineUser
                                var organizationUserList = db.User.Where(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID)).Select(x => x.ID).ToList();

                                var onlineUserIDList = pdb.OnlineUserLocus.Where(x => organizationUserList.Contains(x.UserID)).Select(x => x.UserID).Distinct().ToList();

                                foreach (var onlineUserID in onlineUserIDList)
                                {
                                    if (!model.OnlineUserList.Any(x => x.ID == onlineUserID))
                                    {
                                        var onlineUser = pdb.OnlineUserLocus.Where(x => x.UserID == onlineUserID && DateTime.Compare(x.UpdateTime, onlineUserTime) >= 0).OrderByDescending(x => x.UpdateTime).FirstOrDefault();

                                        if (onlineUser != null)
                                        {
                                            var user = UserDataAccessor.GetUser(onlineUser.UserID);

                                            model.OnlineUserList.Add(new OnlineUserViewModel()
                                            {
                                                OrganizationDescription = user.OrganizationDescription,
                                                ID = user.ID,
                                                Name = user.Name,
                                                LNG = onlineUser.LNG,
                                                LAT = onlineUser.LAT,
                                                UpdateTime = onlineUser.UpdateTime
                                            });
                                        }
                                        
                                    }
                                }
                                #endregion
                            }
                        }
                    }
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

        public static RequestResult GetTreeItem(Define.EnumTreeNodeType NodeType, string OrganizationUniqueID, string PipePointType, Account Account)
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
                    { Define.EnumTreeAttribute.PipelineUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.PipePointType, string.Empty },
                    { Define.EnumTreeAttribute.PipePointUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.ConstructionUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.InspectionUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.PipelineAbnormalUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.UserID, string.Empty },
                    { Define.EnumTreeAttribute.Class, "jstree-checked" }
                };

                using (PDbEntities pdb = new PDbEntities())
                {
                    if (NodeType == Define.EnumTreeNodeType.ConstructionRoot)
                    {
                        var constructionList = pdb.Construction.Where(x => !x.ClosedTime.HasValue).OrderByDescending(x => x.VHNO).ToList();

                        foreach (var construction in constructionList)
                        {
                            var treeItem = new TreeItem() { Title = construction.VHNO };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Construction.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = construction.VHNO;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.PipelineUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.PipePointType] = string.Empty;
                            attributes[Define.EnumTreeAttribute.PipePointUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.ConstructionUniqueID] = construction.UniqueID;
                            attributes[Define.EnumTreeAttribute.InspectionUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.PipelineAbnormalUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.UserID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else if (NodeType == Define.EnumTreeNodeType.InspectionRoot)
                    {
                        var inspectionList = pdb.Inspection.OrderByDescending(x=>x.VHNO).ToList();

                        foreach (var inspection in inspectionList)
                        {
                            var treeItem = new TreeItem() { Title = inspection.VHNO };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Inspection.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = inspection.VHNO;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.PipelineUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.PipePointType] = string.Empty;
                            attributes[Define.EnumTreeAttribute.PipePointUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.ConstructionUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.InspectionUniqueID] = inspection.UniqueID;
                            attributes[Define.EnumTreeAttribute.PipelineAbnormalUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.UserID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else if (NodeType == Define.EnumTreeNodeType.PipelineAbnormalRoot)
                    {
                        var pipelineAbnormalList = pdb.PipelineAbnormal.Where(x => !x.ClosedTime.HasValue).OrderByDescending(x => x.VHNO).ToList();

                        foreach (var pipelineAbnormal in pipelineAbnormalList)
                        {
                            var treeItem = new TreeItem() { Title = pipelineAbnormal.VHNO };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.PipelineAbnormal.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = pipelineAbnormal.VHNO;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.PipelineUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.PipePointType] = string.Empty;
                            attributes[Define.EnumTreeAttribute.PipePointUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.ConstructionUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.InspectionUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.PipelineAbnormalUniqueID] = pipelineAbnormal.UniqueID;
                            attributes[Define.EnumTreeAttribute.UserID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else if (NodeType == Define.EnumTreeNodeType.PipePointRoot)
                    {
                        var pipePointTypeList = pdb.PipePoint.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).Select(x => x.PointType).Distinct().OrderBy(x => x).ToList();

                        foreach (var pipePointType in pipePointTypeList)
                        {
                            var treeItem = new TreeItem() { Title = pipePointType };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.PipePointType.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = pipePointType;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.PipelineUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.PipePointType] = pipePointType;
                            attributes[Define.EnumTreeAttribute.PipePointUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.ConstructionUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.InspectionUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.PipelineAbnormalUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.UserID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItem.State = "closed";

                            treeItemList.Add(treeItem);
                        }
                    }
                    else if (NodeType == Define.EnumTreeNodeType.PipePointType)
                    {
                        var pipePointList = pdb.PipePoint.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && x.PointType == PipePointType).OrderBy(x => x.ID).ToList();

                        foreach (var pipePoint in pipePointList)
                        {
                            var treeItem = new TreeItem() { Title = pipePoint.ID };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.PipePoint.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", pipePoint.ID, pipePoint.Name);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = pipePoint.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.PipelineUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.PipePointType] = pipePoint.PointType;
                            attributes[Define.EnumTreeAttribute.PipePointUniqueID] = pipePoint.UniqueID;
                            attributes[Define.EnumTreeAttribute.ConstructionUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.InspectionUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.PipelineAbnormalUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.UserID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else
                    {
                        if (OrganizationUniqueID == "*")
                        {
                            if (pdb.Inspection.Count() > 0)
                            {
                                var treeItem = new TreeItem() { Title = Resources.Resource.Inspection };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.InspectionRoot.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = Resources.Resource.Inspection;
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                                attributes[Define.EnumTreeAttribute.PipelineUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.PipePointType] = string.Empty;
                                attributes[Define.EnumTreeAttribute.PipePointUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.ConstructionUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.InspectionUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.PipelineAbnormalUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.UserID] = string.Empty;

                                foreach (var attribute in attributes)
                                {
                                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                }

                                treeItem.State = "closed";

                                treeItemList.Add(treeItem);
                            }

                            if (pdb.Construction.Count(x => !x.ClosedTime.HasValue) > 0)
                            {
                                var treeItem = new TreeItem() { Title = Resources.Resource.Construction };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.ConstructionRoot.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = Resources.Resource.Construction;
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                                attributes[Define.EnumTreeAttribute.PipelineUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.PipePointType] = string.Empty;
                                attributes[Define.EnumTreeAttribute.PipePointUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.ConstructionUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.InspectionUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.PipelineAbnormalUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.UserID] = string.Empty;

                                foreach (var attribute in attributes)
                                {
                                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                }

                                treeItem.State = "closed";

                                treeItemList.Add(treeItem);
                            }

                            if (pdb.PipelineAbnormal.Count(x => !x.ClosedTime.HasValue) > 0)
                            {
                                var treeItem = new TreeItem() { Title = Resources.Resource.PipelineAbnormal };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.PipelineAbnormalRoot.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = Resources.Resource.PipelineAbnormal;
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                                attributes[Define.EnumTreeAttribute.PipelineUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.PipePointType] = string.Empty;
                                attributes[Define.EnumTreeAttribute.PipePointUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.ConstructionUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.InspectionUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.PipelineAbnormalUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.UserID] = string.Empty;

                                foreach (var attribute in attributes)
                                {
                                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                }

                                treeItem.State = "closed";

                                treeItemList.Add(treeItem);
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
                                    attributes[Define.EnumTreeAttribute.PipelineUniqueID] = string.Empty;
                                    attributes[Define.EnumTreeAttribute.PipePointType] = string.Empty;
                                    attributes[Define.EnumTreeAttribute.PipePointUniqueID] = string.Empty;
                                    attributes[Define.EnumTreeAttribute.ConstructionUniqueID] = string.Empty;
                                    attributes[Define.EnumTreeAttribute.InspectionUniqueID] = string.Empty;
                                    attributes[Define.EnumTreeAttribute.PipelineAbnormalUniqueID] = string.Empty;
                                    attributes[Define.EnumTreeAttribute.UserID] = string.Empty;

                                    foreach (var attribute in attributes)
                                    {
                                        treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                    }

                                    var haveDownStreamOrganization = db.Organization.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID));

                                    var havePipeline = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && pdb.Pipeline.Any(x => x.OrganizationUniqueID == organization.UniqueID);

                                    var havePipePoint = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && pdb.PipePoint.Any(x => x.OrganizationUniqueID == organization.UniqueID);

                                    var organizationUserList = db.User.Where(x => x.OrganizationUniqueID == organization.UniqueID).Select(x => x.ID).ToList();

                                    var haveOnlineUser = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && pdb.OnlineUserLocus.Any(x => organizationUserList.Contains(x.UserID));

                                    if (haveDownStreamOrganization || havePipeline || havePipePoint || haveOnlineUser)
                                    {
                                        treeItem.State = "closed";
                                    }

                                    treeItemList.Add(treeItem);
                                }
                            }
                        }
                        else
                        {
                            using (DbEntities db = new DbEntities())
                            {
                                if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                                {
                                    var pipelineList = pdb.Pipeline.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                                    foreach (var pipeline in pipelineList)
                                    {
                                        var treeItem = new TreeItem() { Title = pipeline.ID };

                                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Pipeline.ToString();
                                        attributes[Define.EnumTreeAttribute.ToolTip] = pipeline.ID;
                                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                                        attributes[Define.EnumTreeAttribute.PipelineUniqueID] = pipeline.UniqueID;
                                        attributes[Define.EnumTreeAttribute.PipePointType] = string.Empty;
                                        attributes[Define.EnumTreeAttribute.PipePointUniqueID] = string.Empty;
                                        attributes[Define.EnumTreeAttribute.ConstructionUniqueID] = string.Empty;
                                        attributes[Define.EnumTreeAttribute.InspectionUniqueID] = string.Empty;
                                        attributes[Define.EnumTreeAttribute.PipelineAbnormalUniqueID] = string.Empty;
                                        attributes[Define.EnumTreeAttribute.UserID] = string.Empty;

                                        foreach (var attribute in attributes)
                                        {
                                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                        }

                                        treeItemList.Add(treeItem);
                                    }

                                    if (pdb.PipePoint.Any(x => x.OrganizationUniqueID == OrganizationUniqueID))
                                    {
                                        var treeItem = new TreeItem() { Title = Resources.Resource.PipePoint };

                                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.PipePointRoot.ToString();
                                        attributes[Define.EnumTreeAttribute.ToolTip] = Resources.Resource.PipePoint;
                                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                                        attributes[Define.EnumTreeAttribute.PipelineUniqueID] = string.Empty;
                                        attributes[Define.EnumTreeAttribute.PipePointType] = string.Empty;
                                        attributes[Define.EnumTreeAttribute.PipePointUniqueID] = string.Empty;
                                        attributes[Define.EnumTreeAttribute.ConstructionUniqueID] = string.Empty;
                                        attributes[Define.EnumTreeAttribute.InspectionUniqueID] = string.Empty;
                                        attributes[Define.EnumTreeAttribute.PipelineAbnormalUniqueID] = string.Empty;
                                        attributes[Define.EnumTreeAttribute.UserID] = string.Empty;

                                        foreach (var attribute in attributes)
                                        {
                                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                        }

                                        treeItem.State = "closed";

                                        treeItemList.Add(treeItem);
                                    }

                                    var organizationUserList = db.User.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).Select(x => x.ID).ToList();

                                    var onlineUserList = pdb.OnlineUserLocus.Where(x => organizationUserList.Contains(x.UserID)).Select(x => x.UserID).Distinct().ToList();

                                    foreach (var onlineUser in onlineUserList)
                                    {
                                        var user = UserDataAccessor.GetUser(onlineUser);

                                        var treeItem = new TreeItem() { Title = string.Format("{0}/{1}", user.OrganizationDescription, user.Name) };

                                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.User.ToString();
                                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", user.ID, user.Name);
                                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                                        attributes[Define.EnumTreeAttribute.PipelineUniqueID] = string.Empty;
                                        attributes[Define.EnumTreeAttribute.PipePointType] = string.Empty;
                                        attributes[Define.EnumTreeAttribute.PipePointUniqueID] = string.Empty;
                                        attributes[Define.EnumTreeAttribute.ConstructionUniqueID] = string.Empty;
                                        attributes[Define.EnumTreeAttribute.InspectionUniqueID] = string.Empty;
                                        attributes[Define.EnumTreeAttribute.PipelineAbnormalUniqueID] = string.Empty;
                                        attributes[Define.EnumTreeAttribute.UserID] = user.ID;

                                        foreach (var attribute in attributes)
                                        {
                                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                        }

                                        treeItemList.Add(treeItem);
                                    }
                                }


                                var organizationList = db.Organization.Where(x => x.ParentUniqueID == OrganizationUniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                                foreach (var organization in organizationList)
                                {
                                    var treeItem = new TreeItem() { Title = organization.Description };

                                    attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                                    attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                                    attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                                    attributes[Define.EnumTreeAttribute.PipelineUniqueID] = string.Empty;
                                    attributes[Define.EnumTreeAttribute.PipePointType] = string.Empty;
                                    attributes[Define.EnumTreeAttribute.PipePointUniqueID] = string.Empty;
                                    attributes[Define.EnumTreeAttribute.ConstructionUniqueID] = string.Empty;
                                    attributes[Define.EnumTreeAttribute.InspectionUniqueID] = string.Empty;
                                    attributes[Define.EnumTreeAttribute.PipelineAbnormalUniqueID] = string.Empty;
                                    attributes[Define.EnumTreeAttribute.UserID] = string.Empty;

                                    foreach (var attribute in attributes)
                                    {
                                        treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                    }

                                    var haveDownStreamOrganization = db.Organization.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID));

                                    var havePipeline = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && pdb.Pipeline.Any(x => x.OrganizationUniqueID == organization.UniqueID);

                                    var havePipePoint = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && pdb.PipePoint.Any(x => x.OrganizationUniqueID == organization.UniqueID);

                                    var organizationUserList = db.User.Where(x => x.OrganizationUniqueID == organization.UniqueID).Select(x => x.ID).ToList();

                                    var haveOnlineUser = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && pdb.OnlineUserLocus.Any(x => organizationUserList.Contains(x.UserID));

                                    if (haveDownStreamOrganization || havePipeline || havePipePoint || haveOnlineUser)
                                    {
                                        treeItem.State = "closed";
                                    }

                                    treeItemList.Add(treeItem);
                                }
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
