//using DbEntity.ASE;
//using Models.Authenticated;
//using Models.EquipmentMaintenance.NoMJobManagement;
//using Utility.Models;
//using System.Linq;
//using System;
//using System.Reflection;
//using Utility;

namespace DataAccess.ASE
{
    //public class NoMJobDataAccessor
    //{
    //    public static RequestResult Query(QueryParameters Parameters, Account Account)
    //    {
    //        RequestResult result = new RequestResult();

    //        try
    //        {
    //            using (ASEDbEntities db = new ASEDbEntities())
    //            {
    //                var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.ORGANIZATIONUNIQUEID, true);

    //                var query = db.NoMJobForm.Where(x => downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID) && Account.AvailableOrganizationUniqueIDList.Contains(x.ORGANIZATIONUNIQUEID)).AsQueryable();

    //                var model = new GridViewModel()
    //                {
    //                    OrganizationUniqueID = Parameters.ORGANIZATIONUNIQUEID,
    //                    OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(Parameters.ORGANIZATIONUNIQUEID),
    //                    FullOrganizationDescription = OrganizationDataAccessor.GetOrganizationFullDescription(Parameters.ORGANIZATIONUNIQUEID),
    //                    ItemList = query.ToList().Select(x => new GridItem()
    //                    {
    //                        UniqueID = x.UNIQUEID,
    //                        OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.ORGANIZATIONUNIQUEID),
    //                        CreateTime = x.CreateTime,
    //                        CreateUserID = x.CreateUserID,
    //                        ReasonID = x.ReasonID,
    //                        ReasonRemark = x.ReasonRemark
    //                    }).OrderBy(x => x.OrganizationDescription).ThenByDescending(x => x.CreateTime).ToList()
    //                };

    //                foreach (var item in model.ItemList)
    //                {
    //                    var createUser = UserDataAccessor.GetUser(item.CreateUserID);

    //                    if (createUser != null)
    //                    {
    //                        item.CreateUserName = createUser.Name;
    //                    }

    //                    var flow = db.NoMJobFormFlow.FirstOrDefault(x => x.FormUniqueID == item.UNIQUEID);

    //                    bool canVerify = false;

    //                    string nextVerifyUserID = string.Empty;
    //                    string nextVerifyUserName = string.Empty;

    //                    if (flow != null)
    //                    {
    //                        if (!flow.IsClosed)
    //                        {
    //                            canVerify = flow.NextUserID == Account.ID;

    //                            if (!canVerify)
    //                            {
    //                                nextVerifyUserID = flow.NextUserID;

    //                                var user = UserDataAccessor.GetUser(flow.NextUserID);

    //                                if (user != null)
    //                                {
    //                                    nextVerifyUserName = user.Name;
    //                                }
    //                            }
    //                        }
    //                    }
    //                }

    //                result.ReturnData(model);
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            var err = new Error(MethodBase.GetCurrentMethod(), ex);

    //            Logger.Log(err);

    //            result.ReturnError(err);
    //        }

    //        return result;
    //    }

    //    public static RequestResult GetCreateFormModel(string OrganizationUniqueID)
    //    {
    //        RequestResult result = new RequestResult();

    //        try
    //        {
    //            using (ASEDbEntities db = new ASEDbEntities())
    //            {
    //                result.ReturnData(new CreateFormModel()
    //                {
    //                    OrganizationUniqueID = OrganizationUniqueID,
    //                    ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(OrganizationUniqueID),
    //                    EquipmentList = db.EQUIPMENT.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID && !x.IsNoMJob).OrderBy(x => x.ID).ToList()
    //                });
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            var err = new Error(MethodBase.GetCurrentMethod(), ex);

    //            Logger.Log(err);

    //            result.ReturnError(err);
    //        }

    //        return result;
    //    }
    //}
}
