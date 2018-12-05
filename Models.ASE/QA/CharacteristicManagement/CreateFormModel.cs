using System.Web.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.ASE.QA.CharacteristicManagement
{
    public class CreateFormModel
    {
        public FormInput FormInput { get; set; }

        public List<SelectListItem> TypeSelectItemList { get; set; }

        public List<UnitModel> UnitList { get; set; }

        public CreateFormModel()
        {
            FormInput = new FormInput();
            TypeSelectItemList = new List<SelectListItem>();
            UnitList = new List<UnitModel>();
        }
    }
}
