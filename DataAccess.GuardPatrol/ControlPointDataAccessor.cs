using System;
using System.Linq;
using System.Reflection;
#if !DEBUG
using System.Transactions;
#endif
using System.Collections.Generic;
using Utility;
using Utility.Models;
#if ORACLE
using DbEntity.ORACLE;
using DbEntity.ORACLE.GuardPatrol;
#else
using DbEntity.MSSQL;
using DbEntity.MSSQL.GuardPatrol;
#endif
using Models.Shared;
using Models.Authenticated;
using Models.GuardPatrol.ControlPointManagement;

namespace DataAccess.GuardPatrol
{
    public class ControlPointDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (GDbEntities db = new GDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.ControlPoint.Where(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.ID.Contains(Parameters.Keyword) || x.Description.Contains(Parameters.Keyword));
                    }

                    var organization = OrganizationDataAccessor.GetOrganization(Parameters.OrganizationUniqueID);

                    result.ReturnData(new GridViewModel()
                    {
                        Permission = Account.OrganizationPermission(Parameters.OrganizationUniqueID),
                        OrganizationUniqueID = Parameters.OrganizationUniqueID,
                        OrganizationDescription = organization.Description,
                        FullOrganizationDescription = organization.FullDescription,
                        ItemList = query.ToList().Select(x => new GridItem
                        {
                            UniqueID = x.UniqueID,
                            Permission = Account.OrganizationPermission(x.OrganizationUniqueID),
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.OrganizationUniqueID),
                            ID = x.ID,
                            Description = x.Description
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
                using (GDbEntities db = new GDbEntities())
                {
                    var controlPoint = db.ControlPoint.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = controlPoint.UniqueID,
                        Permission = Account.OrganizationPermission(controlPoint.OrganizationUniqueID),
                        OrganizationUniqueID = controlPoint.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(controlPoint.OrganizationUniqueID),
                        ID = controlPoint.ID,
                        Description = controlPoint.Description,
#if ORACLE
                        IsFeelItemDefaultNormal = controlPoint.IsFeelItemDefaultNormal==1,
#else
                        IsFeelItemDefaultNormal = controlPoint.IsFeelItemDefaultNormal,
#endif
                        TagID = controlPoint.TagID,
                        Remark = controlPoint.Remark,
                        CheckItemList = (from x in db.ControlPointCheckItem
                                         join c in db.CheckItem
                                         on x.CheckItemUniqueID equals c.UniqueID
                                         where x.ControlPointUniqueID == UniqueID
                                         select new CheckItemModel
                                         {
                                             UniqueID = c.UniqueID,
                                             CheckType = c.CheckType,
                                             ID = c.ID,
                                             Description = c.Description,
#if ORACLE
                                             IsFeelItem = c.IsFeelItem==1,
                                             IsInherit = x.IsInherit==1,
#else
                                             IsFeelItem = c.IsFeelItem,
                                             IsAccumulation = c.IsAccumulation,
                                             IsInherit = x.IsInherit,
#endif
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
                using (GDbEntities db = new GDbEntities())
                {
                    var exists = db.ControlPoint.FirstOrDefault(x => x.OrganizationUniqueID == Model.OrganizationUniqueID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
                        if (!string.IsNullOrEmpty(Model.FormInput.TagID) && db.ControlPoint.Any(x => x.TagID == Model.FormInput.TagID))
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.TagID, Resources.Resource.Exists));
                        }
                        else
                        {
                            string uniqueID = Guid.NewGuid().ToString();

                            db.ControlPoint.Add(new ControlPoint()
                            {
                                UniqueID = uniqueID,
                                OrganizationUniqueID = Model.OrganizationUniqueID,
                                ID = Model.FormInput.ID,
                                Description = Model.FormInput.Description,
#if ORACLE
                                IsFeelItemDefaultNormal = Model.FormInput.IsFeelItemDefaultNormal?1:0,
#else
                                IsFeelItemDefaultNormal = Model.FormInput.IsFeelItemDefaultNormal,
#endif
                                TagID = Model.FormInput.TagID,
                                Remark = Model.FormInput.Remark,
                                LastModifyTime = DateTime.Now
                            });

                            db.ControlPointCheckItem.AddRange(Model.CheckItemList.Select(x => new ControlPointCheckItem
                            {
                                ControlPointUniqueID = uniqueID,
                                CheckItemUniqueID = x.UniqueID,
#if ORACLE
                                IsInherit = x.IsInherit?1:0,
#else
                                IsInherit = x.IsInherit,
#endif
                                LowerLimit = x.LowerLimit,
                                LowerAlertLimit = x.LowerAlertLimit,
                                UpperAlertLimit = x.UpperAlertLimit,
                                UpperLimit = x.UpperLimit,
                                AccumulationBase = x.AccumulationBase,
                                Unit = x.Unit,
                                Remark = x.Remark
                            }).ToList());

                            db.SaveChanges();

                            result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.ControlPoint, Resources.Resource.Success));
                        }
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.ControlPointID, Resources.Resource.Exists));
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

        public static RequestResult GetCopyFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (GDbEntities db = new GDbEntities())
                {
                    var controlPoint = db.ControlPoint.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new CreateFormModel()
                    {
                        OrganizationUniqueID = controlPoint.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(controlPoint.OrganizationUniqueID),
                        CheckItemList = (from x in db.ControlPointCheckItem
                                         join c in db.CheckItem
                                         on x.CheckItemUniqueID equals c.UniqueID
                                         where x.ControlPointUniqueID == UniqueID
                                         select new CheckItemModel
                                         {
                                             UniqueID = c.UniqueID,
                                             CheckType = c.CheckType,
                                             ID = c.ID,
                                             Description = c.Description,
#if ORACLE
                                             IsFeelItem = c.IsFeelItem==1,
                                             IsInherit = x.IsInherit==1,
#else
                                             IsFeelItem = c.IsFeelItem,
                                             IsAccumulation = c.IsAccumulation,
                                             IsInherit = x.IsInherit,
#endif
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
                        FormInput = new FormInput()
                        {
#if ORACLE
                            IsFeelItemDefaultNormal = controlPoint.IsFeelItemDefaultNormal==1,
#else
                            IsFeelItemDefaultNormal = controlPoint.IsFeelItemDefaultNormal,
#endif
                            Remark = controlPoint.Remark
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

        public static RequestResult GetEditFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (GDbEntities db = new GDbEntities())
                {
                    var controlPoint = db.ControlPoint.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = controlPoint.UniqueID,
                        OrganizationUniqueID = controlPoint.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(controlPoint.OrganizationUniqueID),
                        CheckItemList = (from x in db.ControlPointCheckItem
                                         join c in db.CheckItem
                                         on x.CheckItemUniqueID equals c.UniqueID
                                         where x.ControlPointUniqueID == UniqueID
                                         orderby c.CheckType, c.ID
                                         select new CheckItemModel
                                         {
                                             UniqueID = c.UniqueID,
                                             CheckType = c.CheckType,
                                             ID = c.ID,
                                             Description = c.Description,
#if ORACLE
                                             IsFeelItem = c.IsFeelItem==1,
                                             IsInherit = x.IsInherit==1,
#else
                                             IsFeelItem = c.IsFeelItem,
                                             IsAccumulation = c.IsAccumulation,
                                             IsInherit = x.IsInherit,
#endif
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
                                         }).ToList(),
                        FormInput = new FormInput()
                        {
                            ID = controlPoint.ID,
                            Description = controlPoint.Description,
#if ORACLE
                            IsFeelItemDefaultNormal = controlPoint.IsFeelItemDefaultNormal==1,
#else
                            IsFeelItemDefaultNormal = controlPoint.IsFeelItemDefaultNormal,
#endif
                            TagID = controlPoint.TagID,
                            Remark = controlPoint.Remark
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
                using (GDbEntities db = new GDbEntities())
                {
                    var controlPoint = db.ControlPoint.First(x => x.UniqueID == Model.UniqueID);

                    var exists = db.ControlPoint.FirstOrDefault(x => x.UniqueID != controlPoint.UniqueID && x.OrganizationUniqueID == controlPoint.OrganizationUniqueID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
                        if (!string.IsNullOrEmpty(Model.FormInput.TagID) && db.ControlPoint.Any(x => x.UniqueID != controlPoint.UniqueID && x.TagID == Model.FormInput.TagID))
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.TagID, Resources.Resource.Exists));
                        }
                        else
                        {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                            #region ControlPoint
                            controlPoint.ID = Model.FormInput.ID;
                            controlPoint.Description = Model.FormInput.Description;
                            controlPoint.TagID = Model.FormInput.TagID;
                            controlPoint.Remark = Model.FormInput.Remark;
#if ORACLE
                            controlPoint.IsFeelItemDefaultNormal = Model.FormInput.IsFeelItemDefaultNormal?1:0;
#else
                            controlPoint.IsFeelItemDefaultNormal = Model.FormInput.IsFeelItemDefaultNormal;
#endif
                            controlPoint.LastModifyTime = DateTime.Now;

                            db.SaveChanges();
                            #endregion

                            #region ControlPointCheckItem
                            #region Delete
                            db.ControlPointCheckItem.RemoveRange(db.ControlPointCheckItem.Where(x => x.ControlPointUniqueID == Model.UniqueID).ToList());

                            db.SaveChanges();
                            #endregion

                            #region Insert
                            db.ControlPointCheckItem.AddRange(Model.CheckItemList.Select(x => new ControlPointCheckItem
                            {
                                ControlPointUniqueID = controlPoint.UniqueID,
                                CheckItemUniqueID = x.UniqueID,
#if ORACLE
                                IsInherit = x.IsInherit?1:0,
#else
                                IsInherit = x.IsInherit,
#endif
                                LowerLimit = x.LowerLimit,
                                LowerAlertLimit = x.LowerAlertLimit,
                                UpperAlertLimit = x.UpperAlertLimit,
                                UpperLimit = x.UpperLimit,
                                AccumulationBase = x.AccumulationBase,
                                Unit = x.Unit,
                                Remark = x.Remark
                            }).ToList());

                            db.SaveChanges();
                            #endregion
                            #endregion
#if !DEBUG
                        trans.Complete();
                    }
#endif
                            result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.ControlPoint, Resources.Resource.Success));
                        }
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.ControlPointID, Resources.Resource.Exists));
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
#if ORACLE
                        checkItem.LowerLimit = !string.IsNullOrEmpty(lowerLimit) ? decimal.Parse(lowerLimit) : default(decimal?);
                        checkItem.LowerAlertLimit = !string.IsNullOrEmpty(lowerAlertLimit) ? decimal.Parse(lowerAlertLimit) : default(decimal?);
                        checkItem.UpperAlertLimit = !string.IsNullOrEmpty(upperAlertLimit) ? decimal.Parse(upperAlertLimit) : default(decimal?);
                        checkItem.UpperLimit = !string.IsNullOrEmpty(upperLimit) ? decimal.Parse(upperLimit) : default(decimal?);
#else
                        checkItem.LowerLimit = !string.IsNullOrEmpty(lowerLimit) ? double.Parse(lowerLimit) : default(double?);
                        checkItem.LowerAlertLimit = !string.IsNullOrEmpty(lowerAlertLimit) ? double.Parse(lowerAlertLimit) : default(double?);
                        checkItem.UpperAlertLimit = !string.IsNullOrEmpty(upperAlertLimit) ? double.Parse(upperAlertLimit) : default(double?);
                        checkItem.UpperLimit = !string.IsNullOrEmpty(upperLimit) ? double.Parse(upperLimit) : default(double?);
                        checkItem.AccumulationBase = !string.IsNullOrEmpty(accumulationBase) ? double.Parse(accumulationBase) : default(double?);
#endif
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

        public static RequestResult Delete(List<string> SelectedList)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (GDbEntities db = new GDbEntities())
                {
                    foreach (var uniqueID in SelectedList)
                    {
                        db.ControlPoint.Remove(db.ControlPoint.First(x => x.UniqueID == uniqueID));

                        db.ControlPointCheckItem.RemoveRange(db.ControlPointCheckItem.Where(x => x.ControlPointUniqueID == uniqueID).ToList());

                        db.JobControlPoint.RemoveRange(db.JobControlPoint.Where(x => x.ControlPointUniqueID == uniqueID).ToList());

                        db.JobControlPointCheckItem.RemoveRange(db.JobControlPointCheckItem.Where(x => x.ControlPointUniqueID == uniqueID).ToList());

                        db.RouteControlPoint.RemoveRange(db.RouteControlPoint.Where(x => x.ControlPointUniqueID == uniqueID).ToList());

                        db.RouteControlPointCheckItem.RemoveRange(db.RouteControlPointCheckItem.Where(x => x.ControlPointUniqueID == uniqueID).ToList());
                    }

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.ControlPoint, Resources.Resource.Success));
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
                using (GDbEntities db = new GDbEntities())
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
#if ORACLE
                                    IsFeelItem = checkItem.IsFeelItem==1,
#else
                                    IsFeelItem = checkItem.IsFeelItem,
                                    IsAccumulation = checkItem.IsAccumulation,
#endif
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
#if ORACLE
                                            IsFeelItem = checkItem.IsFeelItem==1,
#else
                                            IsFeelItem = checkItem.IsFeelItem,
                                            IsAccumulation = checkItem.IsAccumulation,
#endif
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
#if ORACLE
                                                    IsFeelItem = checkItem.IsFeelItem==1,
#else
                                                    IsFeelItem = checkItem.IsFeelItem,
                                                    IsAccumulation = checkItem.IsAccumulation,
#endif
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

                CheckItemList = CheckItemList.OrderBy(x => x.CheckType).ThenBy(x => x.ID).ToList();

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

        public static RequestResult GetTreeItem(string OrganizationUniqueID, Account Account)
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
                    { Define.EnumTreeAttribute.ControlPointUniqueID, string.Empty }
                };

                using (GDbEntities edb = new GDbEntities())
                {
                    if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                    {
                        var controlPointList = edb.ControlPoint.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                        foreach (var controlPoint in controlPointList)
                        {
                            var treeItem = new TreeItem() { Title = controlPoint.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.ControlPoint.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", controlPoint.ID, controlPoint.Description);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = controlPoint.UniqueID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

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
                            attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (db.Organization.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID)) 
                                ||
                                (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && edb.ControlPoint.Any(x => x.OrganizationUniqueID == organization.UniqueID)))
                            {
                                treeItem.State = "closed";
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

        public static RequestResult GetTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID)
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
                    { Define.EnumTreeAttribute.ControlPointUniqueID, string.Empty }
                };

                using (GDbEntities edb = new GDbEntities())
                {
                    var controlPointList = edb.ControlPoint.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                    foreach (var controlPoint in controlPointList)
                    {
                        var treeItem = new TreeItem() { Title = string.Format("{0}/{1}", controlPoint.ID, controlPoint.Description) };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.ControlPoint.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", controlPoint.ID, controlPoint.Description);
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                        attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = controlPoint.UniqueID;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        treeItemList.Add(treeItem);
                    }

                    var availableOrganizationList = OrganizationDataAccessor.GetUserOrganizationPermissionList(RefOrganizationUniqueID).Select(x => x.UniqueID).ToList();

                    using(DbEntities db = new DbEntities())
                    {
                        var organizationList = db.Organization.Where(x => x.ParentUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                        foreach (var organization in organizationList)
                        {
                            var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organization.UniqueID, true);

                            if (edb.ControlPoint.Any(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && availableOrganizationList.Contains(x.OrganizationUniqueID)))
                            {
                                var treeItem = new TreeItem() { Title = organization.Description };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                                attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = string.Empty;

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
