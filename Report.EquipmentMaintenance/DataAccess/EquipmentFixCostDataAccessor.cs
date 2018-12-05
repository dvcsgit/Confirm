using DataAccess;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.Authenticated;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using Report.EquipmentMaintenance.Models.EquipmentFixCost;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace Report.EquipmentMaintenance.DataAccess
{
   public  class EquipmentFixCostDataAccessor
    {
       public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var duration = (Parameters.EndDate.Value - Parameters.BeginDate.Value).TotalHours;

                var model = new GridViewModel()
                {
                    Parameters = Parameters,
                    Duration = duration
                };

                var downSteamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                var organizationList = Account.QueryableOrganizationUniqueIDList.Intersect(downSteamOrganizationList);

                using (EDbEntities db = new EDbEntities())
                {
                    var equipmentList = db.Equipment.Where(x => organizationList.Contains(x.OrganizationUniqueID)).ToList();

                    foreach (var e in equipmentList)
                    {
                        var repairFormList = db.RForm.Where(x => x.EquipmentUniqueID == e.UniqueID && x.Status == "8" && x.ClosedTime.HasValue && DateTime.Compare(x.CreateTime, Parameters.BeginDate.Value) >= 0 && DateTime.Compare(x.CreateTime, Parameters.EndDate.Value) <= 0).ToList();

                        if (repairFormList.Count > 0)
                        {
                            var item = new GridItem()
                            {
                                OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(e.OrganizationUniqueID),
                                EquipmentID = e.ID,
                                EquipmentName = e.Name,
                                MaintenanceOrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(e.MaintenanceOrganizationUniqueID),
                                Duration = duration
                            };

                            foreach (var repairForm in repairFormList)
                            {
                                item.RepairFormList.Add(new RepairFormModel()
                                {
                                    CreateTime = repairForm.CreateTime,
                                    ClosedTime = repairForm.ClosedTime.Value
                                });
                            }

                            model.ItemList.Add(item);
                        }
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

       public static RequestResult Export(List<GridItem> Model, Define.EnumExcelVersion ExcelVersion)
       {
           RequestResult result = new RequestResult();

           try
           {
               using (ExcelHelper helper = new ExcelHelper(string.Format("{0}_{1}({2})", "設備異常成本統計表", Resources.Resource.ExportTime, DateTimeHelper.DateTime2DateTimeString(DateTime.Now)), ExcelVersion))
               {

                   helper.CreateSheet("設備異常成本統計", Model);

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

       //public static ExcelExportModel Export(GridViewModel model, Define.EnumExcelVersion ExcelVersion)
       //{

       //    //try
       //    //{
       //    //    IWorkbook wk;

       //    //    //判断excel的版本
       //    //    if (ExcelVersion == Define.EnumExcelVersion._2003)
       //    //    {
       //    //        wk = new HSSFWorkbook();
       //    //    }
       //    //    else
       //    //    {
       //    //        wk = new XSSFWorkbook();
       //    //    }

       //    //    ISheet sheet = wk.CreateSheet(Resources.Resource.EquipmentFixCost);

       //    //    sheet.DefaultColumnWidth = 9;     //设置单元格长度
       //    //    sheet.DefaultRowHeight = 400;      //设置单元格高度
       //    //    sheet.SetColumnWidth(1, 25 * 256);
       //    //    sheet.SetColumnWidth(2, 25 * 256);
       //    //    sheet.SetColumnWidth(3, 25 * 256);


       //    //    ICellStyle cellStyle = wk.CreateCellStyle();  //一般数据的样式 ，显示边框
       //    //    IFont font = wk.CreateFont();
       //    //    font.FontHeightInPoints = 12;
       //    //    cellStyle.BorderTop = BorderStyle.Thin;
       //    //    cellStyle.BorderBottom = BorderStyle.Thin;
       //    //    cellStyle.BorderLeft = BorderStyle.Thin;
       //    //    cellStyle.BorderRight = BorderStyle.Thin;
       //    //    cellStyle.SetFont(font);

       //    //    ICellStyle cellSingleStyle = wk.CreateCellStyle();  //一般数据的样式 
       //    //    IFont singleFont = wk.CreateFont();
       //    //    singleFont.FontHeightInPoints = 12;
       //    //    cellSingleStyle.SetFont(singleFont);


       //    //    ICellStyle titleCellStyle = wk.CreateCellStyle();  //标题的样式，显示边框，上下居中，字体加大
       //    //    IFont titleFont = wk.CreateFont();
       //    //    titleFont.FontName = "新細明體";
       //    //    titleFont.FontHeightInPoints = 12;
       //    //    titleCellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
       //    //    titleCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
       //    //    titleCellStyle.SetFont(titleFont);

       //    //    sheet.CreateRow(0).CreateCell(0).SetCellValue(Resources.Resource.EquipmentFixCost);
       //    //    sheet.GetRow(0).GetCell(0).CellStyle = titleCellStyle;
       //    //    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 4));

       //    //    IRow row2 = sheet.CreateRow(1);
       //    //    row2.CreateCell(0).SetCellValue(Resources.Resource.DurationTime);
       //    //    row2.CreateCell(2).SetCellValue(Resources.Resource.PrintDate);

       //    //    row2.CreateCell(1).SetCellValue(model.Parameters.BeginDateString + "~" + model.Parameters.EndDateString);
       //    //    row2.CreateCell(3).SetCellValue(DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTime.Now));

       //    //    row2.GetCell(0).CellStyle = cellSingleStyle;
       //    //    row2.GetCell(1).CellStyle = cellSingleStyle;
       //    //    row2.GetCell(2).CellStyle = cellSingleStyle;
       //    //    row2.GetCell(3).CellStyle = cellSingleStyle;

       //    //    int rTypeCount=model.RFormTypeList.Count(); //获取修复单类型
       //    //    int colIndex = 0;

       //    //    CreateAndSetCellStyle(sheet, 2, cellStyle, rTypeCount);
       //    //    IRow row = sheet.GetRow(2);
       //    //    row.GetCell(0).SetCellValue(Resources.Resource.EquipmentID);
       //    //    row.GetCell(1).SetCellValue(Resources.Resource.EquipmentName);
       //    //    row.GetCell(2).SetCellValue(Resources.Resource.MaintenanceOrganization);
       //    //    row.GetCell(3).SetCellValue(Resources.Resource.OwnOrganization);
       //    //    colIndex = 4;
       //    //    foreach(var item in model.RFormTypeList)
       //    //    {
       //    //        row.GetCell(colIndex++).SetCellValue(item.RFormTypViewDescription);
       //    //      //  row.GetCell(colIndex++).SetCellValue(item.RFormTypViewClassDescription);
       //    //    }
       //    //    row.GetCell(colIndex++).SetCellValue(Resources.Resource.NumberCount);
       //    //    row.GetCell(colIndex++).SetCellValue("MTBF");

       //    //    var rowIndex = 3;

       //    //    foreach (var item in model.GridItem)
       //    //    {
       //    //        CreateAndSetCellStyle(sheet, rowIndex, cellStyle, rTypeCount);
       //    //        row = sheet.GetRow(rowIndex);
       //    //        row.GetCell(0).SetCellValue(item.EquipmentID);
       //    //        row.GetCell(1).SetCellValue(item.EquipmentName);
       //    //        row.GetCell(2).SetCellValue(item.EquipmentMaintenanceOrganizationUniqueID);
       //    //        row.GetCell(3).SetCellValue(item.EquipmentOrganizationUniqueID);
       //    //        colIndex = 4;
       //    //        foreach (var rFormWorkingHourModel in item.RFormWorkingHourModelList)
       //    //        {
       //    //            row.GetCell(colIndex++).SetCellValue(rFormWorkingHourModel.Count);
       //    //            //row.GetCell(colIndex++).SetCellValue(rFormWorkingHourModel.Hour.ToString());
       //    //        }
       //    //        row.GetCell(colIndex++).SetCellValue(item.TotalCount);
       //    //        row.GetCell(colIndex++).SetCellValue(item.MTBF);
       //    //        rowIndex++;
       //    //    }

       //    //    CreateAndSetCellStyle(sheet, rowIndex, cellStyle, rTypeCount);
       //    //    IRow rowLast = sheet.GetRow(rowIndex);
       //    //    rowLast.GetCell(0).SetCellValue(Resources.Resource.Total);
       //    //    rowLast.GetCell(0).CellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
       //    //    rowLast.GetCell(0).CellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;


       //    //    sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 0, 3));
       //    //    colIndex = 4;
       //    //    foreach (var item in model.RFormTypeSumModelList)
       //    //    {
       //    //        rowLast.GetCell(colIndex++).SetCellValue(item.TotalCount);
       //    //       // rowLast.GetCell(colIndex++).SetCellValue(item.TotalSumHour);
       //    //    }

       //    //    rowIndex = rowIndex + 2;

       //    //    foreach(var item in model.RFormTypeList)
       //    //    {
       //    //      row = sheet.CreateRow(rowIndex);
       //    //      row.CreateCell(0).SetCellValue(item.RFormTypViewDescription);
       //    //      row.CreateCell(1).SetCellValue(item.RFormTypeDescription);
       //    //      rowIndex++;
       //    //    }


       //    //    var modelExcel = new ExcelExportModel(Resources.Resource.EquipmentFixCost, ExcelVersion);

       //    //    using (FileStream fs = System.IO.File.OpenWrite(modelExcel.FullFileName)) //打开一个xls文件，如果没有则自行创建，如果存在myxls.xls文件则在创建是不要打开该文件！
       //    //    {
       //    //        wk.Write(fs);   //向打开的这个xls文件中写入mySheet表并保存。
       //    //    }

       //    //    byte[] buff = null;

       //    //    using (var fs = System.IO.File.OpenRead(modelExcel.FullFileName))
       //    //    {
       //    //        using (BinaryReader br = new BinaryReader(fs))
       //    //        {
       //    //            long numBytes = new FileInfo(modelExcel.FullFileName).Length;

       //    //            buff = br.ReadBytes((int)numBytes);

       //    //            br.Close();
       //    //        }

       //    //        fs.Close();
       //    //    }

       //    //    modelExcel.Data = buff;

       //    //    return modelExcel;


       //    //}
       //    //catch (Exception ex)
       //    //{

       //    //    Logger.Log(MethodBase.GetCurrentMethod(), ex);

       //    //    return null;

       //    //}


       //}


       /// <summary>
       /// 创建单元格，并为单元格赋予相应的样式
       /// </summary>
       /// <param name="Sheet"></param>
       /// <param name="RowIndex"></param>
       /// <param name="CellStyle"></param>
       protected static void CreateAndSetCellStyle(ISheet Sheet, int RowIndex, ICellStyle CellStyle,int RTypeCount)
       {
           var row = Sheet.CreateRow(RowIndex);
           for (int i = 0; i < 6 + RTypeCount*1; i++)
           {
               row.CreateCell(i).CellStyle = CellStyle;
           }
       }


    }
}
