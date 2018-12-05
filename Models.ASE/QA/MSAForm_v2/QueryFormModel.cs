using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;

namespace Models.ASE.QA.MSAForm_v2
{
    public class QueryFormModel
    {
        public List<SelectListItem> StatusSelectItemList
        {
            get
            {
                return new List<SelectListItem>()
                {
                    Define.DefaultSelectListItem(Resources.Resource.SelectAll),
                    new SelectListItem() { Selected = false, Text = Resources.Resource.MSAFormStatus_1, Value = "1" },
                    new SelectListItem() { Selected = false, Text = Resources.Resource.MSAFormStatus_2, Value = "2" },
                    new SelectListItem() { Selected = false, Text = "簽核中", Value = "3" },
                    new SelectListItem() { Selected = false, Text = "退回修正", Value = "4" },
                    new SelectListItem() { Selected = false, Text = "已完成", Value = "5" },
                    new SelectListItem() { Selected = false, Text = "取消立案", Value = "6" }
                };
            }
        }

        public List<SelectListItem> FactorySelectItemList { get; set; }

        public QueryParameters Parameters { get; set; }

        public QueryFormModel()
        {
            FactorySelectItemList = new List<SelectListItem>();
            Parameters = new QueryParameters();
        }
    }
}
