using System.Collections.Generic;
using System.Web.Mvc;
namespace Models.ASE.QA.MSAIchiManagement
{
    public class CreateFormModel
    {
        public List<SelectListItem> StationSelectItemList { get; set; }

        public FormInput FormInput { get; set; }

        public CreateFormModel()
        {
            StationSelectItemList = new List<SelectListItem>();
            FormInput = new FormInput();
        }
    }
}
