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
using Models.EquipmentMaintenance.AbnormalReasonManagement;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.ASE
{
    public class AbnormalReasonDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.ABNORMALREASON.Where(x => downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID) && Account.QueryableOrganizationUniqueIDList.Contains(x.ORGANIZATIONUNIQUEID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.AbnormalType))
                    {
                        query = query.Where(x => x.ORGANIZATIONUNIQUEID == Parameters.OrganizationUniqueID && x.ABNORMALTYPE == Parameters.AbnormalType);
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
                        AbnormalType = Parameters.AbnormalType,
                        OrganizationDescription = organization.Description,
                        FullOrganizationDescription = organization.FullDescription,
                        ItemList = query.ToList().Select(x => new GridItem()
                        {
                            UniqueID = x.UNIQUEID,
                            Permission = Account.OrganizationPermission(x.ORGANIZATIONUNIQUEID),
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.ORGANIZATIONUNIQUEID),
                            AbnormalType = x.ABNORMALTYPE,
                            ID = x.ID,
                            Description = x.DESCRIPTION
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var abnormalReason = db.ABNORMALREASON.First(x => x.UNIQUEID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = abnormalReason.UNIQUEID,
                        Permission = Account.OrganizationPermission(abnormalReason.ORGANIZATIONUNIQUEID),
                        OrganizationUniqueID = abnormalReason.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(abnormalReason.ORGANIZATIONUNIQUEID),
                        AbnormalType = abnormalReason.ABNORMALTYPE,
                        ID = abnormalReason.ID,
                        Description = abnormalReason.DESCRIPTION,
                        HandlingMethodDescriptionList = (from x in db.ABNORMALREASONHANDLINGMETHOD
                                                         join h in db.HANDLINGMETHOD
                                                         on x.HANDLINGMETHODUNIQUEID equals h.UNIQUEID
                                                         where x.ABNORMALREASONUNIQUEID == UniqueID
                                                         orderby h.HANDLINGMETHODTYPE, h.ID
                                                         select h.DESCRIPTION).ToList()
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
                using (ASEDbEntities db = new ASEDbEntities())
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

                    model.AbnormalTypeSelectItemList.AddRange(db.ABNORMALREASON.Where(x => upStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)).Select(x => x.ABNORMALTYPE).Distinct().OrderBy(x => x).Select(x => new SelectListItem
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var abnormalReason = db.ABNORMALREASON.First(x => x.UNIQUEID == UniqueID);

                    var model = new CreateFormModel()
                    {
                        OrganizationUniqueID = abnormalReason.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(abnormalReason.ORGANIZATIONUNIQUEID),
                        AbnormalTypeSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne),
                            new SelectListItem()
                            {
                                Text = Resources.Resource.Create + "...",
                                Value = Define.NEW
                            }
                        },
                        HandlingMethodList = (from x in db.ABNORMALREASONHANDLINGMETHOD
                                              join h in db.HANDLINGMETHOD
                                              on x.HANDLINGMETHODUNIQUEID equals h.UNIQUEID
                                              where x.ABNORMALREASONUNIQUEID == UniqueID
                                              select new HandlingMethodModel
                                              {
                                                  UniqueID = h.UNIQUEID,
                                                  HandlingMethodType = h.HANDLINGMETHODTYPE,
                                                  ID = h.ID,
                                                  Description = h.DESCRIPTION
                                              }).OrderBy(x => x.HandlingMethodType).ThenBy(x => x.ID).ToList(),
                        FormInput = new FormInput()
                        {
                            AbnormalType = abnormalReason.ABNORMALTYPE
                        }
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(abnormalReason.ORGANIZATIONUNIQUEID, true);

                    model.AbnormalTypeSelectItemList.AddRange(db.ABNORMALREASON.Where(x => upStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)).Select(x => x.ABNORMALTYPE).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    model.AbnormalTypeSelectItemList.First(x => x.Value == abnormalReason.ABNORMALTYPE).Selected = true;

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
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var exists = db.ABNORMALREASON.FirstOrDefault(x => x.ORGANIZATIONUNIQUEID == Model.OrganizationUniqueID && x.ABNORMALTYPE == Model.FormInput.AbnormalType && x.ID == Model.FormInput.ID);

                        if (exists == null)
                        {
                            string uniqueID = Guid.NewGuid().ToString();

                            db.ABNORMALREASON.Add(new ABNORMALREASON()
                            {
                                UNIQUEID = uniqueID,
                                ORGANIZATIONUNIQUEID = Model.OrganizationUniqueID,
                                ABNORMALTYPE = Model.FormInput.AbnormalType,
                                ID = Model.FormInput.ID,
                                DESCRIPTION = Model.FormInput.Description,
                                LASTMODIFYTIME = DateTime.Now
                            });

                            db.ABNORMALREASONHANDLINGMETHOD.AddRange(Model.HandlingMethodList.Select(x => new ABNORMALREASONHANDLINGMETHOD
                            {
                                ABNORMALREASONUNIQUEID = uniqueID,
                                HANDLINGMETHODUNIQUEID = x.UniqueID
                            }).ToList());
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var abnormalReason = db.ABNORMALREASON.First(x => x.UNIQUEID == UniqueID);

                    var model = new EditFormModel()
                    {
                        UniqueID = abnormalReason.UNIQUEID,
                        OrganizationUniqueID = abnormalReason.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(abnormalReason.ORGANIZATIONUNIQUEID),
                        AbnormalTypeSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne),
                            new SelectListItem()
                            {
                                Text = Resources.Resource.Create + "...",
                                Value = Define.NEW
                            }
                        },
                        HandlingMethodList = (from x in db.ABNORMALREASONHANDLINGMETHOD
                                              join h in db.HANDLINGMETHOD
                                              on x.HANDLINGMETHODUNIQUEID equals h.UNIQUEID
                                              where x.ABNORMALREASONUNIQUEID == UniqueID
                                              select new HandlingMethodModel
                                              {
                                                  UniqueID = h.UNIQUEID,
                                                  HandlingMethodType = h.HANDLINGMETHODTYPE,
                                                  ID = h.ID,
                                                  Description = h.DESCRIPTION
                                              }).OrderBy(x => x.HandlingMethodType).ThenBy(x => x.ID).ToList(),
                        FormInput = new FormInput()
                        {
                            AbnormalType = abnormalReason.ABNORMALTYPE,
                            ID = abnormalReason.ID,
                            Description = abnormalReason.DESCRIPTION
                        }
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(model.OrganizationUniqueID, true);

                    model.AbnormalTypeSelectItemList.AddRange(db.ABNORMALREASON.Where(x => upStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)).Select(x => x.ABNORMALTYPE).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    if (!string.IsNullOrEmpty(model.FormInput.AbnormalType) && model.AbnormalTypeSelectItemList.Any(x => x.Value == model.FormInput.AbnormalType))
                    {
                        model.AbnormalTypeSelectItemList.First(x => x.Value == model.FormInput.AbnormalType).Selected = true;
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
                if (Model.FormInput.AbnormalType == Define.OTHER || Model.FormInput.AbnormalType == Define.NEW)
                {
                    result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.Unsupported, Resources.Resource.AbnormalType));
                }
                else
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var abnormalReason = db.ABNORMALREASON.First(x => x.UNIQUEID == Model.UniqueID);

                        var exists = db.ABNORMALREASON.FirstOrDefault(x => x.UNIQUEID != abnormalReason.UNIQUEID && x.ORGANIZATIONUNIQUEID == abnormalReason.ORGANIZATIONUNIQUEID && x.ABNORMALTYPE == Model.FormInput.AbnormalType && x.ID == Model.FormInput.ID);

                        if (exists == null)
                        {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                            #region AbnormalReason
                            abnormalReason.ABNORMALTYPE = Model.FormInput.AbnormalType;
                            abnormalReason.ID = Model.FormInput.ID;
                            abnormalReason.DESCRIPTION = Model.FormInput.Description;
                            abnormalReason.LASTMODIFYTIME = DateTime.Now;

                            db.SaveChanges();
                            #endregion

                            #region AbnormalReasonHandlingMethod
                            #region Delete
                            db.ABNORMALREASONHANDLINGMETHOD.RemoveRange(db.ABNORMALREASONHANDLINGMETHOD.Where(x => x.ABNORMALREASONUNIQUEID == Model.UniqueID).ToList());

                            db.SaveChanges();
                            #endregion

                            #region Insert
                            db.ABNORMALREASONHANDLINGMETHOD.AddRange(Model.HandlingMethodList.Select(x => new ABNORMALREASONHANDLINGMETHOD
                            {
                                ABNORMALREASONUNIQUEID = abnormalReason.UNIQUEID,
                                HANDLINGMETHODUNIQUEID = x.UniqueID
                            }).ToList());

                            db.SaveChanges();
                            #endregion
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
                    using (ASEDbEntities db = new ASEDbEntities())
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
                using (ASEDbEntities db = new ASEDbEntities())
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
                                var handlingMethod = db.HANDLINGMETHOD.First(x => x.UNIQUEID == handlingMethodUniqueID);

                                HandlingMethodList.Add(new HandlingMethodModel()
                                {
                                    UniqueID = handlingMethod.UNIQUEID,
                                    HandlingMethodType = handlingMethod.HANDLINGMETHODTYPE,
                                    ID = handlingMethod.ID,
                                    Description = handlingMethod.DESCRIPTION
                                });
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(handlingMethodType))
                            {
                                var handlingMethodList = db.HANDLINGMETHOD.Where(x => x.ORGANIZATIONUNIQUEID == organizationUniqueID && x.HANDLINGMETHODTYPE == handlingMethodType).ToList();

                                foreach (var handlingMethod in handlingMethodList)
                                {
                                    if (!HandlingMethodList.Any(x => x.UniqueID == handlingMethod.UNIQUEID))
                                    {
                                        HandlingMethodList.Add(new HandlingMethodModel()
                                        {
                                            UniqueID = handlingMethod.UNIQUEID,
                                            HandlingMethodType = handlingMethod.HANDLINGMETHODTYPE,
                                            ID = handlingMethod.ID,
                                            Description = handlingMethod.DESCRIPTION
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
                                        var handlingMethodList = db.HANDLINGMETHOD.Where(x => x.ORGANIZATIONUNIQUEID == downStreamOrganization).ToList();

                                        foreach (var handlingMethod in handlingMethodList)
                                        {
                                            if (!HandlingMethodList.Any(x => x.UniqueID == handlingMethod.UNIQUEID))
                                            {
                                                HandlingMethodList.Add(new HandlingMethodModel()
                                                {
                                                    UniqueID = handlingMethod.UNIQUEID,
                                                    HandlingMethodType = handlingMethod.HANDLINGMETHODTYPE,
                                                    ID = handlingMethod.ID,
                                                    Description = handlingMethod.DESCRIPTION
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

        public static RequestResult GetTreeItem(List<Models.Shared.Organization> OrganizationList, string OrganizationUniqueID, string AbnormalType, Account Account)
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (string.IsNullOrEmpty(AbnormalType))
                    {
                        if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                        {
                            var abnormalReasonTypeList = db.ABNORMALREASON.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).Select(x => x.ABNORMALTYPE).Distinct().OrderBy(x => x).ToList();

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

                        var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

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

                            if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                                ||
                                (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.ABNORMALREASON.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID)))
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        } 
                    }
                    else
                    {
                        var abnormalReasonList = db.ABNORMALREASON.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID && x.ABNORMALTYPE == AbnormalType).OrderBy(x => x.ID).ToList();

                        foreach (var abnormalReason in abnormalReasonList)
                        {
                            var treeItem = new TreeItem() { Title = abnormalReason.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.AbnormalReason.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", abnormalReason.ID, abnormalReason.DESCRIPTION);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.AbnormalType] = abnormalReason.ABNORMALTYPE;
                            attributes[Define.EnumTreeAttribute.AbnormalReasonUniqueID] = abnormalReason.UNIQUEID;

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
                    { Define.EnumTreeAttribute.AbnormalType, string.Empty },
                    { Define.EnumTreeAttribute.AbnormalReasonUniqueID, string.Empty }
                };

                var organization = OrganizationList.First(x => x.UniqueID == OrganizationUniqueID);

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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                        ||
                        (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.ABNORMALREASON.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID)))
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

        public static RequestResult GetTreeItem(List<Models.Shared.Organization> OrganizationList, string RefOrganizationUniqueID, string OrganizationUniqueID, string AbnormalType)
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (string.IsNullOrEmpty(AbnormalType))
                    {
                        var abnormalReasonTypeList = db.ABNORMALREASON.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).Select(x => x.ABNORMALTYPE).Distinct().OrderBy(x => x).ToList();

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

                        var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && availableOrganizationList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                        foreach (var organization in organizationList)
                        {
                            var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organization.UniqueID, true);

                            if (db.ABNORMALREASON.Any(x => downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID) && availableOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)))
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
                    else
                    {
                        var abnormalReasonList = db.ABNORMALREASON.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID && x.ABNORMALTYPE == AbnormalType).OrderBy(x => x.ID).ToList();

                        foreach (var abnormalReason in abnormalReasonList)
                        {
                            var treeItem = new TreeItem() { Title = abnormalReason.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.AbnormalReason.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", abnormalReason.ID, abnormalReason.DESCRIPTION);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = abnormalReason.ORGANIZATIONUNIQUEID;
                            attributes[Define.EnumTreeAttribute.AbnormalType] = abnormalReason.ABNORMALTYPE;
                            attributes[Define.EnumTreeAttribute.AbnormalReasonUniqueID] = abnormalReason.UNIQUEID;

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
