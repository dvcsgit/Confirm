using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.DataSync
{
    public class RepairFormModel
    {
        public string UniqueID { get; set; }

        public string VHNO { get; set; }

        public string EquipmentID { get; set; }

        public string EquipmentName { get; set; }

        public string PartDescription { get; set; }

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
                    if (!string.IsNullOrEmpty(EquipmentName))
                    {
                        return string.Format("{0}/{1}", EquipmentID, EquipmentName);
                    }
                    else
                    {
                        return EquipmentID;
                    }
                }
            }
        }
        public string Subject { get; set; }

        public string Description { get; set; }

        public string RepairFormType { get; set; }

        public DateTime? BeginDate { get; set; }

        public string BeginDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateString(BeginDate);
            }
        }

        public DateTime? EndDate { get; set; }

        public string EndDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateString(BeginDate);
            }
        }

        public List<UserModel> UserList { get; set; }

        public RepairFormModel()
        {
            UserList = new List<UserModel>();
        }
    }
}
