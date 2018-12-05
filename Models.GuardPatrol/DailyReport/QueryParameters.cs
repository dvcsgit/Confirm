using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.GuardPatrol.DailyReport
{
    public class QueryParameters
    {
        public bool IsOnlyChecked { get; set; }

        public bool IsOnlyAbnormal { get; set; }

        public string Jobs { get; set; }

        public List<JobParameters> JobList
        {
            get
            {
                if (!string.IsNullOrEmpty(Jobs))
                {
                    var jobList = new List<JobParameters>();

                    var jobs = JsonConvert.DeserializeObject<List<string>>(Jobs);

                    foreach (var job in jobs)
                    {
                        string[] t = job.Split(Define.Seperators, StringSplitOptions.None);

                        jobList.Add(new JobParameters()
                        {
                            OrganizationUniqueID = t[0],
                            JobUniqueID = t[1]
                        });
                    }

                    return jobList;
                }
                else
                {
                    return new List<JobParameters>();
                }
            }
        }

        public string BeginDateString { get; set; }

        public DateTime BeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(BeginDateString).Value;
            }
        }

        public string EndDateString { get; set; }

        public DateTime EndDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(EndDateString).Value;
            }
        }
    }
}
