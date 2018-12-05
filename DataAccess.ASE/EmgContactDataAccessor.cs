using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using Models.Shared;
using Models.Authenticated;
using Models.EmgContactManagement;
using DbEntity.ASE;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.ASE
{
    public class EmgContactDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = (from e in db.EMGCONTACT
                                 join o in db.ORGANIZATION
                                 on e.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                 join u in db.ACCOUNT
                                 on e.USERID equals u.ID into tmpUser
                                 from u in tmpUser.DefaultIfEmpty()
                                 where downStreamOrganizationList.Contains(e.ORGANIZATIONUNIQUEID) && Account.QueryableOrganizationUniqueIDList.Contains(e.ORGANIZATIONUNIQUEID)
                                 select new
                                 {
                                     e.UNIQUEID,
                                     e.ORGANIZATIONUNIQUEID,
                                     OrganizationDescription = o.DESCRIPTION,
                                     Title = u != null ? u.TITLE : e.TITLE,
                                     Name = u != null ? u.NAME : e.NAME
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
                            UniqueID = x.UNIQUEID,
                            Permission = Account.OrganizationPermission(x.ORGANIZATIONUNIQUEID),
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var emgContact = (from e in db.EMGCONTACT
                                      join u in db.ACCOUNT
                                      on e.USERID equals u.ID into tmpUser
                                      from u in tmpUser.DefaultIfEmpty()
                                      where e.UNIQUEID == UniqueID
                                      select new
                                      {
                                          e.UNIQUEID,
                                          e.ORGANIZATIONUNIQUEID,
                                          Title = u != null ? u.TITLE : e.TITLE,
                                          Name = u != null ? u.NAME : e.NAME
                                      }).First();

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = emgContact.UNIQUEID,
                        Permission = Account.OrganizationPermission(emgContact.ORGANIZATIONUNIQUEID),
                        OrganizationUniqueID = emgContact.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(emgContact.ORGANIZATIONUNIQUEID),
                        Title = emgContact.Title,
                        Name = emgContact.Name,
                        TelList = db.EMGCONTACTTEL.Where(x => x.EMGCONTACTUNIQUEID == emgContact.UNIQUEID).OrderBy(x => x.SEQ).Select(x => x.TEL).ToList()
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
                    var exists = db.EMGCONTACT.FirstOrDefault(x => x.ORGANIZATIONUNIQUEID == Model.OrganizationUniqueID && !string.IsNullOrEmpty(x.USERID) && x.USERID == Model.FormInput.UserID);

                    if (exists == null)
                    {
                        var uniqueID = Guid.NewGuid().ToString();

                        db.EMGCONTACT.Add(new EMGCONTACT()
                        {
                            UNIQUEID = uniqueID,
                            ORGANIZATIONUNIQUEID = Model.OrganizationUniqueID,
                            USERID = Model.FormInput.UserID,
                            TITLE = Model.FormInput.Title,
                            NAME = Model.FormInput.Name,
                            LASTMODIFYTIME = DateTime.Now
                        });

                        int seq = 1;

                        foreach (var tel in Model.FormInput.TelList)
                        {
                            db.EMGCONTACTTEL.Add(new EMGCONTACTTEL()
                            {
                                EMGCONTACTUNIQUEID = uniqueID,
                                SEQ = seq,
                                TEL = tel
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var emgContact = (from e in db.EMGCONTACT
                                      join u in db.ACCOUNT
                                      on e.USERID equals u.ID into tmpUser
                                      from u in tmpUser.DefaultIfEmpty()
                                      where e.UNIQUEID == UniqueID
                                      select new
                                      {
                                          e.UNIQUEID,
                                          e.ORGANIZATIONUNIQUEID,
                                          e.USERID,
                                          UserName = u != null ? u.NAME : string.Empty,
                                          e.TITLE,
                                          e.NAME
                                      }).First();

                    var organization = OrganizationDataAccessor.GetOrganization(emgContact.ORGANIZATIONUNIQUEID);

                    result.ReturnData(new EditFormModel()
                    {
                        UniqueID = emgContact.UNIQUEID,
                        AncestorOrganizationUniqueID = organization.AncestorOrganizationUniqueID,
                        OrganizationUniqueID = emgContact.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = organization.FullDescription,
                        UserName = emgContact.UserName,
                        TelList = db.EMGCONTACTTEL.Where(x => x.EMGCONTACTUNIQUEID == UniqueID).OrderBy(x => x.SEQ).Select(x => x.TEL).ToList(),
                        FormInput = new FormInput()
                        {
                            UserID = emgContact.USERID,
                            Name = emgContact.NAME,
                            Title = emgContact.TITLE
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var exists = db.EMGCONTACT.FirstOrDefault(x => x.UNIQUEID != Model.UniqueID && x.ORGANIZATIONUNIQUEID == Model.OrganizationUniqueID && !string.IsNullOrEmpty(x.USERID) && x.USERID == Model.FormInput.UserID);

                    if (exists == null)
                    {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                        #region EmgContact
                        var emgContact = db.EMGCONTACT.First(x => x.UNIQUEID == Model.UniqueID);

                        emgContact.USERID = Model.FormInput.UserID;
                        emgContact.NAME = Model.FormInput.Name;
                        emgContact.TITLE = Model.FormInput.Title;
                        emgContact.LASTMODIFYTIME = DateTime.Now;

                        db.SaveChanges();
                        #endregion

                        #region EmgContactTel
                        #region Delete
                        db.EMGCONTACTTEL.RemoveRange(db.EMGCONTACTTEL.Where(x => x.EMGCONTACTUNIQUEID == emgContact.UNIQUEID).ToList());

                        db.SaveChanges();
                        #endregion

                        #region Insert
                        int seq = 1;

                        foreach (var tel in Model.FormInput.TelList)
                        {
                            db.EMGCONTACTTEL.Add(new EMGCONTACTTEL()
                            {
                                EMGCONTACTUNIQUEID = emgContact.UNIQUEID,
                                SEQ = seq,
                                TEL = tel
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    DeleteHelper.EmgContact(db, SelectedList);

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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                    {
                        var emgContactList = (from e in db.EMGCONTACT
                                              join u in db.ACCOUNT
                                              on e.USERID equals u.ID into tmpUser
                                              from u in tmpUser.DefaultIfEmpty()
                                              where e.ORGANIZATIONUNIQUEID == OrganizationUniqueID
                                              select new
                                              {
                                                  e.UNIQUEID,
                                                  Title = u != null ? u.TITLE : e.TITLE,
                                                  Name = u != null ? u.NAME : e.NAME
                                              }).OrderBy(x => x.Title).ThenBy(x => x.Name).ToList();

                        foreach (var emgContact in emgContactList)
                        {
                            var treeItem = new TreeItem() { Title = emgContact.Name };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.EmgContact.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", emgContact.Title, emgContact.Name);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.EmgContactUniqueID] = emgContact.UNIQUEID;

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

                        var haveEmgContact = Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.EMGCONTACT.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID);

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
