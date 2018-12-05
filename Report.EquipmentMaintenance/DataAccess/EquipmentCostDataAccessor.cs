using Report.EquipmentMaintenance.Models.EquipmentCost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if ORACLE
using DbEntity.ORACLE.EquipmentMaintenance;
#else
using DbEntity.MSSQL.EquipmentMaintenance;
#endif
using System.Threading.Tasks;
using Utility.Models;
using Utility;
using System.Reflection;
using DataAccess;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;
using System.IO;
using System.Data.Entity;
using System.Data.Entity.SqlServer;

namespace Report.EquipmentMaintenance.DataAccess
{
    public class EquipmentCostDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)
        {


            RequestResult result = new RequestResult();
            try
            {
                var model = new GridViewModel()
                 {
                     Parameters = Parameters
                 };
                //    EquipmentCostGridItem item = new EquipmentCostGridItem();

                //    item.QueryDate = BeginDate + "～" + EndDate;
                //using (EDbEntities db = new EDbEntities())
                //{
                //    model.GridItem = (from x in db.Equipment
                //                      where x.UniqueID == Parameters.EquipmentUniqueID
                //                      select new GridItem
                //            {
                //                EquipmentID = x.ID,
                //                EquipmentName = x.Name,
                //                EquipmentMaintenanceOrganizationUniqueID = x.MaintenanceOrganizationUniqueID,
                //                EquipmentOrganizationUniqueID = x.OrganizationUniqueID,
                //                EquipmentSpecValueAgent = null,
                //                EquipmentSpecValueModel = null,
                //                EquipmentSpecValueFactoryNumber = null,

                //            }).FirstOrDefault();

                //    model.GridItem.EquipmentMaintenanceOrganizationUniqueID = OrganizationDataAccessor.GetOrganizationDescription(model.GridItem.EquipmentMaintenanceOrganizationUniqueID);
                //    model.GridItem.EquipmentOrganizationUniqueID = OrganizationDataAccessor.GetOrganizationDescription(model.GridItem.EquipmentOrganizationUniqueID);


                //    var EquipmentSpecValue = (from x in db.Equipment
                //                              join x1 in db.EquipmentSpecValue on x.UniqueID equals x1.EquipmentUniqueID
                //                              join x2 in db.EquipmentSpec on x1.SpecUniqueID equals x2.UniqueID
                //                              where x.UniqueID == Parameters.EquipmentUniqueID
                //                              select new
                //                              {
                //                                  x2.Description,
                //                                  x1.Value
                //                              }).ToList();  //獲取設備的出廠編號，型號，代理商


                //    model.GridItem.EquipmentSpecValueFactoryNumber = EquipmentSpecValue.Where(x => x.Description == Resources.Resource.FactoryNumber).Select(x => x.Value).FirstOrDefault();  //設置出廠編號的值
                //    model.GridItem.EquipmentSpecValueModel = EquipmentSpecValue.Where(x => x.Description == Resources.Resource.Model).Select(x => x.Value).FirstOrDefault();  //設置型號的值
                //    model.GridItem.EquipmentSpecValueAgent = EquipmentSpecValue.Where(x => x.Description == Resources.Resource.Agent).Select(x => x.Value).FirstOrDefault();  //設置出廠代理商的值

                //    var queryHour = (from x in db.RForm
                //                     join x1 in db.Equipment on x.EquipmentUniqueID equals x1.UniqueID
                //                     join x2 in db.RFormWorkingHour on x.UniqueID equals x2.RFormUniqueID
                //                     where x1.UniqueID == Parameters.EquipmentUniqueID
                //                     select new
                //                     {
                //                         x.MFormUniqueID,  //MFormUniqueID為空表示異常修復單，工時紀錄為修復工時；否則表示保養修復單，工時紀錄為保養工時
                //                         WorkingHour = x2.WorkingHour,  //由于FrontWorkingHour可能为空，如果为空则会导致这个属性为空，所以需要判断一下
                //                         //WorkingHour = x2.WorkingHour + (x2.FrontWorkingHour == null ? 0 : x2.FrontWorkingHour),  //由于FrontWorkingHour可能为空，如果为空则会导致这个属性为空，所以需要判断一下
                //                         x.CreateTime
                //                     }).AsQueryable();   //查詢工時



                //    DateTime dtmBeginDate = Convert.ToDateTime(Parameters.BeginDateString);
                //    DateTime dtmEndDate = Convert.ToDateTime(Parameters.EndDateString).AddDays(1);



                //    if (!string.IsNullOrEmpty(Parameters.BeginDate))
                //    {
                //        queryHour = queryHour.Where(x => x.CreateTime >= dtmBeginDate);
                //    }

                //    if (!string.IsNullOrEmpty(Parameters.EndDate))
                //    {
                //        queryHour = queryHour.Where(x => x.CreateTime < dtmEndDate);
                //    }

                //    var hourDataList = queryHour.ToList();

                //    model.GridItem.MaintenanceCostTotal = hourDataList.Where(x => !string.IsNullOrEmpty(x.MFormUniqueID)).Sum(x => x.WorkingHour).ToString();
                //    model.GridItem.RepairCostTotal = hourDataList.Where(x => string.IsNullOrEmpty(x.MFormUniqueID)).Sum(x => x.WorkingHour).ToString();


                //    var queryMaterial = (from x in db.RForm
                //                         join x2 in db.RFormMaterial on x.UniqueID equals x2.RFormUniqueID
                //                         join x3 in db.Material on x2.MaterialUniqueID equals x3.UniqueID
                //                         join x4 in db.MaterialSpecValue on x3.UniqueID equals x4.MaterialUniqueID
                //                         join x5 in db.MaterialSpec on x4.SpecUniqueID equals x5.UniqueID
                //                         where x.EquipmentUniqueID == Parameters.EquipmentUniqueID && x5.Description == Resources.Resource.Format
                //                         select new
                //                         {
                //                             x3.ID,//材料編號
                //                             x3.Name,//材料名稱
                //                             x4.Value, //材料規格
                //                             x2.Quantity   //更換數量
                //                         }).ToList();

                //    model.GridItem.EquipmentCostList = (from x in queryMaterial
                //                                        group x by new { x.ID, x.Name, x.Value } into g
                //                                        select new EquipmentCostGridViewModel
                //                                        {
                //                                            MaterialChageNumber = g.Sum(x => x.Quantity).ToString(),
                //                                            MaterialID = g.Key.ID,
                //                                            MaterialName = g.Key.Name,
                //                                            MaterialSpecValueMaterialStandard = g.Key.Value
                //                                        }).ToList();  //按照材料的id，name和規格進行分組，并計算數量

                //}
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

        public static ExcelExportModel Export(GridViewModel model, Define.EnumExcelVersion ExcelVersion)
        {
            RequestResult result = new RequestResult();


            try
            {
                
                  IWorkbook wk ;
                  IRow row;

                   //判断excel的版本
                  if (ExcelVersion == Define.EnumExcelVersion._2003)
                  {
                      wk = new HSSFWorkbook();
                  }
                  else
                  {
                      wk = new XSSFWorkbook();
                  }

                  ISheet sheet = wk.CreateSheet(Resources.Resource.EquipmentCost);
    

                  sheet.DefaultColumnWidth = 18;     //设置单元格长度
                  sheet.DefaultRowHeight = 400;      //设置单元格高度
                  sheet.SetColumnWidth(0, 4 * 256);
                 

                  ICellStyle titleCellStyle = wk.CreateCellStyle();  //标题的样式，显示边框，上下居中，字体加大
                  IFont titleFont = wk.CreateFont();
                  titleFont.FontName = "新細明體";
                  titleFont.FontHeightInPoints = 12;
                  titleCellStyle.BorderTop = BorderStyle.Thin;
                  titleCellStyle.BorderBottom = BorderStyle.Thin;
                  titleCellStyle.BorderLeft = BorderStyle.Thin;
                  titleCellStyle.BorderRight = BorderStyle.Thin;
                  titleCellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                  titleCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                  titleCellStyle.SetFont(titleFont);


                  ICellStyle cellStyle = wk.CreateCellStyle();  //一般数据的样式 ，显示边框
                  IFont font = wk.CreateFont();
                  font.FontHeightInPoints = 12;
                  cellStyle.BorderTop = BorderStyle.Thin;
                  cellStyle.BorderBottom = BorderStyle.Thin;
                  cellStyle.BorderLeft = BorderStyle.Thin;
                  cellStyle.BorderRight = BorderStyle.Thin;
                  cellStyle.SetFont(font);

                  CreateAndSetCellStyle(sheet, 0, titleCellStyle);
                  sheet.GetRow(0).GetCell(0).SetCellValue(Resources.Resource.EquipmentCost);
                  sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 5));
                  sheet.GetRow(0).GetCell(6).SetCellValue(DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTime.Now));

                  CreateAndSetCellStyle(sheet, 1, cellStyle);
                  IRow row2 = sheet.GetRow(1);
                  row2.GetCell(1).SetCellValue(Resources.Resource.EquipmentID);
                  row2.GetCell(4).SetCellValue(Resources.Resource.EquipmentName);

                  row2.GetCell(2).SetCellValue(model.GridItem.EquipmentID);
                  sheet.AddMergedRegion(new CellRangeAddress(1, 1, 2, 3));
                  row2.GetCell(5).SetCellValue(model.GridItem.EquipmentName);
                  sheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 6));

                  CreateAndSetCellStyle(sheet, 2, cellStyle);
                  IRow row3 = sheet.GetRow(2);
                  row3.GetCell(1).SetCellValue(Resources.Resource.MaintenanceOrganization);
                  row3.GetCell(4).SetCellValue(Resources.Resource.OwnOrganization);

                  row3.GetCell(2).SetCellValue(model.GridItem.EquipmentMaintenanceOrganizationUniqueID);
                  sheet.AddMergedRegion(new CellRangeAddress(2, 2, 2, 3));
                  row3.GetCell(5).SetCellValue(model.GridItem.EquipmentOrganizationUniqueID);
                  sheet.AddMergedRegion(new CellRangeAddress(2, 2, 5, 6));

                  CreateAndSetCellStyle(sheet, 3, cellStyle);
                  IRow row4 = sheet.GetRow(3);
                  row4.GetCell(1).SetCellValue(Resources.Resource.Agent);
                  row4.GetCell(4).SetCellValue(Resources.Resource.FactoryNumber);

                  row4.GetCell(2).SetCellValue(model.GridItem.EquipmentSpecValueAgent);
                  sheet.AddMergedRegion(new CellRangeAddress(3, 3, 2, 3));
                  row4.GetCell(5).SetCellValue(model.GridItem.EquipmentSpecValueFactoryNumber);
                  sheet.AddMergedRegion(new CellRangeAddress(3, 3, 5, 6));

                  CreateAndSetCellStyle(sheet, 4, cellStyle);
                  IRow row5 = sheet.GetRow(4);
                  row5.GetCell(1).SetCellValue(Resources.Resource.Model);
                  row5.GetCell(2).SetCellValue(model.GridItem.EquipmentSpecValueModel);
                  sheet.AddMergedRegion(new CellRangeAddress(4, 4, 2, 6));

                  CreateAndSetCellStyle(sheet, 6, cellStyle);
                  IRow row6 = sheet.GetRow(6);
                  row6.GetCell(1).SetCellValue(Resources.Resource.DurationTime);
                  row6.GetCell(2).SetCellValue(model.Parameters.BeginDateString + "~" + model.Parameters.EndDateString);

                  CreateAndSetCellStyle(sheet, 7, cellStyle);
                  IRow row7 = sheet.GetRow(7);
                  row7.GetCell(0).SetCellValue(Resources.Resource.MaintenanceCostTimeTotal);
                  sheet.AddMergedRegion(new CellRangeAddress(7, 7, 0, 1));
                  row7.GetCell(4).SetCellValue(Resources.Resource.RepairCostTimeTotal);

                  row7.GetCell(2).SetCellValue(model.GridItem.MaintenanceCostTotal);
                  sheet.AddMergedRegion(new CellRangeAddress(7, 7, 2, 3));
                  row7.GetCell(5).SetCellValue(model.GridItem.RepairCostTotal);
                  sheet.AddMergedRegion(new CellRangeAddress(6, 6, 5, 6));
                 

                  CreateAndSetCellStyle(sheet, 8, cellStyle);
                  IRow row8= sheet.GetRow(8);
                  row8.GetCell(1).SetCellValue(Resources.Resource.MaterialID);
                  row8.GetCell(2).SetCellValue(Resources.Resource.MaterialName);
                  row8.GetCell(3).SetCellValue(Resources.Resource.MaterialSpec);
                  row8.GetCell(4).SetCellValue(Resources.Resource.MaterialPrice);
                  row8.GetCell(5).SetCellValue(Resources.Resource.ReplaceQTY);
                  row8.GetCell(6).SetCellValue(Resources.Resource.MaterialTotalPrice);

                  var rowIndex = 9;
                  foreach (var EquipmentDetail in model.GridItem.EquipmentCostList)
                  {
                      CreateAndSetCellStyle(sheet, rowIndex, cellStyle);
                      row = sheet.GetRow(rowIndex);
                      row.GetCell(0).SetCellValue(rowIndex-8);
                      row.GetCell(1).SetCellValue(EquipmentDetail.MaterialID);
                      row.GetCell(2).SetCellValue(EquipmentDetail.MaterialName);
                      row.GetCell(3).SetCellValue(EquipmentDetail.MaterialSpecValueMaterialStandard);
                      row.GetCell(4).SetCellValue(EquipmentDetail.MaterialPrice);
                      row.GetCell(5).SetCellValue(EquipmentDetail.MaterialChageNumber);
                      row.GetCell(6).SetCellValue(EquipmentDetail.MaterialTotalPrice);

                      rowIndex++;
                  }

                  CreateAndSetCellStyle(sheet, rowIndex, cellStyle);
                  IRow rowLast = sheet.GetRow(rowIndex);
                  rowLast.GetCell(1).SetCellValue(Resources.Resource.Total);
                  sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 1, 5));
                  rowLast.GetCell(6).SetCellValue(model.GridItem.TotalPrice);


                  var modelExcel = new ExcelExportModel(Resources.Resource.EquipmentCost, ExcelVersion);

                  using (FileStream fs = System.IO.File.OpenWrite(modelExcel.FullFileName)) //打开一个xls文件，如果没有则自行创建，如果存在myxls.xls文件则在创建是不要打开该文件！
                  {
                      wk.Write(fs);   //向打开的这个xls文件中写入mySheet表并保存。
                  }

                  byte[] buff = null;

                  using (var fs = System.IO.File.OpenRead(modelExcel.FullFileName))
                  {
                      using (BinaryReader br = new BinaryReader(fs))
                      {
                          long numBytes = new FileInfo(modelExcel.FullFileName).Length;

                          buff = br.ReadBytes((int)numBytes);

                          br.Close();
                      }

                      fs.Close();
                  }

                  modelExcel.Data = buff;

                  return modelExcel;
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);

                return null;
            }
        }

        /// <summary>
        /// 创建单元格，并为单元格赋予相应的样式
        /// </summary>
        /// <param name="Sheet"></param>
        /// <param name="RowIndex"></param>
        /// <param name="CellStyle"></param>
        protected static void CreateAndSetCellStyle(ISheet Sheet, int RowIndex, ICellStyle CellStyle)
        {
            var row = Sheet.CreateRow(RowIndex);
            for (int i = 0; i < 7; i++)
            {
                row.CreateCell(i).CellStyle = CellStyle;
            }
        }






    }
}



