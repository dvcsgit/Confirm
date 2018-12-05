using System;
using System.Linq;
using System.Reflection;
using Utility;
using Utility.Models;
using DbEntity.ASE;
using Models.EquipmentMaintenance.DailyReport;
using Models.Authenticated;
using Models.Shared;
using System.Collections.Generic;
using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.Util;
using System.Text;
using NPOI.HSSF.UserModel;

namespace DataAccess.ASE
{
    public class DailyReportHelper
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var jobResultList = new List<string>();

                    var route = db.ROUTE.First(x => x.UNIQUEID == Parameters.RouteUniqueID);

                    var model = new ReportModel()
                    {
                        Date = Parameters.DateString,
                        OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(route.ORGANIZATIONUNIQUEID),
                        RouteName = route.NAME
                    };

                    var jobList = db.JOB.Where(x => x.ROUTEUNIQUEID == route.UNIQUEID).OrderBy(x => x.BEGINTIME).ThenBy(x => x.DESCRIPTION).ToList();

                    foreach (var job in jobList)
                    {
                        DateTime begin, end;

                        JobCycleHelper.GetDateSpan(DateTimeHelper.DateString2DateTime(Parameters.Date).Value, job.BEGINDATE.Value, job.ENDDATE, job.CYCLECOUNT.Value, job.CYCLEMODE, out begin, out end);

                        var beginDateString = DateTimeHelper.DateTime2DateString(begin);
                        var endDateString = DateTimeHelper.DateTime2DateString(end);

                        var jobResult = db.JOBRESULT.FirstOrDefault(x => x.JOBUNIQUEID == job.UNIQUEID && x.BEGINDATE == beginDateString && x.ENDDATE == endDateString);

                        if (jobResult == null)
                        {
                            JobResultDataAccessor.Refresh(Guid.NewGuid().ToString(), job.UNIQUEID, beginDateString, endDateString);

                            jobResult = db.JOBRESULT.FirstOrDefault(x => x.JOBUNIQUEID == job.UNIQUEID && x.BEGINDATE == beginDateString && x.ENDDATE == endDateString);
                        }

                        jobResultList.Add(jobResult.UNIQUEID);    

                        var jobModel = new JobModel()
                        {
                            UniqueID = job.UNIQUEID,
                            Description = job.DESCRIPTION,
                            Users = jobResult.CHECKUSERS
                        };

                        model.JobList.Add(jobModel);
                    }

                    int no = 1;

                    var checkResultList = (from a in db.ARRIVERECORD
                                           join r in db.CHECKRESULT
                                           on a.UNIQUEID equals r.ARRIVERECORDUNIQUEID
                                           where jobResultList.Contains(a.JOBRESULTUNIQUEID)
                                           select r).ToList();

                    var controlPointList = (from x in db.ROUTECONTROLPOINT
                                            join c in db.CONTROLPOINT
                                            on x.CONTROLPOINTUNIQUEID equals c.UNIQUEID
                                            where x.ROUTEUNIQUEID == route.UNIQUEID
                                            select new
                                            {
                                                c.UNIQUEID,
                                                c.DESCRIPTION,
                                                x.SEQ
                                            }).OrderBy(x => x.SEQ).ToList();

                    foreach (var controlPoint in controlPointList)
                    {
                        var controlPointCheckItemList = (from x in db.ROUTECONTROLPOINTCHECKITEM
                                                         join c in db.CONTROLPOINTCHECKITEM
                                                         on new { x.CONTROLPOINTUNIQUEID, x.CHECKITEMUNIQUEID } equals new { c.CONTROLPOINTUNIQUEID, c.CHECKITEMUNIQUEID }
                                                         join item in db.CHECKITEM
                                                         on x.CHECKITEMUNIQUEID equals item.UNIQUEID
                                                         where x.ROUTEUNIQUEID == route.UNIQUEID && x.CONTROLPOINTUNIQUEID == controlPoint.UNIQUEID
                                                         select new
                                                         {
                                                             UniqueID = c.CHECKITEMUNIQUEID,
                                                             CheckItemDescription = item.DESCRIPTION,
                                                             IsFeelItem = item.ISFEELITEM=="Y",
                                                             LowerLimit =c.ISINHERIT=="Y"?item.LOWERLIMIT: c.LOWERLIMIT,
                                                             LowerAlertLimit = c.ISINHERIT == "Y" ? item.LOWERALERTLIMIT : c.LOWERALERTLIMIT,
                                                             UpperAlertLimit = c.ISINHERIT == "Y" ? item.UPPERALERTLIMIT : c.UPPERALERTLIMIT,
                                                             UpperLimit = c.ISINHERIT == "Y" ? item.UPPERLIMIT : c.UPPERLIMIT,
                                                             Unit = c.ISINHERIT == "Y" ? item.UNIT : c.UNIT,
                                                             Remark = c.ISINHERIT == "Y" ? item.REMARK : c.REMARK,
                                                             Seq = x.SEQ
                                                         }).OrderBy(x => x.Seq).ToList();

                        foreach (var checkItem in controlPointCheckItemList)
                        {
                            var item = new CheckItemModel()
                            {
                                No = no,
                                ControlPointDescription = controlPoint.DESCRIPTION,
                                EquipmentName = string.Empty,
                                PartDescription = string.Empty,
                                CheckItemDescription = checkItem.CheckItemDescription,
                                IsFeelItem = checkItem.IsFeelItem,
                                LowerLimit = checkItem.LowerLimit.HasValue?double.Parse(checkItem.LowerLimit.Value.ToString()):default(double?),
                                LowerAlertLimit = checkItem.LowerAlertLimit.HasValue ? double.Parse(checkItem.LowerAlertLimit.Value.ToString()) : default(double?),
                                UpperAlertLimit = checkItem.UpperAlertLimit.HasValue ? double.Parse(checkItem.UpperAlertLimit.Value.ToString()) : default(double?),
                                UpperLimit = checkItem.UpperLimit.HasValue ? double.Parse(checkItem.UpperLimit.Value.ToString()) : default(double?),
                                Unit = checkItem.Unit,
                                CheckItemRemark = checkItem.Remark,
                                FeelOptionDescriptionList = (from x in db.CHECKITEMFEELOPTION
                                                             where x.CHECKITEMUNIQUEID == checkItem.UniqueID
                                                             orderby x.SEQ
                                                             select x).ToList().Select(x=> x.DESCRIPTION + (x.ISABNORMAL=="Y" ? "(" + Resources.Resource.Abnormal + ")" : "")).ToList()
                            };

                            foreach (var job in model.JobList)
                            {
                                var jobControlPointCheckItem = db.JOBCONTROLPOINTCHECKITEM.FirstOrDefault(x => x.JOBUNIQUEID == job.UniqueID && x.CONTROLPOINTUNIQUEID == controlPoint.UNIQUEID && x.CHECKITEMUNIQUEID == checkItem.UniqueID);

                                if (jobControlPointCheckItem != null)
                                {
                                    if (Parameters.IsShowLastData)
                                    {
                                        var checkResult = checkResultList.Where(x => x.JOBUNIQUEID == job.UniqueID && x.CONTROLPOINTUNIQUEID == controlPoint.UNIQUEID && string.IsNullOrEmpty(x.EQUIPMENTUNIQUEID) && x.CHECKITEMUNIQUEID == checkItem.UniqueID).OrderByDescending(x => x.CHECKDATE).ThenByDescending(x => x.CHECKTIME).Select(x => new CheckResultModel
                                        {
                                            Result = x.RESULT,
                                            AbnormalReasonList = db.CHECKRESULTABNORMALREASON.Where(a => a.CHECKRESULTUNIQUEID == x.UNIQUEID).OrderBy(a => a.ABNORMALREASONID).Select(a => new AbnormalReasonModel
                                            {
                                                Description = a.ABNORMALREASONDESCRIPTION,
                                                Remark = a.ABNORMALREASONREMARK,
                                                HandlingMethodList = db.CHECKRESULTHANDLINGMETHOD.Where(h => h.CHECKRESULTUNIQUEID == x.UNIQUEID && h.ABNORMALREASONUNIQUEID == a.ABNORMALREASONUNIQUEID).OrderBy(h => h.HANDLINGMETHODID).Select(h => new HandlingMethodModel
                                                {
                                                    Description = h.HANDLINGMETHODDESCRIPTION,
                                                    Remark = h.HANDLINGMETHODREMARK
                                                }).ToList()
                                            }).ToList()
                                        }).ToList().FirstOrDefault();
                                        if (checkResult != null)
                                        {
                                            item.ResultList.Add(job.UniqueID, new List<CheckResultModel>()
                                            {
                                                checkResult
                                            });
                                        }
                                        else
                                        {
                                            item.ResultList.Add(job.UniqueID, null);
                                        }
                                        
                                    }
                                    else
                                    {
                                        item.ResultList.Add(job.UniqueID, checkResultList.Where(x => x.JOBUNIQUEID == job.UniqueID && x.CONTROLPOINTUNIQUEID == controlPoint.UNIQUEID && string.IsNullOrEmpty(x.EQUIPMENTUNIQUEID) && x.CHECKITEMUNIQUEID == checkItem.UniqueID).OrderBy(x => x.CHECKDATE).ThenBy(x => x.CHECKTIME).Select(x => new CheckResultModel
                                        {
                                            Result = x.RESULT,
                                            AbnormalReasonList = db.CHECKRESULTABNORMALREASON.Where(a => a.CHECKRESULTUNIQUEID == x.UNIQUEID).OrderBy(a => a.ABNORMALREASONID).Select(a => new AbnormalReasonModel
                                            {
                                                Description = a.ABNORMALREASONDESCRIPTION,
                                                Remark = a.ABNORMALREASONREMARK,
                                                HandlingMethodList = db.CHECKRESULTHANDLINGMETHOD.Where(h => h.CHECKRESULTUNIQUEID == x.UNIQUEID && h.ABNORMALREASONUNIQUEID == a.ABNORMALREASONUNIQUEID).OrderBy(h => h.HANDLINGMETHODID).Select(h => new HandlingMethodModel
                                                {
                                                    Description = h.HANDLINGMETHODDESCRIPTION,
                                                    Remark = h.HANDLINGMETHODREMARK
                                                }).ToList()
                                            }).ToList()
                                        }).ToList());
                                    }
                                }
                                else
                                {
                                    item.ResultList.Add(job.UniqueID, null);
                                }
                            }

                            model.ItemList.Add(item);

                            no++;
                        }

                        var routeEquipmentList = db.ROUTEEQUIPMENT.Where(x => x.ROUTEUNIQUEID == route.UNIQUEID && x.CONTROLPOINTUNIQUEID == controlPoint.UNIQUEID).OrderBy(x => x.SEQ).ToList();

                        foreach (var routeEquipment in routeEquipmentList)
                        {
                            var equipment = db.EQUIPMENT.FirstOrDefault(x => x.UNIQUEID == routeEquipment.EQUIPMENTUNIQUEID);

                            var part = db.EQUIPMENTPART.FirstOrDefault(x => x.EQUIPMENTUNIQUEID == routeEquipment.EQUIPMENTUNIQUEID && x.UNIQUEID == routeEquipment.PARTUNIQUEID);

                            var equipmentCheckItemList = (from x in db.ROUTEEQUIPMENTCHECKITEM
                                                          join c in db.EQUIPMENTCHECKITEM
                                                          on new { x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID, x.CHECKITEMUNIQUEID } equals new { c.EQUIPMENTUNIQUEID, c.PARTUNIQUEID, c.CHECKITEMUNIQUEID }
                                                          join i in db.CHECKITEM
                                                          on x.CHECKITEMUNIQUEID equals i.UNIQUEID
                                                          where x.ROUTEUNIQUEID == route.UNIQUEID && x.CONTROLPOINTUNIQUEID == controlPoint.UNIQUEID && x.EQUIPMENTUNIQUEID == routeEquipment.EQUIPMENTUNIQUEID && x.PARTUNIQUEID == routeEquipment.PARTUNIQUEID
                                                          select new
                                                          {
                                                              UniqueID = c.CHECKITEMUNIQUEID,
                                                              CheckItemDescription = i.DESCRIPTION,
                                                              IsFeelItem = i.ISFEELITEM=="Y",
                                                              LowerLimit = c.ISINHERIT == "Y" ? i.LOWERLIMIT : c.LOWERLIMIT,
                                                              LowerAlertLimit = c.ISINHERIT == "Y" ? i.LOWERALERTLIMIT : c.LOWERALERTLIMIT,
                                                              UpperAlertLimit = c.ISINHERIT == "Y" ? i.UPPERALERTLIMIT : c.UPPERALERTLIMIT,
                                                              UpperLimit = c.ISINHERIT == "Y" ? i.UPPERLIMIT : c.UPPERLIMIT,
                                                              Unit = c.ISINHERIT == "Y" ? i.UNIT : c.UNIT,
                                                              Remark = c.ISINHERIT == "Y" ? i.REMARK : c.REMARK,
                                                              Seq = x.SEQ
                                                          }).OrderBy(x => x.Seq).ToList();

                            foreach (var checkItem in equipmentCheckItemList)
                            {
                                var item = new CheckItemModel()
                                {
                                    No = no,
                                    ControlPointDescription = controlPoint.DESCRIPTION,
                                    EquipmentName =equipment!=null? equipment.NAME:string.Empty,
                                    PartDescription = part!=null?part.DESCRIPTION:string.Empty,
                                    CheckItemDescription = checkItem.CheckItemDescription,
                                    IsFeelItem = checkItem.IsFeelItem,
                                    LowerLimit = checkItem.LowerLimit.HasValue?double.Parse(checkItem.LowerLimit.Value.ToString()):default(double?),
                                    LowerAlertLimit = checkItem.LowerAlertLimit.HasValue ? double.Parse(checkItem.LowerAlertLimit.Value.ToString()) : default(double?),
                                    UpperAlertLimit = checkItem.UpperAlertLimit.HasValue ? double.Parse(checkItem.UpperAlertLimit.Value.ToString()) : default(double?),
                                    UpperLimit = checkItem.UpperLimit.HasValue ? double.Parse(checkItem.UpperLimit.Value.ToString()) : default(double?),
                                    Unit = checkItem.Unit,
                                    CheckItemRemark = checkItem.Remark,
                                    FeelOptionDescriptionList = (from x in db.CHECKITEMFEELOPTION
                                                                 where x.CHECKITEMUNIQUEID == checkItem.UniqueID
                                                                 orderby x.SEQ
                                                                 select x).ToList().Select(x=> x.DESCRIPTION + (x.ISABNORMAL=="Y" ? "(" + Resources.Resource.Abnormal + ")" : "")).ToList()
                                };

                                foreach (var job in model.JobList)
                                {
                                    var jobEquipmentCheckItem = db.JOBEQUIPMENTCHECKITEM.FirstOrDefault(x => x.JOBUNIQUEID == job.UniqueID && x.CONTROLPOINTUNIQUEID == controlPoint.UNIQUEID && x.EQUIPMENTUNIQUEID == routeEquipment.EQUIPMENTUNIQUEID && x.PARTUNIQUEID == routeEquipment.PARTUNIQUEID && x.CHECKITEMUNIQUEID == checkItem.UniqueID);

                                    if (jobEquipmentCheckItem != null)
                                    {
                                        if (Parameters.IsShowLastData)
                                        {
                                            var checkResult = checkResultList.Where(x => x.JOBUNIQUEID == job.UniqueID && x.CONTROLPOINTUNIQUEID == controlPoint.UNIQUEID && x.EQUIPMENTUNIQUEID == routeEquipment.EQUIPMENTUNIQUEID && x.PARTUNIQUEID == routeEquipment.PARTUNIQUEID && x.CHECKITEMUNIQUEID == checkItem.UniqueID).OrderByDescending(x => x.CHECKDATE).ThenByDescending(x => x.CHECKTIME).Select(x => new CheckResultModel
                                            {
                                                Result = x.RESULT,
                                                AbnormalReasonList = db.CHECKRESULTABNORMALREASON.Where(a => a.CHECKRESULTUNIQUEID == x.UNIQUEID).OrderBy(a => a.ABNORMALREASONID).Select(a => new AbnormalReasonModel
                                                {
                                                    Description = a.ABNORMALREASONDESCRIPTION,
                                                    Remark = a.ABNORMALREASONREMARK,
                                                    HandlingMethodList = db.CHECKRESULTHANDLINGMETHOD.Where(h => h.CHECKRESULTUNIQUEID == x.UNIQUEID && h.ABNORMALREASONUNIQUEID == a.ABNORMALREASONUNIQUEID).OrderBy(h => h.HANDLINGMETHODID).Select(h => new HandlingMethodModel
                                                    {
                                                        Description = h.HANDLINGMETHODDESCRIPTION,
                                                        Remark = h.HANDLINGMETHODREMARK
                                                    }).ToList()
                                                }).ToList()
                                            }).ToList().FirstOrDefault();

                                            if (checkResult != null)
                                            {
                                                item.ResultList.Add(job.UniqueID, new List<CheckResultModel>()
                                                {
                                                    checkResult
                                                });
                                            }
                                            else
                                            {
                                                item.ResultList.Add(job.UniqueID, null);
                                            }
                                        }
                                        else {
                                            item.ResultList.Add(job.UniqueID, checkResultList.Where(x => x.JOBUNIQUEID == job.UniqueID && x.CONTROLPOINTUNIQUEID == controlPoint.UNIQUEID && x.EQUIPMENTUNIQUEID == routeEquipment.EQUIPMENTUNIQUEID && x.PARTUNIQUEID == routeEquipment.PARTUNIQUEID && x.CHECKITEMUNIQUEID == checkItem.UniqueID).OrderBy(x=>x.CHECKDATE).ThenBy(x=>x.CHECKTIME).Select(x => new CheckResultModel
                                            {
                                                Result = x.RESULT,
                                                AbnormalReasonList = db.CHECKRESULTABNORMALREASON.Where(a => a.CHECKRESULTUNIQUEID == x.UNIQUEID).OrderBy(a => a.ABNORMALREASONID).Select(a => new AbnormalReasonModel
                                                {
                                                    Description = a.ABNORMALREASONDESCRIPTION,
                                                    Remark = a.ABNORMALREASONREMARK,
                                                    HandlingMethodList = db.CHECKRESULTHANDLINGMETHOD.Where(h => h.CHECKRESULTUNIQUEID == x.UNIQUEID && h.ABNORMALREASONUNIQUEID == a.ABNORMALREASONUNIQUEID).OrderBy(h => h.HANDLINGMETHODID).Select(h => new HandlingMethodModel
                                                    {
                                                        Description = h.HANDLINGMETHODDESCRIPTION,
                                                        Remark = h.HANDLINGMETHODREMARK
                                                    }).ToList()
                                                }).ToList()
                                            }).ToList());
                                        }
                                    }
                                    else
                                    {
                                        item.ResultList.Add(job.UniqueID, null);
                                    }
                                }

                                model.ItemList.Add(item);

                                no++;
                            }
                        }
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

        public static RequestResult Export(ReportModel Model, Define.EnumExcelVersion ExcelVersion)
        {
            RequestResult result = new RequestResult();

            try
            {
                IWorkbook workBook = null;

                if (ExcelVersion == Define.EnumExcelVersion._2003)
                {
                    workBook = new HSSFWorkbook();
                }

                if (ExcelVersion == Define.EnumExcelVersion._2007)
                {
                    workBook = new XSSFWorkbook();
                }

                #region 標楷體 16px, UnderLine
                var headerFont = workBook.CreateFont();
                headerFont.FontName = "標楷體";
                headerFont.FontHeightInPoints = 16;
                headerFont.Underline = FontUnderlineType.Single;
                #endregion

                #region Header Style
                var headerStyle = workBook.CreateCellStyle();
                headerStyle.SetFont(headerFont);
                headerStyle.Alignment = HorizontalAlignment.Center;
                headerStyle.VerticalAlignment = VerticalAlignment.Top - 1;
                headerStyle.BorderTop = BorderStyle.None;
                headerStyle.BorderBottom = BorderStyle.None;
                headerStyle.BorderLeft = BorderStyle.None;
                headerStyle.BorderRight = BorderStyle.None;
                #endregion

                #region Cell Style
                var cellFont = workBook.CreateFont();
                cellFont.FontName = "標楷體";
                cellFont.Color = HSSFColor.Black.Index;
                cellFont.Boldweight = (short)FontBoldWeight.Normal;
                cellFont.FontHeightInPoints = 12;

                var cellStyle = workBook.CreateCellStyle();
                cellStyle.VerticalAlignment = VerticalAlignment.Center;
                cellStyle.BorderTop = BorderStyle.Thin;
                cellStyle.BorderBottom = BorderStyle.Thin;
                cellStyle.BorderLeft = BorderStyle.Thin;
                cellStyle.BorderRight = BorderStyle.Thin;
                cellStyle.SetFont(cellFont);

                var cellStyleNoneBorder = workBook.CreateCellStyle();
                cellStyleNoneBorder.VerticalAlignment = VerticalAlignment.Center;
                cellStyleNoneBorder.BorderTop = BorderStyle.None;
                cellStyleNoneBorder.BorderBottom = BorderStyle.None;
                cellStyleNoneBorder.BorderLeft = BorderStyle.None;
                cellStyleNoneBorder.BorderRight = BorderStyle.None;
                cellStyleNoneBorder.SetFont(cellFont);

                var cellStyleAlignCenter = workBook.CreateCellStyle();
                cellStyleAlignCenter.Alignment = HorizontalAlignment.Center;
                cellStyleAlignCenter.VerticalAlignment = VerticalAlignment.Center;
                cellStyleAlignCenter.BorderTop = BorderStyle.Thin;
                cellStyleAlignCenter.BorderBottom = BorderStyle.Thin;
                cellStyleAlignCenter.BorderLeft = BorderStyle.Thin;
                cellStyleAlignCenter.BorderRight = BorderStyle.Thin;
                cellStyleAlignCenter.SetFont(cellFont);

                var cellStyleAlignRight = workBook.CreateCellStyle();
                cellStyleAlignRight.Alignment = HorizontalAlignment.Right;
                cellStyleAlignRight.VerticalAlignment = VerticalAlignment.Center;
                cellStyleAlignRight.BorderTop = BorderStyle.Thin;
                cellStyleAlignRight.BorderBottom = BorderStyle.Thin;
                cellStyleAlignRight.BorderLeft = BorderStyle.Thin;
                cellStyleAlignRight.BorderRight = BorderStyle.Thin;
                cellStyleAlignRight.SetFont(cellFont);
                #endregion

                var worksheet = workBook.CreateSheet(Model.Date);

                IRow row;

                ICell cell;

                int lastCellIndex = 0;

                if (Model.JobList.Count > 0)
                {
                    lastCellIndex = 4 + Model.JobList.Count;
                }
                else
                {
                    lastCellIndex = 5;
                }

                int currentCellIndex = 0;

                #region Row 0
                row = worksheet.CreateRow(0);

                cell = row.CreateCell(0);
                cell.CellStyle = headerStyle;
                cell.SetCellValue(Model.OrganizationDescription);

                worksheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, lastCellIndex));
                #endregion

                #region Row 1
                row = worksheet.CreateRow(1);

                cell = row.CreateCell(0);
                cell.CellStyle = headerStyle;
                cell.SetCellValue(Model.RouteName);

                worksheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, lastCellIndex));
                #endregion

                #region Row 2
                row = worksheet.CreateRow(2);

                cell = row.CreateCell(0);
                cell.CellStyle = cellStyleNoneBorder;
                cell.SetCellValue(string.Format("{0}:{1}", Resources.Resource.CheckDate, Model.Date));

                worksheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, lastCellIndex));
                #endregion

                #region Row 3
                row = worksheet.CreateRow(3);

                cell = row.CreateCell(0);
                cell.CellStyle = cellStyleAlignCenter;
                cell.SetCellValue(Resources.Resource.Item);

                worksheet.AddMergedRegion(new CellRangeAddress(3, 4, 0, 0));

                cell = row.CreateCell(1);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.CheckItem);

                worksheet.AddMergedRegion(new CellRangeAddress(3, 4, 1, 1));

                cell = row.CreateCell(2);
                cell.CellStyle = cellStyleAlignCenter;
                cell.SetCellValue(Resources.Resource.Limit);

                worksheet.AddMergedRegion(new CellRangeAddress(3, 4, 2, 2));

                cell = row.CreateCell(3);
                cell.CellStyle = cellStyleAlignCenter;
                cell.SetCellValue(Resources.Resource.Unit);

                worksheet.AddMergedRegion(new CellRangeAddress(3, 4, 3, 3));

                cell = row.CreateCell(4);
                cell.CellStyle = cellStyleAlignCenter;
                cell.SetCellValue(Resources.Resource.CheckResult);

                if (Model.JobList.Count > 0)
                {
                    currentCellIndex = 4;

                    for (int i = 1; i < Model.JobList.Count; i++)
                    {
                        currentCellIndex = currentCellIndex + 1;

                        cell = row.CreateCell(currentCellIndex);
                        cell.CellStyle = cellStyle;
                    }

                    worksheet.AddMergedRegion(new CellRangeAddress(3, 3, 4, currentCellIndex));
                }
                else
                {
                    worksheet.AddMergedRegion(new CellRangeAddress(3, 4, 4, 4));
                }
                

                cell = row.CreateCell(lastCellIndex);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.Remark);

                worksheet.AddMergedRegion(new CellRangeAddress(3, 5, lastCellIndex, lastCellIndex));
                #endregion

                #region Row 4
                row = worksheet.CreateRow(4);

                cell = row.CreateCell(0);
                cell.CellStyle = cellStyle;
                cell = row.CreateCell(1);
                cell.CellStyle = cellStyle;
                cell = row.CreateCell(2);
                cell.CellStyle = cellStyle;
                cell = row.CreateCell(3);
                cell.CellStyle = cellStyle;

                if (Model.JobList.Count > 0)
                {
                    currentCellIndex = 4;

                    foreach (var job in Model.JobList)
                    {
                        cell = row.CreateCell(currentCellIndex);
                        cell.CellStyle = cellStyleAlignCenter;
                        cell.SetCellValue(job.Description);

                        currentCellIndex++;
                    }
                }
                else
                {
                    cell = row.CreateCell(4);
                    cell.CellStyle = cellStyle;
                }
                
                cell = row.CreateCell(lastCellIndex);
                cell.CellStyle = cellStyle;
                #endregion

                #region Row 5
                row = worksheet.CreateRow(5);

                cell = row.CreateCell(0);
                cell.CellStyle = cellStyleAlignRight;
                cell.SetCellValue(Resources.Resource.CheckUser);

                cell = row.CreateCell(1);
                cell.CellStyle = cellStyle;
                cell = row.CreateCell(2);
                cell.CellStyle = cellStyle;
                cell = row.CreateCell(3);
                cell.CellStyle = cellStyle;

                worksheet.AddMergedRegion(new CellRangeAddress(5, 5, 0, 3));

                if (Model.JobList.Count > 0)
                {
                    currentCellIndex = 4;

                    foreach (var job in Model.JobList)
                    {
                        cell = row.CreateCell(currentCellIndex);
                        cell.CellStyle = cellStyleAlignCenter;
                        cell.SetCellValue(job.Users);

                        currentCellIndex++;
                    }
                }
                else
                {
                    cell = row.CreateCell(4);
                    cell.CellStyle = cellStyle;
                }

                cell = row.CreateCell(lastCellIndex);
                cell.CellStyle = cellStyle;
                #endregion

                var currentRowIndex = 6;

                foreach (var item in Model.ItemList)
                {
                    row = worksheet.CreateRow(currentRowIndex);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyleAlignCenter;
                    cell.SetCellValue(item.No);

                    cell = row.CreateCell(1);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.Description);

                    cell = row.CreateCell(2);
                    cell.CellStyle = cellStyleAlignCenter;
                    cell.SetCellValue(item.Limit);

                    cell = row.CreateCell(3);
                    cell.CellStyle = cellStyleAlignCenter;
                    cell.SetCellValue(item.Unit);

                    if (Model.JobList.Count > 0)
                    {
                        currentCellIndex = 4;

                        foreach (var job in Model.JobList)
                        {
                            cell = row.CreateCell(currentCellIndex);
                            cell.CellStyle = cellStyleAlignCenter;
                            cell.SetCellValue(item.Result[job.UniqueID]);

                            currentCellIndex++;
                        }
                    }
                    else
                    {
                        cell = row.CreateCell(4);
                        cell.CellStyle = cellStyleAlignCenter;
                    }
                    
                    cell = row.CreateCell(lastCellIndex);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.Remark);
                    //cell.SetCellValue(item.Remark + item.FeelOptions);

                    currentRowIndex++;
                }

                if (Model.JobList.Count > 0)
                {
                    for (int i = 0; i <= lastCellIndex; i++)
                    {
                        worksheet.AutoSizeColumn(i);
                    }
                }
                else
                {
                    for (int i = 0; i <= 5; i++)
                    {
                        worksheet.AutoSizeColumn(i);
                    }
                }

                var output = new ExcelExportModel(Model.Date, ExcelVersion);

                using (FileStream fs = System.IO.File.OpenWrite(output.FullFileName))
                {
                    workBook.Write(fs);
                }

                byte[] buff = null;

                using (var fs = System.IO.File.OpenRead(output.FullFileName))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        long numBytes = new FileInfo(output.FullFileName).Length;

                        buff = br.ReadBytes((int)numBytes);

                        br.Close();
                    }

                    fs.Close();
                }

                output.Data = buff;

                result.ReturnData(output);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetTreeItem(List<Models.Shared.Organization> OrganizationList, string OrganizationUniqueID, Account Account)
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
                    { Define.EnumTreeAttribute.RouteUniqueID, string.Empty }
                };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                    {
                        var routeList = db.ROUTE.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                        foreach (var route in routeList)
                        {
                            var treeItem = new TreeItem() { Title = route.NAME };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Route.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", route.ID, route.NAME);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.RouteUniqueID] = route.UNIQUEID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }
                    }

                    var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                    foreach (var organization in organizationList)
                    {
                        var treeItem = new TreeItem() { Title = organization.Description };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                        attributes[Define.EnumTreeAttribute.RouteUniqueID] = string.Empty;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                            ||
                            (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.ROUTE.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID)))
                        {
                            treeItem.State = "closed";
                        }

                        treeItemList.Add(treeItem);
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

        public static RequestResult GetRootTreeItem(List<Models.Shared.Organization> OrganizationList, string OrganizationUniqueID, Account Account)
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
                    { Define.EnumTreeAttribute.RouteUniqueID, string.Empty }
                };

                var organization = OrganizationList.First(x => x.UniqueID == OrganizationUniqueID);

                var treeItem = new TreeItem() { Title = organization.Description };

                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                attributes[Define.EnumTreeAttribute.RouteUniqueID] = string.Empty;

                foreach (var attribute in attributes)
                {
                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                }

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                        ||
                        (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.ROUTE.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID)))
                    {
                        treeItem.State = "closed";
                    }
                }

                treeItemList.Add(treeItem);

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
