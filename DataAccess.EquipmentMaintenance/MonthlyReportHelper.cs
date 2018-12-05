using System;
using System.Text;
using System.Linq;
using System.Web.Mvc;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.EquipmentMaintenance.MonthlyReport;
using Models.Authenticated;
using Models.Shared;
using DbEntity.MSSQL;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.Util;
using System.IO;

namespace DataAccess.EquipmentMaintenance
{
    public class MonthlyReportHelper
    {
        public static RequestResult GetQueryFormModel()
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new QueryFormModel()
                {
                    YearSelectItemList = new List<SelectListItem>() 
                    { 
                        new SelectListItem()
                        {
                            Selected = true,
                            Text = Resources.Resource.SelectOne,
                            Value = ""
                        }
                    }
                };

                using (EDbEntities db = new EDbEntities())
                {
                    var min = db.CheckResult.Min(x => x.CheckDate);

                    var max = db.CheckResult.Max(x => x.CheckDate);

                    var minYear = int.Parse(min.Substring(0, 4));
                    var maxYear = int.Parse(max.Substring(0, 4));

                    for (int year = minYear; year <= maxYear; year++)
                    {
                        model.YearSelectItemList.Add(new SelectListItem()
                        {
                            Text = year.ToString().PadLeft(4, '0'),
                            Value = year.ToString().PadLeft(4, '0')
                        });
                    }
                }

                result.ReturnData(model);
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
                using (EDbEntities db = new EDbEntities())
                {
                    var route = db.Route.First(x => x.UniqueID == Parameters.RouteUniqueID);

                    var model = new ReportModel()
                    {
                        Year = int.Parse(Parameters.Year),
                        Month = int.Parse(Parameters.Month),
                        OrganizationDescription = OrganizationDataAccessor.GetOrganizationFullDescriptionWithOutArrow(route.OrganizationUniqueID),
                        RouteName = route.Name,
                        JobList = db.Job.Where(x => x.RouteUniqueID == route.UniqueID).OrderBy(x => x.BeginTime).ThenBy(x => x.Description).Select(x => new JobModel
                        {
                            UniqueID = x.UniqueID,
                            Description = x.Description
                        }).ToList()
                    };

                    int no = 1;

                    var checkResultList = db.CheckResult.Where(x => x.RouteUniqueID == route.UniqueID && x.CheckDate.StartsWith(Parameters.Ym)).ToList();

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
                                                         where x.RouteUniqueID == route.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID
                                                         select new
                                                         {
                                                             UniqueID = c.CheckItemUniqueID,
                                                             CheckItemDescription = c.Description,
                                                             c.LowerLimit,
                                                             c.UpperLimit,
                                                             c.Unit,
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
                                LowerLimit = checkItem.LowerLimit,
                                UpperLimit = checkItem.UpperLimit,
                                Unit = checkItem.Unit
                            };

                            foreach (var job in model.JobList)
                            {
                                var jobControlPointCheckItem = db.JobControlPointCheckItem.FirstOrDefault(x => x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID);

                                if (jobControlPointCheckItem != null)
                                {
                                    item.ResultList.Add(job.UniqueID, checkResultList.Where(x => x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && string.IsNullOrEmpty(x.EquipmentUniqueID) && x.CheckItemUniqueID == checkItem.UniqueID).Select(x => new CheckResultModel
                                    {
                                        CheckDate = x.CheckDate,
                                        Result = x.Result
                                    }).ToList());
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
                                                          where x.RouteUniqueID == route.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == equipment.EquipmentUniqueID && x.PartUniqueID == equipment.PartUniqueID
                                                          select new
                                                          {
                                                              UniqueID = c.CheckItemUniqueID,
                                                              CheckItemDescription = c.Description,
                                                              c.LowerLimit,
                                                              c.UpperLimit,
                                                              c.Unit,
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
                                    LowerLimit = checkItem.LowerLimit,
                                    UpperLimit = checkItem.UpperLimit,
                                    Unit = checkItem.Unit
                                };

                                foreach (var job in model.JobList)
                                {
                                    var jobEquipmentCheckItem = db.JobEquipmentCheckItem.FirstOrDefault(x => x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == equipment.EquipmentUniqueID && x.PartUniqueID == equipment.PartUniqueID && x.CheckItemUniqueID == checkItem.UniqueID);

                                    if (jobEquipmentCheckItem != null)
                                    {
                                        item.ResultList.Add(job.UniqueID, checkResultList.Where(x => x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == equipment.EquipmentUniqueID && x.PartUniqueID == equipment.PartUniqueID && x.CheckItemUniqueID == checkItem.UniqueID).Select(x => new CheckResultModel
                                        {
                                            CheckDate = x.CheckDate,
                                            Result = x.Result
                                        }).ToList());
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
                #endregion

                var worksheet = workBook.CreateSheet(Model.Ym);

                IRow row;

                ICell cell;

                int lastCellIndex = 4 + Model.DaysInMonth;
                int currentRowIndex = 0;
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
                cell.SetCellValue(string.Format("{0}:{1}", Resources.Resource.CheckDate, Model.Ym));

                worksheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, lastCellIndex));
                #endregion

                #region Row 3
                row = worksheet.CreateRow(3);

                cell = row.CreateCell(0);
                cell.CellStyle = cellStyleAlignCenter;
                cell.SetCellValue(Resources.Resource.Item);

                cell = row.CreateCell(1);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.CheckItem);

                cell = row.CreateCell(2);
                cell.CellStyle = cellStyleAlignCenter;
                cell.SetCellValue(Resources.Resource.Limit);

                cell = row.CreateCell(3);
                cell.CellStyle = cellStyleAlignCenter;
                cell.SetCellValue(Resources.Resource.Unit);

                cell = row.CreateCell(4);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(Resources.Resource.Job);

                currentCellIndex = 5;

                for (int day = 1; day <= Model.DaysInMonth; day++)
                {
                    cell = row.CreateCell(currentCellIndex);
                    cell.CellStyle = cellStyleAlignCenter;
                    cell.SetCellValue(day.ToString().PadLeft(2,'0'));

                    currentCellIndex++;
                }
                #endregion

                currentRowIndex = 4;

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
                        worksheet.AddMergedRegion(new CellRangeAddress(currentRowIndex, currentRowIndex + Model.JobList.Count - 1, 0, 0));
                        worksheet.AddMergedRegion(new CellRangeAddress(currentRowIndex, currentRowIndex + Model.JobList.Count - 1, 1, 1));
                        worksheet.AddMergedRegion(new CellRangeAddress(currentRowIndex, currentRowIndex + Model.JobList.Count - 1, 2, 2));
                        worksheet.AddMergedRegion(new CellRangeAddress(currentRowIndex, currentRowIndex + Model.JobList.Count - 1, 3, 3));

                        cell = row.CreateCell(4);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(Model.JobList[0].Description);

                        currentCellIndex = 5;

                        var checkResult = item.ResultList[Model.JobList[0].UniqueID];

                        if (checkResult != null)
                        {
                            for (int day = 1; day <= Model.DaysInMonth; day++)
                            {
                                var temp = checkResult.Where(x => x.Day == day).ToList();

                                if (temp.Count > 0)
                                {
                                    var sb = new StringBuilder();

                                    foreach (var t in temp)
                                    {
                                        sb.Append(t.Result);
                                        sb.Append("/");
                                    }

                                    sb.Remove(sb.Length - 1, 1);

                                    cell = row.CreateCell(currentCellIndex);
                                    cell.CellStyle = cellStyleAlignCenter;
                                    cell.SetCellValue(sb.ToString());
                                }
                                else
                                {
                                    cell = row.CreateCell(currentCellIndex);
                                    cell.CellStyle = cellStyle;
                                }

                                currentCellIndex++;
                            }
                        }
                        else
                        {
                            for (int day = 1; day <= Model.DaysInMonth; day++)
                            {
                                cell = row.CreateCell(currentCellIndex);
                                cell.CellStyle = cellStyle;
                                cell.SetCellValue("-");

                                currentCellIndex++;
                            }
                        }

                        currentRowIndex++;

                        for (int i = 1; i < Model.JobList.Count; i++)
                        {
                            row = worksheet.CreateRow(currentRowIndex);

                            cell = row.CreateCell(0);
                            cell.CellStyle = cellStyle;
                            cell = row.CreateCell(1);
                            cell.CellStyle = cellStyle;
                            cell = row.CreateCell(2);
                            cell.CellStyle = cellStyle;
                            cell = row.CreateCell(3);
                            cell.CellStyle = cellStyle;

                            cell = row.CreateCell(4);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(Model.JobList[i].Description);

                            currentCellIndex = 5;

                            checkResult = item.ResultList[Model.JobList[i].UniqueID];

                            if (checkResult != null)
                            {
                                for (int day = 1; day <= Model.DaysInMonth; day++)
                                {
                                    var temp = checkResult.Where(x => x.Day == day).ToList();

                                    if (temp.Count > 0)
                                    {
                                        var sb = new StringBuilder();

                                        foreach (var t in temp)
                                        {
                                            sb.Append(t.Result);
                                            sb.Append("/");
                                        }

                                        sb.Remove(sb.Length - 1, 1);

                                        cell = row.CreateCell(currentCellIndex);
                                        cell.CellStyle = cellStyleAlignCenter;
                                        cell.SetCellValue(sb.ToString());
                                    }
                                    else
                                    {
                                        cell = row.CreateCell(currentCellIndex);
                                        cell.CellStyle = cellStyle;
                                    }

                                    currentCellIndex++;
                                }
                            }
                            else
                            {
                                for (int day = 1; day <= Model.DaysInMonth; day++)
                                {
                                    cell = row.CreateCell(currentCellIndex);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("-");

                                    currentCellIndex++;
                                }
                            }

                            currentRowIndex++;
                        }
                    }
                    else
                    {
                        cell = row.CreateCell(4);
                        cell.CellStyle = cellStyle;

                        currentCellIndex = 5;

                        for (int day = 1; day <= Model.DaysInMonth; day++)
                        {
                            cell = row.CreateCell(currentCellIndex);
                            cell.CellStyle = cellStyle;

                            currentCellIndex++;
                        }

                        currentRowIndex++;
                    }
                }

                //for (int i = 0; i <= lastCellIndex; i++)
                //{
                //    worksheet.AutoSizeColumn(i);
                //}

                var output = new ExcelExportModel(Model.Ym, ExcelVersion);

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

                result.ReturnData(err);
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
