using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.ASE;
using Models.Shared;
using Models.Authenticated;
using Models.EquipmentMaintenance.EquipmentStandardManagement;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.ASE
{
    public class EquipmentStandardDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.EQUIPMENT.Where(x => (downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID) || downStreamOrganizationList.Contains(x.PMORGANIZATIONUNIQUEID))  && (Account.QueryableOrganizationUniqueIDList.Contains(x.ORGANIZATIONUNIQUEID)||Account.QueryableOrganizationUniqueIDList.Contains(x.PMORGANIZATIONUNIQUEID))).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.ID.Contains(Parameters.Keyword) || x.NAME.Contains(Parameters.Keyword));
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
                            UniqueID = x.UNIQUEID,
                            Permission = Account.OrganizationPermission(x.ORGANIZATIONUNIQUEID),
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.ORGANIZATIONUNIQUEID),
                            MaintenanceOrganization = OrganizationDataAccessor.GetOrganizationDescription(x.PMORGANIZATIONUNIQUEID),
                            ID = x.ID,
                            Name = x.NAME
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var equipment = db.EQUIPMENT.First(x => x.UNIQUEID == UniqueID);

                    var model = new DetailViewModel()
                    {
                        UniqueID = equipment.UNIQUEID,
                        Permission = Account.OrganizationPermission(equipment.ORGANIZATIONUNIQUEID),
                        OrganizationUniqueID = equipment.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(equipment.ORGANIZATIONUNIQUEID),
                        MaintenanceOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(equipment.PMORGANIZATIONUNIQUEID),
                        ID = equipment.ID,
                        Name = equipment.NAME
                    };

                    var standardList = (from x in db.EQUIPMENTSTANDARD
                                         join c in db.STANDARD
                                         on x.STANDARDUNIQUEID equals c.UNIQUEID
                                         where x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.PARTUNIQUEID == "*"
                                         select new
                                         {
                                             x=x,
                                             c=c
                                         }).OrderBy(x => x.c.MAINTENANCETYPE).ThenBy(x => x.c.ID).ToList();

                    foreach (var standard in standardList)
                    {
                        model.StandardList.Add(new StandardModel()
                        {
                            UniqueID = standard.c.UNIQUEID,
                            MaintenanceType = standard.c.MAINTENANCETYPE,
                            ID = standard.c.ID,
                            Description = standard.c.DESCRIPTION,
                            IsFeelItem = standard.c.ISFEELITEM == "Y",
                            IsAccumulation = standard.c.ISACCUMULATION == "Y",
                            IsInherit = standard.x.ISINHERIT == "Y",
                            OriLowerLimit = standard.c.LOWERLIMIT.HasValue ? double.Parse(standard.c.LOWERLIMIT.Value.ToString()) : default(double?),
                            OriLowerAlertLimit = standard.c.LOWERALERTLIMIT.HasValue ? double.Parse(standard.c.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                            OriUpperAlertLimit = standard.c.UPPERALERTLIMIT.HasValue ? double.Parse(standard.c.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                            OriUpperLimit = standard.c.UPPERLIMIT.HasValue ? double.Parse(standard.c.UPPERLIMIT.Value.ToString()) : default(double?),
                            OriAccumulationBase = standard.c.ACCUMULATIONBASE.HasValue ? double.Parse(standard.c.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                            OriUnit = standard.c.UNIT,
                            OriRemark = standard.c.REMARK,
                            LowerLimit = standard.x.LOWERLIMIT.HasValue ? double.Parse(standard.x.LOWERLIMIT.Value.ToString()) : default(double?),
                            LowerAlertLimit = standard.x.LOWERALERTLIMIT.HasValue ? double.Parse(standard.x.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                            UpperAlertLimit = standard.x.UPPERALERTLIMIT.HasValue ? double.Parse(standard.x.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                            UpperLimit = standard.x.UPPERLIMIT.HasValue ? double.Parse(standard.x.UPPERLIMIT.Value.ToString()) : default(double?),
                            AccumulationBase = standard.x.ACCUMULATIONBASE.HasValue ? double.Parse(standard.x.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                            Unit = standard.x.UNIT,
                            Remark = standard.x.REMARK
                        });
                    }

                    var partList = db.EQUIPMENTPART.Where(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID).OrderBy(x => x.DESCRIPTION).ToList();

                    foreach (var part in partList)
                    {
                        var partModel = new PartModel()
                        {
                            UniqueID = part.UNIQUEID,
                            Description = part.DESCRIPTION,
                        };

                        var partStandardList = (from x in db.EQUIPMENTSTANDARD
                                                 join c in db.STANDARD
                                                 on x.STANDARDUNIQUEID equals c.UNIQUEID
                                                 where x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.PARTUNIQUEID == part.UNIQUEID
                                                 select new
                                                 {
                                                     x = x,
                                                     c = c
                                                 }).OrderBy(x => x.c.MAINTENANCETYPE).ThenBy(x => x.c.ID).ToList();

                        foreach (var standard in partStandardList)
                        {
                            partModel.StandardList.Add(new StandardModel()
                            {
                                UniqueID = standard.c.UNIQUEID,
                                MaintenanceType = standard.c.MAINTENANCETYPE,
                                ID = standard.c.ID,
                                Description = standard.c.DESCRIPTION,
                                IsFeelItem = standard.c.ISFEELITEM == "Y",
                                IsAccumulation = standard.c.ISACCUMULATION == "Y",
                                IsInherit = standard.x.ISINHERIT == "Y",
                                OriLowerLimit = standard.c.LOWERLIMIT.HasValue ? double.Parse(standard.c.LOWERLIMIT.Value.ToString()) : default(double?),
                                OriLowerAlertLimit = standard.c.LOWERALERTLIMIT.HasValue ? double.Parse(standard.c.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                                OriUpperAlertLimit = standard.c.UPPERALERTLIMIT.HasValue ? double.Parse(standard.c.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                                OriUpperLimit = standard.c.UPPERLIMIT.HasValue ? double.Parse(standard.c.UPPERLIMIT.Value.ToString()) : default(double?),
                                OriAccumulationBase = standard.c.ACCUMULATIONBASE.HasValue ? double.Parse(standard.c.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                                OriUnit = standard.c.UNIT,
                                OriRemark = standard.c.REMARK,
                                LowerLimit = standard.x.LOWERLIMIT.HasValue ? double.Parse(standard.x.LOWERLIMIT.Value.ToString()) : default(double?),
                                LowerAlertLimit = standard.x.LOWERALERTLIMIT.HasValue ? double.Parse(standard.x.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                                UpperAlertLimit = standard.x.UPPERALERTLIMIT.HasValue ? double.Parse(standard.x.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                                UpperLimit = standard.x.UPPERLIMIT.HasValue ? double.Parse(standard.x.UPPERLIMIT.Value.ToString()) : default(double?),
                                AccumulationBase = standard.x.ACCUMULATIONBASE.HasValue ? double.Parse(standard.x.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                                Unit = standard.x.UNIT,
                                Remark = standard.x.REMARK
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var equipment = db.EQUIPMENT.First(x => x.UNIQUEID == UniqueID);

                    var model = new EditFormModel()
                    {
                        UniqueID = equipment.UNIQUEID,
                        OrganizationUniqueID = equipment.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(equipment.ORGANIZATIONUNIQUEID),
                        MaintenanceOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(equipment.PMORGANIZATIONUNIQUEID),
                        ID = equipment.ID,
                        Name = equipment.NAME
                    };

                    var standardList = (from x in db.EQUIPMENTSTANDARD
                                         join c in db.STANDARD
                                         on x.STANDARDUNIQUEID equals c.UNIQUEID
                                         where x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.PARTUNIQUEID == "*"
                                         select new
                                         {
                                             x = x,
                                             c = c
                                         }).OrderBy(x => x.c.MAINTENANCETYPE).ThenBy(x => x.c.ID).ToList();

                    foreach (var standard in standardList)
                    {
                        model.StandardList.Add(new StandardModel()
                        {
                            UniqueID = standard.c.UNIQUEID,
                             MaintenanceType = standard.c.MAINTENANCETYPE,
                            ID = standard.c.ID,
                            Description = standard.c.DESCRIPTION,
                            IsFeelItem = standard.c.ISFEELITEM == "Y",
                            IsAccumulation = standard.c.ISACCUMULATION == "Y",
                            IsInherit = standard.x.ISINHERIT == "Y",
                            OriLowerLimit = standard.c.LOWERLIMIT.HasValue ? double.Parse(standard.c.LOWERLIMIT.Value.ToString()) : default(double?),
                            OriLowerAlertLimit = standard.c.LOWERALERTLIMIT.HasValue ? double.Parse(standard.c.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                            OriUpperAlertLimit = standard.c.UPPERALERTLIMIT.HasValue ? double.Parse(standard.c.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                            OriUpperLimit = standard.c.UPPERLIMIT.HasValue ? double.Parse(standard.c.UPPERLIMIT.Value.ToString()) : default(double?),
                            OriAccumulationBase = standard.c.ACCUMULATIONBASE.HasValue ? double.Parse(standard.c.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                            OriUnit = standard.c.UNIT,
                            OriRemark = standard.c.REMARK,
                            LowerLimit = standard.x.LOWERLIMIT.HasValue ? double.Parse(standard.x.LOWERLIMIT.Value.ToString()) : default(double?),
                            LowerAlertLimit = standard.x.LOWERALERTLIMIT.HasValue ? double.Parse(standard.x.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                            UpperAlertLimit = standard.x.UPPERALERTLIMIT.HasValue ? double.Parse(standard.x.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                            UpperLimit = standard.x.UPPERLIMIT.HasValue ? double.Parse(standard.x.UPPERLIMIT.Value.ToString()) : default(double?),
                            AccumulationBase = standard.x.ACCUMULATIONBASE.HasValue ? double.Parse(standard.x.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                            Unit = standard.x.UNIT,
                            Remark = standard.x.REMARK
                        });
                    }

                    var partList = db.EQUIPMENTPART.Where(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID).OrderBy(x => x.DESCRIPTION).ToList();

                    foreach (var part in partList)
                    {
                        var partModel = new PartModel()
                        {
                            UniqueID = part.UNIQUEID,
                            Description = part.DESCRIPTION
                        };

                        var partStandardList = (from x in db.EQUIPMENTSTANDARD
                                                 join c in db.STANDARD
                                                 on x.STANDARDUNIQUEID equals c.UNIQUEID
                                                 where x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.PARTUNIQUEID == part.UNIQUEID
                                                 select new
                                                 {
                                                     x = x,
                                                     c = c
                                                 }).OrderBy(x => x.c.MAINTENANCETYPE).ThenBy(x => x.c.ID).ToList();

                        foreach (var standard in partStandardList)
                        {
                            partModel.StandardList.Add(new StandardModel()
                            {
                                UniqueID = standard.c.UNIQUEID,
                                MaintenanceType = standard.c.MAINTENANCETYPE,
                                ID = standard.c.ID,
                                Description = standard.c.DESCRIPTION,
                                IsFeelItem = standard.c.ISFEELITEM == "Y",
                                IsAccumulation = standard.c.ISACCUMULATION == "Y",
                                IsInherit = standard.x.ISINHERIT == "Y",
                                OriLowerLimit = standard.c.LOWERLIMIT.HasValue ? double.Parse(standard.c.LOWERLIMIT.Value.ToString()) : default(double?),
                                OriLowerAlertLimit = standard.c.LOWERALERTLIMIT.HasValue ? double.Parse(standard.c.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                                OriUpperAlertLimit = standard.c.UPPERALERTLIMIT.HasValue ? double.Parse(standard.c.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                                OriUpperLimit = standard.c.UPPERLIMIT.HasValue ? double.Parse(standard.c.UPPERLIMIT.Value.ToString()) : default(double?),
                                OriAccumulationBase = standard.c.ACCUMULATIONBASE.HasValue ? double.Parse(standard.c.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                                OriUnit = standard.c.UNIT,
                                OriRemark = standard.c.REMARK,
                                LowerLimit = standard.x.LOWERLIMIT.HasValue ? double.Parse(standard.x.LOWERLIMIT.Value.ToString()) : default(double?),
                                LowerAlertLimit = standard.x.LOWERALERTLIMIT.HasValue ? double.Parse(standard.x.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                                UpperAlertLimit = standard.x.UPPERALERTLIMIT.HasValue ? double.Parse(standard.x.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                                UpperLimit = standard.x.UPPERLIMIT.HasValue ? double.Parse(standard.x.UPPERLIMIT.Value.ToString()) : default(double?),
                                AccumulationBase = standard.x.ACCUMULATIONBASE.HasValue ? double.Parse(standard.x.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                                Unit = standard.x.UNIT,
                                Remark = standard.x.REMARK
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var equipment = db.EQUIPMENT.First(x => x.UNIQUEID == Model.UniqueID);

#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                    #region Equipment
                    equipment.LASTMODIFYTIME = DateTime.Now;

                    db.SaveChanges();
                    #endregion

                    #region EquipmentPart, EquipmentStandard
                    #region Delete
                    db.EQUIPMENTSTANDARD.RemoveRange(db.EQUIPMENTSTANDARD.Where(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID).ToList());

                    db.SaveChanges();
                    #endregion

                    #region Insert
                    db.EQUIPMENTSTANDARD.AddRange(Model.StandardList.Select(x => new EQUIPMENTSTANDARD
                    {
                        EQUIPMENTUNIQUEID = equipment.UNIQUEID,
                        PARTUNIQUEID = "*",
                        STANDARDUNIQUEID = x.UniqueID,
                        ISINHERIT = x.IsInherit ? "Y" : "N",
                        LOWERLIMIT = x.LowerLimit.HasValue ? decimal.Parse(x.LowerLimit.Value.ToString()) : default(decimal?),
                        LOWERALERTLIMIT = x.LowerAlertLimit.HasValue ? decimal.Parse(x.LowerAlertLimit.Value.ToString()) : default(decimal?),
                        UPPERALERTLIMIT = x.UpperAlertLimit.HasValue ? decimal.Parse(x.UpperAlertLimit.Value.ToString()) : default(decimal?),
                        UPPERLIMIT = x.UpperLimit.HasValue ? decimal.Parse(x.UpperLimit.Value.ToString()) : default(decimal?),
                        ACCUMULATIONBASE = x.AccumulationBase.HasValue ? decimal.Parse(x.AccumulationBase.Value.ToString()) : default(decimal?),
                        UNIT = x.Unit,
                        REMARK = x.Remark
                    }).ToList());

                    foreach (var part in Model.PartList)
                    {
                        db.EQUIPMENTSTANDARD.AddRange(part.StandardList.Select(x => new EQUIPMENTSTANDARD
                        {
                            EQUIPMENTUNIQUEID = equipment.UNIQUEID,
                            PARTUNIQUEID = part.UniqueID,
                            STANDARDUNIQUEID = x.UniqueID,
                            ISINHERIT = x.IsInherit ? "Y" : "N",
                            LOWERLIMIT = x.LowerLimit.HasValue ? decimal.Parse(x.LowerLimit.Value.ToString()) : default(decimal?),
                            LOWERALERTLIMIT = x.LowerAlertLimit.HasValue ? decimal.Parse(x.LowerAlertLimit.Value.ToString()) : default(decimal?),
                            UPPERALERTLIMIT = x.UpperAlertLimit.HasValue ? decimal.Parse(x.UpperAlertLimit.Value.ToString()) : default(decimal?),
                            UPPERLIMIT = x.UpperLimit.HasValue ? decimal.Parse(x.UpperLimit.Value.ToString()) : default(decimal?),
                            ACCUMULATIONBASE = x.AccumulationBase.HasValue ? decimal.Parse(x.AccumulationBase.Value.ToString()) : default(decimal?),
                            UNIT = x.Unit,
                            REMARK = x.Remark
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
                using (ASEDbEntities db = new ASEDbEntities())
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
                                var standard = db.STANDARD.First(x => x.UNIQUEID == standardUniqueID);

                                StandardList.Add(new StandardModel()
                                {
                                    UniqueID = standard.UNIQUEID,
                                    MaintenanceType = standard.MAINTENANCETYPE,
                                    ID = standard.ID,
                                    Description = standard.DESCRIPTION,
                                    IsFeelItem = standard.ISFEELITEM=="Y",
                                    IsAccumulation = standard.ISACCUMULATION=="Y",
                                    IsInherit = true,
                                    OriLowerLimit = standard.LOWERLIMIT.HasValue?double.Parse(standard.LOWERLIMIT.Value.ToString()):default(double?),
                                    OriLowerAlertLimit = standard.LOWERALERTLIMIT.HasValue ? double.Parse(standard.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                                    OriUpperAlertLimit = standard.UPPERALERTLIMIT.HasValue ? double.Parse(standard.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                                    OriUpperLimit = standard.UPPERLIMIT.HasValue ? double.Parse(standard.UPPERLIMIT.Value.ToString()) : default(double?),
                                    OriAccumulationBase = standard.ACCUMULATIONBASE.HasValue ? double.Parse(standard.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                                    OriUnit = standard.UNIT,
                                    OriRemark = standard.REMARK
                                });
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(maintenanceType))
                            {
                                var standardList = db.STANDARD.Where(x => x.ORGANIZATIONUNIQUEID == organizationUniqueID && x.MAINTENANCETYPE == maintenanceType).ToList();

                                foreach (var standard in standardList)
                                {
                                    if (!StandardList.Any(x => x.UniqueID == standard.UNIQUEID))
                                    {
                                        StandardList.Add(new StandardModel()
                                        {
                                            UniqueID = standard.UNIQUEID,
                                            MaintenanceType = standard.MAINTENANCETYPE,
                                            ID = standard.ID,
                                            Description = standard.DESCRIPTION,
                                            IsFeelItem = standard.ISFEELITEM == "Y",
                                            IsAccumulation = standard.ISACCUMULATION == "Y",
                                            IsInherit = true,
                                            OriLowerLimit = standard.LOWERLIMIT.HasValue ? double.Parse(standard.LOWERLIMIT.Value.ToString()) : default(double?),
                                            OriLowerAlertLimit = standard.LOWERALERTLIMIT.HasValue ? double.Parse(standard.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                                            OriUpperAlertLimit = standard.UPPERALERTLIMIT.HasValue ? double.Parse(standard.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                                            OriUpperLimit = standard.UPPERLIMIT.HasValue ? double.Parse(standard.UPPERLIMIT.Value.ToString()) : default(double?),
                                            OriAccumulationBase = standard.ACCUMULATIONBASE.HasValue ? double.Parse(standard.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                                            OriUnit = standard.UNIT,
                                            OriRemark = standard.REMARK
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
                                        var standardList = db.STANDARD.Where(x => x.ORGANIZATIONUNIQUEID == organization).ToList();

                                        foreach (var standard in standardList)
                                        {
                                            if (!StandardList.Any(x => x.UniqueID == standard.UNIQUEID))
                                            {
                                                StandardList.Add(new StandardModel()
                                                {
                                                    UniqueID = standard.UNIQUEID,
                                                    MaintenanceType = standard.MAINTENANCETYPE,
                                                    ID = standard.ID,
                                                    Description = standard.DESCRIPTION,
                                                    IsFeelItem = standard.ISFEELITEM == "Y",
                                                    IsAccumulation = standard.ISACCUMULATION == "Y",
                                                    IsInherit = true,
                                                    OriLowerLimit = standard.LOWERLIMIT.HasValue ? double.Parse(standard.LOWERLIMIT.Value.ToString()) : default(double?),
                                                    OriLowerAlertLimit = standard.LOWERALERTLIMIT.HasValue ? double.Parse(standard.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                                                    OriUpperAlertLimit = standard.UPPERALERTLIMIT.HasValue ? double.Parse(standard.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                                                    OriUpperLimit = standard.UPPERLIMIT.HasValue ? double.Parse(standard.UPPERLIMIT.Value.ToString()) : default(double?),
                                                    OriAccumulationBase = standard.ACCUMULATIONBASE.HasValue ? double.Parse(standard.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                                                    OriUnit = standard.UNIT,
                                                    OriRemark = standard.REMARK
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

                        if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                            ||
                            (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.EQUIPMENT.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID)))
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
