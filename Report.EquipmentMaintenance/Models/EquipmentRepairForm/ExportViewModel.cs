using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;
using System.Linq;

namespace Report.EquipmentMaintenance.Models.EquipmentRepairForm
{
    public class ExportViewModel
    {
        
      

        //public bool IsClosed { get; set; }

        //public bool IsSubmit { get; set; }

        //[Display(Name = "RefuseReason", ResourceType = typeof(Resources.Resource))]
        //public string RefuseReason { get; set; }

        //public DateTime? JobTime { get; set; }

        //[Display(Name = "JobTime", ResourceType = typeof(Resources.Resource))]
        //public string JobTimeString
        //{
        //    get
        //    {
        //        return DateTimeHelper.DateTime2DateTimeStringWithSeperator(JobTime);
        //    }
        //}

        //[Display(Name = "JobRefuseReason", ResourceType = typeof(Resources.Resource))]
        //public string JobRefuseReason { get; set; }

        //public DateTime? TakeJobTime { get; set; }

        //[Display(Name = "TakeJobTime", ResourceType = typeof(Resources.Resource))]
        //public string TakeJobTimeString
        //{
        //    get
        //    {
        //        return DateTimeHelper.DateTime2DateTimeStringWithSeperator(TakeJobTime);
        //    }
        //}

        //public DateTime? EstEndDate { get; set; }

        //[Display(Name = "MaintenanceEndDate", ResourceType = typeof(Resources.Resource))]
        //public string EstEndDateString
        //{
        //    get
        //    {
        //        return DateTimeHelper.DateTime2DateStringWithSeperator(EstEndDate);
        //    }
        //}

        //public int Status
        //{
        //    get
        //    {
        //        //已結案
        //        if (IsClosed)
        //        {
        //            return 7;
        //        }
        //        //取消立案
        //        else if (!string.IsNullOrEmpty(RefuseReason))
        //        {
        //            return 2;
        //        }
        //        //未派工
        //        else if (!JobTime.HasValue || JobTime.HasValue && !string.IsNullOrEmpty(JobRefuseReason))
        //        {
        //            return 1;
        //        }
        //        //未接案
        //        else if (JobTime.HasValue && !TakeJobTime.HasValue)
        //        {
        //            return 3;
        //        }
        //        //修復完成覆核中
        //        else if (IsSubmit && !IsClosed)
        //        {
        //            return 6;
        //        }
        //        //修復中
        //        else if (TakeJobTime.HasValue)
        //        {
        //            //逾期
        //            if (DateTime.Compare(DateTime.Today, EstEndDate.Value) > 0)
        //            {
        //                return 5;
        //            }
        //            else
        //            {
        //                return 4;
        //            }
        //        }
        //        else
        //        {
        //            return 0;
        //        }
        //    }
        //}

        //public string StatusDescription
        //{
        //    get
        //    {
        //        switch (Status)
        //        {
        //            case 1:
        //                return Resources.Resource.RFormStatus_1;
        //            case 2:
        //                return Resources.Resource.RFormStatus_2;
        //            case 3:
        //                return Resources.Resource.RFormStatus_3;
        //            case 4:
        //                return Resources.Resource.RFormStatus_4;
        //            case 5:
        //                return Resources.Resource.RFormStatus_5;
        //            case 6:
        //                return Resources.Resource.RFormStatus_6;
        //            case 7:
        //                return Resources.Resource.RFormStatus_7;
        //            default:
        //                return "-";
        //        }
        //    }
        //}

        public string EquipmentID { get; set; }

        public string EquipmentName { get; set; }

        public DateTime? PlanBeginDate { get; set; }

        [Display(Name = "PlanBeginDate", ResourceType = typeof(Resources.Resource))]
        public string PlanBeginDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(PlanBeginDate);
            }
        }

          [Display(Name = "PlanBeginDate", ResourceType = typeof(Resources.Resource))]
        
        public DateTime? PutDate { get; set; }
        public string PutDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(PutDate);
            }
        }

        public string StatusDescription { get; set; }

        public string ManagerUserID { get; set; }

        public string Engineer { get; set; }

        public string ImplementUser { get; set; }

        
        //public string CycleCount { get; set; }

        //public string CycleMode { get; set; }

        //[Display(Name = "Cycle", ResourceType = typeof(Resources.Resource))]
        //public string Cycle
        //{
        //    get
        //    {
        //        return string.Format("{0}{1}", CycleCount, CycleMode);                                       
        //    }
        //}

        //public string StandardPartDescription { get; set; }

        //public string StandardDescription { get; set; }

        //public string State { get; set; }

        //public string Manage { get; set; }

       


     

       

      

       

        

        

        

    }
}
