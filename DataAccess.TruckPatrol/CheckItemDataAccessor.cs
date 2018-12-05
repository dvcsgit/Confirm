using System;
using System.Linq;
using System.Web.Mvc;
using System.Reflection;
#if !DEBUG
using System.Transactions;
#endif
using System.Collections.Generic;
using Utility;
using Utility.Models;
#if ORACLE
using DbEntity.ORACLE;
using DbEntity.ORACLE.EquipmentMaintenance;
#else
using DbEntity.MSSQL;
using DbEntity.MSSQL.TruckPatrol;
#endif
using Models.Shared;
using Models.Authenticated;
using Models.TruckPatrol.CheckItemManagement;

namespace DataAccess.TruckPatrol
{
    public class CheckItemDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (TDbEntities db = new TDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.CheckItem.Where(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.CheckType))
                    {
                        query = query.Where(x => x.OrganizationUniqueID == Parameters.OrganizationUniqueID && x.CheckType == Parameters.CheckType);
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
                        CheckType = Parameters.CheckType,
                        OrganizationDescription = organization.Description,
                        FullOrganizationDescription = organization.FullDescription,
                        ItemList = query.ToList().Select(x => new GridItem()
                        {
                            UniqueID = x.UniqueID,
                            Permission = Account.OrganizationPermission(x.OrganizationUniqueID),
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.OrganizationUniqueID),
                            CheckType = x.CheckType,
                            ID = x.ID,
                            Description = x.Description,
                            Unit = x.Unit
                        }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.CheckType).ThenBy(x => x.ID).ToList()
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
                using (TDbEntities db = new TDbEntities())
                {
                    var checkItem = db.CheckItem.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = checkItem.UniqueID,
                        Permission = Account.OrganizationPermission(checkItem.OrganizationUniqueID),
                        OrganizationUniqueID = checkItem.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(checkItem.OrganizationUniqueID),
                        CheckType = checkItem.CheckType,
                        ID = checkItem.ID,
                        Description = checkItem.Description,
                        IsFeelItem = checkItem.IsFeelItem,
                        UpperLimit = checkItem.UpperLimit.HasValue ? checkItem.UpperLimit.ToString() : "",
                        UpperAlertLimit = checkItem.UpperAlertLimit.HasValue ? checkItem.UpperAlertLimit.ToString() : "",
                        LowerAlertLimit = checkItem.LowerAlertLimit.HasValue ? checkItem.LowerAlertLimit.ToString() : "",
                        LowerLimit = checkItem.LowerLimit.HasValue ? checkItem.LowerLimit.ToString() : "",
                        IsAccumulation = checkItem.IsAccumulation,
                        AccumulationBase = checkItem.AccumulationBase.HasValue ? checkItem.AccumulationBase.ToString() : "",
                        Unit = checkItem.Unit,
                        Remark = checkItem.Remark,
                        AbnormalReasonDescriptionList = (from x in db.CheckItemAbnormalReason
                                                         join a in db.AbnormalReason
                                                         on x.AbnormalReasonUniqueID equals a.UniqueID
                                                         where x.CheckItemUniqueID == UniqueID
                                                         orderby a.AbnormalType, a.ID
                                                         select a.Description).ToList(),
                        FeelOptionDescriptionList = (from x in db.CheckItemFeelOption
                                                     where x.CheckItemUniqueID == UniqueID
                                                     orderby x.Seq
                                                     select x.Description + (x.IsAbnormal ? "(" + Resources.Resource.Abnormal + ")" : "")).ToList()
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

        public static RequestResult GetCreateFormModel(string OrganizationUniqueID, string CheckType)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (TDbEntities db = new TDbEntities())
                {
                    var model = new CreateFormModel()
                    {
                        OrganizationUniqueID = OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(OrganizationUniqueID),
                        CheckTypeSelectItemList = new List<SelectListItem>() 
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
                            CheckType = CheckType
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

                    model.CheckTypeSelectItemList.AddRange(db.CheckItem.Where(x => upStreamOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.CheckType).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    if (!string.IsNullOrEmpty(CheckType) && model.CheckTypeSelectItemList.Any(x => x.Value == CheckType))
                    {
                        model.CheckTypeSelectItemList.First(x => x.Value == CheckType).Selected = true;
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
                using (TDbEntities db = new TDbEntities())
                {
                    var checkItem = db.CheckItem.First(x => x.UniqueID == UniqueID);

                    var model = new CreateFormModel()
                    {
                        OrganizationUniqueID = checkItem.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(checkItem.OrganizationUniqueID),
                        CheckTypeSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne),
                            new SelectListItem()
                            {
                                Text = Resources.Resource.Create + "...",
                                Value = Define.NEW
                            }
                        },
                        AbnormalReasonList = (from x in db.CheckItemAbnormalReason
                                              join a in db.AbnormalReason
                                              on x.AbnormalReasonUniqueID equals a.UniqueID
                                              where x.CheckItemUniqueID == UniqueID
                                              select new AbnormalReasonModel
                                              {
                                                  UniqueID = a.UniqueID,
                                                  AbnormalType = a.AbnormalType,
                                                  ID = a.ID,
                                                  Description = a.Description
                                              }).OrderBy(x => x.AbnormalType).ThenBy(x => x.ID).ToList(),
                        FormInput = new FormInput()
                        {
                            CheckType = checkItem.CheckType,
                            IsFeelItem = checkItem.IsFeelItem,
                            LowerLimit = checkItem.LowerLimit.HasValue ? checkItem.LowerLimit.ToString() : "",
                            LowerAlertLimit = checkItem.LowerAlertLimit.HasValue ? checkItem.LowerAlertLimit.ToString() : "",
                            UpperAlertLimit = checkItem.UpperAlertLimit.HasValue ? checkItem.UpperAlertLimit.ToString() : "",
                            UpperLimit = checkItem.UpperLimit.HasValue ? checkItem.UpperLimit.ToString() : "",
                            IsAccumulation = checkItem.IsAccumulation,
                            AccumulationBase = checkItem.AccumulationBase.HasValue ? checkItem.AccumulationBase.ToString() : "",
                            Unit = checkItem.Unit,
                            Remark = checkItem.Remark
                        }
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(checkItem.OrganizationUniqueID, true);

                    model.CheckTypeSelectItemList.AddRange(db.CheckItem.Where(x => upStreamOrganizationList.Contains(x.OrganizationUniqueID)).Select(x => x.CheckType).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    model.CheckTypeSelectItemList.First(x => x.Value == checkItem.CheckType).Selected = true;

#if ORACLE
                    if (checkItem.IsFeelItem==1)
#else
                    if (checkItem.IsFeelItem)
#endif
                    {
                        model.FeelOptionList = (from x in db.CheckItemFeelOption
                                                where x.CheckItemUniqueID == UniqueID
                                                select new FeelOptionModel
                                                {
                                                    UniqueID = x.UniqueID,
                                                    Description = x.Description,
#if ORACLE
                                                    IsAbnormal = x.IsAbnormal==1,
#else
                                                    IsAbnormal = x.IsAbnormal,
#endif
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
                if (Model.FormInput.CheckType == Define.OTHER || Model.FormInput.CheckType == Define.NEW)
                {
                    result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.Unsupported, Resources.Resource.CheckType));
                }
                else
                {
                    using (TDbEntities db = new TDbEntities())
                    {
                        var exists = db.CheckItem.FirstOrDefault(x => x.OrganizationUniqueID == Model.OrganizationUniqueID && x.CheckType == Model.FormInput.CheckType && x.ID == Model.FormInput.ID);

                        if (exists == null)
                        {
                            string uniqueID = Guid.NewGuid().ToString();

                            db.CheckItem.Add(new CheckItem()
                            {
                                UniqueID = uniqueID,
                                OrganizationUniqueID = Model.OrganizationUniqueID,
                                CheckType = Model.FormInput.CheckType,
                                ID = Model.FormInput.ID,
                                Description = Model.FormInput.Description,
#if ORACLE
                                IsFeelItem = Model.FormInput.IsFeelItem?1:0,
                                UpperLimit = !string.IsNullOrEmpty(Model.FormInput.UpperLimit) ? decimal.Parse(Model.FormInput.UpperLimit) : default(decimal?),
                                UpperAlertLimit = !string.IsNullOrEmpty(Model.FormInput.UpperAlertLimit) ? decimal.Parse(Model.FormInput.UpperAlertLimit) : default(decimal?),
                                LowerAlertLimit = !string.IsNullOrEmpty(Model.FormInput.LowerAlertLimit) ? decimal.Parse(Model.FormInput.LowerAlertLimit) : default(decimal?),
                                LowerLimit = !string.IsNullOrEmpty(Model.FormInput.LowerLimit) ? decimal.Parse(Model.FormInput.LowerLimit) : default(decimal?),
#else
                                IsFeelItem = Model.FormInput.IsFeelItem,
                                UpperLimit = !string.IsNullOrEmpty(Model.FormInput.UpperLimit) ? double.Parse(Model.FormInput.UpperLimit) : default(double?),
                                UpperAlertLimit = !string.IsNullOrEmpty(Model.FormInput.UpperAlertLimit) ? double.Parse(Model.FormInput.UpperAlertLimit) : default(double?),
                                LowerAlertLimit = !string.IsNullOrEmpty(Model.FormInput.LowerAlertLimit) ? double.Parse(Model.FormInput.LowerAlertLimit) : default(double?),
                                LowerLimit = !string.IsNullOrEmpty(Model.FormInput.LowerLimit) ? double.Parse(Model.FormInput.LowerLimit) : default(double?),
                                IsAccumulation = Model.FormInput.IsAccumulation,
                                AccumulationBase = !string.IsNullOrEmpty(Model.FormInput.AccumulationBase) ? double.Parse(Model.FormInput.AccumulationBase) : default(double?),
#endif
                                Unit = Model.FormInput.Unit,
                                Remark = Model.FormInput.Remark,
                                LastModifyTime = DateTime.Now
                            });

                            if (Model.FormInput.IsFeelItem)
                            {
                                db.CheckItemFeelOption.AddRange(Model.FormInput.FeelOptionList.Select(x => new CheckItemFeelOption
                                {
                                    UniqueID = Guid.NewGuid().ToString(),
                                    CheckItemUniqueID = uniqueID,
                                    Description = x.Description,
#if ORACLE
                                    IsAbnormal = x.IsAbnormal?1:0,
#else
                                    IsAbnormal = x.IsAbnormal,
#endif
                                    Seq = x.Seq
                                }).ToList());
                            }

                            db.CheckItemAbnormalReason.AddRange(Model.AbnormalReasonList.Select(x => new CheckItemAbnormalReason
                            {
                                CheckItemUniqueID = uniqueID,
                                AbnormalReasonUniqueID = x.UniqueID
                            }).ToList());

                            db.SaveChanges();

                            result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.CheckItem, Resources.Resource.Success));
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.CheckItemID, Resources.Resource.Exists));
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
                using (TDbEntities db = new TDbEntities())
                {
                    var checkItem = db.CheckItem.First(x => x.UniqueID == UniqueID);

                    var model = new EditFormModel()
                    {
                        UniqueID = checkItem.UniqueID,
                        OrganizationUniqueID = checkItem.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(checkItem.OrganizationUniqueID),
                        AbnormalReasonList = (from x in db.CheckItemAbnormalReason
                                              join a in db.AbnormalReason
                                              on x.AbnormalReasonUniqueID equals a.UniqueID
                                              where x.CheckItemUniqueID == UniqueID
                                              select new AbnormalReasonModel
                                              {
                                                  UniqueID = a.UniqueID,
                                                  AbnormalType = a.AbnormalType,
                                                  ID = a.ID,
                                                  Description = a.Description
                                              }).OrderBy(x => x.AbnormalType).ThenBy(x => x.ID).ToList(),
                        FormInput = new FormInput()
                        {
                            CheckType = checkItem.CheckType,
                            ID = checkItem.ID,
                            Description = checkItem.Description,
                            IsFeelItem = checkItem.IsFeelItem,
                            LowerLimit = checkItem.LowerLimit.HasValue ? checkItem.LowerLimit.ToString() : "",
                            LowerAlertLimit = checkItem.LowerAlertLimit.HasValue ? checkItem.LowerAlertLimit.ToString() : "",
                            UpperAlertLimit = checkItem.UpperAlertLimit.HasValue ? checkItem.UpperAlertLimit.ToString() : "",
                            UpperLimit = checkItem.UpperLimit.HasValue ? checkItem.UpperLimit.ToString() : "",
                            IsAccumulation = checkItem.IsAccumulation,
                            AccumulationBase = checkItem.AccumulationBase.HasValue ? checkItem.AccumulationBase.ToString() : "",
                            Unit = checkItem.Unit,
                            Remark = checkItem.Remark
                        }
                    };

#if ORACLE
                    if (checkItem.IsFeelItem==1)
#else
                        if (checkItem.IsFeelItem)
#endif
                    {
                        model.FeelOptionList = (from x in db.CheckItemFeelOption
                                                where x.CheckItemUniqueID == UniqueID
                                                select new FeelOptionModel
                                                {
                                                    UniqueID = x.UniqueID,
                                                    Description = x.Description,
#if ORACLE
                                                    IsAbnormal = x.IsAbnormal==1,
#else
                                                    IsAbnormal = x.IsAbnormal,
#endif
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
                using (TDbEntities db = new TDbEntities())
                {
                    var checkItem = db.CheckItem.First(x => x.UniqueID == Model.UniqueID);

                    var exists = db.CheckItem.FirstOrDefault(x => x.UniqueID != checkItem.UniqueID && x.OrganizationUniqueID == checkItem.OrganizationUniqueID && x.CheckType == checkItem.CheckType && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                        #region CheckItem
                        checkItem.ID = Model.FormInput.ID;
                        checkItem.Description = Model.FormInput.Description;
#if ORACLE
                        checkItem.IsFeelItem = Model.FormInput.IsFeelItem?1:0;
                        checkItem.UpperLimit = !string.IsNullOrEmpty(Model.FormInput.UpperLimit) ? decimal.Parse(Model.FormInput.UpperLimit) : default(decimal?);
                        checkItem.UpperAlertLimit = !string.IsNullOrEmpty(Model.FormInput.UpperAlertLimit) ? decimal.Parse(Model.FormInput.UpperAlertLimit) : default(decimal?);
                        checkItem.LowerAlertLimit = !string.IsNullOrEmpty(Model.FormInput.LowerAlertLimit) ? decimal.Parse(Model.FormInput.LowerAlertLimit) : default(decimal?);
                        checkItem.LowerLimit = !string.IsNullOrEmpty(Model.FormInput.LowerLimit) ? decimal.Parse(Model.FormInput.LowerLimit) : default(decimal?);
#else
                        checkItem.IsFeelItem = Model.FormInput.IsFeelItem;
                        checkItem.UpperLimit = !string.IsNullOrEmpty(Model.FormInput.UpperLimit) ? double.Parse(Model.FormInput.UpperLimit) : default(double?);
                        checkItem.UpperAlertLimit = !string.IsNullOrEmpty(Model.FormInput.UpperAlertLimit) ? double.Parse(Model.FormInput.UpperAlertLimit) : default(double?);
                        checkItem.LowerAlertLimit = !string.IsNullOrEmpty(Model.FormInput.LowerAlertLimit) ? double.Parse(Model.FormInput.LowerAlertLimit) : default(double?);
                        checkItem.LowerLimit = !string.IsNullOrEmpty(Model.FormInput.LowerLimit) ? double.Parse(Model.FormInput.LowerLimit) : default(double?);
                        checkItem.IsAccumulation = Model.FormInput.IsAccumulation;
                        checkItem.AccumulationBase = !string.IsNullOrEmpty(Model.FormInput.AccumulationBase) ? double.Parse(Model.FormInput.AccumulationBase) : default(double?);
#endif
                        checkItem.Remark = Model.FormInput.Remark;
                        checkItem.Unit = Model.FormInput.Unit;
                        checkItem.LastModifyTime = DateTime.Now;

                        db.SaveChanges();
                        #endregion

                        #region CheckItemFeelOption
                        #region Delete
                        db.CheckItemFeelOption.RemoveRange(db.CheckItemFeelOption.Where(x => x.CheckItemUniqueID == Model.UniqueID).ToList());

                        db.SaveChanges();
                        #endregion

                        #region Insert
                        if (Model.FormInput.IsFeelItem)
                        {
                            db.CheckItemFeelOption.AddRange(Model.FormInput.FeelOptionList.Select(x => new CheckItemFeelOption
                            {
                                UniqueID = !string.IsNullOrEmpty(x.UniqueID) ? x.UniqueID : Guid.NewGuid().ToString(),
                                CheckItemUniqueID = checkItem.UniqueID,
                                Description = x.Description,
#if ORACLE
                                IsAbnormal = x.IsAbnormal?1:0,
#else
                                IsAbnormal = x.IsAbnormal,
#endif
                                Seq = x.Seq
                            }).ToList());
                        }

                        db.SaveChanges();
                        #endregion
                        #endregion

                        #region CheckItemAbnormalReason
                        #region Delete
                        db.CheckItemAbnormalReason.RemoveRange(db.CheckItemAbnormalReason.Where(x => x.CheckItemUniqueID == Model.UniqueID).ToList());

                        db.SaveChanges();
                        #endregion

                        #region Insert
                        db.CheckItemAbnormalReason.AddRange(Model.AbnormalReasonList.Select(x => new CheckItemAbnormalReason
                        {
                            CheckItemUniqueID = checkItem.UniqueID,
                            AbnormalReasonUniqueID = x.UniqueID
                        }).ToList());

                        db.SaveChanges();
                        #endregion
                        #endregion

#if !DEBUG
                        trans.Complete();
                    }
#endif

                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.CheckItem, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.CheckItemID, Resources.Resource.Exists));
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
                using (TDbEntities db = new TDbEntities())
                {
                    DeleteHelper.CheckItem(db, SelectedList);

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.CheckItem, Resources.Resource.Success));
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
                using (TDbEntities db = new TDbEntities())
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

        public static RequestResult GetTreeItem(string OrganizationUniqueID, string CheckType, Account Account)
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
                    { Define.EnumTreeAttribute.CheckType, string.Empty },
                    { Define.EnumTreeAttribute.CheckItemUniqueID, string.Empty }
                };

                using (TDbEntities tdb = new TDbEntities())
                {
                    if (string.IsNullOrEmpty(CheckType))
                    {
                        if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                        {
                            var checkTypeList = tdb.CheckItem.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).Select(x => x.CheckType).Distinct().OrderBy(x => x).ToList();

                            foreach (var checkType in checkTypeList)
                            {
                                var treeItem = new TreeItem() { Title = checkType };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.CheckType.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = checkType;
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                                attributes[Define.EnumTreeAttribute.CheckType] = checkType;
                                attributes[Define.EnumTreeAttribute.CheckItemUniqueID] = string.Empty;

                                foreach (var attribute in attributes)
                                {
                                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                }

                                treeItem.State = "closed";

                                treeItemList.Add(treeItem);
                            }
                        }

                        using (DbEntities db = new DbEntities())
                        {
                            var organizationList = db.Organization.Where(x => x.ParentUniqueID == OrganizationUniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                            foreach (var organization in organizationList)
                            {
                                var treeItem = new TreeItem() { Title = organization.Description };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                                attributes[Define.EnumTreeAttribute.CheckType] = string.Empty;
                                attributes[Define.EnumTreeAttribute.CheckItemUniqueID] = string.Empty;

                                foreach (var attribute in attributes)
                                {
                                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                }

                                if (db.Organization.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                                    ||
                                    (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && tdb.CheckItem.Any(x => x.OrganizationUniqueID == organization.UniqueID)))
                                {
                                    treeItem.State = "closed";
                                }

                                treeItemList.Add(treeItem);
                            }
                        }
                    }
                    else
                    {
                        var checkItemList = tdb.CheckItem.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && x.CheckType == CheckType).OrderBy(x => x.ID).ToList();

                        foreach (var checkItem in checkItemList)
                        {
                            var treeItem = new TreeItem() { Title = checkItem.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.CheckItem.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", checkItem.ID, checkItem.Description);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = checkItem.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.CheckType] = checkItem.CheckType;
                            attributes[Define.EnumTreeAttribute.CheckItemUniqueID] = checkItem.UniqueID;

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

        public static RequestResult GetTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID, string CheckType)
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
                    { Define.EnumTreeAttribute.CheckType, string.Empty },
                    { Define.EnumTreeAttribute.CheckItemUniqueID, string.Empty }
                };

                using (TDbEntities tdb = new TDbEntities())
                {
                    if (string.IsNullOrEmpty(CheckType))
                    {
                        var checkTypeList = tdb.CheckItem.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).Select(x => x.CheckType).Distinct().OrderBy(x => x).ToList();

                        foreach (var checkType in checkTypeList)
                        {
                            var treeItem = new TreeItem() { Title = checkType };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.CheckType.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = checkType;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.CheckType] = checkType;
                            attributes[Define.EnumTreeAttribute.CheckItemUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItem.State = "closed";

                            treeItemList.Add(treeItem);
                        }

                        var availableOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(RefOrganizationUniqueID, true);

                        using (DbEntities db = new DbEntities())
                        {
                            var organizationList = db.Organization.Where(x => x.ParentUniqueID == OrganizationUniqueID && availableOrganizationList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                            foreach (var organization in organizationList)
                            {
                                var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organization.UniqueID, true);

                                if (tdb.CheckItem.Any(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && availableOrganizationList.Contains(x.OrganizationUniqueID)))
                                {
                                    var treeItem = new TreeItem() { Title = organization.Description };

                                    attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                                    attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                                    attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                                    attributes[Define.EnumTreeAttribute.CheckType] = string.Empty;
                                    attributes[Define.EnumTreeAttribute.CheckItemUniqueID] = string.Empty;

                                    foreach (var attribute in attributes)
                                    {
                                        treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                    }

                                    treeItem.State = "closed";

                                    treeItemList.Add(treeItem);
                                }
                            }
                        }
                    }
                    else
                    {
                        var checkItemList = tdb.CheckItem.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && x.CheckType == CheckType).OrderBy(x => x.ID).ToList();

                        foreach (var checkItem in checkItemList)
                        {
                            var treeItem = new TreeItem() { Title = checkItem.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.CheckItem.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", checkItem.ID, checkItem.Description);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = checkItem.OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.CheckType] = checkItem.CheckType;
                            attributes[Define.EnumTreeAttribute.CheckItemUniqueID] = checkItem.UniqueID;

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
