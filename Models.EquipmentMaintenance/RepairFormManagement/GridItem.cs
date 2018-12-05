using Models.Shared;
using System;
using System.Collections.Generic;
using Utility;
using System.Linq;

namespace Models.EquipmentMaintenance.RepairFormManagement
{
    public class GridItem
    {
        public string UniqueID { get; set; }

        public string Status { get; set; }

        public string StatusCode
        {
            get
            {
                if (Status == "4")
                {
                    if (DateTime.Compare(DateTime.Today, EstEndDate.Value) >= 0)
                    {
                        return "5";
                    }
                    else
                    {
                        return Status;
                    }
                }
                else
                {
                    return Status;
                }
            }
        }

        public string StatusDescription
        {
            get
            {
                switch (Status)
                { 
                    case "0":
                        return Resources.Resource.RFormStatus_0;
                    case "1":
                        return Resources.Resource.RFormStatus_1;
                    case "2":
                        return Resources.Resource.RFormStatus_2;
                    case "3":
                        return Resources.Resource.RFormStatus_3;
                    case "4":
                        return Resources.Resource.RFormStatus_4;
                    case "5":
                        return Resources.Resource.RFormStatus_5;
                    case "6":
                        return Resources.Resource.RFormStatus_6;
                    case "7":
                        return Resources.Resource.RFormStatus_7;
                    case "8":
                        return Resources.Resource.RFormStatus_8;
                    case "9":
                        return Resources.Resource.RFormStatus_9;
                    default:
                        return "-";
                }
            }
        }

        public string OrganizationDescription { get; set; }

        public string MaintenanceOrganizationDescription { get; set; }

        public string VHNO { get; set; }

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

        public string TakeJobUserID { get; set; }

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
    }
}
