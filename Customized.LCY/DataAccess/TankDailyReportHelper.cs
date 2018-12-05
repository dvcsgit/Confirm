using Customized.LCY.Models.TankDailyReport;
using DbEntity.MSSQL.EquipmentMaintenance;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace Customized.LCY.DataAccess
{
    public class TankDailyReportHelper
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                var jobUniqueID = "99e79d33-2776-4829-949d-5ba5fe65f722";

                var model = new ReportModel()
                {
                    CheckDateString = DateTimeHelper.DateString2DateStringWithSeparator(Parameters.Date)
                };

                using (EDbEntities db = new EDbEntities())
                {
                    var job = (from j in db.Job
                               join r in db.Route
                               on j.RouteUniqueID equals r.UniqueID
                               where j.UniqueID == jobUniqueID
                               select new
                               {
                                   Job = j,
                                   Route = r
                               }).First();

                    var firstArriveRecord = db.ArriveRecord.Where(x => x.JobUniqueID == job.Job.UniqueID && x.ArriveDate == Parameters.Date).OrderBy(x => x.ArriveTime).FirstOrDefault();

                    if (firstArriveRecord != null)
                    {
                        model.CheckTimeBegin = DateTimeHelper.TimeString2TimeStringWithSeperator(firstArriveRecord.ArriveTime);
                    }

                    var checkResultList = db.CheckResult.Where(x => x.JobUniqueID == job.Job.UniqueID && x.CheckDate == Parameters.Date).ToList();

                    var lastCheckResult = checkResultList.OrderByDescending(x => x.CheckTime).FirstOrDefault();

                    if (lastCheckResult != null)
                    {
                        model.CheckTimeEnd = DateTimeHelper.TimeString2TimeStringWithSeperator(lastCheckResult.CheckTime);
                    }

                    var equipmentList = (from x in db.JobEquipment
                                         join e in db.Equipment
                                         on x.EquipmentUniqueID equals e.UniqueID
                                         where x.JobUniqueID == jobUniqueID
                                         select new
                                         {
                                             e.UniqueID,
                                             e.ID,
                                             e.Name
                                         }).OrderBy(x => x.ID).ToList();

                    foreach (var equipment in equipmentList)
                    {
                        if (equipment.Name.Contains("排水閥"))
                        {
                            var status = checkResultList.FirstOrDefault(x => x.EquipmentUniqueID == equipment.UniqueID && x.CheckItemUniqueID == "29e1544a-6ae3-4b50-9c8a-06ca29eaeacf");

                            model.ValveList.Add(new ValveModel()
                            {
                                ID = equipment.ID,
                                Status = status != null ? status.FeelOptionDescription : ""
                            });
                        }
                        else
                        {
                            var equipmentCheckResultList = checkResultList.Where(x => x.EquipmentUniqueID == equipment.UniqueID).ToList();

                            var upperLimit = db.EquipmentCheckItem.FirstOrDefault(x => x.EquipmentUniqueID == equipment.UniqueID && x.CheckItemUniqueID == "0829b19a-9b21-4166-851b-f607960be645");

                            var level = equipmentCheckResultList.Where(x => x.CheckItemDescription.Contains("儲槽液位")).OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();
                            var content = equipmentCheckResultList.Where(x => x.CheckItemDescription.Contains("儲槽內容物")).OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();
                            var temperature = equipmentCheckResultList.Where(x => x.CheckItemDescription.Contains("儲槽溫度")).OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();

                            var pipeCheckItemDescription = string.Format("{0}地下管接管情形", equipment.ID);
                            var pipe = checkResultList.Where(x => x.CheckItemDescription == pipeCheckItemDescription).OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();

                            var exchangeCheckItemDescription = string.Format("{0}-V7是否有調撥管", equipment.ID);
                            var haveExchange = checkResultList.Where(x => x.CheckItemDescription == exchangeCheckItemDescription).OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();

                            var v1 = equipmentCheckResultList.Where(x => x.CheckItemDescription == "V1").OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();
                            var v2 = equipmentCheckResultList.Where(x => x.CheckItemDescription == "V2").OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();
                            var v3 = equipmentCheckResultList.Where(x => x.CheckItemDescription == "V3").OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();
                            var v4 = equipmentCheckResultList.Where(x => x.CheckItemDescription == "V4").OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();
                            var v5 = equipmentCheckResultList.Where(x => x.CheckItemDescription == "V5").OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();
                            var v6 = equipmentCheckResultList.Where(x => x.CheckItemDescription == "V6").OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();
                            var v7 = equipmentCheckResultList.Where(x => x.CheckItemDescription == "V7").OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();
                            var v8 = equipmentCheckResultList.Where(x => x.CheckItemDescription == "V8").OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();
                            var v9 = equipmentCheckResultList.Where(x => x.CheckItemDescription == "V9").OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();
                            var v10 = equipmentCheckResultList.Where(x => x.CheckItemDescription == "V10").OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();
                            var v11 = equipmentCheckResultList.Where(x => x.CheckItemDescription == "V11").OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();
                            var v12 = equipmentCheckResultList.Where(x => x.CheckItemDescription == "V12").OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();
                            var v13 = equipmentCheckResultList.Where(x => x.CheckItemDescription == "V13").OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();
                            var n2In = equipmentCheckResultList.Where(x => x.CheckItemDescription == "N2進口閥").OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();
                            var n2Out = equipmentCheckResultList.Where(x => x.CheckItemDescription == "N2入槽閥").OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();
                            var pressure = equipmentCheckResultList.Where(x => x.CheckItemDescription == "槽內壓力").OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();
                            var recycle = equipmentCheckResultList.Where(x => x.CheckItemDescription == "冷凝液回收量").OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();
                            
                            model.TankList.Add(new TankModel()
                            {
                                ID = equipment.ID,
                                _UpperLimit = upperLimit != null ? upperLimit.UpperLimit : default(double?),
                                _Level = level != null ? level.Value : default(double?),
                                _Content = content != null ? content.FeelOptionDescription : string.Empty,
                                _Temperature = temperature != null ? temperature.Value : default(double?),
                                _V1 = v1 != null ? v1.FeelOptionDescription : string.Empty,
                                _V2 = v2 != null ? v2.FeelOptionDescription : string.Empty,
                                _V3 = v3 != null ? v3.FeelOptionDescription : string.Empty,
                                _V4 = v4 != null ? v4.FeelOptionDescription : string.Empty,
                                _V5 = v5 != null ? v5.FeelOptionDescription : string.Empty,
                                _V6 = v6 != null ? v6.FeelOptionDescription : string.Empty,
                                _V7 = v7 != null ? v7.FeelOptionDescription : string.Empty,
                                _V8 = v8 != null ? v8.FeelOptionDescription : string.Empty,
                                _V9 = v9 != null ? v9.FeelOptionDescription : string.Empty,
                                _V10 = v10 != null ? v10.FeelOptionDescription : string.Empty,
                                _V11 = v11 != null ? v11.FeelOptionDescription : string.Empty,
                                _V12 = v12 != null ? v12.FeelOptionDescription : string.Empty,
                                _V13 = v13 != null ? v13.FeelOptionDescription : string.Empty,
                                _N2In = n2In != null ? n2In.FeelOptionDescription : string.Empty,
                                _N2Out = n2Out != null ? n2Out.FeelOptionDescription : string.Empty,
                                _Pressure = pressure != null ? pressure.Value : default(double?),
                                _Recycle = recycle != null ? recycle.Value : default(double?),
                                Pipe = pipe != null ? pipe.FeelOptionDescription : string.Empty,
                                _HaveExchange = haveExchange != null ? haveExchange.FeelOptionDescription : string.Empty
                            });
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

        public static RequestResult Export(ReportModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                var template = Customized.LCY.Utility.Config.TankDailyReportTemplate;

                IWorkbook workBook = null;

                using (FileStream fs = new FileStream(template, FileMode.Open, FileAccess.ReadWrite))
                {
                    workBook = new XSSFWorkbook(fs);

                    fs.Close();
                }

                IRow row;
                ICell cell;

                var worksheet = workBook.GetSheetAt(0);

                #region Page1
                if (Model.CheckDate.HasValue)
                {
                    row = worksheet.GetRow(3);
                    cell = row.GetCell(12);
                    cell.SetCellValue(string.Format("檢查日期：{0}年{1}月{2}日", Model.CheckDate.Value.Year, Model.CheckDate.Value.Month, Model.CheckDate.Value.Day));
                }

                row = worksheet.GetRow(4);
                cell = row.GetCell(12);
                if (!string.IsNullOrEmpty(Model.CheckTimeBegin) && !string.IsNullOrEmpty(Model.CheckTimeEnd))
                {
                    cell.SetCellValue(string.Format("檢查時間：{0} 開始 {1} 結束", Model.CheckTimeBegin, Model.CheckTimeEnd));
                }
                else
                {
                    cell.SetCellValue("檢查時間：");
                }

                for (int rowIndex = 8; rowIndex <= 37; rowIndex++)
                {
                    row = worksheet.GetRow(rowIndex);

                    var tankID = row.GetCell(0).StringCellValue;

                    var tank = Model.TankList.FirstOrDefault(x => x.ID == tankID);

                    if (tank != null)
                    {
                        row.GetCell(1).SetCellValue(tank.UpperLimit);
                        row.GetCell(2).SetCellValue(tank.Level);
                        row.GetCell(3).SetCellValue(tank.Content);
                        row.GetCell(4).SetCellValue(tank.Temperature);
                        row.GetCell(5).SetCellValue(tank.V1);
                        row.GetCell(6).SetCellValue(tank.V2);
                        row.GetCell(7).SetCellValue(tank.V3);
                        row.GetCell(8).SetCellValue(tank.V4);
                        row.GetCell(9).SetCellValue(tank.V5);
                        row.GetCell(10).SetCellValue(tank.V6);
                        row.GetCell(11).SetCellValue(tank.V7);
                        row.GetCell(12).SetCellValue(tank.V8);
                        row.GetCell(13).SetCellValue(tank.V9);
                        row.GetCell(14).SetCellValue(tank.V10);
                        row.GetCell(15).SetCellValue(tank.V11);
                        row.GetCell(16).SetCellValue(tank.V12);
                        row.GetCell(17).SetCellValue(tank.V13);
                        row.GetCell(18).SetCellValue(tank.N2In);
                        row.GetCell(19).SetCellValue(tank.N2Out);
                        row.GetCell(20).SetCellValue(tank.Pressure);
                    }
                }
                #endregion

                #region Page2
                if (Model.CheckDate.HasValue)
                {
                    row = worksheet.GetRow(43);
                    cell = row.GetCell(12);
                    cell.SetCellValue(string.Format("檢查日期：{0}年{1}月{2}日", Model.CheckDate.Value.Year, Model.CheckDate.Value.Month, Model.CheckDate.Value.Day));
                }

                row = worksheet.GetRow(44);
                cell = row.GetCell(12);
                if (!string.IsNullOrEmpty(Model.CheckTimeBegin) && !string.IsNullOrEmpty(Model.CheckTimeEnd))
                {
                    cell.SetCellValue(string.Format("檢查時間：{0} 開始 {1} 結束", Model.CheckTimeBegin, Model.CheckTimeEnd));
                }
                else
                {
                    cell.SetCellValue("檢查時間：");
                }

                for (int rowIndex = 48; rowIndex <= 71; rowIndex++)
                {
                    row = worksheet.GetRow(rowIndex);

                    var tankID = row.GetCell(0).StringCellValue;

                    var tank = Model.TankList.FirstOrDefault(x => x.ID == tankID);

                    if (tank != null)
                    {
                        row.GetCell(1).SetCellValue(tank.UpperLimit);
                        row.GetCell(2).SetCellValue(tank.Level);
                        row.GetCell(3).SetCellValue(tank.Content);
                        row.GetCell(4).SetCellValue(tank.Temperature);
                        row.GetCell(5).SetCellValue(tank.V1);
                        row.GetCell(6).SetCellValue(tank.V2);
                        row.GetCell(7).SetCellValue(tank.V3);
                        row.GetCell(8).SetCellValue(tank.V4);
                        row.GetCell(9).SetCellValue(tank.V5);
                        row.GetCell(10).SetCellValue(tank.V6);
                        row.GetCell(11).SetCellValue(tank.V7);
                        row.GetCell(12).SetCellValue(tank.V8);
                        row.GetCell(13).SetCellValue(tank.V9);
                        row.GetCell(14).SetCellValue(tank.V10);
                        row.GetCell(15).SetCellValue(tank.V11);
                        row.GetCell(16).SetCellValue(tank.V12);
                        row.GetCell(17).SetCellValue(tank.V13);
                        row.GetCell(18).SetCellValue(tank.N2In);
                        row.GetCell(19).SetCellValue(tank.N2Out);
                        row.GetCell(20).SetCellValue(tank.Pressure);
                    }
                }

                row = worksheet.GetRow(72);
                var tmp = Model.TankList.FirstOrDefault(x => x.ID == "AT01");
                if (tmp != null)
                {
                    row.GetCell(7).SetCellValue(tmp.Recycle);
                    if (tmp._Recycle.HasValue && tmp._Recycle.Value > 0)
                    {
                        row.GetCell(12).SetCellValue("V"); 
                    }
                    row.GetCell(18).SetCellValue(tmp.Pressure);
                }

                row = worksheet.GetRow(73);
                tmp = Model.TankList.FirstOrDefault(x => x.ID == "ET01");
                if (tmp != null)
                {
                    row.GetCell(7).SetCellValue(tmp.Recycle);
                    if (tmp._Recycle.HasValue && tmp._Recycle.Value > 0)
                    {
                        row.GetCell(12).SetCellValue("V");
                    }
                    row.GetCell(18).SetCellValue(tmp.Pressure);
                }

                #endregion

                #region Page3
                if (Model.CheckDate.HasValue)
                {
                    row = worksheet.GetRow(83);
                    cell = row.GetCell(12);
                    cell.SetCellValue(string.Format("檢查日期：{0}年{1}月{2}日", Model.CheckDate.Value.Year, Model.CheckDate.Value.Month, Model.CheckDate.Value.Day));
                }

                row = worksheet.GetRow(84);
                cell = row.GetCell(12);
                if (!string.IsNullOrEmpty(Model.CheckTimeBegin) && !string.IsNullOrEmpty(Model.CheckTimeEnd))
                {
                    cell.SetCellValue(string.Format("檢查時間：{0} 開始 {1} 結束", Model.CheckTimeBegin, Model.CheckTimeEnd));
                }
                else
                {
                    cell.SetCellValue("檢查時間：");
                }

                row = worksheet.GetRow(88);

                for (int cellIndex = 2; cellIndex <= 20; cellIndex++)
                {
                    var valveID = (cellIndex - 1).ToString().PadLeft(2, '0');

                    var valve = Model.ValveList.FirstOrDefault(x => x.ID == valveID);

                    if (valve != null)
                    {
                        if (valve.Status == "開啟") {
                            row.GetCell(cellIndex).SetCellValue("V");
                        }
                    }
                }

                row = worksheet.GetRow(89);

                for (int cellIndex = 2; cellIndex <= 20; cellIndex++)
                {
                    var valveID = (cellIndex - 1).ToString().PadLeft(2, '0');

                    var valve = Model.ValveList.FirstOrDefault(x => x.ID == valveID);

                    if (valve != null)
                    {
                        if (valve.Status == "關閉")
                        {
                            row.GetCell(cellIndex).SetCellValue("V");
                        }
                    }
                }

                row = worksheet.GetRow(91);

                for (int cellIndex = 2; cellIndex <= 5; cellIndex++)
                {
                    var valveID = (cellIndex +18).ToString().PadLeft(2, '0');

                    var valve = Model.ValveList.FirstOrDefault(x => x.ID == valveID);

                    if (valve != null)
                    {
                        if (valve.Status == "開啟")
                        {
                            row.GetCell(cellIndex).SetCellValue("V");
                        }
                    }
                }

                row = worksheet.GetRow(92);

                for (int cellIndex = 2; cellIndex <= 5; cellIndex++)
                {
                    var valveID = (cellIndex + 18).ToString().PadLeft(2, '0');

                    var valve = Model.ValveList.FirstOrDefault(x => x.ID == valveID);

                    if (valve != null)
                    {
                        if (valve.Status == "關閉")
                        {
                            row.GetCell(cellIndex).SetCellValue("V");
                        }
                    }
                }

                row = worksheet.GetRow(95);
                row.GetCell(0).SetCellValue(Model.PipeStatus);

                row = worksheet.GetRow(97);
                row.GetCell(0).SetCellValue(Model.ExchangeStatus);


                #endregion

                var output = new ExcelExportModel(string.Format("每日儲槽區域暨儲槽動態巡檢紀錄表_{0}", Model.CheckDateString), Define.EnumExcelVersion._2007);
                

                using (FileStream fs = System.IO.File.OpenWrite(output.FullFileName))
                {
                    workBook.Write(fs);

                    fs.Close();
                }

                byte[] buff = null;

                using (var fs = System.IO.File.OpenRead(output.FullFileName))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        long numBytes = new FileInfo(output.FullFileName).Length;

                        buff = br.ReadBytes((int)numBytes);

                        br.Close();
                    }

                    fs.Close();
                }

                output.Data = buff;

                result.ReturnData(output);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }
    }
}
