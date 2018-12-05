using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.Inventory
{
    public class UploadResultModel
    {
        public List<UploadItem> ItemList { get; set; }

        public UploadResultModel()
        {
            ItemList = new List<UploadItem>();
        }
    }
}
