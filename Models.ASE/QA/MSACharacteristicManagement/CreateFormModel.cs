using System.Web.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.ASE.QA.MSACharacteristicManagement
{
    public class CreateFormModel
    {
        public FormInput FormInput { get; set; }

        public List<SelectListItem> IchiSelectItemList { get; set; }

        public CreateFormModel()
        {
            FormInput = new FormInput();
            IchiSelectItemList = new List<SelectListItem>();
        }
    }
}
