using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.MSAReport
{
    public class GridViewModel
    {
        public List<AvgAndRGridItem> AvgAndRItemList { get; set; }

        public List<GoOnGoGridItem> GoOnGoItemList { get; set; }

        public List<AnovaGridItem> AnovaItemList { get; set; }

        public int ItemCount
        {
            get
            {
                var count = 0;

                if (AvgAndRItemList != null) {
                    count += AvgAndRItemList.Count;
                }

                if (GoOnGoItemList != null)
                {
                    count += GoOnGoItemList.Count;
                }

                if (AnovaItemList != null)
                {
                    count += AnovaItemList.Count;
                }

                return count;
            }
        }

        public GridViewModel()
        {
            AvgAndRItemList = new List<AvgAndRGridItem>();
            GoOnGoItemList = new List<GoOnGoGridItem>();
            AnovaItemList = new List<AnovaGridItem>();
        }
    }
}
