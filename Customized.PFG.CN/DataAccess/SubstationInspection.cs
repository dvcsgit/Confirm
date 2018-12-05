using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.Util;
using DataAccess;
using Utility;
using Utility.Models;
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using Customized.PFG.CN.Models.SubstationInspection;



namespace Customized.PFG.CN.DataAccess
{
    public  class SubstationInspection
    {
        //查詢一個月數據
          public static List<List<CheckResult>> Query(QueryParameters Parameters)
        {
              List<List<CheckResult>> result=new List<List<CheckResult>>();
              List<string> valueData = new List<string>();
              try
              {

                  using (EDbEntities db = new EDbEntities())
                  {

                      string CheckStartDate = "";
                      string CheckEndDate = "";
                      if (int.Parse(Parameters.Month) < 10)
                      {
                          CheckStartDate = Parameters.Year + "0" + Parameters.Month + "01";
                          CheckEndDate = Parameters.Year + "0" + Parameters.Month + "31";
                      }
                      else
                      {
                          CheckStartDate = Parameters.Year + Parameters.Month + "01";
                          CheckEndDate = Parameters.Year + Parameters.Month + "31";
                      }

                      string[] equipmentName = EquipmentName.AllEquipment();

                      for (int i = 0; i < equipmentName.Length; i++)
                      {
                          string EName = equipmentName[i];
                          var checkdata = (from a in db.CheckResult
                                           
                                           where string.Compare(a.CheckDate, CheckStartDate) >= 0 && string.Compare(a.CheckDate, CheckEndDate) <= 0
                                           && a.EquipmentName == EName && a.CheckItemDescription.Contains("用電量") orderby a.CheckDate,a.CheckTime
                                           select new
                                           {
                                               a.Value,
                                               a.CheckDate
                                           }).ToList();
                       //   var sum = checkdata.Sum(x => x.Value); 
                          List<CheckResult> list = checkdata.ToList().ConvertAll<CheckResult>(item => new CheckResult()
                              {
                                  Value = item.Value,
                                  CheckDate=item.CheckDate
                              });
                          result.Add(list);
                      }
                  }
              }
              catch (Exception ex)
              {
                  var err = new Error(MethodBase.GetCurrentMethod(), ex);

                  Logger.Log(err);

                  RequestResult resultE = new RequestResult();

                  resultE.ReturnError(err);
              }
      
              return result;

        }

          public static List<List<CheckResult>> YDL(QueryParameters Parameters)
          {
              List<List<CheckResult>> list = new List<List<CheckResult>>();
              try
              {
                  using (EDbEntities db = new EDbEntities())
                  {
                      string CheckStartDate = "";
                      string CheckEndDate = "";
                      if (int.Parse(Parameters.Month) < 10)
                      {
                          CheckStartDate = Parameters.Year + "0" + Parameters.Month + "01";
                          CheckEndDate = Parameters.Year + "0" + Parameters.Month + "31";
                      }
                      else
                      {
                          CheckStartDate = Parameters.Year + Parameters.Month + "01";
                          CheckEndDate = Parameters.Year + Parameters.Month + "31";
                      }

                      string[] equipmentName = new string[] { "A饋主受電盤", "B饋主受電盤", "C饋主受電盤", "D饋主受電盤" };

                      for (int i = 0; i < equipmentName.Length; i++)
                      {
                          string EName = equipmentName[i];
                          var checkdata = (from a in db.CheckResult

                                           where string.Compare(a.CheckDate, CheckStartDate) >= 0 && string.Compare(a.CheckDate, CheckEndDate) <= 0
                                           && a.EquipmentName == EName && (a.CheckItemDescription.Contains("電壓") || a.CheckItemDescription.Contains("電流") || a.CheckItemDescription.Contains("功率因數") || a.CheckItemDescription == "功率")
                                           orderby a.CheckItemDescription,a.CheckTime
                                           select new
                                           {
                                               a.CheckItemDescription,
                                               a.Value,
                                               a.CheckDate
                                           }).ToList();
                          //   var sum = checkdata.Sum(x => x.Value); 
                          List<CheckResult> list1 = checkdata.ToList().ConvertAll<CheckResult>(item => new CheckResult()
                          {
                              CheckItemDescription = item.CheckItemDescription,
                              Value = item.Value,
                              CheckDate = item.CheckDate
                          });
                          list.Add(list1);
                      }

                  }
              }
              catch (Exception ex)
              {
                  var err = new Error(MethodBase.GetCurrentMethod(), ex);

                  Logger.Log(err);

                  RequestResult resultE = new RequestResult();

                  resultE.ReturnError(err);
              }


              return list;
          }

        //查詢室外氣溫
          public static List<List<CheckResult>> SWQW(QueryParameters Parameters)
          {
              List<List<CheckResult>> list = new List<List<CheckResult>>();
              try
              {
                  using (EDbEntities db = new EDbEntities())
                  {
                      string CheckStartDate = "";
                      string CheckEndDate = "";
                      if (int.Parse(Parameters.Month) < 10)
                      {
                          CheckStartDate = Parameters.Year + "0" + Parameters.Month + "01";
                          CheckEndDate = Parameters.Year + "0" + Parameters.Month + "31";
                      }
                      else
                      {
                          CheckStartDate = Parameters.Year + Parameters.Month + "01";
                          CheckEndDate = Parameters.Year + Parameters.Month + "31";
                      }

                        var checkdata = (from a in db.CheckResult

                                        where string.Compare(a.CheckDate, CheckStartDate) >= 0 && string.Compare(a.CheckDate, CheckEndDate) <= 0
                                        && a.CheckItemDescription.Contains("室外氣溫")
                                        orderby a.CheckItemDescription, a.CheckTime
                                        select new
                                        {
                                            a.Value,
                                            a.CheckDate
                                        }).ToList(); 
                        List<CheckResult> list1 = checkdata.ToList().ConvertAll<CheckResult>(item => new CheckResult()
                        {
                            Value = item.Value,
                            CheckDate = item.CheckDate
                        });
                        list.Add(list1);

                  }
              }
              catch (Exception ex)
              {
                  var err = new Error(MethodBase.GetCurrentMethod(), ex);

                  Logger.Log(err);

                  RequestResult resultE = new RequestResult();

                  resultE.ReturnError(err);
              }
              return list;
          }

        //純氧日用量
          public static List<List<CheckResult>> CYRYL(QueryParameters Parameters)
          {
              List<List<CheckResult>> list = new List<List<CheckResult>>();
              try
              {
                  using (EDbEntities db = new EDbEntities())
                  {
                      string CheckStartDate = "";
                      string CheckEndDate = "";
                      if (int.Parse(Parameters.Month) < 10)
                      {
                          CheckStartDate = Parameters.Year + "0" + Parameters.Month + "01";
                          CheckEndDate = Parameters.Year + "0" + Parameters.Month + "31";
                      }
                      else
                      {
                          CheckStartDate = Parameters.Year + Parameters.Month + "01";
                          CheckEndDate = Parameters.Year + Parameters.Month + "31";
                      }
                      string[] checkItemDescription = new string[] { "601熔爐純氧日用量", "602熔爐純氧日用量", "603熔爐純氧日用量", "604熔爐純氧日用量" };

                      for (int i = 0; i < checkItemDescription.Length; i++)
                      {
                          string CName = checkItemDescription[i];
                          var checkdata = (from a in db.CheckResult

                                           where string.Compare(a.CheckDate, CheckStartDate) >= 0 && string.Compare(a.CheckDate, CheckEndDate) <= 0
                                           && a.CheckItemDescription == CName orderby a.CheckItemDescription, a.CheckTime
                                           select new
                                           {
                                               a.CheckItemDescription,
                                               a.Value,
                                               a.CheckDate
                                           }).ToList();
                          //   var sum = checkdata.Sum(x => x.Value); 
                          List<CheckResult> list1 = checkdata.ToList().ConvertAll<CheckResult>(item => new CheckResult()
                          {
                              CheckItemDescription = item.CheckItemDescription,
                              Value = item.Value,
                              CheckDate = item.CheckDate
                          });
                          list.Add(list1);
                      }
                  }
              }
              catch (Exception ex)
              {
                  var err = new Error(MethodBase.GetCurrentMethod(), ex);

                  Logger.Log(err);

                  RequestResult resultE = new RequestResult();

                  resultE.ReturnError(err);
              }
              return list;
          }

        //查詢上月最後一天數據
        public static List<List<CheckResult>> LastMonth(QueryParameters Parameters)
        {
            List<List<CheckResult>> list = new List<List<CheckResult>>();
            try
            {

                using (EDbEntities db = new EDbEntities())
                {

                    string CheckStartDate = "";
                    string CheckEndDate = "";
                    string checkDate = "";
                    int Year = int.Parse(Parameters.Year);
                    int  Month =int.Parse(Parameters.Month);

                    if(Month==1)
                    {
                        int lastDay = DateTime.DaysInMonth(Year - 1, 12);

                        checkDate = Convert.ToString(Year - 1) + "12" + Convert.ToString(lastDay);

                        CheckStartDate = Convert.ToString(Year-1) +"12"+"01";
                        CheckEndDate = Convert.ToString(Year - 1) +"12" + "31";
                    }
                    else
                    {
                        if(Month<11)
                        {
                            int lastDay = DateTime.DaysInMonth(Year, Month - 1);

                            checkDate = Parameters.Year + "0" + Convert.ToString(Month - 1) + Convert.ToString(lastDay);

                            CheckStartDate = Parameters.Year + "0" + Convert.ToString(Month - 1) + "01";
                            CheckEndDate = Parameters.Year + "0" + Convert.ToString(Month - 1) + "31";
                        }
                        else
                        {
                            int lastDay = DateTime.DaysInMonth(Year, Month - 1);

                            checkDate = Parameters.Year + Convert.ToString(Month - 1) + Convert.ToString(lastDay);

                            CheckStartDate = Parameters.Year + Convert.ToString(Month - 1) + "01";
                            CheckEndDate = Parameters.Year + Convert.ToString(Month - 1) + "31";
                        }
                    }
  
                    string[] equipmentName = EquipmentName.AllEquipment();

                    for (int i = 0; i < equipmentName.Length; i++)
                    {
                        string EName = equipmentName[i];
                        var checkdata = (from a in db.CheckResult
                                         where a.CheckDate == checkDate
                                         && a.EquipmentName == EName && a.CheckItemDescription.Contains("用電量")
                                         orderby a.CheckTime
                                         select new
                                         {
                                             a.Value,
                                             a.CheckDate
                                         }).ToList();

                        List<CheckResult> list1 = checkdata.ToList().ConvertAll<CheckResult>(item => new CheckResult()
                        {
                            Value = item.Value,
                            CheckDate = item.CheckDate
                        });

                        list.Add(list1);
                    }
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                RequestResult resultE = new RequestResult();

                resultE.ReturnError(err);
            }


            return list;
        }

        public static RequestResult Export(QueryParameters Parameters,Define.EnumExcelVersion ExcelVersion)
        {
            List<List<CheckResult>> list = Query(Parameters);

            List<List<CheckResult>> lastMonth = LastMonth(Parameters);

            List<List<CheckResult>> YDLList = YDL(Parameters);

            List<List<CheckResult>> SWQWList = SWQW(Parameters);

            List<List<CheckResult>> CYRYLList = CYRYL(Parameters);

            RequestResult result = new RequestResult();


            IWorkbook workBook=null;
            

            //string strpath = Config.EquipmentMaintenanceSubstationInspectionReports;

            FileInfo fi = new FileInfo(Config.EquipmentMaintenanceSubstationInspectionReports);
            
            string FilePath ;

            string fileName = "變電站巡檢抄錶記錄" + Parameters.Year + Parameters.Month + ".xls";

            if (!Directory.Exists(FilePath = Path.Combine(Config.TempFolder, Guid.NewGuid().ToString())))
            {
                Directory.CreateDirectory(FilePath);
            }

            fi.CopyTo(Path.Combine(FilePath, fileName), true);

            FileStream fs = System.IO.File.Open(Path.Combine(FilePath, fileName), FileMode.Open, FileAccess.Read);

              
            if (ExcelVersion == Define.EnumExcelVersion._2003)
            {
                workBook = new HSSFWorkbook(fs);
            }

            if (ExcelVersion == Define.EnumExcelVersion._2007)
            {
                workBook = new HSSFWorkbook(fs);
            }
            fs.Close();

            ISheet sheet = workBook.GetSheetAt(3);
            ISheet sheet2 = workBook.GetSheetAt(1);

            ICellStyle titleCellStyle = workBook.CreateCellStyle();
            IFont titleFont = workBook.CreateFont();
            titleFont.FontName = "新細明體";
            titleFont.FontHeightInPoints = 9;
            titleFont.Underline = FontUnderlineType.Single;
            titleFont.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
            titleCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Right;
            titleCellStyle.BorderTop = BorderStyle.None;
            titleCellStyle.BorderBottom = BorderStyle.None;
            titleCellStyle.BorderLeft = BorderStyle.None;
            titleCellStyle.BorderRight = BorderStyle.None;
            titleCellStyle.SetFont(titleFont);

            string month;
            if (int.Parse(Parameters.Month) < 10)
            {
                month = "0" + Parameters.Month;
            }
            else
            {
                month = Parameters.Month;
            }
            sheet2.GetRow(0).GetCell(0).SetCellValue("電力統計（" + Parameters.Year.Substring(1, 3) + "年" + month + "月）                                                                                                               單位：MWH");
            sheet2.GetRow(0).GetCell(0).CellStyle = titleCellStyle;
            

            //for (int i = 2; i < 94; i++)
            //{
            //    for (int j = 2; j < 33; j++)
            //    {

            //        IRow row = sheet.GetRow(i);
            //        ICell cell = row.GetCell(j);
            //        cell.SetCellValue("");
            //    }
            //}
            //上月最後一天數據
            for (int i = 0; i < lastMonth.Count;i++ )
            {
                List<CheckResult> last = lastMonth[i];
                for (int ii = 0; ii < last.Count;ii++ )
                {
                    IRow row = sheet.GetRow(i + 2);
                    ICell cell = row.GetCell(2);
                    DecimalFormat df = new DecimalFormat("0.000");

                    var value = df.Format(last[ii].Value);

                    cell.SetCellValue(Convert.ToDouble(value));
                }

                //IRow row = sheet.GetRow(i+2);
                //ICell cell = row.GetCell(2);
                //cell.SetCellValue(lastMonth[i]);
            }
            //變壓器數據
            for (int i = 0; i < list.Count; i++)
            {
                List<CheckResult> dataVale = list[i];
                if(i==70)
                {
                    for (int j = 0; j < dataVale.Count; j++)
                    {
                        IRow row = sheet.GetRow(i + 7);
                        int dateIndex =Convert.ToInt32(dataVale[j].CheckDate.Substring(dataVale[j].CheckDate.Length - 2, 2));
                        ICell cell = row.GetCell(dateIndex + 2);

                        DecimalFormat df = new DecimalFormat("0.000");

                        var value = df.Format(dataVale[j].Value);
                        cell.SetCellValue(Convert.ToDouble(value));
                    }
                }
                else
                {
                    for (int j = 0; j < dataVale.Count; j++)
                    {
                        IRow row = sheet.GetRow(i + 2);
                        int dateIndex = Convert.ToInt32(dataVale[j].CheckDate.Substring(dataVale[j].CheckDate.Length - 2, 2));
                        ICell cell = row.GetCell(dateIndex + 2);

                        DecimalFormat df = new DecimalFormat("0.000");

                        var value = df.Format(dataVale[j].Value);

                        cell.SetCellValue(Convert.ToDouble(value));
                    }
                }
               

            }
            //室外氣溫
            for (int k = 0; k < SWQWList.Count; k++)
            {
                List<CheckResult> SWQWList2 = SWQWList[k];
                for (int l = 0; l < SWQWList2.Count; l++)
                {
                    IRow row = sheet.GetRow(77);
                    int dateIndex = Convert.ToInt32(SWQWList2[l].CheckDate.Substring(SWQWList2[l].CheckDate.Length - 2, 2));
                    ICell cell = row.GetCell(dateIndex + 2);
                    cell.SetCellValue(Convert.ToDouble(SWQWList2[l].Value.ToString()));
                }
            }
            //純氧日用量
            for (int c = 0; c < CYRYLList.Count; c++)
            {
                List<CheckResult> CYRYLList2 = CYRYLList[c];
                IRow row;
                if (c == 0)
                {
                    for (int d = 0; d < CYRYLList2.Count; d++)
                    {
                        row = sheet.GetRow(74);
                        int dateIndex = Convert.ToInt32(CYRYLList2[d].CheckDate.Substring(CYRYLList2[d].CheckDate.Length - 2, 2));
                        ICell cell = row.GetCell(dateIndex + 2);
                        DecimalFormat df = new DecimalFormat("0.000");

                        var value = df.Format(CYRYLList2[d].Value);
                        cell.SetCellValue(Convert.ToDouble(value));
                    }
                }
                if (c == 1)
                {
                    for (int d = 0; d < CYRYLList2.Count; d++)
                    {
                        row = sheet.GetRow(72);
                        int dateIndex = Convert.ToInt32(CYRYLList2[d].CheckDate.Substring(CYRYLList2[d].CheckDate.Length - 2, 2));
                        ICell cell = row.GetCell(dateIndex + 2);
                        DecimalFormat df = new DecimalFormat("0.000");

                        var value = df.Format(CYRYLList2[d].Value);
                        cell.SetCellValue(Convert.ToDouble(value));
                    }
                }
                if (c == 2)
                {
                    for (int d = 0; d < CYRYLList2.Count; d++)
                    {
                        row = sheet.GetRow(75);
                        int dateIndex = Convert.ToInt32(CYRYLList2[d].CheckDate.Substring(CYRYLList2[d].CheckDate.Length - 2, 2));
                        ICell cell = row.GetCell(dateIndex + 2);
                        DecimalFormat df = new DecimalFormat("0.000");

                        var value = df.Format(CYRYLList2[d].Value);
                        cell.SetCellValue(Convert.ToDouble(value));
                    }
                }
                if (c == 3)
                {
                    for (int d = 0; d < CYRYLList2.Count; d++)
                    {
                        row = sheet.GetRow(73);
                        int dateIndex = Convert.ToInt32(CYRYLList2[d].CheckDate.Substring(CYRYLList2[d].CheckDate.Length - 2, 2));
                        ICell cell = row.GetCell(dateIndex + 2);
                        DecimalFormat df = new DecimalFormat("0.000");

                        var value = df.Format(CYRYLList2[d].Value);
                        cell.SetCellValue(Convert.ToDouble(value));
                    }
                }
                
            }
            //ABCD功率、電壓、電流、功率因數
            for (int a = 0; a < YDLList.Count; a++)
            {
                List<CheckResult> YdlList2 = YDLList[a];
                IRow row;
                if (a == 0)
                {
                    for (int b = 0; b < YdlList2.Count; b++)
                    {
                        if (YdlList2[b].CheckItemDescription == "電壓")
                        {
                            row = sheet.GetRow(78);
                            int dateIndex = Convert.ToInt32(YdlList2[b].CheckDate.Substring(YdlList2[b].CheckDate.Length - 2, 2));
                            ICell cell = row.GetCell(dateIndex + 2);

                            DecimalFormat df = new DecimalFormat("0.000");

                            var value = df.Format(YdlList2[b].Value);
                            cell.SetCellValue(Convert.ToDouble(value));

                            //cell.SetCellValue(Convert.ToDouble(YdlList2[b].Result.ToString()));

                        }
                        if (YdlList2[b].CheckItemDescription == "電流")
                        {
                            row = sheet.GetRow(79);
                            int dateIndex = Convert.ToInt32(YdlList2[b].CheckDate.Substring(YdlList2[b].CheckDate.Length - 2, 2));
                            ICell cell = row.GetCell(dateIndex + 2);

                            DecimalFormat df = new DecimalFormat("0.000");

                            var value = df.Format(YdlList2[b].Value);
                            cell.SetCellValue(Convert.ToDouble(value));

                        }
                        if (YdlList2[b].CheckItemDescription.Contains("功率因數"))
                        {
                            row = sheet.GetRow(80);
                            int dateIndex = Convert.ToInt32(YdlList2[b].CheckDate.Substring(YdlList2[b].CheckDate.Length - 2, 2));
                            ICell cell = row.GetCell(dateIndex + 2);

                            DecimalFormat df = new DecimalFormat("0.00");

                            var value = df.Format(YdlList2[b].Value);
                            cell.SetCellValue(Convert.ToDouble(value));

                        }
                        if (YdlList2[b].CheckItemDescription == "功率")
                        {
                            row = sheet.GetRow(81);
                            int dateIndex = Convert.ToInt32(YdlList2[b].CheckDate.Substring(YdlList2[b].CheckDate.Length - 2, 2));
                            ICell cell = row.GetCell(dateIndex + 2);

                            DecimalFormat df = new DecimalFormat("0.000");

                            var value = df.Format(YdlList2[b].Value);
                            cell.SetCellValue(Convert.ToDouble(value));

                        }
                    }
                }

                if (a == 1)
                {
                    for (int b = 0; b < YdlList2.Count; b++)
                    {
                        if (YdlList2[b].CheckItemDescription == "電壓")
                        {
                            row = sheet.GetRow(82);
                            int dateIndex = Convert.ToInt32(YdlList2[b].CheckDate.Substring(YdlList2[b].CheckDate.Length - 2, 2));
                            ICell cell = row.GetCell(dateIndex + 2);

                            DecimalFormat df = new DecimalFormat("0.000");

                            var value = df.Format(YdlList2[b].Value);
                            cell.SetCellValue(Convert.ToDouble(value));

                        }
                        if (YdlList2[b].CheckItemDescription == "電流")
                        {
                            row = sheet.GetRow(83);
                            int dateIndex = Convert.ToInt32(YdlList2[b].CheckDate.Substring(YdlList2[b].CheckDate.Length - 2, 2));
                            ICell cell = row.GetCell(dateIndex + 2);

                            DecimalFormat df = new DecimalFormat("0.000");

                            var value = df.Format(YdlList2[b].Value);
                            cell.SetCellValue(Convert.ToDouble(value));

                        }
                        if (YdlList2[b].CheckItemDescription.Contains("功率因數"))
                        {
                            row = sheet.GetRow(84);
                            int dateIndex = Convert.ToInt32(YdlList2[b].CheckDate.Substring(YdlList2[b].CheckDate.Length - 2, 2));
                            ICell cell = row.GetCell(dateIndex + 2);

                            DecimalFormat df = new DecimalFormat("0.00");

                            var value = df.Format(YdlList2[b].Value);
                            cell.SetCellValue(Convert.ToDouble(value));

                        }
                        if (YdlList2[b].CheckItemDescription == "功率")
                        {
                            row = sheet.GetRow(85);
                            int dateIndex = Convert.ToInt32(YdlList2[b].CheckDate.Substring(YdlList2[b].CheckDate.Length - 2, 2));
                            ICell cell = row.GetCell(dateIndex + 2);

                            DecimalFormat df = new DecimalFormat("0.000");

                            var value = df.Format(YdlList2[b].Value);
                            cell.SetCellValue(Convert.ToDouble(value));

                        }
                    }
                }

                if (a == 2)
                {
                    for (int b = 0; b < YdlList2.Count; b++)
                    {
                        if (YdlList2[b].CheckItemDescription == "電壓")
                        {
                            row = sheet.GetRow(86);
                            int dateIndex = Convert.ToInt32(YdlList2[b].CheckDate.Substring(YdlList2[b].CheckDate.Length - 2, 2));
                            ICell cell = row.GetCell(dateIndex + 2);

                            DecimalFormat df = new DecimalFormat("0.000");

                            var value = df.Format(YdlList2[b].Value);
                            cell.SetCellValue(Convert.ToDouble(value));

                        }
                        if (YdlList2[b].CheckItemDescription == "電流")
                        {
                            row = sheet.GetRow(87);
                            int dateIndex = Convert.ToInt32(YdlList2[b].CheckDate.Substring(YdlList2[b].CheckDate.Length - 2, 2));
                            ICell cell = row.GetCell(dateIndex + 2);

                            DecimalFormat df = new DecimalFormat("0.000");

                            var value = df.Format(YdlList2[b].Value);
                            cell.SetCellValue(Convert.ToDouble(value));

                        }
                        if (YdlList2[b].CheckItemDescription.Contains("功率因數"))
                        {
                            row = sheet.GetRow(88);
                            int dateIndex = Convert.ToInt32(YdlList2[b].CheckDate.Substring(YdlList2[b].CheckDate.Length - 2, 2));
                            ICell cell = row.GetCell(dateIndex + 2);

                            DecimalFormat df = new DecimalFormat("0.00");

                            var value = df.Format(YdlList2[b].Value);
                            cell.SetCellValue(Convert.ToDouble(value));

                        }
                        if (YdlList2[b].CheckItemDescription == "功率")
                        {
                            row = sheet.GetRow(89);
                            int dateIndex = Convert.ToInt32(YdlList2[b].CheckDate.Substring(YdlList2[b].CheckDate.Length - 2, 2));
                            ICell cell = row.GetCell(dateIndex + 2);

                            DecimalFormat df = new DecimalFormat("0.000");

                            var value = df.Format(YdlList2[b].Value);
                            cell.SetCellValue(Convert.ToDouble(value));

                        }
                    }
                }

                if (a == 3)
                {
                    for (int b = 0; b < YdlList2.Count; b++)
                    {
                        if (YdlList2[b].CheckItemDescription == "電壓")
                        {
                            row = sheet.GetRow(90);
                            int dateIndex = Convert.ToInt32(YdlList2[b].CheckDate.Substring(YdlList2[b].CheckDate.Length - 2, 2));
                            ICell cell = row.GetCell(dateIndex + 2);

                            DecimalFormat df = new DecimalFormat("0.000");

                            var value = df.Format(YdlList2[b].Value);
                            cell.SetCellValue(Convert.ToDouble(value));

                        }
                        if (YdlList2[b].CheckItemDescription == "電流")
                        {
                            row = sheet.GetRow(91);
                            int dateIndex = Convert.ToInt32(YdlList2[b].CheckDate.Substring(YdlList2[b].CheckDate.Length - 2, 2));
                            ICell cell = row.GetCell(dateIndex + 2);

                            DecimalFormat df = new DecimalFormat("0.000");

                            var value = df.Format(YdlList2[b].Value);
                            cell.SetCellValue(Convert.ToDouble(value));

                        }
                        if (YdlList2[b].CheckItemDescription.Contains("功率因數"))
                        {
                            row = sheet.GetRow(92);
                            int dateIndex = Convert.ToInt32(YdlList2[b].CheckDate.Substring(YdlList2[b].CheckDate.Length - 2, 2));
                            ICell cell = row.GetCell(dateIndex + 2);

                            DecimalFormat df = new DecimalFormat("0.00");

                            var value = df.Format(YdlList2[b].Value);
                            cell.SetCellValue(Convert.ToDouble(value));

                        }
                        if (YdlList2[b].CheckItemDescription == "功率")
                        {
                            row = sheet.GetRow(93);
                            int dateIndex = Convert.ToInt32(YdlList2[b].CheckDate.Substring(YdlList2[b].CheckDate.Length - 2, 2));
                            ICell cell = row.GetCell(dateIndex + 2);

                            DecimalFormat df = new DecimalFormat("0.000");

                            var value = df.Format(YdlList2[b].Value);
                            cell.SetCellValue(Convert.ToDouble(value));

                        }
                    }
                }


            }

            sheet.ForceFormulaRecalculation = true;
            sheet2.ForceFormulaRecalculation = true;

            string dateVale = "";
            if (int.Parse(Parameters.Month)<10)
            {
                dateVale = Parameters.Year + "0" + Parameters.Month;
            }
            else
            {
                dateVale = Parameters.Year + Parameters.Month;
            }
            string name = "變電站巡檢抄錶記錄_" + dateVale;
            var output = new ExcelExportModel(name, ExcelVersion);

            using (FileStream fss = System.IO.File.OpenWrite(Path.Combine(FilePath, fileName)))
            {
                workBook.Write(fss);
            }
            byte[] data=null;
            using (var fss = System.IO.File.OpenRead(Path.Combine(FilePath, fileName)))

              {
                  using (BinaryReader br = new BinaryReader(fss))
                   {
                       long numBytes = new FileInfo(Path.Combine(FilePath, fileName)).Length;

                        data = br.ReadBytes((int)numBytes);

                        br.Close();
                   }

                    fss.Close();
                }

                output.Data = data;

                result.ReturnData(output);

                return result;
        }

        public static string Validate(string Year,string Month)
          {
           string prompt=string.Empty;
            
            if(!string.IsNullOrEmpty(Year))
            {
                string yearNow = DateTime.Now.Year.ToString() ;
                if(Year.Length!=4||string.Compare(yearNow,Year)<0)
                {
                    prompt="1";//Resources.Resource.Prompt
                }
                else
                {
                    prompt="0";
                }
            }
            else
            {
                prompt="1";//Resources.Resource.YearRequired
            }
            if(!string.IsNullOrEmpty(Month))
            {
                if (Convert.ToInt32(Month) > 12 || Convert.ToInt32(Month) < 1)
                {
                    prompt=prompt+"1";//Resources.Resource.Prompt
                }
                else
                {
                    prompt = prompt + "0";
                }
            }
            else
            {
                prompt = prompt + "1";//Resources.Resource.MonthRequired
            }
          

            return prompt;
          }
         
    }
}
