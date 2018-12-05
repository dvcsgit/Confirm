using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Utility;

namespace WebApi.CHIMEI.E.Models
{
    public class JobItem
    {
        public string JobUniqueID { get; set; }

        public string JobDescription { get; set; }

        public string RouteID { get; set; }

        public string RouteName { get; set; }

        public string Description
        {
            get
            {
                return string.Format("{0}/{1}-{2}", RouteID, RouteName, JobDescription);
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

        public string BeginTime { get; set; }

        public DateTime? EndDate { get; set; }

        public string EndDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(EndDate);
            }
        }

        public string EndTime { get; set; }

        public string CompleteRate { get; set; }

        public double CheckItemCount { get; set; }

        public double CheckedItemCount { get; set; }

        public string RepairFormUniqueID { get; set; }

        public string FormType { get; set; }

        public string Subject { get; set; }

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

        public string MaintanenceFormUniqueID { get; set; }
    }
}