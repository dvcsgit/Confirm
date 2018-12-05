using DbEntity.MSSQL;
using DbEntity.MSSQL.PipelinePatrol;
using Models.Authenticated;
using Models.PipelinePatrol.ResultQuery;
using Models.PipelinePatrol.Shared;
using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace DataAccess.PipelinePatrol
{
    public class ResultQueryHelper
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = new List<JobModel>();

                var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                using (PDbEntities db = new PDbEntities())
                {
                    var query = (from a in db.ArriveRecord
                                 join j in db.Job
                                 on a.JobUniqueID equals j.UniqueID
                                 where downStreamOrganizationList.Contains(j.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(j.OrganizationUniqueID) && string.Compare(a.ArriveDate, Parameters.BeginDate) >= 0 && string.Compare(a.ArriveDate, Parameters.EndDate) <= 0
                                 select new
                                 {
                                     j.UniqueID,
                                     j.ID,
                                     j.Description,
                                     CheckDate = a.ArriveDate
                                 }).Distinct().AsQueryable();
                  
                    if (!string.IsNullOrEmpty(Parameters.JobUniqueID))
                    {
                        query = query.Where(x => x.UniqueID == Parameters.JobUniqueID);
                    }

                    var jobList = query.OrderByDescending(x => x.CheckDate).ThenBy(x => x.ID).ToList();

                    var temp = jobList.Select(x => x.UniqueID).Distinct().ToList();

                    var arriveRecordList = db.ArriveRecord.Where(x =>temp.Contains(x.JobUniqueID)&& string.Compare(x.ArriveDate, Parameters.BeginDate) >= 0 && string.Compare(x.ArriveDate, Parameters.EndDate) <= 0).ToList();

                    var checkResultList = db.CheckResult.Where(x => temp.Contains(x.JobUniqueID) && string.Compare(x.CheckDate, Parameters.BeginDate) >= 0 && string.Compare(x.CheckDate, Parameters.EndDate) <= 0).ToList();

                    foreach (var job in jobList)
                    {
                        var jobModel = new JobModel()
                        {
                            UniqueID = job.UniqueID,
                            ID = job.ID,
                            Description = job.Description,
                            CheckDate = job.CheckDate
                        };

                        var userList = db.JobUserLocus.Where(x => x.JobUniqueID == job.UniqueID && x.CheckDate == job.CheckDate).Select(x => x.UserID).Distinct().ToList();

                        foreach (var userID in userList)
                        {
                            jobModel.UserLocusList.Add(new UserLocus()
                            {
                                User = UserDataAccessor.GetUser(userID),
                                Locus = db.JobUserLocus.Where(l => l.JobUniqueID == job.UniqueID && l.CheckDate == job.CheckDate && l.UserID == userID).GroupBy(l => new { l.LNG, l.LAT }).Select(l => new
                                {
                                    l.Key,
                                    CheckTime = l.Min(x => x.CheckTime)
                                }).ToList().Select(x => new UserLocation
                                {
                                    Date = job.CheckDate,
                                    Time = x.CheckTime,
                                    LAT = x.Key.LAT,
                                    LNG = x.Key.LNG
                                }).OrderBy(x => x.Date).ThenBy(x => x.Time).ToList()
                            });
                        }

                        var pipelineList = (from x in db.JobRoute
                                            join r in db.Route
                                            on x.RouteUniqueID equals r.UniqueID
                                            join y in db.RoutePipeline
                                            on r.UniqueID equals y.RouteUniqueID
                                            join p in db.Pipeline
                                            on y.PipelineUniqueID equals p.UniqueID
                                            where x.JobUniqueID == job.UniqueID
                                            select p).Distinct().ToList();

                        foreach (var pipeline in pipelineList)
                        {
                            jobModel.PipelineList.Add(new PipelineViewModel()
                            {
                                UniqueID = pipeline.UniqueID,
                                ID = pipeline.ID,
                                Color = pipeline.Color,
                                Locus = db.PipelineLocus.Where(x => x.PipelineUniqueID == pipeline.UniqueID).OrderBy(x => x.Seq).Select(x => new Location()
                                {
                                    LAT = x.LAT,
                                    LNG = x.LNG
                                }).ToList()
                            });
                        }

                        var routeList = (from x in db.JobRoute
                                         join r in db.Route
                                         on x.RouteUniqueID equals r.UniqueID
                                         where x.JobUniqueID == job.UniqueID
                                         select r).OrderBy(x => x.ID).ToList();

                        foreach (var route in routeList)
                        {
                            var routeModel = new RouteModel()
                            {
                                UniqueID = route.UniqueID,
                                ID = route.ID,
                                Name = route.Name
                            };

                            var pipePointList = (from x in db.RouteCheckPoint
                                                 join p in db.PipePoint
                                                 on x.PipePointUniqueID equals p.UniqueID
                                                 where x.RouteUniqueID == route.UniqueID
                                                 select new
                                                 {
                                                     p.UniqueID,
                                                     p.ID,
                                                     p.Name,
                                                     x.MinTimeSpan,
                                                     x.Seq
                                                 }).OrderBy(x => x.Seq).ToList();

                            foreach (var pipePoint in pipePointList)
                            {
                                var pipePointModel = new PipePointModel()
                                {
                                    UniqueID = pipePoint.UniqueID,
                                    RouteDisplay = routeModel.Display,
                                    JobUniqueID = job.UniqueID,
                                    CheckDate = job.CheckDate,
                                    ID = pipePoint.ID,
                                    Name = pipePoint.Name,
                                    MinTimeSpan = pipePoint.MinTimeSpan,
                                    ArriveRecordList = arriveRecordList.Where(x => x.JobUniqueID == job.UniqueID && x.RouteUniqueID == route.UniqueID && x.PipePointUniqueID == pipePoint.UniqueID && x.ArriveDate == job.CheckDate).Select(x => new ArriveRecordModel
                                    {
                                        UniqueID = x.UniqueID,
                                        ArriveDate = x.ArriveDate,
                                        ArriveTime = x.ArriveTime,
                                        LAT = x.LAT,
                                        LNG = x.LNG,
                                        UserID = x.UserID,
                                        UserName = x.UserName,
                                        TimeSpanAbnormalReasonDescription = x.TimeSpanAbnormalReasonDescription,
                                        TimeSpanAbnormalReasonRemark = x.TimeSpanAbnormalReasonRemark
                                    }).OrderBy(x=>x.ArriveDate).ThenBy(x=>x.ArriveTime).ToList()
                                };

                                var checkItemList = (from x in db.RouteCheckPointCheckItem
                                                     join c in db.View_PipePointCheckItem
                                                     on new { x.PipePointUniqueID, x.CheckItemUniqueID } equals new { c.PipePointUniqueID, c.CheckItemUniqueID }
                                                     where x.RouteUniqueID == route.UniqueID && x.PipePointUniqueID == pipePoint.UniqueID
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
                                                         x.Seq
                                                     }).OrderBy(x => x.Seq).ToList();

                                foreach (var checkItem in checkItemList)
                                {
                                    pipePointModel.CheckItemList.Add(new CheckItemModel()
                                    {
                                        UniqueID = checkItem.UniqueID,
                                        ID = checkItem.ID,
                                        Description = checkItem.Description,
                                        LowerLimit = checkItem.LowerLimit,
                                        LowerAlertLimit = checkItem.LowerAlertLimit,
                                        UpperAlertLimit = checkItem.UpperAlertLimit,
                                        UpperLimit = checkItem.UpperLimit,
                                        Unit = checkItem.Unit,
                                        CheckResultList = checkResultList.Where(x => x.JobUniqueID == job.UniqueID && x.RouteUniqueID == route.UniqueID && x.PipePointUniqueID == pipePoint.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID && x.CheckDate == job.CheckDate).Select(x => new CheckResultModel
                                        {
                                            UniqueID = x.UniqueID,
                                            ArriveRecordUniqueID = x.ArriveRecordUniqueID,
                                            CheckDate = x.CheckDate,
                                            CheckTime = x.CheckTime,
                                            IsAbnormal = x.IsAbnormal,
                                            IsAlert = x.IsAlert,
                                            Result = x.Result,
                                            PhotoList = db.CheckResultPhoto.Where(p => p.CheckResultUniqueID == x.UniqueID).Select(p => p.CheckResultUniqueID + "_" + p.Seq + "." + p.Extension).ToList(),
                                            AbnormalReasonList = db.CheckResultAbnormalReason.Where(a => a.CheckResultUniqueID == x.UniqueID).OrderBy(a => a.AbnormalReasonID).Select(a => new AbnormalReasonModel
                                            {
                                                Description = a.AbnormalReasonDescription,
                                                Remark = a.AbnormalReasonRemark,
                                                HandlingMethodList = db.CheckResultHandlingMethod.Where(h => h.CheckResultUniqueID == x.UniqueID && h.AbnormalReasonUniqueID == a.AbnormalReasonUniqueID).OrderBy(h => h.HandlingMethodID).Select(h => new HandlingMethodModel
                                                {
                                                    Description = h.HandlingMethodDescription,
                                                    Remark = h.HandlingMethodRemark
                                                }).ToList()
                                            }).ToList()
                                        }).OrderBy(x=>x.CheckDate).ThenBy(x=>x.CheckTime).ToList()
                                    });
                                }

                                routeModel.PipePointList.Add(pipePointModel);
                            }

                            jobModel.RouteList.Add(routeModel);
                        }

                        itemList.Add(jobModel);
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

                using (PDbEntities pdb = new PDbEntities())
                {
                    if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                    {
                        var jobList = pdb.Job.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                        foreach (var job in jobList)
                        {
                            var treeItem = new TreeItem() { Title = job.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Job.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", job.ID, job.Description);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.JobUniqueID] = job.UniqueID;
                            attributes[Define.EnumTreeAttribute.RouteUniqueID] = string.Empty;

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
                            attributes[Define.EnumTreeAttribute.RouteUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            if (db.Organization.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                                ||
                                (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && pdb.Job.Any(x => x.OrganizationUniqueID == organization.UniqueID)))
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

        public static RequestResult Export(List<JobModel> Model, Define.EnumExcelVersion ExcelVersion)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ExcelHelper helper = new ExcelHelper(string.Format("{0}_{1}({2})", Resources.Resource.EquipmentPatrolResult, Resources.Resource.ExportTime, DateTimeHelper.DateTime2DateTimeString(DateTime.Now)), ExcelVersion))
                {
                    var jobExcelItemList = new List<JobExcelItem>();
                    var routeExcelItemList = new List<RouteExcelItem>();
                    var checkItemExcelItemList = new List<CheckItemExcelItem>();

                    foreach (var job in Model)
                    {
                        foreach (var route in job.RouteList)
                        {
                            jobExcelItemList.Add(new JobExcelItem()
                            {
                                CheckDate = job.CheckDate,
                                User = job.CheckUser,
                                IsJobAbnormal = job.HaveAbnormal ? Resources.Resource.Abnormal : (job.HaveAlert ? Resources.Resource.Warning : ""),
                                Job = job.Display,
                                JobCompleteRate = job.CompleteRate,
                                JobTimeSpan = job.TimeSpan,
                                IsRouteAbnormal = route.HaveAbnormal ? Resources.Resource.Abnormal : (route.HaveAlert ? Resources.Resource.Warning : ""),
                                Route = route.Display,
                                RouteCompleteRate = route.CompleteRate,
                                RouteTimeSpan = route.TimeSpan
                            });

                            foreach (var pipePoint in route.PipePointList)
                            {
                                if (pipePoint.ArriveRecordList.Count > 0)
                                {
                                    foreach (var arriveRecord in pipePoint.ArriveRecordList)
                                    {
                                        routeExcelItemList.Add(new RouteExcelItem()
                                        {
                                            IsAbnormal = pipePoint.HaveAbnormal ? Resources.Resource.Abnormal : (pipePoint.HaveAlert ? Resources.Resource.Warning : ""),
                                            Route = route.Display,
                                            PipePoint = pipePoint.Display,
                                            CompleteRate = pipePoint.CompleteRate,
                                            TimeSpan = pipePoint.TimeSpan,
                                            ArriveDate = arriveRecord.ArriveDate,
                                            ArriveTime = arriveRecord.ArriveTime,
                                            User = arriveRecord.User,
                                            IsTimeSpanAbnormal = pipePoint.IsTimeSpanAbnormal ? Resources.Resource.Abnormal : "",
                                            MinTimeSpan = pipePoint.MinTimeSpan.HasValue ? pipePoint.MinTimeSpan.Value.ToString() : "",
                                            TimeSpanAbnormalReason = arriveRecord.TimeSpanAbnormalReason
                                        });
                                    }
                                }
                                else
                                {
                                    routeExcelItemList.Add(new RouteExcelItem()
                                    {
                                        IsAbnormal = pipePoint.HaveAbnormal ? Resources.Resource.Abnormal : (pipePoint.HaveAlert ? Resources.Resource.Warning : ""),
                                        Route = route.Display,
                                        PipePoint = pipePoint.Display,
                                        CompleteRate = pipePoint.CompleteRate,
                                        TimeSpan = pipePoint.TimeSpan,
                                        ArriveDate = string.Empty,
                                        ArriveTime = string.Empty,
                                        User = string.Empty,
                                        IsTimeSpanAbnormal = string.Empty,
                                        MinTimeSpan = pipePoint.MinTimeSpan.HasValue ? pipePoint.MinTimeSpan.Value.ToString() : "",
                                        TimeSpanAbnormalReason = string.Empty
                                    });
                                }

                                foreach (var checkItem in pipePoint.CheckItemList)
                                {
                                    if (checkItem.CheckResultList.Count > 0)
                                    {
                                        foreach (var checkResult in checkItem.CheckResultList)
                                        {
                                            var arriveRecord = pipePoint.ArriveRecordList.FirstOrDefault(x => x.UniqueID == checkResult.ArriveRecordUniqueID);

                                            checkItemExcelItemList.Add(new CheckItemExcelItem()
                                            {
                                                
                                                IsAbnormal = checkResult.IsAbnormal ? Resources.Resource.Abnormal : (checkResult.IsAlert ? Resources.Resource.Warning : ""),
                                                 Route=route.Display,
                                                 PipePoint=pipePoint.Display,
                                                CheckItem = checkItem.Display,
                                                CheckDate = checkResult.CheckDate,
                                                CheckTime = checkResult.CheckTime,
                                                Result = checkResult.Result,
                                                LowerLimit = checkItem.LowerLimit.HasValue ? checkItem.LowerLimit.Value.ToString() : "",
                                                LowerAlertLimit = checkItem.LowerAlertLimit.HasValue ? checkItem.LowerAlertLimit.Value.ToString() : "",
                                                UpperAlertLimit = checkItem.UpperAlertLimit.HasValue ? checkItem.UpperAlertLimit.Value.ToString() : "",
                                                UpperLimit = checkItem.UpperLimit.HasValue ? checkItem.UpperLimit.Value.ToString() : "",
                                                Unit = checkItem.Unit,
                                                User = arriveRecord != null ? arriveRecord.User : "",
                                                AbnormalReasons = checkResult.AbnormalReasons
                                            });
                                        }
                                    }
                                    else
                                    {
                                        checkItemExcelItemList.Add(new CheckItemExcelItem()
                                        {
                                            IsAbnormal = "",
                                             PipePoint=pipePoint.Display,
                                              Route=route.Display,
                                            CheckItem = checkItem.Display,
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
                    }

                    helper.CreateSheet(Resources.Resource.PatrolStatus, new Dictionary<string, Utility.ExcelHelper.ExcelDisplayItem>() 
                    {
                        { "IsJobAbnormal", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.Abnormal, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "Job", new Utility.ExcelHelper.ExcelDisplayItem() { Name =  Resources.Resource.Job, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "CheckDate", new Utility.ExcelHelper.ExcelDisplayItem() { Name =  Resources.Resource.CheckDate, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "User", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.CheckUser, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "JobCompleteRate", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.CompleteRate), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "JobTimeSpan", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.TimeSpan), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "IsRouteAbnormal", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.Abnormal, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "Route", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.Route, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "RouteCompleteRate", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Route, Resources.Resource.CompleteRate), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "RouteTimeSpan", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Route, Resources.Resource.TimeSpan), CellType = NPOI.SS.UserModel.CellType.String }}
                    }, jobExcelItemList);

                    helper.CreateSheet(Resources.Resource.ArriveRecord, new Dictionary<string, Utility.ExcelHelper.ExcelDisplayItem>() 
                    {
                        { "IsAbnormal", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.Abnormal, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "Route", new Utility.ExcelHelper.ExcelDisplayItem() { Name =  Resources.Resource.Route, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "PipePoint", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.PipePoint, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "CompleteRate", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.CompleteRate, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "TimeSpan", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.TimeSpan, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "ArriveDate", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.ArriveDate, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "ArriveTime", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.ArriveTime, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "User", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.ArriveUser, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "MinTimeSpan", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.MinTimeSpan, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "IsTimeSpanAbnormal", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.TimeSpanAbnormal, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "TimeSpanAbnormalReason", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.TimeSpanAbnormalReason, CellType = NPOI.SS.UserModel.CellType.String }}
                    }, routeExcelItemList);

                    helper.CreateSheet(Resources.Resource.CheckResult, new Dictionary<string, Utility.ExcelHelper.ExcelDisplayItem>() 
                    {
                        { "IsAbnormal", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.Abnormal, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "Route", new Utility.ExcelHelper.ExcelDisplayItem() { Name =  Resources.Resource.Route, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "PipePoint", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.PipePoint, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "CheckItem", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.CheckItem, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "CheckDate", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.CheckDate, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "CheckTime", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.CheckTime, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "User", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.CheckUser, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "Result", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.CheckResult, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "LowerLimit", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.LowerLimit, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "LowerAlertLimit", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.LowerAlertLimit, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "UpperAlertLimit", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.UpperAlertLimit, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "UpperLimit", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.UpperLimit, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "Unit", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.Unit, CellType = NPOI.SS.UserModel.CellType.String }},
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
