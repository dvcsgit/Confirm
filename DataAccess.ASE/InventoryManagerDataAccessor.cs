using DbEntity.ASE;
using Models.ASE.InventoryManagerManagement;
using Models.Authenticated;
using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace DataAccess.ASE
{
    public class InventoryManagerDataAccessor
    {
        public static RequestResult Query()
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = new List<GridItem>();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = (from x in db.INVENTORYMANAGER
                                 join o in db.ORGANIZATION
                                 on x.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                 join u in db.ACCOUNT
                                 on x.USERID equals u.ID
                                 select new
                                 {
                                     OrganizationUniqueID = x.ORGANIZATIONUNIQUEID,
                                     OrganizationID = o.ID,
                                     OrganizationDescription = o.DESCRIPTION,
                                     ManagerID = x.USERID,
                                     ManagerName = u.NAME
                                 }).ToList();

                    var temp = query.Select(x => new { x.OrganizationUniqueID, x.OrganizationID, x.OrganizationDescription }).Distinct().OrderBy(x => x.OrganizationID).ToList();

                    foreach (var t in temp)
                    {
                        var managerList = query.Where(x => x.OrganizationUniqueID == t.OrganizationUniqueID).Select(x => new { x.ManagerID, x.ManagerName }).OrderBy(x => x.ManagerID).ToList();

                        itemList.Add(new GridItem()
                        {
                            OrganizationUniqueID = t.OrganizationUniqueID,
                            OrganizationDescription = t.OrganizationDescription,
                            ManagerList = managerList.Select(x => string.Format("{0}/{1}", x.ManagerID, x.ManagerName)).ToList()
                        });
                    }
                            
                }

                result.ReturnData(itemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetDetailViewModel(string OrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new DetailViewModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = (from x in db.INVENTORYMANAGER
                                 join o in db.ORGANIZATION
                                 on x.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                 join u in db.ACCOUNT
                                 on x.USERID equals u.ID
                                 where x.ORGANIZATIONUNIQUEID == OrganizationUniqueID
                                 select new
                                 {
                                     OrganizationUniqueID = x.ORGANIZATIONUNIQUEID,
                                     OrganizationID = o.ID,
                                     OrganizationDescription = o.DESCRIPTION,
                                     ManagerID = x.USERID,
                                     ManagerName = u.NAME
                                 }).ToList();

                    model.OrganizationUniqueID = query.First().OrganizationUniqueID;
                    model.OrganizationDescription = query.First().OrganizationDescription;
                    
                    foreach (var q in query)
                    {
                        model.ManagerList.Add(string.Format("{0}/{1}", q.ManagerID, q.ManagerName));
                    }
                }

                result.ReturnData(model);
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
                    if (!db.INVENTORYMANAGER.Any(x => x.ORGANIZATIONUNIQUEID == Model.FormInput.OrganizationUniqueID))
                    {
                        foreach (var manager in Model.FormInput.ManagerList)
                        { 
                            db.INVENTORYMANAGER.Add(new INVENTORYMANAGER(){
                             ORGANIZATIONUNIQUEID = Model.FormInput.OrganizationUniqueID,
                              USERID = manager
                            });
                        }

                        db.SaveChanges();

                        result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Create, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage("組織資料已存在");
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

        public static RequestResult GetEditFormModel(string OrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new EditFormModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = (from x in db.INVENTORYMANAGER
                                 join o in db.ORGANIZATION
                                 on x.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                 join u in db.ACCOUNT
                                 on x.USERID equals u.ID
                                 where x.ORGANIZATIONUNIQUEID==OrganizationUniqueID
                                 select new
                                 {
                                     OrganizationUniqueID = x.ORGANIZATIONUNIQUEID,
                                     OrganizationID = o.ID,
                                     OrganizationDescription = o.DESCRIPTION,
                                     ManagerID = x.USERID,
                                     ManagerName = u.NAME
                                 }).ToList();

                    model.OrganizationDescription = query.First().OrganizationDescription;

                    model.FormInput.OrganizationUniqueID = query.First().OrganizationUniqueID;

                    foreach (var q in query)
                    {
                        model.ManagerList.Add(q.ManagerID); 
                    }
                }

                result.ReturnData(model);
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
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    { 
#endif

                    db.INVENTORYMANAGER.RemoveRange(db.INVENTORYMANAGER.Where(x => x.ORGANIZATIONUNIQUEID == Model.FormInput.OrganizationUniqueID).ToList());

                    db.SaveChanges();

                    foreach (var manager in Model.FormInput.ManagerList)
                    {
                        db.INVENTORYMANAGER.Add(new INVENTORYMANAGER()
                        {
                            ORGANIZATIONUNIQUEID = Model.FormInput.OrganizationUniqueID,
                            USERID = manager
                        });
                    }

                    db.SaveChanges();

#if !DEBUG
                        trans.Complete();
                    }
#endif
                }

                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Edit, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult Delete(string OrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    db.INVENTORYMANAGER.RemoveRange(db.INVENTORYMANAGER.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).ToList());

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Delete, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetUserOptions(List<Models.Shared.UserModel> UserList, string Term, bool IsInit)
        {
            RequestResult result = new RequestResult();

            try
            {
                var query = UserList.Select(x => new Models.ASE.Shared.ASEUserModel
                {
                    ID = x.ID,
                    Name = x.Name,
                    Email = x.Email,
                    OrganizationDescription = x.OrganizationDescription
                }).ToList();

                if (!string.IsNullOrEmpty(Term))
                {
                    if (IsInit)
                    {
                        query = query.Where(x => x.ID == Term).ToList();
                    }
                    else
                    {
                        var term = Term.ToLower();

                        query = query.Where(x => x.Term.Contains(term)).ToList();
                    }
                }

                result.ReturnData(query.Select(x => new SelectListItem { Value = x.ID, Text = x.Display }).Distinct().ToList());
            }
            catch (Exception ex)
            {
                Error err = new Error(MethodInfo.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetOrganizationOptions(List<Organization> OrganizationList, string Term, bool IsInit)
        {
            RequestResult result = new RequestResult();

            try
            {
                var query = OrganizationList.AsQueryable();

                if (!string.IsNullOrEmpty(Term))
                {
                    if (IsInit)
                    {
                        query = query.Where(x => x.UniqueID == Term);
                    }
                    else
                    {
                        var term = Term.ToLower();

                        query = query.Where(x => x.ID.Contains(term) || x.Description.Contains(term));
                    }
                }

                result.ReturnData(query.Select(x => new SelectListItem { Value = x.UniqueID, Text = string.Format("{0}/{1}", x.ID, x.Description) }).Distinct().ToList());
            }
            catch (Exception ex)
            {
                Error err = new Error(MethodInfo.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }
    }
}
