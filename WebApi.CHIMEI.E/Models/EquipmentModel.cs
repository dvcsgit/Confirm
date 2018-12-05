using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.CHIMEI.E.Models
{
    public class EquipmentModel
    {
        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Name { get; set; }

        public bool IsFeelItemDefaultNormal { get; set; }

        public int Seq { get; set; }

        public List<CheckItemModel> CheckItemList { get; set; }

        public EquipmentModel()
        {
            CheckItemList = new List<CheckItemModel>();
        }
    }
}