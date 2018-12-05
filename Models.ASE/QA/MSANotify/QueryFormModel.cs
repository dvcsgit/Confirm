using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;

namespace Models.ASE.QA.MSANotify
{
    public class QueryFormModel
    {
        public Dictionary<string, string> StatusList
        {
            get
            {
                return new Dictionary<string, string>() 
                { 
                    { "1", "簽核中" },
                    { string.Format("0{0}2", Define.Seperator), "退回修正" },
                    { "3", "簽核完成" },
                    { "4", "取消立案" }
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
