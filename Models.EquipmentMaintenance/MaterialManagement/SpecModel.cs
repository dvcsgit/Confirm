using System.Linq;
using System.Collections.Generic;

namespace Models.EquipmentMaintenance.MaterialManagement
{
    public class SpecModel
    {
        public string UniqueID { get; set; }

        public string Description { get; set; }

        public List<MaterialSpecOption> OptionList { get; set; }

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

        public int Seq { get; set; }

        public SpecModel()
        {
            OptionList = new List<MaterialSpecOption>();
        }
    }
}
