
using System.Collections.Generic;
using System.ComponentModel;

namespace Models.ASE.QS.FactoryCheckItemManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        [DisplayName("受稽廠別描述")]
        public string Description { get; set; }

        public List<CheckItemModel> CheckItemList { get; set; }

        public DetailViewModel()
        {
            CheckItemList = new List<CheckItemModel>();
        }
    }
}
