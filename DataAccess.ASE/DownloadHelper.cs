using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Data.SQLite;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using Utility;
using Utility.Models;
using DataAccess;
using DbEntity.ASE;
using Models.ASE.DataSync;

namespace DataAccess.ASE
{
    public class DownloadHelper : IDisposable
    {
        private string Guid;

        private DownloadDataModel DataModel = new DownloadDataModel();

        public RequestResult Generate(DownloadFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                result = Init();

                if (result.IsSuccess)
                {
                    result = Query(Model);

                    if (result.IsSuccess)
                    {
                        result = GenerateSQLite();

                        if (result.IsSuccess)
                        {
                            result = GenerateZip();
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

        private RequestResult Init()
        {
            RequestResult result = new RequestResult();

            try
            {
                Guid = System.Guid.NewGuid().ToString();

                Directory.CreateDirectory(GeneratedFolderPath);

                System.IO.File.Copy(TemplateDbFilePath, GeneratedDbFilePath);

                result.Success();
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        private RequestResult Query(DownloadFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                var checkDate = DateTimeHelper.DateString2DateTime(Model.CheckDate).Value;

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    #region UnRFIDReason
                    DataModel.UnPatrolReasonList = db.UNPATROLREASON.Select(x => new Models.ASE.DataSync.UnPatrolReason
                    {
                        UniqueID = x.UNIQUEID,
                        ID = x.ID,
                        Description = x.DESCRIPTION,
                        LastModifyTime = x.LASTMODIFYTIME.Value
                    }).ToList();
                    #endregion

                    #region UnRFIDReason
                    DataModel.UnRFIDReasonList = db.UNRFIDREASON.Select(x => new Models.ASE.DataSync.UnRFIDReason
                    {
                        UniqueID = x.UNIQUEID,
                        ID = x.ID,
                        Description = x.DESCRIPTION,
                        LastModifyTime = x.LASTMODIFYTIME.Value
                    }).ToList();
                    #endregion

                    #region OverTimeReason
                    DataModel.OverTimeReasonList = db.OVERTIMEREASON.Select(x => new Models.ASE.DataSync.OverTimeReason
                    {
                        UniqueID = x.UNIQUEID,
                        ID = x.ID,
                        Description = x.DESCRIPTION,
                        LastModifyTime = x.LASTMODIFYTIME.Value
                    }).ToList();
                    #endregion

                    #region TimeSpanAbnormalReason
                    DataModel.TimeSpanAbnormalReasonList = db.TIMESPANABNORMALREASON.Select(x => new Models.ASE.DataSync.TimeSpanAbnormalReason
                    {
                        UniqueID = x.UNIQUEID,
                        ID = x.ID,
                        Description = x.DESCRIPTION,
                        LastModifyTime = x.LASTMODIFYTIME.Value
                    }).ToList();
                    #endregion

                    foreach (var parameter in Model.Parameters)
                    {
                        if (!string.IsNullOrEmpty(parameter.JobUniqueID))
                        {
                            #region Job
                            var job = (from j in db.JOB
                                       join r in db.ROUTE
                                       on j.ROUTEUNIQUEID equals r.UNIQUEID
                                       where j.UNIQUEID == parameter.JobUniqueID
                                       select new
                                       {
                                           UniqueID = j.UNIQUEID,
                                           OrganizationUniqueID = r.ORGANIZATIONUNIQUEID,
                                           Description = j.DESCRIPTION,
                                           RouteID = r.ID,
                                           RouteName = r.NAME,
                                           BeginDate = j.BEGINDATE.Value,
                                           EndDate = j.ENDDATE,
                                           TimeMode = j.TIMEMODE.Value,
                                           BeginTime = j.BEGINTIME,
                                           EndTime = j.ENDTIME,
                                           CycleCount = j.CYCLECOUNT.Value,
                                           CycleMode = j.CYCLEMODE,
                                           IsCheckBySeq = j.ISCHECKBYSEQ == "Y",
                                           IsShowPrevRecord = j.ISSHOWPREVRECORD == "Y",
                                           Remark = j.REMARK
                                       }).First();

                            var jobBeginDateString = string.Empty;
                            var jobEndDateString = string.Empty;

                            if (parameter.IsExceptChecked)
                            {
                                DateTime beginDate, endDate;

                                JobCycleHelper.GetDateSpan(checkDate, job.BeginDate, job.EndDate, job.CycleCount, job.CycleMode, out beginDate, out endDate);

                                jobBeginDateString = DateTimeHelper.DateTime2DateString(beginDate);
                                jobEndDateString = DateTimeHelper.DateTime2DateString(endDate);
                            }

                            var jobModel = new JobModel()
                            {
                                UniqueID = job.UniqueID,
                                OrganizationUniqueID = job.OrganizationUniqueID,
                                JobDescription = job.Description,
                                RouteID = job.RouteID,
                                RouteName = job.RouteName,
                                TimeMode = job.TimeMode,
                                BeginTime = job.BeginTime,
                                EndTime = job.EndTime,
                                IsCheckBySeq = job.IsCheckBySeq,
                                IsShowPrevRecord = job.IsShowPrevRecord,
                                Remark = job.Remark,
                                LastModifyTime = LastModifyTimeHelper.Get(job.UniqueID),
                                UserList = (from x in db.JOBUSER
                                            where x.JOBUNIQUEID == job.UniqueID
                                            select new UserModel
                                            {
                                                ID = x.USERID
                                            }).ToList()
                            };

                            #region ControlPoint
                            var controlPointList = (from x in db.JOBCONTROLPOINT
                                                    join j in db.JOB
                                                    on x.JOBUNIQUEID equals j.UNIQUEID
                                                    join y in db.ROUTECONTROLPOINT
                                                    on new { j.ROUTEUNIQUEID, x.CONTROLPOINTUNIQUEID } equals new { y.ROUTEUNIQUEID, y.CONTROLPOINTUNIQUEID }
                                                    join c in db.CONTROLPOINT
                                                    on x.CONTROLPOINTUNIQUEID equals c.UNIQUEID
                                                    where x.JOBUNIQUEID == job.UniqueID
                                                    select new
                                                    {
                                                        UniqueID = c.UNIQUEID,
                                                        c.ID,
                                                        Description = c.DESCRIPTION,
                                                        IsFeelItemDefaultNormal = c.ISFEELITEMDEFAULTNORMAL == "Y",
                                                        TagID = c.TAGID,
                                                        MinTimeSpan = x.MINTIMESPAN,
                                                        Remark = c.REMARK,
                                                        Seq = y.SEQ.Value
                                                    }).ToList();

                            foreach (var controlPoint in controlPointList)
                            {
                                var controlPointModel = new ControlPointModel()
                                {
                                    UniqueID = controlPoint.UniqueID,
                                    ID = controlPoint.ID,
                                    Description = controlPoint.Description,
                                    IsFeelItemDefaultNormal = controlPoint.IsFeelItemDefaultNormal,
                                    TagID = controlPoint.TagID,
                                    MinTimeSpan = controlPoint.MinTimeSpan,
                                    Remark = controlPoint.Remark,
                                    Seq = controlPoint.Seq
                                };

                                #region ControlPointCheckItem
                                var controlPointCheckItemList = (from x in db.JOBCONTROLPOINTCHECKITEM
                                                                 join j in db.JOB
                                                                 on x.JOBUNIQUEID equals j.UNIQUEID
                                                                 join y in db.ROUTECONTROLPOINTCHECKITEM
                                                                 on new { j.ROUTEUNIQUEID, x.CONTROLPOINTUNIQUEID, x.CHECKITEMUNIQUEID } equals new { y.ROUTEUNIQUEID, y.CONTROLPOINTUNIQUEID, y.CHECKITEMUNIQUEID }
                                                                 join c in db.CONTROLPOINTCHECKITEM
                                                                 on new { x.CONTROLPOINTUNIQUEID, x.CHECKITEMUNIQUEID } equals new { c.CONTROLPOINTUNIQUEID, c.CHECKITEMUNIQUEID }
                                                                 join item in db.CHECKITEM
                                                                 on x.CHECKITEMUNIQUEID equals item.UNIQUEID
                                                                 where x.JOBUNIQUEID == job.UniqueID && x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID
                                                                 select new
                                                                 {
                                                                     UniqueID = item.UNIQUEID,
                                                                     item.ID,
                                                                     Description = item.DESCRIPTION,
                                                                     IsFeelItem = item.ISFEELITEM,
                                                                     LowerLimit = c.ISINHERIT=="Y"?item.LOWERLIMIT:c.LOWERLIMIT,
                                                                     LowerAlertLimit = c.ISINHERIT == "Y" ? item.LOWERALERTLIMIT : c.LOWERALERTLIMIT,
                                                                     UpperAlertLimit = c.ISINHERIT == "Y" ? item.UPPERALERTLIMIT : c.UPPERALERTLIMIT,
                                                                     UpperLimit = c.ISINHERIT == "Y" ? item.UPPERLIMIT : c.UPPERLIMIT,
                                                                     Remark = c.ISINHERIT == "Y" ? item.REMARK : c.REMARK,
                                                                     Unit = c.ISINHERIT == "Y" ? item.UNIT : c.UNIT,
                                                                     Seq = y.SEQ.Value
                                                                 }).ToList();

                                foreach (var checkItem in controlPointCheckItemList)
                                {
                                    bool except = false;

                                    if (parameter.IsExceptChecked)
                                    {
                                        var prevCheckResult = db.CHECKRESULT.Where(x => x.JOBUNIQUEID == job.UniqueID && x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID && x.CHECKITEMUNIQUEID == checkItem.UniqueID && string.Compare(x.CHECKDATE, jobBeginDateString) >= 0 && string.Compare(x.CHECKDATE, jobEndDateString) <= 0).FirstOrDefault();

                                        except = prevCheckResult != null;
                                    }

                                    if (!except)
                                    {
                                        var checkItemModel = new CheckItemModel()
                                        {
                                            UniqueID = checkItem.UniqueID,
                                            ID = checkItem.ID,
                                            Description = checkItem.Description,
                                            IsFeelItem = checkItem.IsFeelItem == "Y",
                                            LowerLimit = checkItem.LowerLimit,
                                            LowerAlertLimit = checkItem.LowerAlertLimit,
                                            UpperAlertLimit = checkItem.UpperAlertLimit,
                                            UpperLimit = checkItem.UpperLimit,
                                            Remark = checkItem.Remark,
                                            Unit = checkItem.Unit,
                                            Seq = checkItem.Seq,
                                            FeelOptionList = db.CHECKITEMFEELOPTION.Where(x => x.CHECKITEMUNIQUEID == checkItem.UniqueID).Select(x => new FeelOptionModel
                                            {
                                                UniqueID = x.UNIQUEID,
                                                Description = x.DESCRIPTION,
                                                IsAbnormal = x.ISABNORMAL=="Y",
                                                Seq = x.SEQ.Value
                                            }).ToList(),
                                            AbnormalReasonList = (from ca in db.CHECKITEMABNORMALREASON
                                                                  join a in db.ABNORMALREASON
                                                                  on ca.ABNORMALREASONUNIQUEID equals a.UNIQUEID
                                                                  where ca.CHECKITEMUNIQUEID == checkItem.UniqueID
                                                                  select new AbnormalReasonModel
                                                                  {
                                                                      UniqueID = a.UNIQUEID,
                                                                      ID = a.ID,
                                                                      Description = a.DESCRIPTION,
                                                                      HandlingMethodList = (from ah in db.ABNORMALREASONHANDLINGMETHOD
                                                                                            join h in db.HANDLINGMETHOD
                                                                                                on ah.HANDLINGMETHODUNIQUEID equals h.UNIQUEID
                                                                                            where ah.ABNORMALREASONUNIQUEID == a.UNIQUEID
                                                                                            select new HandlingMethodModel
                                                                                            {
                                                                                                UniqueID = h.UNIQUEID,
                                                                                                ID = h.ID,
                                                                                                Description = h.DESCRIPTION
                                                                                            }).ToList()
                                                                  }).ToList()
                                        };

                                        if (job.IsShowPrevRecord)
                                        {
                                            var prevCheckResult = db.CHECKRESULT.Where(x => x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID && x.CHECKITEMUNIQUEID == checkItem.UniqueID).OrderByDescending(x => x.CHECKDATE).ThenByDescending(x => x.CHECKTIME).FirstOrDefault();

                                            if (prevCheckResult != null && !DataModel.PrevCheckResultList.Any(x => x.ControlPointUniqueID == controlPoint.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID))
                                            {
                                                DataModel.PrevCheckResultList.Add(new PrevCheckResultModel()
                                                {
                                                    ControlPointUniqueID = controlPoint.UniqueID,
                                                    EquipmentUniqueID = "",
                                                    PartUniqueID = "",
                                                    CheckItemUniqueID = checkItem.UniqueID,
                                                    CheckDate = prevCheckResult.CHECKDATE,
                                                    CheckTime = prevCheckResult.CHECKTIME,
                                                    Result = prevCheckResult.RESULT,
                                                    IsAbnormal = prevCheckResult.ISABNORMAL=="Y",
                                                    LowerLimit = prevCheckResult.LOWERLIMIT,
                                                    LowerAlertLimit = prevCheckResult.LOWERALERTLIMIT,
                                                    UpperAlertLimit = prevCheckResult.UPPERALERTLIMIT,
                                                    UpperLimit = prevCheckResult.UPPERLIMIT,
                                                    Unit = prevCheckResult.UNIT,
                                                    AbnormalReasonList = db.CHECKRESULTABNORMALREASON.Where(a => a.CHECKRESULTUNIQUEID == prevCheckResult.UNIQUEID).OrderBy(a => a.ABNORMALREASONID).Select(a => new PrevCheckResultAbnormalReasonModel
                                                    {
                                                        Description = a.ABNORMALREASONDESCRIPTION,
                                                        Remark = a.ABNORMALREASONREMARK,
                                                        HandlingMethodList = db.CHECKRESULTHANDLINGMETHOD.Where(h => h.CHECKRESULTUNIQUEID == prevCheckResult.UNIQUEID && h.ABNORMALREASONUNIQUEID == a.ABNORMALREASONUNIQUEID).OrderBy(h => h.HANDLINGMETHODID).Select(h => new PrevCheckResultHandlingMethodModel
                                                        {
                                                            Description = h.HANDLINGMETHODDESCRIPTION,
                                                            Remark = h.HANDLINGMETHODREMARK
                                                        }).ToList()
                                                    }).ToList()
                                                });
                                            }
                                        }

                                        controlPointModel.CheckItemList.Add(checkItemModel);
                                    }
                                }
                                #endregion

                                #region Equipment
                                var equipmentList = (from x in db.JOBEQUIPMENT
                                                     join e in db.EQUIPMENT
                                                     on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                                     where x.JOBUNIQUEID == job.UniqueID && x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID
                                                     select new
                                                     {
                                                        UniqueID= e.UNIQUEID,
                                                         e.ID,
                                                        Name = e.NAME,
                                                        IsFeelItemDefaultNormal = e.ISFEELITEMDEFAULTNORMAL == "Y"
                                                     }).Distinct().ToList();

                                foreach (var equipment in equipmentList)
                                {
                                    var equipmentModel = new EquipmentModel()
                                    {
                                        UniqueID = equipment.UniqueID,
                                        ID = equipment.ID,
                                        Name = equipment.Name,
                                        IsFeelItemDefaultNormal = equipment.IsFeelItemDefaultNormal,
                                    };

                                    var specList = (from x in db.EQUIPMENTSPECVALUE
                                                    join s in db.EQUIPMENTSPEC
                                                    on x.SPECUNIQUEID equals s.UNIQUEID
                                                    //join o in db.EQUIPMENTSPECOPTION
                                                    //on x.SPECOPTIONUNIQUEID equals o.UNIQUEID into tmpSpecOption
                                                    //from so in tmpSpecOption.DefaultIfEmpty()
                                                    where x.EQUIPMENTUNIQUEID == equipment.UniqueID
                                                    select new
                                                    {
                                                        EquipmentUniqueID = equipment.UniqueID,
                                                        SpecOptionUniqueID = x.SPECOPTIONUNIQUEID,
                                                        Spec = s.DESCRIPTION,
                                                        //Option = so != null ? so.DESCRIPTION : "",
                                                        Input = x.VALUE
                                                    }).ToList();

                                    foreach (var spec in specList)
                                    {
                                        var specOption = db.EQUIPMENTSPECOPTION.FirstOrDefault(x => x.UNIQUEID == spec.SpecOptionUniqueID);

                                        equipmentModel.SpecList.Add(new EquipmentSpecModel
                                        {
                                            EquipmentUniqueID = equipment.UniqueID,
                                            Spec = spec.Spec,
                                            Option = specOption != null ? specOption.DESCRIPTION : "",
                                            Input = spec.Input
                                        });
                                    }

                                    var partList = (from x in db.JOBEQUIPMENT
                                                    join j in db.JOB
                                                    on x.JOBUNIQUEID equals j.UNIQUEID
                                                    join y in db.ROUTEEQUIPMENT
                                                    on new { j.ROUTEUNIQUEID, x.CONTROLPOINTUNIQUEID, x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID } equals new { y.ROUTEUNIQUEID, y.CONTROLPOINTUNIQUEID, y.EQUIPMENTUNIQUEID, y.PARTUNIQUEID }
                                                    //join p in db.EQUIPMENTPART
                                                    //on new { EquipmentUniqueID = x.EQUIPMENTUNIQUEID, PartUniqueID = x.PARTUNIQUEID } equals new { EquipmentUniqueID = p.EQUIPMENTUNIQUEID, PartUniqueID = p.UNIQUEID } into tmpPart
                                                    //from p in tmpPart.DefaultIfEmpty()
                                                    where x.JOBUNIQUEID == job.UniqueID && x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID && x.EQUIPMENTUNIQUEID == equipment.UniqueID
                                                    select new
                                                    {
                                                        UniqueID = x.PARTUNIQUEID,
                                                        //Description = p != null ? p.DESCRIPTION : "",
                                                        y.SEQ
                                                    }).ToList();

                                    foreach (var part in partList)
                                    {
                                        var p = db.EQUIPMENTPART.FirstOrDefault(x => x.UNIQUEID == part.UniqueID);

                                        var partModel = new PartModel()
                                        {
                                            UniqueID = part.UniqueID,
                                            Description = p!=null?p.DESCRIPTION:"",
                                            Seq = part.SEQ.Value,
                                            FileList = db.FFILE.Where(x => x.EQUIPMENTUNIQUEID == equipment.UniqueID && x.PARTUNIQUEID == part.UniqueID && x.ISDOWNLOAD2MOBILE=="Y").Select(x => new FileModel
                                            {
                                                UniqueID = x.UNIQUEID,
                                                FileName = x.FILENAME,
                                                Extension = x.EXTENSION
                                            }).ToList(),
                                            MaterialList = (from x in db.EQUIPMENTMATERIAL
                                                            join m in db.MATERIAL
                                                            on x.MATERIALUNIQUEID equals m.UNIQUEID
                                                            where x.EQUIPMENTUNIQUEID == equipment.UniqueID && x.PARTUNIQUEID == part.UniqueID
                                                            select new MaterialModel
                                                            {
                                                                EquipmentUniqueID = equipment.UniqueID,
                                                                PartUniqueID = part.UniqueID,
                                                                UniqueID = m.UNIQUEID,
                                                                ID = m.ID,
                                                                Name = m.NAME,
                                                                Quantity = x.QUANTITY.Value,
                                                                SpecList = (from sv in db.MATERIALSPECVALUE
                                                                            join s in db.MATERIALSPEC
                                                                            on sv.SPECUNIQUEID equals s.UNIQUEID
                                                                            //join so in db.MATERIALSPECOPTION
                                                                            //on sv.SPECOPTIONUNIQUEID equals so.UNIQUEID into tmpSpecOption
                                                                            //from so in tmpSpecOption.DefaultIfEmpty()
                                                                            where sv.MATERIALUNIQUEID == m.UNIQUEID
                                                                            select new MaterialSpecModel
                                                                            {
                                                                                MaterialUniqueID = m.UNIQUEID,
                                                                                Spec = s.DESCRIPTION,
                                                                                //Option = so != null ? so.DESCRIPTION : "",
                                                                                Input = sv.VALUE
                                                                            }).ToList()
                                                            }).ToList()
                                        };

                                        #region CheckItem
                                        var equipmentCheckItemList = (from x in db.JOBEQUIPMENTCHECKITEM
                                                                      join j in db.JOB
                                                                      on x.JOBUNIQUEID equals j.UNIQUEID
                                                                      join y in db.ROUTEEQUIPMENTCHECKITEM
                                                                      on new { j.ROUTEUNIQUEID, x.CONTROLPOINTUNIQUEID, x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID, x.CHECKITEMUNIQUEID } equals new { y.ROUTEUNIQUEID, y.CONTROLPOINTUNIQUEID, y.EQUIPMENTUNIQUEID, y.PARTUNIQUEID, y.CHECKITEMUNIQUEID }
                                                                      join c in db.EQUIPMENTCHECKITEM
                                                                      on new { x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID, x.CHECKITEMUNIQUEID } equals new { c.EQUIPMENTUNIQUEID, c.PARTUNIQUEID, c.CHECKITEMUNIQUEID }
                                                                      join item in db.CHECKITEM
                                                                    on x.CHECKITEMUNIQUEID equals item.UNIQUEID
                                                                      where x.JOBUNIQUEID == job.UniqueID && x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID && x.EQUIPMENTUNIQUEID == equipment.UniqueID && x.PARTUNIQUEID == part.UniqueID
                                                                      select new
                                                                      {
                                                                          UniqueID = item.UNIQUEID,
                                                                          //UniqueID = c.CHECKITEMUNIQUEID,
                                                                          item.ID,
                                                                          Description = item.DESCRIPTION,
                                                                          IsFeelItem = item.ISFEELITEM == "Y",
                                                                          LowerLimit = c.ISINHERIT == "Y" ? item.LOWERLIMIT : c.LOWERLIMIT,
                                                                          LowerAlertLimit = c.ISINHERIT == "Y" ? item.LOWERALERTLIMIT : c.LOWERALERTLIMIT,
                                                                          UpperAlertLimit = c.ISINHERIT == "Y" ? item.UPPERALERTLIMIT : c.UPPERALERTLIMIT,
                                                                          UpperLimit = c.ISINHERIT == "Y" ? item.UPPERLIMIT : c.UPPERLIMIT,
                                                                          Remark = c.ISINHERIT == "Y" ? item.REMARK : c.REMARK,
                                                                          Unit = c.ISINHERIT == "Y" ? item.UNIT : c.UNIT,
                                                                          Seq = y.SEQ
                                                                      }).ToList();

                                        foreach (var checkItem in equipmentCheckItemList)
                                        {
                                            bool except = false;

                                            if (parameter.IsExceptChecked)
                                            {
                                                var prevCheckResult = db.CHECKRESULT.Where(x => x.JOBUNIQUEID == job.UniqueID && x.CONTROLPOINTUNIQUEID == controlPoint.UniqueID && x.EQUIPMENTUNIQUEID == equipment.UniqueID && x.PARTUNIQUEID == part.UniqueID && x.CHECKITEMUNIQUEID == checkItem.UniqueID && string.Compare(x.CHECKDATE, jobBeginDateString) >= 0 && string.Compare(x.CHECKDATE, jobEndDateString) <= 0).FirstOrDefault();

                                                except = prevCheckResult != null;
                                            }

                                            if (!except)
                                            {
                                                var checkItemModel = new CheckItemModel()
                                                {
                                                    UniqueID = checkItem.UniqueID,
                                                    ID = checkItem.ID,
                                                    Description = checkItem.Description,
                                                    IsFeelItem = checkItem.IsFeelItem,
                                                    LowerLimit = checkItem.LowerLimit,
                                                    LowerAlertLimit = checkItem.LowerAlertLimit,
                                                    UpperAlertLimit = checkItem.UpperAlertLimit,
                                                    UpperLimit = checkItem.UpperLimit,
                                                    Remark = checkItem.Remark,
                                                    Unit = checkItem.Unit,
                                                    Seq = checkItem.Seq.HasValue ? checkItem.Seq.Value : 1,
                                                    FeelOptionList = db.CHECKITEMFEELOPTION.Where(x => x.CHECKITEMUNIQUEID == checkItem.UniqueID).Select(x => new FeelOptionModel
                                                    {
                                                        UniqueID = x.UNIQUEID,
                                                        Description = x.DESCRIPTION,
                                                        IsAbnormal = x.ISABNORMAL=="Y",
                                                        Seq = x.SEQ.Value
                                                    }).ToList(),
                                                    AbnormalReasonList = (from ca in db.CHECKITEMABNORMALREASON
                                                                          join a in db.ABNORMALREASON
                                                                          on ca.ABNORMALREASONUNIQUEID equals a.UNIQUEID
                                                                          where ca.CHECKITEMUNIQUEID == checkItem.UniqueID
                                                                          select new AbnormalReasonModel
                                                                          {
                                                                              UniqueID = a.UNIQUEID,
                                                                              ID = a.ID,
                                                                              Description = a.DESCRIPTION,
                                                                              HandlingMethodList = (from ah in db.ABNORMALREASONHANDLINGMETHOD
                                                                                                    join h in db.HANDLINGMETHOD
                                                                                                        on ah.HANDLINGMETHODUNIQUEID equals h.UNIQUEID
                                                                                                    where ah.ABNORMALREASONUNIQUEID == a.UNIQUEID
                                                                                                    select new HandlingMethodModel
                                                                                                    {
                                                                                                        UniqueID = h.UNIQUEID,
                                                                                                        ID = h.ID,
                                                                                                        Description = h.DESCRIPTION
                                                                                                    }).ToList()
                                                                          }).ToList()
                                                };

                                                if (job.IsShowPrevRecord)
                                                {
                                                    var prevCheckResult = db.CHECKRESULT.Where(x => x.EQUIPMENTUNIQUEID == equipment.UniqueID && x.PARTUNIQUEID == part.UniqueID && x.CHECKITEMUNIQUEID == checkItem.UniqueID).OrderByDescending(x => x.CHECKDATE).ThenByDescending(x => x.CHECKTIME).FirstOrDefault();

                                                    if (prevCheckResult != null && !DataModel.PrevCheckResultList.Any(x => x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == part.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID))
                                                    {
                                                        DataModel.PrevCheckResultList.Add(new PrevCheckResultModel()
                                                        {
                                                            ControlPointUniqueID = controlPoint.UniqueID,
                                                            EquipmentUniqueID = equipment.UniqueID,
                                                            PartUniqueID = part.UniqueID,
                                                            CheckItemUniqueID = checkItem.UniqueID,
                                                            CheckDate = prevCheckResult.CHECKDATE,
                                                            CheckTime = prevCheckResult.CHECKTIME,
                                                            Result = prevCheckResult.RESULT,
                                                            IsAbnormal = prevCheckResult.ISABNORMAL=="Y",
                                                            LowerLimit = prevCheckResult.LOWERLIMIT,
                                                            LowerAlertLimit = prevCheckResult.LOWERALERTLIMIT,
                                                            UpperAlertLimit = prevCheckResult.UPPERALERTLIMIT,
                                                            UpperLimit = prevCheckResult.UPPERLIMIT,
                                                            Unit = prevCheckResult.UNIT,
                                                            AbnormalReasonList = db.CHECKRESULTABNORMALREASON.Where(a => a.CHECKRESULTUNIQUEID == prevCheckResult.UNIQUEID).OrderBy(a => a.ABNORMALREASONID).Select(a => new PrevCheckResultAbnormalReasonModel
                                                            {
                                                                Description = a.ABNORMALREASONDESCRIPTION,
                                                                Remark = a.ABNORMALREASONREMARK,
                                                                HandlingMethodList = db.CHECKRESULTHANDLINGMETHOD.Where(h => h.CHECKRESULTUNIQUEID == prevCheckResult.UNIQUEID && h.ABNORMALREASONUNIQUEID == a.ABNORMALREASONUNIQUEID).OrderBy(h => h.HANDLINGMETHODID).Select(h => new PrevCheckResultHandlingMethodModel
                                                                {
                                                                    Description = h.HANDLINGMETHODDESCRIPTION,
                                                                    Remark = h.HANDLINGMETHODREMARK
                                                                }).ToList()
                                                            }).ToList()
                                                        });
                                                    }
                                                }

                                                partModel.CheckItemList.Add(checkItemModel);
                                            }
                                        }
                                        #endregion

                                        if (partModel.CheckItemList.Count > 0)
                                        {
                                            equipmentModel.PartList.Add(partModel);
                                        }
                                    }

                                    if (equipmentModel.PartList.Count > 0)
                                    {
                                        controlPointModel.EquipmentList.Add(equipmentModel);
                                    }
                                }
                                #endregion

                                if (controlPointModel.CheckItemList.Count > 0 || controlPointModel.EquipmentList.Count > 0)
                                {
                                    jobModel.ControlPointList.Add(controlPointModel);
                                }
                            }
                            #endregion

                            #endregion

                            if (jobModel.ControlPointList.Count > 0)
                            {
                                DataModel.JobList.Add(jobModel);
                            }
                        }
                        else if (!string.IsNullOrEmpty(parameter.MaintenanceFormUniqueID))
                        {
                            var form = (from f in db.MFORM
                                        join j in db.MJOB
                                        on f.MJOBUNIQUEID equals j.UNIQUEID
                                        join e in db.EQUIPMENT
                                        on f.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                        join p in db.EQUIPMENTPART
                                        on f.PARTUNIQUEID equals p.UNIQUEID into tmpPart
                                        from p in tmpPart.DefaultIfEmpty()
                                        where f.UNIQUEID == parameter.MaintenanceFormUniqueID
                                        select new
                                        {
                                            MaintenanceForm = f,
                                            Job = j,
                                            Equipment = e,
                                            PartDescription = p != null ? p.DESCRIPTION : ""
                                        }).FirstOrDefault();

                            if (form != null)
                            {
                                var mFormModel = new MaintenanceFormModel()
                                {
                                    UniqueID = form.MaintenanceForm.UNIQUEID,
                                    VHNO = form.MaintenanceForm.VHNO,
                                    Description = form.Job.DESCRIPTION,
                                    EquipmentID = form.Equipment.ID,
                                    EquipmentName = form.Equipment.NAME,
                                    PartDescription = form.PartDescription,
                                    BeginDate =form.MaintenanceForm.ESTBEGINDATE,
                                    EndDate = form.MaintenanceForm.ESTENDDATE,
                                    Remark = form.Job.REMARK,
                                    UserList = (from f in db.MFORM
                                                join x in db.MJOBUSER
                                                on f.MJOBUNIQUEID equals x.MJOBUNIQUEID
                                                where f.UNIQUEID == parameter.MaintenanceFormUniqueID
                                                select new UserModel { ID = x.USERID }).ToList()
                                };

                                var standardList = (from x in db.MJOBEQUIPMENTSTANDARD
                                                    join y in db.EQUIPMENTSTANDARD
                                                    on new { x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID, x.STANDARDUNIQUEID } equals new { y.EQUIPMENTUNIQUEID, y.PARTUNIQUEID, y.STANDARDUNIQUEID }
                                                    join s in db.STANDARD
                                                    on y.STANDARDUNIQUEID equals s.UNIQUEID
                                                    where x.MJOBUNIQUEID == form.Job.UNIQUEID
                                                    select new
                                                    {
                                                        UniqueID = s.UNIQUEID,
                                                        MaintenanceType = s.MAINTENANCETYPE,
                                                        s.ID,
                                                        Description = s.DESCRIPTION,
                                                        IsFeelItem = s.ISFEELITEM,
                                                        LowerLimit = y.ISINHERIT == "Y" ? s.LOWERLIMIT : y.LOWERLIMIT,
                                                        LowerAlertLimit = y.ISINHERIT == "Y" ? s.LOWERALERTLIMIT : y.LOWERALERTLIMIT,
                                                        UpperAlertLimit = y.ISINHERIT == "Y" ? s.UPPERALERTLIMIT : y.UPPERALERTLIMIT,
                                                        UpperLimit = y.ISINHERIT == "Y" ? s.UPPERLIMIT : y.UPPERLIMIT,
                                                        Remark = y.ISINHERIT == "Y" ? s.REMARK : y.REMARK,
                                                        Unit = y.ISINHERIT == "Y" ? s.UNIT : y.UNIT
                                                    }).Distinct().OrderBy(x => x.MaintenanceType).ThenBy(x => x.ID).ToList();

                                int seq = 1;

                                foreach (var standard in standardList)
                                {
                                    mFormModel.StandardList.Add(new StandardModel()
                                    {
                                        UniqueID = standard.UniqueID,
                                        ID = standard.ID,
                                        Description = standard.Description,
                                        IsFeelItem = standard.IsFeelItem == "Y",
                                        LowerLimit = standard.LowerLimit,
                                        LowerAlertLimit = standard.LowerAlertLimit,
                                        UpperAlertLimit = standard.UpperAlertLimit,
                                        UpperLimit = standard.UpperLimit,
                                        Remark = standard.Remark,
                                        Unit = standard.Unit,
                                        Seq = seq,
                                        FeelOptionList = db.STANDARDFEELOPTION.Where(x => x.STANDARDUNIQUEID == standard.UniqueID).Select(x => new FeelOptionModel
                                        {
                                            UniqueID = x.UNIQUEID,
                                            Description = x.DESCRIPTION,
                                            IsAbnormal = x.ISABNORMAL == "Y",
                                            Seq = x.SEQ.Value
                                        }).ToList(),
                                        AbnormalReasonList = (from sa in db.STANDARDABNORMALREASON
                                                              join a in db.ABNORMALREASON
                                                              on sa.ABNORMALREASONUNIQUEID equals a.UNIQUEID
                                                              where sa.STANDARDUNIQUEID == standard.UniqueID
                                                              select new AbnormalReasonModel
                                                              {
                                                                  UniqueID = a.UNIQUEID,
                                                                  ID = a.ID,
                                                                  Description = a.DESCRIPTION,
                                                                  HandlingMethodList = (from ah in db.ABNORMALREASONHANDLINGMETHOD
                                                                                        join h in db.HANDLINGMETHOD
                                                                                        on ah.HANDLINGMETHODUNIQUEID equals h.UNIQUEID
                                                                                        where ah.ABNORMALREASONUNIQUEID == a.UNIQUEID
                                                                                        select new HandlingMethodModel
                                                                                        {
                                                                                            UniqueID = h.UNIQUEID,
                                                                                            ID = h.ID,
                                                                                            Description = h.DESCRIPTION
                                                                                        }).ToList()
                                                              }).ToList()
                                    });

                                    seq++;
                                }

                                var materialList = (from x in db.MJOBEQUIPMENTMATERIAL
                                                    join m in db.MATERIAL
                                                    on x.MATERIALUNIQUEID equals m.UNIQUEID
                                                    where x.MJOBUNIQUEID == form.Job.UNIQUEID
                                                    select new
                                                    {
                                                        UniqueID = m.UNIQUEID,
                                                        m.ID,
                                                        Name = m.NAME,
                                                        x.QUANTITY
                                                    }).OrderBy(x => x.ID).ToList();

                                foreach (var material in materialList)
                                {
                                    mFormModel.MaterialList.Add(new MFormMaterialModel()
                                    {
                                        UniqueID = material.UniqueID,
                                        ID = material.ID,
                                        Name = material.Name,
                                        Quantity = material.QUANTITY
                                    });
                                }

                                DataModel.MaintenanceFormList.Add(mFormModel);
                            }
                        }
                        else if (!string.IsNullOrEmpty(parameter.RepairFormUniqueID))
                        {
                            //#region Equipment Maintenance

                            var repairForm = (from x in db.RFORM
                                              join t in db.RFORMTYPE
                                              on x.RFORMTYPEUNIQUEID equals t.UNIQUEID into tmpType
                                              from t in tmpType.DefaultIfEmpty()
                                              join e in db.EQUIPMENT
                                              on x.EQUIPMENTUNIQUEID equals e.UNIQUEID into tmpEquipment
                                              from e in tmpEquipment.DefaultIfEmpty()
                                              join p in db.EQUIPMENTPART
                                              on x.PARTUNIQUEID equals p.UNIQUEID into tmpPart
                                              from p in tmpPart.DefaultIfEmpty()
                                              where x.UNIQUEID == parameter.RepairFormUniqueID
                                              select new
                                              {
                                                  x.UNIQUEID,
                                                  x.SUBJECT,
                                                  x.DESCRIPTION,
                                                  x.VHNO,
                                                  x.ESTBEGINDATE,
                                                  x.ESTENDDATE,
                                                  RepairFormType = t != null ? t.DESCRIPTION : "",
                                                  EquipmentID = e != null ? e.ID : "",
                                                  EquipmentName = e != null ? e.NAME : "",
                                                  PartDescription = p != null ? p.DESCRIPTION : ""
                                              }).First();

                            var repairFormModel = new RepairFormModel()
                            {
                                UniqueID = repairForm.UNIQUEID,
                                Subject = repairForm.SUBJECT,
                                Description = repairForm.DESCRIPTION,
                                VHNO = repairForm.VHNO,
                                RepairFormType = repairForm.RepairFormType,
                                EquipmentID = repairForm.EquipmentID,
                                EquipmentName = repairForm.EquipmentName,
                                 PartDescription = repairForm.PartDescription,
                                BeginDate = repairForm.ESTBEGINDATE,
                                EndDate = repairForm.ESTENDDATE,
                                UserList = db.RFORMJOBUSER.Where(x => x.RFORMUNIQUEID == repairForm.UNIQUEID).Select(x => new UserModel
                                {
                                    ID = x.USERID
                                }).ToList()
                            };

                            DataModel.RepairFormList.Add(repairFormModel);

                            //#endregion
                        }
                    }
                }

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    foreach (var repairForm in DataModel.RepairFormList)
                    {
                        repairForm.UserList = (from x in repairForm.UserList
                                               join u in db.ACCOUNT
                                               on x.ID equals u.ID
                                               select new UserModel
                                               {
                                                   ID = u.ID,
                                                   Name = u.NAME,
                                                   Title = u.TITLE,
                                                   Password = u.PASSWORD,
                                                   UID = u.TAGID
                                               }).ToList();
                    }

                    foreach (var maintenanceForm in DataModel.MaintenanceFormList)
                    {
                        maintenanceForm.UserList = (from x in maintenanceForm.UserList
                                                    join u in db.ACCOUNT
                                                    on x.ID equals u.ID
                                                    select new UserModel
                                                    {
                                                        ID = u.ID,
                                                        Name = u.NAME,
                                                        Title = u.TITLE,
                                                        Password = u.PASSWORD,
                                                        UID = u.TAGID
                                                    }).ToList();
                    }

                    foreach (var job in DataModel.JobList)
                    {
                        job.UserList = (from x in job.UserList
                                        join u in db.ACCOUNT
                                        on x.ID equals u.ID
                                        select new UserModel
                                        {
                                            ID = u.ID,
                                            Name = u.NAME,
                                            Title = u.TITLE,
                                            Password = u.PASSWORD,
                                            UID = u.TAGID
                                        }).ToList();

                        #region EmgContactList
                        //var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(job.OrganizationUniqueID, true);

                        //job.EmgContactList = (from e in db.EMGCONTACT
                        //                      join u in db.ACCOUNT
                        //                      on e.USERID equals u.ID into tmpUser
                        //                      from u in tmpUser.DefaultIfEmpty()
                        //                      where upStreamOrganizationList.Contains(e.ORGANIZATIONUNIQUEID)
                        //                      select new EmgContactModel
                        //                      {
                        //                          UniqueID = e.UNIQUEID,
                        //                          Title = u != null ? u.TITLE : e.TITLE,
                        //                          Name = u != null ? u.NAME : e.NAME,
                        //                          TelList = db.EMGCONTACTTEL.Where(x => x.EMGCONTACTUNIQUEID == e.UNIQUEID).Select(x => new EmgContactTelModel
                        //                          {
                        //                              Seq = x.SEQ,
                        //                              Tel = x.TEL
                        //                          }).ToList()
                        //                      }).ToList();
                        #endregion
                    }
                }

                result.Success();
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        private RequestResult GenerateSQLite()
        {
            RequestResult result = new RequestResult();

            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(this.SQLiteConnString))
                {
                    conn.Open();

                    using (SQLiteTransaction trans = conn.BeginTransaction())
                    {
                        #region AbnormalReason, AbnormalReasonHandlingMethod
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO AbnormalReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", Define.OTHER);
                            cmd.Parameters.AddWithValue("ID", Define.OTHER);
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO AbnormalReasonHandlingMethod (AbnormalReasonUniqueID, HandlingMethodUniqueID) VALUES (@AbnormalReasonUniqueID, @HandlingMethodUniqueID)";

                            cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", Define.OTHER);
                            cmd.Parameters.AddWithValue("HandlingMethodUniqueID", Define.OTHER);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var abnormalReason in DataModel.AbnormalReasonList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO AbnormalReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", abnormalReason.UniqueID);
                                cmd.Parameters.AddWithValue("ID", abnormalReason.ID);
                                cmd.Parameters.AddWithValue("Description", abnormalReason.Description);

                                cmd.ExecuteNonQuery();
                            }

                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO AbnormalReasonHandlingMethod (AbnormalReasonUniqueID, HandlingMethodUniqueID) VALUES (@AbnormalReasonUniqueID, @HandlingMethodUniqueID)";

                                cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", abnormalReason.UniqueID);
                                cmd.Parameters.AddWithValue("HandlingMethodUniqueID", Define.OTHER);

                                cmd.ExecuteNonQuery();
                            }

                            #region AbnormalReasonHandlingMethod
                            foreach (var handlingMethod in abnormalReason.HandlingMethodList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO AbnormalReasonHandlingMethod (AbnormalReasonUniqueID, HandlingMethodUniqueID) VALUES (@AbnormalReasonUniqueID, @HandlingMethodUniqueID)";

                                    cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", abnormalReason.UniqueID);
                                    cmd.Parameters.AddWithValue("HandlingMethodUniqueID", handlingMethod.UniqueID);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                            #endregion
                        }
                        #endregion

                        #region CheckItemAbnormalReason, CheckItemFeelOption
                        foreach (var checkItem in DataModel.CheckItemList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO CheckItemAbnormalReason (CheckItemUniqueID, AbnormalReasonUniqueID) VALUES (@CheckItemUniqueID, @AbnormalReasonUniqueID)";

                                cmd.Parameters.AddWithValue("CheckItemUniqueID", checkItem.UniqueID);
                                cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", "OTHER");

                                cmd.ExecuteNonQuery();
                            }

                            #region CheckItemAbnormalReason
                            foreach (var abnormalReason in checkItem.AbnormalReasonList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO CheckItemAbnormalReason (CheckItemUniqueID, AbnormalReasonUniqueID) VALUES (@CheckItemUniqueID, @AbnormalReasonUniqueID)";

                                    cmd.Parameters.AddWithValue("CheckItemUniqueID", checkItem.UniqueID);
                                    cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", abnormalReason.UniqueID);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                            #endregion

                            #region CheckItemFeelOption
                            foreach (var feelOption in checkItem.FeelOptionList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO CheckItemFeelOption (UniqueID, CheckItemUniqueID, Description, IsAbnormal, Seq) VALUES (@UniqueID, @CheckItemUniqueID, @Description, @IsAbnormal, @Seq)";

                                    cmd.Parameters.AddWithValue("UniqueID", feelOption.UniqueID);
                                    cmd.Parameters.AddWithValue("CheckItemUniqueID", checkItem.UniqueID);
                                    cmd.Parameters.AddWithValue("Description", feelOption.Description);
                                    cmd.Parameters.AddWithValue("IsAbnormal", feelOption.IsAbnormal ? "Y" : "N");
                                    cmd.Parameters.AddWithValue("Seq", feelOption.Seq);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                            #endregion
                        }
                        #endregion

                        #region HandlingMethod
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO HandlingMethod (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", "OTHER");
                            cmd.Parameters.AddWithValue("ID", "OTHER");
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var handlingMethod in DataModel.HandlingMethodList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO HandlingMethod (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", handlingMethod.UniqueID);
                                cmd.Parameters.AddWithValue("ID", handlingMethod.ID);
                                cmd.Parameters.AddWithValue("Description", handlingMethod.Description);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region Job, ControlPoint, Equipment, CheckItem
                        foreach (var job in DataModel.JobList)
                        {
                            #region EmgContact
                            foreach (var emgContact in job.EmgContactList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO EmgContact (JobUniqueID, UniqueID, Title, Name) VALUES (@JobUniqueID, @UniqueID, @Title, @Name)";

                                    cmd.Parameters.AddWithValue("JobUniqueID", job.UniqueID);
                                    cmd.Parameters.AddWithValue("UniqueID", emgContact.UniqueID);
                                    cmd.Parameters.AddWithValue("Title", emgContact.Title);
                                    cmd.Parameters.AddWithValue("Name", emgContact.Name);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                            #endregion

                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO Job (UniqueID, Description, Remark, TimeMode, BeginTime, EndTime, IsCheckBySeq, IsShowPrevRecord) VALUES (@UniqueID, @Description, @Remark, @TimeMode, @BeginTime, @EndTime, @IsCheckBySeq, @IsShowPrevRecord)";

                                cmd.Parameters.AddWithValue("UniqueID", job.UniqueID);
                                cmd.Parameters.AddWithValue("Description", job.Description);
                                cmd.Parameters.AddWithValue("Remark", job.Remark);
                                cmd.Parameters.AddWithValue("TimeMode", job.TimeMode);
                                cmd.Parameters.AddWithValue("BeginTime", job.BeginTime);
                                cmd.Parameters.AddWithValue("EndTime", job.EndTime);
                                cmd.Parameters.AddWithValue("IsCheckBySeq", job.IsCheckBySeq ? "Y" : "N");
                                cmd.Parameters.AddWithValue("IsShowPrevRecord", job.IsShowPrevRecord ? "Y" : "N");

                                cmd.ExecuteNonQuery();
                            }

                            foreach (var controlPoint in job.ControlPointList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO ControlPoint (JobUniqueID, ControlPointUniqueID, ID, Description, IsFeelItemDefaultNormal, TagID, MinTimeSpan, Remark, Seq) VALUES (@JobUniqueID, @ControlPointUniqueID, @ID, @Description, @IsFeelItemDefaultNormal, @TagID, @MinTimeSpan, @Remark, @Seq)";

                                    cmd.Parameters.AddWithValue("JobUniqueID", job.UniqueID);
                                    cmd.Parameters.AddWithValue("ControlPointUniqueID", controlPoint.UniqueID);
                                    cmd.Parameters.AddWithValue("ID", controlPoint.ID);
                                    cmd.Parameters.AddWithValue("Description", controlPoint.Description);
                                    cmd.Parameters.AddWithValue("IsFeelItemDefaultNormal", controlPoint.IsFeelItemDefaultNormal ? "Y" : "N");
                                    cmd.Parameters.AddWithValue("TagID", controlPoint.TagID);
                                    cmd.Parameters.AddWithValue("MinTimeSpan", controlPoint.MinTimeSpan);
                                    cmd.Parameters.AddWithValue("Remark", controlPoint.Remark);
                                    cmd.Parameters.AddWithValue("Seq", controlPoint.Seq);

                                    cmd.ExecuteNonQuery();
                                }

                                foreach (var equipment in controlPoint.EquipmentList)
                                {
                                    foreach (var part in equipment.PartList)
                                    {
                                        using (SQLiteCommand cmd = conn.CreateCommand())
                                        {
                                            cmd.CommandText = "INSERT INTO Equipment (JobUniqueID, ControlPointUniqueID, EquipmentUniqueID, PartUniqueID, ID, Name, PartDescription, IsFeelItemDefaultNormal, Seq) VALUES (@JobUniqueID, @ControlPointUniqueID, @EquipmentUniqueID, @PartUniqueID, @ID, @Name, @PartDescription, @IsFeelItemDefaultNormal, @Seq)";

                                            cmd.Parameters.AddWithValue("JobUniqueID", job.UniqueID);
                                            cmd.Parameters.AddWithValue("ControlPointUniqueID", controlPoint.UniqueID);
                                            cmd.Parameters.AddWithValue("EquipmentUniqueID", equipment.UniqueID);
                                            cmd.Parameters.AddWithValue("PartUniqueID", part.UniqueID);
                                            cmd.Parameters.AddWithValue("ID", equipment.ID);
                                            cmd.Parameters.AddWithValue("Name", equipment.Name);
                                            cmd.Parameters.AddWithValue("PartDescription", part.Description);
                                            cmd.Parameters.AddWithValue("IsFeelItemDefaultNormal", equipment.IsFeelItemDefaultNormal ? "Y" : "N");
                                            cmd.Parameters.AddWithValue("Seq", part.Seq);

                                            cmd.ExecuteNonQuery();
                                        }

                                        foreach (var checkItem in part.CheckItemList)
                                        {
                                            using (SQLiteCommand cmd = conn.CreateCommand())
                                            {
                                                cmd.CommandText = "INSERT INTO CheckItem (JobUniqueID, ControlPointUniqueID, EquipmentUniqueID, PartUniqueID, CheckItemUniqueID, ID, Description, IsFeelItem, LowerLimit, LowerAlertLimit, UpperAlertLimit, UpperLimit, Unit, Remark, Seq) VALUES (@JobUniqueID, @ControlPointUniqueID, @EquipmentUniqueID, @PartUniqueID, @CheckItemUniqueID, @ID, @Description, @IsFeelItem, @LowerLimit, @LowerAlertLimit, @UpperAlertLimit, @UpperLimit, @Unit, @Remark, @Seq)";

                                                cmd.Parameters.AddWithValue("JobUniqueID", job.UniqueID);
                                                cmd.Parameters.AddWithValue("ControlPointUniqueID", controlPoint.UniqueID);
                                                cmd.Parameters.AddWithValue("EquipmentUniqueID", equipment.UniqueID);
                                                cmd.Parameters.AddWithValue("PartUniqueID", part.UniqueID);
                                                cmd.Parameters.AddWithValue("CheckItemUniqueID", checkItem.UniqueID);
                                                cmd.Parameters.AddWithValue("ID", checkItem.ID);
                                                cmd.Parameters.AddWithValue("Description", checkItem.Description);
                                                cmd.Parameters.AddWithValue("IsFeelItem", checkItem.IsFeelItem ? "Y" : "N");
                                                cmd.Parameters.AddWithValue("LowerLimit", checkItem.LowerLimit);
                                                cmd.Parameters.AddWithValue("LowerAlertLimit", checkItem.LowerAlertLimit);
                                                cmd.Parameters.AddWithValue("UpperAlertLimit", checkItem.UpperAlertLimit);
                                                cmd.Parameters.AddWithValue("UpperLimit", checkItem.UpperLimit);
                                                cmd.Parameters.AddWithValue("Unit", checkItem.Unit);
                                                cmd.Parameters.AddWithValue("Remark", checkItem.Remark);
                                                cmd.Parameters.AddWithValue("Seq", checkItem.Seq);

                                                cmd.ExecuteNonQuery();
                                            }
                                        }

                                        foreach (var file in part.FileList)
                                        {
                                            using (SQLiteCommand cmd = conn.CreateCommand())
                                            {
                                                cmd.CommandText = "INSERT INTO EquipmentFile (UniqueID, EquipmentUniqueID, PartUniqueID, FileName, Extension) VALUES (@UniqueID, @EquipmentUniqueID, @PartUniqueID, @FileName, @Extension)";

                                                cmd.Parameters.AddWithValue("UniqueID", file.UniqueID);
                                                cmd.Parameters.AddWithValue("EquipmentUniqueID", equipment.UniqueID);
                                                cmd.Parameters.AddWithValue("PartUniqueID", part.UniqueID);
                                                cmd.Parameters.AddWithValue("FileName", file.FileName);
                                                cmd.Parameters.AddWithValue("Extension", file.Extension);

                                                cmd.ExecuteNonQuery();
                                            }
                                        }
                                    }
                                }

                                foreach (var checkItem in controlPoint.CheckItemList)
                                {
                                    using (SQLiteCommand cmd = conn.CreateCommand())
                                    {
                                        cmd.CommandText = "INSERT INTO CheckItem (JobUniqueID, ControlPointUniqueID, EquipmentUniqueID, PartUniqueID, CheckItemUniqueID, ID, Description, IsFeelItem, LowerLimit, LowerAlertLimit, UpperAlertLimit, UpperLimit, Unit, Remark, Seq) VALUES (@JobUniqueID, @ControlPointUniqueID, @EquipmentUniqueID, @PartUniqueID, @CheckItemUniqueID, @ID, @Description, @IsFeelItem, @LowerLimit, @LowerAlertLimit, @UpperAlertLimit, @UpperLimit, @Unit, @Remark, @Seq)";

                                        cmd.Parameters.AddWithValue("JobUniqueID", job.UniqueID);
                                        cmd.Parameters.AddWithValue("ControlPointUniqueID", controlPoint.UniqueID);
                                        cmd.Parameters.AddWithValue("EquipmentUniqueID", "");
                                        cmd.Parameters.AddWithValue("PartUniqueID", "");
                                        cmd.Parameters.AddWithValue("CheckItemUniqueID", checkItem.UniqueID);
                                        cmd.Parameters.AddWithValue("ID", checkItem.ID);
                                        cmd.Parameters.AddWithValue("Description", checkItem.Description);
                                        cmd.Parameters.AddWithValue("IsFeelItem", checkItem.IsFeelItem ? "Y" : "N");
                                        cmd.Parameters.AddWithValue("LowerLimit", checkItem.LowerLimit);
                                        cmd.Parameters.AddWithValue("LowerAlertLimit", checkItem.LowerAlertLimit);
                                        cmd.Parameters.AddWithValue("UpperAlertLimit", checkItem.UpperAlertLimit);
                                        cmd.Parameters.AddWithValue("UpperLimit", checkItem.UpperLimit);
                                        cmd.Parameters.AddWithValue("Unit", checkItem.Unit);
                                        cmd.Parameters.AddWithValue("Remark", checkItem.Remark);
                                        cmd.Parameters.AddWithValue("Seq", checkItem.Seq);

                                        cmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                        #endregion

                        #region LastModifyTime
                        foreach (var lasyModifyTime in DataModel.LastModifyTimeList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO LastModifyTime (JobUniqueID, VersionTime) VALUES (@JobUniqueID, @VersionTime)";

                                cmd.Parameters.AddWithValue("JobUniqueID", lasyModifyTime.Key);
                                cmd.Parameters.AddWithValue("VersionTime", lasyModifyTime.Value);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region EquipmentMaterial
                        foreach (var material in DataModel.MaterialList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO EquipmentMaterial (EquipmentUniqueID, PartUniqueID, MaterialUniqueID, MaterialID, MaterialName, QTY) VALUES (@EquipmentUniqueID, @PartUniqueID, @MaterialUniqueID, @MaterialID, @MaterialName, @QTY)";

                                cmd.Parameters.AddWithValue("EquipmentUniqueID", material.EquipmentUniqueID);
                                cmd.Parameters.AddWithValue("PartUniqueID", material.PartUniqueID);
                                cmd.Parameters.AddWithValue("MaterialUniqueID", material.UniqueID);
                                cmd.Parameters.AddWithValue("MaterialID", material.ID);
                                cmd.Parameters.AddWithValue("MaterialName", material.Name);
                                cmd.Parameters.AddWithValue("QTY", material.Quantity);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region EquipmentSpec
                        foreach (var spec in DataModel.EquipmentSpecList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO EquipmentSpec (EquipmentUniqueID, Spec, Value) VALUES (@EquipmentUniqueID, @Spec, @Value)";

                                cmd.Parameters.AddWithValue("EquipmentUniqueID", spec.EquipmentUniqueID);
                                cmd.Parameters.AddWithValue("Spec", spec.Spec);
                                cmd.Parameters.AddWithValue("Value", spec.Value);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region MaterialSpec
                        foreach (var spec in DataModel.MaterialSpecList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO MaterialSpec (MaterialUniqueID, Spec, Value) VALUES (@MaterialUniqueID, @Spec, @Value)";

                                cmd.Parameters.AddWithValue("MaterialUniqueID", spec.MaterialUniqueID);
                                cmd.Parameters.AddWithValue("Spec", spec.Spec);
                                cmd.Parameters.AddWithValue("Value", spec.Value);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region OverTimeReason
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO OverTimeReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", "OTHER");
                            cmd.Parameters.AddWithValue("ID", "OTHER");
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var overTimeReason in DataModel.OverTimeReasonList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO OverTimeReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", overTimeReason.UniqueID);
                                cmd.Parameters.AddWithValue("ID", overTimeReason.ID);
                                cmd.Parameters.AddWithValue("Description", overTimeReason.Description);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region TimeSpanAbnormalReason
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO TimeSpanAbnormalReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", "OTHER");
                            cmd.Parameters.AddWithValue("ID", "OTHER");
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var timeSpanAbnormalReason in DataModel.TimeSpanAbnormalReasonList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO TimeSpanAbnormalReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", timeSpanAbnormalReason.UniqueID);
                                cmd.Parameters.AddWithValue("ID", timeSpanAbnormalReason.ID);
                                cmd.Parameters.AddWithValue("Description", timeSpanAbnormalReason.Description);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region UnPatrolReason
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO UnPatrolReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", "OTHER");
                            cmd.Parameters.AddWithValue("ID", "OTHER");
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var unPatrolReason in DataModel.UnPatrolReasonList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO UnPatrolReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", unPatrolReason.UniqueID);
                                cmd.Parameters.AddWithValue("ID", unPatrolReason.ID);
                                cmd.Parameters.AddWithValue("Description", unPatrolReason.Description);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region EmgContactTel
                        foreach (var emgContact in DataModel.EmgContactList)
                        {
                            foreach (var tel in emgContact.TelList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO EmgContactTel (EmgContactUniqueID, Seq, Tel) VALUES (@EmgContactUniqueID, @Seq, @Tel)";

                                    cmd.Parameters.AddWithValue("EmgContactUniqueID", emgContact.UniqueID);
                                    cmd.Parameters.AddWithValue("Seq", tel.Seq);
                                    cmd.Parameters.AddWithValue("Tel", tel.Tel);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                        #endregion

                        #region UnRFIDReason
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO UnRFIDReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", "OTHER");
                            cmd.Parameters.AddWithValue("ID", "OTHER");
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var unRFIDReason in DataModel.UnRFIDReasonList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO UnRFIDReason (UniqueID, ID, Description) VALUES (@UniqueID, @ID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", unRFIDReason.UniqueID);
                                cmd.Parameters.AddWithValue("ID", unRFIDReason.ID);
                                cmd.Parameters.AddWithValue("Description", unRFIDReason.Description);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region User
                        foreach (var user in DataModel.UserList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO [User] (ID, Title, Name, Password, UID) VALUES (@ID, @Title, @Name, @Password, @UID)";

                                cmd.Parameters.AddWithValue("ID", user.ID);
                                cmd.Parameters.AddWithValue("Title", user.Title);
                                cmd.Parameters.AddWithValue("Name", user.Name);
                                cmd.Parameters.AddWithValue("Password", user.Password);
                                cmd.Parameters.AddWithValue("UID", user.UID);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region PrevCheckResult
                        foreach (var prevCheckResult in DataModel.PrevCheckResultList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO PrevCheckResult (ControlPointUniqueID, EquipmentUniqueID, PartUniqueID, CheckItemUniqueID, LowerLimit, LowerAlertLimit, UpperAlertLimit, UpperLimit, Unit, CheckDate, CheckTime, Result, IsAbnormal, AbnormalReason) VALUES (@ControlPointUniqueID, @EquipmentUniqueID, @PartUniqueID, @CheckItemUniqueID, @LowerLimit, @LowerAlertLimit, @UpperAlertLimit, @UpperLimit, @Unit, @CheckDate, @CheckTime, @Result, @IsAbnormal, @AbnormalReason)";

                                cmd.Parameters.AddWithValue("ControlPointUniqueID", prevCheckResult.ControlPointUniqueID);
                                cmd.Parameters.AddWithValue("EquipmentUniqueID", prevCheckResult.EquipmentUniqueID);
                                cmd.Parameters.AddWithValue("PartUniqueID", prevCheckResult.PartUniqueID);
                                cmd.Parameters.AddWithValue("CheckItemUniqueID", prevCheckResult.CheckItemUniqueID);
                                cmd.Parameters.AddWithValue("LowerLimit", prevCheckResult.LowerLimit);
                                cmd.Parameters.AddWithValue("LowerAlertLimit", prevCheckResult.LowerAlertLimit);
                                cmd.Parameters.AddWithValue("UpperAlertLimit", prevCheckResult.UpperAlertLimit);
                                cmd.Parameters.AddWithValue("UpperLimit", prevCheckResult.UpperLimit);
                                cmd.Parameters.AddWithValue("Unit", prevCheckResult.Unit);
                                cmd.Parameters.AddWithValue("CheckDate", prevCheckResult.CheckDate);
                                cmd.Parameters.AddWithValue("CheckTime", prevCheckResult.CheckTime);
                                cmd.Parameters.AddWithValue("Result", prevCheckResult.Result);
                                cmd.Parameters.AddWithValue("IsAbnormal", prevCheckResult.IsAbnormal);
                                cmd.Parameters.AddWithValue("AbnormalReason", prevCheckResult.AbnormalReasons);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        #region StandardAbnormalReason, StandardFeelOption
                        foreach (var standard in DataModel.StandardList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO StandardAbnormalReason (StandardUniqueID, AbnormalReasonUniqueID) VALUES (@StandardUniqueID, @AbnormalReasonUniqueID)";

                                cmd.Parameters.AddWithValue("StandardUniqueID", standard.UniqueID);
                                cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", "OTHER");

                                cmd.ExecuteNonQuery();
                            }

                            #region StandardAbnormalReason
                            foreach (var abnormalReason in standard.AbnormalReasonList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO StandardUniqueIDAbnormalReason (StandardUniqueID, AbnormalReasonUniqueID) VALUES (@StandardUniqueID, @AbnormalReasonUniqueID)";

                                    cmd.Parameters.AddWithValue("StandardUniqueID", standard.UniqueID);
                                    cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", abnormalReason.UniqueID);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                            #endregion

                            #region CheckItemFeelOption
                            foreach (var feelOption in standard.FeelOptionList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO StandardFeelOption (UniqueID, StandardUniqueID, Description, IsAbnormal, Seq) VALUES (@UniqueID, @StandardUniqueID, @Description, @IsAbnormal, @Seq)";

                                    cmd.Parameters.AddWithValue("UniqueID", feelOption.UniqueID);
                                    cmd.Parameters.AddWithValue("StandardUniqueID", standard.UniqueID);
                                    cmd.Parameters.AddWithValue("Description", feelOption.Description);
                                    cmd.Parameters.AddWithValue("IsAbnormal", feelOption.IsAbnormal ? "Y" : "N");
                                    cmd.Parameters.AddWithValue("Seq", feelOption.Seq);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                            #endregion
                        }
                        #endregion

                        foreach (var maintenanceForm in DataModel.MaintenanceFormList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO MForm (UniqueID, VHNO, Description, Remark, EquipmentID, EquipmentName, PartDescription, BeginDate, EndDate) VALUES (@UniqueID, @VHNO, @Description, @Remark, @EquipmentID, @EquipmentName, @PartDescription, @BeginDate, @EndDate)";

                                cmd.Parameters.AddWithValue("UniqueID", maintenanceForm.UniqueID);
                                cmd.Parameters.AddWithValue("VHNO", maintenanceForm.VHNO);
                                cmd.Parameters.AddWithValue("Description", maintenanceForm.Description);
                                cmd.Parameters.AddWithValue("Remark", maintenanceForm.Remark);
                                cmd.Parameters.AddWithValue("EquipmentID", maintenanceForm.EquipmentID);
                                cmd.Parameters.AddWithValue("EquipmentName", maintenanceForm.EquipmentName);
                                cmd.Parameters.AddWithValue("PartDescription", maintenanceForm.PartDescription);
                                cmd.Parameters.AddWithValue("BeginDate", maintenanceForm.BeginDateString);
                                cmd.Parameters.AddWithValue("EndDate", maintenanceForm.EndDateString);

                                cmd.ExecuteNonQuery();
                            }

                            foreach (var standard in maintenanceForm.StandardList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO Standard (MFormUniqueID, StandardUniqueID, ID, Description, IsFeelItem, LowerLimit, LowerAlertLimit, UpperAlertLimit, UpperLimit, Unit, Remark, Seq) VALUES (@MFormUniqueID, @StandardUniqueID,  @ID, @Description, @IsFeelItem, @LowerLimit, @LowerAlertLimit, @UpperAlertLimit, @UpperLimit, @Unit, @Remark, @Seq)";

                                    cmd.Parameters.AddWithValue("MFormUniqueID", maintenanceForm.UniqueID);
                                    cmd.Parameters.AddWithValue("StandardUniqueID", standard.UniqueID);
                                    cmd.Parameters.AddWithValue("ID", standard.ID);
                                    cmd.Parameters.AddWithValue("Description", standard.Description);
                                    cmd.Parameters.AddWithValue("IsFeelItem", standard.IsFeelItem ? "Y" : "N");
                                    cmd.Parameters.AddWithValue("LowerLimit", standard.LowerLimit);
                                    cmd.Parameters.AddWithValue("LowerAlertLimit", standard.LowerAlertLimit);
                                    cmd.Parameters.AddWithValue("UpperAlertLimit", standard.UpperAlertLimit);
                                    cmd.Parameters.AddWithValue("UpperLimit", standard.UpperLimit);
                                    cmd.Parameters.AddWithValue("Unit", standard.Unit);
                                    cmd.Parameters.AddWithValue("Remark", standard.Remark);
                                    cmd.Parameters.AddWithValue("Seq", standard.Seq);

                                    cmd.ExecuteNonQuery();
                                }
                            }

                            foreach (var material in maintenanceForm.MaterialList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO MFormMaterial (MFormUniqueID, MaterialUniqueID, MaterialID, MaterialName, Quantity) VALUES (@MFormUniqueID, @MaterialUniqueID, @MaterialID, @MaterialName, @Quantity)";

                                    cmd.Parameters.AddWithValue("MFormUniqueID", maintenanceForm.UniqueID);
                                    cmd.Parameters.AddWithValue("MaterialUniqueID", material.UniqueID);
                                    cmd.Parameters.AddWithValue("MaterialID", material.ID);
                                    cmd.Parameters.AddWithValue("MaterialName", material.Name);
                                    cmd.Parameters.AddWithValue("Quantity", material.Quantity);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        #region RepairForm
                        foreach (var repairForm in DataModel.RepairFormList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO RepairForm (UniqueID, VHNO, Equipment, Subject, Description, RepairFormType, BeginDate, EndDate) VALUES (@UniqueID, @VHNO, @Equipment, @Subject, @Description, @RepairFormType, @BeginDate, @EndDate)";

                                cmd.Parameters.AddWithValue("UniqueID", repairForm.UniqueID);
                                cmd.Parameters.AddWithValue("VHNO", repairForm.VHNO);
                                cmd.Parameters.AddWithValue("Equipment", repairForm.Equipment);
                                cmd.Parameters.AddWithValue("Subject", repairForm.Subject);
                                cmd.Parameters.AddWithValue("Description", repairForm.Description);
                                cmd.Parameters.AddWithValue("RepairFormType", repairForm.RepairFormType);
                                cmd.Parameters.AddWithValue("BeginDate", repairForm.BeginDateString);
                                cmd.Parameters.AddWithValue("EndDate", repairForm.EndDateString);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        #endregion

                        trans.Commit();
                    }

                    conn.Close();
                }

                result.Success();
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        private RequestResult GenerateZip()
        {
            RequestResult result = new RequestResult();

            try
            {
                using (FileStream fs = System.IO.File.Create(GeneratedZipFilePath))
                {
                    using (ZipOutputStream zipStream = new ZipOutputStream(fs))
                    {
                        zipStream.SetLevel(9); //0-9, 9 being the highest level of compression

                        ZipHelper.CompressFolder(GeneratedDbFilePath, zipStream);

                        zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream

                        zipStream.Close();
                    }
                }

                result.ReturnData(GeneratedZipFilePath);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        private string TemplateDbFilePath
        {
            get
            {
                return Path.Combine(Config.EquipmentMaintenanceSQLiteTemplateFolderPath, Define.SQLite_EquipmentMaintenance);
            }
        }

        private string GeneratedFolderPath
        {
            get
            {
                return Path.Combine(Config.EquipmentMaintenanceSQLiteGeneratedFolderPath, Guid);
            }
        }

        private string GeneratedDbFilePath
        {
            get
            {
                return Path.Combine(GeneratedFolderPath, Define.SQLite_EquipmentMaintenance);
            }
        }

        private string GeneratedZipFilePath
        {
            get
            {
                return Path.Combine(GeneratedFolderPath, Define.SQLiteZip_EquipmentMaintenance);
            }
        }

        private string SQLiteConnString
        {
            get
            {
                return string.Format("Data Source={0};Version=3;", GeneratedDbFilePath);
            }
        }

        #region IDisposable

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {

                }
            }

            _disposed = true;
        }

        ~DownloadHelper()
        {
            Dispose(false);
        }

        #endregion
    }
}
