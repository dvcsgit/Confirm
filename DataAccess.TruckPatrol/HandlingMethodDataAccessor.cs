using System;
using System.Linq;
using System.Web.Mvc;
using System.Reflection;
#if !DEBUG
using System.Transactions;
#endif
using System.Collections.Generic;
using Utility;
using Utility.Models;
#if ORACLE
using DbEntity.ORACLE;
using DbEntity.ORACLE.EquipmentMaintenance;
#else
using DbEntity.MSSQL;
using DbEntity.MSSQL.TruckPatrol;
#endif
using Models.Shared;
using Models.Authenticated;
using Models.TruckPatrol.HandlingMethodManagement;

namespace DataAccess.TruckPatrol
{
    public class HandlingMethodDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (TDbEntities db = new TDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.HandlingMethod.Where(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.HandlingMethodType))
                    {
                        query = query.Where(x => x.OrganizationUniqueID == Parameters.OrganizationUniqueID && x.HandlingMethodType == Parameters.HandlingMethodType);
                    }

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.ID.Contains(Parameters.Keyword) || x.Description.Contains(Parameters.Keyword));
                    }

                    var organization = OrganizationDataAccessor.GetOrganization(Parameters.OrganizationUniqueID);

                    result.ReturnData(new GridViewModel()
                    {
                        OrganizationUniqueID = Parameters.OrganizationUniqueID,
                        Permission = Account.OrganizationPermission(Parameters.OrganizationUniqueID),
                        HandlingMethodType = Parameters.HandlingMethodType,
                        OrganizationDescription = organization.Description,
                        FullOrganizationDescription = organization.FullDescription,
                        ItemList = query.ToList().Select(x => new GridItem()
                        {
                            UniqueID = x.UniqueID,
                            Permission = Account.OrganizationPermission(x.OrganizationUniqueID),
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.OrganizationUniqueID),
                            HandlingMethodType = x.HandlingMethodType,
                            ID = x.ID,
                            Description = x.Description
                        }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.HandlingMethodType).ThenBy(x => x.ID).ToList()
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
                using (TDbEntities db = new TDbEntities())
                {
                    var handlingMethod = db.HandlingMethod.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = handlingMethod.UniqueID,
                        Permission = Account.OrganizationPermission(handlingMethod.OrganizationUniqueID),
                        OrganizationUniqueID = handlingMethod.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(handlingMethod.OrganizationUniqueID),
                        HandlingMethodType = handlingMethod.HandlingMethodType,
                        ID = handlingMethod.ID,
                        Description = handlingMethod.Description
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

        public static RequestResult GetCreateFormModel(string OrganizationUniqueID, string HandlingMethodType)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (TDbEntities db = new TDbEntities())
                {
                    var model = new CreateFormModel()
                    {
                        OrganizationUniqueID = OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(OrganizationUniqueID),
                        HandlingMethodTypeSelectItemList = new List<SelectListItem>() 
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
                            HandlingMethodType = HandlingMethodType
                        }
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(OrganizationUniqueID, true);

                    model.HandlingMethodTypeSelectItemList.AddRange(db.HandlingMethod.Where(x => upStreamOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.HandlingMethodType).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    if (!string.IsNullOrEmpty(HandlingMethodType) && model.HandlingMethodTypeSelectItemList.Any(x => x.Value == HandlingMethodType))
                    {
                        model.HandlingMethodTypeSelectItemList.First(x => x.Value == HandlingMethodType).Selected = true;
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
                if (Model.FormInput.HandlingMethodType == Define.OTHER || Model.FormInput.HandlingMethodType == Define.NEW)
                {
                    result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.Unsupported, Resources.Resource.HandlingMethodType));
                }
                else
                {
                    using (TDbEntities db = new TDbEntities())
                    {
                        var exists = db.HandlingMethod.FirstOrDefault(x => x.OrganizationUniqueID == Model.OrganizationUniqueID && x.HandlingMethodType == Model.FormInput.HandlingMethodType && x.ID == Model.FormInput.ID);

                        if (exists == null)
                        {
                            string uniqueID = Guid.NewGuid().ToString();

                            db.HandlingMethod.Add(new HandlingMethod()
                            {
                                UniqueID = uniqueID,
                                OrganizationUniqueID = Model.OrganizationUniqueID,
                                HandlingMethodType = Model.FormInput.HandlingMethodType,
                                ID = Model.FormInput.ID,
                                Description = Model.FormInput.Description,
                                LastModifyTime = DateTime.Now
                            });

                            db.SaveChanges();

                            result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.HandlingMethod, Resources.Resource.Success));
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.HandlingMethodID, Resources.Resource.Exists));
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
                using (TDbEntities db = new TDbEntities())
                {
                    var handlingMethod = db.HandlingMethod.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = handlingMethod.UniqueID,
                        OrganizationUniqueID = handlingMethod.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(handlingMethod.OrganizationUniqueID),
                        FormInput = new FormInput()
                        {
                            HandlingMethodType = handlingMethod.HandlingMethodType,
                            ID = handlingMethod.ID,
                            Description = handlingMethod.Description
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
                using (TDbEntities db = new TDbEntities())
                {
                    var handlingMethod = db.HandlingMethod.First(x => x.UniqueID == Model.UniqueID);

                    var exists = db.HandlingMethod.FirstOrDefault(x => x.UniqueID != handlingMethod.UniqueID && x.OrganizationUniqueID == handlingMethod.OrganizationUniqueID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                        handlingMethod.ID = Model.FormInput.ID;
                        handlingMethod.Description = Model.FormInput.Description;
                        handlingMethod.LastModifyTime = DateTime.Now;

                        db.SaveChanges();
#if !DEBUG
                        trans.Complete();
                    }
#endif
                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.HandlingMethod, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.HandlingMethodID, Resources.Resource.Exists));
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

        public static RequestResult Delete(List<string> SelectedList)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (TDbEntities db = new TDbEntities())
                {
                    DeleteHelper.HandlingMethod(db, SelectedList);

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.HandlingMethod, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetTreeItem(string OrganizationUniqueID, string HandlingMethodType, Account Account)
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
                    { Define.EnumTreeAttribute.HandlingMethodType, string.Empty },
                    { Define.EnumTreeAttribute.HandlingMethodUniqueID, string.Empty }
                };

                using (TDbEntities tdb = new TDbEntities())
                {
                    if (string.IsNullOrEmpty(HandlingMethodType))
                    {
                        if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                        {
                            var handlingMethodTypeList = tdb.HandlingMethod.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).Select(x => x.HandlingMethodType).Distinct().OrderBy(x => x).ToList();

                            foreach (var handlingMethodType in handlingMethodTypeList)
                            {
                                var treeItem = new TreeItem() { Title = handlingMethodType };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.HandlingMethodType.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = handlingMethodType;
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                                attributes[Define.EnumTreeAttribute.HandlingMethodType] = handlingMethodType;
                                attributes[Define.EnumTreeAttribute.HandlingMethodUniqueID] = string.Empty;

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
                                attributes[Define.EnumTreeAttribute.HandlingMethodType] = string.Empty;
                                attributes[Define.EnumTreeAttribute.HandlingMethodUniqueID] = string.Empty;

                                foreach (var attribute in attributes)
                                {
                                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                }

                                if (db.Organization.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID)) 
                                    ||
                                    (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && tdb.HandlingMethod.Any(x => x.OrganizationUniqueID == organization.UniqueID)))
                                {
                                    treeItem.State = "closed";
                                }

                                treeItemList.Add(treeItem);
                            }
                        }
                    }
                    else
                    {
                        var handlingMethodList = tdb.HandlingMethod.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && x.HandlingMethodType == HandlingMethodType).OrderBy(x => x.ID).ToList();

                        foreach (var handlingMethod in handlingMethodList)
                        {
                            var treeItem = new TreeItem() { Title = handlingMethod.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.HandlingMethod.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", handlingMethod.ID, handlingMethod.Description);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.HandlingMethodType] = handlingMethod.HandlingMethodType;
                            attributes[Define.EnumTreeAttribute.HandlingMethodUniqueID] = handlingMethod.UniqueID;

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

        public static RequestResult GetTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID, string HandlingMethodType)
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
                    { Define.EnumTreeAttribute.HandlingMethodType, string.Empty },
                    { Define.EnumTreeAttribute.HandlingMethodUniqueID, string.Empty }
                };

                using (TDbEntities tdb = new TDbEntities())
                {
                    if (string.IsNullOrEmpty(HandlingMethodType))
                    {
                        var handlingMethodTypeList = tdb.HandlingMethod.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).Select(x => x.HandlingMethodType).Distinct().OrderBy(x => x).ToList();

                        foreach (var handlingMethodType in handlingMethodTypeList)
                        {
                            var treeItem = new TreeItem() { Title = handlingMethodType };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.HandlingMethodType.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = handlingMethodType;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.HandlingMethodType] = handlingMethodType;
                            attributes[Define.EnumTreeAttribute.HandlingMethodUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItem.State = "closed";

                            treeItemList.Add(treeItem);
                        }

                        using (DbEntities db = new DbEntities())
                        {
                            var availableOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(RefOrganizationUniqueID, true);

                            var organizationList = db.Organization.Where(x => x.ParentUniqueID == OrganizationUniqueID && availableOrganizationList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                            foreach (var organization in organizationList)
                            {
                                var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organization.UniqueID, true);

                                if (tdb.HandlingMethod.Any(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && availableOrganizationList.Contains(x.OrganizationUniqueID)))
                                {
                                    var treeItem = new TreeItem() { Title = organization.Description };

                                    attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                                    attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                                    attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                                    attributes[Define.EnumTreeAttribute.HandlingMethodType] = string.Empty;
                                    attributes[Define.EnumTreeAttribute.HandlingMethodUniqueID] = string.Empty;

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
                        var handlingMethodList = tdb.HandlingMethod.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && x.HandlingMethodType == HandlingMethodType).OrderBy(x => x.ID).ToList();

                        foreach (var handlingMethod in handlingMethodList)
                        {
                            var treeItem = new TreeItem() { Title = handlingMethod.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.HandlingMethod.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", handlingMethod.ID, handlingMethod.Description);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = handlingMethod.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.HandlingMethodType] = handlingMethod.HandlingMethodType;
                            attributes[Define.EnumTreeAttribute.HandlingMethodUniqueID] = handlingMethod.UniqueID;

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
