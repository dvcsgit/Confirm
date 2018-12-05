using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Models.ASE.QA.CharacteristicManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        public FormInput FormInput { get; set; }

        public List<SelectListItem> TypeSelectItemList { get; set; }

        public List<UnitModel> UnitList { get; set; }

        public List<string> CharacteristicUnitList { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
            UnitList = new List<UnitModel>();
            CharacteristicUnitList = new List<string>();
            TypeSelectItemList = new List<SelectListItem>();
        }
    }
}
