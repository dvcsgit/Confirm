using CHIMEI.AHSummaryReport.Models;
using DataAccess;
using DbEntity.MSSQL.EquipmentMaintenance;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Utility;

namespace CHIMEI.AHSummaryReport
{
    public class ReportHelper : IDisposable
    {
        private const string ConfigFileName = "Mail2.xml";

        private static string ConfigFile
        {
            get
            {
                string exePath = System.AppDomain.CurrentDomain.BaseDirectory;

                string filePath = Path.Combine(exePath, ConfigFileName);

                if (System.IO.File.Exists(filePath))
                {
                    return filePath;
                }
                else
                {
                    filePath = Path.Combine(exePath, "bin", ConfigFileName);

                    if (System.IO.File.Exists(filePath))
                    {
                        return filePath;
                    }
                    else
                    {
                        return ConfigFileName;
                    }
                }
            }
        }

        private List<MailAddress> ToList
        {
            get
            {
                var toList = new List<MailAddress>();

                try
                {
                    var config = XDocument.Load(ConfigFile);

                    var elementList = config.Root.Elements("MAIL").ToList();

                    foreach (var element in elementList)
                    {
                        toList.Add(new MailAddress(element.Attribute("EMAIL").Value, element.Attribute("NAME").Value));
                    }
                }
                catch (Exception ex)
                {
                    toList = new List<MailAddress>();

                    Logger.Log(MethodBase.GetCurrentMethod(), ex);
                }

                return toList;
            }
        }

        public void Send()
        {
            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var query = (from abnormal in db.Abnormal
                                 join x in db.AbnormalCheckResult
                                 on abnormal.UniqueID equals x.AbnormalUniqueID
                                 join c in db.CheckResult
                                 on x.CheckResultUniqueID equals c.UniqueID
                                 join a in db.ArriveRecord
                                 on c.ArriveRecordUniqueID equals a.UniqueID
                                 where !abnormal.ClosedTime.HasValue
                                 select new
                                 {
                                     a.OrganizationUniqueID,
                                     c.EquipmentUniqueID,
                                     c.EquipmentID,
                                     c.EquipmentName,
                                     c.PartDescription,
                                     c.CheckDate,
                                     c.IsAbnormal,
                                     c.IsAlert,
                                     CheckUserID = a.UserID,
                                     CheckUserName = a.UserName
                                 }).AsQueryable();

                    var itemList = new List<MailContent>();

                    var queryResult = query.ToList();

                    var tmp = queryResult.Select(x => new
                    {
                        x.OrganizationUniqueID,
                        x.EquipmentUniqueID,
                        x.EquipmentID,
                        x.EquipmentName,
                        x.PartDescription,
                        x.CheckDate
                    }).Distinct().ToList();

                    foreach (var t in tmp)
                    {
                        var item = new MailContent
                        {
                            OrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(t.OrganizationUniqueID),
                            EquipmentID = t.EquipmentID,
                            EquipmentName = t.EquipmentName,
                            PartDescription = t.PartDescription,
                            CheckDate = t.CheckDate
                        };

                        var results = queryResult.Where(x => x.EquipmentUniqueID == t.EquipmentUniqueID && x.CheckDate == t.CheckDate).ToList();

                        item.IsAbnormal = results.Any(x => x.IsAbnormal);
                        item.IsAlert = !item.IsAbnormal && results.Any(x => x.IsAlert);

                        var checkUsers = results.Select(x => new { x.CheckUserID, x.CheckUserName }).Distinct().ToList();

                        foreach (var checkUser in checkUsers)
                        {
                            var user = string.Empty;

                            if (!string.IsNullOrEmpty(checkUser.CheckUserName))
                            {
                                user = string.Format("{0}/{1}", checkUser.CheckUserID, checkUser.CheckUserName);
                            }
                            else
                            {
                                user = checkUser.CheckUserID;
                            }

                            item.CheckUserList.Add(user);
                        }

                        itemList.Add(item);
                    }

                    itemList = itemList.OrderBy(x => x.CheckDate).ThenBy(x => x.EquipmentDisplay).ToList();

                    Logger.Log(string.Format("{0} Abnormal to Send", itemList.Count));

                    if (itemList.Count > 0)
                    {
                        Send(itemList);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private void Send(List<MailContent> ItemList)
        {
            try
            {
                var subject = string.Format("[異常追蹤管理]截至{0}尚有{1}筆未結案資料", DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today), ItemList.Count);

                Logger.Log(subject);

                var th = "<th style=\"border:1px solid #333;padding:8px;text-align:center;color:#707070;background: #F4F4F4;\">{0}</th>";
                var td = "<td style=\"border:1px solid #333;padding:8px;color:#707070;\">{0}</td>";

                var sb = new StringBuilder();

                sb.Append("<table style=\"border:1px solid #E5E5E5;font-size:13px;border-collapse:collapse;\">");

                sb.Append("<tr>");
                sb.Append(string.Format(th, "異常"));
                sb.Append(string.Format(th, "組織"));
                sb.Append(string.Format(th, "設備"));
                sb.Append(string.Format(th, "檢查日期"));
                sb.Append(string.Format(th, "檢查人員"));
                sb.Append("</tr>");

                foreach (var item in ItemList)
                {
                    sb.Append("<tr>");
                    sb.Append(string.Format(td, item.Abnormal));
                    sb.Append(string.Format(td, item.OrganizationDescription));
                    sb.Append(string.Format(td, item.EquipmentDisplay));
                    sb.Append(string.Format(td, item.CheckDateString));
                    sb.Append(string.Format(td, item.CheckUsers));
                    sb.Append("</tr>");
                }

                sb.Append("</table>");

                Logger.Log(string.Format("Mail Body:{0}", sb.ToString()));

                MailHelper.SendMail(ToList, subject, sb.ToString());
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

        ~ReportHelper()
        {
            Dispose(false);
        }

        #endregion
    }
}
