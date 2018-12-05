using DbEntity.ASE;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace ASE.FEM2MES
{
    public class TransHelper : IDisposable
    {
        public void Trans()
        {
            using (ASEDbEntities db = new ASEDbEntities())
            {
                var resultList = (from sr in db.MFORMSTANDARDRESULT
                                  join r in db.MFORMRESULT
                                  on sr.RESULTUNIQUEID equals r.UNIQUEID
                                  join f in db.MFORM
                                  on r.MFORMUNIQUEID equals f.UNIQUEID
                                  join s in db.STANDARD
                                  on sr.STANDARDUNIQUEID equals s.UNIQUEID
                                  join b in db.FWCATNS_PMBUYOFFLIST
                                  on new { s.MAINTENANCETYPE, s.ID } equals new { MAINTENANCETYPE = b.MODEL, ID = b.BUYOFFNO }
                                  select new
                                  {
                                      sr.UNIQUEID,
                                      FORMUNIQUEID = f.UNIQUEID,
                                      f.STATUS,
                                      r.PMDATE,
                                      r.PMTIME,
                                      r.USERID,
                                      b.BUYOFFNO,
                                      sr.EQUIPMENTID,
                                      sr.RESULT,
                                      b.PMTYPE,
                                      b.PLANT,
                                      b.AREA,
                                      b.SEQNO,
                                      b.ITEM,
                                      b.CRITERIA,
                                      b.SAMPLESIZE,
                                      b.UNIT,
                                      b.AUTOMOTIVE,
                                      f.ESTBEGINDATE
                                  }).ToList();

                DateTimeFormatInfo dateTimeInfo = DateTimeFormatInfo.CurrentInfo;

                foreach (var r in resultList)
                {
                    var q = db.FWCATNS_PMCHECKLIST_PN2M.FirstOrDefault(x => x.UNIQUEID == r.UNIQUEID);

                    var flowLogList = db.MFORMFLOWLOG.Where(x => x.MFORMUNIQUEID == r.FORMUNIQUEID).ToList();

                    var submitTime = string.Empty;
                    //SubmitTime
                    if (r.STATUS == "3" || r.STATUS == "5")
                    {
                        if (flowLogList.Count > 0)
                        {
                            var minFlowSeq = flowLogList.Min(x => x.FLOWSEQ);

                            var submit = flowLogList.Where(x => x.FLOWSEQ == minFlowSeq).OrderByDescending(x => x.NOTIFYTIME).FirstOrDefault();

                            if (submit != null)
                            {
                                submitTime = submit.NOTIFYTIME.ToString("yyyyMMdd HHmmss") + "000";
                            }
                        }
                    }

                    var verifyTime = string.Empty;
                    var verifyUserID = string.Empty;
                    //VerifyTime, VerifyUserID
                    if (r.STATUS == "5")
                    {
                        if (flowLogList.Count > 0)
                        {
                            var verify = flowLogList.OrderByDescending(x => x.SEQ).First();

                            verifyUserID = verify.USERID;
                            if (verify.VERIFYTIME.HasValue)
                            {
                                verifyTime = verify.VERIFYTIME.Value.ToString("yyyyMMdd HHmmss") + "000";
                            }
                        }
                    }

                    if (q != null)
                    {
                        q.SUBMITTIME = submitTime;
                        q.VERIFYUSERID = verifyUserID;
                        q.VERIFYTIME = verifyTime;
                    }
                    else
                    {
                        var date = DateTimeHelper.DateString2DateTime(r.PMDATE);

                        var checkTime = DateTimeHelper.DateTimeString2DateTime(r.PMDATE, r.PMTIME);

                        var estWeek = string.Format("W{0}", dateTimeInfo.Calendar.GetWeekOfYear(r.ESTBEGINDATE, dateTimeInfo.CalendarWeekRule, dateTimeInfo.FirstDayOfWeek).ToString().PadLeft(2, '0'));
                        var week = string.Format("W{0}", dateTimeInfo.Calendar.GetWeekOfYear(checkTime.Value, dateTimeInfo.CalendarWeekRule, dateTimeInfo.FirstDayOfWeek).ToString().PadLeft(2, '0'));

                        var checkTimeString = string.Empty;

                        if (checkTime.HasValue)
                        {
                            checkTimeString = checkTime.Value.ToString("yyyyMMdd HHmmss") + "000";
                        }

                        db.FWCATNS_PMCHECKLIST_PN2M.Add(new FWCATNS_PMCHECKLIST_PN2M()
                        {
                            UNIQUEID = r.UNIQUEID,
                            CHECKNO = r.BUYOFFNO,
                            EQPID = r.EQUIPMENTID,
                            RESULT = r.RESULT,
                            PMPERIOD = r.PMTYPE,
                            INITIALTIME = null,
                            UPDATETIME = null,
                            EDITOR = null,
                            ACTPERIOD = week,
                            SYSID = null,
                            PLANT = r.PLANT,
                            AREA = r.AREA,
                            SEQNO = r.SEQNO,
                            ITEM = r.ITEM,
                            REQUIREMENT = r.CRITERIA,
                            ACTION = r.SAMPLESIZE,
                            UNIT = r.UNIT,
                            CURRENTYEAR = date.Value.Year.ToString(),
                            AUTOMOTIVE = r.AUTOMOTIVE,
                            CHECKTIME = checkTimeString,
                            CHECKUSERID = r.USERID,
                            SUBMITTIME = submitTime,
                            VERIFYUSERID = verifyUserID,
                            VERIFYTIME = verifyTime,
                            MK = null,
                            ESTPERIOD = estWeek
                        });
                    }
                }

                db.SaveChanges();
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

        ~TransHelper()
        {
            Dispose(false);
        }

        #endregion
    }
}
