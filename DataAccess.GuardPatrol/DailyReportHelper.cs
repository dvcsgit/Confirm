using DbEntity.MSSQL;
using DbEntity.MSSQL.GuardPatrol;
using Models.Authenticated;
using Models.GuardPatrol.DailyReport;
using Models.Shared;
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
    public class DailyReportHelper
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                if (DateTime.Compare(Parameters.BeginDate, Parameters.BeginDate) > 0)
                {
                    result.ReturnFailedMessage("查詢條件巡邏日期起日不可大於迄日");
                }
                else
                {
                    if (Parameters.IsOnlyAbnormal)
                    {
                        Parameters.IsOnlyChecked = true;
                    }

                    using (GDbEntities db = new GDbEntities())
                    {
                        var jobUniqueIDList = new List<string>();

                        foreach (var p in Parameters.JobList)
                        {
                            if (!string.IsNullOrEmpty(p.JobUniqueID))
                            {
                                jobUniqueIDList.Add(p.JobUniqueID);
                            }
                            else
                            {
                                var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(p.OrganizationUniqueID, true);

                                var organizationList = Account.QueryableOrganizationUniqueIDList.Intersect(downStreamOrganizationList);

                                jobUniqueIDList.AddRange(db.Job.Where(x => organizationList.Contains(x.OrganizationUniqueID)).Select(x => x.UniqueID).ToList());
                            }
                        }

                        jobUniqueIDList = jobUniqueIDList.Distinct().ToList();

                        var jobRouteList = (from x in db.JobRoute
                                            join j in db.Job
                                            on x.JobUniqueID equals j.UniqueID
                                            join r in db.Route
                                            on x.RouteUniqueID equals r.UniqueID
                                            where jobUniqueIDList.Contains(j.UniqueID)
                                            select new
                                            {
                                                j.OrganizationUniqueID,
                                                JobRouteUniqueID = x.UniqueID,
                                                x.JobUniqueID,
                                                x.RouteUniqueID,
                                                RouteID = r.ID,
                                                RouteName = r.Name,
                                                JobDescription = j.Description,
                                                j.BeginDate,
                                                j.EndDate,
                                                j.BeginTime,
                                                j.EndTime,
                                                j.CycleCount,
                                                j.CycleMode
                                            }).ToList();

                        var jobRouteResultList = new List<JobRouteModel>();

                        foreach (var jobRoute in jobRouteList)
                        {
                            var date = Parameters.BeginDate;

                            while (DateTime.Compare(date, Parameters.EndDate) <= 0)
                            {
                                if (JobCycleHelper.IsInCycle(date, jobRoute.BeginDate, jobRoute.EndDate, jobRoute.CycleCount, jobRoute.CycleMode))
                                {
                                    DateTime beginDate, endDate;

                                    JobCycleHelper.GetDateSpan(date, jobRoute.BeginDate, jobRoute.EndDate, jobRoute.CycleCount, jobRoute.CycleMode, out beginDate, out endDate);

                                    var beginDateString = DateTimeHelper.DateTime2DateString(beginDate);
                                    var endDateString = DateTimeHelper.DateTime2DateString(endDate);

                                    if (!jobRouteResultList.Any(x => x.UniqueID == jobRoute.JobRouteUniqueID && x.BeginDate == beginDateString && x.EndDate == endDateString))
                                    {
                                        var allArriveRecordList = db.ArriveRecord.Where(x => x.JobUniqueID == jobRoute.JobUniqueID && x.RouteUniqueID == jobRoute.RouteUniqueID && string.Compare(x.ArriveDate, beginDateString) >= 0 && string.Compare(x.ArriveDate, endDateString) <= 0).ToList();

                                        if (Parameters.IsOnlyChecked && allArriveRecordList.Count == 0)
                                        { }
                                        else
                                        {
                                            var allCheckResultList = db.CheckResult.Where(x => x.JobUniqueID == jobRoute.JobUniqueID && x.RouteUniqueID == jobRoute.RouteUniqueID && string.Compare(x.CheckDate, beginDateString) >= 0 && string.Compare(x.CheckDate, endDateString) <= 0).ToList();

                                            if (Parameters.IsOnlyAbnormal && !allCheckResultList.Any(x => x.IsAbnormal || x.IsAlert))
                                            { }
                                            else
                                            {
                                                var item = new JobRouteModel()
                                                {
                                                    UniqueID = jobRoute.JobRouteUniqueID,
                                                    OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(jobRoute.OrganizationUniqueID),
                                                    BeginDate = beginDateString,
                                                    EndDate = endDateString,
                                                    RouteID = jobRoute.RouteID,
                                                    RouteName = jobRoute.RouteName,
                                                    JobDescription = jobRoute.JobDescription,
                                                    BeginTime = jobRoute.BeginTime,
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
                                                                            y.Seq
                                                                        }).OrderBy(x => x.Seq).ToList();

                                                foreach (var controlPoint in controlPointList)
                                                {
                                                    var controlPointModel = new ControlPointModel()
                                                    {
                                                        ID = controlPoint.ID,
                                                        Seq = controlPoint.Seq,
                                                        Description = controlPoint.Description,
                                                        ArriveRecordList = allArriveRecordList.Where(x => x.ControlPointUniqueID == controlPoint.UniqueID).OrderBy(x => x.ArriveDate).ThenBy(x => x.ArriveTime).Select(x => new ArriveRecordModel
                                                        {
                                                            ArriveDate = x.ArriveDate,
                                                            ArriveTime = x.ArriveTime,
                                                            UserID = x.UserID,
                                                            UserName = x.UserName,
                                                            UnRFIDReasonDescription = x.UnRFIDReasonDescription,
                                                            UnRFIDReasonRemark = x.UnRFIDReasonRemark,
                                                            TimeSpanAbnormalReasonDescription = x.TimeSpanAbnormalReasonDescription,
                                                            TimeSpanAbnormalReasonRemark = x.TimeSpanAbnormalReasonRemark
                                                        }).ToList()
                                                    };

                                                    var controlPointCheckItemList = (from x in db.JobControlPointCheckItem
                                                                                     join y in db.RouteControlPointCheckItem
                                                                                     on new { x.RouteUniqueID, x.ControlPointUniqueID, x.CheckItemUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.CheckItemUniqueID }
                                                                                     where x.JobUniqueID == jobRoute.JobUniqueID && x.RouteUniqueID == jobRoute.RouteUniqueID && x.ControlPointUniqueID == controlPoint.UniqueID
                                                                                     select new
                                                                                     {
                                                                                         UniqueID = x.CheckItemUniqueID,
                                                                                         y.Seq
                                                                                     }).OrderBy(x => x.Seq).ToList();

                                                    foreach (var checkItem in controlPointCheckItemList)
                                                    {
                                                        var checkItemModel = new CheckItemModel()
                                                        {

                                                        };

                                                        var checkResultList = allCheckResultList.Where(x => x.ControlPointUniqueID == controlPoint.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

                                                        foreach (var checkResult in checkResultList)
                                                        {
                                                            checkItemModel.CheckResultList.Add(new CheckResultModel()
                                                            {
                                                                AbnormalReasonList = db.CheckResultAbnormalReason.Where(a => a.CheckResultUniqueID == checkResult.UniqueID).OrderBy(a => a.AbnormalReasonID).Select(a => new AbnormalReasonModel
                                                                {
                                                                    Description = a.AbnormalReasonDescription,
                                                                    Remark = a.AbnormalReasonRemark
                                                                }).ToList()
                                                            });
                                                        }

                                                        controlPointModel.CheckItemList.Add(checkItemModel);
                                                    }

                                                    item.ControlPointList.Add(controlPointModel);
                                                }

                                                jobRouteResultList.Add(item);
                                            }
                                        }
                                    }
                                }

                                date = date.AddDays(1);
                            }
                        }

                        var tmp1 = jobRouteResultList.Where(x => x.FirstArriveTime.HasValue).OrderBy(x => x.FirstArriveTime.Value).ToList();
                        var tmp2 = jobRouteResultList.Where(x => !x.FirstArriveTime.HasValue && !string.IsNullOrEmpty(x.BeginTime)).OrderBy(x => x.BeginDate).ThenBy(x => x.BeginTime).ThenBy(x => x.Description).ToList();
                        var tmp3 = jobRouteResultList.Where(x => !x.FirstArriveTime.HasValue && string.IsNullOrEmpty(x.BeginTime)).OrderBy(x => x.BeginDate).ThenBy(x => x.Description).ToList();

                        var itemList = new List<GridItem>();

                        foreach (var jobRouteResult in tmp1)
                        {
                            itemList.Add(new GridItem()
                            {
                                CheckDate = DateTimeHelper.DateString2DateStringWithSeparator(jobRouteResult.BeginDate),
                                CheckUser = !string.IsNullOrEmpty(jobRouteResult.CheckUsers) ? jobRouteResult.CheckUsers : "未檢查",
                                ID = jobRouteResult.RouteID,
                                OrganizationDescription = jobRouteResult.OrganizationDescription,
                                Name = jobRouteResult.Description,
                                CheckTime = jobRouteResult.FirstArriveTime.HasValue ? DateTimeHelper.DateTime2TimeStringWithSeperator(jobRouteResult.FirstArriveTime) : "未檢查",
                                Remark = ""
                            });

                            var tmp4 = jobRouteResult.ControlPointList.Where(x => x.FirstArriveTime.HasValue).OrderBy(x => x.FirstArriveTime.Value).ToList();
                            var tmp5 = jobRouteResult.ControlPointList.Where(x => !x.FirstArriveTime.HasValue).OrderBy(x => x.Seq).ToList();

                            foreach (var controlPoint in tmp4)
                            {
                                itemList.Add(new GridItem()
                                {
                                    CheckDate = "",
                                    CheckUser = "",
                                    ID = controlPoint.ID,
                                    OrganizationDescription = "",
                                    Name = controlPoint.Description,
                                    CheckTime = DateTimeHelper.DateTime2TimeStringWithSeperator(controlPoint.FirstArriveTime),
                                    Remark = controlPoint.Remark
                                });
                            }

                            foreach (var controlPoint in tmp5)
                            {
                                itemList.Add(new GridItem()
                                {
                                    CheckDate = "",
                                    CheckUser = "",
                                    ID = controlPoint.ID,
                                    OrganizationDescription = "",
                                    Name = controlPoint.Description,
                                    CheckTime = DateTimeHelper.DateTime2TimeStringWithSeperator(controlPoint.FirstArriveTime),
                                    Remark = controlPoint.Remark
                                });
                            }
                        }

                        foreach (var jobRouteResult in tmp2)
                        {
                            itemList.Add(new GridItem()
                            {
                                CheckDate = DateTimeHelper.DateString2DateStringWithSeparator(jobRouteResult.BeginDate),
                                CheckUser = !string.IsNullOrEmpty(jobRouteResult.CheckUsers) ? jobRouteResult.CheckUsers : "未檢查",
                                ID = jobRouteResult.RouteID,
                                OrganizationDescription = jobRouteResult.OrganizationDescription,
                                Name = jobRouteResult.Description,
                                CheckTime = jobRouteResult.FirstArriveTime.HasValue ? DateTimeHelper.DateTime2TimeStringWithSeperator(jobRouteResult.FirstArriveTime) : "未檢查",
                                Remark = ""
                            });

                            var tmp4 = jobRouteResult.ControlPointList.Where(x => x.FirstArriveTime.HasValue).OrderBy(x => x.FirstArriveTime.Value).ToList();
                            var tmp5 = jobRouteResult.ControlPointList.Where(x => !x.FirstArriveTime.HasValue).OrderBy(x => x.Seq).ToList();

                            foreach (var controlPoint in tmp4)
                            {
                                itemList.Add(new GridItem()
                                {
                                    CheckDate = "",
                                    CheckUser = "",
                                    ID = controlPoint.ID,
                                    OrganizationDescription = "",
                                    Name = controlPoint.Description,
                                    CheckTime = DateTimeHelper.DateTime2TimeStringWithSeperator(controlPoint.FirstArriveTime),
                                    Remark = controlPoint.Remark
                                });
                            }

                            foreach (var controlPoint in tmp5)
                            {
                                itemList.Add(new GridItem()
                                {
                                    CheckDate = "",
                                    CheckUser = "",
                                    ID = controlPoint.ID,
                                    OrganizationDescription = "",
                                    Name = controlPoint.Description,
                                    CheckTime = DateTimeHelper.DateTime2TimeStringWithSeperator(controlPoint.FirstArriveTime),
                                    Remark = controlPoint.Remark
                                });
                            }
                        }

                        foreach (var jobRouteResult in tmp3)
                        {
                            itemList.Add(new GridItem()
                            {
                                CheckDate = DateTimeHelper.DateString2DateStringWithSeparator(jobRouteResult.BeginDate),
                                CheckUser = !string.IsNullOrEmpty(jobRouteResult.CheckUsers) ? jobRouteResult.CheckUsers : "未檢查",
                                ID = jobRouteResult.RouteID,
                                OrganizationDescription = jobRouteResult.OrganizationDescription,
                                Name = jobRouteResult.Description,
                                CheckTime = jobRouteResult.FirstArriveTime.HasValue ? DateTimeHelper.DateTime2TimeStringWithSeperator(jobRouteResult.FirstArriveTime) : "未檢查",
                                Remark = ""
                            });

                            var tmp4 = jobRouteResult.ControlPointList.Where(x => x.FirstArriveTime.HasValue).OrderBy(x => x.FirstArriveTime.Value).ToList();
                            var tmp5 = jobRouteResult.ControlPointList.Where(x => !x.FirstArriveTime.HasValue).OrderBy(x => x.Seq).ToList();

                            foreach (var controlPoint in tmp4)
                            {
                                itemList.Add(new GridItem()
                                {
                                    CheckDate = "",
                                    CheckUser = "",
                                    ID = controlPoint.ID,
                                    OrganizationDescription = "",
                                    Name = controlPoint.Description,
                                    CheckTime = DateTimeHelper.DateTime2TimeStringWithSeperator(controlPoint.FirstArriveTime),
                                    Remark = controlPoint.Remark
                                });
                            }

                            foreach (var controlPoint in tmp5)
                            {
                                itemList.Add(new GridItem()
                                {
                                    CheckDate = "",
                                    CheckUser = "",
                                    ID = controlPoint.ID,
                                    OrganizationDescription = "",
                                    Name = controlPoint.Description,
                                    CheckTime = DateTimeHelper.DateTime2TimeStringWithSeperator(controlPoint.FirstArriveTime),
                                    Remark = controlPoint.Remark
                                });
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

                using (GDbEntities gdb = new GDbEntities())
                {
                    if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                    {
                        var jobList = gdb.Job.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.Description).ToList();

                        foreach (var job in jobList)
                        {
                            var treeItem = new TreeItem() { Title = job.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Job.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = job.Description;
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
                                (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && gdb.Job.Any(x => x.OrganizationUniqueID == organization.UniqueID)))
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
