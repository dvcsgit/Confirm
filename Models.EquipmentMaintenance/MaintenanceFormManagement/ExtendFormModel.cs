using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.EquipmentMaintenance.MaintenanceFormManagement
{
    public class ExtendFormModel
    {
        public string FormUniqueID { get; set; }

        [Display(Name = "VHNO", ResourceType = typeof(Resources.Resource))]
        public string VHNO { get; set; }

        [Display(Name = "Subject", ResourceType = typeof(Resources.Resource))]
        public string Subject { get; set; }

        public string EquipmentID { get; set; }

        public string EquipmentName { get; set; }

        public string PartDescription { get; set; }

        [Display(Name = "Equipment", ResourceType = typeof(Resources.Resource))]
        public string Equipment
        {
            get
            {
                if (!string.IsNullOrEmpty(PartDescription))
                {
                    return string.Format("{0}/{1}-{2}", EquipmentID, EquipmentName, PartDescription);
                }
                else
                {
                    return string.Format("{0}/{1}", EquipmentID, EquipmentName);
                }
            }
        }

        [Display(Name = "CycleBeginDate", ResourceType = typeof(Resources.Resource))]
        public string OBeginDateString { get; set; }

        public DateTime OBeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(OBeginDateString).Value;
            }
        }

        [Display(Name = "CycleEndDate", ResourceType = typeof(Resources.Resource))]
        public string OEndDateString { get; set; }

        public DateTime OEndDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(OEndDateString).Value;
            }
        }
        public ExtendFormInput FormInput { get; set; }
    }
}
