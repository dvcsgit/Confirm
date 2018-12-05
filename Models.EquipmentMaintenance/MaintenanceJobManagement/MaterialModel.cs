using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EquipmentMaintenance.MaintenanceJobManagement
{
    public class MaterialModel
    {
        public bool IsChecked { get; set; }

        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Name { get; set; }

        public string Display
        {
            get
            {
                return string.Format("{0}/{1}", ID, Name);
            }
        }

        public int Quantity { get; set; }

        public int? ChangeQuantity { get; set; }
    }
}
