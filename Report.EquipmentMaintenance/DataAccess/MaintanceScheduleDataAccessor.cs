using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using NPOI.SS.Util;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using Utility;
using Utility.Models;
#if ORACLE
using DbEntity.ORACLE;
using DbEntity.ORACLE.EquipmentMaintenance;
#else
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
#endif
using Models.Authenticated;
using DataAccess;
using DataAccess.EquipmentMaintenance;
using Report.EquipmentMaintenance.Models.MaintanceSchedule;

namespace Report.EquipmentMaintenance.DataAccess
{
    public class MaintanceScheduleDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)  //按照组织进行查詢
        {
            RequestResult result = new RequestResult();
            try
            {
                
                using (EDbEntities db = new EDbEntities())
                {
                    var GridList = new GridViewModel();
                    var ItemList = new List<GridItem>();
                    DbEntities db1 = new DbEntities();

                    var downSteamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);
                    var organizationName = (from a in db1.Organization where a.UniqueID == Parameters.OrganizationUniqueID select a.Description).FirstOrDefault();//组织名称
                    var query = (from a in db.Equipment
                                 join b in db.MJobEquipment on a.UniqueID equals b.EquipmentUniqueID
                                 join c in db.EquipmentPart on b.PartUniqueID equals c.UniqueID
                                 join d in db.MJob on b.MJobUniqueID equals d.UniqueID
                                 //join e in db.MRoute on d.MRouteUniqueID equals e.UniqueID
                                 where   downSteamOrganizationList.Contains(a.OrganizationUniqueID)
                                 select new { a, b.MJobUniqueID, PartName = c.Description, d, RouteName = d.Description }).ToList();
                    //select new { a, b.MJobUniqueID, PartName = c.Description, d, RouteName = e.Description }).ToList();
                    if (query.Count() != 0)
                    {
                        foreach (var item in query)
                        {
                            var BeginDate = (from a in db.MForm where a.MJobUniqueID == item.MJobUniqueID && a.BeginDate != null orderby a.BeginDate select a.BeginDate).FirstOrDefault();//最近保养日期
                            //if (BeginDate != null)
                            //{
                                var Cyclemode = "";
                                if (item.d.CycleMode == "D")
                                {
                                    Cyclemode = Resources.Resource.Every;
                                }
                                if (item.d.CycleMode == "W")
                                {
                                    Cyclemode = Resources.Resource.Day;
                                }
                                if (item.d.CycleMode == "M")
                                {
                                    Cyclemode = Resources.Resource.Week;
                                }
                                if (item.d.CycleMode == "Y")
                                {
                                    Cyclemode = Resources.Resource.Year;
                                }
                                var DataList = new GridItem
                                {
                                    Organization = organizationName,
                                    EquipmentID = item.a.ID,
                                    EquipmentName = item.a.Name,
                                    EquipmentPart = item.PartName,
                                    MRoute = item.RouteName,
                                    CycleMode = Resources.Resource.Every + item.d.CycleCount + Cyclemode,
                                    MJobBeginDate = item.d.BeginDate.ToString("yyyy/MM/dd"),
                                    MJobEndDate = item.d.EndDate == null ? "" : Convert.ToDateTime(item.d.EndDate).ToString("yyyy/MM/dd"),
                                    MFormBeginDate = BeginDate == null ? "" : Convert.ToDateTime(BeginDate).ToString("yyyy/MM/dd")
                                };
                                ItemList.Add(DataList);
                            //}
                        }
                    }
                    GridList.ItemList = ItemList;
                    result.ReturnData(GridList);
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


        public static ExcelExportModel Export(GridViewModel Model, Define.EnumExcelVersion ExcelVersion)
        {
            try
            {
                //var strpath = Utility.Config.TempFolder;
                //string filename = null;
                //result = Query(OrganizationUniqueID);   //根據參數查詢資料，返回result
                //if (result.IsSuccess)
                //{
                //    var itemList = result.Data as List<GridItem>;
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
                    ISheet sheet = wk.CreateSheet(Resources.Resource.MaintanceSchedule);
                    sheet.DefaultColumnWidth = 17;//设置单元格长度
                    sheet.DefaultRowHeight = 400;//设置单元格高度
                    //設置樣式
                    ICellStyle Titlestyle = wk.CreateCellStyle();   //标题的样式
                    IFont Titlefont = wk.CreateFont();
                    Titlefont.FontHeightInPoints = 18;
                    //Titlefont.Boldweight = (short)FontBoldWeight.Bold;
                    //Titlefont.Color = XSSFColor..Index;
                    Titlestyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;  //水平居中
                    Titlestyle.VerticalAlignment = VerticalAlignment.Center;// 垂直居中
                    Titlestyle.SetFont(Titlefont);

                    ICellStyle Borderstyle = wk.CreateCellStyle();//上下左右都有边框
                    Borderstyle.WrapText = true;//自动换行
                    Borderstyle.BorderBottom = BorderStyle.Thin;
                    Borderstyle.BorderTop = BorderStyle.Thin;
                    Borderstyle.BorderLeft = BorderStyle.Thin;
                    Borderstyle.BorderRight = BorderStyle.Thin;
                    Borderstyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;//水平居中
                    Borderstyle.VerticalAlignment = VerticalAlignment.Center;// 垂直居中

                    sheet.CreateRow(0).CreateCell(0).SetCellValue(Resources.Resource.MaintanceSchedule);
                    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 8));
                    sheet.GetRow(0).GetCell(0).CellStyle = Titlestyle;//表頭樣式

                    IRow row2 = sheet.CreateRow(1);
                    row2.CreateCell(0).SetCellValue(Resources.Resource.Organization);
                    row2.CreateCell(1).SetCellValue(Resources.Resource.EquipmentID);
                    row2.CreateCell(2).SetCellValue(Resources.Resource.EquipmentName);
                    row2.CreateCell(3).SetCellValue(Resources.Resource.PartName);
                    row2.CreateCell(4).SetCellValue(Resources.Resource.MaintanceJob);
                    row2.CreateCell(5).SetCellValue(Resources.Resource.CycleMode);
                    row2.CreateCell(6).SetCellValue(Resources.Resource.BeginDate);
                    row2.CreateCell(7).SetCellValue(Resources.Resource.EndDate);
                    row2.CreateCell(8).SetCellValue(Resources.Resource.RecentMaintenanceDate);


                    //绑定数据

                    for (var i = 0; i < Model.ItemList.Count; i++)
                    {
                        sheet.CreateRow(2 + i).CreateCell(0).SetCellValue(Model.ItemList[i].Organization);
                        sheet.GetRow(2 + i).CreateCell(1).SetCellValue(Model.ItemList[i].EquipmentID);
                        sheet.GetRow(2 + i).CreateCell(2).SetCellValue(Model.ItemList[i].EquipmentName);
                        sheet.GetRow(2 + i).CreateCell(3).SetCellValue(Model.ItemList[i].EquipmentPart);
                        sheet.GetRow(2 + i).CreateCell(4).SetCellValue(Model.ItemList[i].MRoute);
                        sheet.GetRow(2 + i).CreateCell(5).SetCellValue(Model.ItemList[i].CycleMode);
                        sheet.GetRow(2 + i).CreateCell(6).SetCellValue(Model.ItemList[i].MJobBeginDate);
                        sheet.GetRow(2 + i).CreateCell(7).SetCellValue(Model.ItemList[i].MJobEndDate);
                        sheet.GetRow(2 + i).CreateCell(8).SetCellValue(Model.ItemList[i].MFormBeginDate);

                    }
                    for (var i = 0; i <= Model.ItemList.Count(); i++)
                    {
                        for (var j = 0; j < 9; j++)
                        {
                            sheet.GetRow(1 + i).GetCell(j).CellStyle = Borderstyle;//內容樣式
                        }

                    }
                    var model = new ExcelExportModel(Resources.Resource.MaintanceSchedule, ExcelVersion);
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
