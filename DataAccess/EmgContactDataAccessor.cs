using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using Models.Shared;
using Models.Authenticated;
using Models.EmgContactManagement;
using DbEntity.MSSQL;
#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess
{
    public class EmgContactDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (DbEntities db = new DbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = (from e in db.EmgContact
                                 join o in db.Organization
                                 on e.OrganizationUniqueID equals o.UniqueID
                                 join u in db.User
                                 on e.UserID equals u.ID into tmpUser
                                 from u in tmpUser.DefaultIfEmpty()
                                 where downStreamOrganizationList.Contains(e.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(e.OrganizationUniqueID)
                                 select new
                                 {
                                     e.UniqueID,
                                     e.OrganizationUniqueID,
                                     OrganizationDescription = o.Description,
                                     Title = u != null ? u.Title : e.Title,
                                     Name = u != null ? u.Name : e.Name
                                 }).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.Title.Contains(Parameters.Keyword) || x.Name.Contains(Parameters.Keyword));
                    }

                    var organization = OrganizationDataAccessor.GetOrganization(Parameters.OrganizationUniqueID);

                    var itemList = query.ToList();

                    result.ReturnData(new GridViewModel()
                    {
                        Permission = Account.OrganizationPermission(Parameters.OrganizationUniqueID),
                        OrganizationUniqueID = Parameters.OrganizationUniqueID,
                        OrganizationDescription = organization.Description,
                        FullOrganizationDescription = organization.FullDescription,
                        ItemList = itemList.Select(x => new GridItem()
                        {
                            UniqueID = x.UniqueID,
                            Permission = Account.OrganizationPermission(x.OrganizationUniqueID),
                            OrganizationDescription = x.OrganizationDescription,
                            Title = x.Title,
                            Name = x.Name
                        }).ToList()
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
                using (DbEntities db = new DbEntities())
                {
                    var emgContact = (from e in db.EmgContact
                                      join u in db.User
                                      on e.UserID equals u.ID into tmpUser
                                      from u in tmpUser.DefaultIfEmpty()
                                      where e.UniqueID == UniqueID
                                      select new
                                      {
                                          e.UniqueID,
                                          e.OrganizationUniqueID,
                                          Title = u != null ? u.Title : e.Title,
                                          Name = u != null ? u.Name : e.Name
                                      }).First();

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = emgContact.UniqueID,
                        Permission = Account.OrganizationPermission(emgContact.OrganizationUniqueID),
                        OrganizationUniqueID = emgContact.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(emgContact.OrganizationUniqueID),
                        Title = emgContact.Title,
                        Name = emgContact.Name,
                        TelList = db.EmgContactTel.Where(x => x.EmgContactUniqueID == emgContact.UniqueID).OrderBy(x => x.Seq).Select(x => x.Tel).ToList()
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
                using (DbEntities db = new DbEntities())
                {
                    var exists = db.EmgContact.FirstOrDefault(x => x.OrganizationUniqueID == Model.OrganizationUniqueID && !string.IsNullOrEmpty(x.UserID) && x.UserID == Model.FormInput.UserID);

                    if (exists == null)
                    {
                        var uniqueID = Guid.NewGuid().ToString();

                        db.EmgContact.Add(new EmgContact()
                        {
                            UniqueID = uniqueID,
                            OrganizationUniqueID = Model.OrganizationUniqueID,
                            UserID = Model.FormInput.UserID,
                            Title = Model.FormInput.Title,
                            Name = Model.FormInput.Name,
                            LastModifyTime = DateTime.Now
                        });

                        int seq = 1;

                        foreach (var tel in Model.FormInput.TelList)
                        {
                            db.EmgContactTel.Add(new EmgContactTel()
                            {
                                EmgContactUniqueID = uniqueID,
                                Seq = seq,
                                Tel = tel
                            });

                            seq++;
                        }

                        db.SaveChanges();

                        result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.EmgContact, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.EmgContact, Resources.Resource.Exists));
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
                using (DbEntities db = new DbEntities())
                {
                    var emgContact = (from e in db.EmgContact
                                      join u in db.User
                                      on e.UserID equals u.ID into tmpUser
                                      from u in tmpUser.DefaultIfEmpty()
                                      where e.UniqueID == UniqueID
                                      select new
                                      {
                                          e.UniqueID,
                                          e.OrganizationUniqueID,
                                          e.UserID,
                                          UserName = u != null ? u.Name : string.Empty,
                                          e.Title,
                                          e.Name
                                      }).First();

                    var organization = OrganizationDataAccessor.GetOrganization(emgContact.OrganizationUniqueID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = emgContact.UniqueID,
                        AncestorOrganizationUniqueID = organization.AncestorOrganizationUniqueID,
                        OrganizationUniqueID = emgContact.OrganizationUniqueID,
                        ParentOrganizationFullDescription = organization.FullDescription,
                        UserName = emgContact.UserName,
                        TelList = db.EmgContactTel.Where(x => x.EmgContactUniqueID == UniqueID).OrderBy(x => x.Seq).Select(x => x.Tel).ToList(),
                        FormInput = new FormInput()
                        {
                            UserID = emgContact.UserID,
                            Name = emgContact.Name,
                            Title = emgContact.Title
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
                using (DbEntities db = new DbEntities())
                {
                    var exists = db.EmgContact.FirstOrDefault(x => x.UniqueID != Model.UniqueID && x.OrganizationUniqueID == Model.OrganizationUniqueID && !string.IsNullOrEmpty(x.UserID) && x.UserID == Model.FormInput.UserID);

                    if (exists == null)
                    {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                        #region EmgContact
                        var emgContact = db.EmgContact.First(x => x.UniqueID == Model.UniqueID);

                        emgContact.UserID = Model.FormInput.UserID;
                        emgContact.Name = Model.FormInput.Name;
                        emgContact.Title = Model.FormInput.Title;
                        emgContact.LastModifyTime = DateTime.Now;

                        db.SaveChanges();
                        #endregion

                        #region EmgContactTel
                        #region Delete
                        db.EmgContactTel.RemoveRange(db.EmgContactTel.Where(x => x.EmgContactUniqueID == emgContact.UniqueID).ToList());

                        db.SaveChanges();
                        #endregion

                        #region Insert
                        int seq = 1;

                        foreach (var tel in Model.FormInput.TelList)
                        {
                            db.EmgContactTel.Add(new EmgContactTel()
                            {
                                EmgContactUniqueID = emgContact.UniqueID,
                                Seq = seq,
                                Tel = tel
                            });

                            seq++;
                        }

                        db.SaveChanges();
                        #endregion
                        #endregion
#if !DEBUG
                        trans.Complete();
                    }
#endif
                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.EmgContact, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.EmgContact, Resources.Resource.Exists));
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
                using (DbEntities db = new DbEntities())
                {
                    foreach (var emgContact in SelectedList)
                    {
                        db.EmgContact.Remove(db.EmgContact.First(x => x.UniqueID == emgContact));

                        db.EmgContactTel.RemoveRange(db.EmgContactTel.Where(x => x.EmgContactUniqueID == emgContact).ToList());
                    }

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.EmgContact, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetTreeItem(List<Models.Shared.Organization> OrganizationList, string OrganizationUniqueID, Account Account)
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
                    { Define.EnumTreeAttribute.EmgContactUniqueID, string.Empty }
                };

                using (DbEntities db = new DbEntities())
                {
                    if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                    {
                        var emgContactList = (from e in db.EmgContact
                                              join u in db.User
                                              on e.UserID equals u.ID into tmpUser
                                              from u in tmpUser.DefaultIfEmpty()
                                              where e.OrganizationUniqueID == OrganizationUniqueID
                                              select new
                                              {
                                                  e.UniqueID,
                                                  Title = u != null ? u.Title : e.Title,
                                                  Name = u != null ? u.Name : e.Name
                                              }).OrderBy(x => x.Title).ThenBy(x => x.Name).ToList();

                        foreach (var emgContact in emgContactList)
                        {
                            var treeItem = new TreeItem() { Title = emgContact.Name };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.EmgContact.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", emgContact.Title, emgContact.Name);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.EmgContactUniqueID] = emgContact.UniqueID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

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
                        attributes[Define.EnumTreeAttribute.EmgContactUniqueID] = string.Empty;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        var haveDownStreamOrganization = OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID));

                        var haveEmgContact = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.EmgContact.Any(x => x.OrganizationUniqueID == organization.UniqueID);

                        if (haveDownStreamOrganization || haveEmgContact)
                        {
                            treeItem.State = "closed";
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
    }
}
