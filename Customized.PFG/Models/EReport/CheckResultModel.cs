using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Customized.PFG.Models.EReport
{
    public class CheckResultModel
    {
        public string CheckTime { get; set; }

        public string Result { get; set; }

        public bool IsFeelItem { get; set; }

        public bool IsAbnormal { get; set; }

        public bool IsAlert { get; set; }

        public string Display
        {
            get
            {
                string result = string.Empty;

                if (IsFeelItem)
                {
                    result = Result;
                }
                else
                {
                    if (!string.IsNullOrEmpty(Result))
                    {
                        try
                        {
                            result = double.Parse(Result).ToString("F2");
                            //result = double.Parse(Result).ToString("F1");
                        }
                        catch
                        {
                            result = string.Empty;
                        }
                    }
                }

                if (IsAbnormal)
                {
                    return string.Format("{0}({1})", result, Resources.Resource.Abnormal);
                }
                else if (IsAlert)
                {
                    return string.Format("{0}({1})", result, Resources.Resource.Warning);
                }
                else
                {
                    return result;
                }
            }
        }
    }
}
