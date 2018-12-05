using System.Collections.Generic;

namespace Models.ASE.QS.FactoryStationManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        public string Description { get; set; }

        public FormInput FormInput { get; set; }

        public List<StationModel> StationList { get; set; }

        public List<string> FactoryStationList { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
            StationList = new List<StationModel>();
            FactoryStationList = new List<string>();
        }
    }
}
