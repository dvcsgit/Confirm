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
using Report.EquipmentMaintenance.Models.CheckResultHour;

namespace Report.EquipmentMaintenance.DataAccess
{
    public class CheckResultHoursDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = new List<GridItem>();

                var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                using (EDbEntities db = new EDbEntities())
                {
                    var query = db.ArriveRecord.AsNoTracking().Where(x => downStreamOrganizationList.Contains(x.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID) && string.Compare(x.ArriveDate, Parameters.BeginDate) >= 0 && string.Compare(x.ArriveDate, Parameters.EndDate) <= 0).ToList();

                    var routeList = query.Select(x => new
                    {
                        x.JobUniqueID,
                        x.RouteID,
                        x.RouteName,
                        x.JobDescription,
                        x.UserID,
                        x.UserName
                    }).Distinct().ToList();

                    foreach (var route in routeList)
                    {
                        var item = new GridItem()
                        {
                            RouteID = route.RouteID,
                            RouteName = route.RouteName,
                            JobDescription = route.JobDescription,
                            UserID = route.UserID,
                            UserName = route.UserName
                        };

                        var controlPointList = query.Where(x => x.JobUniqueID == route.JobUniqueID && x.UserID == route.UserID).Select(x => new
                        {
                            x.ControlPointUniqueID,
                            x.ControlPointID,
                            x.ControlPointDescription
                        }).Distinct().ToList();

                        foreach (var controlPoint in controlPointList)
                        {
                            var controlPointModel = new ControlPointModel()
                            {
                                ControlPointID = controlPoint.ControlPointID,
                                ControlPointDescription = controlPoint.ControlPointDescription
                            };

                            var arriveRecordList = query.Where(x => x.JobUniqueID == route.JobUniqueID && x.UserID == route.UserID && x.ControlPointUniqueID == controlPoint.ControlPointUniqueID).Select(x => new
                            {
                                x.UniqueID,
                                x.ArriveDate,
                                x.ArriveTime
                            }).Distinct().ToList();

                            foreach (var arriveRecord in arriveRecordList)
                            {
                                controlPointModel.ArriveRecordList.Add(new ArriveRecordModel()
                                {
                                    ArriveDate = arriveRecord.ArriveDate,
                                    ArriveTime = arriveRecord.ArriveTime,
                                    CheckResultList = db.CheckResult.Where(x => x.ArriveRecordUniqueID == arriveRecord.UniqueID).Select(x => new CheckResultModel
                                    {
                                        CheckDate = x.CheckDate,
                                        CheckTime = x.CheckTime
                                    }).ToList()
                                });
                            }
                            item.ControlPointList.Add(controlPointModel);
                        }
                        item.ControlPointList =item.ControlPointList.OrderBy(x => x.ArriveTime).OrderBy(x => x.BeginTime).ToList();
                        itemList.Add(item);
                    }
                  itemList= itemList.OrderBy(x => x.Route).ToList();
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

                ISheet sheet = wk.CreateSheet(Resources.Resource.CheckResultHour);

                sheet.DefaultColumnWidth = 18;     //设置单元格长度
                sheet.DefaultRowHeight = 400;      //设置单元格高度
                sheet.CreateRow(0).CreateCell(0).SetCellValue(Resources.Resource.CheckResultHour);
                sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 4));
                sheet.GetRow(0).CreateCell(5).SetCellValue(DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTime.Now));

                var rowIndex = 1;
                foreach (var item in ItemList)
                {
                    var row = sheet.CreateRow(rowIndex);

                    var cell = row.CreateCell(0);
                    cell.SetCellValue(Resources.Resource.Route);

                    cell = row.CreateCell(1);
                    cell.SetCellValue(item.Route);

                    cell = row.CreateCell(2);
                    cell.SetCellValue(Resources.Resource.CheckUser);

                    cell = row.CreateCell(3);
                    cell.SetCellValue(item.User);

                    cell = row.CreateCell(4);
                    cell.SetCellValue(Resources.Resource.TimeSpan);

                    cell = row.CreateCell(5);
                    cell.SetCellValue(item.TimeSpan);

                    rowIndex++;

                    row = sheet.CreateRow(rowIndex);

                    cell = row.CreateCell(0);
                    cell.SetCellValue(Resources.Resource.ControlPoint);

                    cell = row.CreateCell(1);
                    cell.SetCellValue(Resources.Resource.ArriveTime);

                    cell = row.CreateCell(2);
                    cell.SetCellValue(Resources.Resource.BeginTime);

                    cell = row.CreateCell(3);
                    cell.SetCellValue(Resources.Resource.EndTime);

                    cell = row.CreateCell(4);
                    cell.SetCellValue(Resources.Resource.TimeSpan);

                    rowIndex++;

                    foreach (var controlPoint in item.ControlPointList)
                    {
                        row = sheet.CreateRow(rowIndex);

                        cell = row.CreateCell(0);
                        cell.SetCellValue(controlPoint.ControlPoint);

                        cell = row.CreateCell(1);
                        cell.SetCellValue(controlPoint.ArriveTime);

                        cell = row.CreateCell(2);
                        cell.SetCellValue(controlPoint.BeginTime);

                        cell = row.CreateCell(3);
                        cell.SetCellValue(controlPoint.EndTime);

                        cell = row.CreateCell(4);
                        cell.SetCellValue(controlPoint.TimeSpan);

                        rowIndex++;
                    }
                }

                var model = new ExcelExportModel(Resources.Resource.CheckResultHour, ExcelVersion);

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
