using System.Collections.Generic;
namespace Models.ASE.QS.FactoryCheckItemManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        public string Description { get; set; }

        public FormInput FormInput { get; set; }

        public List<CheckItemModel> CheckItemList { get; set; }

        public List<string> FactoryCheckItemList { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
            CheckItemList = new List<CheckItemModel>();
            FactoryCheckItemList = new List<string>();
        }
    }
}
