using DbEntity.MSSQL.TankPatrol;
using Models.Authenticated;
using Models.TankPatrol.ResultQuery;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace DataAccess.TankPatrol
{
    public class ResultQueryHelper
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = new List<ArriveRecordModel>();

                using (TankDbEntities db = new TankDbEntities())
                {
                    var query = db.ArriveRecord.Where(x => Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.BeginDate))
                    {
                        query = query.Where(x => string.Compare(x.ArriveDate, Parameters.BeginDate) >= 0);
                    }

                    if (!string.IsNullOrEmpty(Parameters.EndDate))
                    {
                        query = query.Where(x => string.Compare(x.ArriveDate, Parameters.EndDate) <= 0);
                    }

                    if (!string.IsNullOrEmpty(Parameters.OrganizationUniqueID))
                    {
                        var downStream = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                        query = query.Where(x => downStream.Contains(x.OrganizationUniqueID));
                    }

                    if (!string.IsNullOrEmpty(Parameters.StationUniqueID))
                    {
                        query = query.Where(x => x.StationUniqueID == Parameters.StationUniqueID);
                    }

                    if (!string.IsNullOrEmpty(Parameters.IslandUniqueID))
                    {
                        query = query.Where(x => x.IslandUniqueID == Parameters.IslandUniqueID);
                    }

                    if (!string.IsNullOrEmpty(Parameters.PortUniqueID))
                    {
                        query = query.Where(x => x.PortUniqueID == Parameters.PortUniqueID);
                    }

                    itemList = query.ToList().Select(x => new ArriveRecordModel
                    {
                        UniqueID = x.UniqueID,
                        OrganizationDescription = x.OrganizationDescription,
                        StationID = x.StationID,
                        StationDescription = x.StationDescription,
                        IslandID = x.IslandID,
                        IslandDescription = x.IslandDescription,
                        PortID = x.PortID,
                        PortDescription = x.PortDescription,
                        CheckType = x.CheckType,
                        TankNo = x.TankNo,
                        ArriveDate = x.ArriveDate,
                        ArriveTime = x.ArriveTime,
                        Driver = x.Driver,
                        LastTimeMaterial = x.LastTimeMaterial,
                        ThisTimeMaterial = x.ThisTimeMaterial,
                        Owner = x.Owner,
                        UnRFIDReasonDescription = x.UnRFIDReasonDescription,
                        UnRFIDReasonRemark = x.UnRFIDReasonRemark,
                        SignExtension = x.SignExtension,
                        UserID = x.UserID,
                        UserName = x.UserName
                    }).OrderBy(x => x.ArriveDate).ThenBy(x => x.ArriveTime).ThenBy(x => x.OrganizationDescription).ThenBy(x => x.Station).ThenBy(x => x.Island).ThenBy(x => x.Port).ToList();
                }

                result.ReturnData(itemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetDetailViewModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new ArriveRecordModel();

                using (TankDbEntities db = new TankDbEntities())
                {
                    var arriveRecord = db.ArriveRecord.First(x => x.UniqueID == UniqueID);

                    model = new ArriveRecordModel()
                    {
                        UniqueID = arriveRecord.UniqueID,
                        OrganizationDescription = arriveRecord.OrganizationDescription,
                        StationID = arriveRecord.StationID,
                        StationDescription = arriveRecord.StationDescription,
                        IslandID = arriveRecord.IslandID,
                        IslandDescription = arriveRecord.IslandDescription,
                        PortID = arriveRecord.PortID,
                        PortDescription = arriveRecord.PortDescription,
                        CheckType = arriveRecord.CheckType,
                        TankNo = arriveRecord.TankNo,
                        ArriveDate = arriveRecord.ArriveDate,
                        ArriveTime = arriveRecord.ArriveTime,
                        Driver = arriveRecord.Driver,
                        LastTimeMaterial = arriveRecord.LastTimeMaterial,
                        ThisTimeMaterial = arriveRecord.ThisTimeMaterial,
                        Owner = arriveRecord.Owner,
                        UnRFIDReasonDescription = arriveRecord.UnRFIDReasonDescription,
                        UnRFIDReasonRemark = arriveRecord.UnRFIDReasonRemark,
                        SignExtension = arriveRecord.SignExtension,
                        UserID = arriveRecord.UserID,
                        UserName = arriveRecord.UserName
                    };

                    var checkResultList = db.CheckResult.Where(x => x.ArriveRecordUniqueID == arriveRecord.UniqueID).ToList();

                    if (arriveRecord.CheckType == "U")
                    {
                        model.UBCheckResultList = checkResultList.Where(x => x.Procedure == "B").Select(x => new CheckResultModel
                        {
                            UniqueID = x.UniqueID,
                            CheckItemID = x.CheckItemID,
                            CheckItemDescription = x.CheckItemDescription,
                            CheckDate = x.CheckDate,
                            CheckTime = x.CheckTime,
                            IsAbnormal = x.IsAbnormal,
                            IsAlert = x.IsAlert,
                            LowerLimit = x.LowerLimit,
                            LowerAlertLimit = x.LowerAlertLimit,
                            UpperAlertLimit = x.UpperAlertLimit,
                            UpperLimit = x.UpperLimit,
                            Result = x.Result,
                            Unit = x.Unit
                        }).OrderBy(x => x.CheckItemID).ToList();

                        model.UPCheckResultList = checkResultList.Where(x => x.Procedure == "P").Select(x => new CheckResultModel
                        {
                            UniqueID = x.UniqueID,
                            CheckItemID = x.CheckItemID,
                            CheckItemDescription = x.CheckItemDescription,
                            CheckDate = x.CheckDate,
                            CheckTime = x.CheckTime,
                            IsAbnormal = x.IsAbnormal,
                            IsAlert = x.IsAlert,
                            LowerLimit = x.LowerLimit,
                            LowerAlertLimit = x.LowerAlertLimit,
                            UpperAlertLimit = x.UpperAlertLimit,
                            UpperLimit = x.UpperLimit,
                            Result = x.Result,
                            Unit = x.Unit
                        }).OrderBy(x => x.CheckItemID).ToList();

                        model.UACheckResultList = checkResultList.Where(x => x.Procedure == "A").Select(x => new CheckResultModel
                        {
                            UniqueID = x.UniqueID,
                            CheckItemID = x.CheckItemID,
                            CheckItemDescription = x.CheckItemDescription,
                            CheckDate = x.CheckDate,
                            CheckTime = x.CheckTime,
                            IsAbnormal = x.IsAbnormal,
                            IsAlert = x.IsAlert,
                            LowerLimit = x.LowerLimit,
                            LowerAlertLimit = x.LowerAlertLimit,
                            UpperAlertLimit = x.UpperAlertLimit,
                            UpperLimit = x.UpperLimit,
                            Result = x.Result,
                            Unit = x.Unit
                        }).OrderBy(x => x.CheckItemID).ToList();

                        model.UDCheckResultList = checkResultList.Where(x => x.Procedure == "D").Select(x => new CheckResultModel
                        {
                            UniqueID = x.UniqueID,
                            CheckItemID = x.CheckItemID,
                            CheckItemDescription = x.CheckItemDescription,
                            CheckDate = x.CheckDate,
                            CheckTime = x.CheckTime,
                            IsAbnormal = x.IsAbnormal,
                            IsAlert = x.IsAlert,
                            LowerLimit = x.LowerLimit,
                            LowerAlertLimit = x.LowerAlertLimit,
                            UpperAlertLimit = x.UpperAlertLimit,
                            UpperLimit = x.UpperLimit,
                            Result = x.Result,
                            Unit = x.Unit
                        }).OrderBy(x => x.CheckItemID).ToList();
                    }

                    if (arriveRecord.CheckType == "L")
                    {
                        model.LBCheckResultList = checkResultList.Where(x => x.Procedure == "B").Select(x => new CheckResultModel
                        {
                            UniqueID = x.UniqueID,
                            CheckItemID = x.CheckItemID,
                            CheckItemDescription = x.CheckItemDescription,
                            CheckDate = x.CheckDate,
                            CheckTime = x.CheckTime,
                            IsAbnormal = x.IsAbnormal,
                            IsAlert = x.IsAlert,
                            LowerLimit = x.LowerLimit,
                            LowerAlertLimit = x.LowerAlertLimit,
                            UpperAlertLimit = x.UpperAlertLimit,
                            UpperLimit = x.UpperLimit,
                            Result = x.Result,
                            Unit = x.Unit
                        }).OrderBy(x => x.CheckItemID).ToList();

                        model.LPCheckResultList = checkResultList.Where(x => x.Procedure == "P").Select(x => new CheckResultModel
                        {
                            UniqueID = x.UniqueID,
                            CheckItemID = x.CheckItemID,
                            CheckItemDescription = x.CheckItemDescription,
                            CheckDate = x.CheckDate,
                            CheckTime = x.CheckTime,
                            IsAbnormal = x.IsAbnormal,
                            IsAlert = x.IsAlert,
                            LowerLimit = x.LowerLimit,
                            LowerAlertLimit = x.LowerAlertLimit,
                            UpperAlertLimit = x.UpperAlertLimit,
                            UpperLimit = x.UpperLimit,
                            Result = x.Result,
                            Unit = x.Unit
                        }).OrderBy(x => x.CheckItemID).ToList();

                        model.LACheckResultList = checkResultList.Where(x => x.Procedure == "A").Select(x => new CheckResultModel
                        {
                            UniqueID = x.UniqueID,
                            CheckItemID = x.CheckItemID,
                            CheckItemDescription = x.CheckItemDescription,
                            CheckDate = x.CheckDate,
                            CheckTime = x.CheckTime,
                            IsAbnormal = x.IsAbnormal,
                            IsAlert = x.IsAlert,
                            LowerLimit = x.LowerLimit,
                            LowerAlertLimit = x.LowerAlertLimit,
                            UpperAlertLimit = x.UpperAlertLimit,
                            UpperLimit = x.UpperLimit,
                            Result = x.Result,
                            Unit = x.Unit
                        }).OrderBy(x => x.CheckItemID).ToList();

                        model.LDCheckResultList = checkResultList.Where(x => x.Procedure == "D").Select(x => new CheckResultModel
                        {
                            UniqueID = x.UniqueID,
                            CheckItemID = x.CheckItemID,
                            CheckItemDescription = x.CheckItemDescription,
                            CheckDate = x.CheckDate,
                            CheckTime = x.CheckTime,
                            IsAbnormal = x.IsAbnormal,
                            IsAlert = x.IsAlert,
                            LowerLimit = x.LowerLimit,
                            LowerAlertLimit = x.LowerAlertLimit,
                            UpperAlertLimit = x.UpperAlertLimit,
                            UpperLimit = x.UpperLimit,
                            Result = x.Result,
                            Unit = x.Unit
                        }).OrderBy(x => x.CheckItemID).ToList();
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

        public static RequestResult Export(List<ArriveRecordModel> ArriveRecordList, Define.EnumExcelVersion ExcelVersion)
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = new List<ArriveRecordModel>();

                foreach (var arriveRecord in ArriveRecordList)
                {
                    result = GetDetailViewModel(arriveRecord.UniqueID);

                    if (result.IsSuccess)
                    {
                        itemList.Add(result.Data as ArriveRecordModel);
                    }
                }

                var arriveRecordExcelItemList = new List<ArriveRecordExcelItem>();

                var checkResultExcelItemList = new List<CheckResultExcelItem>();

                foreach (var item in itemList)
                {
                    arriveRecordExcelItemList.Add(new ArriveRecordExcelItem()
                    {
                        ArriveDate = item.ArriveDate,
                        ArriveTime = item.ArriveTime,
                        CheckType = item.CheckTypeDisplay,
                        Driver = item.Driver,
                        Island = item.Island,
                        LastTimeMaterial = item.LastTimeMaterial,
                        Organization = item.OrganizationDescription,
                        Owner = item.Owner,
                        Port = item.Port,
                        Station = item.Station,
                        TankNo = item.TankNo,
                        ThisTimeMaterial = item.ThisTimeMaterial,
                        User = item.User,
                         Sign = item.Sign
                    });

                    if (item.LBCheckResultList != null && item.LBCheckResultList.Count > 0)
                    {
                        foreach (var checkResult in item.LBCheckResultList)
                        {
                            checkResultExcelItemList.Add(new CheckResultExcelItem()
                            {
                                CheckDate = checkResult.CheckDate,
                                CheckItem = checkResult.CheckItem,
                                CheckTime = checkResult.CheckTime,
                                CheckType = "裝料前檢查",
                                IsAbnormal = checkResult.IsAbnormal ? "異常" : checkResult.IsAlert ? "注意" : "",
                                Island = item.Island,
                                LowerAlertLimit = checkResult.LowerAlertLimit,
                                LowerLimit = checkResult.LowerLimit,
                                Organization = item.OrganizationDescription,
                                Port = item.Port,
                                Result = checkResult.Result,
                                Station = item.Station,
                                TankNo = item.TankNo,
                                Unit = checkResult.Unit,
                                UpperAlertLimit = checkResult.UpperAlertLimit,
                                UpperLimit = checkResult.UpperLimit,
                                User = item.User
                            });
                        }
                    }

                    if (item.LPCheckResultList != null && item.LPCheckResultList.Count > 0)
                    {
                        foreach (var checkResult in item.LPCheckResultList)
                        {
                            checkResultExcelItemList.Add(new CheckResultExcelItem()
                            {
                                CheckDate = checkResult.CheckDate,
                                CheckItem = checkResult.CheckItem,
                                CheckTime = checkResult.CheckTime,
                                CheckType = "裝料中檢查",
                                IsAbnormal = checkResult.IsAbnormal ? "異常" : checkResult.IsAlert ? "注意" : "",
                                Island = item.Island,
                                LowerAlertLimit = checkResult.LowerAlertLimit,
                                LowerLimit = checkResult.LowerLimit,
                                Organization = item.OrganizationDescription,
                                Port = item.Port,
                                Result = checkResult.Result,
                                Station = item.Station,
                                TankNo = item.TankNo,
                                Unit = checkResult.Unit,
                                UpperAlertLimit = checkResult.UpperAlertLimit,
                                UpperLimit = checkResult.UpperLimit,
                                User = item.User
                            });
                        }
                    }

                    if (item.LACheckResultList != null && item.LACheckResultList.Count > 0)
                    {
                        foreach (var checkResult in item.LACheckResultList)
                        {
                            checkResultExcelItemList.Add(new CheckResultExcelItem()
                            {
                                CheckDate = checkResult.CheckDate,
                                CheckItem = checkResult.CheckItem,
                                CheckTime = checkResult.CheckTime,
                                CheckType = "裝料後檢查",
                                IsAbnormal = checkResult.IsAbnormal ? "異常" : checkResult.IsAlert ? "注意" : "",
                                Island = item.Island,
                                LowerAlertLimit = checkResult.LowerAlertLimit,
                                LowerLimit = checkResult.LowerLimit,
                                Organization = item.OrganizationDescription,
                                Port = item.Port,
                                Result = checkResult.Result,
                                Station = item.Station,
                                TankNo = item.TankNo,
                                Unit = checkResult.Unit,
                                UpperAlertLimit = checkResult.UpperAlertLimit,
                                UpperLimit = checkResult.UpperLimit,
                                User = item.User
                            });
                        }
                    }

                    if (item.LDCheckResultList != null && item.LDCheckResultList.Count > 0)
                    {
                        foreach (var checkResult in item.LDCheckResultList)
                        {
                            checkResultExcelItemList.Add(new CheckResultExcelItem()
                            {
                                CheckDate = checkResult.CheckDate,
                                CheckItem = checkResult.CheckItem,
                                CheckTime = checkResult.CheckTime,
                                CheckType = "當日裝料完畢檢查",
                                IsAbnormal = checkResult.IsAbnormal ? "異常" : checkResult.IsAlert ? "注意" : "",
                                Island = item.Island,
                                LowerAlertLimit = checkResult.LowerAlertLimit,
                                LowerLimit = checkResult.LowerLimit,
                                Organization = item.OrganizationDescription,
                                Port = item.Port,
                                Result = checkResult.Result,
                                Station = item.Station,
                                TankNo = item.TankNo,
                                Unit = checkResult.Unit,
                                UpperAlertLimit = checkResult.UpperAlertLimit,
                                UpperLimit = checkResult.UpperLimit,
                                User = item.User
                            });
                        }
                    }

                    if (item.UBCheckResultList != null && item.UBCheckResultList.Count > 0)
                    {
                        foreach (var checkResult in item.UBCheckResultList)
                        {
                            checkResultExcelItemList.Add(new CheckResultExcelItem()
                            {
                                CheckDate = checkResult.CheckDate,
                                CheckItem = checkResult.CheckItem,
                                CheckTime = checkResult.CheckTime,
                                CheckType = "卸料前檢查",
                                IsAbnormal = checkResult.IsAbnormal ? "異常" : checkResult.IsAlert ? "注意" : "",
                                Island = item.Island,
                                LowerAlertLimit = checkResult.LowerAlertLimit,
                                LowerLimit = checkResult.LowerLimit,
                                Organization = item.OrganizationDescription,
                                Port = item.Port,
                                Result = checkResult.Result,
                                Station = item.Station,
                                TankNo = item.TankNo,
                                Unit = checkResult.Unit,
                                UpperAlertLimit = checkResult.UpperAlertLimit,
                                UpperLimit = checkResult.UpperLimit,
                                User = item.User
                            });
                        }
                    }

                    if (item.UPCheckResultList != null && item.UPCheckResultList.Count > 0)
                    {
                        foreach (var checkResult in item.UPCheckResultList)
                        {
                            checkResultExcelItemList.Add(new CheckResultExcelItem()
                            {
                                CheckDate = checkResult.CheckDate,
                                CheckItem = checkResult.CheckItem,
                                CheckTime = checkResult.CheckTime,
                                CheckType = "卸料中檢查",
                                IsAbnormal = checkResult.IsAbnormal ? "異常" : checkResult.IsAlert ? "注意" : "",
                                Island = item.Island,
                                LowerAlertLimit = checkResult.LowerAlertLimit,
                                LowerLimit = checkResult.LowerLimit,
                                Organization = item.OrganizationDescription,
                                Port = item.Port,
                                Result = checkResult.Result,
                                Station = item.Station,
                                TankNo = item.TankNo,
                                Unit = checkResult.Unit,
                                UpperAlertLimit = checkResult.UpperAlertLimit,
                                UpperLimit = checkResult.UpperLimit,
                                User = item.User
                            });
                        }
                    }

                    if (item.UACheckResultList != null && item.UACheckResultList.Count > 0)
                    {
                        foreach (var checkResult in item.UACheckResultList)
                        {
                            checkResultExcelItemList.Add(new CheckResultExcelItem()
                            {
                                CheckDate = checkResult.CheckDate,
                                CheckItem = checkResult.CheckItem,
                                CheckTime = checkResult.CheckTime,
                                CheckType = "卸料後檢查",
                                IsAbnormal = checkResult.IsAbnormal ? "異常" : checkResult.IsAlert ? "注意" : "",
                                Island = item.Island,
                                LowerAlertLimit = checkResult.LowerAlertLimit,
                                LowerLimit = checkResult.LowerLimit,
                                Organization = item.OrganizationDescription,
                                Port = item.Port,
                                Result = checkResult.Result,
                                Station = item.Station,
                                TankNo = item.TankNo,
                                Unit = checkResult.Unit,
                                UpperAlertLimit = checkResult.UpperAlertLimit,
                                UpperLimit = checkResult.UpperLimit,
                                User = item.User
                            });
                        }
                    }

                    if (item.UDCheckResultList != null && item.UDCheckResultList.Count > 0)
                    {
                        foreach (var checkResult in item.UDCheckResultList)
                        {
                            checkResultExcelItemList.Add(new CheckResultExcelItem()
                            {
                                CheckDate = checkResult.CheckDate,
                                CheckItem = checkResult.CheckItem,
                                CheckTime = checkResult.CheckTime,
                                CheckType = "當日卸料完畢檢查",
                                IsAbnormal = checkResult.IsAbnormal ? "異常" : checkResult.IsAlert ? "注意" : "",
                                Island = item.Island,
                                LowerAlertLimit = checkResult.LowerAlertLimit,
                                LowerLimit = checkResult.LowerLimit,
                                Organization = item.OrganizationDescription,
                                Port = item.Port,
                                Result = checkResult.Result,
                                Station = item.Station,
                                TankNo = item.TankNo,
                                Unit = checkResult.Unit,
                                UpperAlertLimit = checkResult.UpperAlertLimit,
                                UpperLimit = checkResult.UpperLimit,
                                User = item.User
                            });
                        }
                    }
                }

                IWorkbook workbook = null;

                if (ExcelVersion == Define.EnumExcelVersion._2003)
                {
                    workbook = new HSSFWorkbook();
                }

                if (ExcelVersion == Define.EnumExcelVersion._2007)
                {
                    workbook = new XSSFWorkbook();
                }

                #region WorkBook Style
                IFont headerFont;
                IFont cellFont;

                ICellStyle headerStyle;
                ICellStyle cellStyle;
                #endregion

                #region Header Style
                headerFont = workbook.CreateFont();
                headerFont.Color = HSSFColor.Black.Index;
                headerFont.Boldweight = (short)FontBoldWeight.Bold;
                headerFont.FontHeightInPoints = 12;

                headerStyle = workbook.CreateCellStyle();
                headerStyle.FillForegroundColor = HSSFColor.Grey25Percent.Index;
                headerStyle.FillPattern = FillPattern.SolidForeground;
                headerStyle.BorderTop = BorderStyle.Thin;
                headerStyle.BorderBottom = BorderStyle.Thin;
                headerStyle.BorderLeft = BorderStyle.Thin;
                headerStyle.BorderRight = BorderStyle.Thin;
                headerStyle.SetFont(headerFont);
                #endregion

                #region Cell Style
                cellFont = workbook.CreateFont();
                cellFont.Color = HSSFColor.Black.Index;
                cellFont.Boldweight = (short)FontBoldWeight.Normal;
                cellFont.FontHeightInPoints = 12;

                cellStyle = workbook.CreateCellStyle();
                cellStyle.VerticalAlignment = VerticalAlignment.Center;
                cellStyle.BorderTop = BorderStyle.Thin;
                cellStyle.BorderBottom = BorderStyle.Thin;
                cellStyle.BorderLeft = BorderStyle.Thin;
                cellStyle.BorderRight = BorderStyle.Thin;
                cellStyle.SetFont(cellFont);
                #endregion

                IRow row;

                ICell cell;

                var rowIndex = 0;

                #region Sheet 1
                var worksheet1 = workbook.CreateSheet(Resources.Resource.ArriveRecord);

                row = worksheet1.CreateRow(0);

                #region Header
                cell = row.CreateCell(0);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("組織");

                cell = row.CreateCell(1);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("裝卸料站");

                cell = row.CreateCell(2);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("灌島");

                cell = row.CreateCell(3);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("灌口");

                cell = row.CreateCell(4);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("車牌號碼");

                cell = row.CreateCell(5);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("檢查類別");

                cell = row.CreateCell(6);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("司機");

                cell = row.CreateCell(7);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("上次承載物質");

                cell = row.CreateCell(8);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("本次承載物質");

                cell = row.CreateCell(9);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("貨主");

                cell = row.CreateCell(10);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("檢查人員");

                cell = row.CreateCell(11);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("到位日期");

                cell = row.CreateCell(12);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("到位時間");

                cell = row.CreateCell(13);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("司機簽名");
                #endregion

                rowIndex = 1;

                foreach (var item in arriveRecordExcelItemList)
                {
                    row = worksheet1.CreateRow(rowIndex);

                    row.HeightInPoints = 120;

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.Organization);

                    cell = row.CreateCell(1);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.Station);

                    cell = row.CreateCell(2);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.Island);

                    cell = row.CreateCell(3);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.Port);

                    cell = row.CreateCell(4);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.TankNo);

                    cell = row.CreateCell(5);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.CheckType);

                    cell = row.CreateCell(6);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.Driver);

                    cell = row.CreateCell(7);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.LastTimeMaterial);

                    cell = row.CreateCell(8);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.ThisTimeMaterial);

                    cell = row.CreateCell(9);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.Owner);

                    cell = row.CreateCell(10);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.User);

                    cell = row.CreateCell(11);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.ArriveDate);

                    cell = row.CreateCell(12);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.ArriveTime);

                    cell = row.CreateCell(13);
                    cell.CellStyle = cellStyle;

                    if (!string.IsNullOrEmpty(item.Sign))
                    {
                        try
                        {
                            var signPath = Path.Combine(Config.TankPatrolPhotoFolderPath, item.Sign);

                            byte[] bytes = File.ReadAllBytes(signPath);

                            int pictureIndex = workbook.AddPicture(bytes, PictureType.JPEG);

                            var patriarch = worksheet1.CreateDrawingPatriarch();

                            if (ExcelVersion == Define.EnumExcelVersion._2003)
                            {
                                var anchor = new HSSFClientAnchor(0, 0, 0, 0, 13, rowIndex, 14, rowIndex + 1);

                                patriarch.CreatePicture(anchor, pictureIndex);
                            }

                            if (ExcelVersion == Define.EnumExcelVersion._2007)
                            {
                                var anchor = new XSSFClientAnchor(0, 0, 0, 0, 13, rowIndex, 14, rowIndex + 1);

                                patriarch.CreatePicture(anchor, pictureIndex);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(MethodBase.GetCurrentMethod(), ex);
                        }
                    }
                    
                    rowIndex++;
                }
                #endregion

                for (int cellIndex = 0; cellIndex <= 12; cellIndex++)
                {
                    worksheet1.AutoSizeColumn(cellIndex);
                }

                worksheet1.SetColumnWidth(13, 30 * 256);

                #region Sheet 2
                var worksheet2 = workbook.CreateSheet(Resources.Resource.CheckResult);

                row = worksheet2.CreateRow(0);

                #region Header
                cell = row.CreateCell(0);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("異常");

                cell = row.CreateCell(1);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("組織");

                cell = row.CreateCell(2);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("裝卸料站");

                cell = row.CreateCell(3);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("灌島");

                cell = row.CreateCell(4);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("灌口");

                cell = row.CreateCell(5);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("車牌號碼");

                cell = row.CreateCell(6);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("檢查人員");

                cell = row.CreateCell(7);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("檢查日期");

                cell = row.CreateCell(8);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("檢查時間");

                cell = row.CreateCell(9);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("檢查類別");

                cell = row.CreateCell(10);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("檢查項目");

                cell = row.CreateCell(11);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("檢查結果");

                cell = row.CreateCell(12);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("下限值");

                cell = row.CreateCell(13);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("下限警戒值");

                cell = row.CreateCell(14);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("上限警戒值");

                cell = row.CreateCell(15);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("上限值");

                cell = row.CreateCell(16);
                cell.CellStyle = headerStyle;
                cell.SetCellValue("單位");
                #endregion

                rowIndex = 1;

                foreach (var item in checkResultExcelItemList)
                {
                    row = worksheet2.CreateRow(rowIndex);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.IsAbnormal);

                    cell = row.CreateCell(1);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.Organization);

                    cell = row.CreateCell(2);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.Station);

                    cell = row.CreateCell(3);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.Island);

                    cell = row.CreateCell(4);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.Port);

                    cell = row.CreateCell(5);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.TankNo);

                    cell = row.CreateCell(6);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.User);

                    cell = row.CreateCell(7);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.CheckDate);

                    cell = row.CreateCell(8);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.CheckTime);

                    cell = row.CreateCell(9);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.CheckType);

                    cell = row.CreateCell(10);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.CheckItem);

                    cell = row.CreateCell(11);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.Result);

                    cell = row.CreateCell(12);
                    cell.CellStyle = cellStyle;
                    if (item.LowerLimit.HasValue)
                    {
                        cell.SetCellValue(item.LowerLimit.Value);
                    }
                    
                    cell = row.CreateCell(13);
                    cell.CellStyle = cellStyle;
                    if (item.LowerAlertLimit.HasValue)
                    {
                        cell.SetCellValue(item.LowerAlertLimit.Value);
                    }
                    
                    cell = row.CreateCell(14);
                    cell.CellStyle = cellStyle;
                    if (item.UpperAlertLimit.HasValue)
                    {
                        cell.SetCellValue(item.UpperAlertLimit.Value);
                    }

                    cell = row.CreateCell(15);
                    cell.CellStyle = cellStyle;
                    if (item.UpperLimit.HasValue)
                    {
                        cell.SetCellValue(item.UpperLimit.Value);
                    }

                    cell = row.CreateCell(16);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.Unit);

                    rowIndex++;
                }
                #endregion

                //using (ExcelHelper helper = new ExcelHelper(string.Format("{0}_{1}({2})", "槽車檢查結果", Resources.Resource.ExportTime, DateTimeHelper.DateTime2DateTimeString(DateTime.Now)), ExcelVersion))
                //{
                //    helper.CreateSheet(Resources.Resource.ArriveRecord, arriveRecordExcelItemList);
                //    helper.CreateSheet(Resources.Resource.CheckResult, checkResultExcelItemList);

                //    result.ReturnData(helper.Export());
                //}

                var model = new ExcelExportModel(string.Format("{0}_{1}({2})", "槽車檢查結果", Resources.Resource.ExportTime, DateTimeHelper.DateTime2DateTimeString(DateTime.Now)), ExcelVersion);

                using (var fs = File.OpenWrite(model.FullFileName))
                {
                    workbook.Write(fs);

                    fs.Close();
                }

                byte[] buff = null;

                using (var fs = File.OpenRead(model.FullFileName))
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
    }
}
