using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;

namespace Models.ASE.AbnormalNotify
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
                    new SelectListItem() { Selected = false, Text = Resources.Resource.AbnormalNotifyStatus_0, Value = "0" },
                    new SelectListItem() { Selected = false, Text = Resources.Resource.AbnormalNotifyStatus_1, Value = "1" },
                    new SelectListItem() { Selected = false, Text = Resources.Resource.AbnormalNotifyStatus_2, Value = "2" },
                    new SelectListItem() { Selected = false, Text = Resources.Resource.AbnormalNotifyStatus_3, Value = "3" }
                };
            }
        }

        public QueryParameters Parameters { get; set; }

        public QueryFormModel()
        {
            Parameters = new QueryParameters();
        }
    }
}
