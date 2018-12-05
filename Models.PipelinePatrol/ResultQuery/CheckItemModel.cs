using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.ResultQuery
{
    public class CheckItemModel
    {
        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public string Display
        {
            get
            {
                return string.Format("{0}/{1}", ID, Description);
            }
        }

        public double? LowerLimit { get; set; }

        public double? LowerAlertLimit { get; set; }

        public double? UpperAlertLimit { get; set; }

        public double? UpperLimit { get; set; }

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
