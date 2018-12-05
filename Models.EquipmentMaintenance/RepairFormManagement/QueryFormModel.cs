using System.Web.Mvc;
using System.Collections.Generic;
using Utility;

namespace Models.EquipmentMaintenance.RepairFormManagement
{
    public class QueryFormModel
    {
        public List<SelectListItem> StatusSelectItemList { get; set; }

        public List<SelectListItem> RepairFormTypeSelectItemList { get; set; }

        public InitParameters InitParameters { get; set; }

        public QueryParameters Parameters { get; set; }

        public QueryFormModel()
        {
            StatusSelectItemList = new List<SelectListItem>() 
            { 
                //Define.DefaultSelectListItem(Resources.Resource.SelectAll),
                new SelectListItem() { Text = Resources.Resource.RFormStatus_0, Value = "0", Selected = true },
                new SelectListItem() { Text = Resources.Resource.RFormStatus_1, Value = "1", Selected = true },
                new SelectListItem() { Text = Resources.Resource.RFormStatus_2, Value = "2", Selected = true },
                new SelectListItem() { Text = Resources.Resource.RFormStatus_3, Value = "3", Selected = true },
                new SelectListItem() { Text = Resources.Resource.RFormStatus_4, Value = "4", Selected = true },
                new SelectListItem() { Text = Resources.Resource.RFormStatus_5, Value = "5", Selected = true },
                new SelectListItem() { Text = Resources.Resource.RFormStatus_6, Value = "6", Selected = true },
                new SelectListItem() { Text = Resources.Resource.RFormStatus_7, Value = "7", Selected = true },
                new SelectListItem() { Text = Resources.Resource.RFormStatus_8, Value = "8", Selected = false },
                new SelectListItem() { Text = Resources.Resource.RFormStatus_9, Value = "9", Selected = true }
            };

            RepairFormTypeSelectItemList = new List<SelectListItem>();
            InitParameters = new InitParameters();
            Parameters = new QueryParameters();
        }
    }
}
