using DbEntity.ASE;
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
using System.Transactions;

namespace DataAccess.ASE
{
    public class RepairFormColumnDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = (from x in db.RFORMCOLUMN
                                 where x.ANCESTORORGANIZATIONUNIQUEID == Parameters.AncestorOrganizationUniqueID
                                 select new
                                 {
                                     x.UNIQUEID,
                                     x.DESCRIPTION
                                 }).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.DESCRIPTION.Contains(Parameters.Keyword));
                    }

                    result.ReturnData(new GridViewModel()
                    {
                        AncestorOrganizationUniqueID = Parameters.AncestorOrganizationUniqueID,
                        AncestorOrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(Parameters.AncestorOrganizationUniqueID),
                        ItemList = query.ToList().Select(x => new GridItem()
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION,
                            OptionDescriptionList = db.RFORMCOLUMNOPTION.Where(o => o.COLUMNUNIQUEID == x.UNIQUEID).OrderBy(o => o.SEQ).Select(o => o.DESCRIPTION).ToList(),
                            CanDelete = !(from f in db.RFORM
                                        join y in db.RFORMTYPECOLUMN
                                        on f.RFORMTYPEUNIQUEID equals y.RFORMTYPEUNIQUEID
                                          where y.COLUMNUNIQUEID == x.UNIQUEID
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var column = db.RFORMCOLUMN.First(x => x.UNIQUEID == UniqueID);
                    
                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = column.UNIQUEID,
                        AncestorOrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(column.ANCESTORORGANIZATIONUNIQUEID),
                        Description = column.DESCRIPTION,
                        OptionDescriptionList = db.RFORMCOLUMNOPTION.Where(x => x.COLUMNUNIQUEID == column.UNIQUEID).OrderBy(x => x.SEQ).Select(x => x.DESCRIPTION).ToList(),
                        CanDelete = !(from f in db.RFORM
                                      join y in db.RFORMTYPECOLUMN
                                      on f.RFORMTYPEUNIQUEID equals y.RFORMTYPEUNIQUEID
                                      where y.COLUMNUNIQUEID == column.UNIQUEID
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var exists = db.RFORMCOLUMN.FirstOrDefault(x => x.ANCESTORORGANIZATIONUNIQUEID == Model.AncestorOrganizationUniqueID && x.DESCRIPTION == Model.FormInput.Description);

                    if (exists == null)
                    {
                        string uniqueID = Guid.NewGuid().ToString();

                        db.RFORMCOLUMN.Add(new RFORMCOLUMN()
                        {
                            UNIQUEID = uniqueID,
                            ANCESTORORGANIZATIONUNIQUEID = Model.AncestorOrganizationUniqueID,
                            DESCRIPTION = Model.FormInput.Description
                        });

                        db.RFORMCOLUMNOPTION.AddRange(Model.FormInput.OptionList.Select(x => new RFORMCOLUMNOPTION
                        {
                            UNIQUEID = Guid.NewGuid().ToString(),
                            COLUMNUNIQUEID = uniqueID,
                            DESCRIPTION = x.Description,
                            SEQ = x.Seq
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var column = db.RFORMCOLUMN.First(x => x.UNIQUEID == UniqueID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = column.UNIQUEID,
                        AncestorOrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(column.ANCESTORORGANIZATIONUNIQUEID),
                        FormInput = new FormInput()
                        {
                            Description = column.DESCRIPTION
                        },
                        OptionList = db.RFORMCOLUMNOPTION.Where(x => x.COLUMNUNIQUEID == column.UNIQUEID).Select(x => new OptionModel
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION,
                            Seq = x.SEQ.Value
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var column = db.RFORMCOLUMN.First(x => x.UNIQUEID == Model.UniqueID);

                    var exists = db.RFORMCOLUMN.FirstOrDefault(x => x.UNIQUEID != column.UNIQUEID && x.ANCESTORORGANIZATIONUNIQUEID == column.ANCESTORORGANIZATIONUNIQUEID && x.DESCRIPTION == Model.FormInput.Description);

                    if (exists == null)
                    {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                        #region RFormColumn
                        column.DESCRIPTION = Model.FormInput.Description;

                        db.SaveChanges();
                        #endregion

                        #region RFormColumnOption
                        #region Delete
                        db.RFORMCOLUMNOPTION.RemoveRange(db.RFORMCOLUMNOPTION.Where(x => x.COLUMNUNIQUEID == Model.UniqueID).ToList());

                        db.SaveChanges();
                        #endregion

                        #region Insert
                        db.RFORMCOLUMNOPTION.AddRange(Model.FormInput.OptionList.Select(x => new RFORMCOLUMNOPTION
                        {
                            UNIQUEID = !string.IsNullOrEmpty(x.UniqueID) ? x.UniqueID : Guid.NewGuid().ToString(),
                            COLUMNUNIQUEID = column.UNIQUEID,
                            DESCRIPTION = x.Description,
                            SEQ  = x.Seq
                        }).ToList());

                        db.SaveChanges();
                        #endregion
                        #endregion

                        #region RFormColumnValue
                        var optionList = Model.FormInput.OptionList.Where(x => !string.IsNullOrEmpty(x.UniqueID)).Select(x => x.UniqueID).ToList();

                        var columnValueList = db.RFORMCOLUMNVALUE.Where(x => x.COLUMNUNIQUEID == column.UNIQUEID && !optionList.Contains(x.COLUMNOPTIONUNIQUEID)).ToList();

                        foreach (var columnValue in columnValueList)
                        {
                            columnValue.COLUMNOPTIONUNIQUEID = string.Empty;
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
                using (ASEDbEntities db = new ASEDbEntities())
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

                using (ASEDbEntities db = new ASEDbEntities())
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

                            if (db.RFORMCOLUMN.Any(x => x.ANCESTORORGANIZATIONUNIQUEID == organization.UniqueID))
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else
                    {
                        var columnList = db.RFORMCOLUMN.Where(x => x.ANCESTORORGANIZATIONUNIQUEID == AncestorOrganizationUniqueID).OrderBy(x => x.DESCRIPTION).ToList();

                        foreach (var column in columnList)
                        {
                            var treeItem = new TreeItem() { Title = column.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.RepairFormColumn.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = column.DESCRIPTION;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = column.ANCESTORORGANIZATIONUNIQUEID;
                            attributes[Define.EnumTreeAttribute.RepairFormColumnUniqueID] = column.UNIQUEID;

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
