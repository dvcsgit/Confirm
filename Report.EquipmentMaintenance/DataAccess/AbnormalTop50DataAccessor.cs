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
using DataAccess;
#if ORACLE
using DbEntity.ORACLE.EquipmentMaintenance;
#else
using DbEntity.MSSQL.EquipmentMaintenance;
#endif
using Models.Authenticated;
using Report.EquipmentMaintenance.Models.AbnormalTop50;

namespace Report.EquipmentMaintenance.DataAccess
{
    public class AbnormalTop50DataAccessor
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
                    var query = (from x in db.CheckResult
                                 where downSteamOrganizationList.Contains(x.OrganizationUniqueID) &&
                                       Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID) &&
#if ORACLE
                                       x.IsAbnormal==1
#else
                                       x.IsAbnormal
#endif
                                 select x).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.BeginDate))
                    {
                        query = query.Where(x => string.Compare(x.CheckDate, Parameters.BeginDate) >= 0);
                    }

                    if (!string.IsNullOrEmpty(Parameters.EndDate))
                    {
                        query = query.Where(x => string.Compare(x.CheckDate, Parameters.EndDate) <= 0);
                    }

                    model.ItemList = (from x in query
                                      group x by new
                                      {
                                          x.JobUniqueID,
                                          x.JobDescription,
                                          x.RouteUniqueID,
                                          x.RouteID,
                                          x.RouteName,
                                          x.ControlPointUniqueID,
                                          x.ControlPointID,
                                          x.ControlPointDescription,
                                          x.EquipmentUniqueID,
                                          x.EquipmentID,
                                          x.EquipmentName,
                                          x.PartUniqueID,
                                          x.PartDescription,
                                          x.CheckItemUniqueID,
                                          x.CheckItemID,
                                          x.CheckItemDescription
                                      } into tmp
                                      select new
                                      {
                                          tmp.Key.JobUniqueID,
                                          tmp.Key.JobDescription,
                                          tmp.Key.RouteUniqueID,
                                          tmp.Key.RouteID,
                                          tmp.Key.RouteName,
                                          tmp.Key.ControlPointUniqueID,
                                          tmp.Key.ControlPointID,
                                          tmp.Key.ControlPointDescription,
                                          tmp.Key.EquipmentUniqueID,
                                          tmp.Key.EquipmentID,
                                          tmp.Key.EquipmentName,
                                          tmp.Key.PartUniqueID,
                                          tmp.Key.PartDescription,
                                          tmp.Key.CheckItemUniqueID,
                                          tmp.Key.CheckItemID,
                                          tmp.Key.CheckItemDescription,
                                          Count = tmp.Count()
                                      }).OrderByDescending(x => x.Count).Take(50).Select(x => new GridItem
                                      {
                                          JobDescription = x.JobDescription,
                                          RouteID = x.RouteID,
                                          RouteName = x.RouteName,
                                          ControlPointID = x.ControlPointID,
                                          ControlPointDescription = x.ControlPointDescription,
                                          EquipmentID = x.EquipmentID,
                                          EquipmentName = x.EquipmentName,
                                          PartDescription = x.PartDescription,
                                          CheckItemID = x.CheckItemID,
                                          CheckItemDescription = x.CheckItemDescription,
                                          AbnormalCount = x.Count
                                      }).ToList();
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

                ISheet sheet = wk.CreateSheet(Resources.Resource.AbnormalTop50);

                sheet.DefaultColumnWidth = 18;     //设置单元格长度
                sheet.DefaultRowHeight = 400;      //设置单元格高度
                sheet.CreateRow(0).CreateCell(0).SetCellValue(Resources.Resource.AbnormalTop50);
                sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 4));
                sheet.GetRow(0).CreateCell(5).SetCellValue(DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTime.Now));

                IRow row2 = sheet.CreateRow(1);
                row2.CreateCell(0).SetCellValue(Resources.Resource.Organization);
                row2.CreateCell(2).SetCellValue(Resources.Resource.BeginDate);
                row2.CreateCell(4).SetCellValue(Resources.Resource.EndDate);

                //綁定查詢條件
                row2.CreateCell(1).SetCellValue(OrganizationDataAccessor.GetOrganizationDescription(Model.Parameters.OrganizationUniqueID));
                row2.CreateCell(3).SetCellValue(Model.Parameters.BeginDateString);
                row2.CreateCell(5).SetCellValue(Model.Parameters.EndDateString);

                IRow row3 = sheet.CreateRow(2);
                row3.CreateCell(0).SetCellValue(Resources.Resource.Route);
                row3.CreateCell(1).SetCellValue(Resources.Resource.ControlPoint);
                row3.CreateCell(2).SetCellValue(Resources.Resource.Equipment);
                row3.CreateCell(3).SetCellValue(Resources.Resource.CheckItem);
                row3.CreateCell(4).SetCellValue(Resources.Resource.AbnormalCount);

                //循環綁定異常基準數據
                for (var i = 0; i < Model.ItemList.Count; i++)
                {
                    sheet.CreateRow(3 + i).CreateCell(0).SetCellValue(Model.ItemList[i].Route);
                    sheet.GetRow(3 + i).CreateCell(1).SetCellValue(Model.ItemList[i].ControlPoint);
                    sheet.GetRow(3 + i).CreateCell(2).SetCellValue(Model.ItemList[i].Equipment);
                    sheet.GetRow(3 + i).CreateCell(3).SetCellValue(Model.ItemList[i].CheckItem);
                    sheet.GetRow(3 + i).CreateCell(4).SetCellValue(Model.ItemList[i].AbnormalCount);
                }

                var model = new ExcelExportModel(Resources.Resource.AbnormalTop50, ExcelVersion);

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
