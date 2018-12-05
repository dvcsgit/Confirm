using Customized.PFG.Models.EReport;
using DataAccess;
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.Authenticated;
using Models.Shared;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Utility;
using Utility.Models;

namespace Customized.PFG.DataAccess
{
    public class EReportHelper
    {
        public static RequestResult Query(QueryParameters Parameters)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new ReportModel()
                {
                    RouteUniqueID = Parameters.RouteUniqueID,
                    CheckDate = Parameters.Date
                };

                using (EDbEntities db = new EDbEntities())
                {
                    var route = db.Route.First(x => x.UniqueID == Parameters.RouteUniqueID);

                    var arriveRecrod = db.ArriveRecord.Where(x => x.RouteUniqueID == route.UniqueID && x.ArriveDate == Parameters.Date).OrderBy(x => x.ArriveTime).FirstOrDefault();

                    if (arriveRecrod != null)
                    {
                        if (!string.IsNullOrEmpty(arriveRecrod.UserName))
                        {
                            model.CheckUser = arriveRecrod.UserName;
                        }
                        else
                        {
                            model.CheckUser = arriveRecrod.UserID;
                        }
                    }

                    var checkResultList = db.CheckResult.Where(x => x.RouteUniqueID == route.UniqueID && x.CheckDate == Parameters.Date).ToList();

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

                        var equipmentList = (from x in db.RouteEquipment
                                             join e in db.Equipment
                                             on x.EquipmentUniqueID equals e.UniqueID
                                             where x.RouteUniqueID == route.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID
                                             select new
                                             {
                                                 e.UniqueID,
                                                 e.Name
                                             }).ToList();

                        foreach (var equipment in equipmentList)
                        {
                            var equipmentModel = new EquipmentModel()
                            {
                                EquipmentName = equipment.Name
                            };

                            var checkItemList = (from x in db.RouteEquipmentCheckItem
                                                 join c in db.CheckItem
                                                 on x.CheckItemUniqueID equals c.UniqueID
                                                 where x.RouteUniqueID == route.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == equipment.UniqueID
                                                 select new
                                                 {
                                                     c.UniqueID,
                                                     c.IsFeelItem,
                                                     c.Description
                                                 }).ToList();

                            foreach (var checkItem in checkItemList)
                            {
                                equipmentModel.CheckItemList.Add(new CheckItemModel()
                                {
                                    CheckItemDescription = checkItem.Description,
                                    CheckResultList = checkResultList.Where(x => x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == equipment.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID).Select(x => new CheckResultModel
                                    {
                                        CheckTime = x.CheckTime,
                                        IsFeelItem = checkItem.IsFeelItem,
                                        IsAbnormal = x.IsAbnormal,
                                        IsAlert = x.IsAlert,
                                        Result = x.Result
                                    }).ToList()
                                });
                            }

                            controlPointModel.EquipmentList.Add(equipmentModel);
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
                IWorkbook workBook = null;

                var config = Customized.PFG.Utility.Config.EReportConfig.First(x => x.RouteUniqueID == Model.RouteUniqueID);

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

                foreach (var s in config.SheetList)
                {
                    var sheet = workBook.GetSheetAt(s.SheetIndex);

                    var row = sheet.GetRow(1);
                    var cell = row.GetCell(1);

                    cell.SetCellValue(string.Format("日期：{0}", Model.CheckDate));

                    row = sheet.GetRow(s.CheckUserRowIndex);
                    cell = row.GetCell(s.CheckUserCellIndex);

                    cell.SetCellValue(string.Format("巡檢員:{0}", Model.CheckUser));

                    foreach (var r in s.RowList)
                    {
                        row = sheet.GetRow(r.RowIndex);

                        foreach (var c in r.CellList)
                        {
                            var controlPoint = Model.ControlPointList.FirstOrDefault(x => x.ControlPointDescription == c.ControlPoint);

                            if (controlPoint != null)
                            {
                                var equipment = controlPoint.EquipmentList.FirstOrDefault(x => x.EquipmentName == c.Equipment);

                                if (equipment != null)
                                {
                                    var checkItem = equipment.CheckItemList.FirstOrDefault(x => x.CheckItemDescription == c.CheckItem);

                                    if (checkItem != null)
                                    {
                                        cell = row.GetCell(c.CellIndex);

                                        cell.SetCellValue(checkItem.Result);
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

                var query = Customized.PFG.Utility.Config.EReportConfig.Select(x => x.RouteUniqueID).ToList();

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
