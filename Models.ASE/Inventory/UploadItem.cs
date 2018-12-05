using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.Inventory
{
    public class UploadItem
    {
        public int RowIndex { get; set; }

        public bool OK { get; set; }

        public string UniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public string EquipmentType { get; set; }

        public string ID { get; set; }

        public string Name { get; set; }

        public int? Quantity { get; set; }

        public int? Q1 { get; set; }

        public int? Q2
        {
            get
            {
                if (Quantity.HasValue && Q1.HasValue)
                {
                    return Q1.Value - Quantity.Value;
                }
                else
                {
                    return null;
                }
            }
        }

        public string Diff
        {
            get
            {
                if (Q2.HasValue)
                {
                    if (Q2 > 0)
                    {
                        return "+" + Q2.Value.ToString();
                    }
                    else
                    {
                        return Q2.Value.ToString();
                    }
                }
                else
                {
                    return "-";
                }
            }
        }

        public string ErrorMessage { get; set; }
    }
}
