using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using PSI.AbnormalSummaryMail.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace PSI.AbnormalSummaryMail
{
    public class ReportHelper : IDisposable
    {
        private string header = "<th colspan=\"{0}\" style=\"border:1px solid #333;padding:8px;text-align:center;color:#707070;background: #F4F4F4;\">{1}</th>";
        private string th = "<th style=\"border:1px solid #333;padding:8px;text-align:center;color:#707070;background: #F4F4F4;\">{0}</th>";
        private string td = "<td style=\"border:1px solid #333;padding:8px;color:#707070;\">{0}</td>";

        private DateTime Now = DateTime.Now;

        private DateTime SummaryBeginTime
        {
            get
            {
                return SummaryEndTime.AddMinutes(-10);
            }
        }

        private DateTime SummaryEndTime
        {
            get
            {
                var min = int.Parse(string.Format("{0}0", Now.Minute.ToString().PadLeft(2, '0').Substring(0, 1)));

                return new DateTime(Now.Year, Now.Month, Now.Day, Now.Hour, min, 0);
            }
        }

        private DateTime NotifyBeginTime
        {
            get
            {
                return SummaryBeginTime.AddHours(1);
            }
        }

        private DateTime NotifyEndTime
        {
            get
            {
                return SummaryEndTime.AddHours(1);
            }
        }

        /// <summary>
        /// 00:01:00開始
        /// 每10分鐘執行一次
        /// </summary>
        public void Send()
        {
            try
            {
                Logger.Log(string.Format("Send Summary Report between 【0】 ~ 【1】", DateTimeHelper.DateTime2DateTimeStringWithSeperator(SummaryBeginTime), DateTimeHelper.DateTime2DateTimeStringWithSeperator(SummaryEndTime)));
                Logger.Log(string.Format("Send Notify Report between 【0】 ~ 【1】", DateTimeHelper.DateTime2DateTimeStringWithSeperator(NotifyBeginTime), DateTimeHelper.DateTime2DateTimeStringWithSeperator(NotifyEndTime)));

                using (EDbEntities db = new EDbEntities())
                {
                    var jobResultList = db.JobResult.Where(x => DateTime.Compare(x.JobEndTime, SummaryEndTime) == 0).Select(x => x.UniqueID).ToList();

                    SendSummaryMail(jobResultList);

                    jobResultList = db.JobResult.Where(x => DateTime.Compare(x.JobEndTime, NotifyEndTime) == 0).Select(x => x.UniqueID).ToList();

                    SendNotifyMail(jobResultList);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private void SendSummaryMail(List<string> JobResultList)
        {
            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var jobResultList = (from x in db.JobResult
                                         join j in db.Job
                                         on x.JobUniqueID equals j.UniqueID
                                         join r in db.Route
                                         on j.RouteUniqueID equals r.UniqueID
                                         where JobResultList.Contains(x.UniqueID)
                                         select new
                                         {
                                             JobResultUniqueID = x.UniqueID,
                                             r.OrganizationUniqueID,
                                             x.OrganizationDescription,
                                             JobDescription = j.Description,
                                             RouteUniqueID = r.UniqueID,
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

                    var mailContentList = new List<MailContent>();

                    var routeList = jobResultList.Select(x => new
                    {
                        x.OrganizationUniqueID,
                        x.OrganizationDescription,
                        x.RouteUniqueID,
                        x.RouteName
                    }).Distinct().ToList();

                    foreach (var route in routeList)
                    {
                        var jobList = jobResultList.Where(x => x.RouteUniqueID == route.RouteUniqueID).ToList();
                        var jobResultUniqueIDList = jobList.Select(x => x.JobResultUniqueID).ToList();

                        var mailContent = new MailContent()
                        {
                            OrganizationUniqueID = route.OrganizationUniqueID,
                            RouteUniqueID = route.RouteUniqueID,
                            JobAbnormalList = jobList.Where(x => (!x.IsCompleted || !(x.ArriveStatus == "正常" || x.ArriveStatus == "-"))).Select(x => new JobAbnormal
                            {
                                OrganizationDescription = route.OrganizationDescription,
                                RouteName = route.RouteName,
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
                                OrganizationDescription = route.OrganizationDescription,
                                RouteName = route.RouteName,
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
                                                         OrganizationDescription = route.OrganizationDescription,
                                                         RouteName = route.RouteName,
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
                                                         Result = c.Result,
                                                         PhotoList = db.CheckResultPhoto.Where(p => p.CheckResultUniqueID == c.UniqueID).Select(p => new CheckResultPhotoModel
                                                         {
                                                             UniqueID = p.CheckResultUniqueID,
                                                             Seq = p.Seq,
                                                             Extension = p.Extension
                                                         }).ToList()
                                                     }).ToList()
                        };

                        if (mailContent.HaveAbnormal)
                        {
                            mailContentList.Add(mailContent);
                        }
                    }

                    SendSummaryMail(mailContentList);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private void SendSummaryMail(List<MailContent> MailContentList)
        {
            var userMailContentList = new List<SummaryUserMailContent>();

            using (DbEntities db = new DbEntities())
            {
                using (EDbEntities edb = new EDbEntities())
                {
                    foreach (var mailContent in MailContentList)
                    {
                        var organization = db.Organization.FirstOrDefault(x => x.UniqueID == mailContent.OrganizationUniqueID);

                        if (organization != null && !string.IsNullOrEmpty(organization.ManagerUserID))
                        {
                            var manager = db.User.FirstOrDefault(x => x.ID == organization.ManagerUserID);

                            if (manager != null && !string.IsNullOrEmpty(manager.Email))
                            {
                                var userMailContent = userMailContentList.FirstOrDefault(x => x.UserID == manager.ID);

                                if (userMailContent != null)
                                {
                                    userMailContent.JobAbnormalList.AddRange(mailContent.JobAbnormalList);
                                    userMailContent.ControlPointAbnormalList.AddRange(mailContent.ControlPointAbnormalList);
                                    userMailContent.CheckItemAbnormalList.AddRange(mailContent.CheckItemAbnormalList);
                                }
                                else
                                {
                                    userMailContentList.Add(new SummaryUserMailContent()
                                    {
                                        UserID = manager.ID,
                                        UserName = manager.Name,
                                        Email = manager.Email,
                                        EndTime = SummaryEndTime,
                                        JobAbnormalList = mailContent.JobAbnormalList,
                                        ControlPointAbnormalList = mailContent.ControlPointAbnormalList,
                                        CheckItemAbnormalList = mailContent.CheckItemAbnormalList
                                    });
                                }
                            }
                        }

                        var routeManagerList = edb.RouteManager.Where(x => x.RouteUniqueID == mailContent.RouteUniqueID).Select(x => x.UserID).ToList();

                        foreach (var routeManager in routeManagerList)
                        {
                            var manager = db.User.FirstOrDefault(x => x.ID == routeManager);

                            if (manager != null && !string.IsNullOrEmpty(manager.Email))
                            {
                                var userMailContent = userMailContentList.FirstOrDefault(x => x.UserID == manager.ID);

                                if (userMailContent != null)
                                {
                                    userMailContent.JobAbnormalList.AddRange(mailContent.JobAbnormalList);
                                    userMailContent.ControlPointAbnormalList.AddRange(mailContent.ControlPointAbnormalList);
                                    userMailContent.CheckItemAbnormalList.AddRange(mailContent.CheckItemAbnormalList);
                                }
                                else
                                {
                                    userMailContentList.Add(new SummaryUserMailContent()
                                    {
                                        UserID = manager.ID,
                                        UserName = manager.Name,
                                        Email = manager.Email,
                                        EndTime = SummaryEndTime,
                                        JobAbnormalList = mailContent.JobAbnormalList,
                                        ControlPointAbnormalList = mailContent.ControlPointAbnormalList,
                                        CheckItemAbnormalList = mailContent.CheckItemAbnormalList
                                    });
                                }
                            }
                        }
                    }
                }
            }

            SendSummaryMail(userMailContentList);
        }

        private void SendSummaryMail(List<SummaryUserMailContent> UserMailContentList)
        {
            foreach (var userMailContent in UserMailContentList)
            {
                SendSummaryMail(userMailContent);
            }
        }

        private void SendSummaryMail(SummaryUserMailContent UserMailContent)
        {
            try
            {
                var sb = new StringBuilder();

                if (UserMailContent.JobAbnormalList.Count > 0)
                {
                    sb.Append("<table style=\"border:1px solid #E5E5E5;font-size:13px;border-collapse:collapse;\">");

                    sb.Append("<tr>");
                    sb.Append(string.Format(header, 9, string.Format("{0}{1}{2}", Resources.Resource.Job, "執行", Resources.Resource.Abnormal)));
                    sb.Append("</tr>");

                    sb.Append("<tr>");
                    sb.Append(string.Format(th, Resources.Resource.Organization));
                    sb.Append(string.Format(th, Resources.Resource.Route));
                    sb.Append(string.Format(th, Resources.Resource.Job));
                    sb.Append(string.Format(th, Resources.Resource.BeginTime));
                    sb.Append(string.Format(th, Resources.Resource.EndTime));
                    sb.Append(string.Format(th, Resources.Resource.CompleteRate));
                    sb.Append(string.Format(th, Resources.Resource.UnPatrolReason));
                    sb.Append(string.Format(th, Resources.Resource.ArriveStatus));
                    sb.Append(string.Format(th, Resources.Resource.OverTimeReason));
                    sb.Append("</tr>");

                    var jobAbnormalList = UserMailContent.JobAbnormalList.Select(x => new
                    {
                        x.OrganizationDescription,
                        x.RouteName,
                        x.JobDescription,
                        x.Begin,
                        x.End,
                        x.CompleteRate,
                        x.UnPatrolReason,
                        x.ArriveStatus,
                        x.OverTimeReason
                    }).Distinct().ToList();

                    foreach (var a in jobAbnormalList)
                    {
                        sb.Append("<tr>");
                        sb.Append(string.Format(td, a.OrganizationDescription));
                        sb.Append(string.Format(td, a.RouteName));
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

                if (UserMailContent.ControlPointAbnormalList.Count > 0)
                {
                    sb.Append("<table style=\"border:1px solid #E5E5E5;font-size:13px;border-collapse:collapse;\">");

                    sb.Append("<tr>");
                    sb.Append(string.Format(header, 7, string.Format("{0}{1}", Resources.Resource.UnRFID, Resources.Resource.Abnormal)));
                    sb.Append("</tr>");

                    sb.Append("<tr>");
                    sb.Append(string.Format(th, Resources.Resource.Organization));
                    sb.Append(string.Format(th, Resources.Resource.Route));
                    sb.Append(string.Format(th, Resources.Resource.Job));
                    sb.Append(string.Format(th, Resources.Resource.ControlPoint));
                    sb.Append(string.Format(th, Resources.Resource.ArriveTime));
                    sb.Append(string.Format(th, Resources.Resource.ArriveUser));
                    sb.Append(string.Format(th, Resources.Resource.UnRFIDReason));
                    sb.Append("</tr>");

                    var controlPointAbnormalList = UserMailContent.ControlPointAbnormalList.Select(x => new
                    {
                        x.OrganizationDescription,
                        x.RouteName,
                        x.JobDescription,
                        x.ControlPoint,
                        x.ArriveDateTime,
                        x.User,
                        x.UnRFIDReason
                    }).Distinct().ToList();

                    foreach (var a in controlPointAbnormalList)
                    {
                        sb.Append("<tr>");
                        sb.Append(string.Format(td, a.OrganizationDescription));
                        sb.Append(string.Format(td, a.RouteName));
                        sb.Append(string.Format(td, a.JobDescription));
                        sb.Append(string.Format(td, a.ControlPoint));
                        sb.Append(string.Format(td, a.ArriveDateTime));
                        sb.Append(string.Format(td, a.User));
                        sb.Append(string.Format(td, a.UnRFIDReason));
                        sb.Append("</tr>");
                    }

                    sb.Append("</table>");
                }

                if (UserMailContent.CheckItemAbnormalList.Count > 0)
                {
                    sb.Append("<table style=\"border:1px solid #E5E5E5;font-size:13px;border-collapse:collapse;\">");

                    sb.Append("<tr>");
                    sb.Append(string.Format(header, 15, string.Format("{0}{1}", Resources.Resource.CheckItem, Resources.Resource.Abnormal)));
                    sb.Append("</tr>");

                    sb.Append("<tr>");
                    sb.Append(string.Format(th, Resources.Resource.Abnormal));
                    sb.Append(string.Format(th, Resources.Resource.Photo));
                    sb.Append(string.Format(th, Resources.Resource.Organization));
                    sb.Append(string.Format(th, Resources.Resource.Route));
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

                    var checkItemAbnormalList = UserMailContent.CheckItemAbnormalList.Select(x => new
                    {
                        x.AbnormalStatus,
                        x.PhotoList,
                        x.OrganizationDescription,
                        x.RouteName,
                        x.JobDescription,
                        x.ControlPoint,
                        x.Equipment,
                        x.CheckItem,
                        x.CheckDateTime,
                        x.Result,
                        x.LowerLimit,
                        x.LowerAlertLimit,
                        x.UpperAlertLimit,
                        x.UpperLimit,
                        x.Unit
                    }).Distinct().ToList();

                    foreach (var a in checkItemAbnormalList)
                    {
                        sb.Append("<tr>");
                        sb.Append(string.Format(td, a.AbnormalStatus));

                        var photo = string.Empty;

                        foreach (var p in a.PhotoList)
                        {
                            photo += string.Format("<p><a href=\"{0}\"><img height=\"30\" width=\"30\" src=\"{0}\" /></a></p>", p.FullFileName);
                        }

                        sb.Append(string.Format(td, photo));

                        sb.Append(string.Format(td, a.OrganizationDescription));
                        sb.Append(string.Format(td, a.RouteName));
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

                Logger.Log(string.Format("SendMail「{0}」to「{1}」", UserMailContent.Subject, UserMailContent.Email));

                MailHelper.SendMail(new MailAddress(UserMailContent.Email, UserMailContent.UserName), UserMailContent.Subject, sb.ToString());
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), string.Format("Send Failed:{0}", UserMailContent.Subject));
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private void SendNotifyMail(List<string> JobResultList)
        {
            var userJobResultList = new List<NotifyUserJobResult>();

            using (DbEntities db = new DbEntities())
            {
                using (EDbEntities edb = new EDbEntities())
                {
                    var jobResultList = (from x in edb.JobResult
                                         join j in edb.Job
                                         on x.JobUniqueID equals j.UniqueID
                                         join r in edb.Route
                                         on j.RouteUniqueID equals r.UniqueID
                                         where JobResultList.Contains(x.UniqueID) && !x.IsCompleted
                                         select new
                                         {
                                             JobResult = x,
                                             RouteUniqueID = r.UniqueID,
                                             OrganizationUniqueID = r.OrganizationUniqueID
                                         }).Distinct().ToList();

                    foreach (var jobResult in jobResultList)
                    {
                        var organization = db.Organization.FirstOrDefault(x => x.UniqueID == jobResult.OrganizationUniqueID);

                        if (organization != null && !string.IsNullOrEmpty(organization.ManagerUserID))
                        {
                            var manager = db.User.FirstOrDefault(x => x.ID == organization.ManagerUserID);

                            if (manager != null && !string.IsNullOrEmpty(manager.Email))
                            {
                                var userJobResult = userJobResultList.FirstOrDefault(x => x.UserID == manager.ID);

                                if (userJobResult != null)
                                {
                                    userJobResult.JobResultList.Add(jobResult.JobResult);
                                }
                                else
                                {
                                    userJobResultList.Add(new NotifyUserJobResult()
                                    {
                                        UserID = manager.ID,
                                        UserName = manager.Name,
                                        Email = manager.Email,
                                        EndTime = SummaryEndTime,
                                        JobEndTime = NotifyEndTime,
                                        JobResultList = new List<JobResult>() { jobResult.JobResult }
                                    });
                                }
                            }
                        }

                        var routeManagerList = edb.RouteManager.Where(x => x.RouteUniqueID == jobResult.RouteUniqueID).Select(x => x.UserID).ToList();

                        foreach (var routeManager in routeManagerList)
                        {
                            var manager = db.User.FirstOrDefault(x => x.ID == routeManager);

                            if (manager != null && !string.IsNullOrEmpty(manager.Email))
                            {
                                var userJobResult = userJobResultList.FirstOrDefault(x => x.UserID == manager.ID);

                                if (userJobResult != null)
                                {
                                    userJobResult.JobResultList.Add(jobResult.JobResult);
                                }
                                else
                                {
                                    userJobResultList.Add(new NotifyUserJobResult()
                                    {
                                        UserID = manager.ID,
                                        UserName = manager.Name,
                                        Email = manager.Email,
                                        EndTime = SummaryEndTime,
                                        JobEndTime = NotifyEndTime,
                                        JobResultList = new List<JobResult>() { jobResult.JobResult }
                                    });
                                }
                            }
                        }
                    }
                }
            }

            SendNotifyMail(userJobResultList);
        }

        private void SendNotifyMail(List<NotifyUserJobResult> UserJobResultList)
        {
            foreach (var userJobResult in UserJobResultList)
            {
                SendNotifyMail(userJobResult);
            }
        }

        private void SendNotifyMail(NotifyUserJobResult UserJobResult)
        {
            try
            {
                var sb = new StringBuilder();

                sb.Append("<table style=\"border:1px solid #E5E5E5;font-size:13px;border-collapse:collapse;\">");

                sb.Append("<tr>");
                sb.Append(string.Format(th, Resources.Resource.Organization));
                sb.Append(string.Format(th, Resources.Resource.Job));
                sb.Append(string.Format(th, Resources.Resource.BeginTime));
                sb.Append(string.Format(th, Resources.Resource.EndTime));
                sb.Append(string.Format(th, Resources.Resource.CompleteRate));
                sb.Append("</tr>");

                var jobResultList = UserJobResult.JobResultList.Select(x => new
                {
                    x.OrganizationDescription,
                    x.Description,
                    x.BeginDate,
                    x.BeginTime,
                    x.EndDate,
                    x.EndTime,
                    x.CompleteRate
                }).Distinct().ToList();

                foreach (var jobResult in jobResultList)
                {
                    sb.Append("<tr>");
                    sb.Append(string.Format(td, jobResult.OrganizationDescription));
                    sb.Append(string.Format(td, jobResult.Description));
                    sb.Append(string.Format(td, DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTimeHelper.DateTimeString2DateTime(jobResult.BeginDate, jobResult.BeginTime))));
                    sb.Append(string.Format(td, DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTimeHelper.DateTimeString2DateTime(jobResult.EndDate, jobResult.EndTime))));
                    sb.Append(string.Format(td, jobResult.CompleteRate));
                    sb.Append("</tr>");
                }

                sb.Append("</table>");

                Logger.Log(string.Format("SendMail「{0}」to「{1}」", UserJobResult.Subject, UserJobResult.Email));

                MailHelper.SendMail(new MailAddress(UserJobResult.Email, UserJobResult.UserName), UserJobResult.Subject, sb.ToString());
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), string.Format("Send Failed:{0}", UserJobResult.Subject));
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
