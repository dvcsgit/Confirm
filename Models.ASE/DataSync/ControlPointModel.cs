using System.Linq;
using System.Collections.Generic;

namespace Models.ASE.DataSync
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

        public List<CheckItemModel> CheckItemList { get; set; }

        public List<CheckItemModel> AllCheckItemList
        {
            get
            {
                return EquipmentList.SelectMany(x => x.CheckItemList).Union(CheckItemList).Distinct().ToList();
            }
        }

        public List<EquipmentSpecModel> EquipmentSpecList
        {
            get
            {
                return EquipmentList.SelectMany(x => x.SpecList).Distinct().ToList();
            }
        }

        public List<MaterialModel> MaterialList
        {
            get
            {
                return EquipmentList.SelectMany(x => x.MaterialList).Distinct().ToList();
            }
        }

        public ControlPointModel()
        {
            EquipmentList = new List<EquipmentModel>();
            CheckItemList = new List<CheckItemModel>();
        }
    }
}
