using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customized.PFG.Models.DailyReport
{
    public class CheckItemModel
    {
        public string CheckItemDescription { get; set; }

        public Dictionary<string, CheckResultModel> CheckResultList { get; set; }

        public List<CheckResultModel> AllCheckResultList
        {
            get
            {
                return CheckResultList.Where(x => x.Value != null).Select(x => x.Value).ToList();
            }
        }

        public CheckItemModel()
        {
            CheckResultList = new Dictionary<string, CheckResultModel>();
        }
    }
}
