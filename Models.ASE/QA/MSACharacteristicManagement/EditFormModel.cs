using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Models.ASE.QA.MSACharacteristicManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        public string Station { get; set; }

        public string Ichi { get; set; }

        public FormInput FormInput { get; set; }

        public List<UnitModel> CharacteristicUnitList { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
            CharacteristicUnitList = new List<UnitModel>();
        }
    }
}
