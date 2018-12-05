using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.Home
{
    public class MaintenanceFormVerifyItem
    {
        public string UniqueID { get; set; }

        public string VHNO { get; set; }

        public string OrganizationDescription { get; set; }

        public string Subject { get; set; }

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
                    return string.Format("{0}/{1}", EquipmentID, EquipmentName);
                }
            }
        }

        public DateTime? CycleBeginDate { get; set; }

        public string CycleBeginDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(CycleBeginDate);
            }
        }

        public DateTime? CycleEndDate { get; set; }

        public string CycleEndDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(CycleEndDate);
            }
        }

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

        public DateTime? BeginDate { get; set; }

        public string BeginDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(BeginDate);
            }
        }

        public DateTime? EndDate { get; set; }

        public string EndDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(EndDate);
            }
        }

        public List<UserModel> MaintenanceUserList { get; set; }

        public string MaintenanceUser
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                if (MaintenanceUserList.Count > 0)
                {
                    foreach (var user in MaintenanceUserList)
                    {
                        sb.Append(user.User);
                        sb.Append("、");
                    }

                    sb.Remove(sb.Length - 1, 1);
                }

                return sb.ToString();
            }
        }
    }
}
