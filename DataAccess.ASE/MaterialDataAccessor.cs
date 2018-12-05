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
using Models.EquipmentMaintenance.MaterialManagement;
using System.IO;
using NPOI.SS.UserModel;
using ZXing;
using ZXing.Common;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.ASE
{
    public class MaterialDataAccessor
    {
        public static RequestResult DeletePhoto(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var material = db.MATERIAL.First(x => x.UNIQUEID == UniqueID);

                    material.EXTENSION = string.Empty;

                    db.SaveChanges();

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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var material = db.MATERIAL.First(x => x.UNIQUEID == UniqueID);

                    material.EXTENSION = Extension;

                    db.SaveChanges();

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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.MATERIAL.Where(x => downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID) && Account.QueryableOrganizationUniqueIDList.Contains(x.ORGANIZATIONUNIQUEID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.EquipmentType))
                    {
                        query = query.Where(x => x.ORGANIZATIONUNIQUEID == Parameters.OrganizationUniqueID && x.EQUIPMENTTYPE == Parameters.EquipmentType);
                    }

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.ID.Contains(Parameters.Keyword) || x.NAME.Contains(Parameters.Keyword));
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
                            Permission = Account.OrganizationPermission(x.ORGANIZATIONUNIQUEID),
                            UniqueID = x.UNIQUEID,
                            ID = x.ID,
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.ORGANIZATIONUNIQUEID),
                            EquipmentType = x.EQUIPMENTTYPE,
                            Name = x.NAME,
                            Quantity = x.QUANTITY.Value
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var material = db.MATERIAL.First(x => x.UNIQUEID == UniqueID);

                    var model = new DetailViewModel()
                    {
                        Permission = Account.OrganizationPermission(material.ORGANIZATIONUNIQUEID),
                        UniqueID = material.UNIQUEID,
                        OrganizationUniqueID = material.ORGANIZATIONUNIQUEID,
                        ParentOrganizationDescription = OrganizationDataAccessor.GetOrganizationFullDescription(material.ORGANIZATIONUNIQUEID),
                        EquipmentType = material.EQUIPMENTTYPE,
                        ID = material.ID,
                        Name = material.NAME,
                        Quantity = material.QUANTITY.Value,
                        Extension = material.EXTENSION,
                        SpecList = (from x in db.MATERIALSPECVALUE
                                    join s in db.MATERIALSPEC
                                    on x.SPECUNIQUEID equals s.UNIQUEID
                                    where x.MATERIALUNIQUEID == material.UNIQUEID
                                    select new SpecModel
                                    {
                                        UniqueID = s.UNIQUEID,
                                        Description = s.DESCRIPTION,
                                        OptionUniqueID = x.SPECOPTIONUNIQUEID,
                                        Value = x.VALUE,
                                        Seq = x.SEQ.Value,
                                        OptionList = db.MATERIALSPECOPTION.Where(o => o.SPECUNIQUEID == s.UNIQUEID).Select(o => new Models.EquipmentMaintenance.MaterialManagement.MaterialSpecOption
                                        {
                                         SpecUniqueID=o.SPECUNIQUEID,
                                          Seq=o.SEQ.Value,
                                           Description=o.DESCRIPTION,
                                            UniqueID=o.UNIQUEID
                                        }).OrderBy(o => o.Seq).ToList()
                                    }).OrderBy(x => x.Seq).ToList()
                    };

                    var query = db.EQUIPMENTMATERIAL.Where(x => x.MATERIALUNIQUEID == material.UNIQUEID).Select(x => new
                    {
                        x.EQUIPMENTUNIQUEID,
                        x.PARTUNIQUEID,
                        Quantity= x.QUANTITY.Value
                    }).ToList();

                    foreach (var q in query)
                    {
                        var equipment = db.EQUIPMENT.FirstOrDefault(x => x.UNIQUEID == q.EQUIPMENTUNIQUEID);

                        if (equipment != null)
                        {
                            var equipmentModel = new EquipmentModel()
                            {
                                OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(equipment.ORGANIZATIONUNIQUEID),
                                EquipmentID = equipment.ID,
                                EquipmentName = equipment.NAME,
                                Quantity = q.Quantity
                            };

                            var part = db.EQUIPMENTPART.FirstOrDefault(x => x.EQUIPMENTUNIQUEID == q.EQUIPMENTUNIQUEID && x.UNIQUEID == q.PARTUNIQUEID);

                            if (part != null)
                            {
                                equipmentModel.PartDescription = part.DESCRIPTION;
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

                    model.EquipmentTypeSelectItemList.AddRange(db.MATERIAL.Where(x => upStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)).Select(x => x.EQUIPMENTTYPE).Distinct().OrderBy(x => x).Select(x => new SelectListItem
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
                    var material = db.MATERIAL.First(x => x.UNIQUEID == UniqueID);

                    var model = new CreateFormModel()
                    {
                        OrganizationUniqueID = material.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(material.ORGANIZATIONUNIQUEID),
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
                            EquipmentType = material.EQUIPMENTTYPE
                        },
                        SpecList = (from x in db.MATERIALSPECVALUE
                                    join s in db.MATERIALSPEC
                                    on x.SPECUNIQUEID equals s.UNIQUEID
                                    where x.MATERIALUNIQUEID == material.UNIQUEID
                                    select new SpecModel
                                    {
                                        UniqueID = s.UNIQUEID,
                                        Description = s.DESCRIPTION,
                                        OptionUniqueID = x.SPECOPTIONUNIQUEID,
                                        Value = x.VALUE,
                                        Seq = x.SEQ.Value,
                                        OptionList = db.MATERIALSPECOPTION.Where(o => o.SPECUNIQUEID == s.UNIQUEID).Select(o => new Models.EquipmentMaintenance.MaterialManagement.MaterialSpecOption
                                        {
                                            SpecUniqueID = o.SPECUNIQUEID,
                                            Seq = o.SEQ.Value,
                                            Description = o.DESCRIPTION,
                                            UniqueID = o.UNIQUEID
                                        }).OrderBy(o => o.Seq).ToList()
                                    }).OrderBy(x => x.Seq).ToList()
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(material.ORGANIZATIONUNIQUEID, true);

                    model.EquipmentTypeSelectItemList.AddRange(db.MATERIAL.Where(x => upStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)).Select(x => x.EQUIPMENTTYPE).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    model.EquipmentTypeSelectItemList.First(x => x.Value == material.EQUIPMENTTYPE).Selected = true;

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
                        var exists = db.MATERIAL.FirstOrDefault(x => x.ORGANIZATIONUNIQUEID == Model.OrganizationUniqueID && x.EQUIPMENTTYPE == Model.FormInput.EquipmentType && x.ID == Model.FormInput.ID);

                        if (exists == null)
                        {
                            string uniqueID = Guid.NewGuid().ToString();

                            db.MATERIAL.Add(new MATERIAL()
                            {
                                UNIQUEID = uniqueID,
                                ORGANIZATIONUNIQUEID = Model.OrganizationUniqueID,
                                EQUIPMENTTYPE = Model.FormInput.EquipmentType,
                                ID = Model.FormInput.ID,
                                NAME = Model.FormInput.Name,
                                QUANTITY = Model.FormInput.Quantity.HasValue ? Model.FormInput.Quantity.Value : 0
                            });

                            db.MATERIALSPECVALUE.AddRange(Model.SpecList.Select(x => new MATERIALSPECVALUE
                            {
                                MATERIALUNIQUEID = uniqueID,
                                SPECUNIQUEID = x.UniqueID,
                                SPECOPTIONUNIQUEID = x.OptionUniqueID,
                                VALUE = x.Value,
                                SEQ = x.Seq
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var material = db.MATERIAL.First(x => x.UNIQUEID == UniqueID);

                    var model = new EditFormModel()
                    {
                        UniqueID = material.UNIQUEID,
                        OrganizationUniqueID = material.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(material.ORGANIZATIONUNIQUEID),
                        Extension = material.EXTENSION,
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
                            EquipmentType = material.EQUIPMENTTYPE,
                            ID = material.ID,
                            Name = material.NAME,
                            Quantity = material.QUANTITY.Value
                        },
                        SpecList = (from x in db.MATERIALSPECVALUE
                                    join s in db.MATERIALSPEC
                                    on x.SPECUNIQUEID equals s.UNIQUEID
                                    where x.MATERIALUNIQUEID == material.UNIQUEID
                                    select new SpecModel
                                    {
                                        UniqueID = s.UNIQUEID,
                                        Description = s.DESCRIPTION,
                                        OptionUniqueID = x.SPECOPTIONUNIQUEID,
                                        Value = x.VALUE,
                                        Seq = x.SEQ.Value,
                                        OptionList = db.MATERIALSPECOPTION.Where(o => o.SPECUNIQUEID == s.UNIQUEID).Select(o => new Models.EquipmentMaintenance.MaterialManagement.MaterialSpecOption
                                        {
                                            UniqueID = o.UNIQUEID,
                                            Description = o.DESCRIPTION,
                                            Seq = o.SEQ.Value,
                                            SpecUniqueID = o.SPECUNIQUEID
                                        }).OrderBy(o => o.Description).ToList()
                                    }).OrderBy(x => x.Seq).ToList()
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(model.OrganizationUniqueID, true);

                    model.EquipmentTypeSelectItemList.AddRange(db.MATERIAL.Where(x => upStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)).Select(x => x.EQUIPMENTTYPE).Distinct().OrderBy(x => x).Select(x => new SelectListItem
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

                        var material = db.MATERIAL.First(x => x.UNIQUEID == Model.UniqueID);

                        var exists = db.MATERIAL.FirstOrDefault(x => x.UNIQUEID != material.UNIQUEID && x.ORGANIZATIONUNIQUEID == material.ORGANIZATIONUNIQUEID && x.EQUIPMENTTYPE == Model.FormInput.EquipmentType && x.ID == Model.FormInput.ID);

                        if (exists == null)
                        {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                            #region Material
                            material.EQUIPMENTTYPE = Model.FormInput.EquipmentType;
                            material.ID = Model.FormInput.ID;
                            material.NAME = Model.FormInput.Name;
                            //material.Quantity = Model.FormInput.Quantity.HasValue ? Model.FormInput.Quantity.Value : 0;

                            db.SaveChanges();
                            #endregion

                            #region MaterialSpecValue
                            #region Delete
                            db.MATERIALSPECVALUE.RemoveRange(db.MATERIALSPECVALUE.Where(x => x.MATERIALUNIQUEID == Model.UniqueID).ToList());

                            db.SaveChanges();
                            #endregion

                            #region Insert
                            db.MATERIALSPECVALUE.AddRange(Model.SpecList.Select(x => new MATERIALSPECVALUE
                            {
                                MATERIALUNIQUEID = Model.UniqueID,
                                SPECUNIQUEID = x.UniqueID,
                                SPECOPTIONUNIQUEID = x.OptionUniqueID,
                                VALUE = x.Value,
                                SEQ = x.Seq
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
                using (ASEDbEntities db = new ASEDbEntities())
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (string.IsNullOrEmpty(EquipmentType))
                    {
                        if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                        {
                            var equipmentTypeList = db.MATERIAL.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).Select(x => x.EQUIPMENTTYPE).Distinct().OrderBy(x => x).ToList();

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

                            var haveMaterial = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.MATERIAL.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID);

                            if (haveDownStreamOrganization || haveMaterial)
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else
                    {
                        var materialList = db.MATERIAL.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID && x.EQUIPMENTTYPE == EquipmentType).OrderBy(x => x.ID).ToList();

                        foreach (var material in materialList)
                        {
                            var treeItem = new TreeItem() { Title = material.NAME };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Material.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", material.ID, material.NAME);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = material.ORGANIZATIONUNIQUEID;
                            attributes[Define.EnumTreeAttribute.EquipmentType] = material.EQUIPMENTTYPE;
                            attributes[Define.EnumTreeAttribute.MaterialUniqueID] = material.UNIQUEID;

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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var haveDownStreamOrganization = OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID));

                    var haveMaterial = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.MATERIAL.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID);

                    if (haveDownStreamOrganization || haveMaterial)
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
                using (ASEDbEntities db = new ASEDbEntities())
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
                                var spec = db.MATERIALSPEC.First(x => x.UNIQUEID == specUniqueID);

                                SpecList.Add(new SpecModel()
                                {
                                    UniqueID = spec.UNIQUEID,
                                    Description = spec.DESCRIPTION,
                                    Seq = SpecList.Count+1,
                                    OptionList = db.MATERIALSPECOPTION.Where(x => x.SPECUNIQUEID == spec.UNIQUEID).Select(x => new Models.EquipmentMaintenance.MaterialManagement.MaterialSpecOption
                                    {
                                     UniqueID = x.UNIQUEID,
                                      Description=x.DESCRIPTION,
                                      Seq = x.SEQ.Value,
                                       SpecUniqueID=x.SPECUNIQUEID
                                    }).OrderBy(x => x.Description).ToList()
                                });
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(materialType))
                            {
                                var specList = db.MATERIALSPEC.Where(x => x.ORGANIZATIONUNIQUEID == organizationUniqueID && x.MATERIALTYPE == materialType).ToList();

                                foreach (var spec in specList)
                                {
                                    if (!SpecList.Any(x => x.UniqueID == spec.UNIQUEID))
                                    {
                                        SpecList.Add(new SpecModel()
                                        {
                                            UniqueID = spec.UNIQUEID,
                                            Description = spec.DESCRIPTION,
                                            Seq = SpecList.Count + 1,
                                            OptionList = db.MATERIALSPECOPTION.Where(x => x.SPECUNIQUEID == spec.UNIQUEID).Select(x => new Models.EquipmentMaintenance.MaterialManagement.MaterialSpecOption
                                            {
                                                UniqueID = x.UNIQUEID,
                                                Description = x.DESCRIPTION,
                                                Seq = x.SEQ.Value,
                                                SpecUniqueID = x.SPECUNIQUEID
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
                                        var specList = db.MATERIALSPEC.Where(x => x.ORGANIZATIONUNIQUEID == downStreamOrganization).ToList();

                                        foreach (var spec in specList)
                                        {
                                            if (!SpecList.Any(x => x.UniqueID == spec.UNIQUEID))
                                            {
                                                SpecList.Add(new SpecModel()
                                                {
                                                    UniqueID = spec.UNIQUEID,
                                                    Description = spec.DESCRIPTION,
                                                    Seq = SpecList.Count + 1,
                                                    OptionList = db.MATERIALSPECOPTION.Where(x => x.SPECUNIQUEID == spec.UNIQUEID).Select(x => new Models.EquipmentMaintenance.MaterialManagement.MaterialSpecOption
                                                    {
                                                        UniqueID = x.UNIQUEID,
                                                        Description = x.DESCRIPTION,
                                                        Seq = x.SEQ.Value,
                                                        SpecUniqueID = x.SPECUNIQUEID
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (string.IsNullOrEmpty(EquipmentType))
                    {
                        var equipmentTypeList = db.MATERIAL.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).Select(x => x.EQUIPMENTTYPE).Distinct().OrderBy(x => x).ToList();

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

                            if (db.MATERIAL.Any(x => downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID) && availableOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)))
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
                        var materialList = db.MATERIAL.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID && x.EQUIPMENTTYPE == EquipmentType).OrderBy(x => x.ID).ToList();

                        foreach (var material in materialList)
                        {
                            var treeItem = new TreeItem() { Title = material.NAME };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Material.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", material.ID, material.NAME);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = material.ORGANIZATIONUNIQUEID;
                            attributes[Define.EnumTreeAttribute.EquipmentType] = material.EQUIPMENTTYPE;
                            attributes[Define.EnumTreeAttribute.MaterialUniqueID] = material.UNIQUEID;

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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (!string.IsNullOrEmpty(EquipmentUniqueID))
                    {
                        var materialList = (from x in db.EQUIPMENTMATERIAL
                                            join m in db.MATERIAL
                                            on x.MATERIALUNIQUEID equals m.UNIQUEID
                                            where x.EQUIPMENTUNIQUEID == EquipmentUniqueID && x.PARTUNIQUEID == PartUniqueID
                                            select m).OrderBy(x => x.ID).ToList();

                        foreach (var material in materialList)
                        {
                            var treeItem = new TreeItem() { Title = material.NAME };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Material.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", material.ID, material.NAME);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = material.ORGANIZATIONUNIQUEID;
                            attributes[Define.EnumTreeAttribute.EquipmentType] = material.EQUIPMENTTYPE;
                            attributes[Define.EnumTreeAttribute.MaterialUniqueID] = material.UNIQUEID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else  if (string.IsNullOrEmpty(EquipmentType))
                    {
                        var equipmentTypeList = db.MATERIAL.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).Select(x => x.EQUIPMENTTYPE).Distinct().OrderBy(x => x).ToList();

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

                            if (db.MATERIAL.Any(x => downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID) && availableOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)))
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
                        var materialList = db.MATERIAL.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID && x.EQUIPMENTTYPE == EquipmentType).OrderBy(x => x.ID).ToList();

                        foreach (var material in materialList)
                        {
                            var treeItem = new TreeItem() { Title = material.NAME };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Material.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", material.ID, material.NAME);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = material.ORGANIZATIONUNIQUEID;
                            attributes[Define.EnumTreeAttribute.EquipmentType] = material.EQUIPMENTTYPE;
                            attributes[Define.EnumTreeAttribute.MaterialUniqueID] = material.UNIQUEID;

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

        public static RequestResult ExportQRCode(List<Models.Shared.UserModel> UserList, List<string> UniqueIDList, Account Account, Define.EnumExcelVersion ExcelVersion, string fileName)
        {
            RequestResult result = new RequestResult();
            try
            {
                IWorkbook wk = null;


                if (ExcelVersion == Define.EnumExcelVersion._2003)
                {
                    wk = new HSSFWorkbook();
                }
                else
                {
                    wk = new XSSFWorkbook();
                }

                ISheet sheet = wk.CreateSheet(Resources.Resource.CalibrationForm);

                // set zoom size
                sheet.SetZoom(2, 1);


                //側邊標題
                ICellStyle titleCellStyle = wk.CreateCellStyle();
                IFont titleFont = wk.CreateFont();
                titleFont.FontName = "Trebuchet MS";
                titleFont.FontHeightInPoints = 8;
                titleFont.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
                titleCellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                titleCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
                titleCellStyle.SetFont(titleFont);


                //內容
                ICellStyle contentCellStyle = wk.CreateCellStyle();
                IFont contentFont = wk.CreateFont();
                contentFont.FontName = "Trebuchet MS";
                contentFont.FontHeightInPoints = 7;
                contentFont.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
                contentFont.Underline = FontUnderlineType.Single;
                contentCellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                contentCellStyle.SetFont(contentFont);

                var index = 0;
                //var startColumnForPrint = 0;
                //var endColumnForPrint = 2;
                //var startRowForPrint = 0;//dynamic
                var endRowForPrint = 0;//dynamic




                // TODO should move to other place

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = db.MATERIAL.Where(x => UniqueIDList.Contains(x.UNIQUEID)).Select(x => new
                    {

                        x.ID,
                        x.NAME
                    });



                    //int columnHeightUnit = 20;
                    //double contentHeight = 9.75;




                    //1~23 one size
                    int page = 1;
                    foreach (var item in query)
                    {
                        // create empty row
                        var startIdx = index;
                        var endIdx = (index + 22);
                        //_logger.Info("startIdx:{0} ~ endIdx:{1}", startIdx, endIdx);

                        // create empty row
                        for (int i = startIdx; i <= endIdx; i++)
                        {
                            var row = sheet.CreateRow(index);

                            //Create cell
                            for (int j = 0; j <= 8; j++)
                            {
                                row.CreateCell(j);
                            }

                            //var row = sheet.CreateRow(index);
                            //_logger.Debug("index:{0}", i);
                            index = i;

                        }

                        var qrSIdxRow = startIdx + 2;
                        var qrEIdxRow = startIdx + 15;
                        var idIdxRow = startIdx + 17;
                        var nameRow = startIdx + 20;

                        //_logger.Warn("qrSIdxRow:{0}", qrSIdxRow);
                        //_logger.Warn("qrEIdxRow:{0}", qrEIdxRow);
                        //_logger.Warn("idIdxRow:{0}", idIdxRow);
                        //_logger.Warn("nameRow:{0}", nameRow);
                        var barcodeWriter = new BarcodeWriter
                        {
                            Format = BarcodeFormat.QR_CODE,
                            Options = new EncodingOptions
                            {
                                Height = 450,
                                Width = 450,
                                Margin = 1

                            }
                        };
                        int pictureIdx = 0;
                        var path = Path.Combine(Config.TempFolder, "EQUIP_" + Guid.NewGuid().ToString() + ".jpg");
                        using (var bitmap = barcodeWriter.Write(item.ID))
                        using (var stream = new MemoryStream())
                        {


                            bitmap.Save(path);
                            byte[] bytes = System.IO.File.ReadAllBytes(path);
                            pictureIdx = wk.AddPicture(bytes, PictureType.JPEG);
                        }

                        IDrawing drawing = sheet.CreateDrawingPatriarch();
                        IClientAnchor anchor = drawing.CreateAnchor(0, 0, 0, 0, 2, qrSIdxRow, 6, qrEIdxRow);


                        //ref http://www.cnblogs.com/firstcsharp/p/4896121.html

                        if (anchor != null)
                        {
                            anchor.AnchorType = AnchorType.MoveDontResize;
                            drawing.CreatePicture(anchor, pictureIdx);
                        }

                        index += 4;


                        var row_id = sheet.GetRow(idIdxRow);
                        row_id.GetCell(2).SetCellValue(item.ID);


                        var row_name = sheet.GetRow(nameRow);
                        row_name.GetCell(2).SetCellValue(item.NAME);

                        page++;
                        index++;


                    }
                    endRowForPrint = index > 1 ? index - 1 : index;
                    //定欄位單位寬度



                }

                

                //Save file
                var savePath = Path.Combine(Config.TempFolder, fileName);
                using (FileStream file = new FileStream(savePath, FileMode.Create))
                {
                    wk.Write(file);
                    file.Close();
                }



                result.Data = fileName;
                result.IsSuccess = true;
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
