using DataAccess.ASE;
using DbEntity.ASE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace JobResultGenerator.ASE
{
    public class Generator : IDisposable
    {
        private DateTime BaseDate = DateTime.Today;

        public void Generate()
        {
            var total = 0;
            var generate = 0;
            var success = 0;
            var failed = 0;

            using (ASEDbEntities db = new ASEDbEntities())
            {
                var jobList = db.JOB.Where(x => !x.ENDDATE.HasValue || DateTime.Compare(x.ENDDATE.Value, DateTime.Today) >= 0).ToList();

                total = jobList.Count;

                foreach (var job in jobList)
                {
                    if (JobCycleHelper.IsInCycle(BaseDate, job.BEGINDATE.Value, job.ENDDATE, job.CYCLECOUNT.Value, job.CYCLEMODE))
                    {
                        generate++;

                        if (Generate(job))
                        {
                            success++;
                        }
                        else
                        {
                            failed++;
                        }
                    }
                }

                Logger.Log(string.Format("{0} Job, {1} to Generate, {2} Success, {3} Failed", total, generate, success, failed));
            }
        }

        private bool Generate(JOB Job)
        {
            bool result = false;

            try
            {
                DateTime begin, end;

                JobCycleHelper.GetDateSpan(BaseDate, Job.BEGINDATE.Value, Job.ENDDATE, Job.CYCLECOUNT.Value, Job.CYCLEMODE, out begin, out end);

                var beginString = DateTimeHelper.DateTime2DateString(begin);
                var endString = DateTimeHelper.DateTime2DateString(end);

                using (ASEDbEntities db = new ASEDbEntities())
                {
                    var jobResult = db.JOBRESULT.FirstOrDefault(x => x.JOBUNIQUEID == Job.UNIQUEID && x.BEGINDATE == beginString && x.ENDDATE == endString);

                    if (jobResult == null)
                    {
                        JobResultDataAccessor.Refresh(Guid.NewGuid().ToString(), Job.UNIQUEID, beginString, endString);
                    }
                }

                result = true;
            }
            catch (Exception ex)
            {
                result = false;

                Logger.Log(string.Format("Generate Failed : {0}", Job.UNIQUEID));

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return result;
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
