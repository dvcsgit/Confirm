using System;
using System.Linq;
using System.Web.Mvc;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.ASE;
using Models.Shared;
using Models.Authenticated;
using Models.EquipmentMaintenance.HandlingMethodManagement;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.ASE
{
    public class HandlingMethodDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.HANDLINGMETHOD.Where(x => downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID) && Account.QueryableOrganizationUniqueIDList.Contains(x.ORGANIZATIONUNIQUEID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.HandlingMethodType))
                    {
                        query = query.Where(x => x.ORGANIZATIONUNIQUEID == Parameters.OrganizationUniqueID && x.HANDLINGMETHODTYPE == Parameters.HandlingMethodType);
                    }

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.ID.Contains(Parameters.Keyword) || x.DESCRIPTION.Contains(Parameters.Keyword));
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
                            UniqueID = x.UNIQUEID,
                            Permission = Account.OrganizationPermission(x.ORGANIZATIONUNIQUEID),
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.ORGANIZATIONUNIQUEID),
                            HandlingMethodType = x.HANDLINGMETHODTYPE,
                            ID = x.ID,
                            Description = x.DESCRIPTION
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var handlingMethod = db.HANDLINGMETHOD.First(x => x.UNIQUEID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = handlingMethod.UNIQUEID,
                        Permission = Account.OrganizationPermission(handlingMethod.ORGANIZATIONUNIQUEID),
                        OrganizationUniqueID = handlingMethod.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(handlingMethod.ORGANIZATIONUNIQUEID),
                        HandlingMethodType = handlingMethod.HANDLINGMETHODTYPE,
                        ID = handlingMethod.ID,
                        Description = handlingMethod.DESCRIPTION
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
                using (ASEDbEntities db = new ASEDbEntities())
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

                    model.HandlingMethodTypeSelectItemList.AddRange(db.HANDLINGMETHOD.Where(x => upStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)).Select(x => x.HANDLINGMETHODTYPE).Distinct().OrderBy(x => x).Select(x => new SelectListItem
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
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var exists = db.HANDLINGMETHOD.FirstOrDefault(x => x.ORGANIZATIONUNIQUEID == Model.OrganizationUniqueID && x.HANDLINGMETHODTYPE == Model.FormInput.HandlingMethodType && x.ID == Model.FormInput.ID);

                        if (exists == null)
                        {
                            string uniqueID = Guid.NewGuid().ToString();

                            db.HANDLINGMETHOD.Add(new HANDLINGMETHOD()
                            {
                                UNIQUEID = uniqueID,
                                ORGANIZATIONUNIQUEID = Model.OrganizationUniqueID,
                                HANDLINGMETHODTYPE = Model.FormInput.HandlingMethodType,
                                ID = Model.FormInput.ID,
                                DESCRIPTION = Model.FormInput.Description,
                                LASTMODIFYTIME = DateTime.Now
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var handlingMethod = db.HANDLINGMETHOD.First(x => x.UNIQUEID == UniqueID);

                    var model = new EditFormModel()
                    {
                        UniqueID = handlingMethod.UNIQUEID,
                        OrganizationUniqueID = handlingMethod.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(handlingMethod.ORGANIZATIONUNIQUEID),
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
                            HandlingMethodType = handlingMethod.HANDLINGMETHODTYPE,
                            ID = handlingMethod.ID,
                            Description = handlingMethod.DESCRIPTION
                        }
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(model.OrganizationUniqueID, true);

                    model.HandlingMethodTypeSelectItemList.AddRange(db.HANDLINGMETHOD.Where(x => upStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)).Select(x => x.HANDLINGMETHODTYPE).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    if (!string.IsNullOrEmpty(model.FormInput.HandlingMethodType) && model.HandlingMethodTypeSelectItemList.Any(x => x.Value == model.FormInput.HandlingMethodType))
                    {
                        model.HandlingMethodTypeSelectItemList.First(x => x.Value == model.FormInput.HandlingMethodType).Selected = true;
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
                if (Model.FormInput.HandlingMethodType == Define.OTHER || Model.FormInput.HandlingMethodType == Define.NEW)
                {
                    result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.Unsupported, Resources.Resource.HandlingMethodType));
                }
                else
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var handlingMethod = db.HANDLINGMETHOD.First(x => x.UNIQUEID == Model.UniqueID);

                        var exists = db.HANDLINGMETHOD.FirstOrDefault(x => x.UNIQUEID != handlingMethod.UNIQUEID && x.ORGANIZATIONUNIQUEID == handlingMethod.ORGANIZATIONUNIQUEID && x.HANDLINGMETHODTYPE == Model.FormInput.HandlingMethodType && x.ID == Model.FormInput.ID);

                        if (exists == null)
                        {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                            handlingMethod.HANDLINGMETHODTYPE = Model.FormInput.HandlingMethodType;
                            handlingMethod.ID = Model.FormInput.ID;
                            handlingMethod.DESCRIPTION = Model.FormInput.Description;
                            handlingMethod.LASTMODIFYTIME = DateTime.Now;

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
                using (ASEDbEntities db = new ASEDbEntities())
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

        public static RequestResult GetTreeItem(List<Models.Shared.Organization> OrganizationList, string OrganizationUniqueID, string HandlingMethodType, Account Account)
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (string.IsNullOrEmpty(HandlingMethodType))
                    {
                        if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                        {
                            var handlingMethodTypeList = db.HANDLINGMETHOD.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).Select(x => x.HANDLINGMETHODTYPE).Distinct().OrderBy(x => x).ToList();

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

                        var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

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

                            if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                                ||
                                (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.HANDLINGMETHOD.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID)))
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else
                    {
                        var handlingMethodList = db.HANDLINGMETHOD.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID && x.HANDLINGMETHODTYPE == HandlingMethodType).OrderBy(x => x.ID).ToList();

                        foreach (var handlingMethod in handlingMethodList)
                        {
                            var treeItem = new TreeItem() { Title = handlingMethod.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.HandlingMethod.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", handlingMethod.ID, handlingMethod.DESCRIPTION);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.HandlingMethodType] = handlingMethod.HANDLINGMETHODTYPE;
                            attributes[Define.EnumTreeAttribute.HandlingMethodUniqueID] = handlingMethod.UNIQUEID;

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
                    { Define.EnumTreeAttribute.HandlingMethodType, string.Empty },
                    { Define.EnumTreeAttribute.HandlingMethodUniqueID, string.Empty }
                };

                var organization = OrganizationList.First(x => x.UniqueID == OrganizationUniqueID);

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

                using (ASEDbEntities db = new ASEDbEntities()) {
                    if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                       ||
                       (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.HANDLINGMETHOD.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID)))
                    {
                        treeItem.State = "closed";
                    }
                }
               

                treeItemList.Add(treeItem);

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

        public static RequestResult GetTreeItem(List<Models.Shared.Organization> OrganizationList, string RefOrganizationUniqueID, string OrganizationUniqueID, string HandlingMethodType)
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (string.IsNullOrEmpty(HandlingMethodType))
                    {
                        var handlingMethodTypeList = db.HANDLINGMETHOD.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).Select(x => x.HANDLINGMETHODTYPE).Distinct().OrderBy(x => x).ToList();

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

                        var availableOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(RefOrganizationUniqueID, true);

                        var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && availableOrganizationList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                        foreach (var organization in organizationList)
                        {
                            var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organization.UniqueID, true);

                            if (db.HANDLINGMETHOD.Any(x => downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID) && availableOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)))
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
                    else
                    {
                        var handlingMethodList = db.HANDLINGMETHOD.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID && x.HANDLINGMETHODTYPE == HandlingMethodType).OrderBy(x => x.ID).ToList();

                        foreach (var handlingMethod in handlingMethodList)
                        {
                            var treeItem = new TreeItem() { Title = handlingMethod.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.HandlingMethod.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", handlingMethod.ID, handlingMethod.DESCRIPTION);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = handlingMethod.ORGANIZATIONUNIQUEID;
                            attributes[Define.EnumTreeAttribute.HandlingMethodType] = handlingMethod.HANDLINGMETHODTYPE;
                            attributes[Define.EnumTreeAttribute.HandlingMethodUniqueID] = handlingMethod.UNIQUEID;

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
