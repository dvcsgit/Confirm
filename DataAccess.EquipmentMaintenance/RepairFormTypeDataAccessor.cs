using System;
using System.Linq;
using System.Reflection;
using Utility;
using Utility.Models;
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.EquipmentMaintenance.RepairFormTypeManagement;
using Models.Authenticated;
using Models.Shared;
using System.Collections.Generic;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.EquipmentMaintenance
{
    public class RepairFormTypeDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var query = db.RFormType.Where(x => x.AncestorOrganizationUniqueID == Parameters.AncestorOrganizationUniqueID).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.Description.Contains(Parameters.Keyword));
                    }

                    result.ReturnData(new GridViewModel()
                    {
                        AncestorOrganizationUniqueID = Parameters.AncestorOrganizationUniqueID,
                        AncestorOrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(Parameters.AncestorOrganizationUniqueID),
                        ItemList = query.Select(x => new GridItem()
                        {
                            UniqueID = x.UniqueID,
                            Description = x.Description,
                            MTTR = x.MTTR.HasValue&&x.MTTR.Value,
                            CanDelete = !db.RForm.Any(f => f.RFormTypeUniqueID == x.UniqueID)
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
                    var type = db.RFormType.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = type.UniqueID,
                        AncestorOrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(type.AncestorOrganizationUniqueID),
                        Description = type.Description,
                        MTTR = type.MTTR.HasValue&&type.MTTR.Value,
                        CanDelete = !db.RForm.Any(x => x.RFormTypeUniqueID == type.UniqueID),
                        SubjectList = (from x in db.RFormTypeSubject
                                       join j in db.RFormSubject
                                       on x.SubjectUniqueID equals j.UniqueID
                                       where x.RFormTypeUniqueID == type.UniqueID
                                       select new SubjectModel
                                       {
                                           UniqueID = j.UniqueID,
                                           ID = j.ID,
                                           Description = j.Description,
                                           Seq = x.Seq
                                       }).OrderBy(j => j.Seq).ToList(),
                        ColumnList = (from x in db.RFormTypeColumn
                                      join c in db.RFormColumn
                                      on x.ColumnUniqueID equals c.UniqueID
                                      where x.RFormTypeUniqueID == type.UniqueID
                                      select new ColumnModel
                                      {
                                          UniqueID = c.UniqueID,
                                          Description = c.Description,
                                          Seq = x.Seq,
                                          OptionDescriptionList = db.RFormColumnOption.Where(o => o.ColumnUniqueID == c.UniqueID).OrderBy(o => o.Seq).Select(o => o.Description).ToList()
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
                using (EDbEntities db = new EDbEntities())
                {
                    var type = db.RFormType.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new CreateFormModel()
                    {
                        AncestorOrganizationUniqueID = type.AncestorOrganizationUniqueID,
                        AncestorOrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(type.AncestorOrganizationUniqueID),
                        FormInput = new FormInput(){
                         MTTR = type.MTTR.HasValue&&type.MTTR.Value
                        },
                        SubjectList = (from x in db.RFormTypeSubject
                                       join j in db.RFormSubject
                                       on x.SubjectUniqueID equals j.UniqueID
                                       where x.RFormTypeUniqueID == type.UniqueID
                                       select new SubjectModel
                                       {
                                           UniqueID = j.UniqueID,
                                           ID = j.ID,
                                           Description = j.Description,
                                           Seq = x.Seq
                                       }).OrderBy(j => j.Seq).ToList(),
                        ColumnList = (from x in db.RFormTypeColumn
                                      join c in db.RFormColumn
                                      on x.ColumnUniqueID equals c.UniqueID
                                      where x.RFormTypeUniqueID == type.UniqueID
                                      select new ColumnModel
                                      {
                                          UniqueID = c.UniqueID,
                                          Description = c.Description,
                                          Seq = x.Seq,
                                          OptionDescriptionList = db.RFormColumnOption.Where(o => o.ColumnUniqueID == c.UniqueID).OrderBy(o => o.Seq).Select(o => o.Description).ToList()
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
                using (EDbEntities db = new EDbEntities())
                {
                    var exists = db.RFormType.FirstOrDefault(x => x.AncestorOrganizationUniqueID == Model.AncestorOrganizationUniqueID && x.Description == Model.FormInput.Description);

                    if (exists == null)
                    {
                        string uniqueID = Guid.NewGuid().ToString();

                        db.RFormType.Add(new RFormType()
                        {
                            UniqueID = uniqueID,
                            AncestorOrganizationUniqueID = Model.AncestorOrganizationUniqueID,
                            Description = Model.FormInput.Description,
                            MTTR = Model.FormInput.MTTR
                        });

                        db.RFormTypeSubject.AddRange(Model.SubjectList.Select(x => new RFormTypeSubject
                        {
                            RFormTypeUniqueID = uniqueID,
                            SubjectUniqueID = x.UniqueID,
                            Seq = x.Seq
                        }).ToList());

                        db.RFormTypeColumn.AddRange(Model.ColumnList.Select(x => new RFormTypeColumn
                        {
                            RFormTypeUniqueID = uniqueID,
                            ColumnUniqueID = x.UniqueID,
                            Seq = x.Seq
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
                using (EDbEntities db = new EDbEntities())
                {
                    var type = db.RFormType.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = type.UniqueID,
                        AncestorOrganizationUniqueID = type.AncestorOrganizationUniqueID,
                        AncestorOrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(type.AncestorOrganizationUniqueID),
                        FormInput = new FormInput()
                        {
                            Description = type.Description,
                            MTTR = type.MTTR.HasValue&&type.MTTR.Value
                        },
                        SubjectList = (from x in db.RFormTypeSubject
                                       join j in db.RFormSubject
                                       on x.SubjectUniqueID equals j.UniqueID
                                       where x.RFormTypeUniqueID == type.UniqueID
                                       select new SubjectModel
                                       {
                                           UniqueID = j.UniqueID,
                                           ID = j.ID,
                                           Description = j.Description,
                                           Seq = x.Seq
                                       }).OrderBy(j => j.Seq).ToList(),
                        ColumnList = (from x in db.RFormTypeColumn
                                      join c in db.RFormColumn
                                      on x.ColumnUniqueID equals c.UniqueID
                                      where x.RFormTypeUniqueID == type.UniqueID
                                      select new ColumnModel
                                      {
                                          UniqueID = c.UniqueID,
                                          Description = c.Description,
                                          Seq = x.Seq,
                                          OptionDescriptionList = db.RFormColumnOption.Where(o => o.ColumnUniqueID == c.UniqueID).OrderBy(o => o.Seq).Select(o => o.Description).ToList()
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
                using (EDbEntities db = new EDbEntities())
                {
                    var type = db.RFormType.First(x => x.UniqueID == Model.UniqueID);

                    var exists = db.RFormType.FirstOrDefault(x => x.UniqueID != type.UniqueID && x.AncestorOrganizationUniqueID == type.AncestorOrganizationUniqueID && x.Description == Model.FormInput.Description);

                    if (exists == null)
                    {
#if !DEBUG
                        using (TransactionScope trans = new TransactionScope())
                        {
#endif
                            type.Description = Model.FormInput.Description;
                            type.MTTR = Model.FormInput.MTTR;

                            db.SaveChanges();

                            db.RFormTypeSubject.RemoveRange(db.RFormTypeSubject.Where(x => x.RFormTypeUniqueID == type.UniqueID).ToList());

                            db.SaveChanges();

                            db.RFormTypeSubject.AddRange(Model.SubjectList.Select(x => new RFormTypeSubject
                            {
                                RFormTypeUniqueID = type.UniqueID,
                                SubjectUniqueID = x.UniqueID,
                                Seq = x.Seq
                            }).ToList());

                            db.SaveChanges();

                            db.RFormTypeColumn.RemoveRange(db.RFormTypeColumn.Where(x => x.RFormTypeUniqueID == type.UniqueID).ToList());

                            db.SaveChanges();

                            db.RFormTypeColumn.AddRange(Model.ColumnList.Select(x => new RFormTypeColumn
                            {
                                RFormTypeUniqueID = type.UniqueID,
                                ColumnUniqueID = x.UniqueID,
                                Seq = x.Seq
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
                using (DbEntities db = new DbEntities())
                {
                    using (EDbEntities edb = new EDbEntities())
                    {
                        DeleteHelper.RepairFormType(db, edb, SelectedList);

                        edb.SaveChanges();
                    }

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
                            attributes[Define.EnumTreeAttribute.RepairFormTypeUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (edb.RFormType.Any(x => x.AncestorOrganizationUniqueID == organization.UniqueID))
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else
                    {
                        var repairFormTypeList = edb.RFormType.Where(x => x.AncestorOrganizationUniqueID == AncestorOrganizationUniqueID).OrderBy(x => x.Description).ToList();

                        foreach (var repairFormType in repairFormTypeList)
                        {
                            var treeItem = new TreeItem() { Title = repairFormType.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.RepairFormType.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = repairFormType.Description;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = repairFormType.AncestorOrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.RepairFormTypeUniqueID] = repairFormType.UniqueID;

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

                using (EDbEntities edb = new EDbEntities())
                {
                    var subjectList = edb.RFormSubject.Where(x => x.AncestorOrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                    foreach (var subject in subjectList)
                    {
                        var treeItem = new TreeItem() { Title = subject.Description };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.RepairFormSubject.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", subject.ID, subject.Description);
                        attributes[Define.EnumTreeAttribute.RepairFormSubjectUniqueID] = subject.UniqueID;

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

                using (EDbEntities edb = new EDbEntities())
                {
                    var columnList = edb.RFormColumn.Where(x => x.AncestorOrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.Description).ToList();

                    foreach (var column in columnList)
                    {
                        var treeItem = new TreeItem() { Title = column.Description };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.RepairFormColumn.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = column.Description;
                        attributes[Define.EnumTreeAttribute.RepairFormColumnUniqueID] = column.UniqueID;

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
                using (EDbEntities db = new EDbEntities())
                {
                    foreach (string selected in SelectedList)
                    {
                        if (!SubjectList.Any(x => x.UniqueID == selected))
                        {
                            var subject = db.RFormSubject.First(x => x.UniqueID == selected);

                            SubjectList.Add(new SubjectModel() {
                                UniqueID = subject.UniqueID,
                                ID = subject.ID,
                                Description = subject.Description,
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
                using (EDbEntities db = new EDbEntities())
                {
                    foreach (string selected in SelectedList)
                    {
                        if (!ColumnList.Any(x => x.UniqueID == selected))
                        {
                            var column = db.RFormColumn.First(x => x.UniqueID == selected);

                            ColumnList.Add(new ColumnModel()
                            {
                                UniqueID = column.UniqueID,
                                Description = column.Description,
                                 OptionDescriptionList = db.RFormColumnOption.Where(o => o.ColumnUniqueID == column.UniqueID).OrderBy(o => o.Seq).Select(o => o.Description).ToList(),
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
