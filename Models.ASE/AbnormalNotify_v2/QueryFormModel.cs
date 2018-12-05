using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;

namespace Models.ASE.AbnormalNotify_v2
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
                    new SelectListItem() { Selected = false, Text = "未接案", Value = "1" },
                    new SelectListItem() { Selected = false, Text = "已接案未處置", Value = "2" },
                    new SelectListItem() { Selected = false, Text = "呈核中(已即時處理)", Value = "3" },
                    new SelectListItem() { Selected = false, Text = "呈核中(提報預計完成日)", Value = "4" },
                    new SelectListItem() { Selected = false, Text = "待處置", Value = "5" },
                    new SelectListItem() { Selected = false, Text = "呈核中(處理完成)", Value = "6" },
                    new SelectListItem() { Selected = false, Text = "已結案", Value = "7" }
                };
            }
        }

        public List<SelectListItem> ResponsibleOrganizationSelectItemList { get; set; }

        public QueryParameters Parameters { get; set; }

        public QueryFormModel()
        {
            Parameters = new QueryParameters();
            ResponsibleOrganizationSelectItemList = new List<SelectListItem>();
        }
    }
}
