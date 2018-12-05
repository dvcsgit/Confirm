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
using Models.EquipmentMaintenance.CheckItemManagement;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.ASE
{
    public class CheckItemDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.CHECKITEM.Where(x => downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID) && Account.QueryableOrganizationUniqueIDList.Contains(x.ORGANIZATIONUNIQUEID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.CheckType))
                    {
                        query = query.Where(x => x.ORGANIZATIONUNIQUEID == Parameters.OrganizationUniqueID && x.CHECKTYPE == Parameters.CheckType);
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
                        CheckType = Parameters.CheckType,
                        OrganizationDescription = organization.Description,
                        FullOrganizationDescription = organization.FullDescription,
                        ItemList = query.ToList().Select(x => new GridItem()
                        {
                            UniqueID = x.UNIQUEID,
                            Permission = Account.OrganizationPermission(x.ORGANIZATIONUNIQUEID),
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.ORGANIZATIONUNIQUEID),
                            CheckType = x.CHECKTYPE,
                            ID = x.ID,
                            Description = x.DESCRIPTION,
                            Unit = x.UNIT
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var checkItem = db.CHECKITEM.First(x => x.UNIQUEID == UniqueID);

                                                     result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = checkItem.UNIQUEID,
                        Permission = Account.OrganizationPermission(checkItem.ORGANIZATIONUNIQUEID),
                        OrganizationUniqueID = checkItem.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(checkItem.ORGANIZATIONUNIQUEID),
                        CheckType = checkItem.CHECKTYPE,
                        ID = checkItem.ID,
                        Description = checkItem.DESCRIPTION,
                        IsFeelItem = checkItem.ISFEELITEM=="Y",
                        UpperLimit = checkItem.UPPERLIMIT.HasValue ? checkItem.UPPERLIMIT.ToString() : "",
                        UpperAlertLimit = checkItem.UPPERALERTLIMIT.HasValue ? checkItem.UPPERALERTLIMIT.ToString() : "",
                        LowerAlertLimit = checkItem.LOWERALERTLIMIT.HasValue ? checkItem.LOWERALERTLIMIT.ToString() : "",
                        LowerLimit = checkItem.LOWERLIMIT.HasValue ? checkItem.LOWERLIMIT.ToString() : "",
                        IsAccumulation = checkItem.ISACCUMULATION=="Y",
                        AccumulationBase = checkItem.ACCUMULATIONBASE.HasValue ? checkItem.ACCUMULATIONBASE.ToString() : "",
                        Unit = checkItem.UNIT,
                        Remark = checkItem.REMARK,
                        AbnormalReasonDescriptionList = (from x in db.CHECKITEMABNORMALREASON
                                                         join a in db.ABNORMALREASON
                                                         on x.ABNORMALREASONUNIQUEID equals a.UNIQUEID
                                                         where x.CHECKITEMUNIQUEID == UniqueID
                                                         orderby a.ABNORMALTYPE, a.ID
                                                         select a.DESCRIPTION).ToList(),
                        FeelOptionDescriptionList = db.CHECKITEMFEELOPTION.Where(x=>x.CHECKITEMUNIQUEID==UniqueID).OrderBy(x=>x.SEQ).ToList().Select(x=>x.DESCRIPTION+ (x.ISABNORMAL == "Y" ? "(" + Resources.Resource.Abnormal + ")" : "")).ToList()
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
                using (ASEDbEntities db = new ASEDbEntities())
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

                    model.CheckTypeSelectItemList.AddRange(db.CHECKITEM.Where(x => upStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)).Select(x => x.CHECKTYPE).Distinct().OrderBy(x => x).Select(x => new SelectListItem
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var checkItem = db.CHECKITEM.First(x => x.UNIQUEID == UniqueID);

                    var model = new CreateFormModel()
                    {
                        OrganizationUniqueID = checkItem.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(checkItem.ORGANIZATIONUNIQUEID),
                        CheckTypeSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne),
                            new SelectListItem()
                            {
                                Text = Resources.Resource.Create + "...",
                                Value = Define.NEW
                            }
                        },
                        AbnormalReasonList = (from x in db.CHECKITEMABNORMALREASON
                                              join a in db.ABNORMALREASON
                                              on x.ABNORMALREASONUNIQUEID equals a.UNIQUEID
                                              where x.CHECKITEMUNIQUEID == UniqueID
                                              select new AbnormalReasonModel
                                              {
                                                  UniqueID = a.UNIQUEID,
                                                  AbnormalType = a.ABNORMALTYPE,
                                                  ID = a.ID,
                                                  Description = a.DESCRIPTION
                                              }).OrderBy(x => x.AbnormalType).ThenBy(x => x.ID).ToList(),
                        FormInput = new FormInput()
                        {
                            CheckType = checkItem.CHECKTYPE,
                            IsFeelItem = checkItem.ISFEELITEM,
                            LowerLimit = checkItem.LOWERLIMIT.HasValue ? checkItem.LOWERLIMIT.ToString() : "",
                            LowerAlertLimit = checkItem.LOWERALERTLIMIT.HasValue ? checkItem.LOWERALERTLIMIT.ToString() : "",
                            UpperAlertLimit = checkItem.UPPERALERTLIMIT.HasValue ? checkItem.UPPERALERTLIMIT.ToString() : "",
                            UpperLimit = checkItem.UPPERLIMIT.HasValue ? checkItem.UPPERLIMIT.ToString() : "",
                            IsAccumulation = checkItem.ISACCUMULATION=="Y",
                            AccumulationBase = checkItem.ACCUMULATIONBASE.HasValue ? checkItem.ACCUMULATIONBASE.ToString() : "",
                            Unit = checkItem.UNIT,
                            Remark = checkItem.REMARK
                        }
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(checkItem.ORGANIZATIONUNIQUEID, true);

                    model.CheckTypeSelectItemList.AddRange(db.CHECKITEM.Where(x => upStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)).Select(x => x.CHECKTYPE).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    model.CheckTypeSelectItemList.First(x => x.Value == checkItem.CHECKTYPE).Selected = true;

                    if (checkItem.ISFEELITEM=="Y")
                    {
                        model.FeelOptionList = (from x in db.CHECKITEMFEELOPTION
                                                where x.CHECKITEMUNIQUEID == UniqueID
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
                if (Model.FormInput.CheckType == Define.OTHER || Model.FormInput.CheckType == Define.NEW)
                {
                    result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.Unsupported, Resources.Resource.CheckType));
                }
                else
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var exists = db.CHECKITEM.FirstOrDefault(x => x.ORGANIZATIONUNIQUEID == Model.OrganizationUniqueID && x.CHECKTYPE == Model.FormInput.CheckType && x.ID == Model.FormInput.ID);

                        if (exists == null)
                        {
                            string uniqueID = Guid.NewGuid().ToString();

                            db.CHECKITEM.Add(new CHECKITEM()
                            {
                                UNIQUEID = uniqueID,
                                ORGANIZATIONUNIQUEID = Model.OrganizationUniqueID,
                                CHECKTYPE = Model.FormInput.CheckType,
                                ID = Model.FormInput.ID,
                                DESCRIPTION = Model.FormInput.Description,
                                ISFEELITEM = Model.FormInput.IsFeelItem,
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

                            if (Model.FormInput.IsFeelItem=="Y")
                            {
                                db.CHECKITEMFEELOPTION.AddRange(Model.FormInput.FeelOptionList.Select(x => new CHECKITEMFEELOPTION
                                {
                                    UNIQUEID = Guid.NewGuid().ToString(),
                                    CHECKITEMUNIQUEID = uniqueID,
                                    DESCRIPTION = x.Description,
                                    ISABNORMAL = x.IsAbnormal?"Y":"N",
                                    SEQ = x.Seq
                                }).ToList());
                            }

                            db.CHECKITEMABNORMALREASON.AddRange(Model.AbnormalReasonList.Select(x => new CHECKITEMABNORMALREASON
                            {
                                CHECKITEMUNIQUEID = uniqueID,
                                ABNORMALREASONUNIQUEID = x.UniqueID
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var checkItem = db.CHECKITEM.First(x => x.UNIQUEID == UniqueID);

                    var model = new EditFormModel()
                    {
                        UniqueID = checkItem.UNIQUEID,
                        OrganizationUniqueID = checkItem.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(checkItem.ORGANIZATIONUNIQUEID),
                        CheckTypeSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne),
                            new SelectListItem()
                            {
                                Text = Resources.Resource.Create + "...",
                                Value = Define.NEW
                            }
                        },
                        AbnormalReasonList = (from x in db.CHECKITEMABNORMALREASON
                                              join a in db.ABNORMALREASON
                                              on x.ABNORMALREASONUNIQUEID equals a.UNIQUEID
                                              where x.CHECKITEMUNIQUEID == UniqueID
                                              select new AbnormalReasonModel
                                              {
                                                  UniqueID = a.UNIQUEID,
                                                  AbnormalType = a.ABNORMALTYPE,
                                                  ID = a.ID,
                                                  Description = a.DESCRIPTION
                                              }).OrderBy(x => x.AbnormalType).ThenBy(x => x.ID).ToList(),
                        FormInput = new FormInput()
                        {
                            CheckType = checkItem.CHECKTYPE,
                            ID = checkItem.ID,
                            Description = checkItem.DESCRIPTION,
                            IsFeelItem = checkItem.ISFEELITEM,
                            LowerLimit = checkItem.LOWERLIMIT.HasValue ? checkItem.LOWERLIMIT.ToString() : "",
                            LowerAlertLimit = checkItem.LOWERALERTLIMIT.HasValue ? checkItem.LOWERALERTLIMIT.ToString() : "",
                            UpperAlertLimit = checkItem.UPPERALERTLIMIT.HasValue ? checkItem.UPPERALERTLIMIT.ToString() : "",
                            UpperLimit = checkItem.UPPERLIMIT.HasValue ? checkItem.UPPERLIMIT.ToString() : "",
                            IsAccumulation = checkItem.ISACCUMULATION == "Y",
                            AccumulationBase = checkItem.ACCUMULATIONBASE.HasValue ? checkItem.ACCUMULATIONBASE.ToString() : "",
                            Unit = checkItem.UNIT,
                            Remark = checkItem.REMARK
                        }
                    };

                    var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(model.OrganizationUniqueID, true);

                    model.CheckTypeSelectItemList.AddRange(db.CHECKITEM.Where(x => upStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)).Select(x => x.CHECKTYPE).Distinct().OrderBy(x => x).Select(x => new SelectListItem
                    {
                        Value = x,
                        Text = x
                    }).ToList());

                    if (!string.IsNullOrEmpty(model.FormInput.CheckType) && model.CheckTypeSelectItemList.Any(x => x.Value == model.FormInput.CheckType))
                    {
                        model.CheckTypeSelectItemList.First(x => x.Value == model.FormInput.CheckType).Selected = true;
                    }

                    if (checkItem.ISFEELITEM=="Y")
                    {
                        model.FeelOptionList = (from x in db.CHECKITEMFEELOPTION
                                                where x.CHECKITEMUNIQUEID == UniqueID
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
                if (Model.FormInput.CheckType == Define.OTHER || Model.FormInput.CheckType == Define.NEW)
                {
                    result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.Unsupported, Resources.Resource.CheckType));
                }
                else
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var checkItem = db.CHECKITEM.First(x => x.UNIQUEID == Model.UniqueID);

                        var exists = db.CHECKITEM.FirstOrDefault(x => x.UNIQUEID != checkItem.UNIQUEID && x.ORGANIZATIONUNIQUEID == checkItem.ORGANIZATIONUNIQUEID && x.CHECKTYPE == Model.FormInput.CheckType && x.ID == Model.FormInput.ID);

                        if (exists == null)
                        {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                            #region CheckItem
                            checkItem.CHECKTYPE = Model.FormInput.CheckType;
                            checkItem.ID = Model.FormInput.ID;
                            checkItem.DESCRIPTION = Model.FormInput.Description;
                            checkItem.ISFEELITEM = Model.FormInput.IsFeelItem;
                            checkItem.UPPERLIMIT = !string.IsNullOrEmpty(Model.FormInput.UpperLimit) ? decimal.Parse(Model.FormInput.UpperLimit) : default(decimal?);
                            checkItem.UPPERALERTLIMIT = !string.IsNullOrEmpty(Model.FormInput.UpperAlertLimit) ? decimal.Parse(Model.FormInput.UpperAlertLimit) : default(decimal?);
                            checkItem.LOWERALERTLIMIT = !string.IsNullOrEmpty(Model.FormInput.LowerAlertLimit) ? decimal.Parse(Model.FormInput.LowerAlertLimit) : default(decimal?);
                            checkItem.LOWERLIMIT = !string.IsNullOrEmpty(Model.FormInput.LowerLimit) ? decimal.Parse(Model.FormInput.LowerLimit) : default(decimal?);
                            checkItem.ISACCUMULATION = Model.FormInput.IsAccumulation ? "Y" : "N";
                            checkItem.ACCUMULATIONBASE = !string.IsNullOrEmpty(Model.FormInput.AccumulationBase) ? decimal.Parse(Model.FormInput.AccumulationBase) : default(decimal?);
                            checkItem.REMARK = Model.FormInput.Remark;
                            checkItem.UNIT = Model.FormInput.Unit;
                            checkItem.LASTMODIFYTIME = DateTime.Now;

                            db.SaveChanges();
                            #endregion

                            #region CheckItemFeelOption
                            #region Delete
                            db.CHECKITEMFEELOPTION.RemoveRange(db.CHECKITEMFEELOPTION.Where(x => x.CHECKITEMUNIQUEID == Model.UniqueID).ToList());

                            db.SaveChanges();
                            #endregion

                            #region Insert
                            if (Model.FormInput.IsFeelItem=="Y")
                            {
                                db.CHECKITEMFEELOPTION.AddRange(Model.FormInput.FeelOptionList.Select(x => new CHECKITEMFEELOPTION
                                {
                                    UNIQUEID = !string.IsNullOrEmpty(x.UniqueID) ? x.UniqueID : Guid.NewGuid().ToString(),
                                    CHECKITEMUNIQUEID = checkItem.UNIQUEID,
                                    DESCRIPTION = x.Description,
                                    ISABNORMAL = x.IsAbnormal ? "Y" : "N",
                                    SEQ = x.Seq
                                }).ToList());
                            }

                            db.SaveChanges();
                            #endregion
                            #endregion

                            #region CheckItemAbnormalReason
                            #region Delete
                            db.CHECKITEMABNORMALREASON.RemoveRange(db.CHECKITEMABNORMALREASON.Where(x => x.CHECKITEMUNIQUEID == Model.UniqueID).ToList());

                            db.SaveChanges();
                            #endregion

                            #region Insert
                            db.CHECKITEMABNORMALREASON.AddRange(Model.AbnormalReasonList.Select(x => new CHECKITEMABNORMALREASON
                            {
                                CHECKITEMUNIQUEID = checkItem.UNIQUEID,
                                ABNORMALREASONUNIQUEID = x.UniqueID
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

        public static RequestResult GetTreeItem(List<Models.Shared.Organization> OrganizationList, string OrganizationUniqueID, string CheckType, Account Account)
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (string.IsNullOrEmpty(CheckType))
                    {
                        if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                        {
                            var checkTypeList = db.CHECKITEM.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).Select(x => x.CHECKTYPE).Distinct().OrderBy(x => x).ToList();

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

                        var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

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

                            if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                                ||
                                (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.CHECKITEM.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID)))
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else
                    {
                        var checkItemList = db.CHECKITEM.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID && x.CHECKTYPE == CheckType).OrderBy(x => x.ID).ToList();

                        foreach (var checkItem in checkItemList)
                        {
                            var treeItem = new TreeItem() { Title = checkItem.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.CheckItem.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", checkItem.ID, checkItem.DESCRIPTION);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = checkItem.ORGANIZATIONUNIQUEID;
                            attributes[Define.EnumTreeAttribute.CheckType] = checkItem.CHECKTYPE;
                            attributes[Define.EnumTreeAttribute.CheckItemUniqueID] = checkItem.UNIQUEID;

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
                    { Define.EnumTreeAttribute.CheckType, string.Empty },
                    { Define.EnumTreeAttribute.CheckItemUniqueID, string.Empty }
                };

                var organization = OrganizationList.First(x => x.UniqueID == OrganizationUniqueID);

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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                        ||
                        (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.CHECKITEM.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID)))
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

        public static RequestResult GetTreeItem(List<Models.Shared.Organization> OrganizationList, string RefOrganizationUniqueID, string OrganizationUniqueID, string CheckType)
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (string.IsNullOrEmpty(CheckType))
                    {
                        var checkTypeList = db.CHECKITEM.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).Select(x => x.CHECKTYPE).Distinct().OrderBy(x => x).ToList();

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
                        var queryableOrganizationList = OrganizationDataAccessor.GetQueryableOrganizationList(RefOrganizationUniqueID);

                        var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && (availableOrganizationList.Contains(x.UniqueID) || queryableOrganizationList.Contains(x.UniqueID))).OrderBy(x => x.ID).ToList();

                        foreach (var organization in organizationList)
                        {
                            var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organization.UniqueID, true);

                            if (db.CHECKITEM.Any(x => downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID) && (availableOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)||queryableOrganizationList.Contains(x.ORGANIZATIONUNIQUEID))))
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
                    else
                    {
                        var checkItemList = db.CHECKITEM.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID && x.CHECKTYPE == CheckType).OrderBy(x => x.ID).ToList();

                        foreach (var checkItem in checkItemList)
                        {
                            var treeItem = new TreeItem() { Title = checkItem.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.CheckItem.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", checkItem.ID, checkItem.DESCRIPTION);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = checkItem.ORGANIZATIONUNIQUEID;
                            attributes[Define.EnumTreeAttribute.CheckType] = checkItem.CHECKTYPE;
                            attributes[Define.EnumTreeAttribute.CheckItemUniqueID] = checkItem.UNIQUEID;

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
