using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.ASE;
using Models.Shared;
using Models.Authenticated;
using Models.EquipmentMaintenance.EquipmentManagement;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;
using System.IO;
using ZXing;
using ZXing.Common;
using NPOI.OpenXmlFormats.Spreadsheet;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.ASE
{
    public class EquipmentDataAccessor
    {
        public static RequestResult UploadPhoto(string UniqueID, string Extension)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var equipment = db.EQUIPMENT.First(x => x.UNIQUEID == UniqueID);

                    equipment.EXTENSION = Extension;

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

        public static RequestResult DeletePhoto(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var equipment = db.EQUIPMENT.First(x => x.UNIQUEID == UniqueID);

                    equipment.EXTENSION = string.Empty;

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

                    var query = db.EQUIPMENT.Where(x => downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID) && Account.QueryableOrganizationUniqueIDList.Contains(x.ORGANIZATIONUNIQUEID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.ID.Contains(Parameters.Keyword) || x.NAME.Contains(Parameters.Keyword));
                    }

                    var organization = OrganizationDataAccessor.GetOrganization(Parameters.OrganizationUniqueID);

                    result.ReturnData(new GridViewModel()
                    {
                        OrganizationUniqueID = Parameters.OrganizationUniqueID,
                        Permission = Account.OrganizationPermission(Parameters.OrganizationUniqueID),
                        OrganizationDescription = organization.Description,
                        FullOrganizationDescription = organization.FullDescription,
                        ItemList = query.ToList().Select(x => new GridItem()
                        {
                            UniqueID = x.UNIQUEID,
                            Permission = Account.OrganizationPermission(x.ORGANIZATIONUNIQUEID),
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.ORGANIZATIONUNIQUEID),
                            MaintenanceOrganization = OrganizationDataAccessor.GetOrganizationDescription(x.PMORGANIZATIONUNIQUEID),
                            ID = x.ID,
                            Name = x.NAME
                        }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.ID).ToList()
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
                    var equipment = db.EQUIPMENT.First(x => x.UNIQUEID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = equipment.UNIQUEID,
                        Permission = Account.OrganizationPermission(equipment.ORGANIZATIONUNIQUEID),
                        OrganizationUniqueID = equipment.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(equipment.ORGANIZATIONUNIQUEID),
                        ID = equipment.ID,
                        Name = equipment.NAME,
                        MaintenanceOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(equipment.PMORGANIZATIONUNIQUEID),
                        Extension = equipment.EXTENSION,
                        SpecList = (from x in db.EQUIPMENTSPECVALUE
                                    join s in db.EQUIPMENTSPEC
                                    on x.SPECUNIQUEID equals s.UNIQUEID
                                    where x.EQUIPMENTUNIQUEID == equipment.UNIQUEID
                                    select new SpecModel
                                    {
                                        UniqueID = s.UNIQUEID,
                                        Description = s.DESCRIPTION,
                                        OptionUniqueID = x.SPECOPTIONUNIQUEID,
                                        Value = x.VALUE,
                                        Seq = x.SEQ.Value,
                                        OptionList = db.EQUIPMENTSPECOPTION.Where(o => o.SPECUNIQUEID == s.UNIQUEID).Select(o => new Models.EquipmentMaintenance.EquipmentManagement.EquipmentSpecOption
                                        {
                                            Seq = o.SEQ.Value,
                                            SpecUniqueID = o.SPECUNIQUEID,
                                            Description = o.DESCRIPTION,
                                            UniqueID = o.UNIQUEID
                                        }).OrderBy(o => o.Seq).ToList()
                                    }).OrderBy(x => x.Seq).ToList(),
                        MaterialList = (from x in db.EQUIPMENTMATERIAL
                                        join m in db.MATERIAL
                                        on x.MATERIALUNIQUEID equals m.UNIQUEID
                                        where x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.PARTUNIQUEID == "*"
                                        select new MaterialModel
                                        {
                                            UniqueID = m.UNIQUEID,
                                            ID = m.ID,
                                            Name = m.NAME,
                                            Quantity = x.QUANTITY.Value
                                        }).OrderBy(x => x.ID).ToList(),
                        PartList = (from p in db.EQUIPMENTPART
                                    where p.EQUIPMENTUNIQUEID == equipment.UNIQUEID
                                    select new PartModel
                                    {
                                        UniqueID = p.UNIQUEID,
                                        Description = p.DESCRIPTION,
                                        MaterialList = (from x in db.EQUIPMENTMATERIAL
                                                        join m in db.MATERIAL
                                                        on x.MATERIALUNIQUEID equals m.UNIQUEID
                                                        where x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.PARTUNIQUEID == p.UNIQUEID
                                                        select new MaterialModel
                                                        {
                                                            UniqueID = m.UNIQUEID,
                                                            ID = m.ID,
                                                            Name = m.NAME,
                                                            Quantity = x.QUANTITY.Value
                                                        }).OrderBy(x => x.ID).ToList()
                                    }).OrderBy(x => x.Description).ToList()
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

        public static RequestResult GetCopyFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var equipment = db.EQUIPMENT.First(x => x.UNIQUEID == UniqueID);

                    var organization = OrganizationDataAccessor.GetOrganization(equipment.ORGANIZATIONUNIQUEID);

                    var maintenanceOrganization = OrganizationDataAccessor.GetOrganization(equipment.PMORGANIZATIONUNIQUEID);

                    result.ReturnData(new CreateFormModel()
                    {
                        UniqueID = Guid.NewGuid().ToString(),
                        AncestorOrganizationUniqueID = organization.AncestorOrganizationUniqueID,
                        OrganizationUniqueID = equipment.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = organization.FullDescription,
                        MaintenanceOrganizationID = maintenanceOrganization != null ? maintenanceOrganization.ID : string.Empty,
                        MaintenanceOrganizationDescription = maintenanceOrganization != null ? maintenanceOrganization.Description : string.Empty,
                        SpecList = (from x in db.EQUIPMENTSPECVALUE
                                    join s in db.EQUIPMENTSPEC
                                    on x.SPECUNIQUEID equals s.UNIQUEID
                                    where x.EQUIPMENTUNIQUEID == equipment.UNIQUEID
                                    select new
                                    {
                                        s.UNIQUEID,
                                        s.DESCRIPTION,
                                        x.SPECOPTIONUNIQUEID,
                                        x.VALUE,
                                        x.SEQ
                                    }).ToList().Select(x => new SpecModel
                                    {
                                        UniqueID = x.UNIQUEID,
                                        Description = x.DESCRIPTION,
                                        OptionUniqueID = x.SPECOPTIONUNIQUEID,
                                        Value = x.VALUE,
                                        Seq = x.SEQ.Value,
                                        OptionList = db.EQUIPMENTSPECOPTION.Where(o => o.SPECUNIQUEID == x.UNIQUEID).ToList().Select(o => new Models.EquipmentMaintenance.EquipmentManagement.EquipmentSpecOption
                                        {
                                            UniqueID = o.UNIQUEID,
                                            Description = o.DESCRIPTION,
                                            SpecUniqueID = o.SPECUNIQUEID,
                                            Seq = o.SEQ.Value
                                        }).OrderBy(o => o.Seq).ToList()
                                    }).OrderBy(x => x.Seq).ToList(),
                        MaterialList = (from x in db.EQUIPMENTMATERIAL
                                        join m in db.MATERIAL
                                        on x.MATERIALUNIQUEID equals m.UNIQUEID
                                        where x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.PARTUNIQUEID == "*"
                                        select new
                                        {
                                            m.UNIQUEID,
                                            m.ID,
                                            m.NAME,
                                            x.QUANTITY
                                        }).ToList().Select(x => new MaterialModel
                                        {
                                            UniqueID = x.UNIQUEID,
                                            ID = x.ID,
                                            Name = x.NAME,
                                            Quantity = x.QUANTITY.Value
                                        }).OrderBy(x => x.ID).ToList(),
                        PartList = (from p in db.EQUIPMENTPART
                                    where p.EQUIPMENTUNIQUEID == equipment.UNIQUEID
                                    select new
                                    {
                                        p.UNIQUEID,
                                        p.DESCRIPTION
                                    }).ToList().Select(x => new PartModel
                                    {
                                        UniqueID = Guid.NewGuid().ToString(),
                                        Description = x.DESCRIPTION,
                                        MaterialList = (from y in db.EQUIPMENTMATERIAL
                                                        join m in db.MATERIAL
                                                        on y.MATERIALUNIQUEID equals m.UNIQUEID
                                                        where y.EQUIPMENTUNIQUEID == equipment.UNIQUEID && y.PARTUNIQUEID == x.UNIQUEID
                                                        select new
                                                        {
                                                            m.UNIQUEID,
                                                            m.ID,
                                                            m.NAME,
                                                            y.QUANTITY
                                                        }).ToList().Select(y => new MaterialModel
                                                        {
                                                            UniqueID = y.UNIQUEID,
                                                            ID = y.ID,
                                                            Name = y.NAME,
                                                            Quantity = y.QUANTITY.Value
                                                        }).OrderBy(y => y.ID).ToList()
                                    }).OrderBy(x => x.Description).ToList(),
                        FormInput = new FormInput()
                        {
                            Name = equipment.NAME,
                            MaintenanceOrganizationUniqueID = equipment.PMORGANIZATIONUNIQUEID
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

        public static RequestResult Create(CreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var exists = db.EQUIPMENT.FirstOrDefault(x => x.ORGANIZATIONUNIQUEID == Model.OrganizationUniqueID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
                        db.EQUIPMENT.Add(new EQUIPMENT()
                        {
                            UNIQUEID = Model.UniqueID,
                            ORGANIZATIONUNIQUEID = Model.OrganizationUniqueID,
                            ID = Model.FormInput.ID,
                            NAME = Model.FormInput.Name,
                            ISFEELITEMDEFAULTNORMAL = "N",
                            PMORGANIZATIONUNIQUEID = Model.FormInput.MaintenanceOrganizationUniqueID,
                            LASTMODIFYTIME = DateTime.Now
                        });

                        db.EQUIPMENTSPECVALUE.AddRange(Model.SpecList.Select(x => new EQUIPMENTSPECVALUE
                        {
                            EQUIPMENTUNIQUEID = Model.UniqueID,
                            SPECUNIQUEID = x.UniqueID,
                            SPECOPTIONUNIQUEID = x.OptionUniqueID,
                            VALUE = x.Value,
                            SEQ = x.Seq
                        }).ToList());

                        db.EQUIPMENTMATERIAL.AddRange(Model.MaterialList.Select(x => new EQUIPMENTMATERIAL
                        {
                            EQUIPMENTUNIQUEID = Model.UniqueID,
                            PARTUNIQUEID = "*",
                            MATERIALUNIQUEID = x.UniqueID,
                            QUANTITY = x.Quantity
                        }).ToList());

                        foreach (var part in Model.PartList)
                        {
                            db.EQUIPMENTPART.Add(new EQUIPMENTPART()
                            {
                                UNIQUEID = part.UniqueID,
                                EQUIPMENTUNIQUEID = Model.UniqueID,
                                DESCRIPTION = part.Description
                            });

                            db.EQUIPMENTMATERIAL.AddRange(part.MaterialList.Select(x => new EQUIPMENTMATERIAL
                            {
                                EQUIPMENTUNIQUEID = Model.UniqueID,
                                PARTUNIQUEID = part.UniqueID,
                                MATERIALUNIQUEID = x.UniqueID,
                                QUANTITY = x.Quantity
                            }).ToList());
                        }

                        db.SaveChanges();

                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.Equipment, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.EquipmentID, Resources.Resource.Exists));
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
                    var equipment = db.EQUIPMENT.First(x => x.UNIQUEID == UniqueID);

                    var organization = OrganizationDataAccessor.GetOrganization(equipment.ORGANIZATIONUNIQUEID);

                    var maintenanceOrganization = OrganizationDataAccessor.GetOrganization(equipment.PMORGANIZATIONUNIQUEID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = equipment.UNIQUEID,
                        AncestorOrganizationUniqueID = organization.AncestorOrganizationUniqueID,
                        OrganizationUniqueID = equipment.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = organization.FullDescription,
                        MaintenanceOrganizationID = maintenanceOrganization != null ? maintenanceOrganization.ID : string.Empty,
                        MaintenanceOrganizationDescription = maintenanceOrganization != null ? maintenanceOrganization.Description : string.Empty,
                        Extension = equipment.EXTENSION,
                        SpecList = (from x in db.EQUIPMENTSPECVALUE
                                    join s in db.EQUIPMENTSPEC
                                    on x.SPECUNIQUEID equals s.UNIQUEID
                                    where x.EQUIPMENTUNIQUEID == equipment.UNIQUEID
                                    select new SpecModel
                                    {
                                        UniqueID = s.UNIQUEID,
                                        Description = s.DESCRIPTION,
                                        OptionUniqueID = x.SPECOPTIONUNIQUEID,
                                        Value = x.VALUE,
                                        Seq = x.SEQ.Value,
                                        OptionList = db.EQUIPMENTSPECOPTION.Where(o => o.SPECUNIQUEID == s.UNIQUEID).Select(o => new Models.EquipmentMaintenance.EquipmentManagement.EquipmentSpecOption
                                        {
                                            UniqueID = o.UNIQUEID,
                                            Description = o.DESCRIPTION,
                                            SpecUniqueID = o.SPECUNIQUEID,
                                            Seq = o.SEQ.Value
                                        }).OrderBy(o => o.Seq).ToList()
                                    }).OrderBy(x => x.Seq).ToList(),
                        MaterialList = (from x in db.EQUIPMENTMATERIAL
                                        join m in db.MATERIAL
                                        on x.MATERIALUNIQUEID equals m.UNIQUEID
                                        where x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.PARTUNIQUEID == "*"
                                        select new MaterialModel
                                        {
                                            UniqueID = m.UNIQUEID,
                                            ID = m.ID,
                                            Name = m.NAME,
                                            Quantity = x.QUANTITY.Value
                                        }).OrderBy(x => x.ID).ToList(),
                        PartList = (from p in db.EQUIPMENTPART
                                    where p.EQUIPMENTUNIQUEID == equipment.UNIQUEID
                                    select new PartModel
                                    {
                                        UniqueID = p.UNIQUEID,
                                        Description = p.DESCRIPTION,
                                        MaterialList = (from x in db.EQUIPMENTMATERIAL
                                                        join m in db.MATERIAL
                                                        on x.MATERIALUNIQUEID equals m.UNIQUEID
                                                        where x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.PARTUNIQUEID == p.UNIQUEID
                                                        select new MaterialModel
                                                        {
                                                            UniqueID = m.UNIQUEID,
                                                            ID = m.ID,
                                                            Name = m.NAME,
                                                            Quantity = x.QUANTITY.Value
                                                        }).OrderBy(x => x.ID).ToList()
                                    }).OrderBy(x => x.Description).ToList(),
                        FormInput = new FormInput()
                        {
                            ID = equipment.ID,
                            Name = equipment.NAME,
                            MaintenanceOrganizationUniqueID = equipment.PMORGANIZATIONUNIQUEID
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var equipment = db.EQUIPMENT.First(x => x.UNIQUEID == Model.UniqueID);

                    var exists = db.EQUIPMENT.FirstOrDefault(x => x.ORGANIZATIONUNIQUEID == equipment.ORGANIZATIONUNIQUEID && x.UNIQUEID != equipment.UNIQUEID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                        #region Equipment
                        equipment.ID = Model.FormInput.ID;
                        equipment.NAME = Model.FormInput.Name;
                        equipment.PMORGANIZATIONUNIQUEID = Model.FormInput.MaintenanceOrganizationUniqueID;
                        equipment.LASTMODIFYTIME = DateTime.Now;

                        db.SaveChanges();
                        #endregion

                        #region EquipmentSpecValue
                        #region Delete
                        db.EQUIPMENTSPECVALUE.RemoveRange(db.EQUIPMENTSPECVALUE.Where(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID).ToList());

                        db.SaveChanges();
                        #endregion

                        #region Insert
                        db.EQUIPMENTSPECVALUE.AddRange(Model.SpecList.Select(x => new EQUIPMENTSPECVALUE
                        {
                            EQUIPMENTUNIQUEID = equipment.UNIQUEID,
                            SPECUNIQUEID = x.UniqueID,
                            SPECOPTIONUNIQUEID = x.OptionUniqueID,
                            VALUE = x.Value,
                            SEQ = x.Seq
                        }).ToList());

                        db.SaveChanges();
                        #endregion
                        #endregion

                        #region EquipmentPart, EquipmentMaterial
                        #region Delete
                        db.EQUIPMENTPART.RemoveRange(db.EQUIPMENTPART.Where(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID).ToList());
                        db.EQUIPMENTMATERIAL.RemoveRange(db.EQUIPMENTMATERIAL.Where(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID).ToList());

                        db.SaveChanges();
                        #endregion

                        #region Insert
                        db.EQUIPMENTMATERIAL.AddRange(Model.MaterialList.Select(x => new EQUIPMENTMATERIAL
                        {
                            EQUIPMENTUNIQUEID = equipment.UNIQUEID,
                            PARTUNIQUEID = "*",
                            MATERIALUNIQUEID = x.UniqueID,
                            QUANTITY = x.Quantity
                        }).ToList());

                        foreach (var part in Model.PartList)
                        {
                            db.EQUIPMENTPART.Add(new EQUIPMENTPART()
                            {
                                UNIQUEID = part.UniqueID,
                                EQUIPMENTUNIQUEID = equipment.UNIQUEID,
                                DESCRIPTION = part.Description
                            });

                            db.EQUIPMENTMATERIAL.AddRange(part.MaterialList.Select(x => new EQUIPMENTMATERIAL
                            {
                                EQUIPMENTUNIQUEID = equipment.UNIQUEID,
                                PARTUNIQUEID = part.UniqueID,
                                MATERIALUNIQUEID = x.UniqueID,
                                QUANTITY = x.Quantity
                            }).ToList());
                        }

                        db.SaveChanges();
                        #endregion
                        #endregion

                        #region EquipmentCheckItem, EquipmentStandard
                        var partList = Model.PartList.Select(x => x.UniqueID).ToList();

                        db.EQUIPMENTCHECKITEM.RemoveRange(db.EQUIPMENTCHECKITEM.Where(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.PARTUNIQUEID != "*" && !partList.Contains(x.PARTUNIQUEID)).ToList());
                        db.EQUIPMENTSTANDARD.RemoveRange(db.EQUIPMENTSTANDARD.Where(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.PARTUNIQUEID != "*" && !partList.Contains(x.PARTUNIQUEID)).ToList());

                        db.SaveChanges();
                        #endregion
#if !DEBUG
                        trans.Complete();
                    }
#endif
                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.Equipment, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.EquipmentID, Resources.Resource.Exists));
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
                    DeleteHelper.Equipment(db, SelectedList);

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.Equipment, Resources.Resource.Success));
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

        public static RequestResult SavePageState(List<MaterialModel> MaterialList, List<string> PageStateList)
        {
            RequestResult result = new RequestResult();

            try
            {
                foreach (string pageState in PageStateList)
                {
                    string[] temp = pageState.Split(Define.Seperators, StringSplitOptions.None);

                    string materialUniqueID = temp[0];
                    string qty = temp[1];

                    var material = MaterialList.First(x => x.UniqueID == materialUniqueID);

                    if (!string.IsNullOrEmpty(qty))
                    {
                        int q;

                        if (int.TryParse(qty, out q))
                        {
                            material.Quantity = q;
                        }
                        else
                        {
                            material.Quantity = 0;
                        }
                    }
                    else
                    {
                        material.Quantity = 0;
                    }
                }

                result.ReturnData(MaterialList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult SavePageState(List<PartModel> PartList, List<string> PageStateList)
        {
            RequestResult result = new RequestResult();

            try
            {
                foreach (string pageState in PageStateList)
                {
                    string[] temp = pageState.Split(Define.Seperators, StringSplitOptions.None);

                    string partUniqueID = temp[0];
                    string materialUniqueID = temp[1];
                    string qty = temp[2];

                    var material = PartList.First(x => x.UniqueID == partUniqueID).MaterialList.First(x => x.UniqueID == materialUniqueID);

                    if (!string.IsNullOrEmpty(qty))
                    {
                        int q;

                        if (int.TryParse(qty, out q))
                        {
                            material.Quantity = q;
                        }
                        else
                        {
                            material.Quantity = 0;
                        }
                    }
                    else
                    {
                        material.Quantity = 0;
                    }
                }

                result.ReturnData(PartList);
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
                        var equipmentType = temp[1];
                        var specUniqueID = temp[2];

                        if (!string.IsNullOrEmpty(specUniqueID))
                        {
                            if (!SpecList.Any(x => x.UniqueID == specUniqueID))
                            {
                                var spec = db.EQUIPMENTSPEC.First(x => x.UNIQUEID == specUniqueID);

                                SpecList.Add(new SpecModel()
                                {
                                    UniqueID = spec.UNIQUEID,
                                    Description = spec.DESCRIPTION,
                                    Seq = SpecList.Count + 1,
                                    OptionList = db.EQUIPMENTSPECOPTION.Where(o => o.SPECUNIQUEID == spec.UNIQUEID).Select(o => new Models.EquipmentMaintenance.EquipmentManagement.EquipmentSpecOption
                                    {
                                        UniqueID = o.UNIQUEID,
                                        Description = o.DESCRIPTION,
                                        SpecUniqueID = o.SPECUNIQUEID,
                                        Seq = o.SEQ.Value
                                    }).OrderBy(o => o.Seq).ToList()
                                });
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(equipmentType))
                            {
                                var specList = db.EQUIPMENTSPEC.Where(x => x.ORGANIZATIONUNIQUEID == organizationUniqueID && x.EQUIPMENTTYPE == equipmentType).ToList();

                                foreach (var spec in specList)
                                {
                                    if (!SpecList.Any(x => x.UniqueID == spec.UNIQUEID))
                                    {
                                        SpecList.Add(new SpecModel()
                                        {
                                            UniqueID = spec.UNIQUEID,
                                            Description = spec.DESCRIPTION,
                                            Seq = SpecList.Count+1,
                                            OptionList = db.EQUIPMENTSPECOPTION.Where(o => o.SPECUNIQUEID == spec.UNIQUEID).Select(o => new Models.EquipmentMaintenance.EquipmentManagement.EquipmentSpecOption
                                            {
                                                UniqueID = o.UNIQUEID,
                                                Description = o.DESCRIPTION,
                                                SpecUniqueID = o.SPECUNIQUEID,
                                                Seq = o.SEQ.Value
                                            }).OrderBy(o => o.Seq).ToList()
                                        });
                                    }
                                }
                            }
                            else
                            {
                                var availableOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(RefOrganizationUniqueID, true);

                                var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organizationUniqueID, true);

                                foreach (var organization in downStreamOrganizationList)
                                {
                                    if (availableOrganizationList.Any(x => x == organization))
                                    {
                                        var specList = db.EQUIPMENTSPEC.Where(x => x.ORGANIZATIONUNIQUEID == organization).ToList();

                                        foreach (var spec in specList)
                                        {
                                            if (!SpecList.Any(x => x.UniqueID == spec.UNIQUEID))
                                            {
                                                SpecList.Add(new SpecModel()
                                                {
                                                    UniqueID = spec.UNIQUEID,
                                                    Description = spec.DESCRIPTION,
                                                    Seq = SpecList.Count+1,
                                                    OptionList = db.EQUIPMENTSPECOPTION.Where(o => o.SPECUNIQUEID == spec.UNIQUEID).Select(o => new Models.EquipmentMaintenance.EquipmentManagement.EquipmentSpecOption
                                                    {
                                                        UniqueID = o.UNIQUEID,
                                                        Description = o.DESCRIPTION,
                                                        SpecUniqueID = o.SPECUNIQUEID,
                                                        Seq = o.SEQ.Value
                                                    }).OrderBy(o => o.Seq).ToList()
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
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

        public static RequestResult AddMaterial(List<MaterialModel> MaterialList, List<string> SelectedList, string RefOrganizationUniqueID)
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
                        var equipmentType = temp[1];
                        var materialUniqueID = temp[2];

                        if (!string.IsNullOrEmpty(materialUniqueID))
                        {
                            if (!MaterialList.Any(x => x.UniqueID == materialUniqueID))
                            {
                                var material = db.MATERIAL.First(x => x.UNIQUEID == materialUniqueID);

                                MaterialList.Add(new MaterialModel()
                                {
                                    UniqueID = material.UNIQUEID,
                                    ID = material.ID,
                                    Name = material.NAME,
                                    Quantity = 0
                                });
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(equipmentType))
                            {
                                var materialList = db.MATERIAL.Where(x => x.ORGANIZATIONUNIQUEID == organizationUniqueID && x.EQUIPMENTTYPE == equipmentType).ToList();

                                foreach (var material in materialList)
                                {
                                    if (!MaterialList.Any(x => x.UniqueID == material.UNIQUEID))
                                    {
                                        MaterialList.Add(new MaterialModel()
                                        {
                                            UniqueID = material.UNIQUEID,
                                            ID = material.ID,
                                            Name = material.NAME,
                                            Quantity = 0
                                        });
                                    }
                                }
                            }
                            else
                            {
                                var availableOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(RefOrganizationUniqueID, true);

                                var organizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organizationUniqueID, true);

                                foreach (var organization in organizationList)
                                {
                                    if (availableOrganizationList.Any(x => x == organization))
                                    {
                                        var materialList = db.MATERIAL.Where(x => x.ORGANIZATIONUNIQUEID == organization).ToList();

                                        foreach (var material in materialList)
                                        {
                                            if (!MaterialList.Any(x => x.UniqueID == material.UNIQUEID))
                                            {
                                                MaterialList.Add(new MaterialModel()
                                                {
                                                    UniqueID = material.UNIQUEID,
                                                    ID = material.ID,
                                                    Name = material.NAME,
                                                    Quantity = 0
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                result.ReturnData(MaterialList.OrderBy(x => x.ID).ToList());
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetTreeItem(List<Models.Shared.Organization> OrganizationList, string OrganizationUniqueID, Account Account)
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
                    { Define.EnumTreeAttribute.EquipmentUniqueID, string.Empty }
                };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                    {
                        var equipmentList = db.EQUIPMENT.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                        foreach (var equipment in equipmentList)
                        {
                            var treeItem = new TreeItem() { Title = equipment.NAME };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Equipment.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", equipment.ID, equipment.NAME);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = equipment.UNIQUEID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

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
                        attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = string.Empty;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        var haveDownStreamOrganization = OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID));

                        var haveEquipment = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.EQUIPMENT.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID);

                        if (haveDownStreamOrganization || haveEquipment)
                        {
                            treeItem.State = "closed";
                        }

                        treeItemList.Add(treeItem);
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
                    { Define.EnumTreeAttribute.EquipmentUniqueID, string.Empty }
                };

                var organization = OrganizationList.First(x => x.UniqueID == OrganizationUniqueID);

                var treeItem = new TreeItem() { Title = organization.Description };

                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = string.Empty;

                foreach (var attribute in attributes)
                {
                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                }

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var haveDownStreamOrganization = OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID));

                    var haveEquipment = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.EQUIPMENT.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID);

                    if (haveDownStreamOrganization || haveEquipment)
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

        public static RequestResult GetTreeItem(List<Models.Shared.Organization> OrganizationList, string RefOrganizationUniqueID, string OrganizationUniqueID)
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
                    { Define.EnumTreeAttribute.EquipmentUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.PartUniqueID, string.Empty }
                };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var equipmentList = db.EQUIPMENT.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).OrderBy(x=>x.ID).ToList();

                    foreach (var equipment in equipmentList)
                    {
                        var treeItem = new TreeItem() { Title = equipment.NAME };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Equipment.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", equipment.ID, equipment.NAME);
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                        attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = equipment.UNIQUEID;
                        attributes[Define.EnumTreeAttribute.PartUniqueID] = "*";

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        treeItemList.Add(treeItem);

                        var partList = db.EQUIPMENTPART.Where(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID).OrderBy(x => x.DESCRIPTION).ToList();

                        foreach (var part in partList)
                        {
                            var partTreeItem = new TreeItem() { Title = string.Format("{0}-{1}", equipment.NAME, part.DESCRIPTION) };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.EquipmentPart.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}-{2}", equipment.ID, equipment.NAME, part.DESCRIPTION);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = equipment.UNIQUEID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = part.UNIQUEID;

                            foreach (var attribute in attributes)
                            {
                                partTreeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(partTreeItem);
                        }
                    }

                    var availableOrganizationList = OrganizationDataAccessor.GetUserOrganizationPermissionList(RefOrganizationUniqueID).Select(x => x.UniqueID).ToList();

                    var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && availableOrganizationList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                    foreach (var organization in organizationList)
                    {
                        var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organization.UniqueID, true);

                        if (db.EQUIPMENT.Any(x => downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID) && availableOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)))
                        {
                            var treeItem = new TreeItem() { Title = organization.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = string.Empty;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItem.State = "closed";

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

        public static ExcelExportModel Export(GridViewModel gridViewModel, Define.EnumExcelVersion ExcelVersion)
        {
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

                ISheet sheet = wk.CreateSheet(Resources.Resource.EquipmentReport);

                sheet.DefaultColumnWidth = 18;
                sheet.DefaultRowHeight = 400;

                ICellStyle titleCellStyle = wk.CreateCellStyle();  //标题的样式，上下居中，字体加大
                IFont titleFont = wk.CreateFont();
                titleFont.FontName = "新細明體";
                titleFont.FontHeightInPoints = 12;
                titleCellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                titleCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                titleCellStyle.SetFont(titleFont);



                sheet.CreateRow(0).CreateCell(0).SetCellValue(Resources.Resource.EquipmentReport);
                sheet.GetRow(0).GetCell(0).CellStyle = titleCellStyle;
                sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 2));
                sheet.GetRow(0).CreateCell(3).SetCellValue(DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTime.Now));

                var row1 = sheet.CreateRow(1);  //標題行
                row1.CreateCell(0).SetCellValue(Resources.Resource.Organization);
                row1.CreateCell(1).SetCellValue(Resources.Resource.EquipmentID);
                row1.CreateCell(2).SetCellValue(Resources.Resource.EquipmentName);
                row1.CreateCell(3).SetCellValue(Resources.Resource.MaintenanceOrganization);

                var index=2;
                foreach (var item in gridViewModel.ItemList)
                {
                    var row = sheet.CreateRow(index);
                    row.CreateCell(0).SetCellValue(item.OrganizationDescription);
                    row.CreateCell(1).SetCellValue(item.ID);
                    row.CreateCell(2).SetCellValue(item.Name);
                    row.CreateCell(3).SetCellValue(item.MaintenanceOrganization);
                    index++;
                }

                var model = new ExcelExportModel(Resources.Resource.EquipmentReport, ExcelVersion);

                using (FileStream fs = System.IO.File.OpenWrite(model.FullFileName))
                {
                    wk.Write(fs);
                }

                byte[] buff = null;

                using (var fs = System.IO.File.OpenRead(model.FullFileName))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        long numBytes = new FileInfo(model.FullFileName).Length;

                        buff = br.ReadBytes((int)numBytes);

                        br.Close();
                    }

                    fs.Close();
                }

                model.Data = buff;

                return model;
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);

                return null;

            }
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
                    var query = db.EQUIPMENT.Where(x => UniqueIDList.Contains(x.UNIQUEID)).Select(x => new { 
                    
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


                        

                        ////blank
                        //var row1 = sheet.CreateRow(index);
                        //row1.Height = (short)(6 * columnHeightUnit);

                        //// s/n
                        //var row2Idx = index + 1;
                        //var row2 = sheet.CreateRow(row2Idx);
                        //row2.Height = (short)(contentHeight * columnHeightUnit);
                        //var row2Cell1 = row2.CreateCell(0);
                        //row2Cell1.SetCellValue("S/N:");
                        //row2Cell1.CellStyle = titleCellStyle;



                        //var row2Cell2 = row2.CreateCell(1);
                        //row2Cell2.SetCellValue(item.SN);
                        //row2Cell2.CellStyle = contentCellStyle;

                        //row2.CreateCell(2);

                        //// DATE
                        //var row3Idx = index + 2;
                        //var row3 = sheet.CreateRow(row3Idx);
                        //row3.Height = (short)(contentHeight * columnHeightUnit);
                        //var row3Cell1 = row3.CreateCell(0);
                        //row3Cell1.SetCellValue("DATE:");
                        //row3Cell1.CellStyle = titleCellStyle;


                        //var row3Cell2 = row3.CreateCell(1);
                        //row3Cell2.SetCellValue(item.CALDate);
                        //row3Cell2.CellStyle = contentCellStyle;


                        //row3.CreateCell(2);

                        //// SIGN
                        //var row4Idx = index + 3;
                        //var row4 = sheet.CreateRow(row4Idx);
                        //row4.Height = (short)(contentHeight * columnHeightUnit);
                        //var row4Cell1 = row4.CreateCell(0);
                        //row4Cell1.SetCellValue("SIGN:");
                        //row4Cell1.CellStyle = titleCellStyle;


                        //var row4Cell2 = row4.CreateCell(1);
                        //row4Cell2.SetCellValue(item.Sign);
                        //row4Cell2.CellStyle = contentCellStyle;


                        //row4.CreateCell(2);

                        //sheet.AddMergedRegion(new CellRangeAddress(row2Idx, row4Idx, 2, 2));

                        

                        
                        
                    }
                    endRowForPrint = index > 1 ? index - 1 : index;
                    //定欄位單位寬度
                    


                }

                //set the print area for the first sheet
                //wk.SetPrintArea(0, startColumnForPrint, endColumnForPrint, startRowForPrint, endRowForPrint);

                // Use reflection go call internal method GetCTWorksheet()
                //MethodInfo methodInfo = sheet.GetType().GetMethod("GetCTWorksheet", BindingFlags.NonPublic | BindingFlags.Instance);
                //var ct = (CT_Worksheet)methodInfo.Invoke(sheet, new object[] { });

                //CT_SheetView view = ct.sheetViews.GetSheetViewArray(0);
                //view.view = ST_SheetViewType.pageBreakPreview;

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
