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
using Report.EquipmentMaintenance.Models.MaintanceDelay;
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
    public class MaintanceDelayDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters)  //按照日期区间进行查詢
        {
            RequestResult result = new RequestResult();

            try
            {
                //var downSteamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);
                using (EDbEntities db = new EDbEntities())
                {
                    var GridList = new GridViewModel();
                    //var ItemList = new List<GridItem>();
                    //var query = (from a in db.MForm
                    //             join b in db.MJob on a.MJobUniqueID equals b.UniqueID
                    //             join c in db.MRoute on b.MRouteUniqueID equals c.UniqueID
                    //             //where a.CycleBeginDate >= Parameters.BeginDate && a.CycleBeginDate <= Parameters.BeginDate && downSteamOrganizationList.Contains(c.OrganizationUniqueID)
                    //             select new { a, CycleCount = b.CycleCount, CycleMode = b.CycleMode, c.Description }).ToList();//根据日期区间获取定期保养资料
                    //if (query.Count() != 0)
                    //{
                    //    foreach (var item in query)
                    //    {
                    //        var DataList = new GridItem();
                    //        if (item.a.BeginDate != null)
                    //        {
                    //            if (item.a.BeginDate > item.a.CycleEndDate)
                    //            {
                    //                DateTime BeginDate = Convert.ToDateTime(item.a.BeginDate);
                    //                DataList.VHNO = item.a.VHNO;
                    //                DataList.MrouteDescription = item.Description;
                    //                DataList.MCycle = item.CycleCount + item.CycleMode;
                    //                DataList.CycleBeginDate = item.a.CycleBeginDate.ToString();
                    //                DataList.CycleEndDate = item.a.CycleEndDate.ToString();
                    //                //DataList.MFormUserID = item.a.UserID;
                    //                DataList.MFormBeginDate = item.a.BeginDate.ToString();
                    //                //DataList.Status = item.a.Status;
                    //                DataList.DelayDays = (BeginDate - item.a.CycleEndDate).Days;
                    //                ItemList.Add(DataList);
                    //            }
                    //        }
                    //        else
                    //        {
                    //            DateTime BeginDate = DateTime.Now.Date;
                    //            if (BeginDate > item.a.CycleEndDate)
                    //            {
                    //                DataList.VHNO = item.a.VHNO;
                    //                DataList.MrouteDescription = item.Description;
                    //                DataList.MCycle = item.CycleCount + item.CycleMode;
                    //                DataList.CycleBeginDate = item.a.CycleBeginDate.ToString();
                    //                DataList.CycleEndDate = item.a.CycleEndDate.ToString();
                    //                //DataList.MFormUserID = item.a.UserID;
                    //                DataList.MFormBeginDate = " ";
                    //                //DataList.Status = item.a.Status;
                    //                DataList.DelayDays = (BeginDate - item.a.CycleEndDate).Days;
                    //                ItemList.Add(DataList);
                    //            }
                    //        }

                    //    }

                    //}
                    //GridList.ItemList = ItemList;

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
                //result = Query(StartTime, EndTime);   //根據參數查詢資料，返回result
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
                ISheet sheet = wk.CreateSheet(Resources.Resource.MaintanceDelay);
                sheet.DefaultColumnWidth = 17;//设置单元格长度
                sheet.DefaultRowHeight = 400;//设置单元格高度
                //設置樣式
                ICellStyle Titlestyle = wk.CreateCellStyle();   //标题的样式
                IFont Titlefont = wk.CreateFont();
                Titlefont.FontHeightInPoints = 18;
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
                sheet.CreateRow(0).CreateCell(0).SetCellValue(Resources.Resource.MaintanceDelay);
                sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 8));
                sheet.GetRow(0).GetCell(0).CellStyle = Titlestyle;//表頭樣式
                IRow row2 = sheet.CreateRow(1);
                row2.CreateCell(0).SetCellValue(Resources.Resource.VHNO);
                row2.CreateCell(1).SetCellValue(Resources.Resource.Subject);
                row2.CreateCell(2).SetCellValue(Resources.Resource.MaintenanceCycle);
                row2.CreateCell(3).SetCellValue(Resources.Resource.BeginDate);
                row2.CreateCell(4).SetCellValue(Resources.Resource.EndDate);
                row2.CreateCell(5).SetCellValue(Resources.Resource.TakeJobUser);
                row2.CreateCell(6).SetCellValue(Resources.Resource.TakeJobTime);
                row2.CreateCell(7).SetCellValue(Resources.Resource.Status);
                row2.CreateCell(8).SetCellValue(Resources.Resource.DelayDays);

                //绑定数据
                for (var i = 0; i < Model.ItemList.Count; i++)
                {
                    sheet.CreateRow(2 + i).CreateCell(0).SetCellValue(Model.ItemList[i].VHNO);
                    sheet.GetRow(2 + i).CreateCell(1).SetCellValue(Model.ItemList[i].MrouteDescription);
                    sheet.GetRow(2 + i).CreateCell(2).SetCellValue(Model.ItemList[i].MCycle);
                    sheet.GetRow(2 + i).CreateCell(3).SetCellValue(Model.ItemList[i].CycleBeginDate);
                    sheet.GetRow(2 + i).CreateCell(4).SetCellValue(Model.ItemList[i].CycleEndDate);
                    sheet.GetRow(2 + i).CreateCell(5).SetCellValue(Model.ItemList[i].MFormUserID);
                    sheet.GetRow(2 + i).CreateCell(6).SetCellValue(Model.ItemList[i].MFormBeginDate);
                    sheet.GetRow(2 + i).CreateCell(7).SetCellValue(Model.ItemList[i].Status);
                    sheet.GetRow(2 + i).CreateCell(8).SetCellValue(Model.ItemList[i].DelayDays);

                }
                for (var i = 0; i <= Model.ItemList.Count(); i++)
                {
                    for (var j = 0; j < 9; j++)
                    {
                        sheet.GetRow(1 + i).GetCell(j).CellStyle = Borderstyle;//內容樣式
                    }

                }

                var model = new ExcelExportModel(Resources.Resource.MaintanceDelay, ExcelVersion);

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
