using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.CHIMEI.E.Models
{
     public class ControlPointModel
    {
        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public bool IsFeelItemDefaultNormal { get; set; }

        public string TagID { get; set; }

        public int? MinTimeSpan { get; set; }

        public string Remark { get; set; }

        public int Seq { get; set; }

        public List<EquipmentModel> EquipmentList { get; set; }

        public List<CheckItemModel> CheckItemList
        {
            get
            {
                return EquipmentList.SelectMany(x => x.CheckItemList).Distinct().ToList();
            }
        }

        public ControlPointModel()
        {
            EquipmentList = new List<EquipmentModel>();
        }
    }
}