using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity;
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.Authenticated;
using Report.EquipmentMaintenance.Models.MaterialCost;
using DataAccess;
using DataAccess.EquipmentMaintenance;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Report.EquipmentMaintenance.DataAccess
{
    public class MaterialCostDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)  //按照日期区间进行查詢
        {
            RequestResult result = new RequestResult();
            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    //var downSteamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var MaterialReportList = new MaterialViewModel();//創建存放兩張頁簽內的欄位對象

                    //var MaterialList = new List<MaterialListModel>();//創建存放MaterialList頁簽欄位的list集合
                    //var MaterialReport = new List<MaterialReportModel>();//創建存放Report頁簽欄位的List集合
                    //var SpecUniqueID = (from a in db.MaterialSpec where a.Description == "規格" select a.UniqueID).FirstOrDefault();//獲取Description為規格的UniqueID
                    //var Listquery = (from t1 in db.RForm
                    //                 join t2 in db.Equipment on t1.EquipmentUniqueID equals t2.UniqueID
                    //                 join t3 in db.EquipmentPart on t1.PartUniqueID equals t3.UniqueID
                    //                 join t4 in db.RFormMaterial on t1.UniqueID equals t4.RFormUniqueID
                    //                 join t5 in db.Material on t4.MaterialUniqueID equals t5.UniqueID
                    //                 join t6 in db.MaterialSpecValue on t5.UniqueID equals t6.MaterialUniqueID
                    //                 where t1.CreateTime >= Parameters.BeginDate && t1.CreateTime <= Parameters.EndDate && t6.SpecUniqueID == SpecUniqueID && downSteamOrganizationList.Contains(t1.OrganizationUniqueID)
                    //                 select new { t1, EquipmentName = t2.Name, PartName = t3.Description, t4, MaterialName = t5.Name, MaterialID = t5.ID, t6.Value }).ToList();

                    //if (Listquery.Count() != 0)
                    //{
                    //    foreach (var item in Listquery)
                    //    {
                    //        string RFormType = string.Empty;

                    //        if (string.IsNullOrEmpty(item.t1.MFormUniqueID))
                    //        {
                    //            RFormType = Resources.Resource.Abnormal;
                    //        }
                    //        else
                    //        {
                    //            RFormType = Resources.Resource.Maintenance;
                    //        }

                    //        var ListData = new MaterialListModel
                    //        {
                    //            RformVHNO = item.t1.VHNO,
                    //            RFormTypeName = RFormType,
                    //            EquipmentName = item.EquipmentName,
                    //            PartName = item.PartName,
                    //            CreateDate = "",
                    //            MaterialID = item.MaterialID,
                    //            MaterialName = item.MaterialName,
                    //            MaterialValue = item.Value,
                    //            MaterialQTY = item.t4.Quantity
                    //        };

                    //        MaterialList.Add(ListData);
                    //    }
                    //}

                    ////var Reportquery = (from a in db.RForm
                    ////                   join b in db.RFormMaterial on a.UniqueID equals b.RFormUniqueID
                    ////                   where a.CreateTime >= Parameters.BeginDate && a.CreateTime <= Parameters.EndDate
                    ////                   select b.MaterialUniqueID).Distinct().ToList();

                    ////if (Reportquery.Count() != 0)
                    ////{
                    ////    foreach (var report in Reportquery)
                    ////    {
                    ////        var Material = (from a in db.Material where a.UniqueID == report select new { a.Name ,a.ID}).FirstOrDefault();
                    ////        var SpecValue = (from a in db.MaterialSpecValue where a.MaterialUniqueID == report && a.SpecUniqueID == SpecUniqueID select a.Value).FirstOrDefault();
                    ////        var ReportData = new MaterialReportModel
                    ////        {
                    ////            MaterialUniqueID = Material.ID,
                    ////            MaterialName = Material.Name,
                    ////            MaterialSpec = SpecValue,
                    ////            Price = "",
                    ////            TotalPrice = "",


                    ////        };
                    ////        MaterialReport.Add(ReportData);
                    ////    }
                    ////}

                    //var materialList = (from x in Listquery
                    //                    group x by new { x.MaterialID, x.MaterialName, x.Value }
                    //                        into g
                    //                        select new MaterialReportModel
                    //                        {
                    //                            MaterialUniqueID = g.Key.MaterialID,
                    //                            MaterialName = g.Key.MaterialName,
                    //                            MaterialSpec = g.Key.Value,
                    //                            Total = g.Sum(x => x.t4.Quantity).ToString(),
                    //                            Price = "",
                    //                            TotalPrice = "",
                    //                        }).ToList();

                    //MaterialReport.AddRange(materialList);

                    //MaterialReportList.BeginDateString = Parameters.BeginDateString;
                    //MaterialReportList.EndDateString = Parameters.EndDateString;
                    //MaterialReportList.MaterialList = MaterialList;
                    //MaterialReportList.MaterialReport = MaterialReport;

                    result.ReturnData(MaterialReportList);
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


        public static ExcelExportModel Export( MaterialViewModel Model, Define.EnumExcelVersion ExcelVersion)
        {
            //RequestResult result = new RequestResult();
            
            try
            {
                //var strpath = Utility.Config.TempFolder;
                //string filename = null;
                //result = Query(Model.Parameters);   //根據參數查詢資料，返回result
                //if (result.IsSuccess)
                //{
                //var MaterialListReport = result.Data as MaterialViewModel;
                //var MaterialList = MaterialListReport.MaterialList;
                //var MaterialReport = MaterialListReport.MaterialReport;
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
                    ISheet sheet = wk.CreateSheet("Report");
                    ISheet sheet1 = wk.CreateSheet("MaterialList");

                    sheet.SetColumnWidth(0, 15 * 250);     //设置report单元格长度
                    sheet.SetColumnWidth(1, 15 * 250);
                    sheet.SetColumnWidth(2, 15 * 250);
                    sheet.SetColumnWidth(3, 15 * 250);
                    sheet.SetColumnWidth(4, 15 * 250);
                    sheet.SetColumnWidth(5, 15 * 250);


                    sheet1.SetColumnWidth(0, 15 * 250);     //设置materiallist单元格长度
                    sheet1.SetColumnWidth(1, 15 * 250);
                    sheet1.SetColumnWidth(2, 15 * 250);
                    sheet1.SetColumnWidth(3, 15 * 250);
                    sheet1.SetColumnWidth(4, 15 * 250);
                    sheet1.SetColumnWidth(5, 15 * 250);
                    sheet1.SetColumnWidth(6, 15 * 250);
                    sheet1.SetColumnWidth(7, 15 * 250);
                    sheet1.SetColumnWidth(8, 15 * 250);

                    //設置樣式
                    ICellStyle Titlestyle = wk.CreateCellStyle();   //标题的样式
                    IFont Titlefont = wk.CreateFont();
                    Titlefont.FontHeightInPoints = 18;
                    //Titlefont.Boldweight = (short)FontBoldWeight.Bold;
                    //Titlefont.Color = XSSFColor..Index;
                    Titlestyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;  //水平居中
                    Titlestyle.VerticalAlignment = VerticalAlignment.Center;// 垂直居中
                    Titlestyle.SetFont(Titlefont);


                    ICellStyle Centerstyle = wk.CreateCellStyle();   //居中的样式
                    Centerstyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                    Centerstyle.VerticalAlignment = VerticalAlignment.Center;

                    ICellStyle Borderstyle = wk.CreateCellStyle();//上下左右都有边框
                    Borderstyle.WrapText = true;//自动换行
                    Borderstyle.BorderBottom = BorderStyle.Thin;
                    Borderstyle.BorderTop = BorderStyle.Thin;
                    Borderstyle.BorderLeft = BorderStyle.Thin;
                    Borderstyle.BorderRight = BorderStyle.Thin;
                    Borderstyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;//水平居中
                    Borderstyle.VerticalAlignment = VerticalAlignment.Center;// 垂直居中

                    sheet1.CreateRow(0).CreateCell(0).SetCellValue(Resources.Resource.RFormVHNO);
                    sheet1.GetRow(0).CreateCell(1).SetCellValue(Resources.Resource.RepairFormType);
                    sheet1.GetRow(0).CreateCell(2).SetCellValue(Resources.Resource.EquipmentName);
                    sheet1.GetRow(0).CreateCell(3).SetCellValue(Resources.Resource.PartName);
                    sheet1.GetRow(0).CreateCell(4).SetCellValue(Resources.Resource.VHNOCreateDate);
                    sheet1.GetRow(0).CreateCell(5).SetCellValue(Resources.Resource.RMaterialID);
                    sheet1.GetRow(0).CreateCell(6).SetCellValue(Resources.Resource.RMaterialName);
                    sheet1.GetRow(0).CreateCell(7).SetCellValue(Resources.Resource.RMaterialSpec);
                    sheet1.GetRow(0).CreateCell(8).SetCellValue(Resources.Resource.RMaterialQTY);


                    for (var i = 0; i < Model.MaterialList.Count(); i++)
                    {
                        sheet1.CreateRow(1 + i).CreateCell(0).SetCellValue(Model.MaterialList[i].RformVHNO);
                        sheet1.GetRow(1 + i).CreateCell(1).SetCellValue(Model.MaterialList[i].RFormTypeName);
                        sheet1.GetRow(1 + i).CreateCell(2).SetCellValue(Model.MaterialList[i].EquipmentName);
                        sheet1.GetRow(1 + i).CreateCell(3).SetCellValue(Model.MaterialList[i].PartName);
                        sheet1.GetRow(1 + i).CreateCell(4).SetCellValue(Model.MaterialList[i].CreateDate);
                        sheet1.GetRow(1 + i).CreateCell(5).SetCellValue(Model.MaterialList[i].MaterialID);
                        sheet1.GetRow(1 + i).CreateCell(6).SetCellValue(Model.MaterialList[i].MaterialName);
                        sheet1.GetRow(1 + i).CreateCell(7).SetCellValue(Model.MaterialList[i].MaterialValue);
                        sheet1.GetRow(1 + i).CreateCell(8).SetCellValue(Double.Parse(Model.MaterialList[i].MaterialQTY.ToString()));
                    }
                    //for (var i = 0; i <= Model.MaterialList.Count(); i++)
                    //{
                    //    for (var j = 0; j < 9; j++)
                    //    {
                    //        sheet1.GetRow(i).GetCell(j).CellStyle = Borderstyle;
                    //    }

                    //}
                    sheet.CreateRow(0).CreateCell(0).SetCellValue(Resources.Resource.MaterialCostReport);
                    for (var i = 0; i < 6; i++)
                    {
                        sheet.GetRow(0).CreateCell(1 + i);
                        sheet.GetRow(0).GetCell(i).CellStyle = Titlestyle;//表頭樣式
                    }
                    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 5));

                    sheet.CreateRow(1).CreateCell(0).SetCellValue(Resources.Resource.ByCent);
                    sheet.GetRow(1).GetCell(0).CellStyle = Centerstyle;//設置居中樣式
                    sheet.GetRow(1).CreateCell(1).SetCellValue(Convert.ToDateTime(Model.BeginDateString).ToString("yyyy/MM/dd") + "~" + Convert.ToDateTime(Model.EndDateString).ToString("yyyy/MM/dd"));
                    sheet.GetRow(1).CreateCell(4).SetCellValue(Resources.Resource.PrintDate);
                    sheet.GetRow(1).GetCell(4).CellStyle = Centerstyle;//設置居中樣式
                    sheet.GetRow(1).CreateCell(5).SetCellValue(DateTime.Now.ToString("yyyy/MM/dd"));
                    sheet.CreateRow(2).CreateCell(0).SetCellValue(Resources.Resource.MaterialID);
                    sheet.GetRow(2).CreateCell(1).SetCellValue(Resources.Resource.MaterialName);
                    sheet.GetRow(2).CreateCell(2).SetCellValue(Resources.Resource.MaterialSpec);
                    sheet.GetRow(2).CreateCell(3).SetCellValue(Resources.Resource.MaterialPrice);
                    sheet.GetRow(2).CreateCell(4).SetCellValue(Resources.Resource.MaterialCount);
                    sheet.GetRow(2).CreateCell(5).SetCellValue(Resources.Resource.MaterialTotal);
                    for (var i = 0; i < Model.MaterialReport.Count(); i++)
                    {
                        sheet.CreateRow(3 + i).CreateCell(0).SetCellValue(Model.MaterialReport[i].MaterialUniqueID);
                        sheet.GetRow(3 + i).CreateCell(1).SetCellValue(Model.MaterialReport[i].MaterialName);
                        sheet.GetRow(3 + i).CreateCell(2).SetCellValue(Model.MaterialReport[i].MaterialSpec);
                        sheet.GetRow(3 + i).CreateCell(3).SetCellValue(Model.MaterialReport[i].Price);
                        sheet.GetRow(3 + i).CreateCell(4).SetCellFormula("SUMIF(MaterialList!F:F,A" + (4 + i) + ",MaterialList!I:I)");
                        sheet.GetRow(3 + i).CreateCell(5).SetCellValue(Model.MaterialReport[i].TotalPrice);
                    }

                    for (var i = 0; i <= Model.MaterialReport.Count(); i++)
                    {
                        for (var j = 0; j < 6; j++)
                        {
                            sheet.GetRow(2 + i).GetCell(j).CellStyle = Borderstyle;//內容樣式
                        }

                    }

                   
                    var model = new ExcelExportModel(Resources.Resource.MaterialCost, ExcelVersion);
                    using (FileStream fs = System.IO.File.OpenWrite(model.FullFileName))
                    {
                        wk.Write(fs);
                    }
                    byte[] buff = null;

                    using (var fs = System.IO.File.OpenRead(model.FullFileName))
                    {
                        using (BinaryReader br = new BinaryReader(fs))
                        {
                            long numBytes = new FileInfo(model.FullFileName).Length;

                            buff = br.ReadBytes((int)numBytes);

                            br.Close();
                        }

                        fs.Close();
                    }
                    model.Data = buff;
                    return model;
                //}
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);

                return null;
            }

           
        }
    }
}
