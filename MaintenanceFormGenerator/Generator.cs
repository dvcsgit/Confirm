using System;
using System.Linq;
using System.Reflection;
using Utility;
#if ORACLE
using DbEntity.ORACLE.EquipmentMaintenance;
#else
using DbEntity.MSSQL.EquipmentMaintenance;
#endif
using DataAccess.EquipmentMaintenance;
using System.Collections.Generic;
using System.Net.Mail;
using DbEntity.MSSQL;
using System.Text;
using DataAccess;

namespace MaintenanceFormGenerator
{
    public class Generator : IDisposable
    {
        public void Generate()
        {
            using (EDbEntities db = new EDbEntities())
            {
                var query = (from j in db.MJob
                             join x in db.MJobUser
                             on j.UniqueID equals x.MJobUniqueID
                             where !j.EndDate.HasValue || DateTime.Compare(j.EndDate.Value, DateTime.Today) >= 0
                             select j.UniqueID).Distinct().ToList();

                var jobList = db.MJob.Where(x => query.Contains(x.UniqueID)).ToList();

                foreach (var job in jobList)
                {
                    Generate(job);
                }
            }
        }

        private void Generate(MJob Job)
        {
            try
            {
                DateTime begin, end;

                JobCycleHelper.GetDateSpan(DateTime.Today.AddDays(Job.NotifyDay), Job.BeginDate, Job.EndDate, Job.CycleCount, Job.CycleMode, out begin, out end);

                if (DateTime.Compare(DateTime.Today, begin.AddDays(-1 * Job.NotifyDay)) >= 0)
                {
                    using (EDbEntities db = new EDbEntities())
                    {
                        if (!db.MForm.Any(x => x.MJobUniqueID == Job.UniqueID && x.CycleBeginDate == begin && x.CycleEndDate == end))
                        {
                            var vhnoPrefix = string.Format("M{0}", DateTime.Today.ToString("yyyyMM").Substring(2));

                            var seq = 1;

                            var temp = db.MForm.Where(x => x.VHNO.StartsWith(vhnoPrefix)).OrderByDescending(x => x.VHNO).FirstOrDefault();

                            if (temp != null)
                            {
                                seq = int.Parse(temp.VHNO.Substring(5, 3)) + 1;
                            }

                            var vhno = string.Format("{0}{1}", vhnoPrefix, seq.ToString().PadLeft(3, '0'));

                            var equipmentList = (from x in db.MJobEquipment
                                                 join e in db.Equipment
                                                 on x.EquipmentUniqueID equals e.UniqueID
                                                 join p in db.EquipmentPart
                                                 on x.PartUniqueID equals p.UniqueID into tmpPart
                                                 from p in tmpPart.DefaultIfEmpty()
                                                 where x.MJobUniqueID == Job.UniqueID
                                                 select new
                                                 {
                                                     EquipmentUniqueID = x.EquipmentUniqueID,
                                                     EquipmentID = e.ID,
                                                     EquipmentName = e.Name,
                                                     PartUniqueID = x.PartUniqueID,
                                                     PartDescription = p != null ? p.Description : ""
                                                 }).OrderBy(x => x.EquipmentID).ThenBy(x => x.PartDescription).ToList();

                            var subSeq = 1;

                            Dictionary<string, string> vhnoEquipment = new Dictionary<string, string>();

                            foreach (var equipment in equipmentList)
                            {
                                var subVhno = string.Format("{0}-{1}", vhno, subSeq.ToString().PadLeft(4, '0'));

                                Logger.Log(string.Format("Create MaintenanceForm {0}", subVhno));

                                var uniqueID = Guid.NewGuid().ToString();

                                db.MForm.Add(new MForm()
                                {
                                    UniqueID = uniqueID,
                                    MJobUniqueID = Job.UniqueID,
                                    VHNO = subVhno,
                                    Status = "0",
                                    EquipmentUniqueID = equipment.EquipmentUniqueID,
                                    PartUniqueID = equipment.PartUniqueID,
                                    CycleBeginDate = begin,
                                    CycleEndDate = end,
                                    EstBeginDate = begin,
                                    EstEndDate = end,
                                    CreateTime = DateTime.Now
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

        private static void SendNotifyMail(MJob Job, string VHNO, DateTime CycleBeginDate, DateTime CycleEndDate, Dictionary<string, string> VHNOEquipment)
        {
            try
            {
#if !DEBUG
                using (DbEntities db = new DbEntities())
                {
                    using (EDbEntities edb = new EDbEntities())
                    {
                        var jobUserList = edb.MJobUser.Where(x => x.MJobUniqueID == Job.UniqueID).ToList();

                        var recipientList = new List<MailAddress>();

                        foreach (var jobUser in jobUserList)
                        {
                            var user = db.User.FirstOrDefault(x => x.ID == jobUser.UserID);

                            if (user != null && !string.IsNullOrEmpty(user.Email))
                            {
                                recipientList.Add(new MailAddress(user.Email, user.Name));
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

                            var subject = string.Format("[定期保養通知][{0}]{1}({2})", VHNO, Job.Description, mDate);

                            var th = "<th style=\"width:150px;border:1px solid #333;padding:8px;text-align:right;color:#707070;background: #F4F4F4;\">{0}</th>";
                            var td = "<td style=\"width:400px;border:1px solid #333;padding:8px;color:#707070;\">{0}</td>";

                            var sb = new StringBuilder();

                            sb.Append("<table>");

                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "部門"));
                            sb.Append(string.Format(td, OrganizationDataAccessor.GetOrganizationDescription(Job.OrganizationUniqueID)));
                            sb.Append("</tr>");

                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "定期保養派工描述"));
                            sb.Append(string.Format(td, Job.Description));
                            sb.Append("</tr>");

                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "保養週期"));
                            sb.Append(string.Format(td, mDate));
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
