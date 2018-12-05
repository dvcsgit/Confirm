using System.Linq;
using System.Collections.Generic;

namespace Models.EquipmentMaintenance.DataSync
{
    public class PartModel
    {
        public string UniqueID { get; set; }

        public string Description { get; set; }

        public int Seq { get; set; }

        public List<CheckItemModel> CheckItemList { get; set; }

        public List<FileModel> FileList { get; set; }

        public List<MaterialModel> MaterialList { get; set; }

        public PartModel()
        {
            CheckItemList = new List<CheckItemModel>();
            FileList = new List<FileModel>();
            MaterialList = new List<MaterialModel>();
        }
    }
}
