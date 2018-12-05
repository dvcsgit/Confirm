
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Models.ASE.QA.UnitManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        [Display(Name = "Unit", ResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }

        public List<ToleranceUnitModel> ToleranceUnitList { get; set; }

        public DetailViewModel()
        {
            ToleranceUnitList = new List<ToleranceUnitModel>();
        }
    }
}
