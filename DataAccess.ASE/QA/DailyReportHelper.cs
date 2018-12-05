using DbEntity.ASE;
using Models.ASE.QA.DailyReport;
using Models.Authenticated;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace DataAccess.ASE.QA
{
    public class DailyReportHelper
    {
        public static RequestResult GetQueryFormModel(List<Models.Shared.Organization> OrganizationList)
        {
            RequestResult result = new RequestResult();

            try
            {
                var factoryList = OrganizationDataAccessor.GetFactoryList(OrganizationList);

                var model = new QueryFormModel()
                {
                    FactorySelectItemList = new List<SelectListItem>() {
                     Define.DefaultSelectListItem(Resources.Resource.SelectAll)
                    }
                };

                foreach (var factory in factoryList)
                {
                    model.FactorySelectItemList.Add(new SelectListItem()
                    {
                        Text = factory.Description,
                        Value = factory.UniqueID
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

        public static RequestResult Query(List<Models.Shared.Organization> OrganizationList, QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new GridViewModel()
                {
                    BeginDate = Parameters.BeginDate,
                    EndDate = Parameters.EndDate
                };

                var qa = Account.UserAuthGroupList.Contains("QA-Verify") || Account.UserAuthGroupList.Contains("QA") || Account.UserAuthGroupList.Contains("QA-FullQuery");

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query1 = (from form in db.QA_CALIBRATIONFORM
                                  join apply in db.QA_CALIBRATIONAPPLY
                                  on form.APPLYUNIQUEID equals apply.UNIQUEID
                                  join equipment in db.QA_EQUIPMENT
                                  on apply.EQUIPMENTUNIQUEID equals equipment.UNIQUEID
                                  join ichi in db.QA_ICHI
                                  on equipment.ICHIUNIQUEID equals ichi.UNIQUEID into tmpIchi
                                  from ichi in tmpIchi.DefaultIfEmpty()
                                  join organization in db.ORGANIZATION
                                  on equipment.ORGANIZATIONUNIQUEID equals organization.UNIQUEID
                                  join calibrator in db.ACCOUNT
                                  on form.CALUSERID equals calibrator.ID into tmpCalibrator
                                  from calibrator in tmpCalibrator.DefaultIfEmpty()
                                  join responsor in db.ACCOUNT
                                  on form.CALRESPONSORID equals responsor.ID into tmpResponsor
                                  from responsor in tmpResponsor.DefaultIfEmpty()
                                  join owner in db.ACCOUNT
                             on equipment.OWNERID equals owner.ID into tmpOwner
                                  from owner in tmpOwner.DefaultIfEmpty()
                                  join lab in db.QA_LAB
                                  on form.LABUNIQUEID equals lab.UNIQUEID into tmpLab
                                  from lab in tmpLab.DefaultIfEmpty()
                                  join jobCalibrator in db.ACCOUNT
                                  on form.JOBCALRESPONSERID equals jobCalibrator.ID into tmpJobCalibrator
                                  from jobCalibrator in tmpJobCalibrator.DefaultIfEmpty()
                                  select new
                                  {
                                      UniqueID = form.UNIQUEID,
                                      CalibrationApplyUniqueID = apply.UNIQUEID,
                                      VHNO = form.VHNO,
                                      Status = form.STATUS,
                                      OrganizationUniqueID = equipment.ORGANIZATIONUNIQUEID,
                                      OrganizationDescription = organization.DESCRIPTION,
                                      EstCalibrateDate = form.ESTCALDATE,
                                      CalibrateDate = form.CALDATE,
                                      ResponsorID = form.CALRESPONSORID,
                                      ResponsorName = responsor != null ? responsor.NAME : "",
                                      CalibratorID = form.CALUSERID,
                                      CalibratorName = calibrator != null ? calibrator.NAME : "",
                                      LabDescription = lab != null ? lab.DESCRIPTION : "",
                                      NotifyTime = form.NOTIFYDATE,
                                      TakeJobTime = form.TAKEJOBDATE,
                                      OwnerID = equipment.OWNERID,
                                      OwnerName = owner != null ? owner.NAME : "",
                                      OwnerManagerID = equipment.OWNERMANAGERID,
                                      PEID = equipment.PEID,
                                      PEManagerID = equipment.PEMANAGERID,
                                      JobCalibratorID = form.JOBCALRESPONSERID,
                                      JobCalibratorName = jobCalibrator != null ? jobCalibrator.NAME : "",
                                      CalNo = equipment.CALNO,
                                      SerialNo = equipment.SERIALNO,
                                      IchiUniqueID = equipment.ICHIUNIQUEID,
                                      IchiName = ichi != null ? ichi.NAME : "",
                                      IchiRemark = equipment.ICHIREMARK,
                                      MachineNo = equipment.MACHINENO,
                                      Brand = equipment.BRAND,
                                      Model = equipment.MODEL,
                                      IsAbnormal = form.HAVEABNORMAL == "Y",
                                      CalibrateType = equipment.CALTYPE,
                                      CalibrateUnit = equipment.CALUNIT,
                                      IsQRCoded = form.ISQRCODE == "Y"
                                  }).AsQueryable();

                    var query2 = (from form in db.QA_CALIBRATIONFORM
                                  join notify in db.QA_CALIBRATIONNOTIFY
                                  on form.NOTIFYUNIQUEID equals notify.UNIQUEID
                                  join equipment in db.QA_EQUIPMENT
                                  on notify.EQUIPMENTUNIQUEID equals equipment.UNIQUEID
                                  join ichi in db.QA_ICHI
                                  on equipment.ICHIUNIQUEID equals ichi.UNIQUEID into tmpIchi
                                  from ichi in tmpIchi.DefaultIfEmpty()
                                  join organization in db.ORGANIZATION
                                 on equipment.ORGANIZATIONUNIQUEID equals organization.UNIQUEID
                                  join calibrator in db.ACCOUNT
                                   on form.CALUSERID equals calibrator.ID into tmpCalibrator
                                  from calibrator in tmpCalibrator.DefaultIfEmpty()
                                  join responsor in db.ACCOUNT
                                  on form.CALRESPONSORID equals responsor.ID into tmpResponsor
                                  from responsor in tmpResponsor.DefaultIfEmpty()
                                  join owner in db.ACCOUNT
                                on equipment.OWNERID equals owner.ID into tmpOwner
                                  from owner in tmpOwner.DefaultIfEmpty()
                                  join lab in db.QA_LAB
                                  on form.LABUNIQUEID equals lab.UNIQUEID into tmpLab
                                  from lab in tmpLab.DefaultIfEmpty()
                                  join jobCalibrator in db.ACCOUNT
                                  on form.JOBCALRESPONSERID equals jobCalibrator.ID into tmpJobCalibrator
                                  from jobCalibrator in tmpJobCalibrator.DefaultIfEmpty()
                                  select new
                                  {

                                      UniqueID = form.UNIQUEID,
                                      CalibrationApplyUniqueID = "",
                                      VHNO = form.VHNO,
                                      Status = form.STATUS,
                                      OrganizationUniqueID = equipment.ORGANIZATIONUNIQUEID,
                                      OrganizationDescription = organization.DESCRIPTION,
                                      EstCalibrateDate = form.ESTCALDATE,
                                      CalibrateDate = form.CALDATE,
                                      ResponsorID = form.CALRESPONSORID,
                                      ResponsorName = responsor != null ? responsor.NAME : "",
                                      CalibratorID = form.CALUSERID,
                                      CalibratorName = calibrator != null ? calibrator.NAME : "",
                                      LabDescription = lab != null ? lab.DESCRIPTION : "",
                                      NotifyTime = form.NOTIFYDATE,
                                      TakeJobTime = form.TAKEJOBDATE,
                                      OwnerID = equipment.OWNERID,
                                      OwnerName = owner!=null?owner.NAME:"",
                                      OwnerManagerID = equipment.OWNERMANAGERID,
                                      PEID = equipment.PEID,
                                      PEManagerID = equipment.PEMANAGERID,
                                      JobCalibratorID = form.JOBCALRESPONSERID,
                                      JobCalibratorName = jobCalibrator != null ? jobCalibrator.NAME : "",
                                      CalNo = equipment.CALNO,
                                      SerialNo = equipment.SERIALNO,
                                      IchiUniqueID = equipment.ICHIUNIQUEID,
                                      IchiName = ichi != null ? ichi.NAME : "",
                                      IchiRemark = equipment.ICHIREMARK,
                                      MachineNo = equipment.MACHINENO,
                                      Brand = equipment.BRAND,
                                      Model = equipment.MODEL,
                                      IsAbnormal = form.HAVEABNORMAL == "Y",
                                      CalibrateType = equipment.CALTYPE,
                                      CalibrateUnit = equipment.CALUNIT,
                                      IsQRCoded = form.ISQRCODE == "Y"
                                  }).AsQueryable();

                    var queryResults = query1.Union(query2);

                    if (!qa)
                    {
                        queryResults = queryResults.Where(x => Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID) || x.JobCalibratorID == Account.ID || x.CalibratorID == Account.ID || x.OwnerID == Account.ID || x.OwnerManagerID == Account.ID || x.PEID == Account.ID || x.PEManagerID == Account.ID).AsQueryable();
                    }

                    queryResults = queryResults.Where(x => DateTime.Compare(x.EstCalibrateDate, Parameters.BeginDate) >= 0);

                    queryResults = queryResults.Where(x => DateTime.Compare(x.EstCalibrateDate, Parameters.EndDate) < 0);

                    if (!string.IsNullOrEmpty(Parameters.SerialNo))
                    {
                        queryResults = queryResults.Where(x => x.SerialNo.Contains(Parameters.SerialNo));
                    }

                    if (!string.IsNullOrEmpty(Parameters.CalNo))
                    {
                        queryResults = queryResults.Where(x => x.CalNo.Contains(Parameters.CalNo));
                    }

                    if (!string.IsNullOrEmpty(Parameters.FactoryUniqueID))
                    {
                        var downStream = OrganizationDataAccessor.GetDownStreamOrganizationList(OrganizationList, Parameters.FactoryUniqueID, true);

                        queryResults = queryResults.Where(x => downStream.Contains(x.OrganizationUniqueID));
                    }

                    if (!string.IsNullOrEmpty(Parameters.IchiName))
                    {
                        queryResults = queryResults.Where(x => x.IchiName.Contains(Parameters.IchiName) || x.IchiRemark.Contains(Parameters.IchiName));
                    }

                    if (!string.IsNullOrEmpty(Parameters.Brand))
                    {
                        queryResults = queryResults.Where(x => x.Brand.Contains(Parameters.Brand));
                    }

                    if (!string.IsNullOrEmpty(Parameters.Model))
                    {
                        queryResults = queryResults.Where(x => x.Model.Contains(Parameters.Model));
                    }

                    var itemList = queryResults.ToList();

                    foreach (var item in itemList)
                    {
                        var log = db.QA_CALIBRATIONFORMSTEPLOG.Where(x => x.FORMUNIQUEID == item.UniqueID).OrderByDescending(x => x.SEQ).FirstOrDefault();

                        model.CalItemList.Add(new Models.ASE.QA.CALnMSAReport.CalGridItem()
                        {
                            UniqueID = item.UniqueID,
                            CalibrationApplyUniqueID = item.CalibrationApplyUniqueID,
                            VHNO = item.VHNO,
                            Status = new Models.ASE.QA.CALnMSAReport.CalFormStatus(item.Status, item.CalibrateType, item.EstCalibrateDate, log != null ? log.STEP : string.Empty),
                            CalibrateType = item.CalibrateType,
                            CalibrateUnit = item.CalibrateUnit,
                            Factory = OrganizationDataAccessor.GetFactory(OrganizationList, item.OrganizationUniqueID),
                            OrganizationDescription = item.OrganizationDescription,
                            CalNo = item.CalNo,
                            IsAbnormal = item.IsAbnormal,
                            SerialNo = item.SerialNo,
                            MachineNo = item.MachineNo,
                            IchiUniqueID = item.IchiUniqueID,
                            IchiName = item.IchiName,
                            IchiRemark = item.IchiRemark,
                            Brand = item.Brand,
                            Model = item.Model,
                            CalibrateDate = item.CalibrateDate,
                            EstCalibrateDate = item.EstCalibrateDate,
                            CalibratorID = item.CalibratorID,
                            CalibratorName = item.CalibratorName,
                            OwnerID = item.OwnerID,
                            OwnerName = item.OwnerName,
                            ResponsorID = item.ResponsorID,
                            ResponsorName = item.ResponsorName,
                            LabDescription = item.LabDescription
                        });
                    }

                    var query = (from x in db.QA_MSAFORM
                                 join e in db.QA_EQUIPMENT
                                 on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                 join pe in db.ACCOUNT
                                 on e.PEID equals pe.ID into tmpPE
                                 from pe in tmpPE.DefaultIfEmpty()
                                 join i in db.QA_ICHI on e.ICHIUNIQUEID equals i.UNIQUEID into tmpIchi
                                 from i in tmpIchi.DefaultIfEmpty()
                                 join n in db.QA_MSANOTIFY
                                 on x.EQUIPMENTUNIQUEID equals n.EQUIPMENTUNIQUEID into tmpNotify
                                 from n in tmpNotify.DefaultIfEmpty()
                                 select new
                                 {
                                     UniqueID = x.UNIQUEID,
                                     x.VHNO,
                                     x.TYPE,
                                     x.SUBTYPE,
                                     Status = x.STATUS,
                                     CalNo = e.MSACALNO,
                                     OrganizationUniqueID = e.ORGANIZATIONUNIQUEID,
                                     MSAResponsorID = x.MSARESPONSORID,
                                     OwnerID = e.OWNERID,
                                     OwnerManagerID = e.OWNERMANAGERID,
                                     PEID = e.PEID,
                                     PEName = pe!=null?pe.NAME:"",
                                     PEManagerID = e.PEMANAGERID,
                                     EstMSADate = x.ESTMSADATE,
                                     MSADate = x.MSADATE,
                                     CreateTime = x.CREATETIME,
                                     Station = x.STATION,
                                     MSAIchi = x.MSAICHI,
                                     Characteristic = x.CHARACTERISITIC,
                                     LowerRange = x.LOWERRANGE,
                                     UpperRange = x.UPPERRANGE,
                                     Model = e.MODEL,
                                     Brand = e.BRAND,
                                     IchiRemark = e.ICHIREMARK,
                                     IchiName = i != null ? i.NAME : "",
                                     IsNew = n == null
                                 }).AsQueryable();

                    if (!qa)
                    {
                        query = query.Where(x => Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID) || x.MSAResponsorID == Account.ID || x.OwnerID == Account.ID || x.OwnerManagerID == Account.ID || x.PEID == Account.ID || x.PEManagerID == Account.ID).AsQueryable();
                    }

                    query = query.Where(x => DateTime.Compare(x.EstMSADate.Value, Parameters.BeginDate) >= 0);

                    query = query.Where(x => DateTime.Compare(x.EstMSADate.Value, Parameters.EndDate) < 0);

                    if (!string.IsNullOrEmpty(Parameters.CalNo))
                    {
                        query = query.Where(x => x.CalNo.Contains(Parameters.CalNo));
                    }

                    if (!string.IsNullOrEmpty(Parameters.FactoryUniqueID))
                    {
                        var downStream = OrganizationDataAccessor.GetDownStreamOrganizationList(OrganizationList, Parameters.FactoryUniqueID, true);

                        query = query.Where(x => downStream.Contains(x.OrganizationUniqueID));
                    }

                    if (!string.IsNullOrEmpty(Parameters.IchiName))
                    {
                        query = query.Where(x => x.IchiName.Contains(Parameters.IchiName) || x.IchiRemark.Contains(Parameters.IchiName));
                    }

                    if (!string.IsNullOrEmpty(Parameters.Brand))
                    {
                        query = query.Where(x => x.Brand.Contains(Parameters.Brand));
                    }

                    if (!string.IsNullOrEmpty(Parameters.Model))
                    {
                        query = query.Where(x => x.Model.Contains(Parameters.Model));
                    }

                    var temp = query.OrderByDescending(x => x.VHNO).ToList();

                    foreach (var t in temp)
                    {
                        var msaReponsor = db.ACCOUNT.FirstOrDefault(x => x.ID == t.MSAResponsorID);

                        model.MSAItemList.Add(new Models.ASE.QA.CALnMSAReport.MSAGridItem()
                        {
                            UniqueID = t.UniqueID,
                            VHNO = t.VHNO,
                            Type = t.TYPE,
                            SubType = t.SUBTYPE,
                            Status = new Models.ASE.QA.CALnMSAReport.MSAFormStatus(t.Status, t.EstMSADate.Value),
                            Factory = OrganizationDataAccessor.GetFactory(OrganizationList, t.OrganizationUniqueID),
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(t.OrganizationUniqueID),
                            CalNo = t.CalNo,
                            MSAIchi = t.MSAIchi,
                            MSACharacteristic = t.Characteristic,
                            MSALowerRange = t.LowerRange,
                            MSAUpperRange = t.UpperRange,
                            Station = t.Station,
                            EstMSADate = t.EstMSADate.Value,
                            MSADate = t.MSADate,
                            PEID=t.PEID,
                            PEName=t.PEName,
                            MSAResponsorID = t.MSAResponsorID,
                            MSAResponsorName = msaReponsor != null ? msaReponsor.NAME : string.Empty,
                            CreateTime = t.CreateTime.Value,
                            IsNew = t.IsNew
                        });
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

        public static RequestResult Export(GridViewModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                IWorkbook workBook = new XSSFWorkbook();

                #region Header Style
                var headerFont = workBook.CreateFont();
                headerFont.Color = HSSFColor.Black.Index;
                headerFont.Boldweight = (short)FontBoldWeight.Bold;
                headerFont.FontHeightInPoints = 12;

                var headerStyle = workBook.CreateCellStyle();
                headerStyle.FillForegroundColor = HSSFColor.Grey25Percent.Index;
                headerStyle.FillPattern = FillPattern.SolidForeground;
                headerStyle.BorderTop = BorderStyle.Thin;
                headerStyle.BorderBottom = BorderStyle.Thin;
                headerStyle.BorderLeft = BorderStyle.Thin;
                headerStyle.BorderRight = BorderStyle.Thin;
                headerStyle.SetFont(headerFont);
                #endregion

                #region Cell Style
                var cellFont = workBook.CreateFont();
                cellFont.Color = HSSFColor.Black.Index;
                cellFont.Boldweight = (short)FontBoldWeight.Normal;
                cellFont.FontHeightInPoints = 12;

                var cellStyle = workBook.CreateCellStyle();
                cellStyle.BorderTop = BorderStyle.Thin;
                cellStyle.BorderBottom = BorderStyle.Thin;
                cellStyle.BorderLeft = BorderStyle.Thin;
                cellStyle.BorderRight = BorderStyle.Thin;
                cellStyle.SetFont(cellFont);
                #endregion

                IRow row;

                ICell cell;

                var rowIndex = 0;

                if (Model.CalItemList != null && Model.CalItemList.Count > 0)
                {
                    var sheetCALSummary = workBook.CreateSheet("CAL-WEEKLY");

                    #region Row 0
                    row = sheetCALSummary.CreateRow(0);

                    cell = row.CreateCell(0);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Weekly");

                    var cellIndex = 1;

                    foreach (var key in Model.KeyList)
                    {
                        cell = row.CreateCell(cellIndex);
                        cell.CellStyle = headerStyle;
                        cell.SetCellValue(key.Display);

                        cellIndex++;
                    }
                    #endregion

                    #region Row 1
                    row = sheetCALSummary.CreateRow(1);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("Total Cal Q'ty");

                    cellIndex = 1;

                    foreach (var key in Model.KeyList)
                    {
                        cell = row.CreateCell(cellIndex);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(Model.CalItemList.Count(x => DateTime.Compare(x.EstCalibrateDate, key.Date) >= 0 && DateTime.Compare(x.EstCalibrateDate, key.Date) <= 0));

                        cellIndex++;
                    }
                    #endregion

                    #region Row 2
                    row = sheetCALSummary.CreateRow(2);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("Abnormal Q'ty");

                    cellIndex = 1;

                    foreach (var key in Model.KeyList)
                    {
                        cell = row.CreateCell(cellIndex);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(Model.CalItemList.Count(x => DateTime.Compare(x.EstCalibrateDate, key.Date) >= 0 && DateTime.Compare(x.EstCalibrateDate, key.Date) <= 0 && x.IsAbnormal));

                        cellIndex++;
                    }
                    #endregion

                    #region Row 3
                    row = sheetCALSummary.CreateRow(3);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("New Q'ty");

                    cellIndex = 1;

                    foreach (var key in Model.KeyList)
                    {
                        cell = row.CreateCell(cellIndex);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(Model.CalItemList.Count(x => DateTime.Compare(x.EstCalibrateDate, key.Date) >= 0 && DateTime.Compare(x.EstCalibrateDate, key.Date) <= 0 && !string.IsNullOrEmpty(x.CalibrationApplyUniqueID)));

                        cellIndex++;
                    }
                    #endregion

                    #region Row 4
                    row = sheetCALSummary.CreateRow(4);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("Complete Rate%");

                    cellIndex = 1;

                    foreach (var key in Model.KeyList)
                    {
                        cell = row.CreateCell(cellIndex);
                        cell.CellStyle = cellStyle;

                        double total = Model.CalItemList.Count(x => DateTime.Compare(x.EstCalibrateDate, key.Date) >= 0 && DateTime.Compare(x.EstCalibrateDate, key.Date) <= 0);
                        double finished = Model.CalItemList.Count(x => DateTime.Compare(x.EstCalibrateDate, key.Date) >= 0 && DateTime.Compare(x.EstCalibrateDate, key.Date) <= 0 && x.Status._Status == "5");

                        cell.SetCellValue((finished / total).ToString("P"));

                        cellIndex++;
                    }
                    #endregion

                    var sheetCALDetail = workBook.CreateSheet("CAL-HISTORY");

                    #region Row 0
                    row = sheetCALDetail.CreateRow(0);

                    cell = row.CreateCell(0);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("單號");

                    cell = row.CreateCell(1);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("狀態");

                    cell = row.CreateCell(2);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("校驗類別");

                    cell = row.CreateCell(3);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("負責單位");

                    cell = row.CreateCell(4);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("廠別");

                    cell = row.CreateCell(5);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("部門");

                    cell = row.CreateCell(6);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("儀校編號");

                    cell = row.CreateCell(7);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("預計校驗日期");

                    cell = row.CreateCell(8);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("實際校驗日期");

                    cell = row.CreateCell(9);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("序號");

                    cell = row.CreateCell(10);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("機台編號");

                    cell = row.CreateCell(11);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("儀器名稱");

                    cell = row.CreateCell(12);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("廠牌");

                    cell = row.CreateCell(13);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("型號");

                    cell = row.CreateCell(14);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("設備負責人");

                    cell = row.CreateCell(15);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("校驗負責人員");

                    cell = row.CreateCell(16);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("校驗人員");
                    #endregion

                    rowIndex = 1;

                    foreach (var item in Model.CalItemList)
                    {
                        row = sheetCALDetail.CreateRow(rowIndex);

                        cell = row.CreateCell(0);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.VHNO);

                        cell = row.CreateCell(1);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.Status.Display);

                        cell = row.CreateCell(2);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.CalibrateTypeDisplay);

                        cell = row.CreateCell(3);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.CalibrateUnitDisplay);

                        cell = row.CreateCell(4);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.Factory);

                        cell = row.CreateCell(5);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.OrganizationDescription);

                        cell = row.CreateCell(6);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.CalNo);

                        cell = row.CreateCell(7);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.EstCalibrateDateString);

                        cell = row.CreateCell(8);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.CalibrateDateString);

                        cell = row.CreateCell(9);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.SerialNo);

                        cell = row.CreateCell(10);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.MachineNo);

                        cell = row.CreateCell(11);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.Ichi);

                        cell = row.CreateCell(12);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.Brand);

                        cell = row.CreateCell(13);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.Model);

                        cell = row.CreateCell(14);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.Owner);

                        cell = row.CreateCell(15);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.Responsor);

                        cell = row.CreateCell(16);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.Calibrator);

                        rowIndex++;
                    }
                }

                if (Model.MSAItemList != null && Model.MSAItemList.Count > 0)
                {
                    var sheetMSASummary = workBook.CreateSheet("MSA-WEEKLY");

                    #region Row 0
                    row = sheetMSASummary.CreateRow(0);

                    cell = row.CreateCell(0);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("Weekly");

                    var cellIndex = 1;

                    foreach (var key in Model.KeyList)
                    {
                        cell = row.CreateCell(cellIndex);
                        cell.CellStyle = headerStyle;
                        cell.SetCellValue(key.Display);

                        cellIndex++;
                    }
                    #endregion

                    #region Row 1
                    row = sheetMSASummary.CreateRow(1);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("Total MSA Q'ty");

                    cellIndex = 1;

                    foreach (var key in Model.KeyList)
                    {
                        cell = row.CreateCell(cellIndex);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(Model.MSAItemList.Count(x => DateTime.Compare(x.EstMSADate, key.Date) >= 0 && DateTime.Compare(x.EstMSADate, key.Date) <= 0));

                        cellIndex++;
                    }
                    #endregion

                    #region Row 2
                    row = sheetMSASummary.CreateRow(2);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("NEW Q'ty");

                    cellIndex = 1;

                    foreach (var key in Model.KeyList)
                    {
                        cell = row.CreateCell(cellIndex);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(Model.MSAItemList.Count(x => DateTime.Compare(x.EstMSADate, key.Date) >= 0 && DateTime.Compare(x.EstMSADate, key.Date) <= 0 && x.IsNew));

                        cellIndex++;
                    }
                    #endregion

                    #region Row 3
                    row = sheetMSASummary.CreateRow(3);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("Completed Rate");

                    cellIndex = 1;

                    foreach (var key in Model.KeyList)
                    {
                        cell = row.CreateCell(cellIndex);
                        cell.CellStyle = cellStyle;

                        double total = Model.MSAItemList.Count(x => DateTime.Compare(x.EstMSADate, key.Date) >= 0 && DateTime.Compare(x.EstMSADate, key.Date) <= 0);
                        double finished = Model.MSAItemList.Count(x => DateTime.Compare(x.EstMSADate, key.Date) >= 0 && DateTime.Compare(x.EstMSADate, key.Date) <= 0 && x.Status.StatusCode == "5");

                        cell.SetCellValue((finished / total).ToString("P"));

                        cellIndex++;
                    }
                    #endregion

                    var sheetMSADetail = workBook.CreateSheet("MSA-HISTORY");

                    #region Row 0
                    row = sheetMSADetail.CreateRow(0);

                    cell = row.CreateCell(0);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("單號");

                    cell = row.CreateCell(1);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("儀校編號");

                    cell = row.CreateCell(2);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("狀態");

                    cell = row.CreateCell(3);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("預計MSA日期");

                    cell = row.CreateCell(4);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("實際MSA日期");

                    cell = row.CreateCell(5);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("製程負責人");

                    cell = row.CreateCell(6);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("MSA負責人");

                    cell = row.CreateCell(7);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("廠別");

                    cell = row.CreateCell(8);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("部門");

                    cell = row.CreateCell(9);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("類型");

                    cell = row.CreateCell(10);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("站別");

                    cell = row.CreateCell(11);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("儀器");

                    cell = row.CreateCell(12);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("量測特性");

                    cell = row.CreateCell(13);
                    cell.CellStyle = headerStyle;
                    cell.SetCellValue("ALL PACKAGE 量測範圍");
                    #endregion

                    rowIndex = 1;

                    foreach (var item in Model.MSAItemList)
                    {
                        row = sheetMSADetail.CreateRow(rowIndex);

                        cell = row.CreateCell(0);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.VHNO);

                        cell = row.CreateCell(1);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.CalNo);

                        cell = row.CreateCell(2);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.Status.Display);

                        cell = row.CreateCell(3);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.EstMSADateString);

                        cell = row.CreateCell(4);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.MSADateString);

                        cell = row.CreateCell(5);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.PE);

                        cell = row.CreateCell(6);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.MSAResponsor);

                        cell = row.CreateCell(7);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.Factory);

                        cell = row.CreateCell(8);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.OrganizationDescription);

                        cell = row.CreateCell(9);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.TypeDisplay);

                        cell = row.CreateCell(10);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.Station);

                        cell = row.CreateCell(11);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.MSAIchi);

                        cell = row.CreateCell(12);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.MSACharacteristic);

                        cell = row.CreateCell(13);
                        cell.CellStyle = cellStyle;
                        cell.SetCellValue(item.MSARange);

                        rowIndex++;
                    }
                }

                var output = new ExcelExportModel("Report", Define.EnumExcelVersion._2007);

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
    }
}
