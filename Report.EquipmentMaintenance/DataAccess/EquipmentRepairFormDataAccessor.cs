using System;
using System.Text;
using System.Linq;
using System.Web.Mvc;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DataAccess;
#if ORACLE
using DbEntity.ORACLE;
using DbEntity.ORACLE.EquipmentMaintenance;
#else
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
#endif
using Models.Authenticated;
using Report.EquipmentMaintenance.Models.EquipmentRepairForm;
using System.Transactions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;
using System.IO;

namespace Report.EquipmentMaintenance.DataAccess
{
    public class EquipmentRepairFormDataAccessor
    {
        public static RequestResult GetQueryFormModel(string RepairFormUniqueID, string CheckResultUniqueID,string OrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    
                    var model = new QueryFormModel()
                    {
                        InitParameters = new InitParameters()
                        {
                            RepairFormUniqueID = RepairFormUniqueID,
                            CheckResultUniqueID = CheckResultUniqueID
                        },
                        RepairFormTypeSelectItemList = new List<SelectListItem>() 
                        {                             
                            new SelectListItem() 
                            {
                                Value = "", 
                                Text = "="+ Resources.Resource.SelectAll+"="
                            },
                            Define.SelectListItem(Resources.Resource.MaintenanceRoute)
                        },
                        
                        EuipmentSelectItemList = new List<SelectListItem>()
                        {
                            Define.DefaultSelectListItem(Resources.Resource.None)  
                        }
                    };                                                                              

                    var formTypeList = db.RFormType.OrderBy(x => x.Description).ToList();

                    foreach (var formType in formTypeList)
                    {
                        model.RepairFormTypeSelectItemList.Add(new SelectListItem()
                        {
                            Value = formType.UniqueID,
                            Text = formType.Description
                        });
                    }
                    model.Parameters = new QueryParameters();
                    model.Parameters.EstBeginDateString = DateTime.Now.AddDays(-6).ToString("yyyy-MM-dd");
                    model.Parameters.EstEndDateString = DateTime.Now.ToString("yyyy-MM-dd");
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

         public static RequestResult GetEuipmentList(string OrganizationUniqueID)
        {
            RequestResult result = new RequestResult();
            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    List<SelectListItem> EuipSelectItemList = new List<SelectListItem>();
                    var euipmentList = db.Equipment.Where(x => x.OrganizationUniqueID == OrganizationUniqueID).OrderBy(x => x.UniqueID).ToList();
                    if (OrganizationUniqueID == null || euipmentList.Count == 0)
                    {
                        EuipSelectItemList = new List<SelectListItem>(){
                            Define.DefaultSelectListItem(Resources.Resource.None)  
                        };
                    }
                    else
                    {
                        foreach (var euipment in euipmentList)
                        {
                            EuipSelectItemList.Add(new SelectListItem()
                            {
                                Value = euipment.UniqueID,
                                Text = euipment.Name
                            });
                        }
                    }
                    var model = new QueryFormModel()
                    {
                        EuipmentSelectItemList = EuipSelectItemList
                    };
                    result.ReturnData(model.EuipmentSelectItemList);
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

        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    using (DbEntities db = new DbEntities())
                    {
                        using (EDbEntities edb = new EDbEntities())
                        {
                            var query = (from f in edb.RForm
                                         where (downStreamOrganizationList.Contains(f.OrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(f.OrganizationUniqueID)) || (downStreamOrganizationList.Contains(f.MaintenanceOrganizationUniqueID) && Account.QueryableOrganizationUniqueIDList.Contains(f.MaintenanceOrganizationUniqueID))
                                         select f).AsQueryable();
                            
                                query = query.Where(x => x.EquipmentUniqueID.Contains(Parameters.Equipment));
                            

                            if (!string.IsNullOrEmpty(Parameters.RepairFormType))
                            {
                                if (Parameters.RepairFormType == Resources.Resource.MaintenanceRoute)
                                {
                                    query = query.Where(x => !string.IsNullOrEmpty(x.MFormUniqueID));
                                }
                                else
                                {
                                    query = query.Where(x => x.RFormTypeUniqueID == Parameters.RepairFormType);
                                }
                            }

                            if (Parameters.EstBeginDate.HasValue)
                            {
                                query = query.Where(x => x.EstBeginDate.HasValue);

                                query = query.Where(x => x.EstBeginDate.Value >= Parameters.EstBeginDate.Value);
                            }

                            if (Parameters.EstEndDate.HasValue)
                            {
                                query = query.Where(x => x.EstEndDate.HasValue);

                                query = query.Where(x => x.EstEndDate.Value <= Parameters.EstEndDate.Value);
                            }

                            var formList = query.ToList();//查询条件下的所有修复单资料

                            var organization = OrganizationDataAccessor.GetOrganization(Parameters.OrganizationUniqueID);

                            var model = new GridViewModel()
                            {
                                Permission = Account.OrganizationPermission(Parameters.OrganizationUniqueID),
                                OrganizationUniqueID = Parameters.OrganizationUniqueID,
                                OrganizationDescription = organization != null ? organization.Description : "*",
                                FullOrganizationDescription = organization != null ? organization.FullDescription : "*"
                            };

                            foreach (var form in formList)
                            {
                                var item = new GridItem()
                                {
                                    UniqueID = form.UniqueID,
                                    OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(form.OrganizationUniqueID),
                                    MaintenanceOrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(form.MaintenanceOrganizationUniqueID),
                                    VHNO = form.VHNO,
                                    IsSubmit = form.IsSubmit,
                                    Subject = form.Subject,
                                    EstBeginDate = form.EstBeginDate,
                                    EstEndDate = form.EstEndDate,
                                    //CreateUserID = form.CreateUserID,
                                    //CreateTime = form.CreateTime,
                                    //JobManagerID = form.JobManagerID,
                                    RefuseReason = form.RefuseReason,
                                    JobRefuseReason = form.JobRefuseReason,
                                    JobTime = form.JobTime,
                                    JobUserID = form.TakeJobUserID,
                                    TakeJobTime = form.TakeJobTime
                                };

                                var flow = edb.RFormFlow.FirstOrDefault(x => x.RFormUniqueID == item.UniqueID);

                                if (flow != null)
                                {
                                    item.IsClosed = flow.IsClosed;
                                }
                                else
                                {
                                    item.IsClosed = false;
                                }

                                //if (!string.IsNullOrEmpty(item.CreateUserID))
                                //{
                                //    var user = db.User.FirstOrDefault(x => x.ID == item.CreateUserID);

                                //    if (user != null)
                                //    {
                                //        item.CreateUserName = user.Name;
                                //    }
                                //}

                                //if (!string.IsNullOrEmpty(item.JobManagerID))
                                //{
                                //    var user = db.User.FirstOrDefault(x => x.ID == item.JobManagerID);

                                //    if (user != null)
                                //    {
                                //        item.JobManagerName = user.Name;
                                //    }
                                //}

                                //if (!string.IsNullOrEmpty(item.JobUserID))
                                //{
                                //    var user = db.User.FirstOrDefault(x => x.ID == item.JobUserID);

                                //    if (user != null)
                                //    {
                                //        item.JobUserName = user.Name;
                                //    }
                                //}

                                if (!string.IsNullOrEmpty(form.MFormUniqueID))
                                {
                                    item.RepairFormType = Resources.Resource.MaintenanceRoute;
                                }
                                else
                                {
                                    var repairFormType = edb.RFormType.FirstOrDefault(x => x.UniqueID == form.RFormTypeUniqueID);

                                    if (repairFormType != null)
                                    {
                                        item.RepairFormType = repairFormType.Description;
                                    }
                                    else
                                    {
                                        item.RepairFormType = "-";
                                    }
                                }

                                if (!string.IsNullOrEmpty(form.EquipmentUniqueID))
                                {
                                    var equipment = edb.Equipment.FirstOrDefault(x => x.UniqueID == form.EquipmentUniqueID);

                                    if (equipment != null)
                                    {
                                        item.EquipmentID = equipment.ID;
                                        item.EquipmentName = equipment.Name;

                                        if (!string.IsNullOrEmpty(form.PartUniqueID))
                                        {
                                            var part = edb.EquipmentPart.FirstOrDefault(x => x.EquipmentUniqueID == equipment.UniqueID && x.UniqueID == form.PartUniqueID);

                                            if (part != null)
                                            {
                                                item.PartDescription = part.Description;
                                            }
                                        }
                                    }
                                }

                                model.ItemList.Add(item);
                            }

                            if (!string.IsNullOrEmpty(Parameters.Status))
                            {
                                int status = int.Parse(Parameters.Status);

                                model.ItemList = model.ItemList.Where(x => x.Status == status).ToList();
                            }

                            model.ItemList = model.ItemList.OrderBy(x => x.VHNO).ThenBy(x => x.OrganizationDescription).ThenBy(x => x.MaintenanceOrganizationDescription).ToList();
                            if (model.ItemList.Count() != 0)
                            {
                                //填充ExportViewModel资料
                                var mformuniqueid = formList.Select(x => x.MFormUniqueID).Distinct();//去重后的MFormUniqueID
                                var mformquery = edb.MForm.Where(x => mformuniqueid.Contains(x.UniqueID)).AsQueryable();//去重后的定保单
                                var planbegindate = mformquery.OrderBy(x => x.BeginDate).Select(x => x.BeginDate).FirstOrDefault();//获取计划日期


                                var MainOrganization = edb.Equipment.Where(x => x.UniqueID == Parameters.Equipment).Select(x => x.MaintenanceOrganizationUniqueID).FirstOrDefault();
                                var manageid = db.Organization.Where(x => x.UniqueID == MainOrganization).Select(x => x.ManagerUserID).FirstOrDefault();
                                var managename = db.User.Where(x => x.ID == manageid).Select(x => x.Name).FirstOrDefault();
                                var export = new ExportViewModel()
                                {
                                    EquipmentID = model.ItemList[0].EquipmentID,
                                    EquipmentName = model.ItemList[0].EquipmentName,
                                    PlanBeginDate = planbegindate,
                                    PutDate = null,
                                    StatusDescription = model.ItemList[0].StatusDescription,
                                    ManagerUserID = managename,
                                    Engineer = null,
                                    ImplementUser = null
                                };
                               
                                var mjobuniqueid = mformquery.Select(x => x.MJobUniqueID).Distinct();//去重后的MJobUniqueID
                                var mjobquery = edb.MJob.Where(x => mjobuniqueid.Contains(x.UniqueID)).AsQueryable();//去重后获取的MJOB资料
                                foreach (var mjob in mjobquery)
                                {
                                    var Standardquery = edb.MJobEquipmentStandard.Where(x => x.MJobUniqueID == mjob.UniqueID && x.EquipmentUniqueID == Parameters.Equipment).AsQueryable();
                                    foreach (var standard in Standardquery)
                                    {
                                        var partdescrip = edb.EquipmentPart.Where(x => x.UniqueID == standard.PartUniqueID).Select(x => x.Description).FirstOrDefault();//点检部位名称
                                        var standarddescrip = edb.Standard.Where(x => x.UniqueID == standard.StandardUniqueID).Select(x => x.Description).FirstOrDefault();//点检项目名称
                                        var exportitem = new ExportItem()
                                        {
                                            CycleCount = mjob.CycleCount,
                                            CycleMode = mjob.CycleMode,
                                            StandardPartDescription = partdescrip,
                                            StandardDescription = standarddescrip,
                                            State = null,
                                            Manage = null
                                        };
                                        model.ExportList.Add(exportitem);
                                    }
                                }
                                model.ExportList = model.ExportList.OrderBy(x => x.StandardPartDescription).ToList();//根据部位排序
                                if (model.ExportList.Count() != 0)
                                {
                                    model.Export = export;
                                }
                            }
                            result.ReturnData(model);
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

        public static RequestResult Query(Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var parameters = new QueryParameters()
                {
                    OrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(Account.OrganizationUniqueID)
                };

                result = Query(parameters, Account);

                if (result.IsSuccess)
                {
                    var model = result.Data as GridViewModel;

                    model.ItemList = model.ItemList.Where(x => x.Status == 1 || x.Status == 3 || x.Status == 4 || x.Status == 5).ToList();

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

       

        public static ExcelExportModel Export(GridViewModel Model, Define.EnumExcelVersion ExcelVersion)
        {
            
            try
            {                
                    IWorkbook wk = new XSSFWorkbook();

                    ISheet sheet = wk.CreateSheet(Resources.Resource.EquipmentRForm);

                    //先创建100行单元格
                    for (var i = 0; i < 101; i++)
                    {
                        sheet.CreateRow(i);

                        for (var j = 0; j < 8; j++)
                        {
                            sheet.GetRow(i).CreateCell(j);
                        }
                    }
                    //设置样式
                    sheet.DisplayGridlines = false;       //不显示单元格边框
                    sheet.SetColumnWidth(0, 5 * 250);     //设置单元格长度
                    sheet.SetColumnWidth(1, 15 * 250);
                    sheet.SetColumnWidth(2, 30 * 250);
                    sheet.SetColumnWidth(3, 15 * 250);
                    sheet.SetColumnWidth(4, 15 * 250);
                    sheet.SetColumnWidth(5, 15 * 250);
                    sheet.SetColumnWidth(6, 15 * 250);
                    sheet.SetColumnWidth(7, 5 * 250);


                    //設置樣式
                    ICellStyle Titlestyle = wk.CreateCellStyle();   //标题1的样式
                    IFont Titlefont = wk.CreateFont();
                    Titlefont.FontHeightInPoints = 18;
                    Titlestyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;  //水平居中
                    Titlestyle.VerticalAlignment = VerticalAlignment.Center;// 垂直居中
                    Titlestyle.SetFont(Titlefont);

                    ICellStyle Titlestyles = wk.CreateCellStyle();   //标题2的样式
                    IFont Titlefont1 = wk.CreateFont();
                    Titlefont1.FontHeightInPoints = 16;
                    Titlestyles.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;  //水平居中
                    Titlestyles.VerticalAlignment = VerticalAlignment.Center;// 垂直居中
                    Titlestyles.SetFont(Titlefont1);

                    //上下左右都有边框
                    ICellStyle Borderstyle = wk.CreateCellStyle();//上下左右都有边框
                    Borderstyle.WrapText = true;//自动换行
                    Borderstyle.BorderBottom = BorderStyle.Thin;
                    Borderstyle.BorderTop = BorderStyle.Thin;
                    Borderstyle.BorderLeft = BorderStyle.Thin;
                    Borderstyle.BorderRight = BorderStyle.Thin;
                    Borderstyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;//水平居中
                    Borderstyle.VerticalAlignment = VerticalAlignment.Center;// 垂直居中

                   

                    //統一合併單元格
                    sheet.AddMergedRegion(new CellRangeAddress(0, 0, 1, 6));
                    sheet.AddMergedRegion(new CellRangeAddress(1, 1, 1, 6));
                    for (var i = 3; i < 8; i++)
                    {
                        sheet.AddMergedRegion(new CellRangeAddress(i, i, 2, 3));
                    }
                    for (var j = 4; j < 7; j++)
                    {
                        sheet.AddMergedRegion(new CellRangeAddress(4, 7, j, j));
                    }
                    
                    
                       sheet.AddMergedRegion(new CellRangeAddress(8, 8, 3, 4));
                    

                    sheet.GetRow(0).GetCell(1).SetCellValue(Resources.Resource.TWGY);//表頭標題                  

                    for (var i = 1; i < 7; i++)
                    {
                        sheet.GetRow(0).GetCell(i).CellStyle = Titlestyle;//表頭樣式
                    }
                    sheet.GetRow(1).GetCell(1).SetCellValue(Resources.Resource.EquipMFormReport);//表頭1標題  
                    for (var i = 1; i < 7; i++)
                    {
                        sheet.GetRow(1).GetCell(i).CellStyle = Titlestyles;//表頭2樣式
                    }

                    //創建填充查詢條件數據
                    sheet.GetRow(2).GetCell(5).SetCellValue(Resources.Resource.PrintDate + ":");
                    sheet.GetRow(2).GetCell(6).SetCellValue(DateTime.Now.ToString("yyyy/MM/dd"));

                    //創建填充資料
                    sheet.GetRow(3).GetCell(1).SetCellValue(Resources.Resource.EquipUniqueID + "：");
                    sheet.GetRow(3).GetCell(2).SetCellValue(Model.Export.EquipmentID);
                    sheet.GetRow(3).GetCell(4).SetCellValue(Resources.Resource.ManageUserID);
                    sheet.GetRow(3).GetCell(5).SetCellValue(Resources.Resource.Engineer);
                    sheet.GetRow(3).GetCell(6).SetCellValue(Resources.Resource.ImplementUser);


                    sheet.GetRow(4).GetCell(1).SetCellValue(Resources.Resource.EquipmentName+ "：");
                    sheet.GetRow(4).GetCell(2).SetCellValue(Model.Export.EquipmentName);
                    sheet.GetRow(4).GetCell(4).SetCellValue(Model.Export.ManagerUserID);
                    sheet.GetRow(4).GetCell(5).SetCellValue(Model.Export.Engineer);
                    sheet.GetRow(4).GetCell(6).SetCellValue(Model.Export.ImplementUser);

                    sheet.GetRow(5).GetCell(1).SetCellValue(Resources.Resource.PlanBeginDate + "：");
                    sheet.GetRow(5).GetCell(2).SetCellValue(Model.Export.PlanBeginDateString);
                    sheet.GetRow(6).GetCell(1).SetCellValue(Resources.Resource.PutDate + "：");
                    sheet.GetRow(6).GetCell(2).SetCellValue(Model.Export.PutDateString);

                    sheet.GetRow(7).GetCell(1).SetCellValue(Resources.Resource.VHNOStatus + "：");
                    sheet.GetRow(7).GetCell(2).SetCellValue(Model.Export.StatusDescription);

                    sheet.GetRow(8).GetCell(1).SetCellValue(Resources.Resource.Cycle);
                    sheet.GetRow(8).GetCell(2).SetCellValue(Resources.Resource.StandardPart);
                    sheet.GetRow(8).GetCell(3).SetCellValue(Resources.Resource.StandardItem);
                    sheet.GetRow(8).GetCell(5).SetCellValue(Resources.Resource.State);
                    sheet.GetRow(8).GetCell(6).SetCellValue(Resources.Resource.Deal);
                    int row = 9;
                    foreach (var export in Model.ExportList)
                    {
                        sheet.AddMergedRegion(new CellRangeAddress(row, row, 3, 4));
                        sheet.GetRow(row).GetCell(1).SetCellValue(export.Cycle);
                        sheet.GetRow(row).GetCell(2).SetCellValue(export.StandardPartDescription);
                        sheet.GetRow(row).GetCell(3).SetCellValue(export.StandardDescription);
                        sheet.GetRow(row).GetCell(5).SetCellValue(export.State);
                        sheet.GetRow(row).GetCell(6).SetCellValue(export.Manage);
                        row++;
                    }
                    sheet.AddMergedRegion(new CellRangeAddress(row, row, 3, 4));
                    sheet.AddMergedRegion(new CellRangeAddress(row + 1, row + 6, 1, 1));
                    sheet.AddMergedRegion(new CellRangeAddress(row + 1, row + 6, 2, 6));

                    sheet.GetRow(row + 1).GetCell(1).SetCellValue(Resources.Resource.Tips);
                    sheet.GetRow(row + 7).GetCell(5).SetCellValue(Resources.Resource.Fieldcadre);

                    for (var i = 3; i < row + 7; i++)
                    {                       
                        for (var j = 1; j < 7; j++)
                        {
                            sheet.GetRow(i).GetCell(j).CellStyle = Borderstyle;
                        }

                    }


                    var output = new ExcelExportModel(Resources.Resource.EquipmentRForm, ExcelVersion);

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

                    return output;
                
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);

                return null;
            }
        }
    }
}
