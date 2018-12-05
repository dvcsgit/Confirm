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
using Models.EquipmentMaintenance.EquipmentCheckItemManagement;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.EquipmentMaintenance
{
    public class EquipmentCheckItemDataAccessor
    {
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
                        IsFeelItemDefaultNormal = equipment.IsFeelItemDefaultNormal,
                        CheckItemList = (from x in db.EquipmentCheckItem
                                         join c in db.CheckItem
                                         on x.CheckItemUniqueID equals c.UniqueID
                                         where x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == "*"
                                         select new CheckItemModel
                                         {
                                             UniqueID = c.UniqueID,
                                             CheckType = c.CheckType,
                                             ID = c.ID,
                                             Description = c.Description,
                                             IsFeelItem = c.IsFeelItem,
                                             IsAccumulation = c.IsAccumulation,
                                             IsInherit = x.IsInherit,
                                             OriLowerLimit = c.LowerLimit,
                                             OriLowerAlertLimit = c.LowerAlertLimit,
                                             OriUpperAlertLimit = c.UpperAlertLimit,
                                             OriUpperLimit = c.UpperLimit,
                                             OriAccumulationBase = c.AccumulationBase,
                                             OriUnit = c.Unit,
                                             OriRemark = c.Remark,
                                             LowerLimit = x.LowerLimit,
                                             LowerAlertLimit = x.LowerAlertLimit,
                                             UpperAlertLimit = x.UpperAlertLimit,
                                             UpperLimit = x.UpperLimit,
                                             AccumulationBase = x.AccumulationBase,
                                             Unit = x.Unit,
                                             Remark = x.Remark
                                         }).OrderBy(x => x.CheckType).ThenBy(x => x.ID).ToList(),
                        PartList = (from p in db.EquipmentPart
                                    where p.EquipmentUniqueID == equipment.UniqueID
                                    select new PartModel
                                    {
                                        UniqueID = p.UniqueID,
                                        Description = p.Description,
                                        CheckItemList = (from x in db.EquipmentCheckItem
                                                         join c in db.CheckItem
                                                         on x.CheckItemUniqueID equals c.UniqueID
                                                         where x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == p.UniqueID
                                                         select new CheckItemModel
                                                         {
                                                             UniqueID = c.UniqueID,
                                                             CheckType = c.CheckType,
                                                             ID = c.ID,
                                                             Description = c.Description,
                                                             IsFeelItem = c.IsFeelItem,
                                                             IsAccumulation = c.IsAccumulation,
                                                             IsInherit = x.IsInherit,
                                                             OriLowerLimit = c.LowerLimit,
                                                             OriLowerAlertLimit = c.LowerAlertLimit,
                                                             OriUpperAlertLimit = c.UpperAlertLimit,
                                                             OriUpperLimit = c.UpperLimit,
                                                             OriAccumulationBase = c.AccumulationBase,
                                                             OriUnit = c.Unit,
                                                             OriRemark = c.Remark,
                                                             LowerLimit = x.LowerLimit,
                                                             LowerAlertLimit = x.LowerAlertLimit,
                                                             UpperAlertLimit = x.UpperAlertLimit,
                                                             UpperLimit = x.UpperLimit,
                                                             AccumulationBase = x.AccumulationBase,
                                                             Unit = x.Unit,
                                                             Remark = x.Remark
                                                         }).OrderBy(x => x.CheckType).ThenBy(x => x.ID).ToList()
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

                    result.ReturnData(new CreateFormModel()
                    {
                        UniqueID =  Guid.NewGuid().ToString(),
                        OrganizationUniqueID = equipment.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(equipment.OrganizationUniqueID),
                        FormInput = new FormInput()
                        {
                            Name = equipment.Name,
                            IsFeelItemDefaultNormal = equipment.IsFeelItemDefaultNormal
                        },
                        CheckItemList = (from x in db.EquipmentCheckItem
                                         join c in db.CheckItem
                                         on x.CheckItemUniqueID equals c.UniqueID
                                         where x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID=="*"
                                         select new CheckItemModel
                                         {
                                             UniqueID = c.UniqueID,
                                             CheckType = c.CheckType,
                                             ID = c.ID,
                                             Description = c.Description,
                                             IsFeelItem = c.IsFeelItem,
                                             IsAccumulation = c.IsAccumulation,
                                             IsInherit = x.IsInherit,
                                             OriLowerLimit = c.LowerLimit,
                                             OriLowerAlertLimit = c.LowerAlertLimit,
                                             OriUpperAlertLimit = c.UpperAlertLimit,
                                             OriUpperLimit = c.UpperLimit,
                                             OriAccumulationBase = c.AccumulationBase,
                                             OriUnit = c.Unit,
                                             OriRemark = c.Remark,
                                             LowerLimit = x.LowerLimit,
                                             LowerAlertLimit = x.LowerAlertLimit,
                                             UpperAlertLimit = x.UpperAlertLimit,
                                             UpperLimit = x.UpperLimit,
                                             AccumulationBase = x.AccumulationBase,
                                             Unit = x.Unit,
                                             Remark = x.Remark
                                         }).OrderBy(x => x.CheckType).ThenBy(x => x.ID).ToList(),
                        PartList = (from p in db.EquipmentPart
                                    where p.EquipmentUniqueID == equipment.UniqueID
                                    select new PartModel
                                    {
                                        UniqueID = Guid.NewGuid().ToString(),
                                        Description = p.Description,
                                        CheckItemList = (from x in db.EquipmentCheckItem
                                                         join c in db.CheckItem
                                                         on x.CheckItemUniqueID equals c.UniqueID
                                                         where x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == p.UniqueID
                                                         select new CheckItemModel
                                                         {
                                                             UniqueID = c.UniqueID,
                                                             CheckType = c.CheckType,
                                                             ID = c.ID,
                                                             Description = c.Description,
                                                             IsFeelItem = c.IsFeelItem,
                                                             IsAccumulation = c.IsAccumulation,
                                                             IsInherit = x.IsInherit,
                                                             OriLowerLimit = c.LowerLimit,
                                                             OriLowerAlertLimit = c.LowerAlertLimit,
                                                             OriUpperAlertLimit = c.UpperAlertLimit,
                                                             OriUpperLimit = c.UpperLimit,
                                                             OriAccumulationBase = c.AccumulationBase,
                                                             OriUnit = c.Unit,
                                                             OriRemark = c.Remark,
                                                             LowerLimit = x.LowerLimit,
                                                             LowerAlertLimit = x.LowerAlertLimit,
                                                             UpperAlertLimit = x.UpperAlertLimit,
                                                             UpperLimit = x.UpperLimit,
                                                             AccumulationBase = x.AccumulationBase,
                                                             Unit = x.Unit,
                                                             Remark = x.Remark
                                                         }).OrderBy(x => x.CheckType).ThenBy(x => x.ID).ToList()
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
                            IsFeelItemDefaultNormal = Model.FormInput.IsFeelItemDefaultNormal,
                            LastModifyTime = DateTime.Now
                        });

                        db.EquipmentCheckItem.AddRange(Model.CheckItemList.Select(x => new EquipmentCheckItem
                        {
                            EquipmentUniqueID = Model.UniqueID,
                            PartUniqueID = "*",
                            CheckItemUniqueID = x.UniqueID,
                            IsInherit = x.IsInherit,
                            LowerLimit = x.LowerLimit,
                            LowerAlertLimit = x.LowerAlertLimit,
                            UpperAlertLimit = x.UpperAlertLimit,
                            UpperLimit = x.UpperLimit,
                            AccumulationBase = x.AccumulationBase,
                            Unit = x.Unit,
                            Remark = x.Remark
                        }).ToList());

                        foreach (var part in Model.PartList)
                        {
                            db.EquipmentPart.Add(new EquipmentPart()
                            {
                                UniqueID = part.UniqueID,
                                EquipmentUniqueID = Model.UniqueID,
                                Description = part.Description
                            });

                            db.EquipmentCheckItem.AddRange(part.CheckItemList.Select(x => new EquipmentCheckItem
                            {
                                EquipmentUniqueID = Model.UniqueID,
                                PartUniqueID = part.UniqueID,
                                CheckItemUniqueID = x.UniqueID,
                                IsInherit = x.IsInherit,
                                LowerLimit = x.LowerLimit,
                                LowerAlertLimit = x.LowerAlertLimit,
                                UpperAlertLimit = x.UpperAlertLimit,
                                UpperLimit = x.UpperLimit,
                                AccumulationBase = x.AccumulationBase,
                                Unit = x.Unit,
                                Remark = x.Remark
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

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = equipment.UniqueID,
                        OrganizationUniqueID = equipment.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(equipment.OrganizationUniqueID),
                        FormInput = new FormInput()
                        {
                            ID = equipment.ID,
                            Name = equipment.Name,
                            IsFeelItemDefaultNormal = equipment.IsFeelItemDefaultNormal
                        },
                        CheckItemList = (from x in db.EquipmentCheckItem
                                         join c in db.CheckItem
                                         on x.CheckItemUniqueID equals c.UniqueID
                                         where x.EquipmentUniqueID == equipment.UniqueID &&x.PartUniqueID=="*"
                                         select new CheckItemModel
                                         {
                                             UniqueID = c.UniqueID,
                                             CheckType = c.CheckType,
                                             ID = c.ID,
                                             Description = c.Description,
                                             IsFeelItem = c.IsFeelItem,
                                             IsAccumulation = c.IsAccumulation,
                                             IsInherit = x.IsInherit,
                                             OriLowerLimit = c.LowerLimit,
                                             OriLowerAlertLimit = c.LowerAlertLimit,
                                             OriUpperAlertLimit = c.UpperAlertLimit,
                                             OriUpperLimit = c.UpperLimit,
                                             OriAccumulationBase = c.AccumulationBase,
                                             OriUnit = c.Unit,
                                             OriRemark = c.Remark,
                                             LowerLimit = x.LowerLimit,
                                             LowerAlertLimit = x.LowerAlertLimit,
                                             UpperAlertLimit = x.UpperAlertLimit,
                                             UpperLimit = x.UpperLimit,
                                             AccumulationBase = x.AccumulationBase,
                                             Unit = x.Unit,
                                             Remark = x.Remark
                                         }).OrderBy(x => x.CheckType).ThenBy(x => x.ID).ToList(),
                        PartList = (from p in db.EquipmentPart
                                    where p.EquipmentUniqueID == equipment.UniqueID
                                    select new PartModel
                                    {
                                        UniqueID = p.UniqueID,
                                        Description = p.Description,
                                        CheckItemList = (from x in db.EquipmentCheckItem
                                                         join c in db.CheckItem
                                                         on x.CheckItemUniqueID equals c.UniqueID
                                                         where x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == p.UniqueID
                                                         select new CheckItemModel
                                                         {
                                                             UniqueID = c.UniqueID,
                                                             CheckType = c.CheckType,
                                                             ID = c.ID,
                                                             Description = c.Description,
                                                             IsFeelItem = c.IsFeelItem,
                                                             IsAccumulation = c.IsAccumulation,
                                                             IsInherit = x.IsInherit,
                                                             OriLowerLimit = c.LowerLimit,
                                                             OriLowerAlertLimit = c.LowerAlertLimit,
                                                             OriUpperAlertLimit = c.UpperAlertLimit,
                                                             OriUpperLimit = c.UpperLimit,
                                                             OriAccumulationBase = c.AccumulationBase,
                                                             OriUnit = c.Unit,
                                                             OriRemark = c.Remark,
                                                             LowerLimit = x.LowerLimit,
                                                             LowerAlertLimit = x.LowerAlertLimit,
                                                             UpperAlertLimit = x.UpperAlertLimit,
                                                             UpperLimit = x.UpperLimit,
                                                             AccumulationBase = x.AccumulationBase,
                                                             Unit = x.Unit,
                                                             Remark = x.Remark
                                                         }).OrderBy(x => x.CheckType).ThenBy(x => x.ID).ToList()
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

        public static RequestResult Edit(EditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var equipment = db.Equipment.First(x => x.UniqueID == Model.UniqueID);

                    var exists = db.Equipment.FirstOrDefault(x => x.UniqueID != equipment.UniqueID && x.OrganizationUniqueID == equipment.OrganizationUniqueID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                        #region Equipment
                        equipment.ID = Model.FormInput.ID;
                        equipment.Name = Model.FormInput.Name;
                        equipment.IsFeelItemDefaultNormal = Model.FormInput.IsFeelItemDefaultNormal;
                        equipment.LastModifyTime = DateTime.Now;

                        db.SaveChanges();
                        #endregion

                        #region EquipmentPart, EquipmentCheckItem
                        #region Delete
                        db.EquipmentPart.RemoveRange(db.EquipmentPart.Where(x => x.EquipmentUniqueID == equipment.UniqueID).ToList());
                        db.EquipmentCheckItem.RemoveRange(db.EquipmentCheckItem.Where(x => x.EquipmentUniqueID == equipment.UniqueID).ToList());

                        db.SaveChanges();
                        #endregion

                        #region Insert
                        db.EquipmentCheckItem.AddRange(Model.CheckItemList.Select(x => new EquipmentCheckItem
                        {
                            EquipmentUniqueID = equipment.UniqueID,
                            PartUniqueID = "*",
                            CheckItemUniqueID = x.UniqueID,
                            IsInherit = x.IsInherit,
                            LowerLimit = x.LowerLimit,
                            LowerAlertLimit = x.LowerAlertLimit,
                            UpperAlertLimit = x.UpperAlertLimit,
                            UpperLimit = x.UpperLimit,
                            AccumulationBase = x.AccumulationBase,
                            Unit = x.Unit,
                            Remark = x.Remark
                        }).ToList());

                        foreach (var part in Model.PartList)
                        {
                            db.EquipmentPart.Add(new EquipmentPart()
                            {
                                UniqueID = part.UniqueID,
                                EquipmentUniqueID = equipment.UniqueID,
                                Description = part.Description
                            });

                            db.EquipmentCheckItem.AddRange(part.CheckItemList.Select(x => new EquipmentCheckItem
                            {
                                EquipmentUniqueID = equipment.UniqueID,
                                PartUniqueID = part.UniqueID,
                                CheckItemUniqueID = x.UniqueID,
                                IsInherit = x.IsInherit,
                                LowerLimit = x.LowerLimit,
                                LowerAlertLimit = x.LowerAlertLimit,
                                UpperAlertLimit = x.UpperAlertLimit,
                                UpperLimit = x.UpperLimit,
                                AccumulationBase = x.AccumulationBase,
                                Unit = x.Unit,
                                Remark = x.Remark
                            }).ToList());
                        }

                        db.SaveChanges();
                        #endregion
                        #endregion

                        #region EquipmentMaterial, EquipmentStandard
                        var partList = Model.PartList.Select(x => x.UniqueID).ToList();

                        db.EquipmentStandard.RemoveRange(db.EquipmentStandard.Where(x => x.EquipmentUniqueID == equipment.UniqueID && !partList.Contains(x.PartUniqueID)).ToList());
                        db.EquipmentMaterial.RemoveRange(db.EquipmentMaterial.Where(x => x.EquipmentUniqueID == equipment.UniqueID && !partList.Contains(x.PartUniqueID)).ToList());

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

        public static RequestResult SavePageState(List<CheckItemModel> CheckItemList, List<string> PageStateList)
        {
            RequestResult result = new RequestResult();

            try
            {
                foreach (string pageState in PageStateList)
                {
                    string[] temp = pageState.Split(Define.Seperators, StringSplitOptions.None);

                    string isInherit = temp[0];
                    string checkItemUniqueID = temp[1];
                    string lowerLimit = temp[2];
                    string lowerAlertLimit = temp[3];
                    string upperAlertLimit = temp[4];
                    string upperLimit = temp[5];
                    string accumulationBase = temp[6];
                    string unit = temp[7];
                    string remark = temp[8];

                    var checkItem = CheckItemList.First(x => x.UniqueID == checkItemUniqueID);

                    checkItem.IsInherit = isInherit == "Y";

                    if (!checkItem.IsInherit)
                    {
                        checkItem.LowerLimit = !string.IsNullOrEmpty(lowerLimit) ? double.Parse(lowerLimit) : default(double?);
                        checkItem.LowerAlertLimit = !string.IsNullOrEmpty(lowerAlertLimit) ? double.Parse(lowerAlertLimit) : default(double?);
                        checkItem.UpperAlertLimit = !string.IsNullOrEmpty(upperAlertLimit) ? double.Parse(upperAlertLimit) : default(double?);
                        checkItem.UpperLimit = !string.IsNullOrEmpty(upperLimit) ? double.Parse(upperLimit) : default(double?);
                        checkItem.AccumulationBase = !string.IsNullOrEmpty(accumulationBase) ? double.Parse(accumulationBase) : default(double?);
                        checkItem.Unit = unit;
                        checkItem.Remark = remark;
                    }
                    else
                    {
                        checkItem.LowerLimit = null;
                        checkItem.LowerAlertLimit = null;
                        checkItem.UpperAlertLimit = null;
                        checkItem.UpperLimit = null;
                        checkItem.AccumulationBase = null;
                        checkItem.Unit = string.Empty;
                        checkItem.Remark = string.Empty;
                    }
                }

                result.ReturnData(CheckItemList);
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

                    string isInherit = temp[0];
                    string partUniqueID = temp[1];
                    string checkItemUniqueID = temp[2];
                    string lowerLimit = temp[3];
                    string lowerAlertLimit = temp[4];
                    string upperAlertLimit = temp[5];
                    string upperLimit = temp[6];
                    string accumulationBase = temp[7];
                    string unit = temp[8];
                    string remark = temp[9];

                    var checkItem = PartList.First(x => x.UniqueID == partUniqueID).CheckItemList.First(x => x.UniqueID == checkItemUniqueID);

                    checkItem.IsInherit = isInherit == "Y";

                    if (!checkItem.IsInherit)
                    {
                        checkItem.LowerLimit = !string.IsNullOrEmpty(lowerLimit) ? double.Parse(lowerLimit) : default(double?);
                        checkItem.LowerAlertLimit = !string.IsNullOrEmpty(lowerAlertLimit) ? double.Parse(lowerAlertLimit) : default(double?);
                        checkItem.UpperAlertLimit = !string.IsNullOrEmpty(upperAlertLimit) ? double.Parse(upperAlertLimit) : default(double?);
                        checkItem.UpperLimit = !string.IsNullOrEmpty(upperLimit) ? double.Parse(upperLimit) : default(double?);
                        checkItem.AccumulationBase = !string.IsNullOrEmpty(accumulationBase) ? double.Parse(accumulationBase) : default(double?);
                        checkItem.Unit = unit;
                        checkItem.Remark = remark;
                    }
                    else
                    {
                        checkItem.LowerLimit = null;
                        checkItem.LowerAlertLimit = null;
                        checkItem.UpperAlertLimit = null;
                        checkItem.UpperLimit = null;
                        checkItem.AccumulationBase = null;
                        checkItem.Unit = string.Empty;
                        checkItem.Remark = string.Empty;
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

        public static RequestResult AddCheckItem(List<CheckItemModel> CheckItemList, List<string> SelectedList, string RefOrganizationUniqueID)
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
                        var checkType = temp[1];
                        var checkItemUniqueID = temp[2];

                        if (!string.IsNullOrEmpty(checkItemUniqueID))
                        {
                            if (!CheckItemList.Any(x => x.UniqueID == checkItemUniqueID))
                            {
                                var checkItem = db.CheckItem.First(x => x.UniqueID == checkItemUniqueID);

                                CheckItemList.Add(new CheckItemModel()
                                {
                                    UniqueID = checkItem.UniqueID,
                                    CheckType = checkItem.CheckType,
                                    ID = checkItem.ID,
                                    Description = checkItem.Description,
                                    IsFeelItem = checkItem.IsFeelItem,
                                    IsAccumulation = checkItem.IsAccumulation,
                                    IsInherit = true,
                                    OriLowerLimit = checkItem.LowerLimit,
                                    OriLowerAlertLimit = checkItem.LowerAlertLimit,
                                    OriUpperAlertLimit = checkItem.UpperAlertLimit,
                                    OriUpperLimit = checkItem.UpperLimit,
                                    OriAccumulationBase = checkItem.AccumulationBase,
                                    OriUnit = checkItem.Unit,
                                    OriRemark = checkItem.Remark
                                });
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(checkType))
                            {
                                var checkItemList = db.CheckItem.Where(x => x.OrganizationUniqueID == organizationUniqueID && x.CheckType == checkType).ToList();

                                foreach (var checkItem in checkItemList)
                                {
                                    if (!CheckItemList.Any(x => x.UniqueID == checkItem.UniqueID))
                                    {
                                        CheckItemList.Add(new CheckItemModel()
                                        {
                                            UniqueID = checkItem.UniqueID,
                                            CheckType = checkItem.CheckType,
                                            ID = checkItem.ID,
                                            Description = checkItem.Description,
                                            IsFeelItem = checkItem.IsFeelItem,
                                            IsAccumulation = checkItem.IsAccumulation,
                                            IsInherit = true,
                                            OriLowerLimit = checkItem.LowerLimit,
                                            OriLowerAlertLimit = checkItem.LowerAlertLimit,
                                            OriUpperAlertLimit = checkItem.UpperAlertLimit,
                                            OriUpperLimit = checkItem.UpperLimit,
                                            OriAccumulationBase = checkItem.AccumulationBase,
                                            OriUnit = checkItem.Unit,
                                            OriRemark = checkItem.Remark
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
                                        var checkItemList = db.CheckItem.Where(x => x.OrganizationUniqueID == organization).ToList();

                                        foreach (var checkItem in checkItemList)
                                        {
                                            if (!CheckItemList.Any(x => x.UniqueID == checkItem.UniqueID))
                                            {
                                                CheckItemList.Add(new CheckItemModel()
                                                {
                                                    UniqueID = checkItem.UniqueID,
                                                    CheckType = checkItem.CheckType,
                                                    ID = checkItem.ID,
                                                    Description = checkItem.Description,
                                                    IsFeelItem = checkItem.IsFeelItem,
                                                    IsAccumulation = checkItem.IsAccumulation,
                                                    IsInherit = true,
                                                    OriLowerLimit = checkItem.LowerLimit,
                                                    OriLowerAlertLimit = checkItem.LowerAlertLimit,
                                                    OriUpperAlertLimit = checkItem.UpperAlertLimit,
                                                    OriUpperLimit = checkItem.UpperLimit,
                                                    OriAccumulationBase = checkItem.AccumulationBase,
                                                    OriUnit = checkItem.Unit,
                                                    OriRemark = checkItem.Remark
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                result.ReturnData(CheckItemList.OrderBy(x => x.CheckType).ThenBy(x => x.ID).ToList());
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

                        if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                            ||
                            (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && edb.Equipment.Any(x => x.OrganizationUniqueID == organization.UniqueID)))
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
    }
}
