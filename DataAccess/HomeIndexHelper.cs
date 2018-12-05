using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.Authenticated;
using Models.Home;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace DataAccess
{
    public class HomeIndexHelper
    {
        public static RequestResult GetVerifyList(List<Models.Shared.UserModel> AccountList, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                result.ReturnData(new VerifyViewModel()
                {
                    EquipmentPatrolVerifyItemList = GetEquipmentPatrolVerifyItemList(Account),
                    MaintenanceFormVerifyItemList = GetMaintenanceFormVerifyItemList(AccountList, Account),
                    MaintenanceFormExtendVerifyItemList = GetMaintenanceFormExtendVerifyItemList(AccountList, Account),
                    RepairFormVerifyItemList = GetRepairFormVerifyItemList(Account),
                    RepairFormExtendVerifyItemList = GetRepairFormExtendVerifyItemList(Account)
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

        public static List<EquipmentPatrolVerifyItem> GetEquipmentPatrolVerifyItemList(Account Account)
        {
            var itemList = new List<EquipmentPatrolVerifyItem>();

            var baseTime = DateTime.Now.AddHours(-2);

            using (EDbEntities db = new EDbEntities())
            {
                var query = (from x in db.JobResult
                             join y in db.JobResultFlow
                             on x.UniqueID equals y.JobResultUniqueID into tmpFlow
                             from y in tmpFlow.DefaultIfEmpty()
                             join j in db.Job
                             on x.JobUniqueID equals j.UniqueID
                             join r in db.Route
                             on j.RouteUniqueID equals r.UniqueID
                             where x.IsNeedVerify  && (x.IsCompleted  || DateTime.Compare(baseTime, x.JobEndTime) > 0) && Account.QueryableOrganizationUniqueIDList.Contains(r.OrganizationUniqueID)
                             select new
                             {
                                 IsClosed = y != null ? y.IsClosed  : false,
                                 x.UniqueID,
                                 OrganizationDescription = x.OrganizationDescription,
                                 JobUniqueID = x.JobUniqueID,
                                 RouteUniqueID = r.UniqueID,
                                 BeginDate = x.BeginDate,
                                 EndDate = x.EndDate,
                                 BeginTime = x.BeginTime,
                                 EndTime = x.EndTime,
                                 x.Description,
                                 HaveAbnormal = x.HaveAbnormal ,
                                 HaveAlert = x.HaveAlert,
                                 TimeSpan = x.TimeSpan,
                                 CompleteRate = x.CompleteRate,
                                 CompleteRateLabelClass = x.CompleteRateLabelClass,
                                 ArriveStatus = x.ArriveStatus,
                                 ArriveStatusLabelClass = x.ArriveStatusLabelClass,
                                 CheckUsers = x.CheckUsers
                             }).AsQueryable();

                itemList = query.Select(x => new EquipmentPatrolVerifyItem
                {
                    IsClosed = x.IsClosed,
                    UniqueID = x.UniqueID,
                    BeginDate = x.BeginDate,
                    EndDate = x.EndDate,
                    BeginTime = x.BeginTime,
                    EndTime = x.EndTime,
                    ArriveStatus = x.ArriveStatus,
                    ArriveStatusLabelClass = x.ArriveStatusLabelClass,
                    CheckUsers = x.CheckUsers,
                    OrganizationDescription = x.OrganizationDescription,
                    CompleteRate = x.CompleteRate,
                    CompleteRateLabelClass = x.CompleteRateLabelClass,
                    Description = x.Description,
                    HaveAbnormal = x.HaveAbnormal,
                    HaveAlert = x.HaveAlert,
                    TimeSpan = x.TimeSpan
                }).OrderBy(x => x.BeginDate).ThenBy(x => x.BeginTime).ThenBy(x => x.Description).ToList();

                foreach (var item in itemList)
                {
                    if (!item.IsClosed)
                    {
                        var flow = db.JobResultFlow.FirstOrDefault(x => x.JobResultUniqueID == item.UniqueID);

                        if (flow != null)
                        {
                            var flowLog = db.JobResultFlowLog.FirstOrDefault(x => x.JobResultUniqueID == flow.JobResultUniqueID && x.Seq == flow.CurrentSeq);

                            if (flowLog != null)
                            {
                                item.CurrentVerifyUserID = flowLog.UserID;
                                item.CurrentVerifyUserName = flowLog.UserName;
                            }
                        }
                    }
                }

                itemList = itemList.Where(x => x.CurrentVerifyUserID == Account.ID && !x.IsClosed).ToList();
            }

            return itemList;
        }

        public static List<MaintenanceFormVerifyItem> GetMaintenanceFormVerifyItemList(List<Models.Shared.UserModel> AccountList, Account Account)
        {
            var itemList = new List<MaintenanceFormVerifyItem>();

            using (EDbEntities db = new EDbEntities())
            {
                var query = (from f in db.MForm
                             join j in db.MJob
                             on f.MJobUniqueID equals j.UniqueID
                             join e in db.Equipment
                             on f.EquipmentUniqueID equals e.UniqueID
                             join p in db.EquipmentPart
                             on f.PartUniqueID equals p.UniqueID into tmpPart
                             from p in tmpPart.DefaultIfEmpty()
                             join x in db.MFormFlowLog
                             on f.UniqueID equals x.MFormUniqueID
                             where !x.VerifyTime.HasValue && x.UserID == Account.ID
                             select new
                             {
                                 UniqueID = f.UniqueID,
                                 j.OrganizationUniqueID,
                                 f.VHNO,
                                 CycleBeginDate = f.CycleBeginDate,
                                 CycleEndDate = f.CycleEndDate,
                                 EstBeginDate = f.EstBeginDate,
                                 EstEndDate = f.EstEndDate,
                                 Subject = j.Description,
                                 EquipmentID = e.ID,
                                 EquipmentName = e.Name,
                                 PartUniqueID = f.PartUniqueID,
                                 PartDescription = p != null ? p.Description : "",
                                 BeginDate = f.BeginDate,
                                 EndDate = f.EndDate
                             }).Distinct().AsQueryable();

                foreach (var q in query)
                {
                    var item = new MaintenanceFormVerifyItem()
                    {
                        VHNO = q.VHNO,
                        OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(q.OrganizationUniqueID),
                        Subject = q.Subject,
                        CycleBeginDate = q.CycleBeginDate,
                        CycleEndDate = q.CycleEndDate,
                        EstBeginDate = q.EstBeginDate,
                        EstEndDate = q.EstEndDate,
                        EquipmentID = q.EquipmentID,
                        EquipmentName = q.EquipmentName,
                        PartDescription = q.PartDescription,
                        BeginDate = q.BeginDate,
                        EndDate = q.EndDate
                    };

                    var maintenanceUserList = db.MFormResult.Where(x => x.MFormUniqueID == q.UniqueID).Select(x => x.UserID).Distinct().ToList();

                    item.MaintenanceUserList = (from x in maintenanceUserList
                                                join y in AccountList
                                                on x equals y.ID
                                                select new Models.Shared.UserModel
                                                {
                                                    OrganizationDescription = y.OrganizationDescription,
                                                    ID = y.ID,
                                                    Name = y.Name
                                                }).ToList();

                    itemList.Add(item);
                }
            }

            return itemList;
        }

        public static List<MaintenanceFormVerifyItem> GetMaintenanceFormExtendVerifyItemList(List<Models.Shared.UserModel> AccountList, Account Account)
        {
            var itemList = new List<MaintenanceFormVerifyItem>();

            using (EDbEntities db = new EDbEntities())
            {
                var query = (from f in db.MForm
                             join j in db.MJob
                             on f.MJobUniqueID equals j.UniqueID
                             join e in db.Equipment
                             on f.EquipmentUniqueID equals e.UniqueID
                             join p in db.EquipmentPart
                             on f.PartUniqueID equals p.UniqueID into tmpPart
                             from p in tmpPart.DefaultIfEmpty()
                             join x in db.MFormExtendFlowLog
                             on f.UniqueID equals x.MFormUniqueID
                             where !x.VerifyTime.HasValue && x.UserID == Account.ID
                             select new
                             {
                                 UniqueID = f.UniqueID,
                                 j.OrganizationUniqueID,
                                 f.VHNO,
                                 CycleBeginDate = f.CycleBeginDate,
                                 CycleEndDate = f.CycleEndDate,
                                 EstBeginDate = f.EstBeginDate,
                                 EstEndDate = f.EstEndDate,
                                 Subject = j.Description,
                                 EquipmentID = e.ID,
                                 EquipmentName = e.Name,
                                 PartUniqueID = f.PartUniqueID,
                                 PartDescription = p != null ? p.Description : "",
                                 BeginDate = f.BeginDate,
                                 EndDate = f.EndDate
                             }).Distinct().AsQueryable();

                foreach (var q in query)
                {
                    var item = new MaintenanceFormVerifyItem()
                    {
                        VHNO = q.VHNO,
                        OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(q.OrganizationUniqueID),
                        Subject = q.Subject,
                        CycleBeginDate = q.CycleBeginDate,
                        CycleEndDate = q.CycleEndDate,
                        EstBeginDate = q.EstBeginDate,
                        EstEndDate = q.EstEndDate,
                        EquipmentID = q.EquipmentID,
                        EquipmentName = q.EquipmentName,
                        PartDescription = q.PartDescription,
                        BeginDate = q.BeginDate,
                        EndDate = q.EndDate
                    };

                    var maintenanceUserList = db.MFormResult.Where(x => x.MFormUniqueID == q.UniqueID).Select(x => x.UserID).Distinct().ToList();

                    item.MaintenanceUserList = (from x in maintenanceUserList
                                                join y in AccountList
                                                on x equals y.ID
                                                select new Models.Shared.UserModel
                                                {
                                                    OrganizationDescription = y.OrganizationDescription,
                                                    ID = y.ID,
                                                    Name = y.Name
                                                }).ToList();

                    itemList.Add(item);
                }
            }

            return itemList;
        }

        public static List<RepairFormVerifyItem> GetRepairFormVerifyItemList(Account Account)
        {
            var itemList = new List<RepairFormVerifyItem>();

            using (EDbEntities db = new EDbEntities())
            {
                var query = (from f in db.RForm
                             join t in db.RFormType
                             on f.RFormTypeUniqueID equals t.UniqueID
                             join x in db.RFormFlowLog
                            on f.UniqueID equals x.RFormUniqueID
                             where !x.VerifyTime.HasValue && x.UserID == Account.ID
                             select new
                             {
                                 f.OrganizationUniqueID,
                                 f.MaintenanceOrganizationUniqueID,
                                 f.VHNO,
                                 RepairFormType = t.Description,
                                 EstBeginDate = f.EstBeginDate,
                                 EstEndDate = f.EstEndDate,
                                 Subject = f.Subject,
                                 f.EquipmentID,
                                 f.EquipmentName,
                                 f.PartDescription,
                                 f.TakeJobUserID
                             }).Distinct().AsQueryable();

                itemList = query.Select(x => new RepairFormVerifyItem
                {
                    OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.OrganizationUniqueID),
                    MaintenanceOrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(x.MaintenanceOrganizationUniqueID),
                    VHNO = x.VHNO,
                    Subject = x.Subject,
                    EstBeginDate = x.EstBeginDate,
                    EstEndDate = x.EstEndDate,
                    EquipmentID = x.EquipmentID,
                    EquipmentName = x.EquipmentName,
                    PartDescription = x.PartDescription,
                    RepairFormType = x.RepairFormType,
                    TakeJobUserID = x.TakeJobUserID
                }).ToList();
            }

            return itemList;
        }

        public static List<RepairFormVerifyItem> GetRepairFormExtendVerifyItemList(Account Account)
        {
            return new List<RepairFormVerifyItem>();
        }

        public static RequestResult GetAbnormalHandlingList(Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new SummaryViewModel();

                using (EDbEntities db = new EDbEntities())
                {
                    var today = DateTimeHelper.DateTime2DateString(DateTime.Today);

                    //個人負責巡檢異常
                    var query1 = (from a in db.Abnormal
                                  join x in db.AbnormalCheckResult
                                  on a.UniqueID equals x.AbnormalUniqueID
                                  join c in db.CheckResult
                                  on x.CheckResultUniqueID equals c.UniqueID
                                  join y in db.RouteManager
                                  on c.RouteUniqueID equals y.RouteUniqueID
                                  where y.UserID == Account.ID
                                  select new
                                  {
                                      a.UniqueID,
                                      a.ClosedTime,
                                      c.CheckDate,
                                      c.IsAbnormal,
                                      c.IsAlert
                                  }).Distinct().ToList();

                    //個人負責保養異常
                    var query2 = (from a in db.Abnormal
                                  join x in db.AbnormalMFormStandardResult
                                  on a.UniqueID equals x.AbnormalUniqueID
                                  join r in db.MFormStandardResult
                                  on x.MFormStandardResultUniqueID equals r.UniqueID
                                  join y in db.MFormResult
                                  on r.ResultUniqueID equals y.UniqueID
                                  where y.UserID==Account.ID
                                  select new
                                  {
                                      a.UniqueID,
                                      a.ClosedTime,
                                      y.PMDate,
                                      r.IsAbnormal,
                                      r.IsAlert
                                  }).Distinct().ToList();

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-exclamation-circle",
                        Count = query1.Count(x => x.IsAbnormal && x.CheckDate == today) + query2.Count(x => x.IsAbnormal && x.PMDate == today),
                        Text = "今日異常項目總數",
                        Status = "1"
                    });

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-exclamation-circle",
                        Count = query1.Count(x => x.IsAlert && x.CheckDate == today) + query2.Count(x => x.IsAlert && x.PMDate == today),
                        Text = "今日注意項目總數",
                        Status = "2"
                    });

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-exclamation-circle",
                        Count = query1.Count(x => x.IsAbnormal && !x.ClosedTime.HasValue) + query2.Count(x => x.IsAbnormal && !x.ClosedTime.HasValue),
                        Text = "累積異常未結案",
                        Status = "3"
                    });

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-orange",
                        Icon = "fa-exclamation-circle",
                        Count = query1.Count(x => x.IsAlert && !x.ClosedTime.HasValue) + query2.Count(x => x.IsAlert && !x.ClosedTime.HasValue),
                        Text = "累積注意未結案",
                        Status = "4"
                    });

                    //組織巡檢異常
                    var query3 = (from a in db.Abnormal
                                  join x in db.AbnormalCheckResult
                                  on a.UniqueID equals x.AbnormalUniqueID
                                  join c in db.CheckResult
                                  on x.CheckResultUniqueID equals c.UniqueID
                                  join r in db.Route
                                  on c.RouteUniqueID equals r.UniqueID
                                  where Account.QueryableOrganizationUniqueIDList.Contains(r.OrganizationUniqueID)
                                  select new
                                  {
                                      a.UniqueID,
                                      a.ClosedTime,
                                      c.CheckDate,
                                      c.IsAbnormal,
                                      c.IsAlert
                                  }).Distinct().ToList();

                    //組織保養異常
                    var query4 = (from a in db.Abnormal
                                  join x in db.AbnormalMFormStandardResult
                                  on a.UniqueID equals x.AbnormalUniqueID
                                  join r in db.MFormStandardResult
                                  on x.MFormStandardResultUniqueID equals r.UniqueID
                                  join y in db.MFormResult
                                  on r.ResultUniqueID equals y.UniqueID
                                  where Account.QueryableOrganizationUniqueIDList.Contains(r.OrganizationUniqueID)
                                  select new
                                  {
                                      a.UniqueID,
                                      a.ClosedTime,
                                      y.PMDate,
                                      r.IsAbnormal,
                                      r.IsAlert
                                  }).Distinct().ToList();

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-exclamation-circle",
                        Count = query3.Count(x => x.IsAbnormal && x.CheckDate == today) + query4.Count(x => x.IsAbnormal && x.PMDate == today),
                        Text = "今日異常項目總數",
                        Status = "5"
                    });

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-exclamation-circle",
                        Count = query3.Count(x => x.IsAlert && x.CheckDate == today) + query4.Count(x => x.IsAlert && x.PMDate == today),
                        Text = "今日注意項目總數",
                        Status = "6"
                    });

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-exclamation-circle",
                        Count = query3.Count(x => x.IsAbnormal && !x.ClosedTime.HasValue) + query4.Count(x => x.IsAbnormal && !x.ClosedTime.HasValue),
                        Text = "累積異常未結案",
                        Status = "7"
                    });

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-orange",
                        Icon = "fa-exclamation-circle",
                        Count = query3.Count(x => x.IsAlert && !x.ClosedTime.HasValue) + query4.Count(x => x.IsAlert && !x.ClosedTime.HasValue),
                        Text = "累積注意未結案",
                        Status = "8"
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

        public static RequestResult GetRepairFormList(Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new SummaryViewModel();

                using (DbEntities db = new DbEntities())
                {
                    var managerOrganizationList = db.Organization.Where(x => x.ManagerUserID == Account.ID).Select(x => x.UniqueID).ToList();

                    using (EDbEntities edb = new EDbEntities())
                    {
                        model.PersonalItemList.Add(new SummaryItem()
                        {
                            BoxColor = "infobox-orange",
                            Icon = "fa-download",
                            Count = edb.RForm.Count(x => managerOrganizationList.Contains(x.MaintenanceOrganizationUniqueID) && (x.Status == "0" || x.Status == "3")),
                            Text = Resources.Resource.RFormStatus_0,
                            Status = "0"
                        });

                        model.PersonalItemList.Add(new SummaryItem()
                        {
                            BoxColor = "infobox-orange",
                            Icon = "fa-download",
                            Count = (from x in edb.RFormJobUser
                                     join f in edb.RForm
                                     on x.RFormUniqueID equals f.UniqueID
                                     where x.UserID == Account.ID && f.Status == "2"
                                     select f.UniqueID).Count(),
                            Text = Resources.Resource.RFormStatus_2,
                            Status = "2"
                        });

                        var query = (from f in edb.RForm
                                     where f.Status == "0" || f.Status == "2" || f.Status == "3" || f.Status == "4" || f.Status == "7" || f.Status == "9"
                                     select new
                                     {
                                         UniqueID = f.UniqueID,
                                         Status = f.Status,
                                         f.OrganizationUniqueID,
                                         f.MaintenanceOrganizationUniqueID,
                                         f.EstEndDate,
                                         f.TakeJobUserID
                                     }).AsQueryable();

                        var personalRFormList = query.Where(x => x.TakeJobUserID == Account.ID).ToList();

                        int repairingCount = personalRFormList.Count(x => x.Status == "4");

                        int delayCount = personalRFormList.Count(x => x.Status == "4" && x.EstEndDate.HasValue && DateTime.Compare(x.EstEndDate.Value, DateTime.Today) < 0);

                        model.PersonalItemList.Add(new SummaryItem()
                        {
                            BoxColor = "infobox-blue",
                            Icon = "fa-wrench",
                            Count = repairingCount - delayCount,
                            Text = Resources.Resource.RFormStatus_4,
                            Status = "4"
                        });

                        model.PersonalItemList.Add(new SummaryItem()
                        {
                            BoxColor = "infobox-red",
                            Icon = "fa-exclamation-circle",
                            Count = delayCount,
                            Text = Resources.Resource.RFormStatus_5,
                            Status = "5"
                        });

                        model.PersonalItemList.Add(new SummaryItem()
                        {
                            BoxColor = "infobox-red",
                            Icon = "fa-times-circle-o",
                            Count = personalRFormList.Count(x => x.Status == "7"),
                            Text = Resources.Resource.RFormStatus_7,
                            Status = "7"
                        });

                        model.PersonalItemList.Add(new SummaryItem()
                        {
                            BoxColor = "infobox-purple",
                            Icon = "fa-gavel",
                            Count = personalRFormList.Count(x => x.Status == "9"),
                            Text = Resources.Resource.RFormStatus_9,
                            Status = "9"
                        });

                        var queryableOrganizationRFormList = query.Where(x => Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID) || Account.QueryableOrganizationUniqueIDList.Contains(x.MaintenanceOrganizationUniqueID)).ToList();

                        model.QueryableOrganizationItemList.Add(new SummaryItem()
                        {
                            BoxColor = "infobox-orange",
                            Icon = "fa-download",
                            Count = queryableOrganizationRFormList.Count(x => x.Status == "0" || x.Status == "3"),
                            Text = Resources.Resource.RFormStatus_0,
                            Status = "0"
                        });

                        model.QueryableOrganizationItemList.Add(new SummaryItem()
                        {
                            BoxColor = "infobox-orange",
                            Icon = "fa-download",
                            Count = queryableOrganizationRFormList.Count(x => x.Status == "2"),
                            Text = Resources.Resource.RFormStatus_2,
                            Status = "2"
                        });

                        repairingCount = queryableOrganizationRFormList.Count(x => x.Status == "4");

                        delayCount = queryableOrganizationRFormList.Count(x => x.Status == "4" && x.EstEndDate.HasValue && DateTime.Compare(x.EstEndDate.Value, DateTime.Today) < 0);

                        model.QueryableOrganizationItemList.Add(new SummaryItem()
                        {
                            BoxColor = "infobox-blue",
                            Icon = "fa-wrench",
                            Count = repairingCount - delayCount,
                            Text = Resources.Resource.RFormStatus_4,
                            Status = "4"
                        });

                        model.QueryableOrganizationItemList.Add(new SummaryItem()
                        {
                            BoxColor = "infobox-red",
                            Icon = "fa-exclamation-circle",
                            Count = delayCount,
                            Text = Resources.Resource.RFormStatus_5,
                            Status = "5"
                        });

                        model.QueryableOrganizationItemList.Add(new SummaryItem()
                        {
                            BoxColor = "infobox-red",
                            Icon = "fa-times-circle-o",
                            Count = queryableOrganizationRFormList.Count(x => x.Status == "7"),
                            Text = Resources.Resource.RFormStatus_7,
                            Status = "7"
                        });

                        model.QueryableOrganizationItemList.Add(new SummaryItem()
                        {
                            BoxColor = "infobox-purple",
                            Icon = "fa-gavel",
                            Count = queryableOrganizationRFormList.Count(x => x.Status == "9"),
                            Text = Resources.Resource.RFormStatus_9,
                            Status = "9"
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

        public static RequestResult GetMaintenanceFormList(Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new SummaryViewModel();

                using (EDbEntities db = new EDbEntities())
                {
                    var personalMFormList = (from x in db.MForm
                                             join j in db.MJob
                                             on x.MJobUniqueID equals j.UniqueID
                                             join y in db.MJobUser
                                             on j.UniqueID equals y.MJobUniqueID
                                             where y.UserID == Account.ID && (x.Status == "0" || x.Status == "1" || x.Status == "4" || x.Status == "6")
                                             select new
                                             {
                                                 x.UniqueID,
                                                 x.Status,
                                                 x.EstEndDate
                                             }).Distinct().ToList();

                    var queryableOrganizationMFormList = (from x in db.MForm
                                                          join j in db.MJob
                                                          on x.MJobUniqueID equals j.UniqueID
                                                          where Account.QueryableOrganizationUniqueIDList.Contains(j.OrganizationUniqueID) && (x.Status == "0" || x.Status == "1" || x.Status == "4" || x.Status == "6")
                                                          select new
                                                          {
                                                              x.UniqueID,
                                                              x.Status,
                                                              x.EstEndDate
                                                          }).Distinct().ToList();

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-orange",
                        Icon = "fa-download",
                        Count = personalMFormList.Count(x => x.Status == "0"),
                        Text = Resources.Resource.MFormStatus_0,
                        Status = "0"
                    });

                    int repairingCount = personalMFormList.Count(x => x.Status == "1");

                    int delayCount = personalMFormList.Count(x => x.Status == "1" && DateTime.Compare(x.EstEndDate, DateTime.Today) < 0);

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-blue",
                        Icon = "fa-wrench",
                        Count = repairingCount - delayCount,
                        Text = Resources.Resource.MFormStatus_1,
                        Status = "1"
                    });

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-exclamation-circle",
                        Count = delayCount,
                        Text = Resources.Resource.MFormStatus_2,
                        Status = "2"
                    });

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-times-circle-o",
                        Count = personalMFormList.Count(x => x.Status == "4"),
                        Text = Resources.Resource.MFormStatus_4,
                        Status = "4"
                    });

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-purple",
                        Icon = "fa-gavel",
                        Count = personalMFormList.Count(x => x.Status == "6"),
                        Text = Resources.Resource.MFormStatus_6,
                        Status = "6"
                    });

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-orange",
                        Icon = "fa-download",
                        Count = queryableOrganizationMFormList.Count(x => x.Status == "0"),
                        Text = Resources.Resource.MFormStatus_0,
                        Status = "0"
                    });

                    repairingCount = queryableOrganizationMFormList.Count(x => x.Status == "1");

                    delayCount = queryableOrganizationMFormList.Count(x => x.Status == "1" && DateTime.Compare(x.EstEndDate, DateTime.Today) < 0);

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-blue",
                        Icon = "fa-wrench",
                        Count = repairingCount - delayCount,
                        Text = Resources.Resource.MFormStatus_1,
                        Status = "1"
                    });

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-exclamation-circle",
                        Count = delayCount,
                        Text = Resources.Resource.MFormStatus_2,
                        Status = "2"
                    });

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-times-circle-o",
                        Count = queryableOrganizationMFormList.Count(x => x.Status == "4"),
                        Text = Resources.Resource.MFormStatus_4,
                        Status = "4"
                    });

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-purple",
                        Icon = "fa-gavel",
                        Count = queryableOrganizationMFormList.Count(x => x.Status == "6"),
                        Text = Resources.Resource.MFormStatus_6,
                        Status = "6"
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
    }
}
