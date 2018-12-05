using DbEntity.ASE;
using Models.Authenticated;
using Models.EquipmentMaintenance.AbnormalHandlingManagement;
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

namespace DataAccess.ASE
{
    public class AbnormalHandlingDataAccessor
    {
        public static RequestResult Query(QueryParameters Parameters, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new GridViewModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (!string.IsNullOrEmpty(Parameters.UniqueID))
                    {
                        var query1 = (from abnormal in db.ABNORMAL
                                      join x in db.ABNORMALCHECKRESULT
                                      on abnormal.UNIQUEID equals x.ABNORMALUNIQUEID
                                      join c in db.CHECKRESULT
                                      on x.CHECKRESULTUNIQUEID equals c.UNIQUEID
                                      join a in db.ARRIVERECORD
                                      on c.ARRIVERECORDUNIQUEID equals a.UNIQUEID
                                      where abnormal.UNIQUEID==Parameters.UniqueID
                                      select new
                                      {
                                          UniqueID = abnormal.UNIQUEID,
                                          RouteUniqueID = c.ROUTEUNIQUEID,
                                          ControlPointID = c.CONTROLPOINTID,
                                          ControlPointDescription = c.CONTROLPOINTDESCRIPTION,
                                          EquipmentUniqueID = c.EQUIPMENTUNIQUEID,
                                          EquipmentID = c.EQUIPMENTID,
                                          EquipmentName = c.EQUIPMENTNAME,
                                          PartDescription = c.PARTDESCRIPTION,
                                          CheckItemID = c.CHECKITEMID,
                                          CheckItemDescription = c.CHECKITEMDESCRIPTION,
                                          CheckDate = c.CHECKDATE,
                                          CheckTime = c.CHECKTIME,
                                          IsAbnormal = c.ISABNORMAL == "Y",
                                          IsAlert = c.ISALERT == "Y",
                                          ClosedTime = abnormal.CLOSEDTIME,
                                          ClosedUserID = abnormal.CLOSEDUSERID,
                                          CheckUserID = a.USERID
                                      }).AsQueryable();

                        var query2 = (from abnormal in db.ABNORMAL
                                      join x in db.ABNORMALMFORMSTANDARDRESULT
                                      on abnormal.UNIQUEID equals x.ABNORMALUNIQUEID
                                      join sr in db.MFORMSTANDARDRESULT
                                      on x.MFORMSTANDARDRESULTUNIQUEID equals sr.UNIQUEID
                                      join r in db.MFORMRESULT
                                      on sr.RESULTUNIQUEID equals r.UNIQUEID
                                      where abnormal.UNIQUEID==Parameters.UniqueID
                                      select new
                                      {
                                          UniqueID = abnormal.UNIQUEID,
                                          EquipmentUniqueID = sr.EQUIPMENTUNIQUEID,
                                          EquipmentID = sr.EQUIPMENTID,
                                          EquipmentName = sr.EQUIPMENTNAME,
                                          PartDescription = sr.PARTDESCRIPTION,
                                          StandardID = sr.STANDARDID,
                                          StandardDescription = sr.STANDARDDESCRIPTION,
                                          CheckDate = r.PMDATE,
                                          CheckTime = r.PMTIME,
                                          IsAbnormal = sr.ISABNORMAL == "Y",
                                          IsAlert = sr.ISALERT == "Y",
                                          ClosedTime = abnormal.CLOSEDTIME,
                                          ClosedUserID = abnormal.CLOSEDUSERID,
                                          CheckUserID = r.USERID
                                      }).AsQueryable();

                        var queryResult = query1.ToList();

                        foreach (var q in queryResult)
                        {
                            var item = new GridItem
                            {
                                UniqueID = q.UniqueID,
                                ControlPointID = q.ControlPointID,
                                ControlPointDescription = q.ControlPointDescription,
                                EquipmentID = q.EquipmentID,
                                EquipmentName = q.EquipmentName,
                                PartDescription = q.PartDescription,
                                CheckItemID = q.CheckItemID,
                                CheckItemDescription = q.CheckItemDescription,
                                Date = q.CheckDate,
                                Time = q.CheckTime,
                                ClosedTime = q.ClosedTime,
                                IsAbnormal = q.IsAbnormal,
                                IsAlert = q.IsAlert,
                                CheckUser = UserDataAccessor.GetUser(q.CheckUserID),
                                ClosedUser = UserDataAccessor.GetUser(q.ClosedUserID)
                            };

                            var routeManagerList = db.ROUTEMANAGER.Where(x => x.ROUTEUNIQUEID == q.RouteUniqueID).Select(x => x.USERID).OrderBy(x => x).ToList();

                            foreach (var routeManager in routeManagerList)
                            {
                                item.ChargePersonList.Add(UserDataAccessor.GetUser(routeManager));
                            }

                            model.ItemList.Add(item);
                        }

                        model.ItemList.AddRange(query2.ToList().Select(x => new GridItem
                        {
                            UniqueID = x.UniqueID,
                            EquipmentID = x.EquipmentID,
                            EquipmentName = x.EquipmentName,
                            PartDescription = x.PartDescription,
                            StandardID = x.StandardID,
                            StandardDescription = x.StandardDescription,
                            Date = x.CheckDate,
                            Time = x.CheckTime,
                            ClosedTime = x.ClosedTime,
                            IsAbnormal = x.IsAbnormal,
                            IsAlert = x.IsAlert,
                            CheckUser = UserDataAccessor.GetUser(x.CheckUserID),
                            ClosedUser = UserDataAccessor.GetUser(x.ClosedUserID),
                            ChargePersonList = new List<Models.Shared.UserModel>() { 
                         UserDataAccessor.GetUser(x.CheckUserID)
                        }
                        }).ToList());
                    }
                    else if (!string.IsNullOrEmpty(Parameters.Status))
                    {
                        var today = DateTimeHelper.DateTime2DateString(DateTime.Today);

                        if (Parameters.Status == "1" || Parameters.Status == "2" || Parameters.Status == "3" || Parameters.Status == "4")
                        {
                            var query1 = (from abnormal in db.ABNORMAL
                                          join x in db.ABNORMALCHECKRESULT
                                          on abnormal.UNIQUEID equals x.ABNORMALUNIQUEID
                                          join c in db.CHECKRESULT
                                          on x.CHECKRESULTUNIQUEID equals c.UNIQUEID
                                          join y in db.ROUTEMANAGER
                                          on c.ROUTEUNIQUEID equals y.ROUTEUNIQUEID
                                          join a in db.ARRIVERECORD
                                          on c.ARRIVERECORDUNIQUEID equals a.UNIQUEID
                                          where y.USERID == Account.ID
                                          select new
                                          {
                                              UniqueID = abnormal.UNIQUEID,
                                              RouteUniqueID = c.ROUTEUNIQUEID,
                                              ControlPointID = c.CONTROLPOINTID,
                                              ControlPointDescription = c.CONTROLPOINTDESCRIPTION,
                                              EquipmentUniqueID = c.EQUIPMENTUNIQUEID,
                                              EquipmentID = c.EQUIPMENTID,
                                              EquipmentName = c.EQUIPMENTNAME,
                                              PartDescription = c.PARTDESCRIPTION,
                                              CheckItemID = c.CHECKITEMID,
                                              CheckItemDescription = c.CHECKITEMDESCRIPTION,
                                              CheckDate = c.CHECKDATE,
                                              CheckTime = c.CHECKTIME,
                                              IsAbnormal = c.ISABNORMAL == "Y",
                                              IsAlert = c.ISALERT == "Y",
                                              ClosedTime = abnormal.CLOSEDTIME,
                                              ClosedUserID = abnormal.CLOSEDUSERID,
                                              CheckUserID = a.USERID
                                          }).Distinct().AsQueryable();

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
                                              EquipmentUniqueID = r.EQUIPMENTUNIQUEID,
                                              EquipmentID = r.EQUIPMENTID,
                                              EquipmentName = r.EQUIPMENTNAME,
                                              PartDescription = r.PARTDESCRIPTION,
                                              StandardID = r.STANDARDID,
                                              StandardDescription = r.STANDARDDESCRIPTION,
                                              CheckDate = y.PMDATE,
                                              CheckTime = y.PMTIME,
                                              IsAbnormal = r.ISABNORMAL == "Y",
                                              IsAlert = r.ISALERT == "Y",
                                              ClosedTime = a.CLOSEDTIME,
                                              ClosedUserID = a.CLOSEDUSERID,
                                              CheckUserID = y.USERID
                                          }).Distinct().AsQueryable();

                            if (Parameters.Status == "1")
                            {
                                query1 = query1.Where(x => x.IsAbnormal && x.CheckDate == today);
                                query2 = query2.Where(x => x.IsAbnormal && x.CheckDate == today);
                            }

                            if (Parameters.Status == "2")
                            {
                                query1 = query1.Where(x => x.IsAlert && x.CheckDate == today);
                                query2 = query2.Where(x => x.IsAlert && x.CheckDate == today);
                            }

                            if (Parameters.Status == "3")
                            {
                                query1 = query1.Where(x => x.IsAbnormal && !x.ClosedTime.HasValue);
                                query2 = query2.Where(x => x.IsAbnormal && !x.ClosedTime.HasValue);
                            }

                            if (Parameters.Status == "4")
                            {
                                query1 = query1.Where(x => x.IsAlert && !x.ClosedTime.HasValue);
                                query2 = query2.Where(x => x.IsAlert && !x.ClosedTime.HasValue);
                            }

                            var queryResult = query1.ToList();

                            foreach (var q in queryResult)
                            {
                                var item = new GridItem
                                {
                                    UniqueID = q.UniqueID,
                                    ControlPointID = q.ControlPointID,
                                    ControlPointDescription = q.ControlPointDescription,
                                    EquipmentID = q.EquipmentID,
                                    EquipmentName = q.EquipmentName,
                                    PartDescription = q.PartDescription,
                                    CheckItemID = q.CheckItemID,
                                    CheckItemDescription = q.CheckItemDescription,
                                    Date = q.CheckDate,
                                    Time = q.CheckTime,
                                    ClosedTime = q.ClosedTime,
                                    IsAbnormal = q.IsAbnormal,
                                    IsAlert = q.IsAlert,
                                    CheckUser = UserDataAccessor.GetUser(q.CheckUserID),
                                    ClosedUser = UserDataAccessor.GetUser(q.ClosedUserID)
                                };

                                var routeManagerList = db.ROUTEMANAGER.Where(x => x.ROUTEUNIQUEID == q.RouteUniqueID).Select(x => x.USERID).OrderBy(x => x).ToList();

                                foreach (var routeManager in routeManagerList)
                                {
                                    item.ChargePersonList.Add(UserDataAccessor.GetUser(routeManager));
                                }

                                model.ItemList.Add(item);
                            }

                            model.ItemList.AddRange(query2.ToList().Select(x => new GridItem
                            {
                                UniqueID = x.UniqueID,
                                EquipmentID = x.EquipmentID,
                                EquipmentName = x.EquipmentName,
                                PartDescription = x.PartDescription,
                                StandardID = x.StandardID,
                                StandardDescription = x.StandardDescription,
                                Date = x.CheckDate,
                                Time = x.CheckTime,
                                ClosedTime = x.ClosedTime,
                                IsAbnormal = x.IsAbnormal,
                                IsAlert = x.IsAlert,
                                CheckUser = UserDataAccessor.GetUser(x.CheckUserID),
                                ClosedUser = UserDataAccessor.GetUser(x.ClosedUserID),
                                ChargePersonList = new List<Models.Shared.UserModel>() { 
                                    UserDataAccessor.GetUser(x.CheckUserID)
                                }
                            }).ToList());

                            model.ItemList = model.ItemList.OrderBy(x => x.Date).ThenBy(x => x.Time).ThenBy(x => x.EquipmentDisplay).ToList();
                        }

                        if (Parameters.Status == "5" || Parameters.Status == "6" || Parameters.Status == "7" || Parameters.Status == "8")
                        {
                            var query1 = (from abnormal in db.ABNORMAL
                                          join x in db.ABNORMALCHECKRESULT
                                          on abnormal.UNIQUEID equals x.ABNORMALUNIQUEID
                                          join c in db.CHECKRESULT
                                          on x.CHECKRESULTUNIQUEID equals c.UNIQUEID
                                          join r in db.ROUTE
                                          on c.ROUTEUNIQUEID equals r.UNIQUEID
                                          join a in db.ARRIVERECORD
                                          on c.ARRIVERECORDUNIQUEID equals a.UNIQUEID
                                          where Account.QueryableOrganizationUniqueIDList.Contains(r.ORGANIZATIONUNIQUEID)
                                          select new
                                          {
                                              UniqueID = abnormal.UNIQUEID,
                                              RouteUniqueID = c.ROUTEUNIQUEID,
                                              ControlPointID = c.CONTROLPOINTID,
                                              ControlPointDescription = c.CONTROLPOINTDESCRIPTION,
                                              EquipmentUniqueID = c.EQUIPMENTUNIQUEID,
                                              EquipmentID = c.EQUIPMENTID,
                                              EquipmentName = c.EQUIPMENTNAME,
                                              PartDescription = c.PARTDESCRIPTION,
                                              CheckItemID = c.CHECKITEMID,
                                              CheckItemDescription = c.CHECKITEMDESCRIPTION,
                                              CheckDate = c.CHECKDATE,
                                              CheckTime = c.CHECKTIME,
                                              IsAbnormal = c.ISABNORMAL == "Y",
                                              IsAlert = c.ISALERT == "Y",
                                              ClosedTime = abnormal.CLOSEDTIME,
                                              ClosedUserID = abnormal.CLOSEDUSERID,
                                              CheckUserID = a.USERID
                                          }).Distinct().AsQueryable();

                            var query2 = (from a in db.ABNORMAL
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
                                              EquipmentUniqueID = r.EQUIPMENTUNIQUEID,
                                              EquipmentID = r.EQUIPMENTID,
                                              EquipmentName = r.EQUIPMENTNAME,
                                              PartDescription = r.PARTDESCRIPTION,
                                              StandardID = r.STANDARDID,
                                              StandardDescription = r.STANDARDDESCRIPTION,
                                              CheckDate = y.PMDATE,
                                              CheckTime = y.PMTIME,
                                              IsAbnormal = r.ISABNORMAL == "Y",
                                              IsAlert = r.ISALERT == "Y",
                                              ClosedTime = a.CLOSEDTIME,
                                              ClosedUserID = a.CLOSEDUSERID,
                                              CheckUserID = y.USERID
                                          }).Distinct().AsQueryable();

                            if (Parameters.Status == "5")
                            {
                                query1 = query1.Where(x => x.IsAbnormal && x.CheckDate == today);
                                query2 = query2.Where(x => x.IsAbnormal && x.CheckDate == today);
                            }

                            if (Parameters.Status == "6")
                            {
                                query1 = query1.Where(x => x.IsAlert && x.CheckDate == today);
                                query2 = query2.Where(x => x.IsAlert && x.CheckDate == today);
                            }

                            if (Parameters.Status == "7")
                            {
                                query1 = query1.Where(x => x.IsAbnormal && !x.ClosedTime.HasValue);
                                query2 = query2.Where(x => x.IsAbnormal && !x.ClosedTime.HasValue);
                            }

                            if (Parameters.Status == "8")
                            {
                                query1 = query1.Where(x => x.IsAlert && !x.ClosedTime.HasValue);
                                query2 = query2.Where(x => x.IsAlert && !x.ClosedTime.HasValue);
                            }

                            var queryResult = query1.ToList();

                            foreach (var q in queryResult)
                            {
                                var item = new GridItem
                                {
                                    UniqueID = q.UniqueID,
                                    ControlPointID = q.ControlPointID,
                                    ControlPointDescription = q.ControlPointDescription,
                                    EquipmentID = q.EquipmentID,
                                    EquipmentName = q.EquipmentName,
                                    PartDescription = q.PartDescription,
                                    CheckItemID = q.CheckItemID,
                                    CheckItemDescription = q.CheckItemDescription,
                                    Date = q.CheckDate,
                                    Time = q.CheckTime,
                                    ClosedTime = q.ClosedTime,
                                    IsAbnormal = q.IsAbnormal,
                                    IsAlert = q.IsAlert,
                                    CheckUser = UserDataAccessor.GetUser(q.CheckUserID),
                                    ClosedUser = UserDataAccessor.GetUser(q.ClosedUserID)
                                };

                                var routeManagerList = db.ROUTEMANAGER.Where(x => x.ROUTEUNIQUEID == q.RouteUniqueID).Select(x => x.USERID).OrderBy(x => x).ToList();

                                foreach (var routeManager in routeManagerList)
                                {
                                    item.ChargePersonList.Add(UserDataAccessor.GetUser(routeManager));
                                }

                                model.ItemList.Add(item);
                            }

                            model.ItemList.AddRange(query2.ToList().Select(x => new GridItem
                            {
                                UniqueID = x.UniqueID,
                                EquipmentID = x.EquipmentID,
                                EquipmentName = x.EquipmentName,
                                PartDescription = x.PartDescription,
                                StandardID = x.StandardID,
                                StandardDescription = x.StandardDescription,
                                Date = x.CheckDate,
                                Time = x.CheckTime,
                                ClosedTime = x.ClosedTime,
                                IsAbnormal = x.IsAbnormal,
                                IsAlert = x.IsAlert,
                                CheckUser = UserDataAccessor.GetUser(x.CheckUserID),
                                ClosedUser = UserDataAccessor.GetUser(x.ClosedUserID),
                                ChargePersonList = new List<Models.Shared.UserModel>() { 
                                    UserDataAccessor.GetUser(x.CheckUserID)
                                }
                            }).ToList());

                            model.ItemList = model.ItemList.OrderBy(x => x.Date).ThenBy(x => x.Time).ThenBy(x => x.EquipmentDisplay).ToList();
                        }
                    }
                    else
                    {
                        var downStreamOrganizationList = OrganizationDataAccessor.GetDownStreamOrganizationList(Parameters.OrganizationUniqueID, true);

                        var organizationList = Account.QueryableOrganizationUniqueIDList.Intersect(downStreamOrganizationList);

                        var query1 = (from abnormal in db.ABNORMAL
                                      join x in db.ABNORMALCHECKRESULT
                                      on abnormal.UNIQUEID equals x.ABNORMALUNIQUEID
                                      join c in db.CHECKRESULT
                                      on x.CHECKRESULTUNIQUEID equals c.UNIQUEID
                                      join a in db.ARRIVERECORD
                                      on c.ARRIVERECORDUNIQUEID equals a.UNIQUEID
                                      where organizationList.Contains(c.ORGANIZATIONUNIQUEID)
                                      select new
                                      {
                                          UniqueID = abnormal.UNIQUEID,
                                          RouteUniqueID = c.ROUTEUNIQUEID,
                                          ControlPointID = c.CONTROLPOINTID,
                                          ControlPointDescription = c.CONTROLPOINTDESCRIPTION,
                                          EquipmentUniqueID = c.EQUIPMENTUNIQUEID,
                                          EquipmentID = c.EQUIPMENTID,
                                          EquipmentName = c.EQUIPMENTNAME,
                                          PartDescription = c.PARTDESCRIPTION,
                                          CheckItemID = c.CHECKITEMID,
                                          CheckItemDescription = c.CHECKITEMDESCRIPTION,
                                          CheckDate = c.CHECKDATE,
                                          CheckTime = c.CHECKTIME,
                                          IsAbnormal = c.ISABNORMAL == "Y",
                                          IsAlert = c.ISALERT == "Y",
                                          ClosedTime = abnormal.CLOSEDTIME,
                                          ClosedUserID = abnormal.CLOSEDUSERID,
                                          CheckUserID = a.USERID
                                      }).AsQueryable();

                        var query2 = (from abnormal in db.ABNORMAL
                                      join x in db.ABNORMALMFORMSTANDARDRESULT
                                      on abnormal.UNIQUEID equals x.ABNORMALUNIQUEID
                                      join sr in db.MFORMSTANDARDRESULT
                                      on x.MFORMSTANDARDRESULTUNIQUEID equals sr.UNIQUEID
                                      join r in db.MFORMRESULT
                                      on sr.RESULTUNIQUEID equals r.UNIQUEID
                                      where organizationList.Contains(sr.ORGANIZATIONUNIQUEID)
                                      select new
                                      {
                                          UniqueID = abnormal.UNIQUEID,
                                          EquipmentUniqueID = sr.EQUIPMENTUNIQUEID,
                                          EquipmentID = sr.EQUIPMENTID,
                                          EquipmentName = sr.EQUIPMENTNAME,
                                          PartDescription = sr.PARTDESCRIPTION,
                                          StandardID = sr.STANDARDID,
                                          StandardDescription = sr.STANDARDDESCRIPTION,
                                          CheckDate = r.PMDATE,
                                          CheckTime = r.PMTIME,
                                          IsAbnormal = sr.ISABNORMAL == "Y",
                                          IsAlert = sr.ISALERT == "Y",
                                          ClosedTime = abnormal.CLOSEDTIME,
                                          ClosedUserID = abnormal.CLOSEDUSERID,
                                          CheckUserID = r.USERID
                                      }).AsQueryable();

                        if (!string.IsNullOrEmpty(Parameters.BeginDateString))
                        {
                            query1 = query1.Where(x => string.Compare(x.CheckDate, Parameters.BeginDate) >= 0);
                            query2 = query2.Where(x => string.Compare(x.CheckDate, Parameters.BeginDate) >= 0);
                        }

                        if (!string.IsNullOrEmpty(Parameters.EndDateString))
                        {
                            query1 = query1.Where(x => string.Compare(x.CheckDate, Parameters.EndDate) <= 0);
                            query2 = query2.Where(x => string.Compare(x.CheckDate, Parameters.EndDate) <= 0);
                        }

                        if (!string.IsNullOrEmpty(Parameters.EquipmentUniqueID))
                        {
                            query1 = query1.Where(x => x.EquipmentUniqueID == Parameters.EquipmentUniqueID);
                            query2 = query2.Where(x => x.EquipmentUniqueID == Parameters.EquipmentUniqueID);
                        }

                        if (!string.IsNullOrEmpty(Parameters.UniqueID))
                        {
                            query1 = query1.Where(x => x.UniqueID == Parameters.UniqueID);
                            query2 = query2.Where(x => x.UniqueID == Parameters.UniqueID);
                        }

                        if (!string.IsNullOrEmpty(Parameters.AbnormalType))
                        {
                            if (Parameters.AbnormalType == "1")
                            {
                                query1 = query1.Where(x => x.IsAbnormal);
                                query2 = query2.Where(x => x.IsAbnormal);
                            }

                            if (Parameters.AbnormalType == "2")
                            {
                                query1 = query1.Where(x => !x.IsAbnormal&&x.IsAlert);
                                query2 = query2.Where(x => !x.IsAbnormal && x.IsAlert);
                            }
                        }

                        if (!string.IsNullOrEmpty(Parameters.ClosedStatus))
                        {
                            if (Parameters.ClosedStatus == "0")
                            {
                                query1 = query1.Where(x => !x.ClosedTime.HasValue);
                                query2 = query2.Where(x => !x.ClosedTime.HasValue);
                            }

                            if (Parameters.ClosedStatus == "1")
                            {
                                query1 = query1.Where(x => x.ClosedTime.HasValue);
                                query2 = query2.Where(x => x.ClosedTime.HasValue);
                            }
                        }

                        if (string.IsNullOrEmpty(Parameters.Type))
                        {
                            var queryResult = query1.ToList();

                            foreach (var q in queryResult)
                            {
                                var item = new GridItem
                                {
                                    UniqueID = q.UniqueID,
                                    ControlPointID = q.ControlPointID,
                                    ControlPointDescription = q.ControlPointDescription,
                                    EquipmentID = q.EquipmentID,
                                    EquipmentName = q.EquipmentName,
                                    PartDescription = q.PartDescription,
                                    CheckItemID = q.CheckItemID,
                                    CheckItemDescription = q.CheckItemDescription,
                                    Date = q.CheckDate,
                                    Time = q.CheckTime,
                                    ClosedTime = q.ClosedTime,
                                    IsAbnormal = q.IsAbnormal,
                                    IsAlert = q.IsAlert,
                                    CheckUser = UserDataAccessor.GetUser(q.CheckUserID),
                                    ClosedUser = UserDataAccessor.GetUser(q.ClosedUserID)
                                };

                                var routeManagerList = db.ROUTEMANAGER.Where(x => x.ROUTEUNIQUEID == q.RouteUniqueID).Select(x => x.USERID).OrderBy(x => x).ToList();

                                foreach (var routeManager in routeManagerList)
                                {
                                    item.ChargePersonList.Add(UserDataAccessor.GetUser(routeManager));
                                }

                                model.ItemList.Add(item);
                            }

                            model.ItemList.AddRange(query2.ToList().Select(x => new GridItem
                            {
                                UniqueID = x.UniqueID,
                                EquipmentID = x.EquipmentID,
                                EquipmentName = x.EquipmentName,
                                PartDescription = x.PartDescription,
                                StandardID = x.StandardID,
                                StandardDescription = x.StandardDescription,
                                Date = x.CheckDate,
                                Time = x.CheckTime,
                                ClosedTime = x.ClosedTime,
                                IsAbnormal = x.IsAbnormal,
                                IsAlert = x.IsAlert,
                                CheckUser = UserDataAccessor.GetUser(x.CheckUserID),
                                ClosedUser = UserDataAccessor.GetUser(x.ClosedUserID),
                                ChargePersonList = new List<Models.Shared.UserModel>() { 
                         UserDataAccessor.GetUser(x.CheckUserID)
                        }
                            }).ToList());
                        }
                        else
                        {
                            if (Parameters.Type == "P")
                            {
                                var queryResult = query1.ToList();

                                foreach (var q in queryResult)
                                {
                                    var item = new GridItem
                                    {
                                        UniqueID = q.UniqueID,
                                        ControlPointID = q.ControlPointID,
                                        ControlPointDescription = q.ControlPointDescription,
                                        EquipmentID = q.EquipmentID,
                                        EquipmentName = q.EquipmentName,
                                        PartDescription = q.PartDescription,
                                        CheckItemID = q.CheckItemID,
                                        CheckItemDescription = q.CheckItemDescription,
                                        Date = q.CheckDate,
                                        Time = q.CheckTime,
                                        ClosedTime = q.ClosedTime,
                                        IsAbnormal = q.IsAbnormal,
                                        IsAlert = q.IsAlert,
                                        CheckUser = UserDataAccessor.GetUser(q.CheckUserID),
                                        ClosedUser = UserDataAccessor.GetUser(q.ClosedUserID)
                                    };

                                    var routeManagerList = db.ROUTEMANAGER.Where(x => x.ROUTEUNIQUEID == q.RouteUniqueID).Select(x => x.USERID).OrderBy(x => x).ToList();

                                    foreach (var routeManager in routeManagerList)
                                    {
                                        item.ChargePersonList.Add(UserDataAccessor.GetUser(routeManager));
                                    }

                                    model.ItemList.Add(item);
                                }
                            }
                            else if (Parameters.Type == "M")
                            {
                                model.ItemList.AddRange(query2.ToList().Select(x => new GridItem
                                {
                                    UniqueID = x.UniqueID,
                                    EquipmentID = x.EquipmentID,
                                    EquipmentName = x.EquipmentName,
                                    PartDescription = x.PartDescription,
                                    StandardID = x.StandardID,
                                    StandardDescription = x.StandardDescription,
                                    Date = x.CheckDate,
                                    Time = x.CheckTime,
                                    ClosedTime = x.ClosedTime,
                                    IsAbnormal = x.IsAbnormal,
                                    IsAlert = x.IsAlert,
                                    CheckUser = UserDataAccessor.GetUser(x.CheckUserID),
                                    ClosedUser = UserDataAccessor.GetUser(x.ClosedUserID),
                                    ChargePersonList = new List<Models.Shared.UserModel>() { 
                         UserDataAccessor.GetUser(x.CheckUserID)
                        }
                                }).ToList());
                            }
                        }
                        

                        model.ItemList = model.ItemList.OrderBy(x => x.Date).ThenBy(x => x.Time).ThenBy(x => x.EquipmentDisplay).ToList();
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

        public static RequestResult GetDetailViewModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new DetailViewModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query1 = (from abnormal in db.ABNORMAL
                                  join x in db.ABNORMALCHECKRESULT
                                  on abnormal.UNIQUEID equals x.ABNORMALUNIQUEID
                                  join c in db.CHECKRESULT
                                  on x.CHECKRESULTUNIQUEID equals c.UNIQUEID
                                  join a in db.ARRIVERECORD
                                  on c.ARRIVERECORDUNIQUEID equals a.UNIQUEID
                                  where abnormal.UNIQUEID==UniqueID
                                  select new
                                  {
                                      UniqueID = abnormal.UNIQUEID,
                                      CheckResultUniqueID = c.UNIQUEID,
                                      RouteUniqueID = c.ROUTEUNIQUEID,
                                      ControlPointID = c.CONTROLPOINTID,
                                      ControlPointDescription = c.CONTROLPOINTDESCRIPTION,
                                      EquipmentUniqueID = c.EQUIPMENTUNIQUEID,
                                      EquipmentID = c.EQUIPMENTID,
                                      EquipmentName = c.EQUIPMENTNAME,
                                      PartDescription = c.PARTDESCRIPTION,
                                      CheckItemID = c.CHECKITEMID,
                                      CheckItemDescription = c.CHECKITEMDESCRIPTION,
                                      CheckDate = c.CHECKDATE,
                                      CheckTime = c.CHECKTIME,
                                      IsAbnormal = c.ISABNORMAL == "Y",
                                      IsAlert = c.ISALERT == "Y",
                                      Result = c.RESULT,
                                      LowerLimit = c.LOWERLIMIT,
                                      LowerAlertLimit = c.LOWERALERTLIMIT,
                                      UpperAlertLimit = c.UPPERALERTLIMIT,
                                      UpperLimit = c.UPPERLIMIT,
                                      Unit = c.UNIT,
                                      ClosedTime = abnormal.CLOSEDTIME,
                                      ClosedUserID = abnormal.CLOSEDUSERID,
                                      ClosedRemark = abnormal.CLOSEDREMARK,
                                      CheckUserID = a.USERID
                                  }).FirstOrDefault();

                    var query2 = (from abnormal in db.ABNORMAL
                                  join x in db.ABNORMALMFORMSTANDARDRESULT
                                  on abnormal.UNIQUEID equals x.ABNORMALUNIQUEID
                                  join sr in db.MFORMSTANDARDRESULT
                                  on x.MFORMSTANDARDRESULTUNIQUEID equals sr.UNIQUEID
                                  join r in db.MFORMRESULT
                                  on sr.RESULTUNIQUEID equals r.UNIQUEID
                                  where abnormal.UNIQUEID == UniqueID
                                  select new
                                  {
                                      UniqueID = abnormal.UNIQUEID,
                                      EquipmentUniqueID = sr.EQUIPMENTUNIQUEID,
                                      EquipmentID = sr.EQUIPMENTID,
                                      EquipmentName = sr.EQUIPMENTNAME,
                                      PartDescription = sr.PARTDESCRIPTION,
                                      StandardID = sr.STANDARDID,
                                      StandardDescription = sr.STANDARDDESCRIPTION,
                                      CheckDate = r.PMDATE,
                                      CheckTime = r.PMTIME,
                                      IsAbnormal = sr.ISABNORMAL == "Y",
                                      IsAlert = sr.ISALERT == "Y",
                                      Result = sr.RESULT,
                                      LowerLimit = sr.LOWERLIMIT,
                                      LowerAlertLimit = sr.LOWERALERTLIMIT,
                                      UpperAlertLimit = sr.UPPERALERTLIMIT,
                                      UpperLimit = sr.UPPERLIMIT,
                                      Unit = sr.UNIT,
                                      ClosedTime = abnormal.CLOSEDTIME,
                                      ClosedUserID = abnormal.CLOSEDUSERID,
                                      ClosedRemark = abnormal.CLOSEDREMARK,
                                      CheckUserID = r.USERID
                                  }).FirstOrDefault();

                    if (query1 != null)
                    {
                        model = new DetailViewModel()
                        {
                            UniqueID = query1.UniqueID,
                            ControlPointID = query1.ControlPointID,
                            ControlPointDescription = query1.ControlPointDescription,
                            EquipmentID = query1.EquipmentID,
                            EquipmentName = query1.EquipmentName,
                            PartDescription = query1.PartDescription,
                            CheckItemID = query1.CheckItemID,
                            CheckItemDescription = query1.CheckItemDescription,
                            Date = query1.CheckDate,
                            Time = query1.CheckTime,
                            IsAbnormal = query1.IsAbnormal,
                            IsAlert = query1.IsAlert,
                            Result = query1.Result,
                            LowerLimit = query1.LowerLimit.HasValue?Convert.ToDouble(query1.LowerLimit.Value):default(double?),
                            LowerAlertLimit = query1.LowerAlertLimit.HasValue ? Convert.ToDouble(query1.LowerAlertLimit.Value) : default(double?),
                            UpperAlertLimit = query1.UpperAlertLimit.HasValue ? Convert.ToDouble(query1.UpperAlertLimit.Value) : default(double?),
                            UpperLimit = query1.UpperLimit.HasValue ? Convert.ToDouble(query1.UpperLimit.Value) : default(double?),
                            Unit = query1.Unit,
                            CheckUser = UserDataAccessor.GetUser(query1.CheckUserID),
                            ClosedUser = UserDataAccessor.GetUser(query1.ClosedUserID),
                            ClosedTime = query1.ClosedTime,
                             ClosedRemark = query1.ClosedRemark,
                            AbnormalReasonList = db.CHECKRESULTABNORMALREASON.Where(a => a.CHECKRESULTUNIQUEID == query1.CheckResultUniqueID).OrderBy(a => a.ABNORMALREASONID).Select(a => new AbnormalReasonModel
                            {
                                Description = a.ABNORMALREASONDESCRIPTION,
                                Remark = a.ABNORMALREASONREMARK,
                                HandlingMethodList = db.CHECKRESULTHANDLINGMETHOD.Where(h => h.CHECKRESULTUNIQUEID == query1.CheckResultUniqueID && h.ABNORMALREASONUNIQUEID == a.ABNORMALREASONUNIQUEID).OrderBy(h => h.HANDLINGMETHODID).Select(h => new HandlingMethodModel
                                {
                                    Description = h.HANDLINGMETHODDESCRIPTION,
                                    Remark = h.HANDLINGMETHODREMARK
                                }).ToList()
                            }).ToList(),
                            BeforePhotoList = db.ABNORMALPHOTO.Where(p => p.ABNORMALUNIQUEID == query1.UniqueID && p.TYPE == "B").OrderBy(p => p.SEQ).ToList().Select(p => p.ABNORMALUNIQUEID+"_"+p.TYPE + "_" + p.SEQ + "." + p.EXTENSION).ToList(),
                            AfterPhotoList = db.ABNORMALPHOTO.Where(p => p.ABNORMALUNIQUEID == query1.UniqueID && p.TYPE == "A").OrderBy(p => p.SEQ).ToList().Select(p => p.ABNORMALUNIQUEID + "_" + p.TYPE + "_" + p.SEQ + "." + p.EXTENSION).ToList(),
                            FileList = db.ABNORMALFILE.Where(f => f.ABNORMALUNIQUEID == query1.UniqueID).ToList().Select(f => new FileModel
                            {
                                AbnormalUniqueID = f.ABNORMALUNIQUEID,
                                Seq = f.SEQ,
                                FileName = f.FILENAME,
                                Extension = f.EXTENSION,
                                Size = f.CONTENTLENGTH.Value,
                                UploadTime = f.UPLOADTIME.Value,
                                 IsSaved=true
                            }).OrderBy(f => f.UploadTime).ToList()
                        };

                        var routeManagerList = db.ROUTEMANAGER.Where(x => x.ROUTEUNIQUEID == query1.RouteUniqueID).Select(x => x.USERID).OrderBy(x => x).ToList();

                        foreach (var routeManager in routeManagerList)
                        {
                            model.ChargePersonList.Add(UserDataAccessor.GetUser(routeManager));
                        }
                    }
                    else if (query2 != null)
                    {
                        model = new DetailViewModel()
                        {
                            UniqueID = query2.UniqueID,
                            EquipmentID = query2.EquipmentID,
                            EquipmentName = query2.EquipmentName,
                            PartDescription = query2.PartDescription,
                            StandardID = query2.StandardID,
                            StandardDescription = query2.StandardDescription,
                            Date = query2.CheckDate,
                            Time = query2.CheckTime,
                            IsAbnormal = query2.IsAbnormal,
                            IsAlert = query2.IsAlert,
                            Result = query2.Result,
                            LowerLimit = query2.LowerLimit.HasValue ? Convert.ToDouble(query2.LowerLimit.Value) : default(double?),
                            LowerAlertLimit = query2.LowerAlertLimit.HasValue ? Convert.ToDouble(query2.LowerAlertLimit.Value) : default(double?),
                            UpperAlertLimit = query2.UpperAlertLimit.HasValue ? Convert.ToDouble(query2.UpperAlertLimit.Value) : default(double?),
                            UpperLimit = query2.UpperLimit.HasValue ? Convert.ToDouble(query2.UpperLimit.Value) : default(double?),
                            Unit = query2.Unit,
                            CheckUser = UserDataAccessor.GetUser(query2.CheckUserID),
                            ClosedUser = UserDataAccessor.GetUser(query2.ClosedUserID),
                            ClosedTime = query2.ClosedTime,
                            ClosedRemark = query2.ClosedRemark,
                            BeforePhotoList = db.ABNORMALPHOTO.Where(p => p.ABNORMALUNIQUEID == query2.UniqueID && p.TYPE == "B").OrderBy(p => p.SEQ).ToList().Select(p => p.ABNORMALUNIQUEID + "_" + p.TYPE + "_" + p.SEQ + "." + p.EXTENSION).ToList(),
                            AfterPhotoList = db.ABNORMALPHOTO.Where(p => p.ABNORMALUNIQUEID == query2.UniqueID && p.TYPE == "A").OrderBy(p => p.SEQ).ToList().Select(p => p.ABNORMALUNIQUEID + "_" + p.TYPE + "_" + p.SEQ + "." + p.EXTENSION).ToList(),
                            FileList = db.ABNORMALFILE.Where(f => f.ABNORMALUNIQUEID == query2.UniqueID).ToList().Select(f => new FileModel
                            {
                                AbnormalUniqueID = f.ABNORMALUNIQUEID,
                                Seq = f.SEQ,
                                FileName = f.FILENAME,
                                Extension = f.EXTENSION,
                                Size = f.CONTENTLENGTH.Value,
                                UploadTime = f.UPLOADTIME.Value,
                                IsSaved = true
                            }).OrderBy(f => f.UploadTime).ToList()
                        };

                        model.ChargePersonList.Add(UserDataAccessor.GetUser(query2.CheckUserID));
                    }

                    var repairFormList = (from x in db.ABNORMALRFORM
                                          join f in db.RFORM
                                          on x.RFORMUNIQUEID equals f.UNIQUEID
                                          join t in db.RFORMTYPE
                                          on f.RFORMTYPEUNIQUEID equals t.UNIQUEID
                                          join e in db.EQUIPMENT
                                          on f.EQUIPMENTUNIQUEID equals e.UNIQUEID into tmpEquipment
                                          from e in tmpEquipment.DefaultIfEmpty()
                                          join p in db.EQUIPMENTPART 
                                          on f.PARTUNIQUEID equals p.UNIQUEID into tmpPart
                                          from p in tmpPart.DefaultIfEmpty()
                                          where x.ABNORMALUNIQUEID == model.UniqueID
                                          select new
                                          {
                                              OrganizationUniqueID = f.ORGANIZATIONUNIQUEID,
                                              MaintenanceOrganizationUniqueID = f.PMORGANIZATIONUNIQUEID,
                                              UniqueID = f.UNIQUEID,
                                              f.VHNO,
                                              Status =f.STATUS,
                                              EstBeginDate = f.ESTBEGINDATE,
                                              RepairFormType = t.DESCRIPTION,
                                              EstEndDate = f.ESTENDDATE,
                                              Subject = f.SUBJECT,
                                              EquipmentID = e!=null?e.ID:"",
                                              EquipmentName = e!=null?e.NAME:"",
                                              PartDescription = p!=null?p.DESCRIPTION:""
                                          }).ToList();

                    foreach (var form in repairFormList)
                    {
                        var repairFormModel = new RepairFormModel()
                        {
                            UniqueID = form.UniqueID,
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(form.OrganizationUniqueID),
                            MaintenanceOrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(form.MaintenanceOrganizationUniqueID),
                            VHNO = form.VHNO,
                            Subject = form.Subject,
                            EstBeginDate = form.EstBeginDate,
                            EstEndDate = form.EstEndDate,
                            Status = form.Status,
                            EquipmentID = form.EquipmentID,
                            EquipmentName = form.EquipmentName,
                             PartDescription = form.PartDescription,
                              RepairFormType=form.RepairFormType
                        };

                        var flow = db.RFORMFLOW.FirstOrDefault(x => x.RFORMUNIQUEID == form.UniqueID);

                        if (flow != null)
                        {
                            repairFormModel.IsClosed = flow.ISCLOSED == "Y";
                        }
                        else
                        {
                            repairFormModel.IsClosed = false;
                        }

                        model.RepairFormList.Add(repairFormModel);
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

        public static RequestResult GetEditFormModel(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new EditFormModel();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query1 = (from abnormal in db.ABNORMAL
                                  join x in db.ABNORMALCHECKRESULT
                                  on abnormal.UNIQUEID equals x.ABNORMALUNIQUEID
                                  join c in db.CHECKRESULT
                                  on x.CHECKRESULTUNIQUEID equals c.UNIQUEID
                                  join a in db.ARRIVERECORD
                                  on c.ARRIVERECORDUNIQUEID equals a.UNIQUEID
                                  where abnormal.UNIQUEID == UniqueID
                                  select new
                                  {
                                      UniqueID = abnormal.UNIQUEID,
                                      CheckResultUniqueID = c.UNIQUEID,
                                      RouteUniqueID = c.ROUTEUNIQUEID,
                                      ControlPointID = c.CONTROLPOINTID,
                                      ControlPointDescription = c.CONTROLPOINTDESCRIPTION,
                                      EquipmentUniqueID = c.EQUIPMENTUNIQUEID,
                                      EquipmentID = c.EQUIPMENTID,
                                      EquipmentName = c.EQUIPMENTNAME,
                                      PartDescription = c.PARTDESCRIPTION,
                                      CheckItemID = c.CHECKITEMID,
                                      CheckItemDescription = c.CHECKITEMDESCRIPTION,
                                      CheckDate = c.CHECKDATE,
                                      CheckTime = c.CHECKTIME,
                                      IsAbnormal = c.ISABNORMAL == "Y",
                                      IsAlert = c.ISALERT == "Y",
                                      Result = c.RESULT,
                                      LowerLimit = c.LOWERLIMIT,
                                      LowerAlertLimit = c.LOWERALERTLIMIT,
                                      UpperAlertLimit = c.UPPERALERTLIMIT,
                                      UpperLimit = c.UPPERLIMIT,
                                      Unit = c.UNIT,
                                      ClosedTime = abnormal.CLOSEDTIME,
                                      ClosedUserID = abnormal.CLOSEDUSERID,
                                      ClosedRemark = abnormal.CLOSEDREMARK,
                                      CheckUserID = a.USERID
                                  }).FirstOrDefault();

                    var query2 = (from abnormal in db.ABNORMAL
                                  join x in db.ABNORMALMFORMSTANDARDRESULT
                                  on abnormal.UNIQUEID equals x.ABNORMALUNIQUEID
                                  join sr in db.MFORMSTANDARDRESULT
                                  on x.MFORMSTANDARDRESULTUNIQUEID equals sr.UNIQUEID
                                  join r in db.MFORMRESULT
                                  on sr.RESULTUNIQUEID equals r.UNIQUEID
                                  where abnormal.UNIQUEID == UniqueID
                                  select new
                                  {
                                      UniqueID = abnormal.UNIQUEID,
                                      EquipmentUniqueID = sr.EQUIPMENTUNIQUEID,
                                      EquipmentID = sr.EQUIPMENTID,
                                      EquipmentName = sr.EQUIPMENTNAME,
                                      PartDescription = sr.PARTDESCRIPTION,
                                      StandardID = sr.STANDARDID,
                                      StandardDescription = sr.STANDARDDESCRIPTION,
                                      CheckDate = r.PMDATE,
                                      CheckTime = r.PMTIME,
                                      IsAbnormal = sr.ISABNORMAL == "Y",
                                      IsAlert = sr.ISALERT == "Y",
                                      Result = sr.RESULT,
                                      LowerLimit = sr.LOWERLIMIT,
                                      LowerAlertLimit = sr.LOWERALERTLIMIT,
                                      UpperAlertLimit = sr.UPPERALERTLIMIT,
                                      UpperLimit = sr.UPPERLIMIT,
                                      Unit = sr.UNIT,
                                      ClosedTime = abnormal.CLOSEDTIME,
                                      ClosedUserID = abnormal.CLOSEDUSERID,
                                      ClosedRemark = abnormal.CLOSEDREMARK,
                                      CheckUserID = r.USERID
                                  }).FirstOrDefault();

                    if (query1 != null)
                    {
                        model = new EditFormModel()
                        {
                            UniqueID = query1.UniqueID,
                            ControlPointID = query1.ControlPointID,
                            ControlPointDescription = query1.ControlPointDescription,
                            EquipmentID = query1.EquipmentID,
                            EquipmentName = query1.EquipmentName,
                            PartDescription = query1.PartDescription,
                            CheckItemID = query1.CheckItemID,
                            CheckItemDescription = query1.CheckItemDescription,
                            Date = query1.CheckDate,
                            Time = query1.CheckTime,
                            IsAbnormal = query1.IsAbnormal,
                            IsAlert = query1.IsAlert,
                            Result = query1.Result,
                            LowerLimit = query1.LowerLimit.HasValue ? Convert.ToDouble(query1.LowerLimit.Value) : default(double?),
                            LowerAlertLimit = query1.LowerAlertLimit.HasValue ? Convert.ToDouble(query1.LowerAlertLimit.Value) : default(double?),
                            UpperAlertLimit = query1.UpperAlertLimit.HasValue ? Convert.ToDouble(query1.UpperAlertLimit.Value) : default(double?),
                            UpperLimit = query1.UpperLimit.HasValue ? Convert.ToDouble(query1.UpperLimit.Value) : default(double?),
                            Unit = query1.Unit,
                            CheckUser = UserDataAccessor.GetUser(query1.CheckUserID),
                            ClosedUser = UserDataAccessor.GetUser(query1.ClosedUserID),
                            ClosedTime = query1.ClosedTime,
                            ClosedRemark = query1.ClosedRemark,
                            AbnormalReasonList = db.CHECKRESULTABNORMALREASON.Where(a => a.CHECKRESULTUNIQUEID == query1.CheckResultUniqueID).OrderBy(a => a.ABNORMALREASONID).Select(a => new AbnormalReasonModel
                            {
                                Description = a.ABNORMALREASONDESCRIPTION,
                                Remark = a.ABNORMALREASONREMARK,
                                HandlingMethodList = db.CHECKRESULTHANDLINGMETHOD.Where(h => h.CHECKRESULTUNIQUEID == query1.CheckResultUniqueID && h.ABNORMALREASONUNIQUEID == a.ABNORMALREASONUNIQUEID).OrderBy(h => h.HANDLINGMETHODID).Select(h => new HandlingMethodModel
                                {
                                    Description = h.HANDLINGMETHODDESCRIPTION,
                                    Remark = h.HANDLINGMETHODREMARK
                                }).ToList()
                            }).ToList(),
                            FileList = db.ABNORMALFILE.Where(f => f.ABNORMALUNIQUEID == query1.UniqueID).ToList().Select(f => new FileModel
                            {
                                AbnormalUniqueID = f.ABNORMALUNIQUEID,
                                Seq = f.SEQ,
                                FileName = f.FILENAME,
                                Extension = f.EXTENSION,
                                Size = f.CONTENTLENGTH.Value,
                                UploadTime = f.UPLOADTIME.Value,
                                IsSaved = true
                            }).OrderBy(f => f.UploadTime).ToList()
                        };

                        var routeManagerList = db.ROUTEMANAGER.Where(x => x.ROUTEUNIQUEID == query1.RouteUniqueID).Select(x => x.USERID).OrderBy(x => x).ToList();

                        foreach (var routeManager in routeManagerList)
                        {
                            model.ChargePersonList.Add(UserDataAccessor.GetUser(routeManager));
                        }
                    }
                    else if (query2 != null)
                    {
                        model = new EditFormModel()
                        {
                            UniqueID = query2.UniqueID,
                            EquipmentID = query2.EquipmentID,
                            EquipmentName = query2.EquipmentName,
                            PartDescription = query2.PartDescription,
                            StandardID = query2.StandardID,
                            StandardDescription = query2.StandardDescription,
                            Date = query2.CheckDate,
                            Time = query2.CheckTime,
                            IsAbnormal = query2.IsAbnormal,
                            IsAlert = query2.IsAlert,
                            Result = query2.Result,
                            LowerLimit = query2.LowerLimit.HasValue ? Convert.ToDouble(query2.LowerLimit.Value) : default(double?),
                            LowerAlertLimit = query2.LowerAlertLimit.HasValue ? Convert.ToDouble(query2.LowerAlertLimit.Value) : default(double?),
                            UpperAlertLimit = query2.UpperAlertLimit.HasValue ? Convert.ToDouble(query2.UpperAlertLimit.Value) : default(double?),
                            UpperLimit = query2.UpperLimit.HasValue ? Convert.ToDouble(query2.UpperLimit.Value) : default(double?),
                            Unit = query2.Unit,
                            CheckUser = UserDataAccessor.GetUser(query2.CheckUserID),
                            ClosedUser = UserDataAccessor.GetUser(query2.ClosedUserID),
                            ClosedTime = query2.ClosedTime,
                            ClosedRemark = query2.ClosedRemark,
                            FileList = db.ABNORMALFILE.Where(f => f.ABNORMALUNIQUEID == query2.UniqueID).ToList().Select(f => new FileModel
                            {
                                AbnormalUniqueID = f.ABNORMALUNIQUEID,
                                Seq = f.SEQ,
                                FileName = f.FILENAME,
                                Extension = f.EXTENSION,
                                Size = f.CONTENTLENGTH.Value,
                                UploadTime = f.UPLOADTIME.Value,
                                IsSaved = true
                            }).OrderBy(f => f.UploadTime).ToList()
                        };

                        model.ChargePersonList.Add(UserDataAccessor.GetUser(query2.CheckUserID));
                    }

                    var beforePhotoList = db.ABNORMALPHOTO.Where(x => x.ABNORMALUNIQUEID == model.UniqueID && x.TYPE == "B").OrderBy(x => x.SEQ).ToList();

                    foreach (var photo in beforePhotoList)
                    {
                        model.BeforePhotoList.Add(new PhotoModel()
                        {
                            TempUniqueID = Guid.NewGuid().ToString(),
                            AbnormalUniqueID = photo.ABNORMALUNIQUEID,
                            Seq = photo.SEQ,
                            Extension = photo.EXTENSION,
                            Type = photo.TYPE,
                            IsSaved = true
                        });
                    }

                    var afterPhotoList = db.ABNORMALPHOTO.Where(x => x.ABNORMALUNIQUEID == model.UniqueID && x.TYPE == "A").OrderBy(x => x.SEQ).ToList();

                    foreach (var photo in afterPhotoList)
                    {
                        model.AfterPhotoList.Add(new PhotoModel()
                        {
                            TempUniqueID = Guid.NewGuid().ToString(),
                            AbnormalUniqueID = photo.ABNORMALUNIQUEID,
                            Seq = photo.SEQ,
                            Extension = photo.EXTENSION,
                            Type = photo.TYPE,
                            IsSaved = true
                        });
                    }

                    model.RepairFormList = GetRepairFormList(model.UniqueID);
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

        public static RequestResult Save(EditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var abnormal = db.ABNORMAL.First(x => x.UNIQUEID == Model.UniqueID);

                    abnormal.CLOSEDREMARK = Model.ClosedRemark;

                    var beforePhotoList = db.ABNORMALPHOTO.Where(x => x.ABNORMALUNIQUEID == Model.UniqueID && x.TYPE == "B").ToList();

                    foreach (var photo in beforePhotoList)
                    {
                        if (!Model.BeforePhotoList.Any(x => x.Seq == photo.SEQ))
                        {
                            db.ABNORMALPHOTO.Remove(photo);

                            try
                            {
                                System.IO.File.Delete(Path.Combine(Config.EquipmentMaintenancePhotoFolderPath, string.Format("{0}_{1}_{2}.{3}", Model.UniqueID, photo.TYPE, photo.SEQ, photo.EXTENSION)));
                            }
                            catch { }
                        }
                    }

                    var afterPhotoList = db.ABNORMALPHOTO.Where(x => x.ABNORMALUNIQUEID == Model.UniqueID && x.TYPE == "A").ToList();

                    foreach (var photo in afterPhotoList)
                    {
                        if (!Model.AfterPhotoList.Any(x => x.Seq == photo.SEQ))
                        {
                            db.ABNORMALPHOTO.Remove(photo);

                            try
                            {
                                System.IO.File.Delete(Path.Combine(Config.EquipmentMaintenancePhotoFolderPath, string.Format("{0}_{1}_{2}.{3}", Model.UniqueID, photo.TYPE, photo.SEQ, photo.EXTENSION)));
                            }
                            catch { }
                        }
                    }

                    foreach (var photo in Model.BeforePhotoList)
                    {
                        if (!photo.IsSaved)
                        {
                            db.ABNORMALPHOTO.Add(new ABNORMALPHOTO()
                            {
                                ABNORMALUNIQUEID = Model.UniqueID,
                                SEQ = photo.Seq,
                                EXTENSION = photo.Extension,
                                TYPE = photo.Type
                            });

                            System.IO.File.Move(photo.TempFullFileName, Path.Combine(Config.EquipmentMaintenancePhotoFolderPath, string.Format("{0}_{1}_{2}.{3}", Model.UniqueID, photo.Type, photo.Seq, photo.Extension)));
                        }
                    }

                    foreach (var photo in Model.AfterPhotoList)
                    {
                        if (!photo.IsSaved)
                        {
                            db.ABNORMALPHOTO.Add(new ABNORMALPHOTO()
                            {
                                ABNORMALUNIQUEID = Model.UniqueID,
                                SEQ = photo.Seq,
                                EXTENSION = photo.Extension,
                                TYPE = photo.Type
                            });

                            System.IO.File.Move(photo.TempFullFileName, Path.Combine(Config.EquipmentMaintenancePhotoFolderPath, string.Format("{0}_{1}_{2}.{3}", Model.UniqueID, photo.Type, photo.Seq, photo.Extension)));
                        }
                    }

                    var fileList = db.ABNORMALFILE.Where(x => x.ABNORMALUNIQUEID == Model.UniqueID).ToList();

                    foreach (var file in fileList)
                    {
                        if (!Model.FileList.Any(x => x.Seq == file.SEQ))
                        {
                            db.ABNORMALFILE.Remove(file);

                            try
                            {
                                System.IO.File.Delete(Path.Combine(Config.EquipmentMaintenanceFileFolderPath, string.Format("{0}_{1}.{2}", Model.UniqueID, file.SEQ, file.EXTENSION)));
                            }
                            catch { }
                        }
                    }

                    foreach (var file in Model.FileList)
                    {
                        if (!file.IsSaved)
                        {
                            db.ABNORMALFILE.Add(new ABNORMALFILE()
                            {
                                ABNORMALUNIQUEID = Model.UniqueID,
                                SEQ = file.Seq,
                                EXTENSION = file.Extension,
                                FILENAME = file.FileName,
                                UPLOADTIME = file.UploadTime,
                                CONTENTLENGTH = file.Size
                            });

                            System.IO.File.Move(file.TempFullFileName, Path.Combine(Config.EquipmentMaintenanceFileFolderPath, string.Format("{0}_{1}.{2}", Model.UniqueID, file.Seq, file.Extension)));
                        }
                    }

                    foreach (var repairForm in Model.RepairFormList)
                    {
                        if (!db.ABNORMALRFORM.Any(x => x.ABNORMALUNIQUEID == Model.UniqueID && x.RFORMUNIQUEID == repairForm.UniqueID))
                        {
                            db.ABNORMALRFORM.Add(new ABNORMALRFORM()
                            {
                                ABNORMALUNIQUEID = Model.UniqueID,
                                RFORMUNIQUEID = repairForm.UniqueID
                            });
                        }
                    }

                    db.SaveChanges();

                    result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Save, Resources.Resource.Success));
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

        public static RequestResult Closed(EditFormModel Model, Account Account)
        {
            RequestResult result = new RequestResult();

            try
            {
                result = Save(Model);

                if (result.IsSuccess)
                {
                    if (string.IsNullOrEmpty(Model.ClosedRemark) && Model.RepairFormList.Count == 0)
                    {
                        result.ReturnFailedMessage(string.Format("{0} {1} {2}", Resources.Resource.CommentRequired, Resources.Resource.Or, Resources.Resource.TransRepairForm));
                    }
                    else
                    {
                        using (ASEDbEntities db = new ASEDbEntities())
                        {
                            var abnormal = db.ABNORMAL.First(x => x.UNIQUEID == Model.UniqueID);

                            abnormal.CLOSEDTIME = DateTime.Now;
                            abnormal.CLOSEDUSERID = Account.ID;

                            db.SaveChanges();
                        }

                        result.ReturnSuccessMessage(string.Format("{0} {1}", Resources.Resource.Closed, Resources.Resource.Success));
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

        public static RequestResult GetRepairFormCreateFormModel(string AbnormalUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = new RepairFormCreateFormModel()
                {
                    RepairFormTypeSelectItemList = new List<SelectListItem>() 
                    { 
                        Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                    },
                    SubjectSelectItemList = new List<SelectListItem>() 
                    { 
                        Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                    },
                    AbnormalUniqueID = AbnormalUniqueID
                };

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var query1 = (from a in db.ABNORMAL
                                  join x in db.ABNORMALCHECKRESULT
                                  on a.UNIQUEID equals x.ABNORMALUNIQUEID
                                  join c in db.CHECKRESULT
                                  on x.CHECKRESULTUNIQUEID equals c.UNIQUEID
                                  where a.UNIQUEID == AbnormalUniqueID
                                  select new
                                  {
                                      CheckResultUniqueID = c.UNIQUEID,
                                      OrganizationUniqueID = c.ORGANIZATIONUNIQUEID,
                                      EquipmentUniqueID = c.EQUIPMENTUNIQUEID,
                                      EquipmentID = c.EQUIPMENTID,
                                      EquipmentName = c.EQUIPMENTNAME,
                                      PartUniqueID = c.PARTUNIQUEID,
                                      PartDescription = c.PARTDESCRIPTION,
                                    CheckItemDescription=  c.CHECKITEMDESCRIPTION
                                  }).FirstOrDefault();

                    var query2 = (from a in db.ABNORMAL
                                  join x in db.ABNORMALMFORMSTANDARDRESULT
                                  on a.UNIQUEID equals x.ABNORMALUNIQUEID
                                  join sr in db.MFORMSTANDARDRESULT
                                  on x.MFORMSTANDARDRESULTUNIQUEID equals sr.UNIQUEID
                                  where a.UNIQUEID == AbnormalUniqueID
                                  select new
                                  {
                                      OrganizationUniqueID = sr.ORGANIZATIONUNIQUEID,
                                      EquipmentUniqueID = sr.EQUIPMENTUNIQUEID,
                                      EquipmentID = sr.EQUIPMENTID,
                                      EquipmentName = sr.EQUIPMENTNAME,
                                      PartUniqueID = sr.PARTUNIQUEID,
                                      PartDescription = sr.PARTDESCRIPTION,
                                     StandardDescription= sr.STANDARDDESCRIPTION
                                  }).FirstOrDefault();

                    var ancestorOrganizationUniqueID = string.Empty;

                    if (query1 != null)
                    {
                        ancestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(query1.OrganizationUniqueID);
                        model.OrganizationUniqueID = query1.OrganizationUniqueID;
                        model.AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(query1.OrganizationUniqueID);
                        model.FullOrganizationDescription = OrganizationDataAccessor.GetOrganizationFullDescription(query1.OrganizationUniqueID);

                        if (!string.IsNullOrEmpty(query1.EquipmentUniqueID))
                        {
                            var equipment = db.EQUIPMENT.FirstOrDefault(x => x.UNIQUEID == query1.EquipmentUniqueID);

                            if (equipment != null && !string.IsNullOrEmpty(equipment.PMORGANIZATIONUNIQUEID))
                            {
                                var maintenanceOrganization = OrganizationDataAccessor.GetOrganization(equipment.PMORGANIZATIONUNIQUEID);

                                model.MaintenanceOrganization = string.Format("{0}/{1}", maintenanceOrganization.ID, maintenanceOrganization.Description);

                                model.FormInput.MaintenanceOrganizationUniqueID = equipment.PMORGANIZATIONUNIQUEID;
                            }
                        }

                        model.EquipmentID = query1.EquipmentID;
                        model.EquipmentName = query1.EquipmentName;
                        model.PartDescription = !string.IsNullOrEmpty(query1.PartUniqueID) && query1.PartUniqueID != "*" ? query1.PartDescription : "";

                        string reason = string.Empty;

                        var abnormalReasonList = db.CHECKRESULTABNORMALREASON.Where(x => x.CHECKRESULTUNIQUEID == query1.CheckResultUniqueID).OrderBy(x => x.ABNORMALREASONID).ToList();

                        if (abnormalReasonList.Count > 0)
                        {
                            var sb = new StringBuilder();

                            foreach (var abnormalReason in abnormalReasonList)
                            {
                                if (!string.IsNullOrEmpty(abnormalReason.ABNORMALREASONDESCRIPTION))
                                {
                                    sb.Append(abnormalReason.ABNORMALREASONDESCRIPTION);
                                }
                                else
                                {
                                    sb.Append(abnormalReason.ABNORMALREASONREMARK);
                                }

                                sb.Append("、");
                            }

                            sb.Remove(sb.Length - 1, 1);

                            reason = sb.ToString();
                        }

                        model.FormInput.EquipmentUniqueID = string.Format("{0}{1}{2}", query1.EquipmentUniqueID, Define.Seperator, query1.PartUniqueID);
                        model.FormInput.Subject = query1.CheckItemDescription;
                        model.FormInput.Description = reason;
                    }

                    if (query2 != null)
                    {
                        ancestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(query2.OrganizationUniqueID);
                        model.OrganizationUniqueID = query2.OrganizationUniqueID;
                        model.AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(query2.OrganizationUniqueID);
                        model.FullOrganizationDescription = OrganizationDataAccessor.GetOrganizationFullDescription(query2.OrganizationUniqueID);

                        if (!string.IsNullOrEmpty(query2.EquipmentUniqueID))
                        {
                            var equipment = db.EQUIPMENT.FirstOrDefault(x => x.UNIQUEID == query2.EquipmentUniqueID);

                            if (equipment != null && !string.IsNullOrEmpty(equipment.PMORGANIZATIONUNIQUEID))
                            {
                                var maintenanceOrganization = OrganizationDataAccessor.GetOrganization(equipment.PMORGANIZATIONUNIQUEID);

                                model.MaintenanceOrganization = string.Format("{0}/{1}", maintenanceOrganization.ID, maintenanceOrganization.Description);

                                model.FormInput.MaintenanceOrganizationUniqueID = equipment.PMORGANIZATIONUNIQUEID;
                            }
                        }

                        model.EquipmentID = query2.EquipmentID;
                        model.EquipmentName = query2.EquipmentName;
                        model.PartDescription = !string.IsNullOrEmpty(query2.PartUniqueID) && query2.PartUniqueID != "*" ? query2.PartDescription : "";

                        model.FormInput.EquipmentUniqueID = string.Format("{0}{1}{2}", query2.EquipmentUniqueID, Define.Seperator, query2.PartUniqueID);
                        model.FormInput.Subject = query2.StandardDescription;
                    }

                    var repairFormTypeList = db.RFORMTYPE.Where(x => x.ANCESTORORGANIZATIONUNIQUEID == ancestorOrganizationUniqueID).OrderBy(x => x.DESCRIPTION).ToList();

                    foreach (var repairFormType in repairFormTypeList)
                    {
                        model.RepairFormTypeSelectItemList.Add(new SelectListItem()
                        {
                            Value = repairFormType.UNIQUEID,
                            Text = repairFormType.DESCRIPTION
                        });

                        model.RepairFormTypeSubjectList.Add(repairFormType.UNIQUEID, (from x in db.RFORMTYPESUBJECT
                                                                                      join s in db.RFORMSUBJECT
                                                                                      on x.SUBJECTUNIQUEID equals s.UNIQUEID
                                                                                      where x.RFORMTYPEUNIQUEID == repairFormType.UNIQUEID
                                                                                      orderby x.SEQ
                                                                                      select new Models.EquipmentMaintenance.AbnormalHandlingManagement.RFormSubject
                                                                                      {
                                                                                          UniqueID = s.UNIQUEID,
                                                                                          ID = s.ID,
                                                                                          Description = s.DESCRIPTION,
                                                                                          AncestorOrganizationUniqueID = s.ANCESTORORGANIZATIONUNIQUEID
                                                                                      }).ToList());
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

        public static RequestResult CreateRepairForm(RepairFormCreateFormModel Model, string UserID)
        {
            RequestResult result = new RequestResult();

            try
            {
                string jobManagerID = string.Empty;
                string maintenanceOrganizationUniqueID = string.Empty;

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    if (Model.FormInput.IsRepairBySelf)
                    {
                        maintenanceOrganizationUniqueID = db.ACCOUNT.First(x => x.ID == UserID).ORGANIZATIONUNIQUEID;
                        jobManagerID = UserID;
                    }
                    else
                    {
                        maintenanceOrganizationUniqueID = Model.FormInput.MaintenanceOrganizationUniqueID;

                        var organization = db.ORGANIZATION.First(x => x.UNIQUEID == Model.FormInput.MaintenanceOrganizationUniqueID);

                        if (!string.IsNullOrEmpty(organization.MANAGERUSERID))
                        {
                            jobManagerID = organization.MANAGERUSERID;
                        }
                        else
                        {
                            jobManagerID = string.Empty;

                            result.ReturnFailedMessage(string.Format("{0} {1} {2} {3}", Resources.Resource.Organization, organization.DESCRIPTION, Resources.Resource.NotSet, Resources.Resource.Manager));
                        }
                    }

                    if (!string.IsNullOrEmpty(jobManagerID))
                    {
                        var equipmentUniqueID = string.Empty;
                        var partUniqueID = string.Empty;

                        if (!string.IsNullOrEmpty(Model.FormInput.EquipmentUniqueID))
                        {
                            string[] t = Model.FormInput.EquipmentUniqueID.Split(Define.Seperators, StringSplitOptions.None);

                            equipmentUniqueID = t[0];
                            partUniqueID = t[1];
                        }

                        var vhnoPrefix = string.Format("R{0}", DateTime.Today.ToString("yyyyMM").Substring(2));

                        var seq = 1;

                        var temp = db.RFORM.Where(x => x.VHNO.StartsWith(vhnoPrefix)).OrderByDescending(x => x.VHNO).FirstOrDefault();

                        if (temp != null)
                        {
                            seq = int.Parse(temp.VHNO.Substring(5)) + 1;
                        }

                        var vhno = string.Format("{0}{1}", vhnoPrefix, seq.ToString().PadLeft(4, '0'));

                        var uniqueID = Guid.NewGuid().ToString();

                        var createTime = DateTime.Now;

                        var equipment = db.EQUIPMENT.FirstOrDefault(x => x.UNIQUEID == equipmentUniqueID);
                        var part = db.EQUIPMENTPART.FirstOrDefault(x => x.UNIQUEID == partUniqueID);

                        var form = new RFORM()
                        {
                            UNIQUEID = uniqueID,
                            STATUS = "0",
                            VHNO = vhno,
                            ORGANIZATIONUNIQUEID = Model.OrganizationUniqueID,
                            PMORGANIZATIONUNIQUEID = maintenanceOrganizationUniqueID,
                            EQUIPMENTUNIQUEID = equipmentUniqueID,
                            PARTUNIQUEID = partUniqueID,
                            RFORMTYPEUNIQUEID = Model.FormInput.RepairFormTypeUniqueID,
                            SUBJECT = Model.FormInput.Subject,
                            DESCRIPTION = Model.FormInput.Description,
                            CREATEUSERID = UserID,
                            CREATETIME = createTime,
                            EQUIPMENTID = equipment != null ? equipment.ID : string.Empty,
                            EQUIPMENTNAME = equipment != null ? equipment.NAME : string.Empty,
                            PARTDESCRIPTION = part != null ? part.DESCRIPTION : string.Empty
                        };

                        if (Model.FormInput.IsRepairBySelf)
                        {
                            form.STATUS = "4";
                            form.MANAGERJOBTIME = createTime;
                            form.JOBREFUSEREASON = string.Empty;
                            form.TAKEJOBTIME = createTime;
                            form.TAKEJOBUSERID = UserID;
                            form.ESTBEGINDATE = Model.FormInput.EstBeginDate;
                            form.ESTENDDATE = Model.FormInput.EstEndDate;

                            db.RFORMJOBUSER.Add(new RFORMJOBUSER()
                            {
                                RFORMUNIQUEID = uniqueID,
                                USERID = UserID
                            });
                        }

                        db.RFORM.Add(form);

                        db.ABNORMALRFORM.Add(new ABNORMALRFORM()
                        {
                            ABNORMALUNIQUEID = Model.AbnormalUniqueID,
                            RFORMUNIQUEID = uniqueID
                        });

                        db.SaveChanges();

                        result.ReturnSuccessMessage(string.Format("{0} {1} {2}", Resources.Resource.Create, Resources.Resource.RepairForm, Resources.Resource.Success));
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

        public static List<RepairFormModel> GetRepairFormList(string UniqueID)
        {
            var itemList = new List<RepairFormModel>();

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var repairFormList = (from x in db.ABNORMALRFORM
                                          join f in db.RFORM
                                          on x.RFORMUNIQUEID equals f.UNIQUEID
                                          join t in db.RFORMTYPE
                                          on f.RFORMTYPEUNIQUEID equals t.UNIQUEID
                                          join e in db.EQUIPMENT
                                          on f.EQUIPMENTUNIQUEID equals e.UNIQUEID into tmpEquipment
                                          from e in tmpEquipment.DefaultIfEmpty()
                                          join p in db.EQUIPMENTPART
                                          on f.PARTUNIQUEID equals p.UNIQUEID into tmpPart
                                          from p in tmpPart.DefaultIfEmpty()
                                          where x.ABNORMALUNIQUEID == UniqueID
                                          select new
                                          {
                                              OrganizationUniqueID = f.ORGANIZATIONUNIQUEID,
                                              MaintenanceOrganizationUniqueID = f.PMORGANIZATIONUNIQUEID,
                                              UniqueID = f.UNIQUEID,
                                              f.VHNO,
                                              Status = f.STATUS,
                                              EstBeginDate = f.ESTBEGINDATE,
                                              RepairFormType = t.DESCRIPTION,
                                              EstEndDate = f.ESTENDDATE,
                                              Subject = f.SUBJECT,
                                              EquipmentID = e != null ? e.ID : "",
                                              EquipmentName = e != null ? e.NAME : "",
                                              PartDescription = p != null ? p.DESCRIPTION : ""
                                          }).ToList();

                    foreach (var form in repairFormList)
                    {
                        var repairFormModel = new RepairFormModel()
                        {
                            UniqueID = form.UniqueID,
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(form.OrganizationUniqueID),
                            MaintenanceOrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(form.MaintenanceOrganizationUniqueID),
                            VHNO = form.VHNO,
                            Subject = form.Subject,
                            EstBeginDate = form.EstBeginDate,
                            EstEndDate = form.EstEndDate,
                            Status = form.Status,
                            EquipmentID = form.EquipmentID,
                            EquipmentName = form.EquipmentName,
                            PartDescription = form.PartDescription,
                            RepairFormType = form.RepairFormType
                        };

                        var flow = db.RFORMFLOW.FirstOrDefault(x => x.RFORMUNIQUEID == form.UniqueID);

                        if (flow != null)
                        {
                            repairFormModel.IsClosed = flow.ISCLOSED == "Y";
                        }
                        else
                        {
                            repairFormModel.IsClosed = false;
                        }

                        itemList.Add(repairFormModel);
                    }
                }
            }
            catch (Exception ex)
            {
                itemList = null;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return itemList;
        }
    }
}
