using DbEntity.MSSQL;
using DbEntity.MSSQL.PipelinePatrol;
using Models.Authenticated;
using Models.PipelinePatrol.JobManagement;
using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;
using System.Transactions;

namespace DataAccess.PipelinePatrol
{
    public class JobDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.Job.Where(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.ID.Contains(Parameters.Keyword) || x.Description.Contains(Parameters.Keyword));
                    }

                    var organization = OrganizationDataAccessor.GetOrganization(Parameters.OrganizationUniqueID);

                    result.ReturnData(new GridViewModel()
                    {
                        OrganizationUniqueID = Parameters.OrganizationUniqueID,
                        Permission = Account.OrganizationPermission(Parameters.OrganizationUniqueID),
                        OrganizationDescription = organization.Description,
                        FullOrganizationDescription = organization.FullDescription,
                        ItemList = query.ToList().Select(x => new GridItem()
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
                using (PDbEntities db = new PDbEntities())
                {
                    var job = db.Job.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new DetailViewModel()
                    {
                        UniqueID = job.UniqueID,
                        Permission = Account.OrganizationPermission(job.OrganizationUniqueID),
                        OrganizationUniqueID = job.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(job.OrganizationUniqueID),
                        ID = job.ID,
                        Description = job.Description,
                        RouteList = (from x in db.JobRoute
                                     join r in db.Route
                                     on x.RouteUniqueID equals r.UniqueID
                                     where x.JobUniqueID == job.UniqueID
                                     select new RouteModel
                                     {
                                         UniqueID = r.UniqueID,
                                         ID = r.ID,
                                         Name = r.Name
                                     }).OrderBy(x => x.ID).ToList()
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
                using (PDbEntities db = new PDbEntities())
                {
                    var job = db.Job.First(x => x.UniqueID == UniqueID);

                    result.ReturnData(new CreateFormModel()
                    {
                        OrganizationUniqueID = job.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(job.OrganizationUniqueID),
                        RouteList = (from x in db.JobRoute
                                    join r in db.Route
                                    on x.RouteUniqueID equals r.UniqueID
                                    where x.JobUniqueID == job.UniqueID
                                    select new RouteModel {
                                        UniqueID = r.UniqueID,
                                        ID=r.ID,
                                        Name=r.Name
                                    }).OrderBy(x=>x.ID).ToList()
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
                using (PDbEntities db = new PDbEntities())
                {
                    var exists = db.Job.FirstOrDefault(x => x.OrganizationUniqueID == Model.OrganizationUniqueID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
                        string uniqueID = Guid.NewGuid().ToString();

                        db.Job.Add(new Job()
                        {
                            UniqueID = uniqueID,
                            OrganizationUniqueID = Model.OrganizationUniqueID,
                            ID = Model.FormInput.ID,
                            Description = Model.FormInput.Description,
 IsCheckBySeq=false,
 IsNeedVerify=false,
  IsShowPrevRecord=false,
                            LastModifyTime = DateTime.Now
                        });

                        db.JobRoute.AddRange(Model.RouteList.Select(x => new JobRoute { 
                         JobUniqueID = uniqueID,
                          RouteUniqueID=x.UniqueID
                        }).ToList());

                        db.SaveChanges();

                        result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.Job, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.JobID, Resources.Resource.Exists));
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
                using (PDbEntities db = new PDbEntities())
                {
                    var job = db.Job.First(x => x.UniqueID == UniqueID);

                    var model = new EditFormModel()
                    {
                        UniqueID = job.UniqueID,
                        OrganizationUniqueID = job.OrganizationUniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(job.OrganizationUniqueID),
                        RouteList = (from x in db.JobRoute
                                    join r in db.Route
                                    on x.RouteUniqueID equals r.UniqueID
                                    where x.JobUniqueID == job.UniqueID
                                    select new RouteModel {
                                        UniqueID = r.UniqueID,
                                        ID=r.ID,
                                        Name=r.Name
                                    }).OrderBy(x=>x.ID).ToList(),
                        FormInput = new FormInput()
                        {
                            ID = job.ID,
                            Description = job.Description
                        }
                    };

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
                using (PDbEntities db = new PDbEntities())
                {
                    var job = db.Job.First(x => x.UniqueID == Model.UniqueID);

                    var exists = db.Job.FirstOrDefault(x => x.UniqueID != job.UniqueID && x.OrganizationUniqueID == job.OrganizationUniqueID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                        #region Job

                        job.ID = Model.FormInput.ID;
                        job.Description = Model.FormInput.Description;
                        job.LastModifyTime = DateTime.Now;

                        db.SaveChanges();
                        #endregion

                        #region JobRoute
                        #region Delete
                        db.JobRoute.RemoveRange(db.JobRoute.Where(x => x.JobUniqueID == Model.UniqueID).ToList());

                        db.SaveChanges();
                        #endregion

                        #region Insert
                        db.JobRoute.AddRange(Model.RouteList.Select(x => new JobRoute
                        {
                            JobUniqueID = job.UniqueID,
                            RouteUniqueID = x.UniqueID
                        }).ToList());

                        db.SaveChanges();
                        #endregion
                        #endregion


#if !DEBUG
                        trans.Complete();
                    }
#endif

                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.Job, Resources.Resource.Success));
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.JobID, Resources.Resource.Exists));
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

        //public static RequestResult Delete(List<string> SelectedList)
        //{
        //    RequestResult result = new RequestResult();

        //    try
        //    {
        //        using (PDbEntities db = new PDbEntities())
        //        {
        //            foreach (var uniqueID in SelectedList)
        //            {
        //                //CheckItem
        //                db.CheckItem.Remove(db.CheckItem.First(x => x.UniqueID == uniqueID));

        //                //CheckItemAbnormalReason
        //                db.CheckItemAbnormalReason.RemoveRange(db.CheckItemAbnormalReason.Where(x => x.CheckItemUniqueID == uniqueID).ToList());

        //                //CheckItemFeelOption
        //                db.CheckItemFeelOption.RemoveRange(db.CheckItemFeelOption.Where(x => x.CheckItemUniqueID == uniqueID).ToList());

        //                //ControlPointCheckItem
        //                db.PipePointCheckItem.RemoveRange(db.PipePointCheckItem.Where(x => x.CheckItemUniqueID == uniqueID).ToList());

        //                //RouteControlPointCheckItem
        //                db.RouteCheckPointCheckItem.RemoveRange(db.RouteCheckPointCheckItem.Where(x => x.CheckItemUniqueID == uniqueID).ToList());
        //            }

        //            db.SaveChanges();
        //        }

        //        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.CheckItem, Resources.Resource.Success));
        //    }
        //    catch (Exception ex)
        //    {
        //        var err = new Error(MethodBase.GetCurrentMethod(), ex);

        //        Logger.Log(err);

        //        result.ReturnError(err);
        //    }

        //    return result;
        //}

        public static RequestResult AddRoute(List<RouteModel> RouteList, List<string> SelectedList, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (PDbEntities db = new PDbEntities())
                {
                    foreach (string selected in SelectedList)
                    {
                        string[] temp = selected.Split(Define.Seperators, StringSplitOptions.None);

                        var organizationUniqueID = temp[0];
                        var routeUniqueID = temp[1];

                        if (!string.IsNullOrEmpty(routeUniqueID))
                        {
                            if (!RouteList.Any(x => x.UniqueID == routeUniqueID))
                            {
                                var route = db.Route.First(x => x.UniqueID == routeUniqueID);

                                RouteList.Add(new RouteModel   ()
                                {
                                    UniqueID = route.UniqueID,
                                    ID = route.ID,
                                    Name = route.Name
                                });
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
                                    var routeList = db.Route.Where(x => x.OrganizationUniqueID == downStreamOrganization).ToList();

                                    foreach (var route in routeList)
                                    {
                                        if (!RouteList.Any(x => x.UniqueID == route.UniqueID))
                                        {
                                            RouteList.Add(new RouteModel()
                                            {
                                                UniqueID = route.UniqueID,
                                                ID = route.ID,
                                                Name = route.Name
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                result.ReturnData(RouteList.OrderBy(x => x.ID).ToList());
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
                    { Define.EnumTreeAttribute.JobUniqueID, string.Empty }
                };

                using (PDbEntities edb = new PDbEntities())
                {
                    if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                    {
                        var jobList = edb.Job.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                        foreach (var job in jobList)
                        {
                            var treeItem = new TreeItem() { Title = job.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Job.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", job.ID,job.Description);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.JobUniqueID] = job.UniqueID;

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
                            attributes[Define.EnumTreeAttribute.JobUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (db.Organization.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                                ||
                                (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && edb.Job.Any(x => x.OrganizationUniqueID == organization.UniqueID)))
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
    }
}
