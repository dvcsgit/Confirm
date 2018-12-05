using DataAccess;
using DbEntity.MSSQL.EquipmentMaintenance;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using Report.EquipmentMaintenance.Models.EquipmentDetail;
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
   public class EquipmentDetailDataAccessor
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

               List<EquipmentDetailGridViewModel> equipmentDetaillist=new List<EquipmentDetailGridViewModel>(); 

                using (EDbEntities db = new EDbEntities())
               {
                   model.GridItem = (from x in db.Equipment
                           where x.UniqueID == Parameters.EquipmentUniqueID
                           select new GridItem
                           {
                               EquipmentID = x.ID,
                               EquipmentName = x.Name,
                               EquipmentMaintenanceOrganizationUniqueID = x.MaintenanceOrganizationUniqueID,
                               EquipmentOrganizationUniqueID = x.OrganizationUniqueID,
                               EquipmentSpecValueAgent = null,
                               EquipmentSpecValueModel = null,
                               EquipmentSpecValueFactoryNumber = null,
                           }).FirstOrDefault();

                    var EquipmentSpecValue = (from x in db.Equipment
                                             join x1 in db.EquipmentSpecValue on x.UniqueID equals x1.EquipmentUniqueID
                                             join x2 in db.EquipmentSpec on x1.SpecUniqueID equals x2.UniqueID
                                             where x.UniqueID ==  Parameters.EquipmentUniqueID
                                             select new
                                             {
                                                 x2.Description,
                                                 x1.Value
                                             }).ToList();  //獲取設備的出廠編號，型號，代理商


                   model.GridItem.EquipmentSpecValueFactoryNumber = EquipmentSpecValue.Where(x => x.Description == Resources.Resource.FactoryNumber).Select(x => x.Value).FirstOrDefault();  //設置出廠編號的值
                   model.GridItem.EquipmentSpecValueModel = EquipmentSpecValue.Where(x => x.Description == Resources.Resource.Model).Select(x => x.Value).FirstOrDefault();  //設置型號的值
                   model.GridItem.EquipmentSpecValueAgent = EquipmentSpecValue.Where(x => x.Description == Resources.Resource.Agent).Select(x => x.Value).FirstOrDefault();  //設置出廠代理商的值


                   model.GridItem.EquipmentDetailList = (from x in db.Equipment
                                                         join x1 in db.EquipmentPart on x.UniqueID equals x1.EquipmentUniqueID
                                                         join s1 in db.EquipmentMaterial on x1.UniqueID equals s1.PartUniqueID into g1
                                                         from x2 in g1.DefaultIfEmpty()
                                                         join s2 in db.Material on x2.MaterialUniqueID equals s2.UniqueID into g2
                                                         from x3 in g2.DefaultIfEmpty()
                                                         join s3 in db.MaterialSpecValue on x3.UniqueID equals s3.MaterialUniqueID into g3
                                                         from x4 in g3.DefaultIfEmpty()
                                                         join s4 in db.MaterialSpec.Where(s => s.Description == Resources.Resource.Format) on x4.SpecUniqueID equals s4.UniqueID into g4
                                                         from x5 in g4.DefaultIfEmpty()
                                                         where x.UniqueID == Parameters.EquipmentUniqueID 
                                                         orderby x1.Description, x3.Name, x5.Description
                                                         select new EquipmentDetailGridViewModel
                                                         {
                                                             EquipmentPartDescription = x1.Description,
                                                             MaterialID = x3.ID,
                                                             MaterialName = x3.Name,
                                                             MaterialSpecValueMaterialStandard = x4.Value,
                                                             EquipmentMaterialQTY = x2.Quantity == null ? 0 : x2.Quantity
                                                         }).ToList();

                    model.GridItem.EquipmentMaintenanceOrganizationUniqueID =OrganizationDataAccessor.GetOrganizationDescription(model.GridItem.EquipmentMaintenanceOrganizationUniqueID);  //保養單位
                    model.GridItem.EquipmentOrganizationUniqueID =OrganizationDataAccessor.GetOrganizationDescription(model.GridItem.EquipmentOrganizationUniqueID);  //保養單位;   //保管單位

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


          public static ExcelExportModel Export(QueryParameters Parameters)
          {

              RequestResult result = new RequestResult();
              try
              {
                   result = Query(Parameters);

                  var model = result.Data as GridViewModel;

                  IWorkbook wk ;
                  IRow row;

                   //判断excel的版本
                  if (Parameters.ExcelVersion == Define.EnumExcelVersion._2003)
                  {
                      wk = new HSSFWorkbook();
                  }
                  else
                  {
                      wk = new XSSFWorkbook();
                  }

                  ISheet sheet = wk.CreateSheet(Resources.Resource.EquipmentDetail);

                  sheet.DefaultColumnWidth = 18;     //设置单元格长度
                  sheet.DefaultRowHeight = 400;      //设置单元格高度


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
                  sheet.GetRow(0).GetCell(0).SetCellValue(Resources.Resource.EquipmentDetail);
                  sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 4));
                  sheet.GetRow(0).GetCell(5).SetCellValue(DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTime.Now));

                  CreateAndSetCellStyle(sheet, 1, cellStyle);
                  IRow row2 = sheet.GetRow(1);
                  row2.GetCell(0).SetCellValue(Resources.Resource.EquipmentID);
                  row2.GetCell(3).SetCellValue(Resources.Resource.EquipmentName);


                  row2.GetCell(1).SetCellValue(model.GridItem.EquipmentID);
                  sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 2));
                  row2.GetCell(4).SetCellValue(model.GridItem.EquipmentName);
                  sheet.AddMergedRegion(new CellRangeAddress(1, 1, 4, 5));

                  CreateAndSetCellStyle(sheet, 2, cellStyle);
                  IRow row3 = sheet.GetRow(2);
                  row3.GetCell(0).SetCellValue(Resources.Resource.MaintenanceOrganization);
                  row3.GetCell(3).SetCellValue(Resources.Resource.OwnOrganization);

                  row3.GetCell(1).SetCellValue(model.GridItem.EquipmentMaintenanceOrganizationUniqueID);
                  sheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 2));
                  row3.GetCell(4).SetCellValue(model.GridItem.EquipmentOrganizationUniqueID);
                  sheet.AddMergedRegion(new CellRangeAddress(2, 2, 4, 5));

                  CreateAndSetCellStyle(sheet, 3, cellStyle);
                  IRow row4 = sheet.GetRow(3);
                  row4.GetCell(0).SetCellValue(Resources.Resource.Agent);
                  row4.GetCell(3).SetCellValue(Resources.Resource.FactoryNumber);

                  row4.GetCell(1).SetCellValue(model.GridItem.EquipmentSpecValueAgent);
                  sheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 2));
                  row4.GetCell(4).SetCellValue(model.GridItem.EquipmentSpecValueFactoryNumber);
                  sheet.AddMergedRegion(new CellRangeAddress(3, 3, 4, 5));

                  CreateAndSetCellStyle(sheet, 4, cellStyle);
                  IRow row5 = sheet.GetRow(4);
                  row5.GetCell(0).SetCellValue(Resources.Resource.Model);
                  row5.GetCell(1).SetCellValue(model.GridItem.EquipmentSpecValueModel);
                  sheet.AddMergedRegion(new CellRangeAddress(4, 4, 1, 5));


                  CreateAndSetCellStyle(sheet, 6, cellStyle);
                  IRow row6= sheet.GetRow(6);
                  row6.GetCell(0).SetCellValue(Resources.Resource.EquipmentPart);
                  row6.GetCell(1).SetCellValue(Resources.Resource.MaterialID);
                  sheet.AddMergedRegion(new CellRangeAddress(6, 6, 3, 4));
                  row6.GetCell(2).SetCellValue(Resources.Resource.MaterialName);
                  row6.GetCell(3).SetCellValue(Resources.Resource.MaterialSpec);
                  row6.GetCell(5).SetCellValue(Resources.Resource.QTY);

                  var rowIndex = 7;
                  foreach (var EquipmentDetail in model.GridItem.EquipmentDetailList)
                  {
                      CreateAndSetCellStyle(sheet, rowIndex, cellStyle);
                      row = sheet.GetRow(rowIndex);
                      row.GetCell(0).SetCellValue(EquipmentDetail.EquipmentPartDescription);
                      row.GetCell(1).SetCellValue(EquipmentDetail.MaterialID);
                      row.GetCell(2).SetCellValue(EquipmentDetail.MaterialName);
                      row.GetCell(3).SetCellValue(EquipmentDetail.MaterialSpecValueMaterialStandard);
                      sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 3, 4));
                      row.GetCell(5).SetCellValue(EquipmentDetail.EquipmentMaterialQTY);

                      rowIndex++;
                  }


                  var modelExcel = new ExcelExportModel(Resources.Resource.EquipmentDetail, Parameters.ExcelVersion);

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
          protected static void CreateAndSetCellStyle(ISheet Sheet, int RowIndex,ICellStyle CellStyle)
          {
              var row = Sheet.CreateRow(RowIndex);
              for(int i=0;i< 6;i++)
              {
                  row.CreateCell(i).CellStyle = CellStyle;
              }
          }
        
    }
}
