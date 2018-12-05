using Customized.PFG.CN.Models.AbnormalHanding;
using DataAccess;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.Authenticated;
using Models.Shared;
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
using Utility;
using Utility.Models;

namespace Customized.PFG.CN.DataAccess 
{
    public class AbnormalHandingHelper
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    //var route = db.Route.First(x => x.UniqueID == Parameters.RouteUniqueID);

                    int no = 1;

                    var model = new ReportModel()
                    {
                        RouteUniqueID = Parameters.RouteUniqueID,
                        OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(Parameters.OrganizationUniqueID),
                        Parameters = Parameters
                    };

                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    //DateTime dtmBeginDate = Convert.ToDateTime(Parameters.BeginDateString);
                    //DateTime dtmEndDate = Convert.ToDateTime(Parameters.EndDateString);
                    
                    
                    var checkResultList = (from checkResult in db.CheckResult
                                           join arriveRecord in db.ArriveRecord on checkResult.ArriveRecordUniqueID equals arriveRecord.UniqueID
                                           join checkResultAbnormalReason in db.CheckResultAbnormalReason on checkResult.UniqueID equals checkResultAbnormalReason.CheckResultUniqueID
                                           join checkResultHandingMethod in db.CheckResultHandlingMethod on checkResult.UniqueID equals checkResultHandingMethod.CheckResultUniqueID
                                           into ch from ch1 in ch.DefaultIfEmpty()
                                           

                                           where downStreamOrganizationList.Contains(checkResult.OrganizationUniqueID) &&
                                               checkResult.CheckDate.CompareTo(Parameters.BeginDate) >= 0 && checkResult.CheckDate.CompareTo(Parameters.EndDate) <= 0
                                           
                                           select new
                                           {
                                               checkResult.RouteUniqueID,
                                               checkResult.CheckDate,
                                               checkResult.CheckTime,
                                               checkResult.RouteID,
                                               checkResult.RouteName,
                                               checkResult.ControlPointDescription,
                                               checkResult.EquipmentID,
                                               checkResult.EquipmentName,
                                               checkResult.PartDescription,
                                               checkResult.CheckItemDescription,
                                               checkResult.Remark,
                                               checkResultAbnormalReason.AbnormalReasonID,
                                               checkResultAbnormalReason.AbnormalReasonDescription,
                                               ch1.HandlingMethodID,
                                               ch1.HandlingMethodRemark,
                                               ch1.HandlingMethodDescription,
                                               arriveRecord.UserID,
                                               arriveRecord.UserName
                                           }).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).ToList();

                    if (!String.IsNullOrEmpty(Parameters.RouteUniqueID))
                    {
                        var route = db.Route.First(x => x.UniqueID == Parameters.RouteUniqueID);
                        checkResultList = checkResultList.Where(x => x.RouteUniqueID == route.UniqueID).ToList();
                    }

                    foreach (var cr in checkResultList)
                    {
                        var item = new CheckItemModel()
                        {
                            No = no,
                            CheckDate = cr.CheckDate,
                            CheckTime = cr.CheckTime,
                            RouteID = cr.RouteID,
                            RouteName = cr.RouteName,
                            ControlPointDescription = cr.ControlPointDescription,
                            EquipmentID = cr.EquipmentID,
                            EquipmentName = cr.EquipmentName,
                            PartDescription = cr.PartDescription,
                            CheckItemDescription = cr.CheckItemDescription,
                            HandlingMethodID = cr.HandlingMethodID,
                            HandingMethodDescription = cr.HandlingMethodDescription,
                            HandingMethodRemark = cr.HandlingMethodRemark,
                            ID = cr.AbnormalReasonID,
                            Description = cr.AbnormalReasonDescription,
                            UserID = cr.UserID,
                            UserName = cr.UserName
                        };
                        model.ItemList.Add(item);
                        no++;
                    }
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
        public static RequestResult Export(ReportModel model, Define.EnumExcelVersion ExcelVersion)
        {
            RequestResult result = new RequestResult();
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

                ISheet sheet = wk.CreateSheet("預保巡車異常及處理匯總表");

                //sheet.DefaultColumnWidth = 9;
                //sheet.DefaultRowHeight = 400;
                //sheet.SetColumnWidth(1, 25 * 256);
                //sheet.SetColumnWidth(2, 25 * 256);
                //sheet.SetColumnWidth(3, 25 * 256);

                ICellStyle cellSingleStyle = wk.CreateCellStyle();

                IFont iFont = wk.CreateFont();
                iFont.FontName = "標楷體";
                iFont.FontHeightInPoints = 12;
                cellSingleStyle.BorderTop = BorderStyle.Thin;
                cellSingleStyle.BorderBottom = BorderStyle.Thin;
                cellSingleStyle.BorderLeft = BorderStyle.Thin;
                cellSingleStyle.BorderRight = BorderStyle.Thin;
                cellSingleStyle.SetFont(iFont);

                //标题的样式，显示边框，上下居中，字体加大
                ICellStyle titleCellStyle = wk.CreateCellStyle();
                IFont titleFont = wk.CreateFont();
                titleFont.FontName = "標楷體";
                titleFont.FontHeightInPoints = 22;
                titleFont.Underline = FontUnderlineType.Single;
                titleCellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                titleCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                titleCellStyle.BorderTop = BorderStyle.None;
                titleCellStyle.BorderBottom = BorderStyle.None;
                titleCellStyle.BorderLeft = BorderStyle.None;
                titleCellStyle.BorderRight = BorderStyle.None;
                titleCellStyle.SetFont(titleFont);

                ICellStyle cellStylte = wk.CreateCellStyle();
                IFont cellFont = wk.CreateFont();
                cellFont.FontName = "標楷體";
                cellFont.FontHeightInPoints = 12;
                cellStylte.BorderTop = BorderStyle.None;
                cellStylte.BorderBottom = BorderStyle.None;
                cellStylte.BorderLeft = BorderStyle.None;
                cellStylte.BorderRight = BorderStyle.None;
                cellStylte.SetFont(cellFont);

                sheet.CreateRow(0).CreateCell(0).SetCellValue("預保巡車異常及處理匯總表");
                sheet.GetRow(0).GetCell(0).CellStyle = titleCellStyle;
                sheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 11));

                IRow row2 = sheet.CreateRow(1);
                row2.CreateCell(0).SetCellValue(Resources.Resource.Department+":"+model.OrganizationDescription);
                row2.CreateCell(1);
                row2.CreateCell(2);
                row2.CreateCell(3);
                row2.CreateCell(4);
                row2.CreateCell(5);
                row2.CreateCell(6);
                row2.CreateCell(7);
                row2.CreateCell(8).SetCellValue("時間區間:" + model.Parameters.BeginDateString + "~" + model.Parameters.EndDateString);
                row2.CreateCell(9);
                row2.CreateCell(10);
                row2.CreateCell(11);
                

                row2.GetCell(0).CellStyle = cellStylte;
                row2.GetCell(8).CellStyle = cellStylte;

                IRow row = sheet.CreateRow(2);
                row.CreateCell(0).SetCellValue(Resources.Resource.Item);
                row.CreateCell(1).SetCellValue(Resources.Resource.CheckDate);
                row.CreateCell(2).SetCellValue(Resources.Resource.CheckTime);
                row.CreateCell(3).SetCellValue(Resources.Resource.Route);
                row.CreateCell(4).SetCellValue(Resources.Resource.ControlPoint);
                row.CreateCell(5).SetCellValue(Resources.Resource.Equipment);
                row.CreateCell(6).SetCellValue(Resources.Resource.PartName);
                row.CreateCell(7).SetCellValue(Resources.Resource.CheckItem);
                row.CreateCell(8).SetCellValue(Resources.Resource.AbnormalReason);
                row.CreateCell(9).SetCellValue(Resources.Resource.HandlingMethod);
                row.CreateCell(10).SetCellValue(Resources.Resource.CheckUser);
                row.CreateCell(11).SetCellValue(Resources.Resource.Remark);

                row.GetCell(0).CellStyle = cellSingleStyle;
                row.GetCell(1).CellStyle = cellSingleStyle;
                row.GetCell(2).CellStyle = cellSingleStyle;
                row.GetCell(3).CellStyle = cellSingleStyle;
                row.GetCell(4).CellStyle = cellSingleStyle;
                row.GetCell(5).CellStyle = cellSingleStyle;
                row.GetCell(6).CellStyle = cellSingleStyle;
                row.GetCell(7).CellStyle = cellSingleStyle;
                row.GetCell(8).CellStyle = cellSingleStyle;
                row.GetCell(9).CellStyle = cellSingleStyle;
                row.GetCell(10).CellStyle = cellSingleStyle;
                row.GetCell(11).CellStyle = cellSingleStyle;

                var rowIndex = 3;
                foreach (var item in model.ItemList)
                {
                    row = sheet.CreateRow(rowIndex);
                    row.CreateCell(0).SetCellValue(item.No);
                    row.CreateCell(1).SetCellValue(item.CheckDate);
                    row.CreateCell(2).SetCellValue(item.CheckTime);
                    row.CreateCell(3).SetCellValue(item.Route);
                    row.CreateCell(4).SetCellValue(item.ControlPointDescription);
                    row.CreateCell(5).SetCellValue(item.EquipmentDescription);
                    row.CreateCell(6).SetCellValue(item.PartDescription);
                    row.CreateCell(7).SetCellValue(item.CheckItemDescription);
                    row.CreateCell(8).SetCellValue(item.AbnormalReasonDescription);
                    row.CreateCell(9).SetCellValue(item.HandingMethod);
                    row.CreateCell(10).SetCellValue(item.ArriveRecord);
                    row.CreateCell(11).SetCellValue(item.Remark);

                    row.GetCell(0).CellStyle = cellSingleStyle;
                    row.GetCell(1).CellStyle = cellSingleStyle;
                    row.GetCell(2).CellStyle = cellSingleStyle;
                    row.GetCell(3).CellStyle = cellSingleStyle;
                    row.GetCell(4).CellStyle = cellSingleStyle;
                    row.GetCell(5).CellStyle = cellSingleStyle;
                    row.GetCell(6).CellStyle = cellSingleStyle;
                    row.GetCell(7).CellStyle = cellSingleStyle;
                    row.GetCell(8).CellStyle = cellSingleStyle;
                    row.GetCell(9).CellStyle = cellSingleStyle;
                    row.GetCell(10).CellStyle = cellSingleStyle;
                    row.GetCell(11).CellStyle = cellSingleStyle;

                    rowIndex++;
                }
                IRow row3 = sheet.CreateRow(rowIndex+1);
                row3.CreateCell(0);
                row3.CreateCell(1);
                row3.CreateCell(2);
                row3.CreateCell(3);
                row3.CreateCell(4);
                row3.CreateCell(5);
                row3.CreateCell(6);
                row3.CreateCell(7);
                row3.CreateCell(8).SetCellValue("主管:");
                row3.CreateCell(9);
                row3.CreateCell(10).SetCellValue("經辦:");
                row3.CreateCell(11);

                row3.GetCell(7).CellStyle = cellStylte;
                row3.GetCell(8).CellStyle = cellStylte;
                row3.GetCell(10).CellStyle = cellStylte;

                var output = new ExcelExportModel("預防巡檢異常匯總表"+" "+DateTime.Now.ToString("yyyy-MM-dd"), ExcelVersion);

                using (FileStream fs = System.IO.File.OpenWrite(output.FullFileName))
                {
                    wk.Write(fs);
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
                //return output;
                result.ReturnData(output);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);
                //return null;
                result.ReturnError(err);
            }
            return result;
        }
        public static RequestResult GetTreeItem(List<Organization> OrganizationList, string OrganizationUniqueID, Account Account)
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

                using (EDbEntities edb = new EDbEntities())
                {
                    if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                    {
                        var routeList = edb.Route.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

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

                    var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID)).OrderBy(x => x.ID).ToList();

                    foreach (var organization in organizationList)
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

                        if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                            ||
                            (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && edb.Route.Any(x => x.OrganizationUniqueID == organization.UniqueID)))
                        {
                            treeItem.State = "closed";
                        }

                        treeItemList.Add(treeItem);
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
        public static RequestResult GetRootTreeItem(List<Organization> OrganizationList, string OrganizationUniqueID, Account Account)
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

                using (EDbEntities edb = new EDbEntities())
                {
                    var organization = OrganizationList.First(x => x.UniqueID == OrganizationUniqueID);

                    var treeItem = new TreeItem() { Title = organization.Description };

                    attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                    attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                    attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                    attributes[Define.EnumTreeAttribute.RouteUniqueID] = string.Empty;

                    foreach (var attribute in attributes)
                    {
                        treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                    }

                    if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                        ||
                        (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && edb.Route.Any(x => x.OrganizationUniqueID == organization.UniqueID)))
                    {
                        treeItem.State = "closed";
                    }

                    treeItemList.Add(treeItem);
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
