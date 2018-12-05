using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.Inventory
{
    public class DetailViewModel
    {
        public string FullOrganizationDescription { get; set; }

        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Name { get; set; }

        public int Quantity { get; set; }

        public List<InventoryModel> InventoryList { get; set; }

        public List<SpecModel> SpecList { get; set; }

        public DetailViewModel()
        {
            InventoryList = new List<InventoryModel>();
        }
    }
}
