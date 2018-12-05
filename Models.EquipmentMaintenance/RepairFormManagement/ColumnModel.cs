using System.Collections.Generic;
using System.Linq;

namespace Models.EquipmentMaintenance.RepairFormManagement
{
    public class ColumnModel
    {
        public string UniqueID { get; set; }

        public string Description { get; set; }

        public List<RFormColumnOption> OptionList { get; set; }

        public string OptionUniqueID { get; set; }

        public string OptionValue
        {
            get
            {
                var option = OptionList.FirstOrDefault(x => x.UniqueID == OptionUniqueID);

                if (option != null)
                {
                    return option.Description;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string Value { get; set; }

        public ColumnModel()
        {
            OptionList = new List<RFormColumnOption>();
        }
    }
}
