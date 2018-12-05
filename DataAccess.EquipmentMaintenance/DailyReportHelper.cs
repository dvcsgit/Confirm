using System;
using System.Linq;
using System.Reflection;
using Utility;
using Utility.Models;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.EquipmentMaintenance.DailyReport;
using Models.Authenticated;
using Models.Shared;
using System.Collections.Generic;
using DbEntity.MSSQL;
using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.Util;
using System.Text;
using NPOI.HSSF.UserModel;

namespace DataAccess.EquipmentMaintenance
{
    public class DailyReportHelper
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var jobResultList = new List<string>();
                    
                    var route = db.Route.First(x => x.UniqueID == Parameters.RouteUniqueID);

                    var model = new ReportModel()
                    {
                        Date = Parameters.DateString,
                        OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(route.OrganizationUniqueID),
                        RouteName = route.Name
                    };

                    var jobList = db.Job.Where(x => x.RouteUniqueID == route.UniqueID).OrderBy(x => x.BeginTime).ThenBy(x => x.Description).ToList();

                    foreach (var job in jobList)
                    {
                        if (JobCycleHelper.IsInCycle(DateTimeHelper.DateString2DateTime(Parameters.Date).Value, job.BeginDate, job.EndDate, job.CycleCount, job.CycleMode))
                        {
                            DateTime begin, end;

                            JobCycleHelper.GetDateSpan(DateTimeHelper.DateString2DateTime(Parameters.Date).Value, job.BeginDate, job.EndDate, job.CycleCount, job.CycleMode, out begin, out end);

                            var beginDateString = DateTimeHelper.DateTime2DateString(begin);
                            var endDateString = DateTimeHelper.DateTime2DateString(end);

                            if (!string.IsNullOrEmpty(job.BeginTime) && !string.IsNullOrEmpty(job.EndTime) && string.Compare(job.BeginTime, job.EndTime) > 0)
                            {
                                endDateString = DateTimeHelper.DateTime2DateString(end.AddDays(1));
                            }

                            var jobResult = db.JobResult.FirstOrDefault(x => x.JobUniqueID == job.UniqueID && x.BeginDate == beginDateString);

                            if (jobResult == null)
                            {
                                JobResultDataAccessor.Refresh(Guid.NewGuid().ToString(), job.UniqueID, beginDateString, endDateString);

                                jobResult = db.JobResult.FirstOrDefault(x => x.JobUniqueID == job.UniqueID && x.BeginDate == beginDateString);
                            }

                            if (jobResult != null)
                            {
                                jobResultList.Add(jobResult.UniqueID);

                                var jobModel = new JobModel()
                                {
                                    UniqueID = job.UniqueID,
                                    Description = job.Description,
                                    Users = jobResult.CheckUsers
                                };

                                model.JobList.Add(jobModel);
                            }
                        }
                    }

                    int no = 1;

                    var checkResultList = (from a in db.ArriveRecord
                                           join r in db.CheckResult
                                           on a.UniqueID equals r.ArriveRecordUniqueID
                                           where jobResultList.Contains(a.JobResultUniqueID)
                                           select r).ToList();

                    //var checkResultList = db.CheckResult.Where(x => x.RouteUniqueID == route.UniqueID && x.CheckDate == Parameters.Date).OrderBy(x => x.CheckTime).ToList();

                    var controlPointList = (from x in db.RouteControlPoint
                                            join c in db.ControlPoint
                                            on x.ControlPointUniqueID equals c.UniqueID
                                            where x.RouteUniqueID == route.UniqueID
                                            select new
                                            {
                                                c.UniqueID,
                                                c.Description,
                                                x.Seq
                                            }).OrderBy(x => x.Seq).ToList();

                    foreach (var controlPoint in controlPointList)
                    {
                        var controlPointCheckItemList = (from x in db.RouteControlPointCheckItem
                                                         join c in db.View_ControlPointCheckItem
                                                         on new { x.ControlPointUniqueID, x.CheckItemUniqueID } equals new { c.ControlPointUniqueID, c.CheckItemUniqueID }
                                                         join item in db.CheckItem
                                                         on x.CheckItemUniqueID equals item.UniqueID
                                                         where x.RouteUniqueID == route.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID
                                                         select new
                                                         {
                                                             UniqueID = c.CheckItemUniqueID,
                                                             CheckItemDescription = c.Description,
                                                             item.IsFeelItem,
                                                             c.LowerLimit,
                                                             c.LowerAlertLimit,
                                                             c.UpperAlertLimit,
                                                             c.UpperLimit,
                                                             c.Unit,
                                                             c.Remark,
                                                             x.Seq
                                                         }).OrderBy(x => x.Seq).ToList();

                        foreach (var checkItem in controlPointCheckItemList)
                        {
                            var item = new CheckItemModel()
                            {
                                No = no,
                                ControlPointDescription = controlPoint.Description,
                                EquipmentName = string.Empty,
                                PartDescription = string.Empty,
                                CheckItemDescription = checkItem.CheckItemDescription,
                                IsFeelItem = checkItem.IsFeelItem,
                                LowerLimit = checkItem.LowerLimit,
                                LowerAlertLimit = checkItem.LowerAlertLimit,
                                UpperAlertLimit = checkItem.UpperAlertLimit,
                                UpperLimit = checkItem.UpperLimit,
                                Unit = checkItem.Unit,
                                CheckItemRemark = checkItem.Remark,
                                FeelOptionDescriptionList = (from x in db.CheckItemFeelOption
                                                             where x.CheckItemUniqueID == checkItem.UniqueID
                                                             orderby x.Seq
                                                             select x.Description + (x.IsAbnormal ? "(" + Resources.Resource.Abnormal + ")" : "")).ToList()
                            };

                            foreach (var job in model.JobList)
                            {
                                var jobControlPointCheckItem = db.JobControlPointCheckItem.FirstOrDefault(x => x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID);

                                if (jobControlPointCheckItem != null)
                                {
                                    if (Parameters.IsShowLastData)
                                    {
                                        var checkResult = checkResultList.Where(x => x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && string.IsNullOrEmpty(x.EquipmentUniqueID) && x.CheckItemUniqueID == checkItem.UniqueID).OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).Select(x => new CheckResultModel
                                        {
                                            Result = x.Result,
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
                                        item.ResultList.Add(job.UniqueID, checkResultList.Where(x => x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && string.IsNullOrEmpty(x.EquipmentUniqueID) && x.CheckItemUniqueID == checkItem.UniqueID).OrderBy(x=>x.CheckDate).ThenBy(x=>x.CheckTime).Select(x => new CheckResultModel
                                        {
                                            Result = x.Result,
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

                        var equipmentList = (from x in db.RouteEquipment
                                             join e in db.Equipment
                                             on x.EquipmentUniqueID equals e.UniqueID
                                             join p in db.EquipmentPart
                                             on new { x.EquipmentUniqueID, x.PartUniqueID } equals new { p.EquipmentUniqueID, PartUniqueID = p.UniqueID } into tmpPart
                                             from p in tmpPart.DefaultIfEmpty()
                                             where x.RouteUniqueID == route.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID
                                             select new
                                             {
                                                 x.EquipmentUniqueID,
                                                 x.PartUniqueID,
                                                 EquipmentName = e.Name,
                                                 PartDescription = p != null ? p.Description : "",
                                                 x.Seq
                                             }).OrderBy(x => x.Seq).ToList();

                        foreach (var equipment in equipmentList)
                        {
                            var equipmentCheckItemList = (from x in db.RouteEquipmentCheckItem
                                                          join c in db.View_EquipmentCheckItem
                                                          on new { x.EquipmentUniqueID, x.PartUniqueID, x.CheckItemUniqueID } equals new { c.EquipmentUniqueID, c.PartUniqueID, c.CheckItemUniqueID }
                                                          join i in db.CheckItem
                                                          on x.CheckItemUniqueID equals i.UniqueID
                                                          where x.RouteUniqueID == route.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == equipment.EquipmentUniqueID && x.PartUniqueID == equipment.PartUniqueID
                                                          select new
                                                          {
                                                              UniqueID = c.CheckItemUniqueID,
                                                              CheckItemDescription = c.Description,
                                                              i.IsFeelItem,
                                                              c.LowerLimit,
                                                              c.LowerAlertLimit,
                                                              c.UpperAlertLimit,
                                                              c.UpperLimit,
                                                              c.Unit,
                                                              c.Remark,
                                                              x.Seq
                                                          }).OrderBy(x => x.Seq).ToList();

                            foreach (var checkItem in equipmentCheckItemList)
                            {
                                var item = new CheckItemModel()
                                {
                                    No = no,
                                    ControlPointDescription = controlPoint.Description,
                                    EquipmentName = equipment.EquipmentName,
                                    PartDescription = equipment.PartDescription,
                                    CheckItemDescription = checkItem.CheckItemDescription,
                                    IsFeelItem = checkItem.IsFeelItem,
                                    LowerLimit = checkItem.LowerLimit,
                                    LowerAlertLimit = checkItem.LowerAlertLimit,
                                    UpperAlertLimit = checkItem.UpperAlertLimit,
                                    UpperLimit = checkItem.UpperLimit,
                                    Unit = checkItem.Unit,
                                    CheckItemRemark = checkItem.Remark,
                                    FeelOptionDescriptionList = (from x in db.CheckItemFeelOption
                                                                 where x.CheckItemUniqueID == checkItem.UniqueID
                                                                 orderby x.Seq
                                                                 select x.Description + (x.IsAbnormal ? "(" + Resources.Resource.Abnormal + ")" : "")).ToList()
                                };

                                foreach (var job in model.JobList)
                                {
                                    var jobEquipmentCheckItem = db.JobEquipmentCheckItem.FirstOrDefault(x => x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == equipment.EquipmentUniqueID && x.PartUniqueID == equipment.PartUniqueID && x.CheckItemUniqueID == checkItem.UniqueID);

                                    if (jobEquipmentCheckItem != null)
                                    {
                                        if(Parameters.IsShowLastData)
                                        {
                                            var checkResult = checkResultList.Where(x => x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == equipment.EquipmentUniqueID && x.PartUniqueID == equipment.PartUniqueID && x.CheckItemUniqueID == checkItem.UniqueID).OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).Select(x => new CheckResultModel
                                        {
                                            Result = x.Result,
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
                                        else{
                                            item.ResultList.Add(job.UniqueID, checkResultList.Where(x => x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == equipment.EquipmentUniqueID && x.PartUniqueID == equipment.PartUniqueID && x.CheckItemUniqueID == checkItem.UniqueID).OrderBy(x=>x.CheckDate).ThenBy(x=>x.CheckTime).Select(x => new CheckResultModel
                                        {
                                            Result = x.Result,
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

                using (EDbEntities edb = new EDbEntities())
                {
                    if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                    {
                        var routeList = edb.Route.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                        foreach (var route in routeList)
                        {
                            var treeItem = new TreeItem() { Title = route.Name };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Route.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", route.ID, route.Name);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.RouteUniqueID] = route.UniqueID;

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
                            (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && edb.Route.Any(x => x.OrganizationUniqueID == organization.UniqueID)))
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

                using (EDbEntities edb = new EDbEntities())
                {
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

                    if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                        ||
                        (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && edb.Route.Any(x => x.OrganizationUniqueID == organization.UniqueID)))
                    {
                        treeItem.State = "closed";
                    }

                    treeItemList.Add(treeItem);
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
