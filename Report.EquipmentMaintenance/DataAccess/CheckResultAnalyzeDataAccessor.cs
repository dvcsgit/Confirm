using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using NPOI.SS.Util;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
#if ORACLE
using DbEntity.ORACLE.EquipmentMaintenance;
#else
using DbEntity.MSSQL.EquipmentMaintenance;
#endif
using Models.Authenticated;
using DataAccess;
using Utility;
using Utility.Models;
using Report.EquipmentMaintenance.Models.CheckResultAnalyze;

namespace Report.EquipmentMaintenance.DataAccess
{
    public class CheckResultAnalyzeDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
//                var downSteamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

//                using (EDbEntities db = new EDbEntities())
//                {
//                    result.ReturnData((from c in db.CheckResult
//                                       join a in db.ArriveRecord
//                                       on c.ArriveRecordUniqueID equals a.UniqueID
//                                       join f in db.RForm
//                                       on c.UniqueID equals f.CheckResultUniqueID into tmpRForm
//                                       from f in tmpRForm.DefaultIfEmpty()
//                                       where downSteamOrganizationList.Contains(c.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(c.OrganizationUniqueID) && string.Compare(c.CheckDate, Parameters.BeginDate) >= 0 && string.Compare(c.CheckDate, Parameters.EndDate) <= 0
//                                       select new GridItem
//                                       {
//                                           RouteID = c.RouteID,
//                                           RouteName = c.RouteName,
//                                           JobDescription = c.JobDescription,
//                                           ControlPointID = c.ControlPointID,
//                                           ControlPointDescription = c.ControlPointDescription,
//                                           EquipmentID = c.EquipmentID,
//                                           EquipmentName = c.EquipmentName,
//                                           PartDescription = c.PartDescription,
//                                           CheckItemID = c.CheckItemID,
//                                           CheckItemDescription = c.CheckItemDescription,
//                                           LowerLimit = c.LowerLimit,
//                                           LowerAlertLimit = c.LowerAlertLimit,
//                                           UpperAlertLimit = c.UpperAlertLimit,
//                                           UpperLimit = c.UpperLimit,
//                                           CheckDate = c.CheckDate,
//                                           CheckTime = c.CheckTime,
//                                           Result = c.Result,
//#if ORACLE
//                                           IsAbnormal = c.IsAbnormal==1,
//#else
//                                           IsAbnormal = c.IsAbnormal,
//#endif
//                                           UserID = a.UserID,
//                                           UserName = a.UserName,
//                                           VHNO = f != null ? f.VHNO : "",
//                                           AbnormalReasonList = db.CheckResultAbnormalReason.Where(x => x.CheckResultUniqueID == c.UniqueID).Select(x => new AbnormalReasonModel
//                                           {
//                                               Description = x.AbnormalReasonDescription,
//                                               Remark = x.AbnormalReasonRemark
//                                           }).ToList()
//                                       }).OrderBy(x => x.RouteID).ThenBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList());
//                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }
        public static ExcelExportModel Export(List<GridItem> ItemList, Define.EnumExcelVersion ExcelVersion)
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

                ISheet sheet = wk.CreateSheet(Resources.Resource.CheckResultAnalyze);

                sheet.DefaultColumnWidth = 18;     //设置单元格长度
                sheet.DefaultRowHeight = 400;      //设置单元格高度
                sheet.CreateRow(0).CreateCell(0).SetCellValue(Resources.Resource.CheckResultAnalyze);
                sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 13));
                sheet.GetRow(0).CreateCell(14).SetCellValue(DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTime.Now));

                IRow row2 = sheet.CreateRow(1);
                row2.CreateCell(0).SetCellValue(Resources.Resource.Route);
                row2.CreateCell(1).SetCellValue(Resources.Resource.ControlPoint);
                row2.CreateCell(2).SetCellValue(Resources.Resource.Equipment);
                row2.CreateCell(3).SetCellValue(Resources.Resource.CheckItem);
                row2.CreateCell(4).SetCellValue(Resources.Resource.LowerLimit);
                row2.CreateCell(5).SetCellValue(Resources.Resource.LowerAlertLimit);
                row2.CreateCell(6).SetCellValue(Resources.Resource.UpperAlertLimit);
                row2.CreateCell(7).SetCellValue(Resources.Resource.UpperLimit);
                row2.CreateCell(8).SetCellValue(Resources.Resource.CheckResult);
                row2.CreateCell(10).SetCellValue(Resources.Resource.Status);
                row2.CreateCell(9).SetCellValue(Resources.Resource.AbnormalReason);
                row2.CreateCell(11).SetCellValue(Resources.Resource.CheckUser);
                row2.CreateCell(12).SetCellValue(Resources.Resource.CheckDate);
                row2.CreateCell(13).SetCellValue(Resources.Resource.CheckTime);
                row2.CreateCell(14).SetCellValue(Resources.Resource.VHNO);

                var rowIndex = 2;

                foreach (var item in ItemList)
                {
                    var row = sheet.CreateRow(rowIndex);

                    var cell = row.CreateCell(0);
                    cell.SetCellValue(item.Route);

                    cell = row.CreateCell(1);
                    cell.SetCellValue(item.ControlPoint);

                    cell = row.CreateCell(2);
                    cell.SetCellValue(item.Equipment);

                    cell = row.CreateCell(3);
                    cell.SetCellValue(item.CheckItem);

                    cell = row.CreateCell(4);
                    cell.SetCellValue(item.LowerLimit.HasValue?item.LowerLimit.Value.ToString():"");

                    cell = row.CreateCell(5);
                    cell.SetCellValue(item.LowerAlertLimit.HasValue ? item.LowerAlertLimit.Value.ToString() : "");

                    cell = row.CreateCell(6);
                    cell.SetCellValue(item.UpperAlertLimit.HasValue ? item.UpperAlertLimit.Value.ToString() : "");

                    cell = row.CreateCell(7);
                    cell.SetCellValue(item.UpperLimit.HasValue ? item.UpperLimit.Value.ToString() : "");

                    cell = row.CreateCell(8);
                    cell.SetCellValue(item.Result);

                    cell = row.CreateCell(9);
                    cell.SetCellValue(item.IsAbnormal ? Resources.Resource.Abnormal : Resources.Resource.Normal);

                    cell = row.CreateCell(10);
                    cell.SetCellValue(item.AbnormalReasons);

                    cell = row.CreateCell(11);
                    cell.SetCellValue(item.User);

                    cell = row.CreateCell(12);
                    cell.SetCellValue(item.CheckDate);

                    cell = row.CreateCell(13);
                    cell.SetCellValue(item.CheckTime);

                    cell = row.CreateCell(14);
                    cell.SetCellValue(item.VHNO);

                    rowIndex++;
                }

                var model = new ExcelExportModel(Resources.Resource.CheckResultAnalyze, ExcelVersion);

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
