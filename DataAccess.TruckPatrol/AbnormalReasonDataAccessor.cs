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
using Models.TruckPatrol.AbnormalReasonManagement;

namespace DataAccess.TruckPatrol
{
    public class AbnormalReasonDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (TDbEntities db = new TDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.AbnormalReason.Where(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.AbnormalType))
                    {
                        query = query.Where(x => x.OrganizationUniqueID == Parameters.OrganizationUniqueID && x.AbnormalType == Parameters.AbnormalType);
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
                        AbnormalType = Parameters.AbnormalType,
                        OrganizationDescription = organization.Description,
                        FullOrganizationDescription = organization.FullDescription,
                        ItemList = query.ToList().Select(x => new GridItem()
                        {
                            UniqueID = x.UniqueID,
                            Permission = Account.OrganizationPermission(x.OrganizationUniqueID),
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.OrganizationUniqueID),
                            AbnormalType = x.AbnormalType,
                            ID = x.ID,
                            Description = x.Description
                        }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.AbnormalType).ThenBy(x => x.ID).ToList()
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
                    var abnormalReason = db.AbnormalReason.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = abnormalReason.UniqueID,
                        Permission = Account.OrganizationPermission(abnormalReason.OrganizationUniqueID),
                        OrganizationUniqueID = abnormalReason.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(abnormalReason.OrganizationUniqueID),
                        AbnormalType = abnormalReason.AbnormalType,
                        ID = abnormalReason.ID,
                        Description = abnormalReason.Description,
                        HandlingMethodDescriptionList = (from x in db.AbnormalReasonHandlingMethod
                                                         join h in db.HandlingMethod
                                                         on x.HandlingMethodUniqueID equals h.UniqueID
                                                         where x.AbnormalReasonUniqueID == UniqueID
                                                         orderby h.HandlingMethodType, h.ID
                                                         select h.Description).ToList()
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

        public static RequestResult GetCreateFormModel(string OrganizationUniqueID, string AbnormalType)
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
                        AbnormalTypeSelectItemList = new List<SelectListItem>() 
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
                            AbnormalType = AbnormalType
                        }
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(OrganizationUniqueID, true);

                    model.AbnormalTypeSelectItemList.AddRange(db.AbnormalReason.Where(x => upStreamOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.AbnormalType).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    if (!string.IsNullOrEmpty(AbnormalType) && model.AbnormalTypeSelectItemList.Any(x => x.Value == AbnormalType))
                    {
                        model.AbnormalTypeSelectItemList.First(x => x.Value == AbnormalType).Selected = true;
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

        public static RequestResult GetCopyFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (TDbEntities db = new TDbEntities())
                {
                    var abnormalReason = db.AbnormalReason.First(x => x.UniqueID == UniqueID);

                    var model = new CreateFormModel()
                    {
                        OrganizationUniqueID = abnormalReason.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(abnormalReason.OrganizationUniqueID),
                        AbnormalTypeSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne),
                            new SelectListItem()
                            {
                                Text = Resources.Resource.Create + "...",
                                Value = Define.NEW
                            }
                        },
                        HandlingMethodList = (from x in db.AbnormalReasonHandlingMethod
                                              join h in db.HandlingMethod
                                              on x.HandlingMethodUniqueID equals h.UniqueID
                                              where x.AbnormalReasonUniqueID == UniqueID
                                              select new HandlingMethodModel
                                              {
                                                  UniqueID = h.UniqueID,
                                                  HandlingMethodType = h.HandlingMethodType,
                                                  ID = h.ID,
                                                  Description = h.Description
                                              }).OrderBy(x => x.HandlingMethodType).ThenBy(x => x.ID).ToList(),
                        FormInput = new FormInput()
                        {
                            AbnormalType = abnormalReason.AbnormalType
                        }
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(abnormalReason.OrganizationUniqueID, true);

                    model.AbnormalTypeSelectItemList.AddRange(db.AbnormalReason.Where(x => upStreamOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.AbnormalType).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    model.AbnormalTypeSelectItemList.First(x => x.Value == abnormalReason.AbnormalType).Selected = true;

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
                if (Model.FormInput.AbnormalType == Define.OTHER || Model.FormInput.AbnormalType == Define.NEW)
                {
                    result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.Unsupported, Resources.Resource.AbnormalType));
                }
                else
                {
                    using (TDbEntities db = new TDbEntities())
                    {
                        var exists = db.AbnormalReason.FirstOrDefault(x => x.OrganizationUniqueID == Model.OrganizationUniqueID && x.AbnormalType == Model.FormInput.AbnormalType && x.ID == Model.FormInput.ID);

                        if (exists == null)
                        {
                            string uniqueID = Guid.NewGuid().ToString();

                            db.AbnormalReason.Add(new AbnormalReason()
                            {
                                UniqueID = uniqueID,
                                OrganizationUniqueID = Model.OrganizationUniqueID,
                                AbnormalType = Model.FormInput.AbnormalType,
                                ID = Model.FormInput.ID,
                                Description = Model.FormInput.Description,
                                LastModifyTime = DateTime.Now
                            });

#if ORACLE
                            db.AbnormalReasonHandling.AddRange(Model.HandlingMethodList.Select(x => new AbnormalReasonHandling
                            {
                                AbnormalReasonUniqueID = uniqueID,
                                HandlingMethodUniqueID = x.UniqueID
                            }).ToList());
#else
                            db.AbnormalReasonHandlingMethod.AddRange(Model.HandlingMethodList.Select(x => new AbnormalReasonHandlingMethod
                            {
                                AbnormalReasonUniqueID = uniqueID,
                                HandlingMethodUniqueID = x.UniqueID
                            }).ToList());
#endif
                            db.SaveChanges();

                            result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.AbnormalReason, Resources.Resource.Success));
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.AbnormalReasonID, Resources.Resource.Exists));
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
                    var abnormalReason = db.AbnormalReason.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = abnormalReason.UniqueID,
                        OrganizationUniqueID = abnormalReason.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(abnormalReason.OrganizationUniqueID),
                        HandlingMethodList = (from x in db.AbnormalReasonHandlingMethod
                                              join h in db.HandlingMethod
                                              on x.HandlingMethodUniqueID equals h.UniqueID
                                              where x.AbnormalReasonUniqueID == UniqueID
                                              select new HandlingMethodModel
                                              {
                                                  UniqueID = h.UniqueID,
                                                  HandlingMethodType = h.HandlingMethodType,
                                                  ID = h.ID,
                                                  Description = h.Description
                                              }).OrderBy(x => x.HandlingMethodType).ThenBy(x => x.ID).ToList(),
                        FormInput = new FormInput()
                        {
                            AbnormalType = abnormalReason.AbnormalType,
                            ID = abnormalReason.ID,
                            Description = abnormalReason.Description
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
                    var abnormalReason = db.AbnormalReason.First(x => x.UniqueID == Model.UniqueID);

                    var exists = db.AbnormalReason.FirstOrDefault(x => x.UniqueID != abnormalReason.UniqueID && x.OrganizationUniqueID == abnormalReason.OrganizationUniqueID && x.AbnormalType == abnormalReason.AbnormalType && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                        #region AbnormalReason
                        abnormalReason.Description = Model.FormInput.Description;
                        abnormalReason.LastModifyTime = DateTime.Now;

                        db.SaveChanges();
                        #endregion

                        #region AbnormalReasonHandlingMethod
                        #if ORACLE
                        #region Delete
                        db.AbnormalReasonHandling.RemoveRange(db.AbnormalReasonHandling.Where(x => x.AbnormalReasonUniqueID == Model.UniqueID).ToList());

                        db.SaveChanges();
                        #endregion

                        #region Insert
                        db.AbnormalReasonHandling.AddRange(Model.HandlingMethodList.Select(x => new AbnormalReasonHandling
                        {
                            AbnormalReasonUniqueID = abnormalReason.UniqueID,
                            HandlingMethodUniqueID = x.UniqueID
                        }).ToList());

                        db.SaveChanges();
                        #endregion
#else
                        #region Delete
                        db.AbnormalReasonHandlingMethod.RemoveRange(db.AbnormalReasonHandlingMethod.Where(x => x.AbnormalReasonUniqueID == Model.UniqueID).ToList());

                        db.SaveChanges();
                        #endregion

                        #region Insert
                        db.AbnormalReasonHandlingMethod.AddRange(Model.HandlingMethodList.Select(x => new AbnormalReasonHandlingMethod
                        {
                            AbnormalReasonUniqueID = abnormalReason.UniqueID,
                            HandlingMethodUniqueID = x.UniqueID
                        }).ToList());

                        db.SaveChanges();
                        #endregion
#endif
                        #endregion
#if !DEBUG
                        trans.Complete();
                    }
#endif
                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.AbnormalReason, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.AbnormalReasonID, Resources.Resource.Exists));
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
#if !DEBUG
                using (TransactionScope trans = new TransactionScope())
                {
#endif
                    using (TDbEntities db = new TDbEntities())
                    {
                        DeleteHelper.AbnormalReason(db, SelectedList);

                        db.SaveChanges();
                    }

#if !DEBUG
                    trans.Complete();
                }
#endif
                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.AbnormalReason, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult AddHandlingMethod(List<HandlingMethodModel> HandlingMethodList, List<string> SelectedList, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (TDbEntities db = new TDbEntities())
                {
                    foreach (string selected in SelectedList)
                    {
                        string[] temp = selected.Split(Define.Seperators, StringSplitOptions.None);

                        var organizationUniqueID = temp[0];
                        var handlingMethodType = temp[1];
                        var handlingMethodUniqueID = temp[2];

                        if (!string.IsNullOrEmpty(handlingMethodUniqueID))
                        {
                            if (!HandlingMethodList.Any(x => x.UniqueID == handlingMethodUniqueID))
                            {
                                var handlingMethod = db.HandlingMethod.First(x => x.UniqueID == handlingMethodUniqueID);

                                HandlingMethodList.Add(new HandlingMethodModel()
                                {
                                    UniqueID = handlingMethod.UniqueID,
                                    HandlingMethodType = handlingMethod.HandlingMethodType,
                                    ID = handlingMethod.ID,
                                    Description = handlingMethod.Description
                                });
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(handlingMethodType))
                            {
                                var handlingMethodList = db.HandlingMethod.Where(x => x.OrganizationUniqueID == organizationUniqueID && x.HandlingMethodType == handlingMethodType).ToList();

                                foreach (var handlingMethod in handlingMethodList)
                                {
                                    if (!HandlingMethodList.Any(x => x.UniqueID == handlingMethod.UniqueID))
                                    {
                                        HandlingMethodList.Add(new HandlingMethodModel()
                                        {
                                            UniqueID = handlingMethod.UniqueID,
                                            HandlingMethodType = handlingMethod.HandlingMethodType,
                                            ID = handlingMethod.ID,
                                            Description = handlingMethod.Description
                                        });
                                    }
                                }
                            }
                            else
                            {
                                var availableOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(RefOrganizationUniqueID, true);

                                var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organizationUniqueID, true);

                                foreach (var downStreamOrganization in downStreamOrganizationList)
                                {
                                    if (availableOrganizationList.Any(x => x == downStreamOrganization))
                                    {
                                        var handlingMethodList = db.HandlingMethod.Where(x => x.OrganizationUniqueID == downStreamOrganization).ToList();

                                        foreach (var handlingMethod in handlingMethodList)
                                        {
                                            if (!HandlingMethodList.Any(x => x.UniqueID == handlingMethod.UniqueID))
                                            {
                                                HandlingMethodList.Add(new HandlingMethodModel()
                                                {
                                                    UniqueID = handlingMethod.UniqueID,
                                                    HandlingMethodType = handlingMethod.HandlingMethodType,
                                                    ID = handlingMethod.ID,
                                                    Description = handlingMethod.Description
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                result.ReturnData(HandlingMethodList.OrderBy(x => x.HandlingMethodType).ThenBy(x => x.ID).ToList());
            }
            catch (Exception ex)
            {
                var err = new Error(MethodInfo.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetTreeItem(string OrganizationUniqueID, string AbnormalType, Account Account)
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
                    { Define.EnumTreeAttribute.AbnormalType, string.Empty },
                    { Define.EnumTreeAttribute.AbnormalReasonUniqueID, string.Empty }
                };

                using (TDbEntities tdb = new TDbEntities())
                {
                    if (string.IsNullOrEmpty(AbnormalType))
                    {
                        if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                        {
                            var abnormalReasonTypeList = tdb.AbnormalReason.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).Select(x => x.AbnormalType).Distinct().OrderBy(x => x).ToList();

                            foreach (var abnormalReasonType in abnormalReasonTypeList)
                            {
                                var treeItem = new TreeItem() { Title = abnormalReasonType };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.AbnormalType.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = abnormalReasonType;
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                                attributes[Define.EnumTreeAttribute.AbnormalType] = abnormalReasonType;
                                attributes[Define.EnumTreeAttribute.AbnormalReasonUniqueID] = string.Empty;

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
                                attributes[Define.EnumTreeAttribute.AbnormalType] = string.Empty;
                                attributes[Define.EnumTreeAttribute.AbnormalReasonUniqueID] = string.Empty;

                                foreach (var attribute in attributes)
                                {
                                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                }

                                if (db.Organization.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                                    ||
                                    (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && tdb.AbnormalReason.Any(x => x.OrganizationUniqueID == organization.UniqueID)))
                                {
                                    treeItem.State = "closed";
                                }

                                treeItemList.Add(treeItem);
                            }   
                        }
                    }
                    else
                    {
                        var abnormalReasonList = tdb.AbnormalReason.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && x.AbnormalType == AbnormalType).OrderBy(x => x.ID).ToList();

                        foreach (var abnormalReason in abnormalReasonList)
                        {
                            var treeItem = new TreeItem() { Title = abnormalReason.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.AbnormalReason.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", abnormalReason.ID, abnormalReason.Description);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.AbnormalType] = abnormalReason.AbnormalType;
                            attributes[Define.EnumTreeAttribute.AbnormalReasonUniqueID] = abnormalReason.UniqueID;

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

        public static RequestResult GetTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID, string AbnormalType)
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
                    { Define.EnumTreeAttribute.AbnormalType, string.Empty },
                    { Define.EnumTreeAttribute.AbnormalReasonUniqueID, string.Empty }
                };

                using (TDbEntities tdb = new TDbEntities())
                {
                    if (string.IsNullOrEmpty(AbnormalType))
                    {
                        var abnormalReasonTypeList = tdb.AbnormalReason.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).Select(x => x.AbnormalType).Distinct().OrderBy(x => x).ToList();

                        foreach (var abnormalReasonType in abnormalReasonTypeList)
                        {
                            var treeItem = new TreeItem() { Title = abnormalReasonType };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.AbnormalType.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = abnormalReasonType;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.AbnormalType] = abnormalReasonType;
                            attributes[Define.EnumTreeAttribute.AbnormalReasonUniqueID] = string.Empty;

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

                                if (tdb.AbnormalReason.Any(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && availableOrganizationList.Contains(x.OrganizationUniqueID)))
                                {
                                    var treeItem = new TreeItem() { Title = organization.Description };

                                    attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                                    attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                                    attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                                    attributes[Define.EnumTreeAttribute.AbnormalType] = string.Empty;
                                    attributes[Define.EnumTreeAttribute.AbnormalReasonUniqueID] = string.Empty;

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
                        var abnormalReasonList = tdb.AbnormalReason.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && x.AbnormalType == AbnormalType).OrderBy(x => x.ID).ToList();

                        foreach (var abnormalReason in abnormalReasonList)
                        {
                            var treeItem = new TreeItem() { Title = abnormalReason.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.AbnormalReason.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", abnormalReason.ID, abnormalReason.Description);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = abnormalReason.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.AbnormalType] = abnormalReason.AbnormalType;
                            attributes[Define.EnumTreeAttribute.AbnormalReasonUniqueID] = abnormalReason.UniqueID;

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
