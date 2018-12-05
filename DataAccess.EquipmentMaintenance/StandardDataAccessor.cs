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
using Models.EquipmentMaintenance.StandardManagement;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.EquipmentMaintenance
{
    public class StandardDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.Standard.Where(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.MaintenanceType))
                    {
                        query = query.Where(x => x.OrganizationUniqueID == Parameters.OrganizationUniqueID && x.MaintenanceType == Parameters.MaintenanceType);
                    }

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.ID.Contains(Parameters.Keyword) || x.Description.Contains(Parameters.Keyword));
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
                            UniqueID = x.UniqueID,
                            Permission = Account.OrganizationPermission(x.OrganizationUniqueID),
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.OrganizationUniqueID),
                            MaintenanceType = x.MaintenanceType,
                            ID = x.ID,
                            Description = x.Description,
                            Unit = x.Unit
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
                using (EDbEntities db = new EDbEntities())
                {
                    var standard = db.Standard.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = standard.UniqueID,
                        Permission = Account.OrganizationPermission(standard.OrganizationUniqueID),
                        OrganizationUniqueID = standard.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(standard.OrganizationUniqueID),
                        MaintenanceType = standard.MaintenanceType,
                        ID = standard.ID,
                        Description = standard.Description,
                        IsFeelItem = standard.IsFeelItem,
                        UpperLimit = standard.UpperLimit.HasValue ? standard.UpperLimit.ToString() : "",
                        UpperAlertLimit = standard.UpperAlertLimit.HasValue ? standard.UpperAlertLimit.ToString() : "",
                        LowerAlertLimit = standard.LowerAlertLimit.HasValue ? standard.LowerAlertLimit.ToString() : "",
                        LowerLimit = standard.LowerLimit.HasValue ? standard.LowerLimit.ToString() : "",
                        IsAccumulation = standard.IsAccumulation,
                        AccumulationBase = standard.AccumulationBase.HasValue ? standard.AccumulationBase.ToString() : "",
                        Unit = standard.Unit,
                        Remark = standard.Remark,
                        AbnormalReasonDescriptionList = (from x in db.StandardAbnormalReason
                                                         join a in db.AbnormalReason
                                                         on x.AbnormalReasonUniqueID equals a.UniqueID
                                                         where x.StandardUniqueID == UniqueID
                                                         orderby a.AbnormalType, a.ID
                                                         select a.Description).ToList(),
                        FeelOptionDescriptionList = db.StandardFeelOption.Where(x => x.StandardUniqueID == UniqueID).OrderBy(x => x.Seq).ToList().Select(x => x.Description + (x.IsAbnormal ? "(" + Resources.Resource.Abnormal + ")" : "")).ToList()
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
                using (EDbEntities db = new EDbEntities())
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

                    model.MaintenanceTypeSelectItemList.AddRange(db.Standard.Where(x => upStreamOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.MaintenanceType).Distinct().OrderBy(x => x).Select(x => new SelectListItem
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
                using (EDbEntities db = new EDbEntities())
                {
                    var standard = db.Standard.First(x => x.UniqueID == UniqueID);

                    var model = new CreateFormModel()
                    {
                        OrganizationUniqueID = standard.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(standard.OrganizationUniqueID),
                        MaintenanceTypeSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne),
                            new SelectListItem()
                            {
                                Text = Resources.Resource.Create + "...",
                                Value = Define.NEW
                            }
                        },
                        AbnormalReasonList = (from x in db.StandardAbnormalReason
                                              join a in db.AbnormalReason
                                              on x.AbnormalReasonUniqueID equals a.UniqueID
                                              where x.StandardUniqueID == UniqueID
                                              select new AbnormalReasonModel
                                              {
                                                  UniqueID = a.UniqueID,
                                                  AbnormalType = a.AbnormalType,
                                                  ID = a.ID,
                                                  Description = a.Description
                                              }).OrderBy(x => x.AbnormalType).ThenBy(x => x.ID).ToList(),
                        FormInput = new FormInput()
                        {
                            MaintenanceType = standard.MaintenanceType,
                            IsFeelItem = standard.IsFeelItem,
                            LowerLimit = standard.LowerLimit.HasValue ? standard.LowerLimit.ToString() : "",
                            LowerAlertLimit = standard.LowerAlertLimit.HasValue ? standard.LowerAlertLimit.ToString() : "",
                            UpperAlertLimit = standard.UpperAlertLimit.HasValue ? standard.UpperAlertLimit.ToString() : "",
                            UpperLimit = standard.UpperLimit.HasValue ? standard.UpperLimit.ToString() : "",
                            IsAccumulation = standard.IsAccumulation,
                            AccumulationBase = standard.AccumulationBase.HasValue ? standard.AccumulationBase.ToString() : "",
                            Unit = standard.Unit,
                            Remark = standard.Remark
                        }
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(standard.OrganizationUniqueID, true);

                    model.MaintenanceTypeSelectItemList.AddRange(db.Standard.Where(x => upStreamOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.MaintenanceType).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    model.MaintenanceTypeSelectItemList.First(x => x.Value == standard.MaintenanceType).Selected = true;

                    if (standard.IsFeelItem)
                    {
                        model.FeelOptionList = (from x in db.StandardFeelOption
                                                where x.StandardUniqueID == UniqueID
                                                select new FeelOptionModel
                                                {
                                                    UniqueID = x.UniqueID,
                                                    Description = x.Description,
                                                    IsAbnormal = x.IsAbnormal,
                                                    Seq = x.Seq
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
                    using (EDbEntities db = new EDbEntities())
                    {
                        var exists = db.Standard.FirstOrDefault(x => x.OrganizationUniqueID == Model.OrganizationUniqueID && x.MaintenanceType == Model.FormInput.MaintenanceType && x.ID == Model.FormInput.ID);

                        if (exists == null)
                        {
                            string uniqueID = Guid.NewGuid().ToString();

                            db.Standard.Add(new Standard()
                            {
                                UniqueID = uniqueID,
                                OrganizationUniqueID = Model.OrganizationUniqueID,
                                MaintenanceType = Model.FormInput.MaintenanceType,
                                ID = Model.FormInput.ID,
                                Description = Model.FormInput.Description,
                                IsFeelItem = Model.FormInput.IsFeelItem ,
                                UpperLimit = !string.IsNullOrEmpty(Model.FormInput.UpperLimit) ? double.Parse(Model.FormInput.UpperLimit) : default(double?),
                                UpperAlertLimit = !string.IsNullOrEmpty(Model.FormInput.UpperAlertLimit) ? double.Parse(Model.FormInput.UpperAlertLimit) : default(double?),
                                LowerAlertLimit = !string.IsNullOrEmpty(Model.FormInput.LowerAlertLimit) ? double.Parse(Model.FormInput.LowerAlertLimit) : default(double?),
                                LowerLimit = !string.IsNullOrEmpty(Model.FormInput.LowerLimit) ? double.Parse(Model.FormInput.LowerLimit) : default(double?),
                                IsAccumulation = Model.FormInput.IsAccumulation ,
                                AccumulationBase = !string.IsNullOrEmpty(Model.FormInput.AccumulationBase) ? double.Parse(Model.FormInput.AccumulationBase) : default(double?),
                                Unit = Model.FormInput.Unit,
                                Remark = Model.FormInput.Remark,
                                LastModifyTime = DateTime.Now
                            });

                            if (Model.FormInput.IsFeelItem)
                            {
                                db.StandardFeelOption.AddRange(Model.FormInput.FeelOptionList.Select(x => new StandardFeelOption
                                {
                                    UniqueID = Guid.NewGuid().ToString(),
                                    StandardUniqueID = uniqueID,
                                    Description = x.Description,
                                    IsAbnormal = x.IsAbnormal,
                                    Seq = x.Seq
                                }).ToList());
                            }

                            db.StandardAbnormalReason.AddRange(Model.AbnormalReasonList.Select(x => new StandardAbnormalReason
                            {
                                StandardUniqueID = uniqueID,
                                AbnormalReasonUniqueID = x.UniqueID
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
                using (EDbEntities db = new EDbEntities())
                {
                    var standard = db.Standard.First(x => x.UniqueID == UniqueID);

                    var model = new EditFormModel()
                    {
                        UniqueID = standard.UniqueID,
                        OrganizationUniqueID = standard.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(standard.OrganizationUniqueID),
                        MaintenanceTypeSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne),
                            new SelectListItem()
                            {
                                Text = Resources.Resource.Create + "...",
                                Value = Define.NEW
                            }
                        },
                        AbnormalReasonList = (from x in db.StandardAbnormalReason
                                              join a in db.AbnormalReason
                                              on x.AbnormalReasonUniqueID equals a.UniqueID
                                              where x.StandardUniqueID == UniqueID
                                              select new AbnormalReasonModel
                                              {
                                                  UniqueID = a.UniqueID,
                                                  AbnormalType = a.AbnormalType,
                                                  ID = a.ID,
                                                  Description = a.Description
                                              }).OrderBy(x => x.AbnormalType).ThenBy(x => x.ID).ToList(),
                        FormInput = new FormInput()
                        {
                            MaintenanceType = standard.MaintenanceType,
                            ID = standard.ID,
                            Description = standard.Description,
                            IsFeelItem = standard.IsFeelItem,
                            LowerLimit = standard.LowerLimit.HasValue ? standard.LowerLimit.ToString() : "",
                            LowerAlertLimit = standard.LowerAlertLimit.HasValue ? standard.LowerAlertLimit.ToString() : "",
                            UpperAlertLimit = standard.UpperAlertLimit.HasValue ? standard.UpperAlertLimit.ToString() : "",
                            UpperLimit = standard.UpperLimit.HasValue ? standard.UpperLimit.ToString() : "",
                            IsAccumulation = standard.IsAccumulation,
                            AccumulationBase = standard.AccumulationBase.HasValue ? standard.AccumulationBase.ToString() : "",
                            Unit = standard.Unit,
                            Remark = standard.Remark
                        }
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(model.OrganizationUniqueID, true);

                    model.MaintenanceTypeSelectItemList.AddRange(db.Standard.Where(x => upStreamOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.MaintenanceType).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    if (!string.IsNullOrEmpty(model.FormInput.MaintenanceType) && model.MaintenanceTypeSelectItemList.Any(x => x.Value == model.FormInput.MaintenanceType))
                    {
                        model.MaintenanceTypeSelectItemList.First(x => x.Value == model.FormInput.MaintenanceType).Selected = true;
                    }

                    if (standard.IsFeelItem)
                    {
                        model.FeelOptionList = (from x in db.StandardFeelOption
                                                where x.StandardUniqueID == UniqueID
                                                select new FeelOptionModel
                                                {
                                                    UniqueID = x.UniqueID,
                                                    Description = x.Description,
                                                    IsAbnormal = x.IsAbnormal,
                                                    Seq = x.Seq
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
                    using (EDbEntities db = new EDbEntities())
                    {
                        var standard = db.Standard.First(x => x.UniqueID == Model.UniqueID);

                        var exists = db.Standard.FirstOrDefault(x => x.UniqueID != standard.UniqueID && x.OrganizationUniqueID == standard.OrganizationUniqueID && x.MaintenanceType == Model.FormInput.MaintenanceType && x.ID == Model.FormInput.ID);

                        if (exists == null)
                        {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                            #region Standard
                            standard.MaintenanceType = Model.FormInput.MaintenanceType;
                            standard.ID = Model.FormInput.ID;
                            standard.Description = Model.FormInput.Description;
                            standard.IsFeelItem = Model.FormInput.IsFeelItem;
                            standard.UpperLimit = !string.IsNullOrEmpty(Model.FormInput.UpperLimit) ? double.Parse(Model.FormInput.UpperLimit) : default(double?);
                            standard.UpperAlertLimit = !string.IsNullOrEmpty(Model.FormInput.UpperAlertLimit) ? double.Parse(Model.FormInput.UpperAlertLimit) : default(double?);
                            standard.LowerAlertLimit = !string.IsNullOrEmpty(Model.FormInput.LowerAlertLimit) ? double.Parse(Model.FormInput.LowerAlertLimit) : default(double?);
                            standard.LowerLimit = !string.IsNullOrEmpty(Model.FormInput.LowerLimit) ? double.Parse(Model.FormInput.LowerLimit) : default(double?);
                            standard.IsAccumulation = Model.FormInput.IsAccumulation;
                            standard.AccumulationBase = !string.IsNullOrEmpty(Model.FormInput.AccumulationBase) ? double.Parse(Model.FormInput.AccumulationBase) : default(double?);
                            standard.Remark = Model.FormInput.Remark;
                            standard.Unit = Model.FormInput.Unit;
                            standard.LastModifyTime = DateTime.Now;

                            db.SaveChanges();
                            #endregion

                            #region StandardFeelOption
                            #region Delete
                            db.StandardFeelOption.RemoveRange(db.StandardFeelOption.Where(x => x.StandardUniqueID == Model.UniqueID).ToList());

                            db.SaveChanges();
                            #endregion

                            #region Insert
                            if (Model.FormInput.IsFeelItem)
                            {
                                db.StandardFeelOption.AddRange(Model.FormInput.FeelOptionList.Select(x => new StandardFeelOption
                                {
                                    UniqueID = !string.IsNullOrEmpty(x.UniqueID) ? x.UniqueID : Guid.NewGuid().ToString(),
                                    StandardUniqueID = standard.UniqueID,
                                    Description = x.Description,
                                    IsAbnormal = x.IsAbnormal,
                                    Seq = x.Seq
                                }).ToList());
                            }

                            db.SaveChanges();
                            #endregion
                            #endregion

                            #region StandardAbnormalReason
                            #region Delete
                            db.StandardAbnormalReason.RemoveRange(db.StandardAbnormalReason.Where(x => x.StandardUniqueID == Model.UniqueID).ToList());

                            db.SaveChanges();
                            #endregion

                            #region Insert
                            db.StandardAbnormalReason.AddRange(Model.AbnormalReasonList.Select(x => new StandardAbnormalReason
                            {
                                StandardUniqueID = standard.UniqueID,
                                AbnormalReasonUniqueID = x.UniqueID
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
                using (EDbEntities db = new EDbEntities())
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
                using (EDbEntities db = new EDbEntities())
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
                                var abnormalReason = db.AbnormalReason.First(x => x.UniqueID == abnormalReasonUniqueID);

                                AbnormalReasonList.Add(new AbnormalReasonModel()
                                {
                                    UniqueID = abnormalReason.UniqueID,
                                    AbnormalType = abnormalReason.AbnormalType,
                                    ID = abnormalReason.ID,
                                    Description = abnormalReason.Description
                                });
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(abnormalType))
                            {
                                var abnormalReasonList = db.AbnormalReason.Where(x => x.OrganizationUniqueID == organizationUniqueID && x.AbnormalType == abnormalType).ToList();

                                foreach (var abnormalReason in abnormalReasonList)
                                {
                                    if (!AbnormalReasonList.Any(x => x.UniqueID == abnormalReason.UniqueID))
                                    {
                                        AbnormalReasonList.Add(new AbnormalReasonModel()
                                        {
                                            UniqueID = abnormalReason.UniqueID,
                                            AbnormalType = abnormalReason.AbnormalType,
                                            ID = abnormalReason.ID,
                                            Description = abnormalReason.Description
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
                                        var abnormalReasonList = db.AbnormalReason.Where(x => x.OrganizationUniqueID == downStreamOrganization).ToList();

                                        foreach (var abnormalReason in abnormalReasonList)
                                        {
                                            if (!AbnormalReasonList.Any(x => x.UniqueID == abnormalReason.UniqueID))
                                            {
                                                AbnormalReasonList.Add(new AbnormalReasonModel()
                                                {
                                                    UniqueID = abnormalReason.UniqueID,
                                                    AbnormalType = abnormalReason.AbnormalType,
                                                    ID = abnormalReason.ID,
                                                    Description = abnormalReason.Description
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

                using (EDbEntities db = new EDbEntities())
                {
                    if (string.IsNullOrEmpty(MaintenanceType))
                    {
                        if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                        {
                            var maintenanceTypeeList = db.Standard.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).Select(x => x.MaintenanceType).Distinct().OrderBy(x => x).ToList();

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
                                (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.Standard.Any(x => x.OrganizationUniqueID == organization.UniqueID)))
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else
                    {
                        var standardList = db.Standard.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && x.MaintenanceType == MaintenanceType).OrderBy(x => x.ID).ToList();

                        foreach (var standard in standardList)
                        {
                            var treeItem = new TreeItem() { Title = standard.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Standard.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", standard.ID, standard.Description);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = standard.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.MaintenanceType] = standard.MaintenanceType;
                            attributes[Define.EnumTreeAttribute.StandardUniqueID] = standard.UniqueID;

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

                using (EDbEntities db = new EDbEntities())
                {
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

                    if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                        ||
                        (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.Standard.Any(x => x.OrganizationUniqueID == organization.UniqueID)))
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

                using (EDbEntities db = new EDbEntities())
                {
                    if (string.IsNullOrEmpty(MaintenanceType))
                    {
                        var maintenanceTypeList = db.Standard.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).Select(x => x.MaintenanceType).Distinct().OrderBy(x => x).ToList();

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

                        var availableOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(RefOrganizationUniqueID, true);

                        var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && availableOrganizationList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                        foreach (var organization in organizationList)
                        {
                            var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organization.UniqueID, true);

                            if (db.Standard.Any(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && availableOrganizationList.Contains(x.OrganizationUniqueID)))
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
                        var standardList = db.Standard.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && x.MaintenanceType == MaintenanceType).OrderBy(x => x.ID).ToList();

                        foreach (var standard in standardList)
                        {
                            var treeItem = new TreeItem() { Title = standard.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Standard.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", standard.ID, standard.Description);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = standard.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.MaintenanceType] = standard.MaintenanceType;
                            attributes[Define.EnumTreeAttribute.StandardUniqueID] = standard.UniqueID;

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
