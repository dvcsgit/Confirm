using System.Collections.Generic;
namespace Models.ASE.QA.UnitManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        public FormInput FormInput { get; set; }

        public List<ToleranceUnitModel> ToleranceUnitList { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
            ToleranceUnitList = new List<ToleranceUnitModel>();
        }
    }
}
