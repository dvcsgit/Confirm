using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.ASE;
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

namespace DataAccess.ASE
{
	public class MaintenanceFormDataAccessor
    {
        #region Reviewed
        public static RequestResult Query(Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new SummaryViewModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = new List<GridItem>();

                    if (db.ORGANIZATION.Any(x => x.MANAGERUSERID == Account.ID))
                    {
                        query = (from x in db.MFORM
                                 join j in db.MJOB
                                 on x.MJOBUNIQUEID equals j.UNIQUEID
                                 where Account.QueryableOrganizationUniqueIDList.Contains(j.ORGANIZATIONUNIQUEID) && (x.STATUS == "0" || x.STATUS == "1" || x.STATUS == "4")
                                 select new GridItem
                                 {
                                     UniqueID = x.UNIQUEID,
                                     Status = x.STATUS,
                                     EstEndDate = x.ESTENDDATE,
                                 }).ToList();
                    }
                    else
                    {
                        query = (from u in db.MJOBUSER
                                 join j in db.MJOB
                                 on u.MJOBUNIQUEID equals j.UNIQUEID
                                 join x in db.MFORM
                                 on j.UNIQUEID equals x.MJOBUNIQUEID
                                 where u.USERID == Account.ID && (x.STATUS == "0" || x.STATUS == "1" || x.STATUS == "4")
                                 select new GridItem
                                 {
                                     UniqueID = x.UNIQUEID,
                                     Status = x.STATUS,
                                     EstEndDate = x.ESTENDDATE,
                                 }).ToList();
                    }

                    model.ItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-orange",
                        Icon = "fa-download",
                        Count = query.Count(x => x.StatusCode == "0"),
                        Text = Resources.Resource.MFormStatus_0,
                        Status = 0
                    });

                    model.ItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-blue",
                        Icon = "fa-wrench",
                        Count = query.Count(x => x.StatusCode == "1"),
                        Text = Resources.Resource.MFormStatus_1,
                        Status = 1
                    });

                    model.ItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-exclamation-circle",
                        Count = query.Count(x => x.StatusCode == "2"),
                        Text = Resources.Resource.MFormStatus_2,
                        Status = 2
                    });

                    model.ItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-times-circle-o",
                        Count = query.Count(x => x.StatusCode == "4"),
                        Text = Resources.Resource.MFormStatus_4,
                        Status = 4
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

        public static RequestResult Query(QueryParameters Parameters, List<Models.Shared.UserModel> AccountList, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                var organizationList = Account.QueryableOrganizationUniqueIDList.Intersect(downStreamOrganizationList);

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = (from f in db.MFORM
                                 join j in db.MJOB
                                 on f.MJOBUNIQUEID equals j.UNIQUEID
                                 join e in db.EQUIPMENT
                                 on f.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                 join p in db.EQUIPMENTPART
                                 on f.PARTUNIQUEID equals p.UNIQUEID into tmpPart
                                 from p in tmpPart.DefaultIfEmpty()
                                 join u in db.ACCOUNT
                                 on f.TAKEJOBUSERID equals u.ID into tmpTakeJobUser
                                 from u in tmpTakeJobUser.DefaultIfEmpty()
                                 where organizationList.Contains(e.ORGANIZATIONUNIQUEID)
                                 select new
                                 {
                                     UniqueID = f.UNIQUEID,
                                     //j.ORGANIZATIONUNIQUEID,
                                     e.ORGANIZATIONUNIQUEID,
                                     MJobUniqueID = f.MJOBUNIQUEID,
                                     f.VHNO,
                                     Status = f.STATUS,
                                     CycleBeginDate = f.CYCLEBEGINDATE,
                                     CycleEndDate = f.CYCLEENDDATE,
                                     EstBeginDate = f.ESTBEGINDATE,
                                     EstEndDate = f.ESTENDDATE,
                                     Subject = j.DESCRIPTION,
                                     CreateTime = f.CREATETIME,
                                     EquipmentUniqueID = f.EQUIPMENTUNIQUEID,
                                     EquipmentID = e.ID,
                                     EquipmentName = e.NAME,
                                     PartUniqueID = f.PARTUNIQUEID,
                                     PartDescription = p != null ? p.DESCRIPTION : "",
                                     TakeJobTime = f.TAKEJOBTIME,
                                     TakeJobUserID = f.TAKEJOBUSERID,
                                     TakeJobUserName = u != null ? u.NAME : "",
                                     BeginDate = f.BEGINDATE,
                                     EndDate = f.ENDDATE
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

                        if (Parameters.StatusList.Contains("0"))
                        {
                            Parameters.StatusList.Add("7");
                        }

                        query = query.Where(x => Parameters.StatusList.Contains(x.Status));
                    }

                    //if (!string.IsNullOrEmpty(Parameters.Status))
                    //{
                    //    if (Parameters.Status == "2")
                    //    {
                    //        query = query.Where(x => x.Status == "1");
                    //    }
                    //    else
                    //    {
                    //        query = query.Where(x => x.Status == Parameters.Status);
                    //    }
                    //}

                    if (!string.IsNullOrEmpty(Parameters.Subject))
                    {
                        query = query.Where(x => x.Subject.Contains(Parameters.Subject));
                    }

                    var model = new GridViewModel()
                    {
                        FullOrganizationDescription = OrganizationDataAccessor.GetOrganizationFullDescription(Parameters.OrganizationUniqueID)
                    };

                    var itemList = query.ToList();

                    var flowLogList = db.MFORMFLOWLOG.Where(x => !x.VERIFYTIME.HasValue).ToList();
                    var extendFlowLogList = db.MFORMEXTENDFLOWLOG.Where(x => !x.VERIFYTIME.HasValue).ToList();

                    foreach (var item in itemList)
                    {
                        var flow = flowLogList.Where(x => x.MFORMUNIQUEID == item.UniqueID).OrderByDescending(x => x.SEQ).FirstOrDefault();
                        var extendFlow = extendFlowLogList.Where(x => x.MFORMUNIQUEID == item.UniqueID).OrderByDescending(x => x.SEQ).FirstOrDefault();

                        var itemModel = new GridItem()
                        {
                            UniqueID = item.UniqueID,
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(item.ORGANIZATIONUNIQUEID),
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
                            TakeJobUserName = item.TakeJobUserName,
                            TakeJobTime = item.TakeJobTime,
                            BeginDate = item.BeginDate,
                            EndDate = item.EndDate,
                            CurrentVerifyUserID = flow != null ? flow.USERID : string.Empty,
                            CurrentExtendVerifyUserID = extendFlow != null ? extendFlow.USERID : string.Empty
                        };

                        var jobUserList = db.MJOBUSER.Where(x => x.MJOBUNIQUEID == item.MJobUniqueID).Select(x => x.USERID).ToList();

                        itemModel.JobUserList = (from x in jobUserList
                                                 join y in AccountList
                                                 on x equals y.ID
                                                 select new Models.Shared.UserModel
                                                 {
                                                     OrganizationDescription = y.OrganizationDescription,
                                                     ID = y.ID,
                                                     Name = y.Name
                                                 }).ToList();

                        var maintenanceUserList = db.MFORMRESULT.Where(x => x.MFORMUNIQUEID == item.UniqueID).Select(x => x.USERID).Distinct().ToList();

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

                    //if (!string.IsNullOrEmpty(Parameters.Status) && Parameters.Status == "2")
                    //{
                    //    model.ItemList = model.ItemList.Where(x => x.StatusCode == Parameters.Status).ToList();
                    //}

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
                var excelItemList = new List<ExcelItem>();

                foreach (var item in Model.ItemList)
                {
                    var tmp = GetDetailViewModel(item.UniqueID, AccountList);

                    var model = tmp.Data as DetailViewModel;

                    foreach (var s in model.FormViewModel.StandardList)
                    {
                        excelItemList.Add(new ExcelItem()
                        {
                            IsAbnormal = s.IsAbnormal ? "Y" : "N",
                            IsAlert = s.IsAlert ? "Y" : "N",
                            BeginDate = item.BeginDateString,
                            EndDate = item.EndDateString,
                            CycleBeginDate = item.CycleBeginDateString,
                            CycleEndDate = item.CycleEndDateString,
                            Equipment = item.Equipment,
                            EstBeginDate = item.EstBeginDateString,
                            EstEndDate = item.EstEndDateString,
                            LowerLimit = s.LowerLimit,
                            LowerAlertLimit = s.LowerAlertLimit,
                            UpperAlertLimit = s.UpperAlertLimit,
                            UpperLimit = s.UpperLimit,
                            MaintenanceUser = item.MaintenanceUser,
                            OrganizationDescription = item.OrganizationDescription,
                            Result = s.ResultList != null && s.ResultList.Count > 0 ? s.ResultList[0].Result.Result : "",
                            Standard = s.Display,
                            Status = item.StatusDescription,
                            Subject = item.Subject,
                            TakeJobTime = item.TakeJobTimeString,
                            TakeJobUser = item.TakeJobUser,
                            Unit = s.Unit,
                            VHNO = item.VHNO
                        });
                    }
                }

                using (ExcelHelper helper = new ExcelHelper(string.Format("預防保養作業單_({0})", DateTimeHelper.DateTime2DateTimeString(DateTime.Now)), ExcelVersion))
                {
                    helper.CreateSheet<GridItem>("預防保養作業單",Model.ItemList);

                    helper.CreateSheet<ExcelItem>("保養紀錄", excelItemList);

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
                    cell.SetCellValue(item.CycleEndDateString);

                    worksheet.AddMergedRegion(new CellRangeAddress(4, 4, 3, 7));
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
                    cell.SetCellValue(item.EstEndDateString);

                    worksheet.AddMergedRegion(new CellRangeAddress(5, 5, 3, 7));
                    #endregion

                    #region Row 6
                    row = worksheet.CreateRow(0);

                    cell = row.CreateCell(0);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("保養紀錄");

                    worksheet.AddMergedRegion(new CellRangeAddress(6, 6, 0, 7));
                    #endregion

                    #region Row 7
                    row = worksheet.CreateRow(7);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("保養日期");

                    cell = row.CreateCell(1);
                    cell.CellStyle = cellStyle;
                    var mDate = string.Empty;

                    if (!string.IsNullOrEmpty(item.BeginDateString) && !string.IsNullOrEmpty(item.EndDateString))
                    {
                        mDate = string.Format("{0}~{1}", item.BeginDateString, item.EndDateString);
                    }
                    else if (!string.IsNullOrEmpty(item.BeginDateString) && string.IsNullOrEmpty(item.EndDateString))
                    {
                        mDate = item.BeginDateString;
                    }
                    else if (string.IsNullOrEmpty(item.BeginDateString) && !string.IsNullOrEmpty(item.EndDateString))
                    {
                        mDate = item.EndDateString;
                    }
                    else
                    {
                        mDate = string.Empty;
                    }
                    cell.SetCellValue(mDate);

                    cell = row.CreateCell(2);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("保養人員");

                    cell = row.CreateCell(3);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(item.MaintenanceUser);

                    worksheet.AddMergedRegion(new CellRangeAddress(7, 7, 3, 7));
                    #endregion

                    #region Row 8
                    row = worksheet.CreateRow(8);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("保養基準");

                    worksheet.AddMergedRegion(new CellRangeAddress(8, 8, 0, 1));

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

                    var rowIndex = 9;

                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        var standardList = (from f in db.MFORM
                                            join x in db.MJOBEQUIPMENTSTANDARD
                                            on new { f.MJOBUNIQUEID, f.EQUIPMENTUNIQUEID, f.PARTUNIQUEID } equals new { x.MJOBUNIQUEID ,x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID}
                                            join y in db.EQUIPMENTSTANDARD
                                             on new { x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID, x.STANDARDUNIQUEID } equals new { y.EQUIPMENTUNIQUEID, y.PARTUNIQUEID, y.STANDARDUNIQUEID }
                                            join s in db.STANDARD
                                            on x.STANDARDUNIQUEID equals s.UNIQUEID
                                            where f.UNIQUEID == item.UniqueID
                                            select new
                                            {
                                               UniqueID= s.UNIQUEID,
                                               MaintenanceType= s.MAINTENANCETYPE,
                                                s.ID,
                                              Description=  s.DESCRIPTION,
                                              IsFeelItem=  s.ISFEELITEM=="Y",
                                               UpperLimit = y.ISINHERIT == "Y" ? s.UPPERLIMIT : y.UPPERLIMIT,
                                               UpperAlertLimit = y.ISINHERIT == "Y" ? s.UPPERALERTLIMIT : y.UPPERALERTLIMIT,
                                               LowerAlertLimit = y.ISINHERIT == "Y" ? s.LOWERALERTLIMIT : y.LOWERALERTLIMIT,
                                               LowerLimit = y.ISINHERIT == "Y" ? s.LOWERLIMIT : y.LOWERLIMIT,
                                               Unit = y.ISINHERIT == "Y" ? s.UNIT : y.UNIT
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
                                OptionList = db.STANDARDFEELOPTION.Where(o => o.STANDARDUNIQUEID == standard.UniqueID).Select(o => new FeelOptionModel
                                {
                                    UniqueID = o.UNIQUEID,
                                    Description = o.DESCRIPTION,
                                    IsAbnormal = o.ISABNORMAL=="Y",
                                    Seq = o.SEQ.Value
                                }).ToList()
                            };

                            #region Row
                            row = worksheet.CreateRow(rowIndex);

                            cell = row.CreateCell(0);
                            cell.CellStyle = cellStyle;
                            cell.SetCellValue(standardModel.Display);

                            worksheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 0, 1));

                            if (standardModel.IsFeelItem)
                            {
                                cell = row.CreateCell(2);
                                cell.CellStyle = cellStyle;
                                cell.SetCellValue(standardModel.FeelOptions);

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
                            cell.SetCellValue("");
                            #endregion

                            rowIndex++;
                        }
                    }

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
                    cell.SetCellValue("工時");

                    worksheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 2, 7));

                    for (int i = 1; i <= 3; i++)
                    {
                        rowIndex = rowIndex + i;

                        row = worksheet.CreateRow(rowIndex);

                        cell = row.CreateCell(0);
                        cell.CellStyle = cellStyle;

                        cell = row.CreateCell(1);
                        cell.CellStyle = cellStyle;

                        cell = row.CreateCell(2);
                        cell.CellStyle = cellStyle;

                        worksheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 2, 7));
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

            using (ASEDbEntities db = new ASEDbEntities())
            {
                var form = (from f in db.MFORM
                            join j in db.MJOB
                            on f.MJOBUNIQUEID equals j.UNIQUEID
                            join e in db.EQUIPMENT
                            on f.EQUIPMENTUNIQUEID equals e.UNIQUEID
                            join p in db.EQUIPMENTPART
                            on f.PARTUNIQUEID equals p.UNIQUEID into tmpPart
                            from p in tmpPart.DefaultIfEmpty()
                            join u in db.ACCOUNT
                            on f.TAKEJOBUSERID equals u.ID into tmpTakeJobUser
                            from u in tmpTakeJobUser.DefaultIfEmpty()
                            where f.UNIQUEID == UniqueID
                            select new
                            {
                                MaintenanceForm = f,
                                Job = j,
                                Equipment = e,
                                PartDescription = p != null ? p.DESCRIPTION : "",
                                TakeJobUserName = u != null ? u.NAME : ""
                            }).First();

                model = new FormViewModel()
                {
                    ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationDescription(form.Equipment.ORGANIZATIONUNIQUEID),
                    VHNO = form.MaintenanceForm.VHNO,
                    EquipmentID = form.Equipment.ID,
                    EquipmentName = form.Equipment.NAME,
                    PartDescription = form.PartDescription,
                    Subject = form.Job.DESCRIPTION,
                    CycleBeginDate = form.MaintenanceForm.CYCLEBEGINDATE,
                    CycleEndDate = form.MaintenanceForm.CYCLEENDDATE,
                    EstBeginDate = form.MaintenanceForm.ESTBEGINDATE,
                    EstEndDate = form.MaintenanceForm.ESTENDDATE,
                    BeginDate = form.MaintenanceForm.BEGINDATE,
                    EndDate = form.MaintenanceForm.ENDDATE,
                    TakeJobUserID = form.MaintenanceForm.TAKEJOBUSERID,
                    TakeJobUserName = form.TakeJobUserName,
                    TakeJobTime = form.MaintenanceForm.TAKEJOBTIME,
                    CreateTime = form.MaintenanceForm.CREATETIME,
                    Status = form.MaintenanceForm.STATUS,
                    FileList = db.MFORMFILE.Where(f => f.MFORMUNIQUEID == form.MaintenanceForm.UNIQUEID).ToList().Select(f => new FileModel
                    {
                        Seq = f.SEQ,
                        FileName = f.FILENAME,
                        Extension = f.EXTENSION,
                        Size = f.CONTENTLENGTH,
                        UploadTime = f.UPLOADTIME,
                        IsSaved = true
                    }).OrderBy(f => f.UploadTime).ToList(),
                    WorkingHourList = db.MFORMWORKINGHOUR.Where(x => x.MFORMUNIQUEID == form.MaintenanceForm.UNIQUEID).ToList().Select(x => new WorkingHourModel()
                    {
                        Seq = x.SEQ,
                        User = UserDataAccessor.GetUser(x.USERID),
                        BeginDate = x.BEGINDATE,
                        EndDate = x.ENDDATE,
                        WorkingHour = double.Parse(x.WORKINGHOUR.ToString())
                    }).OrderBy(x => x.Seq).ToList(),
                    //ResultList = db.MFORMRESULT.Where(x => x.MFORMUNIQUEID == form.MaintenanceForm.UNIQUEID).ToList().Select(x => new ResultModel
                    //{
                    //    UniqueID = x.UNIQUEID,
                    //    Date = x.PMDATE,
                    //    Time = x.PMTIME,
                    //    Remark = x.JOBREMARK,
                    //    UserID = x.USERID,
                    //    UserName = x.USERNAME,
                    //    ResultList = db.MFORMSTANDARDRESULT.Where(r => r.RESULTUNIQUEID == x.UNIQUEID).ToList().Select(r => new StandardResultModel
                    //    {
                    //        ID = r.STANDARDID,
                    //        Description = r.STANDARDDESCRIPTION,
                    //        IsAbnormal = r.ISABNORMAL == "Y",
                    //        IsAlert = r.ISALERT == "Y",
                    //        Result = r.RESULT,
                    //        FeelOptionUniqueID = r.FEELOPTIONUNIQUEID,
                    //        FeelOptionDescription = r.FEELOPTIONDESCRIPTION,
                    //        LowerLimit = r.LOWERLIMIT.HasValue ? double.Parse(r.LOWERLIMIT.Value.ToString()) : default(double?),
                    //        LowerAlertLimit = r.LOWERALERTLIMIT.HasValue ? double.Parse(r.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                    //        UpperAlertLimit = r.UPPERALERTLIMIT.HasValue ? double.Parse(r.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                    //        UpperLimit = r.UPPERLIMIT.HasValue ? double.Parse(r.UPPERLIMIT.Value.ToString()) : default(double?),
                    //        Unit = r.UNIT,
                    //        StandardUniqueID = r.STANDARDUNIQUEID,
                    //        NetValue = r.NETVALUE.HasValue ? double.Parse(r.NETVALUE.Value.ToString()) : default(double?),
                    //        Value = r.VALUE.HasValue ? double.Parse(r.VALUE.Value.ToString()) : default(double?),
                    //    }).OrderBy(r => r.ID).ToList()
                    //}).OrderBy(x => x.Date).ThenBy(x => x.Time).ToList()
                };

                var jobUserList = db.MJOBUSER.Where(x => x.MJOBUNIQUEID == form.Job.UNIQUEID).Select(x => x.USERID).ToList();

                model.JobUserList = (from x in jobUserList
                                     join y in AccountList
                                     on x equals y.ID
                                     select new Models.Shared.UserModel
                                     {
                                         OrganizationDescription = y.OrganizationDescription,
                                         ID = y.ID,
                                         Name = y.Name
                                     }).ToList();

                var maintenanceUserList = db.MFORMRESULT.Where(x => x.MFORMUNIQUEID == form.MaintenanceForm.UNIQUEID).Select(x => x.USERID).Distinct().ToList();

                model.MaintenanceUserList = (from x in maintenanceUserList
                                             join y in AccountList
                                             on x equals y.ID
                                             select new Models.Shared.UserModel
                                             {
                                                 OrganizationDescription = y.OrganizationDescription,
                                                 ID = y.ID,
                                                 Name = y.Name
                                             }).ToList();

                var standardList = (from x in db.MJOBEQUIPMENTSTANDARD
                                    join y in db.EQUIPMENTSTANDARD
                                    on new { x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID, x.STANDARDUNIQUEID } equals new { y.EQUIPMENTUNIQUEID, y.PARTUNIQUEID, y.STANDARDUNIQUEID }
                                    join s in db.STANDARD
                                    on x.STANDARDUNIQUEID equals s.UNIQUEID
                                    where x.MJOBUNIQUEID == form.MaintenanceForm.MJOBUNIQUEID && x.EQUIPMENTUNIQUEID == form.MaintenanceForm.EQUIPMENTUNIQUEID && x.PARTUNIQUEID==form.MaintenanceForm.PARTUNIQUEID
                                    select new
                                    {
                                        s.UNIQUEID,
                                        s.MAINTENANCETYPE,
                                        s.ID,
                                        s.DESCRIPTION,
                                        s.ISFEELITEM,
                                        UpperLimit = y.ISINHERIT == "Y" ? s.UPPERLIMIT : y.UPPERLIMIT,
                                        UpperAlertLimit = y.ISINHERIT == "Y" ? s.UPPERALERTLIMIT : y.UPPERALERTLIMIT,
                                        LowerAlertLimit = y.ISINHERIT == "Y" ? s.LOWERALERTLIMIT : y.LOWERALERTLIMIT,
                                        LowerLimit = y.ISINHERIT == "Y" ? s.LOWERLIMIT : y.LOWERLIMIT,
                                        Unit = y.ISINHERIT == "Y" ? s.UNIT : y.UNIT
                                    }).Distinct().OrderBy(x => x.MAINTENANCETYPE).ThenBy(x => x.ID).ToList();

                foreach (var standard in standardList)
                {
                    var standardModel = new StandardModel()
                    {
                        UniqueID = standard.UNIQUEID,
                        ID = standard.ID,
                        Description = standard.DESCRIPTION,
                        IsFeelItem = standard.ISFEELITEM == "Y",
                        Unit = standard.Unit,
                        LowerLimit = standard.LowerLimit.HasValue ? double.Parse(standard.LowerLimit.Value.ToString()) : default(double?),
                        LowerAlertLimit = standard.LowerAlertLimit.HasValue ? double.Parse(standard.LowerAlertLimit.Value.ToString()) : default(double?),
                        UpperAlertLimit = standard.UpperAlertLimit.HasValue ? double.Parse(standard.UpperAlertLimit.Value.ToString()) : default(double?),
                        UpperLimit = standard.UpperLimit.HasValue ? double.Parse(standard.UpperLimit.Value.ToString()) : default(double?),
                        OptionList = db.STANDARDFEELOPTION.Where(o => o.STANDARDUNIQUEID == standard.UNIQUEID).Select(o => new FeelOptionModel
                        {
                            UniqueID = o.UNIQUEID,
                            Description = o.DESCRIPTION,
                            IsAbnormal = o.ISABNORMAL == "Y",
                            Seq = o.SEQ.Value
                        }).OrderBy(o=>o.Seq).ToList(),
                        ResultList = (from x in db.MFORMSTANDARDRESULT
                                      join y in db.MFORMRESULT
                                      on x.RESULTUNIQUEID equals y.UNIQUEID
                                      where x.MFORMUNIQUEID == form.MaintenanceForm.UNIQUEID && x.STANDARDUNIQUEID == standard.UNIQUEID
                                      select new
                                      {
                                          UniqueID = y.UNIQUEID,
                                          Date = y.PMDATE,
                                          Time = y.PMTIME,
                                          Remark = y.JOBREMARK,
                                          UserID = y.USERID,
                                          UserName = y.USERNAME
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
                        result.Result = db.MFORMSTANDARDRESULT.Where(x => x.MFORMUNIQUEID == form.MaintenanceForm.UNIQUEID && x.RESULTUNIQUEID == result.UniqueID && x.STANDARDUNIQUEID == standardModel.UniqueID).Select(x => new StandardResultModel
                        {
                            UniqueID = x.UNIQUEID,
                            IsAlert = x.ISALERT == "Y",
                            IsAbnormal = x.ISABNORMAL == "Y",
                            Result = x.RESULT
                        }).First();
                    }

                    model.StandardList.Add(standardModel);
                }

                var materialList = (from f in db.MFORM
                                    join x in db.MJOBEQUIPMENTMATERIAL
                                    on new { f.MJOBUNIQUEID, f.EQUIPMENTUNIQUEID, f.PARTUNIQUEID } equals new { x.MJOBUNIQUEID, x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID }
                                    join m in db.MATERIAL
                                    on x.MATERIALUNIQUEID equals m.UNIQUEID
                                    join r in db.MFORMMATERIALRESULT
                                    on new { MFORMUNIQUEID = f.UNIQUEID, x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID, x.MATERIALUNIQUEID } equals new { r.MFORMUNIQUEID, r.EQUIPMENTUNIQUEID, r.PARTUNIQUEID, r.MATERIALUNIQUEID } into tmpResult
                                    from r in tmpResult.DefaultIfEmpty()
                                    select new
                                    {
                                        m.UNIQUEID,
                                        m.ID,
                                        m.NAME,
                                        x.QUANTITY,
                                        CHANGEQUANTITY = r != null ? r.CHANGEQUANTITY : default(int?)
                                    }).Distinct().OrderBy(x => x.ID).ToList();

                foreach (var m in materialList)
                {
                    model.MaterialList.Add(new MaterialModel()
                    {
                        UniqueID = m.UNIQUEID,
                        ID = m.ID,
                        Name = m.NAME,
                        Quantity = m.QUANTITY,
                        ChangeQuantity = m.CHANGEQUANTITY
                    });
                }

                var extends = db.MFORMEXTEND.Where(x => x.MFORMUNIQUEID == form.MaintenanceForm.UNIQUEID).OrderBy(x => x.SEQ).ToList();

                foreach (var extend in extends)
                {
                    var log = db.MFORMEXTENDFLOW.FirstOrDefault(x => x.MFORMUNIQUEID == extend.MFORMUNIQUEID && x.EXTENDSEQ == extend.SEQ);

                    var extendLogModel = new ExtendLogModel()
                    {
                        Seq = extend.SEQ,
                        OBeginDate = extend.OBEGINDATE,
                        OEndDate = extend.OENDDATE,
                        NBeginDate = extend.NBEGINDATE,
                        NEndDate = extend.NENDDATE,
                        CreateTime = extend.CREATETIME,
                        Reason = extend.REASON,
                        IsClosed = log != null && log.ISCLOSED == "Y"
                    };

                    var extendLogs = db.MFORMEXTENDFLOWLOG.Where(x => x.MFORMUNIQUEID == form.MaintenanceForm.UNIQUEID && x.EXTENDSEQ==extend.SEQ).OrderBy(x => x.SEQ).ToList();

                    foreach (var extendLog in extendLogs)
                    {
                        extendLogModel.LogList.Add(new ExtendFlowLogModel()
                        {
                            Seq = extendLog.SEQ,
                            IsReject = string.IsNullOrEmpty(extendLog.ISREJECT) ? default(bool?) : extendLog.ISREJECT == "Y",
                            UserID = extendLog.USERID,
                            NotifyTime = extendLog.NOTIFYTIME,
                            VerifyTime = extendLog.VERIFYTIME,
                            Remark = extendLog.REMARK
                        });
                    }

                    model.ExtendLogList.Add(extendLogModel);
                }

                var logs = db.MFORMFLOWLOG.Where(x => x.MFORMUNIQUEID == form.MaintenanceForm.UNIQUEID).OrderBy(x => x.SEQ).ToList();

                foreach (var log in logs)
                {
                    model.FlowLogList.Add(new FlowLogModel()
                    {
                        Seq = log.SEQ,
                        IsReject = string.IsNullOrEmpty(log.ISREJECT) ? default(bool?) : log.ISREJECT == "Y",
                        User = UserDataAccessor.GetUser(log.USERID),
                        NotifyTime = log.NOTIFYTIME,
                        VerifyTime = log.VERIFYTIME,
                        Remark = log.REMARK
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                        var form = db.MFORM.First(x => x.UNIQUEID == Model.UniqueID);

                        if (Model.FormViewModel.ResultList.Count > 0)
                        {
                            var begin = Model.FormViewModel.ResultList.Min(x => x.Date);
                            var end = Model.FormViewModel.ResultList.Max(x => x.Date);

                            form.BEGINDATE = DateTimeHelper.DateString2DateTime(begin);
                            form.ENDDATE = DateTimeHelper.DateString2DateTime(end);
                        }

                        db.SaveChanges();

                        var query = (from f in db.MFORM
                                     join j in db.MJOB
                                     on f.MJOBUNIQUEID equals j.UNIQUEID
                                     join e in db.EQUIPMENT
                                     on f.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                     join p in db.EQUIPMENTPART
                                     on f.PARTUNIQUEID equals p.UNIQUEID into tmpPart
                                     from p in tmpPart.DefaultIfEmpty()
                                     where f.UNIQUEID == form.UNIQUEID
                                     select new
                                     {
                                         MaintenanceForm = f,
                                         Job = j,
                                         Equipment = e,
                                         PartDescription = p != null ? p.DESCRIPTION : ""
                                     }).First();

                        foreach (var r in Model.FormViewModel.ResultList)
                        {
                            var mFormResult = db.MFORMRESULT.FirstOrDefault(x => x.UNIQUEID == r.UniqueID);

                            if (mFormResult == null)
                            {
                                db.MFORMRESULT.Add(new MFORMRESULT()
                                {
                                    UNIQUEID = r.UniqueID,
                                    PMDATE = r.Date,
                                    PMTIME = r.Time,
                                    JOBREMARK = r.Remark,
                                    MFORMUNIQUEID = form.UNIQUEID,
                                    USERID = r.UserID,
                                    USERNAME = r.UserName
                                });

                                db.SaveChanges();
                            }

                            if (!db.MFORMSTANDARDRESULT.Any(x => x.UNIQUEID == r.Result.UniqueID))
                            {
                                if (r.Result.IsAlert || r.Result.IsAbnormal)
                                {
                                    var abnormalUniqueID = Guid.NewGuid().ToString();

                                    db.ABNORMAL.Add(new ABNORMAL()
                                    {
                                        UNIQUEID = abnormalUniqueID
                                    });

                                    db.ABNORMALMFORMSTANDARDRESULT.Add(new ABNORMALMFORMSTANDARDRESULT()
                                    {
                                        ABNORMALUNIQUEID = abnormalUniqueID,
                                        MFORMSTANDARDRESULTUNIQUEID = r.Result.UniqueID
                                    });
                                }

                                db.MFORMSTANDARDRESULT.Add(new MFORMSTANDARDRESULT()
                                {
                                    UNIQUEID = r.Result.UniqueID,
                                    RESULTUNIQUEID = r.UniqueID,
                                    ORGANIZATIONUNIQUEID = query.Equipment.ORGANIZATIONUNIQUEID,
                                    MFORMUNIQUEID = query.MaintenanceForm.UNIQUEID,
                                    MJOBUNIQUEID = query.MaintenanceForm.MJOBUNIQUEID,
                                    MJOBDESCRIPTION = query.Job.DESCRIPTION,
                                    EQUIPMENTUNIQUEID = query.MaintenanceForm.EQUIPMENTUNIQUEID,
                                    EQUIPMENTID = query.Equipment.ID,
                                    EQUIPMENTNAME = query.Equipment.NAME,
                                    PARTUNIQUEID = query.MaintenanceForm.PARTUNIQUEID,
                                    PARTDESCRIPTION = query.PartDescription,
                                    STANDARDUNIQUEID = r.Result.StandardUniqueID,
                                    STANDARDID = r.Result.StandardID,
                                    STANDARDDESCRIPTION = r.Result.StandardDescription,
                                    FEELOPTIONUNIQUEID = r.Result.FeelOptionUniqueID,
                                    FEELOPTIONDESCRIPTION = r.Result.FeelOptionDescription,
                                    ISABNORMAL = r.Result.IsAbnormal ? "Y" : "N",
                                    ISALERT = r.Result.IsAlert ? "Y" : "N",
                                    LOWERLIMIT = r.Result.LowerLimit.HasValue ? decimal.Parse(r.Result.LowerLimit.Value.ToString()) : default(decimal?),
                                    LOWERALERTLIMIT = r.Result.LowerAlertLimit.HasValue ? decimal.Parse(r.Result.LowerAlertLimit.Value.ToString()) : default(decimal?),
                                    UPPERALERTLIMIT = r.Result.UpperAlertLimit.HasValue ? decimal.Parse(r.Result.UpperAlertLimit.Value.ToString()) : default(decimal?),
                                    UPPERLIMIT = r.Result.UpperLimit.HasValue ? decimal.Parse(r.Result.UpperLimit.Value.ToString()) : default(decimal?),
                                    UNIT = r.Result.Unit,
                                    RESULT = r.Result.Result,
                                    NETVALUE = r.Result.NetValue.HasValue ? decimal.Parse(r.Result.NetValue.Value.ToString()) : default(decimal?),
                                    VALUE = r.Result.Value.HasValue ? decimal.Parse(r.Result.Value.Value.ToString()) : default(decimal?)
                                });

                                db.SaveChanges();
                            }
                        }

                        db.MFORMWORKINGHOUR.RemoveRange(db.MFORMWORKINGHOUR.Where(x => x.MFORMUNIQUEID == Model.UniqueID).ToList());

                        db.SaveChanges();

                        db.MFORMWORKINGHOUR.AddRange(Model.FormViewModel.WorkingHourList.Select(x => new MFORMWORKINGHOUR()
                        {
                            MFORMUNIQUEID = Model.UniqueID,
                            SEQ = x.Seq,
                            BEGINDATE = x.BeginDate,
                            ENDDATE = x.EndDate,
                            WORKINGHOUR = decimal.Parse(x.WorkingHour.ToString()),
                            USERID = x.User.ID
                        }).ToList());

                        db.SaveChanges();

                        var fileList = db.MFORMFILE.Where(x => x.MFORMUNIQUEID == form.UNIQUEID).ToList();

                        foreach (var file in fileList)
                        {
                            if (!Model.FormViewModel.FileList.Any(x => x.Seq == file.SEQ))
                            {
                                try
                                {
                                    System.IO.File.Delete(Path.Combine(Config.EquipmentMaintenanceFileFolderPath, string.Format("{0}_{1}.{2}", form.UNIQUEID, file.SEQ, file.EXTENSION)));
                                }
                                catch { }
                            }
                        }

                        db.MFORMFILE.RemoveRange(fileList);

                        db.SaveChanges();

                        foreach (var file in Model.FormViewModel.FileList)
                        {
                            db.MFORMFILE.Add(new MFORMFILE()
                            {
                                MFORMUNIQUEID = form.UNIQUEID,
                                SEQ = file.Seq,
                                EXTENSION = file.Extension,
                                FILENAME = file.FileName,
                                UPLOADTIME = file.UploadTime,
                                CONTENTLENGTH = file.Size
                            });

                            if (!file.IsSaved)
                            {
                                System.IO.File.Copy(Path.Combine(Config.TempFolder, file.TempFileName), Path.Combine(Config.EquipmentMaintenanceFileFolderPath, string.Format("{0}_{1}.{2}", form.UNIQUEID, file.Seq, file.Extension)), true);
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = (from f in db.MFORM
                                join j in db.MJOB
                                on f.MJOBUNIQUEID equals j.UNIQUEID
                                join e in db.EQUIPMENT
                                on f.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                join p in db.EQUIPMENTPART
                                on f.PARTUNIQUEID equals p.UNIQUEID into tmpPart
                                from p in tmpPart.DefaultIfEmpty()
                                where f.UNIQUEID == UniqueID
                                select new
                                {
                                    MaintenanceForm = f,
                                    Job = j,
                                    Equipment = e,
                                    PartDescription = p != null ? p.DESCRIPTION : ""
                                }).First();

                    result.ReturnData(new ExtendFormModel()
                    {
                        FormUniqueID = form.MaintenanceForm.UNIQUEID,
                        OBeginDateString = DateTimeHelper.DateTime2DateStringWithSeperator(form.MaintenanceForm.ESTBEGINDATE),
                        OEndDateString = DateTimeHelper.DateTime2DateStringWithSeperator(form.MaintenanceForm.ESTENDDATE),
                        EquipmentID = form.Equipment.ID,
                        EquipmentName = form.Equipment.NAME,
                        PartDescription = form.PartDescription,
                        Subject = form.Job.DESCRIPTION,
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = db.MFORM.First(x => x.UNIQUEID == Model.FormUniqueID);

                    var organizationUniqueID = db.MJOB.First(x => x.UNIQUEID == form.MJOBUNIQUEID).ORGANIZATIONUNIQUEID;

                    var nextVerifyOrganization = (from f in db.FLOW
                                                  join x in db.FLOWFORM
                                                  on f.UNIQUEID equals x.FLOWUNIQUEID
                                                  join v in db.FLOWVERIFYORGANIZATION
                                                  on f.UNIQUEID equals v.FLOWUNIQUEID
                                                  join o in db.ORGANIZATION
                                                  on v.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                                  where f.ORGANIZATIONUNIQUEID == organizationUniqueID && x.FORM == Define.EnumForm.MaintenanceForm.ToString()
                                                  select new
                                                  {
                                                      o.UNIQUEID,
                                                      o.DESCRIPTION,
                                                      o.MANAGERUSERID,
                                                      v.SEQ
                                                  }).OrderBy(x => x.SEQ).FirstOrDefault();

                    //有設定簽核流程
                    if (nextVerifyOrganization != null)
                    {
                        if (!string.IsNullOrEmpty(nextVerifyOrganization.MANAGERUSERID))
                        {
                            form.STATUS = "6";

                            var seq = 1;

                            var extends = db.MFORMEXTEND.Where(x => x.MFORMUNIQUEID == Model.FormUniqueID).ToList();

                            if (extends.Count > 0)
                            {
                                seq = extends.Max(x => x.SEQ) + 1;
                            }

                            db.MFORMEXTEND.Add(new MFORMEXTEND()
                            {
                                MFORMUNIQUEID = Model.FormUniqueID,
                                SEQ = seq,
                                OBEGINDATE = Model.OBeginDate,
                                OENDDATE = Model.OEndDate,
                                NBEGINDATE = Model.FormInput.NBeginDate.Value,
                                NENDDATE = Model.FormInput.NEndDate.Value,
                                REASON = Model.FormInput.Reason,
                                CREATETIME = DateTime.Now
                            });

                            db.MFORMEXTENDFLOW.Add(new MFORMEXTENDFLOW()
                            {
                                MFORMUNIQUEID = form.UNIQUEID,
                                EXTENDSEQ = seq,
                                CURRENTSEQ = 1,
                                ISCLOSED = "N"
                            });

                            db.MFORMEXTENDFLOWLOG.Add(new MFORMEXTENDFLOWLOG()
                            {
                                MFORMUNIQUEID = form.UNIQUEID,
                                EXTENDSEQ = seq,
                                SEQ = 1,
                                FLOWSEQ = nextVerifyOrganization.SEQ,
                                USERID = nextVerifyOrganization.MANAGERUSERID,
                                NOTIFYTIME = DateTime.Now
                            });

                            db.SaveChanges();

                            result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Submit, Resources.Resource.Complete));
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1} {2} {3}", Resources.Resource.Organization, nextVerifyOrganization.DESCRIPTION, Resources.Resource.NotSet, Resources.Resource.Manager));
                        }
                    }
                    else
                    {
                        var organization = db.ORGANIZATION.First(x => x.UNIQUEID == organizationUniqueID);

                        if (!string.IsNullOrEmpty(organization.MANAGERUSERID))
                        {
                            form.STATUS = "6";

                            var seq = 1;

                            var extends = db.MFORMEXTEND.Where(x => x.MFORMUNIQUEID == Model.FormUniqueID).ToList();

                            if (extends.Count > 0)
                            {
                                seq = extends.Max(x => x.SEQ) + 1;
                            }

                            db.MFORMEXTEND.Add(new MFORMEXTEND()
                            {
                                MFORMUNIQUEID = Model.FormUniqueID,
                                SEQ = seq,
                                OBEGINDATE = Model.OBeginDate,
                                OENDDATE = Model.OEndDate,
                                NBEGINDATE = Model.FormInput.NBeginDate.Value,
                                NENDDATE = Model.FormInput.NEndDate.Value,
                                REASON = Model.FormInput.Reason,
                                CREATETIME = DateTime.Now
                            });

                            db.MFORMEXTENDFLOW.Add(new MFORMEXTENDFLOW()
                            {
                                MFORMUNIQUEID = form.UNIQUEID,
                                EXTENDSEQ = seq,
                                CURRENTSEQ = 1,
                                ISCLOSED = "N"
                            });

                            db.MFORMEXTENDFLOWLOG.Add(new MFORMEXTENDFLOWLOG()
                            {
                                MFORMUNIQUEID = form.UNIQUEID,
                                EXTENDSEQ = seq,
                                SEQ = 1,
                                FLOWSEQ = 0,
                                USERID = organization.MANAGERUSERID,
                                NOTIFYTIME = DateTime.Now
                            });

                            db.SaveChanges();

                            result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Submit, Resources.Resource.Complete));
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1} {2} {3}", Resources.Resource.Organization, organization.DESCRIPTION, Resources.Resource.NotSet, Resources.Resource.Manager));
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = db.MFORM.First(x => x.UNIQUEID == UniqueID);

                    var organizationUniqueID = db.MJOB.First(x => x.UNIQUEID == form.MJOBUNIQUEID).ORGANIZATIONUNIQUEID;

                    var flow = db.MFORMEXTENDFLOW.First(x => x.MFORMUNIQUEID == form.UNIQUEID && x.ISCLOSED == "N");

                    flow.ISCLOSED = "Y";

                    var extend = db.MFORMEXTEND.First(x => x.MFORMUNIQUEID == form.UNIQUEID && x.SEQ == flow.EXTENDSEQ);

                    var currentFlowLog = db.MFORMEXTENDFLOWLOG.Where(x => x.MFORMUNIQUEID == UniqueID && x.EXTENDSEQ == flow.EXTENDSEQ).OrderByDescending(x => x.SEQ).First();

                    form.STATUS = "1";
                    form.ESTBEGINDATE = extend.NBEGINDATE;
                    form.ESTENDDATE = extend.NENDDATE;

                    currentFlowLog.ISREJECT = "N";
                    currentFlowLog.VERIFYTIME = DateTime.Now;
                    currentFlowLog.REMARK = Remark;

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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = db.MFORM.First(x => x.UNIQUEID == UniqueID);

                    form.STATUS = "1";

                    var flow = db.MFORMEXTENDFLOW.First(x => x.MFORMUNIQUEID == form.UNIQUEID && x.ISCLOSED == "N");

                    flow.ISCLOSED = "Y";

                    var currentFlowLog = db.MFORMEXTENDFLOWLOG.Where(x => x.MFORMUNIQUEID == UniqueID && x.EXTENDSEQ == flow.EXTENDSEQ).OrderByDescending(x => x.SEQ).First();

                    currentFlowLog.ISREJECT = "Y";
                    currentFlowLog.VERIFYTIME = DateTime.Now;
                    currentFlowLog.REMARK = Remark;

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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var time = DateTime.Now;

                    foreach (var uniqueID in SelectedList)
                    {
                        var maintenanceForm = db.MFORM.First(x => x.UNIQUEID == uniqueID);

                        maintenanceForm.STATUS = "1";
                        maintenanceForm.TAKEJOBUSERID = Account.ID;
                        maintenanceForm.TAKEJOBTIME = time;
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

        public static RequestResult Confirm(List<string> VerifyResultList, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    foreach (var verifyResult in VerifyResultList)
                    {
                        string[] temp = verifyResult.Split(Define.Seperators, StringSplitOptions.None);

                        string uniqueID = temp[0];
                        string action = temp[1];
                        string remark = temp[2];

                        var form = db.MFORM.First(x => x.UNIQUEID == uniqueID);

                        if (action == "1")
                        {
                            var organizationUniqueID = db.MJOB.First(x => x.UNIQUEID == form.MJOBUNIQUEID).ORGANIZATIONUNIQUEID;

                            var flow = db.MFORMFLOW.First(x => x.MFORMUNIQUEID == uniqueID);
                            var currentFlowLog = db.MFORMFLOWLOG.Where(x => x.MFORMUNIQUEID == uniqueID).OrderByDescending(x => x.SEQ).First();

                            if (currentFlowLog.FLOWSEQ == 0)
                            {
                                form.STATUS = "5";

                                flow.ISCLOSED = "Y";

                                currentFlowLog.ISREJECT = "N";
                                currentFlowLog.VERIFYTIME = DateTime.Now;
                                currentFlowLog.REMARK = remark;

                                db.SaveChanges();
                            }
                            else
                            {
                                var nextVerifyOrganization = (from f in db.FLOW
                                                              join x in db.FLOWFORM
                                                              on f.UNIQUEID equals x.FLOWUNIQUEID
                                                              join v in db.FLOWVERIFYORGANIZATION
                                                              on f.UNIQUEID equals v.FLOWUNIQUEID
                                                              join o in db.ORGANIZATION
                                                              on v.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                                              where f.ORGANIZATIONUNIQUEID == organizationUniqueID && x.FORM == Define.EnumForm.MaintenanceForm.ToString()
                                                              select new
                                                              {
                                                                  o.UNIQUEID,
                                                                  o.DESCRIPTION,
                                                                  o.MANAGERUSERID,
                                                                  v.SEQ
                                                              }).Where(x => x.SEQ > currentFlowLog.FLOWSEQ).OrderBy(x => x.SEQ).FirstOrDefault();

                                if (nextVerifyOrganization != null)
                                {
                                    if (!string.IsNullOrEmpty(nextVerifyOrganization.MANAGERUSERID))
                                    {
                                        flow.CURRENTSEQ = flow.CURRENTSEQ + 1;

                                        currentFlowLog.ISREJECT = "N";
                                        currentFlowLog.VERIFYTIME = DateTime.Now;
                                        currentFlowLog.REMARK = remark;

                                        db.MFORMFLOWLOG.Add(new MFORMFLOWLOG()
                                        {
                                            MFORMUNIQUEID = uniqueID,
                                            SEQ = flow.CURRENTSEQ,
                                            FLOWSEQ = nextVerifyOrganization.SEQ,
                                            USERID = nextVerifyOrganization.MANAGERUSERID,
                                            NOTIFYTIME = DateTime.Now
                                        });

                                        db.SaveChanges();

                                        SendVerifyMail(uniqueID, nextVerifyOrganization.MANAGERUSERID);
                                    }
                                    else
                                    {
                                        
                                    }
                                }
                                else
                                {
                                    form.STATUS = "5";

                                    flow.ISCLOSED = "Y";

                                    currentFlowLog.ISREJECT = "N";
                                    currentFlowLog.VERIFYTIME = DateTime.Now;
                                    currentFlowLog.REMARK = remark;

                                    db.SaveChanges();
                                }
                            }
                        }

                        if (action == "2")
                        {
                            form.STATUS = "4";

                            var currentFlowLog = db.MFORMFLOWLOG.Where(x => x.MFORMUNIQUEID == uniqueID).OrderByDescending(x => x.SEQ).First();

                            currentFlowLog.ISREJECT = "Y";
                            currentFlowLog.VERIFYTIME = DateTime.Now;
                            currentFlowLog.REMARK = remark;

                            db.SaveChanges();
                        }
                    }

                    result.ReturnSuccessMessage("確認完成");
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

        public static RequestResult Approve(string UniqueID, string Remark, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = db.MFORM.First(x => x.UNIQUEID == UniqueID);

                    var organizationUniqueID = db.MJOB.First(x => x.UNIQUEID == form.MJOBUNIQUEID).ORGANIZATIONUNIQUEID;

                    var flow = db.MFORMFLOW.First(x => x.MFORMUNIQUEID == UniqueID);
                    var currentFlowLog = db.MFORMFLOWLOG.Where(x => x.MFORMUNIQUEID == UniqueID).OrderByDescending(x => x.SEQ).First();

                    if (currentFlowLog.FLOWSEQ == 0)
                    {
                        form.STATUS = "5";

                        flow.ISCLOSED = "Y";

                        currentFlowLog.ISREJECT = "N";
                        currentFlowLog.VERIFYTIME = DateTime.Now;
                        currentFlowLog.REMARK = Remark;

                        db.SaveChanges();

                        result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Approve, Resources.Resource.Complete));
                    }
                    else
                    {
                        var nextVerifyOrganization = (from f in db.FLOW
                                                      join x in db.FLOWFORM
                                                      on f.UNIQUEID equals x.FLOWUNIQUEID
                                                      join v in db.FLOWVERIFYORGANIZATION
                                                      on f.UNIQUEID equals v.FLOWUNIQUEID
                                                      join o in db.ORGANIZATION
                                                      on v.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                                      where f.ORGANIZATIONUNIQUEID == organizationUniqueID && x.FORM == Define.EnumForm.MaintenanceForm.ToString()
                                                      select new
                                                      {
                                                          o.UNIQUEID,
                                                          o.DESCRIPTION,
                                                          o.MANAGERUSERID,
                                                          v.SEQ
                                                      }).Where(x => x.SEQ > currentFlowLog.FLOWSEQ).OrderBy(x => x.SEQ).FirstOrDefault();

                        if (nextVerifyOrganization != null)
                        {
                            if (!string.IsNullOrEmpty(nextVerifyOrganization.MANAGERUSERID))
                            {
                                flow.CURRENTSEQ = flow.CURRENTSEQ + 1;

                                currentFlowLog.ISREJECT = "N";
                                currentFlowLog.VERIFYTIME = DateTime.Now;
                                currentFlowLog.REMARK = Remark;

                                db.MFORMFLOWLOG.Add(new MFORMFLOWLOG()
                                {
                                    MFORMUNIQUEID = UniqueID,
                                    SEQ = flow.CURRENTSEQ,
                                    FLOWSEQ = nextVerifyOrganization.SEQ,
                                    USERID = nextVerifyOrganization.MANAGERUSERID,
                                    NOTIFYTIME = DateTime.Now
                                });

                                db.SaveChanges();

                                SendVerifyMail(UniqueID, nextVerifyOrganization.MANAGERUSERID);

                                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Approve, Resources.Resource.Complete));
                            }
                            else
                            {
                                result.ReturnFailedMessage(string.Format("{0} {1} {2} {3}", Resources.Resource.Organization, nextVerifyOrganization.DESCRIPTION, Resources.Resource.NotSet, Resources.Resource.Manager));
                            }
                        }
                        else
                        {
                            form.STATUS = "5";

                            flow.ISCLOSED = "Y";

                            currentFlowLog.ISREJECT = "N";
                            currentFlowLog.VERIFYTIME = DateTime.Now;
                            currentFlowLog.REMARK = Remark;

                            db.SaveChanges();

                            result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Approve, Resources.Resource.Complete));
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = db.MFORM.First(x => x.UNIQUEID == UniqueID);

                    form.STATUS = "4";

                    var currentFlowLog = db.MFORMFLOWLOG.Where(x => x.MFORMUNIQUEID == UniqueID).OrderByDescending(x => x.SEQ).First();

                    currentFlowLog.ISREJECT = "Y";
                    currentFlowLog.VERIFYTIME = DateTime.Now;
                    currentFlowLog.REMARK = Remark;

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
                        var feelOption = standard.OptionList.First(x=>x.UniqueID==feelOptionUniqueID);

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
                var query = AccountList.Select(x => new Models.ASE.Shared.ASEUserModel
                {
                    ID = x.ID,
                    Name = x.Name,
                    Email = x.Email,
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

                        query = query.Where(x => x.Term.Contains(term)).ToList();
                    }
                }

                result.ReturnData(query.Select(x => new SelectListItem { Value = x.ID, Text = x.Display }).Distinct().ToList());
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var file = db.MFORMFILE.First(x => x.MFORMUNIQUEID == MFormUniqueID && x.SEQ == Seq);

                    model = new FileDownloadModel()
                    {
                        FormUniqueID = file.MFORMUNIQUEID,
                        Seq = file.SEQ,
                        FileName = file.FILENAME,
                        Extension = file.EXTENSION
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = db.MFORM.First(x => x.UNIQUEID == UniqueID);

                    var organizationUniqueID = db.MJOB.First(x => x.UNIQUEID == form.MJOBUNIQUEID).ORGANIZATIONUNIQUEID;

                    var nextVerifyOrganization = (from f in db.FLOW
                                                  join x in db.FLOWFORM
                                                  on f.UNIQUEID equals x.FLOWUNIQUEID
                                                  join v in db.FLOWVERIFYORGANIZATION
                                                  on f.UNIQUEID equals v.FLOWUNIQUEID
                                                  join o in db.ORGANIZATION
                                                  on v.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                                  where f.ORGANIZATIONUNIQUEID == organizationUniqueID && x.FORM == Define.EnumForm.MaintenanceForm.ToString()
                                                  select new
                                                  {
                                                      o.UNIQUEID,
                                                      o.DESCRIPTION,
                                                      o.MANAGERUSERID,
                                                      v.SEQ
                                                  }).OrderBy(x => x.SEQ).FirstOrDefault();

                    //有設定簽核流程
                    if (nextVerifyOrganization != null)
                    {
                        if (!string.IsNullOrEmpty(nextVerifyOrganization.MANAGERUSERID))
                        {
                            form.STATUS = "3";

                            var flow = db.MFORMFLOW.FirstOrDefault(x => x.MFORMUNIQUEID == UniqueID);

                            int currentSeq = 1;

                            if (flow == null)
                            {
                                db.MFORMFLOW.Add(new MFORMFLOW()
                                {
                                    MFORMUNIQUEID = UniqueID,
                                    CURRENTSEQ = currentSeq,
                                    ISCLOSED = "N"
                                });
                            }
                            else
                            {
                                currentSeq = flow.CURRENTSEQ + 1;

                                flow.CURRENTSEQ = currentSeq;
                            }

                            db.MFORMFLOWLOG.Add(new MFORMFLOWLOG()
                            {
                                MFORMUNIQUEID = UniqueID,
                                SEQ = currentSeq,
                                FLOWSEQ = nextVerifyOrganization.SEQ,
                                USERID = nextVerifyOrganization.MANAGERUSERID,
                                NOTIFYTIME = DateTime.Now
                            });

                            db.SaveChanges();

                            SendVerifyMail(UniqueID, nextVerifyOrganization.MANAGERUSERID);

                            result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Submit, Resources.Resource.Complete));
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1} {2} {3}", Resources.Resource.Organization, nextVerifyOrganization.DESCRIPTION, Resources.Resource.NotSet, Resources.Resource.Manager));
                        }
                    }
                    else
                    {
                        var organization = db.ORGANIZATION.First(x => x.UNIQUEID == organizationUniqueID);

                        if (!string.IsNullOrEmpty(organization.MANAGERUSERID))
                        {
                            form.STATUS = "3";

                            var flow = db.MFORMFLOW.FirstOrDefault(x => x.MFORMUNIQUEID == UniqueID);

                            int currentSeq = 1;

                            if (flow == null)
                            {
                                db.MFORMFLOW.Add(new MFORMFLOW()
                                {
                                    MFORMUNIQUEID = UniqueID,
                                    CURRENTSEQ = currentSeq,
                                    ISCLOSED = "N"
                                });
                            }
                            else
                            {
                                currentSeq = flow.CURRENTSEQ + 1;

                                flow.CURRENTSEQ = currentSeq;
                            }

                            db.MFORMFLOWLOG.Add(new MFORMFLOWLOG()
                            {
                                MFORMUNIQUEID = UniqueID,
                                SEQ = currentSeq,
                                FLOWSEQ = 0,
                                USERID = organization.MANAGERUSERID,
                                NOTIFYTIME = DateTime.Now
                            });

                            db.SaveChanges();

                            SendVerifyMail(UniqueID, organization.MANAGERUSERID);

                            result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Submit, Resources.Resource.Complete));
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1} {2} {3}", Resources.Resource.Organization, organization.DESCRIPTION, Resources.Resource.NotSet, Resources.Resource.Manager));
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var user = db.ACCOUNT.FirstOrDefault(x => x.ID == UserID);

                    if (user != null && !string.IsNullOrEmpty(user.EMAIL))
                    {
                        recipientList.Add(new MailAddress(user.EMAIL, user.NAME));
                    }

                    if (recipientList.Count > 0)
                    {
                        var mform = (from f in db.MFORM
                                     join j in db.MJOB
                                     on f.MJOBUNIQUEID equals j.UNIQUEID
                                     where f.UNIQUEID == UniqueID
                                     select new {
                                     f.VHNO,
                                     j.DESCRIPTION,
                                     f.CYCLEBEGINDATE,
                                     f.CYCLEENDDATE,
                                     j.ORGANIZATIONUNIQUEID
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
                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "連結"));
                        sb.Append(string.Format(td, string.Format("<a href=\"http://ASECL-eINSPRD01/FEM.Portal/Home/Index?ReturnUrl=http://ASECL-eINSPRD01/FEM/zh-tw/EquipmentMaintenance/MaintenanceForm/Index?VHNO={0}\">連結</a>", mform.VHNO)));
                        sb.Append("</tr>");

                        sb.Append("</table>");

                        MailHelper.SendMail(recipientList, subject, sb.ToString());
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
