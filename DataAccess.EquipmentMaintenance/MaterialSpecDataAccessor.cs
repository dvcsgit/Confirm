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
using Models.EquipmentMaintenance.MaterialSpecManagement;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.EquipmentMaintenance
{
    public class MaterialSpecDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.MaterialSpec.Where(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.MaterialType))
                    {
                        query = query.Where(x => x.OrganizationUniqueID == Parameters.OrganizationUniqueID && x.MaterialType == Parameters.MaterialType);
                    }

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.Description.Contains(Parameters.Keyword));
                    }

                    result.ReturnData(new GridViewModel()
                    {
                        Permission = Account.OrganizationPermission(Parameters.OrganizationUniqueID),
                        OrganizationUniqueID = Parameters.OrganizationUniqueID,
                        MaterialType = Parameters.MaterialType,
                        OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(Parameters.OrganizationUniqueID),
                        FullOrganizationDescription = OrganizationDataAccessor.GetOrganizationFullDescription(Parameters.OrganizationUniqueID),
                        ItemList = query.ToList().Select(x => new GridItem()
                        {
                            Permission = Account.OrganizationPermission(x.OrganizationUniqueID),
                            UniqueID = x.UniqueID,
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.OrganizationUniqueID),
                            MaterialType = x.MaterialType,
                            Description = x.Description
                        }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.MaterialType).ThenBy(x => x.Description).ToList()
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
                    var spec = db.MaterialSpec.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        Permission = Account.OrganizationPermission(spec.OrganizationUniqueID),
                        UniqueID = spec.UniqueID,
                        OrganizationUniqueID = spec.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(spec.OrganizationUniqueID),
                        MaterialType = spec.MaterialType,
                        Description = spec.Description,
                        OptionDescriptionList = db.MaterialSpecOption.Where(x => x.SpecUniqueID == spec.UniqueID).OrderBy(x => x.Seq).Select(x => x.Description).ToList()
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

        public static RequestResult GetCreateFormModel(string OrganizationUniqueID, string MaterialType)
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
                        MaterialTypeSelectItemList = new List<SelectListItem>()
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
                            MaterialType = MaterialType
                        }
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(OrganizationUniqueID, true);

                    model.MaterialTypeSelectItemList.AddRange(db.MaterialSpec.Where(x => upStreamOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.MaterialType).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    if (!string.IsNullOrEmpty(MaterialType) && model.MaterialTypeSelectItemList.Any(x => x.Value == MaterialType))
                    {
                        model.MaterialTypeSelectItemList.First(x => x.Value == MaterialType).Selected = true;
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
                    var spec = db.MaterialSpec.First(x => x.UniqueID == UniqueID);

                    var model = new CreateFormModel()
                    {
                        OrganizationUniqueID = spec.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(spec.OrganizationUniqueID),
                        MaterialTypeSelectItemList = new List<SelectListItem>()
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
                            MaterialType = spec.MaterialType
                        },
                        OptionList = db.MaterialSpecOption.Where(x => x.SpecUniqueID == spec.UniqueID).Select(x => new OptionModel
                        {
                            Description = x.Description,
                            Seq = x.Seq
                        }).OrderBy(x => x.Seq).ToList()
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(spec.OrganizationUniqueID, true);

                    model.MaterialTypeSelectItemList.AddRange(db.MaterialSpec.Where(x => upStreamOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.MaterialType).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    model.MaterialTypeSelectItemList.First(x => x.Value == spec.MaterialType).Selected = true;

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
                if (Model.FormInput.MaterialType == Define.NEW)
                {
                    result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.Unsupported, Resources.Resource.MaterialType));
                }
                else
                {
                    using (EDbEntities db = new EDbEntities())
                    {
                        var exists = db.MaterialSpec.FirstOrDefault(x => x.OrganizationUniqueID == Model.OrganizationUniqueID && x.MaterialType == Model.FormInput.MaterialType && x.Description == Model.FormInput.Description);

                        if (exists == null)
                        {
                            string uniqueID = Guid.NewGuid().ToString();

                            db.MaterialSpec.Add(new MaterialSpec()
                            {
                                UniqueID = uniqueID,
                                OrganizationUniqueID = Model.OrganizationUniqueID,
                                MaterialType = Model.FormInput.MaterialType,
                                Description = Model.FormInput.Description
                            });

                            db.MaterialSpecOption.AddRange(Model.FormInput.OptionList.Select(x => new MaterialSpecOption
                            {
                                UniqueID = Guid.NewGuid().ToString(),
                                SpecUniqueID = uniqueID,
                                Description = x.Description,
                                Seq = x.Seq
                            }).ToList());
                           
                            db.SaveChanges();

                            result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.MaterialSpec, Resources.Resource.Success));
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.MaterialSpecDescription, Resources.Resource.Exists));
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
                    var spec = db.MaterialSpec.First(x => x.UniqueID == UniqueID);

                    var model = new EditFormModel()
                    {
                        UniqueID = spec.UniqueID,
                        OrganizationUniqueID = spec.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(spec.OrganizationUniqueID),
                        MaterialTypeSelectItemList = new List<SelectListItem>()
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
                            MaterialType = spec.MaterialType,
                            Description = spec.Description
                        },
                        OptionList = db.MaterialSpecOption.Where(x => x.SpecUniqueID == spec.UniqueID).Select(x => new OptionModel
                        {
                            UniqueID = x.UniqueID,
                            Description = x.Description,
                            Seq = x.Seq
                        }).OrderBy(x => x.Seq).ToList()
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(spec.OrganizationUniqueID, true);

                    model.MaterialTypeSelectItemList.AddRange(db.MaterialSpec.Where(x => upStreamOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.MaterialType).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    if (!string.IsNullOrEmpty(spec.MaterialType) && model.MaterialTypeSelectItemList.Any(x => x.Value == spec.MaterialType))
                    {
                        model.MaterialTypeSelectItemList.First(x => x.Value == spec.MaterialType).Selected = true;
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
                if (Model.FormInput.MaterialType == Define.NEW)
                {
                    result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.Unsupported, Resources.Resource.MaterialType));
                }
                else
                {
                    using (EDbEntities db = new EDbEntities())
                    {

                        var spec = db.MaterialSpec.First(x => x.UniqueID == Model.UniqueID);

                        var exists = db.MaterialSpec.FirstOrDefault(x => x.UniqueID != spec.UniqueID && x.OrganizationUniqueID == spec.OrganizationUniqueID && x.MaterialType == Model.FormInput.MaterialType && x.Description == Model.FormInput.Description);

                        if (exists == null)
                        {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                            #region MaterialSpec
                            spec.Description = Model.FormInput.Description;

                            db.SaveChanges();
                            #endregion

                            #region MaterialSpecOption
                            #region Delete
                            db.MaterialSpecOption.RemoveRange(db.MaterialSpecOption.Where(x => x.SpecUniqueID == Model.UniqueID).ToList());

                            db.SaveChanges();
                            #endregion

                            #region Insert
                            db.MaterialSpecOption.AddRange(Model.FormInput.OptionList.Select(x => new MaterialSpecOption
                            {
                                UniqueID = !string.IsNullOrEmpty(x.UniqueID) ? x.UniqueID : Guid.NewGuid().ToString(),
                                SpecUniqueID = spec.UniqueID,
                                Description = x.Description,
                                Seq = x.Seq
                            }).ToList());

                            db.SaveChanges();
                            #endregion
                            #endregion

                            #region MaterialSpecValue
                            var optionList = Model.FormInput.OptionList.Where(x => !string.IsNullOrEmpty(x.UniqueID)).Select(x => x.UniqueID).ToList();

                            var specValueList = db.MaterialSpecValue.Where(x => x.SpecUniqueID == spec.UniqueID && !optionList.Contains(x.SpecOptionUniqueID)).ToList();

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
                            result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.MaterialSpec, Resources.Resource.Success));
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.MaterialSpecDescription, Resources.Resource.Exists));
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
                    DeleteHelper.MaterialSpec(db, SelectedList);

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.MaterialSpec, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetTreeItem(List<Models.Shared.Organization> OrganizationList, string OrganizationUniqueID, string MaterialType, Account Account)
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
                    { Define.EnumTreeAttribute.MaterialType, string.Empty },
                    { Define.EnumTreeAttribute.MaterialSpecUniqueID, string.Empty }
                };

                using (EDbEntities edb = new EDbEntities())
                {
                    if (string.IsNullOrEmpty(MaterialType))
                    {
                        if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                        {
                            var materialTypeList = edb.MaterialSpec.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).Select(x => x.MaterialType).Distinct().OrderBy(x => x).ToList();

                            foreach (var materialType in materialTypeList)
                            {
                                var treeItem = new TreeItem() { Title = materialType };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.MaterialType.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = materialType;
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                                attributes[Define.EnumTreeAttribute.MaterialType] = materialType;
                                attributes[Define.EnumTreeAttribute.MaterialSpecUniqueID] = string.Empty;

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
                            attributes[Define.EnumTreeAttribute.MaterialType] = string.Empty;
                            attributes[Define.EnumTreeAttribute.MaterialSpecUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            var haveDownStreamOrganization = OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID));

                            var haveMaterialSpec = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && edb.MaterialSpec.Any(x => x.OrganizationUniqueID == organization.UniqueID);

                            if (haveDownStreamOrganization || haveMaterialSpec)
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else
                    {
                        var specList = edb.MaterialSpec.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && x.MaterialType == MaterialType).OrderBy(x => x.Description).ToList();

                        foreach (var spec in specList)
                        {
                            var treeItem = new TreeItem() { Title = spec.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.MaterialSpec.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = spec.Description;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = spec.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.MaterialType] = spec.MaterialType;
                            attributes[Define.EnumTreeAttribute.MaterialSpecUniqueID] = spec.UniqueID;

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
                    { Define.EnumTreeAttribute.MaterialType, string.Empty },
                    { Define.EnumTreeAttribute.MaterialSpecUniqueID, string.Empty }
                };

                using (EDbEntities edb = new EDbEntities())
                {
                    var organization = OrganizationList.First(x => x.UniqueID == OrganizationUniqueID);

                    var treeItem = new TreeItem() { Title = organization.Description };

                    attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                    attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                    attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                    attributes[Define.EnumTreeAttribute.MaterialType] = string.Empty;
                    attributes[Define.EnumTreeAttribute.MaterialSpecUniqueID] = string.Empty;

                    foreach (var attribute in attributes)
                    {
                        treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                    }

                    var haveDownStreamOrganization = OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID));

                    var haveMaterialSpec = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && edb.MaterialSpec.Any(x => x.OrganizationUniqueID == organization.UniqueID);

                    if (haveDownStreamOrganization || haveMaterialSpec)
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

        public static RequestResult GetTreeItem(List<Models.Shared.Organization> OrganizationList, string RefOrganizationUniqueID, string OrganizationUniqueID, string MaterialType)
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
                    { Define.EnumTreeAttribute.MaterialType, string.Empty },
                    { Define.EnumTreeAttribute.MaterialSpecUniqueID, string.Empty }
                };

                using (EDbEntities edb = new EDbEntities())
                {
                    if (string.IsNullOrEmpty(MaterialType))
                    {
                        var materialTypeList = edb.MaterialSpec.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).Select(x => x.MaterialType).Distinct().OrderBy(x => x).ToList();

                        foreach (var materialType in materialTypeList)
                        {
                            var treeItem = new TreeItem() { Title = materialType };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.MaterialType.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = materialType;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.MaterialType] = materialType;
                            attributes[Define.EnumTreeAttribute.MaterialSpecUniqueID] = string.Empty;

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

                            if (edb.MaterialSpec.Any(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && availableOrganizationList.Contains(x.OrganizationUniqueID)))
                            {
                                var treeItem = new TreeItem() { Title = organization.Description };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                                attributes[Define.EnumTreeAttribute.MaterialType] = string.Empty;
                                attributes[Define.EnumTreeAttribute.MaterialSpecUniqueID] = string.Empty;

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
                        var specList = edb.MaterialSpec.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && x.MaterialType == MaterialType).OrderBy(x => x.Description).ToList();

                        foreach (var spec in specList)
                        {
                            var treeItem = new TreeItem() { Title = spec.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.MaterialSpec.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = spec.Description;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = spec.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.MaterialType] = spec.MaterialType;
                            attributes[Define.EnumTreeAttribute.MaterialSpecUniqueID] = spec.UniqueID;

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
