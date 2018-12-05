using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customized.PFG.CN.Models.AbnormalHanding
{
    public class ControlPointModel
    {
        public string ControlPointDescription { get; set; }

        public List<CheckItemModel> CheckItemList { get; set; }

        public ControlPointModel()
        {
            CheckItemList = new List<CheckItemModel>();
        }
    }
}
