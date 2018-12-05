using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Customized.PFG.Models.DailyReport
{
    public class CheckResultModel
    {
        public string OrganizationUniqueID { get; set; }

        public string CheckItemDescription { get; set; }

        public string CheckTime { get; set; }

        public string CheckTimeDisplay
        {
            get
            {
                try
                {
                    return string.Format("{0}:{1}", CheckTime.Substring(0, 2), CheckTime.Substring(2, 2));
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        public string Result { get; set; }

        public bool IsAbnormal { get; set; }

        public bool IsAlert { get; set; }

        //public string Display
        //{
        //    get
        //    {
        //        string result = string.Empty;

        //        if (!string.IsNullOrEmpty(Result))
        //        {
        //            if (OrganizationUniqueID == "8be56067-3158-4803-a51a-54ab0bd1fe99" && (CheckItemDescription == "氮氣濃度" || CheckItemDescription == "爐壓" || CheckItemDescription == "東側餵料機轉速" || CheckItemDescription == "西側餵料機轉速" || CheckItemDescription == "氧氣瓦斯比" || CheckItemDescription == "玻璃液位"))
        //            {
        //                try
        //                {
        //                    result = double.Parse(Result).ToString("F2");
        //                }
        //                catch
        //                {
        //                    result = string.Empty;
        //                }
        //            }
        //            else if (CheckItemDescription == "氮氣濃度" || CheckItemDescription == "玻璃液位" || CheckItemDescription == "風油比")
        //            {
        //                try
        //                {
        //                    result = double.Parse(Result).ToString("F3");
        //                }
        //                catch
        //                {
        //                    result = string.Empty;
        //                }
        //            }
        //            else if ((OrganizationUniqueID == "b7beee76-8af9-4de0-b106-2e56c62255b2" || OrganizationUniqueID == "8be56067-3158-4803-a51a-54ab0bd1fe99") && CheckItemDescription == "氧氣燃油比")
        //            {
        //                try
        //                {
        //                    result = double.Parse(Result).ToString("F2");
        //                }
        //                catch
        //                {
        //                    result = string.Empty;
        //                }
        //            }
        //            else
        //            {
        //                try
        //                {
        //                    result = double.Parse(Result).ToString("F1");
        //                }
        //                catch
        //                {
        //                    result = string.Empty;
        //                }
        //            }
        //        }
                
        //        if (IsAbnormal)
        //        {
        //            return string.Format("{0}({1})", result, Resources.Resource.Abnormal);
        //        }
        //        else if (IsAlert)
        //        {
        //            return string.Format("{0}({1})", result, Resources.Resource.Warning);
        //        }
        //        else
        //        {
        //            return result;
        //        }
        //    }
        //}
    }
}
