using System.Collections.Generic;
using System.Web.Mvc;
using Utility;
namespace Models.TruckPatrol.CheckedTruckManagement
{
    public class QueryFormModel
    {
        public QueryParameters Parameters { get; set; }

        public List<SelectListItem> KeyStatusSelectItemList
        {
            get
            {
                return new List<SelectListItem>() 
                { 
                    new SelectListItem() { Text = "全部", Value = "" },
                    new SelectListItem() { Text = "未發鑰匙", Value = "0", Selected = true },
                    new SelectListItem() { Text = "已發鑰匙", Value = "1" }
                };
            }
        }

        public QueryFormModel()
        {
            Parameters = new QueryParameters();
        }
    }
}
