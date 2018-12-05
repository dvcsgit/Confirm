using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Utility;
using Utility.Models;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.EquipmentMaintenance.DataSync;
using DataAccess.EquipmentMaintenance;

namespace DataSync.EquipmentMaintenance
{
    public class SyncHelper
    {
        public static RequestResult GetJobList(string UserID, string CheckDate)
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = new List<JobItem>();

                using (EDbEntities db = new EDbEntities())
                {
                    #region Patrol
                    if (Config.Modules.Contains(Define.EnumSystemModule.EquipmentPatrol))
                    {
                        var checkDate = DateTimeHelper.DateString2DateTime(CheckDate).Value;

                        var jobList = (from x in db.JobUser
                                       join j in db.Job
                                       on x.JobUniqueID equals j.UniqueID
                                       join r in db.Route
                                       on j.RouteUniqueID equals r.UniqueID
                                       where x.UserID == UserID
                                       select new { Job = j, Route = r }).ToList();

                        foreach (var job in jobList)
                        {
                            if (JobCycleHelper.IsInCycle(checkDate, job.Job.BeginDate, job.Job.EndDate, job.Job.CycleCount, job.Job.CycleMode))
                            {
                                DateTime beginDate, endDate;

                                JobCycleHelper.GetDateSpan(checkDate, job.Job.BeginDate, job.Job.EndDate, job.Job.CycleCount, job.Job.CycleMode, out beginDate, out endDate);

                                var beginDateString = DateTimeHelper.DateTime2DateString(beginDate);
                                var endDateString = DateTimeHelper.DateTime2DateString(endDate);

                                if (!string.IsNullOrEmpty(job.Job.BeginTime) && !string.IsNullOrEmpty(job.Job.EndTime) && string.Compare(job.Job.BeginTime, job.Job.EndTime) > 0)
                                {
                                    endDateString = DateTimeHelper.DateTime2DateString(endDate.AddDays(1));
                                }

                                var jobResult = db.JobResult.FirstOrDefault(x => x.JobUniqueID == job.Job.UniqueID && x.BeginDate == beginDateString && x.EndDate == endDateString);

                                //if (jobResult == null)
                                //{
                                //    JobResultDataAccessor.Refresh(Guid.NewGuid().ToString(), job.Job.UniqueID, beginDateString, endDateString);

                                //    jobResult = db.JobResult.FirstOrDefault(x => x.JobUniqueID == job.Job.UniqueID && x.BeginDate == beginDateString && x.EndDate == endDateString);
                                //}

                                //var beginDateTime = beginDate;
                                //var endDateTime = endDate;

                                //if (!string.IsNullOrEmpty(job.Job.BeginTime))
                                //{
                                //    beginDateTime = DateTimeHelper.DateTimeString2DateTime(beginDateString, job.Job.BeginTime).Value;
                                //}

                                //if (!string.IsNullOrEmpty(job.Job.EndTime))
                                //{
                                //    endDateTime = DateTimeHelper.DateTimeString2DateTime(endDateString, job.Job.EndTime).Value;
                                //}

                                //if (DateTime.Compare(beginDateTime, endDateTime) >= 0)
                                //{
                                //    endDateTime = endDateTime.AddDays(1);
                                //}

                                //endDateString = DateTimeHelper.DateTime2DateString(endDateTime);

                                //var checkResultQuery = db.CheckResult.Where(x => x.JobUniqueID == job.Job.UniqueID && string.Compare(x.CheckDate, beginDateString) >= 0 && string.Compare(x.CheckDate, endDateString) < 0);

                                //if (!string.IsNullOrEmpty(job.Job.BeginTime))
                                //{
                                //    var beginTimeString = DateTimeHelper.DateTime2TimeString(beginDateTime);

                                //    checkResultQuery = checkResultQuery.Where(x => string.Compare(x.CheckTime, beginTimeString) >= 0);
                                //}

                                //if (!string.IsNullOrEmpty(job.Job.EndTime))
                                //{
                                //    var endTimeString = DateTimeHelper.DateTime2TimeString(endDateTime);

                                //    checkResultQuery = checkResultQuery.Where(x => string.Compare(x.CheckTime, endTimeString) < 0);
                                //}

                                //var checkResultList = checkResultQuery.Select(x => new
                                //{
                                //    JobUniqueID = x.JobUniqueID,
                                //    ControlPointUniqueID = x.ControlPointUniqueID,
                                //    EquipmentUniqueID = x.EquipmentUniqueID,
                                //    PartUniqueID = x.PartUniqueID,
                                //    CheckItemUniqueID = x.CheckItemUniqueID
                                //}).Distinct().ToList();

                                //var controlPointCheckItemList = db.JobControlPointCheckItem.Where(x => x.JobUniqueID == job.Job.UniqueID).ToList();

                                //var equipmentCheckItemList = db.JobEquipmentCheckItem.Where(x => x.JobUniqueID == job.Job.UniqueID).ToList();

                                //var query1 = (from checkItem in controlPointCheckItemList
                                //              join checkResult in checkResultList
                                //              on new { JobUniqueID = checkItem.JobUniqueID, ControlPointUniqueID = checkItem.ControlPointUniqueID, EquipmentUniqueID = "", PartUniqueID = "", CheckItemUniqueID = checkItem.CheckItemUniqueID } equals new { checkResult.JobUniqueID, checkResult.ControlPointUniqueID, checkResult.EquipmentUniqueID, checkResult.PartUniqueID, checkResult.CheckItemUniqueID } into tmpCheckResult
                                //              from checkResult in tmpCheckResult.DefaultIfEmpty()
                                //              where checkItem.JobUniqueID == job.Job.UniqueID
                                //              select new
                                //              {
                                //                  CheckItem = checkItem,
                                //                  IsChecked = checkResult != null
                                //              }).ToList();

                                //var query2 = (from checkItem in equipmentCheckItemList
                                //              join checkResult in checkResultList
                                //              on new { JobUniqueID = checkItem.JobUniqueID, ControlPointUniqueID = checkItem.ControlPointUniqueID, EquipmentUniqueID = checkItem.EquipmentUniqueID, PartUniqueID = checkItem.PartUniqueID, CheckItemUniqueID = checkItem.CheckItemUniqueID } equals new { checkResult.JobUniqueID, checkResult.ControlPointUniqueID, checkResult.EquipmentUniqueID, checkResult.PartUniqueID, checkResult.CheckItemUniqueID } into tmpCheckResult
                                //              from checkResult in tmpCheckResult.DefaultIfEmpty()
                                //              where checkItem.JobUniqueID == job.Job.UniqueID
                                //              select new
                                //              {
                                //                  CheckItem = checkItem,
                                //                  IsChecked = checkResult != null
                                //              }).ToList();

                                itemList.Add(new JobItem()
                                {
                                    JobUniqueID = job.Job.UniqueID,
                                    JobDescription = job.Job.Description,
                                    RouteID = job.Route.ID,
                                    RouteName = job.Route.Name,
                                    BeginDate = beginDate,
                                    BeginTime = job.Job.BeginTime,
                                    EndDate = endDate,
                                    EndTime = job.Job.EndTime,
                                    CompleteRate = jobResult != null ? jobResult.CompleteRate : "-",
                                    CheckedItemCount = 0,
                                    CheckItemCount = 0
                                    //CheckItemCount = query1.Count + query2.Count,
                                    //CheckedItemCount = query1.Count(x => x.IsChecked) + query2.Count(x => x.IsChecked)
                                });
                            }
                        }
                    }
                    #endregion

                    #region Maintenance
                    if (Config.Modules.Contains(Define.EnumSystemModule.EquipmentMaintenance))
                    {
                        var mFormList = (from f in db.MForm
                                         join x in db.MJobUser
                                         on f.MJobUniqueID equals x.MJobUniqueID
                                         where (f.Status == "0" || f.Status == "1" || f.Status == "4" || f.Status == "6") && x.UserID == UserID
                                         select f.UniqueID).ToList();

                        foreach (var formUniqueID in mFormList)
                        {
                            var query = (from f in db.MForm
                                         join j in db.MJob
                                         on f.MJobUniqueID equals j.UniqueID
                                         join e in db.Equipment
                                         on f.EquipmentUniqueID equals e.UniqueID
                                         join p in db.EquipmentPart
                                         on f.PartUniqueID equals p.UniqueID into tmpPart
                                         from p in tmpPart.DefaultIfEmpty()
                                         where f.UniqueID == formUniqueID
                                         select new
                                         {
                                             UniqueID = f.UniqueID,
                                             f.VHNO,
                                             f.EstBeginDate,
                                             f.EstEndDate,
                                             EquipmentID = e.ID,
                                             EquipmentName = e.Name,
                                             PartUniqueID = f.PartUniqueID,
                                             PartDescription = p != null ? p.Description : "",
                                             Description = j.Description
                                         }).First();

                            itemList.Add(new JobItem()
                            {
                                MaintanenceFormUniqueID = query.UniqueID,
                                VHNO = query.VHNO,
                                FormType = Resources.Resource.MaintenanceForm,
                                Subject = query.Description,
                                BeginDate = query.EstBeginDate,
                                EndDate = query.EstEndDate,
                                EquipmentID = query.EquipmentID,
                                EquipmentName = query.EquipmentName,
                                PartDescription = query.PartDescription
                            });
                        }

                        #region RepairForm
                        var repairFormList = (from f in db.RForm
                                              join e in db.Equipment
                                              on f.EquipmentUniqueID equals e.UniqueID into tmpEquipment
                                              from e in tmpEquipment.DefaultIfEmpty()
                                              join p in db.EquipmentPart
                                              on f.PartUniqueID equals p.UniqueID into tmpPart
                                              from p in tmpPart.DefaultIfEmpty()
                                              join x in db.RFormJobUser
                                              on f.UniqueID equals x.RFormUniqueID
                                              join t in db.RFormType
                                              on f.RFormTypeUniqueID equals t.UniqueID
                                              where (f.Status == "2" || f.Status == "4" || f.Status == "7" || f.Status == "9") && x.UserID == UserID
                                              select new
                                              {
                                                  UniqueID = f.UniqueID,
                                                  RFormType = t.Description,
                                                  EstBeginDate = f.EstBeginDate,
                                                  EstEndDate = f.EstEndDate,
                                                  VHNO = f.VHNO,
                                                  Subject = f.Subject,
                                                  EquipmentUniqueID = f.EquipmentUniqueID,
                                                  EquipmentID = e != null ? e.ID : "",
                                                  EquipmentName = e != null ? e.Name : "",
                                                  PartUniqueID = f.PartUniqueID,
                                                  PartDescription = p != null ? p.Description : ""
                                              }).Distinct().ToList();

                        foreach (var repairForm in repairFormList)
                        {
                            itemList.Add(new JobItem()
                            {
                                RepairFormUniqueID = repairForm.UniqueID,
                                Subject = repairForm.Subject,
                                VHNO = repairForm.VHNO,
                                FormType = repairForm.RFormType,
                                BeginDate = repairForm.EstBeginDate,
                                EndDate = repairForm.EstEndDate,
                                EquipmentID = repairForm.EquipmentID,
                                EquipmentName = repairForm.EquipmentName,
                                PartDescription = repairForm.PartDescription
                            });
                        }
                        #endregion
                    }
                    #endregion
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
    }
}
