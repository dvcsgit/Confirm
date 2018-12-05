using System.Linq;
using System.Collections.Generic;

namespace Report.EquipmentMaintenance.Models.UnArriveResultAnalyze
{
    public class GridItem
    {
        public string User
        {
            get
            {
                if (!string.IsNullOrEmpty(UserName))
                {
                    return string.Format("{0}/{1}", UserID, UserName);
                }
                else
                {
                    return UserID;
                }
            }
        }
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

        public string UserID 
        {
            get 
            {
                return ArriveRecordModelList.FirstOrDefault().UserID;
            } 
        }

        public List<ArriveRecordModel> ArriveRecordModelList{get;set;}

        public GridItem()
        {
            ArriveRecordModelList = new List<ArriveRecordModel>();
        }

    }


}
