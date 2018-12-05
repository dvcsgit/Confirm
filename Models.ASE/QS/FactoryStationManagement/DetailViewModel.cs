
using System.Collections.Generic;
using System.ComponentModel;

namespace Models.ASE.QS.FactoryStationManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        [DisplayName("受稽廠別描述")]
        public string Description { get; set; }

        public List<StationModel> StationList { get; set; }

        public DetailViewModel()
        {
            StationList = new List<StationModel>();
        }
    }
}
