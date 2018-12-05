using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.Home
{
    public class RepairFormVerifyItem
    {
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
