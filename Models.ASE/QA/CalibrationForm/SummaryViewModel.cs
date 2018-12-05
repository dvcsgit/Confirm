using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.CalibrationForm
{
    public class SummaryViewModel
    {
        public int Count
        {
            get
            {
                return ItemList.Sum(x => x.Count);
            }
        }

        public List<SummaryItem> ItemList { get; set; }

        public SummaryViewModel()
        {
            ItemList = new List<SummaryItem>();
        }
    }
}
