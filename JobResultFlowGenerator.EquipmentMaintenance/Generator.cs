using DataAccess.EquipmentMaintenance;
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

namespace JobResultFlowGenerator.EquipmentMaintenance
{
    public class Generator : IDisposable
    {
        private DateTime BaseTime = DateTime.Now.AddHours(-2);

        public void Generate()
        {
            var total = 0;
            var success = 0;
            var failed = 0;

            using (EDbEntities db = new EDbEntities())
            {
                var jobResultList = (from x in db.JobResult
                                     join y in db.JobResultFlow
                                     on x.UniqueID equals y.JobResultUniqueID into tmpFlow
                                     from y in tmpFlow.DefaultIfEmpty()
                                     where x.IsNeedVerify && (x.IsCompleted || BaseTime > x.JobEndTime) && y == null
                                     select x).ToList();

                total = jobResultList.Count;

                foreach (var jobResult in jobResultList)
                {
                    if (Generate(jobResult))
                    {
                        success++;
                    }
                    else
                    {
                        failed++;
                    }
                }

                Logger.Log(string.Format("{0} Job to Generate, {1} Success, {2} Failed", total, success, failed));
            }

            using (EDbEntities db = new EDbEntities())
            {
                var jobResultList = db.JobResult.Where(x => DateTime.Compare(DateTime.Now, x.JobEndTime) >= 0 && x.CompleteRate.StartsWith(Resources.Resource.Processing)).ToList();

                foreach (var jobResult in jobResultList)
                {
                    JobResultDataAccessor.Refresh(jobResult.UniqueID, jobResult.JobUniqueID, jobResult.BeginDate, jobResult.EndDate, jobResult.OverTimeReason, jobResult.OverTimeUser, jobResult.UnPatrolReason, jobResult.UnPatrolUser);
                }
            }
        }

        private bool Generate(JobResult JobResult)
        {
            bool result = false;

            try
            {
                using (EDbEntities edb = new EDbEntities())
                {
                    var route = (from j in edb.Job
                                 join r in edb.Route
                                 on j.RouteUniqueID equals r.UniqueID
                                 where j.UniqueID == JobResult.JobUniqueID
                                 select r).First();

                    using (DbEntities db = new DbEntities())
                    {
                        var nextVerifyOrganization = (from f in db.Flow
                                                      join x in db.FlowForm
                                                      on f.UniqueID equals x.FlowUniqueID
                                                      join v in db.FlowVerifyOrganization
                                                      on f.UniqueID equals v.FlowUniqueID
                                                      join o in db.Organization
                                                      on v.OrganizationUniqueID equals o.UniqueID
                                                      where f.OrganizationUniqueID == route.OrganizationUniqueID && x.Form == Define.EnumForm.EquipmentPatrolResult.ToString()
                                                      select new
                                                      {
                                                          o.UniqueID,
                                                          o.Description,
                                                          o.ManagerUserID,
                                                          v.Seq
                                                      }).OrderBy(x => x.Seq).FirstOrDefault();

                        //有設定簽核流程
                        if (nextVerifyOrganization != null)
                        {
                            var organizationManagers = (from x in db.OrganizationManager
                                                        join u in db.User
                                                        on x.UserID equals u.ID
                                                        where x.OrganizationUniqueID == nextVerifyOrganization.UniqueID
                                                        select u).ToList();

                            //簽核組織有設定主管
                            //if (!string.IsNullOrEmpty(nextVerifyOrganization.ManagerUserID))
                            if (organizationManagers.Count > 0)
                            {
                                edb.JobResultFlow.Add(new JobResultFlow()
                                {
                                    JobResultUniqueID = JobResult.UniqueID,
                                    CurrentSeq = 1,
                                    IsClosed = false
                                });

                                //var user = db.User.First(x => x.ID == nextVerifyOrganization.ManagerUserID);

                                edb.JobResultFlowLog.Add(new JobResultFlowLog()
                                {
                                    JobResultUniqueID = JobResult.UniqueID,
                                    Seq = 1,
                                    FlowSeq = nextVerifyOrganization.Seq,
                                    OrganizationUniqueID=nextVerifyOrganization.UniqueID,
                                    //UserID = nextVerifyOrganization.ManagerUserID,
                                    //UserName = user.Name,
                                    NotifyTime = DateTime.Now
                                });

                                if (Config.HaveMailSetting)
                                {
                                    foreach (var user in organizationManagers)
                                    {
                                        SendVerifyMail(user, JobResult);
                                    }
                                }

                                edb.SaveChanges();

                                result = true;
                            }
                            else
                            {
                                result = false;
                            }
                        }
                        else
                        {
                            var organization = db.Organization.First(x => x.UniqueID == route.OrganizationUniqueID);

                            var organizationManagers = (from x in db.OrganizationManager
                                                        join u in db.User
                                                        on x.UserID equals u.ID
                                                        where x.OrganizationUniqueID == organization.UniqueID
                                                        select u).ToList();

                            //簽核組織有設定主管
                            //if (!string.IsNullOrEmpty(nextVerifyOrganization.ManagerUserID))
                            if (organizationManagers.Count > 0)
                            {
                                edb.JobResultFlow.Add(new JobResultFlow()
                                {
                                    JobResultUniqueID = JobResult.UniqueID,
                                    CurrentSeq = 1,
                                    IsClosed = false
                                });

                                //var user = db.User.First(x => x.ID == organization.ManagerUserID);

                                edb.JobResultFlowLog.Add(new JobResultFlowLog()
                                {
                                    JobResultUniqueID = JobResult.UniqueID,
                                    Seq = 1,
                                    FlowSeq = 0,
                                    OrganizationUniqueID = organization.UniqueID,
                                    //UserID = organization.ManagerUserID,
                                    //UserName = user.Name,
                                    NotifyTime = DateTime.Now
                                });

                                if (Config.HaveMailSetting)
                                {
                                    foreach (var user in organizationManagers)
                                    {
                                        SendVerifyMail(user, JobResult);
                                    }
                                }

                                edb.SaveChanges();

                                result = true;
                            }
                            else
                            {
                                result = false;
                            }
                        }
                    }   
                }
            }
            catch (Exception ex)
            {
                result = false;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return result;
        }

        private static void SendVerifyMail(User Account, JobResult JobResult)
        {
            try
            {
                var beginTime = JobResult.BeginDate;

                if (!string.IsNullOrEmpty(JobResult.BeginTime))
                {
                    beginTime = string.Format("{0} {1}", JobResult.BeginDate, JobResult.BeginTime);
                }

                var endTime = JobResult.EndDate;

                if (!string.IsNullOrEmpty(JobResult.EndTime))
                {
                    endTime = string.Format("{0} {1}", JobResult.EndDate, JobResult.EndTime);
                }

                var checkDate = string.Empty;

                if (beginTime == endTime)
                {
                    checkDate = beginTime;
                }
                else
                {
                    checkDate = string.Format("{0}~{1}", beginTime, endTime);
                }

                var subject = string.Format("[巡檢結果簽核通知][{0}]{1}({2})", JobResult.OrganizationDescription, JobResult.Description, checkDate);

                var th = "<th style=\"width:150px;border:1px solid #333;padding:8px;text-align:right;color:#707070;background: #F4F4F4;\">{0}</th>";
                var td = "<td style=\"width:400px;border:1px solid #333;padding:8px;color:#707070;\">{0}</td>";

                var sb = new StringBuilder();

                sb.Append("<table style=\"1px solid #ddd;font-size:13px;border-collapse:collapse;\">");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "組織"));
                sb.Append(string.Format(td, JobResult.OrganizationDescription));
                sb.Append("</tr>");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "巡檢路線派工"));
                sb.Append(string.Format(td, JobResult.Description));
                sb.Append("</tr>");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "派工開始日期"));
                sb.Append(string.Format(td, JobResult.BeginDate));
                sb.Append("</tr>");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "派工開始時間"));
                sb.Append(string.Format(td, JobResult.BeginTime));
                sb.Append("</tr>");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "派工結束日期"));
                sb.Append(string.Format(td, JobResult.EndDate));
                sb.Append("</tr>");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "派工結束時間"));
                sb.Append(string.Format(td, JobResult.EndTime));
                sb.Append("</tr>");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "完成率"));
                sb.Append(string.Format(td, JobResult.CompleteRate));
                sb.Append("</tr>");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "檢查人員"));
                sb.Append(string.Format(td, JobResult.CheckUsers));
                sb.Append("</tr>");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "詳細資料"));
                sb.Append(string.Format(td, string.Format("<a href=\"http://172.28.128.165/FEM/Home/Index?ReturnUrl=http://172.28.128.165/FEM/zh-tw/EquipmentMaintenance/ResultVerify/Index?JobResultUniqueID=" + JobResult.UniqueID + "\">連結</a>")));
                sb.Append("</tr>");

                sb.Append("</table>");

                MailHelper.SendMail(new MailAddress(Account.Email, Account.Name), subject, sb.ToString());
            }
            catch (Exception ex)
            {
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

        ~Generator()
        {
            Dispose(false);
        }

        #endregion
    }
}
