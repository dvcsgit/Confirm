using System.Web.Mvc;
using System.Collections.Generic;

namespace Models.EquipmentMaintenance.MonthlyReport
{
    public class QueryFormModel
    {
        public QueryParameters Parameters { get; set; }

        public List<SelectListItem> YearSelectItemList { get; set; }

        public List<SelectListItem> MonthSelectItemList { get; set; }

        public QueryFormModel()
        {
            Parameters = new QueryParameters();

            MonthSelectItemList = new List<SelectListItem>() 
            { 
                new SelectListItem()
                {
                    Selected = true,
                    Text = Resources.Resource.SelectOne,
                    Value = ""
                }
            };

            for (int i = 1; i <= 12; i++)
            {
                MonthSelectItemList.Add(new SelectListItem()
                {
                    Value = i.ToString().PadLeft(2, '0'),
                    Text = i.ToString().PadLeft(2, '0')
                });
            }
        }
    }
}
