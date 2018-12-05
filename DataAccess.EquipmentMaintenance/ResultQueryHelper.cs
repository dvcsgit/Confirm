using DbEntity.MSSQL.EquipmentMaintenance;
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

namespace DataAccess.EquipmentMaintenance
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

                using (EDbEntities db = new EDbEntities())
                {
                
                    var query = db.ArriveRecord.AsNoTracking().Where(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID) && string.Compare(x.ArriveDate, Parameters.BeginDate) >= 0 && string.Compare(x.ArriveDate, Parameters.EndDate) <= 0).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.RouteUniqueID))
                    {
                        query = query.Where(x => x.RouteUniqueID == Parameters.RouteUniqueID);
                    }

                    if (!string.IsNullOrEmpty(Parameters.JobUniqueID))
                    {
                        query = query.Where(x => x.JobUniqueID == Parameters.JobUniqueID);
                    }

                    var temp = query.Select(x => new
                    {
                        x.UniqueID,
                        x.JobUniqueID,
                        x.OrganizationDescription,
                        x.JobDescription,
                        x.RouteID,
                        x.RouteName,
                        x.ArriveDate,
                        x.ArriveTime,
                       x. ControlPointID,
                        x.ControlPointDescription,
                        x.TimeSpanAbnormalReasonDescription,
                        x.TimeSpanAbnormalReasonRemark,
                       x. MinTimeSpan,
                       x. UnRFIDReasonDescription,
                        x.UserID,
                        x.UnRFIDReasonRemark,
                        x.UserName
                    }).ToList();

                    var jobList = temp.Select(x => new
                    {
                        x.JobUniqueID,
                        x.OrganizationDescription,
                        x.JobDescription,
                        x.RouteID,
                        x.RouteName,
                        x.ArriveDate
                    }).Distinct().ToList();

                    var arriveRecordUniqueIDList=temp.Select(x=>x.UniqueID).ToList();

                    var allCheckResultList = db.CheckResult.Where(x => arriveRecordUniqueIDList.Contains(x.ArriveRecordUniqueID)).Select(x => new {
                    x.UniqueID,
                    x.EquipmentID,
                    x.EquipmentName,
                    x.PartDescription,
                    x.CheckDate,
                    x.ArriveRecordUniqueID,
                    x.CheckTime,
                    x.CheckItemID,
                    x.CheckItemDescription,
                    x.IsAbnormal,
                    x.IsAlert,
                    x.Result,
                    x.LowerLimit,
                    x.LowerAlertLimit,
                    x.UpperAlertLimit,
                    x.UpperLimit,
                    x.Unit
                    }).ToList();  // 一次性加載完所有的checkresult的資料

                    var checkResultUniqueIDList=allCheckResultList.Select(x=>x.UniqueID).ToList();
                    var checkResultPhotoList = db.CheckResultPhoto.Where(x => checkResultUniqueIDList.Contains(x.CheckResultUniqueID)).ToList();
                    var checkResultAbnormalReasonList = db.CheckResultAbnormalReason.Where(x => checkResultUniqueIDList.Contains(x.CheckResultUniqueID)).ToList();
                    var checkResultHandlingMethodList = db.CheckResultHandlingMethod.Where(x => checkResultUniqueIDList.Contains(x.CheckResultUniqueID)).ToList();
                    var arriveRecordPhotoList = db.ArriveRecordPhoto.Where(x => arriveRecordUniqueIDList.Contains(x.ArriveRecordUniqueID)).ToList();

                    foreach (var job in jobList)
                    {
                        var jobModel = new JobModel()
                        {
                            JobUniqueID = job.JobUniqueID,
                            OrganizationDescription = job.OrganizationDescription,
                            RouteID = job.RouteID,
                            RouteName = job.RouteName,
                            JobDescription = job.JobDescription,
                            CheckDate = job.ArriveDate
                        };

                        var arriveRecordList = temp.Where(x => x.JobUniqueID == job.JobUniqueID && x.ArriveDate == job.ArriveDate).OrderBy(x => x.ArriveTime).ToList();

                        var arrivceRecordUniqueIDList=arriveRecordList.Select(x=>x.UniqueID).ToList();
                        var checkReusltList = allCheckResultList.Where(x => arrivceRecordUniqueIDList.Contains(x.ArriveRecordUniqueID)).ToList();
                   
                        foreach (var arriveRecord in arriveRecordList)
                        {
                            var arriveRecordModel = new ArriveRecordModel()
                            {
                                JobUniqueID = job.JobUniqueID,
                                CheckDate = job.ArriveDate,
                                UniqueID = arriveRecord.UniqueID,
                                ControlPointID = arriveRecord.ControlPointID,
                                ControlPointDescription = arriveRecord.ControlPointDescription,
                                Date = arriveRecord.ArriveDate,
                                Time = arriveRecord.ArriveTime,
                                TimeSpanAbnormalReasonDescription = arriveRecord.TimeSpanAbnormalReasonDescription,
                                TimeSpanAbnormalReasonRemark = arriveRecord.TimeSpanAbnormalReasonRemark,
                                MinTimeSpan = arriveRecord.MinTimeSpan,
                                //TotalTimeSpan = arriveRecord.TotalTimeSpan,
                                UnRFIDReasonDescription = arriveRecord.UnRFIDReasonDescription,
                                UnRFIDReasonRemark = arriveRecord.UnRFIDReasonRemark,
                                UserID = arriveRecord.UserID,
                                UserName = arriveRecord.UserName,
                                PhotoList = arriveRecordPhotoList.Where(p => p.ArriveRecordUniqueID == arriveRecord.UniqueID).Select(p => p.ArriveRecordUniqueID + "_" + p.Seq + "." + p.Extension).OrderBy(p => p).ToList()
                            };

                            var checkResultList = checkReusltList.Where(x => x.ArriveRecordUniqueID == arriveRecord.UniqueID).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ThenBy(x => x.CheckItemID).ToList();

                            foreach (var checkResult in checkResultList)
                            {
                                arriveRecordModel.CheckResultList.Add(new CheckResultModel
                                {
                                    UniqueID = checkResult.UniqueID,
                                    EquipmentID = checkResult.EquipmentID,
                                    EquipmentName = checkResult.EquipmentName,
                                    PartDescription = checkResult.PartDescription,
                                    Date = checkResult.CheckDate,
                                    Time = checkResult.CheckTime,
                                    CheckItemID = checkResult.CheckItemID,
                                    CheckItemDescription = checkResult.CheckItemDescription,
                                    IsAbnormal = checkResult.IsAbnormal,
                                    IsAlert = checkResult.IsAlert,
                                    Result = checkResult.Result,
                                    LowerLimit = checkResult.LowerLimit,
                                    LowerAlertLimit = checkResult.LowerAlertLimit,
                                    UpperAlertLimit = checkResult.UpperAlertLimit,
                                    UpperLimit = checkResult.UpperLimit,
                                    Unit = checkResult.Unit,
                                    PhotoList = checkResultPhotoList.Where(p => p.CheckResultUniqueID == checkResult.UniqueID).Select(p => p.CheckResultUniqueID + "_" + p.Seq + "." + p.Extension).ToList(),
                                    AbnormalReasonList = checkResultAbnormalReasonList.Where(a => a.CheckResultUniqueID == checkResult.UniqueID).OrderBy(a => a.AbnormalReasonID).Select(a => new AbnormalReasonModel
                                    {
                                        Description = a.AbnormalReasonDescription,
                                        Remark = a.AbnormalReasonRemark,
                                        HandlingMethodList = checkResultHandlingMethodList.Where(h => h.CheckResultUniqueID == checkResult.UniqueID && h.AbnormalReasonUniqueID == a.AbnormalReasonUniqueID).OrderBy(h => h.HandlingMethodID).Select(h => new HandlingMethodModel
                                        {
                                            Description = h.HandlingMethodDescription,
                                            Remark = h.HandlingMethodRemark
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
