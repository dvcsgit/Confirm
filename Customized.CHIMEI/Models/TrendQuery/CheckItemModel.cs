using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customized.CHIMEI.Models.TrendQuery
{
    public class CheckItemModel
    {
        public string CheckItemDescription { get; set; }

        public string Display
        {
            get
            {
                var limit = new StringBuilder();

                if (LowerLimit.HasValue)
                {
                    limit.Append(string.Format("下限值：{0}", LowerLimit.Value));
                    limit.Append("、");
                }

                if (LowerAlertLimit.HasValue)
                {
                    limit.Append(string.Format("下限警戒值：{0}", LowerAlertLimit.Value));
                    limit.Append("、");
                }

                if (UpperAlertLimit.HasValue)
                {
                    limit.Append(string.Format("上限警戒值：{0}", UpperAlertLimit.Value));
                    limit.Append("、");
                }

                if (UpperLimit.HasValue)
                {
                    limit.Append(string.Format("上限值：{0}", UpperLimit.Value));
                    limit.Append("、");
                }

                if (limit.Length > 0)
                {
                    limit.Remove(limit.Length - 1, 1);
                }

                if (limit.Length > 0)
                {
                    return string.Format("{0}({1})", CheckItemDescription, limit.ToString());
                }
                else
                {
                    return CheckItemDescription;
                }
            }
        }

        public string Color { get; set; }

        public double? LowerLimit { get; set; }

        public double? LowerAlertLimit { get; set; }

        public double? UpperAlertLimit { get; set; }

        public double? UpperLimit { get; set; }

        public List<CheckResultModel> CheckResultList { get; set; }

        public CheckItemModel()
        {
            CheckResultList = new List<CheckResultModel>();
        }
    }
}
