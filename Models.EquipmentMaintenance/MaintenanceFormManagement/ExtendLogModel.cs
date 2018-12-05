using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.EquipmentMaintenance.MaintenanceFormManagement
{
    public class ExtendLogModel
    {
        public int Seq { get; set; }

        public DateTime OBeginDate { get; set; }

        public string OBeginDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(OBeginDate);
            }
        }

        public DateTime OEndDate { get; set; }

        public string OEndDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(OEndDate);
            }
        }

        public DateTime NBeginDate { get; set; }

        public string NBeginDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(NBeginDate);
            }
        }

        public DateTime NEndDate { get; set; }

        public string NEndDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(NEndDate);
            }
        }

        public DateTime CreateTime { get; set; }

        public string CreateTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(CreateTime);
            }
        }

        public string Reason { get; set; }

        public bool IsClosed { get; set; }

        public List<ExtendFlowLogModel> LogList { get; set; }

        public string Result
        {
            get
            {
                if (LogList.Count > 0)
                {
                    return LogList.OrderByDescending(x => x.Seq).First().Result;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public ExtendLogModel()
        {
            LogList = new List<ExtendFlowLogModel>();
        }
    }
}
