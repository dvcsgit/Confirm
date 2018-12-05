using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Models.ASE.QA.IchiManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        public FormInput FormInput { get; set; }

        public List<SelectListItem> TypeSelectItemList { get; set; }

        public List<CharacteristicModel> CharacteristicList { get; set; }

        public List<string> IchiCharacteristicList { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
            CharacteristicList = new List<CharacteristicModel>();
            IchiCharacteristicList = new List<string>();
            TypeSelectItemList = new List<SelectListItem>();
        }
    }
}
