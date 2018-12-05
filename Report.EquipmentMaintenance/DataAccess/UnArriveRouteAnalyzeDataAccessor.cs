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
#endif
using Report.EquipmentMaintenance.Models.UnArriveRouteAnalyze;
using Models.Authenticated;
using DataAccess;
using DataAccess.EquipmentMaintenance;
using Utility;
using System.Reflection;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;
using System.IO;

namespace Report.EquipmentMaintenance.DataAccess
{
  public   class UnArriveRouteAnalyzeDataAccessor
    {
         public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            var model = new GridViewModel()
            {
                Parameters = Parameters
            };

            List<ArriveRecordModel> arriveRecordModelList = new List<ArriveRecordModel>();

            string strUserName=string.Empty;
            string strCycleMode = string.Empty;
            int cycleCount = 0;
            DateTime JobStartTime = DateTime.Now;
            DateTime? JobEndTime = DateTime.Now;
            string strJobUniqueID = string.Empty;
            string strRouteUniqueID = string.Empty;
            string strRouteID = string.Empty;
            string strJobDescription = string.Empty;
            string strRouteName = string.Empty;
            DateTime startDateTime = DateTime.Now;  
            DateTime endDateTime = DateTime.Now; 
            DateTime dateBaseTime = DateTime.Now;


            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var downSteamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);
                    var query = (from a in db.Route 
                                 join b in db.Job on a.UniqueID equals b.RouteUniqueID 
                                 where downSteamOrganizationList.Contains(a.OrganizationUniqueID) 
                                 && Account.QueryableOrganizationUniqueIDList.Contains(a.OrganizationUniqueID) 
                                 select new { b, a.Name,a.ID }).AsQueryable();   //根據組織獲取到路線

                    #region   循環遍歷數據產生集合數據

                    foreach (var temp in query)
                    {
                        strCycleMode = temp.b.CycleMode;
                        cycleCount = temp.b.CycleCount;

                        JobStartTime = temp.b.BeginDate;
                        JobEndTime = temp.b.EndDate;

                        strJobUniqueID = temp.b.UniqueID;
                        strRouteUniqueID = temp.b.RouteUniqueID;
                        strRouteID=temp.ID ;
                      
                        strJobDescription = temp.b.Description;//派工作業
                        strRouteName = temp.Name;   //路線名稱

                        startDateTime = Parameters.BeginDate.Value;    //查詢的起始時間
                        endDateTime = Parameters.EndDate.Value;   //查詢的結束時間
                        dateBaseTime = startDateTime;  //當前查詢的時間

                        List<string> ControlPointUniqueIDList = (from a in db.JobControlPoint where a.JobUniqueID == strJobUniqueID select a.ControlPointUniqueID).ToList();   //獲取job對應的巡檢點信息

                        while (dateBaseTime <= endDateTime)
                        {

                            bool isExist = JobCycleHelper.IsInCycle(dateBaseTime, JobStartTime, JobEndTime, cycleCount, strCycleMode);//調用API 查詢在指定時間是否需要巡檢
                            if (isExist)
                            {
                                DateTime arriveBeginDate = dateBaseTime;
                                DateTime arriveEndDate = dateBaseTime;

                                if (strCycleMode.ToUpper() == "D")
                                {
                                    arriveEndDate = arriveEndDate.AddDays(cycleCount - 1);
                                }
                                else
                                {
                                    if (strCycleMode.ToUpper() == "W")
                                    {
                                        arriveEndDate = arriveEndDate.AddDays(7 * cycleCount - 1);
                                    }
                                    else
                                    {
                                        arriveEndDate = arriveEndDate.AddMonths(cycleCount - 1);
                                    }
                                }

                                string strArriveBeginDate = DateTimeHelper.DateTime2DateString(arriveBeginDate);
                                string strArriveEndDate = DateTimeHelper.DateTime2DateString(arriveEndDate);

                                foreach (var tempControlPoint in ControlPointUniqueIDList)   // 遍歷巡檢點
                                {
                                   var  isArriveRecordExist = (from a in db.ArriveRecord where a.JobUniqueID == strJobUniqueID && a.RouteUniqueID == strRouteUniqueID && a.ControlPointUniqueID == tempControlPoint && (string.Compare(a.ArriveDate, strArriveBeginDate) >= 0) && (string.Compare(a.ArriveDate, strArriveEndDate) <= 0) select a).Any();//判斷在ArriveRecord中是否存在對應巡檢點，路線，和派工資料的信息

                                    if (isArriveRecordExist)
                                    {
                                        var userList = (from a in db.JobUser where a.JobUniqueID == strJobUniqueID select a.UserID).ToList();   //獲取該派工所對應的所有巡檢人員的信息
                                        foreach (var user in userList)
                                        {
                                           var controlPoint = (from a in db.ControlPoint where a.UniqueID == tempControlPoint select new { a.Description,a.ID}).FirstOrDefault();  //获取到巡检点的Description和ID
                                            using(DbEntities db1=new DbEntities())
                                            {
                                                strUserName = (from x in db1.User where x.ID == user select x.Name).FirstOrDefault();
                                            }
                                            ArriveRecordModel arriveRecordModel = new ArriveRecordModel() 
                                            {
                                                JobDescription = strJobDescription,
                                                RouteID = strRouteID,
                                                RouteName=strRouteName,
                                                ControlPointID = controlPoint.ID,
                                                ControlPointDescription=controlPoint.Description,
                                                ArriveDate = dateBaseTime,
                                                UserID=user,
                                                UserName = strUserName
                                            };

                                            arriveRecordModelList.Add(arriveRecordModel); 
                                        }
                                    }
                                }
                            }
                            switch (strCycleMode.ToUpper())   //根據想
                            {
                                case "D": dateBaseTime = dateBaseTime.AddDays(cycleCount); break;  //增加天
                                case "W": dateBaseTime = dateBaseTime.AddDays(cycleCount * 7); break; //增加周
                                case "M": dateBaseTime = dateBaseTime.AddMonths(cycleCount); break;  //增加月
                            }
                        }
                    }

                    #endregion


                    var arriveRecordModelGroupByRoute = (from x in arriveRecordModelList group x by x.Route into g select new { g.Key, g  }).ToList();

                    foreach (var arriveRecordModel in arriveRecordModelGroupByRoute)
                    {
                        GridItem girdItem = new GridItem();
                        girdItem.ArriveRecordModelList = arriveRecordModel.g.ToList();
                        model.ItemList.Add(girdItem);
                    
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
           
                IWorkbook wk = null;


                if (ExcelVersion == Define.EnumExcelVersion._2003)
                {
                    wk = new HSSFWorkbook();
                }
                else
                {
                    wk = new XSSFWorkbook();
                }

                ISheet sheet = wk.CreateSheet(Resources.Resource.UnArriveRouteAnalyze);

                sheet.DefaultColumnWidth = 18;
                sheet.DefaultRowHeight = 400;

                //設置單元格格式
                ICellStyle cellStyle = wk.CreateCellStyle();
                IFont font = wk.CreateFont();
                font.FontName = "新細明體";
                font.FontHeightInPoints = 12;
                cellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                cellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                cellStyle.SetFont(font);

                sheet.CreateRow(0).CreateCell(0).SetCellValue(Resources.Resource.UnArriveRouteAnalyze);
                sheet.GetRow(0).GetCell(0).CellStyle = cellStyle;
                sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 4));
                sheet.GetRow(0).CreateCell(5).SetCellValue(DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Now));

                IRow row2 = sheet.CreateRow(1);
                row2.CreateCell(0).SetCellValue(Resources.Resource.Unit);  //单位
                row2.CreateCell(2).SetCellValue(Resources.Resource.BeginDate);  //开始日期
                row2.CreateCell(4).SetCellValue(Resources.Resource.EndDate);  //结束日期

                //綁定查詢條件
                row2.CreateCell(1).SetCellValue(OrganizationDataAccessor.GetOrganizationDescription(Model.Parameters.OrganizationUniqueID));
                row2.CreateCell(3).SetCellValue(Model.Parameters.BeginDateString);
                row2.CreateCell(5).SetCellValue(Model.Parameters.EndDateString);

                var rowIndex = 2;

                foreach (var item in Model.ItemList)
                {
                    var row = sheet.CreateRow(rowIndex);  

                    var cell = row.CreateCell(0);
                    cell.SetCellValue(Resources.Resource.Route);  //巡检路线

                    cell = row.CreateCell(1);
                    cell.SetCellValue(item.Route);

                    rowIndex++;
                    row = sheet.CreateRow(rowIndex);

                    cell = row.CreateCell(0);
                    cell.SetCellValue(Resources.Resource.UnArriveCount);  //未到位次数

                    cell = row.CreateCell(1);
                    cell.SetCellValue(item.Count);

                    rowIndex++;

                    row = sheet.CreateRow(rowIndex);
                    row.CreateCell(0).SetCellValue(Resources.Resource.Job);
                    row.CreateCell(1).SetCellValue(Resources.Resource.RouteName);
                    row.CreateCell(2).SetCellValue(Resources.Resource.ControlPoint);
                    row.CreateCell(3).SetCellValue(Resources.Resource.ArriveTime);
                    row.CreateCell(4).SetCellValue(Resources.Resource.CheckUser);

                    rowIndex++;

                    foreach (var ArriveRecordModelItem in item.ArriveRecordModelList)
                    {
                        row = sheet.CreateRow(rowIndex);

                        cell = row.CreateCell(0);
                        cell.SetCellValue(ArriveRecordModelItem.JobDescription);

                        cell = row.CreateCell(1);
                        cell.SetCellValue(ArriveRecordModelItem.Route);

                        cell = row.CreateCell(2);
                        cell.SetCellValue(ArriveRecordModelItem.ControlPoint);

                        cell = row.CreateCell(3);
                        cell.SetCellValue(ArriveRecordModelItem.ArriveDateTime);

                        cell = row.CreateCell(4);
                        cell.SetCellValue(ArriveRecordModelItem.User);

                        rowIndex++;
                    }
                }

                var model = new ExcelExportModel(Resources.Resource.UnArriveRouteAnalyze, ExcelVersion);

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
