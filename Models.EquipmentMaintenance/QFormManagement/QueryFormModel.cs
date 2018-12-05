using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;

namespace Models.EquipmentMaintenance.QFormManagement
{
    public class QueryFormModel
    {
        public List<SelectListItem> StatusSelectItemList { get; set; }

        public QueryParameters Parameters { get; set; }

        public QueryFormModel()
        {
            StatusSelectItemList = new List<SelectListItem>() 
            { 
                Define.DefaultSelectListItem(Resources.Resource.SelectAll),
                new SelectListItem() { Text = Resources.Resource.QFormStatus_1, Value = "1" },
                new SelectListItem() { Text = Resources.Resource.QFormStatus_2, Value = "2" },
                new SelectListItem() { Text = Resources.Resource.QFormStatus_3, Value = "3" }
            };

            Parameters = new QueryParameters();
        }
    }
}
