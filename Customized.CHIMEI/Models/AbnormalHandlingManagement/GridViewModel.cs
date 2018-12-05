using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customized.CHIMEI.Models.AbnormalHandlingManagement
{
    public class GridViewModel
    {
        public int AbnormalCount
        {
            get
            {
                return ItemList.Count(x => x.IsAbnormal);
            }
        }

        public int WarningCount
        {
            get
            {
                return ItemList.Count(x => !x.IsAbnormal && x.IsAlert);
            }
        }

        public int UnClosedAbnormalCount
        {
            get
            {
                return ItemList.Count(x => x.IsAbnormal && !x.IsClosed);
            }
        }

        public int UnClosedWarningCount
        {
            get
            {
                return ItemList.Count(x => !x.IsAbnormal && x.IsAlert && !x.IsClosed);
            }
        }

        public int ClosedCount
        {
            get
            {
                return ItemList.Count(x => x.IsClosed);
            }
        }

        public List<GridItem> ItemList { get; set; }

        public GridViewModel()
        {
            ItemList = new List<GridItem>();
        }
    }
}
