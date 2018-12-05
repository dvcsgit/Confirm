using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Models;
using Models.ASE.QA.CalibrationForm;
using Models.Authenticated;
using DbEntity.ASE;
using System.Reflection;
using Utility;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;
using ZXing;
using ZXing.Common;
using NPOI.OpenXmlFormats.Spreadsheet;
using System.Web.Mvc;
using System.Transactions;
using Webdiyer.WebControls.Mvc;
using System.Net.Mail;
using Models.ASE.QA.Shared;

namespace DataAccess.ASE.QA
{
    public class CalibrationFormDataAccessor
    {
        private static string QueryString1="SELECT " +
"F.UNIQUEID UniqueID, " +
"F.VHNO, " +
"F.STATUS Status, " +
"E.ORGANIZATIONUNIQUEID OrganizationUniqueID, " +
"O.DESCRIPTION OrganizationDescription, " +
"F.ESTCALDATE EstCalibrateDate, " +
"F.CALDATE CalibrateDate, " +
"F.CALRESPONSORID ResponsorID, " +
"RESPONSOR.NAME ResponsorName, " +
"F.CALUSERID CalibratorID, " +
"CALIBRATOR.NAME CalibratorName, " +
"LAB.DESCRIPTION LabDescription, " +
"F.NOTIFYDATE NotifyTime, " +
"F.TAKEJOBDATE TakeJobTime, " +
"E.OWNERID OwnerID, " +
"E.OWNERMANAGERID OwnerManagerID, " +
"E.PEID, " +
"E.PEMANAGERID PEManageID, " +
"F.JOBCALRESPONSERID JobCaliratorID, " +
"JOBCALIBRATOR.NAME JobCaliratorName, " +
"E.CALNO CalNo, " +
"E.SERIALNO SerialNo, " +
"E.ICHIUNIQUEID IchiUniqueID, " +
"I.NAME IchiName, " +
"E.ICHIREMARK IchiRemark, " +
"E.MACHINENO MachineNo, " +
"E.BRAND Brand, " +
"E.MODEL Model, " +
"E.CALTYPE CalibrateType, " +
"E.CALUNIT CalibrateUnit, " +
"F.ISQRCODE IsQRCoded, " +
"L.SEQ LogSeq, " +
"L.STEP Step " +
"FROM  " +
"EIPC.QA_CALIBRATIONFORM F " +
"JOIN EIPC.QA_CALIBRATIONAPPLY A ON F.APPLYUNIQUEID = A.UNIQUEID " +
"JOIN EIPC.QA_EQUIPMENT E ON A.EQUIPMENTUNIQUEID = E.UNIQUEID " +
"LEFT JOIN EIPC.QA_CALIBRATIONFORMSTEPLOG L ON L.FORMUNIQUEID = F.UNIQUEID " +
"LEFT JOIN EIPC.ORGANIZATION O ON E.ORGANIZATIONUNIQUEID = O.UNIQUEID " +
"LEFT JOIN EIPC.QA_ICHI I ON E.ICHIUNIQUEID = I.UNIQUEID " +
"LEFT JOIN EIPC.ACCOUNT CALIBRATOR ON F.CALUSERID = CALIBRATOR.ID " +
"LEFT JOIN EIPC.ACCOUNT RESPONSOR ON F.CALRESPONSORID = RESPONSOR.ID " +
"LEFT JOIN EIPC.QA_LAB LAB ON F.LABUNIQUEID = LAB.UNIQUEID " +
"LEFT JOIN EIPC.ACCOUNT JOBCALIBRATOR ON F.JOBCALRESPONSERID = JOBCALIBRATOR.ID";

        private static string QueryString2="SELECT " +
"F.UNIQUEID UniqueID, " +
"F.VHNO, " +
"F.STATUS Status, " +
"E.ORGANIZATIONUNIQUEID OrganizationUniqueID, " +
"O.DESCRIPTION OrganizationDescription, " +
"F.ESTCALDATE EstCalibrateDate, " +
"F.CALDATE CalibrateDate, " +
"F.CALRESPONSORID ResponsorID, " +
"RESPONSOR.NAME ResponsorName, " +
"F.CALUSERID CalibratorID, " +
"CALIBRATOR.NAME CalibratorName, " +
"LAB.DESCRIPTION LabDescription, " +
"F.NOTIFYDATE NotifyTime, " +
"F.TAKEJOBDATE TakeJobTime, " +
"E.OWNERID OwnerID, " +
"E.OWNERMANAGERID OwnerManagerID, " +
"E.PEID, " +
"E.PEMANAGERID PEManageID, " +
"F.JOBCALRESPONSERID JobCaliratorID, " +
"JOBCALIBRATOR.NAME JobCaliratorName, " +
"E.CALNO CalNo, " +
"E.SERIALNO SerialNo, " +
"E.ICHIUNIQUEID IchiUniqueID, " +
"I.NAME IchiName, " +
"E.ICHIREMARK IchiRemark, " +
"E.MACHINENO MachineNo, " +
"E.BRAND Brand, " +
"E.MODEL Model, " +
"E.CALTYPE CalibrateType, " +
"E.CALUNIT CalibrateUnit, " +
"F.ISQRCODE IsQRCoded, " +
"L.SEQ LogSeq, " +
"L.STEP Step " +
"FROM  " +
"EIPC.QA_CALIBRATIONFORM F " +
"JOIN EIPC.QA_CALIBRATIONNOTIFY N ON F.NOTIFYUNIQUEID = N.UNIQUEID " +
"JOIN EIPC.QA_EQUIPMENT E ON N.EQUIPMENTUNIQUEID = E.UNIQUEID " +
"LEFT JOIN EIPC.QA_CALIBRATIONFORMSTEPLOG L ON L.FORMUNIQUEID = F.UNIQUEID " +
"LEFT JOIN EIPC.ORGANIZATION O ON E.ORGANIZATIONUNIQUEID = O.UNIQUEID " +
"LEFT JOIN EIPC.QA_ICHI I ON E.ICHIUNIQUEID = I.UNIQUEID " +
"LEFT JOIN EIPC.ACCOUNT CALIBRATOR ON F.CALUSERID = CALIBRATOR.ID " +
"LEFT JOIN EIPC.ACCOUNT RESPONSOR ON F.CALRESPONSORID = RESPONSOR.ID " +
"LEFT JOIN EIPC.QA_LAB LAB ON F.LABUNIQUEID = LAB.UNIQUEID " +
"LEFT JOIN EIPC.ACCOUNT JOBCALIBRATOR ON F.JOBCALRESPONSERID = JOBCALIBRATOR.ID";

        public static RequestResult GetQueryFormModel(List<Models.Shared.Organization> OrganizationList, string VHNO)
        {
            RequestResult result = new RequestResult();

            try
            {
                var factoryList = OrganizationDataAccessor.GetFactoryList(OrganizationList);

                var model = new QueryFormModel()
                {
                    FactorySelectItemList = new List<SelectListItem>() {
                     Define.DefaultSelectListItem(Resources.Resource.SelectAll)
                    },
                    Parameters = new QueryParameters()
                    {
                        VHNO = VHNO
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

        public static RequestResult GetStepLogCreateFormModel(string Step, string CalibrateUnit)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new StepLogCreateFormModel()
                {
                    CalibrateUnit = CalibrateUnit,
                    Step = Step,
                    HourSelectItemList = new List<SelectListItem>() 
                    {
                        Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                    },
                    MinuteSelectItemList = new List<SelectListItem>() 
                    {
                        Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                    },
                    QASelectItemList = new List<SelectListItem>() 
                    {
                        Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                    },
                    FormInput = new StepLogFormInput()
                    {
                        DateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today),
                        Hour = DateTime.Now.Hour.ToString().PadLeft(2, '0'),
                        Minute = DateTime.Now.Minute.ToString().PadLeft(2, '0'),
                    }
                };

                for (int i = 0; i <= 23; i++)
                {
                    var hour = i.ToString().PadLeft(2, '0');

                    model.HourSelectItemList.Add(new SelectListItem()
                    {
                        Value = hour,
                        Text = hour
                    });
                }

                for (int i = 0; i <= 59; i++)
                {
                    var min = i.ToString().PadLeft(2, '0');

                    model.MinuteSelectItemList.Add(new SelectListItem()
                    {
                        Value = min,
                        Text = min
                    });
                }

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var calibratorList = (from x in db.USERAUTHGROUP
                                          join u in db.ACCOUNT
                                          on x.USERID equals u.ID
                                          where x.AUTHGROUPID == "QA"
                                          select u).ToList();

                    foreach (var c in calibratorList)
                    {
                        var ename = string.Empty;

                        if (!string.IsNullOrEmpty(c.EMAIL))
                        {
                            try
                            {
                                ename = c.EMAIL.Substring(0, c.EMAIL.IndexOf('@'));
                            }
                            catch
                            {
                                ename = string.Empty;
                            }
                        }

                        var display = string.Empty;

                        if (!string.IsNullOrEmpty(ename))
                        {
                            display = string.Format("{0}/{1}/{2}", c.ID, c.NAME, ename);
                        }
                        else
                        {
                            display = string.Format("{0}/{1}", c.ID, c.NAME);
                        }

                        model.QASelectItemList.Add(new SelectListItem()
                        {
                            Value = c.ID,
                            Text = display
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

        public static RequestResult CreateStepLog(List<StepLogModel> StepLogList, StepLogCreateFormModel Model, string CalibrateUnit, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                int seq = 1;

                if (StepLogList.Count > 0)
                {
                    seq = StepLogList.Max(x => x.Seq) + 1;
                }

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (CalibrateUnit == "L")
                    {
                        var owner = db.ACCOUNT.FirstOrDefault(x => x.ID == Model.FormInput.OwnerID);
                        var qa = db.ACCOUNT.FirstOrDefault(x => x.ID == Model.FormInput.QAID);

                        StepLogList.Add(new StepLogModel()
                        {
                            Step = Model.Step,
                            OwnerID = Model.FormInput.OwnerID,
                            OwnerName = owner != null ? owner.NAME : "",
                            QAID = Model.FormInput.QAID,
                            QAName = qa != null ? qa.NAME : "",
                            Seq = seq,
                            Time = Model.FormInput.Time
                        });
                    }

                    if (CalibrateUnit == "F")
                    {
                        StepLogList.Add(new StepLogModel()
                        {
                            Step = Model.Step,
                            OwnerID = Account.ID,
                            OwnerName = Account.Name,
                            Seq = seq,
                            Time = Model.FormInput.Time
                        });
                    }
                }

                result.ReturnData(StepLogList.OrderBy(x => x.Seq).ToList());
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static List<GridItem> Query(List<Models.Shared.Organization> OrganizationList, string EquipmentUniqueID)
        {
            var model = new List<GridItem>();

            try
            {
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
                                  join lab in db.QA_LAB
                                  on form.LABUNIQUEID equals lab.UNIQUEID into tmpLab
                                  from lab in tmpLab.DefaultIfEmpty()
                                  join jobCalibrator in db.ACCOUNT
                                  on form.JOBCALRESPONSERID equals jobCalibrator.ID into tmpJobCalibrator
                                  from jobCalibrator in tmpJobCalibrator.DefaultIfEmpty()
                                  where apply.EQUIPMENTUNIQUEID==EquipmentUniqueID
                                  select new
                                  {
                                      UniqueID = form.UNIQUEID,
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
                                  join lab in db.QA_LAB
                                  on form.LABUNIQUEID equals lab.UNIQUEID into tmpLab
                                  from lab in tmpLab.DefaultIfEmpty()
                                  join jobCalibrator in db.ACCOUNT
                                  on form.JOBCALRESPONSERID equals jobCalibrator.ID into tmpJobCalibrator
                                  from jobCalibrator in tmpJobCalibrator.DefaultIfEmpty()
                                  where notify.EQUIPMENTUNIQUEID==EquipmentUniqueID
                                  select new
                                  {

                                      UniqueID = form.UNIQUEID,
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
                                      CalibrateType = equipment.CALTYPE,
                                      CalibrateUnit = equipment.CALUNIT,
                                      IsQRCoded = form.ISQRCODE == "Y"
                                  }).AsQueryable();

                    var query = query1.Union(query2);

                    var itemList = query.ToList();

                    foreach (var item in itemList)
                    {
                        var log = db.QA_CALIBRATIONFORMSTEPLOG.Where(x => x.FORMUNIQUEID == item.UniqueID).OrderByDescending(x => x.SEQ).FirstOrDefault();

                        model.Add(new GridItem()
                        {
                            UniqueID = item.UniqueID,
                            VHNO = item.VHNO,
                            Status = new FormStatus(item.Status, item.CalibrateType, item.EstCalibrateDate, log != null ? log.STEP : string.Empty),
                            CalibrateType = item.CalibrateType,
                            CalibrateUnit = item.CalibrateUnit,
                            Factory = OrganizationDataAccessor.GetFactory(OrganizationList, item.OrganizationUniqueID),
                            OrganizationDescription = item.OrganizationDescription,
                            CalNo = item.CalNo,
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
                            ResponsorID = item.ResponsorID,
                            ResponsorName = item.ResponsorName,
                            LabDescription = item.LabDescription,
                            JobCalibratorID = item.JobCalibratorID,
                            JobCalibratorName = item.JobCalibratorName,
                            NotifyTime = item.NotifyTime,
                            TakeJobTime = item.TakeJobTime,
                            IsQRCoded = item.IsQRCoded
                        });
                    }

                    model = model.OrderBy(x => x.EstCalibrateDateString).ToList();
                }
            }
            catch (Exception ex)
            {
                model = null;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return model;
        }

       

        #region Reviewed
        public static FormStatus GetFormStatus(string UniqueID)
        {
            FormStatus status = null;

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = (from x in db.QA_CALIBRATIONFORM
                                join apply in db.QA_CALIBRATIONAPPLY on x.APPLYUNIQUEID equals apply.UNIQUEID into tmpApply
                                from apply in tmpApply.DefaultIfEmpty()
                                join notify in db.QA_CALIBRATIONNOTIFY on x.NOTIFYUNIQUEID equals notify.UNIQUEID into tmpNotify
                                from notify in tmpNotify.DefaultIfEmpty()
                                join responsor in db.ACCOUNT on x.CALRESPONSORID equals responsor.ID into tmpResponsor
                                from responsor in tmpResponsor.DefaultIfEmpty()
                                join calibrator in db.ACCOUNT on x.CALUSERID equals calibrator.ID into tmpCalibrator
                                from calibrator in tmpCalibrator.DefaultIfEmpty()
                                join lab in db.QA_LAB on x.LABUNIQUEID equals lab.UNIQUEID into tmpLab
                                from lab in tmpLab.DefaultIfEmpty()
                                join jobCalibrator in db.ACCOUNT on x.JOBCALRESPONSERID equals jobCalibrator.ID into tmpJobCalibrator
                                from jobCalibrator in tmpJobCalibrator.DefaultIfEmpty()
                                where x.UNIQUEID == UniqueID
                                select new
                                {
                                    x.UNIQUEID,
                                    x.STATUS,
                                    x.ESTCALDATE,
                                    CalType = !string.IsNullOrEmpty(x.APPLYUNIQUEID) ? apply.CALTYPE : notify.CALTYPE
                                }).First();

                    var stepLogList = (from x in db.QA_CALIBRATIONFORMSTEPLOG
                                       join owner in db.ACCOUNT
                                       on x.OWNERID equals owner.ID into tmpOwner
                                       from owner in tmpOwner.DefaultIfEmpty()
                                       join qa in db.ACCOUNT
                                       on x.QAID equals qa.ID into tmpQA
                                       from qa in tmpQA.DefaultIfEmpty()
                                       where x.FORMUNIQUEID == form.UNIQUEID
                                       select new StepLogModel
                                       {
                                           OwnerID = x.OWNERID,
                                           OwnerName = owner != null ? owner.NAME : "",
                                           QAID = x.QAID,
                                           QAName = qa != null ? qa.NAME : "",
                                           Seq = x.SEQ,
                                           Step = x.STEP,
                                           Time = x.TIME.Value
                                       }).OrderBy(x => x.Seq).ToList();

                    var log = stepLogList.OrderByDescending(x => x.Seq).FirstOrDefault();

                    status = new FormStatus(form.STATUS, form.CalType, form.ESTCALDATE, log != null ? log.Step : string.Empty);
                }
            }
            catch (Exception ex)
            {
                status = null;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return status;
        }

        public static RequestResult Query(Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new SummaryViewModel();

                var qa = Account.UserAuthGroupList.Contains("QA-Verify") || Account.UserAuthGroupList.Contains("QA") || Account.UserAuthGroupList.Contains("QA-FullQuery");

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query1 = (from x in db.QA_CALIBRATIONFORM
                                 join a in db.QA_CALIBRATIONAPPLY
                                 on x.APPLYUNIQUEID equals a.UNIQUEID
                                 join e in db.QA_EQUIPMENT
                                 on a.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                 join l in db.QA_LAB
                                 on x.LABUNIQUEID equals l.UNIQUEID into tmpLab
                                 from l in tmpLab.DefaultIfEmpty()
                                 where x.STATUS == "0" || x.STATUS == "1" || x.STATUS == "4"
                                 select new
                                 {
                                     UniqueID = x.UNIQUEID,
                                     Status = x.STATUS,
                                     OrganizationUniqueID = e.ORGANIZATIONUNIQUEID,
                                     EstCalibrateDate = x.ESTCALDATE,
                                     CalibrateType = e.CALTYPE,
                                     CalibrateDate = x.CALDATE,
                                     CalibratorID = x.CALUSERID,
                                     ResponsorID=x.CALRESPONSORID,
                                     OwnerID = e.OWNERID,
                                     OwnerManagerID = e.OWNERMANAGERID,
                                     PEID = e.PEID,
                                     PEManagerID = e.PEMANAGERID,
                                     JobCalibratorID = x.JOBCALRESPONSERID,
                                     LabDescription = l!=null?l.DESCRIPTION:""
                                 }).AsQueryable();

                    var query2 = (from x in db.QA_CALIBRATIONFORM
                                  join n in db.QA_CALIBRATIONNOTIFY
                                  on x.NOTIFYUNIQUEID equals n.UNIQUEID
                                  join e in db.QA_EQUIPMENT
                                  on n.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                  join l in db.QA_LAB
                                 on x.LABUNIQUEID equals l.UNIQUEID into tmpLab
                                  from l in tmpLab.DefaultIfEmpty()
                                  where x.STATUS == "0" || x.STATUS == "1" || x.STATUS == "4"
                                  select new
                                  {
                                      UniqueID = x.UNIQUEID,
                                      Status = x.STATUS,
                                      OrganizationUniqueID = e.ORGANIZATIONUNIQUEID,
                                      EstCalibrateDate = x.ESTCALDATE,
                                      CalibrateType = e.CALTYPE,
                                      CalibrateDate = x.CALDATE,
                                      CalibratorID = x.CALUSERID,
                                      ResponsorID = x.CALRESPONSORID,
                                      OwnerID = e.OWNERID,
                                      OwnerManagerID = e.OWNERMANAGERID,
                                      PEID = e.PEID,
                                      PEManagerID = e.PEMANAGERID,
                                      JobCalibratorID = x.JOBCALRESPONSERID,
                                      LabDescription = l != null ? l.DESCRIPTION : ""
                                  }).AsQueryable();

                    var query = query1.Union(query2);

                    if (!qa)
                    {
                        query = query.Where(x => Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID) || x.JobCalibratorID == Account.ID || x.CalibratorID == Account.ID || x.OwnerID == Account.ID || x.OwnerManagerID == Account.ID || x.PEID == Account.ID || x.PEManagerID == Account.ID).AsQueryable();
                    }

                    var itemList = query.ToList();

                    var temp = new List<GridItem>();

                    foreach (var item in itemList)
                    {
                        var log = db.QA_CALIBRATIONFORMSTEPLOG.Where(x => x.FORMUNIQUEID == item.UniqueID).OrderByDescending(x => x.SEQ).FirstOrDefault();

                        temp.Add(new GridItem()
                        {
                            UniqueID = item.UniqueID,
                            Status = new FormStatus(item.Status, item.CalibrateType, item.EstCalibrateDate, log!=null?log.STEP:string.Empty),
                            CalibrateDate = item.CalibrateDate,
                            EstCalibrateDate = item.EstCalibrateDate
                        });
                    }

                    model.ItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-orange",
                        Icon = "fa-download",
                        Count = temp.Count(x => x.Status._Status == "0"),
                        Text = Resources.Resource.CalibrationFormStatus_0,
                        Status = 0
                    });

                    model.ItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-blue",
                        Icon = "fa-wrench",
                        Count = temp.Count(x => x.Status._Status == "1"),
                        Text = Resources.Resource.CalibrationFormStatus_1,
                        Status = 1
                    });

                    model.ItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-exclamation-circle",
                        Count = temp.Count(x => x.Status._Status == "2"),
                        Text = Resources.Resource.CalibrationFormStatus_2,
                        Status = 2
                    });

                    model.ItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-times-circle-o",
                        Count = temp.Count(x => x.Status._Status == "4"),
                        Text = Resources.Resource.CalibrationFormStatus_4,
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

        public static RequestResult Query(List<Models.Shared.Organization> OrganizationList, QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new GridViewModel();

                var qa = Account.UserAuthGroupList.Contains("QA-Verify") || Account.UserAuthGroupList.Contains("QA") || Account.UserAuthGroupList.Contains("QA-FullQuery");

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query1 = db.Database.SqlQuery<SQLQueryItem>(QueryString1).AsQueryable();

                    //var query1 = (from form in db.QA_CALIBRATIONFORM
                    //              join apply in db.QA_CALIBRATIONAPPLY
                    //              on form.APPLYUNIQUEID equals apply.UNIQUEID
                    //              join equipment in db.QA_EQUIPMENT
                    //              on apply.EQUIPMENTUNIQUEID equals equipment.UNIQUEID
                    //              join ichi in db.QA_ICHI
                    //              on equipment.ICHIUNIQUEID equals ichi.UNIQUEID into tmpIchi
                    //              from ichi in tmpIchi.DefaultIfEmpty()
                    //              join organization in db.ORGANIZATION
                    //              on equipment.ORGANIZATIONUNIQUEID equals organization.UNIQUEID
                    //              join calibrator in db.ACCOUNT
                    //              on form.CALUSERID equals calibrator.ID into tmpCalibrator
                    //              from calibrator in tmpCalibrator.DefaultIfEmpty()
                    //              join responsor in db.ACCOUNT
                    //              on form.CALRESPONSORID equals responsor.ID into tmpResponsor
                    //              from responsor in tmpResponsor.DefaultIfEmpty()
                    //              join lab in db.QA_LAB
                    //              on form.LABUNIQUEID equals lab.UNIQUEID into tmpLab
                    //              from lab in tmpLab.DefaultIfEmpty()
                    //              join jobCalibrator in db.ACCOUNT
                    //              on form.JOBCALRESPONSERID equals jobCalibrator.ID into tmpJobCalibrator
                    //              from jobCalibrator in tmpJobCalibrator.DefaultIfEmpty()
                    //              select new
                    //              {
                    //                  UniqueID = form.UNIQUEID,
                    //                  VHNO = form.VHNO,
                    //                  Status = form.STATUS,
                    //                  OrganizationUniqueID = equipment.ORGANIZATIONUNIQUEID,
                    //                  OrganizationDescription = organization.DESCRIPTION,
                    //                  EstCalibrateDate = form.ESTCALDATE,
                    //                  CalibrateDate = form.CALDATE,
                    //                  ResponsorID = form.CALRESPONSORID,
                    //                  ResponsorName = responsor != null ? responsor.NAME : "",
                    //                  CalibratorID = form.CALUSERID,
                    //                  CalibratorName = calibrator != null ? calibrator.NAME : "",
                    //                  LabDescription = lab!=null?lab.DESCRIPTION:"",
                    //                  NotifyTime = form.NOTIFYDATE,
                    //                  TakeJobTime = form.TAKEJOBDATE,
                    //                  OwnerID = equipment.OWNERID,
                    //                  OwnerManagerID = equipment.OWNERMANAGERID,
                    //                  PEID = equipment.PEID,
                    //                  PEManagerID = equipment.PEMANAGERID,
                    //                  JobCalibratorID = form.JOBCALRESPONSERID,
                    //                  JobCalibratorName = jobCalibrator != null ? jobCalibrator.NAME : "",
                    //                  CalNo = equipment.CALNO,
                    //                  SerialNo = equipment.SERIALNO,
                    //                  IchiUniqueID = equipment.ICHIUNIQUEID,
                    //                  IchiName = ichi != null ? ichi.NAME : "",
                    //                  IchiRemark = equipment.ICHIREMARK,
                    //                  MachineNo = equipment.MACHINENO,
                    //                  Brand = equipment.BRAND,
                    //                  Model = equipment.MODEL,
                    //                  CalibrateType = equipment.CALTYPE,
                    //                  CalibrateUnit = equipment.CALUNIT,
                    //                 IsQRCoded= form.ISQRCODE=="Y"
                    //              }).AsQueryable();

                    var query2 = db.Database.SqlQuery<SQLQueryItem>(QueryString2).AsQueryable();

                    //var query2 = (from form in db.QA_CALIBRATIONFORM
                    //             join notify in db.QA_CALIBRATIONNOTIFY
                    //             on form.NOTIFYUNIQUEID equals notify.UNIQUEID
                    //             join equipment in db.QA_EQUIPMENT
                    //             on notify.EQUIPMENTUNIQUEID equals equipment.UNIQUEID
                    //             join ichi in db.QA_ICHI
                    //             on equipment.ICHIUNIQUEID equals ichi.UNIQUEID into tmpIchi
                    //             from ichi in tmpIchi.DefaultIfEmpty()
                    //             join organization in db.ORGANIZATION
                    //            on equipment.ORGANIZATIONUNIQUEID equals organization.UNIQUEID
                    //              join calibrator in db.ACCOUNT
                    //               on form.CALUSERID equals calibrator.ID into tmpCalibrator
                    //              from calibrator in tmpCalibrator.DefaultIfEmpty()
                    //              join responsor in db.ACCOUNT
                    //              on form.CALRESPONSORID equals responsor.ID into tmpResponsor
                    //              from responsor in tmpResponsor.DefaultIfEmpty()
                    //              join lab in db.QA_LAB
                    //              on form.LABUNIQUEID equals lab.UNIQUEID into tmpLab
                    //              from lab in tmpLab.DefaultIfEmpty()
                    //             join jobCalibrator in db.ACCOUNT
                    //             on form.JOBCALRESPONSERID equals jobCalibrator.ID into tmpJobCalibrator
                    //              from jobCalibrator in tmpJobCalibrator.DefaultIfEmpty()
                    //             select new
                    //             {

                    //                 UniqueID = form.UNIQUEID,
                    //                 VHNO = form.VHNO,
                    //                 Status = form.STATUS,
                    //                 OrganizationUniqueID = equipment.ORGANIZATIONUNIQUEID,
                    //                 OrganizationDescription = organization.DESCRIPTION,
                    //                 EstCalibrateDate = form.ESTCALDATE,
                    //                 CalibrateDate = form.CALDATE,
                    //                 ResponsorID = form.CALRESPONSORID,
                    //                 ResponsorName = responsor != null ? responsor.NAME : "",
                    //                 CalibratorID = form.CALUSERID,
                    //                 CalibratorName = calibrator != null ? calibrator.NAME : "",
                    //                 LabDescription = lab != null ? lab.DESCRIPTION : "",
                    //                 NotifyTime = form.NOTIFYDATE,
                    //                 TakeJobTime = form.TAKEJOBDATE,
                    //                 OwnerID = equipment.OWNERID,
                    //                 OwnerManagerID = equipment.OWNERMANAGERID,
                    //                 PEID = equipment.PEID,
                    //                 PEManagerID = equipment.PEMANAGERID,
                    //                 JobCalibratorID = form.JOBCALRESPONSERID,
                    //                 JobCalibratorName = jobCalibrator != null ? jobCalibrator.NAME : "",
                    //                 CalNo = equipment.CALNO,
                    //                 SerialNo = equipment.SERIALNO,
                    //                 IchiUniqueID = equipment.ICHIUNIQUEID,
                    //                 IchiName = ichi != null ? ichi.NAME : "",
                    //                 IchiRemark = equipment.ICHIREMARK,
                    //                 MachineNo = equipment.MACHINENO,
                    //                 Brand = equipment.BRAND,
                    //                 Model = equipment.MODEL,
                    //                 CalibrateType = equipment.CALTYPE,
                    //                 CalibrateUnit = equipment.CALUNIT,
                    //                 IsQRCoded = form.ISQRCODE == "Y"
                    //             }).AsQueryable();

                    var query = query1.Union(query2);

                    if (!qa)
                    {
                        query = query.Where(x => Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID) || x.JobCalibratorID == Account.ID || x.CalibratorID == Account.ID || x.OwnerID == Account.ID || x.OwnerManagerID == Account.ID || x.PEID == Account.ID || x.PEManagerID == Account.ID).AsQueryable();
                    }

                    if (Parameters.EstBeginDate.HasValue)
                    {
                        query = query.Where(x => DateTime.Compare(x.EstCalibrateDate, Parameters.EstBeginDate.Value) >= 0);
                    }

                    if (Parameters.EstEndDate.HasValue)
                    {
                        query = query.Where(x => DateTime.Compare(x.EstCalibrateDate, Parameters.EstEndDate.Value) < 0);
                    }

                    if (Parameters.ActBeginDate.HasValue)
                    {
                        query = query.Where(x =>x.CalibrateDate.HasValue&& DateTime.Compare(x.CalibrateDate.Value, Parameters.ActBeginDate.Value) >= 0);
                    }

                    if (Parameters.ActEndDate.HasValue)
                    {
                        query = query.Where(x => x.CalibrateDate.HasValue && DateTime.Compare(x.CalibrateDate.Value, Parameters.ActEndDate.Value) < 0);
                    }

                    if (!string.IsNullOrEmpty(Parameters.VHNO))
                    {
                        query = query.Where(x => x.VHNO.Contains(Parameters.VHNO));
                    }

                    if (!string.IsNullOrEmpty(Parameters.SerialNo))
                    {
                        query = query.Where(x => x.SerialNo.Contains(Parameters.SerialNo));
                    }

                    if (!string.IsNullOrEmpty(Parameters.CalNo))
                    {
                        query = query.Where(x => x.CalNo.Contains(Parameters.CalNo));
                    }

                    if (!string.IsNullOrEmpty(Parameters.OwnerID))
                    {
                        query = query.Where(x => x.OwnerID == Parameters.OwnerID);
                    }

                    if (!string.IsNullOrEmpty(Parameters.OwnerManagerID))
                    {
                        query = query.Where(x => x.OwnerManagerID == Parameters.OwnerManagerID);
                    }

                    if (!string.IsNullOrEmpty(Parameters.PEID))
                    {
                        query = query.Where(x => x.PEID == Parameters.PEID);
                    }

                    if (!string.IsNullOrEmpty(Parameters.PEManagerID))
                    {
                        query = query.Where(x => x.PEManagerID == Parameters.PEManagerID);
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

                    var all = query.ToList();

                    var itemList = all.Select(x => new
                    {
                        x.UniqueID,
                        x.VHNO ,
                        x.Status ,
                        x.OrganizationUniqueID,
                        x.OrganizationDescription,
                        x.EstCalibrateDate ,
                        x.CalibrateDate ,
                        x.ResponsorID,
                        x.ResponsorName ,
                        x.CalibratorID,
                        x.CalibratorName,
                        x.LabDescription,
                        x.NotifyTime,
                        x.TakeJobTime,
                        x.OwnerID,
                        x.OwnerManagerID,
                        x.PEID,
                        x.PEManagerID,
                        x.JobCalibratorID,
                        x.JobCalibratorName,
                        x.CalNo,
                        x.SerialNo,
                        x.IchiUniqueID,
                        x.IchiName,
                        x.IchiRemark,
                        x.MachineNo,
                        x.Brand,
                        x.Model,
                        x.CalibrateType,
                        x.CalibrateUnit,
                        x.IsQRCoded
                    }).Distinct().ToList();


                    foreach (var item in itemList)
                    {
                        //var log = db.QA_CALIBRATIONFORMSTEPLOG.Where(x => x.FORMUNIQUEID == item.UniqueID).OrderByDescending(x => x.SEQ).FirstOrDefault();
                        var log = all.Where(x => x.UniqueID == item.UniqueID).OrderByDescending(x => x.LogSeq).FirstOrDefault();

                        model.ItemList.Add(new GridItem()
                        {
                            UniqueID = item.UniqueID,
                            VHNO = item.VHNO,
                            Status = new FormStatus(item.Status, item.CalibrateType, item.EstCalibrateDate, log != null ? log.Step : string.Empty),
                            CalibrateType = item.CalibrateType,
                            CalibrateUnit = item.CalibrateUnit,
                            Factory = OrganizationDataAccessor.GetFactory(OrganizationList, item.OrganizationUniqueID),
                            OrganizationDescription = item.OrganizationDescription,
                            CalNo = item.CalNo,
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
                            ResponsorID = item.ResponsorID,
                            ResponsorName = item.ResponsorName,
                            LabDescription = item.LabDescription,
                            JobCalibratorID = item.JobCalibratorID,
                            JobCalibratorName = item.JobCalibratorName,
                            NotifyTime = item.NotifyTime,
                            TakeJobTime = item.TakeJobTime,
                            Account = Account,
                            IsQRCoded = item.IsQRCoded == "Y"
                        });
                    }

                    if (Parameters.StatusList.Count > 0)
                    {
                        var temp = model.ItemList.Where(x => x.Status._Status != "5" && Parameters.StatusList.Contains(x.Status._Status)).OrderBy(x => x.Seq).ThenBy(x => x.StatusSeq).ThenBy(x => x.EstCalibrateDate).ToList();

                        temp = temp.Union(model.ItemList.Where(x => x.Status._Status == "5" && Parameters.StatusList.Contains(x.Status._Status)).OrderBy(x => x.Seq).ThenBy(x => x.StatusSeq).ThenByDescending(x => x.EstCalibrateDate).ToList()).ToList();

                        model.ItemList = temp;
                    }
                    else
                    {
                        var temp = model.ItemList.Where(x => x.Status._Status != "5").OrderBy(x => x.Seq).ThenBy(x => x.StatusSeq).ThenBy(x => x.EstCalibrateDate).ToList();

                        temp = temp.Union(model.ItemList.Where(x => x.Status._Status == "5").OrderBy(x => x.Seq).ThenBy(x => x.StatusSeq).ThenByDescending(x => x.EstCalibrateDate).ToList()).ToList();

                        model.ItemList = temp;
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

        public static RequestResult GetDetailViewModel(List<Models.Shared.Organization> OrganizationList, string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                result.ReturnData(new DetailViewModel()
                {
                    UniqueID = UniqueID,
                    FormViewModel = GetFormViewModel(OrganizationList, UniqueID)
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

        public static RequestResult TakeJob(string UniqueID, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = db.QA_CALIBRATIONFORM.First(x => x.UNIQUEID == UniqueID);

                    var takeJobTime = DateTime.Now;

                    form.TAKEJOBDATE = takeJobTime;
                    form.STATUS = "1";
                    form.CALRESPONSORID = Account.ID;

                    int seq = db.QA_CALIBRATIONFORMTAKEJOBLOG.Count(x => x.FORMUNIQUEID == form.UNIQUEID)+1;

                    db.QA_CALIBRATIONFORMTAKEJOBLOG.Add(new QA_CALIBRATIONFORMTAKEJOBLOG
                    {
                        FORMUNIQUEID = form.UNIQUEID,
                        SEQ = seq,
                        TIME = takeJobTime,
                        USERID = Account.ID
                    });

                    db.SaveChanges();

                    result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.TakeJob, Resources.Resource.Success));
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

        public static RequestResult Return(List<Models.Shared.Organization> OrganizationList, string UniqueID, string Comment, Account Account)
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
                        var form = db.QA_CALIBRATIONFORM.First(x => x.UNIQUEID == UniqueID);

                        form.STATUS = "6";

                        db.SaveChanges();

                        var time = DateTime.Now;

                        if (!string.IsNullOrEmpty(form.APPLYUNIQUEID))
                        {
                            var apply = db.QA_CALIBRATIONAPPLY.First(x => x.UNIQUEID == form.APPLYUNIQUEID);

                            apply.STATUS = "2";

                            db.SaveChanges();

                            var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == apply.EQUIPMENTUNIQUEID);

                            var seq = db.QA_CALIBRATIONAPPLYFLOWLOG.Where(x => x.APPLYUNIQUEID == apply.UNIQUEID).Max(x => x.SEQ) + 1;

                            if (equipment.CALUNIT == "F")
                            {
                                db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                {
                                    APPLYUNIQUEID = apply.UNIQUEID,
                                    FLOWSEQ = 1,
                                    ISCANCELED = "N",
                                    NOTIFYTIME = time,
                                    SEQ = seq,
                                    USERID = Account.ID,
                                    VERIFYRESULT = "N",
                                    VERIFYTIME = time,
                                    VERIFYCOMMENT = Comment
                                });
                            }
                            else
                            {
                                db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                                {
                                    APPLYUNIQUEID = apply.UNIQUEID,
                                    FLOWSEQ = 5,
                                    ISCANCELED = "N",
                                    NOTIFYTIME = time,
                                    SEQ = seq,
                                    USERID = Account.ID,
                                    VERIFYRESULT = "N",
                                    VERIFYTIME = time,
                                    VERIFYCOMMENT = Comment
                                });
                            }

                            db.SaveChanges();

                            seq++;

                            db.QA_CALIBRATIONAPPLYFLOWLOG.Add(new QA_CALIBRATIONAPPLYFLOWLOG()
                            {
                                APPLYUNIQUEID = apply.UNIQUEID,
                                SEQ = seq,
                                FLOWSEQ = 0,
                                NOTIFYTIME = DateTime.Now,
                                USERID = apply.CREATORID,
                                ISCANCELED = "N"
                            });

                            db.SaveChanges();

                            CalibrationApplyDataAccessor.SendRejectMail(apply.UNIQUEID, apply.VHNO, new List<string>() { apply.CREATORID, apply.OWNERID });
                        }

                        if (!string.IsNullOrEmpty(form.NOTIFYUNIQUEID))
                        {
                            var notify = db.QA_CALIBRATIONNOTIFY.First(x => x.UNIQUEID == form.NOTIFYUNIQUEID);

                            notify.STATUS = "2";

                            db.SaveChanges();

                            var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == notify.EQUIPMENTUNIQUEID);

                            var seq = db.QA_CALIBRATIONNOTIFYFLOWLOG.Where(x => x.NOTIFYUNIQUEID == notify.UNIQUEID).Max(x => x.SEQ) + 1;

                            if (equipment.CALUNIT == "F")
                            {
                                db.QA_CALIBRATIONNOTIFYFLOWLOG.Add(new QA_CALIBRATIONNOTIFYFLOWLOG()
                                {
                                    NOTIFYUNIQUEID = notify.UNIQUEID,
                                    FLOWSEQ = 1,
                                    ISCANCELED = "N",
                                    NOTIFYTIME = time,
                                    SEQ = seq,
                                    USERID = Account.ID,
                                    VERIFYRESULT = "N",
                                    VERIFYTIME = time,
                                    VERIFYCOMMENT = Comment
                                });
                            }
                            else
                            {
                                db.QA_CALIBRATIONNOTIFYFLOWLOG.Add(new QA_CALIBRATIONNOTIFYFLOWLOG()
                                {
                                    NOTIFYUNIQUEID = notify.UNIQUEID,
                                    FLOWSEQ = 5,
                                    ISCANCELED = "N",
                                    NOTIFYTIME = time,
                                    SEQ = seq,
                                    USERID = Account.ID,
                                    VERIFYRESULT = "N",
                                    VERIFYTIME = time,
                                    VERIFYCOMMENT = Comment
                                });
                            }

                            db.SaveChanges();

                            seq++;

                            db.QA_CALIBRATIONNOTIFYFLOWLOG.Add(new QA_CALIBRATIONNOTIFYFLOWLOG()
                            {
                                NOTIFYUNIQUEID = notify.UNIQUEID,
                                SEQ = seq,
                                FLOWSEQ = 1,
                                NOTIFYTIME = DateTime.Now,
                                USERID = equipment.OWNERID,
                                ISCANCELED = "N"
                            });

                            db.SaveChanges();

                            CalibrationNotifyDataAccessor.SendRejectMail(OrganizationList, new List<string>() { equipment.OWNERID }, notify);
                        }
                        
                    #if !DEBUG
                        trans.Complete();
#endif

                        result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Reject, Resources.Resource.Success));

                        
                    #if !DEBUG
                    }
#endif
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

        public static RequestResult GetEditFormModel(List<Models.Shared.Organization> OrganizationList, string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new EditFormModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = (from x in db.QA_CALIBRATIONFORM
                                join apply in db.QA_CALIBRATIONAPPLY on x.APPLYUNIQUEID equals apply.UNIQUEID into tmpApply
                                from apply in tmpApply.DefaultIfEmpty()
                                join notify in db.QA_CALIBRATIONNOTIFY on x.NOTIFYUNIQUEID equals notify.UNIQUEID into tmpNotify
                                from notify in tmpNotify.DefaultIfEmpty()
                                join responsor in db.ACCOUNT on x.CALRESPONSORID equals responsor.ID into tmpResponsor
                                from responsor in tmpResponsor.DefaultIfEmpty()
                                join jobCalibrator in db.ACCOUNT on x.JOBCALRESPONSERID equals jobCalibrator.ID into tmpJobCalibrator
                                from jobCalibrator in tmpJobCalibrator.DefaultIfEmpty()
                                where x.UNIQUEID == UniqueID
                                select new
                                {
                                    x.UNIQUEID,
                                    x.HAVEABNORMAL,
                                    x.VHNO,
                                    x.STATUS,
                                    ApplyUniqueID = apply != null ? apply.UNIQUEID : "",
                                    ApplyVHNO = apply != null ? apply.VHNO : "",
                                    NotifyUniqueID = notify != null ? notify.UNIQUEID : "",
                                    NotifyVHNO = notify != null ? notify.VHNO : "",
                                    x.ESTCALDATE,
                                    x.CALDATE,
                                    x.NOTIFYDATE,
                                    x.JOBCALRESPONSERID,
                                    JobCalibratorName = jobCalibrator != null ? jobCalibrator.NAME : "",
                                    x.CALRESPONSORID,
                                    ResponsorName = responsor != null ? responsor.NAME : "",
                                    x.CALUSERID,
                                    x.LABUNIQUEID,
                                    x.TRACEABLENO,
                                    x.TAKEJOBDATE,
                                    x.TEMPPERATURE,
                                    x.HUMIDITY,
                                    EquipmentUniqueID = !string.IsNullOrEmpty(x.APPLYUNIQUEID) ? apply.EQUIPMENTUNIQUEID : notify.EQUIPMENTUNIQUEID,
                                    CalType = !string.IsNullOrEmpty(x.APPLYUNIQUEID) ? apply.CALTYPE : notify.CALTYPE,
                                    CalUnit = !string.IsNullOrEmpty(x.APPLYUNIQUEID) ? apply.CALUNIT : notify.CALUNIT
                                }).First();

                    var equipment = EquipmentHelper.Get(OrganizationList, form.EquipmentUniqueID);

                    var stepLogList = (from x in db.QA_CALIBRATIONFORMSTEPLOG
                                       join owner in db.ACCOUNT
                                       on x.OWNERID equals owner.ID into tmpOwner
                                       from owner in tmpOwner.DefaultIfEmpty()
                                       join qa in db.ACCOUNT
                                       on x.QAID equals qa.ID into tmpQA
                                       from qa in tmpQA.DefaultIfEmpty()
                                       where x.FORMUNIQUEID == form.UNIQUEID
                                       select new StepLogModel
                                       {
                                           OwnerID = x.OWNERID,
                                           OwnerName = owner != null ? owner.NAME : "",
                                           QAID = x.QAID,
                                           QAName = qa != null ? qa.NAME : "",
                                           Seq = x.SEQ,
                                           Step = x.STEP,
                                           Time = x.TIME.Value
                                       }).OrderBy(x => x.Seq).ToList();

                    var log = stepLogList.OrderByDescending(x => x.Seq).FirstOrDefault();

                    model = new EditFormModel()
                    {
                        UniqueID = form.UNIQUEID,
                        VHNO = form.VHNO,
                        Status = new FormStatus(form.STATUS, form.CalType, form.ESTCALDATE, log != null ? log.Step : string.Empty),
                        Equipment = equipment,
                        CalibrateType = form.CalType,
                        CalibrateUnit = form.CalUnit,
                        ApplyUniqueID = form.ApplyUniqueID,
                        ApplyVHNO = form.ApplyVHNO,
                        NotifyUniqueID = form.NotifyUniqueID,
                        NotifyVHNO = form.NotifyVHNO,
                        EstCalibrateDate = form.ESTCALDATE,
                        ResponsorID = form.CALRESPONSORID,
                        ResponsorName = form.ResponsorName,
                        JobCalibratorID = form.JOBCALRESPONSERID,
                        JobCalibratorName = form.JobCalibratorName,
                        NotifyTime = form.NOTIFYDATE,
                        TakeJobTime = form.TAKEJOBDATE,
                        FormInput = new FormInput()
                        {
                            Temperature = form.TEMPPERATURE,
                            Humidity = form.HUMIDITY,
                            TraceableNo = form.TRACEABLENO,
                            CalNo = equipment.CalNo,
                            HaveAbnormal = form.HAVEABNORMAL == "Y",
                            CalibrateDateString = DateTimeHelper.DateTime2DateStringWithSeperator(form.CALDATE),
                            CalibratorID = form.CALUSERID,
                            LabUniqueID = form.LABUNIQUEID
                        },
                        AbnormalFormList = db.QA_ABNORMALFORM.Where(x => x.CALFORMUNIQUEID == form.UNIQUEID).Select(x => new AbnormalFormModel
                        {
                            UniqueID = x.UNIQUEID,
                            VHNO = x.VHNO,
                            Status = x.STATUS,
                            CreateTime = x.CREATETIME.Value,
                            HandlingRemark = x.HANDLINGREMARK
                        }).OrderBy(x => x.CreateTime).ToList(),
                        StepLogList = stepLogList,
                        TakeJobLogList = (from x in db.QA_CALIBRATIONFORMTAKEJOBLOG
                                          join u in db.ACCOUNT
                                          on x.USERID equals u.ID into tmpTakeJobUser
                                          from u in tmpTakeJobUser.DefaultIfEmpty()
                                          where x.FORMUNIQUEID == form.UNIQUEID
                                          select new TakeJobLogModel
                                          {
                                              Seq = x.SEQ,
                                              CalibratorID = x.USERID,
                                              CalibratorName = u != null ? u.NAME : "",
                                              Time = x.TIME.Value
                                          }).OrderBy(x => x.Seq).ToList(),
                        LogList = (from l in db.QA_CALIBRATIONFORMFLOWLOG
                                   join u in db.ACCOUNT
                                   on l.USERID equals u.ID
                                   where l.FORMUNIQUEID == form.UNIQUEID
                                   select new LogModel
                                   {
                                       Seq = l.SEQ,
                                       FlowSeq = l.FLOWSEQ,
                                       NotifyTime = l.NOTIFYTIME,
                                       VerifyResult = l.VERIFYRESULT,
                                       UserID = l.USERID,
                                       UserName = u.NAME,
                                       VerifyTime = l.VERIFYTIME,
                                       VerifyComment = l.VERIFYCOMMENT
                                   }).OrderBy(x => x.VerifyTime).ThenBy(x => x.Seq).ToList(),
                        ItemList = (from x in db.QA_CALIBRATIONFORMDETAIL
                                    where x.FORMUNIQUEID == form.UNIQUEID
                                    select new DetailItem
                                    {
                                        Seq = x.SEQ,
                                        Characteristic = x.CHARACTERISTIC,
                                        CalibrationPoint = x.CALIBRATIONPOINT,
                                        UsingRange = x.USINGRANGE,
                                        RangeTolerance = x.RANGETOLERANCE,
                                        Standard = x.STANDARD,
                                        CalibrateDate = x.CALDATE,
                                        Tolerance = x.TOLERANCE,
                                        ToleranceMark = x.TOLERANCESYMBOL,
                                        Unit = x.UNIT,
                                        ToleranceUnit = x.TOLERANCEUNIT,
                                        ToleranceUnitRate = x.TOLERANCEUNITRATE,
                                        ReadingValue = x.READINGVALUE
                                    }).OrderBy(x => x.Seq).ToList(),
                        STDUSEList = (from x in db.QA_CALIBRATIONFORMSTDUSE
                                      join e in db.QA_EQUIPMENT
                                      on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                      join calibrator in db.ACCOUNT
                                      on x.LASTCALUSERID equals calibrator.ID into tmpCalibrator
                                      from calibrator in tmpCalibrator.DefaultIfEmpty()
                                      join lab in db.QA_LAB
                                      on x.LASTLABUNIQUEID equals lab.UNIQUEID into tmpLab
                                      from lab in tmpLab.DefaultIfEmpty()
                                      join i in db.QA_ICHI
                                      on e.ICHIUNIQUEID equals i.UNIQUEID into tmpIchi
                                      from i in tmpIchi.DefaultIfEmpty()
                                      where x.FORMUNIQUEID == form.UNIQUEID
                                      select new STDUSEModel
                                      {
                                          UniqueID = x.EQUIPMENTUNIQUEID,
                                          CalNo = e.CALNO,
                                          LastCalibrateDate = x.LASTCALDATE,
                                          NextCalibrateDate = x.NEXTCALDATE,
                                          CalibratorID = x.LASTCALUSERID,
                                          CalibratorName = calibrator != null ? calibrator.NAME : "",
                                           LabDescription = lab!=null?lab.DESCRIPTION:"",
                                           IchiUniqueID=e.ICHIUNIQUEID,
                                           IchiName=i!=null?i.NAME:"",
                                            IchiRemark=e.ICHIREMARK
                                      }).ToList(),
                        FileList = db.QA_CALIBRATIONFORMFILE.Where(f => f.FORMUNIQUEID == UniqueID).ToList().Select(f => new FileModel
                        {
                            Seq = f.SEQ,
                            FileName = f.FILENAME,
                            Extension = f.EXTENSION,
                            Size = f.CONTENTLENGTH,
                            LastModifyTime = f.UPLOADTIME,
                            IsSaved = true
                        }).OrderBy(f => f.LastModifyTime).ToList(),
                        CalibratorSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        },
                        LabSelectItemList = new List<SelectListItem>() 
                        {
                            Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                        },
                    };

                    foreach (var item in model.ItemList)
                    {
                        var pattern = string.Format("{0}_{1}_*", model.UniqueID, item.Seq);

                        var files = Directory.GetFiles(Config.QAFileFolderPath, pattern);

                        if (files != null && files.Count() > 0)
                        {
                            item.PhotoFullPathList = files.ToList();
                        }
                    }

                    var calibratorList = (from x in db.USERAUTHGROUP
                                          join u in db.ACCOUNT
                                          on x.USERID equals u.ID
                                          where x.AUTHGROUPID == "QA"
                                          select u).ToList();

                    foreach (var c in calibratorList)
                    {
                        var ename = string.Empty;

                        if (!string.IsNullOrEmpty(c.EMAIL))
                        {
                            try
                            {
                                ename = c.EMAIL.Substring(0, c.EMAIL.IndexOf('@'));
                            }
                            catch
                            {
                                ename = string.Empty;
                            }
                        }

                        var display = string.Empty;

                        if (!string.IsNullOrEmpty(ename))
                        {
                            display = string.Format("{0}/{1}/{2}", c.ID, c.NAME, ename);
                        }
                        else
                        {
                            display = string.Format("{0}/{1}", c.ID, c.NAME);
                        }

                        model.CalibratorSelectItemList.Add(new SelectListItem()
                        {
                            Value = c.ID,
                            Text = display
                        });
                    }

                    var labList = db.QA_LAB.OrderBy(x => x.DESCRIPTION).ToList();

                    foreach (var lab in labList)
                    {
                        model.LabSelectItemList.Add(new SelectListItem()
                        {
                            Value = lab.UNIQUEID,
                            Text = lab.DESCRIPTION
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
                        var form = db.QA_CALIBRATIONFORM.First(x => x.UNIQUEID == Model.UniqueID);

                        var equipmentUniqueID = string.Empty;

                        if (!string.IsNullOrEmpty(form.APPLYUNIQUEID))
                        {
                            equipmentUniqueID = db.QA_CALIBRATIONAPPLY.First(x => x.UNIQUEID == form.APPLYUNIQUEID).EQUIPMENTUNIQUEID;
                        }

                        if (!string.IsNullOrEmpty(form.NOTIFYUNIQUEID))
                        {
                            equipmentUniqueID = db.QA_CALIBRATIONNOTIFY.First(x => x.UNIQUEID == form.NOTIFYUNIQUEID).EQUIPMENTUNIQUEID;
                        }

                        var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == equipmentUniqueID);

                        if (db.QA_EQUIPMENT.Any(x => x.CALNO == Model.FormInput.CalNo && x.UNIQUEID != equipment.UNIQUEID))
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.CalNo, Resources.Resource.Exists));
                        }
                        else
                        {
                            equipment.CALNO = Model.FormInput.CalNo;
                            equipment.PHOTOEXTENSION = Model.Equipment.Extension;

                            db.SaveChanges();

                            if ((Model.CalibrateType == "IL" || Model.CalibrateType == "IF") && Model.CalibrateUnit == "L")
                            {
                                form.TEMPPERATURE = Model.FormInput.Temperature;
                                form.HUMIDITY = Model.FormInput.Humidity;
                            }

                            form.HAVEABNORMAL = Model.FormInput.HaveAbnormal ? "Y" : "N";
                            form.TRACEABLENO = Model.FormInput.TraceableNo;
                            form.CALDATE = Model.FormInput.CalibrateDate;
                            form.LABUNIQUEID = Model.FormInput.LabUniqueID;
                            form.CALUSERID = Model.FormInput.CalibratorID;

                            db.SaveChanges();

                            foreach (var item in Model.ItemList)
                            {
                                var d = db.QA_CALIBRATIONFORMDETAIL.First(x => x.FORMUNIQUEID == Model.UniqueID && x.SEQ == item.Seq);

                                d.STANDARD = item.Standard;
                                d.READINGVALUE = item.ReadingValue.HasValue ? decimal.Parse(item.ReadingValue.Value.ToString()) : default(decimal?);
                                d.CALDATE = item.CalibrateDate;
                            }

                            db.SaveChanges();

                            db.QA_CALIBRATIONFORMSTEPLOG.RemoveRange(db.QA_CALIBRATIONFORMSTEPLOG.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList());

                            db.SaveChanges();

                            db.QA_CALIBRATIONFORMSTEPLOG.AddRange(Model.StepLogList.Select(x => new QA_CALIBRATIONFORMSTEPLOG
                            {
                                FORMUNIQUEID = form.UNIQUEID,
                                SEQ = x.Seq,
                                OWNERID = x.OwnerID,
                                QAID = x.QAID,
                                TIME = x.Time,
                                STEP = x.Step
                            }).ToList());

                            db.SaveChanges();

                            db.QA_CALIBRATIONFORMSTDUSE.RemoveRange(db.QA_CALIBRATIONFORMSTDUSE.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList());

                            db.SaveChanges();

                            foreach (var stduse in Model.FormInput.STDUSEList)
                            {
                                var e = db.QA_EQUIPMENT.First(x => x.UNIQUEID == stduse);

                                db.QA_CALIBRATIONFORMSTDUSE.Add(new QA_CALIBRATIONFORMSTDUSE
                                {
                                    FORMUNIQUEID = form.UNIQUEID,
                                    EQUIPMENTUNIQUEID = e.UNIQUEID,
                                    LASTCALUSERID = e.LASTCALUSERID,
                                    LASTCALDATE = e.LASTCALDATE,
                                    LASTLABUNIQUEID = e.LASTLABUNIQUEID,
                                    NEXTCALDATE = e.NEXTCALDATE
                                });
                            }

                            db.SaveChanges();

                            var fileList = db.QA_CALIBRATIONFORMFILE.Where(x => x.FORMUNIQUEID == form.UNIQUEID).ToList();

                            foreach (var file in fileList)
                            {
                                if (!Model.FileList.Any(x => x.Seq == file.SEQ))
                                {
                                    try
                                    {
                                        System.IO.File.Delete(Path.Combine(Config.QAFileFolderPath, string.Format("{0}_{1}.{2}", form.UNIQUEID, file.SEQ, file.EXTENSION)));
                                    }
                                    catch { }
                                }
                            }

                            db.QA_CALIBRATIONFORMFILE.RemoveRange(fileList);

                            db.SaveChanges();

                            foreach (var file in Model.FileList)
                            {
                                db.QA_CALIBRATIONFORMFILE.Add(new QA_CALIBRATIONFORMFILE()
                                {
                                    FORMUNIQUEID = form.UNIQUEID,
                                    SEQ = file.Seq,
                                    EXTENSION = file.Extension,
                                    FILENAME = file.FileName,
                                    UPLOADTIME = file.LastModifyTime,
                                    CONTENTLENGTH = file.Size
                                });

                                if (!file.IsSaved)
                                {
                                    System.IO.File.Copy(Path.Combine(Config.TempFolder, file.TempFileName), Path.Combine(Config.QAFileFolderPath, string.Format("{0}_{1}.{2}", form.UNIQUEID, file.Seq, file.Extension)), true);
                                    System.IO.File.Delete(Path.Combine(Config.TempFolder, file.TempFileName));
                                }
                            }

                            db.SaveChanges();

#if !DEBUG
                            trans.Complete();
                        }
#endif

                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Edit, Resources.Resource.CalibrationForm, Resources.Resource.Success));
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

        public static RequestResult GetDetailItemEditFormModel(string CalibrationFormUniqueID, DetailItem Item)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    result.ReturnData(new DetailItemEditFormModel()
                    {
                         CalibrationFormUniqueID=CalibrationFormUniqueID,
                        Seq = Item.Seq,
                        Characteristic = Item.Characteristic,
                        UsingRange = Item.UsingRange,
                        RangeTolerance = Item.RangeTolerance,
                        CalibrationPoint = Item.CalibrationPoint,
                        Tolerance = Item.ToleranceDisplay,
                        FormInput = new DetailItemFormInput()
                        {
                            //CalibrateDateString = !Item.ReadingValue.HasValue && !Item.Standard.HasValue ? DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today) : Item.CalibrateDateString,
                            ReadingValue = Item.ReadingValue,
                            Standard = Item.Standard,
                        },
                        PhotoList = Item.PhotoModelList
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

        public static RequestResult EditDetailItem(List<DetailItem> ItemList, DetailItemEditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                var item = ItemList.First(x => x.Seq == Model.Seq);

                item.ReadingValue = Model.FormInput.ReadingValue;
                item.CalibrateDate = DateTime.Today;
                //item.CalibrateDate = Model.FormInput.CalibrateDate;
                item.Standard = Model.FormInput.Standard;

                

                var pattern = string.Format("{0}_{1}_*", Model.CalibrationFormUniqueID, item.Seq);

                var files = Directory.GetFiles(Config.QAFileFolderPath, pattern);

                if (files != null && files.Count() > 0)
                {
                    var fileList = files.ToList();

                    foreach (var file in fileList)
                    {
                        var fileInfo = new FileInfo(file);

                        if (!Model.PhotoList.Any(x => x.FileName == fileInfo.Name))
                        {
                            File.Delete(file);
                        }
                    }
                }

                files = Directory.GetFiles(Config.QAFileFolderPath, pattern);

                item.PhotoFullPathList = new List<string>();

                if (files != null && files.Count() > 0)
                {
                    item.PhotoFullPathList = files.ToList();
                }

                result.ReturnData(ItemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult Submit(List<Models.Shared.Organization> OrganizationList, EditFormModel Model, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if ((Model.CalibrateType == "IF" || Model.CalibrateType == "IL") && Model.CalibrateUnit == "L" && string.IsNullOrEmpty(Model.FormInput.CalibratorID))
                    {
                        result.ReturnFailedMessage("請選擇校驗人員");
                    }
                    else if ((Model.CalibrateType == "EF" || Model.CalibrateType == "EL") && string.IsNullOrEmpty(Model.FormInput.LabUniqueID))
                    {
                        result.ReturnFailedMessage("請選擇外校實驗室");
                    }
                    else if (Model.CalibrateType == "IL" && Model.ItemList.Any(x => !x.IsFailed.HasValue) && !Model.AbnormalFormList.Any(x => x.Status == "4" && string.IsNullOrEmpty(x.HandlingRemark)))
                    {
                        result.ReturnFailedMessage("校正資訊未填寫完整");
                    }
                    else if (Model.ItemList.Any(x => !x.IsFailed.HasValue) && Model.FileList.Count == 0)
                    {
                        result.ReturnFailedMessage("校正資訊未填寫完整或未檢附校驗報告");
                    }
                    else
                    {
                        var form = db.QA_CALIBRATIONFORM.First(x => x.UNIQUEID == Model.UniqueID);

                        form.STATUS = "3";

                        var time = DateTime.Now;

                        var seq = 1;

                        if (db.QA_CALIBRATIONFORMFLOWLOG.Any(x => x.FORMUNIQUEID == Model.UniqueID))
                        {
                            seq = db.QA_CALIBRATIONFORMFLOWLOG.Max(x => x.SEQ) + 1;
                        }

                        var log = db.QA_CALIBRATIONFORMFLOWLOG.FirstOrDefault(x => x.FORMUNIQUEID == Model.UniqueID && x.FLOWSEQ == 0 && !x.VERIFYTIME.HasValue);

                        if (log != null)
                        {
                            log.VERIFYTIME = time;
                            log.VERIFYRESULT = "Y";
                        }
                        else
                        {
                            db.QA_CALIBRATIONFORMFLOWLOG.Add(new QA_CALIBRATIONFORMFLOWLOG()
                            {
                                FORMUNIQUEID = Model.UniqueID,
                                SEQ = seq,
                                FLOWSEQ = 0,
                                USERID = Account.ID,
                                NOTIFYTIME = time,
                                VERIFYTIME = time,
                                VERIFYRESULT = "Y"
                            });

                            seq++;
                        }

                        db.QA_CALIBRATIONFORMFLOWLOG.Add(new QA_CALIBRATIONFORMFLOWLOG()
                        {
                            FORMUNIQUEID = Model.UniqueID,
                            SEQ = seq,
                            FLOWSEQ = 1,
                            NOTIFYTIME = time
                        });

                        db.SaveChanges();

                        SendVerifyMail(OrganizationList, Model.UniqueID, Model.VHNO);

                        result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.SendApprove, Resources.Resource.Success));
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

        public static RequestResult GetVerifyFormModel(List<Models.Shared.Organization> OrganizationList, string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                result.ReturnData(new VerifyFormModel()
                {
                    UniqueID = UniqueID,
                    FormViewModel = GetFormViewModel(OrganizationList, UniqueID)
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

        private static FormViewModel GetFormViewModel(List<Models.Shared.Organization> OrganizationList, string UniqueID)
        {
            var model = new FormViewModel();

            using (ASEDbEntities db = new ASEDbEntities())
            {
                var form = (from x in db.QA_CALIBRATIONFORM
                            join apply in db.QA_CALIBRATIONAPPLY on x.APPLYUNIQUEID equals apply.UNIQUEID into tmpApply
                            from apply in tmpApply.DefaultIfEmpty()
                            join notify in db.QA_CALIBRATIONNOTIFY on x.NOTIFYUNIQUEID equals notify.UNIQUEID into tmpNotify
                            from notify in tmpNotify.DefaultIfEmpty()
                            join responsor in db.ACCOUNT on x.CALRESPONSORID equals responsor.ID into tmpResponsor
                            from responsor in tmpResponsor.DefaultIfEmpty()
                            join calibrator in db.ACCOUNT on x.CALUSERID equals calibrator.ID into tmpCalibrator
                            from calibrator in tmpCalibrator.DefaultIfEmpty()
                            join lab in db.QA_LAB on x.LABUNIQUEID equals lab.UNIQUEID into tmpLab
                            from lab in tmpLab.DefaultIfEmpty()
                            join jobCalibrator in db.ACCOUNT on x.JOBCALRESPONSERID equals jobCalibrator.ID into tmpJobCalibrator
                            from jobCalibrator in tmpJobCalibrator.DefaultIfEmpty()
                            where x.UNIQUEID == UniqueID
                            select new
                            {
                                x.UNIQUEID,
                                x.HAVEABNORMAL,
                                x.VHNO,
                                x.STATUS,
                                x.ESTCALDATE,
                                x.CALDATE,
                                x.NOTIFYDATE,
                                x.JOBCALRESPONSERID,
                                ApplyUniqueID = apply!=null?apply.UNIQUEID:"",
                                ApplyVHNO = apply!=null?apply.VHNO:"",
                                NotifyUniqueID = notify!=null?notify.UNIQUEID:"",
                                NotifyVHNO=notify!=null?notify.VHNO:"",
                                JobCalibratorName = jobCalibrator != null ? jobCalibrator.NAME : "",
                                x.CALRESPONSORID,
                                ResponsorName = responsor != null ? responsor.NAME : "",
                                x.CALUSERID,
                                CalibratorName = calibrator != null ? calibrator.NAME : "",
                                LabDescription = lab != null ? lab.DESCRIPTION : "",
                                x.TRACEABLENO,
                                x.TAKEJOBDATE,
                                x.TEMPPERATURE,
                                x.HUMIDITY,
                                EquipmentUniqueID = !string.IsNullOrEmpty(x.APPLYUNIQUEID) ? apply.EQUIPMENTUNIQUEID : notify.EQUIPMENTUNIQUEID,
                                CalType = !string.IsNullOrEmpty(x.APPLYUNIQUEID) ? apply.CALTYPE : notify.CALTYPE,
                                CalUnit = !string.IsNullOrEmpty(x.APPLYUNIQUEID) ? apply.CALUNIT : notify.CALUNIT
                            }).First();

                var equipment = EquipmentHelper.Get(OrganizationList, form.EquipmentUniqueID);

                var stepLogList = (from x in db.QA_CALIBRATIONFORMSTEPLOG
                                   join owner in db.ACCOUNT
                                   on x.OWNERID equals owner.ID into tmpOwner
                                   from owner in tmpOwner.DefaultIfEmpty()
                                   join qa in db.ACCOUNT
                                   on x.QAID equals qa.ID into tmpQA
                                   from qa in tmpQA.DefaultIfEmpty()
                                   where x.FORMUNIQUEID == form.UNIQUEID
                                   select new StepLogModel
                                   {
                                       OwnerID = x.OWNERID,
                                       OwnerName = owner != null ? owner.NAME : "",
                                       QAID = x.QAID,
                                       QAName = qa != null ? qa.NAME : "",
                                       Seq = x.SEQ,
                                       Step = x.STEP,
                                       Time = x.TIME.Value
                                   }).OrderBy(x => x.Seq).ToList();

                var log = stepLogList.OrderByDescending(x => x.Seq).FirstOrDefault();

                model = new FormViewModel()
                {
                    VHNO = form.VHNO,
                    Status = new FormStatus(form.STATUS, form.CalType, form.ESTCALDATE, log != null ? log.Step : string.Empty),
                    Equipment = equipment,
                    ApplyUniqueID = form.ApplyUniqueID,
                    ApplyVHNO = form.ApplyVHNO,
                    NotifyUniqueID = form.NotifyUniqueID,
                    NotifyVHNO =form.NotifyVHNO,
                    CalibrateType = form.CalType,
                    CalibrateUnit = form.CalUnit,
                    EstCalibrateDate = form.ESTCALDATE,
                    CalibrateDate = form.CALDATE,
                    ResponsorID = form.CALRESPONSORID,
                    ResponsorName = form.ResponsorName,
                    CalibratorID = form.CALUSERID,
                    CalibratorName = form.CalibratorName,
                    LabDescription = form.LabDescription,
                    JobCalibratorID = form.JOBCALRESPONSERID,
                    JobCalibratorName = form.JobCalibratorName,
                    NotifyTime = form.NOTIFYDATE,
                    TakeJobTime = form.TAKEJOBDATE,
                    TraceableNo = form.TRACEABLENO,
                    HaveAbnormal = form.HAVEABNORMAL == "Y",
                    Temperature = form.TEMPPERATURE,
                    Humidity = form.HUMIDITY,
                    AbnormalFormList = db.QA_ABNORMALFORM.Where(x => x.CALFORMUNIQUEID == form.UNIQUEID).Select(x => new AbnormalFormModel
                    {
                        UniqueID = x.UNIQUEID,
                        VHNO = x.VHNO,
                        Status = x.STATUS,
                        CreateTime = x.CREATETIME.Value,
                        HandlingRemark = x.HANDLINGREMARK
                    }).OrderBy(x => x.CreateTime).ToList(),
                    StepLogList = stepLogList,
                    TakeJobLogList = (from x in db.QA_CALIBRATIONFORMTAKEJOBLOG
                                      join u in db.ACCOUNT
                                      on x.USERID equals u.ID into tmpTakeJobUser
                                      from u in tmpTakeJobUser.DefaultIfEmpty()
                                      where x.FORMUNIQUEID == form.UNIQUEID
                                      select new TakeJobLogModel
                                      {
                                          Seq = x.SEQ,
                                          CalibratorID = x.USERID,
                                          CalibratorName = u != null ? u.NAME : "",
                                          Time = x.TIME.Value
                                      }).OrderBy(x => x.Seq).ToList(),
                    LogList = (from l in db.QA_CALIBRATIONFORMFLOWLOG
                               join u in db.ACCOUNT
                               on l.USERID equals u.ID
                               where l.FORMUNIQUEID == form.UNIQUEID
                               select new LogModel
                               {
                                   Seq = l.SEQ,
                                   FlowSeq = l.FLOWSEQ,
                                   NotifyTime = l.NOTIFYTIME,
                                   VerifyResult = l.VERIFYRESULT,
                                   UserID = l.USERID,
                                   UserName = u.NAME,
                                   VerifyTime = l.VERIFYTIME,
                                   VerifyComment = l.VERIFYCOMMENT
                               }).OrderBy(x => x.VerifyTime).ThenBy(x => x.Seq).ToList(),
                    ItemList = (from x in db.QA_CALIBRATIONFORMDETAIL
                                where x.FORMUNIQUEID == form.UNIQUEID
                                select new DetailItem
                                {
                                    Seq = x.SEQ,
                                    Characteristic = x.CHARACTERISTIC,
                                    CalibrationPoint = x.CALIBRATIONPOINT,
                                    UsingRange = x.USINGRANGE,
                                    RangeTolerance = x.RANGETOLERANCE,
                                    Standard = x.STANDARD,
                                    CalibrateDate = x.CALDATE,
                                    Tolerance = x.TOLERANCE,
                                    ToleranceMark = x.TOLERANCESYMBOL,
                                    Unit = x.UNIT,
                                    ToleranceUnit = x.TOLERANCEUNIT,
                                    ToleranceUnitRate = x.TOLERANCEUNITRATE,
                                    ReadingValue = x.READINGVALUE
                                }).OrderBy(x => x.Seq).ToList(),
                    STDUSEList = (from x in db.QA_CALIBRATIONFORMSTDUSE
                                  join e in db.QA_EQUIPMENT
                                  on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                  join calibrator in db.ACCOUNT
                                  on x.LASTCALUSERID equals calibrator.ID into tmpCalibrator
                                  from calibrator in tmpCalibrator.DefaultIfEmpty()
                                  join lab in db.QA_LAB
                                  on x.LASTLABUNIQUEID equals lab.UNIQUEID into tmpLab
                                  from lab in tmpLab.DefaultIfEmpty()
                                  join i in db.QA_ICHI
                                     on e.ICHIUNIQUEID equals i.UNIQUEID into tmpIchi
                                  from i in tmpIchi.DefaultIfEmpty()
                                  where x.FORMUNIQUEID == form.UNIQUEID
                                  select new STDUSEModel
                                  {
                                      UniqueID = x.EQUIPMENTUNIQUEID,
                                      CalNo = e.CALNO,
                                      LastCalibrateDate = x.LASTCALDATE,
                                      NextCalibrateDate = x.NEXTCALDATE,
                                      CalibratorID = x.LASTCALUSERID,
                                      CalibratorName = calibrator != null ? calibrator.NAME : "",
                                      LabDescription = lab != null ? lab.DESCRIPTION : "",
                                      IchiUniqueID=e.UNIQUEID,
                                      IchiName=i!=null?i.NAME:"",
                                       IchiRemark=e.ICHIREMARK
                                  }).ToList(),
                    FileList = db.QA_CALIBRATIONFORMFILE.Where(f => f.FORMUNIQUEID == UniqueID).ToList().Select(f => new FileModel
                    {
                        Seq = f.SEQ,
                        FileName = f.FILENAME,
                        Extension = f.EXTENSION,
                        Size = f.CONTENTLENGTH,
                        LastModifyTime = f.UPLOADTIME,
                        IsSaved = true
                    }).OrderBy(f => f.LastModifyTime).ToList()
                };

                foreach (var item in model.ItemList)
                {
                    var pattern = string.Format("{0}_{1}_*", UniqueID, item.Seq);

                    var files = Directory.GetFiles(Config.QAFileFolderPath, pattern);

                    if (files != null && files.Count() > 0)
                    {
                        item.PhotoFullPathList = files.ToList();
                    }
                }
            }

            return model;
        }

        public static RequestResult GetUserOptions(List<Models.Shared.UserModel> UserList, string Term, bool IsInit)
        {
            RequestResult result = new RequestResult();

            try
            {
                var query = UserList.Select(x => new Models.ASE.Shared.ASEUserModel
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

        public static RequestResult GetSTDUSEOptions(string Term, bool IsInit,int PageIndex, int PageSize)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = (from e in db.QA_EQUIPMENT
                                 join calibrator in db.ACCOUNT
                                 on e.LASTCALUSERID equals calibrator.ID into tmpCalibrator
                                 from calibrator in tmpCalibrator.DefaultIfEmpty()
                                 join lab in db.QA_LAB
                                 on e.LASTLABUNIQUEID equals lab.UNIQUEID into tmpLab
                                 from lab in tmpLab.DefaultIfEmpty()
                                 join i in db.QA_ICHI
                                     on e.ICHIUNIQUEID equals i.UNIQUEID into tmpIchi
                                 from i in tmpIchi.DefaultIfEmpty()
                                 select new
                                 {
                                     UniqueID = e.UNIQUEID,
                                     CalNo = e.CALNO,
                                     LastCalibrateDate = e.LASTCALDATE,
                                     NextCalibrateDate = e.NEXTCALDATE,
                                     CalibratorID = e.LASTCALUSERID,
                                     CalibratorName = calibrator != null ? calibrator.NAME : "",
                                     LabDescription = lab != null ? lab.DESCRIPTION : "",
                                     IchiUniqueID=e.ICHIUNIQUEID,
                                     IchiName=i!=null?i.NAME:"",
                                     IchiRemark=e.ICHIREMARK
                                 }).AsQueryable();

                    if (!string.IsNullOrEmpty(Term))
                    {
                        if (IsInit)
                        {
                            query = query.Where(x => x.UniqueID == Term);
                        }
                        else
                        {
                            var term = Term.ToLower();

                            query = query.Where(x => x.CalNo.ToLower().Contains(term));
                        }
                    }

                    result.ReturnData(query.Select(x => new STDUSEModel
                    {
                        UniqueID = x.UniqueID,
                        CalNo = x.CalNo,
                        LastCalibrateDate = x.LastCalibrateDate,
                        NextCalibrateDate = x.NextCalibrateDate,
                         CalibratorID=x.CalibratorID,
                          CalibratorName=x.CalibratorName,
                           LabDescription = x.LabDescription,
                           IchiUniqueID=x.IchiUniqueID,
                           IchiName=x.IchiName,
                           IchiRemark=x.IchiRemark
                    }).OrderBy(x=>x.CalNo).ToPagedList(PageIndex, PageSize));
                }

            }
            catch (Exception ex)
            {
                Error err = new Error(MethodInfo.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult ExportQRCode(List<Models.Shared.Organization> OrganizationList, List<Models.Shared.UserModel> UserList, List<string> UniqueIDList, Account Account, Define.EnumExcelVersion ExcelVersion, string fileName)
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

                ISheet sheet = wk.CreateSheet("QRCODE");
                ISheet sheet2 = wk.CreateSheet("QRCODE_NG");



                List<QRCodeItem> dataSource = new List<QRCodeItem>();
                #region fetching data
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query1 = (from f in db.QA_CALIBRATIONFORM
                                 join a in db.QA_CALIBRATIONAPPLY
                                 on f.APPLYUNIQUEID equals a.UNIQUEID
                                 join e in db.QA_EQUIPMENT
                                 on a.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                 join calibrator in db.ACCOUNT
                                 on f.CALUSERID equals calibrator.ID into tmpCalibrator
                                 from calibrator in tmpCalibrator.DefaultIfEmpty()
                                 join lab in db.QA_LAB
                                 on f.LABUNIQUEID equals lab.UNIQUEID into tmpLab
                                 from lab in tmpLab.DefaultIfEmpty()
                                 where UniqueIDList.Contains(f.UNIQUEID)
                                 select new
                                 {
                                     UniqueID = f.UNIQUEID,
                                     HaveAbnormal = f.HAVEABNORMAL,
                                     EquipmentUniqueID = e.UNIQUEID,
                                     EstCalibrateDate = f.ESTCALDATE,
                                     CalibrateDate = f.CALDATE,
                                    CalibrateType= a.CALTYPE,
                                     CalNo = e.CALNO,
                                     CalibratorID = f.CALUSERID,
                                     CalibratorName = calibrator!=null?calibrator.NAME:"",
                                      LabDescription = lab!=null?lab.DESCRIPTION:""
                                 }).ToList();
                   

                    var query2 = (from f in db.QA_CALIBRATIONFORM
                                 join n in db.QA_CALIBRATIONNOTIFY
                                 on f.NOTIFYUNIQUEID equals n.UNIQUEID
                                 join e in db.QA_EQUIPMENT
                                 on n.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                  join calibrator in db.ACCOUNT
                                 on f.CALUSERID equals calibrator.ID into tmpCalibrator
                                  from calibrator in tmpCalibrator.DefaultIfEmpty()
                                  join lab in db.QA_LAB
                                  on f.LABUNIQUEID equals lab.UNIQUEID into tmpLab
                                  from lab in tmpLab.DefaultIfEmpty()
                                 where UniqueIDList.Contains(f.UNIQUEID)
                                 select new
                                 {
                                     UniqueID = f.UNIQUEID,
                                     HaveAbnormal = f.HAVEABNORMAL,
                                     EquipmentUniqueID = e.UNIQUEID,
                                     EstCalibrateDate = f.ESTCALDATE,
                                     CalibrateDate = f.CALDATE,
                                     CalibrateType = n.CALTYPE,
                                     CalNo = e.CALNO,
                                     CalibratorID = f.CALUSERID,
                                     CalibratorName = calibrator != null ? calibrator.NAME : "",
                                     LabDescription = lab != null ? lab.DESCRIPTION : ""
                                 }).ToList();

                    var query = query1.Union(query2);

                    foreach (var q in query)
                    {
                        //var detailList = db.QA_CALIBRATIONFORMDETAIL.Where(x => x.FORMUNIQUEID == q.UniqueID && x.CALDATE.HasValue).Select(x => x.CALDATE).OrderByDescending(x => x).ToList();

                        var calDate = q.EstCalibrateDate;

                        if (q.CalibrateDate.HasValue)
                        {
                            calDate = q.CalibrateDate.Value;
                        }

                        //if (detailList.Count > 0)
                        //{
                        //    calDate = detailList.First().Value;
                        //}

                        var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == q.EquipmentUniqueID);

                        var dueDate = calDate.AddMonths(Convert.ToInt32(equipment.CALCYCLE)).AddDays(-1);

                        var calibrator = string.Empty;

                        if (q.CalibrateType == "EF" || q.CalibrateType == "EL")
                        {
                            calibrator = q.LabDescription;
                        }
                        else
                        {
                            //var a = UserList.First(x => x.ID == q.CalibratorID);

                            //var user = new Models.ASE.Shared.ASEUserModel()
                            //{
                            //    ID = a.ID,
                            //    Name = a.Name,
                            //    Email = a.Email,
                            //    OrganizationDescription = a.OrganizationDescription
                            //};

                            calibrator = q.CalibratorName;
                            //calibrator = !string.IsNullOrEmpty(user.EName) ? user.EName.Replace("_", " ") : user.Name;
                        }
                        
                        dataSource.Add(new QRCodeItem()
                        {
                            SN = q.CalNo,
                            CALDate = DateTimeHelper.DateTime2DateStringWithSeperator(calDate),
                            DueDate = DateTimeHelper.DateTime2DateStringWithSeperator(dueDate),
                            Sign = calibrator,
                            IsFailed = q.HaveAbnormal == "Y"
                        });
                    }

                    var formUniqueIDList = query.Select(x => x.UniqueID).Distinct().ToList();

                    var formList = db.QA_CALIBRATIONFORM.Where(x => formUniqueIDList.Contains(x.UNIQUEID)).ToList();

                    foreach (var form in formList)
                    {
                        form.ISQRCODE = "Y";
                    }

                    db.SaveChanges();
                }
                #endregion

                processQRCodeSheet1(wk, sheet, dataSource.Where(x => x.IsFailed == false).ToList());
                processQRCodeSheet2(OrganizationList, wk, sheet2, dataSource.Where(x => x.IsFailed == true).ToList());

                // Setting print margin
                sheet.SetMargin(MarginType.TopMargin, 0);
                sheet.SetMargin(MarginType.RightMargin, 0);
                sheet.SetMargin(MarginType.LeftMargin, 0);
                sheet.SetMargin(MarginType.BottomMargin, 0);
                sheet.SetMargin(MarginType.HeaderMargin, 0);
                sheet.SetMargin(MarginType.HeaderMargin, 0);

                sheet2.SetMargin(MarginType.TopMargin, 0);
                sheet2.SetMargin(MarginType.RightMargin, 0);
                sheet2.SetMargin(MarginType.LeftMargin, 0);
                sheet2.SetMargin(MarginType.BottomMargin, 0);
                sheet2.SetMargin(MarginType.HeaderMargin, 0);
                sheet2.SetMargin(MarginType.HeaderMargin, 0);


                // Use reflection go call internal method GetCTWorksheet()
                MethodInfo methodInfo = sheet.GetType().GetMethod("GetCTWorksheet", BindingFlags.NonPublic | BindingFlags.Instance);
                var ct = (CT_Worksheet)methodInfo.Invoke(sheet, new object[] { });

                CT_SheetView view = ct.sheetViews.GetSheetViewArray(0);
                view.view = ST_SheetViewType.pageBreakPreview;

                var ct2 = (CT_Worksheet)methodInfo.Invoke(sheet2, new object[] { });
                CT_SheetView view2 = ct2.sheetViews.GetSheetViewArray(0);
                view2.view = ST_SheetViewType.pageBreakPreview;

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

        /// <summary>
        /// create sheet 1 (normal version)
        /// </summary>
        /// <param name="wk"></param>
        /// <param name="sheet"></param>
        /// <param name="dataSource"></param>
        public static void processQRCodeSheet1(IWorkbook wk, ISheet sheet, List<QRCodeItem> dataSource)
        {
            // set zoom size
            sheet.SetZoom(2, 1);

            //主標題
            ICellStyle mainCellStyle = wk.CreateCellStyle();
            IFont mainFont = wk.CreateFont();
            mainFont.FontName = "Trebuchet MS";
            mainFont.FontHeightInPoints = 6;
            mainFont.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
            mainCellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            mainCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            mainCellStyle.SetFont(mainFont);



            //側邊標題
            ICellStyle titleCellStyle = wk.CreateCellStyle();
            IFont titleFont = wk.CreateFont();
            titleFont.FontName = "Trebuchet MS";
            titleFont.FontHeightInPoints = 6;
            titleFont.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
            titleCellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            titleCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
            titleCellStyle.SetFont(titleFont);

            //內容
            ICellStyle contentCellStyle = wk.CreateCellStyle();
            IFont contentFont = wk.CreateFont();
            contentFont.FontName = "Trebuchet MS";
            contentFont.FontHeightInPoints = 6;
            contentFont.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
            contentFont.Underline = FontUnderlineType.Single;
            contentCellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            contentCellStyle.SetFont(contentFont);

            //內容
            ICellStyle content1CellStyle = wk.CreateCellStyle();
            IFont content1Font = wk.CreateFont();
            content1Font.FontName = "Trebuchet MS";
            content1Font.FontHeightInPoints = 5;
            content1Font.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
            content1Font.Underline = FontUnderlineType.Single;
            content1CellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            content1CellStyle.SetFont(content1Font);

            var index = 0;
            var startColumnForPrint = 0;
            var endColumnForPrint = 2;
            var startRowForPrint = 0;//dynamic
            var endRowForPrint = 0;//dynamic

            int columnHeightUnit = 20;
            double contentHeight = 10;


            foreach (var item in dataSource)
            {
                //blank
                var row1 = sheet.CreateRow(index);
                row1.Height = (short)(9 * columnHeightUnit);
                var row1Cell1 = row1.CreateCell(0);
                row1Cell1.SetCellValue("CALIBRATED BY CAL LAB");
                row1Cell1.CellStyle = mainCellStyle;
                row1.CreateCell(1);
                row1.CreateCell(2);
                sheet.AddMergedRegion(new CellRangeAddress(index, index, 0, 2));

                // s/n
                var row2Idx = index + 1;
                var row2 = sheet.CreateRow(row2Idx);
                row2.Height = (short)(contentHeight * columnHeightUnit);
                var row2Cell1 = row2.CreateCell(0);
                row2Cell1.SetCellValue("S/N:");
                row2Cell1.CellStyle = titleCellStyle;



                var row2Cell2 = row2.CreateCell(1);
                row2Cell2.SetCellValue(item.SN);
                row2Cell2.CellStyle = contentCellStyle;

                row2.CreateCell(2);

                // CAL DATE
                var row3Idx = index + 2;
                var row3 = sheet.CreateRow(row3Idx);
                row3.Height = (short)(contentHeight * columnHeightUnit);
                var row3Cell1 = row3.CreateCell(0);
                row3Cell1.SetCellValue("CAL DATE:");
                row3Cell1.CellStyle = titleCellStyle;


                var row3Cell2 = row3.CreateCell(1);
                row3Cell2.SetCellValue(item.CALDate);
                row3Cell2.CellStyle = contentCellStyle;


                row3.CreateCell(2);

                // DUE DATE
                var row4Idx = index + 3;
                var row4 = sheet.CreateRow(row4Idx);
                row4.Height = (short)(contentHeight * columnHeightUnit);
                var row4Cell1 = row4.CreateCell(0);
                row4Cell1.SetCellValue("DUE DATE:");
                row4Cell1.CellStyle = titleCellStyle;


                var row4Cell2 = row4.CreateCell(1);
                row4Cell2.SetCellValue(item.DueDate);
                row4Cell2.CellStyle = contentCellStyle;


                row4.CreateCell(2);

                // SIGN
                var row5Idx = index + 4;
                var row5 = sheet.CreateRow(row5Idx);
                row5.Height = (short)(contentHeight * columnHeightUnit);
                var row5Cell1 = row5.CreateCell(0);
                row5Cell1.SetCellValue("SIGN:");
                row5Cell1.CellStyle = titleCellStyle;


                var row5Cell2 = row5.CreateCell(1);
                row5Cell2.SetCellValue(item.Sign);
                row5Cell2.CellStyle = content1CellStyle;


                row5.CreateCell(2);

                sheet.AddMergedRegion(new CellRangeAddress(row2Idx, row5Idx, 2, 2));

                var barcodeWriter = new BarcodeWriter
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new EncodingOptions
                    {
                        Height = 150,
                        Width = 150,
                        Margin = 1

                    }
                };
                int pictureIdx = 0;
                var path = Path.Combine(Config.TempFolder, "QRCODE_" + Guid.NewGuid().ToString() + ".jpg");
                using (var bitmap = barcodeWriter.Write(item.SN))
                using (var stream = new MemoryStream())
                {


                    bitmap.Save(path);
                    byte[] bytes = System.IO.File.ReadAllBytes(path);
                    pictureIdx = wk.AddPicture(bytes, PictureType.JPEG);
                }

                IDrawing drawing = sheet.CreateDrawingPatriarch();
                IClientAnchor anchor = drawing.CreateAnchor(5, 5, 5, 5, 2, row2Idx, 3, row5Idx + 1);


                //ref http://www.cnblogs.com/firstcsharp/p/4896121.html

                if (anchor != null)
                {
                    anchor.AnchorType = AnchorType.MoveDontResize;
                    drawing.CreatePicture(anchor, pictureIdx);
                }

                index += 5;

            }
            endRowForPrint = index > 1 ? index - 1 : index;
            //定欄位單位寬度
            short columnWidthUnit = 256;
            // cloumn Width
            sheet.SetColumnWidth(0, 6 * columnWidthUnit);
            sheet.SetColumnWidth(1, 7 * columnWidthUnit);
            sheet.SetColumnWidth(2, 6 * columnWidthUnit);




            //set the print area for the first sheet
            wk.SetPrintArea(0, startColumnForPrint, endColumnForPrint, startRowForPrint, endRowForPrint);
        }

        /// <summary>
        /// create sheet 2 (ng version)
        /// </summary>
        /// <param name="wk"></param>
        /// <param name="sheet"></param>
        /// <param name="dataSource"></param>
        public static void processQRCodeSheet2(List<Models.Shared.Organization> OrganizationList, IWorkbook wk, ISheet sheet, List<QRCodeItem> dataSource)
        {
            // set zoom size
            sheet.SetZoom(2, 1);

            //主標題
            ICellStyle warnCellStyle = wk.CreateCellStyle();
            IFont warnFont = wk.CreateFont();
            warnFont.FontName = "Bell MT";
            warnFont.FontHeightInPoints = 14;
            warnFont.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
            warnFont.IsItalic = true;
            warnCellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            warnCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            warnCellStyle.SetFont(warnFont);

            //主標題
            ICellStyle mainCellStyle = wk.CreateCellStyle();
            IFont mainFont = wk.CreateFont();
            mainFont.FontName = "Trebuchet MS";
            mainFont.FontHeightInPoints = 6;
            mainFont.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
            mainCellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            mainCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            mainCellStyle.SetFont(mainFont);



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
            //contentCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
            contentCellStyle.SetFont(contentFont);

            //內容2
            ICellStyle content1CellStyle = wk.CreateCellStyle();
            IFont content1Font = wk.CreateFont();
            content1Font.FontName = "Trebuchet MS";
            content1Font.FontHeightInPoints = 6;
            content1Font.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
            content1Font.Underline = FontUnderlineType.Single;
            content1CellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            content1CellStyle.SetFont(content1Font);


            //底
            ICellStyle footerCellStyle = wk.CreateCellStyle();
            IFont footerFont = wk.CreateFont();
            footerFont.FontName = "Trebuchet MS";
            footerFont.FontHeightInPoints = 7;
            footerFont.Boldweight = (short)NPOI.SS.UserModel.FontBoldWeight.Bold;
            footerCellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Top;
            footerCellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            footerCellStyle.SetFont(footerFont);

            var index = 0;
            var startColumnForPrint = 0;
            var endColumnForPrint = 2;
            var startRowForPrint = 0;//dynamic
            var endRowForPrint = 0;//dynamic

            int columnHeightUnit = 20;
            double contentHeight = 9.75;

            foreach (var item in dataSource)
            {
                //blank
                var row1Idx = index;
                var row1 = sheet.CreateRow(row1Idx);
                row1.Height = (short)(9.75 * columnHeightUnit);
                var row1Cell1 = row1.CreateCell(0);
                row1Cell1.SetCellValue("DO NOT USE");
                row1Cell1.CellStyle = warnCellStyle;
                row1.CreateCell(1);
                row1.CreateCell(2);


                var row2Idx = index + 1;
                var row2 = sheet.CreateRow(row2Idx);
                row2.Height = (short)(9.75 * columnHeightUnit);
                var row2Cell1 = row2.CreateCell(0);

                sheet.AddMergedRegion(new CellRangeAddress(row1Idx, row2Idx, 0, 2));

                // empty
                var row3Idx = index + 2;
                var row3 = sheet.CreateRow(row3Idx);
                row3.Height = (short)(6 * columnHeightUnit);
                var row3Cell1 = row3.CreateCell(0);



                //s/n
                var row4Idx = index + 3;
                var row4 = sheet.CreateRow(row4Idx);
                row4.Height = (short)(contentHeight * columnHeightUnit);
                var row4Cell1 = row4.CreateCell(0);
                row4Cell1.SetCellValue("S/N:");
                row4Cell1.CellStyle = titleCellStyle;
                var row4Cell2 = row4.CreateCell(1);
                row4Cell2.SetCellValue(item.SN);
                row4Cell2.CellStyle = contentCellStyle;
                row4.CreateCell(2);

                // CAL DATE
                var row5Idx = index + 4;
                var row5 = sheet.CreateRow(row5Idx);
                row5.Height = (short)(contentHeight * columnHeightUnit);
                var row5Cell1 = row5.CreateCell(0);
                row5Cell1.SetCellValue("DATE:");
                row5Cell1.CellStyle = titleCellStyle;


                var row5Cell2 = row5.CreateCell(1);
                row5Cell2.SetCellValue(item.CALDate);
                row5Cell2.CellStyle = contentCellStyle;
                row5.CreateCell(2);

                // SIGN
                var row6Idx = index + 5;
                var row6 = sheet.CreateRow(row6Idx);
                row6.Height = (short)(contentHeight * columnHeightUnit);
                var row6Cell1 = row6.CreateCell(0);
                row6Cell1.SetCellValue("SIGN:");
                row6Cell1.CellStyle = titleCellStyle;


                var row6Cell2 = row6.CreateCell(1);
                row6Cell2.SetCellValue(item.Sign);
                row6Cell2.CellStyle = content1CellStyle;


                row6.CreateCell(2);

                sheet.AddMergedRegion(new CellRangeAddress(row4Idx, row6Idx, 2, 2));

                var barcodeWriter = new BarcodeWriter
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new EncodingOptions
                    {
                        Height = 150,
                        Width = 150,
                        Margin = 1

                    }
                };
                int pictureIdx = 0;
                var path = Path.Combine(Config.TempFolder, "QRCODE_" + Guid.NewGuid().ToString() + ".jpg");
                using (var bitmap = barcodeWriter.Write(item.SN))
                using (var stream = new MemoryStream())
                {


                    bitmap.Save(path);
                    byte[] bytes = System.IO.File.ReadAllBytes(path);
                    pictureIdx = wk.AddPicture(bytes, PictureType.JPEG);
                }

                IDrawing drawing = sheet.CreateDrawingPatriarch();
                IClientAnchor anchor = drawing.CreateAnchor(5, 5, 5, 5, 2, row4Idx, 3, row6Idx + 1);


                //ref http://www.cnblogs.com/firstcsharp/p/4896121.html

                if (anchor != null)
                {
                    anchor.AnchorType = AnchorType.MoveDontResize;
                    drawing.CreatePicture(anchor, pictureIdx);
                }


                // empty
                var row7Idx = index + 6;
                var row7 = sheet.CreateRow(row7Idx);
                row7.Height = (short)(10 * columnHeightUnit);
                var row7Cell1 = row7.CreateCell(0);

                // footer
                var row8Idx = index + 7;
                var row8 = sheet.CreateRow(row8Idx);
                row8.Height = (short)(9.75 * columnHeightUnit);
                var row8Cell1 = row8.CreateCell(0);
                row8Cell1.SetCellValue("RE-CALIBRATE BEFORE USING");
                row8Cell1.CellStyle = footerCellStyle;
                row8.CreateCell(1);
                row8.CreateCell(2);

                sheet.AddMergedRegion(new CellRangeAddress(row8Idx, row8Idx, 0, 2));

                index += 8;

            }
            endRowForPrint = index > 1 ? index - 1 : index;
            //定欄位單位寬度
            short columnWidthUnit = 256;
            // cloumn Width
            sheet.SetColumnWidth(0, 5 * columnWidthUnit);
            sheet.SetColumnWidth(1, 8 * columnWidthUnit);
            sheet.SetColumnWidth(2, 7 * columnWidthUnit);




            //set the print area for the first sheet
            wk.SetPrintArea(1, startColumnForPrint, endColumnForPrint, startRowForPrint, endRowForPrint);
        }
        #endregion

        public static RequestResult Approve(List<Models.Shared.Organization> OrganizationList, VerifyFormModel Model, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = db.QA_CALIBRATIONFORM.First(x => x.UNIQUEID == Model.UniqueID);

                    form.STATUS = "5";

                    var log = db.QA_CALIBRATIONFORMFLOWLOG.First(x => x.FORMUNIQUEID == form.UNIQUEID && x.FLOWSEQ == 1 && !x.VERIFYTIME.HasValue);

                    log.USERID = Account.ID;
                    log.VERIFYTIME = DateTime.Now;
                    log.VERIFYRESULT = "Y";
                    log.VERIFYCOMMENT = Model.FormInput.Comment;

                    var equipmentUniqueID = string.Empty;

                    if (!string.IsNullOrEmpty(form.APPLYUNIQUEID))
                    {
                        var apply = db.QA_CALIBRATIONAPPLY.First(x => x.UNIQUEID == form.APPLYUNIQUEID);

                        equipmentUniqueID = apply.EQUIPMENTUNIQUEID;
                    }

                    if (!string.IsNullOrEmpty(form.NOTIFYUNIQUEID))
                    {
                        var notify = db.QA_CALIBRATIONNOTIFY.First(x => x.UNIQUEID == form.NOTIFYUNIQUEID);

                        equipmentUniqueID = notify.EQUIPMENTUNIQUEID;
                    }

                    var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == equipmentUniqueID);

                    var calibrateDate = form.ESTCALDATE;

                    if (form.CALDATE.HasValue)
                    {
                        calibrateDate = form.CALDATE.Value;
                    }

                    //var calibrateDate = db.QA_CALIBRATIONFORMDETAIL.Where(x => x.FORMUNIQUEID == form.UNIQUEID).Select(x => x.CALDATE).OrderByDescending(x => x).FirstOrDefault();

                    //if (!calibrateDate.HasValue)
                    //{
                    //    calibrateDate = form.ESTCALDATE;
                    //}

                    var nextCalibrateDate = calibrateDate.AddMonths(Convert.ToInt32(equipment.CALCYCLE)).AddDays(-1);

                    equipment.LASTCALUSERID = form.CALRESPONSORID;
                    equipment.LASTLABUNIQUEID = form.LABUNIQUEID;
                    equipment.LASTCALDATE = calibrateDate;
                    equipment.NEXTCALDATE = nextCalibrateDate;

                    if (equipment.MSA == "Y" && !db.QA_CALIBRATIONNOTIFY.Any(x=>x.EQUIPMENTUNIQUEID==equipment.UNIQUEID))
                    {
                        var createTime = DateTime.Now;

                        var msaDate = calibrateDate.AddMonths(3);

                        var vhnoPrefix = string.Format("MSA{0}", msaDate.ToString("yyyyMM").Substring(2));

                        var vhnoSeq = 1;

                        var query = db.QA_MSAFORM.Where(x => x.VHNO.StartsWith(vhnoPrefix)).OrderByDescending(x => x.VHNO).ToList();

                        if (query.Count > 0)
                        {
                            vhnoSeq = int.Parse(query.First().VHNO.Substring(7, 4)) + 1;
                        }

                        var vhno = string.Format("{0}{1}", vhnoPrefix, vhnoSeq.ToString().PadLeft(4, '0'));

                        var detailList = (from x in db.QA_EQUIPMENTMSADETAIL
                                          join c in db.QA_MSACHARACTERISTICS
                                          on x.MSACHARACTERISITICUNIQUEID equals c.UNIQUEID into tmpCharacteristic
                                          from c in tmpCharacteristic.DefaultIfEmpty()
                                          join u in db.QA_MSAUNIT
                                          on x.MSAUNITUNIQUEID equals u.UNIQUEID into tmpUnit
                                          from u in tmpUnit.DefaultIfEmpty()
                                          where x.EQUIPMENTUNIQUEID == equipment.UNIQUEID
                                          select new
                                          {
                                              x.SEQ,
                                              x.LOWERRANGE,
                                              x.UPPERRANGE,
                                              CharacteristicName = x.MSACHARACTERISITICUNIQUEID == Define.OTHER ? x.MSACHARACTERISITICREMARK : (c != null ? c.NAME : ""),
                                              UnitDescription = x.MSAUNITUNIQUEID == Define.OTHER ? x.MSAUNITREMARK : (u != null ? u.DESCRIPTION : "")
                                          }).ToList();

                        var equipmentModel = EquipmentHelper.Get(OrganizationList, equipment.UNIQUEID);

                        if (detailList.Count > 1)
                        {
                            var subSeq = 1;

                            foreach (var detail in detailList)
                            {
                                var subVhno = string.Format("{0}-{1}", vhno, subSeq);

                                var formUniqueID = Guid.NewGuid().ToString();

                                db.QA_MSAFORM.Add(new QA_MSAFORM()
                                {
                                    UNIQUEID = formUniqueID,
                                    VHNO = subVhno,
                                    EQUIPMENTUNIQUEID = equipment.UNIQUEID,
                                    STATUS = "1",
                                    LOWERRANGE = detail.LOWERRANGE,
                                    UPPERRANGE = detail.UPPERRANGE,
                                    MSARESPONSORID = equipment.PEID,
                                    CREATETIME = createTime,
                                    CHARACTERISITIC = detail.CharacteristicName,
                                    UNIT = detail.UnitDescription,
                                    ESTMSADATE = msaDate,
                                    STATION = equipmentModel.MSAStation,
                                    MSAICHI = equipmentModel.MSAIchi,
                                    TYPE = equipmentModel.MSAType,
                                    SUBTYPE = equipmentModel.MSASubType
                                });

                                subSeq++;
                            }
                        }
                        else
                        {
                            var formUniqueID = Guid.NewGuid().ToString();

                            var detail = detailList.First();

                            db.QA_MSAFORM.Add(new QA_MSAFORM()
                            {
                                UNIQUEID = formUniqueID,
                                VHNO = vhno,
                                EQUIPMENTUNIQUEID = equipment.UNIQUEID,
                                STATUS = "1",
                                LOWERRANGE = detail.LOWERRANGE,
                                UPPERRANGE = detail.UPPERRANGE,
                                MSARESPONSORID = equipment.PEID,
                                CREATETIME = createTime,
                                CHARACTERISITIC = detail.CharacteristicName,
                                UNIT = detail.UnitDescription,
                                ESTMSADATE = msaDate,
                                STATION = equipmentModel.MSAStation,
                                MSAICHI = equipmentModel.MSAIchi,
                                TYPE = equipmentModel.MSAType,
                                SUBTYPE = equipmentModel.MSASubType
                            });
                        }
                    }

                    db.SaveChanges();

                    SendFinishedMail(OrganizationList, form.UNIQUEID, form.VHNO);
                }

                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Verify, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static RequestResult Reject(List<Models.Shared.Organization> OrganizationList, VerifyFormModel Model, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = db.QA_CALIBRATIONFORM.First(x => x.UNIQUEID == Model.UniqueID);

                    form.STATUS = "4";

                    var seq = db.QA_CALIBRATIONFORMFLOWLOG.Where(x => x.FORMUNIQUEID == form.UNIQUEID).Max(x => x.SEQ) + 1;

                    var log = db.QA_CALIBRATIONFORMFLOWLOG.FirstOrDefault(x => x.FORMUNIQUEID == Model.UniqueID && x.FLOWSEQ == 1 && !x.VERIFYTIME.HasValue);

                    if (log != null)
                    {
                        log.USERID = Account.ID;
                        log.VERIFYTIME = DateTime.Now;
                        log.VERIFYRESULT = "N";
                        log.VERIFYCOMMENT = Model.FormInput.Comment;
                    }
                    else
                    {
                        db.QA_CALIBRATIONFORMFLOWLOG.Add(new QA_CALIBRATIONFORMFLOWLOG()
                        {
                            FORMUNIQUEID = form.UNIQUEID,
                            SEQ = seq,
                            FLOWSEQ = 1,
                            NOTIFYTIME = DateTime.Now,
                            USERID = Account.ID,
                            VERIFYTIME = DateTime.Now,
                            VERIFYRESULT = "N",
                            VERIFYCOMMENT = "退回重新執行"
                        });

                        seq++;
                    }
                    

                    db.QA_CALIBRATIONFORMFLOWLOG.Add(new QA_CALIBRATIONFORMFLOWLOG()
                    {
                        FORMUNIQUEID = form.UNIQUEID,
                        SEQ = seq,
                        FLOWSEQ = 0,
                        NOTIFYTIME = DateTime.Now,
                        USERID = form.CALRESPONSORID
                    });

                    db.SaveChanges();

                    SendRejectMail(OrganizationList, form.UNIQUEID, form.VHNO, new List<string> { form.CALRESPONSORID });
                }

                result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Reject, Resources.Resource.Success));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        public static FileDownloadModel GetFile(string FormUniqueID, int Seq)
        {
            var model = new FileDownloadModel();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var file = db.QA_CALIBRATIONFORMFILE.First(x => x.FORMUNIQUEID == FormUniqueID && x.SEQ == Seq);

                    model = new FileDownloadModel()
                    {
                        FormUniqueID = file.FORMUNIQUEID,
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

        public static void SendRemindMail(List<Models.Shared.Organization> OrganizationList, string UniqueID, string VHNO, int Days, bool IsDelay)
        {
            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var userList = db.USERAUTHGROUP.Where(x => x.AUTHGROUPID == "QA-Verify").Select(x => x.USERID).ToList();

                    if (userList != null && userList.Count > 0)
                    {
                        if (IsDelay)
                        {
                            SendVerifyMail(OrganizationList, UniqueID, string.Format("[逾期][簽核通知][{0}天][{1}]儀器校驗執行單", Days, VHNO), userList);
                        }
                        else
                        {
                            SendVerifyMail(OrganizationList, UniqueID, string.Format("[跟催][簽核通知][{0}天][{1}]儀器校驗執行單", Days, VHNO), userList);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public static void SendVerifyMail(List<Models.Shared.Organization> OrganizationList, string UniqueID, string VHNO)
        {
            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var userList = db.USERAUTHGROUP.Where(x => x.AUTHGROUPID == "QA-Verify").Select(x => x.USERID).ToList();

                    if (userList != null && userList.Count > 0)
                    {
                        SendVerifyMail(OrganizationList, UniqueID, string.Format("[簽核通知][{0}]儀器校驗執行單", VHNO), userList);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public static void SendFinishedMail(List<Models.Shared.Organization> OrganizationList, string UniqueID, string VHNO)
        {
            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var form = db.QA_CALIBRATIONFORM.First(x => x.UNIQUEID == UniqueID);

                    var userList = new List<string>();

                    userList.Add(form.JOBCALRESPONSERID);
                    userList.Add(form.CALUSERID);

                    if (!string.IsNullOrEmpty(form.APPLYUNIQUEID))
                    { 
                        var apply  = db.QA_CALIBRATIONAPPLY.First(x=>x.UNIQUEID==form.APPLYUNIQUEID);

                        var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == apply.EQUIPMENTUNIQUEID);

                        userList.Add(equipment.OWNERID);
                        userList.Add(equipment.OWNERMANAGERID);
                        userList.Add(equipment.PEID);
                        userList.Add(equipment.PEMANAGERID);
                    }

                    if (!string.IsNullOrEmpty(form.NOTIFYUNIQUEID))
                    {
                        var notify = db.QA_CALIBRATIONNOTIFY.First(x => x.UNIQUEID == form.NOTIFYUNIQUEID);

                        var equipment = db.QA_EQUIPMENT.First(x => x.UNIQUEID == notify.EQUIPMENTUNIQUEID);

                        userList.Add(equipment.OWNERID);
                        userList.Add(equipment.OWNERMANAGERID);
                        userList.Add(equipment.PEID);
                        userList.Add(equipment.PEMANAGERID);
                    }

                    if (userList != null && userList.Count > 0)
                    {
                        SendVerifyMail(OrganizationList, UniqueID, string.Format("[完成通知][{0}]儀器校驗執行單", VHNO), userList);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private static void SendVerifyMail(List<Models.Shared.Organization> OrganizationList, string UniqueID, string Subject, List<string> UserList)
        {
            try
            {
                var mailAddressList = new List<MailAddress>();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var model = GetFormViewModel(OrganizationList, UniqueID);
                    
                    foreach (var u in UserList)
                    {
                        var user = db.ACCOUNT.FirstOrDefault(x => x.ID == u);

                        if (user != null && !string.IsNullOrEmpty(user.EMAIL))
                        {
                            mailAddressList.Add(new MailAddress(user.EMAIL, user.NAME));
                        }
                    }

                    if (mailAddressList.Count > 0)
                    {
                        var th = "<th style=\"width:150px;border:1px solid #333;padding:8px;text-align:right;color:#707070;background: #F4F4F4;\">{0}</th>";
                        var td = "<td style=\"width:400px;border:1px solid #333;padding:8px;color:#707070;\">{0}</td>";

                        var sb = new StringBuilder();

                        sb.Append("<table style=\"1px solid #ddd;font-size:13px;border-collapse:collapse;\">");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "單號"));
                        sb.Append(string.Format(td, model.VHNO));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "廠別"));
                        sb.Append(string.Format(td, model.Equipment.Factory));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "部門"));
                        sb.Append(string.Format(td, model.Equipment.OrganizationDescription));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "序號"));
                        sb.Append(string.Format(td, model.Equipment.SerialNo));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "儀器名稱"));
                        sb.Append(string.Format(td, model.Equipment.IchiDisplay));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "廠牌"));
                        sb.Append(string.Format(td, model.Equipment.Brand));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "型號"));
                        sb.Append(string.Format(td, model.Equipment.Model));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "類別"));
                        sb.Append(string.Format(td, model.CalibrateTypeDisplay));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "校驗負責單位"));
                        sb.Append(string.Format(td, model.CalibrateUnitDisplay));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "預計校驗日期"));
                        sb.Append(string.Format(td, model.EstCalibrateDateString));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "派工人員"));
                        sb.Append(string.Format(td, model.JobCalibrator));
                        sb.Append("</tr>");

                        if (!string.IsNullOrEmpty(model.Responsor))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "校驗負責人員"));
                            sb.Append(string.Format(td, model.Responsor));
                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(model.Calibrator))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "校驗人員"));
                            sb.Append(string.Format(td, model.Calibrator));
                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(model.CalibrateDateString))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "實際校驗日期"));
                            sb.Append(string.Format(td, model.CalibrateDateString));
                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(model.Equipment.OwnerID))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "設備負責人"));
                            sb.Append(string.Format(td, model.Equipment.Owner));
                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(model.Equipment.OwnerManagerID))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "設備負責人主管"));
                            sb.Append(string.Format(td, model.Equipment.OwnerManager));
                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(model.Equipment.PEID))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "製程負責人"));
                            sb.Append(string.Format(td, model.Equipment.PE));
                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(model.Equipment.PEManagerID))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "製程負責人主管"));
                            sb.Append(string.Format(td, model.Equipment.PEManager));
                            sb.Append("</tr>");
                        }

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "連結"));
                        sb.Append(string.Format(td, string.Format("<a href=\"http://ASECL-eINSPRD01/FEM.Portal/Home/Index?ReturnUrl=http://ASECL-eINSPRD01/FEM/zh-tw/Customized_ASE_QA/CalibrationForm/Index?VHNO={0}\">連結</a>", model.VHNO)));
                        sb.Append("</tr>");

                        sb.Append("</table>");

                        MailHelper.SendMail(mailAddressList, Subject, sb.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public static void SendRejectMail(List<Models.Shared.Organization> OrganizationList, string UniqueID, string VHNO, List<string> UserList)
        {
            try
            {
                SendVerifyMail(OrganizationList, UniqueID, string.Format("[退回修正通知][{0}]儀器校驗執行單", VHNO), UserList);
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public static void GenExcel(List<Models.Shared.Organization> OrganizationList, string UniqueID)
        {
            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var model = GetDetailViewModel(OrganizationList, UniqueID).Data as DetailViewModel;

                    int stduseCount = model.FormViewModel.STDUSEList.Count;

                    if (stduseCount < 3)
                    {
                        stduseCount = 3;
                    }

                    var workBook = new HSSFWorkbook();

                    #region Header Font
                    var headerFont1 = workBook.CreateFont();
                    headerFont1.FontName = "Times New Roman";
                    headerFont1.FontHeightInPoints = 12;
                    headerFont1.Boldweight = (short)FontBoldWeight.Bold;
                    headerFont1.Color = NPOI.HSSF.Util.HSSFColor.Blue.Index;

                    var headerFont2 = workBook.CreateFont();
                    headerFont2.FontName = "Times New Roman";
                    headerFont2.FontHeightInPoints = 20;
                    headerFont2.Boldweight = (short)FontBoldWeight.Bold;
                    headerFont2.Color = NPOI.HSSF.Util.HSSFColor.Black.Index;

                    var headerFont3 = workBook.CreateFont();
                    headerFont3.FontName = "Times New Roman";
                    headerFont3.FontHeightInPoints = 12;
                    headerFont3.Boldweight = (short)FontBoldWeight.Bold;
                    headerFont3.Color = NPOI.HSSF.Util.HSSFColor.Black.Index;

                    var headerFont4 = workBook.CreateFont();
                    headerFont4.FontName = "Times New Roman";
                    headerFont4.FontHeightInPoints = 10;
                    headerFont4.Boldweight = (short)FontBoldWeight.Bold;
                    headerFont4.Color = NPOI.HSSF.Util.HSSFColor.Black.Index;
                    headerFont4.Underline = FontUnderlineType.Single;
                    #endregion

                    #region Header Style
                    var headerStyle1 = workBook.CreateCellStyle();
                    headerStyle1.SetFont(headerFont1);
                    headerStyle1.Alignment = HorizontalAlignment.Right;
                    headerStyle1.VerticalAlignment = VerticalAlignment.Center;
                    headerStyle1.BorderTop = BorderStyle.None;
                    headerStyle1.BorderBottom = BorderStyle.None;
                    headerStyle1.BorderLeft = BorderStyle.None;
                    headerStyle1.BorderRight = BorderStyle.None;

                    var headerStyle2 = workBook.CreateCellStyle();
                    headerStyle2.SetFont(headerFont2);
                    headerStyle2.Alignment = HorizontalAlignment.Left;
                    headerStyle2.VerticalAlignment = VerticalAlignment.Center;
                    headerStyle2.BorderTop = BorderStyle.None;
                    headerStyle2.BorderBottom = BorderStyle.None;
                    headerStyle2.BorderLeft = BorderStyle.None;
                    headerStyle2.BorderRight = BorderStyle.None;

                    var headerStyle3 = workBook.CreateCellStyle();
                    headerStyle3.SetFont(headerFont3);
                    headerStyle3.Alignment = HorizontalAlignment.Center;
                    headerStyle3.VerticalAlignment = VerticalAlignment.Center;
                    headerStyle3.BorderTop = BorderStyle.None;
                    headerStyle3.BorderBottom = BorderStyle.None;
                    headerStyle3.BorderLeft = BorderStyle.None;
                    headerStyle3.BorderRight = BorderStyle.None;

                    var headerStyle4 = workBook.CreateCellStyle();
                    headerStyle4.SetFont(headerFont4);
                    headerStyle4.Alignment = HorizontalAlignment.Center;
                    headerStyle4.VerticalAlignment = VerticalAlignment.Center;
                    headerStyle4.BorderTop = BorderStyle.None;
                    headerStyle4.BorderBottom = BorderStyle.None;
                    headerStyle4.BorderLeft = BorderStyle.None;
                    headerStyle4.BorderRight = BorderStyle.None;
                    #endregion

                    #region Cell Style
                    var cellFont = workBook.CreateFont();
                    cellFont.FontName = "Times New Roman";
                    cellFont.FontHeightInPoints = 8;
                    cellFont.Boldweight = (short)FontBoldWeight.Normal;
                    cellFont.Color = NPOI.HSSF.Util.HSSFColor.Black.Index;

                    var cellStyle = workBook.CreateCellStyle();
                    cellStyle.SetFont(cellFont);
                    cellStyle.Alignment = HorizontalAlignment.Left;
                    cellStyle.VerticalAlignment = VerticalAlignment.Center;
                    cellStyle.BorderTop = BorderStyle.Thin;
                    cellStyle.BorderBottom = BorderStyle.Thin;
                    cellStyle.BorderLeft = BorderStyle.Thin;
                    cellStyle.BorderRight = BorderStyle.Thin;

                    var cellStyleAlignCenter = workBook.CreateCellStyle();
                    cellStyleAlignCenter.SetFont(cellFont);
                    cellStyleAlignCenter.Alignment = HorizontalAlignment.Center;
                    cellStyleAlignCenter.VerticalAlignment = VerticalAlignment.Center;
                    cellStyleAlignCenter.BorderTop = BorderStyle.Thin;
                    cellStyleAlignCenter.BorderBottom = BorderStyle.Thin;
                    cellStyleAlignCenter.BorderLeft = BorderStyle.Thin;
                    cellStyleAlignCenter.BorderRight = BorderStyle.Thin;

                    var cellStyleBorderTop = workBook.CreateCellStyle();
                    cellStyleBorderTop.SetFont(cellFont);
                    cellStyleBorderTop.Alignment = HorizontalAlignment.Center;
                    cellStyleBorderTop.VerticalAlignment = VerticalAlignment.Center;
                    cellStyleBorderTop.BorderTop = BorderStyle.Thin;
                    cellStyleBorderTop.BorderBottom = BorderStyle.None;
                    cellStyleBorderTop.BorderLeft = BorderStyle.Thin;
                    cellStyleBorderTop.BorderRight = BorderStyle.Thin;

                    var cellStyleBorderBottom = workBook.CreateCellStyle();
                    cellStyleBorderBottom.SetFont(cellFont);
                    cellStyleBorderBottom.Alignment = HorizontalAlignment.Center;
                    cellStyleBorderBottom.VerticalAlignment = VerticalAlignment.Center;
                    cellStyleBorderBottom.BorderTop = BorderStyle.None;
                    cellStyleBorderBottom.BorderBottom = BorderStyle.Thin;
                    cellStyleBorderBottom.BorderLeft = BorderStyle.Thin;
                    cellStyleBorderBottom.BorderRight = BorderStyle.Thin;
                    #endregion

                    var worksheet = workBook.CreateSheet(model.FormViewModel.Equipment.CalNo);

                    IRow row;

                    ICell cell;

                    #region Row 0
                    row = worksheet.CreateRow(0);

                    cell = row.CreateCell(0);
                    cell.CellStyle = headerStyle1;
                    cell.SetCellValue("ASECL Confidential / Security-C");

                    worksheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 12));
                    #endregion

                    #region Row 1
                    row = worksheet.CreateRow(1);

                    cell = row.CreateCell(0);
                    cell.CellStyle = headerStyle2;
                    cell.SetCellValue("ASECL");
                    worksheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 2));

                    cell = row.CreateCell(3);
                    cell.CellStyle = headerStyle3;
                    cell.SetCellValue("METROLOGY / CALIBRATION LABORATORY");
                    worksheet.AddMergedRegion(new CellRangeAddress(1, 1, 3, 12));
                    #endregion

                    #region Row 2
                    row = worksheet.CreateRow(2);

                    cell = row.CreateCell(0);
                    cell.CellStyle = headerStyle4;
                    cell.SetCellValue("INSTRUMENT CALIBRATION RECORD");
                    worksheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 12));
                    #endregion

                    #region Row 4
                    row = worksheet.CreateRow(4);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("BRAND");
                    
                    cell = row.CreateCell(1);
                    cell.CellStyle = cellStyle;

                    worksheet.AddMergedRegion(new CellRangeAddress(4, 4, 0, 1));

                    cell = row.CreateCell(2);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(model.FormViewModel.Equipment.Brand);

                    cell = row.CreateCell(3);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(4);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(5);
                    cell.CellStyle = cellStyle;

                    worksheet.AddMergedRegion(new CellRangeAddress(4, 4, 2, 5));

                    cell = row.CreateCell(6);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("DESCRIPTION");

                    cell = row.CreateCell(7);
                    cell.CellStyle = cellStyle;

                    worksheet.AddMergedRegion(new CellRangeAddress(4, 4, 6, 7));

                    cell = row.CreateCell(8);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(model.FormViewModel.Equipment.IchiDisplay);

                    cell = row.CreateCell(9);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(10);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(11);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(12);
                    cell.CellStyle = cellStyle;

                    worksheet.AddMergedRegion(new CellRangeAddress(4, 4, 8, 12));
                    #endregion

                    #region Row 5
                    row = worksheet.CreateRow(5);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("ASSET NO.");

                    cell = row.CreateCell(1);
                    cell.CellStyle = cellStyle;

                    worksheet.AddMergedRegion(new CellRangeAddress(5, 5, 0, 1));

                    cell = row.CreateCell(2);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(model.FormViewModel.Equipment.CalNo);

                    cell = row.CreateCell(3);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(4);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(5);
                    cell.CellStyle = cellStyle;

                    worksheet.AddMergedRegion(new CellRangeAddress(5, 5, 2, 5));

                    cell = row.CreateCell(6);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("DEPARTMENT");

                    cell = row.CreateCell(7);
                    cell.CellStyle = cellStyle;

                    worksheet.AddMergedRegion(new CellRangeAddress(5, 5, 6, 7));

                    cell = row.CreateCell(8);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(model.FormViewModel.Equipment.Factory);

                    cell = row.CreateCell(9);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(10);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(11);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(12);
                    cell.CellStyle = cellStyle;

                    worksheet.AddMergedRegion(new CellRangeAddress(5, 5, 8, 12));
                    #endregion

                    #region Row 6
                    row = worksheet.CreateRow(6);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("MODEL NO.");

                    cell = row.CreateCell(1);
                    cell.CellStyle = cellStyle;

                    worksheet.AddMergedRegion(new CellRangeAddress(6, 6, 0, 1));

                    cell = row.CreateCell(2);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(model.FormViewModel.Equipment.Model);

                    cell = row.CreateCell(3);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(4);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(5);
                    cell.CellStyle = cellStyle;

                    worksheet.AddMergedRegion(new CellRangeAddress(6, 6, 2, 5));

                    cell = row.CreateCell(6);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("RESPONSIBLE");

                    cell = row.CreateCell(7);
                    cell.CellStyle = cellStyle;

                    worksheet.AddMergedRegion(new CellRangeAddress(6, 6, 6, 7));

                    cell = row.CreateCell(8);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(model.FormViewModel.Equipment.Owner);

                    cell = row.CreateCell(9);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(10);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(11);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(12);
                    cell.CellStyle = cellStyle;

                    worksheet.AddMergedRegion(new CellRangeAddress(6, 6, 8, 12));
                    #endregion

                    #region Row 7
                    row = worksheet.CreateRow(7);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("SERIAL NO.");

                    cell = row.CreateCell(1);
                    cell.CellStyle = cellStyle;

                    worksheet.AddMergedRegion(new CellRangeAddress(7,7, 0, 1));

                    cell = row.CreateCell(2);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue(model.FormViewModel.Equipment.SerialNo);

                    cell = row.CreateCell(3);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(4);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(5);
                    cell.CellStyle = cellStyle;

                    worksheet.AddMergedRegion(new CellRangeAddress(7,7, 2, 5));

                    cell = row.CreateCell(6);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("CAL. FREQ.");

                    cell = row.CreateCell(7);
                    cell.CellStyle = cellStyle;

                    worksheet.AddMergedRegion(new CellRangeAddress(7,7, 6, 7));

                    cell = row.CreateCell(8);
                    cell.CellStyle = cellStyle;
                    if (model.FormViewModel.Equipment.CalCycle.HasValue)
                    {
                        cell.SetCellValue(model.FormViewModel.Equipment.CalCycle.Value);
                    }

                    cell = row.CreateCell(9);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(10);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(11);
                    cell.CellStyle = cellStyle;
                    cell = row.CreateCell(12);
                    cell.CellStyle = cellStyle;

                    worksheet.AddMergedRegion(new CellRangeAddress(7,7, 8, 12));
                    #endregion

                    #region Row 8
                    row = worksheet.CreateRow(8);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyle;
                    cell.SetCellValue("STD.USED.");

                    cell = row.CreateCell(1);
                    cell.CellStyle = cellStyle;

                    worksheet.AddMergedRegion(new CellRangeAddress(8, 8 + stduseCount, 0, 1));

                    cell = row.CreateCell(2);
                    cell.CellStyle = cellStyleAlignCenter;
                    cell.SetCellValue("CAL No.");

                    cell = row.CreateCell(3);
                    cell.CellStyle = cellStyleAlignCenter;
                    cell.SetCellValue("Description");

                    cell = row.CreateCell(4);
                    cell.CellStyle = cellStyleAlignCenter;
                    cell = row.CreateCell(5);
                    cell.CellStyle = cellStyleAlignCenter;

                    worksheet.AddMergedRegion(new CellRangeAddress(8, 8, 3, 5));

                    cell = row.CreateCell(6);
                    cell.CellStyle = cellStyleAlignCenter;
                    cell.SetCellValue("Cal Date");

                    cell = row.CreateCell(7);
                    cell.CellStyle = cellStyleAlignCenter;

                    worksheet.AddMergedRegion(new CellRangeAddress(8, 8, 6, 7));

                    cell = row.CreateCell(8);
                    cell.CellStyle = cellStyleAlignCenter;
                    cell.SetCellValue("Next Due Date");

                    cell = row.CreateCell(9);
                    cell.CellStyle = cellStyleAlignCenter;
                    cell = row.CreateCell(10);
                    cell.CellStyle = cellStyleAlignCenter;

                    worksheet.AddMergedRegion(new CellRangeAddress(8, 8, 8, 10));

                    cell = row.CreateCell(11);
                    cell.CellStyle = cellStyleAlignCenter;
                    cell.SetCellValue("Calibration laboratory");

                    cell = row.CreateCell(12);
                    cell.CellStyle = cellStyleAlignCenter;

                    worksheet.AddMergedRegion(new CellRangeAddress(8, 8, 11, 12));
                    #endregion

                    var rowIndex = 9;

                    #region STDUSE
                    for (int i = 0; i < stduseCount; i++)
                    {
                        row = worksheet.CreateRow(rowIndex);

                        STDUSEModel stduse = null;

                        if (model.FormViewModel.STDUSEList.Count > i)
                        {
                            stduse = model.FormViewModel.STDUSEList[i];
                        }

                        cell = row.CreateCell(0);
                        cell.CellStyle = cellStyle;
                        cell = row.CreateCell(1);
                        cell.CellStyle = cellStyle;
                        cell = row.CreateCell(2);
                        cell.CellStyle = cellStyleAlignCenter;
                        cell.SetCellValue(stduse != null ? stduse.CalNo : "");
                        cell = row.CreateCell(3);
                        cell.CellStyle = cellStyleAlignCenter;
                        cell.SetCellValue(stduse != null ? stduse.IchiDisplay : "");
                        cell = row.CreateCell(4);
                        cell.CellStyle = cellStyleAlignCenter;
                        cell = row.CreateCell(5);
                        cell.CellStyle = cellStyleAlignCenter;
                        worksheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 3, 5));
                        cell = row.CreateCell(6);
                        cell.CellStyle = cellStyleAlignCenter;
                        cell.SetCellValue(stduse != null ? stduse.LastCalibrateDateString : "");
                        cell = row.CreateCell(7);
                        cell.CellStyle = cellStyleAlignCenter;
                        worksheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 6, 7));
                        cell = row.CreateCell(8);
                        cell.CellStyle = cellStyleAlignCenter;
                        cell.SetCellValue(stduse != null ? stduse.NextCalibrateDateString : "");
                        cell = row.CreateCell(9);
                        cell.CellStyle = cellStyleAlignCenter;
                        cell = row.CreateCell(10);
                        cell.CellStyle = cellStyleAlignCenter;
                        worksheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 8, 10));
                        cell = row.CreateCell(11);
                        cell.CellStyle = cellStyleAlignCenter;
                        cell.SetCellValue(stduse != null ? stduse.Calibrator : "");
                        cell = row.CreateCell(12);
                        cell.CellStyle = cellStyleAlignCenter;
                        worksheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 11, 12));

                        rowIndex++;
                    }

                    #endregion

                    row = worksheet.CreateRow(rowIndex);

                    for (int cellIndex = 0; cellIndex <= 12; cellIndex++)
                    {
                        cell = row.CreateCell(cellIndex);
                    }

                    worksheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 0, 12));

                    rowIndex++;

                    #region Detail Header
                    row = worksheet.CreateRow(rowIndex);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyleBorderTop;
                    cell.SetCellValue("CAL.");

                    cell = row.CreateCell(1);
                    cell.CellStyle = cellStyleBorderTop;
                    cell.SetCellValue("RANGE");

                    cell = row.CreateCell(2);
                    cell.CellStyle = cellStyleBorderTop;
                    cell.SetCellValue("CALIBRATOR");

                    cell = row.CreateCell(3);
                    cell.CellStyle = cellStyleBorderTop;
                    cell.SetCellValue("READING OF");

                    cell = row.CreateCell(4);
                    cell.CellStyle = cellStyleBorderTop;
                    cell.SetCellValue("STANDARD");

                    cell = row.CreateCell(5);
                    cell.CellStyle = cellStyleBorderTop;

                    worksheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 4, 5));

                    cell = row.CreateCell(6);
                    cell.CellStyle = cellStyleBorderTop;
                    cell.SetCellValue("ACTUALITY");

                    cell = row.CreateCell(7);
                    cell.CellStyle = cellStyleBorderTop;
                    cell.SetCellValue("PASS");

                    cell = row.CreateCell(8);
                    cell.CellStyle = cellStyleBorderTop;
                    cell.SetCellValue("TEMP.");

                    cell = row.CreateCell(9);
                    cell.CellStyle = cellStyleBorderTop;

                    worksheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 8, 9));

                    cell = row.CreateCell(10);
                    cell.CellStyle = cellStyleBorderTop;
                    cell.SetCellValue("R.H.");

                    cell = row.CreateCell(11);
                    cell.CellStyle = cellStyleBorderTop;
                    cell.SetCellValue("PERF.");

                    cell = row.CreateCell(12);
                    cell.CellStyle = cellStyleBorderTop;
                    cell.SetCellValue("NEXT CAL");

                    rowIndex++;

                    row = worksheet.CreateRow(rowIndex);

                    cell = row.CreateCell(0);
                    cell.CellStyle = cellStyleBorderBottom;
                    cell.SetCellValue("DATE");

                    cell = row.CreateCell(1);
                    cell.CellStyle = cellStyleBorderBottom;

                    cell = row.CreateCell(2);
                    cell.CellStyle = cellStyleBorderBottom;
                    cell.SetCellValue("SETTING");

                    cell = row.CreateCell(3);
                    cell.CellStyle = cellStyleBorderBottom;
                    cell.SetCellValue("INSTRUMENT");

                    cell = row.CreateCell(4);
                    cell.CellStyle = cellStyleBorderBottom;
                    cell.SetCellValue("TOLERANCE");

                    cell = row.CreateCell(5);
                    cell.CellStyle = cellStyleBorderBottom;

                    worksheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 4, 5));

                    cell = row.CreateCell(6);
                    cell.CellStyle = cellStyleBorderBottom;
                    cell.SetCellValue("TOLERANCE");

                    cell = row.CreateCell(7);
                    cell.CellStyle = cellStyleBorderBottom;
                    
                    cell = row.CreateCell(8);
                    cell.CellStyle = cellStyleBorderBottom;

                    cell = row.CreateCell(9);
                    cell.CellStyle = cellStyleBorderBottom;

                    worksheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 8, 9));

                    cell = row.CreateCell(10);
                    cell.CellStyle = cellStyleBorderBottom;

                    cell = row.CreateCell(11);
                    cell.CellStyle = cellStyleBorderBottom;
                    cell.SetCellValue("BY");

                    cell = row.CreateCell(12);
                    cell.CellStyle = cellStyleBorderBottom;
                    cell.SetCellValue("DUE DATE");

                    rowIndex++;
                    #endregion

                    #region Detail
                    foreach (var item in model.FormViewModel.ItemList)
                    {
                        row = worksheet.CreateRow(rowIndex);

                        cell = row.CreateCell(0);
                        cell.CellStyle = cellStyleAlignCenter;
                        cell.SetCellValue(model.FormViewModel.CalibrateDateString);

                        cell = row.CreateCell(1);
                        cell.CellStyle = cellStyleAlignCenter;
                        cell.SetCellValue(item.Unit);

                        cell = row.CreateCell(2);
                        cell.CellStyle = cellStyleAlignCenter;
                        cell.SetCellValue(item.Standard.HasValue?item.Standard.ToString():"");

                        cell = row.CreateCell(3);
                        cell.CellStyle = cellStyleAlignCenter;
                        cell.SetCellValue(item.ReadingValue.HasValue?item.ReadingValue.ToString():"");

                        cell = row.CreateCell(4);
                        cell.CellStyle = cellStyleAlignCenter;
                        cell.SetCellValue(item.ToleranceMarkDisplay);

                        cell = row.CreateCell(5);
                        cell.CellStyle = cellStyleAlignCenter;
                        cell.SetCellValue(item.Tolerance.HasValue?item.Tolerance.ToString():"");

                        cell = row.CreateCell(6);
                        cell.CellStyle = cellStyleAlignCenter;
                        cell.SetCellValue(item.Diff);

                        cell = row.CreateCell(7);
                        cell.CellStyle = cellStyleAlignCenter;
                        cell.SetCellValue(item.IsFailed.HasValue ? (item.IsFailed.Value ? "FALSE" : "TRUE") : "");

                        cell = row.CreateCell(8);
                        cell.CellStyle = cellStyleAlignCenter;
                        cell.SetCellValue(model.FormViewModel.Temperature.HasValue?model.FormViewModel.Temperature.ToString():"");

                        cell = row.CreateCell(9);
                        cell.CellStyle = cellStyleAlignCenter;

                        worksheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, 8, 9));

                        cell = row.CreateCell(10);
                        cell.CellStyle = cellStyleAlignCenter;
                        cell.SetCellValue(model.FormViewModel.Humidity.HasValue ? model.FormViewModel.Humidity.ToString() : "");

                        cell = row.CreateCell(11);
                        cell.CellStyle = cellStyleAlignCenter;
                        cell.SetCellValue(model.FormViewModel.Calibrator);

                        cell = row.CreateCell(12);
                        cell.CellStyle = cellStyleAlignCenter;
                        if (model.FormViewModel.CalibrateDate.HasValue && model.FormViewModel.Equipment.CalCycle.HasValue)
                        {
                            var dueDate = model.FormViewModel.CalibrateDate.Value.AddMonths(model.FormViewModel.Equipment.CalCycle.Value).AddDays(-1);

                            cell.SetCellValue(DateTimeHelper.DateTime2DateStringWithSeperator(dueDate));
                        }
                        else
                        {
                            cell.SetCellValue("");
                        }

                        rowIndex++;
                    }
                    #endregion

                    worksheet.SetColumnWidth(0, 10 * 256);
                    worksheet.SetColumnWidth(1, 7 * 256);
                    worksheet.SetColumnWidth(2, 12 * 256);
                    worksheet.SetColumnWidth(3, 12 * 256);
                    worksheet.SetColumnWidth(4, 3 * 256);
                    worksheet.SetColumnWidth(5, 8 * 256);
                    worksheet.SetColumnWidth(6, 11 * 256);
                    worksheet.SetColumnWidth(7, 6 * 256);
                    worksheet.SetColumnWidth(8, 5 * 256);
                    worksheet.SetColumnWidth(9, 5 * 256);
                    worksheet.SetColumnWidth(10, 5 * 256);
                    worksheet.SetColumnWidth(11, 11 * 256);
                    worksheet.SetColumnWidth(12, 10 * 256);

                    using (FileStream fs = System.IO.File.OpenWrite(Path.Combine(Config.QAFile_v2FolderPath, string.Format("{0}.xls", UniqueID))))
                    {
                        workBook.Write(fs);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public static void SendCreatedMail(List<Models.Shared.Organization> OrganizationList, string UniqueID)
        {
            try
            {
                var mailAddressList = new List<MailAddress>();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var model = GetFormViewModel(OrganizationList, UniqueID);

                    var user = db.ACCOUNT.FirstOrDefault(x => x.ID == model.JobCalibratorID);

                    if (user != null && !string.IsNullOrEmpty(user.EMAIL))
                    {
                        mailAddressList.Add(new MailAddress(user.EMAIL, user.NAME));
                    }

                    if (mailAddressList.Count > 0)
                    {
                        var th = "<th style=\"width:150px;border:1px solid #333;padding:8px;text-align:right;color:#707070;background: #F4F4F4;\">{0}</th>";
                        var td = "<td style=\"width:400px;border:1px solid #333;padding:8px;color:#707070;\">{0}</td>";

                        var sb = new StringBuilder();

                        sb.Append("<table style=\"1px solid #ddd;font-size:13px;border-collapse:collapse;\">");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "單號"));
                        sb.Append(string.Format(td, model.VHNO));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "廠別"));
                        sb.Append(string.Format(td, model.Equipment.Factory));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "部門"));
                        sb.Append(string.Format(td, model.Equipment.OrganizationDescription));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "序號"));
                        sb.Append(string.Format(td, model.Equipment.SerialNo));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "儀器名稱"));
                        sb.Append(string.Format(td, model.Equipment.IchiDisplay));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "廠牌"));
                        sb.Append(string.Format(td, model.Equipment.Brand));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "型號"));
                        sb.Append(string.Format(td, model.Equipment.Model));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "類別"));
                        sb.Append(string.Format(td, model.CalibrateTypeDisplay));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "校驗負責單位"));
                        sb.Append(string.Format(td, model.CalibrateUnitDisplay));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "預計校驗日期"));
                        sb.Append(string.Format(td, model.EstCalibrateDateString));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "派工人員"));
                        sb.Append(string.Format(td, model.JobCalibrator));
                        sb.Append("</tr>");

                        if (!string.IsNullOrEmpty(model.Responsor))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "校驗負責人員"));
                            sb.Append(string.Format(td, model.Responsor));
                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(model.Calibrator))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "校驗人員"));
                            sb.Append(string.Format(td, model.Calibrator));
                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(model.CalibrateDateString))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "實際校驗日期"));
                            sb.Append(string.Format(td, model.CalibrateDateString));
                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(model.Equipment.OwnerID))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "設備負責人"));
                            sb.Append(string.Format(td, model.Equipment.Owner));
                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(model.Equipment.OwnerManagerID))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "設備負責人主管"));
                            sb.Append(string.Format(td, model.Equipment.OwnerManager));
                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(model.Equipment.PEID))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "製程負責人"));
                            sb.Append(string.Format(td, model.Equipment.PE));
                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(model.Equipment.PEManagerID))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "製程負責人主管"));
                            sb.Append(string.Format(td, model.Equipment.PEManager));
                            sb.Append("</tr>");
                        }

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "連結"));
                        sb.Append(string.Format(td, string.Format("<a href=\"http://ASECL-eINSPRD01/FEM.Portal/Home/Index?ReturnUrl=http://ASECL-eINSPRD01/FEM/zh-tw/Customized_ASE_QA/CalibrationForm/Index?VHNO={0}\">連結</a>", model.VHNO)));
                        sb.Append("</tr>");

                        sb.Append("</table>");

                        MailHelper.SendMail(mailAddressList, string.Format("[立案通知][{0}]儀器校驗執行單", model.VHNO), sb.ToString());
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
