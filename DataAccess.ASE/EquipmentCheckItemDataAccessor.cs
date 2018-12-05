using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.ASE;
using Models.Shared;
using Models.Authenticated;
using Models.EquipmentMaintenance.EquipmentCheckItemManagement;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.ASE
{
    public class EquipmentCheckItemDataAccessor
    {
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
                            ID = x.ID,
                            Name = x.NAME
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var equipment = db.EQUIPMENT.First(x => x.UNIQUEID == UniqueID);

                    var model = new DetailViewModel()
                    {
                        UniqueID = equipment.UNIQUEID,
                        Permission = Account.OrganizationPermission(equipment.ORGANIZATIONUNIQUEID),
                        OrganizationUniqueID = equipment.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(equipment.ORGANIZATIONUNIQUEID),
                        ID = equipment.ID,
                        Name = equipment.NAME,
                        IsFeelItemDefaultNormal = equipment.ISFEELITEMDEFAULTNORMAL == "Y"
                    };

                    var checkItemList = (from x in db.EQUIPMENTCHECKITEM
                                         join c in db.CHECKITEM
                                         on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                         where x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.PARTUNIQUEID == "*"
                                         select new
                                         {
                                             x=x,
                                             c=c
                                         }).OrderBy(x => x.c.CHECKTYPE).ThenBy(x => x.c.ID).ToList();

                    foreach (var checkItem in checkItemList)
                    {
                        model.CheckItemList.Add(new CheckItemModel()
                        {
                            UniqueID = checkItem.c.UNIQUEID,
                            CheckType = checkItem.c.CHECKTYPE,
                            ID = checkItem.c.ID,
                            Description = checkItem.c.DESCRIPTION,
                            IsFeelItem = checkItem.c.ISFEELITEM == "Y",
                            IsAccumulation = checkItem.c.ISACCUMULATION == "Y",
                            IsInherit = checkItem.x.ISINHERIT == "Y",
                            OriLowerLimit = checkItem.c.LOWERLIMIT.HasValue ? double.Parse(checkItem.c.LOWERLIMIT.Value.ToString()) : default(double?),
                            OriLowerAlertLimit = checkItem.c.LOWERALERTLIMIT.HasValue ? double.Parse(checkItem.c.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                            OriUpperAlertLimit = checkItem.c.UPPERALERTLIMIT.HasValue ? double.Parse(checkItem.c.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                            OriUpperLimit = checkItem.c.UPPERLIMIT.HasValue ? double.Parse(checkItem.c.UPPERLIMIT.Value.ToString()) : default(double?),
                            OriAccumulationBase = checkItem.c.ACCUMULATIONBASE.HasValue ? double.Parse(checkItem.c.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                            OriUnit = checkItem.c.UNIT,
                            OriRemark = checkItem.c.REMARK,
                            LowerLimit = checkItem.x.LOWERLIMIT.HasValue ? double.Parse(checkItem.x.LOWERLIMIT.Value.ToString()) : default(double?),
                            LowerAlertLimit = checkItem.x.LOWERALERTLIMIT.HasValue ? double.Parse(checkItem.x.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                            UpperAlertLimit = checkItem.x.UPPERALERTLIMIT.HasValue ? double.Parse(checkItem.x.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                            UpperLimit = checkItem.x.UPPERLIMIT.HasValue ? double.Parse(checkItem.x.UPPERLIMIT.Value.ToString()) : default(double?),
                            AccumulationBase = checkItem.x.ACCUMULATIONBASE.HasValue ? double.Parse(checkItem.x.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                            Unit = checkItem.x.UNIT,
                            Remark = checkItem.x.REMARK
                        });
                    }

                    var partList = db.EQUIPMENTPART.Where(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID).OrderBy(x => x.DESCRIPTION).ToList();

                    foreach (var part in partList)
                    {
                        var partModel = new PartModel() {
                            UniqueID = part.UNIQUEID,
                            Description = part.DESCRIPTION,
                        };

                        var partCheckItemList = (from x in db.EQUIPMENTCHECKITEM
                                                 join c in db.CHECKITEM
                                                 on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                                 where x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.PARTUNIQUEID == part.UNIQUEID
                                                 select new
                                                 {
                                                     x = x,
                                                     c = c
                                                 }).OrderBy(x => x.c.CHECKTYPE).ThenBy(x => x.c.ID).ToList();

                        foreach (var checkItem in partCheckItemList)
                        {
                            partModel.CheckItemList.Add(new CheckItemModel()
                            {
                                UniqueID = checkItem.c.UNIQUEID,
                                CheckType = checkItem.c.CHECKTYPE,
                                ID = checkItem.c.ID,
                                Description = checkItem.c.DESCRIPTION,
                                IsFeelItem = checkItem.c.ISFEELITEM == "Y",
                                IsAccumulation = checkItem.c.ISACCUMULATION == "Y",
                                IsInherit = checkItem.x.ISINHERIT == "Y",
                                OriLowerLimit = checkItem.c.LOWERLIMIT.HasValue ? double.Parse(checkItem.c.LOWERLIMIT.Value.ToString()) : default(double?),
                                OriLowerAlertLimit = checkItem.c.LOWERALERTLIMIT.HasValue ? double.Parse(checkItem.c.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                                OriUpperAlertLimit = checkItem.c.UPPERALERTLIMIT.HasValue ? double.Parse(checkItem.c.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                                OriUpperLimit = checkItem.c.UPPERLIMIT.HasValue ? double.Parse(checkItem.c.UPPERLIMIT.Value.ToString()) : default(double?),
                                OriAccumulationBase = checkItem.c.ACCUMULATIONBASE.HasValue ? double.Parse(checkItem.c.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                                OriUnit = checkItem.c.UNIT,
                                OriRemark = checkItem.c.REMARK,
                                LowerLimit = checkItem.x.LOWERLIMIT.HasValue ? double.Parse(checkItem.x.LOWERLIMIT.Value.ToString()) : default(double?),
                                LowerAlertLimit = checkItem.x.LOWERALERTLIMIT.HasValue ? double.Parse(checkItem.x.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                                UpperAlertLimit = checkItem.x.UPPERALERTLIMIT.HasValue ? double.Parse(checkItem.x.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                                UpperLimit = checkItem.x.UPPERLIMIT.HasValue ? double.Parse(checkItem.x.UPPERLIMIT.Value.ToString()) : default(double?),
                                AccumulationBase = checkItem.x.ACCUMULATIONBASE.HasValue ? double.Parse(checkItem.x.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                                Unit = checkItem.x.UNIT,
                                Remark = checkItem.x.REMARK
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

        public static RequestResult GetCopyFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var equipment = db.EQUIPMENT.First(x => x.UNIQUEID == UniqueID);

                    var model = new CreateFormModel()
                    {
                        UniqueID = Guid.NewGuid().ToString(),
                        OrganizationUniqueID = equipment.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(equipment.ORGANIZATIONUNIQUEID),
                        FormInput = new FormInput()
                        {
                            Name = equipment.NAME,
                            IsFeelItemDefaultNormal = equipment.ISFEELITEMDEFAULTNORMAL == "Y"
                        },
                    };

                    var checkItemList = (from x in db.EQUIPMENTCHECKITEM
                                         join c in db.CHECKITEM
                                         on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                         where x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.PARTUNIQUEID == "*"
                                         select new
                                         {
                                             x = x,
                                             c = c
                                         }).OrderBy(x => x.c.CHECKTYPE).ThenBy(x => x.c.ID).ToList();

                    foreach (var checkItem in checkItemList)
                    {
                        model.CheckItemList.Add(new CheckItemModel()
                        {
                            UniqueID = checkItem.c.UNIQUEID,
                            CheckType = checkItem.c.CHECKTYPE,
                            ID = checkItem.c.ID,
                            Description = checkItem.c.DESCRIPTION,
                            IsFeelItem = checkItem.c.ISFEELITEM == "Y",
                            IsAccumulation = checkItem.c.ISACCUMULATION == "Y",
                            IsInherit = checkItem.x.ISINHERIT == "Y",
                            OriLowerLimit = checkItem.c.LOWERLIMIT.HasValue ? double.Parse(checkItem.c.LOWERLIMIT.Value.ToString()) : default(double?),
                            OriLowerAlertLimit = checkItem.c.LOWERALERTLIMIT.HasValue ? double.Parse(checkItem.c.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                            OriUpperAlertLimit = checkItem.c.UPPERALERTLIMIT.HasValue ? double.Parse(checkItem.c.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                            OriUpperLimit = checkItem.c.UPPERLIMIT.HasValue ? double.Parse(checkItem.c.UPPERLIMIT.Value.ToString()) : default(double?),
                            OriAccumulationBase = checkItem.c.ACCUMULATIONBASE.HasValue ? double.Parse(checkItem.c.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                            OriUnit = checkItem.c.UNIT,
                            OriRemark = checkItem.c.REMARK,
                            LowerLimit = checkItem.x.LOWERLIMIT.HasValue ? double.Parse(checkItem.x.LOWERLIMIT.Value.ToString()) : default(double?),
                            LowerAlertLimit = checkItem.x.LOWERALERTLIMIT.HasValue ? double.Parse(checkItem.x.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                            UpperAlertLimit = checkItem.x.UPPERALERTLIMIT.HasValue ? double.Parse(checkItem.x.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                            UpperLimit = checkItem.x.UPPERLIMIT.HasValue ? double.Parse(checkItem.x.UPPERLIMIT.Value.ToString()) : default(double?),
                            AccumulationBase = checkItem.x.ACCUMULATIONBASE.HasValue ? double.Parse(checkItem.x.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                            Unit = checkItem.x.UNIT,
                            Remark = checkItem.x.REMARK
                        });
                    }

                    var partList = db.EQUIPMENTPART.Where(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID).OrderBy(x => x.DESCRIPTION).ToList();

                    foreach (var part in partList)
                    {
                        var partModel = new PartModel()
                        {
                            UniqueID = Guid.NewGuid().ToString(),
                            Description = part.DESCRIPTION,
                        };

                        var partCheckItemList = (from x in db.EQUIPMENTCHECKITEM
                                                 join c in db.CHECKITEM
                                                 on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                                 where x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.PARTUNIQUEID == part.UNIQUEID
                                                 select new
                                                 {
                                                     x = x,
                                                     c = c
                                                 }).OrderBy(x => x.c.CHECKTYPE).ThenBy(x => x.c.ID).ToList();

                        foreach (var checkItem in partCheckItemList)
                        {
                            partModel.CheckItemList.Add(new CheckItemModel()
                            {
                                UniqueID = checkItem.c.UNIQUEID,
                                CheckType = checkItem.c.CHECKTYPE,
                                ID = checkItem.c.ID,
                                Description = checkItem.c.DESCRIPTION,
                                IsFeelItem = checkItem.c.ISFEELITEM == "Y",
                                IsAccumulation = checkItem.c.ISACCUMULATION == "Y",
                                IsInherit = checkItem.x.ISINHERIT == "Y",
                                OriLowerLimit = checkItem.c.LOWERLIMIT.HasValue ? double.Parse(checkItem.c.LOWERLIMIT.Value.ToString()) : default(double?),
                                OriLowerAlertLimit = checkItem.c.LOWERALERTLIMIT.HasValue ? double.Parse(checkItem.c.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                                OriUpperAlertLimit = checkItem.c.UPPERALERTLIMIT.HasValue ? double.Parse(checkItem.c.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                                OriUpperLimit = checkItem.c.UPPERLIMIT.HasValue ? double.Parse(checkItem.c.UPPERLIMIT.Value.ToString()) : default(double?),
                                OriAccumulationBase = checkItem.c.ACCUMULATIONBASE.HasValue ? double.Parse(checkItem.c.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                                OriUnit = checkItem.c.UNIT,
                                OriRemark = checkItem.c.REMARK,
                                LowerLimit = checkItem.x.LOWERLIMIT.HasValue ? double.Parse(checkItem.x.LOWERLIMIT.Value.ToString()) : default(double?),
                                LowerAlertLimit = checkItem.x.LOWERALERTLIMIT.HasValue ? double.Parse(checkItem.x.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                                UpperAlertLimit = checkItem.x.UPPERALERTLIMIT.HasValue ? double.Parse(checkItem.x.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                                UpperLimit = checkItem.x.UPPERLIMIT.HasValue ? double.Parse(checkItem.x.UPPERLIMIT.Value.ToString()) : default(double?),
                                AccumulationBase = checkItem.x.ACCUMULATIONBASE.HasValue ? double.Parse(checkItem.x.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                                Unit = checkItem.x.UNIT,
                                Remark = checkItem.x.REMARK
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
                            ISFEELITEMDEFAULTNORMAL = Model.FormInput.IsFeelItemDefaultNormal?"Y":"N",
                            LASTMODIFYTIME = DateTime.Now
                        });

                        db.EQUIPMENTCHECKITEM.AddRange(Model.CheckItemList.Select(x => new EQUIPMENTCHECKITEM
                        {
                            EQUIPMENTUNIQUEID = Model.UniqueID,
                            PARTUNIQUEID = "*",
                            CHECKITEMUNIQUEID = x.UniqueID,
                            ISINHERIT = x.IsInherit ? "Y" : "N",
                            LOWERLIMIT = x.LowerLimit.HasValue?decimal.Parse(x.LowerLimit.Value.ToString()):default(decimal?),
                            LOWERALERTLIMIT = x.LowerAlertLimit.HasValue ? decimal.Parse(x.LowerAlertLimit.Value.ToString()) : default(decimal?),
                            UPPERALERTLIMIT = x.UpperAlertLimit.HasValue ? decimal.Parse(x.UpperAlertLimit.Value.ToString()) : default(decimal?),
                            UPPERLIMIT = x.UpperLimit.HasValue ? decimal.Parse(x.UpperLimit.Value.ToString()) : default(decimal?),
                            ACCUMULATIONBASE = x.AccumulationBase.HasValue ? decimal.Parse(x.AccumulationBase.Value.ToString()) : default(decimal?),
                            UNIT = x.Unit,
                            REMARK = x.Remark
                        }).ToList());

                        foreach (var part in Model.PartList)
                        {
                            db.EQUIPMENTPART.Add(new EQUIPMENTPART()
                            {
                                UNIQUEID = part.UniqueID,
                                EQUIPMENTUNIQUEID = Model.UniqueID,
                                DESCRIPTION = part.Description
                            });

                            db.EQUIPMENTCHECKITEM.AddRange(part.CheckItemList.Select(x => new EQUIPMENTCHECKITEM
                            {
                                EQUIPMENTUNIQUEID = Model.UniqueID,
                                PARTUNIQUEID = part.UniqueID,
                                CHECKITEMUNIQUEID = x.UniqueID,
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

                    var model = new EditFormModel()
                    {
                        UniqueID = equipment.UNIQUEID,
                        OrganizationUniqueID = equipment.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(equipment.ORGANIZATIONUNIQUEID),
                        FormInput = new FormInput()
                        {
                            ID = equipment.ID,
                            Name = equipment.NAME,
                            IsFeelItemDefaultNormal = equipment.ISFEELITEMDEFAULTNORMAL == "Y"
                        }
                    };

                    var checkItemList = (from x in db.EQUIPMENTCHECKITEM
                                         join c in db.CHECKITEM
                                         on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                         where x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.PARTUNIQUEID == "*"
                                         select new
                                         {
                                             x = x,
                                             c = c
                                         }).OrderBy(x => x.c.CHECKTYPE).ThenBy(x => x.c.ID).ToList();

                    foreach (var checkItem in checkItemList)
                    {
                        model.CheckItemList.Add(new CheckItemModel()
                        {
                            UniqueID = checkItem.c.UNIQUEID,
                            CheckType = checkItem.c.CHECKTYPE,
                            ID = checkItem.c.ID,
                            Description = checkItem.c.DESCRIPTION,
                            IsFeelItem = checkItem.c.ISFEELITEM == "Y",
                            IsAccumulation = checkItem.c.ISACCUMULATION == "Y",
                            IsInherit = checkItem.x.ISINHERIT == "Y",
                            OriLowerLimit = checkItem.c.LOWERLIMIT.HasValue ? double.Parse(checkItem.c.LOWERLIMIT.Value.ToString()) : default(double?),
                            OriLowerAlertLimit = checkItem.c.LOWERALERTLIMIT.HasValue ? double.Parse(checkItem.c.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                            OriUpperAlertLimit = checkItem.c.UPPERALERTLIMIT.HasValue ? double.Parse(checkItem.c.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                            OriUpperLimit = checkItem.c.UPPERLIMIT.HasValue ? double.Parse(checkItem.c.UPPERLIMIT.Value.ToString()) : default(double?),
                            OriAccumulationBase = checkItem.c.ACCUMULATIONBASE.HasValue ? double.Parse(checkItem.c.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                            OriUnit = checkItem.c.UNIT,
                            OriRemark = checkItem.c.REMARK,
                            LowerLimit = checkItem.x.LOWERLIMIT.HasValue ? double.Parse(checkItem.x.LOWERLIMIT.Value.ToString()) : default(double?),
                            LowerAlertLimit = checkItem.x.LOWERALERTLIMIT.HasValue ? double.Parse(checkItem.x.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                            UpperAlertLimit = checkItem.x.UPPERALERTLIMIT.HasValue ? double.Parse(checkItem.x.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                            UpperLimit = checkItem.x.UPPERLIMIT.HasValue ? double.Parse(checkItem.x.UPPERLIMIT.Value.ToString()) : default(double?),
                            AccumulationBase = checkItem.x.ACCUMULATIONBASE.HasValue ? double.Parse(checkItem.x.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                            Unit = checkItem.x.UNIT,
                            Remark = checkItem.x.REMARK
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

                        var partCheckItemList = (from x in db.EQUIPMENTCHECKITEM
                                                 join c in db.CHECKITEM
                                                 on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                                 where x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && x.PARTUNIQUEID == part.UNIQUEID
                                                 select new
                                                 {
                                                     x = x,
                                                     c = c
                                                 }).OrderBy(x => x.c.CHECKTYPE).ThenBy(x => x.c.ID).ToList();

                        foreach (var checkItem in partCheckItemList)
                        {
                            partModel.CheckItemList.Add(new CheckItemModel()
                            {
                                UniqueID = checkItem.c.UNIQUEID,
                                CheckType = checkItem.c.CHECKTYPE,
                                ID = checkItem.c.ID,
                                Description = checkItem.c.DESCRIPTION,
                                IsFeelItem = checkItem.c.ISFEELITEM == "Y",
                                IsAccumulation = checkItem.c.ISACCUMULATION == "Y",
                                IsInherit = checkItem.x.ISINHERIT == "Y",
                                OriLowerLimit = checkItem.c.LOWERLIMIT.HasValue ? double.Parse(checkItem.c.LOWERLIMIT.Value.ToString()) : default(double?),
                                OriLowerAlertLimit = checkItem.c.LOWERALERTLIMIT.HasValue ? double.Parse(checkItem.c.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                                OriUpperAlertLimit = checkItem.c.UPPERALERTLIMIT.HasValue ? double.Parse(checkItem.c.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                                OriUpperLimit = checkItem.c.UPPERLIMIT.HasValue ? double.Parse(checkItem.c.UPPERLIMIT.Value.ToString()) : default(double?),
                                OriAccumulationBase = checkItem.c.ACCUMULATIONBASE.HasValue ? double.Parse(checkItem.c.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                                OriUnit = checkItem.c.UNIT,
                                OriRemark = checkItem.c.REMARK,
                                LowerLimit = checkItem.x.LOWERLIMIT.HasValue ? double.Parse(checkItem.x.LOWERLIMIT.Value.ToString()) : default(double?),
                                LowerAlertLimit = checkItem.x.LOWERALERTLIMIT.HasValue ? double.Parse(checkItem.x.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                                UpperAlertLimit = checkItem.x.UPPERALERTLIMIT.HasValue ? double.Parse(checkItem.x.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                                UpperLimit = checkItem.x.UPPERLIMIT.HasValue ? double.Parse(checkItem.x.UPPERLIMIT.Value.ToString()) : default(double?),
                                AccumulationBase = checkItem.x.ACCUMULATIONBASE.HasValue ? double.Parse(checkItem.x.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                                Unit = checkItem.x.UNIT,
                                Remark = checkItem.x.REMARK
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

                    var exists = db.EQUIPMENT.FirstOrDefault(x => x.UNIQUEID != equipment.UNIQUEID && x.ORGANIZATIONUNIQUEID == equipment.ORGANIZATIONUNIQUEID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                        #region Equipment
                        equipment.ID = Model.FormInput.ID;
                        equipment.NAME = Model.FormInput.Name;
                        equipment.ISFEELITEMDEFAULTNORMAL = Model.FormInput.IsFeelItemDefaultNormal?"Y":"N";
                        equipment.LASTMODIFYTIME = DateTime.Now;

                        db.SaveChanges();
                        #endregion

                        #region EquipmentPart, EquipmentCheckItem
                        #region Delete
                        db.EQUIPMENTPART.RemoveRange(db.EQUIPMENTPART.Where(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID).ToList());
                        db.EQUIPMENTCHECKITEM.RemoveRange(db.EQUIPMENTCHECKITEM.Where(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID).ToList());

                        db.SaveChanges();
                        #endregion

                        #region Insert
                        db.EQUIPMENTCHECKITEM.AddRange(Model.CheckItemList.Select(x => new EQUIPMENTCHECKITEM
                        {
                            EQUIPMENTUNIQUEID = equipment.UNIQUEID,
                            PARTUNIQUEID = "*",
                            CHECKITEMUNIQUEID = x.UniqueID,
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
                            db.EQUIPMENTPART.Add(new EQUIPMENTPART()
                            {
                                UNIQUEID = part.UniqueID,
                                EQUIPMENTUNIQUEID = equipment.UNIQUEID,
                                DESCRIPTION = part.Description
                            });

                            db.EQUIPMENTCHECKITEM.AddRange(part.CheckItemList.Select(x => new EQUIPMENTCHECKITEM
                            {
                                EQUIPMENTUNIQUEID = equipment.UNIQUEID,
                                PARTUNIQUEID = part.UniqueID,
                                CHECKITEMUNIQUEID = x.UniqueID,
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

                        #region EquipmentMaterial, EquipmentStandard
                        var partList = Model.PartList.Select(x => x.UniqueID).ToList();

                        db.EQUIPMENTSTANDARD.RemoveRange(db.EQUIPMENTSTANDARD.Where(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && !partList.Contains(x.PARTUNIQUEID)).ToList());
                        db.EQUIPMENTMATERIAL.RemoveRange(db.EQUIPMENTMATERIAL.Where(x => x.EQUIPMENTUNIQUEID == equipment.UNIQUEID && !partList.Contains(x.PARTUNIQUEID)).ToList());

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
                using (ASEDbEntities db = new ASEDbEntities())
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
                                var checkItem = db.CHECKITEM.First(x => x.UNIQUEID == checkItemUniqueID);

                                CheckItemList.Add(new CheckItemModel()
                                {
                                    UniqueID = checkItem.UNIQUEID,
                                    CheckType = checkItem.CHECKTYPE,
                                    ID = checkItem.ID,
                                    Description = checkItem.DESCRIPTION,
                                    IsFeelItem = checkItem.ISFEELITEM=="Y",
                                    IsAccumulation = checkItem.ISACCUMULATION=="Y",
                                    IsInherit = true,
                                    OriLowerLimit = checkItem.LOWERLIMIT.HasValue?double.Parse(checkItem.LOWERLIMIT.Value.ToString()):default(double?),
                                    OriLowerAlertLimit = checkItem.LOWERALERTLIMIT.HasValue ? double.Parse(checkItem.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                                    OriUpperAlertLimit = checkItem.UPPERALERTLIMIT.HasValue ? double.Parse(checkItem.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                                    OriUpperLimit = checkItem.UPPERLIMIT.HasValue ? double.Parse(checkItem.UPPERLIMIT.Value.ToString()) : default(double?),
                                    OriAccumulationBase = checkItem.ACCUMULATIONBASE.HasValue ? double.Parse(checkItem.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                                    OriUnit = checkItem.UNIT,
                                    OriRemark = checkItem.REMARK
                                });
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(checkType))
                            {
                                var checkItemList = db.CHECKITEM.Where(x => x.ORGANIZATIONUNIQUEID == organizationUniqueID && x.CHECKTYPE == checkType).ToList();

                                foreach (var checkItem in checkItemList)
                                {
                                    if (!CheckItemList.Any(x => x.UniqueID == checkItem.UNIQUEID))
                                    {
                                        CheckItemList.Add(new CheckItemModel()
                                        {
                                            UniqueID = checkItem.UNIQUEID,
                                            CheckType = checkItem.CHECKTYPE,
                                            ID = checkItem.ID,
                                            Description = checkItem.DESCRIPTION,
                                            IsFeelItem = checkItem.ISFEELITEM == "Y",
                                            IsAccumulation = checkItem.ISACCUMULATION == "Y",
                                            IsInherit = true,
                                            OriLowerLimit = checkItem.LOWERLIMIT.HasValue ? double.Parse(checkItem.LOWERLIMIT.Value.ToString()) : default(double?),
                                            OriLowerAlertLimit = checkItem.LOWERALERTLIMIT.HasValue ? double.Parse(checkItem.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                                            OriUpperAlertLimit = checkItem.UPPERALERTLIMIT.HasValue ? double.Parse(checkItem.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                                            OriUpperLimit = checkItem.UPPERLIMIT.HasValue ? double.Parse(checkItem.UPPERLIMIT.Value.ToString()) : default(double?),
                                            OriAccumulationBase = checkItem.ACCUMULATIONBASE.HasValue ? double.Parse(checkItem.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                                            OriUnit = checkItem.UNIT,
                                            OriRemark = checkItem.REMARK
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
                                        var checkItemList = db.CHECKITEM.Where(x => x.ORGANIZATIONUNIQUEID == organization).ToList();

                                        foreach (var checkItem in checkItemList)
                                        {
                                            if (!CheckItemList.Any(x => x.UniqueID == checkItem.UNIQUEID))
                                            {
                                                CheckItemList.Add(new CheckItemModel()
                                                {
                                                    UniqueID = checkItem.UNIQUEID,
                                                    CheckType = checkItem.CHECKTYPE,
                                                    ID = checkItem.ID,
                                                    Description = checkItem.DESCRIPTION,
                                                    IsFeelItem = checkItem.ISFEELITEM == "Y",
                                                    IsAccumulation = checkItem.ISACCUMULATION == "Y",
                                                    IsInherit = true,
                                                    OriLowerLimit = checkItem.LOWERLIMIT.HasValue ? double.Parse(checkItem.LOWERLIMIT.Value.ToString()) : default(double?),
                                                    OriLowerAlertLimit = checkItem.LOWERALERTLIMIT.HasValue ? double.Parse(checkItem.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                                                    OriUpperAlertLimit = checkItem.UPPERALERTLIMIT.HasValue ? double.Parse(checkItem.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                                                    OriUpperLimit = checkItem.UPPERLIMIT.HasValue ? double.Parse(checkItem.UPPERLIMIT.Value.ToString()) : default(double?),
                                                    OriAccumulationBase = checkItem.ACCUMULATIONBASE.HasValue ? double.Parse(checkItem.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                                                    OriUnit = checkItem.UNIT,
                                                    OriRemark = checkItem.REMARK
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
