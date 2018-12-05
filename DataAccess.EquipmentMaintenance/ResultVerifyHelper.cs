using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.Authenticated;
using Models.EquipmentMaintenance.ResultVerify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace DataAccess.EquipmentMaintenance
{
    public class ResultVerifyHelper
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                var baseTime = DateTime.Now.AddHours(-2);

                using (EDbEntities edb = new EDbEntities())
                {
                    using (DbEntities db = new DbEntities())
                    {


                        var query = (from x in edb.JobResult
                                     join y in edb.JobResultFlow
                                     on x.UniqueID equals y.JobResultUniqueID into tmpFlow
                                     from y in tmpFlow.DefaultIfEmpty()
                                     join j in edb.Job
                                     on x.JobUniqueID equals j.UniqueID
                                     join r in edb.Route
                                     on j.RouteUniqueID equals r.UniqueID
                                     where x.IsNeedVerify && (x.IsCompleted || baseTime > x.JobEndTime) && downStreamOrganizationList.Contains(r.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(r.OrganizationUniqueID)
                                     select new
                                      {
                                          IsClosed = y != null ? y.IsClosed : false,
                                          x.UniqueID,
                                          x.OrganizationDescription,
                                          x.JobUniqueID,
                                          RouteUniqueID = r.UniqueID,
                                          x.BeginDate,
                                          x.EndDate,
                                          x.BeginTime,
                                          x.EndTime,
                                          x.Description,
                                          x.HaveAbnormal,
                                          x.HaveAlert,
                                          x.TimeSpan,
                                          x.CompleteRate,
                                          x.CompleteRateLabelClass,
                                          x.ArriveStatus,
                                          x.ArriveStatusLabelClass,
                                          x.CheckUsers
                                      }).AsQueryable();

                        if (!string.IsNullOrEmpty(Parameters.RouteUniqueID))
                        {
                            query = query.Where(x => x.RouteUniqueID == Parameters.RouteUniqueID);
                        }

                        if (!string.IsNullOrEmpty(Parameters.JobUniqueID))
                        {
                            query = query.Where(x => x.JobUniqueID == Parameters.JobUniqueID);
                        }

                        query = query.Where(x => string.Compare(x.EndDate, Parameters.BeginDate) >= 0 && string.Compare(x.EndDate, Parameters.EndDate) <= 0);

                        var itemList = query.Select(x => new GridItem
                        {
                            IsClosed = x.IsClosed,
                            UniqueID = x.UniqueID,
                            BeginDate = x.BeginDate,
                            EndDate = x.EndDate,
                            BeginTime = x.BeginTime,
                            EndTime = x.EndTime,
                            ArriveStatus = x.ArriveStatus,
                            ArriveStatusLabelClass = x.ArriveStatusLabelClass,
                            CheckUsers = x.CheckUsers,
                            OrganizationDescription = x.OrganizationDescription,
                            CompleteRate = x.CompleteRate,
                            CompleteRateLabelClass = x.CompleteRateLabelClass,
                            Description = x.Description,
                            HaveAbnormal = x.HaveAbnormal,
                            HaveAlert = x.HaveAlert,
                            TimeSpan = x.TimeSpan
                        }).OrderBy(x => x.BeginDate).ThenBy(x => x.BeginTime).ThenBy(x => x.Description).ToList();

                        foreach (var item in itemList)
                        {
                            if (!item.IsClosed)
                            {
                                var flow = edb.JobResultFlow.FirstOrDefault(x => x.JobResultUniqueID == item.UniqueID);

                                if (flow != null)
                                {
                                    var flowLog = edb.JobResultFlowLog.FirstOrDefault(x => x.JobResultUniqueID == flow.JobResultUniqueID && x.Seq == flow.CurrentSeq);

                                    if (flowLog != null)
                                    {
                                        var organizationManagers = (from x in db.OrganizationManager
                                                                    join u in db.User
                                                                    on x.UserID equals u.ID
                                                                    where x.OrganizationUniqueID == flowLog.OrganizationUniqueID
                                                                    select u).ToList();

                                        foreach (var manager in organizationManagers)
                                        {
                                            item.CurrentVerifyUserList.Add(UserDataAccessor.GetUser(manager.ID));
                                        }
                                    }
                                }
                            }
                        }

                        result.ReturnData(itemList);
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

        public static RequestResult GetDetailViewModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities edb = new EDbEntities())
                {
                    using (DbEntities db = new DbEntities())
                    {


                        var query = (from x in edb.JobResult
                                     join j in edb.Job
                                     on x.JobUniqueID equals j.UniqueID
                                     join r in edb.Route
                                     on j.RouteUniqueID equals r.UniqueID
                                     where x.UniqueID == UniqueID
                                     select new
                                     {
                                         x.UniqueID,
                                         x.JobUniqueID,
                                         r.OrganizationUniqueID,
                                         x.Description,
                                         x.BeginDate,
                                         x.EndDate,
                                         x.BeginTime,
                                         x.EndTime,
                                         x.JobUsers,
                                         x.CheckUsers,
                                         x.CompleteRate,
                                         x.CompleteRateLabelClass,
                                         x.ArriveStatus,
                                         x.ArriveStatusLabelClass,
                                         x.TimeSpan,
                                         x.UnPatrolReason,
                                         x.OverTimeReason
                                     }).First();

                        var flow = edb.JobResultFlow.FirstOrDefault(x => x.JobResultUniqueID == query.UniqueID);

                        var model = new DetailViewModel()
                        {
                            UniqueID = query.UniqueID,
                            ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(query.OrganizationUniqueID),
                            Description = query.Description,
                            BeginDate = query.BeginDate,
                            EndDate = query.EndDate,
                            BeginTime = query.BeginTime,
                            EndTime = query.EndTime,
                            JobUsers = query.JobUsers,
                            CheckUsers = query.CheckUsers,
                            CompleteRate = query.CompleteRate,
                            CompleteRateLabelClass = query.CompleteRateLabelClass,
                            ArriveStatus = query.ArriveStatus,
                            ArriveStatusLabelClass = query.ArriveStatusLabelClass,
                            TimeSpan = query.TimeSpan,
                            UnPatrolReason = query.UnPatrolReason,
                            OverTimeReason = query.OverTimeReason,
                            IsClosed = flow != null ? flow.IsClosed : false,
                            FlowLogList = edb.JobResultFlowLog.Where(x => x.JobResultUniqueID == query.UniqueID).OrderBy(x => x.Seq).Select(x => new FlowLogModel
                            {
                                UserID = x.UserID,
                                UserName = x.UserName,
                                Remark = x.Remark,
                                NotifyTime = x.NotifyTime,
                                VerifyTime = x.VerifyTime
                            }).ToList()
                        };

                        if (!model.IsClosed)
                        {
                            if (flow != null)
                            {
                                var flowLog = edb.JobResultFlowLog.FirstOrDefault(x => x.JobResultUniqueID == flow.JobResultUniqueID && x.Seq == flow.CurrentSeq);

                                if (flowLog != null)
                                {
                                    var organizationManagers = (from x in db.OrganizationManager
                                                                join u in db.User
                                                                on x.UserID equals u.ID
                                                                where x.OrganizationUniqueID == flowLog.OrganizationUniqueID
                                                                select u).ToList();

                                    foreach (var manager in organizationManagers)
                                    {
                                        model.CurrentVerifyUserList.Add(UserDataAccessor.GetUser(manager.ID));
                                    }
                                }
                            }
                        }

                        var allArriveRecordList = edb.ArriveRecord.Where(x => x.JobResultUniqueID == query.UniqueID).ToList();

                        var allCheckResultList = (from c in edb.CheckResult
                                                  join a in edb.ArriveRecord
                                                  on c.ArriveRecordUniqueID equals a.UniqueID
                                                  where a.JobResultUniqueID == query.UniqueID
                                                  select c).ToList();

                        var controlPointList = (from x in edb.JobControlPoint
                                                join j in edb.Job
                                                on x.JobUniqueID equals j.UniqueID
                                                join y in edb.RouteControlPoint
                                                on new { j.RouteUniqueID, x.ControlPointUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID }
                                                join c in edb.ControlPoint
                                                on x.ControlPointUniqueID equals c.UniqueID
                                                where x.JobUniqueID == query.JobUniqueID
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
                                    TimeSpanAbnormalReasonRemark = x.TimeSpanAbnormalReasonRemark
                                }).ToList()
                            };

                            var controlPointCheckItemList = (from x in edb.JobControlPointCheckItem
                                                             join j in edb.Job
                                                             on x.JobUniqueID equals j.UniqueID
                                                             join y in edb.RouteControlPointCheckItem
                                                             on new { j.RouteUniqueID, x.ControlPointUniqueID, x.CheckItemUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.CheckItemUniqueID }
                                                             join c in edb.View_ControlPointCheckItem
                                                             on new { x.ControlPointUniqueID, x.CheckItemUniqueID } equals new { c.ControlPointUniqueID, c.CheckItemUniqueID }
                                                             where x.JobUniqueID == query.JobUniqueID && x.ControlPointUniqueID == controlPoint.UniqueID
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
                                        Remark = checkResult.Remark,
                                        AbnormalReasonList = edb.CheckResultAbnormalReason.Where(a => a.CheckResultUniqueID == checkResult.UniqueID).OrderBy(a => a.AbnormalReasonID).Select(a => new AbnormalReasonModel
                                        {
                                            Description = a.AbnormalReasonDescription,
                                            Remark = a.AbnormalReasonRemark,
                                            HandlingMethodList = edb.CheckResultHandlingMethod.Where(h => h.CheckResultUniqueID == checkResult.UniqueID && h.AbnormalReasonUniqueID == a.AbnormalReasonUniqueID).OrderBy(h => h.HandlingMethodID).Select(h => new HandlingMethodModel
                                            {
                                                Description = h.HandlingMethodDescription,
                                                Remark = h.HandlingMethodRemark
                                            }).ToList()
                                        }).ToList()
                                    });
                                }

                                controlPointModel.CheckItemList.Add(checkItemModel);
                            }

                            var equipmentList = (from x in edb.JobEquipment
                                                 join j in edb.Job
                                                 on x.JobUniqueID equals j.UniqueID
                                                 join y in edb.RouteEquipment
                                                 on new { j.RouteUniqueID, x.ControlPointUniqueID, x.EquipmentUniqueID, x.PartUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.EquipmentUniqueID, y.PartUniqueID }
                                                 where x.JobUniqueID == query.JobUniqueID && x.ControlPointUniqueID == controlPoint.UniqueID
                                                 select new { x.EquipmentUniqueID, x.PartUniqueID, y.Seq }).OrderBy(x => x.Seq).ToList();

                            foreach (var equipment in equipmentList)
                            {
                                var equipmentCheckItemList = (from x in edb.JobEquipmentCheckItem
                                                              join j in edb.Job
                                                              on x.JobUniqueID equals j.UniqueID
                                                              join y in edb.RouteEquipmentCheckItem
                                                              on new { j.RouteUniqueID, x.ControlPointUniqueID, x.EquipmentUniqueID, x.PartUniqueID, x.CheckItemUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.EquipmentUniqueID, y.PartUniqueID, y.CheckItemUniqueID }
                                                              join e in edb.Equipment
                                                              on x.EquipmentUniqueID equals e.UniqueID
                                                              join p in edb.EquipmentPart
                                                              on new { x.EquipmentUniqueID, x.PartUniqueID } equals new { p.EquipmentUniqueID, PartUniqueID = p.UniqueID } into tmpPart
                                                              from p in tmpPart.DefaultIfEmpty()
                                                              join c in edb.View_EquipmentCheckItem
                                                              on new { x.EquipmentUniqueID, x.PartUniqueID, x.CheckItemUniqueID } equals new { c.EquipmentUniqueID, c.PartUniqueID, c.CheckItemUniqueID }
                                                              where x.JobUniqueID == query.JobUniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == equipment.EquipmentUniqueID && x.PartUniqueID == equipment.PartUniqueID
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
                                            Remark = checkResult.Remark,
                                            AbnormalReasonList = edb.CheckResultAbnormalReason.Where(a => a.CheckResultUniqueID == checkResult.UniqueID).OrderBy(a => a.AbnormalReasonID).Select(a => new AbnormalReasonModel
                                            {
                                                Description = a.AbnormalReasonDescription,
                                                Remark = a.AbnormalReasonRemark,
                                                HandlingMethodList = edb.CheckResultHandlingMethod.Where(h => h.CheckResultUniqueID == checkResult.UniqueID && h.AbnormalReasonUniqueID == a.AbnormalReasonUniqueID).OrderBy(h => h.HandlingMethodID).Select(h => new HandlingMethodModel
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

                            model.ControlPointList.Add(controlPointModel);
                        }

                        result.ReturnData(model);
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

        public static RequestResult GetVerifyFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var query = (from x in db.JobResult
                                 join j in db.Job
                                 on x.JobUniqueID equals j.UniqueID
                                 join r in db.Route
                                 on j.RouteUniqueID equals r.UniqueID
                                 where x.UniqueID == UniqueID
                                 select new
                                 {
                                     x.UniqueID,
                                     x.JobUniqueID,
                                     r.OrganizationUniqueID,
                                     x.Description,
                                     x.BeginDate,
                                     x.EndDate,
                                     x.BeginTime,
                                     x.EndTime,
                                     x.JobUsers,
                                     x.CheckUsers,
                                     x.CompleteRate,
                                     x.CompleteRateLabelClass,
                                     x.ArriveStatus,
                                     x.ArriveStatusLabelClass,
                                     x.TimeSpan,
                                     x.UnPatrolReason,
                                     x.OverTimeReason
                                 }).First();

                    var model = new VerifyFormModel()
                    {
                        UniqueID = query.UniqueID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(query.OrganizationUniqueID),
                        Description = query.Description,
                        BeginDate = query.BeginDate,
                        EndDate = query.EndDate,
                        BeginTime = query.BeginTime,
                        EndTime = query.EndTime,
                        JobUsers = query.JobUsers,
                        CheckUsers = query.CheckUsers,
                        CompleteRate = query.CompleteRate,
                        CompleteRateLabelClass = query.CompleteRateLabelClass,
                        ArriveStatus = query.ArriveStatus,
                        ArriveStatusLabelClass = query.ArriveStatusLabelClass,
                        TimeSpan = query.TimeSpan,
                        UnPatrolReason = query.UnPatrolReason,
                        OverTimeReason = query.OverTimeReason,
                        FlowLogList = db.JobResultFlowLog.Where(x => x.JobResultUniqueID == query.UniqueID).OrderBy(x => x.Seq).Select(x => new FlowLogModel
                        {
                            UserID = x.UserID,
                            UserName = x.UserName,
                            Remark = x.Remark,
                            NotifyTime = x.NotifyTime,
                            VerifyTime = x.VerifyTime
                        }).ToList()
                    };

                    var unPatrolReasonList = db.UnPatrolReason.OrderBy(x => x.ID).ToList();

                    foreach (var unPatrolReason in unPatrolReasonList)
                    {
                        model.UnPatrolReasonSelectItemList.Add(new SelectListItem()
                        {
                            Text = unPatrolReason.Description,
                            Value = unPatrolReason.UniqueID
                        });
                    }

                    model.UnPatrolReasonSelectItemList.Add(new SelectListItem()
                    {
                        Value = Define.OTHER,
                        Text = Resources.Resource.Other
                    });

                    var allArriveRecordList = db.ArriveRecord.Where(x => x.JobResultUniqueID == query.UniqueID).ToList();

                    var allCheckResultList = (from c in db.CheckResult
                                              join a in db.ArriveRecord
                                              on c.ArriveRecordUniqueID equals a.UniqueID
                                              where a.JobResultUniqueID == query.UniqueID
                                              select c).ToList();

                    var controlPointList = (from x in db.JobControlPoint
                                            join j in db.Job
                                            on x.JobUniqueID equals j.UniqueID
                                            join y in db.RouteControlPoint
                                            on new { j.RouteUniqueID, x.ControlPointUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID }
                                            join c in db.ControlPoint
                                            on x.ControlPointUniqueID equals c.UniqueID
                                            where x.JobUniqueID == query.JobUniqueID
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
                                TimeSpanAbnormalReasonRemark = x.TimeSpanAbnormalReasonRemark
                            }).ToList()
                        };

                        var controlPointCheckItemList = (from x in db.JobControlPointCheckItem
                                                         join j in db.Job
                                                         on x.JobUniqueID equals j.UniqueID
                                                         join y in db.RouteControlPointCheckItem
                                                         on new { j.RouteUniqueID, x.ControlPointUniqueID, x.CheckItemUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.CheckItemUniqueID }
                                                         join c in db.View_ControlPointCheckItem
                                                         on new { x.ControlPointUniqueID, x.CheckItemUniqueID } equals new { c.ControlPointUniqueID, c.CheckItemUniqueID }
                                                         where x.JobUniqueID == query.JobUniqueID && x.ControlPointUniqueID == controlPoint.UniqueID
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
                                    Remark = checkResult.Remark,
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
                                             where x.JobUniqueID == query.JobUniqueID && x.ControlPointUniqueID == controlPoint.UniqueID
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
                                                          where x.JobUniqueID == query.JobUniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == equipment.EquipmentUniqueID && x.PartUniqueID == equipment.PartUniqueID
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
                                        Remark = checkResult.Remark,
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

                        model.ControlPointList.Add(controlPointModel);
                    }

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

        //public static RequestResult Confirm(string UniqueID, string Remark)
        //{
        //    RequestResult result = new RequestResult();

        //    try
        //    {
        //        using (EDbEntities edb = new EDbEntities())
        //        {
        //            var flow = edb.JobResultFlow.First(x => x.JobResultUniqueID == UniqueID);

        //            var currentFlowLog = edb.JobResultFlowLog.Where(x => x.JobResultUniqueID == UniqueID && x.Seq == flow.CurrentSeq).First();

        //            if (currentFlowLog.FlowSeq == 0)
        //            {
        //                flow.IsClosed = true;

        //                currentFlowLog.VerifyTime = DateTime.Now;
        //                currentFlowLog.Remark = Remark;

        //                edb.SaveChanges();

        //                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Confirm, Resources.Resource.Complete));
        //            }
        //            else
        //            {
        //                var route = (from x in edb.JobResult
        //                             join j in edb.Job
        //                             on x.JobUniqueID equals j.UniqueID
        //                             join r in edb.Route
        //                             on j.RouteUniqueID equals r.UniqueID
        //                             where x.UniqueID == UniqueID
        //                             select r).First();

        //                using (DbEntities db = new DbEntities())
        //                {
        //                    var nextVerifyOrganization = (from f in db.Flow
        //                                                  join x in db.FlowForm
        //                                                  on f.UniqueID equals x.FlowUniqueID
        //                                                  join v in db.FlowVerifyOrganization
        //                                                  on f.UniqueID equals v.FlowUniqueID
        //                                                  join o in db.Organization
        //                                                  on v.OrganizationUniqueID equals o.UniqueID
        //                                                  where f.OrganizationUniqueID == route.OrganizationUniqueID && x.Form == Define.EnumForm.EquipmentPatrolResult.ToString()
        //                                                  select new
        //                                                  {
        //                                                      o.UniqueID,
        //                                                      o.Description,
        //                                                      o.ManagerUserID,
        //                                                      v.Seq
        //                                                  }).Where(x => x.Seq > currentFlowLog.FlowSeq).OrderBy(x => x.Seq).FirstOrDefault();

        //                    if (nextVerifyOrganization != null)
        //                    {
        //                        if (!string.IsNullOrEmpty(nextVerifyOrganization.ManagerUserID))
        //                        {
        //                            var user = UserDataAccessor.GetUser(nextVerifyOrganization.ManagerUserID);

        //                            flow.CurrentSeq = flow.CurrentSeq + 1;

        //                            currentFlowLog.VerifyTime = DateTime.Now;
        //                            currentFlowLog.Remark = Remark;

        //                            edb.JobResultFlowLog.Add(new JobResultFlowLog()
        //                            {
        //                                JobResultUniqueID = UniqueID,
        //                                Seq = flow.CurrentSeq,
        //                                FlowSeq = nextVerifyOrganization.Seq,
        //                                UserName = user.Name,
        //                                UserID = nextVerifyOrganization.ManagerUserID,
        //                                NotifyTime = DateTime.Now
        //                            });

        //                            edb.SaveChanges();

        //                            result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Confirm, Resources.Resource.Complete));
        //                        }
        //                        else
        //                        {
        //                            result.ReturnFailedMessage(string.Format("{0} {1} {2} {3}", Resources.Resource.Organization, nextVerifyOrganization.Description, Resources.Resource.NotSet, Resources.Resource.Manager));
        //                        }
        //                    }
        //                    else
        //                    {
        //                        flow.IsClosed = true;

        //                        currentFlowLog.VerifyTime = DateTime.Now;
        //                        currentFlowLog.Remark = Remark;

        //                        edb.SaveChanges();

        //                        result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Confirm, Resources.Resource.Complete));
        //                    }
        //                }
        //            }
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

        public static RequestResult Confirm(string UniqueID, VerifyFormInput FormInput, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities edb = new EDbEntities())
                {
                    if (!string.IsNullOrEmpty(FormInput.UnPatrolReasonUniqueID))
                    {
                        var jobResult = edb.JobResult.First(x => x.UniqueID == UniqueID);

                        if (FormInput.UnPatrolReasonUniqueID == Define.OTHER)
                        {
                            jobResult.UnPatrolReason = FormInput.UnPatrolReasonRemark;
                        }
                        else
                        {
                            var unPatrolReason = edb.UnPatrolReason.First(x => x.UniqueID == FormInput.UnPatrolReasonUniqueID);

                            jobResult.UnPatrolReason = unPatrolReason.Description;
                        }
                    }

                    var flow = edb.JobResultFlow.First(x => x.JobResultUniqueID == UniqueID);

                    var currentFlowLog = edb.JobResultFlowLog.Where(x => x.JobResultUniqueID == UniqueID && x.Seq == flow.CurrentSeq).First();

                    if (currentFlowLog.FlowSeq == 0)
                    {
                        flow.IsClosed = true;

                        currentFlowLog.UserID = Account.ID;
                        currentFlowLog.UserName = Account.Name;
                        currentFlowLog.VerifyTime = DateTime.Now;
                        currentFlowLog.Remark = FormInput.VerifyComment;

                        edb.SaveChanges();

                        result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Confirm, Resources.Resource.Complete));
                    }
                    else
                    {
                        var route = (from x in edb.JobResult
                                     join j in edb.Job
                                     on x.JobUniqueID equals j.UniqueID
                                     join r in edb.Route
                                     on j.RouteUniqueID equals r.UniqueID
                                     where x.UniqueID == UniqueID
                                     select r).First();

                        using (DbEntities db = new DbEntities())
                        {
                            var nextVerifyOrganization = (from f in db.Flow
                                                          join x in db.FlowForm
                                                          on f.UniqueID equals x.FlowUniqueID
                                                          join v in db.FlowVerifyOrganization
                                                          on f.UniqueID equals v.FlowUniqueID
                                                          join o in db.Organization
                                                          on v.OrganizationUniqueID equals o.UniqueID
                                                          where f.OrganizationUniqueID == route.OrganizationUniqueID && x.Form == Define.EnumForm.EquipmentPatrolResult.ToString()
                                                          select new
                                                          {
                                                              o.UniqueID,
                                                              o.Description,
                                                              o.ManagerUserID,
                                                              v.Seq
                                                          }).Where(x => x.Seq > currentFlowLog.FlowSeq).OrderBy(x => x.Seq).FirstOrDefault();

                            if (nextVerifyOrganization != null)
                            {
                                var organizationManagers = (from x in db.OrganizationManager
                                                            join u in db.User
                                                            on x.UserID equals u.ID
                                                            where x.OrganizationUniqueID == nextVerifyOrganization.UniqueID
                                                            select u).ToList();

                                //if (!string.IsNullOrEmpty(nextVerifyOrganization.ManagerUserID))
                                if (organizationManagers.Count > 0)
                                {
                                    //var user = UserDataAccessor.GetUser(nextVerifyOrganization.ManagerUserID);

                                    flow.CurrentSeq = flow.CurrentSeq + 1;

                                    currentFlowLog.UserID = Account.ID;
                                    currentFlowLog.UserName = Account.Name;
                                    currentFlowLog.VerifyTime = DateTime.Now;
                                    currentFlowLog.Remark = FormInput.VerifyComment;

                                    edb.JobResultFlowLog.Add(new JobResultFlowLog()
                                    {
                                        JobResultUniqueID = UniqueID,
                                        Seq = flow.CurrentSeq,
                                        FlowSeq = nextVerifyOrganization.Seq,
                                        //UserName = user.Name,
                                         OrganizationUniqueID = nextVerifyOrganization.UniqueID,
                                        //UserID = nextVerifyOrganization.ManagerUserID,
                                        NotifyTime = DateTime.Now
                                    });

                                    edb.SaveChanges();

                                    result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Confirm, Resources.Resource.Complete));
                                }
                                else
                                {
                                    result.ReturnFailedMessage(string.Format("{0} {1} {2} {3}", Resources.Resource.Organization, nextVerifyOrganization.Description, Resources.Resource.NotSet, Resources.Resource.Manager));
                                }
                            }
                            else
                            {
                                flow.IsClosed = true;

                                currentFlowLog.UserID = Account.ID;
                                currentFlowLog.UserName = Account.Name;
                                currentFlowLog.VerifyTime = DateTime.Now;
                                currentFlowLog.Remark = FormInput.VerifyComment;

                                edb.SaveChanges();

                                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Confirm, Resources.Resource.Complete));
                            }
                        }
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
    }
}
