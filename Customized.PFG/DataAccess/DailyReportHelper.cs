using DataAccess;
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.Authenticated;
using Models.Shared;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Utility;
using Utility.Models;
using Customized.PFG.Models.DailyReport;

namespace Customized.PFG.DataAccess
{
    public class DailyReportHelper
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new ReportModel()
                {
                    RouteUniqueID = Parameters.RouteUniqueID,
                    PrintTime = DateTime.Now,
                    CheckDate = Parameters.Date
                };

                using (EDbEntities db = new EDbEntities())
                {
                    var route = db.Route.First(x => x.UniqueID == Parameters.RouteUniqueID);

                    var checkResultList = db.CheckResult.Where(x => x.RouteUniqueID == route.UniqueID && x.CheckDate == Parameters.Date).ToList();

                    var jobList = db.Job.Where(x => x.RouteUniqueID == route.UniqueID).ToList();

                    model.OrganizationUniqueID = route.OrganizationUniqueID;
                    model.OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(route.OrganizationUniqueID);

                    var controlPointList = (from x in db.RouteControlPoint
                                            join c in db.ControlPoint
                                            on x.ControlPointUniqueID equals c.UniqueID
                                            where x.RouteUniqueID == route.UniqueID
                                            select new
                                            {
                                                c.UniqueID,
                                                c.Description
                                            }).ToList();

                    foreach (var controlPoint in controlPointList)
                    {
                        var controlPointModel = new ControlPointModel()
                        {
                            ControlPointDescription = controlPoint.Description
                        };

                        var controlPointCheckItemList = (from x in db.RouteControlPointCheckItem
                                                         join c in db.CheckItem
                                                         on x.CheckItemUniqueID equals c.UniqueID
                                                         where x.RouteUniqueID == route.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID
                                                         select new
                                                         {
                                                             c.UniqueID,
                                                             c.Description
                                                         }).ToList();

                        foreach (var checkItem in controlPointCheckItemList)
                        {
                            var checkItemModel = new CheckItemModel()
                            {
                                CheckItemDescription = checkItem.Description
                            };

                            foreach (var job in jobList)
                            {
                                checkItemModel.CheckResultList.Add(job.Description, checkResultList.Where(x => x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID && x.CheckDate == Parameters.Date).OrderByDescending(x => x.CheckTime).Select(x => new CheckResultModel
                                {
                                    OrganizationUniqueID = route.OrganizationUniqueID,
                                    CheckItemDescription = checkItem.Description,
                                    CheckTime = x.CheckTime,
                                    IsAbnormal = x.IsAbnormal,
                                    IsAlert = x.IsAlert,
                                    Result = x.Result
                                }).FirstOrDefault());
                            }

                            controlPointModel.CheckItemList.Add(checkItemModel);
                        }

                        model.ControlPointList.Add(controlPointModel);
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

        public static RequestResult Export(ReportModel Model, Define.EnumExcelVersion ExcelVersion)
        {
            RequestResult result = new RequestResult();

            try
            {
                var config = Customized.PFG.Utility.Config.DailyReportConfig.First(x => x.RouteUniqueID == Model.RouteUniqueID);
                
                IWorkbook workBook = null;

                if (ExcelVersion == Define.EnumExcelVersion._2003)
                {
                    using (FileStream fs = new FileStream(config._2003, FileMode.Open, FileAccess.ReadWrite))
                    {
                        workBook = new HSSFWorkbook(fs);

                        fs.Close();
                    }
                }

                if (ExcelVersion == Define.EnumExcelVersion._2007)
                {
                    using (FileStream fs = new FileStream(config._2007, FileMode.Open, FileAccess.ReadWrite))
                    {
                        workBook = new XSSFWorkbook(fs);

                        fs.Close();
                    }
                }

                IRow row;
                ICell cell;

                var abnormalFont = workBook.CreateFont();
                abnormalFont.Color = HSSFColor.Red.Index;
                abnormalFont.Boldweight = (short)FontBoldWeight.Bold;
                abnormalFont.FontHeightInPoints = 12;

                var abnormalStyle = workBook.CreateCellStyle();
                abnormalStyle.BorderTop = BorderStyle.Thin;
                abnormalStyle.BorderBottom = BorderStyle.Thin;
                abnormalStyle.BorderLeft = BorderStyle.Thin;
                abnormalStyle.BorderRight = BorderStyle.Thin;
                abnormalStyle.SetFont(abnormalFont);

                var alertFont = workBook.CreateFont();
                alertFont.Color = HSSFColor.Orange.Index;
                alertFont.Boldweight = (short)FontBoldWeight.Bold;
                alertFont.FontHeightInPoints = 12;

                var alertStyle = workBook.CreateCellStyle();
                alertStyle.BorderTop = BorderStyle.Thin;
                alertStyle.BorderBottom = BorderStyle.Thin;
                alertStyle.BorderLeft = BorderStyle.Thin;
                alertStyle.BorderRight = BorderStyle.Thin;
                alertStyle.SetFont(alertFont);

                var worksheet = workBook.GetSheetAt(0);

                row = worksheet.GetRow(1);
                cell = row.CreateCell(21);
                cell.SetCellValue(Model.CheckDateString);

                cell = row.CreateCell(24);
                cell.SetCellValue(Model.PrintDateTimeString);

                foreach (var define in config.ExcelDefineList)
                {
                    row = worksheet.GetRow(define.ControlPointRowIndex);

                    cell = row.GetCell(define.ControlPointCellIndex);

                    var controlPoint = Model.ControlPointList.FirstOrDefault(x => x.ControlPointDescription == cell.StringCellValue);

                    if (controlPoint != null)
                    {
                        var jobList = new Dictionary<int, string>();

                        var jobPrefixRow = worksheet.GetRow(define.JobPrefixRowIndex);

                        row = worksheet.GetRow(define.JobDetailRowIndex);

                        for (int cellIndex = define.JobDetailBeginCellIndex; cellIndex <= define.JobDetailEndCellIndex; cellIndex++)
                        {
                            var i = cellIndex;

                            var prefix = jobPrefixRow.GetCell(i);

                            while (string.IsNullOrEmpty(prefix.StringCellValue))
                            {
                                prefix = jobPrefixRow.GetCell(i - 1);
                                i--;
                            }

                            cell = row.GetCell(cellIndex);

                            jobList.Add(cellIndex, string.Format("{0}{1}", prefix.StringCellValue, cell.StringCellValue));
                        }

                        for (int rowIndex = define.CheckItemBeginRowIndex; rowIndex <= define.CheckItemEndRowIndex; rowIndex++)
                        {
                            row = worksheet.GetRow(rowIndex);

                            cell = row.GetCell(define.CheckItemCellIndex);

                            var checkItem = controlPoint.CheckItemList.FirstOrDefault(x => x.CheckItemDescription == cell.StringCellValue);

                            if (checkItem != null && checkItem.CheckResultList != null)
                            {
                                foreach (var job in jobList)
                                {
                                    if (checkItem.CheckResultList.Any(x => x.Key == job.Value))
                                    {
                                        var checkResult = checkItem.CheckResultList[job.Value];

                                        if (checkResult != null)
                                        {
                                            cell = row.GetCell(job.Key);

                                            if (checkResult.IsAbnormal)
                                            {
                                                cell.CellStyle = abnormalStyle;
                                            }
                                            else if (checkResult.IsAlert)
                                            {
                                                cell.CellStyle = alertStyle;
                                            }

                                            var r = string.Empty;

                                            var display = string.Empty;

                                            var decimalDefine = config.DecimalDefineList.FirstOrDefault(x => x.CheckItem == checkItem.CheckItemDescription);

                                            if (decimalDefine != null)
                                            {
                                                r = double.Parse(checkResult.Result).ToString(decimalDefine.Decimals);
                                            }
                                            else
                                            {
                                                r = double.Parse(checkResult.Result).ToString("F1");
                                            }

                                            if (checkResult.IsAbnormal)
                                            {
                                                display= string.Format("{0}({1})", r, Resources.Resource.Abnormal);
                                            }
                                            else if (checkResult.IsAlert)
                                            {
                                                display = string.Format("{0}({1})", r, Resources.Resource.Warning);
                                            }
                                            else
                                            {
                                                display = r;
                                            }

                                            cell.SetCellValue(display);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                var output = new ExcelExportModel(Model.FileName, ExcelVersion);

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

        public static RequestResult GetTreeItem(string OrganizationUniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var treeItemList = new List<TreeItem>();

                var attributes = new Dictionary<Define.EnumTreeAttribute, string>() 
                { 
                    { Define.EnumTreeAttribute.NodeType, string.Empty },
                    { Define.EnumTreeAttribute.ToolTip, string.Empty },
                    { Define.EnumTreeAttribute.OrganizationUniqueID, string.Empty },
                    { Define.EnumTreeAttribute.RouteUniqueID, string.Empty }
                };

                var query = Customized.PFG.Utility.Config.DailyReportConfig.Select(x => x.RouteUniqueID).ToList();

                using (EDbEntities edb = new EDbEntities())
                {
                    if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                    {
                        var routeList = edb.Route.Where(x => x.OrganizationUniqueID == OrganizationUniqueID && query.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                        foreach (var route in routeList)
                        {
                            var treeItem = new TreeItem() { Title = route.Name };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Route.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", route.ID, route.Name);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.RouteUniqueID] = route.UniqueID;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItemList.Add(treeItem);
                        }
                    }

                    using (DbEntities db = new DbEntities())
                    {
                        var organizationList = db.Organization.Where(x => x.ParentUniqueID == OrganizationUniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                        foreach (var organization in organizationList)
                        {
                            var downStream = OrganizationDataAccessor.GetDownStreamOrganizationList(organization.UniqueID, true);

                            if (edb.Route.Any(x => downStream.Contains(x.OrganizationUniqueID) && query.Contains(x.UniqueID)))
                            {
                                var treeItem = new TreeItem() { Title = organization.Description };

                                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                                attributes[Define.EnumTreeAttribute.RouteUniqueID] = string.Empty;

                                foreach (var attribute in attributes)
                                {
                                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                                }

                                treeItem.State = "closed";

                                treeItemList.Add(treeItem);
                            }
                        }
                    }
                }

                result.ReturnData(treeItemList);
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
