using System;
using System.Linq;
using System.Web.Mvc;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
#if ORACLE
using DbEntity.ORACLE;
using DbEntity.ORACLE.EquipmentMaintenance;
#else
using DbEntity.MSSQL;
using DbEntity.MSSQL.TruckPatrol;
#endif
using Models.Authenticated;
using Models.TruckPatrol.ResultQuery;

namespace DataAccess.TruckPatrol
{
    public class ResultQueryHelper
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

                        var tmpQueryResults = query.ToList();

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
                                TimeSpan = truckBindingResult.TimeSpan
                            });
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

        public static RequestResult Query(string TruckBindingUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = new List<ControlPointModel>();

                using (TDbEntities db = new TDbEntities())
                {
                    var arriveRecordList = db.ArriveRecord.Where(x => x.TruckBindingUniqueID == TruckBindingUniqueID).ToList();

                    var firstTruck = arriveRecordList.FirstOrDefault(x => x.TruckBindingType == "0" || x.TruckBindingType == "1");
                    var sencordTruck = arriveRecordList.FirstOrDefault(x => x.TruckBindingType == "2");

                    if (firstTruck != null)
                    {
                        var controlPointList = db.ControlPoint.Where(x => x.TruckUniqueID == firstTruck.TruckUniqueID).OrderBy(x => x.ID).ToList();

                        foreach (var controlPoint in controlPointList)
                        {
                            var controlPointModel = new ControlPointModel()
                            {
                                TruckBindingUniqueID = TruckBindingUniqueID,
                                ArriveRecordList = arriveRecordList.Where(x => x.ControlPointUniqueID == controlPoint.UniqueID).OrderBy(x => x.ArriveDate).ThenBy(x => x.ArriveTime).Select(x => new ArriveRecordModel
                                {
                                    UniqueID = x.UniqueID,
                                    ArriveDate = x.ArriveDate,
                                    ArriveTime = x.ArriveTime,
                                    UserID = x.UserID,
                                    UserName = x.UserName,
                                    UnRFIDReasonDescription = x.UnRFIDReasonDescription,
                                    UnRFIDReasonRemark = x.UnRFIDReasonRemark,
                                    PhotoList = db.ArriveRecordPhoto.Where(p => p.ArriveRecordUniqueID == x.UniqueID).Select(p => p.ArriveRecordUniqueID + "_" + p.Seq + "." + p.Extension).OrderBy(p => p).ToList()
                                }).ToList(),
                                UniqueID = controlPoint.UniqueID,
                                ID = controlPoint.ID,
                                Description = controlPoint.Description
                            };

                            var controlPointCheckItemList = (from x in db.View_ControlPointCheckItem
                                                             where x.ControlPointUniqueID == controlPoint.UniqueID
                                                             select new
                                                             {
                                                                 UniqueID = x.CheckItemUniqueID,
                                                                 x.ID,
                                                                 x.Description,
                                                                 x.LowerLimit,
                                                                 x.LowerAlertLimit,
                                                                 x.UpperAlertLimit,
                                                                 x.UpperLimit,
                                                                 x.Unit,
                                                                 x.Seq
                                                             }).OrderBy(x => x.Seq).ToList();

                            foreach (var checkItem in controlPointCheckItemList)
                            {
                                var checkItemModel = new CheckItemModel()
                                {
                                    CheckItemID = checkItem.ID,
                                    CheckItemDescription = checkItem.Description,
                                    LowerLimit = checkItem.LowerLimit,
                                    LowerAlertLimit = checkItem.LowerAlertLimit,
                                    UpperAlertLimit = checkItem.UpperAlertLimit,
                                    UpperLimit = checkItem.UpperLimit,
                                    Unit = checkItem.Unit
                                };

                                var checkResultList = (from c in db.CheckResult
                                                       join a in db.ArriveRecord
                                                       on c.ArriveRecordUniqueID equals a.UniqueID
                                                       where a.TruckBindingUniqueID == TruckBindingUniqueID && a.ControlPointUniqueID == controlPoint.UniqueID && c.CheckItemUniqueID == checkItem.UniqueID
                                                       select c).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

                                foreach (var checkResult in checkResultList)
                                {
                                    checkItemModel.CheckResultList.Add(new CheckResultModel()
                                    {
                                        ArriveRecordUniqueID = checkResult.ArriveRecordUniqueID,
                                        UniqueID = checkResult.UniqueID,
                                        CheckDate = checkResult.CheckDate,
                                        CheckTime = checkResult.CheckTime,
                                        IsAbnormal = checkResult.IsAbnormal,
                                        IsAlert = checkResult.IsAlert,
                                        Result = checkResult.Result,
                                        LowerLimit = checkResult.LowerLimit,
                                        LowerAlertLimit = checkResult.LowerAlertLimit,
                                        UpperAlertLimit = checkResult.UpperAlertLimit,
                                        UpperLimit = checkResult.UpperLimit,
                                        Unit = checkResult.Unit,
                                        PhotoList = db.CheckResultPhoto.Where(p => p.CheckResultUniqueID == checkResult.UniqueID).Select(p => p.CheckResultUniqueID + "_" + p.Seq + "." + p.Extension).ToList(),
                                        AbnormalReasonList = db.CheckResultAbnormalReason.Where(a => a.CheckResultUniqueID == checkResult.UniqueID).OrderBy(a => a.AbnormalReasonID).Select(a => new AbnormalReasonModel
                                        {
                                            Description = a.AbnormalReasonDescription,
                                            Remark = a.AbnormalReasonRemark,
                                            HandlingMethodList = db.CheckResultHandlingMethod.Where(h => h.CheckResultUniqueID == checkResult.UniqueID && h.AbnormalReasonUniqueID == a.AbnormalReasonUniqueID).OrderBy(h => h.HandlingMethodID).Select(h => new HandlingMethodModel
                                            {
                                                Description = h.HandlingMethodDescription,
                                                Remark = h.HandlingMethodRemark
                                            }).ToList()
                                        }).ToList()
                                    });
                                }

                                controlPointModel.CheckItemList.Add(checkItemModel);
                            }

                            itemList.Add(controlPointModel);
                        }
                    }

                    if (sencordTruck != null)
                    {
                        var controlPointList = db.ControlPoint.Where(x => x.TruckUniqueID == sencordTruck.TruckUniqueID).OrderBy(x => x.ID).ToList();

                        foreach (var controlPoint in controlPointList)
                        {
                            var controlPointModel = new ControlPointModel()
                            {
                                TruckBindingUniqueID = TruckBindingUniqueID,
                                ArriveRecordList = arriveRecordList.Where(x => x.ControlPointUniqueID == controlPoint.UniqueID).OrderBy(x => x.ArriveDate).ThenBy(x => x.ArriveTime).Select(x => new ArriveRecordModel
                                {
                                    UniqueID = x.UniqueID,
                                    ArriveDate = x.ArriveDate,
                                    ArriveTime = x.ArriveTime,
                                    UserID = x.UserID,
                                    UserName = x.UserName,
                                    UnRFIDReasonDescription = x.UnRFIDReasonDescription,
                                    UnRFIDReasonRemark = x.UnRFIDReasonRemark,
                                    PhotoList = db.ArriveRecordPhoto.Where(p => p.ArriveRecordUniqueID == x.UniqueID).Select(p => p.ArriveRecordUniqueID + "_" + p.Seq + "." + p.Extension).OrderBy(p => p).ToList()
                                }).ToList(),
                                UniqueID = controlPoint.UniqueID,
                                ID = controlPoint.ID,
                                Description = controlPoint.Description
                            };

                            var controlPointCheckItemList = (from x in db.View_ControlPointCheckItem
                                                             where x.ControlPointUniqueID == controlPoint.UniqueID
                                                             select new
                                                             {
                                                                 UniqueID = x.CheckItemUniqueID,
                                                                 x.ID,
                                                                 x.Description,
                                                                 x.LowerLimit,
                                                                 x.LowerAlertLimit,
                                                                 x.UpperAlertLimit,
                                                                 x.UpperLimit,
                                                                 x.Unit,
                                                                 x.Seq
                                                             }).OrderBy(x => x.Seq).ToList();

                            foreach (var checkItem in controlPointCheckItemList)
                            {
                                var checkItemModel = new CheckItemModel()
                                {
                                    CheckItemID = checkItem.ID,
                                    CheckItemDescription = checkItem.Description,
                                    LowerLimit = checkItem.LowerLimit,
                                    LowerAlertLimit = checkItem.LowerAlertLimit,
                                    UpperAlertLimit = checkItem.UpperAlertLimit,
                                    UpperLimit = checkItem.UpperLimit,
                                    Unit = checkItem.Unit
                                };

                                var checkResultList = (from c in db.CheckResult
                                                       join a in db.ArriveRecord
                                                       on c.ArriveRecordUniqueID equals a.UniqueID
                                                       where a.TruckBindingUniqueID == TruckBindingUniqueID && a.ControlPointUniqueID == controlPoint.UniqueID && c.CheckItemUniqueID == checkItem.UniqueID
                                                       select c).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

                                foreach (var checkResult in checkResultList)
                                {
                                    checkItemModel.CheckResultList.Add(new CheckResultModel()
                                    {
                                        ArriveRecordUniqueID = checkResult.ArriveRecordUniqueID,
                                        UniqueID = checkResult.UniqueID,
                                        CheckDate = checkResult.CheckDate,
                                        CheckTime = checkResult.CheckTime,
                                        IsAbnormal = checkResult.IsAbnormal,
                                        IsAlert = checkResult.IsAlert,
                                        Result = checkResult.Result,
                                        LowerLimit = checkResult.LowerLimit,
                                        LowerAlertLimit = checkResult.LowerAlertLimit,
                                        UpperAlertLimit = checkResult.UpperAlertLimit,
                                        UpperLimit = checkResult.UpperLimit,
                                        Unit = checkResult.Unit,
                                        PhotoList = db.CheckResultPhoto.Where(p => p.CheckResultUniqueID == checkResult.UniqueID).Select(p => p.CheckResultUniqueID + "_" + p.Seq + "." + p.Extension).ToList(),
                                        AbnormalReasonList = db.CheckResultAbnormalReason.Where(a => a.CheckResultUniqueID == checkResult.UniqueID).OrderBy(a => a.AbnormalReasonID).Select(a => new AbnormalReasonModel
                                        {
                                            Description = a.AbnormalReasonDescription,
                                            Remark = a.AbnormalReasonRemark,
                                            HandlingMethodList = db.CheckResultHandlingMethod.Where(h => h.CheckResultUniqueID == checkResult.UniqueID && h.AbnormalReasonUniqueID == a.AbnormalReasonUniqueID).OrderBy(h => h.HandlingMethodID).Select(h => new HandlingMethodModel
                                            {
                                                Description = h.HandlingMethodDescription,
                                                Remark = h.HandlingMethodRemark
                                            }).ToList()
                                        }).ToList()
                                    });
                                }

                                controlPointModel.CheckItemList.Add(checkItemModel);
                            }

                            itemList.Add(controlPointModel);
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

//        public static RequestResult Query(QueryParameters Parameters, Account Account)
//        {
//            RequestResult result = new RequestResult();

//            try
//            {
//                var itemList = new List<TruckModel>();

//                var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

//                using (TDbEntities db = new TDbEntities())
//                {
//                    var query = (from j in db.Job
//                                 join r in db.Route
//                                 on j.RouteUniqueID equals r.UniqueID
//                                 where downStreamOrganizationList.Contains(r.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(r.OrganizationUniqueID)
//                                 select new
//                                 {
//                                     Job = j,
//                                     Route = r
//                                 }).AsQueryable();

//                    if (!string.IsNullOrEmpty(Parameters.RouteUniqueID))
//                    {
//                        query = query.Where(x => x.Job.RouteUniqueID == Parameters.RouteUniqueID);
//                    }

//                    if (!string.IsNullOrEmpty(Parameters.JobUniqueID))
//                    {
//                        query = query.Where(x => x.Job.UniqueID == Parameters.JobUniqueID);
//                    }

//                    var jobList = query.ToList();

//                    var date = Parameters.BeginDate;
//                    var end = Parameters.EndDate;

//                    while (date <= end)
//                    {
//                        foreach (var job in jobList)
//                        {
//                            if (JobCycleHelper.IsInCycle(date, job.Job.BeginDate, job.Job.EndDate, job.Job.CycleCount, job.Job.CycleMode))
//                            {
//                                DateTime beginDate, endDate;

//                                JobCycleHelper.GetDateSpan(date, job.Job.BeginDate, job.Job.EndDate, job.Job.CycleCount, job.Job.CycleMode, out beginDate, out endDate);

//                                var beginDateString = DateTimeHelper.DateTime2DateString(beginDate);
//                                var endDateString = DateTimeHelper.DateTime2DateString(endDate);

//                                if (!itemList.Any(x => x.UniqueID == job.Job.UniqueID && x.BeginDate == beginDateString && x.EndDate == endDateString))
//                                {
//                                    var flow = db.CheckResultFlow.FirstOrDefault(x => x.JobUniqueID == job.Job.UniqueID && x.BeginDate == beginDateString && x.EndDate == endDateString);

//                                    //bool canVerify = false;

//                                    string nextVerifyUserID = string.Empty;
//                                    string nextVerifyUserName = string.Empty;

//                                    var allArriveRecordList = db.ArriveRecord.Where(x => x.JobUniqueID == job.Job.UniqueID && string.Compare(x.ArriveDate, beginDateString) >= 0 && string.Compare(x.ArriveDate, endDateString) <= 0).ToList();
//                                    var allCheckResultList = db.CheckResult.Where(x => x.JobUniqueID == job.Job.UniqueID && string.Compare(x.CheckDate, beginDateString) >= 0 && string.Compare(x.CheckDate, endDateString) <= 0).ToList();

//                                    var unPatrolRecord = db.UnPatrolRecord.FirstOrDefault(x => x.JobUniqueID == job.Job.UniqueID && x.BeginDate == beginDateString && x.EndDate == endDateString);

//                                    var item = new TruckModel()
//                                    {
//                                        UniqueID = job.Job.UniqueID,
//                                        CanVerify = false,
//                                        //CanVerify = canVerify,
//                                        NextVerifyUserID = nextVerifyUserID,
//                                        NextVerifyUserName=nextVerifyUserName,
//                                        BeginDate = beginDateString,
//                                        EndDate = endDateString,
//                                        RouteID = job.Route.ID,
//                                        RouteName = job.Route.Name,
//                                        JobDescription = job.Job.Description,
//                                        BeginTime = job.Job.BeginTime,
//                                        EndTime = job.Job.EndTime,
//                                        UnPatrolReasonDescription = unPatrolRecord != null ? unPatrolRecord.UnPatrolReasonDescription : "",
//                                        UnPatrolReasonRemark = unPatrolRecord != null ? unPatrolRecord.UnPatrolReasonRemark : "",
//                                        UserIDList = db.JobUser.Where(x => x.JobUniqueID == job.Job.UniqueID).Select(x => x.UserID).ToList()
//                                    };

//                                    var controlPointList = (from x in db.JobControlPoint
//                                                            join c in db.ControlPoint
//                                                            on x.ControlPointUniqueID equals c.UniqueID
//                                                            where x.JobUniqueID == job.Job.UniqueID
//                                                            select new
//                                                            {
//                                                                UniqueID = c.UniqueID,
//                                                                ID = c.ID,
//                                                                Description = c.Description,
//                                                                x.Seq
//                                                            }).OrderBy(x => x.Seq).ToList();

//                                    foreach (var controlPoint in controlPointList)
//                                    {
//                                        var controlPointModel = new ControlPointModel()
//                                        {
//                                            UniqueID = controlPoint.UniqueID,
//                                            JobUniqueID = job.Job.UniqueID,
//                                            BeginDate = beginDateString,
//                                            EndDate = endDateString,
//                                            ID = controlPoint.ID,
//                                            Description = controlPoint.Description,
//                                            ArriveRecordList = allArriveRecordList.Where(x => x.ControlPointUniqueID == controlPoint.UniqueID).OrderBy(x => x.ArriveDate).ThenBy(x => x.ArriveTime).Select(x => new ArriveRecordModel
//                                            {
//                                                UniqueID = x.UniqueID,
//                                                ArriveDate = x.ArriveDate,
//                                                ArriveTime = x.ArriveTime,
//                                                UserID = x.UserID,
//                                                UserName = x.UserName,
//                                                UnRFIDReasonDescription = x.UnRFIDReasonDescription,
//                                                UnRFIDReasonRemark = x.UnRFIDReasonRemark,
//                                                PhotoList = db.ArriveRecordPhoto.Where(p => p.ArriveRecordUniqueID == x.UniqueID).Select(p => p.ArriveRecordUniqueID + "_" + p.Seq + "." + p.Extension).OrderBy(p => p).ToList()
//                                            }).ToList()
//                                        };

//                                        var controlPointCheckItemList = (from x in db.JobControlPointCheckItem
//                                                                         join c in db.View_ControlPointCheckItem
//                                                                         on new { x.ControlPointUniqueID, x.CheckItemUniqueID } equals new { c.ControlPointUniqueID, c.CheckItemUniqueID }
//                                                                         where x.JobUniqueID == job.Job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID
//                                                                         select new
//                                                                         {
//                                                                             UniqueID = c.CheckItemUniqueID,
//                                                                             c.ID,
//                                                                             c.Description,
//                                                                             c.LowerLimit,
//                                                                             c.LowerAlertLimit,
//                                                                             c.UpperAlertLimit,
//                                                                             c.UpperLimit,
//                                                                             c.Unit,
//                                                                             x.Seq
//                                                                         }).OrderBy(x => x.Seq).ToList();

//                                        foreach (var checkItem in controlPointCheckItemList)
//                                        {
//                                            var checkItemModel = new CheckItemModel()
//                                            {
//                                                EquipmentID = "",
//                                                EquipmentName = "",
//                                                PartDescription = "",
//                                                CheckItemID = checkItem.ID,
//                                                CheckItemDescription = checkItem.Description,
//                                                LowerLimit = checkItem.LowerLimit,
//                                                LowerAlertLimit = checkItem.LowerAlertLimit,
//                                                UpperAlertLimit = checkItem.UpperAlertLimit,
//                                                UpperLimit = checkItem.UpperLimit,
//                                                Unit = checkItem.Unit
//                                            };

//                                            var checkResultList = allCheckResultList.Where(x => x.ControlPointUniqueID == controlPoint.UniqueID && string.IsNullOrEmpty(x.EquipmentUniqueID) && string.IsNullOrEmpty(x.PartUniqueID) && x.CheckItemUniqueID == checkItem.UniqueID).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

//                                            foreach (var checkResult in checkResultList)
//                                            {
//                                                var repairForm = db.RForm.FirstOrDefault(x => x.CheckResultUniqueID == checkResult.UniqueID);

//                                                checkItemModel.CheckResultList.Add(new CheckResultModel()
//                                                {
//                                                    ArriveRecordUniqueID = checkResult.ArriveRecordUniqueID,
//                                                    UniqueID = checkResult.UniqueID,
//                                                    CheckDate = checkResult.CheckDate,
//                                                    CheckTime = checkResult.CheckTime,
//#if ORACLE
//                                                    IsAbnormal = checkResult.IsAbnormal==1,
//                                                    IsAlert = checkResult.IsAlert==1,
//#else
//                                                    IsAbnormal = checkResult.IsAbnormal,
//                                                    IsAlert = checkResult.IsAlert,
//#endif
//                                                    Result = checkResult.Result,
//                                                    LowerLimit = checkResult.LowerLimit,
//                                                    LowerAlertLimit = checkResult.LowerAlertLimit,
//                                                    UpperAlertLimit = checkResult.UpperAlertLimit,
//                                                    UpperLimit = checkResult.UpperLimit,
//                                                    Unit = checkResult.Unit,
//                                                    PhotoList = db.CheckResultPhoto.Where(p => p.CheckResultUniqueID == checkResult.UniqueID).Select(p => p.CheckResultUniqueID + "_" + p.Seq + "." + p.Extension).ToList(),
//                                                    AbnormalReasonList = db.CheckResultAbnormalReason.Where(a => a.CheckResultUniqueID == checkResult.UniqueID).OrderBy(a => a.AbnormalReasonID).Select(a => new AbnormalReasonModel
//                                                    {
//                                                        Description = a.AbnormalReasonDescription,
//                                                        Remark = a.AbnormalReasonRemark,
//                                                        HandlingMethodList = db.CheckResultHandlingMethod.Where(h => h.CheckResultUniqueID == checkResult.UniqueID && h.AbnormalReasonUniqueID == a.AbnormalReasonUniqueID).OrderBy(h => h.HandlingMethodID).Select(h => new HandlingMethodModel
//                                                        {
//                                                            Description = h.HandlingMethodDescription,
//                                                            Remark = h.HandlingMethodRemark
//                                                        }).ToList()
//                                                    }).ToList(),
//                                                    RepairFormUniqueID = repairForm != null ? repairForm.UniqueID : "",
//                                                    RepairFormVHNO = repairForm != null ? repairForm.VHNO : ""
//                                                });
//                                            }

//                                            controlPointModel.CheckItemList.Add(checkItemModel);
//                                        }

//                                        var equipmentList = (from x in db.JobEquipment
//                                                             where x.JobUniqueID == job.Job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID
//                                                             select new { x.EquipmentUniqueID, x.PartUniqueID, x.Seq }).OrderBy(x => x.Seq).ToList();

//                                        foreach (var equipment in equipmentList)
//                                        {
//                                            var equipmentCheckItemList = (from x in db.JobEquipmentCheckItem
//                                                                          join e in db.Equipment
//                                                                          on x.EquipmentUniqueID equals e.UniqueID
//                                                                          join p in db.EquipmentPart
//                                                                          on new { x.EquipmentUniqueID, x.PartUniqueID } equals new { p.EquipmentUniqueID, PartUniqueID = p.UniqueID } into tmpPart
//                                                                          from p in tmpPart.DefaultIfEmpty()
//                                                                          join c in db.View_EquipmentCheckItem
//                                                                          on new { x.EquipmentUniqueID, x.PartUniqueID, x.CheckItemUniqueID } equals new { c.EquipmentUniqueID, c.PartUniqueID, c.CheckItemUniqueID }
//                                                                          where x.JobUniqueID == job.Job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == equipment.EquipmentUniqueID && x.PartUniqueID == equipment.PartUniqueID
//                                                                          select new
//                                                                          {
//                                                                              x.EquipmentUniqueID,
//                                                                              EquipmentID = e.ID,
//                                                                              EquipmentName = e.Name,
//                                                                              x.PartUniqueID,
//                                                                              PartDescription = p != null ? p.Description : "",
//                                                                              x.CheckItemUniqueID,
//                                                                              CheckItemID = c.ID,
//                                                                              CheckItemDescription = c.Description,
//                                                                              c.LowerLimit,
//                                                                              c.LowerAlertLimit,
//                                                                              c.UpperAlertLimit,
//                                                                              c.UpperLimit,
//                                                                              c.Unit,
//                                                                              x.Seq
//                                                                          }).OrderBy(x => x.Seq).ToList();

//                                            foreach (var checkItem in equipmentCheckItemList)
//                                            {
//                                                var checkItemModel = new CheckItemModel()
//                                                {
//                                                    EquipmentID = checkItem.EquipmentID,
//                                                    EquipmentName = checkItem.EquipmentName,
//                                                    PartDescription = checkItem.PartDescription,
//                                                    CheckItemID = checkItem.CheckItemID,
//                                                    CheckItemDescription = checkItem.CheckItemDescription,
//                                                    LowerLimit = checkItem.LowerLimit,
//                                                    LowerAlertLimit = checkItem.LowerAlertLimit,
//                                                    UpperAlertLimit = checkItem.UpperAlertLimit,
//                                                    UpperLimit = checkItem.UpperLimit,
//                                                    Unit = checkItem.Unit
//                                                };

//                                                var checkResultList = allCheckResultList.Where(x => x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == checkItem.EquipmentUniqueID && x.PartUniqueID == checkItem.PartUniqueID && x.CheckItemUniqueID == checkItem.CheckItemUniqueID).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

//                                                foreach (var checkResult in checkResultList)
//                                                {
//                                                    var repairForm = db.RForm.FirstOrDefault(x => x.CheckResultUniqueID == checkResult.UniqueID);

//                                                    checkItemModel.CheckResultList.Add(new CheckResultModel()
//                                                    {
//                                                        ArriveRecordUniqueID = checkResult.ArriveRecordUniqueID,
//                                                        UniqueID = checkResult.UniqueID,
//                                                        CheckDate = checkResult.CheckDate,
//                                                        CheckTime = checkResult.CheckTime,
//#if ORACLE
//                                                    IsAbnormal = checkResult.IsAbnormal==1,
//                                                    IsAlert = checkResult.IsAlert==1,
//#else
//                                                        IsAbnormal = checkResult.IsAbnormal,
//                                                        IsAlert = checkResult.IsAlert,
//#endif
//                                                        Result = checkResult.Result,
//                                                        LowerLimit = checkResult.LowerLimit,
//                                                        LowerAlertLimit = checkResult.LowerAlertLimit,
//                                                        UpperAlertLimit = checkResult.UpperAlertLimit,
//                                                        UpperLimit = checkResult.UpperLimit,
//                                                        Unit = checkResult.Unit,
//                                                        PhotoList = db.CheckResultPhoto.Where(p => p.CheckResultUniqueID == checkResult.UniqueID).Select(p => p.CheckResultUniqueID + "_" + p.Seq + "." + p.Extension).ToList(),
//                                                        AbnormalReasonList = db.CheckResultAbnormalReason.Where(a => a.CheckResultUniqueID == checkResult.UniqueID).OrderBy(a => a.AbnormalReasonID).Select(a => new AbnormalReasonModel
//                                                        {
//                                                            Description = a.AbnormalReasonDescription,
//                                                            Remark = a.AbnormalReasonRemark,
//                                                            HandlingMethodList = db.CheckResultHandlingMethod.Where(h => h.CheckResultUniqueID == checkResult.UniqueID && h.AbnormalReasonUniqueID == a.AbnormalReasonUniqueID).OrderBy(h => h.HandlingMethodID).Select(h => new HandlingMethodModel
//                                                            {
//                                                                Description = h.HandlingMethodDescription,
//                                                                Remark = h.HandlingMethodRemark
//                                                            }).ToList()
//                                                        }).ToList(),
//                                                        RepairFormUniqueID = repairForm != null ? repairForm.UniqueID : "",
//                                                        RepairFormVHNO = repairForm != null ? repairForm.VHNO : ""
//                                                    });
//                                                }

//                                                controlPointModel.CheckItemList.Add(checkItemModel);
//                                            }
//                                        }

//                                        item.ControlPointList.Add(controlPointModel);
//                                    }

//                                    itemList.Add(item);
//                                }
//                            }
//                        }

//                        date = date.AddDays(1);
//                    }
//                }

//                using (DbEntities db = new DbEntities())
//                {
//                    foreach (var item in itemList)
//                    {
//                        item.UserList = (from x in item.UserIDList
//                                         join u in db.User
//                                         on x equals u.ID
//                                         select string.Format("{0}/{1}", u.ID, u.Name)).OrderBy(x => x).ToList();
//                    }
//                }

//                result.ReturnData(itemList.OrderBy(x => x.BeginDate).ThenBy(x => x.BeginTime).ThenBy(x => x.Description).ToList());
//            }
//            catch (Exception ex)
//            {
//                var err = new Error(MethodBase.GetCurrentMethod(), ex);

//                Logger.Log(err);

//                result.ReturnError(err);
//            }

//            return result;
//        }

//        public static RequestResult Query(Account Account)
//        {
//            RequestResult result = new RequestResult();

//            try
//            {
//                var parameters = new QueryParameters()
//                {
//                    BeginDateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today),
//                    EndDateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today),
//                    OrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(Account.OrganizationUniqueID)
//                };

//                result = Query(parameters, Account);
//            }
//            catch (Exception ex)
//            {
//                var err = new Error(MethodBase.GetCurrentMethod(), ex);

//                Logger.Log(err);

//                result.ReturnError(err);
//            }

//            return result;
//        }

        //public static RequestResult GetEditFormModel(string JobUniqueID, string BeginDate, string EndDate)
        //{
        //    RequestResult result = new RequestResult();

        //    try
        //    {
        //        using (TDbEntities db = new TDbEntities())
        //        {
        //            var record = db.UnPatrolRecord.FirstOrDefault(x => x.JobUniqueID == JobUniqueID && x.BeginDate == BeginDate && x.EndDate == EndDate);

        //            var model = new EditFormModel()
        //            {
        //                JobUniqueID = JobUniqueID,
        //                BeginDate = BeginDate,
        //                EndDate = EndDate,
        //                FormInput = new FormInput()
        //                {
        //                    UnPatrolReasonUniqueID = record != null ? record.UnPatrolReasonUniqueID : "",
        //                    UnPatrolReasonRemark = record != null ? record.UnPatrolReasonRemark : ""
        //                },
        //                UnPatrolReasonSelectItemList = new List<SelectListItem>() 
        //                { 
        //                    Define.DefaultSelectListItem(Resources.Resource.SelectOne)
        //                }
        //            };

        //            var reasonList = db.UnPatrolReason.OrderBy(x => x.ID).ToList();

        //            foreach (var reason in reasonList)
        //            {
        //                model.UnPatrolReasonSelectItemList.Add(new SelectListItem()
        //                {
        //                    Value = reason.UniqueID,
        //                    Text = reason.Description
        //                });
        //            }

        //            model.UnPatrolReasonSelectItemList.Add(new SelectListItem()
        //            {
        //                Value = "OTHER",
        //                Text = Resources.Resource.Other
        //            });

        //            result.ReturnData(model);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        var err = new Error(MethodBase.GetCurrentMethod(), ex);

        //        Logger.Log(err);

        //        result.ReturnError(err);
        //    }

        //    return result;
        //}

        //public static RequestResult Edit(EditFormModel Model, Account Account)
        //{
        //    RequestResult result = new RequestResult();

        //    try
        //    {
        //        using (EDbEntities db = new EDbEntities())
        //        {
        //            var record = db.UnPatrolRecord.FirstOrDefault(x => x.JobUniqueID == Model.JobUniqueID && x.BeginDate == Model.BeginDate && x.EndDate == Model.EndDate);

        //            var reason = db.UnPatrolReason.FirstOrDefault(x => x.UniqueID == Model.FormInput.UnPatrolReasonUniqueID);

        //            if (record == null)
        //            {
        //                db.UnPatrolRecord.Add(new UnPatrolRecord()
        //                {
        //                    JobUniqueID = Model.JobUniqueID,
        //                    BeginDate = Model.BeginDate,
        //                    EndDate = Model.EndDate,
        //                    UnPatrolReasonUniqueID = Model.FormInput.UnPatrolReasonUniqueID,
        //                    UnPatrolReasonID = reason != null ? reason.ID : "",
        //                    UnPatrolReasonDescription = reason != null ? reason.Description : "",
        //                    UnPatrolReasonRemark = Model.FormInput.UnPatrolReasonRemark,
        //                    UserID = Account.ID,
        //                    LastModifyTime = DateTime.Now
        //                });
        //            }
        //            else
        //            {
        //                record.UnPatrolReasonUniqueID = Model.FormInput.UnPatrolReasonUniqueID;
        //                record.UnPatrolReasonID = reason != null ? reason.ID : "";
        //                record.UnPatrolReasonDescription = reason != null ? reason.Description : "";
        //                record.UnPatrolReasonRemark = Model.FormInput.UnPatrolReasonRemark;
        //                record.UserID = Account.ID;
        //                record.LastModifyTime = DateTime.Now;
        //            }

        //            db.SaveChanges();

        //            result.Success();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        var err = new Error(MethodBase.GetCurrentMethod(), ex);

        //        Logger.Log(err);

        //        result.ReturnError(err);
        //    }

        //    return result;
        //}

        public static RequestResult Export(List<TruckBindingResultModel> TruckList, Define.EnumExcelVersion ExcelVersion)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ExcelHelper helper = new ExcelHelper(string.Format("{0}_{1}({2})", "巡檢結果", Resources.Resource.ExportTime, DateTimeHelper.DateTime2DateTimeString(DateTime.Now)), ExcelVersion))
                {
                    var truckBindingResultList = new List<TruckBindingResultExcelItem>();

                    var controlPointExcelItemList = new List<ControlPointExcelItem>();

                    var checkItemExcelItemList = new List<CheckItemExcelItem>();

                    foreach (var truck in TruckList)
                    {
                        truckBindingResultList.Add(new TruckBindingResultExcelItem()
                        {
                            Abnormal = truck.HaveAbnormal ? Resources.Resource.Abnormal : (truck.HaveAlert ? Resources.Resource.Warning : ""),
                            CarNo = truck.FirstTruckNo,
                            SecondCarNo = truck.SecondTruckNo,
                            CheckDate = truck.CheckDate,
                            CheckUser = truck.CheckUser,
                            ComplateRate = truck.CompleteRate,
                            TimeSpan = truck.TimeSpan
                        });

                       var controlPointList =Query(truck.BindingUniqueID).Data as List<ControlPointModel>;

                        foreach (var controlPoint in controlPointList)
                        {
                            if (controlPoint.ArriveRecordList.Count > 0)
                            {
                                foreach (var arriveRecord in controlPoint.ArriveRecordList)
                                {
                                    controlPointExcelItemList.Add(new ControlPointExcelItem()
                                    {
                                        Abnormal = controlPoint.HaveAbnormal ? Resources.Resource.Abnormal : (controlPoint.HaveAlert ? Resources.Resource.Warning : ""),
                                        CarNo = truck.FirstTruckNo,
                                        SecondCarNo = truck.SecondTruckNo,
                                        ControlPoint = controlPoint.ControlPoint,
                                        CompleteRate = controlPoint.CompleteRate,
                                        TimeSpan = controlPoint.TimeSpan,
                                        ArriveDate = arriveRecord.ArriveDate,
                                        ArriveTime = arriveRecord.ArriveTime,
                                        User = arriveRecord.User,
                                        UnRFIDReason = arriveRecord.UnRFIDReason
                                    });
                                }
                            }
                            else
                            {
                                controlPointExcelItemList.Add(new ControlPointExcelItem()
                                {
                                    Abnormal = controlPoint.HaveAbnormal ? Resources.Resource.Abnormal : (controlPoint.HaveAlert ? Resources.Resource.Warning : ""),
                                    CarNo = truck.FirstTruckNo,
                                    SecondCarNo = truck.SecondTruckNo,
                                    ControlPoint = controlPoint.ControlPoint,
                                    CompleteRate = controlPoint.CompleteRate,
                                    TimeSpan = controlPoint.TimeSpan,
                                    ArriveDate = string.Empty,
                                    ArriveTime = string.Empty,
                                    User = string.Empty,
                                    UnRFIDReason = string.Empty
                                });
                            }

                            foreach (var checkItem in controlPoint.CheckItemList)
                            {
                                if (checkItem.CheckResultList.Count > 0)
                                {
                                    foreach (var checkResult in checkItem.CheckResultList)
                                    {
                                        var arriveRecord = controlPoint.ArriveRecordList.FirstOrDefault(x => x.UniqueID == checkResult.ArriveRecordUniqueID);

                                        checkItemExcelItemList.Add(new CheckItemExcelItem()
                                        {
                                            Abnormal = checkResult.IsAbnormal ? Resources.Resource.Abnormal : (checkResult.IsAlert ? Resources.Resource.Warning : ""),
                                            CarNo = truck.FirstTruckNo,
                                            SecondCarNo = truck.SecondTruckNo,
                                            ControlPoint = controlPoint.ControlPoint,
                                            CheckItem = checkItem.CheckItem,
                                            CheckDate = checkResult.CheckDate,
                                            CheckTime = checkResult.CheckTime,
                                            Result = checkResult.Result,
                                            LowerLimit = checkResult.LowerLimit.HasValue ? checkResult.LowerLimit.Value.ToString() : "",
                                            LowerAlertLimit = checkItem.LowerAlertLimit.HasValue ? checkItem.LowerAlertLimit.Value.ToString() : "",
                                            UpperAlertLimit = checkItem.UpperAlertLimit.HasValue ? checkItem.UpperAlertLimit.Value.ToString() : "",
                                            UpperLimit = checkResult.UpperLimit.HasValue ? checkResult.UpperLimit.Value.ToString() : "",
                                            Unit = checkResult.Unit,
                                            User = arriveRecord != null ? arriveRecord.User : "",
                                            AbnormalReasons = checkResult.AbnormalReasons
                                        });
                                    }
                                }
                                else
                                {
                                    checkItemExcelItemList.Add(new CheckItemExcelItem()
                                    {
                                        Abnormal = "",
                                        CarNo = truck.FirstTruckNo,
                                        SecondCarNo = truck.SecondTruckNo,
                                        ControlPoint = controlPoint.ControlPoint,
                                        CheckItem = checkItem.CheckItem,
                                        CheckDate = string.Empty,
                                        CheckTime = string.Empty,
                                        Result = string.Empty,
                                        LowerLimit = checkItem.LowerLimit.HasValue ? checkItem.LowerLimit.Value.ToString() : "",
                                        LowerAlertLimit = checkItem.LowerAlertLimit.HasValue ? checkItem.LowerAlertLimit.Value.ToString() : "",
                                        UpperAlertLimit = checkItem.UpperAlertLimit.HasValue ? checkItem.UpperAlertLimit.Value.ToString() : "",
                                        UpperLimit = checkItem.UpperLimit.HasValue ? checkItem.UpperLimit.Value.ToString() : "",
                                        Unit = checkItem.Unit,
                                        User = string.Empty,
                                        AbnormalReasons = string.Empty
                                    });
                                }
                            }
                        }
                    }

                    helper.CreateSheet<TruckBindingResultExcelItem>(Resources.Resource.PatrolStatus, truckBindingResultList);
                    helper.CreateSheet<ControlPointExcelItem>(Resources.Resource.ArriveRecord, controlPointExcelItemList);
                    helper.CreateSheet<CheckItemExcelItem>(Resources.Resource.CheckResult, checkItemExcelItemList);

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
