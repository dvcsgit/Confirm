using DbEntity.ASE;
using Models.ASE.QA.MSAReport;
using Models.Authenticated;
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
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace DataAccess.ASE.QA
{
    public class MSAReportHelper
    {
        public static RequestResult GetQueryFormModel(List<Models.Shared.Organization> OrganizationList)
        {
            RequestResult result = new RequestResult();

            try
            {
                var factoryList = OrganizationDataAccessor.GetFactoryList(OrganizationList);

                var model = new QueryFormModel()
                {
                    FactorySelectItemList = new List<SelectListItem>() {
                     Define.DefaultSelectListItem(Resources.Resource.SelectAll)
                    }
                };

                foreach (var factory in factoryList)
                {
                    model.FactorySelectItemList.Add(new SelectListItem()
                    {
                        Text = factory.Description,
                        Value = factory.UniqueID
                    });
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

        public static RequestResult Query(List<Models.Shared.Organization> OrganizationList, QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var isQA = Account.UserAuthGroupList.Contains("QA-Verify") || Account.UserAuthGroupList.Contains("QA") || Account.UserAuthGroupList.Contains("QA-FullQuery");

                var model = new GridViewModel()
                {
                     
                };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = (from f in db.QA_MSAFORM
                                 join e in db.QA_EQUIPMENT
                                 on f.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                 where f.MSADATE.HasValue && f.STATUS == "5"
                                 select new
                                 {
                                     e.ORGANIZATIONUNIQUEID,
                                     f.MSAICHI,
                                     e.MSACYCLE,
                                     f.MSADATE,
                                     e.MSACALNO,
                                     e.SERIALNO,
                                     e.BRAND,
                                     e.MODEL,
                                     f.CHARACTERISITIC,
                                     f.TYPE,
                                     f.SUBTYPE,
                                     f.MSARESPONSORID,
                                     f.STABILITYRESULT,
                                     f.STABILITYVAL,
                                     f.STABILITYSTDEV,
                                     f.BIASRESULT,
                                     f.BIASTSTATISTIC,
                                     f.COUNTAALARM,
                                     f.COUNTAEFFECTIVE,
                                     f.COUNTAERROR,
                                     f.COUNTBALARM,
                                     f.COUNTBEFFECTIVE,
                                     f.COUNTBERROR,
                                     f.COUNTCALARM,
                                     f.COUNTCEFFECTIVE,
                                     f.COUNTCERROR,
                                     f.COUNTRESULT,
                                     f.GRRH,
                                     f.GRRHNDC,
                                     f.GRRHRESULT,
                                     f.GRRM,
                                     f.GRRMNDC,
                                     f.GRRMRESULT,
                                     f.GRRL,
                                     f.GRRLNDC,
                                     f.GRRLRESULT,
                                     f.KAPPAA,
                                     f.KAPPAB,
                                     f.KAPPAC,
                                     f.KAPPARESULT,
                                     f.LINEARITYRESULT,
                                     f.LINEARITYTA,
                                     f.LINEARITYTB,
                                     f.ANOVAHRESULT,
                                     f.ANOVAHNDC,
                                     f.ANOVAHPT,
                                     f.ANOVAHTV,
                                     f.ANOVALRESULT,
                                     f.ANOVALNDC,
                                     f.ANOVALPT,
                                     f.ANOVALTV,
                                     f.ANOVAMRESULT,
                                     f.ANOVAMNDC,
                                     f.ANOVAMPT,
                                     f.ANOVAMTV
                                 }).AsQueryable();

                    if (!isQA)
                    {
                        query = query.Where(x => Account.QueryableOrganizationUniqueIDList.Contains(x.ORGANIZATIONUNIQUEID)).AsQueryable();
                    }

                    if (Parameters.MSABeginDate.HasValue)
                    {
                        query = query.Where(x => DateTime.Compare(x.MSADATE.Value, Parameters.MSABeginDate.Value) >= 0);
                    }

                    if (Parameters.MSAEndDate.HasValue)
                    {
                        query = query.Where(x => DateTime.Compare(x.MSADATE.Value, Parameters.MSAEndDate.Value) < 0);
                    }

                    if (!string.IsNullOrEmpty(Parameters.MSACalNo))
                    {
                        query = query.Where(x => x.MSACALNO.Contains(Parameters.MSACalNo));
                    }

                    if (!string.IsNullOrEmpty(Parameters.SerialNo))
                    {
                        query = query.Where(x => x.SERIALNO.Contains(Parameters.SerialNo));
                    }

                    if (!string.IsNullOrEmpty(Parameters.FactoryUniqueID))
                    {
                        var downStream = OrganizationDataAccessor.GetDownStreamOrganizationList(OrganizationList, Parameters.FactoryUniqueID, true);

                        query = query.Where(x => downStream.Contains(x.ORGANIZATIONUNIQUEID));
                    }

                    if (!string.IsNullOrEmpty(Parameters.Brand))
                    {
                        query = query.Where(x => x.BRAND.Contains(Parameters.Brand));
                    }

                    if (!string.IsNullOrEmpty(Parameters.Model))
                    {
                        query = query.Where(x => x.MODEL.Contains(Parameters.Model));
                    }

                    if (!string.IsNullOrEmpty(Parameters.MSAIchi))
                    {
                        query = query.Where(x => x.MSAICHI.Contains(Parameters.MSAIchi));
                    }

                    var queryResults = query.ToList();

                    var tmp1 = queryResults.Where(x => x.TYPE == "1").ToList();

                    foreach (var tmp in tmp1)
                    {
                        var msaReponsor = db.ACCOUNT.FirstOrDefault(x => x.ID == tmp.MSARESPONSORID);

                        var managerName = string.Empty;

                        if (msaReponsor != null)
                        {
                            var manager = db.ACCOUNT.FirstOrDefault(x => x.ID == msaReponsor.MANAGERID);

                            if (manager != null)
                            {
                                managerName = manager.NAME;
                            }
                        }

                        if (tmp.SUBTYPE == "1")
                        {
                            var item = new AvgAndRGridItem()
                            {
                                Cycle = tmp.MSACYCLE.Value,
                                MSADate = tmp.MSADATE,
                                MSACalNo = tmp.MSACALNO,
                                MSAIchi = tmp.MSAICHI,
                                MSACharacteristic = tmp.CHARACTERISITIC,
                                Model = tmp.MODEL,
                                SerialNo = tmp.SERIALNO,
                                Brand = tmp.BRAND,
                                Factory = OrganizationDataAccessor.GetFactory(OrganizationList, tmp.ORGANIZATIONUNIQUEID),
                                MSAResponsorID = tmp.MSARESPONSORID,
                                MSAResponsorName = msaReponsor != null ? msaReponsor.NAME : string.Empty,
                                ManagerID = msaReponsor != null ? msaReponsor.MANAGERID : string.Empty,
                                ManagerName = managerName
                            };

                            if (!string.IsNullOrEmpty(tmp.STABILITYRESULT))
                            {
                                item.StabilityResult = new StabilityResult()
                                {
                                    Result = tmp.STABILITYRESULT,
                                    Stability = tmp.STABILITYVAL,
                                    Stdev = tmp.STABILITYSTDEV
                                };
                            }
                            else
                            {
                                item.StabilityResult = null;
                            }

                            if (!string.IsNullOrEmpty(tmp.BIASRESULT))
                            {
                                item.BiasResult = new BiasResult()
                                {
                                    SignificantT = Convert.ToDecimal(2.14479),
                                    TStatistic = tmp.BIASTSTATISTIC,
                                    Result = tmp.BIASRESULT
                                };
                            }
                            else
                            {
                                item.BiasResult = null;
                            }

                            if (!string.IsNullOrEmpty(tmp.LINEARITYRESULT))
                            {
                                item.LinearityResult = new LinearityResult()
                                {
                                    ta = tmp.LINEARITYTA,
                                    tb = tmp.LINEARITYTB,
                                    t58 = Convert.ToDecimal(2.00172),
                                    Result = tmp.LINEARITYRESULT
                                };
                            }
                            else
                            {
                                item.LinearityResult = null;
                            }

                            if (!string.IsNullOrEmpty(tmp.GRRHRESULT))
                            {
                                item.GRRHResult = new GRRResult()
                                {
                                    GRR = tmp.GRRH,
                                    ndc = tmp.GRRHNDC,
                                    Result = tmp.GRRHRESULT
                                };
                            }
                            else
                            {
                                item.GRRHResult = null;
                            }

                            if (!string.IsNullOrEmpty(tmp.GRRMRESULT))
                            {
                                item.GRRMResult = new GRRResult()
                                {
                                    GRR = tmp.GRRM,
                                    ndc = tmp.GRRMNDC,
                                    Result = tmp.GRRMRESULT
                                };
                            }
                            else
                            {
                                item.GRRMResult = null;
                            }

                            if (!string.IsNullOrEmpty(tmp.GRRLRESULT))
                            {
                                item.GRRLResult = new GRRResult()
                                {
                                    GRR = tmp.GRRL,
                                    ndc = tmp.GRRLNDC,
                                    Result = tmp.GRRLRESULT
                                };
                            }
                            else
                            {
                                item.GRRLResult = null;
                            }

                            model.AvgAndRItemList.Add(item);
                        }

                        if (tmp.SUBTYPE == "2")
                        {
                            var item = new AnovaGridItem()
                            {
                                Cycle = tmp.MSACYCLE.Value,
                                MSADate = tmp.MSADATE,
                                MSACalNo = tmp.MSACALNO,
                                MSAIchi = tmp.MSAICHI,
                                MSACharacteristic = tmp.CHARACTERISITIC,
                                Model = tmp.MODEL,
                                SerialNo = tmp.SERIALNO,
                                Brand = tmp.BRAND,
                                Factory = OrganizationDataAccessor.GetFactory(OrganizationList, tmp.ORGANIZATIONUNIQUEID),
                                MSAResponsorID = tmp.MSARESPONSORID,
                                MSAResponsorName = msaReponsor != null ? msaReponsor.NAME : string.Empty,
                                ManagerID = msaReponsor != null ? msaReponsor.MANAGERID : string.Empty,
                                ManagerName = managerName
                            };

                            if (!string.IsNullOrEmpty(tmp.STABILITYRESULT))
                            {
                                item.StabilityResult = new StabilityResult()
                                {
                                    Result = tmp.STABILITYRESULT,
                                    Stability = tmp.STABILITYVAL,
                                    Stdev = tmp.STABILITYSTDEV
                                };
                            }
                            else
                            {
                                item.StabilityResult = null;
                            }

                            if (!string.IsNullOrEmpty(tmp.BIASRESULT))
                            {
                                item.BiasResult = new BiasResult()
                                {
                                    SignificantT = Convert.ToDecimal(2.14479),
                                    TStatistic = tmp.BIASTSTATISTIC,
                                    Result = tmp.BIASRESULT
                                };
                            }
                            else
                            {
                                item.BiasResult = null;
                            }

                            if (!string.IsNullOrEmpty(tmp.LINEARITYRESULT))
                            {
                                item.LinearityResult = new LinearityResult()
                                {
                                    ta = tmp.LINEARITYTA,
                                    tb = tmp.LINEARITYTB,
                                    t58 = Convert.ToDecimal(2.00172),
                                    Result = tmp.LINEARITYRESULT
                                };
                            }
                            else
                            {
                                item.LinearityResult = null;
                            }

                            if (!string.IsNullOrEmpty(tmp.GRRHRESULT))
                            {
                                item.GRRHResult = new GRRResult()
                                {
                                    GRR = tmp.GRRH,
                                    ndc = tmp.GRRHNDC,
                                    Result = tmp.GRRHRESULT
                                };
                            }
                            else
                            {
                                item.GRRHResult = null;
                            }

                            if (!string.IsNullOrEmpty(tmp.GRRMRESULT))
                            {
                                item.GRRMResult = new GRRResult()
                                {
                                    GRR = tmp.GRRM,
                                    ndc = tmp.GRRMNDC,
                                    Result = tmp.GRRMRESULT
                                };
                            }
                            else
                            {
                                item.GRRMResult = null;
                            }

                            if (!string.IsNullOrEmpty(tmp.GRRLRESULT))
                            {
                                item.GRRLResult = new GRRResult()
                                {
                                    GRR = tmp.GRRL,
                                    ndc = tmp.GRRLNDC,
                                    Result = tmp.GRRLRESULT
                                };
                            }
                            else
                            {
                                item.GRRLResult = null;
                            }

                            if (!string.IsNullOrEmpty(tmp.ANOVAHRESULT))
                            {
                                item.AnovaHResult = new AnovaResult()
                                {
                                    TV = tmp.ANOVAHTV,
                                    PT = tmp.ANOVAHPT,
                                    NDC = tmp.ANOVAHNDC,
                                    Result = tmp.ANOVAHRESULT
                                };
                            }
                            else
                            {
                                item.AnovaHResult = null;
                            }

                            if (!string.IsNullOrEmpty(tmp.ANOVAMRESULT))
                            {
                                item.AnovaMResult = new AnovaResult()
                                {
                                    TV = tmp.ANOVAMTV,
                                    PT = tmp.ANOVAMPT,
                                    NDC = tmp.ANOVAMNDC,
                                    Result = tmp.ANOVAMRESULT
                                };
                            }
                            else
                            {
                                item.AnovaMResult = null;
                            }

                            if (!string.IsNullOrEmpty(tmp.ANOVALRESULT))
                            {
                                item.AnovaLResult = new AnovaResult()
                                {
                                    TV = tmp.ANOVALTV,
                                    PT = tmp.ANOVALPT,
                                    NDC = tmp.ANOVALNDC,
                                    Result = tmp.ANOVALRESULT
                                };
                            }
                            else
                            {
                                item.AnovaLResult = null;
                            }

                            model.AnovaItemList.Add(item);
                        }
                    }

                    var tmp2 = queryResults.Where(x => x.TYPE == "2").ToList();

                    foreach (var tmp in tmp2)
                    {
                        var msaReponsor = db.ACCOUNT.FirstOrDefault(x => x.ID == tmp.MSARESPONSORID);

                        var managerName = string.Empty;

                        if (msaReponsor != null)
                        {
                            var manager = db.ACCOUNT.FirstOrDefault(x => x.ID == msaReponsor.MANAGERID);

                            if (manager != null)
                            {
                                managerName = manager.NAME;
                            }
                        }

                        var item = new GoOnGoGridItem()
                        {
                            Cycle = tmp.MSACYCLE.Value,
                            MSADate = tmp.MSADATE,
                            MSACalNo = tmp.MSACALNO,
                            MSAIchi = tmp.MSAICHI,
                            MSACharacteristic = tmp.CHARACTERISITIC,
                            Model = tmp.MODEL,
                            SerialNo = tmp.SERIALNO,
                            Brand = tmp.BRAND,
                            Factory = OrganizationDataAccessor.GetFactory(OrganizationList, tmp.ORGANIZATIONUNIQUEID),
                            MSAResponsorID = tmp.MSARESPONSORID,
                            MSAResponsorName = msaReponsor != null ? msaReponsor.NAME : string.Empty,
                            ManagerID = msaReponsor != null ? msaReponsor.MANAGERID : string.Empty,
                            ManagerName = managerName
                        };

                        if (!string.IsNullOrEmpty(tmp.COUNTRESULT))
                        {
                            item.CountResult = new CountResult()
                            {
                                CountAAlarm = tmp.COUNTAALARM,
                                CountAEffective = tmp.COUNTAEFFECTIVE,
                                CountAError = tmp.COUNTAERROR,
                                CountBAlarm = tmp.COUNTBALARM,
                                CountBEffective = tmp.COUNTBEFFECTIVE,
                                CountBError = tmp.COUNTBERROR,
                                CountCAlarm = tmp.COUNTCALARM,
                                CountCEffective = tmp.COUNTCEFFECTIVE,
                                CountCError = tmp.COUNTCERROR,
                                KappaA = tmp.KAPPAA,
                                KappaB = tmp.KAPPAB,
                                KappaC = tmp.KAPPAC,
                                KappaResult = tmp.KAPPARESULT,
                                Result = tmp.COUNTRESULT
                            };
                        }
                        else
                        {
                            item.CountResult = null;
                        }

                        model.GoOnGoItemList.Add(item);
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

        public static RequestResult Export(GridViewModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                IWorkbook workBook = new XSSFWorkbook();

                #region Header Style
                var headerFont = workBook.CreateFont();
                headerFont.Color = HSSFColor.Black.Index;
                headerFont.Boldweight = (short)FontBoldWeight.Bold;
                headerFont.FontHeightInPoints = 12;

                var headerStyle = workBook.CreateCellStyle();
                headerStyle.FillForegroundColor = HSSFColor.Grey25Percent.Index;
                headerStyle.FillPattern = FillPattern.SolidForeground;
                headerStyle.BorderTop = BorderStyle.Thin;
                headerStyle.BorderBottom = BorderStyle.Thin;
                headerStyle.BorderLeft = BorderStyle.Thin;
                headerStyle.BorderRight = BorderStyle.Thin;
                headerStyle.SetFont(headerFont);
                #endregion

                #region Cell Style
                var cellFont = workBook.CreateFont();
                cellFont.Color = HSSFColor.Black.Index;
                cellFont.Boldweight = (short)FontBoldWeight.Normal;
                cellFont.FontHeightInPoints = 12;

               var cellStyle = workBook.CreateCellStyle();
                cellStyle.BorderTop = BorderStyle.Thin;
                cellStyle.BorderBottom = BorderStyle.Thin;
                cellStyle.BorderLeft = BorderStyle.Thin;
                cellStyle.BorderRight = BorderStyle.Thin;
                cellStyle.SetFont(cellFont);
                #endregion

                IRow row;

                ICell cell;

                var rowIndex = 0;

                if (Model.AvgAndRItemList != null && Model.AvgAndRItemList.Count > 0)
                {
                    var sheetAvgAndR = workBook.CreateSheet("Avg and R");

                    #region Row 0
                    row = sheetAvgAndR.CreateRow(0);

                    cell = row.CreateCell(0);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("MSA Date");

                    cell = row.CreateCell(1);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("MSA No");

                    cell = row.CreateCell(2);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Discription");

                    cell = row.CreateCell(3);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Characteristic Measured");

                    cell = row.CreateCell(4);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Model");

                    cell = row.CreateCell(5);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Serail No");

                    cell = row.CreateCell(6);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Brand");

                    cell = row.CreateCell(7);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Dept");

                    cell = row.CreateCell(8);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Responsible");

                    cell = row.CreateCell(9);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Manager");

                    cell = row.CreateCell(10);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("GR&R");

                    cell = row.CreateCell(11);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(12);
                    cell.CellStyle = headerStyle;

                    cell = row.CreateCell(13);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Bias");

                    cell = row.CreateCell(14);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(15);
                    cell.CellStyle = headerStyle;

                    cell = row.CreateCell(16);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Linearity");

                    cell = row.CreateCell(17);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(18);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(19);
                    cell.CellStyle = headerStyle;

                    cell = row.CreateCell(20);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Stability");

                    cell = row.CreateCell(21);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(22);
                    cell.CellStyle = headerStyle;

                    cell = row.CreateCell(23);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Due Date");
                    #endregion

                    #region Row 1
                    row = sheetAvgAndR.CreateRow(1);

                    cell = row.CreateCell(0);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(1);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(2);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(3);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(4);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(5);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(6);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(7);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(8);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(9);
                    cell.CellStyle = headerStyle;

                    cell = row.CreateCell(10);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("%R&R < 10%");

                    cell = row.CreateCell(11);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("ndc => 5");

                    cell = row.CreateCell(12);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("%R&R < 10% ndc => 5");

                    cell = row.CreateCell(13);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("t statistic");

                    cell = row.CreateCell(14);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Significant t value(2-tailed)");

                    cell = row.CreateCell(15);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("t statistic < Significant t");

                    cell = row.CreateCell(16);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("ta");

                    cell = row.CreateCell(17);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("tb");

                    cell = row.CreateCell(18);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("t58,0.975");

                    cell = row.CreateCell(19);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("ta & tb < t58,0.975");

                    cell = row.CreateCell(20);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Stability");

                    cell = row.CreateCell(21);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Stdev");

                    cell = row.CreateCell(22);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Stability < STDEV");

                    cell = row.CreateCell(23);
                    cell.CellStyle = headerStyle;
                    #endregion

                    #region Merged
                    sheetAvgAndR.AddMergedRegion(new CellRangeAddress(0, 1, 0, 0));
                    sheetAvgAndR.AddMergedRegion(new CellRangeAddress(0, 1, 1, 1));
                    sheetAvgAndR.AddMergedRegion(new CellRangeAddress(0, 1, 2, 2));
                    sheetAvgAndR.AddMergedRegion(new CellRangeAddress(0, 1, 3, 3));
                    sheetAvgAndR.AddMergedRegion(new CellRangeAddress(0, 1, 4, 4));
                    sheetAvgAndR.AddMergedRegion(new CellRangeAddress(0, 1, 5, 5));
                    sheetAvgAndR.AddMergedRegion(new CellRangeAddress(0, 1, 6, 6));
                    sheetAvgAndR.AddMergedRegion(new CellRangeAddress(0, 1, 7, 7));
                    sheetAvgAndR.AddMergedRegion(new CellRangeAddress(0, 1, 8, 8));
                    sheetAvgAndR.AddMergedRegion(new CellRangeAddress(0, 1, 9, 9));
                    sheetAvgAndR.AddMergedRegion(new CellRangeAddress(0, 0, 10, 12));
                    sheetAvgAndR.AddMergedRegion(new CellRangeAddress(0, 0, 13, 15));
                    sheetAvgAndR.AddMergedRegion(new CellRangeAddress(0, 0, 16, 19));
                    sheetAvgAndR.AddMergedRegion(new CellRangeAddress(0, 0, 20, 22));
                    sheetAvgAndR.AddMergedRegion(new CellRangeAddress(0, 1, 23, 23));
                    #endregion

                    rowIndex = 2;

                    foreach (var item in Model.AvgAndRItemList)
                    {
                        #region GRRH
                        if (item.GRRHResult != null)
                        {
                            row = sheetAvgAndR.CreateRow(rowIndex);

                            cell = row.CreateCell(0);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSADateString);

                            cell = row.CreateCell(1);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSACalNo);

                            cell = row.CreateCell(2);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSAIchi);

                            cell = row.CreateCell(3);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSACharacteristic);

                            cell = row.CreateCell(4);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.Model);

                            cell = row.CreateCell(5);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.SerialNo);

                            cell = row.CreateCell(6);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.Brand);

                            cell = row.CreateCell(7);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.Factory);

                            cell = row.CreateCell(8);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSAResponsor);

                            cell = row.CreateCell(9);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.Manager);

                            cell = row.CreateCell(10);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.GRRHResult.GRRDisplay);

                            cell = row.CreateCell(11);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.GRRHResult.ndc.HasValue?item.GRRHResult.ndc.Value.ToString():"");

                            cell = row.CreateCell(12);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.GRRHResult.ResultDisplay);

                            cell = row.CreateCell(13);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.BiasResult != null ? (item.BiasResult.TStatistic.HasValue?item.BiasResult.TStatistic.Value.ToString():"") : "");

                            cell = row.CreateCell(14);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.BiasResult != null ? (item.BiasResult.SignificantT.HasValue ? item.BiasResult.SignificantT.Value.ToString() : "") : "");

                            cell = row.CreateCell(15);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.BiasResult != null ? item.BiasResult.ResultDisplay : "");

                            cell = row.CreateCell(16);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.LinearityResult != null ? (item.LinearityResult.ta.HasValue ? item.LinearityResult.ta.Value.ToString() : "") : "");

                            cell = row.CreateCell(17);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.LinearityResult != null ? (item.LinearityResult.tb.HasValue ? item.LinearityResult.tb.Value.ToString() : "") : "");

                            cell = row.CreateCell(18);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.LinearityResult != null ? (item.LinearityResult.t58.HasValue ? item.LinearityResult.t58.Value.ToString() : "") : "");

                            cell = row.CreateCell(19);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.LinearityResult != null ? item.LinearityResult.ResultDisplay : "");

                            cell = row.CreateCell(20);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.StabilityResult != null ? (item.StabilityResult.Stability.HasValue ? item.StabilityResult.Stability.Value.ToString() : "") : "");

                            cell = row.CreateCell(21);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.StabilityResult != null ? (item.StabilityResult.Stdev.HasValue ? item.StabilityResult.Stdev.Value.ToString() : "") : "");

                            cell = row.CreateCell(22);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.StabilityResult != null ? item.StabilityResult.ResultDisplay : "");

                            cell = row.CreateCell(23);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.DueDateString);

                            rowIndex++;
                        }
                        #endregion

                        #region GRRM
                        if (item.GRRMResult != null)
                        {
                            row = sheetAvgAndR.CreateRow(rowIndex);

                            cell = row.CreateCell(0);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSADateString);

                            cell = row.CreateCell(1);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSACalNo);

                            cell = row.CreateCell(2);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSAIchi);

                            cell = row.CreateCell(3);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSACharacteristic);

                            cell = row.CreateCell(4);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.Model);

                            cell = row.CreateCell(5);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.SerialNo);

                            cell = row.CreateCell(6);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.Brand);

                            cell = row.CreateCell(7);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.Factory);

                            cell = row.CreateCell(8);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSAResponsor);

                            cell = row.CreateCell(9);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.Manager);

                            cell = row.CreateCell(10);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.GRRMResult.GRRDisplay);

                            cell = row.CreateCell(11);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.GRRMResult.ndc.HasValue ? item.GRRMResult.ndc.Value.ToString() : "");

                            cell = row.CreateCell(12);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.GRRMResult.ResultDisplay);

                            cell = row.CreateCell(13);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.BiasResult != null ? (item.BiasResult.TStatistic.HasValue ? item.BiasResult.TStatistic.Value.ToString() : "") : "");

                            cell = row.CreateCell(14);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.BiasResult != null ? (item.BiasResult.SignificantT.HasValue ? item.BiasResult.SignificantT.Value.ToString() : "") : "");

                            cell = row.CreateCell(15);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.BiasResult != null ? item.BiasResult.ResultDisplay : "");

                            cell = row.CreateCell(16);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.LinearityResult != null ? (item.LinearityResult.ta.HasValue ? item.LinearityResult.ta.Value.ToString() : "") : "");

                            cell = row.CreateCell(17);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.LinearityResult != null ? (item.LinearityResult.tb.HasValue ? item.LinearityResult.tb.Value.ToString() : "") : "");

                            cell = row.CreateCell(18);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.LinearityResult != null ? (item.LinearityResult.t58.HasValue ? item.LinearityResult.t58.Value.ToString() : "") : "");

                            cell = row.CreateCell(19);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.LinearityResult != null ? item.LinearityResult.ResultDisplay : "");

                            cell = row.CreateCell(20);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.StabilityResult != null ? (item.StabilityResult.Stability.HasValue ? item.StabilityResult.Stability.Value.ToString() : "") : "");

                            cell = row.CreateCell(21);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.StabilityResult != null ? (item.StabilityResult.Stdev.HasValue ? item.StabilityResult.Stdev.Value.ToString() : "") : "");

                            cell = row.CreateCell(22);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.StabilityResult != null ? item.StabilityResult.ResultDisplay : "");

                            cell = row.CreateCell(23);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.DueDateString);

                            rowIndex++;
                        }
                        #endregion

                        #region GRRL
                        if (item.GRRLResult != null)
                        {
                            row = sheetAvgAndR.CreateRow(rowIndex);

                            cell = row.CreateCell(0);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSADateString);

                            cell = row.CreateCell(1);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSACalNo);

                            cell = row.CreateCell(2);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSAIchi);

                            cell = row.CreateCell(3);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSACharacteristic);

                            cell = row.CreateCell(4);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.Model);

                            cell = row.CreateCell(5);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.SerialNo);

                            cell = row.CreateCell(6);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.Brand);

                            cell = row.CreateCell(7);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.Factory);

                            cell = row.CreateCell(8);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSAResponsor);

                            cell = row.CreateCell(9);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.Manager);

                            cell = row.CreateCell(10);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.GRRLResult.GRRDisplay);

                            cell = row.CreateCell(11);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.GRRLResult.ndc.HasValue ? item.GRRLResult.ndc.Value.ToString() : "");

                            cell = row.CreateCell(12);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.GRRLResult.ResultDisplay);

                            cell = row.CreateCell(13);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.BiasResult != null ? (item.BiasResult.TStatistic.HasValue ? item.BiasResult.TStatistic.Value.ToString() : "") : "");

                            cell = row.CreateCell(14);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.BiasResult != null ? (item.BiasResult.SignificantT.HasValue ? item.BiasResult.SignificantT.Value.ToString() : "") : "");

                            cell = row.CreateCell(15);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.BiasResult != null ? item.BiasResult.ResultDisplay : "");

                            cell = row.CreateCell(16);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.LinearityResult != null ? (item.LinearityResult.ta.HasValue ? item.LinearityResult.ta.Value.ToString() : "") : "");

                            cell = row.CreateCell(17);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.LinearityResult != null ? (item.LinearityResult.tb.HasValue ? item.LinearityResult.tb.Value.ToString() : "") : "");

                            cell = row.CreateCell(18);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.LinearityResult != null ? (item.LinearityResult.t58.HasValue ? item.LinearityResult.t58.Value.ToString() : "") : "");

                            cell = row.CreateCell(19);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.LinearityResult != null ? item.LinearityResult.ResultDisplay : "");

                            cell = row.CreateCell(20);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.StabilityResult != null ? (item.StabilityResult.Stability.HasValue ? item.StabilityResult.Stability.Value.ToString() : "") : "");

                            cell = row.CreateCell(21);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.StabilityResult != null ? (item.StabilityResult.Stdev.HasValue ? item.StabilityResult.Stdev.Value.ToString() : "") : "");

                            cell = row.CreateCell(22);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.StabilityResult != null ? item.StabilityResult.ResultDisplay : "");

                            cell = row.CreateCell(23);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.DueDateString);

                            rowIndex++;
                        }
                        #endregion
                    }
                }

                if (Model.AnovaItemList != null && Model.AnovaItemList.Count > 0)
                {
                    var sheetAnova = workBook.CreateSheet("ANOVA");

                    #region Row 0
                    row = sheetAnova.CreateRow(0);

                    cell = row.CreateCell(0);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("MSA Date");

                    cell = row.CreateCell(1);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("MSA No");

                    cell = row.CreateCell(2);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Discription");

                    cell = row.CreateCell(3);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Characteristic Measured");

                    cell = row.CreateCell(4);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Model");

                    cell = row.CreateCell(5);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Serail No");

                    cell = row.CreateCell(6);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Brand");

                    cell = row.CreateCell(7);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Dept");

                    cell = row.CreateCell(8);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Responsible");

                    cell = row.CreateCell(9);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Manager");

                    cell = row.CreateCell(10);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("GR&R ANOVA");

                    cell = row.CreateCell(11);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(12);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(13);
                    cell.CellStyle = headerStyle;

                    cell = row.CreateCell(14);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Bias");

                    cell = row.CreateCell(15);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(16);
                    cell.CellStyle = headerStyle;

                    cell = row.CreateCell(17);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Linearity");

                    cell = row.CreateCell(18);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(18);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(20);
                    cell.CellStyle = headerStyle;

                    cell = row.CreateCell(21);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Stability");

                    cell = row.CreateCell(22);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(23);
                    cell.CellStyle = headerStyle;

                    cell = row.CreateCell(24);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Due Date");
                    #endregion

                    #region Row 1
                    row = sheetAnova.CreateRow(1);

                    cell = row.CreateCell(0);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(1);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(2);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(3);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(4);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(5);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(6);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(7);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(8);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(9);
                    cell.CellStyle = headerStyle;

                    cell = row.CreateCell(10);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("%TV < 10%");

                    cell = row.CreateCell(11);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("%PT < 10%");

                    cell = row.CreateCell(12);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("ndc > 5");

                    cell = row.CreateCell(13);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("<10% NDC > 5");

                    cell = row.CreateCell(14);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("t statistic");

                    cell = row.CreateCell(15);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Significant t value(2-tailed)");

                    cell = row.CreateCell(16);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("t statistic < Significant t");

                    cell = row.CreateCell(17);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("ta");

                    cell = row.CreateCell(18);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("tb");

                    cell = row.CreateCell(19);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("t58,0.975");

                    cell = row.CreateCell(20);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("ta & tb < t58,0.975");

                    cell = row.CreateCell(21);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Stability");

                    cell = row.CreateCell(22);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Stdev");

                    cell = row.CreateCell(23);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Stability < STDEV");

                    cell = row.CreateCell(24);
                    cell.CellStyle = headerStyle;
                    #endregion

                    #region Merged
                    sheetAnova.AddMergedRegion(new CellRangeAddress(0, 1, 0, 0));
                    sheetAnova.AddMergedRegion(new CellRangeAddress(0, 1, 1, 1));
                    sheetAnova.AddMergedRegion(new CellRangeAddress(0, 1, 2, 2));
                    sheetAnova.AddMergedRegion(new CellRangeAddress(0, 1, 3, 3));
                    sheetAnova.AddMergedRegion(new CellRangeAddress(0, 1, 4, 4));
                    sheetAnova.AddMergedRegion(new CellRangeAddress(0, 1, 5, 5));
                    sheetAnova.AddMergedRegion(new CellRangeAddress(0, 1, 6, 6));
                    sheetAnova.AddMergedRegion(new CellRangeAddress(0, 1, 7, 7));
                    sheetAnova.AddMergedRegion(new CellRangeAddress(0, 1, 8, 8));
                    sheetAnova.AddMergedRegion(new CellRangeAddress(0, 1, 9, 9));
                    sheetAnova.AddMergedRegion(new CellRangeAddress(0, 0, 10, 13));
                    sheetAnova.AddMergedRegion(new CellRangeAddress(0, 0, 14, 16));
                    sheetAnova.AddMergedRegion(new CellRangeAddress(0, 0, 17, 20));
                    sheetAnova.AddMergedRegion(new CellRangeAddress(0, 0, 21, 23));
                    sheetAnova.AddMergedRegion(new CellRangeAddress(0, 1, 24, 24));
                    #endregion

                    rowIndex = 2;

                    foreach (var item in Model.AnovaItemList)
                    {
                        #region AnovaH
                        if (item.AnovaHResult != null)
                        {
                            row = sheetAnova.CreateRow(rowIndex);

                            cell = row.CreateCell(0);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSADateString);

                            cell = row.CreateCell(1);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSACalNo);

                            cell = row.CreateCell(2);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSAIchi);

                            cell = row.CreateCell(3);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSACharacteristic);

                            cell = row.CreateCell(4);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.Model);

                            cell = row.CreateCell(5);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.SerialNo);

                            cell = row.CreateCell(6);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.Brand);

                            cell = row.CreateCell(7);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.Factory);

                            cell = row.CreateCell(8);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSAResponsor);

                            cell = row.CreateCell(9);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.Manager);

                            cell = row.CreateCell(10);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.AnovaHResult.TVDisplay);

                            cell = row.CreateCell(11);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.AnovaHResult.PTDisplay);

                            cell = row.CreateCell(12);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.AnovaHResult.NDC.HasValue ? item.AnovaHResult.NDC.Value.ToString() : "");

                            cell = row.CreateCell(13);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.AnovaHResult.ResultDisplay);

                            cell = row.CreateCell(14);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.BiasResult != null ? (item.BiasResult.TStatistic.HasValue ? item.BiasResult.TStatistic.Value.ToString() : "") : "");

                            cell = row.CreateCell(15);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.BiasResult != null ? (item.BiasResult.SignificantT.HasValue ? item.BiasResult.SignificantT.Value.ToString() : "") : "");

                            cell = row.CreateCell(16);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.BiasResult != null ? item.BiasResult.ResultDisplay : "");

                            cell = row.CreateCell(17);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.LinearityResult != null ? (item.LinearityResult.ta.HasValue ? item.LinearityResult.ta.Value.ToString() : "") : "");

                            cell = row.CreateCell(18);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.LinearityResult != null ? (item.LinearityResult.tb.HasValue ? item.LinearityResult.tb.Value.ToString() : "") : "");

                            cell = row.CreateCell(19);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.LinearityResult != null ? (item.LinearityResult.t58.HasValue ? item.LinearityResult.t58.Value.ToString() : "") : "");

                            cell = row.CreateCell(20);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.LinearityResult != null ? item.LinearityResult.ResultDisplay : "");

                            cell = row.CreateCell(21);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.StabilityResult != null ? (item.StabilityResult.Stability.HasValue ? item.StabilityResult.Stability.Value.ToString() : "") : "");

                            cell = row.CreateCell(22);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.StabilityResult != null ? (item.StabilityResult.Stdev.HasValue ? item.StabilityResult.Stdev.Value.ToString() : "") : "");

                            cell = row.CreateCell(23);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.StabilityResult != null ? item.StabilityResult.ResultDisplay : "");

                            cell = row.CreateCell(24);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.DueDateString);

                            rowIndex++;
                        }
                        #endregion

                        #region AnovaM
                        if (item.AnovaMResult != null)
                        {
                            row = sheetAnova.CreateRow(rowIndex);

                            cell = row.CreateCell(0);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSADateString);

                            cell = row.CreateCell(1);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSACalNo);

                            cell = row.CreateCell(2);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSAIchi);

                            cell = row.CreateCell(3);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSACharacteristic);

                            cell = row.CreateCell(4);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.Model);

                            cell = row.CreateCell(5);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.SerialNo);

                            cell = row.CreateCell(6);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.Brand);

                            cell = row.CreateCell(7);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.Factory);

                            cell = row.CreateCell(8);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSAResponsor);

                            cell = row.CreateCell(9);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.Manager);

                            cell = row.CreateCell(10);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.AnovaMResult.TVDisplay);

                            cell = row.CreateCell(11);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.AnovaMResult.PTDisplay);

                            cell = row.CreateCell(12);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.AnovaMResult.NDC.HasValue ? item.AnovaMResult.NDC.Value.ToString() : "");

                            cell = row.CreateCell(13);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.AnovaMResult.ResultDisplay);

                            cell = row.CreateCell(14);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.BiasResult != null ? (item.BiasResult.TStatistic.HasValue ? item.BiasResult.TStatistic.Value.ToString() : "") : "");

                            cell = row.CreateCell(15);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.BiasResult != null ? (item.BiasResult.SignificantT.HasValue ? item.BiasResult.SignificantT.Value.ToString() : "") : "");

                            cell = row.CreateCell(16);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.BiasResult != null ? item.BiasResult.ResultDisplay : "");

                            cell = row.CreateCell(17);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.LinearityResult != null ? (item.LinearityResult.ta.HasValue ? item.LinearityResult.ta.Value.ToString() : "") : "");

                            cell = row.CreateCell(18);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.LinearityResult != null ? (item.LinearityResult.tb.HasValue ? item.LinearityResult.tb.Value.ToString() : "") : "");

                            cell = row.CreateCell(19);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.LinearityResult != null ? (item.LinearityResult.t58.HasValue ? item.LinearityResult.t58.Value.ToString() : "") : "");

                            cell = row.CreateCell(20);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.LinearityResult != null ? item.LinearityResult.ResultDisplay : "");

                            cell = row.CreateCell(21);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.StabilityResult != null ? (item.StabilityResult.Stability.HasValue ? item.StabilityResult.Stability.Value.ToString() : "") : "");

                            cell = row.CreateCell(22);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.StabilityResult != null ? (item.StabilityResult.Stdev.HasValue ? item.StabilityResult.Stdev.Value.ToString() : "") : "");

                            cell = row.CreateCell(23);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.StabilityResult != null ? item.StabilityResult.ResultDisplay : "");

                            cell = row.CreateCell(24);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.DueDateString);

                            rowIndex++;
                        }
                        #endregion

                        #region AnovaML
                        if (item.AnovaLResult != null)
                        {
                            row = sheetAnova.CreateRow(rowIndex);

                            cell = row.CreateCell(0);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSADateString);

                            cell = row.CreateCell(1);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSACalNo);

                            cell = row.CreateCell(2);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSAIchi);

                            cell = row.CreateCell(3);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSACharacteristic);

                            cell = row.CreateCell(4);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.Model);

                            cell = row.CreateCell(5);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.SerialNo);

                            cell = row.CreateCell(6);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.Brand);

                            cell = row.CreateCell(7);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.Factory);

                            cell = row.CreateCell(8);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.MSAResponsor);

                            cell = row.CreateCell(9);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.Manager);

                            cell = row.CreateCell(10);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.AnovaLResult.TVDisplay);

                            cell = row.CreateCell(11);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.AnovaLResult.PTDisplay);

                            cell = row.CreateCell(12);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.AnovaLResult.NDC.HasValue ? item.AnovaLResult.NDC.Value.ToString() : "");

                            cell = row.CreateCell(13);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.AnovaLResult.ResultDisplay);

                            cell = row.CreateCell(14);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.BiasResult != null ? (item.BiasResult.TStatistic.HasValue ? item.BiasResult.TStatistic.Value.ToString() : "") : "");

                            cell = row.CreateCell(15);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.BiasResult != null ? (item.BiasResult.SignificantT.HasValue ? item.BiasResult.SignificantT.Value.ToString() : "") : "");

                            cell = row.CreateCell(16);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.BiasResult != null ? item.BiasResult.ResultDisplay : "");

                            cell = row.CreateCell(17);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.LinearityResult != null ? (item.LinearityResult.ta.HasValue ? item.LinearityResult.ta.Value.ToString() : "") : "");

                            cell = row.CreateCell(18);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.LinearityResult != null ? (item.LinearityResult.tb.HasValue ? item.LinearityResult.tb.Value.ToString() : "") : "");

                            cell = row.CreateCell(19);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.LinearityResult != null ? (item.LinearityResult.t58.HasValue ? item.LinearityResult.t58.Value.ToString() : "") : "");

                            cell = row.CreateCell(20);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.LinearityResult != null ? item.LinearityResult.ResultDisplay : "");

                            cell = row.CreateCell(21);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.StabilityResult != null ? (item.StabilityResult.Stability.HasValue ? item.StabilityResult.Stability.Value.ToString() : "") : "");

                            cell = row.CreateCell(22);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.StabilityResult != null ? (item.StabilityResult.Stdev.HasValue ? item.StabilityResult.Stdev.Value.ToString() : "") : "");

                            cell = row.CreateCell(23);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.StabilityResult != null ? item.StabilityResult.ResultDisplay : "");

                            cell = row.CreateCell(24);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(item.DueDateString);

                            rowIndex++;
                        }
                        #endregion
                    }
                }

                if (Model.GoOnGoItemList != null && Model.GoOnGoItemList.Count > 0)
                {
                    var sheetGoNoGo = workBook.CreateSheet("Go no Go");

                    #region Row 0
                    row = sheetGoNoGo.CreateRow(0);

                    cell = row.CreateCell(0);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("MSA Date");

                    cell = row.CreateCell(1);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("MSA No");

                    cell = row.CreateCell(2);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Discription");

                    cell = row.CreateCell(3);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Characteristic Measured");

                    cell = row.CreateCell(4);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Model");

                    cell = row.CreateCell(5);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Serail No");

                    cell = row.CreateCell(6);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Brand");

                    cell = row.CreateCell(7);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Dept");

                    cell = row.CreateCell(8);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Responsible");

                    cell = row.CreateCell(9);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Manager");

                    cell = row.CreateCell(10);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("APPRAISER");

                    cell = row.CreateCell(11);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Kappa");

                    cell = row.CreateCell(12);
                    cell.CellStyle = headerStyle;

                    cell = row.CreateCell(13);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Effectiveness");

                    cell = row.CreateCell(14);
                    cell.CellStyle = headerStyle;

                    cell = row.CreateCell(15);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Error Rate");

                    cell = row.CreateCell(16);
                    cell.CellStyle = headerStyle;

                    cell = row.CreateCell(17);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("False Alarm Rate");

                    cell = row.CreateCell(18);
                    cell.CellStyle = headerStyle;

                    cell = row.CreateCell(19);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Due Date");
                    #endregion

                    #region Row 1
                    row = sheetGoNoGo.CreateRow(1);

                    cell = row.CreateCell(0);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(1);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(2);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(3);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(4);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(5);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(6);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(7);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(8);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(9);
                    cell.CellStyle = headerStyle;
                    cell = row.CreateCell(10);
                    cell.CellStyle = headerStyle;

                    cell = row.CreateCell(11);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("kappa > 0.75 or = 1");

                    cell = row.CreateCell(12);
                    cell.CellStyle = headerStyle;

                    cell = row.CreateCell(13);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Effectiveness >= 90%");

                    cell = row.CreateCell(14);
                    cell.CellStyle = headerStyle;

                    cell = row.CreateCell(15);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Error Rate <= 2%");

                    cell = row.CreateCell(16);
                    cell.CellStyle = headerStyle;

                    cell = row.CreateCell(17);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("False Alarm Rate <= 5%");

                    cell = row.CreateCell(18);
                    cell.CellStyle = headerStyle;
                    #endregion

                    #region Merged
                    sheetGoNoGo.AddMergedRegion(new CellRangeAddress(0, 1, 0, 0));
                    sheetGoNoGo.AddMergedRegion(new CellRangeAddress(0, 1, 1, 1));
                    sheetGoNoGo.AddMergedRegion(new CellRangeAddress(0, 1, 2, 2));
                    sheetGoNoGo.AddMergedRegion(new CellRangeAddress(0, 1, 3, 3));
                    sheetGoNoGo.AddMergedRegion(new CellRangeAddress(0, 1, 4, 4));
                    sheetGoNoGo.AddMergedRegion(new CellRangeAddress(0, 1, 5, 5));
                    sheetGoNoGo.AddMergedRegion(new CellRangeAddress(0, 1, 6, 6));
                    sheetGoNoGo.AddMergedRegion(new CellRangeAddress(0, 1, 7, 7));
                    sheetGoNoGo.AddMergedRegion(new CellRangeAddress(0, 1, 8, 8));
                    sheetGoNoGo.AddMergedRegion(new CellRangeAddress(0, 1, 9, 9));
                    sheetGoNoGo.AddMergedRegion(new CellRangeAddress(0, 1, 10, 10));

                    sheetGoNoGo.AddMergedRegion(new CellRangeAddress(0, 0, 11, 12));
                    sheetGoNoGo.AddMergedRegion(new CellRangeAddress(0, 0, 13, 14));
                    sheetGoNoGo.AddMergedRegion(new CellRangeAddress(0, 0, 15, 16));
                    sheetGoNoGo.AddMergedRegion(new CellRangeAddress(0, 0, 17, 18));
                    sheetGoNoGo.AddMergedRegion(new CellRangeAddress(0, 1, 19, 19));

                    sheetGoNoGo.AddMergedRegion(new CellRangeAddress(1, 1, 11, 12));
                    sheetGoNoGo.AddMergedRegion(new CellRangeAddress(1, 1, 13, 14));
                    sheetGoNoGo.AddMergedRegion(new CellRangeAddress(1, 1, 15, 16));
                    sheetGoNoGo.AddMergedRegion(new CellRangeAddress(1, 1, 17, 18));
                    #endregion

                    rowIndex = 2;

                    foreach (var item in Model.GoOnGoItemList)
                    {
                        var beginRowIndex = rowIndex;

                        row = sheetGoNoGo.CreateRow(rowIndex);

                        cell = row.CreateCell(0);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.MSADateString);

                        cell = row.CreateCell(1);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.MSACalNo);

                        cell = row.CreateCell(2);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.MSAIchi);

                        cell = row.CreateCell(3);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.MSACharacteristic);

                        cell = row.CreateCell(4);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.Model);

                        cell = row.CreateCell(5);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.SerialNo);

                        cell = row.CreateCell(6);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.Brand);

                        cell = row.CreateCell(7);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.Factory);

                        cell = row.CreateCell(8);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.MSAResponsor);

                        cell = row.CreateCell(9);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.Manager);

                        cell = row.CreateCell(10);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue("A");

                        cell = row.CreateCell(11);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.CountResult != null ? (item.CountResult.KappaA.HasValue ? item.CountResult.KappaA.Value.ToString() : "") : "");

                        cell = row.CreateCell(12);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.CountResult != null ? item.CountResult.KappaResultDisplay : "");

                        cell = row.CreateCell(13);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.CountResult != null ? item.CountResult.CountAEffectiveDisplay : "");

                        cell = row.CreateCell(14);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.CountResult != null ? item.CountResult.ResultDisplay : "");

                        cell = row.CreateCell(15);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.CountResult != null ? item.CountResult.CountAErrorDisplay : "");

                        cell = row.CreateCell(16);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.CountResult != null ? item.CountResult.ResultDisplay : "");

                        cell = row.CreateCell(17);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.CountResult != null ? item.CountResult.CountAAlarmDisplay : "");

                        cell = row.CreateCell(18);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.CountResult != null ? item.CountResult.ResultDisplay : "");

                        cell = row.CreateCell(19);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.DueDateString);

                        rowIndex++;

                        row = sheetGoNoGo.CreateRow(rowIndex);

                        cell = row.CreateCell(0);
                        cell.CellStyle = cellStyle;
                        cell = row.CreateCell(1);
                        cell.CellStyle = cellStyle;
                        cell = row.CreateCell(2);
                        cell.CellStyle = cellStyle;
                        cell = row.CreateCell(3);
                        cell.CellStyle = cellStyle;
                        cell = row.CreateCell(4);
                        cell.CellStyle = cellStyle;
                        cell = row.CreateCell(5);
                        cell.CellStyle = cellStyle;
                        cell = row.CreateCell(6);
                        cell.CellStyle = cellStyle;
                        cell = row.CreateCell(7);
                        cell.CellStyle = cellStyle;
                        cell = row.CreateCell(8);
                        cell.CellStyle = cellStyle;
                        cell = row.CreateCell(9);
                        cell.CellStyle = cellStyle;

                        cell = row.CreateCell(10);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue("B");

                        cell = row.CreateCell(11);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.CountResult != null ? (item.CountResult.KappaB.HasValue ? item.CountResult.KappaB.Value.ToString() : "") : "");

                        cell = row.CreateCell(12);
                        cell.CellStyle = cellStyle;

                        cell = row.CreateCell(13);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.CountResult != null ? item.CountResult.CountBEffectiveDisplay : "");

                        cell = row.CreateCell(14);
                        cell.CellStyle = cellStyle;

                        cell = row.CreateCell(15);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.CountResult != null ? item.CountResult.CountBErrorDisplay : "");

                        cell = row.CreateCell(16);
                        cell.CellStyle = cellStyle;

                        cell = row.CreateCell(17);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.CountResult != null ? item.CountResult.CountBAlarmDisplay : "");

                        cell = row.CreateCell(18);
                        cell.CellStyle = cellStyle;

                        rowIndex++;

                        row = sheetGoNoGo.CreateRow(rowIndex);

                        cell = row.CreateCell(0);
                        cell.CellStyle = cellStyle;
                        cell = row.CreateCell(1);
                        cell.CellStyle = cellStyle;
                        cell = row.CreateCell(2);
                        cell.CellStyle = cellStyle;
                        cell = row.CreateCell(3);
                        cell.CellStyle = cellStyle;
                        cell = row.CreateCell(4);
                        cell.CellStyle = cellStyle;
                        cell = row.CreateCell(5);
                        cell.CellStyle = cellStyle;
                        cell = row.CreateCell(6);
                        cell.CellStyle = cellStyle;
                        cell = row.CreateCell(7);
                        cell.CellStyle = cellStyle;
                        cell = row.CreateCell(8);
                        cell.CellStyle = cellStyle;
                        cell = row.CreateCell(9);
                        cell.CellStyle = cellStyle;

                        cell = row.CreateCell(10);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue("C");

                        cell = row.CreateCell(11);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.CountResult != null ? (item.CountResult.KappaC.HasValue ? item.CountResult.KappaC.Value.ToString() : "") : "");

                        cell = row.CreateCell(12);
                        cell.CellStyle = cellStyle;

                        cell = row.CreateCell(13);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.CountResult != null ? item.CountResult.CountCEffectiveDisplay : "");

                        cell = row.CreateCell(14);
                        cell.CellStyle = cellStyle;

                        cell = row.CreateCell(15);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.CountResult != null ? item.CountResult.CountCErrorDisplay : "");

                        cell = row.CreateCell(16);
                        cell.CellStyle = cellStyle;

                        cell = row.CreateCell(17);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.CountResult != null ? item.CountResult.CountCAlarmDisplay : "");

                        cell = row.CreateCell(18);
                        cell.CellStyle = cellStyle;

                        sheetGoNoGo.AddMergedRegion(new CellRangeAddress(beginRowIndex, beginRowIndex + 2, 0, 0));
                        sheetGoNoGo.AddMergedRegion(new CellRangeAddress(beginRowIndex, beginRowIndex + 2, 1, 1));
                        sheetGoNoGo.AddMergedRegion(new CellRangeAddress(beginRowIndex, beginRowIndex + 2, 2, 2));
                        sheetGoNoGo.AddMergedRegion(new CellRangeAddress(beginRowIndex, beginRowIndex + 2, 3, 3));
                        sheetGoNoGo.AddMergedRegion(new CellRangeAddress(beginRowIndex, beginRowIndex + 2, 4, 4));
                        sheetGoNoGo.AddMergedRegion(new CellRangeAddress(beginRowIndex, beginRowIndex + 2, 5, 5));
                        sheetGoNoGo.AddMergedRegion(new CellRangeAddress(beginRowIndex, beginRowIndex + 2, 6, 6));
                        sheetGoNoGo.AddMergedRegion(new CellRangeAddress(beginRowIndex, beginRowIndex + 2, 7, 7));
                        sheetGoNoGo.AddMergedRegion(new CellRangeAddress(beginRowIndex, beginRowIndex + 2, 8, 8));
                        sheetGoNoGo.AddMergedRegion(new CellRangeAddress(beginRowIndex, beginRowIndex + 2, 9, 9));

                        sheetGoNoGo.AddMergedRegion(new CellRangeAddress(beginRowIndex, beginRowIndex + 2, 12, 12));
                        sheetGoNoGo.AddMergedRegion(new CellRangeAddress(beginRowIndex, beginRowIndex + 2, 14, 14));
                        sheetGoNoGo.AddMergedRegion(new CellRangeAddress(beginRowIndex, beginRowIndex + 2, 16, 16));
                        sheetGoNoGo.AddMergedRegion(new CellRangeAddress(beginRowIndex, beginRowIndex + 2, 18, 18));
                        sheetGoNoGo.AddMergedRegion(new CellRangeAddress(beginRowIndex, beginRowIndex + 2, 19, 19));

                        rowIndex++;
                    }
                }

                var output = new ExcelExportModel("MSA master list", Define.EnumExcelVersion._2007);

                using (FileStream fs = System.IO.File.OpenWrite(output.FullFileName))
                {
                    workBook.Write(fs);
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
