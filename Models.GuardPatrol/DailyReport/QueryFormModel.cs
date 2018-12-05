using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;

namespace Models.GuardPatrol.DailyReport
{
    public class QueryFormModel
    {
        public QueryParameters Parameters { get; set; }

        public List<SelectListItem> HourSelectItemList { get; set; }

        public QueryFormModel()
        {
            Parameters = new QueryParameters();

            HourSelectItemList = new List<SelectListItem>() 
            { 
                Define.DefaultSelectListItem(Resources.Resource.SelectAll)
            };

            for (int hour = 0; hour < 24; hour++)
            {
                HourSelectItemList.Add(new SelectListItem()
                {
                    Text = hour.ToString().PadLeft(2, '0'),
                    Value = hour.ToString().PadLeft(2, '0')
                });
            }
        }
    }
}
