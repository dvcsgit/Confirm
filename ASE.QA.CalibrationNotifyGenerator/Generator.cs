using DataAccess.ASE;
using DataAccess.ASE.QA;
using DbEntity.ASE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace ASE.QA.CalibrationNotifyGenerator
{
    public class Generator : IDisposable
    {
        public void Generate()
        {
            using (ASEDbEntities db = new ASEDbEntities())
            {
                var baseDate = DateTime.Today.AddMonths(2);

                var equipmentList = db.QA_EQUIPMENT.Where(x => x.STATUS == "1" && x.CAL == "Y" && x.NEXTCALDATE.HasValue && DateTime.Compare(x.NEXTCALDATE.Value, baseDate) <= 0).ToList();

                foreach (var e in equipmentList)
                {
                    var a = db.QA_CALIBRATIONAPPLY.FirstOrDefault(x => x.EQUIPMENTUNIQUEID == e.UNIQUEID && x.ESTCALDATE == e.NEXTCALDATE);
                    var n = db.QA_CALIBRATIONNOTIFY.Where(x => x.EQUIPMENTUNIQUEID == e.UNIQUEID && x.ESTCALDATE == e.NEXTCALDATE).OrderByDescending(x => x.CREATETIME).FirstOrDefault();

                    QA_CALIBRATIONFORM f = null;

                    if (n != null)
                    {
                        f = db.QA_CALIBRATIONFORM.FirstOrDefault(x => x.NOTIFYUNIQUEID == n.UNIQUEID);
                    }

                    if (f == null && a != null)
                    {
                        f = db.QA_CALIBRATIONFORM.FirstOrDefault(x => x.APPLYUNIQUEID == a.UNIQUEID);
                    }

                    //已轉出執行單
                    if (f != null && f.STATUS != "9")
                    { 
                    
                    }
                    else
                    {
                        //已轉出通知單
                        if (n != null && n.STATUS != "4")
                        {

                        }
                        else
                        {
                            try
                            {
                                #region VHNO
                                var vhnoPreFix = string.Format("R{0}", e.NEXTCALDATE.Value.ToString("yyyyMM").Substring(2));

                                var tmp = db.QA_CALIBRATIONNOTIFY.Where(x => x.VHNO.StartsWith(vhnoPreFix)).OrderByDescending(x => x.VHNO).ToList();

                                int vhnoSeq = 1;

                                if (tmp.Count > 0)
                                {
                                    vhnoSeq = int.Parse(tmp.First().VHNO.Substring(5)) + 1;
                                }

                                var vhno = string.Format("{0}{1}", vhnoPreFix, vhnoSeq.ToString().PadLeft(4, '0'));
                                #endregion

                                var notifyUniqueID = Guid.NewGuid().ToString();
                                var time = DateTime.Now;

                                var notify = new QA_CALIBRATIONNOTIFY()
                                {
                                    UNIQUEID = notifyUniqueID,
                                    EQUIPMENTUNIQUEID = e.UNIQUEID,
                                    CREATETIME = time,
                                    VHNO = vhno,
                                    STATUS = "1",
                                    CALTYPE = e.CALTYPE,
                                    CALUNIT = e.CALUNIT,
                                    CASETYPE = e.CASETYPE,
                                    ESTCALDATE = e.NEXTCALDATE,
                                };

                                db.QA_CALIBRATIONNOTIFY.Add(notify);

                                db.QA_CALIBRATIONNOTIFYFLOWLOG.Add(new QA_CALIBRATIONNOTIFYFLOWLOG()
                                {
                                    NOTIFYUNIQUEID = notifyUniqueID,
                                    SEQ = 1,
                                    FLOWSEQ = 0,
                                    USERID = "SYS",
                                    NOTIFYTIME = time,
                                    VERIFYTIME = time,
                                    VERIFYRESULT = "Y",
                                    ISCANCELED = "N"
                                });

                                db.QA_CALIBRATIONNOTIFYFLOWLOG.Add(new QA_CALIBRATIONNOTIFYFLOWLOG()
                                {
                                    NOTIFYUNIQUEID = notifyUniqueID,
                                    SEQ = 2,
                                    FLOWSEQ = 1,
                                    USERID = e.OWNERID,
                                    NOTIFYTIME = time,
                                    ISCANCELED = "N"
                                });

                                var detailList = db.QA_EQUIPMENTCALDETAIL.Where(x => x.EQUIPMENTUNIQUEID == e.UNIQUEID).ToList();
                                var subDetailList = db.QA_EQUIPMENTCALSUBDETAIL.Where(x => x.EQUIPMENTUNIQUEID == e.UNIQUEID).ToList();

                                db.QA_CALIBRATIONNOTIFYDETAIL.AddRange(detailList.Select(x => new QA_CALIBRATIONNOTIFYDETAIL
                                {
                                    NOTIFYUNIQUEID = notifyUniqueID,
                                    SEQ = x.SEQ,
                                    UPPERUSINGRANGEUNITUNIQUEID = x.UPPERUSINGRANGEUNITUNIQUEID,
                                    UPPERUSINGRANGEUNITREMARK = x.UPPERUSINGRANGEUNITREMARK,
                                    UPPERUSINGRANGE = x.UPPERUSINGRANGE,
                                    UNITUNIQUEID = x.UNITUNIQUEID,
                                    UNITREMARK = x.UNITREMARK,
                                    RANGETOLERANCEUNITUNIQUEID = x.RANGETOLERANCEUNITUNIQUEID,
                                    RANGETOLERANCEUNITREMARK = x.RANGETOLERANCEUNITREMARK,
                                    RANGETOLERANCESYMBOL = x.RANGETOLERANCESYMBOL,
                                    RANGETOLERANCE = x.RANGETOLERANCE,
                                    LOWERUSINGRANGEUNITUNIQUEID = x.LOWERUSINGRANGEUNITUNIQUEID,
                                    CHARACTERISTICREMARK = x.CHARACTERISTICREMARK,
                                    CHARACTERISTICUNIQUEID = x.CHARACTERISTICUNIQUEID,
                                    LOWERUSINGRANGE = x.LOWERUSINGRANGE,
                                    LOWERUSINGRANGEUNITREMARK = x.LOWERUSINGRANGEUNITREMARK
                                }).ToList());

                                db.QA_CALIBRATIONNOTIFYSUBDETAIL.AddRange(subDetailList.Select(x => new QA_CALIBRATIONNOTIFYSUBDETAIL
                                {
                                    NOTIFYUNIQUEID = notifyUniqueID,
                                    DETAILSEQ = x.DETAILSEQ,
                                    CALIBRATIONPOINT = x.CALIBRATIONPOINT,
                                    CALIBRATIONPOINTUNITUNIQUEID = x.CALIBRATIONPOINTUNITUNIQUEID,
                                    SEQ = x.SEQ,
                                    TOLERANCE = x.TOLERANCE,
                                    TOLERANCESYMBOL = x.TOLERANCESYMBOL,
                                    TOLERANCEUNITUNIQUEID = x.TOLERANCEUNITUNIQUEID
                                }).ToList());

                                db.SaveChanges();

                                SendVerifyMail(e.OWNERID, notify);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }   
                        }
                    }
                }
            }
        }

        private static void SendVerifyMail(string UserID, QA_CALIBRATIONNOTIFY Notify)
        {

            try
            {
                var mailAddressList = new List<MailAddress>();

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var user = db.ACCOUNT.FirstOrDefault(x => x.ID == UserID);

                    if (user != null && !string.IsNullOrEmpty(user.EMAIL))
                    {
                        mailAddressList.Add(new MailAddress(user.EMAIL, user.NAME));
                    }

                    if (mailAddressList.Count > 0)
                    {
                        var equipment = EquipmentHelper.Get(null, Notify.EQUIPMENTUNIQUEID);

                        var subject = string.Format("[簽核通知][{0}][{1}]儀器校驗通知單", Notify.VHNO, equipment.CalNo);

                        var th = "<th style=\"width:150px;border:1px solid #333;padding:8px;text-align:right;color:#707070;background: #F4F4F4;\">{0}</th>";
                        var td = "<td style=\"width:400px;border:1px solid #333;padding:8px;color:#707070;\">{0}</td>";

                        var sb = new StringBuilder();

                        sb.Append("<table style=\"1px solid #ddd;font-size:13px;border-collapse:collapse;\">");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "單號"));
                        sb.Append(string.Format(td, Notify.VHNO));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "校驗編號"));
                        sb.Append(string.Format(td, equipment.CalNo));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "預計校驗日期"));
                        sb.Append(string.Format(td, DateTimeHelper.DateTime2DateStringWithSeperator(Notify.ESTCALDATE)));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "部門"));
                        sb.Append(string.Format(td, equipment.OrganizationDescription));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "序號"));
                        sb.Append(string.Format(td, equipment.SerialNo));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "儀器名稱"));
                        sb.Append(string.Format(td, equipment.IchiDisplay));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "廠牌"));
                        sb.Append(string.Format(td, equipment.Brand));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "型號"));
                        sb.Append(string.Format(td, equipment.Model));
                        sb.Append("</tr>");

                        if (!string.IsNullOrEmpty(equipment.OwnerID))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "設備負責人"));
                            sb.Append(string.Format(td, equipment.Owner));
                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(equipment.OwnerManagerID))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "設備負責人主管"));

                            sb.Append(string.Format(td, equipment.OwnerManager));

                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(equipment.PEID))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "製程負責人"));

                            sb.Append(string.Format(td, equipment.PE));

                            sb.Append("</tr>");
                        }

                        if (!string.IsNullOrEmpty(equipment.PEManagerID))
                        {
                            sb.Append("<tr>");
                            sb.Append(string.Format(th, "製程負責人主管"));

                            sb.Append(string.Format(td, equipment.PEManager));

                            sb.Append("</tr>");
                        }

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "立案時間"));
                        sb.Append(string.Format(td, DateTimeHelper.DateTime2DateTimeStringWithSeperator(Notify.CREATETIME)));
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        sb.Append(string.Format(th, "連結"));
                        sb.Append(string.Format(td, string.Format("<a href=\"http://ASECL-eINSPRD01/FEM.Portal/Home/Index?ReturnUrl=http://ASECL-eINSPRD01/FEM/zh-tw/Customized_ASE_QA/CalibrationNotify/Index?VHNO={0}\">連結</a>", Notify.VHNO)));
                        sb.Append("</tr>");

                        MailHelper.SendMail(mailAddressList, subject, sb.ToString());
                    }
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
