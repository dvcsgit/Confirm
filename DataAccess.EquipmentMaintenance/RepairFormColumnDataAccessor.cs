using DbEntity.MSSQL.EquipmentMaintenance;
using Models.Authenticated;
using Models.EquipmentMaintenance.RepairFormColumnManagement;
using Utility.Models;
using System.Linq;
using System;
using System.Reflection;
using Utility;
using System.Collections.Generic;
using System.Web.Mvc;
using Models.Shared;
using DbEntity.MSSQL;
using System.Transactions;

namespace DataAccess.EquipmentMaintenance
{
    public class RepairFormColumnDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var query = (from x in db.RFormColumn
                                 where x.AncestorOrganizationUniqueID == Parameters.AncestorOrganizationUniqueID
                                 select new
                                 {
                                     x.UniqueID,
                                     x.Description
                                 }).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.Description.Contains(Parameters.Keyword));
                    }

                    result.ReturnData(new GridViewModel()
                    {
                        AncestorOrganizationUniqueID = Parameters.AncestorOrganizationUniqueID,
                        AncestorOrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(Parameters.AncestorOrganizationUniqueID),
                        ItemList = query.ToList().Select(x => new GridItem()
                        {
                            UniqueID = x.UniqueID,
                            Description = x.Description,
                            OptionDescriptionList = db.RFormColumnOption.Where(o => o.ColumnUniqueID == x.UniqueID).OrderBy(o => o.Seq).Select(o => o.Description).ToList(),
                            CanDelete = !(from f in db.RForm
                                        join y in db.RFormTypeColumn
                                        on f.RFormTypeUniqueID equals y.RFormTypeUniqueID
                                        where y.ColumnUniqueID ==x.UniqueID
                                        select f).Any()
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

        public static RequestResult GetDetailViewModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var column = db.RFormColumn.First(x => x.UniqueID == UniqueID);
                    
                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = column.UniqueID,
                        AncestorOrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(column.AncestorOrganizationUniqueID),
                        Description = column.Description,
                        OptionDescriptionList = db.RFormColumnOption.Where(x => x.ColumnUniqueID == column.UniqueID).OrderBy(x => x.Seq).Select(x => x.Description).ToList(),
                        CanDelete = !(from f in db.RForm
                                      join y in db.RFormTypeColumn
                                      on f.RFormTypeUniqueID equals y.RFormTypeUniqueID
                                      where y.ColumnUniqueID == column.UniqueID
                                      select f).Any()
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
                    var exists = db.RFormColumn.FirstOrDefault(x => x.AncestorOrganizationUniqueID == Model.AncestorOrganizationUniqueID && x.Description == Model.FormInput.Description);

                    if (exists == null)
                    {
                        string uniqueID = Guid.NewGuid().ToString();

                        db.RFormColumn.Add(new RFormColumn()
                        {
                            UniqueID = uniqueID,
                            AncestorOrganizationUniqueID = Model.AncestorOrganizationUniqueID,
                            Description = Model.FormInput.Description
                        });

                        db.RFormColumnOption.AddRange(Model.FormInput.OptionList.Select(x => new RFormColumnOption
                        {
                            UniqueID = Guid.NewGuid().ToString(),
                            ColumnUniqueID = uniqueID,
                            Description = x.Description,
                            Seq = x.Seq
                        }).ToList());

                        db.SaveChanges();

                        result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.RepairFormColumn, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.RepairFormColumnDescription, Resources.Resource.Exists));
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
                    var column = db.RFormColumn.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = column.UniqueID,
                        AncestorOrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(column.AncestorOrganizationUniqueID),
                        FormInput = new FormInput()
                        {
                            Description = column.Description
                        },
                        OptionList = db.RFormColumnOption.Where(x => x.ColumnUniqueID == column.UniqueID).Select(x => new OptionModel
                        {
                            UniqueID = x.UniqueID,
                            Description = x.Description,
                            Seq = x.Seq
                        }).OrderBy(x => x.Seq).ToList()
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
                    var column = db.RFormColumn.First(x => x.UniqueID == Model.UniqueID);

                    var exists = db.RFormColumn.FirstOrDefault(x => x.UniqueID != column.UniqueID && x.AncestorOrganizationUniqueID == column.AncestorOrganizationUniqueID && x.Description == Model.FormInput.Description);

                    if (exists == null)
                    {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                        #region RFormColumn
                        column.Description = Model.FormInput.Description;

                        db.SaveChanges();
                        #endregion

                        #region RFormColumnOption
                        #region Delete
                        db.RFormColumnOption.RemoveRange(db.RFormColumnOption.Where(x => x.ColumnUniqueID == Model.UniqueID).ToList());

                        db.SaveChanges();
                        #endregion

                        #region Insert
                        db.RFormColumnOption.AddRange(Model.FormInput.OptionList.Select(x => new RFormColumnOption
                        {
                            UniqueID = !string.IsNullOrEmpty(x.UniqueID) ? x.UniqueID : Guid.NewGuid().ToString(),
                            ColumnUniqueID = column.UniqueID,
                            Description = x.Description,
                            Seq = x.Seq
                        }).ToList());

                        db.SaveChanges();
                        #endregion
                        #endregion

                        #region RFormColumnValue
                        var optionList = Model.FormInput.OptionList.Where(x => !string.IsNullOrEmpty(x.UniqueID)).Select(x => x.UniqueID).ToList();

                        var columnValueList = db.RFormColumnValue.Where(x => x.ColumnUniqueID == column.UniqueID && !optionList.Contains(x.ColumnOptionUniqueID)).ToList();

                        foreach (var columnValue in columnValueList)
                        {
                            columnValue.ColumnOptionUniqueID = string.Empty;
                        }

                        db.SaveChanges();
                        #endregion
#if !DEBUG
                        trans.Complete();
                    }
#endif
                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.RepairFormColumn, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.RepairFormColumnDescription, Resources.Resource.Exists));
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
                    DeleteHelper.RepairFormColumn(db, SelectedList);

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.RepairFormColumn, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetTreeItem(List<Models.Shared.Organization> OrganizationList, string AncestorOrganizationUniqueID, Account Account)
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
                    { Define.EnumTreeAttribute.RepairFormColumnUniqueID, string.Empty }
                };

                using (EDbEntities edb = new EDbEntities())
                {
                    if (AncestorOrganizationUniqueID == "*")
                    {
                        var organizationList = OrganizationList.Where(x => x.ParentUniqueID == "*" && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                        foreach (var organization in organizationList)
                        {
                            var treeItem = new TreeItem() { Title = organization.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                            attributes[Define.EnumTreeAttribute.RepairFormColumnUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (edb.RFormColumn.Any(x => x.AncestorOrganizationUniqueID == organization.UniqueID))
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else
                    {
                        var columnList = edb.RFormColumn.Where(x => x.AncestorOrganizationUniqueID == AncestorOrganizationUniqueID).OrderBy(x => x.Description).ToList();

                        foreach (var column in columnList)
                        {
                            var treeItem = new TreeItem() { Title = column.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.RepairFormColumn.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = column.Description;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = column.AncestorOrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.RepairFormColumnUniqueID] = column.UniqueID;

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
