using AbnormalSummaryReport.EquipmentMaintenance.Models;
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace AbnormalSummaryReport.EquipmentMaintenance
{
    public class ReportHelper : IDisposable
    {
        public void Send()
        {
            using (EDbEntities db = new EDbEntities())
            {
                var b = new DateTime();
                var begin = string.Empty;
                var e = new DateTime();
                var end = string.Empty;

                if (DateTime.Now.Hour == 5)
                {
                    var yesterday = DateTime.Today.AddDays(-1);
                    var today = DateTime.Today;

                    b = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 17, 0, 0);
                    e = new DateTime(today.Year, today.Month, today.Day, 5, 0, 0);
                }

                if (DateTime.Now.Hour == 17)
                {
                    var yesterday = DateTime.Today.AddDays(-1);
                    var today = DateTime.Today;

                    b = new DateTime(today.Year, today.Month, today.Day, 5, 0, 0);
                    e = new DateTime(today.Year, today.Month, today.Day, 17, 0, 0);
                }
                
                begin = b.ToString("yyyyMMddHHmmss");
                end = e.ToString("yyyyMMddHHmmss");

                Logger.Log(string.Format("Begin:{0}", begin));
                Logger.Log(string.Format("End:{0}", end));

                var jobResultList = (from x in db.JobResult
                                     join j in db.Job
                                     on x.JobUniqueID equals j.UniqueID
                                     join r in db.Route
                                     on j.RouteUniqueID equals r.UniqueID
                                     where string.Compare(x.BeginDate + x.BeginTime + "00", begin) > 0 && DateTime.Compare(x.JobEndTime, e) <= 0
                                     select new
                                     {
                                         JobResultUniqueID = x.UniqueID,
                                         r.OrganizationUniqueID,
                                         JobUniqueID = j.UniqueID,
                                         JobDescription = j.Description,
                                         RouteUniqueID = r.UniqueID,
                                         RouteID = r.ID,
                                         RouteName = r.Name,
                                         x.IsCompleted,
                                         x.CompleteRate,
                                         x.ArriveStatus,
                                         x.BeginDate,
                                         x.BeginTime,
                                         x.EndDate,
                                         x.EndTime,
                                         x.UnPatrolReason,
                                         x.OverTimeReason
                                     }).ToList();

                Logger.Log(string.Format("JobResult Count:{0}", jobResultList.Count));

                var mailContentList = new List<MailContent>();

                var routeList = jobResultList.Select(x => new
                {
                    OrganizationUniqueID = x.OrganizationUniqueID,
                    RouteUniqueID = x.RouteUniqueID,
                    RouteID = x.RouteID,
                    RouteName = x.RouteName
                }).Distinct().ToList();

                Logger.Log(string.Format("Route Count:{0}", routeList.Count));

                foreach (var route in routeList)
                {
                    var jobList = jobResultList.Where(x => x.RouteUniqueID == route.RouteUniqueID).ToList();
                    var jobResultUniqueIDList = jobList.Select(x => x.JobResultUniqueID).ToList();

                    mailContentList.Add(new MailContent()
                    {
                        OrganizationUniqueID = route.OrganizationUniqueID,
                        RouteID = route.RouteID,
                        RouteName = route.RouteName,
                        RouteUniqueID = route.RouteUniqueID,
                        BeginTime = b,
                        EndTime = e,
                        JobAbnormalList = jobList.Where(x => (!x.IsCompleted || !(x.ArriveStatus == "正常" || x.ArriveStatus == "-"))).Select(x => new JobAbnormal
                        {
                            JobDescription = x.JobDescription,
                            BeginDate = x.BeginDate,
                            BeginTime = x.BeginTime,
                            EndDate = x.EndDate,
                            EndTime = x.EndTime,
                            CompleteRate = x.CompleteRate,
                            UnPatrolReason = x.UnPatrolReason,
                            ArriveStatus = x.ArriveStatus,
                            OverTimeReason = x.OverTimeReason
                        }).ToList(),
                        ControlPointAbnormalList = db.ArriveRecord.Where(x => jobResultUniqueIDList.Contains(x.JobResultUniqueID) && !string.IsNullOrEmpty(x.UnRFIDReasonUniqueID)).Select(x => new ControlPointAbnormal
                        {
                            JobDescription = x.JobDescription,
                            ControlPointID = x.ControlPointID,
                            ControlPointDescription = x.ControlPointDescription,
                            ArriveDate = x.ArriveDate,
                            ArriveTime = x.ArriveTime,
                            UserID = x.UserID,
                            UserName = x.UserName,
                            UnRFIDReasonUniqueID = x.UnRFIDReasonUniqueID,
                            UnRFIDReasonDescription = x.UnRFIDReasonDescription,
                            UnRFIDReasonRemark = x.UnRFIDReasonRemark
                        }).ToList(),
                        CheckItemAbnormalList = (from x in db.AbnormalCheckResult
                                                 join c in db.CheckResult
                                                 on x.CheckResultUniqueID equals c.UniqueID
                                                 join a in db.ArriveRecord
                                                 on c.ArriveRecordUniqueID equals a.UniqueID
                                                 where jobResultUniqueIDList.Contains(a.JobResultUniqueID)
                                                 select new CheckItemAbnormal
                                                 {
                                                     JobDescription = c.JobDescription,
                                                     ControlPointID = c.ControlPointID,
                                                     ControlPointDescription = c.ControlPointDescription,
                                                     EquipmentID = c.EquipmentID,
                                                     EquipmentName = c.EquipmentName,
                                                     PartDescription = c.PartDescription,
                                                     CheckItemID = c.CheckItemID,
                                                     CheckItemDescription = c.CheckItemDescription,
                                                     CheckDate = c.CheckDate,
                                                     CheckTime = c.CheckTime,
                                                     IsAbnormal = c.IsAbnormal,
                                                     IsAlert = c.IsAlert,
                                                     LowerLimit = c.LowerLimit,
                                                     LowerAlertLimit = c.LowerAlertLimit,
                                                     UpperAlertLimit = c.UpperAlertLimit,
                                                     UpperLimit = c.UpperLimit,
                                                     Unit = c.Unit,
                                                     Result = c.Result
                                                 }).ToList()
                    });
                }

                foreach (var mailContent in mailContentList)
                {
                    if (mailContent.HaveAbnormal)
                    {
                        SendPatrolAbnormalReport(mailContent);
                    }
                }
            }
        }

        private void SendPatrolAbnormalReport(MailContent MailContent)
        {
            try
            {
                Logger.Log(string.Format("Abnormal Subject:{0}", MailContent.Subject));

                var header = "<th colspan=\"{0}\" style=\"border:1px solid #333;padding:8px;text-align:center;color:#707070;background: #F4F4F4;\">{1}</th>";
                var th = "<th style=\"border:1px solid #333;padding:8px;text-align:center;color:#707070;background: #F4F4F4;\">{0}</th>";
                var td = "<td style=\"border:1px solid #333;padding:8px;color:#707070;\">{0}</td>";

                var mailAddressList = new List<MailAddress>();

                var managerList = new List<string>();

                using (EDbEntities db = new EDbEntities())
                {
                    managerList = db.RouteManager.Where(x => x.RouteUniqueID == MailContent.RouteUniqueID).Select(x => x.UserID).ToList();
                }

                using (DbEntities db = new DbEntities())
                {
                    var organization = db.Organization.FirstOrDefault(x => x.UniqueID == MailContent.OrganizationUniqueID);

                    if (organization != null && !string.IsNullOrEmpty(organization.ManagerUserID))
                    {
                        managerList.Add(organization.ManagerUserID);
                    }

                    foreach (var manager in managerList)
                    {
                        var user = db.User.FirstOrDefault(x => x.ID == manager);

                        if (user != null && !string.IsNullOrEmpty(user.Email))
                        {
                            mailAddressList.Add(new MailAddress(user.Email, user.Name));
                        }
                    }
                }

                if (mailAddressList.Count > 0)
                {
                    var sb = new StringBuilder();

                    if (MailContent.JobAbnormalList.Count > 0)
                    {
                        sb.Append("<table style=\"border:1px solid #E5E5E5;font-size:13px;border-collapse:collapse;\">");

                        sb.Append("<tr>");
                        sb.Append(string.Format(header, 7, string.Format("{0}{1}{2}", Resources.Resource.Job, "執行", Resources.Resource.Abnormal)));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, Resources.Resource.Job));
                        sb.Append(string.Format(th, Resources.Resource.BeginTime));
                        sb.Append(string.Format(th, Resources.Resource.EndTime));
                        sb.Append(string.Format(th, Resources.Resource.CompleteRate));
                        sb.Append(string.Format(th, Resources.Resource.UnPatrolReason));
                        sb.Append(string.Format(th, Resources.Resource.ArriveStatus));
                        sb.Append(string.Format(th, Resources.Resource.OverTimeReason));
                        sb.Append("</tr>");

                        foreach (var a in MailContent.JobAbnormalList)
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(td, a.JobDescription));
                            sb.Append(string.Format(td, a.Begin));
                            sb.Append(string.Format(td, a.End));
                            sb.Append(string.Format(td, a.CompleteRate));
                            sb.Append(string.Format(td, a.UnPatrolReason));
                            sb.Append(string.Format(td, a.ArriveStatus));
                            sb.Append(string.Format(td, a.OverTimeReason));
                            sb.Append("</tr>");
                        }

                        sb.Append("</table>");
                    }

                    if (MailContent.ControlPointAbnormalList.Count > 0)
                    {
                        sb.Append("<table style=\"border:1px solid #E5E5E5;font-size:13px;border-collapse:collapse;\">");

                        sb.Append("<tr>");
                        sb.Append(string.Format(header, 5, string.Format("{0}{1}", Resources.Resource.UnRFID, Resources.Resource.Abnormal)));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, Resources.Resource.Job));
                        sb.Append(string.Format(th, Resources.Resource.ControlPoint));
                        sb.Append(string.Format(th, Resources.Resource.ArriveTime));
                        sb.Append(string.Format(th, Resources.Resource.ArriveUser));
                        sb.Append(string.Format(th, Resources.Resource.UnRFIDReason));
                        sb.Append("</tr>");

                        foreach (var a in MailContent.ControlPointAbnormalList)
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(td, a.JobDescription));
                            sb.Append(string.Format(td, a.ControlPoint));
                            sb.Append(string.Format(td, a.ArriveDateTime));
                            sb.Append(string.Format(td, a.User));
                            sb.Append(string.Format(td, a.UnRFIDReason));
                            sb.Append("</tr>");
                        }

                        sb.Append("</table>");
                    }

                    if (MailContent.CheckItemAbnormalList.Count > 0)
                    {
                        sb.Append("<table style=\"border:1px solid #E5E5E5;font-size:13px;border-collapse:collapse;\">");

                        sb.Append("<tr>");
                        sb.Append(string.Format(header, 12, string.Format("{0}{1}", Resources.Resource.CheckItem, Resources.Resource.Abnormal)));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, Resources.Resource.Abnormal));
                        sb.Append(string.Format(th, Resources.Resource.Job));
                        sb.Append(string.Format(th, Resources.Resource.ControlPoint));
                        sb.Append(string.Format(th, Resources.Resource.Equipment));
                        sb.Append(string.Format(th, Resources.Resource.CheckItem));
                        sb.Append(string.Format(th, Resources.Resource.CheckTime));
                        sb.Append(string.Format(th, Resources.Resource.CheckResult));
                        sb.Append(string.Format(th, Resources.Resource.LowerLimit));
                        sb.Append(string.Format(th, Resources.Resource.LowerAlertLimit));
                        sb.Append(string.Format(th, Resources.Resource.UpperAlertLimit));
                        sb.Append(string.Format(th, Resources.Resource.UpperLimit));
                        sb.Append(string.Format(th, Resources.Resource.Unit));
                        sb.Append("</tr>");

                        foreach (var a in MailContent.CheckItemAbnormalList)
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(td, a.AbnormalStatus));
                            sb.Append(string.Format(td, a.JobDescription));
                            sb.Append(string.Format(td, a.ControlPoint));
                            sb.Append(string.Format(td, a.Equipment));
                            sb.Append(string.Format(td, a.CheckItem));
                            sb.Append(string.Format(td, a.CheckDateTime));
                            sb.Append(string.Format(td, a.Result));
                            sb.Append(string.Format(td, a.LowerLimit));
                            sb.Append(string.Format(td, a.LowerAlertLimit));
                            sb.Append(string.Format(td, a.UpperAlertLimit));
                            sb.Append(string.Format(td, a.UpperLimit));
                            sb.Append(string.Format(td, a.Unit));
                            sb.Append("</tr>");
                        }

                        sb.Append("</table>");
                    }

                    Logger.Log(string.Format("Mail Body:{0}", sb.ToString()));

                    MailHelper.SendMail(mailAddressList, MailContent.Subject, sb.ToString());
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), string.Format("Send Failed:{0}", MailContent.Subject));
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
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

        ~ReportHelper()
        {
            Dispose(false);
        }

        #endregion
    }
}
