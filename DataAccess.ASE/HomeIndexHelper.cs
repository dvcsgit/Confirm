using Models.Authenticated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Models;
using Models.Home;
using System.Reflection;
using Utility;
using DbEntity.ASE;

namespace DataAccess.ASE
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

            using (ASEDbEntities db = new ASEDbEntities())
            {
                var query = (from x in db.JOBRESULT
                             join y in db.JOBRESULTFLOW
                             on x.UNIQUEID equals y.JOBRESULTUNIQUEID into tmpFlow
                             from y in tmpFlow.DefaultIfEmpty()
                             join j in db.JOB
                             on x.JOBUNIQUEID equals j.UNIQUEID
                             join r in db.ROUTE
                             on j.ROUTEUNIQUEID equals r.UNIQUEID
                             where x.ISNEEDVERIFY == "Y" && (x.ISCOMPLETED == "Y" || DateTime.Compare(baseTime, x.JOBENDTIME.Value) > 0) && Account.QueryableOrganizationUniqueIDList.Contains(r.ORGANIZATIONUNIQUEID)
                             select new
                             {
                                 IsClosed = y != null ? y.ISCLOSED == "Y" : false,
                                 x.UNIQUEID,
                                 OrganizationDescription = x.ORGANIZATIONDESCRIPTION,
                                 JobUniqueID = x.JOBUNIQUEID,
                                 RouteUniqueID = r.UNIQUEID,
                                 BeginDate = x.BEGINDATE,
                                 EndDate = x.ENDDATE,
                                 BeginTime = x.BEGINTIME,
                                 EndTime = x.ENDTIME,
                                 x.DESCRIPTION,
                                 HaveAbnormal = x.HAVEABNORMAL == "Y",
                                 HaveAlert = x.HAVEALERT == "Y",
                                 TimeSpan = x.TIMESPAN,
                                 CompleteRate = x.COMPLETERATE,
                                 CompleteRateLabelClass = x.COMPLETERATELABELCLASS,
                                 ArriveStatus = x.ARRIVESTATUS,
                                 ArriveStatusLabelClass = x.ARRIVESTATUSLABELCLASS,
                                 CheckUsers = x.CHECKUSERS
                             }).AsQueryable();

                itemList = query.Select(x => new EquipmentPatrolVerifyItem
                {
                    IsClosed = x.IsClosed,
                    UniqueID = x.UNIQUEID,
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
                    Description = x.DESCRIPTION,
                    HaveAbnormal = x.HaveAbnormal,
                    HaveAlert = x.HaveAlert,
                    TimeSpan = x.TimeSpan
                }).OrderBy(x => x.BeginDate).ThenBy(x => x.BeginTime).ThenBy(x => x.Description).ToList();

                foreach (var item in itemList)
                {
                    if (!item.IsClosed)
                    {
                        var flow = db.JOBRESULTFLOW.FirstOrDefault(x => x.JOBRESULTUNIQUEID == item.UniqueID);

                        if (flow != null)
                        {
                            var flowLog = db.JOBRESULTFLOWLOG.FirstOrDefault(x => x.JOBRESULTUNIQUEID == flow.JOBRESULTUNIQUEID && x.SEQ == flow.CURRENTSEQ);

                            if (flowLog != null)
                            {
                                item.CurrentVerifyUserID = flowLog.USERID;
                                item.CurrentVerifyUserName = flowLog.USERNAME;
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

            using (ASEDbEntities db = new ASEDbEntities())
            {
                var query = (from f in db.MFORM
                             join j in db.MJOB
                             on f.MJOBUNIQUEID equals j.UNIQUEID
                             join e in db.EQUIPMENT
                             on f.EQUIPMENTUNIQUEID equals e.UNIQUEID
                             join p in db.EQUIPMENTPART
                             on f.PARTUNIQUEID equals p.UNIQUEID into tmpPart
                             from p in tmpPart.DefaultIfEmpty()
                             join x in db.MFORMFLOWLOG
                             on f.UNIQUEID equals x.MFORMUNIQUEID
                             where !x.VERIFYTIME.HasValue && x.USERID == Account.ID
                             select new
                             {
                                 UniqueID = f.UNIQUEID,
                                 j.ORGANIZATIONUNIQUEID,
                                 f.VHNO,
                                 CycleBeginDate = f.CYCLEBEGINDATE,
                                 CycleEndDate = f.CYCLEENDDATE,
                                 EstBeginDate = f.ESTBEGINDATE,
                                 EstEndDate = f.ESTENDDATE,
                                 Subject = j.DESCRIPTION,
                                 EquipmentID = e.ID,
                                 EquipmentName = e.NAME,
                                 PartUniqueID = f.PARTUNIQUEID,
                                 PartDescription = p != null ? p.DESCRIPTION : "",
                                 BeginDate = f.BEGINDATE,
                                 EndDate = f.ENDDATE
                             }).Distinct().AsQueryable();

                foreach (var q in query)
                {
                    var item = new MaintenanceFormVerifyItem()
                    {
                        UniqueID = q.UniqueID,
                        VHNO = q.VHNO,
                        OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(q.ORGANIZATIONUNIQUEID),
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

                    var maintenanceUserList = db.MFORMRESULT.Where(x => x.MFORMUNIQUEID == q.UniqueID).Select(x => x.USERID).Distinct().ToList();

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

            using (ASEDbEntities db = new ASEDbEntities())
            {
                var query = (from f in db.MFORM
                             join j in db.MJOB
                             on f.MJOBUNIQUEID equals j.UNIQUEID
                             join e in db.EQUIPMENT
                             on f.EQUIPMENTUNIQUEID equals e.UNIQUEID
                             join p in db.EQUIPMENTPART
                             on f.PARTUNIQUEID equals p.UNIQUEID into tmpPart
                             from p in tmpPart.DefaultIfEmpty()
                             join x in db.MFORMEXTENDFLOWLOG
                             on f.UNIQUEID equals x.MFORMUNIQUEID
                             where !x.VERIFYTIME.HasValue && x.USERID == Account.ID
                             select new
                             {
                                 UniqueID = f.UNIQUEID,
                                 j.ORGANIZATIONUNIQUEID,
                                 f.VHNO,
                                 CycleBeginDate = f.CYCLEBEGINDATE,
                                 CycleEndDate = f.CYCLEENDDATE,
                                 EstBeginDate = f.ESTBEGINDATE,
                                 EstEndDate = f.ESTENDDATE,
                                 Subject = j.DESCRIPTION,
                                 EquipmentID = e.ID,
                                 EquipmentName = e.NAME,
                                 PartUniqueID = f.PARTUNIQUEID,
                                 PartDescription = p != null ? p.DESCRIPTION : "",
                                 BeginDate = f.BEGINDATE,
                                 EndDate = f.ENDDATE
                             }).Distinct().AsQueryable();

                foreach (var q in query)
                {
                    var item = new MaintenanceFormVerifyItem()
                    {
                        VHNO = q.VHNO,
                        OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(q.ORGANIZATIONUNIQUEID),
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

                    var maintenanceUserList = db.MFORMRESULT.Where(x => x.MFORMUNIQUEID == q.UniqueID).Select(x => x.USERID).Distinct().ToList();

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

            using (ASEDbEntities db = new ASEDbEntities())
            {
                var query = (from f in db.RFORM
                             join o in db.ORGANIZATION
                             on f.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                             join pm in db.ORGANIZATION
                             on f.PMORGANIZATIONUNIQUEID equals pm.UNIQUEID
                             join t in db.RFORMTYPE
                             on f.RFORMTYPEUNIQUEID equals t.UNIQUEID
                             join x in db.RFORMFLOWLOG
                            on f.UNIQUEID equals x.RFORMUNIQUEID
                             where !x.VERIFYTIME.HasValue && x.USERID == Account.ID
                             select new
                             {
                                 OrganizationDescription = o.DESCRIPTION,
                                 MaintenanceOrganizationDescription = pm.DESCRIPTION,
                                 f.VHNO,
                                 f.STATUS,
                                 RepairFormType = t.DESCRIPTION,
                                 EstBeginDate = f.ESTBEGINDATE,
                                 EstEndDate = f.ESTENDDATE,
                                 Subject = f.SUBJECT,
                                 f.EQUIPMENTID,
                                 f.EQUIPMENTNAME,
                                 f.PARTDESCRIPTION,
                                 f.TAKEJOBUSERID
                             }).Distinct().AsQueryable();

                itemList = query.Select(x => new RepairFormVerifyItem
                {
                    OrganizationDescription = x.OrganizationDescription,
                    MaintenanceOrganizationDescription = x.MaintenanceOrganizationDescription,
                    VHNO = x.VHNO,
                    Subject = x.Subject,
                    EstBeginDate = x.EstBeginDate,
                    EstEndDate = x.EstEndDate,
                    EquipmentID = x.EQUIPMENTID,
                    EquipmentName = x.EQUIPMENTNAME,
                    PartDescription = x.PARTDESCRIPTION,
                    RepairFormType = x.RepairFormType,
                    TakeJobUserID = x.TAKEJOBUSERID
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var today = DateTimeHelper.DateTime2DateString(DateTime.Today);

                    //個人負責巡檢異常
                    var query1 = (from a in db.ABNORMAL
                                  join x in db.ABNORMALCHECKRESULT
                                  on a.UNIQUEID equals x.ABNORMALUNIQUEID
                                  join c in db.CHECKRESULT
                                  on x.CHECKRESULTUNIQUEID equals c.UNIQUEID
                                  join y in db.ROUTEMANAGER
                                  on c.ROUTEUNIQUEID equals y.ROUTEUNIQUEID
                                  where y.USERID == Account.ID
                                  select new
                                  {
                                      UniqueID = a.UNIQUEID,
                                      ClosedTime = a.CLOSEDTIME,
                                      CheckDate = c.CHECKDATE,
                                      IsAbnormal = c.ISABNORMAL == "Y",
                                      IsAlert = c.ISALERT == "Y"
                                  }).Distinct().ToList();

                    //個人負責保養異常
                    var query2 = (from a in db.ABNORMAL
                                  join x in db.ABNORMALMFORMSTANDARDRESULT
                                  on a.UNIQUEID equals x.ABNORMALUNIQUEID
                                  join r in db.MFORMSTANDARDRESULT
                                  on x.MFORMSTANDARDRESULTUNIQUEID equals r.UNIQUEID
                                  join y in db.MFORMRESULT
                                  on r.RESULTUNIQUEID equals y.UNIQUEID
                                  where y.USERID == Account.ID
                                  select new
                                  {
                                      UniqueID = a.UNIQUEID,
                                      ClosedTime = a.CLOSEDTIME,
                                      PMDate = y.PMDATE,
                                      IsAbnormal = r.ISABNORMAL == "Y",
                                      IsAlert = r.ISALERT == "Y"
                                  }).Distinct().ToList();

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-exclamation-circle",
                        Count = query1.Count(x => x.IsAbnormal && x.CheckDate == today) + query2.Count(x => x.IsAbnormal && x.PMDate == today),
                        Text = "今日異常項目總數"
                    });

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-exclamation-circle",
                        Count = query1.Count(x => x.IsAlert && x.CheckDate == today) + query2.Count(x => x.IsAlert && x.PMDate == today),
                        Text = "今日注意項目總數"
                    });

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-exclamation-circle",
                        Count = query1.Count(x => x.IsAbnormal && !x.ClosedTime.HasValue) + query2.Count(x => x.IsAbnormal && !x.ClosedTime.HasValue),
                        Text = "累積異常未結案"
                    });

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-orange",
                        Icon = "fa-exclamation-circle",
                        Count = query1.Count(x => x.IsAlert && !x.ClosedTime.HasValue) + query2.Count(x => x.IsAlert && !x.ClosedTime.HasValue),
                        Text = "累積注意未結案"
                    });

                    //組織巡檢異常
                    var query3 = (from a in db.ABNORMAL
                                  join x in db.ABNORMALCHECKRESULT
                                  on a.UNIQUEID equals x.ABNORMALUNIQUEID
                                  join c in db.CHECKRESULT
                                  on x.CHECKRESULTUNIQUEID equals c.UNIQUEID
                                  join r in db.ROUTE
                                  on c.ROUTEUNIQUEID equals r.UNIQUEID
                                  where Account.QueryableOrganizationUniqueIDList.Contains(r.ORGANIZATIONUNIQUEID)
                                  select new
                                  {
                                      UniqueID = a.UNIQUEID,
                                      ClosedTime = a.CLOSEDTIME,
                                      CheckDate = c.CHECKDATE,
                                      IsAbnormal = c.ISABNORMAL == "Y",
                                      IsAlert = c.ISALERT == "Y"
                                  }).Distinct().ToList();

                    //組織保養異常
                    var query4 = (from a in db.ABNORMAL
                                  join x in db.ABNORMALMFORMSTANDARDRESULT
                                  on a.UNIQUEID equals x.ABNORMALUNIQUEID
                                  join r in db.MFORMSTANDARDRESULT
                                  on x.MFORMSTANDARDRESULTUNIQUEID equals r.UNIQUEID
                                  join y in db.MFORMRESULT
                                  on r.RESULTUNIQUEID equals y.UNIQUEID
                                  where Account.QueryableOrganizationUniqueIDList.Contains(r.ORGANIZATIONUNIQUEID)
                                  select new
                                  {
                                      UniqueID = a.UNIQUEID,
                                      ClosedTime = a.CLOSEDTIME,
                                      PMDate = y.PMDATE,
                                      IsAbnormal = r.ISABNORMAL == "Y",
                                      IsAlert = r.ISALERT == "Y"
                                  }).Distinct().ToList();

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-exclamation-circle",
                        Count = query3.Count(x => x.IsAbnormal && x.CheckDate == today) + query4.Count(x => x.IsAbnormal && x.PMDate == today),
                        Text = "今日異常項目總數"
                    });

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-exclamation-circle",
                        Count = query3.Count(x => x.IsAlert && x.CheckDate == today) + query4.Count(x => x.IsAlert && x.PMDate == today),
                        Text = "今日注意項目總數"
                    });

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-exclamation-circle",
                        Count = query3.Count(x => x.IsAbnormal && !x.ClosedTime.HasValue) + query4.Count(x => x.IsAbnormal && !x.ClosedTime.HasValue),
                        Text = "累積異常未結案"
                    });

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-orange",
                        Icon = "fa-exclamation-circle",
                        Count = query3.Count(x => x.IsAlert && !x.ClosedTime.HasValue) + query4.Count(x => x.IsAlert && !x.ClosedTime.HasValue),
                        Text = "累積注意未結案"
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

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query = (from f in db.RFORM
                                 where f.STATUS == "0" || f.STATUS == "2" || f.STATUS == "3" || f.STATUS == "4" || f.STATUS == "7" || f.STATUS == "9"
                                 select new
                                 {
                                     UniqueID = f.UNIQUEID,
                                     Status = f.STATUS,
                                     OrganizationUniqueID = f.ORGANIZATIONUNIQUEID,
                                     MaintenanceOrganizationUniqueID = f.PMORGANIZATIONUNIQUEID,
                                     EstEndDate = f.ESTENDDATE,
                                     TakeJobUserID = f.TAKEJOBUSERID
                                 }).AsQueryable();

                    var personalRFormList = query.Where(x => x.TakeJobUserID == Account.ID).ToList();

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-orange",
                        Icon = "fa-download",
                        Count = personalRFormList.Count(x => x.Status == "0" || x.Status == "3"),
                        Text = Resources.Resource.RFormStatus_0
                    });

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-orange",
                        Icon = "fa-download",
                        Count = personalRFormList.Count(x => x.Status == "2"),
                        Text = Resources.Resource.RFormStatus_2
                    });

                    int repairingCount = personalRFormList.Count(x => x.Status == "4");

                    int delayCount = personalRFormList.Count(x => x.Status == "4" && x.EstEndDate.HasValue && DateTime.Compare(x.EstEndDate.Value, DateTime.Today) < 0);

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-blue",
                        Icon = "fa-wrench",
                        Count = repairingCount - delayCount,
                        Text = Resources.Resource.RFormStatus_4
                    });

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-exclamation-circle",
                        Count = delayCount,
                        Text = Resources.Resource.RFormStatus_5
                    });

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-times-circle-o",
                        Count = personalRFormList.Count(x => x.Status == "7"),
                        Text = Resources.Resource.RFormStatus_7
                    });

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-purple",
                        Icon = "fa-gavel",
                        Count = personalRFormList.Count(x => x.Status == "9"),
                        Text = Resources.Resource.RFormStatus_9
                    });


                    var queryableOrganizationRFormList = query.Where(x => Account.QueryableOrganizationUniqueIDList.Contains(x.OrganizationUniqueID) || Account.QueryableOrganizationUniqueIDList.Contains(x.MaintenanceOrganizationUniqueID)).ToList();

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-orange",
                        Icon = "fa-download",
                        Count = queryableOrganizationRFormList.Count(x => x.Status == "0" || x.Status == "3"),
                        Text = Resources.Resource.RFormStatus_0
                    });

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-orange",
                        Icon = "fa-download",
                        Count = queryableOrganizationRFormList.Count(x => x.Status == "2"),
                        Text = Resources.Resource.RFormStatus_2
                    });

                    repairingCount = queryableOrganizationRFormList.Count(x => x.Status == "4");

                    delayCount = queryableOrganizationRFormList.Count(x => x.Status == "4" && x.EstEndDate.HasValue && DateTime.Compare(x.EstEndDate.Value, DateTime.Today) < 0);

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-blue",
                        Icon = "fa-wrench",
                        Count = repairingCount - delayCount,
                        Text = Resources.Resource.RFormStatus_4
                    });

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-exclamation-circle",
                        Count = delayCount,
                        Text = Resources.Resource.RFormStatus_5
                    });

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-times-circle-o",
                        Count = queryableOrganizationRFormList.Count(x => x.Status == "7"),
                        Text = Resources.Resource.RFormStatus_7
                    });

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-purple",
                        Icon = "fa-gavel",
                        Count = queryableOrganizationRFormList.Count(x => x.Status == "9"),
                        Text = Resources.Resource.RFormStatus_9
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

        public static RequestResult GetMaintenanceFormList(Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new SummaryViewModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var personalMFormList = (from x in db.MFORM
                                             join j in db.MJOB
                                             on x.MJOBUNIQUEID equals j.UNIQUEID
                                             join y in db.MJOBUSER
                                             on j.UNIQUEID equals y.MJOBUNIQUEID
                                             where y.USERID == Account.ID && (x.STATUS == "0" || x.STATUS == "1" || x.STATUS == "4" || x.STATUS == "6")
                                             select new
                                             {
                                                 UniqueID = x.UNIQUEID,
                                                 Status = x.STATUS,
                                                 EstEndDate = x.ESTENDDATE
                                             }).Distinct().ToList();

                    var queryableOrganizationMFormList = (from x in db.MFORM
                                                          join j in db.MJOB
                                                          on x.MJOBUNIQUEID equals j.UNIQUEID
                                                          where Account.QueryableOrganizationUniqueIDList.Contains(j.ORGANIZATIONUNIQUEID) && (x.STATUS == "0" || x.STATUS == "1" || x.STATUS == "4" || x.STATUS == "6")
                                                          select new
                                                          {
                                                              UniqueID = x.UNIQUEID,
                                                              Status = x.STATUS,
                                                              EstEndDate = x.ESTENDDATE
                                                          }).Distinct().ToList();

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-orange",
                        Icon = "fa-download",
                        Count = personalMFormList.Count(x => x.Status == "0"),
                        Text = Resources.Resource.MFormStatus_0
                    });

                    int repairingCount = personalMFormList.Count(x => x.Status == "1");

                    int delayCount = personalMFormList.Count(x => x.Status == "1" && DateTime.Compare(x.EstEndDate, DateTime.Today) < 0);

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-blue",
                        Icon = "fa-wrench",
                        Count = repairingCount - delayCount,
                        Text = Resources.Resource.MFormStatus_1
                    });

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-exclamation-circle",
                        Count = delayCount,
                        Text = Resources.Resource.MFormStatus_2
                    });

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-times-circle-o",
                        Count = personalMFormList.Count(x => x.Status == "4"),
                        Text = Resources.Resource.MFormStatus_4
                    });

                    model.PersonalItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-purple",
                        Icon = "fa-gavel",
                        Count = personalMFormList.Count(x => x.Status == "6"),
                        Text = Resources.Resource.MFormStatus_6
                    });

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-orange",
                        Icon = "fa-download",
                        Count = queryableOrganizationMFormList.Count(x => x.Status == "0"),
                        Text = Resources.Resource.MFormStatus_0
                    });

                    repairingCount = queryableOrganizationMFormList.Count(x => x.Status == "1");

                    delayCount = queryableOrganizationMFormList.Count(x => x.Status == "1" && DateTime.Compare(x.EstEndDate, DateTime.Today) < 0);

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-blue",
                        Icon = "fa-wrench",
                        Count = repairingCount - delayCount,
                        Text = Resources.Resource.MFormStatus_1
                    });

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-exclamation-circle",
                        Count = delayCount,
                        Text = Resources.Resource.MFormStatus_2
                    });

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-red",
                        Icon = "fa-times-circle-o",
                        Count = queryableOrganizationMFormList.Count(x => x.Status == "4"),
                        Text = Resources.Resource.MFormStatus_4
                    });

                    model.QueryableOrganizationItemList.Add(new SummaryItem()
                    {
                        BoxColor = "infobox-purple",
                        Icon = "fa-gavel",
                        Count = queryableOrganizationMFormList.Count(x => x.Status == "6"),
                        Text = Resources.Resource.MFormStatus_6
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
