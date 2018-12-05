using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.Authenticated;
using Models.EquipmentMaintenance.AbnormalHandlingManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace DataAccess.EquipmentMaintenance
{
    public class AbnormalHandlingDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new GridViewModel();

                using (EDbEntities edb = new EDbEntities())
                {
                    if (!string.IsNullOrEmpty(Parameters.Status))
                    {
                        var today = DateTimeHelper.DateTime2DateString(DateTime.Today);

                        if (Parameters.Status == "1" || Parameters.Status == "2" || Parameters.Status == "3" || Parameters.Status == "4")
                        {
                            var query1 = (from abnormal in edb.Abnormal
                                          join x in edb.AbnormalCheckResult
                                          on abnormal.UniqueID equals x.AbnormalUniqueID
                                          join c in edb.CheckResult
                                          on x.CheckResultUniqueID equals c.UniqueID
                                          join y in edb.RouteManager
                                          on c.RouteUniqueID equals y.RouteUniqueID
                                          join a in edb.ArriveRecord
                                          on c.ArriveRecordUniqueID equals a.UniqueID
                                          where y.UserID == Account.ID
                                          select new
                                          {
                                              abnormal.UniqueID,
                                              c.RouteUniqueID,
                                              c.ControlPointID,
                                              c.ControlPointDescription,
                                              c.EquipmentUniqueID,
                                              c.EquipmentID,
                                              c.EquipmentName,
                                              c.PartDescription,
                                              c.CheckItemID,
                                              c.CheckItemDescription,
                                              c.CheckDate,
                                              c.CheckTime,
                                              c.IsAbnormal,
                                              c.IsAlert,
                                              abnormal.ClosedTime,
                                              abnormal.ClosedUserID,
                                              CheckUserID = a.UserID
                                          }).Distinct().AsQueryable();

                            var query2 = (from a in edb.Abnormal
                                          join x in edb.AbnormalMFormStandardResult
                                          on a.UniqueID equals x.AbnormalUniqueID
                                          join r in edb.MFormStandardResult
                                          on x.MFormStandardResultUniqueID equals r.UniqueID
                                          join y in edb.MFormResult
                                          on r.ResultUniqueID equals y.UniqueID
                                          where y.UserID == Account.ID
                                          select new
                                          {
                                              a.UniqueID,
                                              r.EquipmentUniqueID,
                                              r.EquipmentID,
                                              r.EquipmentName,
                                              r.PartDescription,
                                              r.StandardID,
                                              r.StandardDescription,
                                              CheckDate = y.PMDate,
                                              CheckTime = y.PMTime,
                                              r.IsAbnormal,
                                              r.IsAlert,
                                              ClosedTime = a.ClosedTime,
                                              ClosedUserID = a.ClosedUserID,
                                              CheckUserID = y.UserID
                                          }).Distinct().AsQueryable();

                            if (Parameters.Status == "1")
                            {
                                query1 = query1.Where(x => x.IsAbnormal && x.CheckDate == today);
                                query2 = query2.Where(x => x.IsAbnormal && x.CheckDate == today);
                            }

                            if (Parameters.Status == "2")
                            {
                                query1 = query1.Where(x => x.IsAlert && x.CheckDate == today);
                                query2 = query2.Where(x => x.IsAlert && x.CheckDate == today);
                            }

                            if (Parameters.Status == "3")
                            {
                                query1 = query1.Where(x => x.IsAbnormal && !x.ClosedTime.HasValue);
                                query2 = query2.Where(x => x.IsAbnormal && !x.ClosedTime.HasValue);
                            }

                            if (Parameters.Status == "4")
                            {
                                query1 = query1.Where(x => x.IsAlert && !x.ClosedTime.HasValue);
                                query2 = query2.Where(x => x.IsAlert && !x.ClosedTime.HasValue);
                            }

                            var queryResult = query1.ToList();

                            foreach (var q in queryResult)
                            {
                                var item = new GridItem
                                {
                                    UniqueID = q.UniqueID,
                                    ControlPointID = q.ControlPointID,
                                    ControlPointDescription = q.ControlPointDescription,
                                    EquipmentID = q.EquipmentID,
                                    EquipmentName = q.EquipmentName,
                                    PartDescription = q.PartDescription,
                                    CheckItemID = q.CheckItemID,
                                    CheckItemDescription = q.CheckItemDescription,
                                    Date = q.CheckDate,
                                    Time = q.CheckTime,
                                    ClosedTime = q.ClosedTime,
                                    IsAbnormal = q.IsAbnormal,
                                    IsAlert = q.IsAlert,
                                    CheckUser = UserDataAccessor.GetUser(q.CheckUserID),
                                    ClosedUser = UserDataAccessor.GetUser(q.ClosedUserID)
                                };

                                var routeManagerList = edb.RouteManager.Where(x => x.RouteUniqueID == q.RouteUniqueID).Select(x => x.UserID).OrderBy(x => x).ToList();

                                foreach (var routeManager in routeManagerList)
                                {
                                    item.ChargePersonList.Add(UserDataAccessor.GetUser(routeManager));
                                }

                                model.ItemList.Add(item);
                            }

                            model.ItemList.AddRange(query2.ToList().Select(x => new GridItem
                            {
                                UniqueID = x.UniqueID,
                                EquipmentID = x.EquipmentID,
                                EquipmentName = x.EquipmentName,
                                PartDescription = x.PartDescription,
                                StandardID = x.StandardID,
                                StandardDescription = x.StandardDescription,
                                Date = x.CheckDate,
                                Time = x.CheckTime,
                                ClosedTime = x.ClosedTime,
                                IsAbnormal = x.IsAbnormal,
                                IsAlert = x.IsAlert,
                                CheckUser = UserDataAccessor.GetUser(x.CheckUserID),
                                ClosedUser = UserDataAccessor.GetUser(x.ClosedUserID),
                                ChargePersonList = new List<Models.Shared.UserModel>() { 
                                    UserDataAccessor.GetUser(x.CheckUserID)
                                }
                            }).ToList());

                            model.ItemList = model.ItemList.OrderBy(x => x.Date).ThenBy(x => x.Time).ThenBy(x => x.EquipmentDisplay).ToList();
                        }

                        if (Parameters.Status == "5" || Parameters.Status == "6" || Parameters.Status == "7" || Parameters.Status == "8")
                        {
                            var query1 = (from abnormal in edb.Abnormal
                                          join x in edb.AbnormalCheckResult
                                          on abnormal.UniqueID equals x.AbnormalUniqueID
                                          join c in edb.CheckResult
                                          on x.CheckResultUniqueID equals c.UniqueID
                                          join r in edb.Route
                                          on c.RouteUniqueID equals r.UniqueID
                                          join a in edb.ArriveRecord
                                          on c.ArriveRecordUniqueID equals a.UniqueID
                                          where Account.QueryableOrganizationUniqueIDList.Contains(r.OrganizationUniqueID)
                                          select new
                                          {
                                              abnormal.UniqueID,
                                              c.RouteUniqueID,
                                              c.ControlPointID,
                                              c.ControlPointDescription,
                                              c.EquipmentUniqueID,
                                              c.EquipmentID,
                                              c.EquipmentName,
                                              c.PartDescription,
                                              c.CheckItemID,
                                              c.CheckItemDescription,
                                              c.CheckDate,
                                              c.CheckTime,
                                              c.IsAbnormal,
                                              c.IsAlert,
                                              abnormal.ClosedTime,
                                              abnormal.ClosedUserID,
                                              CheckUserID = a.UserID
                                          }).Distinct().AsQueryable();

                            var query2 = (from a in edb.Abnormal
                                          join x in edb.AbnormalMFormStandardResult
                                          on a.UniqueID equals x.AbnormalUniqueID
                                          join r in edb.MFormStandardResult
                                          on x.MFormStandardResultUniqueID equals r.UniqueID
                                          join y in edb.MFormResult
                                          on r.ResultUniqueID equals y.UniqueID
                                          where Account.QueryableOrganizationUniqueIDList.Contains(r.OrganizationUniqueID)
                                          select new
                                          {
                                              a.UniqueID,
                                              r.EquipmentUniqueID,
                                              r.EquipmentID,
                                              r.EquipmentName,
                                              r.PartDescription,
                                              r.StandardID,
                                              r.StandardDescription,
                                              CheckDate = y.PMDate,
                                              CheckTime = y.PMTime,
                                              r.IsAbnormal,
                                              r.IsAlert,
                                              ClosedTime = a.ClosedTime,
                                              ClosedUserID = a.ClosedUserID,
                                              CheckUserID = y.UserID
                                          }).Distinct().AsQueryable();

                            if (Parameters.Status == "5")
                            {
                                query1 = query1.Where(x => x.IsAbnormal && x.CheckDate == today);
                                query2 = query2.Where(x => x.IsAbnormal && x.CheckDate == today);
                            }

                            if (Parameters.Status == "6")
                            {
                                query1 = query1.Where(x => x.IsAlert && x.CheckDate == today);
                                query2 = query2.Where(x => x.IsAlert && x.CheckDate == today);
                            }

                            if (Parameters.Status == "7")
                            {
                                query1 = query1.Where(x => x.IsAbnormal && !x.ClosedTime.HasValue);
                                query2 = query2.Where(x => x.IsAbnormal && !x.ClosedTime.HasValue);
                            }

                            if (Parameters.Status == "8")
                            {
                                query1 = query1.Where(x => x.IsAlert && !x.ClosedTime.HasValue);
                                query2 = query2.Where(x => x.IsAlert && !x.ClosedTime.HasValue);
                            }

                            var queryResult = query1.ToList();

                            foreach (var q in queryResult)
                            {
                                var item = new GridItem
                                {
                                    UniqueID = q.UniqueID,
                                    ControlPointID = q.ControlPointID,
                                    ControlPointDescription = q.ControlPointDescription,
                                    EquipmentID = q.EquipmentID,
                                    EquipmentName = q.EquipmentName,
                                    PartDescription = q.PartDescription,
                                    CheckItemID = q.CheckItemID,
                                    CheckItemDescription = q.CheckItemDescription,
                                    Date = q.CheckDate,
                                    Time = q.CheckTime,
                                    ClosedTime = q.ClosedTime,
                                    IsAbnormal = q.IsAbnormal,
                                    IsAlert = q.IsAlert,
                                    CheckUser = UserDataAccessor.GetUser(q.CheckUserID),
                                    ClosedUser = UserDataAccessor.GetUser(q.ClosedUserID)
                                };

                                var routeManagerList = edb.RouteManager.Where(x => x.RouteUniqueID == q.RouteUniqueID).Select(x => x.UserID).OrderBy(x => x).ToList();

                                foreach (var routeManager in routeManagerList)
                                {
                                    item.ChargePersonList.Add(UserDataAccessor.GetUser(routeManager));
                                }

                                model.ItemList.Add(item);
                            }

                            model.ItemList.AddRange(query2.ToList().Select(x => new GridItem
                            {
                                UniqueID = x.UniqueID,
                                EquipmentID = x.EquipmentID,
                                EquipmentName = x.EquipmentName,
                                PartDescription = x.PartDescription,
                                StandardID = x.StandardID,
                                StandardDescription = x.StandardDescription,
                                Date = x.CheckDate,
                                Time = x.CheckTime,
                                ClosedTime = x.ClosedTime,
                                IsAbnormal = x.IsAbnormal,
                                IsAlert = x.IsAlert,
                                CheckUser = UserDataAccessor.GetUser(x.CheckUserID),
                                ClosedUser = UserDataAccessor.GetUser(x.ClosedUserID),
                                ChargePersonList = new List<Models.Shared.UserModel>() { 
                                    UserDataAccessor.GetUser(x.CheckUserID)
                                }
                            }).ToList());

                            model.ItemList = model.ItemList.OrderBy(x => x.Date).ThenBy(x => x.Time).ThenBy(x => x.EquipmentDisplay).ToList();
                        }
                    }
                    else
                    {
                        var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                        var organizationList = Account.QueryableOrganizationUniqueIDList.Intersect(downStreamOrganizationList);

                        var query1 = (from abnormal in edb.Abnormal
                                      join x in edb.AbnormalCheckResult
                                      on abnormal.UniqueID equals x.AbnormalUniqueID
                                      join c in edb.CheckResult
                                      on x.CheckResultUniqueID equals c.UniqueID
                                      join a in edb.ArriveRecord
                                      on c.ArriveRecordUniqueID equals a.UniqueID
                                      where organizationList.Contains(c.OrganizationUniqueID)
                                      select new
                                      {
                                          abnormal.UniqueID,
                                          c.RouteUniqueID,
                                          c.ControlPointID,
                                          c.ControlPointDescription,
                                          c.EquipmentUniqueID,
                                          c.EquipmentID,
                                          c.EquipmentName,
                                          c.PartDescription,
                                          c.CheckItemID,
                                          c.CheckItemDescription,
                                          c.CheckDate,
                                          c.CheckTime,
                                          c.IsAbnormal,
                                          c.IsAlert,
                                          abnormal.ClosedTime,
                                          abnormal.ClosedUserID,
                                          CheckUserID = a.UserID
                                      }).AsQueryable();

                        var query2 = (from abnormal in edb.Abnormal
                                      join x in edb.AbnormalMFormStandardResult
                                      on abnormal.UniqueID equals x.AbnormalUniqueID
                                      join sr in edb.MFormStandardResult
                                      on x.MFormStandardResultUniqueID equals sr.UniqueID
                                      join r in edb.MFormResult
                                      on sr.ResultUniqueID equals r.UniqueID
                                      where organizationList.Contains(sr.OrganizationUniqueID)
                                      select new
                                      {
                                          abnormal.UniqueID,
                                          sr.EquipmentUniqueID,
                                          sr.EquipmentID,
                                          sr.EquipmentName,
                                          sr.PartDescription,
                                          sr.StandardID,
                                          sr.StandardDescription,
                                          CheckDate = r.PMDate,
                                          CheckTime = r.PMTime,
                                          sr.IsAbnormal,
                                          sr.IsAlert,
                                          ClosedTime = abnormal.ClosedTime,
                                          ClosedUserID = abnormal.ClosedUserID,
                                          CheckUserID = r.UserID
                                      }).AsQueryable();

                        if (!string.IsNullOrEmpty(Parameters.BeginDateString))
                        {
                            query1 = query1.Where(x => string.Compare(x.CheckDate, Parameters.BeginDate) >= 0);
                            query2 = query2.Where(x => string.Compare(x.CheckDate, Parameters.BeginDate) >= 0);
                        }

                        if (!string.IsNullOrEmpty(Parameters.EndDateString))
                        {
                            query1 = query1.Where(x => string.Compare(x.CheckDate, Parameters.EndDate) <= 0);
                            query2 = query2.Where(x => string.Compare(x.CheckDate, Parameters.EndDate) <= 0);
                        }

                        if (!string.IsNullOrEmpty(Parameters.EquipmentUniqueID))
                        {
                            query1 = query1.Where(x => x.EquipmentUniqueID == Parameters.EquipmentUniqueID);
                            query2 = query2.Where(x => x.EquipmentUniqueID == Parameters.EquipmentUniqueID);
                        }

                        if (!string.IsNullOrEmpty(Parameters.AbnormalType))
                        {
                            if (Parameters.AbnormalType == "1")
                            {
                                query1 = query1.Where(x => x.IsAbnormal);
                                query2 = query2.Where(x => x.IsAbnormal);
                            }

                            if (Parameters.AbnormalType == "2")
                            {
                                query1 = query1.Where(x => !x.IsAbnormal && x.IsAlert);
                                query2 = query2.Where(x => !x.IsAbnormal && x.IsAlert);
                            }
                        }

                        if (!string.IsNullOrEmpty(Parameters.ClosedStatus))
                        {
                            if (Parameters.ClosedStatus == "0")
                            {
                                query1 = query1.Where(x => !x.ClosedTime.HasValue);
                                query2 = query2.Where(x => !x.ClosedTime.HasValue);
                            }

                            if (Parameters.ClosedStatus == "1")
                            {
                                query1 = query1.Where(x => x.ClosedTime.HasValue);
                                query2 = query2.Where(x => x.ClosedTime.HasValue);
                            }
                        }

                        if (string.IsNullOrEmpty(Parameters.Type))
                        {
                            var queryResult = query1.ToList();

                            foreach (var q in queryResult)
                            {
                                var item = new GridItem
                                {
                                    UniqueID = q.UniqueID,
                                    ControlPointID = q.ControlPointID,
                                    ControlPointDescription = q.ControlPointDescription,
                                    EquipmentID = q.EquipmentID,
                                    EquipmentName = q.EquipmentName,
                                    PartDescription = q.PartDescription,
                                    CheckItemID = q.CheckItemID,
                                    CheckItemDescription = q.CheckItemDescription,
                                    Date = q.CheckDate,
                                    Time = q.CheckTime,
                                    ClosedTime = q.ClosedTime,
                                    IsAbnormal = q.IsAbnormal,
                                    IsAlert = q.IsAlert,
                                    CheckUser = UserDataAccessor.GetUser(q.CheckUserID),
                                    ClosedUser = UserDataAccessor.GetUser(q.ClosedUserID)
                                };

                                var routeManagerList = edb.RouteManager.Where(x => x.RouteUniqueID == q.RouteUniqueID).Select(x => x.UserID).OrderBy(x => x).ToList();

                                foreach (var routeManager in routeManagerList)
                                {
                                    item.ChargePersonList.Add(UserDataAccessor.GetUser(routeManager));
                                }

                                model.ItemList.Add(item);
                            }

                            model.ItemList.AddRange(query2.ToList().Select(x => new GridItem
                            {
                                UniqueID = x.UniqueID,
                                EquipmentID = x.EquipmentID,
                                EquipmentName = x.EquipmentName,
                                PartDescription = x.PartDescription,
                                StandardID = x.StandardID,
                                StandardDescription = x.StandardDescription,
                                Date = x.CheckDate,
                                Time = x.CheckTime,
                                ClosedTime = x.ClosedTime,
                                IsAbnormal = x.IsAbnormal,
                                IsAlert = x.IsAlert,
                                CheckUser = UserDataAccessor.GetUser(x.CheckUserID),
                                ClosedUser = UserDataAccessor.GetUser(x.ClosedUserID),
                                ChargePersonList = new List<Models.Shared.UserModel>() 
                                { 
                                    UserDataAccessor.GetUser(x.CheckUserID)
                                }
                            }).ToList());
                        }
                        else
                        {
                            if (Parameters.Type == "P")
                            {
                                var queryResult = query1.ToList();

                                foreach (var q in queryResult)
                                {
                                    var item = new GridItem
                                    {
                                        UniqueID = q.UniqueID,
                                        ControlPointID = q.ControlPointID,
                                        ControlPointDescription = q.ControlPointDescription,
                                        EquipmentID = q.EquipmentID,
                                        EquipmentName = q.EquipmentName,
                                        PartDescription = q.PartDescription,
                                        CheckItemID = q.CheckItemID,
                                        CheckItemDescription = q.CheckItemDescription,
                                        Date = q.CheckDate,
                                        Time = q.CheckTime,
                                        ClosedTime = q.ClosedTime,
                                        IsAbnormal = q.IsAbnormal,
                                        IsAlert = q.IsAlert,
                                        CheckUser = UserDataAccessor.GetUser(q.CheckUserID),
                                        ClosedUser = UserDataAccessor.GetUser(q.ClosedUserID)
                                    };

                                    var routeManagerList = edb.RouteManager.Where(x => x.RouteUniqueID == q.RouteUniqueID).Select(x => x.UserID).OrderBy(x => x).ToList();

                                    foreach (var routeManager in routeManagerList)
                                    {
                                        item.ChargePersonList.Add(UserDataAccessor.GetUser(routeManager));
                                    }

                                    model.ItemList.Add(item);
                                }
                            }

                            else if (Parameters.Type == "M")
                            {
                                model.ItemList.AddRange(query2.ToList().Select(x => new GridItem
                                {
                                    UniqueID = x.UniqueID,
                                    EquipmentID = x.EquipmentID,
                                    EquipmentName = x.EquipmentName,
                                    PartDescription = x.PartDescription,
                                    StandardID = x.StandardID,
                                    StandardDescription = x.StandardDescription,
                                    Date = x.CheckDate,
                                    Time = x.CheckTime,
                                    ClosedTime = x.ClosedTime,
                                    IsAbnormal = x.IsAbnormal,
                                    IsAlert = x.IsAlert,
                                    CheckUser = UserDataAccessor.GetUser(x.CheckUserID),
                                    ClosedUser = UserDataAccessor.GetUser(x.ClosedUserID),
                                    ChargePersonList = new List<Models.Shared.UserModel>() 
                                    { 
                                        UserDataAccessor.GetUser(x.CheckUserID)
                                    }
                                }).ToList());
                            }
                        }

                        model.ItemList = model.ItemList.OrderBy(x => x.Date).ThenBy(x => x.Time).ThenBy(x => x.EquipmentDisplay).ToList();
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

        public static RequestResult GetDetailViewModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new DetailViewModel();

                using (EDbEntities db = new EDbEntities())
                {
                    var query1 = (from abnormal in db.Abnormal
                                  join x in db.AbnormalCheckResult
                                  on abnormal.UniqueID equals x.AbnormalUniqueID
                                  join c in db.CheckResult
                                  on x.CheckResultUniqueID equals c.UniqueID
                                  join a in db.ArriveRecord
                                  on c.ArriveRecordUniqueID equals a.UniqueID
                                  where abnormal.UniqueID == UniqueID
                                  select new
                                  {
                                      abnormal.UniqueID,
                                      CheckResultUniqueID = c.UniqueID,
                                      c.RouteUniqueID,
                                      c.ControlPointID,
                                      c.ControlPointDescription,
                                      c.EquipmentUniqueID,
                                      c.EquipmentID,
                                      c.EquipmentName,
                                      c.PartDescription,
                                      c.CheckItemID,
                                      c.CheckItemDescription,
                                      c.CheckDate,
                                      c.CheckTime,
                                      c.IsAbnormal,
                                      c.IsAlert,
                                      c.Result,
                                      c.LowerLimit,
                                      c.LowerAlertLimit,
                                      c.UpperAlertLimit,
                                      c.UpperLimit,
                                      c.Unit,
                                      c.Remark,
                                      abnormal.ClosedTime,
                                      abnormal.ClosedUserID,
                                      abnormal.ClosedRemark,
                                      CheckUserID = a.UserID
                                  }).FirstOrDefault();

                    var query2 = (from abnormal in db.Abnormal
                                  join x in db.AbnormalMFormStandardResult
                                  on abnormal.UniqueID equals x.AbnormalUniqueID
                                  join sr in db.MFormStandardResult
                                  on x.MFormStandardResultUniqueID equals sr.UniqueID
                                  join r in db.MFormResult
                                  on sr.ResultUniqueID equals r.UniqueID
                                  where abnormal.UniqueID == UniqueID
                                  select new
                                  {
                                      abnormal.UniqueID,
                                      sr.EquipmentUniqueID,
                                      sr.EquipmentID,
                                      sr.EquipmentName,
                                      sr.PartDescription,
                                      sr.StandardID,
                                      sr.StandardDescription,
                                      CheckDate = r.PMDate,
                                      CheckTime = r.PMTime,
                                      sr.IsAbnormal,
                                      sr.IsAlert,
                                      sr.Result,
                                      sr.LowerLimit,
                                      sr.LowerAlertLimit,
                                      sr.UpperAlertLimit,
                                      sr.UpperLimit,
                                      sr.Unit,
                                      abnormal.ClosedTime,
                                      abnormal.ClosedUserID,
                                      abnormal.ClosedRemark,
                                      CheckUserID = r.UserID
                                  }).FirstOrDefault();

                    if (query1 != null)
                    {
                        model = new DetailViewModel()
                        {
                            UniqueID = query1.UniqueID,
                            ControlPointID = query1.ControlPointID,
                            ControlPointDescription = query1.ControlPointDescription,
                            EquipmentID = query1.EquipmentID,
                            EquipmentName = query1.EquipmentName,
                            PartDescription = query1.PartDescription,
                            CheckItemID = query1.CheckItemID,
                            CheckItemDescription = query1.CheckItemDescription,
                            Date = query1.CheckDate,
                            Time = query1.CheckTime,
                            IsAbnormal = query1.IsAbnormal,
                            IsAlert = query1.IsAlert,
                            Result = query1.Result,
                            LowerLimit = query1.LowerLimit,
                            LowerAlertLimit=query1.LowerAlertLimit,
                            UpperAlertLimit = query1.UpperAlertLimit,
                            UpperLimit = query1.UpperLimit,
                            Unit = query1.Unit,
                            Remark = query1.Remark,
                            CheckUser = UserDataAccessor.GetUser(query1.CheckUserID),
                            ClosedUser = UserDataAccessor.GetUser(query1.ClosedUserID),
                            ClosedTime = query1.ClosedTime,
                            ClosedRemark = query1.ClosedRemark,
                            AbnormalReasonList = db.CheckResultAbnormalReason.Where(a => a.CheckResultUniqueID == query1.CheckResultUniqueID).OrderBy(a => a.AbnormalReasonID).Select(a => new AbnormalReasonModel
                            {
                                Description = a.AbnormalReasonDescription,
                                Remark = a.AbnormalReasonRemark,
                                HandlingMethodList = db.CheckResultHandlingMethod.Where(h => h.CheckResultUniqueID == query1.CheckResultUniqueID && h.AbnormalReasonUniqueID == a.AbnormalReasonUniqueID).OrderBy(h => h.HandlingMethodID).Select(h => new HandlingMethodModel
                                {
                                    Description = h.HandlingMethodDescription,
                                    Remark = h.HandlingMethodRemark
                                }).ToList()
                            }).ToList(),
                            BeforePhotoList = db.AbnormalPhoto.Where(p => p.AbnormalUniqueID == query1.UniqueID && p.Type == "B").OrderBy(p => p.Seq).ToList().Select(p => p.AbnormalUniqueID + "_" + p.Type + "_" + p.Seq + "." + p.Extension).ToList(),
                            AfterPhotoList = db.AbnormalPhoto.Where(p => p.AbnormalUniqueID == query1.UniqueID && p.Type == "A").OrderBy(p => p.Seq).ToList().Select(p => p.AbnormalUniqueID + "_" + p.Type + "_" + p.Seq + "." + p.Extension).ToList(),
                            FileList = db.AbnormalFile.Where(f => f.AbnormalUniqueID == query1.UniqueID).ToList().Select(f => new FileModel
                            {
                                AbnormalUniqueID = f.AbnormalUniqueID,
                                Seq = f.Seq,
                                FileName = f.FileName,
                                Extension = f.Extension,
                                Size = f.ContentLength,
                                UploadTime = f.UploadTime,
                                IsSaved = true
                            }).OrderBy(f => f.UploadTime).ToList()
                        };

                        var routeManagerList = db.RouteManager.Where(x => x.RouteUniqueID == query1.RouteUniqueID).Select(x => x.UserID).OrderBy(x => x).ToList();

                        foreach (var routeManager in routeManagerList)
                        {
                            model.ChargePersonList.Add(UserDataAccessor.GetUser(routeManager));
                        }
                    }
                    else if (query2 != null)
                    {
                        model = new DetailViewModel()
                        {
                            UniqueID = query2.UniqueID,
                            EquipmentID = query2.EquipmentID,
                            EquipmentName = query2.EquipmentName,
                            PartDescription = query2.PartDescription,
                            StandardID = query2.StandardID,
                            StandardDescription = query2.StandardDescription,
                            Date = query2.CheckDate,
                            Time = query2.CheckTime,
                            IsAbnormal = query2.IsAbnormal,
                            IsAlert = query2.IsAlert,
                            Result = query2.Result,
                            LowerLimit = query2.LowerLimit,
                            LowerAlertLimit = query2.LowerAlertLimit,
                            UpperAlertLimit = query2.UpperAlertLimit,
                            UpperLimit = query2.UpperLimit,
                            Unit = query2.Unit,
                            CheckUser = UserDataAccessor.GetUser(query2.CheckUserID),
                            ClosedUser = UserDataAccessor.GetUser(query2.ClosedUserID),
                            ClosedTime = query2.ClosedTime,
                            ClosedRemark = query2.ClosedRemark,
                            BeforePhotoList = db.AbnormalPhoto.Where(p => p.AbnormalUniqueID == query1.UniqueID && p.Type == "B").OrderBy(p => p.Seq).ToList().Select(p => p.AbnormalUniqueID + "_" + p.Type + "_" + p.Seq + "." + p.Extension).ToList(),
                            AfterPhotoList = db.AbnormalPhoto.Where(p => p.AbnormalUniqueID == query1.UniqueID && p.Type == "A").OrderBy(p => p.Seq).ToList().Select(p => p.AbnormalUniqueID + "_" + p.Type + "_" + p.Seq + "." + p.Extension).ToList(),
                            FileList = db.AbnormalFile.Where(f => f.AbnormalUniqueID == query1.UniqueID).ToList().Select(f => new FileModel
                            {
                                AbnormalUniqueID = f.AbnormalUniqueID,
                                Seq = f.Seq,
                                FileName = f.FileName,
                                Extension = f.Extension,
                                Size = f.ContentLength,
                                UploadTime = f.UploadTime,
                                IsSaved = true
                            }).OrderBy(f => f.UploadTime).ToList(),
                        };

                        model.ChargePersonList.Add(UserDataAccessor.GetUser(query2.CheckUserID));
                    }

                    model.RepairFormList = GetRepairFormList(model.UniqueID);
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

        public static RequestResult GetEditFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new EditFormModel();

                using (EDbEntities db = new EDbEntities())
                {
                    var query1 = (from abnormal in db.Abnormal
                                  join x in db.AbnormalCheckResult
                                  on abnormal.UniqueID equals x.AbnormalUniqueID
                                  join c in db.CheckResult
                                  on x.CheckResultUniqueID equals c.UniqueID
                                  join a in db.ArriveRecord
                                  on c.ArriveRecordUniqueID equals a.UniqueID
                                  where abnormal.UniqueID == UniqueID
                                  select new
                                  {
                                      abnormal.UniqueID,
                                      CheckResultUniqueID = c.UniqueID,
                                      c.RouteUniqueID,
                                      c.ControlPointID,
                                      c.ControlPointDescription,
                                      c.EquipmentUniqueID,
                                      c.EquipmentID,
                                      c.EquipmentName,
                                      c.PartDescription,
                                      c.CheckItemID,
                                      c.CheckItemDescription,
                                      c.CheckDate,
                                      c.CheckTime,
                                      c.IsAbnormal,
                                      c.IsAlert,
                                      c.Result,
                                      c.LowerLimit,
                                      c.LowerAlertLimit,
                                      c.UpperAlertLimit,
                                      c.UpperLimit,
                                      c.Unit,
                                      c.Remark,
                                      abnormal.ClosedTime,
                                      abnormal.ClosedUserID,
                                      abnormal.ClosedRemark,
                                      CheckUserID = a.UserID
                                  }).FirstOrDefault();

                    var query2 = (from abnormal in db.Abnormal
                                  join x in db.AbnormalMFormStandardResult
                                  on abnormal.UniqueID equals x.AbnormalUniqueID
                                  join sr in db.MFormStandardResult
                                  on x.MFormStandardResultUniqueID equals sr.UniqueID
                                  join r in db.MFormResult
                                  on sr.ResultUniqueID equals r.UniqueID
                                  where abnormal.UniqueID == UniqueID
                                  select new
                                  {
                                      abnormal.UniqueID,
                                      sr.EquipmentUniqueID,
                                      sr.EquipmentID,
                                      sr.EquipmentName,
                                      sr.PartDescription,
                                      sr.StandardID,
                                      sr.StandardDescription,
                                      CheckDate = r.PMDate,
                                      CheckTime = r.PMTime,
                                      sr.IsAbnormal,
                                      sr.IsAlert,
                                      sr.Result,
                                      sr.LowerLimit,
                                      sr.LowerAlertLimit,
                                      sr.UpperAlertLimit,
                                      sr.UpperLimit,
                                      sr.Unit,
                                      abnormal.ClosedTime,
                                      abnormal.ClosedUserID,
                                      abnormal.ClosedRemark,
                                      CheckUserID = r.UserID
                                  }).FirstOrDefault();

                    if (query1 != null)
                    {
                        model = new EditFormModel()
                        {
                            UniqueID = query1.UniqueID,
                            ControlPointID = query1.ControlPointID,
                            ControlPointDescription = query1.ControlPointDescription,
                            EquipmentID = query1.EquipmentID,
                            EquipmentName = query1.EquipmentName,
                            PartDescription = query1.PartDescription,
                            CheckItemID = query1.CheckItemID,
                            CheckItemDescription = query1.CheckItemDescription,
                            Date = query1.CheckDate,
                            Time = query1.CheckTime,
                            IsAbnormal = query1.IsAbnormal,
                            IsAlert = query1.IsAlert,
                            Result = query1.Result,
                            LowerLimit = query1.LowerLimit,
                            LowerAlertLimit = query1.LowerAlertLimit,
                            UpperAlertLimit = query1.UpperAlertLimit,
                            UpperLimit = query1.UpperLimit,
                            Unit = query1.Unit,
                            Remark = query1.Remark,
                            CheckUser = UserDataAccessor.GetUser(query1.CheckUserID),
                            ClosedUser = UserDataAccessor.GetUser(query1.ClosedUserID),
                            ClosedTime = query1.ClosedTime,
                            ClosedRemark = query1.ClosedRemark,
                            AbnormalReasonList = db.CheckResultAbnormalReason.Where(a => a.CheckResultUniqueID == query1.CheckResultUniqueID).OrderBy(a => a.AbnormalReasonID).Select(a => new AbnormalReasonModel
                            {
                                Description = a.AbnormalReasonDescription,
                                Remark = a.AbnormalReasonRemark,
                                HandlingMethodList = db.CheckResultHandlingMethod.Where(h => h.CheckResultUniqueID == query1.CheckResultUniqueID && h.AbnormalReasonUniqueID == a.AbnormalReasonUniqueID).OrderBy(h => h.HandlingMethodID).Select(h => new HandlingMethodModel
                                {
                                    Description = h.HandlingMethodDescription,
                                    Remark = h.HandlingMethodRemark
                                }).ToList()
                            }).ToList()
                        };

                        var routeManagerList = db.RouteManager.Where(x => x.RouteUniqueID == query1.RouteUniqueID).Select(x => x.UserID).OrderBy(x => x).ToList();

                        foreach (var routeManager in routeManagerList)
                        {
                            model.ChargePersonList.Add(UserDataAccessor.GetUser(routeManager));
                        }
                    }
                    else if (query2 != null)
                    {
                        model = new EditFormModel()
                        {
                            UniqueID = query2.UniqueID,
                            EquipmentID = query2.EquipmentID,
                            EquipmentName = query2.EquipmentName,
                            PartDescription = query2.PartDescription,
                            StandardID = query2.StandardID,
                            StandardDescription = query2.StandardDescription,
                            Date = query2.CheckDate,
                            Time = query2.CheckTime,
                            IsAbnormal = query2.IsAbnormal,
                            IsAlert = query2.IsAlert,
                            Result = query2.Result,
                            LowerLimit = query2.LowerLimit,
                            LowerAlertLimit = query2.LowerAlertLimit,
                            UpperAlertLimit = query2.UpperAlertLimit,
                            UpperLimit = query2.UpperLimit,
                            Unit = query2.Unit,
                            CheckUser = UserDataAccessor.GetUser(query2.CheckUserID),
                            ClosedUser = UserDataAccessor.GetUser(query2.ClosedUserID),
                            ClosedTime = query2.ClosedTime,
                            ClosedRemark = query2.ClosedRemark
                        };

                        model.ChargePersonList.Add(UserDataAccessor.GetUser(query2.CheckUserID));
                    }

                    var beforePhotoList = db.AbnormalPhoto.Where(x => x.AbnormalUniqueID == model.UniqueID && x.Type == "B").OrderBy(x => x.Seq).ToList();

                    foreach (var photo in beforePhotoList)
                    {
                        model.BeforePhotoList.Add(new PhotoModel()
                        {
                            TempUniqueID = Guid.NewGuid().ToString(),
                            AbnormalUniqueID = photo.AbnormalUniqueID,
                            Seq = photo.Seq,
                            Extension = photo.Extension,
                            Type = photo.Type,
                            IsSaved = true
                        });
                    }

                    var afterPhotoList = db.AbnormalPhoto.Where(x => x.AbnormalUniqueID == model.UniqueID && x.Type == "A").OrderBy(x => x.Seq).ToList();

                    foreach (var photo in afterPhotoList)
                    {
                        model.AfterPhotoList.Add(new PhotoModel()
                        {
                            TempUniqueID = Guid.NewGuid().ToString(),
                            AbnormalUniqueID = photo.AbnormalUniqueID,
                            Seq = photo.Seq,
                            Extension = photo.Extension,
                            Type = photo.Type,
                            IsSaved = true
                        });
                    }

                    model.FileList = db.AbnormalFile.Where(x => x.AbnormalUniqueID == model.UniqueID).ToList().Select(x => new FileModel
                    {
                        AbnormalUniqueID = x.AbnormalUniqueID,
                        Seq = x.Seq,
                        FileName = x.FileName,
                        Extension = x.Extension,
                        Size = x.ContentLength,
                        UploadTime = x.UploadTime,
                        IsSaved = true
                    }).OrderBy(x => x.UploadTime).ToList();

                    model.RepairFormList = GetRepairFormList(model.UniqueID);
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

        public static RequestResult Save(EditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var abnormal = db.Abnormal.First(x => x.UniqueID == Model.UniqueID);

                    abnormal.ClosedRemark = Model.ClosedRemark;

                    var beforePhotoList = db.AbnormalPhoto.Where(x => x.AbnormalUniqueID == Model.UniqueID && x.Type == "B").ToList();

                    foreach (var photo in beforePhotoList)
                    {
                        if (!Model.BeforePhotoList.Any(x => x.Seq == photo.Seq))
                        {
                            db.AbnormalPhoto.Remove(photo);

                            try
                            {
                                System.IO.File.Delete(Path.Combine(Config.EquipmentMaintenancePhotoFolderPath, string.Format("{0}_{1}_{2}.{3}", Model.UniqueID,photo.Type, photo.Seq, photo.Extension)));
                            }
                            catch { }
                        }
                    }

                    var afterPhotoList = db.AbnormalPhoto.Where(x => x.AbnormalUniqueID == Model.UniqueID && x.Type == "A").ToList();

                    foreach (var photo in afterPhotoList)
                    {
                        if (!Model.AfterPhotoList.Any(x => x.Seq == photo.Seq))
                        {
                            db.AbnormalPhoto.Remove(photo);

                            try
                            {
                                System.IO.File.Delete(Path.Combine(Config.EquipmentMaintenancePhotoFolderPath, string.Format("{0}_{1}_{2}.{3}", Model.UniqueID, photo.Type, photo.Seq, photo.Extension)));
                            }
                            catch { }
                        }
                    }

                    foreach (var photo in Model.BeforePhotoList)
                    {
                        if (!photo.IsSaved)
                        {
                            db.AbnormalPhoto.Add(new AbnormalPhoto()
                            {
                                AbnormalUniqueID = Model.UniqueID,
                                Seq = photo.Seq,
                                Extension = photo.Extension,
                                Type = photo.Type
                            });

                            System.IO.File.Move(photo.TempFullFileName, Path.Combine(Config.EquipmentMaintenancePhotoFolderPath, string.Format("{0}_{1}_{2}.{3}", Model.UniqueID, photo.Type, photo.Seq, photo.Extension)));
                        }
                    }

                    foreach (var photo in Model.AfterPhotoList)
                    {
                        if (!photo.IsSaved)
                        {
                            db.AbnormalPhoto.Add(new AbnormalPhoto()
                            {
                                AbnormalUniqueID = Model.UniqueID,
                                Seq = photo.Seq,
                                Extension = photo.Extension,
                                Type = photo.Type
                            });

                            System.IO.File.Move(photo.TempFullFileName, Path.Combine(Config.EquipmentMaintenancePhotoFolderPath, string.Format("{0}_{1}_{2}.{3}", Model.UniqueID, photo.Type, photo.Seq, photo.Extension)));
                        }
                    }

                    var fileList = db.AbnormalFile.Where(x => x.AbnormalUniqueID == Model.UniqueID).ToList();

                    foreach (var file in fileList)
                    {
                        if (!Model.FileList.Any(x => x.Seq == file.Seq))
                        {
                            db.AbnormalFile.Remove(file);

                            try
                            {
                                System.IO.File.Delete(Path.Combine(Config.EquipmentMaintenanceFileFolderPath, string.Format("{0}_{1}.{2}", Model.UniqueID, file.Seq, file.Extension)));
                            }
                            catch { }
                        }
                    }

                    foreach (var file in Model.FileList)
                    {
                        if (!file.IsSaved)
                        {
                            db.AbnormalFile.Add(new AbnormalFile()
                            {
                                AbnormalUniqueID = Model.UniqueID,
                                Seq = file.Seq,
                                Extension = file.Extension,
                                FileName = file.FileName,
                                UploadTime = file.UploadTime,
                                ContentLength = file.Size
                            });

                            System.IO.File.Move(file.TempFullFileName, Path.Combine(Config.EquipmentMaintenanceFileFolderPath, string.Format("{0}_{1}.{2}", Model.UniqueID, file.Seq, file.Extension)));
                        }
                    }

                    foreach (var repairForm in Model.RepairFormList)
                    {
                        if (!db.AbnormalRForm.Any(x => x.AbnormalUniqueID == Model.UniqueID && x.RFormUniqueID == repairForm.UniqueID))
                        {
                            db.AbnormalRForm.Add(new AbnormalRForm()
                            {
                                AbnormalUniqueID = Model.UniqueID,
                                RFormUniqueID = repairForm.UniqueID
                            });
                        }
                    }

                    db.SaveChanges();

                    result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Save, Resources.Resource.Success));
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

        public static RequestResult Closed(EditFormModel Model, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                result = Save(Model);

                if (result.IsSuccess)
                {
                    if (string.IsNullOrEmpty(Model.ClosedRemark) && Model.RepairFormList.Count == 0)
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1} {2}", Resources.Resource.CommentRequired, Resources.Resource.Or, Resources.Resource.TransRepairForm));
                    }
                    else
                    {
                        using (EDbEntities db = new EDbEntities())
                        {
                            var abnormal = db.Abnormal.First(x => x.UniqueID == Model.UniqueID);

                            abnormal.ClosedTime = DateTime.Now;
                            abnormal.ClosedUserID = Account.ID;

                            db.SaveChanges();
                        }

                        result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Closed, Resources.Resource.Success));
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

        public static RequestResult GetRepairFormCreateFormModel(string AbnormalUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new RepairFormCreateFormModel()
                {
                    RepairFormTypeSelectItemList = new List<SelectListItem>() 
                    { 
                        Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                    },
                    SubjectSelectItemList = new List<SelectListItem>() 
                    { 
                        Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                    },
                    AbnormalUniqueID = AbnormalUniqueID
                };

                using (EDbEntities db = new EDbEntities())
                {
                    var query1 = (from a in db.Abnormal
                                  join x in db.AbnormalCheckResult
                                  on a.UniqueID equals x.AbnormalUniqueID
                                  join c in db.CheckResult
                                  on x.CheckResultUniqueID equals c.UniqueID
                                  where a.UniqueID == AbnormalUniqueID
                                  select new
                                  {
                                      CheckResultUniqueID = c.UniqueID,
                                      c.OrganizationUniqueID,
                                      c.EquipmentUniqueID,
                                      c.EquipmentID,
                                      c.EquipmentName,
                                      c.PartUniqueID,
                                      c.PartDescription,
                                      c.CheckItemDescription
                                  }).FirstOrDefault();

                    var query2 = (from a in db.Abnormal
                                  join x in db.AbnormalMFormStandardResult
                                  on a.UniqueID equals x.AbnormalUniqueID
                                  join sr in db.MFormStandardResult
                                  on x.MFormStandardResultUniqueID equals sr.UniqueID
                                  where a.UniqueID == AbnormalUniqueID
                                  select new
                                  {
                                      sr.OrganizationUniqueID,
                                      sr.EquipmentUniqueID,
                                      sr.EquipmentID,
                                      sr.EquipmentName,
                                      sr.PartUniqueID,
                                      sr.PartDescription,
                                      sr.StandardDescription
                                  }).FirstOrDefault();

                    var ancestorOrganizationUniqueID = string.Empty;

                    if (query1 != null)
                    {
                        ancestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(query1.OrganizationUniqueID);
                        model.OrganizationUniqueID = query1.OrganizationUniqueID;
                        model.AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(query1.OrganizationUniqueID);
                        model.FullOrganizationDescription = OrganizationDataAccessor.GetOrganizationFullDescription(query1.OrganizationUniqueID);

                        if (!string.IsNullOrEmpty(query1.EquipmentUniqueID))
                        {
                            var equipment = db.Equipment.FirstOrDefault(x => x.UniqueID == query1.EquipmentUniqueID);

                            if (equipment != null && !string.IsNullOrEmpty(equipment.MaintenanceOrganizationUniqueID))
                            {
                                var maintenanceOrganization = OrganizationDataAccessor.GetOrganization(equipment.MaintenanceOrganizationUniqueID);

                                model.MaintenanceOrganization = string.Format("{0}/{1}", maintenanceOrganization.ID, maintenanceOrganization.Description);

                                model.FormInput.MaintenanceOrganizationUniqueID = equipment.MaintenanceOrganizationUniqueID;
                            }
                        }

                        model.EquipmentID = query1.EquipmentID;
                        model.EquipmentName = query1.EquipmentName;
                        model.PartDescription = !string.IsNullOrEmpty(query1.PartUniqueID) && query1.PartUniqueID != "*" ? query1.PartDescription : "";

                        string reason = string.Empty;

                        var abnormalReasonList = db.CheckResultAbnormalReason.Where(x => x.CheckResultUniqueID == query1.CheckResultUniqueID).OrderBy(x => x.AbnormalReasonID).ToList();

                        if (abnormalReasonList.Count > 0)
                        {
                            var sb = new StringBuilder();

                            foreach (var abnormalReason in abnormalReasonList)
                            {
                                if (!string.IsNullOrEmpty(abnormalReason.AbnormalReasonDescription))
                                {
                                    sb.Append(abnormalReason.AbnormalReasonDescription);
                                }
                                else
                                {
                                    sb.Append(abnormalReason.AbnormalReasonRemark);
                                }

                                sb.Append("、");
                            }

                            sb.Remove(sb.Length - 1, 1);

                            reason = sb.ToString();
                        }

                        model.FormInput.EquipmentUniqueID = string.Format("{0}{1}{2}", query1.EquipmentUniqueID, Define.Seperator, query1.PartUniqueID);
                        model.FormInput.Subject = query1.CheckItemDescription;
                        model.FormInput.Description = reason;
                    }

                    if (query2 != null)
                    {
                        ancestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(query2.OrganizationUniqueID);
                        model.OrganizationUniqueID = query2.OrganizationUniqueID;
                        model.AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(query2.OrganizationUniqueID);
                        model.FullOrganizationDescription = OrganizationDataAccessor.GetOrganizationFullDescription(query2.OrganizationUniqueID);

                        if (!string.IsNullOrEmpty(query2.EquipmentUniqueID))
                        {
                            var equipment = db.Equipment.FirstOrDefault(x => x.UniqueID == query2.EquipmentUniqueID);

                            if (equipment != null && !string.IsNullOrEmpty(equipment.MaintenanceOrganizationUniqueID))
                            {
                                var maintenanceOrganization = OrganizationDataAccessor.GetOrganization(equipment.MaintenanceOrganizationUniqueID);

                                model.MaintenanceOrganization = string.Format("{0}/{1}", maintenanceOrganization.ID, maintenanceOrganization.Description);

                                model.FormInput.MaintenanceOrganizationUniqueID = equipment.MaintenanceOrganizationUniqueID;
                            }
                        }

                        model.EquipmentID = query2.EquipmentID;
                        model.EquipmentName = query2.EquipmentName;
                        model.PartDescription = !string.IsNullOrEmpty(query2.PartUniqueID) && query2.PartUniqueID != "*" ? query2.PartDescription : "";

                        model.FormInput.EquipmentUniqueID = string.Format("{0}{1}{2}", query2.EquipmentUniqueID, Define.Seperator, query2.PartUniqueID);
                        model.FormInput.Subject = query2.StandardDescription;
                    }

                    var repairFormTypeList = db.RFormType.Where(x => x.AncestorOrganizationUniqueID == ancestorOrganizationUniqueID).OrderBy(x => x.Description).ToList();

                    foreach (var repairFormType in repairFormTypeList)
                    {
                        model.RepairFormTypeSelectItemList.Add(new SelectListItem()
                        {
                            Value = repairFormType.UniqueID,
                            Text = repairFormType.Description
                        });

                        model.RepairFormTypeSubjectList.Add(repairFormType.UniqueID, (from x in db.RFormTypeSubject
                                                                                      join s in db.RFormSubject
                                                                                      on x.SubjectUniqueID equals s.UniqueID
                                                                                      where x.RFormTypeUniqueID == repairFormType.UniqueID
                                                                                      orderby x.Seq
                                                                                      select new Models.EquipmentMaintenance.AbnormalHandlingManagement.RFormSubject
                                                                                      {
                                                                                          UniqueID = s.UniqueID,
                                                                                          ID = s.ID,
                                                                                          Description = s.Description,
                                                                                          AncestorOrganizationUniqueID = s.AncestorOrganizationUniqueID
                                                                                      }).ToList());
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

        public static RequestResult CreateRepairForm(RepairFormCreateFormModel Model, string UserID)
        {
            RequestResult result = new RequestResult();

            try
            {
                //string jobManagerID = string.Empty;
                string maintenanceOrganizationUniqueID = string.Empty;

                using (DbEntities db = new DbEntities())
                {
                    if (Model.FormInput.IsRepairBySelf)
                    {
                        maintenanceOrganizationUniqueID = db.User.First(x => x.ID == UserID).OrganizationUniqueID;
                        //jobManagerID = UserID;
                    }
                    else
                    {
                        maintenanceOrganizationUniqueID = Model.FormInput.MaintenanceOrganizationUniqueID;

                        var organizationManagers = (from x in db.OrganizationManager
                                                    join u in db.User
                                                    on x.UserID equals u.ID
                                                    where x.OrganizationUniqueID == Model.FormInput.MaintenanceOrganizationUniqueID
                                                    select u).ToList();

                        var organization = db.Organization.First(x => x.UniqueID == Model.FormInput.MaintenanceOrganizationUniqueID);

                        //if (!string.IsNullOrEmpty(organization.ManagerUserID))
                            if(organizationManagers.Count>0)
                        {
                            //jobManagerID = organization.ManagerUserID;
                        }
                        else
                        {
                            //jobManagerID = string.Empty;
                            maintenanceOrganizationUniqueID = string.Empty;

                            result.ReturnFailedMessage(string.Format("{0} {1} {2} {3}", Resources.Resource.Organization, organization.Description, Resources.Resource.NotSet, Resources.Resource.Manager));
                        }
                    }
                }

                if (!string.IsNullOrEmpty(maintenanceOrganizationUniqueID))
                    //if (!string.IsNullOrEmpty(jobManagerID))
                {
                    using (EDbEntities db = new EDbEntities())
                    {
                        var equipmentUniqueID = string.Empty;
                        var partUniqueID = string.Empty;

                        if (!string.IsNullOrEmpty(Model.FormInput.EquipmentUniqueID))
                        {
                            string[] t = Model.FormInput.EquipmentUniqueID.Split(Define.Seperators, StringSplitOptions.None);

                            equipmentUniqueID = t[0];
                            partUniqueID = t[1];
                        }

                        var vhnoPrefix = string.Format("R{0}", DateTime.Today.ToString("yyyyMM").Substring(2));

                        var seq = 1;

                        var temp = db.RForm.Where(x => x.VHNO.StartsWith(vhnoPrefix)).OrderByDescending(x => x.VHNO).FirstOrDefault();

                        if (temp != null)
                        {
                            seq = int.Parse(temp.VHNO.Substring(5)) + 1;
                        }

                        var vhno = string.Format("{0}{1}", vhnoPrefix, seq.ToString().PadLeft(4, '0'));

                        var uniqueID = Guid.NewGuid().ToString();

                        var createTime = DateTime.Now;

                        var equipment = db.Equipment.FirstOrDefault(x => x.UniqueID == equipmentUniqueID);
                        var part = db.EquipmentPart.FirstOrDefault(x => x.UniqueID == partUniqueID);

                        var form = new RForm()
                        {
                            UniqueID = uniqueID,
                            Status = "0",
                            VHNO = vhno,
                            OrganizationUniqueID = Model.OrganizationUniqueID,
                            MaintenanceOrganizationUniqueID = maintenanceOrganizationUniqueID,
                            EquipmentUniqueID = equipmentUniqueID,
                            PartUniqueID = partUniqueID,
                            RFormTypeUniqueID = Model.FormInput.RepairFormTypeUniqueID,
                            Subject = Model.FormInput.Subject,
                            Description = Model.FormInput.Description,
                            CreateUserID = UserID,
                            CreateTime = createTime,
                            EquipmentID = equipment != null ? equipment.ID : string.Empty,
                            EquipmentName = equipment != null ? equipment.Name : string.Empty,
                            PartDescription = part != null ? part.Description : string.Empty
                        };

                        if (Model.FormInput.IsRepairBySelf)
                        {
                            form.Status = "4";
                            form.ManagerJobTime = createTime;
                            form.JobRefuseReason = string.Empty;
                            form.TakeJobTime = createTime;
                            form.TakeJobUserID = UserID;
                            form.EstBeginDate = Model.FormInput.EstBeginDate;
                            form.EstEndDate = Model.FormInput.EstEndDate;

                            db.RFormJobUser.Add(new RFormJobUser()
                            {
                                RFormUniqueID = uniqueID,
                                UserID = UserID
                            });
                        }

                        db.RForm.Add(form);

                        db.AbnormalRForm.Add(new AbnormalRForm()
                        {
                            AbnormalUniqueID = Model.AbnormalUniqueID,
                            RFormUniqueID = uniqueID
                        });

                        db.SaveChanges();

                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.RepairForm, Resources.Resource.Success));
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

        public static List<RepairFormModel> GetRepairFormList(string UniqueID)
        {
            var itemList = new List<RepairFormModel>();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var repairFormList = (from x in db.AbnormalRForm
                                          join f in db.RForm
                                          on x.RFormUniqueID equals f.UniqueID
                                          join t in db.RFormType
                                          on f.RFormTypeUniqueID equals t.UniqueID
                                          join e in db.Equipment
                                          on f.EquipmentUniqueID equals e.UniqueID into tmpEquipment
                                          from e in tmpEquipment.DefaultIfEmpty()
                                          join p in db.EquipmentPart
                                          on f.PartDescription equals p.UniqueID into tmpPart
                                          from p in tmpPart.DefaultIfEmpty()
                                          where x.AbnormalUniqueID == UniqueID
                                          select new
                                          {
                                              f.OrganizationUniqueID,
                                              f.MaintenanceOrganizationUniqueID,
                                              UniqueID = f.UniqueID,
                                              f.VHNO,
                                              f.Status,
                                              f.EstBeginDate,
                                              RepairFormType = t.Description,
                                              f.EstEndDate,
                                              f.Subject,
                                              EquipmentID = e != null ? e.ID : "",
                                              EquipmentName = e != null ? e.Name : "",
                                              PartDescription = p != null ? p.Description : ""
                                          }).ToList();

                    foreach (var form in repairFormList)
                    {
                        var repairFormModel = new RepairFormModel()
                        {
                            UniqueID = form.UniqueID,
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(form.OrganizationUniqueID),
                            MaintenanceOrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(form.MaintenanceOrganizationUniqueID),
                            VHNO = form.VHNO,
                            Subject = form.Subject,
                            EstBeginDate = form.EstBeginDate,
                            EstEndDate = form.EstEndDate,
                            Status = form.Status,
                            EquipmentID = form.EquipmentID,
                            EquipmentName = form.EquipmentName,
                            PartDescription = form.PartDescription,
                            RepairFormType = form.RepairFormType
                        };

                        var flow = db.RFormFlow.FirstOrDefault(x => x.RFormUniqueID == form.UniqueID);

                        if (flow != null)
                        {
                            repairFormModel.IsClosed = flow.IsClosed;
                        }
                        else
                        {
                            repairFormModel.IsClosed = false;
                        }

                        itemList.Add(repairFormModel);
                    }
                }
            }
            catch (Exception ex)
            {
                itemList = null;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return itemList;
        }
    }
}
