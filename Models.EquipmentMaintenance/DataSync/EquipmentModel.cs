using System.Linq;
using System.Collections.Generic;

namespace Models.EquipmentMaintenance.DataSync
{
    public class EquipmentModel
    {
        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Name { get; set; }

        public bool IsFeelItemDefaultNormal { get; set; }

        public List<PartModel> PartList { get; set; }

        public List<EquipmentSpecModel> SpecList { get; set; }

        public List<CheckItemModel> CheckItemList
        {
            get
            {
                return PartList.SelectMany(x => x.CheckItemList).Distinct().ToList();
            }
        }

        public List<MaterialModel> MaterialList
        {
            get
            {
                return PartList.SelectMany(x => x.MaterialList).Distinct().ToList();
            }
        }

        public EquipmentModel()
        {
            PartList = new List<PartModel>();
            SpecList = new List<EquipmentSpecModel>();
        }
    }
}
