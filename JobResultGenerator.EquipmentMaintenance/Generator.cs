using DataAccess.EquipmentMaintenance;
using DbEntity.MSSQL.EquipmentMaintenance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace JobResultGenerator.EquipmentMaintenance
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

            using (EDbEntities db = new EDbEntities())
            {
                var jobList = db.Job.Where(x => !x.EndDate.HasValue || DateTime.Compare(x.EndDate.Value, DateTime.Today) >= 0).ToList();

                total = jobList.Count;

                foreach (var job in jobList)
                {
                    if (JobCycleHelper.IsInCycle(BaseDate, job.BeginDate, job.EndDate, job.CycleCount, job.CycleMode))
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

        private bool Generate(Job Job)
        {
            bool result = false;

            try
            {
                DateTime begin, end;

                JobCycleHelper.GetDateSpan(BaseDate, Job.BeginDate, Job.EndDate, Job.CycleCount, Job.CycleMode, out begin, out end);

                var beginString = DateTimeHelper.DateTime2DateString(begin);
                var endString = DateTimeHelper.DateTime2DateString(end);

                if (!string.IsNullOrEmpty(Job.BeginTime) && !string.IsNullOrEmpty(Job.EndTime) && string.Compare(Job.BeginTime, Job.EndTime) > 0)
                {
                    endString = DateTimeHelper.DateTime2DateString(end.AddDays(1));
                }

                using (EDbEntities db = new EDbEntities())
                {
                    var jobResult = db.JobResult.FirstOrDefault(x => x.JobUniqueID == Job.UniqueID && x.BeginDate == beginString && x.EndDate == endString);

                    if (jobResult == null)
                    {
                        JobResultDataAccessor.Refresh(Guid.NewGuid().ToString(), Job.UniqueID, beginString, endString);
                    }
                }

                result = true;
            }
            catch (Exception ex)
            {
                result = false;

                Logger.Log(string.Format("Generate Failed : {0}", Job.UniqueID));

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
