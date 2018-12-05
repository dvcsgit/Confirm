using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Models.ASE.QA.AbnormalForm
{
    public class QueryFormModel
    {
        public Dictionary<string, string> StatusList
        {
            get
            {
                return new Dictionary<string, string>() 
                { 
                    { "1", Resources.Resource.AbnormalFormStatus_1 },
                    { "2", Resources.Resource.AbnormalFormStatus_2 },
                    { "3", Resources.Resource.AbnormalFormStatus_3 },
                    { "4", Resources.Resource.AbnormalFormStatus_4 }
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
