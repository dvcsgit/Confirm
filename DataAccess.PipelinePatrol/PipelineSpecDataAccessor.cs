using System;
using System.Linq;
using System.Reflection;
#if !DEBUG
using System.Transactions;
#endif
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.MSSQL;
using DbEntity.MSSQL.PipelinePatrol;
using Models.Shared;
using Models.Authenticated;
using Models.PipelinePatrol.PipelineSpecManagement;
using System.Web.Mvc;

namespace DataAccess.PipelinePatrol
{
    public class PipelineSpecDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.PipelineSpec.Where(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Type))
                    {
                        query = query.Where(x => x.OrganizationUniqueID == Parameters.OrganizationUniqueID && x.Type == Parameters.Type);
                    }

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.Description.Contains(Parameters.Keyword));
                    }

                    var organization = OrganizationDataAccessor.GetOrganization(Parameters.OrganizationUniqueID);

                    result.ReturnData(new GridViewModel()
                    {
                        OrganizationUniqueID = Parameters.OrganizationUniqueID,
                        Permission = Account.OrganizationPermission(Parameters.OrganizationUniqueID),
                        Type = Parameters.Type,
                        FullOrganizationDescription = organization.FullDescription,
                        OrganizationDescription = organization.Description,
                        ItemList = query.ToList().Select(x => new GridItem()
                        {
                            UniqueID = x.UniqueID,
                            Permission = Account.OrganizationPermission(x.OrganizationUniqueID),
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.OrganizationUniqueID),
                            Type = x.Type,
                            Description = x.Description
                        }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.Type).ThenBy(x => x.Description).ToList()
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
                    var spec = db.PipelineSpec.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = spec.UniqueID,
                        Permission = Account.OrganizationPermission(spec.OrganizationUniqueID),
                        OrganizationUniqueID = spec.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(spec.OrganizationUniqueID),
                        Type = spec.Type,
                        Description = spec.Description,
                        OptionDescriptionList = db.PipelineSpecOption.Where(x => x.SpecUniqueID == spec.UniqueID).OrderBy(x => x.Seq).Select(x => x.Description).ToList()
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

        public static RequestResult GetCreateFormModel(string OrganizationUniqueID, string Type)
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
                        TypeSelectItemList = new List<SelectListItem>()
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
                            Type = Type
                        }
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(OrganizationUniqueID, true);

                    model.TypeSelectItemList.AddRange(db.PipelineSpec.Where(x => upStreamOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.Type).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    if (!string.IsNullOrEmpty(Type) && model.TypeSelectItemList.Any(x => x.Value == Type))
                    {
                        model.TypeSelectItemList.First(x => x.Value == Type).Selected = true;
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
                using (PDbEntities db = new PDbEntities())
                {
                    var spec = db.PipelineSpec.First(x => x.UniqueID == UniqueID);

                    var model = new CreateFormModel()
                    {
                        OrganizationUniqueID = spec.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(spec.OrganizationUniqueID),
                        TypeSelectItemList = new List<SelectListItem>()
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
                            Type = spec.Type
                        },
                        OptionList = db.PipelineSpecOption.Where(x => x.SpecUniqueID == spec.UniqueID).Select(x => new OptionModel
                        {
                            Description = x.Description,
                            Seq = x.Seq
                        }).OrderBy(x => x.Seq).ToList()
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(spec.OrganizationUniqueID, true);

                    model.TypeSelectItemList.AddRange(db.PipelineSpec.Where(x => upStreamOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.Type).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    model.TypeSelectItemList.First(x => x.Value == spec.Type).Selected = true;

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
                if (Model.FormInput.Type == Define.NEW)
                {
                    result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.Unsupported, Resources.Resource.SpecType));
                }
                else
                {
                    using (PDbEntities db = new PDbEntities())
                    {
                        var exists = db.PipelineSpec.FirstOrDefault(x => x.OrganizationUniqueID == Model.OrganizationUniqueID && x.Type == Model.FormInput.Type && x.Description == Model.FormInput.Description);

                        if (exists == null)
                        {
                            string uniqueID = Guid.NewGuid().ToString();

                            db.PipelineSpec.Add(new PipelineSpec()
                            {
                                UniqueID = uniqueID,
                                OrganizationUniqueID = Model.OrganizationUniqueID,
                                Type = Model.FormInput.Type,
                                Description = Model.FormInput.Description
                            });

                            db.PipelineSpecOption.AddRange(Model.FormInput.OptionList.Select(x => new PipelineSpecOption
                            {
                                UniqueID = Guid.NewGuid().ToString(),
                                SpecUniqueID = uniqueID,
                                Description = x.Description,
                                Seq = x.Seq
                            }).ToList());

                            db.SaveChanges();

                            result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.PipelineSpec, Resources.Resource.Success));
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.PipelineSpecDescription, Resources.Resource.Exists));
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
                    var spec = db.PipelineSpec.First(x => x.UniqueID == UniqueID);

                    var model = new EditFormModel()
                    {
                        UniqueID = spec.UniqueID,
                        OrganizationUniqueID = spec.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(spec.OrganizationUniqueID),
                        TypeSelectItemList = new List<SelectListItem>()
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
                            Type = spec.Type,
                            Description = spec.Description
                        },
                        OptionList = db.PipelineSpecOption.Where(x => x.SpecUniqueID == spec.UniqueID).Select(x => new OptionModel
                        {
                            UniqueID = x.UniqueID,
                            Description = x.Description,
                            Seq = x.Seq
                        }).OrderBy(x => x.Seq).ToList()
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(spec.OrganizationUniqueID, true);

                    model.TypeSelectItemList.AddRange(db.PipelineSpec.Where(x => upStreamOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.Type).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    model.TypeSelectItemList.First(x => x.Value == spec.Type).Selected = true;

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
                using (PDbEntities db = new PDbEntities())
                {

                    var spec = db.PipelineSpec.First(x => x.UniqueID == Model.UniqueID);

                    var exists = db.PipelineSpec.FirstOrDefault(x => x.UniqueID != spec.UniqueID && x.OrganizationUniqueID == spec.OrganizationUniqueID && x.Type==Model.FormInput.Type && x.Description == Model.FormInput.Description);

                    if (exists == null)
                    {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                        #region PipelineSpec
                        spec.Type = Model.FormInput.Type;
                        spec.Description = Model.FormInput.Description;

                        db.SaveChanges();
                        #endregion

                        #region PipelineSpecOption
                        #region Delete
                        db.PipelineSpecOption.RemoveRange(db.PipelineSpecOption.Where(x => x.SpecUniqueID == Model.UniqueID).ToList());

                        db.SaveChanges();
                        #endregion

                        #region Insert
                        db.PipelineSpecOption.AddRange(Model.FormInput.OptionList.Select(x => new PipelineSpecOption
                        {
                            UniqueID = !string.IsNullOrEmpty(x.UniqueID) ? x.UniqueID : Guid.NewGuid().ToString(),
                            SpecUniqueID = spec.UniqueID,
                            Description = x.Description,
                            Seq = x.Seq
                        }).ToList());

                        db.SaveChanges();
                        #endregion
                        #endregion

                        #region PipelineSpecValue
                        var optionList = Model.FormInput.OptionList.Where(x => !string.IsNullOrEmpty(x.UniqueID)).Select(x => x.UniqueID).ToList();

                        var specValueList = db.PipelineSpecValue.Where(x => x.SpecUniqueID == spec.UniqueID && !optionList.Contains(x.SpecOptionUniqueID)).ToList();

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
                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.PipelineSpec, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.PipelineSpecDescription, Resources.Resource.Exists));
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
                using (PDbEntities db = new PDbEntities())
                {
                    DeleteHelper.PipelineSpec(db, SelectedList);

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.PipelineSpec, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetTreeItem(string OrganizationUniqueID, string Type, Account Account)
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
                    { Define.EnumTreeAttribute.SpecType, string.Empty },
                    { Define.EnumTreeAttribute.PipelineSpecUniqueID, string.Empty }
                };

                using (PDbEntities pdb = new PDbEntities())
                {
                    if (string.IsNullOrEmpty(Type))
                    {
                        if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                        {
                            var typeList = pdb.PipelineSpec.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).Select(x => x.Type).Distinct().OrderBy(x => x).ToList();

                            foreach (var type in typeList)
                            {
                                var treeItem = new TreeItem() { Title = type };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.SpecType.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = type;
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                                attributes[Define.EnumTreeAttribute.SpecType] = type;
                                attributes[Define.EnumTreeAttribute.PipelineSpecUniqueID] = string.Empty;

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
                                attributes[Define.EnumTreeAttribute.SpecType] = string.Empty;
                                attributes[Define.EnumTreeAttribute.PipelineSpecUniqueID] = string.Empty;

                                foreach (var attribute in attributes)
                                {
                                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                }

                                var haveDownStreamOrganization = db.Organization.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID));

                                var havePipelineSpec = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && pdb.PipelineSpec.Any(x => x.OrganizationUniqueID == organization.UniqueID);

                                if (haveDownStreamOrganization || havePipelineSpec)
                                {
                                    treeItem.State = "closed";
                                }

                                treeItemList.Add(treeItem);
                            }
                        }
                    }
                    else
                    {
                        var specList = pdb.PipelineSpec.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && x.Type == Type).OrderBy(x => x.Description).ToList();

                        foreach (var spec in specList)
                        {
                            var treeItem = new TreeItem() { Title = spec.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.PipelineSpec.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = spec.Description;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = spec.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.SpecType] = spec.Type;
                            attributes[Define.EnumTreeAttribute.PipelineSpecUniqueID] = spec.UniqueID;

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

        public static RequestResult GetTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID, string Type)
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
                    { Define.EnumTreeAttribute.SpecType, string.Empty },
                    { Define.EnumTreeAttribute.PipelineSpecUniqueID, string.Empty }
                };

                using (PDbEntities pdb = new PDbEntities())
                {
                    if (string.IsNullOrEmpty(Type))
                    {
                        var typeList = pdb.PipelineSpec.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).Select(x => x.Type).Distinct().OrderBy(x => x).ToList();

                        foreach (var type in typeList)
                        {
                            var treeItem = new TreeItem() { Title = type };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.SpecType.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = type;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.SpecType] = type;
                            attributes[Define.EnumTreeAttribute.PipelineSpecUniqueID] = string.Empty;

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

                                if (pdb.PipelineSpec.Any(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && availableOrganizationList.Contains(x.OrganizationUniqueID)))
                                {
                                    var treeItem = new TreeItem() { Title = organization.Description };

                                    attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                                    attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                                    attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                                    attributes[Define.EnumTreeAttribute.SpecType] = string.Empty;
                                    attributes[Define.EnumTreeAttribute.PipelineSpecUniqueID] = string.Empty;

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
                        var specList = pdb.PipelineSpec.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && x.Type == Type).OrderBy(x => x.Description).ToList();

                        foreach (var spec in specList)
                        {
                            var treeItem = new TreeItem() { Title = spec.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.PipelineSpec.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = spec.Description;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = spec.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.SpecType] = spec.Type;
                            attributes[Define.EnumTreeAttribute.PipelineSpecUniqueID] = spec.UniqueID;

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
