using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;

namespace Models.ASE.QA.EquipmentManagement
{
    public class QueryFormModel
    {
        public Dictionary<string, string> StatusList
        {
            get
            {
                return new Dictionary<string, string>() 
                { 
                    { "1", "正常" },
                    { "2", "逾期" },
                    { "3", "免校驗" },
                    { "4", "遺失" },
                    { "5", "報廢" },
                    { "6", "維修中" },
                    { "7", "庫存" }
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
