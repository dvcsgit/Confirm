using DbEntity.MSSQL;
using DbEntity.MSSQL.PipelinePatrol;
using Models.Authenticated;
using Models.PipelinePatrol.PipePointManagement;
using Models.PipelinePatrol.Shared;
using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;
using Utility.Models;
using System.Transactions;

namespace DataAccess.PipelinePatrol
{
    public class PipePointDataAccessor
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
                        OrganizationUniqueID = Parameters.OrganizationUniqueID,
                        Permission = Account.OrganizationPermission(Parameters.OrganizationUniqueID),
                        PipePointType = Parameters.PipePointType,
                        OrganizationDescription = organization.Description,
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
                        Remark = pipePoint.Remark,
                        Location = new Location()
                        {
                            LAT = pipePoint.LAT,
                            LNG = pipePoint.LNG
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

        public static RequestResult GetCreateFormModel(string OrganizationUniqueID, string PipePointType)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    var model = new CreateFormModel()
                    {
                        OrganizationUniqueID = OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(OrganizationUniqueID),
                        PipePointTypeSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne),
                            new SelectListItem()
                            {
                                Text = Resources.Resource.Create + "...",
                                Value = Define.NEW
                            }
                        },
                        FormInput = new FormInput()
                        {
                            PipePointType = PipePointType
                        }
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(OrganizationUniqueID, true);

                    model.PipePointTypeSelectItemList.AddRange(db.PipePoint.Where(x => upStreamOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.PointType).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    if (!string.IsNullOrEmpty(PipePointType) && model.PipePointTypeSelectItemList.Any(x => x.Value == PipePointType))
                    {
                        model.PipePointTypeSelectItemList.First(x => x.Value == PipePointType).Selected = true;
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
                if (Model.FormInput.PipePointType == Define.NEW)
                {
                    result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.Unsupported, Resources.Resource.PipePointType));
                }
                else
                {
                    using (PDbEntities db = new PDbEntities())
                    {
                        var exists = db.PipePoint.FirstOrDefault(x => x.OrganizationUniqueID == Model.OrganizationUniqueID && x.PointType == Model.FormInput.PipePointType && x.ID == Model.FormInput.ID);

                        if (exists == null)
                        {
                            string uniqueID = Guid.NewGuid().ToString();

                            db.PipePoint.Add(new PipePoint()
                            {
                                UniqueID = uniqueID,
                                OrganizationUniqueID = Model.OrganizationUniqueID,
                                PointType = Model.FormInput.PipePointType,
                                ID = Model.FormInput.ID,
                                Name = Model.FormInput.Name,
                                Remark = Model.FormInput.Remark,
                                IsFeelItemDefaultNormal = false,
                                LNG = Model.FormInput.LNG.Value,
                                LAT = Model.FormInput.LAT.Value,
                                LastModifyTime = DateTime.Now
                            });

                            db.SaveChanges();

                            result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.PipePoint, Resources.Resource.Success));
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.PipePointID, Resources.Resource.Exists));
                        }
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
                using (PDbEntities db = new PDbEntities())
                {
                    var pipePoint = db.PipePoint.First(x => x.UniqueID == UniqueID);

                    var model = new EditFormModel()
                    {
                        UniqueID = pipePoint.UniqueID,
                        OrganizationUniqueID = pipePoint.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(pipePoint.OrganizationUniqueID),
                        PipePointTypeSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne),
                            new SelectListItem()
                            {
                                Text = Resources.Resource.Create + "...",
                                Value = Define.NEW
                            }
                        },
                        FormInput = new FormInput()
                        {
                            PipePointType = pipePoint.PointType,
                            ID = pipePoint.ID,
                            Name = pipePoint.Name,
                            Remark = pipePoint.Remark,
                            LAT = pipePoint.LAT,
                            LNG = pipePoint.LNG
                        }
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(model.OrganizationUniqueID, true);

                    model.PipePointTypeSelectItemList.AddRange(db.PipePoint.Where(x => upStreamOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.PointType).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    if (!string.IsNullOrEmpty(model.FormInput.PipePointType) && model.PipePointTypeSelectItemList.Any(x => x.Value == model.FormInput.PipePointType))
                    {
                        model.PipePointTypeSelectItemList.First(x => x.Value == model.FormInput.PipePointType).Selected = true;
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

        public static RequestResult Edit(EditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                if (Model.FormInput.PipePointType == Define.NEW)
                {
                    result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.Unsupported, Resources.Resource.PipePointType));
                }
                else
                {
                    using (PDbEntities db = new PDbEntities())
                    {
                        var pipePoint = db.PipePoint.First(x => x.UniqueID == Model.UniqueID);

                        var exists = db.PipePoint.FirstOrDefault(x => x.UniqueID != pipePoint.UniqueID && x.OrganizationUniqueID == pipePoint.OrganizationUniqueID && x.PointType == Model.FormInput.PipePointType && x.ID == Model.FormInput.ID);

                        if (exists == null)
                        {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                            #region PipePoint
                            pipePoint.PointType = Model.FormInput.PipePointType;
                            pipePoint.ID = Model.FormInput.ID;
                            pipePoint.Name = Model.FormInput.Name;
                            pipePoint.Remark = Model.FormInput.Remark;
                            pipePoint.LAT = Model.FormInput.LAT.Value;
                            pipePoint.LNG = Model.FormInput.LNG.Value;
                            pipePoint.LastModifyTime = DateTime.Now;

                            db.SaveChanges();
                            #endregion


#if !DEBUG
                        trans.Complete();
                    }
#endif
                            result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.PipePoint, Resources.Resource.Success));
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.PipePointID, Resources.Resource.Exists));
                        }
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
    }
}
