using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.EquipmentMaintenance.Models.CheckJobAnalyze
{
   public  class GridItem
    {


       public string Route
       {
           get
           {
               if (!string.IsNullOrEmpty(JobDescription))
               {
                   if (!string.IsNullOrEmpty(RouteName))
                   {
                       return string.Format("{0}/{1}-{2}", RouteID, RouteName, JobDescription);
                   }
                   else
                   {
                       return string.Format("{0}-{1}", RouteID, JobDescription);
                   }
               }
               else
               {
                   if (!string.IsNullOrEmpty(RouteName))
                   {
                       return string.Format("{0}/{1}", RouteID, RouteName);
                   }
                   else
                   {
                       return RouteID;
                   }
               }
           }
       }
       public List<CheckAllResult> CheckResultList { get; set; }
        public string RouteID { get; set; }

        public string RouteName { get; set; }

        public string JobDescription { get; set; }

        public string CheckResultCount   //计算总的数量
        {
            get 
            {
                if (CheckResultList != null && CheckResultList.Count != 0)
                {
                    return   CheckResultList.Sum(x => x.Result.Count()).ToString();
                } 
                else
                {
                return "0";
                }
            } 
        }


        public string UnSendJob 
        {
            get 
            {
                if (CheckResultList != null && CheckResultList.Count != 0)
                {
                    return CheckResultList.Sum(x => x.Result.Where(a => a == "○").Count()).ToString() + "/" + CheckResultCount;
                }
                else 
                {
                    return "0/0";
                }
            }
            }
        public string UnCheckResult
        {
            get
            {
                if (CheckResultList != null && CheckResultList.Count != 0)
                {
                    return CheckResultList.Sum(x => x.Result.Where(a => a == "※").Count()).ToString() + "/" + CheckResultCount;
                }
                else
                {
                    return "0/0";
                }
            }
        }
        public string UnCompletelyCheckResult
        {
            get
            {
                if (CheckResultList != null && CheckResultList.Count != 0)
                {
                    return CheckResultList.Sum(x => x.Result.Where(a => a == "●").Count()).ToString() + "/" + CheckResultCount;
                }
                else
                {
                    return "0/0";
                }
            }
        }
        public string NormalCheckResult
        {
            get
            {
                if (CheckResultList != null && CheckResultList.Count != 0)
                {
                    return CheckResultList.Sum(x => x.Result.Where(a => a == "V").Count()).ToString() + "/" + CheckResultCount;
                }
                else
                {
                    return "0/0";
                }
            }
        }

       public GridItem()
       {
           CheckResultList = new List<CheckAllResult>();
       }

    }
}
