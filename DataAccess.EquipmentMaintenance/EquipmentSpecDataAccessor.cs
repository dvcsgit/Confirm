using System;
using System.Linq;
using System.Web.Mvc;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.Shared;
using Models.Authenticated;
using Models.EquipmentMaintenance.EquipmentSpecManagement;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.EquipmentMaintenance
{
    public class EquipmentSpecDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.EquipmentSpec.Where(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.EquipmentType))
                    {
                        query = query.Where(x => x.OrganizationUniqueID == Parameters.OrganizationUniqueID && x.EquipmentType == Parameters.EquipmentType);
                    }

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.Description.Contains(Parameters.Keyword));
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
                            UniqueID = x.UniqueID,
                            Permission = Account.OrganizationPermission(x.OrganizationUniqueID),
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.OrganizationUniqueID),
                            EquipmentType = x.EquipmentType,
                            Description = x.Description
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
                using (EDbEntities db = new EDbEntities())
                {
                    var spec = db.EquipmentSpec.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = spec.UniqueID,
                        Permission = Account.OrganizationPermission(spec.OrganizationUniqueID),
                        OrganizationUniqueID = spec.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(spec.OrganizationUniqueID),
                        EquipmentType = spec.EquipmentType,
                        Description = spec.Description,
                        OptionDescriptionList = db.EquipmentSpecOption.Where(x => x.SpecUniqueID == spec.UniqueID).OrderBy(x => x.Seq).Select(x => x.Description).ToList()
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
                using (EDbEntities db = new EDbEntities())
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

                    model.EquipmentTypeSelectItemList.AddRange(db.EquipmentSpec.Where(x => upStreamOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.EquipmentType).Distinct().OrderBy(x => x).Select(x => new SelectListItem
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
                using (EDbEntities db = new EDbEntities())
                {
                    var spec = db.EquipmentSpec.First(x => x.UniqueID == UniqueID);

                    var model = new CreateFormModel()
                    {
                        OrganizationUniqueID = spec.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(spec.OrganizationUniqueID),
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
                            EquipmentType = spec.EquipmentType
                        },
                        OptionList = db.EquipmentSpecOption.Where(x => x.SpecUniqueID == spec.UniqueID).Select(x => new OptionModel
                        {
                            Description = x.Description,
                            Seq = x.Seq
                        }).OrderBy(x => x.Seq).ToList()
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(spec.OrganizationUniqueID, true);

                    model.EquipmentTypeSelectItemList.AddRange(db.EquipmentSpec.Where(x => upStreamOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.EquipmentType).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    model.EquipmentTypeSelectItemList.First(x => x.Value == spec.EquipmentType).Selected = true;

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
                    using (EDbEntities db = new EDbEntities())
                    {
                        var exists = db.EquipmentSpec.FirstOrDefault(x => x.OrganizationUniqueID == Model.OrganizationUniqueID && x.EquipmentType == Model.FormInput.EquipmentType && x.Description == Model.FormInput.Description);

                        if (exists == null)
                        {
                            string uniqueID = Guid.NewGuid().ToString();

                            db.EquipmentSpec.Add(new EquipmentSpec()
                            {
                                UniqueID = uniqueID,
                                OrganizationUniqueID = Model.OrganizationUniqueID,
                                EquipmentType = Model.FormInput.EquipmentType,
                                Description = Model.FormInput.Description
                            });

                            db.EquipmentSpecOption.AddRange(Model.FormInput.OptionList.Select(x => new EquipmentSpecOption
                            {
                                UniqueID = Guid.NewGuid().ToString(),
                                SpecUniqueID = uniqueID,
                                Description = x.Description,
                                Seq = x.Seq
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
                using (EDbEntities db = new EDbEntities())
                {
                    var query = db.EquipmentSpec.First(x => x.UniqueID == UniqueID);

                    var model = new EditFormModel()
                    {
                        UniqueID = query.UniqueID,
                        OrganizationUniqueID = query.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(query.OrganizationUniqueID),
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
                            EquipmentType = query.EquipmentType,
                            Description = query.Description
                        },
                        OptionList = db.EquipmentSpecOption.Where(x => x.SpecUniqueID == query.UniqueID).Select(x => new OptionModel
                        {
                            UniqueID = x.UniqueID,
                            Description = x.Description,
                            Seq = x.Seq
                        }).OrderBy(x => x.Seq).ToList()
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(model.OrganizationUniqueID, true);

                    model.EquipmentTypeSelectItemList.AddRange(db.EquipmentSpec.Where(x => upStreamOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.EquipmentType).Distinct().OrderBy(x => x).Select(x => new SelectListItem
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
                    using (EDbEntities db = new EDbEntities())
                    {

                        var spec = db.EquipmentSpec.First(x => x.UniqueID == Model.UniqueID);

                        var exists = db.EquipmentSpec.FirstOrDefault(x => x.UniqueID != spec.UniqueID && x.OrganizationUniqueID == spec.OrganizationUniqueID && x.EquipmentType == Model.FormInput.EquipmentType && x.Description == Model.FormInput.Description);

                        if (exists == null)
                        {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                            #region EquipmentSpec
                            spec.EquipmentType = Model.FormInput.EquipmentType;
                            spec.Description = Model.FormInput.Description;

                            db.SaveChanges();
                            #endregion

                            #region EquipmentSpecOption
                            #region Delete
                            db.EquipmentSpecOption.RemoveRange(db.EquipmentSpecOption.Where(x => x.SpecUniqueID == Model.UniqueID).ToList());

                            db.SaveChanges();
                            #endregion

                            #region Insert
                            db.EquipmentSpecOption.AddRange(Model.FormInput.OptionList.Select(x => new EquipmentSpecOption
                            {
                                UniqueID = !string.IsNullOrEmpty(x.UniqueID) ? x.UniqueID : Guid.NewGuid().ToString(),
                                SpecUniqueID = spec.UniqueID,
                                Description = x.Description,
                                Seq = x.Seq
                            }).ToList());

                            db.SaveChanges();
                            #endregion
                            #endregion

                            #region EquipmentSpecValue
                            var optionList = Model.FormInput.OptionList.Where(x => !string.IsNullOrEmpty(x.UniqueID)).Select(x => x.UniqueID).ToList();

                            var specValueList = db.EquipmentSpecValue.Where(x => x.SpecUniqueID == spec.UniqueID && !optionList.Contains(x.SpecOptionUniqueID)).ToList();

                            foreach (var specValue in specValueList)
                            {
                                specValue.SpecOptionUniqueID = string.Empty;
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
                using (EDbEntities db = new EDbEntities())
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

                using (EDbEntities edb = new EDbEntities())
                {
                    if (string.IsNullOrEmpty(EquipmentType))
                    {
                        if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                        {
                            var equipmentTypeList = edb.EquipmentSpec.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).Select(x => x.EquipmentType).Distinct().OrderBy(x => x).ToList();

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

                            var haveEquipmentSpec = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && edb.EquipmentSpec.Any(x => x.OrganizationUniqueID == organization.UniqueID);

                            if (haveDownStreamOrganization || haveEquipmentSpec)
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else
                    {
                        var specList = edb.EquipmentSpec.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && x.EquipmentType == EquipmentType).OrderBy(x => x.Description).ToList();

                        foreach (var spec in specList)
                        {
                            var treeItem = new TreeItem() { Title = spec.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.EquipmentSpec.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = spec.Description;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = spec.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentType] = spec.EquipmentType;
                            attributes[Define.EnumTreeAttribute.EquipmentSpecUniqueID] = spec.UniqueID;

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

                using (EDbEntities edb = new EDbEntities())
                {
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

                    var haveDownStreamOrganization = OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID));

                    var haveEquipmentSpec = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && edb.EquipmentSpec.Any(x => x.OrganizationUniqueID == organization.UniqueID);

                    if (haveDownStreamOrganization || haveEquipmentSpec)
                    {
                        treeItem.State = "closed";
                    }

                    treeItemList.Add(treeItem);
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

                using (EDbEntities edb = new EDbEntities())
                {
                    if (string.IsNullOrEmpty(EquipmentType))
                    {
                        var equipmentTypeList = edb.EquipmentSpec.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).Select(x => x.EquipmentType).Distinct().OrderBy(x => x).ToList();

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

                            if (edb.EquipmentSpec.Any(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && availableOrganizationList.Contains(x.OrganizationUniqueID)))
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
                        var specList = edb.EquipmentSpec.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && x.EquipmentType == EquipmentType).OrderBy(x => x.Description).ToList();

                        foreach (var spec in specList)
                        {
                            var treeItem = new TreeItem() { Title = spec.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.EquipmentSpec.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = spec.Description;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = spec.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentType] = spec.EquipmentType;
                            attributes[Define.EnumTreeAttribute.EquipmentSpecUniqueID] = spec.UniqueID;

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
