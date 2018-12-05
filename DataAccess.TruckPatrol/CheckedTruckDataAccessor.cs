using DbEntity.MSSQL;
using DbEntity.MSSQL.TruckPatrol;
using Models.Authenticated;
using Models.TruckPatrol.CheckedTruckManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace DataAccess.TruckPatrol
{
    public class CheckedTruckDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = new List<TruckBindingResultModel>();

                var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                var organizationList = Account.QueryableOrganizationUniqueIDList.Intersect(downStreamOrganizationList);

                using (DbEntities db = new DbEntities())
                {
                    var userList = db.User.Where(x => organizationList.Contains(x.OrganizationUniqueID)).ToList();

                    using (TDbEntities tdb = new TDbEntities())
                    {
                        var query = tdb.ArriveRecord.Where(x => string.Compare(x.ArriveDate, Parameters.BeginDate) >= 0 && string.Compare(x.ArriveDate, Parameters.EndDate) <= 0).AsQueryable();

                        if (!string.IsNullOrEmpty(Parameters.TruckUniqueID))
                        {
                            query = query.Where(x => x.TruckUniqueID == Parameters.TruckUniqueID);
                        }

                        var tmpQueryResults  = query.ToList();

                        var queryResults = (from a in tmpQueryResults
                                            join u in userList
                                             on a.UserID equals u.ID
                                            where organizationList.Contains(u.OrganizationUniqueID)
                                            select a.TruckBindingUniqueID).Distinct().ToList();

                        foreach (var q in queryResults)
                        {
                            var truckBindingResult = tdb.TruckBindingResult.FirstOrDefault(x => x.UniqueID == q);

                            if (truckBindingResult == null)
                            {
                                TruckBindingResultHelper.Insert(q);

                                truckBindingResult = tdb.TruckBindingResult.First(x => x.UniqueID == q);
                            }

                            bool keyStatus = true;

                            if (!string.IsNullOrEmpty(Parameters.KeyStatus))
                            {
                                if (Parameters.KeyStatus == "0" && truckBindingResult.KeyTime.HasValue)
                                {
                                    keyStatus = false;
                                }

                                if (Parameters.KeyStatus == "1" && !truckBindingResult.KeyTime.HasValue)
                                {
                                    keyStatus = false;
                                }
                            }

                            if (keyStatus)
                            {
                                itemList.Add(new TruckBindingResultModel
                                {
                                    BindingUniqueID = truckBindingResult.UniqueID,
                                    CheckDate = truckBindingResult.CheckDate,
                                    CheckUser = truckBindingResult.CheckUser,
                                    CompleteRate = truckBindingResult.CompleteRate,
                                    FirstTruckNo = truckBindingResult.FirstTruckNo,
                                    FirstTruckUniqueID = truckBindingResult.FirstTruckUniqueID,
                                    HaveAbnormal = truckBindingResult.IsAbnormal,
                                    HaveAlert = truckBindingResult.IsAlert,
                                    LabelClass = truckBindingResult.CompleteRateLabelClass,
                                    OrganizationDescription = truckBindingResult.OrganizationDescription,
                                    OrganizationUniqueID = truckBindingResult.OrganizationUniqueID,
                                    SecondTruckNo = truckBindingResult.SecondTruckNo,
                                    SecondTruckUniqueID = truckBindingResult.SecondTruckUniqueID,
                                    TimeSpan = truckBindingResult.TimeSpan,
                                    KeyTime = truckBindingResult.KeyTime,
                                    KeyUser = UserDataAccessor.GetUser(truckBindingResult.KeyUserID)
                                });
                            }
                        }
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

        public static RequestResult Key(string TruckBindingUniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (TDbEntities db = new TDbEntities())
                {
                    var query = db.TruckBindingResult.First(x => x.UniqueID == TruckBindingUniqueID);

                    query.KeyUserID = Account.ID;
                    query.KeyTime = DateTime.Now;

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage("發送鑰匙成功");
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult Export(List<TruckBindingResultModel> ItemList, Define.EnumExcelVersion ExcelVersion)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ExcelHelper helper = new ExcelHelper(string.Format("檢查完成清單({0})", DateTimeHelper.DateTime2DateTimeString(DateTime.Now)), ExcelVersion))
                {
                    helper.CreateSheet<TruckBindingResultModel>(ItemList);

                    result.ReturnData(helper.Export());
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
    }
}
