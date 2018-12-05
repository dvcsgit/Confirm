using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.Authenticated;
using Models.EquipmentMaintenance.MaintenanceFormManagement;
using System.Transactions;
using System.Text;
using System.Web.Mvc;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.Util;
using System.Net.Mail;

namespace DataAccess.EquipmentMaintenance
{
    public class MaintenanceFormDataAccessor
    {
        #region Reviewed
        public static RequestResult Query(QueryParameters Parameters, List<Models.Shared.UserModel> AccountList, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                var organizationList = Account.QueryableOrganizationUniqueIDList.Intersect(downStreamOrganizationList);

                using (EDbEntities db = new EDbEntities())
                {
                    var query = (from f in db.MForm
                                 join j in db.MJob
                                 on f.MJobUniqueID equals j.UniqueID
                                 join e in db.Equipment
                                 on f.EquipmentUniqueID equals e.UniqueID
                                 join p in db.EquipmentPart
                                 on f.PartUniqueID equals p.UniqueID into tmpPart
                                 from p in tmpPart.DefaultIfEmpty()
                                 where organizationList.Contains(e.OrganizationUniqueID)
                                 select new
                                 {
                                     UniqueID = f.UniqueID,
                                     e.OrganizationUniqueID,
                                     MJobUniqueID = f.MJobUniqueID,
                                     f.VHNO,
                                     Status = f.Status,
                                     CycleBeginDate = f.CycleBeginDate,
                                     CycleEndDate = f.CycleEndDate,
                                     EstBeginDate = f.EstBeginDate,
                                     EstEndDate = f.EstEndDate,
                                     Subject = j.Description,
                                     CreateTime = f.CreateTime,
                                     EquipmentUniqueID = f.EquipmentUniqueID,
                                     EquipmentID = e.ID,
                                     EquipmentName = e.Name,
                                     PartUniqueID = f.PartUniqueID,
                                     PartDescription = p != null ? p.Description : "",
                                     TakeJobTime = f.TakeJobTime,
                                     TakeJobUserID = f.TakeJobUserID,
                                     //TakeJobUserName = u != null ? u.Name : "",
                                     BeginDate = f.BeginDate,
                                     EndDate = f.EndDate
                                 }).Distinct().AsQueryable();

                    if (Parameters.CycleBeginDate.HasValue)
                    {
                        query = query.Where(x => x.CycleBeginDate >= Parameters.CycleBeginDate.Value);
                    }

                    if (Parameters.CycleEndDate.HasValue)
                    {
                        query = query.Where(x => x.CycleEndDate <= Parameters.CycleEndDate.Value);
                    }

                    if (Parameters.EstBeginDate.HasValue)
                    {
                        query = query.Where(x => x.EstBeginDate >= Parameters.EstBeginDate.Value);
                    }

                    if (Parameters.EstEndDate.HasValue)
                    {
                        query = query.Where(x => x.EstEndDate <= Parameters.EstEndDate.Value);
                    }

                    if (!string.IsNullOrEmpty(Parameters.VHNO))
                    {
                        query = query.Where(x => x.VHNO.Contains(Parameters.VHNO));
                    }

                    if (Parameters.StatusList.Count > 0)
                    {
                        if (Parameters.StatusList.Contains("2"))
                        {
                            Parameters.StatusList.Add("1");
                        }

                        query = query.Where(x => Parameters.StatusList.Contains(x.Status));
                    }
                   
                    if (!string.IsNullOrEmpty(Parameters.Subject))
                    {
                        query = query.Where(x => x.Subject.Contains(Parameters.Subject));
                    }

                    var model = new GridViewModel()
                    {
                        FullOrganizationDescription = OrganizationDataAccessor.GetOrganizationFullDescription(Parameters.OrganizationUniqueID)
                    };

                    var itemList = query.ToList();

                    var flowLogList = db.MFormFlowLog.Where(x => !x.VerifyTime.HasValue).ToList();
                    var extendFlowLogList = db.MFormExtendFlowLog.Where(x => !x.VerifyTime.HasValue).ToList();

                    foreach (var item in itemList)
                    {
                        var flow = flowLogList.Where(x => x.MFormUniqueID == item.UniqueID).OrderByDescending(x => x.Seq).FirstOrDefault();
                        var extendFlow = extendFlowLogList.Where(x => x.MFormUniqueID == item.UniqueID).OrderByDescending(x => x.Seq).FirstOrDefault();

                        var itemModel = new GridItem()
                        {
                            UniqueID = item.UniqueID,
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(item.OrganizationUniqueID),
                            VHNO = item.VHNO,
                            Subject = item.Subject,
                            CycleBeginDate = item.CycleBeginDate,
                            CycleEndDate = item.CycleEndDate,
                            EstBeginDate = item.EstBeginDate,
                            EstEndDate = item.EstEndDate,
                            Status = item.Status,
                            CreateTime = item.CreateTime,
                            EquipmentID = item.EquipmentID,
                            EquipmentName = item.EquipmentName,
                            PartDescription = item.PartDescription,
                            TakeJobUserID = item.TakeJobUserID,
                            TakeJobUserName = UserDataAccessor.GetUser(item.TakeJobUserID).Name,
                            TakeJobTime = item.TakeJobTime,
                            BeginDate = item.BeginDate,
                            EndDate = item.EndDate,
                            CurrentVerifyUserID = flow != null ? flow.UserID : string.Empty,
                            CurrentExtendVerifyUserID = extendFlow != null ? extendFlow.UserID : string.Empty
                        };

                        var jobUserList = db.MJobUser.Where(x => x.MJobUniqueID == item.MJobUniqueID).Select(x => x.UserID).ToList();

                        itemModel.JobUserList = (from x in jobUserList
                                                 join y in AccountList
                                                 on x equals y.ID
                                                 select new Models.Shared.UserModel
                                                 {
                                                     OrganizationDescription = y.OrganizationDescription,
                                                     ID = y.ID,
                                                     Name = y.Name
                                                 }).ToList();

                        var maintenanceUserList = db.MFormResult.Where(x => x.MFormUniqueID == item.UniqueID).Select(x => x.UserID).Distinct().ToList();

                        itemModel.MaintenanceUserList = (from x in maintenanceUserList
                                                         join y in AccountList
                                                         on x equals y.ID
                                                         select new Models.Shared.UserModel
                                                         {
                                                             OrganizationDescription = y.OrganizationDescription,
                                                             ID = y.ID,
                                                             Name = y.Name
                                                         }).ToList();

                        model.ItemList.Add(itemModel);
                    }

                    if (Parameters.StatusList.Count > 0)
                    {
                        model.ItemList = model.ItemList.Where(x => Parameters.StatusList.Contains(x.StatusCode)).ToList();
                    }

                    model.ItemList = model.ItemList.OrderByDescending(x => x.VHNO).ToList();

                    result.ReturnData(model);
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

        public static RequestResult Export(List<Models.Shared.UserModel> AccountList, GridViewModel Model, Define.EnumExcelVersion ExcelVersion)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ExcelHelper helper = new ExcelHelper(string.Format("預防保養作業單_({0})", DateTimeHelper.DateTime2DateTimeString(DateTime.Now)), ExcelVersion))
                {
                    helper.CreateSheet<GridItem>(Model.ItemList);

                    result.ReturnData(helper.Export());
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

        public static RequestResult ExportForm(GridViewModel Model, Define.EnumExcelVersion ExcelVersion)
        {
            RequestResult result = new RequestResult();

            try
            {
                IWorkbook workBook = null;

                if (ExcelVersion == Define.EnumExcelVersion._2003)
                {
                    workBook = new HSSFWorkbook();
                }

                if (ExcelVersion == Define.EnumExcelVersion._2007)
                {
                    workBook = new XSSFWorkbook();
                }

                #region 標楷體 16px, UnderLine
                var headerFont = workBook.CreateFont();
                headerFont.FontName = "標楷體";
                headerFont.FontHeightInPoints = 16;
                headerFont.Underline = FontUnderlineType.Single;
                #endregion

                #region Header Style
                var headerStyle = workBook.CreateCellStyle();
                headerStyle.SetFont(headerFont);
                headerStyle.Alignment = HorizontalAlignment.Center;
                headerStyle.VerticalAlignment = VerticalAlignment.Top - 1;
                headerStyle.BorderTop = BorderStyle.None;
                headerStyle.BorderBottom = BorderStyle.None;
                headerStyle.BorderLeft = BorderStyle.None;
                headerStyle.BorderRight = BorderStyle.None;
                #endregion

                #region Cell Style
                var cellFont = workBook.CreateFont();
                cellFont.FontName = "標楷體";
                cellFont.Color = HSSFColor.Black.Index;
                cellFont.Boldweight = (short)FontBoldWeight.Normal;
                cellFont.FontHeightInPoints = 12;

                var cellStyle = workBook.CreateCellStyle();
                cellStyle.VerticalAlignment = VerticalAlignment.Center;
                cellStyle.BorderTop = BorderStyle.Thin;
                cellStyle.BorderBottom = BorderStyle.Thin;
                cellStyle.BorderLeft = BorderStyle.Thin;
                cellStyle.BorderRight = BorderStyle.Thin;
                cellStyle.SetFont(cellFont);

                var cellStyleNoneBorder = workBook.CreateCellStyle();
                cellStyleNoneBorder.VerticalAlignment = VerticalAlignment.Center;
                cellStyleNoneBorder.BorderTop = BorderStyle.None;
                cellStyleNoneBorder.BorderBottom = BorderStyle.None;
                cellStyleNoneBorder.BorderLeft = BorderStyle.None;
                cellStyleNoneBorder.BorderRight = BorderStyle.None;
                cellStyleNoneBorder.SetFont(cellFont);

                var cellStyleAlignCenter = workBook.CreateCellStyle();
                cellStyleAlignCenter.Alignment = HorizontalAlignment.Center;
                cellStyleAlignCenter.VerticalAlignment = VerticalAlignment.Center;
                cellStyleAlignCenter.BorderTop = BorderStyle.Thin;
                cellStyleAlignCenter.BorderBottom = BorderStyle.Thin;
                cellStyleAlignCenter.BorderLeft = BorderStyle.Thin;
                cellStyleAlignCenter.BorderRight = BorderStyle.Thin;
                cellStyleAlignCenter.SetFont(cellFont);

                var cellStyleAlignRight = workBook.CreateCellStyle();
                cellStyleAlignRight.Alignment = HorizontalAlignment.Right;
                cellStyleAlignRight.VerticalAlignment = VerticalAlignment.Center;
                cellStyleAlignRight.BorderTop = BorderStyle.Thin;
                cellStyleAlignRight.BorderBottom = BorderStyle.Thin;
                cellStyleAlignRight.BorderLeft = BorderStyle.Thin;
                cellStyleAlignRight.BorderRight = BorderStyle.Thin;
                cellStyleAlignRight.SetFont(cellFont);
                #endregion

                foreach (var item in Model.ItemList)
                {
                    var worksheet = workBook.CreateSheet(item.VHNO);

                    worksheet.SetColumnWidth(0, 21 * 256);
                    worksheet.SetColumnWidth(1, 40 * 256);
                    worksheet.SetColumnWidth(2, 12 * 256);
                    worksheet.SetColumnWidth(3, 12 * 256);
                    worksheet.SetColumnWidth(4, 12 * 256);
                    worksheet.SetColumnWidth(5, 12 * 256);
                    worksheet.SetColumnWidth(6, 12 * 256);
                    worksheet.SetColumnWidth(7, 12 * 256);

                    worksheet.PrintSetup.Landscape = true;

                    IRow row;

                    ICell cell;

                    #region Row 0
                    row = worksheet.CreateRow(0);

                    cell = row.CreateCell(0);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("定期保養作業單");

                    worksheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 7));
                    #endregion

                    #region Row 1
                    row = worksheet.CreateRow(1);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("單號");

                    cell = row.CreateCell(1);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.VHNO);

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

                    worksheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 7));
                    #endregion

                    #region Row 2
                    row = worksheet.CreateRow(2);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("主旨");

                    cell = row.CreateCell(1);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.Subject);

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

                    worksheet.AddMergedRegion(new CellRangeAddress(2, 2, 1, 7));
                    #endregion

                    #region Row 3
                    row = worksheet.CreateRow(3);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("設備");

                    cell = row.CreateCell(1);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.Equipment);

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

                    worksheet.AddMergedRegion(new CellRangeAddress(3, 3, 1, 7));
                    #endregion

                    #region Row 4
                    row = worksheet.CreateRow(4);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("保養週期(起)");

                    cell = row.CreateCell(1);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.CycleBeginDateString);

                    cell = row.CreateCell(2);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("保養週期(迄)");

                    cell = row.CreateCell(3);
                    cell.CellStyle = cellStyle;

                    worksheet.AddMergedRegion(new CellRangeAddress(4, 4, 2, 3));

                    cell = row.CreateCell(4);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.CycleEndDateString);

                    cell = row.CreateCell(5);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(6);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(7);
                    cell.CellStyle = cellStyle;

                    worksheet.AddMergedRegion(new CellRangeAddress(4, 4, 4, 7));
                    #endregion

                    #region Row 5
                    row = worksheet.CreateRow(5);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("預計保養日期(起)");

                    cell = row.CreateCell(1);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.EstBeginDateString);

                    cell = row.CreateCell(2);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("預計保養日期(迄)");

                    cell = row.CreateCell(3);
                    cell.CellStyle = cellStyle;
                    
                    worksheet.AddMergedRegion(new CellRangeAddress(5, 5, 2, 3));

                    cell = row.CreateCell(4);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.EstEndDateString);

                    cell = row.CreateCell(5);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(6);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(7);
                    cell.CellStyle = cellStyle;

                    worksheet.AddMergedRegion(new CellRangeAddress(5, 5, 4, 7));
                    #endregion

                    #region Row 6
                    row = worksheet.CreateRow(6);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("實際保養日期(起)");

                    cell = row.CreateCell(1);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.BeginDateString);

                    cell = row.CreateCell(2);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("實際保養日期(迄)");

                    cell = row.CreateCell(3);
                    cell.CellStyle = cellStyle;

                    worksheet.AddMergedRegion(new CellRangeAddress(6, 6, 2, 3));

                    cell = row.CreateCell(4);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.EndDateString);

                    cell = row.CreateCell(5);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(6);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(7);
                    cell.CellStyle = cellStyle;

                    worksheet.AddMergedRegion(new CellRangeAddress(6, 6, 4, 7));
                    #endregion

                    #region Row 7
                    row = worksheet.CreateRow(7);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("保養人員");

                    cell = row.CreateCell(1);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.MaintenanceUser);

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

                    worksheet.AddMergedRegion(new CellRangeAddress(7, 7, 1, 7));
                    #endregion

                    #region Row 8
                    row = worksheet.CreateRow(8);

                    cell = row.CreateCell(0);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("保養紀錄");

                    worksheet.AddMergedRegion(new CellRangeAddress(8, 8, 0, 7));
                    #endregion

                    #region Row 9
                    row = worksheet.CreateRow(9);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("保養基準");

                    cell = row.CreateCell(1);
                    cell.CellStyle = cellStyle;

                    worksheet.AddMergedRegion(new CellRangeAddress(9, 9, 0, 1));

                    cell = row.CreateCell(2);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("下限值");

                    cell = row.CreateCell(3);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("下限警戒值");

                    cell = row.CreateCell(4);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("上限警戒值");

                    cell = row.CreateCell(5);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("上限值");

                    cell = row.CreateCell(6);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("單位");

                    cell = row.CreateCell(7);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("結果");
                    #endregion

                    var rowIndex = 10;

                    using (EDbEntities db = new EDbEntities())
                    {
                        var standardList = (from f in db.MForm
                                            join x in db.MJobEquipmentStandard
                                            on new { f.MJobUniqueID, f.EquipmentUniqueID, f.PartUniqueID } equals new { x.MJobUniqueID, x.EquipmentUniqueID, x.PartUniqueID }
                                            join y in db.EquipmentStandard
                                             on new { x.EquipmentUniqueID, x.PartUniqueID, x.StandardUniqueID } equals new { y.EquipmentUniqueID, y.PartUniqueID, y.StandardUniqueID }
                                            join s in db.Standard
                                            on x.StandardUniqueID equals s.UniqueID
                                            where f.UniqueID == item.UniqueID
                                            select new
                                             {
                                                 s.UniqueID,
                                                 s.MaintenanceType,
                                                 s.ID,
                                                 s.Description,
                                                 s.IsFeelItem,
                                                 UpperLimit = y.IsInherit ? s.UpperLimit : y.UpperLimit,
                                                 UpperAlertLimit = y.IsInherit ? s.UpperAlertLimit : y.UpperAlertLimit,
                                                 LowerAlertLimit = y.IsInherit ? s.LowerAlertLimit : y.LowerAlertLimit,
                                                 LowerLimit = y.IsInherit ? s.LowerLimit : y.LowerLimit,
                                                 Unit = y.IsInherit ? s.Unit : y.Unit
                                             }).Distinct().OrderBy(x => x.MaintenanceType).ThenBy(x => x.ID).ToList();

                        foreach (var standard in standardList)
                        {
                            var standardModel = new StandardModel()
                            {
                                UniqueID = standard.UniqueID,
                                ID = standard.ID,
                                Description = standard.Description,
                                IsFeelItem = standard.IsFeelItem,
                                Unit = standard.Unit,
                                LowerLimit = standard.LowerLimit.HasValue ? double.Parse(standard.LowerLimit.Value.ToString()) : default(double?),
                                LowerAlertLimit = standard.LowerAlertLimit.HasValue ? double.Parse(standard.LowerAlertLimit.Value.ToString()) : default(double?),
                                UpperAlertLimit = standard.UpperAlertLimit.HasValue ? double.Parse(standard.UpperAlertLimit.Value.ToString()) : default(double?),
                                UpperLimit = standard.UpperLimit.HasValue ? double.Parse(standard.UpperLimit.Value.ToString()) : default(double?),
                                OptionList = db.StandardFeelOption.Where(o => o.StandardUniqueID == standard.UniqueID).Select(o => new FeelOptionModel
                                {
                                    UniqueID = o.UniqueID,
                                    Description = o.Description,
                                    IsAbnormal = o.IsAbnormal,
                                    Seq = o.Seq
                                }).ToList(),
                                ResultList = (from x in db.MFormStandardResult
                                              join y in db.MFormResult
                                              on x.ResultUniqueID equals y.UniqueID
                                              where x.MFormUniqueID == item.UniqueID && x.StandardUniqueID == standard.UniqueID
                                              select new
                                              {
                                                  UniqueID = y.UniqueID,
                                                  Date = y.PMDate,
                                                  Time = y.PMTime,
                                                  Remark = y.JobRemark,
                                                  UserID = y.UserID,
                                                  UserName = y.UserName
                                              }).Distinct().Select(x => new ResultModel
                                              {
                                                  UniqueID = x.UniqueID,
                                                  Date = x.Date,
                                                  Time = x.Time,
                                                  UserID = x.UserID,
                                                  UserName = x.UserName,
                                                  Remark = x.Remark
                                              }).ToList()
                            };

                            foreach (var r in standardModel.ResultList)
                            {
                                r.Result = db.MFormStandardResult.Where(x => x.MFormUniqueID == item.UniqueID && x.ResultUniqueID == r.UniqueID && x.StandardUniqueID == standardModel.UniqueID).Select(x => new StandardResultModel
                                {
                                    UniqueID = x.UniqueID,
                                    IsAlert = x.IsAlert,
                                    IsAbnormal = x.IsAbnormal,
                                    Result = x.Result
                                }).First();
                            }

                            #region Row
                            row = worksheet.CreateRow(rowIndex);

                            cell = row.CreateCell(0);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(standardModel.Display);

                            cell = row.CreateCell(1);
                            cell.CellStyle = cellStyle;

                            worksheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 0, 1));

                            if (standardModel.IsFeelItem)
                            {
                                cell = row.CreateCell(2);
                                cell.CellStyle = cellStyle;
                                cell.SetCellValue(standardModel.FeelOptions);

                                cell = row.CreateCell(3);
                                cell.CellStyle = cellStyle;
                                cell = row.CreateCell(4);
                                cell.CellStyle = cellStyle;
                                cell = row.CreateCell(5);
                                cell.CellStyle = cellStyle;
                                cell = row.CreateCell(6);
                                cell.CellStyle = cellStyle;

                                worksheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 2, 6));
                            }
                            else
                            {
                                cell = row.CreateCell(2);
                                cell.CellStyle = cellStyle;
                                cell.SetCellValue(standardModel.LowerLimit.HasValue ? standardModel.LowerLimit.Value.ToString() : "");

                                cell = row.CreateCell(3);
                                cell.CellStyle = cellStyle;
                                cell.SetCellValue(standardModel.LowerAlertLimit.HasValue ? standardModel.LowerAlertLimit.Value.ToString() : "");

                                cell = row.CreateCell(4);
                                cell.CellStyle = cellStyle;
                                cell.SetCellValue(standardModel.UpperAlertLimit.HasValue ? standardModel.UpperAlertLimit.Value.ToString() : "");

                                cell = row.CreateCell(5);
                                cell.CellStyle = cellStyle;
                                cell.SetCellValue(standardModel.UpperLimit.HasValue ? standardModel.UpperLimit.Value.ToString() : "");

                                cell = row.CreateCell(6);
                                cell.CellStyle = cellStyle;
                                cell.SetCellValue(standardModel.Unit);
                            }

                            cell = row.CreateCell(7);
                            cell.CellStyle = cellStyle;
                            var t = standardModel.ResultList.OrderByDescending(x=>x.DateTime).FirstOrDefault();
                            if (t != null)
                            {
                                cell.SetCellValue(t.Result.Result);
                            }
                            #endregion

                            rowIndex++;
                        }

                        var workingHourList = db.MFormWorkingHour.Where(x => x.MFormUniqueID == item.UniqueID).ToList().Select(x => new WorkingHourModel()
                        {
                            Seq = x.Seq,
                            User = UserDataAccessor.GetUser(x.UserID),
                            BeginDate = x.BeginDate,
                            EndDate = x.EndDate,
                            WorkingHour = double.Parse(x.WorkingHour.ToString())
                        }).OrderBy(x => x.Seq).ToList();


                        #region Row
                        row = worksheet.CreateRow(rowIndex);

                        cell = row.CreateCell(0);
                        cell.CellStyle = headerStyle;
                        cell.SetCellValue("工時紀錄");

                        worksheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 0, 7));
                        #endregion

                        rowIndex++;

                        row = worksheet.CreateRow(rowIndex);

                        cell = row.CreateCell(0);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue("開始日期");

                        cell = row.CreateCell(1);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue("結束日期");

                        cell = row.CreateCell(2);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue("保養人員");

                        cell = row.CreateCell(3);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue("工時");

                        cell = row.CreateCell(4);
                        cell.CellStyle = cellStyle;

                        cell = row.CreateCell(5);
                        cell.CellStyle = cellStyle;

                        cell = row.CreateCell(6);
                        cell.CellStyle = cellStyle;

                        cell = row.CreateCell(7);
                        cell.CellStyle = cellStyle;

                        worksheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 3, 7));

                        rowIndex++;

                        if (workingHourList.Count > 3)
                        {
                            foreach (var workingHour in workingHourList)
                            {
                                row = worksheet.CreateRow(rowIndex);

                                cell = row.CreateCell(0);
                                cell.CellStyle = cellStyle;
                                cell.SetCellValue(workingHour.BeginDateString);

                                cell = row.CreateCell(1);
                                cell.CellStyle = cellStyle;
                                cell.SetCellValue(workingHour.EndDateString);

                                cell = row.CreateCell(2);
                                cell.CellStyle = cellStyle;
                                cell.SetCellValue(workingHour.User.User);

                                cell = row.CreateCell(3);
                                cell.CellStyle = cellStyle;
                                cell.SetCellValue(workingHour.WorkingHour);

                                cell = row.CreateCell(4);
                                cell.CellStyle = cellStyle;
                                
                                cell = row.CreateCell(5);
                                cell.CellStyle = cellStyle;

                                cell = row.CreateCell(6);
                                cell.CellStyle = cellStyle;

                                cell = row.CreateCell(7);
                                cell.CellStyle = cellStyle;

                                worksheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 3, 7));

                                rowIndex++;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                row = worksheet.CreateRow(rowIndex);

                                if (workingHourList.Count > i)
                                {
                                    var workingHour = workingHourList[i];

                                    cell = row.CreateCell(0);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue(workingHour.BeginDateString);

                                    cell = row.CreateCell(1);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue(workingHour.EndDateString);

                                    cell = row.CreateCell(2);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue(workingHour.User.User);

                                    cell = row.CreateCell(3);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue(workingHour.WorkingHour);

                                    cell = row.CreateCell(4);
                                    cell.CellStyle = cellStyle;
                                    
                                    cell = row.CreateCell(5);
                                    cell.CellStyle = cellStyle;

                                    cell = row.CreateCell(6);
                                    cell.CellStyle = cellStyle;

                                    cell = row.CreateCell(7);
                                    cell.CellStyle = cellStyle;

                                    worksheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 3, 7));

                                    rowIndex++;
                                }
                                else
                                {
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

                                    worksheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 3, 7));

                                    rowIndex++;
                                }
                            }
                        }
                    }
                }

                var output = new ExcelExportModel("定期保樣作業單", ExcelVersion);

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

        public static RequestResult GetDetailViewModel(string UniqueID, List<Models.Shared.UserModel> AccountList)
        {
            RequestResult result = new RequestResult();

            try
            {
                result.ReturnData(new DetailViewModel()
                {
                    UniqueID = UniqueID,
                    FormViewModel = GetFormViewModel(UniqueID, AccountList)
                });
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static FormViewModel GetFormViewModel(string UniqueID, List<Models.Shared.UserModel> AccountList)
        {
            var model = new FormViewModel();

            using (EDbEntities db = new EDbEntities())
            {
                var form = (from f in db.MForm
                            join j in db.MJob
                            on f.MJobUniqueID equals j.UniqueID
                            join e in db.Equipment
                            on f.EquipmentUniqueID equals e.UniqueID
                            join p in db.EquipmentPart
                            on f.PartUniqueID equals p.UniqueID into tmpPart
                            from p in tmpPart.DefaultIfEmpty()
                            where f.UniqueID == UniqueID
                            select new
                            {
                                MaintenanceForm = f,
                                Job = j,
                                Equipment = e,
                                PartDescription = p != null ? p.Description : "",
                            }).First();

                model = new FormViewModel()
                {
                    ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationDescription(form.Equipment.OrganizationUniqueID),
                    VHNO = form.MaintenanceForm.VHNO,
                    EquipmentID = form.Equipment.ID,
                    EquipmentName = form.Equipment.Name,
                    PartDescription = form.PartDescription,
                    Subject = form.Job.Description,
                    CycleBeginDate = form.MaintenanceForm.CycleBeginDate,
                    CycleEndDate = form.MaintenanceForm.CycleEndDate,
                    EstBeginDate = form.MaintenanceForm.EstBeginDate,
                    EstEndDate = form.MaintenanceForm.EstEndDate,
                    BeginDate = form.MaintenanceForm.BeginDate,
                    EndDate = form.MaintenanceForm.EndDate,
                    TakeJobUserID = form.MaintenanceForm.TakeJobUserID,
                    TakeJobUserName = UserDataAccessor.GetUser(form.MaintenanceForm.TakeJobUserID).Name,
                    TakeJobTime = form.MaintenanceForm.TakeJobTime,
                    CreateTime = form.MaintenanceForm.CreateTime,
                    Status = form.MaintenanceForm.Status,
                    FileList = db.MFormFile.Where(f => f.MFormUniqueID == form.MaintenanceForm.UniqueID).ToList().Select(f => new FileModel
                    {
                        Seq = f.Seq,
                        FileName = f.FileName,
                        Extension = f.Extension,
                        Size = f.ContentLength,
                        UploadTime = f.UploadTime,
                        IsSaved = true
                    }).OrderBy(f => f.UploadTime).ToList(),
                    WorkingHourList = db.MFormWorkingHour.Where(x => x.MFormUniqueID == form.MaintenanceForm.UniqueID).ToList().Select(x => new WorkingHourModel()
                    {
                        Seq = x.Seq,
                        User = UserDataAccessor.GetUser(x.UserID),
                        BeginDate = x.BeginDate,
                        EndDate = x.EndDate,
                        WorkingHour = double.Parse(x.WorkingHour.ToString())
                    }).OrderBy(x => x.Seq).ToList(),
                };

                var jobUserList = db.MJobUser.Where(x => x.MJobUniqueID == form.Job.UniqueID).Select(x => x.UserID).ToList();

                model.JobUserList = (from x in jobUserList
                                     join y in AccountList
                                     on x equals y.ID
                                     select new Models.Shared.UserModel
                                     {
                                         OrganizationDescription = y.OrganizationDescription,
                                         ID = y.ID,
                                         Name = y.Name
                                     }).ToList();

                var maintenanceUserList = db.MFormResult.Where(x => x.MFormUniqueID == form.MaintenanceForm.UniqueID).Select(x => x.UserID).Distinct().ToList();

                model.MaintenanceUserList = (from x in maintenanceUserList
                                             join y in AccountList
                                             on x equals y.ID
                                             select new Models.Shared.UserModel
                                             {
                                                 OrganizationDescription = y.OrganizationDescription,
                                                 ID = y.ID,
                                                 Name = y.Name
                                             }).ToList();

                //var standardList = (from f in db.MForm
                //                    join x in db.MJobEquipment
                //                    on new { f.MJobUniqueID, f.EquipmentUniqueID, f.PartUniqueID } equals new { x.MJobUniqueID, x.EquipmentUniqueID, x.PartUniqueID }
                //                    join y in db.EquipmentStandard
                //                    on new { x.EquipmentUniqueID, x.PartUniqueID } equals new { y.EquipmentUniqueID, y.PartUniqueID }
                //                    join s in db.Standard
                //                    on y.StandardUniqueID equals s.UniqueID
                //                    where f.UniqueID == form.MaintenanceForm.UniqueID
                //                    select new
                //                    {
                //                        s.UniqueID,
                //                        s.MaintenanceType,
                //                        s.ID,
                //                        s.Description,
                //                        s.IsFeelItem,
                //                        UpperLimit = y.IsInherit ? s.UpperLimit : y.UpperLimit,
                //                        UpperAlertLimit = y.IsInherit ? s.UpperAlertLimit : y.UpperAlertLimit,
                //                        LowerAlertLimit = y.IsInherit ? s.LowerAlertLimit : y.LowerAlertLimit,
                //                        LowerLimit = y.IsInherit ? s.LowerLimit : y.LowerLimit,
                //                        Unit = y.IsInherit ? s.Unit : y.Unit
                //                    }).Distinct().OrderBy(x => x.MaintenanceType).ThenBy(x => x.ID).ToList();

                var standardList = (from f in db.MForm
                                    join x in db.MJobEquipmentStandard
                                    on new { f.MJobUniqueID, f.EquipmentUniqueID, f.PartUniqueID } equals new { x.MJobUniqueID, x.EquipmentUniqueID, x.PartUniqueID }
                                    join y in db.EquipmentStandard
                                    on new { x.EquipmentUniqueID, x.PartUniqueID, x.StandardUniqueID } equals new { y.EquipmentUniqueID, y.PartUniqueID, y.StandardUniqueID }
                                    join s in db.Standard
                                    on x.StandardUniqueID equals s.UniqueID
                                    where f.UniqueID == form.MaintenanceForm.UniqueID
                                    select new
                                    {
                                        s.UniqueID,
                                        s.MaintenanceType,
                                        s.ID,
                                        s.Description,
                                        s.IsFeelItem,
                                        UpperLimit = y.IsInherit ? s.UpperLimit : y.UpperLimit,
                                        UpperAlertLimit = y.IsInherit ? s.UpperAlertLimit : y.UpperAlertLimit,
                                        LowerAlertLimit = y.IsInherit ? s.LowerAlertLimit : y.LowerAlertLimit,
                                        LowerLimit = y.IsInherit ? s.LowerLimit : y.LowerLimit,
                                        Unit = y.IsInherit ? s.Unit : y.Unit
                                    }).Distinct().OrderBy(x => x.MaintenanceType).ThenBy(x => x.ID).ToList();

                foreach (var standard in standardList)
                {
                    var standardModel = new StandardModel()
                    {
                        UniqueID = standard.UniqueID,
                        ID = standard.ID,
                        Description = standard.Description,
                        IsFeelItem = standard.IsFeelItem,
                        Unit = standard.Unit,
                        LowerLimit = standard.LowerLimit.HasValue ? double.Parse(standard.LowerLimit.Value.ToString()) : default(double?),
                        LowerAlertLimit = standard.LowerAlertLimit.HasValue ? double.Parse(standard.LowerAlertLimit.Value.ToString()) : default(double?),
                        UpperAlertLimit = standard.UpperAlertLimit.HasValue ? double.Parse(standard.UpperAlertLimit.Value.ToString()) : default(double?),
                        UpperLimit = standard.UpperLimit.HasValue ? double.Parse(standard.UpperLimit.Value.ToString()) : default(double?),
                        OptionList = db.StandardFeelOption.Where(o => o.StandardUniqueID == standard.UniqueID).Select(o => new FeelOptionModel
                        {
                            UniqueID = o.UniqueID,
                            Description = o.Description,
                            IsAbnormal = o.IsAbnormal,
                            Seq = o.Seq
                        }).OrderBy(o=>o.Seq).ToList(),
                        ResultList = (from x in db.MFormStandardResult
                                      join y in db.MFormResult
                                      on x.ResultUniqueID equals y.UniqueID
                                      where x.MFormUniqueID == form.MaintenanceForm.UniqueID && x.StandardUniqueID == standard.UniqueID
                                      select new
                                      {
                                          UniqueID = y.UniqueID,
                                          Date = y.PMDate,
                                          Time = y.PMTime,
                                          Remark = y.JobRemark,
                                          UserID = y.UserID,
                                          UserName = y.UserName
                                      }).Distinct().Select(x => new ResultModel
                                      {
                                          UniqueID = x.UniqueID,
                                          Date = x.Date,
                                          Time = x.Time,
                                          UserID = x.UserID,
                                          UserName = x.UserName,
                                          Remark = x.Remark
                                      }).ToList()
                    };

                    foreach (var result in standardModel.ResultList)
                    {
                        result.Result = db.MFormStandardResult.Where(x => x.MFormUniqueID == form.MaintenanceForm.UniqueID && x.ResultUniqueID == result.UniqueID && x.StandardUniqueID == standardModel.UniqueID).Select(x => new StandardResultModel
                        {
                            UniqueID = x.UniqueID,
                            IsAlert = x.IsAlert,
                            IsAbnormal = x.IsAbnormal,
                            Result = x.Result
                        }).First();
                    }

                    model.StandardList.Add(standardModel);
                }

                var materialList = (from f in db.MForm
                                    join x in db.MJobEquipmentMaterial
                                    on new { f.MJobUniqueID, f.EquipmentUniqueID, f.PartUniqueID } equals new { x.MJobUniqueID, x.EquipmentUniqueID, x.PartUniqueID }
                                    join m in db.Material
                                    on x.MaterialUniqueID equals m.UniqueID
                                    join r in db.MFormMaterialResult
                                    on new { MFormUniqueID = f.UniqueID, x.EquipmentUniqueID, x.PartUniqueID, x.MaterialUniqueID } equals new { r.MFormUniqueID, r.EquipmentUniqueID, r.PartUniqueID, r.MaterialUniqueID } into tmpResult
                                    from r in tmpResult.DefaultIfEmpty()
                                    select new
                                    {
                                        m.UniqueID,
                                        m.ID,
                                        m.Name,
                                        x.Quantity,
                                        ChangeQuantity = r != null ? r.ChangeQuantity : default(int?)
                                    }).Distinct().OrderBy(x => x.ID).ToList();

                foreach (var m in materialList)
                {
                    model.MaterialList.Add(new MaterialModel()
                    {
                        UniqueID = m.UniqueID,
                        ID = m.ID,
                        Name = m.Name,
                        Quantity = m.Quantity.Value,
                        ChangeQuantity = m.ChangeQuantity
                    });
                }

                var extends = db.MFormExtend.Where(x => x.MFormUniqueID == form.MaintenanceForm.UniqueID).OrderBy(x => x.Seq).ToList();

                foreach (var extend in extends)
                {
                    var log = db.MFormExtendFlow.FirstOrDefault(x => x.MFormUniqueID == extend.MFormUniqueID && x.ExtendSeq == extend.Seq);

                    var extendLogModel = new ExtendLogModel()
                    {
                        Seq = extend.Seq,
                        OBeginDate = extend.OBeginDate,
                        OEndDate = extend.OEndDate,
                        NBeginDate = extend.NBeginDate,
                        NEndDate = extend.NEndDate,
                        CreateTime = extend.CreateTime,
                        Reason = extend.Reason,
                        IsClosed = log.IsClosed
                    };

                    var extendLogs = db.MFormExtendFlowLog.Where(x => x.MFormUniqueID == form.MaintenanceForm.UniqueID && x.ExtendSeq == extend.Seq).OrderBy(x => x.Seq).ToList();

                    foreach (var extendLog in extendLogs)
                    {
                        extendLogModel.LogList.Add(new ExtendFlowLogModel()
                        {
                            Seq = extendLog.Seq,
                            IsReject = extendLog.IsReject,
                            UserID = extendLog.UserID,
                            NotifyTime = extendLog.NotifyTime,
                            VerifyTime = extendLog.VerifyTime,
                            Remark = extendLog.Remark
                        });
                    }

                    model.ExtendLogList.Add(extendLogModel);
                }

                var logs = db.MFormFlowLog.Where(x => x.MFormUniqueID == form.MaintenanceForm.UniqueID).OrderBy(x => x.Seq).ToList();

                foreach (var log in logs)
                {
                    model.FlowLogList.Add(new FlowLogModel()
                    {
                        Seq = log.Seq,
                        IsReject = log.IsReject,
                        User = UserDataAccessor.GetUser(log.UserID),
                        NotifyTime = log.NotifyTime,
                        VerifyTime = log.VerifyTime,
                        Remark = log.Remark
                    });
                }
            }

            return model;
        }

        public static RequestResult GetEditFormModel(string UniqueID, List<Models.Shared.UserModel> AccountList)
        {
            RequestResult result = new RequestResult();

            try
            {
                result.ReturnData(new EditFormModel()
                {
                    UniqueID = UniqueID,
                    FormViewModel = GetFormViewModel(UniqueID, AccountList)
                });
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult Edit(EditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                    var form = db.MForm.First(x => x.UniqueID == Model.UniqueID);

                    if (Model.FormViewModel.ResultList.Count > 0)
                    {
                        var begin = Model.FormViewModel.ResultList.Min(x => x.Date);
                        var end = Model.FormViewModel.ResultList.Max(x => x.Date);

                        form.BeginDate = DateTimeHelper.DateString2DateTime(begin);
                        form.EndDate = DateTimeHelper.DateString2DateTime(end);
                    }

                    db.SaveChanges();

                    var query = (from f in db.MForm
                                 join j in db.MJob
                                 on f.MJobUniqueID equals j.UniqueID
                                 join e in db.Equipment
                                 on f.EquipmentUniqueID equals e.UniqueID
                                 join p in db.EquipmentPart
                                 on f.PartUniqueID equals p.UniqueID into tmpPart
                                 from p in tmpPart.DefaultIfEmpty()
                                 where f.UniqueID == form.UniqueID
                                 select new
                                 {
                                     MaintenanceForm = f,
                                     Job = j,
                                     Equipment = e,
                                     PartDescription = p != null ? p.Description : ""
                                 }).First();

                    foreach (var r in Model.FormViewModel.ResultList)
                    {
                        var mFormResult = db.MFormResult.FirstOrDefault(x => x.UniqueID == r.UniqueID);

                        if (mFormResult == null)
                        {
                            db.MFormResult.Add(new MFormResult()
                            {
                                UniqueID = r.UniqueID,
                                PMDate = r.Date,
                                PMTime = r.Time,
                                JobRemark = r.Remark,
                                MFormUniqueID = form.UniqueID,
                                UserID = r.UserID,
                                UserName = r.UserName
                            });

                            db.SaveChanges();
                        }

                        if (!db.MFormStandardResult.Any(x => x.UniqueID == r.Result.UniqueID))
                        {
                            if (r.Result.IsAlert || r.Result.IsAbnormal)
                            {
                                var abnormalUniqueID = Guid.NewGuid().ToString();

                                db.Abnormal.Add(new Abnormal()
                                {
                                    UniqueID = abnormalUniqueID
                                });

                                db.AbnormalMFormStandardResult.Add(new AbnormalMFormStandardResult()
                                {
                                    AbnormalUniqueID = abnormalUniqueID,
                                    MFormStandardResultUniqueID = r.Result.UniqueID
                                });
                            }

                            db.MFormStandardResult.Add(new MFormStandardResult()
                            {
                                UniqueID = r.Result.UniqueID,
                                ResultUniqueID = r.UniqueID,
                                OrganizationUniqueID = query.Equipment.OrganizationUniqueID,
                                MFormUniqueID = query.MaintenanceForm.UniqueID,
                                MJobUniqueID = query.MaintenanceForm.MJobUniqueID,
                                MJobDescription = query.Job.Description,
                                EquipmentUniqueID = query.MaintenanceForm.EquipmentUniqueID,
                                EquipmentID = query.Equipment.ID,
                                EquipmentName = query.Equipment.Name,
                                PartUniqueID = query.MaintenanceForm.PartUniqueID,
                                PartDescription = query.PartDescription,
                                StandardUniqueID = r.Result.StandardUniqueID,
                                StandardID = r.Result.StandardID,
                                StandardDescription = r.Result.StandardDescription,
                                FeelOptionUniqueID = r.Result.FeelOptionUniqueID,
                                FeelOptionDescription = r.Result.FeelOptionDescription,
                                IsAbnormal = r.Result.IsAbnormal,
                                IsAlert = r.Result.IsAlert,
                                LowerLimit = r.Result.LowerLimit.HasValue ? double.Parse(r.Result.LowerLimit.Value.ToString()) : default(double?),
                                LowerAlertLimit = r.Result.LowerAlertLimit.HasValue ? double.Parse(r.Result.LowerAlertLimit.Value.ToString()) : default(double?),
                                UpperAlertLimit = r.Result.UpperAlertLimit.HasValue ? double.Parse(r.Result.UpperAlertLimit.Value.ToString()) : default(double?),
                                UpperLimit = r.Result.UpperLimit.HasValue ? double.Parse(r.Result.UpperLimit.Value.ToString()) : default(double?),
                                Unit = r.Result.Unit,
                                Result = r.Result.Result,
                                NetValue = r.Result.NetValue.HasValue ? double.Parse(r.Result.NetValue.Value.ToString()) : default(double?),
                                Value = r.Result.Value.HasValue ? double.Parse(r.Result.Value.Value.ToString()) : default(double?)
                            });

                            db.SaveChanges();
                        }
                    }

                    db.MFormWorkingHour.RemoveRange(db.MFormWorkingHour.Where(x => x.MFormUniqueID == Model.UniqueID).ToList());

                    db.SaveChanges();

                    db.MFormWorkingHour.AddRange(Model.FormViewModel.WorkingHourList.Select(x => new MFormWorkingHour()
                    {
                        MFormUniqueID = Model.UniqueID,
                        Seq = x.Seq,
                        BeginDate = x.BeginDate,
                        EndDate = x.EndDate,
                        WorkingHour = double.Parse(x.WorkingHour.ToString()),
                        UserID = x.User.ID
                    }).ToList());

                    db.SaveChanges();

                    var fileList = db.MFormFile.Where(x => x.MFormUniqueID == form.UniqueID).ToList();

                    foreach (var file in fileList)
                    {
                        if (!Model.FormViewModel.FileList.Any(x => x.Seq == file.Seq))
                        {
                            try
                            {
                                System.IO.File.Delete(Path.Combine(Config.EquipmentMaintenanceFileFolderPath, string.Format("{0}_{1}.{2}", form.UniqueID, file.Seq, file.Extension)));
                            }
                            catch { }
                        }
                    }

                    db.MFormFile.RemoveRange(fileList);

                    db.SaveChanges();

                    foreach (var file in Model.FormViewModel.FileList)
                    {
                        db.MFormFile.Add(new MFormFile()
                        {
                            MFormUniqueID = form.UniqueID,
                            Seq = file.Seq,
                            Extension = file.Extension,
                            FileName = file.FileName,
                            UploadTime = file.UploadTime,
                            ContentLength = file.Size
                        });

                        if (!file.IsSaved)
                        {
                            System.IO.File.Copy(Path.Combine(Config.TempFolder, file.TempFileName), Path.Combine(Config.EquipmentMaintenanceFileFolderPath, string.Format("{0}_{1}.{2}", form.UniqueID, file.Seq, file.Extension)), true);
                            System.IO.File.Delete(Path.Combine(Config.TempFolder, file.TempFileName));
                        }
                    }

                    db.SaveChanges();
#if !DEBUG
                        trans.Complete();
                    }
#endif

                    result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.MaintenanceForm, Resources.Resource.Success));
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

        public static RequestResult GetExtendFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var form = (from f in db.MForm
                                join j in db.MJob
                                on f.MJobUniqueID equals j.UniqueID
                                join e in db.Equipment
                                on f.EquipmentUniqueID equals e.UniqueID
                                join p in db.EquipmentPart
                                on f.PartUniqueID equals p.UniqueID into tmpPart
                                from p in tmpPart.DefaultIfEmpty()
                                where f.UniqueID == UniqueID
                                select new
                                {
                                    MaintenanceForm = f,
                                    Job = j,
                                    Equipment = e,
                                    PartDescription = p != null ? p.Description : ""
                                }).First();

                    result.ReturnData(new ExtendFormModel()
                    {
                        FormUniqueID = form.MaintenanceForm.UniqueID,
                        OBeginDateString = DateTimeHelper.DateTime2DateStringWithSeperator(form.MaintenanceForm.EstBeginDate),
                        OEndDateString = DateTimeHelper.DateTime2DateStringWithSeperator(form.MaintenanceForm.EstEndDate),
                        EquipmentID = form.Equipment.ID,
                        EquipmentName = form.Equipment.Name,
                        PartDescription = form.PartDescription,
                        Subject = form.Job.Description,
                        VHNO = form.MaintenanceForm.VHNO
                    });
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

        public static RequestResult Extend(ExtendFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities edb = new EDbEntities())
                {
                    using(DbEntities db = new DbEntities())
                    {
                        var form = edb.MForm.First(x => x.UniqueID == Model.FormUniqueID);

                        var organizationUniqueID = edb.MJob.First(x => x.UniqueID == form.MJobUniqueID).OrganizationUniqueID;

                        var nextVerifyOrganization = (from f in db.Flow
                                                      join x in db.FlowForm
                                                      on f.UniqueID equals x.FlowUniqueID
                                                      join v in db.FlowVerifyOrganization
                                                      on f.UniqueID equals v.FlowUniqueID
                                                      join o in db.Organization
                                                      on v.OrganizationUniqueID equals o.UniqueID
                                                      where f.OrganizationUniqueID == organizationUniqueID && x.Form == Define.EnumForm.MaintenanceForm.ToString()
                                                      select new
                                                      {
                                                          o.UniqueID,
                                                          o.Description,
                                                          o.ManagerUserID,
                                                          v.Seq
                                                      }).OrderBy(x => x.Seq).FirstOrDefault();

                        //有設定簽核流程
                        if (nextVerifyOrganization != null)
                        {
                            if (!string.IsNullOrEmpty(nextVerifyOrganization.ManagerUserID))
                            {
                                form.Status = "6";

                                var seq = 1;

                                var extends = edb.MFormExtend.Where(x => x.MFormUniqueID == Model.FormUniqueID).ToList();

                                if (extends.Count > 0)
                                {
                                    seq = extends.Max(x => x.Seq) + 1;
                                }

                                edb.MFormExtend.Add(new MFormExtend()
                                {
                                    MFormUniqueID = Model.FormUniqueID,
                                    Seq = seq,
                                    OBeginDate = Model.OBeginDate,
                                    OEndDate = Model.OEndDate,
                                    NBeginDate = Model.FormInput.NBeginDate.Value,
                                    NEndDate = Model.FormInput.NEndDate.Value,
                                    Reason = Model.FormInput.Reason,
                                    CreateTime = DateTime.Now
                                });

                                edb.MFormExtendFlow.Add(new MFormExtendFlow()
                                {
                                    MFormUniqueID = form.UniqueID,
                                    ExtendSeq = seq,
                                    CurrentSeq = 1,
                                    IsClosed = false
                                });

                                edb.MFormExtendFlowLog.Add(new MFormExtendFlowLog()
                                {
                                    MFormUniqueID = form.UniqueID,
                                    ExtendSeq = seq,
                                    Seq = 1,
                                    FlowSeq = nextVerifyOrganization.Seq,
                                    UserID = nextVerifyOrganization.ManagerUserID,
                                    NotifyTime = DateTime.Now
                                });

                                edb.SaveChanges();

                                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Submit, Resources.Resource.Complete));
                            }
                            else
                            {
                                result.ReturnFailedMessage(string.Format("{0} {1} {2} {3}", Resources.Resource.Organization, nextVerifyOrganization.Description, Resources.Resource.NotSet, Resources.Resource.Manager));
                            }
                        }
                        else
                        {
                            var organization = db.Organization.First(x => x.UniqueID == organizationUniqueID);

                            if (!string.IsNullOrEmpty(organization.ManagerUserID))
                            {
                                form.Status = "6";

                                var seq = 1;

                                var extends = edb.MFormExtend.Where(x => x.MFormUniqueID == Model.FormUniqueID).ToList();

                                if (extends.Count > 0)
                                {
                                    seq = extends.Max(x => x.Seq) + 1;
                                }

                                edb.MFormExtend.Add(new MFormExtend()
                                {
                                    MFormUniqueID = Model.FormUniqueID,
                                    Seq = seq,
                                    OBeginDate = Model.OBeginDate,
                                    OEndDate = Model.OEndDate,
                                    NBeginDate = Model.FormInput.NBeginDate.Value,
                                    NEndDate = Model.FormInput.NEndDate.Value,
                                    Reason = Model.FormInput.Reason,
                                    CreateTime = DateTime.Now
                                });

                                edb.MFormExtendFlow.Add(new MFormExtendFlow()
                                {
                                    MFormUniqueID = form.UniqueID,
                                    ExtendSeq = seq,
                                    CurrentSeq = 1,
                                    IsClosed = false
                                });

                                edb.MFormExtendFlowLog.Add(new MFormExtendFlowLog()
                                {
                                    MFormUniqueID = form.UniqueID,
                                    ExtendSeq = seq,
                                    Seq = 1,
                                    FlowSeq = 0,
                                    UserID = organization.ManagerUserID,
                                    NotifyTime = DateTime.Now
                                });

                                edb.SaveChanges();

                                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Submit, Resources.Resource.Complete));
                            }
                            else
                            {
                                result.ReturnFailedMessage(string.Format("{0} {1} {2} {3}", Resources.Resource.Organization, organization.Description, Resources.Resource.NotSet, Resources.Resource.Manager));
                            }
                        }
                    }
                }

                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Submit, Resources.Resource.Complete));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult ExtendApprove(string UniqueID, string Remark, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var form = db.MForm.First(x => x.UniqueID == UniqueID);

                    var organizationUniqueID = db.MJob.First(x => x.UniqueID == form.MJobUniqueID).OrganizationUniqueID;

                    var flow = db.MFormExtendFlow.First(x => x.MFormUniqueID == form.UniqueID && !x.IsClosed);

                    flow.IsClosed = true;

                    var extend = db.MFormExtend.First(x => x.MFormUniqueID == form.UniqueID && x.Seq == flow.ExtendSeq);

                    var currentFlowLog = db.MFormExtendFlowLog.Where(x => x.MFormUniqueID == UniqueID && x.ExtendSeq == flow.ExtendSeq).OrderByDescending(x => x.Seq).First();

                    form.Status = "1";
                    form.EstBeginDate = extend.NBeginDate;
                    form.EstEndDate = extend.NEndDate;

                    currentFlowLog.IsReject = false;
                    currentFlowLog.VerifyTime = DateTime.Now;
                    currentFlowLog.Remark = Remark;

                    db.SaveChanges();

                    result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Approve, Resources.Resource.Complete));
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

        public static RequestResult ExtendReject(string UniqueID, string Remark, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var form = db.MForm.First(x => x.UniqueID == UniqueID);

                    form.Status = "1";

                    var flow = db.MFormExtendFlow.First(x => x.MFormUniqueID == form.UniqueID && !x.IsClosed);

                    flow.IsClosed = true;

                    var currentFlowLog = db.MFormExtendFlowLog.Where(x => x.MFormUniqueID == UniqueID && x.ExtendSeq == flow.ExtendSeq).OrderByDescending(x => x.Seq).First();

                    currentFlowLog.IsReject = true;
                    currentFlowLog.VerifyTime = DateTime.Now;
                    currentFlowLog.Remark = Remark;

                    db.SaveChanges();

                    result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Reject, Resources.Resource.Complete));
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

        public static RequestResult TakeJob(List<string> SelectedList, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var time = DateTime.Now;

                    foreach (var uniqueID in SelectedList)
                    {
                        var maintenanceForm = db.MForm.First(x => x.UniqueID == uniqueID);

                        maintenanceForm.Status = "1";
                        maintenanceForm.TakeJobUserID = Account.ID;
                        maintenanceForm.TakeJobTime = time;
                    }

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.TakeJob, Resources.Resource.Complete));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult Approve(string UniqueID, string Remark, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities edb = new EDbEntities())
                {
                    using (DbEntities db = new DbEntities())
                    {
                        var form = edb.MForm.First(x => x.UniqueID == UniqueID);

                        var organizationUniqueID = edb.MJob.First(x => x.UniqueID == form.MJobUniqueID).OrganizationUniqueID;

                        var flow = edb.MFormFlow.First(x => x.MFormUniqueID == UniqueID);
                        var currentFlowLog = edb.MFormFlowLog.Where(x => x.MFormUniqueID == UniqueID).OrderByDescending(x => x.Seq).First();

                        if (currentFlowLog.FlowSeq == 0)
                        {
                            form.Status = "5";

                            flow.IsClosed = true;

                            currentFlowLog.IsReject = false;
                            currentFlowLog.VerifyTime = DateTime.Now;
                            currentFlowLog.Remark = Remark;

                            edb.SaveChanges();

                            result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Approve, Resources.Resource.Complete));
                        }
                        else
                        {
                            var nextVerifyOrganization = (from f in db.Flow
                                                          join x in db.FlowForm
                                                          on f.UniqueID equals x.FlowUniqueID
                                                          join v in db.FlowVerifyOrganization
                                                          on f.UniqueID equals v.FlowUniqueID
                                                          join o in db.Organization
                                                          on v.OrganizationUniqueID equals o.UniqueID
                                                          where f.OrganizationUniqueID == organizationUniqueID && x.Form == Define.EnumForm.MaintenanceForm.ToString()
                                                          select new
                                                          {
                                                              o.UniqueID,
                                                              o.Description,
                                                              o.ManagerUserID,
                                                              v.Seq
                                                          }).Where(x => x.Seq > currentFlowLog.FlowSeq).OrderBy(x => x.Seq).FirstOrDefault();

                            if (nextVerifyOrganization != null)
                            {
                                if (!string.IsNullOrEmpty(nextVerifyOrganization.ManagerUserID))
                                {
                                    flow.CurrentSeq = flow.CurrentSeq + 1;

                                    currentFlowLog.IsReject = false;
                                    currentFlowLog.VerifyTime = DateTime.Now;
                                    currentFlowLog.Remark = Remark;

                                    edb.MFormFlowLog.Add(new MFormFlowLog()
                                    {
                                        MFormUniqueID = UniqueID,
                                        Seq = flow.CurrentSeq,
                                        FlowSeq = nextVerifyOrganization.Seq,
                                        UserID = nextVerifyOrganization.ManagerUserID,
                                        NotifyTime = DateTime.Now
                                    });

                                    edb.SaveChanges();

                                    SendVerifyMail(UniqueID, nextVerifyOrganization.ManagerUserID);

                                    result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Approve, Resources.Resource.Complete));
                                }
                                else
                                {
                                    result.ReturnFailedMessage(string.Format("{0} {1} {2} {3}", Resources.Resource.Organization, nextVerifyOrganization.Description, Resources.Resource.NotSet, Resources.Resource.Manager));
                                }
                            }
                            else
                            {
                                form.Status = "5";

                                flow.IsClosed = true;

                                currentFlowLog.IsReject = false;
                                currentFlowLog.VerifyTime = DateTime.Now;
                                currentFlowLog.Remark = Remark;

                                edb.SaveChanges();

                                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Approve, Resources.Resource.Complete));
                            }
                        }
                    }
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

        public static RequestResult Reject(string UniqueID, string Remark, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var form = db.MForm.First(x => x.UniqueID == UniqueID);

                    form.Status = "4";

                    var currentFlowLog = db.MFormFlowLog.Where(x => x.MFormUniqueID == UniqueID).OrderByDescending(x => x.Seq).First();

                    currentFlowLog.IsReject = true;
                    currentFlowLog.VerifyTime = DateTime.Now;
                    currentFlowLog.Remark = Remark;

                    db.SaveChanges();

                    result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Reject, Resources.Resource.Complete));
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

        public static RequestResult CreateWorkingHour(List<WorkingHourModel> WorkingHourList, CreateWorkingHourFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                if (string.Compare(Model.FormInput.BeginDate, Model.FormInput.EndDate) > 0)
                {
                    result.ReturnFailedMessage(Resources.Resource.BeginDateGreaterThanEndDate);
                }
                else
                {
                    int seq = 1;

                    if (WorkingHourList.Count > 0)
                    {
                        seq = WorkingHourList.Max(x => x.Seq) + 1;
                    }

                    WorkingHourList.Add(new WorkingHourModel()
                    {
                        Seq = seq,
                        BeginDate = Model.FormInput.BeginDate,
                        EndDate = Model.FormInput.EndDate,
                        WorkingHour = Model.FormInput.WorkingHour,
                        User = UserDataAccessor.GetUser(Model.FormInput.UserID)
                    });

                    WorkingHourList = WorkingHourList.OrderBy(x => x.Seq).ToList();

                    result.ReturnData(WorkingHourList);
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

        public static RequestResult CreateResult(EditFormModel Model, string Remark, List<string> StandardResultStringList, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var uniqueID = Guid.NewGuid().ToString();

                var tmp = new ResultModel()
                {
                    UniqueID = uniqueID,
                    Date = DateTimeHelper.DateTime2DateString(DateTime.Now),
                    Time = DateTimeHelper.DateTime2TimeString(DateTime.Now),
                    Remark = Remark,
                    UserID = Account.ID,
                    UserName = Account.Name
                };

                foreach (var standardResultString in StandardResultStringList)
                {
                    string[] temp = standardResultString.Split(Define.Seperators, StringSplitOptions.None);

                    var standardUniqueID = temp[0];
                    var feelOptionUniqueID = temp[1];
                    var value = temp[2];

                    var standard = Model.FormViewModel.StandardList.First(x => x.UniqueID == standardUniqueID);

                    if (!string.IsNullOrEmpty(feelOptionUniqueID))
                    {
                        var feelOption = standard.OptionList.First(x => x.UniqueID == feelOptionUniqueID);

                        standard.ResultList.Add(new ResultModel()
                        {
                            UniqueID = tmp.UniqueID,
                            Date = tmp.Date,
                            Time = tmp.Time,
                            Remark = tmp.Remark,
                            UserID = tmp.UserID,
                            UserName = tmp.UserName,
                            Result = new StandardResultModel()
                            {
                                UniqueID = Guid.NewGuid().ToString(),
                                StandardUniqueID = standard.UniqueID,
                                StandardID = standard.ID,
                                StandardDescription = standard.Description,
                                FeelOptionUniqueID = feelOption.UniqueID,
                                FeelOptionDescription = feelOption.Description,
                                Value = null,
                                NetValue = null,
                                Result = feelOption.IsAbnormal ? string.Format("{0}({1})", feelOption.Description, Resources.Resource.Abnormal) : feelOption.Description,
                                IsAbnormal = feelOption.IsAbnormal,
                                IsAlert = false,
                                LowerAlertLimit = standard.LowerAlertLimit,
                                LowerLimit = standard.LowerLimit,
                                UpperLimit = standard.UpperLimit,
                                UpperAlertLimit = standard.UpperAlertLimit,
                                Unit = standard.Unit
                            }
                        });
                    }
                    else if (!string.IsNullOrEmpty(value))
                    {
                        var val = double.Parse(value);

                        var isAbnormal = false;
                        var isAlert = false;

                        if (standard.LowerLimit.HasValue && val < standard.LowerLimit.Value)
                        {
                            isAbnormal = true;
                        }
                        else if (standard.UpperLimit.HasValue && val > standard.UpperLimit.Value)
                        {
                            isAbnormal = true;
                        }
                        else if (standard.LowerAlertLimit.HasValue && val < standard.LowerAlertLimit.Value)
                        {
                            isAlert = true;
                        }
                        else if (standard.UpperAlertLimit.HasValue && val > standard.UpperAlertLimit.Value)
                        {
                            isAlert = true;
                        }

                        standard.ResultList.Add(new ResultModel()
                        {
                            UniqueID = tmp.UniqueID,
                            Date = tmp.Date,
                            Time = tmp.Time,
                            Remark = tmp.Remark,
                            UserID = tmp.UserID,
                            UserName = tmp.UserName,
                            Result = new StandardResultModel()
                            {
                                UniqueID = Guid.NewGuid().ToString(),
                                StandardUniqueID = standard.UniqueID,
                                StandardID = standard.ID,
                                StandardDescription = standard.Description,
                                FeelOptionUniqueID = string.Empty,
                                FeelOptionDescription = string.Empty,
                                Value = val,
                                NetValue = val,
                                Result = isAbnormal ? string.Format("{0}({1})", val.ToString("F2"), Resources.Resource.Abnormal) : isAlert ? string.Format("{0}({1})", val.ToString("F2"), Resources.Resource.Warning) : val.ToString("F2"),
                                IsAbnormal = isAbnormal,
                                IsAlert = isAlert,
                                LowerAlertLimit = standard.LowerAlertLimit,
                                LowerLimit = standard.LowerLimit,
                                UpperLimit = standard.UpperLimit,
                                UpperAlertLimit = standard.UpperAlertLimit,
                                Unit = standard.Unit
                            }
                        });
                    }
                }

                //Model.FormViewModel.ResultList.Add(model);

                //Model.FormViewModel.ResultList = Model.FormViewModel.ResultList.OrderBy(x => x.DateTime).ToList();

                result.ReturnData(Model.FormViewModel.StandardList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetUserOptions(List<Models.Shared.UserModel> AccountList, string Term, bool IsInit)
        {
            RequestResult result = new RequestResult();

            try
            {
                var query = AccountList.Select(x => new
                {
                    ID = x.ID,
                    Name = x.Name,
                    OrganizationDescription = x.OrganizationDescription
                }).ToList();

                if (!string.IsNullOrEmpty(Term))
                {
                    if (IsInit)
                    {
                        query = query.Where(x => x.ID == Term).ToList();
                    }
                    else
                    {
                        var term = Term.ToLower();

                        query = query.Where(x => x.ID.Contains(term) || x.Name.Contains(term)).ToList();
                    }
                }

                result.ReturnData(query.Select(x => new SelectListItem { Value = x.ID, Text = string.Format("{0}/{1}/{2}", x.OrganizationDescription, x.ID, x.Name) }).Distinct().ToList());
            }
            catch (Exception ex)
            {
                Error err = new Error(MethodInfo.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static FileDownloadModel GetFile(string MFormUniqueID, int Seq)
        {
            var model = new FileDownloadModel();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var file = db.MFormFile.First(x => x.MFormUniqueID == MFormUniqueID && x.Seq == Seq);

                    model = new FileDownloadModel()
                    {
                        FormUniqueID = file.MFormUniqueID,
                        Seq = file.Seq,
                        FileName = file.FileName,
                        Extension = file.Extension
                    };
                }
            }
            catch (Exception ex)
            {
                model = null;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return model;
        }
        #endregion













        public static RequestResult Submit(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities edb = new EDbEntities())
                {
                    using (DbEntities db = new DbEntities())
                    {
                        var form = edb.MForm.First(x => x.UniqueID == UniqueID);

                        var organizationUniqueID = edb.MJob.First(x => x.UniqueID == form.MJobUniqueID).OrganizationUniqueID;

                        var nextVerifyOrganization = (from f in db.Flow
                                                      join x in db.FlowForm
                                                      on f.UniqueID equals x.FlowUniqueID
                                                      join v in db.FlowVerifyOrganization
                                                      on f.UniqueID equals v.FlowUniqueID
                                                      join o in db.Organization
                                                      on v.OrganizationUniqueID equals o.UniqueID
                                                      where f.OrganizationUniqueID == organizationUniqueID && x.Form == Define.EnumForm.MaintenanceForm.ToString()
                                                      select new
                                                      {
                                                          o.UniqueID,
                                                          o.Description,
                                                          o.ManagerUserID,
                                                          v.Seq
                                                      }).OrderBy(x => x.Seq).FirstOrDefault();

                        //有設定簽核流程
                        if (nextVerifyOrganization != null)
                        {
                            if (!string.IsNullOrEmpty(nextVerifyOrganization.ManagerUserID))
                            {
                                form.Status = "3";

                                var flow = edb.MFormFlow.FirstOrDefault(x => x.MFormUniqueID == UniqueID);

                                int currentSeq = 1;

                                if (flow == null)
                                {
                                    edb.MFormFlow.Add(new MFormFlow()
                                    {
                                        MFormUniqueID = UniqueID,
                                        CurrentSeq = currentSeq,
                                        IsClosed = false
                                    });
                                }
                                else
                                {
                                    currentSeq = flow.CurrentSeq + 1;

                                    flow.CurrentSeq = currentSeq;
                                }

                                edb.MFormFlowLog.Add(new MFormFlowLog()
                                {
                                    MFormUniqueID = UniqueID,
                                    Seq = currentSeq,
                                    FlowSeq = nextVerifyOrganization.Seq,
                                    UserID = nextVerifyOrganization.ManagerUserID,
                                    NotifyTime = DateTime.Now
                                });

                                edb.SaveChanges();

                                SendVerifyMail(UniqueID, nextVerifyOrganization.ManagerUserID);

                                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Submit, Resources.Resource.Complete));
                            }
                            else
                            {
                                result.ReturnFailedMessage(string.Format("{0} {1} {2} {3}", Resources.Resource.Organization, nextVerifyOrganization.Description, Resources.Resource.NotSet, Resources.Resource.Manager));
                            }
                        }
                        else
                        {
                            var organization = db.Organization.First(x => x.UniqueID == organizationUniqueID);

                            if (!string.IsNullOrEmpty(organization.ManagerUserID))
                            {
                                form.Status = "3";

                                var flow = edb.MFormFlow.FirstOrDefault(x => x.MFormUniqueID == UniqueID);

                                int currentSeq = 1;

                                if (flow == null)
                                {
                                    edb.MFormFlow.Add(new MFormFlow()
                                    {
                                        MFormUniqueID = UniqueID,
                                        CurrentSeq = currentSeq,
                                        IsClosed = false
                                    });
                                }
                                else
                                {
                                    currentSeq = flow.CurrentSeq + 1;

                                    flow.CurrentSeq = currentSeq;
                                }

                                edb.MFormFlowLog.Add(new MFormFlowLog()
                                {
                                    MFormUniqueID = UniqueID,
                                    Seq = currentSeq,
                                    FlowSeq = 0,
                                    UserID = organization.ManagerUserID,
                                    NotifyTime = DateTime.Now
                                });

                                edb.SaveChanges();

                                SendVerifyMail(UniqueID, organization.ManagerUserID);

                                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Submit, Resources.Resource.Complete));
                            }
                            else
                            {
                                result.ReturnFailedMessage(string.Format("{0} {1} {2} {3}", Resources.Resource.Organization, organization.Description, Resources.Resource.NotSet, Resources.Resource.Manager));
                            }
                        }
                    }
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

        private static void SendVerifyMail(string UniqueID, string UserID)
        {
            try
            {
                var recipientList = new List<MailAddress>();

                using (DbEntities db = new DbEntities())
                {
                    using(EDbEntities edb = new EDbEntities())
                    {
                        var user = db.User.FirstOrDefault(x => x.ID == UserID);

                        if (user != null && !string.IsNullOrEmpty(user.Email))
                        {
                            recipientList.Add(new MailAddress(user.Email, user.Name));
                        }

                        if (recipientList.Count > 0)
                        {
                            var mform = (from f in edb.MForm
                                         join j in edb.MJob
                                         on f.MJobUniqueID equals j.UniqueID
                                         where f.UniqueID == UniqueID
                                         select new
                                         {
                                             f.VHNO,
                                           DESCRIPTION=  j.Description,
                                          CYCLEBEGINDATE=   f.CycleBeginDate,
                                            CYCLEENDDATE= f.CycleEndDate,
                                            ORGANIZATIONUNIQUEID= j.OrganizationUniqueID
                                         }).First();

                            var begin = DateTimeHelper.DateTime2DateStringWithSeperator(mform.CYCLEBEGINDATE);
                            var end = DateTimeHelper.DateTime2DateStringWithSeperator(mform.CYCLEENDDATE);

                            var mDate = string.Empty;

                            if (begin == end)
                            {
                                mDate = begin;
                            }
                            else
                            {
                                mDate = string.Format("{0}~{1}", begin, end);
                            }

                            var subject = string.Format("[核簽通知][定期保養作業單][{0}]{1}({2})", mform.VHNO, mform.DESCRIPTION, mDate);

                            var th = "<th style=\"width:150px;border:1px solid #333;padding:8px;text-align:right;color:#707070;background: #F4F4F4;\">{0}</th>";
                            var td = "<td style=\"width:400px;border:1px solid #333;padding:8px;color:#707070;\">{0}</td>";

                            var sb = new StringBuilder();

                            sb.Append("<table>");

                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "部門"));
                            sb.Append(string.Format(td, OrganizationDataAccessor.GetOrganizationDescription(mform.ORGANIZATIONUNIQUEID)));
                            sb.Append("</tr>");

                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "定期保養派工描述"));
                            sb.Append(string.Format(td, mform.DESCRIPTION));
                            sb.Append("</tr>");

                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "保養週期"));
                            sb.Append(string.Format(td, mDate));
                            sb.Append("</tr>");

#if ASE
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "連結"));
                            sb.Append(string.Format(td, string.Format("<a href=\"http://ASECL-eINSPRD01/FEM.Portal/Home/Index?ReturnUrl=http://ASECL-eINSPRD01/FEM/zh-tw/EquipmentMaintenance/MaintenanceForm/Index?VHNO={0}\">連結</a>", mform.VHNO)));
                            sb.Append("</tr>");
#endif

                            sb.Append("</table>");

                            MailHelper.SendMail(recipientList, subject, sb.ToString());
                        }   
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }


    }
}
