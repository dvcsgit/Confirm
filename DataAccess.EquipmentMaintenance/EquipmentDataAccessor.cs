using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.Shared;
using Models.Authenticated;
using Models.EquipmentMaintenance.EquipmentManagement;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;
using System.IO;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.EquipmentMaintenance
{
    public class EquipmentDataAccessor
    {
        public static RequestResult UploadPhoto(string UniqueID, string Extension)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var equipment = db.Equipment.First(x => x.UniqueID == UniqueID);

                    //equipment.Extension = Extension;

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

        public static RequestResult DeletePhoto(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var equipment = db.Equipment.First(x => x.UniqueID == UniqueID);

                    //equipment.Extension = string.Empty;

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

                    var query = db.Equipment.Where(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.ID.Contains(Parameters.Keyword) || x.Name.Contains(Parameters.Keyword));
                    }

                    var organization = OrganizationDataAccessor.GetOrganization(Parameters.OrganizationUniqueID);

                    var model = new GridViewModel()
                    {
                        OrganizationUniqueID = Parameters.OrganizationUniqueID,
                        Permission = Account.OrganizationPermission(Parameters.OrganizationUniqueID),
                        OrganizationDescription = organization.Description,
                        FullOrganizationDescription = organization.FullDescription,
                        ItemList = query.ToList().Select(x => new GridItem()
                        {
                            UniqueID = x.UniqueID,
                            Permission = Account.OrganizationPermission(x.OrganizationUniqueID),
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.OrganizationUniqueID),
                            MaintenanceOrganization = OrganizationDataAccessor.GetOrganizationDescription(x.MaintenanceOrganizationUniqueID),
                            ID = x.ID,
                            Name = x.Name
                        }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.ID).ToList()
                    };

                    var upStreamList = OrganizationDataAccessor.GetUpStreamOrganizationList(Parameters.OrganizationUniqueID, false);
                    var downStreamList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, false);

                    foreach (var upStream in upStreamList)
                    {
                        if (Account.EditableOrganizationUniqueIDList.Any(x => x == upStream))
                        {
                            model.MoveToTargetList.Add(new MoveToTarget()
                            {
                                UniqueID = upStream,
                                Description = OrganizationDataAccessor.GetOrganizationFullDescription(upStream),
                                Direction = Define.EnumMoveDirection.Up
                            });
                        }
                    }

                    foreach (var downStream in downStreamList)
                    {
                        if (Account.EditableOrganizationUniqueIDList.Any(x => x == downStream))
                        {
                            model.MoveToTargetList.Add(new MoveToTarget()
                            {
                                UniqueID = downStream,
                                Description = OrganizationDataAccessor.GetOrganizationFullDescription(downStream),
                                Direction = Define.EnumMoveDirection.Down
                            });
                        }
                    }

                    model.MoveToTargetList = model.MoveToTargetList.OrderBy(x => x.Description).ToList();

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

        public static RequestResult GetDetailViewModel(string UniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var equipment = db.Equipment.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = equipment.UniqueID,
                        Permission = Account.OrganizationPermission(equipment.OrganizationUniqueID),
                        OrganizationUniqueID = equipment.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(equipment.OrganizationUniqueID),
                        ID = equipment.ID,
                        Name = equipment.Name,
                        MaintenanceOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(equipment.MaintenanceOrganizationUniqueID),
                        SpecList = (from x in db.EquipmentSpecValue
                                    join s in db.EquipmentSpec
                                    on x.SpecUniqueID equals s.UniqueID
                                    where x.EquipmentUniqueID == equipment.UniqueID
                                    select new SpecModel
                                    {
                                        UniqueID = s.UniqueID,
                                        Description = s.Description,
                                        OptionUniqueID = x.SpecOptionUniqueID,
                                        Value = x.Value,
                                        Seq = x.Seq,
                                        OptionList = db.EquipmentSpecOption.Where(o => o.SpecUniqueID == s.UniqueID).Select(o=> new Models.EquipmentMaintenance.EquipmentManagement.EquipmentSpecOption{
                                         Seq=o.Seq,
                                          SpecUniqueID=o.SpecUniqueID,
                                           Description=o.Description,
                                            UniqueID=o.UniqueID
                                        }).OrderBy(o => o.Seq).ToList()
                                    }).OrderBy(x => x.Seq).ToList(),
                        MaterialList = (from x in db.EquipmentMaterial
                                        join m in db.Material
                                        on x.MaterialUniqueID equals m.UniqueID
                                        where x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == "*"
                                        select new MaterialModel
                                        {
                                            UniqueID = m.UniqueID,
                                            ID = m.ID,
                                            Name = m.Name,
                                            Quantity = x.Quantity
                                        }).OrderBy(x => x.ID).ToList(),
                        PartList = (from p in db.EquipmentPart
                                    where p.EquipmentUniqueID == equipment.UniqueID
                                    select new PartModel
                                    {
                                        UniqueID = p.UniqueID,
                                        Description = p.Description,
                                        MaterialList = (from x in db.EquipmentMaterial
                                                        join m in db.Material
                                                        on x.MaterialUniqueID equals m.UniqueID
                                                        where x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == p.UniqueID
                                                        select new MaterialModel
                                                        {
                                                            UniqueID = m.UniqueID,
                                                            ID = m.ID,
                                                            Name = m.Name,
                                                            Quantity = x.Quantity
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
                using (EDbEntities db = new EDbEntities())
                {
                    var equipment = db.Equipment.First(x => x.UniqueID == UniqueID);

                    var organization = OrganizationDataAccessor.GetOrganization(equipment.OrganizationUniqueID);

                    var maintenanceOrganization = OrganizationDataAccessor.GetOrganization(equipment.MaintenanceOrganizationUniqueID);

                    result.ReturnData(new CreateFormModel()
                    {
                        UniqueID = Guid.NewGuid().ToString(),
                        AncestorOrganizationUniqueID = organization.AncestorOrganizationUniqueID,
                        OrganizationUniqueID = equipment.OrganizationUniqueID,
                        ParentOrganizationFullDescription = organization.FullDescription,
                        MaintenanceOrganizationID =maintenanceOrganization!=null? maintenanceOrganization.ID:string.Empty,
                        MaintenanceOrganizationDescription = maintenanceOrganization != null ? maintenanceOrganization.Description : string.Empty,
                        SpecList = (from x in db.EquipmentSpecValue
                                    join s in db.EquipmentSpec
                                    on x.SpecUniqueID equals s.UniqueID
                                    where x.EquipmentUniqueID == equipment.UniqueID
                                    select new SpecModel
                                    {
                                        UniqueID = s.UniqueID,
                                        Description = s.Description,
                                        OptionUniqueID = x.SpecOptionUniqueID,
                                        Value = x.Value,
                                        Seq = x.Seq,
                                        OptionList = db.EquipmentSpecOption.Where(o => o.SpecUniqueID == s.UniqueID).Select(o=>new Models.EquipmentMaintenance.EquipmentManagement.EquipmentSpecOption{
                                         UniqueID = o.UniqueID,
                                          Description=o.Description,
                                          SpecUniqueID = o.SpecUniqueID,
                                           Seq=o.Seq
                                        }).OrderBy(o => o.Seq).ToList()
                                    }).OrderBy(x => x.Seq).ToList(),
                        MaterialList = (from x in db.EquipmentMaterial
                                        join m in db.Material
                                        on x.MaterialUniqueID equals m.UniqueID
                                        where x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == "*"
                                        select new MaterialModel
                                        {
                                            UniqueID = m.UniqueID,
                                            ID = m.ID,
                                            Name = m.Name,
                                            Quantity = x.Quantity
                                        }).OrderBy(x => x.ID).ToList(),
                        PartList = (from p in db.EquipmentPart
                                    where p.EquipmentUniqueID == equipment.UniqueID
                                    select new PartModel
                                    {
                                        UniqueID = Guid.NewGuid().ToString(),
                                        Description = p.Description,
                                        MaterialList = (from x in db.EquipmentMaterial
                                                        join m in db.Material
                                                        on x.MaterialUniqueID equals m.UniqueID
                                                        where x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == p.UniqueID
                                                        select new MaterialModel
                                                        {
                                                            UniqueID = m.UniqueID,
                                                            ID = m.ID,
                                                            Name = m.Name,
                                                            Quantity = x.Quantity
                                                        }).OrderBy(x => x.ID).ToList()
                                    }).OrderBy(x => x.Description).ToList(),
                        FormInput = new FormInput()
                        {
                            Name = equipment.Name,
                            MaintenanceOrganizationUniqueID = equipment.MaintenanceOrganizationUniqueID
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
                using (EDbEntities db = new EDbEntities())
                {
                    var exists = db.Equipment.FirstOrDefault(x => x.OrganizationUniqueID == Model.OrganizationUniqueID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
                        db.Equipment.Add(new Equipment()
                        {
                            UniqueID = Model.UniqueID,
                            OrganizationUniqueID = Model.OrganizationUniqueID,
                            ID = Model.FormInput.ID,
                            Name = Model.FormInput.Name,
                            IsFeelItemDefaultNormal = false,
                            MaintenanceOrganizationUniqueID = Model.FormInput.MaintenanceOrganizationUniqueID,
                            LastModifyTime = DateTime.Now
                        });

                        db.EquipmentSpecValue.AddRange(Model.SpecList.Select(x => new EquipmentSpecValue
                        {
                            EquipmentUniqueID = Model.UniqueID,
                            SpecUniqueID = x.UniqueID,
                            SpecOptionUniqueID = x.OptionUniqueID,
                            Value = x.Value,
                            Seq = x.Seq
                        }).ToList());

                        db.EquipmentMaterial.AddRange(Model.MaterialList.Select(x => new EquipmentMaterial
                        {
                            EquipmentUniqueID = Model.UniqueID,
                            PartUniqueID = "*",
                            MaterialUniqueID = x.UniqueID,
                            Quantity = x.Quantity
                        }).ToList());

                        foreach (var part in Model.PartList)
                        {
                            db.EquipmentPart.Add(new EquipmentPart()
                            {
                                UniqueID = part.UniqueID,
                                EquipmentUniqueID = Model.UniqueID,
                                Description = part.Description
                            });

                            db.EquipmentMaterial.AddRange(part.MaterialList.Select(x => new EquipmentMaterial
                            {
                                EquipmentUniqueID = Model.UniqueID,
                                PartUniqueID = part.UniqueID,
                                MaterialUniqueID = x.UniqueID,
                                Quantity = x.Quantity
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
                using (EDbEntities db = new EDbEntities())
                {
                    var equipment = db.Equipment.First(x => x.UniqueID == UniqueID);

                    var organization = OrganizationDataAccessor.GetOrganization(equipment.OrganizationUniqueID);

                    var maintenanceOrganization = OrganizationDataAccessor.GetOrganization(equipment.MaintenanceOrganizationUniqueID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = equipment.UniqueID,
                        AncestorOrganizationUniqueID = organization.AncestorOrganizationUniqueID,
                        OrganizationUniqueID = equipment.OrganizationUniqueID,
                        ParentOrganizationFullDescription = organization.FullDescription,
                        MaintenanceOrganizationID = maintenanceOrganization != null ? maintenanceOrganization.ID : string.Empty,
                        MaintenanceOrganizationDescription = maintenanceOrganization != null ? maintenanceOrganization.Description : string.Empty,
                        SpecList = (from x in db.EquipmentSpecValue
                                    join s in db.EquipmentSpec
                                    on x.SpecUniqueID equals s.UniqueID
                                    where x.EquipmentUniqueID == equipment.UniqueID
                                    select new SpecModel
                                    {
                                        UniqueID = s.UniqueID,
                                        Description = s.Description,
                                        OptionUniqueID = x.SpecOptionUniqueID,
                                        Value = x.Value,
                                        Seq = x.Seq,
                                        OptionList = db.EquipmentSpecOption.Where(o => o.SpecUniqueID == s.UniqueID).Select(o => new Models.EquipmentMaintenance.EquipmentManagement.EquipmentSpecOption
                                        {
                                            UniqueID = o.UniqueID,
                                            Description = o.Description,
                                            SpecUniqueID = o.SpecUniqueID,
                                            Seq = o.Seq
                                        }).OrderBy(o => o.Seq).ToList()
                                    }).OrderBy(x => x.Seq).ToList(),
                        MaterialList = (from x in db.EquipmentMaterial
                                        join m in db.Material
                                        on x.MaterialUniqueID equals m.UniqueID
                                        where x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == "*"
                                        select new MaterialModel
                                        {
                                            UniqueID = m.UniqueID,
                                            ID = m.ID,
                                            Name = m.Name,
                                            Quantity = x.Quantity
                                        }).OrderBy(x => x.ID).ToList(),
                        PartList = (from p in db.EquipmentPart
                                    where p.EquipmentUniqueID == equipment.UniqueID
                                    select new PartModel
                                    {
                                        UniqueID = p.UniqueID,
                                        Description = p.Description,
                                        MaterialList = (from x in db.EquipmentMaterial
                                                        join m in db.Material
                                                        on x.MaterialUniqueID equals m.UniqueID
                                                        where x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == p.UniqueID
                                                        select new MaterialModel
                                                        {
                                                            UniqueID = m.UniqueID,
                                                            ID = m.ID,
                                                            Name = m.Name,
                                                            Quantity = x.Quantity
                                                        }).OrderBy(x => x.ID).ToList()
                                    }).OrderBy(x => x.Description).ToList(),
                        FormInput = new FormInput()
                        {
                            ID = equipment.ID,
                            Name = equipment.Name,
                            MaintenanceOrganizationUniqueID = equipment.MaintenanceOrganizationUniqueID
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
                using (EDbEntities db = new EDbEntities())
                {
                    var equipment = db.Equipment.First(x => x.UniqueID == Model.UniqueID);

                    var exists = db.Equipment.FirstOrDefault(x => x.OrganizationUniqueID == equipment.OrganizationUniqueID && x.UniqueID != equipment.UniqueID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                        #region Equipment
                        equipment.ID = Model.FormInput.ID;
                        equipment.Name = Model.FormInput.Name;
                        equipment.MaintenanceOrganizationUniqueID = Model.FormInput.MaintenanceOrganizationUniqueID;
                        equipment.LastModifyTime = DateTime.Now;

                        db.SaveChanges();
                        #endregion

                        #region EquipmentSpecValue
                        #region Delete
                        db.EquipmentSpecValue.RemoveRange(db.EquipmentSpecValue.Where(x => x.EquipmentUniqueID == equipment.UniqueID).ToList());

                        db.SaveChanges();
                        #endregion

                        #region Insert
                        db.EquipmentSpecValue.AddRange(Model.SpecList.Select(x => new EquipmentSpecValue
                        {
                            EquipmentUniqueID = equipment.UniqueID,
                            SpecUniqueID = x.UniqueID,
                            SpecOptionUniqueID = x.OptionUniqueID,
                            Value = x.Value,
                            Seq = x.Seq
                        }).ToList());

                        db.SaveChanges();
                        #endregion
                        #endregion

                        #region EquipmentPart, EquipmentMaterial
                        #region Delete
                        db.EquipmentPart.RemoveRange(db.EquipmentPart.Where(x => x.EquipmentUniqueID == equipment.UniqueID).ToList());
                        db.EquipmentMaterial.RemoveRange(db.EquipmentMaterial.Where(x => x.EquipmentUniqueID == equipment.UniqueID).ToList());

                        db.SaveChanges();
                        #endregion

                        #region Insert
                        db.EquipmentMaterial.AddRange(Model.MaterialList.Select(x => new EquipmentMaterial
                        {
                            EquipmentUniqueID = equipment.UniqueID,
                            PartUniqueID = "*",
                            MaterialUniqueID = x.UniqueID,
                            Quantity = x.Quantity
                        }).ToList()); 

                        foreach (var part in Model.PartList)
                        {
                            db.EquipmentPart.Add(new EquipmentPart()
                            {
                                UniqueID = part.UniqueID,
                                EquipmentUniqueID = equipment.UniqueID,
                                Description = part.Description
                            });

                            db.EquipmentMaterial.AddRange(part.MaterialList.Select(x => new EquipmentMaterial
                            {
                                EquipmentUniqueID = equipment.UniqueID,
                                PartUniqueID = part.UniqueID,
                                MaterialUniqueID = x.UniqueID,
                                Quantity = x.Quantity
                            }).ToList()); 
                        }

                        db.SaveChanges();
                        #endregion
                        #endregion

                        #region EquipmentCheckItem, EquipmentStandard
                        var partList = Model.PartList.Select(x => x.UniqueID).ToList();

                        db.EquipmentCheckItem.RemoveRange(db.EquipmentCheckItem.Where(x => x.EquipmentUniqueID == equipment.UniqueID&&x.PartUniqueID!="*" && !partList.Contains(x.PartUniqueID)).ToList());
                        db.EquipmentStandard.RemoveRange(db.EquipmentStandard.Where(x => x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID != "*" && !partList.Contains(x.PartUniqueID)).ToList());

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
                using (EDbEntities db = new EDbEntities())
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
                using (EDbEntities db = new EDbEntities())
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
                                var spec = db.EquipmentSpec.First(x => x.UniqueID == specUniqueID);

                                SpecList.Add(new SpecModel()
                                {
                                    UniqueID = spec.UniqueID,
                                    Description = spec.Description,
                                    Seq = SpecList.Count + 1,
                                    OptionList = db.EquipmentSpecOption.Where(o => o.SpecUniqueID == spec.UniqueID).Select(o => new Models.EquipmentMaintenance.EquipmentManagement.EquipmentSpecOption
                                    {
                                        UniqueID = o.UniqueID,
                                        Description = o.Description,
                                        SpecUniqueID = o.SpecUniqueID,
                                        Seq = o.Seq
                                    }).OrderBy(o => o.Seq).ToList()
                                });
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(equipmentType))
                            {
                                var specList = db.EquipmentSpec.Where(x => x.OrganizationUniqueID == organizationUniqueID && x.EquipmentType == equipmentType).ToList();

                                foreach (var spec in specList)
                                {
                                    if (!SpecList.Any(x => x.UniqueID == spec.UniqueID))
                                    {
                                        SpecList.Add(new SpecModel()
                                        {
                                            UniqueID = spec.UniqueID,
                                            Description = spec.Description,
                                            Seq = SpecList.Count+1,
                                            OptionList = db.EquipmentSpecOption.Where(o => o.SpecUniqueID == spec.UniqueID).Select(o => new Models.EquipmentMaintenance.EquipmentManagement.EquipmentSpecOption
                                            {
                                                UniqueID = o.UniqueID,
                                                Description = o.Description,
                                                SpecUniqueID = o.SpecUniqueID,
                                                Seq = o.Seq
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
                                        var specList = db.EquipmentSpec.Where(x => x.OrganizationUniqueID == organization).ToList();

                                        foreach (var spec in specList)
                                        {
                                            if (!SpecList.Any(x => x.UniqueID == spec.UniqueID))
                                            {
                                                SpecList.Add(new SpecModel()
                                                {
                                                    UniqueID = spec.UniqueID,
                                                    Description = spec.Description,
                                                    Seq = SpecList.Count+1,
                                                    OptionList = db.EquipmentSpecOption.Where(o => o.SpecUniqueID == spec.UniqueID).Select(o => new Models.EquipmentMaintenance.EquipmentManagement.EquipmentSpecOption
                                                    {
                                                        UniqueID = o.UniqueID,
                                                        Description = o.Description,
                                                        SpecUniqueID = o.SpecUniqueID,
                                                        Seq = o.Seq
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
                using (EDbEntities db = new EDbEntities())
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
                                var material = db.Material.First(x => x.UniqueID == materialUniqueID);

                                MaterialList.Add(new MaterialModel()
                                {
                                    UniqueID = material.UniqueID,
                                    ID = material.ID,
                                    Name = material.Name,
                                    Quantity = 0
                                });
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(equipmentType))
                            {
                                var materialList = db.Material.Where(x => x.OrganizationUniqueID == organizationUniqueID && x.EquipmentType == equipmentType).ToList();

                                foreach (var material in materialList)
                                {
                                    if (!MaterialList.Any(x => x.UniqueID == material.UniqueID))
                                    {
                                        MaterialList.Add(new MaterialModel()
                                        {
                                            UniqueID = material.UniqueID,
                                            ID = material.ID,
                                            Name = material.Name,
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
                                        var materialList = db.Material.Where(x => x.OrganizationUniqueID == organization).ToList();

                                        foreach (var material in materialList)
                                        {
                                            if (!MaterialList.Any(x => x.UniqueID == material.UniqueID))
                                            {
                                                MaterialList.Add(new MaterialModel()
                                                {
                                                    UniqueID = material.UniqueID,
                                                    ID = material.ID,
                                                    Name = material.Name,
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

                using (EDbEntities edb = new EDbEntities())
                {
                    if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                    {
                        var equipmentList = edb.Equipment.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                        foreach (var equipment in equipmentList)
                        {
                            var treeItem = new TreeItem() { Title = equipment.Name };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Equipment.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", equipment.ID, equipment.Name);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = equipment.UniqueID;

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

                        var haveEquipment = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && edb.Equipment.Any(x => x.OrganizationUniqueID == organization.UniqueID);

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

                using (EDbEntities edb = new EDbEntities())
                {
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

                    var haveDownStreamOrganization = OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID));

                    var haveEquipment = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && edb.Equipment.Any(x => x.OrganizationUniqueID == organization.UniqueID);

                    if (haveDownStreamOrganization || haveEquipment)
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

                using (EDbEntities edb = new EDbEntities())
                {
                    var equipmentList = edb.Equipment.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x=>x.ID).ToList();

                    foreach (var equipment in equipmentList)
                    {
                        var treeItem = new TreeItem() { Title = equipment.Name };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Equipment.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", equipment.ID, equipment.Name);
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                        attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = equipment.UniqueID;
                        attributes[Define.EnumTreeAttribute.PartUniqueID] = "*";

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        treeItemList.Add(treeItem);

                        var partList = edb.EquipmentPart.Where(x => x.EquipmentUniqueID == equipment.UniqueID).OrderBy(x => x.Description).ToList();

                        foreach (var part in partList)
                        {
                            var partTreeItem = new TreeItem() { Title = string.Format("{0}-{1}", equipment.Name, part.Description) };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.EquipmentPart.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}-{2}", equipment.ID, equipment.Name, part.Description);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.EquipmentUniqueID] = equipment.UniqueID;
                            attributes[Define.EnumTreeAttribute.PartUniqueID] = part.UniqueID;

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

                        if (edb.Equipment.Any(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && availableOrganizationList.Contains(x.OrganizationUniqueID)))
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

        public static RequestResult Move(string OrganizationUniqueID, List<string> SelectedList)
        {
            RequestResult result = new RequestResult();

            try
            {
#if !DEBUG
                using (TransactionScope trans = new TransactionScope())
                {
#endif
                using (EDbEntities db = new EDbEntities())
                {
                    foreach (var equipmentUniqueID in SelectedList)
                    {
                        db.Equipment.First(x => x.ID == equipmentUniqueID).OrganizationUniqueID = OrganizationUniqueID;
                    }

                    db.SaveChanges();
                }
#if !DEBUG
                    trans.Complete();
                }
#endif

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Move, Resources.Resource.Equipment, Resources.Resource.Success));
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
