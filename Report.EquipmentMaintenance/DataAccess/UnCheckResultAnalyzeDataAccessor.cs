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
using Report.EquipmentMaintenance.Models.UnCheckResultAnalyze;

namespace Report.EquipmentMaintenance.DataAccess
{
    public class UnCheckResultAnalyzeDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new GridViewModel()
                {
                    Parameters = Parameters
                };

                var downSteamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                using (EDbEntities db = new EDbEntities())
                {
                    var query = (from x in db.Route
                                      where downSteamOrganizationList.Contains(x.OrganizationUniqueID) &&
                                       Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID)
                                      select new { x.UniqueID, x.Name }).ToList();    //获取路线名称

                    foreach (var tempRoute in query)
                    {
                        var jobList = (from x in db.Job where x.RouteUniqueID == tempRoute.UniqueID select x);//获取该路线的所有job排程
                        foreach (var tempjob in jobList)
                        {
                            DateTime Basedate = Parameters.BeginDate.Value;
                            DateTime Enddate = Parameters.EndDate.Value;

                            while (Basedate <= Enddate)
                            {
                                bool isExist;
                                isExist = JobCycleHelper.IsInCycle(Basedate, tempjob.BeginDate, tempjob.EndDate, tempjob.CycleCount, tempjob.CycleMode);  //判断当天是否需要巡检                                

                                if (isExist)
                                {
                                    string strBaseDate = DateTimeHelper.DateTime2DateString(Basedate);
                                    int CheckCount = (from a in db.CheckResult
                                                      where a.JobUniqueID == tempjob.UniqueID &&
                                                      a.CheckDate == strBaseDate
                                                      select a).Count();
                                    if (CheckCount == 0)
                                    {
                                        var EquipmetPartCheckitem = (from x in db.Job    //派工
                                                                     join x1 in db.Route on x.RouteUniqueID equals x1.UniqueID  //路線
                                                                     join x2 in db.JobControlPoint on x.UniqueID equals x2.JobUniqueID    //派工和巡檢點的中間表
                                                                     join x3 in db.ControlPoint on x2.ControlPointUniqueID equals x3.UniqueID   //巡檢點
                                                                     join xtemp1 in db.JobEquipment on new { x2.JobUniqueID, x2.ControlPointUniqueID } equals new { xtemp1.JobUniqueID, xtemp1.ControlPointUniqueID } into temp//派工和設備，部位之間的關聯   注意由于这儿jobEquipment可能为空，所以需要使用外连接的方式，并且设备和部位表也需要外连接
                                                                     from x4 in temp.DefaultIfEmpty() 
                                                                     join xtemp1 in db.Equipment on x4.EquipmentUniqueID equals xtemp1.UniqueID into temp1//設備
                                                                     from x5 in temp1.DefaultIfEmpty()
                                                                     join xtemp2 in db.EquipmentPart on x4.PartUniqueID equals xtemp2.UniqueID into temp2 //部位
                                                                     from x6 in temp2.DefaultIfEmpty()
                                                                     join x7 in db.JobControlPointCheckItem on new { x2.JobUniqueID, x2.ControlPointUniqueID } equals new { x7.JobUniqueID, x7.ControlPointUniqueID }  //檢查基準與派工和巡檢點的關聯表
                                                                     join x8 in db.CheckItem on x7.CheckItemUniqueID equals x8.UniqueID   //基準
                                                                     where x.UniqueID == tempjob.UniqueID
                                                                     select new
                                                                     {
                                                                         JobDescription = x.Description,   //派工作業
                                                                         RouteID=x1.ID,   //路线ID
                                                                         RouteName = x1.Name,  //路線名稱
                                                                         ControlPointID=x3.ID,
                                                                         ControlPointDescription = x3.Description,   //巡檢點
                                                                         EquipmentID=x5.ID,
                                                                         EquipmentName = x5.Name,  //設備名稱
                                                                         PartDescription = x6.Description,   //部位名稱
                                                                         CheckItemID=x8.ID,
                                                                         CheckItemDescription = x8.Description, //基準
                                                                         ArriveDate = DateTime.Now,
                                                                     }).ToList().Distinct();

                                        var EquipmetCheckitem = (from x in db.Job    //派工
                                                                 join x1 in db.Route on x.RouteUniqueID equals x1.UniqueID  //路線
                                                                 join x2 in db.JobControlPoint on x.UniqueID equals x2.JobUniqueID    //派工和巡檢點的中間表
                                                                 join x3 in db.ControlPoint on x2.ControlPointUniqueID equals x3.UniqueID   //巡檢點
                                                                 join x4 in db.JobEquipmentCheckItem on new { x2.JobUniqueID, x2.ControlPointUniqueID} equals new { x4.JobUniqueID, x4.ControlPointUniqueID }  //檢查基準與派工和巡檢點的關聯表
                                                                 join x5 in db.Equipment on x4.EquipmentUniqueID equals x5.UniqueID  //設備
                                                                 join xtemp in db.EquipmentPart on x5.UniqueID equals xtemp.EquipmentUniqueID into temp
                                                                 from x6 in temp.DefaultIfEmpty()
                                                                 join x7 in db.CheckItem on x4.CheckItemUniqueID equals x7.UniqueID   //基準
                                                                 where x.UniqueID == tempjob.UniqueID

                                                                 select new
                                                                 {
                                                                      JobDescription = x.Description,   //派工作業
                                                                      RouteID=x1.ID,   //路线ID
                                                                      RouteName = x1.Name,  //路線名稱
                                                                      ControlPointID=x3.ID,
                                                                      ControlPointDescription = x3.Description,   //巡檢點
                                                                      EquipmentID=x5.ID,
                                                                      EquipmentName = x5.Name,  //設備名稱
                                                                      PartDescription = x6.Description,   //部位名稱
                                                                      CheckItemID=x7.ID,
                                                                      CheckItemDescription = x7.Description, //基準
                                                                      ArriveDate = DateTime.Now,
                                                                 }).ToList().Distinct();

                                        var unionDataList = EquipmetPartCheckitem.Union(EquipmetCheckitem).ToList();


                                        if (unionDataList.Count() > 0)
                                        {
                                            foreach (var unionDataItem in unionDataList)
                                            {
                                                DbEntities db1 = new DbEntities();
                                                var userIDList = db.JobUser.Where(x => x.JobUniqueID == tempjob.UniqueID).Select(x => x.UserID).ToList();

                                                var userList = (from x in db1.User where userIDList.Contains(x.ID) select new UserModel{ UserID=x.ID, UserName=x.Name}).ToList();
                                              
                                                GridItem Data = new GridItem
                                                {
                                                    JobDescription = unionDataItem.JobDescription,
                                                    RouteID=unionDataItem.RouteID,
                                                    RouteName = unionDataItem.RouteName,
                                                    ControlPointID=unionDataItem.ControlPointID,
                                                    ControlPointDescription = unionDataItem.ControlPointDescription,
                                                    EquipmentID=unionDataItem.EquipmentID,
                                                    EquipmentName = unionDataItem.EquipmentName,                                         
                                                    PartDescription = unionDataItem.PartDescription,
                                                    CheckItemID=unionDataItem.CheckItemID,
                                                    CheckItemDescription=unionDataItem.CheckItemDescription,
                                                    ArriveDate=unionDataItem.ArriveDate,
                                                    UserList = userList
                                                };
                                                model.ItemList.Add(Data);
                                            }
                                        }
                                    }

                                }
                               Basedate= Basedate.AddDays(1);
                            }
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

        public static ExcelExportModel Export(GridViewModel Model, Define.EnumExcelVersion ExcelVersion)
        {
            try
            {

                OrganizationDataAccessor.GetOrganizationDescription(Model.Parameters.OrganizationUniqueID);
               
                IWorkbook wk;

                if (ExcelVersion == Define.EnumExcelVersion._2003)
                {
                    wk = new HSSFWorkbook();
                }
                else
                {
                    wk = new XSSFWorkbook();
                }

                ISheet sheet = wk.CreateSheet(Resources.Resource.UnCheckResultAnalyze);
                sheet.DefaultColumnWidth = 18;//设置单元格长度
                sheet.DefaultRowHeight = 400;//设置单元格高度

                ICellStyle cellStyle = wk.CreateCellStyle();
                cellStyle.WrapText = true;
                wk.CreateFont().FontName = "新細明體";
                wk.CreateFont().FontHeightInPoints = 18;
                cellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                sheet.CreateRow(0).CreateCell(0).SetCellValue(Resources.Resource.UnCheckResultAnalyze);


                sheet.CreateRow(0).CreateCell(0).SetCellValue(Resources.Resource.UnArriveResultAnalyze);
                sheet.GetRow(0).GetCell(0).CellStyle = cellStyle;
                sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 7));
                sheet.GetRow(0).CreateCell(8).SetCellValue(DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Now));




                IRow row2 = sheet.CreateRow(1);
                row2.CreateCell(0).SetCellValue(Resources.Resource.Unit);  //单位
                row2.CreateCell(2).SetCellValue(Resources.Resource.BeginDate);  //开始日期
                row2.CreateCell(4).SetCellValue(Resources.Resource.EndDate);  //结束日期

                //綁定查詢條件
                row2.CreateCell(1).SetCellValue(OrganizationDataAccessor.GetOrganizationDescription(Model.Parameters.OrganizationUniqueID));
                row2.CreateCell(3).SetCellValue(Model.Parameters.BeginDateString);
                row2.CreateCell(5).SetCellValue(Model.Parameters.EndDateString);

                IRow row3 = sheet.CreateRow(2);
                row3.CreateCell(0).SetCellValue(Resources.Resource.Job);
                row3.CreateCell(1).SetCellValue(Resources.Resource.Route);
                row3.CreateCell(2).SetCellValue(Resources.Resource.ControlPoint);
                row3.CreateCell(3).SetCellValue(Resources.Resource.Equipment);
                row3.CreateCell(4).SetCellValue(Resources.Resource.EquipmentPart);
                row3.CreateCell(5).SetCellValue(Resources.Resource.CheckItem);
                row3.CreateCell(6).SetCellValue(Resources.Resource.CheckDate);
                row3.CreateCell(7).SetCellValue(Resources.Resource.CheckUser);
               
                var rowIndex = 3;

                foreach (var item in Model.ItemList)
                {
                    var row = sheet.CreateRow(rowIndex);

                    var cell = row.CreateCell(0);
                    cell.SetCellValue(item.JobDescription);

                    cell = row.CreateCell(1);
                    cell.SetCellValue(item.Route);

                    cell = row.CreateCell(2);
                    cell.SetCellValue(item.ControlPoint);

                    cell = row.CreateCell(3);
                    cell.SetCellValue(item.Equipment);

                    cell = row.CreateCell(4);
                    cell.SetCellValue(item.PartDescription);

                    cell = row.CreateCell(5);
                    cell.SetCellValue(item.CheckItem);

                    cell = row.CreateCell(6);
                    cell.SetCellValue(item.ArriveDateTime);

                    cell = row.CreateCell(7);
                    cell.SetCellValue(item.User);

                    rowIndex++;
                }

                var model = new ExcelExportModel(Resources.Resource.UnCheckResultAnalyze, ExcelVersion);

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
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);

                return null;
            }
        }
    }
}
