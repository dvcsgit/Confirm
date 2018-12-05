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
using Models.EquipmentMaintenance.StandardManagement;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.ASE
{
    public class StandardDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.STANDARD.Where(x => downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID) && Account.QueryableOrganizationUniqueIDList.Contains(x.ORGANIZATIONUNIQUEID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.MaintenanceType))
                    {
                        query = query.Where(x => x.ORGANIZATIONUNIQUEID == Parameters.OrganizationUniqueID && x.MAINTENANCETYPE == Parameters.MaintenanceType);
                    }

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.ID.Contains(Parameters.Keyword) || x.DESCRIPTION.Contains(Parameters.Keyword));
                    }

                    var organization = OrganizationDataAccessor.GetOrganization(Parameters.OrganizationUniqueID);

                    result.ReturnData(new GridViewModel()
                    {
                        OrganizationUniqueID = Parameters.OrganizationUniqueID,
                        Permission = Account.OrganizationPermission(Parameters.OrganizationUniqueID),
                        MaintenanceType = Parameters.MaintenanceType,
                        OrganizationDescription = organization.Description,
                        FullOrganizationDescription = organization.FullDescription,
                        ItemList = query.ToList().Select(x => new GridItem()
                        {
                            UniqueID = x.UNIQUEID,
                            Permission = Account.OrganizationPermission(x.ORGANIZATIONUNIQUEID),
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.ORGANIZATIONUNIQUEID),
                            MaintenanceType = x.MAINTENANCETYPE,
                            ID = x.ID,
                            Description = x.DESCRIPTION,
                            Unit = x.UNIT
                        }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.MaintenanceType).ThenBy(x => x.ID).ToList()
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
                    var standard = db.STANDARD.First(x => x.UNIQUEID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = standard.UNIQUEID,
                        Permission = Account.OrganizationPermission(standard.ORGANIZATIONUNIQUEID),
                        OrganizationUniqueID = standard.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(standard.ORGANIZATIONUNIQUEID),
                        MaintenanceType = standard.MAINTENANCETYPE,
                        ID = standard.ID,
                        Description = standard.DESCRIPTION,
                        IsFeelItem = standard.ISFEELITEM == "Y",
                        UpperLimit = standard.UPPERLIMIT.HasValue ? standard.UPPERLIMIT.ToString() : "",
                        UpperAlertLimit = standard.UPPERALERTLIMIT.HasValue ? standard.UPPERALERTLIMIT.ToString() : "",
                        LowerAlertLimit = standard.LOWERALERTLIMIT.HasValue ? standard.LOWERALERTLIMIT.ToString() : "",
                        LowerLimit = standard.LOWERLIMIT.HasValue ? standard.LOWERLIMIT.ToString() : "",
                        IsAccumulation = standard.ISACCUMULATION == "Y",
                        AccumulationBase = standard.ACCUMULATIONBASE.HasValue ? standard.ACCUMULATIONBASE.ToString() : "",
                        Unit = standard.UNIT,
                        Remark = standard.REMARK,
                        AbnormalReasonDescriptionList = (from x in db.STANDARDABNORMALREASON
                                                         join a in db.ABNORMALREASON
                                                         on x.ABNORMALREASONUNIQUEID equals a.UNIQUEID
                                                         where x.STANDARDUNIQUEID == UniqueID
                                                         orderby a.ABNORMALTYPE, a.ID
                                                         select a.DESCRIPTION).ToList(),
                        FeelOptionDescriptionList = db.STANDARDFEELOPTION.Where(x => x.STANDARDUNIQUEID == UniqueID).OrderBy(x => x.SEQ).ToList().Select(x => x.DESCRIPTION + (x.ISABNORMAL == "Y" ? "(" + Resources.Resource.Abnormal + ")" : "")).ToList()
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

        public static RequestResult GetCreateFormModel(string OrganizationUniqueID, string MaintenanceType)
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
                        MaintenanceTypeSelectItemList = new List<SelectListItem>() 
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
                            MaintenanceType = MaintenanceType
                        },
                        FeelOptionList = new List<FeelOptionModel>() 
                        { 
                            new FeelOptionModel() 
                            {
                                Description = Resources.Resource.Normal,
                                IsAbnormal = false,
                                Seq = 1
                            },
                            new FeelOptionModel() 
                            {
                                Description = Resources.Resource.Abnormal,
                                IsAbnormal = true,
                                Seq = 2
                            }
                        }
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(OrganizationUniqueID, true);

                    model.MaintenanceTypeSelectItemList.AddRange(db.STANDARD.Where(x => upStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)).Select(x => x.MAINTENANCETYPE).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    if (!string.IsNullOrEmpty(MaintenanceType) && model.MaintenanceTypeSelectItemList.Any(x => x.Value == MaintenanceType))
                    {
                        model.MaintenanceTypeSelectItemList.First(x => x.Value == MaintenanceType).Selected = true;
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
                    var standard = db.STANDARD.First(x => x.UNIQUEID == UniqueID);

                    var model = new CreateFormModel()
                    {
                        OrganizationUniqueID = standard.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(standard.ORGANIZATIONUNIQUEID),
                        MaintenanceTypeSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne),
                            new SelectListItem()
                            {
                                Text = Resources.Resource.Create + "...",
                                Value = Define.NEW
                            }
                        },
                        AbnormalReasonList = (from x in db.STANDARDABNORMALREASON
                                              join a in db.ABNORMALREASON
                                              on x.ABNORMALREASONUNIQUEID equals a.UNIQUEID
                                              where x.STANDARDUNIQUEID == UniqueID
                                              select new AbnormalReasonModel
                                              {
                                                  UniqueID = a.UNIQUEID,
                                                  AbnormalType = a.ABNORMALTYPE,
                                                  ID = a.ID,
                                                  Description = a.DESCRIPTION
                                              }).OrderBy(x => x.AbnormalType).ThenBy(x => x.ID).ToList(),
                        FormInput = new FormInput()
                        {
                            MaintenanceType = standard.MAINTENANCETYPE,
                            IsFeelItem = standard.ISFEELITEM=="Y",
                            LowerLimit = standard.LOWERLIMIT.HasValue ? standard.LOWERLIMIT.ToString() : "",
                            LowerAlertLimit = standard.LOWERALERTLIMIT.HasValue ? standard.LOWERALERTLIMIT.ToString() : "",
                            UpperAlertLimit = standard.UPPERALERTLIMIT.HasValue ? standard.UPPERALERTLIMIT.ToString() : "",
                            UpperLimit = standard.UPPERLIMIT.HasValue ? standard.UPPERLIMIT.ToString() : "",
                            IsAccumulation = standard.ISACCUMULATION=="Y",
                            AccumulationBase = standard.ACCUMULATIONBASE.HasValue ? standard.ACCUMULATIONBASE.ToString() : "",
                            Unit = standard.UNIT,
                            Remark = standard.REMARK
                        }
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(standard.ORGANIZATIONUNIQUEID, true);

                    model.MaintenanceTypeSelectItemList.AddRange(db.STANDARD.Where(x => upStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)).Select(x => x.MAINTENANCETYPE).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    model.MaintenanceTypeSelectItemList.First(x => x.Value == standard.MAINTENANCETYPE).Selected = true;

                    if (standard.ISFEELITEM=="Y")
                    {
                        model.FeelOptionList = (from x in db.STANDARDFEELOPTION
                                                where x.STANDARDUNIQUEID == UniqueID
                                                select new FeelOptionModel
                                                {
                                                    UniqueID = x.UNIQUEID,
                                                    Description = x.DESCRIPTION,
                                                    IsAbnormal = x.ISABNORMAL=="Y",
                                                    Seq = x.SEQ.Value
                                                }).OrderBy(x => x.Seq).ToList();
                    }
                    else
                    {
                        model.FeelOptionList = new List<FeelOptionModel>() 
                        { 
                            new FeelOptionModel() 
                            {
                                Description = Resources.Resource.Normal,
                                IsAbnormal = false,
                                Seq = 1
                            },
                            new FeelOptionModel() 
                            {
                                Description = Resources.Resource.Abnormal,
                                IsAbnormal = true,
                                Seq = 2
                            }
                        };
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
                if (Model.FormInput.MaintenanceType == Define.OTHER || Model.FormInput.MaintenanceType == Define.NEW)
                {
                    result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.Unsupported, Resources.Resource.MaintenanceType));
                }
                else
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var exists = db.STANDARD.FirstOrDefault(x => x.ORGANIZATIONUNIQUEID == Model.OrganizationUniqueID && x.MAINTENANCETYPE == Model.FormInput.MaintenanceType && x.ID == Model.FormInput.ID);

                        if (exists == null)
                        {
                            string uniqueID = Guid.NewGuid().ToString();

                            db.STANDARD.Add(new STANDARD()
                            {
                                UNIQUEID = uniqueID,
                                ORGANIZATIONUNIQUEID = Model.OrganizationUniqueID,
                                MAINTENANCETYPE = Model.FormInput.MaintenanceType,
                                ID = Model.FormInput.ID,
                                DESCRIPTION = Model.FormInput.Description,
                                ISFEELITEM = Model.FormInput.IsFeelItem?"Y":"N",
                                UPPERLIMIT = !string.IsNullOrEmpty(Model.FormInput.UpperLimit) ? decimal.Parse(Model.FormInput.UpperLimit) : default(decimal?),
                                UPPERALERTLIMIT = !string.IsNullOrEmpty(Model.FormInput.UpperAlertLimit) ? decimal.Parse(Model.FormInput.UpperAlertLimit) : default(decimal?),
                                LOWERALERTLIMIT = !string.IsNullOrEmpty(Model.FormInput.LowerAlertLimit) ? decimal.Parse(Model.FormInput.LowerAlertLimit) : default(decimal?),
                                LOWERLIMIT = !string.IsNullOrEmpty(Model.FormInput.LowerLimit) ? decimal.Parse(Model.FormInput.LowerLimit) : default(decimal?),
                                ISACCUMULATION = Model.FormInput.IsAccumulation ? "Y" : "N",
                                ACCUMULATIONBASE = !string.IsNullOrEmpty(Model.FormInput.AccumulationBase) ? decimal.Parse(Model.FormInput.AccumulationBase) : default(decimal?),
                                UNIT = Model.FormInput.Unit,
                                REMARK = Model.FormInput.Remark,
                                LASTMODIFYTIME = DateTime.Now
                            });

                            if (Model.FormInput.IsFeelItem)
                            {
                                db.STANDARDFEELOPTION.AddRange(Model.FormInput.FeelOptionList.Select(x => new STANDARDFEELOPTION
                                {
                                    UNIQUEID = Guid.NewGuid().ToString(),
                                    STANDARDUNIQUEID = uniqueID,
                                    DESCRIPTION = x.Description,
                                    ISABNORMAL = x.IsAbnormal?"Y":"N",
                                    SEQ = x.Seq
                                }).ToList());
                            }

                            db.STANDARDABNORMALREASON.AddRange(Model.AbnormalReasonList.Select(x => new STANDARDABNORMALREASON
                            {
                                STANDARDUNIQUEID = uniqueID,
                                ABNORMALREASONUNIQUEID = x.UniqueID
                            }).ToList());

                            db.SaveChanges();

                            result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.Standard, Resources.Resource.Success));
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.StandardID, Resources.Resource.Exists));
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
                    var standard = db.STANDARD.First(x => x.UNIQUEID == UniqueID);

                    var model = new EditFormModel()
                    {
                        UniqueID = standard.UNIQUEID,
                        OrganizationUniqueID = standard.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(standard.ORGANIZATIONUNIQUEID),
                        MaintenanceTypeSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne),
                            new SelectListItem()
                            {
                                Text = Resources.Resource.Create + "...",
                                Value = Define.NEW
                            }
                        },
                        AbnormalReasonList = (from x in db.STANDARDABNORMALREASON
                                              join a in db.ABNORMALREASON
                                              on x.ABNORMALREASONUNIQUEID equals a.UNIQUEID
                                              where x.STANDARDUNIQUEID == UniqueID
                                              select new AbnormalReasonModel
                                              {
                                                  UniqueID = a.UNIQUEID,
                                                  AbnormalType = a.ABNORMALTYPE,
                                                  ID = a.ID,
                                                  Description = a.DESCRIPTION
                                              }).OrderBy(x => x.AbnormalType).ThenBy(x => x.ID).ToList(),
                        FormInput = new FormInput()
                        {
                            MaintenanceType = standard.MAINTENANCETYPE,
                            ID = standard.ID,
                            Description = standard.DESCRIPTION,
                            IsFeelItem = standard.ISFEELITEM=="Y",
                            LowerLimit = standard.LOWERLIMIT.HasValue ? standard.LOWERLIMIT.ToString() : "",
                            LowerAlertLimit = standard.LOWERALERTLIMIT.HasValue ? standard.LOWERALERTLIMIT.ToString() : "",
                            UpperAlertLimit = standard.UPPERALERTLIMIT.HasValue ? standard.UPPERALERTLIMIT.ToString() : "",
                            UpperLimit = standard.UPPERLIMIT.HasValue ? standard.UPPERLIMIT.ToString() : "",
                            IsAccumulation = standard.ISACCUMULATION == "Y",
                            AccumulationBase = standard.ACCUMULATIONBASE.HasValue ? standard.ACCUMULATIONBASE.ToString() : "",
                            Unit = standard.UNIT,
                            Remark = standard.REMARK
                        }
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(model.OrganizationUniqueID, true);
                    
                    model.MaintenanceTypeSelectItemList.AddRange(db.STANDARD.Where(x => upStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)).Select(x => x.MAINTENANCETYPE).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    if (!string.IsNullOrEmpty(model.FormInput.MaintenanceType) && model.MaintenanceTypeSelectItemList.Any(x => x.Value == model.FormInput.MaintenanceType))
                    {
                        model.MaintenanceTypeSelectItemList.First(x => x.Value == model.FormInput.MaintenanceType).Selected = true;
                    }

                    if (standard.ISFEELITEM=="Y")
                    {
                        model.FeelOptionList = (from x in db.STANDARDFEELOPTION
                                                where x.STANDARDUNIQUEID == UniqueID
                                                select new FeelOptionModel
                                                {
                                                    UniqueID = x.UNIQUEID,
                                                    Description = x.DESCRIPTION,
                                                    IsAbnormal = x.ISABNORMAL=="Y",
                                                    Seq = x.SEQ.Value
                                                }).OrderBy(x => x.Seq).ToList();
                    }
                    else
                    {
                        model.FeelOptionList = new List<FeelOptionModel>() 
                        { 
                            new FeelOptionModel() 
                            {
                                Description = Resources.Resource.Normal,
                                IsAbnormal = false,
                                Seq = 1
                            },
                            new FeelOptionModel() 
                            {
                                Description = Resources.Resource.Abnormal,
                                IsAbnormal = true,
                                Seq = 2
                            }
                        };
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
                if (Model.FormInput.MaintenanceType == Define.OTHER || Model.FormInput.MaintenanceType == Define.NEW)
                {
                    result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.Unsupported, Resources.Resource.MaintenanceType));
                }
                else
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var standard = db.STANDARD.First(x => x.UNIQUEID == Model.UniqueID);

                        var exists = db.STANDARD.FirstOrDefault(x => x.UNIQUEID != standard.UNIQUEID && x.ORGANIZATIONUNIQUEID == standard.ORGANIZATIONUNIQUEID && x.MAINTENANCETYPE == Model.FormInput.MaintenanceType && x.ID == Model.FormInput.ID);

                        if (exists == null)
                        {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                            #region Standard
                            standard.MAINTENANCETYPE = Model.FormInput.MaintenanceType;
                            standard.ID = Model.FormInput.ID;
                            standard.DESCRIPTION = Model.FormInput.Description;
                            standard.ISFEELITEM = Model.FormInput.IsFeelItem?"Y":"N";
                            standard.UPPERLIMIT = !string.IsNullOrEmpty(Model.FormInput.UpperLimit) ? decimal.Parse(Model.FormInput.UpperLimit) : default(decimal?);
                            standard.UPPERALERTLIMIT = !string.IsNullOrEmpty(Model.FormInput.UpperAlertLimit) ? decimal.Parse(Model.FormInput.UpperAlertLimit) : default(decimal?);
                            standard.LOWERALERTLIMIT = !string.IsNullOrEmpty(Model.FormInput.LowerAlertLimit) ? decimal.Parse(Model.FormInput.LowerAlertLimit) : default(decimal?);
                            standard.LOWERLIMIT = !string.IsNullOrEmpty(Model.FormInput.LowerLimit) ? decimal.Parse(Model.FormInput.LowerLimit) : default(decimal?);
                            standard.ISACCUMULATION = Model.FormInput.IsAccumulation ? "Y" : "N";
                            standard.ACCUMULATIONBASE = !string.IsNullOrEmpty(Model.FormInput.AccumulationBase) ? decimal.Parse(Model.FormInput.AccumulationBase) : default(decimal?);
                            standard.REMARK = Model.FormInput.Remark;
                            standard.UNIT = Model.FormInput.Unit;
                            standard.LASTMODIFYTIME = DateTime.Now;

                            db.SaveChanges();
                            #endregion

                            #region StandardFeelOption
                            #region Delete
                            db.STANDARDFEELOPTION.RemoveRange(db.STANDARDFEELOPTION.Where(x => x.STANDARDUNIQUEID == Model.UniqueID).ToList());

                            db.SaveChanges();
                            #endregion

                            #region Insert
                            if (Model.FormInput.IsFeelItem)
                            {
                                db.STANDARDFEELOPTION.AddRange(Model.FormInput.FeelOptionList.Select(x => new STANDARDFEELOPTION
                                {
                                    UNIQUEID = !string.IsNullOrEmpty(x.UniqueID) ? x.UniqueID : Guid.NewGuid().ToString(),
                                    STANDARDUNIQUEID = standard.UNIQUEID,
                                    DESCRIPTION = x.Description,
                                    ISABNORMAL = x.IsAbnormal ? "Y" : "N",
                                    SEQ = x.Seq
                                }).ToList());
                            }

                            db.SaveChanges();
                            #endregion
                            #endregion

                            #region StandardAbnormalReason
                            #region Delete
                            db.STANDARDABNORMALREASON.RemoveRange(db.STANDARDABNORMALREASON.Where(x => x.STANDARDUNIQUEID == Model.UniqueID).ToList());

                            db.SaveChanges();
                            #endregion

                            #region Insert
                            db.STANDARDABNORMALREASON.AddRange(Model.AbnormalReasonList.Select(x => new STANDARDABNORMALREASON
                            {
                                STANDARDUNIQUEID = standard.UNIQUEID,
                                ABNORMALREASONUNIQUEID = x.UniqueID
                            }).ToList());

                            db.SaveChanges();
                            #endregion
                            #endregion

#if !DEBUG
                        trans.Complete();
                    }
#endif

                            result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.Standard, Resources.Resource.Success));
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.StandardID, Resources.Resource.Exists));
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
                    DeleteHelper.Standard(db, SelectedList);

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.Standard, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult AddAbnormalReason(List<AbnormalReasonModel> AbnormalReasonList, List<string> SelectedList, string RefOrganizationUniqueID)
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
                        var abnormalType = temp[1];
                        var abnormalReasonUniqueID = temp[2];

                        if (!string.IsNullOrEmpty(abnormalReasonUniqueID))
                        {
                            if (!AbnormalReasonList.Any(x => x.UniqueID == abnormalReasonUniqueID))
                            {
                                var abnormalReason = db.ABNORMALREASON.First(x => x.UNIQUEID == abnormalReasonUniqueID);

                                AbnormalReasonList.Add(new AbnormalReasonModel()
                                {
                                    UniqueID = abnormalReason.UNIQUEID,
                                    AbnormalType = abnormalReason.ABNORMALTYPE,
                                    ID = abnormalReason.ID,
                                    Description = abnormalReason.DESCRIPTION
                                });
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(abnormalType))
                            {
                                var abnormalReasonList = db.ABNORMALREASON.Where(x => x.ORGANIZATIONUNIQUEID == organizationUniqueID && x.ABNORMALTYPE == abnormalType).ToList();

                                foreach (var abnormalReason in abnormalReasonList)
                                {
                                    if (!AbnormalReasonList.Any(x => x.UniqueID == abnormalReason.UNIQUEID))
                                    {
                                        AbnormalReasonList.Add(new AbnormalReasonModel()
                                        {
                                            UniqueID = abnormalReason.UNIQUEID,
                                            AbnormalType = abnormalReason.ABNORMALTYPE,
                                            ID = abnormalReason.ID,
                                            Description = abnormalReason.DESCRIPTION
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
                                        var abnormalReasonList = db.ABNORMALREASON.Where(x => x.ORGANIZATIONUNIQUEID == downStreamOrganization).ToList();

                                        foreach (var abnormalReason in abnormalReasonList)
                                        {
                                            if (!AbnormalReasonList.Any(x => x.UniqueID == abnormalReason.UNIQUEID))
                                            {
                                                AbnormalReasonList.Add(new AbnormalReasonModel()
                                                {
                                                    UniqueID = abnormalReason.UNIQUEID,
                                                    AbnormalType = abnormalReason.ABNORMALTYPE,
                                                    ID = abnormalReason.ID,
                                                    Description = abnormalReason.DESCRIPTION
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                result.ReturnData(AbnormalReasonList.OrderBy(x => x.AbnormalType).ThenBy(x => x.ID).ToList());
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetTreeItem(List<Models.Shared.Organization> OrganizationList, string OrganizationUniqueID, string MaintenanceType, Account Account)
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
                    { Define.EnumTreeAttribute.MaintenanceType, string.Empty },
                    { Define.EnumTreeAttribute.StandardUniqueID, string.Empty }
                };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (string.IsNullOrEmpty(MaintenanceType))
                    {
                        if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                        {
                            var maintenanceTypeeList = db.STANDARD.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).Select(x => x.MAINTENANCETYPE).Distinct().OrderBy(x => x).ToList();

                            foreach (var maintenanceType in maintenanceTypeeList)
                            {
                                var treeItem = new TreeItem() { Title = maintenanceType };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.MaintenanceType.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = maintenanceType;
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                                attributes[Define.EnumTreeAttribute.MaintenanceType] = maintenanceType;
                                attributes[Define.EnumTreeAttribute.StandardUniqueID] = string.Empty;

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
                            attributes[Define.EnumTreeAttribute.MaintenanceType] = string.Empty;
                            attributes[Define.EnumTreeAttribute.StandardUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                                ||
                                (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.STANDARD.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID)))
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else
                    {
                        var standardList = db.STANDARD.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID && x.MAINTENANCETYPE == MaintenanceType).OrderBy(x => x.ID).ToList();

                        foreach (var standard in standardList)
                        {
                            var treeItem = new TreeItem() { Title = standard.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Standard.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", standard.ID, standard.DESCRIPTION);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = standard.ORGANIZATIONUNIQUEID;
                            attributes[Define.EnumTreeAttribute.MaintenanceType] = standard.MAINTENANCETYPE;
                            attributes[Define.EnumTreeAttribute.StandardUniqueID] = standard.UNIQUEID;

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
                    { Define.EnumTreeAttribute.MaintenanceType, string.Empty },
                    { Define.EnumTreeAttribute.StandardUniqueID, string.Empty }
                };

                var organization = OrganizationList.First(x => x.UniqueID == OrganizationUniqueID);

                var treeItem = new TreeItem() { Title = organization.Description };

                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                attributes[Define.EnumTreeAttribute.MaintenanceType] = string.Empty;
                attributes[Define.EnumTreeAttribute.StandardUniqueID] = string.Empty;

                foreach (var attribute in attributes)
                {
                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                }

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                        ||
                        (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.STANDARD.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID)))
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

        public static RequestResult GetTreeItem(List<Models.Shared.Organization> OrganizationList, string RefOrganizationUniqueID, string OrganizationUniqueID, string MaintenanceType)
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
                    { Define.EnumTreeAttribute.MaintenanceType, string.Empty },
                    { Define.EnumTreeAttribute.StandardUniqueID, string.Empty }
                };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (string.IsNullOrEmpty(MaintenanceType))
                    {
                        var maintenanceTypeList = db.STANDARD.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).Select(x => x.MAINTENANCETYPE).Distinct().OrderBy(x => x).ToList();

                        foreach (var maintenanceType in maintenanceTypeList)
                        {
                            var treeItem = new TreeItem() { Title = maintenanceType };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.MaintenanceType.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = maintenanceType;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.MaintenanceType] = maintenanceType;
                            attributes[Define.EnumTreeAttribute.StandardUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItem.State = "closed";

                            treeItemList.Add(treeItem);
                        }

                        var avalibleOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(OrganizationList, RefOrganizationUniqueID, true).Union(OrganizationDataAccessor.GetDownStreamOrganizationList(OrganizationList, RefOrganizationUniqueID, false)).Union(OrganizationDataAccessor.GetQueryableOrganizationList(RefOrganizationUniqueID)).Union(OrganizationDataAccessor.GetEditableOrganizationList(RefOrganizationUniqueID)).Distinct().ToList();

                        var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && avalibleOrganizationList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                        foreach (var organization in organizationList)
                        {
                            var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organization.UniqueID, true);

                            if (db.STANDARD.Any(x => downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID) &&  avalibleOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)))
                            {
                                var treeItem = new TreeItem() { Title = organization.Description };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                                attributes[Define.EnumTreeAttribute.MaintenanceType] = string.Empty;
                                attributes[Define.EnumTreeAttribute.StandardUniqueID] = string.Empty;

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
                        var standardList = db.STANDARD.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID && x.MAINTENANCETYPE == MaintenanceType).OrderBy(x => x.ID).ToList();

                        foreach (var standard in standardList)
                        {
                            var treeItem = new TreeItem() { Title = standard.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Standard.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", standard.ID, standard.DESCRIPTION);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = standard.ORGANIZATIONUNIQUEID;
                            attributes[Define.EnumTreeAttribute.MaintenanceType] = standard.MAINTENANCETYPE;
                            attributes[Define.EnumTreeAttribute.StandardUniqueID] = standard.UNIQUEID;

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
