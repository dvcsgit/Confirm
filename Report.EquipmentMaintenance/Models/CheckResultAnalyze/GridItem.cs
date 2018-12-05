using System.Collections.Generic;
using System.Text;
namespace Report.EquipmentMaintenance.Models.CheckResultAnalyze
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

        public string RouteID { get; set; }

        public string RouteName { get; set; }

        public string JobDescription { get; set; }

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

        public string ControlPointID { get; set; }

        public string ControlPointDescription { get; set; }

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

        public string EquipmentID { get; set; }

        public string EquipmentName { get; set; }

        public string PartDescription { get; set; }

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

        public string CheckItemID { get; set; }

        public string CheckItemDescription { get; set; }

#if ORACLE
        public decimal? UpperLimit { get; set; }

        public decimal? UpperAlertLimit { get; set; }

        public decimal? LowerAlertLimit { get; set; }

        public decimal? LowerLimit { get; set; }
#else
        public double? UpperLimit { get; set; }

        public double? UpperAlertLimit { get; set; }

        public double? LowerAlertLimit { get; set; }

        public double? LowerLimit { get; set; }
#endif

        public string Result { get; set; }

        public List<AbnormalReasonModel> AbnormalReasonList { get; set; }

        public string AbnormalReasons
        {
            get
            {
                if (AbnormalReasonList != null && AbnormalReasonList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var abnormalReason in AbnormalReasonList)
                    {
                        sb.Append(abnormalReason.Display);
                        sb.Append("、");
                    }

                    sb.Remove(sb.Length - 1, 1);

                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public bool IsAbnormal { get; set; }

        public string User
        {
            get
            {
                if (!string.IsNullOrEmpty(UserName))
                {
                    return string.Format("{0}/{1}", UserID, UserName);
                }
                else
                {
                    return UserID;
                }
            }
        }

        public string UserID { get; set; }

        public string UserName { get; set; }

        public string CheckDate { get; set; }

        public string CheckTime { get; set; }

        public string VHNO { get; set; }

        public GridItem()
        {
            AbnormalReasonList = new List<AbnormalReasonModel>();
        }
    }
}
