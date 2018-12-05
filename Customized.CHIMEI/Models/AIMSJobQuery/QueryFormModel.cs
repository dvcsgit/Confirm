using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;

namespace Customized.CHIMEI.Models.AIMSJobQuery
{
    public class QueryFormModel
    {
        public QueryParameters Parameters { get; set; }

        public List<SelectListItem> CycleSelectItemList
        {
            get
            {
                return new List<SelectListItem>() 
                { 
                    Define.DefaultSelectListItem(Resources.Resource.SelectAll),
                    new SelectListItem() { Value = "1M", Text = "1M" },
                    new SelectListItem() { Value = "3M", Text = "3M" },
                    new SelectListItem() { Value = "4M", Text = "4M" }
                };
            }
        }

        public List<SelectListItem> MotorTypeSelectItemList 
        {
            get
            {
                return new List<SelectListItem>() 
                { 
                    Define.DefaultSelectListItem(Resources.Resource.SelectAll),
                    new SelectListItem() { Value = "A級", Text = "A級" },
                    new SelectListItem() { Value = "B級", Text = "B級" },
                    new SelectListItem() { Value = "C級", Text = "C級" },
                    new SelectListItem() { Value = "D級", Text = "D級" }
                };
            }
        }

        public QueryFormModel()
        {
            Parameters = new QueryParameters();
        }
    }
}
