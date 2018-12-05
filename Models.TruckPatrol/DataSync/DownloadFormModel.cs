using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.TruckPatrol.DataSync
{
    public class DownloadFormModel
    {
        public string UserID { get; set; }

        public List<string> TruckUniqueIDList { get; set; }
         
        public DownloadFormModel()
        {
            TruckUniqueIDList = new List<string>();
        }
    }
}
