using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Models.ASE.QA.MonthlyReport
{
    public class QueryFormModel
    {
        public List<SelectListItem> FactorySelectItemList { get; set; }

        public List<SelectListItem> MonthSelectItemList { get; set; }

        public QueryParameters Parameters { get; set; }

        public QueryFormModel()
        {
            FactorySelectItemList = new List<SelectListItem>();
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
