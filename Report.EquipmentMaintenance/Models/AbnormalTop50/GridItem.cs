using System.ComponentModel;

namespace Report.EquipmentMaintenance.Models.AbnormalTop50
{
    public class GridItem
    {
        public string Route
        {
            get
            {
                if (!string.IsNullOrEmpty(JobDescription))
                {
                    if (!string.IsNullOrEmpty(RouteName))
                    {
                        return string.Format("{0}/{1}-{2}", RouteID, RouteName, JobDescription);
                    }
                    else
                    {
                        return string.Format("{0}-{1}", RouteID, JobDescription);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(RouteName))
                    {
                        return string.Format("{0}/{1}", RouteID, RouteName);
                    }
                    else
                    {
                        return RouteID;
                    }
                }
            }
        }

        public string ControlPoint
        {
            get
            {
                if (!string.IsNullOrEmpty(ControlPointDescription))
                {
                    return string.Format("{0}/{1}", ControlPointID, ControlPointDescription);
                }
                else
                {
                    return ControlPointID;
                }
            }
        }

        public string Equipment
        {
            get
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
        }

        public string CheckItem
        {
            get
            {
                if (!string.IsNullOrEmpty(CheckItemDescription))
                {
                    return string.Format("{0}/{1}", CheckItemID, CheckItemDescription);
                }
                else
                {
                    return CheckItemID;
                }
            }
        }

        public int AbnormalCount { get; set; }

        public string RouteID { get; set; }

        public string RouteName { get; set; }

        public string JobDescription { get; set; }

        public string ControlPointID { get; set; }

        public string ControlPointDescription { get; set; }

        public string EquipmentID { get; set; }

        public string EquipmentName { get; set; }

        public string PartDescription { get; set; }

        public string CheckItemID { get; set; }

        public string CheckItemDescription { get; set; }
    }
}
