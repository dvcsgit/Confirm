using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Models.PipelinePatrol.Construction
{
    public class QueryFormModel
    {
        public List<SelectListItem> ConstructionFirmSelectItemList { get; set; }

        public List<SelectListItem> ConstructionTypeSelectItemList { get; set; }

        public QueryParameters Parameters { get; set; }

        public QueryFormModel()
        {
            ConstructionFirmSelectItemList = new List<SelectListItem>();
            ConstructionTypeSelectItemList = new List<SelectListItem>();
            Parameters = new QueryParameters();
        }
    }
}
