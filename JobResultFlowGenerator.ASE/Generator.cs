using DataAccess.ASE;
using DbEntity.ASE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace JobResultFlowGenerator.ASE
{
    public class Generator : IDisposable
    {
        private DateTime BaseTime = DateTime.Now.AddHours(-2);

        public void Generate()
        {
            var total = 0;
            var success = 0;
            var failed = 0;

            using (ASEDbEntities db = new ASEDbEntities())
            {
                var jobResultList = (from x in db.JOBRESULT
                                     join y in db.JOBRESULTFLOW
                                     on x.UNIQUEID equals y.JOBRESULTUNIQUEID into tmpFlow
                                     from y in tmpFlow.DefaultIfEmpty()
                                     where x.ISNEEDVERIFY == "Y" && (x.ISCOMPLETED == "Y" || BaseTime > x.JOBENDTIME) && y == null
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

            using (ASEDbEntities db = new ASEDbEntities())
            {
                var jobResultList = db.JOBRESULT.Where(x => DateTime.Compare(DateTime.Now, x.JOBENDTIME.Value) >= 0 && x.COMPLETERATE.StartsWith(Resources.Resource.Processing)).ToList();

                foreach (var jobResult in jobResultList)
                {
                    JobResultDataAccessor.Refresh(jobResult.UNIQUEID, jobResult.JOBUNIQUEID, jobResult.BEGINDATE, jobResult.ENDDATE, jobResult.OVERTIMEREASON, jobResult.OVERTIMEUSER, jobResult.UNPATROLREASON, jobResult.UNPATROLUSER);
                }
            }
        }

        private bool Generate(JOBRESULT JobResult)
        {
            bool result = false;

            try
            {
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var route = (from j in db.JOB
                                 join r in db.ROUTE
                                 on j.ROUTEUNIQUEID equals r.UNIQUEID
                                 where j.UNIQUEID == JobResult.JOBUNIQUEID
                                 select r).First();

                    var nextVerifyOrganization = (from f in db.FLOW
                                                  join x in db.FLOWFORM
                                                  on f.UNIQUEID equals x.FLOWUNIQUEID
                                                  join v in db.FLOWVERIFYORGANIZATION
                                                  on f.UNIQUEID equals v.FLOWUNIQUEID
                                                  join o in db.ORGANIZATION
                                                  on v.ORGANIZATIONUNIQUEID equals o.UNIQUEID
                                                  where f.ORGANIZATIONUNIQUEID == route.ORGANIZATIONUNIQUEID && x.FORM == Define.EnumForm.EquipmentPatrolResult.ToString()
                                                  select new
                                                  {
                                                      UniqueID = o.UNIQUEID,
                                                      Description = o.DESCRIPTION,
                                                      ManagerUserID = o.MANAGERUSERID,
                                                      Seq = v.SEQ
                                                  }).OrderBy(x => x.Seq).FirstOrDefault();

                    //有設定簽核流程
                    if (nextVerifyOrganization != null)
                    {
                        //簽核組織有設定主管
                        if (!string.IsNullOrEmpty(nextVerifyOrganization.ManagerUserID))
                        {
                            db.JOBRESULTFLOW.Add(new JOBRESULTFLOW()
                            {
                                JOBRESULTUNIQUEID = JobResult.UNIQUEID,
                                CURRENTSEQ = 1,
                                ISCLOSED = "N"
                            });

                            var user = db.ACCOUNT.First(x => x.ID == nextVerifyOrganization.ManagerUserID);

                            db.JOBRESULTFLOWLOG.Add(new JOBRESULTFLOWLOG()
                            {
                                JOBRESULTUNIQUEID = JobResult.UNIQUEID,
                                SEQ = 1,
                                FLOWSEQ = nextVerifyOrganization.Seq,
                                USERID = nextVerifyOrganization.ManagerUserID,
                                USERNAME = user.NAME,
                                NOTIFYTIME = DateTime.Now
                            });

                            if (Config.HaveMailSetting)
                            {
                                SendVerifyMail(user, JobResult);
                            }

                            db.SaveChanges();

                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                    }
                    else
                    {
                        var organization = db.ORGANIZATION.First(x => x.UNIQUEID == route.ORGANIZATIONUNIQUEID);

                        //簽核組織有設定主管
                        if (!string.IsNullOrEmpty(organization.MANAGERUSERID))
                        {
                            db.JOBRESULTFLOW.Add(new JOBRESULTFLOW()
                            {
                                JOBRESULTUNIQUEID = JobResult.UNIQUEID,
                                CURRENTSEQ = 1,
                                ISCLOSED = "N"
                            });

                            var user = db.ACCOUNT.First(x => x.ID == organization.MANAGERUSERID);

                            db.JOBRESULTFLOWLOG.Add(new JOBRESULTFLOWLOG()
                            {
                                JOBRESULTUNIQUEID = JobResult.UNIQUEID,
                                SEQ = 1,
                                FLOWSEQ = 0,
                                USERID = organization.MANAGERUSERID,
                                USERNAME = user.NAME,
                                NOTIFYTIME = DateTime.Now
                            });

                            if (Config.HaveMailSetting)
                            {
                                SendVerifyMail(user, JobResult);
                            }

                            db.SaveChanges();

                            result = true;
                        }
                        else
                        {
                            result = false;
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
       
        private static void SendVerifyMail(ACCOUNT Account, JOBRESULT JobResult)
        {
            try
            {
                if (JobResult.COMPLETERATE != Resources.Resource.Completed)
                {
                    SendUnCompleteMail(JobResult);
                }

                var beginTime = JobResult.BEGINDATE;

                if (!string.IsNullOrEmpty(JobResult.BEGINTIME))
                {
                    beginTime = string.Format("{0} {1}", JobResult.BEGINDATE, JobResult.BEGINTIME);
                }

                var endTime = JobResult.ENDDATE;

                if (!string.IsNullOrEmpty(JobResult.ENDTIME))
                {
                    endTime = string.Format("{0} {1}", JobResult.ENDDATE, JobResult.ENDTIME);
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

                var subject = string.Format("[巡檢結果簽核通知][{0}]{1}({2})", JobResult.ORGANIZATIONDESCRIPTION, JobResult.DESCRIPTION, checkDate);

                var th = "<th style=\"width:150px;border:1px solid #333;padding:8px;text-align:right;color:#707070;background: #F4F4F4;\">{0}</th>";
                var td = "<td style=\"width:400px;border:1px solid #333;padding:8px;color:#707070;\">{0}</td>";

                var sb = new StringBuilder();

                sb.Append("<table style=\"1px solid #ddd;font-size:13px;border-collapse:collapse;\">");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "組織"));
                sb.Append(string.Format(td, JobResult.ORGANIZATIONDESCRIPTION));
                sb.Append("</tr>");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "巡檢路線派工"));
                sb.Append(string.Format(td, JobResult.DESCRIPTION));
                sb.Append("</tr>");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "派工開始日期"));
                sb.Append(string.Format(td, JobResult.BEGINDATE));
                sb.Append("</tr>");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "派工開始時間"));
                sb.Append(string.Format(td, JobResult.BEGINTIME));
                sb.Append("</tr>");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "派工結束日期"));
                sb.Append(string.Format(td, JobResult.ENDDATE));
                sb.Append("</tr>");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "派工結束時間"));
                sb.Append(string.Format(td, JobResult.ENDTIME));
                sb.Append("</tr>");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "完成率"));
                sb.Append(string.Format(td, JobResult.COMPLETERATE));
                sb.Append("</tr>");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "檢查人員"));
                sb.Append(string.Format(td, JobResult.CHECKUSERS));
                sb.Append("</tr>");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "詳細資料"));
                sb.Append(string.Format(td, string.Format("<a href=\"http://ASECL-eINSPRD01/FEM.Portal/Home/Index?ReturnUrl=http://ASECL-eINSPRD01/FEM/zh-tw/EquipmentMaintenance/ResultVerify/Index?JobResultUniqueID=" + JobResult.UNIQUEID + "\">連結</a>")));
                sb.Append("</tr>");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "批次核簽"));
                sb.Append(string.Format(td, string.Format("<a href=\"http://ASECL-eINSPRD01/FEM.Portal/Home/Index?ReturnUrl=http://ASECL-eINSPRD01/FEM/zh-tw/EquipmentMaintenance/ResultBatchVerify/Index\">連結</a>")));
                sb.Append("</tr>");

                sb.Append("</table>");

                MailHelper.SendMail(new MailAddress(Account.EMAIL, Account.NAME), subject, sb.ToString());
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private static void SendUnCompleteMail(JOBRESULT JobResult)
        {
            try
            {
                var mailAddressList = new List<MailAddress>();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var jobUserList = (from x in db.JOBUSER
                                       join u in db.ACCOUNT
                                       on x.USERID equals u.ID
                                       where x.JOBUNIQUEID == JobResult.JOBUNIQUEID && !string.IsNullOrEmpty(u.EMAIL)
                                       select u).ToList();

                    foreach (var jobUser in jobUserList)
                    {
                        mailAddressList.Add(new MailAddress(jobUser.EMAIL, jobUser.NAME));
                    }

                    var routeManagerList = (from x in db.ROUTEMANAGER
                                            join u in db.ACCOUNT
                                            on x.USERID equals u.ID
                                            join j in db.JOB
                                            on x.ROUTEUNIQUEID equals j.ROUTEUNIQUEID
                                            where j.UNIQUEID == JobResult.JOBUNIQUEID && !string.IsNullOrEmpty(u.EMAIL)
                                            select u).ToList();

                    foreach (var routeManager in routeManagerList)
                    {
                        mailAddressList.Add(new MailAddress(routeManager.EMAIL, routeManager.NAME));
                    }
                }

                if (mailAddressList.Count > 0)
                {
                    var beginTime = JobResult.BEGINDATE;

                    if (!string.IsNullOrEmpty(JobResult.BEGINTIME))
                    {
                        beginTime = string.Format("{0} {1}", JobResult.BEGINDATE, JobResult.BEGINTIME);
                    }

                    var endTime = JobResult.ENDDATE;

                    if (!string.IsNullOrEmpty(JobResult.ENDTIME))
                    {
                        endTime = string.Format("{0} {1}", JobResult.ENDDATE, JobResult.ENDTIME);
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

                    var subject = string.Format("[巡檢派工異常通知][{0}]{1}({2})", JobResult.ORGANIZATIONDESCRIPTION, JobResult.DESCRIPTION, checkDate);

                    var th = "<th style=\"width:150px;border:1px solid #333;padding:8px;text-align:right;color:#707070;background: #F4F4F4;\">{0}</th>";
                    var td = "<td style=\"width:400px;border:1px solid #333;padding:8px;color:#707070;\">{0}</td>";

                    var sb = new StringBuilder();

                    sb.Append("<table style=\"1px solid #ddd;font-size:13px;border-collapse:collapse;\">");

                    sb.Append("<tr>");
                    sb.Append(string.Format(th, "組織"));
                    sb.Append(string.Format(td, JobResult.ORGANIZATIONDESCRIPTION));
                    sb.Append("</tr>");

                    sb.Append("<tr>");
                    sb.Append(string.Format(th, "巡檢路線派工"));
                    sb.Append(string.Format(td, JobResult.DESCRIPTION));
                    sb.Append("</tr>");

                    sb.Append("<tr>");
                    sb.Append(string.Format(th, "派工開始日期"));
                    sb.Append(string.Format(td, JobResult.BEGINDATE));
                    sb.Append("</tr>");

                    sb.Append("<tr>");
                    sb.Append(string.Format(th, "派工開始時間"));
                    sb.Append(string.Format(td, JobResult.BEGINTIME));
                    sb.Append("</tr>");

                    sb.Append("<tr>");
                    sb.Append(string.Format(th, "派工結束日期"));
                    sb.Append(string.Format(td, JobResult.ENDDATE));
                    sb.Append("</tr>");

                    sb.Append("<tr>");
                    sb.Append(string.Format(th, "派工結束時間"));
                    sb.Append(string.Format(td, JobResult.ENDTIME));
                    sb.Append("</tr>");

                    sb.Append("<tr>");
                    sb.Append(string.Format(th, "完成率"));
                    sb.Append(string.Format(td, JobResult.COMPLETERATE));
                    sb.Append("</tr>");

                    sb.Append("<tr>");
                    sb.Append(string.Format(th, "檢查人員"));
                    sb.Append(string.Format(td, JobResult.CHECKUSERS));
                    sb.Append("</tr>");

                    sb.Append("<tr>");
                    sb.Append(string.Format(th, "詳細資料"));
                    sb.Append(string.Format(td, string.Format("<a href=\"http://ASECL-eINSPRD01/FEM.Portal/Home/Index?ReturnUrl=http://ASECL-eINSPRD01/FEM/zh-tw/EquipmentMaintenance/ProgressQuery/Index?JobResultUniqueID=" + JobResult.UNIQUEID + "\">連結</a>")));
                    sb.Append("</tr>");

                    sb.Append("</table>");

                    MailHelper.SendMail(mailAddressList, subject, sb.ToString());
                }
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
