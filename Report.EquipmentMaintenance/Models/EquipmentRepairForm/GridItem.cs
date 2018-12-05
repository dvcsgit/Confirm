using System;
using Utility;
namespace Report.EquipmentMaintenance.Models.EquipmentRepairForm
{
    public class GridItem
    {
        public string UniqueID { get; set; }

        public int Status
        {
            get
            {
                //已結案
                if (IsClosed)
                {
                    return 7;
                }
                //取消立案
                else if (!string.IsNullOrEmpty(RefuseReason))
                {
                    return 2;
                }
                //未派工
                else if (!JobTime.HasValue || JobTime.HasValue && !string.IsNullOrEmpty(JobRefuseReason))
                {
                    return 1;
                }
                //未接案
                else if (JobTime.HasValue && !TakeJobTime.HasValue)
                {
                    return 3;
                }
                //修復完成覆核中
                else if (IsSubmit && !IsClosed)
                {
                    return 6;
                }
                //修復中
                else if (TakeJobTime.HasValue)
                {
                    //逾期
                    if (DateTime.Compare(DateTime.Today, EstEndDate.Value) > 0)
                    {
                        return 5;
                    }
                    else
                    {
                        return 4;
                    }
                }
                else
                {
                    return 0;
                }
            }
        }

        public string StatusDescription
        {
            get
            {
                switch (Status)
                { 
                    case 1:
                        return Resources.Resource.RFormStatus_1;
                    case 2:
                        return Resources.Resource.RFormStatus_2;
                    case 3:
                        return Resources.Resource.RFormStatus_3;
                    case 4:
                        return Resources.Resource.RFormStatus_4;
                    case 5:
                        return Resources.Resource.RFormStatus_5;
                    case 6:
                        return Resources.Resource.RFormStatus_6;
                    case 7:
                        return Resources.Resource.RFormStatus_7;
                    default:
                        return "-";
                }
            }
        }

        public string OrganizationDescription { get; set; }

        public string MaintenanceOrganizationDescription { get; set; }

        public string VHNO { get; set; }

        public bool IsSubmit { get; set; }

        public string EquipmentID { get; set; }

        public string EquipmentName { get; set; }

        public string PartDescription { get; set; }

        public string Equipment
        {
            get
            {
                if (!string.IsNullOrEmpty(EquipmentID))
                {
                    if (string.IsNullOrEmpty(PartDescription))
                    {
                        return string.Format("{0}/{1}", EquipmentID, EquipmentName);
                    }
                    else
                    {
                        return string.Format("{0}/{1}-{2}", EquipmentID, EquipmentName, PartDescription);
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string Subject { get; set; }

        public string RepairFormType { get; set; }

        //public string CreateUserID { get; set; }

        //public string CreateUserName { get; set; }

        //public string CreateUser
        //{
        //    get
        //    {
        //        if (!string.IsNullOrEmpty(CreateUserName))
        //        {
        //            return string.Format("{0}/{1}", CreateUserID, CreateUserName);
        //        }
        //        else
        //        {
        //            return CreateUserID;
        //        }
        //    }
        //}

        //public DateTime CreateTime { get; set; }

        //public string CreateTimeString
        //{
        //    get
        //    {
        //        return DateTimeHelper.DateTime2DateTimeStringWithSeperator(CreateTime);
        //    }
        //}

        //public string JobManagerID { get; set; }

        //public string JobManagerName { get; set; }

        //public string JobManager
        //{
        //    get
        //    {
        //        if (!string.IsNullOrEmpty(JobManagerName))
        //        {
        //            return string.Format("{0}/{1}", JobManagerID, JobManagerName);
        //        }
        //        else
        //        {
        //            return JobManagerID;
        //        }
        //    }
        //}

        public string RefuseReason { get; set; }

        public DateTime? JobTime { get; set; }

        //public string JobTimeString
        //{
        //    get
        //    {
        //        return DateTimeHelper.DateTime2DateTimeStringWithSeperator(JobTime);
        //    }
        //}

        public string JobUserID { get; set; }

        //public string JobUserName { get; set; }

        //public string JobUser
        //{
        //    get
        //    {
        //        if (!string.IsNullOrEmpty(JobUserName))
        //        {
        //            return string.Format("{0}/{1}", JobUserID, JobUserName);
        //        }
        //        else
        //        {
        //            return JobUserID;
        //        }
        //    }
        //}

        public string JobRefuseReason { get; set; }

        public DateTime? TakeJobTime { get; set; }

        //public string TakeJobTimeString
        //{
        //    get
        //    {
        //        return DateTimeHelper.DateTime2DateTimeStringWithSeperator(TakeJobTime);
        //    }
        //}

        public DateTime? EstBeginDate { get; set; }

        public string EstBeginDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(EstBeginDate);
            }
        }

        public DateTime? EstEndDate { get; set; }

        public string EstEndDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(EstEndDate);
            }
        }

        public bool IsClosed { get; set; }
    }
}
