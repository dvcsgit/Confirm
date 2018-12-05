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
using Models.EquipmentMaintenance.MaterialManagement;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.EquipmentMaintenance
{
    public class MaterialDataAccessor
    {
        public static RequestResult DeletePhoto(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var material = db.Material.First(x => x.UniqueID == UniqueID);

                    //material.Extension = string.Empty;

                    //db.SaveChanges();

                    result.Success();
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

        public static RequestResult UploadPhoto(string UniqueID, string Extension)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var material = db.Material.First(x => x.UniqueID == UniqueID);

                    //material.Extension = Extension;

                    //db.SaveChanges();

                    result.Success();
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

        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.Material.Where(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.EquipmentType))
                    {
                        query = query.Where(x => x.OrganizationUniqueID == Parameters.OrganizationUniqueID && x.EquipmentType == Parameters.EquipmentType);
                    }

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.ID.Contains(Parameters.Keyword) || x.Name.Contains(Parameters.Keyword));
                    }

                    result.ReturnData(new GridViewModel()
                    {
                        OrganizationUniqueID = Parameters.OrganizationUniqueID,
                        Permission = Account.OrganizationPermission(Parameters.OrganizationUniqueID),
                        EquipmentType = Parameters.EquipmentType,
                        OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(Parameters.OrganizationUniqueID),
                        FullOrganizationDescription = OrganizationDataAccessor.GetOrganizationFullDescription(Parameters.OrganizationUniqueID),
                        ItemList = query.ToList().Select(x => new GridItem()
                        {
                            Permission = Account.OrganizationPermission(x.OrganizationUniqueID),
                            UniqueID = x.UniqueID,
                            ID = x.ID,
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.OrganizationUniqueID),
                            EquipmentType = x.EquipmentType,
                            Name = x.Name,
                            Quantity = x.Quantity
                        }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.EquipmentType).ThenBy(x => x.ID).ToList()
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
                    var material = db.Material.First(x => x.UniqueID == UniqueID);

                    var model = new DetailViewModel()
                    {
                        Permission = Account.OrganizationPermission(material.OrganizationUniqueID),
                        UniqueID = material.UniqueID,
                        OrganizationUniqueID = material.OrganizationUniqueID,
                        ParentOrganizationDescription = OrganizationDataAccessor.GetOrganizationFullDescription(material.OrganizationUniqueID),
                        EquipmentType = material.EquipmentType,
                        ID = material.ID,
                        Name = material.Name,
                        Quantity = material.Quantity,
                        SpecList = (from x in db.MaterialSpecValue
                                    join s in db.MaterialSpec
                                    on x.SpecUniqueID equals s.UniqueID
                                    where x.MaterialUniqueID == material.UniqueID
                                    select new SpecModel
                                    {
                                        UniqueID = s.UniqueID,
                                        Description = s.Description,
                                        OptionUniqueID = x.SpecOptionUniqueID,
                                        Value = x.Value,
                                        Seq = x.Seq,
                                        OptionList = db.MaterialSpecOption.Where(o => o.SpecUniqueID == s.UniqueID).Select(o=>new Models.EquipmentMaintenance.MaterialManagement.MaterialSpecOption{
                                         SpecUniqueID=o.SpecUniqueID,
                                          Seq=o.Seq,
                                           Description=o.Description,
                                            UniqueID=o.UniqueID
                                        }).OrderBy(o => o.Seq).ToList()
                                    }).OrderBy(x => x.Seq).ToList()
                    };

                    var query = db.EquipmentMaterial.Where(x => x.MaterialUniqueID == material.UniqueID).Select(x => new
                    {
                        x.EquipmentUniqueID,
                        x.PartUniqueID,
                        x.Quantity
                    }).ToList();

                    foreach (var q in query)
                    {
                        var equipment = db.Equipment.FirstOrDefault(x => x.UniqueID == q.EquipmentUniqueID);

                        if (equipment != null)
                        {
                            var equipmentModel = new EquipmentModel()
                            {
                                OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(equipment.OrganizationUniqueID),
                                EquipmentID = equipment.ID,
                                EquipmentName = equipment.Name,
                                Quantity = q.Quantity
                            };

                            var part = db.EquipmentPart.FirstOrDefault(x => x.EquipmentUniqueID == q.EquipmentUniqueID && x.UniqueID == q.PartUniqueID);

                            if (part != null)
                            {
                                equipmentModel.PartDescription = part.Description;
                            }

                            model.EquipmentList.Add(equipmentModel);
                        }
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
                                Text = Resources.Resource.Create + "...",
                                Value = Define.NEW
                            }
                        },
                        FormInput = new FormInput()
                        {
                            EquipmentType = EquipmentType
                        }
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(OrganizationUniqueID, true);

                    model.EquipmentTypeSelectItemList.AddRange(db.Material.Where(x => upStreamOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.EquipmentType).Distinct().OrderBy(x => x).Select(x => new SelectListItem
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
                    var material = db.Material.First(x => x.UniqueID == UniqueID);

                    var model = new CreateFormModel()
                    {
                        OrganizationUniqueID = material.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(material.OrganizationUniqueID),
                        EquipmentTypeSelectItemList = new List<SelectListItem>()
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
                            EquipmentType = material.EquipmentType
                        },
                        SpecList = (from x in db.MaterialSpecValue
                                    join s in db.MaterialSpec
                                    on x.SpecUniqueID equals s.UniqueID
                                    where x.MaterialUniqueID == material.UniqueID
                                    select new SpecModel
                                    {
                                        UniqueID = s.UniqueID,
                                        Description = s.Description,
                                        OptionUniqueID = x.SpecOptionUniqueID,
                                        Value = x.Value,
                                        Seq = x.Seq,
                                         OptionList = db.MaterialSpecOption.Where(o => o.SpecUniqueID == s.UniqueID).Select(o=>new Models.EquipmentMaintenance.MaterialManagement.MaterialSpecOption{
                                         SpecUniqueID=o.SpecUniqueID,
                                          Seq=o.Seq,
                                           Description=o.Description,
                                            UniqueID=o.UniqueID
                                        }).OrderBy(o => o.Seq).ToList()
                                    }).OrderBy(x => x.Seq).ToList()
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(material.OrganizationUniqueID, true);

                    model.EquipmentTypeSelectItemList.AddRange(db.Material.Where(x => upStreamOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.EquipmentType).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    model.EquipmentTypeSelectItemList.First(x => x.Value == material.EquipmentType).Selected = true;

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
                        var exists = db.Material.FirstOrDefault(x => x.OrganizationUniqueID == Model.OrganizationUniqueID && x.EquipmentType == Model.FormInput.EquipmentType && x.ID == Model.FormInput.ID);

                        if (exists == null)
                        {
                            string uniqueID = Guid.NewGuid().ToString();

                            db.Material.Add(new Material()
                            {
                                UniqueID = uniqueID,
                                OrganizationUniqueID = Model.OrganizationUniqueID,
                                EquipmentType = Model.FormInput.EquipmentType,
                                ID = Model.FormInput.ID,
                                Name = Model.FormInput.Name,
                                Quantity = Model.FormInput.Quantity.HasValue ? Model.FormInput.Quantity.Value : 0
                            });

                            db.MaterialSpecValue.AddRange(Model.SpecList.Select(x => new MaterialSpecValue
                            {
                                MaterialUniqueID = uniqueID,
                                SpecUniqueID = x.UniqueID,
                                SpecOptionUniqueID = x.OptionUniqueID,
                                Value = x.Value,
                                Seq = x.Seq
                            }).ToList());

                            db.SaveChanges();

                            result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.Material, Resources.Resource.Success));
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.MaterialID, Resources.Resource.Exists));
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
                    var material = db.Material.First(x => x.UniqueID == UniqueID);

                    var model = new EditFormModel()
                    {
                        UniqueID = material.UniqueID,
                        OrganizationUniqueID = material.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(material.OrganizationUniqueID),
                        EquipmentTypeSelectItemList = new List<SelectListItem>()
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
                            EquipmentType = material.EquipmentType,
                            ID = material.ID,
                            Name = material.Name,
                            Quantity = material.Quantity
                        },
                        SpecList = (from x in db.MaterialSpecValue
                                    join s in db.MaterialSpec
                                    on x.SpecUniqueID equals s.UniqueID
                                    where x.MaterialUniqueID == material.UniqueID
                                    select new SpecModel
                                    {
                                        UniqueID = s.UniqueID,
                                        Description = s.Description,
                                        OptionUniqueID = x.SpecOptionUniqueID,
                                        Value = x.Value,
                                        Seq = x.Seq,
                                        OptionList = db.MaterialSpecOption.Where(o => o.SpecUniqueID == s.UniqueID).Select(o => new Models.EquipmentMaintenance.MaterialManagement.MaterialSpecOption
                                        {
                                            UniqueID = o.UniqueID,
                                            Description = o.Description,
                                            Seq = o.Seq,
                                            SpecUniqueID = o.SpecUniqueID
                                        }).OrderBy(o => o.Description).ToList()
                                    }).OrderBy(x => x.Seq).ToList()
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(model.OrganizationUniqueID, true);

                    model.EquipmentTypeSelectItemList.AddRange(db.Material.Where(x => upStreamOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.EquipmentType).Distinct().OrderBy(x => x).Select(x => new SelectListItem
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

                        var material = db.Material.First(x => x.UniqueID == Model.UniqueID);

                        var exists = db.Material.FirstOrDefault(x => x.UniqueID != material.UniqueID && x.OrganizationUniqueID == material.OrganizationUniqueID && x.EquipmentType == Model.FormInput.EquipmentType && x.ID == Model.FormInput.ID);

                        if (exists == null)
                        {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                            #region Material
                            material.EquipmentType = Model.FormInput.EquipmentType;
                            material.ID = Model.FormInput.ID;
                            material.Name = Model.FormInput.Name;
                            //material.Quantity = Model.FormInput.Quantity.HasValue ? Model.FormInput.Quantity.Value : 0;

                            db.SaveChanges();
                            #endregion

                            #region MaterialSpecValue
                            #region Delete
                            db.MaterialSpecValue.RemoveRange(db.MaterialSpecValue.Where(x => x.MaterialUniqueID == Model.UniqueID).ToList());

                            db.SaveChanges();
                            #endregion

                            #region Insert
                            db.MaterialSpecValue.AddRange(Model.SpecList.Select(x => new MaterialSpecValue
                            {
                                MaterialUniqueID = Model.UniqueID,
                                SpecUniqueID = x.UniqueID,
                                SpecOptionUniqueID = x.OptionUniqueID,
                                Value = x.Value,
                                Seq = x.Seq
                            }).ToList());

                            db.SaveChanges();
                            #endregion
                            #endregion

                            result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.Material, Resources.Resource.Success));
#if !DEBUG
                        trans.Complete();
                    }
#endif
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.MaterialID, Resources.Resource.Exists));
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
                    DeleteHelper.Material(db, SelectedList);

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.Material, Resources.Resource.Success));
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
                    { Define.EnumTreeAttribute.MaterialUniqueID, string.Empty }
                };

                using (EDbEntities edb = new EDbEntities())
                {
                    if (string.IsNullOrEmpty(EquipmentType))
                    {
                        if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                        {
                            var equipmentTypeList = edb.Material.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).Select(x => x.EquipmentType).Distinct().OrderBy(x => x).ToList();

                            foreach (var equipmentType in equipmentTypeList)
                            {
                                var treeItem = new TreeItem() { Title = equipmentType };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.EquipmentType.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = equipmentType;
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                                attributes[Define.EnumTreeAttribute.EquipmentType] = equipmentType;
                                attributes[Define.EnumTreeAttribute.MaterialUniqueID] = string.Empty;

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
                            attributes[Define.EnumTreeAttribute.MaterialUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            var haveDownStreamOrganization = OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID));

                            var haveMaterial = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && edb.Material.Any(x => x.OrganizationUniqueID == organization.UniqueID);

                            if (haveDownStreamOrganization || haveMaterial)
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else
                    {
                        var materialList = edb.Material.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && x.EquipmentType == EquipmentType).OrderBy(x => x.ID).ToList();

                        foreach (var material in materialList)
                        {
                            var treeItem = new TreeItem() { Title = material.Name };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Material.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", material.ID, material.Name);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = material.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentType] = material.EquipmentType;
                            attributes[Define.EnumTreeAttribute.MaterialUniqueID] = material.UniqueID;

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
                    { Define.EnumTreeAttribute.MaterialUniqueID, string.Empty }
                };

                using (EDbEntities edb = new EDbEntities())
                {
                    var organization = OrganizationList.First(x => x.UniqueID == OrganizationUniqueID);

                    var treeItem = new TreeItem() { Title = organization.Description };

                    attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                    attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                    attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                    attributes[Define.EnumTreeAttribute.EquipmentType] = string.Empty;
                    attributes[Define.EnumTreeAttribute.MaterialUniqueID] = string.Empty;

                    foreach (var attribute in attributes)
                    {
                        treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                    }

                    var haveDownStreamOrganization = OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID));

                    var haveMaterial = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && edb.Material.Any(x => x.OrganizationUniqueID == organization.UniqueID);

                    if (haveDownStreamOrganization || haveMaterial)
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

        public static RequestResult SavePageState(List<SpecModel> SpecList, List<string> PageStateList)
        {
            RequestResult result = new RequestResult();

            try
            {
                int seq = 1;

                foreach (string pageState in PageStateList)
                {
                    string[] temp = pageState.Split(Define.Seperators, StringSplitOptions.None);

                    string specUniqueID = temp[0];
                    string optionUniqueID = temp[1];
                    string value = temp[2];

                    var spec = SpecList.First(x => x.UniqueID == specUniqueID);

                    spec.OptionUniqueID = optionUniqueID;
                    spec.Value = value;
                    spec.Seq = seq;

                    seq++;
                }

                SpecList = SpecList.OrderBy(x => x.Seq).ToList();

                result.ReturnData(SpecList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult AddSpec(List<SpecModel> SpecList, List<string> SelectedList, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    foreach (string selected in SelectedList)
                    {
                        string[] temp = selected.Split(Define.Seperators, StringSplitOptions.None);

                        var organizationUniqueID = temp[0];
                        var materialType = temp[1];
                        var specUniqueID = temp[2];

                        if (!string.IsNullOrEmpty(specUniqueID))
                        {
                            if (!SpecList.Any(x => x.UniqueID == specUniqueID))
                            {
                                var spec = db.MaterialSpec.First(x => x.UniqueID == specUniqueID);

                                SpecList.Add(new SpecModel()
                                {
                                    UniqueID = spec.UniqueID,
                                    Description = spec.Description,
                                    Seq = SpecList.Count+1,
                                    OptionList = db.MaterialSpecOption.Where(x => x.SpecUniqueID == spec.UniqueID).Select(x=>new Models.EquipmentMaintenance.MaterialManagement.MaterialSpecOption{
                                     UniqueID = x.UniqueID,
                                      Description=x.Description,
                                      Seq = x.Seq,
                                       SpecUniqueID=x.SpecUniqueID
                                    }).OrderBy(x => x.Description).ToList()
                                });
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(materialType))
                            {
                                var specList = db.MaterialSpec.Where(x => x.OrganizationUniqueID == organizationUniqueID && x.MaterialType == materialType).ToList();

                                foreach (var spec in specList)
                                {
                                    if (!SpecList.Any(x => x.UniqueID == spec.UniqueID))
                                    {
                                        SpecList.Add(new SpecModel()
                                        {
                                            UniqueID = spec.UniqueID,
                                            Description = spec.Description,
                                            Seq = SpecList.Count + 1,
                                            OptionList = db.MaterialSpecOption.Where(x => x.SpecUniqueID == spec.UniqueID).Select(x => new Models.EquipmentMaintenance.MaterialManagement.MaterialSpecOption
                                            {
                                                UniqueID = x.UniqueID,
                                                Description = x.Description,
                                                Seq = x.Seq,
                                                SpecUniqueID = x.SpecUniqueID
                                            }).OrderBy(x => x.Description).ToList()
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
                                        var specList = db.MaterialSpec.Where(x => x.OrganizationUniqueID == downStreamOrganization).ToList();

                                        foreach (var spec in specList)
                                        {
                                            if (!SpecList.Any(x => x.UniqueID == spec.UniqueID))
                                            {
                                                SpecList.Add(new SpecModel()
                                                {
                                                    UniqueID = spec.UniqueID,
                                                    Description = spec.Description,
                                                    Seq = SpecList.Count + 1,
                                                    OptionList = db.MaterialSpecOption.Where(x => x.SpecUniqueID == spec.UniqueID).Select(x => new Models.EquipmentMaintenance.MaterialManagement.MaterialSpecOption
                                                    {
                                                        UniqueID = x.UniqueID,
                                                        Description = x.Description,
                                                        Seq = x.Seq,
                                                        SpecUniqueID = x.SpecUniqueID
                                                    }).OrderBy(x => x.Description).ToList()
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                SpecList = SpecList.OrderBy(x => x.Description).ToList();

                result.ReturnData(SpecList);
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
                    { Define.EnumTreeAttribute.MaterialUniqueID, string.Empty }
                };

                using (EDbEntities edb = new EDbEntities())
                {
                    if (string.IsNullOrEmpty(EquipmentType))
                    {
                        var equipmentTypeList = edb.Material.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).Select(x => x.EquipmentType).Distinct().OrderBy(x => x).ToList();

                        foreach (var equipmentType in equipmentTypeList)
                        {
                            var treeItem = new TreeItem() { Title = equipmentType };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.EquipmentType.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = equipmentType;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentType] = equipmentType;
                            attributes[Define.EnumTreeAttribute.MaterialUniqueID] = string.Empty;

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

                            if (edb.Material.Any(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && availableOrganizationList.Contains(x.OrganizationUniqueID)))
                            {
                                var treeItem = new TreeItem() { Title = organization.Description };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                                attributes[Define.EnumTreeAttribute.EquipmentType] = string.Empty;
                                attributes[Define.EnumTreeAttribute.MaterialUniqueID] = string.Empty;

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
                        var materialList = edb.Material.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && x.EquipmentType == EquipmentType).OrderBy(x => x.ID).ToList();

                        foreach (var material in materialList)
                        {
                            var treeItem = new TreeItem() { Title = material.Name };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Material.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", material.ID, material.Name);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = material.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentType] = material.EquipmentType;
                            attributes[Define.EnumTreeAttribute.MaterialUniqueID] = material.UniqueID;

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

        public static RequestResult GetTreeItem(List<Models.Shared.Organization> OrganizationList, string RefOrganizationUniqueID, string EquipmentUniqueID, string PartUniqueID, string OrganizationUniqueID, string EquipmentType)
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
                    { Define.EnumTreeAttribute.MaterialUniqueID, string.Empty }
                };

                using (EDbEntities edb = new EDbEntities())
                {
                    if (!string.IsNullOrEmpty(EquipmentUniqueID))
                    {
                        var materialList = (from x in edb.EquipmentMaterial
                                            join m in edb.Material
                                            on x.MaterialUniqueID equals m.UniqueID
                                            where x.EquipmentUniqueID == EquipmentUniqueID && x.PartUniqueID == PartUniqueID
                                            select m).OrderBy(x => x.ID).ToList();

                        foreach (var material in materialList)
                        {
                            var treeItem = new TreeItem() { Title = material.Name };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Material.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", material.ID, material.Name);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = material.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentType] = material.EquipmentType;
                            attributes[Define.EnumTreeAttribute.MaterialUniqueID] = material.UniqueID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else  if (string.IsNullOrEmpty(EquipmentType))
                    {
                        var equipmentTypeList = edb.Material.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).Select(x => x.EquipmentType).Distinct().OrderBy(x => x).ToList();

                        foreach (var equipmentType in equipmentTypeList)
                        {
                            var treeItem = new TreeItem() { Title = equipmentType };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.EquipmentType.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = equipmentType;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentType] = equipmentType;
                            attributes[Define.EnumTreeAttribute.MaterialUniqueID] = string.Empty;

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

                            if (edb.Material.Any(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && availableOrganizationList.Contains(x.OrganizationUniqueID)))
                            {
                                var treeItem = new TreeItem() { Title = organization.Description };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                                attributes[Define.EnumTreeAttribute.EquipmentType] = string.Empty;
                                attributes[Define.EnumTreeAttribute.MaterialUniqueID] = string.Empty;

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
                        var materialList = edb.Material.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && x.EquipmentType == EquipmentType).OrderBy(x => x.ID).ToList();

                        foreach (var material in materialList)
                        {
                            var treeItem = new TreeItem() { Title = material.Name };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Material.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", material.ID, material.Name);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = material.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentType] = material.EquipmentType;
                            attributes[Define.EnumTreeAttribute.MaterialUniqueID] = material.UniqueID;

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
