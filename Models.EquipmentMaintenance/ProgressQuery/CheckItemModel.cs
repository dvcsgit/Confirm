using System.Linq;
using System.Collections.Generic;

namespace Models.EquipmentMaintenance.ProgressQuery
{
    public class CheckItemModel
    {
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

        public string CheckItemID { get; set; }

        public string CheckItemDescription { get; set; }

        public string CheckItem
        {
            get
            {
                return string.Format("{0}/{1}", CheckItemID, CheckItemDescription);
            }
        }

#if ORACLE
        public decimal? LowerLimit { get; set; }

        public decimal? LowerAlertLimit { get; set; }

        public decimal? UpperAlertLimit { get; set; }

        public decimal? UpperLimit { get; set; }
#else
        public double? LowerLimit { get; set; }

        public double? LowerAlertLimit { get; set; }

        public double? UpperAlertLimit { get; set; }

        public double? UpperLimit { get; set; }
#endif

        public string Unit { get; set; }

        public List<CheckResultModel> CheckResultList { get; set; }

        public bool HaveAbnormal
        {
            get
            {
                return CheckResultList.Any(x => x.IsAbnormal);
            }
        }

        public bool HaveAlert
        {
            get
            {
                return CheckResultList.Any(x => x.IsAlert);
            }
        }

        public bool IsChecked
        {
            get
            {
                return CheckResultList.Count > 0;
            }
        }

        public CheckItemModel()
        {
            CheckResultList = new List<CheckResultModel>();
        }
    }
}
