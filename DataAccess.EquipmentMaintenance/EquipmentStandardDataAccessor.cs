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
using Models.EquipmentMaintenance.EquipmentStandardManagement;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.EquipmentMaintenance
{
    public class EquipmentStandardDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.Equipment.Where(x => (downStreamOrganizationList.Contains(x.OrganizationUniqueID) || downStreamOrganizationList.Contains(x.MaintenanceOrganizationUniqueID)) && (Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID) || Account.QueryableOrganizationUniqueIDList.Contains(x.MaintenanceOrganizationUniqueID))).AsQueryable();

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

                    //var upStreamList = OrganizationDataAccessor.GetUpStreamOrganizationList(Parameters.OrganizationUniqueID, false);
                    //var downStreamList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, false);

                    //foreach (var upStream in upStreamList)
                    //{
                    //    if (Account.EditableOrganizationUniqueIDList.Any(x => x == upStream))
                    //    {
                    //        model.MoveToTargetList.Add(new MoveToTarget()
                    //        {
                    //            UniqueID = upStream,
                    //            Description = OrganizationDataAccessor.GetOrganizationFullDescription(upStream),
                    //            Direction = Define.EnumMoveDirection.Up
                    //        });
                    //    }
                    //}

                    //foreach (var downStream in downStreamList)
                    //{
                    //    if (Account.EditableOrganizationUniqueIDList.Any(x => x == downStream))
                    //    {
                    //        model.MoveToTargetList.Add(new MoveToTarget()
                    //        {
                    //            UniqueID = downStream,
                    //            Description = OrganizationDataAccessor.GetOrganizationFullDescription(downStream),
                    //            Direction = Define.EnumMoveDirection.Down
                    //        });
                    //    }
                    //}

                    //model.MoveToTargetList = model.MoveToTargetList.OrderBy(x => x.Description).ToList();

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

                    var model = new DetailViewModel()
                    {
                        UniqueID = equipment.UniqueID,
                        Permission = Account.OrganizationPermission(equipment.OrganizationUniqueID),
                        OrganizationUniqueID = equipment.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(equipment.OrganizationUniqueID),
                        MaintenanceOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(equipment.MaintenanceOrganizationUniqueID),
                        ID = equipment.ID,
                        Name = equipment.Name
                    };

                    var standardList = (from x in db.EquipmentStandard
                                        join c in db.Standard
                                        on x.StandardUniqueID equals c.UniqueID
                                        where x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == "*"
                                        select new
                                        {
                                            x = x,
                                            c = c
                                        }).OrderBy(x => x.c.MaintenanceType).ThenBy(x => x.c.ID).ToList();

                    foreach (var standard in standardList)
                    {
                        model.StandardList.Add(new StandardModel()
                        {
                            UniqueID = standard.c.UniqueID,
                            MaintenanceType = standard.c.MaintenanceType,
                            ID = standard.c.ID,
                            Description = standard.c.Description,
                            IsFeelItem = standard.c.IsFeelItem,
                            IsAccumulation = standard.c.IsAccumulation,
                            IsInherit = standard.x.IsInherit,
                            OriLowerLimit = standard.c.LowerLimit.HasValue ? double.Parse(standard.c.LowerLimit.Value.ToString()) : default(double?),
                            OriLowerAlertLimit = standard.c.LowerAlertLimit.HasValue ? double.Parse(standard.c.LowerAlertLimit.Value.ToString()) : default(double?),
                            OriUpperAlertLimit = standard.c.UpperAlertLimit.HasValue ? double.Parse(standard.c.UpperAlertLimit.Value.ToString()) : default(double?),
                            OriUpperLimit = standard.c.UpperLimit.HasValue ? double.Parse(standard.c.UpperLimit.Value.ToString()) : default(double?),
                            OriAccumulationBase = standard.c.AccumulationBase.HasValue ? double.Parse(standard.c.AccumulationBase.Value.ToString()) : default(double?),
                            OriUnit = standard.c.Unit,
                            OriRemark = standard.c.Remark,
                            LowerLimit = standard.x.LowerLimit.HasValue ? double.Parse(standard.x.LowerLimit.Value.ToString()) : default(double?),
                            LowerAlertLimit = standard.x.LowerAlertLimit.HasValue ? double.Parse(standard.x.LowerAlertLimit.Value.ToString()) : default(double?),
                            UpperAlertLimit = standard.x.UpperAlertLimit.HasValue ? double.Parse(standard.x.UpperAlertLimit.Value.ToString()) : default(double?),
                            UpperLimit = standard.x.UpperLimit.HasValue ? double.Parse(standard.x.UpperLimit.Value.ToString()) : default(double?),
                            AccumulationBase = standard.x.AccumulationBase.HasValue ? double.Parse(standard.x.AccumulationBase.Value.ToString()) : default(double?),
                            Unit = standard.x.Unit,
                            Remark = standard.x.Remark
                        });
                    }

                    var partList = db.EquipmentPart.Where(x => x.EquipmentUniqueID == equipment.UniqueID).OrderBy(x => x.Description).ToList();

                    foreach (var part in partList)
                    {
                        var partModel = new PartModel()
                        {
                            UniqueID = part.UniqueID,
                            Description = part.Description,
                        };

                        var partStandardList = (from x in db.EquipmentStandard
                                                join c in db.Standard
                                                on x.StandardUniqueID equals c.UniqueID
                                                where x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == part.UniqueID
                                                select new
                                                {
                                                    x = x,
                                                    c = c
                                                }).OrderBy(x => x.c.MaintenanceType).ThenBy(x => x.c.ID).ToList();

                        foreach (var standard in partStandardList)
                        {
                            partModel.StandardList.Add(new StandardModel()
                            {
                                UniqueID = standard.c.UniqueID,
                                MaintenanceType = standard.c.MaintenanceType,
                                ID = standard.c.ID,
                                Description = standard.c.Description,
                                IsFeelItem = standard.c.IsFeelItem,
                                IsAccumulation = standard.c.IsAccumulation,
                                IsInherit = standard.x.IsInherit,
                                OriLowerLimit = standard.c.LowerLimit.HasValue ? double.Parse(standard.c.LowerLimit.Value.ToString()) : default(double?),
                                OriLowerAlertLimit = standard.c.LowerAlertLimit.HasValue ? double.Parse(standard.c.LowerAlertLimit.Value.ToString()) : default(double?),
                                OriUpperAlertLimit = standard.c.UpperAlertLimit.HasValue ? double.Parse(standard.c.UpperAlertLimit.Value.ToString()) : default(double?),
                                OriUpperLimit = standard.c.UpperLimit.HasValue ? double.Parse(standard.c.UpperLimit.Value.ToString()) : default(double?),
                                OriAccumulationBase = standard.c.AccumulationBase.HasValue ? double.Parse(standard.c.AccumulationBase.Value.ToString()) : default(double?),
                                OriUnit = standard.c.Unit,
                                OriRemark = standard.c.Remark,
                                LowerLimit = standard.x.LowerLimit.HasValue ? double.Parse(standard.x.LowerLimit.Value.ToString()) : default(double?),
                                LowerAlertLimit = standard.x.LowerAlertLimit.HasValue ? double.Parse(standard.x.LowerAlertLimit.Value.ToString()) : default(double?),
                                UpperAlertLimit = standard.x.UpperAlertLimit.HasValue ? double.Parse(standard.x.UpperAlertLimit.Value.ToString()) : default(double?),
                                UpperLimit = standard.x.UpperLimit.HasValue ? double.Parse(standard.x.UpperLimit.Value.ToString()) : default(double?),
                                AccumulationBase = standard.x.AccumulationBase.HasValue ? double.Parse(standard.x.AccumulationBase.Value.ToString()) : default(double?),
                                Unit = standard.x.Unit,
                                Remark = standard.x.Remark
                            });
                        }

                        model.PartList.Add(partModel);
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

        public static RequestResult GetEditFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var equipment = db.Equipment.First(x => x.UniqueID == UniqueID);

                    var model = new EditFormModel()
                    {
                        UniqueID = equipment.UniqueID,
                        OrganizationUniqueID = equipment.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(equipment.OrganizationUniqueID),
                        MaintenanceOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(equipment.MaintenanceOrganizationUniqueID),
                        ID = equipment.ID,
                        Name = equipment.Name
                    };

                    var standardList = (from x in db.EquipmentStandard
                                        join c in db.Standard
                                        on x.StandardUniqueID equals c.UniqueID
                                        where x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == "*"
                                        select new
                                        {
                                            x = x,
                                            c = c
                                        }).OrderBy(x => x.c.MaintenanceType).ThenBy(x => x.c.ID).ToList();

                    foreach (var standard in standardList)
                    {
                        model.StandardList.Add(new StandardModel()
                        {
                            UniqueID = standard.c.UniqueID,
                            MaintenanceType = standard.c.MaintenanceType,
                            ID = standard.c.ID,
                            Description = standard.c.Description,
                            IsFeelItem = standard.c.IsFeelItem,
                            IsAccumulation = standard.c.IsAccumulation,
                            IsInherit = standard.x.IsInherit,
                            OriLowerLimit = standard.c.LowerLimit.HasValue ? double.Parse(standard.c.LowerLimit.Value.ToString()) : default(double?),
                            OriLowerAlertLimit = standard.c.LowerAlertLimit.HasValue ? double.Parse(standard.c.LowerAlertLimit.Value.ToString()) : default(double?),
                            OriUpperAlertLimit = standard.c.UpperAlertLimit.HasValue ? double.Parse(standard.c.UpperAlertLimit.Value.ToString()) : default(double?),
                            OriUpperLimit = standard.c.UpperLimit.HasValue ? double.Parse(standard.c.UpperLimit.Value.ToString()) : default(double?),
                            OriAccumulationBase = standard.c.AccumulationBase.HasValue ? double.Parse(standard.c.AccumulationBase.Value.ToString()) : default(double?),
                            OriUnit = standard.c.Unit,
                            OriRemark = standard.c.Remark,
                            LowerLimit = standard.x.LowerLimit.HasValue ? double.Parse(standard.x.LowerLimit.Value.ToString()) : default(double?),
                            LowerAlertLimit = standard.x.LowerAlertLimit.HasValue ? double.Parse(standard.x.LowerAlertLimit.Value.ToString()) : default(double?),
                            UpperAlertLimit = standard.x.UpperAlertLimit.HasValue ? double.Parse(standard.x.UpperAlertLimit.Value.ToString()) : default(double?),
                            UpperLimit = standard.x.UpperLimit.HasValue ? double.Parse(standard.x.UpperLimit.Value.ToString()) : default(double?),
                            AccumulationBase = standard.x.AccumulationBase.HasValue ? double.Parse(standard.x.AccumulationBase.Value.ToString()) : default(double?),
                            Unit = standard.x.Unit,
                            Remark = standard.x.Remark
                        });
                    }

                    var partList = db.EquipmentPart.Where(x => x.EquipmentUniqueID == equipment.UniqueID).OrderBy(x => x.Description).ToList();

                    foreach (var part in partList)
                    {
                        var partModel = new PartModel()
                        {
                            UniqueID = part.UniqueID,
                            Description = part.Description
                        };

                        var partStandardList = (from x in db.EquipmentStandard
                                                join c in db.Standard
                                                on x.StandardUniqueID equals c.UniqueID
                                                where x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == part.UniqueID
                                                select new
                                                {
                                                    x = x,
                                                    c = c
                                                }).OrderBy(x => x.c.MaintenanceType).ThenBy(x => x.c.ID).ToList();

                        foreach (var standard in partStandardList)
                        {
                            partModel.StandardList.Add(new StandardModel()
                            {
                                UniqueID = standard.c.UniqueID,
                                MaintenanceType = standard.c.MaintenanceType,
                                ID = standard.c.ID,
                                Description = standard.c.Description,
                                IsFeelItem = standard.c.IsFeelItem,
                                IsAccumulation = standard.c.IsAccumulation,
                                IsInherit = standard.x.IsInherit,
                                OriLowerLimit = standard.c.LowerLimit.HasValue ? double.Parse(standard.c.LowerLimit.Value.ToString()) : default(double?),
                                OriLowerAlertLimit = standard.c.LowerAlertLimit.HasValue ? double.Parse(standard.c.LowerAlertLimit.Value.ToString()) : default(double?),
                                OriUpperAlertLimit = standard.c.UpperAlertLimit.HasValue ? double.Parse(standard.c.UpperAlertLimit.Value.ToString()) : default(double?),
                                OriUpperLimit = standard.c.UpperLimit.HasValue ? double.Parse(standard.c.UpperLimit.Value.ToString()) : default(double?),
                                OriAccumulationBase = standard.c.AccumulationBase.HasValue ? double.Parse(standard.c.AccumulationBase.Value.ToString()) : default(double?),
                                OriUnit = standard.c.Unit,
                                OriRemark = standard.c.Remark,
                                LowerLimit = standard.x.LowerLimit.HasValue ? double.Parse(standard.x.LowerLimit.Value.ToString()) : default(double?),
                                LowerAlertLimit = standard.x.LowerAlertLimit.HasValue ? double.Parse(standard.x.LowerAlertLimit.Value.ToString()) : default(double?),
                                UpperAlertLimit = standard.x.UpperAlertLimit.HasValue ? double.Parse(standard.x.UpperAlertLimit.Value.ToString()) : default(double?),
                                UpperLimit = standard.x.UpperLimit.HasValue ? double.Parse(standard.x.UpperLimit.Value.ToString()) : default(double?),
                                AccumulationBase = standard.x.AccumulationBase.HasValue ? double.Parse(standard.x.AccumulationBase.Value.ToString()) : default(double?),
                                Unit = standard.x.Unit,
                                Remark = standard.x.Remark
                            });
                        }

                        model.PartList.Add(partModel);
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
                using (EDbEntities db = new EDbEntities())
                {
                    var equipment = db.Equipment.First(x => x.UniqueID == Model.UniqueID);

#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                    #region Equipment
                    equipment.LastModifyTime = DateTime.Now;

                    db.SaveChanges();
                    #endregion

                    #region EquipmentPart, EquipmentStandard
                    #region Delete
                    db.EquipmentStandard.RemoveRange(db.EquipmentStandard.Where(x => x.EquipmentUniqueID == equipment.UniqueID).ToList());

                    db.SaveChanges();
                    #endregion

                    #region Insert
                    db.EquipmentStandard.AddRange(Model.StandardList.Select(x => new EquipmentStandard
                    {
                        EquipmentUniqueID = equipment.UniqueID,
                        PartUniqueID = "*",
                        StandardUniqueID = x.UniqueID,
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
                        db.EquipmentStandard.AddRange(part.StandardList.Select(x => new EquipmentStandard
                        {
                            EquipmentUniqueID = equipment.UniqueID,
                            PartUniqueID = part.UniqueID,
                            StandardUniqueID = x.UniqueID,
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
#if !DEBUG
                        trans.Complete();
                    }
#endif

                    result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.Equipment, Resources.Resource.Success));
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

        public static RequestResult SavePageState(List<StandardModel> StandardList, List<string> PageStateList)
        {
            RequestResult result = new RequestResult();

            try
            {
                foreach (string pageState in PageStateList)
                {
                    string[] temp = pageState.Split(Define.Seperators, StringSplitOptions.None);

                    string isInherit = temp[0];
                    string standardUniqueID = temp[1];
                    string lowerLimit = temp[2];
                    string lowerAlertLimit = temp[3];
                    string upperAlertLimit = temp[4];
                    string upperLimit = temp[5];
                    string accumulationBase = temp[6];
                    string unit = temp[7];
                    string remark = temp[8];

                    var standard = StandardList.First(x => x.UniqueID == standardUniqueID);

                    standard.IsInherit = isInherit == "Y";

                    if (!standard.IsInherit)
                    {
                        standard.LowerLimit = !string.IsNullOrEmpty(lowerLimit) ? double.Parse(lowerLimit) : default(double?);
                        standard.LowerAlertLimit = !string.IsNullOrEmpty(lowerAlertLimit) ? double.Parse(lowerAlertLimit) : default(double?);
                        standard.UpperAlertLimit = !string.IsNullOrEmpty(upperAlertLimit) ? double.Parse(upperAlertLimit) : default(double?);
                        standard.UpperLimit = !string.IsNullOrEmpty(upperLimit) ? double.Parse(upperLimit) : default(double?);
                        standard.AccumulationBase = !string.IsNullOrEmpty(accumulationBase) ? double.Parse(accumulationBase) : default(double?);
                        standard.Unit = unit;
                        standard.Remark = remark;
                    }
                    else
                    {
                        standard.LowerLimit = null;
                        standard.LowerAlertLimit = null;
                        standard.UpperAlertLimit = null;
                        standard.UpperLimit = null;
                        standard.AccumulationBase = null;
                        standard.Unit = string.Empty;
                        standard.Remark = string.Empty;
                    }
                }

                result.ReturnData(StandardList);
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
                    string standardUniqueID = temp[2];
                    string lowerLimit = temp[3];
                    string lowerAlertLimit = temp[4];
                    string upperAlertLimit = temp[5];
                    string upperLimit = temp[6];
                    string accumulationBase = temp[7];
                    string unit = temp[8];
                    string remark = temp[9];

                    var standard = PartList.First(x => x.UniqueID == partUniqueID).StandardList.First(x => x.UniqueID == standardUniqueID);

                    standard.IsInherit = isInherit == "Y";

                    if (!standard.IsInherit)
                    {
                        standard.LowerLimit = !string.IsNullOrEmpty(lowerLimit) ? double.Parse(lowerLimit) : default(double?);
                        standard.LowerAlertLimit = !string.IsNullOrEmpty(lowerAlertLimit) ? double.Parse(lowerAlertLimit) : default(double?);
                        standard.UpperAlertLimit = !string.IsNullOrEmpty(upperAlertLimit) ? double.Parse(upperAlertLimit) : default(double?);
                        standard.UpperLimit = !string.IsNullOrEmpty(upperLimit) ? double.Parse(upperLimit) : default(double?);
                        standard.AccumulationBase = !string.IsNullOrEmpty(accumulationBase) ? double.Parse(accumulationBase) : default(double?);
                        standard.Unit = unit;
                        standard.Remark = remark;
                    }
                    else
                    {
                        standard.LowerLimit = null;
                        standard.LowerAlertLimit = null;
                        standard.UpperAlertLimit = null;
                        standard.UpperLimit = null;
                        standard.AccumulationBase = null;
                        standard.Unit = string.Empty;
                        standard.Remark = string.Empty;
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

        public static RequestResult AddStandard(List<StandardModel> StandardList, List<string> SelectedList, string RefOrganizationUniqueID)
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
                        var maintenanceType = temp[1];
                        var standardUniqueID = temp[2];

                        if (!string.IsNullOrEmpty(standardUniqueID))
                        {
                            if (!StandardList.Any(x => x.UniqueID == standardUniqueID))
                            {
                                var standard = db.Standard.First(x => x.UniqueID == standardUniqueID);

                                StandardList.Add(new StandardModel()
                                {
                                    UniqueID = standard.UniqueID,
                                    MaintenanceType = standard.MaintenanceType,
                                    ID = standard.ID,
                                    Description = standard.Description,
                                    IsFeelItem = standard.IsFeelItem,
                                    IsAccumulation = standard.IsAccumulation,
                                    IsInherit = true,
                                    OriLowerLimit = standard.LowerLimit.HasValue ? double.Parse(standard.LowerLimit.Value.ToString()) : default(double?),
                                    OriLowerAlertLimit = standard.LowerAlertLimit.HasValue ? double.Parse(standard.LowerAlertLimit.Value.ToString()) : default(double?),
                                    OriUpperAlertLimit = standard.UpperAlertLimit.HasValue ? double.Parse(standard.UpperAlertLimit.Value.ToString()) : default(double?),
                                    OriUpperLimit = standard.UpperLimit.HasValue ? double.Parse(standard.UpperLimit.Value.ToString()) : default(double?),
                                    OriAccumulationBase = standard.AccumulationBase.HasValue ? double.Parse(standard.AccumulationBase.Value.ToString()) : default(double?),
                                    OriUnit = standard.Unit,
                                    OriRemark = standard.Remark
                                });
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(maintenanceType))
                            {
                                var standardList = db.Standard.Where(x => x.OrganizationUniqueID == organizationUniqueID && x.MaintenanceType == maintenanceType).ToList();

                                foreach (var standard in standardList)
                                {
                                    if (!StandardList.Any(x => x.UniqueID == standard.UniqueID))
                                    {
                                        StandardList.Add(new StandardModel()
                                        {
                                            UniqueID = standard.UniqueID,
                                            MaintenanceType = standard.MaintenanceType,
                                            ID = standard.ID,
                                            Description = standard.Description,
                                            IsFeelItem = standard.IsFeelItem,
                                            IsAccumulation = standard.IsAccumulation,
                                            IsInherit = true,
                                            OriLowerLimit = standard.LowerLimit.HasValue ? double.Parse(standard.LowerLimit.Value.ToString()) : default(double?),
                                            OriLowerAlertLimit = standard.LowerAlertLimit.HasValue ? double.Parse(standard.LowerAlertLimit.Value.ToString()) : default(double?),
                                            OriUpperAlertLimit = standard.UpperAlertLimit.HasValue ? double.Parse(standard.UpperAlertLimit.Value.ToString()) : default(double?),
                                            OriUpperLimit = standard.UpperLimit.HasValue ? double.Parse(standard.UpperLimit.Value.ToString()) : default(double?),
                                            OriAccumulationBase = standard.AccumulationBase.HasValue ? double.Parse(standard.AccumulationBase.Value.ToString()) : default(double?),
                                            OriUnit = standard.Unit,
                                            OriRemark = standard.Remark
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
                                        var standardList = db.Standard.Where(x => x.OrganizationUniqueID == organization).ToList();

                                        foreach (var standard in standardList)
                                        {
                                            if (!StandardList.Any(x => x.UniqueID == standard.UniqueID))
                                            {
                                                StandardList.Add(new StandardModel()
                                                {
                                                    UniqueID = standard.UniqueID,
                                                    MaintenanceType = standard.MaintenanceType,
                                                    ID = standard.ID,
                                                    Description = standard.Description,
                                                    IsFeelItem = standard.IsFeelItem,
                                                    IsAccumulation = standard.IsAccumulation,
                                                    IsInherit = true,
                                                    OriLowerLimit = standard.LowerLimit.HasValue ? double.Parse(standard.LowerLimit.Value.ToString()) : default(double?),
                                                    OriLowerAlertLimit = standard.LowerAlertLimit.HasValue ? double.Parse(standard.LowerAlertLimit.Value.ToString()) : default(double?),
                                                    OriUpperAlertLimit = standard.UpperAlertLimit.HasValue ? double.Parse(standard.UpperAlertLimit.Value.ToString()) : default(double?),
                                                    OriUpperLimit = standard.UpperLimit.HasValue ? double.Parse(standard.UpperLimit.Value.ToString()) : default(double?),
                                                    OriAccumulationBase = standard.AccumulationBase.HasValue ? double.Parse(standard.AccumulationBase.Value.ToString()) : default(double?),
                                                    OriUnit = standard.Unit,
                                                    OriRemark = standard.Remark
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                result.ReturnData(StandardList.OrderBy(x => x.MaintenanceType).ThenBy(x => x.ID).ToList());
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

                using (EDbEntities db = new EDbEntities())
                {
                    if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                    {
                        var equipmentList = db.Equipment.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

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
                            (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.Equipment.Any(x => x.OrganizationUniqueID == organization.UniqueID)))
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
