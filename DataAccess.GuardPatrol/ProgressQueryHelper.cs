using DbEntity.MSSQL;
using DbEntity.MSSQL.GuardPatrol;
using Models.Authenticated;
using Models.GuardPatrol.ProgressQuery;
using Models.GuardPatrol.ProgressQuery.ExcelExport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace DataAccess.GuardPatrol
{
    public class ProgressQueryHelper
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = new List<JobRouteModel>();

                var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                using (GDbEntities db = new GDbEntities())
                {
                    var query = (from x in db.JobRoute
                                 join j in db.Job
                                 on x.JobUniqueID equals j.UniqueID
                                 join r in db.Route
                                 on x.RouteUniqueID equals r.UniqueID
                                 where downStreamOrganizationList.Contains(j.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(r.OrganizationUniqueID)
                                 select new
                                 {
                                     JobRouteUniqueID = x.UniqueID,
                                     x.IsOptional,
                                     x.JobUniqueID,
                                     x.RouteUniqueID,
                                     RouteID=r.ID,
                                     RouteName=r.Name,
                                     JobDescription = j.Description,
                                     j.TimeMode,
                                     j.BeginDate,
                                     j.EndDate,
                                     j.BeginTime,
                                     j.EndTime,
                                     j.CycleCount,
                                     j.CycleMode
                                 }).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.JobUniqueID))
                    {
                        query = query.Where(x => x.JobUniqueID == Parameters.JobUniqueID);
                    }

                    var jobRouteList = query.ToList();

                    var date = Parameters.BeginDate;
                    var end = Parameters.EndDate;

                    while (date <= end)
                    {
                        foreach (var jobRoute in jobRouteList)
                        {
                            if (JobCycleHelper.IsInCycle(date, jobRoute.BeginDate, jobRoute.EndDate, jobRoute.CycleCount, jobRoute.CycleMode))
                            {
                                DateTime beginDate, endDate;

                                JobCycleHelper.GetDateSpan(date, jobRoute.BeginDate, jobRoute.EndDate, jobRoute.CycleCount, jobRoute.CycleMode, out beginDate, out endDate);

                                var beginDateString = DateTimeHelper.DateTime2DateString(beginDate);
                                var endDateString = DateTimeHelper.DateTime2DateString(endDate);

                                if (!itemList.Any(x => x.UniqueID == jobRoute.JobRouteUniqueID && x.BeginDate == beginDateString && x.EndDate == endDateString))
                                {
                                    var allArriveRecordList = db.ArriveRecord.Where(x => x.JobUniqueID == jobRoute.JobUniqueID && x.RouteUniqueID==jobRoute.RouteUniqueID && string.Compare(x.ArriveDate, beginDateString) >= 0 && string.Compare(x.ArriveDate, endDateString) <= 0).ToList();
                                    var allCheckResultList = db.CheckResult.Where(x => x.JobUniqueID == jobRoute.JobUniqueID && x.RouteUniqueID==jobRoute.RouteUniqueID && string.Compare(x.CheckDate, beginDateString) >= 0 && string.Compare(x.CheckDate, endDateString) <= 0).ToList();

                                    var unPatrolRecord = db.UnPatrolRecord.FirstOrDefault(x => x.JobUniqueID == jobRoute.JobUniqueID && x.RouteUniqueID == jobRoute.RouteUniqueID && x.BeginDate == beginDateString && x.EndDate == endDateString);

                                    var item = new JobRouteModel()
                                    {
                                        UniqueID = jobRoute.JobRouteUniqueID,
                                        JobUniqueID = jobRoute.JobUniqueID,
                                        RouteUniqueID = jobRoute.RouteUniqueID,
                                        BeginDate = beginDateString,
                                        EndDate = endDateString,
                                        IsOptional = jobRoute.IsOptional,
                                        RouteID = jobRoute.RouteID,
                                        RouteName = jobRoute.RouteName,
                                        JobDescription = jobRoute.JobDescription,
                                        TimeMode = jobRoute.TimeMode,
                                        BeginTime = jobRoute.BeginTime,
                                        EndTime = jobRoute.EndTime,
                                        UnPatrolReasonDescription = unPatrolRecord != null ? unPatrolRecord.UnPatrolReasonDescription : "",
                                        UnPatrolReasonRemark = unPatrolRecord != null ? unPatrolRecord.UnPatrolReasonRemark : "",
                                        JobUserIDList = db.JobUser.Where(x => x.JobUniqueID == jobRoute.JobUniqueID).Select(x => x.UserID).ToList()
                                    };

                                    var controlPointList = (from x in db.JobControlPoint
                                                            join y in db.RouteControlPoint
                                                            on new { x.RouteUniqueID, x.ControlPointUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID }
                                                            join c in db.ControlPoint
                                                            on x.ControlPointUniqueID equals c.UniqueID
                                                            where x.JobUniqueID == jobRoute.JobUniqueID && x.RouteUniqueID == jobRoute.RouteUniqueID
                                                            select new
                                                            {
                                                                UniqueID = c.UniqueID,
                                                                ID = c.ID,
                                                                Description = c.Description,
                                                                x.MinTimeSpan,
                                                                y.Seq
                                                            }).OrderBy(x => x.Seq).ToList();

                                    foreach (var controlPoint in controlPointList)
                                    {
                                        var controlPointModel = new ControlPointModel()
                                        {
                                            UniqueID = controlPoint.UniqueID,
                                            JobRouteUniqueID = jobRoute.JobRouteUniqueID,
                                            BeginDate = beginDateString,
                                            EndDate = endDateString,
                                            ID = controlPoint.ID,
                                            Description = controlPoint.Description,
                                            MinTimeSpan = controlPoint.MinTimeSpan,
                                            ArriveRecordList = allArriveRecordList.Where(x => x.ControlPointUniqueID == controlPoint.UniqueID).OrderBy(x => x.ArriveDate).ThenBy(x => x.ArriveTime).Select(x => new ArriveRecordModel
                                            {
                                                UniqueID = x.UniqueID,
                                                ArriveDate = x.ArriveDate,
                                                ArriveTime = x.ArriveTime,
                                                UserID = x.UserID,
                                                UserName = x.UserName,
                                                UnRFIDReasonDescription = x.UnRFIDReasonDescription,
                                                UnRFIDReasonRemark = x.UnRFIDReasonRemark,
                                                TimeSpanAbnormalReasonDescription = x.TimeSpanAbnormalReasonDescription,
                                                TimeSpanAbnormalReasonRemark = x.TimeSpanAbnormalReasonRemark,
                                                PhotoList = db.ArriveRecordPhoto.Where(p => p.ArriveRecordUniqueID == x.UniqueID).Select(p => p.ArriveRecordUniqueID + "_" + p.Seq + "." + p.Extension).OrderBy(p => p).ToList()
                                            }).ToList()
                                        };

                                        var controlPointCheckItemList = (from x in db.JobControlPointCheckItem
                                                                         join y in db.RouteControlPointCheckItem
                                                                         on new { x.RouteUniqueID, x.ControlPointUniqueID, x.CheckItemUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.CheckItemUniqueID }
                                                                         join c in db.View_ControlPointCheckItem
                                                                         on new { x.ControlPointUniqueID, x.CheckItemUniqueID } equals new { c.ControlPointUniqueID, c.CheckItemUniqueID }
                                                                         where x.JobUniqueID == jobRoute.JobUniqueID && x.RouteUniqueID == jobRoute.RouteUniqueID && x.ControlPointUniqueID == controlPoint.UniqueID
                                                                         select new
                                                                         {
                                                                             UniqueID = c.CheckItemUniqueID,
                                                                             c.ID,
                                                                             c.Description,
                                                                             c.LowerLimit,
                                                                             c.LowerAlertLimit,
                                                                             c.UpperAlertLimit,
                                                                             c.UpperLimit,
                                                                             c.Unit,
                                                                             y.Seq
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

                                            var checkResultList = allCheckResultList.Where(x => x.ControlPointUniqueID == controlPoint.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

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

                                        item.ControlPointList.Add(controlPointModel);
                                    }

                                    itemList.Add(item);
                                }
                            }
                        }

                        date = date.AddDays(1);
                    }
                }

                using (DbEntities db = new DbEntities())
                {
                    foreach (var item in itemList)
                    {
                        item.JobUserList = (from x in item.JobUserIDList
                                            join u in db.User
                                            on x equals u.ID
                                            select string.Format("{0}/{1}", u.ID, u.Name)).OrderBy(x => x).ToList();
                    }
                }

                result.ReturnData(itemList.OrderBy(x => x.BeginDate).ThenBy(x => x.BeginTime).ThenBy(x => x.Description).ToList());
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult Export(List<JobRouteModel> Model, Define.EnumExcelVersion ExcelVersion)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ExcelHelper helper = new ExcelHelper(string.Format("{0}_{1}({2})", Resources.Resource.CheckResult, Resources.Resource.ExportTime, DateTimeHelper.DateTime2DateTimeString(DateTime.Now)), ExcelVersion))
                {
                    var jobExcelItemList = new List<JobExcelItem>();

                    var controlPointExcelItemList = new List<ControlPointExcelItem>();

                    var checkItemExcelItemList = new List<CheckItemExcelItem>();

                    foreach (var job in Model)
                    {
                        jobExcelItemList.Add(new JobExcelItem()
                        {
                            IsAbnormal = job.HaveAbnormal ? Resources.Resource.Abnormal : (job.HaveAlert ? Resources.Resource.Warning : ""),
                            JobDescription = job.Description,
                            BeginDate = job.BeginDate,
                            EndDate = job.EndDate,
                            BeginTime = job.BeginTime,
                            EndTime = job.EndTime,
                            CompleteRate = job.CompleteRate,
                            TimeSpan = job.TimeSpan,
                            User = job.JobUsers,
                            UnPatrolReason = job.UnPatrolReason,
                            ArriveStatus = job.ArriveStatus,
                            OverTimeReason = job.OverTimeReason
                        });

                        foreach (var controlPoint in job.ControlPointList)
                        {
                            if (controlPoint.ArriveRecordList.Count > 0)
                            {
                                foreach (var arriveRecord in controlPoint.ArriveRecordList)
                                {
                                    controlPointExcelItemList.Add(new ControlPointExcelItem()
                                    {
                                        JobDescription = job.Description,
                                        BeginDate = job.BeginDate,
                                        EndDate = job.EndDate,
                                        BeginTime = job.BeginTime,
                                        EndTime = job.EndTime,
                                        IsAbnormal = controlPoint.HaveAbnormal ? Resources.Resource.Abnormal : (controlPoint.HaveAlert ? Resources.Resource.Warning : ""),
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
                                    JobDescription = job.Description,
                                    BeginDate = job.BeginDate,
                                    EndDate = job.EndDate,
                                    BeginTime = job.BeginTime,
                                    EndTime = job.EndTime,
                                    IsAbnormal = controlPoint.HaveAbnormal ? Resources.Resource.Abnormal : (controlPoint.HaveAlert ? Resources.Resource.Warning : ""),
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
                                            JobDescription = job.Description,
                                            BeginDate = job.BeginDate,
                                            EndDate = job.EndDate,
                                            BeginTime = job.BeginTime,
                                            EndTime = job.EndTime,
                                            ControlPoint = controlPoint.ControlPoint,
                                            IsAbnormal = checkResult.IsAbnormal ? Resources.Resource.Abnormal : (checkResult.IsAlert ? Resources.Resource.Warning : ""),
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
                                        JobDescription = job.Description,
                                        BeginDate = job.BeginDate,
                                        EndDate = job.EndDate,
                                        BeginTime = job.BeginTime,
                                        EndTime = job.EndTime,
                                        ControlPoint = controlPoint.ControlPoint,
                                        IsAbnormal = "",
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

                    helper.CreateSheet(Resources.Resource.PatrolStatus, new Dictionary<string, Utility.ExcelHelper.ExcelDisplayItem>() 
                    {
                        { "IsAbnormal", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.Abnormal, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "JobDescription", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Route, Resources.Resource.Job), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "BeginDate", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.BeginDate), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "EndDate", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.EndDate), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "BeginTime", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.BeginTime), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "EndTime", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.EndTime), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "CompleteRate", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.CompleteRate, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "TimeSpan", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.TimeSpan, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "User", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.JobUser, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "UnPatrolReason", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.UnPatrolReason, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "ArriveStatus", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.ArriveStatus, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "OverTimeReason", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.OverTimeReason, CellType = NPOI.SS.UserModel.CellType.String }}
                    }, jobExcelItemList);

                    helper.CreateSheet(Resources.Resource.ArriveRecord, new Dictionary<string, Utility.ExcelHelper.ExcelDisplayItem>() 
                    {
                        { "JobDescription", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Route, Resources.Resource.Job), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "BeginDate", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.BeginDate), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "EndDate", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.EndDate), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "BeginTime", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.BeginTime), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "EndTime", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.EndTime), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "IsAbnormal", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.Abnormal, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "ControlPoint", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.ControlPoint, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "CompleteRate", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.CompleteRate, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "TimeSpan", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.TimeSpan, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "ArriveDate", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.ArriveDate, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "ArriveTime", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.ArriveTime, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "User", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.ArriveUser, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "UnRFIDReason", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.UnRFIDReason, CellType = NPOI.SS.UserModel.CellType.String }}
                    }, controlPointExcelItemList);

                    helper.CreateSheet(Resources.Resource.CheckResult, new Dictionary<string, Utility.ExcelHelper.ExcelDisplayItem>() 
                    {
                        { "JobDescription", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Route, Resources.Resource.Job), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "BeginDate", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.BeginDate), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "EndDate", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.EndDate), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "BeginTime", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.BeginTime), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "EndTime", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.EndTime), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "ControlPoint", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.ControlPoint, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "IsAbnormal", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.Abnormal, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "CheckItem", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.CheckItem, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "CheckDate", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.CheckDate, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "CheckTime", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.CheckTime, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "Result", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.CheckResult, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "LowerLimit", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.LowerLimit, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "LowerAlertLimit", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.LowerAlertLimit, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "UpperAlertLimit", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.UpperAlertLimit, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "UpperLimit", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.UpperLimit, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "Unit", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.Unit, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "User", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.CheckUser, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "AbnormalReasons", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1} {2}", Resources.Resource.AbnormalReason, Resources.Resource.And, Resources.Resource.HandlingMethod), CellType = NPOI.SS.UserModel.CellType.String }}
                    }, checkItemExcelItemList);

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
