using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.Inventory
{
    public class GridItem
    {
        [DisplayName("UniqueID")]
        public string UniqueID { get; set; }

        [DisplayName("部門")]
        public string OrganizationDescription { get; set; }

        [DisplayName("材料類別")]
        public string EquipmentType { get; set; }

        [DisplayName("材料編號")]
        public string ID { get; set; }

        [DisplayName("材料名稱")]
        public string Name { get; set; }

        [DisplayName("數量")]
        public int Quantity { get; set; }

        [DisplayName("盤點數量")]
        public int Q1 { get; set; }

        //[DisplayName("盤點日期")]
        //public string Date { get; set; }
    }
}
