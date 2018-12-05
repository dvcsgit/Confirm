using System.Web.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.ASE.QA.IchiManagement
{
    public class CreateFormModel
    {
        public FormInput FormInput { get; set; }

        public List<SelectListItem> TypeSelectItemList { get; set; }

        public List<CharacteristicModel> CharacteristicList { get; set; }

        public CreateFormModel()
        {
            FormInput = new FormInput();
            TypeSelectItemList = new List<SelectListItem>();
            CharacteristicList = new List<CharacteristicModel>();
        }
    }
}
