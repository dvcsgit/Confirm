using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Models.ASE.QA.ChangeForm
{
    public class QueryFormModel
    {
        public List<SelectListItem> StatusSelectItemList { get; set; }

        public List<SelectListItem> ChangeTypeSelectItemList { get; set; }

        public QueryParameters Parameters { get; set; }

        public QueryFormModel()
        {
            StatusSelectItemList = new List<SelectListItem>();
            ChangeTypeSelectItemList = new List<SelectListItem>();
            Parameters = new QueryParameters();
        }
    }
}
