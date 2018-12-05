using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.GuardPatrol.JobManagement
{
    public class ControlPointModel
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

        public int? MinTimeSpan { get; set; }

        public List<CheckItemModel> CheckItemList { get; set; }

        public int Seq { get; set; }

        public bool IsChecked
        {
            get
            {
                return CheckItemList.Any(x => x.IsChecked);
            }
        }

        public ControlPointModel()
        {
            CheckItemList = new List<CheckItemModel>();
        }
    }
}
