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
using Models.EquipmentMaintenance.EquipmentSpecManagement;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.ASE
{
    public class EquipmentSpecDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.EQUIPMENTSPEC.Where(x => downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID) && Account.QueryableOrganizationUniqueIDList.Contains(x.ORGANIZATIONUNIQUEID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.EquipmentType))
                    {
                        query = query.Where(x => x.ORGANIZATIONUNIQUEID == Parameters.OrganizationUniqueID && x.EQUIPMENTTYPE == Parameters.EquipmentType);
                    }

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.DESCRIPTION.Contains(Parameters.Keyword));
                    }

                    result.ReturnData(new GridViewModel()
                    {
                        OrganizationUniqueID = Parameters.OrganizationUniqueID,
                        Permission = Account.OrganizationPermission(Parameters.OrganizationUniqueID),
                        EquipmentType = Parameters.EquipmentType,
                        FullOrganizationDescription = OrganizationDataAccessor.GetOrganizationFullDescription(Parameters.OrganizationUniqueID),
                        OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(Parameters.OrganizationUniqueID),
                        ItemList = query.ToList().Select(x => new GridItem()
                        {
                            UniqueID = x.UNIQUEID,
                            Permission = Account.OrganizationPermission(x.ORGANIZATIONUNIQUEID),
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.ORGANIZATIONUNIQUEID),
                            EquipmentType = x.EQUIPMENTTYPE,
                            Description = x.DESCRIPTION
                        }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.EquipmentType).ThenBy(x => x.Description).ToList()
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
                    var spec = db.EQUIPMENTSPEC.First(x => x.UNIQUEID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = spec.UNIQUEID,
                        Permission = Account.OrganizationPermission(spec.ORGANIZATIONUNIQUEID),
                        OrganizationUniqueID = spec.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(spec.ORGANIZATIONUNIQUEID),
                        EquipmentType = spec.EQUIPMENTTYPE,
                        Description = spec.DESCRIPTION,
                        OptionDescriptionList = db.EQUIPMENTSPECOPTION.Where(x => x.SPECUNIQUEID == spec.UNIQUEID).OrderBy(x => x.SEQ).Select(x => x.DESCRIPTION).ToList()
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

        public static RequestResult GetCreateFormModel(string OrganizationUniqueID, string EquipmentType)
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
                        EquipmentTypeSelectItemList = new List<SelectListItem>()
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne),
                            new SelectListItem()
                            {
                                Text = string.Format("{0}...", Resources.Resource.Create),
                                Value = Define.NEW
                            }
                        },
                        FormInput = new FormInput()
                        {
                            EquipmentType = EquipmentType
                        }
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(OrganizationUniqueID, true);

                    model.EquipmentTypeSelectItemList.AddRange(db.EQUIPMENTSPEC.Where(x => upStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)).Select(x => x.EQUIPMENTTYPE).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    if (!string.IsNullOrEmpty(EquipmentType) && model.EquipmentTypeSelectItemList.Any(x => x.Value == EquipmentType))
                    {
                        model.EquipmentTypeSelectItemList.First(x => x.Value == EquipmentType).Selected = true;
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
                    var spec = db.EQUIPMENTSPEC.First(x => x.UNIQUEID == UniqueID);

                    var model = new CreateFormModel()
                    {
                        OrganizationUniqueID = spec.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(spec.ORGANIZATIONUNIQUEID),
                        EquipmentTypeSelectItemList = new List<SelectListItem>()
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne),
                            new SelectListItem()
                            {
                                Text = string.Format("{0}...", Resources.Resource.Create),
                                Value = Define.NEW
                            }
                        },
                        FormInput = new FormInput()
                        {
                            EquipmentType = spec.EQUIPMENTTYPE
                        },
                        OptionList = db.EQUIPMENTSPECOPTION.Where(x => x.SPECUNIQUEID == spec.UNIQUEID).Select(x => new OptionModel
                        {
                            Description = x.DESCRIPTION,
                            Seq = x.SEQ.Value
                        }).OrderBy(x => x.Seq).ToList()
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(spec.ORGANIZATIONUNIQUEID, true);

                    model.EquipmentTypeSelectItemList.AddRange(db.EQUIPMENTSPEC.Where(x => upStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)).Select(x => x.EQUIPMENTTYPE).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    model.EquipmentTypeSelectItemList.First(x => x.Value == spec.EQUIPMENTTYPE).Selected = true;

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
                if (Model.FormInput.EquipmentType == Define.NEW)
                {
                    result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.Unsupported, Resources.Resource.EquipmentType));
                }
                else
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var exists = db.EQUIPMENTSPEC.FirstOrDefault(x => x.ORGANIZATIONUNIQUEID == Model.OrganizationUniqueID && x.EQUIPMENTTYPE == Model.FormInput.EquipmentType && x.DESCRIPTION == Model.FormInput.Description);

                        if (exists == null)
                        {
                            string uniqueID = Guid.NewGuid().ToString();

                            db.EQUIPMENTSPEC.Add(new EQUIPMENTSPEC()
                            {
                                UNIQUEID = uniqueID,
                                ORGANIZATIONUNIQUEID = Model.OrganizationUniqueID,
                                EQUIPMENTTYPE = Model.FormInput.EquipmentType,
                                DESCRIPTION = Model.FormInput.Description
                            });

                            db.EQUIPMENTSPECOPTION.AddRange(Model.FormInput.OptionList.Select(x => new EQUIPMENTSPECOPTION
                            {
                                UNIQUEID = Guid.NewGuid().ToString(),
                                SPECUNIQUEID = uniqueID,
                                DESCRIPTION = x.Description,
                                SEQ = x.Seq
                            }).ToList());

                            db.SaveChanges();

                            result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.EquipmentSpec, Resources.Resource.Success));
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.EquipmentSpecDescription, Resources.Resource.Exists));
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
                    var query = db.EQUIPMENTSPEC.First(x => x.UNIQUEID == UniqueID);

                    var model = new EditFormModel()
                    {
                        UniqueID = query.UNIQUEID,
                        OrganizationUniqueID = query.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(query.ORGANIZATIONUNIQUEID),
                        EquipmentTypeSelectItemList = new List<SelectListItem>()
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne),
                            new SelectListItem()
                            {
                                Text = string.Format("{0}...", Resources.Resource.Create),
                                Value = Define.NEW
                            }
                        },
                        FormInput = new FormInput()
                        {
                            EquipmentType = query.EQUIPMENTTYPE,
                            Description = query.DESCRIPTION
                        },
                        OptionList = db.EQUIPMENTSPECOPTION.Where(x => x.SPECUNIQUEID == query.UNIQUEID).Select(x => new OptionModel
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION,
                            Seq = x.SEQ.Value
                        }).OrderBy(x => x.Seq).ToList()
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(model.OrganizationUniqueID, true);

                    model.EquipmentTypeSelectItemList.AddRange(db.EQUIPMENTSPEC.Where(x => upStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)).Select(x => x.EQUIPMENTTYPE).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    if (!string.IsNullOrEmpty(model.FormInput.EquipmentType) && model.EquipmentTypeSelectItemList.Any(x => x.Value == model.FormInput.EquipmentType))
                    {
                        model.EquipmentTypeSelectItemList.First(x => x.Value == model.FormInput.EquipmentType).Selected = true;
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
                if (Model.FormInput.EquipmentType == Define.NEW)
                {
                    result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.Unsupported, Resources.Resource.EquipmentType));
                }
                else
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {

                        var spec = db.EQUIPMENTSPEC.First(x => x.UNIQUEID == Model.UniqueID);

                        var exists = db.EQUIPMENTSPEC.FirstOrDefault(x => x.UNIQUEID != spec.UNIQUEID && x.ORGANIZATIONUNIQUEID == spec.ORGANIZATIONUNIQUEID && x.EQUIPMENTTYPE == Model.FormInput.EquipmentType && x.DESCRIPTION == Model.FormInput.Description);

                        if (exists == null)
                        {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                            #region EquipmentSpec
                            spec.EQUIPMENTTYPE = Model.FormInput.EquipmentType;
                            spec.DESCRIPTION = Model.FormInput.Description;

                            db.SaveChanges();
                            #endregion

                            #region EquipmentSpecOption
                            #region Delete
                            db.EQUIPMENTSPECOPTION.RemoveRange(db.EQUIPMENTSPECOPTION.Where(x => x.SPECUNIQUEID == Model.UniqueID).ToList());

                            db.SaveChanges();
                            #endregion

                            #region Insert
                            db.EQUIPMENTSPECOPTION.AddRange(Model.FormInput.OptionList.Select(x => new EQUIPMENTSPECOPTION
                            {
                                UNIQUEID = !string.IsNullOrEmpty(x.UniqueID) ? x.UniqueID : Guid.NewGuid().ToString(),
                                SPECUNIQUEID = spec.UNIQUEID,
                                DESCRIPTION = x.Description,
                                SEQ = x.Seq
                            }).ToList());

                            db.SaveChanges();
                            #endregion
                            #endregion

                            #region EquipmentSpecValue
                            var optionList = Model.FormInput.OptionList.Where(x => !string.IsNullOrEmpty(x.UniqueID)).Select(x => x.UniqueID).ToList();

                            var specValueList = db.EQUIPMENTSPECVALUE.Where(x => x.SPECUNIQUEID == spec.UNIQUEID && !optionList.Contains(x.SPECOPTIONUNIQUEID)).ToList();

                            foreach (var specValue in specValueList)
                            {
                                specValue.SPECOPTIONUNIQUEID = string.Empty;
                            }

                            db.SaveChanges();
                            #endregion
#if !DEBUG
                        trans.Complete();
                    }
#endif
                            result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.EquipmentSpec, Resources.Resource.Success));
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.EquipmentSpecDescription, Resources.Resource.Exists));
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
                    DeleteHelper.EquipmentSpec(db, SelectedList);

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.EquipmentSpec, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetTreeItem(List<Models.Shared.Organization> OrganizationList, string OrganizationUniqueID, string EquipmentType, Account Account)
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
                    { Define.EnumTreeAttribute.EquipmentType, string.Empty },
                    { Define.EnumTreeAttribute.EquipmentSpecUniqueID, string.Empty }
                };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (string.IsNullOrEmpty(EquipmentType))
                    {
                        if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                        {
                            var equipmentTypeList = db.EQUIPMENTSPEC.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).Select(x => x.EQUIPMENTTYPE).Distinct().OrderBy(x => x).ToList();

                            foreach (var equipmentType in equipmentTypeList)
                            {
                                var treeItem = new TreeItem() { Title = equipmentType };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.EquipmentType.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = equipmentType;
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                                attributes[Define.EnumTreeAttribute.EquipmentType] = equipmentType;
                                attributes[Define.EnumTreeAttribute.EquipmentSpecUniqueID] = string.Empty;

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
                            attributes[Define.EnumTreeAttribute.EquipmentType] = string.Empty;
                            attributes[Define.EnumTreeAttribute.EquipmentSpecUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            var haveDownStreamOrganization = OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID));

                            var haveEquipmentSpec = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.EQUIPMENTSPEC.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID);

                            if (haveDownStreamOrganization || haveEquipmentSpec)
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else
                    {
                        var specList = db.EQUIPMENTSPEC.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID && x.EQUIPMENTTYPE == EquipmentType).OrderBy(x => x.DESCRIPTION).ToList();

                        foreach (var spec in specList)
                        {
                            var treeItem = new TreeItem() { Title = spec.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.EquipmentSpec.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = spec.DESCRIPTION;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = spec.ORGANIZATIONUNIQUEID;
                            attributes[Define.EnumTreeAttribute.EquipmentType] = spec.EQUIPMENTTYPE;
                            attributes[Define.EnumTreeAttribute.EquipmentSpecUniqueID] = spec.UNIQUEID;

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
                    { Define.EnumTreeAttribute.EquipmentType, string.Empty },
                    { Define.EnumTreeAttribute.EquipmentSpecUniqueID, string.Empty }
                };

                var organization = OrganizationList.First(x => x.UniqueID == OrganizationUniqueID);

                var treeItem = new TreeItem() { Title = organization.Description };

                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                attributes[Define.EnumTreeAttribute.EquipmentType] = string.Empty;
                attributes[Define.EnumTreeAttribute.EquipmentSpecUniqueID] = string.Empty;

                foreach (var attribute in attributes)
                {
                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                }

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var haveDownStreamOrganization = OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID));

                    var haveEquipmentSpec = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.EQUIPMENTSPEC.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID);

                    if (haveDownStreamOrganization || haveEquipmentSpec)
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

        public static RequestResult GetTreeItem(List<Models.Shared.Organization> OrganizationList, string RefOrganizationUniqueID, string OrganizationUniqueID, string EquipmentType)
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
                    { Define.EnumTreeAttribute.EquipmentType, string.Empty },
                    { Define.EnumTreeAttribute.EquipmentSpecUniqueID, string.Empty }
                };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (string.IsNullOrEmpty(EquipmentType))
                    {
                        var equipmentTypeList = db.EQUIPMENTSPEC.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).Select(x => x.EQUIPMENTTYPE).Distinct().OrderBy(x => x).ToList();

                        foreach (var equipmentType in equipmentTypeList)
                        {
                            var treeItem = new TreeItem() { Title = equipmentType };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.EquipmentType.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = equipmentType;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentType] = equipmentType;
                            attributes[Define.EnumTreeAttribute.EquipmentSpecUniqueID] = string.Empty;

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

                            if (db.EQUIPMENTSPEC.Any(x => downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID) && availableOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)))
                            {
                                var treeItem = new TreeItem() { Title = organization.Description };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                                attributes[Define.EnumTreeAttribute.EquipmentType] = string.Empty;
                                attributes[Define.EnumTreeAttribute.EquipmentSpecUniqueID] = string.Empty;

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
                        var specList = db.EQUIPMENTSPEC.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID && x.EQUIPMENTTYPE == EquipmentType).OrderBy(x => x.DESCRIPTION).ToList();

                        foreach (var spec in specList)
                        {
                            var treeItem = new TreeItem() { Title = spec.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.EquipmentSpec.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = spec.DESCRIPTION;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = spec.ORGANIZATIONUNIQUEID;
                            attributes[Define.EnumTreeAttribute.EquipmentType] = spec.EQUIPMENTTYPE;
                            attributes[Define.EnumTreeAttribute.EquipmentSpecUniqueID] = spec.UNIQUEID;

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
