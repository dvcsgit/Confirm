using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Models;
using Models.EquipmentMaintenance.ProgressQuery;
using Models.Authenticated;
using DataAccess;
using DbEntity.MSSQL.EquipmentMaintenance;
using DataAccess.EquipmentMaintenance;
using Utility;
using System.Reflection;
using Models.Shared;

namespace Customized.CHIMEI.DataAccess
{
    public class ProgressQueryHelper
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = new List<JobResultModel>();

                var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                using (EDbEntities db = new EDbEntities())
                {
                    var query = (from j in db.Job
                                 join r in db.Route
                                 on j.RouteUniqueID equals r.UniqueID
                                 where downStreamOrganizationList.Contains(r.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(r.OrganizationUniqueID)
                                 select new
                                 {
                                     Job = j,
                                     Route = r
                                 }).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.RouteUniqueID))
                    {
                        query = query.Where(x => x.Job.RouteUniqueID == Parameters.RouteUniqueID);
                    }

                    if (!string.IsNullOrEmpty(Parameters.JobUniqueID))
                    {
                        query = query.Where(x => x.Job.UniqueID == Parameters.JobUniqueID);
                    }

                    var jobList = query.ToList();

                    var date = Parameters.BeginDate;
                    var end = Parameters.EndDate;

                    while (date <= end)
                    {
                        foreach (var job in jobList)
                        {
                            if (JobCycleHelper.IsInCycle(date, job.Job.BeginDate, job.Job.EndDate, job.Job.CycleCount, job.Job.CycleMode))
                            {
                                DateTime beginDate, endDate;

                                JobCycleHelper.GetDateSpan(date, job.Job.BeginDate, job.Job.EndDate, job.Job.CycleCount, job.Job.CycleMode, out beginDate, out endDate);

                                var beginDateString = DateTimeHelper.DateTime2DateString(beginDate);
                                var endDateString = DateTimeHelper.DateTime2DateString(endDate);

                                if (!string.IsNullOrEmpty(job.Job.BeginTime) && !string.IsNullOrEmpty(job.Job.EndTime) && string.Compare(job.Job.BeginTime, job.Job.EndTime) > 0)
                                {
                                    endDateString = DateTimeHelper.DateTime2DateString(endDate.AddDays(1));
                                }

                                var jobResult = db.JobResult.FirstOrDefault(x => x.JobUniqueID == job.Job.UniqueID && x.BeginDate == beginDateString && x.EndDate == endDateString);

                                if (jobResult == null)
                                {
                                    JobResultDataAccessor.Refresh(Guid.NewGuid().ToString(), job.Job.UniqueID, beginDateString, endDateString);

                                    jobResult = db.JobResult.FirstOrDefault(x => x.JobUniqueID == job.Job.UniqueID && x.BeginDate == beginDateString && x.EndDate == endDateString);
                                }

                                if (jobResult != null && !itemList.Any(x => x.UniqueID == jobResult.UniqueID))
                                {
                                    var allArriveRecordList = db.ArriveRecord.Where(x => x.JobResultUniqueID == jobResult.UniqueID).ToList();

                                    var allCheckResultList = (from c in db.CheckResult
                                                              join a in db.ArriveRecord
                                                              on c.ArriveRecordUniqueID equals a.UniqueID
                                                              where a.JobResultUniqueID == jobResult.UniqueID
                                                              select c).ToList();

                                    var item = new JobResultModel()
                                    {
                                        UniqueID = jobResult.UniqueID,
                                        OrganizationDescription = jobResult.OrganizationDescription,
                                        JobUniqueID = jobResult.JobUniqueID,
                                        BeginDate = jobResult.BeginDate,
                                        EndDate = jobResult.EndDate,
                                        BeginTime = jobResult.BeginTime,
                                        EndTime = jobResult.EndTime,
                                        ArriveStatus = jobResult.ArriveStatus,
                                        ArriveStatusLabelClass = jobResult.ArriveStatusLabelClass,
                                        CheckUsers = jobResult.CheckUsers,
                                        CompleteRate = jobResult.CompleteRate,
                                        CompleteRateLabelClass = jobResult.CompleteRateLabelClass,
                                        Description = jobResult.Description,
                                        HaveAbnormal = jobResult.HaveAbnormal,
                                        HaveAlert = jobResult.HaveAlert,
                                        JobUsers = jobResult.JobUsers,
                                        OverTimeReason = jobResult.OverTimeReason,
                                        TimeSpan = jobResult.TimeSpan,
                                        UnPatrolReason = jobResult.UnPatrolReason
                                    };

                                    var controlPointList = (from x in db.JobControlPoint
                                                            join j in db.Job
                                                            on x.JobUniqueID equals j.UniqueID
                                                            join y in db.RouteControlPoint
                                                            on new { j.RouteUniqueID, x.ControlPointUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID }
                                                            join c in db.ControlPoint
                                                            on x.ControlPointUniqueID equals c.UniqueID
                                                            where x.JobUniqueID == job.Job.UniqueID
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
                                            JobResultUniqueID = jobResult.UniqueID,
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
                                                User = UserDataAccessor.GetUser(x.UserID),
                                                UnRFIDReasonID = x.UnRFIDReasonID,
                                                UnRFIDReasonDescription = x.UnRFIDReasonDescription,
                                                UnRFIDReasonRemark = x.UnRFIDReasonRemark,
                                                TimeSpanAbnormalReasonID = x.TimeSpanAbnormalReasonID,
                                                TimeSpanAbnormalReasonDescription = x.TimeSpanAbnormalReasonDescription,
                                                TimeSpanAbnormalReasonRemark = x.TimeSpanAbnormalReasonRemark,
                                                PhotoList = db.ArriveRecordPhoto.Where(p => p.ArriveRecordUniqueID == x.UniqueID).Select(p => p.ArriveRecordUniqueID + "_" + p.Seq + "." + p.Extension).OrderBy(p => p).ToList()
                                            }).ToList()
                                        };

                                        var controlPointCheckItemList = (from x in db.JobControlPointCheckItem
                                                                         join j in db.Job
                                                                         on x.JobUniqueID equals j.UniqueID
                                                                         join y in db.RouteControlPointCheckItem
                                                                         on new { j.RouteUniqueID, x.ControlPointUniqueID, x.CheckItemUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.CheckItemUniqueID }
                                                                         join c in db.View_ControlPointCheckItem
                                                                         on new { x.ControlPointUniqueID, x.CheckItemUniqueID } equals new { c.ControlPointUniqueID, c.CheckItemUniqueID }
                                                                         where x.JobUniqueID == job.Job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID
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
                                                EquipmentID = "",
                                                EquipmentName = "",
                                                PartDescription = "",
                                                CheckItemID = checkItem.ID,
                                                CheckItemDescription = checkItem.Description,
                                                LowerLimit = checkItem.LowerLimit,
                                                LowerAlertLimit = checkItem.LowerAlertLimit,
                                                UpperAlertLimit = checkItem.UpperAlertLimit,
                                                UpperLimit = checkItem.UpperLimit,
                                                Unit = checkItem.Unit
                                            };

                                            var checkResultList = allCheckResultList.Where(x => x.ControlPointUniqueID == controlPoint.UniqueID && string.IsNullOrEmpty(x.EquipmentUniqueID) && string.IsNullOrEmpty(x.PartUniqueID) && x.CheckItemUniqueID == checkItem.UniqueID).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

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

                                        var equipmentList = (from x in db.JobEquipment
                                                             join j in db.Job
                                                             on x.JobUniqueID equals j.UniqueID
                                                             join y in db.RouteEquipment
                                                             on new { j.RouteUniqueID, x.ControlPointUniqueID, x.EquipmentUniqueID, x.PartUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.EquipmentUniqueID, y.PartUniqueID }
                                                             where x.JobUniqueID == job.Job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID
                                                             select new { x.EquipmentUniqueID, x.PartUniqueID, y.Seq }).OrderBy(x => x.Seq).ToList();

                                        foreach (var equipment in equipmentList)
                                        {
                                            var equipmentCheckItemList = (from x in db.JobEquipmentCheckItem
                                                                          join j in db.Job
                                                                          on x.JobUniqueID equals j.UniqueID
                                                                          join y in db.RouteEquipmentCheckItem
                                                                          on new { j.RouteUniqueID, x.ControlPointUniqueID, x.EquipmentUniqueID, x.PartUniqueID, x.CheckItemUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.EquipmentUniqueID, y.PartUniqueID, y.CheckItemUniqueID }
                                                                          join e in db.Equipment
                                                                          on x.EquipmentUniqueID equals e.UniqueID
                                                                          join p in db.EquipmentPart
                                                                          on new { x.EquipmentUniqueID, x.PartUniqueID } equals new { p.EquipmentUniqueID, PartUniqueID = p.UniqueID } into tmpPart
                                                                          from p in tmpPart.DefaultIfEmpty()
                                                                          join c in db.View_EquipmentCheckItem
                                                                          on new { x.EquipmentUniqueID, x.PartUniqueID, x.CheckItemUniqueID } equals new { c.EquipmentUniqueID, c.PartUniqueID, c.CheckItemUniqueID }
                                                                          where x.JobUniqueID == job.Job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == equipment.EquipmentUniqueID && x.PartUniqueID == equipment.PartUniqueID
                                                                          select new
                                                                          {
                                                                              x.EquipmentUniqueID,
                                                                              EquipmentID = e.ID,
                                                                              EquipmentName = e.Name,
                                                                              x.PartUniqueID,
                                                                              PartDescription = p != null ? p.Description : "",
                                                                              x.CheckItemUniqueID,
                                                                              CheckItemID = c.ID,
                                                                              CheckItemDescription = c.Description,
                                                                              c.LowerLimit,
                                                                              c.LowerAlertLimit,
                                                                              c.UpperAlertLimit,
                                                                              c.UpperLimit,
                                                                              c.Unit,
                                                                              y.Seq
                                                                          }).OrderBy(x => x.Seq).ToList();

                                            foreach (var checkItem in equipmentCheckItemList)
                                            {
                                                var checkItemModel = new CheckItemModel()
                                                {
                                                    EquipmentID = checkItem.EquipmentID,
                                                    EquipmentName = checkItem.EquipmentName,
                                                    PartDescription = checkItem.PartDescription,
                                                    CheckItemID = checkItem.CheckItemID,
                                                    CheckItemDescription = checkItem.CheckItemDescription,
                                                    LowerLimit = checkItem.LowerLimit,
                                                    LowerAlertLimit = checkItem.LowerAlertLimit,
                                                    UpperAlertLimit = checkItem.UpperAlertLimit,
                                                    UpperLimit = checkItem.UpperLimit,
                                                    Unit = checkItem.Unit
                                                };

                                                var checkResultList = allCheckResultList.Where(x => x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == checkItem.EquipmentUniqueID && x.PartUniqueID == checkItem.PartUniqueID && x.CheckItemUniqueID == checkItem.CheckItemUniqueID).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

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

        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = new List<JobResultModel>();

                var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                using (EDbEntities db = new EDbEntities())
                {
                    var query = (from j in db.Job
                                 join r in db.Route
                                 on j.RouteUniqueID equals r.UniqueID
                                 where downStreamOrganizationList.Contains(r.OrganizationUniqueID)
                                 select new
                                 {
                                     Job = j,
                                     Route = r
                                 }).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.RouteUniqueID))
                    {
                        query = query.Where(x => x.Job.RouteUniqueID == Parameters.RouteUniqueID);
                    }

                    if (!string.IsNullOrEmpty(Parameters.JobUniqueID))
                    {
                        query = query.Where(x => x.Job.UniqueID == Parameters.JobUniqueID);
                    }

                    var jobList = query.ToList();

                    var date = Parameters.BeginDate;
                    var end = Parameters.EndDate;

                    while (date <= end)
                    {
                        foreach (var job in jobList)
                        {
                            if (JobCycleHelper.IsInCycle(date, job.Job.BeginDate, job.Job.EndDate, job.Job.CycleCount, job.Job.CycleMode))
                            {
                                DateTime beginDate, endDate;

                                JobCycleHelper.GetDateSpan(date, job.Job.BeginDate, job.Job.EndDate, job.Job.CycleCount, job.Job.CycleMode, out beginDate, out endDate);

                                var beginDateString = DateTimeHelper.DateTime2DateString(beginDate);
                                var endDateString = DateTimeHelper.DateTime2DateString(endDate);

                                if (!string.IsNullOrEmpty(job.Job.BeginTime) && !string.IsNullOrEmpty(job.Job.EndTime) && string.Compare(job.Job.BeginTime, job.Job.EndTime) > 0)
                                {
                                    endDateString = DateTimeHelper.DateTime2DateString(endDate.AddDays(1));
                                }

                                var jobResult = db.JobResult.FirstOrDefault(x => x.JobUniqueID == job.Job.UniqueID && x.BeginDate == beginDateString && x.EndDate == endDateString);

                                if (jobResult == null)
                                {
                                    JobResultDataAccessor.Refresh(Guid.NewGuid().ToString(), job.Job.UniqueID, beginDateString, endDateString);

                                    jobResult = db.JobResult.FirstOrDefault(x => x.JobUniqueID == job.Job.UniqueID && x.BeginDate == beginDateString && x.EndDate == endDateString);
                                }

                                if (jobResult != null && !itemList.Any(x => x.UniqueID == jobResult.UniqueID))
                                {
                                    var allArriveRecordList = db.ArriveRecord.Where(x => x.JobResultUniqueID == jobResult.UniqueID).ToList();

                                    var allCheckResultList = (from c in db.CheckResult
                                                              join a in db.ArriveRecord
                                                              on c.ArriveRecordUniqueID equals a.UniqueID
                                                              where a.JobResultUniqueID == jobResult.UniqueID
                                                              select c).ToList();

                                    var item = new JobResultModel()
                                    {
                                        UniqueID = jobResult.UniqueID,
                                        OrganizationDescription = jobResult.OrganizationDescription,
                                        JobUniqueID = jobResult.JobUniqueID,
                                        BeginDate = jobResult.BeginDate,
                                        EndDate = jobResult.EndDate,
                                        BeginTime = jobResult.BeginTime,
                                        EndTime = jobResult.EndTime,
                                        ArriveStatus = jobResult.ArriveStatus,
                                        ArriveStatusLabelClass = jobResult.ArriveStatusLabelClass,
                                        CheckUsers = jobResult.CheckUsers,
                                        CompleteRate = jobResult.CompleteRate,
                                        CompleteRateLabelClass = jobResult.CompleteRateLabelClass,
                                        Description = jobResult.Description,
                                        HaveAbnormal = jobResult.HaveAbnormal,
                                        HaveAlert = jobResult.HaveAlert,
                                        JobUsers = jobResult.JobUsers,
                                        OverTimeReason = jobResult.OverTimeReason,
                                        TimeSpan = jobResult.TimeSpan,
                                        UnPatrolReason = jobResult.UnPatrolReason
                                    };

                                    var controlPointList = (from x in db.JobControlPoint
                                                            join j in db.Job
                                                            on x.JobUniqueID equals j.UniqueID
                                                            join y in db.RouteControlPoint
                                                            on new { j.RouteUniqueID, x.ControlPointUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID }
                                                            join c in db.ControlPoint
                                                            on x.ControlPointUniqueID equals c.UniqueID
                                                            where x.JobUniqueID == job.Job.UniqueID
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
                                            JobResultUniqueID = jobResult.UniqueID,
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
                                                User = UserDataAccessor.GetUser(x.UserID),
                                                UnRFIDReasonID = x.UnRFIDReasonID,
                                                UnRFIDReasonDescription = x.UnRFIDReasonDescription,
                                                UnRFIDReasonRemark = x.UnRFIDReasonRemark,
                                                TimeSpanAbnormalReasonID = x.TimeSpanAbnormalReasonID,
                                                TimeSpanAbnormalReasonDescription = x.TimeSpanAbnormalReasonDescription,
                                                TimeSpanAbnormalReasonRemark = x.TimeSpanAbnormalReasonRemark,
                                                PhotoList = db.ArriveRecordPhoto.Where(p => p.ArriveRecordUniqueID == x.UniqueID).Select(p => p.ArriveRecordUniqueID + "_" + p.Seq + "." + p.Extension).OrderBy(p => p).ToList()
                                            }).ToList()
                                        };

                                        var controlPointCheckItemList = (from x in db.JobControlPointCheckItem
                                                                         join j in db.Job
                                                                         on x.JobUniqueID equals j.UniqueID
                                                                         join y in db.RouteControlPointCheckItem
                                                                         on new { j.RouteUniqueID, x.ControlPointUniqueID, x.CheckItemUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.CheckItemUniqueID }
                                                                         join c in db.View_ControlPointCheckItem
                                                                         on new { x.ControlPointUniqueID, x.CheckItemUniqueID } equals new { c.ControlPointUniqueID, c.CheckItemUniqueID }
                                                                         where x.JobUniqueID == job.Job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID
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
                                                EquipmentID = "",
                                                EquipmentName = "",
                                                PartDescription = "",
                                                CheckItemID = checkItem.ID,
                                                CheckItemDescription = checkItem.Description,
                                                LowerLimit = checkItem.LowerLimit,
                                                LowerAlertLimit = checkItem.LowerAlertLimit,
                                                UpperAlertLimit = checkItem.UpperAlertLimit,
                                                UpperLimit = checkItem.UpperLimit,
                                                Unit = checkItem.Unit
                                            };

                                            var checkResultList = allCheckResultList.Where(x => x.ControlPointUniqueID == controlPoint.UniqueID && string.IsNullOrEmpty(x.EquipmentUniqueID) && string.IsNullOrEmpty(x.PartUniqueID) && x.CheckItemUniqueID == checkItem.UniqueID).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

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

                                        var equipmentList = (from x in db.JobEquipment
                                                             join j in db.Job
                                                             on x.JobUniqueID equals j.UniqueID
                                                             join y in db.RouteEquipment
                                                             on new { j.RouteUniqueID, x.ControlPointUniqueID, x.EquipmentUniqueID, x.PartUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.EquipmentUniqueID, y.PartUniqueID }
                                                             where x.JobUniqueID == job.Job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID
                                                             select new { x.EquipmentUniqueID, x.PartUniqueID, y.Seq }).OrderBy(x => x.Seq).ToList();

                                        foreach (var equipment in equipmentList)
                                        {
                                            var equipmentCheckItemList = (from x in db.JobEquipmentCheckItem
                                                                          join j in db.Job
                                                                          on x.JobUniqueID equals j.UniqueID
                                                                          join y in db.RouteEquipmentCheckItem
                                                                          on new { j.RouteUniqueID, x.ControlPointUniqueID, x.EquipmentUniqueID, x.PartUniqueID, x.CheckItemUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.EquipmentUniqueID, y.PartUniqueID, y.CheckItemUniqueID }
                                                                          join e in db.Equipment
                                                                          on x.EquipmentUniqueID equals e.UniqueID
                                                                          join p in db.EquipmentPart
                                                                          on new { x.EquipmentUniqueID, x.PartUniqueID } equals new { p.EquipmentUniqueID, PartUniqueID = p.UniqueID } into tmpPart
                                                                          from p in tmpPart.DefaultIfEmpty()
                                                                          join c in db.View_EquipmentCheckItem
                                                                          on new { x.EquipmentUniqueID, x.PartUniqueID, x.CheckItemUniqueID } equals new { c.EquipmentUniqueID, c.PartUniqueID, c.CheckItemUniqueID }
                                                                          where x.JobUniqueID == job.Job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == equipment.EquipmentUniqueID && x.PartUniqueID == equipment.PartUniqueID
                                                                          select new
                                                                          {
                                                                              x.EquipmentUniqueID,
                                                                              EquipmentID = e.ID,
                                                                              EquipmentName = e.Name,
                                                                              x.PartUniqueID,
                                                                              PartDescription = p != null ? p.Description : "",
                                                                              x.CheckItemUniqueID,
                                                                              CheckItemID = c.ID,
                                                                              CheckItemDescription = c.Description,
                                                                              c.LowerLimit,
                                                                              c.LowerAlertLimit,
                                                                              c.UpperAlertLimit,
                                                                              c.UpperLimit,
                                                                              c.Unit,
                                                                              y.Seq
                                                                          }).OrderBy(x => x.Seq).ToList();

                                            foreach (var checkItem in equipmentCheckItemList)
                                            {
                                                var checkItemModel = new CheckItemModel()
                                                {
                                                    EquipmentID = checkItem.EquipmentID,
                                                    EquipmentName = checkItem.EquipmentName,
                                                    PartDescription = checkItem.PartDescription,
                                                    CheckItemID = checkItem.CheckItemID,
                                                    CheckItemDescription = checkItem.CheckItemDescription,
                                                    LowerLimit = checkItem.LowerLimit,
                                                    LowerAlertLimit = checkItem.LowerAlertLimit,
                                                    UpperAlertLimit = checkItem.UpperAlertLimit,
                                                    UpperLimit = checkItem.UpperLimit,
                                                    Unit = checkItem.Unit
                                                };

                                                var checkResultList = allCheckResultList.Where(x => x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == checkItem.EquipmentUniqueID && x.PartUniqueID == checkItem.PartUniqueID && x.CheckItemUniqueID == checkItem.CheckItemUniqueID).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

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

        public static RequestResult Export(List<JobResultModel> Model, Define.EnumExcelVersion ExcelVersion)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ExcelHelper helper = new ExcelHelper(string.Format("{0}_{1}({2})", Resources.Resource.EquipmentPatrolResult, Resources.Resource.ExportTime, DateTimeHelper.DateTime2DateTimeString(DateTime.Now)), ExcelVersion))
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
                                        User = arriveRecord.User.User,
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
                                            Equipment = checkItem.Equipment,
                                            CheckItem = checkItem.CheckItem,
                                            CheckDate = checkResult.CheckDate,
                                            CheckTime = checkResult.CheckTime,
                                            Result = checkResult.Result,
                                            LowerLimit = checkResult.LowerLimit.HasValue ? checkResult.LowerLimit.Value.ToString() : "",
                                            LowerAlertLimit = checkItem.LowerAlertLimit.HasValue ? checkItem.LowerAlertLimit.Value.ToString() : "",
                                            UpperAlertLimit = checkItem.UpperAlertLimit.HasValue ? checkItem.UpperAlertLimit.Value.ToString() : "",
                                            UpperLimit = checkResult.UpperLimit.HasValue ? checkResult.UpperLimit.Value.ToString() : "",
                                            Unit = checkResult.Unit,
                                            User = arriveRecord != null ? arriveRecord.User.User : "",
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
                                        Equipment = checkItem.Equipment,
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
                        { "Equipment", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.Equipment, CellType = NPOI.SS.UserModel.CellType.String }},
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

        public static RequestResult GetTreeItem(List<Organization> OrganizationList, string OrganizationUniqueID, string RouteUniqueID)
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
                    { Define.EnumTreeAttribute.RouteUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.JobUniqueID, string.Empty },
                };

                using (EDbEntities edb = new EDbEntities())
                {
                    if (string.IsNullOrEmpty(RouteUniqueID))
                    {
                        var routeList = (from j in edb.Job
                                         join r in edb.Route
                                         on j.RouteUniqueID equals r.UniqueID
                                         where r.OrganizationUniqueID == OrganizationUniqueID
                                         select new
                                         {
                                             r.UniqueID,
                                             r.ID,
                                             r.Name
                                         }).Distinct().OrderBy(x => x.ID).ToList();

                        foreach (var route in routeList)
                        {
                            var treeItem = new TreeItem() { Title = route.Name };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Route.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", route.ID, route.Name);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.RouteUniqueID] = route.UniqueID;
                            attributes[Define.EnumTreeAttribute.JobUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItem.State = "closed";

                            treeItemList.Add(treeItem);
                        }

                        var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                        foreach (var organization in organizationList)
                        {
                            var downStream = OrganizationDataAccessor.GetDownStreamOrganizationList(OrganizationList, organization.UniqueID, true);

                            var query = (from j in edb.Job
                                         join r in edb.Route
                                         on j.RouteUniqueID equals r.UniqueID
                                         where downStream.Contains(r.OrganizationUniqueID)
                                         select r).ToList();

                            if (query.Count > 0)
                            {
                                var treeItem = new TreeItem() { Title = organization.Description };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                                attributes[Define.EnumTreeAttribute.RouteUniqueID] = string.Empty;
                                attributes[Define.EnumTreeAttribute.JobUniqueID] = string.Empty;

                                foreach (var attribute in attributes)
                                {
                                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                }

                                treeItem.State = "closed";

                                treeItemList.Add(treeItem);
                            }
                        }
                    }
                    else
                    {
                        var jobList = edb.Job.Where(x => x.RouteUniqueID == RouteUniqueID).OrderBy(x => x.Description).ToList();

                        foreach (var job in jobList)
                        {
                            var treeItem = new TreeItem() { Title = job.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Job.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = job.Description;
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.RouteUniqueID] = RouteUniqueID;
                            attributes[Define.EnumTreeAttribute.JobUniqueID] = job.UniqueID;

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
