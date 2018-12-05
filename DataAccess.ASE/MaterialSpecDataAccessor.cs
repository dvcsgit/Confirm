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
using Models.EquipmentMaintenance.MaterialSpecManagement;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.ASE
{
    public class MaterialSpecDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.MATERIALSPEC.Where(x => downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID) && Account.QueryableOrganizationUniqueIDList.Contains(x.ORGANIZATIONUNIQUEID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.MaterialType))
                    {
                        query = query.Where(x => x.ORGANIZATIONUNIQUEID == Parameters.OrganizationUniqueID && x.MATERIALTYPE == Parameters.MaterialType);
                    }

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.DESCRIPTION.Contains(Parameters.Keyword));
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
                            Permission = Account.OrganizationPermission(x.ORGANIZATIONUNIQUEID),
                            UniqueID = x.UNIQUEID,
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.ORGANIZATIONUNIQUEID),
                            MaterialType = x.MATERIALTYPE,
                            Description = x.DESCRIPTION
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var spec = db.MATERIALSPEC.First(x => x.UNIQUEID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        Permission = Account.OrganizationPermission(spec.ORGANIZATIONUNIQUEID),
                        UniqueID = spec.UNIQUEID,
                        OrganizationUniqueID = spec.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(spec.ORGANIZATIONUNIQUEID),
                        MaterialType = spec.MATERIALTYPE,
                        Description = spec.DESCRIPTION,
                        OptionDescriptionList = db.MATERIALSPECOPTION.Where(x => x.SPECUNIQUEID == spec.UNIQUEID).OrderBy(x => x.SEQ).Select(x => x.DESCRIPTION).ToList()
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
                using (ASEDbEntities db = new ASEDbEntities())
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

                    model.MaterialTypeSelectItemList.AddRange(db.MATERIALSPEC.Where(x => upStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)).Select(x => x.MATERIALTYPE).Distinct().OrderBy(x => x).Select(x => new SelectListItem
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var spec = db.MATERIALSPEC.First(x => x.UNIQUEID == UniqueID);

                    var model = new CreateFormModel()
                    {
                        OrganizationUniqueID = spec.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(spec.ORGANIZATIONUNIQUEID),
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
                            MaterialType = spec.MATERIALTYPE
                        },
                        OptionList = db.MATERIALSPECOPTION.Where(x => x.SPECUNIQUEID == spec.UNIQUEID).Select(x => new OptionModel
                        {
                            Description = x.DESCRIPTION,
                            Seq = x.SEQ.Value
                        }).OrderBy(x => x.Seq).ToList()
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(spec.ORGANIZATIONUNIQUEID, true);

                    model.MaterialTypeSelectItemList.AddRange(db.MATERIALSPEC.Where(x => upStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)).Select(x => x.MATERIALTYPE).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    model.MaterialTypeSelectItemList.First(x => x.Value == spec.MATERIALTYPE).Selected = true;

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
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var exists = db.MATERIALSPEC.FirstOrDefault(x => x.ORGANIZATIONUNIQUEID == Model.OrganizationUniqueID && x.MATERIALTYPE == Model.FormInput.MaterialType && x.DESCRIPTION == Model.FormInput.Description);

                        if (exists == null)
                        {
                            string uniqueID = Guid.NewGuid().ToString();

                            db.MATERIALSPEC.Add(new MATERIALSPEC()
                            {
                                UNIQUEID = uniqueID,
                                ORGANIZATIONUNIQUEID = Model.OrganizationUniqueID,
                                MATERIALTYPE = Model.FormInput.MaterialType,
                                DESCRIPTION = Model.FormInput.Description
                            });

                            db.MATERIALSPECOPTION.AddRange(Model.FormInput.OptionList.Select(x => new MATERIALSPECOPTION
                            {
                                UNIQUEID = Guid.NewGuid().ToString(),
                                SPECUNIQUEID = uniqueID,
                                DESCRIPTION = x.Description,
                                SEQ = x.Seq
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var spec = db.MATERIALSPEC.First(x => x.UNIQUEID == UniqueID);

                    var model = new EditFormModel()
                    {
                        UniqueID = spec.UNIQUEID,
                        OrganizationUniqueID = spec.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(spec.ORGANIZATIONUNIQUEID),
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
                            MaterialType = spec.MATERIALTYPE,
                            Description = spec.DESCRIPTION
                        },
                        OptionList = db.MATERIALSPECOPTION.Where(x => x.SPECUNIQUEID == spec.UNIQUEID).Select(x => new OptionModel
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION,
                            Seq = x.SEQ.Value
                        }).OrderBy(x => x.Seq).ToList()
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(spec.ORGANIZATIONUNIQUEID, true);

                    model.MaterialTypeSelectItemList.AddRange(db.MATERIALSPEC.Where(x => upStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)).Select(x => x.MATERIALTYPE).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    if (!string.IsNullOrEmpty(spec.MATERIALTYPE) && model.MaterialTypeSelectItemList.Any(x => x.Value == spec.MATERIALTYPE))
                    {
                        model.MaterialTypeSelectItemList.First(x => x.Value == spec.MATERIALTYPE).Selected = true;
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
                    using (ASEDbEntities db = new ASEDbEntities())
                    {

                        var spec = db.MATERIALSPEC.First(x => x.UNIQUEID == Model.UniqueID);

                        var exists = db.MATERIALSPEC.FirstOrDefault(x => x.UNIQUEID != spec.UNIQUEID && x.ORGANIZATIONUNIQUEID == spec.ORGANIZATIONUNIQUEID && x.MATERIALTYPE == Model.FormInput.MaterialType && x.DESCRIPTION == Model.FormInput.Description);

                        if (exists == null)
                        {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                            #region MaterialSpec
                            spec.DESCRIPTION = Model.FormInput.Description;

                            db.SaveChanges();
                            #endregion

                            #region MaterialSpecOption
                            #region Delete
                            db.MATERIALSPECOPTION.RemoveRange(db.MATERIALSPECOPTION.Where(x => x.SPECUNIQUEID == Model.UniqueID).ToList());

                            db.SaveChanges();
                            #endregion

                            #region Insert
                            db.MATERIALSPECOPTION.AddRange(Model.FormInput.OptionList.Select(x => new MATERIALSPECOPTION
                            {
                                UNIQUEID = !string.IsNullOrEmpty(x.UniqueID) ? x.UniqueID : Guid.NewGuid().ToString(),
                                SPECUNIQUEID = spec.UNIQUEID,
                                DESCRIPTION = x.Description,
                                SEQ = x.Seq
                            }).ToList());

                            db.SaveChanges();
                            #endregion
                            #endregion

                            #region MaterialSpecValue
                            var optionList = Model.FormInput.OptionList.Where(x => !string.IsNullOrEmpty(x.UniqueID)).Select(x => x.UniqueID).ToList();

                            var specValueList = db.MATERIALSPECVALUE.Where(x => x.SPECUNIQUEID == spec.UNIQUEID && !optionList.Contains(x.SPECOPTIONUNIQUEID)).ToList();

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
                using (ASEDbEntities db = new ASEDbEntities())
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (string.IsNullOrEmpty(MaterialType))
                    {
                        if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                        {
                            var materialTypeList = db.MATERIALSPEC.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).Select(x => x.MATERIALTYPE).Distinct().OrderBy(x => x).ToList();

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

                            var haveMaterialSpec = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.MATERIALSPEC.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID);

                            if (haveDownStreamOrganization || haveMaterialSpec)
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else
                    {
                        var specList = db.MATERIALSPEC.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID && x.MATERIALTYPE == MaterialType).OrderBy(x => x.DESCRIPTION).ToList();

                        foreach (var spec in specList)
                        {
                            var treeItem = new TreeItem() { Title = spec.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.MaterialSpec.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = spec.DESCRIPTION;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = spec.ORGANIZATIONUNIQUEID;
                            attributes[Define.EnumTreeAttribute.MaterialType] = spec.MATERIALTYPE;
                            attributes[Define.EnumTreeAttribute.MaterialSpecUniqueID] = spec.UNIQUEID;

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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var haveDownStreamOrganization = OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID));

                    var haveMaterialSpec = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.MATERIALSPEC.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID);

                    if (haveDownStreamOrganization || haveMaterialSpec)
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (string.IsNullOrEmpty(MaterialType))
                    {
                        var materialTypeList = db.MATERIALSPEC.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).Select(x => x.MATERIALTYPE).Distinct().OrderBy(x => x).ToList();

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

                            if (db.MATERIALSPEC.Any(x => downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID) && availableOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)))
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
                        var specList = db.MATERIALSPEC.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID && x.MATERIALTYPE == MaterialType).OrderBy(x => x.DESCRIPTION).ToList();

                        foreach (var spec in specList)
                        {
                            var treeItem = new TreeItem() { Title = spec.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.MaterialSpec.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = spec.DESCRIPTION;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = spec.ORGANIZATIONUNIQUEID;
                            attributes[Define.EnumTreeAttribute.MaterialType] = spec.MATERIALTYPE;
                            attributes[Define.EnumTreeAttribute.MaterialSpecUniqueID] = spec.UNIQUEID;

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
