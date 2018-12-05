using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customized.PFG.CN.Models.AbnormalHanding
{
    public class CheckItemModel
    {
        public int No { get; set; }
        public string CheckDate { get; set; }
        public string CheckTime { get; set; }
        public string ControlPointDescription { get; set; }

        public string EquipmentID { get; set; }

        public string EquipmentName { get; set; }
        public string Remark { get; set; }
        public string EquipmentDescription
        {
            get
            {
                if (!string.IsNullOrEmpty(EquipmentID))
                {
                    if (!string.IsNullOrEmpty(EquipmentName))
                    {
                        return string.Format("{0}/{1}", EquipmentID, EquipmentName);
                    }
                    else
                    {
                        return string.Format("{0}/{1}", EquipmentID,"-");
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(EquipmentName))
                    {
                        return string.Format("{0}/{1}","-", EquipmentName);
                    }
                    else
                    {
                        return string.Format("{0}/{1}", "-", "-");
                    }
                }
            }
        }
        public string PartDescription { get; set; }

        public string CheckItemDescription { get; set; }

        public string RouteID { get; set; }
        public string RouteName { get; set; }
        public string Route
        {
            get
            {
                return string.Format("{0}/{1}", RouteID, RouteName);
            }
        }
        public string ID { get; set; }

        public string Description { get; set; }
        public string AbnormalReasonDescription
        {
            get
            {
                return string.Format("{0}/{1}", ID, Description);
            }
        }
        public string HandlingMethodID { get; set; }
        public string HandingMethodDescription { get; set; }
        public string HandingMethodRemark { get; set; }
        public string HandingMethod
        {
            get
            {
                if (!string.IsNullOrEmpty(HandingMethodRemark))
                {
                    return string.Format("{0}/{1}", HandlingMethodID, HandingMethodRemark);
                }
                else
                {
                    return string.Format("{0}/{1}", HandlingMethodID, HandingMethodDescription);
                }
            }
        }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string ArriveRecord
        {
            get
            {
                return string.Format("{0}/{1}", UserID, UserName);
            }
        }
    }
}
