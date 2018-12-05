using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.EquipmentMaintenance.Models.EquipmentFixCost
{
   public  class GridViewModel
    {
        public QueryParameters Parameters { get; set; }

        public List<GridItem> ItemList { get; set; }

        public int AbnormalCount
        {
            get
            {
                if (ItemList != null && ItemList.Count > 0)
                {
                    return ItemList.Sum(x => x.AbnormalCount);
                }
                else
                {
                    return 0;
                }
            }
        }

        public string MTBF
        {
            get
            {
                if (ItemList != null && ItemList.Count > 0)
                {
                    return Math.Round(ItemList.Average(x => x.MTBF), 1).ToString();
                }
                else
                {
                    return "-";
                }
            }
        }

        public string MTTR
        {
            get
            {
                if (ItemList != null && ItemList.Count > 0)
                {
                    return Math.Round(ItemList.Average(x => x.MTTR), 1).ToString();
                }
                else
                {
                    return "-";
                }
            }
        }

        public double Duration { get; set; }

        public GridViewModel()
        {
            Parameters = new QueryParameters();
            ItemList = new List<GridItem>();
        }
    }
}
