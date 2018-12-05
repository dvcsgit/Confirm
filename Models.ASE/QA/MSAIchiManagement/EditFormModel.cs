using System.Collections.Generic;
using System.Web.Mvc;
namespace Models.ASE.QA.MSAIchiManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        public List<SelectListItem> StationSelectItemList { get; set; }

        public FormInput FormInput { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
            StationSelectItemList = new List<SelectListItem>();
        }
    }
}
