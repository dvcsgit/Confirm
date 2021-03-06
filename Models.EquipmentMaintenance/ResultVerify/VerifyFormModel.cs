﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;

namespace Models.EquipmentMaintenance.ResultVerify
{
   public class VerifyFormModel
    {
       public VerifyFormInput FormInput { get; set; }

       public List<SelectListItem> UnPatrolReasonSelectItemList { get; set; }

        public string UniqueID { get; set; }

        public string ParentOrganizationFullDescription { get; set; }

        public string Description { get; set; }

        public string BeginDate { get; set; }

        public string EndDate { get; set; }

        public string BeginTime { get; set; }

        public string EndTime { get; set; }

        public string JobUsers { get; set; }

        public string CheckUsers { get; set; }

        public string CompleteRate { get; set; }

        public string CompleteRateLabelClass { get; set; }

        public string UnPatrolReason { get; set; }

        public string ArriveStatus { get; set; }

        public string ArriveStatusLabelClass { get; set; }

        public string OverTimeReason { get; set; }

        public string TimeSpan { get; set; }

        private DateTime? ExecBeginDateTime
        {
            get
            {
                var q = ControlPointList.Where(x => x.ArriveDateTime.HasValue).OrderBy(x => x.ArriveDateTime).FirstOrDefault();

                if (q != null)
                {
                    return q.ArriveDateTime;
                }
                else
                {
                    return null;
                }
            }
        }

        public string ExecBeginDateTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(ExecBeginDateTime);
            }
        }

        private DateTime? ExecEndDateTime
        {
            get
            {
                var q = ControlPointList.Where(x => x.FinishedDateTime.HasValue).OrderByDescending(x => x.FinishedDateTime).FirstOrDefault();

                if (q != null)
                {
                    return q.FinishedDateTime;
                }
                else
                {
                    return null;
                }
            }
        }

        public string ExecEndDateTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(ExecEndDateTime);
            }
        }

        private int ExecTotalSeconds
        {
            get
            {
                if (ExecBeginDateTime.HasValue && ExecEndDateTime.HasValue)
                {
                    return Convert.ToInt32((ExecEndDateTime.Value - ExecBeginDateTime.Value).TotalSeconds);
                }
                else
                {
                    return 0;
                }
            }
        }

        public string ExecTimeSpan
        {
            get
            {
                if (ExecTotalSeconds == 0)
                {
                    return "-";
                }
                else
                {
                    var ts = new TimeSpan(0, 0, ExecTotalSeconds);

                    return ts.Hours.ToString().PadLeft(2, '0') + ":" + ts.Minutes.ToString().PadLeft(2, '0') + ":" + ts.Seconds.ToString().PadLeft(2, '0');
                }
            }
        }

        public List<ControlPointModel> ControlPointList { get; set; }

        public List<ControlPointModel> TimeSpanAbnormalList
        {
            get
            {
                return ControlPointList.Where(x => x.IsTimeSpanAbnormal).ToList();
            }
        }

        public List<ControlPointModel> UnRFIDList
        {
            get
            {
                return ControlPointList.Where(x => x.IsUnRFID).ToList();
            }
        }

        public List<CheckItemModel> AbnormalList
        {
            get
            {
                return ControlPointList.SelectMany(x => x.AbnormalList).ToList();
            }
        }

        public List<FlowLogModel> FlowLogList { get; set; }

        public VerifyFormModel()
        {
            ControlPointList = new List<ControlPointModel>();
            FlowLogList = new List<FlowLogModel>();
            UnPatrolReasonSelectItemList = new List<SelectListItem>() 
            { 
             Define.DefaultSelectListItem(Resources.Resource.SelectOne)
            };
        }
    }
}
