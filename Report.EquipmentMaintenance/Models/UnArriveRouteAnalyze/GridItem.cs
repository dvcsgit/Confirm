using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.EquipmentMaintenance.Models.UnArriveRouteAnalyze
{
   public class GridItem
    {
        
        public int Count
        {
            get
            {
                return ArriveRecordModelList.Count();

            }
        }

        public string UserName
        {
            get
            {
                return ArriveRecordModelList.FirstOrDefault().UserName;
            }
        }

        public string Route
        {
            get
            {
                return ArriveRecordModelList.FirstOrDefault().Route;
            }
        }

        public List<ArriveRecordModel> ArriveRecordModelList { get; set; }

        public GridItem()
        {
            ArriveRecordModelList = new List<ArriveRecordModel>();
        }
    }
}
