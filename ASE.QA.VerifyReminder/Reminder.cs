using DataAccess.ASE.QA;
using DbEntity.ASE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace ASE.QA.VerifyReminder
{
    public class Reminder : IDisposable
    {
        public void Remind(string SendType)
        {
            using (ASEDbEntities db = new ASEDbEntities())
            {
                var baseDay = DateTime.Today.AddDays(-3);

                if (SendType == "1")
                {
                    #region CalibrationApply
                    var q1 = (from x in db.QA_CALIBRATIONAPPLYFLOWLOG
                              join a in db.QA_CALIBRATIONAPPLY
                              on x.APPLYUNIQUEID equals a.UNIQUEID
                              where !x.VERIFYTIME.HasValue && x.ISCANCELED == "N" && a.STATUS == "1" && DateTime.Compare(x.NOTIFYTIME, baseDay) <= 0
                              select new
                              {
                                  a.UNIQUEID,
                                  a.VHNO,
                                  x.USERID,
                                  x.NOTIFYTIME
                              }).ToList();

                    foreach (var q in q1)
                    {
                        var days = new TimeSpan(DateTime.Today.Ticks - q.NOTIFYTIME.AddDays(3).Ticks).Days;

                        if (!string.IsNullOrEmpty(q.USERID))
                        {
                            CalibrationApplyDataAccessor.SendRemindMail(q.UNIQUEID, q.VHNO, days, q.USERID);
                        }
                        else
                        {
                            CalibrationApplyDataAccessor.SendRemindMail(q.UNIQUEID, q.VHNO, days);
                        }
                    }
                    #endregion
                }

                if (SendType == "2")
                {
                    #region CalibrationNotify
                    var q2 = (from x in db.QA_CALIBRATIONNOTIFYFLOWLOG
                              join n in db.QA_CALIBRATIONNOTIFY
                              on x.NOTIFYUNIQUEID equals n.UNIQUEID
                              where !x.VERIFYTIME.HasValue && x.ISCANCELED == "N" && n.STATUS == "1" && DateTime.Compare(x.NOTIFYTIME, baseDay) <= 0
                              select new
                              {
                                  n.UNIQUEID,
                                  n.VHNO,
                                  x.USERID,
                                  x.NOTIFYTIME,
                                  n.ESTCALDATE
                              }).ToList();

                    foreach (var q in q2)
                    {
                        var days = new TimeSpan(DateTime.Today.Ticks - q.NOTIFYTIME.AddDays(3).Ticks).Days;

                        var isDelay = false;

                        if (q.ESTCALDATE.HasValue)
                        {
                            isDelay = DateTime.Compare(q.ESTCALDATE.Value, DateTime.Today) > 0;
                        }


                        if (!string.IsNullOrEmpty(q.USERID))
                        {
                            CalibrationNotifyDataAccessor.SendRemindMail(q.UNIQUEID, days, q.USERID, isDelay);
                        }
                        else
                        {
                            CalibrationNotifyDataAccessor.SendRemindMail(q.UNIQUEID, days, isDelay);
                        }
                    }
                    #endregion
                }

                if (SendType == "3")
                {
                    #region CalibrationForm
                    var q3 = (from x in db.QA_CALIBRATIONFORMFLOWLOG
                              join f in db.QA_CALIBRATIONFORM
                              on x.FORMUNIQUEID equals f.UNIQUEID
                              where !x.VERIFYTIME.HasValue && f.STATUS == "3" && DateTime.Compare(x.NOTIFYTIME, baseDay) <= 0
                              select new
                              {
                                  f.UNIQUEID,
                                  f.VHNO,
                                  x.NOTIFYTIME,
                                  f.ESTCALDATE
                              }).ToList();

                    foreach (var q in q3)
                    {
                        var days = new TimeSpan(DateTime.Today.Ticks - q.NOTIFYTIME.AddDays(3).Ticks).Days;

                        bool isDelay = DateTime.Compare(q.ESTCALDATE, DateTime.Today) > 0;

                        CalibrationFormDataAccessor.SendRemindMail(null, q.UNIQUEID, q.VHNO, days, isDelay);
                    }
                    #endregion
                }

                if (SendType == "4")
                {
                    #region MSANotify
                    var q4 = (from x in db.QA_MSANOTIFYFLOWLOG
                              join n in db.QA_MSANOTIFY
                              on x.NOTIFYUNIQUEID equals n.UNIQUEID
                              where !x.VERIFYTIME.HasValue && x.ISCANCELED == "N" && n.STATUS == "1" && DateTime.Compare(x.NOTIFYTIME, baseDay) <= 0
                              select new
                              {
                                  n.UNIQUEID,
                                  n.VHNO,
                                  x.USERID,
                                  x.NOTIFYTIME,
                                  n.ESTMSADATE
                              }).ToList();

                    foreach (var q in q4)
                    {
                        var days = new TimeSpan(DateTime.Today.Ticks - q.NOTIFYTIME.AddDays(3).Ticks).Days;

                        var isDelay = false;

                        if (q.ESTMSADATE.HasValue)
                        {
                            isDelay = DateTime.Compare(q.ESTMSADATE.Value, DateTime.Today) > 0;
                        }

                        if (!string.IsNullOrEmpty(q.USERID))
                        {
                            MSANotifyDataAccessor.SendRemindMail(q.UNIQUEID, days, q.USERID, isDelay);
                        }
                        else
                        {
                            MSANotifyDataAccessor.SendRemindMail(q.UNIQUEID, days, isDelay);
                        }
                    }
                    #endregion
                }

                if (SendType == "5")
                {
                    #region MSAForm
                    var q5 = (from x in db.QA_MSANOTIFYFLOWLOG
                              join f in db.QA_MSAFORM
                              on x.NOTIFYUNIQUEID equals f.UNIQUEID
                              where !x.VERIFYTIME.HasValue && x.ISCANCELED == "N" && f.STATUS == "3" && DateTime.Compare(x.NOTIFYTIME, baseDay) <= 0
                              select new
                              {
                                  f.UNIQUEID,
                                  f.VHNO,
                                  x.NOTIFYTIME,
                                  f.ESTMSADATE
                              }).ToList();

                    foreach (var q in q5)
                    {
                        var days = new TimeSpan(DateTime.Today.Ticks - q.NOTIFYTIME.AddDays(3).Ticks).Days;

                        var isDelay = false;

                        if (q.ESTMSADATE.HasValue)
                        {
                            isDelay = DateTime.Compare(q.ESTMSADATE.Value, DateTime.Today) > 0;
                        }

                        MSAForm_v2DataAccessor.SendRemindMail(null, q.UNIQUEID, q.VHNO, days, isDelay);
                    }
                    #endregion
                }

                //#region AbnormalForm
                //var q6 = (from x in db.QA_ABNORMALFORMFLOWLOG
                //          join f in db.QA_ABNORMALFORM
                //          on x.FORMUNIQUEID equals f.UNIQUEID
                //          where !x.VERIFYTIME.HasValue && x.ISCANCELED == "N" && f.STATUS == "1" && DateTime.Compare(x.NOTIFYTIME, baseDay) <= 0
                //          select new
                //          {
                //              f.UNIQUEID,
                //              f.VHNO,
                //              x.USERID
                //          }).ToList();

                //foreach (var q in q4)
                //{
                //    Logger.Log(string.Format("AbnormalForm[{0}][{1}]", q.VHNO, q.USERID));

                //    if (!string.IsNullOrEmpty(q.USERID))
                //    {
                //        AbnormalFormDataAccessor.SendVerifyMail(q.UNIQUEID, q.USERID);
                //    }
                //    else
                //    {
                //        AbnormalFormDataAccessor.SendVerifyMail(q.UNIQUEID);
                //    }
                //}
                //#endregion

                if (SendType == "6")
                {
                    #region ChangeForm
                    var q7 = (from x in db.QA_CHANGEFORMFLOWLOG
                              join f in db.QA_CHANGEFORM
                              on x.FORMUNIQUEID equals f.UNIQUEID
                              where !x.VERIFYTIME.HasValue && x.ISCANCELED == "N" && f.STATUS == "1" && DateTime.Compare(x.NOTIFYTIME.Value, baseDay) <= 0
                              select new
                              {
                                  f.UNIQUEID,
                                  f.VHNO,
                                  x.USERID,
                                  x.NOTIFYTIME
                              }).ToList();

                    foreach (var q in q7)
                    {
                        var days = new TimeSpan(DateTime.Today.Ticks - q.NOTIFYTIME.Value.AddDays(3).Ticks).Days;

                        if (!string.IsNullOrEmpty(q.USERID))
                        {
                            ChangeFormDataAccessor.SendRemindMail(q.UNIQUEID, q.VHNO, days, q.USERID);
                        }
                        else
                        {
                            ChangeFormDataAccessor.SendRemindMail(q.UNIQUEID, q.VHNO, days);
                        }
                    }
                    #endregion
                }
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

        ~Reminder()
        {
            Dispose(false);
        }

        #endregion
    }
}
