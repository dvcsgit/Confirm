using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.Authenticated;
using Models.EquipmentMaintenance.RepairFormSubjectManagement;
using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace DataAccess.EquipmentMaintenance
{
    public class RepairFormSubjectDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var query = (from x in db.RFormSubject
                                 where x.AncestorOrganizationUniqueID == Parameters.AncestorOrganizationUniqueID
                                 select new
                                 {
                                     x.UniqueID,
                                     x.ID,
                                     x.Description
                                 }).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.ID.Contains(Parameters.Keyword) || x.Description.Contains(Parameters.Keyword));
                    }

                    result.ReturnData(new GridViewModel()
                    {
                        AncestorOrganizationUniqueID = Parameters.AncestorOrganizationUniqueID,
                        AncestorOrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(Parameters.AncestorOrganizationUniqueID),
                        ItemList = query.ToList().Select(x => new GridItem()
                        {
                            UniqueID = x.UniqueID,
                            ID=x.ID,
                            Description = x.Description
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
                    var subject = db.RFormSubject.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = subject.UniqueID,
                        AncestorOrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(subject.AncestorOrganizationUniqueID),
                        ID = subject.ID,
                        Description = subject.Description
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
                    var exists = db.RFormSubject.FirstOrDefault(x => x.AncestorOrganizationUniqueID == Model.AncestorOrganizationUniqueID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
                        string uniqueID = Guid.NewGuid().ToString();

                        db.RFormSubject.Add(new RFormSubject()
                        {
                            UniqueID = uniqueID,
                            AncestorOrganizationUniqueID = Model.AncestorOrganizationUniqueID,
                            ID = Model.FormInput.ID,
                            Description = Model.FormInput.Description
                        });

                        db.SaveChanges();

                        result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.RepairFormSubject, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.RepairFormSubjectID, Resources.Resource.Exists));
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
                    var subject = db.RFormSubject.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = subject.UniqueID,
                        AncestorOrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(subject.AncestorOrganizationUniqueID),
                        FormInput = new FormInput()
                        {
                            ID=subject.ID,
                            Description = subject.Description
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
                using (EDbEntities db = new EDbEntities())
                {
                    var subject = db.RFormSubject.First(x => x.UniqueID == Model.UniqueID);

                    var exists = db.RFormSubject.FirstOrDefault(x => x.UniqueID != subject.UniqueID && x.AncestorOrganizationUniqueID == subject.AncestorOrganizationUniqueID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
                        subject.ID = Model.FormInput.ID;
                        subject.Description = Model.FormInput.Description;

                        db.SaveChanges();

                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.RepairFormSubject, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.RepairFormSubjectID, Resources.Resource.Exists));
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
                    DeleteHelper.RepairFormSubject(db, SelectedList);

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.RepairFormSubject, Resources.Resource.Success));
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
                    { Define.EnumTreeAttribute.RepairFormSubjectUniqueID, string.Empty }
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
                            attributes[Define.EnumTreeAttribute.RepairFormSubjectUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (edb.RFormSubject.Any(x => x.AncestorOrganizationUniqueID == organization.UniqueID))
                            {
                                treeItem.State = "closed";
                            }

                            treeItemList.Add(treeItem);
                        }
                    }
                    else
                    {
                        var subjectList = edb.RFormSubject.Where(x => x.AncestorOrganizationUniqueID == AncestorOrganizationUniqueID).OrderBy(x => x.ID).ToList();

                        foreach (var subject in subjectList)
                        {
                            var treeItem = new TreeItem() { Title = subject.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.RepairFormSubject.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", subject.ID, subject.Description);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = subject.AncestorOrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.RepairFormSubjectUniqueID] = subject.UniqueID;

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
