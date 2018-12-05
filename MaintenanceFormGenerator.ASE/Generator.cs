using System;
using System.Linq;
using System.Reflection;
using Utility;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using DbEntity.ASE;
using DataAccess.ASE;

namespace MaintenanceFormGenerator.ASE
{
    public class Generator : IDisposable
    {
        public void Generate()
        {
            using (ASEDbEntities db = new ASEDbEntities())
            {
                var query = (from j in db.MJOB
                             join x in db.MJOBUSER
                             on j.UNIQUEID equals x.MJOBUNIQUEID
                             where !j.ENDDATE.HasValue || DateTime.Compare(j.ENDDATE.Value, DateTime.Today) >= 0
                             select j.UNIQUEID).Distinct().ToList();

                var jobList = db.MJOB.Where(x => query.Contains(x.UNIQUEID)).ToList();

                foreach (var job in jobList)
                {
                    Generate(job);
                }
            }
        }

        private void Generate(MJOB Job)
        {
            try
            {
                DateTime begin, end;
                
                JobCycleHelper.GetDateSpan(DateTime.Today.AddDays(Job.NOTIFYDAY.Value), Job.BEGINDATE.Value, Job.ENDDATE, Job.CYCLECOUNT.Value, Job.CYCLEMODE, out begin, out end);

                if (DateTime.Compare(DateTime.Today, begin.AddDays(-1 * Job.NOTIFYDAY.Value)) >= 0)
                {
                    using (ASEDbEntities db = new ASEDbEntities())
                    {
                        if (!db.MFORM.Any(x => x.MJOBUNIQUEID == Job.UNIQUEID && x.CYCLEBEGINDATE == begin && x.CYCLEENDDATE == end))
                        {
                            var vhnoPrefix = string.Format("M{0}", DateTime.Today.ToString("yyyyMM").Substring(2));

                            var seq = 1;

                            var temp = db.MFORM.Where(x => x.VHNO.StartsWith(vhnoPrefix)).OrderByDescending(x => x.VHNO).FirstOrDefault();

                            if (temp != null)
                            {
                                seq = int.Parse(temp.VHNO.Substring(5, 3)) + 1;
                            }

                            var vhno = string.Format("{0}{1}", vhnoPrefix, seq.ToString().PadLeft(3, '0'));

                            var equipmentList = (from x in db.MJOBEQUIPMENT
                                                 join e in db.EQUIPMENT
                                                 on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                                 join p in db.EQUIPMENTPART
                                                 on x.PARTUNIQUEID equals p.UNIQUEID into tmpPart
                                                 from p in tmpPart.DefaultIfEmpty()
                                                 where x.MJOBUNIQUEID == Job.UNIQUEID
                                                 select new
                                                 {
                                                     EquipmentUniqueID = x.EQUIPMENTUNIQUEID,
                                                     EquipmentID = e.ID,
                                                     EquipmentName = e.NAME,
                                                     PartUniqueID = x.PARTUNIQUEID,
                                                     PartDescription = p != null ? p.DESCRIPTION : ""
                                                 }).OrderBy(x => x.EquipmentID).ThenBy(x => x.PartDescription).ToList();

                            var subSeq = 1;

                            Dictionary<string, string> vhnoEquipment = new Dictionary<string, string>();

                            foreach (var equipment in equipmentList)
                            {
                                if (db.MJOBEQUIPMENTSTANDARD.Any(x => x.MJOBUNIQUEID == Job.UNIQUEID && x.EQUIPMENTUNIQUEID == equipment.EquipmentUniqueID && x.PARTUNIQUEID == equipment.PartUniqueID))
                                {
                                    var subVhno = string.Format("{0}-{1}", vhno, subSeq.ToString().PadLeft(4, '0'));

                                    Logger.Log(string.Format("Create MaintenanceForm {0}", subVhno));

                                    var uniqueID = Guid.NewGuid().ToString();

                                    db.MFORM.Add(new MFORM()
                                    {
                                        UNIQUEID = uniqueID,
                                        MJOBUNIQUEID = Job.UNIQUEID,
                                        VHNO = subVhno,
                                        STATUS = "0",
                                        EQUIPMENTUNIQUEID = equipment.EquipmentUniqueID,
                                        PARTUNIQUEID = equipment.PartUniqueID,
                                        CYCLEBEGINDATE = begin,
                                        CYCLEENDDATE = end,
                                        ESTBEGINDATE = begin,
                                        ESTENDDATE = end,
                                        CREATETIME = DateTime.Now
                                    });

                                    db.SaveChanges();

                                    var equipmentDescription = string.Format("{0}/{1}", equipment.EquipmentID, equipment.EquipmentName);

                                    if (!string.IsNullOrEmpty(equipment.PartDescription))
                                    {
                                        equipmentDescription = string.Format("{0}-{1}", equipmentDescription, equipment.PartDescription);
                                    }

                                    vhnoEquipment.Add(subVhno, equipmentDescription);

                                    subSeq++;
                                }
                            }

                            if (Config.HaveMailSetting)
                            {
                                SendNotifyMail(Job, vhno, begin, end, vhnoEquipment);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private static void SendNotifyMail(MJOB Job, string VHNO, DateTime CycleBeginDate, DateTime CycleEndDate, Dictionary<string, string> VHNOEquipment)
        {
            try
            {
#if !DEBUG
                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var jobUserList = db.MJOBUSER.Where(x => x.MJOBUNIQUEID == Job.UNIQUEID).ToList();

                    var recipientList = new List<MailAddress>();

                    foreach (var jobUser in jobUserList)
                    {
                        var user = db.ACCOUNT.FirstOrDefault(x => x.ID == jobUser.USERID);

                        if (user != null && !string.IsNullOrEmpty(user.EMAIL))
                        {
                            recipientList.Add(new MailAddress(user.EMAIL, user.NAME));
                        }
                    }

                    if (recipientList.Count > 0)
                    {
                        var begin = DateTimeHelper.DateTime2DateStringWithSeperator(CycleBeginDate);
                        var end = DateTimeHelper.DateTime2DateStringWithSeperator(CycleEndDate);

                        var mDate = string.Empty;

                        if (begin == end)
                        {
                            mDate = begin;
                        }
                        else
                        {
                            mDate = string.Format("{0}~{1}", begin, end);
                        }

                        var subject = string.Format("[定期保養通知][{0}]{1}({2})", VHNO, Job.DESCRIPTION, mDate);

                        var th = "<th style=\"width:150px;border:1px solid #333;padding:8px;text-align:right;color:#707070;background: #F4F4F4;\">{0}</th>";
                        var td = "<td style=\"width:400px;border:1px solid #333;padding:8px;color:#707070;\">{0}</td>";

                        var sb = new StringBuilder();

                        sb.Append("<table>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "部門"));
                        sb.Append(string.Format(td, OrganizationDataAccessor.GetOrganizationDescription(Job.ORGANIZATIONUNIQUEID)));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "定期保養派工描述"));
                        sb.Append(string.Format(td, Job.DESCRIPTION));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "保養週期"));
                        sb.Append(string.Format(td, mDate));
                        sb.Append("</tr>");
                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "連結"));
                        sb.Append(string.Format(td, string.Format("<a href=\"http://ASECL-eINSPRD01/FEM.Portal/Home/Index?ReturnUrl=http://ASECL-eINSPRD01/FEM/zh-tw/EquipmentMaintenance/MaintenanceForm/Index?VHNO={0}\">連結</a>", VHNO)));
                        sb.Append("</tr>");

                        sb.Append("</table>");

                        sb.Append("<h4>設備清單</h4>");

                        sb.Append("<table>");

                        sb.Append("<tr>");

                        sb.Append(string.Format(th, "單號"));
                        sb.Append(string.Format(th, "設備"));

                        sb.Append("</tr>");

                        foreach (var item in VHNOEquipment)
                        {
                            sb.Append("<tr>");

                            sb.Append(string.Format(td, item.Key));
                            sb.Append(string.Format(td, item.Value));

                            sb.Append("</tr>");
                        }

                        sb.Append("</table>");

                        MailHelper.SendMail(recipientList, subject, sb.ToString());
                    }
                }
#endif
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
