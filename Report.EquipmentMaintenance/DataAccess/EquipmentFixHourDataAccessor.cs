using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if ORACLE
using DbEntity.ORACLE;
using DbEntity.ORACLE.EquipmentMaintenance;
#else
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using Utility.Models;
using Report.EquipmentMaintenance.Models.EquipmentFixHour;
using DataAccess;
using System.Reflection;
using Utility;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;
using System.IO;
#endif

namespace Report.EquipmentMaintenance.DataAccess
{
   public  class EquipmentFixHourDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();
            try
            {
                //var model = new GridViewModel()
                //{
                //    Parameters = Parameters
                //};

                //var downSteamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);
             
                //using (EDbEntities db = new EDbEntities())
                //{

                //    var rFormTypeList = db.RFormType.Select(x => new RFormTypeModel { RFormTypeUniqueID = x.UniqueID, RFormTypeDescription = x.Description }).ToList();

                //    int startA = 65;   //A的ASCII碼為65
                //    for (int i = 0; i < rFormTypeList.Count; i++)
                //    {
                //        char character = Convert.ToChar(startA + i);
                //        rFormTypeList[i].RFormTypViewDescription = character + Resources.Resource.Type;
                //        rFormTypeList[i].RFormTypViewClassDescription = character + Resources.Resource.TypeHour;
                //    }

                //    model.RFormTypeList = rFormTypeList;

                //    DateTime dtmBeginDate = Convert.ToDateTime(Parameters.BeginDateString);
                //    DateTime dtmEndDate = Convert.ToDateTime(Parameters.EndDateString).AddDays(1);

                //    var dataResult = (from rForm in db.RForm
                //                      join rFormType in db.RFormType on rForm.RFormTypeUniqueID equals rFormType.UniqueID
                //                      join equipment in db.Equipment on rForm.EquipmentUniqueID equals equipment.UniqueID
                //                      join rFormWorkingHour in db.RFormWorkingHour on rForm.UniqueID equals rFormWorkingHour.RFormUniqueID into JoinedRFormWorkHour
                //                      from x in JoinedRFormWorkHour.DefaultIfEmpty()
                //                      where downSteamOrganizationList.Contains(rForm.OrganizationUniqueID)
                //                      && rForm.CreateTime >= dtmBeginDate && rForm.CreateTime < dtmEndDate
                //                      && (rForm.MFormUniqueID == "" || rForm.MFormUniqueID == null)
                //                      select new
                //                      {
                //                          equipment.ID,  //設備編號
                //                          equipment.Name,  //設備名稱
                //                          rForm.OrganizationUniqueID, //保管單位
                //                          rForm.MaintenanceOrganizationUniqueID,//保養單位
                //                          rForm.RFormTypeUniqueID,  //修復單類型
                //                          WorkingHour = x.WorkingHour,  //工時
                //                          //FrontWorkingHour = x.FrontWorkingHour == null ? 0 : x.FrontWorkingHour
                //                      }).ToList();


                //    foreach (var rFormType in rFormTypeList)
                //    {
                //        RFormTypeSumModel rFormTypeSumModel = new RFormTypeSumModel();
                //        rFormTypeSumModel.TotalCount = dataResult.Where(x => x.RFormTypeUniqueID == rFormType.RFormTypeUniqueID).Count().ToString();
                //        rFormTypeSumModel.TotalSumHour = dataResult.Where(x => x.RFormTypeUniqueID == rFormType.RFormTypeUniqueID).Sum(x => (x.WorkingHour)).ToString();
                //        //rFormTypeSumModel.TotalSumHour = dataResult.Where(x => x.RFormTypeUniqueID == rFormType.RFormTypeUniqueID).Sum(x => (x.FrontWorkingHour + x.WorkingHour)).ToString();
                //        model.RFormTypeSumModelList.Add(rFormTypeSumModel);

                //    }

                //    var groupResultList = (from x in dataResult group x by new { x.ID, x.Name, x.OrganizationUniqueID, x.MaintenanceOrganizationUniqueID } into g select new { g, g.Key }).ToList();

                //    foreach (var groupResult in groupResultList)
                //    {
                //        var gridItem = new GridItem();
                //        gridItem.EquipmentID = groupResult.Key.ID;  //设备的ID
                //        gridItem.EquipmentName = groupResult.Key.Name;  //设备的名称
                //        gridItem.EquipmentOrganizationUniqueID = OrganizationDataAccessor.GetOrganizationDescription(groupResult.Key.OrganizationUniqueID);//保管单位
                //        gridItem.EquipmentMaintenanceOrganizationUniqueID = OrganizationDataAccessor.GetOrganizationDescription(groupResult.Key.MaintenanceOrganizationUniqueID); //保养单位
                //        gridItem.TotalCount = groupResult.g.Count();

                //        foreach (var rFormType in rFormTypeList)
                //        {
                //            var rFormWorkingHourModel = new RFormWorkingHourModel();
                //            rFormWorkingHourModel.RFormTypeUniqueID = rFormType.RFormTypeUniqueID;
                //            rFormWorkingHourModel.Count = groupResult.g.Where(x => x.RFormTypeUniqueID == rFormType.RFormTypeUniqueID).Count().ToString();
                //            rFormWorkingHourModel.Hour = groupResult.g.Where(x => x.RFormTypeUniqueID == rFormType.RFormTypeUniqueID).Sum(x => (x.WorkingHour));
                //            //rFormWorkingHourModel.Hour = groupResult.g.Where(x => x.RFormTypeUniqueID == rFormType.RFormTypeUniqueID).Sum(x => (x.WorkingHour + x.FrontWorkingHour));
                //            gridItem.RFormWorkingHourModelList.Add(rFormWorkingHourModel);
                //        }
                //        model.GridItem.Add(gridItem);
                //    }
                //}
                //result.ReturnData(model);
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
            try
            {
                IWorkbook wk;

                //判断excel的版本
                if (ExcelVersion == Define.EnumExcelVersion._2003)
                {
                    wk = new HSSFWorkbook();
                }
                else
                {
                    wk = new XSSFWorkbook();
                }

                ISheet sheet = wk.CreateSheet(Resources.Resource.EquipmentFixHour);

                sheet.DefaultColumnWidth = 9;     //设置单元格长度
                sheet.DefaultRowHeight = 400;      //设置单元格高度
                sheet.SetColumnWidth(1, 25 * 256);
                sheet.SetColumnWidth(2, 25 * 256);
                sheet.SetColumnWidth(3, 25 * 256);


                ICellStyle cellStyle = wk.CreateCellStyle();  //一般数据的样式 ，显示边框
                IFont font = wk.CreateFont();
                font.FontHeightInPoints = 12;
                cellStyle.BorderTop = BorderStyle.Thin;
                cellStyle.BorderBottom = BorderStyle.Thin;
                cellStyle.BorderLeft = BorderStyle.Thin;
                cellStyle.BorderRight = BorderStyle.Thin;
                cellStyle.SetFont(font);

                ICellStyle cellSingleStyle = wk.CreateCellStyle();  //一般数据的样式 
                IFont singleFont = wk.CreateFont();
                singleFont.FontHeightInPoints = 12;
                cellSingleStyle.SetFont(singleFont);


                ICellStyle titleCellStyle = wk.CreateCellStyle();  //标题的样式，显示边框，上下居中，字体加大
                IFont titleFont = wk.CreateFont();
                titleFont.FontName = "新細明體";
                titleFont.FontHeightInPoints = 12;
                titleCellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                titleCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                titleCellStyle.SetFont(titleFont);

                sheet.CreateRow(0).CreateCell(0).SetCellValue(Resources.Resource.EquipmentFixHour);
                sheet.GetRow(0).GetCell(0).CellStyle = titleCellStyle;
                sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 4));

                IRow row2 = sheet.CreateRow(1);
                row2.CreateCell(0).SetCellValue(Resources.Resource.DurationTime);
                row2.CreateCell(2).SetCellValue(Resources.Resource.PrintDate);

                row2.CreateCell(1).SetCellValue(model.Parameters.BeginDateString + "~" + model.Parameters.EndDateString);
                row2.CreateCell(3).SetCellValue(DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTime.Now));

                row2.GetCell(0).CellStyle = cellSingleStyle;
                row2.GetCell(1).CellStyle = cellSingleStyle;
                row2.GetCell(2).CellStyle = cellSingleStyle;
                row2.GetCell(3).CellStyle = cellSingleStyle;

                int rTypeCount = model.RFormTypeList.Count(); //获取修复单类型
                int colIndex = 0;

                CreateAndSetCellStyle(sheet, 2, cellStyle, rTypeCount);
                IRow row = sheet.GetRow(2);
                row.GetCell(0).SetCellValue(Resources.Resource.EquipmentID);
                row.GetCell(1).SetCellValue(Resources.Resource.EquipmentName);
                row.GetCell(2).SetCellValue(Resources.Resource.MaintenanceOrganization);
                row.GetCell(3).SetCellValue(Resources.Resource.OwnOrganization);
                colIndex = 4;
                foreach (var item in model.RFormTypeList)
                {
                    //row.GetCell(colIndex++).SetCellValue(item.RFormTypViewDescription);
                    row.GetCell(colIndex++).SetCellValue(item.RFormTypViewClassDescription);
                }
                row.GetCell(colIndex++).SetCellValue(Resources.Resource.HourCount);
                row.GetCell(colIndex++).SetCellValue("MTTR");

                var rowIndex = 3;

                foreach (var item in model.GridItem)
                {
                    CreateAndSetCellStyle(sheet, rowIndex, cellStyle, rTypeCount);
                    row = sheet.GetRow(rowIndex);
                    row.GetCell(0).SetCellValue(item.EquipmentID);
                    row.GetCell(1).SetCellValue(item.EquipmentName);
                    row.GetCell(2).SetCellValue(item.EquipmentMaintenanceOrganizationUniqueID);
                    row.GetCell(3).SetCellValue(item.EquipmentOrganizationUniqueID);
                    colIndex = 4;
                    foreach (var rFormWorkingHourModel in item.RFormWorkingHourModelList)
                    {
                        //row.GetCell(colIndex++).SetCellValue(rFormWorkingHourModel.Count);
                        row.GetCell(colIndex++).SetCellValue(rFormWorkingHourModel.Hour.ToString());
                    }
                    row.GetCell(colIndex++).SetCellValue(item.TotalSumHour);
                    row.GetCell(colIndex++).SetCellValue(item.MTTR);
                    rowIndex++;
                }

                CreateAndSetCellStyle(sheet, rowIndex, cellStyle, rTypeCount);
                IRow rowLast = sheet.GetRow(rowIndex);
                rowLast.GetCell(0).SetCellValue(Resources.Resource.Total);
                rowLast.GetCell(0).CellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                rowLast.GetCell(0).CellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;


                sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 0, 3));
                colIndex = 4;
                foreach (var item in model.RFormTypeSumModelList)
                {
                    //rowLast.GetCell(colIndex++).SetCellValue(item.TotalCount);
                    rowLast.GetCell(colIndex++).SetCellValue(item.TotalSumHour);
                }

                rowIndex = rowIndex + 2;

                foreach (var item in model.RFormTypeList)
                {
                    row = sheet.CreateRow(rowIndex);
                    row.CreateCell(0).SetCellValue(item.RFormTypViewDescription);
                    row.CreateCell(1).SetCellValue(item.RFormTypeDescription);
                    rowIndex++;
                }


                var modelExcel = new ExcelExportModel(Resources.Resource.EquipmentFixHour, ExcelVersion);

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
        protected static void CreateAndSetCellStyle(ISheet Sheet, int RowIndex, ICellStyle CellStyle, int RTypeCount)
        {
            var row = Sheet.CreateRow(RowIndex);
            for (int i = 0; i < 6 + RTypeCount * 1; i++)
            {
                row.CreateCell(i).CellStyle = CellStyle;
            }
        }
    }
}
