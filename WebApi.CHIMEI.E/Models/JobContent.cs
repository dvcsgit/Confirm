using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.CHIMEI.E.Models
{
    public class JobContent
    {
        public string JobUniqueID { get; set; }
        public string JobDescription { get; set; }
        public string RouteID { get; set; }
        public string RouteName { get; set; }
        public int TimeMode { get; set; }
        public string BeginTime { get; set; }
        public string EndTime { get; set; }
        public string JobRemark { get; set; }
        public string ControlPointUniqueID { get; set; }
        public string ControlPointID { get; set; }
        public string ControlPointDescription { get; set; }
        public string TagID { get; set; }
        public string ControlPointRemark { get; set; }
        public int ControlPointSeq { get; set; }
        public string EquipmentUniqueID { get; set; }
        public string EquipmentID { get; set; }
        public string EquipmentName { get; set; }
        public bool IsFeelItemDefaultNormal { get; set; }
        public int EquipmentSeq { get; set; }
        public string CheckItemUniqueID { get; set; }
        public string CheckItemID { get; set; }
        public string CheckItemDescription { get; set; }
        public bool IsFeelItem { get; set; }
        public double? LowerLimit { get; set; }
        public double? LowerAlertLimit { get; set; }
        public double? UpperAlertLimit { get; set; }
        public double? UpperLimit { get; set; }
        public string CheckItemRemark { get; set; }
        public string CheckItemUnit { get; set; }
        public int CheckItemSeq { get; set; }

        public string CheckResultUniqueID { get; set; }
    }
}