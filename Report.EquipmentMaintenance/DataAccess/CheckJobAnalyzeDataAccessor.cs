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
using Report.EquipmentMaintenance.Models.CheckJobAnalyze;

namespace Report.EquipmentMaintenance.DataAccess
{
    public class CheckJobAnalyzeDataAccessor
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

                List<string> CheckTime = new List<string>();

                DateTime beginDate = Convert.ToDateTime(Parameters.BeginDate);

                while (beginDate <= Parameters.EndDate)
                {
                    CheckTime.Add(DateTimeHelper.DateTime2DateStringWithSeperator(beginDate)); 
                    beginDate= beginDate.AddDays(1);
                }

                model.CheckTime = CheckTime;

                var downSteamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                using (EDbEntities db = new EDbEntities())
                {
                    DateTime dtmBeginDate = Parameters.BeginDate.Value;
                    DateTime dtmEndDate = Parameters.EndDate.Value;

                    var query = (from a in db.Route where downSteamOrganizationList.Contains(a.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(a.OrganizationUniqueID) select new { a.UniqueID, a.Name,a.ID }).ToList();

                    foreach (var tempRoute in query)
                    {
                        GridItem item = new GridItem();

                        item.RouteID = tempRoute.ID;
                        item.RouteName = tempRoute.Name;

                        var jobInfoList = (from a in db.Job where a.RouteUniqueID == tempRoute.UniqueID select a).ToList();   //获取到该巡检路线多对应的多个job

                        foreach (var tempJob in jobInfoList)
                        {
                            DateTime dtmBaseDate = dtmBeginDate;
                           
                            CheckAllResult allResult = new CheckAllResult();

                            string strJobId = tempJob.UniqueID;

                            allResult.JobDescription = tempJob.Description;  //派工作业名称

                            string strCycleMode = tempJob.CycleMode;
                            int intCycleCount = tempJob.CycleCount;

                            List<string> resultList = new List<string>();

                            while (dtmBaseDate <= dtmEndDate)
                            {
                                bool isExist;

                                isExist = JobCycleHelper.IsInCycle(dtmBaseDate, tempJob.BeginDate, tempJob.EndDate, intCycleCount, strCycleMode);  //判断当天是否有巡检

                                if (!isExist)
                                {
                                    resultList.Add("○");
                                }
                                else
                                {
                                    DateTime b, e;

                                    JobCycleHelper.GetDateSpan(dtmBaseDate, tempJob.BeginDate, tempJob.EndDate, intCycleCount, strCycleMode, out b, out e);

                                    var beginDateString = DateTimeHelper.DateTime2DateString(b);
                                    var endDateString = DateTimeHelper.DateTime2DateString(e);

                                    if (!string.IsNullOrEmpty(tempJob.BeginTime) && !string.IsNullOrEmpty(tempJob.EndTime) && string.Compare(tempJob.BeginTime, tempJob.EndTime) > 0)
                                    {
                                        endDateString = DateTimeHelper.DateTime2DateString(e.AddDays(1));
                                    }

                                    var jobResult = db.JobResult.FirstOrDefault(x => x.JobUniqueID == tempJob.UniqueID && x.BeginDate == beginDateString && x.EndDate == endDateString);

                                    if (jobResult != null)
                                    {
                                        //string strBaseDateTime = DateTimeHelper.DateTime2DateString(dtmBaseDate);  

                                        //var checkItemList = (from a in db.JobControlPointCheckItem where a.JobUniqueID == strJobId select new { a.JobUniqueID, a.ControlPointUniqueID, EquipmentUniqueID = " ", a.CheckItemUniqueID, CurrentDate = strBaseDateTime }).Concat(from a in db.JobEquipmentCheckItem where a.JobUniqueID == strJobId select new { a.JobUniqueID, a.ControlPointUniqueID, a.EquipmentUniqueID, a.CheckItemUniqueID, CurrentDate = strBaseDateTime }).ToList();

                                        //int intShouldCount = checkItemList.Count();     //应巡检数量
                                        //int intActualCount = 0;     //实际巡检数量

                                        //foreach (var CheckItem in checkItemList)
                                        //{
                                        //    int intDataCount = (from a in db.CheckResult where a.JobUniqueID == CheckItem.JobUniqueID && a.ControlPointUniqueID == CheckItem.ControlPointUniqueID && a.EquipmentUniqueID == CheckItem.EquipmentUniqueID && a.CheckItemUniqueID == CheckItem.CheckItemUniqueID && a.CheckDate == CheckItem.CurrentDate select a).Count();
                                        //}

                                        if (jobResult.IsCompleted)
                                        {
                                            resultList.Add("V");
                                        }
                                        else if (jobResult.CompleteRate == string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.UnPatrol))
                                        {
                                            resultList.Add("※");
                                        }
                                        else
                                        {
                                            resultList.Add("●");
                                        }

                                        //if (intActualCount == 0)
                                        //{
                                        //    resultList.Add("※");
                                        //}
                                        //else if (intShouldCount == intActualCount)
                                        //{
                                        //    resultList.Add("V");
                                        //}
                                        //else
                                        //{
                                        //    resultList.Add("●");
                                        //}
                                    }
                                }

                                dtmBaseDate = dtmBaseDate.AddDays(1);
                            }

                            allResult.Result  = resultList;
                           item.CheckResultList.Add(allResult);
                        }

                        model.ItemList.Add(item);
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

                ISheet sheet = wk.CreateSheet(Resources.Resource.CheckJobAnalyze);

                //設置單元格格式
                ICellStyle cellStyle = wk.CreateCellStyle();
                IFont font = wk.CreateFont();
                font.FontName = "新細明體";
                font.FontHeightInPoints = 12;
                cellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                cellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                cellStyle.SetFont(font);
             

                sheet.DefaultColumnWidth = 18;     //设置单元格长度
                sheet.DefaultRowHeight = 400;      //设置单元格高度
                sheet.CreateRow(0).CreateCell(0).SetCellValue(Resources.Resource.CheckJobAnalyze);
                sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 4));
                sheet.GetRow(0).GetCell(0).CellStyle = cellStyle;
                sheet.GetRow(0).CreateCell(5).SetCellValue(DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTime.Now));



                IRow row2 = sheet.CreateRow(1);
                row2.CreateCell(0).SetCellValue(Resources.Resource.Unit);
                row2.CreateCell(2).SetCellValue(Resources.Resource.BeginDate);
                row2.CreateCell(4).SetCellValue(Resources.Resource.EndDate);

                //綁定查詢條件
                row2.CreateCell(1).SetCellValue(OrganizationDataAccessor.GetOrganizationDescription(Model.Parameters.OrganizationUniqueID));
                row2.CreateCell(3).SetCellValue(Model.Parameters.BeginDateString);
                row2.CreateCell(5).SetCellValue(Model.Parameters.EndDateString);

                var rowIndex=2;
                IRow row;
                foreach (var item in Model.ItemList)
                {

                    row = sheet.CreateRow(rowIndex);
                    row.CreateCell(0).SetCellValue(@Resources.Resource.Route);
                    row.CreateCell(1).SetCellValue(@Resources.Resource.UnSendJob);
                    row.CreateCell(2).SetCellValue(@Resources.Resource.UnCheckResult);
                    row.CreateCell(3).SetCellValue(@Resources.Resource.UnCompletelyCheckResult);
                    row.CreateCell(4).SetCellValue(@Resources.Resource.NormalCheckResult);

                    rowIndex++;
                    row = sheet.CreateRow(rowIndex);
                    row.CreateCell(0).SetCellValue(item.Route);
                    row.CreateCell(1).SetCellValue(item.UnSendJob);
                    row.CreateCell(2).SetCellValue(item.UnCheckResult);
                    row.CreateCell(3).SetCellValue(item.UnCompletelyCheckResult);
                    row.CreateCell(4).SetCellValue(item.NormalCheckResult);

                    rowIndex++;

                    row = sheet.CreateRow(rowIndex);
                    row.CreateCell(1).SetCellValue(Resources.Resource.Job);
                    for(int i=2;i<Model.CheckTime.Count+2;i++)
                    {
                        row.CreateCell(i).SetCellValue(Model.CheckTime[i-2]);
                    }

                    rowIndex++;
                    foreach (var checkResult in item.CheckResultList)
                    {
                        row = sheet.CreateRow(rowIndex);

                        row.CreateCell(1).SetCellValue(checkResult.JobDescription);
                        for (int i = 2; i < checkResult.Result.Count+2; i++)
                        {
                            row.CreateCell(i).SetCellValue(checkResult.Result[i-2]);
                        }
                        rowIndex++;
                    }
                }
                var model = new ExcelExportModel(Resources.Resource.CheckJobAnalyze, ExcelVersion);

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
