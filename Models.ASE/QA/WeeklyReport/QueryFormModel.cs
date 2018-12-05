using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Models.ASE.QA.WeeklyReport
{
    public class QueryFormModel
    {
        public List<SelectListItem> FactorySelectItemList { get; set; }

        public List<SelectListItem> WeekSelectItemList { get; set; }

        public QueryParameters Parameters { get; set; }

        public QueryFormModel()
        {
            FactorySelectItemList = new List<SelectListItem>();
            Parameters = new QueryParameters();

            WeekSelectItemList = new List<SelectListItem>() 
            { 
                new SelectListItem()
                {
                    Selected = true,
                    Text = Resources.Resource.SelectOne,
                    Value = ""
                }
            };

            for (int i = 1; i <= 52; i++)
            {
                WeekSelectItemList.Add(new SelectListItem()
                {
                    Value = i.ToString().PadLeft(2, '0'),
                    Text = i.ToString().PadLeft(2, '0')
                });
            }
        }
    }
}
