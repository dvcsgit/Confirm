using System.Web.Mvc;
using System.Collections.Generic;
using Utility;

namespace Models.EquipmentMaintenance.MaintenanceFormManagement
{
    public class QueryFormModel
    {
        public List<SelectListItem> StatusSelectItemList { get; set; }

        public QueryParameters Parameters { get; set; }

        public QueryFormModel()
        {
            StatusSelectItemList = new List<SelectListItem>() 
            { 
                //Define.DefaultSelectListItem(Resources.Resource.SelectAll),
                new SelectListItem() { Text = Resources.Resource.MFormStatus_0, Value = "0", Selected = true },
                new SelectListItem() { Text = "未接案(逾期)", Value = "7", Selected = true },
                new SelectListItem() { Text = Resources.Resource.MFormStatus_1, Value = "1", Selected = true },
                new SelectListItem() { Text = Resources.Resource.MFormStatus_2, Value = "2", Selected = true },
                new SelectListItem() { Text = Resources.Resource.MFormStatus_3, Value = "3", Selected = true },
                new SelectListItem() { Text = Resources.Resource.MFormStatus_4, Value = "4", Selected = true },
                new SelectListItem() { Text = Resources.Resource.MFormStatus_5, Value = "5", Selected = false },
                new SelectListItem() { Text = Resources.Resource.MFormStatus_6, Value = "6", Selected = true }
            };

            Parameters = new QueryParameters();
        }
    }
}
