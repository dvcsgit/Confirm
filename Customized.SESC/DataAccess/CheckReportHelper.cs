using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.Authenticated;
using Models.Shared;
using System;
using System.Collections.Generic;
using System.Reflection;
using Utility;
using Utility.Models;
using System.Linq;
using Customized.SESC.Models.CheckReport;
using System.Text;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.Util;
using System.IO;

namespace Customized.SESC.DataAccess
{
    public class CheckReportHelper
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                int no=1;

                using (EDbEntities db = new EDbEntities())
                {
                    var route = db.Route.First(x => x.UniqueID == Parameters.RouteUniqueID);

                    var checkResultList = db.CheckResult.Where(x => x.RouteUniqueID == route.UniqueID && x.CheckDate == Parameters.Date).ToList();

                    var model = new ReportModel()
                    {
                        Date = Parameters.DateString,
                        RouteName = route.Name,
                        JobList = db.Job.Where(x => x.RouteUniqueID == route.UniqueID).Select(x => new JobModel
                        {
                            UniqueID = x.UniqueID,
                            Description = x.Description
                        }).ToList()
                    };

                    foreach (var job in model.JobList)
                    {
                        if (job.Description.StartsWith("大夜")) {
                            job.Seq = 1;
                        }
                        else if (job.Description.StartsWith("白天")) {
                            job.Seq = 2;
                        }
                        else if (job.Description.StartsWith("小夜"))
                        {
                            job.Seq = 3;
                        }
                        else
                        {
                            job.Seq = 99;
                        }
                    }

                    model.JobList = model.JobList.OrderBy(x => x.Seq).ThenBy(x => x.Description).ToList();

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
                                                             c.Remark,
                                                             x.Seq
                                                         }).OrderBy(x => x.Seq).ToList();

                        foreach (var checkItem in controlPointCheckItemList)
                        {
                            var item = new ItemModel()
                            {
                                No = no,
                                ControlPointDescription = controlPoint.Description,
                                EquipmentName = string.Empty,
                                CheckItemDescription = checkItem.CheckItemDescription,
                                LowerLimit = checkItem.LowerLimit,
                                UpperLimit = checkItem.UpperLimit,
                                Unit = checkItem.Unit,
                                Remark = checkItem.Remark
                            };

                            foreach (var job in model.JobList)
                            {
                                var jobControlPointCheckItem = db.JobControlPointCheckItem.FirstOrDefault(x => x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID);

                                if (jobControlPointCheckItem != null)
                                {
                                    var query = checkResultList.Where(x => x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && string.IsNullOrEmpty(x.EquipmentUniqueID) && x.CheckItemUniqueID == checkItem.UniqueID).ToList();

                                    if (query.Count > 0)
                                    {
                                        var sb = new StringBuilder();

                                        foreach (var q in query)
                                        {
                                            sb.Append(q.Result);
                                            sb.Append("/");
                                        }

                                        sb.Remove(sb.Length - 1, 1);

                                        item.Result.Add(job.UniqueID, sb.ToString());
                                    }
                                    else
                                    {
                                        item.Result.Add(job.UniqueID, string.Empty);
                                    }
                                }
                                else
                                {
                                    item.Result.Add(job.UniqueID, "-");
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
                                                              c.Remark,
                                                              x.Seq
                                                          }).OrderBy(x => x.Seq).ToList();

                            foreach (var checkItem in equipmentCheckItemList)
                            {
                                var item = new ItemModel()
                                {
                                    No = no,
                                    ControlPointDescription = controlPoint.Description,
                                    EquipmentName = equipment.EquipmentName,
                                    PartDescription = equipment.PartDescription,
                                    CheckItemDescription = checkItem.CheckItemDescription,
                                    LowerLimit = checkItem.LowerLimit,
                                    UpperLimit = checkItem.UpperLimit,
                                    Unit = checkItem.Unit,
                                    Remark = checkItem.Remark
                                };

                                foreach (var job in model.JobList)
                                {
                                    var jobEquipmentCheckItem = db.JobEquipmentCheckItem.FirstOrDefault(x => x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == equipment.EquipmentUniqueID && x.PartUniqueID == equipment.PartUniqueID && x.CheckItemUniqueID == checkItem.UniqueID);

                                    if (jobEquipmentCheckItem != null)
                                    {
                                        var query = checkResultList.Where(x => x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == equipment.EquipmentUniqueID && x.PartUniqueID == equipment.PartUniqueID && x.CheckItemUniqueID == checkItem.UniqueID).ToList();

                                        if (query.Count > 0)
                                        {
                                            var sb = new StringBuilder();

                                            foreach (var q in query)
                                            {
                                                sb.Append(q.Result);
                                                sb.Append("/");
                                            }

                                            sb.Remove(sb.Length - 1, 1);

                                            item.Result.Add(job.UniqueID, sb.ToString());
                                        }
                                        else
                                        {
                                            item.Result.Add(job.UniqueID, string.Empty);
                                        }
                                    }
                                    else
                                    {
                                        item.Result.Add(job.UniqueID, "-");
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
                cellStyle.BorderTop = BorderStyle.Thin;
                cellStyle.BorderBottom = BorderStyle.Thin;
                cellStyle.BorderLeft = BorderStyle.Thin;
                cellStyle.BorderRight = BorderStyle.Thin;
                cellStyle.SetFont(cellFont);

                var cellStyle2 = workBook.CreateCellStyle();
                cellStyle2.BorderTop = BorderStyle.None;
                cellStyle2.BorderBottom = BorderStyle.None;
                cellStyle2.BorderLeft = BorderStyle.None;
                cellStyle2.BorderRight = BorderStyle.None;
                cellStyle2.SetFont(cellFont);

                var cellStyle3 = workBook.CreateCellStyle();
                cellStyle3.Alignment = HorizontalAlignment.Center;
                cellStyle3.BorderTop = BorderStyle.Thin;
                cellStyle3.BorderBottom = BorderStyle.Thin;
                cellStyle3.BorderLeft = BorderStyle.Thin;
                cellStyle3.BorderRight = BorderStyle.Thin;
                cellStyle3.SetFont(cellFont);
                #endregion

                var worksheet = workBook.CreateSheet(Model.Date);

                IRow row;

                ICell cell;

                #region Row 0
                row = worksheet.CreateRow(0);

                cell = row.CreateCell(0);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("新店垃圾焚化廠");

                worksheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 4 + Model.JobList.Count));
                #endregion

                #region Row 1
                row = worksheet.CreateRow(1);

                cell = row.CreateCell(0);
                cell.CellStyle = headerStyle;
                cell.SetCellValue(Model.RouteName);

                worksheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 4 + Model.JobList.Count));
                #endregion

                #region Row 2
                row = worksheet.CreateRow(2);

                cell = row.CreateCell(0);
                cell.CellStyle = cellStyle2;
                cell.SetCellValue(string.Format("紀錄日期:{0}", Model.Date));

                worksheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 4 + Model.JobList.Count));
                #endregion

                #region Row 3
                row = worksheet.CreateRow(3);

                cell = row.CreateCell(0);
                cell.CellStyle = cellStyle3;
                cell.SetCellValue("項次");

                cell = row.CreateCell(1);
                cell.CellStyle = cellStyle;
                cell.SetCellValue("檢查紀錄項目");

                cell = row.CreateCell(2);
                cell.CellStyle = cellStyle3;
                cell.SetCellValue("檢查要求及管理值");

                cell = row.CreateCell(3);
                cell.CellStyle = cellStyle3;
                cell.SetCellValue("單位");

                cell = row.CreateCell(4);
                cell.CellStyle = cellStyle3;
                cell.SetCellValue("檢查紀錄結果");

                for (int i = 0; i < Model.JobList.Count - 1; i++)
                {
                    cell = row.CreateCell(i + 5);
                    cell.CellStyle = cellStyle;
                }

                cell = row.CreateCell(4 + Model.JobList.Count);
                cell.CellStyle = cellStyle;
                cell.SetCellValue("備註說明欄");
                #endregion

                var currentRowIndex = 4;

                if (Model.JobList.Count > 0)
                {
                    row = worksheet.CreateRow(4);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(1);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(2);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(3);
                    cell.CellStyle = cellStyle;

                    currentRowIndex = 5;

                    var currentCellIndex = 4;

                    foreach (var job in Model.JobList)
                    {
                        cell = row.CreateCell(currentCellIndex);
                        cell.CellStyle = cellStyle3;
                        cell.SetCellValue(job.Description);

                        currentCellIndex++;
                    }

                    cell = row.CreateCell(currentCellIndex);
                    cell.CellStyle = cellStyle;

                    worksheet.AddMergedRegion(new CellRangeAddress(3, 3, 4, 3 + Model.JobList.Count));

                    worksheet.AddMergedRegion(new CellRangeAddress(3, 4, 0, 0));
                    worksheet.AddMergedRegion(new CellRangeAddress(3, 4, 1, 1));
                    worksheet.AddMergedRegion(new CellRangeAddress(3, 4, 2, 2));
                    worksheet.AddMergedRegion(new CellRangeAddress(3, 4, 3, 3));
                    worksheet.AddMergedRegion(new CellRangeAddress(3, 4, currentCellIndex, currentCellIndex));
                }

                foreach (var item in Model.ItemList)
                {
                    row = worksheet.CreateRow(currentRowIndex);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle3;
                    cell.SetCellValue(item.No);

                    cell = row.CreateCell(1);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.Description);

                    cell = row.CreateCell(2);
                    cell.CellStyle = cellStyle3;
                    cell.SetCellValue(item.Limit);

                    cell = row.CreateCell(3);
                    cell.CellStyle = cellStyle3;
                    cell.SetCellValue(item.Unit);

                    var currentCellIndex = 4;

                    if (Model.JobList.Count > 0)
                    {
                        foreach (var job in Model.JobList)
                        {
                            cell = row.CreateCell(currentCellIndex);
                            cell.CellStyle = cellStyle3;
                            cell.SetCellValue(item.Result[job.UniqueID]);

                            currentCellIndex++;
                        }
                    }
                    else
                    {
                        cell = row.CreateCell(currentCellIndex);
                        cell.CellStyle = cellStyle;
                    }

                    cell = row.CreateCell(4 + Model.JobList.Count);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.Remark);

                    currentRowIndex++;
                }

                if (Model.JobList.Count > 0)
                {
                    for (int i = 0; i < Model.JobList.Count + 5; i++)
                    {
                        worksheet.AutoSizeColumn(i);
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
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

                    using (DbEntities db = new DbEntities())
                    {
                        var organizationList = db.Organization.Where(x => x.ParentUniqueID == OrganizationUniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

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

                            if (db.Organization.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                                ||
                                (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && edb.Route.Any(x => x.OrganizationUniqueID == organization.UniqueID)))
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
