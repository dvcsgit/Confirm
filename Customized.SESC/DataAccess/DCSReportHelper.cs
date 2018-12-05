using Customized.SESC.Models.DCSReport;
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.Authenticated;
using Models.Shared;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace Customized.SESC.DataAccess
{
    public class DCSReportHelper
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var route = db.Route.First(x => x.UniqueID == Parameters.RouteUniqueID);

                    var checkResultList = db.CheckResult.Where(x => x.RouteUniqueID == route.UniqueID && x.CheckDate == Parameters.Date).ToList();

                    var model = new ReportModel()
                    {
                        Date = Parameters.DateString,
                        RouteName = route.Name
                    };

                    var equipmentList = (from x in db.RouteEquipment
                                         join e in db.Equipment
                                         on x.EquipmentUniqueID equals e.UniqueID
                                         where x.RouteUniqueID == route.UniqueID
                                         select new
                                         {
                                             e.UniqueID,
                                             e.Name,
                                             x.Seq
                                         }).OrderBy(x => x.Seq).ToList();

                    int no = 1;

                    foreach (var equipment in equipmentList)
                    {
                        var dcsCheckItemList = (from x in db.RouteEquipmentCheckItem
                                                join c in db.CheckItem
                                                on x.CheckItemUniqueID equals c.UniqueID
                                                where x.RouteUniqueID == route.UniqueID && x.EquipmentUniqueID == equipment.UniqueID && c.Description.StartsWith("【DCS】")
                                                select new
                                                {
                                                    c.UniqueID,
                                                    c.ID,
                                                    c.Description,
                                                    c.LowerLimit,
                                                    c.UpperLimit,
                                                    c.Unit
                                                }).OrderBy(x => x.Description).ToList();

                        var factoryCheckItemList = (from x in db.RouteEquipmentCheckItem
                                                    join c in db.CheckItem
                                                    on x.CheckItemUniqueID equals c.UniqueID
                                                    where x.RouteUniqueID == route.UniqueID && x.EquipmentUniqueID == equipment.UniqueID && c.Description.StartsWith("【現場】")
                                                    select new
                                                    {
                                                        c.UniqueID,
                                                        c.ID,
                                                        c.Description
                                                    }).OrderBy(x => x.Description).ToList();
                        int index = 0;

                        foreach (var dcsCheckItem in dcsCheckItemList)
                        {
                            var factoryCheckItem = factoryCheckItemList[index];

                            var dcsCheckResult = checkResultList.Where(x => x.EquipmentUniqueID == equipment.UniqueID && x.CheckItemUniqueID == dcsCheckItem.UniqueID).OrderByDescending(x => x.CheckTime).FirstOrDefault();
                            var factoryCheckResult = checkResultList.Where(x => x.EquipmentUniqueID == equipment.UniqueID && x.CheckItemUniqueID == factoryCheckItem.UniqueID).OrderByDescending(x => x.CheckTime).FirstOrDefault();

                            model.ItemList.Add(new ItemModel()
                            {
                                No = no,
                                EquipmentName = equipment.Name,
                                CheckItemDescription = dcsCheckItem.Description,
                                Unit = dcsCheckItem.Unit,
                                DCSCheckItemID = dcsCheckItem.ID,
                                FactoryCheckItemID = factoryCheckItem.ID,
                                LowerLimit = dcsCheckItem.LowerLimit,
                                UpperLimit = dcsCheckItem.UpperLimit,
                                DCSValue = dcsCheckResult != null ? dcsCheckResult.Value : default(double?),
                                FactoryValue = factoryCheckResult != null ? factoryCheckResult.Value : default(double?)
                            });

                            index++;
                            no++;
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

                var font = workBook.CreateFont();
                font.FontName = "標楷體";
                font.Color = HSSFColor.Black.Index;
                font.Boldweight = (short)FontBoldWeight.Normal;
                font.FontHeightInPoints = 12;

                #region Header Style
                var headerStyle = workBook.CreateCellStyle();
                headerStyle.SetFont(font);
                headerStyle.Alignment = HorizontalAlignment.Left;
                headerStyle.VerticalAlignment = VerticalAlignment.Top - 1;
                headerStyle.BorderTop = BorderStyle.None;
                headerStyle.BorderBottom = BorderStyle.None;
                headerStyle.BorderLeft = BorderStyle.None;
                headerStyle.BorderRight = BorderStyle.None;
                #endregion

                #region Cell Style
                var cellStyle = workBook.CreateCellStyle();
                cellStyle.BorderTop = BorderStyle.Thin;
                cellStyle.BorderBottom = BorderStyle.Thin;
                cellStyle.BorderLeft = BorderStyle.Thin;
                cellStyle.BorderRight = BorderStyle.Thin;
                cellStyle.SetFont(font);
                #endregion

                var worksheet = workBook.CreateSheet(Model.RouteName);

                IRow row;

                ICell cell;

                #region Row 0
                row = worksheet.CreateRow(0);

                cell = row.CreateCell(0);
                cell.CellStyle = headerStyle;
                cell.SetCellValue(string.Format("執行日期：{0}", Model.Date));

                worksheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 10));
                #endregion

                #region Row 1
                row = worksheet.CreateRow(1);

                cell = row.CreateCell(0);
                cell.CellStyle = cellStyle;
                cell.SetCellValue("項次");

                cell = row.CreateCell(1);
                cell.CellStyle = cellStyle;
                cell.SetCellValue("設備名稱");

                cell = row.CreateCell(2);
                cell.CellStyle = cellStyle;
                cell.SetCellValue("比對項目");

                cell = row.CreateCell(3);
                cell.CellStyle = cellStyle;
                cell.SetCellValue("單位");

                cell = row.CreateCell(4);
                cell.CellStyle = cellStyle;
                cell.SetCellValue("DCS錶計編號");

                cell = row.CreateCell(5);
                cell.CellStyle = cellStyle;
                cell.SetCellValue("DCS數值【1】");

                cell = row.CreateCell(6);
                cell.CellStyle = cellStyle;
                cell.SetCellValue("現場錶計編號");

                cell = row.CreateCell(7);
                cell.CellStyle = cellStyle;
                cell.SetCellValue("現場數值【2】");

                cell = row.CreateCell(8);
                cell.CellStyle = cellStyle;
                cell.SetCellValue("誤差值【1】減【2】");

                cell = row.CreateCell(9);
                cell.CellStyle = cellStyle;
                cell.SetCellValue("誤差管理值【1】減【2】");

                cell = row.CreateCell(10);
                cell.CellStyle = cellStyle;
                cell.SetCellValue("故障維修工單編號");
                #endregion

                var currentRowIndex = 2;

                foreach (var item in Model.ItemList)
                {
                    row = worksheet.CreateRow(currentRowIndex);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.No);

                    cell = row.CreateCell(1);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.EquipmentName);

                    cell = row.CreateCell(2);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.CheckItemDisplay);

                    cell = row.CreateCell(3);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.Unit);

                    cell = row.CreateCell(4);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.DCSCheckItemID);

                    cell = row.CreateCell(5);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.DCSValue.HasValue ? item.DCSValue.Value.ToString() : "");

                    cell = row.CreateCell(6);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.FactoryCheckItemID);

                    cell = row.CreateCell(7);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.FactoryValue.HasValue ? item.FactoryValue.Value.ToString() : "");

                    cell = row.CreateCell(8);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.Diff);

                    cell = row.CreateCell(9);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.Limit);

                    cell = row.CreateCell(10);
                    cell.CellStyle = cellStyle;

                    currentRowIndex++;
                }

                var output = new ExcelExportModel(string.Format("{0}_{1}", Model.RouteName, Model.Date), ExcelVersion);

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
                        var routeList = edb.Route.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && x.ID.StartsWith("DCS")).OrderBy(x => x.ID).ToList();

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
                                (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && edb.Route.Any(x => x.OrganizationUniqueID == organization.UniqueID && x.ID.StartsWith("DCS"))))
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
