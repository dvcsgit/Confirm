using System.Collections.Generic;
namespace Models.ASE.QS.CheckItemManagement
{
    public class EditFormModel
    {
        public FormInput FormInput { get; set; }

        public List<CheckItemModel> ItemList { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
            ItemList = new List<CheckItemModel>();
        }
    }
}
