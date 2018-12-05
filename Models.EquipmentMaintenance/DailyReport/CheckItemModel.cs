using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EquipmentMaintenance.DailyReport
{
    public class CheckItemModel
    {
        public int No { get; set; }

        public bool IsFeelItem { get; set; }

        public string ControlPointDescription { get; set; }

        public string EquipmentName { get; set; }

        public string PartDescription { get; set; }

        public string CheckItemDescription { get; set; }

        public string Description
        {
            get
            {
                if (!string.IsNullOrEmpty(EquipmentName))
                {
                    if (!string.IsNullOrEmpty(PartDescription))
                    {
                        return string.Format("{0}-{1} {2}", EquipmentName, PartDescription, CheckItemDescription);
                    }
                    else
                    {
                        return string.Format("{0} {1}", EquipmentName, CheckItemDescription);
                    }
                }
                else
                {
                    return string.Format("{0} {1}", ControlPointDescription, CheckItemDescription);
                }
            }
        }

        public double? LowerLimit { get; set; }

        public double? LowerAlertLimit { get; set; }

        public double? UpperAlertLimit { get; set; }

        public double? UpperLimit { get; set; }

        public string Limit
        {
            get
            {
                if (!IsFeelItem)
                {
                    if (LowerLimit.HasValue && UpperLimit.HasValue)
                    {
                        return string.Format("{0}~{1}", LowerLimit.Value, UpperLimit.Value);
                    }
                    else if (!LowerLimit.HasValue && UpperLimit.HasValue)
                    {
                        return string.Format("<{0}", UpperLimit.Value);
                    }
                    else if (LowerLimit.HasValue && !UpperLimit.HasValue)
                    {
                        return string.Format(">{0}", LowerLimit.Value);
                    }
                    else if (LowerAlertLimit.HasValue && UpperAlertLimit.HasValue)
                    {
                        return string.Format("{0}~{1}", LowerAlertLimit.Value, UpperAlertLimit.Value);
                    }
                    else if (!LowerAlertLimit.HasValue && UpperAlertLimit.HasValue)
                    {
                        return string.Format("<{0}", UpperAlertLimit.Value);
                    }
                    else if (LowerAlertLimit.HasValue && !UpperAlertLimit.HasValue)
                    {
                        return string.Format(">{0}", LowerAlertLimit.Value);
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    return FeelOptions;
                }
            }
        }

        public string Unit { get; set; }

        public string CheckItemRemark { get; set; }

        public Dictionary<string, List<CheckResultModel>> ResultList { get; set; }

        public Dictionary<string, string> Result
        {
            get
            {
                var result = new Dictionary<string, string>();

                foreach (var job in ResultList)
                {
                    if (job.Value != null)
                    {
                        if (job.Value.Count > 0)
                        {
                            var sb = new StringBuilder();

                            foreach (var q in job.Value)
                            {
                                sb.Append(q.Result);
                                sb.Append("/");
                            }

                            sb.Remove(sb.Length - 1, 1);

                            result.Add(job.Key, sb.ToString());
                        }
                        else
                        {
                            result.Add(job.Key, string.Empty);
                        }
                    }
                    else
                    {
                        result.Add(job.Key, "-");
                    }
                }

                return result;
            }
        }

        public string Remark
        {
            get
            {
                var sb = new StringBuilder();

                if (!string.IsNullOrEmpty(CheckItemRemark))
                {
                    sb.AppendLine(CheckItemRemark);
                }

                foreach (var job in ResultList)
                {
                    if (job.Value != null && job.Value.Count > 0)
                    {
                        foreach (var r in job.Value)
                        {
                            sb.AppendLine(r.AbnormalReasons);
                        }
                    }
                }

                return sb.ToString();
            }
        }

        public List<string> FeelOptionDescriptionList { get; set; }

        public string FeelOptions
        {
            get
            {
                if (FeelOptionDescriptionList != null && FeelOptionDescriptionList.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (var feelOption in FeelOptionDescriptionList)
                    {
                        sb.Append(feelOption);

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

        public CheckItemModel()
        {
            ResultList = new Dictionary<string, List<CheckResultModel>>();
        }
    }
}
