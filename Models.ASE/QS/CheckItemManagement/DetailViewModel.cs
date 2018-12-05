
using System.Collections.Generic;
using System.ComponentModel;

namespace Models.ASE.QS.CheckItemManagement
{
    public class DetailViewModel
    {
        [DisplayName("稽核類別代號")]
        public decimal ID { get; set; }

        [DisplayName("稽核類別英文描述")]
        public string EDescription { get; set; }

        [DisplayName("稽核類別中文描述")]
        public string CDescription { get; set; }

        public List<CheckItemModel> ItemList { get; set; }

        public DetailViewModel()
        {
            ItemList = new List<CheckItemModel>();
        }
    }
}
