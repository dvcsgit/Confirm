using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Models.EquipmentMaintenance.MaintenanceFormManagement
{
    public class StandardModel
    {
        public bool IsChecked
        {
            get
            {
                return ResultList.Count > 0;
            }
        }

        public bool IsAbnormal
        {
            get
            {
                return ResultList.Select(x => x.Result).ToList().Any(x => x.IsAbnormal);
            }
        }

        public bool IsAlert
        {
            get
            {
                return ResultList.Select(x => x.Result).ToList().Any(x => x.IsAlert);
            }
        }

        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public string Display
        {
            get
            {
                if (!string.IsNullOrEmpty(Description))
                {
                    return string.Format("{0}/{1}", ID, Description);
                }
                else
                {
                    return ID;
                }
            }
        }

        public bool IsFeelItem { get; set; }

        public List<FeelOptionModel> OptionList { get; set; }

        public string FeelOptions
        {
            get
            {
                if (OptionList != null && OptionList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var option in OptionList)
                    {
                        sb.Append(option.Display);
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

        public double? LowerLimit { get; set; }

        public double? LowerAlertLimit { get; set; }

        public double? UpperAlertLimit { get; set; }

        public double? UpperLimit { get; set; }

        public string Unit { get; set; }

        public List<ResultModel> ResultList { get; set; }

        public StandardModel()
        {
            OptionList = new List<FeelOptionModel>();
            ResultList = new List<ResultModel>();
        }
    }
}
