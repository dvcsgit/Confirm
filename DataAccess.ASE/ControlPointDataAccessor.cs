using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.ASE;
using Models.Shared;
using Models.Authenticated;
using Models.EquipmentMaintenance.ControlPointManagement;
using System.IO;
using ZXing.Common;
using ZXing;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;

#if !DEBUG
using System.Transactions;
#endif

namespace DataAccess.ASE
{
    public class ControlPointDataAccessor
    {
        public static RequestResult ExportQRCode(List<string> UniqueIDList, Account Account, Define.EnumExcelVersion ExcelVersion, string fileName)
        {
            RequestResult result = new RequestResult();
            try
            {
                IWorkbook wk = null;


                if (ExcelVersion == Define.EnumExcelVersion._2003)
                {
                    wk = new HSSFWorkbook();
                }
                else
                {
                    wk = new XSSFWorkbook();
                }

                ISheet sheet = wk.CreateSheet(Resources.Resource.CalibrationForm);

                // set zoom size
                sheet.SetZoom(2, 1);


                //側邊標題
                ICellStyle titleCellStyle = wk.CreateCellStyle();
                IFont titleFont = wk.CreateFont();
                titleFont.FontName = "Trebuchet MS";
                titleFont.FontHeightInPoints = 8;
                titleFont.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
                titleCellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                titleCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
                titleCellStyle.SetFont(titleFont);


                //內容
                ICellStyle contentCellStyle = wk.CreateCellStyle();
                IFont contentFont = wk.CreateFont();
                contentFont.FontName = "Trebuchet MS";
                contentFont.FontHeightInPoints = 7;
                contentFont.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
                contentFont.Underline = FontUnderlineType.Single;
                contentCellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                contentCellStyle.SetFont(contentFont);

                var index = 0;
                //var startColumnForPrint = 0;
                //var endColumnForPrint = 2;
                //var startRowForPrint = 0;//dynamic
                var endRowForPrint = 0;//dynamic




                // TODO should move to other place

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = db.CONTROLPOINT.Where(x => UniqueIDList.Contains(x.UNIQUEID)).Select(x => new
                    {

                        x.ID,
                        x.DESCRIPTION,
                        x.TAGID
                    });



                    //int columnHeightUnit = 20;
                    //double contentHeight = 9.75;




                    //1~23 one size
                    int page = 1;
                    foreach (var item in query)
                    {
                        // create empty row
                        var startIdx = index;
                        var endIdx = (index + 22);
                        //_logger.Info("startIdx:{0} ~ endIdx:{1}", startIdx, endIdx);

                        // create empty row
                        for (int i = startIdx; i <= endIdx; i++)
                        {
                            var row = sheet.CreateRow(index);

                            //Create cell
                            for (int j = 0; j <= 8; j++)
                            {
                                row.CreateCell(j);
                            }

                            //var row = sheet.CreateRow(index);
                            //_logger.Debug("index:{0}", i);
                            index = i;

                        }

                        var qrSIdxRow = startIdx + 2;
                        var qrEIdxRow = startIdx + 15;
                        var idIdxRow = startIdx + 17;
                        var nameRow = startIdx + 20;

                        //_logger.Warn("qrSIdxRow:{0}", qrSIdxRow);
                        //_logger.Warn("qrEIdxRow:{0}", qrEIdxRow);
                        //_logger.Warn("idIdxRow:{0}", idIdxRow);
                        //_logger.Warn("nameRow:{0}", nameRow);
                        var barcodeWriter = new BarcodeWriter
                        {
                            Format = BarcodeFormat.QR_CODE,
                            Options = new EncodingOptions
                            {
                                Height = 450,
                                Width = 450,
                                Margin = 1

                            }
                        };
                        int pictureIdx = 0;
                        var path = Path.Combine(Config.TempFolder, "ControlPoint_" + Guid.NewGuid().ToString() + ".jpg");
                        using (var bitmap = barcodeWriter.Write(item.TAGID))
                        using (var stream = new MemoryStream())
                        {


                            bitmap.Save(path);
                            byte[] bytes = System.IO.File.ReadAllBytes(path);
                            pictureIdx = wk.AddPicture(bytes, PictureType.JPEG);
                        }

                        IDrawing drawing = sheet.CreateDrawingPatriarch();
                        IClientAnchor anchor = drawing.CreateAnchor(0, 0, 0, 0, 2, qrSIdxRow, 6, qrEIdxRow);


                        //ref http://www.cnblogs.com/firstcsharp/p/4896121.html

                        if (anchor != null)
                        {
                            anchor.AnchorType = AnchorType.MoveDontResize;
                            drawing.CreatePicture(anchor, pictureIdx);
                        }

                        index += 4;


                        var row_id = sheet.GetRow(idIdxRow);
                        row_id.GetCell(2).SetCellValue(item.ID);


                        var row_name = sheet.GetRow(nameRow);
                        row_name.GetCell(2).SetCellValue(item.DESCRIPTION);

                        page++;
                        index++;




                        ////blank
                        //var row1 = sheet.CreateRow(index);
                        //row1.Height = (short)(6 * columnHeightUnit);

                        //// s/n
                        //var row2Idx = index + 1;
                        //var row2 = sheet.CreateRow(row2Idx);
                        //row2.Height = (short)(contentHeight * columnHeightUnit);
                        //var row2Cell1 = row2.CreateCell(0);
                        //row2Cell1.SetCellValue("S/N:");
                        //row2Cell1.CellStyle = titleCellStyle;



                        //var row2Cell2 = row2.CreateCell(1);
                        //row2Cell2.SetCellValue(item.SN);
                        //row2Cell2.CellStyle = contentCellStyle;

                        //row2.CreateCell(2);

                        //// DATE
                        //var row3Idx = index + 2;
                        //var row3 = sheet.CreateRow(row3Idx);
                        //row3.Height = (short)(contentHeight * columnHeightUnit);
                        //var row3Cell1 = row3.CreateCell(0);
                        //row3Cell1.SetCellValue("DATE:");
                        //row3Cell1.CellStyle = titleCellStyle;


                        //var row3Cell2 = row3.CreateCell(1);
                        //row3Cell2.SetCellValue(item.CALDate);
                        //row3Cell2.CellStyle = contentCellStyle;


                        //row3.CreateCell(2);

                        //// SIGN
                        //var row4Idx = index + 3;
                        //var row4 = sheet.CreateRow(row4Idx);
                        //row4.Height = (short)(contentHeight * columnHeightUnit);
                        //var row4Cell1 = row4.CreateCell(0);
                        //row4Cell1.SetCellValue("SIGN:");
                        //row4Cell1.CellStyle = titleCellStyle;


                        //var row4Cell2 = row4.CreateCell(1);
                        //row4Cell2.SetCellValue(item.Sign);
                        //row4Cell2.CellStyle = contentCellStyle;


                        //row4.CreateCell(2);

                        //sheet.AddMergedRegion(new CellRangeAddress(row2Idx, row4Idx, 2, 2));





                    }
                    endRowForPrint = index > 1 ? index - 1 : index;
                    //定欄位單位寬度



                }

                //set the print area for the first sheet
                //wk.SetPrintArea(0, startColumnForPrint, endColumnForPrint, startRowForPrint, endRowForPrint);

                // Use reflection go call internal method GetCTWorksheet()
                //MethodInfo methodInfo = sheet.GetType().GetMethod("GetCTWorksheet", BindingFlags.NonPublic | BindingFlags.Instance);
                //var ct = (CT_Worksheet)methodInfo.Invoke(sheet, new object[] { });

                //CT_SheetView view = ct.sheetViews.GetSheetViewArray(0);
                //view.view = ST_SheetViewType.pageBreakPreview;

                //Save file
                var savePath = Path.Combine(Config.TempFolder, fileName);
                using (FileStream file = new FileStream(savePath, FileMode.Create))
                {
                    wk.Write(file);
                    file.Close();
                }



                result.Data = fileName;
                result.IsSuccess = true;
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
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                    var query = db.CONTROLPOINT.Where(x => downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID) && Account.QueryableOrganizationUniqueIDList.Contains(x.ORGANIZATIONUNIQUEID)).AsQueryable();

                    if (!string.IsNullOrEmpty(Parameters.Keyword))
                    {
                        query = query.Where(x => x.ID.Contains(Parameters.Keyword) || x.DESCRIPTION.Contains(Parameters.Keyword));
                    }

                    var organization = OrganizationDataAccessor.GetOrganization(Parameters.OrganizationUniqueID);

                    result.ReturnData(new GridViewModel()
                    {
                        Permission = Account.OrganizationPermission(Parameters.OrganizationUniqueID),
                        OrganizationUniqueID = Parameters.OrganizationUniqueID,
                        OrganizationDescription = organization.Description,
                        FullOrganizationDescription = organization.FullDescription,
                        ItemList = query.ToList().Select(x => new GridItem
                        {
                            UniqueID = x.UNIQUEID,
                            Permission = Account.OrganizationPermission(x.ORGANIZATIONUNIQUEID),
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.ORGANIZATIONUNIQUEID),
                            ID = x.ID,
                            Description = x.DESCRIPTION
                        }).OrderBy(x => x.OrganizationDescription).ThenBy(x => x.ID).ToList()
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

        public static RequestResult GetDetailViewModel(string UniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var controlPoint = db.CONTROLPOINT.First(x => x.UNIQUEID == UniqueID);

                    var model = new DetailViewModel()
                    {
                        UniqueID = controlPoint.UNIQUEID,
                        Permission = Account.OrganizationPermission(controlPoint.ORGANIZATIONUNIQUEID),
                        OrganizationUniqueID = controlPoint.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(controlPoint.ORGANIZATIONUNIQUEID),
                        ID = controlPoint.ID,
                        Description = controlPoint.DESCRIPTION,
                        IsFeelItemDefaultNormal = controlPoint.ISFEELITEMDEFAULTNORMAL == "Y",
                        TagID = controlPoint.TAGID,
                        Remark = controlPoint.REMARK
                    };

                    var checkItemList = (from x in db.CONTROLPOINTCHECKITEM
                                         join c in db.CHECKITEM
                                         on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                         where x.CONTROLPOINTUNIQUEID == UniqueID
                                         select new
                                         {
                                             x = x,
                                             c = c
                                         }).OrderBy(x => x.c.CHECKTYPE).ThenBy(x => x.c.ID).ToList();

                    foreach (var checkItem in checkItemList)
                    {
                        model.CheckItemList.Add(new CheckItemModel
                        {
                            UniqueID = checkItem.c.UNIQUEID,
                            CheckType = checkItem.c.CHECKTYPE,
                            ID = checkItem.c.ID,
                            Description = checkItem.c.DESCRIPTION,
                            IsFeelItem = checkItem.c.ISFEELITEM == "Y",
                            IsAccumulation = checkItem.c.ISACCUMULATION == "Y",
                            IsInherit = checkItem.x.ISINHERIT == "Y",
                            OriLowerLimit = checkItem.c.LOWERLIMIT.HasValue ? double.Parse(checkItem.c.LOWERLIMIT.Value.ToString()) : default(double?),
                            OriLowerAlertLimit = checkItem.c.LOWERALERTLIMIT.HasValue ? double.Parse(checkItem.c.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                            OriUpperAlertLimit = checkItem.c.UPPERALERTLIMIT.HasValue ? double.Parse(checkItem.c.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                            OriUpperLimit = checkItem.c.UPPERLIMIT.HasValue ? double.Parse(checkItem.c.UPPERLIMIT.Value.ToString()) : default(double?),
                            OriAccumulationBase = checkItem.c.ACCUMULATIONBASE.HasValue ? double.Parse(checkItem.c.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                            OriUnit = checkItem.c.UNIT,
                            OriRemark = checkItem.c.REMARK,
                            LowerLimit = checkItem.x.LOWERLIMIT.HasValue ? double.Parse(checkItem.x.LOWERLIMIT.Value.ToString()) : default(double?),
                            LowerAlertLimit = checkItem.x.LOWERALERTLIMIT.HasValue ? double.Parse(checkItem.x.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                            UpperAlertLimit = checkItem.x.UPPERALERTLIMIT.HasValue ? double.Parse(checkItem.x.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                            UpperLimit = checkItem.x.UPPERLIMIT.HasValue ? double.Parse(checkItem.x.UPPERLIMIT.Value.ToString()) : default(double?),
                            AccumulationBase = checkItem.x.ACCUMULATIONBASE.HasValue ? double.Parse(checkItem.x.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                            Unit = checkItem.x.UNIT,
                            Remark = checkItem.x.REMARK
                        });
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

        public static RequestResult Create(CreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var exists = db.CONTROLPOINT.FirstOrDefault(x => x.ORGANIZATIONUNIQUEID == Model.OrganizationUniqueID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
                        if (!string.IsNullOrEmpty(Model.FormInput.TagID) && db.CONTROLPOINT.Any(x => x.TAGID == Model.FormInput.TagID))
                        {
                            var controlPoint = db.CONTROLPOINT.First(x => x.TAGID == Model.FormInput.TagID);

                            var organization = OrganizationDataAccessor.GetOrganization(controlPoint.ORGANIZATIONUNIQUEID);

                            result.ReturnFailedMessage(string.Format("{0} {1} {2} {3} {4}", Resources.Resource.TagID, Model.FormInput.TagID, Resources.Resource.Exists, organization.Description, controlPoint.DESCRIPTION));
                        }
                        else
                        {
                            string uniqueID = Guid.NewGuid().ToString();

                            db.CONTROLPOINT.Add(new CONTROLPOINT()
                            {
                                UNIQUEID = uniqueID,
                                ORGANIZATIONUNIQUEID = Model.OrganizationUniqueID,
                                ID = Model.FormInput.ID,
                                DESCRIPTION = Model.FormInput.Description,
                                ISFEELITEMDEFAULTNORMAL = Model.FormInput.IsFeelItemDefaultNormal?"Y":"N",
                                TAGID = Model.FormInput.TagID,
                                REMARK = Model.FormInput.Remark,
                                LASTMODIFYTIME = DateTime.Now
                            });

                            db.CONTROLPOINTCHECKITEM.AddRange(Model.CheckItemList.Select(x => new CONTROLPOINTCHECKITEM
                            {
                                CONTROLPOINTUNIQUEID = uniqueID,
                                CHECKITEMUNIQUEID = x.UniqueID,
                                ISINHERIT = x.IsInherit ? "Y" : "N",
                                LOWERLIMIT = x.LowerLimit.HasValue?decimal.Parse(x.LowerLimit.Value.ToString()):default(decimal?),
                                LOWERALERTLIMIT = x.LowerAlertLimit.HasValue ? decimal.Parse(x.LowerAlertLimit.Value.ToString()) : default(decimal?),
                                UPPERALERTLIMIT = x.UpperAlertLimit.HasValue ? decimal.Parse(x.UpperAlertLimit.Value.ToString()) : default(decimal?),
                                UPPERLIMIT = x.UpperLimit.HasValue ? decimal.Parse(x.UpperLimit.Value.ToString()) : default(decimal?),
                                ACCUMULATIONBASE = x.AccumulationBase.HasValue ? decimal.Parse(x.AccumulationBase.Value.ToString()) : default(decimal?),
                                UNIT = x.Unit,
                                REMARK = x.Remark
                            }).ToList());

                            db.SaveChanges();

                            result.ReturnData(uniqueID, string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.ControlPoint, Resources.Resource.Success));
                        }
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.ControlPointID, Resources.Resource.Exists));
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

        public static RequestResult GetCopyFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var controlPoint = db.CONTROLPOINT.First(x => x.UNIQUEID == UniqueID);

                    var model = new CreateFormModel()
                    {
                        OrganizationUniqueID = controlPoint.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(controlPoint.ORGANIZATIONUNIQUEID),
                        FormInput = new FormInput()
                        {
                            IsFeelItemDefaultNormal = controlPoint.ISFEELITEMDEFAULTNORMAL == "Y",
                            Remark = controlPoint.REMARK
                        }
                    };

                    var checkItemList = (from x in db.CONTROLPOINTCHECKITEM
                                         join c in db.CHECKITEM
                                         on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                         where x.CONTROLPOINTUNIQUEID == UniqueID
                                         select new
                                         {
                                             x = x,
                                             c = c
                                         }).OrderBy(x => x.c.CHECKTYPE).ThenBy(x => x.c.ID).ToList();

                    foreach (var checkItem in checkItemList)
                    {
                        model.CheckItemList.Add(new CheckItemModel
                        {
                            UniqueID = checkItem.c.UNIQUEID,
                            CheckType = checkItem.c.CHECKTYPE,
                            ID = checkItem.c.ID,
                            Description = checkItem.c.DESCRIPTION,
                            IsFeelItem = checkItem.c.ISFEELITEM == "Y",
                            IsAccumulation = checkItem.c.ISACCUMULATION == "Y",
                            IsInherit = checkItem.x.ISINHERIT == "Y",
                            OriLowerLimit = checkItem.c.LOWERLIMIT.HasValue ? double.Parse(checkItem.c.LOWERLIMIT.Value.ToString()) : default(double?),
                            OriLowerAlertLimit = checkItem.c.LOWERALERTLIMIT.HasValue ? double.Parse(checkItem.c.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                            OriUpperAlertLimit = checkItem.c.UPPERALERTLIMIT.HasValue ? double.Parse(checkItem.c.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                            OriUpperLimit = checkItem.c.UPPERLIMIT.HasValue ? double.Parse(checkItem.c.UPPERLIMIT.Value.ToString()) : default(double?),
                            OriAccumulationBase = checkItem.c.ACCUMULATIONBASE.HasValue ? double.Parse(checkItem.c.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                            OriUnit = checkItem.c.UNIT,
                            OriRemark = checkItem.c.REMARK,
                            LowerLimit = checkItem.x.LOWERLIMIT.HasValue ? double.Parse(checkItem.x.LOWERLIMIT.Value.ToString()) : default(double?),
                            LowerAlertLimit = checkItem.x.LOWERALERTLIMIT.HasValue ? double.Parse(checkItem.x.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                            UpperAlertLimit = checkItem.x.UPPERALERTLIMIT.HasValue ? double.Parse(checkItem.x.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                            UpperLimit = checkItem.x.UPPERLIMIT.HasValue ? double.Parse(checkItem.x.UPPERLIMIT.Value.ToString()) : default(double?),
                            AccumulationBase = checkItem.x.ACCUMULATIONBASE.HasValue ? double.Parse(checkItem.x.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                            Unit = checkItem.x.UNIT,
                            Remark = checkItem.x.REMARK
                        });
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

        public static RequestResult GetEditFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var controlPoint = db.CONTROLPOINT.First(x => x.UNIQUEID == UniqueID);

                    var model = new EditFormModel()
                    {
                        UniqueID = controlPoint.UNIQUEID,
                        OrganizationUniqueID = controlPoint.ORGANIZATIONUNIQUEID,
                        ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(controlPoint.ORGANIZATIONUNIQUEID),
                        FormInput = new FormInput()
                        {
                            ID = controlPoint.ID,
                            Description = controlPoint.DESCRIPTION,
                            IsFeelItemDefaultNormal = controlPoint.ISFEELITEMDEFAULTNORMAL == "Y",
                            TagID = controlPoint.TAGID,
                            Remark = controlPoint.REMARK
                        }
                    };

                    var checkItemList = (from x in db.CONTROLPOINTCHECKITEM
                                         join c in db.CHECKITEM
                                         on x.CHECKITEMUNIQUEID equals c.UNIQUEID
                                         where x.CONTROLPOINTUNIQUEID == UniqueID
                                         select new
                                         {
                                             x = x,
                                             c = c
                                         }).OrderBy(x => x.c.CHECKTYPE).ThenBy(x => x.c.ID).ToList();

                    foreach (var checkItem in checkItemList)
                    {
                        model.CheckItemList.Add(new CheckItemModel
                        {
                            UniqueID = checkItem.c.UNIQUEID,
                            CheckType = checkItem.c.CHECKTYPE,
                            ID = checkItem.c.ID,
                            Description = checkItem.c.DESCRIPTION,
                            IsFeelItem = checkItem.c.ISFEELITEM == "Y",
                            IsAccumulation = checkItem.c.ISACCUMULATION == "Y",
                            IsInherit = checkItem.x.ISINHERIT == "Y",
                            OriLowerLimit = checkItem.c.LOWERLIMIT.HasValue ? double.Parse(checkItem.c.LOWERLIMIT.Value.ToString()) : default(double?),
                            OriLowerAlertLimit = checkItem.c.LOWERALERTLIMIT.HasValue ? double.Parse(checkItem.c.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                            OriUpperAlertLimit = checkItem.c.UPPERALERTLIMIT.HasValue ? double.Parse(checkItem.c.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                            OriUpperLimit = checkItem.c.UPPERLIMIT.HasValue ? double.Parse(checkItem.c.UPPERLIMIT.Value.ToString()) : default(double?),
                            OriAccumulationBase = checkItem.c.ACCUMULATIONBASE.HasValue ? double.Parse(checkItem.c.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                            OriUnit = checkItem.c.UNIT,
                            OriRemark = checkItem.c.REMARK,
                            LowerLimit = checkItem.x.LOWERLIMIT.HasValue ? double.Parse(checkItem.x.LOWERLIMIT.Value.ToString()) : default(double?),
                            LowerAlertLimit = checkItem.x.LOWERALERTLIMIT.HasValue ? double.Parse(checkItem.x.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                            UpperAlertLimit = checkItem.x.UPPERALERTLIMIT.HasValue ? double.Parse(checkItem.x.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                            UpperLimit = checkItem.x.UPPERLIMIT.HasValue ? double.Parse(checkItem.x.UPPERLIMIT.Value.ToString()) : default(double?),
                            AccumulationBase = checkItem.x.ACCUMULATIONBASE.HasValue ? double.Parse(checkItem.x.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                            Unit = checkItem.x.UNIT,
                            Remark = checkItem.x.REMARK
                        });
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

        public static RequestResult Edit(EditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var controlPoint = db.CONTROLPOINT.First(x => x.UNIQUEID == Model.UniqueID);

                    var exists = db.CONTROLPOINT.FirstOrDefault(x => x.UNIQUEID != controlPoint.UNIQUEID && x.ORGANIZATIONUNIQUEID == controlPoint.ORGANIZATIONUNIQUEID && x.ID == Model.FormInput.ID);

                    if (exists == null)
                    {
                        if (!string.IsNullOrEmpty(Model.FormInput.TagID) && db.CONTROLPOINT.Any(x => x.UNIQUEID != controlPoint.UNIQUEID && x.TAGID == Model.FormInput.TagID))
                        {
                            var query = db.CONTROLPOINT.First(x => x.UNIQUEID != controlPoint.UNIQUEID && x.TAGID == Model.FormInput.TagID);

                            var organization = OrganizationDataAccessor.GetOrganization(query.ORGANIZATIONUNIQUEID);

                            result.ReturnFailedMessage(string.Format("{0} {1} {2} {3} {4}", Resources.Resource.TagID, Model.FormInput.TagID, Resources.Resource.Exists, organization.Description, controlPoint.DESCRIPTION));
                        }
                        else
                        {
#if !DEBUG
                    using (TransactionScope trans = new TransactionScope())
                    {
#endif
                            #region ControlPoint
                            controlPoint.ID = Model.FormInput.ID;
                            controlPoint.DESCRIPTION = Model.FormInput.Description;
                            controlPoint.TAGID = Model.FormInput.TagID;
                            controlPoint.REMARK = Model.FormInput.Remark;
                            controlPoint.ISFEELITEMDEFAULTNORMAL = Model.FormInput.IsFeelItemDefaultNormal ? "Y" : "N";
                            controlPoint.LASTMODIFYTIME = DateTime.Now;

                            db.SaveChanges();
                            #endregion

                            #region ControlPointCheckItem
                            #region Delete
                            db.CONTROLPOINTCHECKITEM.RemoveRange(db.CONTROLPOINTCHECKITEM.Where(x => x.CONTROLPOINTUNIQUEID == Model.UniqueID).ToList());

                            db.SaveChanges();
                            #endregion

                            #region Insert
                            db.CONTROLPOINTCHECKITEM.AddRange(Model.CheckItemList.Select(x => new CONTROLPOINTCHECKITEM
                            {
                                CONTROLPOINTUNIQUEID = controlPoint.UNIQUEID,
                                CHECKITEMUNIQUEID = x.UniqueID,
                                ISINHERIT = x.IsInherit ? "Y" : "N",
                                LOWERLIMIT = x.LowerLimit.HasValue ? decimal.Parse(x.LowerLimit.Value.ToString()) : default(decimal?),
                                LOWERALERTLIMIT = x.LowerAlertLimit.HasValue ? decimal.Parse(x.LowerAlertLimit.Value.ToString()) : default(decimal?),
                                UPPERALERTLIMIT = x.UpperAlertLimit.HasValue ? decimal.Parse(x.UpperAlertLimit.Value.ToString()) : default(decimal?),
                                UPPERLIMIT = x.UpperLimit.HasValue ? decimal.Parse(x.UpperLimit.Value.ToString()) : default(decimal?),
                                ACCUMULATIONBASE = x.AccumulationBase.HasValue ? decimal.Parse(x.AccumulationBase.Value.ToString()) : default(decimal?),
                                UNIT = x.Unit,
                                REMARK = x.Remark
                            }).ToList());

                            db.SaveChanges();
                            #endregion
                            #endregion
#if !DEBUG
                        trans.Complete();
                    }
#endif
                            result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.ControlPoint, Resources.Resource.Success));
                        }
                    }
                    else
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.ControlPointID, Resources.Resource.Exists));
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

        public static RequestResult SavePageState(List<CheckItemModel> CheckItemList, List<string> PageStateList)
        {
            RequestResult result = new RequestResult();

            try
            {
                foreach (string pageState in PageStateList)
                {
                    string[] temp = pageState.Split(Define.Seperators, StringSplitOptions.None);

                    string isInherit = temp[0];
                    string checkItemUniqueID = temp[1];
                    string lowerLimit = temp[2];
                    string lowerAlertLimit = temp[3];
                    string upperAlertLimit = temp[4];
                    string upperLimit = temp[5];
                    string accumulationBase = temp[6];
                    string unit = temp[7];
                    string remark = temp[8];

                    var checkItem = CheckItemList.First(x => x.UniqueID == checkItemUniqueID);

                    checkItem.IsInherit = isInherit == "Y";

                    if (!checkItem.IsInherit)
                    {
                        checkItem.LowerLimit = !string.IsNullOrEmpty(lowerLimit) ? double.Parse(lowerLimit) : default(double?);
                        checkItem.LowerAlertLimit = !string.IsNullOrEmpty(lowerAlertLimit) ? double.Parse(lowerAlertLimit) : default(double?);
                        checkItem.UpperAlertLimit = !string.IsNullOrEmpty(upperAlertLimit) ? double.Parse(upperAlertLimit) : default(double?);
                        checkItem.UpperLimit = !string.IsNullOrEmpty(upperLimit) ? double.Parse(upperLimit) : default(double?);
                        checkItem.AccumulationBase = !string.IsNullOrEmpty(accumulationBase) ? double.Parse(accumulationBase) : default(double?);
                        checkItem.Unit = unit;
                        checkItem.Remark = remark;
                    }
                    else
                    {
                        checkItem.LowerLimit = null;
                        checkItem.LowerAlertLimit = null;
                        checkItem.UpperAlertLimit = null;
                        checkItem.UpperLimit = null;
                        checkItem.AccumulationBase = null;
                        checkItem.Unit = string.Empty;
                        checkItem.Remark = string.Empty;
                    }
                }

                result.ReturnData(CheckItemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult Delete(List<string> SelectedList)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    DeleteHelper.ControlPoint(db, SelectedList);

                    db.SaveChanges();
                }

                result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Delete, Resources.Resource.ControlPoint, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult AddCheckItem(List<CheckItemModel> CheckItemList, List<string> SelectedList, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    foreach (string selected in SelectedList)
                    {
                        string[] temp = selected.Split(Define.Seperators, StringSplitOptions.None);

                        var organizationUniqueID = temp[0];
                        var checkType = temp[1];
                        var checkItemUniqueID = temp[2];

                        if (!string.IsNullOrEmpty(checkItemUniqueID))
                        {
                            if (!CheckItemList.Any(x => x.UniqueID == checkItemUniqueID))
                            {
                                var checkItem = db.CHECKITEM.First(x => x.UNIQUEID == checkItemUniqueID);

                                CheckItemList.Add(new CheckItemModel()
                                {
                                    UniqueID = checkItem.UNIQUEID,
                                    CheckType = checkItem.CHECKTYPE,
                                    ID = checkItem.ID,
                                    Description = checkItem.DESCRIPTION,
                                    IsFeelItem = checkItem.ISFEELITEM=="Y",
                                    IsAccumulation = checkItem.ISACCUMULATION=="Y",
                                    IsInherit = true,
                                    OriLowerLimit = checkItem.LOWERLIMIT.HasValue?double.Parse(checkItem.LOWERLIMIT.Value.ToString()):default(double?),
                                    OriLowerAlertLimit = checkItem.LOWERALERTLIMIT.HasValue ? double.Parse(checkItem.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                                    OriUpperAlertLimit = checkItem.UPPERALERTLIMIT.HasValue ? double.Parse(checkItem.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                                    OriUpperLimit = checkItem.UPPERLIMIT.HasValue ? double.Parse(checkItem.UPPERLIMIT.Value.ToString()) : default(double?),
                                    OriAccumulationBase = checkItem.ACCUMULATIONBASE.HasValue ? double.Parse(checkItem.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                                    OriUnit = checkItem.UNIT,
                                    OriRemark = checkItem.REMARK
                                });
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(checkType))
                            {
                                var checkItemList = db.CHECKITEM.Where(x => x.ORGANIZATIONUNIQUEID == organizationUniqueID && x.CHECKTYPE == checkType).ToList();

                                foreach (var checkItem in checkItemList)
                                {
                                    if (!CheckItemList.Any(x => x.UniqueID == checkItem.UNIQUEID))
                                    {
                                        CheckItemList.Add(new CheckItemModel()
                                        {
                                            UniqueID = checkItem.UNIQUEID,
                                            CheckType = checkItem.CHECKTYPE,
                                            ID = checkItem.ID,
                                            Description = checkItem.DESCRIPTION,
                                            IsFeelItem = checkItem.ISFEELITEM == "Y",
                                            IsAccumulation = checkItem.ISACCUMULATION == "Y",
                                            IsInherit = true,
                                            OriLowerLimit = checkItem.LOWERLIMIT.HasValue ? double.Parse(checkItem.LOWERLIMIT.Value.ToString()) : default(double?),
                                            OriLowerAlertLimit = checkItem.LOWERALERTLIMIT.HasValue ? double.Parse(checkItem.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                                            OriUpperAlertLimit = checkItem.UPPERALERTLIMIT.HasValue ? double.Parse(checkItem.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                                            OriUpperLimit = checkItem.UPPERLIMIT.HasValue ? double.Parse(checkItem.UPPERLIMIT.Value.ToString()) : default(double?),
                                            OriAccumulationBase = checkItem.ACCUMULATIONBASE.HasValue ? double.Parse(checkItem.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                                            OriUnit = checkItem.UNIT,
                                            OriRemark = checkItem.REMARK
                                        });
                                    }
                                }
                            }
                            else
                            {
                                var availableOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(RefOrganizationUniqueID, true);

                                var organizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organizationUniqueID, true);

                                foreach (var organization in organizationList)
                                {
                                    if (availableOrganizationList.Any(x => x == organization))
                                    {
                                        var checkItemList = db.CHECKITEM.Where(x => x.ORGANIZATIONUNIQUEID == organization).ToList();

                                        foreach (var checkItem in checkItemList)
                                        {
                                            if (!CheckItemList.Any(x => x.UniqueID == checkItem.UNIQUEID))
                                            {
                                                CheckItemList.Add(new CheckItemModel()
                                                {
                                                    UniqueID = checkItem.UNIQUEID,
                                                    CheckType = checkItem.CHECKTYPE,
                                                    ID = checkItem.ID,
                                                    Description = checkItem.DESCRIPTION,
                                                    IsFeelItem = checkItem.ISFEELITEM == "Y",
                                                    IsAccumulation = checkItem.ISACCUMULATION == "Y",
                                                    IsInherit = true,
                                                    OriLowerLimit = checkItem.LOWERLIMIT.HasValue ? double.Parse(checkItem.LOWERLIMIT.Value.ToString()) : default(double?),
                                                    OriLowerAlertLimit = checkItem.LOWERALERTLIMIT.HasValue ? double.Parse(checkItem.LOWERALERTLIMIT.Value.ToString()) : default(double?),
                                                    OriUpperAlertLimit = checkItem.UPPERALERTLIMIT.HasValue ? double.Parse(checkItem.UPPERALERTLIMIT.Value.ToString()) : default(double?),
                                                    OriUpperLimit = checkItem.UPPERLIMIT.HasValue ? double.Parse(checkItem.UPPERLIMIT.Value.ToString()) : default(double?),
                                                    OriAccumulationBase = checkItem.ACCUMULATIONBASE.HasValue ? double.Parse(checkItem.ACCUMULATIONBASE.Value.ToString()) : default(double?),
                                                    OriUnit = checkItem.UNIT,
                                                    OriRemark = checkItem.REMARK
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                CheckItemList = CheckItemList.OrderBy(x => x.CheckType).ThenBy(x => x.ID).ToList();

                result.ReturnData(CheckItemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult GetTreeItem(List<Models.Shared.Organization> OrganizationList, string OrganizationUniqueID, Account Account)
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
                    { Define.EnumTreeAttribute.ControlPointUniqueID, string.Empty }
                };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (Account.QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
                    {
                        var controlPointList = db.CONTROLPOINT.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                        foreach (var controlPoint in controlPointList)
                        {
                            var treeItem = new TreeItem() { Title = controlPoint.DESCRIPTION };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.ControlPoint.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", controlPoint.ID, controlPoint.DESCRIPTION);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                            attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = controlPoint.UNIQUEID;

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
                        attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = string.Empty;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                            ||
                            (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.CONTROLPOINT.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID)))
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

        public static RequestResult GetRootTreeItem(List<Models.Shared.Organization> OrganizationList, string OrganizationUniqueID, Account Account)
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
                    { Define.EnumTreeAttribute.ControlPointUniqueID, string.Empty }
                };

                var organization = OrganizationList.First(x => x.UniqueID == OrganizationUniqueID);

                var treeItem = new TreeItem() { Title = organization.Description };

                attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = string.Empty;

                foreach (var attribute in attributes)
                {
                    treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                }

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (OrganizationList.Any(x => x.ParentUniqueID == organization.UniqueID && Account.VisibleOrganizationUniqueIDList.Contains(x.UniqueID))
                        ||
                        (Account.QueryableOrganizationUniqueIDList.Contains(organization.UniqueID) && db.CONTROLPOINT.Any(x => x.ORGANIZATIONUNIQUEID == organization.UniqueID)))
                    {
                        treeItem.State = "closed";
                    }
                }

                treeItemList.Add(treeItem);

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

        public static RequestResult GetTreeItem(List<Models.Shared.Organization> OrganizationList, string RefOrganizationUniqueID, string OrganizationUniqueID)
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
                    { Define.EnumTreeAttribute.ControlPointUniqueID, string.Empty }
                };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var controlPointList = db.CONTROLPOINT.Where(x => x.ORGANIZATIONUNIQUEID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                    foreach (var controlPoint in controlPointList)
                    {
                        var treeItem = new TreeItem() { Title = string.Format("{0}/{1}", controlPoint.ID, controlPoint.DESCRIPTION) };

                        attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.ControlPoint.ToString();
                        attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", controlPoint.ID, controlPoint.DESCRIPTION);
                        attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = OrganizationUniqueID;
                        attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = controlPoint.UNIQUEID;

                        foreach (var attribute in attributes)
                        {
                            treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                        }

                        treeItemList.Add(treeItem);
                    }

                    var availableOrganizationList = OrganizationDataAccessor.GetUserOrganizationPermissionList(RefOrganizationUniqueID).Select(x => x.UniqueID).ToList();

                    var organizationList = OrganizationList.Where(x => x.ParentUniqueID == OrganizationUniqueID).OrderBy(x => x.ID).ToList();

                    foreach (var organization in organizationList)
                    {
                        var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(organization.UniqueID, true);

                        if (db.CONTROLPOINT.Any(x => downStreamOrganizationList.Contains(x.ORGANIZATIONUNIQUEID) && availableOrganizationList.Contains(x.ORGANIZATIONUNIQUEID)))
                        {
                            var treeItem = new TreeItem() { Title = organization.Description };

                            attributes[Define.EnumTreeAttribute.NodeType] = Define.EnumTreeNodeType.Organization.ToString();
                            attributes[Define.EnumTreeAttribute.ToolTip] = string.Format("{0}/{1}", organization.ID, organization.Description);
                            attributes[Define.EnumTreeAttribute.OrganizationUniqueID] = organization.UniqueID;
                            attributes[Define.EnumTreeAttribute.ControlPointUniqueID] = string.Empty;

                            foreach (var attribute in attributes)
                            {
                                treeItem.Attributes[attribute.Key.ToString()] = attribute.Value;
                            }

                            treeItem.State = "closed";

                            treeItemList.Add(treeItem);
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
