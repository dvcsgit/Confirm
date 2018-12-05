using System.Web.Mvc;
using System.Collections.Generic;

namespace Models.TruckPatrol.ResultQuery
{
    public class EditFormModel
    {
        public string JobUniqueID { get; set; }

        public string BeginDate { get; set; }

        public string EndDate { get; set; }

        public List<SelectListItem> UnPatrolReasonSelectItemList { get; set; }

        public FormInput FormInput { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
            UnPatrolReasonSelectItemList = new List<SelectListItem>();
        }
    }
}
