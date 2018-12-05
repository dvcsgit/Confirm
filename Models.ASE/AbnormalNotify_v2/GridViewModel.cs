using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.AbnormalNotify_v2
{
    public class GridViewModel
    {
        public List<GridItem> ItemList { get; set; }

        public List<string> ReplyUserList { get; set; }

        public GridViewModel()
        {
            ItemList = new List<GridItem>();
            ReplyUserList = new List<string>();
        }
    }
}
