using System;
using System.Linq;
using System.Reflection;
using Utility;
using Utility.Models;
using DbEntity.ASE;
using Models.EquipmentMaintenance.RepairFormTypeManagement;
using Models.Authenticated;
using Models.Shared;
using System.Collections.Generic;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.ASE
{
    public class RepairFormTypeDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = db.RFORMTYPE.Where(x => x.ANCESTORORGANIZATIONUNIQUEID == Parameters.AncestorOrganizationUniqueID).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.DESCRIPTION.Contains(Parameters.Keyword));
                    }

                    result.ReturnData(new GridViewModel()
                    {
                        AncestorOrganizationUniqueID = Parameters.AncestorOrganizationUniqueID,
                        AncestorOrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(Parameters.AncestorOrganizationUniqueID),
                        ItemList = query.Select(x => new GridItem()
                        {
                            UniqueID = x.UNIQUEID,
                            Description = x.DESCRIPTION,
                            CanDelete = !db.RFORM.Any(f => f.RFORMTYPEUNIQUEID == x.UNIQUEID)
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
                    var type = db.RFORMTYPE.First(x => x.UNIQUEID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = type.UNIQUEID,
                        AncestorOrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(type.ANCESTORORGANIZATIONUNIQUEID),
                        Description = type.DESCRIPTION,
                        CanDelete = !db.RFORM.Any(x => x.RFORMTYPEUNIQUEID == type.UNIQUEID),
                        SubjectList = (from x in db.RFORMTYPESUBJECT
                                       join j in db.RFORMSUBJECT
                                       on x.SUBJECTUNIQUEID equals j.UNIQUEID
                                       where x.RFORMTYPEUNIQUEID == type.UNIQUEID
                                       select new SubjectModel
                                       {
                                           UniqueID = j.UNIQUEID,
                                           ID = j.ID,
                                           Description = j.DESCRIPTION,
                                           Seq = x.SEQ.Value
                                       }).OrderBy(j => j.Seq).ToList(),
                        ColumnList = (from x in db.RFORMTYPECOLUMN
                                      join c in db.RFORMCOLUMN
                                      on x.COLUMNUNIQUEID equals c.UNIQUEID
                                      where x.RFORMTYPEUNIQUEID == type.UNIQUEID
                                      select new ColumnModel
                                      {
                                          UniqueID = c.UNIQUEID,
                                          Description = c.DESCRIPTION,
                                          Seq = x.SEQ.Value,
                                          OptionDescriptionList = db.RFORMCOLUMNOPTION.Where(o => o.COLUMNUNIQUEID == c.UNIQUEID).OrderBy(o => o.SEQ).Select(o => o.DESCRIPTION).ToList()
                                      }).OrderBy(c => c.Seq).ToList()
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

        public static RequestResult GetCopyFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var type = db.RFORMTYPE.First(x => x.UNIQUEID == UniqueID);

                    result.ReturnData(new CreateFormModel()
                    {
                        AncestorOrganizationUniqueID = type.ANCESTORORGANIZATIONUNIQUEID,
                        AncestorOrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(type.ANCESTORORGANIZATIONUNIQUEID),
                        SubjectList = (from x in db.RFORMTYPESUBJECT
                                       join j in db.RFORMSUBJECT
                                       on x.SUBJECTUNIQUEID equals j.UNIQUEID
                                       where x.RFORMTYPEUNIQUEID == type.UNIQUEID
                                       select new SubjectModel
                                       {
                                           UniqueID = j.UNIQUEID,
                                           ID = j.ID,
                                           Description = j.DESCRIPTION,
                                           Seq = x.SEQ.Value
                                       }).OrderBy(j => j.Seq).ToList(),
                        ColumnList = (from x in db.RFORMTYPECOLUMN
                                      join c in db.RFORMCOLUMN
                                      on x.COLUMNUNIQUEID equals c.UNIQUEID
                                      where x.RFORMTYPEUNIQUEID == type.UNIQUEID
                                      select new ColumnModel
                                      {
                                          UniqueID = c.UNIQUEID,
                                          Description = c.DESCRIPTION,
                                          Seq = x.SEQ.Value,
                                          OptionDescriptionList = db.RFORMCOLUMNOPTION.Where(o => o.COLUMNUNIQUEID == c.UNIQUEID).OrderBy(o => o.SEQ).Select(o => o.DESCRIPTION).ToList()
                                      }).OrderBy(c => c.Seq).ToList()
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
                    var exists = db.RFORMTYPE.FirstOrDefault(x => x.ANCESTORORGANIZATIONUNIQUEID == Model.AncestorOrganizationUniqueID && x.DESCRIPTION == Model.FormInput.Description);

                    if (exists == null)
                    {
                        string uniqueID = Guid.NewGuid().ToString();

                        db.RFORMTYPE.Add(new RFORMTYPE()
                        {
                            UNIQUEID = uniqueID,
                            ANCESTORORGANIZATIONUNIQUEID = Model.AncestorOrganizationUniqueID,
                            DESCRIPTION = Model.FormInput.Description
                        });

                        db.RFORMTYPESUBJECT.AddRange(Model.SubjectList.Select(x => new RFORMTYPESUBJECT
                        {
                            RFORMTYPEUNIQUEID = uniqueID,
                            SUBJECTUNIQUEID = x.UniqueID,
                            SEQ = x.Seq
                        }).ToList());

                        db.RFORMTYPECOLUMN.AddRange(Model.ColumnList.Select(x => new RFORMTYPECOLUMN
                        {
                            RFORMTYPEUNIQUEID = uniqueID,
                            COLUMNUNIQUEID = x.UniqueID,
                            SEQ = x.Seq
                        }).ToList());

                        db.SaveChanges();

                        result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.RepairFormType, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.RepairFormTypeDescription, Resources.Resource.Exists));
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
                    var type = db.RFORMTYPE.First(x => x.UNIQUEID == UniqueID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = type.UNIQUEID,
                        AncestorOrganizationUniqueID = type.ANCESTORORGANIZATIONUNIQUEID,
                        AncestorOrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(type.ANCESTORORGANIZATIONUNIQUEID),
                        FormInput = new FormInput()
                        {
                            Description = type.DESCRIPTION
                        },
                        SubjectList = (from x in db.RFORMTYPESUBJECT
                                       join j in db.RFORMSUBJECT
                                       on x.SUBJECTUNIQUEID equals j.UNIQUEID
                                       where x.RFORMTYPEUNIQUEID == type.UNIQUEID
                                       select new SubjectModel
                                       {
                                           UniqueID = j.UNIQUEID,
                                           ID = j.ID,
                                           Description = j.DESCRIPTION,
                                           Seq = x.SEQ.Value
                                       }).OrderBy(j => j.Seq).ToList(),
                        ColumnList = (from x in db.RFORMTYPECOLUMN
                                      join c in db.RFORMCOLUMN
                                      on x.COLUMNUNIQUEID equals c.UNIQUEID
                                      where x.RFORMTYPEUNIQUEID == type.UNIQUEID
                                      select new ColumnModel
                                      {
                                          UniqueID = c.UNIQUEID,
                                          Description = c.DESCRIPTION,
                                          Seq = x.SEQ.Value,
                                          OptionDescriptionList = db.RFORMCOLUMNOPTION.Where(o => o.COLUMNUNIQUEID == c.UNIQUEID).OrderBy(o => o.SEQ).Select(o => o.DESCRIPTION).ToList()
                                      }).OrderBy(c => c.Seq).ToList()
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
                    var type = db.RFORMTYPE.First(x => x.UNIQUEID == Model.UniqueID);

                    var exists = db.RFORMTYPE.FirstOrDefault(x => x.UNIQUEID != type.UNIQUEID && x.ANCESTORORGANIZATIONUNIQUEID == type.ANCESTORORGANIZATIONUNIQUEID && x.DESCRIPTION == Model.FormInput.Description);

                    if (exists == null)
                    {
#if !DEBUG
                        using (TransactionScope trans = new TransactionScope())
                        {
#endif
                            type.DESCRIPTION = Model.FormInput.Description;

                            db.SaveChanges();

                            db.RFORMTYPESUBJECT.RemoveRange(db.RFORMTYPESUBJECT.Where(x => x.RFORMTYPEUNIQUEID == type.UNIQUEID).ToList());

                            db.SaveChanges();

                            db.RFORMTYPESUBJECT.AddRange(Model.SubjectList.Select(x => new RFORMTYPESUBJECT
                            {
                                RFORMTYPEUNIQUEID = type.UNIQUEID,
                                SUBJECTUNIQUEID = x.UniqueID,
                                SEQ = x.Seq
                            }).ToList());

                            db.SaveChanges();

                            db.RFORMTYPECOLUMN.RemoveRange(db.RFORMTYPECOLUMN.Where(x => x.RFORMTYPEUNIQUEID == type.UNIQUEID).ToList());

                            db.SaveChanges();

                            db.RFORMTYPECOLUMN.AddRange(Model.ColumnList.Select(x => new RFORMTYPECOLUMN
                            {
                                RFORMTYPEUNIQUEID = type.UNIQUEID,
                                COLUMNUNIQUEID = x.UniqueID,
                                SEQ = x.Seq
                            }).ToList());

                            db.SaveChanges();
#if !DEBUG
                            trans.Complete();
                        }
#endif

                            result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.RepairFormType, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.RepairFormTypeDescription, Resources.Resource.Exists));
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
#if !DEBUG
                using (TransactionScope trans = new TransactionScope())
                {
#endif
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    DeleteHelper.RepairFormType(db, SelectedList);

                    db.SaveChanges();
                }
#if !DEBUG
                    trans.Complete();
                }
#endif

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.RepairFormType, Resources.Resource.Success));
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
                    { Define.EnumTreeAttribute.RepairFormTypeUniqueID, string.Empty }
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
                            attributes[Define.EnumTreeAttribute.RepairFormTypeUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (db.RFORMTYPE.Any(x => x.ANCESTORORGANIZATIONUNIQUEID == organization.UniqueID))
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else
                    {
                        var repairFormTypeList = db.RFORMTYPE.Where(x => x.ANCESTORORGANIZATIONUNIQUEID == AncestorOrganizationUniqueID).OrderBy(x => x.DESCRIPTION).ToList();

                        foreach (var repairFormType in repairFormTypeList)
                        {
                            var treeItem = new TreeItem() { Title = repairFormType.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.RepairFormType.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = repairFormType.DESCRIPTION;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = repairFormType.ANCESTORORGANIZATIONUNIQUEID;
                            attributes[Define.EnumTreeAttribute.RepairFormTypeUniqueID] = repairFormType.UNIQUEID;

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

        public static RequestResult GetSubjectTreeItem(string OrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>()
                {
                    { Define.EnumTreeAttribute.NodeType, string.Empty },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.RepairFormSubjectUniqueID, string.Empty }
                };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var subjectList = db.RFORMSUBJECT.Where(x => x.ANCESTORORGANIZATIONUNIQUEID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                    foreach (var subject in subjectList)
                    {
                        var treeItem = new TreeItem() { Title = subject.DESCRIPTION };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.RepairFormSubject.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", subject.ID, subject.DESCRIPTION);
                        attributes[Define.EnumTreeAttribute.RepairFormSubjectUniqueID] = subject.UNIQUEID;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
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

        public static RequestResult GetColumnTreeItem(string OrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>()
                {
                    { Define.EnumTreeAttribute.NodeType, string.Empty },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.RepairFormColumnUniqueID, string.Empty }
                };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var columnList = db.RFORMCOLUMN.Where(x => x.ANCESTORORGANIZATIONUNIQUEID == OrganizationUniqueID).OrderBy(x => x.DESCRIPTION).ToList();

                    foreach (var column in columnList)
                    {
                        var treeItem = new TreeItem() { Title = column.DESCRIPTION };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.RepairFormColumn.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = column.DESCRIPTION;
                        attributes[Define.EnumTreeAttribute.RepairFormColumnUniqueID] = column.UNIQUEID;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
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

        public static RequestResult AddSubject(List<SubjectModel> SubjectList, List<string> SelectedList)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    foreach (string selected in SelectedList)
                    {
                        if (!SubjectList.Any(x => x.UniqueID == selected))
                        {
                            var subject = db.RFORMSUBJECT.First(x => x.UNIQUEID == selected);

                            SubjectList.Add(new SubjectModel() {
                                UniqueID = subject.UNIQUEID,
                                ID = subject.ID,
                                Description = subject.DESCRIPTION,
                                 Seq = SubjectList.Count+1
                            });
                        }
                    }
                }

                SubjectList = SubjectList.OrderBy(x => x.Seq).ToList();

                result.ReturnData(SubjectList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult AddColumn(List<ColumnModel> ColumnList, List<string> SelectedList)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    foreach (string selected in SelectedList)
                    {
                        if (!ColumnList.Any(x => x.UniqueID == selected))
                        {
                            var column = db.RFORMCOLUMN.First(x => x.UNIQUEID == selected);

                            ColumnList.Add(new ColumnModel()
                            {
                                UniqueID = column.UNIQUEID,
                                Description = column.DESCRIPTION,
                                 OptionDescriptionList = db.RFORMCOLUMNOPTION.Where(o => o.COLUMNUNIQUEID == column.UNIQUEID).OrderBy(o => o.SEQ).Select(o => o.DESCRIPTION).ToList(),
                                Seq = ColumnList.Count + 1
                            });
                        }
                    }
                }

                ColumnList = ColumnList.OrderBy(x => x.Seq).ToList();

                result.ReturnData(ColumnList);
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
