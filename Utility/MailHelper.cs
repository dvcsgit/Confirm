using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility.Models;

namespace Utility
{
    public class MailHelper
    {
        //public static void SendMail(string BatchName, string Message, Error Error)
        //{
        //    try
        //    {
        //        StringBuilder sb = new StringBuilder();

        //        if (!string.IsNullOrEmpty(Message))
        //        {
        //            sb.Append("<p>");
        //            sb.Append(Message);
        //            sb.Append("</p>");
        //        }

        //        foreach (var errMessage in Error.ErrorMessageList)
        //        {
        //            sb.Append("<p>");
        //            sb.Append(errMessage);
        //            sb.Append("</p>");
        //        }

        //        SendMail(AdministratorList, BatchName, sb.ToString(), false);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Log(MethodBase.GetCurrentMethod(), ex);
        //    }
        //}

        //public static void SendMail(string BatchName, Error Error)
        //{
        //    SendMail(BatchName + " Exception", "", Error);
        //}

        public static bool SendMail(MailAddress Recipient, string Subject, string Body)
        {
            return SendMail(new List<MailAddress>() { Recipient }, null, null, Subject, Body, null);
        }

        public static bool SendMail(MailAddress Recipient, List<MailAddress> CCList, string Subject, string Body)
        {
            return SendMail(new List<MailAddress>() { Recipient }, CCList, null, Subject, Body, null);
        }

        public static bool SendMail(MailAddress Recipient, List<MailAddress> CCList, List<MailAddress> BCCList, string Subject, string Body)
        {
            return SendMail(new List<MailAddress>() { Recipient }, CCList, BCCList, Subject, Body, null);
        }

        public static void SendMail(List<MailAddress> RecipientList, string Subject, string Body)
        {
            SendMail(RecipientList, null, null, Subject, Body, null);
        }

        public static void SendMail(List<MailAddress> RecipientList, string Subject, string Body, List<Attachment> AttachList)
        {
            SendMail(RecipientList, null,null, Subject, Body, AttachList);
        }

        public static void SendMail(List<MailAddress> RecipientList, List<MailAddress> CCList, string Subject, string Body)
        {
            SendMail(RecipientList, CCList, null, Subject, Body, null);
        }

        public static void SendMail(List<MailAddress> RecipientList, List<MailAddress> CCList, List<MailAddress> BCCList, string Subject, string Body)
        {
            SendMail(RecipientList, CCList,BCCList, Subject, Body, null);
        }

        public static bool SendMail(List<MailAddress> RecipientList, List<MailAddress> CCList, List<MailAddress> BCCList, string Subject, string Body, List<Attachment> AttachList)
        {
            bool result = false;

            try
            {
                using (MailMessage mailMsg = new MailMessage())
                {
                    mailMsg.From = Config.MailFrom;

                    foreach (MailAddress recipient in RecipientList)
                    {
                        mailMsg.To.Add(recipient);
                    }

                    if (CCList != null && CCList.Count > 0)
                    {
                        foreach (MailAddress cc in CCList)
                        {
                            mailMsg.CC.Add(cc);
                        }
                    }

                    if (BCCList != null && BCCList.Count > 0)
                    {
                        foreach (MailAddress bcc in BCCList)
                        {
                            mailMsg.Bcc.Add(bcc);
                        }
                    }

                    if (Config.BCC != null && Config.BCC.Count > 0)
                    {
                        foreach (var bcc in Config.BCC)
                        {
                            mailMsg.Bcc.Add(bcc);
                        }
                    }

                    mailMsg.Subject = Subject;
                    mailMsg.Body = Body;
                    mailMsg.IsBodyHtml = true;
                    mailMsg.Priority = MailPriority.High;

                    if (AttachList != null)
                    {
                        foreach (var attach in AttachList)
                        {
                            mailMsg.Attachments.Add(attach);
                        }
                    }

                    using (SmtpClient smtp = new SmtpClient(Config.SmtpHost))
                    {
                        if (!string.IsNullOrEmpty(Config.SmtpDomain))
                        {
                            smtp.UseDefaultCredentials = false;

                            smtp.Credentials = new System.Net.NetworkCredential()
                            {
                                UserName = Config.SmtpUserName,
                                Password = Config.SmtpPassword,
                                Domain = Config.SmtpDomain
                            };
                        }
                        
#if !DEBUG
                        smtp.Send(mailMsg);

                        Logger.Log(string.Format("Send Mail [{0}] Success", Subject));
#endif                  
                             
                    }
                }

                result = true;
            }
            catch (Exception ex)
            {
                result = false;

                Logger.Log(MethodInfo.GetCurrentMethod(), ex);
            }

            return result;
        }
    }
}
