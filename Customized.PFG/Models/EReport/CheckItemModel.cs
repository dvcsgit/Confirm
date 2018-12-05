using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customized.PFG.Models.EReport
{
    public class CheckItemModel
    {
        public string CheckItemDescription { get; set; }

        public List<CheckResultModel> CheckResultList { get; set; }

        public CheckResultModel CheckResult
        {
            get
            {
                return CheckResultList.OrderByDescending(x => x.CheckTime).FirstOrDefault();
            }
        }

        public string Result
        {
            get
            {
                if (CheckResult != null)
                {
                    return CheckResult.Display;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public CheckItemModel()
        {
            CheckResultList = new List<CheckResultModel>();
        }
    }
}
