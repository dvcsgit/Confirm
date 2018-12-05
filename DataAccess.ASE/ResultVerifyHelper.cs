using DbEntity.ASE;
using Models.Authenticated;
using Models.EquipmentMaintenance.ResultVerify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace DataAccess.ASE
{
    public class ResultVerifyHelper
    {
        public static RequestResult Query(Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var baseTime = DateTime.Now.AddHours(-2);

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = (from x in db.JOBRESULT
                                 join y in db.JOBRESULTFLOW
                                 on x.UNIQUEID equals y.JOBRESULTUNIQUEID into tmpFlow
                                 from y in tmpFlow.DefaultIfEmpty()
                                 join j in db.JOB
                                 on x.JOBUNIQUEID equals j.UNIQUEID
                                 join r in db.ROUTE
                                 on j.ROUTEUNIQUEID equals r.UNIQUEID
                                 where x.ISNEEDVERIFY == "Y" && (x.ISCOMPLETED == "Y" || DateTime.Compare(baseTime, x.JOBENDTIME.Value) > 0) && Account.QueryableOrganizationUniqueIDList.Contains(r.ORGANIZATIONUNIQUEID)
                                 select new
                                 {
                                     IsClosed = y != null ? y.ISCLOSED == "Y" : false,
                                     x.UNIQUEID,
                                     OrganizationDescription = x.ORGANIZATIONDESCRIPTION,
                                     JobUniqueID = x.JOBUNIQUEID,
                                     RouteUniqueID = r.UNIQUEID,
                                     BeginDate = x.BEGINDATE,
                                     EndDate = x.ENDDATE,
                                     BeginTime = x.BEGINTIME,
                                     EndTime = x.ENDTIME,
                                     x.DESCRIPTION,
                                     HaveAbnormal = x.HAVEABNORMAL == "Y",
                                     HaveAlert = x.HAVEALERT == "Y",
                                     TimeSpan = x.TIMESPAN,
                                     CompleteRate = x.COMPLETERATE,
                                     CompleteRateLabelClass = x.COMPLETERATELABELCLASS,
                                     ArriveStatus = x.ARRIVESTATUS,
                                     ArriveStatusLabelClass = x.ARRIVESTATUSLABELCLASS,
                                     CheckUsers = x.CHECKUSERS
                                 }).AsQueryable();

                    var itemList = query.Select(x => new GridItem
                    {
                        IsClosed = x.IsClosed,
                        UniqueID = x.UNIQUEID,
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
                        Description = x.DESCRIPTION,
                        HaveAbnormal = x.HaveAbnormal,
                        HaveAlert = x.HaveAlert,
                        TimeSpan = x.TimeSpan
                    }).OrderBy(x => x.BeginDate).ThenBy(x => x.BeginTime).ThenBy(x => x.Description).ToList();

                    foreach (var item in itemList)
                    {
                        if (!item.IsClosed)
                        {
                            var flow = db.JOBRESULTFLOW.FirstOrDefault(x => x.JOBRESULTUNIQUEID == item.UniqueID);

                            if (flow != null)
                            {
                                var flowLog = db.JOBRESULTFLOWLOG.FirstOrDefault(x => x.JOBRESULTUNIQUEID == flow.JOBRESULTUNIQUEID && x.SEQ == flow.CURRENTSEQ);

                                if (flowLog != null)
                                {
                                    item.CurrentVerifyUserID = flowLog.USERID;
                                    item.CurrentVerifyUserName = flowLog.USERNAME;
                                }
                            }
                        }
                    }

                    result.ReturnData(itemList.Where(x => x.CurrentVerifyUserID == Account.ID && !x.IsClosed).ToList());
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

        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                var baseTime = DateTime.Now.AddHours(-2);

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = (from x in db.JOBRESULT
                                 join y in db.JOBRESULTFLOW
                                 on x.UNIQUEID equals y.JOBRESULTUNIQUEID into tmpFlow
                                 from y in tmpFlow.DefaultIfEmpty()
                                 join j in db.JOB
                                 on x.JOBUNIQUEID equals j.UNIQUEID
                                 join r in db.ROUTE
                                 on j.ROUTEUNIQUEID equals r.UNIQUEID
                                 where x.ISNEEDVERIFY=="Y" && (x.ISCOMPLETED=="Y" ||DateTime.Compare(baseTime, x.JOBENDTIME.Value)>0  ) && downStreamOrganizationList.Contains(r.ORGANIZATIONUNIQUEID) && Account.QueryableOrganizationUniqueIDList.Contains(r.ORGANIZATIONUNIQUEID)
                                 select new
                                  {
                                      IsClosed = y!=null?y.ISCLOSED=="Y":false,
                                      x.UNIQUEID,
                                      OrganizationDescription=x.ORGANIZATIONDESCRIPTION,
                                      JobUniqueID=x.JOBUNIQUEID,
                                      RouteUniqueID = r.UNIQUEID,
                                      BeginDate=x.BEGINDATE,
                                      EndDate=x.ENDDATE,
                                      BeginTime=x.BEGINTIME,
                                      EndTime=x.ENDTIME,
                                      x.DESCRIPTION,
                                      HaveAbnormal=x.HAVEABNORMAL=="Y",
                                      HaveAlert = x.HAVEALERT == "Y",
                                      TimeSpan=x.TIMESPAN,
                                      CompleteRate=x.COMPLETERATE,
                                      CompleteRateLabelClass=x.COMPLETERATELABELCLASS,
                                      ArriveStatus=x.ARRIVESTATUS,
                                      ArriveStatusLabelClass=x.ARRIVESTATUSLABELCLASS,
                                      CheckUsers=x.CHECKUSERS
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
                        UniqueID = x.UNIQUEID,
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
                        Description = x.DESCRIPTION,
                        HaveAbnormal = x.HaveAbnormal,
                        HaveAlert = x.HaveAlert,
                        TimeSpan = x.TimeSpan
                    }).OrderBy(x => x.BeginDate).ThenBy(x => x.BeginTime).ThenBy(x => x.Description).ToList();

                    foreach (var item in itemList)
                    {
                        if (!item.IsClosed)
                        {
                            var flow = db.JOBRESULTFLOW.FirstOrDefault(x => x.JOBRESULTUNIQUEID == item.UniqueID);

                            if (flow != null)
                            {
                                var flowLog = db.JOBRESULTFLOWLOG.FirstOrDefault(x => x.JOBRESULTUNIQUEID == flow.JOBRESULTUNIQUEID && x.SEQ == flow.CURRENTSEQ);

                                if (flowLog != null)
                                {
                                    item.CurrentVerifyUserID = flowLog.USERID;
                                    item.CurrentVerifyUserName = flowLog.USERNAME;
                                }
                            }
                        }
                    }

                    result.ReturnData(itemList);
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = (from x in db.JOBRESULT
                                 join j in db.JOB
                                 on x.JOBUNIQUEID equals j.UNIQUEID
                                 join r in db.ROUTE
                                 on j.ROUTEUNIQUEID equals r.UNIQUEID
                                 where x.UNIQUEID == UniqueID
                                 select new
                                 {
                                     x.UNIQUEID,
                                     JobUniqueID=x.JOBUNIQUEID,
                                    OrganizationUniqueID= r.ORGANIZATIONUNIQUEID,
                                    Description= x.DESCRIPTION,
                                     BeginDate=x.BEGINDATE,
                                     EndDate=x.ENDDATE,
                                     BeginTime=x.BEGINTIME,
                                     EndTime=x.ENDTIME,
                                     JobUsers=x.JOBUSERS,
                                     CheckUsers=x.CHECKUSERS,
                                     CompleteRate=x.COMPLETERATE,
                                     CompleteRateLabelClass=x.COMPLETERATELABELCLASS,
                                     ArriveStatus=x.ARRIVESTATUS,
                                     ArriveStatusLabelClass=x.ARRIVESTATUSLABELCLASS,
                                     TimeSpan=x.TIMESPAN,
                                     UnPatrolReason=x.UNPATROLREASON,
                                     OverTimeReason=x.OVERTIMEREASON
                                 }).First();

                    var flow = db.JOBRESULTFLOW.FirstOrDefault(x => x.JOBRESULTUNIQUEID == query.UNIQUEID);

                    var model = new DetailViewModel()
                    {
                        UniqueID = query.UNIQUEID,
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
                        IsClosed = flow != null ? flow.ISCLOSED=="Y" : false,
                        FlowLogList = db.JOBRESULTFLOWLOG.Where(x => x.JOBRESULTUNIQUEID == query.UNIQUEID).OrderBy(x => x.SEQ).Select(x => new FlowLogModel
                        {
                            UserID = x.USERID,
                            UserName = x.USERNAME,
                            Remark = x.REMARK,
                            NotifyTime = x.NOTIFYTIME.Value,
                            VerifyTime = x.VERIFYTIME
                        }).ToList()
                    };

                    if (!model.IsClosed)
                    {
                        if (flow != null)
                        {
                            var flowLog = db.JOBRESULTFLOWLOG.FirstOrDefault(x => x.JOBRESULTUNIQUEID == flow.JOBRESULTUNIQUEID && x.SEQ == flow.CURRENTSEQ);

                            if (flowLog != null)
                            {
                                model.CurrentVerifyUserID = flowLog.USERID;
                            }
                        }
                    }

                    var allArriveRecordList = db.ARRIVERECORD.Where(x => x.JOBRESULTUNIQUEID == query.UNIQUEID).ToList();

                    var allCheckResultList = (from c in db.CHECKRESULT
                                              join a in db.ARRIVERECORD
                                              on c.ARRIVERECORDUNIQUEID equals a.UNIQUEID
                                              where a.JOBRESULTUNIQUEID == query.UNIQUEID
                                              select c).ToList();

                    var controlPointList = (from x in db.JOBCONTROLPOINT
                                            join j in db.JOB
                                            on x.JOBUNIQUEID equals j.UNIQUEID
                                            join y in db.ROUTECONTROLPOINT
                                            on new { j.ROUTEUNIQUEID, x.CONTROLPOINTUNIQUEID } equals new { y.ROUTEUNIQUEID, y.CONTROLPOINTUNIQUEID }
                                            join c in db.CONTROLPOINT
                                            on x.CONTROLPOINTUNIQUEID equals c.UNIQUEID
                                            where x.JOBUNIQUEID == query.JobUniqueID
                                            select new
                                            {
                                                UniqueID = c.UNIQUEID,
                                                ID = c.ID,
                                                Description = c.DESCRIPTION,
                                              MinTimeSpan=  x.MINTIMESPAN,
                                                y.SEQ
                                            }).OrderBy(x => x.SEQ).ToList();

                    foreach (var controlPoint in controlPointList)
                    {
                        var controlPointModel = new ControlPointModel()
                        {
                            ID = controlPoint.ID,
                            Description = controlPoint.Description,
                            MinTimeSpan = controlPoint.MinTimeSpan,
                            ArriveRecordList = allArriveRecordList.Where(x => x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID).OrderBy(x => x.ARRIVEDATE).ThenBy(x => x.ARRIVETIME).Select(x => new ArriveRecordModel
                            {
                                UniqueID = x.UNIQUEID,
                                ArriveDate = x.ARRIVEDATE,
                                ArriveTime = x.ARRIVETIME,
                                User = UserDataAccessor.GetUser(x.USERID),
                                UnRFIDReasonID = x.UNRFIDREASONID,
                                UnRFIDReasonDescription = x.UNRFIDREASONDESCRIPTION,
                                UnRFIDReasonRemark = x.UNRFIDREASONREMARK,
                                TimeSpanAbnormalReasonID = x.TIMESPANABNORMALREASONID,
                                TimeSpanAbnormalReasonDescription = x.TIMESPANABNORMALREASONDESC,
                                TimeSpanAbnormalReasonRemark = x.TIMESPANABNORMALREASONREMARK
                            }).ToList()
                        };

                        var controlPointCheckItemList = (from x in db.JOBCONTROLPOINTCHECKITEM
                                                         join j in db.JOB
                                                         on x.JOBUNIQUEID equals j.UNIQUEID
                                                         join y in db.ROUTECONTROLPOINTCHECKITEM
                                                         on new { j.ROUTEUNIQUEID, x.CONTROLPOINTUNIQUEID, x.CHECKITEMUNIQUEID } equals new { y.ROUTEUNIQUEID, y.CONTROLPOINTUNIQUEID, y.CHECKITEMUNIQUEID }
                                                         join c in db.CONTROLPOINTCHECKITEM
                                                         on new { x.CONTROLPOINTUNIQUEID, x.CHECKITEMUNIQUEID } equals new { c.CONTROLPOINTUNIQUEID, c.CHECKITEMUNIQUEID }
                                                         join i in db.CHECKITEM
                                                         on x.CHECKITEMUNIQUEID equals i.UNIQUEID
                                                         where x.JOBUNIQUEID == query.JobUniqueID && x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID
                                                         select new
                                                         {
                                                             UniqueID = c.CHECKITEMUNIQUEID,
                                                             i.ID,
                                                             i.DESCRIPTION,
                                                             LowerLimit = c.ISINHERIT == "Y" ? i.LOWERLIMIT : c.LOWERLIMIT,
                                                             LowerAlertLimit = c.ISINHERIT == "Y" ? i.LOWERALERTLIMIT : c.LOWERALERTLIMIT,
                                                             UpperAlertLimit = c.ISINHERIT == "Y" ? i.UPPERALERTLIMIT : c.UPPERALERTLIMIT,
                                                             UpperLimit = c.ISINHERIT == "Y" ? i.UPPERLIMIT : c.UPPERLIMIT,
                                                             Unit = c.ISINHERIT == "Y" ? i.UNIT : c.UNIT,
                                                             y.SEQ
                                                         }).OrderBy(x => x.SEQ).ToList();

                        foreach (var checkItem in controlPointCheckItemList)
                        {
                            var checkItemModel = new CheckItemModel()
                            {
                                EquipmentID = "",
                                EquipmentName = "",
                                PartDescription = "",
                                CheckItemID = checkItem.ID,
                                CheckItemDescription = checkItem.DESCRIPTION,
                                LowerLimit = checkItem.LowerLimit.HasValue?double.Parse(checkItem.LowerLimit.Value.ToString()):default(double?),
                                LowerAlertLimit = checkItem.LowerAlertLimit.HasValue ? double.Parse(checkItem.LowerAlertLimit.Value.ToString()) : default(double?),
                                UpperAlertLimit = checkItem.UpperAlertLimit.HasValue ? double.Parse(checkItem.UpperAlertLimit.Value.ToString()) : default(double?),
                                UpperLimit = checkItem.UpperLimit.HasValue ? double.Parse(checkItem.UpperLimit.Value.ToString()) : default(double?),
                                Unit = checkItem.Unit
                            };

                            var checkResultList = allCheckResultList.Where(x => x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID && string.IsNullOrEmpty(x.EQUIPMENTUNIQUEID) && string.IsNullOrEmpty(x.PARTUNIQUEID) && x.CHECKITEMUNIQUEID == checkItem.UniqueID).OrderBy(x => x.CHECKDATE).ThenBy(x => x.CHECKTIME).ToList();

                            foreach (var checkResult in checkResultList)
                            {
                                checkItemModel.CheckResultList.Add(new CheckResultModel()
                                {
                                    ArriveRecordUniqueID = checkResult.ARRIVERECORDUNIQUEID,
                                    CheckDate = checkResult.CHECKDATE,
                                    CheckTime = checkResult.CHECKTIME,
                                    IsAbnormal = checkResult.ISABNORMAL=="Y",
                                    IsAlert = checkResult.ISALERT=="Y",
                                    Result = checkResult.RESULT,
                                    LowerLimit = checkResult.LOWERLIMIT.HasValue ? double.Parse(checkResult.LOWERLIMIT.Value.ToString()) : default(double?),
                                    LowerAlertLimit = checkResult.LOWERALERTLIMIT.HasValue ? double.Parse(checkResult.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                                    UpperAlertLimit = checkResult.UPPERALERTLIMIT.HasValue ? double.Parse(checkResult.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                                    UpperLimit = checkResult.UPPERLIMIT.HasValue ? double.Parse(checkResult.UPPERLIMIT.Value.ToString()) : default(double?),
                                    Unit = checkResult.UNIT,
                                    Remark = checkResult.REMARK,
                                    AbnormalReasonList = db.CHECKRESULTABNORMALREASON.Where(a => a.CHECKRESULTUNIQUEID == checkResult.UNIQUEID).OrderBy(a => a.ABNORMALREASONID).Select(a => new AbnormalReasonModel
                                    {
                                        Description = a.ABNORMALREASONDESCRIPTION,
                                        Remark = a.ABNORMALREASONREMARK,
                                        HandlingMethodList = db.CHECKRESULTHANDLINGMETHOD.Where(h => h.CHECKRESULTUNIQUEID == checkResult.UNIQUEID && h.ABNORMALREASONUNIQUEID == a.ABNORMALREASONUNIQUEID).OrderBy(h => h.HANDLINGMETHODID).Select(h => new HandlingMethodModel
                                        {
                                            Description = h.HANDLINGMETHODDESCRIPTION,
                                            Remark = h.HANDLINGMETHODREMARK
                                        }).ToList()
                                    }).ToList()
                                });
                            }

                            controlPointModel.CheckItemList.Add(checkItemModel);
                        }

                        var equipmentList = (from x in db.JOBEQUIPMENT
                                             join j in db.JOB
                                             on x.JOBUNIQUEID equals j.UNIQUEID
                                             join y in db.ROUTEEQUIPMENT
                                             on new { j.ROUTEUNIQUEID, x.CONTROLPOINTUNIQUEID, x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID } equals new { y.ROUTEUNIQUEID, y.CONTROLPOINTUNIQUEID, y.EQUIPMENTUNIQUEID, y.PARTUNIQUEID }
                                             where x.JOBUNIQUEID == query.JobUniqueID && x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID
                                             select new { x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID, y.SEQ }).OrderBy(x => x.SEQ).ToList();

                        foreach (var equipment in equipmentList)
                        {
                            var equipmentCheckItemList = (from x in db.JOBEQUIPMENTCHECKITEM
                                                          join j in db.JOB
                                                          on x.JOBUNIQUEID equals j.UNIQUEID
                                                          join y in db.ROUTEEQUIPMENTCHECKITEM
                                                          on new { j.ROUTEUNIQUEID, x.CONTROLPOINTUNIQUEID, x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID, x.CHECKITEMUNIQUEID } equals new { y.ROUTEUNIQUEID, y.CONTROLPOINTUNIQUEID, y.EQUIPMENTUNIQUEID, y.PARTUNIQUEID, y.CHECKITEMUNIQUEID }
                                                          join e in db.EQUIPMENT
                                                          on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                                          join c in db.EQUIPMENTCHECKITEM
                                                          on new { x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID, x.CHECKITEMUNIQUEID } equals new { c.EQUIPMENTUNIQUEID, c.PARTUNIQUEID, c.CHECKITEMUNIQUEID }
                                                          join i in db.CHECKITEM
                                                          on x.CHECKITEMUNIQUEID equals i.UNIQUEID
                                                          where x.JOBUNIQUEID == query.JobUniqueID && x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID && x.EQUIPMENTUNIQUEID == equipment.EQUIPMENTUNIQUEID && x.PARTUNIQUEID == equipment.PARTUNIQUEID
                                                          select new
                                                          {
                                                              x.EQUIPMENTUNIQUEID,
                                                              EquipmentID = e.ID,
                                                              EquipmentName = e.NAME,
                                                              x.PARTUNIQUEID,
                                                              CheckItemUniqueID = x.CHECKITEMUNIQUEID,
                                                              CheckItemID = i.ID,
                                                              CheckItemDescription = i.DESCRIPTION,
                                                              LowerLimit = c.ISINHERIT == "Y" ? i.LOWERLIMIT : c.LOWERLIMIT,
                                                              LowerAlertLimit = c.ISINHERIT == "Y" ? i.LOWERALERTLIMIT : c.LOWERALERTLIMIT,
                                                              UpperAlertLimit = c.ISINHERIT == "Y" ? i.UPPERALERTLIMIT : c.UPPERALERTLIMIT,
                                                              UpperLimit = c.ISINHERIT == "Y" ? i.UPPERLIMIT : c.UPPERLIMIT,
                                                              Unit = c.ISINHERIT == "Y" ? i.UNIT : c.UNIT,
                                                              y.SEQ
                                                          }).OrderBy(x => x.SEQ).ToList();

                            foreach (var checkItem in equipmentCheckItemList)
                            {
                                var part = db.EQUIPMENTPART.FirstOrDefault(x => x.UNIQUEID == checkItem.PARTUNIQUEID);

                                var checkItemModel = new CheckItemModel()
                                {
                                    EquipmentID = checkItem.EquipmentID,
                                    EquipmentName = checkItem.EquipmentName,
                                    PartDescription = part!=null?part.DESCRIPTION:string.Empty,
                                    CheckItemID = checkItem.CheckItemID,
                                    CheckItemDescription = checkItem.CheckItemDescription,
                                    LowerLimit = checkItem.LowerLimit.HasValue?double.Parse(checkItem.LowerLimit.Value.ToString()):default(double?),
                                    LowerAlertLimit = checkItem.LowerAlertLimit.HasValue ? double.Parse(checkItem.LowerAlertLimit.Value.ToString()) : default(double?),
                                    UpperAlertLimit = checkItem.UpperAlertLimit.HasValue ? double.Parse(checkItem.UpperAlertLimit.Value.ToString()) : default(double?),
                                    UpperLimit = checkItem.UpperLimit.HasValue ? double.Parse(checkItem.UpperLimit.Value.ToString()) : default(double?),
                                    Unit = checkItem.Unit
                                };

                                var checkResultList = allCheckResultList.Where(x => x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID && x.EQUIPMENTUNIQUEID == checkItem.EQUIPMENTUNIQUEID && x.PARTUNIQUEID == checkItem.PARTUNIQUEID && x.CHECKITEMUNIQUEID == checkItem.CheckItemUniqueID).OrderBy(x => x.CHECKDATE).ThenBy(x => x.CHECKTIME).ToList();

                                foreach (var checkResult in checkResultList)
                                {
                                    checkItemModel.CheckResultList.Add(new CheckResultModel()
                                    {
                                        ArriveRecordUniqueID = checkResult.ARRIVERECORDUNIQUEID,
                                        CheckDate = checkResult.CHECKDATE,
                                        CheckTime = checkResult.CHECKTIME,
                                        IsAbnormal = checkResult.ISABNORMAL=="Y",
                                        IsAlert = checkResult.ISALERT=="Y",
                                        Result = checkResult.RESULT,
                                        LowerLimit = checkResult.LOWERLIMIT.HasValue?double.Parse(checkResult.LOWERLIMIT.Value.ToString()):default(double?),
                                        LowerAlertLimit = checkResult.LOWERALERTLIMIT.HasValue ? double.Parse(checkResult.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                                        UpperAlertLimit = checkResult.UPPERALERTLIMIT.HasValue ? double.Parse(checkResult.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                                        UpperLimit = checkResult.UPPERLIMIT.HasValue ? double.Parse(checkResult.UPPERLIMIT.Value.ToString()) : default(double?),
                                        Unit = checkResult.UNIT,
                                        Remark = checkResult.REMARK,
                                        AbnormalReasonList = db.CHECKRESULTABNORMALREASON.Where(a => a.CHECKRESULTUNIQUEID == checkResult.UNIQUEID).OrderBy(a => a.ABNORMALREASONID).Select(a => new AbnormalReasonModel
                                        {
                                            Description = a.ABNORMALREASONDESCRIPTION,
                                            Remark = a.ABNORMALREASONREMARK,
                                            HandlingMethodList = db.CHECKRESULTHANDLINGMETHOD.Where(h => h.CHECKRESULTUNIQUEID == checkResult.UNIQUEID && h.ABNORMALREASONUNIQUEID == a.ABNORMALREASONUNIQUEID).OrderBy(h => h.HANDLINGMETHODID).Select(h => new HandlingMethodModel
                                            {
                                                Description = h.HANDLINGMETHODDESCRIPTION,
                                                Remark = h.HANDLINGMETHODREMARK
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

        public static RequestResult GetVerifyFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = (from x in db.JOBRESULT
                                 join j in db.JOB
                                 on x.JOBUNIQUEID equals j.UNIQUEID
                                 join r in db.ROUTE
                                 on j.ROUTEUNIQUEID equals r.UNIQUEID
                                 where x.UNIQUEID == UniqueID
                                 select new
                                 {
                                     x.UNIQUEID,
                                     JobUniqueID = x.JOBUNIQUEID,
                                     OrganizationUniqueID = r.ORGANIZATIONUNIQUEID,
                                     Description = x.DESCRIPTION,
                                     BeginDate = x.BEGINDATE,
                                     EndDate = x.ENDDATE,
                                     BeginTime = x.BEGINTIME,
                                     EndTime = x.ENDTIME,
                                     JobUsers = x.JOBUSERS,
                                     CheckUsers = x.CHECKUSERS,
                                     CompleteRate = x.COMPLETERATE,
                                     CompleteRateLabelClass = x.COMPLETERATELABELCLASS,
                                     ArriveStatus = x.ARRIVESTATUS,
                                     ArriveStatusLabelClass = x.ARRIVESTATUSLABELCLASS,
                                     TimeSpan = x.TIMESPAN,
                                     UnPatrolReason = x.UNPATROLREASON,
                                     OverTimeReason = x.OVERTIMEREASON
                                 }).First();

                    var flow = db.JOBRESULTFLOW.FirstOrDefault(x => x.JOBRESULTUNIQUEID == query.UNIQUEID);

                    var model = new VerifyFormModel()
                    {
                        UniqueID = query.UNIQUEID,
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
                        FlowLogList = db.JOBRESULTFLOWLOG.Where(x => x.JOBRESULTUNIQUEID == query.UNIQUEID).OrderBy(x => x.SEQ).Select(x => new FlowLogModel
                        {
                            UserID = x.USERID,
                            UserName = x.USERNAME,
                            Remark = x.REMARK,
                            NotifyTime = x.NOTIFYTIME.Value,
                            VerifyTime = x.VERIFYTIME
                        }).ToList()
                    };

                    var unPatrolReasonList = db.UNPATROLREASON.OrderBy(x => x.ID).ToList();

                    foreach (var unPatrolReason in unPatrolReasonList)
                    {
                        model.UnPatrolReasonSelectItemList.Add(new SelectListItem()
                        {
                            Text = unPatrolReason.DESCRIPTION,
                            Value = unPatrolReason.UNIQUEID
                        });
                    }

                    model.UnPatrolReasonSelectItemList.Add(new SelectListItem()
                    {
                        Value = Define.OTHER,
                        Text = Resources.Resource.Other
                    });

                    var allArriveRecordList = db.ARRIVERECORD.Where(x => x.JOBRESULTUNIQUEID == query.UNIQUEID).ToList();

                    var allCheckResultList = (from c in db.CHECKRESULT
                                              join a in db.ARRIVERECORD
                                              on c.ARRIVERECORDUNIQUEID equals a.UNIQUEID
                                              where a.JOBRESULTUNIQUEID == query.UNIQUEID
                                              select c).ToList();

                    var controlPointList = (from x in db.JOBCONTROLPOINT
                                            join j in db.JOB
                                            on x.JOBUNIQUEID equals j.UNIQUEID
                                            join y in db.ROUTECONTROLPOINT
                                            on new { j.ROUTEUNIQUEID, x.CONTROLPOINTUNIQUEID } equals new { y.ROUTEUNIQUEID, y.CONTROLPOINTUNIQUEID }
                                            join c in db.CONTROLPOINT
                                            on x.CONTROLPOINTUNIQUEID equals c.UNIQUEID
                                            where x.JOBUNIQUEID == query.JobUniqueID
                                            select new
                                            {
                                                UniqueID = c.UNIQUEID,
                                                ID = c.ID,
                                                Description = c.DESCRIPTION,
                                                MinTimeSpan = x.MINTIMESPAN,
                                                y.SEQ
                                            }).OrderBy(x => x.SEQ).ToList();

                    foreach (var controlPoint in controlPointList)
                    {
                        var controlPointModel = new ControlPointModel()
                        {
                            ID = controlPoint.ID,
                            Description = controlPoint.Description,
                            MinTimeSpan = controlPoint.MinTimeSpan,
                            ArriveRecordList = allArriveRecordList.Where(x => x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID).OrderBy(x => x.ARRIVEDATE).ThenBy(x => x.ARRIVETIME).Select(x => new ArriveRecordModel
                            {
                                UniqueID = x.UNIQUEID,
                                ArriveDate = x.ARRIVEDATE,
                                ArriveTime = x.ARRIVETIME,
                                User = UserDataAccessor.GetUser(x.USERID),
                                UnRFIDReasonID = x.UNRFIDREASONID,
                                UnRFIDReasonDescription = x.UNRFIDREASONDESCRIPTION,
                                UnRFIDReasonRemark = x.UNRFIDREASONREMARK,
                                TimeSpanAbnormalReasonID = x.TIMESPANABNORMALREASONID,
                                TimeSpanAbnormalReasonDescription = x.TIMESPANABNORMALREASONDESC,
                                TimeSpanAbnormalReasonRemark = x.TIMESPANABNORMALREASONREMARK
                            }).ToList()
                        };

                        var controlPointCheckItemList = (from x in db.JOBCONTROLPOINTCHECKITEM
                                                         join j in db.JOB
                                                         on x.JOBUNIQUEID equals j.UNIQUEID
                                                         join y in db.ROUTECONTROLPOINTCHECKITEM
                                                         on new { j.ROUTEUNIQUEID, x.CONTROLPOINTUNIQUEID, x.CHECKITEMUNIQUEID } equals new { y.ROUTEUNIQUEID, y.CONTROLPOINTUNIQUEID, y.CHECKITEMUNIQUEID }
                                                         join c in db.CONTROLPOINTCHECKITEM
                                                         on new { x.CONTROLPOINTUNIQUEID, x.CHECKITEMUNIQUEID } equals new { c.CONTROLPOINTUNIQUEID, c.CHECKITEMUNIQUEID }
                                                         join i in db.CHECKITEM
                                                         on x.CHECKITEMUNIQUEID equals i.UNIQUEID
                                                         where x.JOBUNIQUEID == query.JobUniqueID && x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID
                                                         select new
                                                         {
                                                             UniqueID = c.CHECKITEMUNIQUEID,
                                                             i.ID,
                                                             i.DESCRIPTION,
                                                             LowerLimit = c.ISINHERIT == "Y" ? i.LOWERLIMIT : c.LOWERLIMIT,
                                                             LowerAlertLimit = c.ISINHERIT == "Y" ? i.LOWERALERTLIMIT : c.LOWERALERTLIMIT,
                                                             UpperAlertLimit = c.ISINHERIT == "Y" ? i.UPPERALERTLIMIT : c.UPPERALERTLIMIT,
                                                             UpperLimit = c.ISINHERIT == "Y" ? i.UPPERLIMIT : c.UPPERLIMIT,
                                                             Unit = c.ISINHERIT == "Y" ? i.UNIT : c.UNIT,
                                                             y.SEQ
                                                         }).OrderBy(x => x.SEQ).ToList();

                        foreach (var checkItem in controlPointCheckItemList)
                        {
                            var checkItemModel = new CheckItemModel()
                            {
                                EquipmentID = "",
                                EquipmentName = "",
                                PartDescription = "",
                                CheckItemID = checkItem.ID,
                                CheckItemDescription = checkItem.DESCRIPTION,
                                LowerLimit = checkItem.LowerLimit.HasValue ? double.Parse(checkItem.LowerLimit.Value.ToString()) : default(double?),
                                LowerAlertLimit = checkItem.LowerAlertLimit.HasValue ? double.Parse(checkItem.LowerAlertLimit.Value.ToString()) : default(double?),
                                UpperAlertLimit = checkItem.UpperAlertLimit.HasValue ? double.Parse(checkItem.UpperAlertLimit.Value.ToString()) : default(double?),
                                UpperLimit = checkItem.UpperLimit.HasValue ? double.Parse(checkItem.UpperLimit.Value.ToString()) : default(double?),
                                Unit = checkItem.Unit
                            };

                            var checkResultList = allCheckResultList.Where(x => x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID && string.IsNullOrEmpty(x.EQUIPMENTUNIQUEID) && string.IsNullOrEmpty(x.PARTUNIQUEID) && x.CHECKITEMUNIQUEID == checkItem.UniqueID).OrderBy(x => x.CHECKDATE).ThenBy(x => x.CHECKTIME).ToList();

                            foreach (var checkResult in checkResultList)
                            {
                                checkItemModel.CheckResultList.Add(new CheckResultModel()
                                {
                                    ArriveRecordUniqueID = checkResult.ARRIVERECORDUNIQUEID,
                                    CheckDate = checkResult.CHECKDATE,
                                    CheckTime = checkResult.CHECKTIME,
                                    IsAbnormal = checkResult.ISABNORMAL == "Y",
                                    IsAlert = checkResult.ISALERT == "Y",
                                    Result = checkResult.RESULT,
                                    LowerLimit = checkResult.LOWERLIMIT.HasValue ? double.Parse(checkResult.LOWERLIMIT.Value.ToString()) : default(double?),
                                    LowerAlertLimit = checkResult.LOWERALERTLIMIT.HasValue ? double.Parse(checkResult.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                                    UpperAlertLimit = checkResult.UPPERALERTLIMIT.HasValue ? double.Parse(checkResult.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                                    UpperLimit = checkResult.UPPERLIMIT.HasValue ? double.Parse(checkResult.UPPERLIMIT.Value.ToString()) : default(double?),
                                    Unit = checkResult.UNIT,
                                    Remark = checkResult.REMARK,
                                    AbnormalReasonList = db.CHECKRESULTABNORMALREASON.Where(a => a.CHECKRESULTUNIQUEID == checkResult.UNIQUEID).OrderBy(a => a.ABNORMALREASONID).Select(a => new AbnormalReasonModel
                                    {
                                        Description = a.ABNORMALREASONDESCRIPTION,
                                        Remark = a.ABNORMALREASONREMARK,
                                        HandlingMethodList = db.CHECKRESULTHANDLINGMETHOD.Where(h => h.CHECKRESULTUNIQUEID == checkResult.UNIQUEID && h.ABNORMALREASONUNIQUEID == a.ABNORMALREASONUNIQUEID).OrderBy(h => h.HANDLINGMETHODID).Select(h => new HandlingMethodModel
                                        {
                                            Description = h.HANDLINGMETHODDESCRIPTION,
                                            Remark = h.HANDLINGMETHODREMARK
                                        }).ToList()
                                    }).ToList()
                                });
                            }

                            controlPointModel.CheckItemList.Add(checkItemModel);
                        }

                        var equipmentList = (from x in db.JOBEQUIPMENT
                                             join j in db.JOB
                                             on x.JOBUNIQUEID equals j.UNIQUEID
                                             join y in db.ROUTEEQUIPMENT
                                             on new { j.ROUTEUNIQUEID, x.CONTROLPOINTUNIQUEID, x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID } equals new { y.ROUTEUNIQUEID, y.CONTROLPOINTUNIQUEID, y.EQUIPMENTUNIQUEID, y.PARTUNIQUEID }
                                             where x.JOBUNIQUEID == query.JobUniqueID && x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID
                                             select new { x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID, y.SEQ }).OrderBy(x => x.SEQ).ToList();

                        foreach (var equipment in equipmentList)
                        {
                            var equipmentCheckItemList = (from x in db.JOBEQUIPMENTCHECKITEM
                                                          join j in db.JOB
                                                          on x.JOBUNIQUEID equals j.UNIQUEID
                                                          join y in db.ROUTEEQUIPMENTCHECKITEM
                                                          on new { j.ROUTEUNIQUEID, x.CONTROLPOINTUNIQUEID, x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID, x.CHECKITEMUNIQUEID } equals new { y.ROUTEUNIQUEID, y.CONTROLPOINTUNIQUEID, y.EQUIPMENTUNIQUEID, y.PARTUNIQUEID, y.CHECKITEMUNIQUEID }
                                                          join e in db.EQUIPMENT
                                                          on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                                          join c in db.EQUIPMENTCHECKITEM
                                                          on new { x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID, x.CHECKITEMUNIQUEID } equals new { c.EQUIPMENTUNIQUEID, c.PARTUNIQUEID, c.CHECKITEMUNIQUEID }
                                                          join i in db.CHECKITEM
                                                          on x.CHECKITEMUNIQUEID equals i.UNIQUEID
                                                          where x.JOBUNIQUEID == query.JobUniqueID && x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID && x.EQUIPMENTUNIQUEID == equipment.EQUIPMENTUNIQUEID && x.PARTUNIQUEID == equipment.PARTUNIQUEID
                                                          select new
                                                          {
                                                              x.EQUIPMENTUNIQUEID,
                                                              EquipmentID = e.ID,
                                                              EquipmentName = e.NAME,
                                                              x.PARTUNIQUEID,
                                                              CheckItemUniqueID = x.CHECKITEMUNIQUEID,
                                                              CheckItemID = i.ID,
                                                              CheckItemDescription = i.DESCRIPTION,
                                                              LowerLimit = c.ISINHERIT == "Y" ? i.LOWERLIMIT : c.LOWERLIMIT,
                                                              LowerAlertLimit = c.ISINHERIT == "Y" ? i.LOWERALERTLIMIT : c.LOWERALERTLIMIT,
                                                              UpperAlertLimit = c.ISINHERIT == "Y" ? i.UPPERALERTLIMIT : c.UPPERALERTLIMIT,
                                                              UpperLimit = c.ISINHERIT == "Y" ? i.UPPERLIMIT : c.UPPERLIMIT,
                                                              Unit = c.ISINHERIT == "Y" ? i.UNIT : c.UNIT,
                                                              y.SEQ
                                                          }).OrderBy(x => x.SEQ).ToList();

                            foreach (var checkItem in equipmentCheckItemList)
                            {
                                var part = db.EQUIPMENTPART.FirstOrDefault(x => x.UNIQUEID == checkItem.PARTUNIQUEID);

                                var checkItemModel = new CheckItemModel()
                                {
                                    EquipmentID = checkItem.EquipmentID,
                                    EquipmentName = checkItem.EquipmentName,
                                    PartDescription = part != null ? part.DESCRIPTION : string.Empty,
                                    CheckItemID = checkItem.CheckItemID,
                                    CheckItemDescription = checkItem.CheckItemDescription,
                                    LowerLimit = checkItem.LowerLimit.HasValue ? double.Parse(checkItem.LowerLimit.Value.ToString()) : default(double?),
                                    LowerAlertLimit = checkItem.LowerAlertLimit.HasValue ? double.Parse(checkItem.LowerAlertLimit.Value.ToString()) : default(double?),
                                    UpperAlertLimit = checkItem.UpperAlertLimit.HasValue ? double.Parse(checkItem.UpperAlertLimit.Value.ToString()) : default(double?),
                                    UpperLimit = checkItem.UpperLimit.HasValue ? double.Parse(checkItem.UpperLimit.Value.ToString()) : default(double?),
                                    Unit = checkItem.Unit
                                };

                                var checkResultList = allCheckResultList.Where(x => x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID && x.EQUIPMENTUNIQUEID == checkItem.EQUIPMENTUNIQUEID && x.PARTUNIQUEID == checkItem.PARTUNIQUEID && x.CHECKITEMUNIQUEID == checkItem.CheckItemUniqueID).OrderBy(x => x.CHECKDATE).ThenBy(x => x.CHECKTIME).ToList();

                                foreach (var checkResult in checkResultList)
                                {
                                    checkItemModel.CheckResultList.Add(new CheckResultModel()
                                    {
                                        ArriveRecordUniqueID = checkResult.ARRIVERECORDUNIQUEID,
                                        CheckDate = checkResult.CHECKDATE,
                                        CheckTime = checkResult.CHECKTIME,
                                        IsAbnormal = checkResult.ISABNORMAL == "Y",
                                        IsAlert = checkResult.ISALERT == "Y",
                                        Result = checkResult.RESULT,
                                        LowerLimit = checkResult.LOWERLIMIT.HasValue ? double.Parse(checkResult.LOWERLIMIT.Value.ToString()) : default(double?),
                                        LowerAlertLimit = checkResult.LOWERALERTLIMIT.HasValue ? double.Parse(checkResult.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                                        UpperAlertLimit = checkResult.UPPERALERTLIMIT.HasValue ? double.Parse(checkResult.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                                        UpperLimit = checkResult.UPPERLIMIT.HasValue ? double.Parse(checkResult.UPPERLIMIT.Value.ToString()) : default(double?),
                                        Unit = checkResult.UNIT,
                                        Remark = checkResult.REMARK,
                                        AbnormalReasonList = db.CHECKRESULTABNORMALREASON.Where(a => a.CHECKRESULTUNIQUEID == checkResult.UNIQUEID).OrderBy(a => a.ABNORMALREASONID).Select(a => new AbnormalReasonModel
                                        {
                                            Description = a.ABNORMALREASONDESCRIPTION,
                                            Remark = a.ABNORMALREASONREMARK,
                                            HandlingMethodList = db.CHECKRESULTHANDLINGMETHOD.Where(h => h.CHECKRESULTUNIQUEID == checkResult.UNIQUEID && h.ABNORMALREASONUNIQUEID == a.ABNORMALREASONUNIQUEID).OrderBy(h => h.HANDLINGMETHODID).Select(h => new HandlingMethodModel
                                            {
                                                Description = h.HANDLINGMETHODDESCRIPTION,
                                                Remark = h.HANDLINGMETHODREMARK
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
        //        using (ASEDbEntities db = new ASEDbEntities())
        //        {
        //            var flow = db.JOBRESULTFLOW.First(x => x.JOBRESULTUNIQUEID == UniqueID);

        //            var currentFlowLog = db.JOBRESULTFLOWLOG.Where(x => x.JOBRESULTUNIQUEID == UniqueID && x.SEQ == flow.CURRENTSEQ).First();

        //            if (currentFlowLog.FLOWSEQ.Value == 0)
        //            {
        //                flow.ISCLOSED = "Y";

        //                currentFlowLog.VERIFYTIME = DateTime.Now;
        //                currentFlowLog.REMARK = Remark;

        //                db.SaveChanges();

        //                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Confirm, Resources.Resource.Complete));
        //            }
        //            else
        //            {
        //                var route = (from x in db.JOBRESULT
        //                             join j in db.JOB
        //                             on x.JOBUNIQUEID equals j.UNIQUEID
        //                             join r in db.ROUTE
        //                             on j.ROUTEUNIQUEID equals r.UNIQUEID
        //                             where x.UNIQUEID == UniqueID
        //                             select r).First();

        //                var nextVerifyOrganization = (from f in db.FLOW
        //                                              join x in db.FLOWFORM
        //                                              on f.UNIQUEID equals x.FLOWUNIQUEID
        //                                              join v in db.FLOWVERIFYORGANIZATION
        //                                              on f.UNIQUEID equals v.FLOWUNIQUEID
        //                                              join o in db.ORGANIZATION
        //                                              on v.ORGANIZATIONUNIQUEID equals o.UNIQUEID
        //                                              where f.ORGANIZATIONUNIQUEID == route.ORGANIZATIONUNIQUEID && x.FORM == Define.EnumForm.EquipmentPatrolResult.ToString()
        //                                              select new
        //                                              {
        //                                                  o.UNIQUEID,
        //                                                  o.DESCRIPTION,
        //                                                  o.MANAGERUSERID,
        //                                                  v.SEQ
        //                                              }).Where(x => x.SEQ > currentFlowLog.FLOWSEQ).OrderBy(x => x.SEQ).FirstOrDefault();

        //                if (nextVerifyOrganization != null)
        //                {
        //                    if (!string.IsNullOrEmpty(nextVerifyOrganization.MANAGERUSERID))
        //                    {
        //                        //var user = UserDataAccessor.GetUser(nextVerifyOrganization.MANAGERUSERID);
        //                        var user = db.ACCOUNT.First(x => x.ID == nextVerifyOrganization.MANAGERUSERID);

        //                        flow.CURRENTSEQ = flow.CURRENTSEQ.Value + 1;

        //                        currentFlowLog.VERIFYTIME = DateTime.Now;
        //                        currentFlowLog.REMARK = Remark;

        //                        db.JOBRESULTFLOWLOG.Add(new JOBRESULTFLOWLOG()
        //                        {
        //                            JOBRESULTUNIQUEID = UniqueID,
        //                            SEQ = flow.CURRENTSEQ.Value,
        //                            FLOWSEQ = nextVerifyOrganization.SEQ,
        //                            USERNAME = user.NAME,
        //                            USERID = nextVerifyOrganization.MANAGERUSERID,
        //                            NOTIFYTIME = DateTime.Now
        //                        });

        //                        if (Config.HaveMailSetting)
        //                        {
        //                            SendVerifyMail(user, db.JOBRESULT.First(x => x.UNIQUEID == UniqueID));
        //                        }

        //                        db.SaveChanges();

        //                        result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Confirm, Resources.Resource.Complete));
        //                    }
        //                    else
        //                    {
        //                        result.ReturnFailedMessage(string.Format("{0} {1} {2} {3}", Resources.Resource.Organization, nextVerifyOrganization.DESCRIPTION, Resources.Resource.NotSet, Resources.Resource.Manager));
        //                    }
        //                }
        //                else
        //                {
        //                    flow.ISCLOSED = "Y";

        //                    currentFlowLog.VERIFYTIME = DateTime.Now;
        //                    currentFlowLog.REMARK = Remark;

        //                    db.SaveChanges();

        //                    result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Confirm, Resources.Resource.Complete));
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

        public static RequestResult Confirm(List<string> VerifyResultList, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    foreach (var verifyResult in VerifyResultList)
                    {
                        string[] temp = verifyResult.Split(Define.Seperators, StringSplitOptions.None);

                        string uniqueID = temp[0];
                        string action = temp[1];
                        string remark = temp[2];

                        var flow = db.JOBRESULTFLOW.First(x => x.JOBRESULTUNIQUEID == uniqueID);

                        var currentFlowLog = db.JOBRESULTFLOWLOG.Where(x => x.JOBRESULTUNIQUEID == uniqueID && x.SEQ == flow.CURRENTSEQ).First();

                        if (currentFlowLog.FLOWSEQ.Value == 0)
                        {
                            flow.ISCLOSED = "Y";

                            currentFlowLog.VERIFYTIME = DateTime.Now;
                            currentFlowLog.REMARK = remark;

                            db.SaveChanges();

                            result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Confirm, Resources.Resource.Complete));
                        }
                        else
                        {
                            var route = (from x in db.JOBRESULT
                                         join j in db.JOB
                                         on x.JOBUNIQUEID equals j.UNIQUEID
                                         join r in db.ROUTE
                                         on j.ROUTEUNIQUEID equals r.UNIQUEID
                                         where x.UNIQUEID == uniqueID
                                         select r).First();

                            var nextVerifyOrganization = (from f in db.FLOW
                                                          join x in db.FLOWFORM
                                                          on f.UNIQUEID equals x.FLOWUNIQUEID
                                                          join v in db.FLOWVERIFYORGANIZATION
                                                          on f.UNIQUEID equals v.FLOWUNIQUEID
                                                          join o in db.ORGANIZATION
                                                          on v.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                                          where f.ORGANIZATIONUNIQUEID == route.ORGANIZATIONUNIQUEID && x.FORM == Define.EnumForm.EquipmentPatrolResult.ToString()
                                                          select new
                                                          {
                                                              o.UNIQUEID,
                                                              o.DESCRIPTION,
                                                              o.MANAGERUSERID,
                                                              v.SEQ
                                                          }).Where(x => x.SEQ > currentFlowLog.FLOWSEQ).OrderBy(x => x.SEQ).FirstOrDefault();

                            if (nextVerifyOrganization != null)
                            {
                                if (!string.IsNullOrEmpty(nextVerifyOrganization.MANAGERUSERID))
                                {
                                    //var user = UserDataAccessor.GetUser(nextVerifyOrganization.MANAGERUSERID);
                                    var user = db.ACCOUNT.First(x => x.ID == nextVerifyOrganization.MANAGERUSERID);

                                    flow.CURRENTSEQ = flow.CURRENTSEQ.Value + 1;

                                    currentFlowLog.VERIFYTIME = DateTime.Now;
                                    currentFlowLog.REMARK = remark;

                                    db.JOBRESULTFLOWLOG.Add(new JOBRESULTFLOWLOG()
                                    {
                                        JOBRESULTUNIQUEID = uniqueID,
                                        SEQ = flow.CURRENTSEQ.Value,
                                        FLOWSEQ = nextVerifyOrganization.SEQ,
                                        USERNAME = user.NAME,
                                        USERID = nextVerifyOrganization.MANAGERUSERID,
                                        NOTIFYTIME = DateTime.Now
                                    });

                                    if (Config.HaveMailSetting)
                                    {
                                        SendVerifyMail(user, db.JOBRESULT.First(x => x.UNIQUEID == uniqueID));
                                    }

                                    db.SaveChanges();

                                    result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Confirm, Resources.Resource.Complete));
                                }
                                else
                                {
                                    result.ReturnFailedMessage(string.Format("{0} {1} {2} {3}", Resources.Resource.Organization, nextVerifyOrganization.DESCRIPTION, Resources.Resource.NotSet, Resources.Resource.Manager));
                                }
                            }
                            else
                            {
                                flow.ISCLOSED = "Y";

                                currentFlowLog.VERIFYTIME = DateTime.Now;
                                currentFlowLog.REMARK = remark;

                                db.SaveChanges();

                                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Confirm, Resources.Resource.Complete));
                            }
                        }
                    }

                    result.ReturnSuccessMessage("確認完成");
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

        public static RequestResult Confirm(string UniqueID, VerifyFormInput FormInput)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (!string.IsNullOrEmpty(FormInput.UnPatrolReasonUniqueID))
                    {
                        var jobResult = db.JOBRESULT.First(x => x.UNIQUEID == UniqueID);

                        if (FormInput.UnPatrolReasonUniqueID == Define.OTHER)
                        {
                            jobResult.UNPATROLREASON = FormInput.UnPatrolReasonRemark;
                        }
                        else
                        {
                            var unPatrolReason = db.UNPATROLREASON.First(x => x.UNIQUEID == FormInput.UnPatrolReasonUniqueID);

                            jobResult.UNPATROLREASON = unPatrolReason.DESCRIPTION;
                        }
                    }

                    var flow = db.JOBRESULTFLOW.First(x => x.JOBRESULTUNIQUEID == UniqueID);

                    var currentFlowLog = db.JOBRESULTFLOWLOG.Where(x => x.JOBRESULTUNIQUEID == UniqueID && x.SEQ == flow.CURRENTSEQ).First();

                    if (currentFlowLog.FLOWSEQ.Value == 0)
                    {
                        flow.ISCLOSED = "Y";

                        currentFlowLog.VERIFYTIME = DateTime.Now;
                        currentFlowLog.REMARK = FormInput.VerifyComment;

                        db.SaveChanges();

                        result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Confirm, Resources.Resource.Complete));
                    }
                    else
                    {
                        var route = (from x in db.JOBRESULT
                                     join j in db.JOB
                                     on x.JOBUNIQUEID equals j.UNIQUEID
                                     join r in db.ROUTE
                                     on j.ROUTEUNIQUEID equals r.UNIQUEID
                                     where x.UNIQUEID == UniqueID
                                     select r).First();

                        var nextVerifyOrganization = (from f in db.FLOW
                                                      join x in db.FLOWFORM
                                                      on f.UNIQUEID equals x.FLOWUNIQUEID
                                                      join v in db.FLOWVERIFYORGANIZATION
                                                      on f.UNIQUEID equals v.FLOWUNIQUEID
                                                      join o in db.ORGANIZATION
                                                      on v.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                                      where f.ORGANIZATIONUNIQUEID == route.ORGANIZATIONUNIQUEID && x.FORM == Define.EnumForm.EquipmentPatrolResult.ToString()
                                                      select new
                                                      {
                                                          o.UNIQUEID,
                                                          o.DESCRIPTION,
                                                          o.MANAGERUSERID,
                                                          v.SEQ
                                                      }).Where(x => x.SEQ > currentFlowLog.FLOWSEQ).OrderBy(x => x.SEQ).FirstOrDefault();

                        if (nextVerifyOrganization != null)
                        {
                            if (!string.IsNullOrEmpty(nextVerifyOrganization.MANAGERUSERID))
                            {
                                //var user = UserDataAccessor.GetUser(nextVerifyOrganization.MANAGERUSERID);
                                var user = db.ACCOUNT.First(x => x.ID == nextVerifyOrganization.MANAGERUSERID);

                                flow.CURRENTSEQ = flow.CURRENTSEQ.Value + 1;

                                currentFlowLog.VERIFYTIME = DateTime.Now;
                                currentFlowLog.REMARK = FormInput.VerifyComment;

                                db.JOBRESULTFLOWLOG.Add(new JOBRESULTFLOWLOG()
                                {
                                    JOBRESULTUNIQUEID = UniqueID,
                                    SEQ = flow.CURRENTSEQ.Value,
                                    FLOWSEQ = nextVerifyOrganization.SEQ,
                                    USERNAME = user.NAME,
                                    USERID = nextVerifyOrganization.MANAGERUSERID,
                                    NOTIFYTIME = DateTime.Now
                                });

                                if (Config.HaveMailSetting)
                                {
                                    SendVerifyMail(user, db.JOBRESULT.First(x => x.UNIQUEID == UniqueID));
                                }

                                db.SaveChanges();

                                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Confirm, Resources.Resource.Complete));
                            }
                            else
                            {
                                result.ReturnFailedMessage(string.Format("{0} {1} {2} {3}", Resources.Resource.Organization, nextVerifyOrganization.DESCRIPTION, Resources.Resource.NotSet, Resources.Resource.Manager));
                            }
                        }
                        else
                        {
                            flow.ISCLOSED = "Y";

                            currentFlowLog.VERIFYTIME = DateTime.Now;
                            currentFlowLog.REMARK = FormInput.VerifyComment;

                            db.SaveChanges();

                            result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Confirm, Resources.Resource.Complete));
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

        private static void SendVerifyMail(ACCOUNT Account, JOBRESULT JobResult)
        {
            try
            {
                var beginTime = JobResult.BEGINDATE;

                if (!string.IsNullOrEmpty(JobResult.BEGINTIME))
                {
                    beginTime = string.Format("{0} {1}", JobResult.BEGINDATE, JobResult.BEGINTIME);
                }

                var endTime = JobResult.ENDDATE;

                if (!string.IsNullOrEmpty(JobResult.ENDTIME))
                {
                    endTime = string.Format("{0} {1}", JobResult.ENDDATE, JobResult.ENDTIME);
                }

                var checkDate = string.Empty;

                if (beginTime == endTime)
                {
                    checkDate = beginTime;
                }
                else
                {
                    checkDate = string.Format("{0}~{1}", beginTime, endTime);
                }

                var subject = string.Format("[巡檢結果簽核通知][{0}]{1}({2})", JobResult.ORGANIZATIONDESCRIPTION, JobResult.DESCRIPTION, checkDate);

                var th = "<th style=\"width:150px;border:1px solid #333;padding:8px;text-align:right;color:#707070;background: #F4F4F4;\">{0}</th>";
                var td = "<td style=\"width:400px;border:1px solid #333;padding:8px;color:#707070;\">{0}</td>";

                var sb = new StringBuilder();

                sb.Append("<table style=\"1px solid #ddd;font-size:13px;border-collapse:collapse;\">");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "組織"));
                sb.Append(string.Format(td, JobResult.ORGANIZATIONDESCRIPTION));
                sb.Append("</tr>");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "巡檢路線派工"));
                sb.Append(string.Format(td, JobResult.DESCRIPTION));
                sb.Append("</tr>");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "派工開始日期"));
                sb.Append(string.Format(td, JobResult.BEGINDATE));
                sb.Append("</tr>");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "派工開始時間"));
                sb.Append(string.Format(td, JobResult.BEGINTIME));
                sb.Append("</tr>");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "派工結束日期"));
                sb.Append(string.Format(td, JobResult.ENDDATE));
                sb.Append("</tr>");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "派工結束時間"));
                sb.Append(string.Format(td, JobResult.ENDTIME));
                sb.Append("</tr>");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "完成率"));
                sb.Append(string.Format(td, JobResult.COMPLETERATE));
                sb.Append("</tr>");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "檢查人員"));
                sb.Append(string.Format(td, JobResult.CHECKUSERS));
                sb.Append("</tr>");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "詳細資料"));
                sb.Append(string.Format(td, string.Format("<a href=\"http://ASECL-eINSPRD01/FEM.Portal/Home/Index?ReturnUrl=http://ASECL-eINSPRD01/FEM/zh-tw/EquipmentMaintenance/ResultVerify/Index?JobResultUniqueID=" + JobResult.UNIQUEID + "\">連結</a>")));
                //sb.Append(string.Format(td, string.Format("<a href=\"http://ASECL-eINSQAS/FEM.Portal/Home/Index?ReturnUrl=http://ASECL-eINSQAS/FEM/zh-tw/EquipmentMaintenance/ResultVerify/Index?JobResultUniqueID=" + JobResult.UNIQUEID + "\">連結</a>")));
                sb.Append("</tr>");

                sb.Append("</table>");

                MailHelper.SendMail(new MailAddress(Account.EMAIL, Account.NAME), subject, sb.ToString());
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }
    }
}
