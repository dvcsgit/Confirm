using System.Linq;
using System.Collections.Generic;

namespace Models.GuardPatrol.DailyReport
{
    public class CheckItemModel
    {
        //public string CheckItemID { get; set; }

        //public string CheckItemDescription { get; set; }

        //public string CheckItem
        //{
        //    get
        //    {
        //        return string.Format("{0}/{1}", CheckItemID, CheckItemDescription);
        //    }
        //}

        //public double? LowerLimit { get; set; }

        //public double? LowerAlertLimit { get; set; }

        //public double? UpperAlertLimit { get; set; }

        //public double? UpperLimit { get; set; }

        //public string Unit { get; set; }

        public List<CheckResultModel> CheckResultList { get; set; }

        //public bool HaveAbnormal
        //{
        //    get
        //    {
        //        return CheckResultList.Any(x => x.IsAbnormal);
        //    }
        //}

        //public bool HaveAlert
        //{
        //    get
        //    {
        //        return CheckResultList.Any(x => x.IsAlert);
        //    }
        //}

        //public bool IsChecked
        //{
        //    get
        //    {
        //        return CheckResultList.Count > 0;
        //    }
        //}

        public CheckItemModel()
        {
            CheckResultList = new List<CheckResultModel>();
        }
    }
}
