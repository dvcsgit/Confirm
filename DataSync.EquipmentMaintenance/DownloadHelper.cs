using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Data.SQLite;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using Utility;
using Utility.Models;
#if ORACLE
using DbEntity.ORACLE;
using DbEntity.ORACLE.EquipmentMaintenance;
#else
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
#endif
using Models.EquipmentMaintenance.DataSync;
using DataAccess.EquipmentMaintenance;
using DataAccess;

namespace DataSync.EquipmentMaintenance
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

                            if (!result.IsSuccess)
                            {
                                Logger.Log("GenerateZip Failed");
                            }
                        }
                        else
                        {
                            Logger.Log("GenerateSQLite Failed");
                        }
                    }
                    else
                    {
                        Logger.Log("Query Failed");
                    }
                }
                else
                {
                    Logger.Log("Init Failed");
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

#if CHIMEI
        private RequestResult Query(DownloadFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    #region UnPatrolReason
                    DataModel.UnPatrolReasonList = db.UnPatrolReason.Select(x => new Models.EquipmentMaintenance.DataSync.UnPatrolReason
                    {
                        UniqueID = x.UniqueID,
                        ID = x.ID,
                        Description = x.Description,
                        LastModifyTime = x.LastModifyTime
                    }).ToList();
                    #endregion

                    #region UnRFIDReason
                    DataModel.UnRFIDReasonList = db.UnRFIDReason.Select(x => new Models.EquipmentMaintenance.DataSync.UnRFIDReason
                    {
                        UniqueID = x.UniqueID,
                        ID = x.ID,
                        Description = x.Description,
                        LastModifyTime = x.LastModifyTime
                    }).ToList();
                    #endregion

                    #region OverTimeReason
                    DataModel.OverTimeReasonList = db.OverTimeReason.Select(x => new Models.EquipmentMaintenance.DataSync.OverTimeReason
                    {
                        UniqueID = x.UniqueID,
                        ID = x.ID,
                        Description = x.Description,
                        LastModifyTime = x.LastModifyTime
                    }).ToList();
                    #endregion

                    #region TimeSpanAbnormalReason
                    DataModel.TimeSpanAbnormalReasonList = db.TimeSpanAbnormalReason.Select(x => new Models.EquipmentMaintenance.DataSync.TimeSpanAbnormalReason
                    {
                        UniqueID = x.UniqueID,
                        ID = x.ID,
                        Description = x.Description,
                        LastModifyTime = x.LastModifyTime
                    }).ToList();
                    #endregion

                    foreach (var parameter in Model.Parameters)
                    {
                        if (!string.IsNullOrEmpty(parameter.JobUniqueID))
                        {
                            #region Job
                            var job = (from j in db.Job
                                       join r in db.Route
                                       on j.RouteUniqueID equals r.UniqueID
                                       where j.UniqueID == parameter.JobUniqueID
                                       select new
                                       {
                                           UniqueID = j.UniqueID,
                                           OrganizationUniqueID = r.OrganizationUniqueID,
                                           Description = j.Description,
                                           RouteID = r.ID,
                                           RouteName = r.Name,
                                           BeginDate = j.BeginDate,
                                           EndDate = j.EndDate,
                                           TimeMode = j.TimeMode,
                                           BeginTime = j.BeginTime,
                                           EndTime = j.EndTime,
                                           CycleCount = j.CycleCount,
                                           CycleMode = j.CycleMode,
                                           IsCheckBySeq = j.IsCheckBySeq,
                                           IsShowPrevRecord = j.IsShowPrevRecord ,
                                           Remark = j.Remark
                                       }).First();

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
                                UserList = (from x in db.JobUser
                                            where x.JobUniqueID == job.UniqueID
                                            select new UserModel
                                            {
                                                ID = x.UserID
                                            }).ToList()
                            };

                            #region ControlPoint
                            var controlPointList = (from x in db.JobControlPoint
                                                    join j in db.Job
                                                    on x.JobUniqueID equals j.UniqueID
                                                    join y in db.RouteControlPoint
                                                    on new { j.RouteUniqueID, x.ControlPointUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID }
                                                    join c in db.ControlPoint
                                                    on x.ControlPointUniqueID equals c.UniqueID
                                                    where x.JobUniqueID == job.UniqueID
                                                    select new
                                                    {
                                                        UniqueID = c.UniqueID,
                                                        c.ID,
                                                        Description = c.Description,
                                                        IsFeelItemDefaultNormal = c.IsFeelItemDefaultNormal,
                                                        TagID = c.TagID,
                                                        MinTimeSpan = x.MinTimeSpan,
                                                        Remark = c.Remark,
                                                        Seq = y.Seq
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
                                var controlPointCheckItemList = (from x in db.JobControlPointCheckItem
                                                                 join j in db.Job
                                                                 on x.JobUniqueID equals j.UniqueID
                                                                 join y in db.RouteControlPointCheckItem
                                                                 on new { j.RouteUniqueID, x.ControlPointUniqueID, x.CheckItemUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.CheckItemUniqueID }
                                                                 join c in db.ControlPointCheckItem
                                                                 on new { x.ControlPointUniqueID, x.CheckItemUniqueID } equals new { c.ControlPointUniqueID, c.CheckItemUniqueID }
                                                                 join item in db.CheckItem
                                                                 on x.CheckItemUniqueID equals item.UniqueID
                                                                 where x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID
                                                                 select new
                                                                 {
                                                                     UniqueID = item.UniqueID,
                                                                     item.ID,
                                                                     Description = item.Description,
                                                                     IsFeelItem = item.IsFeelItem,
                                                                     LowerLimit = c.IsInherit  ? item.LowerLimit : c.LowerLimit,
                                                                     LowerAlertLimit = c.IsInherit ? item.LowerAlertLimit : c.LowerAlertLimit,
                                                                     UpperAlertLimit = c.IsInherit ? item.UpperAlertLimit : c.UpperAlertLimit,
                                                                     UpperLimit = c.IsInherit ? item.UpperLimit : c.UpperLimit,
                                                                     Remark = c.IsInherit ? item.Remark : c.Remark,
                                                                     Unit = c.IsInherit ? item.Unit : c.Unit,
                                                                     Seq = y.Seq
                                                                 }).ToList();

                                foreach (var checkItem in controlPointCheckItemList)
                                {
                                    bool except = false;

                                    if (parameter.IsExceptChecked)
                                    {
                                        var prevCheckResult = db.CheckResult.Where(x => x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID).FirstOrDefault();

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
                                            Seq = checkItem.Seq,
                                            FeelOptionList = db.CheckItemFeelOption.Where(x => x.CheckItemUniqueID == checkItem.UniqueID).Select(x => new FeelOptionModel
                                            {
                                                UniqueID = x.UniqueID,
                                                Description = x.Description,
                                                IsAbnormal = x.IsAbnormal,
                                                Seq = x.Seq
                                            }).ToList(),
                                            AbnormalReasonList = (from ca in db.CheckItemAbnormalReason
                                                                  join a in db.AbnormalReason
                                                                  on ca.AbnormalReasonUniqueID equals a.UniqueID
                                                                  where ca.CheckItemUniqueID == checkItem.UniqueID
                                                                  select new AbnormalReasonModel
                                                                  {
                                                                      UniqueID = a.UniqueID,
                                                                      ID = a.ID,
                                                                      Description = a.Description,
                                                                      HandlingMethodList = (from ah in db.AbnormalReasonHandlingMethod
                                                                                            join h in db.HandlingMethod
                                                                                                on ah.HandlingMethodUniqueID equals h.UniqueID
                                                                                            where ah.AbnormalReasonUniqueID == a.UniqueID
                                                                                            select new HandlingMethodModel
                                                                                            {
                                                                                                UniqueID = h.UniqueID,
                                                                                                ID = h.ID,
                                                                                                Description = h.Description
                                                                                            }).ToList()
                                                                  }).ToList()
                                        };

                                        if (job.IsShowPrevRecord)
                                        {
                                            var prevCheckResult = db.CheckResult.Where(x => x.ControlPointUniqueID == controlPoint.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID).OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();

                                            if (prevCheckResult != null && !DataModel.PrevCheckResultList.Any(x => x.ControlPointUniqueID == controlPoint.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID))
                                            {
                                                DataModel.PrevCheckResultList.Add(new PrevCheckResultModel()
                                                {
                                                    ControlPointUniqueID = controlPoint.UniqueID,
                                                    EquipmentUniqueID = "",
                                                    PartUniqueID = "",
                                                    CheckItemUniqueID = checkItem.UniqueID,
                                                    CheckDate = prevCheckResult.CheckDate,
                                                    CheckTime = prevCheckResult.CheckTime,
                                                    Result = prevCheckResult.Result,
                                                    IsAbnormal = prevCheckResult.IsAbnormal ,
                                                    LowerLimit = prevCheckResult.LowerLimit,
                                                    LowerAlertLimit = prevCheckResult.LowerAlertLimit,
                                                    UpperAlertLimit = prevCheckResult.UpperAlertLimit,
                                                    UpperLimit = prevCheckResult.UpperLimit,
                                                    Unit = prevCheckResult.Unit,
                                                    AbnormalReasonList = db.CheckResultAbnormalReason.Where(a => a.CheckResultUniqueID == prevCheckResult.UniqueID).OrderBy(a => a.AbnormalReasonID).Select(a => new PrevCheckResultAbnormalReasonModel
                                                    {
                                                        Description = a.AbnormalReasonDescription,
                                                        Remark = a.AbnormalReasonRemark,
                                                        HandlingMethodList = db.CheckResultHandlingMethod.Where(h => h.CheckResultUniqueID == prevCheckResult.UniqueID && h.AbnormalReasonUniqueID == a.AbnormalReasonUniqueID).OrderBy(h => h.HandlingMethodID).Select(h => new PrevCheckResultHandlingMethodModel
                                                        {
                                                            Description = h.HandlingMethodDescription,
                                                            Remark = h.HandlingMethodRemark
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
                                var equipmentList = (from x in db.JobEquipment
                                                     join e in db.Equipment
                                                     on x.EquipmentUniqueID equals e.UniqueID
                                                     where x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID
                                                     select new
                                                     {
                                                         UniqueID = e.UniqueID,
                                                         e.ID,
                                                         Name = e.Name,
                                                         IsFeelItemDefaultNormal = e.IsFeelItemDefaultNormal 
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

                                    var specList = (from x in db.EquipmentSpecValue
                                                    join s in db.EquipmentSpec
                                                    on x.SpecUniqueID equals s.UniqueID
                                                    where x.EquipmentUniqueID == equipment.UniqueID
                                                    select new
                                                    {
                                                        EquipmentUniqueID = equipment.UniqueID,
                                                        SpecOptionUniqueID = x.SpecOptionUniqueID,
                                                        Spec = s.Description,
                                                        Input = x.Value
                                                    }).ToList();

                                    foreach (var spec in specList)
                                    {
                                        var specOption = db.EquipmentSpecOption.FirstOrDefault(x => x.UniqueID == spec.SpecOptionUniqueID);

                                        equipmentModel.SpecList.Add(new EquipmentSpecModel
                                        {
                                            EquipmentUniqueID = equipment.UniqueID,
                                            Spec = spec.Spec,
                                            Option = specOption != null ? specOption.Description : "",
                                            Input = spec.Input
                                        });
                                    }

                                    var partList = (from x in db.JobEquipment
                                                    join j in db.Job
                                                    on x.JobUniqueID equals j.UniqueID
                                                    join y in db.RouteEquipment
                                                    on new { j.RouteUniqueID, x.ControlPointUniqueID, x.EquipmentUniqueID, x.PartUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.EquipmentUniqueID, y.PartUniqueID }
                                                    where x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == equipment.UniqueID
                                                    select new
                                                    {
                                                        UniqueID = x.PartUniqueID,
                                                        y.Seq
                                                    }).ToList();

                                    foreach (var part in partList)
                                    {
                                        var p = db.EquipmentPart.FirstOrDefault(x => x.UniqueID == part.UniqueID);

                                        var partModel = new PartModel()
                                        {
                                            UniqueID = part.UniqueID,
                                            Description = p != null ? p.Description : "",
                                            Seq = part.Seq,
                                            FileList = db.File.Where(x => x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == part.UniqueID && x.IsDownload2Mobile).Select(x => new FileModel
                                            {
                                                UniqueID = x.UniqueID,
                                                FileName = x.FileName,
                                                Extension = x.Extension
                                            }).ToList(),
                                            MaterialList = (from x in db.EquipmentMaterial
                                                            join m in db.Material
                                                            on x.MaterialUniqueID equals m.UniqueID
                                                            where x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == part.UniqueID
                                                            select new MaterialModel
                                                            {
                                                                EquipmentUniqueID = equipment.UniqueID,
                                                                PartUniqueID = part.UniqueID,
                                                                UniqueID = m.UniqueID,
                                                                ID = m.ID,
                                                                Name = m.Name,
                                                                Quantity = x.Quantity,
                                                                SpecList = (from sv in db.MaterialSpecValue
                                                                            join s in db.MaterialSpec
                                                                            on sv.SpecUniqueID equals s.UniqueID
                                                                            where sv.MaterialUniqueID == m.UniqueID
                                                                            select new MaterialSpecModel
                                                                            {
                                                                                MaterialUniqueID = m.UniqueID,
                                                                                Spec = s.Description,
                                                                                Input = sv.Value
                                                                            }).ToList()
                                                            }).ToList()
                                        };

                                        #region CheckItem
                                        var equipmentCheckItemList = (from x in db.JobEquipmentCheckItem
                                                                      join j in db.Job
                                                                      on x.JobUniqueID equals j.UniqueID
                                                                      join y in db.RouteEquipmentCheckItem
                                                                      on new { j.RouteUniqueID, x.ControlPointUniqueID, x.EquipmentUniqueID, x.PartUniqueID, x.CheckItemUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.EquipmentUniqueID, y.PartUniqueID, y.CheckItemUniqueID }
                                                                      join c in db.EquipmentCheckItem
                                                                      on new { x.EquipmentUniqueID, x.PartUniqueID, x.CheckItemUniqueID } equals new { c.EquipmentUniqueID, c.PartUniqueID, c.CheckItemUniqueID }
                                                                      join item in db.CheckItem
                                                                    on x.CheckItemUniqueID equals item.UniqueID
                                                                      where x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == part.UniqueID
                                                                      select new
                                                                      {
                                                                          UniqueID = item.UniqueID,
                                                                          item.ID,
                                                                          Description = item.Description,
                                                                          IsFeelItem = item.IsFeelItem ,
                                                                          LowerLimit = c.IsInherit  ? item.LowerLimit : c.LowerLimit,
                                                                          LowerAlertLimit = c.IsInherit  ? item.LowerAlertLimit : c.LowerAlertLimit,
                                                                          UpperAlertLimit = c.IsInherit  ? item.UpperAlertLimit : c.UpperAlertLimit,
                                                                          UpperLimit = c.IsInherit  ? item.UpperLimit : c.UpperLimit,
                                                                          Remark = c.IsInherit  ? item.Remark : c.Remark,
                                                                          Unit = c.IsInherit  ? item.Unit : c.Unit,
                                                                          Seq = y.Seq
                                                                      }).ToList();

                                        foreach (var checkItem in equipmentCheckItemList)
                                        {
                                            bool except = false;

                                            if (parameter.IsExceptChecked)
                                            {
                                                var prevCheckResult = db.CheckResult.Where(x => x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == part.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID).FirstOrDefault();

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
                                                    Seq = checkItem.Seq,
                                                    FeelOptionList = db.CheckItemFeelOption.Where(x => x.CheckItemUniqueID == checkItem.UniqueID).Select(x => new FeelOptionModel
                                                    {
                                                        UniqueID = x.UniqueID,
                                                        Description = x.Description,
                                                        IsAbnormal = x.IsAbnormal,
                                                        Seq = x.Seq
                                                    }).ToList(),
                                                    AbnormalReasonList = (from ca in db.CheckItemAbnormalReason
                                                                          join a in db.AbnormalReason
                                                                          on ca.AbnormalReasonUniqueID equals a.UniqueID
                                                                          where ca.CheckItemUniqueID == checkItem.UniqueID
                                                                          select new AbnormalReasonModel
                                                                          {
                                                                              UniqueID = a.UniqueID,
                                                                              ID = a.ID,
                                                                              Description = a.Description,
                                                                              HandlingMethodList = (from ah in db.AbnormalReasonHandlingMethod
                                                                                                    join h in db.HandlingMethod
                                                                                                        on ah.HandlingMethodUniqueID equals h.UniqueID
                                                                                                    where ah.AbnormalReasonUniqueID == a.UniqueID
                                                                                                    select new HandlingMethodModel
                                                                                                    {
                                                                                                        UniqueID = h.UniqueID,
                                                                                                        ID = h.ID,
                                                                                                        Description = h.Description
                                                                                                    }).ToList()
                                                                          }).ToList()
                                                };

                                                if (job.IsShowPrevRecord)
                                                {
                                                    var prevCheckResult = db.CheckResult.Where(x => x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == part.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID).OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();

                                                    if (prevCheckResult != null && !DataModel.PrevCheckResultList.Any(x => x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == part.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID))
                                                    {
                                                        DataModel.PrevCheckResultList.Add(new PrevCheckResultModel()
                                                        {
                                                            ControlPointUniqueID = controlPoint.UniqueID,
                                                            EquipmentUniqueID = equipment.UniqueID,
                                                            PartUniqueID = part.UniqueID,
                                                            CheckItemUniqueID = checkItem.UniqueID,
                                                            CheckDate = prevCheckResult.CheckDate,
                                                            CheckTime = prevCheckResult.CheckTime,
                                                            Result = prevCheckResult.Result,
                                                            IsAbnormal = prevCheckResult.IsAbnormal ,
                                                            LowerLimit = prevCheckResult.LowerLimit,
                                                            LowerAlertLimit = prevCheckResult.LowerAlertLimit,
                                                            UpperAlertLimit = prevCheckResult.UpperAlertLimit,
                                                            UpperLimit = prevCheckResult.UpperLimit,
                                                            Unit = prevCheckResult.Unit,
                                                            AbnormalReasonList = db.CheckResultAbnormalReason.Where(a => a.CheckResultUniqueID == prevCheckResult.UniqueID).OrderBy(a => a.AbnormalReasonID).Select(a => new PrevCheckResultAbnormalReasonModel
                                                            {
                                                                Description = a.AbnormalReasonDescription,
                                                                Remark = a.AbnormalReasonRemark,
                                                                HandlingMethodList = db.CheckResultHandlingMethod.Where(h => h.CheckResultUniqueID == prevCheckResult.UniqueID && h.AbnormalReasonUniqueID == a.AbnormalReasonUniqueID).OrderBy(h => h.HandlingMethodID).Select(h => new PrevCheckResultHandlingMethodModel
                                                                {
                                                                    Description = h.HandlingMethodDescription,
                                                                    Remark = h.HandlingMethodRemark
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
                            var form = (from f in db.MForm
                                        join j in db.MJob
                                        on f.MJobUniqueID equals j.UniqueID
                                        join e in db.Equipment
                                        on f.EquipmentUniqueID equals e.UniqueID
                                        join p in db.EquipmentPart
                                        on f.PartUniqueID equals p.UniqueID into tmpPart
                                        from p in tmpPart.DefaultIfEmpty()
                                        where f.UniqueID == parameter.MaintenanceFormUniqueID
                                        select new
                                        {
                                            MaintenanceForm = f,
                                            Job = j,
                                            Equipment = e,
                                            PartDescription = p != null ? p.Description : ""
                                        }).FirstOrDefault();

                            if (form != null)
                            {
                                var mFormModel = new MaintenanceFormModel()
                                {
                                    UniqueID = form.MaintenanceForm.UniqueID,
                                    VHNO = form.MaintenanceForm.VHNO,
                                    Description = form.Job.Description,
                                    EquipmentID = form.Equipment.ID,
                                    EquipmentName = form.Equipment.Name,
                                    PartDescription = form.PartDescription,
                                    BeginDate = form.MaintenanceForm.EstBeginDate,
                                    EndDate = form.MaintenanceForm.EstEndDate,
                                    Remark = form.Job.Remark,
                                    UserList = (from f in db.MForm
                                                join x in db.MJobUser
                                                on f.MJobUniqueID equals x.MJobUniqueID
                                                where f.UniqueID == parameter.MaintenanceFormUniqueID
                                                select new UserModel { ID = x.UserID }).ToList()
                                };

                                var standardList = (from x in db.MJobEquipmentStandard
                                                    join y in db.EquipmentStandard
                                                    on new { x.EquipmentUniqueID, x.PartUniqueID, x.StandardUniqueID } equals new { y.EquipmentUniqueID, y.PartUniqueID, y.StandardUniqueID }
                                                    join s in db.Standard
                                                    on y.StandardUniqueID equals s.UniqueID
                                                    where x.MJobUniqueID == form.Job.UniqueID
                                                    select new
                                                    {
                                                        UniqueID = s.UniqueID,
                                                        MaintenanceType = s.MaintenanceType,
                                                        s.ID,
                                                        Description = s.Description,
                                                        IsFeelItem = s.IsFeelItem,
                                                        LowerLimit = y.IsInherit ? s.LowerLimit : y.LowerLimit,
                                                        LowerAlertLimit = y.IsInherit  ? s.LowerAlertLimit : y.LowerAlertLimit,
                                                        UpperAlertLimit = y.IsInherit  ? s.UpperAlertLimit : y.UpperAlertLimit,
                                                        UpperLimit = y.IsInherit  ? s.UpperLimit : y.UpperLimit,
                                                        Remark = y.IsInherit  ? s.Remark : y.Remark,
                                                        Unit = y.IsInherit  ? s.Unit : y.Unit
                                                    }).Distinct().OrderBy(x => x.MaintenanceType).ThenBy(x => x.ID).ToList();

                                int seq = 1;

                                foreach (var standard in standardList)
                                {
                                    mFormModel.StandardList.Add(new StandardModel()
                                    {
                                        UniqueID = standard.UniqueID,
                                        ID = standard.ID,
                                        Description = standard.Description,
                                        IsFeelItem = standard.IsFeelItem ,
                                        LowerLimit = standard.LowerLimit,
                                        LowerAlertLimit = standard.LowerAlertLimit,
                                        UpperAlertLimit = standard.UpperAlertLimit,
                                        UpperLimit = standard.UpperLimit,
                                        Remark = standard.Remark,
                                        Unit = standard.Unit,
                                        Seq = seq,
                                        FeelOptionList = db.StandardFeelOption.Where(x => x.StandardUniqueID == standard.UniqueID).Select(x => new FeelOptionModel
                                        {
                                            UniqueID = x.UniqueID,
                                            Description = x.Description,
                                            IsAbnormal = x.IsAbnormal,
                                            Seq = x.Seq
                                        }).ToList(),
                                        AbnormalReasonList = (from sa in db.StandardAbnormalReason
                                                              join a in db.AbnormalReason
                                                              on sa.AbnormalReasonUniqueID equals a.UniqueID
                                                              where sa.StandardUniqueID == standard.UniqueID
                                                              select new AbnormalReasonModel
                                                              {
                                                                  UniqueID = a.UniqueID,
                                                                  ID = a.ID,
                                                                  Description = a.Description,
                                                                  HandlingMethodList = (from ah in db.AbnormalReasonHandlingMethod
                                                                                        join h in db.HandlingMethod
                                                                                        on ah.HandlingMethodUniqueID equals h.UniqueID
                                                                                        where ah.AbnormalReasonUniqueID == a.UniqueID
                                                                                        select new HandlingMethodModel
                                                                                        {
                                                                                            UniqueID = h.UniqueID,
                                                                                            ID = h.ID,
                                                                                            Description = h.Description
                                                                                        }).ToList()
                                                              }).ToList()
                                    });

                                    seq++;
                                }

                                var materialList = (from x in db.MJobEquipmentMaterial
                                                    join m in db.Material
                                                    on x.MaterialUniqueID equals m.UniqueID
                                                    where x.MJobUniqueID == form.Job.UniqueID
                                                    select new
                                                    {
                                                        UniqueID = m.UniqueID,
                                                        m.ID,
                                                        Name = m.Name,
                                                        x.Quantity
                                                    }).OrderBy(x => x.ID).ToList();

                                foreach (var material in materialList)
                                {
                                    mFormModel.MaterialList.Add(new MFormMaterialModel()
                                    {
                                        UniqueID = material.UniqueID,
                                        ID = material.ID,
                                        Name = material.Name,
                                        Quantity = material.Quantity.Value
                                    });
                                }

                                DataModel.MaintenanceFormList.Add(mFormModel);
                            }
                        }
                        else if (!string.IsNullOrEmpty(parameter.RepairFormUniqueID))
                        {
                            //#region Equipment Maintenance

                            var repairForm = (from x in db.RForm
                                              join t in db.RFormType
                                              on x.RFormTypeUniqueID equals t.UniqueID into tmpType
                                              from t in tmpType.DefaultIfEmpty()
                                              join e in db.Equipment
                                              on x.EquipmentUniqueID equals e.UniqueID into tmpEquipment
                                              from e in tmpEquipment.DefaultIfEmpty()
                                              join p in db.EquipmentPart
                                              on x.PartUniqueID equals p.UniqueID into tmpPart
                                              from p in tmpPart.DefaultIfEmpty()
                                              where x.UniqueID == parameter.RepairFormUniqueID
                                              select new
                                              {
                                                  x.UniqueID,
                                                  x.Subject,
                                                  x.Description,
                                                  x.VHNO,
                                                  x.EstBeginDate,
                                                  x.EstEndDate,
                                                  RepairFormType = t != null ? t.Description : "",
                                                  EquipmentID = e != null ? e.ID : "",
                                                  EquipmentName = e != null ? e.Name : "",
                                                  PartDescription = p != null ? p.Description : ""
                                              }).First();

                            var repairFormModel = new RepairFormModel()
                            {
                                UniqueID = repairForm.UniqueID,
                                Subject = repairForm.Subject,
                                Description = repairForm.Description,
                                VHNO = repairForm.VHNO,
                                RepairFormType = repairForm.RepairFormType,
                                EquipmentID = repairForm.EquipmentID,
                                EquipmentName = repairForm.EquipmentName,
                                PartDescription = repairForm.PartDescription,
                                BeginDate = repairForm.EstBeginDate,
                                EndDate = repairForm.EstEndDate,
                                UserList = db.RFormJobUser.Where(x => x.RFormUniqueID == repairForm.UniqueID).Select(x => new UserModel
                                {
                                    ID = x.UserID
                                }).ToList()
                            };

                            DataModel.RepairFormList.Add(repairFormModel);

                            //#endregion
                        }
                    }
                }

                using (DbEntities db = new DbEntities())
                {
                    foreach (var repairForm in DataModel.RepairFormList)
                    {
                        repairForm.UserList = (from x in repairForm.UserList
                                               join u in db.User
                                               on x.ID equals u.ID
                                               select new UserModel
                                               {
                                                   ID = u.ID,
                                                   Name = u.Name,
                                                   Title = u.Title,
                                                   Password = u.Password,
                                                   UID = u.UID
                                               }).ToList();
                    }

                    foreach (var maintenanceForm in DataModel.MaintenanceFormList)
                    {
                        maintenanceForm.UserList = (from x in maintenanceForm.UserList
                                                    join u in db.User
                                                    on x.ID equals u.ID
                                                    select new UserModel
                                                    {
                                                        ID = u.ID,
                                                        Name = u.Name,
                                                        Title = u.Title,
                                                        Password = u.Password,
                                                        UID = u.UID
                                                    }).ToList();
                    }

                    foreach (var job in DataModel.JobList)
                    {
                        job.UserList = (from x in job.UserList
                                        join u in db.User
                                        on x.ID equals u.ID
                                        select new UserModel
                                        {
                                            ID = u.ID,
                                            Name = u.Name,
                                            Title = u.Title,
                                            Password = u.Password,
                                            UID = u.UID
                                        }).ToList();

                        #region EmgContactList
                        //var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(job.OrganizationUniqueID, true);

                        //job.EmgContactList = (from e in db.EMGCONTACT
                        //                      join u in db.User
                        //                      on e.UserID equals u.ID into tmpUser
                        //                      from u in tmpUser.DefaultIfEmpty()
                        //                      where upStreamOrganizationList.Contains(e.OrganizationUniqueID)
                        //                      select new EmgContactModel
                        //                      {
                        //                          UniqueID = e.UniqueID,
                        //                          Title = u != null ? u.Title : e.Title,
                        //                          Name = u != null ? u.Name : e.Name,
                        //                          TelList = db.EMGCONTACTTEL.Where(x => x.EMGCONTACTUniqueID == e.UniqueID).Select(x => new EmgContactTelModel
                        //                          {
                        //                              Seq = x.Seq,
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
#else
        private RequestResult Query(DownloadFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                var checkDate = DateTimeHelper.DateString2DateTime(Model.CheckDate).Value;

                using (EDbEntities db = new EDbEntities())
                {
                    #region UnRFIDReason
                    DataModel.UnPatrolReasonList = db.UnPatrolReason.Select(x => new Models.EquipmentMaintenance.DataSync.UnPatrolReason
                    {
                        UniqueID = x.UniqueID,
                        ID = x.ID,
                        Description = x.Description,
                        LastModifyTime = x.LastModifyTime
                    }).ToList();
                    #endregion

                    #region UnRFIDReason
                    DataModel.UnRFIDReasonList = db.UnRFIDReason.Select(x => new Models.EquipmentMaintenance.DataSync.UnRFIDReason
                    {
                        UniqueID = x.UniqueID,
                        ID = x.ID,
                        Description = x.Description,
                        LastModifyTime = x.LastModifyTime
                    }).ToList();
                    #endregion

                    #region OverTimeReason
                    DataModel.OverTimeReasonList = db.OverTimeReason.Select(x => new Models.EquipmentMaintenance.DataSync.OverTimeReason
                    {
                        UniqueID = x.UniqueID,
                        ID = x.ID,
                        Description = x.Description,
                        LastModifyTime = x.LastModifyTime
                    }).ToList();
                    #endregion

                    #region TimeSpanAbnormalReason
                    DataModel.TimeSpanAbnormalReasonList = db.TimeSpanAbnormalReason.Select(x => new Models.EquipmentMaintenance.DataSync.TimeSpanAbnormalReason
                    {
                        UniqueID = x.UniqueID,
                        ID = x.ID,
                        Description = x.Description,
                        LastModifyTime = x.LastModifyTime
                    }).ToList();
                    #endregion

                    foreach (var parameter in Model.Parameters)
                    {
                        if (!string.IsNullOrEmpty(parameter.JobUniqueID))
                        {
                            #region Job
                            var job = (from j in db.Job
                                       join r in db.Route
                                       on j.RouteUniqueID equals r.UniqueID
                                       where j.UniqueID == parameter.JobUniqueID
                                       select new
                                       {
                                           UniqueID = j.UniqueID,
                                           OrganizationUniqueID = r.OrganizationUniqueID,
                                           Description = j.Description,
                                           RouteID = r.ID,
                                           RouteName = r.Name,
                                           BeginDate = j.BeginDate,
                                           EndDate = j.EndDate,
                                           TimeMode = j.TimeMode,
                                           BeginTime = j.BeginTime,
                                           EndTime = j.EndTime,
                                           CycleCount = j.CycleCount,
                                           CycleMode = j.CycleMode,
                                           IsCheckBySeq = j.IsCheckBySeq,
                                           IsShowPrevRecord = j.IsShowPrevRecord ,
                                           Remark = j.Remark
                                       }).First();

                            var jobBeginDateString = string.Empty;
                            var jobEndDateString = string.Empty;

                            if (parameter.IsExceptChecked)
                            {
                                DateTime beginDate, endDate;

                                JobCycleHelper.GetDateSpan(checkDate, job.BeginDate, job.EndDate, job.CycleCount, job.CycleMode, out beginDate, out endDate);

                                jobBeginDateString = DateTimeHelper.DateTime2DateString(beginDate);
                                jobEndDateString = DateTimeHelper.DateTime2DateString(endDate);

                                if (!string.IsNullOrEmpty(job.BeginTime) && !string.IsNullOrEmpty(job.EndTime) && string.Compare(job.BeginTime, job.EndTime) > 0)
                                {
                                    jobEndDateString = DateTimeHelper.DateTime2DateString(endDate.AddDays(1));
                                }
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
                                UserList = (from x in db.JobUser
                                            where x.JobUniqueID == job.UniqueID
                                            select new UserModel
                                            {
                                                ID = x.UserID
                                            }).ToList()
                            };

                            #region ControlPoint
                            var controlPointList = (from x in db.JobControlPoint
                                                    join j in db.Job
                                                    on x.JobUniqueID equals j.UniqueID
                                                    join y in db.RouteControlPoint
                                                    on new { j.RouteUniqueID, x.ControlPointUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID }
                                                    join c in db.ControlPoint
                                                    on x.ControlPointUniqueID equals c.UniqueID
                                                    where x.JobUniqueID == job.UniqueID
                                                    select new
                                                    {
                                                        UniqueID = c.UniqueID,
                                                        c.ID,
                                                        Description = c.Description,
                                                        IsFeelItemDefaultNormal = c.IsFeelItemDefaultNormal,
                                                        TagID = c.TagID,
                                                        MinTimeSpan = x.MinTimeSpan,
                                                        Remark = c.Remark,
                                                        Seq = y.Seq
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
                                var controlPointCheckItemList = (from x in db.JobControlPointCheckItem
                                                                 join j in db.Job
                                                                 on x.JobUniqueID equals j.UniqueID
                                                                 join y in db.RouteControlPointCheckItem
                                                                 on new { j.RouteUniqueID, x.ControlPointUniqueID, x.CheckItemUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.CheckItemUniqueID }
                                                                 join c in db.ControlPointCheckItem
                                                                 on new { x.ControlPointUniqueID, x.CheckItemUniqueID } equals new { c.ControlPointUniqueID, c.CheckItemUniqueID }
                                                                 join item in db.CheckItem
                                                                 on x.CheckItemUniqueID equals item.UniqueID
                                                                 where x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID
                                                                 select new
                                                                 {
                                                                     UniqueID = item.UniqueID,
                                                                     item.ID,
                                                                     Description = item.Description,
                                                                     IsFeelItem = item.IsFeelItem,
                                                                     TextValueType = item.TextValueType,
                                                                     LowerLimit = c.IsInherit  ? item.LowerLimit : c.LowerLimit,
                                                                     LowerAlertLimit = c.IsInherit ? item.LowerAlertLimit : c.LowerAlertLimit,
                                                                     UpperAlertLimit = c.IsInherit ? item.UpperAlertLimit : c.UpperAlertLimit,
                                                                     UpperLimit = c.IsInherit ? item.UpperLimit : c.UpperLimit,
                                                                     Remark = c.IsInherit ? item.Remark : c.Remark,
                                                                     Unit = c.IsInherit ? item.Unit : c.Unit,
                                                                     Seq = y.Seq
                                                                 }).ToList();

                                foreach (var checkItem in controlPointCheckItemList)
                                {
                                    bool except = false;

                                    if (parameter.IsExceptChecked)
                                    {
                                        var prevCheckResult = db.CheckResult.Where(x => x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID && string.Compare(x.CheckDate, jobBeginDateString) >= 0 && string.Compare(x.CheckDate, jobEndDateString) <= 0).FirstOrDefault();

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
                                            TextValueType = checkItem.TextValueType,
                                            LowerLimit = checkItem.LowerLimit,
                                            LowerAlertLimit = checkItem.LowerAlertLimit,
                                            UpperAlertLimit = checkItem.UpperAlertLimit,
                                            UpperLimit = checkItem.UpperLimit,
                                            Remark = checkItem.Remark,
                                            Unit = checkItem.Unit,
                                            Seq = checkItem.Seq,
                                            FeelOptionList = db.CheckItemFeelOption.Where(x => x.CheckItemUniqueID == checkItem.UniqueID).Select(x => new FeelOptionModel
                                            {
                                                UniqueID = x.UniqueID,
                                                Description = x.Description,
                                                IsAbnormal = x.IsAbnormal,
                                                Seq = x.Seq
                                            }).ToList(),
                                            AbnormalReasonList = (from ca in db.CheckItemAbnormalReason
                                                                  join a in db.AbnormalReason
                                                                  on ca.AbnormalReasonUniqueID equals a.UniqueID
                                                                  where ca.CheckItemUniqueID == checkItem.UniqueID
                                                                  select new AbnormalReasonModel
                                                                  {
                                                                      UniqueID = a.UniqueID,
                                                                      ID = a.ID,
                                                                      Description = a.Description,
                                                                      HandlingMethodList = (from ah in db.AbnormalReasonHandlingMethod
                                                                                            join h in db.HandlingMethod
                                                                                                on ah.HandlingMethodUniqueID equals h.UniqueID
                                                                                            where ah.AbnormalReasonUniqueID == a.UniqueID
                                                                                            select new HandlingMethodModel
                                                                                            {
                                                                                                UniqueID = h.UniqueID,
                                                                                                ID = h.ID,
                                                                                                Description = h.Description
                                                                                            }).ToList()
                                                                  }).ToList()
                                        };

                                        if (job.IsShowPrevRecord)
                                        {
                                            var prevCheckResult = db.CheckResult.Where(x => x.ControlPointUniqueID == controlPoint.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID).OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();

                                            if (prevCheckResult != null && !DataModel.PrevCheckResultList.Any(x => x.ControlPointUniqueID == controlPoint.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID))
                                            {
                                                DataModel.PrevCheckResultList.Add(new PrevCheckResultModel()
                                                {
                                                    ControlPointUniqueID = controlPoint.UniqueID,
                                                    EquipmentUniqueID = "",
                                                    PartUniqueID = "",
                                                    CheckItemUniqueID = checkItem.UniqueID,
                                                    CheckDate = prevCheckResult.CheckDate,
                                                    CheckTime = prevCheckResult.CheckTime,
                                                    Result = prevCheckResult.Result,
                                                    IsAbnormal = prevCheckResult.IsAbnormal ,
                                                    LowerLimit = prevCheckResult.LowerLimit,
                                                    LowerAlertLimit = prevCheckResult.LowerAlertLimit,
                                                    UpperAlertLimit = prevCheckResult.UpperAlertLimit,
                                                    UpperLimit = prevCheckResult.UpperLimit,
                                                    Unit = prevCheckResult.Unit,
                                                    AbnormalReasonList = db.CheckResultAbnormalReason.Where(a => a.CheckResultUniqueID == prevCheckResult.UniqueID).OrderBy(a => a.AbnormalReasonID).Select(a => new PrevCheckResultAbnormalReasonModel
                                                    {
                                                        Description = a.AbnormalReasonDescription,
                                                        Remark = a.AbnormalReasonRemark,
                                                        HandlingMethodList = db.CheckResultHandlingMethod.Where(h => h.CheckResultUniqueID == prevCheckResult.UniqueID && h.AbnormalReasonUniqueID == a.AbnormalReasonUniqueID).OrderBy(h => h.HandlingMethodID).Select(h => new PrevCheckResultHandlingMethodModel
                                                        {
                                                            Description = h.HandlingMethodDescription,
                                                            Remark = h.HandlingMethodRemark
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
                                var equipmentList = (from x in db.JobEquipment
                                                     join e in db.Equipment
                                                     on x.EquipmentUniqueID equals e.UniqueID
                                                     where x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID
                                                     select new
                                                     {
                                                         UniqueID = e.UniqueID,
                                                         e.ID,
                                                         Name = e.Name,
                                                         IsFeelItemDefaultNormal = e.IsFeelItemDefaultNormal 
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

                                    var specList = (from x in db.EquipmentSpecValue
                                                    join s in db.EquipmentSpec
                                                    on x.SpecUniqueID equals s.UniqueID
                                                    //join o in db.EquipmentSpecOption
                                                    //on x.SpecOptionUniqueID equals o.UniqueID into tmpSpecOption
                                                    //from so in tmpSpecOption.DefaultIfEmpty()
                                                    where x.EquipmentUniqueID == equipment.UniqueID
                                                    select new
                                                    {
                                                        EquipmentUniqueID = equipment.UniqueID,
                                                        SpecOptionUniqueID = x.SpecOptionUniqueID,
                                                        Spec = s.Description,
                                                        //Option = so != null ? so.Description : "",
                                                        Input = x.Value
                                                    }).ToList();

                                    foreach (var spec in specList)
                                    {
                                        var specOption = db.EquipmentSpecOption.FirstOrDefault(x => x.UniqueID == spec.SpecOptionUniqueID);

                                        equipmentModel.SpecList.Add(new EquipmentSpecModel
                                        {
                                            EquipmentUniqueID = equipment.UniqueID,
                                            Spec = spec.Spec,
                                            Option = specOption != null ? specOption.Description : "",
                                            Input = spec.Input
                                        });
                                    }

                                    var partList = (from x in db.JobEquipment
                                                    join j in db.Job
                                                    on x.JobUniqueID equals j.UniqueID
                                                    join y in db.RouteEquipment
                                                    on new { j.RouteUniqueID, x.ControlPointUniqueID, x.EquipmentUniqueID, x.PartUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.EquipmentUniqueID, y.PartUniqueID }
                                                    //join p in db.EquipmentPart
                                                    //on new { EquipmentUniqueID = x.EquipmentUniqueID, PartUniqueID = x.PartUniqueID } equals new { EquipmentUniqueID = p.EquipmentUniqueID, PartUniqueID = p.UniqueID } into tmpPart
                                                    //from p in tmpPart.DefaultIfEmpty()
                                                    where x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == equipment.UniqueID
                                                    select new
                                                    {
                                                        UniqueID = x.PartUniqueID,
                                                        //Description = p != null ? p.Description : "",
                                                        y.Seq
                                                    }).ToList();

                                    foreach (var part in partList)
                                    {
                                        var p = db.EquipmentPart.FirstOrDefault(x => x.UniqueID == part.UniqueID);

                                        var partModel = new PartModel()
                                        {
                                            UniqueID = part.UniqueID,
                                            Description = p != null ? p.Description : "",
                                            Seq = part.Seq,
                                            FileList = db.File.Where(x => x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == part.UniqueID && x.IsDownload2Mobile).Select(x => new FileModel
                                            {
                                                UniqueID = x.UniqueID,
                                                FileName = x.FileName,
                                                Extension = x.Extension
                                            }).ToList(),
                                            MaterialList = (from x in db.EquipmentMaterial
                                                            join m in db.Material
                                                            on x.MaterialUniqueID equals m.UniqueID
                                                            where x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == part.UniqueID
                                                            select new MaterialModel
                                                            {
                                                                EquipmentUniqueID = equipment.UniqueID,
                                                                PartUniqueID = part.UniqueID,
                                                                UniqueID = m.UniqueID,
                                                                ID = m.ID,
                                                                Name = m.Name,
                                                                Quantity = x.Quantity,
                                                                SpecList = (from sv in db.MaterialSpecValue
                                                                            join s in db.MaterialSpec
                                                                            on sv.SpecUniqueID equals s.UniqueID
                                                                            //join so in db.MaterialSpecOption
                                                                            //on sv.SpecOptionUniqueID equals so.UniqueID into tmpSpecOption
                                                                            //from so in tmpSpecOption.DefaultIfEmpty()
                                                                            where sv.MaterialUniqueID == m.UniqueID
                                                                            select new MaterialSpecModel
                                                                            {
                                                                                MaterialUniqueID = m.UniqueID,
                                                                                Spec = s.Description,
                                                                                //Option = so != null ? so.Description : "",
                                                                                Input = sv.Value
                                                                            }).ToList()
                                                            }).ToList()
                                        };

        #region CheckItem
                                        var equipmentCheckItemList = (from x in db.JobEquipmentCheckItem
                                                                      join j in db.Job
                                                                      on x.JobUniqueID equals j.UniqueID
                                                                      join y in db.RouteEquipmentCheckItem
                                                                      on new { j.RouteUniqueID, x.ControlPointUniqueID, x.EquipmentUniqueID, x.PartUniqueID, x.CheckItemUniqueID } equals new { y.RouteUniqueID, y.ControlPointUniqueID, y.EquipmentUniqueID, y.PartUniqueID, y.CheckItemUniqueID }
                                                                      join c in db.EquipmentCheckItem
                                                                      on new { x.EquipmentUniqueID, x.PartUniqueID, x.CheckItemUniqueID } equals new { c.EquipmentUniqueID, c.PartUniqueID, c.CheckItemUniqueID }
                                                                      join item in db.CheckItem
                                                                    on x.CheckItemUniqueID equals item.UniqueID
                                                                      where x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == part.UniqueID
                                                                      select new
                                                                      {
                                                                          UniqueID = item.UniqueID,
                                                                          //UniqueID = c.CheckItemUniqueID,
                                                                          item.ID,
                                                                          Description = item.Description,
                                                                          IsFeelItem = item.IsFeelItem ,
                                                                          TextValueType = item.TextValueType,
                                                                          LowerLimit = c.IsInherit  ? item.LowerLimit : c.LowerLimit,
                                                                          LowerAlertLimit = c.IsInherit  ? item.LowerAlertLimit : c.LowerAlertLimit,
                                                                          UpperAlertLimit = c.IsInherit  ? item.UpperAlertLimit : c.UpperAlertLimit,
                                                                          UpperLimit = c.IsInherit  ? item.UpperLimit : c.UpperLimit,
                                                                          Remark = c.IsInherit  ? item.Remark : c.Remark,
                                                                          Unit = c.IsInherit  ? item.Unit : c.Unit,
                                                                          Seq = y.Seq
                                                                      }).ToList();

                                        foreach (var checkItem in equipmentCheckItemList)
                                        {
                                            bool except = false;

                                            if (parameter.IsExceptChecked)
                                            {
                                                var prevCheckResult = db.CheckResult.Where(x => x.JobUniqueID == job.UniqueID && x.ControlPointUniqueID == controlPoint.UniqueID && x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == part.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID && string.Compare(x.CheckDate, jobBeginDateString) >= 0 && string.Compare(x.CheckDate, jobEndDateString) <= 0).FirstOrDefault();

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
                                                    TextValueType =checkItem.TextValueType,
                                                    LowerLimit = checkItem.LowerLimit,
                                                    LowerAlertLimit = checkItem.LowerAlertLimit,
                                                    UpperAlertLimit = checkItem.UpperAlertLimit,
                                                    UpperLimit = checkItem.UpperLimit,
                                                    Remark = checkItem.Remark,
                                                    Unit = checkItem.Unit,
                                                    Seq = checkItem.Seq,
                                                    FeelOptionList = db.CheckItemFeelOption.Where(x => x.CheckItemUniqueID == checkItem.UniqueID).Select(x => new FeelOptionModel
                                                    {
                                                        UniqueID = x.UniqueID,
                                                        Description = x.Description,
                                                        IsAbnormal = x.IsAbnormal,
                                                        Seq = x.Seq
                                                    }).ToList(),
                                                    AbnormalReasonList = (from ca in db.CheckItemAbnormalReason
                                                                          join a in db.AbnormalReason
                                                                          on ca.AbnormalReasonUniqueID equals a.UniqueID
                                                                          where ca.CheckItemUniqueID == checkItem.UniqueID
                                                                          select new AbnormalReasonModel
                                                                          {
                                                                              UniqueID = a.UniqueID,
                                                                              ID = a.ID,
                                                                              Description = a.Description,
                                                                              HandlingMethodList = (from ah in db.AbnormalReasonHandlingMethod
                                                                                                    join h in db.HandlingMethod
                                                                                                        on ah.HandlingMethodUniqueID equals h.UniqueID
                                                                                                    where ah.AbnormalReasonUniqueID == a.UniqueID
                                                                                                    select new HandlingMethodModel
                                                                                                    {
                                                                                                        UniqueID = h.UniqueID,
                                                                                                        ID = h.ID,
                                                                                                        Description = h.Description
                                                                                                    }).ToList()
                                                                          }).ToList()
                                                };

                                                if (job.IsShowPrevRecord)
                                                {
                                                    var prevCheckResult = db.CheckResult.Where(x => x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == part.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID).OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();

                                                    if (prevCheckResult != null && !DataModel.PrevCheckResultList.Any(x => x.EquipmentUniqueID == equipment.UniqueID && x.PartUniqueID == part.UniqueID && x.CheckItemUniqueID == checkItem.UniqueID))
                                                    {
                                                        DataModel.PrevCheckResultList.Add(new PrevCheckResultModel()
                                                        {
                                                            ControlPointUniqueID = controlPoint.UniqueID,
                                                            EquipmentUniqueID = equipment.UniqueID,
                                                            PartUniqueID = part.UniqueID,
                                                            CheckItemUniqueID = checkItem.UniqueID,
                                                            CheckDate = prevCheckResult.CheckDate,
                                                            CheckTime = prevCheckResult.CheckTime,
                                                            Result = prevCheckResult.Result,
                                                            IsAbnormal = prevCheckResult.IsAbnormal ,
                                                            LowerLimit = prevCheckResult.LowerLimit,
                                                            LowerAlertLimit = prevCheckResult.LowerAlertLimit,
                                                            UpperAlertLimit = prevCheckResult.UpperAlertLimit,
                                                            UpperLimit = prevCheckResult.UpperLimit,
                                                            Unit = prevCheckResult.Unit,
                                                            AbnormalReasonList = db.CheckResultAbnormalReason.Where(a => a.CheckResultUniqueID == prevCheckResult.UniqueID).OrderBy(a => a.AbnormalReasonID).Select(a => new PrevCheckResultAbnormalReasonModel
                                                            {
                                                                Description = a.AbnormalReasonDescription,
                                                                Remark = a.AbnormalReasonRemark,
                                                                HandlingMethodList = db.CheckResultHandlingMethod.Where(h => h.CheckResultUniqueID == prevCheckResult.UniqueID && h.AbnormalReasonUniqueID == a.AbnormalReasonUniqueID).OrderBy(h => h.HandlingMethodID).Select(h => new PrevCheckResultHandlingMethodModel
                                                                {
                                                                    Description = h.HandlingMethodDescription,
                                                                    Remark = h.HandlingMethodRemark
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

                        #region MaintenanceForm
                        else if (!string.IsNullOrEmpty(parameter.MaintenanceFormUniqueID))
                        {
                            var form = (from f in db.MForm
                                        join j in db.MJob
                                        on f.MJobUniqueID equals j.UniqueID
                                        join e in db.Equipment
                                        on f.EquipmentUniqueID equals e.UniqueID
                                        join p in db.EquipmentPart
                                        on f.PartUniqueID equals p.UniqueID into tmpPart
                                        from p in tmpPart.DefaultIfEmpty()
                                        where f.UniqueID == parameter.MaintenanceFormUniqueID
                                        select new
                                        {
                                            MaintenanceForm = f,
                                            Job = j,
                                            Equipment = e,
                                            PartDescription = p != null ? p.Description : ""
                                        }).FirstOrDefault();

                            if (form != null)
                            {
                                var mFormModel = new MaintenanceFormModel()
                                {
                                    UniqueID = form.MaintenanceForm.UniqueID,
                                    VHNO = form.MaintenanceForm.VHNO,
                                    Description = form.Job.Description,
                                    EquipmentID = form.Equipment.ID,
                                    EquipmentName = form.Equipment.Name,
                                    PartDescription = form.PartDescription,
                                    BeginDate = form.MaintenanceForm.EstBeginDate,
                                    EndDate = form.MaintenanceForm.EstEndDate,
                                    Remark = form.Job.Remark,
                                    UserList = (from f in db.MForm
                                                join x in db.MJobUser
                                                on f.MJobUniqueID equals x.MJobUniqueID
                                                where f.UniqueID == parameter.MaintenanceFormUniqueID
                                                select new UserModel { ID = x.UserID }).ToList()
                                };

                                var standardList = (from x in db.MJobEquipmentStandard
                                                    join y in db.EquipmentStandard
                                                    on new { x.EquipmentUniqueID, x.PartUniqueID, x.StandardUniqueID } equals new { y.EquipmentUniqueID, y.PartUniqueID, y.StandardUniqueID }
                                                    join s in db.Standard
                                                    on y.StandardUniqueID equals s.UniqueID
                                                    where x.MJobUniqueID == form.Job.UniqueID
                                                    select new
                                                    {
                                                        UniqueID = s.UniqueID,
                                                        MaintenanceType = s.MaintenanceType,
                                                        s.ID,
                                                        Description = s.Description,
                                                        IsFeelItem = s.IsFeelItem,
                                                        LowerLimit = y.IsInherit ? s.LowerLimit : y.LowerLimit,
                                                        LowerAlertLimit = y.IsInherit  ? s.LowerAlertLimit : y.LowerAlertLimit,
                                                        UpperAlertLimit = y.IsInherit  ? s.UpperAlertLimit : y.UpperAlertLimit,
                                                        UpperLimit = y.IsInherit  ? s.UpperLimit : y.UpperLimit,
                                                        Remark = y.IsInherit  ? s.Remark : y.Remark,
                                                        Unit = y.IsInherit  ? s.Unit : y.Unit
                                                    }).Distinct().OrderBy(x => x.MaintenanceType).ThenBy(x => x.ID).ToList();

                                int seq = 1;

                                foreach (var standard in standardList)
                                {
                                    mFormModel.StandardList.Add(new StandardModel()
                                    {
                                        UniqueID = standard.UniqueID,
                                        ID = standard.ID,
                                        Description = standard.Description,
                                        IsFeelItem = standard.IsFeelItem ,
                                        LowerLimit = standard.LowerLimit,
                                        LowerAlertLimit = standard.LowerAlertLimit,
                                        UpperAlertLimit = standard.UpperAlertLimit,
                                        UpperLimit = standard.UpperLimit,
                                        Remark = standard.Remark,
                                        Unit = standard.Unit,
                                        Seq = seq,
                                        FeelOptionList = db.StandardFeelOption.Where(x => x.StandardUniqueID == standard.UniqueID).Select(x => new FeelOptionModel
                                        {
                                            UniqueID = x.UniqueID,
                                            Description = x.Description,
                                            IsAbnormal = x.IsAbnormal,
                                            Seq = x.Seq
                                        }).ToList(),
                                        AbnormalReasonList = (from sa in db.StandardAbnormalReason
                                                              join a in db.AbnormalReason
                                                              on sa.AbnormalReasonUniqueID equals a.UniqueID
                                                              where sa.StandardUniqueID == standard.UniqueID
                                                              select new AbnormalReasonModel
                                                              {
                                                                  UniqueID = a.UniqueID,
                                                                  ID = a.ID,
                                                                  Description = a.Description,
                                                                  HandlingMethodList = (from ah in db.AbnormalReasonHandlingMethod
                                                                                        join h in db.HandlingMethod
                                                                                        on ah.HandlingMethodUniqueID equals h.UniqueID
                                                                                        where ah.AbnormalReasonUniqueID == a.UniqueID
                                                                                        select new HandlingMethodModel
                                                                                        {
                                                                                            UniqueID = h.UniqueID,
                                                                                            ID = h.ID,
                                                                                            Description = h.Description
                                                                                        }).ToList()
                                                              }).ToList()
                                    });

                                    seq++;
                                }

                                var materialList = (from x in db.MJobEquipmentMaterial
                                                    join m in db.Material
                                                    on x.MaterialUniqueID equals m.UniqueID
                                                    where x.MJobUniqueID == form.Job.UniqueID
                                                    select new
                                                    {
                                                        UniqueID = m.UniqueID,
                                                        m.ID,
                                                        Name = m.Name,
                                                        x.Quantity
                                                    }).OrderBy(x => x.ID).ToList();

                                foreach (var material in materialList)
                                {
                                    mFormModel.MaterialList.Add(new MFormMaterialModel()
                                    {
                                        UniqueID = material.UniqueID,
                                        ID = material.ID,
                                        Name = material.Name,
                                        Quantity = material.Quantity.Value
                                    });
                                }

                                DataModel.MaintenanceFormList.Add(mFormModel);
                            }
                        }
                        #endregion

                        #region RepairForm
                        else if (!string.IsNullOrEmpty(parameter.RepairFormUniqueID))
                        {
                            //#region Equipment Maintenance

                            var repairForm = (from x in db.RForm
                                              join t in db.RFormType
                                              on x.RFormTypeUniqueID equals t.UniqueID into tmpType
                                              from t in tmpType.DefaultIfEmpty()
                                              join e in db.Equipment
                                              on x.EquipmentUniqueID equals e.UniqueID into tmpEquipment
                                              from e in tmpEquipment.DefaultIfEmpty()
                                              join p in db.EquipmentPart
                                              on x.PartUniqueID equals p.UniqueID into tmpPart
                                              from p in tmpPart.DefaultIfEmpty()
                                              where x.UniqueID == parameter.RepairFormUniqueID
                                              select new
                                              {
                                                  x.UniqueID,
                                                  x.Subject,
                                                  x.Description,
                                                  x.VHNO,
                                                  x.EstBeginDate,
                                                  x.EstEndDate,
                                                  RepairFormType = t != null ? t.Description : "",
                                                  EquipmentID = e != null ? e.ID : "",
                                                  EquipmentName = e != null ? e.Name : "",
                                                  PartDescription = p != null ? p.Description : ""
                                              }).First();

                            var repairFormModel = new RepairFormModel()
                            {
                                UniqueID = repairForm.UniqueID,
                                Subject = repairForm.Subject,
                                Description = repairForm.Description,
                                VHNO = repairForm.VHNO,
                                RepairFormType = repairForm.RepairFormType,
                                EquipmentID = repairForm.EquipmentID,
                                EquipmentName = repairForm.EquipmentName,
                                PartDescription = repairForm.PartDescription,
                                BeginDate = repairForm.EstBeginDate,
                                EndDate = repairForm.EstEndDate,
                                UserList = db.RFormJobUser.Where(x => x.RFormUniqueID == repairForm.UniqueID).Select(x => new UserModel
                                {
                                    ID = x.UserID
                                }).ToList()
                            };

                            DataModel.RepairFormList.Add(repairFormModel);

                            //#endregion
                        }
                        #endregion
                    }
                }

                using (DbEntities db = new DbEntities())
                {
                    foreach (var repairForm in DataModel.RepairFormList)
                    {
                        repairForm.UserList = (from x in repairForm.UserList
                                               join u in db.User
                                               on x.ID equals u.ID
                                               select new UserModel
                                               {
                                                   ID = u.ID,
                                                   Name = u.Name,
                                                   Title = u.Title,
                                                   Password = u.Password,
                                                   UID = u.UID
                                               }).ToList();
                    }

                    foreach (var maintenanceForm in DataModel.MaintenanceFormList)
                    {
                        maintenanceForm.UserList = (from x in maintenanceForm.UserList
                                                    join u in db.User
                                                    on x.ID equals u.ID
                                                    select new UserModel
                                                    {
                                                        ID = u.ID,
                                                        Name = u.Name,
                                                        Title = u.Title,
                                                        Password = u.Password,
                                                        UID = u.UID
                                                    }).ToList();
                    }

                    foreach (var job in DataModel.JobList)
                    {
                        job.UserList = (from x in job.UserList
                                        join u in db.User
                                        on x.ID equals u.ID
                                        select new UserModel
                                        {
                                            ID = u.ID,
                                            Name = u.Name,
                                            Title = u.Title,
                                            Password = u.Password,
                                            UID = u.UID
                                        }).ToList();

                        #region EmgContactList
                        //var upStreamOrganizationList = OrganizationDataAccessor.GetUpStreamOrganizationList(job.OrganizationUniqueID, true);

                        //job.EmgContactList = (from e in db.EMGCONTACT
                        //                      join u in db.User
                        //                      on e.UserID equals u.ID into tmpUser
                        //                      from u in tmpUser.DefaultIfEmpty()
                        //                      where upStreamOrganizationList.Contains(e.OrganizationUniqueID)
                        //                      select new EmgContactModel
                        //                      {
                        //                          UniqueID = e.UniqueID,
                        //                          Title = u != null ? u.Title : e.Title,
                        //                          Name = u != null ? u.Name : e.Name,
                        //                          TelList = db.EMGCONTACTTEL.Where(x => x.EMGCONTACTUniqueID == e.UniqueID).Select(x => new EmgContactTelModel
                        //                          {
                        //                              Seq = x.Seq,
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
#endif

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
                            cmd.CommandText = "INSERT INTO AbnormalReason (UniqueID, ID, Description) ValueS (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", Define.OTHER);
                            cmd.Parameters.AddWithValue("ID", Define.OTHER);
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "INSERT INTO AbnormalReasonHandlingMethod (AbnormalReasonUniqueID, HandlingMethodUniqueID) ValueS (@AbnormalReasonUniqueID, @HandlingMethodUniqueID)";

                            cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", Define.OTHER);
                            cmd.Parameters.AddWithValue("HandlingMethodUniqueID", Define.OTHER);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var abnormalReason in DataModel.AbnormalReasonList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO AbnormalReason (UniqueID, ID, Description) ValueS (@UniqueID, @ID, @Description)";

                                cmd.Parameters.AddWithValue("UniqueID", abnormalReason.UniqueID);
                                cmd.Parameters.AddWithValue("ID", abnormalReason.ID);
                                cmd.Parameters.AddWithValue("Description", abnormalReason.Description);

                                cmd.ExecuteNonQuery();
                            }

                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO AbnormalReasonHandlingMethod (AbnormalReasonUniqueID, HandlingMethodUniqueID) ValueS (@AbnormalReasonUniqueID, @HandlingMethodUniqueID)";

                                cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", abnormalReason.UniqueID);
                                cmd.Parameters.AddWithValue("HandlingMethodUniqueID", Define.OTHER);

                                cmd.ExecuteNonQuery();
                            }

                            #region AbnormalReasonHandlingMethod
                            foreach (var handlingMethod in abnormalReason.HandlingMethodList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO AbnormalReasonHandlingMethod (AbnormalReasonUniqueID, HandlingMethodUniqueID) ValueS (@AbnormalReasonUniqueID, @HandlingMethodUniqueID)";

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
                                cmd.CommandText = "INSERT INTO CheckItemAbnormalReason (CheckItemUniqueID, AbnormalReasonUniqueID) ValueS (@CheckItemUniqueID, @AbnormalReasonUniqueID)";

                                cmd.Parameters.AddWithValue("CheckItemUniqueID", checkItem.UniqueID);
                                cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", Define.OTHER);

                                cmd.ExecuteNonQuery();
                            }

                            #region CheckItemAbnormalReason
                            foreach (var abnormalReason in checkItem.AbnormalReasonList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO CheckItemAbnormalReason (CheckItemUniqueID, AbnormalReasonUniqueID) ValueS (@CheckItemUniqueID, @AbnormalReasonUniqueID)";

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
                                    cmd.CommandText = "INSERT INTO CheckItemFeelOption (UniqueID, CheckItemUniqueID, Description, IsAbnormal, Seq) ValueS (@UniqueID, @CheckItemUniqueID, @Description, @IsAbnormal, @Seq)";

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
                            cmd.CommandText = "INSERT INTO HandlingMethod (UniqueID, ID, Description) ValueS (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", Define.OTHER);
                            cmd.Parameters.AddWithValue("ID", Define.OTHER);
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var handlingMethod in DataModel.HandlingMethodList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO HandlingMethod (UniqueID, ID, Description) ValueS (@UniqueID, @ID, @Description)";

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
                                    cmd.CommandText = "INSERT INTO EmgContact (JobUniqueID, UniqueID, Title, Name) ValueS (@JobUniqueID, @UniqueID, @Title, @Name)";

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
                                cmd.CommandText = "INSERT INTO Job (UniqueID, Description, Remark, TimeMode, BeginTime, EndTime, IsCheckBySeq, IsShowPrevRecord) ValueS (@UniqueID, @Description, @Remark, @TimeMode, @BeginTime, @EndTime, @IsCheckBySeq, @IsShowPrevRecord)";

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
                                    cmd.CommandText = "INSERT INTO ControlPoint (JobUniqueID, ControlPointUniqueID, ID, Description, IsFeelItemDefaultNormal, TagID, MinTimeSpan, Remark, Seq) ValueS (@JobUniqueID, @ControlPointUniqueID, @ID, @Description, @IsFeelItemDefaultNormal, @TagID, @MinTimeSpan, @Remark, @Seq)";

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
                                            cmd.CommandText = "INSERT INTO Equipment (JobUniqueID, ControlPointUniqueID, EquipmentUniqueID, PartUniqueID, ID, Name, PartDescription, IsFeelItemDefaultNormal, Seq) ValueS (@JobUniqueID, @ControlPointUniqueID, @EquipmentUniqueID, @PartUniqueID, @ID, @Name, @PartDescription, @IsFeelItemDefaultNormal, @Seq)";

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
                                                cmd.CommandText = "INSERT INTO CheckItem (JobUniqueID, ControlPointUniqueID, EquipmentUniqueID, PartUniqueID, CheckItemUniqueID, ID, Description, IsFeelItem, TextValueType, LowerLimit, LowerAlertLimit, UpperAlertLimit, UpperLimit, Unit, Remark, Seq) ValueS (@JobUniqueID, @ControlPointUniqueID, @EquipmentUniqueID, @PartUniqueID, @CheckItemUniqueID, @ID, @Description, @IsFeelItem, @TextValueType, @LowerLimit, @LowerAlertLimit, @UpperAlertLimit, @UpperLimit, @Unit, @Remark, @Seq)";

                                                cmd.Parameters.AddWithValue("JobUniqueID", job.UniqueID);
                                                cmd.Parameters.AddWithValue("ControlPointUniqueID", controlPoint.UniqueID);
                                                cmd.Parameters.AddWithValue("EquipmentUniqueID", equipment.UniqueID);
                                                cmd.Parameters.AddWithValue("PartUniqueID", part.UniqueID);
                                                cmd.Parameters.AddWithValue("CheckItemUniqueID", checkItem.UniqueID);
                                                cmd.Parameters.AddWithValue("ID", checkItem.ID);
                                                cmd.Parameters.AddWithValue("Description", checkItem.Description);
                                                cmd.Parameters.AddWithValue("IsFeelItem", checkItem.IsFeelItem ? "Y" : "N");
                                                cmd.Parameters.AddWithValue("TextValueType", checkItem.TextValueType);
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
                                                cmd.CommandText = "INSERT INTO EquipmentFile (UniqueID, EquipmentUniqueID, PartUniqueID, FileName, Extension) ValueS (@UniqueID, @EquipmentUniqueID, @PartUniqueID, @FileName, @Extension)";

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
                                        cmd.CommandText = "INSERT INTO CheckItem (JobUniqueID, ControlPointUniqueID, EquipmentUniqueID, PartUniqueID, CheckItemUniqueID, ID, Description, IsFeelItem, TextValueType, LowerLimit, LowerAlertLimit, UpperAlertLimit, UpperLimit, Unit, Remark, Seq) ValueS (@JobUniqueID, @ControlPointUniqueID, @EquipmentUniqueID, @PartUniqueID, @CheckItemUniqueID, @ID, @Description, @IsFeelItem, @TextValueType, @LowerLimit, @LowerAlertLimit, @UpperAlertLimit, @UpperLimit, @Unit, @Remark, @Seq)";

                                        cmd.Parameters.AddWithValue("JobUniqueID", job.UniqueID);
                                        cmd.Parameters.AddWithValue("ControlPointUniqueID", controlPoint.UniqueID);
                                        cmd.Parameters.AddWithValue("EquipmentUniqueID", "");
                                        cmd.Parameters.AddWithValue("PartUniqueID", "");
                                        cmd.Parameters.AddWithValue("CheckItemUniqueID", checkItem.UniqueID);
                                        cmd.Parameters.AddWithValue("ID", checkItem.ID);
                                        cmd.Parameters.AddWithValue("Description", checkItem.Description);
                                        cmd.Parameters.AddWithValue("IsFeelItem", checkItem.IsFeelItem ? "Y" : "N");
                                        cmd.Parameters.AddWithValue("TextValueType", checkItem.TextValueType);
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
                                cmd.CommandText = "INSERT INTO LastModifyTime (JobUniqueID, VersionTime) ValueS (@JobUniqueID, @VersionTime)";

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
                                cmd.CommandText = "INSERT INTO EquipmentMaterial (EquipmentUniqueID, PartUniqueID, MaterialUniqueID, MaterialID, MaterialName, QTY) ValueS (@EquipmentUniqueID, @PartUniqueID, @MaterialUniqueID, @MaterialID, @MaterialName, @QTY)";

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
                                cmd.CommandText = "INSERT INTO EquipmentSpec (EquipmentUniqueID, Spec, Value) ValueS (@EquipmentUniqueID, @Spec, @Value)";

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
                                cmd.CommandText = "INSERT INTO MaterialSpec (MaterialUniqueID, Spec, Value) ValueS (@MaterialUniqueID, @Spec, @Value)";

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
                            cmd.CommandText = "INSERT INTO OverTimeReason (UniqueID, ID, Description) ValueS (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", Define.OTHER);
                            cmd.Parameters.AddWithValue("ID", Define.OTHER);
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var overTimeReason in DataModel.OverTimeReasonList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO OverTimeReason (UniqueID, ID, Description) ValueS (@UniqueID, @ID, @Description)";

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
                            cmd.CommandText = "INSERT INTO TimeSpanAbnormalReason (UniqueID, ID, Description) ValueS (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", Define.OTHER);
                            cmd.Parameters.AddWithValue("ID", Define.OTHER);
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var timeSpanAbnormalReason in DataModel.TimeSpanAbnormalReasonList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO TimeSpanAbnormalReason (UniqueID, ID, Description) ValueS (@UniqueID, @ID, @Description)";

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
                            cmd.CommandText = "INSERT INTO UnPatrolReason (UniqueID, ID, Description) ValueS (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", Define.OTHER);
                            cmd.Parameters.AddWithValue("ID", Define.OTHER);
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var unPatrolReason in DataModel.UnPatrolReasonList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO UnPatrolReason (UniqueID, ID, Description) ValueS (@UniqueID, @ID, @Description)";

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
                                    cmd.CommandText = "INSERT INTO EmgContactTel (EmgContactUniqueID, Seq, Tel) ValueS (@EmgContactUniqueID, @Seq, @Tel)";

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
                            cmd.CommandText = "INSERT INTO UnRFIDReason (UniqueID, ID, Description) ValueS (@UniqueID, @ID, @Description)";

                            cmd.Parameters.AddWithValue("UniqueID", Define.OTHER);
                            cmd.Parameters.AddWithValue("ID", Define.OTHER);
                            cmd.Parameters.AddWithValue("Description", Resources.Resource.Other);

                            cmd.ExecuteNonQuery();
                        }

                        foreach (var unRFIDReason in DataModel.UnRFIDReasonList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO UnRFIDReason (UniqueID, ID, Description) ValueS (@UniqueID, @ID, @Description)";

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
                                cmd.CommandText = "INSERT INTO [User] (ID, Title, Name, Password, UID) ValueS (@ID, @Title, @Name, @Password, @UID)";

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
                                cmd.CommandText = "INSERT INTO PrevCheckResult (ControlPointUniqueID, EquipmentUniqueID, PartUniqueID, CheckItemUniqueID, LowerLimit, LowerAlertLimit, UpperAlertLimit, UpperLimit, Unit, CheckDate, CheckTime, Result, IsAbnormal, AbnormalReason) ValueS (@ControlPointUniqueID, @EquipmentUniqueID, @PartUniqueID, @CheckItemUniqueID, @LowerLimit, @LowerAlertLimit, @UpperAlertLimit, @UpperLimit, @Unit, @CheckDate, @CheckTime, @Result, @IsAbnormal, @AbnormalReason)";

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
                                cmd.CommandText = "INSERT INTO StandardAbnormalReason (StandardUniqueID, AbnormalReasonUniqueID) ValueS (@StandardUniqueID, @AbnormalReasonUniqueID)";

                                cmd.Parameters.AddWithValue("StandardUniqueID", standard.UniqueID);
                                cmd.Parameters.AddWithValue("AbnormalReasonUniqueID", Define.OTHER);

                                cmd.ExecuteNonQuery();
                            }

                            #region StandardAbnormalReason
                            foreach (var abnormalReason in standard.AbnormalReasonList)
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO StandardUniqueIDAbnormalReason (StandardUniqueID, AbnormalReasonUniqueID) ValueS (@StandardUniqueID, @AbnormalReasonUniqueID)";

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
                                    cmd.CommandText = "INSERT INTO StandardFeelOption (UniqueID, StandardUniqueID, Description, IsAbnormal, Seq) ValueS (@UniqueID, @StandardUniqueID, @Description, @IsAbnormal, @Seq)";

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

                        #region MaintenanceForm
                        foreach (var maintenanceForm in DataModel.MaintenanceFormList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO MForm (UniqueID, VHNO, Description, Remark, EquipmentID, EquipmentName, PartDescription, BeginDate, EndDate) ValueS (@UniqueID, @VHNO, @Description, @Remark, @EquipmentID, @EquipmentName, @PartDescription, @BeginDate, @EndDate)";

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
                                    cmd.CommandText = "INSERT INTO Standard (MFormUniqueID, StandardUniqueID, ID, Description, IsFeelItem, LowerLimit, LowerAlertLimit, UpperAlertLimit, UpperLimit, Unit, Remark, Seq) ValueS (@MFormUniqueID, @StandardUniqueID,  @ID, @Description, @IsFeelItem, @LowerLimit, @LowerAlertLimit, @UpperAlertLimit, @UpperLimit, @Unit, @Remark, @Seq)";

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
                                    cmd.CommandText = "INSERT INTO MFormMaterial (MFormUniqueID, MaterialUniqueID, MaterialID, MaterialName, Quantity) ValueS (@MFormUniqueID, @MaterialUniqueID, @MaterialID, @MaterialName, @Quantity)";

                                    cmd.Parameters.AddWithValue("MFormUniqueID", maintenanceForm.UniqueID);
                                    cmd.Parameters.AddWithValue("MaterialUniqueID", material.UniqueID);
                                    cmd.Parameters.AddWithValue("MaterialID", material.ID);
                                    cmd.Parameters.AddWithValue("MaterialName", material.Name);
                                    cmd.Parameters.AddWithValue("Quantity", material.Quantity);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                        #endregion

                        #region RepairForm
                        foreach (var repairForm in DataModel.RepairFormList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "INSERT INTO RepairForm (UniqueID, VHNO, Equipment, Subject, Description, RepairFormType, BeginDate, EndDate) ValueS (@UniqueID, @VHNO, @Equipment, @Subject, @Description, @RepairFormType, @BeginDate, @EndDate)";

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
