using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EquipmentMaintenance.MaintenanceFormManagement
{
    public class FeelOptionModel
    {
        public string UniqueID { get; set; }

        public string Description { get; set; }

        public bool IsAbnormal { get; set; }

        public string Display
        {
            get
            {
                if (IsAbnormal)
                {
                    return string.Format("{0}({1})", Description, Resources.Resource.Abnormal);
                }
                else
                {
                    return Description;
                }
            }
        }

        public int Seq { get; set; }
    }
}
