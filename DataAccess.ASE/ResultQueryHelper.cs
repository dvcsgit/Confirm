using DbEntity.ASE;
using Models.Authenticated;
using Models.EquipmentMaintenance.ResultQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace DataAccess.ASE
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = db.ARRIVERECORD.Where(x => downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID) && Account.QueryableOrganizationUniqueIDList.Contains(x.ORGANIZATIONUNIQUEID) && string.Compare(x.ARRIVEDATE, Parameters.BeginDate) >= 0 && string.Compare(x.ARRIVEDATE, Parameters.EndDate) <= 0).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.RouteUniqueID))
                    {
                        query = query.Where(x => x.ROUTEUNIQUEID == Parameters.RouteUniqueID);
                    }

                    if (!string.IsNullOrEmpty(Parameters.JobUniqueID))
                    {
                        query = query.Where(x => x.UNIQUEID == Parameters.JobUniqueID);
                    }

                    var temp = query.ToList();

                    var jobList = temp.Select(x => new
                    {
                        x.JOBUNIQUEID,
                        x.ORGANIZATIONDESCRIPTION,
                        x.JOBDESCRIPTION,
                        x.ROUTEID,
                        x.ROUTENAME,
                        x.ARRIVEDATE
                    }).Distinct().ToList();

                    foreach (var job in jobList)
                    {
                        var jobModel = new JobModel()
                        {
                            JobUniqueID = job.JOBUNIQUEID,
                            OrganizationDescription = job.ORGANIZATIONDESCRIPTION,
                            RouteID = job.ROUTEID,
                            RouteName = job.ROUTENAME,
                            JobDescription = job.JOBDESCRIPTION,
                            CheckDate = job.ARRIVEDATE
                        };

                        var arriveRecordList = temp.Where(x => x.JOBUNIQUEID == job.JOBUNIQUEID && x.ARRIVEDATE == job.ARRIVEDATE).OrderBy(x => x.ARRIVETIME).ToList();

                        foreach (var arriveRecord in arriveRecordList)
                        {
                            var arriveRecordModel = new ArriveRecordModel()
                            {
                                JobUniqueID = job.JOBUNIQUEID,
                                CheckDate = job.ARRIVEDATE,
                                UniqueID = arriveRecord.UNIQUEID,
                                ControlPointID = arriveRecord.CONTROLPOINTID,
                                ControlPointDescription = arriveRecord.CONTROLPOINTDESCRIPTION,
                                Date = arriveRecord.ARRIVEDATE,
                                Time = arriveRecord.ARRIVETIME,
                                TimeSpanAbnormalReasonDescription = arriveRecord.TIMESPANABNORMALREASONDESC,
                                TimeSpanAbnormalReasonRemark = arriveRecord.TIMESPANABNORMALREASONREMARK,
                                MinTimeSpan = arriveRecord.MINTIMESPAN.HasValue ? double.Parse(arriveRecord.MINTIMESPAN.Value.ToString()) : default(double?),
                                //TotalTimeSpan = arriveRecord.TOTALTIMESPAN.HasValue ? double.Parse(arriveRecord.TOTALTIMESPAN.Value.ToString()) : default(double?),
                                UnRFIDReasonDescription = arriveRecord.UNRFIDREASONDESCRIPTION,
                                UnRFIDReasonRemark = arriveRecord.UNRFIDREASONREMARK,
                                UserID = arriveRecord.USERID,
                                UserName = arriveRecord.USERNAME,
                                PhotoList = db.ARRIVERECORDPHOTO.Where(p => p.ARRIVERECORDUNIQUEID == arriveRecord.UNIQUEID).OrderBy(x=>x.SEQ).ToList().Select(p => p.ARRIVERECORDUNIQUEID + "_" + p.SEQ + "." + p.EXTENSION).OrderBy(p => p).ToList()
                            };

                            var checkResultList = db.Database.SqlQuery<CHECKRESULT>(string.Format("SELECT * FROM EIPC.CHECKRESULT WHERE ARRIVERECORDUNIQUEID = '{0}' ORDER BY CHECKDATE, CHECKTIME", arriveRecord.UNIQUEID)).ToList();

                            //var checkResultList = db.CHECKRESULT.Where(x => x.ARRIVERECORDUNIQUEID == arriveRecord.UNIQUEID).OrderBy(x => x.CHECKDATE).ThenBy(x => x.CHECKTIME).ToList();

                            foreach (var checkResult in checkResultList)
                            {
                                arriveRecordModel.CheckResultList.Add(new CheckResultModel
                                {
                                    UniqueID = checkResult.UNIQUEID,
                                    EquipmentID = checkResult.EQUIPMENTID,
                                    EquipmentName = checkResult.EQUIPMENTNAME,
                                    PartDescription = checkResult.PARTDESCRIPTION,
                                    Date = checkResult.CHECKDATE,
                                    Time = checkResult.CHECKTIME,
                                    CheckItemID = checkResult.CHECKITEMID,
                                    CheckItemDescription = checkResult.CHECKITEMDESCRIPTION,
                                    IsAbnormal = checkResult.ISABNORMAL=="Y",
                                    IsAlert = checkResult.ISALERT=="Y",
                                    Result = checkResult.RESULT,
                                    LowerLimit = checkResult.LOWERLIMIT.HasValue?double.Parse(checkResult.LOWERLIMIT.Value.ToString()):default(double?),
                                    LowerAlertLimit = checkResult.LOWERALERTLIMIT.HasValue ? double.Parse(checkResult.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                                    UpperAlertLimit = checkResult.UPPERALERTLIMIT.HasValue ? double.Parse(checkResult.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                                    UpperLimit = checkResult.UPPERLIMIT.HasValue ? double.Parse(checkResult.UPPERLIMIT.Value.ToString()) : default(double?),
                                    Unit = checkResult.UNIT,
                                    PhotoList = db.CHECKRESULTPHOTO.Where(p => p.CHECKRESULTUNIQUEID == checkResult.UNIQUEID).OrderBy(x=>x.SEQ).ToList().Select(p => p.CHECKRESULTUNIQUEID + "_" + p.SEQ + "." + p.EXTENSION).ToList(),
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

                            jobModel.ArriveRecordList.Add(arriveRecordModel);
                        }

                        itemList.Add(jobModel);
                    }
                }

                result.ReturnData(itemList.OrderBy(x => x.CheckDate).ThenBy(x => x.OrganizationDescription).ThenBy(x => x.Description).ToList());
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
                    var excelItemList = new List<CheckResultExcelItem>();

                    foreach (var job in Model)
                    {
                        foreach (var arriveRecord in job.ArriveRecordList)
                        {
                            foreach (var checkResult in arriveRecord.CheckResultList)
                            {
                                excelItemList.Add(new CheckResultExcelItem()
                                {
                                    Organization = job.OrganizationDescription,
                                    Job = job.Description,
                                    ControlPoint = arriveRecord.ControlPoint,
                                    ArriveTime = arriveRecord.ArriveTime,
                                    User = arriveRecord.User,
                                    UnRFIDReason = arriveRecord.UnRFIDReason,
                                    Equipment = checkResult.Equipment,
                                    CheckItem = checkResult.CheckItem,
                                    CheckTime = checkResult.CheckTime,
                                    Result = checkResult.Result,
                                    LowerLimit = checkResult.LowerLimit.HasValue ? checkResult.LowerLimit.Value.ToString() : string.Empty,
                                    LowerAlertLimit = checkResult.LowerAlertLimit.HasValue ? checkResult.LowerAlertLimit.Value.ToString() : string.Empty,
                                    UpperAlertLimit = checkResult.UpperAlertLimit.HasValue ? checkResult.UpperAlertLimit.Value.ToString() : string.Empty,
                                    UpperLimit = checkResult.UpperLimit.HasValue ? checkResult.UpperLimit.Value.ToString() : string.Empty,
                                    Unit = checkResult.Unit,
                                    AbnormalReason = checkResult.AbnormalReasons
                                });
                            }
                        }
                    }

                    helper.CreateSheet(Resources.Resource.CheckResult, new Dictionary<string, Utility.ExcelHelper.ExcelDisplayItem>() 
                    {
                        { "Organization", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.Organization, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "Job", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1}", Resources.Resource.Route, Resources.Resource.Job), CellType = NPOI.SS.UserModel.CellType.String }},
                        { "ControlPoint", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.ControlPoint, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "ArriveTime", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.ArriveTime, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "User", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.CheckUser, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "UnRFIDReason", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.UnRFIDReason, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "Equipment", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.Equipment, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "CheckItem", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.CheckItem, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "CheckTime", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.CheckTime, CellType = NPOI.SS.UserModel.CellType.String }},
                        { "Result", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.CheckResult    , CellType = NPOI.SS.UserModel.CellType.String }},
                        { "LowerLimit", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.LowerLimit    , CellType = NPOI.SS.UserModel.CellType.String }},
                        { "LowerAlertLimit", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.LowerAlertLimit    , CellType = NPOI.SS.UserModel.CellType.String }},
                        { "UpperAlertLimit", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.UpperAlertLimit    , CellType = NPOI.SS.UserModel.CellType.String }},
                        { "UpperLimit", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.UpperLimit    , CellType = NPOI.SS.UserModel.CellType.String }},
                        { "Unit", new Utility.ExcelHelper.ExcelDisplayItem() { Name = Resources.Resource.Unit    , CellType = NPOI.SS.UserModel.CellType.String }},
                        { "AbnormalReason", new Utility.ExcelHelper.ExcelDisplayItem() { Name = string.Format("{0} {1} {2}", Resources.Resource.AbnormalReason, Resources.Resource.And, Resources.Resource.HandlingMethod), CellType = NPOI.SS.UserModel.CellType.String }}
                    }, excelItemList);

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
