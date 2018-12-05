using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.AbnormalNotify
{
    public class RepairFormModel
    {
        public string UniqueID { get; set; }

        public string Status { get; set; }

        public string StatusCode
        {
            get
            {
                if (Status == "4")
                {
                    if (DateTime.Compare(DateTime.Today, EstEndDate.Value) > 0)
                    {
                        return "5";
                    }
                    else
                    {
                        return "4";
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
                switch (StatusCode)
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

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        [Display(Name = "MaintenanceOrganization", ResourceType = typeof(Resources.Resource))]
        public string MaintenanceOrganizationDescription { get; set; }

        [Display(Name = "Subject", ResourceType = typeof(Resources.Resource))]
        public string Subject { get; set; }

        [Display(Name = "Description", ResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }

        [Display(Name = "VHNO", ResourceType = typeof(Resources.Resource))]
        public string VHNO { get; set; }

        public string EquipmentID { get; set; }

        public string EquipmentName { get; set; }

        public string PartDescription { get; set; }

        [Display(Name = "Equipment", ResourceType = typeof(Resources.Resource))]
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

        [Display(Name = "RepairFormType", ResourceType = typeof(Resources.Resource))]
        public string RepairFormType { get; set; }

        public DateTime? EstBeginDate { get; set; }

        [Display(Name = "MaintenanceBeginDate", ResourceType = typeof(Resources.Resource))]
        public string EstBeginDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(EstBeginDate);
            }
        }

        public DateTime? EstEndDate { get; set; }

        [Display(Name = "MaintenanceEndDate", ResourceType = typeof(Resources.Resource))]
        public string EstEndDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(EstEndDate);
            }
        }
    }
}
