using System.Web;
using System.Collections.Generic;
using Utility;
using System.Web.Mvc;

namespace Customized.TAIWAY.Models.EquipmentRepairForm
{
    public class QueryFormModel
    {
        public List<SelectListItem> StatusSelectItemList { get; set; }

        public List<SelectListItem> RepairFormTypeSelectItemList { get; set; }

        public List<SelectListItem> EuipmentSelectItemList { get; set; }

        public InitParameters InitParameters { get; set; }

        public QueryParameters Parameters { get; set; }

        public QueryFormModel()
        {
            StatusSelectItemList = new List<SelectListItem>() 
            {                 
                new SelectListItem() { Text = "=" + Resources.Resource.SelectAll + "=", Value = "" },
                new SelectListItem() { Text = Resources.Resource.RFormStatus_1, Value = "1" },
                new SelectListItem() { Text = Resources.Resource.RFormStatus_2, Value = "2" },
                new SelectListItem() { Text = Resources.Resource.RFormStatus_3, Value = "3" },
                new SelectListItem() {Selected = true, Text = Resources.Resource.RFormStatus_4, Value = "4" },
                new SelectListItem() { Text = Resources.Resource.RFormStatus_5, Value = "5" },
                new SelectListItem() { Text = Resources.Resource.RFormStatus_6, Value = "6" },
                new SelectListItem() { Text = Resources.Resource.RFormStatus_7, Value = "7" }
            };

            RepairFormTypeSelectItemList = new List<SelectListItem>();
            EuipmentSelectItemList = new List<SelectListItem>();
            InitParameters = new InitParameters();
            Parameters = new QueryParameters();
        }
    }
}
